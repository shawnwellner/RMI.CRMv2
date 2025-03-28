<% @Page Language="C#" %>
<% @Import namespace="System.IO" %>
<% @Import namespace="System.Net.Http" %>
<% @Import namespace="System.Text" %>
<% @Import namespace="System.Text.RegularExpressions" %>
<% @Import namespace="System.Threading.Tasks" %>
<% @Import namespace="System.Web" %>
<% @Import namespace="Newtonsoft.Json" %>
<% @Import namespace="Newtonsoft.Json.Linq" %>
<% @Import namespace="System.Dynamic" %>
<% @Import namespace="System.Collections.Generic" %>
<% @Import namespace="System.Globalization" %>

<script runat="server">
	/*
		Webhooks Setup: https://gist.github.com/tixastronauta/0b9c3b409a7ba96edffc
		Enable/Disable Page Webhooks: http://sandbox.rmiatl.org/facebook.html
		Create FB TestLeads: https://developers.facebook.com/tools/lead-ads-testing/
		FB Developer Dashboard: https://developers.facebook.com/apps/550754551759896/dashboard/
		FB Graph API Tester: https://developers.facebook.com/tools/explorer/145634995501895/
	*/
	
	private const bool DebugMode = false;
	private const string token = Settings.FacebookToken;
	private StringBuilder log = new StringBuilder();
	
	private class Authorization {
		public string AuthKey { get; private set; }
		public string Identifier { get; private set; }
		
		public Authorization(long pageId, List<object> additionalData) {
			switch(pageId) {
				case 525699790931888: //Trugreen
					this.AuthKey = Settings.TrugreenAuthKey;
					this.Identifier = "trugreenoffer.lawncare.net";
					break;
				case 1573022179624534: //BPC
					this.AuthKey = Settings.BPCAuthKey;
					this.Identifier = "backpaincenters.com";
					additionalData.Add(new { Name = "FiveNine", Value = (!DebugMode).ToString() });
					additionalData.Add(new { Name = "VerticalId", Value = "1" });
					if(DebugMode) {
						additionalData.Add(new { Name = "TestEmail", Value = "shawn.wellner@responsemine.com" });
					}
					break;
				default:
					throw new NotImplementedException();
			}
		}
	}
	
	private static string PostRequest(string url, string json) {
		using (HttpClient client = new HttpClient()) {
			Task<HttpResponseMessage> task = client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
			task.Wait();
			string resp = GetResponseString(task.Result);
			return resp;
		}
	}
	
	private static string GetRequest(string url) {
		using (HttpClient client = new HttpClient()) {
			Task<HttpResponseMessage> task = client.GetAsync(url);
			task.Wait();
			return GetResponseString(task.Result);
		}
	}
	
	private static string CleanZipcode(string value) {
		if (string.IsNullOrEmpty(value)) { return value; }
		value = Regex.Replace(value, "[^0-9]", ""); //Remove anything that's not a number
		return value.PadLeft(5, '0');
	}
	
	private static string GetResponseString(HttpResponseMessage resp) {
		Task<byte[]> task = resp.Content.ReadAsByteArrayAsync();
		task.Wait();
		return Encoding.UTF8.GetString(task.Result);
	}
			
	private static string CleanPhoneNumber(string value) {
		if (string.IsNullOrEmpty(value)) { return value; }
		value = Regex.Replace(value, "[^0-9]", ""); //Remove anything that's not a number
		return Regex.Replace(value, "^1", ""); //Remove prefixed 1's
	}
	
	private static string ToTitleCase(string value) {
		if (string.IsNullOrEmpty(value)) { return value; }
		string lowerCase = value.ToLower().Trim();
		return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lowerCase).Replace("_", "");
	}
	
	private static bool IsMatch(string value, string pattern) {
		if (string.IsNullOrEmpty(value)) { return false; }
		return Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase);
	}
	
	private static string TryToLower(string value) {
		if (string.IsNullOrEmpty(value)) { return value; }
		return value.Trim().ToLower();
	}
	
	private static string GetValue(string key, IDictionary<string, object> item, bool forceAdd) {
		if(item.ContainsKey(key)) {
			return item[key].ToString();
		}
		if(forceAdd) { item.Add(key, ""); }
		return null;
	}
	
	private void AppendToLog(string format, params string[] values) {
		if(log.Length > 0) { log.AppendLine(); }
		log.AppendFormat(format, values)
		   .AppendLine();
	}
	
	private void LookupGeo(IDictionary<string, object> lead) {
		try {
			string zip = GetValue("Zip", lead, true);
			string city = GetValue("City", lead, true);
			string state = GetValue("State", lead, true);
			string url = null;
			if(!string.IsNullOrEmpty(zip) && string.IsNullOrEmpty(city) && string.IsNullOrEmpty(state)) {
				if(zip != "00000") {
					string address = GetValue("Address", lead, false);
					url = string.Format("http://api.rmiatl.org/lead/geo/{0}/{1}", zip, address);
				}
			} else if(string.IsNullOrEmpty(zip) && !string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(state)) {
				url = string.Format("http://api.rmiatl.org/lead/geo/{0}/{1}", state, city);
			} else {
				return;
			}			
			string json = GetRequest(url);
			if(!string.IsNullOrEmpty(json)) {
				dynamic obj = JsonConvert.DeserializeObject(json);
				lead["City"] = obj.City;
				lead["State"] = obj.State;
				lead["Zip"] = obj.ZipCode;
			}
		} catch(Exception ex) {
			AppendToLog("<b>Error</b>: {0}", ex.Message);
		}
	}
	
	private static void SendEmail(string body) {
		var msg = new {
			To="shawn.wellner@responsemine.com", 
			From="shawn.wellner@responsemine", 
			Subject="Facebook LeadGen Page", 
			Body=body.Replace("\r\n", "</br>")
		};
		string json = JsonConvert.SerializeObject(msg);
		PostRequest("http://api.rmiatl.org/email", json);
	}
	
	private void SubmitLead(string json, Authorization auth, List<object> additionalData) {
		JObject resp = JObject.Parse(json);
		JToken error = resp["error"];
		if(error != null) {
			AppendToLog("<b>FB Error</b>: {0}", error.Value<string>("message"));
			AppendToLog("<b>FB ErrorCode</b>: {0}",	error.Value<string>("code"));
			return;
		}
		AppendToLog("<b>FBLead Response</b>:{0}", json);
		JToken fields = resp["field_data"];
		IDictionary<string, object> data = new ExpandoObject();
		string name;
		string value;
		
		foreach(var field in fields.Children()) {
			name = ToTitleCase(field.Value<string>("name"));
			value = field.Value<JArray>("values").Single().ToString();
			//value = Regex.Replace(value, @"\>$", "");
			
			if(IsMatch(name, "^(first|last|city)")) {
				value = ToTitleCase(value);
			} else if(IsMatch(name, "^zip")) {
				name = "Zip";
				value = CleanZipcode(value);
			} else if(IsMatch(name, "^phone")) {
				name = "Phone";
				value = CleanPhoneNumber(value);
			} else if(IsMatch(name, "^email")) {
				value = TryToLower(value);
			} else if(IsMatch(name, "^street")) {
				name = "Address";
			}
			data.Add(name, value);
		}
		LookupGeo(data);
		data.Add("AdditionalData", additionalData);
		var rmiLead = new {
			Authorization = auth,
			IsTest = DebugMode,
			LeadData = data
		};
		
		json = JsonConvert.SerializeObject(rmiLead);
		string url = DebugMode ? "http://10.1.0.50:61520/ReceiveLeadService.svc/PostLeadJson" : 
		                         "http://webservice.responsemine.com/LeadService/ReceiveLeadService.svc/PostLeadJson";
		AppendToLog("<b>RMILeadAPI Post</b>:{0}\r\n{1}", url, json);
		json = PostRequest(url, json);
		AppendToLog("<b>RMILeadAPI Response</b>:{0}", json);
	}
	
	private void GetFacebookLead(string leadGenId, Authorization auth, List<object> additionalData) {
		AppendToLog("<b>LeadGenId</b>:{0}", leadGenId);
		string url = string.Format("https://graph.facebook.com/v2.8/{0}/?access_token={1}", leadGenId, token);
		string json = GetRequest(url);
		SubmitLead(json, auth, additionalData);
	}
	
	private void ProcessFacebookLead(string json) {
		if(string.IsNullOrEmpty(json)) { return; }
		List<object> additionalData = new List<object>();
		dynamic obj = JsonConvert.DeserializeObject(json);
		if(obj.@object == "page" && obj.entry.Count > 0) {
			foreach(var item in obj.entry[0].changes) {
				if(item.field == "leadgen") {
					var info = item.value;
					additionalData.Add(new {Name = "FBPageId", Value = info.page_id.ToString() });
					additionalData.Add(new {Name = "FBFormId", Value = info.form_id.ToString() });
					additionalData.Add(new {Name = "FBAdId", Value = info.ad_id.ToString() });
					additionalData.Add(new {Name = "FBAdGroupId", Value = info.adgroup_id.ToString() });
					additionalData.Add(new {Name = "FBCreatedTime", Value = info.created_time.ToString() });
					additionalData.Add(new {Name = "SkipServiceObjects", Value = "true" });
					Authorization auth = new Authorization((long)info.page_id, additionalData);
					GetFacebookLead(info.leadgen_id.ToString(), auth, additionalData);
					return;
				}
			}
		}
		throw new Exception("Unexpected Data"); //Should never get to this line of code
	}
	
	protected void Page_Load(object sender, EventArgs e) {
		try {
			AppendToLog("<b>Recieved Page Request</b>: {0}", Request.Url.ToString());
			string token = Request.QueryString["hub.verify_token"];
			if(token == Settings.FacebookChallangeToken) {
				string challenge = Request.QueryString["hub.challenge"];
				Response.Write(challenge);
			} else {			
				string json = null;
				using(StreamReader rdr = new StreamReader(Request.InputStream)) {
					json = rdr.ReadToEnd();
				}
				if(!string.IsNullOrEmpty(json)) {
					AppendToLog("<b>Posted Data</b>: {0}", json);
					ProcessFacebookLead(json);
				}
			}
		} catch(Exception ex) {
			string msg = string.Format("<b>Exception</b>:{0}{2}\t{1}{2}", ex.Message, ex.StackTrace, "\r\n\r\n");
			log.Insert(0, msg);
		} finally {
			SendEmail(log.ToString());
		}
	}
</script>