(function ($) {
    $.fn.imagePreview = function (previewSelector, callback) {
        $(this).change(function () {
            var t = this;
            for (var i = 0; i < t.files.length; i++) {
                var file = t.files[i],
                    imageType = /image.*/,
                    l = null;

                if (!file.type.match(imageType)) {
                    continue;
                }

                if ($.ladda != null) {
                    l = $(this).parent('label.ladda-button').ladda();
                    l.ladda('start');
                }

                $('#' + t.id + 'Remove').val('False');

                var $img = $(previewSelector),
                    img = $img[0],
                    reader = new FileReader();

                reader.onload = (function (aImg, aCallback) {
                    return function (e) {
                        if (aImg.nodeName == 'IMG') {
                            aImg.src = e.target.result;
                        } else {
                            aImg.style.backgroundImage = 'url(' + e.target.result + ')';
                        }
                        if (l != null) {
                            l.ladda('remove');
                        }
                        if (callback != null) callback();
                    };
                })(img);
                reader.readAsDataURL(file);
            }
        });
    };
})(jQuery);