using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace CRM {
    public static class Settings {
        public static string ClientEmailTransferFrom {
            //"scheduling@backpaincentersofamerica.com"
            get { return ConfigurationManager.AppSettings["ClientEmailTransferFrom"]; }
        }

        public static string[] ClientEmailTemplateRecipients {
            get { return ConfigurationManager.AppSettings["ClientEmailTemplateRecipients"].Split(';'); }
        }

        public static string ClientEmailTemplateSubjectFormat {
            get { return ConfigurationManager.AppSettings["ClientEmailTemplateSubjectFormat"]; }
        }

        public static string FacebookToken {
            get { return ConfigurationManager.AppSettings["FacebookToken"]; }
        }

        public static string TrugreenAuthKey {
            get { return ConfigurationManager.AppSettings["TrugreenAuthKey"]; }
        }

        public static string FacebookChallangeToken {
            get { return ConfigurationManager.AppSettings["FacebookChallangeToken"]; }
        }
    }
}