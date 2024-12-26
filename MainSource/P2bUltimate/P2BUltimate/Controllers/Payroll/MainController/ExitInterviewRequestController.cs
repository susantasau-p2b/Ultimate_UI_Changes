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
using P2BUltimate.Security;
using EMS;


namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class ExitInterviewRequestController : Controller
    {
        //
        // GET: /ExitInterviewRequest/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ExitInterviewRequest/Index.cshtml");
        }

        public ActionResult partial_LeavingReason()
        {
            return View("~/Views/Shared/Payroll/_LeavingReason.cshtml");
        }



        public ActionResult Create(ExitInterviewRequest c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string LeavingReason = form["LeavingReasonList"] == "0" ? "" : form["LeavingReasonList"];
                
               
                List<String> Msg = new List<String>();
                try
                {
                    if (LeavingReason != null)
                    {
                        if (LeavingReason != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(LeavingReason));
                            c.LeavingReason = val;
                        }
                    }

                    //c.ChecklistName = Convert.ToBoolean(ChecklistName);
                    //c.ChecklistName = Convert.ToBoolean(ChecklistName);
                    //c.ExitProcess_CheckList_Object = Convert.ToBoolean(ExitProcess_CheckList_Object);
                    ////c.IsFFSDocAppl = Convert.ToBoolean(IsFFSDocAppl);
                    //c.IsNoDuesAppl = Convert.ToBoolean(IsNoDuesAppl);
                    //c.IsNoticePeriodAppl = Convert.ToBoolean(IsNoticePeriodAppl);
                    //c.IsPartPayAppl = Convert.ToBoolean(IsPartPayAppl);
                    //c.IsRefDocAppl = Convert.ToBoolean(IsRefDocAppl);
                    //c.IsResignRequestAppl = Convert.ToBoolean(IsResignRequestAppl);
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            ExitInterviewRequest sep = new ExitInterviewRequest()
                            {
                                LeavingReason = c.LeavingReason,
                                AfterLeavingPlan = c.AfterLeavingPlan,
                                AppraisalPolicy = c.AppraisalPolicy,
                                 AttractionInNewJob = c.AttractionInNewJob,
                                DeptInfo = c.DeptInfo,
                                 DisciplinePolicy = c.DisciplinePolicy,
                               ExtraComments = c.ExtraComments,
                                 ImprovementToHoldJob = c.ImprovementToHoldJob,
                               IncrementPolicy = c.IncrementPolicy,
                                 InternalCommunication = c.InternalCommunication,
                                 JoiningDate = c.JoiningDate,
                                 JoiningPay = c.JoiningPay,
                                 JoiningRole =c.JoiningRole,
                                LastWorkDate = c.LastWorkDate,
                            LeavingPay = c. LeavingPay,
                                 LeavingRole = c.LeavingRole,
                               LifeInsurance = c.LifeInsurance,
                                 LikeLeast = c.LifeInsurance,
                                 LikeMost = c.LikeMost,
                                 MedicalDepend = c.MedicalDepend,
                                MedicalSelf = c.MedicalSelf,
                                OpportunityAbility = c.OpportunityAbility,
                              OtherRemark = c.OtherRemark,
                                 OvertimePolicy = c.OvertimePolicy,
                                PaidHoliday = c.PaidHoliday,
                                 PaidVacation = c.PaidVacation,
                                 PhysicalWorkCondition = c.PhysicalWorkCondition,
                           ProgramInfo = c.ProgramInfo,
                                 PromotionPolicy = c.PromotionPolicy,
                                 ReconsiderEmployment = c. ReconsiderEmployment,
                                ResignDate = c.ResignDate,
                                 RetirementPlan = c.RetirementPlan,
                                 SeperationType = c.SeperationType,
                              SickLeave = c.SickLeave,
                                 SupervisorManagement = c.SupervisorManagement,
                                TrainingPolicy = c.TrainingPolicy,
                                 TrainingRecd = c.TrainingRecd,
                                 TransferPolicy = c.TransferPolicy,
                                 WorkInNewJob = c.WorkInNewJob,
                               WorkRecognition = c.WorkRecognition,
                                DBTrack = c.DBTrack
                            };

                            db.ExitInterviewRequest.Add(sep);
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        //catch (DbUpdateConcurrencyException)
                        //{
                        //    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        //}


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
                        Msg.Add("Code Already Exists.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
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
                IEnumerable<ExitInterviewRequest> Corporate = null;
                if (gp.IsAutho == true)
                {
                    Corporate = db.ExitInterviewRequest.Include(e => e.LeavingReason).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Corporate = db.ExitInterviewRequest
                                .Include(e => e.LeavingReason).AsNoTracking().ToList();
                }

                IEnumerable<ExitInterviewRequest> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Corporate;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.LeavingReason != null ? e.LeavingReason.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                              || (e.LeavingPay != null ? e.LeavingPay.ToString().ToUpper().Contains(gp.searchString.ToUpper()) : false)
                            || (e.LeavingRole.ToString().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString))
                              ).Select(a => new Object[] { a.LeavingReason.LookupVal, a.LeavingPay, a.LeavingRole, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.LeavingReason, a.LeavingPay,a.LeavingRole, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Corporate;
                    Func<ExitInterviewRequest, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "LeavingReason" ? c.LeavingReason != null ? c.LeavingReason.LookupVal : "" :
                                         gp.sidx == "LeavingPay" ? c.LeavingPay.ToString():
                                         gp.sidx == "LeavingRole" ? c.LeavingRole.ToString() :
                                          "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.LeavingReason.LookupVal.ToString(), a.LeavingPay,a.LeavingRole, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.LeavingReason.LookupVal.ToString(), a.LeavingPay, a.LeavingRole, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.LeavingReason.LookupVal.ToString(), a.LeavingPay, a.LeavingRole, a.Id }).ToList();
                    }
                    totalRecords = Corporate.Count();
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


        //public ActionResult GetLookupLeavingReason(List<int> SkipIds)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.ExitInterviewRequest.Include(e => e.LeavingReason).ToList();

        //        IEnumerable<ExitInterviewRequest> all;
        //        if (SkipIds != null)
        //        {
        //            foreach (var a in SkipIds)
        //            {
        //                if (fall == null)
        //                    fall = db.ExitInterviewRequest.Include(e => e.LeavingReason)
        //                        .Where(e => e.Id != a).ToList();
        //                else
        //                    fall = fall.Where(e => e.Id != a).ToList();
        //            }
        //        }

        //        var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.LeavingReason + ",Leaving Reason:" }).Distinct();
        //        var result_1 = (from c in fall
        //                        select new { c.Id, c.LeavingReason });
        //        return Json(r, JsonRequestBehavior.AllowGet);

        //    }
        //    return View();
        //}
     
      
        
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ExitInterviewRequest

                 .Where(e => e.Id == data).Select
                 (e => new
                 {
                     LeavingReason = e.LeavingReason,
                                AfterLeavingPlan = e.AfterLeavingPlan,
                                AppraisalPolicy = e.AppraisalPolicy,
                                 AttractionInNewJob = e.AttractionInNewJob,
                                DeptInfo = e.DeptInfo,
                                 DisciplinePolicy = e.DisciplinePolicy,
                               ExtraComments = e.ExtraComments,
                                 ImprovementToHoldJob = e.ImprovementToHoldJob,
                               IncrementPolicy = e.IncrementPolicy,
                                 InternalCommunication = e.InternalCommunication,
                                 JoiningDate = e.JoiningDate,
                                 JoiningPay = e.JoiningPay,
                                 JoiningRole =e.JoiningRole,
                                LastWorkDate = e.LastWorkDate,
                            LeavingPay = e. LeavingPay,
                                 LeavingRole = e.LeavingRole,
                               LifeInsurance = e.LifeInsurance,
                                 LikeLeast = e.LifeInsurance,
                                 LikeMost = e.LikeMost,
                                 MedicalDepend = e.MedicalDepend,
                                MedicalSelf = e.MedicalSelf,
                                OpportunityAbility = e.OpportunityAbility,
                              OtherRemark = e.OtherRemark,
                                 OvertimePolicy = e.OvertimePolicy,
                                PaidHoliday = e.PaidHoliday,
                                 PaidVacation = e.PaidVacation,
                                 PhysicalWorkCondition = e.PhysicalWorkCondition,
                           ProgramInfo = e.ProgramInfo,
                                 PromotionPolicy = e.PromotionPolicy,
                                 ReconsiderEmployment = e. ReconsiderEmployment,
                                ResignDate = e.ResignDate,
                                 RetirementPlan = e.RetirementPlan,
                                 SeperationType = e.SeperationType,
                              SickLeave = e.SickLeave,
                                 SupervisorManagement = e.SupervisorManagement,
                                TrainingPolicy = e.TrainingPolicy,
                                 TrainingRecd = e.TrainingRecd,
                                 TransferPolicy = e.TransferPolicy,
                                 WorkInNewJob = e.WorkInNewJob,
                               WorkRecognition = e.WorkRecognition,

                 }).ToList();

                var Corp = db.ExitInterviewRequest.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, Auth, JsonRequestBehavior.AllowGet });

            }

        }



         



        

        [HttpPost]
        public async Task<ActionResult> EditSave(ExitInterviewRequest c, int data, FormCollection form)// Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //string typeofseparation = form["TypeOfSeperationList"] == "0" ? "" : form["TypeOfSeperationList"];
                    //string subtypeofseparation = form["SubTypeOfSeperation"] == "0" ? "" : form["SubTypeOfSeperation"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (Auth == false)
                    {

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    ExitInterviewRequest blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ExitInterviewRequest.Where(e => e.Id == data)
                                                                .AsNoTracking().SingleOrDefault();
                                        //originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    var m1 = db.ExitInterviewRequest.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ExitInterviewRequest.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    var CurCorp = db.ExitInterviewRequest.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {


                                        ExitInterviewRequest corp = new ExitInterviewRequest()
                                        {
                                 LeavingReason = c.LeavingReason,
                                AfterLeavingPlan = c.AfterLeavingPlan,
                                AppraisalPolicy = c.AppraisalPolicy,
                                 AttractionInNewJob = c.AttractionInNewJob,
                                DeptInfo = c.DeptInfo,
                                 DisciplinePolicy = c.DisciplinePolicy,
                               ExtraComments = c.ExtraComments,
                                 ImprovementToHoldJob = c.ImprovementToHoldJob,
                               IncrementPolicy = c.IncrementPolicy,
                                 InternalCommunication = c.InternalCommunication,
                                 JoiningDate = c.JoiningDate,
                                 JoiningPay = c.JoiningPay,
                                 JoiningRole =c.JoiningRole,
                                LastWorkDate = c.LastWorkDate,
                            LeavingPay = c. LeavingPay,
                                 LeavingRole = c.LeavingRole,
                               LifeInsurance = c.LifeInsurance,
                                 LikeLeast = c.LifeInsurance,
                                 LikeMost = c.LikeMost,
                                 MedicalDepend = c.MedicalDepend,
                                MedicalSelf = c.MedicalSelf,
                                OpportunityAbility = c.OpportunityAbility,
                              OtherRemark = c.OtherRemark,
                                 OvertimePolicy = c.OvertimePolicy,
                                PaidHoliday = c.PaidHoliday,
                                 PaidVacation = c.PaidVacation,
                                 PhysicalWorkCondition = c.PhysicalWorkCondition,
                           ProgramInfo = c.ProgramInfo,
                                 PromotionPolicy = c.PromotionPolicy,
                                 ReconsiderEmployment = c. ReconsiderEmployment,
                                ResignDate = c.ResignDate,
                                 RetirementPlan = c.RetirementPlan,
                                 SeperationType = c.SeperationType,
                              SickLeave = c.SickLeave,
                                 SupervisorManagement = c.SupervisorManagement,
                                TrainingPolicy = c.TrainingPolicy,
                                 TrainingRecd = c.TrainingRecd,
                                 TransferPolicy = c.TransferPolicy,
                                 WorkInNewJob = c.WorkInNewJob,
                               WorkRecognition = c.WorkRecognition,
                                       Id = data,
                                       DBTrack = c.DBTrack
                                        };

                                        db.ExitInterviewRequest.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;
                                    }

                                    using (var context = new DataBaseContext())
                                    {
                                        //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //DT_ExitProcess_Process_Policy DT_Corp = (DT_IncrActivity)obj;
                                        //DT_Corp.IncrList_Id = blog.IncrList == null ? 0 : blog.IncrList.Id;
                                        //DT_Corp.IncrPolicy_Id = blog.IncrPolicy == null ? 0 : blog.IncrPolicy.Id;

                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass
                                    {
                                        Id = c.Id,
                                        Val = c.LeavingReason != null ?  c.LeavingReason.LookupVal.ToString() : "",
                                        success = true,
                                        responseText = Msg
                                    }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (PromoActivity)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PromoActivity)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    ExitInterviewRequest ExitInterviewRequest = db.ExitInterviewRequest

                                                       .Where(e => e.Id == data).SingleOrDefault();

                    db.Entry(ExitInterviewRequest).State = System.Data.Entity.EntityState.Deleted;
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
    }
}