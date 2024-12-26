using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using P2BUltimate.App_Start;
using System.Net;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using System.Text;
using System.Threading.Tasks;
using P2BUltimate.Security;
using Payroll;
using Newtonsoft.Json;
using System.Configuration;
using System.Data.SqlClient;
namespace P2BUltimate.Controllers
{
    public class LoanAdvRepaymentTController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/LoanAdvRepaymentT/Index.cshtml");
        }
        public ActionResult ChkProcess(string typeofbtn, string month, string EmpCode)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool selected = false;
                var query1 = db.EmployeePayroll.Include(e => e.LoanAdvRequest).ToList();
                foreach (var b in query1)
                {
                    foreach (var c in b.LoanAdvRequest)
                    {
                        if (c.Id.ToString() == EmpCode)
                        {
                            var queryT = db.EmployeePayroll.Include(e => e.SalaryT).Where(e => e.Id == b.Id).SingleOrDefault();
                            if (queryT != null)
                            {
                                var query = queryT.SalaryT.Where(e => e.PayMonth == month).ToList();
                                if (query.Count > 0)
                                {
                                    selected = true;
                                }
                                var data = new
                                {
                                    status = selected,
                                };
                                return Json(data, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GridEditSave(LoanAdvRepaymentT ypay, FormCollection from, string data, string Conf, int Parentdata, int EmpdataId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    var db_data = db.LoanAdvRepaymentT.Where(e => e.Id == id).SingleOrDefault();
                    string PayMonth = "";
                    int LoanReqId = 0;
                    //var OEmployeePayroll = db.EmployeePayroll.Include(e => e.LoanAdvRequest)
                    //                    .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT)).Where(q => q.Id == EmpdataId).AsNoTracking().AsParallel().Single();

                    //var LoanAdvRepaymentTdata = OEmployeePayroll.LoanAdvRequest.Where(q => q.Id == Parentdata).Select(q => q.LoanAdvRepaymentT).SingleOrDefault();

                    var LoanAdvRequest = db.LoanAdvRequest.Where(e => e.Id == Parentdata && e.EmployeePayroll_Id == EmpdataId).Select(r => new
                    {
                        iLoanAdvRepaymentT = r.LoanAdvRepaymentT.Select(y => new {
                            iInstallementDate=y.InstallementDate, 
                            iId=y.Id                
                        }).ToList()
                    }).SingleOrDefault();

                    var LoanAdvRepaymentTdata = LoanAdvRequest.iLoanAdvRepaymentT.ToList();
                    //foreach (var b in OEmployeePayroll.LoanAdvRequest)
                    //{
                    foreach (var c in LoanAdvRepaymentTdata)
                    {
                        if (c.iId == id)
                        {
                            LoanReqId = Parentdata;
                            PayMonth = c.iInstallementDate.Value.ToString("MM/yyyy");
                            //var SalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Select(e => e.SalaryT.Where(r => r.PayMonth == PayMonth && r.ReleaseDate != null).FirstOrDefault()).SingleOrDefault();
                            var SalT = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate != null).ToList();
                            if (SalT.Count()>0)
                            {
                                return Json(new { status = false, data = db_data, responseText = "Salary is released for this month. You can't change amount now..!" }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        }
                        //}
                    }

                    

                    if (Conf == "1")
                    {
                        var LoanAdvReq = db.LoanAdvRequest.Where(e => e.Id == Parentdata && e.EmployeePayroll_Id== EmpdataId).SingleOrDefault();
                        LoanAdvReq.MonthlyPricipalAmount = ypay.MonthlyPricipalAmount;
                        LoanAdvReq.MonthlyInterest = ypay.MonthlyInterest;
                        db.LoanAdvRequest.Attach(LoanAdvReq);
                        db.Entry(LoanAdvReq).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        List<LoanAdvRepaymentT> OLoanRepay = db.LoanAdvRequest.Where(e => e.Id == Parentdata).SelectMany(e => e.LoanAdvRepaymentT).ToList();

                        foreach (var L in OLoanRepay.Where(e => e.InstallementDate >= db_data.InstallementDate))
                        {
                            var CurLoanRepay = db.LoanAdvRepaymentT.Where(e => e.Id == L.Id).SingleOrDefault();
                            CurLoanRepay.InstallmentPaid = ypay.InstallmentPaid;
                            CurLoanRepay.InstallmentAmount = ypay.InstallmentAmount;
                            CurLoanRepay.MonthlyPricipalAmount = ypay.MonthlyPricipalAmount;
                            CurLoanRepay.MonthlyInterest = ypay.MonthlyInterest;
                            try
                            {
                                db.LoanAdvRepaymentT.Attach(CurLoanRepay);
                                db.Entry(CurLoanRepay).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(CurLoanRepay).State = System.Data.Entity.EntityState.Detached;

                            }
                            catch (Exception e)
                            {

                                return Json(new { status = false, responseText = e.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {


                        var LoanAdvReq = db.LoanAdvRequest.Where(e => e.Id == Parentdata && e.EmployeePayroll_Id == EmpdataId).SingleOrDefault();
                        LoanAdvReq.MonthlyPricipalAmount = ypay.MonthlyPricipalAmount;
                        LoanAdvReq.MonthlyInterest = ypay.MonthlyInterest;
                        db.LoanAdvRequest.Attach(LoanAdvReq);
                        db.Entry(LoanAdvReq).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        db_data.MonthlyPricipalAmount = ypay.MonthlyPricipalAmount;
                        db_data.MonthlyInterest = ypay.MonthlyInterest;
                        db_data.InstallmentPaid = ypay.InstallmentPaid;
                        db_data.InstallmentAmount = ypay.InstallmentAmount;
                        try
                        {
                            db.LoanAdvRepaymentT.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                            return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception e)
                        {

                            return Json(new { status = false, responseText = e.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { status = false, responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_LoanRequestGridPartial.cshtml");
        }
        public class returnDataClass
        {
            public double MonthlyPricipalAmount { get; set; }
            public double MonthlyInstallmentAmount { get; set; }
        }
        public ActionResult GridEditData(int data)
        {
            var returnlist = new List<returnDataClass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != 0)
                {

                    var retrundata = db.LoanAdvRepaymentT.Where(e => e.Id == data).Select(e => new
                    {
                        InstallmentPaid = e.InstallmentPaid,
                        InstallmentAmount = e.InstallmentAmount,
                        MonthlyPrincipaleAmount=e.MonthlyPricipalAmount!=0 && e.MonthlyPricipalAmount!=null? e.MonthlyPricipalAmount:0,
                        MonthlyInterestAmount=e.MonthlyInterest!=0 && e.MonthlyInterest!=null?e.MonthlyInterest:0
                    }).SingleOrDefault();

                    return Json(new { data = retrundata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }
        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var LvEP = db.LoanAdvRepaymentT.Find(data);
                db.LoanAdvRepaymentT.Remove(LvEP);
                db.SaveChanges();
                //return Json(new Object[] { "", "", " Record Deleted Successfully " }, JsonRequestBehavior.AllowGet);
                List<string> Msgr = new List<string>();
                Msgr.Add("Record Deleted Successfully  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);
            }
        }
        public class LoanRepayChildDataClass
        {
            public int Id { get; set; }
            public string InstallementDate { get; set; }
            public string InstallmentAmount { get; set; }
            public string InstallmentCount { get; set; }
            public string InstallmentPaid { get; set; }
            public string PayMonth { get; set; }
            public string RepaymentDate { get; set; }
            public string TotalLoanBalance { get; set; }
            public string TotalLoanPaid { get; set; }

        }
        public class returndatagridclass //Parentgrid
        {
            public int Id { get; set; }
            public int LId { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Location { get; set; }
            public string LoanAdvanceHead { get; set; }
            public double LoanAppliedAmount { get; set; }
            public string EnforcementDate { get; set; }
            public double LastInstallmentPaid { get; set; }
        }
        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<returndatagridclass> model = new List<returndatagridclass>();
                    returndatagridclass view = null;

                    var all = db.EmployeePayroll.Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        // .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj)
                        .Include(e => e.LoanAdvRequest)
                         .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))
                        .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                        .Include(e => e.Employee.ServiceBookDates).AsNoTracking().AsParallel()
                        // .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                        .ToList();

                    IEnumerable<EmployeePayroll> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                   || (e.Employee.EmpCode.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                   || (e.Employee.EmpName.FullNameFML.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                   || (e.LoanAdvRequest.Select(q => q.LoanAdvanceHead.Name.ToString().ToUpper()).Contains(param.sSearch.ToUpper()))
                            //|| (e.LoanAdvRequest.Select(t => t.LoanAdvRepaymentT.Select(q => q.InstallmentAmount.ToString().Contains(param.sSearch)))
                                   ).ToList();
                    }
                    foreach (var z in fall)
                    {
                        foreach (var a in z.LoanAdvRequest)                     //old format-  foreach (var a in z.LoanAdvRequest)
                        {
                            if (a.LoanAdvRepaymentT.Count() > 0)
                            {

                                var check = a.LoanAdvRepaymentT.Any(e => e.RepaymentDate != null);

                                LoanAdvRepaymentT AmtList = null;

                                //foreach (var item in check)
                                //{
                                if (check == true)
                                {
                                    AmtList = a.LoanAdvRepaymentT.Where(e => e.RepaymentDate != null).OrderBy(e => e.InstallementDate).Last();

                                }
                                else
                                {
                                    AmtList = a.LoanAdvRepaymentT.OrderBy(e => e.InstallementDate).FirstOrDefault();
                                }
                                //}



                                view = new returndatagridclass()
                                {
                                    Id = z.Id,
                                    LId = a.Id,
                                    Code = z.Employee.EmpCode,
                                    Name = z.Employee.EmpName != null ? z.Employee.EmpName.FullNameFML : null,
                                    //Location = z.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                    LoanAdvanceHead = a.LoanAdvanceHead != null ? a.LoanAdvanceHead.Name : null,
                                    EnforcementDate = a.EnforcementDate != null ? a.EnforcementDate.Value.ToShortDateString() : null,
                                    LoanAppliedAmount = a.LoanAppliedAmount,
                                    LastInstallmentPaid = AmtList.InstallmentPaid != 0 ? AmtList.InstallmentPaid : 0
                                };

                                model.Add(view);
                            }
                        }
                    }



                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<returndatagridclass, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Name : "");
                    var sortcolumn = Request["sSortDir_0"];
                    //if (sortcolumn == "asc")
                    //{
                    //    model = model.OrderBy(orderfunc);
                    //}
                    //else
                    //{
                    //    model = model.OrderByDescending(orderfunc);
                    //}
                    // Paging 
                    var dcompanies = model
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        result = model;
                        //var jsona = JsonConvert.SerializeObject(result, Formatting.Indented);

                        //return Json(new
                        //{
                        //    sEcho = param.sEcho,
                        //    iTotalRecords = model.Count(),
                        //    iTotalDisplayRecords = fall.Count(),
                        //    data = result,
                        //}, JsonRequestBehavior.AllowGet);

                        var jsonResult = Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = model.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result,
                        }, JsonRequestBehavior.AllowGet);
                        jsonResult.MaxJsonLength = int.MaxValue;
                        return jsonResult;
                    }
                    else
                    {
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.Name };

                       // var jsona = JsonConvert.SerializeObject(result, Formatting.Indented);

                        //var jsonResult = Json(Item, JsonRequestBehavior.AllowGet);
                        //jsonResult.MaxJsonLength = int.MaxValue;
                        //return jsonResult;

                        var jsonResult = Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = model.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result,
                        }, JsonRequestBehavior.AllowGet);
                        jsonResult.MaxJsonLength = int.MaxValue;
                        return jsonResult;
                    }//for data reterivation

                    //var jsona = JsonConvert.SerializeObject(excList, Formatting.Indented);
                    //if (jsona != null)
                    //{
                    //    return Json(new { json = jsona, col = ListCol1 }, JsonRequestBehavior.AllowGet);
                    //}
                }
                catch (Exception ex)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult EditGridDetails(int data, string filter)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var db_data = db.LoanAdvRequest.Include(e => e.LoanAdvRepaymentT).Where(e => e.Id == data).SingleOrDefault();

                if (db_data.LoanAdvRepaymentT.Count > 0)
                {
                    List<LoanRepayChildDataClass> returndata = new List<LoanRepayChildDataClass>();
                    List<LoanAdvRepaymentT> LoanAdvRepaymentTData = new List<LoanAdvRepaymentT>();

                    if (filter != "")
                    {
                        LoanAdvRepaymentTData = db_data.LoanAdvRepaymentT.Where(e => e.PayMonth == filter).OrderByDescending(e => e.Id).ToList();
                    }
                    else
                    {
                        LoanAdvRepaymentTData = db_data.LoanAdvRepaymentT.OrderByDescending(e => e.Id).ToList();
                    }
                    foreach (var item in LoanAdvRepaymentTData)
                    {

                        returndata.Add(new LoanRepayChildDataClass
                        {
                            Id = item.Id,
                            InstallementDate = item.InstallementDate != null ? item.InstallementDate.Value.ToShortDateString() : null,
                            InstallmentAmount = item.InstallmentAmount.ToString(),
                            InstallmentCount = item.InstallmentCount.ToString(),
                            InstallmentPaid = item.InstallmentPaid.ToString(),
                            PayMonth = item.PayMonth,
                            RepaymentDate = item.RepaymentDate != null ? item.InstallementDate.Value.ToShortDateString() : null,
                            TotalLoanBalance = item.TotalLoanBalance.ToString(),
                            TotalLoanPaid = item.TotalLoanPaid.ToString(),

                        });
                    }
                    return Json(returndata, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }
            }
        }
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LoanAdvRepaymentT LoanAdvRepay = db.LoanAdvRepaymentT.Where(e => e.Id == data).SingleOrDefault();

                if (LoanAdvRepay.RepaymentDate != null)
                {
                    return this.Json(new { status = true, responseText = "You cannot delete as salary is generated.", JsonRequestBehavior.AllowGet });
                }



                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        db.LoanAdvRepaymentT.Remove(LoanAdvRepay);
                        await db.SaveChangesAsync();


                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data removed.", JsonRequestBehavior.AllowGet });

                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        //Log the error (uncomment dex variable name and add a line here to write a log.)
                        //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                        //return RedirectToAction("Delete");
                        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                    }
                    catch (Exception ex)
                    {
                        LogFile Logfile = new LogFile();
                        ErrorLog Err = new ErrorLog()
                        {
                            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                            ExceptionMessage = ex.Message,
                            ExceptionStackTrace = ex.StackTrace,
                            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                            LogTime = DateTime.Now
                        };
                        Logfile.CreateLogFile(Err);
                        List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }


        //public ActionResult AddCarryForwad(FormCollection Form)
        //{
        //    List<string> Msg = new List<string>();
        //    var LoanHeads = Form["LoanHead-Selectmenu"] == null ? null : Form["LoanHead-Selectmenu"];
        //    var paymonth = Form["paymonth"] == null ? null : Form["paymonth"];

        //        try
        //        {
        //            using (DataBaseContext db = new DataBaseContext())
        //            {
        //                var LoanHeads_ids = LoanHeads != null ? Utility.StringIdsToListIds(LoanHeads) :
        //                    db.LoanAdvanceHead.Select(e => e.Id).ToList();
        //                DateTime PrevSal = Convert.ToDateTime("01/" + paymonth);
        //                var PrevSal_string = Utility._Convert_DateTime_To_PayMonth(new List<DateTime>() { PrevSal }).LastOrDefault();


        //                string Month = PrevSal.AddMonths(1).Month.ToString().Length == 1 ? "0" + PrevSal.AddMonths(1).Month.ToString() : PrevSal.AddMonths(1).Month.ToString();
        //                string CurMonth = Month + "/" + PrevSal.AddMonths(1).Year.ToString();
        //                DateTime CurMonthDateTime = Convert.ToDateTime("01/" + CurMonth);
        //                var EmpList = db.EmployeePayroll.Where(a => a.EmpSalStruct.Any(s => s.EffectiveDate == CurMonthDateTime)).Select(e => e.Id).ToList();
        //                foreach (var id in EmpList)
        //                {
        //                    var EmployeePayroll = db.EmployeePayroll.Include(a => a.LoanAdvRequest)
        //                        .Include(a => a.LoanAdvRequest.Select(e => e.LoanAdvRepaymentT))
        //                        .Include(a => a.LoanAdvRequest.Select(e => e.LoanAdvanceHead))
        //                        .Where(a => a.Id == id).FirstOrDefault();

        //                    foreach (var LoanHeads_id in LoanHeads_ids)
        //                    {
        //                        var LoanAdvRequests = EmployeePayroll.LoanAdvRequest
        //                            .Where(e => e.IsActive == true && e.LoanAdvanceHead.Id == LoanHeads_id)
        //                            .ToList();
        //                        foreach (var LoanAdvRequest in LoanAdvRequests)
        //                        {
        //                            var LastLoanAdvRepaymentTs = LoanAdvRequest.LoanAdvRepaymentT.OrderByDescending(e => e.InstallementDate).FirstOrDefault();
        //                            if (LastLoanAdvRepaymentTs.InstallementDate < CurMonthDateTime)
        //                            {

        //                                LoanAdvRepaymentT oLoanAdvRepaymentT = new LoanAdvRepaymentT
        //                                {
        //                                    InstallementDate = LastLoanAdvRepaymentTs.InstallementDate.Value.AddMonths(1),
        //                                    InstallmentAmount = LastLoanAdvRepaymentTs.InstallmentAmount,
        //                                    InstallmentCount = LastLoanAdvRepaymentTs.InstallmentCount,
        //                                    MonthlyInterest = LastLoanAdvRepaymentTs.MonthlyInterest,
        //                                    MonthlyPricipalAmount = LastLoanAdvRepaymentTs.MonthlyPricipalAmount,
        //                                    PayMonth = CurMonth,
        //                                    TotalLoanBalance = LastLoanAdvRepaymentTs.TotalLoanBalance,
        //                                    TotalLoanPaid = LastLoanAdvRepaymentTs.TotalLoanPaid,
        //                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
        //                                };

        //                                LoanAdvRequest.LoanAdvRepaymentT.Add(oLoanAdvRepaymentT);
        //                                db.Entry(LoanAdvRequest).State = System.Data.Entity.EntityState.Modified;
        //                                db.SaveChanges();
        //                            }
        //                        }
        //                    }
        //                }
        //                Msg.Add("  Data Created successfully  ");
        //                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = ex.Message }, JsonRequestBehavior.AllowGet);
        //        }

        //}
        public ActionResult Carryforward_PARTIAL()
        {
            return View("~/Views/Shared/Payroll/_CarryforwardLoanAdvRepayment.cshtml");
        }
        public ActionResult GetLoanHead()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var salryheadlist = db.LoanAdvanceHead.ToList();
                var a = new SelectList(salryheadlist, "Id", "Name");
                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ChkProcesscarry(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                if (month != null)
                {
                    bool selected = false;
                    var query = db.SalaryT.Where(e => e.PayMonth == month).ToList();

                    if (query == null || query.Count() == 0)
                    {
                        selected = true;
                    }
                    else
                    {
                        selected = true;
                    }
                    var data = new
                    {
                        status = selected,
                    };
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }
        public ActionResult AddCarryForwad(string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime CurDate = Convert.ToDateTime("01/" + month);
                string curmon = CurDate.ToString("MM/yyyy");
                DateTime prevmn = Convert.ToDateTime("01/" + month).AddDays(-1).Date;
                string prevmnon = prevmn.ToString("MM/yyyy");

                var DataChkcur = db.SalaryT.Where(e => e.PayMonth == curmon).FirstOrDefault();
                if ( DataChkcur!=  null)
                {
                    // Msg.Add(" Income tax Not available for month =" + prevmn.ToString("MM/yyyy"));
                    return Json(new Utility.JsonReturnClass { success = true, responseText = "Please delete salary for month :" + curmon + ". and Try Again.." }, JsonRequestBehavior.AllowGet);

                }
                var DataChkprv = db.SalaryT.Where(e => e.PayMonth == prevmnon).ToList();
                if (DataChkprv.Count() == 0)
                {
                    // Msg.Add(" Income tax Not available for month =" + prevmn.ToString("MM/yyyy"));
                    return Json(new Utility.JsonReturnClass { success = true, responseText = "Please First Process salary for month :" + prevmnon + ". Then Go to Next Month" }, JsonRequestBehavior.AllowGet);

                }


                var PrevSal = db.SalaryT.OrderByDescending(e => e.Id).FirstOrDefault();
                DateTime PrevDate = Convert.ToDateTime("01/" + PrevSal.PayMonth);
                string Month = PrevDate.AddMonths(1).Month.ToString().Length == 1 ? "0" + PrevDate.AddMonths(1).Month.ToString() : PrevDate.AddMonths(1).Month.ToString();
                string CurMonth = Month + "/" + PrevDate.AddMonths(1).Year.ToString();

                var DataChk1 = db.CPIEntryT.Where(e => e.PayMonth == CurMonth).ToList();
                if (DataChk1.Count == 0)
                {
                    return this.Json(new { success = true, responseText = "CPI Entry has not been done for =" + CurMonth, JsonRequestBehavior.AllowGet });
                }


               
                    int Compid = int.Parse(Convert.ToString(Session["CompId"]));
                    string connString = ConfigurationManager.ConnectionStrings["DataBaseContext"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(connString))
                    {
                        using (SqlCommand cmd = new SqlCommand("Loan_carryforward", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add("@Nmonyear", SqlDbType.VarChar).Value = CurMonth;
                            cmd.Parameters.Add("@monyear", SqlDbType.VarChar).Value = prevmnon;

                            con.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
               

                // old code take time 19072023
                //var EmpList = db.LoanAdvRequest.Include(e => e.LoanAdvRepaymentT).Where(e => e.IsActive == true && e.CloserDate == null).OrderBy(e => e.Id).ToList();
                //foreach (var a in EmpList)
                //{
                //    LoanAdvRepaymentT PrevRepay = a.LoanAdvRepaymentT.Where(e => e.PayMonth == PrevSal.PayMonth).FirstOrDefault();
                //    int id = a.LoanAdvRepaymentT.Where(e => e.PayMonth == CurMonth).FirstOrDefault() != null ? a.LoanAdvRepaymentT.Where(e => e.PayMonth == CurMonth).FirstOrDefault().Id : 0;
                //    if (id != 0 && PrevRepay != null)
                //    {
                //        var db_data = db.LoanAdvRepaymentT.Where(e => e.Id == id).FirstOrDefault();

                //        db_data.InstallmentAmount = PrevRepay.InstallmentAmount;
                //        db_data.InstallmentPaid = PrevRepay.InstallmentPaid;
                //        db_data.InstallmentAmount = PrevRepay.InstallmentAmount;
                //        db_data.MonthlyInterest = PrevRepay.MonthlyInterest;
                //        db_data.MonthlyPricipalAmount = PrevRepay.MonthlyPricipalAmount;
                //        try
                //        {
                //            db.LoanAdvRepaymentT.Attach(db_data);
                //            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                //            db.SaveChanges();
                //            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                //            //return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                //        }
                //        catch (Exception e)
                //        {

                //            return Json(new { success = false, responseText = e.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
                //        }
                //    }
                //    else if (PrevRepay != null && id == 0)
                //    {
                //        try
                //        {
                //        DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                //        LoanAdvRepaymentT LoanAdvRepayT = new LoanAdvRepaymentT()
                //        {
                //            DBTrack = DBTrack,
                //            InstallementDate = Convert.ToDateTime("01/" + CurMonth),
                //            InstallmentAmount = PrevRepay.InstallmentAmount,
                //            InstallmentCount = PrevRepay.InstallmentCount,
                //            InstallmentPaid = PrevRepay.InstallmentPaid,
                //            MonthlyInterest = PrevRepay.MonthlyInterest,
                //            MonthlyPricipalAmount = PrevRepay.MonthlyPricipalAmount,
                //            RepaymentDate = null,
                //            TotalLoanBalance = PrevRepay.TotalLoanBalance,
                //            TotalLoanPaid = PrevRepay.TotalLoanPaid,
                //            PayMonth = CurMonth
                //        };
                //        db.LoanAdvRepaymentT.Add(LoanAdvRepayT);
                //        db.SaveChanges();

                //        LoanAdvRequest OLoanAdvReq = db.LoanAdvRequest.Include(e => e.LoanAdvRepaymentT).Where(e => e.Id == a.Id).SingleOrDefault();
                //        List<LoanAdvRepaymentT> OLoanAdvRepayT = new List<LoanAdvRepaymentT>();
                //        if (OLoanAdvReq.LoanAdvRepaymentT != null)
                //        {
                //            OLoanAdvRepayT.AddRange(OLoanAdvReq.LoanAdvRepaymentT);
                //        }
                //        OLoanAdvRepayT.Add(LoanAdvRepayT);

                //        OLoanAdvReq.LoanAdvRepaymentT = OLoanAdvRepayT;
                //        db.LoanAdvRequest.Attach(OLoanAdvReq);
                //        db.Entry(OLoanAdvReq).State = System.Data.Entity.EntityState.Modified;
                //        db.SaveChanges();
                //        }
                //        catch (Exception e)
                //        {

                //            return Json(new { success = false, responseText = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
                //        }
                //    }
                //}

                // old code take time 19072023

                return this.Json(new { success = true, responseText = "Data carry forwarded for month :" + CurMonth, JsonRequestBehavior.AllowGet });
            }
        }
    }
}