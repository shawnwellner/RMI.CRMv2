using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CRM.Domain;
using Newtonsoft.Json;

namespace CRM.Models {
    public class ClientFormModel : DetailedUserLogin {
        public int? ClientId { get; set; }
        public List<int> VerticalIds { get; set; }

        [JsonIgnore]
        public string PrevStateJson { get; set; }

        public ClientFormModel() {
            this.LoginUsers = new List<LoginUserModel>(0);
        }

        internal ClientFormModel(IReadOnlyList<Client> children) {
            this.LoginUsers = new List<LoginUserModel>(children.Count);
            foreach (Client client in children) {
                this.LoginUsers.Add(client.CloneAs<LoginUserModel>());
            }
        }

        [Required(ErrorMessage = "Address is a Required field.")]
        [DataType(DataType.Text)]
        [Display(Order = 3, Name = "Address")]
        [UIHint("Textbox")]
        public string Address { get; set; }

        [DataType(DataType.Text)]
        [Display(Order = 4, Name = "Address2")]
        [UIHint("Textbox")]
        public string Address2 { get; set; }

        [Required(ErrorMessage = "Description is a Required field.")]
        [DataType(DataType.Text)]
        [Display(Order = 12, Name = "Description")]
        //[RegularExpression("^[a-zA-Z0-9'&-]+$", ErrorMessage = "Division Name can only contain numbers, letters, or the following characters(&-').")]
        public string Description { get; set; }

        public string AgentNotification { get; set; }

        [Required(ErrorMessage = "Forward Number is a Required field.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression("^[01]?[- .]?\\(?[2-9]\\d{2}\\)?[- .]?\\d{3}[- .]?\\d{4}$", ErrorMessage = "Forward Number must not contain letters or special cahracters.")]
        [Display(Order = 10, Name = "Forward Phone")]
        public string ForwardPhone { get; set; }

        [Required(ErrorMessage = "2ndForward Number is a Required field.")]//
        [DataType(DataType.PhoneNumber)]
        [RegularExpression("^[01]?[- .]?\\(?[2-9]\\d{2}\\)?[- .]?\\d{3}[- .]?\\d{4}$", ErrorMessage = "2nd Forward Number must not contain letters or special cahracters.")]
        [Display(Order = 11, Name = "2nd Forward Phone")]
        public string ForwardPhone2 { get; set; }

        [Required(ErrorMessage = "Company Name is a Required field.")]
        [DataType(DataType.Text)]
        [Display(Order = 1, Name = "CompanyName")]
        //[RegularExpression(@"^[a-zA-Z0-9 !@#&\*%$\.'\-]+$", ErrorMessage = "Company Name must not contain any numbers or special characters.")]
        public string CompanyName { get; set; }

        //[DataType(DataType.Text)]
        //[Display(Order = 2, Name = "Division")]
        //[RegularExpression("^[a-zA-Z0-9'&-]+$", ErrorMessage = "Division Name can only contain numbers, letters, or the following characters(&-').")]
        //public string Division { get; set; }

        [Required(ErrorMessage = "Website is a Required field.")]
        [DataType(DataType.Url)]
        [Display(Order = 3, Name = "Website")]
        [RegularExpression(@"^(?:http(?:s)?\:\/\/[a-zA-Z0-9\-]+(?:\.[a-zA-Z0-9\-]+)*\.[a-zA-Z]{2,6}(?:\/?|(?:\/[\w\-]+)*)(?:\/?|\/\w+\.[a-zA-Z]{2,4}(?:\?[\w]+\=[\w\-]+)?)?(?:\&[\w]+\=[\w\-]+)*)", ErrorMessage = "Website must not contain any numbers or special characters.")]
        public string Website { get; set; }

        public int TargetTypeId { get; set; }

        public bool AllowEmailTransfer { get; set; }
		public bool ConciergeModel { get; set; }
		public ZipCodeModel ZipPrefixes { get; set; }
        /*[ScriptIgnore()]
        public static List<SelectListItem> TargetTypeDropDownList
        {
            get {
                List<SelectListItem> list = new List<SelectListItem>();
                list.Add(new SelectListItem() {
                    Text = "Select Target Type",
                    Value = string.Empty
                });
                foreach (var item in Search.ClientTargetTypes) {
                    list.Add(new SelectListItem() {
                        Value = item.Key.ToString(),
                        Text = item.Value
                    });
                }
                return list;
            }
        }*/

        [JsonIgnore]
        public List<LoginUserModel> LoginUsers { get; set; }

        public override string ToString() {
            /*var properties = from p in this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                             where p.GetValue(this, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(this, null).ToString());

            return string.Join("&", properties.ToArray());
             */
            return JsonConvert.SerializeObject(this);
        }
    }
}