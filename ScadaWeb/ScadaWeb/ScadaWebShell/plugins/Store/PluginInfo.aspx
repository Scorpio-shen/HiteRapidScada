<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PluginInfo.aspx.cs" Inherits="Scada.Web.Plugins.Store.WFrmPluginInfo" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <!-- The above 3 meta tags are required by Bootstrap -->
    <title>Plugin Info</title>
    <link href="~/lib/bootstrap/css/bootstrap.min.css" rel="stylesheet" type="text/css" />
    <link href="~/lib/font-awesome/css/font-awesome.min.css" rel="stylesheet" type="text/css" />
    <link href="~/lib/open-sans/css/open-sans.css" rel="stylesheet" type="text/css" />
    <link href="~/plugins/Store/css/plugininfo.min.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="../../lib/jquery/jquery.min.js"></script>
    <script type="text/javascript" src="../../lib/bootstrap/js/bootstrap.min.js"></script>
    <script type="text/javascript" src="../../js/api/utils.js"></script>
    <script type="text/javascript" src="../../js/api/eventtypes.js"></script>
    <script type="text/javascript" src="../../js/controls/popup.js"></script>
    <script type="text/javascript">
        var phrases = <%= Scada.Web.WebUtils.DictionaryToJs("Scada.Web.Plugins.Store.WFrmPluginInfo.Js") %>;
    </script>
    <script type="text/javascript" src="js/storecommon.js"></script>
    <script type="text/javascript" src="js/plugininfo.js"></script>
</head>
<body>
    <form id="frmPluginInfo" runat="server">
        <ul class="nav nav-tabs">
          <li class="active"><a href="#divGeneralTab" data-toggle="tab"><asp:Label ID="lblGeneralTab" runat="server" Text="General"></asp:Label></a></li>
          <li><a href="#divScreenshotsTab" data-toggle="tab"><asp:Label ID="lblScreenshotsTab" runat="server" Text="Screenshots"></asp:Label></a></li>
        </ul>

        <div class="tab-content">
            <div id="divGeneralTab" class="tab-pane active">
                <div class="form-group">
                    <asp:Label ID="lblDescrHdr" runat="server" CssClass="section-header" Text="Description"></asp:Label>
                    <span id="spanDescr" class="section-text"></span>
                </div>
                <div class="form-group">
                    <asp:Label ID="lblPriceHdr" runat="server" CssClass="section-header" Text="Price"></asp:Label>
                    <span id="spanPrice" class="section-text"></span>
                    <span id="spanBuy" class="section-text hidden"><asp:HyperLink ID="hlBuy" runat="server" Target="_blank">Click here to buy</asp:HyperLink></span>
                </div>
                <div class="form-group">
                    <asp:Label ID="lblAuthorHdr" runat="server" CssClass="section-header" Text="Author"></asp:Label>
                    <span id="spanAuthor" class="section-text"></span>
                </div>
                <div id="divURLs" class="form-group">
                    <asp:Label ID="lblUrlHdr" runat="server" CssClass="section-header" Text="Web pages"></asp:Label>
                    <span id="spanHomeURL" class="section-text hidden"><asp:HyperLink ID="hlHome" runat="server" Target="_blank">Home page</asp:HyperLink></span>
                    <span id="spanDocURL" class="section-text hidden"><asp:HyperLink ID="hlDoc" runat="server" Target="_blank">Documentation</asp:HyperLink></span>
                </div>
                <div id="divDownload" class="form-group">
                    <asp:Label ID="lblDownloadHdr" runat="server" CssClass="section-header" Text="Download"></asp:Label>
                </div>
            </div>
            <div id="divScreenshotsTab" class="tab-pane">
            </div>
        </div>
    </form>
</body>
</html>
