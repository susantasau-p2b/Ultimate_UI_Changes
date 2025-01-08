using P2BUltimate.App_Start;
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
using P2BUltimate.Process;
using P2BUltimate.Security;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class BasicLinkedDAController : Controller
    {
        //   private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/BasicLinkedDA/Index.cshtml");
        }




        public ActionResult getPayscaleDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int id = int.Parse(data);
                var query = db.PayScaleAgreement.Include(e => e.PayScale).Where(e => e.PayScale.CPIAppl == true && e.Id == id).SingleOrDefault();

                var selected = query.PayScale.ActualIndexAppl;

                return Json(selected, JsonRequestBehavior.AllowGet);
            }
        }

        #region CRUD OPERATION
        #region CREATE
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(BasicLinkedDA C, FormCollection form)
        {
            List<string> Msg = new List<string>();
            try
            {

                using (DataBaseContext db = new DataBaseContext())
                {
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                           new System.TimeSpan(0, 30, 0)))
                        {
                          
                            if (C.EffectiveTo.Value < C.EffectiveFrom.Value)
                            {
                                Msg.Add("Effective To Date must be greater than Effective From Date");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            if (C.ToDate.Value < C.FromDate.Value)
                            {
                                Msg.Add(" To Date must be greater than From Date");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            if (C.EffectiveTo.Value.ToString("MM/YYYY") != C.EffectiveFrom.Value.ToString("MM/YYYY"))
                            {
                                Msg.Add("Only One Effective Month you can Define");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            if (C.ToDate.Value.ToString("MM/YYYY") != C.FromDate.Value.ToString("MM/YYYY"))
                            {
                                Msg.Add(" From Month and to Month Will same Define");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            var dbDateFrom = db.BasicLinkedDA.Where(e => e.EffectiveFrom.Value.Month == C.EffectiveFrom.Value.Month && e.EffectiveFrom.Value.Year == C.EffectiveFrom.Value.Year).FirstOrDefault();
                            if (dbDateFrom != null)
                            {
                                Msg.Add("EffectiveFrom Month Already Exists !!!");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }                         
                            var dbDateTo = db.BasicLinkedDA.Where(e => e.EffectiveTo.Value.Month == C.EffectiveTo.Value.Month && e.EffectiveTo.Value.Year == C.EffectiveTo.Value.Year).FirstOrDefault();
                            if (dbDateTo != null)
                            {
                                Msg.Add("EffectiveTo Month Already Exists !!!");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            //if (db.BasicLinkedDA.Any(o => o.EffectiveFrom = C.EffectiveFrom))
                            //{
                            //    return Json(new Object[] { "", "", "CPIEntry Already Defined For This Month.", JsonRequestBehavior.AllowGet });
                            //}
                            C.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //if (C.CalIndexPoint != 0 && C.CalIndexPoint != null)
                            //    C.ActualIndexPoint = C.CalIndexPoint * 22.7328;
                            //else
                            //    C.CalIndexPoint = C.ActualIndexPoint;

                            BasicLinkedDA basiclinkedDA = new BasicLinkedDA()
                            {
                                EffectiveFrom = C.EffectiveFrom,
                                EffectiveTo = C.EffectiveTo,
                                FromDate = C.FromDate,
                                ToDate = C.ToDate,
                                DAPoint = C.DAPoint,
                                DBTrack = C.DBTrack
                            };
                            try
                            {
                                db.BasicLinkedDA.Add(basiclinkedDA);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, C.DBTrack);
                                DT_BasicLinkedDA DT_OBJ = (DT_BasicLinkedDA)rtn_Obj;
                                db.Create(DT_OBJ);
                                db.SaveChanges();


                                //var DAList = db.EmpSalStruct.Where(e => e.EndDate != null).ToList();
                                var DAList = db.EmpSalStruct.Where(e => e.EffectiveDate >= basiclinkedDA.EffectiveFrom && e.EffectiveDate <= basiclinkedDA.EffectiveTo).ToList();
                                if (DAList != null)
                                {
                                    foreach (var Struct in DAList)
                                    {
                                        var DAListDet = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                                    .Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula))
                                    .Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula.PercentDependRule))
                                    .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType)).Where(e => e.Id == Struct.Id).SingleOrDefault();
                                        var Rule = DAListDet.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "DA" && e.SalHeadFormula != null).Select(r => r.SalHeadFormula.PercentDependRule).FirstOrDefault();
                                        if (Rule != null)
                                        {
                                            int PerId = Rule.Id;
                                            PercentDependRule PercDependRule = db.PercentDependRule
                                                   .Where(e => e.Id == PerId).SingleOrDefault();
                                            PercDependRule.SalPercent = Convert.ToDouble(basiclinkedDA.DAPoint);
                                            db.PercentDependRule.Attach(PercDependRule);
                                            db.Entry(PercDependRule).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        break;
                                    }
                                }
                                ts.Complete();
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = C.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //  return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });

                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                        return this.Json(new { msg = errorMsg });
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
        #endregion

        #region EDIT & EDIT SAVE
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.BasicLinkedDA

                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        EffectiveFrom = e.EffectiveFrom,
                        EffectiveTo = e.EffectiveTo,
                        FromDate = e.FromDate,
                        ToDate = e.ToDate,
                        DAPoint = e.DAPoint,
                        Action = e.DBTrack.Action
                    }).ToList();

                //var add_data = db.BasicLinkedDA
                //    .Include(e => e.EffectiveFrom)
                //    .Include(e => e.EffectiveTo)
                //         .Include(e => e.FromDate)
                //    .Include(e => e.ToDate)
                //        .Include(e => e.DAPoint)
                //    .Where(e => e.Id == data)
                //   .ToList();

                var Old_Data = db.DT_BasicLinkedDA
                    // .Include(e => e.VisaType)
                    //  .Include(e => e.Country)                 
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,

                         EffectiveFrom = e.EffectiveFrom,
                         EffectiveTo = e.EffectiveTo,
                         FromDate = e.FromDate,
                         ToDate = e.ToDate,
                         DAPoint = e.DAPoint,
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var EOBJ = db.BasicLinkedDA.Find(data);
                TempData["RowVersion"] = EOBJ.RowVersion;
                var Auth = EOBJ.DBTrack.IsModified;
                return Json(new Object[] { Q, Old_Data, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(BasicLinkedDA ESOBJ, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            try
            {

                using (DataBaseContext db = new DataBaseContext())
                {
                    //string VisaTypelst = form["VisaTypelist"] == "0" ? "" : form["VisaTypelist"];
                    //string Countrylst = form["CountryList_DDL"] == "0" ? "" : form["CountryList_DDL"];                
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var db_data1 = db.BasicLinkedDA.Where(e => e.Id == data).SingleOrDefault();

                    ESOBJ.EffectiveFrom = db_data1.EffectiveFrom;
                    ESOBJ.EffectiveTo = db_data1.EffectiveTo;
                    ESOBJ.ToDate = db_data1.ToDate;
                    ESOBJ.FromDate = db_data1.FromDate;
                    var paymon = ESOBJ.EffectiveFrom.Value.ToString("MM/yyyy");
                    if (db.SalaryT.Any(o => o.PayMonth == paymon && o.ReleaseDate == null))
                    {
                        Msg.Add(" You can't edit Basicliked DA as this month salary is Processed. Please delete salary and try again..");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (db.SalaryT.Any(o => o.PayMonth == paymon && o.ReleaseDate != null))
                    {
                        Msg.Add(" You can't edit Basicliked DA as this month salary is Released.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    BasicLinkedDA blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.BasicLinkedDA.Where(e => e.Id == data)
                                                               .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    ESOBJ.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    // int a = EditS(data, ESOBJ, ESOBJ.DBTrack);
                                    // Start 28022019

                                    var m1 = db.BasicLinkedDA.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.BasicLinkedDA.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }

                                    var CurCorp = db.BasicLinkedDA.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        BasicLinkedDA corp = new BasicLinkedDA()
                                        {
                                            EffectiveFrom = ESOBJ.EffectiveFrom,
                                            EffectiveTo = ESOBJ.EffectiveTo,
                                            FromDate = ESOBJ.FromDate,
                                            ToDate = ESOBJ.ToDate,
                                            DAPoint = ESOBJ.DAPoint,
                                            Id = data,
                                            DBTrack = ESOBJ.DBTrack
                                        };


                                        db.BasicLinkedDA.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                    }
                                    // End 28022019


                                    //      var DAList = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                                    //.Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula))
                                    //.Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula.PercentDependRule)).Where(e => e.EndDate != null).ToList();

                                    var DAList = db.EmpSalStruct.Where(e => e.EffectiveDate >= ESOBJ.EffectiveFrom && e.EffectiveDate <= ESOBJ.EffectiveTo).ToList();

                                    if (DAList != null)
                                    {
                                        foreach (var Struct in DAList)
                                        {
                                            var DAListDet = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                                    .Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula))
                                    .Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula.PercentDependRule))
                                    .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType)).Where(e => e.Id == Struct.Id).SingleOrDefault();
                                            var Rule = DAListDet.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "DA" && e.SalHeadFormula != null).Select(r => r.SalHeadFormula.PercentDependRule).FirstOrDefault();
                                            //var Rule = Struct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "DA").Select(r => r.SalHeadFormula.PercentDependRule).FirstOrDefault();
                                            if (Rule != null)
                                            {
                                                int PerId = Rule.Id;
                                                PercentDependRule PercDependRule = db.PercentDependRule
                                                       .Where(e => e.Id == PerId).SingleOrDefault();
                                                PercDependRule.SalPercent = Convert.ToDouble(ESOBJ.DAPoint);
                                                db.PercentDependRule.Attach(PercDependRule);
                                                db.Entry(PercDependRule).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                            }
                                            break;
                                        }
                                    }

                                    //await db.SaveChangesAsync();

                                    using (var context = new DataBaseContext())
                                    {

                                        //var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ESOBJ.DBTrack);
                                        //DT_BasicLinkedDA DT_OBJ = (DT_BasicLinkedDA)obj;

                                        //db.Create(DT_OBJ);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();


                                    //return Json(new Object[] { ESOBJ.Id, ESOBJ.DAPoint, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.DAPoint.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (BasicLinkedDA)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    //   return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (BasicLinkedDA)databaseEntry.ToObject();
                                    ESOBJ.RowVersion = databaseValues.RowVersion;

                                }
                            }

                            //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {



                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            BasicLinkedDA blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            BasicLinkedDA Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.BasicLinkedDA.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            ESOBJ.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            BasicLinkedDA corp = new BasicLinkedDA()
                            {
                                EffectiveFrom = ESOBJ.EffectiveFrom,
                                EffectiveTo = ESOBJ.EffectiveTo,
                                FromDate = ESOBJ.FromDate,
                                ToDate = ESOBJ.ToDate,
                                DAPoint = ESOBJ.DAPoint,
                                Id = data,
                                DBTrack = ESOBJ.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, corp, "VisaDetails", ESOBJ.DBTrack);
                                Old_Corp = context.BasicLinkedDA.Where(e => e.Id == data).Include(e => e.EffectiveFrom).Include(e => e.EffectiveTo).Include(e => e.FromDate)
                                    .Include(e => e.ToDate).Include(e => e.DAPoint)
                                    .SingleOrDefault();
                                DT_BasicLinkedDA DT_Corp = (DT_BasicLinkedDA)obj;
                                db.Create(DT_Corp);
                            }
                            blog.DBTrack = ESOBJ.DBTrack;
                            db.BasicLinkedDA.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            return Json(new Object[] { blog.Id, ESOBJ.Id, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.Id.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        public int EditS(int data, BasicLinkedDA ESOBJ, DBTrack dbT)
        {

            using (DataBaseContext db = new DataBaseContext())
            {

                var CurCorp = db.BasicLinkedDA.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    BasicLinkedDA corp = new BasicLinkedDA()
                    {
                        EffectiveFrom = ESOBJ.EffectiveFrom,
                        EffectiveTo = ESOBJ.EffectiveTo,
                        FromDate = ESOBJ.FromDate,
                        ToDate = ESOBJ.ToDate,
                        DAPoint = ESOBJ.DAPoint,
                        Id = data,
                        DBTrack = ESOBJ.DBTrack
                    };


                    db.BasicLinkedDA.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        #endregion

        #region DELETE

        [HttpPost]
        public ActionResult Delete(int data)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    BasicLinkedDA BasicLinkedDA = db.BasicLinkedDA
                                                       .Where(e => e.Id == data).SingleOrDefault();



                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        try
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = BasicLinkedDA.DBTrack.CreatedBy != null ? BasicLinkedDA.DBTrack.CreatedBy : null,
                                CreatedOn = BasicLinkedDA.DBTrack.CreatedOn != null ? BasicLinkedDA.DBTrack.CreatedOn : null,
                                IsModified = BasicLinkedDA.DBTrack.IsModified == true ? false : false//,

                            };



                            db.BasicLinkedDA.Where(e => e.Id == BasicLinkedDA.Id);
                            db.BasicLinkedDA = null;
                            db.Entry(BasicLinkedDA).State = System.Data.Entity.EntityState.Deleted;


                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                            Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
        #endregion

        #region AUTH SAVE
        #endregion
        #endregion



        #region P2BGRID

        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<BasicLinkedDA> BasicLinkedDA = null;
        //        if (gp.IsAutho == true)
        //        {
        //            BasicLinkedDA = db.BasicLinkedDA.Where(e => e.DBTrack.IsModified == true);
        //        }


        //        IEnumerable<BasicLinkedDA> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = BasicLinkedDA;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.EffectiveFrom, a.EffectiveTo }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "ActualIndexPoint")
        //                    jsonData = IE.Select(a => new { a.Id, a.EffectiveFrom, a.EffectiveTo }).Where((e => (e.EffectiveFrom.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "PayMonth")
        //                    jsonData = IE.Select(a => new { a.Id, a.EffectiveFrom, a.EffectiveTo }).Where((e => (e.EffectiveTo.ToString().Contains(gp.searchString)))).ToList();

        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EffectiveFrom, a.EffectiveTo }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = BasicLinkedDA;
        //            Func<BasicLinkedDA, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "EffectiveFrom" ? c.EffectiveFrom.ToString() :
        //                                 gp.sidx == "EffectiveTo" ? c.EffectiveTo.ToString() :
        //                                "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.EffectiveFrom), Convert.ToString(a.EffectiveTo)}).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.EffectiveFrom), Convert.ToString(a.EffectiveTo) }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EffectiveFrom, a.EffectiveTo}).ToList();
        //            }
        //            totalRecords = BasicLinkedDA.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
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
                IEnumerable<BasicLinkedDA> BasicLinkedDA = null;
                if (gp.IsAutho == true)
                {
                    BasicLinkedDA = db.BasicLinkedDA.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    BasicLinkedDA = db.BasicLinkedDA.AsNoTracking().ToList();
                }

                IEnumerable<BasicLinkedDA> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = BasicLinkedDA;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EffectiveFrom.Value.ToShortDateString().ToString().Contains(gp.searchString))
                            || (e.EffectiveTo.Value.ToShortDateString().ToString().Contains(gp.searchString))
                            || (e.DAPoint.ToString().Contains(gp.searchString))
                            || (e.Id.ToString().Contains(gp.searchString))

                            ).Select(a => new Object[] { a.EffectiveFrom.Value.ToShortDateString(), a.EffectiveTo.Value.ToShortDateString(),a.DAPoint.ToString(), a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }

                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EffectiveFrom, a.EffectiveTo,a.DAPoint.ToString(), a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = BasicLinkedDA;
                    Func<BasicLinkedDA, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EffectiveFrom" ? c.EffectiveFrom.Value.ToShortDateString() :
                                         gp.sidx == "EffectiveTo" ? c.EffectiveTo.Value.ToShortDateString() :
                                         gp.sidx == "DAPoint" ? c.DAPoint.ToString() :
                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EffectiveFrom.Value.ToShortDateString()), Convert.ToString(a.EffectiveTo.Value.ToShortDateString()),a.DAPoint.ToString(), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EffectiveFrom.Value.ToShortDateString()), Convert.ToString(a.EffectiveTo.Value.ToShortDateString()),a.DAPoint.ToString(), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EffectiveFrom, a.EffectiveTo,a.DAPoint.ToString(), a.Id }).ToList();
                    }
                    totalRecords = BasicLinkedDA.Count();
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

        #endregion


        public ActionResult Begin_Month(string Month, int PayScaleAgreementId) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgreementId).SingleOrDefault();


                    var EmpList = db.EmployeePayroll.Include(e => e.Employee.GeoStruct)
                                .Include(e => e.Employee.PayStruct)
                                .Include(e => e.Employee.FuncStruct).Include(e => e.EmpSalStruct)
                                .Include(e => e.Employee.EmpOffInfo)
                                .Include(e => e.Employee.EmpOffInfo.PayScale)
                                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment)))
                                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.SalHeadFormula)))//added by prashant 14042017
                                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.PayScaleAgreement)))
                                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                                .Include(e => e.CPIEntryT)
                                .ToList();

                    foreach (var a in EmpList)
                    {
                        var SalStruct = a.EmpSalStruct.Where(r => r.EndDate == null).ToList();
                        foreach (var b in SalStruct)
                        {
                            var OEmpSalStructDet = b.EmpSalStructDetails.Select(r => r.PayScaleAssignment.PayScaleAgreement.Id == PayScaleAgreementId).ToList();
                            if (OEmpSalStructDet.Count > 0)
                            {
                                Employee OEmployee = db.Employee
                                                   .Include(e => e.GeoStruct)
                                                   .Include(e => e.FuncStruct)
                                                   .Include(e => e.PayStruct)
                                                    .Where(e => e.Id == a.Employee.Id)
                                                    .SingleOrDefault();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                              new System.TimeSpan(0, 30, 0)))
                                {
                                    try
                                    {
                                        SalaryHeadGenProcess.EmployeeSalaryStructCreationWithUpdation(b.Id, OEmployee, OPayScalAgreement, Convert.ToDateTime(b.EffectiveDate), a);
                                        //   db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 14042017
                                        ts.Complete();

                                    }
                                    catch (DataException ex)
                                    {
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
                                        return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                                    }


                                }
                            }
                        }

                    }

                    return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
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