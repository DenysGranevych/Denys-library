using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Mvc;
using CMMD.DataSenders;
using CMMD.DAL.UnitOfWork;
using CMMD.Enums;
using CMMD.Helpers;
using CMMD.Model.Entities;
using CMMD.Models;
using CMMD.Services;
using LinqToExcel;

namespace CMMD.Controllers
{
    [System.Web.Mvc.Authorize]
    public class ClientsController : Controller
    {
        #region Private Fields

        private  readonly IUnitOfWork _unitOfWork;
        private ISmsSender _smsSender;
        
        #endregion

        #region Constructors

        public ClientsController()
        {
            _unitOfWork = new UnitOfWork();
        }

        public ClientsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #endregion

        #region Public Method

        // GET: Clients
        [System.Web.Mvc.Authorize]
        [System.Web.Mvc.HttpGet]
        public ActionResult Clients()
        {
            Session["nav_bar"] = "nav_clients";

            var allCountries = _unitOfWork.CountryRepository.GetAll().ToList();

            var countries = new List<SelectListItem>();
            foreach (var country in allCountries)
            {
                countries.Add(new SelectListItem
                {
                    Value = country.Name,
                    Text = country.Name,
                });
            }
            if (countries == null || !countries.Any())
            {
                var defaultCountry =
                    _unitOfWork.SystemConstantsRepository.GetConstantByName<string>(Resource.DefaultCountry);

                countries.Add(new SelectListItem
                {
                    Value = defaultCountry,
                    Text = defaultCountry,
                });
            }

            ViewBag.Countries = countries;

            List<ClientTableModel> clientsToDelete = new List<ClientTableModel>();
            List<UpdatedClientDreamsModel> updatedClientDreamsModel = new List<UpdatedClientDreamsModel>();

            if (TempData["ClientToDelete"] != null || TempData["ClientsWithIncrementedDreams"]!=null)
            {
                clientsToDelete = (List<ClientTableModel>)TempData["ClientToDelete"];
                updatedClientDreamsModel = (List<UpdatedClientDreamsModel>)TempData["ClientsWithIncrementedDreams"];
            }

            var  generalClientViewModel=new GeneralClientViewModel();
            generalClientViewModel.ClientTableModel.AddRange(clientsToDelete);
            generalClientViewModel.ClientsWithIncrementedDreams.AddRange(updatedClientDreamsModel);

            return View(generalClientViewModel);
        }

