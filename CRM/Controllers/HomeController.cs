﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CRM.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            //-- set default master view ----
            ViewData["MasterView"] = "DefaultView";
            return View("Index");
        }

        //public ActionResult ThankYou()
        //{
        //    //-- set default master view ----
        //    ViewData["MasterView"] = "DefaultView";
        //    return View("thank-you");
        //}


      
    }
}
