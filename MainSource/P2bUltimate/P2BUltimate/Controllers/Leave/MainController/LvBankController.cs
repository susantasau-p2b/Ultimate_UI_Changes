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
    public class LvBankController : Controller
    {
        // GET: LvBank
        public ActionResult Index()
        {
            return View("~/Views/Leave/MainViews/LvBank/Index.cshtml");
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        public ActionResult Create(LvBank L, FormCollection form)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    //  var LvCalendarlist = form["LvCalendarlist"] == "0" ? "" : form["LvCalendarlist"];


                    //if (LvCalendarlist != null && LvCalendarlist != "")
                    //{
                    //    var value = db.Calendar.Find(int.Parse(LvCalendarlist));
                    //    L.LvCalendar = value;
                    //}

                    var LvHeadCollectionlist = form["LvHeadCollectionlist"] == "0" ? "" : form["LvHeadCollectionlist"];
                    List<LvHead> ObjLvheadCLHB = new List<LvHead>();
                    if (LvHeadCollectionlist != null && LvHeadCollectionlist != " ")
                    {
                        var ids = one_ids(LvHeadCollectionlist);
                        foreach (var ca in ids)
                        {
                            var value = db.LvHead.Find(ca);
                            ObjLvheadCLHB.Add(value);
                            L.LvHeadCollection = ObjLvheadCLHB;
                        }

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

                            LvBank Lvbank = new LvBank()
                            {
                                CreditDate = L.CreditDate,
                                CreditDays = L.CreditDays,
                                LeaveBankName = L.LeaveBankName,
                                LvDebitInCredit = L.LvDebitInCredit,
                                LvMaxDays = L.LvMaxDays,
                                OccuranceInService = L.OccuranceInService,
                                OpeningBalance = L.OpeningBalance,
                                UtilizedDays = L.UtilizedDays,
                                LvHeadCollection= L.LvHeadCollection,
                                DBTrack = L.DBTrack
                                //  DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.LvBank.Add(Lvbank);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, L.DBTrack);
                                // DT_LvBank DT_Corp = (DT_LvBank)rtn_Obj;
                                // db.Create(DT_Corp);
                                db.SaveChanges();
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
        public async Task<ActionResult> EditSave(LvBank L, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var LvHeadCollectionlist = form["LvHeadCollectionlist"] == "0" ? "" : form["LvHeadCollectionlist"];

                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    LvBank blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.LvBank.Where(e => e.Id == data).SingleOrDefault();
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

                                    List<LvHead> LvHeadObjCLH = new List<LvHead>();
                                    LvBank lvheaddetails = null;
                                    lvheaddetails = db.LvBank.Include(e => e.LvHeadCollection).Where(e => e.Id == data).SingleOrDefault();
                                    if (LvHeadCollectionlist != null && LvHeadCollectionlist != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(LvHeadCollectionlist);
                                        foreach (var ca in ids)
                                        {
                                            var ConvertLeaveHeadlistvalue = db.LvHead.Find(ca);
                                            LvHeadObjCLH.Add(ConvertLeaveHeadlistvalue);
                                            lvheaddetails.LvHeadCollection = LvHeadObjCLH;
                                        }
                                    }
                                    else
                                    {
                                        lvheaddetails.LvHeadCollection = null;
                                    }


                                    //  int a = EditS(data, L, L.DBTrack);

                                    // Surendra start 01/03/2019
                                    var CurCorp = db.LvBank.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        // L.DBTrack = dbT;
                                        LvBank corp = new LvBank()
                                        {
                                            CreditDate = L.CreditDate,
                                            CreditDays = L.CreditDays,
                                            LeaveBankName = L.LeaveBankName,
                                           LvHeadCollection=L.LvHeadCollection,
                                            LvDebitInCredit = L.LvDebitInCredit,
                                            LvMaxDays = L.LvMaxDays,
                                            OccuranceInService = L.OccuranceInService,
                                            OpeningBalance = L.OpeningBalance,
                                            UtilizedDays = L.UtilizedDays,
                                            Id = data,
                                            DBTrack = L.DBTrack
                                        };


                                        db.LvBank.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        // return 1;
                                    }

                                    // Surendra End 01/03/2019



                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                      
                                        db.SaveChanges();
                                    }
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

                            LvBank blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            LvBank Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.LvBank.Where(e => e.Id == data).SingleOrDefault();
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
                            LvBank corp = new LvBank()
                            {
                                CreditDate = L.CreditDate,
                                CreditDays = L.CreditDays,
                                LeaveBankName = L.LeaveBankName,
                                LvDebitInCredit = L.LvDebitInCredit,
                                LvMaxDays = L.LvMaxDays,
                                OccuranceInService = L.OccuranceInService,
                                OpeningBalance = L.OpeningBalance,
                                UtilizedDays = L.UtilizedDays,
                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvBank", L.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.LvBank.Where(e => e.Id == data).SingleOrDefault();
                               // DT_LvEncashPolicy DT_Corp = (DT_LvEncashPolicy)obj;

                              //  db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = L.DBTrack;
                            db.LvBank.Attach(blog);
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

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LvBank
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        CreditDate = e.CreditDate,
                        CreditDays = e.CreditDays,
                        LeaveBankName = e.LeaveBankName,
                        LvDebitInCredit = e.LvDebitInCredit,
                        LvMaxDays = e.LvMaxDays,
                        OccuranceInService = e.OccuranceInService,
                        OpeningBalance = e.OpeningBalance,
                        UtilizedDays = e.UtilizedDays,
                        Action = e.DBTrack.Action



                    }).ToList();


                List<LvBankEditDetails> LvHeadObj = new List<LvBankEditDetails>();

                var CLH = db.LvBank.Include(e => e.LvHeadCollection).Where(e => e.Id == data).ToList();
                if (CLH != null && CLH.Count()>0)
                {
                 
                    foreach (var ca1 in CLH)
                    {
                        var lvh = ca1.LvHeadCollection.ToList();
                        foreach (var ca in lvh)
                        {
                            
                        
                        LvHeadObj.Add(new LvBankEditDetails
                        {
                            LvHeadObjCLH_Id = ca.Id,
                            LvHeadObjCLH_FullDetails = ca.FullDetails.ToString()

                        });
                        }

                    }

                }

                var Corp = db.LvBank.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, LvHeadObj, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }
        public class LvBankEditDetails
        {
            public int LvHeadObjCLH_Id { get; set; }

            public string LvHeadObjCLH_FullDetails { get; set; }

        }

        public ActionResult GetLookupLvHeadObj(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvHead.ToList();
                IEnumerable<LvHead> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LvHead.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


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
                IEnumerable<LvBank> lencash = null;
                if (gp.IsAutho == true)
                {
                    lencash = db.LvBank.Include(r => r.LvHeadCollection).AsNoTracking().Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    lencash = db.LvBank.Include(r => r.LvHeadCollection).AsNoTracking().ToList();
                }

                IEnumerable<LvBank> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = lencash;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.LeaveBankName.ToString().Contains(gp.searchString))
                              || (e.OpeningBalance.ToString().ToUpper().Contains(gp.searchString))
                              || (e.UtilizedDays.ToString().ToUpper().Contains(gp.searchString))
                              || (e.CreditDate.Value.ToShortDateString().ToString().Contains(gp.searchString))
                              || (e.CreditDays.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString))
                              ).Select(a => new Object[] { a.LeaveBankName, a.OpeningBalance, a.UtilizedDays, a.CreditDate.Value.ToShortDateString(), a.CreditDays, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.LeaveBankName != null ? a.LeaveBankName.ToString() : "", a.OpeningBalance, a.UtilizedDays, a.CreditDate.Value.ToShortDateString(), a.CreditDays, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = lencash;
                    Func<LvBank, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "LeaveBankName" ? c.LeaveBankName.ToString() :
                             gp.sidx == "OpeningBalance" ? c.OpeningBalance.ToString() :
                              gp.sidx == "UtilizedDays" ? c.UtilizedDays.ToString() :
                               gp.sidx == "CreditDate" ? c.CreditDate.Value.ToShortDateString().ToString() :
                               gp.sidx == "CreditDays" ? c.CreditDays.ToString() :

                            "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.LeaveBankName != null ? a.LeaveBankName.ToString() : "", a.OpeningBalance, a.UtilizedDays, a.CreditDate.Value.ToShortDateString(), a.CreditDays, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.LeaveBankName != null ? a.LeaveBankName.ToString() : "", a.OpeningBalance, a.UtilizedDays, a.CreditDate.Value.ToShortDateString(), a.CreditDays, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.LeaveBankName != null ? a.LeaveBankName.ToString() : "", a.OpeningBalance, a.UtilizedDays, a.CreditDate.Value.ToShortDateString(), a.CreditDays, a.Id }).ToList();
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
    }
}