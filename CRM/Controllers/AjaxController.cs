using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
//using System.Web.WebPages;
using System.Web.UI;
using CRM.Domain;
using CRM.Models;
using System.Linq;
using Newtonsoft.Json;
using User = CRM.Domain.UserBase;
using System.IO;
using System.Web;
using System.Dynamic;
using CRM.Domain.CustomExceptions;

namespace CRM.Controllers {
    public class AjaxController : Controller {
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleException]
        public ActionResult ValidateLogin(LoginModel viewModel) {
            #region Form & Form validation
            if (ModelState.IsValid) {
                LoginUser user;
                Uri referrer = Request.UrlReferrer;
                //-- If lead success then return url to redirect too ------
                if (LoginUser.Validate(viewModel.UserName, viewModel.Password, out user)) {
                    return Json(new { redirect = referrer.PathAndQuery });
                }
            }
            #endregion Form & Form validation

            throw this.ValidationException();
        }

        [AcceptVerbs(HttpVerbs.Post)]
		[HandleException]
		public PartialViewResult AddClientLoginUser() {
            ClientFormModel model = Request.InputStream.Deserialize<ClientFormModel>();
            ViewData["StartIndex"] = model.LoginUsers.Count;
            model.LoginUsers.Add(new LoginUserModel());
            return PartialView("_ClientUserLogin", model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleException(typeof(JsonResult))]
        public JsonResult CheckCredentials(string userName, string password) {
            if (!LoginUser.CurrentUser.LoggedIn) { throw new UnauthorizedAccessException(); }
            if (userName.HasValue() && password.HasValue()) {
                bool exists = Domain.LoginUser.CheckCredentialsExist(userName, password);
                return Json(new { Exists = exists });
            }
            throw new InvalidDataException();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleException]
        public ActionResult CustomerSearch(SearchParams viewModel) {
            LoginUser user = LoginUser.CurrentUser;
            if (!user.LoggedIn) { throw new UnauthorizedAccessException(); }
            //throw new Exception("Testing", new Exception("Inner Exception"));
            #region Form & Form validation
            if (ModelState.IsValid) {
				Pagination<Customer> results = Customer.Search(viewModel, user.UserType);

				/*CustomerSearchResult results = Customer.Search(viewModel, user.UserType);*/
                object list;
                if (results.Items.Count > 0) {
                    if (viewModel.Exporting) {
                        list = results.Items.ToExportView(user);
                    } else {
                        list = results.Items;
                    }
                } else {
                    list = new object[] { };
                }
                return new JsonResult() {
                    Data = new { ListofCustomers = list, PageInfo = results.PageInfo },
                    MaxJsonLength = int.MaxValue
                };
            }
            #endregion Form & Form validation
            throw this.ValidationException();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleException]
        public ActionResult GetReport(string name, DateTime? startTime, DateTime? endTime, int? page, int? rows, int? leadType) {
            LoginUser user = LoginUser.CurrentUser;
            if (!user.LoggedIn) { throw new UnauthorizedAccessException(); }
            if (user.IsAdmin) {
                ReportResults report = Customer.GetReport(name, startTime, endTime, page, rows, leadType);
                return new JsonSerializer(report);
            }
            return null;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleException(typeof(JsonResult))]
        public ActionResult CheckForDuplicate(int? UserId, string Phone, string Email) {
            LoginUser user = LoginUser.CurrentUser;
            if (!user.LoggedIn) { throw new UnauthorizedAccessException(); }
            Customer duplicate = Customer.GetDuplicates(UserId, Phone, Email);
            if (duplicate != null) {
                ViewData["IsDuplicate"] = true;
                return PartialView("_CustomerDetails", duplicate);
            } else {
                return new JsonResult() {
                    Data = new {
                        sucess = true,
                        contents = PartialView("_CustomerDetails", duplicate)

                    }
                };
            }
            //return null;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleException]
        public ActionResult CallCenterSearch(SearchParams viewModel) {
            string message = string.Empty;
            LoginUser user = LoginUser.CurrentUser;
            if (!user.LoggedIn) { throw new UnauthorizedAccessException(); }
            //viewModel.VerticalId = user.VerticalId;
            viewModel.LoginUserId = user.UserId;
            if (ModelState.IsValid) {
                IReadOnlyList<CallCenterUser> users = CallCenterUser.Search(viewModel);
                if (users != null) {
                    return Json(users.OrderBy(u => u.LastName));
                }
                return null;
            }
            throw this.ValidationException();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleException]
        public ActionResult ClientSearch(SearchParams viewModel) {
            string message = string.Empty;
            #region Form & Form validation
            LoginUser user = LoginUser.CurrentUser;
            if (!user.LoggedIn) { throw new UnauthorizedAccessException(); }
            if (ModelState.IsValid) {
                //viewModel.VerticalId = user.VerticalId;
                viewModel.LoginUserId = user.UserId;
                IReadOnlyList<Client> clients = Client.Search(viewModel);
                if (clients != null) {
                    return Json(clients.OrderBy(c => c.CompanyName));
                }
                return null;
            }
            #endregion Form & Form validation
            throw this.ValidationException();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleException]
        public PartialViewResult GetCustomerDetails(int userId) {
            LoginUser user = LoginUser.CurrentUser;
            if (!user.LoggedIn) { throw new UnauthorizedAccessException(); }
            Customer customer = Customer.GetCustomerById(user, userId, true);
            //if (customer != null) { customer.GetDirections(); }
            return PartialView("_CustomerDetails", customer);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public void Logout() {
            LoginUser.Dispose();
            Session.Abandon();
            Response.Redirect("/");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public void Error(string url, string form, string error, string stacktrace) {
            string additionalInfo = string.Format(@"
<div>
    <b>Additional Information:</b></br>
    &nbsp;&nbsp;&nbsp;<b>Page Url:</b>&nbsp;{0}</br>
    &nbsp;&nbsp;&nbsp;<b>Form:</b>
    &nbsp;&nbsp;&nbsp;<pre>{1}</pre>
</div>
</br></br>
", url, form);
			Utility.LogError(Request, new JavaScriptException(error, stacktrace), additionalInfo);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleException]
        public ActionResult ClientUpdate(ClientFormModel formModel) {
            LoginUser user = LoginUser.CurrentUser;
            if (!user.LoggedIn) { throw new UnauthorizedAccessException(); }
            if (ModelState.IsValid) {
                List<Client> clientLogins = formModel.LoginUsers.ConvertAll(c => c.CloneAs<Client>());

                Client client = formModel.CloneAs<Client>(clientLogins);
                //client.StateAbbr = formModel.State;
                //client.ZipCode = formModel.Zip;
                if (formModel.ClientId != null && formModel.PrevStateJson.HasValue()) {
                    client.PrevState = JsonConvert.DeserializeObject<Client>(formModel.PrevStateJson);
                }
                client = Client.Update(client, formModel.VerticalIds);
                return Json(new { redirect = "/user/path/thankyou/clientupdate" });
            }
            throw this.ValidationException();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleException]
        public ActionResult CallCenterUpdate(CallCenterFormModel formModel) {
            LoginUser user = LoginUser.CurrentUser;
            if (!user.LoggedIn) { throw new UnauthorizedAccessException(); }
            const string redirect = "/user/path/thankyou/callcenterupdate";
            if (formModel.Enabled == false) {
                ModelState["Five9AgentId"].Errors.Clear();
            }
            if (ModelState.IsValid) {
                CallCenterUser agent = formModel.CloneAs<CallCenterUser>();
                CallCenterUser.Update(agent, formModel.VerticalIds);
                return Json(new { redirect = redirect });
            }
            throw this.ValidationException();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleException]
        public JsonResult GetListOfProviders(CustomerFormModel viewModel) {
            LoginUser user = LoginUser.CurrentUser;
            if (!user.LoggedIn) { throw new UnauthorizedAccessException(); }
            if (ModelState.IsValid) {
                try {
                    viewModel.TrimStringProperties();
                    ProviderList providers = getProviders(viewModel, user);
					
					if(viewModel.UserId == null && //Creating Customer
					   providers.NotQualifiedAnswers.Count == 0 && //Customer Is Qualified
					   viewModel.ListOfAnswers.Any(a => a.QuestionId == 10 && a.Answer.ToBool())) { //Customer Answered Yes to Prescription Refills
						string dialog = PartialView("_ModalDialog", DialogTypes.PrescriptionRefills).RenderViewToString(this);
						return Json(new {
							customerLocation = providers.CustomerLocation,
							listOfProviders = providers,
							notQualified = providers.NotQualifiedAnswers,
							dialog = dialog
						});
					}
                    return Json(new {
                        customerLocation = providers.CustomerLocation,
                        listOfProviders = providers,
                        notQualified = providers.NotQualifiedAnswers,
					});
                } catch (ZipCodeException ex) {
                    switch (ex.Reason) {
                        case ZipCodeException.ReasonTypes.StateNotMatched:
                            return Json(new { invalidZipcode = true, reason = "nomatch", customerLocation = ex.Location });
                        default:
                            return Json(new { invalidZipcode = true, reason = "notfound" });
                    }
                }
            }
            throw this.ValidationException();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleException]
        public ActionResult TransferCustomer(CustomerTransferModel model) {
            LoginUser user = LoginUser.CurrentUser;
            if (!user.LoggedIn) { throw new UnauthorizedAccessException(); }
            if (ModelState.IsValid) {
                model.Customer.UserId = model.Customer.CustomerId;
                model.Customer.OfficeLocationId = model.OfficeLocationId;
                model.Customer.VCLRelId = model.VCLRelId;
                bool transfered = model.Customer.Transfered;
                if (transfered) {
                    if (model.EmailTransfer == true) {
						model.Customer.DispositionId = (int)Disposition.DispositionIds.EmailTransfer;
                    } else if (model.Customer.Disposition.HasValue()) {
                        model.Customer.DispositionId = (int)Disposition.DispositionIds.TalkingToClient;
					} else {
						model.Customer.DispositionId = (int)Disposition.DispositionIds.TransferSuccess;
                    }
                } else if (model.Customer.Disposition.HasValue()) {
                    model.Customer.DispositionId = Disposition.LookupDisposition(model.Customer.VerticalId, model.Customer.Disposition);
                }

                Customer.TransferCustomer(model.Customer);
                if (transfered) {
                    if (model.EmailTransfer.ToBool()) {
                        var body = this.RenderViewToString("_ClientEmailTemplate", model.Customer);
                        Utility.SendEmail(Settings.ClientEmailTransferFrom, string.Format(Settings.ClientEmailTemplateSubjectFormat, user.VerticalName, model.Customer.LastName), body, Settings.ClientEmailTemplateRecipients);
                    }
                    WebSockets.BroadcastTransferedChange(model, transfered);
                    if (model.Customer.DispositionId != 0) {
                        return PartialView("_Five9Reminder", model.Customer);
                    }
                } else {
                    WebSockets.BroadcastTransferedChange(model, false);
                    return Json(new { redirect = "/user/path/thankyou/customerupdate" });
                }
                return null;
            }
            throw this.ValidationException();
        }

        [AcceptVerbs(HttpVerbs.Post)]
		[HandleException]
		public ActionResult UpdateFromStrollHealth(StrollHealthApi.Patient patient) {
			var success = patient.Update();
            return Json(new { success = success });
        }

        [AcceptVerbs(HttpVerbs.Post)]
		[HandleException]
		public ActionResult CustomerCreate(CustomerFormModel customer) {
            try {
               if (!customer.FirstName.Matches("^test") && !customer.LastName.Matches("^test")) {
                    if (Customer.IsDuplicate(customer.Phone, customer.Email)) {
                        throw new Exception("Phone and/or Email is a possible duplicate from Form Post");
                    }
                }
                ModelState["customer.City"].Errors.Clear();
                ModelState["customer.State"].Errors.Clear();
                //ModelState["customer.Zip"].Errors.Clear();
                return CustomerUpdate(customer, true);
            } catch (Exception ex) {
                Utility.LogError(Request, ex);
                return Json(new { Error = ex.Message });
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleException]
        public ActionResult CustomerUpdate(CustomerFormModel model, bool? formPost) {
            if (ModelState.IsValid) {
                LoginUser user = LoginUser.CurrentUser;
                if (formPost == null && !user.LoggedIn) { throw new UnauthorizedAccessException(); }
                model.TrimStringProperties();

                IDictionary<string, object> taxonomy = Request.ParseTaxonmy(model);
                model.VerticalId = model.VerticalId ?? OfficeLocation.GetVerticalIdByLocationId(model.OfficeLocationId.ToInt());
                if (model.VerticalId == null) { throw new ArgumentNullException("VerticalId is not set"); }
                model.LeadSource = model.LeadSource ?? Request.GetDomainName();

                CustomerTransferModel transferModel = null;
                Customer cust = model.CloneAs<Customer>();
				cust.OnCustomerCreate += Customer_OnCustomerCreate;

				if (taxonomy.GetValue("PartialUpdate", false) == true && user.IsAdmin && model.Transfered == true) {
                    Customer.Update(cust, user.Five9AgentId, null, model.ListOfAnswers); //Don't Update Taxonomy Data
                    return Json(new { redirect = "/user/path/thankyou/customerupdate" });
				} else {
                    Customer.Update(cust, user.Five9AgentId, taxonomy, model.ListOfAnswers);
                    if (formPost == true) {
                        return new JsonSerializer(cust, true);
                    } else if (model.NotQualified == true || model.DispositionId != null) {
                        transferModel = cust.CloneAs<CustomerTransferModel>();
                        transferModel.Customer = cust;
                        transferModel.AllowEmailTransfer = model.AllowEmailTransfer == true;
                        WebSockets.BroadcastTransferedChange(transferModel, cust.Transfered || model.Transfered == true);
						if(cust.SentToStrollHealth) {
							return PartialView("_ModalDialog", DialogTypes.StrollHealth);
						} else if (cust.DispositionId.In((int)Disposition.DispositionIds.EmailTransfer)) {
							var body = this.RenderViewToString("_ClientEmailTemplate", cust);
							Utility.SendEmail(Settings.ClientEmailTransferFrom, string.Format(Settings.ClientEmailTemplateSubjectFormat, user.VerticalName, cust.LastName), body, Settings.ConciergeEmailTransferFrom);
						}
                        return Json(new { redirect = "/user/path/thankyou/customerupdate", sentToStrollHealth = cust.SentToStrollHealth });
                    } else if (model.ClientVerticalRelId.HasValue && model.OfficeLocationId.HasValue) {
                        var questions = QuestionModel.GetClientQuestions(model.VerticalId.Value, model.ClientVerticalRelId, model.OfficeLocationId, cust.UserId);
                        if (questions.Count > 0) {
                            ClientAnswerModel answerModel = cust.CloneAs<ClientAnswerModel>();
                            answerModel.Customer = cust;
                            answerModel.ListOfQuestions = questions;
                            ViewData["ListOfAnswersName"] = "ListOfClientAnswers";
                            return PartialView("_ClientQuestions", answerModel);
                        }
                    }

                    transferModel = cust.CloneAs<CustomerTransferModel>();
                    transferModel.Customer = cust;
                    transferModel.AllowEmailTransfer = model.AllowEmailTransfer == true;
                    return PartialView("_CustomerTransfer", transferModel);
                }
            }
            throw this.ValidationException();
        }

		/// <summary>
		/// This event is to handle exceptions that might occur
		/// after a customer is is created to prevent a duplicate customer
		/// from being created.
		/// Errror Dialogs render the view "_UpdateCustomerId" which loads
		/// JavaScript to set the UserId input field, to the customer gets updated
		/// and not created on retries
		/// </summary>
		private void Customer_OnCustomerCreate(Customer customer) {
			ViewData["CustomerId"] = customer.UserId;
		}

		[AcceptVerbs(HttpVerbs.Post)]
        [HandleException]
        public ActionResult UpdateClientAnswers(ClientAnswerModel model) {
            LoginUser user = LoginUser.CurrentUser;
            if (!user.LoggedIn) { throw new UnauthorizedAccessException(); }
            if (ModelState.IsValid) {
                if (model.ListOfClientAnswers.Any(a => a.Rules.Any(r => !r.IsQualified))) {
                    return Json(new { notQualified = true, officeLocationId = model.VCLRelId, script = ContentCopy.NotQualified });
                } else {
                    Customer.UpdateCustomerAnswers(model.Customer.CustomerId, model.ListOfClientAnswers);
                    OfficeLocation loc = Provider.GetProviderByVCLRelId(model.Customer.VerticalId, model.Customer.UserId, model.ClientVerticalRelId, model.VCLRelId, model.Customer.ZipCode, model.Customer.City, model.Customer.StateAbbr);
                    CustomerTransferModel transfer = loc.CloneAs<CustomerTransferModel>().AutoFill(loc.Provider).AutoFill(model);
                    return PartialView("_CustomerTransfer", transfer);
                }
            }
            throw this.ValidationException();
        }

        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [HandleException]
        public PartialViewResult GetView(string viewName, string param) {
            if (!LoginUser.CurrentUser.LoggedIn) { throw new UnauthorizedAccessException(); }
            object model = null;
            if (Request.HttpMethod == "POST") {
                if (param.HasValue()) {
                    Type type = Type.GetType(string.Format("CRM.Models.{0}", param));
                    model = Request.InputStream.Deserialize(type);
                }
            } else {
                model = param;
            }
            return PartialView(viewName, model);
        }

        [ValidateInput(false),AcceptVerbs(HttpVerbs.Post), HandleException]
        public PartialViewResult GetVerticalQuestions(int verticalId, int? customerId) {
            if (!LoginUser.CurrentUser.LoggedIn) { throw new UnauthorizedAccessException(); }
			var questions = QuestionModel.GetVerticalQuestions(verticalId, customerId);
            return PartialView("_VerticalQuestions", questions);
        }

        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public PartialViewResult SelectDisposition() {
            LoginUser user = LoginUser.CurrentUser;
            if (!user.LoggedIn) { throw new UnauthorizedAccessException(); }
            return PartialView("_Dispositions", 1);
        }

        [AcceptVerbs(HttpVerbs.Post)]
		[HandleException]
		public JsonResult GetTypeAheadLists() {
            LoginUser user = LoginUser.CurrentUser;
            if (!user.LoggedIn) { throw new UnauthorizedAccessException(); }
            InputModel model = new InputModel();
			//throw new Exception("THIS IS A TEST");
            return Json(new {
                InsuranceTypes = Question.InsuranceList.Select(i => i.Value),
                //ListOfYears = model.ListOfYears.Where(y => y.Value.HasValue()).Select(y => y.Value),
                DispositionList = Disposition.GetDispostionByTypeId(1) //.Select(d => d.Value)
            });
        }

        [AcceptVerbs(HttpVerbs.Post)]
		[HandleException]
		public JsonResult ValidateZipcode(string zipcode) {
            try {
                Place place = ZipCodeInfo.GetZipCodeInfo(zipcode);
                return Json(place);
            } catch(ZipCodeException ex) {
                switch (ex.Reason) {
                    case ZipCodeException.ReasonTypes.StateNotMatched:
                        return Json(new { invalidZipcode = true, reason = "nomatch", customerLocation = ex.Location });
                    default:
                        return Json(new { invalidZipcode = true, reason = "notfound" });
                }
            }
        }

        private static ProviderList getProviders(CustomerFormModel model, LoginUser user) {
            ProviderList list;
            if (model.OfficeLocationId == null || model.OfficeLocationId.Value > 0) {
                List<QuestionInput> answers = model.ListOfAnswers;

                int? vclRelId = model.Transfered.ToBool() ? model.VCLRelId : null;
                if (model.Latitude.HasValue && model.Longitude.HasValue) {
                    Place place = model.CloneAs<Place>();
                    list = Provider.GetListOfProviders(model.VerticalId, model.UserId, place, model.ClientVerticalRelId, vclRelId, answers);
                } else {
                    list = Provider.GetListOfProviders(model.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, vclRelId, answers);
                }
                /*
                var providers = list.GroupBy(l => l.Provider.ClientId).FirstOrDefault();
                if (providers != null) {
                    foreach (var loc in providers) {
                        loc.Provider.Description = loc.Provider.Description.Replace("{UserName}", user.FullName)
                                                      .Replace("{CustomerName}", model.FirstName)
                                                      .Replace("{customer name}", model.FirstName)
                                                      .Replace("{Company}", user.CompanyName);
                    }
                }*/
                foreach (var loc in list) {
                    loc.Provider.Description = loc.Provider.Description.Replace("{UserName}", user.FullName)
                                                                       .Replace("{CustomerName}", model.FirstName)
                                                                       .Replace("{Company}", user.CompanyName);
                }
            } else {
                list = new ProviderList();
            }
            return list;
        }

        private static string Serialize(ModelStateDictionary modelState, DetailedUser model) {
            return Serialize(modelState, model, null);
        }

        private static string Serialize(ModelStateDictionary modelState, DetailedUser model, params string[] ingnoreProperties) {
            List<string> errors = new List<string>(modelState.Keys.Count);
            ModelState value;
            string propName;
            StringBuilder buffer = new StringBuilder();
            foreach (string k in modelState.Keys) {
                value = modelState[k];
                if (value.Errors.Count > 0) {
                    propName = k.Match("[a-z_]+$").Value;
                    errors.Add(propName);
                    foreach (var e in value.Errors) {
                        buffer.AppendLine(e.ErrorMessage);
                    }
                }
            }
            buffer.AppendLine().Append(model.SerializeToHtml(errors.ToArray(), ingnoreProperties));
            return buffer.ToString();
        }
    }
}
