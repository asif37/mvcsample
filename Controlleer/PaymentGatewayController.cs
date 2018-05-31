using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Threading.Tasks;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;
using AgingInHome.Models;
namespace AgingInHome.Controllers
{
    public class PaymentGatewayController : Controller
    {
        // GET: PaymentGateway
        public ActionResult Index()
        {
            //PaymentGatewayModel.ChargeCreditCard.Run("668mbMb6H9K8", "8b79AFxaW3p7376D", 100);
            PaymentGatewayModel.ChargeCreditCard.Run("5vB68s9Vu", "87XN9h3Vvka74284", 100);
            return View();
        }
    }
}