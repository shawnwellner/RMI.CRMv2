using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.CustomExceptions {
	public class JavaScriptException : Exception {
		
		public JavaScriptException(string message, string stacktrace)
            : base(message, null) {
			this.StackTrace = stacktrace;
		}

		public new string StackTrace { get; private set; }
	}
}
