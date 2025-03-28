using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Domain
{
    [Flags]
    public enum TargetTypeFlags {
        Zipcode = 1,
        State = 2,
        National = 4,
        Radius = 8,
    }

    public enum TargetTypeFilterTypes {
        ZipCode = 1,
        State = 2
    }

    public class OfficeLocation : v_OfficeLocations {
        public Provider Provider { get; set; }
        public int? SortPriority { get; set; }

        public int? CustomerId { get; set; }
        public bool? Transfered { get; set; }

        public bool IsPrimaryOffice { get; set; }
        public string ForwardPhone { get; set; }
        public string ForwardPhone2 { get; set; }
        public decimal MaxRadius { get; set; }
        public double Distance { get; set; }
        public bool InRadius { get; private set; }

        public string LocationText {
            get { return string.Format("{0} | {1} Miles", this.OfficeName, this.Distance.ToString("N0")); }
        }

        private Place _place = null;
        private Place Place {
            get {
                if (this._place == null) {
                    this._place = ZipCodeInfo.GetZipCodeInfo(this.OfficeZipCode, this.OfficeCity, this.OfficeState);
                }
                return this._place;
            }
        }

        public static int? GetVerticalIdByLocationId(int officeLocationId) {
            if (officeLocationId > 0) {
                using (CRMEntities context = new CRMEntities()) {
                    return (from o in context.v_OfficeLocations
                            where o.OfficeLocationId == officeLocationId
                            select o.VerticalId).SingleOrDefault();
                }
            }
            return null;
        }

        public string Polyline { get; private set; }

        public void SetDistance(Place customerPlace) {
            if (!this.OfficeLatitude.HasValue || !this.OfficeLongitude.HasValue) {
                this.OfficeLatitude = this.Place.Latitude.ToDecimal();
                this.OfficeLongitude = this.Place.Longitude.ToDecimal();
            }

            string polyline;
            this.Distance = ZipCodeInfo.GetDistance(customerPlace.Latitude.ToDouble(),
                                                    customerPlace.Longitude.ToDouble(),
                                                    this.OfficeLatitude.ToDouble(), 
                                                    this.OfficeLongitude.ToDouble(), out polyline);
            if (this.Distance > 0) { this.Polyline = polyline; }
            this.InRadius = this.Distance == 0 || (this.Distance.ToDecimal() < this.MaxRadius);
        }

        internal bool ValidateRules(List<QuestionInput> inputRules, List<QuestionInput> notQualfied) {
            //if (this.Provider.IsQualified == false) { return this.Provider.IsQualified.Value; }
            List<QuestionInput> childRules;
            QuestionInput childRule;
            List<QuestionInput> clientInputs = inputRules.Where(i => i.Rules.Any(r => r.ClientVerticalRelId == this.ClientVerticalRelId)).ToList();
            if (clientInputs.Count == 0) {
                clientInputs = inputRules.Where(i => i.Rules.Any(r => r.ClientVerticalRelId == null && !r.IsQualified)).ToList();
                clientInputs.ForEach(i => notQualfied.AddUnique(i));
                return !clientInputs.Any();
            } else {
                //foreach (QuestionInput input in clientInputs.Where(i => !notQualfied.Contains(i) && i.Rules.Any(r => !r.IsQualified))) {
                //foreach (QuestionInput input in clientInputs.Where(i => !notQualfied.Contains(i))) {
                foreach (QuestionInput input in clientInputs) {
                    v_QuesRulesAndFilters locRule = input.Rules.Where(r => r.Equals(this, true)).SingleOrDefault();
                    if(locRule != null) {
                        if(!locRule.IsQualified) {
                            notQualfied.AddUnique(input);
                            return false;
                        }
                        continue;
                    } else if (!input.IsQualified(this)) {
                        if (input.ParentClientInputTypeRelId != null) {
                            if (!clientInputs.Where(r => r.ClientInputTypeRelId == input.ParentClientInputTypeRelId).Any(r => r.ToggleYesNo(this))) {
                                notQualfied.AddUnique(input);
                                return false;
                            }
                        } else {
                            notQualfied.AddUnique(input);
                            return false;
                        }
                    }

                    if (input.Children.Count > 0) {
                        childRules = input.Children.Where(c => c.ClientVerticalRelId == this.ClientVerticalRelId).ToList();
                        if (!childRules.Any()) {
                            childRules = input.Children.Where(c => c.ClientVerticalRelId == null).ToList();
                        }
                        childRule = childRules.FirstOrDefault(r => !r.IsQualified(this));
                        if (childRule != null) {
                            if (!input.ToggleYesNo(this)) {
                                notQualfied.AddUnique(childRule);
                                //notQualfied.AddUnique(input);
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
        }
    }
}
