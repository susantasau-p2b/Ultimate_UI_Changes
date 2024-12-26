
using Newtonsoft.Json;
using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
 

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class AnalyzerController : Controller
    {
        //
        // GET: /Analyzer/
        public ActionResult Index()
        {
           return View("~/Views/Payroll/MainViews/Analyzer/Index.cshtml");
            //string path = System.Web.HttpContext.Current.Server.MapPath("/Views/Payroll/MainViews/Analyzer/Analyzer.aspx");
            //return Redirect("~/Views/Payroll/MainViews/Analyzer/Analyzer.aspx");
        }
        public ActionResult Partial()
        {
            //var id = new Test();
            return View("~/Views/Shared/Payroll/_CheckPassword.cshtml");
        }

        public ActionResult GetQueryLabelData(string data, string data2, string Type)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.QueryParameter.Where(e => e.Type.ToUpper() == Type.ToUpper()).ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "QName", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public class remark_return_class
        {
            public Dictionary<string, object> Fields { get; set; }
            
        }

         

        public FileContentResult ExportToExcel(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int QID = Convert.ToInt32(data);
                var Qp = db.QueryParameter.Where(e => e.Id == QID).SingleOrDefault();
                string TableViewName = Qp.QName;
                var qurey = "select * from " + TableViewName + "";
                //var cmd1 = db.Database.SqlQuery<propertype>(qurey).ToList();
               
                string connString = ConfigurationManager.ConnectionStrings["DataBaseContext"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connString))
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(qurey, con))
                        {
                            cmd.CommandTimeout = 3000;
                            cmd.CommandType = System.Data.CommandType.Text;
                            con.Open();

                            SqlDataAdapter da = new SqlDataAdapter();
                            // DataSet ds = new DataSet();
                            DataTable dt = new DataTable();

                            da.SelectCommand = cmd;
                            da.Fill(dt);
                            //dt = ds.Tables["P2bTable"];
                            string[] heading = new string[dt.Columns.Count];

                            for (var index = 0; index < dt.Columns.Count; index++)
                            {
                                heading[index] = dt.Columns[index].Caption;
                            }

                            byte[] filecontent = ExcelExportHelper.ExportExcel(dt, null, null, null, heading, false, null);
                            return File(filecontent, ExcelExportHelper.ExcelContentType, TableViewName + ".xlsx");

                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }            
            }
        }

        //public FileContentResult ExportToPDF(string data)
        //{
        //    int QID = Convert.ToInt32(data);
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var Qp = db.QueryParameter.Where(e => e.Id == QID).SingleOrDefault();
        //        string TableViewName = Qp.QName;
        //        var qurey = "select top(10) EmpCode,FullNameFML from " + TableViewName + " Where Paymonth='05/2024'";
        //        //var cmd1 = db.Database.SqlQuery<propertype>(qurey).ToList();

        //        string connString = ConfigurationManager.ConnectionStrings["DataBaseContext"].ConnectionString;
        //        using (SqlConnection con = new SqlConnection(connString))
        //        {
        //            using (SqlCommand cmd = new SqlCommand(qurey, con))
        //            {
        //                cmd.CommandTimeout = 3000;
        //                cmd.CommandType = System.Data.CommandType.Text;
        //                con.Open();

        //                SqlDataAdapter da = new SqlDataAdapter();
        //               DataSet ds = new DataSet();
        //                DataTable dt = new DataTable();

        //                da.SelectCommand = cmd;
        //                da.Fill(dt);
        //                //dt = ds.Tables["P2bTable"];
        //                ds.Tables.Add(dt);
        //                byte[] filecontent = exportpdf(dt);
        //                string filename = "Sample_PDF_" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".pdf";
        //                return File(filecontent, "application/pdf", filename);

        //            }
        //        }
        //    }         
        //}

        //private byte[] exportpdf(DataTable dtEmployee)
        //{
        //    System.IO.MemoryStream ms = new System.IO.MemoryStream();
        //    iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(PageSize.A4);
        //    rec.BackgroundColor = new BaseColor(System.Drawing.Color.Olive);
        //    Document doc = new Document(rec);
        //    doc.SetPageSize(iTextSharp.text.PageSize.A4);
        //    PdfWriter writer = PdfWriter.GetInstance(doc, ms);
        //    doc.Open();
        //    BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        //    iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bfntHead, 16, 1, iTextSharp.text.BaseColor.BLUE);
        //    Paragraph prgHeading = new Paragraph();
        //    prgHeading.Alignment = Element.ALIGN_LEFT;
        //    prgHeading.Add(new Chunk("Dynamic Report PDF".ToUpper(), fntHead));
        //    doc.Add(prgHeading);
        //    Paragraph prgGeneratedBY = new Paragraph();
        //    BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        //    iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, iTextSharp.text.BaseColor.BLUE);
        //    prgGeneratedBY.Alignment = Element.ALIGN_RIGHT;
        //    doc.Add(prgGeneratedBY);
        //    Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
        //    doc.Add(p);
        //    doc.Add(new Chunk("\n", fntHead));
        //    PdfPTable table = new PdfPTable(dtEmployee.Columns.Count);
        //    for (int i = 0; i < dtEmployee.Columns.Count; i++)
        //    {
        //        string cellText = Server.HtmlDecode(dtEmployee.Columns[i].ColumnName);
        //        PdfPCell cell = new PdfPCell();
        //        cell.Phrase = new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
        //        cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.PaddingBottom = 5;
        //        table.AddCell(cell);
        //    }
        //    for (int i = 0; i < dtEmployee.Rows.Count; i++)
        //    {
        //        for (int j = 0; j < dtEmployee.Columns.Count; j++)
        //        {
        //            table.AddCell(dtEmployee.Rows[i][j].ToString());
        //        }
        //    }
        //    doc.Add(table);
        //    doc.Close();
        //    byte[] result = ms.ToArray();
        //    return result;
        //}
    }
}
