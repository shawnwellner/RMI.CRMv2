using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.CustomExceptions {
    public class ControlledException : Exception {
        public bool SendEmail { get; set; }
        public Exception OriginalException { get; set; }
        public string AppendMessage { get; set; }

        public ControlledException(string message)
            : this(message, null) {
        }

        public ControlledException(string message, bool sendExceptionEmail)
            : this(message, null) {
                this.SendEmail = sendExceptionEmail;
        }

        public ControlledException(string message, Exception orgException) : base(message) {
            this.OriginalException = orgException;
            this.SendEmail = orgException != null;
        }

        public ControlledException(string friendlyMessage, string errorMessage, Exception orgException)
            : base(friendlyMessage, orgException) {
                this.AppendMessage = errorMessage;
                this.OriginalException = orgException;
                this.SendEmail = true;
        }
    }
}
