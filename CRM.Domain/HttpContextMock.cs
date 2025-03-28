using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace CRM.Domain {
    public static class HttpContextMock {
        private static HttpContext mContext;
        private static void CreateSession() {
            string lsId = Guid.NewGuid().ToString();
            var lcSItems = new SessionStateItemCollection();
            var lcSObjects = new HttpStaticObjectsCollection();

            var sessionContainer = new HttpSessionStateContainer(lsId, lcSItems, lcSObjects, 10, true,
                HttpCookieMode.AutoDetect, SessionStateMode.InProc, false);

            mContext.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
                                         BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Standard,
                                     new Type[] { typeof(HttpSessionStateContainer) },
                                     null).Invoke(new object[] { sessionContainer });

        }
 
        public static HttpContext Current {
            get {
                if (HttpContext.Current == null) {
                    var lcRequest = new HttpRequest("/", "http://http://localhost:61968/", string.Empty);
                    var lcResponse = new HttpResponse(new System.IO.StringWriter());

                    mContext = new HttpContext(lcRequest, lcResponse);
                    mContext.User = new GenericPrincipal(new GenericIdentity(@"rmiatl\shawnw"), new string[] { });
                    CreateSession();
                    return mContext;
                } else {
                    return HttpContext.Current;
                }
            } 
        }
    }
}
