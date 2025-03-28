using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CRM.Domain {
    public class ReadOnlyList<T> : IReadOnlyList<T> {
        private IReadOnlyList<T> _list;

        public static implicit operator ReadOnlyList<T>(List<T> list) {
            return new ReadOnlyList<T>(list);
        }

        public ReadOnlyList() {
            this._list = new List<T>(0).AsReadOnly();
        }

        public ReadOnlyList(List<T> list) {
            this._list = list.AsReadOnly();
        }
        
        public T this[int index] {
            get { return this._list[index]; }
        }

        public int Count 
        {
            get { return this._list.Count; }
        }

        public IEnumerator<T> GetEnumerator() {
            return this._list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this._list.GetEnumerator();
        }

        public bool Any(Func<T, bool> selector) {
            return _list.Any(selector);
        }

        public T First() {
            return _list.First();
        }

        public T First(Func<T, bool> selector) {
            return _list.First(selector);
        }
    }

    public class ProviderList : ReadOnlyList<OfficeLocation> {
        #region Constructors
        public ProviderList() :this(null) {
            
        }
        public ProviderList(Place customerLocation) {
            var test = new ReadOnlyList<OfficeLocation>();

            //this._list = new List<OfficeLocation>(0).AsReadOnly();
            this.NotQualifiedAnswers = new List<QuestionInput>(0);
            this.CustomerLocation = customerLocation;
        }

        internal ProviderList(Place customerLocation, List<OfficeLocation> list, List<QuestionInput> notQualifiedAnswers) :
            base(list.GroupBy(p => p.OfficeLocationId)
                             .Select(p => p.FirstOrDefault())
                             .OrderByDescending(p => p.IsPrimaryOffice)
                             .ThenBy(p => p.SortPriority.IsNull(9999))
                             .ThenBy(p => p.Distance)
                             .ToList()) {

            /*this._list = list.GroupBy(p => p.OfficeLocationId)
                             .Select(p => p.FirstOrDefault())
                             .OrderByDescending(p => p.IsPrimaryOffice)
                             .ThenBy(p => p.SortPriority.IsNull(9999))
                             .ThenBy(p => p.Distance)                             
                             .ToList().AsReadOnly();*/
            this.NotQualifiedAnswers = notQualifiedAnswers;
            this.CustomerLocation = customerLocation;
        }
        #endregion

        public ReadOnlyList<QuestionInput> NotQualifiedAnswers { get; private set; }

        public Place CustomerLocation { get; private set; }
    }

    public class Provider : Client {        
        //public bool? IsQualified { get; set; }

        public static OfficeLocation GetProviderByVCLRelId(int verticalId, int? userId, int clientVerticalRelId, int vclRelId, string zipCode, string city, string state) {
            List<QuestionInput> list = new List<QuestionInput>(0);
            return GetListOfProviders(verticalId, userId, zipCode, city, state, clientVerticalRelId, vclRelId, list).SingleOrDefault();
        }

        public static ProviderList GetListOfProviders(int? verticalId, int? userId, Place place, int? clientVerticalRelId, int? vclRelId, List<QuestionInput> listOfAnswers) {
            List<sp_GetListOfProviders_Result> results;
            List<QuestionInput> inputRules = new List<QuestionInput>();
            List<v_TargetTypeFilters> targetTypes;
            QuestionInput insuranceAnswer = listOfAnswers.Where(a => a.IsInsurance).SingleOrDefault();
            int? insuranceId = insuranceAnswer != null ? insuranceAnswer.InsuranceId : null;
            bool ignoreRadius;
            
            using (CRMEntities context = new CRMEntities()) {
                targetTypes = context.v_TargetTypeFilters.Where(f => (f.FilterTypeId == 1 && place.ZipCode.StartsWith(f.FilterValue)) || (f.FilterTypeId == 2 && f.FilterValue == place.State)).ToList();
                results = context.sp_GetListOfProviders(verticalId, userId, place.State, place.ZipCode, insuranceId, vclRelId).ToList();

                List<sp_GetListOfProviders_Result> vailSummit = results.Where(r => r.VerticalId == 2 && r.CompanyName.Matches("Vail Summit")).ToList();
                if (vailSummit.Count > 0) { //Band Aid Fix for Question 2 for OCA [SW] 8/2/2017
                    bool spinePainOnly = !listOfAnswers.Any(a => a.QuestionId == 7 &&
                                                             a.InputTypeId == 5 &&
                                                             a.Answer.HasValue() &&
                                                             a.ClientInputTypeRelId != 18);
                    if (spinePainOnly) {
                        bool isMedicare = context.Insurance.Any(i => i.Id == insuranceAnswer.InsuranceId && (i.Name.Contains("Medicare") || i.Description.Contains("Medicare")));
                        vailSummit.ForEach(r => r.AcceptsInsurance = !isMedicare);
                    }
                }

                var allRules = (from r in context.v_QuesRulesAndFilters
                                where r.VerticalId == verticalId
                                group r by r.ClientInputTypeRelId into rules
                                select rules).ToList();
                QuestionInput input;

                foreach (IGrouping<int, v_QuesRulesAndFilters> item in allRules) {
                    
                    //rule = item.First();
                    input = listOfAnswers.Single(a => a.ClientInputTypeRelId == item.Key);
                    input.CustomerPlace = place;
                    input.Rules = item.ToList();
                    input.Rules.ForEach(r => r.Input = input);
                    input.Children = listOfAnswers.Where(l => input.ClientInputTypeRelId == l.ParentClientInputTypeRelId).ToList();
                    foreach (var r in input.Rules) {
                        r.Input = input;
                        r.Filters = item.Where(i => i.FilterTypeId != null && i.InputRuleRelId == r.InputRuleRelId).Select(f => f.CloneAs<v_QuesInputRuleFilters>()).ToList();
                    }
                    inputRules.Add(input);
                }
            }


            OfficeLocation loc;
            List<OfficeLocation> providers = new List<OfficeLocation>(results.Count);
            List<QuestionInput> removeRules = new List<QuestionInput>();
            List<QuestionInput> notQualfied = new List<QuestionInput>();
            Dictionary<int, Provider> clients = new Dictionary<int, Provider>();

            if (vclRelId != null) { notQualfied.Clear(); }

            foreach (var result in results.Where(r => r.AcceptsInsurance)) {
                loc = result.CloneAs<OfficeLocation>();
                loc.AgentNotification = result.OfficeNotification;
                if(clients.ContainsKey(result.ClientId)) {
                    loc.Provider = clients[result.ClientId];
                } else {
                    loc.Provider = result.CloneAs<Provider>();
                    clients.Add(result.ClientId, loc.Provider);
                }
                
                ignoreRadius = false;

                if (!loc.ValidateRules(inputRules, notQualfied)) { continue; }

                loc.SetDistance(place);
                if (((TargetTypeFlags)result.TargetTypes & TargetTypeFlags.National) != 0) {
                    //No need to check radius here
                } else if (((TargetTypeFlags)result.TargetTypes & TargetTypeFlags.Radius) != 0) {
                    if (((TargetTypeFlags)result.TargetTypes & TargetTypeFlags.State) != 0) {
                        ignoreRadius = targetTypes.Any(t => t.ClientVerticalRelId == result.ClientVerticalRelId && (TargetTypeFlags)t.TargetTypeFlag == TargetTypeFlags.State && (TargetTypeFilterTypes)t.FilterTypeId == TargetTypeFilterTypes.State && t.FilterValue == place.State);
                    }
                    if (!ignoreRadius && ((TargetTypeFlags)result.TargetTypes & TargetTypeFlags.Zipcode) != 0) {
                        ignoreRadius = targetTypes.Any(t => t.ClientVerticalRelId == result.ClientVerticalRelId && (TargetTypeFlags)t.TargetTypeFlag == TargetTypeFlags.Zipcode && (TargetTypeFilterTypes)t.FilterTypeId == TargetTypeFilterTypes.ZipCode && place.ZipCode.StartsWith(t.FilterValue));
                    }

                    if (!ignoreRadius && !loc.InRadius) { continue; }
                }

                providers.Add(loc);
            }

            //providers.RemoveAll(p => notQualfied.Any(n => n.Rules.Any(r => !r.IsQualified && r.Equals(p, true))));
            
            //providers.RemoveAll(p => notQualfied.Any(n => n.Rules.Any(r => !r.IsQualifiedByLocation(p))));
            
            if (providers.Count > 0) {
                notQualfied.Clear();
            } else if (results.Any(r => !r.AcceptsInsurance) && insuranceAnswer != null) {
                notQualfied.Add(insuranceAnswer);
            }

            return new ProviderList(place, providers, notQualfied);
        }


        public static ProviderList GetListOfProviders(int? verticalId, int? userId, string zipcode, string city, string state, int? clientVerticalRelId, int? officeLocationId, List<QuestionInput> listOfAnswers) {
             //-- 1) get zip code info ---
            Place place = ZipCodeInfo.GetZipCodeInfo(zipcode, city, state);
            return GetListOfProviders(verticalId, userId, place, clientVerticalRelId, officeLocationId, listOfAnswers);
        }
    }
}
