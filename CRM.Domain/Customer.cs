using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Domain {
    #region CustomerSearchResults class
    /*public partial class CustomerSearchResult {
        public List<Customer> Customers { get; private set; }
        public Pagination PageInfo { get; private set; }

        internal CustomerSearchResult(List<Customer> results, bool includeQuestions) {
            if (results.Count > 0) {
				if(includeQuestions) {
					results.ForEach(c => c.FillQuestions());
				}
				this.Customers = results;
                this.PageInfo = results.First().CloneAs<Pagination>();
            } else {
                this.Customers = new List<Customer>(0);
                this.PageInfo = new Pagination(0, 0, 0, 0);
            }
        }
    }*/
    #endregion

    public partial class Customer {
		public delegate void OnCustomerCreateDelegate(Customer customer);
		public event OnCustomerCreateDelegate OnCustomerCreate;

		#region Constructors
		public Customer() { }
        
		public static implicit operator Customer(sp_CustomerSearch_Result results) {
			Pagination<Customer> pageInfo = results;
			return pageInfo.Items.SingleOrDefault();
		}

        /*private Customer(sp_CustomerSearch_Result customer, bool includeQuestions) {
            this.VerticalId = customer.VerticalId;
            this.CustomerId = customer.CustomerId;
            this.UserId = customer.CustomerId;
            this.ClientVerticalRelId = customer.ClientVerticalRelId;
            this.OfficeLocationId = customer.OfficeLocationId;
            this.VCLRelId = customer.VCLRelId;
            if (includeQuestions) { this.FillQuestions(); }
        }

        private Customer(v_Customers customer, bool includeQuestions) {
            this.VerticalId = customer.VerticalId;
            this.CustomerId = customer.CustomerId;
            this.UserId = customer.CustomerId;
            this.ClientVerticalRelId = customer.ClientVerticalRelId;
            this.OfficeLocationId = customer.OfficeLocationId;
            this.VCLRelId = customer.VCLRelId;
            if (includeQuestions) { this.FillQuestions(); }
        }*/
        #endregion
		
		public bool SentToStrollHealth { get; internal set; }
		public string Polyline { get; private set; }
        
        [JsonIgnore]
        public List<Question<QuestionInput>> ListOfQuestions { get; private set; }

        public void GetDirections() {
            string polyline;
            ZipCodeInfo.GetDistance(this.Latitude.ToDouble(),
                                    this.Longitude.ToDouble(),
                                    this.OfficeLatitude.ToDouble(),
                                    this.OfficeLongitude.ToDouble(), out polyline);
            this.Polyline = polyline;
        }

        internal void FillQuestions() {
            this.ListOfQuestions = Question.GetListOfQuestions(this.VerticalId, this.ClientVerticalRelId, this.OfficeLocationId, this.CustomerId);
        }

        public static void Update(Customer customer, int? five9AgentId) {
            Update(customer, five9AgentId, null, null);
        }

        public static void Update(Customer cust, int? five9AgentId, IDictionary<string, object> taxonomy, List<QuestionInput> answers) {
            if (!cust.Latitude.HasValue || !cust.Latitude.HasValue) {
                Place place = ZipCodeInfo.GetZipCodeInfo(cust.ZipCode, cust.City, cust.StateAbbr);
                if (place != null) {
                    cust.City = place.City;
                    cust.StateAbbr = place.State;
                    cust.Latitude = decimal.Parse(place.Latitude);
                    cust.Longitude = decimal.Parse(place.Longitude);
                }
            }
			bool creating = cust.UserId <= 0;
            using (CRMEntities context = new CRMEntities()) {
                if (cust.NotQualified == true) {
                    QuestionInput notQualified = answers.FirstOrDefault(a => a.Qualified == false);
                    if(notQualified != null) {
                        cust.DispositionId = Domain.Disposition.GetDispositionByQuestionId(context, notQualified.QuestionId);
                    } else {
                        cust.DispositionId = Domain.Disposition.GetDefaultDisposition(context);
                    }
                    if(cust.DispositionId.ToInt() <= 0) {
                        cust.DispositionId = null;
                        throw new Exception("Failed to find DispostionId by QuestionId");
                    }
                }

				int? userId = cust.UserId <= 0 ? (int?)null : cust.UserId;
				Customer user = context.sp_UpdateCustomer(userId, cust.VerticalId,
														  cust.FirstName.ToTitleCase(),
														  cust.LastName.ToTitleCase(),
														  cust.Email, cust.Phone.CleanPhoneNumber(),
														  cust.City.ToTitleCase(), cust.StateAbbr, cust.ZipCode,
														  cust.Latitude, cust.Longitude, cust.OfficeLocationId,
														  cust.DispositionId, cust.Distance, cust.LeadSource,
														  cust.FireHostLeadId, five9AgentId,
														  cust.Notes).SingleOrDefault();
                if (user == null) { throw new Exception("A problem occured updating Customer."); }
				cust.AutoFill<Customer>(user);

				if(creating && cust.OnCustomerCreate != null) {
					cust.OnCustomerCreate(cust);
				}

                UpdateCustomerAnswers(context, cust.UserId, answers);
                UpdateTaxonomy(context, cust.UserId, taxonomy);
				cust.SentToStrollHealth = false;

				/* We are no longer using StrollHealth */
				/*if (cust.NotQualified == true && cust.StrollHealthComplete == null && cust.DispositionId.ToInt() == (int)Domain.Disposition.DispositionIds.NoMRI) {
					cust.SentToStrollHealth = StrollHealthApi.PostCustomer(cust);
				}*/
            }
        }

        private static void UpdateTaxonomy(CRMEntities context, int userId, IDictionary<string, object> taxonomy) {
            string value;
            if (taxonomy != null && taxonomy.Count > 0) {
                foreach (string key in taxonomy.Keys) {
                    value = taxonomy[key].ToString(null);
                    context.sp_UpdateTaxonomy(userId, null, key, value);
                }
            }
        }

        public static void TransferCustomer(Customer customer) {
            using (CRMEntities context = new CRMEntities()) {
				Customer cust = context.sp_TransferCustomer(customer.VerticalId, customer.CustomerId, customer.Transfered, customer.PatientCoordName, customer.DispositionId, customer.VCLRelId).Single();
				customer.AutoFill(cust);
            }
        }

        public static void UpdateCustomerAnswers(int userId, IList<QuestionInput> answers) {
            using (CRMEntities context = new CRMEntities()) {
                UpdateCustomerAnswers(context, userId, answers);
            }
        }
        
        private static void UpdateCustomerAnswers(CRMEntities context, int userId, IList<QuestionInput> answers) {
            if (answers == null) { return; }
            foreach (QuestionInput a in answers) {
                if(a.IsInsurance && a.InsuranceId == null) {
                    a.Answer = a.Answer.ToTitleCase();
                }
                context.sp_UpdateCustomerAnswer(userId, a.ClientInputTypeRelId, a.Answer);
            }
        }

        public static Customer GetCustomerById(LoginUser loginUser, int userId) {
            return GetCustomerById(loginUser, userId, false);
        }

        public static Customer GetCustomerById(LoginUser loginUser, int userId, bool includeQuestions) {
			using (CRMEntities context = new CRMEntities()) {
				return GetCustomerById(context, loginUser, userId, includeQuestions);
			}
		}

        private static Customer GetCustomerById(CRMEntities context, LoginUser loginUser, int userId, bool includeQuestions) {
			Customer cust = context.sp_CustomerSearch(userId, loginUser.UserId, null, null, null, null, null, 1, 1).SingleOrDefault();

            if(cust != null && includeQuestions) {
				cust.FillQuestions();
            }
            return cust;
        }
		
		public static bool IsDuplicate(string phone, string email) {
            using (CRMEntities context = new CRMEntities()) {
                return context.sp_GetDuplicates(null, phone, email).Any();
            }
        }

        public static Customer GetDuplicates(int? userId, string phone, string email) {
            using (CRMEntities context = new CRMEntities()) {
				return context.sp_GetDuplicates(userId, phone, email).SingleOrDefault();
            }
        }

        
        public static Pagination<Customer> Search(SearchParams search, UserTypes userType) {
            using (CRMEntities context = new CRMEntities()) {

				Pagination<Customer> results = context.sp_CustomerSearch(search.UserId, search.LoginUserId, search.Email, search.Phone, search.LastName,
																		 search.VerticalId, search.Transfered, search.PageNum, search.MaxRows);
				
				if (userType == UserTypes.CallCenterAgent) {
                    results.RemoveAll(c => c.ClientByPass.ToBool());
                }
				if(search.IncludeQuestions) {
					results.Foreach(c => c.FillQuestions());
				}
				return results;
            }
        }

        public static ReportResults GetReport(string name, DateTime? startDate, DateTime? endDate, int? pageNum, int? maxRows, int? leadType) {
            Pagination pageInfo = null;
            var results = DBHelpers.ExecuteStoredProcedure<Pagination>(name, startDate, endDate, pageNum, maxRows, leadType, out pageInfo);
            return new ReportResults() {
                PageInfo = pageInfo,
                Report = results
            };
        }
    }
}
