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

namespace P2BUltimate.Controllers.Training.MainController
{
    public class _EmpAssignParaController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();

        //public ActionResult Index()
        //{
        //    return View("~/Views/Training/MainViews/Venue/Index.cshtml");
        //}


        public ActionResult partial()
        {
            return View("~/Views/Shared/Training/_EmpAssignPara.cshtml");
        }

        //[HttpPost]
        //// [ValidateAntiForgeryToken]
        //public ActionResult Create(Venue c, FormCollection form) //Create submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        string Category = form["VenuTypelist"] == "0" ? "" : form["VenuTypelist"];
        //        string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
        //        string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];

        //        var id = int.Parse(Session["CompId"].ToString());
        //        var companypayroll = db.CompanyTraining.Where(e => e.Company.Id == id).SingleOrDefault();

        //        List<String> Msg = new List<String>();
        //        if (Category != null && Category != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(Category));
        //            c.VenuType = val;
        //        }



        //        if (ContactDetails != null && ContactDetails != "")
        //        {
        //            int ContId = Convert.ToInt32(ContactDetails);
        //            var val = db.ContactDetails.Include(e => e.ContactNumbers)
        //                                .Where(e => e.Id == ContId).SingleOrDefault();
        //            c.ContactDetails = val;
        //        }

        //        if (Addrs != null)
        //        {
        //            if (Addrs != "")
        //            {
        //                int AddId = Convert.ToInt32(Addrs);
        //                var val = db.Address//.Include(e => e.Area)
        //                    //.Include(e => e.City)
        //                    //.Include(e => e.Country)
        //                    //.Include(e => e.District)
        //                    //.Include(e => e.State)
        //                    //.Include(e => e.StateRegion)
        //                    //.Include(e => e.Taluka)
        //                                    .Where(e => e.Id == AddId).SingleOrDefault();
        //                c.Address = val;
        //            }
        //        }

        //        if (ModelState.IsValid)
        //        {
        //            using (TransactionScope ts = new TransactionScope())
        //            {
        //                if (db.Venue.Any(o => o.Name == c.Name))
        //                {


        //                    Msg.Add("  Record Already Exists.  ");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }

        //                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

        //                Venue Venue = new Venue()
        //                {
        //                    Fees = c.Fees,
        //                    Name = c.Name == null ? "" : c.Name.Trim(),
        //                    Narration = c.Narration == null ? "" : c.Narration.Trim(),
        //                    ContactPerson = c.ContactPerson == null ? "" : c.ContactPerson.Trim(),
        //                    VenuType = c.VenuType,
        //                    Address = c.Address,
        //                    ContactDetails = c.ContactDetails,
        //                    DBTrack = c.DBTrack
        //                };
        //                try
        //                {
        //                    db.Venue.Add(Venue);
        //                    var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, c.DBTrack);
        //                    DT_Venue DT_OBJ = (DT_Venue)rtn_Obj;
        //                    DT_OBJ.VenuType_Id = c.VenuType == null ? 0 : c.VenuType.Id;
        //                    DT_OBJ.Address_Id = c.Address == null ? 0 : c.Address.Id;
        //                    DT_OBJ.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
        //                    db.Create(DT_OBJ);
        //                    db.SaveChanges();
        //                    //DBTrackFile.DBTrackSave("Training/Training", "C", Venue, null, "Venue", null);

        //                    if (companypayroll != null)
        //                    {
        //                        List<Venue> pfmasterlist = new List<Venue>();
        //                        pfmasterlist.Add(Venue);
        //                        companypayroll.Venue = pfmasterlist;
        //                        db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
        //                    }
        //                    ts.Complete();
        //                    Msg.Add("Data Saved Successfully.");
        //                    return Json(new Utility.JsonReturnClass { Id = Venue.Id, Val = Venue.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return this.Json(new Object[] { Venue.Id, Venue.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
        //                }
        //                catch (DbUpdateConcurrencyException)
        //                {
        //                    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
        //                }
        //                catch (DataException /* dex */)
        //                {
        //                    Msg.Add("Unable to create. Try again, and if the problem persists contact your system administrator.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //        }
        //        else
        //        {
        //            StringBuilder sb = new StringBuilder("");
        //            foreach (ModelState modelState in ModelState.Values)
        //            {
        //                foreach (ModelError error in modelState.Errors)
        //                {
        //                    sb.Append(error.ErrorMessage);
        //                    sb.Append("." + "\n");
        //                }
        //            }
        //            var errorMsg = sb.ToString();
        //            Msg.Add(errorMsg);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //            //return this.Json(new { msg = errorMsg });
        //        }
        //    }
        //}

