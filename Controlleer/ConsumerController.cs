using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AgingInHome.Models;
using Microsoft.AspNet.Identity;
using AgingInHome.Helpers;
using AgingInHome.BLL.Enums;
using System.Security.Claims;
using AgingInHome.DAL;

namespace AgingInHome.Controllers
{
    public class ConsumerController : Controller
    {
        [Authorize(Roles ="Consumer")]
        public ActionResult Dashboard()
        {
            string host = Request.Url.Host.ToLower();
            var userid = User.Identity.GetUserId();
            var Servicerequest = new ProviderListingModel().GetServiceRequestByUserId(User.Identity.GetUserId()).ToList();
            ViewBag.CustomeMessages = new InboxModel().GetAllCustomeMessagesByUserid(userid).GroupBy(p => p.ConversationId)
                                   .Select(s => new CustomeMessagesModel { AllMessages = s.ToList() }
                                   ).ToList();
            ViewBag.alllisting = new ProviderListingModel().GetAllListing();

            return View(Servicerequest);
        }
        public ActionResult ServiceRequest(int zipcode=0)
        {
            ViewBag.StateList = new SelectList(new ServicesModel().GetUsStates(), "Id", "State", "Select the State");
            ViewBag.categoryList = new ProviderListingModel().AllCategory();
            ViewBag.zipcode = zipcode;
            var model = new ServiceRequestModel();
            return View();
        }
        [HttpPost]
        public ActionResult ServiceRequest(ServiceRequestModel serviceRequestModel)
        {
            if (ModelState.IsValid)
            {

            }
            ViewBag.StateList = new SelectList(new ServicesModel().GetUsStates(), "Id", "State", "Select the State");
            ViewBag.categoryList = new ProviderListingModel().AllCategory();
            var model = new ServiceRequestModel();
            return View();
        }
        [HttpPost]
        public string AddServiceRequest(ServiceRequestModel serviceRequestModel)
        {
            if (serviceRequestModel.IsDirectContact)
            {

                var url = new ServiceRequestModel().AddServiceRequest(serviceRequestModel);
                return url.Split(',')[0];

            }
            else
            {
                var url = new ServiceRequestModel().AddServiceRequest(serviceRequestModel);
                var listinglist = new ProviderListingModel().GetAllListing()
                    .Where(o => serviceRequestModel.SelectedCategory.Contains(o.ProviderCategory1.Id.ToString()) && o.IsApproved == (int)ListingStatus.Accepted).ToList();
                //Get service RequestId
                var RequestId = url.Split(',')[1];
                serviceRequestModel.Id = Convert.ToInt32(RequestId);
                foreach (var providerlisting in listinglist)
                {
                    EmailSender.SendEmailToServiceProvider(providerlisting, serviceRequestModel);
                }

                return "Dashboard";
            }
        }
        public ActionResult SuccessServiceRequest()
        {
            return View();
        }
        public ActionResult IsWait(int RequiredListingId, bool Iswait)
        {
            return View();
        }
        public ActionResult ServiceRequestDetail(int RequestId)
        {
            var Servicerequest = new ProviderListingModel().GetServiceRequestByUserId(User.Identity.GetUserId()).FirstOrDefault(d => d.Id == RequestId);

            return View(Servicerequest);
        }
        public ActionResult ServiceRequestDelete(int RequestId)
        {
            new ServiceRequestModel().DeleteServiceRequest(RequestId);

            return RedirectToAction("Dashboard");
        }
        public ActionResult UpdateServiceRequest(int RequestId)
        {
            ViewBag.StateList = new SelectList(new ServicesModel().GetUsStates(), "Id", "State", "Select the State");
            ViewBag.categoryList = new ProviderListingModel().AllCategory();
            var model = new ServiceRequestModel();
            var Servicerequest = new ProviderListingModel().GetServiceRequestByUserId(User.Identity.GetUserId()).FirstOrDefault(d => d.Id == RequestId);
            return View(Servicerequest);
        }
        [HttpPost]
        public string UpdateServiceRequest(ServiceRequestModel serviceRequestModel)
        {

            var url = new ServiceRequestModel().UpdateServiceRequest(serviceRequestModel);
            //var listinglist = new ProviderListingModel().GetAllListing()
            //    .Where(o => serviceRequestModel.SelectedCategory.Contains(o.ProviderCategory1.Id.ToString()) && o.IsApproved == (int)ListingStatus.Accepted).ToList();
            //foreach (var providerlisting in listinglist)
            //{
            //    EmailSender.SendEmailToServiceProvider(providerlisting.PrimaryEmail, serviceRequestModel);
            //}

            return "Dashboard";

        }
        public ActionResult ProviderInvatationStatus(int RequestId, int ProviderListingId, bool Status,int alter=0)
        {
            new ServiceRequestModel().UpdateInvitationStatus(RequestId, ProviderListingId, Status, alter);
            return RedirectToAction("Dashboard", "ProviderListing");
        }
        public PartialViewResult GetMessageDetail(int RequestId)
        {
            var model = new InboxModel().GetMessageDetailByDetailId(RequestId, User.Identity.GetUserId());

            ViewBag.alllisting = new ProviderListingModel().GetAllListing();
            ViewBag.msgType = "all";
            return PartialView("_MessageDetailPartial", model.OrderByDescending(d => d.Id).ToList());
        }
        public PartialViewResult GetCustomMessageDetail(int ConversationId)
         {
            var model = new InboxModel().GetMessageDetailByConversationId(ConversationId, User.Identity.GetUserId());
            if (User.IsInRole("Provider"))
            {
                model = model.Where(s => s.IsProviderArchive != true).ToList();
            }
            if (User.IsInRole("Admin"))
            {
                model = model.Where(s => s.isAdminArchive != true).ToList();
            }
            if (User.IsInRole("Consumer"))
            {
                model = model.Where(s => s.IsArchive != true).ToList();
            }
            ViewBag.alllisting = new ProviderListingModel().GetAllListing();
            ViewBag.msgType = "all";
            return PartialView("_CustomeMessageDetail", model.OrderByDescending(d => d.Id).ToList());
        }
        public PartialViewResult GetinboxDetail()
        {
            var userid = User.Identity.GetUserId();
            var Servicerequest = new ProviderListingModel().GetServiceRequestByUserId(userid).ToList();
            ViewBag.CustomeMessages = new InboxModel().GetAllCustomeMessagesByUserid(userid).GroupBy(p => p.ConversationId)
                                    .Select(s => new CustomeMessagesModel { AllMessages = s.ToList() }
                                    ).ToList();
            ViewBag.alllisting = new ProviderListingModel().GetAllListing();
            return PartialView("_inboxMessageDetail", Servicerequest);
        }

