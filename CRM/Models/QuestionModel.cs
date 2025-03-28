using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CRM.Domain;
using System.Web.Mvc;
using System.Web.Routing;
using System.Text;

namespace CRM.Models {
    public class QuestionModel : Question<InputModel> {}

    public class InputModel : QuestionInput {
        private static List<SelectListItem> _listOfYears = null;
        private static List<SelectListItem> _listOfHeights = null;
        #region Constructors
        static InputModel() {
            _listOfYears = new List<SelectListItem>();
            _listOfHeights = new List<SelectListItem>();

            //List Of Years
            _listOfYears.Add(new SelectListItem() {
                Text = "Select Year",
                Value = string.Empty
            });
            int year = DateTime.Now.Year;
            int start = year - 115;
            for (int i = start; i < year; i++) {
                _listOfYears.Add(new SelectListItem() {
                    Text = i.ToString(),
                    Value = i.ToString()
                });
            }

            //List Of Hights
            _listOfHeights.Add(new SelectListItem() {
                Text = "Select Height",
                Value = string.Empty
            });

            string text;
            for(int f = 3; f < 9; f++) {
                text = string.Format("{0} Feet", f);
                _listOfHeights.Add(new SelectListItem() {
                    Text = text,
                    Value = string.Format("{0}.0", f)
                });
                for (int i = 1; i < 12; i++) {
                    _listOfHeights.Add(new SelectListItem() {
                        Text = string.Format("{0} {1} Inches", text, i),
                        Value = string.Format("{0}.{1}", f, i)
                    });
                }               
            }
        }

        public InputModel() {

        }
        #endregion

        public RouteValueDictionary Attributes {
            get {
                v_QuesRulesAndFilters rule = this.Rules.FirstOrDefault(r => r.QualifyingRule.HasValue());
                string qualifingRule = rule != null ? rule.QualifyingRule : "";
                var attrs = new { questionId = this.QuestionId, @clientInputTypeRelId = this.ClientInputTypeRelId, @inputTypeId = this.InputTypeId, @parentId = this.ParentClientInputTypeRelId, @rule = qualifingRule };
				var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(attrs);
				attributes.Where(a => a.Value == null).ToList().ForEach(i => attributes.Remove(i.Key));
				
				string value;
                if (this.Required) {
                    attributes.Add("class", "form-control requiredQuestion");
                } else {
                    attributes.Add("class", "form-control");
                }
                switch (this.InputType) {
                    case InputTypes.yesno:
                        attributes.Add("optionvalue", this.OptionValue);
                        break;
                    case InputTypes.date:
                    case InputTypes.datetime:
                        attributes.Add("autocomplete", "off");
                        /*if(this.Answer.IsEmpty()) {
                            attributes.Add("disabled", "disabled");
                        }*/
                        break;
                    case InputTypes.typeahead:
                        attributes.Add("data-provide", "typeahead");
                        attributes.Add("autocomplete", "off");
                        attributes.Add("source", this.OptionValue);
                        break;
                }

                foreach (string key in base.InputAttributes.Keys) {
					value = Convert.ToString(base.InputAttributes[key]);
                    if (!attributes.ContainsKey(key)) {
                        if (key == "data-val-regex-pattern") {
                            attributes.Add("data-val-regex", string.Format("Question {0} is improperly formatted", this.AnswerIndex));
                        }
						if(value.HasValue()) { attributes.Add(key, value); }
                    } else if(key == "class") {
                        attributes[key] += string.Format(" {0}", value);
                    } else if(value.HasValue()) {
                        attributes[key] = value;
                    }
                }
                return attributes;
            }
        }

        public static void ResetStaticLists() {
            _listOfYears.ForEach(l => l.Selected = false);
            _listOfHeights.ForEach(l => l.Selected = false);
        }

        public List<SelectListItem> ListOfYears {
            get {
                SelectListItem selected = _listOfYears.Where(v => v.Value == this.Answer).SingleOrDefault();
                if (selected != null) {
                    selected.Selected = true;
                } else {
                    _listOfYears.First().Selected = true;
                }
                return _listOfYears; 
            }
        }

        public string ListOfYearsValue {
            get { return this.Answer; }
        }


        public List<SelectListItem> ListOfHeights {
            get {
                SelectListItem selected = _listOfHeights.Where(v => v.Value == this.Answer).SingleOrDefault();
                if (selected != null) {
                    selected.Selected = true;
                } else {
                    _listOfHeights.First().Selected = true;
                }
                return _listOfHeights; 
            }
        }

        public List<SelectListItem> InsuranceTypes {
            get { return Question.InsuranceList.ToDropdownList("Select Insurance Type", this.Answer); }
        }

        public string InsuranceTypesValue {
            get {
                try {
                    if (this.InsuranceId != null) {
                        return Question.InsuranceList[this.InsuranceId.Value];
                    }
                    return this.Answer;
                } catch {
                    return null;
                }
            }
        }

        public List<SelectListItem> DropDownList {
            get {
                try {
                    return this.GetType().GetProperty(this.OptionValue).GetValue(this) as List<SelectListItem>;
                } catch {
                    return null;
                }
            }
        }

        public string GetInputControl(string name) {
            StringBuilder html = new StringBuilder();
            string value = this.OptionValue.HasValue() ? this.GetType().GetProperty(string.Format("{0}Value", this.OptionValue)).GetValue(this) as string : this.Answer;
			var attrs = this.Attributes;
			attrs.Add("value", value);

            switch (this.InputType) {
                case InputTypes.typeahead:
                    html.AppendFormat("<input name='{0}' type='text' ", name);
					break;
				default:
					return null;
            }

			foreach (string key in attrs.Keys) {
				value = Convert.ToString(attrs[key]);
				if (value.HasValue()) {
					html.AppendFormat("{0}='{1}'", key, value);
				}
				if (key == "data-val-regex-pattern") {
					html.AppendFormat("data-val-regex='{0}'", string.Format("Question {0} is improperly formatted", this.AnswerIndex));
				}
			}
			return html.Append(" />").ToString();
        }
    }
}