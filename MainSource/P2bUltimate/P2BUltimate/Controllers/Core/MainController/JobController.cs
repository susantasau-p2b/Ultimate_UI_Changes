///
/// Created by Tanushri
///

using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Threading.Tasks;
using Attendance;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Core.MainController
{
     [AuthoriseManger]
    public class JobController : Controller
    {

//private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Job/Index.cshtml");
        }



        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
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


                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);

                    var BindCompList = db.Company.Include(e => e.Job).Where(e => e.Id == company_Id).ToList();

                    foreach (var z in BindCompList)
                    {
                        if (z.Job != null && z.Job.Count > 0)
                        {

                            foreach (var s in z.Job)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = s.Id,
                                    Code = s.Code,
                                    Name = s.Name

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
                            jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                               || (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                               ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code != null ? Convert.ToString(a.Code) : "", a.Name != null ? Convert.ToString(a.Name) : "", a.Id }).ToList();
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
                            orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
                                             gp.sidx == "Name" ? c.Name.ToString() : ""

                                            );
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code != null ? Convert.ToString(a.Code) : "", a.Name != null ? Convert.ToString(a.Name) : "", a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code != null ? Convert.ToString(a.Code) : "", a.Name != null ? Convert.ToString(a.Name) : "", a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code != null ? Convert.ToString(a.Code) : "", a.Name != null ? Convert.ToString(a.Name) : "", a.Id }).ToList();
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
        //        var LKVal = db.Job.ToList();

        //        if (gp.IsAutho == true)
        //        {
        //            LKVal = db.Job.AsNoTracking().Where(e => e.DBTrack.IsModified == true).ToList();
        //        }
        //        else
        //        {
        //            LKVal = db.Job.AsNoTracking().ToList();
        //        }


        //        IEnumerable<Job> IE;
        //        if (!string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = LKVal;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToString() == gp.searchString.ToLower()) || (e.Name.ToString() == gp.searchString.ToLower())));
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = LKVal;
        //            Func<Job, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
        //                                 gp.sidx == "Name " ? c.Name.ToString() :
        //                                 "");
        //            }

        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
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


        public ActionResult EditContactDetails_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var val = db.JobPosition.Where(e => e.Id == data).SingleOrDefault();

                var r = (from ca in db.JobPosition
                         select new
                         {
                             Id = ca.Id,
                             FullDetails = ca.JobPositionDesc,
                         }).Where(e => e.Id == data).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult GetPositionDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.JobPosition.ToList();
        //        IEnumerable<Job> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Job.ToList().Where(d => d.Name.Contains(data));

        //        }
        //        else
        //        {
        //            //var list1 = db.Job.ToList().SelectMany(e => e.JobPosition);
        //            //var list2 = fall.Except(list1);
        //            //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
        //            //var list1 = db.Job.Include(e => e.JobPosition).SelectMany(e => e.JobPosition);
        //            //var list2 = fall.Except(list1);
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.JobPositionDesc }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.CorporateCode, c.CorporateName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.Name }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    // return View();
        //}

        public ActionResult GetPositionDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.JobPosition.ToList();


                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.JobPosition.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.JobPositionDesc }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        //public ActionResult GetLookupDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        IEnumerable<JobPosition>all;
        //        var fall = db.JobPosition.ToList();
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.JobPosition.ToList().Where(d => d.StartingSlab.ToString().Contains(data));
        //            var result = (from c in all
        //                          select new { c.Id, c.StartingSlab }).Distinct();
        //            return Json(result, JsonRequestBehavior.AllowGet);

        //       }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.StartingSlab }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.CorporateCode, c.CorporateName });
        //            return Json(r, JsonRequestBehavior.AllowGet);

        //        }

        //    }
        //    // return View();
        //}


        private MultiSelectList GetContactDetailsValues(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<JobPosition> lkval = new List<JobPosition>();
                lkval = db.JobPosition.ToList();
                return new MultiSelectList(lkval, "Id", "JobPositionDesc", selectedValues);
            }
        }




        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(Job NOBJ, FormCollection form)
        {
             List<string> Msg = new List<string>();
             using (DataBaseContext db = new DataBaseContext())
             {
                 try
                 {
                     int comp_Id = 0;
                     comp_Id = Convert.ToInt32(Session["CompId"]);
                     var Company = new Company();
                     Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
                     NOBJ.JobPosition = null;
                     List<JobPosition> OBJ = new List<JobPosition>();
                     string Values = form["PostionList"];

                     if (Values != null)
                     {
                         var ids = one_ids(Values);
                         foreach (var ca in ids)
                         {
                             var OBJ_val = db.JobPosition.Find(ca);
                             OBJ.Add(OBJ_val);
                             NOBJ.JobPosition = OBJ;
                         }
                     }

                     //string Values = form["PostionList"];
                     //if (Values != null)
                     //{
                     //    List<int> IDs = Values.Split(',').Select(s => int.Parse(s)).ToList();
                     //    ViewBag.JobPosition = GetContactDetailsValues(IDs);
                     //}
                     //else
                     //{
                     //    ViewBag.JobPosition = GetContactDetailsValues(null);
                     //}

                     if (ModelState.IsValid)
                     {
                         using (TransactionScope ts = new TransactionScope())
                         {

                             if (db.Job.Any(o => o.Code == NOBJ.Code))
                             {
                                 Msg.Add("Code Name Already Exists.  ");
                                 return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                 //return this.Json(new { msg = "Job Name already exists." });
                             }

                             NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                             Job Job = new Job()
                             {
                                 Name = NOBJ.Name == null ? "" : NOBJ.Name.Trim(),
                                 Code = NOBJ.Code == null ? "" : NOBJ.Code.Trim(),
                                 JobPosition = NOBJ.JobPosition,
                                 DBTrack = NOBJ.DBTrack
                             };
                             try
                             {
                                 //db.Job.Add(Job);
                                 ////DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, NOBJ.DBTrack);
                                 //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", Job, null, "Job", null);
                                 //db.SaveChanges();
                                 //ts.Complete();
                                 //return Json(new Object[] { Job.Id, Job.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });


                                 db.Job.Add(Job);
                                 var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, NOBJ.DBTrack);
                                 DT_Job DT_OBJ = (DT_Job)rtn_Obj;
                                 db.Create(DT_OBJ);
                                 db.SaveChanges();
                                 if (Company != null)
                                 {
                                     var Objjob = new List<Job>();
                                     Objjob.Add(Job);
                                     Company.Job = Objjob;
                                     db.Company.Attach(Company);
                                     db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                     db.SaveChanges();
                                     db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

                                 }

                                 ts.Complete();
                                 Msg.Add("  Data Saved successfully  ");
                                 return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                 //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                             }

                             catch (DbUpdateConcurrencyException)
                             {
                                 return RedirectToAction("Create", new { concurrencyError = true, id = NOBJ.Id });
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
                         //var errorMsg = sb.ToString();
                         //return this.Json(new { msg = errorMsg });
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


        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }



        public int EditS(string Lkval, string OBJ, int data, Job ESOBJ, DBTrack dbT)
        {
            //if (Lkval != null)
            //{
            //    if (Lkval != "")
            //    {
            //        var val = db.LookupValue.Find(int.Parse(Lkval));
            //        ESOBJ.IsManualRotateShift = val;

            //        var type = db.Job.Include(e => e.IsManualRotateShift).Where(e => e.Id == data).SingleOrDefault();
            //        IList<Job> typedetails = null;
            //        if (type.IsManualRotateShift != null)
            //        {
            //            typedetails = db.Job.Where(x => x.IsManualRotateShift.Id == type.IsManualRotateShift.Id && x.Id == data).ToList();
            //        }
            //        else
            //        {
            //            typedetails = db.Job.Where(x => x.Id == data).ToList();
            //        }
            //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
            //        foreach (var s in typedetails)
            //        {
            //            s.IsManualRotateShift = ESOBJ.IsManualRotateShift;
            //            db.Job.Attach(s);
            //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //            //await db.SaveChangesAsync();
            //            db.SaveChanges();
            //            TempData["RowVersion"] = s.RowVersion;
            //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //        }
            //    }
            //    else
            //    {
            //        var InstituteTypeDetails = db.Job.Include(e => e.IsManualRotateShift).Where(x => x.Id == data).ToList();
            //        foreach (var s in InstituteTypeDetails)
            //        {
            //            s.IsManualRotateShift = null;
            //            db.Job.Attach(s);
            //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //            //await db.SaveChangesAsync();
            //            db.SaveChanges();
            //            TempData["RowVersion"] = s.RowVersion;
            //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //        }
            //    }
            //}
            //else
            //{
            //    var InstituteTypeDetails = db.Job.Include(e => e.IsManualRotateShift).Where(x => x.Id == data).ToList();
            //    foreach (var s in InstituteTypeDetails)
            //    {
            //        s.IsManualRotateShift = null;
            //        db.Job.Attach(s);
            //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //        //await db.SaveChangesAsync();
            //        db.SaveChanges();
            //        TempData["RowVersion"] = s.RowVersion;
            //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //    }
            //}

            //var db_data = db.Job.Include(e => e.JobPosition).Where(e => e.Id == data).SingleOrDefault();
            //List<JobPosition> lookupval = new List<JobPosition>();
            //string Values = OBJ;

            //if (Values != null)
            //{
            //    var ids = one_ids(Values);
            //    foreach (var ca in ids)
            //    {
            //        var Lookup_val = db.JobPosition.Find(ca);
            //        lookupval.Add(Lookup_val);
            //        db_data.JobPosition = lookupval;
            //    }
            //}
            //else
            //{
            //    db_data.JobPosition = null;
            //}
            using (DataBaseContext db = new DataBaseContext())
            {
                if (OBJ != null)
                {
                    if (OBJ != "")
                    {

                        List<int> IDs = OBJ.Split(',').Select(e => int.Parse(e)).ToList();
                        foreach (var k in IDs)
                        {
                            var value = db.JobPosition.Find(k);
                            ESOBJ.JobPosition = new List<JobPosition>();
                            ESOBJ.JobPosition.Add(value);
                        }
                    }
                }
                else
                {
                    var Details = db.Job.Include(e => e.JobPosition).Where(x => x.Id == data).ToList();
                    foreach (var s in Details)
                    {
                        s.JobPosition = null;
                        db.Job.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                //db.Job.Attach(db_data);
                //db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                //TempData["RowVersion"] = db_data.RowVersion;
                //db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;



                var CurOBJ = db.Job.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESOBJ.DBTrack = dbT;
                    Job BOBJ = new Job()
                    {
                        Name = ESOBJ.Name,
                        Code = ESOBJ.Code,
                        Id = data,
                        DBTrack = ESOBJ.DBTrack
                    };


                    db.Job.Attach(BOBJ);
                    db.Entry(BOBJ).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(BOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    ////  DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }


        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            Job Job = db.Job.Include(e => e.JobPosition)
                              .FirstOrDefault(e => e.Id == auth_id);
                            Job.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = Job.DBTrack.ModifiedBy != null ? Job.DBTrack.ModifiedBy : null,
                                CreatedBy = Job.DBTrack.CreatedBy != null ? Job.DBTrack.CreatedBy : null,
                                CreatedOn = Job.DBTrack.CreatedOn != null ? Job.DBTrack.CreatedOn : null,
                                IsModified = Job.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };
                            db.Job.Attach(Job);
                            db.Entry(Job).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(Job).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            //using (var context = new DataBaseContext())
                            //{
                            //    //  Lkval = db.Job.Include(e => e.JobPosition)
                            //    //.Include(e => e.JobPosition)
                            //    //.Include(e => e.IsManualRotateShift).FirstOrDefault(e => e.Id == auth_id);
                            //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Job, null, "Job", Job.DBTrack);
                            //}
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Job.DBTrack);
                            DT_Job DT_OBJ = (DT_Job)rtn_Obj;

                            //DT_OBJ.InstituteType_Id = Job.IsManualRotateShift == null ? 0 : Job.IsManualRotateShift.Id;
                            // DT_OBJ.Position_Id = Job.JobPosition == null ? 0 : Job.JobPosition.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();


                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = Job.Id, Val = Job.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { ,, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {
                        Job Old_OBJ = db.Job
                                                          .Include(e => e.JobPosition).Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_Job Curr_OBJ = db.DT_Job
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_OBJ != null)
                        {
                            Job Job = new Job();
                            string LKVAL = "";
                            //string LKVAL = Curr_OBJ.InstituteType_Id == null ? null : Curr_OBJ.InstituteType_Id.ToString();
                            string CONVAL = Curr_OBJ.Position_Id == null ? null : Curr_OBJ.Position_Id.ToString();

                            Job.Name = Curr_OBJ.Name == null ? Old_OBJ.Name : Curr_OBJ.Name;
                            Job.Code = Curr_OBJ.Code == null ? Old_OBJ.Code : Curr_OBJ.Code;
                            //Job.IsAutoShift = Curr_OBJ.IsAutoShift == null ? Old_OBJ.IsAutoShift : Curr_OBJ.IsAutoShift;
                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        Job.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                                            CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        int a = EditS(LKVAL, CONVAL, auth_id, Job, Job.DBTrack);


                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = Job.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { Job.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Job)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (Job)databaseEntry.ToObject();
                                        Job.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                        {
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        //eturn Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });

                        //Job Old_OBJ = db.Job.Include(e => e.JobPosition).Include(e => e.IsManualRotateShift)
                        //                                  .Where(e => e.Id == auth_id).SingleOrDefault();

                        //// DT_Job Curr_OBJ = db.DT_Job.Include(e => e.JobPosition)  
                        //DT_Job Curr_OBJ = db.DT_Job
                        //                      .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                        //                      .OrderByDescending(e => e.Id)
                        //                      .FirstOrDefault();

                        //Job Job = new Job();

                        //string JobPosition = Curr_OBJ.Position_Id== null ? null : Curr_OBJ.Position_Id.ToString();
                        //string INType = Curr_OBJ.InstituteType_Id == null ? null : Curr_OBJ.InstituteType_Id.ToString();

                        ////Lkval.Name = Curr_OBJ.Name == null ? Old_OBJ.Name : Curr_OBJ.Name;
                        ////Lkval.Code = Curr_OBJ.Code == null ? Old_OBJ.Code : Curr_OBJ.Code;
                        ////      Lkval.Id = auth_id;

                        //if (ModelState.IsValid)
                        //{
                        //    try
                        //    {

                        //        //DbContextTransaction transaction = db.Database.BeginTransaction();

                        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        //        {
                        //            // db.Configuration.AutoDetectChangesEnabled = false;
                        //            Job.DBTrack = new DBTrack
                        //            {
                        //                CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                        //                CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                        //                Action = "M",
                        //                ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
                        //                ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
                        //                AuthorizedBy = SessionManager.UserName,
                        //                AuthorizedOn = DateTime.Now,
                        //                IsModified = false
                        //            };

                        //            int a = EditS(INType, JobPosition, auth_id, Job, Job.DBTrack);

                        //            await db.SaveChangesAsync();

                        //            ts.Complete();
                        //            return Json(new Object[] { Job.Id, Job.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                        //        }
                        //    }
                        //    catch (DbUpdateConcurrencyException ex)
                        //    {
                        //        var entry = ex.Entries.Single();
                        //        var clientValues = (Job)entry.Entity;
                        //        var databaseEntry = entry.GetDatabaseValues();
                        //        if (databaseEntry == null)
                        //        {
                        //            return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                        //        }
                        //        else
                        //        {
                        //            var databaseValues = (Job)databaseEntry.ToObject();
                        //            Job.RowVersion = databaseValues.RowVersion;
                        //        }
                        //    }

                        //    return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        //}
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Job Lkval = db.Job.Find(auth_id);
                            Job Job = db.Job.AsNoTracking().Include(e => e.JobPosition)
                                                                        .FirstOrDefault(e => e.Id == auth_id);
                            var selectedValues = Job.JobPosition;
                            Job.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = Job.DBTrack.ModifiedBy != null ? Job.DBTrack.ModifiedBy : null,
                                CreatedBy = Job.DBTrack.CreatedBy != null ? Job.DBTrack.CreatedBy : null,
                                CreatedOn = Job.DBTrack.CreatedOn != null ? Job.DBTrack.CreatedOn : null,
                                IsModified = Job.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };
                            db.Job.Attach(Job);
                            db.Entry(Job).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                Job.JobPosition = selectedValues;
                                // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Job, null, "Job", Job.DBTrack);
                            }
                            db.Entry(Job).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
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
        public async Task<ActionResult> EditSave(Job ESOBJ, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var db_data = db.Job.Include(e => e.JobPosition).Where(e => e.Id == data).SingleOrDefault();
                    List<JobPosition> JobPosition = new List<JobPosition>();
                    string Values = form["PostionList"];

                    if (Values != null)
                    {
                        var ids = one_ids(Values);
                        foreach (var ca in ids)
                        {
                            var ContactDetails_val = db.JobPosition.Find(ca);
                            JobPosition.Add(ContactDetails_val);
                            db_data.JobPosition = JobPosition;
                        }
                    }
                    else
                    {
                        db_data.JobPosition = null;
                    }


                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    db.Job.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_OBJ = db.Job.Find(data);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        Job blog = blog = null;
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.Job.Where(e => e.Id == data).SingleOrDefault();
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
                                        Job OBJ = new Job
                                        {
                                            Id = data,
                                            JobPosition = db_data.JobPosition,
                                            DesigSequenceNo = ESOBJ.DesigSequenceNo,
                                            Code = ESOBJ.Code,
                                            Name = ESOBJ.Name,
                                            DBTrack = ESOBJ.DBTrack
                                        };
                                        db.Job.Attach(OBJ);
                                        db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                                        DT_Job DT_OBJ = (DT_Job)obj;
                                        db.Create(DT_OBJ);
                                        db.SaveChanges();
                                        await db.SaveChangesAsync();
                                        db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = OBJ.Id, Val = OBJ.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] {, , "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                            }

                            catch (DbUpdateException e) { throw e; }
                            catch (DataException e) { throw e; }
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
                            //var errorMsg = sb.ToString();
                            //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                            var errorMsg = sb.ToString();
                            Msg.Add(errorMsg);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            Job blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            //Job Old_LKup = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Job.Where(e => e.Id == data).SingleOrDefault();
                                TempData["RowVersion"] = blog.RowVersion;
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
                            Job Job = new Job()
                            {
                                Id = data,
                                JobPosition = db_data.JobPosition,
                                Name = blog.Name,
                                DesigSequenceNo = blog.DesigSequenceNo,
                                Code = blog.Code,
                                DBTrack = ESOBJ.DBTrack
                            };
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                            db.Job.Attach(Job);
                            db.Entry(Job).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(Job).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            TempData["RowVersion"] = db_data.RowVersion;
                            db.Entry(Job).State = System.Data.Entity.EntityState.Detached;
                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ESOBJ, "Job", ESOBJ.DBTrack);
                                DT_Job DT_LKup = (DT_Job)obj;
                                db.Create(DT_LKup);
                            }

                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ESOBJ.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, ESOBJ.Name, "Record Updated", JsonRequestBehavior.AllowGet });
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

                //string INTYPE = "";
                //string INTYPE = form["InstituteTypelist"] == "0" ? "" : form["InstituteTypelist"];
                //if (INTYPE != null)
                //{
                //    if (INTYPE != "")
                //    {
                //        var val = db.LookupValue.Find(int.Parse(INTYPE));
                //        ESOBJ.IsManualRotateShift = val;
                //    }
                //}


                //db.Job.Attach(db_data);
                //db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                //TempData["RowVersion"] = db_data.RowVersion;
                //db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                //if (Auth == false)
                //{
                //    if (ModelState.IsValid)
                //    {
                //        try
                //        {
                //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                //            {
                //                Job blog = null; // to retrieve old data
                //                DbPropertyValues originalBlogValues = null;

                //                using (var context = new DataBaseContext())
                //                {
                //                    blog = context.Job.Where(e => e.Id == data).Include(e => e.JobPosition)
                //                                          .AsNoTracking().SingleOrDefault();
                //                    originalBlogValues = context.Entry(blog).OriginalValues;
                //                }

                //                ESOBJ.DBTrack = new DBTrack
                //                {
                //                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                //                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                //                    Action = "M",
                //                    ModifiedBy = SessionManager.UserName,
                //                    ModifiedOn = DateTime.Now
                //                };

                //                int a = EditS(INTYPE, Values, data, ESOBJ, ESOBJ.DBTrack);



                //                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, ESOBJ.DBTrack);
                //                DT_Job DT_OBJ = (DT_Job)obj;

                //                // DT_OBJ.InstituteType_Id = blog.IsManualRotateShift == null ? 0 : blog.IsManualRotateShift.Id;
                //                //DT_OBJ.Position_Id = blog.JobPosition == null ? 0 : blog.JobPosition.Id;
                //                db.Create(DT_OBJ);
                //                db.SaveChanges();




                //                //using (var context = new DataBaseContext())
                //                //{

                //                //To save data in history table 
                //                // var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ESOBJ, "Job", ESOBJ.DBTrack);
                //                //DT_Job DT_OBJ = (DT_Job)Obj;
                //                //db.DT_Job.Add(DT_OBJ);
                //                //db.SaveChanges();
                //                //}

                //                await db.SaveChangesAsync();
                //                ts.Complete();
                //                return Json(new Object[] { ESOBJ.Id, ESOBJ.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                //            }
                //        }
                //        catch (DbUpdateConcurrencyException ex)
                //        {
                //            var entry = ex.Entries.Single();
                //            var clientValues = (Job)entry.Entity;
                //            var databaseEntry = entry.GetDatabaseValues();
                //            if (databaseEntry == null)
                //            {
                //                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                //            }
                //            else
                //            {
                //                var databaseValues = (Job)databaseEntry.ToObject();
                //                ESOBJ.RowVersion = databaseValues.RowVersion;
                //            }
                //        }
                //        return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                //    }
                //}
                //else
                //{
                //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                //    {

                //        Job blog = null; // to retrieve old data
                //        DbPropertyValues originalBlogValues = null;
                //        Job Old_OBJ = null;

                //        using (var context = new DataBaseContext())
                //        {
                //            blog = context.Job.Where(e => e.Id == data).SingleOrDefault();
                //            originalBlogValues = context.Entry(blog).OriginalValues;
                //        }
                //        ESOBJ.DBTrack = new DBTrack
                //        {
                //            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                //            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                //            Action = "M",
                //            IsModified = blog.DBTrack.IsModified == true ? true : false,
                //            ModifiedBy = SessionManager.UserName,
                //            ModifiedOn = DateTime.Now
                //        };
                //        Job corp = new Job()
                //        {
                //            Code = ESOBJ.Code,
                //            Name = ESOBJ.Name,                       
                //            Id = data,
                //            DBTrack = ESOBJ.DBTrack,
                //            RowVersion = (Byte[])TempData["RowVersion"]
                //        };


                //        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                //        using (var context = new DataBaseContext())
                //        {
                //            var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Job", ESOBJ.DBTrack);
                //            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                //            Old_OBJ = context.Job.Where(e => e.Id == data)
                //               .Include(e => e.JobPosition).SingleOrDefault();
                //            DT_Job DT_OBJ = (DT_Job)obj;

                //            // DT_OBJ.InstituteType_Id = DBTrackFile.ValCompare(Old_OBJ.IsManualRotateShift, ESOBJ.IsManualRotateShift); //Old_OBJ.BusinessType == c.BusinessType ? 0 : Old_OBJ.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_OBJ.BusinessType.Id;
                //            DT_OBJ.Position_Id = DBTrackFile.ValCompare(Old_OBJ.JobPosition, ESOBJ.JobPosition); //Old_OBJ.JobPosition == c.JobPosition ? 0 : Old_OBJ.JobPosition == null && c.JobPosition != null ? c.JobPosition.Id : Old_OBJ.JobPosition.Id;
                //            db.Create(DT_OBJ);
                //            //db.SaveChanges();
                //        }
                //        blog.DBTrack = ESOBJ.DBTrack;
                //        db.Job.Attach(blog);
                //        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                //        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                //        db.SaveChanges();
                //        ts.Complete();
                //        return Json(new Object[] { blog.Id, ESOBJ.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                //    }

                //    //using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                //    //{
                //    //    Job Old_OBJ = db.Job.Include(e => e.JobPosition)
                //    //                                        .Where(e => e.Id == data).SingleOrDefault();
                //    //    Job Curr_OBJ = ESOBJ;
                //    //    ESOBJ.DBTrack = new DBTrack
                //    //    {
                //    //        CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                //    //        CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                //    //        Action = "M",
                //    //        IsModified = Old_OBJ.DBTrack.IsModified == true ? true : false,
                //    //        //ModifiedBy = SessionManager.UserName,
                //    //        //ModifiedOn = DateTime.Now
                //    //    };
                //    //    Old_OBJ.DBTrack = ESOBJ.DBTrack;
                //    //    db.Entry(Old_OBJ).State = System.Data.Entity.EntityState.Modified;
                //    //    db.SaveChanges();
                //    //    using (var context = new DataBaseContext())
                //    //    {
                //    //        DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Old_OBJ, Curr_OBJ, "Job", ESOBJ.DBTrack);
                //    //    }

                //    //    ts.Complete();
                //    //    return Json(new Object[] { Old_OBJ.Id, ESOBJ.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                //    //}

                //}
                //return View();
            }
        }

        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public async Task<ActionResult> EditSave1(Job NOBJ, int data, FormCollection form)
        //{
        //    var db_data = db.Job.Include(e => e.JobPosition).Where(e => e.Id == data).SingleOrDefault();
        //    List<JobPosition> JobPosition = new List<JobPosition>();
        //    string Values = form["PostionList"];

        //    if (Values != null)
        //    {
        //        var ids = one_ids(Values);
        //        foreach (var ca in ids)
        //        {
        //            var Medicine_val = db.JobPosition.Find(ca);
        //            JobPosition.Add(Medicine_val);
        //            db_data.JobPosition = JobPosition;
        //        }
        //    }
        //    else
        //    {
        //        db_data.JobPosition = null;
        //    }


        //    db.Job.Attach(db_data);
        //    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //    db.SaveChanges();
        //    TempData["RowVersion"] = db_data.RowVersion;
        //    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;



        //    try
        //    {


        //        if (ModelState.IsValid)
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {
        //                var Curr_Medicine = db.Job.Find(data);
        //                TempData["CurrRowVersion"] = Curr_Medicine.RowVersion;
        //                db.Entry(Curr_Medicine).State = System.Data.Entity.EntityState.Detached;

        //                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                {
        //                    Job blog = blog = null;
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.Job.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }

        //                    NOBJ.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };
        //                    Job Job = new Job
        //                    {
        //                        Id = data,
        //                        JobPosition = NOBJ.JobPosition,
        //                        Name = NOBJ.Name == null ? "" : NOBJ.Name.Trim(),
        //                        DBTrack = NOBJ.DBTrack
        //                    };


        //                    db.Job.Attach(Job);
        //                    db.Entry(Job).State = System.Data.Entity.EntityState.Modified;

        //                    // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
        //                    //db.SaveChanges();
        //                    db.Entry(Job).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, NOBJ.DBTrack);
        //                    await db.SaveChangesAsync();
        //                    //DisplayTrackedEntities(db.ChangeTracker);
        //                    db.Entry(Job).State = System.Data.Entity.EntityState.Detached;
        //                    ts.Complete();
        //                    return Json(new Object[] { Job.Id, Job.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //        }
        //        return View();
        //    }
        //    catch (DbUpdateException e) { throw e; }
        //    catch (DataException e) { throw e; }

        //}

        public ActionResult partial()
        {
            return View("~/Views/Shared/Core/_JobPostion.cshtml");
        }


        public class TimingPolicy_CD
        {
            public Array Position_Id { get; set; }
            public Array Position_FullDetails { get; set; }
        }


        //[HttpPost]
        //public ActionResult Edit(int data)
        //{
        //    List<TimingPolicy_CD> return_data = new List<TimingPolicy_CD>();
        //    var Job = db.Job
        //        .Include(e => e.JobPosition)
        //        .Where(e => e.Id == data).ToList();
        //    var r = (from ca in Job
        //             select new
        //             {
        //                 Id = ca.Id,
        //                 Name = ca.Name,
        //                 Code = ca.Code,                         
        //                 Action = ca.DBTrack.Action
        //             }).Distinct();


        //    var a = db.Job.Include(e => e.JobPosition).Where(e => e.Id == data).Select(e => e.JobPosition).ToList();

        //    foreach (var ca in a)
        //    {
        //        return_data.Add(
        //    new TimingPolicy_CD
        //    {
        //        Position_Id = ca.Select(e => e.Id.ToString()).ToArray(),
        //        Position_FullDetails = ca.Select(e => e.JobPositionDesc).ToArray()
        //    });
        //    }
        //    //var a = db.Job.Include(e => e.JobPosition).Where(e => e.Id == data).Select(e => e.JobPosition).SingleOrDefault();
        //    //var BCDETAILS = (from ca in a
        //    //                 select new
        //    //                 {
        //    //                     Id = ca.Id,
        //    //                     Position_Id = ca.Id,
        //    //                     TimingPolicy_FullDetails = ca.FullDetails
        //    //                 }).Distinct();

        //    TempData["RowVersion"] = db.Job.Find(data).RowVersion;

        //    var Old_Data = db.DT_Job
        //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M")
        //     .Select
        //     (e => new
        //     {
        //         DT_Id = e.Id,
        //         Name = e.Name == null ? "" : e.Name,
        //         Code = e.Code == null ? "" : e.Code,
        //         PostionList_Val = e.Position_Id == 0 ? "" : db.JobPosition.Where(x => x.Id == e.Position_Id).Select(x => x.JobPositionDesc).FirstOrDefault()
        //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

        //    var Lkval = db.Job.Find(data);
        //    TempData["RowVersion"] = Lkval.RowVersion;
        //    var Auth = Lkval.DBTrack.IsModified;

        //    return Json(new Object[] { r, return_data, Old_Data, Auth, JsonRequestBehavior.AllowGet });
        //    //return this.Json(new Object[] { r, a, JsonRequestBehavior.AllowGet });
        //}


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.Job
                    .Include(e => e.JobPosition)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Code = e.Code,
                        Name = e.Name,
                        Action = e.DBTrack.Action
                    }).ToList();

                List<TimingPolicy_CD> return_data = new List<TimingPolicy_CD>();
                var a = db.Job.Include(e => e.JobPosition).Where(e => e.Id == data).Select(e => e.JobPosition).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new TimingPolicy_CD
                {
                    Position_Id = ca.Select(e => e.Id.ToString()).ToArray(),
                    Position_FullDetails = ca.Select(e => e.JobPositionDesc).ToArray()
                });
                }
                var Corp = db.Job.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> Delete1(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    Job Job = db.Job.Include(e => e.JobPosition)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    var add = Job.JobPosition;

                    //Job Job = db.Job.Where(e => e.Id == data).SingleOrDefault();
                    if (Job.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Job.DBTrack, Job, null, "Job");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Job.DBTrack.CreatedBy != null ? Job.DBTrack.CreatedBy : null,
                                CreatedOn = Job.DBTrack.CreatedOn != null ? Job.DBTrack.CreatedOn : null,
                                IsModified = Job.DBTrack.IsModified == true ? true : false
                            };
                            Job.DBTrack = dbT;
                            db.Entry(Job).State = System.Data.Entity.EntityState.Modified;
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Job, null, "Job", Job.DBTrack);
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Job, null, "Job", Job.DBTrack);
                            }
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        //var selectedRegions = Job.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(Job.Regions.Select(e => e.Id));
                            //    if (corpRegion.Count > 0)
                            //    {
                            //        return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    }
                            //}

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = Job.DBTrack.CreatedBy != null ? Job.DBTrack.CreatedBy : null,
                                    CreatedOn = Job.DBTrack.CreatedOn != null ? Job.DBTrack.CreatedOn : null,
                                    IsModified = Job.DBTrack.IsModified == true ? false : false,
                                    AuthorizedBy = SessionManager.UserName,
                                    AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(Job).State = System.Data.Entity.EntityState.Deleted;
                                await db.SaveChangesAsync();


                                using (var context = new DataBaseContext())
                                {
                                    Job.JobPosition = add;
                                    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Job, null, "Job", dbT);
                                }
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable Name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
                return new EmptyResult();
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
                       Job Job = db.Job.Include(e => e.JobPosition).Where(e => e.Id == data).SingleOrDefault();

                       if (Job.DBTrack.IsModified == true)
                       {
                           using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                           {
                               //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                               DBTrack dbT = new DBTrack
                               {
                                   Action = "D",
                                   CreatedBy = Job.DBTrack.CreatedBy != null ? Job.DBTrack.CreatedBy : null,
                                   CreatedOn = Job.DBTrack.CreatedOn != null ? Job.DBTrack.CreatedOn : null,
                                   IsModified = Job.DBTrack.IsModified == true ? true : false
                               };
                               Job.DBTrack = dbT;
                               db.Entry(Job).State = System.Data.Entity.EntityState.Modified;
                               var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Job.DBTrack);
                               DT_Job DT_OBJ = (DT_Job)rtn_Obj;
                               db.Create(DT_OBJ);

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
                               var selectedValues = Job.JobPosition;
                               var lkValue = new HashSet<int>(Job.JobPosition.Select(e => e.Id));
                               if (lkValue.Count > 0)
                               {
                                   Msg.Add(" Child record exists.Cannot remove it..  ");
                                   return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                   // return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                               }
                               db.Entry(Job).State = System.Data.Entity.EntityState.Deleted;
                               db.SaveChanges();
                               ts.Complete();

                               Msg.Add("  Data removed successfully.  ");
                               return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                               //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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

        public async Task<ActionResult> Delete2(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    Job TIOBJ = db.Job.Include(e => e.JobPosition).Where(e => e.Id == data).SingleOrDefault();


                    //LookupValue val = TIOBJ.IsManualRotateShift;
                    //Job TIOBJ = db.Job.Where(e => e.Id == data).SingleOrDefault();
                    if (TIOBJ.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, TIOBJ.DBTrack, TIOBJ, null, "Job");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = TIOBJ.DBTrack.CreatedBy != null ? TIOBJ.DBTrack.CreatedBy : null,
                                CreatedOn = TIOBJ.DBTrack.CreatedOn != null ? TIOBJ.DBTrack.CreatedOn : null,
                                IsModified = TIOBJ.DBTrack.IsModified == true ? true : false
                            };
                            TIOBJ.DBTrack = dbT;
                            db.Entry(TIOBJ).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, TIOBJ.DBTrack);
                            DT_Job DT_Corp = (DT_Job)rtn_Obj;

                            //DT_Corp.InstituteType_Id = TIOBJ.IsManualRotateShift == null ? 0 : TIOBJ.IsManualRotateShift.Id;

                            db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", TIOBJ, null, "Job", TIOBJ.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", TIOBJ, null, "Job", TIOBJ.DBTrack );
                            //}
                            ts.Complete();
                            Msg.Add("  Data will be removed after authorization.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data will be removed after authorization.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        //var selectedRegions = TIOBJ.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(TIOBJ.Regions.Select(e => e.Id));
                            //    if (corpRegion.Count > 0)
                            //    {
                            //        return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    }
                            //}

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = TIOBJ.DBTrack.CreatedBy != null ? TIOBJ.DBTrack.CreatedBy : null,
                                    CreatedOn = TIOBJ.DBTrack.CreatedOn != null ? TIOBJ.DBTrack.CreatedOn : null,
                                    IsModified = TIOBJ.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(TIOBJ).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_Job DT_OBJ = (DT_Job)rtn_Obj;
                                //DT_Corp.Address_Id = add == null ? 0 : add.Id;
                                //DT_OBJ.InstituteType_Id = val == null ? 0 : val.Id;
                                //DT_OBJ.Position_Id = conDet == null ? 0 : conDet.Id;
                                db.Create(DT_OBJ);

                                await db.SaveChangesAsync();


                                //using (var context = new DataBaseContext())
                                //{
                                //    TIOBJ.Address = add;
                                //    TIOBJ.JobPosition = conDet;
                                //    TIOBJ.BusinessType = val;
                                //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", TIOBJ, null, "Job", dbT);
                                //}
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable Name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
        }
    }
}