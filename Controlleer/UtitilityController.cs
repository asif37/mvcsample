using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AgingInHome.Models;
using System.Globalization;
using Microsoft.AspNet.Identity;
using AgingInHome.Helpers;
using System.Configuration;
using System.Net.Mail;

namespace AgingInHome.Controllers
{
    public class UtitilityController : Controller
    {
        // GET: Utitility
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public bool Contactus(ContactUsModel _ContactUsModel)
        {
            _ContactUsModel.AppointmentDate = Convert.ToDateTime(_ContactUsModel.AppointmentDate);
            var userid = User.Identity.GetUserId();
            var message = new InboxModel() {
                IsActive = true,
                IsArchive = false,
                IsProviderArchive = false,
                IsRead=false,
                IsStarred=false,
                MessageBody= _ContactUsModel.CustomeMessageBody,
                RecipientId=_ContactUsModel.ProviderUserId,
                SenderId= userid,
                SentDate=DateTime.Now,
                Subject="Consumer wants your Services",
            };
            if (_ContactUsModel.IsCall == true)
            {
                message.MessageBody = " Please call me with more information <br/>" + message.MessageBody;
            }
                var ContactUsid = _ContactUsModel.addContactUsForm(_ContactUsModel);
            message.ContactUsId = ContactUsid;
            new InboxModel().SaveCustomeMessage(message);
            if (_ContactUsModel.AppointmentSchedule==true)
            {
                var ProviderEmail = new ProviderListingModel().GetListingByListingId(_ContactUsModel.ProviderListingId).PrimaryEmail;
                SerdEmail(ProviderEmail, _ContactUsModel);
                //create appointment 
                var model = new AppointmentModel();
                model.BestTime = _ContactUsModel.AppointmentTime;
                model.ConsumerId = userid;
                model.CreatedBy = userid;
                model.CreatedDate = DateTime.Now;
                model.ProviderId = _ContactUsModel.ProviderUserId;
                model.ServiceDate = _ContactUsModel.AppointmentDate;
                model.ContactUsId = ContactUsid;
                model.SubmitAppointment(model, "Consumer");
            }
          
             return true;
        }
        [HttpPost]
        public bool Rating(RatingModal _RatingModal)
        {
            var userid = User.Identity.GetUserId();
            new AppointmentModel().ChangeAppointmentStatus(userid, _RatingModal.ProviderId);
            return _RatingModal.AddRating(_RatingModal);
        }
        [HttpPost]
        public bool DontRatign(string ProviderId)
        {
            var userid = User.Identity.GetUserId();
            new AppointmentModel().ChangeAppointmentStatus(userid,ProviderId);
            return true;
        }
        
        public ActionResult intUit()
        {
            return View();
        }
        public void SerdEmail(string ProviderEmail, ContactUsModel contactUsModel)
        {
            var root = ConfigurationManager.AppSettings["root"];
            MailMessage mail = new MailMessage();
            mail.To.Add("usama.eteksol@gmail.com");
            //mail.To.Add(ProviderEmail);
            mail.From = new MailAddress(ConfigurationManager.AppSettings["AdminEmail"]);
            mail.Subject = "Service Request";
            mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.Body = "<table border='1' rules='all' width='600'><tr><th>Consumer Name:</th><td>" + contactUsModel.FirstName + " " + contactUsModel.LastName + "</td></tr><tr><th>Service Date</th><td>" + contactUsModel.AppointmentDate + "</td></tr><tr><th>Best Time</th><td>" + contactUsModel.AppointmentTime;
            mail.Body += "</td></tr><tr><th>Invitation Options</th><td style='height: 40px;'>";
            //mail.Body += "Consumer wants your services in "+serviceRequestModel.City+" City With Zipcode "+serviceRequestModel.ZipCode+" on  "+serviceRequestModel.ServiceDate+ "<br/><br/>";
            //mail.Body += "<a style='margin-right: 3px; padding: 7px;background-color:darkgreen; color: white' href='" + root + "/Consumer/ProviderInvatationStatus?RequestId=" + serviceRequestModel.Id + "&ProviderListingId=" + listingmodel.ProviderListingId + "&Status=true" + "'>Accept Invitation</a>";
            //mail.Body += "<a style='margin-right: 3px; padding: 7px;background-color:red; color: white' href='" + root + "/Consumer/ProviderInvatationStatus?RequestId=" + serviceRequestModel.Id + "&ProviderListingId=" + listingmodel.ProviderListingId + "&Status=false" + "'>Cancel Invitation</a>";
            //mail.Body += "<a style='margin-right: 3px; padding: 7px;background-color:blue; color: white' href='" + root + "/Consumer/ProviderInvatationStatus?RequestId=" + serviceRequestModel.Id + "&ProviderListingId=" + listingmodel.ProviderListingId + "&Status=true&alter=1" + "'>Alternative Offer</a></td></tr></table>";
            //mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.High;
            SmtpClient client = new SmtpClient();
            client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["AdminEmail"], ConfigurationManager.AppSettings["Password"]);
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Send(mail);
        }
        public bool SaveCustomeMessage(InboxModel message)
        {

            message.IsActive = true;
            message.IsArchive = false;
            message.IsProviderArchive = false;
            message.IsRead = false;
            message.IsStarred = false;
            message.SentDate = DateTime.Now;
            new InboxModel().SaveCustomeMessage(message);
            return true;

        }


    }
}