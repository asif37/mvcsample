using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AgingInHome.BLL.Enums;
using AgingInHome.Models;

namespace AgingInHome.Controllers
{
    [Authorize(Roles="SaleUser") ]
    public class SaleDepartmentController : Controller
    {
        // GET: SaleDepartment
        public ActionResult Dashboard()
        {
            var AllListing = new ProviderListingModel().GetAllListing();
            return View(AllListing.OrderBy(d => d.IsApproved).ThenByDescending(f => f.ProviderListingId).ToList());
        }
    }
}