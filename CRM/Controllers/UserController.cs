using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CRM.Models;
using CRM.Domain;
using System.Collections.Specialized;
using System.Text;

namespace CRM.Controllers {
    public class UserController : Controller {

		public ActionResult Logoff() {
			try {
				LoginUser.Dispose();
				Session.Abandon();
			} catch {

			}
			return Redirect("/");
		}

		// GET: /User/
		public ActionResult Path() {
            LoginUser user = LoginUser.CurrentUser;
            ActionResult redirect = ValidateLogin(user);
            if (redirect != null || Response.StatusCode == 401) { return redirect; }

            //-- set default master view ----
            ViewData["VerticalName"] = user.VerticalName;
            ViewData["MasterView"] = "DefaultView";
            ViewData["Instructions"] = "This is where the creation instructions go.";
			
			String routPath = RouteData.Values["type"] != null ? RouteData.Values["type"].ToString() : null;
			String thankyouType = RouteData.Values["id"] != null ? RouteData.Values["id"].ToString() : null;

			if (routPath == "thankyou") {
                switch (thankyouType) {
                    case "manager":
                        ViewData["ThankYouCopy"] = "A new Call Center Manager has been created.";
                        break;
                    case "managerupdate":
                        ViewData["ThankYouCopy"] = "You have successfully updated a Call Center Manager.";
                        break;
                    case "callcenter":
                        ViewData["ThankYouCopy"] = "A new Call Center Associate has been created.";
                        break;
                    case "callcenterupdate":
                        ViewData["ThankYouCopy"] = "You have successfully updated a Call Center Associate.";
                        break;
                    case "client":
                        ViewData["ThankYouCopy"] = string.Format("A new {0} Network Client has been created.", user.VerticalName);
                        break;
                    case "clientupdate":
                        ViewData["ThankYouCopy"] = string.Format("You have successfully updated a {0} Network Client.", user.VerticalName);
                        break;
                    case "customer":
                        ViewData["ThankYouCopy"] = string.Format("A new {0} Patient has been created.", user.VerticalName);
                        break;
                    case "customerupdate":
                        ViewData["ThankYouCopy"] = string.Format("You have successfully updated a {0} patient.", user.VerticalName);
                        break;
                    default:
                        ViewData["TypeCreated"] = string.Format("{0} Network Admin", user.VerticalName);
                        break;
                }
                return View("thank-you");
            }
            return View("User");
        }

        public ActionResult Create() {
            return Update();
        }

        public ActionResult Search() {
            LoginUser user = LoginUser.CurrentUser;
            ActionResult redirect = ValidateLogin(user);
            if (redirect != null) { return redirect; }

            ViewData["VerticalName"] = user.VerticalName;
            string searchType = RouteData.Values["type"] != null ? RouteData.Values["type"].ToString().ToLower() : null;
            switch (searchType) {
                case "customer":
                    //ViewData["customersearchpanel"] = "style=display:block;";
                    //ViewData["clientsearchpanel"] = "style=display:none;";
                    //ViewData["callacentersearchpanel"] = "style=display:none;";
                    ViewData["ModalTitleCopy"] = "Patient Search Parameters";
                    ViewData["FormDescription"] = "This panel allows you to search for patients.";
                    ViewData["ModalCopy"] = ContentCopy.SearchCustomerModalCopy;
                    ViewData["UserType"] = "Patient";
                    ViewData["FormTitle"] = String.Format("{0} Network Patient Search", user.VerticalName);
                    ViewData["ParialView"] = "SearchTypes/_Customer";
                    ViewData["OnSuccess"] = "getCustomerTable";
                    ViewData["PostUrl"] = "/ajax/customersearch/";
                    ViewData["PageView"] = PageViews.Customer;
                    break;
                case "client":
                    //ViewData["customersearchpanel"] = "style=display:none;";
                    //ViewData["clientsearchpanel"] = "style=display:block;";
                    //ViewData["callacentersearchpanel"] = "style=display:none;";
                    ViewData["ModalTitleCopy"] = "Client Search Parameters";
                    ViewData["FormDescription"] = "This panel allows you to search for clients.";
                    ViewData["ModalCopy"] = ContentCopy.SearchClientModalCopy;
                    ViewData["UserType"] = "Client";
                    ViewData["FormTitle"] = String.Format("{0} Network Client Search", user.VerticalName);
                    ViewData["ParialView"] = "SearchTypes/_Client";
                    ViewData["OnSuccess"] = "getClientTable";
                    ViewData["PostUrl"] = "/ajax/clientsearch/";
                    ViewData["PageView"] = PageViews.Client;
                    break;
                case "callcenteruser":
                    //ViewData["customersearchpanel"] = "style=display:none;";
                    //ViewData["clientsearchpanel"] = "style=display:none;";
                    //ViewData["callacentersearchpanel"] = "style=display:block;";
                    ViewData["ModalTitleCopy"] = "Call Center Search Parameters";
                    ViewData["FormDescription"] = "This panel allows you to search for call center agents/managers.";
                    ViewData["UserType"] = "Call Center User";
                    ViewData["ModalCopy"] = ContentCopy.SearchCallCenterUserModalCopy;
                    ViewData["FormTitle"] = String.Format("{0} Network Call Center User Search", user.VerticalName);
                    ViewData["ParialView"] = "SearchTypes/_CallCenter";
                    ViewData["OnSuccess"] = "getCallCenterTable";
                    ViewData["PostUrl"] = "/ajax/callcentersearch/";
                    ViewData["PageView"] = PageViews.CallCenter;
                    break;
            }
            return View("Search", new SearchParams());
        }

