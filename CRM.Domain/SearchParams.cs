using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain {
    public class SearchParams : Pagination {
        #region Constructors
        public SearchParams() : this(LoginUser.CurrentUser, null) {}
        public SearchParams(LoginUser user) : this(user, null)  {}
        public SearchParams(LoginUser user, int? userId) {
            this.LoginUserId = LoginUser.CurrentUser.UserId;
            //this.VerticalId = user.VerticalId;
            this.UserId = userId;
        }
        #endregion

        public int? LoginUserId { get; set; }
        [RegularExpression(@"^\d+$", ErrorMessage = "UserId can only contain numbers.")]
        public int? UserId { get; set; }
        public int? VerticalId { get; set; }
        public string CompanyName { get; set; }
        public string Website { get; set; }
        [DataType(DataType.EmailAddress)]
        [RegularExpression("^[A-Za-z0-9._%+-]*@[A-Za-z0-9.-]*\\.[A-Za-z0-9-]{2,}$", ErrorMessage = "Email address is improperly formatted.")]
        public string Email { get; set; }
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^[0-9*]{0,10}$", ErrorMessage = "Phone can only contain numbers and/or an asterix(*) for wild card searching.")]
        public string Phone { get; set; }
        public string LastName { get; set; }
        public bool Exporting { get; set; }
        public bool IncludeQuestions { get; set; }
        public bool ShowAll { get; set; }
        public bool? Transfered { get; set; }
    }
}
