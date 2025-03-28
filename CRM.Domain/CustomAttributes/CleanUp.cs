using System;

namespace CRM.Domain {
    public class Cleanup : Attribute {
        public enum CleanupTypes {
            UpperCase,
            LowerCase,
            TitleCase,
            PhoneNumber,
            Url
        }

        public CleanupTypes CleanupType { get; set; }
        public bool Required { get; set; }
        public string RegularExpression { get; set; }
    }
}
