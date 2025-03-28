using CRM.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRM.Models {
    public class ClientAnswerModel {
        public string CompanyName { get; set; }
        public int ClientId { get; set; }
        public int ClientVerticalRelId { get; set; }
        public int VCLRelId { get; set; }
        public int OfficeLocationId { get; set; }
        public bool AllowEmailTransfer { get; set; }
        public Customer Customer { get; set; }
        public List<Question<InputModel>> ListOfQuestions { get; set; }
        public List<QuestionInput> ListOfClientAnswers { get; set; }
    }
}