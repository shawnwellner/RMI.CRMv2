using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CRM.Domain;

namespace CRM.Controllers
{
    public class ReportingController : Controller
    {
        public ActionResult Index(string report) {
            LoginUser user = LoginUser.CurrentUser;
            ViewData["MasterView"] = "DefaultView";
            if (user.LoggedIn) {
                if (user.UserRole == UserRoles.Administrator) {
                    ViewBag.Report = (report ?? "GetLeadReport");
                    return View("Default");
                } else if (!user.IsRmiUser) {
                    return Redirect("/user/search/customer");
                }
            }
            return Redirect("/");
        }

    }
}
