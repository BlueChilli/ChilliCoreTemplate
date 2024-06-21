//Add your custom javascript that you wanted added to all pages here
function initTooltips() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        tooltipTriggerEl.addEventListener("click", function () { bootstrap.Tooltip.getInstance(this).hide(); });
        return new bootstrap.Tooltip(tooltipTriggerEl, { trigger: "hover" })
    })
};

function shorten(text, max = 25, to = 20, postfix = '&centerdot;&centerdot;&centerdot;') {
    if (text == null) return '';
    return text.length > max ? '<span data-bs-toggle="tooltip" data-bs-original-title="{0}">{1}</span>'.format(text, text.substr(0, to) + postfix) : text;
}
