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
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class DiseaseController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Disease/Index.cshtml");
        }

        public ActionResult EditMedicine_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var val = db.Medicine.Where(e => e.Id == data).SingleOrDefault();

                var r = (from ca in db.Medicine
                         select new
                         {
                             Id = ca.Id,
                             FullDetails = ca.FullDetails
                         }).Where(e => e.Id == data).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
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
                var Disease = db.Disease.ToList();
                IEnumerable<Disease> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Disease;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString() == gp.searchString) || (e.Name.ToLower() == gp.searchString.ToLower()) || (e.DiseaseType.LookupVal.ToLower() == gp.searchString.ToLower())).Select(a => new Object[] { a.Id, Convert.ToString(a.Name), Convert.ToString(a.DiseaseType.LookupVal) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.DiseaseType.LookupVal }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Disease;
                    Func<Disease, string> orderfuc = (c =>
                                                               gp.sidx == "ID" ? c.Id.ToString() :
                                                               gp.sidx == "Name" ? c.Name :
                                                               gp.sidx == "DiseaseType" ? c.DiseaseType.LookupVal :
                                                                 "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), Convert.ToString(a.DiseaseType.LookupVal) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.DiseaseType.LookupVal }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.DiseaseType.LookupVal }).ToList();
                    }
                    totalRecords = Disease.Count();
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

        //public ActionResult GetLookupDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.PersonalLoan.ToList();
        //        IEnumerable<PersonalLoan> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.PersonalLoan.ToList().Where(d => d.FullDetails.Contains(data));
        //        }
        //        else
        //        {
        //            var list1 = db.ITLoan.Include(e => e.PersLoan).SelectMany(e => e.PersLoan);
        //            var list2 = fall.Except(list1);
        //            var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //public ActionResult GetMedicineDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Medicine.ToList();
        //        IEnumerable<Disease> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Disease.ToList().Where(d => d.Name.Contains(data));

        //        }
        //        else
        //        {
        //            //var list1 = db.Disease.ToList().SelectMany(e => e.DiseaseMedicine);
        //            //var list2 = fall.Except(list1);
        //            //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
        //            //var list1 = db.Disease.Include(e => e.DiseaseMedicine).SelectMany(e => e.DiseaseMedicine);
        //            //var list2 = fall.Except(list1);
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
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

        public ActionResult GetMedicineDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Medicine.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Medicine.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult GetLookupDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        IEnumerable<Medicine>all;
        //        var fall = db.Medicine.ToList();
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Medicine.ToList().Where(d => d.StartingSlab.ToString().Contains(data));
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

        private MultiSelectList GetMedicineValues(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Medicine> lkval = new List<Medicine>();
                lkval = db.Medicine.ToList();
                return new MultiSelectList(lkval, "Id", "FullDetails", selectedValues);
            }
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(Disease NOBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["Diseasetypelist"] == "0" ? "" : form["Diseasetypelist"];
                    string Values = form["MedicineList1"];

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "300").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(Category));
                            NOBJ.DiseaseType = val;
                        }
                    }


                    if (Values != null)
                    {
                        List<int> IDs = Values.Split(',').Select(s => int.Parse(s)).ToList();
                        ViewBag.Medicine = GetMedicineValues(IDs);
                    }
                    else
                    {
                        ViewBag.Medicine = GetMedicineValues(null);
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            NOBJ.DiseaseMedicine = new List<Medicine>();
                            if (ViewBag.Medicine != null)
                            {
                                foreach (var val in ViewBag.Medicine)
                                {
                                    if (val.Selected == true)
                                    {
                                        var valToAdd = db.Medicine.Find(int.Parse(val.Value));
                                        NOBJ.DiseaseMedicine.Add(valToAdd);
                                    }
                                }
                            }

                            if (db.Disease.Any(o => o.Name == NOBJ.Name))
                            {
                                //  return this.Json(new { msg = "Disease Name already exists." });
                                Msg.Add("  Disease Name Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            Disease Disease = new Disease()
                            {
                                Name = NOBJ.Name == null ? "" : NOBJ.Name.Trim(),
                                DiseaseMedicine = NOBJ.DiseaseMedicine,
                                DiseaseType = NOBJ.DiseaseType,
                                DBTrack = NOBJ.DBTrack
                            };
                            try
                            {
                                db.Disease.Add(Disease);
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, NOBJ.DBTrack);
                                // DBTrackFile.DBTrackSave("Core/P2b.Global", "C", Disease, null, "Disease", null);
                                db.SaveChanges();
                                ts.Complete();
                                // return Json(new Object[] { Disease.Id, Disease.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = Disease.Id, Val = Disease.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = NOBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //  return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        public int EditS(string Corp, string OBJ, int data, Disease c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        c.DiseaseType = val;

                        var type = db.Disease.Include(e => e.DiseaseType).Where(e => e.Id == data).SingleOrDefault();
                        IList<Disease> typedetails = null;
                        if (type.DiseaseType != null)
                        {
                            typedetails = db.Disease.Where(x => x.DiseaseType.Id == type.DiseaseType.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Disease.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.DiseaseType = c.DiseaseType;
                            db.Disease.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.Disease.Include(e => e.DiseaseType).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.DiseaseType = null;
                            db.Disease.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.Disease.Include(e => e.DiseaseType).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.DiseaseType = null;
                        db.Disease.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }





                var db_data = db.Disease.Include(e => e.DiseaseMedicine).Where(e => e.Id == data).SingleOrDefault();
                List<Medicine> lookupval = new List<Medicine>();
                string Values = OBJ;

                if (Values != null)
                {
                    var ids = Utility.StringIdsToListIds(Values);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.Medicine.Find(ca);
                        lookupval.Add(Lookup_val);
                        db_data.DiseaseMedicine = lookupval;
                    }
                }
                else
                {
                    db_data.DiseaseMedicine = null;
                }


                db.Disease.Attach(db_data);
                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["RowVersion"] = db_data.RowVersion;
                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;



                var CurOBJ = db.Disease.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Disease BOBJ = new Disease()
                    {
                        Name = c.Name,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.Disease.Attach(BOBJ);
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
                            Disease Disease = db.Disease.Include(e => e.DiseaseMedicine)
                              .FirstOrDefault(e => e.Id == auth_id);
                            Disease.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = Disease.DBTrack.ModifiedBy != null ? Disease.DBTrack.ModifiedBy : null,
                                CreatedBy = Disease.DBTrack.CreatedBy != null ? Disease.DBTrack.CreatedBy : null,
                                CreatedOn = Disease.DBTrack.CreatedOn != null ? Disease.DBTrack.CreatedOn : null,
                                IsModified = Disease.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };
                            db.Disease.Attach(Disease);
                            db.Entry(Disease).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(Disease).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                //  corp = db.Disease.Include(e => e.DiseaseMedicine)
                                //.Include(e => e.ContactDetails)
                                //.Include(e => e.BusinessType).FirstOrDefault(e => e.Id == auth_id);
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Disease, null, "Disease", Disease.DBTrack);
                            }
                            ts.Complete();
                            // return Json(new Object[] { Disease.Id, Disease.Name, "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = Disease.Id, Val = Disease.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "M")
                    {

                        Disease Old_OBJ = db.Disease.Include(e => e.DiseaseMedicine)
                                                          .Where(e => e.Id == auth_id).SingleOrDefault();

                        // DT_Disease Curr_OBJ = db.DT_Disease.Include(e => e.DiseaseMedicine)  
                        DT_Disease Curr_OBJ = db.DT_Disease.Include(e => e.DiseaseMedicine)
                                              .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                              .OrderByDescending(e => e.Id)
                                              .FirstOrDefault();

                        Disease Disease = new Disease();
                        string Corp = Curr_OBJ.DiseaseType_Id == null ? null : Curr_OBJ.DiseaseType_Id.ToString();
                        string EOBJ = Curr_OBJ.DiseaseMedicine == null ? null : Curr_OBJ.DiseaseMedicine.ToString();
                        //corp.Name = Curr_OBJ.Name == null ? Old_OBJ.Name : Curr_OBJ.Name;
                        //corp.Code = Curr_OBJ.Code == null ? Old_OBJ.Code : Curr_OBJ.Code;
                        //      corp.Id = auth_id;

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
                                    Disease.DBTrack = new DBTrack
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

                                    int a = EditS(Corp, EOBJ, auth_id, Disease, Disease.DBTrack);

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    //   return Json(new Object[] { Disease.Id, Disease.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = Disease.Id, Val = Disease.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Disease)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (Disease)databaseEntry.ToObject();
                                    Disease.RowVersion = databaseValues.RowVersion;
                                }
                            }

                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Disease corp = db.Disease.Find(auth_id);
                            Disease Disease = db.Disease.AsNoTracking().Include(e => e.DiseaseMedicine)
                                                                        .FirstOrDefault(e => e.Id == auth_id);
                            var selectedValues = Disease.DiseaseMedicine;
                            Disease.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = Disease.DBTrack.ModifiedBy != null ? Disease.DBTrack.ModifiedBy : null,
                                CreatedBy = Disease.DBTrack.CreatedBy != null ? Disease.DBTrack.CreatedBy : null,
                                CreatedOn = Disease.DBTrack.CreatedOn != null ? Disease.DBTrack.CreatedOn : null,
                                IsModified = Disease.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };
                            db.Disease.Attach(Disease);
                            db.Entry(Disease).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                Disease.DiseaseMedicine = selectedValues;
                                // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Disease, null, "Disease", Disease.DBTrack);
                            }
                            db.Entry(Disease).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //  return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        [HttpPost]
        public async Task<ActionResult> EditSave1(Disease ESOBJ, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Corp = form["DiseaseTypeList_DDL"] == "0" ? "" : form["DiseaseTypeList_DDL"];
                    if (Corp != null)
                    {
                        if (Corp != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Corp));
                            ESOBJ.DiseaseType = val;
                        }
                    }
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var db_data = db.Disease.Include(e => e.DiseaseMedicine).Where(e => e.Id == data).SingleOrDefault();
                    List<Medicine> Medicine = new List<Medicine>();
                    string Values = form["MedicineList"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var Medicine_val = db.Medicine.Find(ca);
                            Medicine.Add(Medicine_val);
                            db_data.DiseaseMedicine = Medicine;
                        }
                    }
                    else
                    {
                        db_data.DiseaseMedicine = null;
                    }
                    db.Disease.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["RowVersion"] = db_data.RowVersion;
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                    //if (Auth == false)
                    //{
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                Disease blog = null; // to retrieve old data
                                // DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.Disease.Where(e => e.Id == data).Include(e => e.DiseaseMedicine)
                                                          .AsNoTracking().SingleOrDefault();
                                    // originalBlogValues = context.Entry(blog).OriginalValues;
                                }

                                ESOBJ.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };

                                int a = EditS(Corp, Values, data, ESOBJ, ESOBJ.DBTrack);

                                await db.SaveChangesAsync();

                                //using (var context = new DataBaseContext())
                                //{

                                //To save data in history table 
                                var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ESOBJ, "Disease", ESOBJ.DBTrack);
                                //DT_Disease DT_OBJ = (DT_Disease)Obj;
                                //db.DT_Disease.Add(DT_OBJ);
                                db.SaveChanges();
                                //}
                                ts.Complete();
                                //return Json(new Object[] { ESOBJ.Id, ESOBJ.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (Disease)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            else
                            {
                                var databaseValues = (Disease)databaseEntry.ToObject();
                                ESOBJ.RowVersion = databaseValues.RowVersion;
                            }
                        }
                        //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    //}
                    //else
                    //{
                    //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //    {
                    //        Disease Old_OBJ = db.Disease.Include(e => e.DiseaseMedicine)
                    //                                            .Where(e => e.Id == data).SingleOrDefault();
                    //        Disease Curr_OBJ = ESOBJ;
                    //        ESOBJ.DBTrack = new DBTrack
                    //        {
                    //            CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
                    //            CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
                    //            Action = "M",
                    //            IsModified = Old_OBJ.DBTrack.IsModified == true ? true : false,
                    //            //ModifiedBy = SessionManager.UserName,
                    //            //ModifiedOn = DateTime.Now
                    //        };
                    //        Old_OBJ.DBTrack = ESOBJ.DBTrack;
                    //        db.Entry(Old_OBJ).State = System.Data.Entity.EntityState.Modified;
                    //        db.SaveChanges();
                    //        using (var context = new DataBaseContext())
                    //        {
                    //             DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Old_OBJ, Curr_OBJ, "Disease", ESOBJ.DBTrack);
                    //        }

                    //        ts.Complete();
                    //        return Json(new Object[] { Old_OBJ.Id, ESOBJ.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                    //    }

                    //}
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

        //[HttpPost]
        //public async Task<ActionResult> EditSave(Disease ESOBJ, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            string Corp = form["DiseaseTypeList_DDL"] == "0" ? "" : form["DiseaseTypeList_DDL"];
        //            if (Corp != null)
        //            {
        //                if (Corp != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(Corp));
        //                    ESOBJ.DiseaseType = val;
        //                }
        //            }
        //            var db_data = db.Disease.Include(e => e.DiseaseMedicine).Include(e => e.DiseaseType).Where(e => e.Id == data).SingleOrDefault();
        //            List<Medicine> Medicine = new List<Medicine>();
        //            string Values = form["MedicineList"];

        //            if (Values != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(Values);
        //                foreach (var ca in ids)
        //                {
        //                    var Medicine_val = db.Medicine.Find(ca);
        //                    Medicine.Add(Medicine_val);
        //                    db_data.DiseaseMedicine = Medicine;
        //                }
        //            }
        //            else
        //            {
        //                db_data.DiseaseMedicine = null;
        //            }
        //            db.Disease.Attach(db_data);
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            TempData["RowVersion"] = db_data.RowVersion;
        //            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

        //            //if (Auth == false)
        //            //{
        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {
        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        Disease blog = null; // to retrieve old data
        //                        // DbPropertyValues originalBlogValues = null;

        //                        using (var context = new DataBaseContext())
        //                        {
        //                            blog = context.Disease.Where(e => e.Id == data).Include(e => e.DiseaseMedicine).Include(e => e.DiseaseType)
        //                                                  .AsNoTracking().SingleOrDefault();
        //                            // originalBlogValues = context.Entry(blog).OriginalValues;
        //                        }

        //                        ESOBJ.DBTrack = new DBTrack
        //                        {
        //                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                            Action = "M",
        //                            ModifiedBy = SessionManager.UserName,
        //                            ModifiedOn = DateTime.Now
        //                        };

        //                        //  int a = EditS(Values, data, ESOBJ, ESOBJ.DBTrack);
        //                        int a = EditS(Corp, Values, data, ESOBJ, ESOBJ.DBTrack);
        //                        await db.SaveChangesAsync();

        //                        //using (var context = new DataBaseContext())
        //                        //{

        //                        //To save data in history table 
        //                        var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ESOBJ, "Disease", ESOBJ.DBTrack);
        //                        //DT_Allergy DT_OBJ = (DT_Allergy)Obj;
        //                        //db.DT_Allergy.Add(DT_OBJ);
        //                        db.SaveChanges();
        //                        //}
        //                        ts.Complete();
        //                        // return Json(new Object[] { ESOBJ.Id, ESOBJ.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //                        Msg.Add("  Record Updated");
        //                        return Json(new Utility.JsonReturnClass { Id = data, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (Disease)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (Disease)databaseEntry.ToObject();
        //                        ESOBJ.RowVersion = databaseValues.RowVersion;
        //                    }
        //                }
        //                //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //            }
        //            //}
        //            //else
        //            //{
        //            //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            //    {
        //            //        Allergy Old_OBJ = db.Allergy.Include(e => e.AllergyMedicine)
        //            //                                            .Where(e => e.Id == data).SingleOrDefault();
        //            //        Allergy Curr_OBJ = ESOBJ;
        //            //        ESOBJ.DBTrack = new DBTrack
        //            //        {
        //            //            CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
        //            //            CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
        //            //            Action = "M",
        //            //            IsModified = Old_OBJ.DBTrack.IsModified == true ? true : false,
        //            //            //ModifiedBy = SessionManager.UserName,
        //            //            //ModifiedOn = DateTime.Now
        //            //        };
        //            //        Old_OBJ.DBTrack = ESOBJ.DBTrack;
        //            //        db.Entry(Old_OBJ).State = System.Data.Entity.EntityState.Modified;
        //            //        db.SaveChanges();
        //            //        using (var context = new DataBaseContext())
        //            //        {
        //            //             DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Old_OBJ, Curr_OBJ, "Allergy", ESOBJ.DBTrack);
        //            //        }

        //            //        ts.Complete();
        //            //        return Json(new Object[] { Old_OBJ.Id, ESOBJ.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //            //    }

        //            //}
        //            return View();
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
        public async Task<ActionResult> EditSave(Disease c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    string Corp = form["DiseaseTypeList_DDL"] == "0" ? "" : form["DiseaseTypeList_DDL"];
                    string Lang = form["MedicineList"] == "0" ? "" : form["MedicineList1"];

                    Disease corporates =db.Disease.Include(e => e.DiseaseMedicine).Include(e => e.DiseaseType).Where(e => e.Id == data).SingleOrDefault();
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if ( Corp != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "300").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Corp)).FirstOrDefault();//db.LookupValue.Find(int.Parse(Corp));
                        corporates.DiseaseType = val;
                    }

                    List<Medicine> lookupLang = new List<Medicine>();

                    if (Lang != null)
                    {
                        var ids = Utility.StringIdsToListIds(Lang);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.Medicine.Find(ca);

                            lookupLang.Add(Lookup_val);
                            corporates.DiseaseMedicine = lookupLang;
                        }
                    }
                    else
                    {
                        corporates.DiseaseMedicine = null;
                    }

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    using (var context = new DataBaseContext())
                                    {
                                        db.Disease.Attach(corporates);

                                        db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        TempData["RowVersion"] = corporates.RowVersion;
                                        db.Entry(corporates).State = System.Data.Entity.EntityState.Detached;

                                        var Curr_OBJ = db.Disease.Find(data);
                                        TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                        db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                        {
                                            Disease blog = null; // to retrieve old data
                                            DbPropertyValues originalBlogValues = null;


                                            blog = context.Disease.Where(e => e.Id == data).Include(e => e.DiseaseMedicine).Include(e => e.DiseaseType).SingleOrDefault();
                                            originalBlogValues = context.Entry(blog).OriginalValues;


                                            c.DBTrack = new DBTrack
                                            {
                                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                                Action = "M",
                                                ModifiedBy = SessionManager.UserName,
                                                ModifiedOn = DateTime.Now
                                            };

                                            Disease lk = new Disease
                                            {
                                                Id = data,
                                                Name = c.Name,
                                                DBTrack = c.DBTrack
                                            };


                                            db.Disease.Attach(lk);
                                            db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                            // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                            //db.SaveChanges();
                                            db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                            DT_Disease DT_LK = (DT_Disease)obj;
                                            //  DT_LK.Allergy = lk.Allergy.Select(e => e.Id);
                                            db.Create(DT_LK);
                                            db.SaveChanges();
                                            await db.SaveChangesAsync();
                                            db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                            ts.Complete();
                                            //  return Json(new Object[] { lk.Id, lk.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                            Msg.Add("  Record Updated");
                                            return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        }
                                    }
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Disease)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (Disease)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            Disease blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Disease Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Disease.Where(e => e.Id == data).SingleOrDefault();
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

                            Disease corp = new Disease()
                            {

                                Name = c.Name,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Disease", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                //Old_Corp = context.Disease.Where(e => e.Id == AID).Include(e => e.Relation)
                                //    .Include(e => e.Address).Include(e => e.ContactDetails).Include(e => e.NomineeName).Include(e => e.BenefitList).SingleOrDefault();
                                //DT_Disease DT_Corp = (DT_Disease)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails);
                                //DT_Corp.BenefitList_Id = DBTrackFile.ValCompare(Old_Corp.BenefitList, c.BenefitList);
                                //DT_Corp.Relation_Id = DBTrackFile.ValCompare(Old_Corp.Relation, c.Relation); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.NomineeName_Id = DBTrackFile.ValCompare(Old_Corp.NomineeName, c.NomineeName);
                                ////DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                //db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.Disease.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            //    return Json(new Object[] { blog.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_Medicine.cshtml");
        }

        //[HttpPost]
        //public ActionResult Edit(int data)
        //{
        //    var Disease = db.Disease.Include(e => e.DiseaseMedicine).Where(e => e.Id == data).ToList();
        //    var r = (from ca in Disease
        //             select new
        //             {
        //                 Id = ca.Id,
        //                 Name = ca.Name,
        //                 //   DiseaseType_Id = e. == null ? 0 : e.BusinessType.Id,
        //                 DiseaseType_Id = ca.DiseaseType.Id == null ? 0 : ca.DiseaseType.Id
        //             }).ToList();
        //    var a = db.Disease.Include(e => e.DiseaseMedicine).Where(e => e.Id == data).Select(e => e.DiseaseMedicine).SingleOrDefault();
        //    var BCDETAILS = (from ca in a
        //                     select new
        //                     {
        //                         Id = ca.Id,
        //                         Medicine_Id = ca.Id,
        //                         Medicine_FullDetails = ca.FullDetails
        //                     }).Distinct();

        //    TempData["RowVersion"] = db.Disease.Find(data).RowVersion;

        //    var Old_Data = db.DT_Disease
        //     .Include(e => e.DiseaseMedicine)
        //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M")
        //     .Select
        //     (e => new
        //     {
        //         DT_Id = e.Id,
        //         Name = e.Name == null ? "" : e.Name,
        //         // DiseaseType_Id = e.DiseaseType_Id,
        //         DiseaseType_val = e.DiseaseType_Id == 0 ? "" : db.LookupValue
        //                       .Where(x => x.Id == e.DiseaseType_Id)
        //                       .Select(x => x.LookupVal).FirstOrDefault(),

        //         ////Medicine_val = e.DiseaseMedicine.Id == null ? "" : e.DiseaseMedicine.FullDetails,  
        //         //Medicine_Id = e.DiseaseMedicine.Select(d => new { Id = d.Id }),
        //         //Medicine_FullDetails = e.DiseaseMedicine.Select(d => new { Id = d.FullDetails }),                        
        //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

        //    var Corp = db.Disease.Find(data);
        //    TempData["RowVersion"] = Corp.RowVersion;
        //    var Auth = Corp.DBTrack.IsModified;

        //    return Json(new Object[] { r, Disease,a,BCDETAILS, Old_Data, Auth, JsonRequestBehavior.AllowGet });
        //    //return this.Json(new Object[] { r, a, JsonRequestBehavior.AllowGet });
        //}
        public class medicdetails
        {
            public Array Medicine_Id { get; set; }
            public Array Medicine_FullDetails { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            //var Disease = db.Disease.Include(e => e.DiseaseMedicine).Include(e => e.DiseaseType).Where(e => e.Id == data).Select
            //    (e => new
            //    {

            //        Name = e.Name,
            //        DiseaseType_Id = e.DiseaseType.Id == null ? 0 : e.DiseaseType.Id,
            //        Action = e.DBTrack.Action
            //    }).ToList();
            using (DataBaseContext db = new DataBaseContext())
            {
                var Disease = db.Disease.Include(e => e.DiseaseMedicine).Include(e => e.DiseaseType).Where(e => e.Id == data).ToList();
                var r = (from ca in Disease
                         select new
                         {
                             Id = ca.Id,
                             Name = ca.Name,
                             DiseaseType_Id = ca.DiseaseType.Id == null ? 0 : ca.DiseaseType.Id,
                             Action = ca.DBTrack.Action
                         }).Distinct();
                //    List<medicdetails> medical = new List<medicdetails>();
                // //   var k=db.Disease.Include(a=>a.DiseaseMedicine).Where(e => e.Id == data).ToList();
                // foreach(var v in k)
                //{
                //    medical.Add(new medicdetails
                //    {

                //        Medicine_Id = v.DiseaseMedicine.Select(a => a.Id.ToString()).ToArray(),
                //        Medicine_FullDetails = v.DiseaseMedicine.Select(a => a.FullDetails).ToArray(),
                //    });
                // }


                var k = db.Disease.Include(e => e.DiseaseMedicine).Where(e => e.Id == data).Select(e => e.DiseaseMedicine).SingleOrDefault();
                var BCDETAILS = (from ca in k
                                 select new
                                 {
                                     Id = ca.Id,
                                     Medicine_Id = ca.Id,
                                     Medicine_FullDetails = ca.FullDetails
                                 }).Distinct();
                //var add_data = db.Disease
                //.Include(e => e.DiseaseMedicine)
                //  .Include(e => e.DiseaseType)

                //  .Where(e => e.Id == data)
                //  .Select(e => new
                //  {
                //      Medicine_Id = e.Id,
                //      Medicine_FullDetails = e.FullDetails
                //  }).ToList();


                var Old_Data = db.DT_Disease
                 .Include(e => e.DiseaseMedicine)
                 .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M")
                 .Select
                 (e => new
                 {
                     DT_Id = e.Id,
                     Name = e.Name == null ? "" : e.Name,
                     // DiseaseType_Id = e.DiseaseType_Id,
                     DiseaseType_val = e.DiseaseType_Id == 0 ? "" : db.LookupValue
                                   .Where(x => x.Id == e.DiseaseType_Id)
                                   .Select(x => x.LookupVal).FirstOrDefault(),

                     ////Medicine_val = e.DiseaseMedicine.Id == null ? "" : e.DiseaseMedicine.FullDetails,  
                     //Medicine_Id = e.DiseaseMedicine.Select(d => new { Id = d.Id }),
                     //Medicine_FullDetails = e.DiseaseMedicine.Select(d => new { Id = d.FullDetails }),                        
                 }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.Disease.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { r, BCDETAILS, Old_Data, Auth, JsonRequestBehavior.AllowGet });
                //  return Json(new Object[] { Disease, BCDETAILS, Old_Data, Auth, JsonRequestBehavior.AllowGet });
                //return this.Json(new Object[] { r, a, JsonRequestBehavior.AllowGet });
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

                    Disease Disease = db.Disease.Include(e => e.DiseaseMedicine)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    var add = Disease.DiseaseMedicine;

                    //Disease Disease = db.Disease.Where(e => e.Id == data).SingleOrDefault();
                    if (Disease.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Disease.DBTrack, Disease, null, "Disease");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Disease.DBTrack.CreatedBy != null ? Disease.DBTrack.CreatedBy : null,
                                CreatedOn = Disease.DBTrack.CreatedOn != null ? Disease.DBTrack.CreatedOn : null,
                                IsModified = Disease.DBTrack.IsModified == true ? true : false
                            };
                            Disease.DBTrack = dbT;
                            db.Entry(Disease).State = System.Data.Entity.EntityState.Modified;
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Disease, null, "Disease", Disease.DBTrack);
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                //  DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Disease, null, "Disease", Disease.DBTrack);
                            }
                            ts.Complete();
                            //    return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        //var selectedRegions = Disease.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(Disease.Regions.Select(e => e.Id));
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
                                    CreatedBy = Disease.DBTrack.CreatedBy != null ? Disease.DBTrack.CreatedBy : null,
                                    CreatedOn = Disease.DBTrack.CreatedOn != null ? Disease.DBTrack.CreatedOn : null,
                                    IsModified = Disease.DBTrack.IsModified == true ? false : false,
                                    AuthorizedBy = SessionManager.UserName,
                                    AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(Disease).State = System.Data.Entity.EntityState.Deleted;
                                await db.SaveChangesAsync();


                                using (var context = new DataBaseContext())
                                {
                                    Disease.DiseaseMedicine = add;
                                    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Disease, null, "Disease", dbT);
                                }
                                ts.Complete();
                                //   return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                //  return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                    }
                    return new EmptyResult();
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