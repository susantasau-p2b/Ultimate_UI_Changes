using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Recruitment;
using System.Web.Mvc;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System.Transactions;
using P2b.Global;
using P2BUltimate.Security;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using Recruitment;


namespace P2BUltimate.Controllers.Recruitment.MainController
{
    public class SelectionPanelController : Controller
    {
        List<String> Msg = new List<String>();
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /SelectionPanel/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Recruitement/_SelectionPanel.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(SelectionPanel c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    c.Employee = null;
                    List<P2b.Global.Employee> OBJ1 = new List<P2b.Global.Employee>();
                    string Values1 = form["Employeelist"];

                    if (Values1 != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values1);
                        foreach (var ca in ids)
                        {
                            var OBJ_val1 = db.Employee.Find(ca);
                            OBJ1.Add(OBJ_val1);
                            c.Employee = OBJ1;
                        }
                    }

                    c.ExternalSelector = null;
                    List<P2b.Global.NameSingle> OBJ = new List<P2b.Global.NameSingle>();
                    string Values = form["ExternalSelectorlist"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.NameSingle.Find(ca);
                            OBJ.Add(OBJ_val);
                            c.ExternalSelector = OBJ;
                        }
                    }

                    string Category = form["panelCategorylist"] == "0" ? "" : form["panelCategorylist"];


                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            c.PanelType = val;
                        }
                    }



                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            SelectionPanel corporate = new SelectionPanel()
                            {

                                Employee = c.Employee,
                                ExternalSelector = c.ExternalSelector,
                                PanelName = c.PanelName,
                                MaxPoints = c.MaxPoints,
                                PanelType = c.PanelType,
                                SelectionCriteria = c.SelectionCriteria,
                                DBTrack = c.DBTrack
                            };

                            db.SelectionPanel.Add(corporate);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", null, db.ChangeTracker, c.DBTrack);
                            //DT_SelectionPanel DT_Corp = (DT_SelectionPanel)rtn_Obj;
                            //DT_Corp.PanelType_Id = c.PanelType == null ? 0 : c.PanelType.Id;
                            //DT_Corp. = c.BusinessType == null ? 0 : c.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                            // db.Create(DT_Corp);
                            db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { Id = corporate.Id, Val = corporate.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        public class Employee
        {
            public Array RE_id { get; set; }
            public Array RE_val { get; set; }
        }
        public class NameSingle
        {
            public Array NS_id { get; set; }
            public Array NM_val { get; set; }
        }
        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            List<Employee> return_data = new List<Employee>();

            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.SelectionPanel.Include(e => e.Employee).Include(e => e.Employee.Select(q => q.EmpName)).Where(e => e.Id == data).Select(e => e.Employee.Select(q => q.EmpName)).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new Employee
                {
                    RE_id = ca.Select(e => e.Id.ToString()).ToArray(),
                    RE_val = ca.Select(e => e.FullDetails).ToArray()
                });
                }



                List<NameSingle> return_data2 = new List<NameSingle>();


                var a1 = db.SelectionPanel.Include(e => e.ExternalSelector).Where(e => e.Id == data).Select(e => e.ExternalSelector).ToList();

                foreach (var ca in a1)
                {
                    return_data2.Add(
                new NameSingle
                {
                    NS_id = ca.Select(e => e.Id.ToString()).ToArray(),
                    NM_val = ca.Select(e => e.FullDetails).ToArray()
                });
                }

                var Q = db.SelectionPanel

                   .Include(e => e.Employee)
                   .Include(e => e.ExternalSelector)

                   .Where(e => e.Id == data).Select
                   (e => new
                   {

                       MaxPoints = e.MaxPoints,
                       PanelName = e.PanelName,
                       SelectionCriteria = e.SelectionCriteria,
                       PanelType_Id = e.PanelType.Id == null ? 0 : e.PanelType.Id,
                       Action = e.DBTrack.Action
                   }).ToList();




                var Corp = db.SelectionPanel.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, return_data2, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(SelectionPanel c, int data, FormCollection form) // Edit submit
        {

            // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
            //  bool Auth = form["Autho_Action"] == "" ? false : true;
            using (DataBaseContext db = new DataBaseContext())
            {
                bool Auth = form["Autho_Allow"] == "true" ? true : false;
                string Corp = form["panelCategorylist"] == "0" ? "" : form["panelCategorylist"];

                var db_Data = db.SelectionPanel.Include(e => e.Employee).Include(e => e.ExternalSelector).Include(e => e.PanelType).Where(e => e.Id == data).SingleOrDefault();
                db_Data.Employee = null;
                db_Data.ExternalSelector = null;
                db_Data.MaxPoints = c.MaxPoints;

                db_Data.PanelName = c.PanelName;
                db_Data.SelectionCriteria = c.SelectionCriteria;

                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        c.PanelType = val;
                    }
                }
                db_Data.PanelType = c.PanelType;
                List<P2b.Global.Employee> job_agency = new List<P2b.Global.Employee>();
                string j_agency = form["Employeelist"];

                if (j_agency != null)
                {
                    var ids = Utility.StringIdsToListIds(j_agency);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.Employee.Find(ca);

                        job_agency.Add(Lookup_val);
                        db_Data.Employee = job_agency;
                    }
                }
                else
                {
                    db_Data.Employee = null;
                }

                List<P2b.Global.NameSingle> jobinside = new List<P2b.Global.NameSingle>();
                string j_inside = form["ExternalSelectorlist"];

                if (j_inside != null)
                {
                    var ids = Utility.StringIdsToListIds(j_inside);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.NameSingle.Find(ca);

                        jobinside.Add(Lookup_val);
                        db_Data.ExternalSelector = jobinside;
                    }
                }
                else
                {
                    db_Data.ExternalSelector = null;
                }




                if (Auth == false)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {


                                using (var context = new DataBaseContext())
                                {
                                    db.SelectionPanel.Attach(db_Data);
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_Data.RowVersion;
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_OBJ = db.SelectionPanel.Find(data);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        SelectionPanel blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;


                                        blog = context.SelectionPanel.Where(e => e.Id == data).Include(e => e.Employee)
                                                                .Include(e => e.ExternalSelector)
                                             .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;


                                        c.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                        SelectionPanel lk = new SelectionPanel
                                        {
                                            Id = data,

                                            Employee = db_Data.Employee,
                                            ExternalSelector = db_Data.ExternalSelector,
                                            PanelName = db_Data.PanelName,
                                            MaxPoints = db_Data.MaxPoints,
                                            PanelType = db_Data.PanelType,
                                            SelectionCriteria = db_Data.SelectionCriteria,
                                            DBTrack = c.DBTrack
                                        };


                                        db.SelectionPanel.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                        // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                        //db.SaveChanges();
                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                        var obj = DBTrackFile.DBTrackSave("Recruitment/Recruitment", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //DT_SelectionPanel DT_LK = (DT_SelectionPanel)obj;
                                        //DT_LK.PanelType_Id = c.PanelType == null ? 0 : c.PanelType.Id;
                                        //db.Create(DT_LK);
                                        db.SaveChanges();
                                        await db.SaveChangesAsync();
                                        var aaq = db.SelectionPanel.Include(e => e.PanelType).Where(e => e.Id == data).SingleOrDefault();
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        Msg.Add("Record Updated Successfully.");
                                        return Json(new Utility.JsonReturnClass { Id = aaq.Id, Val = aaq.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        // return Json(new Object[] { aaq.Id, aaq.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (SelectionPanel)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (SelectionPanel)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;

                            }
                        }

                        return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        SelectionPanel blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        SelectionPanel Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.SelectionPanel.Where(e => e.Id == data).SingleOrDefault();
                            originalBlogValues = context.Entry(blog).OriginalValues;
                        }
                        c.DBTrack = new DBTrack
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

                        SelectionPanel corp = new SelectionPanel()
                        {

                            Id = data,
                            PanelName = db_Data.PanelName,
                            MaxPoints = db_Data.MaxPoints,
                            PanelType = db_Data.PanelType,
                            SelectionCriteria = db_Data.SelectionCriteria,
                            DBTrack = c.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };


                        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Recruitment/Recruitment", "M", blog, corp, "SelectionPanel", c.DBTrack);
                            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            Old_Corp = context.SelectionPanel.Where(e => e.Id == data).Include(e => e.Employee)
                                .Include(e => e.ExternalSelector).Include(e => e.PanelType).Include(e => e.MaxPoints)
                                .Include(e => e.PanelName).Include(e => e.SelectionCriteria).SingleOrDefault();
                            DT_SelectionPanel DT_Corp = (DT_SelectionPanel)obj;
                            DT_Corp.PanelType_Id = DBTrackFile.ValCompare(Old_Corp.PanelType, c.PanelType);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                            //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();
                        }
                        blog.DBTrack = c.DBTrack;
                        db.SelectionPanel.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        var aaq = db.SelectionPanel.Include(e => e.PanelType).Where(e => e.Id == data).SingleOrDefault();
                        ts.Complete();
                        Msg.Add("Record Updated Successfully.");
                        return Json(new Utility.JsonReturnClass { Id = aaq.Id, Val = aaq.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                }
                return View();
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    SelectionPanel jobsource = db.SelectionPanel.Include(e => e.Employee)
                        .Include(e => e.ExternalSelector)
                         .Include(e => e.PanelType)


                                                        .Where(e => e.Id == data).SingleOrDefault();



                    if (jobsource.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = jobsource.DBTrack.CreatedBy != null ? jobsource.DBTrack.CreatedBy : null,
                                CreatedOn = jobsource.DBTrack.CreatedOn != null ? jobsource.DBTrack.CreatedOn : null,
                                IsModified = jobsource.DBTrack.IsModified == true ? true : false
                            };
                            jobsource.DBTrack = dbT;
                            db.Entry(jobsource).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, jobsource.DBTrack);
                            DT_SelectionPanel DT_OBJ = (DT_SelectionPanel)rtn_Obj;

                            db.Create(DT_OBJ);

                            await db.SaveChangesAsync();
                            ts.Complete();

                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {


                            var lkValue1 = new HashSet<int>(jobsource.Employee.Select(e => e.Id));
                            if (lkValue1.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            }

                            var lkValue2 = new HashSet<int>(jobsource.ExternalSelector.Select(e => e.Id));
                            if (lkValue2.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            }



                            db.Entry(jobsource).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    db.Entry(jobsource).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                    // ts.Complete();

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
                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
            }
        }

        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Employee
                      .Include(e => e.EmpName)
                      .Include(e => e.ServiceBookDates)
                      .Where(e => e.ServiceBookDates.ServiceLastDate == null)
                                     .ToList();
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
            // return View();
        }

        public ActionResult GetNameSingleLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var EmpassignNamesingle = db.Employee.Include(e => e.EmpName).Select(e => e.EmpName.Id).ToList();

                var fall = db.NameSingle.Include(e => e.EmpTitle).Where(e => !EmpassignNamesingle.Contains(e.Id))
                                     .ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.NameSingle.Include(e => e.EmpTitle).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }
    }
}