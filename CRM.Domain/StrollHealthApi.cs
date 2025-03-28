using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CRM.Domain {
    public class StrollHealthApi {
		public class Patient {
			public static implicit operator Patient(Customer cust) {
				return new Patient() {
					PartnerPatientID = cust.CustomerId,
					FirstName = cust.FirstName,
					LastName = cust.LastName,
					Email = cust.Email,
				};
			}

			public static explicit operator CustomerStrollHealth(Patient patient) {
				DateTime now = DateTime.Now;
				return new CustomerStrollHealth() {
					CustomerId = patient.PartnerPatientID,
					Status = null,
					exCreatedTime = null,
					exUpdatedTime = null,
					CreatedTime = now,
					UpdatedTime = now
				};
			}

			public Patient() {
				this.Partner = "BPCA";
			}

			[JsonProperty("partnerPatientID")]
			public int PartnerPatientID { get; set; }
			[JsonProperty("firstName")]
			public string FirstName { get; set; }
			[JsonProperty("lastName")]
			public string LastName { get; set; }
			[JsonProperty("email")]
			public string Email { get; set; }
			[JsonProperty("partner")]
			public string Partner { get; private set; }
			[JsonProperty("status")]
			public string Status { get; set; }
			[JsonProperty("createdTime")]
			public DateTime CreatedTime { get; set; }
			[JsonProperty("updatedTime")]
			public DateTime UpdatedTime { get; set; }

			public bool Update() {
				LoginUser user = LoginUser.CurrentUser;
				user.UserId = 1; //Set User as Administrator
				Customer cust = Customer.GetCustomerById(user, this.PartnerPatientID);
				return Update(cust);
			}

			internal bool Update(Customer cust) {
				try {
					if (cust == null) { throw new Exception($"CustomerId: {this.PartnerPatientID} was not found."); }
					using (CRMEntities context = new CRMEntities()) {
						CustomerStrollHealth record = context.CustomerStrollHealth.SingleOrDefault(c => c.CustomerId == this.PartnerPatientID);
						if (record == null) { //Not in CustomerStrollHealth Table
							record = (CustomerStrollHealth)this;
							context.CustomerStrollHealth.Add(record);
						} else if (this.Status.HasValue()) {
							record.Status = this.Status;
							record.exCreatedTime = this.CreatedTime;
							record.exUpdatedTime = this.UpdatedTime;
							record.UpdatedTime = DateTime.Now;
						} else {
							throw new Exception("StrollHealth Error - Status is Empty");
						}
						context.SaveChanges();
					}
					if(this.Status.Matches("SCHEDULED")) {
						return AddToFive9(cust);
					}
					return true;
				} catch(Exception ex) {
					Utility.LogError(ex.InnerException ?? ex);
				}
				return false;
			}
		}

		private static bool AddToFive9(Customer cust) {
			var data = new {
				listName = Settings.StrollHealthFive9List,
				lead = new {
					CrmId = cust.CustomerId,
					FirstName = cust.FirstName,
					LastName = cust.LastName,
					Phone = cust.Phone,
					InsuranceType = cust.HealthInsurance
				}
			};
			using(HttpClient client = new HttpClient()) {
				try {
					string json = data.ToJson();
					Task<HttpResponseMessage> task = client.PostAsync(Settings.Five9Service, json.ToJsonContent());
					task.Wait();
					HttpResponseMessage resp = task.Result;
					if(resp.IsSuccessStatusCode) {
						dynamic dict = task.Result.GetJsonResponse<ExpandoObject>();
						if(dict.Success) { return true; }
						throw new Exception(dict.FailureMessage ?? "Unknown Error");
					}
					throw new Exception($"HttpError: {resp.ReasonPhrase}\r\nStatusCode: {resp.StatusCode}\r\nUrl: {resp.RequestMessage.RequestUri}");
				} catch(Exception ex) {
					Utility.LogError(ex);
				}
				return false;
			}
		}

		public static bool PostCustomer(Customer cust) {
            return PostCustomer(cust, 0);
        }

        private static bool PostCustomer(Customer cust, int count) {
            try {
                //const string url = "https://provider.strollhealth.com/sh/simplePartnerJourney";
                using (HttpClient client = new HttpClient()) {
                    string json = ((Patient)cust).ToJson();

                    Task<HttpResponseMessage> task = client.PostAsync(Settings.StrollHealthApiUrl, new StringContent(json, Encoding.UTF8, "application/json"));
                    task.Wait();
					Patient patient = task.Result.GetJsonResponse<Patient>();
					return patient.Update(cust);
                }
            } catch (Exception ex) {
                if (count < 3) { //Retry on error
                    Thread.Sleep(1000 * ++count); //Increase the delay time if getting multiple errors in a row.
                    return PostCustomer(cust, count);
                }
                throw new Exception("Error occured posting to StrollHealth.", ex);
            }
        }
    }
}
