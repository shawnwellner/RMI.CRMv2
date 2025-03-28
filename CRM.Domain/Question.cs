using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Domain {
    #region Enums
    public enum InputTypes {
        text = 1,
        textarea = 2,
        radio = 3,
        yesno = 4,
        checkbox = 5,
        dropdown = 6,
        date = 7,
        time = 8,
        datetime = 9,
        typeahead = 10
    }
    public enum QualifyingTypes {
        Equal = 1,
        LessThan = 2,
        LessThanOrEqual = 3,
        GreaterThan = 4,
        GreaterThanOrEqual = 5,
        regex = 6
    }

    public enum UnitTypes {
        Days = 1,
        Months = 2,
        Years = 3
    }
    #endregion

    #region Classes
    public partial class v_QuesInputRuleFilters : IEquatable<Place> {
        public bool Equals(Place other) {
            switch (this.FilterTypeId) {
                case 1: //Zipcode Filter Type
                    return this.FilterValue.Matches(string.Format("^{0}", other.ZipCode));
                case 2: //State Filter Type
                    return this.FilterValue.Equals(other.State, StringComparison.OrdinalIgnoreCase);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public partial class v_QuesRulesAndFilters {
        internal QuestionInput Input { get; set; }

        [CloneAlias("UnitTypeId")]
        internal UnitTypes? UnitType { get; set; }
        internal List<v_QuesInputRuleFilters> Filters { get; set; }

        private bool? _isQualified = null;
        public bool IsQualified {
            get {
                if (this._isQualified == null) {
                    if (this.QualifyingRule.HasValue() && this.Input.Answer.HasValue()) {
                        this.Input.InputType = (InputTypes)this.InputTypeId;
                        switch (this.Input.InputType) {
                            case InputTypes.date:
                            case InputTypes.datetime:
                                DateTime? date = ParseAnswer(this.Input.Answer);
                                if (this.UnitTypeId.HasValue && !this.UnitType.HasValue) {
                                    this.UnitType = (UnitTypes)this.UnitTypeId;
                                }
                                this._isQualified = Validate(date, this.UnitType, (QualifyingTypes)this.QualifyingTypeId, this.QualifyingRule.ToInt());
                                break;
                            default:
                                this._isQualified = this.Input.Answer.RexExEquals(this.QualifyingRule);
                                break;
                        }
                        if (this.Filters.Count > 0 && this._isQualified == false) {
                            this._isQualified = !this.Filters.Any(f => f.Equals(this.Input.CustomerPlace));
                        }
                    } else if (this.Filters.Count > 0) {
                        this._isQualified = this.Filters.Any(f => f.Equals(this.Input.CustomerPlace));
                    } else {
                        this._isQualified = true;
                    }
                }
                return this._isQualified.Value;
            }
            internal set { this._isQualified = value; }
        }

        internal void ResetQualified() {
            this._isQualified = null;
        }

        private static bool Validate(DateTime? date, UnitTypes? uType, QualifyingTypes? qType, int units) {
            if (date.HasValue && uType.HasValue && qType.HasValue) {
                DateTime now = DateTime.Now;
                DateTime modDate;
                switch (uType) {
                    case UnitTypes.Days:
                        now = new DateTime(now.Year, now.Month, now.Day);
                        modDate = date.Value.AddDays(units);
                        break;
                    case UnitTypes.Months:
                        now = new DateTime(now.Year, now.Month, 1);
                        modDate = date.Value.AddMonths(units);
                        break;
                    case UnitTypes.Years:
                        now = new DateTime(now.Year, now.Month, 1);
                        modDate = date.Value.AddYears(units);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                switch (qType) {
                    case QualifyingTypes.Equal:
                        return now.Subtract(modDate).TotalDays == 0;
                    case QualifyingTypes.GreaterThan:
                        return now.Subtract(modDate).TotalDays > 0;
                    case QualifyingTypes.GreaterThanOrEqual:
                        return now.Subtract(modDate).TotalDays >= 0;
                    case QualifyingTypes.LessThan:
                        return now.Subtract(modDate).TotalDays < 0;
                    case QualifyingTypes.LessThanOrEqual:
                        return now.Subtract(modDate).TotalDays <= 0;
                    default:
                        throw new NotImplementedException();
                }
            }
            return false;
        }

        internal static DateTime? ParseAnswer(string answer) {
            try {
                string[] groups;
                string dateAnswer = answer;
                int month, day, year;
                if (dateAnswer.Matches(@"(\d{2})/(\d{2})/(\d{4})", out groups)) {
                    month = groups[1].ToInt();
                    day = groups[2].ToInt();
                    year = groups[3].ToInt();
                } else if (dateAnswer.Matches(@"(\d{2})-(\d{4})", out groups)) {
                    month = groups[1].ToInt();
                    year = groups[2].ToInt();
                    day = 1;
                } else {
                    return null;
                }
                return new DateTime(year, month, day);
            } catch (Exception ex) {
                ex.ToString();
            }
            return null;
        }
        
        public bool Equals(OfficeLocation loc) {
            return this.Equals(loc, false);
        }

        public bool Equals(OfficeLocation loc, bool absolute) {
            return loc.ClientId == this.ClientId &&
                   loc.ClientVerticalRelId == (absolute ? this.ClientVerticalRelId : this.ClientVerticalRelId.IsNull(loc.ClientVerticalRelId)) &&
                   loc.OfficeLocationId == (absolute ? this.OfficeLocationId :  this.OfficeLocationId.IsNull(loc.OfficeLocationId));
        }
    }

    public class QuestionInput : v_QuestionInputs {
        public QuestionInput() : this(null, null) { }

        public QuestionInput(QuestionInput answer) : this(answer, null) {}

        public QuestionInput(QuestionInput answer, List<QuestionInput> list) {
            if (answer != null) {
                this.QuestionId = answer.QuestionId;
                this.ClientRelId = answer.ClientRelId;
                this.ClientVerticalRelId = answer.ClientVerticalRelId;
                this.ClientInputTypeRelId = answer.ClientInputTypeRelId;
                this.OfficeLocationId = answer.OfficeLocationId;
                this.InputTypeId = answer.InputTypeId;
                this.InputType = (InputTypes)answer.InputTypeId;
                this.QuestionId = answer.QuestionId;
                this.Required = answer.Required;
                this.AnswerIndex = answer.AnswerIndex;
                this.Answer = answer.Answer;
                this.Rules = answer.Rules;
            }
            if(this.Rules == null) {
                this.Rules = new List<v_QuesRulesAndFilters>();
            }
            this.Children = list != null ? list.Where(l => answer.ClientInputTypeRelId == l.ParentClientInputTypeRelId).ToList() : new List<QuestionInput>();
        }

        internal Place CustomerPlace { get; set; }
        public int AnswerIndex { get; set; }
        public int QuestionId { get; set; }
        public List<QuestionInput> Children { get; set; }
        [CloneAlias("InputTypeId")]
        public new InputTypes InputType { get; set; }
        
        public string Answer { get; set; }
        public bool Required { get; internal set; }
        public bool? Qualified { get; set; }
        public List<string> ListOfOptions { get; set; }

        public IDictionary<string, string> InputAttributes { get; set; }
        public List<v_QuesRulesAndFilters> Rules { get; set; }
        
        public bool ToggleYesNo(OfficeLocation loc) {
            if (this.InputType == InputTypes.yesno) {
                QuestionInput input = new QuestionInput(this, this.Children);
                input.CustomerPlace = this.CustomerPlace;

                input.Answer = this.Answer == "Yes" ? "No" : "Yes";
                var locationRules = input.Rules.Where(r => r.ClientVerticalRelId.IsNull(loc.ClientVerticalRelId) == loc.ClientVerticalRelId && r.OfficeLocationId.IsNull(loc.OfficeLocationId) == loc.OfficeLocationId);

                foreach (var r in locationRules) {
                    r.ResetQualified();
                    r.Input = input;
                }

                this.Qualified = locationRules.Any(r => r.IsQualified);
                if(this.Qualified == true) {
                    this.Children.ForEach(c => c.Rules.Where(r => r.Equals(loc)).ToList().ForEach(r => r.IsQualified = true));
                }
                return this.Qualified == true;
            }
            return this.IsQualified(loc);
        }

        public bool IsQualified(OfficeLocation loc) {
            if (loc == null) {
                this.Qualified = !this.Rules.Any(r => r.IsQualified == false) && _acceptsInsurance != false;
                
            } else {
                this.Qualified = this.IsQualified(loc.ClientVerticalRelId, loc.OfficeLocationId);
            }
            return this.Qualified.Value;
        }

        public bool IsQualified(int? clientVerticalRelId, int? officeLocationId) {
            int cvrId = clientVerticalRelId.ToInt();
            int olId = officeLocationId.ToInt();

            return !this.Rules.Any(r => r.ClientVerticalRelId.IsNull(cvrId) == cvrId &&
                                   r.OfficeLocationId.IsNull(olId) == olId &&
                                   r.IsQualified == false) && _acceptsInsurance != false;
        }

        public bool IsInsurance {
            get { return this.InputTypeRelId == 10; }
        }

        public int? InsuranceId {
            get {
                if (this.IsInsurance && this.Answer.HasValue()) {
                    if (this.Answer.Matches(@"^\d+$")) {
                        return this.Answer.ToNullableInt();
                    } else {
                        KeyValuePair<int, string> item = Question.InsuranceList.Where(i => i.Value.Equals(this.Answer, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                        if (item.Value == null) {
                            return QuestionInput.AddInsurance(this.Answer);
                        } else {
                            return item.Key;
                        }
                    }
                }
                return null;
            }
        }

        bool? _acceptsInsurance;
        internal void SetAcceptsInsurance(CRMEntities context, int? clientVerticalRelId, int? officeLocationId) {
            if (!this.IsInsurance) { return; }
            if (this._acceptsInsurance == null) {
                int insuranceId = this.InsuranceId.ToInt();
                this._acceptsInsurance = !(from ex in context.InsuranceException
                                           where !ex.Allow && ex.Enabled && ex.InsuranceId == insuranceId &&
                                           ex.ClientVerticalRelId == clientVerticalRelId && ex.OfficeLocationId == officeLocationId
                                           select ex).Any();
            }
        }

        private static int AddInsurance(string name) {
            using (CRMEntities context = new CRMEntities()) {
                Insurance insurance = new Insurance() {
                    Name = name.ToTitleCase(),
                    Description = string.Format("Auto Added - UserId: {0}", LoginUser.CurrentUser.UserId),
                    Validated = false,
                    Enabled = true
                };
                context.Insurance.Add(insurance);
                context.SaveChanges();
                return insurance.Id;
            }
        }

        private string _insuranceName = null;
        public string InsuranceName {
            get {
                int? insuranceId = this.InsuranceId;
                if (this._insuranceName.IsEmpty()) {
					Insurance insurance = Insurance.Find(this.InsuranceId);
					if(insurance != null) {
						this._insuranceName = insurance.Name;
					}
                }
                return this._insuranceName;
            }
        }
    }

	//TODO:  Fix this
    public class Question : Question<QuestionInput> {
        
    }

	//TODO:  Fix this...This was so I could use MVC QuestionModel Type and 
	public class Question<InputType> : v_Questions
        where InputType : QuestionInput {

        private static int _questionCount = 0;
        private static int? _prevVerticalId = -1;

        public int QuestionIndex { get; set; }
        public string QuestionText { get; set; }
        public List<InputType> ListOfAnswers { get; set; }
        public List<InputType> ListOfInputs { get; set; }

        public static IDictionary<int, string> InsuranceList {
            get {
                using (CRMEntities context = new CRMEntities()) {
                    return (from i in context.Insurance
                            where i.Enabled == true
                            orderby i.Name
                            select i).ToDictionary(i => i.Id, i => i.Name);
                }
            }
        }

        public bool IsQualified(int? clientVerticalRelId, int? officeLocationId) {
            return this.ListOfInputs.Any(i => i.IsQualified(clientVerticalRelId, officeLocationId));
        }

        private static List<Question<InputType>> GetListOfQuestions(int verticalId, int? clientVerticalRelId, int? officeLocationId, int? customerId, bool showAll) {
            using (CRMEntities context = new CRMEntities()) {
                return GetListOfQuestions(context, verticalId, clientVerticalRelId, officeLocationId, customerId, showAll);
            }
        }

        internal static List<Question<InputType>> GetListOfQuestions(CRMEntities context, int verticalId, int? clientVerticalRelId, int? officeLocationId, int? customerId, bool showAll) {
            List<v_QuesQualifyingRules> rules = null;
            var questions = (from q in context.sp_VerticalQuestions(verticalId, clientVerticalRelId, officeLocationId, customerId, showAll)
                             group q by new {
                                 QuestionId = q.QuestionId,
                                 ClientVerticalRelId = q.ClientVerticalRelId,
                                 OfficeLocationId = q.OfficeLocationId
                             } into ques
                             select new { Question = ques.Key, Inputs = ques.ToList() })
                             .AsEnumerable();
            if (showAll) {
                rules = (from r in context.v_QuesQualifyingRules
                         where r.ClientVerticalRelId == clientVerticalRelId && r.OfficeLocationId == officeLocationId
                         select r).ToList();
            }

            List<Question<InputType>> list = new List<Question<InputType>>();
            Question<InputType> question;
            InputType input;
            int count = _questionCount;
            int inputCount = 0;
            v_QuesQualifyingRules rule;
            sp_VerticalQuestions_Result result;
            foreach (var q in questions) {
                result = q.Inputs.First();
                question = result.CloneAs<Question<InputType>>();
                question.QuestionIndex = count;
                question.QuestionText = string.Format("{0}) {1}", ++count, result.Question);
                question.ListOfInputs = new List<InputType>();

                for (int i = 0; i < q.Inputs.Count; i++) {
                    input = q.Inputs[i].CloneAs<InputType>();
                    input.SetAcceptsInsurance(context, clientVerticalRelId, officeLocationId);

                    if (rules != null) {
                        if (null != (rule = rules.Where(r => r.ClientInputTypeRelId == input.ClientInputTypeRelId).SingleOrDefault())) {
                            input.AutoFill(rule);
                        }
                    }
                    input.QuestionId = q.Question.QuestionId;
                    input.Required = question.Required;
                    input.AnswerIndex = inputCount++;
                    input.InputAttributes = (from a in context.InputAttributes
                                             where a.Enabled && a.InputTypeRelId == input.InputTypeRelId
                                             select a).ToDictionary(k => k.Name, v => v.Value);
                    input.Answer = input.Answer ?? string.Empty;

                    if (input.InputType == InputTypes.radio) {
                        /*TODO: UNTESTED CODE
                         * For Radio Inputs, only add one input to the list of inputs, but fill the ListofOptions
                        */
                        throw new NotImplementedException();
                        /*input.ListOfOptions = (from o in q.Inputs
                                               orderby o.OptionSortOrder
                                               select o.OptionValue).ToList();
                        i = input.ListOfOptions.Count; //skip these inputs in the for loop above*/
                    }
                    question.ListOfInputs.Add(input);
                }
                list.Add(question);
            }

            _prevVerticalId = verticalId;
            return list;
        }

        public static List<Question<InputType>> GetListOfQuestions(int verticalId, int? clientVerticalRelId, int? officeLocationId, int? customerId) {
            _questionCount = 0;
            _prevVerticalId = -1;
            return GetListOfQuestions(verticalId, clientVerticalRelId, officeLocationId, customerId, true);
        }

        public static List<Question<InputType>> GetClientQuestions(int verticalId, int? clientVerticalRelId, int? officeLocationId, int? customerId) {
            return GetListOfQuestions(verticalId, clientVerticalRelId, officeLocationId, customerId, false);
        }

        public static List<Question<InputType>> GetVerticalQuestions(int verticalId, int? customerId) {
            _questionCount = 0;
            _prevVerticalId = -1;
            return GetListOfQuestions(verticalId, null, null, customerId, false);
        }
    }
    #endregion
}
