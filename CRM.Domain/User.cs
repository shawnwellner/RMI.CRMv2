using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain {
    #region Enums
    public enum UserTypes {
        Administrator = 1,
        CallCenterManager = 2,
        CallCenterAgent = 3,
        StandardUser = 4,
        Client = 5,
        Child = 6,
        Customer = 7
    }
    public enum UserRoles {
        Administrator = 1,
        Authenticated = 2,
        AuthLevel1 = 3,
        AuthLevel2 = 4,
        AuthLLevel3 = 5,
        Web = 6
    }

    internal enum TaxonomyTypes {
        ParentId = 1,
        CompanyName = 2,
        ForwardPhone = 3,
        ForwardPhone2 = 4,
        Website = 5,
        Description = 6,
        Username = 7,
        Password = 8,
        Notes = 9,
        JavaScript = 10,
        Bypass = 11,
        AgentNotification = 12,
        ForceLogin = 13,
        Five9AgentId = 14,
        OfficeLocationId = 15,
        LeadSource = 16,
        Distance = 17,
        PatientCoordname = 18,
        DispositionId = 19,
        Transfer = 20,
        FirehostLeadId = 21,
        Five9PostQueueId = 22,
        FBPageId = 23,
        FBCampaignId = 24,
        FBCampaignName = 25,
        FBLeadId = 26,
        FBCreatedTime = 27,
        TransferDate = 32
    }
    #endregion
    public class UserBase {
        public int VerticalId { get; set; }
        public IDictionary<string, object> Taxonomy { get; set; }

        public static int? GetFive9AgentId(int userId) {
            using (CRMEntities context = new CRMEntities()) {
                return (from u in context.v_CallCenterUsers where u.UserId == userId select u.Five9AgentId).FirstOrDefault();
            }
        }
    }
}
