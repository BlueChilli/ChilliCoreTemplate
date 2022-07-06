/*!
 * jquery.bluechilli.file-preview.js 1.0
*/
(function ($) {
    $.fn.filePreview = function (previewSelector, callback) {

        $(this).change(function () {
            var t = this;
            var $preview = $(previewSelector);

            if (t.files.length > 0) {
                var file = t.files[0];
                var extension = file.name.split('.').pop();

                $preview.html(getIconForExtension(extension) + '&nbsp;<small>' + file.name + '</small>');
            } else {
                $preview.html('');
            }

            if (callback != null) callback();
        });

        function getIconForExtension(extension) {
            return '<i class="fa ' + getClassNameForExtension(extension) + '"></i>';
        }

        function getClassNameForExtension(extension) {
            return extensions[extension.toLowerCase()] || icons.file
        }

        const icons = {
            image: 'fa-file-image-o',
            pdf: 'fa-file-pdf-o',
            word: 'fa-file-word-o',
            powerpoint: 'fa-file-powerpoint-o',
            excel: 'fa-file-excel-o',
            audio: 'fa-file-audio-o',
            video: 'fa-file-video-o',
            zip: 'fa-file-zip-o',
            code: 'fa-file-code-o',
            file: 'fa-file-o'
        }

        const extensions = {
            gif: icons.image,
            jpeg: icons.image,
            jpg: icons.image,
            png: icons.image,

            pdf: icons.pdf,

            doc: icons.word,
            docx: icons.word,

            ppt: icons.powerpoint,
            pptx: icons.powerpoint,

            xls: icons.excel,
            xlsx: icons.excel,

            aac: icons.audio,
            mp3: icons.audio,
            ogg: icons.audio,

            avi: icons.video,
            flv: icons.video,
            mkv: icons.video,
            mp4: icons.video,

            gz: icons.zip,
            zip: icons.zip,

            css: icons.code,
            html: icons.code,
            js: icons.code,

            file: icons.file
        }

    };
})(jQuery);