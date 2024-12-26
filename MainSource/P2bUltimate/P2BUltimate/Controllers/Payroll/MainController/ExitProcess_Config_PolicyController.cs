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
using EMS;
using Training;
using Attendance;
using Leave;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers
{
    public class ExitProcess_Config_PolicyController : Controller
    {
        //
        // GET: /ExitProcess_Config_Policy/ 
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ExitProcess_Config_Policy/Index.cshtml");
        }
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        public ActionResult Create(ExitProcess_Config_Policy c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var LeaveHeadlist = form["LeaveHeadlist"] == "0" ? "" : form["LeaveHeadlist"];
                List<LvHead> ObjLvheadCLHB = new List<LvHead>();
                if (LeaveHeadlist != null && LeaveHeadlist != " ")
                {
                    var ids = one_ids(LeaveHeadlist);
                    foreach (var ca in ids)
                    {
                        var value = db.LvHead.Find(ca);
                        ObjLvheadCLHB.Add(value);
                        c.LeaveHead = ObjLvheadCLHB;
                    }

                }
                //string IsFFSStatementAcceptanceAppl = form["IsFFSStatementAcceptanceAppl"] == "0" ? "" : form["IsFFSStatementAcceptanceAppl"];
                //string IsLeavesAvailAppl = form["IsLeavesAvailAppl"] == "0" ? "" : form["IsLeavesAvailAppl"];
                //string IsSalaryStopOnResignDate = form["IsSalaryStopOnResignDate"] == "0" ? "" : form["IsSalaryStopOnResignDate"];
                //string IsSettlementOnLastWorkingDay = form["IsSettlementOnLastWorkingDay"] == "0" ? "" : form["IsSettlementOnLastWorkingDay"];
                //string IsLastDayAsNoticePeriodLastWorkDay = form["IsLastDayAsNoticePeriodLastWorkDay"] == "0" ? "" : form["IsLastDayAsNoticePeriodLastWorkDay"];
                //string IsLastWorkDateManual = form["IsLastWorkDateManual"] == "0" ? "" : form["IsLastWorkDateManual"];
                //string IsExitInterviewOnReqAcceptance = form["IsExitInterviewOnReqAcceptance"] == "0" ? "" : form["IsExitInterviewOnReqAcceptance"];
                //string NoDuesActivation_BFLastWorkDay = form["NoDuesActivation_BFLastWorkDay"] == "0" ? "" : form["NoDuesActivation_BFLastWorkDay"];
                //string FFSSettlementPeriod_FromLastWorkDay = form["FFSSettlementPeriod_FromLastWorkDay"] == "0" ? "" : form["FFSSettlementPeriod_FromLastWorkDay"];
                List<String> Msg = new List<String>();
                try
                {

                    //c.IsExitInterviewOnReqAcceptance = Convert.ToBoolean(IsExitInterviewOnReqAcceptance);
                    //c.IsFFSStatementAcceptanceAppl = Convert.ToBoolean(IsFFSStatementAcceptanceAppl);
                    //c.IsLeavesAvailAppl = Convert.ToBoolean(IsLeavesAvailAppl);
                    //c.IsSalaryStopOnResignDate = Convert.ToBoolean(IsSalaryStopOnResignDate);
                    //c.IsSettlementOnLastWorkingDay = Convert.ToBoolean(IsSettlementOnLastWorkingDay);
                    //c.IsLastDayAsNoticePeriodLastWorkDay = Convert.ToBoolean(IsLastDayAsNoticePeriodLastWorkDay);
                    //c.IsLastWorkDateManual = Convert.ToBoolean(IsLastWorkDateManual);
                    //c.NoDuesActivation_BFLastWorkDay = Convert.ToInt32(NoDuesActivation_BFLastWorkDay);
                    //c.FFSSettlementPeriod_FromLastWorkDay = Convert.ToInt32(FFSSettlementPeriod_FromLastWorkDay);
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            ExitProcess_Config_Policy sep = new ExitProcess_Config_Policy()
                            {
                                IsExitInterviewOnReqAcceptance = c.IsExitInterviewOnReqAcceptance,
                                IsFFSStatementAcceptanceAppl = c.IsFFSStatementAcceptanceAppl,
                                IsLeavesAvailAppl = c.IsLeavesAvailAppl,
                                IsSalaryStopOnResignDate = c.IsSalaryStopOnResignDate,
                                IsSettlementOnLastWorkingDay = c.IsSettlementOnLastWorkingDay,
                                IsLastDayAsNoticePeriodLastWorkDay = c.IsLastDayAsNoticePeriodLastWorkDay,
                                IsLastWorkDateManual = c.IsLastWorkDateManual,
                                ProcessConfigName = c.ProcessConfigName,
                                FFSSettlementPeriod_FromLastWorkDay = c.FFSSettlementPeriod_FromLastWorkDay,
                                NoDuesActivation_BFLastWorkDay =c.NoDuesActivation_BFLastWorkDay,
                                LeaveHead=c.LeaveHead,
                                DBTrack = c.DBTrack
                            };

                            db.ExitProcess_Config_Policy.Add(sep);
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
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


        public class ExitProcess_Config_Policy_Val
        {
            public Array LeaveHead_Id { get; set; }
            public Array LeaveHead_FullDetails { get; set; }
        }


        public ActionResult GetExitProcessConfigPolicyDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.ExitProcess_Config_Policy.Include(e => e.LeaveHead).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ExitProcess_Config_Policy.Include(e => e.LeaveHead).Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "Process Config Name:" + ca.ProcessConfigName }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }


        public class LvCreditPolicyEditDetails
        {
            public Array LvHeadObjCLH_Id { get; set; }

            public Array LvHeadObjCLH_FullDetails { get; set; }

            public Array LvHeadObjCLHB_Id { get; set; }

            public Array LvHeadObjCLHB_FullDetails { get; set; }

            public Array LvHeadObjELH_Id { get; set; }

            public Array LvHeadObjELH_FullDetails { get; set; }
        }


        public ActionResult Edit(int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ExitProcess_Config_Policy
                    .Include(e => e.LeaveHead)                   
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        IsExitInterviewOnReqAcceptance = e.IsExitInterviewOnReqAcceptance,
                        IsFFSStatementAcceptanceAppl = e.IsFFSStatementAcceptanceAppl,
                        IsLeavesAvailAppl = e.IsLeavesAvailAppl,
                        IsSalaryStopOnResignDate = e.IsSalaryStopOnResignDate,
                        IsSettlementOnLastWorkingDay = e.IsSettlementOnLastWorkingDay,
                        IsLastDayAsNoticePeriodLastWorkDay = e.IsLastDayAsNoticePeriodLastWorkDay,
                        IsLastWorkDateManual = e.IsLastWorkDateManual,
                        FFSSettlementPeriod_FromLastWorkDay = e.FFSSettlementPeriod_FromLastWorkDay,
                        NoDuesActivation_BFLastWorkDay = e.NoDuesActivation_BFLastWorkDay,
                        ProcessConfigName = e.ProcessConfigName,
                        
                        Action = e.DBTrack.Action
                    }).ToList();

                List<LvCreditPolicyEditDetails> LvHeadObj = new List<LvCreditPolicyEditDetails>();

                var CLH = db.ExitProcess_Config_Policy.Include(e => e.LeaveHead).Where(e => e.Id == data).Select(e => e.LeaveHead).ToList();
                if (CLH != null)
                {
                    foreach (var ca in CLH)
                    {
                        LvHeadObj.Add(new LvCreditPolicyEditDetails
                        {
                            LvHeadObjCLH_Id = ca.Select(e => e.Id).ToArray(),
                            LvHeadObjCLH_FullDetails = ca.Select(e => e.FullDetails).ToArray()

                        });


                    }

                }

                var ExitProcess_Config_Policy = db.ExitProcess_Config_Policy.Find(data);
                TempData["RowVersion"] = ExitProcess_Config_Policy.RowVersion;
                var Auth = ExitProcess_Config_Policy.DBTrack.IsModified;
                return Json(new Object[] { Q,LvHeadObj, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        public async Task<ActionResult> EditSave(ExitProcess_Config_Policy c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //string typeofseparation = form["TypeOfSeperationList"] == "0" ? "" : form["TypeOfSeperationList"];
                    //string subtypeofseparation = form["SubTypeOfSeperation"] == "0" ? "" : form["SubTypeOfSeperation"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    string LeaveHeadlist = form["LeaveHeadlist"] == "0" ? "" : form["LeaveHeadlist"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (Auth == false)
                    {

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    ExitProcess_Config_Policy blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ExitProcess_Config_Policy.Where(e => e.Id == data)                                          
                                                                .Include(e => e.LeaveHead).AsNoTracking().SingleOrDefault();
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
                                    var m1 = db.ExitProcess_Config_Policy.Include(e => e.LeaveHead)                                         
                                         .Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ExitProcess_Config_Policy.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }

                                    List<LvHead> LvHeadObjELH = new List<LvHead>();
                                    ExitProcess_Config_Policy lvheaddetails = null;
                                    lvheaddetails = db.ExitProcess_Config_Policy.Include(e => e.LeaveHead).Where(e => e.Id == data).SingleOrDefault();
                                    if (LeaveHeadlist != null && LeaveHeadlist != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(LeaveHeadlist);
                                        foreach (var ca in ids)
                                        {
                                            var ExcludeLeaveHeadslistvalue = db.LvHead.Find(ca);
                                            LvHeadObjELH.Add(ExcludeLeaveHeadslistvalue);
                                            lvheaddetails.LeaveHead = LvHeadObjELH;
                                        }
                                    }
                                    else
                                    {
                                        lvheaddetails.LeaveHead = null;
                                    }

                                    db.ExitProcess_Config_Policy.Attach(lvheaddetails);
                                    db.Entry(lvheaddetails).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = lvheaddetails.RowVersion;
                                    db.Entry(lvheaddetails).State = System.Data.Entity.EntityState.Detached;

                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    var CurCorp = db.ExitProcess_Config_Policy.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {


                                        ExitProcess_Config_Policy corp = new ExitProcess_Config_Policy()
                                        {
                                            IsExitInterviewOnReqAcceptance = c.IsExitInterviewOnReqAcceptance,
                                            IsFFSStatementAcceptanceAppl = c.IsFFSStatementAcceptanceAppl,
                                            IsLeavesAvailAppl = c.IsLeavesAvailAppl,
                                            IsSalaryStopOnResignDate = c.IsSalaryStopOnResignDate,
                                            IsSettlementOnLastWorkingDay = c.IsSettlementOnLastWorkingDay,
                                            IsLastDayAsNoticePeriodLastWorkDay = c.IsLastDayAsNoticePeriodLastWorkDay,
                                            IsLastWorkDateManual = c.IsLastWorkDateManual,
                                            ProcessConfigName = c.ProcessConfigName,
                                            FFSSettlementPeriod_FromLastWorkDay = c.FFSSettlementPeriod_FromLastWorkDay,
                                            NoDuesActivation_BFLastWorkDay = c.NoDuesActivation_BFLastWorkDay,
                                            Id = data,
                                            DBTrack = c.DBTrack
                                        };

                                        db.ExitProcess_Config_Policy.Attach(corp);
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
                                        Val = c.ProcessConfigName,
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


        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    ExitProcess_Config_Policy ExitProcess_Config_Policy = db.ExitProcess_Config_Policy
                                                        .Where(e => e.Id == data)
                                                       .Include(e => e.LeaveHead).AsNoTracking().SingleOrDefault();

                    db.Entry(ExitProcess_Config_Policy).State = System.Data.Entity.EntityState.Deleted;
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
                var jsonData = (Object)null;
                IEnumerable<ExitProcess_Config_Policy> Corporate = null;
                if (gp.IsAutho == true)
                {
                    Corporate = db.ExitProcess_Config_Policy.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Corporate = db.ExitProcess_Config_Policy.AsNoTracking().ToList();
                }

                IEnumerable<ExitProcess_Config_Policy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Corporate;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.FFSSettlementPeriod_FromLastWorkDay.ToString().Contains(gp.searchString))
                              || (e.NoDuesActivation_BFLastWorkDay.ToString().Contains(gp.searchString))
                              || (e.ProcessConfigName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString))
                              ).Select(a => new Object[] { a.FFSSettlementPeriod_FromLastWorkDay.ToString(), a.NoDuesActivation_BFLastWorkDay.ToString(), a.ProcessConfigName, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.FFSSettlementPeriod_FromLastWorkDay.ToString(), a.NoDuesActivation_BFLastWorkDay.ToString(), a.ProcessConfigName, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Corporate;
                    Func<ExitProcess_Config_Policy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "FFSSettlementPeriod_FromLastWorkDay" ? c.FFSSettlementPeriod_FromLastWorkDay.ToString() :
                                         gp.sidx == "NoDuesActivation_BFLastWorkDay" ? c.NoDuesActivation_BFLastWorkDay.ToString() :
                                         gp.sidx == "ProcessConfigName" ? c.ProcessConfigName : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.FFSSettlementPeriod_FromLastWorkDay.ToString(), a.NoDuesActivation_BFLastWorkDay.ToString(), a.ProcessConfigName, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.FFSSettlementPeriod_FromLastWorkDay.ToString(), a.NoDuesActivation_BFLastWorkDay.ToString(), a.ProcessConfigName, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.FFSSettlementPeriod_FromLastWorkDay.ToString(), a.NoDuesActivation_BFLastWorkDay.ToString(), a.ProcessConfigName, a.Id }).ToList();
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
    }
}