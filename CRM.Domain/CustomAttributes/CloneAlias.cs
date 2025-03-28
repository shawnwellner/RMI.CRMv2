using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain {
    public class CloneAlias : System.Attribute {
        public string Alias { get; private set; }

        public CloneAlias(string alias) {
            this.Alias = alias;
        }
    }
}