        //public ActionResult Editcontactdetails_partial(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var r = (from ca in db.ContactDetails
        //                 .Where(e => e.Id == data)
        //                 select new
        //                 {
        //                     Id = ca.Id,
        //                     EmailId = ca.EmailId,
        //                     FaxNo = ca.FaxNo,
        //                     Website = ca.Website
        //                 }).ToList();

        //        var a = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => e.Id == data).SingleOrDefault();
        //        var b = a.ContactNumbers;

        //        var r1 = (from s in b
        //                  select new
        //                  {
        //                      Id = s.Id,
        //                      FullContactNumbers = s.FullContactNumbers
        //                  }).ToList();

        //        TempData["RowVersion"] = db.ContactDetails.Find(data).RowVersion;
        //        return Json(new object[] { r, r1 }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //public ActionResult Createcontactdetails_partial()
        //{
        //    return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        //}



        //[HttpPost]
        //public ActionResult Edit(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var Q = db.Venue
        //            .Include(e => e.ContactDetails)
        //            .Include(e => e.VenuType)
        //            .Include(e => e.ContactDetails)
        //            .Where(e => e.Id == data).Select
        //            (e => new
        //            {
        //                Fees = e.Fees,
        //                Name = e.Name,
        //                ContactPerson = e.ContactPerson,
        //                Narration = e.Narration,
        //                VenuType_Id = e.VenuType.Id == null ? 0 : e.VenuType.Id,
        //                Action = e.DBTrack.Action
        //            }).ToList();

        //        var add_data = db.Venue
        //          .Include(e => e.ContactDetails)
        //            .Include(e => e.VenuType)
        //            .Include(e => e.ContactDetails)
        //             .Include(e => e.Address.Area)
        //            .Include(e => e.Address.City)
        //            .Include(e => e.Address.Country)
        //            .Include(e => e.Address.District)
        //            .Include(e => e.Address.State)
        //            .Include(e => e.Address.StateRegion)
        //            .Include(e => e.Address.Taluka)
        //            .Where(e => e.Id == data)
        //            .Select(e => new
        //            {
        //                Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
        //                Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
        //                ContactDetails_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
        //                ContactDetails_FullDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails
        //            }).ToList();


        //        var W = db.DT_Venue
        //             .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
        //             (e => new
        //             {
        //                 DT_Id = e.Id,
        //                 Fees = e.Fees == null ? 0.0 : e.Fees,
        //                 Name = e.Name == null ? "" : e.Name,
        //                 ContactPerson = e.ContactPerson == null ? "" : e.ContactPerson,
        //                 Narration = e.Narration == null ? "" : e.Narration,
        //                 VenueType_Val = e.VenuType_Id == 0 ? "" : db.LookupValue
        //                            .Where(x => x.Id == e.VenuType_Id)
        //                            .Select(x => x.LookupVal).FirstOrDefault(),

        //                 //Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
        //                 Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
        //             }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

        //        var Corp = db.Venue.Find(data);
        //        TempData["RowVersion"] = Corp.RowVersion;
        //        var Auth = Corp.DBTrack.IsModified;
        //        return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
        //    }
        //}

        ////[HttpPost]
        ////public async Task<ActionResult> EditSave(Venue c, int data, FormCollection form) // Edit submit
        ////{
        ////    string Corp = form["VenuTypelist"] == "0" ? "" : form["VenuTypelist"]; 
        ////    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
        ////    bool Auth = form["Autho_Allow"] == "true" ? true : false;

        ////    List<String> Msg = new List<String>();

        ////    if (Corp != null && Corp != "")
        ////    { 
        ////            var val = db.LookupValue.Find(int.Parse(Corp));
        ////            c.VenuType = val; 
        ////    }



