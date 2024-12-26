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
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Training.MainController
{
    public class FacultyInternalController : Controller
    {
        //
        // GET: /FacultyInternal/
        List<string> Msg = new List<string>();
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/FacultyInternal/Index.cshtml");
        }


        public ActionResult partial()
        {
            return View("~/Views/Shared/Training/_FacultyInternal.cshtml");
        }

        //private DataBaseContext db = new DataBaseContext();

        [HttpPost]
        public ActionResult Create(FacultyInternal c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    string Category = form["Category"] == "0" ? "" : form["Category"];
                    string Addrs = form["Address_List"] == "0" ? "" : form["Address_List"];
                    string ContactDetails = form["Contact_List"] == "0" ? "" : form["Contact_List"];
                    string employee = form["employee-table"] == "0" ? null : form["employee-table"];
                    string Specilisation = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];

                    int empId;
                    if (employee != null && employee != "")
                    {
                        empId = Convert.ToInt32(employee);
                        var vals = db.Employee.Where(e => e.Id == empId).SingleOrDefault();
                        c.EmpID = vals;
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    var empval = db.FacultyInternal.Include(e => e.EmpID).Any(q => q.EmpID.Id == empId);
                    if (empval == true)
                    {
                        Msg.Add("  Record Already Exists For This Employee.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Category != null && Category != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Category));
                    }

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

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            FacultyInternal FacultyInternal = new FacultyInternal()
                            {
                                Code = c.Code == null ? "" : c.Code,
                                Name = c.Name == null ? "" : c.Name,
                                //  FacultyType = c.FacultyType,
                                EmpID = c.EmpID,
                                Address = c.Address,
                                FacultySpecialization = c.FacultySpecialization,
                                ContactDetails = c.ContactDetails,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.FacultyInternal.Add(FacultyInternal);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, c.DBTrack);
                                DT_FacultyInternal DT_Corp = (DT_FacultyInternal)rtn_Obj;
                                DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                                //   DT_Corp.FacultyType_Id = c.FacultyType == null ? 0 : c.FacultyType.Id;
                                DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                db.Create(DT_Corp);
                                db.SaveChanges();
                                ts.Complete();

                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = FacultyInternal.Id, Val = FacultyInternal.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                        return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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

        /* -------------------------- Employee ----------------------*/
        public ActionResult GetLookupEmployeeDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Employee.ToList();
                IEnumerable<Employee> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Employee.ToList().Where(d => d.EmpCode.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.EmpCode }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.EmpCode }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
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

        public ActionResult Createcontactdetails_partial()
        {
            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        }
        public class FacInternal_Val
        {
            public String Address_FullAddress { get; set; }
            public String Add_Id { get; set; }
            public String Cont_Id { get; set; }
            public String FullContactDetails { get; set; }
        }
        //[HttpPost]
        //public ActionResult Edit(int data)
        //{
        //    //string tableName = "EmpMedicalInfo";

        //    //    // Fetch the table records dynamically
        //    //    var tableData = db.GetType()
        //    //    .GetProperty(tableName)
        //    //    .GetValue(db, null);
        //    try
        //    {
        //        List<FacInternal_Val> return_data = new List<FacInternal_Val>();
        //        var Fac = db.FacultyInternal.Include(e => e.Address).Include(e => e.ContactDetails).Include(e => e.EmpID)
        //            .Where(q => q.Id == data).Select
        //            (e => new
        //            {
        //                Code = e.Code == null ? "" : e.Code,
        //                Name = e.Name == null ? "" : e.Name,
        //            }
        //            ).ToList();

        //        //var Q = db.FacultyInternal.Include(e => e.Address).Include(e => e.ContactDetails).Include(e => e.EmpID).Include(e => e.EmpID.EmpName)
        //        //    .Where(e => e.Id == data).Select
        //        //    (e => new
        //        //    {
        //        //        Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
        //        //        Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
        //        //        Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
        //        //        FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,

        //        //        Action = e.DBTrack.Action
        //        //    }).ToList();

        //        var add_data1 = db.FacultyInternal.Include(e => e.Address).Include(e => e.ContactDetails).Include(e => e.EmpID).Include(e => e.EmpID.EmpName)
        //           .Where(e => e.Id == data).SingleOrDefault();

        //        var add_data = db.FacultyInternal.Include(e => e.Address).Include(e => e.ContactDetails).Include(e => e.EmpID).Include(e => e.EmpID.EmpName)
        //            .Where(e => e.Id == data).ToList();
        //        foreach (var e in add_data)
        //        {
        //            return_data.Add(
        //            new FacInternal_Val
        //            {
        //                Address_FullAddress = e.Address == null ? "" : e.Address.FullAddress,
        //                Add_Id = e.Address == null ? "" : e.Address.Id.ToString(),
        //                Cont_Id = e.ContactDetails == null ? "" : e.ContactDetails.Id.ToString(),
        //                FullContactDetails = e.ContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
        //            });
        //        }


        //        var Corp = db.EmpMedicalInfo.Find(add_data1.Id);
        //        TempData["RowVersion"] = Corp.RowVersion;
        //        var Auth = Corp.DBTrack.IsModified;
        //        return Json(new Object[] { Fac, return_data, "", Auth, JsonRequestBehavior.AllowGet });
        //    }
        //    catch (Exception ex)
        //    {
        //        LogFile Logfile = new LogFile();
        //        ErrorLog Err = new ErrorLog()
        //        {
        //            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //            ExceptionMessage = ex.Message,
        //            ExceptionStackTrace = ex.StackTrace,
        //            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //            LogTime = DateTime.Now
        //        };
        //        Logfile.CreateLogFile(Err);
        //        Msg.Add(ex.Message);
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public class returnLookupClass
        {
            public Array fac_id { get; set; }
            public Array fac_details { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.FacultyInternal.Where(e => e.Id == data).Select
                    (e => new
                    {
                        Code = e.Code,
                        Name = e.Name,
                        Action = e.DBTrack.Action
                    }).ToList();

                List<returnLookupClass> faculty = new List<returnLookupClass>();
                var k = db.FacultyInternal.Include(e => e.FacultySpecialization).Where(b => b.Id == data).ToList();
                foreach (var v in k)
                {
                    faculty.Add(new returnLookupClass
                    {
                        fac_id = v.FacultySpecialization.Select(e => e.Id.ToString()).ToArray(),
                        fac_details = v.FacultySpecialization.Select(e => e.FullDetails).ToArray(),
                    });
                }

                var add_data = db.FacultyInternal.Include(e => e.Address).Include(e => e.ContactDetails).Include(e => e.FacultySpecialization).Include(e => e.EmpID).Include(e => e.EmpID.EmpName)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                        Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
                        Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                        FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails
                    }).ToList();

                var Corp = db.FacultyInternal.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, faculty, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //       [HttpPost]
        //public async Task<ActionResult> EditSave(FacultyInternal L, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //        try
        //        {
        //            string Category = form["Category"] == "0" ? "" : form["Category"];
        //            string Addrs = form["Address_List"] == "0" ? "" : form["Address_List"];
        //            string ContactDetails = form["Contact_List"] == "0" ? "" : form["Contact_List"];
        //            string employee = form["Employee_List"] == "0" ? "" : form["Employee_List"];
        //            string Specilisation = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
        //            var blog1 =   db.FacultyInternal.Include(e => e.FacultySpecialization)
        //          .Include(e => e.Address )
        //            .Include(e => e.ContactDetails ) 
        //            .Where(e => e.Id == data)
        //           .SingleOrDefault();

        //            blog1.Address = null;
        //            blog1.ContactDetails = null;
        //            blog1.FacultySpecialization = null;

        //            List<FacultySpecialization> ObjITsection = new List<FacultySpecialization>();
        //            if (Specilisation != null && Specilisation != "")
        //            {
        //                var ids = Utility.StringIdsToListIds(Specilisation);
        //                foreach (var ca in ids)
        //                {
        //                    var value = db.FacultySpecialization.Find(ca);
        //                    ObjITsection.Add(value);
        //                    L.FacultySpecialization = ObjITsection;

        //                }
        //            }
        //            else
        //            {
        //                L.FacultySpecialization = null;
        //            }

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
        //                    blog1.Address = L.Address;
        //                }
        //            }

        //            if (ContactDetails != null)
        //            {
        //                if (ContactDetails != "")
        //                {
        //                    int ContId = Convert.ToInt32(ContactDetails);
        //                    var val = db.ContactDetails.Include(e => e.ContactNumbers)
        //                                        .Where(e => e.Id == ContId).SingleOrDefault();
        //                   L.ContactDetails = val;
        //                    blog1.ContactDetails = L.ContactDetails;
        //                }
        //            }
        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {
        //                    {
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {

        //                            L.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
        //                                CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            var CurCorp = db.FacultyInternal.Find(data);
        //                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {
        //                                FacultyInternal FacultyInternal = new FacultyInternal()
        //                                {
        //                                    Id = data,
        //                                    Code = L.Code == null ? "" : L.Code,
        //                                    Name = L.Name == null ? "" : L.Name,
        //                                    //  FacultyType = L.FacultyType,
        //                                    EmpID = L.EmpID,
        //                                    Address = L.Address,
        //                                    FacultySpecialization = L.FacultySpecialization,
        //                                    ContactDetails = L.ContactDetails,
        //                                    DBTrack = L.DBTrack
        //                                };
        //                                db.FacultyInternal.Attach(FacultyInternal);
        //                                db.Entry(FacultyInternal).State = System.Data.Entity.EntityState.Modified;

        //                                db.Entry(FacultyInternal).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                                db.SaveChanges();

        //                               // await db.SaveChangesAsync();
        //                                db.Entry(FacultyInternal).State = System.Data.Entity.EntityState.Detached;
        //                                ts.Complete();
        //                                Msg.Add("  Record Updated");
        //                                return Json(new Utility.JsonReturnClass { Id = blog1.Id, Val = blog1.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

        //                            }
        //                        }
        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (FacultyInternal)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (FacultyInternal)databaseEntry.ToObject();
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
        //public async Task<ActionResult> EditSave(FacultyInternal c, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    try
        //    {
        //        var db_data = db.FacultyInternal.Include(e => e.FacultySpecialization)
        //          .Include(e => e.Address)
        //            .Include(e => e.ContactDetails)
        //            .Where(e => e.Id == data)
        //           .SingleOrDefault();

        //        string Category = form["Category"] == "0" ? "" : form["Category"];
        //        string Addrs = form["Address_List"] == "0" ? "" : form["Address_List"];
        //        string ContactDetails = form["Contact_List"] == "0" ? "" : form["Contact_List"];
        //        string employee = form["Employee_List"] == "0" ? "" : form["Employee_List"];
        //        string Specilisation = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
        //        bool Auth = form["autho_allow"] == "true" ? true : false;

        //        List<FacultySpecialization> ObjITsection = new List<FacultySpecialization>();
        //        if (Specilisation != null && Specilisation != "")
        //        {
        //            var ids = Utility.StringIdsToListIds(Specilisation);
        //            foreach (var ca in ids)
        //            {
        //                var value = db.FacultySpecialization.Find(ca);
        //                ObjITsection.Add(value);
        //                c.FacultySpecialization = ObjITsection;

        //            }
        //        }
        //        else
        //        {
        //            c.FacultySpecialization = null;
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
        //                try
        //                {

        //                    //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        db.FacultyInternal.Attach(db_data);
        //                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = db_data.RowVersion;
        //                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

        //                        var Curr_Lookup = db.FacultyInternal.Find(data);
        //                        TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
        //                        db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

        //                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        {

        //                            FacultyInternal blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.FacultyInternal.Where(e => e.Id == data).Include(e => e.Address).Include(e => e.ContactDetails).Include(e => e.FacultySpecialization)
        //                                                  .SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };
        //                            FacultyInternal FacultyInternal = new FacultyInternal()
        //                            {
        //                                Id = data,
        //                                Code = c.Code == null ? "" : c.Code,
        //                                Name = c.Name == null ? "" : c.Name,
        //                                //  FacultyType = c.FacultyType,
        //                                EmpID = c.EmpID,
        //                                Address = c.Address,
        //                                FacultySpecialization = c.FacultySpecialization,
        //                                ContactDetails = c.ContactDetails,
        //                                DBTrack = c.DBTrack
        //                            };
        //                            db.FacultyInternal.Attach(FacultyInternal);
        //                            db.Entry(FacultyInternal).State = System.Data.Entity.EntityState.Modified;

        //                            db.Entry(FacultyInternal).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

        //                            // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


        //                            using (var context = new DataBaseContext())
        //                            {

        //                                var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                DT_FacultyInternal DT_Corp = (DT_FacultyInternal)obj;
        //                                DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
        //                                DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;

        //                                db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();

        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = FacultyInternal.Id, Val = FacultyInternal.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            //return Json(new Object[] { lk.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
        //                        }
        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (LanguageSkill)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (LanguageSkill)databaseEntry.ToObject();
        //                        c.RowVersion = databaseValues.RowVersion;

        //                    }
        //                }
        //                Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //        else
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {

        //                FacultyInternal blog = null; // to retrieve old data
        //                DbPropertyValues originalBlogValues = null;
        //                FacultyInternal Old_Corp = null;

        //                using (var context = new DataBaseContext())
        //                {
        //                    blog = context.FacultyInternal.Include(e => e.FacultySpecialization).Include(e => e.Address).Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
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
        //                FacultyInternal FacultyInternal = new FacultyInternal()
        //                {
        //                    Id = data,
        //                    Code = c.Code == null ? "" : c.Code,
        //                    Name = c.Name == null ? "" : c.Name,
        //                    //  FacultyType = c.FacultyType,
        //                    EmpID = c.EmpID,
        //                    Address = c.Address,
        //                    FacultySpecialization = c.FacultySpecialization,
        //                    ContactDetails = c.ContactDetails,
        //                    DBTrack = c.DBTrack
        //                };
        //                using (var context = new DataBaseContext())
        //                {
        //                    var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, FacultyInternal, "FacultyInternal", c.DBTrack);
        //                    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                    Old_Corp = context.FacultyInternal.Include(e => e.FacultySpecialization).Include(e => e.Address).Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
        //                    DT_FacultyInternal DT_Corp = (DT_FacultyInternal)obj;
        //                    //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                    //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                    //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                    db.Create(DT_Corp);
        //                    //db.SaveChanges();
        //                }
        //                blog.DBTrack = c.DBTrack;
        //                db.FacultyInternal.Attach(blog);
        //                db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                db.SaveChanges();
        //                ts.Complete();

        //                Msg.Add("  Record Updated");
        //                return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return Json(new Object[] { blog.Id, c.Institute, "Record Updated", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogFile Logfile = new LogFile();
        //        ErrorLog Err = new ErrorLog()
        //        {
        //            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //            ExceptionMessage = ex.Message,
        //            ExceptionStackTrace = ex.StackTrace,
        //            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //            LogTime = DateTime.Now
        //        };
        //        Logfile.CreateLogFile(Err);
        //        Msg.Add(ex.Message);
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //    }

        //    return View();
        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(FacultyInternal c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.FacultyInternal.Include(e => e.FacultySpecialization)
                      .Include(e => e.Address)
                        .Include(e => e.ContactDetails)
                        .Where(e => e.Id == data)
                       .SingleOrDefault();

                    string Category = form["Category"] == "0" ? "" : form["Category"];
                    string Addrs = form["Address_List"] == "0" ? "" : form["Address_List"];
                    string ContactDetails = form["Contact_List"] == "0" ? "" : form["Contact_List"];
                    string employee = form["Employee_List"] == "0" ? "" : form["Employee_List"];
                    string Specilisation = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
                    bool Auth = form["autho_allow"] == "true" ? true : false;

                    //if (FacultyType != null && FacultyType != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(FacultyType));
                    //    //    db_Data.FacultyType = val; 
                    //}

                    List<FacultySpecialization> ObjITsection = new List<FacultySpecialization>();
                    if (Specilisation != null && Specilisation != "")
                    {
                        var ids = Utility.StringIdsToListIds(Specilisation);
                        foreach (var ca in ids)
                        {
                            var value = db.FacultySpecialization.Find(ca);
                            ObjITsection.Add(value);
                            c.FacultySpecialization = ObjITsection;
                            db_data.FacultySpecialization = ObjITsection;
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
                            db_data.ContactDetails = val;
                            c.ContactDetails = val;
                        }
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
                                    db.FacultyInternal.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_Lookup = db.FacultyInternal.Find(data);
                                    TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                    db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        FacultyInternal blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.FacultyInternal.Include(e => e.FacultySpecialization).Include(e => e.Address).Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
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
                                        FacultyInternal FacultyInternal = new FacultyInternal()
                                        {
                                            Id = data,
                                            Code = c.Code == null ? "" : c.Code,
                                            Name = c.Name == null ? "" : c.Name,
                                            //  FacultyType = c.FacultyType,
                                            EmpID = c.EmpID,
                                            Address = c.Address,
                                            FacultySpecialization = c.FacultySpecialization,
                                            ContactDetails = c.ContactDetails,
                                            DBTrack = c.DBTrack
                                        };

                                        db.FacultyInternal.Attach(FacultyInternal);
                                        db.Entry(FacultyInternal).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(FacultyInternal).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                        // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                        using (var context = new DataBaseContext())
                                        {

                                            var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                            DT_FacultyInternal DT_Corp = (DT_FacultyInternal)obj;

                                            db.Create(DT_Corp);
                                            db.SaveChanges();
                                        }
                                        await db.SaveChangesAsync();
                                        ts.Complete();

                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = FacultyInternal.Id, Val = FacultyInternal.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { lk.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (FacultyInternal)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (FacultyInternal)databaseEntry.ToObject();
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

                            FacultyInternal blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            FacultyInternal Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.FacultyInternal.Include(e => e.FacultySpecialization)
                      .Include(e => e.Address)
                        .Include(e => e.ContactDetails)
                        .Where(e => e.Id == data)
                       .SingleOrDefault();
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
                            FacultyInternal FacultyInternal = new FacultyInternal()
                            {
                                Id = data,
                                Code = c.Code == null ? "" : c.Code,
                                Name = c.Name == null ? "" : c.Name,
                                //  FacultyType = c.FacultyType,
                                EmpID = c.EmpID,
                                Address = c.Address,
                                FacultySpecialization = c.FacultySpecialization,
                                ContactDetails = c.ContactDetails,
                                DBTrack = c.DBTrack
                            };
                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, FacultyInternal, "FacultyInternal", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.FacultyInternal.Include(e => e.FacultySpecialization)
                      .Include(e => e.Address)
                        .Include(e => e.ContactDetails)
                        .Where(e => e.Id == data)
                       .SingleOrDefault();
                                DT_FacultyInternal DT_Corp = (DT_FacultyInternal)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.FacultyInternal.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = FacultyInternal.Id, Val = FacultyInternal.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (auth_action == "C")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        FacultyInternal corp = db.FacultyInternal.Include(e => e.Address)
                            .Include(e => e.ContactDetails)
                            .Include(e => e.EmpID)
                   .FirstOrDefault(e => e.Id == auth_id);

                        corp.DBTrack = new DBTrack
                        {
                            Action = "C",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = corp.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.FacultyInternal.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corp.DBTrack);
                        DT_FacultyInternal DT_Corp = (DT_FacultyInternal)rtn_Obj;
                        DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                        // DT_Corp.FacultyType_Id = corp.FacultyType == null ? 0 : corp.FacultyType.Id;
                        DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {

                    FacultyInternal Old_Corp = db.FacultyInternal
                                                      .Include(e => e.Address)
                                                      .Include(e => e.ContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();


                    DT_FacultyInternal Curr_Corp = db.DT_FacultyInternal
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_Corp != null)
                    {
                        FacultyInternal corp = new FacultyInternal();

                        string Corp = Curr_Corp.FacultyType_Id == null ? null : Curr_Corp.FacultyType_Id.ToString();
                        string Addrs = Curr_Corp.Address_Id == null ? null : Curr_Corp.Address_Id.ToString();
                        string ContactDetails = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();
                        string Specilisation = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();

                        corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                        corp.Code = Curr_Corp.Code == null ? Old_Corp.Code : Curr_Corp.Code;

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    corp.DBTrack = new DBTrack
                                    {
                                        CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                        CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                        ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                        AuthorizedBy = SessionManager.UserName,
                                        AuthorizedOn = DateTime.Now,
                                        IsModified = false
                                    };

                                    int a = EditS(Corp, Addrs, ContactDetails, Specilisation, auth_id, corp, corp.DBTrack);
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (FacultyInternal)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (FacultyInternal)databaseEntry.ToObject();
                                    corp.RowVersion = databaseValues.RowVersion;
                                }
                            }

                            return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                        return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                }
                else if (auth_action == "D")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        FacultyInternal corp = db.FacultyInternal.AsNoTracking().Include(e => e.Address)

                                                                    .Include(e => e.ContactDetails).FirstOrDefault(e => e.Id == auth_id);

                        Address add = corp.Address;
                        ContactDetails conDet = corp.ContactDetails;
                        //   LookupValue val = corp.FacultyType;

                        corp.DBTrack = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.FacultyInternal.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corp.DBTrack);
                        DT_FacultyInternal DT_Corp = (DT_FacultyInternal)rtn_Obj;
                        DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                        //   DT_Corp.FacultyType_Id = corp.FacultyType == null ? 0 : corp.FacultyType.Id;
                        DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();
                        db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                    }

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
        //        IEnumerable<FacultyInternal> FacultyInternal = null;
        //        if (gp.IsAutho == true)
        //        {
        //            FacultyInternal = db.FacultyInternal.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            FacultyInternal = db.FacultyInternal.AsNoTracking().ToList();
        //        }

        //        IEnumerable<FacultyInternal> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = FacultyInternal;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToLower() == gp.searchString.ToLower()) || (e.Name.ToLower() == gp.searchString.ToLower()))).ToList();
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.FacultyType) != null ? Convert.ToString(a.FacultyType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name}).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = FacultyInternal;
        //            Func<FacultyInternal, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Code" ? c.Code :
        //                                 gp.sidx == "Name" ? c.Name :
        //                                 gp.sidx == "Name" ? c.Name : "");
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
        //            totalRecords = FacultyInternal.Count();
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
                    var jsonData = (Object)null;

                    IEnumerable<FacultyInternal> FacultyInternal = null;
                    List<FacultyInternal> chkd = new List<FacultyInternal>();
                    FacultyInternal = db.FacultyInternal.Include(q => q.EmpID).Include(q => q.EmpID.EmpName).ToList();
                    if (gp.IsAutho == true)
                    {
                        chkd = db.FacultyInternal.Include(q => q.EmpID).Include(q => q.EmpID.EmpName).Where(e => e.DBTrack.IsModified == true).ToList();
                    }
                    else
                    {
                        // Employee = db.Employee.Where(a=>a.Id==db.EmpAcademicInfo.Contains()).ToList();
                        chkd = db.FacultyInternal.Include(q => q.EmpID).Include(q => q.EmpID.EmpName).Where(q => q.EmpID != null).ToList();
                    }
                    // List<Employee> chkd1 = new List<Employee>();
                    //  chkd1=chkd.Select(e=>e.EmpID).ToList();
                    IEnumerable<FacultyInternal> IE;

                    if (!string.IsNullOrEmpty(gp.searchField))
                    {
                        IE = chkd;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                                   || (e.EmpID.EmpCode.ToString().Contains(gp.searchString.ToUpper())) || (e.EmpID.EmpName.FullNameFML.ToString().Contains(gp.searchString.ToUpper())))
                               .Select(a => new Object[] { a.Id, a.EmpID.EmpCode, a.EmpID.EmpName.FullNameFML }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpID.EmpCode, a.EmpID.EmpName.FullNameFML }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = chkd;
                        Func<FacultyInternal, string> orderfuc = (c => gp.sidx.ToUpper() == "ID" ? c.Id.ToString() : "");



                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.EmpID.EmpCode, a.EmpID.EmpName.FullNameFML }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.EmpID.EmpCode, a.EmpID.EmpName.FullNameFML }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpID.EmpCode, a.EmpID.EmpName.FullNameFML }).ToList();
                        }
                        totalRecords = FacultyInternal.Count();
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

        //public ActionResult P2BGrid_FacultyInternal(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<FacultyInternal> FacultyInternal = null;
        //        if (gp.IsAutho == true)
        //        {
        //            FacultyInternal = db.FacultyInternal.Include(e => e.FacultyType).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            FacultyInternal = db.FacultyInternal.Include(e => e.FacultyType).AsNoTracking().ToList();
        //        }

        //        IEnumerable<FacultyInternal> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = FacultyInternal;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToLower() == gp.searchString.ToLower()) || (e.Name.ToLower() == gp.searchString.ToLower()))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.FacultyType != null ? Convert.ToString(a.FacultyType.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = FacultyInternal;
        //            Func<FacultyInternal, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Code" ? c.Code :
        //                                 gp.sidx == "Name" ? c.Name :
        //                                 gp.sidx == "Name" ? c.Name : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), a.FacultyType != null ? Convert.ToString(a.FacultyType.LookupVal) : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), a.FacultyType != null ? Convert.ToString(a.FacultyType.LookupVal) : "" }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.FacultyType != null ? Convert.ToString(a.FacultyType.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = FacultyInternal.Count();
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
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    FacultyInternal FacultyInternals = db.FacultyInternal.Include(e => e.Address)
                                                       .Include(e => e.ContactDetails)
                                                     .Where(e => e.Id == data).SingleOrDefault();

                    Address add = FacultyInternals.Address;
                    ContactDetails conDet = FacultyInternals.ContactDetails;
                    if (FacultyInternals.ContactDetails != null)
                    {
                        Msg.Add(" Child record exists.Cannot remove it..  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (FacultyInternals.Address != null)
                    {
                        Msg.Add(" Child record exists.Cannot remove it..  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    //  LookupValue val = FacultyInternals.FacultyType;
                    if (FacultyInternals.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = FacultyInternals.DBTrack.CreatedBy != null ? FacultyInternals.DBTrack.CreatedBy : null,
                                CreatedOn = FacultyInternals.DBTrack.CreatedOn != null ? FacultyInternals.DBTrack.CreatedOn : null,
                                IsModified = FacultyInternals.DBTrack.IsModified == true ? true : false
                            };
                            FacultyInternals.DBTrack = dbT;
                            db.Entry(FacultyInternals).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, FacultyInternals.DBTrack);
                            DT_FacultyInternal DT_Corp = (DT_FacultyInternal)rtn_Obj;
                            DT_Corp.Address_Id = FacultyInternals.Address == null ? 0 : FacultyInternals.Address.Id;
                            //  DT_Corp.FacultyType_Id = FacultyInternals.FacultyType == null ? 0 : FacultyInternals.FacultyType.Id;
                            DT_Corp.ContactDetails_Id = FacultyInternals.ContactDetails == null ? 0 : FacultyInternals.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                                    CreatedBy = FacultyInternals.DBTrack.CreatedBy != null ? FacultyInternals.DBTrack.CreatedBy : null,
                                    CreatedOn = FacultyInternals.DBTrack.CreatedOn != null ? FacultyInternals.DBTrack.CreatedOn : null,
                                    IsModified = FacultyInternals.DBTrack.IsModified == true ? false : false//,
                                };

                                db.Entry(FacultyInternals).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dbT);
                                DT_FacultyInternal DT_Corp = (DT_FacultyInternal)rtn_Obj;
                                DT_Corp.Address_Id = add == null ? 0 : add.Id;
                                //     DT_Corp.FacultyType_Id = val == null ? 0 : val.Id;
                                DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                                db.Create(DT_Corp);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        public int EditS(string Corp, string Addrs, string ContactDetails, string Specilisation, int data, FacultyInternal c, DBTrack dbT)
        {
            //if (Corp != null)
            //{
            //    if (Corp != "")
            //    {
            //        var val = db.LookupValue.Find(int.Parse(Corp));
            //       // c.FacultyType = val;

            //        var type = db.FacultyInternal.Where(e => e.Id == data).SingleOrDefault();
            //        IList<FacultyInternal> typedetails = null;
            //        if (type.FacultyType != null)
            //        {
            //            typedetails = db.FacultyInternal.Where(x => x.FacultyType.Id == type.FacultyType.Id && x.Id == data).ToList();
            //        }
            //        else
            //        {
            //            typedetails = db.FacultyInternal.Where(x => x.Id == data).ToList();
            //        }
            //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
            //        foreach (var s in typedetails)
            //        {
            //            s.FacultyType = c.FacultyType;
            //            db.FacultyInternal.Attach(s);
            //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //            db.SaveChanges();
            //            TempData["RowVersion"] = s.RowVersion;
            //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //        }
            //    }
            //    else
            //    {
            //        var BusiTypeDetails = db.FacultyInternal.Include(e => e.FacultyType).Where(x => x.Id == data).ToList();
            //        foreach (var s in BusiTypeDetails)
            //        {
            //            s.FacultyType = null;
            //            db.FacultyInternal.Attach(s);
            //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //            db.SaveChanges();
            //            TempData["RowVersion"] = s.RowVersion;
            //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //        }
            //    }
            //}
            //else
            //{
            //    var BusiTypeDetails = db.FacultyInternal.Include(e => e.FacultyType).Where(x => x.Id == data).ToList();
            //    foreach (var s in BusiTypeDetails)
            //    {
            //        s.FacultyType = null;
            //        db.FacultyInternal.Attach(s);
            //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //        db.SaveChanges();
            //        TempData["RowVersion"] = s.RowVersion;
            //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //    }
            //}
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.Address.Find(int.Parse(Addrs));
                        c.Address = val;

                        var add = db.FacultyInternal.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                        IList<FacultyInternal> addressdetails = null;
                        if (add.Address != null)
                        {
                            addressdetails = db.FacultyInternal.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.FacultyInternal.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Address = c.Address;
                                db.FacultyInternal.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var addressdetails = db.FacultyInternal.Include(e => e.Address).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Address = null;
                        db.FacultyInternal.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (ContactDetails != null && ContactDetails != "")
                {
                    var val = db.ContactDetails.Find(int.Parse(ContactDetails));
                    c.ContactDetails = val;

                    var add = db.FacultyInternal.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                    IList<FacultyInternal> contactsdetails = null;
                    if (add.ContactDetails != null)
                    {
                        contactsdetails = db.FacultyInternal.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        contactsdetails = db.FacultyInternal.Where(x => x.Id == data).ToList();
                    }
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = c.ContactDetails;
                        db.FacultyInternal.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }

                }
                if (Specilisation != null && Specilisation != "")
                {
                    List<int> IDs = Specilisation.Split(',').Select(e => int.Parse(e)).ToList();
                    foreach (var k in IDs)
                    {
                        var value = db.FacultySpecialization.Find(k);
                        c.FacultySpecialization = new List<FacultySpecialization>();
                        c.FacultySpecialization.Add(value);
                    }
                }
                else
                {
                    var Doctordetails = db.EmpMedicalInfo.Include(e => e.Doctor).Where(x => x.Id == data).ToList();
                    foreach (var s in Doctordetails)
                    {
                        s.Doctor = null;
                        db.EmpMedicalInfo.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                var CurCorp = db.FacultyInternal.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    FacultyInternal corp = new FacultyInternal()
                    {
                        Code = c.Code,
                        Name = c.Name,
                        Id = data,
                        DBTrack = c.DBTrack
                    };

                    db.FacultyInternal.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    return 1;
                }
                return 0;
            }
        }

        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var changedEntries = db.ChangeTracker.Entries()
                    .Where(x => x.State != System.Data.Entity.EntityState.Unchanged).ToList();

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Modified))
                {
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Added))
                {
                    entry.State = System.Data.Entity.EntityState.Detached;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Deleted))
                {
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }
            }
        }





        /* ---------------------------- Details Contact -----------------------*/

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

        /*------------------------------- ContactDetails Delete --------------------------------- */
        [HttpPost]
        public ActionResult ContactDetailsRemove(int? data, int? forwarddata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                ContactDetails contDet = db.ContactDetails.Find(data);
                FacultyInternal loc = db.FacultyInternal.Find(forwarddata);
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

        /*--------------------------------------------- Lookup Details Address --------------------------------------- */

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



        public ActionResult ValidateForm(FacultyInternal c, FormCollection form) //forvalidating form
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (db.FacultyInternal.Any(r => r.Code == c.Code))
                {

                    var Msg = new List<string>();
                    Msg.Add("Code Already Exists..!");
                    return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                // for success
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                //for error
                //return Json(new { success = false, responseText = "Not Valid..!" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}