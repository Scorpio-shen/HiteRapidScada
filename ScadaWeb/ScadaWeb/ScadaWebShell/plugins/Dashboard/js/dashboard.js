// Widget types enumeration
var WidgetTypes = {
    CHART: "Chart",
    CUR_DATA: "CurData",
    VIEW: "View",
    CUSTOM_URL: "CustomUrl"
};

// Minimum column count
var MIN_COL_CNT = 1;
// Maximum column count
var MAX_COL_CNT = 4;
// Column count of the Bootstrap grid system
var BS_COL_CNT = 12;
// Minimum aspect ratio
var MIN_RATIO = 0.1;
// Maximum aspect ratio
var MAX_RATIO = 10;

// View in JSON format
var viewJson = viewJson || null;
// Localized phrases
var phrases = phrases || {};
// Dashboard configuration
var dashboardConfig = null;

// Load dashboard
function loadDashboard(key) {
    var operation = "DashboardSvc.svc/GetDashboardConfig";
    var reqSuccessful = false;

    $.ajax({
        url: operation + "?key=" + key,
        method: "GET",
        dataType: "json",
        cache: false
    })
    .done(function (data, textStatus, jqXHR) {
        try {
            var parsedData = $.parseJSON(data.d);
            if (parsedData.Success) {
                scada.utils.logSuccessfulRequest(operation);

                if (obtainDashboardConfig(parsedData)) {
                    showDashboard();
                    reqSuccessful = true;
                } else {
                    scada.utils.logProcessingError(operation, "Received data are incorrect");
                }
            } else {
                scada.utils.logServiceError(operation, parsedData.ErrorMessage);
            }
        }
        catch (ex) {
            scada.utils.logProcessingError(operation, ex.message);
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        scada.utils.logFailedRequest(operation, jqXHR);
    })
    .always(function () {
        if (!reqSuccessful) {
            showAlert(phrases.ReceiveDashboardError);
        }
    });
}

// Obtain received dashboard configuration
function obtainDashboardConfig(parsedData) {
    dashboardConfig = parsedData.Data;

    if (dashboardConfig &&
        typeof dashboardConfig.ColumnCount === "number" &&
        typeof dashboardConfig.AspectRatio === "number" &&
        Array.isArray(dashboardConfig.Widgets)) {

        // correct coulmn count if needed
        if (!Number.isInteger(dashboardConfig.ColumnCount)) {
            dashboardConfig.ColumnCount = Math.round(dashboardConfig.ColumnCount);
        }

        if (dashboardConfig.ColumnCount < MIN_COL_CNT) {
            dashboardConfig.ColumnCount = MIN_COL_CNT;
        } else if (dashboardConfig.ColumnCount > MAX_COL_CNT) {
            dashboardConfig.ColumnCount = MAX_COL_CNT;
        }

        // correct aspect ratio if needed
        if (dashboardConfig.AspectRatio < MIN_RATIO) {
            dashboardConfig.AspectRatio = MIN_RATIO;
        } else if (dashboardConfig.AspectRatio > MAX_RATIO) {
            dashboardConfig.AspectRatio = MAX_RATIO;
        }

        return true;
    } else {
        return false;
    }
}

// Show alert message
function showAlert(message) {
    var divAlert = $("<div class='alert alert-danger'></div>").text(message);
    $(".main-content").prepend(divAlert);
}

// Show dashboard by generating DOM
function showDashboard() {
    // create dashboard rows
    var divRow = null;
    var rowElems = [];
    var colClass = "col col-sm-" + BS_COL_CNT / dashboardConfig.ColumnCount;
    var colInd = 0;

    for (var widget of dashboardConfig.Widgets) {
        if (colInd === 0) {
            divRow = $("<div class='row'></div>");
            rowElems.push(divRow);
        }

        if (++colInd === dashboardConfig.ColumnCount) {
            colInd = 0;
        }

        var widgetElem = $("<div class='" + colClass + "'><div class='widget-wrapper'></div></div>");
        var widgetWrapper = widgetElem.find(".widget-wrapper");
        var frameWidget = $("<iframe class='widget-frame'></iframe>").attr("src", getWidgetUrl(widget));
        scada.utils.styleIOS(widgetWrapper);
        widgetWrapper.append(frameWidget);
        divRow.append(widgetElem);
    }

    // disable the export button if the dashboard is empty
    if (rowElems.length === 0) {
        $("#btnExportPdf").addClass("disabled");
    }

    // display the dashboard
    $("#divDashboard").append(rowElems);
    setTimeout(updateWidgetHeights, 0);
}

// Get URL of a widget frame according to the frame properties
function getWidgetUrl(widget) {
    try {
        switch(widget.TypeName)
        {
            case WidgetTypes.CHART:
                var chartOptions = null;

                if (typeof scada.chart.ChartOptions === "function") {
                    chartOptions = new scada.chart.ChartOptions();
                    chartOptions.mode = widget.Props.mode || "";
                    chartOptions.period = typeof widget.Props.period === "undefined" ? -1 : -widget.Props.period;
                    chartOptions.title = widget.Props.title || "";
                    chartOptions.config = widget.Props.config || "";
                }

                return "../../" + scada.chart.dialog.getChartUrl(
                    widget.Props.cnlNums, widget.Props.viewIDs, null, chartOptions);

            case WidgetTypes.CUR_DATA:
                return "CurDataWidget.aspx" +
                    "?cnlNums=" + widget.Props.cnlNums +
                    "&viewIDs=" + widget.Props.viewIDs +
                    "&title=" + (widget.Props.title ? encodeURIComponent(widget.Props.title) : "");

            case WidgetTypes.VIEW:
                return "../../" + scada.utils.getViewUrl(widget.Props.viewID, true);

            case WidgetTypes.CUSTOM_URL:
                return widget.Props.url;

            default:
                return "";
        }
    }
    catch (ex) {
        console.error(scada.utils.getCurTime() +
            " Error getting URL of the widget of type '" + widget.TypeName + "':", ex.message);
        return "";
    }
}

// Update widget heights to fit aspect ratio
function updateWidgetHeights() {
    var frames = $("iframe.widget-frame");

    if (frames.length > 0) {
        var firstFrame = frames.eq(0);
        var frameH = Math.round(firstFrame.width() / dashboardConfig.AspectRatio);

        frames.each(function () {
            $(this).height(frameH);
        });
    }
}

// Export dashboard to PDF
function exportToPdf() {
    // collect objects those implement export of widgets
    var exportObjects = [];

    $("iframe.widget-frame").each(function () {
        var frame = $(this);
        var frameWnd = frame[0].contentWindow;
        var exportObj = scada.utils.frameAvailable(frameWnd) ? frameWnd.exportObj : null;

        if (exportObj && exportObj.createPdfContent) {
            exportObjects.push(exportObj);
        } else {
            console.warn("Widget with URL '" + frame.attr("src") + "' does not support export to PDF");
        }
    });

    // perform export
    if (exportObjects.length > 0) {
        scada.export.lockExportButton($("#btnExportPdf"));
        generatePdf(exportObjects);
    } else {
        showAlert(phrases.NoWidgetsToExport);
    }
}

// Generate PDF document
function generatePdf(exportObjects) {
    // create document layout
    var docDefinition = {
        pageSize: "A4",
        pageOrientation: "landscape",
        pageMargins: [30, 25, 30, 25],

        content: [],

        footer: function (currentPage, pageCount) {
            return {
                columns: [
                    { text: "" },
                    { text: currentPage.toString(), style: "dashboardPageNumStyle" },
                    { text: "Rapid SCADA", style: "dashboardFooterStyle" }
                ]
            };
        },

        styles: {
            dashboardFooterStyle: {
                fontSize: 8,
                alignment: "right",
                margin: [0, 0, 30, 0]
            },
            dashboardPageNumStyle: {
                fontSize: 10,
                alignment: "center"
            }
        }
    };

    // add widgets to the document model
    for (var i = 0, last = exportObjects.length - 1; i < exportObjects.length; i++) {
        var exportObj = exportObjects[i];
        var pageContent = exportObj.createPdfContent();

        if (i < last) {
            var pageBreak = { text: "", pageBreak: "after" };
            pageContent.push(pageBreak);
        }

        // append content
        Array.prototype.push.apply(docDefinition.content, pageContent);

        // merge styles
        if (exportObj.createPdfStyles) {
            $.extend(docDefinition.styles, exportObj.createPdfStyles());
        }
    }

    // generate output document by pdfmake
    pdfMake.createPdf(docDefinition).download(scada.export.buildFileName("Dashboard", "pdf"));
}

$(document).ready(function () {
    if (viewJson) {
        // get a dashboard that is already loaded
        dashboardConfig = $.parseJSON(viewJson).Data;
        showDashboard();
    } else {
        // get dashboard key and load dashboard
        var key = scada.utils.getQueryParam("key");
        loadDashboard(key);
    }

    // disable the export button for iOS
    if (scada.utils.iOS()) {
        $("#btnExportPdf").addClass("disabled");
    }

    // export to PDF on the button click
    $("#btnExportPdf").click(function (event) {
        event.preventDefault();
        exportToPdf();
    });

    $(window).resize(function () {
        updateWidgetHeights();
    });
});
