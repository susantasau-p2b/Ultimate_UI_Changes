using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using Payroll;
using P2BUltimate.Process;
using System.Diagnostics;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class GratuityTController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);
            return View("~/Views/Payroll/MainViews/GratuityT/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Payroll/_GratuityT.cshtml");

        }

        public class GratuityTChildDataClass
        {
            public string Id { get; set; }
            public string ActualService { get; set; }
            public string Amount { get; set; }
            public string TotalLWP { get; set; }

        }
        public ActionResult Get_GratuityTDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll.Include(e => e.GratuityT)
                        .Where(e => e.Id == data).ToList();
                    if (db_data.Count > 0)
                    {
                        List<GratuityTChildDataClass> returndata = new List<GratuityTChildDataClass>();
                        foreach (var item in db_data.SelectMany(e => e.GratuityT))
                        {
                            returndata.Add(new GratuityTChildDataClass
                            {
                                Id = item.Id.ToString(),
                                ActualService = item.ActualService.ToString(),
                                Amount = item.Amount.ToString(),
                                TotalLWP = item.TotalLWP.ToString(),

                            });
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public JsonResult EditGridDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.GratuityT

                     .Where(e => e.Id == data).Select
                     (e => new
                     {
                         ActualService = e.ActualService,
                         Amount = e.Amount,
                         ProcessDate = e.ProcessDate,
                         RoundedService = e.RoundedService,
                         TotalLWP = e.TotalLWP,

                         Action = e.DBTrack.Action
                     }).ToList();
                var Gratuity = db.GratuityT.Find(data);
                Session["RowVersion"] = Gratuity.RowVersion;
                var Auth = Gratuity.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GridEditSave(GratuityT GT, FormCollection form, string data)
        {
            var Amount = form["GratuityT-Amount"] == " 0" ? "" : form["GratuityT-Amount"];
            using (DataBaseContext db = new DataBaseContext())
            {
                GT.Amount = Convert.ToDouble(Amount);
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    var db_data = db.GratuityT.Where(e => e.Id == id).SingleOrDefault();
                    db_data.Amount = GT.Amount;
                    try
                    {
                        db.GratuityT.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        return Json(new { data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {

                        throw e;
                    }
                }
                else
                {
                    List<string> Msg = new List<string>();
                    Msg.Add("  Data Is Null  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //return Json(new { responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        public ActionResult Create(FormCollection form, String forwarddata) //Create submit
        {
            string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<string> empMsg = new List<string>();
                    DateTime ProcDate = Convert.ToDateTime(form["ProcessDate"] == "0" ? "" : form["ProcessDate"]);

                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = Utility.StringIdsToListIds(Emp);
                    }
                    else
                    {
                        // return Json(new Object[] { "", "", "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                        List<string> Msgu = new List<string>();
                        Msgu.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                    }


                    Employee OEmployee = null;

                    EmployeePayroll OEmployeePayroll = null;
                    CompanyPayroll OCompanyPayroll = null;


                    if (ModelState.IsValid)
                    {
                        if (ids != null)
                        {
                            foreach (var i in ids)
                            {
                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                            .Where(r => r.Id == i).SingleOrDefault();

                                OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == i).SingleOrDefault();

                                OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();

                                using (TransactionScope ts = new TransactionScope())
                                {

                                    Process.PayrollReportGen.GratuityCalc(OCompanyPayroll.Id, OEmployeePayroll.Id, ProcDate);
                                    //db.RefreshAllEntites(RefreshMode.StoreWins);
                                    ts.Complete();

                                   

                                }

                                var emp_msg = System.Web.HttpContext.Current.Session["empcodeMsg"];
                                if (emp_msg != null)
                                {
                                    empMsg.Add(" The " +  OEmployee.EmpCode  + "employee has service is below 5 years");
                                    System.Web.HttpContext.Current.Session["empcodeMsg"] = null;
                                }
                            }
                            //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);  
                            if (empMsg.Count() != 0)
                            {
                                return Json(new Utility.JsonReturnClass { success = false, responseText = empMsg }, JsonRequestBehavior.AllowGet);
                            }
                           
                            List<string> Msgs = new List<string>();
                            Msgs.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                        }
                        List<string> Msga = new List<string>();
                        Msga.Add("  Data Saved successfully  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msga }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {

                        StringBuilder sb = new StringBuilder("");
                        foreach (ModelState modelState in ModelState.Values)
                        {
                            foreach (ModelError error in modelState.Errors)
                            {
                                sb.Append(error.ErrorMessage);
                                sb.Append("." + "\n");
                            }
                        }
                        //var errorMsg = sb.ToString();
                        //return Json(new Object[] { "", "", errorMsg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { msg = errorMsg });
                        List<string> MsgB = new List<string>();
                        var errorMsg = sb.ToString();
                        MsgB.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);

                    }
                }
                catch (Exception ex)
                {
                    List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}