using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace P2BUltimate.Controllers
{
    public class ExcelMappingController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_ExcelMapping.cshtml");
        }
        public class GetJsonData
        {
            public String name { get; set; }
            public String value { get; set; }
        }
        public ActionResult GetMapping(String FileName)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (FileName != null && FileName != "")
                {
                    var a = db.ExcelMapping.Where(e => e.TableName == FileName).OrderByDescending(e => e.Id).FirstOrDefault();

                    if (a != null)
                    {
                        return Json(new { success = true, responseText = "ok", obj = a }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "error", data = "0" }, JsonRequestBehavior.AllowGet);

                    }
                }
                else
                {
                    return Json(new { success = false, responseText = "FileName Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        [HttpPost]
        public ActionResult Update(List<GetJsonData> oGetJsonData)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (oGetJsonData.Count > 0)
                {
                    var jsonSerialiser = new JavaScriptSerializer();

                    var TableName = oGetJsonData.Where(e => e.name == "TableName").SingleOrDefault();
                    var LineNo = oGetJsonData.Where(e => e.name == "LineNo").SingleOrDefault();
                    oGetJsonData.RemoveAll(e => e.name == "TableName");
                    oGetJsonData.RemoveAll(e => e.name == "LineNo");

                    var json = jsonSerialiser.Serialize(oGetJsonData);
                    ExcelMapping oExcelMapping = new ExcelMapping
                    {
                        Mapping = json,
                    };
                    if (TableName != null)
                    {
                        oExcelMapping.TableName = TableName.value;
                    }
                    if (LineNo != null)
                    {
                        oExcelMapping.LineNo = LineNo.value;
                    }
                    try
                    {
                        db.ExcelMapping.Add(oExcelMapping);
                        db.SaveChanges();
                        return Json(new { responseText = "Record Updated..!", success = true }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {
                        return Json(new { responseText = e.InnerException.Message.ToString(), success = false }, JsonRequestBehavior.AllowGet);

                    }

                }
                else
                {
                    return Json(new { responseText = "No Data Found..!", success = false }, JsonRequestBehavior.AllowGet);

                }
            }
        }
        public ActionResult FindMapping(String FileName)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (FileName != null && FileName != "")
                {
                    var a = db.ExcelMapping.Where(e => e.TableName == FileName).ToList();
                    if (a != null && a.Count > 0)
                    {
                        var json = a.OrderByDescending(e => e.Id).FirstOrDefault();
                        return Json(new { success = true, responseText = json.Mapping, data = "1" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = true, responseText = "error", data = "0" }, JsonRequestBehavior.AllowGet);

                    }
                }
                else
                {
                    return Json(new { success = false, responseText = "FileName Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public String FileName(String Name)
        {
            switch (Name.ToUpper())
            {
                case "LOOKUP":
                    return "Lookup";
                case "OTHEARNINGDEDUCTIONT":
                    return "OthEarningDeductionT";
                case "ITAXTRANST":
                    return "ITaxTransT";
                case "OTHEREARNINGT":
                    return "OthEarningDeductionT";
                case "EMPOFF":
                    return "EmpOff";
                case "LOANADVREQUEST":
                    return "LoanAdvRequest";
                case "LOANADVREPAYMENTT":
                    return "LoanAdvRepaymentT";
                case "EMPLOYEE":
                    return "Employee";
                case "ITFORM24QFILEFORMATDEFINITION":
                    return "ITForm24QFileFormatDefinition";
                case "INCREMENTSERVICEBOOK":
                    return "IncrementServiceBook";
                case "PERKTRANST":
                    return "PerkTransT";
                case "YEARLYPAYMENTT":
                    return "YearlyPaymentT";
                case "LVNEWREQ":
                    return "LvNewReq";
                case "ITINVESTMENTPAYMENT":
                    return "ITInvestmentPayment";
                case "PROMOTIONSERVICEBOOK":
                    return "PromotionServiceBook";
                case "TRANSFERSERVICEBOOK":
                    return "TransferServiceBook";
                case "OTHERSERVICEBOOK":
                    return "OtherServiceBook";
                case "ITSECTION10PAYMENT":
                    return "ITSection10Payment";               
                case "ADDRESS":
                    return "Address";
                case "LOCATION":
                    return "Location";

                case "EMPSALSTRUCT":
                    return "EmpSalStruct";

                case "INSURANCEDETAILST":
                    return "InsuranceDetailsT";

                case "ITSECTION24PAYMENTSELFOCCUPIED":
                    return "ITSection24Payment";
                case "ITSECTION24PAYMENTLAYOUT":
                    return "ITSection24Payment";
                case "EMPLOYEEPFTRUST":
                    return "PFTEmployeeLedger";
                case "SALATTENDANCET":
                    return "SalAttendanceT";
                default:
                    return null;
            }
        }
        public ActionResult DownloadFile(string fileName)
        {
            fileName = FileName(fileName);
            if (fileName != null)
            {
                fileName = Server.MapPath("~/App_Data/ExcelFormat/" + fileName + ".xlsx");
            }
            string localPath = new Uri(fileName).LocalPath;
            System.IO.FileInfo file = new System.IO.FileInfo(localPath);
            if (file.Exists)
            {
                return File(file.FullName, "text/plain", file.Name);
            }
            else
            {
                return HttpNotFound();
            }
        }
    }
}