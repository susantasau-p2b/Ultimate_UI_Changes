using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Transactions;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Reflection;
using P2b.Global;
using Leave;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Leave.MainController
{
       [AuthoriseManger]
    public class LvBankPolicyController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Leave/MainViews/LvBankPolicy/Index.cshtml");
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

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        [HttpPost]
        public ActionResult Create(LvBankPolicy L, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    //#ConvertLeaveHeadBallist,#ConvertLeaveHeadlist
                    var LvHeadCollectionlist = form["LvHeadCollectionlist"] == "0" ? "" : form["LvHeadCollectionlist"];
                    List<LvHead> ObjLvhead = new List<LvHead>();
                    if (LvHeadCollectionlist != null && LvHeadCollectionlist != "")
                    {
                        var value = db.LvHead.Find(int.Parse(LvHeadCollectionlist));
                        ObjLvhead.Add(value);
                        L.LvHeadCollection = ObjLvhead;

                    }
                    var SeviceLockOnDebit = form["SeviceLockOnDebit"] == "0" ? "0" : form["SeviceLockOnDebit"];
                    L.IsSeviceLockOnDebit = Convert.ToBoolean(SeviceLockOnDebit);
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var companyLeave = new CompanyLeave();
                    companyLeave = db.CompanyLeave.Where(e => e.Company.Id == company_Id).SingleOrDefault();

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId };

                            LvBankPolicy OBJLVBP = new LvBankPolicy()
                            {
                                LeaveBankName = L.LeaveBankName,
                                LvMaxDays = L.LvMaxDays,
                                OccuranceInService = L.OccuranceInService,
                                LvDebitInCredit = L.LvDebitInCredit,
                                LvHeadCollection = L.LvHeadCollection,
                                IsSeviceLockOnDebit=L.IsSeviceLockOnDebit,
                                MaxServiceForDebit=L.MaxServiceForDebit,
                                DBTrack = L.DBTrack
                            };
                            try
                            {
                                db.LvBankPolicy.Add(OBJLVBP);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, L.DBTrack);
                                DT_LvBankPolicy DT_OBJ = (DT_LvBankPolicy)rtn_Obj;
                                db.Create(DT_OBJ);
                                db.SaveChanges();

                                if (companyLeave != null)
                                {
                                    var LvBankPolicy_list = new List<LvBankPolicy>();
                                    LvBankPolicy_list.Add(OBJLVBP);
                                    companyLeave.LvBankPolicy = LvBankPolicy_list;
                                    db.CompanyLeave.Attach(companyLeave);
                                    db.Entry(companyLeave).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companyLeave).State = System.Data.Entity.EntityState.Detached;
                                }

                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = OBJLVBP.Id, Val = OBJLVBP.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { OBJLVBP.Id, OBJLVBP.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
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
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        // return this.Json(new { msg = errorMsg });
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

        public class LvBankPolicyEditDetails
        {
            public Array LvHead_Id { get; set; }

            public Array LvHead_FullDetails { get; set; }

          
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LvBankPolicy
                    .Include(e => e.LvHeadCollection)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        LeaveBankName = e.LeaveBankName,
                        LvMaxDays = e.LvMaxDays,
                        OccuranceInService = e.OccuranceInService,
                        LvDebitInCredit = e.LvDebitInCredit,
                        Action = e.DBTrack.Action,
                        IsSeviceLockOnDebit = e.IsSeviceLockOnDebit,
                        MaxServiceForDebit = e.MaxServiceForDebit
                    }).ToList();

                List<LvBankPolicyEditDetails> LvHeadObj = new List<LvBankPolicyEditDetails>();

                var Lvheadcollection = db.LvBankPolicy.Include(e => e.LvHeadCollection).Where(e => e.Id == data).Select(e => e.LvHeadCollection).ToList();
                if (Lvheadcollection != null)
                {
                    foreach (var ca in Lvheadcollection)
                    {
                        LvHeadObj.Add(new LvBankPolicyEditDetails
                        {
                            LvHead_Id = ca.Select(e => e.Id).ToArray(),
                            LvHead_FullDetails = ca.Select(e => e.FullDetails).ToArray()

                        });


                    }

                }


                //var W = db.DT_LvBank
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         WFStatus_Val = e. == 0 ? "" : db.LookupValue
                //                    .Where(x => x.Id == e.CreditDate_Id)
                //                    .Select(x => x.LookupVal).FirstOrDefault(),

                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.LvBankPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, LvHeadObj, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

            
        //[HttpPost]
        //public async Task<ActionResult> EditSave(LvBankPolicy L, int data, FormCollection form) // Edit submit
        //{
        //     List<string> Msg = new List<string>();
        //try{

        //    bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //    var LvHeadCollectionlist = form["LvHeadCollectionlist"] == "0" ? "" : form["LvHeadCollectionlist"];
        //    List<LvHead> ObjLvhead = new List<LvHead>();
        //    if (LvHeadCollectionlist != null && LvHeadCollectionlist != "")
        //    {
        //        var value = db.LvHead.Find(int.Parse(LvHeadCollectionlist));
        //        ObjLvhead.Add(value);
        //        L.LvHeadCollection = ObjLvhead;

        //    }
        //    if (Auth == false)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            try
        //            {
        //                //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    LvBankPolicy blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.LvBankPolicy.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }

        //                    L.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    int a = EditS(data, L, L.DBTrack);



        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, L.DBTrack);
        //                        DT_LvBankPolicy DT_Corp = (DT_LvBankPolicy)obj;
        //                        db.Create(DT_Corp);
        //                        db.SaveChanges();
        //                    }
        //                    await db.SaveChangesAsync();
        //                    ts.Complete();
        //                    Msg.Add("  Record Updated");
        //            return Json(new Utility.JsonReturnClass { Id =  L.Id   , Val = L .FullDetails , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                   // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

        //                }
        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (LvEncashPolicy)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                     Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                }
        //                else
        //                {
        //                    var databaseValues = (LvEncashPolicy)databaseEntry.ToObject();
        //                    L.RowVersion = databaseValues.RowVersion;

        //                }
        //            }
        //            Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    else
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {

        //            LvBankPolicy blog = null; // to retrieve old data
        //            DbPropertyValues originalBlogValues = null;
        //            LvBankPolicy Old_Corp = null;

        //            using (var context = new DataBaseContext())
        //            {
        //                blog = context.LvBankPolicy.Where(e => e.Id == data).SingleOrDefault();
        //                originalBlogValues = context.Entry(blog).OriginalValues;
        //            }
        //            L.DBTrack = new DBTrack
        //            {
        //                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                Action = "M",
        //                IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                ModifiedBy = SessionManager.UserName,
        //                ModifiedOn = DateTime.Now
        //            };
        //            LvBankPolicy corp = new LvBankPolicy()
        //            {
        //                LeaveBankName = L.LeaveBankName,
        //                LvMaxDays = L.LvMaxDays,
        //                OccuranceInService = L.OccuranceInService,
        //                LvDebitInCredit = L.LvDebitInCredit,
        //                LvHeadCollection = L.LvHeadCollection,
        //                Id = data,
        //                DBTrack = L.DBTrack,
        //                RowVersion = (Byte[])TempData["RowVersion"]
        //            };


        //            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //            using (var context = new DataBaseContext())
        //            {
        //                var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvBankPolicy", L.DBTrack);
        //                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                Old_Corp = context.LvBankPolicy.Where(e => e.Id == data).SingleOrDefault();
        //                DT_LvEncashPolicy DT_Corp = (DT_LvEncashPolicy)obj;

        //                db.Create(DT_Corp);
        //                //db.SaveChanges();
        //            }
        //            blog.DBTrack = L.DBTrack;
        //            db.LvBankPolicy.Attach(blog);
        //            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            db.SaveChanges();
        //            ts.Complete();
        //             Msg.Add("  Record Updated");
        //            return Json(new Utility.JsonReturnClass { Id =blog  .Id   , Val =  L.FullDetails , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //            //return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
        //        }

        //    }
        //            }
        //            catch (Exception ex)
        //            {
        //                LogFile Logfile = new LogFile();
        //                ErrorLog Err = new ErrorLog()
        //                {
        //                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                    ExceptionMessage = ex.Message,
        //                    ExceptionStackTrace = ex.StackTrace,
        //                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                    LogTime = DateTime.Now
        //                };
        //                Logfile.CreateLogFile(Err);
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            } 
        //    return View();

        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(LvBankPolicy L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
                try
                {
                    var LvHeadCollectionlist = form["LvHeadCollectionlist"] == "0" ? "" : form["LvHeadCollectionlist"];
                    var blog1 = db.LvBankPolicy.Where(e => e.Id == data).Include(e => e.LvHeadCollection).SingleOrDefault();
                   
                   var SeviceLockOnDebit=form["SeviceLockOnDebit"] == "0" ? "0" : form["SeviceLockOnDebit"];
                   L.IsSeviceLockOnDebit = Convert.ToBoolean(SeviceLockOnDebit);
                    blog1.LvHeadCollection = null;


                    List<LvHead> ObjITsection = new List<LvHead>();
                    LvBankPolicy pd = null;
                    pd = db.LvBankPolicy.Include(e => e.LvHeadCollection).Where(e => e.Id == data).SingleOrDefault();
                    if (LvHeadCollectionlist != null && LvHeadCollectionlist != "")
                    {
                        var ids = Utility.StringIdsToListIds(LvHeadCollectionlist);
                        foreach (var ca in ids)
                        {
                            var value = db.LvHead.Find(ca);
                            ObjITsection.Add(value);
                            pd.LvHeadCollection = ObjITsection;

                        }
                    }
                    else
                    {
                        pd.LvHeadCollection = null;
                    }

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            {

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    using (var context = new DataBaseContext())
                                    {

                                    }

                                    blog1.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
                                        CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    var CurCorp = db.LvBankPolicy.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        LvBankPolicy post = new LvBankPolicy()
                                        
                                        {
                                            LeaveBankName = L.LeaveBankName,
                                            LvMaxDays = L.LvMaxDays,
                                            OccuranceInService = L.OccuranceInService,
                                            LvDebitInCredit = L.LvDebitInCredit,
                                            IsSeviceLockOnDebit = L.IsSeviceLockOnDebit,
                                            MaxServiceForDebit = L.MaxServiceForDebit,
                                            LvHeadCollection = blog1.LvHeadCollection,
                                            DBTrack = blog1.DBTrack,
                                            Id = data,
                                        };
                                        db.LvBankPolicy.Attach(post);
                                        db.Entry(post).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        db.SaveChanges();

                                        await db.SaveChangesAsync();
                                        db.Entry(post).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = blog1.Id, Val = blog1.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                    }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (LvBankPolicy)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (LvBankPolicy)databaseEntry.ToObject();
                                blog1.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

                    }
                    return Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
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


        public int EditS(int data, LvBankPolicy L, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurCorp = db.LvBankPolicy.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    L.DBTrack = dbT;
                    LvBankPolicy corp = new LvBankPolicy()
                    {
                        LeaveBankName = L.LeaveBankName,
                        LvMaxDays = L.LvMaxDays,
                        OccuranceInService = L.OccuranceInService,
                        LvDebitInCredit = L.LvDebitInCredit,
                        LvHeadCollection = L.LvHeadCollection,

                        Id = data,
                        DBTrack = L.DBTrack
                    };


                    db.LvBankPolicy.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }


        //[HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{
        //     List<string> Msg = new List<string>();
        //            try{
        //    LvBankPolicy dellvencash = db.LvBankPolicy.Where(e => e.Id == data).SingleOrDefault();

        //    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
        //    if (dellvencash.DBTrack.IsModified == true)
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {
        //            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
        //            DBTrack dbT = new DBTrack
        //            {
        //                Action = "D",
        //                CreatedBy = dellvencash.DBTrack.CreatedBy != null ? dellvencash.DBTrack.CreatedBy : null,
        //                CreatedOn = dellvencash.DBTrack.CreatedOn != null ? dellvencash.DBTrack.CreatedOn : null,
        //                IsModified = dellvencash.DBTrack.IsModified == true ? true : false
        //            };
        //            dellvencash.DBTrack = dbT;
        //            db.Entry(dellvencash).State = System.Data.Entity.EntityState.Modified;
        //            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, dellvencash.DBTrack);
        //            DT_LvBankPolicy DT_Corp = (DT_LvBankPolicy)rtn_Obj;

        //            db.Create(DT_Corp);
        //            // db.SaveChanges();
        //            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
        //            await db.SaveChangesAsync();
        //            //using (var context = new DataBaseContext())
        //            //{
        //            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
        //            //}
        //            ts.Complete();
        //             Msg.Add("  Data removed successfully.  ");
        //           return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    else
        //    {

        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {
        //            var Lvheadcollection = dellvencash.LvHeadCollection;
        //            if (Lvheadcollection != null)
        //            {
        //                var LvBankPolicy = new HashSet<int>(dellvencash.LvHeadCollection.Select(e => e.Id));
        //                if (LvBankPolicy.Count > 0)
        //                {
        //                     Msg.Add(" Child record exists.Cannot remove it..  ");
        //                       return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
        //                    // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            try
        //            {
        //                DBTrack dbT = new DBTrack
        //                {
        //                    Action = "D",
        //                    ModifiedBy = SessionManager.UserName,
        //                    ModifiedOn = DateTime.Now,
        //                    CreatedBy = dellvencash.DBTrack.CreatedBy != null ? dellvencash.DBTrack.CreatedBy : null,
        //                    CreatedOn = dellvencash.DBTrack.CreatedOn != null ? dellvencash.DBTrack.CreatedOn : null,
        //                    IsModified = dellvencash.DBTrack.IsModified == true ? false : false//,
        //                    //AuthorizedBy = SessionManager.EmpId,
        //                    //AuthorizedOn = DateTime.Now
        //                };

        //                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.EmpId, ModifiedOn = DateTime.Now };

        //                db.Entry(dellvencash).State = System.Data.Entity.EntityState.Deleted;
        //                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, dbT);
        //                //DT_LvBankPolicy DT_Corp = (DT_LvBankPolicy)rtn_Obj;
        //                //db.Create(DT_Corp);
        //               // await db.SaveChangesAsync();


        //                ts.Complete();
        //                Msg.Add("  Data removed successfully.  ");
        //   return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //               // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

        //            }
        //            catch (RetryLimitExceededException /* dex */)
        //            {
        //                //Log the error (uncomment dex variable name and add a line here to write a log.)
        //                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
        //                //return RedirectToAction("Delete");
        //                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //               // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
        //            }
        //        }
        //    }
        //            }
        //            catch (Exception ex)
        //            {
        //                LogFile Logfile = new LogFile();
        //                ErrorLog Err = new ErrorLog()
        //                {
        //                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                    ExceptionMessage = ex.Message,
        //                    ExceptionStackTrace = ex.StackTrace,
        //                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                    LogTime = DateTime.Now
        //                };
        //                Logfile.CreateLogFile(Err);
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            } 
        //}

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    LvBankPolicy Postdetails = db.LvBankPolicy.Include(e => e.LvHeadCollection).Where(e => e.Id == data).SingleOrDefault();


                    if (Postdetails.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Postdetails.DBTrack.CreatedBy != null ? Postdetails.DBTrack.CreatedBy : null,
                                CreatedOn = Postdetails.DBTrack.CreatedOn != null ? Postdetails.DBTrack.CreatedOn : null,
                                IsModified = Postdetails.DBTrack.IsModified == true ? true : false
                            };
                            Postdetails.DBTrack = dbT;
                            db.Entry(Postdetails).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, Postdetails.DBTrack);
                            DT_LvBankPolicy DT_Post = (DT_LvBankPolicy)rtn_Obj;
                            //DT_Post.ExpFilter_Id = Postdetails.ExpFilter == null ? 0 : Postdetails.ExpFilter.Id;
                            //DT_Post.RangeFilter_Id = Postdetails.RangeFilter == null ? 0 : Postdetails.RangeFilter.Id;
                            //DT_Post.Gender_Id = Postdetails.Gender == null ? 0 : Postdetails.Gender.Id;                                      // the declaratn for lookup is remain 
                            //DT_Post.MaritalStatus_Id = Postdetails.MaritalStatus == null ? 0 : Postdetails.MaritalStatus.Id;
                            //DT_Post.FuncStruct_Id = Postdetails.FuncStruct == null ? 0 : Postdetails.FuncStruct.Id;
                            db.Create(DT_Post);

                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                           {
                               Action = "D",
                               ModifiedBy = SessionManager.UserName,
                               ModifiedOn = DateTime.Now,
                               CreatedBy = Postdetails.DBTrack.CreatedBy != null ? Postdetails.DBTrack.CreatedBy : null,
                               CreatedOn = Postdetails.DBTrack.CreatedOn != null ? Postdetails.DBTrack.CreatedOn : null,
                               IsModified = Postdetails.DBTrack.IsModified == true ? false : false//,
                               //AuthorizedBy = SessionManager.UserName,
                               //AuthorizedOn = DateTime.Now
                           };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(Postdetails).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, dbT);
                            // var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            DT_LvBankPolicy DT_Post = (DT_LvBankPolicy)rtn_Obj;
                            db.Create(DT_Post);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");                                                                                             // the original place 
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                var jsonData = (Object)null;
                IEnumerable<LvBankPolicy> lencash = null;
                if (gp.IsAutho == true)
                {
                    lencash = db.LvBankPolicy.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    lencash = db.LvBankPolicy.AsNoTracking().ToList();
                }

                IEnumerable<LvBankPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = lencash;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.LeaveBankName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.LvMaxDays.ToString().Contains(gp.searchString))
                               || (e.OccuranceInService.ToString().Contains(gp.searchString))
                               || (e.LvDebitInCredit.ToString().Contains(gp.searchString))
                               || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.LeaveBankName, a.LvMaxDays, a.OccuranceInService, a.LvDebitInCredit, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.LeaveBankName, a.LvMaxDays, a.OccuranceInService, a.LvDebitInCredit, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = lencash;
                    Func<LvBankPolicy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>   gp.sidx == "LeaveBankName" ? c.LeaveBankName :
                                           gp.sidx == "LvMaxDays" ? Convert.ToString(c.LvMaxDays) :
                                           gp.sidx == "OccuranceInService" ? Convert.ToString(c.OccuranceInService) :
                                           gp.sidx == "LvDebitInCredit" ? Convert.ToString(c.LvDebitInCredit) : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.LeaveBankName), Convert.ToString(a.LvMaxDays), Convert.ToString(a.OccuranceInService), Convert.ToString(a.LvDebitInCredit), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.LeaveBankName), Convert.ToString(a.LvMaxDays), a.OccuranceInService, Convert.ToString(a.LvDebitInCredit), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.LeaveBankName), Convert.ToString(a.LvMaxDays), a.OccuranceInService, Convert.ToString(a.LvDebitInCredit), a.Id }).ToList();
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