        public PartialViewResult SentMessageDetail(int RequestId,string type= "")
        {
            var model = new InboxModel().GetSendMessageDetailByDetailId(RequestId, User.Identity.GetUserId(),type);
            ViewBag.alllisting = new ProviderListingModel().GetAllListing();
            ViewBag.msgType = "Sent";
            return PartialView("_MessageDetailPartial", model.OrderByDescending(d => d.Id).ToList());
        }
        public PartialViewResult RecMessageDetail(int RequestId)
        {
            var model = new InboxModel().GetMessageDetailByDetailId(RequestId, User.Identity.GetUserId());
            ViewBag.alllisting = new ProviderListingModel().GetAllListing();
            ViewBag.msgType = "Recieved";
            return PartialView("_MessageDetailPartial", model.OrderByDescending(d => d.Id).ToList());
        }
        public PartialViewResult ArchiMessageDetail(int RequestId)
        {
            var model = new InboxModel().GetMessageDetailByDetailId(RequestId, User.Identity.GetUserId());
            ViewBag.alllisting = new ProviderListingModel().GetAllListing();
            ViewBag.msgType = "Archive";
            return PartialView("_MessageDetailPartial", model.OrderByDescending(d => d.Id).ToList());
        }
        public bool UpdateAchieveStatus(int RequestId)
        {
            var userIdentity = (ClaimsIdentity)User.Identity;
            var claims = userIdentity.Claims;
            var roleClaimType = userIdentity.RoleClaimType;
            var roles = claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            var model = false;
            if (roles[0].Value == "Consumer")
            {
                model = new InboxModel().UpdateAchieveStatus(RequestId);
                model = true;
            }
            if (roles[0].Value == "Provider")
            {
                model = new InboxModel().UpdateAchieveStatusByProvider(RequestId);
                model = false;
            }

            return model;
        }

