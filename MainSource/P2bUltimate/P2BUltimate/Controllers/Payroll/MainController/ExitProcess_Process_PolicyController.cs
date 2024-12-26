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


namespace P2BUltimate.Controllers
{
    public class ExitProcess_Process_PolicyController : Controller
    {
        //
        // GET: /ExitProcess_Process_Policy/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ExitProcess_Process_Policy/Index.cshtml");
        }


      

        public ActionResult Create(ExitProcess_Process_Policy c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string IsFFSAppl = form["IsFFSAppl"] == "0" ? "" : form["IsFFSAppl"];
                string IsFFSDocAppl = form["IsFFSDocAppl"] == "0" ? "" : form["IsFFSDocAppl"];
                string IsNoDuesAppl = form["IsNoDuesAppl"] == "0" ? "" : form["IsNoDuesAppl"];
                string IsPartPayAppl = form["IsPartPayAppl"] == "0" ? "" : form["IsPartPayAppl"];
                string IsNoticePeriodAppl = form["IsNoticePeriodAppl"] == "0" ? "" : form["IsNoticePeriodAppl"];
                string IsRefDocAppl = form["IsRefDocAppl"] == "0" ? "" : form["IsRefDocAppl"];
                string IsResignRequestAppl = form["IsResignRequestAppl"] == "0" ? "" : form["IsResignRequestAppl"];
                string IsExitInterviewAppl = form["IsExitInterviewAppl"] == "0" ? "" : form["IsExitInterviewAppl"];
                List<String> Msg = new List<String>();
                try
                {

                    c.IsExitInterviewAppl = Convert.ToBoolean(IsExitInterviewAppl);
                    c.IsFFSAppl = Convert.ToBoolean(IsFFSAppl);
                    c.IsFFSDocAppl = Convert.ToBoolean(IsFFSDocAppl);
                    c.IsNoDuesAppl = Convert.ToBoolean(IsNoDuesAppl);
                    c.IsNoticePeriodAppl = Convert.ToBoolean(IsNoticePeriodAppl);
                    c.IsPartPayAppl = Convert.ToBoolean(IsPartPayAppl);
                    c.IsRefDocAppl = Convert.ToBoolean(IsRefDocAppl);
                    c.IsResignRequestAppl = Convert.ToBoolean(IsResignRequestAppl);
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            ExitProcess_Process_Policy sep = new ExitProcess_Process_Policy()
                            {
                                IsExitInterviewAppl = c.IsExitInterviewAppl,
                                IsFFSAppl = c.IsFFSAppl,
                                IsFFSDocAppl = c.IsFFSDocAppl,
                                IsNoDuesAppl = c.IsNoDuesAppl,
                                IsNoticePeriodAppl = c.IsNoticePeriodAppl,
                                IsPartPayAppl = c.IsPartPayAppl,
                                IsRefDocAppl = c.IsRefDocAppl,
                                IsResignRequestAppl = c.IsResignRequestAppl,
                                ProcessName = c.ProcessName,
                                DBTrack = c.DBTrack
                            };

                            db.ExitProcess_Process_Policy.Add(sep);
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


        public ActionResult GetExitProcessProcessPolicyDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.ExitProcess_Process_Policy.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ExitProcess_Process_Policy.Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "Process Name:" + ca.ProcessName }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }



        public ActionResult  P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                int ParentId = 2;
                var jsonData = (Object)null;
                var LKVal = db.ExitProcess_Process_Policy.ToList();

                //if (gp.IsAutho == true)
                //{
                //    LKVal = db.ExitProcess_Process_Policy

                // .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                //}
                //else
                //{
                //    LKVal = db.ExitProcess_Process_Policy

                // .Where(e => e.DBTrack.IsModified == false).AsNoTracking().ToList();
                //}


                IEnumerable<ExitProcess_Process_Policy> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.ProcessName.ToUpper().Contains(gp.searchString.ToUpper()))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.ProcessName, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ProcessName, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<ExitProcess_Process_Policy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "ProcessName" ? c.ProcessName :
                                         "");
                    }

                    //Func<BasicScale, string> orderfuc = (c =>
                    //                                           gp.sidx == "Id" ? c.Id.ToString() :
                    //                                           gp.sidx == "Code" ? c.Code :
                    //                                           gp.sidx == "ScaleName" ? c.ScaleName : "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.ProcessName, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.ProcessName, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ProcessName, a.Id }).ToList();
                    }
                    totalRecords = LKVal.Count();
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
                    total = totalPages,
                    p2bparam = ParentId
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ExitProcess_Process_Policy

                 .Where(e => e.Id == data).Select
                 (e => new
                 {
                     IsFFSAppl_id = e.IsFFSAppl,
                     IsFFSDocAppl = e.IsFFSDocAppl,
                     IsNoDuesAppl = e.IsNoDuesAppl,
                     IsPartPayAppl = e.IsPartPayAppl,
                     IsNoticePeriodAppl = e.IsNoticePeriodAppl,
                     IsRefDocAppl = e.IsRefDocAppl,
                     IsResignRequestAppl = e.IsResignRequestAppl,
                     IsExitInterviewAppl = e.IsExitInterviewAppl,
                     ProcessName = e.ProcessName,
                 }).ToList();

                var Corp = db.ExitProcess_Process_Policy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, Auth, JsonRequestBehavior.AllowGet });

            }

        }

        [HttpPost]
        public async Task<ActionResult> EditSave(ExitProcess_Process_Policy c, int data, FormCollection form) // Edit submit
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
                                    ExitProcess_Process_Policy blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ExitProcess_Process_Policy.Where(e => e.Id == data)
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
                                    var m1 = db.ExitProcess_Process_Policy.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ExitProcess_Process_Policy.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    var CurCorp = db.ExitProcess_Process_Policy.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {


                                        ExitProcess_Process_Policy corp = new ExitProcess_Process_Policy()
                                        {
                                            IsExitInterviewAppl = c.IsExitInterviewAppl,
                                            IsFFSAppl = c.IsFFSAppl,
                                            IsFFSDocAppl = c.IsFFSDocAppl,
                                            IsNoDuesAppl = c.IsNoDuesAppl,
                                            IsNoticePeriodAppl = c.IsNoticePeriodAppl,
                                            IsPartPayAppl = c.IsPartPayAppl,
                                            IsRefDocAppl = c.IsRefDocAppl,
                                            IsResignRequestAppl = c.IsResignRequestAppl,
                                            ProcessName = c.ProcessName,
                                            Id = data,
                                            DBTrack = c.DBTrack
                                        };

                                        db.ExitProcess_Process_Policy.Attach(corp);
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
                                        Val = c.ProcessName,
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
                    ExitProcess_Process_Policy ExitProcess_Process_Policy = db.ExitProcess_Process_Policy
                                                                            
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    db.Entry(ExitProcess_Process_Policy).State = System.Data.Entity.EntityState.Deleted;
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