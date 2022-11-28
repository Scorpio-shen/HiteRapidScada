<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/MasterMain.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Scada.Web.Plugins.Dashboard.WFrmDashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cphMainHead" runat="server">
    <link href="../../css/common/contentform.min.css" rel="stylesheet" type="text/css" />
    <link href="css/dashboard.min.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="../../lib/pdfmake/pdfmake.min.js"></script>
    <script type="text/javascript" src="../../lib/pdfmake/vfs_fonts.js"></script>
    <script type="text/javascript" src="../../js/api/export.js"></script>
    <script type="text/javascript">
        var phrases = <%= Scada.Web.WebUtils.DictionaryToJs("Scada.Web.Plugins.Dashboard.WFrmDashboard.Js") %>;
    </script>
    <script type="text/javascript" src="js/dashboard.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cphMainContent" runat="server">
    <div id="divDashboardToolbar" class="form-inline">
        <div class="form-group">
            <asp:DropDownList ID="ddlDashboard" runat="server" CssClass="form-control" AutoPostBack="True" OnSelectedIndexChanged="ddlDashboard_SelectedIndexChanged"></asp:DropDownList>
        </div>
        <asp:Button ID="btnExportPdf" ClientIDMode="Static" runat="server" CssClass="btn btn-default" Text="Export to PDF" />
    </div>
    <div id="divDashboard">
    </div>
</asp:Content>
