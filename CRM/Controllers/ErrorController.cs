using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CRM.Controllers
{
    public class ErrorController : Controller
    {
        public ViewResult Path() {
            return this.Index();
        }

        public ViewResult Index() {
            return View("Error");
        }
    }
}
