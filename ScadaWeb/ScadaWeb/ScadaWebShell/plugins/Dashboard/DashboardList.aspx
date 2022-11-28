<%@ Page Title="Dashboards" Language="C#" MasterPageFile="~/MasterMain.Master" AutoEventWireup="true" CodeBehind="DashboardList.aspx.cs" Inherits="Scada.Web.Plugins.Dashboard.WFrmDashboardList" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cphMainHead" runat="server">
    <link href="css/dashboardlist.min.css" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cphMainContent" runat="server">
    <h1><asp:Label ID="lblTitle" runat="server" Text="Dashboards"></asp:Label></h1>
    <asp:Repeater ID="repDashboards" runat="server">
        <ItemTemplate>
            <div class="report-item">
                <asp:HyperLink ID="hlDashboardItem" runat="server" 
                    NavigateUrl='<%# VirtualPathUtility.ToAbsolute((string)Eval("Url")) %>'><%# (Container.ItemIndex + 1).ToString() + ". " + HttpUtility.HtmlEncode(Eval("Text")) %></asp:HyperLink>
            </div>
        </ItemTemplate>
    </asp:Repeater>
    <asp:Label ID="lblNoDashboards" runat="server" Text="No dashboards available"></asp:Label>
</asp:Content>
