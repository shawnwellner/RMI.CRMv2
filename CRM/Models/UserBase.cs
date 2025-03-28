using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CRM.Domain;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Linq;

namespace CRM.Models {
    #region Enums
	public enum DialogTypes {
		Validation,
		PrescriptionRefills,
		StrollHealth,
		Error
	}

    public enum PageViews {
        Client = 1,
        CallCenter = 2,
        Customer = 3
    }
    #endregion

    #region Interfaces
    public interface IUserDetails {
        string City { get; set; }
        string State { get; set; }
        string Zip { get; set; }
        string Notes { get; set; }
    }
    #endregion

    public class UserBase {
        public UserBase() {
            this.Enabled = true;
        }

        [Display(Order = 0, Name = "UserId")]
        public int? UserId { get; set; }
        
        [Required(ErrorMessage = "First Name is a Required field.")]
        [DataType(DataType.Text)]
        [Display(Order = 1, Name = "FirstName")]
        [RegularExpression(@"^[a-zA-Z ,'-.]+$", ErrorMessage = "First Name must not contain any numbers or special characters.")]
        [UIHint("Textbox")]
        public virtual string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is a Required field.")]
        [DataType(DataType.Text)]
        [Display(Order = 2, Name = "LastName")]
        [RegularExpression(@"^[a-zA-Z ,'-.]+$", ErrorMessage = "Last Name must not contain any numbers or special characters.")]
        [UIHint("Textbox")]
        public virtual string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[A-Za-z0-9._%+-]*@[A-Za-z0-9.-]*\.[A-Za-z0-9-]{2,}$", ErrorMessage = "Email address is improperly formatted.")]
        [Display(Order = 11, Name = "Email")]
        [UIHint("Textbox")]
        public virtual string Email { get; set; }

        [Required(ErrorMessage = "Phone is a Required field.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^[01]?[- .]?\(?[2-9]\d{2}\)?[- .]?\d{3}[- .]?\d{4}$", ErrorMessage = "Phone must not contain letters or special characters.")]
        [Display(Order = 9, Name = "Phone")]
        public string Phone { get; set; }

        public bool Enabled { get; set; }
        
        [CloneAlias("UserTypeId")]
        public UserTypes UserType { get; set; }
        
        [Required(ErrorMessage = "User Role is a required field.")]
        public int UserTypeId { get; set; }
    }

    public class LoginUserModel : UserBase {

        [RegularExpression(@"[a-zA-Z\- ]+")]
        public override string FirstName { get; set; }

        [RegularExpression(@"[a-zA-Z\- ]+")]
        public override string LastName { get; set; }

        [Required(ErrorMessage = "UserName is a Required field.")]
        [DataType(DataType.Text)]
        [Display(Order = 1, Name = "UserName")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is a Required field.")]
        [DataType(DataType.Password)]
        [RegularExpression("^.{8,32}$", ErrorMessage = "Password must be between 8 and 32 characters long.")]
        [Display(Order = 12, Name = "Password")]
        public string Password { get; set; }

        //[Required(ErrorMessage = "Confirm Password is a Required field.")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password")]
        [Display(Order = 13, Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Email is a Required field.")]
        public override string Email { get; set; }
    }


    public class DetailedUserLogin : LoginUserModel, IUserDetails {
        [Required(ErrorMessage = "City is a Required field.")]
        [DataType(DataType.Text)]
        [RegularExpression(@"^[a-zA-Z ,'-.]+$", ErrorMessage = "City must not contain any numbers or special characters.")]
        [Display(Order = 6, Name = "City")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is a Required field.")]
        [DataType(DataType.Text)]
        [Display(Order = 7, Name = "State")]
        [CloneAlias("StateAbbr")]
        public string State { get; set; }

        [Required(ErrorMessage = "Zip is a Required field.")]
        [DataType(DataType.PostalCode)]
        [RegularExpression(@"^\d{5}(?:[-\s]\d{4})?$", ErrorMessage = "Zip Code must not contain any letters or special characters and must be a 5 digit zip code.")]
        [Display(Order = 8, Name = "ZipCode")]
        [CloneAlias("ZipCode")]
        public string Zip { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Order = 16, Name = "Notes")]
        [UIHint("textArea")]
        public string Notes { get; set; }
    }
    
    public class DetailedUser : UserBase, IUserDetails
    {
        [Required(ErrorMessage = "City is a Required field.")]
        [DataType(DataType.Text)]
        [RegularExpression(@"^[a-zA-Z ,'-.]+$", ErrorMessage = "City must not contain any numbers or special characters.")]
        [Display(Order = 6, Name = "City")]
        [UIHint("Textbox")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is a Required field.")]
        [DataType(DataType.Text)]
        [Display(Order = 7, Name = "State")]
        [UIHint("Textbox")]
        [CloneAlias("StateAbbr")]
        public string State { get; set; }

        [Required(ErrorMessage = "Zip is a Required field.")]
        [DataType(DataType.Text)]
        [RegularExpression(@"(^\d{5}(-\d{4})?$)|(^[ABCEGHJKLMNPRSTVXY]{1}\d{1}[A-Z]{1} *\d{1}[A-Z]{1}\d{1}$)", ErrorMessage = "Zip Code must not contain any letters or special characters and must be a 5 digit zip code.")]
        [Display(Order = 8, Name = "ZipCode")]
        [UIHint("Textbox")]
        [CloneAlias("ZipCode")]
        public string Zip { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Order = 16, Name = "Notes")]
        [UIHint("textArea")]
        public string Notes { get; set; }

        [ScriptIgnoreAttribute()]
        public static List<SelectListItem> StateDropdownList {
            get { return Utility.StateList.ToDropdownList("Choose a State *"); }
        }

        public static List<SelectListItem> VerticalDropDownList(LoginUser user) {
            IDictionary<int, string> list = Vertical.GetMyVerticals(user).ToDictionary(v => v.VerticalId, v => v.VerticalName);
            if (list.Count > 1) {
                return list.ToDropdownList("Select Vertical", "");
            }
            return null;
        }
    }
}