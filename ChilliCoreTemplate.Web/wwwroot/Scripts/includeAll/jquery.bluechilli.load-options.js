(function ($) {
    $.fn.loadOptions = function (options) {

        var settings = $.extend({}, $.fn.loadOptions.defaults, options);
        if (!settings.url) throw new Error("url is required");

        var deferred = $.Deferred();

        function doit() {
            var $t = $(this);
            var items = [];
            var addedCount = 0;
            var $firstOption = $t.find('option:eq(0)');

            var hasPlaceholder = $firstOption.length > 0 && ($firstOption.val() === "" || $firstOption.data('placeholder') === true);
            if (hasPlaceholder) {
                items = [$firstOption[0].outerHTML];
            }

            var selectedValues = $t.val();
            if (selectedValues != null && !$.isArray(selectedValues)) {
                selectedValues = [selectedValues];
            }

            $t.find('option').remove();

            $t.append('<option>(loading)</option>');

            $.getJSON(settings.url, function (data) {
                $.each(data.data, function (i, option) {
                    items.push('<option value="' + option.value + '">' + option.text + '</option>');
                    addedCount++;
                });
            }).done(function (result, status, xhr) {
                $t.find('option').remove();

                $t.append(items.join(''));

                if (selectedValues != null && selectedValues.length > 0) {
                    $t.val(selectedValues);
                }

                if (settings.autoSelectSingleOption && $t.find('option:selected').length === 0 && addedCount === 1) {
                    var $allOptions = $t.find('option');
                    var selectIndex = hasPlaceholder ? 1 : $allOptions.length - 1;
                    $allOptions.eq(selectIndex).prop('selected', true);
                }

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
        autoSelectSingleOption: true
    };

})(jQuery);
