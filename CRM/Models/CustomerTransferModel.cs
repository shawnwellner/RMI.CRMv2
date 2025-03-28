using CRM.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CRM.Models {
    public class CustomerTransferModel : ClientAnswerModel {
        public bool? EmailTransfer { get; set; }
        public string OfficeName { get; set; }
        public string ForwardPhone { get; set; }
        public string ForwardPhone2 { get; set; }
        public string AgentNotification { get; set; }
        public double Distance { get; set; }
        public string LocationText {
            get { return string.Format("{0} | {1} Miles", this.OfficeName, this.Distance); }
        }
    }
}