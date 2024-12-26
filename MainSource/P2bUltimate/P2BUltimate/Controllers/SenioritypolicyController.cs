using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2b.Global;
using P2BUltimate.Models;
using System.Transactions;
using P2BUltimate.Security;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;


namespace P2BUltimate.Controllers
{
    public class SenioritypolicyController : Controller
    {
        //
        // GET: /Senioritypolicy/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult JobStatusDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                var fall = db.JobStatus
                    .Include(e => e.EmpActingStatus)
                    .Include(e => e.EmpStatus)
                    .ToList();

                List<JobStatus> loadJobsta = new List<JobStatus>(); 
                var Senioritypolicys = db.Senioritypolicy.Include(e => e.JobStatus).FirstOrDefault();
                List<JobStatus> JOBSExist = new List<JobStatus>();
                if (Senioritypolicys != null)
                {
                    foreach (var item in fall)
                    {
                        var JobstatusCount = Senioritypolicys.JobStatus.Where(e => e.Id == item.Id).FirstOrDefault();
                        if (JobstatusCount != null)
                        {
                            JOBSExist.Add(JobstatusCount);
                        }
                        
                    }

                   loadJobsta = fall.Except(JOBSExist).ToList();
                }
                

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.JobStatus.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                if (JOBSExist.Count()>0)
                {
                    var r = (from ca in loadJobsta select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                    
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                
            }
        }


