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
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers
{
    public class ReportingStructController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_ReportingStruct.cshtml");
        }
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                //if (data != "" && data != null)
                //{
                var qurey = db.ReportingStruct.ToList(); // added by rekha 26-12-16
                if (data2 != "" && data2 != "0")
                {
                    selected = data2;
                }
                if (qurey != null)
                {
                    s = new SelectList(qurey, "Id", "RSName", selected);
                }
                //}
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookup_FuncStruct(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.FuncStruct.Include(e => e.Job).Include(e => e.JobPosition).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.FuncStruct.Include(e => e.Job).Include(e => e.JobPosition).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }

                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookup_BossEmp(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Employee
                    .Include(e => e.EmpName)
                    .Include(e => e.ServiceBookDates)                  
                    .Where(e => e.ServiceBookDates.ServiceLastDate == null).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Employee.Include(e => e.EmpName).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }

                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ReportingStruct.Include(e => e.FuncStruct).Include(e => e.GeoFuncList).Include(e => e.GeoGraphList).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ReportingStruct.Include(e => e.FuncStruct).Include(e => e.GeoFuncList).Include(e => e.GeoGraphList).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Create(ReportingStruct COBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string GeoGraphList = form["GeoGraphList-drop"] == "0" ? "" : form["GeoGraphList-drop"];
                    string GeoFuncList = form["GeoFuncList-drop"] == "0" ? "" : form["GeoFuncList-drop"];
                    string FuncStruct = form["FuncStructlist"] == "0" ? "" : form["FuncStructlist"];

                    string BossEmplist = form["BossEmplist"] == "0" ? "" : form["BossEmplist"];

                    int comp_Id = Convert.ToInt32(Session["CompID"]);
                    var Company = new Company();
                    Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
                    //if (GeoGraphList != null && GeoGraphList != "" && GeoFuncList != null && GeoFuncList != "")
                    //{
                    if (GeoGraphList != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "604").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(GeoGraphList)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(GeoGraphList));
                        COBJ.GeoGraphList = val;
                    }

                    if (GeoFuncList != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "605").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(GeoFuncList)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(GeoFuncList));
                        COBJ.GeoFuncList = val;
                    }
                    if (FuncStruct != "")
                    {
                        var val = db.FuncStruct.Find(Convert.ToInt32(FuncStruct));
                        COBJ.FuncStruct = val;
                    }

                    if (BossEmplist != "")
                    {
                        var val = db.Employee.Find(Convert.ToInt32(BossEmplist));
                        COBJ.BossEmp = val;
                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            //   COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            ReportingStruct ReportingStruct = new ReportingStruct()
                            {
                                GeographicalAppl = COBJ.GeographicalAppl,
                                GeoGraphList = COBJ.GeoGraphList,
                                FunctionalAppl = COBJ.FunctionalAppl,
                                GeoFuncList = COBJ.GeoFuncList,
                                FuncStruct = COBJ.FuncStruct,
                                BossEmp = COBJ.BossEmp,
                                RoleBasedAppl = COBJ.RoleBasedAppl,
                                IndividualAppl = COBJ.IndividualAppl,
                                RSName = COBJ.RSName,
                                FullDetails = COBJ.FullDetails
                                //    DBTrack = COBJ.DBTrack
                            };
                            try
                            {
                                db.ReportingStruct.Add(ReportingStruct);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, COBJ.DBTrack);
                                //DT_JobStatus DT_OBJ = (DT_JobStatus)rtn_Obj;
                                //DT_OBJ.EmpStatus_Id = COBJ.GeoFuncList == null ? 0 : COBJ.GeoFuncList.Id;
                                //DT_OBJ.EmpActingStatus_Id = COBJ.GeoGraphList == null ? 0 : COBJ.GeoGraphList.Id;
                                //db.Create(DT_OBJ);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = ReportingStruct.Id, Val = ReportingStruct.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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


                //else
                //{
                //    var errorMsg = "GeoGraphList , GeoFuncList Can't Be Empty....";

                //    return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });

                //}
            }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ReportingStruct = db.ReportingStruct
                                    .Include(e=>e.BossEmp)
                                    .Include(e=>e.BossEmp.EmpName)
                                    .Include(e => e.GeoGraphList)
                                    .Include(e => e.GeoFuncList)
                                    .Include(e => e.FuncStruct)
                                    .Include(e => e.FuncStruct.Job)
                                    .Include(e => e.FuncStruct.JobPosition)
                                    .Where(e => e.Id == data).ToList();
                var r = (from ca in ReportingStruct
                         select new
                         {
                             Id = ca.Id,
                             GeoFuncList = ca.GeoFuncList != null ? ca.GeoFuncList.Id : 0,
                             GeoGraphList = ca.GeoGraphList != null ? ca.GeoGraphList.Id : 0,
                             FunctionalAppl = ca.FunctionalAppl,
                             GeographicalAppl = ca.GeographicalAppl,
                             RoleBasedAppl = ca.RoleBasedAppl,
                             IndividualAppl = ca.IndividualAppl,
                             FuncStruct_Id = ca.FuncStruct != null ? ca.FuncStruct.Id : 0,
                             FuncStruct_Val = ca.FuncStruct != null ? ca.FuncStruct.FullDetails : null,
                             BossEmp_Id = ca.BossEmp != null ? ca.BossEmp.Id : 0,
                             BossEmp_Val = ca.BossEmp != null ? ca.BossEmp.FullDetails : null,
                             RSName = ca.RSName
                         }).Distinct();

                var a = "";

                var W = db.DT_JobStatus
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         GeoFuncList = e.EmpStatus_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.EmpStatus_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         GeoGraphList = e.EmpActingStatus_Id == 0 ? "" : db.LookupValue
                                     .Where(x => x.Id == e.EmpActingStatus_Id)
                                     .Select(x => x.LookupVal).FirstOrDefault(),

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.ReportingStruct.Find(data);
                ////TempData["RowVersion"] = LKup.RowVersion;
                //var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, a, W, "", JsonRequestBehavior.AllowGet });
            }
        }
        //public async Task<ActionResult> EditSave(ReportingStruct ESOBJ, FormCollection form, int data)
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            string GeoFuncList = form["GeoFuncList-drop"] == "0" ? "" : form["GeoFuncList-drop"];
        //            string GeoGraphList = form["GeoGraphList-drop"] == "0" ? "" : form["GeoGraphList-drop"];
        //            string FuncStructlist = form["FuncStructlist"] == "0" ? "" : form["FuncStructlist"];

        //            //if (GeoGraphList != null && GeoGraphList != "" && GeoFuncList != null && GeoFuncList != "")
        //            //{
        //            if (GeoGraphList != "")
        //            {
        //                var val = db.LookupValue.Find(int.Parse(GeoGraphList));
        //                ESOBJ.GeoGraphList = val;
        //            }


        //            if (GeoFuncList != null)
        //            {
        //                if (GeoFuncList != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(GeoFuncList));
        //                    ESOBJ.GeoFuncList = val;
        //                }
        //            }
        //            if (FuncStructlist != null)
        //            {

        //                var val = db.FuncStruct.Find(int.Parse(FuncStructlist));
        //                ESOBJ.FuncStruct = val;
        //            }


        //            if (Auth == false)
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            //ReportingStruct blog = null; // to retrieve old data
        //                            //DbPropertyValues originalBlogValues = null;

        //                            //using (var context = new DataBaseContext())
        //                            //{
        //                            //    blog = context.ReportingStruct.Where(e => e.Id == data)
        //                            //                            .Include(e => e.GeoGraphList)
        //                            //                            .Include(e => e.GeoFuncList)
        //                            //                            .SingleOrDefault();
        //                            //    originalBlogValues = context.Entry(blog).OriginalValues;
        //                            //}

        //                            //ESOBJ.DBTrack = new DBTrack
        //                            //{
        //                            //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                            //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                            //    Action = "M",
        //                            //    ModifiedBy = SessionManager.UserName,
        //                            //    ModifiedOn = DateTime.Now
        //                            //};

        //                            int a = EditS(GeoFuncList, GeoGraphList, data, ESOBJ);



        //                            using (var context = new DataBaseContext())
        //                            {
        //                                //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
        //                                //DT_JobStatus DT_OBJ = (DT_JobStatus)obj;
        //                                //DT_OBJ.EmpActingStatus_Id = blog.GeoGraphList == null ? 0 : blog.GeoGraphList.Id;
        //                                //DT_OBJ.EmpStatus_Id = blog.GeoFuncList == null ? 0 : blog.GeoFuncList.Id;
        //                                //db.Create(DT_OBJ);
        //                                //db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();
        //                            Msg.Add("  Data Created successfully  ");
        //                            return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            // return this.Json(new Object[] { ESOBJ.Id, ESOBJ.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
        //                        }
        //                    }

        //                    //catch (DbUpdateException e) { throw e; }
        //                    //catch (DataException e) { throw e; }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (ReportingStruct)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (ReportingStruct)databaseEntry.ToObject();
        //                            //ESOBJ.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    StringBuilder sb = new StringBuilder("");
        //                    foreach (ModelState modelState in ModelState.Values)
        //                    {
        //                        foreach (ModelError error in modelState.Errors)
        //                        {
        //                            sb.Append(error.ErrorMessage);
        //                            sb.Append("." + "\n");
        //                        }
        //                    }
        //                    var errorMsg = sb.ToString();
        //                    Msg.Add(errorMsg);
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
        //                }
        //            }


        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    //ReportingStruct blog = null; // to retrieve old data
        //                    //DbPropertyValues originalBlogValues = null;
        //                    //ReportingStruct Old_Obj = null;

        //                    //using (var context = new DataBaseContext())
        //                    //{
        //                    //    blog = context.ReportingStruct.Where(e => e.Id == data).SingleOrDefault();
        //                    //    originalBlogValues = context.Entry(blog).OriginalValues;
        //                    //}
        //                    //ESOBJ.DBTrack = new DBTrack
        //                    //{
        //                    //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                    //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                    //    Action = "M",
        //                    //    IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                    //    ModifiedBy = SessionManager.UserName,
        //                    //    ModifiedOn = DateTime.Now
        //                    //};
        //                    ReportingStruct corp = new ReportingStruct()
        //                    {
        //                        Id = data,
        //                        GeographicalAppl = ESOBJ.GeographicalAppl,
        //                        GeoGraphList = ESOBJ.GeoGraphList,
        //                        FunctionalAppl = ESOBJ.FunctionalAppl,
        //                        GeoFuncList = ESOBJ.GeoFuncList,
        //                        FuncStruct = ESOBJ.FuncStruct,
        //                        RoleBasedAppl = ESOBJ.RoleBasedAppl,
        //                        RSName = ESOBJ.RSName,
        //                        //DBTrack = ESOBJ.DBTrack,
        //                        //RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        //var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "ReportingStruct", ESOBJ.DBTrack);
        //                        //// var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        //Old_Obj = context.ReportingStruct.Where(e => e.Id == data).Include(e => e.GeoGraphList)
        //                        //    .Include(e => e.GeoFuncList).SingleOrDefault();
        //                        //DT_JobStatus DT_Corp = (DT_JobStatus)obj;
        //                        //DT_Corp.EmpActingStatus_Id = DBTrackFile.ValCompare(Old_Obj.GeoGraphList, ESOBJ.GeoGraphList);//Old_Obj.Address == c.Address ? 0 : Old_Obj.Address == null && c.Address != null ? c.Address.Id : Old_Obj.Address.Id;
        //                        //DT_Corp.EmpStatus_Id = DBTrackFile.ValCompare(Old_Obj.GeoFuncList, ESOBJ.GeoFuncList); //Old_Obj.BusinessType == c.BusinessType ? 0 : Old_Obj.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Obj.BusinessType.Id;                        
        //                        //db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    //blog.DBTrack = ESOBJ.DBTrack;
        //                    //db.ReportingStruct.Attach(blog);
        //                    //db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    //db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    Msg.Add("  Data Updated successfully.  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Object[] { "", "", "Record Updated", JsonRequestBehavior.AllowGet });
        //                }
        //            }
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
        //        return View();
        //    }
        //}
        public ActionResult EditSave(ReportingStruct data1, int data, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    //Calendar c = db.Calendar.Find(data);
                    string GeoFuncList = form["GeoFuncList-drop"] == "0" ? "" : form["GeoFuncList-drop"];
                    string GeoGraphList = form["GeoGraphList-drop"] == "0" ? "" : form["GeoGraphList-drop"];
                    string FuncStructlist = form["FuncStructlist"] == "0" ? "" : form["FuncStructlist"];
                    //var Name = form["Name_drop"] == "0" ? 0 : Convert.ToInt32(form["Name_drop"]);

                    //if (Name != 0)
                    //{
                    //    data1.Name = db.LookupValue.Find(Name);
                    //}

                    string BossEmplist = form["BossEmplist"] == "0" ? "" : form["BossEmplist"];

                    var db_data = db.ReportingStruct
                         .Include(e=>e.BossEmp)
                         .Include(e=>e.BossEmp.EmpName)
                         .Include(q => q.GeoFuncList)
                         .Include(q => q.GeoGraphList)
                         .Include(q => q.FuncStruct)
                         .Where(a => a.Id == data).SingleOrDefault();
                    if (GeoGraphList != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "604").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(GeoGraphList)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(GeoGraphList));
                        db_data.GeoGraphList = val;
                    }
                    else
                    {
                        db_data.GeoGraphList = null;
                    }

                    if (GeoFuncList != null)
                    {
                        if (GeoFuncList != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "605").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(GeoFuncList)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(GeoFuncList));
                            db_data.GeoFuncList = val;
                        }
                    }
                    else
                    {
                        db_data.GeoFuncList = null;
                    }
                    if (FuncStructlist != null)
                    {
                        if (FuncStructlist != "")
                        {
                            int id = Convert.ToInt32(FuncStructlist);
                            var val = db.FuncStruct.Where(e => e.Id == id).SingleOrDefault();
                            db_data.FuncStruct = val;
                        }
                    }

                    else
                    {
                        db_data.FuncStruct = null;
                    }

                    if (BossEmplist != null)
                    {
                        if (BossEmplist != "")
                        {
                            int id = Convert.ToInt32(BossEmplist);
                            var val = db.Employee.Find(id);
                            db_data.BossEmp = val;
                        }
                    }
                    else
                    {
                        db_data.BossEmp = null;
                    }

                    //var alrdy = db.Calendar.Include(a => a.Name).Where(e => e.Name.LookupVal.ToString().ToUpper() == db_data.Name.LookupVal.ToString().ToUpper() && e.Default == true && data1.Default == true).Count();

                    //if (alrdy > 0)
                    //{
                    //    Msg.Add("   Default  Year already exist. ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    // return this.Json(new Object[] { "", "", "From Date already exists.", JsonRequestBehavior.AllowGet });
                    //}
                    //data1.db = new DBTrack
                    //{
                    //    CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                    //    CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                    //    Action = "M",
                    //    ModifiedBy = SessionManager.UserName,
                    //    ModifiedOn = DateTime.Now
                    //};

                    db_data.FunctionalAppl = data1.FunctionalAppl;
                    db_data.RoleBasedAppl = data1.RoleBasedAppl;
                    db_data.GeographicalAppl = data1.GeographicalAppl;
                    db_data.GeoGraphList = db_data.GeoGraphList;
                    db_data.GeoFuncList = db_data.GeoFuncList;
                    db_data.FuncStruct = db_data.FuncStruct;
                    db_data.BossEmp = db_data.BossEmp;
                    db_data.IndividualAppl = data1.IndividualAppl;

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                    }
                    Msg.Add("  Data Saved successfully  ");
                    return Json(new Utility.JsonReturnClass { Id = db_data.Id, Val = db_data.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        //else
        //{
        //    var errorMsg = "GeoGraphList , GeoFuncList Can't Be Empty....";

        //    return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });

        //}
        //return View();




        public int EditS(string RMVal, string PerkHVal, int data, ReportingStruct ESOBJ)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (RMVal != null)
                {
                    if (RMVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(RMVal));
                        ESOBJ.GeoFuncList = val;

                        var type = db.ReportingStruct.Include(e => e.GeoFuncList)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<ReportingStruct> typedetails = null;
                        if (type.GeoFuncList != null)
                        {
                            typedetails = db.ReportingStruct.Where(x => x.GeoFuncList.Id == type.GeoFuncList.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.ReportingStruct.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.GeoFuncList = ESOBJ.GeoFuncList;
                            db.ReportingStruct.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            //TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.ReportingStruct.Include(e => e.GeoFuncList).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.GeoFuncList = null;
                            db.ReportingStruct.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            //TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var Dtls = db.ReportingStruct.Include(e => e.GeoFuncList).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.GeoFuncList = null;
                        db.ReportingStruct.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        // TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }



                if (PerkHVal != null)
                {
                    if (PerkHVal != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(PerkHVal));
                        ESOBJ.GeoGraphList = val;

                        var type = db.ReportingStruct
                            .Include(e => e.GeoGraphList)
                            .Where(e => e.Id == data).SingleOrDefault();
                        IList<ReportingStruct> typedetails = null;
                        if (type.GeoGraphList != null)
                        {
                            typedetails = db.ReportingStruct.Where(x => x.GeoGraphList.Id == type.GeoGraphList.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.ReportingStruct.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.GeoGraphList = ESOBJ.GeoGraphList;
                            db.ReportingStruct.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            //TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var Dtls = db.ReportingStruct.Include(e => e.GeoGraphList).Where(x => x.Id == data).ToList();
                        foreach (var s in Dtls)
                        {
                            s.GeoGraphList = null;
                            db.ReportingStruct.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            //TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var Dtls = db.ReportingStruct.Include(e => e.GeoGraphList).Where(x => x.Id == data).ToList();
                    foreach (var s in Dtls)
                    {
                        s.GeoGraphList = null;
                        db.ReportingStruct.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        //TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var db_data = db.ReportingStruct.Include(e => e.FuncStruct).Where(e => e.Id == data).SingleOrDefault();

                db_data.FunctionalAppl = ESOBJ.FunctionalAppl;
                db_data.GeographicalAppl = ESOBJ.GeographicalAppl;
                db_data.RoleBasedAppl = ESOBJ.RoleBasedAppl;
                db_data.RSName = ESOBJ.RSName;
                if (ESOBJ.FuncStruct != null)
                {
                    db_data.FuncStruct = ESOBJ.FuncStruct;
                }
                else
                {
                    db_data.FuncStruct = null;
                }
                db.ReportingStruct.Attach(db_data);
                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                //var CurOBJ = db.ReportingStruct.Find(data);
                ////TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                //db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                //{
                //    ESOBJ.DBTrack = dbT;
                //    ReportingStruct ESIOBJ = new ReportingStruct()
                //    {
                //        Id = data,
                //        DBTrack = ESOBJ.DBTrack
                //    };

                //    db.ReportingStruct.Attach(ESIOBJ);
                //    db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                //    db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                //    //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                //    return 1;
                //}
                return 0;
            }
        }

        public ActionResult Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                ReportingStruct ReportingStruct = db.ReportingStruct.Where(e => e.Id == data).SingleOrDefault();
                try
                {
                    //var selectedValues = ReportingStruct.SocialActivities;
                    //var lkValue = new HashSet<int>(ReportingStruct.SocialActivities.Select(e => e.Id));
                    //if (lkValue.Count > 0)
                    //{
                    //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                    //}

                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(ReportingStruct).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                }

                catch (DataException /* dex */)
                {
                    //return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        public class P2BGridData
        {
            public int Id { get; set; }
            public string Empstatus { get; set; }
            public string EmpsActingstatus { get; set; }
        }


        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> salheadList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);

                    var BindCompList = db.Company.Include(e => e.ReportingStructRights)
                        .Include(e => e.ReportingStructRights.Select(a => a.ReportingStruct))
                        .Include(e => e.ReportingStructRights.Select(a => a.ReportingStruct.FuncStruct))
                        .Include(e => e.ReportingStructRights.Select(a => a.ReportingStruct.GeoFuncList))
                        .Include(e => e.ReportingStructRights.Select(a => a.ReportingStruct.GeoGraphList))
                    .Where(e => e.Id == company_Id).ToList();

                    foreach (var z in BindCompList)
                    {
                        if (z.ReportingStructRights != null && z.ReportingStructRights.Count > 0)
                        {

                            foreach (var s in z.ReportingStructRights.Select(e => e.ReportingStruct))
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = s.Id,
                                    Empstatus = s.GeoFuncList != null ? s.GeoFuncList.LookupVal : null,
                                    EmpsActingstatus = s.GeoGraphList != null ? s.GeoGraphList.LookupVal : null
                                };
                                model.Add(view);
                            }
                        }
                    }
                    salheadList = model;
                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = salheadList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            if (gp.searchField == "Id")
                                jsonData = IE.Select(a => new { a.Id, a.Empstatus }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                            if (gp.searchField == "FullDetails")
                                jsonData = IE.Select(a => new { a.Id, a.Empstatus }).Where((e => (e.Empstatus.ToString().Contains(gp.searchString)))).ToList();

                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Empstatus != null ? Convert.ToString(a.Empstatus) : "", a.Empstatus != null ? Convert.ToString(a.EmpsActingstatus) : "" }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = salheadList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Code" ? c.Id.ToString() :
                                             gp.sidx == "Empstatus" ? c.Empstatus.ToString() : ""

                                            );
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.Empstatus != null ? Convert.ToString(a.Empstatus) : "", a.Empstatus != null ? Convert.ToString(a.EmpsActingstatus) : "" }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.Empstatus != null ? Convert.ToString(a.Empstatus) : "", a.Empstatus != null ? Convert.ToString(a.EmpsActingstatus) : "" }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Empstatus != null ? Convert.ToString(a.Empstatus) : "", a.Empstatus != null ? Convert.ToString(a.EmpsActingstatus) : "" }).ToList();
                        }
                        totalRecords = salheadList.Count();
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

            //public ActionResult P2BGrid(P2BGrid_Parameters gp)
            //{
            //    try
            //    {
            //        DataBaseContext db = new DataBaseContext();
            //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
            //        int pageSize = gp.rows;
            //        int totalPages = 0;
            //        int totalRecords = 0;
            //        int ParentId = 2;
            //        var jsonData = (Object)null;
            //        var LKVal = db.ReportingStruct.ToList();

            //        if (gp.IsAutho == true)
            //        {
            //            LKVal = db.ReportingStruct.Include(e => e.GeoGraphList).Include(e => e.GeoFuncList).AsNoTracking().ToList();
            //        }
            //        else
            //        {
            //            LKVal = db.ReportingStruct.Include(e => e.GeoGraphList).Include(e => e.GeoFuncList).AsNoTracking().ToList();
            //        }


            //        IEnumerable<ReportingStruct> IE;
            //        if (!string.IsNullOrEmpty(gp.searchString))
            //        {
            //            IE = LKVal;
            //            if (gp.searchOper.Equals("eq"))
            //            {
            //                jsonData = IE.Select(a => new { a.Id, a.GeoGraphList, a.GeoFuncList }).Where((e => (e.Id.ToString() == gp.searchString) || (e.GeoGraphList.LookupVal.ToLower() == gp.searchString) || (e.GeoFuncList.LookupVal.ToLower() == gp.searchString.ToLower())));
            //            }
            //            if (pageIndex > 1)
            //            {
            //                int h = pageIndex * pageSize;
            //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.GeoGraphList.LookupVal, a.GeoFuncList.LookupVal }).ToList();
            //            }
            //            totalRecords = IE.Count();
            //        }
            //        else
            //        {
            //            IE = LKVal;
            //            Func<ReportingStruct, dynamic> orderfuc;
            //            if (gp.sidx == "Id")
            //            {
            //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
            //            }
            //            else
            //            {
            //                orderfuc = (c =>
            //                                 gp.sidx == "GeoGraphList" ? c.GeoGraphList.LookupVal.ToString() :
            //                                 gp.sidx == "GeoFuncList" ? c.GeoFuncList.LookupVal.ToString() :

            //                                 "");
            //            }

            //            if (gp.sord == "asc")
            //            {
            //                IE = IE.OrderBy(orderfuc);
            //                jsonData = IE.Select(a => new Object[] { a.Id, a.GeoGraphList.LookupVal, a.GeoFuncList.LookupVal }).ToList();
            //            }
            //            else if (gp.sord == "desc")
            //            {
            //                IE = IE.OrderByDescending(orderfuc);
            //                jsonData = IE.Select(a => new Object[] { a.Id, a.GeoGraphList.LookupVal, a.GeoFuncList.LookupVal }).ToList();
            //            }
            //            if (pageIndex > 1)
            //            {
            //                int h = pageIndex * pageSize;
            //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.GeoGraphList.LookupVal, a.GeoFuncList.LookupVal }).ToList();
            //            }
            //            totalRecords = LKVal.Count();
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
            //            total = totalPages,
            //            p2bparam = ParentId
            //        };
            //        return Json(JsonData, JsonRequestBehavior.AllowGet);
            //    }
            //    catch (Exception ex)
            //    {
            //        throw ex;
            //    }
            //}

            ////public ActionResult P2BGrid(P2BGrid_Parameters gp)
            ////{
            ////    try
            ////    {
            ////        DataBaseContext db = new DataBaseContext();
            ////        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
            ////        int pageSize = gp.rows;
            ////        int totalPages = 0;
            ////        int totalRecords = 0;
            ////        var jsonData = (Object)null;
            ////        IEnumerable<ReportingStruct> ReportingStruct = null;
            ////        if (gp.IsAutho == true)
            ////        {
            ////            ReportingStruct = db.ReportingStruct.Include(e => e.GeoFuncList).Include(e => e.GeoGraphList).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
            ////        }
            ////        else
            ////        {
            ////            ReportingStruct = db.ReportingStruct.Include(e => e.GeoFuncList).Include(e => e.GeoGraphList).AsNoTracking().ToList();
            ////        }

            ////        IEnumerable<ReportingStruct> IE;
            ////        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
            ////        {
            ////            IE = ReportingStruct;
            ////            if (gp.searchOper.Equals("eq"))
            ////            {
            ////                if (gp.searchField == "Id")
            ////                    jsonData = IE.Select(a => new { a.Id, a.GeoFuncList, a.GeoGraphList }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
            ////                else if (gp.searchField == "GeoFuncList")
            ////                    jsonData = IE.Select(a => new { a.Id, a.GeoFuncList, a.GeoGraphList}).Where((e => (e.GeoFuncList.ToString().Contains(gp.searchString)))).ToList();
            ////                else if (gp.searchField == "GeoGraphList")
            ////                    jsonData = IE.Select(a => new { a.Id, a.GeoFuncList, a.GeoGraphList }).Where((e => (e.GeoGraphList.ToString().Contains(gp.searchString)))).ToList();

            ////                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
            ////            }
            ////            if (pageIndex > 1)
            ////            {
            ////                int h = pageIndex * pageSize;
            ////                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.GeoFuncList !=null ? Convert.ToString(a.GeoFuncList.LookupVal):"", a.GeoGraphList != null ? Convert.ToString(a.GeoGraphList.LookupVal) : "" }).ToList();
            ////            }
            ////            totalRecords = IE.Count();
            ////        }
            ////        else
            ////        {
            ////            IE = ReportingStruct;
            ////            Func<ReportingStruct, dynamic> orderfuc;
            ////            if (gp.sidx == "Id")
            ////            {
            ////                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
            ////            }
            ////            else
            ////            {
            ////                orderfuc = (c => gp.sidx == "GeoFuncList" ? c.GeoFuncList.LookupVal :
            ////                                 gp.sidx == "GeoGraphList" ? c.GeoGraphList.LookupVal : "");
            ////            }
            ////            if (gp.sord == "asc")
            ////            {
            ////                IE = IE.OrderBy(orderfuc);
            ////                jsonData = IE.Select(a => new Object[] { a.Id, a.GeoFuncList != null ? Convert.ToString(a.GeoFuncList.LookupVal) : "", a.GeoGraphList !=null ? Convert.ToString(a.GeoGraphList.LookupVal):"" }).ToList();
            ////            }
            ////            else if (gp.sord == "desc")
            ////            {
            ////                IE = IE.OrderByDescending(orderfuc);
            ////                jsonData = IE.Select(a => new Object[] { a.Id, a.GeoFuncList != null ? Convert.ToString(a.GeoFuncList.LookupVal) : "", a.GeoGraphList != null ? Convert.ToString(a.GeoGraphList.LookupVal) : "" }).ToList();
            ////            }
            ////            if (pageIndex > 1)
            ////            {
            ////                int h = pageIndex * pageSize;
            ////                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.GeoFuncList != null ? Convert.ToString(a.GeoFuncList.LookupVal) : "", a.GeoGraphList != null ? Convert.ToString(a.GeoGraphList.LookupVal) : "" }).ToList();
            ////            }
            ////            totalRecords = ReportingStruct.Count();
            ////        }
            ////        if (totalRecords > 0)
            ////        {
            ////            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
            ////        }
            ////        if (gp.page > totalPages)
            ////        {
            ////            gp.page = totalPages;
            ////        }
            ////        var JsonData = new
            ////        {
            ////            page = gp.page,
            ////            rows = jsonData,
            ////            records = totalRecords,
            ////            total = totalPages
            ////        };
            ////        return Json(JsonData, JsonRequestBehavior.AllowGet);
            ////    }
            ////    catch (Exception ex)
            ////    {
            ////        throw ex;
            ////    }
            ////}


            //[HttpPost]
            //public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
            //{
            //    if (auth_action == "C")
            //    {
            //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //        {
            //            ReportingStruct ESI = db.ReportingStruct
            //                .Include(e => e.GeoFuncList)
            //                .Include(e => e.GeoGraphList)
            //                .FirstOrDefault(e => e.Id == auth_id);

            //            ESI.DBTrack = new DBTrack
            //            {
            //                Action = "C",
            //                ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
            //                CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
            //                CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
            //                IsModified = ESI.DBTrack.IsModified == true ? false : false,
            //                AuthorizedBy = SessionManager.UserName,
            //                AuthorizedOn = DateTime.Now
            //            };

            //            db.ReportingStruct.Attach(ESI);
            //            db.Entry(ESI).State = System.Data.Entity.EntityState.Modified;
            //            db.Entry(ESI).OriginalValues["RowVersion"] = TempData["RowVersion"];
            //            //db.SaveChanges();
            //            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ESI.DBTrack);
            //            DT_JobStatus DT_OBJ = (DT_JobStatus)rtn_Obj;

            //            db.Create(DT_OBJ);
            //            await db.SaveChangesAsync();

            //            ts.Complete();
            //            return Json(new Object[] { ESI.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
            //        }
            //    }
            //    else if (auth_action == "M")
            //    {

            //        ReportingStruct Old_OBJ = db.ReportingStruct
            //                                .Include(e => e.GeoFuncList)
            //                                .Include(e => e.GeoGraphList)
            //                                .Where(e => e.Id == auth_id).SingleOrDefault();


            //        DT_JobStatus Curr_OBJ = db.DT_JobStatus
            //                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
            //                                    .OrderByDescending(e => e.Id)
            //                                    .FirstOrDefault();

            //        if (Curr_OBJ != null)
            //        {
            //            ReportingStruct ReportingStruct = new ReportingStruct();
            //            string GeoFuncList = Curr_OBJ.EmpStatus_Id == null ? null : Curr_OBJ.EmpStatus_Id.ToString();
            //            string GeoGraphList = Curr_OBJ.EmpActingStatus_Id == null ? null : Curr_OBJ.EmpActingStatus_Id.ToString();


            //            //      corp.Id = auth_id;

            //            if (ModelState.IsValid)
            //            {
            //                try
            //                {

            //                    //DbContextTransaction transaction = db.Database.BeginTransaction();

            //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //                    {
            //                        // db.Configuration.AutoDetectChangesEnabled = false;
            //                        ReportingStruct.DBTrack = new DBTrack
            //                        {
            //                            CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
            //                            CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
            //                            Action = "M",
            //                            ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
            //                            ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
            //                            AuthorizedBy = SessionManager.UserName,
            //                            AuthorizedOn = DateTime.Now,
            //                            IsModified = false
            //                        };

            //                        int a = EditS(GeoFuncList, GeoGraphList, auth_id, ReportingStruct, ReportingStruct.DBTrack);

            //                        await db.SaveChangesAsync();

            //                        ts.Complete();
            //                        return Json(new Object[] { ReportingStruct.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
            //                    }
            //                }
            //                catch (DbUpdateConcurrencyException ex)
            //                {
            //                    var entry = ex.Entries.Single();
            //                    var clientValues = (ReportingStruct)entry.Entity;
            //                    var databaseEntry = entry.GetDatabaseValues();
            //                    if (databaseEntry == null)
            //                    {
            //                        return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
            //                    }
            //                    else
            //                    {
            //                        var databaseValues = (ReportingStruct)databaseEntry.ToObject();
            //                        ReportingStruct.RowVersion = databaseValues.RowVersion;
            //                    }
            //                }

            //                return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
            //            }
            //        }
            //        else
            //            return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
            //    }
            //    else if (auth_action == "D")
            //    {
            //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //        {
            //            //ReportingStruct corp = db.ReportingStruct.Find(auth_id);
            //            ReportingStruct ESI = db.ReportingStruct.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

            //            //Address add = corp.Address;
            //            //ContactDetails conDet = corp.ContactDetails;
            //            //SocialActivities val = corp.BusinessType;

            //            ESI.DBTrack = new DBTrack
            //            {
            //                Action = "D",
            //                ModifiedBy = ESI.DBTrack.ModifiedBy != null ? ESI.DBTrack.ModifiedBy : null,
            //                CreatedBy = ESI.DBTrack.CreatedBy != null ? ESI.DBTrack.CreatedBy : null,
            //                CreatedOn = ESI.DBTrack.CreatedOn != null ? ESI.DBTrack.CreatedOn : null,
            //                IsModified = false,
            //                AuthorizedBy = SessionManager.UserName,
            //                AuthorizedOn = DateTime.Now
            //            };

            //            db.ReportingStruct.Attach(ESI);
            //            db.Entry(ESI).State = System.Data.Entity.EntityState.Deleted;


            //            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ESI.DBTrack);
            //            DT_JobStatus DT_OBJ = (DT_JobStatus)rtn_Obj;
            //            //DT_OBJ.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
            //            //DT_OBJ.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
            //            //DT_OBJ.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
            //            db.Create(DT_OBJ);
            //            await db.SaveChangesAsync();
            //            db.Entry(ESI).State = System.Data.Entity.EntityState.Detached;
            //            ts.Complete();
            //            return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
            //        }

            //    }
            //    return View();

            //}
        }
    }
}