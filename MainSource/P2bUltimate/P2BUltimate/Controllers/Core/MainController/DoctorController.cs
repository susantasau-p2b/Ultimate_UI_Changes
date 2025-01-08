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
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class DoctorController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(Doctor c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["DoctorCategoryList_DDL"] == "0" ? "" : form["DoctorCategoryList_DDL"];
                    string Splzlist = form["SpecializationList_DDL"] == "0" ? "" : form["SpecializationList_DDL"];
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    //string Namel = "6";
                    string Name1 = form["NameList"] == "0" ? "" : form["NameList"];

                    if (Splzlist != null && Splzlist != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "301").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Splzlist)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(Splzlist));
                        c.Specialization = val;
                    }


                    if (Category != null && Category != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "302").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(Category));
                        c.DoctorCategory = val;
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
                        }
                    }

                    if (ContactDetails != null && ContactDetails != "")
                    {
                        int ContId = Convert.ToInt32(ContactDetails);
                        var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                            .Where(e => e.Id == ContId).SingleOrDefault();
                        c.ContactDetails = val;
                    }

                    if (Name1 != null && Name1 != "")
                    {
                        int AddId = Convert.ToInt32(Name1);
                        var val = db.NameSingle.Include(e => e.EmpTitle)
                                            .Where(e => e.Id == AddId).SingleOrDefault();
                        c.Name = val;
                    }







                    //if (ModelState.IsValid)
                    //{
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //if (db.Doctor.Any(o => o.Name.FullNameFML == c.Name.FullNameFML))
                        //{
                        //    return Json(new Object[] { "", "", "Name Already Exists.", JsonRequestBehavior.AllowGet });
                        //}

                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        Doctor Doctor = new Doctor()
                        {

                            Name = c.Name,
                            Specialization = c.Specialization,
                            Address = c.Address,
                            ContactDetails = c.ContactDetails,
                            MeetingTime = c.MeetingTime,
                            Qualification = c.Qualification,
                            DoctorCategory = c.DoctorCategory,
                            DBTrack = c.DBTrack
                        };
                        try
                        {
                            db.Doctor.Add(Doctor);
                            db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", Doctor, null, "Doctor", null);
                            ////   DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);

                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = Doctor.Id, Val = Doctor.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { Disease.Id, Disease.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            // return Json(new Object[] { Doctor.Id, Doctor.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
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
                    //}
                    //else
                    //{
                    //    StringBuilder sb = new StringBuilder("");
                    //    foreach (ModelState modelState in ModelState.Values)
                    //    {
                    //        foreach (ModelError error in modelState.Errors)
                    //        {
                    //            sb.Append(error.ErrorMessage);
                    //            sb.Append("." + "\n");
                    //        }
                    //    }
                    //    var errorMsg = sb.ToString();
                    //    return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                    //    //return this.Json(new { msg = errorMsg });
                    //}

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



        //public ActionResult EditAddress_partial(int data)
        //{
        //    var r = (from ca in db.Address
        //             .Where(e => e.Id == data)
        //             select new
        //             {
        //                 Id = ca.Id,
        //                 EmailId = ca.,
        //                 FaxNo = ca.FaxNo,
        //                 Website = ca.Website
        //             }).ToList();

        //    var a = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => e.Id == data).SingleOrDefault();
        //    var b = a.ContactNumbers;

        //    var r1 = (from s in b
        //              select new
        //              {
        //                  Id = s.Id,
        //                  FullContactNumbers = s.FullContactNumbers
        //              }).ToList();

        //    TempData["RowVersion"] = db.ContactDetails.Find(data).RowVersion;
        //    return Json(new object[] { r, r1 }, JsonRequestBehavior.AllowGet);
        //}



        //public ActionResult Editcontactdetails_partial(int data)
        //{
        //    var r = (from ca in db.ContactDetails
        //             .Where(e => e.Id == data)
        //             select new
        //             {
        //                 Id = ca.Id,
        //                 EmailId = ca.EmailId,
        //                 FaxNo = ca.FaxNo,
        //                 Website = ca.Website
        //             }).ToList();

        //    var a = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => e.Id == data).SingleOrDefault();
        //    var b = a.ContactNumbers;

        //    var r1 = (from s in b
        //              select new
        //              {
        //                  Id = s.Id,
        //                  FullContactNumbers = s.FullContactNumbers
        //              }).ToList();

        //    TempData["RowVersion"] = db.ContactDetails.Find(data).RowVersion;
        //    return Json(new object[] { r, r1 }, JsonRequestBehavior.AllowGet);
        //}


        public ActionResult Createcontactdetails_partial()
        {
            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        }
        public ActionResult Name_partial()
        {
            return View("~/Views/Shared/Core/_Namesingle.cshtml");
        }
        public ActionResult Address_partial()
        {
            return View("~/Views/Shared/Core/_Address.cshtml");
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "Doctor";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Doctor
                        .Include(e => e.Address)
                        .Include(e => e.Name)
                        .Include(e => e.ContactDetails)
                        .Include(e => e.Specialization)
                        .Include(e => e.DoctorCategory)
                        .Where(e => e.Id == data).Select
                        (e => new
                        {

                            Name = e.Name,
                            Qualification = e.Qualification == null ? "" : e.Qualification,
                            MeetingTime = e.MeetingTime == null ? "" : e.MeetingTime,
                            Specialization_Id = e.Specialization.Id == null ? 0 : e.Specialization.Id,
                            DoctorCategory_Id = e.DoctorCategory.Id == null ? 0 : e.DoctorCategory.Id,
                            Action = e.DBTrack.Action
                        }).ToList();

                var add_data = db.Doctor
                        .Include(e => e.Address)
                        .Include(e => e.Name)
                        .Include(e => e.ContactDetails)
                        .Include(e => e.Address.Area)
                        .Include(e => e.Address.City)
                        .Include(e => e.Address.Country)
                        .Include(e => e.Address.District)
                         .Include(e => e.Address.State)
                        .Include(e => e.Address.StateRegion)
                        .Include(e => e.Address.Taluka)
                        .Include(e => e.Specialization)
                        .Include(e => e.DoctorCategory)
                        .Where(e => e.Id == data)
                        .Select(e => new
                        {
                            Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                            Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
                            Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                            FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
                            Name_FullName = e.Name.FullDetails == null ? "" : e.Name.FullDetails,
                            Name_Id = e.Name.Id == null ? "" : e.Name.Id.ToString(),
                        }).ToList();

                var W = "";
                //var W = db.DT_Corporate
                //        .Include(e => e.Address)
                //        .Include(e => e.Name)
                //        .Include(e => e.ContactDetails)
                //        .Include(e => e.Specialization)
                //        .Include(e => e.DoctorCategory)
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         Code = e.Code == null ? "" : e.Code,
                //         Name = e.Name == null ? "" : e.Name,
                //         BusinessType_Val = e.Specialization.Id == null ? "" : e.Specialization.LookupVal,
                //         Address_Val = e.Address.Id == null ? "" : e.Address.FullAddress,
                //         Contact_Val = e.ContactDetails.Id == null ? "" : e.ContactDetails.FullContactDetails,
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.Doctor.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(Doctor c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Corp = form["SpecializationList_DDL"] == "0" ? "" : form["SpecializationList_DDL"];
                    string Dcategory = form["DoctorCategoryList_DDL"] == "0" ? "" : form["DoctorCategoryList_DDL"];
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    string NameList = form["NameList"] == "0" ? "" : form["NameList"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (Corp != null)
                    {
                        if (Corp != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "301").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Corp)).FirstOrDefault();//db.LookupValue.Find(int.Parse(Corp));
                            c.Specialization = val;
                        }
                    }
                    if (Dcategory != null)
                    {
                        if (Dcategory != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "302").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Dcategory)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(Dcategory));
                            c.DoctorCategory = val;
                        }
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
                        }
                    }

                    if (NameList != null)
                    {
                        if (NameList != "")
                        {
                            int NameId = Convert.ToInt32(NameList);
                            var val = db.NameSingle.Include(e => e.EmpTitle)
                                                .Where(e => e.Id == NameId).SingleOrDefault();
                            c.Name = val;
                        }
                    }

                    //if (Auth == false)
                    //{
                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                Doctor blog = null; // to retrieve old data
                                // DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.Doctor.Where(e => e.Id == data).Include(e => e.Specialization)
                                                            .Include(e => e.DoctorCategory)
                                                             .Include(e => e.Name)
                                                            .Include(e => e.Address)
                                                            .Include(e => e.ContactDetails).AsNoTracking().SingleOrDefault();
                                }

                                c.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };

                               // int a = EditS(Corp, Dcategory, Addrs, ContactDetails, NameList, data, c, c.DBTrack);
                                if (Corp != null)
                                {
                                    if (Corp != "")
                                    {
                                        var val = db.LookupValue.Find(int.Parse(Corp));
                                        c.Specialization = val;

                                        var type = db.Doctor.Include(e => e.Specialization).Where(e => e.Id == data).SingleOrDefault();
                                        IList<Doctor> typedetails = null;
                                        if (type.Specialization != null)
                                        {
                                            typedetails = db.Doctor.Where(x => x.Specialization.Id == type.Specialization.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.Doctor.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.Specialization = c.Specialization;
                                            db.Doctor.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                }


                                if (Dcategory != null)
                                {
                                    if (Dcategory != "")
                                    {
                                        var val = db.LookupValue.Find(int.Parse(Dcategory));
                                        c.Specialization = val;

                                        var type = db.Doctor.Include(e => e.DoctorCategory).Where(e => e.Id == data).SingleOrDefault();
                                        IList<Doctor> typedetails = null;
                                        if (type.DoctorCategory != null)
                                        {
                                            typedetails = db.Doctor.Where(x => x.DoctorCategory.Id == type.DoctorCategory.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.Doctor.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.DoctorCategory = c.DoctorCategory;
                                            db.Doctor.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                }


                                if (Addrs != null)
                                {
                                    if (Addrs != "")
                                    {
                                        var val = db.Address.Find(int.Parse(Addrs));
                                        c.Address = val;

                                        var add = db.Doctor.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                                        IList<Doctor> addressdetails = null;
                                        if (add.Address != null)
                                        {
                                            addressdetails = db.Doctor.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            addressdetails = db.Doctor.Where(x => x.Id == data).ToList();
                                        }
                                        if (addressdetails != null)
                                        {
                                            foreach (var s in addressdetails)
                                            {
                                                s.Address = c.Address;
                                                db.Doctor.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                // await db.SaveChangesAsync(false);
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var addressdetails = db.Doctor.Include(e => e.Address).Where(x => x.Id == data).ToList();
                                    foreach (var s in addressdetails)
                                    {
                                        s.Address = null;
                                        db.Doctor.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }

                                if (ContactDetails != null)
                                {
                                    if (ContactDetails != "")
                                    {
                                        var val = db.ContactDetails.Find(int.Parse(ContactDetails));
                                        c.ContactDetails = val;

                                        var add = db.Doctor.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                                        IList<Doctor> contactsdetails = null;
                                        if (add.ContactDetails != null)
                                        {
                                            contactsdetails = db.Doctor.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            contactsdetails = db.Doctor.Where(x => x.Id == data).ToList();
                                        }
                                        foreach (var s in contactsdetails)
                                        {
                                            s.ContactDetails = c.ContactDetails;
                                            db.Doctor.Attach(s);
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
                                    var contactsdetails = db.Doctor.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                                    foreach (var s in contactsdetails)
                                    {
                                        s.ContactDetails = null;
                                        db.Doctor.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        //  TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }

                                if (NameList != null)
                                {
                                    if (NameList != "")
                                    {
                                        var val = db.NameSingle.Find(int.Parse(NameList));
                                        c.Name = val;

                                        var add = db.Doctor.Include(e => e.Name).Where(e => e.Id == data).SingleOrDefault();
                                        IList<Doctor> addressdetails = null;
                                        if (add.Name != null)
                                        {
                                            addressdetails = db.Doctor.Where(x => x.Name.Id == add.Name.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            addressdetails = db.Doctor.Where(x => x.Id == data).ToList();
                                        }
                                        if (addressdetails != null)
                                        {
                                            foreach (var s in addressdetails)
                                            {
                                                s.Name = c.Name;
                                                db.Doctor.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                // await db.SaveChangesAsync(false);
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var addressdetails = db.Doctor.Include(e => e.Name).Where(x => x.Id == data).ToList();
                                    foreach (var s in addressdetails)
                                    {
                                        s.Name = null;
                                        db.Doctor.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }

                                var CurCorp = db.Doctor.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    Doctor corp = new Doctor()
                                    {

                                        Name = c.Name,
                                        DoctorCategory = c.DoctorCategory,
                                        MeetingTime = c.MeetingTime,
                                        Qualification = c.Qualification,
                                        Specialization = c.Specialization,
                                        Id = data,
                                        DBTrack = c.DBTrack
                                    };

                                    db.Doctor.Attach(corp);
                                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                }
                                await db.SaveChangesAsync();

                                using (var context = new DataBaseContext())
                                {
                                   //var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "Doctor", c.DBTrack);
                                    //  DT_Doctor DT_Corp = (DT_Doctor)Obj;
                                    // db.DT_Doctor.Add(DT_Corp);
                                   // db.SaveChanges();
                                }
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (Doctor)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            else
                            {
                                var databaseValues = (Doctor)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //   return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }
                    //}
                    //else
                    //{
                    //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //    {
                    //        Doctor Old_Corp = db.Doctor.Include(e => e.Specialization)
                    //                                            .Include(e => e.Address)
                    //                                            .Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();

                    //        Doctor Curr_Corp = c;
                    //        c.DBTrack = new DBTrack
                    //        {
                    //            CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                    //            CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                    //            Action = "M",
                    //            IsModified = Old_Corp.DBTrack.IsModified == true ? true : false,
                    //            //ModifiedBy = SessionManager.UserName,
                    //            //ModifiedOn = DateTime.Now
                    //        };
                    //        Old_Corp.DBTrack = c.DBTrack;

                    //        db.Entry(Old_Corp).State = System.Data.Entity.EntityState.Modified;
                    //        db.SaveChanges();
                    //        using (var context = new DataBaseContext())
                    //        {
                    //             DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Old_Corp, Curr_Corp, "Doctor", c.DBTrack);
                    //        }

                    //        ts.Complete();
                    //        return Json(new Object[] { Old_Corp.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
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
                var Doctor = db.Doctor.ToList();
                IEnumerable<Doctor> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Doctor;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), Convert.ToString(a.MeetingTime), a.Qualification.ToString() }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.MeetingTime, a.Qualification }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Doctor;
                    Func<Doctor, string> orderfuc = (c =>
                                                               gp.sidx == "ID" ? c.Id.ToString() :
                                                               gp.sidx == "Name" ? c.Name.FullNameFMLWT :
                                                               gp.sidx == "MeetingTime" ? c.MeetingTime :
                                                               gp.sidx == "Qualification" ? c.Qualification :
                                                                 "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), Convert.ToString(a.MeetingTime), a.Qualification }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Name), a.MeetingTime, a.Qualification }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Name, a.MeetingTime, a.Qualification }).ToList();
                    }
                    totalRecords = Doctor.Count();
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



        //public ActionResult P2BGrid1(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<Doctor> Doctor = null;
        //        if (gp.IsAutho == true)
        //        {
        //            Doctor = db.Doctor.Include(e => e.Specialization).Where(e => e.DBTrack.IsModified == true).ToList();
        //        }
        //        else
        //        {
        //            Doctor = db.Doctor.Include(e => e.Specialization).ToList();
        //        }

        //        IEnumerable<Doctor> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = Doctor;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToLower() == gp.searchString.ToLower()) || (e.Name.ToLower() == gp.searchString.ToLower()))).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.Specialization) != null ? Convert.ToString(a.Specialization.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.Specialization != null ? Convert.ToString(a.Specialization.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = Doctor;
        //            Func<Doctor, int> orderfuc = (c =>
        //                                                       gp.sidx == "Id" ? c.Id : 0);
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), a.Specialization != null ? Convert.ToString(a.Specialization.LookupVal) : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), a.Specialization != null ? Convert.ToString(a.Specialization.LookupVal) : "" }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.Specialization != null ? Convert.ToString(a.Specialization.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = Doctor.Count();
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {

            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    Doctor corporates = db.Doctor.Include(e => e.Address)
                                                       .Include(e => e.ContactDetails)
                                                       .Include(e => e.Specialization).Where(e => e.Id == data).SingleOrDefault();

                    Address add = corporates.Address;
                    ContactDetails conDet = corporates.ContactDetails;
                    LookupValue val = corporates.Specialization;
                    NameSingle NameList = corporates.Name;
                    //Doctor corporates = db.Doctor.Where(e => e.Id == data).SingleOrDefault();
                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            // DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Doctor");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            corporates.DBTrack = dbT;
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Doctor", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Doctor", corporates.DBTrack);
                            }
                            ts.Complete();
                            //    return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        //var selectedRegions = corporates.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(corporates.Regions.Select(e => e.Id));
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
                                    CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                    CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                    IsModified = corporates.DBTrack.IsModified == true ? false : false,
                                    AuthorizedBy = SessionManager.UserName,
                                    AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                                await db.SaveChangesAsync();


                                using (var context = new DataBaseContext())
                                {
                                    corporates.Address = add;
                                    corporates.ContactDetails = conDet;
                                    corporates.Specialization = val;
                                    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Doctor", dbT);
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
                                // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
        public ActionResult GetAddressLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                                     .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
                                     .Include(e => e.Taluka).ToList();
                IEnumerable<Address> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Address.ToList().Where(d => d.FullAddress.Contains(data));

                }
                else
                {
                    ///var list1 = db.Lookup.ToList().SelectMany(e => e.LookupValues);
                    //var list1 = db.Doctor.Include(e => e.ContactDetails.ContactNumbers).ToList().SelectMany(e => e.ContactDetails.ContactNumbers);
                    var list1 = db.Doctor.Include(e => e.Address.Area).ToList().Select(e => e.Address);
                    var list2 = fall.Except(list1);

                    //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                    var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullAddress }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }

        public ActionResult GetContactDetLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactDetails.Include(e => e.ContactNumbers)
                                     .ToList();
                IEnumerable<ContactDetails> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ContactDetails.ToList().Where(d => d.FullContactDetails.Contains(data));

                }
                else
                {
                    var list1 = db.ContactDetails.ToList();
                    //var list1 = db.Doctor.Include(e => e.ContactDetails).ToList();
                    //var list2 = fall.Except(list1);
                    var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }

        public ActionResult GetNameDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.NameSingle.Include(e => e.EmpTitle)
                                     .ToList();
                IEnumerable<NameSingle> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.NameSingle.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    var list1 = db.NameSingle.ToList();
                    //var list1 = db.Doctor.Include(e => e.ContactDetails).ToList();
                    //var list2 = fall.Except(list1);
                    var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }
        //public ActionResult GetContactDetLKDetails1(string data)
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
        //            var list1 = db.Doctor.Include(e => e.ContactDetails.ContactNumbers).ToList().Select(e => e.ContactDetails);

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



        public int EditS(string Corp, string Dcategory, string Addrs, string ContactDetails, string Name, int data, Doctor c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        c.Specialization = val;

                        var type = db.Doctor.Include(e => e.Specialization).Where(e => e.Id == data).SingleOrDefault();
                        IList<Doctor> typedetails = null;
                        if (type.Specialization != null)
                        {
                            typedetails = db.Doctor.Where(x => x.Specialization.Id == type.Specialization.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Doctor.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.Specialization = c.Specialization;
                            db.Doctor.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }


                if (Dcategory != null)
                {
                    if (Dcategory != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Dcategory));
                        c.Specialization = val;

                        var type = db.Doctor.Include(e => e.DoctorCategory).Where(e => e.Id == data).SingleOrDefault();
                        IList<Doctor> typedetails = null;
                        if (type.DoctorCategory != null)
                        {
                            typedetails = db.Doctor.Where(x => x.DoctorCategory.Id == type.DoctorCategory.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Doctor.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.DoctorCategory = c.DoctorCategory;
                            db.Doctor.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }


                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.Address.Find(int.Parse(Addrs));
                        c.Address = val;

                        var add = db.Doctor.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                        IList<Doctor> addressdetails = null;
                        if (add.Address != null)
                        {
                            addressdetails = db.Doctor.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.Doctor.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Address = c.Address;
                                db.Doctor.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                // await db.SaveChangesAsync(false);
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var addressdetails = db.Doctor.Include(e => e.Address).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Address = null;
                        db.Doctor.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (ContactDetails != null)
                {
                    if (ContactDetails != "")
                    {
                        var val = db.ContactDetails.Find(int.Parse(ContactDetails));
                        c.ContactDetails = val;

                        var add = db.Doctor.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<Doctor> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.Doctor.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.Doctor.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = c.ContactDetails;
                            db.Doctor.Attach(s);
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
                    var contactsdetails = db.Doctor.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.Doctor.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        //  TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }







                //if (Name != null)
                //{
                //    if (Name != "")
                //    {
                //        var val = db.NameSingle.Find(int.Parse(Name));
                //        c.Name = val;

                //        var add = db.Doctor.Include(e => e.Name).Where(e => e.Id == data).SingleOrDefault();
                //        IList<Doctor> addressdetails = null;
                //        if (add.Name != null)
                //        {
                //            addressdetails = db.Doctor.Where(x => x.Name.Id == add.Name.Id && x.Id == data).ToList();
                //        }
                //        else
                //        {
                //            addressdetails = db.Doctor.Where(x => x.Id == data).ToList();
                //        }
                //        if (addressdetails != null)
                //        {
                //            foreach (var s in addressdetails)
                //            {
                //                s.Name = c.Name;
                //                db.Doctor.Attach(s);
                //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //                // await db.SaveChangesAsync(false);
                //                db.SaveChanges();
                //                TempData["RowVersion"] = s.RowVersion;
                //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    var addressdetails = db.Doctor.Include(e => e.Name).Where(x => x.Id == data).ToList();
                //    foreach (var s in addressdetails)
                //    {
                //        s.Name = null;
                //        db.Doctor.Attach(s);
                //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //        //await db.SaveChangesAsync();
                //        db.SaveChanges();
                //        TempData["RowVersion"] = s.RowVersion;
                //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //    }
                //}




                //
                if (Name != null)
                {
                    if (Name != "")
                    {
                        var val = db.NameSingle.Find(int.Parse(Name));
                        c.Name = val;

                        var add = db.Doctor.Include(e => e.Name).Where(e => e.Id == data).SingleOrDefault();
                        IList<Doctor> addressdetails = null;
                        if (add.Name != null)
                        {
                            addressdetails = db.Doctor.Where(x => x.Name.Id == add.Name.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.Doctor.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Name = c.Name;
                                db.Doctor.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                // await db.SaveChangesAsync(false);
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var addressdetails = db.Doctor.Include(e => e.Name).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Name = null;
                        db.Doctor.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                //









                var CurCorp = db.Doctor.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Doctor corp = new Doctor()
                    {

                        Name = c.Name,
                        DoctorCategory = c.DoctorCategory,
                        MeetingTime = c.MeetingTime,
                        Qualification = c.Qualification,
                        Specialization = c.Specialization,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.Doctor.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    ////  DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }

            //[HttpPost]
            ////[ValidateAntiForgeryToken]
            //public ActionResult DeleteContactDetails(int? data, int? forwarddata)
            //{
            //    ContactDetails contDet = db.ContactDetails.Find(data);
            //    Doctor corp = db.Doctor.Find(forwarddata);
            //    try
            //    {
            //        using (TransactionScope ts = new TransactionScope())
            //        {
            //            corp.ContactDetails = null;
            //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
            //            db.SaveChanges();
            //            ts.Complete();
            //        }
            //        //return Json(new Object[] { "", "", "Data removed.", JsonRequestBehavior.AllowGet });
            //        return Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
            //    }

            //    catch (DataException /* dex */)
            //    {
            //        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
            //        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
            //        //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
            //        //return RedirectToAction("Index");
            //    }
            //}


            //[HttpPost]
            ////[ValidateAntiForgeryToken]
            //public ActionResult DeleteAddress(int? data, int? forwarddata)
            //{
            //    Address addrs = db.Address.Find(data);
            //    Doctor corp = db.Doctor.Find(forwarddata);
            //    try
            //    {
            //        using (TransactionScope ts = new TransactionScope())
            //        {
            //            corp.Address = null;
            //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
            //            db.SaveChanges();
            //            ts.Complete();
            //        }
            //        return Json(new Object[] { "", "", "Data removed.", JsonRequestBehavior.AllowGet });
            //       // return this.Json(new { msg = "Data deleted." });
            //    }

            //    catch (DataException /* dex */)
            //    {
            //        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
            //        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
            //        //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
            //        //return RedirectToAction("Index");
            //    }
            //}

            //public 



        }

    }


}
