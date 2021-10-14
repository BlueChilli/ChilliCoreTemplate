var confirmationModule = (function () {
    var confirmationTemplate = function () {
        return "confirmationTemplate not initialized";
    };
    var ladda = null;

    function init() {
        var templateElement = document.getElementById("js-confirmation-template");
        if (templateElement) {
            confirmationTemplate = Handlebars.compile(templateElement.innerHTML);
        }
    }

    function showConfirmationModal(config) {
        config.YesAction = 'confirmationModule.callbackClick(function(){' + config.YesAction + '});';

        var innerHtml = confirmationTemplate(config);

        $('#Modal_Public_ConfirmationModal_content').html(innerHtml);
        $('#Modal_Public_ConfirmationModal').modal('show');
    }

    function hideConfirmationModal() {
        $('#Modal_Public_ConfirmationModal').modal('hide');
        destroyLadda();
    }

    function destroyLadda() {
        if (ladda && ladda.stop) {
            ladda.stop(); ladda.remove(); ladda = null;
        }
    }

    function callbackClick(action) {
        if (ladda) {
            return;
        }

        ladda = Ladda.create(document.querySelector('#Modal_Public_ConfirmationModal_content .js-ladda-button'));
        ladda.start();

        action();
    }

    function callbackYesAction(r, s, x) {
        hideConfirmationModal();

        var ajaxRedirect = x.getResponseHeader('X-Ajax-Redirect');
        if (ajaxRedirect !== null) {
            location.href = ajaxRedirect;
        }
        else if (r.Success) {
            location.reload(true);
        }
    }

    return {
        init: init,
        callbackClick: callbackClick,
        showConfirmationModal: showConfirmationModal,
        hideConfirmationModal: hideConfirmationModal,
        callbackYesAction: callbackYesAction
    };
})();

$(function () {
    confirmationModule.init();

    $(document).on('click', '.js-confirmation', function (e) {
        e.preventDefault();
        e.stopPropagation();

        var confirmation = $(this).data('confirmation');
        var posturl = $(this).data('posturl');
        var yesAction = '';
        if (posturl === undefined || posturl === null) {
            yesAction = 'confirmationModule.hideConfirmationModal(); ' + $(this).data('onclick');
        }
        else {
            yesAction = "$.post('" + posturl + "', null, confirmationModule.callbackYesAction);";
        }
        confirmationModule.showConfirmationModal({ Confirmation: confirmation, YesAction: yesAction });
    });

});