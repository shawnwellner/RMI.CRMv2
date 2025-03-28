using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace CRM.Domain {
    public class CallCenterUser : v_CallCenterUsers {
        private static object _syncObject = new object();

        private static List<Five9User> Five9Agents {
            get {
                const string CacheName = "Five9Agents";

                var cache = HttpContext.Current.Cache;
                List<Five9User> list = cache[CacheName] as List<Five9User>;
                if (list == null) {
                    lock (_syncObject) {
                        TimeSpan expire = new TimeSpan(0, 10, 0);
                        long[] hideUsers = new long[] { 2119898, 2132281, 2132486, 2245866, 2152082, 2325433, 2365828 };
                        list = Five9User.GetUsers().Where(u => u.Active && !u.Five9AgentId.In(hideUsers)).OrderBy(u => u.LastName).ToList();
                        if (list.Count > 0) {
                            cache.Add(CacheName, list, null, Cache.NoAbsoluteExpiration, expire, CacheItemPriority.Normal, null);
                        }

                        /*using (var client = new Five9.Five9Client("BasicHttpBinding_IFive9", "http://services.rmiatl.org/Five9/Five9.svc")) {
                            list = client.GetUsers().OrderBy(x => x.LastName).Where(x => !x.Id.In(hideUsers)).ToList();
                            cache.Add(CacheName, list, null, Cache.NoAbsoluteExpiration, expire, CacheItemPriority.Normal, null);
                        }*/
                    }

                }
                return list;
            }
        }

        /*public static dynamic Five9AgentItems {
            get {
                IDictionary<string, object> obj = new ExpandoObject();
                foreach (var user in Five9Agents) {
                    obj.Add(user.Id.ToString(), user);
                }
                return obj;
            }
        }*/

        public static CallCenterUser Update(CallCenterUser user, List<int> verticalIds) {
            using (CRMEntities context = new CRMEntities()) {
                string ids = verticalIds != null && verticalIds.Count > 0 ? string.Join(",", verticalIds.ToArray()) : null;

                v_CallCenterUsers agent = context.sp_UpdateCallCenterUser(user.UserId, user.UserTypeId, 
                                                       user.FirstName.ToTitleCase(), user.LastName.ToTitleCase(),
                                                       user.Phone.CleanPhoneNumber(), user.Email.ToLower(), user.UserName,
                                                       user.Password, user.Five9AgentId, ids, user.Enabled).SingleOrDefault();
                return agent.CloneAs<CallCenterUser>();
            }
        }

        public static CallCenterUser GetUserById(LoginUser user, int userId) {
            return Search(new SearchParams(user, userId)).SingleOrDefault();
        }

        public static List<CallCenterUser> Search(SearchParams search) {

            using (CRMEntities context = new CRMEntities()) {
                var list = context.sp_CallCenterSearch(search.UserId, search.LastName, search.Phone.CleanPhoneNumber(), search.Email,
                                                       search.PageNum, search.MaxRows, search.ShowAll).AsEnumerable().Select(c => c.CloneAs<CallCenterUser>()).ToList();
                return list.ToList();
            }
        }

        public static List<Five9User> GetFive9AgentList() {
            List<v_CallCenterUsers> ccUsers;
            using (CRMEntities context = new CRMEntities()) {
                ccUsers = context.v_CallCenterUsers.ToList();
            }

            var users = (from f in Five9Agents
                         join c in ccUsers on f.Five9AgentId equals c.Five9AgentId.ToInt() into fList
                         from l in fList.DefaultIfEmpty()
                         where f.Active == true
                         select new { fUser = f, isAssigned = (l != null) });

            foreach (var user in users) {
                user.fUser.IsAssigned = user.isAssigned;
            }
            return users.Select(u => u.fUser).ToList();
        }
    }
}
