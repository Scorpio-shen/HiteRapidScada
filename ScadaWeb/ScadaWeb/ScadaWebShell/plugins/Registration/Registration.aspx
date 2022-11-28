<%@ Page Title="Registration" Language="C#" MasterPageFile="~/MasterMain.Master" AutoEventWireup="true" CodeBehind="Registration.aspx.cs" Inherits="Scada.Web.Plugins.Registration.WFrmRegistration" %>
<%@ Register Src="~/plugins/Registration/CtrlRegistration.ascx" TagPrefix="uc" TagName="CtrlRegistration" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cphMainHead" runat="server">
    <link href="css/registration.min.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="js/registration.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cphMainContent" runat="server">
    <uc:CtrlRegistration runat="server" ID="ctrlRegistration" />
</asp:Content>
