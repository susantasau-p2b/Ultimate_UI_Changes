///
/// Created by Tanushri
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
using P2BUltimate.Security;
using System.IO;

namespace P2BUltimate.Controllers.Training.MainController
{
    public class TrainingMaterialController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();

        //public ActionResult Index()
        //{
        //    return View("~/Views/Training/MainViews/Venue/Index.cshtml");
        //}


        public ActionResult partial()
        {
            return View("~/Views/Shared/Training/_TrainingMaterial.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(HttpPostedFileBase[] files, TrainingMaterial c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string extension, newfilename = "";
                Int32 Count = 0;


                string ServerSavePath = "";
                string NewPath = "";
                string deletefilepath = "";

                List<String> Msg = new List<String>();
                try
                {
                    if (ModelState.IsValid)
                    {

                        using (TransactionScope ts = new TransactionScope())
                        {

                            var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", ".jpeg", ".pdf" };
                            foreach (HttpPostedFileBase file in files)
                            {
                                if (file == null)
                                {
                                    return Json(new { success = false, responseText = "Please Select The File..!" }, JsonRequestBehavior.AllowGet);
                                }
                                extension = Path.GetExtension(file.FileName);
                                if (!allowedExtensions.Contains(extension))
                                {
                                    return Json(new { success = false, responseText = "File Type Is Not Supported..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }

                            foreach (HttpPostedFileBase file in files)
                            {

                                if (file != null)
                                {
                                    extension = Path.GetExtension(file.FileName);

                                    newfilename = c.FileName + extension;

                                    String FolderName = "" ;

                                    var InputFileName = Path.GetFileName(file.FileName);
                                    ServerSavePath = InvestmentUploadFile(FolderName);
                                    string ServerMappath = ServerSavePath + newfilename;

                                    deletefilepath = ServerMappath;
                                    if (deletefilepath != null)
                                    {
                                        FileInfo File = new FileInfo(deletefilepath);
                                        bool exists = File.Exists;
                                        if (exists)
                                        {
                                            System.IO.File.Delete(deletefilepath);
                                        }
                                    }
                                    NewPath = Path.Combine(ServerSavePath, newfilename);
                                    file.SaveAs(Path.Combine(ServerSavePath, newfilename));

                                    Count++;
                                }
                                else
                                {

                                }
                            }


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            TrainingMaterial corporate = new TrainingMaterial()
                            {
                                FileName = c.FileName,
                                MaterialFilePath = NewPath,

                                DBTrack = c.DBTrack
                            };

                            db.TrainingMaterial.Add(corporate);
                            db.SaveChanges();

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





        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var un = db.TrainingMaterial.Where(e => e.Id == data).Select
                    (e => new
                    {

                        FileName = e.FileName,
                        MaterialFilePath = e.MaterialFilePath,
                        Action = e.DBTrack.Action
                    }).ToList();


                var Corp = db.TrainingMaterial.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { un, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(TrainingMaterial c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            TrainingMaterial blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.TrainingMaterial.Where(e => e.Id == data)
                                                        .SingleOrDefault();
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


                            TrainingMaterial corp = new TrainingMaterial()
                            {
                                FileName = c.FileName,
                                MaterialFilePath = c.MaterialFilePath,


                                DBTrack = c.DBTrack,
                                Id = data
                            };


                            db.TrainingMaterial.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            db.SaveChanges();
                            await db.SaveChangesAsync();
                            ts.Complete();

                            Msg.Add("Record Updated Successfully.");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }


                    }

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (Corporate)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (Corporate)databaseEntry.ToObject();
                        c.RowVersion = databaseValues.RowVersion;

                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return View();
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
        //        var jsonData = (Object)null;
        //        IEnumerable<Venue> Venue = null;
        //        if (gp.IsAutho == true)
        //        {
        //            Venue = db.Venue.Include(e => e.VenuType).Include(e => e.Address).Include(e => e.Address.City).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            Venue = db.Venue.Include(e => e.VenuType).Include(e => e.Address).Include(e => e.Address.City).AsNoTracking().ToList();
        //        }

        //        IEnumerable<Venue> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = Venue;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.Name, a.ContactPerson, a.VenuType.LookupVal, a.Address.City.FullDetails, }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Name.ToLower() == gp.searchString.ToLower()) || (e.LookupVal.ToString() == gp.searchString.ToLower()) || (e.ContactPerson.ToString() == gp.searchString.ToLower()) || (e.FullDetails.ToString() == gp.searchString.ToLower()))).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Fees), Convert.ToString(a.Name), Convert.ToString(a.VenuType) != null ? Convert.ToString(a.VenuType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.ContactPerson, a.VenuType.LookupVal, a.Address.City.FullDetails }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = Venue;
        //            Func<Venue, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Name" ? c.Name :
        //                                    gp.sidx == "ContactPerson" ? c.ContactPerson :
        //                                    gp.sidx == "VenueType" ? c.VenuType.LookupVal.ToString() :
        //                                    gp.sidx == "Address" ? c.Address.City.FullDetails : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Name, a.ContactPerson, a.VenuType.LookupVal, a.Address.City.FullDetails }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Name, a.ContactPerson, a.VenuType.LookupVal, a.Address.City.FullDetails }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.ContactPerson, a.VenuType.LookupVal, a.Address.City.FullDetails }).ToList();
        //            }
        //            totalRecords = Venue.Count();
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

        //[HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<String> Msg = new List<String>();
        //        Venue VENUE = db.Venue
        //            //.Include(e => e.Address)
        //                                           .Include(e => e.ContactDetails)
        //                                           .Include(e => e.VenuType).Where(e => e.Id == data).SingleOrDefault();

        //        // Address add = VENUE.Address;
        //        ContactDetails conDet = VENUE.ContactDetails;
        //        LookupValue val = VENUE.VenuType;
        //        //Venue VENUE = db.Venue.Where(e => e.Id == data).SingleOrDefault();
        //        if (VENUE.DBTrack.IsModified == true)
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {
        //                //DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, VENUE.DBTrack, VENUE, null, "Venue");
        //                DBTrack dbT = new DBTrack
        //                {
        //                    Action = "D",
        //                    CreatedBy = VENUE.DBTrack.CreatedBy != null ? VENUE.DBTrack.CreatedBy : null,
        //                    CreatedOn = VENUE.DBTrack.CreatedOn != null ? VENUE.DBTrack.CreatedOn : null,
        //                    IsModified = VENUE.DBTrack.IsModified == true ? true : false
        //                };
        //                VENUE.DBTrack = dbT;
        //                db.Entry(VENUE).State = System.Data.Entity.EntityState.Modified;
        //                var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, VENUE.DBTrack);
        //                DT_Venue DT_OBJ = (DT_Venue)rtn_Obj;
        //                //DT_OBJ.Address_Id = VENUE.Address == null ? 0 : VENUE.Address.Id;
        //                DT_OBJ.VenuType_Id = VENUE.VenuType == null ? 0 : VENUE.VenuType.Id;
        //                DT_OBJ.ContactDetails_Id = VENUE.ContactDetails == null ? 0 : VENUE.ContactDetails.Id;
        //                db.Create(DT_OBJ);
        //                // db.SaveChanges();
        //                //DBTrackFile.DBTrackSave("Training/Training", "D", VENUE, null, "Venue", VENUE.DBTrack);
        //                await db.SaveChangesAsync();
        //                //using (var context = new DataBaseContext())
        //                //{
        //                //   // DBTrackFile.DBTrackSave("Training/Training", "D", VENUE, null, "Venue", VENUE.DBTrack );
        //                //}
        //                ts.Complete();
        //                Msg.Add("Data removed.");
        //                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //        else
        //        {
        //            //var selectedRegions = VENUE.Regions;

        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {

        //                try
        //                {
        //                    DBTrack dbT = new DBTrack
        //                    {
        //                        Action = "D",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now,
        //                        CreatedBy = VENUE.DBTrack.CreatedBy != null ? VENUE.DBTrack.CreatedBy : null,
        //                        CreatedOn = VENUE.DBTrack.CreatedOn != null ? VENUE.DBTrack.CreatedOn : null,
        //                        IsModified = VENUE.DBTrack.IsModified == true ? false : false//,
        //                        //AuthorizedBy = SessionManager.UserName,
        //                        //AuthorizedOn = DateTime.Now
        //                    };

        //                    // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

        //                    db.Entry(VENUE).State = System.Data.Entity.EntityState.Deleted;
        //                    var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dbT);
        //                    DT_Venue DT_OBJ = (DT_Venue)rtn_Obj;
        //                    // DT_OBJ.Address_Id = add == null ? 0 : add.Id;
        //                    DT_OBJ.VenuType_Id = val == null ? 0 : val.Id;
        //                    DT_OBJ.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
        //                    db.Create(DT_OBJ);

        //                    await db.SaveChangesAsync();

        //                    ts.Complete();
        //                    Msg.Add("Data removed.");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

        //                }
        //                catch (RetryLimitExceededException /* dex */)
        //                {
        //                    //Log the error (uncomment dex variable name and add a line here to write a log.)
        //                    //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
        //                    //return RedirectToAction("Delete");
        //                    Msg.Add("Unable to delete. Try again, and if the problem persists, see your system administrator.");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
        //                }
        //            }
        //        }
        //    }
        //}


        //public ActionResult GetContactDetLKDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.ContactDetails.Include(e => e.ContactNumbers).ToList();
        //        IEnumerable<ContactDetails> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.ContactDetails.ToList().Where(d => d.FullContactDetails.Contains(data));

        //        }
        //        else
        //        {
        //            var list1 = db.ContactDetails.ToList();
        //            //var list2 = fall.Except(list1);
        //            //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
        //            var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.CorporateCode, c.CorporateName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullContactDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    // return View();
        //}

        ////public int EditS(string LKVAL, string ContactDetails, int data, Venue c, DBTrack dbT)
        ////{
        ////    if (LKVAL != null)
        ////    {
        ////        if (LKVAL != "")
        ////        {
        ////            var val = db.LookupValue.Find(int.Parse(LKVAL));
        ////            c.VenuType = val;

        ////            var type = db.Venue.Include(e => e.VenuType).Where(e => e.Id == data).SingleOrDefault();
        ////            IList<Venue> typedetails = null;
        ////            if (type.VenuType != null)
        ////            {
        ////                typedetails = db.Venue.Where(x => x.VenuType.Id == type.VenuType.Id && x.Id == data).ToList();
        ////            }
        ////            else
        ////            {
        ////                typedetails = db.Venue.Where(x => x.Id == data).ToList();
        ////            }
        ////            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        ////            foreach (var s in typedetails)
        ////            {
        ////                s.VenuType = c.VenuType;
        ////                db.Venue.Attach(s);
        ////                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        ////                //await db.SaveChangesAsync();
        ////                db.SaveChanges();
        ////                TempData["RowVersion"] = s.RowVersion;
        ////                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        ////            }
        ////        }
        ////        else
        ////        {
        ////            var VenueTypeDetails = db.Venue.Include(e => e.VenuType).Where(x => x.Id == data).ToList();
        ////            foreach (var s in VenueTypeDetails)
        ////            {
        ////                s.VenuType = null;
        ////                db.Venue.Attach(s);
        ////                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        ////                //await db.SaveChangesAsync();
        ////                db.SaveChanges();
        ////                TempData["RowVersion"] = s.RowVersion;
        ////                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        ////            }
        ////        }
        ////    }
        ////    else
        ////    {
        ////        var VenueTypeDetails = db.Venue.Include(e => e.VenuType).Where(x => x.Id == data).ToList();
        ////        foreach (var s in VenueTypeDetails)
        ////        {
        ////            s.VenuType = null;
        ////            db.Venue.Attach(s);
        ////            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        ////            //await db.SaveChangesAsync();
        ////            db.SaveChanges();
        ////            TempData["RowVersion"] = s.RowVersion;
        ////            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        ////        }
        ////    }


        ////    if (ContactDetails != null)
        ////    {
        ////        if (ContactDetails != "")
        ////        {
        ////            var val = db.ContactDetails.Find(int.Parse(ContactDetails));
        ////            c.ContactDetails = val;

        ////            var add = db.Venue.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
        ////            IList<Venue> contactsdetails = null;
        ////            if (add.ContactDetails != null)
        ////            {
        ////                contactsdetails = db.Venue.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
        ////            }
        ////            else
        ////            {
        ////                contactsdetails = db.Venue.Where(x => x.Id == data).ToList();
        ////            }
        ////            foreach (var s in contactsdetails)
        ////            {
        ////                s.ContactDetails = c.ContactDetails;
        ////                db.Venue.Attach(s);
        ////                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        ////                //await db.SaveChangesAsync();
        ////                db.SaveChanges();
        ////                TempData["RowVersion"] = s.RowVersion;
        ////                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        ////            }
        ////        }
        ////    }
        ////    else
        ////    {
        ////        var contactsdetails = db.Venue.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
        ////        foreach (var s in contactsdetails)
        ////        {
        ////            s.ContactDetails = null;
        ////            db.Venue.Attach(s);
        ////            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        ////            //await db.SaveChangesAsync();
        ////            db.SaveChanges();
        ////            TempData["RowVersion"] = s.RowVersion;
        ////            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        ////        }
        ////    }

        ////    var CurCorp = db.Venue.Find(data);
        ////    TempData["CurrRowVersion"] = CurCorp.RowVersion;
        ////    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        ////    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        ////    {
        ////        c.DBTrack = dbT;
        ////        Venue OBJ = new Venue()
        ////        {
        ////            Fees = c.Fees,
        ////            Name = c.Name,
        ////            ContactPerson = c.ContactPerson,
        ////            Narration = c.Narration,
        ////            Id = data,
        ////            DBTrack = c.DBTrack
        ////        };


        ////        db.Venue.Attach(OBJ);
        ////        db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
        ////        db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
        ////        //// DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
        ////        return 1;
        ////    }
        ////    return 0;
        ////}


        //public int EditS(string Corp, string ContactDetails, int data, Venue c, DBTrack dbT)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        if (Corp != null)
        //        {
        //            if (Corp != "")
        //            {
        //                var val = db.LookupValue.Find(int.Parse(Corp));
        //                c.VenuType = val;

        //                var type = db.Venue.Include(e => e.VenuType).Where(e => e.Id == data).SingleOrDefault();
        //                IList<Venue> typedetails = null;
        //                if (type.VenuType != null)
        //                {
        //                    typedetails = db.Venue.Where(x => x.VenuType.Id == type.VenuType.Id && x.Id == data).ToList();
        //                }
        //                else
        //                {
        //                    typedetails = db.Venue.Where(x => x.Id == data).ToList();
        //                }
        //                //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                foreach (var s in typedetails)
        //                {
        //                    s.VenuType = c.VenuType;
        //                    db.Venue.Attach(s);
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                    //await db.SaveChangesAsync();
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = s.RowVersion;
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                }
        //            }
        //            else
        //            {
        //                var BusiTypeDetails = db.Venue.Include(e => e.VenuType).Where(x => x.Id == data).ToList();
        //                foreach (var s in BusiTypeDetails)
        //                {
        //                    s.VenuType = null;
        //                    db.Venue.Attach(s);
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                    //await db.SaveChangesAsync();
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = s.RowVersion;
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var BusiTypeDetails = db.Venue.Include(e => e.VenuType).Where(x => x.Id == data).ToList();
        //            foreach (var s in BusiTypeDetails)
        //            {
        //                s.VenuType = null;
        //                db.Venue.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }

        //        if (ContactDetails != null)
        //        {
        //            if (ContactDetails != "")
        //            {
        //                var val = db.ContactDetails.Find(int.Parse(ContactDetails));
        //                c.ContactDetails = val;

        //                var add = db.Venue.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
        //                IList<Venue> contactsdetails = null;
        //                if (add.ContactDetails != null)
        //                {
        //                    contactsdetails = db.Venue.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
        //                }
        //                else
        //                {
        //                    contactsdetails = db.Venue.Where(x => x.Id == data).ToList();
        //                }
        //                foreach (var s in contactsdetails)
        //                {
        //                    s.ContactDetails = c.ContactDetails;
        //                    db.Venue.Attach(s);
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                    //await db.SaveChangesAsync();
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = s.RowVersion;
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var contactsdetails = db.Venue.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
        //            foreach (var s in contactsdetails)
        //            {
        //                s.ContactDetails = null;
        //                db.Venue.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }

        //        var CurCorp = db.Venue.Find(data);
        //        TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //        {
        //            c.DBTrack = dbT;
        //            Venue corp = new Venue()
        //            {
        //                Fees = c.Fees,
        //                Name = c.Name,
        //                ContactPerson = c.ContactPerson,
        //                Narration = c.Narration,
        //                VenuType = c.VenuType,
        //                ContactDetails = c.ContactDetails,
        //                Id = data,
        //                DBTrack = c.DBTrack
        //            };


        //            db.Venue.Attach(corp);
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            return 1;
        //        }
        //        return 0;
        //    }



        //}

        //[HttpPost]
        //public ActionResult GetAddressLKDetails(List<int> SkipIds)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        //List<string> Ids = SkipIds.ToString();
        //        var fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
        //                             .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
        //                             .Include(e => e.Taluka).ToList();

        //        if (SkipIds != null)
        //        {
        //            foreach (var a in SkipIds)
        //            {
        //                if (fall == null)
        //                    fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
        //                            .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
        //                            .Include(e => e.Taluka).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
        //                else
        //                    fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

        //            }
        //        }

        //        var list1 = db.Venue.ToList().Select(e => e.Address);
        //        var list2 = fall.Except(list1);

        //        //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
        //        var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();

        //        return Json(r, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public string InvestmentUploadFile(string FolderName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\TrainingMaterial\\";
            String localPath = "";
            bool exists = System.IO.Directory.Exists(requiredPath);
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            return localPath;
        }


        public ActionResult CheckUploadFile(string id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";
                TrainingMaterial trainingmaterial = null;

                int subid = Convert.ToInt32(id);
                trainingmaterial = db.TrainingMaterial.Where(e => e.Id == subid).SingleOrDefault();
                if (trainingmaterial.MaterialFilePath != null)
                {
                    localpath = trainingmaterial.MaterialFilePath;
                }

                FileInfo file = new FileInfo(localpath);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);
                if (exists)
                {
                    return Json(new { success = true, fileextension = extension }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseText = "File Not Uploaded..!" }, JsonRequestBehavior.AllowGet);

                }
            }
            return null;
        }


        public ActionResult GetCompImage(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";

                TrainingMaterial trainingmaterial = null;

                int subid = Convert.ToInt32(data);

                trainingmaterial = db.TrainingMaterial.Where(e => e.Id == subid).SingleOrDefault();
                if (trainingmaterial.MaterialFilePath != null)
                {
                    localpath = trainingmaterial.MaterialFilePath;
                }
                else
                {
                    return View("File Not Found");
                    //return Content("File Not Found");
                    //return Json(new { success = false, responseText = "File Not Found..!" }, JsonRequestBehavior.AllowGet);
                }

                FileInfo file = new FileInfo(localpath);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);

                if (exists)
                {
                    if (extension == ".pdf")
                    {
                        return File(file.FullName, "application/pdf", file.Name + " ");


                        //string pdf="pdf";
                        //byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");

                        //string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = pdf }, JsonRequestBehavior.AllowGet);
                    }
                    if (extension == ".jpg")
                    {
                        // return File(file.FullName, "image/png", file.Name + " ");
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);

                        JsonRequestBehavior behaviou = new JsonRequestBehavior();
                        return new JsonResult()
                        {
                            Data = base64ImageRepresentation,
                            MaxJsonLength = 86753090,
                            JsonRequestBehavior = behaviou
                        };

                    }
                    if (extension == ".png")
                    {
                        //return File(file.FullName, "image/png", file.Name + " ");
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);

                        JsonRequestBehavior behaviou = new JsonRequestBehavior();
                        return new JsonResult()
                        {
                            Data = base64ImageRepresentation,
                            MaxJsonLength = 86753090,
                            JsonRequestBehavior = behaviou
                        };

                    }
                }
                else
                {
                    return Content("File Not Found");
                    //return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                }
                return null;
            }


        }

    }




}
