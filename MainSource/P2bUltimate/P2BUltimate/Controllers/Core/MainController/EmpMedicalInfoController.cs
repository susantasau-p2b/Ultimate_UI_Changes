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
    public class EmpMedicalInfoController : Controller
    {

        //  private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/EmpMedicalInfo/Index.cshtml");
        }

        public ActionResult Createcontactdetails_partial()
        {
            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        }
        //public ActionResult GetAllergyDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Allergy.Include(a=>a.AllergyMedicine).ToList();
        //        IEnumerable<Allergy> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Allergy.Include(a => a.AllergyMedicine).ToList().Where(d => d.FullDetails.Contains(data));
        //        }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public ActionResult GetAllergyDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Allergy.Include(a => a.AllergyMedicine).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Allergy.Include(b => b.AllergyMedicine).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        //public ActionResult GetDiseaseDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Disease.Include(a=>a.DiseaseType).Include(a=>a.DiseaseMedicine).ToList();
        //        IEnumerable<Disease> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Disease.Include(a => a.DiseaseType).Include(a => a.DiseaseMedicine).ToList().Where(d => d.FullDetails.Contains(data));
        //        }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public ActionResult GetDiseaseDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Disease.Include(a => a.DiseaseType).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Disease.Include(b => b.DiseaseType).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult GetDoctorDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Doctor.Include(e => e.Name).Include(e => e.Specialization).ToList();
        //        IEnumerable<Doctor> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Doctor.ToList().Where(d => d.FullDetails.Contains(data));
        //        }
        //        else
        //        {
        //            //var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public ActionResult GetDoctorDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Doctor.Include(e => e.Name).Include(e => e.Specialization).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Doctor.Include(e => e.Name).Include(e => e.Specialization).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        //public ActionResult GetEmergencyContactDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.EmergencyContact.Include(a=>a.Name).Include(a=>a.ContactDetails).Include(a=>a.Relation).ToList();
        //        IEnumerable<EmergencyContact> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.EmergencyContact.Include(a => a.Name).Include(a => a.ContactDetails).Include(a => a.Relation).ToList().Where(d => d.FullDetails.Contains(data));
        //        }
        //        else
        //        {
        //            //var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public ActionResult GetEmergencyContactDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.EmergencyContact.Include(a => a.Name).Include(a => a.ContactDetails).Include(a => a.Relation).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.EmergencyContact.Include(b => b.Name).Include(b => b.ContactDetails).Include(b => b.Relation).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(EmpMedicalInfo c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string Category = form["BloodGrouplist"] == "0" ? "" : form["BloodGrouplist"];
                    string Addrs = form["HospitalAddressList"] == "0" ? "" : form["HospitalAddressList"];
                    string HospitalContactDetails = form["HospitalContactDetailslist"] == "0" ? "" : form["HospitalContactDetailslist"];
                    //string Allergy = form["Alergylist"] == "0" ? "" : form["Alergylist"];
                    //string Disease = form["Diseaselist"] == "0" ? "" : form["Diseaselist"];
                    //string EmergencyContact = form["EmergencyContactlist"] == "0" ? "" : form["EmergencyContactlist"];
                    //string Doctor = form["Doctorlist"] == "0" ? "" : form["Doctorlist"];

                    List<Allergy> Allergyval = new List<Allergy>();
                    string Allergy = form["Alergylist"];
                    c.Allergy = null;
                    if (Allergy != null)
                    {
                        var ids = Utility.StringIdsToListIds(Allergy);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.Allergy.Find(ca);
                            Allergyval.Add(Lookup_val);
                            c.Allergy = Allergyval;
                        }
                    }



                    List<Disease> DiseaseVal = new List<Disease>();
                    string Disease = form["Diseaselist"];
                    c.Disease = null;
                    if (Disease != null)
                    {
                        var ids = Utility.StringIdsToListIds(Disease);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.Disease.Find(ca);
                            DiseaseVal.Add(Lookup_val);
                            c.Disease = DiseaseVal;
                        }
                    }


                    List<EmergencyContact> EmergencyContactval = new List<EmergencyContact>();
                    string EmergencyContact = form["EmergencyContactlist"];
                    c.EmergencyContact = null;
                    if (EmergencyContact != null)
                    {
                        var ids = Utility.StringIdsToListIds(EmergencyContact);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.EmergencyContact.Find(ca);
                            EmergencyContactval.Add(Lookup_val);
                            c.EmergencyContact = EmergencyContactval;
                        }
                    }

                    List<Doctor> Doctorval = new List<Doctor>();
                    string Doctor = form["Doctorlist"];
                    c.Doctor = null;
                    if (Doctor != null)
                    {
                        var ids = Utility.StringIdsToListIds(Doctor);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.Doctor.Find(ca);
                            Doctorval.Add(Lookup_val);
                            c.Doctor = Doctorval;
                        }
                    }


                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Category));
                            c.BloodGroup = val;
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
                            c.HospitalAddress = val;
                        }
                    }




                    if (HospitalContactDetails != null)
                    {
                        if (HospitalContactDetails != "")
                        {
                            int ContId = Convert.ToInt32(HospitalContactDetails);
                            var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.HospitalContactDetails = val;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        Employee EmpData;
                        if (Emp != null && Emp != 0)
                        {
                            EmpData = db.Employee.Find(Emp);
                        }
                        else
                        {
                            List<string> Msgu = new List<string>();
                            Msg.Add("  Kindly select employee  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        using (TransactionScope ts = new TransactionScope())
                        {


                            var EmpMedicInfo = db.Employee.Include(e => e.EmpmedicalInfo)
                                        .Include(e => e.EmpmedicalInfo.HospitalContactDetails)
                                        .Include(e => e.EmpmedicalInfo.HospitalAddress)
                                         .Include(e => e.EmpmedicalInfo.Allergy)
                                         .Include(e => e.EmpmedicalInfo.Disease)
                                         .Include(e => e.EmpmedicalInfo.Disease.Select(r => r.DiseaseType))
                                          .Include(e => e.EmpmedicalInfo.Doctor)
                                          .Include(e => e.EmpmedicalInfo.Doctor.Select(r => r.Name))
                                           .Include(e => e.EmpmedicalInfo.EmergencyContact)
                                         .Where(e => e.Id != null).ToList();
                            foreach (var item in EmpMedicInfo)
                            {
                                if (item.EmpmedicalInfo != null && EmpData.EmpmedicalInfo != null)
                                {
                                    if (item.EmpmedicalInfo.Id == EmpData.EmpmedicalInfo.Id)
                                    {
                                        var v = EmpData.EmpCode;
                                        Msg.Add("Record Already Exist For Employee Code=" + v);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }

                            }


                            //if (db.EmpMedicalInfo.Any(o => o.PreferredHospital == c.PreferredHospital))
                            //{
                            //    Msg.Add("  PreferredHospital Already Exists.  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //    // return Json(new Object[] { "", "", "PreferredHospital Already Exists.", JsonRequestBehavior.AllowGet });
                            //}
                            if (c.Height >= 15)
                            {
                                Msg.Add("  Height Should Not Be Grater Than 15. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Height Should Not Be Grater Than 15.", JsonRequestBehavior.AllowGet });
                            }
                            if (c.Weight >= 200)
                            {
                                Msg.Add("  Weight Should Not Be Grater Than 200kg.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Weight Should Not Be Grater Than 200kg.", JsonRequestBehavior.AllowGet });
                            }

                            //if (db.EmpMedicalInfo.Any(o => o.PreferredHospital == c.PreferredHospital))
                            //{
                            //    Msg.Add("  PreferredHospital Already Exists.  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //    // return Json(new Object[] { "", "", "PreferredHospital Already Exists.", JsonRequestBehavior.AllowGet });
                            //}

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            EmpMedicalInfo EmpMedicalInfo = new EmpMedicalInfo()
                            {
                                PreferredHospital = c.PreferredHospital == null ? "" : c.PreferredHospital.Trim(),
                                IDMark = c.IDMark == null ? "" : c.IDMark.Trim(),
                                BloodGroup = c.BloodGroup,
                                HospitalAddress = c.HospitalAddress,
                                HospitalContactDetails = c.HospitalContactDetails,
                                Allergy = c.Allergy,
                                Disease = c.Disease,
                                Doctor = c.Doctor,
                                EmergencyContact = c.EmergencyContact,
                                Height = c.Height,
                                Weight = c.Weight,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.EmpMedicalInfo.Add(EmpMedicalInfo);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                DT_EmpMedicalInfo DT_OBJ = (DT_EmpMedicalInfo)rtn_Obj;
                                DT_OBJ.HospitalAddress_Id = c.HospitalAddress == null ? 0 : c.HospitalAddress.Id;
                                DT_OBJ.HospitalContactDetails_Id = c.HospitalContactDetails == null ? 0 : c.HospitalContactDetails.Id;
                                DT_OBJ.BloodGroup_Id = c.BloodGroup == null ? 0 : c.BloodGroup.Id;
                                db.Create(DT_OBJ);
                                db.SaveChanges();

                                if (EmpData != null)
                                {
                                    EmpData.EmpmedicalInfo = EmpMedicalInfo;
                                    db.Employee.Attach(EmpData);
                                    db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(EmpData).State = System.Data.Entity.EntityState.Detached;
                                }

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
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

                        // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
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

        public ActionResult Allergy_partial()
        {
            return View("~/Views/Shared/Core/_Allergy.cshtml");
        }


        public ActionResult Disease_partial()
        {
            return View("~/Views/Shared/Core/_Disease.cshtml");
        }



        public ActionResult Doctor_partial()
        {
            return View("~/Views/Shared/Core/_Doctor.cshtml");
        }


        public ActionResult EmergencyContact_partial()
        {
            return View("~/Views/Shared/Core/_EmergancyContact.cshtml");
        }

        public ActionResult HospitalAddress_partial()
        {
            return View("~/Views/Shared/Core/_Address.cshtml");
        }

        public ActionResult HospitalContactDetails_partial()
        {
            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        }

        public class EmpMedicalInfo_Val
        {
            public string HospitalAddress_FullAddress { get; set; }
            public string HospitalAddress_Id { get; set; }
            public string HospitalContactDetails_Id { get; set; }
            public string FullContactDetails { get; set; }
            public Array Allergy_id { get; set; }
            public Array Allergy_val { get; set; }
            public Array Disease_id { get; set; }
            public Array Disease_val { get; set; }
            public Array Doctor_id { get; set; }
            public Array Doctor_val { get; set; }
            public Array EMCN_id { get; set; }
            public Array EMCN_val { get; set; }

        }




        //[HttpPost]
        //public ActionResult Edit(int data)
        //{
        //    //string tableName = "EmpMedicalInfo";

        //    //    // Fetch the table records dynamically
        //    //    var tableData = db.GetType()
        //    //    .GetProperty(tableName)
        //    //    .GetValue(db, null);

        //    List<EmpMedicalInfo_Val> return_data = new List<EmpMedicalInfo_Val>();

        //    var Q = db.EmpMedicalInfo
        //        .Include(e => e.HospitalContactDetails)
        //        .Include(e => e.HospitalAddress)
        //        .Include(e => e.BloodGroup)
        //        .Include(e => e.HospitalContactDetails)
        //        .Where(e => e.Id == data).Select
        //        (e => new
        //        {
        //            PreferredHospital = e.PreferredHospital,
        //            IDMark = e.IDMark,
        //            Height = e.Height,
        //            Weight = e.Weight,
        //            BloodGroup_Id = e.BloodGroup.Id == null ? 0 : e.BloodGroup.Id,
        //            Action = e.DBTrack.Action
        //        }).ToList();

        //    var add_data = db.EmpMedicalInfo
        //      .Include(e => e.HospitalContactDetails)
        //        .Include(e => e.HospitalAddress)
        //        .Include(e => e.Allergy)
        //        .Include(e => e.Disease)
        //        .Include(e => e.Disease.Select(r => r.DiseaseType))
        //         .Include(e => e.Doctor)
        //         .Include(e => e.Doctor.Select(r => r.Name))
        //          .Include(e => e.EmergencyContact)
        //        .Where(e => e.Id == data)
        //       .ToList();
        //    foreach (var ca in add_data)
        //    {
        //        return_data.Add(
        //        new EmpMedicalInfo_Val
        //        {
        //            Allergy_id = ca.Allergy.Select(e => e.Id.ToString()).ToArray(),
        //            Allergy_val = ca.Allergy.Select(e => e.FullDetails.ToString()).ToArray(),
        //            Disease_id = ca.Disease.Select(e => e.Id.ToString()).ToArray(),
        //            Disease_val = ca.Disease.Select(e => e.FullDetails.ToString()).ToArray(),
        //            Doctor_id = ca.Doctor.Select(e => e.Id.ToString()).ToArray(),
        //            Doctor_val = ca.Doctor.Select(e => e.FullDetails.ToString()).ToArray(),
        //            EMCN_id = ca.EmergencyContact.Select(e => e.Id.ToString()).ToArray(),
        //            EMCN_val = ca.EmergencyContact.Select(e => e.FullDetails.ToString()).ToArray(),
        //            HospitalAddress_Id = ca.HospitalAddress.Id.ToString(),
        //            HospitalAddress_FullAddress = ca.HospitalAddress.FullAddress,
        //            HospitalContactDetails_Id = ca.HospitalContactDetails.Id.ToString(),
        //            FullContactDetails = ca.HospitalContactDetails.FullContactDetails,
        //        });
        //    }



        //    //var ICLTN_Data = db.EmpMedicalInfo.Include(e => e.Allergy).Include(e => e.Disease).Where(e => e.Id == data).ToList();

        //    //foreach (var ca in ICLTN_Data)
        //    //{
        //    //    return_data.Add(
        //    //    new EmpMedicalInfo_Val
        //    //    {
        //    //        Allergy_id = ca.Allergy.Select(x => new Allergy { Id = x.Id }).ToArray(),
        //    //        Allergy_val = ca.Allergy.Select(x => new Allergy { FullDetails = x.FullDetails }).ToArray(),
        //    //        Disease_id = ca.Disease.Select(x => new Disease { Id = x.Id }).ToArray(),
        //    //        Disease_val = ca.Disease.Select(x => new Disease { FullDetails = x.FullDetails }).ToArray()
        //    //    });
        //    //}

        //    var W = db.DT_EmpMedicalInfo
        //         .Include(e => e.Allergy)
        //         .Include(e => e.Disease)
        //         .Include(e => e.Doctor)
        //         .Include(e => e.EmergencyContact)
        //         .Include(e => e.HospitalAddress_Id)
        //         .Include(e => e.BloodGroup_Id)
        //         .Include(e => e.HospitalContactDetails_Id)
        //         .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
        //         (e => new
        //         {
        //             DT_Id = e.Id,
        //             PreferredHospital = e.PreferredHospital == null ? "" : e.PreferredHospital,
        //             Weight = e.Weight,
        //             Height = e.Height,
        //             IDMark = e.IDMark == null ? "" : e.IDMark,
        //             BloodGroup_Val = e.BloodGroup_Id == null ? "" : e.BloodGroup_Id.ToString(),
        //             HospitalAddress_Val = e.HospitalAddress_Id == null ? "" : e.HospitalAddress_Id.ToString(),
        //             HospitalContactDetails_Val = e.HospitalContactDetails_Id == null ? "" : e.HospitalContactDetails_Id.ToString(),
        //             //Contact_Val = e.HospitalContactDetails.Id == null ? "" : e.HospitalContactDetails.FullContactDetails,
        //         }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

        //    var Corp = db.EmpMedicalInfo.Find(data);
        //    TempData["RowVersion"] = Corp.RowVersion;
        //    var Auth = Corp.DBTrack.IsModified;
        //    return Json(new Object[] { Q, return_data, W, Auth, JsonRequestBehavior.AllowGet });
        //}

        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "EmpMedicalInfo";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);

            List<EmpMedicalInfo_Val> return_data = new List<EmpMedicalInfo_Val>();
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.Employee.Include(e => e.EmpmedicalInfo)
                    .Include(e => e.EmpmedicalInfo.HospitalContactDetails)
                    .Include(e => e.EmpmedicalInfo.HospitalAddress)
                    .Include(e => e.EmpmedicalInfo.BloodGroup)
                    .Include(e => e.EmpmedicalInfo.HospitalContactDetails)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        PreferredHospital = e.EmpmedicalInfo.PreferredHospital,
                        IDMark = e.EmpmedicalInfo.IDMark,
                        Height = e.EmpmedicalInfo.Height,
                        Weight = e.EmpmedicalInfo.Weight,
                        BloodGroup_Id = e.EmpmedicalInfo.BloodGroup.Id == null ? 0 : e.EmpmedicalInfo.BloodGroup.Id,
                        HospitalAddress_Id = e.EmpmedicalInfo.HospitalAddress == null ? "" : e.EmpmedicalInfo.HospitalAddress.Id.ToString(),
                        HospitalAddress_FullAddress = e.EmpmedicalInfo.HospitalAddress == null ? "" : e.EmpmedicalInfo.HospitalAddress.FullAddress,
                        HospitalContactDetails_Id = e.EmpmedicalInfo.HospitalContactDetails == null ? "" : e.EmpmedicalInfo.HospitalContactDetails.Id.ToString(),
                        FullContactDetails = e.EmpmedicalInfo.HospitalContactDetails == null ? "" : e.EmpmedicalInfo.HospitalContactDetails.FullContactDetails,

                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data1 = db.Employee.Include(e => e.EmpmedicalInfo)
                 .Include(e => e.EmpmedicalInfo.HospitalContactDetails)
                   .Include(e => e.EmpmedicalInfo.HospitalAddress)
                   .Include(e => e.EmpmedicalInfo.Allergy)
                   .Include(e => e.EmpmedicalInfo.Disease)
                   .Include(e => e.EmpmedicalInfo.Disease.Select(r => r.DiseaseType))
                    .Include(e => e.EmpmedicalInfo.Doctor)
                    .Include(e => e.EmpmedicalInfo.Doctor.Select(r => r.Name))
                     .Include(e => e.EmpmedicalInfo.EmergencyContact)
                   .Where(e => e.Id == data)
                  .SingleOrDefault();

                var add_data = db.Employee.Include(e => e.EmpmedicalInfo)
                  .Include(e => e.EmpmedicalInfo.HospitalContactDetails)
                    .Include(e => e.EmpmedicalInfo.HospitalAddress)
                    .Include(e => e.EmpmedicalInfo.Allergy)
                    .Include(e => e.EmpmedicalInfo.Disease)
                    .Include(e => e.EmpmedicalInfo.Disease.Select(r => r.DiseaseType))
                     .Include(e => e.EmpmedicalInfo.Doctor)
                     .Include(e => e.EmpmedicalInfo.Doctor.Select(r => r.Name))
                      .Include(e => e.EmpmedicalInfo.EmergencyContact)
                         .Include(e => e.EmpmedicalInfo.EmergencyContact.Select(r => r.Name))
                    .Where(e => e.Id == data)
                   .ToList();



                foreach (var ca in add_data)
                {
                    return_data.Add(
                    new EmpMedicalInfo_Val
                    {

                        Allergy_id = ca.EmpmedicalInfo.Allergy.Count > 0 ? ca.EmpmedicalInfo.Allergy.Select(e => e.Id.ToString()).ToArray() : null,
                        Allergy_val = ca.EmpmedicalInfo.Allergy.Count > 0 ? ca.EmpmedicalInfo.Allergy.Select(e => e.FullDetails.ToString()).ToArray() : null,
                        Disease_id = ca.EmpmedicalInfo.Disease.Count > 0 ? ca.EmpmedicalInfo.Disease.Select(e => e.Id.ToString()).ToArray() : null,
                        Disease_val = ca.EmpmedicalInfo.Disease.Count > 0 ? ca.EmpmedicalInfo.Disease.Select(e => e.FullDetails.ToString()).ToArray() : null,
                        Doctor_id = ca.EmpmedicalInfo.Doctor.Count > 0 ? ca.EmpmedicalInfo.Doctor.Select(e => e.Id.ToString()).ToArray() : null,
                        Doctor_val = ca.EmpmedicalInfo.Doctor.Count > 0 ? ca.EmpmedicalInfo.Doctor.Select(e => e.FullDetails.ToString()).ToArray() : null,
                        EMCN_id = ca.EmpmedicalInfo.EmergencyContact.Count > 0 ? ca.EmpmedicalInfo.EmergencyContact.Select(e => e.Id.ToString()).ToArray() : null,
                        EMCN_val = ca.EmpmedicalInfo.EmergencyContact.Count > 0 ? ca.EmpmedicalInfo.EmergencyContact.Select(e => e.FullDetails.ToString()).ToArray() : null,
                        HospitalAddress_Id = ca.EmpmedicalInfo.HospitalAddress == null ? "" : ca.EmpmedicalInfo.HospitalAddress.Id.ToString(),
                        HospitalAddress_FullAddress = ca.EmpmedicalInfo.HospitalAddress == null ? "" : ca.EmpmedicalInfo.HospitalAddress.FullAddress,
                        HospitalContactDetails_Id = ca.EmpmedicalInfo.HospitalContactDetails == null ? "" : ca.EmpmedicalInfo.HospitalContactDetails.Id.ToString(),
                        FullContactDetails = ca.EmpmedicalInfo.HospitalContactDetails == null ? "" : ca.EmpmedicalInfo.HospitalContactDetails.FullContactDetails,
                    });
                }



                //var ICLTN_Data = db.EmpMedicalInfo.Include(e => e.Allergy).Include(e => e.Disease).Where(e => e.Id == data).ToList();

                //foreach (var ca in ICLTN_Data)
                //{
                //    return_data.Add(
                //    new EmpMedicalInfo_Val
                //    {
                //        Allergy_id = ca.Allergy.Select(x => new Allergy { Id = x.Id }).ToArray(),
                //        Allergy_val = ca.Allergy.Select(x => new Allergy { FullDetails = x.FullDetails }).ToArray(),
                //        Disease_id = ca.Disease.Select(x => new Disease { Id = x.Id }).ToArray(),
                //        Disease_val = ca.Disease.Select(x => new Disease { FullDetails = x.FullDetails }).ToArray()
                //    });
                //}

                var W = db.DT_EmpMedicalInfo
                     .Include(e => e.Allergy)
                     .Include(e => e.Disease)
                     .Include(e => e.Doctor)
                     .Include(e => e.EmergencyContact)
                     .Include(e => e.HospitalAddress_Id)
                     .Include(e => e.BloodGroup_Id)
                     .Include(e => e.HospitalContactDetails_Id)
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         PreferredHospital = e.PreferredHospital == null ? "" : e.PreferredHospital,
                         Weight = e.Weight,
                         Height = e.Height,
                         IDMark = e.IDMark == null ? "" : e.IDMark,
                         BloodGroup_Val = e.BloodGroup_Id == null ? "" : e.BloodGroup_Id.ToString(),
                         HospitalAddress_Val = e.HospitalAddress_Id == null ? "" : e.HospitalAddress_Id.ToString(),
                         HospitalContactDetails_Val = e.HospitalContactDetails_Id == null ? "" : e.HospitalContactDetails_Id.ToString(),
                         //Contact_Val = e.HospitalContactDetails.Id == null ? "" : e.HospitalContactDetails.FullContactDetails,
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.EmpMedicalInfo.Find(add_data1.EmpmedicalInfo.Id);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave1(EmpMedicalInfo c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Corp = form["BloodGrouplist"] == "0" ? "" : form["BloodGrouplist"];
                    string Addrs = form["HospitalAddressList"] == "0" ? "" : form["HospitalAddressList"];
                    string HospitalContactDetails = form["HospitalContactDetailslist"] == "0" ? "" : form["HospitalContactDetailslist"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;



                    var db_Data = db.Employee.Include(e => e.EmpmedicalInfo)
                      .Include(e => e.EmpmedicalInfo.HospitalContactDetails)
                        .Include(e => e.EmpmedicalInfo.HospitalAddress)
                        .Include(e => e.EmpmedicalInfo.Allergy)
                        .Include(e => e.EmpmedicalInfo.Disease)
                        .Include(e => e.EmpmedicalInfo.Disease.Select(r => r.DiseaseType))
                         .Include(e => e.EmpmedicalInfo.Doctor)
                         .Include(e => e.EmpmedicalInfo.Doctor.Select(r => r.Name))
                          .Include(e => e.EmpmedicalInfo.EmergencyContact)
                        .Where(e => e.Id == data)
                       .SingleOrDefault();

                    var db_Data1 = db.EmpMedicalInfo.Include(e => e.Allergy).Include(e => e.Disease).Include(e => e.Doctor).Include(e => e.EmergencyContact).Where(e => e.Id == data).SingleOrDefault();


                    int AID = db_Data.EmpmedicalInfo.Id;

                    db_Data.EmpmedicalInfo.Allergy = null;
                    db_Data.EmpmedicalInfo.Disease = null;
                    db_Data.EmpmedicalInfo.Doctor = null;
                    db_Data.EmpmedicalInfo.EmergencyContact = null;

                    List<Allergy> AllergyVal = new List<Allergy>();
                    string AllergyV = form["Alergylist"];
                    if (AllergyV != null)
                    {
                        var ids = Utility.StringIdsToListIds(AllergyV);
                        foreach (var ca in ids)
                        {
                            var Allergy_val = db.Allergy.Find(ca);
                            AllergyVal.Add(Allergy_val);
                            db_Data.EmpmedicalInfo.Allergy = AllergyVal;
                        }
                    }
                    List<Disease> DiseaseVal = new List<Disease>();
                    string DiseaseV = form["Diseaselist"];
                    if (DiseaseV != null)
                    {
                        var ids = Utility.StringIdsToListIds(DiseaseV);
                        foreach (var ca in ids)
                        {
                            var Disease_val = db.Disease.Find(ca);
                            DiseaseVal.Add(Disease_val);
                            db_Data.EmpmedicalInfo.Disease = DiseaseVal;
                        }
                    }
                    List<EmergencyContact> EmergencyContactVal = new List<EmergencyContact>();
                    string EmergencyContactV = form["EmergencyContactlist"];
                    if (EmergencyContactV != null && EmergencyContactV != "0")
                    {
                        var ids = Utility.StringIdsToListIds(EmergencyContactV);
                        foreach (var ca in ids)
                        {
                            var EmergencyContact_val = db.EmergencyContact.Find(ca);
                            EmergencyContactVal.Add(EmergencyContact_val);
                            db_Data.EmpmedicalInfo.EmergencyContact = EmergencyContactVal;
                        }
                    }
                    List<Doctor> DoctorVal = new List<Doctor>();
                    string DoctorV = form["Doctorlist"];
                    if (DoctorV != null && DoctorV != "0")
                    {
                        var ids = Utility.StringIdsToListIds(DoctorV);
                        foreach (var ca in ids)
                        {
                            var Doctor_val = db.Doctor.Find(ca);
                            DoctorVal.Add(Doctor_val);
                            db_Data.EmpmedicalInfo.Doctor = DoctorVal;
                        }
                    }

                    var alrgy = db_Data.EmpmedicalInfo.Allergy;
                    var Dises = db_Data.EmpmedicalInfo.Disease;
                    var Dctr = db_Data.EmpmedicalInfo.Doctor;
                    var EmrgContact = db_Data.EmpmedicalInfo.EmergencyContact;

                    if (Corp != null)
                    {
                        if (Corp != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Corp));
                            c.BloodGroup = val;
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
                            c.HospitalAddress = val;
                        }
                    }

                    if (HospitalContactDetails != null)
                    {
                        if (HospitalContactDetails != "")
                        {
                            int ContId = Convert.ToInt32(HospitalContactDetails);
                            var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.HospitalContactDetails = val;
                        }
                    }

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {

                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    db.EmpMedicalInfo.Attach(db_Data.EmpmedicalInfo);
                                    db.Entry(db_Data.EmpmedicalInfo).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_Data.RowVersion;
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_OBJ = db.EmpMedicalInfo.Find(AID);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                    //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    //{
                                    EmpMedicalInfo blog = blog = null;
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.EmpMedicalInfo.Where(e => e.Id == AID).SingleOrDefault();
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
                                    EmpMedicalInfo lk = new EmpMedicalInfo
                                    {
                                        Id = data,
                                        Allergy = alrgy,
                                        Disease = Dises,
                                        Doctor = Dctr,
                                        EmergencyContact = EmrgContact,
                                        Height = c.Height,
                                        Weight = c.Weight,
                                        HospitalAddress = c.HospitalAddress,
                                        HospitalContactDetails = c.HospitalContactDetails,
                                        PreferredHospital = c.PreferredHospital,
                                        DBTrack = c.DBTrack
                                    };


                                    db.EmpMedicalInfo.Attach(lk);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];



                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_EmpMedicalInfo DT_Corp = (DT_EmpMedicalInfo)obj;

                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }

                                    await db.SaveChangesAsync();
                                    //DisplayTrackedEntities(db.ChangeTracker);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return Json(new Object[] { lk.Id, lk.PreferredHospital, "Record Updated", JsonRequestBehavior.AllowGet });
                                    // }
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

                    //        try
                    //        {

                    //            //DbContextTransaction transaction = db.Database.BeginTransaction();

                    //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //            {
                    //                EmpMedicalInfo blog = null; // to retrieve old data
                    //                DbPropertyValues originalBlogValues = null;

                    //                using (var context = new DataBaseContext())
                    //                {
                    //                    blog = context.EmpMedicalInfo.Where(e => e.Id == data).Include(e => e.BloodGroup)
                    //                                            .Include(e => e.Allergy).Include(e=>e.Disease)
                    //                                            .Include(e => e.Doctor).Include(e => e.EmergencyContact)
                    //                                            .Include(e => e.HospitalAddress).Include(e => e.HospitalContactDetails).SingleOrDefault();
                    //                    originalBlogValues = context.Entry(blog).OriginalValues;
                    //                }

                    //                c.DBTrack = new DBTrack
                    //                {
                    //                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                    //                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                    //                    Action = "M",
                    //                    ModifiedBy = SessionManager.UserName,
                    //                    ModifiedOn = DateTime.Now
                    //                };

                    //                int a = EditS(Corp, Addrs, HospitalContactDetails, AllergyV, DiseaseV, DoctorV, EmergencyContactV, data, c, c.DBTrack);



                    //                using (var context = new DataBaseContext())
                    //                {
                    //                    var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    //                    DT_EmpMedicalInfo DT_OBJ = (DT_EmpMedicalInfo)obj;
                    //                    DT_OBJ.HospitalAddress_Id = blog.HospitalAddress == null ? 0 : blog.HospitalAddress.Id;
                    //                    DT_OBJ.BloodGroup_Id= blog.BloodGroup == null ? 0 : blog.BloodGroup.Id;
                    //                    DT_OBJ.HospitalContactDetails_Id = blog.HospitalContactDetails == null ? 0 : blog.HospitalContactDetails.Id;
                    //                    db.Create(DT_OBJ);
                    //                    db.SaveChanges();
                    //                }
                    //                await db.SaveChangesAsync();
                    //                ts.Complete();


                    //                return Json(new Object[] { c.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                    //            }
                    //        }
                    //        catch (DbUpdateConcurrencyException ex)
                    //        {
                    //            var entry = ex.Entries.Single();
                    //            var clientValues = (EmpMedicalInfo)entry.Entity;
                    //            var databaseEntry = entry.GetDatabaseValues();
                    //            if (databaseEntry == null)
                    //            {
                    //                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    //            }
                    //            else
                    //            {
                    //                var databaseValues = (EmpMedicalInfo)databaseEntry.ToObject();
                    //                c.RowVersion = databaseValues.RowVersion;

                    //            }
                    //        }

                    //        return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    //    }
                    //}
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            EmpMedicalInfo blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            EmpMedicalInfo Old_OBJ = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.EmpMedicalInfo
                                    .Where(e => e.Id == data).SingleOrDefault();
                                TempData["RowVersion"] = blog.RowVersion;
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
                            EmpMedicalInfo empMedInfo = new EmpMedicalInfo()
                            {
                                Allergy = alrgy,
                                Disease = Dises,
                                Doctor = Dctr,
                                EmergencyContact = EmrgContact,
                                PreferredHospital = blog.PreferredHospital,
                                //Allergy =blog.Allergy,
                                //EmergencyContact = blog.EmergencyContact,
                                //Disease = blog.Disease,
                                //Doctor = blog.Doctor,
                                Height = blog.Height,
                                Weight = blog.Weight,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;
                            db.EmpMedicalInfo.Attach(empMedInfo);
                            db.Entry(empMedInfo).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(empMedInfo).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            TempData["RowVersion"] = db_Data.RowVersion;
                            db.Entry(empMedInfo).State = System.Data.Entity.EntityState.Detached;

                            using (var context = new DataBaseContext())
                            {

                                //var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, look, "EmpMedicalInfo", look.DBTrack);
                                //// var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                ////Old_LKup = context.EmpMedicalInfo.Where(e => e.Id == data).Include(e => e.Allergy).SingleOrDefault();
                                //DT_Lookup DT_LKup = (DT_Lookup)obj;

                                //db.Create(DT_LKup);

                                //var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Corporate", c.DBTrack);
                                //// var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                //Old_Corp = context.Corporate.Where(e => e.Id == data).Include(e => e.BusinessType)
                                //    .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
                                //DT_Corporate DT_Corp = (DT_Corporate)obj;
                                //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                //db.Create(DT_Corp);

                                //var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, empMedInfo, "EmpMedicalInfo", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "EmpMedicalInfo", c.DBTrack);
                                Old_OBJ = context.EmpMedicalInfo.Where(e => e.Id == AID).Include(e => e.BloodGroup)
                                    .Include(e => e.HospitalAddress).Include(e => e.HospitalContactDetails).SingleOrDefault();
                                DT_EmpMedicalInfo DT_OBJ = (DT_EmpMedicalInfo)obj;
                                DT_OBJ.HospitalAddress_Id = DBTrackFile.ValCompare(Old_OBJ.HospitalAddress, c.HospitalAddress);//Old_OBJ.Address == c.Address ? 0 : Old_OBJ.Address == null && c.Address != null ? c.Address.Id : Old_OBJ.Address.Id;
                                DT_OBJ.BloodGroup_Id = DBTrackFile.ValCompare(Old_OBJ.BloodGroup, c.BloodGroup); //Old_OBJ.BusinessType == c.BusinessType ? 0 : Old_OBJ.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_OBJ.BusinessType.Id;
                                DT_OBJ.HospitalContactDetails_Id = DBTrackFile.ValCompare(Old_OBJ.HospitalContactDetails, c.HospitalContactDetails); //Old_OBJ.ContactDetails == c.ContactDetails ? 0 : Old_OBJ.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_OBJ.ContactDetails.Id;
                                db.Create(DT_OBJ);
                                //db.SaveChanges();
                            }
                            //blog.DBTrack = c.DBTrack;
                            //db.Entry(blog).State = System.Data.Entity.EntityState.Detached;
                            //db.EmpMedicalInfo.Attach(blog);
                            //db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            //db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { blog.Id, c.PreferredHospital, "Record Updated", JsonRequestBehavior.AllowGet });
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


        [HttpPost]
        public async Task<ActionResult> EditSave(EmpMedicalInfo c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<string> Msg = new List<string>();


                    //var db_data = db.EmpAcademicInfo.Include(e => e.Awards)
                    //            .Include(e => e.Hobby)
                    //            .Include(e => e.LanguageSkill.Select(r => r.SkillType))
                    //              .Include(e => e.QualificationDetails)
                    //            .Include(e => e.Skill)
                    //            .Include(e => e.Scolarship).Where(e => e.Id == data).SingleOrDefault();
                    string Corp = form["BloodGrouplist"] == "0" ? "" : form["BloodGrouplist"];
                    string Addrs = form["HospitalAddressList"] == "0" ? "" : form["HospitalAddressList"];
                    string HospitalContactDetails = form["HospitalContactDetailslist"] == "0" ? "" : form["HospitalContactDetailslist"];

                    var db_Data = db.Employee.Include(e => e.EmpmedicalInfo)
                      .Include(e => e.EmpmedicalInfo.HospitalContactDetails)
                        .Include(e => e.EmpmedicalInfo.HospitalAddress)
                        .Include(e => e.EmpmedicalInfo.Allergy)
                        .Include(e => e.EmpmedicalInfo.Disease)
                        .Include(e => e.EmpmedicalInfo.Disease.Select(r => r.DiseaseType))
                         .Include(e => e.EmpmedicalInfo.Doctor)
                         .Include(e => e.EmpmedicalInfo.Doctor.Select(r => r.Name))
                          .Include(e => e.EmpmedicalInfo.EmergencyContact)
                        .Where(e => e.Id == data)
                       .SingleOrDefault();

                    int AID = db_Data.EmpmedicalInfo.Id;


                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    db_Data.EmpmedicalInfo.Allergy = null;
                    db_Data.EmpmedicalInfo.Disease = null;
                    db_Data.EmpmedicalInfo.Doctor = null;
                    db_Data.EmpmedicalInfo.EmergencyContact = null;
                    db_Data.EmpmedicalInfo.HospitalContactDetails = null;
                    db_Data.EmpmedicalInfo.HospitalAddress = null;
                    List<Allergy> AllergyVal = new List<Allergy>();
                    string AllergyV = form["Alergylist"];
                    if (AllergyV != null)
                    {
                        var ids = Utility.StringIdsToListIds(AllergyV);
                        foreach (var ca in ids)
                        {
                            var Allergy_val = db.Allergy.Find(ca);
                            AllergyVal.Add(Allergy_val);
                            db_Data.EmpmedicalInfo.Allergy = AllergyVal;
                        }
                    }
                    List<Disease> DiseaseVal = new List<Disease>();
                    string DiseaseV = form["Diseaselist"];
                    if (DiseaseV != null)
                    {
                        var ids = Utility.StringIdsToListIds(DiseaseV);
                        foreach (var ca in ids)
                        {
                            var Disease_val = db.Disease.Find(ca);
                            DiseaseVal.Add(Disease_val);
                            db_Data.EmpmedicalInfo.Disease = DiseaseVal;
                        }
                    }
                    List<EmergencyContact> EmergencyContactVal = new List<EmergencyContact>();
                    string EmergencyContactV = form["EmergencyContactlist"];
                    if (EmergencyContactV != null && EmergencyContactV != "0")
                    {
                        var ids = Utility.StringIdsToListIds(EmergencyContactV);
                        foreach (var ca in ids)
                        {
                            var EmergencyContact_val = db.EmergencyContact.Find(ca);
                            EmergencyContactVal.Add(EmergencyContact_val);
                            db_Data.EmpmedicalInfo.EmergencyContact = EmergencyContactVal;
                        }
                    }
                    List<Doctor> DoctorVal = new List<Doctor>();
                    string DoctorV = form["Doctorlist"];
                    if (DoctorV != null && DoctorV != "0")
                    {
                        var ids = Utility.StringIdsToListIds(DoctorV);
                        foreach (var ca in ids)
                        {
                            var Doctor_val = db.Doctor.Find(ca);
                            DoctorVal.Add(Doctor_val);
                            db_Data.EmpmedicalInfo.Doctor = DoctorVal;
                        }
                    }

                    var alrgy = db_Data.EmpmedicalInfo.Allergy;
                    var Dises = db_Data.EmpmedicalInfo.Disease;
                    var Dctr = db_Data.EmpmedicalInfo.Doctor;
                    var EmrgContact = db_Data.EmpmedicalInfo.EmergencyContact;

                    if (Corp != null)
                    {
                        if (Corp != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Corp));
                            c.BloodGroup = val;
                            db_Data.EmpmedicalInfo.BloodGroup = c.BloodGroup;
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
                            c.HospitalAddress = val;
                            db_Data.EmpmedicalInfo.HospitalAddress = c.HospitalAddress;
                        }
                    }

                    if (HospitalContactDetails != null)
                    {
                        if (HospitalContactDetails != "")
                        {
                            int ContId = Convert.ToInt32(HospitalContactDetails);
                            var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.HospitalContactDetails = val;
                            db_Data.EmpmedicalInfo.HospitalContactDetails = c.HospitalContactDetails;
                        }
                    }
                    if (Corp != null)
                    {
                        if (Corp != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Corp));
                            c.BloodGroup = val;

                            var type = db.EmpMedicalInfo.Include(e => e.BloodGroup).Where(e => e.Id == data).SingleOrDefault();
                            IList<EmpMedicalInfo> typedetails = null;
                            if (type.BloodGroup != null)
                            {
                                typedetails = db.EmpMedicalInfo.Where(x => x.BloodGroup.Id == type.BloodGroup.Id && x.Id == data).ToList();
                            }
                            else
                            {
                                typedetails = db.EmpMedicalInfo.Where(x => x.Id == data).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.BloodGroup = c.BloodGroup;
                                db.EmpMedicalInfo.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
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
                                    db.EmpMedicalInfo.Attach(db_Data.EmpmedicalInfo);
                                    db.Entry(db_Data.EmpmedicalInfo).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_Data.EmpmedicalInfo.RowVersion;
                                    db.Entry(db_Data.EmpmedicalInfo).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_Lookup = db.EmpMedicalInfo.Find(AID);
                                    TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                    db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        EmpMedicalInfo blog = null; // to retrieve old data
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.EmpMedicalInfo.Where(e => e.Id == AID).Include(e => e.Allergy)
                                    .Include(e => e.BloodGroup)
                                    .Include(e => e.Disease)
                                      .Include(e => e.Doctor)
                                    .Include(e => e.EmergencyContact)
                                    .Include(e => e.HospitalAddress).Include(e => e.HospitalContactDetails).SingleOrDefault();
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
                                        EmpMedicalInfo lk = new EmpMedicalInfo
                                        {
                                            Id = AID,
                                            Allergy = alrgy,
                                            Disease = Dises,
                                            Doctor = Dctr,
                                            EmergencyContact = EmrgContact,
                                            Height = c.Height,
                                            Weight = c.Weight,
                                            HospitalAddress = c.HospitalAddress,
                                            HospitalContactDetails = c.HospitalContactDetails,
                                            PreferredHospital = c.PreferredHospital,
                                            DBTrack = c.DBTrack
                                        };


                                        db.EmpMedicalInfo.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                        // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                        using (var context = new DataBaseContext())
                                        {

                                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                            DT_EmpMedicalInfo DT_Corp = (DT_EmpMedicalInfo)obj;

                                            db.Create(DT_Corp);
                                            db.SaveChanges();
                                        }
                                        await db.SaveChangesAsync();
                                        ts.Complete();

                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = lk.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (EmpMedicalInfo)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    var databaseValues = (EmpMedicalInfo)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            EmpMedicalInfo blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            EmpMedicalInfo Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.EmpMedicalInfo.Where(e => e.Id == AID).SingleOrDefault();
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
                            EmpMedicalInfo empAcademicInfo = new EmpMedicalInfo()
                            {
                                Id = AID,
                                Allergy = alrgy,
                                Disease = Dises,
                                Doctor = Dctr,
                                EmergencyContact = EmrgContact,
                                Height = c.Height,
                                Weight = c.Weight,
                                HospitalAddress = c.HospitalAddress,
                                HospitalContactDetails = c.HospitalContactDetails,
                                PreferredHospital = c.PreferredHospital,
                                DBTrack = c.DBTrack
                            };

                            db.Entry(blog).State = System.Data.Entity.EntityState.Detached;
                            empAcademicInfo.DBTrack = c.DBTrack;
                            // db.EmpAcademicInfo.Attach(empAcademicInfo);                   
                            //db.Entry(empAcademicInfo).State = System.Data.Entity.EntityState.Modified;
                            //db.Entry(empAcademicInfo).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                    List<string> Msg = new List<string>();
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
                List<string> Msg = new List<string>();
                try
                {

                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //EmpMedicalInfo OBJ = db.EmpMedicalInfo.Find(auth_id);
                            //EmpMedicalInfo OBJ = db.EmpMedicalInfo.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            EmpMedicalInfo OBJ = db.EmpMedicalInfo.Include(e => e.HospitalAddress)
                                .Include(e => e.HospitalContactDetails)
                                .Include(e => e.BloodGroup)
                                .Include(e => e.Allergy)
                                .Include(e => e.Disease)
                                .Include(e => e.Doctor)
                                .Include(e => e.EmergencyContact)
                                .FirstOrDefault(e => e.Id == auth_id);

                            OBJ.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = OBJ.DBTrack.ModifiedBy != null ? OBJ.DBTrack.ModifiedBy : null,
                                CreatedBy = OBJ.DBTrack.CreatedBy != null ? OBJ.DBTrack.CreatedBy : null,
                                CreatedOn = OBJ.DBTrack.CreatedOn != null ? OBJ.DBTrack.CreatedOn : null,
                                IsModified = OBJ.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.EmpMedicalInfo.Attach(OBJ);
                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, OBJ.DBTrack);
                            DT_EmpMedicalInfo DT_OBJ = (DT_EmpMedicalInfo)rtn_Obj;

                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();


                            using (var context = new DataBaseContext())
                            {

                            }


                            ts.Complete();

                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = OBJ.Id, Val = OBJ.IDMark, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { OBJ.Id, OBJ.IDMark, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        EmpMedicalInfo Old_OBJ = db.EmpMedicalInfo.Include(e => e.BloodGroup)
                                                          .Include(e => e.HospitalAddress)
                                                          .Include(e => e.Allergy)
                                                          .Include(e => e.Disease)
                                                          .Include(e => e.Doctor)
                                                          .Include(e => e.EmergencyContact)
                                                          .Include(e => e.HospitalContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_EmpMedicalInfo Curr_OBJ = db.DT_EmpMedicalInfo
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();
                        if (Curr_OBJ != null)
                        {
                            EmpMedicalInfo OBJ = new EmpMedicalInfo();

                            string BloodGroupOBJ = Curr_OBJ.BloodGroup_Id == null ? null : Curr_OBJ.BloodGroup_Id.ToString();
                            string Addrs = Curr_OBJ.HospitalAddress_Id == null ? null : Curr_OBJ.HospitalAddress_Id.ToString();
                            string HospitalContactDetails = Curr_OBJ.HospitalContactDetails_Id == null ? null : Curr_OBJ.HospitalContactDetails_Id.ToString();
                            string AllergyOBJ = Curr_OBJ.Allergy == null ? null : Curr_OBJ.Allergy.ToString();
                            string DiseaseOBJ = Curr_OBJ.Disease == null ? null : Curr_OBJ.Disease.ToString();
                            string DoctorOBJ = Curr_OBJ.Doctor == null ? null : Curr_OBJ.Doctor.ToString();
                            string EmergencyContactOBJ = Curr_OBJ.EmergencyContact == null ? null : Curr_OBJ.EmergencyContact.ToString();


                            OBJ.IDMark = Curr_OBJ.IDMark == null ? Old_OBJ.IDMark : Curr_OBJ.IDMark;
                            OBJ.PreferredHospital = Curr_OBJ.PreferredHospital == null ? Old_OBJ.PreferredHospital : Curr_OBJ.PreferredHospital;
                            OBJ.Height = Curr_OBJ.Height == null ? Old_OBJ.Height : Curr_OBJ.Height;
                            OBJ.Weight = Curr_OBJ.Weight == null ? Old_OBJ.Weight : Curr_OBJ.Weight;
                            //      OBJ.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        OBJ.DBTrack = new DBTrack
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

                                        int a = EditS(BloodGroupOBJ, Addrs, HospitalContactDetails, AllergyOBJ, DiseaseOBJ, DoctorOBJ, EmergencyContactOBJ, auth_id, OBJ, OBJ.DBTrack);
                                        //db.Entry(Old_OBJ).State = System.Data.Entity.EntityState.Detached;
                                        //db.EmpMedicalInfo.Attach(OBJ);
                                        //db.Entry(OBJ).State = System.Data.Entity.EntityState.Modified;
                                        //db.Entry(OBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //db.SaveChanges();
                                        //db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = OBJ.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        //return Json(new Object[] { OBJ.Id, "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (EmpMedicalInfo)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (EmpMedicalInfo)databaseEntry.ToObject();
                                        OBJ.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                        {
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        //return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //EmpMedicalInfo OBJ = db.EmpMedicalInfo.Find(auth_id);
                            EmpMedicalInfo OBJ = db.EmpMedicalInfo.AsNoTracking().Include(e => e.HospitalAddress)
                                                                        .Include(e => e.BloodGroup)
                                                                        .Include(e => e.HospitalContactDetails).FirstOrDefault(e => e.Id == auth_id);

                            Address add = OBJ.HospitalAddress;
                            ContactDetails conDet = OBJ.HospitalContactDetails;
                            LookupValue val = OBJ.BloodGroup;

                            OBJ.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = OBJ.DBTrack.ModifiedBy != null ? OBJ.DBTrack.ModifiedBy : null,
                                CreatedBy = OBJ.DBTrack.CreatedBy != null ? OBJ.DBTrack.CreatedBy : null,
                                CreatedOn = OBJ.DBTrack.CreatedOn != null ? OBJ.DBTrack.CreatedOn : null,
                                IsModified = OBJ.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.EmpMedicalInfo.Attach(OBJ);
                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Deleted;
                            //await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //    OBJ.HospitalAddress = add;
                            //    OBJ.HospitalContactDetails = conDet;
                            //    OBJ.BloodGroup = val;
                            //  //  DBTrackFile.DBTrackSave("Core/P2b.Global", "D", OBJ, null, "EmpMedicalInfo", OBJ.DBTrack);
                            //}
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, OBJ.DBTrack);
                            DT_EmpMedicalInfo DT_OBJ = (DT_EmpMedicalInfo)rtn_Obj;
                            DT_OBJ.HospitalAddress_Id = OBJ.HospitalAddress == null ? 0 : OBJ.HospitalAddress.Id;
                            DT_OBJ.HospitalContactDetails_Id = OBJ.HospitalContactDetails == null ? 0 : OBJ.HospitalContactDetails.Id;
                            DT_OBJ.BloodGroup_Id = OBJ.BloodGroup == null ? 0 : OBJ.BloodGroup.Id;
                            db.Create(DT_OBJ);
                            await db.SaveChangesAsync();

                            db.Entry(OBJ).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
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
        //        IEnumerable<EmpMedicalInfo> EmpMedicalInfo = null;
        //        if (gp.IsAutho == true)
        //        {
        //            EmpMedicalInfo = db.EmpMedicalInfo.Include(e => e.BloodGroup).Where(e => e.DBTrack.IsModified == true).ToList();
        //        }
        //        else
        //        {
        //            EmpMedicalInfo = db.EmpMedicalInfo.Include(e => e.BloodGroup).ToList();
        //        }

        //        IEnumerable<EmpMedicalInfo> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = EmpMedicalInfo;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.PreferredHospital, a.Height, a.Weight }).Where((e => (e.Id.ToString() == gp.searchString) || (e.PreferredHospital.ToLower() == gp.searchString.ToLower()) || (e.Height.ToString() == gp.searchString.ToLower()) || (e.Weight.ToString() == gp.searchString.ToLower()))).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.PreferredHospital), Convert.ToString(a.IDMark), Convert.ToString(a.BloodGroup) != null ? Convert.ToString(a.BloodGroup.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.PreferredHospital, a.Height, a.Weight }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = EmpMedicalInfo;
        //            Func<EmpMedicalInfo, int> orderfuc = (c =>
        //                                                       gp.sidx == "Id" ? c.Id : 0);
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.PreferredHospital), Convert.ToString(a.Height), Convert.ToString(a.Weight) }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.PreferredHospital), Convert.ToString(a.Height), Convert.ToString(a.Weight) }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.PreferredHospital, a.Height, a.Weight }).ToList();
        //            }
        //            totalRecords = EmpMedicalInfo.Count();
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

                IEnumerable<Employee> Employee = null;
                Employee = db.Employee.Include(q => q.EmpmedicalInfo).Include(q => q.EmpName).ToList();
                if (gp.IsAutho == true)
                {
                    Employee = db.Employee.Include(q => q.EmpmedicalInfo).Include(q => q.EmpName).Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    // Employee = db.Employee.Where(a=>a.Id==db.EmpAcademicInfo.Contains()).ToList();
                    Employee = db.Employee.Include(q => q.EmpmedicalInfo).Include(q => q.ServiceBookDates).Where(q => q.EmpmedicalInfo != null && q.ServiceBookDates.ServiceLastDate == null).ToList();
                }
                IEnumerable<Employee> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Employee;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e =>(e.EmpCode.ToString().Contains(gp.searchString.ToString()))
                                || (e.EmpName.FullNameFML.ToUpper().Contains(gp.searchString.ToUpper()))
                           || (e.Id.ToString().Contains(gp.searchString.ToString())))
                           .Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Employee;
                    Func<Employee, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.FullNameFML.ToString() :
                                          "");
                    }


                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = Employee.Count();
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
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete1(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    EmpMedicalInfo EmpMedicalInfo = db.EmpMedicalInfo.Include(e => e.Allergy)
                        .Include(e => e.Disease)
                        .Include(e => e.Doctor)
                        .Include(e => e.EmergencyContact)
                        .Include(e => e.HospitalContactDetails)
                        .Include(e => e.HospitalAddress)
                                                        .Where(e => e.Id == data).SingleOrDefault();

                    if (EmpMedicalInfo.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = EmpMedicalInfo.DBTrack.CreatedBy != null ? EmpMedicalInfo.DBTrack.CreatedBy : null,
                                CreatedOn = EmpMedicalInfo.DBTrack.CreatedOn != null ? EmpMedicalInfo.DBTrack.CreatedOn : null,
                                IsModified = EmpMedicalInfo.DBTrack.IsModified == true ? true : false
                            };
                            EmpMedicalInfo.DBTrack = dbT;
                            db.Entry(EmpMedicalInfo).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, EmpMedicalInfo.DBTrack);
                            DT_EmpMedicalInfo DT_OBJ = (DT_EmpMedicalInfo)rtn_Obj;
                            db.Create(DT_OBJ);

                            await db.SaveChangesAsync();
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
                            var selectedValues = EmpMedicalInfo.Allergy;
                            var lkValue = new HashSet<int>(EmpMedicalInfo.Allergy.Select(e => e.Id));
                            if (lkValue.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            }

                            var lkValue1 = new HashSet<int>(EmpMedicalInfo.Disease.Select(e => e.Id));
                            if (lkValue1.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            }

                            var lkValue2 = new HashSet<int>(EmpMedicalInfo.Doctor.Select(e => e.Id));
                            if (lkValue2.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            }

                            var lkValue3 = new HashSet<int>(EmpMedicalInfo.EmergencyContact.Select(e => e.Id));
                            if (lkValue3.Count > 0)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            }
                            db.Entry(EmpMedicalInfo).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    EmpMedicalInfo EmpMedicalInfo = db.EmpMedicalInfo.Include(e => e.Allergy)
                        .Include(e => e.Disease)
                        .Include(e => e.Doctor)
                        .Include(e => e.EmergencyContact)
                        .Include(e => e.HospitalContactDetails)
                        .Include(e => e.HospitalAddress)
                                                        .Where(e => e.Id == data).SingleOrDefault();


                    var EmpMedicInfo = db.Employee.Include(e => e.EmpmedicalInfo)
                                .Include(e => e.EmpmedicalInfo.HospitalContactDetails)
                                .Include(e => e.EmpmedicalInfo.HospitalAddress)
                                 .Include(e => e.EmpmedicalInfo.Allergy)
                                 .Include(e => e.EmpmedicalInfo.Disease)
                                 .Include(e => e.EmpmedicalInfo.Disease.Select(r => r.DiseaseType))
                                  .Include(e => e.EmpmedicalInfo.Doctor)
                                  .Include(e => e.EmpmedicalInfo.Doctor.Select(r => r.Name))
                                   .Include(e => e.EmpmedicalInfo.EmergencyContact)
                                 .Where(e => e.Id == data).SingleOrDefault();


                    if (EmpMedicInfo.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = EmpMedicInfo.DBTrack.CreatedBy != null ? EmpMedicInfo.DBTrack.CreatedBy : null,
                                CreatedOn = EmpMedicInfo.DBTrack.CreatedOn != null ? EmpMedicInfo.DBTrack.CreatedOn : null,
                                IsModified = EmpMedicInfo.DBTrack.IsModified == true ? true : false
                            };
                            EmpMedicInfo.DBTrack = dbT;
                            db.Entry(EmpMedicInfo).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, EmpMedicInfo.DBTrack);
                            DT_EmpMedicalInfo DT_OBJ = (DT_EmpMedicalInfo)rtn_Obj;
                            db.Create(DT_OBJ);

                            await db.SaveChangesAsync();
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
                            if (EmpMedicInfo.EmpmedicalInfo != null)
                            {



                                var selectedValues = EmpMedicInfo.EmpmedicalInfo.Allergy;
                                var lkValue = new HashSet<int>(EmpMedicInfo.EmpmedicalInfo.Allergy.Select(e => e.Id));
                                if (lkValue.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                                }

                                var lkValue1 = new HashSet<int>(EmpMedicInfo.EmpmedicalInfo.Disease.Select(e => e.Id));
                                if (lkValue1.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                                }

                                var lkValue2 = new HashSet<int>(EmpMedicInfo.EmpmedicalInfo.Doctor.Select(e => e.Id));
                                if (lkValue2.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                                }

                                var lkValue3 = new HashSet<int>(EmpMedicInfo.EmpmedicalInfo.EmergencyContact.Select(e => e.Id));
                                if (lkValue3.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                                }
                            }
                            db.Entry(EmpMedicInfo.EmpmedicalInfo).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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

        //[HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{

        //    EmpMedicalInfo corporates = db.EmpMedicalInfo.Include(e => e.HospitalAddress)
        //                                       .Include(e => e.HospitalContactDetails)
        //                                       .Include(e => e.BloodGroup).Where(e => e.Id == data).SingleOrDefault();

        //   Address add = corporates.HospitalAddress;
        //    ContactDetails conDet = corporates.HospitalContactDetails;
        //    LookupValue val = corporates.BloodGroup;
        //    //EmpMedicalInfo corporates = db.EmpMedicalInfo.Where(e => e.Id == data).SingleOrDefault();
        //    if (corporates.DBTrack.IsModified == true)
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {
        //            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "EmpMedicalInfo");
        //            DBTrack dbT = new DBTrack
        //            {
        //                Action = "D",
        //                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
        //                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
        //                IsModified = corporates.DBTrack.IsModified == true ? true : false
        //            };
        //            corporates.DBTrack = dbT;
        //            db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
        //            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "EmpMedicalInfo", corporates.DBTrack);
        //            await db.SaveChangesAsync();
        //            using (var context = new DataBaseContext())
        //            {
        //               // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "EmpMedicalInfo", corporates.DBTrack);
        //            }
        //            ts.Complete();
        //            return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    else
        //    {
        //        //var selectedRegions = corporates.Regions;

        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {
        //            //if (selectedRegions != null)
        //            //{
        //            //    var corpRegion = new HashSet<int>(corporates.Regions.Select(e => e.Id));
        //            //    if (corpRegion.Count > 0)
        //            //    {
        //            //        return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
        //            //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
        //            //    }
        //            //}

        //            try
        //            {
        //                DBTrack dbT = new DBTrack
        //                {
        //                    Action = "D",
        //                    ModifiedBy = SessionManager.UserName,
        //                    ModifiedOn = DateTime.Now,
        //                    CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
        //                    CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
        //                    IsModified = corporates.DBTrack.IsModified == true ? false : false,
        //                    AuthorizedBy = SessionManager.UserName,
        //                    AuthorizedOn = DateTime.Now
        //                };

        //                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

        //                db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
        //                await db.SaveChangesAsync();


        //                using (var context = new DataBaseContext())
        //                {
        //                    corporates.HospitalAddress = add;
        //                    corporates.HospitalContactDetails = conDet;
        //                    corporates.BloodGroup = val;
        //                   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "EmpMedicalInfo", dbT);
        //                }
        //                ts.Complete();
        //                return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

        //            }
        //            catch (RetryLimitExceededException /* dex */)
        //            {
        //                //Log the error (uncomment dex variable IDMark and add a line here to write a log.)
        //                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
        //                //return RedirectToAction("Delete");
        //                return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
        //            }
        //        }
        //    }
        //    return new EmptyResult();


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
        //            var list1 = db.EmpMedicalInfo.ToList().Select(e => e.HospitalContactDetails);
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
        public ActionResult GetContactDetLKDetails(List<int> SkipIds)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactDetails.Include(e => e.ContactNumbers).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.EmpMedicalInfo.ToList().Select(e => e.HospitalContactDetails);
                var list2 = fall.Except(list1);

                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }


        //public ActionResult GetAddressLKDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
        //                             .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
        //                             .Include(e => e.Taluka).ToList();
        //        IEnumerable<Address> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Address.ToList().Where(d => d.FullAddress.Contains(data));

        //        }
        //        else
        //        {
        //            var list1 = db.EmpMedicalInfo.Include(e => e.HospitalAddress.Area).ToList().Select(e => e.HospitalAddress);
        //            var list2 = fall.Except(list1);

        //            //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
        //            var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.CorporateCode, c.CorporateName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullAddress }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    // return View();
        //}
        [HttpPost]
        public ActionResult GetHospitalAddressDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                                     .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
                                     .Include(e => e.Taluka).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                                    .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
                                    .Include(e => e.Taluka).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var list1 = db.EmpMedicalInfo.ToList().Select(e => e.HospitalAddress);
                var list2 = fall.Except(list1);

                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }



        //public ActionResult GetHospitalAddressDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
        //                             .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
        //                             .Include(e => e.Taluka).ToList();
        //        IEnumerable<Address> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.Address.ToList().Where(d => d.FullAddress.Contains(data));

        //        }
        //        else
        //        {
        //            var list1 = db.EmpMedicalInfo.Include(e => e.HospitalAddress.Area).ToList().Select(e => e.HospitalAddress);
        //            var list2 = fall.Except(list1);

        //            //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
        //            var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.CorporateCode, c.CorporateName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullAddress }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    // return View();
        //}


        //public ActionResult GetHospitalContactDetails(string data)
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
        public ActionResult GetHospitalContactDetails(List<int> SkipIds)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactDetails.Include(e => e.ContactNumbers).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.EmpMedicalInfo.ToList().Select(e => e.HospitalContactDetails);
                var list2 = fall.Except(list1);

                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }




        public int EditS(string Corp, string Addrs, string HospitalContactDetails, string AllergyV, string DiseaseV, string DoctorV, string ECV, int data, EmpMedicalInfo ESFOBJ, DBTrack dbT)
        { // Code For dropdown
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        ESFOBJ.BloodGroup = val;

                        var type = db.EmpMedicalInfo.Include(e => e.BloodGroup).Where(e => e.Id == data).SingleOrDefault();
                        IList<EmpMedicalInfo> typedetails = null;
                        if (type.BloodGroup != null)
                        {
                            typedetails = db.EmpMedicalInfo.Where(x => x.BloodGroup.Id == type.BloodGroup.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.EmpMedicalInfo.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.BloodGroup = ESFOBJ.BloodGroup;
                            db.EmpMedicalInfo.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                //***********************************************************************************************************
                //*************************** Code For One to One********************************************
                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.Address.Find(int.Parse(Addrs));
                        ESFOBJ.HospitalAddress = val;

                        var add = db.EmpMedicalInfo.Include(e => e.HospitalAddress).Where(e => e.Id == data).SingleOrDefault();
                        IList<EmpMedicalInfo> addressdetails = null;
                        if (add.HospitalAddress != null)
                        {
                            addressdetails = db.EmpMedicalInfo.Where(x => x.HospitalAddress.Id == add.HospitalAddress.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.EmpMedicalInfo.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.HospitalAddress = ESFOBJ.HospitalAddress;
                                db.EmpMedicalInfo.Attach(s);
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
                    var addressdetails = db.EmpMedicalInfo.Include(e => e.HospitalAddress).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.HospitalAddress = null;
                        db.EmpMedicalInfo.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                //***********************************************************************************************************
                //*************************** Code For One to One********************************************
                if (HospitalContactDetails != null)
                {
                    if (HospitalContactDetails != "")
                    {
                        var val = db.ContactDetails.Find(int.Parse(HospitalContactDetails));
                        ESFOBJ.HospitalContactDetails = val;

                        var add = db.EmpMedicalInfo.Include(e => e.HospitalContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<EmpMedicalInfo> contactsdetails = null;
                        if (add.HospitalContactDetails != null)
                        {
                            contactsdetails = db.EmpMedicalInfo.Where(x => x.HospitalContactDetails.Id == add.HospitalContactDetails.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.EmpMedicalInfo.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.HospitalContactDetails = ESFOBJ.HospitalContactDetails;
                            db.EmpMedicalInfo.Attach(s);
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
                    var contactsdetails = db.EmpMedicalInfo.Include(e => e.HospitalContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.HospitalContactDetails = null;
                        db.EmpMedicalInfo.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                //***********************************************************************************************************
                //*************************** Code For One to Many********************************************
                if (AllergyV != null)
                {
                    if (AllergyV != "")
                    {
                        List<int> IDs = AllergyV.Split(',').Select(e => int.Parse(e)).ToList();
                        foreach (var k in IDs)
                        {
                            var value = db.Allergy.Find(k);
                            ESFOBJ.Allergy = new List<Allergy>();
                            ESFOBJ.Allergy.Add(value);
                        }
                    }
                }
                else
                {
                    var Allergydetails = db.EmpMedicalInfo.Include(e => e.Allergy).Where(x => x.Id == data).ToList();
                    foreach (var s in Allergydetails)
                    {
                        s.Allergy = null;
                        db.EmpMedicalInfo.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                //***********************************************************************************************************
                //*************************** Code For One to One********************************************
                if (DiseaseV != null)
                {
                    if (DiseaseV != "")
                    {
                        List<int> IDs = DiseaseV.Split(',').Select(e => int.Parse(e)).ToList();
                        foreach (var k in IDs)
                        {
                            var value = db.Disease.Find(k);
                            ESFOBJ.Disease = new List<Disease>();
                            ESFOBJ.Disease.Add(value);
                        }
                    }
                }
                else
                {
                    var Diseasedetails = db.EmpMedicalInfo.Include(e => e.Disease).Where(x => x.Id == data).ToList();
                    foreach (var s in Diseasedetails)
                    {
                        s.Disease = null;
                        db.EmpMedicalInfo.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                //***********************************************************************************************************
                //*************************** Code For One to One********************************************
                if (DoctorV != null)
                {
                    if (DoctorV != "")
                    {
                        List<int> IDs = DoctorV.Split(',').Select(e => int.Parse(e)).ToList();
                        foreach (var k in IDs)
                        {
                            var value = db.Doctor.Find(k);
                            ESFOBJ.Doctor = new List<Doctor>();
                            ESFOBJ.Doctor.Add(value);
                        }
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
                //***********************************************************************************************************
                //*************************** Code For One to One********************************************
                if (ECV != null)
                {
                    if (ECV != "")
                    {
                        List<int> IDs = ECV.Split(',').Select(e => int.Parse(e)).ToList();
                        foreach (var k in IDs)
                        {
                            var value = db.EmergencyContact.Find(k);
                            ESFOBJ.EmergencyContact = new List<EmergencyContact>();
                            ESFOBJ.EmergencyContact.Add(value);
                        }
                    }
                }
                else
                {
                    var EmergencyContactdetails = db.EmpMedicalInfo.Include(e => e.EmergencyContact).Where(x => x.Id == data).ToList();
                    foreach (var s in EmergencyContactdetails)
                    {
                        s.EmergencyContact = null;
                        db.EmpMedicalInfo.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                //***********************************************************************************************************

                var CurOBJ = db.EmpMedicalInfo.Find(data);
                TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    ESFOBJ.DBTrack = dbT;
                    EmpMedicalInfo corp = new EmpMedicalInfo()
                    {
                        PreferredHospital = ESFOBJ.PreferredHospital,
                        Height = ESFOBJ.Height,
                        Weight = ESFOBJ.Weight,
                        IDMark = ESFOBJ.IDMark,
                        Id = data,
                        DBTrack = ESFOBJ.DBTrack
                    };
                    db.EmpMedicalInfo.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }





        public void RollBack()
        {

            //  var context = DataContextFactory.GetDataContext();
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

    }


}
