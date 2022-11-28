<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CtrlRegistration.ascx.cs" Inherits="Scada.Web.Plugins.Registration.CtrlRegistration" %>
<asp:Panel ID="pnlErrMsg" runat="server" CssClass="alert alert-danger alert-dismissible">
    <button type="button" class="close" data-dismiss="alert"><span>&times;</span></button>
    <asp:Label ID="lblErrMsg" runat="server" Text=""></asp:Label>
</asp:Panel>
<h1><asp:Label ID="lblTitle" runat="server" Text="{0} Registration"></asp:Label></h1>
<div class="form-group">
    <asp:Label ID="lblCompCode" runat="server" Text="Computer code" AssociatedControlID="txtCompCode"></asp:Label>
    <asp:TextBox ID="txtCompCode" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" ReadOnly="True"></asp:TextBox>
</div>
<div class="form-group">
    <asp:Label ID="lblRegKey" runat="server" Text="Registration key" AssociatedControlID="txtRegKey"></asp:Label>
    <asp:TextBox ID="txtRegKey" runat="server" CssClass="form-control" autocomplete="off"></asp:TextBox>
    <p class="help-block">
        <asp:HyperLink ID="hlGetPermanentKey" runat="server" Target="_blank">Purchase a permanent key</asp:HyperLink><br />
        <asp:HyperLink ID="hlGetTrialKey" runat="server" Target="_blank">Get a trial key</asp:HyperLink></p>
</div>
<asp:Panel ID="pnlKeyStatus" runat="server" ClientIDMode="Static" CssClass="form-group">
    <asp:Label ID="lblKeyStatus" runat="server" CssClass="control-label" Text="Key status" AssociatedControlID="txtKeyStatus"></asp:Label>
    <asp:TextBox ID="txtKeyStatus" runat="server" CssClass="form-control" ReadOnly="True"></asp:TextBox>
    <span class="form-control-feedback"><i></i></span>
</asp:Panel>
<div class="form-group">
    <asp:Button ID="btnSaveKey" runat="server" CssClass="btn btn-primary" Text="Save Key" OnClick="btnSaveKey_Click" />
</div>
