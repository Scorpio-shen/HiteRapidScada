<%@ Page Title="Download Plugins" Language="C#" MasterPageFile="~/MasterMain.Master" AutoEventWireup="true" CodeBehind="Store.aspx.cs" Inherits="Scada.Web.Plugins.Store.WFrmStore" %>
<%@ Import Namespace="Scada.Web.Plugins.Store" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cphMainHead" runat="server">
    <link href="css/store.min.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript">
        var phrases = <%= Scada.Web.WebUtils.DictionaryToJs("Scada.Web.Plugins.Store.WFrmStore.Js") %>;
        var locale = "<%= Request.QueryString["locale"] ?? Scada.Localization.Culture.Name %>";
    </script>
    <script type="text/javascript" src="js/storecommon.js"></script>
    <script type="text/javascript" src="js/store.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cphMainContent" runat="server">
    <h1><asp:Label ID="lblTitle" runat="server" Text="Download Plugins"></asp:Label></h1>
</asp:Content>
