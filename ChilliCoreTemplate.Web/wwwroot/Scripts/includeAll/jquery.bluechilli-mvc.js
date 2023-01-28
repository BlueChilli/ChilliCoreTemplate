/*!
 * jquery.bluechilli-mvc.js 1.0
*/
// bootstrapify the validation (client side)
$.validator.setDefaults({
    highlight: function (element) {
        $(element).closest(".form-group").addClass("error");
    },
    unhighlight: function (element) {
        $(element).closest(".form-group").removeClass("error");
    },
    ignore: ".ignore,:hidden:not(.validate)"
});

// bottom alignment plugin
(function ($) {
    $.fn.bottomAlign = function (minHeight) {
        return this.each(function (i) {
            $(this).css('margin-top', 0);
            $(this).find("img").each(function () {
                if ($(this).height() == 0)
                    $(this).height($(this).attr("height"));
            });
            var itemHeight = Math.max($(this).outerHeight(true), minHeight);
            var containerHeight = $(this).parent().outerHeight(true);
            var margin = (containerHeight - itemHeight);
            $(this).css('margin-top', margin);
        });
    };
})(jQuery);

// horizontal center
(function ($) {
    $.fn.horizontalCenter = function () {
        return this.each(function (i) {
            var div = $(this);
            var width = 0;
            div.children().each(function () {
                width += $(this).outerWidth();
            });
            div.css("margin-left", (($(window).width() - width) / 2) + $(window).scrollLeft() + "px");
        });
    };
})(jQuery);

// $.ajaxLoad (for partial view loading, with success callback, handles framework redirect also)
// obsolete
(function ($) {
    $.extend({
        ajaxLoad: function (containerId, url, data, success, type) {
            var options = {
                url: url,
                type: type || 'GET',
                dataType: 'html',
                data: data
            }
            if (window.FormData && data instanceof FormData) {
                options.processData = false;
                options.contentType = false;
            }
            $.ajax(options)
                .then(function (result, status, xhr) {
                    if (xhr.getResponseHeader('X-Ajax-Redirect') != null)
                        location.href = xhr.getResponseHeader('X-Ajax-Redirect');
                    else {
                        var render = $('#' + containerId).html(result);
                        $.validator.unobtrusive.parse(render);
                        if (typeof (_gaq) != "undefined") _gaq.push(['_trackPageview', url]);
                        if (typeof (ga) != "undefined") ga('send', 'pageview', url);
                        if (success != undefined) success(result, status, xhr);
                    }
                })
                .fail(function (xhr, status) {
                    $('#' + containerId).html(xhr.responseText);
                })
        }
    });
})(jQuery);

// $.doPost('/MyController/MyAction', { id: 10 });
// Used by ButtonPost
(function ($) {
    $.extend({
        doGet: function (url, params) {
            document.location = url + '?' + $.param(params);
        },
        doPost: function (url, params) {
            var $form = $("<form>")
                .attr("method", "post")
                .attr("action", url);
            if (params != null) {
                $.each(params, function (name, value) {
                    $("<input type='hidden'>")
                        .attr("name", name)
                        .attr("value", typeof value == "object" ? JSON.stringify(value) : value)
                        .appendTo($form);
                });
            }
            $form.appendTo("body");
            $form.submit();
        }
    });
})(jQuery);

//FileMaxSize attribute
(function ($) {
    $.validator.unobtrusive.adapters.add('filemaxsize', ['filemaxsize'], function (options) {
        options.rules['filemaxsize'] = options.params;
        if (options.message) {
            options.messages['filemaxsize'] = options.message;
        }
    });

    $.validator.addMethod('filemaxsize', function (value, element, params) {
        if (!element.files || !element.files[0]) return true;
        var filemaxsize = params.filemaxsize;
        return element.files[0].size < params.filemaxsize
    }, '');
})(jQuery);

//obsolete
function ajaxForm(indication, success, id) {
    // Handle form submit ...
    if (id === undefined) id = 'AjaxForm';
    var theForm = $('form#' + id);
    if (theForm.length == 0) theForm = $('form:first');
    theForm.off("submit").on("submit", function (event) {
        event.preventDefault();
        var form = $(this);
        if (form.valid()) {
            if (indication == 'modal')
                $("#ProgressDialog").modal({ keyboard: false });

            if (indication == 'button') {
                form.find('input[type="submit"],button[type="submit"]').prop('disabled', true).find('i').attr('class', 'icon-loading');
                $('img#ajax-loader').show();
            }
            //(containerId, url, data, success, type)
            $.ajaxLoad('AjaxFormContainer', form.attr('action'), form.serialize(),
                function () {
                    $.validator.unobtrusive.parse(form[0]);
                    bootstrapcssvalidation();
                    if (indication == 'modal') $("#ProgressDialog").modal("hide");
                    if (success != undefined) success();
                },
                'POST'
            );
        }
    });
}

