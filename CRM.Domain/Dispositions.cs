using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain {
    public class Disposition : v_Dispositions {
		public enum DispositionIds {
			TalkingToClient = 0,
			NoMRI = 22,
			TransferSuccess = 38,
			EmailTransfer = 39
		}

        public static List<v_Dispositions> GetDispostionByTypeId(int dispositionTypeId) {
            using (CRMEntities content = new CRMEntities()) {
                return (from d in content.v_Dispositions
                            //where (d.VerticalId & verticalId) == verticalId && d.DispositionTypeId == dispositionTypeId
                        where d.DispositionTypeId == dispositionTypeId
                        orderby d.Disposition
                        select d).ToList();
            }
        }

        public static int? LookupDisposition(int verticalId, string disposition) {
            using (CRMEntities content = new CRMEntities()) {
                return (from d in content.v_Dispositions
                        where (d.VerticalIdFlags & verticalId) == verticalId && d.Disposition == disposition
                        select d.DispositionId).SingleOrDefault();
            }
        }

        internal static int GetDispositionByQuestionId(CRMEntities context, int questionId) {
            return context.v_Dispositions.Single(d => d.QuestionId == questionId).DispositionId;
        }

		internal static int GetDefaultDisposition(CRMEntities context) {
            return context.v_Dispositions.Single(d => d.NotQualifiedDefault == true).DispositionId;
        }
    }
}
