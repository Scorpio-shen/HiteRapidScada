$(document).ready(function () {
    // hide header if the form is inside a frame
    if (window.parent != window) {
        $(".plain-header").css("display", "none");
    }

    // display key status
    var pnlKeyStatus = $("#pnlKeyStatus");
    var statusSign = pnlKeyStatus.find(".form-control-feedback i");

    if (pnlKeyStatus.hasClass("valid")) {
        pnlKeyStatus.addClass("form-group has-success has-feedback");
        statusSign.addClass("fa fa-check");
    } else if (pnlKeyStatus.hasClass("validwithwarning")) {
        pnlKeyStatus.addClass("form-group has-warning has-feedback");
        statusSign.addClass("fa fa-exclamation-triangle");
    } else {
        pnlKeyStatus.addClass("form-group has-error has-feedback");
        statusSign.addClass("fa fa-times");
    }
});