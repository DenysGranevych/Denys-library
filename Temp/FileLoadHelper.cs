using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using CMMD.DAL.UnitOfWork;
using CMMD.Model.DBContext;
using CMMD.Model.Entities;
using CMMD.Model.Entities.ImportLog;
using CMMD.Model.Enums;
using CMMD.Models;

namespace CMMD.Helpers
{
    public static class FileLoadHelper
    {
        #region Private Fields

        private static readonly IUnitOfWork _unitOfWork = new UnitOfWork();

        #endregion

        public static bool HasFile(this HttpPostedFileBase file)
        {
            return (file != null && file.ContentLength > 0) ? true : false;
        }

        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }

        public static ClientsGroupingModel UpdateClients(List<Client> clients, Guid importLogId)
        {
            var clientsGroupingModel = new ClientsGroupingModel();

            using (var context = new BaseDbContext())
            {
                foreach (var client in clients)
                {
                    var originalClient = new Client();

                    context.Configuration.ProxyCreationEnabled = false;

                    originalClient =
                        context.Clients.SingleOrDefault(cl => cl.UserName == client.UserName && cl.Phone == client.Phone && cl.IsDeleted==false);

                    if (originalClient != null)
                    {
                        if (originalClient.IsDeleted == true)
                        {
                            continue;
                        }
                        else
                        {
                            client.Id = originalClient.Id;

                            var clientsWithIncrementedDreams = UpdatedClientDreamsModel.ClientViewModelConverter(client,
                                originalClient.Dreams);

                            //check dream
                            if (client.Dreams > originalClient.Dreams &&
                                !clientsGroupingModel.ClientsWithIncrementedDreams.Contains(clientsWithIncrementedDreams))
                            {
                                clientsGroupingModel.ClientsWithIncrementedDreams.Add(clientsWithIncrementedDreams);
                            }
                            //check if dream changed from 0 to 1
                            if (client.Dreams == 1 && originalClient.Dreams == 0 &&
                                !clientsGroupingModel.FirstDreamClients.Contains(client))
                            {
                                clientsGroupingModel.FirstDreamClients.Add(client);
                            }

                            //change updateDate if Dreams changed
                            if (client.Dreams != originalClient.Dreams)
                            {
                                client.UpdatedDate = DateTime.UtcNow;
                            }
                            else
                            {
                                client.UpdatedDate = originalClient.UpdatedDate;
                            }
                            client.IsSentReminderSms = originalClient.IsSentReminderSms;

                            // update scalar properties(save in DB)
                            var parentEntry = context.Entry(originalClient);
                            parentEntry.CurrentValues.SetValues(client);
                           
                            clientsGroupingModel.UpdatedClients.Add(client);// not need
                        }
                    }
                    else
                    {
                        //add new client
                        var newClient = new Client()
                        {
                            Id = client.Id,
                            UserName = client.UserName,
                            FirstName = client.FirstName,
                            LastName = client.LastName,
                            Country = client.Country,
                            RegistrationDate = client.RegistrationDate,
                            Phone = client.Phone,
                            Dreams = client.Dreams,
                            Email = client.Email,
                            UpdatedDate = DateTime.UtcNow,
                            LoadedDate = DateTime.UtcNow,
                            IsSentReminderSms = false,
                            IsDeleted = false
                        };
                        clientsGroupingModel.NewClients.Add(newClient);
                    }
                }

                // add deleted clients
                foreach (var originalClient in context.Clients.ToList()
                    .Where(originalClient => clients.All(c => c.UserName != originalClient.UserName)))
                {
                    if (originalClient.IsDeleted == false)
                    {
                        clientsGroupingModel.DeletedClients.Add(originalClient);
                    }
                }
                AddImportLogClientUpdate(clientsGroupingModel.UpdatedClients);
                context.SaveChanges();
                _unitOfWork.ImportLogClientRepository.Save();//можливо винести з метода для швидкодії
            }
            return UpdateClientsToDelete(clientsGroupingModel);
        }

