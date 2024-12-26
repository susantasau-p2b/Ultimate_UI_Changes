<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportData.aspx.cs" Inherits="EssPortal.Reports.Payroll.ReportData" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845DCD8080CC91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
  
        <div style="margin:0 auto;">
        <rsweb:ReportViewer ID="ReportViewer1" runat="server"  Height="100%" Width="100%" ZoomMode="PageWidth" SizeToReportContent="True"></rsweb:ReportViewer>
        </div>
   
    </form>
    
</body>
</html>

