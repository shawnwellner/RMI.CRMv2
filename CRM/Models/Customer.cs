using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using CRM.Domain;

namespace CRM.Models {
    public class CustomerFormModel : DetailedUser {
        public Pagination PageInfo { get; set; }

        /*public static List<SelectListItem> GetListofDispositions(int dispositionTypeId) {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = "- Select Disposition -", Value = "" });
            foreach (var disp in Disposition.GetDispostionByTypeId(LoginUser.CurrentUser.VerticalId, dispositionTypeId)) {
                list.Add(new SelectListItem() { Text = disp.Disposition, Value = disp.DispositionId.ToString() });
            }
            return list;
        }*/
        
        public int? VerticalId { get; set; }
        public string VerticalName { get; set; }
        public int? ClientVerticalRelId { get; set; }
        public int? VCLRelId { get; set; }
        public int? OfficeLocationId { get; set; }
        public int? PrevDispositionId { get; set; }
        public int? DispositionId { get; set; }
        private string _disposition;
        public string Disposition {
            get { return _disposition; }
            set {
                _disposition = value;
                this.DispositionId = CRM.Domain.Disposition.LookupDisposition(this.VerticalId.ToInt(), this._disposition);
            }
        }


        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? FireHostLeadId { get; set; }
        public int? Five9PostQueueId { get; set; }
        public string LeadSource { get; set; }
        public bool? Transfered { get; set; }
        public double? Distance { get; set; }
        public bool? NotQualified { get; set; }
        public bool? AllowEmailTransfer { get; set; }
        [NotTaxonomy]
        public List<QuestionInput> ListOfAnswers { get; set; }

        public override String ToString() {
            // return new DataContractJsonSerializer(typeof(CustomerFormModel)).ToString();
            return new JavaScriptSerializer().Serialize(this);
        }
    }
}