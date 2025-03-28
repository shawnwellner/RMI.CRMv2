using CRM.Domain;
using CRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CRM {

    public class HandleExceptionAttribute : HandleErrorAttribute  {
        private Type _type;

        public HandleExceptionAttribute()
            : this(typeof(PartialViewResult)) {
        }

        public HandleExceptionAttribute(Type actionResult) {
            if(actionResult != typeof(PartialViewResult) &&
                actionResult != typeof(JsonResult)) {
                    throw new ArgumentException("actionResult must be either a PartialViewResult or JsonResult type");
            }
            this._type = actionResult;
        }

        public override void OnException(ExceptionContext filterContext) {
            if (filterContext.Exception is UnauthorizedAccessException) {
                filterContext.Result = new JsonResult() {
                    Data = new { redirect = "/" },
                    MaxJsonLength = int.MaxValue,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            } else {
				//filterContext.Controller.ViewData.Model = DialogTypes.Error;
					//filterContext.Exception;
				if (this._type == typeof(JsonResult)) {
                    filterContext.Result = new JsonResult() {
                        Data = new { error = filterContext.Exception.ToDetailedString() },
                        MaxJsonLength = int.MaxValue,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                } else {
					PartialViewResult view = new PartialViewResult() {
						ViewName = "_ModalDialog",
						ViewData = filterContext.Controller.ViewData //new ViewDataDictionary(DialogTypes.Error)
					};

					view.ViewData.Model = DialogTypes.Error;
					view.ViewData["Exception"] = filterContext.Exception;
					filterContext.Result = view;
                }
                Utility.LogError(filterContext.Exception);
            }
            filterContext.ExceptionHandled = true;
        }
    }
}