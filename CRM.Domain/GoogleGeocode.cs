using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Google {
    public class GoogleGeocode {
        public bool Success {
            get { return this.Status == "OK"; }
        }
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("results")]
        public GeoResult[] Results { get; set; }

        public Place ToPlace() {
            if(this.Success) {
                GeoResult result = this.Results.FirstOrDefault();
                if (result != null) {
                    string[] groups;
                    if (result.FormattedAddress.Matches(@"([^,]+),\s.*(\w{2})\s(\d+)", out groups)) {
                        return new Place(result);
                    }
                }
            }
            return null;
        }
    }

    public class GeoResult {
        [JsonProperty("type")]
        public string[] Types { get; set; }

        [JsonProperty("formatted_address")]
        public string FormattedAddress { get; set; }

        [JsonProperty("address_components")]
        public AddressComponent[] AddressComponents { get; set; }

        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; }

        [JsonProperty("place_id")]
        public string PlaceId { get; set; }
    }

    public class AddressComponent {
        [JsonProperty("types")]
        public string[] ComponentTypes { get; set; }

        [JsonProperty("long_name")]
        public string LongName { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }
    }

    public class Geometry {
        [JsonProperty("location")]
        public Point Location { get; set; }

        [JsonProperty("location_type")]
        public string LocationType { get; set; }

        [JsonProperty("viewport")]
        public Bounds ViewPort { get; set; }

        [JsonProperty("bounds")]
        public Bounds Bounds { get; set; }
    }
}
