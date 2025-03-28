using CRM.Domain.CustomExceptions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRM.Domain {
    public partial class Client {
        public int? VerticalId { get; private set; }
        public int? ClientVerticalRelId { get; private set; }

        public List<Client> LoginUsers { get; private set; }
        public Client PrevState { get; set; }

        public Client() {
            this.LoginUsers = new List<Client>();
        }

        public Client(List<Client> loginUsers) {
            this.LoginUsers = loginUsers;
        }

        public static Client Update(Client client, List<int> verticalIds) {
            if (!client.Latitude.HasValue || !client.Latitude.HasValue) {
                Place place = CRM.Domain.ZipCodeInfo.GetZipCodeInfo(client.ZipCode, client.City, client.StateAbbr);
                if (place != null) {
                    client.Latitude = decimal.Parse(place.Latitude);
                    client.Longitude = decimal.Parse(place.Longitude);
                }
            }

            using (CRMEntities context = new CRMEntities()) {
                string ids = verticalIds != null && verticalIds.Count > 0 ? string.Join(",", verticalIds.ToArray()) : null;

                Client uClient = context.sp_UpdateClient(client.ClientId, client.FirstName.ToTitleCase(), client.LastName.ToTitleCase(), client.Email.TryToLower(),
                                                            client.Phone.CleanPhoneNumber(), client.Address, client.Address2, client.City.ToTitleCase(), client.StateAbbr,
                                                            client.ZipCode, client.Latitude, client.Longitude, client.CompanyName,
                                                            client.ForwardPhone.CleanPhoneNumber(), client.ForwardPhone2.CleanPhoneNumber(), client.Website.TryToLower(),
                                                            client.Description, null, client.UserName, client.Password, client.Notes, client.AllowEmailTransfer, client.ConciergeModel, 
															client.AgentNotification, ids, client.Enabled).Single();

                if (uClient.ClientId <= 0) { throw new ControlledException("An unexpected error occured updating a client."); }
                foreach (Client child in client.LoginUsers) {
                    context.sp_UpdateClientLoginUser(uClient.ClientId, child.UserId, child.FirstName, child.LastName, child.UserName, child.Password,child.Email, child.Phone.CleanPhoneNumber(), child.Enabled);
                }

                if (client.PrevState != null) {
                    StringBuilder body = new StringBuilder();
                    if (client.PrevState.Enabled != uClient.Enabled) {
                        body.AppendFormat("</br>Enabled state changed from <b>{0}</b> to <b>{1}</b>", client.PrevState.Enabled, uClient.Enabled).AppendLine();
                    }
                    if (client.PrevState.OmmitFromList != uClient.OmmitFromList) {
                        body.AppendFormat("</br><b>Paused</b> state changed from <b>{0}</b> to <b>{1}</b>", client.PrevState.OmmitFromList, uClient.OmmitFromList).AppendLine();
                    }
                    if (body.Length > 0) {
                        Utility.SendEmail("noreply@responsemine.com", "CRM Client Update", string.Format(@"
The CRM client <b>{0}</b> has changed.
</br>
{1}
", uClient.CompanyName, body.ToString()), Settings.ClientChangeAlertRecipients);
                    }
                }
				return uClient;
            }
        }

        public static List<Client> Search(SearchParams search) {
            using (CRMEntities context = new CRMEntities()) {
                List<Client> list = context.sp_ClientSearch(search.UserId, search.CompanyName, search.Email, search.Phone, 1, int.MaxValue, search.ShowAll).ToList();

                foreach (Client client in list) {
                    client.SetLoginUsers(context);
                }
                return list;
            }
        }

        public static Client GetClientById(LoginUser user, int userId) {
            return Search(new SearchParams(user, userId)).SingleOrDefault();
        }

        private Client SetLoginUsers(CRMEntities context) {
            this.LoginUsers = (from c in context.v_ClientLogins
                               where c.ClientId == this.ClientId && c.UserId != c.ParentClientId
                               select c).AsEnumerable().Select(c => c.CloneAs<Client>()).ToList();
            return this;
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }
}
