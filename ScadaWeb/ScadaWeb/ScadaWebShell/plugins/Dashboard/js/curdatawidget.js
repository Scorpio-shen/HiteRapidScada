// Input channel filter for data requests
var cnlFilter = null;
// jQuery cells those display current data
var curDataCells = null;

// The variables below must be defined in CurDataWidget.aspx
// Current data refresh rate
var dataRefrRate = dataRefrRate || 1000;


// The object creates a document model used by a dashboard to PDF export
var exportObj = {
    // Create an object describes PDF content for pdf maker
    createPdfContent: function () {
        var tableBody = [];

        $("tr").each(function () {
            var tableRow = [];
            tableBody.push(tableRow);

            $(this).find("td").each(function () {
                var cellText = $(this).text();
                tableRow.push(cellText);
            });
        });

        return [
            {
                table: {
                    body: tableBody
                }
            }
        ];
    }
};


// Initialize the channel filter according to the query string parameter
function initCnlFilter() {
    cnlFilter = new scada.CnlFilter();
    cnlFilter.cnlNums = scada.utils.queryParamToIntArray(scada.utils.getQueryParam("cnlNums"));
    cnlFilter.viewIDs = scada.utils.queryParamToIntArray(scada.utils.getQueryParam("viewIDs"));
}

// Select and prepare the current data cells
function initCurDataCells() {
    // select cells
    curDataCells = $("td.cur");

    // copy data-cnl attributes from rows to the cells
    curDataCells.each(function () {
        $(this).attr("data-cnl", $(this).closest("tr").data("cnl"));
    });
}

// Start cyclic updating current data
function startUpdatingCurData() {
    updateCurData(function (success) {
        if (success) {
            hideError();
        } else {
            showError(phrases.LoadError);
        }

        setTimeout(startUpdatingCurData, dataRefrRate);
    });
}

// Request and display current data.
// callback is a function (success)
function updateCurData(callback) {
    scada.clientAPI.getCurCnlDataExt(cnlFilter, function (success, cnlDataExtArr) {
        if (success) {
            var cnlDataMap = scada.clientAPI.createCnlDataExtMap(cnlDataExtArr);

            curDataCells.each(function () {
                displayCellData($(this), cnlDataMap);
            });

            callback(true);
        } else {
            callback(false);
        }
    });
}

// Display the given data in the cell
function displayCellData(cell, cnlDataMap) {
    var cnlNum = parseInt(cell.attr("data-cnl"));
    if (cnlNum) {
        var cnlData = cnlDataMap.get(cnlNum);
        var text = "";
        var color = "";

        if (cnlData) {
            text = cnlData.TextWithUnit;
            color = cnlData.Color;
        }

        cell.text(text); // special characters are encoded
        cell.css("color", color);
    }
}

// Show error message
function showError(message) {
    $("<div class='widget-error'></div>").text(message).prependTo("body");
}

// Hide error message
function hideError() {
    $(".widget-error").remove();
}

$(document).ready(function () {
    scada.clientAPI.rootPath = "../../";
    scada.clientAPI.ajaxQueue = scada.ajaxQueueLocator.getAjaxQueue();
    initCnlFilter();
    initCurDataCells();

    // start updating data
    startUpdatingCurData();
});
