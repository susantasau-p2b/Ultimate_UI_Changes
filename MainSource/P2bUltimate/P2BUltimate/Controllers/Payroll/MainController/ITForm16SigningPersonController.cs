using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Threading.Tasks;
using System.Collections;
using P2BUltimate.Security;
using Payroll;


namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class ITForm16SigningPersonController : Controller
    {
       // private DataBaseContext db = new DataBaseContext();
        List<String> Msg = new List<String>();
        // GET: ITForm16SigningPerson
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITForm16SigningPerson/Index.cshtml");
        }

        public ActionResult GetLookup_Name_Designation1()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var lall = db.NameSingle.Select(e => e.Id.ToString()).Distinct().ToList();
                var rall = db.Employee.Include(e => e.EmpName).Where(e => e.EmpName != null).Select(e => e.EmpName.Id.ToString()).Distinct().ToList();
                var all = lall.Except(rall);
                var EmpList = new List<NameSingle>();
                foreach (var item in all)
                {
                    EmpList.Add(db.NameSingle.Find(Convert.ToInt32(item)));
                }
                var r = (from ca in EmpList select new { srno = ca.Id, lookupvalue = ca.FullNameFML }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookup_Name_DesignationOld(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.FuncStruct.Include(e => e.Job).AsNoTracking().AsParallel().Distinct().ToList();
                IEnumerable<FuncStruct> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.FuncStruct.Distinct().AsNoTracking().AsParallel().ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Job.Name }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            // return View();
        }
        public ActionResult GetLookup_Name_Designation(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Job.AsNoTracking().AsParallel().Distinct().ToList();
                IEnumerable<Job> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Job.Distinct().AsNoTracking().AsParallel().ToList().Where(d => d.Code.Contains(data));
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
            // return View();
        }
        public ActionResult GetLookup_Name_Place1()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var lall = db.Location.Select(e => e.Id.ToString()).Distinct().ToList();
                var rall = db.Location.Include(e => e.Address).Where(e => e.Address.Id != null).Select(e => e.Address.Id.ToString()).Distinct().ToList();
                var all = lall.Except(rall);
                var EmpList = new List<NameSingle>();
                foreach (var item in all)
                {
                    EmpList.Add(db.NameSingle.Find(Convert.ToInt32(item)));
                }
                var r = (from ca in EmpList select new { srno = ca.Id, lookupvalue = ca.FullNameFML }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookup_Name_Place(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Location.Include(e => e.Address).Include(e => e.Address.City).ToList();
                IEnumerable<Location> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Location.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Address.FullAddress }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            // return View();
        }
        public ActionResult GetLookup_Name()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
              //  var lall = db.NameSingle.Select(e => e.Id.ToString()).Distinct().ToList();
                var rall = db.Employee.Include(e => e.EmpName).Include(q => q.ServiceBookDates)
                    .Where(e => e.EmpName != null && e.ServiceBookDates.ServiceLastDate == null).Select(e => e.EmpName).AsNoTracking().AsParallel().Distinct().ToList();
               // var all = lall.Except(rall);
                //var EmpList = new List<NameSingle>();
                //foreach (var item in rall)
                //{
                //    EmpList.Add(db.NameSingle.Find(Convert.ToInt32(item)));
                //}
                var r = (from ca in rall select new { srno = ca.Id, lookupvalue = ca.FullNameFML }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookup_NameParents()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var lall = db.NameSingle.AsNoTracking().AsParallel().Distinct().ToList();
                //var rall = db.Employee.Include(e => e.EmpName)
                //    .Where(e => e.EmpName != null).AsNoTracking().AsParallel().Select(e => e.EmpName).Distinct().ToList();
                var lall = db.NameSingle.Select(e => e.Id.ToString()).AsNoTracking().AsParallel().Distinct().ToList();
                var rall = db.Employee.Include(e => e.EmpName).Where(e => e.EmpName != null).Select(e => e.EmpName.Id.ToString()).AsNoTracking().AsParallel().Distinct().ToList();
                var all = lall.Except(rall);
                var EmpList = new List<NameSingle>();
                foreach (var item in all)
                {
                    EmpList.Add(db.NameSingle.Find(Convert.ToInt32(item)));
                }
                var r = (from ca in EmpList select new { srno = ca.Id, lookupvalue = ca.FullNameFML }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(ITForm16SigningPerson c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var IsDefault = form["IsDefault"] == "0" ? "" : form["IsDefault"];
                try
                {
                    c.IsDefault = Convert.ToBoolean(IsDefault);

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.ITForm16SigningPerson.Any(o => o.SigningPersonFullName == c.SigningPersonFullName))
                            {
                                Msg.Add("Record Already Exists.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            if (db.ITForm16SigningPerson.Any(o => o.IsDefault == true && c.IsDefault == true))
                            {
                                Msg.Add("Default Record Already Exists.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var OFinYr = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToString().ToUpper() == "FINANCIALYEAR").SingleOrDefault();
                            ITForm16SigningPerson ITForm16SigningPerson = new ITForm16SigningPerson()
                            {
                                SigningPersonFullName = c.SigningPersonFullName == null ? "" : c.SigningPersonFullName,
                                SigningPersonFMHName = c.SigningPersonFMHName == null ? "" : c.SigningPersonFMHName,
                                Designation = c.Designation == null ? "" : c.Designation,
                                Place = c.Place == null ? "" : c.Place,
                                IsDefault = c.IsDefault,
                                ReportDate = c.ReportDate,
                                FinancialYear = OFinYr,
                                DBTrack = c.DBTrack
                            };

                            db.ITForm16SigningPerson.Add(ITForm16SigningPerson);

                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);

                            int CompPayrollId = Convert.ToInt32(SessionManager.CompPayrollId);
                            var company_data = db.CompanyPayroll.Where(e => e.Company.Id == CompPayrollId).SingleOrDefault();
                            List<ITForm16SigningPerson> SignPers = new List<ITForm16SigningPerson>();
                            SignPers.Add(ITForm16SigningPerson);
                            company_data.ITForm16SigningPerson = SignPers;
                            db.Entry(company_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            //chng remain
                            DT_ITForm16SigningPerson DT_Corp = (DT_ITForm16SigningPerson)rtn_Obj;
                            db.Create(DT_Corp);
                            db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", ITForm16SigningPerson, null, "ITForm16SigningPerson", null);


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
                        Msg.Add(errorMsg);
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
            List<string> Msg = new List<string>();
            
                try
                {
                    DataBaseContext db = new DataBaseContext();
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;
                    IEnumerable<ITForm16SigningPerson> Sign = null;
                    if (gp.IsAutho == true)
                    {
                        Sign = db.ITForm16SigningPerson.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                    }
                    else
                    {
                        Sign = db.ITForm16SigningPerson.AsNoTracking().ToList();
                    }

                    IEnumerable<ITForm16SigningPerson> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = Sign;
                        if (gp.searchOper.Equals("eq"))
                        {
                            
                            jsonData = IE.Where(e =>  (e.SigningPersonFullName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.SigningPersonFMHName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Designation.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.IsDefault.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.SigningPersonFullName, a.SigningPersonFMHName, a.Designation, a.IsDefault.ToString(), a.Id }).ToList();

                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.SigningPersonFullName, a.SigningPersonFMHName, a.Designation, a.IsDefault, a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = Sign;
                        Func<ITForm16SigningPerson, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {               //['Code', 'Name', 'Father/mother Name', 'Designation', 'Default'];
                            orderfuc = (c => gp.sidx == "Name" ? c.SigningPersonFullName :
                                             gp.sidx == "Father/mother Name" ? c.SigningPersonFMHName :
                                             gp.sidx == "Designation" ? c.Designation :
                                             gp.sidx == "Default" ? c.IsDefault.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.SigningPersonFullName), Convert.ToString(a.SigningPersonFMHName), Convert.ToString(a.Designation), Convert.ToString(a.IsDefault), a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.SigningPersonFullName), Convert.ToString(a.SigningPersonFMHName), Convert.ToString(a.Designation), Convert.ToString(a.IsDefault), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.SigningPersonFullName), Convert.ToString(a.SigningPersonFMHName), Convert.ToString(a.Designation), Convert.ToString(a.IsDefault), a.Id }).ToList();
                        }
                        totalRecords = Sign.Count();
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    ITForm16SigningPerson corporates = db.ITForm16SigningPerson.Where(e => e.Id == data).SingleOrDefault();

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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, corporates.DBTrack);
                            DT_ITForm16SigningPerson DT_Corp = (DT_ITForm16SigningPerson)rtn_Obj;
                            db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            //Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
                            DT_ITForm16SigningPerson DT_Corp = (DT_ITForm16SigningPerson)rtn_Obj;
                            db.Create(DT_Corp);

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
                            Msg.Add("  Data removed.  ");
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
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var lookup = db.ITForm16SigningPerson.Where(e => e.Id == data).ToList();
                var r = (from c in lookup
                         select new
                         {
                             Id = c.Id,
                             SigningPersonFullName = c.SigningPersonFullName == null ? "" : c.SigningPersonFullName,
                             SigningPersonFMHName = c.SigningPersonFMHName == null ? "" : c.SigningPersonFMHName,
                             Designation = c.Designation == null ? "" : c.Designation,
                             Place = c.Place == null ? "" : c.Place,
                             IsDefault = c.IsDefault,
                             ReportDate = c.ReportDate,
                             Action = c.DBTrack.Action
                         }).SingleOrDefault();

                var W = db.DT_ITForm16SigningPerson
              .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
              (c => new
              {
                  Id = c.Id,
                  SigningPersonFullName = c.SigningPersonFullName == null ? "" : c.SigningPersonFullName,
                  SigningPersonFMHName = c.SigningPersonFMHName == null ? "" : c.SigningPersonFMHName,
                  Designation = c.Designation == null ? "" : c.Designation,
                  Place = c.Place == null ? "" : c.Place,
                  IsDefault = c.IsDefault,
                  ReportDate = c.ReportDate,
              }).OrderByDescending(e => e.Id).FirstOrDefault();

                var LKup = db.ITForm16SigningPerson.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { r, "", W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(ITForm16SigningPerson ITForm, int data, FormCollection form) // Edit submit
        {
            var IsDefault = form["IsDefault"] == "0" ? "" : form["IsDefault"];
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    ITForm.IsDefault = Convert.ToBoolean(IsDefault);


                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    var db_Data = db.ITForm16SigningPerson.Where(e => e.Id == data).SingleOrDefault();
                    //db_Data.QuarterName = null;
                    //db_Data.ITChallan = null;

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {

                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    db.ITForm16SigningPerson.Attach(db_Data);
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_Data.RowVersion;
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_OBJ = db.ITForm16SigningPerson.Find(data);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        ITForm16SigningPerson blog = blog = null;
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.ITForm16SigningPerson.Where(e => e.Id == data).SingleOrDefault();
                                            originalBlogValues = context.Entry(blog).OriginalValues;
                                        }

                                        ITForm.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };
                                        ITForm16SigningPerson lk = new ITForm16SigningPerson
                                        {
                                            Id = data,
                                            SigningPersonFMHName = ITForm.SigningPersonFMHName,
                                            SigningPersonFullName = ITForm.SigningPersonFullName,
                                            Place = ITForm.Place,
                                            IsDefault = ITForm.IsDefault,
                                            Designation = ITForm.Designation,
                                            DBTrack = ITForm.DBTrack,
                                            ReportDate = ITForm.ReportDate
                                        };


                                        db.ITForm16SigningPerson.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                        // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                        //db.SaveChanges();
                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                        ////var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        ////DT_EmpMedicalInfo DT_LK = (DT_EmpMedicalInfo)obj;
                                        //////  DT_LK.Allergy = lk.Allergy.Select(e => e.Id);
                                        ////db.Create(DT_LK);
                                        db.SaveChanges();
                                        //   LookupValue QName = db.LookupValue.Find(QuarterName);

                                        //   var EmpId = db.ITForm16QuarterEmpDetails.Where(e => e.QuarterAckNo == ITForm.QuarterAckNo && e.QuarterName.Id == QName.Id && e.QuarterFromDate == ITForm.QuarterFromDate && e.QuarterToDate == ITForm.QuarterToDate).SingleOrDefault();
                                        //EmpId = new ITForm16QuarterEmpDetails
                                        //{
                                        //    QuarterFromDate = ITForm.QuarterFromDate,
                                        //    QuarterToDate = ITForm.QuarterToDate,
                                        //    TaxableIncome = ITForm.TaxableIncome,
                                        //    DBTrack = ITForm.DBTrack,
                                        //    QuarterName = QName,
                                        //    QuarterAckNo = ITForm.QuarterAckNo
                                        //};
                                        //db.ITForm16QuarterEmpDetails.Attach(EmpId);
                                        //db.Entry(EmpId).State = System.Data.Entity.EntityState.Modified;
                                        //db.Entry(EmpId).State = System.Data.Entity.EntityState.Detached;




                                        ////DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, look.DBTrack);
                                        await db.SaveChangesAsync();
                                        //DisplayTrackedEntities(db.ChangeTracker);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        // return Json(new Object[] { lk.Id, lk.PreferredHospital, "Record Updated", JsonRequestBehavior.AllowGet });
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
                            var errorMsg = sb.ToString();
                            Msg.Add(errorMsg);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        }
                    }

                    return View();
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