        [System.Web.Mvc.Authorize]
        [System.Web.Mvc.HttpGet]
        public JsonResult GetClients([FromUri] JQueryDataTablesModel model)
        {
            var sortBy = model.mDataProp_[model.iSortCol_[0]];

            int recordsCount;
            var searchBy = model.sSearch;

            var result = _unitOfWork.ClientTableRepository.GetByPage((model.iDisplayStart/model.iDisplayLength) + 1,
                model.iDisplayLength, searchBy,
                (m =>
                    (m.UserName.Contains(searchBy)) || (m.FirstName.Contains(searchBy)) ||
                    (m.LastName.Contains(searchBy))
                    || (m.Country.Contains(searchBy)) || (m.Phone.Contains(searchBy)) || (m.Email.Contains(searchBy))),
                sortBy, model.sSortDir_[0],
                out recordsCount);


            var returnModel = result.Select(NewClientTableModel.ClientViewModelConverter).ToList();

            return
                Json(
                    new JQueryDataTablesResponse<NewClientTableModel>(returnModel.ToArray(), recordsCount, recordsCount,
                        model.sEcho), JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.Authorize]
        [System.Web.Mvc.HttpPost]
        public ActionResult LoadClients()
        {
            var clientToDelete = new List<ClientTableModel>();
            try
            {
                //create import log
              var importLogId =  FileLoadHelper.AddImportLog();//??
                foreach (string upload in Request.Files)
                {
                    //upload file
                    if (!Request.Files[upload].HasFile()) continue;
                    
                    string path = AppDomain.CurrentDomain.BaseDirectory;
                    string filename = Path.GetFileName(Request.Files[upload].FileName);
                    Request.Files[upload].SaveAs(Path.Combine(path, filename));
                   
                    //add importLogFile
                    FileLoadHelper.AddImportLogFile(importLogId, Path.Combine(path, filename));
                   
                    //get filtering constants
                    var clientSheetName =
                        _unitOfWork.SystemConstantsRepository.GetConstantByName<string>(Resource.ClientSheetName);
                    var clientPhoneStartWith =
                        _unitOfWork.SystemConstantsRepository.GetConstantByName<string>(Resource.ClientPhoneStartWith);
                    var defaultCountry =
                        _unitOfWork.SystemConstantsRepository.GetConstantByName<string>(Resource.DefaultCountry);
                    var clientPhoneLength =
                        _unitOfWork.SystemConstantsRepository.GetConstantByName<string>(Resource.ClientPhoneLength);


                    //read clients info from file
                    var excel = new ExcelQueryFactory();
                    excel.FileName = path + filename;

                    var allClients = from client in excel.Worksheet<ClientExcelModel>(clientSheetName)
                        where client.country == defaultCountry
                              && client.phone != null && client.phone.StartsWith(clientPhoneStartWith)
                        select client;

                    var clientsList = allClients.ToList();
                    var resultClientList = new List<Client>();

                    //add clients to resultList (filtering clients)
                    foreach (var client in clientsList)
                    {
                        if (Regex.IsMatch(client.phone, @"^\d{" + clientPhoneLength + "}$") &&
                            !resultClientList.Where(t => t.UserName == client.username && t.Phone == client.phone).Any())
                        {
                            var newClient = new Client
                            {
                                Id = Guid.NewGuid(),
                                UserName = client.username,
                                FirstName = FileLoadHelper.FirstCharToUpper(client.first_name),
                                LastName = FileLoadHelper.FirstCharToUpper(client.last_name),
                                Country = client.country,
                                RegistrationDate = client.registration_date,
                                Phone = client.phone,
                                Dreams = client.number_dreams,
                                Email = client.email,
                                UpdatedDate = DateTime.UtcNow,
                                LoadedDate = DateTime.UtcNow,
                                IsSentReminderSms = false
                            };
                            resultClientList.Add(newClient);
                        }
                    }

                    // add client to DB and update
                    var updatedClients = FileLoadHelper.UpdateClients(resultClientList, importLogId);

                    var isSMSSendingEnabled = Convert.ToBoolean(Session["isSMSSendingEnabled"]);

                    // add new client
                    AddNewClients(updatedClients.NewClients, isSMSSendingEnabled);
                    //update client with changed UserName
                    UpdateClients(updatedClients.UpdatedClients);
                    //send first dreams sms
                    if (updatedClients.FirstDreamClients.Any())
                    {
                        //send sms
                        _smsSender = new FirstDreamsSMSSender();
                        _smsSender.SendSms(isSMSSendingEnabled, updatedClients.FirstDreamClients, new Guid(), string.Empty);
                    }

                    foreach (var client in updatedClients.DeletedClients)
                    {
                        var convertedClient = ClientTableModel.ClientViewModelConverter(client);
                        clientToDelete.Add(convertedClient);
                    }

                    TempData["ClientToDelete"] = clientToDelete;
                    TempData["ClientsWithIncrementedDreams"] = updatedClients.ClientsWithIncrementedDreams;
                }
            }
            catch (Exception exception)
            {

                throw;
            }
            return RedirectToAction("Clients");
        }


        [System.Web.Mvc.Authorize]
        public ActionResult DeleteClient(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                return RedirectToAction("Clients");
            }
            var client = _unitOfWork.ClientRepository.FindBy(t => t.Id == new Guid(clientId)).FirstOrDefault();

            if (client == null)
            {
                return RedirectToAction("Clients");
            }

           // DeleteClientSMS(new Guid(clientId));
            client.IsDeleted = true;
            _unitOfWork.ClientRepository.Edit(client);
            _unitOfWork.ClientRepository.Save();
            FileLoadHelper.AddImportLogClientDelete(new List<Client>(){client});
            return RedirectToAction("Clients");
        }

        [System.Web.Mvc.Authorize]
        public ActionResult DeleteMultipleClient(string[] clientsId)
        {
            var arrayClientForLog = new List<Client>();
            if (clientsId == null)
            {
                return RedirectToAction("Clients");
            }
            foreach (var clientId in clientsId)
            {
                if (string.IsNullOrEmpty(clientId))
                {
                    return RedirectToAction("Clients");
                }
                var client = _unitOfWork.ClientRepository.FindBy(t => t.Id == new Guid(clientId)).FirstOrDefault();

                if (client == null)
                {
                    return RedirectToAction("Clients");
                }
                 arrayClientForLog.Add(_unitOfWork.ClientRepository.GetByID(new Guid(clientId)));
              //  DeleteClientSMS(client.Id);
                client.IsDeleted = true;
                _unitOfWork.ClientRepository.Edit(client);
                _unitOfWork.ClientRepository.Save();
            }
            FileLoadHelper.AddImportLogClientDelete(arrayClientForLog);
            return RedirectToAction("Clients");
        }

