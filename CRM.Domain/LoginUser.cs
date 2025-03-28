using CRM.Domain.CustomExceptions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace CRM.Domain {
    public class LoginUser {
        private const string CookieName = "UserValidation";

        public int? UserId { get; set; }
        //public int VerticalId { get; set; }
        public int UserVerticalRelId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        public string VerticalName { get; private set; }
        public string VerticalTheme { get; private set; }

        public string CompanyName { get; set; }
        public string FullName { get; private set; }
        [CloneAlias("UserTypeId")]
        public UserTypes UserType { get; private set; }

        [CloneAlias("UserRoleId")]
        public UserRoles UserRole { get; private set; }
        public string Title { get; private set; }
        [CloneAlias("ParentId")]
        public int? ClientParentId { get; private set; }
        public bool IsAdmin { get; private set; }
        public bool IsAdminOrManager { get; private set; }
        public bool IsRmiUser { get; private set; }
        public bool IsStandardUser { get; private set; }
        public bool IsClient { get; private set; }
        public bool LoggedIn { get; private set; }
        public bool VerticalSelected { get; private set; }
        public int? Five9AgentId { get; private set; }
        public bool DevAdmin { get; private set; }

        private int? _vertCount;
        public int VerticalCount {
            get {
                if(this._vertCount == null) {
                    this._vertCount = this.Verticals.Count();
                }
                return this._vertCount.Value;
            }
        }

        private LoginUser() {
            HttpRequest request = HttpContextMock.Current.Request;
            HttpCookie cookie = request.Cookies[CookieName];
            Initialize(cookie);
            this.DevAdmin = this.IsAdmin && (request.IsLocal || request.ServerVariables["REMOTE_ADDR"].Matches(@"^10\.3\.4\.\d+$"));
        }

        public static LoginUser CurrentUser {
            get { return new LoginUser(); }
        }

		private List<Vertical> _verticals = null;
        public List<Vertical> Verticals {
            get {
                if (this.LoggedIn && this._verticals == null) {
                    this._verticals = Vertical.GetMyVerticals(this);
                    /*List<Vertical> verticals = Vertical.GetMyVerticals(this);
                    HttpRequest req = HttpContextMock.Current.Request;
                    Uri url = req.Url;
                    foreach (Vertical vertical in verticals) {
                        if (url.IsLocal()) {
                            vertical.Url = string.Format("{0}://{1}/?vert={2}", url.Scheme, url.Authority, vertical.VerticalId);
                        } else {
                            vertical.Url = string.Format("{0}://{1}", url.Scheme, vertical.Url);
                        }
                    }
                    return verticals;*/
                }
                return this._verticals;
            }
        }

        /*private static Vertical GetVerticalInfo(LoginUser user, HttpCookie cookie) {
            HttpRequest request = HttpContextMock.Current.Request;
            Vertical vertical = null;
            Uri uri = request.Url;
            if (uri.IsLocal()) {
                uri = new Uri("http://medicalnetworkportal.staging.rmiatl.org");

                int? vertId = request.QueryString["vert"].ToNullableInt();
                if (vertId.HasValue) {
                    vertical = Vertical.GetVerticalById(vertId.Value);
                } else if (cookie != null) {
                    int? verticalId = cookie.GetValue("VerticalId", null).ToNullableInt();
                    if (verticalId.HasValue) {
                        vertical = Vertical.GetVerticalById(verticalId.Value);
                    }
                }
            } else {
                user = user ?? LoginUser.CurrentUser;
                if (user.LoggedIn) {
                    vertical = Vertical.GetVerticalById(user.VerticalId);
                }
            }

            return vertical ?? Vertical.GetVerticalByUrl(uri);
        }

        private void UpdateCookie(HttpCookie cookie, Vertical vertical) {
            if (vertical != null) {
                this.VerticalId = vertical.VerticalId;
                this.VerticalName = vertical.VerticalName;
                this.VerticalTheme = vertical.Css; //string.Format("{0}.css", vertical.Url).ToLower();
                RefreshCookie(cookie, vertical, this);
                this.VerticalSelected = true;
            }
        }*/

        private static void RefreshCookie(HttpCookie cookie, LoginUser user) {
            /* Requried to reset the Doman and Expires properties when updating the cookie */
            HttpContext context = HttpContext.Current;
            cookie.Domain = context.Request.Url.GetRootDomain();
            if (user.IsRmiUser) {
                cookie.Expires = DateTime.Now.AddDays(365);
            } else {
                cookie.Expires = DateTime.Now.AddDays(7);
            }
            context.Response.Cookies.Add(cookie);
            /* End Required */
        }

        private static LoginUser CreateCookie(LoginUser validatedUser) {
            try {
                LoginUser.Dispose();
                HttpRequest request = HttpContextMock.Current.Request;
                HttpCookie cookie = new HttpCookie(CookieName);
                cookie.Values["UserId"] = validatedUser.UserId.ToString();
                cookie.Values["UserVerticalRelId"] = validatedUser.UserVerticalRelId.ToString();
                cookie.Values["UserType"] = Convert.ToString((int)validatedUser.UserType);
                cookie.Values["UserRole"] = Convert.ToString((int)validatedUser.UserRole);
                cookie.Values["ResetCookie"] = Convert.ToString(true);
                if (validatedUser.ClientParentId.HasValue && validatedUser.ClientParentId.Value > 0) {
                    cookie.Values["ClientParentId"] = validatedUser.ClientParentId.Value.ToString();
                }
                cookie.Values["Company"] = validatedUser.CompanyName ?? String.Empty;
                cookie.Values["FirstName"] = validatedUser.FirstName;
                cookie.Values["LastName"] = validatedUser.LastName;
                cookie.Values["FullName"] = validatedUser.FullName;
                if (validatedUser.Five9AgentId.HasValue) {
                    cookie.Values["AgentId"] = validatedUser.Five9AgentId.ToString();
                }
                RefreshCookie(cookie, validatedUser);
                return validatedUser;
            } catch {
                validatedUser.LoggedIn = false;
                throw;
            }
        }

        public static bool Validate(string userName, string password, out LoginUser currentUser) {
            LoginUser validateUser = LoginUser.ValidateCredentials(userName, password);
            currentUser = CreateCookie(validateUser);
            return validateUser.LoggedIn && currentUser != null;
        }

        public static void Dispose() {
            try {
                HttpContext context = HttpContextMock.Current;
                HttpCookie cookie = context.Response.Cookies[CookieName];
                if (cookie != null) {    
                    cookie.Domain = context.Request.Url.GetRootDomain();
                    cookie.Expires = DateTime.Now.AddDays(-1d);
                }
            } catch {
            }
        }

        public static bool CheckCredentialsExist(string userName, string password) {
            using (CRMEntities context = new CRMEntities()) {
                return context.sp_CredentialsExist(userName, password).First().ToBool();
            }
        }

        private static LoginUser Initialize(UserLogins loginUser) {
            LoginUser user = loginUser.CloneAs<LoginUser>();
            user.LoggedIn = true;
            user.FullName = String.Format("{0} {1}", user.FirstName, user.LastName);
            user.IsAdmin = false;
            user.IsAdminOrManager = false;
            user.IsRmiUser = false;
            user.IsStandardUser = false;
            user.IsClient = false;
            switch (user.UserType) {
                case UserTypes.Administrator: // administrator
                    user.IsAdmin = true;
                    user.IsRmiUser = true;
                    user.IsAdminOrManager = true;
                    user.Title = "Administrator";
                    break;
                case UserTypes.CallCenterManager: // manager
                    user.IsRmiUser = true;
                    user.IsAdminOrManager = true;
                    user.Title = "Call Center Manager";
                    break;
                case UserTypes.CallCenterAgent: // associate
                    user.IsRmiUser = true;
                    user.Title = "Call Center Associate";
                    break;
                case UserTypes.StandardUser:
                    user.IsRmiUser = true;
                    user.IsStandardUser = true;
                    user.Title = "Standard User";
                    break;
                case UserTypes.Client:
                case UserTypes.Child:
                    user.IsClient = true;
                    user.Title = "Medical Network Portal Member";
                    break;
                default: // Anyone Else
                    throw new Exception("Unexpected UserType");
            }
            return user;
        }

        private void Initialize(HttpCookie cookie) {
            if (this.ValidateUserCookie(cookie)) {
                this.UserVerticalRelId = cookie.GetValue("UserVerticalRelId", null).ToInt();
                this.UserRole = (UserRoles)cookie.GetValue("UserRole", null).ToInt();
                this.UserType = (UserTypes)cookie.GetValue("UserType", null).ToInt();
                this.FirstName = cookie.GetValue("FirstName", "User");
                this.LastName = cookie.GetValue("LastName", null);
                this.FullName = cookie.GetValue("FullName", null);
                this.CompanyName = cookie.GetValue("Company", null);
                this.ClientParentId = cookie.GetValue("ClientParentId", null).ToNullableInt();
                this.Five9AgentId = cookie.GetValue("AgentId", null).ToNullableInt();
                if (!this.Five9AgentId.HasValue) {
                    this.Five9AgentId = UserBase.GetFive9AgentId(this.UserId.ToInt());
                }

                this.IsAdmin = false;
                this.IsAdminOrManager = false;
                this.IsRmiUser = false;
                this.IsClient = false;
                switch (this.UserType) {
                    case UserTypes.Administrator: // administrator
                        this.IsAdmin = true;
                        this.IsRmiUser = true;
                        this.IsAdminOrManager = true;
                        this.Title = "Administrator";
                        break;
                    case UserTypes.CallCenterManager: // manager
                        this.IsRmiUser = true;
                        this.IsAdminOrManager = true;
                        this.Title = "Call Center Manager";
                        break;
                    case UserTypes.CallCenterAgent: // associate
                        this.IsRmiUser = true;
                        this.Title = "Call Center Associate";
                        break;
                    case UserTypes.StandardUser:
                        this.IsRmiUser = true;
                        this.Title = "Standeard User";
                        break;
                    case UserTypes.Client:
                    case UserTypes.Child:
                        this.Title = "Medical Network Portal Member";
                        this.IsClient = true;
                        break;
                    default: // Anyone Else
                        throw new Exception("Unexpected UserType tried logging into the CRM");
                }
            } else {
                this.LoggedIn = false;
                LoginUser.Dispose();
            }
        }

        public static LoginUser ValidateCredentials(string userName, string password) {
            try {
                using (CRMEntities context = new CRMEntities()) {
					UserLogins user = (from u in context.v_UserLogins
										 where u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && u.Password == password
										 && u.Enabled == true
										 select u).SingleOrDefault();
					if (user == null) { throw new UnauthorizedAccessException(); }
                    if (user.ForceLogin) {
                        user.ForceLogin = context.sp_UpdateFoceLogin(user.UserId) == 0; 
                    }
                    return Initialize(user);
                }
            } catch(UnauthorizedAccessException) {
                throw new ControlledException(ContentCopy.InvalidCredentialsCopy);
            }
        }


        private bool ValidateUserCookie(HttpCookie cookie) {
            try {
                if (cookie == null || cookie.GetValue("ResetCookie", null).ToNullableBool() != true) { return false; }
                HttpSessionState session = HttpContextMock.Current.Session;
                DateTime? lastChecked = session["LastChecked"] as DateTime?;
                DateTime now = DateTime.Now;
                string sUserId = cookie.GetValue("UserId", null);
                if (sUserId.HasValue()) {
                    this.LoggedIn = lastChecked != null && now.Subtract(lastChecked.Value).TotalMinutes < 1;
                }
                if (this.LoggedIn) {
                    this.UserId = sUserId.ToInt();
                    return true;
                } else {
                    int userId = sUserId.ToInt();
                    if (userId < 0) { return false; }
                    using (CRMEntities context = new CRMEntities()) {
						UserLogins user = (from u in context.v_UserLogins
										   where u.UserId == userId && u.Enabled == true
										   select u).Single();
                        //if (user.ForceLogin) { context.sp_UpdateFoceLogin(user.UserId); }
                        this.LoggedIn = user.Enabled && !user.ForceLogin;
                        if (this.LoggedIn) {
                            this.UserId = user.UserId;
                            session["LastChecked"] = now;
                            return true;
                        }
                    }
                }
            } catch {
                //Ignore Error
            }
            return false;
        }
    }
}
