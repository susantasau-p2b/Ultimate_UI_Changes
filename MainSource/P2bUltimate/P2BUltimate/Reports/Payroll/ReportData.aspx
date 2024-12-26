<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeBehind="ReportData.aspx.cs" Inherits="P2BUltimate.Reports.Payroll.ReportData" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845DCD8080CC91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="../../Scripts/jquery-2.2.1.js"></script>
    <script>
        $(function () {
            debugger;
            var menulink = window.location.pathname.toLowerCase().substr(1).split('/')[window.location.pathname.toLowerCase().substr(1).split('/').length - 1];
            var urlparm = new URLSearchParams(window.location.search);

            var a = localStorage.getItem(urlparm.get("reportname"));
            console.log(a);
            if (a != null) {
                $('#TextBox1').val(a);
            }
            var aa2 = localStorage.getItem("LeaveCreditProcess");
            if (aa2 != null) {
                $('#textbox2').val(aa2);
            }
            var aa3 = localStorage.getItem("BeforeActionOnAttendanceLV");
            if (aa3 != null) {
                $('#TextBox3').val(aa3);
            }
        });
    </script>
    <style>
        .notShow {
            display: none;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <br />
        <asp:Button runat="server" ID="ShowViewer" Text="Viewer Ready,Click Here To Generate" OnClick="ShowViewer_Click" />
        <input type="text" runat="server" id="TextBox1" class="notShow" />
        <input type="text" runat="server" id="textbox2" class="notShow" />
        <input type="text" runat="server" id="TextBox3" class="notShow" />

        <asp:ScriptManager ID="ScriptManager1" runat="server" AsyncPostBackTimeout="56000"></asp:ScriptManager>
        <div style="margin: 0 auto;">
            <rsweb:ReportViewer ID="ReportViewer1" runat="server" ZoomMode="PageWidth" SizeToReportContent="True" AsyncPostBackTimeOut="560000" Height="100%" Width="100%"></rsweb:ReportViewer>
        </div>

    </form>

</body>
</html>

