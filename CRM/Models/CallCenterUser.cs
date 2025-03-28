using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Newtonsoft.Json;
using CRM.Domain;
using System.Text;

namespace CRM.Models {
    [Serializable]
    public class CallCenterFormModel : LoginUserModel {
        public List<int> VerticalIds { get; set; }

        [Required()]
        [DataType(DataType.Text)]
        [UIHint("Textbox")]
        public long Five9AgentId { get; set; }

        //return JsonConvert.SerializeObject(obj);
        public static MvcHtmlString Five9AgentDropDown(long agentId) {
            //@*Html.DropDownListFor(model => model.Five9AgentId, CRM.Models.CallCenterUser.Five9Agents,  new { @class = "form-control", @five9Items = @CRM.Models.CallCenterUser.Five9AgentItems })*@
            StringBuilder options = new StringBuilder();
            string attrs;
            bool selected = false;
            //List<CallCenterUserType> callCenterUsers = Search.GetCallCenterData(new UserBase(), LoggedInUser.CurrentUser.VerticalId);

            options.AppendLine("<option value=''>Select Five9 Agent</option>");
            if (agentId == -1) {
                selected = true;
                options.AppendLine("<option value='-1' selected>Not a Five9 Agent</option>");
            } else {
                options.AppendLine("<option value='-1'>Not a Five9 Agent</option>");
            }

            foreach (Five9User user in CallCenterUser.GetFive9AgentList()) {
                attrs = string.Format("{0}five9Item='{1}'", (selected == false && user.Five9AgentId == agentId ? "selected " : ""), JsonConvert.SerializeObject(user));
                options.AppendLine(string.Format("<option value='{0}' class='{4}'{5}>{1}, {2} - {3}{6} </option>", user.Five9AgentId, user.LastName, user.FirstName, user.EMail, user.IsAssigned ? "taken" : "", attrs, user.IsAssigned ? "**" : ""));
            }

            const string requiredAttrs = "data-val='true' data-val-required='Five9 Agent is a Required Field.'";

            string html = string.Format("<select id='{0}' name='{0}' class='form-control' {2}>{1}</select>", "Five9AgentId", options.ToString(), requiredAttrs);
            return new MvcHtmlString(html);
        }
    }
}