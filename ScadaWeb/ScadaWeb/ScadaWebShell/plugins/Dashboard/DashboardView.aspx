<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DashboardView.aspx.cs" Inherits="Scada.Web.Plugins.Dashboard.WFrmDashboardView" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <!-- The above 3 meta tags are required by Bootstrap -->
    <title>Dashboard</title>
    <link href="../../lib/bootstrap/css/bootstrap.min.css" rel="stylesheet" type="text/css" />
    <link href="css/dashboardview.min.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="../../lib/jquery/jquery.min.js"></script>
    <script type="text/javascript" src="../../lib/bootstrap/js/bootstrap.min.js"></script>
    <script type="text/javascript" src="../../js/api/utils.js"></script>
    <%= GenChartScriptPathHtml() %>
    <script type="text/javascript">
        var viewJson = '<%= viewJson %>';
        var phrases = <%= Scada.Web.WebUtils.DictionaryToJs("Scada.Web.Plugins.Dashboard.WFrmDashboard.Js") %>;
    </script>
    <script type="text/javascript" src="js/dashboard.js"></script>
</head>
<body>
    <div id="divDashboard"></div>
</body>
</html>
