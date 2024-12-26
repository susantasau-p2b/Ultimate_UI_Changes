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
    public class AllergyController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Allergy/Index.cshtml");
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
                var Allergy = db.Allergy.ToList();
                IEnumerable<Allergy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Allergy;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), Convert.ToString(a.Details) }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.Details }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Allergy;
                    Func<Allergy, string> orderfuc = (c =>
                                                               gp.sidx == "ID" ? c.Id.ToString() :
                                                               gp.sidx == "Name" ? c.Name :
                                                               gp.sidx == "Details" ? c.Details :
                                                                 "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), Convert.ToString(a.Details) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.Details }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.Details }).ToList();
                    }
                    totalRecords = Allergy.Count();
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
        //        IEnumerable<Allergy> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Allergy.ToList().Where(d => d.Name.Contains(data));

        //        }
        //        else
        //        {
        //            //var list1 = db.Allergy.ToList().SelectMany(e => e.AllergyMedicine);
        //            //var list2 = fall.Except(list1);
        //            //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
        //            //var list1 = db.Allergy.Include(e => e.AllergyMedicine).SelectMany(e => e.AllergyMedicine);
        //            //var list2 = fall.Except(list1);
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.CorporateCode, c.CorporateName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
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
        public ActionResult Create(Allergy NOBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Values = form["Medicinelist"];


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
                            NOBJ.AllergyMedicine = new List<Medicine>();
                            if (ViewBag.Medicine != null)
                            {
                                foreach (var val in ViewBag.Medicine)
                                {
                                    if (val.Selected == true)
                                    {
                                        var valToAdd = db.Medicine.Find(int.Parse(val.Value));
                                        NOBJ.AllergyMedicine.Add(valToAdd);
                                    }
                                }
                            }

                            if (db.Allergy.Any(o => o.Name == NOBJ.Name))
                            {
                                //return this.Json(new { msg = "Allergy Name already exists." });
                                Msg.Add("  Allergy Name Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            Allergy Allergy = new Allergy()
                            {
                                Name = NOBJ.Name == null ? "" : NOBJ.Name.Trim(),
                                Details = NOBJ.Details,
                                AllergyMedicine = NOBJ.AllergyMedicine,
                                DBTrack = NOBJ.DBTrack
                            };
                            try
                            {
                                db.Allergy.Add(Allergy);
                                // DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, NOBJ.DBTrack);
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", Allergy, null, "Allergy", null);
                                db.SaveChanges();
                                ts.Complete();
                                //  return Json(new Object[] { Allergy.Id, Allergy.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = Allergy.Id, Val = Allergy.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = NOBJ.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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





        public int EditS(string OBJ, int data, Allergy c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var db_data = db.Allergy.Include(e => e.AllergyMedicine).Where(e => e.Id == data).SingleOrDefault();
                List<Medicine> lookupval = new List<Medicine>();
                string Values = OBJ;

                if (Values != null)
                {
                    var ids = Utility.StringIdsToListIds(Values);
                    foreach (var ca in ids)
                    {
                        var Lookup_val = db.Medicine.Find(ca);
                        lookupval.Add(Lookup_val);
                        db_data.AllergyMedicine = lookupval;
                    }
                }
                else
                {
                    db_data.AllergyMedicine = null;
                }


                db.Allergy.Attach(db_data);
                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["RowVersion"] = db_data.RowVersion;
                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;



                var CurOBJ = db.Allergy.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Allergy BOBJ = new Allergy()
                    {
                        Name = c.Name,
                        Details = c.Details,
                        FullDetails = c.FullDetails,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.Allergy.Attach(BOBJ);
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
                            Allergy Allergy = db.Allergy.Include(e => e.AllergyMedicine)
                              .FirstOrDefault(e => e.Id == auth_id);
                            Allergy.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = Allergy.DBTrack.ModifiedBy != null ? Allergy.DBTrack.ModifiedBy : null,
                                CreatedBy = Allergy.DBTrack.CreatedBy != null ? Allergy.DBTrack.CreatedBy : null,
                                CreatedOn = Allergy.DBTrack.CreatedOn != null ? Allergy.DBTrack.CreatedOn : null,
                                IsModified = Allergy.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };
                            db.Allergy.Attach(Allergy);
                            db.Entry(Allergy).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(Allergy).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                //  corp = db.Allergy.Include(e => e.AllergyMedicine)
                                //.Include(e => e.ContactDetails)
                                //.Include(e => e.BusinessType).FirstOrDefault(e => e.Id == auth_id);
                                // DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Allergy, null, "Allergy", Allergy.DBTrack);
                            }
                            ts.Complete();
                            //return Json(new Object[] { Allergy.Id, Allergy.Name, "Record Authorised", JsonRequestBehavior.AllowGet });

                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = Allergy.Id, Val = Allergy.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "M")
                    {

                        Allergy Old_OBJ = db.Allergy.Include(e => e.AllergyMedicine)
                                                          .Where(e => e.Id == auth_id).SingleOrDefault();

                        // DT_Allergy Curr_OBJ = db.DT_Allergy.Include(e => e.AllergyMedicine)  
                        DT_Allergy Curr_OBJ = db.DT_Allergy.Include(e => e.AllergyMedicine)
                                              .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                              .OrderByDescending(e => e.Id)
                                              .FirstOrDefault();

                        Allergy Allergy = new Allergy();

                        string EOBJ = Curr_OBJ.AllergyMedicine == null ? null : Curr_OBJ.AllergyMedicine.ToString();
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
                                    Allergy.DBTrack = new DBTrack
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

                                    int a = EditS(EOBJ, auth_id, Allergy, Allergy.DBTrack);

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    // return Json(new Object[] { Allergy.Id, Allergy.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = Allergy.Id, Val = Allergy.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Allergy)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    //   return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (Allergy)databaseEntry.ToObject();
                                    Allergy.RowVersion = databaseValues.RowVersion;
                                }
                            }

                            //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Allergy corp = db.Allergy.Find(auth_id);
                            Allergy Allergy = db.Allergy.AsNoTracking().Include(e => e.AllergyMedicine)
                                                                        .FirstOrDefault(e => e.Id == auth_id);
                            var selectedValues = Allergy.Details;
                            Allergy.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = Allergy.DBTrack.ModifiedBy != null ? Allergy.DBTrack.ModifiedBy : null,
                                CreatedBy = Allergy.DBTrack.CreatedBy != null ? Allergy.DBTrack.CreatedBy : null,
                                CreatedOn = Allergy.DBTrack.CreatedOn != null ? Allergy.DBTrack.CreatedOn : null,
                                IsModified = Allergy.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };
                            db.Allergy.Attach(Allergy);
                            db.Entry(Allergy).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                Allergy.Details = selectedValues;
                                // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Allergy, null, "Allergy", Allergy.DBTrack);
                            }
                            db.Entry(Allergy).State = System.Data.Entity.EntityState.Detached;
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
        public async Task<ActionResult> EditSave(Allergy ESOBJ, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var db_data = db.Allergy.Include(e => e.AllergyMedicine).Where(e => e.Id == data).SingleOrDefault();
                    List<Medicine> Medicine = new List<Medicine>();
                    string Values = form["MedicineList"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var Medicine_val = db.Medicine.Find(ca);
                            Medicine.Add(Medicine_val);
                            db_data.AllergyMedicine = Medicine;
                        }
                    }
                    else
                    {
                        db_data.AllergyMedicine = null;
                    }
                    db.Allergy.Attach(db_data);
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
                                Allergy blog = null; // to retrieve old data
                                // DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.Allergy.Where(e => e.Id == data).Include(e => e.AllergyMedicine)
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

                                //int a = EditS(Values, data, ESOBJ, ESOBJ.DBTrack);


                                var CurOBJ = db.Allergy.Find(data);
                                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                  //  c.DBTrack = dbT;
                                    Allergy BOBJ = new Allergy()
                                    {
                                        Name = ESOBJ.Name,
                                        Details = ESOBJ.Details,
                                        FullDetails = ESOBJ.FullDetails,
                                        Id = data,
                                        DBTrack = ESOBJ.DBTrack
                                    };


                                    db.Allergy.Attach(BOBJ);
                                    db.Entry(BOBJ).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(BOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    ////  DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                  //  return 1;

                                using (var context = new DataBaseContext())
                                {

                                //To save data in history table 
                                var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, ESOBJ, "Allergy", ESOBJ.DBTrack);
                                DT_Allergy DT_OBJ = (DT_Allergy)Obj;
                                db.DT_Allergy.Add(DT_OBJ);
                                db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                ts.Complete();
                                // return Json(new Object[] { ESOBJ.Id, ESOBJ.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = data, Val = BOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (Allergy)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            else
                            {
                                var databaseValues = (Allergy)databaseEntry.ToObject();
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
                    //        Allergy Old_OBJ = db.Allergy.Include(e => e.AllergyMedicine)
                    //                                            .Where(e => e.Id == data).SingleOrDefault();
                    //        Allergy Curr_OBJ = ESOBJ;
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
                    //             DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Old_OBJ, Curr_OBJ, "Allergy", ESOBJ.DBTrack);
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
        ////[ValidateAntiForgeryToken]
        //public async Task<ActionResult> EditSave1(Allergy NOBJ, int data, FormCollection form)
        //{
        //    var db_data = db.Allergy.Include(e => e.AllergyMedicine).Where(e => e.Id == data).SingleOrDefault();
        //    List<Medicine> Medicine = new List<Medicine>();
        //    string Values = form["MedicineList"];

        //    if (Values != null)
        //    {
        //        var ids = Utility.StringIdsToListIds(Values);
        //        foreach (var ca in ids)
        //        {
        //            var Medicine_val = db.Medicine.Find(ca);
        //            Medicine.Add(Medicine_val);
        //            db_data.AllergyMedicine = Medicine;
        //        }
        //    }
        //    else
        //    {
        //        db_data.AllergyMedicine = null;
        //    }


        //    db.Allergy.Attach(db_data);
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
        //                var Curr_Medicine = db.Allergy.Find(data);
        //                TempData["CurrRowVersion"] = Curr_Medicine.RowVersion;
        //                db.Entry(Curr_Medicine).State = System.Data.Entity.EntityState.Detached;

        //                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                {
        //                    Allergy blog = blog = null;
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.Allergy.Where(e => e.Id == data).SingleOrDefault();
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
        //                    Allergy Allergy = new Allergy
        //                    {
        //                        Id = data,
        //                        Details = NOBJ.Details,
        //                        Name = NOBJ.Name == null ? "" : NOBJ.Name.Trim(),
        //                        DBTrack = NOBJ.DBTrack
        //                    };


        //                    db.Allergy.Attach(Allergy);
        //                    db.Entry(Allergy).State = System.Data.Entity.EntityState.Modified;

        //                    // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
        //                    //db.SaveChanges();
        //                    db.Entry(Allergy).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, NOBJ.DBTrack);
        //                    await db.SaveChangesAsync();
        //                    //DisplayTrackedEntities(db.ChangeTracker);
        //                    db.Entry(Allergy).State = System.Data.Entity.EntityState.Detached;
        //                    ts.Complete();
        //                    return Json(new Object[] { Allergy.Id, Allergy.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //        }
        //        return View();
        //    }
        //    catch (DbUpdateException e) { throw e; }
        //    catch (DataException e) { throw e; }

        //}



        //public ActionResult EditMedicine_partial(int data)
        //{
        //    var r = (from ca in db.Medicine
        //             .Where(e => e.Id == data)
        //             select new
        //             {
        //                 Id = ca.Id,
        //                 Name = ca.Name,
        //                 Manufacturer = ca.Manufacturer,
        //                 MedPower = ca.MedPower
        //             }).ToList();

        //    var a = db.Medicine.Where(e => e.Id == data).SingleOrDefault();  
        //    var r1 = "";

        //    TempData["RowVersion"] = db.ContactDetails.Find(data).RowVersion;
        //    return Json(new object[] { r, r1 }, JsonRequestBehavior.AllowGet);
        //}


        public ActionResult Partial()
        {
            return View("~/Views/Shared/Core/_Medicine.cshtml");
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Allergy = db.Allergy.Include(e => e.AllergyMedicine).Where(e => e.Id == data).ToList();
                var r = (from ca in Allergy
                         select new
                         {
                             Id = ca.Id,
                             Name = ca.Name,
                             Details = ca.Details
                         }).Distinct();
                var a = db.Allergy.Include(e => e.AllergyMedicine).Where(e => e.Id == data).Select(e => e.AllergyMedicine).SingleOrDefault();
                var BCDETAILS = (from ca in a
                                 select new
                                 {
                                     Id = ca.Id,
                                     Medicine_Id = ca.Id,
                                     Medicine_FullDetails = ca.FullDetails
                                 }).Distinct();

                TempData["RowVersion"] = db.Allergy.Find(data).RowVersion;

                var Old_Data = db.DT_Allergy
                 .Include(e => e.AllergyMedicine)
                 .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M")
                 .Select
                 (e => new
                 {
                     DT_Id = e.Id,
                     Name = e.Name == null ? "" : e.Name,
                     ////Medicine_val = e.AllergyMedicine.Id == null ? "" : e.AllergyMedicine.FullDetails,  
                     //Medicine_Id = e.AllergyMedicine.Select(d => new { Id = d.Id }),
                     //Medicine_FullDetails = e.AllergyMedicine.Select(d => new { Id = d.FullDetails }),                        
                 }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.Allergy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;

                return Json(new Object[] { r, BCDETAILS, Old_Data, Auth, JsonRequestBehavior.AllowGet });
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

                    Allergy Allergy = db.Allergy.Include(e => e.AllergyMedicine)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    var add = Allergy.Details;

                    //Allergy Allergy = db.Allergy.Where(e => e.Id == data).SingleOrDefault();
                    if (Allergy.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            // DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Allergy.DBTrack, Allergy, null, "Allergy");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Allergy.DBTrack.CreatedBy != null ? Allergy.DBTrack.CreatedBy : null,
                                CreatedOn = Allergy.DBTrack.CreatedOn != null ? Allergy.DBTrack.CreatedOn : null,
                                IsModified = Allergy.DBTrack.IsModified == true ? true : false
                            };
                            Allergy.DBTrack = dbT;
                            db.Entry(Allergy).State = System.Data.Entity.EntityState.Modified;
                            // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Allergy, null, "Allergy", Allergy.DBTrack);
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                //  DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Allergy, null, "Allergy", Allergy.DBTrack);
                            }
                            ts.Complete();
                            //  return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            Msg.Add(" Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        //var selectedRegions = Allergy.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(Allergy.Regions.Select(e => e.Id));
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
                                    CreatedBy = Allergy.DBTrack.CreatedBy != null ? Allergy.DBTrack.CreatedBy : null,
                                    CreatedOn = Allergy.DBTrack.CreatedOn != null ? Allergy.DBTrack.CreatedOn : null,
                                    IsModified = Allergy.DBTrack.IsModified == true ? false : false,
                                    AuthorizedBy = SessionManager.UserName,
                                    AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(Allergy).State = System.Data.Entity.EntityState.Deleted;
                                await db.SaveChangesAsync();


                                using (var context = new DataBaseContext())
                                {
                                    Allergy.Details = add;
                                    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", Allergy, null, "Allergy", dbT);
                                }
                                ts.Complete();
                                //  return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });

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
