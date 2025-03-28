using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain {
    public class Five9User {
        [JsonProperty("Id")]
        public long Five9AgentId { get; private set; }
        [JsonProperty("Active")]
        public bool Active { get; private set; }
        [JsonProperty("FirstName")]
        public string FirstName { get; private set; }
        [JsonProperty("LastName")]
        public string LastName { get; private set; }
        [JsonProperty("Email")]
        public string EMail { get; private set; }
        [JsonProperty("UserName")]
        public string UserName { get; private set; }
        [JsonProperty("Extension")]
        public int Extension { get; private set; }
        [JsonProperty("StartDate")]
        public DateTime StartDate { get; private set; }

        public bool IsAssigned { get; set; }

        public static List<Five9User> GetUsers() {
            const string format = "dd/MM/yyyy"; // your datetime format
            IsoDateTimeConverter dateTimeConverter = new IsoDateTimeConverter() { 
                DateTimeFormat = format 
            };
            using (WebClient client = new WebClient()) {
                client.Headers.Add("content-type", "application/json");
                string json = client.DownloadString("http://api.rmiatl.org/five9/json/getusers/");
                return JsonConvert.DeserializeObject<List<Five9User>>(json, dateTimeConverter);
            }
        }

        /*public static int? GetFive9PostQueueId(int? Five9AgentId) {
            if (!Five9AgentId.HasValue) { return null; }
            object result;
            using (CRMEntities context = new CRMEntities()) {
                using (var conn = new SqlConnection(context.Database.Connection.ConnectionString)) {
                    using (var cmd = conn.CreateCommand()) {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "Five9.dbo.sp_GetPostQueueId";
                        if (Five9AgentId.HasValue) {
                            cmd.Parameters.Add("@AgentId", System.Data.SqlDbType.Int).Value = Five9AgentId;
                        } else {
                            cmd.Parameters.Add("@AgentId", System.Data.SqlDbType.Int).Value = DBNull.Value;
                        }
                        conn.Open();
                        if ((result = cmd.ExecuteScalar()) != DBNull.Value) {
                            return Convert.ToInt32(result);
                        }
                        return null;
                    }
                }
            }
        }*/
    }
}
