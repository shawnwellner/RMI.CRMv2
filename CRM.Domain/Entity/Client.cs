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
    
    public partial class Client
    {
        public int ClientId { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string ForwardPhone { get; set; }
        public string ForwardPhone2 { get; set; }
        public string Website { get; set; }
        public string Notes { get; set; }
        public string JavaScript { get; set; }
        public bool OmmitFromList { get; set; }
        public bool AllowEmailTransfer { get; set; }
        public string AgentNotification { get; set; }
        public bool ByPass { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int UserTypeId { get; set; }
        public string UserType { get; set; }
        public int UserRoleId { get; set; }
        public string UserRole { get; set; }
        public Nullable<int> ParentId { get; set; }
        public int AddressId { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string StateAbbr { get; set; }
        public string ZipCode { get; set; }
        public Nullable<decimal> Latitude { get; set; }
        public Nullable<decimal> Longitude { get; set; }
        public System.DateTime CreatedTime { get; set; }
        public System.DateTime UpdatedTime { get; set; }
        public bool Enabled { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool ForceLogin { get; set; }
        public bool ConciergeModel { get; set; }
    }
}
