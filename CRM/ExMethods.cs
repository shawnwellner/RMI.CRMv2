using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.UI;
using CRM.Domain;
using System.IO;
using System.Reflection;
using CRM.Models;
using System.Dynamic;
using System.Text;
using Newtonsoft.Json;
using CRM.Domain.CustomExceptions;

namespace CRM {
	public class PropertyCompair : IEqualityComparer<string> {

		public bool Equals(string x, string y) {
			return y.Matches(x);
		}

		public int GetHashCode(string obj) {
			throw new NotImplementedException();
		}
	}

	public static class ExMethods {
		public static string ToDetailedString(this Exception ex) {
			return ex.ToDetailedString(LoginUser.CurrentUser);
		}

		public static string ToDetailedString(this Exception ex, LoginUser user) {
			StringBuilder retMessage = new StringBuilder();
			if (ex == null || !user.DevAdmin) {
				if (ex is ControlledException) {
					retMessage.AppendLine(ex.Message);
				} else if (user.IsRmiUser) {
					retMessage.AppendLine("If this problem continues, please let the development department know.");
				} else {
					retMessage.AppendLine("If you are having trouble or this error persists, please contact your account representative.");
				}
			} else {
				retMessage.AppendFormat("<strong>Exception:</strong> <pre>{0}</pre>", ex.Message).AppendLine().AppendLine();
				if (ex.InnerException != null) {
					retMessage.AppendFormat("<strong>InnerException:</strong> <pre>{0}</pre>", ex.InnerException.Message).AppendLine().AppendLine();
				}
				retMessage.AppendFormat("<strong>StackTrace:</strong> <pre>{0}</pre>", ex.StackTrace).AppendLine();
			}
			return retMessage.ToString();
		}

		public static string GetDomainName(this HttpRequestBase req) {
			Uri url = req.Url;
			return url.AbsoluteUri.Replace(url.AbsolutePath, "");
		}

		public static Exception ValidationException(this Controller controller) {
			var errors = (from m in controller.ModelState.Values
						  where m.Errors.Count > 0
						  select m.Errors).Select(e => e.FirstOrDefault()).ToList();
			if (errors != null && errors.Count > 0) {
				StringBuilder errMsg = new StringBuilder();

				foreach (var err in errors) {
					errMsg.AppendLine(err.ErrorMessage.HasValue() ? err.ErrorMessage : err.Exception.Message);
				}
				//string errMsg = string.Join("\r\n", errors);
				if (errMsg.Length > 0) {
					return new InvalidDataException(errMsg.ToString());
				}
			}
			return new Exception("Unexpected Exception.  Model appears to be valid.");
		}

		private static dynamic ParseTaxonmy(this ExpandoObject obj, CustomerFormModel customer, string[] ignore) {
			IEnumerable<string> fieldPatterns = (from p in customer.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
												 select p.GetSearchPattern()).AsEnumerable();

			IDictionary<string, object> data = obj as IDictionary<string, object>;
			if (data.ContainsKey("customer")) {
				data = (IDictionary<string, object>)((IDictionary<string, object>)obj)["customer"];
			}

			IDictionary<string, object> taxItems = new ExpandoObject();
			PropertyCompair compair = new PropertyCompair();

			foreach (string key in data.Keys) {
				if (!fieldPatterns.Contains(key, compair) && !ignore.Contains(key)) {
					taxItems.Add(key, data[key]);
				}
			}
			return taxItems;
		}

		private static string GetSearchPattern(this PropertyInfo prop) {
			return prop.GetCustomAttribute<NotTaxonomy>() == null ? string.Format("^{0}$", prop.Name) : string.Format("^{0}", prop.Name);
		}