        [System.Web.Http.Authorize]
        [System.Web.Mvc.HttpGet]
        public JsonResult EditClient(string clientId)
        {
            if (!string.IsNullOrEmpty(clientId))
            {
                var baseClient = _unitOfWork.ClientRepository.FindBy(t => t.Id == new Guid(clientId)).FirstOrDefault();

                if (baseClient != null)
                {
                    return Json(NewClientTableModel.ClientViewModelConverter(ClientTableModel.ClientViewModelConverter(baseClient)), JsonRequestBehavior.AllowGet);
                }
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Http.Authorize]
        [System.Web.Mvc.HttpPost]
        public ActionResult EditClient(Client client)
        {
            if (client != null)
            {
                _unitOfWork.ClientRepository.Edit(client);
                _unitOfWork.ClientRepository.Save();
                FileLoadHelper.AddImportLogClientUpdate(new List<Client>(){client});
                return RedirectToAction("Clients");

            }
            return RedirectToAction("Clients");
        }

        [System.Web.Http.Authorize]
        [System.Web.Mvc.HttpPost]
        public JsonResult CommentClient(string clientId, string commentText)
        {
            if (!string.IsNullOrEmpty(clientId))
            {
                var baseClient = _unitOfWork.ClientRepository.FindBy(t => t.Id == new Guid(clientId)).FirstOrDefault();
                if (baseClient != null)
                {
                    var newComment = new ClientComment
                    {
                        Id = Guid.NewGuid(),
                        ClientId = new Guid(clientId),
                        Comment = commentText
                    };
                    _unitOfWork.ClientCommentRepository.Add(newComment);
                    _unitOfWork.ClientCommentRepository.Save();
                    FileLoadHelper.AddImportLogComment(new List<ClientComment>() { newComment });
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Http.Authorize]
        [System.Web.Mvc.HttpPost]
        public JsonResult CommentMultipleClient(string[] clientsId, string commentText)
        {
            var arrayClientCommentForLog = new List<ClientComment>();
            foreach (var clientId in clientsId)
            {
                if (!string.IsNullOrEmpty(clientId))
                {
                    var baseClient =
                        _unitOfWork.ClientRepository.FindBy(t => t.Id == new Guid(clientId)).FirstOrDefault();
                    if (baseClient != null)
                    {
                        var newComment = new ClientComment
                        {
                            Id = Guid.NewGuid(),
                            ClientId = new Guid(clientId),
                            Comment = commentText
                        };
                        arrayClientCommentForLog.Add(newComment);
                        _unitOfWork.ClientCommentRepository.Add(newComment);
                        _unitOfWork.ClientCommentRepository.Save();
                    }
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            FileLoadHelper.AddImportLogComment(arrayClientCommentForLog);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Http.Authorize]
        [System.Web.Mvc.HttpGet]
        public JsonResult GetAllTemplates()
        {
            var allTemplates = _unitOfWork.SMSTemplateRepository.FindBy(t=>t.IsDeleted==false).ToList();

            var templates = new Dictionary<string, string[]>();

            foreach (var temlate in allTemplates)
            {
                var templateInfo = new string[]
                {
                    temlate.Name, temlate.TemplateBody
                };
                templates.Add(temlate.Id.ToString(), templateInfo);
            }

            return Json(templates, JsonRequestBehavior.AllowGet);
        }


        [System.Web.Http.Authorize]
        [System.Web.Mvc.HttpPost]
        public JsonResult SendCustomSMSClient(string clientId, string smsText, bool isSend)
        {
            if (!string.IsNullOrEmpty(clientId))
            {
                var baseClient = _unitOfWork.ClientRepository.FindBy(t => t.Id == new Guid(clientId)).FirstOrDefault();
                if (baseClient != null)
                {

                    // send custom sms
                    //create clients list
                    var clients = new List<Client>();
                    clients.Add(baseClient);
                    //send sms 
                    _smsSender = new CustomSmsSender();
                    _smsSender.SendSms(isSend, clients, new Guid(), smsText);

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Http.Authorize]
        [System.Web.Mvc.HttpPost]
        public JsonResult SendCustomSMSMultipleClient(string[] clientsId, string smsText, bool isSend, SmsSenderType smsSenderType)
        {
            var clients = GetClientsToSendSms(smsSenderType, clientsId);

            // sent custom sms selected clients
            _smsSender = new CustomSmsSender();
            _smsSender.SendSms(isSend, clients, new Guid(), smsText);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Http.Authorize]
        [System.Web.Mvc.HttpGet]
        public JsonResult ViewClientComments(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            var clientComments =
                _unitOfWork.ClientCommentRepository.FindBy(t => t.ClientId == new Guid(clientId)).ToList();

            var result = clientComments.Select(comment => comment.Comment).ToList();

            return result.Any() ? Json(result, JsonRequestBehavior.AllowGet) : Json(false, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Http.Authorize]
        [System.Web.Mvc.HttpGet]
        public JsonResult ViewClientSentSMS(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            var sentSMS =
                _unitOfWork.SentSMSRepository.FindBy(t => t.ClientId == new Guid(clientId)&& t.SMSServiceResponce==true).ToList();

            foreach (var sms in sentSMS)
            {
                if (sms.SMSTemplateId != null)
                {
                    var template =
                        _unitOfWork.SMSTemplateRepository.FindBy(t => t.Id == sms.SMSTemplateId).FirstOrDefault();
                    var templateBody = template != null ? template.TemplateBody : string.Empty;

                    sms.CustomSMSBody = templateBody;
                }
            }

            var result = sentSMS.ToDictionary(sms => sms.SendedDate.ToString("G"), sms => sms.CustomSMSBody);

            return result.Any() ? Json(result, JsonRequestBehavior.AllowGet) : Json(false, JsonRequestBehavior.AllowGet);
        }


        [System.Web.Http.Authorize]
        [System.Web.Mvc.HttpPost]
        public JsonResult SentSmsClient(string clientId, string templateId, bool isSend)
        {
            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(templateId))
            {
                var baseClient = _unitOfWork.ClientRepository.FindBy(t => t.Id == new Guid(clientId)).FirstOrDefault();

                if (baseClient != null)
                {

                    // sent standard sms
                    //create clients list
                    var clients = new List<Client>();
                    clients.Add(baseClient);
                    //send sms 
                    _smsSender = new DefaultSmsSender();
                    _smsSender.SendSms(isSend, clients, new Guid(templateId), string.Empty);

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Http.Authorize]
        [System.Web.Mvc.HttpPost]
        public JsonResult SentSmsMultipleClient(string[] clientsId, string templateId, bool isSend, SmsSenderType smsSenderType)
        {
            var clients = GetClientsToSendSms(smsSenderType, clientsId);

            if (string.IsNullOrEmpty(templateId)) return Json(false, JsonRequestBehavior.AllowGet);
            // sent standard sms
            _smsSender = new DefaultSmsSender();
            _smsSender.SendSms(isSend, clients, new Guid(templateId), string.Empty);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Private Methods

        private List<Client> GetClientsToSendSms(SmsSenderType smsSenderType, string[] clientsId)
        {
            var clients = new List<Client>();

            switch (smsSenderType)
            {
                case SmsSenderType.AllClientsSms:
                    {
                        clients.AddRange(_unitOfWork.ClientRepository.GetAll().ToList());
                        break;
                    }
                case SmsSenderType.SmsWhereDreamsEqual_0:
                    {
                        clients.AddRange(_unitOfWork.ClientRepository.FindBy(t => t.Dreams == 0).ToList());
                        break;
                    }
                case SmsSenderType.SmsWhereDreamsOver_0:
                    {
                        clients.AddRange(_unitOfWork.ClientRepository.FindBy(t => t.Dreams > 0).ToList());
                        break;
                    }
                default:
                {
                    clients.AddRange(clientsId.Where(clientId => !string.IsNullOrEmpty(clientId)).Select(clientId => _unitOfWork.ClientRepository.FindBy(t => t.Id == new Guid(clientId)).FirstOrDefault()).Where(baseClient => baseClient != null));
                    break;
                }
            }
            return clients;
        }

        private void AddNewClients(List<Client> clients, bool isSendWelcomeSms)
        {
            try
            {
                //add new clients to db
                foreach (var client in clients)
                {
                    _unitOfWork.ClientRepository.Add(client);
                }
                _unitOfWork.ClientRepository.Save();
                FileLoadHelper.AddImportLogClientCreate(clients);
                if (clients.Any())
                {
                    //send sms
                    _smsSender = new WelcomeSmsSender();
                    _smsSender.SendSms(isSendWelcomeSms, clients, new Guid(), string.Empty);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void UpdateClients(List<Client> clients)
        {
            try
            {
                FileLoadHelper.AddImportLogClientUpdate(clients);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DeleteClientSMS(Guid clientId)
        {
            var clientSms = _unitOfWork.SentSMSRepository.FindBy(t => t.ClientId == clientId).ToList();
            foreach (var sms in clientSms)
            {
                _unitOfWork.SentSMSRepository.Delete(sms);
                _unitOfWork.SentSMSRepository.Save();
            }
        }

        #endregion
    }
}