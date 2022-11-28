<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CurDataWidget.aspx.cs" Inherits="Scada.Web.Plugins.Dashboard.WFrmCurDataWidget" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Current Data Widget</title>
    <link href="css/curdatawidget.min.css" rel="stylesheet" type="text/css" />
    <link href="~/lib/open-sans/css/open-sans.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="../../lib/jquery/jquery.min.js"></script>
    <script type="text/javascript" src="../../js/api/utils.js"></script>
    <script type="text/javascript" src="../../js/api/ajaxqueue.js"></script>
    <script type="text/javascript" src="../../js/api/clientapi.js"></script>
    <script type="text/javascript">
        var dataRefrRate = <%= dataRefrRate %>;
        var phrases = <%= Scada.Web.WebUtils.DictionaryToJs("Scada.Web.Plugins.Dashboard.WFrmCurDataWidget.Js") %>;
    </script>
    <script type="text/javascript" src="js/curdatawidget.js"></script>
</head>
<body>
    <form id="frmCurDataWidget" runat="server">
    <%= widgetHtml %>
    </form>
</body>
</html>
