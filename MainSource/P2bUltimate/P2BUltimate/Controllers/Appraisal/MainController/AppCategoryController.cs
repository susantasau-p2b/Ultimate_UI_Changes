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
    public class AppCategoryController : Controller
    {
       // private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Appraisal/MainViews/AppCategory/Index.cshtml");
        }

        //public ActionResult partial()
        //{
        //    return View("~/Views/Shared/Appraisal/_AppCategory.cshtml");
        //}

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(AppCategory p, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Type = form["AppModelist"] == "0" ? "" : form["AppModelist"];
                var IsDescriptive = form["IsDescriptive"] == "0" ? "" : form["IsDescriptive"];
                var IsRatingObjective = form["IsRatingObjective"] == "0" ? "" : form["IsRatingObjective"];
                string AppSubCategory = form["AppSubCategoryList"] == "0" ? "" : form["AppSubCategoryList"];

                List<String> Msg = new List<String>();
                try
                {
                    p.AppSubCategory = null;
                    List<AppSubCategory> cp = new List<AppSubCategory>();
                    string val3 = form["AppSubCategoryList"];

                    if (val3 != null && val3 != "")
                    {
                        var ids = Utility.StringIdsToListIds(val3);
                        foreach (var ca in ids)
                        {
                            var p_val = db.AppSubCategory.Find(ca);
                            cp.Add(p_val);
                            p.AppSubCategory = cp;
                        }
                    }

                    if (Type != null && Type != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Type));
                        p.AppMode = val;
                    }

                    p.IsDescriptive = Convert.ToBoolean(IsDescriptive);
                    p.IsRatingObjective = Convert.ToBoolean(IsRatingObjective);

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.AppCategory.Any(o => o.AppMode.LookupVal.ToString() == p.AppMode.LookupVal.ToString() && o.Code == p.Code))
                            {
                                Msg.Add("Code Already Exists.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            if (p.IsDescriptive == false && p.IsRatingObjective == false)
                            {
                                Msg.Add("Kindly select desciptive or rating objective.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };                     //???????

                            AppCategory Appcategory = new AppCategory()
                            {
                                AppMode = p.AppMode,
                                Code = p.Code == null ? "" : p.Code.ToString(),
                                Name = p.Name == null ? "" : p.Name.ToString(),
                                IsDescriptive = p.IsDescriptive,
                                IsRatingObjective = p.IsRatingObjective,
                                AppSubCategory = p.AppSubCategory,
                                DBTrack = p.DBTrack
                            };

                            db.AppCategory.Add(Appcategory);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, p.DBTrack);

                            //DT_AppCategory DT_AppC = (DT_AppCategory)rtn_Obj;
                            //DT_AppC.AppMode_Id = p.AppMode == null ? 0 : p.AppMode.Id;
                            //db.Create(DT_AppC);
                            db.SaveChanges();

                            List<AppCategory> OFAT = new List<AppCategory>();
                            OFAT.Add(db.AppCategory.Find(Appcategory.Id));

                            var compid = Convert.ToInt32(SessionManager.CompanyId);
                            var oCompany = db.Company.Find(compid);
                            CompanyAppraisal OEmployeePayroll = null;
                            OEmployeePayroll = db.CompanyAppraisal.Include(e => e.AppCategory)
                                                                  .Include(e => e.AppCategory.Select(a => a.AppMode))
                                                                  .Include(e => e.AppCategory.Select(a => a.AppSubCategory))
                                                    .Where(e => e.Company.Id == oCompany.Id).SingleOrDefault();

                            if (OEmployeePayroll == null)
                            {
                                CompanyAppraisal OTEP = new CompanyAppraisal()
                                {
                                    Company = db.Company.Find(oCompany.Id),
                                    AppCategory = OFAT,
                                    DBTrack = p.DBTrack

                                };
                                db.CompanyAppraisal.Add(OTEP);
                                db.SaveChanges();
                            }
                            else
                            {
                                var aa = db.CompanyAppraisal.Find(OEmployeePayroll.Id);
                                OFAT.AddRange(aa.AppCategory);
                                aa.AppCategory = OFAT;
                                //OEmployeePayroll.DBTrack = dbt;

                                db.CompanyAppraisal.Attach(aa);
                                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                            }

                            //List<AppCategory> AppCategoryLost = new List<AppCategory>();
                            //AppCategoryLost.Add(Appcategory);

                            //var CompanyAppraisal = new CompanyAppraisal();

                            //var compid = Convert.ToInt32(SessionManager.CompanyId);
                            //var oCompany = db.Company.Find(compid);
                            //if (db.CompanyAppraisal.Any(o => o.Company.Id == oCompany.Id))
                            //{
                            //    CompanyAppraisal.AppCategory = AppCategoryLost;
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
                            //    CompanyAppraisal.AppCategory = AppCategoryLost;
                            //    db.CompanyAppraisal.Add(CompanyAppraisal);

                            //}

                            //db.SaveChanges();
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

        public ActionResult GetAppSubCatDetailLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.AppSubCategory.ToList();
                IEnumerable<AppSubCategory> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.AppSubCategory.ToList().Where(d => d.Code.Contains(data));    
                }
                else
                {
                   var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();  
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            
                List<string> Msg = new List<string>();
                try
                {
                    DataBaseContext db = new DataBaseContext();
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;
                    IEnumerable<AppCategory> Corporate = null;
                    if (gp.IsAutho == true)
                    {
                        Corporate = db.AppCategory.Include(e => e.AppSubCategory).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                    }
                    else
                    {
                        Corporate = db.AppCategory.Include(e => e.AppSubCategory).Include(e => e.AppMode).AsNoTracking().ToList();
                    }

                    IEnumerable<AppCategory> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = Corporate;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                               || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.AppMode != null ? e.AppMode.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.Code, a.Name, a.AppMode != null ? a.AppMode.LookupVal : "", a.Id }).ToList();
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.AppMode != null ? Convert.ToString(a.AppMode.LookupVal) : "", a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = Corporate;
                        Func<AppCategory, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                             gp.sidx == "Name" ? c.Name :
                                             gp.sidx == "AppMode" ? c.AppMode.LookupVal : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.AppMode != null ? Convert.ToString(a.AppMode.LookupVal) : "", a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.AppMode != null ? Convert.ToString(a.AppMode.LookupVal) : "", a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.AppMode != null ? Convert.ToString(a.AppMode.LookupVal) : "", a.Id }).ToList();
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

   

        public class AppSubCategoryDetailsC
        {
            public Array AppSubCategory_Id { get; set; }
            public Array AppSubCategory_FullAddress { get; set; }

        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<AppSubCategoryDetailsC> return_data = new List<AppSubCategoryDetailsC>();
                var lookup = db.AppCategory
                 .Include(e => e.AppMode)
                 .Include(e => e.AppSubCategory)
                 .Where(e => e.Id == data).ToList();
                var r = (from e in lookup
                         select new
                         {
                             Id = e.Id,
                             Code = e.Code == null ? "" : e.Code.ToString(),
                             Name = e.Name == null ? "" : e.Name.ToString(),
                             AppMode_Id = e.AppMode.Id == null ? 0 : e.AppMode.Id,
                             IsDescriptive = e.IsDescriptive,
                             IsRatingObjective = e.IsRatingObjective,

                             Action = e.DBTrack.Action
                         }).Distinct();

                var a = db.AppCategory
                 .Include(e => e.AppMode)
                 .Include(e => e.AppSubCategory)
                 .Where(e => e.Id == data).ToList();
                foreach (var ca in a)
                {
                    return_data.Add(new AppSubCategoryDetailsC
                {
                    AppSubCategory_Id = ca.AppSubCategory.Select(e => e.Id.ToString()).ToArray(),
                    AppSubCategory_FullAddress = ca.AppSubCategory.Select(e => e.FullDetails).ToArray(),

                });
                }

                var W = db.DT_AppCategory
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         Id = e.Id,
                         Code = e.Code == null ? "" : e.Code.ToString(),
                         Name = e.Name == null ? "" : e.Name.ToString(),
                         // AppMode_Id = e.AppMode.Id == null ? 0 : e.AppMode.Id,
                         IsDescriptive = e.IsDescriptive,
                         IsRatingObjective = e.IsRatingObjective,
                     }).OrderByDescending(e => e.Id).FirstOrDefault();

                var LKup = db.AppCategory.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, return_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }
       
        [HttpPost]
        public async Task<ActionResult> EditSave(AppCategory L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
                try
                {
                    string Gender = form["GenderList_DDL"] == "0" ? "" : form["GenderList_DDL"];

                   // string Type = form["AppModelist"] == "0" ? "" : form["AppModelist"];
                    var IsDescriptive = form["IsDescriptive"] == "0" ? "" : form["IsDescriptive"];
                    var IsRatingObjective = form["IsRatingObjective"] == "0" ? "" : form["IsRatingObjective"];
                    string AppSubCategory = form["AppSubCategoryList"] == "0" ? "" : form["AppSubCategoryList"];

                    var blog1 = db.AppCategory.Where(e => e.Id == data).Include(e => e.AppSubCategory)
                                        .Include(e => e.AppMode).SingleOrDefault();
                  
                    blog1.AppSubCategory = null;
                    blog1.Code = L.Code;
                    blog1.Name = L.Name;
                    blog1.IsDescriptive = L.IsDescriptive;
                    blog1.IsRatingObjective = L.IsRatingObjective;
                    L.AppMode = blog1.AppMode;
                    AppCategory pd = null;
                    pd = db.AppCategory.Include(q => q.AppSubCategory).Where(e => e.Id == data).SingleOrDefault();

                    if (L.IsDescriptive == false && L.IsRatingObjective == false)
                    {
                        Msg.Add("Kindly select desciptive or rating objective.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                               
                    List<AppSubCategory> ObjITsection = new List<AppSubCategory>();
                    if (AppSubCategory != null && AppSubCategory != "")
                    {
                        var ids = Utility.StringIdsToListIds(AppSubCategory);
                        foreach (var ca in ids)
                        {
                            var value = db.AppSubCategory.Find(ca);
                            ObjITsection.Add(value);
                            pd.AppSubCategory = ObjITsection;

                        }
                    }
                    else
                    {
                        pd.AppSubCategory = null;
                    }
                    //if (Type != null && Type != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(Type));
                    //    L.AppMode = val;
                    //}
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {

                                    using (var context = new DataBaseContext())
                                    {
                                        
                                    }

                                    blog1.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
                                        CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };


                                    //if (Type != null && Type != "")
                                    //{
                                    //        var val = db.LookupValue.Find(int.Parse(Type));
                                    //        blog1.AppMode = val;

                                    //        var type = db.AppCategory.Include(e => e.AppMode).Where(e => e.Id == data).SingleOrDefault();
                                    //        IList<AppCategory> typedetails = null;
                                    //        if (type.AppMode != null)
                                    //        {
                                    //            typedetails = db.AppCategory.Where(x => x.AppMode.Id == type.AppMode.Id && x.Id == data).ToList();
                                    //        }
                                    //        else
                                    //        {
                                    //            typedetails = db.AppCategory.Where(x => x.Id == data).ToList();
                                    //        }
                                    //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                    //        foreach (var s in typedetails)
                                    //        {
                                    //            s.AppMode = blog1.AppMode;
                                    //            db.AppCategory.Attach(s);
                                    //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //            //await db.SaveChangesAsync();
                                    //            db.SaveChanges();
                                    //            TempData["RowVersion"] = s.RowVersion;
                                    //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    //        }
                                    //    }
                                    //    else
                                    //    {
                                    //        var WFTypeDetails = db.AppCategory.Include(e => e.AppSubCategory).Where(x => x.Id == data).ToList();
                                    //        foreach (var s in WFTypeDetails)
                                    //        {
                                    //            s.AppMode = null;
                                    //            db.AppCategory.Attach(s);
                                    //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //            //await db.SaveChangesAsync();
                                    //            db.SaveChanges();
                                    //            TempData["RowVersion"] = s.RowVersion;
                                    //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    //        }
                                    //    }


                                    var CurCorp = db.AppCategory.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        AppCategory post = new AppCategory()
                                        {
                                            Code = blog1.Code == null ? "" : blog1.Code.ToString(),
                                            Name = blog1.Name == null ? "" : blog1.Name.ToString(),
                                           // AppMode = L.AppMode,
                                            IsDescriptive = blog1.IsDescriptive,
                                            IsRatingObjective = blog1.IsRatingObjective,
                                            Id = data,
                                            DBTrack = blog1.DBTrack
                                        };
                                        db.AppCategory.Attach(post);
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
                            var clientValues = (AppCategory)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (AppCategory)databaseEntry.ToObject();
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

        //[HttpPost]
        //public async Task<ActionResult> EditSave(Corporate c, int data, FormCollection form) // Edit submit
        //{
        //    List<String> Msg = new List<String>();
        //    try
        //    {
        //        string Gender = form["GenderList_DDL"] == "0" ? "" : form["GenderList_DDL"];

        //        // string Type = form["AppModelist"] == "0" ? "" : form["AppModelist"];
        //        var IsDescriptive = form["IsDescriptive"] == "0" ? "" : form["IsDescriptive"];
        //        var IsRatingObjective = form["IsRatingObjective"] == "0" ? "" : form["IsRatingObjective"];
        //        string AppSubCategory = form["AppSubCategoryList"] == "0" ? "" : form["AppSubCategoryList"];

        //        bool Auth = form["Autho_Allow"] == "true" ? true : false;


        //        if (Corp != null)
        //        {
        //            if (Corp != "")
        //            {
        //                var val = db.LookupValue.Find(int.Parse(Corp));
        //                c.BusinessType = val;
        //            }
        //        }

        //        if (Addrs != null)
        //        {
        //            if (Addrs != "")
        //            {
        //                int AddId = Convert.ToInt32(Addrs);
        //                var val = db.Address.Include(e => e.Area)
        //                                    .Include(e => e.City)
        //                                    .Include(e => e.Country)
        //                                    .Include(e => e.District)
        //                                    .Include(e => e.State)
        //                                    .Include(e => e.StateRegion)
        //                                    .Include(e => e.Taluka)
        //                                    .Where(e => e.Id == AddId).SingleOrDefault();
        //                c.Address = val;
        //            }
        //        }

        //        if (ContactDetails != null)
        //        {
        //            if (ContactDetails != "")
        //            {
        //                int ContId = Convert.ToInt32(ContactDetails);
        //                var val = db.ContactDetails.Include(e => e.ContactNumbers)
        //                                    .Where(e => e.Id == ContId).SingleOrDefault();
        //                c.ContactDetails = val;
        //            }
        //        }


        //        if (Auth == false)
        //        {


        //            if (ModelState.IsValid)
        //            {


        //                //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    Corporate blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.Corporate.Where(e => e.Id == data).Include(e => e.BusinessType)
        //                                                .Include(e => e.Address)
        //                                                .Include(e => e.ContactDetails).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }

        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);



        //                    using (var context = new DataBaseContext())
        //                    {
        //                        //c.Id = data;

        //                        /// DBTrackFile.DBTrackSave("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
        //                        //PropertyInfo[] fi = null;
        //                        //Dictionary<string, object> rt = new Dictionary<string, object>();
        //                        //fi = c.GetType().GetProperties();
        //                        ////foreach (var Prop in fi)
        //                        ////{
        //                        ////    if (Prop.Name != "Id" && Prop.Name != "DBTrack" && Prop.Name != "RowVersion")
        //                        ////    {
        //                        ////        rt.Add(Prop.Name, Prop.GetValue(c));
        //                        ////    }
        //                        ////}
        //                        //rt = blog.DetailedCompare(c);
        //                        //rt.Add("Orig_Id", c.Id);
        //                        //rt.Add("Action", "M");
        //                        //rt.Add("DBTrack", c.DBTrack);
        //                        //rt.Add("RowVersion", c.RowVersion);
        //                        //var aa = DBTrackFile.GetInstance("Core/P2b.Global", "DT_" + "Corporate", rt);
        //                        //DT_Corporate d = (DT_Corporate)aa;
        //                        //db.DT_Corporate.Add(d);
        //                        //db.SaveChanges();

        //                        //To save data in history table 
        //                        //var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
        //                        //DT_Corporate DT_Corp = (DT_Corporate)Obj;
        //                        //db.DT_Corporate.Add(DT_Corp);
        //                        //db.SaveChanges();\


        //                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                        DT_Corporate DT_Corp = (DT_Corporate)obj;
        //                        DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
        //                        DT_Corp.BusinessType_Id = blog.BusinessType == null ? 0 : blog.BusinessType.Id;
        //                        DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
        //                        db.Create(DT_Corp);
        //                        db.SaveChanges();
        //                    }
        //                    await db.SaveChangesAsync();
        //                    ts.Complete();

        //                    Msg.Add("Record Updated Successfully.");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }

        //                Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //        else
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {

        //                Corporate blog = null; // to retrieve old data
        //                DbPropertyValues originalBlogValues = null;
        //                Corporate Old_Corp = null;

        //                using (var context = new DataBaseContext())
        //                {
        //                    blog = context.Corporate.Where(e => e.Id == data).SingleOrDefault();
        //                    originalBlogValues = context.Entry(blog).OriginalValues;
        //                }
        //                c.DBTrack = new DBTrack
        //                {
        //                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                    Action = "M",
        //                    IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                    ModifiedBy = SessionManager.UserName,
        //                    ModifiedOn = DateTime.Now
        //                };

        //                if (TempData["RowVersion"] == null)
        //                {
        //                    TempData["RowVersion"] = blog.RowVersion;
        //                }

        //                Corporate corp = new Corporate()
        //                {
        //                    Code = c.Code,
        //                    Name = c.Name,
        //                    Id = data,
        //                    DBTrack = c.DBTrack,
        //                    RowVersion = (Byte[])TempData["RowVersion"]
        //                };


        //                //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                using (var context = new DataBaseContext())
        //                {
        //                    var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Corporate", c.DBTrack);
        //                    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                    Old_Corp = context.Corporate.Where(e => e.Id == data).Include(e => e.BusinessType)
        //                        .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
        //                    DT_Corporate DT_Corp = (DT_Corporate)obj;
        //                    DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                    DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                    DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                    db.Create(DT_Corp);
        //                    //db.SaveChanges();
        //                }
        //                blog.DBTrack = c.DBTrack;
        //                db.Corporate.Attach(blog);
        //                db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                db.SaveChanges();
        //                ts.Complete();
        //                Msg.Add("Record Updated Successfully.");
        //                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }

        //        }
        //    }
        //    catch (DbUpdateConcurrencyException ex)
        //    {
        //        var entry = ex.Entries.Single();
        //        var clientValues = (Corporate)entry.Entity;
        //        var databaseEntry = entry.GetDatabaseValues();
        //        if (databaseEntry == null)
        //        {
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //        }
        //        else
        //        {
        //            var databaseValues = (Corporate)databaseEntry.ToObject();
        //            c.RowVersion = databaseValues.RowVersion;

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Msg.Add(e.Message);
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //    }
        //    return View();

        //}


        public int EditS(string Type, int data, AppCategory c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Type != null && Type != "")
                {
                    var val = db.LookupValue.Find(int.Parse(Type));
                    c.AppMode = val;

                    var type = db.AppCategory.Include(e => e.AppMode).Where(e => e.Id == data).SingleOrDefault();
                    IList<AppCategory> typedetails = null;
                    if (type.AppMode != null)
                    {
                        typedetails = db.AppCategory.Where(x => x.AppMode.Id == type.AppMode.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        typedetails = db.AppCategory.Where(x => x.Id == data).ToList();
                    }
                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                    foreach (var s in typedetails)
                    {
                        s.AppMode = c.AppMode;
                        db.AppCategory.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    var BusiTypeDetails = db.AppCategory.Include(e => e.AppMode).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.AppMode = null;
                        db.AppCategory.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.AppCategory.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    AppCategory corp = new AppCategory()
                    {
                        Id = c.Id,
                        AppMode = c.AppMode,
                        Code = c.Code == null ? "" : c.Code.ToString(),
                        Name = c.Name == null ? "" : c.Name.ToString(),
                        IsDescriptive = c.IsDescriptive,
                        IsRatingObjective = c.IsRatingObjective,
                        AppSubCategory = c.AppSubCategory,
                        DBTrack = c.DBTrack,
                    };

                    db.AppCategory.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }     

        //[HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{

        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            AppCategory corporates = db.AppCategory
        //         .Include(e => e.AppMode)
        //         .Include(e => e.AppSubCategory)
        //         .Where(e => e.Id == data).SingleOrDefault();

        //            LookupValue val = corporates.AppMode;
        //            // FuncStruct fun = corporates.FuncStruct;
        //            if (corporates.DBTrack.IsModified == true)
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
        //                    DBTrack dbT = new DBTrack
        //                    {
        //                        Action = "D",
        //                        CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
        //                        CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
        //                        IsModified = corporates.DBTrack.IsModified == true ? true : false
        //                    };
        //                    corporates.DBTrack = dbT;
        //                    db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
        //                    var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, corporates.DBTrack);
        //                    DT_AppCategory DT_Corp = (DT_AppCategory)rtn_Obj;
        //                    DT_Corp.AppMode_Id = corporates.AppMode == null ? 0 : corporates.AppMode.Id;
        //                    db.Create(DT_Corp);

        //                    await db.SaveChangesAsync();
        //                    ts.Complete();
        //                    Msg.Add("  Data removed.  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            else
        //            {
        //                // var selectedJobP = Postdetails.FuncStruct;
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    DBTrack dbT = new DBTrack
        //                    {
        //                        Action = "D",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now,
        //                        CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
        //                        CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
        //                        IsModified = corporates.DBTrack.IsModified == true ? false : false//,
        //                        //AuthorizedBy = SessionManager.UserName,
        //                        //AuthorizedOn = DateTime.Now
        //                    };

        //                    // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

        //                    db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
        //                    var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, dbT);
        //                    DT_AppCategory DT_Corp = (DT_AppCategory)rtn_Obj;
        //                    DT_Corp.AppMode_Id = val == null ? 0 : val.Id;


        //                    var v = db.AppCategoryRating.Include(a => a.AppCategory).Where(a => a.AppCategory.Id == data).ToList();
        //                    if (v.Count != null)
        //                    {
        //                        Msg.Add("Record is used in App Category Rating, Can't be Removed  ");                                                                                             // the original place 
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    }

        //                    db.Create(DT_Corp);

        //                    await db.SaveChangesAsync();
        //                    ts.Complete();
        //                    Msg.Add("  Data removed.  ");                                                                                             // the original place 
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    Msg.Add("  Data removed.  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //        }
        //        catch (RetryLimitExceededException /* dex */)
        //        {
        //            Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                   AppCategory corporates = db.AppCategory
                 .Include(e => e.AppMode)
                 .Include(e => e.AppSubCategory)
                 .Where(e => e.Id == data).SingleOrDefault();


                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
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
                            DT_AppCategory DT_U = (DT_AppCategory)rtn_Obj;

                            DT_U.AppMode_Id = corporates.AppMode == null ? 0 : corporates.AppMode.Id;

                            db.Create(DT_U);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
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
                                    CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                    CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                    IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                               
                                var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, corporates.DBTrack);
                                DT_AppCategory DT_U = (DT_AppCategory)rtn_Obj;
                                DT_U.AppMode_Id = corporates.AppMode == null ? 0 : corporates.AppMode.Id;
                                db.Create(DT_U);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.AppCategory.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
    }
}