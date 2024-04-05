(function ($) {
    $.fn.progressPoll = function (options) {

        var settings = $.extend({}, $.fn.progressPoll.defaults, options);
        if (!settings.url) throw new Error("url is required");

        function init() {
            $t = $(this);
            if (settings.message != null) {
                settings.message = '<p>' + settings.message + '</p>';
            }
            $t.append(settings.containerHtml.format(settings.message));
            poll();
            $('.progress-bar').width(settings.startPercentage);
        }

        function poll() {
            $.ajax({
                url: settings.url,
                type: "GET",
                success: function (data) {
                    if (data.errorUrl) {
                        location.href = data.errorUrl;
                    } else if (data.progress == 100) {
                        setTimeout(function () { location.href = data.url; }, 750);
                    }
                    $('.progress-bar').width(data.progress + '%');
                },
                dataType: "json",
                complete: setTimeout(function () { poll() }, 1000),
                timeout: 2000
            })
        }

        this.each(function () {
            init.call(this);
        });
    }

    $.fn.progressPoll.defaults = {
        startPercentage: '5%',
        containerHtml: '<div class="modal-body">{0}<div class="mt-4 progress h-5"><div class="progress-bar bg-success"></div></div></div>',
        message: ''
    };

})(jQuery);
