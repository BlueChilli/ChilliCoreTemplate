var googleMap = (function ($) {
    var googleAddressMapData = function googleAddressMapData(address) {
        return $.extend({}, { center: { latitude: address.geometry.latitude, longitude: address.geometry.longitude }, markers: [{ title: address.address, latitude: address.geometry.latitude, longitude: address.geometry.longitude }] });
    };

    var googleMapDefaults = function () {
        return {
            mapOptions: {
                zoom: 15,
                mapTypeId: google.maps.MapTypeId.ROADMAP
            },
            markerOptions: {
                visible: true
            }
        };
    };

    return {
        getFromAddress: googleAddressMapData,
        defaults: googleMapDefaults
    };
})(jQuery);

(function ($) {
    function GoogleMap(element, options) {
        this.$element = $(element);
        this.element = element;
        this.map = null;
        this.mapCircle = null;
        this.settings = $.extend({}, googleMap.defaults(), options);
        this.markers = [];
        this.init();
    }

    GoogleMap.prototype = {
        init: function () {
            this.settings.mapOptions.center = new google.maps.LatLng(this.settings.center.latitude, this.settings.center.longitude);
            this.map = new google.maps.Map(this.element, this.settings.mapOptions);
            this.settings.markerOptions.map = this.map;

            for (var i = 0; i < this.settings.markers.length; i++) {
                this.settings.markers[i].position = new google.maps.LatLng(this.settings.markers[i].latitude, this.settings.markers[i].longitude);;
                var markerOptions = $.extend({}, this.settings.markers[i], this.settings.markerOptions);
                if (markerOptions.icon != null && typeof (markerOptions.icon) == 'object') {
                    markerOptions.icon.origin = new google.maps.Point(markerOptions.icon.origin.x, markerOptions.icon.origin.y);
                    markerOptions.icon.size = new google.maps.Size(markerOptions.icon.size.width, markerOptions.icon.size.height);
                }
                var marker = new google.maps.Marker(markerOptions);
                this.markers.push(marker);
            }
        },
        redraw: function (options) {
            this.settings = $.extend({}, this.settings, options);
            this.init();
        },
        resize: function () {
            var currCenter = this.map.getCenter();
            google.maps.event.trigger(this.map, 'resize');
            this.map.setCenter(currCenter);
        },
        getMap: function () {
            return this.map;
        },
        drawCircle: function (options) {
            if (this.mapCircle != null) {
                this.mapCircle.setMap(null);
            }
            var circleOptions = $.extend({}, {
                strokeColor: '#61a0bf',
                strokeOpacity: 0.4,
                strokeWeight: 1,
                fillColor: '#61a0bf',
                fillOpacity: 0.4,
                map: this.map,
                center: this.settings.mapOptions.center,
                radius: 1000
            }, options);
            this.mapCircle = new google.maps.Circle(circleOptions);
            this.map.fitBounds(this.mapCircle.getBounds());
        }
    }

    $.fn.googleMap = function (option) {
        var arg = Array.prototype.slice.call(arguments, 0);

        return this.each(function () {
            var $this = $(this)
              , data = $this.data('googlemap');
            if (!data) $this.data('googlemap', (data = new GoogleMap(this, option)))
            if (typeof option == 'string') data[option].call(data, arg[1])
        })

    };

})(jQuery);