        public static ClientsGroupingModel UpdateClientsToDelete(ClientsGroupingModel clients)
        {
            var clientsGroupingModel = new ClientsGroupingModel
            {
                UpdatedClients = new List<Client>(),//new list
                ClientsWithIncrementedDreams = clients.ClientsWithIncrementedDreams,
                FirstDreamClients = clients.FirstDreamClients,
            };

            using (var context = new BaseDbContext())
            {
                foreach (var client in clients.NewClients)
                {
                    var originalClient = new Client();

                    context.Configuration.ProxyCreationEnabled = false;

                    originalClient =
                        clients.DeletedClients.SingleOrDefault(cl => cl.Phone == client.Phone);

                    if (originalClient != null)
                    {
                        client.Id = originalClient.Id;

                        clientsGroupingModel.UpdatedClients.Add(client);

                        var clientsWithIncrementedDreams = UpdatedClientDreamsModel.ClientViewModelConverter(client,
                            originalClient.Dreams);

                        //check dream
                        if (client.Dreams > originalClient.Dreams &&
                            !clientsGroupingModel.ClientsWithIncrementedDreams.Contains(clientsWithIncrementedDreams))
                        {
                            clientsGroupingModel.ClientsWithIncrementedDreams.Add(clientsWithIncrementedDreams);
                        }
                        //check if dream changed from 0 to 1
                        if (client.Dreams == 1 && originalClient.Dreams == 0 &&
                            !clientsGroupingModel.FirstDreamClients.Contains(client))
                        {
                            clientsGroupingModel.FirstDreamClients.Add(client);
                        }
                    }
                    else
                    {
                        // add new client
                        var newClient = new Client()
                        {
                            Id = client.Id,
                            UserName = client.UserName,
                            FirstName = client.FirstName,
                            LastName = client.LastName,
                            Country = client.Country,
                            RegistrationDate = client.RegistrationDate,
                            Phone = client.Phone,
                            Dreams = client.Dreams,
                            Email = client.Email,
                            UpdatedDate = DateTime.UtcNow,
                            LoadedDate = DateTime.UtcNow,
                            IsSentReminderSms = false
                        };
                        clientsGroupingModel.NewClients.Add(newClient);
                    }
                }
                // add deleted clients
                foreach (var originalClient in clients.DeletedClients.ToList()
                    .Where(originalClient => clients.NewClients.All(c => c.Phone != originalClient.Phone)))
                {
                    if (originalClient.IsDeleted == false)
                    {
                        clientsGroupingModel.DeletedClients.Add(originalClient);
                    }
                }
                context.SaveChanges();
            }
            return clientsGroupingModel;
        }

        public static Guid AddImportLog()
        {
            var importLogId = Guid.NewGuid();

            var importLog = new ImportLog
            {
                Id = importLogId,
                ImportDate = DateTime.UtcNow
            };
            _unitOfWork.ImportLogRepository.Add(importLog);
            _unitOfWork.ImportLogRepository.Save();

            return importLogId;
        }

        public static void AddImportLogFile(Guid importLogId, string filePath)
        {
            var importLogFile = new ImportLogFile
            {
                Id = Guid.NewGuid(),
                ImportLogId = importLogId,
                FilePath = filePath
            };
            _unitOfWork.ImportLogFileRepository.Add(importLogFile);
            _unitOfWork.ImportLogFileRepository.Save();
        }