        ////    if (ContactDetails != null && ContactDetails != "")
        ////    { 
        ////            int ContId = Convert.ToInt32(ContactDetails);
        ////            var val = db.ContactDetails.Include(e => e.ContactNumbers)
        ////                                .Where(e => e.Id == ContId).SingleOrDefault();
        ////            c.ContactDetails = val; 
        ////    }


        ////    if (Auth == false)
        ////    {
        ////        if (ModelState.IsValid)
        ////        {
        ////            try
        ////            {

        ////                //DbContextTransaction transaction = db.Database.BeginTransaction();

        ////                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        ////                {
        ////                    Venue blog = null; // to retrieve old data
        ////                    DbPropertyValues originalBlogValues = null;

        ////                    using (var context = new DataBaseContext())
        ////                    {
        ////                        blog = context.Venue.Where(e => e.Id == data).Include(e => e.VenuType)
        ////                            //.Include(e => e.Address)
        ////                                                .Include(e => e.ContactDetails).SingleOrDefault();
        ////                        originalBlogValues = context.Entry(blog).OriginalValues;
        ////                    }

        ////                    c.DBTrack = new DBTrack
        ////                    {
        ////                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        ////                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        ////                        Action = "M",
        ////                        ModifiedBy = SessionManager.UserName,
        ////                        ModifiedOn = DateTime.Now
        ////                    };

        ////                    int a = EditS(Corp, ContactDetails, data, c, c.DBTrack);



        ////                    using (var context = new DataBaseContext())
        ////                    { 
        ////                        var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
        ////                        DT_Venue DT_OBJ = (DT_Venue)obj;
        ////                        //DT_OBJ.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
        ////                        DT_OBJ.VenuType_Id = blog.VenuType == null ? 0 : blog.VenuType.Id;
        ////                        DT_OBJ.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
        ////                        db.Create(DT_OBJ);
        ////                        db.SaveChanges();
        ////                    }
        ////                    await db.SaveChangesAsync();
        ////                    ts.Complete();

        ////                    Msg.Add("Record Updated Successfully.");
        ////                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        ////                    //return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });

        ////                }
        ////            }
        ////            catch (DbUpdateConcurrencyException ex)
        ////            {
        ////                var entry = ex.Entries.Single();
        ////                var clientValues = (Venue)entry.Entity;
        ////                var databaseEntry = entry.GetDatabaseValues();
        ////                if (databaseEntry == null)
        ////                {
        ////                    Msg.Add("Unable to save changes. The record was deleted by another user.");
        ////                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        ////                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        ////                }
        ////                else
        ////                {
        ////                    var databaseValues = (Venue)databaseEntry.ToObject();
        ////                    c.RowVersion = databaseValues.RowVersion;

        ////                }
        ////            }

        ////            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

        ////            Msg.Add("Record modified by another user.So refresh it and try to save again.");
        ////            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        ////        }
        ////    }
        ////    else
        ////    {
        ////        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        ////        {

        ////            Venue blog = null; // to retrieve old data
        ////            DbPropertyValues originalBlogValues = null;
        ////            Venue Old_Corp = null;

        ////            using (var context = new DataBaseContext())
        ////            {
        ////                blog = context.Venue.Where(e => e.Id == data).SingleOrDefault();
        ////                originalBlogValues = context.Entry(blog).OriginalValues;
        ////            }
        ////            c.DBTrack = new DBTrack
        ////            {
        ////                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        ////                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        ////                Action = "M",
        ////                IsModified = blog.DBTrack.IsModified == true ? true : false,
        ////                ModifiedBy = SessionManager.UserName ,
        ////                ModifiedOn = DateTime.Now
        ////            };
        ////            Venue corp = new Venue()
        ////            {
        ////                Fees = c.Fees,
        ////                Name = c.Name,
        ////                ContactPerson = c.ContactPerson,
        ////                Narration = c.Narration,
        ////                Id = data,
        ////                DBTrack = c.DBTrack,
        ////                RowVersion = (Byte[])TempData["RowVersion"]
        ////            };


        ////            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        ////            using (var context = new DataBaseContext())
        ////            {
        ////                var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "Venue", c.DBTrack);
        ////                // var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);

