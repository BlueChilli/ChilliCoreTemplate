(function ($) {
    $.fn.imageCropit = function (form, id, jsId, image, alternativeImage) {

        var exportCropitImage = false;
        var cropitInit = true;
        var containerId = '#js-image-cropper-' + jsId;  //TODO use this
        var base64 = id + 'Base64';
        var remove = id + 'Remove';
        var selectButton = '.js-image-select-' + jsId;
        var removeButton = '.js-image-remove-' + jsId;

        $(form).submit(onSubmitCropit);
        $(containerId).cropit({
            exportZoom: 1,
            imageState: { src: image || alternativeImage },
            onImageError: onErrorCropit,
            onOffsetChange: function () { if (!cropitInit) exportCropitImage = true; },
            onImageLoaded: function () { if (!cropitInit) { exportCropitImage = true; $(remove).val('False'); } }
        });
        $(selectButton).click(function () { $(id).click(); });
        $(removeButton).click(function () { $(containerId).cropit('imageSrc', alternativeImage); removeImage(); });
        setTimeout(function () { cropitInit = false }, 100);

        function removeImage() {
            cropitInit = true;
            $(remove).val('True');
            exportImageService = false;
            setTimeout(function () { cropitInit = false }, 100);
        }

        function onSubmitCropit() {
            if (exportCropitImage) {
                var imageData = $(containerId).cropit('export', {
                    type: 'image/jpeg',
                    quality: .9,
                    originalSize: true
                });
                $(base64).val(imageData);
            }
        }

        function onErrorCropit(error) {
            $(form).validate().showErrors({ [jsId]: error.message });
        }

        return onSubmitCropit;
    };
})(jQuery);