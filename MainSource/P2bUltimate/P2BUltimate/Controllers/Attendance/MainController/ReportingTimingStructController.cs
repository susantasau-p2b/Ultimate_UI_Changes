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
using System.Reflection;
using P2BUltimate.Security;
using Attendance;

namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class ReportingTimingStructController : Controller
    {
        //
        // GET: /ReportingTimingStruct/
        //private DataBaseContext db = new DataBaseContext();
        List<string> Msg = new List<string>();

        public ActionResult Index()
        {
            return View("~/Views/Shared/Attendance/_ReportingTimingStruct.cshtml");
        }

        public ActionResult GetTimingPolicyLKDetails(List<int> SkipIds)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TimingPolicy.Include(e => e.EarlyAction).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TimingPolicy.Include(e => e.EarlyAction).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.ReportingTimingStruct.ToList().Select(e => e.TimingPolicy);
                var list2 = fall.Except(list1);

                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }

        public ActionResult Create(ReportingTimingStruct COBJ, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string GeoGraphList = form["GeoGraphList-drop"] == "0" ? "" : form["GeoGraphList-drop"];
                    string TimingPolicy = form["TimingPolicylist"] == "0" ? "" : form["TimingPolicylist"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (GeoGraphList != null && GeoGraphList != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "604").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(GeoGraphList)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(Corp));
                            COBJ.GeoGraphList = val; 
                    }

                    if (TimingPolicy != null && TimingPolicy != "")
                    {
                        int ContId = Convert.ToInt32(TimingPolicy);
                        var val = db.TimingPolicy.Include(e => e.FlexiAction).Where(e => e.Id == ContId).SingleOrDefault();
                        COBJ.TimingPolicy = val;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            ReportingTimingStruct ReportingStruct = new ReportingTimingStruct()
                            {
                                GeographicalAppl = COBJ.GeographicalAppl,
                                GeoGraphList = COBJ.GeoGraphList,
                                RSName = COBJ.RSName,
                                IsTimeRoaster = COBJ.IsTimeRoaster,
                                IndividualAppl = COBJ.IndividualAppl,
                                TimingPolicy = COBJ.TimingPolicy,
                                DBTrack = COBJ.DBTrack,
                            };
                            try
                            {
                                db.ReportingTimingStruct.Add(ReportingStruct);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = ReportingStruct.Id, Val = ReportingStruct.RSName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { ReportingStruct.Id, ReportingStruct.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
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
        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ReportingTimingStruct.Include(e => e.GeoGraphList).Where(e => e.Id == data).Select
                    (e => new
                    {
                        // JobPosition_Id = e.FuncStruct.JobPosition.Id==null? 0 : e.FuncStruct.JobPosition.Id ,
                        GeographicalAppl = e.GeographicalAppl,
                        GeoGraphList_Id = e.GeoGraphList == null ? 0 : e.GeoGraphList.Id,
                        RSName = e.RSName,
                        IsTimeRoaster = e.IsTimeRoaster,
                        IndividualAppl = e.IndividualAppl,
                        DBTrack = e.DBTrack,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.ReportingTimingStruct.Include(e => e.TimingPolicy).Where(e => e.Id == data).Select(e => new
                    {
                        GeoGraphList_val = e.TimingPolicy == null ? "" : e.TimingPolicy.FullDetails,
                        GeoGraphList_Id = e.TimingPolicy == null ? "" : e.TimingPolicy.Id.ToString(),
                    }).ToList();

                var Corp = db.ReportingTimingStruct.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }
     
        [HttpPost]
        //public async Task<ActionResult> EditSave(ReportingTimingStruct L, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //        try
        //        {
        //            string Corp = form["GeoGraphList-drop"] == "0" ? "" : form["GeoGraphList-drop"];
        //            string TimingPolicy = form["TimingPolicylist"] == "0" ? "" : form["TimingPolicylist"];

        //            var blog1 = db.ReportingTimingStruct.Include(e => e.TimingPolicy).Include(e => e.GeoGraphList).Where(e => e.Id == data).SingleOrDefault();
        //            blog1.TimingPolicy_Id = TimingPolicy != null && TimingPolicy != "" ? int.Parse(TimingPolicy) : 0;
        //            blog1.TimingPolicy = null;
        //            blog1.GeoGraphList = L.GeoGraphList;
        //            blog1.GeographicalAppl = L.GeographicalAppl;
        //            blog1.RSName = L.RSName;
        //            blog1.IsTimeRoaster = L.IsTimeRoaster;
        //            blog1.IndividualAppl = L.IndividualAppl;

        //            //if (Corp != null)
        //            //{
        //            //    if (Corp != "")
        //            //    {
        //            //        var val = db.LookupValue.Find(int.Parse(Corp));
        //            //        blog1.GeoGraphList = val;
        //            //    }
        //            //}
        //            if (Corp != null)
        //            {
        //                if (Corp != "")
        //                {
        //                    //var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "803").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmailAttachmentTypelist)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailAttachmentTypelist));
        //                    //c.EmailAttachmentType = val;

        //                    blog1.GeoGraphList_Id = Convert.ToInt32(Corp);
        //                }
        //            }
        //            else
        //            {
        //                blog1.GeoGraphList_Id = null;
        //            }


        //            //if (TimingPolicy != null && TimingPolicy != "")
        //            //{
        //            //    int ContId = Convert.ToInt32(TimingPolicy);
        //            //    var val = db.TimingPolicy.Include(e => e.FlexiAction).Where(e => e.Id == ContId).SingleOrDefault();
        //            //    blog1.TimingPolicy = val;
        //            //}


        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {
        //                    //  using (DataBaseContext db = new DataBaseContext())
        //                    {
        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {

        //                            using (var context = new DataBaseContext())

        //                                blog1.DBTrack = new DBTrack
        //                                {
        //                                    CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
        //                                    CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
        //                                    Action = "M",
        //                                    ModifiedBy = SessionManager.UserName,
        //                                    ModifiedOn = DateTime.Now
        //                                };

        //                            if (Corp != null && Corp != "")
        //                                {
        //                                    var val = db.LookupValue.Find(int.Parse(Corp));
        //                                    blog1.GeoGraphList = val;

        //                                    var type = db.ReportingTimingStruct.Include(e => e.GeoGraphList).Where(e => e.Id == data).SingleOrDefault();
        //                                    IList<ReportingTimingStruct> typedetails = null;
        //                                    if (type.GeoGraphList != null)
        //                                    {
        //                                        typedetails = db.ReportingTimingStruct.Where(x => x.Id == data && x.GeoGraphList.Id == type.GeoGraphList.Id).ToList();
        //                                    }
        //                                    else
        //                                    {
        //                                        typedetails = db.ReportingTimingStruct.Where(x => x.Id == data).ToList();
        //                                    }
        //                                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                                    foreach (var s in typedetails)
        //                                    {
        //                                        s.GeoGraphList = blog1.GeoGraphList;
        //                                        db.ReportingTimingStruct.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    var WFTypeDetails = db.ReportingTimingStruct.Include(e => e.GeoGraphList).Where(x => x.Id == data).ToList();
        //                                    foreach (var s in WFTypeDetails)
        //                                    {
        //                                        s.GeoGraphList = null;
        //                                        db.ReportingTimingStruct.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }

        //                            var CurCorp = db.ReportingTimingStruct.Find(data);
        //                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                          //  db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {

        //                                ReportingTimingStruct lk = new ReportingTimingStruct
        //                                {
        //                                    Id = data,
        //                                    GeographicalAppl = blog1.GeographicalAppl,
        //                                    GeoGraphList = blog1.GeoGraphList,
        //                                    GeoGraphList_Id =blog1.GeoGraphList_Id, 
        //                                    RSName = blog1.RSName,
        //                                    IsTimeRoaster = blog1.IsTimeRoaster,
        //                                    IndividualAppl = blog1.IndividualAppl,
        //                                    TimingPolicy = blog1.TimingPolicy,
        //                                    TimingPolicy_Id = blog1.TimingPolicy_Id,
        //                                    DBTrack = blog1.DBTrack
        //                                };

        //                                db.ReportingTimingStruct.Attach(lk);
        //                                db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

        //                                db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                                                            
        //                                db.SaveChanges();
        //                                //}

        //                                await db.SaveChangesAsync();
        //                                db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
        //                                ts.Complete();
        //                                Msg.Add("  Record Updated");
        //                                return Json(new Utility.JsonReturnClass { Id = blog1.Id, Val = blog1.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

        //                            }
        //                        }
        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (ReportingTimingStruct)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (ReportingTimingStruct)databaseEntry.ToObject();
        //                        blog1.RowVersion = databaseValues.RowVersion;
        //                    }
        //                }
        //                Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }
        //            return Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //}
        public async Task<ActionResult> EditSave(ReportingTimingStruct L, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            string Corp = form["GeoGraphList-drop"] == "0" ? "" : form["GeoGraphList-drop"];
            string TimingPolicy = form["TimingPolicylist"] == "0" ? "" : form["TimingPolicylist"];
            bool Auth = form["Autho_Allow"] == "true" ? true : false;
            if (Corp != null)
            {
                if (Corp != "")
                {
                    //var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "803").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmailAttachmentTypelist)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailAttachmentTypelist));
                    //c.EmailAttachmentType = val;
                    L.GeoGraphList_Id = Convert.ToInt32(Corp);
                }
            }
            else
            {
                L.GeoGraphList_Id = null;
            }

            L.TimingPolicy_Id= TimingPolicy != null && TimingPolicy != "" ? int.Parse(TimingPolicy) : 0;
           
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var blog1 = db.ReportingTimingStruct.Include(e => e.TimingPolicy).Include(e => e.GeoGraphList).Where(e => e.Id == data).SingleOrDefault();
                        if (L.TimingPolicy_Id == 0)
                        {
                            blog1.TimingPolicy = null;
                        }

                        db.ReportingTimingStruct.Attach(blog1);
                        db.Entry(blog1).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = blog1.RowVersion;

                        ReportingTimingStruct reportingtimingstruct = db.ReportingTimingStruct.Find(data);
                        TempData["CurrRowVersion"] = reportingtimingstruct.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                           

                            L.DBTrack = new DBTrack
                            {
                                CreatedBy = reportingtimingstruct.DBTrack.CreatedBy == null ? null : reportingtimingstruct.DBTrack.CreatedBy,
                                CreatedOn = reportingtimingstruct.DBTrack.CreatedOn == null ? null : reportingtimingstruct.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (L.TimingPolicy_Id != 0)
                                reportingtimingstruct.TimingPolicy_Id = L.TimingPolicy_Id != null ? L.TimingPolicy_Id : 0;

                            reportingtimingstruct.Id = data;
                            reportingtimingstruct.GeographicalAppl = L.GeographicalAppl;

                            reportingtimingstruct.GeoGraphList_Id = L.GeoGraphList_Id;
                            reportingtimingstruct.GeoGraphList = L.GeoGraphList;
                            reportingtimingstruct.RSName = L.RSName;
                            reportingtimingstruct.IsTimeRoaster = L.IsTimeRoaster;
                            reportingtimingstruct.IndividualAppl = L.IndividualAppl;
                            reportingtimingstruct.DBTrack = L.DBTrack;
                            db.Entry(reportingtimingstruct).State = System.Data.Entity.EntityState.Modified;


                            //using (var context = new DataBaseContext())
                            //{


                            db.SaveChanges();
                        }
                        //}
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

            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }

    }
}