        ////                Old_Corp = context.Venue.Where(e => e.Id == data).Include(e => e.VenuType)
        ////                    //.Include(e => e.Address)
        ////                    .Include(e => e.ContactDetails).SingleOrDefault();
        ////                DT_Venue DT_OBJ = (DT_Venue)obj;
        ////                // DT_OBJ.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        ////                DT_OBJ.VenuType_Id = DBTrackFile.ValCompare(Old_Corp.VenuType, c.VenuType); //Old_Corp.VenuType == c.VenuType ? 0 : Old_Corp.VenuType == null && c.VenuType != null ? c.VenuType.Id : Old_Corp.VenuType.Id;
        ////                DT_OBJ.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        ////                db.Create(DT_OBJ);
        ////                //db.SaveChanges();
        ////            }
        ////            blog.DBTrack = c.DBTrack;
        ////            db.Venue.Attach(blog);
        ////            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        ////            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        ////            db.SaveChanges();
        ////            ts.Complete();
        ////            //return Json(new Object[] { blog.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });

        ////            Msg.Add("  Data Updated successfully  ");
        ////            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        ////        }

        ////    }
        ////    return View();

        ////}

        ////[HttpPost]
        ////public async Task<ActionResult> EditSave(Venue c, int data, FormCollection form) // Edit submit
        ////{
        ////    List<string> Msg = new List<string>();
        ////    using (DataBaseContext db = new DataBaseContext())
        ////    {
        ////        try
        ////        {
        ////            string Corp = form["VenuTypelist"] == "0" ? "" : form["VenuTypelist"];
        ////            string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
        ////            bool Auth = form["Autho_Allow"] == "true" ? true : false;


        ////            if (Corp != null && Corp != "")
        ////            {
        ////                var val = db.LookupValue.Find(int.Parse(Corp));
        ////                c.VenuType = val;
        ////            }



        ////            if (ContactDetails != null && ContactDetails != "")
        ////            {
        ////                int ContId = Convert.ToInt32(ContactDetails);
        ////                var val = db.ContactDetails.Include(e => e.ContactNumbers)
        ////                                    .Where(e => e.Id == ContId).SingleOrDefault();
        ////                c.ContactDetails = val;
        ////            }




        ////            if (Auth == false)
        ////            {


        ////                if (ModelState.IsValid)
        ////                {
        ////                    try
        ////                    {

        ////                        //DbContextTransaction transaction = db.Database.BeginTransaction();

        ////                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        ////                        {
        ////                            Venue blog = null; // to retrieve old data
        ////                            DbPropertyValues originalBlogValues = null;

        ////                            using (var context = new DataBaseContext())
        ////                            {
        ////                                blog = context.Venue.Where(e => e.Id == data).Include(e => e.ContactDetails)
        ////                                                        .Include(e => e.VenuType)
        ////                                                       .SingleOrDefault();
        ////                                originalBlogValues = context.Entry(blog).OriginalValues;
        ////                            }

        ////                            c.DBTrack = new DBTrack
        ////                            {
        ////                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        ////                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        ////                                Action = "M",
        ////                                ModifiedBy = SessionManager.UserName,
        ////                                ModifiedOn = DateTime.Now
        ////                            };

        ////                            int a = EditS(Corp, ContactDetails, data, c, c.DBTrack);



        ////                            using (var context = new DataBaseContext())
        ////                            {
        ////                                c.Id = data;


        ////                                /// DBTrackFile.DBTrackSave("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
        ////                                //PropertyInfo[] fi = null;
        ////                                //Dictionary<string, object> rt = new Dictionary<string, object>();
        ////                                //fi = c.GetType().GetProperties();
        ////                                ////foreach (var Prop in fi)
        ////                                ////{
        ////                                ////    if (Prop.Name != "Id" && Prop.Name != "DBTrack" && Prop.Name != "RowVersion")
        ////                                ////    {
        ////                                ////        rt.Add(Prop.Name, Prop.GetValue(c));
        ////                                ////    }
        ////                                ////}
        ////                                //rt = blog.DetailedCompare(c);
        ////                                //rt.Add("Orig_Id", c.Id);
        ////                                //rt.Add("Action", "M");
        ////                                //rt.Add("DBTrack", c.DBTrack);
        ////                                //rt.Add("RowVersion", c.RowVersion);
        ////                                //var aa = DBTrackFile.GetInstance("Core/P2b.Global", "DT_" + "Corporate", rt);
        ////                                //DT_Corporate d = (DT_Corporate)aa;
        ////                                //db.DT_Corporate.Add(d);
        ////                                //db.SaveChanges();

