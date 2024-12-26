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
using P2B.PFTRUST;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class StatutoryEffectiveMonthsPFTController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        #region Pageslink
        public ActionResult Index()
        {
            return View("~/Views/Shared/PFTrust/_StatutoryEffectiveMonthsPFT.cshtml");
        }
       

        public ActionResult Partial()
        {
            return View("~/Views/Shared/PFTrust/_StatutoryEffectiveMonthsPFT.cshtml");
        }

        #endregion

        #region lookupval
        public ActionResult GetStatutoryEffectiveMonths(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.StatutoryEffectiveMonths.Include(e => e.EffectiveMonth).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.StatutoryEffectiveMonths.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var list1 = db.PTaxMaster.Include(e => e.PTStatutoryEffectiveMonths).SelectMany(e => e.PTStatutoryEffectiveMonths).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.EffectiveMonth.LookupVal }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookup_Range(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Range.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Range.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                //  var list1 = db.StatutoryEffectiveMonths.Include(e => e.StatutoryWageRange).SelectMany(e => e.StatutoryWageRange).ToList();
                // var list2 = fall.Except(list1);
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Pageslink DROPDOWNLISTPOPULATE
        public ActionResult PopulateDropDownListEffectiveMonths(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.LookupValue.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        //public ActionResult GetlookupDetailsStatEffectivemonth(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.StatutoryEffectiveMonths.ToList();
        //        IEnumerable<StatutoryEffectiveMonths> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.StatutoryEffectiveMonths.ToList().Where(d => d.Id.Contains(data));
        //        }
        //        else
        //        {

        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "Code :" + ca.Code + "Name :" + ca.Name }).Distinct();

        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.Code, c.Name }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }

        //}
        #endregion

        #region CRUD OPERATION
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(StatutoryEffectiveMonthsPFT S, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                   
                    string EffectiveMonth = form["EffectiveMonthList"] == "0" ? "" : form["EffectiveMonthList"];
                    

                    

                   
                    if (EffectiveMonth != null && EffectiveMonth != "")
                    {
                        if (EffectiveMonth != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(EffectiveMonth));
                            S.EffectiveMonth = val;
                        }


                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {


                                S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                StatutoryEffectiveMonthsPFT SEM = new StatutoryEffectiveMonthsPFT()
                                {

                                    EffectiveMonth = S.EffectiveMonth,
                                   
                                    DBTrack = S.DBTrack,
                                   
                                };
                                try
                                {
                                    db.StatutoryEffectiveMonthsPFT.Add(SEM);
                                    db.SaveChanges();
                                    //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);
                                    ts.Complete();
                                    Msg.Add("  Data Saved successfully  ");
                                    return Json(new Utility.JsonReturnClass { Id = SEM.Id, Val = SEM.EffectiveMonth.LookupVal , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return this.Json(new Object[] { SEM.Id, SEM.EffectiveMonth.LookupVal, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = S.Id });
                                }
                                catch (DataException /* dex */)
                                {
                                    Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                            // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                            //return this.Json(new { msg = errorMsg });
                        }

                    }
                    else
                    {

                        Msg.Add(" EffectiveMonth Can't Be Empty....  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });

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

        public class StatutoryEffectiveMonthsWagesRangeList
        {
            public Array StatutoryEffectiveMonthsWagesRange_Id { get; set; }
            public Array StatutoryEffectiveMonthsWagesRange_Fulldetails { get; set; }

        };


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.StatutoryEffectiveMonthsPFT
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        EffectiveMonth_Id = e.EffectiveMonth.Id == null ? 0 : e.EffectiveMonth.Id,
                        Action = e.DBTrack.Action,
                       
                    }).ToList();



                var Corp = db.StatutoryEffectiveMonthsPFT.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(StatutoryEffectiveMonthsPFT S, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string EffectiveMonth = form["EffectiveMonthList"] == "0" ? "" : form["EffectiveMonthList"];
                  
                    
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    //if (EffectiveMonth != null && EffectiveMonth != "")
                    //{
                    //    if (EffectiveMonth != "")
                    //    {
                    //        var val = db.LookupValue.Find(int.Parse(EffectiveMonth));
                    //        S.EffectiveMonth = val;
                    //    }



                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    StatutoryEffectiveMonthsPFT typedetails = null;
                                    List<Range> Range = new List<Range>();
                                    typedetails = db.StatutoryEffectiveMonthsPFT.Where(e => e.Id == data).SingleOrDefault();
                                   
                                    if (EffectiveMonth != null)
                                    {
                                        if (EffectiveMonth != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(EffectiveMonth));
                                            S.EffectiveMonth = val;
                                            S.EffectiveMonth_Id = int.Parse(EffectiveMonth);
                                            var type = db.StatutoryEffectiveMonths.Where(e => e.Id == data).SingleOrDefault();
                                            IList<StatutoryEffectiveMonthsPFT> typedetails1 = null;
                                            if (type.EffectiveMonth != null)
                                            {
                                                typedetails1 = db.StatutoryEffectiveMonthsPFT.Where(x => x.EffectiveMonth.Id == type.EffectiveMonth.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails1 = db.StatutoryEffectiveMonthsPFT.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails1)
                                            {
                                                s.EffectiveMonth = S.EffectiveMonth;
                                                s.EffectiveMonth_Id = S.EffectiveMonth_Id;
                                                db.StatutoryEffectiveMonthsPFT.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var BusiTypeDetails = db.StatutoryEffectiveMonthsPFT.Where(x => x.Id == data).ToList();
                                            foreach (var s in BusiTypeDetails)
                                            {
                                                s.EffectiveMonth = null;
                                                s.EffectiveMonth_Id = null;
                                                db.StatutoryEffectiveMonthsPFT.Attach(s);
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
                                        var BusiTypeDetails = db.StatutoryEffectiveMonthsPFT.Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.EffectiveMonth = null;
                                            s.EffectiveMonth_Id = null;
                                            db.StatutoryEffectiveMonthsPFT.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    db.StatutoryEffectiveMonthsPFT.Attach(typedetails);
                                    db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = typedetails.RowVersion;
                                    db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;


                                    //var Curr_OBJ = db.StatutoryEffectiveMonths.Find(data);
                                    //TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    //db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;


                                    StatutoryEffectiveMonthsPFT blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.StatutoryEffectiveMonthsPFT.Where(e => e.Id == data).SingleOrDefault();


                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    S.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };


                                    var BusiTypeDetails1 = db.StatutoryEffectiveMonthsPFT.Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails1)
                                        {

                                            db.StatutoryEffectiveMonthsPFT.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                   


                                    //  int a = EditS(EffectiveMonth, data, S, S.DBTrack);
                                        var CurCorp = db.StatutoryEffectiveMonthsPFT.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        StatutoryEffectiveMonthsPFT SEM = new StatutoryEffectiveMonthsPFT()
                                        {
                                            EffectiveMonth = S.EffectiveMonth,
                                            EffectiveMonth_Id=S.EffectiveMonth_Id,
                                            Id = data,
                                            DBTrack = S.DBTrack,
                                          
                                        };


                                        db.StatutoryEffectiveMonthsPFT.Attach(SEM);
                                        db.Entry(SEM).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(SEM).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);


                                        using (var context = new DataBaseContext())
                                        {


                                            db.SaveChanges();
                                        }
                                        await db.SaveChangesAsync();
                                        ts.Complete();

                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = SEM.Id, Val = SEM.EffectiveMonth.LookupVal, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { S.Id, S.EffectiveMonth, "Record Updated", JsonRequestBehavior.AllowGet });
                                    }

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (StatutoryEffectiveMonthsPFT)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Corporate)databaseEntry.ToObject();
                                    S.RowVersion = databaseValues.RowVersion;

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

                            StatutoryEffectiveMonthsPFT blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            StatutoryEffectiveMonthsPFT Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.StatutoryEffectiveMonthsPFT.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            S.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            StatutoryEffectiveMonthsPFT SEM = new StatutoryEffectiveMonthsPFT()
                            {
                                EffectiveMonth = S.EffectiveMonth,
                               
                                Id = data,
                                DBTrack = S.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Payroll", "M", blog, SEM, "StatutoryEffectiveMonths", S.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                Old_Corp = context.StatutoryEffectiveMonthsPFT.Where(e => e.Id == data).SingleOrDefault();

                                //db.SaveChanges();
                            }
                            blog.DBTrack = S.DBTrack;
                            db.StatutoryEffectiveMonthsPFT.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = S.EffectiveMonth.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, S.EffectiveMonth, "Record Updated", JsonRequestBehavior.AllowGet });
                        }

                    }
                    // }
                    //else
                    //{
                    //    // var errorMsg = "EffectiveMonth Can't Be Empty....";
                    //    Msg.Add("  EffectiveMonth Can't Be Empty....  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });

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
                return View();
            }
        }

        public int EditS(string EffectiveMonth, int data, StatutoryEffectiveMonths S, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (EffectiveMonth != null)
                {
                    if (EffectiveMonth != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(EffectiveMonth));
                        S.EffectiveMonth = val;

                        var type = db.StatutoryEffectiveMonths.Where(e => e.Id == data).SingleOrDefault();
                        IList<StatutoryEffectiveMonths> typedetails = null;
                        if (type.EffectiveMonth != null)
                        {
                            typedetails = db.StatutoryEffectiveMonths.Where(x => x.EffectiveMonth.Id == type.EffectiveMonth.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.StatutoryEffectiveMonths.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.EffectiveMonth = S.EffectiveMonth;
                            db.StatutoryEffectiveMonths.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.StatutoryEffectiveMonths.Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.EffectiveMonth = null;
                            db.StatutoryEffectiveMonths.Attach(s);
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
                    var BusiTypeDetails = db.StatutoryEffectiveMonths.Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.EffectiveMonth = null;
                        db.StatutoryEffectiveMonths.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurCorp = db.StatutoryEffectiveMonths.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    S.DBTrack = dbT;
                    StatutoryEffectiveMonths SEM = new StatutoryEffectiveMonths()
                    {
                        EffectiveMonth = S.EffectiveMonth,
                        Id = data,
                        DBTrack = S.DBTrack
                    };


                    db.StatutoryEffectiveMonths.Attach(SEM);
                    db.Entry(SEM).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(SEM).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        #endregion
        }
    }
}