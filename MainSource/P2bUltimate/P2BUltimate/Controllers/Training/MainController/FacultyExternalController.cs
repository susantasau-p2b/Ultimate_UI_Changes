///
/// Created by Kapil
///

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
using P2BUltimate.Controllers;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Training.MainController
{
    public class FacultyExternalController : Controller
    {

        List<string> Msg = new List<string>();
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/FacultyExternal/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Training/_FacultyExternal.cshtml");
        }

        public ActionResult CreateFacultyExternal_partial()
        {
            return View("~/Views/Shared/Training/_FacultyExternal.cshtml");
        }


        //private DataBaseContext db = new DataBaseContext();

        [HttpPost]
        public ActionResult Create(FacultyExternal c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                string Specilisation = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
                string TrainingInst = form["TrainingInstitutelist"] == "0" ? "" : form["TrainingInstitutelist"];
                string Addrs = form["Address_List"] == "0" ? "" : form["Address_List"];
                string ContactDetails = form["Contact_List"] == "0" ? "" : form["Contact_List"];

                if (Category != null)
                {
                    if (Category != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Category));
                        //   c.FacultyType = val;
                    }
                }
                if (db.FacultyExternal.Any(o => o.Code == c.Code))
                {
                    Msg.Add("  Code Already Exists.  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                //if (Specilisation != null)
                //{
                //    List<int> IDs = Specilisation.Split(',').Select(e => int.Parse(e)).ToList();
                //    foreach (var k in IDs)
                //    {
                //        var value = db.FacultySpecialization.Find(k);
                //        c.FacultySpecialization = new List<FacultySpecialization>();
                //        c.FacultySpecialization.Add(value);
                //    }
                //}

                c.FacultySpecialization = null;
                List<FacultySpecialization> Obj = new List<FacultySpecialization>();

                if (Specilisation != null)
                {
                    var ids = Utility.StringIdsToListIds(Specilisation);
                    foreach (var ca in ids)
                    {
                        var Obj_val = db.FacultySpecialization.Find(ca);
                        Obj.Add(Obj_val);
                        c.FacultySpecialization = Obj;
                    }

                }
                else
                {
                    Msg.Add("  Kindly select atleast 1 record for Faculty Specialization ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                if (TrainingInst != null)
                {
                    if (TrainingInst != "")
                    {
                        int InchId = Convert.ToInt32(TrainingInst);
                        var vals = db.TrainingInstitute.Where(e => e.Id == InchId).SingleOrDefault();
                        c.TrainingInstitue = vals;
                    }
                }
                else
                {
                    Msg.Add("  Kindly select atleast 1 record for Training Institute ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                if (Addrs != null && Addrs != "")
                {
                    int AddId = Convert.ToInt32(Addrs);
                    var val = db.Address.Include(e => e.Area)
                                        .Include(e => e.City)
                                        .Include(e => e.Country)
                                        .Include(e => e.District)
                                        .Include(e => e.State)
                                        .Include(e => e.StateRegion)
                                        .Include(e => e.Taluka)
                                        .Where(e => e.Id == AddId).SingleOrDefault();
                    c.Address = val;
                }

                if (ContactDetails != null && ContactDetails != "")
                {
                    int ContId = Convert.ToInt32(ContactDetails);
                    var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                        .Where(e => e.Id == ContId).SingleOrDefault();
                    c.ContactDetails = val;
                }

                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {


                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId, IsModified = false };

                        FacultyExternal FacultyExternal = new FacultyExternal()
                        {
                            Code = c.Code == null ? "" : c.Code.Trim(),
                            Narration = c.Narration == null ? "" : c.Narration.Trim(),
                            Address = c.Address,
                            FacultySpecialization = c.FacultySpecialization,
                            ContactDetails = c.ContactDetails,
                            TrainingInstitue = c.TrainingInstitue,
                            DBTrack = c.DBTrack
                        };
                        try
                        {
                            db.FacultyExternal.Add(FacultyExternal);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, c.DBTrack);
                            DT_FacultyExternal DT_Corp = (DT_FacultyExternal)rtn_Obj;
                            DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                            //   DT_Corp.FacultyType_Id = c.FacultyType == null ? 0 : c.FacultyType.Id;
                            DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                            DT_Corp.TrainingInstitue_Id = c.TrainingInstitue == null ? 0 : c.TrainingInstitue.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        }
                        catch (DataException /* dex */)
                        {
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
        }
        public class returnLookupClass
        {
            public Array fac_id { get; set; }
            public Array fac_details { get; set; }
            public Array train_id { get; set; }
            public Array traininginst_details { get; set; }
        }
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.FacultyExternal
                    .Include(e => e.TrainingInstitue)
                    // .Include(e => e.FacultyType)
                    .Include(e => e.FacultySpecialization)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Code = e.Code,
                        Narration = e.Narration,
                        //   FacultyType_Id = e.FacultyType == null ? 0 : e.FacultyType.Id,
                        Action = e.DBTrack.Action,
                        train_id = e.TrainingInstitue == null ? "" : e.TrainingInstitue.Id.ToString(),
                        traininginst_details = e.TrainingInstitue == null ? "" : e.TrainingInstitue.FullDetails,
                    }).ToList();


                List<returnLookupClass> faculty = new List<returnLookupClass>();
                var k = db.FacultyExternal.Include(e => e.FacultySpecialization).Where(b => b.Id == data).ToList();
                foreach (var v in k)
                {
                    faculty.Add(new returnLookupClass
                    {
                        fac_id = v.FacultySpecialization.Select(e => e.Id.ToString()).ToArray(),
                        fac_details = v.FacultySpecialization.Select(e => e.FullDetails).ToArray(),
                    });
                }

                var add_data = db.FacultyExternal.Include(e => e.Address).Include(e => e.ContactDetails).Include(e => e.FacultySpecialization)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                        Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
                        Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                        FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails
                    }).ToList();

                var W = db.DT_FacultyExternal
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Code = e.Code == null ? "" : e.Code,
                         Narration = e.Narration == null ? "" : e.Narration,
                         FacultyType_Val = e.FacultyType_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.FacultyType_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),

                         FactSpec_Val = e.FacultySpecilisation_Id == 0 ? "" : db.FacultySpecialization.Where(x => x.Id == e.FacultySpecilisation_Id).Select(x => x.FullDetails).FirstOrDefault(),
                         TraInst_Val = e.TrainingInstitue_Id == 0 ? "" : db.TrainingInstitute.Where(x => x.Id == e.TrainingInstitue_Id).Select(x => x.FullDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.FacultyExternal.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, faculty, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //public int EditS(string Corp, string Addrs, string ContactDetails, int data, FacultyExternal c, DBTrack dbT)
        //{
        //    if (Corp != null)
        //    {
        //        if (Corp != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(Corp));
        //            c.FacultyType = val;

        //            var type = db.FacultyExternal.Include(e => e.FacultyType).Where(e => e.Id == data).SingleOrDefault();
        //            IList<FacultyExternal> typedetails = null;
        //            if (type.FacultyType != null)
        //            {
        //                typedetails = db.FacultyExternal.Where(x => x.FacultyType.Id == type.FacultyType.Id && x.Id == data).ToList();
        //            }
        //            else
        //            {
        //                typedetails = db.FacultyExternal.Where(x => x.Id == data).ToList();
        //            }
        //            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //            foreach (var s in typedetails)
        //            {
        //                s.FacultyType = c.FacultyType;
        //                db.FacultyExternal.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //        else
        //        {
        //            var BusiTypeDetails = db.FacultyExternal.Include(e => e.FacultyType).Where(x => x.Id == data).ToList();
        //            foreach (var s in BusiTypeDetails)
        //            {
        //                s.FacultyType = null;
        //                db.FacultyExternal.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var BusiTypeDetails = db.FacultyExternal.Include(e => e.FacultyType).Where(x => x.Id == data).ToList();
        //        foreach (var s in BusiTypeDetails)
        //        {
        //            s.FacultyType = null;
        //            db.FacultyExternal.Attach(s);
        //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //            //await db.SaveChangesAsync();
        //            db.SaveChanges();
        //            TempData["RowVersion"] = s.RowVersion;
        //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //        }
        //    }

        //    if (Addrs != null)
        //    {
        //        if (Addrs != "")
        //        {
        //            var val = db.Address.Find(int.Parse(Addrs));
        //            c.Address = val;

        //            var add = db.Corporate.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
        //            IList<Corporate> addressdetails = null;
        //            if (add.Address != null)
        //            {
        //                addressdetails = db.Corporate.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
        //            }
        //            else
        //            {
        //                addressdetails = db.Corporate.Where(x => x.Id == data).ToList();
        //            }
        //            if (addressdetails != null)
        //            {
        //                foreach (var s in addressdetails)
        //                {
        //                    s.Address = c.Address;
        //                    db.Corporate.Attach(s);
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                    // await db.SaveChangesAsync(false);
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = s.RowVersion;
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var addressdetails = db.Corporate.Include(e => e.Address).Where(x => x.Id == data).ToList();
        //        foreach (var s in addressdetails)
        //        {
        //            s.Address = null;
        //            db.Corporate.Attach(s);
        //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //            //await db.SaveChangesAsync();
        //            db.SaveChanges();
        //            TempData["RowVersion"] = s.RowVersion;
        //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //        }
        //    }

        //    if (ContactDetails != null)
        //    {
        //        if (ContactDetails != "")
        //        {
        //            var val = db.ContactDetails.Find(int.Parse(ContactDetails));
        //            c.ContactDetails = val;

        //            var add = db.Corporate.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
        //            IList<Corporate> contactsdetails = null;
        //            if (add.ContactDetails != null)
        //            {
        //                contactsdetails = db.Corporate.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
        //            }
        //            else
        //            {
        //                contactsdetails = db.Corporate.Where(x => x.Id == data).ToList();
        //            }
        //            foreach (var s in contactsdetails)
        //            {
        //                s.ContactDetails = c.ContactDetails;
        //                db.Corporate.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var contactsdetails = db.Corporate.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
        //        foreach (var s in contactsdetails)
        //        {
        //            s.ContactDetails = null;
        //            db.Corporate.Attach(s);
        //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //            //await db.SaveChangesAsync();
        //            db.SaveChanges();
        //            TempData["RowVersion"] = s.RowVersion;
        //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //        }
        //    }

        //    var CurCorp = db.Corporate.Find(data);
        //    TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //    {
        //        c.DBTrack = dbT;
        //        Corporate corp = new Corporate()
        //        {
        //            Code = c.Code,
        //            Name = c.Name,
        //            Id = data,
        //            DBTrack = c.DBTrack
        //        };


        //        db.Corporate.Attach(corp);
        //        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //        //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //        return 1;
        //    }
        //    return 0;
        //}

        //[HttpPost]
        //public async Task<ActionResult> EditSave(FacultyExternal c, int data, FormCollection form) // Edit submit
        //{
        //    string FacultyType = form["Categorylist"] == "0" ? "" : form["Categorylist"];
        //    string TrainingInstitute = form["TrainingInstitutelist"] == "0" ? "" : form["TrainingInstitutelist"];
        //    string ContactDetails = form["Contact_List"] == "0" ? "" : form["Contact_List"];
        //    string Addrs = form["Address_List"] == "0" ? "" : form["Address_List"];
        //    bool Auth = form["autho_allow"] == "true" ? true : false;

        //    var db_Data = db.FacultyExternal.Include(e => e.FacultySpecialization).Include(e => e.TrainingInstitue)
        //         .Where(e => e.Id == data).SingleOrDefault();
        //    db_Data.FacultySpecialization = null;
        //    db_Data.TrainingInstitue = null;

        //    if (FacultyType != null && FacultyType != "")
        //    {
        //        var val = db.LookupValue.Find(int.Parse(FacultyType));
        //        //    db_Data.FacultyType = val; 
        //    }

        //    List<FacultySpecialization> lookupLang = new List<FacultySpecialization>();
        //    string Lang = form["FacultySpecializationlist"];

        //    if (Lang != null)
        //    {
        //        var ids = Utility.StringIdsToListIds(Lang);
        //        foreach (var ca in ids)
        //        {
        //            var lookup_val = db.FacultySpecialization.Find(ca);

        //            lookupLang.Add(lookup_val);
        //            db_Data.FacultySpecialization = lookupLang;
        //        }
        //    }
        //    else
        //    {
        //        db_Data.FacultySpecialization = null;
        //    }

        //    if (Addrs != null)
        //    {
        //        if (Addrs != "")
        //        {
        //            int AddId = Convert.ToInt32(Addrs);
        //            var val = db.Address.Include(e => e.Area)
        //                                .Include(e => e.City)
        //                                .Include(e => e.Country)
        //                                .Include(e => e.District)
        //                                .Include(e => e.State)
        //                                .Include(e => e.StateRegion)
        //                                .Include(e => e.Taluka)
        //                                .Where(e => e.Id == AddId).SingleOrDefault();
        //            c.Address = val;
        //        }
        //    }

        //    if (ContactDetails != null)
        //    {
        //        if (ContactDetails != "")
        //        {
        //            int ContId = Convert.ToInt32(ContactDetails);
        //            var val = db.ContactDetails.Include(e => e.ContactNumbers)
        //                                .Where(e => e.Id == ContId).SingleOrDefault();
        //            c.ContactDetails = val;
        //        }
        //    }


        //    if (TrainingInstitute != null && TrainingInstitute != "")
        //    {
        //        int ContId = Convert.ToInt32(TrainingInstitute);
        //        var val = db.TrainingInstitute.Where(e => e.Id == ContId).SingleOrDefault();
        //        db_Data.TrainingInstitue = val;
        //    }


        //    if (Auth == false)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            try
        //            {
        //                //DbContextTransaction transaction = db.Database.BeginTransaction();
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    using (var context = new DataBaseContext())
        //                    {
        //                        db.FacultyExternal.Attach(db_Data);
        //                        db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = db_Data.RowVersion;
        //                        db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

        //                        var Curr_OBJ = db.FacultyExternal.Find(data);
        //                        TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
        //                        db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

        //                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        {
        //                            FacultyExternal blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;


        //                            blog = context.FacultyExternal.Where(e => e.Id == data)
        //                                                    .Include(e => e.TrainingInstitue)
        //                                                    .Include(e => e.FacultySpecialization).SingleOrDefault();
        //                            originalBlogValues = context.Entry(blog).OriginalValues;


        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            FacultyExternal lk = new FacultyExternal
        //                            {
        //                                Id = data,
        //                                Code = db_Data.Code,
        //                                Address = c.Address,
        //                            FacultySpecialization = c.FacultySpecialization,
        //                            ContactDetails = c.ContactDetails,
        //                                Narration = db_Data.Narration,
        //                                TrainingInstitue = db_Data.TrainingInstitue,
        //                                DBTrack = c.DBTrack
        //                            };


        //                            db.FacultyExternal.Attach(lk);
        //                            db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

        //                            // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
        //                            //db.SaveChanges();
        //                            db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                            var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                            DT_FacultyExternal DT_LK = (DT_FacultyExternal)obj;
        //                            DT_LK.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
        //                            //DT_Corp.FacultyType_Id = blog.FacultySpecialization == null ? 0 : blog.FacultySpecialization;
        //                            DT_LK.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
        //                            db.Create(DT_LK);
        //                            db.SaveChanges();
        //                            await db.SaveChangesAsync();
        //                            db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
        //                            ts.Complete();
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        }
        //                    }
        //                }
        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (FacultyExternal)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //                else
        //                {
        //                    var databaseValues = (FacultyExternal)databaseEntry.ToObject();
        //                    c.RowVersion = databaseValues.RowVersion;
        //                }
        //            }
        //            Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    else
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {

        //            FacultyExternal blog = null; // to retrieve old data
        //            DbPropertyValues originalBlogValues = null;
        //            FacultyExternal Old_Corp = null;

        //            using (var context = new DataBaseContext())
        //            {
        //                blog = context.FacultyExternal.Where(e => e.Id == data).SingleOrDefault();
        //                originalBlogValues = context.Entry(blog).OriginalValues;
        //            }
        //            c.DBTrack = new DBTrack
        //            {
        //                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                Action = "M",
        //                IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                ModifiedBy = SessionManager.UserName,
        //                ModifiedOn = DateTime.Now
        //            };

        //            if (TempData["RowVersion"] == null)
        //            {
        //                TempData["RowVersion"] = blog.RowVersion;
        //            }

        //            FacultyExternal corp = new FacultyExternal()
        //            {
        //                Code = c.Code,
        //                Narration = c.Narration,
        //                Id = data,
        //                DBTrack = c.DBTrack,
        //                RowVersion = (Byte[])TempData["RowVersion"]
        //            };


        //            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //            using (var context = new DataBaseContext())
        //            {
        //                var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "FacultyExternal", c.DBTrack);
        //                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                Old_Corp = context.FacultyExternal.Where(e => e.Id == data)
        //                    .Include(e => e.TrainingInstitue).Include(e => e.TrainingInstitue).SingleOrDefault();
        //                DT_FacultyExternal DT_Corp = (DT_FacultyExternal)obj;
        //                DT_Corp.TrainingInstitue_Id = DBTrackFile.ValCompare(Old_Corp.TrainingInstitue, c.TrainingInstitue);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                // DT_Corp.FacultyType_Id = DBTrackFile.ValCompare(Old_Corp.FacultyType, c.FacultyType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                db.Create(DT_Corp);
        //                //db.SaveChanges();
        //            }
        //            blog.DBTrack = c.DBTrack;
        //            db.FacultyExternal.Attach(blog);
        //            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            db.SaveChanges();
        //            ts.Complete();
        //            Msg.Add("  Record Updated");
        //            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    return View();

        //}

        //[HttpPost]
        //public async Task<ActionResult> EditSave(FacultyExternal c, int data, FormCollection form) // Edit submit
        //{
        //    string FacultyType = form["Categorylist"] == "0" ? "" : form["Categorylist"];
        //    string TrainingInstitute = form["TrainingInstitutelist"] == "0" ? "" : form["TrainingInstitutelist"];
        //    string ContactDetails = form["Contact_List"] == "0" ? "" : form["Contact_List"];
        //    string Addrs = form["Address_List"] == "0" ? "" : form["Address_List"];
        //    bool Auth = form["autho_allow"] == "true" ? true : false;

        //    var db_Data = db.FacultyExternal.Include(e => e.FacultySpecialization).Include(e => e.TrainingInstitue)
        //         .Where(e => e.Id == data).SingleOrDefault();
        //    db_Data.FacultySpecialization = null;
        //    db_Data.TrainingInstitue = null;

        //    if (FacultyType != null && FacultyType != "")
        //    {
        //        var val = db.LookupValue.Find(int.Parse(FacultyType));
        //        //    db_Data.FacultyType = val; 
        //    }

        //    List<FacultySpecialization> lookupLang = new List<FacultySpecialization>();
        //    string Lang = form["FacultySpecializationlist"];

        //    if (Lang != null)
        //    {
        //        var ids = Utility.StringIdsToListIds(Lang);
        //        foreach (var ca in ids)
        //        {
        //            var lookup_val = db.FacultySpecialization.Find(ca);

        //            lookupLang.Add(lookup_val);
        //            db_Data.FacultySpecialization = lookupLang;
        //        }
        //    }
        //    else
        //    {
        //        db_Data.FacultySpecialization = null;
        //    }

        //    if (Addrs != null)
        //    {
        //        if (Addrs != "")
        //        {
        //            int AddId = Convert.ToInt32(Addrs);
        //            var val = db.Address.Include(e => e.Area)
        //                                .Include(e => e.City)
        //                                .Include(e => e.Country)
        //                                .Include(e => e.District)
        //                                .Include(e => e.State)
        //                                .Include(e => e.StateRegion)
        //                                .Include(e => e.Taluka)
        //                                .Where(e => e.Id == AddId).SingleOrDefault();
        //            c.Address = val;
        //        }
        //    }

        //    if (ContactDetails != null)
        //    {
        //        if (ContactDetails != "")
        //        {
        //            int ContId = Convert.ToInt32(ContactDetails);
        //            var val = db.ContactDetails.Include(e => e.ContactNumbers)
        //                                .Where(e => e.Id == ContId).SingleOrDefault();
        //            c.ContactDetails = val;
        //        }
        //    }


        //    if (TrainingInstitute != null && TrainingInstitute != "")
        //    {
        //        int ContId = Convert.ToInt32(TrainingInstitute);
        //        var val = db.TrainingInstitute.Where(e => e.Id == ContId).SingleOrDefault();
        //        db_Data.TrainingInstitue = val;
        //    }


        //    if (Auth == false)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            try
        //            {
        //                //DbContextTransaction transaction = db.Database.BeginTransaction();
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    try
        //                    {
        //                        using (var context = new DataBaseContext())
        //                        {
        //                            FacultyExternal blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;


        //                            blog = context.FacultyExternal.Where(e => e.Id == data).Include(e => e.Address).Include(e => e.ContactDetails)
        //                                  .Include(e => e.TrainingInstitue).Include(e => e.FacultySpecialization).SingleOrDefault();
        //                            originalBlogValues = context.Entry(blog).OriginalValues;


        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            FacultyExternal lk = new FacultyExternal
        //                            {
        //                                Id = data,
        //                                Code = db_Data.Code,
        //                                Address = c.Address,
        //                                FacultySpecialization = c.FacultySpecialization,
        //                                ContactDetails = c.ContactDetails,
        //                                Narration = db_Data.Narration,
        //                                TrainingInstitue = db_Data.TrainingInstitue,
        //                                DBTrack = c.DBTrack
        //                            };


        //                            db.FacultyExternal.Attach(lk);
        //                            db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

        //                            // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
        //                            //db.SaveChanges();
        //                            db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                            var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                            DT_FacultyExternal DT_LK = (DT_FacultyExternal)obj;
        //                            DT_LK.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
        //                            //DT_Corp.FacultyType_Id = blog.FacultySpecialization == null ? 0 : blog.FacultySpecialization;
        //                            DT_LK.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
        //                            db.Create(DT_LK);
        //                            db.SaveChanges();
        //                            await db.SaveChangesAsync();
        //                            db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
        //                            ts.Complete();
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        LogFile Logfile = new LogFile();
        //                        ErrorLog Err = new ErrorLog()
        //                        {
        //                            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                            ExceptionMessage = ex.Message,
        //                            ExceptionStackTrace = ex.StackTrace,
        //                            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                            LogTime = DateTime.Now
        //                        };
        //                        Logfile.CreateLogFile(Err);
        //                        Msg.Add(ex.Message);
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }
        //                }
        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (FacultyExternal)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //                else
        //                {
        //                    var databaseValues = (FacultyExternal)databaseEntry.ToObject();
        //                    c.RowVersion = databaseValues.RowVersion;
        //                }
        //            }
        //            Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    else
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {

        //            FacultyExternal blog = null; // to retrieve old data
        //            DbPropertyValues originalBlogValues = null;
        //            FacultyExternal Old_Corp = null;

        //            using (var context = new DataBaseContext())
        //            {
        //                blog = context.FacultyExternal.Where(e => e.Id == data).SingleOrDefault();
        //                originalBlogValues = context.Entry(blog).OriginalValues;
        //            }
        //            c.DBTrack = new DBTrack
        //            {
        //                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                Action = "M",
        //                IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                ModifiedBy = SessionManager.UserName,
        //                ModifiedOn = DateTime.Now
        //            };

        //            if (TempData["RowVersion"] == null)
        //            {
        //                TempData["RowVersion"] = blog.RowVersion;
        //            }

        //            FacultyExternal corp = new FacultyExternal()
        //            {
        //                Code = c.Code,
        //                Narration = c.Narration,
        //                Id = data,
        //                DBTrack = c.DBTrack,
        //                RowVersion = (Byte[])TempData["RowVersion"]
        //            };


        //            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //            using (var context = new DataBaseContext())
        //            {
        //                var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "FacultyExternal", c.DBTrack);
        //                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                Old_Corp = context.FacultyExternal.Where(e => e.Id == data)
        //                    .Include(e => e.TrainingInstitue).Include(e => e.TrainingInstitue).SingleOrDefault();
        //                DT_FacultyExternal DT_Corp = (DT_FacultyExternal)obj;
        //                DT_Corp.TrainingInstitue_Id = DBTrackFile.ValCompare(Old_Corp.TrainingInstitue, c.TrainingInstitue);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                // DT_Corp.FacultyType_Id = DBTrackFile.ValCompare(Old_Corp.FacultyType, c.FacultyType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                db.Create(DT_Corp);
        //                //db.SaveChanges();
        //            }
        //            blog.DBTrack = c.DBTrack;
        //            db.FacultyExternal.Attach(blog);
        //            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            db.SaveChanges();
        //            ts.Complete();
        //            Msg.Add("  Record Updated");
        //            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    return View();

        //}


        [HttpPost]
        public async Task<ActionResult> EditSave(FacultyExternal c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var db_data = db.FacultyExternal.Include(e => e.FacultySpecialization).Include(e => e.TrainingInstitue)
                         .Where(e => e.Id == data).SingleOrDefault();

                    //string FacultyType = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    string TrainingInstitute = form["TrainingInstitutelist"] == "0" ? "" : form["TrainingInstitutelist"];
                    string ContactDetails = form["Contact_List"] == "0" ? "" : form["Contact_List"];
                    string Addrs = form["Address_List"] == "0" ? "" : form["Address_List"];
                    bool Auth = form["autho_allow"] == "true" ? true : false;

                    //if (FacultyType != null && FacultyType != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(FacultyType));
                    //    //    db_Data.FacultyType = val; 
                    //}

                    List<FacultySpecialization> lookupLang = new List<FacultySpecialization>();
                    string Lang = form["FacultySpecializationlist"];

                    if (Lang != null)
                    {
                        var ids = Utility.StringIdsToListIds(Lang);
                        foreach (var ca in ids)
                        {
                            var lookup_val = db.FacultySpecialization.Find(ca);

                            lookupLang.Add(lookup_val);
                            c.FacultySpecialization = lookupLang;
                            db_data.FacultySpecialization = lookupLang;
                        }
                    }
                    else
                    {
                        db_data.FacultySpecialization = null;
                        Msg.Add("  Kindly select atleast 1 record for Faculty Specialization ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (Addrs != null)
                    {
                        if (Addrs != "")
                        {
                            int AddId = Convert.ToInt32(Addrs);
                            var val = db.Address.Include(e => e.Area)
                                                .Include(e => e.City)
                                                .Include(e => e.Country)
                                                .Include(e => e.District)
                                                .Include(e => e.State)
                                                .Include(e => e.StateRegion)
                                                .Include(e => e.Taluka)
                                                .Where(e => e.Id == AddId).SingleOrDefault();
                            c.Address = val;
                            db_data.Address = val;
                        }
                    }

                    if (ContactDetails != null)
                    {
                        if (ContactDetails != "")
                        {
                            int ContId = Convert.ToInt32(ContactDetails);
                            var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.ContactDetails = val;
                            db_data.ContactDetails = val;
                        }
                    }


                    if (TrainingInstitute != null && TrainingInstitute != "")
                    {
                        int ContId = Convert.ToInt32(TrainingInstitute);
                        var val = db.TrainingInstitute.Where(e => e.Id == ContId).SingleOrDefault();
                        db_data.TrainingInstitue = val;
                    }
                    else
                    {
                        Msg.Add("  Kindly select atleast 1 record for Training Institute ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                                    db.FacultyExternal.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_Lookup = db.FacultyExternal.Find(data);
                                    TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                    db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        FacultyExternal blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.FacultyExternal.Include(e => e.FacultySpecialization).Include(e => e.TrainingInstitue)
                                                .Where(e => e.Id == data).SingleOrDefault();
                                            originalBlogValues = context.Entry(blog).OriginalValues;
                                        }

                                        c.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };
                                        FacultyExternal FacultyExternal = new FacultyExternal()
                                        {
                                            Id = data,
                                            Code = c.Code == null ? "" : c.Code.Trim(),
                                            Narration = c.Narration == null ? "" : c.Narration.Trim(),
                                            Address = c.Address,
                                            FacultySpecialization = c.FacultySpecialization,
                                            ContactDetails = c.ContactDetails,
                                            TrainingInstitue = c.TrainingInstitue,
                                            DBTrack = c.DBTrack
                                        };


                                        db.FacultyExternal.Attach(FacultyExternal);
                                        db.Entry(FacultyExternal).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(FacultyExternal).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                        // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                        using (var context = new DataBaseContext())
                                        {

                                            var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                            DT_FacultyExternal DT_Corp = (DT_FacultyExternal)obj;

                                            db.Create(DT_Corp);
                                            db.SaveChanges();
                                        }
                                        await db.SaveChangesAsync();
                                        ts.Complete();

                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = FacultyExternal.Id, Val = FacultyExternal.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { lk.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (FacultyExternal)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (FacultyExternal)databaseEntry.ToObject();
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

                            FacultyExternal blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            FacultyExternal Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.FacultyExternal.Include(e => e.FacultySpecialization).Include(e => e.TrainingInstitue)
                                    .Where(e => e.Id == data).SingleOrDefault();
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
                            FacultyExternal FacultyExternal = new FacultyExternal()
                            {
                                Id = data,
                                Code = c.Code == null ? "" : c.Code.Trim(),
                                Narration = c.Narration == null ? "" : c.Narration.Trim(),
                                Address = c.Address,
                                FacultySpecialization = c.FacultySpecialization,
                                ContactDetails = c.ContactDetails,
                                TrainingInstitue = c.TrainingInstitue,
                                DBTrack = c.DBTrack
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, FacultyExternal, "FacultyExternal", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.FacultyExternal.Include(e => e.FacultySpecialization).Include(e => e.TrainingInstitue)
                                    .Where(e => e.Id == data).SingleOrDefault();
                                DT_FacultyExternal DT_Corp = (DT_FacultyExternal)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.FacultyExternal.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, c.Institute, "Record Updated", JsonRequestBehavior.AllowGet });
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
                    FacultyExternal FacExt = db.FacultyExternal.Include(e => e.TrainingInstitue).Include(q => q.FacultySpecialization).Where(e => e.Id == data).SingleOrDefault();

                    TrainingInstitute add = FacExt.TrainingInstitue;

                    if (FacExt.FacultySpecialization != null)
                    {
                        Msg.Add(" Child record exists.Cannot remove it..  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (FacExt.TrainingInstitue != null)
                    {
                        Msg.Add(" Child record exists.Cannot remove it..  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    //    ContactDetails conDet = FacExt.FacultySpecialization;
                    //   LookupValue val = FacExt.FacultyType;
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (FacExt.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = FacExt.DBTrack.CreatedBy != null ? FacExt.DBTrack.CreatedBy : null,
                                CreatedOn = FacExt.DBTrack.CreatedOn != null ? FacExt.DBTrack.CreatedOn : null,
                                IsModified = FacExt.DBTrack.IsModified == true ? true : false
                            };
                            FacExt.DBTrack = dbT;
                            db.Entry(FacExt).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, FacExt.DBTrack);
                            DT_FacultyExternal DT_Corp = (DT_FacultyExternal)rtn_Obj;
                            DT_Corp.TrainingInstitue_Id = FacExt.TrainingInstitue == null ? 0 : FacExt.TrainingInstitue.Id;
                            //  DT_Corp.FacultyType_Id = FacExt.FacultyType == null ? 0 : FacExt.FacultyType.Id;
                            //DT_Corp.FacultySpecilisation_Id = FacExt.FacultySpecialization == null ? 0 : FacExt.FacultySpecialization;
                            db.Create(DT_Corp);
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
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = FacExt.DBTrack.CreatedBy != null ? FacExt.DBTrack.CreatedBy : null,
                                CreatedOn = FacExt.DBTrack.CreatedOn != null ? FacExt.DBTrack.CreatedOn : null,
                                IsModified = FacExt.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };
                            var v = db.TrainingSession.Where(a => a.Faculty.Id == FacExt.Id).ToList();
                            if (v.Count != 0)
                            {
                                Msg.Add("child record exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            db.Entry(FacExt).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dbT);
                            DT_FacultyExternal DT_Corp = (DT_FacultyExternal)rtn_Obj;
                            DT_Corp.TrainingInstitue_Id = add == null ? 0 : add.Id;
                            // DT_Corp.FacultyType_Id = val == null ? 0 : val.Id;
                            // DT_Corp.FacultySpecilisation_Id = conDet == null ? 0 : conDet.Id;
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
                            Msg.Add("  Data removed successfully.  ");
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
        public ActionResult Createcontactdetails_partial()
        {
            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        }
        public ActionResult Editcontactdetails_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = (from ca in db.ContactDetails
                         .Where(e => e.Id == data)
                         select new
                         {
                             Id = ca.Id,
                             EmailId = ca.EmailId,
                             FaxNo = ca.FaxNo,
                             Website = ca.Website
                         }).ToList();

                var a = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => e.Id == data).SingleOrDefault();
                var b = a.ContactNumbers;

                var r1 = (from s in b
                          select new
                          {
                              Id = s.Id,
                              FullContactNumbers = s.FullContactNumbers
                          }).ToList();

                TempData["RowVersion"] = db.ContactDetails.Find(data).RowVersion;
                return Json(new object[] { r, r1 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookupDetailsContact(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactDetails.Include(e => e.ContactNumbers).ToList();
                IEnumerable<ContactDetails> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ContactDetails.ToList().Where(d => d.FullContactDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.FacultyInternalCode, c.FacultyInternalName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            // return View();
        }

        [HttpPost]
        public ActionResult ContactDetailsRemove(int? data, int? forwarddata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                ContactDetails contDet = db.ContactDetails.Find(data);
                FacultyExternal loc = db.FacultyExternal.Find(forwarddata);
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        loc.ContactDetails = null;
                        db.Entry(loc).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ts.Complete();
                    }

                    return Json(new Object[] { null, "Data Remove successfully." }, JsonRequestBehavior.AllowGet);
                }

                catch (DataException /* dex */)
                {

                    return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });

                }
            }
        }

        [HttpPost]
        public ActionResult GetLookupDetailsAddress(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Address.Include(e => e.Country).Include(e => e.State).Include(e => e.StateRegion)
                    .Include(e => e.District).Include(e => e.Taluka).Include(e => e.City).Include(e => e.Area).ToList();
                IEnumerable<Address> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Address.ToList().Where(d => d.FullAddress.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Address3 }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
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
                IEnumerable<FacultyExternal> FacultyExternal = null;
                if (gp.IsAutho == true)
                {
                    FacultyExternal = db.FacultyExternal.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    FacultyExternal = db.FacultyExternal.AsNoTracking().ToList();
                }

                IEnumerable<FacultyExternal> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = FacultyExternal;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.Code, a.Narration }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToLower() == gp.searchString.ToLower()) || (e.Narration.ToLower() == gp.searchString.ToLower()))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Narration }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = FacultyExternal;
                    Func<FacultyExternal, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                         gp.sidx == "Narration" ? c.Narration :
                                         gp.sidx == "Narration" ? c.Narration : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Narration) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Narration) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Narration }).ToList();
                    }
                    totalRecords = FacultyExternal.Count();
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
}
