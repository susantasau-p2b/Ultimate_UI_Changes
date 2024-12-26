using P2b.Global;
using EssPortal.App_Start;
using EssPortal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Text;
using EssPortal.Security;

namespace EssPortal.Controllers
{
    public class BenefitNomineesController : Controller
    {
        // GET: NomineeBenefit
        public ActionResult Index()
        {
            return View("~/Views/Shared/BenefitNominees.cshtml");
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/_BenefitNominees.cshtml");
        }

        public ActionResult view_partial()
        {
            return View("~/Views/Shared/_BenefitNomineesView.cshtml");
        }

        DataBaseContext db = new DataBaseContext();

        public ActionResult AddOrEdit(BenefitNominees lkval, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Add = form["Add"] != null && form["Add"] != "" ? Convert.ToBoolean(form["Add"]) : true;
                var Id = form["auth_id"] != null && form["auth_id"] != "" ? Convert.ToInt32(form["auth_id"]) : 0;
                if (Add == true)
                {
                    //Add
                    var returnobj = Create(lkval, form);
                    return Json(returnobj, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //Edit
                    var returnobj = EditSave(lkval, Id, form);
                    return Json(returnobj, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public Object Create(BenefitNominees c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);

                string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                string Addrs = form["Addresslist"] == "0" ? "" : form["Addresslist"];
                string ContactDetails = form["ContactDetailslist"] == "0" ? "" : form["ContactDetailslist"];
                string nominee = form["NomineeNamelist"] == "0" ? "" : form["NomineeNamelist"];

                if (nominee != null)
                {
                    if (nominee != "")
                    {
                        int nomineeId = Convert.ToInt32(nominee);
                        var vals = db.NameSingle.Where(e => e.Id == nomineeId).SingleOrDefault();
                        c.NomineeName = vals;
                    }
                }


                //if (benifit != null)
                //{
                //    List<int> IDs = benifit.Split(',').Select(e => int.Parse(e)).ToList();
                //    foreach (var k in IDs)
                //    {
                //        var value = db.NomineeBenefit.Find(k);
                //        c.BenefitList = new List<NomineeBenefit>();
                //        c.BenefitList.Add(value);


                //    }
                //}

                c.BenefitList = null;
                List<NomineeBenefit> OBJ = new List<NomineeBenefit>();
                string Values = form["BenefitTypelist"];

                if (Values != null)
                {
                    var ids = Utility.StringIdsToListIds(Values);
                    foreach (var ca in ids)
                    {
                        var OBJ_val = db.NomineeBenefit.Find(ca);
                        OBJ.Add(OBJ_val);
                        c.BenefitList = OBJ;
                    }
                }



                //List<NomineeBenefit> lookupLang = new List<NomineeBenefit>();
                //string Lang = form["Benefitlist"];

                //if (Lang != null)
                //{
                //    var ids = Utility.StringIdsToListIds(Lang);
                //    foreach (var ca in ids)
                //    {
                //        var lookup_val = db.NomineeBenefit.Find(ca);

                //        lookupLang.Add(lookup_val);
                //        c.BenefitList = lookupLang;
                //    }
                //}
                //else
                //{
                //    c.BenefitList = null;
                //}


                if (Category != null)
                {
                    if (Category != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Category));
                        c.Relation = val;
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

                Employee EmpData;
                if (Emp != 0)
                {
                    EmpData = db.Employee.Find(Emp);
                }
                else
                {
                    Msg.Add("  Kindly select employee  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {

                        //var EmpSocialInfo = db.Employee.Include(e => e.Nominees)
                        //                  .Include(e => e.Nominees.ContactDetails)
                        //                  .Include(e => e.Nominees.Address)
                        //                  .Include(e => e.Nominees.Relation)
                        //                  .Include(e => e.Nominees.NomineeName)
                        //                  .Include(e => e.Nominees.BenefitList)
                        //                  .Include(e => e.Nominees.BenefitList.Select(f => f.BenefitType))
                        //                  .Include(e => e.Nominees.ContactDetails)
                        //                  .Where(e => e.Id != null).ToList();

                        //foreach (var item in EmpSocialInfo)
                        //{
                        //    if (item.Nominees != null && EmpData.Nominees != null)
                        //    {
                        //        if (item.Nominees.Id == EmpData.Nominees.Id)
                        //        {
                        //            var v = EmpData.EmpCode;
                        //            Msg.Add("Record Alredy Exist For Employee Code=" + v);
                        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //        }
                        //    }

                        //}


                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        BenefitNominees bns = new BenefitNominees()
                        {
                            Address = c.Address,
                            ContactDetails = c.ContactDetails,
                            DateofBirth = c.DateofBirth,
                            BenefitList = c.BenefitList,
                            NomineeName = c.NomineeName,
                            Relation = c.Relation,
                            DBTrack = c.DBTrack
                        };
                        try
                        {
                            db.BenefitNominees.Add(bns);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                            db.SaveChanges();
                            DT_BenefitNominees DT_Corp = (DT_BenefitNominees)rtn_Obj;
                            DT_Corp.Orig_Id = bns.Id;
                            DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                            //    DT_Corp.BenefitList_Id = c.BenefitList == null ? 0 : c.BenefitList.Id;
                            DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                            DT_Corp.NomineeName_Id = c.NomineeName == null ? 0 : c.NomineeName.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();


                            var EmpAcedemicDataChk = db.Employee.Include(e => e.Nominees)
                                .Where(e => e.Id == Emp).SingleOrDefault();
                            if (EmpAcedemicDataChk != null)
                            {
                                if (EmpAcedemicDataChk.Nominees != null)
                                {
                                    EmpAcedemicDataChk.Nominees.Add(bns);
                                }
                                else
                                {
                                    EmpAcedemicDataChk.Nominees = new List<BenefitNominees> { bns };
                                }
                            }
                            else
                            {
                                var oEmpAcademicInfo = new BenefitNominees();
                                oEmpAcademicInfo.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                EmpAcedemicDataChk.Nominees = new List<BenefitNominees> { bns };
                            }
                            db.Entry(EmpAcedemicDataChk).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            ts.Complete();
                            //if (EmpData != null)
                            //{
                            //    EmpData.Nominees = bns;
                            //    db.Employee.Attach(EmpData);
                            //    db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                            //    db.SaveChanges();
                            //    db.Entry(EmpData).State = System.Data.Entity.EntityState.Detached;
                            //}
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", BenefitNominees, null, "BenefitNominees", null);
                            //  return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });   
                            return new { status = true, responseText = "Data Created Successfully." };


                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            //   return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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

        public ActionResult GetContactDetLKDetails(string data)
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
                    var list1 = db.Corporate.ToList().Select(e => e.ContactDetails);
                    var list2 = fall.Except(list1);
                    var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
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
                    var list1 = db.Corporate.ToList().Select(e => e.Address);
                    var list2 = fall.Except(list1);

                    var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullAddress }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetLookupBenefitNominees(List<int> SkipIds)
        {
            var fall = db.NomineeBenefit.Include(d => d.BenefitType).ToList();
            if (SkipIds != null)
            {
                foreach (var a in SkipIds)
                {
                    if (fall == null)
                        fall = db.NomineeBenefit.Include(d => d.BenefitType).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    else
                        fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                }
            }

            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLookupNomineesName(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.NameSingle.ToList();
                IEnumerable<NameSingle> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.NameSingle.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    var list1 = db.NameSingle.ToList();

                    var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public class returnDataClass
        {
            public Int32 EmpId { get; set; }
            public String val { get; set; }
            public Int32 Id2 { get; set; }
            public bool Id3 { get; set; }
            public Int32 LvHead_Id { get; set; }
        }

        public ActionResult GetMyBenefitNominees()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var qurey = db.Employee.Include(e => e.Nominees)
                    .Include(e => e.Nominees.Select(q => q.BenefitList))
                    .Include(e => e.Nominees.Select(q => q.BenefitList.Select(qa => qa.BenefitType)))
                 .Include(e => e.Nominees.Select(q => q.NomineeName))
                 .Include(e => e.Nominees.Select(q => q.Relation))
                 .Where(e => e.Id == Emp).SingleOrDefault();

                var ListreturnDataClass = new List<returnDataClass>();
                if (qurey != null && qurey.Nominees != null)
                {
                    foreach (var item in qurey.Nominees)
                    {
                        foreach (var item1 in item.BenefitList)
                        {

                            var Relation = item.Relation != null ? item.Relation.LookupVal.ToString() : "";
                            var NomineeName = item.NomineeName != null ? item.NomineeName.FullNameFML : "";
                            var BenefitType = item1.BenefitType != null ? item1.BenefitType.LookupVal.ToString() : "";
                            var BenefitPerc = item1.BenefitPerc != null ? item1.BenefitPerc : 0;
                            ListreturnDataClass.Add(new returnDataClass
                            {
                                EmpId = item.Id,
                                val =
                                "Relation :" + Relation +
                                ", NomineeName :" + NomineeName +
                                ", BenefitType :" + BenefitType +
                                ", BenefitPerc :" + BenefitPerc +
                                ""
                            });
                        }
                    }
                }
                if (ListreturnDataClass != null && ListreturnDataClass.Count > 0)
                {
                    return Json(new { status = true, data = ListreturnDataClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = ListreturnDataClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }


        }
        public class GetLvNewReqClass
        {
            public string Emp { get; set; }
            public string Relation { get; set; }
            public string NomineeName { get; set; }
            public string BenefitType { get; set; }
            public string BenefitPerc { get; set; }
            public string BenefitNomineeFD { get; set; }
            public ChildGetLvNewReqClass RowData { get; set; }
        }


        public ActionResult GetMyBenefitNominees1()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);

                var db_data =db.Employee.Include(e => e.Nominees)
                    .Include(e => e.Nominees.Select(q => q.BenefitList))
                    .Include(e => e.Nominees.Select(q => q.BenefitList.Select(qa => qa.BenefitType)))
                 .Include(e => e.Nominees.Select(q => q.NomineeName))
                 .Include(e => e.Nominees.Select(q => q.Relation))
                 .Where(e => e.Id == Id).SingleOrDefault();


                if (db_data != null)
                {
                    List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();
                    returndata.Add(new GetLvNewReqClass
                    {
                        Relation = "Relation",
                        NomineeName = "Nominee Name",
                        BenefitType = "Benefit Type",
                        BenefitPerc = "Benefit Perc",
                       

                    });
                    foreach (var item in db_data.Nominees.OrderByDescending(a => a.Id).ToList())
                    {
                        var nomineed = item.FullDetails.ToList();
                        if (nomineed != null)
                        {
                            var arralist = nomineed.ToArray().Distinct();
                            var nomineed1 = arralist != null ? string.Join(",", arralist) : null;

                            foreach (var item1 in item.BenefitList)
                            {
                                returndata.Add(new GetLvNewReqClass
                                {
                                    RowData = new ChildGetLvNewReqClass
                                    {
                                        LvNewReq = item.Id.ToString(),
                                        EmpLVid = db_data.Id.ToString(),
                                        LvHead_Id = "",
                                    },


                                    Relation = item.Relation.LookupVal.ToString() == null ? "" : item.Relation.LookupVal.ToString(),
                                    NomineeName = item.NomineeName == null ? "" : item.NomineeName.FullNameFML.ToString(),
                                    BenefitType = item1.BenefitType.LookupVal.ToString() == null ? "" : item1.BenefitType.LookupVal.ToString(),
                                    BenefitPerc = item1.BenefitPerc == null ? "" : item1.BenefitPerc.ToString(),
                                    //BenefitNomineeFD = nomineed.ToString()

                                });
                            }
                           
                        }
                    }
                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class benefitlists
        {
            public Array benefit_id { get; set; }
            public Array benefitlist_details { get; set; }
        }

        public class EmpBenifitNo_Val
        {
            public string Address_FullAddress { get; set; }
            public string Address_Id { get; set; }
            public string ContactDetails_Id { get; set; }
            public string ContactDetails { get; set; }
            public Array Allergy_id { get; set; }
            public Array Allergy_val { get; set; }
            public Array Disease_id { get; set; }
            public Array Disease_val { get; set; }
            public Array Doctor_id { get; set; }
            public Array Doctor_val { get; set; }
            public Array EMCN_id { get; set; }
            public Array EMCN_val { get; set; }

        }
        //   int auth_id = form["auth_id"] != null ? Convert.ToInt32(form["auth_id"]) : 0;


        [HttpPost]
    //    public async Task<ActionResult> EditSave(BenefitNominees c, int data, FormCollection form) // Edit submit
        public Object EditSave(BenefitNominees c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            try
            {
                string auth_id1 = form["auth_id"] != null ? form["auth_id"] : "";
//                int auth_id = form["auth_id"] != null ? Convert.ToInt32(form["auth_id"]) : 0;
                string nominee = form["NomineeNamelist"] == "0" ? "" : form["NomineeNamelist"];
                string Addrs = form["Addresslist"] == "0" ? "" : form["Addresslist"];
                string benefit = form["BenefitTypelist"] == "0" ? "" : form["BenefitTypelist"];
                string ContactDetails = form["ContactDetailslist"] == "0" ? "" : form["ContactDetailslist"];
                string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];

                //  string FacultyType = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                // string TrainingInstitute = form["TrainingInstitutelist"] == "0" ? "" : form["TrainingInstitutelist"];
                // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
                //  bool Auth = form["autho_action"] == "" ? false : true;

                var idsa = Utility.StringIdsToListIds(auth_id1);
                int auth_id = idsa[0];
                var db_Data = db.BenefitNominees
                             .Include(e => e.ContactDetails)
                             .Include(e => e.Address)
                             .Include(e => e.Relation)
                             .Include(e => e.NomineeName)
                             .Include(e => e.BenefitList)
                              .Include(e => e.BenefitList.Select(f => f.BenefitType))
                             .Include(e => e.ContactDetails)
                             .Where(e => e.Id == auth_id).SingleOrDefault();

                bool Auth = form["autho_allow"] == "true" ? true : false;
 
                    if (Category != null && Category != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Category));
                         db_Data.Relation = val;
                    }

                    List<NomineeBenefit> lookupLang = new List<NomineeBenefit>();
                    string Lang = form["BenefitTypelist"];

                    if (Lang != null)
                    {
                        var ids = Utility.StringIdsToListIds(Lang);
                        foreach (var ca in ids)
                        {
                            var lookup_val = db.NomineeBenefit.Find(ca);

                            lookupLang.Add(lookup_val);
                            db_Data.BenefitList = lookupLang;
                        }
                    }
                    else
                    {
                        db_Data.BenefitList = null;
                    }


                    if (nominee != null && nominee != "")
                    {
                        int ContId = Convert.ToInt32(nominee);
                        var val = db.NameSingle.Where(e => e.Id == ContId).SingleOrDefault();
                        db_Data.NomineeName = val;
                    }
                    if (Addrs != null && Addrs != "")
                    {
                        int ContId = Convert.ToInt32(Addrs);
                        var val = db.Address.Where(e => e.Id == ContId).SingleOrDefault();
                        db_Data.Address = val;
                    }
                    if (ContactDetails != null && ContactDetails != "")
                    {
                        int ContId = Convert.ToInt32(ContactDetails);
                        var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
                        db_Data.ContactDetails = val;
                    }

                    //   var dob = db_Data.Nominees.DateofBirth;
                    var bentype = db_Data.BenefitList;
                    var nomname = db_Data.NomineeName;
                    var addrs = db_Data.Address;
                    var cntct = db_Data.ContactDetails;
                    var reln = db_Data.Relation;


                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {


                                    using (var context = new DataBaseContext())
                                    {
                                        db.BenefitNominees.Attach(db_Data);
                                        db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        TempData["RowVersion"] = db_Data.RowVersion;
                                        db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                        var Curr_OBJ = db.BenefitNominees.Find(auth_id);
                                        TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                        db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                        {
                                            BenefitNominees blog = null; // to retrieve old data
                                            DbPropertyValues originalBlogValues = null;


                                            blog = context.BenefitNominees.Where(e => e.Id == auth_id).Include(e => e.Relation)
                                                                    .Include(e => e.Address).Include(e => e.BenefitList).Include(e => e.NomineeName)
                                                                    .Include(e => e.ContactDetails).SingleOrDefault();
                                            originalBlogValues = context.Entry(blog).OriginalValues;


                                            c.DBTrack = new DBTrack
                                            {
                                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                                Action = "M",
                                                ModifiedBy = SessionManager.UserName,
                                                ModifiedOn = DateTime.Now
                                            };

                                            BenefitNominees lk = new BenefitNominees
                                            {
                                                Id = auth_id,
                                                DateofBirth = c.DateofBirth,
                                                NomineeName = nomname,
                                                Address = addrs,
                                                ContactDetails = cntct,
                                                BenefitList = bentype,
                                                Relation = reln,
                                                DBTrack = c.DBTrack
                                            };


                                            db.BenefitNominees.Attach(lk);
                                            db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                            // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                            //db.SaveChanges();
                                            db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                            var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                            DT_BenefitNominees DT_LK = (DT_BenefitNominees)obj;
                                            //  DT_LK.Allergy = lk.Allergy.Select(e => e.Id);
                                            db.Create(DT_LK);
                                            db.SaveChanges();
                                          //  await db.SaveChangesAsync();
                                            db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                            ts.Complete();
                                            //  return Json(new Object[] { lk.Id, lk.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                            return new { status = true, responseText = "Record Updated" };

                                        }
                                    }
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (BenefitNominees)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (BenefitNominees)databaseEntry.ToObject();
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

                            BenefitNominees blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            BenefitNominees Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.BenefitNominees.Where(e => e.Id == auth_id).SingleOrDefault();
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

                            BenefitNominees corp = new BenefitNominees()
                            {

                                DateofBirth = c.DateofBirth,
                                Id = auth_id,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "BenefitNominees", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.BenefitNominees.Where(e => e.Id == auth_id).Include(e => e.Relation)
                                    .Include(e => e.Address).Include(e => e.ContactDetails).Include(e => e.NomineeName).Include(e => e.BenefitList).SingleOrDefault();
                                DT_BenefitNominees DT_Corp = (DT_BenefitNominees)obj;
                                DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails);
                                DT_Corp.BenefitList_Id = DBTrackFile.ValCompare(Old_Corp.BenefitList, c.BenefitList);
                                DT_Corp.Relation_Id = DBTrackFile.ValCompare(Old_Corp.Relation, c.Relation); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                DT_Corp.NomineeName_Id = DBTrackFile.ValCompare(Old_Corp.NomineeName, c.NomineeName);
                                //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.BenefitNominees.Attach(blog);
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

        public int EditS(string Category, string nominee, string Addrs, string ContactDetails, string benefit, int data, BenefitNominees c, DBTrack dbT)
        {
            if (Category != null)
            {
                if (Category != "")
                {
                    var val = db.LookupValue.Find(int.Parse(Category));
                    c.Relation = val;

                    var type = db.BenefitNominees.Include(e => e.Relation).Where(e => e.Id == data).SingleOrDefault();
                    IList<BenefitNominees> typedetails = null;
                    if (type.Relation != null)
                    {
                        typedetails = db.BenefitNominees.Where(x => x.Relation.Id == type.Relation.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        typedetails = db.BenefitNominees.Where(x => x.Id == data).ToList();
                    }
                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                    foreach (var s in typedetails)
                    {
                        s.Relation = c.Relation;
                        db.BenefitNominees.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }
                else
                {
                    var BusiTypeDetails = db.BenefitNominees.Include(e => e.Relation).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.Relation = null;
                        db.BenefitNominees.Attach(s);
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
                var BusiTypeDetails = db.BenefitNominees.Include(e => e.Relation).Where(x => x.Id == data).ToList();
                foreach (var s in BusiTypeDetails)
                {
                    s.Relation = null;
                    db.BenefitNominees.Attach(s);
                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //await db.SaveChangesAsync();
                    db.SaveChanges();
                    TempData["RowVersion"] = s.RowVersion;
                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                }
            }

            if (Addrs != null)
            {
                if (Addrs != "")
                {
                    var val = db.Address.Find(int.Parse(Addrs));
                    c.Address = val;

                    var add = db.BenefitNominees.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                    IList<BenefitNominees> addressdetails = null;
                    if (add.Address != null)
                    {
                        addressdetails = db.BenefitNominees.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        addressdetails = db.BenefitNominees.Where(x => x.Id == data).ToList();
                    }
                    if (addressdetails != null)
                    {
                        foreach (var s in addressdetails)
                        {
                            s.Address = c.Address;
                            db.BenefitNominees.Attach(s);
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
                var addressdetails = db.BenefitNominees.Include(e => e.Address).Where(x => x.Id == data).ToList();
                foreach (var s in addressdetails)
                {
                    s.Address = null;
                    db.BenefitNominees.Attach(s);
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

                    var add = db.BenefitNominees.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                    IList<BenefitNominees> contactsdetails = null;
                    if (add.ContactDetails != null)
                    {
                        contactsdetails = db.BenefitNominees.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        contactsdetails = db.BenefitNominees.Where(x => x.Id == data).ToList();
                    }
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = c.ContactDetails;
                        db.BenefitNominees.Attach(s);
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
                var contactsdetails = db.BenefitNominees.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                foreach (var s in contactsdetails)
                {
                    s.ContactDetails = null;
                    db.BenefitNominees.Attach(s);
                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //await db.SaveChangesAsync();
                    db.SaveChanges();
                    TempData["RowVersion"] = s.RowVersion;
                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                }
            }

            if (nominee != null)
            {
                if (nominee != "")
                {
                    var val = db.NameSingle.Find(int.Parse(nominee));
                    c.NomineeName = val;

                    var add = db.BenefitNominees.Include(e => e.NomineeName).Where(e => e.Id == data).SingleOrDefault();
                    IList<BenefitNominees> contactsdetails = null;
                    if (add.ContactDetails != null)
                    {
                        contactsdetails = db.BenefitNominees.Where(x => x.NomineeName.Id == add.NomineeName.Id && x.Id == data).ToList();
                    }
                    else
                    {
                        contactsdetails = db.BenefitNominees.Where(x => x.Id == data).ToList();
                    }
                    foreach (var s in contactsdetails)
                    {
                        s.NomineeName = c.NomineeName;
                        db.BenefitNominees.Attach(s);
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
                var contactsdetails = db.BenefitNominees.Include(e => e.NomineeName).Where(x => x.Id == data).ToList();
                foreach (var s in contactsdetails)
                {
                    s.NomineeName = null;
                    db.BenefitNominees.Attach(s);
                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //await db.SaveChangesAsync();
                    db.SaveChanges();
                    TempData["RowVersion"] = s.RowVersion;
                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                }
            }


            var CurCorp = db.BenefitNominees.Find(data);
            TempData["CurrRowVersion"] = CurCorp.RowVersion;
            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
            {
                c.DBTrack = dbT;
                BenefitNominees corp = new BenefitNominees()
                {
                    Id = c.Id,
                    FullDetails = c.FullDetails,
                    //Id = data,
                    DBTrack = c.DBTrack
                };


                db.BenefitNominees.Attach(corp);
                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                return 1;
            }
            return 0;
        }


        [HttpPost]
        public ActionResult Edit(string data)
        {
            if (data != "0")
            {

                var ids = Utility.StringIdsToListString(data);
                var ida = Convert.ToInt32(ids[0]);
                var emp = Convert.ToInt32(SessionManager.EmpId);
                var returndata = (Object)null;
                var returnCurrentData = (Object)null;
                List<benefitlists> benifitnominee = new List<benefitlists>();
                var prevcomp = db.Employee.Include(e => e.Nominees).Include(e => e.Nominees.Select(q => q.Address))
                    .Include(e => e.Nominees.Select(q => q.ContactDetails))
                                    .Where(e => e.Id == emp).SingleOrDefault();

                if (prevcomp != null)
                {

                    returndata = db.BenefitNominees
                        .Include(e => e.Relation)
                        .Include(e => e.BenefitList)
                        .Include(e => e.BenefitList.Select(q => q.BenefitType))
                        .Where(e => e.Id == ida).Select
                        (e => new
                        {
                            id = e.Id,
                            DateofBirth = e.DateofBirth,
                            Relation = e.Relation != null ? e.Relation.Id : 0,
                            isauth = true,
                            Add = false,
                            Action = e.DBTrack.Action
                        }).ToList();

                    var v = db.BenefitNominees
                        .Include(e => e.BenefitList)
                        .Include(e => e.BenefitList.Select(q => q.BenefitType)).Where(e => e.Id == ida).ToList();
                    foreach (var item in v)
                    {
                        benifitnominee.Add(
                 new benefitlists
                 {
                     benefit_id = item.BenefitList.Select(q => q.Id).ToArray(),
                     benefitlist_details = item.BenefitList.Select(q => q.FullDetails).ToArray(),
                 });
                    }
                    returnCurrentData = db.BenefitNominees
                      .Include(e => e.ContactDetails)
                        .Include(e => e.Address)
                        .Include(e => e.NomineeName)
                        .Where(e => e.Id == ida)
                        .Select(e => new
                        {
                            Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                            Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
                            Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                            FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
                            name_Id = e.NomineeName == null ? "" : e.NomineeName.Id.ToString(),
                            name_FullDetails = e.NomineeName == null ? "" : e.NomineeName.FullNameFML,
                        }).ToList();
                }
                else
                {
                    returndata = new
                    {
                        Add = true,
                    };
                }



                var Corp = db.BenefitNominees.Find(ida);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { returndata, benifitnominee, returnCurrentData, Auth, JsonRequestBehavior.AllowGet });
            }
            return View();
        }
        //    [HttpPost]
        //public ActionResult Edit(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var Emp = Convert.ToInt32(SessionManager.EmpId);
        //        var ids = Utility.StringIdsToListString(data);
        //        var id = Convert.ToInt32(ids[0]);
        //        //var qurey = db.Employee.Include(e => e.EmpAcademicInfo)
        //        //    .Include(e => e.EmpAcademicInfo.QualificationDetails)
        //        //    .Include(e => e.EmpAcademicInfo.QualificationDetails.Select(a => a.Qualification))
        //        //    .Where(e => e.Id == Emp && e.EmpAcademicInfo != null)
        //        //    .AsEnumerable().Select(e => new
        //        //    {
        //        //        oQualificationDetails = e.EmpAcademicInfo.QualificationDetails.Where(w => w.Id == id).SingleOrDefault(),
        //        //        DBTrack = e.DBTrack
        //        //    }).SingleOrDefault();

        //        var query = db.Employee.Include(e => e.Nominees)
        //            .Include(e => e.Nominees.Select(b => b.BenefitList))
        //            .Include(e => e.Nominees.Select(b => b.ContactDetails))
        //            .Include(e => e.Nominees.Select(b => b.Address))
        //            .Include(e => e.Nominees.Select(b => b.NomineeName))
        //            .Where(e => e.Id == Emp && e.EmpAcademicInfo != null)
        //            .AsEnumerable().Select(e => new
        //            {
        //                oBenefitNominee = e.Nominees.Where(b=>b.Id == id).SingleOrDefault(),
        //                DBTrack = e.DBTrack
        //            }).SingleOrDefault();




        //        var returndata = (Object)null;
        //        var Listbenefitlists = new List<benefitlists>();
        //        var returnCurrentData = (Object)null;
        //        if (query != null)
        //        {
        //            if (query.oBenefitNominee != null)
        //            {
        //                returndata = new
        //                {
        //                    id = query.oBenefitNominee.Id,
        //                    DateofBirth = query.oBenefitNominee.DateofBirth,
        //                    Relation = query.oBenefitNominee.Relation != null ? query.oBenefitNominee.Relation.Id : 0,
        //                    Action = query.oBenefitNominee.DBTrack.Action,
        //                    isauth = true,
        //                    Add = false
        //                };
        //                //   var k = qurey.oQualificationDetails.Qualification.ToList();
        //                //var k = db.QualificationDetails.Include(e => e.Qualification).Where(e => e.Id == id).Select(e => e.Qualification).ToList();
        //                var k = db.BenefitNominees.Include(e => e.Address)
        //                    .Include(e => e.BenefitList).Include(e => e.ContactDetails)
        //                    .Include(e => e.NomineeName).Include(e => e.Relation).Select(e => e.BenefitList).ToList();

        //                foreach (var val in k)
        //                {
        //                    Listbenefitlists.Add(new benefitlists
        //                    {
        //                        benefit_id = val.Select(e=>e.BenefitList.Select(p=>p.Id)).ToArray(),
        //                        benefitlist_details = val.Select(e=>e.BenefitList.Select(b=>b.FullDetails)).ToArray(),

        //                    });
        //                }
        //                //curr data
        //               returnCurrentData = db.BenefitNominees
        //             .Include(e => e.ContactDetails)
        //               .Include(e => e.Address)
        //               .Include(e => e.NomineeName)
        //               .Where(e => e.Id == id)
        //               .Select(e => new
        //               {
        //                   Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
        //                   Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
        //                   Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
        //                   FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
        //                   name_Id = e.NomineeName == null ? "" : e.NomineeName.Id.ToString(),
        //                   name_FullDetails = e.NomineeName == null ? "" : e.NomineeName.FullNameFML,
        //               }).ToList();
        //            }
        //            else
        //            {
        //                returndata = new
        //                {
        //                    Add = true,
        //                };
        //            }

        //            return Json(new Object[] { returndata, Listbenefitlists, returnCurrentData, "", JsonRequestBehavior.AllowGet });
        //        }
        //        return Json(new Object[] { returndata, Listbenefitlists, returnCurrentData, "", JsonRequestBehavior.AllowGet });
        //    }
        //}


    }
}