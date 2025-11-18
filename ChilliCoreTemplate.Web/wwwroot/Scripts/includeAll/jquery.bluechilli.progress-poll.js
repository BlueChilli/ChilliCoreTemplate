(function ($) {
    $.fn.progressPoll = function (options) {

        var settings = $.extend({}, $.fn.progressPoll.defaults, options);
        if (!settings.url) throw new Error("url is required");
        if (!settings.id) settings.id = $.fn.progressPoll.defaults.id;

        function init() {
            var $container = $(this);
            var messageHtml = settings.message != null ? ('<p>' + settings.message + '</p>') : '';
            $container.append(settings.containerHtml.format(messageHtml, settings.id));
            var selector = '.progress-bar#' + settings.id;
            $container.find(selector).width(settings.startPercentage);
            poll($container, selector);
        }

        function poll($container, selector) {
            $.ajax({
                url: settings.url,
                type: "GET",
                success: function (data) {
                    if (data.errorUrl) {
                        location.href = data.errorUrl;
                    } else if (data.progress == 100) {
                        setTimeout(function () { location.href = data.url; }, 750);
                    }
                    $container.find(selector).width(data.progress + '%');
                },
                dataType: "json",
                complete: function (data) {
                    if (data.responseJSON == null || data.responseJSON.progress < 100)
                        setTimeout(function () { poll($container, selector); }, 1000)
                },
                timeout: 2000
            })
        }

        return this.each(function () {
            init.call(this);
        });
    }

    $.fn.progressPoll.defaults = {
        startPercentage: '5%',
        containerHtml: '<div class="modal-body">{0}<div class="mt-4 progress h-5"><div class="progress-bar bg-success" id="{1}"></div></div></div>',
        message: '',
        id: 'default-progress-bar-id'
    };

})(jQuery);
