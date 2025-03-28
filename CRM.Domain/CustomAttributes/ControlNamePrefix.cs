using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain {
    public class ControlNamePrefix : Attribute {
        public string NamePrefix { get; private set; }

        public ControlNamePrefix(string namePrefix) {
            this.NamePrefix = namePrefix;
        }
    }
}