        public static void AddImportLogClientUpdate(IEnumerable<Client> clients)
        {
            var importLogId = _unitOfWork.ImportLogRepository.GetAll().OrderByDescending(x => x.ImportDate).First().Id;
            using (var context = new BaseDbContext())
            {
                foreach (var client in clients)
                {
                    var originalClient =
                    context.Clients.SingleOrDefault(cl => cl.Id == client.Id);
                    var parentEntry = context.Entry(originalClient);
                    parentEntry.CurrentValues.SetValues(client);

                    var nameProperties = parentEntry.CurrentValues.PropertyNames.Where(propertyName => parentEntry.Property(propertyName).IsModified).ToList();
                    var newComment = "";
                    foreach (var nameProperty in nameProperties)
                    {
                        var property = parentEntry.Property(nameProperty);
                        if (property.IsModified == true)
                        {
                            newComment += property.Name + " : " + property.OriginalValue + " -> " + property.CurrentValue + ",  ";
                        }
                    }
                    var importLogClient = new ImportLogClient
                    {
                        Id = Guid.NewGuid(),
                        ClientId = parentEntry.Entity.Id,
                        ImportLogId = importLogId,
                        Action = ActiontImportLogType.EditedClient,
                        Comments = newComment
                    };
                    _unitOfWork.ImportLogClientRepository.Add(importLogClient);
                }
            }
            _unitOfWork.ImportLogClientRepository.Save();
            _unitOfWork.ClientRepository.Save();
        }

        public static void AddImportLogClientCreate(IEnumerable<Client> clients)
        {
            var importLogId = _unitOfWork.ImportLogRepository.GetAll().OrderByDescending(x => x.ImportDate).First().Id;
            foreach (var client in clients)
            {
                var newComment = "";
                foreach (PropertyInfo propertyInfo in client.GetType().GetProperties())
                {
                    if (!propertyInfo.PropertyType.IsGenericType)
                    {
                        newComment += propertyInfo.Name + " : " + propertyInfo.GetValue(client, null) + ",  ";//??SentSMS,ClientComment
                    }
                }
                var importLogClient = new ImportLogClient
                {
                    Id = Guid.NewGuid(),
                    ClientId = client.Id,
                    ImportLogId = importLogId,
                    Action = ActiontImportLogType.NewClient,
                    Comments = newComment
                };
                _unitOfWork.ImportLogClientRepository.Add(importLogClient);

            }
            _unitOfWork.ImportLogClientRepository.Save();
        }

        public static void AddImportLogClientDelete(IEnumerable<Client> clients)
        {
            var importLogId = _unitOfWork.ImportLogRepository.GetAll().OrderByDescending(x => x.ImportDate).First().Id;
                foreach (var client in clients)
                {
                    var newComment = "";
                    foreach (PropertyInfo propertyInfo in client.GetType().GetProperties())
                    {
                        if (!propertyInfo.PropertyType.IsGenericType)
                        {
                            newComment += propertyInfo.Name + " : " + propertyInfo.GetValue(client, null) + ",  ";//??SentSMS,ClientComment
                        }
                    }
                    var importLogClient = new ImportLogClient
                    {
                        Id = Guid.NewGuid(),
                        ClientId = client.Id,
                        ImportLogId = importLogId,
                        Action = ActiontImportLogType.DeletedClient,
                        Comments = newComment
                    };
                    _unitOfWork.ImportLogClientRepository.Add(importLogClient);
                }
            _unitOfWork.ImportLogClientRepository.Save();
            _unitOfWork.ClientRepository.Save();
        }

        public static void AddImportLogComment(IEnumerable<ClientComment> comments)
        {
            var importLogId = _unitOfWork.ImportLogRepository.GetAll().OrderByDescending(x => x.ImportDate).First().Id;
            foreach (var comment in comments)
            {
                var importLogComment = new ImportLogComment()
                {
                    Id = Guid.NewGuid(),
                    ImportLogId = importLogId,
                    CommentId = comment.Id
                };
                _unitOfWork.ImportLogCommentRepository.Add(importLogComment);
            }
            _unitOfWork.ImportLogCommentRepository.Save();
        }

        public static void AddImportLogSms(IEnumerable<Guid> smsGuids)
        {
            var importLogId = _unitOfWork.ImportLogRepository.GetAll().OrderByDescending(x => x.ImportDate).First().Id;
            foreach (var sms in smsGuids)
            {
                var importLogComment = new ImportLogSms()
                {
                    Id = Guid.NewGuid(),
                    ImportLogId = importLogId,
                    SentSmsId = sms
                };
                _unitOfWork.ImportLogSmsRepository.Add(importLogComment);
            }
            _unitOfWork.ImportLogSmsRepository.Save();
        }
    }
}