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
using P2BUltimate.Security;
using Leave;


namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class RegimiPolicyController : Controller
    {
        //
        // GET: /RegimiPolicy/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/RegimiPolicy/Index.cshtml");
        }

      
        //create
        [HttpPost]

        public ActionResult Create(RegimiPolicy c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string FinancialYear = form["RegimiPolicyList"] == "0" ? "" : form["RegimiPolicyList"];


                    if (FinancialYear != null)
                    {
                        c.FinancialYear_Id = int.Parse(FinancialYear);
                    }
                    else
                    {
                        Msg.Add(" Kindly Select Financial Year. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (db.RegimiPolicy.Include(q => q.FinancialYear).Any(e => e.FinancialYear.Id == c.FinancialYear_Id ))
                    {
                        Msg.Add(" Data with this Calendar Year Already Exist. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            RegimiPolicy RegimiPolicyD = new RegimiPolicy()
                            {
                                FinancialYear_Id = c.FinancialYear_Id,
                                StartDate = c.StartDate,
                                SpanPeriodInDays = c.SpanPeriodInDays,
                               
                                DBTrack = c.DBTrack

                            };
                            db.RegimiPolicy.Add(RegimiPolicyD);
                            db.SaveChanges();

                           
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
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
                        var errorMsg = sb.ToString();
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                }

                catch (Exception ex)
                {
                    // List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        //LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

            }


            
           
        }





        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.RegimiPolicy.Include(e => e.FinancialYear)
                       .Where(e => e.Id == data).Select(e => new
                       {                           
                           FinancialYearfulldeatils = e.FinancialYear.FullDetails,
                           FinancialYearId = e.FinancialYear.Id,
                          
                           StartDate = e.StartDate,
                           SpanPeriodInDays = e.SpanPeriodInDays
                       }).ToList();

                var Corp = db.RegimiPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;

                return Json(new Object[] { Q, Auth, JsonRequestBehavior.AllowGet });

            }
        }



        [HttpPost]
        public async Task<ActionResult> EditSave(RegimiPolicy c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();
            string FINANCIALYEAR = form["RegimiPolicyList"] == "0" ? "" : form["RegimiPolicyList"];

            c.FinancialYear_Id = FINANCIALYEAR != null && FINANCIALYEAR != "" ? int.Parse(FINANCIALYEAR) : 0;
       



            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                       
                        RegimiPolicy Regimipolicy = db.RegimiPolicy.Find(data);
                        TempData["CurrRowVersion"] = Regimipolicy.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = Regimipolicy.DBTrack.CreatedBy == null ? null : Regimipolicy.DBTrack.CreatedBy,
                                CreatedOn = Regimipolicy.DBTrack.CreatedOn == null ? null : Regimipolicy.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            Regimipolicy.FinancialYear_Id = c.FinancialYear_Id;
                            Regimipolicy.StartDate = c.StartDate;
                            Regimipolicy.SpanPeriodInDays = c.SpanPeriodInDays;

                            Regimipolicy.DBTrack = c.DBTrack;
                            // SalHead.LvHead = LvH;
                            db.Entry(Regimipolicy).State = System.Data.Entity.EntityState.Modified;
                       
                            db.SaveChanges();
                        }
                        //}
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = c.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    RegimiPolicy regimipolicy = db.RegimiPolicy.Include(e => e.FinancialYear)
                                                                            
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    db.Entry(regimipolicy).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                    //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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



        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                int ParentId = 2;
                var jsonData = (Object)null;
                var LKVal = db.RegimiPolicy.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.RegimiPolicy
                         .Include(e => e.FinancialYear)

                         .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    LKVal = db.RegimiPolicy
                         .Include(e => e.FinancialYear)
                        
                         .Where(e => e.DBTrack.IsModified == false).AsNoTracking().ToList();
                }


                IEnumerable<RegimiPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.FinancialYear.FullDetails.ToUpper().Contains(gp.searchString.ToUpper()))
                            
                             || (e.StartDate.ToString().Contains(gp.searchString.ToUpper()))
                             || (e.SpanPeriodInDays.ToString().Contains(gp.searchString.ToUpper()))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.FinancialYear.FullDetails, a.StartDate.Value.ToShortDateString(), a.SpanPeriodInDays, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.FinancialYear != null ? a.FinancialYear.FullDetails : "", a.StartDate.Value.ToShortDateString(), a.SpanPeriodInDays, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<RegimiPolicy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "FinancialYear" ? c.FinancialYear != null ? c.FinancialYear.FullDetails : "" :
                                         gp.sidx == "StartDate" ? c.StartDate.Value.ToShortDateString() :
                                         gp.sidx == "SpanPeriodInDays" ? c.SpanPeriodInDays.ToString() :
                                         "");
                    }

                    //Func<BasicScale, string> orderfuc = (c =>
                    //                                           gp.sidx == "Id" ? c.Id.ToString() :
                    //                                           gp.sidx == "Code" ? c.Code :
                    //                                           gp.sidx == "ScaleName" ? c.ScaleName : "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] {a.FinancialYear!= null ? a.FinancialYear.FullDetails : "", a.StartDate.Value.ToShortDateString(), a.SpanPeriodInDays, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.FinancialYear != null ? a.FinancialYear.FullDetails : "", a.StartDate.Value.ToShortDateString(), a.SpanPeriodInDays, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.FinancialYear != null ? a.FinancialYear.FullDetails : "", a.StartDate.Value.ToShortDateString(), a.SpanPeriodInDays, a.Id }).ToList();
                    }
                    totalRecords = LKVal.Count();
                }
                if (totalRecords > 0)
                {
                    totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
                }
                if (gp.page > totalPages)
                {
                    gp.page = totalPages;
                }
                var JsonData = new
                {
                    page = gp.page,
                    rows = jsonData,
                    records = totalRecords,
                    total = totalPages,
                    p2bparam = ParentId
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public ActionResult GetLookupDetailsRegimi(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR").ToList();
                // IEnumerable<WeeklyOffCalendar> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Calendar.Include(e => e.Name)
                    .Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                //var result_1 = (from c in fall
                //                select new { c.Id, c.DepartmentCode, c.DepartmentName });
                return Json(r, JsonRequestBehavior.AllowGet);

            }
        }
	}
}