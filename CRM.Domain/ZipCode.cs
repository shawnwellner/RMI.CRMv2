using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Net;
using Newtonsoft.Json;
using CRM.Domain.Google;

namespace CRM.Domain {
    public class ZipCodeException : Exception {
        public enum ReasonTypes {
            NotFound = 0,
            StateNotMatched = 1
        }

        public ZipCodeException(ReasonTypes reason) 
            :this(reason, null, "Unknown") {
        }

        public ZipCodeException(ReasonTypes reason, Place results, string source) :
            base(string.Format(reason == ReasonTypes.NotFound ? "The zip code does not appear to be valid.  API Source={0}" : "The state of the zip code does not appear to match. API Source={0}", source)) {
            this.Reason = reason;
            this.Location = results;
        }

        public ReasonTypes Reason { private set; get; }
        public Place Location { private set; get; }
    }

    public class ZipCodeInfo {
        [JsonProperty("post code")]
        public string PostCode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("country abbreviation")]
        public string CountryAbbreviation { get; set; }

        [JsonProperty("Places")]
        public List<Place> Places { get; set; }

        public static Place GetZipCodeInfo(string zipcode) {
            return GetZipCodeInfo(zipcode, null, null);
        }

        public static Place GetZipCodeInfo(string zipcode, string city, string state) {
            Place place = null;
            string json, source;
            using (var client = new WebClient()) {
                try {
                    source = "maps.googleapis.com";
					string url = $"https://{source}/maps/api/geocode/json?address={zipcode}&sensor=false&key={Settings.GoogleApiKey}";
					json = client.DownloadString(url);
                    GoogleGeocode geo = JsonConvert.DeserializeObject<GoogleGeocode>(json);
                    if (!geo.Success) { throw new ZipCodeException(ZipCodeException.ReasonTypes.NotFound); }
                    place = geo.ToPlace();
                } catch {
                    source = "api.zippopotam.us";
                    try {
                        string country = zipcode.Matches(@"^[a-z]") ? "ca" : "us";
                        //-- if canda only use the first three characters for zipcode ------
                        zipcode = country == "ca" ? zipcode.Substring(0, 3) : zipcode;
                        json = client.DownloadString(String.Format("http://api.zippopotam.us/{0}/{1}", country, zipcode));
                        //var serializer = new JavaScriptSerializer();
                        ZipCodeInfo model = JsonConvert.DeserializeObject<ZipCodeInfo>(json); // serializer.Deserialize<ZipCodeObject>(json);
                        model.Places[0].ZipCode = zipcode;
                        place = model.Places[0];
                        /*json = client.DownloadString(String.Format("http://api.zippopotam.us/{0}/{1}/{2}", country, state, city));
                        //var serializer = new JavaScriptSerializer();
                        ZipCodeInfo model = JsonConvert.DeserializeObject<ZipCodeInfo>(json); // serializer.Deserialize<ZipCodeObject>(json);
                        model.Places[0].City = city;
                        model.Places[0].State = state;
                        return model.Places[0];*/
                    } catch {
                        throw new ZipCodeException(ZipCodeException.ReasonTypes.StateNotMatched, place, source);
                    }
                }
                if(place != null && (state.IsEmpty() || state.Equals(place.State, StringComparison.OrdinalIgnoreCase))) {
                    return place;
                }
                throw new ZipCodeException(ZipCodeException.ReasonTypes.StateNotMatched, place, source);
            }
        }

        private static DateTime lastCallTime = DateTime.MinValue;
        public static Double GetDistance(Double sLatitude, Double sLongitude, Double eLatitude, Double eLongitude, out string polyline) {
            var sCoord = new GeoCoordinate(sLatitude, sLongitude);
            var eCoord = new GeoCoordinate(eLatitude, eLongitude);
            polyline = null;
            string url = $"https://maps.googleapis.com/maps/api/directions/json?origin={sCoord.Latitude},{sCoord.Longitude}&destination={eCoord.Latitude},{eCoord.Longitude}&sensor=false&key={Settings.GoogleApiKey}";
            DateTime now = DateTime.Now;
            if (now.Subtract(lastCallTime).TotalMilliseconds <= 500) {
                System.Threading.Thread.Sleep(500);
            }
            lastCallTime = now;
            using (WebClient client = new WebClient()) {
                for (int i = 0; i < 2; i++) {
                    try {
                        string json = client.DownloadString(url);
                        GoogleDirections model = JsonConvert.DeserializeObject<GoogleDirections>(json);
                        if (model == null || model.Status == "ZERO_RESULTS") { break; }
                        if (model.Status == "OK") {
                            Route route = model.Routes[0];
                            polyline = route.Polyline.Points;
                            return route.Legs[0].Distance.Value.MetersToMiles();
                        } else if (model.Status == "OVER_QUERY_LIMIT") {
                            System.Threading.Thread.Sleep(1000);
                        } else {
                            throw new Exception("Error occured getting directions");
                        }
                    } catch {
                        System.Threading.Thread.Sleep(2000);
                    }
                }
            }
            
            //-- return distance in meters. So devide by 1609.344 then round up --
            return sCoord.GetDistanceTo(eCoord).MetersToMiles();
        } 

    }

    public class Place {
        #region Constructors
        public Place() { }
        public Place(GeoResult result) {
            string[] values = result.FormattedAddress.Split(',');
            string[] info = values[1].Trim().Split(' ');

            this.City = values[0].Trim();
            this.State = info[0];
            this.ZipCode = info[1];
            this.Latitude = result.Geometry.Location.Latitude.ToString();
            this.Longitude = result.Geometry.Location.Longitude.ToString();
        }
        #endregion

        [JsonProperty("place name")]
        public string City { get; set; }
        [JsonProperty("longitude")]
        public string Longitude { get; set; }
        [JsonProperty("state")]
        public string StateName { get; set; }
        [JsonProperty("state abbreviation")]
        [CloneAlias("State")]
        public string State { get; set; }
        [JsonProperty("latitude")]
        public string Latitude { get; set; }
        [JsonProperty("post code")]
        [CloneAlias("Zip")]
        public string ZipCode { get; set; }
    }
}
