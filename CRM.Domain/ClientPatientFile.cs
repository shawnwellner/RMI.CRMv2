using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Data.Linq.Mapping;

namespace CRM.Domain {
    public class ClientPatientFile {
        [Table(Name = "ClientPatientInfo")]
        private class ClientPatientInfo {
            [Column(IsPrimaryKey = true, IsDbGenerated = true)]
            private int Id { get; set; }
            [Column]
            public static int? ClientFileDataId { get; set; }
            public static string JsonData { get; set; }
            [Column]
            public int? UserId { get; set; }

            [JsonProperty("Status")]
            [Column]
            public string Status { get; set; }

            [JsonProperty("LeadStatus")]
            private string LeadStatus {
                get { return this.Status; }
                set { this.Status = value; }
            }

            [JsonProperty("OpenStatus")]
            [Column]
            public string OpenStatus { get; set; }

            [JsonProperty("ClosedStatus")]
            [Column]
            public string ClosedStatus { get; set; }

            [JsonProperty("PatientName")]
            private string PatientName {
                get { return string.Format("{0} {1}", this.FirstName, this.LastName); }
                set {
                    int space = value.IndexOf(' ');
                    this.FirstName = value.Substring(0, space);
                    this.LastName = value.Substring(space);
                }
            }

            [JsonProperty("FirstName")]
            [Cleanup(CleanupType = Cleanup.CleanupTypes.TitleCase)]
            [Column]
            public string FirstName { get; set; }

            [JsonProperty("LastName")]
            [Cleanup(CleanupType = Cleanup.CleanupTypes.TitleCase)]
            [Column]
            public string LastName { get; set; }

            [JsonProperty("Phone")]
            [Cleanup(CleanupType = Cleanup.CleanupTypes.PhoneNumber)]
            [Column]
            public string Phone { get; set; }

            [JsonProperty("DOB")]
            [Column]
            public DateTime? DOB { get; set; }

            [JsonProperty("PrimaryInsurance")]
            [Column]
            public string InsuranceType { get; set; }

            [JsonProperty("Insurance")]
            private string Insurance {
                get { return this.InsuranceType; }
                set { this.InsuranceType = value; }
            }

            [JsonProperty("ProcedureType")]
            [Column]
            public string ProcedureType { get; set; }

            [JsonProperty("Appts")]
            private string Appts {
                set {
                    string[] groups;
                    if (value.Matches(@"([^\s]+) \- (.*)", out groups)) {
                        DateTime dt;
                        if (DateTime.TryParse(groups[1], out dt)) {
                            this.ApptDate = dt;
                        }
                        this.ApptLocation = groups[2];
                    };
                }
            }

            [Column]
            public DateTime? ApptDate { get; set; }
            [Column]
            public string ApptLocation { get; set; }

            [JsonProperty("MRIReceived")]
            [Column]
            public bool? MRIReceived { get; set; }

            [JsonProperty("MRIReviewed")]
            [Column]
            public bool? MRIReviewed { get; set; }

            [JsonProperty("StateProvince")]
            [Column]
            public string State { get; set; }

            [JsonProperty("CreateDate")]
            [Column]
            public DateTime? CreateDate { get; set; }

            [JsonProperty("Notes")]
            [Column]
            public string Notes { get; set; }

            public static IEnumerable<ClientPatientInfo> SaveToDatabase(ClientPatientInfo[] rows) {
                /*foreach (ClientPatientInfo row in rows) {
                    ClientPatientInfo.ClientFileDataId = DBHelpers.ExecuteStoredProcedure<ClientPatientInfo>("CRMv2.dbo.sp_UpdateClientInfo", row);
                }*/
                DBHelpers.ExecuteStoredProcedure("CRMv2.dbo.sp_UpdateClientInfo", rows);

                return DBHelpers.Where<ClientPatientInfo>(c => c.UserId == null);
            }
        }

        private static ClientPatientInfo[] Deserialize(byte[] data) {
            string json = Encoding.Default.GetString(data);
            //ClientPatientInfo.ClientFileDataId = null;
            //ClientPatientFile.ClientPatientInfo.JsonData = json;
            return JsonConvert.DeserializeObject<ClientPatientInfo[]>(json);
        }

        public static bool ProcessFile(HttpPostedFileBase file) {
            if (file.ContentLength > 0) {
                using (file.InputStream) {
                    byte[] data = new byte[file.ContentLength];
                    Match match = file.FileName.Match(@"\.(\w{3,4})$");
                    if (match.Success) {
                        ClientPatientInfo[] rows;
                        //string url = string.Format("http://services.rmiatl.org/excel/{0}", match.Groups[1].Value);
                        string url = string.Format("http://localhost:61160/excel/{0}", match.Groups[1].Value);
                        file.InputStream.Read(data, 0, file.ContentLength);
                        using (WebClient wc = new WebClient()) {
                            wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                            data = wc.UploadData(url, "POST", data);
                            rows = ClientPatientFile.Deserialize(data);
                        }
                        return ClientPatientInfo.SaveToDatabase(rows).Count() > 0;
                    }
                }
            }
            return false;
        }
    }
}