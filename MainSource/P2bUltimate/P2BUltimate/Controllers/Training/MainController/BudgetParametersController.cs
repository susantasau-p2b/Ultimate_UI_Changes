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
using System.Data.Entity.Core.Objects;
using System.Reflection;
using Training;
using P2BUltimate.Security;


namespace P2BUltimate.Controllers.Training.MainController
{
    public class BudgetParametersController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        // GET: /BudgetParameters/
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/BudgetParameters/Index.cshtml");
        }
        //public ActionResult Index()
        //{
        //    return View("~/Views/Shared/Training/_BudgetParameters.cshtml");
        //}


        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.BudgetParameters.ToList();


                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.BudgetParameters.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                //var list1 = db.FacultySpecialization.ToList();
                //var list2 = fall.Except(list1);
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult partial()
        {
            return View("~/Views/Shared/Training/_BudgetParameters.cshtml");
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(BudgetParameters NOBJ, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<String> Msg = new List<String>();

                var IsBudgetAppl = form["IsBudgetAppl"] == "0" ? "" : form["IsBudgetAppl"];
                var IsCategory = form["IsCategory"] == "0" ? "" : form["IsCategory"];
                var IsFuncStruct = form["IsFuncStruct"] == "0" ? "" : form["IsFuncStruct"];
                var IsGeoStruct = form["IsGeoStruct"] == "0" ? "" : form["IsGeoStruct"];
                var IsPayStruct = form["IsPayStruct"] == "0" ? "" : form["IsPayStruct"];
                var IsProgram = form["IsProgram"] == "0" ? "" : form["IsProgram"];


                List<string> ParameterCheck = new List<string>();

                if (IsCategory == "true")
                {
                    ParameterCheck.Add(IsCategory);
                }
                if (IsFuncStruct == "true")
                {
                    ParameterCheck.Add(IsFuncStruct);
                }
                if (IsGeoStruct == "true")
                {
                    ParameterCheck.Add(IsGeoStruct);
                }
                if (IsPayStruct == "true")
                {
                    ParameterCheck.Add(IsPayStruct);
                }
                if (IsProgram == "true")
                {
                    ParameterCheck.Add(IsProgram);
                }

                if (ParameterCheck.Count() > 1)
                {
                    Msg.Add("Please select only one parameter..");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                if (ParameterCheck.Count() == 0)
                {
                    Msg.Add("Please select atleat one parameter..");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                var check = db.BudgetParameters.ToList();
                if (check.Count() == 1)
                {
                    Msg.Add("Enter only one Budget parameter record..");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                if (IsBudgetAppl == "false")
                {
                    Msg.Add("Please select Budget Applicable yes..");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }



                NOBJ.IsBudgetAppl = Convert.ToBoolean(IsBudgetAppl);
                NOBJ.IsCategory = Convert.ToBoolean(IsCategory);
                NOBJ.IsFuncStruct = Convert.ToBoolean(IsFuncStruct);
                NOBJ.IsGeoStruct = Convert.ToBoolean(IsGeoStruct);
                NOBJ.IsPayStruct = Convert.ToBoolean(IsPayStruct);
                NOBJ.IsProgram = Convert.ToBoolean(IsProgram);

                int company_Id = 0;
                company_Id = Convert.ToInt32(Session["CompId"]);
                var companytraining = new CompanyTraining();
                companytraining = db.CompanyTraining.Where(e => e.Company.Id == company_Id).SingleOrDefault();

                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {

                        NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                        BudgetParameters budgetparameters = new BudgetParameters()
                        {
                            IsBudgetAppl = NOBJ.IsBudgetAppl,
                            IsCategory = NOBJ.IsCategory,
                            IsFuncStruct = NOBJ.IsFuncStruct,
                            IsGeoStruct = NOBJ.IsGeoStruct,
                            IsPayStruct = NOBJ.IsPayStruct,
                            IsProgram = NOBJ.IsProgram,
                            DBTrack = NOBJ.DBTrack
                        };
                        try
                        {

                            db.BudgetParameters.Add(budgetparameters);

                            db.SaveChanges();

                            if (companytraining != null)
                            {
                                var budgetparameters_list = new List<BudgetParameters>();
                                budgetparameters_list.Add(budgetparameters);
                                companytraining.Budgetparameters = budgetparameters_list;
                                db.CompanyTraining.Attach(companytraining);
                                db.Entry(companytraining).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(companytraining).State = System.Data.Entity.EntityState.Detached;
                            }
                            ts.Complete();
                            // return this.Json(new Object[] { budgetparameters.Id, budgetparameters.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { Id = budgetparameters.Id, Val = budgetparameters.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = NOBJ.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add("Unable to create. Try again, and if the problem persists contact your system administrator.");
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
                    //return this.Json(new { msg = errorMsg });
                }
            }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.BudgetParameters
                        .Where(e => e.Id == data).Select
                    (e => new
                    {
                        IsBudgetAppl = e.IsBudgetAppl,
                        IsCategory = e.IsCategory,
                        IsFuncStruct = e.IsFuncStruct,
                        IsGeoStruct = e.IsGeoStruct,
                        IsPayStruct = e.IsPayStruct,
                        IsProgram = e.IsProgram,


                        Action = e.DBTrack.Action
                    }).ToList();
                var Corp = db.BudgetParameters.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(BudgetParameters L, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var IsBudgetAppl = form["IsBudgetAppl"] == "0" ? "" : form["IsBudgetAppl"];
                var IsCategory = form["IsCategory"] == "0" ? "" : form["IsCategory"];
                var IsFuncStruct = form["IsFuncStruct"] == "0" ? "" : form["IsFuncStruct"];
                var IsGeoStruct = form["IsGeoStruct"] == "0" ? "" : form["IsGeoStruct"];
                var IsPayStruct = form["IsPayStruct"] == "0" ? "" : form["IsPayStruct"];
                var IsProgram = form["IsProgram"] == "0" ? "" : form["IsProgram"];


                L.IsBudgetAppl = Convert.ToBoolean(IsBudgetAppl);
                L.IsCategory = Convert.ToBoolean(IsCategory);
                L.IsFuncStruct = Convert.ToBoolean(IsFuncStruct);
                L.IsGeoStruct = Convert.ToBoolean(IsGeoStruct);
                L.IsPayStruct = Convert.ToBoolean(IsPayStruct);
                L.IsProgram = Convert.ToBoolean(IsProgram);


                bool Auth = form["Autho_Allow"] == "true" ? true : false;

                if (Auth == false)
                {


                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                BudgetParameters blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.BudgetParameters.Where(e => e.Id == data)
                                                            .SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }

                                L.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };


                                var CurCorp = db.BudgetParameters.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    BudgetParameters budgetparam = new BudgetParameters()
                                    {
                                        IsBudgetAppl = L.IsBudgetAppl,
                                        IsCategory = L.IsCategory,
                                        IsFuncStruct = L.IsFuncStruct,
                                        IsGeoStruct = L.IsGeoStruct,
                                        IsPayStruct = L.IsPayStruct,
                                        IsProgram = L.IsProgram,

                                        Id = data,
                                        DBTrack = L.DBTrack
                                    };
                                    db.BudgetParameters.Attach(budgetparam);
                                    db.Entry(budgetparam).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(budgetparam).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                }
                                // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                using (var context = new DataBaseContext())
                                {
                                    // var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                    var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                    // DT_BudgetParameters DT_Corp = (DT_BudgetParameters)obj;
                                    //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;
                                    // db.Create(DT_Corp);
                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("Record Updated.");
                                return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                            }

                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (BudgetParameters)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            //else
                            //{
                            //    var databaseValues = (LvEncashReq)databaseEntry.ToObject();
                            //    L.RowVersion = databaseValues.RowVersion;

                            //}
                        }

                        return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        BudgetParameters blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        BudgetParameters Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.BudgetParameters.Where(e => e.Id == data).SingleOrDefault();
                            originalBlogValues = context.Entry(blog).OriginalValues;
                        }
                        L.DBTrack = new DBTrack
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

                        BudgetParameters corp = new BudgetParameters()
                        {
                            IsBudgetAppl = L.IsBudgetAppl,
                            IsCategory = L.IsCategory,
                            IsFuncStruct = L.IsFuncStruct,
                            IsGeoStruct = L.IsGeoStruct,
                            IsPayStruct = L.IsPayStruct,
                            IsProgram = L.IsProgram,

                            Id = data,
                            DBTrack = L.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };


                        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];


                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "BudgetParameters", L.DBTrack);
                            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            Old_Corp = context.BudgetParameters.Where(e => e.Id == data).Include(e => e.FullDetails)
                               .SingleOrDefault();
                            DT_BudgetParameters DT_Corp = (DT_BudgetParameters)obj;
                            //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;

                            db.Create(DT_Corp);
                            //db.SaveChanges();
                        }
                        blog.DBTrack = L.DBTrack;
                        db.BudgetParameters.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add("Record Updated.");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                    }

                }
                return View();
            }
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public bool IsBudgetAppl { get; set; }
            public bool IsCategory { get; set; }
            public bool IsFuncStruct { get; set; }
            public bool IsGeoStruct { get; set; }
            public bool IsPayStruct { get; set; }
            public bool IsProgram { get; set; }


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

                    var BindCompList = db.CompanyTraining.Include(e => e.Budgetparameters).Where(e => e.Company.Id == company_Id).ToList();

                    foreach (var z in BindCompList)
                    {
                        if (z.Budgetparameters != null)
                        {

                            foreach (var s in z.Budgetparameters)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = s.Id,
                                    IsBudgetAppl = s.IsBudgetAppl,
                                    IsCategory = s.IsCategory,
                                    IsFuncStruct = s.IsFuncStruct,
                                    IsGeoStruct = s.IsGeoStruct,
                                    IsPayStruct = s.IsPayStruct,
                                    IsProgram = s.IsProgram


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
                            jsonData = IE.Where(e => (e.IsBudgetAppl.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.IsCategory.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.IsFuncStruct.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.IsGeoStruct.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.IsPayStruct.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.IsProgram.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.IsBudgetAppl, a.IsCategory, a.IsFuncStruct, a.IsGeoStruct, a.IsPayStruct, a.IsProgram, a.Id }).ToList();

                                //).Select(a => new Object[] { a.Id, a.IsBudgetAppl, a.IsCategory, a.IsFuncStruct, a.IsGeoStruct, a.IsPayStruct, a.IsProgram }).ToList();
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.IsBudgetAppl), Convert.ToString(a.IsCategory), Convert.ToString(a.IsFuncStruct), Convert.ToString(a.IsGeoStruct), Convert.ToString(a.IsPayStruct), Convert.ToString(a.IsProgram), a.Id }).ToList();
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
                            orderfuc = (c => gp.sidx == "IsBudgetAppl" ? c.IsBudgetAppl.ToString() :
                                             gp.sidx == "IsCategory" ? c.IsCategory.ToString() :
                                             gp.sidx == "IsFuncStruct" ? c.IsFuncStruct.ToString() :
                                             gp.sidx == "IsGeoStruct" ? c.IsGeoStruct.ToString() :
                                             gp.sidx == "IsPayStruct" ? c.IsPayStruct.ToString() :
                                              gp.sidx == "IsProgram" ? c.IsProgram.ToString() :
                                             "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.IsBudgetAppl), Convert.ToString(a.IsCategory), Convert.ToString(a.IsFuncStruct), Convert.ToString(a.IsGeoStruct), Convert.ToString(a.IsPayStruct), Convert.ToString(a.IsProgram), a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.IsBudgetAppl), Convert.ToString(a.IsCategory), Convert.ToString(a.IsFuncStruct), Convert.ToString(a.IsGeoStruct), Convert.ToString(a.IsPayStruct), Convert.ToString(a.IsProgram), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.IsBudgetAppl), Convert.ToString(a.IsCategory), Convert.ToString(a.IsFuncStruct), Convert.ToString(a.IsGeoStruct), Convert.ToString(a.IsPayStruct), Convert.ToString(a.IsProgram), a.Id }).ToList();
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


        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    BudgetParameters corporates = db.BudgetParameters.Where(e => e.Id == data).SingleOrDefault();

                    //Address add = corporates.Address;
                    //ContactDetails conDet = corporates.ContactDetails;
                    //LookupValue val = corporates.BusinessType;
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
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = corporates.Address == null ? 0 : corporates.Address.Id;
                            //DT_Corp.BusinessType_Id = corporates.BusinessType == null ? 0 : corporates.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                            //db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        // var selectedRegions = corporates.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(corporates.Regions.Select(e => e.Id));
                            //    if (corpRegion.Count > 0)
                            //    {
                            //        Msg.Add(" Child record exists.Cannot remove it..  ");
                            //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //        // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    }
                            //}


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
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = add == null ? 0 : add.Id;
                            //DT_Corp.BusinessType_Id = val == null ? 0 : val.Id;
                            //DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                            //db.Create(DT_Corp);

                            await db.SaveChangesAsync();


                            //using (var context = new DataBaseContext())
                            //{
                            //    corporates.Address = add;
                            //    corporates.ContactDetails = conDet;
                            //    corporates.BusinessType = val;
                            //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
                            //}
                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
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