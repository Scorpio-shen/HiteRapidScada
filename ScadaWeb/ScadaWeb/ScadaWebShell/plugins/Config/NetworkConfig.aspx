<%@ Page Title="Web Application Configuration" Language="C#" MasterPageFile="~/MasterMain.Master" AutoEventWireup="true" CodeBehind="NetworkConfig.aspx.cs" Inherits="Scada.Web.plugins.Config.NetworkConfig" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cphMainHead" runat="server">
    <link href="css/webconfig.min.css" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cphMainContent" runat="server">
    <asp:Panel ID="pnlErrMsg" runat="server" CssClass="alert alert-danger alert-dismissible">
        <button type="button" class="close" data-dismiss="alert"><span>&times;</span></button>
        <asp:Label ID="lblErrMsg" runat="server" Text=""></asp:Label>
    </asp:Panel>
    <asp:Panel ID="pnlSuccMsg" runat="server" CssClass="alert alert-success alert-dismissible">
        <button type="button" class="close" data-dismiss="alert"><span>&times;</span></button>
        <asp:Label ID="lblSuccMsg" runat="server" Text=""></asp:Label>
    </asp:Panel>
    <h1><asp:Label ID="lblTitle_Database" runat="server" Text="网络参数配置(预留页面)"></asp:Label></h1>
    </asp:Content>