using System;
using System.Collections.Generic;
using System.Linq;
using CMMD.Helpers;
using CMMD.Loggers;
using CMMD.Model.Entities;
using CMMD.Services;

namespace CMMD.DataSenders
{
    public class DefaultSmsSender : SmsSender
    {
        #region Protected Methods

        protected override void SendDefaultSms(bool isSend, List<Client> clients, Guid smsTemplateId)
        {
            AddDefaultSms(isSend, clients, smsTemplateId);
        }

        #endregion

        #region Private Methods

        private void AddDefaultSms(bool isSend, List<Client> clients, Guid smsTemplateId)
        {
            try
            {
                LogIt.Logger.WriteLogMessage(string.Format("Send default Sms to clients, send  Sms = " + isSend));
               
                //create sms service
                SmsService service = new SmsService();
                //check credit balance
                if (service.IsCreditBalance())
                {
                    var smsText = GetDefaultSmsText(smsTemplateId);
                    //send sms 
                    var isSent = false;
                    if (isSend)
                    {
                        isSent = service.SendClientsSms(clients, smsText);
                    }
                    SaveSentSms(clients, smsTemplateId, isSent);
                }
            }
            catch (Exception ex)
            {
                LogIt.Logger.WriteLogMessage(string.Format("EXCEPTION! Default Sms sender: " + ex.InnerException));
            }
        }

        private string GetDefaultSmsText(Guid smsTemplateId)
        {
            //get sms
            var template = _unitOfWork.SMSTemplateRepository.FindBy(t => t.Id == smsTemplateId).FirstOrDefault();
            return template != null ? template.TemplateBody : string.Empty;
        }

        private void SaveSentSms(List<Client> clients, Guid templateId, bool smsServiceResponce)
        {
            var smsIdCollection = new List<Guid>();
            foreach (var client in clients)
            {
                var newSentSms = new SentSMS
                {
                    Id = Guid.NewGuid(),
                    ClientId = client.Id,
                    SendedDate = DateTime.UtcNow,
                    SMSTemplateId = templateId,
                    SMSServiceResponce = smsServiceResponce
                };
                smsIdCollection.Add(newSentSms.Id);

                _unitOfWork.SentSMSRepository.Add(newSentSms);
                _unitOfWork.SentSMSRepository.Save();
            }
            FileLoadHelper.AddImportLogSms(smsIdCollection);
        }

        #endregion
    }
}