        ////                                //To save data in history table 
        ////                                //var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
        ////                                //DT_Corporate DT_Corp = (DT_Corporate)Obj;
        ////                                //db.DT_Corporate.Add(DT_Corp);
        ////                                //db.SaveChanges();\


        ////                                var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
        ////                                DT_Venue DT_Corp = (DT_Venue)obj;
        ////                                DT_Corp.VenuType_Id = blog.VenuType == null ? 0 : blog.VenuType.Id;
        ////                                DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;

        ////                                db.Create(DT_Corp);

        ////                                db.SaveChanges();
        ////                            }
        ////                            await db.SaveChangesAsync();
        ////                            ts.Complete();



        ////                            Msg.Add(" Record Updated ");
        ////                            return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        ////                        }
        ////                    }
        ////                    catch (DbUpdateConcurrencyException ex)
        ////                    {
        ////                        var entry = ex.Entries.Single();
        ////                        var clientValues = (Venue)entry.Entity;
        ////                        var databaseEntry = entry.GetDatabaseValues();
        ////                        if (databaseEntry == null)
        ////                        {

        ////                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        ////                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        ////                        }
        ////                        else
        ////                        {
        ////                            var databaseValues = (Venue)databaseEntry.ToObject();
        ////                            c.RowVersion = databaseValues.RowVersion;

        ////                        }
        ////                    }

        ////                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        ////                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        ////                }
        ////            }
        ////            else
        ////            {
        ////                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        ////                {

        ////                    Venue blog = null; // to retrieve old data
        ////                    DbPropertyValues originalBlogValues = null;
        ////                    Venue Old_Corp = null;

        ////                    using (var context = new DataBaseContext())
        ////                    {
        ////                        blog = context.Venue.Where(e => e.Id == data).SingleOrDefault();
        ////                        originalBlogValues = context.Entry(blog).OriginalValues;
        ////                    }
        ////                    c.DBTrack = new DBTrack
        ////                    {
        ////                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        ////                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        ////                        Action = "M",
        ////                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        ////                        ModifiedBy = SessionManager.UserName,
        ////                        ModifiedOn = DateTime.Now
        ////                    };

        ////                    if (TempData["RowVersion"] == null)
        ////                    {
        ////                        TempData["RowVersion"] = blog.RowVersion;
        ////                    }

        ////                    Venue corp = new Venue()
        ////                    {
        ////                        Fees = c.Fees,
        ////                        Name = c.Name,
        ////                        ContactPerson = c.ContactPerson,
        ////                        Narration = c.Narration,
        ////                        Id = data,
        ////                        DBTrack = c.DBTrack,
        ////                        RowVersion = (Byte[])TempData["RowVersion"]
        ////                    };


        ////                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        ////                    using (var context = new DataBaseContext())
        ////                    {
        ////                        var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "Venue", c.DBTrack);
        ////                        // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        ////                        Old_Corp = context.Venue.Where(e => e.Id == data).Include(e => e.VenuType)
        ////                            .Include(e => e.ContactDetails).SingleOrDefault();
        ////                        DT_Venue DT_Corp = (DT_Venue)obj;
        ////                        DT_Corp.VenuType_Id = DBTrackFile.ValCompare(Old_Corp.VenuType, c.VenuType);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        ////                        DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;

        ////                        db.Create(DT_Corp);
        ////                        //db.SaveChanges();
        ////                    }
        ////                    blog.DBTrack = c.DBTrack;
        ////                    db.Venue.Attach(blog);
        ////                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        ////                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        ////                    db.SaveChanges();
        ////                    ts.Complete();

        ////                    Msg.Add("  Record Updated");
        ////                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        ////                }

        ////            }
        ////            return View();
        ////        }
        ////        catch (Exception ex)
        ////        {
        ////            LogFile Logfile = new LogFile();
        ////            ErrorLog Err = new ErrorLog()
        ////            {
        ////                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        ////                ExceptionMessage = ex.Message,
        ////                ExceptionStackTrace = ex.StackTrace,
        ////                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        ////                LogTime = DateTime.Now
        ////            };
        ////            Logfile.CreateLogFile(Err);
        ////            Msg.Add(ex.Message);
        ////            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        ////        }
        ////    }
        ////}

