using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Google {
    public class Waypoint {
        [JsonProperty("geocoder_status")]
        public string Status { get; set; }

        [JsonProperty("place_id")]
        public string PlaceId { get; set; }

        [JsonProperty("types")]
        public string[] Types { get; set; }
    }

    public class Point {
        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("lng")]
        public double Longitude { get; set; }
    }

    public class Bounds {
        [JsonProperty("northeast")]
        public Point NorthEast { get; set; }

        [JsonProperty("southwest")]
        public Point SouthWest { get; set; }
    }

    public class TextValue {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("value")]
        public decimal Value { get; set; }
    }

    public class Polyline {
        [JsonProperty("points")]
        public string Points { get; set; }
    }

    public class Step {
         [JsonProperty("distance")]
        public TextValue Distance { get; set; }

        [JsonProperty("duration")]
        public TextValue Duration { get; set; }

        [JsonProperty("end_location")]
        public Point EndLocation { get; set; }

        [JsonProperty("start_location")]
        public Point StartLocation { get; set; }
                     
        [JsonProperty("html_instructions")]
        public string Instructions { get; set; }

        [JsonProperty("travel_mode")]
        public string TravelMode { get; set; }

        [JsonProperty("maneuver")]
        public string Maneuver { get; set; }
    }

    public class Leg {
        [JsonProperty("distance")]
        public TextValue Distance { get; set; }

        [JsonProperty("duration")]
        public TextValue Duration { get; set; }
        
        [JsonProperty("end_address")]
        public string EndAddress { get; set; }

        [JsonProperty("end_location")]
        public Point EndLocation { get; set; }

        [JsonProperty("start_address")]
        public string StartAddress { get; set; }

        [JsonProperty("start_location")]
        public Point StartLocation { get; set; }

        [JsonProperty("steps")]
        public Step[] Steps { get; set; }
    }

    public class Route {
        [JsonProperty("bounds")]
        public Bounds Bounds { get; set; }

        [JsonProperty("copyrights")]
        public string CopyRights { get; set; }

        [JsonProperty("legs")]
        public Leg[] Legs { get; set; }

        [JsonProperty("overview_polyline")]
        public Polyline Polyline { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        /*[JsonProperty("warnings")]
        public string[] Warnings { get; set; }

        [JsonProperty("waypoint_order")]
        public Polyline Polyline { get; set; }*/
    }

    public class GoogleDirections {
        [JsonProperty("geocoded_waypoints")]
        public Waypoint[] WayPoints { get; set; }

        [JsonProperty("routes")]
        public Route[] Routes { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