        public class returndatagridDataclass //Parentgrid
        {
            public int Id { get; set; }
            public string Name { get; set; }

        }
        public ActionResult SeniorityPolicy_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.Senioritypolicy.ToList();
                    IEnumerable<Senioritypolicy> fall;

                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {

                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.Name.ToLower().Contains(param.sSearch.ToLower()))
                                  ).ToList();
                    }
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<Senioritypolicy, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Name : "");
                    var sortcolumn = Request["sSortDir_0"];
                    if (sortcolumn == "asc")
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    else
                    {
                        fall = fall.OrderByDescending(orderfunc);
                    }

                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridDataclass
                            {
                                Id = item.Id,
                                Name = item.Name,
                            });
                        }
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.Name, };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
                }
                catch (Exception e)
                {
                    List<string> Msg = new List<string>();
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        public class returndatagridChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string JobStatus { get; set; }
        }

        public ActionResult A_Senioritypolicy_Grid(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.Senioritypolicy
                        .Include(e => e.JobStatus)
                        .Include(e => e.JobStatus.Select(r => r.EmpStatus))
                        .Include(e => e.JobStatus.Select(r => r.EmpActingStatus))
                        .Where(e => e.Id == data).SingleOrDefault();

                    if (db_data.JobStatus != null)
                    {
                        List<returndatagridChildDataClass> returndata = new List<returndatagridChildDataClass>();
                        foreach (var item in db_data.JobStatus)
                        {
                            returndata.Add(new returndatagridChildDataClass
                            {
                                Id = item.Id,
                                JobStatus = item.FullDetails

                            });
                        }

                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        [HttpPost]
        public ActionResult Create(Senioritypolicy S, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string JobStatusList = form["JobStatusList"] == "0" ? null : form["JobStatusList"];
                    var objjobstatus = new JobStatus();
                    if (JobStatusList != null)
                    {
                        var ids = Utility.StringIdsToListIds(JobStatusList);
                        var JobStatusListData = new List<JobStatus>();
                        foreach (var item in ids)
                        {
                            var returndata = db.JobStatus.Include(e => e.EmpStatus)
                                .Include(e => e.EmpActingStatus).Where(e => e.Id == item).SingleOrDefault();
                            if (returndata != null && returndata.EmpStatus != null && returndata.EmpActingStatus != null)
                            {
                                var EmpStatus = returndata.EmpStatus;
                                var EmpActingStatus = returndata.EmpActingStatus;

                                S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
                                objjobstatus = new JobStatus()
                                {

                                    EmpStatus = EmpStatus,
                                    EmpActingStatus = EmpActingStatus,
                                    DBTrack = S.DBTrack
                                };
                                JobStatusListData.Add(objjobstatus);
                                db.SaveChanges();
                                int comp_Id = 0;
                                comp_Id = Convert.ToInt32(Session["CompId"]);
                                var Company = new Company();
                                Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
                                if (Company != null)
                                {
                                    var Objjob = new List<JobStatus>();
                                    Objjob.Add(objjobstatus);
                                    Company.JobStatus = Objjob;
                                    db.Company.Attach(Company);
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

                                }


                            }
                        }
                        S.JobStatus = JobStatusListData;
                    }
                    else
                        S.JobStatus = null;

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            Senioritypolicy SenirityPolicyT = new Senioritypolicy()
                            {

                                JobStatus = S.JobStatus,
                                Name = S.Name,
                                DBTrack = S.DBTrack
                            };

                            db.Senioritypolicy.Add(SenirityPolicyT);

                            try
                            {

                                db.SaveChanges();

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
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
                        // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                var returndata = db.Senioritypolicy.Include(e => e.JobStatus)
                     .Where(e => e.Id == data).ToList();

                foreach (var item in returndata)
                {
                    result.Add(new returndatagridDataclass
                    {
                        Id = item.Id,
                        Name = item.Name
                    });
                }
                var return_data = db.Senioritypolicy.Include(e => e.JobStatus)
                    .Include(e => e.JobStatus.Select(r => r.EmpStatus))
                    .Include(e => e.JobStatus.Select(r => r.EmpActingStatus))
                    .Where(e => e.Id == data).SingleOrDefault();

                List<returndatagridChildDataClass> dtreturn = new List<returndatagridChildDataClass>();
                if (return_data != null && return_data.JobStatus != null)
                {
                    foreach (var item in return_data.JobStatus)
                    {
                        dtreturn.Add(new returndatagridChildDataClass
                        {
                            Id = item.Id,
                            JobStatus = item.FullDetails
                        });
                    }
                }
                return Json(new Object[] { result, dtreturn, JsonRequestBehavior.AllowGet });
            }
        }



        [HttpPost]
        public async Task<ActionResult> EditSave(Senioritypolicy S, int data, FormCollection form)
        {

            List<string> Msg = new List<string>();
            string JobStatusList = form["JobStatusList"] == "0" ? null : form["JobStatusList"];
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.Senioritypolicy.Include(e => e.JobStatus).Where(e => e.Id == data).SingleOrDefault();

                        List<JobStatus> SenioritypolicyList = new List<JobStatus>();
                        if (JobStatusList != "" && JobStatusList != null)
                        {
                            var ids = Utility.StringIdsToListIds(JobStatusList);
                            foreach (var ca in ids)
                            {

                                var JobStatus_val = db.JobStatus.Find(ca);
                                SenioritypolicyList.Add(JobStatus_val);
                            }
                            db_data.JobStatus = SenioritypolicyList;
                        }
                        else
                        {
                            db_data.JobStatus = null;
                        }

                        db.Senioritypolicy.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;
                        Senioritypolicy SP_Policy = db.Senioritypolicy.Find(data);
                        TempData["CurrRowVersion"] = SP_Policy.RowVersion;

                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            S.DBTrack = new DBTrack
                            {
                                CreatedBy = SP_Policy.DBTrack.CreatedBy == null ? null : SP_Policy.DBTrack.CreatedBy,
                                CreatedOn = SP_Policy.DBTrack.CreatedOn == null ? null : SP_Policy.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            SP_Policy.Id = data;
                            SP_Policy.Name = S.Name;
                            SP_Policy.DBTrack = S.DBTrack;
                            SP_Policy.JobStatus = db_data.JobStatus;
                            db.Senioritypolicy.Attach(SP_Policy);
                            db.Entry(SP_Policy).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
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




        [HttpPost]
        public async Task<ActionResult> Delete(string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListIds(data);
                int empidintrnal = Convert.ToInt32(ids[0]);
                int empidMain = Convert.ToInt32(ids[1]);

                try
                {
                    Senioritypolicy Senioritypoli = db.Senioritypolicy.Include(e => e.JobStatus)

                                                      .Where(e => e.Id == empidMain).SingleOrDefault();

                    var id = int.Parse(Session["CompId"].ToString());
                    var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
                    // companypayroll.SalaryHead.Where(e => e.Id == Senioritypoli.Id);
                    // companypayroll.SalaryHead = null;
                    //Senioritypoli.JobStatus = db.JobStatus.Where(e => e.Id == empidintrnal).ToList();
                    List<JobStatus> SenioritypolicyList = new List<JobStatus>();
                   
                    foreach (var item in Senioritypoli.JobStatus)
                    {
                        var JobStatus_val = db.JobStatus.Find(item.Id);
                        if (empidintrnal == JobStatus_val.Id)
                        {
                            continue;
                        }
                        else
                        {
                            SenioritypolicyList.Add(JobStatus_val);
                        }

                    }
                    Senioritypoli.JobStatus = SenioritypolicyList;
                    db.Senioritypolicy.Attach(Senioritypoli);
                    db.Entry(Senioritypoli).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    // db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                    // db.SaveChanges();
                    //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    //JobStatus val = SalaryHeads.JobStatus;



                    if (Senioritypoli.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Senioritypoli.DBTrack.CreatedBy != null ? Senioritypoli.DBTrack.CreatedBy : null,
                                CreatedOn = Senioritypoli.DBTrack.CreatedOn != null ? Senioritypoli.DBTrack.CreatedOn : null,
                                IsModified = Senioritypoli.DBTrack.IsModified == true ? true : false
                            };
                            Senioritypoli.DBTrack = dbT;
                            db.Entry(Senioritypoli).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/Core", null, db.ChangeTracker, Senioritypoli.DBTrack);

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
                                    CreatedBy = Senioritypoli.DBTrack.CreatedBy != null ? Senioritypoli.DBTrack.CreatedBy : null,
                                    CreatedOn = Senioritypoli.DBTrack.CreatedOn != null ? Senioritypoli.DBTrack.CreatedOn : null,
                                    IsModified = Senioritypoli.DBTrack.IsModified == true ? false : false//,
                                };


                                db.Entry(Senioritypoli).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/Core", null, db.ChangeTracker, dbT);
                                //DT_SalaryHead DT_Corp = (DT_SalaryHead)rtn_Obj;
                                //DT_Corp.Frequency_Id = val1 == null ? 0 : val1.Id;
                                //DT_Corp.Type_Id = val == null ? 0 : val.Id;
                                //DT_Corp.RoundingMethod_Id = val2 == null ? 0 : val2.Id;
                                //DT_Corp.SalHeadOperationType_Id = val3 == null ? 0 : val3.Id;
                                //DT_Corp.ProcessType_Id = val4 == null ? 0 : val4.Id;
                                //db.Create(DT_Corp);

                                await db.SaveChangesAsync();
                                ts.Complete();
                                // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
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