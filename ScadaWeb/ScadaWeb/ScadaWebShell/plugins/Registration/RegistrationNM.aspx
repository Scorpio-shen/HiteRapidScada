<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RegistrationNM.aspx.cs" Inherits="Scada.Web.Plugins.Registration.WFrmRegistrationNM" %>
<%@ Register Src="~/plugins/Registration/CtrlRegistration.ascx" TagPrefix="uc" TagName="CtrlRegistration" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <!-- The above 3 meta tags are required by Bootstrap -->
    <title>Registration</title>
    <link href="~/images/favicon.ico" rel="shortcut icon" type="image/x-icon" />
    <link href="~/lib/bootstrap/css/bootstrap.min.css" rel="stylesheet" type="text/css" />
    <link href="~/lib/font-awesome/css/font-awesome.min.css" rel="stylesheet" type="text/css" />
    <link href="~/lib/open-sans/css/open-sans.css" rel="stylesheet" type="text/css" />
    <link href="~/plugins/Registration/css/registrationnm.min.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="../../lib/jquery/jquery.min.js"></script>
    <script type="text/javascript" src="../../lib/bootstrap/js/bootstrap.min.js"></script>
    <script type="text/javascript" src="js/registration.js"></script>
</head>
<body>
    <form id="frmRegistrationNM" runat="server">
        <div class="plain-header">Rapid SCADA</div>
        <div class="main-content">
            <uc:CtrlRegistration runat="server" ID="ctrlRegistration" />
        </div>
    </form>
</body>
</html>
