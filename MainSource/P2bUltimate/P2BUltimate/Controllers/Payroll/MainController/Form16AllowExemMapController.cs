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
using P2BUltimate.Security;
using Payroll;

namespace P2BUltimate.Controllers.Leave.MainController
{
    [AuthoriseManger]
    public class Form16AllowExemMapController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/Form16AllowExemMap/Index.cshtml");
        }
       // DataBaseContext db = new DataBaseContext();

        public ActionResult GetLookupValue(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                    // var qurey = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == data).SingleOrDefault();
                    var qurey = db.SalaryHead.Include(q => q.Type).Where(e => e.Type.LookupVal == "Earning").ToList(); // added by rekha 26-12-16
                    if (data2 != "" && data2 != "0")
                    {
                        selected = data2;
                    }
                    if (qurey != null)
                    {
                        s = new SelectList(qurey, "Id", "Code", selected);
                    }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

         [HttpPost]
        public ActionResult Create(Form16AllowExemMap L, FormCollection form)
        {
               List<string> Msg = new List<string>();
					try{

            using (DataBaseContext db = new DataBaseContext())
            {
                //string Category = form["AllowExemptionName"] == "0" ? "" : form["AllowExemptionName"];
                //if (Category != null && Category != "")
                //    {
                //        int idd = int.Parse(Category);
                //        var val = db.SalaryHead.Where(e => e.Id == idd).SingleOrDefault();
                //        L.AllowExemptionName = val.Code.ToString();
                //    }

                //if (db.Form16AllowExemMap.Any(e => e.ITSection10ExemCode.Replace(" ", String.Empty) == L.ITSection10ExemCode.Replace(" ", String.Empty)))
                //{
                //    Msg.Add(" Code Already Exist ");
                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //}

                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        Form16AllowExemMap OBJLVH = new Form16AllowExemMap()
                        {
                            AllowExemptionName = L.AllowExemptionName,
                            ITSection10ExemCode = L.ITSection10ExemCode,
                           // DBTrack = L.DBTrack
                        };
                        try
                        {
                            db.Form16AllowExemMap.Add(OBJLVH);
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, L.DBTrack);
                            //DT_Form16AllowExemMap DT_OBJ = (DT_Form16AllowExemMap)rtn_Obj;
                            //db.Create(DT_OBJ);
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data Created successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = OBJLVH.Id, Val = "", success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                    // return this.Json(new { msg = errorMsg });
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
         [HttpPost]
         //public ActionResult Edit(int data)
         //{
         //    using (DataBaseContext db = new DataBaseContext())
         //    {
         //        var Q = db.Form16AllowExemMap
         //            .Where(e => e.Id == data).Select
         //            (e => new
         //            {
         //                ApplAtt = e.ApplAtt,
         //                EncashRegular = e.EncashRegular,
         //                EncashRetirement = e.EncashRetirement,
         //                ESS = e.ESS,
         //                HFPay = e.HFPay,
         //                LTAAppl = e.LTAAppl,
         //                LvCode = e.LvCode,
         //                Form16AllowExemMapAlias = e.Form16AllowExemMapAlias,
         //                Form16AllowExemMapOprationType_Id = e.Form16AllowExemMapOprationType.Id == null ? 0 : e.Form16AllowExemMapOprationType.Id,
         //                LvName = e.LvName,
         //                Action = e.DBTrack.Action
         //            }).ToList();

         //        var W = db.DT_Form16AllowExemMap
         //             .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
         //             (e => new
         //             {
         //                 DT_Id = e.Id,
         //                 Form16AllowExemMapOprationType_Id = e.Form16AllowExemMapOprationType_Id == null ? 0 : e.Form16AllowExemMapOprationType_Id,


         //             }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

         //        var Corp = db.Form16AllowExemMap.Find(data);
         //        TempData["RowVersion"] = Corp.RowVersion;
         //        var Auth = Corp.DBTrack.IsModified;
         //        return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
         //    }
         //}
       
         public ActionResult Edit(int data)
         {
             using (DataBaseContext db = new DataBaseContext())
             {
                 var returndata = db.Form16AllowExemMap.Where(e => e.Id == data).AsEnumerable()
                     .Select(e => new
                     {

                         AllowExemptionName = e.AllowExemptionName,
                         ITSection10ExemCode=e.ITSection10ExemCode
                     }).ToList();
                 return Json(new Object[] { returndata, "", "", "", JsonRequestBehavior.AllowGet });
             }
         }
         public async Task<ActionResult> EditSave(Form16AllowExemMap c, int data, FormCollection form)
         {
             List<string> Msg = new List<string>();
             
             using (DataBaseContext db = new DataBaseContext())
             {
                 try
                 {
                     using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                     {
                         var db_data = db.Form16AllowExemMap.Where(e => e.Id == data).SingleOrDefault();

                         db.Form16AllowExemMap.Attach(db_data);
                         db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                         db.SaveChanges();

                         Form16AllowExemMap form16allowexemap = db.Form16AllowExemMap.Find(data);
                   
                         if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                         {
                             Form16AllowExemMap blog = null; // to retrieve old data
                             DbPropertyValues originalBlogValues = null;

                             form16allowexemap.Id = data;
                             form16allowexemap.ITSection10ExemCode = c.ITSection10ExemCode;
                             form16allowexemap.AllowExemptionName = c.AllowExemptionName;

                             db.Entry(form16allowexemap).State = System.Data.Entity.EntityState.Modified;


                             //using (var context = new DataBaseContext())
                             //{


                             blog = db.Form16AllowExemMap.Where(e => e.Id == data)
                                                     .SingleOrDefault();
                             originalBlogValues = db.Entry(blog).OriginalValues;
                             db.ChangeTracker.DetectChanges();
                            
                             db.SaveChanges();
                         }
                         //}
                         ts.Complete();
                     }
                     Msg.Add("  Record Updated");
                     return Json(new Utility.JsonReturnClass { Id = c.Id, Val =c.AllowExemptionName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        //[HttpPost]
        //public async Task<ActionResult> EditSave(Form16AllowExemMap L, int data, FormCollection form) // Edit submit
        //{
        //       List<string> Msg = new List<string>();
        //       try { 
        //    var Form16AllowExemMapOprationTypelist = form["Form16AllowExemMapOprationTypelist"] == "0" ? "" : form["Form16AllowExemMapOprationTypelist"];

        //    var HFPay = form["HFPay"] == "0" ? "" : form["HFPay"];
        //    var LTAAppl = form["LTAAppl"] == "0" ? "" : form["LTAAppl"];
        //    var ApplAtt = form["ApplAtt"] == "0" ? "" : form["ApplAtt"];
        //    var EncashRegular = form["EncashRegular"] == "0" ? "" : form["EncashRegular"];
        //    var EncashRetirement = form["EncashRetirement"] == "0" ? "" : form["EncashRetirement"];
        //    var ESS = form["ESS"] == "0" ? "" : form["ESS"];

        //    L.HFPay = Convert.ToBoolean(HFPay);
        //    L.LTAAppl = Convert.ToBoolean(LTAAppl);
        //    L.ApplAtt = Convert.ToBoolean(ApplAtt);
        //    L.EncashRegular = Convert.ToBoolean(EncashRegular);
        //    L.EncashRetirement = Convert.ToBoolean(EncashRetirement);
        //    L.ESS = Convert.ToBoolean(ESS);

        //    bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        if (Form16AllowExemMapOprationTypelist != null && Form16AllowExemMapOprationTypelist != "" && Form16AllowExemMapOprationTypelist != "-Select-")
        //        {
        //            var value = db.LookupValue.Find(int.Parse(Form16AllowExemMapOprationTypelist));
        //            L.Form16AllowExemMapOprationType = value;
        //        }

        //        if (Auth == false)
        //        {


        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {

        //                    //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        Form16AllowExemMap blog = null; // to retrieve old data
        //                        DbPropertyValues originalBlogValues = null;

        //                        using (var context = new DataBaseContext())
        //                        {
        //                            blog = context.Form16AllowExemMap.Where(e => e.Id == data).Include(e => e.Form16AllowExemMapOprationType)
        //                                                    .SingleOrDefault();
        //                            originalBlogValues = context.Entry(blog).OriginalValues;
        //                        }

        //                        L.DBTrack = new DBTrack
        //                        {
        //                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                            Action = "M",
        //                            ModifiedBy = SessionManager.UserName,
        //                            ModifiedOn = DateTime.Now
        //                        };

        //                        if (Form16AllowExemMapOprationTypelist != null)
        //                        {
        //                            if (Form16AllowExemMapOprationTypelist != "")
        //                            {
        //                                var val = db.LookupValue.Find(int.Parse(Form16AllowExemMapOprationTypelist));
        //                                L.Form16AllowExemMapOprationType = val;

        //                                var type = db.Form16AllowExemMap.Include(e => e.Form16AllowExemMapOprationType).Where(e => e.Id == data).SingleOrDefault();
        //                                IList<Form16AllowExemMap> typedetails = null;
        //                                if (type.Form16AllowExemMapOprationType != null)
        //                                {
        //                                    typedetails = db.Form16AllowExemMap.Where(x => x.Form16AllowExemMapOprationType.Id == type.Form16AllowExemMapOprationType.Id && x.Id == data).ToList();
        //                                }
        //                                else
        //                                {
        //                                    typedetails = db.Form16AllowExemMap.Where(x => x.Id == data).ToList();
        //                                }
        //                                //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                                foreach (var s in typedetails)
        //                                {
        //                                    s.Form16AllowExemMapOprationType = L.Form16AllowExemMapOprationType;
        //                                    db.Form16AllowExemMap.Attach(s);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                var Form16AllowExemMapOprationTypeDetails = db.Form16AllowExemMap.Include(e => e.Form16AllowExemMapOprationType).Where(x => x.Id == data).ToList();
        //                                foreach (var s in Form16AllowExemMapOprationTypeDetails)
        //                                {
        //                                    s.Form16AllowExemMapOprationType = null;
        //                                    db.Form16AllowExemMap.Attach(s);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            var Form16AllowExemMapOprationTypeDetails = db.Form16AllowExemMap.Include(e => e.Form16AllowExemMapOprationType).Where(x => x.Id == data).ToList();
        //                            foreach (var s in Form16AllowExemMapOprationTypeDetails)
        //                            {
        //                                s.Form16AllowExemMapOprationType = null;
        //                                db.Form16AllowExemMap.Attach(s);
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                //await db.SaveChangesAsync();
        //                                db.SaveChanges();
        //                                TempData["RowVersion"] = s.RowVersion;
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                            }
        //                        }

        //                        var CurCorp = db.Form16AllowExemMap.Find(data);
        //                        TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        {

        //                            Form16AllowExemMap Form16AllowExemMap = new Form16AllowExemMap()
        //                            {
        //                                ApplAtt = L.ApplAtt,
        //                                EncashRegular = L.EncashRegular,
        //                                EncashRetirement = L.EncashRetirement,
        //                                ESS = L.ESS,
        //                                HFPay = L.HFPay,
        //                                LTAAppl = L.LTAAppl,
        //                                LvCode = L.LvCode,
        //                                Form16AllowExemMapAlias = L.Form16AllowExemMapAlias,
        //                                Form16AllowExemMapOprationType = L.Form16AllowExemMapOprationType,
        //                                LvName = L.LvName,
        //                                Id = data,
        //                                DBTrack = L.DBTrack
        //                            };
        //                            db.Form16AllowExemMap.Attach(Form16AllowExemMap);
        //                            db.Entry(Form16AllowExemMap).State = System.Data.Entity.EntityState.Modified;
        //                            db.Entry(Form16AllowExemMap).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                        }
        //                        // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
        //                        using (var context = new DataBaseContext())
        //                        {
        //                            var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, L.DBTrack);
        //                            DT_Form16AllowExemMap DT_Corp = (DT_Form16AllowExemMap)obj;
        //                            DT_Corp.Form16AllowExemMapOprationType_Id = blog.Form16AllowExemMapOprationType == null ? 0 : blog.Form16AllowExemMapOprationType.Id;
        //                            db.Create(DT_Corp);
        //                            db.SaveChanges();
        //                        }
        //                        await db.SaveChangesAsync();
        //                        ts.Complete();
        //                        //return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
        //                         Msg.Add("  Record Updated");
        //            return Json(new Utility.JsonReturnClass { Id = L .Id   , Val =  L.FullDetails , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }

        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (LvCreditPolicy)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (LvEncashReq)databaseEntry.ToObject();
        //                        L.RowVersion = databaseValues.RowVersion;

        //                    }
        //                }
        //                Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //        else
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {

        //                Form16AllowExemMap blog = null; // to retrieve old data
        //                DbPropertyValues originalBlogValues = null;
        //                Form16AllowExemMap Old_Corp = null;

        //                using (var context = new DataBaseContext())
        //                {
        //                    blog = context.Form16AllowExemMap.Where(e => e.Id == data).SingleOrDefault();
        //                    originalBlogValues = context.Entry(blog).OriginalValues;
        //                }
        //                L.DBTrack = new DBTrack
        //                {
        //                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                    Action = "M",
        //                    IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                    ModifiedBy = SessionManager.UserName,
        //                    ModifiedOn = DateTime.Now
        //                };

        //                if (TempData["RowVersion"] == null)
        //                {
        //                    TempData["RowVersion"] = blog.RowVersion;
        //                }

        //                Form16AllowExemMap corp = new Form16AllowExemMap()
        //                {
        //                    ApplAtt = L.ApplAtt,
        //                    EncashRegular = L.EncashRegular,
        //                    EncashRetirement = L.EncashRetirement,
        //                    ESS = L.ESS,
        //                    HFPay = L.HFPay,
        //                    LTAAppl = L.LTAAppl,
        //                    LvCode = L.LvCode,
        //                    Form16AllowExemMapAlias = L.Form16AllowExemMapAlias,
        //                    Form16AllowExemMapOprationType = L.Form16AllowExemMapOprationType,
        //                    LvName = L.LvName,
        //                    Id = data,
        //                    DBTrack = L.DBTrack,
        //                    RowVersion = (Byte[])TempData["RowVersion"]
        //                };


        //                //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                using (var context = new DataBaseContext())
        //                {
        //                    var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvEncashReq", L.DBTrack);
        //                    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                    Old_Corp = context.Form16AllowExemMap.Where(e => e.Id == data).Include(e => e.Form16AllowExemMapOprationType)
        //                                            .SingleOrDefault();
        //                    DT_Form16AllowExemMap DT_Corp = (DT_Form16AllowExemMap)obj;
        //                    DT_Corp.Form16AllowExemMapOprationType_Id = blog.Form16AllowExemMapOprationType == null ? 0 : blog.Form16AllowExemMapOprationType.Id;

        //                    db.Create(DT_Corp);
        //                    //db.SaveChanges();
        //                }
        //                blog.DBTrack = L.DBTrack;
        //                db.Form16AllowExemMap.Attach(blog);
        //                db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                db.SaveChanges();
        //                ts.Complete();
        //                Msg.Add("  Record Updated");
        //            return Json(new Utility.JsonReturnClass { Id =  blog.Id   , Val =  L.FullDetails , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
        //            }

        //        }

        //        return View();
        //    }
        //       }
        //       catch (Exception ex)
        //       {
        //           LogFile Logfile = new LogFile();
        //           ErrorLog Err = new ErrorLog()
        //           {
        //               ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //               ExceptionMessage = ex.Message,
        //               ExceptionStackTrace = ex.StackTrace,
        //               LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //               LogTime = DateTime.Now
        //           };
        //           Logfile.CreateLogFile(Err);
        //           Msg.Add(ex.Message);
        //           return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //       } 

        //}


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
                IEnumerable<Form16AllowExemMap> lencash = null;
                
                    lencash = db.Form16AllowExemMap.AsNoTracking().ToList();

                IEnumerable<Form16AllowExemMap> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = lencash;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.ITSection10ExemCode.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                             || (e.AllowExemptionName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                             || (e.Id.ToString().Contains(gp.searchString))
                             ).Select(a => new Object[] { a.ITSection10ExemCode, a.AllowExemptionName, a.Id }).ToList();

                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.ITSection10ExemCode), Convert.ToString(a.AllowExemptionName), a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = lencash;
                    Func<Form16AllowExemMap, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                            gp.sidx == "ITSection10ExemCode" ? c.ITSection10ExemCode.ToString() :
                            gp.sidx == "AllowExemptionName" ? c.AllowExemptionName.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.ITSection10ExemCode), Convert.ToString(a.AllowExemptionName), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.ITSection10ExemCode), Convert.ToString(a.AllowExemptionName), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.ITSection10ExemCode), Convert.ToString(a.AllowExemptionName), a.Id }).ToList();
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
        public ActionResult delete(string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var id = Convert.ToInt32(data);
                    var qurey = db.Form16AllowExemMap.Where(e => e.Id == id).SingleOrDefault();
                    db.Form16AllowExemMap.Attach(qurey);
                    db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                    db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                    Msg.Add("  Record removed successfully.  ");
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
                Msg.Add("  Data removed successfully.  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                // return Json(new { msg = "Record Deleted", JsonRequestBehavior.AllowGet });
            }
        }

    }
    
}
