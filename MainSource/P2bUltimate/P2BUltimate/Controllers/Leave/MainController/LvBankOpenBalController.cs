using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Payroll;
using Training;
using Attendance;
using Leave;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Leave.MainController
{
    [AuthoriseManger]
    public class LvBankOpenBalController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Leave/MainViews/LvBankOpenBal/Index.cshtml");
        }
        [HttpPost]
        public ActionResult Create(LvBankOpenBal L, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var LvCalendarlist = form["LvCalendarlist"] == "0" ? "" : form["LvCalendarlist"];

                    var singlerec = db.LvBankOpenBal.SingleOrDefault();
                    if (singlerec!=null)
                    {
                          Msg.Add("You can Not enter Second time Balance ");
                         return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    if (LvCalendarlist != null && LvCalendarlist != "")
                    {
                        var value = db.Calendar.Find(int.Parse(LvCalendarlist));
                        L.LvCalendar = value;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.LvEncashPolicy.Any(o => o.PolicyName == L.))
                            //{
                            //    return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            //}

                            L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            LvBankOpenBal Lvopenbalbank = new LvBankOpenBal()
                            {
                                CreditDate = L.CreditDate,
                                CreditDays = L.CreditDays,
                                LvCalendar = L.LvCalendar,
                                OpeningBalance = L.OpeningBalance,
                                UtilizedDays = L.UtilizedDays,
                                DBTrack = L.DBTrack
                                //  DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.LvBankOpenBal.Add(Lvopenbalbank);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, L.DBTrack);
                                DT_LvBankOpenBal DT_Corp = (DT_LvBankOpenBal)rtn_Obj;
                                db.Create(DT_Corp);
                                db.SaveChanges();


                                LvBankLedger _OLvCreditRecordLvBankLedger = new LvBankLedger()
                                {
                                    OpeningBalance = Lvopenbalbank.OpeningBalance,
                                    CreditDays = 0,
                                    ClosingBalance = Lvopenbalbank.ClosingBalance,
                                    CreditDate = L.CreditDate,
                                    DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                    Narration="Opening balance"
                                    

                                };

                                db.LvBankLedger.Add(_OLvCreditRecordLvBankLedger);
                                db.SaveChanges();

                                int CompId = 0;
                                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                                {
                                    CompId = int.Parse(Session["CompId"].ToString());
                                }
                                Company OCompany = null;
                                OCompany = db.Company.Find(CompId);
                                var companyLeave = new CompanyLeave();
                                companyLeave = db.CompanyLeave.Where(e => e.Company.Id == OCompany.Id).SingleOrDefault();
                                if (companyLeave != null)
                                {
                                    var LvBankLedger_list = new List<LvBankLedger>();
                                    LvBankLedger_list.Add(_OLvCreditRecordLvBankLedger);
                                    companyLeave.LvBankLedger = LvBankLedger_list;
                                    db.CompanyLeave.Attach(companyLeave);
                                    db.Entry(companyLeave).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companyLeave).State = System.Data.Entity.EntityState.Detached;
                                }

                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = L.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                            }
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
                        // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { msg = errorMsg });
                    }
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

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LvBankOpenBal
                    .Include(e => e.LvCalendar)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        CreditDate = e.CreditDate,
                        CreditDays = e.CreditDays,
                        OpeningBalance = e.OpeningBalance,
                        UtilizedDays = e.UtilizedDays,
                        Calendar_Id = e.LvCalendar.Id == null ? 0 : e.LvCalendar.Id,
                        Calendar_fullDetails = e.LvCalendar.FullDetails == null ? "" : e.LvCalendar.FullDetails,
                        Action = e.DBTrack.Action
                    }).ToList();




                var Corp = db.LvBankOpenBal.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
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
                var jsonData = (Object)null;
                IEnumerable<LvBankOpenBal> lencash = null;
                if (gp.IsAutho == true)
                {
                    lencash = db.LvBankOpenBal.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    lencash = db.LvBankOpenBal.AsNoTracking().ToList();
                }

                IEnumerable<LvBankOpenBal> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = lencash;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.OpeningBalance.ToString().Contains(gp.searchString))
                               || (e.CreditDays.ToString().Contains(gp.searchString))
                               || (e.UtilizedDays.ToString().Contains(gp.searchString))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.OpeningBalance, a.CreditDays, a.UtilizedDays, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.OpeningBalance, a.CreditDays, a.UtilizedDays, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = lencash;
                    Func<LvBankOpenBal, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>    gp.sidx == "OpeningBalance" ? c.OpeningBalance.ToString() :
                                            gp.sidx == "CreditDays" ? c.CreditDays.ToString() :
                                            gp.sidx == "UtilizedDays" ? c.UtilizedDays.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.OpeningBalance), Convert.ToString(a.CreditDays), Convert.ToString(a.UtilizedDays), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.OpeningBalance), Convert.ToString(a.CreditDays), Convert.ToString(a.UtilizedDays), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.OpeningBalance), Convert.ToString(a.CreditDays), Convert.ToString(a.UtilizedDays), a.Id }).ToList();
                    }
                    totalRecords = lencash.Count();
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
                    total = totalPages
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ActionResult> EditSave(LvBankOpenBal L, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var LvCalendarlist = form["LvCalendarlist"] == "0" ? "" : form["LvCalendarlist"];


                    var singlerec = db.LvBankLedger.ToList();
                    if (singlerec.Count()>1 )
                    {
                        Msg.Add("You can Not Edit Record Because ledger is Genrated ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    if (LvCalendarlist != null && LvCalendarlist != "")
                    {
                        var value = db.Calendar.Find(int.Parse(LvCalendarlist));
                        L.LvCalendar = value;
                    }


                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    LvBankOpenBal blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.LvBankOpenBal.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    L.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    if (LvCalendarlist != null)
                                    {
                                        if (LvCalendarlist != "")
                                        {
                                            var val = db.Calendar.Find(int.Parse(LvCalendarlist));
                                            L.LvCalendar = val;

                                            var type = db.LvBankOpenBal.Include(e => e.LvCalendar).Where(e => e.Id == data).SingleOrDefault();
                                            IList<LvBankOpenBal> typedetails = null;
                                            if (type.LvCalendar != null)
                                            {
                                                typedetails = db.LvBankOpenBal.Where(x => x.LvCalendar.Id == type.LvCalendar.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.LvBankOpenBal.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.LvCalendar = L.LvCalendar;
                                                db.LvBankOpenBal.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var BusiTypeDetails = db.LvBankOpenBal.Include(e => e.LvCalendar).Where(x => x.Id == data).ToList();
                                            foreach (var s in BusiTypeDetails)
                                            {
                                                s.LvCalendar = null;
                                                db.LvBankOpenBal.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var BusiTypeDetails = db.LvBankOpenBal.Include(e => e.LvCalendar).Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.LvCalendar = null;
                                            db.LvBankOpenBal.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                  //  int a = EditS(data, L, L.DBTrack);

                                    // Surendra start 01/03/2019
                                    var CurCorp = db.LvBankOpenBal.Include(e => e.LvCalendar).Where(e=>e.Id==data).SingleOrDefault();
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                       // L.DBTrack = dbT;
                                        //LvBankOpenBal corp = new LvBankOpenBal()
                                        //{
                                            CurCorp.CreditDate = L.CreditDate;
                                            CurCorp.CreditDays = L.CreditDays;
                                            CurCorp.LvCalendar = db.Calendar.Find(int.Parse(LvCalendarlist));
                                            CurCorp.OpeningBalance = L.OpeningBalance;
                                            CurCorp.UtilizedDays = L.UtilizedDays;
                                            CurCorp.Id = data;
                                            CurCorp.DBTrack = L.DBTrack;
                                        //};


                                            db.LvBankOpenBal.Attach(CurCorp);
                                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Modified;
                                            db.Entry(CurCorp).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                       


                                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                       // return 1;
                                    }

                                    // Surendra End 01/03/2019
                                    //leave bank ledger start
                                    var lvopenbal = db.LvBankOpenBal.SingleOrDefault();
                                    int ledgerid = db.LvBankLedger.SingleOrDefault().Id;

                                    var CurCorpledger = db.LvBankLedger.Find(ledgerid);
                                    //TempData["CurrRowVersion"] = CurCorpledger.RowVersion;
                                    //db.Entry(CurCorpledger).State = System.Data.Entity.EntityState.Detached;
                                    
                                        // L.DBTrack = dbT;
                                        //LvBankLedger corpledger = new LvBankLedger()
                                        //{
                                            CurCorpledger.OpeningBalance = lvopenbal.OpeningBalance;
                                            CurCorpledger.CreditDays = lvopenbal.CreditDays;
                                            CurCorpledger.ClosingBalance = lvopenbal.ClosingBalance;
                                            CurCorpledger.DebitDays = lvopenbal.UtilizedDays;
                                            //CurCorpledger.CreditDate = L.CreditDate;
                                            CurCorpledger.Id = ledgerid;
                                           // CurCorpledger.DBTrack = L.DBTrack;
                                        //};


                                            db.LvBankLedger.Attach(CurCorpledger);
                                            db.Entry(CurCorpledger).State = System.Data.Entity.EntityState.Modified;
                                            //db.SaveChanges();
                                           // db.Entry(CurCorpledger).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                   
                                    //leave bank Ledger End
                                   


                                    //using (var context = new DataBaseContext())
                                    //{
                                    //    //var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                    //    //DT_LvBankOpenBal DT_Corp = (DT_LvBankOpenBal)obj;
                                    //    //db.Create(DT_Corp);
                                    //    db.SaveChanges();
                                    //}
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (LvEncashPolicy)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {

                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (LvEncashPolicy)databaseEntry.ToObject();
                                    L.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            LvBankOpenBal blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            LvBankOpenBal Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.LvBankOpenBal.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            L.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            LvBankOpenBal corp = new LvBankOpenBal()
                            {
                                CreditDate = L.CreditDate,
                                CreditDays = L.CreditDays,
                                LvCalendar = L.LvCalendar,
                                OpeningBalance = L.OpeningBalance,
                                UtilizedDays = L.UtilizedDays,
                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvBankOpenBal", L.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.LvBankOpenBal.Where(e => e.Id == data).SingleOrDefault();
                                DT_LvEncashPolicy DT_Corp = (DT_LvEncashPolicy)obj;

                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = L.DBTrack;
                            db.LvBankOpenBal.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                        }

                    }
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
                return View();
            }
        }


        public int EditS(int data, LvBankOpenBal L, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurCorp = db.LvBankOpenBal.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    L.DBTrack = dbT;
                    LvBankOpenBal corp = new LvBankOpenBal()
                     {
                         CreditDate = L.CreditDate,
                         CreditDays = L.CreditDays,
                         LvCalendar = L.LvCalendar,
                         OpeningBalance = L.OpeningBalance,
                         UtilizedDays = L.UtilizedDays,
                         Id = data,
                         DBTrack = L.DBTrack
                     };


                    db.LvBankOpenBal.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        public ActionResult GetLookupCalendar(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Calendar.ToList();
                IEnumerable<Calendar> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Calendar.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }


        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    LvBankOpenBal dellvencash = db.LvBankOpenBal.Where(e => e.Id == data).SingleOrDefault();

                    var singlerec = db.LvBankLedger.ToList();
                    if (singlerec.Count()== 1)
                    {
                        Msg.Add("You can Not delete Record Because ledger is Genrated ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (dellvencash.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = dellvencash.DBTrack.CreatedBy != null ? dellvencash.DBTrack.CreatedBy : null,
                                CreatedOn = dellvencash.DBTrack.CreatedOn != null ? dellvencash.DBTrack.CreatedOn : null,
                                IsModified = dellvencash.DBTrack.IsModified == true ? true : false
                            };
                            dellvencash.DBTrack = dbT;
                            db.Entry(dellvencash).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, dellvencash.DBTrack);
                            DT_LvBankOpenBal DT_Corp = (DT_LvBankOpenBal)rtn_Obj;

                            db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = dellvencash.DBTrack.CreatedBy != null ? dellvencash.DBTrack.CreatedBy : null,
                                    CreatedOn = dellvencash.DBTrack.CreatedOn != null ? dellvencash.DBTrack.CreatedOn : null,
                                    IsModified = dellvencash.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.EmpId,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.EmpId, ModifiedOn = DateTime.Now };

                                db.Entry(dellvencash).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, dbT);
                                DT_LvBankOpenBal DT_Corp = (DT_LvBankOpenBal)rtn_Obj;

                                db.Create(DT_Corp);

                                await db.SaveChangesAsync();


                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                            }
                        }
                    }
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

    }
}