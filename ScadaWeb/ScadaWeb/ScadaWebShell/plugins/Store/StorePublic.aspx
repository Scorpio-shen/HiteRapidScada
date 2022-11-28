<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StorePublic.aspx.cs" Inherits="Scada.Web.Plugins.Store.WFrmStorePublic" %>
<%@ Import Namespace="Scada.Web.Plugins.Store" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <!-- The above 3 meta tags are required by Bootstrap -->
    <title>Download Plugins</title>
    <link href="~/images/favicon.ico" rel="shortcut icon" type="image/x-icon" />
    <link href="~/lib/bootstrap/css/bootstrap.min.css" rel="stylesheet" type="text/css" />
    <link href="~/lib/font-awesome/css/font-awesome.min.css" rel="stylesheet" type="text/css" />
    <link href="~/lib/open-sans/css/open-sans.css" rel="stylesheet" type="text/css" />
    <link href="~/css/controls/popup.min.css" rel="stylesheet" type="text/css" />
    <link href="~/plugins/Store/css/storepublic.min.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="../../lib/jquery/jquery.min.js"></script>
    <script type="text/javascript" src="../../lib/bootstrap/js/bootstrap.min.js"></script>
    <script type="text/javascript" src="../../js/api/utils.js"></script>
    <script type="text/javascript" src="../../js/api/eventtypes.js"></script>
    <script type="text/javascript" src="../../js/controls/popup.js"></script>
    <script type="text/javascript">
        var phrases = <%= Scada.Web.WebUtils.DictionaryToJs("Scada.Web.Plugins.Store.WFrmStore.Js") %>;
        var locale = "<%= Request.QueryString["locale"] ?? Scada.Localization.Culture.Name %>";
        var popup = new scada.Popup();
        scada.modalButtonCaptions = <%= Scada.Web.WebUtils.DictionaryToJs("Scada.Web.Js.ModalButtons") %>;
    </script>
    <script type="text/javascript" src="js/storecommon.js"></script>
    <script type="text/javascript" src="js/store.js"></script>
</head>
<body>
    <form id="frmStorePublic" runat="server">
        <div class="plain-header"><asp:Label ID="lblHeader" runat="server" Text="Module Repository of Rapid SCADA"></asp:Label></div>
        <div class="main-content">
            <table id="tblPlugins" class="table table-striped data-empty">
                <tr>
                    <th><%= PlgPhrases.NameCol %></th>
                    <th><%= PlgPhrases.DescrCol %></th>
                    <th><%= PlgPhrases.PriceCol %></th>
                    <th><%= PlgPhrases.AuthorCol %></th>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>