        public ActionResult Update() {
            LoginUser user = LoginUser.CurrentUser;
            ActionResult redirect = ValidateLogin(user);
            if (redirect != null) { return redirect; }

            String entity = RouteData.Values["type"] != null ? RouteData.Values["type"].ToString() : null;

			int searchUserId = RouteData.Values["id"] != null ? Convert.ToInt32(RouteData.Values["id"].ToString()) : -1;

			String pageType = RouteData.Values["action"] != null ? RouteData.Values["action"].ToString() : null;
            ViewData["action"] = pageType;

            //-- set default master view ----
            ViewData["VerticalName"] = user.VerticalName;
            ViewData["MasterView"] = "DefaultView";
            ViewData["FormUserType"] = HttpContext.Request.RequestContext.RouteData.Values["id"];
            ViewData["FormTitle"] = String.Format("{0} Network Call Center ", user.VerticalName);

            if (entity.Matches("client")) {
                ViewData["PageType"] = PageViews.Client;
                ViewData["ModalTitleCopy"] = "Client / Provider Information";
                ViewData["ModalCopy"] = ContentCopy.UpdateClientModalCopy;
                ViewData["PartialView"] = "UpdateTypes/_Client";
                ClientFormModel model;
                Client client = Client.GetClientById(user, searchUserId) ?? new Client();

                if (client.UserId > 0) {
                    ViewData["FormTitle"] = String.Format("Client Update - ({0})", client.CompanyName);
                    ViewData["FormDescription"] = "This panel allows you to update a client/provider";
                    model = client.CloneAs<ClientFormModel>(client.LoginUsers);
                    model.VerticalIds = Vertical.GetVerticalsByUserId(client.UserId).Select(v => v.VerticalId).ToList();
                } else {
                    ViewData["FormTitle"] = String.Format("{0} Network Client / Provider Create", user.VerticalName);
                    ViewData["FormDescription"] = "This panel allows you to create client/provider.";
                    model = new ClientFormModel() {
                        VerticalIds = new List<int>()
                    };
                }
                return View("Update", model);
            } else if (entity.Matches("customer")) {
                ViewData["PageType"] = PageViews.Customer;
                ViewData["PartialView"] = "UpdateTypes/_Customer";
                InputModel.ResetStaticLists();
                CustomerFormModel customer;
                Customer cust = Customer.GetCustomerById(user, searchUserId, true);
                if (cust != null) {
                    ViewData["FormTitle"] = String.Format("Patient Update - ({0} {1}){2}", cust.FirstName, cust.LastName, cust.Transfered == true ? " - Transfered" : "");
                    ViewData["FormDescription"] = "This panel allows you to update a patient.";
                    customer = cust.CloneAs<CustomerFormModel>();
                } else {
                    ViewData["FormTitle"] = "Patient Create";
                    ViewData["FormDescription"] = "This panel allows you to create a patient.";
                    customer = new CustomerFormModel() {
                        VerticalId = user.Verticals.First().VerticalId //Set default vertical id
                    };
                }
                return View("Update", customer);

            } else if (entity.Matches("callcenter")) {
                ViewData["PageType"] = PageViews.CallCenter;
                ViewData["PartialView"] = "UpdateTypes/_CallCenterUser";
                CallCenterFormModel agent;
                CallCenterUser ccUser = CallCenterUser.GetUserById(user, searchUserId);
                if (ccUser != null) {
                    ViewData["FormDescription"] = "This panel allows you to update a call center agent/manager.";
                    switch ((UserTypes)ccUser.UserTypeId) {
                        case UserTypes.CallCenterManager:
                            ViewData["FormTitle"] = String.Format("Manager Update - ({0} {1})", ccUser.FirstName, ccUser.LastName);
                            break;
                        case UserTypes.CallCenterAgent:
                            ViewData["FormTitle"] = String.Format("Agent Update - ({0} {1})", ccUser.FirstName, ccUser.LastName);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    agent = ccUser.CloneAs<CallCenterFormModel>();
                    agent.VerticalIds = Vertical.GetVerticalsByUserId(ccUser.UserId).Select(v => v.VerticalId).ToList();
                } else {
                    ViewData["FormTitle"] = "Call Center Agent/Manager Create";
                    ViewData["FormDescription"] = "This panel allows you to create a call center agent/manager.";
                    agent = new CallCenterFormModel() {
                        VerticalIds = new List<int>()
                    };
                }
                return View("Update", agent);
            }
            return null;
        }

        public string TechInfo() {
            StringBuilder buffer = new StringBuilder();
            NameValueCollection variables = Request.Headers;

            buffer.AppendLine("<h2>Headers</h2>");
            foreach (string key in variables.AllKeys) {
                buffer.AppendFormat("{0} = {1}", key, variables.GetValues(key).FirstOrDefault()).AppendLine();
            }
            buffer.AppendLine().AppendLine();

            buffer.AppendLine("<h2>Variables</h2>");
            variables = Request.ServerVariables;
            foreach (string key in variables.AllKeys) {
                buffer.AppendFormat("{0} = {1}", key, variables.GetValues(key).FirstOrDefault()).AppendLine();
            }
            return buffer.ToString().Replace("\r\n", "<br/>");
        }

        private ActionResult ValidateLogin(LoginUser user) {
            ViewData["CurrentUser"] = user;
            //ViewData["VerticalCss"] = UserInfo.VerticalInfo.Url + ".css";
            if (!user.LoggedIn) { return View("Login"); }
            Uri reqUrl = Request.Url;
            string path = Request.Path;
            string query = Request.Url.Query;

            /*if ((Request.IsLocal || Request.ServerVariables["REMOTE_ADDR"].Matches(@"^10\.3\.4\.\d+$")) && !Request.IsAuthenticated) {
                Response.StatusCode = 401;
                return null;
            }*/

            /*if (!user.VerticalSelected) {
                IReadOnlyList<Vertical> verticals = user.Verticals;
                ViewData["VerticalList"] = verticals;
                if (verticals.Count == 1) {
                    Vertical vert = verticals.First();
                    Uri reqUrl = Request.Url;
                    Uri vertUrl = new Uri(vert.Url);
                    string url = vert.Url;
                    string redirect;
                    string query;
                    if (vertUrl.Query.HasValue()) {
                        url = string.Format("{0}://{1}", vertUrl.Scheme, vertUrl.Authority);
                        query = string.Concat(vertUrl.Query, reqUrl.Query.Replace('?', '&'));
                    } else {
                        query = reqUrl.Query;
                    }

                    switch (user.UserType) {
                        case UserTypes.Administrator:
                            redirect = string.Concat(url, reqUrl.AbsolutePath, query);
                            break;
                        case UserTypes.CallCenterManager:
                            redirect = string.Format("{0}/user/path/manager/{1}", url, query);
                            break;
                        case UserTypes.CallCenterAgent:
                            redirect = string.Format("{0}/user/path/accosiate/{1}", url, query);
                            break;
                        case UserTypes.Client:
                        case UserTypes.Child:
                            redirect = string.Format("{0}/user/search/customer/{1}", url, query);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                return Redirect(redirect);
                } else {
                    return View("SelectVertical");
                }
            }*/
            string redirect = null;
            switch (user.UserType) {
                case UserTypes.Administrator:
                    return null;
                case UserTypes.CallCenterManager:
                    if (path == "/" || path.Matches(@"^/user/\w+/client")) {
                        redirect = string.Concat("/user/path/manager/", query);
                    }
                    break;
                case UserTypes.CallCenterAgent:
                    if (path == "/" || path.Matches(@"^/user/\w+/client")) {
                        redirect = string.Concat("/user/path/accosiate/", query);
                    }
                    break;
                case UserTypes.Client:
                case UserTypes.Child:
                    if(path == "/" || path.Matches("^/user/update")) {
                        redirect = string.Concat("/user/search/customer/", query);
                    }
                    break;
                case UserTypes.StandardUser:
                    if (path == "/" || path.Matches("^/user/update")) {
                        redirect = string.Concat("/user/search/customer/", query);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            if (redirect.HasValue()) { return Redirect(redirect); }
            return null;
        }
    }
}
