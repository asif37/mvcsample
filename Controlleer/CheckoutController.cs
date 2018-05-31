using AgingInHome.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AgingInHome.Controllers
{
    public class CheckoutController : Controller
    {
        // GET: Checkout
        public ActionResult Index()
        {
            return View();
        }
        //[Authorize]
        public ActionResult Checkout()
        {
            CheckoutDetailModel _CheckoutDetailModel = new CheckoutDetailModel();
            _CheckoutDetailModel.SelectedServices = (List<ServicesModel>)TempData["orderModel"];
            ViewBag.Countries = new SelectList(new CountryModel().GetAllCountry(), "Id", "CountryName", "Select the Country");
            ViewBag.State = new SelectList(new ServicesModel().GetUsStates(), "Id", "State", "Select the State");
            if (_CheckoutDetailModel.SelectedServices != null)
            {
                return View(_CheckoutDetailModel);
            }
            return HttpNotFound();
        }
        [HttpPost]
        public ActionResult Checkout( CheckoutDetailModel _checkoutDetailModel)
        {
            if (ModelState.IsValid)
            {
                CheckoutDetailModel _CheckoutDetailModel = new CheckoutDetailModel();
                _CheckoutDetailModel.SelectedServices = (List<ServicesModel>)TempData["orderModel"];
                ViewBag.Countries = new SelectList(new CountryModel().GetAllCountry(), "Id", "CountryName", "Select the Country");
                ViewBag.State = new SelectList(new ServicesModel().GetUsStates(), "Id", "State", "Select the State");
                if (_CheckoutDetailModel != null)
                {
                    return View(_CheckoutDetailModel);
                }
                return HttpNotFound();
            }
            ViewBag.Countries = new SelectList(new CountryModel().GetAllCountry(), "Id", "CountryName", "Select the Country");
            ViewBag.State = new SelectList(new ServicesModel().GetUsStates(), "Id", "State", "Select the State");

            return View();
           
        }
        public ActionResult Success()
        {
            return View();
        }
        public ActionResult Cancel()
        {
            return View();
        }
    }
}