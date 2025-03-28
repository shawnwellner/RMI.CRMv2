using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace CRM.Models
{
    public class ZipCodeModel
    {
        public String ClassName { get; set; }
        public List<KeyValuePair<int, string>> ListOfZipCodes { get; set; }
    }
}