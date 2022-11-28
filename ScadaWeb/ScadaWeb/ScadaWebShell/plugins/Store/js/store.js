// Get plugin list from the Store service
function getPlugins() {
    var operation = "StoreSvc.svc/GetModules";

    $.ajax({
        url: API_PATH + operation + "?locale=" + locale,
        method: "GET",
        dataType: "json",
        cache: false
    })
    .done(function (data, textStatus, jqXHR) {
        try {
            var parsedData = $.parseJSON(data.d);
            if (parsedData.Success) {
                scada.utils.logSuccessfulRequest(operation);
                obtainPlugins(parsedData);
            } else {
                scada.utils.logServiceError(operation, parsedData.ErrorMessage);
                showAlert(phrases.GetPluginsError);
            }
        }
        catch (ex) {
            scada.utils.logProcessingError(operation, ex.message);
            showAlert(phrases.GetPluginsError);
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        scada.utils.logFailedRequest(operation, jqXHR);
        showAlert(phrases.GetPluginsError);
    });
}

// Obtain and display received plugins
function obtainPlugins(parsedData) {
    if (Array.isArray(parsedData.Data)) {
        var rows = [];

        for (var module of parsedData.Data) {
            if ("ModuleID" in module) {
                rows.push(createPluginRow(module));
            }
        }

        if (rows.length > 0) {
            $("#tblPlugins").append(rows);
            bindPluginEvents();
        } else {
            showAlert(phrases.NoPlugins);
        }
    }
}

// Create new plugin row
function createPluginRow(module) {
    var row = $("<tr>");

    // name cell
    var nameSpan = $("<span>").text(module.Name)

    if (module.New) {
        var newSpan = $("<span class='label label-info'></span>").text(phrases.PluginNew);
        nameSpan.append(" ").append(newSpan);
    }

    if (module.Recommended) {
        var recomSpan = $("<span class='label label-info'><i class='fa fa-thumbs-o-up'></i></span>");
        nameSpan.append(" ").append(recomSpan);
    }

    var infoBtn = $("<a href='' class='plugin-info-btn'></a>")
        .text(phrases.PluginInfoBtn)
        .data("module-code", module.Code);
    $("<td class='name-cell'>").append(nameSpan).append("<br />").append(infoBtn).appendTo(row);

    // description cell
    $("<td>").text(module.ShortDescr).appendTo(row);

    // price cell
    var price = formatPrice(module);
    $("<td class='price-cell'>").text(price).appendTo(row);

    // author cell
    $("<td>").text(module.AuthorName).appendTo(row);

    return row;
}

// Bind events of the plugin list
function bindPluginEvents() {
    // open plugin info on button click
    $("a.plugin-info-btn").click(function () {
        var code = $(this).data("module-code");
        popup.showModal("PluginInfo.aspx?code=" + code + "&locale=" + locale,
            new scada.ModalOptions([scada.ModalButtons.CLOSE]));
        return false;
    });
}

// Show alert message
function showAlert(message) {
    var divAlert = $("<div class='alert alert-danger'></div>").text(message);
    $(".main-content").prepend(divAlert);
}

$(document).ready(function () {
    // fill plugin list
    if ($("#tblPlugins").hasClass("data-empty")) {
        $("#tblPlugins").removeClass("data-empty");
        getPlugins();
    }
});