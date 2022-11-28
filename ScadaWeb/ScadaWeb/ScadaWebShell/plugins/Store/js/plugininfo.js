// Author ID of Rapid SCADA team
var OWNER_AUTHOR_ID = 1;
// Payment form links
var BUY_URL_EN = "https://rapidscada.org/download-all-files/purchase-module/";
var BUY_URL_RU = "https://rapidscada.ru/download-all-files/purchase-module/";

// Popup dialogs manipulation object
var popup = scada.popupLocator.getPopup();
// Plugin information already loaded
var infoLoaded = false;

// Update the modal dialog height according to a frame height
function updateModalHeight() {
    if (popup) {
        setTimeout(popup.updateModalHeight, 0, window);
    }
}

// Get plugin info from the Store service
function getPluginInfo() {
    var operation = "StoreSvc.svc/GetModule";

    $.ajax({
        url: API_PATH + operation + location.search,
        method: "GET",
        dataType: "json",
        cache: false
    })
    .done(function (data, textStatus, jqXHR) {
        try {
            var parsedData = $.parseJSON(data.d);
            if (parsedData.Success) {
                scada.utils.logSuccessfulRequest(operation);
                obtainPluginInfo(parsedData);
            } else {
                scada.utils.logServiceError(operation, parsedData.ErrorMessage);
                showAlert(phrases.GetPluginInfoError);
            }
        }
        catch (ex) {
            scada.utils.logProcessingError(operation, ex.message);
            showAlert(phrases.GetPluginInfoError);
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        scada.utils.logFailedRequest(operation, jqXHR);
        showAlert(phrases.GetPluginInfoError);
    })
    .always(function () {
        updateModalHeight();
    });
}

// Obtain and display received plugin information
function obtainPluginInfo(parsedData) {
    var module = parsedData.Data;

    if (module.ModuleID) {
        // title
        popup.setModalTitle(window, module.Name);

        // description
        $("#spanDescr").text(module.LongDescr);

        // price
        $("#spanPrice").text(formatPrice(module));

        if (module.AuthorID === OWNER_AUTHOR_ID && (module.PersonalPrice || module.BusinessPrice)) {
            var isRussian = scada.utils.getQueryParam("locale") === "ru";
            $("#spanBuy").removeClass("hidden");
            $("#spanBuy").find("a")
                .attr("href", isRussian ? BUY_URL_RU : BUY_URL_EN)
                .append(" <i class='fa fa-shopping-cart fa-lg'></i>"); // add cart icon
        }

        // author
        $("#spanAuthor").text(module.AuthorName);

        // web pages
        if (module.HomeURL || module.DocURL) {
            if (module.HomeURL) {
                $("#spanHomeURL a").attr("href", module.HomeURL);
                $("#spanHomeURL").removeClass("hidden");
            }

            if (module.DocURL) {
                $("#spanDocURL a").attr("href", module.DocURL);
                $("#spanDocURL").removeClass("hidden");
            }
        } else {
            $("#divURLs").addClass("hidden");
        }

        // downlaod
        if (Array.isArray(module.Versions) && module.Versions.length > 0) {
            var lblDownloadHdr = $("#divDownload .section-header");

            for (var version of module.Versions) {
                var verItem = $("<span class='section-text'></span>");

                if (version.Compatibility) {
                    verItem.text(", " + version.Compatibility);
                }

                $("<a target='_blank'>")
                    .text(version.Name)
                    .attr("href", version.DownloadURL)
                    .prependTo(verItem);

                verItem.insertAfter(lblDownloadHdr);
            }
        } else {
            $("#divDownload").addClass("hidden");
        }

        // screenshots
        if (Array.isArray(module.Screenshots) && module.Screenshots.length > 0) {
            var divScreenshotsTab = $("#divScreenshotsTab");

            for (var screenshot of module.Screenshots) {
                var scrItem = $("<div class='form-group'></div>")
                    .append("<a target='_blank'><img class='screenshot' /><a>");
                scrItem.find("a").attr("href", screenshot.ImageURL);
                scrItem.find("img").attr("src", screenshot.ImageURL);

                if (screenshot.Caption) {
                    $("<span class='section-text'></span>").text(screenshot.Caption).appendTo(scrItem);
                }

                divScreenshotsTab.append(scrItem);
            }
        } else {
            $(".nav-tabs").addClass("hidden");
        }
    }
}

// Show alert message
function showAlert(message) {
    var divAlert = $("<div class='alert alert-danger'></div>").text(message);
    $("form").prepend(divAlert);
}

$(document).ready(function () {
    if (!infoLoaded) {
        infoLoaded = true;
        getPluginInfo();
    }

    $(".nav a").click(function () {
        updateModalHeight();
    });
});