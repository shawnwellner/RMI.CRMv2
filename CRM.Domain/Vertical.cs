using System.Collections.Generic;
using System.Linq;

namespace CRM.Domain {
    public delegate void VerticalQuestionHandler(Vertical vertical, int? customerId, List<Question<QuestionInput>> questions);
	
    public class Vertical : v_Verticals {
        public static List<Vertical> GetVerticalsByUserId(int userId) {
            using (CRMEntities context = new CRMEntities()) {
                return (from u in context.v_VerticalUsers
                        where u.UserId == userId && u.UserVerticalEnabled
                        select u).AsEnumerable().Select(v => v.CloneAs<Vertical>()).ToList();
            }
        }

        public static List<Vertical> GetMyVerticals(LoginUser user) {
            user = user ?? LoginUser.CurrentUser;
            if (user.IsAdminOrManager || user.UserType == UserTypes.StandardUser) {
                return GetVerticalList();
            } else {
                int userId = user.ClientParentId ?? user.UserId.ToInt();
                return Vertical.GetVerticalsByUserId(userId);
            }
        }

        public static List<Vertical> GetVerticalList() {
            using (CRMEntities context = new CRMEntities()) {
                return (from v in context.v_Verticals orderby v.SortOrder select v).AsEnumerable().Select(v => v.CloneAs<Vertical>()).ToList();
            }
        }

        protected static List<T> GetVerticalQuestions<T>(int? customerId, out List<Question<QuestionInput>> questions) where T : Vertical {
            List<T> list = new List<T>();
			questions = null;
			using (CRMEntities context = new CRMEntities()) {

                var verticals = (from v in context.v_Verticals orderby v.SortOrder select v);
                T vertical;
                foreach (v_Verticals v in verticals) {
                    vertical = v.CloneAs<T>();
					//if (callback != null) {
					questions = Question<QuestionInput>.GetListOfQuestions(context, v.VerticalId, null, null, customerId, false);
						//callback(vertical, customerId, questions);
                    //}
                    list.Add(vertical);
                }
            }
            return list;
        }

        /*
        public static Vertical GetVerticalById(int verticalId) {
            using (CRMEntities context = new CRMEntities()) {
                return (from v in context.v_Verticals where v.VerticalId == verticalId select v).SingleOrDefault().CloneAs<Vertical>();
            }
        }

        public static Vertical GetVerticalByUrl(Uri uri) {
            string url = uri.DnsSafeHost.ToLower();
            using (CRMEntities context = new CRMEntities()) {
                return (from v in context.v_Verticals where v.Url.ToLower() == url select v).SingleOrDefault().CloneAs<Vertical>();
            }
        }
        */
    }
}
