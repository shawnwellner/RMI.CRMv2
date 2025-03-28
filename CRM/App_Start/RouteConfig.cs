using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CRM
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Logoff", // Route name
				"logoff", // URL with parameters 
				new { controller = "User", action = "Logoff" }
			);

			routes.MapRoute(
				"Logout", // Route name
				"logout", // URL with parameters 
				new { controller = "User", action = "Logoff" }
			);

			routes.MapRoute(
               "Reporting",
               "Reporting/{*report}",
               new { controller = "Reporting", action = "Index", report = UrlParameter.Optional }
            );
			
            routes.MapRoute(
               "Error",
               "Error",
               new { controller = "Error", action = "Index", id = UrlParameter.Optional, type = UrlParameter.Optional }
           );

            routes.MapRoute(
                "GetView", // Route name
                "GetView/{viewName}/{param}", // URL with parameters 
                new { controller = "Ajax", action = "GetView", viewName = UrlParameter.Optional, param = UrlParameter.Optional } // Parameter defaults
             );

			routes.MapRoute(
                "Ajax", // Route name
                "ajax/{action}", // URL with parameters 
                new { controller = "Ajax", action = UrlParameter.Optional} // Parameter defaults
             );

            /*routes.MapRoute(
                "StrollHealth", // Route name
                "api/563e14ad-5d99-4cf6-af6d-7691981e420d/update", // URL with parameters 
                new { controller = "Ajax", action = "UpdateFromStrollHealth" } // Parameter defaults
             );*/

            routes.MapRoute(
               "User",
               "{controller}/{action}/{type}",
               new { controller = "User", action = "Path", type = UrlParameter.Optional}
			);

			routes.MapRoute(
			   "ThankYou",
			   "{controller}/{action}/{type}/{id}",
			   new { type = UrlParameter.Optional, id = UrlParameter.Optional }
		   );

			routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { id = UrlParameter.Optional }
            );
        }
    }
}