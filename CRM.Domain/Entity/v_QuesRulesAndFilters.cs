//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CRM.Domain
{
    using System;
    using System.Collections.Generic;
    
    public partial class v_QuesRulesAndFilters
    {
        public int VCLRelId { get; set; }
        public int VerticalId { get; set; }
        public Nullable<int> ClientId { get; set; }
        public Nullable<int> ClientVerticalRelId { get; set; }
        public Nullable<int> OfficeLocationId { get; set; }
        public int ClientInputTypeRelId { get; set; }
        public int InputTypeRelId { get; set; }
        public int InputTypeId { get; set; }
        public Nullable<int> ParentClientInputTypeRelId { get; set; }
        public Nullable<int> InputRuleRelId { get; set; }
        public Nullable<int> QualifyingRuleId { get; set; }
        public Nullable<int> QualifyingTypeId { get; set; }
        public string QualifyingType { get; set; }
        public string QualifyingRule { get; set; }
        public Nullable<int> UnitTypeId { get; set; }
        public string Units { get; set; }
        public Nullable<int> FilterTypeId { get; set; }
        public string FilterValue { get; set; }
        public int QuestionId { get; set; }
    }
}