		public static dynamic ParseTaxonmy(this HttpRequestBase req, CustomerFormModel customer) {
			string[] ignore = new string[] { "SkipServiceObjects", "FiveNine", "Offer", "Address", "Address2",
				"IsTest", "List", "TestEmail", "TimeOfDay", "BestStartTime", "BestEndTime" };

			string body = req.PostedData();
			if (req.ContentType.Matches("application/json")) {
				return JsonConvert.DeserializeObject<ExpandoObject>(body).ParseTaxonmy(customer, ignore);
			}

			string name, value;
			GroupCollection groups;

			IEnumerable<string> fieldPatterns = (from p in customer.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
												 select p.GetSearchPattern()).AsEnumerable();
			IDictionary<string, object> taxItems = new ExpandoObject();
			PropertyCompair compair = new PropertyCompair();

			foreach (Match m in Regex.Matches(body, @"([^?&=]+)=([^?&=]+)", RegexOptions.IgnoreCase)) {
				if (m.Success) {
					groups = m.Groups;
					name = groups[1].Value;
					if (!fieldPatterns.Contains(name, compair) && !ignore.Contains(name)) {
						value = groups[2].Value;
						taxItems.Add(name, value);
					}
				}
			}
			return taxItems;
		}

		public static dynamic ParseTaxonmy(this ModelStateDictionary model, CustomerFormModel customer) {
			IEnumerable<string> properties = (from p in customer.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
											  let pattern = p.GetCustomAttribute<NotTaxonomy>() == null ? string.Format("^{0}$", p.Name) : string.Format("^{0}", p.Name)
											  select pattern).AsEnumerable();
			IDictionary<string, object> taxItems = new ExpandoObject();
			PropertyCompair compair = new PropertyCompair();

			var items = (from m in model.Keys
						 where !properties.Contains(m, compair)
						 select m).AsEnumerable();

			foreach (string key in items) {
				taxItems.Add(key, model[key].Value);
			}

			return taxItems;
		}

		public static List<SelectListItem> ToDropdownList(this string[] values, string FirstItemText) {
			List<SelectListItem> list = new List<SelectListItem>();
			list.Add(new SelectListItem() { Text = FirstItemText, Value = "" });

			foreach (string value in values) {
				if (value.Length > 2) {
					list.Add(new SelectListItem() { Text = value, Value = "" });
				} else {
					list.Add(new SelectListItem() { Text = value, Value = value });
				}
			}
			return list;
		}

		public static List<SelectListItem> ToDropdownList(this IDictionary<int, string> items, string FirstItemText, string selectedValue) {
			List<SelectListItem> list = new List<SelectListItem>();
			list.Add(new SelectListItem() { Text = FirstItemText, Value = "" });
			string value;
			foreach (int key in items.Keys) {
				value = key.ToString();
				list.Add(new SelectListItem() { Text = items[key], Value = value, Selected = value.Equals(selectedValue) });
			}
			return list;
		}

		public static List<SelectListItem> ToDropdownListIncrementValue(this List<Pair> values, string FirstItemText) {
			List<SelectListItem> list = new List<SelectListItem>();
			list.Add(new SelectListItem() { Text = FirstItemText, Value = "" });
			foreach (Pair value in values) {
				list.Add(new SelectListItem() { Text = value.Second.ToString(), Value = value.First.ToString() });
			}
			return list;
		}

		public static string RenderViewToString(this PartialViewResult view, Controller controller) {
			var context = controller.ControllerContext;
			var viewData = new ViewDataDictionary(view.Model);
			using (var sw = new StringWriter()) {
				IView _view = view.View;
				if (_view == null) {
					var viewResult = ViewEngines.Engines.FindPartialView(context, view.ViewName);
					_view = viewResult.View;
				}
				var viewContext = new ViewContext(context, _view, viewData, new TempDataDictionary(), sw);
				_view.Render(viewContext, sw);
				return sw.GetStringBuilder().ToString();
			}
		}

		public static string RenderViewToString(this Controller controller, string viewName, object model) {
			var context = controller.ControllerContext;
			if (string.IsNullOrEmpty(viewName))
				viewName = context.RouteData.GetRequiredString("action");

			var viewData = new ViewDataDictionary(model);

			using (var sw = new StringWriter()) {
				var viewResult = ViewEngines.Engines.FindPartialView(context, viewName);
				var viewContext = new ViewContext(context, viewResult.View, viewData, new TempDataDictionary(), sw);
				viewResult.View.Render(viewContext, sw);
				return sw.GetStringBuilder().ToString();
			}
		}
	}
}