//Misc ajax functions
(function ($) {
    $.extend({
        onNavTabStart: function (o) {
            var me = $(o);
            me.parents('ul').children().removeClass('active');
            me.parent().addClass('active');
        },
        onAjaxStart: function (containerId, text, t) {
            if (text == null) text = '&nbsp';
            var container = $('#' + containerId);
            container.html('<div class="loading" style="height:' + container.height() + 'px">' + text + '</div>');
            if (t != null) {
                $(t).prop('disabled', true).find('i').addClass('icon-loading');
            }
        },
        onAjaxEnd: function (type) {
            $(type + '[disabled]').prop('disabled', false).find('i').removeClass('icon-loading');
        }
    });
})(jQuery);

$().ready(function () {
    bootstrapcssvalidation();

    //prevent form double submit
    //set htmlAttributes: new { data_submitted = "false" } This is set by Menu.BeginForm
    $('form').submit(function () {
        var $t = $(this);
        if ($t.data('submitted') == true) {
            return false;
        }

        if ($t.valid()) {
            if ($t.data('submitted') == false) {
                $t.data('submitted', true);
            }
        }
    });
});

//So can be called DHTML
function bootstrapcssvalidation() {
    // add boostrap css to server validation
    $('form .input-validation-error').closest(".form-group").addClass("error");
    $('.validation-summary-errors:not(:has(button)),.validation-summary-valid:not(:has(button))').prepend('<button type="button" class="close">×</button>');
    $('.validation-summary-errors,.validation-summary-valid').addClass("alert alert-error");
    $('.validation-summary-errors ul').addClass("unstyled");

    $('.alert .close').unbind('click').bind('click', function () {
        $(this).parent().hide();
    });

    $('form').bind('invalid-form.validate', function () {
        if ($('.validation-summary-errors').length > 0) {
            $('.validation-summary-errors ul').addClass("unstyled");
            $('.validation-summary-errors').show();
            var errorDiv = $('.validation-summary-errors').first();
            var scrollPos = errorDiv.offset().top;
            $(window).scrollTop(scrollPos);
        }
    });
};

//GreaterThan attribute
(function ($) {
    $.validator.unobtrusive.adapters.addSingleVal('greaterthan', 'other');


    $.validator.addMethod("greaterthan", function (value, element, params) {

        var from = $('input[name="' + other + '"]').val();

        // one of them null then nothing to compare so return true
        if (!value || !from) return true;

        return Date.parse(value) > Date.parse(from);

    }, '');


})(jQuery);

var BlueChilli = BlueChilli || {};

BlueChilli.validateABN = function (value) {

    value = value.replace(/[ ]+/g, '');

    if (!value.match(/\d{11}/)) {
        return false;
    }

    var weighting = [10, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19];

    var tally = (parseInt(value.charAt(0)) - 1) * weighting[0];

    for (var i = 1; i < value.length; i++) {
        tally += (parseInt(value.charAt(i)) * weighting[i]);
    }

    return (tally % 89) == 0;
};

BlueChilli.validateACN = function (value) {
    value = value.replace(/[ ]+/g, '');

    if (!value.match(/\d{9}/)) {
        return false;
    }

    var weighting = [8, 7, 6, 5, 4, 3, 2, 1];
    var tally = 0;
    for (var i = 0; i < weighting.length; i++) {
        tally += (parseInt(value.charAt(i)) * weighting[i]);
    }

    var check = 10 - (tally % 10);
    check = check == 10 ? 0 : check;

    return check == parseInt(value.charAt(i));
};

BlueChilli.checkAnchor = function () {
    if (document.location.hash.length > 0) {
        $('.nav-tabs a[data-bs-target="' + document.location.hash + '"]').tab("show");
    }
}

