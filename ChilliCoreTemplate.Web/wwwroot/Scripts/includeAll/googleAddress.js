var googleAddress = (function ($) {
    var googleAddressDefaults = function () {
        return {
            isSelectedId: '',
            componentsId: '',
            countryToRestrictSearch: '',
            restrictSearchToNearMyLocation: false,
            restrictBoundsToLatLng: null,
            restrictBoundsTo: null
        };
    };

    return {
        defaults: googleAddressDefaults
    };

})(jQuery);


(function ($) {

    function createAddress(result) {
        if (result.geometry == null) result.geometry = { location: { lat: 0, lng: 0 } };
        var isNumber = result.geometry == null ? false : typeof result.geometry.location.lat == "number";
        return {
            place_id: result.place_id,
            address: result.formatted_address, addressParts: result.address_components,
            geometry: { latitude: isNumber ? result.geometry.location.lat : result.geometry.location.lat(), longitude: isNumber ? result.geometry.location.lng : result.geometry.location.lng() }
        };
    }

    function GoogleAddress(element, options) {
        this.$element = $(element);
        this.element = element;
        this.settings = $.extend({}, googleAddress.defaults(), options);
        this.$textbox = this.$element.is(":input") ? this.$element : this.$element.find("input[type='text']");
        this.init();
    }

    GoogleAddress.prototype = {
        init: function () {
            this.initGoogleAddressAutoComplete();
        },
        initGoogleAddressAutoComplete: function () {
            var $this = this.$textbox;
            var settings = this.settings;

            var options = {
                types: ['geocode']
            };

            if (settings.countryToRestrictSearch != '') {
                options = $.extend({ componentRestrictions: { country: settings.countryToRestrictSearch } }, options);
            }

            this.autocomplete = new google.maps.places.Autocomplete($this.get(0), options);

            google.maps.event.addListener(this.autocomplete, 'place_changed', $.proxy(function () {

                this.fillInAddress();
            }, this));

            if (settings.restrictBoundsTo != null) {
                this.restrictToCustomBound(settings.restrictBoundsTo);

            } else if (settings.restrictBoundsToLatLng != null) {
                var geolocation = new google.maps.LatLng(
                    restrictBoundsToLatLng.latitude, restrictBoundsToLatLng.longitude);
                this.restrictBoundsTolatlng(geolocation);
            } else if (settings.restrictSearchToNearMyLocation) {
                $this.on("focus", $.proxy(this.restrictGeoCode, this));
            }

        },
        fillInAddress: function () {
            // Get the place details from the autocomplete object.
            var $textbox = this.$textbox;
            var place = this.autocomplete.getPlace();
            var settings = this.settings;

            if (settings.isSelectedId) {
                var $isSelected = $('#' + settings.isSelectedId);
                $isSelected.val(true);
            }

            var result = createAddress(place);

            if (settings.componentsId) {
                var $addressComponent = $('#' + settings.componentsId);

                $addressComponent.val(JSON.stringify(result));
            }

            $textbox.val(place.formatted_address);

            if (typeof settings.onselect === 'function') settings.onselect(result);
        },
        restrictGeoCode: function () {
            var that = this;
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(function (position) {
                    var geolocation = new google.maps.LatLng(
                        position.coords.latitude, position.coords.longitude);
                    that.autocomplete.setBounds(new google.maps.LatLngBounds(geolocation,
                        geolocation));
                });
            }

        },
        restrictToCustomBound: function (bound) {
            this.autocomplete.setBounds(bound);
        },
        restrictBoundsTolatlng: function (geolocation) {
            this.autocomplete.setBounds(new google.maps.LatLngBounds(geolocation,
                geolocation));
        },
        currentLocation: function () {
            var deferred = $.Deferred();
            var address = this.$element;
            var componentsId = '#' + this.settings.componentsId;

            address.data('placeholder', this.$element.attr('placeholder'));
            address.attr('disabled', 'disabled').attr('placeholder', 'loading ...').val('');
            if (navigator && navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(function (position) {
                    var latlng = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);
                    var geocoder = new google.maps.Geocoder();
                    geocoder.geocode({ 'latLng': latlng }, function (results, status) {
                        if (status == google.maps.GeocoderStatus.OK && results[0]) {
                            address.val(results[0].formatted_address);
                            results[0].geometry.location.lat = results[0].geometry.location.lat()
                            results[0].geometry.location.lng = results[0].geometry.location.lng()
                            $(componentsId).val(JSON.stringify(results[0]));
                            deferred.resolve();
                        } else {
                            deferred.reject(status);
                        }
                        ;
                    });
                },
                    function errorCallback(error) {
                        deferred.reject(error.message);
                    }, { maximumAge: 60000, timeout: 5000, enableHighAccuracy: true });
            } else {
                deferred.reject('Geolocation is not supported by this browser.');
            }

            return deferred.promise();
        },
        reset: function () {
            this.$element.val('').removeAttr('disabled').attr('placeholder', this.$element.data('placeholder')).focus();
        }

    };

    $.fn.googleAddress = function (option) {
        var arg = Array.prototype.slice.call(arguments, 0);
        var $this = $(this), data = $this.data('googleaddress');
        if (!data) $this.data('googleaddress', (data = new GoogleAddress(this, option)));
        if (typeof option == 'string') return data[option].call(data, arg[1]);

    };

    $.fn.googleAddress_extract = function (components, type, returnLongForm) {
        if (components == null) return '';
        var result = '';
        if (typeof (returnLongForm) == 'undefined') returnLongForm = false;
        for (var i = 0; i < components.length; i++)
            for (var j = 0; j < components[i].types.length; j++)
                if (components[i].types[j] == type) result = returnLongForm ? components[i].long_name : components[i].short_name;
        return this.val(result);
    };

    $.fn.googleAddress_join = function (components, types) {
        if (components == null) return '';
        var result = '';
        for (var i = 0; i < types.length; i++) {
            var part = this.googleAddress_extract(components, types[i]).val();
            if (part.length > 0 && result.length > 0) result += (types[i] == 'street_number') ? '/' : ' ';
            result += part;
        }
        return this.val(result);
    };

})(jQuery);
