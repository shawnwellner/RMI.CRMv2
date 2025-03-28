using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CRM {
    public class JsonSerializer : ActionResult {
        public JsonSerializer(Object data) : this(data, false) {}

        public JsonSerializer(Object data, bool ignoreNulls) {
            this.Data = data;
            this.IgnoreNulls = ignoreNulls;
        }

        public Object Data { get; private set; }
        public bool IgnoreNulls { get; private set; }

        public override void ExecuteResult(ControllerContext context) {
            /*var serializer = new DataContractJsonSerializer(this.Data.GetType());
            String output = String.Empty;
            using (var ms = new MemoryStream()) {
                serializer.WriteObject(ms, this.Data);
                output = Encoding.Default.GetString(ms.ToArray());
            }*/
            string json = JsonConvert.SerializeObject(this.Data, new JsonSerializerSettings() {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                NullValueHandling = this.IgnoreNulls ? NullValueHandling.Ignore : NullValueHandling.Include
            });

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.Write(json);
        }
    }
}