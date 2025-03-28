using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain {
    public class ReportResults {
        public Pagination PageInfo { get; set; }
        public dynamic[] Report { get; set; }
    }
}
