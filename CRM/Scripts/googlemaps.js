(function ($) {
    var userMarker = null,
         locMarker = null,
            bounds = null,
          polyline = new google.maps.Polyline({
              strokeColor: "#0000ff",
              strokeOpacity: 1.0,
              strokeWeight: 2,
              clickable: false
          }),
          mapOptions = {
              zoom: 12,
              center: new window.google.maps.LatLng(0, 0),
              scrollwheel: false,
              navigationControl: false,
              mapTypeControl: false,
              //scaleControl: false,
              //draggable: false,
              maxZoom: 16,
              streetViewControl: false,
              mapTypeId: window.google.maps.MapTypeId.ROADMAP,
          };

    $.fn.setGoogleMapContainer = function () {
        if (this.exists()) {
            var gmap = this.get(0);
            window.googleMap = new window.google.maps.Map(gmap, mapOptions);
        }
        return this;
    };

    google.maps.Map.prototype.refresh = function () {
        window.google.maps.event.trigger(this, 'resize');
    };

    google.maps.Map.prototype.autozoom = function () {
        this.fitBounds(bounds);
        this.panToBounds(bounds);
        this.refresh();
    };

    window.updateMap = function (provider, fromDialog) {
        if (!window.googleMap) { $('.googleMap').setGoogleMapContainer(); }
        var user = fromDialog ? provider : $('form').toJSON();
        if (!user || !user.Latitude || !user.Longitude) { return; }
        var content = " \
            <div> \
                <strong>{0}</strong> \
                <p>{1}</p> \
                </br> \
                <a href='https://www.google.com/maps/place/{1}' target='__blank'>View on Google Maps</a> \
            </div>";

        var fullName = "{0} {1}".format(user.FirstName, user.LastName);
        var value = '{0},{1} {2}'.format(user.City, user.State, user.Zip);
        var body = content.format(fullName, value);

        bounds = new google.maps.LatLngBounds();
        var gPoint = new window.google.maps.LatLng(user.Latitude, user.Longitude);
        bounds.extend(gPoint);
        userMarker = updateMarker(userMarker, gPoint, body, "https://maps.google.com/mapfiles/ms/micons/blue.png");

        value = '{0}</br>{1},{2} {3}'.format(provider.OfficeAddress, provider.OfficeCity, provider.OfficeState, provider.OfficeZipCode);
        body = content.format(provider.OfficeName, value);

        gPoint = new window.google.maps.LatLng(provider.OfficeLatitude, provider.OfficeLongitude);
        bounds.extend(gPoint);
        locMarker = updateMarker(locMarker, gPoint, body);
        if (provider.Polyline) {
            var pathPoints = google.maps.geometry.encoding.decodePath(provider.Polyline);
            polyline.setPath(pathPoints);
            polyline.setMap(window.googleMap);
        } else {
            polyline.setMap(null);
        }
        window.googleMap.autozoom();
    };

    var updateMarker = function (orgMarker, gPoint, content, image) {
        if (orgMarker == null) {
            var clickHandler = function (e) {
                if (this instanceof google.maps.InfoWindow) {
                    this.opened = !this.opened;
                    if (!this.opened) { window.googleMap.autozoom(); }
                } else if (this instanceof google.maps.Map) {
                    if (userMarker.infoWindow.opened) {
                        userMarker.infoWindow.close();
                        window.googleMap.autozoom();
                    } else if (locMarker.infoWindow.opened) {
                        locMarker.infoWindow.close();
                        window.googleMap.autozoom();
                    }
                } else {
                    this.infoWindow.opened = !this.infoWindow.opened;
                    if (this.infoWindow.opened) {
                        this.infoWindow.open(window.googleMap, this);
                    } else {
                        this.infoWindow.close();
                        window.googleMap.autozoom();
                    }
                }
                window.googleMap.refresh();
            };

            orgMarker = new window.google.maps.Marker({
                map: window.googleMap,
                position: gPoint,
                icon: image
            });

            orgMarker.infoWindow = new window.google.maps.InfoWindow({
                content: content
            });

            window.google.maps.event.addDomListener(window.googleMap, 'click', clickHandler);
            window.google.maps.event.addListener(orgMarker, 'click', clickHandler);
            window.google.maps.event.addListener(orgMarker.infoWindow, 'closeclick', clickHandler);
        } else {
            orgMarker.setPosition(gPoint);
            orgMarker.infoWindow.setContent(content);
        }
        if (image) { window.googleMap.setCenter(gPoint); }
        window.googleMap.refresh();
        return orgMarker;
    };
})(jQuery);