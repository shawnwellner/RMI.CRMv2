using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using CRM;
using CRM.Domain.CustomExceptions;

namespace CRM.Domain
{
    public class Utility
    {
        /*public static IEnumerable<KeyValuePair<string, T>> PropertiesOfType<T>(object obj)
        {
            return from p in obj.GetType().GetProperties()
                   where p.PropertyType == typeof(T)
                   select new KeyValuePair<string, T>(p.Name, (T)p.GetValue(obj));
        }

        public static IEnumerable<KeyValuePair<string, string>> Properties(object obj)
        {
            return from p in obj.GetType().GetProperties()
                   select new KeyValuePair<string, string>(p.Name, Convert.ToString(p.GetValue(obj))); //(string)p.GetValue(obj)
        }*/

        public static string[] StateList {
            get
            {
                return new string[] {
        "AK", "AL", "AR", "AZ", "CA", "CO", "CT", "DC", "DE", "FL",
        "GA", "HI", "IA", "ID", "IL", "IN", "KS", "KY", "LA", "MA", "MD", "ME", "MI", "MN", "MO", "MS", "MT",
        "NC", "ND", "NE", "NH", "NJ", "NM", "NV", "NY", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX",
        "UT", "VA", "VT", "WA", "WI", "WV", "WY",
        "--- Canadian Provinces ---","AB","BC","MB","NB","NL","NS","NT","NU","ON","PE","QC","SK","YT" };
            }
        }

        #region FormatException
        private static string FormatException(Exception ex, bool isInnerEx)
        {
            if (ex == null) { return ""; }
            string innerEx = "";
			if (ex.InnerException != null) {
                innerEx = string.Format("<b>InnerException:</b> <pre>{0}</pre></br>", ex.InnerException.Message);
            }

			string stackTrace = ex.StackTrace;
			if (ex is JavaScriptException) {
				stackTrace = ((JavaScriptException)ex).StackTrace;
			}
			
			return string.Format(@"
<div>
	<b>{0}:</b> <pre>{1}</pre></br>
    {2}
    <b>StackTrace:</b> <pre>{3}</pre>
</div>
</br>
", ex.GetType().FullName, ex.Message, innerEx, stackTrace);
        }
        #endregion

        #region FormatHttpRequest
        private static string FormatHttpRequest(HttpRequestBase req, bool includePostedData)
        {
            LoginUser user = LoginUser.CurrentUser;
            string remoteAddess = req.IsLocal ? "localhost" : req.UserHostAddress;
            string remoteHost = req.IsLocal ? "localhost" : req.UserHostName;
            string postedData = "";
            if (includePostedData) {
                postedData = string.Format(@"
<div>
    <b>Posted Data:</b></br>
    <pre>{0}</pre>
</div>
", req.PostedData());
            }         

            return string.Format(@"
<div>
    <b>UserId:</b> {0}</br>
    <b>UserName:</b> {1}</br>
    <b>Company:</b> {2}</br>
    <b>UserTitle:</b> {3}</br>
	<b>User Agent:</b> {4}</br>
	<b>Remote Address:</b> {5}</br>
    <b>Remote Host:</b> {6}</br>
    <b>Request Method:</b> {7}</br>
    <b>URL:</b> <a href='{8}'>{8}</a></br>
</div>
</br>
{9}
</br>
", user.UserId, user.FullName, user.CompanyName, user.Title, req.UserAgent, remoteAddess, remoteHost, req.HttpMethod, req.Url, postedData);
        }
        #endregion

        public static void SendEmail(string subject, string body) {
            SendEmail(null, subject, body);
        }

        public static void SendEmail(string from, string subject, string body, params string[] recipients) {
            try {
                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(from ?? "dev@responsemine.com");
                if (recipients.Length > 0) {
                    foreach (string addr in recipients) {
                        if (addr.HasValue()) { msg.To.Add(addr); }
                    }
                } else {
                    msg.To.Add("dev@responsemine.com");
                }

                msg.IsBodyHtml = true;
                msg.Subject = subject;
                msg.Body = body;

                SmtpClient smtp = new SmtpClient("mail.rmiatl.com");
                smtp.Credentials = new NetworkCredential("svc_utilities", "5C3HZfXqVy", "rmiatl.com");
                smtp.Send(msg);
            }
            catch {
                
            }
        }

        public static void LogError(Exception ex) {
            LogError(new HttpRequestWrapper(HttpContextMock.Current.Request), ex);
        }

        public static void LogError(HttpRequestBase request, Exception ex) {
            LogError(request, ex, null);
        }

        public static void LogError(HttpRequestBase request, Exception ex, string additionInfo) {
            try {
                if (ex is ControlledException) {
                    ControlledException cEx = (ControlledException)ex;
                    if (!cEx.SendEmail) { return; }
                    if (cEx.AppendMessage.HasValue()) {
                        ex = new Exception(string.Format("{0}\r\t{1}", ex.Message, cEx.AppendMessage), cEx.OriginalException);
                    } else if (cEx.OriginalException != null) {
                        ex = cEx.OriginalException;
                    }
                }

                string body = string.Format(@"
<html>
    <head>
        <style>
            pre {{ margin-left:20px; }}
        </style>
    </head>
    <body>
        {0}
        {1}
        {2}
    </body>
</html>", FormatHttpRequest(request, additionInfo.IsEmpty()), additionInfo, FormatException(ex, false));

                SendEmail("An Exception Occured in DisplayNetwork", body);
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