        //[HttpPost]
        //public async Task<ActionResult> EditSave(Venue L, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //        try
        //        {
        //            string Corp = form["VenuTypelist"] == "0" ? "" : form["VenuTypelist"];
        //            string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
        //            string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
        //            var blog1 = db.Venue.Where(e => e.Id == data).Include(e => e.Address).Include(e => e.ContactDetails).Include(e => e.VenuType).SingleOrDefault();
        //            blog1.Address = null;
        //            blog1.ContactDetails = null;

        //            if (Addrs != null)
        //            {
        //                if (Addrs != "")
        //                {
        //                    int AddId = Convert.ToInt32(Addrs);
        //                    var val = db.Address.Include(e => e.Area)
        //                                        .Include(e => e.City)
        //                                        .Include(e => e.Country)
        //                                        .Include(e => e.District)
        //                                        .Include(e => e.State)
        //                                        .Include(e => e.StateRegion)
        //                                        .Include(e => e.Taluka)
        //                                        .Where(e => e.Id == AddId).SingleOrDefault();
        //                    L.Address = val;
        //                    blog1.Address = val;
        //                }
        //            }
        //            else
        //            {
        //                L.Address = null;
        //                blog1.Address = null;

        //            }

        //            if (ContactDetails != "")
        //            {
        //                int ContId = Convert.ToInt32(ContactDetails);
        //                var val = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => e.Id == ContId).SingleOrDefault();
        //                L.ContactDetails = val;
        //                blog1.ContactDetails = val;
        //            }
        //            else
        //            {
        //                L.ContactDetails = null;
        //                blog1.ContactDetails = null;

        //            }

        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {
        //                    // using (DataBaseContext db = new DataBaseContext())
        //                    //{
        //                    //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        if (Corp != null)
        //                        {
        //                            if (Corp != "")
        //                            {
        //                                var val = db.LookupValue.Find(int.Parse(Corp));
        //                                blog1.VenuType = val;

        //                                var type = db.Venue.Include(e => e.VenuType).Where(e => e.Id == data).SingleOrDefault();
        //                                IList<Venue> typedetails = null;
        //                                if (type.VenuType != null)
        //                                {
        //                                    typedetails = db.Venue.Where(x => x.VenuType.Id == type.VenuType.Id && x.Id == data).ToList();
        //                                }
        //                                else
        //                                {
        //                                    typedetails = db.Venue.Where(x => x.Id == data).ToList();
        //                                }
        //                                //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                                foreach (var s in typedetails)
        //                                {
        //                                    s.VenuType = blog1.VenuType;
        //                                    db.Venue.Attach(s);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                var WFTypeDetails = db.Venue.Include(e => e.VenuType).Where(x => x.Id == data).ToList();
        //                                foreach (var s in WFTypeDetails)
        //                                {
        //                                    s.VenuType = null;
        //                                    db.Venue.Attach(s);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    TempData["RowVersion"] = s.RowVersion;
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            var CreditdateypeDetails = db.Venue.Include(e => e.VenuType).Where(x => x.Id == data).ToList();
        //                            foreach (var s in CreditdateypeDetails)
        //                            {
        //                                s.VenuType = null;
        //                                db.Venue.Attach(s);
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                //await db.SaveChangesAsync();
        //                                db.SaveChanges();
        //                                TempData["RowVersion"] = s.RowVersion;
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                            }
        //                        }

        //                        var Curr_Lookup = db.Venue.Find(data);
        //                        TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
        //                        db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

        //                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        {

        //                            Venue blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.Venue.Where(e => e.Id == data).Include(e => e.Address).Include(e => e.ContactDetails).Include(e => e.VenuType).SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            blog1.DBTrack = new DBTrack
        //                   {
        //                       CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
        //                       CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
        //                       Action = "M",
        //                       ModifiedBy = SessionManager.UserName,
        //                       ModifiedOn = DateTime.Now
        //                   };
        //                            Venue post = new Venue()
        //                            {
        //                                Fees = L.Fees,
        //                                Name = L.Name,
        //                                ContactPerson = L.ContactPerson,
        //                                Narration = L.Narration,
        //                                VenuType = blog1.VenuType,
        //                                //   ContactDetails = blog1.ContactDetails,
        //                                Id = data,
        //                                DBTrack = blog1.DBTrack
        //                            };