BlueChilli.uploadSummernoteImage = function (id, url, file) {
    formData = new FormData();
    formData.append("ImageFile", file);
    $.ajax({
        type: 'POST',
        url: url,
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        success: function (fileUrl) {
            var imgNode = document.createElement('img');
            imgNode.src = fileUrl;
            $(id).summernote('insertNode', imgNode);
        },
        error: function (data) {
            alert(data.responseText);
        }
    });
};

(function ($) {
    $.validator.addMethod("bluechilli_checksum", function (value, element, checksumtype) {
        if (value == null || value.length == 0) {
            return true;
        }

        if (checksumtype === 'abn') {
            return BlueChilli.validateABN(value);
        } else if (checksumtype === 'acn') {
            return BlueChilli.validateACN(value);
        }
    });

    $.validator.addMethod("mustbetrue", function (value, element, params) {
        return value === true;
    });

    //To be used in conjunction wih HttpPostedFileBaseFileExtensionsAttribute
    $.validator.addMethod('accept', function (value, element, param) {
        param = $(element).attr('extensions');
        param = typeof param == "string" ? param.replace(/,/g, '|') : "png|jpe?g|gif";
        return this.optional(element) || value.match(new RegExp(".(" + param + ")$", "i"));
    });

    $.validator.unobtrusive.adapters.addSingleVal('checksum', 'checksumtype', 'bluechilli_checksum');
    $.validator.unobtrusive.adapters.add("mustbetrue", function (options) {
        if (options.element.tagName.toUpperCase() === "INPUT" && options.element.type.toUpperCase() === "CHECKBOX") {
            options.rules["required"] = true;
            if (options.message) {
                options.messages["required"] = options.message;
            }
        }
    });

    $.validator.unobtrusive.adapters.addBool('datecontrol');
    $.validator.addMethod("datecontrol", function (value, element, params) {
        var $t = $(element);
        var controls = $('input[name="' + $t.attr('name') + '"]');
        var values = [controls[0].value, controls[1].value, controls[2].value]
        if (values[2].length > 0) {
            var successElement = $(element).parents('.js-date-control').siblings('.js-date-control-success');
            if (values[0].length > 0 && values[1].length > 0 && values[2].length == 4) {
                var d = new Date(values[2], values[1] - 1, values[0]);
                if (d.getFullYear() == values[2] && (d.getMonth() + 1) == values[1] && d.getDate() == Number(values[0])) {
                    successElement.text(d.toDateString());
                    return true;
                }
            }
            successElement.text('');
            return false;
        }
        return true;
    });

    $.validator.methods.date = function (value, element) {
        var r = this.optional(element) || Date.parse(value);
        if (!r && moment) {
            var f = $(element).data('date-format');
            if (f != '') {
                return moment(value, f.toUpperCase()).isValid();
            }
        }
        return r;
    }

})(jQuery);

$(function () {
    //To normalise newlines
    $.valHooks.textarea = {
        get: function (elem) {
            return elem.value.replace(/\r?\n/g, "\r\n");
        }
    };
    $('.js-date-control input').on('keyup', function (e) {
        if (/\D/g.test(this.value)) {
            // Filter non-digits from input value.
            this.value = this.value.replace(/\D/g, '');
        }

        //tab if completed (unless shift or tab)
        if (e.which != 16 && e.which != 9 && this.value.length == $(this).attr('maxlength') && this.value.length == 2) {
            var controls = $('input:visible,select:visible,textarea:visible');
            var index = controls.index(this);
            controls.length > index ? controls[index + 1].focus() : controls[0].focus();
        }
    });
});

// new ajaxLoad => eg: $("#load").ajaxLoad({url: "xxx" , all the ajax options }).done(function() { }).fail(function() {});
// customAction: 'append' || customAction: 'none'
(function ($, window) {
    $.fn.ajaxLoad = function (options) {
        var settings = $.extend({
            url: null,
            customAction: 'html',
            type: 'GET',
            data: {}
        }, options, {
                datatype: 'html'
            });

        var $this = $(this);

        var deferred = $.Deferred();

        if (!settings.url) throw new Error("url is required");

        if (window.FormData && settings.data instanceof FormData) {
            settings.processData = false;
            settings.contentType = false;
        }

        $.ajax(settings)
            .done(function (result, status, xhr) {
                if (xhr.getResponseHeader('X-Ajax-Redirect') != null) {
                    window.history.pushState(null, null, xhr.getResponseHeader('X-Ajax-Redirect'));
                    window.location.reload();
                } else {
                    if (settings.customAction != 'none') {
                        var render = $this[settings.customAction](result);
                        $.validator.unobtrusive.parse(render);
                    }
                    if (typeof (_gaq) != "undefined") _gaq.push(['_trackPageview', settings.url]);
                    if (typeof (ga) != "undefined") ga('send', 'pageview', settings.url);
                    deferred.resolve(result);
                }
            })
            .fail(function (xhr, status) {
                var validationSummary = $('#validationSummary div.validation-summary-errors');
                if (validationSummary.length == 0) {
                    $('#validationSummary').append('<div class="validation-summary-errors"></div>');
                    validationSummary = $('#validationSummary div.validation-summary-errors');
                }
                validationSummary.append('<ul><li>There was an error while performing your request: ' + xhr.statusText + ' (' + xhr.status + ')</li></ul>');
                deferred.reject.apply(deferred, arguments);
            });
        return deferred.promise();
    };
})(jQuery, window);

