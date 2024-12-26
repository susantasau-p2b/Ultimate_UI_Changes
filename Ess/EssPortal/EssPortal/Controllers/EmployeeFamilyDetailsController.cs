using EssPortal.App_Start;
using P2b.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using EssPortal.Models;
using System.Data;
using EssPortal.Security;
using System.Data.Entity.Infrastructure;
using System.Text;
using System.Transactions;

namespace EssPortal.Controllers
{
    public class EmployeeFamilyDetailsController : Controller
    {
        //
        // GET: /EmployeeFamilyDetails/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetLookupFamilyName(string data, string Empid)
        {
            int empids = Convert.ToInt32(Empid);


            using (DataBaseContext db = new DataBaseContext())
            {
                var employeedata = db.Employee
                          .Include(e => e.FamilyDetails)
                          .Include(e => e.FamilyDetails.Select(t => t.MemberName))
                              .Where(e => e.Id == empids).ToList();
                var fall = employeedata.SelectMany(e => e.FamilyDetails).ToList();

                IEnumerable<FamilyDetails> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.FamilyDetails.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    //var list1 = db.FamilyDetails.ToList();

                    var r = (from ca in fall select new { srno = ca.MemberName.Id, lookupvalue = ca.MemberName.FullNameFML }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetFamilyDetails(List<int> SkipIds)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = new List<FamilyDetails>();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Employee.Include(e => e.FamilyDetails).Where(e => !e.Id.ToString().Contains(a.ToString())).SelectMany(h => h.FamilyDetails).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.FamilyDetails.ToList();
                var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(FamilyDetails c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int Emp = form["employee-table"] == "0" ? 0 : Convert.ToInt32(form["employee-table"]);
                    //  string Emp = form["employee-table"] == "0" ? "" : form["employee-table"];
                    string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    string Addrs = form["Addresslist"] == "0" ? "" : form["Addresslist"];
                    string ContactDetails = form["ContactDetailslist"] == "0" ? "" : form["ContactDetailslist"];
                    //     string benifit = form["BenefitTypelist"] == "0" ? "" : form["BenefitTypelist"];
                    string nominee = form["NomineeNamelist"] == "0" ? "" : form["NomineeNamelist"];

                    if (nominee != null)
                    {
                        if (nominee != "")
                        {
                            int nomineeId = Convert.ToInt32(nominee);
                            var vals = db.NameSingle.Where(e => e.Id == nomineeId).SingleOrDefault();
                            c.MemberName = vals;
                        }
                    }
                    if (DateTime.Now < c.DateofBirth)
                    {
                        Msg.Add("Birth Should Not be Greater than Current Date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

                    //c.BenefitList = null;
                    //List<NomineeBenefit> OBJ = new List<NomineeBenefit>();
                    //string Values = form["BenefitTypelist"];

                    //if (Values != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(Values);
                    //    foreach (var ca in ids)
                    //    {
                    //        var OBJ_val = db.NomineeBenefit.Find(ca);
                    //        OBJ.Add(OBJ_val);
                    //        c.BenefitList = OBJ;
                    //    }
                    //}



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
                        int em = Convert.ToInt32(Emp);
                        EmpData = db.Employee.Include(q => q.FamilyDetails).Where(q => q.Id == em).SingleOrDefault();
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

                            //var EmpSocialInfo = db.Employee.Include(e => e.FamilyDetails)
                            //                   .Include(e => e.FamilyDetails.Select(lm => lm.ContactDetails))
                            //                   .Include(e => e.FamilyDetails.Select(lm => lm.Address))
                            //                   .Include(e => e.FamilyDetails.Select(lm => lm.Relation))
                            //                   .Include(e => e.FamilyDetails.Select(lm => lm.MemberName))

                            //                  .Where(e => e.Id != null).ToList();

                            //foreach (var item in EmpSocialInfo)
                            //{
                            //    if (item.CanFamilyDetails.Count() > 0 && EmpData.FamilyDetails.Count() > 0)
                            //    {
                            //        var chk = item.CanFamilyDetails.Select(e => e.Id).SingleOrDefault();
                            //        var chk1 = EmpData.FamilyDetails.Select(e => e.Id).SingleOrDefault();
                            //        if (chk == chk1)
                            //        {
                            //            var v = EmpData.EmpCode;
                            //            Msg.Add("Record Alredy Exist For Employee Code=" + v);
                            //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //        }
                            //    }

                            //}


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<FamilyDetails> FamilyDetails = new List<FamilyDetails>();
                            FamilyDetails bns = new FamilyDetails()
                            {
                                Address = c.Address,
                                ContactDetails = c.ContactDetails,
                                DateofBirth = c.DateofBirth,
                                //BenefitList = c.BenefitList,
                                MemberName = c.MemberName,
                                Relation = c.Relation,
                                IsHandicap = c.IsHandicap,
                                HandicapRemark = c.HandicapRemark,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.FamilyDetails.Add(bns);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                //DT_BenefitNominees DT_Corp = (DT_BenefitNominees)rtn_Obj;
                                //DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                                ////    DT_Corp.BenefitList_Id = c.BenefitList == null ? 0 : c.BenefitList.Id;
                                //DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                //DT_Corp.NomineeName_Id = c.MemberName == null ? 0 : c.MemberName.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                                FamilyDetails.Add(db.FamilyDetails.Find(bns.Id));
                                //List<BenefitNominees>newben=new List<BenefitNominees>();
                                //newben.Add(bns);
                                if (EmpData.FamilyDetails.Count() > 0)
                                {
                                    FamilyDetails.AddRange(EmpData.FamilyDetails);

                                }
                                EmpData.FamilyDetails = FamilyDetails;
                                db.Employee.Attach(EmpData);
                                db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(EmpData).State = System.Data.Entity.EntityState.Detached;
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", BenefitNominees, null, "BenefitNominees", null);
                                ts.Complete();
                                //  return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });   
                                Msg.Add("  Data Saved successfully  ");
                                //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                return Json(new Utility.JsonReturnClass { Id = bns.Id, Val = bns.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        }

        [HttpPost]
        public ActionResult GridCreate(FamilyDetails c, FormCollection form, string Empid) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int Emp = 0;
                    if (Empid != null)
                    {
                        Emp = Convert.ToInt32(Empid);

                    }
                    else
                    {
                        Emp = form["employee-table"] == "0" ? 0 : Convert.ToInt32(form["employee-table"]);
                    }
                    //  string Emp = form["employee-table"] == "0" ? "" : form["employee-table"];
                    string Category = form["Categorylist1"] == "0" ? "" : form["Categorylist1"];
                    string Addrs = form["Addresslist"] == "0" ? "" : form["Addresslist"];
                    string ContactDetails = form["ContactDetailslist"] == "0" ? "" : form["ContactDetailslist"];
                    //     string benifit = form["BenefitTypelist"] == "0" ? "" : form["BenefitTypelist"];
                    string nominee = form["NomineeNamelist1"] == "0" ? "" : form["NomineeNamelist1"];

                    if (nominee != null)
                    {
                        if (nominee != "")
                        {
                            int nomineeId = Convert.ToInt32(nominee);
                            var vals = db.NameSingle.Where(e => e.Id == nomineeId).SingleOrDefault();
                            c.MemberName = vals;
                        }
                    }
                    if (DateTime.Now < c.DateofBirth)
                    {
                        Msg.Add("Birth Should Not be Greater than Current Date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }



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
                        int em = Convert.ToInt32(Emp);
                        EmpData = db.Employee.Include(q => q.FamilyDetails).Where(q => q.Id == em).SingleOrDefault();
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


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<FamilyDetails> FamilyDetails = new List<FamilyDetails>();
                            FamilyDetails bns = new FamilyDetails()
                            {
                                Address = c.Address,
                                ContactDetails = c.ContactDetails,
                                DateofBirth = c.DateofBirth,
                                //BenefitList = c.BenefitList,
                                MemberName = c.MemberName,
                                Relation = c.Relation,
                                IsHandicap = c.IsHandicap,
                                HandicapRemark = c.HandicapRemark,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.FamilyDetails.Add(bns);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                //DT_BenefitNominees DT_Corp = (DT_BenefitNominees)rtn_Obj;
                                //DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                                ////    DT_Corp.BenefitList_Id = c.BenefitList == null ? 0 : c.BenefitList.Id;
                                //DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                //DT_Corp.NomineeName_Id = c.MemberName == null ? 0 : c.MemberName.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();
                                FamilyDetails.Add(db.FamilyDetails.Find(bns.Id));
                                //List<BenefitNominees>newben=new List<BenefitNominees>();
                                //newben.Add(bns);
                                if (EmpData.FamilyDetails.Count() > 0)
                                {
                                    FamilyDetails.AddRange(EmpData.FamilyDetails);

                                }
                                EmpData.FamilyDetails = FamilyDetails;
                                db.Employee.Attach(EmpData);
                                db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(EmpData).State = System.Data.Entity.EntityState.Detached;
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", BenefitNominees, null, "BenefitNominees", null);
                                ts.Complete();
                                //  return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });   
                                Msg.Add("  Data Saved successfully  ");
                                //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                return Json(new Utility.JsonReturnClass { Id = bns.MemberName.Id, Val = bns.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/_EmployeeFamilyDetails.cshtml");
        }
	}
}