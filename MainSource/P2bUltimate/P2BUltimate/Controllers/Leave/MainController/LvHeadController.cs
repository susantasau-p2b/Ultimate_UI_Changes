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
using Leave;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Leave.MainController
{
    [AuthoriseManger]
    public class LvHeadController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Leave/MainViews/LvHead/Index.cshtml");
        }
        DataBaseContext db = new DataBaseContext();
        public ActionResult GetLookupDetails(string data)
        {
            {
                var fall = db.LvHead.ToList();
                IEnumerable<LvHead> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.LvHead.ToList().Where(d => d.LvCode.ToString().Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult Create(LvHead L, FormCollection form)
        {
            List<string> Msg = new List<string>();
            try
            {
                //#ConvertLeaveHeadBallist,#ConvertLeaveHeadlist
                var LvHeadOprationTypelist = form["LvHeadOprationTypelist"] == "0" ? "" : form["LvHeadOprationTypelist"];

                var LvActionOnAttlist = form["LvActionOnAtt"] == "0" ? "" : form["LvActionOnAtt"];

                var HFPay = form["HFPay"] == "0" ? "" : form["HFPay"];
                var LTAAppl = form["LTAAppl"] == "0" ? "" : form["LTAAppl"];
                var ApplAtt = form["ApplAtt"] == "0" ? "" : form["ApplAtt"];
                var EncashRegular = form["EncashRegular"] == "0" ? "" : form["EncashRegular"];
                var EncashRetirement = form["EncashRetirement"] == "0" ? "" : form["EncashRetirement"];
                var ESS = form["ESS"] == "0" ? "" : form["ESS"];
                var ExemptEncashAmtRetirement = form["ExemptEncashAmtRetirement"] == "0" ? "" : form["ExemptEncashAmtRetirement"];


                L.HFPay = Convert.ToBoolean(HFPay);
                L.LTAAppl = Convert.ToBoolean(LTAAppl);
                L.ApplAtt = Convert.ToBoolean(ApplAtt);
                L.EncashRegular = Convert.ToBoolean(EncashRegular);
                L.EncashRetirement = Convert.ToBoolean(EncashRetirement);
                L.ESS = Convert.ToBoolean(ESS);
                L.ExemptEncashAmtRetirement = Convert.ToBoolean(ExemptEncashAmtRetirement);

                using (DataBaseContext db = new DataBaseContext())
                {
                    if (LvHeadOprationTypelist != null && LvHeadOprationTypelist != "-Select-" && LvHeadOprationTypelist != "")
                    {
                        var value = db.LookupValue.Find(int.Parse(LvHeadOprationTypelist));
                        L.LvHeadOprationType = value;
                    }

                    if (LvActionOnAttlist != null && LvActionOnAttlist != "-Select-" && LvActionOnAttlist != "")
                    {
                        //var value = db.LookupValue.Find(int.Parse(LvActionOnAttlist));
                        //int LvAtt = Convert.ToInt32(value.LookupVal);
                        //L.LvActionOnAtt = LvAtt;
                        var value = db.LookupValue.Find(int.Parse(LvActionOnAttlist));
                        L.LvActionOnAtt = value;
                    }

                    if (db.LvHead.Any(e => e.LvCode.Replace(" ", String.Empty) == L.LvCode.Replace(" ", String.Empty)))
                    {
                        Msg.Add(" Code Already Exist ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    //if (ModelState.IsValid)
                    //{
                    using (TransactionScope ts = new TransactionScope())
                    {


                        L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId };

                        LvHead OBJLVH = new LvHead()
                        {
                            ApplAtt = L.ApplAtt,
                            EncashRegular = L.EncashRegular,
                            EncashRetirement = L.EncashRetirement,
                            ESS = L.ESS,
                            ExemptEncashAmtRetirement = L.ExemptEncashAmtRetirement,
                            HFPay = L.HFPay,
                            LTAAppl = L.LTAAppl,
                            LvCode = L.LvCode,
                            LvHeadAlias = L.LvHeadAlias,
                            LvHeadOprationType = L.LvHeadOprationType,
                            LvName = L.LvName,
                            LvActionOnAtt = L.LvActionOnAtt,
                            DBTrack = L.DBTrack
                        };
                        try
                        {
                            db.LvHead.Add(OBJLVH);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, L.DBTrack);
                            DT_LvHead DT_OBJ = (DT_LvHead)rtn_Obj;
                            db.Create(DT_OBJ);
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data Created successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = OBJLVH.Id, Val = OBJLVH.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new Object[] { OBJLVH.Id, OBJLVH.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
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
                    //}
                    //    else
                    //    {
                    //        StringBuilder sb = new StringBuilder("");
                    //        foreach (ModelState modelState in ModelState.Values)
                    //        {
                    //            foreach (ModelError error in modelState.Errors)
                    //            {
                    //                sb.Append(error.ErrorMessage);
                    //                sb.Append("." + "\n");
                    //            }
                    //        }
                    //        var errorMsg = sb.ToString();
                    //         Msg.Add(errorMsg);
                    //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //       // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //        // return this.Json(new { msg = errorMsg });
                    //    }

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
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LvHead
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        ApplAtt = e.ApplAtt,
                        EncashRegular = e.EncashRegular,
                        EncashRetirement = e.EncashRetirement,
                        ESS = e.ESS,
                        ExemptEncashAmtRetirement = e.ExemptEncashAmtRetirement,
                        HFPay = e.HFPay,
                        LTAAppl = e.LTAAppl,
                        LvCode = e.LvCode,
                        LvHeadAlias = e.LvHeadAlias,
                        LvActionOnAtt_Id = e.LvActionOnAtt != null ? e.LvActionOnAtt.Id : 0,
                        LvHeadOprationType_Id = e.LvHeadOprationType.Id == null ? 0 : e.LvHeadOprationType.Id,
                        LvName = e.LvName,
                        Action = e.DBTrack.Action
                    }).ToList();

                //var LVWF = db.Lookup.Where(e => e.Code == "1005").Select(r => r.LookupValues).ToList().FirstOrDefault();
                //int LvAtt = db.LvHead.Where(e => e.Id == data).Select(e => e.LvActionOnAtt).FirstOrDefault();
                //string LvAttDet = "";

                //foreach (var item in LVWF)
                //{
                //if (LvAtt == Convert.ToInt32(item.LookupVal))
                //{
                //    LvAttDet = db.LookupValue.Where(e => e.Id == item.Id).Select(r => r.LookupValData).FirstOrDefault();

                //}



                //}


                var W = db.DT_LvHead
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         LvHeadOprationType_Id = e.LvHeadOprationType_Id == null ? 0 : e.LvHeadOprationType_Id,


                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.LvHead.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(LvHead L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            try
            {
                var LvHeadOprationTypelist = form["LvHeadOprationTypelist"] == "0" ? "" : form["LvHeadOprationTypelist"];
                var LvActionOnAttlist = form["LvActionOnAtt"] == "0" ? "" : form["LvActionOnAtt"];
                var HFPay = form["HFPay"] == "0" ? "" : form["HFPay"];
                var LTAAppl = form["LTAAppl"] == "0" ? "" : form["LTAAppl"];
                var ApplAtt = form["ApplAtt"] == "0" ? "" : form["ApplAtt"];
                var EncashRegular = form["EncashRegular"] == "0" ? "" : form["EncashRegular"];
                var EncashRetirement = form["EncashRetirement"] == "0" ? "" : form["EncashRetirement"];
                var ESS = form["ESS"] == "0" ? "" : form["ESS"];
                var ExemptEncashAmtRetirement = form["ExemptEncashAmtRetirement"] == "0" ? "" : form["ExemptEncashAmtRetirement"];

                L.HFPay = Convert.ToBoolean(HFPay);
                L.LTAAppl = Convert.ToBoolean(LTAAppl);
                L.ApplAtt = Convert.ToBoolean(ApplAtt);
                L.EncashRegular = Convert.ToBoolean(EncashRegular);
                L.EncashRetirement = Convert.ToBoolean(EncashRetirement);
                L.ESS = Convert.ToBoolean(ESS);
                L.ExemptEncashAmtRetirement = Convert.ToBoolean(ExemptEncashAmtRetirement);

                bool Auth = form["Autho_Allow"] == "true" ? true : false;
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (LvHeadOprationTypelist != null && LvHeadOprationTypelist != "" && LvHeadOprationTypelist != "-Select-")
                    {
                        var value = db.LookupValue.Find(int.Parse(LvHeadOprationTypelist));
                        L.LvHeadOprationType = value;
                    }
                    if (LvHeadOprationTypelist != null && LvHeadOprationTypelist != "" && LvHeadOprationTypelist != "-Select-")
                    {

                        L.LvHeadOprationType_Id = int.Parse(LvHeadOprationTypelist);
                    }
                    if (LvActionOnAttlist != null && LvActionOnAttlist != "-Select-" && LvActionOnAttlist != "")
                    {
                        //var value = db.LookupValue.Find(int.Parse(LvActionOnAttlist));
                        //int LvAtt = Convert.ToInt32(value.LookupVal);
                        //L.LvActionOnAtt = LvAtt;
                        var value = db.LookupValue.Find(int.Parse(LvActionOnAttlist));
                        L.LvActionOnAtt = value;
                    }

                    if (Auth == false)
                    {


                        //if (ModelState.IsValid)
                        //{
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                LvHead blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.LvHead.Where(e => e.Id == data).Include(e => e.LvHeadOprationType)
                                                            .SingleOrDefault();
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

                                if (LvHeadOprationTypelist != null)
                                {
                                    if (LvHeadOprationTypelist != "")
                                    {
                                        var val = db.LookupValue.Find(int.Parse(LvHeadOprationTypelist));
                                        L.LvHeadOprationType = val;

                                        var type = db.LvHead.Include(e => e.LvHeadOprationType).Where(e => e.Id == data).SingleOrDefault();
                                        IList<LvHead> typedetails = null;
                                        if (type.LvHeadOprationType != null)
                                        {
                                            typedetails = db.LvHead.Where(x => x.LvHeadOprationType.Id == type.LvHeadOprationType.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.LvHead.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.LvHeadOprationType = L.LvHeadOprationType;
                                            db.LvHead.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var LvHeadOprationTypeDetails = db.LvHead.Include(e => e.LvHeadOprationType).Where(x => x.Id == data).ToList();
                                        foreach (var s in LvHeadOprationTypeDetails)
                                        {
                                            s.LvHeadOprationType = null;
                                            db.LvHead.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                }
                                if (LvActionOnAttlist != null)
                                {
                                    if (LvActionOnAttlist != "")
                                    {
                                        var val = db.LookupValue.Find(int.Parse(LvActionOnAttlist));
                                        L.LvActionOnAtt = val;

                                        var type = db.LvHead.Include(e => e.LvActionOnAtt).Where(e => e.Id == data).SingleOrDefault();
                                        IList<LvHead> typedetails = null;
                                        if (type.LvActionOnAtt != null)
                                        {
                                            typedetails = db.LvHead.Where(x => x.LvActionOnAtt.Id == type.LvActionOnAtt.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.LvHead.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.LvActionOnAtt = L.LvActionOnAtt;
                                            db.LvHead.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var LvHeadOprationTypeDetails = db.LvHead.Include(e => e.LvActionOnAtt).Where(x => x.Id == data).ToList();
                                        foreach (var s in LvHeadOprationTypeDetails)
                                        {
                                            s.LvActionOnAtt = null;
                                            db.LvHead.Attach(s);
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
                                    var LvHeadOprationTypeDetails = db.LvHead.Include(e => e.LvHeadOprationType).Where(x => x.Id == data).ToList();
                                    foreach (var s in LvHeadOprationTypeDetails)
                                    {
                                        s.LvHeadOprationType = null;
                                        db.LvHead.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }

                                var CurCorp = db.LvHead.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    LvHead LvHead = new LvHead()
                                    {
                                        ApplAtt = L.ApplAtt,
                                        EncashRegular = L.EncashRegular,
                                        EncashRetirement = L.EncashRetirement,
                                        ESS = L.ESS,
                                        ExemptEncashAmtRetirement = L.ExemptEncashAmtRetirement,
                                        HFPay = L.HFPay,
                                        LTAAppl = L.LTAAppl,
                                        LvCode = L.LvCode,
                                        LvHeadAlias = L.LvHeadAlias,
                                        LvHeadOprationType = L.LvHeadOprationType,
                                        LvHeadOprationType_Id = L.LvHeadOprationType_Id,
                                        LvName = L.LvName,
                                        LvActionOnAtt = L.LvActionOnAtt,
                                        Id = data,
                                        DBTrack = L.DBTrack
                                    };
                                    db.LvHead.Attach(LvHead);
                                    db.Entry(LvHead).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(LvHead).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                }
                                // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                using (var context = new DataBaseContext())
                                {
                                    var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                    DT_LvHead DT_Corp = (DT_LvHead)obj;
                                    DT_Corp.LvHeadOprationType_Id = blog.LvHeadOprationType == null ? 0 : blog.LvHeadOprationType.Id;
                                    db.Create(DT_Corp);
                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                ts.Complete();
                                //return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (LvCreditPolicy)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (LvEncashReq)databaseEntry.ToObject();
                                L.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        //}
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            LvHead blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            LvHead Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.LvHead.Where(e => e.Id == data).SingleOrDefault();
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

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            LvHead corp = new LvHead()
                            {
                                ApplAtt = L.ApplAtt,
                                EncashRegular = L.EncashRegular,
                                EncashRetirement = L.EncashRetirement,
                                ESS = L.ESS,
                                ExemptEncashAmtRetirement = L.ExemptEncashAmtRetirement,
                                HFPay = L.HFPay,
                                LTAAppl = L.LTAAppl,
                                LvCode = L.LvCode,
                                LvHeadAlias = L.LvHeadAlias,
                                LvActionOnAtt = L.LvActionOnAtt,
                                LvHeadOprationType = L.LvHeadOprationType,
                                LvName = L.LvName,
                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvEncashReq", L.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.LvHead.Where(e => e.Id == data).Include(e => e.LvHeadOprationType)
                                                        .SingleOrDefault();
                                DT_LvHead DT_Corp = (DT_LvHead)obj;
                                DT_Corp.LvHeadOprationType_Id = blog.LvHeadOprationType == null ? 0 : blog.LvHeadOprationType.Id;

                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = L.DBTrack;
                            db.LvHead.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                        }

                    }

                    return View();
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
        public ActionResult GetLookupValueDATA(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                SelectList svaldata = (SelectList)null;
                var selected = "";
                List<string> AppStatus = new List<string> { "0", "1", "3", "5", "7" };
                if (data != "" && data != null)
                {
                    var qurey1 = db.Lookup.Where(e => e.Code == data).Select(e => e.LookupValues.Where(r => r.IsActive == true && AppStatus.Contains(r.LookupVal))).SingleOrDefault(); // added

                    if (data2 != "" && data2 != "0")
                    {
                        selected = data2;
                    }
                    if (qurey1 != null)
                    {
                        svaldata = new SelectList(qurey1, "Id", "LookupValData", selected);
                    }

                }
                return Json(svaldata, JsonRequestBehavior.AllowGet);

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
                IEnumerable<LvHead> lencash = null;
                if (gp.IsAutho == true)
                {
                    lencash = db.LvHead.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    lencash = db.LvHead.AsNoTracking().ToList();
                }

                IEnumerable<LvHead> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = lencash;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.LvCode.ToUpper().Contains(gp.searchString.ToUpper()))
                            || (e.LvName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                             || (e.LvHeadAlias.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.LvCode, a.LvName, a.LvHeadAlias, a.Id }).ToList();


                        //jsonData = IE.Select(a => new { a.Id, a.LvCode, a.LvName, a.LvHeadAlias }).Where((e => (e.Id.ToString() == gp.searchString) || (e.LvCode.ToLower() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.LvCode), Convert.ToString(a.LvName), Convert.ToString(a.LvHeadAlias), a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = lencash;
                    Func<LvHead, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "LvCode" ? c.LvCode.ToString() :
                            gp.sidx == "LvName" ? c.LvCode.ToString() :
                            gp.sidx == "LvHeadAlias" ? c.LvCode.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.LvCode), Convert.ToString(a.LvName), Convert.ToString(a.LvHeadAlias), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.LvCode), Convert.ToString(a.LvName), Convert.ToString(a.LvHeadAlias), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.LvCode), Convert.ToString(a.LvName), Convert.ToString(a.LvHeadAlias), a.Id }).ToList();
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
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {

                    LvHead OBJLvHead = db.LvHead.Include(e => e.LvHeadOprationType)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    var asda = db.LvOpenBal.Include(q => q.LvHead).Where(e => e.LvHead.Id == data).Count();
                    if (asda > 0)
                    {
                        Msg.Add(" child record exist ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    var asdag = db.LvCreditPolicy.Include(q => q.LvHead).Where(e => e.LvHead.Id == data).Count();
                    if (asdag > 0)
                    {
                        Msg.Add(" child record exist ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (OBJLvHead.DBTrack.IsModified == true)
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = OBJLvHead.DBTrack.CreatedBy != null ? OBJLvHead.DBTrack.CreatedBy : null,
                                CreatedOn = OBJLvHead.DBTrack.CreatedOn != null ? OBJLvHead.DBTrack.CreatedOn : null,
                                IsModified = OBJLvHead.DBTrack.IsModified == true ? true : false
                            };
                            OBJLvHead.DBTrack = dbT;
                            db.Entry(OBJLvHead).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, OBJLvHead.DBTrack);
                            DT_LvHead DT_Corp = (DT_LvHead)rtn_Obj;
                            DT_Corp.LvHeadOprationType_Id = OBJLvHead.LvHeadOprationType == null ? 0 : OBJLvHead.LvHeadOprationType.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
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
                                    CreatedBy = OBJLvHead.DBTrack.CreatedBy != null ? OBJLvHead.DBTrack.CreatedBy : null,
                                    CreatedOn = OBJLvHead.DBTrack.CreatedOn != null ? OBJLvHead.DBTrack.CreatedOn : null,
                                    IsModified = OBJLvHead.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.EmpId,
                                    //AuthorizedOn = DateTime.Now
                                };
                                db.Entry(OBJLvHead).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, OBJLvHead.DBTrack);
                                DT_LvHead DT_Corp = (DT_LvHead)rtn_Obj;
                                DT_Corp.LvHeadOprationType_Id = OBJLvHead.LvHeadOprationType == null ? 0 : OBJLvHead.LvHeadOprationType.Id;
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
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                            }
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

    public static class StringHelper
    {
        public static string SafeToLower(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            return value.ToLower();
        }
    }
}