//new ajaxForm - captures submit event on form, dsplays throbbers, detects error in partial view result
//$("#myForm").ajaxForm({}).done(function() { }).fail(function() { }).always(function() { });
//target for partial view defaults to #ajaxForm-container and can be a function whichs returns selector.
(function ($) {
    $.fn.ajaxForm = function (options) {

        var settings = $.extend({}, $.fn.ajaxForm.defaults, options);

        if (settings.pause != null) {
            $(this).data('ajaxForm-pause', settings.pause);
            return;
        }

        var deferred = $.Deferred();

        $(this).submit(function (e) {
            e.preventDefault();

            var form = $(this);
            if (!form.data('ajaxForm-pause') && form.valid()) {
                //(containerId, url, data, success, type)
                var target = typeof (settings.target) == 'function' ? settings.target() : settings.target;
                if (settings.beforeSubmit != null) settings.beforeSubmit();

                if (settings.indication == 'button') {
                    var button = form.find('input[type="submit"],button[type="submit"]');
                    if ($.ladda != null && button.hasClass('ladda-button')) {
                        var l = button.ladda();
                        l.ladda('start');
                    } else {
                        button.prop('disabled', true);
                    }
                }

                if (form.attr('enctype') == 'multipart/form-data') settings.useFormData = true;
                $(target).ajaxLoad({ url: form.attr('action'), data: settings.useFormData ? new FormData(form[0]) : form.serialize(), type: 'POST' })
                    .always(function (result) {
                        var targetLoaded = $(target);
                        if (targetLoaded.find('input[type="submit"],button[type="submit"]').prop('disabled', false).ladda)
                            targetLoaded.find('input[type="submit"],button[type="submit"]').prop('disabled', false).ladda().ladda('stop');
                        if (form.find('input[type="submit"],button[type="submit"]').prop('disabled', false).ladda)
                            form.find('input[type="submit"],button[type="submit"]').prop('disabled', false).ladda().ladda('stop');
                        targetLoaded.find('img#ajax-loader').hide();

                        if (targetLoaded.find('.validation-summary-errors').length == 0) {
                            deferred.resolve(result);
                        } else {
                            bootstrapcssvalidation();
                            deferred.reject(result);
                        }
                    });
            }
        });
        return deferred.promise();
    };

    $.fn.ajaxForm.defaults = {
        indication: "button",
        target: "#ajaxForm-container",
        useFormData: false,
        beforeSubmit: null,
        pause: null //If set to true trap form submit but do nothing, this option doesn't change the bound event.
    };
})(jQuery);

if ($.isFunction(String.prototype.format) === false) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/\{\{|\}\}|\{(\d+)\}/g, function (m, n) {
            if (m == "{{") { return "{"; }
            if (m == "}}") { return "}"; }
            return args[n];
        });
    };
}

//Disable number scrolling when trying to page scroll
(function ($) {
    $(':input[type=number]').on('mousewheel', function (e) { $(this).blur(); });
})(jQuery);

//print a section of a page $('#page-container').printElem(); Use class noprint for non-printable sections
jQuery.fn.extend({
    printElem: function () {
        var cloned = this.clone();
        var printSection = $('#printSection');
        if (printSection.length == 0) {
            printSection = $('<div id="printSection"></div>')
            $('body').append(printSection);
        }
        printSection.append(cloned);
        var toggleBody = $('body *:visible');
        toggleBody.hide();
        $('#printSection, #printSection *').not('.noprint').show();
        window.print();
        printSection.remove();
        toggleBody.show();
    }
});