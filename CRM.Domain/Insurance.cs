using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain {
	public partial class Insurance {
		public static Insurance Find(int? insuranceId) {
			if(insuranceId == null) { return null; }
			return Insurance.Find(insuranceId.Value);
		}
		public static Insurance Find(int insuranceId) {
			using(CRMEntities context = new CRMEntities()) {
				return (from i in context.Insurance
						where i.Id == insuranceId
						select i).SingleOrDefault();
			}
		}
	}
}
