using AgingInHome.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Configuration;
using AgingInHome.Helpers;
using Microsoft.AspNet.Identity;
using PagedList;
using PagedList.Mvc;
using AgingInHome.BLL.Enums;
using AgingInHome.DAL;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Data;
using System.Xml;
using System.Security.Claims;
using System.Web.Security;

namespace AgingInHome.Controllers
{
    public class ProviderListingController : Controller
    {
        [Authorize(Roles = "Provider")]
        public ActionResult Dashboard()
        {
            var listing = new ProviderListingModel();
            var UserId = User.Identity.GetUserId();
            listing = listing.GetListingByUserId(UserId);
            ViewBag.alllisting = new ProviderListingModel().GetAllListing();
            listing.AvgRating = listing.AvgRatings(listing);
            ViewBag.allusers = new AgingInHomeContext().AspNetUsers.ToList();
            ViewBag.CustomeMessages = new InboxModel().GetAllCustomeMessagesByUserid(UserId).GroupBy(p=>p.ConversationId)
                                      .Select(s=> new CustomeMessagesModel {AllMessages=s.ToList()}
                                      ).ToList();
            return View(listing);
        }
        public ActionResult Index()
        {
            return View();
        }
        //[Authorize]
        public ActionResult SubmitListing(int ListingId = 0)
        {
            var providerListingModel = new ProviderListingModel();
            if (ListingId == 0)
            {
                ViewBag.Categorylist = new SelectList(providerListingModel.AllCategory(), "Id", "CategoryName", "Select the Category");
                //ViewBag.CategoryServicelist = new SelectList(providerListingModel.GetCategoryServices(1), "CategoryServiceId", "Name", "Select the Service");
                ViewBag.CategoryServicelist = providerListingModel.GetCategoryServices(1);

            }
            ViewBag.State = new SelectList(new ServicesModel().GetUsStates(), "Id", "State", "Select the State");
            return View(providerListingModel);
        }
        [HttpPost]
        public ActionResult SubmitListing(ProviderListingModel providerListingModel)
        {

            ModelState.Remove("LogoUrl"); ModelState.Remove("YearOfExperience");
            if (providerListingModel.ProviderListingId > 0)
            {
                ModelState.Remove("PrimaryEmail"); ModelState.Remove("Password"); ModelState.Remove("ConfirmPassword"); ModelState.Remove("imageUrl");
            }
            if (ModelState.IsValid)
            {
                if (providerListingModel.ProviderListingId == 0)
                {
                    //if (providerListingModel.Bitimagestring != null)
                    //{
                    //    providerListingModel.Logo = HelperClass.SaveImage64binarystring("~/Images/ProviderListingImages", providerListingModel.Bitimagestring);
                    //}
                    var _ProviderListingModel = new ProviderListingModel();
                    TempData["ListingId"] = _ProviderListingModel.AddProviderListing(providerListingModel);
                    EmailSender.SendEmail(providerListingModel.PrimaryEmail);
                    var ProviderRegisterUser = new RegisterViewModel();
                    ProviderRegisterUser.Email = providerListingModel.PrimaryEmail;
                    ProviderRegisterUser.Password = providerListingModel.Password;
                    ProviderRegisterUser.UserRole = "Provider";
                    TempData["ProviderRegister"] = ProviderRegisterUser;
                    return RedirectToAction("ProviderRegister", "Account");
                }
                else
                {
                    if (providerListingModel.Bitimagestring != null)
                    {
                        providerListingModel.Logo = HelperClass.SaveImage64binarystring("~/Images/ProviderListingImages", providerListingModel.Bitimagestring);
                    }
                    new ProviderListingModel().UpdateProviderListng(providerListingModel);
                    return RedirectToAction("MyListing");
                }

            }

            ViewBag.Categorylist = new SelectList(providerListingModel.AllCategory(), "Id", "CategoryName", "Select the Category");
            ViewBag.State = new SelectList(new ServicesModel().GetUsStates(), "Id", "State", "Select the State");

            var _providerListingModel = new ProviderListingModel();
            return View(_providerListingModel);
        }
        //[Authorize]
        public ActionResult BecomeProvider()
        {
            ViewBag.CategoryList = new ProviderListingModel().AllCategory();
            return View();
        }
        //[Authorize]
        public ActionResult PromoteYourListing(int ProviderlistingId = 0)
        {
            if (ProviderlistingId == 0)
            {
                var listingId = (int)TempData["ListingId"];
                var userid = User.Identity.GetUserId();
                new ProviderListingModel().UpdateUserId(userid, listingId);
            }

            var servicesmodel = new ServicesModel();
            servicesmodel.servicelist = new ServicesModel().GetAllServices();
            return View(servicesmodel);
        }
        [HttpPost]
        public bool PromoteYourListing(OrderModel orderModel)
        {
            TempData["orderModel"] = new ServicesModel().AllSelectedServices(orderModel);

            return true;
        }
        public PartialViewResult ServiceAreaPartial(int count)
        {
            var providerListingModel = new ProviderListingModel();
            ViewBag.Count = count;
            ViewBag.State = new SelectList(new ServicesModel().GetUsStates(), "Id", "State", "Select the State");
            return PartialView(providerListingModel);
        }
        public PartialViewResult LoadCategoryService(int categoryId)
        {
           // ViewBag.CategoryServicelist = new SelectList(new ProviderListingModel().GetCategoryServices(categoryId), "CategoryServiceId", "Name", "Select the Service");
            ViewBag.CategoryServicelist =new ProviderListingModel().GetCategoryServices(categoryId);
            return PartialView();
        }
        public PartialViewResult LoadCustomService(int count)
        {
            ViewBag.customId = count;
            return PartialView();
        }
        [Authorize]
        public ActionResult MyListing()
        {
            var listing = new ProviderListingModel();
            var UserId = User.Identity.GetUserId();
            listing = listing.GetListingByUserId(UserId);
            listing.AvgRating = listing.AvgRatings(listing);
            return View(listing);
        }
        public ActionResult ViewListing(string CompanyName)
        {
            CompanyName=CompanyName.Replace('-', ' ');
            var listing = new ProviderListingModel();
            listing = listing.GetListingByListingName(CompanyName);
            var userid = User.Identity.GetUserId();
            //var userIdentity = (ClaimsIdentity)User.Identity;
            //var claims = userIdentity.Claims;
            //var roleClaimType = userIdentity.RoleClaimType;
            //var roles = claims.Where(c => c.Type == ClaimTypes.Role).ToList();
            //if (roles[0].Value == "Consumer")
            if (User.Identity.GetUserId()!="")
            {
                if (User.IsInRole("Consumer"))
                {
                    ViewBag.UserDetail = new AgingInHomeContext().AspNetUsers.Include("ConsumerProfiles").FirstOrDefault(s => s.Id == userid);
                }
                else
                {                   
                    ViewBag.UserDetail = null;
                }
            }
            
                
            ViewBag.checkRatingShowOrNot = new AppointmentModel().CheckAppointmentStatus(userid, listing.UserId);
            ViewBag.SimilarProvider = listing.GetAllListing().Where(m => m.ProviderCategory1.Id == listing.ProviderCategory1.Id && m.IsApproved == (int)ListingStatus.Accepted && m.ProviderListingId != listing.ProviderListingId).ToList();
            return View(listing);
        }
        public ActionResult EditProviderListing()
        {
            var listing = new ProviderListingModel();
            var UserId = User.Identity.GetUserId();
            listing = listing.GetListingByUserId(UserId);
            ViewBag.Categorylist = new SelectList(listing.AllCategory(), "Id", "CategoryName", Convert.ToInt32(listing.CategoryId));
            ViewBag.State = new SelectList(new ServicesModel().GetUsStates(), "Id", "State", listing.ServiceAreas[0].UsState.Id);
            //ViewBag.CategoryServicelist = new SelectList(listing.GetCategoryServices(Convert.ToInt32(listing.CategoryId)), "CategoryServiceId", "Name", listing.CategoryServiceId);
            ViewBag.CategoryServicelist = listing.GetCategoryServices(Convert.ToInt32(listing.CategoryId));
            return View("SubmitListing", listing);
        }
        [HttpPost]
        public ActionResult PreviewProviderListing(ProviderListingModel providerListingModel)
        {
            if (ModelState.IsValid)
            {
                if (providerListingModel.Bitimagestring != null)
                {
                    providerListingModel.Logo = HelperClass.SaveImage64binarystring("~/Images/ProviderListingImages", providerListingModel.Bitimagestring);
                }
                ViewBag.Categorylist = new SelectList(providerListingModel.AllCategory(), "Id", "CategoryName", Convert.ToInt32(providerListingModel.CategoryId));
                ViewBag.State = new SelectList(new ServicesModel().GetUsStates(), "Id", "State", providerListingModel.ServiceAreas[0].StateId);
                return View(providerListingModel);
            }
            ViewBag.Categorylist = new SelectList(providerListingModel.AllCategory(), "Id", "CategoryName", "Select the Category");
            ViewBag.State = new SelectList(new ServicesModel().GetUsStates(), "Id", "State", "Select the State");

            var _providerListingModel = new ProviderListingModel();
            return View("SubmitListing", _providerListingModel);

        }
        public ActionResult RatingDetail(int listingId)
        {
            return View();
        }
        public bool ListingRating(RatingModal _RatingModel)
        {
            return _RatingModel.AddRating(_RatingModel);
        }
        public bool EmailExits(RatingModal _RatingModel)
        {
            return _RatingModel.CheckEmailExits(_RatingModel);
        }
        public ActionResult SearchListing(SearchFilterModel searchFilterModel)
        {
            var AllListing = new ProviderListingModel().GetAllListing().Where(g => g.IsApproved == (int)ListingStatus.Accepted).ToList();
            ViewBag.AllListing = AllListing;
            ViewBag.CategoryList = new ProviderListingModel().AllCategory();
            ViewBag.Categorylists = new SelectList(new ProviderListingModel().AllCategory(), "Id", "CategoryName", "Select the Category");

            if (searchFilterModel.Location != null)
            {
                var newAllListing = new List<ProviderListingModel>();
                foreach (var list in AllListing)
                {
                    foreach (var Servicearea in list.ServiceAreas)
                    {

                        int num;
                        bool isNum = int.TryParse(searchFilterModel.Location, out num);
                        if (isNum)
                        {
                            int CheckZipCode = Convert.ToInt32(searchFilterModel.Location);
                            var distance = getDistance(CheckZipCode, (int)Servicearea.ZipCode);
                            if (distance <= Convert.ToDecimal(Servicearea.ServiceRadius))
                            {
                                newAllListing.Add(list);
                                break;
                            }

                        }
                        else
                        {
                            if (Servicearea.City.ToLower() == searchFilterModel.Location.ToLower())
                            {
                                newAllListing.Add(list);
                                break;
                            }
                        }

                    }
                }
                if (searchFilterModel.selectedCategory != null)
                {
                    newAllListing = newAllListing.Where(f => searchFilterModel.selectedCategory.Contains(f.ProviderCategory1.Id)).ToList();
                    ViewBag.selectedCat = searchFilterModel.selectedCategory;
                }
                if (searchFilterModel.SortBy != null)
                {
                    newAllListing = newAllListing.Where(f => f.ProviderCategory1.Id == searchFilterModel.SortBy).ToList();
                }

                return View(newAllListing.OrderByDescending(d => d.ProviderListingId).ToPagedList(searchFilterModel.page ?? 1, 10));
            }
            if (searchFilterModel.selectedCategory != null)
            {
                AllListing = AllListing.Where(f => searchFilterModel.selectedCategory.Contains(f.ProviderCategory1.Id)).ToList();
                ViewBag.selectedCat = searchFilterModel.selectedCategory;
            }
            if (searchFilterModel.SortBy != null)
            {
                AllListing = AllListing.Where(f => f.ProviderCategory1.Id == searchFilterModel.SortBy).ToList();
            }
            return View(AllListing.OrderByDescending(d => d.ProviderListingId).ToPagedList(searchFilterModel.page ?? 1, 10));
        }
        public JsonResult UserEmailExits(string PrimaryEmail)
        {
            return Json(new RatingModal().UserEmailExits(PrimaryEmail), JsonRequestBehavior.AllowGet);
        }
        public JsonResult UserEmailExit(string Email)
        {
            return Json(new RatingModal().UserEmailExits(Email), JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddAboutUs(int listingId)
        {
            ViewBag.listingId = listingId;
            return View();
        }
        [HttpPost]
        public ActionResult AddAboutUs(ListingAboutusModel listingAboutusModel)
        {
            listingAboutusModel.CreatedBy = User.Identity.GetUserId();
            new ListingAboutusModel().AddAboutUs(listingAboutusModel);
            return RedirectToAction("MyListing");
        }
        public ActionResult UpdateAboutUs(int ListingAboutUsId)
        {
            var model = new ListingAboutusModel().EditDescription(ListingAboutUsId);
            return View(model);
        }
        [HttpPost]
        public ActionResult UpdateAboutUs(ListingAboutusModel listingAboutusModel)
        {
            new ListingAboutusModel().UpdateDescription(listingAboutusModel);
            return RedirectToAction("MyListing");
        }
        public bool ChangeStatusOfTabs(bool status, string feildName, int providerListingId)
        {
            return new ListingAboutusModel().ChangeStatusOfTabs(status, feildName, providerListingId);
        }
        public ActionResult AddImage(ProviderImageGalaryModel image)
        {
            if (ModelState.IsValid)
            {
                image.ImagePath = HelperClass.SaveImage("~/Images/ProviderGalaryImages", image.ImageUrl);
                image.CreatedBy = User.Identity.GetUserId();
                image.CreatedDate = DateTime.Now;
                new ProviderListingModel().addProviderImage(image);
                return RedirectToAction("MyListing");
            }
            return RedirectToAction("MyListing");

        }
        public ActionResult AddVideo(ProviderVideo video)
        {
            video.CreatedBy = User.Identity.GetUserId();
            video.CreatedDate = DateTime.Now;
            new ProviderListingModel().addProvidervideo(video);
            return RedirectToAction("MyListing");


        }
        public bool DeleteImage(int imgid)
        {
            return new ProviderListingModel().DelProviderImage(imgid);
        }
        public bool DeleteVideo(int Videoid)
        {
            return new ProviderListingModel().DelProviderVideoLink(Videoid);
        }
        public decimal getDistance(int origin, int destination)
        {
            string url = @"http://maps.googleapis.com/maps/api/distancematrix/xml?origins=" + origin + "&destinations=" + destination + "&sensor=false";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader sreader = new StreamReader(dataStream);
            string responsereader = sreader.ReadToEnd();
            response.Close();
            try
            {
                string times = "";
                DataSet ds = new DataSet();
                ds.ReadXml(new XmlTextReader(new StringReader(responsereader)));
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables["element"].Rows[0]["status"].ToString() == "OK")
                    {
                        // var dis = ds.Tables["duration"].Rows[0]["text"].ToString();
                        times = ds.Tables["distance"].Rows[0]["text"].ToString();
                        var distance = times.Split(' ');
                        decimal dd = Convert.ToDecimal(distance[0]) * Convert.ToDecimal(0.621371);

                        return dd;

                    }
                    return 100000;
                }

                return 100000;
            }
            catch (Exception)
            {
                return 100000;

            }



            //double distance = 0;
            ////string from = origin.Text;
            ////string to = destination.Text;
            //string url = "http://maps.googleapis.com/maps/api/directions/json?origin=" + origin + "&destination=" + destination + "&sensor=false";
            //string requesturl = url;
            ////string requesturl = @"http://maps.googleapis.com/maps/api/directions/json?origin=" + from + "&alternatives=false&units=imperial&destination=" + to + "&sensor=false";
            //string content = fileGetContents(requesturl);

            //try
            //{
            //    JObject o = JObject.Parse(content);
            //    var dd=(double) o.SelectToken("routes[0].legs[0].distance.value");
            //    distance = dd/ 1609;
            //    return distance;
            //}
            //catch
            //{
            //    return 10000;
            //}

        }
        protected string fileGetContents(string fileName)
        {
            string sContents = string.Empty;
            string me = string.Empty;
            try
            {
                if (fileName.ToLower().IndexOf("http:") > -1)
                {
                    System.Net.WebClient wc = new System.Net.WebClient();
                    byte[] response = wc.DownloadData(fileName);
                    sContents = System.Text.Encoding.ASCII.GetString(response);

                }
                else
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(fileName);
                    sContents = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch { sContents = "unable to connect to server "; }
            return sContents;
        }
        public string GetZipCoordinates(string zip)
        {
            string address = "";
            address = "http://maps.googleapis.com/maps/api/geocode/xml?components=postal_code:" + zip.Trim() + "&sensor=false";

            var result = new System.Net.WebClient().DownloadString(address);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);
            XmlNodeList parentNode = doc.GetElementsByTagName("location");
            var lat = "";
            var lng = "";
            foreach (XmlNode childrenNode in parentNode)
            {
                lat = childrenNode.SelectSingleNode("lat").InnerText;
                lng = childrenNode.SelectSingleNode("lng").InnerText;
            }

            return lat + "," + lng;
        }
        public bool UpdateAchieveProviderStatus(int RequestId)
        {
            var model = new InboxModel().UpdateAchieveStatusByProvider(RequestId);
            return model;
        }
        public PartialViewResult GetinboxDetail()
        {
            var listing = new ProviderListingModel();
            var UserId = User.Identity.GetUserId();
            listing = listing.GetListingByUserId(UserId);
            listing.AvgRating = listing.AvgRatings(listing);
            ViewBag.allusers = new AgingInHomeContext().AspNetUsers.ToList();
            ViewBag.CustomeMessages = new InboxModel().GetAllCustomeMessagesByUserid(UserId).GroupBy(p => p.ConversationId)
                                     .Select(s => new CustomeMessagesModel { AllMessages = s.ToList() }
                                     ).ToList();
            return PartialView("_providerInbox", listing.ServiceRequestDetails.ToList());
        }
        public PartialViewResult ArchiMessageDetail(int RequestId)
        {
            var model = new InboxModel().GetMessageDetailByDetailId(RequestId, User.Identity.GetUserId());
            ViewBag.alllisting = new ProviderListingModel().GetAllListing();
            ViewBag.msgType = "ProviderArchive";
            return PartialView("_MessageDetailPartial", model.OrderByDescending(d => d.Id).ToList());
        }
        public PartialViewResult GetAllProviderAppointments()
        {
            var userid = User.Identity.GetUserId();
            var GetAllProviderAppointment = new AppointmentModel().GetAllProviderAppointment(userid).OrderByDescending(s=>s.AppointmentId).ToList();
            return PartialView(GetAllProviderAppointment);
        }
        public bool MakeAppointment(AppointmentModel model)
        {
            model.SubmitAppointment(model,"Provider");
            return true;
        }
    }
}