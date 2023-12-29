//Add your custom javascript that you wanted added to all pages here
function initTooltips() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        tooltipTriggerEl.addEventListener("click", function () { bootstrap.Tooltip.getInstance(this).hide(); });
        return new bootstrap.Tooltip(tooltipTriggerEl, { trigger: "hover" })
    })
};