        //                            db.Venue.Attach(post);
        //                            db.Entry(post).State = System.Data.Entity.EntityState.Modified;
        //                            db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

        //                            // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);
        //                            using (var context = new DataBaseContext())
        //                            {
        //                                var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, blog1.DBTrack);
        //                                DT_Venue DT_Corp = (DT_Venue)obj;

        //                                db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }

        //                            //var CurCorp = db.Venue.Find(data);
        //                            //TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                            //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            //{

        //                            //    Venue post = new Venue()
        //                            //    {
        //                            //        Fees = blog1.Fees,
        //                            //        Name = blog1.Name,
        //                            //        ContactPerson = blog1.ContactPerson,
        //                            //        Narration = blog1.Narration,
        //                            //        VenuType = blog1.VenuType,
        //                            //        ContactDetails = blog1.ContactDetails,
        //                            //        Id = data,
        //                            //        DBTrack = blog1.DBTrack
        //                            //    };
        //                            //    db.Venue.Attach(post);
        //                            //    db.Entry(post).State = System.Data.Entity.EntityState.Modified;

        //                            //    db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            //    db.SaveChanges();

        //                            //await db.SaveChangesAsync();
        //                            //db.Entry(post).State = System.Data.Entity.EntityState.Detached;


        //                            ts.Complete();
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

        //                        }
        //                    }
        //                    // }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (Venue)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (Venue)databaseEntry.ToObject();
        //                        blog1.RowVersion = databaseValues.RowVersion;

        //                    }
        //                }
        //                Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

        //            }
        //            return Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
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
        //}


