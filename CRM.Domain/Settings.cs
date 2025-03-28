using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace CRM.Domain {
    public static class Settings {
		public static string ConciergeEmailTransferFrom {
			get { return ConfigurationManager.AppSettings["ConciergeEmailTransferFrom"]; }
		}

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

        public static string ClientChangeAlertRecipients {
            get { return ConfigurationManager.AppSettings["ClientChangeAlertRecipients"]; }
        }

		public static string Five9Service {
			get { return ConfigurationManager.AppSettings["Five9Service"]; }
		}

		public static string StrollHealthFive9List {
			get { return ConfigurationManager.AppSettings["StrollHealthFive9List"]; }
		}

		public static string GoogleApiKey {
			get { return ConfigurationManager.AppSettings["GoogleApiKey"]; }
		}

		public static string StrollHealthApiUrl {
			get { return ConfigurationManager.AppSettings["StrollHealthApiUrl"]; }
		}

	}
}