        public bool SaveMessage(InboxModel Message)
        {
            Message.SaveMessage(Message);
            return true;
        }
        public PartialViewResult LoadServiceDateAndBestTime()
        {
            return PartialView("_ServiceDateandBestitme");
        }
        [HttpPost]
        public bool ServiceRequestDetails(List<ServiceSelectedCatDetails> ServiceRequest)
        {
            var userid = User.Identity.GetUserId();
            var UserDetails = new AgingInHomeContext().ConsumerProfiles.FirstOrDefault(d => d.UserId == userid);
            var url = new ServiceRequestModel().SubmitServiceRequest(ServiceRequest, UserDetails);
            var providercatlistig = ServiceRequest.Select(d => d.catId.ToString());
            var listinglist = new ProviderListingModel().GetAllListing()
                    .Where(o => providercatlistig.Contains(o.ProviderCategory1.Id.ToString()) && o.IsApproved == (int)ListingStatus.Accepted).ToList();
            //Get service RequestId
            var RequestId = url.Split(',')[1];
            ServiceRequestModel serviceRequestModel = AutoMapper.Mapper.Map<ServiceRequestModel>(UserDetails);
            serviceRequestModel.Id = Convert.ToInt32(RequestId);
            serviceRequestModel.Email = UserDetails.AspNetUser.Email;
            foreach (var providerlisting in listinglist.OrderByDescending(s=>s.ProviderListingId).ToList())
            {
                //Get selected date time and best time
                var getselectedInfo = ServiceRequest.FirstOrDefault(s => s.catId == providerlisting.ProviderCategory1.Id);
                serviceRequestModel.ServiceDate =Convert.ToDateTime(getselectedInfo.CatserviceDate);
                serviceRequestModel.BestTime = getselectedInfo.CatBestTime;
                EmailSender.SendEmailToServiceProvider(providerlisting, serviceRequestModel);
                break;
            }
            return true;
        }
        [HttpPost]
        public bool UpdateServiceRequestDetails(List<ServiceSelectedCatDetails> ServiceRequest)
        {
            var userid = User.Identity.GetUserId();
            var UserDetails = new AgingInHomeContext().ConsumerProfiles.FirstOrDefault(d => d.UserId == userid);
            var returnProviderListingIds = new ServiceRequestModel().UpdateServiceRequest(ServiceRequest, UserDetails);
            if (returnProviderListingIds != "")
            {
                var providercatlistig = returnProviderListingIds.Substring(1).Split(',');
                var listinglist = new ProviderListingModel().GetAllListing();
                var listingForMail = new List<ProviderListingModel>();
                foreach (var listing in listinglist)
                {
                    if (providercatlistig.Contains(listing.CategoryId.ToString()))
                    {
                        listingForMail.Add(listing);
                    }

                }
                //.Where(o => providercatlistig.Contains(o.ProviderCategory1.Id.ToString()) && o.IsApproved == (int)ListingStatus.Accepted).ToList();
                //Get service RequestId
                var RequestId = ServiceRequest.First().ServiceRequestId;
                ServiceRequestModel serviceRequestModel = AutoMapper.Mapper.Map<ServiceRequestModel>(UserDetails);
                serviceRequestModel.Id = Convert.ToInt32(RequestId);
                serviceRequestModel.Email = UserDetails.AspNetUser.Email;
                foreach (var providerlisting in listingForMail)
                {
                    if (providerlisting.PrimaryEmail.Contains("@"))
                    {
                        //Get selected date time and best time
                        var getselectedInfo = ServiceRequest.FirstOrDefault(s => s.catId == providerlisting.ProviderCategory1.Id);
                        if (getselectedInfo!=null)
                        {
                            serviceRequestModel.ServiceDate = Convert.ToDateTime(getselectedInfo.CatserviceDate);
                            serviceRequestModel.BestTime = getselectedInfo.CatBestTime;
                            EmailSender.SendEmailToServiceProvider(providerlisting, serviceRequestModel);
                        }
                        

                    }
                }
            }
            return true;
        }
        public ActionResult CancelCategoryRequest(int CatId)
        {
            new ServiceRequestModel().CancelCategoryRequest(CatId);
            return RedirectToAction("Dashboard");
        }
        public bool MakeAppointment(AppointmentModel model)
        {
            model.SubmitAppointment(model,"Consumer");
            return true;
        }
        public bool MakeCustomeAppointment(AppointmentModel model)
        {
            model.SubmitCustomeAppointment(model, "Consumer");
            return true;
        }
        
        public PartialViewResult GetAllConsumerAppointments()
        {
            var userid = User.Identity.GetUserId();
            var GetAllConsumerAppointment = new AppointmentModel().GetAllConsumerAppointment(userid).OrderByDescending(d=>d.AppointmentId).ToList();
            return PartialView(GetAllConsumerAppointment);
        }
        public bool CancelAppointment(int AppointId)
        {
            return new AppointmentModel().CancelAppointment(AppointId);
        }
        public bool AcceptAppointment(int AppointId,string UserType)
        {
            return new AppointmentModel().AcceptAppointment(AppointId, UserType);
        }
        public bool RejectAppointment(int AppointId, string UserType)
        {
            return new AppointmentModel().RejectAppointment(AppointId, UserType);
        }





    }
}