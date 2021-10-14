(function ($) {
    //google analytics form field complettion tracking
    $.fn.formAnalytics = function (options) {

        var timingFields = [];
        var settings = $.extend({}, $.fn.formAnalytics.defaults, options);

        function bindEvents() {
            var form = $(this);

            form.on('blur', ':input', function () {
                //Record event if field completed or skipped
                var me = $(this), isCompleted = me.val().length > 0;

                if (!me.hasClass('ga-completed')) {
                    ga('send', {
                        hitType: 'event',
                        eventCategory: 'Form tracking - ' + form.attr('action'),
                        eventAction: isCompleted ? 'completed' : 'skipped',
                        eventLabel: me.attr('name')
                    });

                    if (isCompleted) {
                        me.addClass('ga-completed');
                    };
                }

            })
            .on('focus', ':input', function () {
                //Form field completion timing - start
                timingFields[$(this).attr('name')] = new Date().getTime();
            })
            .on('change', ':input', function () {
                //Form field completion timing - end
                if (!$(this).hasClass('ga-completed')) {
                    var name = $(this).attr('name');
                    if (timingFields[name]) {
                        var timeSpent = new Date().getTime() - timingFields[name];
                        ga('send', {
                            hitType: 'timing',
                            timingCategory: 'Field Completion - ' + form.attr('action'),
                            timingVar: name,
                            timingValue: timeSpent
                        });
                    }
                }
            })
            .find(':input').each(function () {
                //Mark fields with a value as completed on load
                if ($(this).val().length > 0) {
                    $(this).addClass('ga-completed');
                }
            });

            //Form Completion timing
            var formStart = new Date();
            form.submit(function () {
                ga('send', {
                    hitType: 'timing',
                    timingCategory: 'Form Completion',
                    timingVar: form.attr('action'),
                    timingValue: new Date().getTime() - formStart.getTime()
                });
            });
        }


        return this.each(function () {
            bindEvents.call(this);
        });

    }

    $.fn.formAnalytics.defaults = {
    };
})(jQuery);


