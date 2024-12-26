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
    public class ExitProcess_CheckList_PolicyController : Controller
    {
        //
        // GET: /ExitProcess_CheckList_Policy/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ExitProcess_CheckList_Policy/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View();
        }



        public ActionResult GetExitProcessCheckListPolicyDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.ExitProcess_CheckList_Policy.Include(e => e.ExitProcess_CheckList_Object).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ExitProcess_CheckList_Policy.Include(e => e.ExitProcess_CheckList_Object).Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "Checklist Name:" + ca.ChecklistName }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }




        public ActionResult Create(ExitProcess_CheckList_Policy c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //string ChecklistName = form["ChecklistName"] == "0" ? "" : form["ChecklistName"];
                //string ExitProcess_CheckList_Object = form["IsFFSDocExitProcess_CheckList_ObjectAppl"] == "0" ? "" : form["ExitProcess_CheckList_Object"];
                //string IsNoDuesAppl = form["IsNoDuesAppl"] == "0" ? "" : form["IsNoDuesAppl"];
                //string IsPartPayAppl = form["IsPartPayAppl"] == "0" ? "" : form["IsPartPayAppl"];
                //string IsNoticePeriodAppl = form["IsNoticePeriodAppl"] == "0" ? "" : form["IsNoticePeriodAppl"];
                //string IsRefDocAppl = form["IsRefDocAppl"] == "0" ? "" : form["IsRefDocAppl"];
                //string IsResignRequestAppl = form["IsResignRequestAppl"] == "0" ? "" : form["IsResignRequestAppl"];
                //string IsExitInterviewAppl = form["IsExitInterviewAppl"] == "0" ? "" : form["IsExitInterviewAppl"];
                List<String> Msg = new List<String>();
                try
                {
                    string ExitProcess_CheckList_ObjectList = form["ExitProcess_CheckList_ObjectList"] == "0" ? "" : form["ExitProcess_CheckList_ObjectList"];
                    List<ExitProcess_CheckList_Object> ObjExitProcess_CheckList_Object = new List<ExitProcess_CheckList_Object>();
                    if (ExitProcess_CheckList_ObjectList != null && ExitProcess_CheckList_ObjectList != "")
                    {
                       
                        var ids = Utility.StringIdsToListIds(ExitProcess_CheckList_ObjectList);
                        foreach (var ca in ids)
                        {
                            var value = db.ExitProcess_CheckList_Object.Find(ca);
                            ObjExitProcess_CheckList_Object.Add(value);
                            c.ExitProcess_CheckList_Object = ObjExitProcess_CheckList_Object;
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

                            ExitProcess_CheckList_Policy sep = new ExitProcess_CheckList_Policy()
                            {
                                ChecklistName = c.ChecklistName,
                                ExitProcess_CheckList_Object = c.ExitProcess_CheckList_Object,
                                //IsFFSDocAppl = c.IsFFSDocAppl,
                                //IsNoDuesAppl = c.IsNoDuesAppl,
                                //IsNoticePeriodAppl = c.IsNoticePeriodAppl,
                                //IsPartPayAppl = c.IsPartPayAppl,
                                //IsRefDocAppl = c.IsRefDocAppl,
                                //IsResignRequestAppl = c.IsResignRequestAppl,
                                //ProcessName = c.ProcessName,
                                DBTrack = c.DBTrack
                            };

                            db.ExitProcess_CheckList_Policy.Add(sep); 
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
                IEnumerable<ExitProcess_CheckList_Policy> Corporate = null;
                if (gp.IsAutho == true)
                {
                    Corporate = db.ExitProcess_CheckList_Policy.Include(e => e.ExitProcess_CheckList_Object).Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    Corporate = db.ExitProcess_CheckList_Policy
                                .Include(e => e.ExitProcess_CheckList_Object).ToList();
                }

                IEnumerable<ExitProcess_CheckList_Policy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Corporate;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.ChecklistName != null ? e.ChecklistName.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                            //|| (e.CheckListItemDesc != null ? e.CheckListItemDesc.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                            //|| (e.ProcessConfigName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString))
                              ).Select(a => new Object[] { a.ChecklistName != null ? a.ChecklistName : "", a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ChecklistName, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Corporate;
                    Func<ExitProcess_CheckList_Policy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "ChecklistName" ? c.ChecklistName != null ? c.ChecklistName : "" :

                                          "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.ChecklistName, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.ChecklistName, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ChecklistName, a.Id }).ToList();
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


        //public ActionResult GetLookupExitProcessCheckListPolicy(List<int> SkipIds)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.ExitProcess_CheckList_Policy.Include(e => e.ExitProcess_CheckList_Object).ToList();

        //        // IEnumerable<WeeklyOffCalendar> all;
        //        if (SkipIds != null)
        //        {
        //            foreach (var a in SkipIds)
        //            {
        //                if (fall == null)
        //                    fall = db.ExitProcess_CheckList_Policy.Include(e => e.ExitProcess_CheckList_Object)
        //                        .Where(e => e.Id != a).ToList();
        //                else
        //                    fall = fall.Where(e => e.Id != a).ToList();
        //            }
        //        }

        //        var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.ChecklistName + ",Checklist Name:" }).Distinct();
        //        //var result_1 = (from c in fall
        //        //                select new { c.Id, c.DepartmentCode, c.DepartmentName });
        //        return Json(r, JsonRequestBehavior.AllowGet);

        //    }
        //    // return View();
        //}

        public class ExitprocessPolicyEditDetails
        {
            public Array ExitProcessObj_Id { get; set; }

            public Array ExitProcessObj_FullDetails { get; set; }


        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
              
                List<ExitprocessPolicyEditDetails> return_data = new List<ExitprocessPolicyEditDetails>();
                var ExitProcPolicy = db.ExitProcess_CheckList_Policy
                    .Include(e => e.ExitProcess_CheckList_Object)
                    .Where(e => e.Id == data).ToList();
                var r = (from ca in ExitProcPolicy
                         select new
                         {
                             Id = ca.Id,
                             ChecklistName = ca.ChecklistName,
                             Action = ca.DBTrack.Action
                         }).Distinct();


                var a = db.ExitProcess_CheckList_Policy.Include(e => e.ExitProcess_CheckList_Object).Where(e => e.Id == data)
                    //.Include(e => e.Equals)
                    .Select(e => e.ExitProcess_CheckList_Object).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new ExitprocessPolicyEditDetails
                {
                    ExitProcessObj_Id  = ca.Select(e => e.Id.ToString()).ToArray(),
                    ExitProcessObj_FullDetails = ca.Select(e => e.CheckListItemDesc.ToString()).ToArray()
                });
                }

                var Corp = db.ExitProcess_CheckList_Policy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                

                

                return Json(new Object[] { r, return_data, null, Auth, JsonRequestBehavior.AllowGet });

            }

        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(ExitProcess_CheckList_Policy ESOBJ, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    var db_data = db.ExitProcess_CheckList_Policy.Include(e => e.ExitProcess_CheckList_Object).Where(e => e.Id == data).SingleOrDefault();
                    List<ExitProcess_CheckList_Object> ExitProcess_CheckList_Obj = new List<ExitProcess_CheckList_Object>();
                    string Values = form["ExitProcess_CheckList_ObjectList"];

                    if (Values != null)
                    {
                        var ids = one_ids(Values);
                        foreach (var ca in ids)
                        {
                            var ExitProcObj_val = db.ExitProcess_CheckList_Object.Find(ca);
                            ExitProcess_CheckList_Obj.Add(ExitProcObj_val);
                            db_data.ExitProcess_CheckList_Object = ExitProcess_CheckList_Obj;
                        }
                    }
                    else
                    {
                        db_data.ExitProcess_CheckList_Object = null;
                    }

                    db.ExitProcess_CheckList_Policy.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["RowVersion"] = db_data.RowVersion;
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;


                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                try
                                {
                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    ExitProcess_CheckList_Policy blog = null; // to retrieve old data                           
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ExitProcess_CheckList_Policy.Where(e => e.Id == data)
                                                                .Include(e => e.ExitProcess_CheckList_Object).SingleOrDefault();
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
                                    //  int a = EditS(Values, data, ESOBJ, ESOBJ.DBTrack);

                                    if (Values != null)
                                    {
                                        if (Values != "")
                                        {

                                            List<int> IDs = Values.Split(',').Select(e => int.Parse(e)).ToList();
                                            foreach (var k in IDs)
                                            {
                                                var value = db.ExitProcess_CheckList_Object.Find(k);
                                                ESOBJ.ExitProcess_CheckList_Object = new List<ExitProcess_CheckList_Object>();
                                                ESOBJ.ExitProcess_CheckList_Object.Add(value);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var ExitProcObjdetails = db.ExitProcess_CheckList_Policy.Include(e => e.ExitProcess_CheckList_Object).Where(x => x.Id == data).ToList();
                                        foreach (var s in ExitProcObjdetails)
                                        {
                                            s.ExitProcess_CheckList_Object = null;
                                            db.ExitProcess_CheckList_Policy.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    var CurOBJ = db.ExitProcess_CheckList_Policy.Find(data);
                                    TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                                    db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        ESOBJ.DBTrack = ESOBJ.DBTrack;
                                        ExitProcess_CheckList_Policy TOBJ = new ExitProcess_CheckList_Policy()
                                        {
                                            ChecklistName = ESOBJ.ChecklistName,
                                            Id = data,
                                            DBTrack = ESOBJ.DBTrack
                                        };


                                        db.ExitProcess_CheckList_Policy.Attach(TOBJ);
                                        db.Entry(TOBJ).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(TOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                    }

                                    await db.SaveChangesAsync();
                                    //using (var context = new DataBaseContext())
                                    //{

                                    //    //To save data in history table 
                                    //    var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ESOBJ, "Grade", ESOBJ.DBTrack);
                                    //    DT_Grade DT_GRD = (DT_Grade)Obj;
                                    //    db.DT_Grade.Add(DT_GRD);
                                    //    db.SaveChanges();
                                    //}
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.ChecklistName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return Json(new Object[] { ESOBJ.Id, ESOBJ.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                }

                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Grade)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (Grade)databaseEntry.ToObject();
                                        ESOBJ.RowVersion = databaseValues.RowVersion;
                                    }
                                }

                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                    }
                    else
                    {


                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            ExitProcess_CheckList_Policy blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Grade Old_OBJ = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.ExitProcess_CheckList_Policy.Where(e => e.Id == data).SingleOrDefault();
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
                            ExitProcess_CheckList_Policy OBJ = new ExitProcess_CheckList_Policy()
                            {

                                Id = data,
                                ChecklistName = ESOBJ.ChecklistName,
                                DBTrack = ESOBJ.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            //using (var context = new DataBaseContext())
                            //{
                            //    var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, OBJ, "Grade", ESOBJ.DBTrack);
                            //    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            //    Old_OBJ = context.Grade.Where(e => e.Id == data)
                            //       .Include(e => e.Levels).SingleOrDefault();
                            //    DT_Grade DT_OBJ = (DT_Grade)obj;

                            //    // DT_OBJ.InstituteType_Id = DBTrackFile.ValCompare(Old_OBJ.IsManualRotateShift, ESOBJ.IsManualRotateShift); //Old_OBJ.BusinessType == c.BusinessType ? 0 : Old_OBJ.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_OBJ.BusinessType.Id;
                            //    DT_OBJ.Levels_Id = DBTrackFile.ValCompare(Old_OBJ.Levels, ESOBJ.); //Old_OBJ.Levels == c.Levels ? 0 : Old_OBJ.Levels == null && c.Levels != null ? c.Levels.Id : Old_OBJ.Levels.Id;
                            //    db.Create(DT_OBJ);
                            //    //db.SaveChanges();
                            //}
                            blog.DBTrack = ESOBJ.DBTrack;
                            db.ExitProcess_CheckList_Policy.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = OBJ.ChecklistName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { , , "Record Updated", JsonRequestBehavior.AllowGet });
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
                    ExitProcess_CheckList_Policy ExitProcess_CheckList_Policy = db.ExitProcess_CheckList_Policy

                                                       .Where(e => e.Id == data).SingleOrDefault();

                    db.Entry(ExitProcess_CheckList_Policy).State = System.Data.Entity.EntityState.Deleted;
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