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
using Appraisal;

namespace P2BUltimate.Controllers.Appraisal.MainController
{
    public class AppEvalMethodController : Controller
    {
      //  private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Appraisal/MainViews/AppEvalMethod/Index.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(AppEvalMethod p, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<String> Msg = new List<String>();
                var IsDescriptive = form["IsNormalAvg"] == "0" ? "" : form["IsNormalAvg"];
                var IsRatingObjective = form["IsGPA"] == "0" ? "" : form["IsGPA"];

                var chk = db.AppEvalMethod.Any(e => e.Code == p.Code);
                if (chk == true)
                {
                    Msg.Add("Code Already Exist.");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                try
                {
                    p.IsNormalAvg = Convert.ToBoolean(IsDescriptive);
                    p.IsGPA = Convert.ToBoolean(IsRatingObjective);

                    if (p.IsNormalAvg == true && p.IsGPA == true)
                    {
                        Msg.Add("Normal Avg/GPA both can't be selected at a time.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (p.IsNormalAvg == false && p.IsGPA == false)
                    {
                        Msg.Add("Normal Avg/GPA both can't be selected at a time.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };                     //???????

                            AppEvalMethod AppEvalmethod = new AppEvalMethod()
                            {
                                Code = p.Code == null ? "" : p.Code.ToString(),
                                Name = p.Name == null ? "" : p.Name.ToString(),
                                IsNormalAvg = p.IsNormalAvg,
                                IsGPA = p.IsGPA,
                                DBTrack = p.DBTrack
                            };

                            db.AppEvalMethod.Add(AppEvalmethod);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, p.DBTrack);

                            db.SaveChanges();

                            List<AppEvalMethod> OFAT = new List<AppEvalMethod>();
                            OFAT.Add(AppEvalmethod);

                            var compid = Convert.ToInt32(SessionManager.CompanyId);
                            var oCompany = db.Company.Find(compid);
                            CompanyAppraisal OEmployeePayroll = null;
                            OEmployeePayroll = db.CompanyAppraisal.Include(e => e.AppEvalMethod)
                                .Where(e => e.Company.Id == oCompany.Id).SingleOrDefault();

                            if (OEmployeePayroll == null)
                            {
                                CompanyAppraisal OTEP = new CompanyAppraisal()
                                {
                                    Company = db.Company.Find(oCompany.Id),
                                    AppEvalMethod = AppEvalmethod,
                                    DBTrack = p.DBTrack

                                };
                                db.CompanyAppraisal.Add(OTEP);
                                db.SaveChanges();
                            }
                            else
                            {
                                var aa = db.CompanyAppraisal.Find(OEmployeePayroll.Id);
                                // OFAT=aa.AppEvalMethod;
                                //   OFAT.AddRange(aa.AppEvalMethod);
                                aa.AppEvalMethod = AppEvalmethod;
                                //OEmployeePayroll.DBTrack = dbt;

                                db.CompanyAppraisal.Attach(aa);
                                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                            }

                            // var AppEvalMethodLost = new AppEvalMethod();
                            // AppEvalMethodLost=AppEvalmethod;

                            //var CompanyAppraisal = new CompanyAppraisal();

                            //var compid = Convert.ToInt32(SessionManager.CompanyId);
                            //var oCompany = db.Company.Find(compid);
                            //if (db.CompanyAppraisal.Any(o => o.Company.Id == oCompany.Id))
                            //{
                            //    CompanyAppraisal.AppEvalMethod = AppEvalMethodLost;
                            //    CompanyAppraisal.DBTrack = p.DBTrack;
                            //    db.CompanyAppraisal.Attach(CompanyAppraisal);
                            //    db.Entry(CompanyAppraisal).State = System.Data.Entity.EntityState.Modified;
                            //    //db.SaveChanges();
                            //    //Msg.Add("Code Already Exists.");
                            //    //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}
                            //else
                            //{
                            //    CompanyAppraisal.DBTrack = p.DBTrack;
                            //    CompanyAppraisal.Company = oCompany;
                            //    CompanyAppraisal.AppEvalMethod = AppEvalMethodLost;
                            //    db.CompanyAppraisal.Add(CompanyAppraisal);

                            //}

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
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
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

        


        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    List<string> Msg = new List<string>();
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<AppEvalMethod> Corporate = null;
        //        if (gp.IsAutho == true)
        //        {
        //            Corporate = db.AppEvalMethod .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            Corporate = db.AppEvalMethod .AsNoTracking().ToList();
        //        }

        //        IEnumerable<AppEvalMethod> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = Corporate;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Code")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Code.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Name")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();

        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
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
        //            IE = Corporate;
        //            Func<AppCategory, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Code" ? c.Code :
        //                                 gp.sidx == "Name" ? c.Name:"" );
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name)}).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name)}).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name}).ToList();
        //            }
        //            totalRecords = Corporate.Count();
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
                int ParentId = 2;
                var jsonData = (Object)null;
                var LKVal = db.AppEvalMethod.ToList();

                if (gp.IsAutho == true)
                {
                    LKVal = db.AppEvalMethod .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    LKVal = db.AppEvalMethod .AsNoTracking().ToList();
                }


                IEnumerable<AppEvalMethod> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = LKVal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                               || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();


                        //jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToUpper() == gp.searchString.ToUpper()) || (e.Name.ToLower() == gp.searchString.ToLower())));
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LKVal;
                    Func<AppEvalMethod, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                         gp.sidx == "Name" ? c.Name : "");
                    }

                    //Func<Lookup, string> orderfuc = (c =>
                    //                                           gp.sidx == "Id" ? c.Id.ToString() :
                    //                                           gp.sidx == "Code" ? c.Code :
                    //                                           gp.sidx == "Name" ? c.Name : "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
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


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var lookup = db.AppEvalMethod.Where(e => e.Id == data).ToList();
                var r = (from e in lookup
                         select new
                         {
                             Id = e.Id,
                             Code = e.Code == null ? "" : e.Code.ToString(),
                             Name = e.Name == null ? "" : e.Name.ToString(),
                             IsNormalAvg = e.IsNormalAvg,
                             IsGPA = e.IsGPA,
                             Action = e.DBTrack.Action
                         }).Distinct();

                var W = db.DT_AppEvalMethod
              .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
              (e => new
              {
                  Id = e.Id,
                  Code = e.Code == null ? "" : e.Code.ToString(),
                  Name = e.Name == null ? "" : e.Name.ToString(),
                  // AppMode_Id = e.AppMode.Id == null ? 0 : e.AppMode.Id,
                  IsNormalAvg = e.IsNormalAvg,
                  IsGPA = e.IsGPA,
              }).OrderByDescending(e => e.Id).FirstOrDefault();

                var LKup = db.AppEvalMethod.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, "", W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave( AppEvalMethod L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
                try
                {
                    var IsDescriptive = form["IsNormalAvg"] == "0" ? "" : form["IsNormalAvg"];
                    var IsRatingObjective = form["IsGPA"] == "0" ? "" : form["IsGPA"];
            
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var blog1 = db.AppEvalMethod.Where(e => e.Id == data).SingleOrDefault();

                    blog1.Code = L.Code;
                    blog1.Name = L.Name;
                    blog1.IsGPA = L.IsGPA;
                    blog1.IsNormalAvg = L.IsNormalAvg;

                    if (L.IsNormalAvg == true && L.IsGPA == true)
                    {
                        Msg.Add("Normal Avg/GPA both can't be selected at a time.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (L.IsNormalAvg == false && L.IsGPA == false)
                    {
                        Msg.Add("Normal Avg/GPA both can't be selected at a time.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    AppEvalMethod pd = null;
                    pd = db.AppEvalMethod.Where(e => e.Id == data).SingleOrDefault();
                    List<AppEvalMethod> ObjITsection = new List<AppEvalMethod>();
                    
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                   
                                    using (var context = new DataBaseContext())
                                        //{
                                        //    blog = context.PostDetails.Where(e => e.Id == data).Include(e => e.FuncStruct)
                                        //                  .Include(e => e.FuncStruct.JobPosition)
                                        //                  .Include(e => e.FuncStruct.Job)
                                        //                  .Include(e => e.ExpFilter)
                                        //                  .Include(e => e.RangeFilter)
                                        //                  .Include(e => e.Qualification)
                                        //                  .Include(e => e.Skill)
                                        //                  .Include(e => e.Gender)
                                        //                  .Include(e => e.MaritalStatus)
                                        //                  .Include(e => e.CategoryPost)
                                        //                  .Include(e => e.CategoryPost.Select(q => q.Category))
                                        //                  .Include(e => e.CategorySplPost)
                                        //                  .Include(e => e.CategorySplPost.Select(q => q.SpecialCategory))
                                        //                            .SingleOrDefault();
                                        //    originalBlogValues = context.Entry(blog).OriginalValues;
                                        //   }

                                        blog1.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
                                            CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                  
                                    var CurCorp = db.AppEvalMethod.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        AppEvalMethod post = new AppEvalMethod()
                                        {
                                            Code = blog1.Code == null ? "" : blog1.Code.ToString(),
                                            Name = blog1.Name == null ? "" : blog1.Name.ToString(),
                                            //  AppMode = blog1.AppMode,
                                            IsGPA = blog1.IsGPA,
                                            IsNormalAvg = blog1.IsNormalAvg,
                                            Id = data,
                                            DBTrack = blog1.DBTrack
                                        };
                                        db.AppEvalMethod.Attach(post);
                                        db.Entry(post).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                         db.SaveChanges();
                                        
                                        await db.SaveChangesAsync();
                                        db.Entry(post).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = blog1.Id, Val = blog1.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                    }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (AppEvalMethod)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (AppEvalMethod)databaseEntry.ToObject();
                                blog1.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

                    }
                    return Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public async Task<ActionResult> Delete1(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    AppEvalMethod corporates = db.AppEvalMethod.Where(e => e.Id == data).SingleOrDefault();

                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            corporates.DBTrack = dbT;
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, corporates.DBTrack);
                            DT_AppEvalMethod DT_Corp = (DT_AppEvalMethod)rtn_Obj;
                            db.Create(DT_Corp);

                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        // var selectedJobP = Postdetails.FuncStruct;
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, dbT);
                            //chng  528/9
                            DT_AppEvalMethod DT_Corp = (DT_AppEvalMethod)rtn_Obj;
                            db.Create(DT_Corp);

                            //   await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed.  ");                                                                                             // the original place 
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    AppEvalMethod OTPolicy = db.AppEvalMethod.Where(e => e.Id == data).SingleOrDefault();
                    try
                    {
                        //var selectedValues = OTPolicy.SocialActivities;
                        //var lkValue = new HashSet<int>(OTPolicy.SocialActivities.Select(e => e.Id));
                        //if (lkValue.Count > 0)
                        //{
                        //    return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                        //}

                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.Entry(OTPolicy).State = System.Data.Entity.EntityState.Deleted;

                            var v = db.CompanyAppraisal.Include(a => a.AppEvalMethod).ToList();
                            foreach (var item in v)
                            {
                                //if (item.AppEvalMethod.Id == OTPolicy.Id)
                                //{
                                //    Msg.Add("Record Present In Company Apprisal,Unable To Delete");
                                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //}
                            }


                            db.SaveChanges();
                            ts.Complete();
                        }
                        //   Msg.Add("  Data removed successfully.  ");
                        // return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    catch (DataException /* dex */)
                    {
                        // return this.Json(new Object[] { "", "Unable to delete. Try again, and if the problem persists contact your system administrator.", JsonRequestBehavior.AllowGet });
                        Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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