
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class LoginModel : IValidatableObject
    {
        [DataType(DataType.Text)]
        [Display(Order = 1, Name = "UserName")]
        [Required(ErrorMessage = "UserName is a Required field.")]
        [UIHint("Textbox")]
        public string UserName { get; set; }

        [DataType(DataType.Text)]
        [Display(Order = 2, Name = "Password")]
        [Required(ErrorMessage = "Password is a Required field.")]
        [UIHint("Textbox")]
        public string Password { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (UserName == null)
            {
                yield return new ValidationResult("UserName is a Required field.");
            }
        }
    }
}