        //[HttpPost]
        //public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        //{
        //    List<String> Msg = new List<String>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        if (auth_action == "C")
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {
        //                //Venue OBJ = db.Venue.Find(auth_id);
        //                //Venue OBJ = db.Venue.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

        //                Venue OBJ = db.Venue
        //                    //.Include(e => e.Address)
        //                    .Include(e => e.ContactDetails)
        //                    .Include(e => e.VenuType).FirstOrDefault(e => e.Id == auth_id);

        //                OBJ.DBTrack = new DBTrack
        //                {
        //                    Action = "C",
        //                    ModifiedBy = OBJ.DBTrack.ModifiedBy != null ? OBJ.DBTrack.ModifiedBy : null,
        //                    CreatedBy = OBJ.DBTrack.CreatedBy != null ? OBJ.DBTrack.CreatedBy : null,
        //                    CreatedOn = OBJ.DBTrack.CreatedOn != null ? OBJ.DBTrack.CreatedOn : null,
        //                    IsModified = OBJ.DBTrack.IsModified == true ? false : false,
        //                    AuthorizedBy = SessionManager.UserName,
        //                    AuthorizedOn = DateTime.Now
        //                };

        //                db.Venue.Attach(OBJ);
        //                db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
        //                db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                //db.SaveChanges();
        //                var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, OBJ.DBTrack);
        //                DT_Venue DT_OBJ = (DT_Venue)rtn_Obj;
        //                //DT_OBJ.Address_Id = OBJ.Address == null ? 0 : OBJ.Address.Id;
        //                DT_OBJ.VenuType_Id = OBJ.VenuType == null ? 0 : OBJ.VenuType.Id;
        //                DT_OBJ.ContactDetails_Id = OBJ.ContactDetails == null ? 0 : OBJ.ContactDetails.Id;
        //                db.Create(DT_OBJ);
        //                await db.SaveChangesAsync();

        //                ts.Complete();

        //                Msg.Add(" Record Authorised ");
        //                return Json(new Utility.JsonReturnClass { Id = OBJ.Id, Val = OBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                // return Json(new Object[] { OBJ.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //        else if (auth_action == "M")
        //        {

        //            Venue Old_OBJ = db.Venue.Include(e => e.VenuType)
        //                //.Include(e => e.Address)
        //                                              .Include(e => e.ContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();



        //            DT_Venue Curr_OBJ = db.DT_Venue
        //                                        .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
        //                                        .OrderByDescending(e => e.Id)
        //                                        .FirstOrDefault();

        //            if (Curr_OBJ != null)
        //            {
        //                Venue OBJ = new Venue();

        //                string LKVAL = Curr_OBJ.VenuType_Id == null ? null : Curr_OBJ.VenuType_Id.ToString();
        //                //string Addrs = Curr_OBJ.Address_Id == null ? null : Curr_OBJ.Address_Id.ToString();
        //                string Addrs = "";
        //                string ContactDetails = Curr_OBJ.ContactDetails_Id == null ? null : Curr_OBJ.ContactDetails_Id.ToString();
        //                OBJ.Name = Curr_OBJ.Name == null ? Old_OBJ.Name : Curr_OBJ.Name;
        //                OBJ.Fees = Curr_OBJ.Fees == null ? Old_OBJ.Fees : Curr_OBJ.Fees;
        //                //      OBJ.Id = auth_id;

        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {

        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            // db.Configuration.AutoDetectChangesEnabled = false;
        //                            OBJ.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = Old_OBJ.DBTrack.CreatedBy == null ? null : Old_OBJ.DBTrack.CreatedBy,
        //                                CreatedOn = Old_OBJ.DBTrack.CreatedOn == null ? null : Old_OBJ.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = Old_OBJ.DBTrack.ModifiedBy == null ? null : Old_OBJ.DBTrack.ModifiedBy,
        //                                ModifiedOn = Old_OBJ.DBTrack.ModifiedOn == null ? null : Old_OBJ.DBTrack.ModifiedOn,
        //                                AuthorizedBy = SessionManager.UserName,
        //                                AuthorizedOn = DateTime.Now,
        //                                IsModified = false
        //                            };

        //                            int a = EditS(LKVAL, ContactDetails, auth_id, OBJ, OBJ.DBTrack);


        //                            await db.SaveChangesAsync();

        //                            ts.Complete();
        //                            Msg.Add(" Record Authorised ");
        //                            return Json(new Utility.JsonReturnClass { Id = OBJ.Id, Val = OBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        }
        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (Venue)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (Venue)databaseEntry.ToObject();
        //                            OBJ.RowVersion = databaseValues.RowVersion;
        //                        }
        //                    }
        //                    Msg.Add(" Record modified by another user.So refresh it and try to save again. ");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                }
        //            }
        //            else
        //                Msg.Add(" Data removed from history ");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
        //        }
        //        else if (auth_action == "D")
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {
        //                //Venue OBJ = db.Venue.Find(auth_id);
        //                Venue OBJ = db.Venue.AsNoTracking().Include(e => e.VenuType)
        //                                                            .Include(e => e.ContactDetails).FirstOrDefault(e => e.Id == auth_id);


        //                ContactDetails conDet = OBJ.ContactDetails;
        //                LookupValue val = OBJ.VenuType;

        //                OBJ.DBTrack = new DBTrack
        //                {
        //                    Action = "D",
        //                    ModifiedBy = OBJ.DBTrack.ModifiedBy != null ? OBJ.DBTrack.ModifiedBy : null,
        //                    CreatedBy = OBJ.DBTrack.CreatedBy != null ? OBJ.DBTrack.CreatedBy : null,
        //                    CreatedOn = OBJ.DBTrack.CreatedOn != null ? OBJ.DBTrack.CreatedOn : null,
        //                    IsModified = false,
        //                    AuthorizedBy = SessionManager.UserName,
        //                    AuthorizedOn = DateTime.Now
        //                };

        //                db.Venue.Attach(OBJ);
        //                db.Entry(OBJ).State = System.Data.Entity.EntityState.Deleted;


        //                var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, OBJ.DBTrack);
        //                DT_Venue DT_OBJ = (DT_Venue)rtn_Obj;
        //                DT_OBJ.VenuType_Id = OBJ.VenuType == null ? 0 : OBJ.VenuType.Id;
        //                DT_OBJ.ContactDetails_Id = OBJ.ContactDetails == null ? 0 : OBJ.ContactDetails.Id;
        //                db.Create(DT_OBJ);
        //                await db.SaveChangesAsync();
        //                db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
        //                ts.Complete();
        //                Msg.Add(" Record Authorised. ");
        //                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
        //            }

        //        }
        //        return View();

        //    }
        //}

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

    }




}
