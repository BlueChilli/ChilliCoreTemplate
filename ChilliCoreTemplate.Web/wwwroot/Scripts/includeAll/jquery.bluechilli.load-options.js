(function ($) {
    $.fn.loadOptions = function (options) {

        var settings = $.extend({}, $.fn.loadOptions.defaults, options);
        if (!settings.url) throw new Error("url is required");

        var deferred = $.Deferred();

        function doit() {
            $t = $(this);
            var items = [];

            const options = $t.find('option:eq(0)');
            if (options && options.length > 0) {
                var items = [options[0].outerHTML];
            }
            items.push();
            $t.find('option').remove();
            $t.append('<option>(loading)</option>');
            $.getJSON(settings.url, function (data) {
                $.each(data.data, function (i, option) {
                    items.push('<option value="' + option.value + '">' + option.text + '</option>');
                });
            }).done(function (result, status, xhr) {
                $t.find('option').remove();
                $t.append(items.join(''));
                if (items.length == 2) $t.find('option:eq(1)').attr('selected', 'selected'); deferred.resolve(result);
                deferred.resolve(result);
            }).fail(function (xhr, status) {
                deferred.reject.apply(deferred, arguments);
            });
        }

        this.each(function () {
            doit.call(this);
        });

        return deferred.promise();
    }

    $.fn.loadOptions.defaults = {
    };

})(jQuery);
