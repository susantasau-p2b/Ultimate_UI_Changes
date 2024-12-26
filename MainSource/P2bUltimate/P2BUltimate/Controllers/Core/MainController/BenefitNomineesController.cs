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
using P2BUltimate.Security;
using Payroll;
namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class BenefitNomineesController : Controller
    {
        //
        // GET: /BenefitNominees/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/BenefitNominees/Index.cshtml");
        }

        public ActionResult Createcontactdetails_partial()
        {
            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        }
        public ActionResult Createnomineesname_partial()
        {
            return View("~/Views/Shared/Core/_Nomines_benifits.cshtml");
        }


        //private DataBaseContext db = new DataBaseContext();

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
        //        IEnumerable<BenefitNominees> BenefitNominees = null;
        //        if (gp.IsAutho == true)
        //        {
        //           BenefitNominees = db.BenefitNominees.Include(e => e.Relation).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            BenefitNominees = db.BenefitNominees.Include(e => e.Relation).AsNoTracking().ToList();
        //        }

        //        IEnumerable<BenefitNominees> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = BenefitNominees;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id,a.DateofBirth,a.Relation }).Where((e => (e.Id.ToString() == gp.searchString))).ToList();

        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.DateofBirth, a.Relation != null ? Convert.ToString(a.Relation.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = BenefitNominees;
        //            Func<BenefitNominees, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c =>
        //                                 gp.sidx == "DateofBirth" ? Convert.ToString(c.DateofBirth) :
        //                                 gp.sidx == "Relation" ? c.Relation.LookupVal : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.DateofBirth), a.Relation != null ? Convert.ToString(a.Relation.LookupVal) : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.DateofBirth), a.Relation != null ? Convert.ToString(a.Relation.LookupVal) : "" }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.DateofBirth, a.Relation != null ? Convert.ToString(a.Relation.LookupVal) : "" }).ToList();
        //            }
        //            totalRecords = BenefitNominees.Count();
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
                Employee = db.Employee.Include(q => q.Nominees).Include(q => q.EmpName).ToList();
                if (gp.IsAutho == true)
                {
                    Employee = db.Employee.Include(q => q.Nominees).Include(q => q.EmpName).Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    // Employee = db.Employee.Where(a=>a.Id==db.EmpAcademicInfo.Contains()).ToList();
                    Employee = db.Employee.Include(q => q.Nominees).Where(q => q.Nominees.Count() > 0).ToList();
                }
                IEnumerable<Employee> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Employee;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString.ToString()))
                                                      || (e.EmpCode.ToString().Contains(gp.searchString.ToString())) || (e.EmpName.FullNameFML.ToUpper().Contains(gp.searchString.ToUpper())))
                                                  .Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Employee;
                    Func<Employee, string> orderfuc = (c => gp.sidx == "ID" ? c.Id.ToString() : "");



                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
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


        private MultiSelectList GetContactNos(List<int> selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<NomineeBenefit> Nos = new List<NomineeBenefit>();
                Nos = db.NomineeBenefit.ToList();
                return new MultiSelectList(Nos, "Id", "InchargeContactNos", selectedValues);
            }
        }

        [HttpPost]
        public ActionResult Create(BenefitNominees c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    //  string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    //     string benifit = form["BenefitTypelist"] == "0" ? "" : form["BenefitTypelist"];
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
                    //        var Lookup_val = db.NomineeBenefit.Find(ca);

                    //        lookupLang.Add(Lookup_val);
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
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "107").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(ActionName));//db.LookupValue.Find(int.Parse(Category));
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
                        EmpData = db.Employee.Include(e=>e.Nominees).Where(e=>e.Id==Emp).SingleOrDefault();
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

                  //          var EmpSocialInfo = db.Employee.Include(e => e.Nominees)
                  //                            .Include(e => e.Nominees.Select(lm => lm.ContactDetails))
                  //.Include(e => e.Nominees.Select(lm => lm.Address))
                  //.Include(e => e.Nominees.Select(lm => lm.Relation))
                  //.Include(e => e.Nominees.Select(lm => lm.NomineeName))
                  //.Include(e => e.Nominees.Select(lm => lm.BenefitList))
                  //.Include(e => e.Nominees.Select(lm => lm.BenefitList.Select(f => f.BenefitType)))
                  //                            .Where(e => e.Id != null).ToList();

                            //foreach (var item in EmpSocialInfo)
                            //{
                            //    if (item.Nominees.Count() > 0 && EmpData.Nominees.Count() > 0)
                            //    {
                            //        var chk = item.Nominees.Select(e => e.Id).SingleOrDefault();
                            //        var chk1 = EmpData.Nominees.Select(e => e.Id).SingleOrDefault();
                            //        if (chk == chk1)
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
                                DT_BenefitNominees DT_Corp = (DT_BenefitNominees)rtn_Obj;
                                DT_Corp.Address_Id = c.Address == null ? 0 : c.Address.Id;
                                //    DT_Corp.BenefitList_Id = c.BenefitList == null ? 0 : c.BenefitList.Id;
                                DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                DT_Corp.NomineeName_Id = c.NomineeName == null ? 0 : c.NomineeName.Id;
                                db.Create(DT_Corp);
                                db.SaveChanges();

                                //List<BenefitNominees>newben=new List<BenefitNominees>();
                                //newben.Add(bns);
                                if (EmpData != null)
                                {
                                    EmpData.Nominees.Add(bns);
                                    db.Employee.Attach(EmpData);
                                    db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(EmpData).State = System.Data.Entity.EntityState.Detached;
                                }
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", BenefitNominees, null, "BenefitNominees", null);
                                ts.Complete();
                                //  return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });   
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        //[HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{
        //    List<string> Msg = new List<string>();
        //    try
        //    {
        //        BenefitNominees corporates = db.BenefitNominees
        //        .Include(e => e.ContactDetails)
        //        .Include(e => e.Address)
        //        .Include(e => e.Relation)
        //        .Include(e=>e.NomineeName)
        //        .Include(e=>e.BenefitList)
        //        .Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();

        //        Address add = corporates.Address;
        //        ContactDetails conDet = corporates.ContactDetails;
        //        //NameSingle GName = corporates.GuarantorName ;
        //        //NameSingle Borrow = corporates.Borrower;
        //        //LookupValue val = corporates.GuarantorType;
        //        //LookupValue val1 = corporates.Profession;
        //        //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
        //        if (corporates.DBTrack.IsModified == true)
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {
        //                //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
        //                DBTrack dbT = new DBTrack
        //                {
        //                    Action = "D",
        //                    CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
        //                    CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
        //                    IsModified = corporates.DBTrack.IsModified == true ? true : false
        //                };
        //                corporates.DBTrack = dbT;
        //                db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
        //                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack);
        //                DT_GuarantorDetails DT_Corp = (DT_GuarantorDetails)rtn_Obj;
        //                DT_Corp.Address_Id = corporates.Address == null ? 0 : corporates.Address.Id;
        //                DT_Corp.GuarantorType_Id = corporates.GuarantorType == null ? 0 : corporates.GuarantorType.Id;
        //                DT_Corp.Profession_Id = corporates.Profession == null ? 0 : corporates.Profession.Id;
        //                DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
        //                DT_Corp.GuarantorName_Id = corporates.GuarantorName == null ? 0 : corporates.GuarantorName.Id;
        //                DT_Corp.Borrower_Id = corporates.Borrower == null ? 0 : corporates.Borrower.Id;
        //                db.Create(DT_Corp);
        //                // db.SaveChanges();
        //                //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
        //                await db.SaveChangesAsync();
        //                //using (var context = new DataBaseContext())
        //                //{
        //                //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
        //                //}
        //                ts.Complete();
        //                Msg.Add("  Data removed successfully.  ");
        //                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);




        //            }
        //        }
        //        else
        //        {
        //            // var selectedRegions = corporates.Regions;

        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {
        //                //if (selectedRegions != null)
        //                //{
        //                //    var corpRegion = new HashSet<int>(corporates.Regions.Select(e => e.Id));
        //                //    if (corpRegion.Count > 0)
        //                //    {
        //                //        return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
        //                //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
        //                //    }
        //                //}

        //                try
        //                {
        //                    DBTrack dbT = new DBTrack
        //                    {
        //                        Action = "D",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now,
        //                        CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
        //                        CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
        //                        IsModified = corporates.DBTrack.IsModified == true ? false : false//,
        //                        //AuthorizedBy = SessionManager.UserName,
        //                        //AuthorizedOn = DateTime.Now
        //                    };

        //                    // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

        //                    db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
        //                    var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
        //                    DT_GuarantorDetails DT_Corp = (DT_GuarantorDetails)rtn_Obj;
        //                    DT_Corp.Address_Id = add == null ? 0 : add.Id;
        //                    DT_Corp.GuarantorType_Id = val == null ? 0 : val.Id;
        //                    DT_Corp.Profession_Id = val1 == null ? 0 : val1.Id;
        //                    DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
        //                    DT_Corp.GuarantorName_Id = GName == null ? 0 : GName.Id;
        //                    DT_Corp.Borrower_Id = Borrow == null ? 0 : Borrow.Id;
        //                    db.Create(DT_Corp);

        //                    await db.SaveChangesAsync();


        //                    //using (var context = new DataBaseContext())
        //                    //{
        //                    //    corporates.Address = add;
        //                    //    corporates.ContactDetails = conDet;
        //                    //    corporates.BusinessType = val;
        //                    //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
        //                    //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
        //                    //}
        //                    ts.Complete();
        //                    // Msg.Add("  Data removed successfully.  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    Msg.Add("  Data removed successfully.  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }
        //                catch (RetryLimitExceededException /* dex */)
        //                {
        //                    //Log the error (uncomment dex variable name and add a line here to write a log.)
        //                    //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
        //                    //return RedirectToAction("Delete");
        //                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
        //                }
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
        //}



        /*------------------------------------------------- */

        public class NomineeBenefitC
        {
            public string id { get; set; }
            public string MobileNo { get; set; }
            public string LandlineNo { get; set; }
            public string contactdetailsnumbers { get; set; }

        }
        /*--------------------------------------------------*/
        public class benefitlists
        {
            public Array benefit_id { get; set; }
            public Array benefitlist_details { get; set; }
        }
        public class benefitlists1
        {
            public int Add_Id { get; set; }
            public string Address_FullAddress { get; set; }
            public int Cont_Id { get; set; }
            public string FullContactDetails { get; set; }
            public int name_Id { get; set; }
            public string name_FullDetails { get; set; }
        }
        //[HttpPost]
        //public ActionResult Edit1(int data)
        //{
        //    var ben = db.Employee.Include(e => e.Nominees)
        //       .Include(e => e.Nominees.ContactDetails)
        //       .Include(e => e.Nominees.Address)
        //       .Include(e => e.Nominees.Relation)
        //       .Include(e => e.Nominees.NomineeName)
        //       .Include(e => e.Nominees.BenefitList)
        //       .Include(e => e.Nominees.BenefitList.Select(f => f.BenefitType))
        //       .Include(e => e.Nominees.ContactDetails)
        //       .Where(e => e.Id == data).SingleOrDefault();
        //    int AID = ben.Nominees.Id;

        //    var Q = db.Employee.Include(e => e.Nominees)
        //        .Include(e => e.Nominees.ContactDetails)
        //        .Include(e => e.Nominees.Address)
        //        .Include(e => e.Nominees.Relation)
        //        .Include(e => e.Nominees.NomineeName)
        //        .Include(e => e.Nominees.BenefitList)
        //        .Include(e => e.Nominees.ContactDetails)
        //        .Where(e => e.Id == AID).Select
        //        (e => new
        //        {
        //            DateofBirth = e.Nominees.DateofBirth,
        //            Relations_Id = e.Nominees.Relation.Id == null ? 0 : e.Nominees.Relation.Id,
        //            Action = e.DBTrack.Action
        //        }).ToList();

        //    List<benefitlists> benifitnominee = new List<benefitlists>();
        //    var k = db.Employee.Include(e => e.Nominees).Include(e => e.Nominees.BenefitList).Where(e => e.Id == AID).ToList();
        //    foreach (var e in k)
        //    {
        //        benifitnominee.Add(new benefitlists
        //        {

        //            benefit_id = e.Nominees.BenefitList.Select(a => a.Id.ToString()).ToArray(),
        //            benefitlist_details = e.Nominees.BenefitList.Select(a => a.FullDetails).ToArray(),
        //        });
        //    }


        //    var add_data = db.Employee.Include(e => e.Nominees)
        //     .Include(e => e.Nominees.ContactDetails)
        //        .Include(e => e.Nominees.Address)
        //        .Include(e => e.Nominees.Address.Area)
        //        .Include(e => e.Nominees.Address.City)
        //        .Include(e => e.Nominees.Address.Country)
        //        .Include(e => e.Nominees.Address.District)
        //        .Include(e => e.Nominees.Address.State)
        //        .Include(e => e.Nominees.Address.StateRegion)
        //        .Include(e => e.Nominees.Address.Taluka)
        //        .Include(e => e.Nominees.Relation)
        //        .Include(e => e.Nominees.NomineeName)
        //        .Include(e => e.Nominees.BenefitList)
        //        .Include(e => e.Nominees.ContactDetails)
        //        .Where(e => e.Id == AID)
        //        .Select(e => new
        //        {
        //            Address_FullAddress = e.Nominees.Address.FullAddress == null ? "" : e.Nominees.Address.FullAddress,
        //            Add_Id = e.Nominees.Address.Id == null ? "" : e.Nominees.Address.Id.ToString(),
        //            Cont_Id = e.Nominees.ContactDetails.Id == null ? "" : e.Nominees.ContactDetails.Id.ToString(),
        //            FullContactDetails = e.Nominees.ContactDetails.FullContactDetails == null ? "" : e.Nominees.ContactDetails.FullContactDetails,
        //            name_Id = e.Nominees.NomineeName.Id == null ? "" : e.Nominees.NomineeName.Id.ToString(),
        //            name_FullDetails = e.Nominees.NomineeName.FullDetails == null ? "" : e.Nominees.NomineeName.FullDetails,
        //        }).ToList();


        //    var W = db.DT_BenefitNominees
        //         .Where(e => e.Orig_Id == AID && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
        //         (e => new
        //         {
        //             DT_Id = e.Id,
        //             DateofBirth = e.DateofBirth,

        //             BusinessType_Val = e.Relation_Id == 0 ? "" : db.LookupValue
        //                        .Where(x => x.Id == e.Relation_Id)
        //                        .Select(x => x.LookupVal).FirstOrDefault(),

        //             Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
        //             Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
        //         }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

        //    var Corp = db.BenefitNominees.Find(AID);
        //    TempData["RowVersion"] = Corp.RowVersion;
        //    var Auth = Corp.DBTrack.IsModified;
        //    return Json(new Object[] { Q, add_data, benifitnominee, W, Auth, JsonRequestBehavior.AllowGet });
        //}
        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string EmpCode { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }
        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.Employee
                        .Include(e => e.EmpName)
                        .Include(e => e.ServiceBookDates)
                        .Include(e => e.GeoStruct)
                        //.Include(e => e.Employee.FuncStruct)
                        //   .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct)
                        //  .Include(e => e.Employee.PayStruct.Grade)
                        .Where(e => e.ServiceBookDates.ServiceLastDate == null)
                        .ToList();
                    // for searchs
                    IEnumerable<Employee> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        // fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.EmpCode.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.EmpName.FullNameFML.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            //   || (e.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy").Contains(param.sSearch))
                            //   || (e.Employee.FuncStruct.Job.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            // || (e.Employee.PayStruct.Grade.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            //  || (e.Employee.GeoStruct.Location.LocationObj.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<Employee, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.EmpCode : "");
                    var sortcolumn = Request["sSortDir_0"];
                    if (sortcolumn == "asc")
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    else
                    {
                        fall = fall.OrderByDescending(orderfunc);
                    }
                    // Paging 
                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                EmpCode = item != null ? item.EmpCode : null,
                                Name = item != null && item.EmpName != null ? item.EmpName.FullNameFML : null,
                                //    JoiningDate = item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy") : null,
                                //    Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                                //   Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
                                // Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null,
                            });
                        }
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.EmpCode, };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
                }
                catch (Exception e)
                {
                    List<string> Msg = new List<string>();
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public class BenefitnomineesData //childgrid
        {
            public int Id { get; set; }
            public string FullDetails { get; set; }
            public string BenefitPerc { get; set; }
            public string BenefitType { get; set; }
            public string Relation { get; set; }
        }

        public ActionResult Get_BenefitNomineesData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.Employee
                        .Include(e => e.Nominees)
                        .Include(e => e.Nominees.Select(t => t.NomineeName))
                        .Include(e => e.Nominees.Select(t => t.Relation))
                        .Include(e => e.Nominees.Select(t => t.BenefitList))
                        .Include(e => e.Nominees.Select(t => t.BenefitList.Select(x => x.BenefitType)))
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data.Nominees != null)
                    {
                        List<BenefitnomineesData> returndata = new List<BenefitnomineesData>();

                        foreach (var item in db_data.Nominees.OrderByDescending(e => e.Id))
                        {
                            returndata.Add(new BenefitnomineesData
                            {
                                Id = item.Id,
                                FullDetails = item.FullDetails,
                                BenefitPerc = string.Join(",", item.BenefitList.Select(e => e.BenefitPerc).ToList()),
                                BenefitType =string.Join(",", item.BenefitList.Select(e => e.BenefitType.LookupVal).ToList()),
                                Relation = item.Relation.LookupVal
                            });
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {


                    List<benefitlists> benifitnominee = new List<benefitlists>();
                    List<benefitlists1> benifitnominee1 = new List<benefitlists1>();


                    var EmpSocialInfo = db.Employee.Include(e => e.Nominees)
                      .Include(e => e.Nominees.Select(lm => lm.ContactDetails))
                      .Include(e => e.Nominees.Select(lm => lm.Address))
                      .Include(e => e.Nominees.Select(lm => lm.Relation))
                      .Include(e => e.Nominees.Select(lm => lm.NomineeName))
                      .Include(e => e.Nominees.Select(lm => lm.BenefitList))
                      .Include(e => e.Nominees.Select(lm => lm.BenefitList.Select(f => f.BenefitType)))
                      .Where(e => e.Id == data).ToList();


                    var EmpSocialInfo1 = db.Employee.Include(e => e.Nominees)
                      .Include(e => e.Nominees.Select(lm => lm.ContactDetails))
                      .Include(e => e.Nominees.Select(lm => lm.Address))
                      .Include(e => e.Nominees.Select(lm => lm.Relation))
                      .Include(e => e.Nominees.Select(lm => lm.NomineeName))
                      .Include(e => e.Nominees.Select(lm => lm.BenefitList))
                      .Include(e => e.Nominees.Select(lm => lm.BenefitList.Select(f => f.BenefitType)))
                      .Where(e => e.Id == data).SingleOrDefault();
                    var AID = EmpSocialInfo1.Nominees.Select(e => e.Id).ToList();
                    // var datae=EmpSocialInfo.Select(q=>q.)
                    string DateofBirth = "";
                    int Relations_Id = 0;
                    string Action = "";
                    foreach (var item in EmpSocialInfo.Select(q => q.Nominees))
                    {
                        DateofBirth = item.Select(e => e.DateofBirth.Value.ToShortDateString()).SingleOrDefault();
                        Relations_Id = item.Select(e => e.Relation) == null ? 0 : item.Select(e => e.Relation.Id).SingleOrDefault();
                        Action = item.Select(q => q.DBTrack.Action).SingleOrDefault();
                    }
                    var r = (from ca in EmpSocialInfo.Select(q => q.Nominees)
                             select new
                             {
                                 DateofBirth = DateofBirth,
                                 Relations_Id = Relations_Id,
                                 Action = Action,

                             }).Distinct();

                    // var a = db.Employee.Include(e => e.EmpSocialInfo).Include(e => e.EmpSocialInfo.SocialActivities).Where(e => e.Id == data).Select(e => e.EmpSocialInfo.SocialActivities).ToList();
                    var aaa = db.Employee.Include(e => e.Nominees.Select(lm => lm.BenefitList)).Include(e => e.Nominees.Select(lm => lm.BenefitList.Select(b => b.BenefitType))).Where(e => e.Id == data).ToList();

                    foreach (var ca in aaa.Select(q => q.Nominees))
                    {
                        foreach (var ca1 in ca.Select(w => w.BenefitList))
                        {
                            // int benefit_id=  ca1.Select(q=>q.Id).SingleOrDefault();
                            // string benefitlist_details=ca1.Select(q=>q.FullDetails).SingleOrDefault();
                            benifitnominee.Add(
                        new benefitlists
                        {
                            benefit_id = ca1.Select(q => q.Id).ToArray(),
                            benefitlist_details = ca1.Select(q => q.FullDetails).ToArray()
                        });
                        }
                    }


                    var add_data = db.Employee.Include(e => e.Nominees)
                 .Include(e => e.Nominees.Select(lm => lm.ContactDetails))
                    .Include(e => e.Nominees.Select(lm => lm.Address))
                    .Include(e => e.Nominees.Select(lm => lm.Address.Area))
                    .Include(e => e.Nominees.Select(lm => lm.Address.City))
                    .Include(e => e.Nominees.Select(lm => lm.Address.Country))
                    .Include(e => e.Nominees.Select(lm => lm.Address.District))
                    .Include(e => e.Nominees.Select(lm => lm.Address.State))
                    .Include(e => e.Nominees.Select(lm => lm.Address.StateRegion))
                    .Include(e => e.Nominees.Select(lm => lm.Address.Taluka))
                    .Include(e => e.Nominees.Select(lm => lm.Relation))
                    .Include(e => e.Nominees.Select(lm => lm.NomineeName))
                    .Include(e => e.Nominees.Select(lm => lm.BenefitList))
                    .Where(e => e.Id == data).ToList();

                    //int Add_Id = 0;
                    //string Address_FullAddress = "";
                    //int Cont_Id = 0;
                    //string FullContactDetails = "";
                    //int name_Id = 0;
                    //string name_FullDetails = "";
                    foreach (var item in add_data.Select(q => q.Nominees))
                    {
                        benifitnominee1.Add(
                     new benefitlists1
                     {
                         Add_Id = item.Select(q => q.Address == null ? 0 : q.Address.Id).SingleOrDefault(),
                         Address_FullAddress = item.Select(q => q.Address == null ? "" : q.Address.FullAddress).SingleOrDefault(),

                         Cont_Id = item.Select(q => q.ContactDetails == null ? 0 : q.ContactDetails.Id).SingleOrDefault(),
                         FullContactDetails = item.Select(q => q.ContactDetails == null ? "" : q.ContactDetails.FullContactDetails).SingleOrDefault(),

                         name_Id = item.Select(q => q.NomineeName == null ? 0 : q.NomineeName.Id).SingleOrDefault(),
                         name_FullDetails = item.Select(q => q.NomineeName == null ? "" : q.NomineeName.FullNameFML).SingleOrDefault(),

                     });
                    }
                    //benifitnominee.Add(
                    //                new benefitlists
                    // {
                    //     Add_Id = Add_Id,
                    //     Address_FullAddress = Address_FullAddress,

                    //     Cont_Id = Cont_Id,
                    //     FullContactDetails = FullContactDetails,

                    //     name_Id = name_Id,
                    //     name_FullDetails = name_FullDetails,
                    // }).ToList();

                    var W = db.DT_EmpSocialInfo
                         .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                         (e => new
                         {
                             DT_Id = e.Id,
                             Category = e.Category_Id == 0 ? "" : db.LookupValue
                                        .Where(x => x.Id == e.Category_Id)
                                        .Select(x => x.LookupVal).FirstOrDefault(),
                             Religion = e.Religion_Id == 0 ? "" : db.LookupValue
                                        .Where(x => x.Id == e.Religion_Id)
                                        .Select(x => x.LookupVal).FirstOrDefault(),
                             Caste = e.Caste_Id == 0 ? "" : db.LookupValue
                                         .Where(x => x.Id == e.Caste_Id)
                                         .Select(x => x.LookupVal).FirstOrDefault(),
                             SubCaste = e.SubCaste_Id == 0 ? "" : db.LookupValue
                                        .Where(x => x.Id == e.SubCaste_Id)
                                        .Select(x => x.LookupVal).FirstOrDefault(),
                             SA_Val = e.SocialActivities_Id == 0 ? "" : db.SocialActivities.Where(x => x.Id == e.SocialActivities_Id).Select(x => x.FullDetails).FirstOrDefault(),

                         }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                    bool Auth = false;
                    foreach (var item in AID)
                    {
                        var LKup = db.BenefitNominees.Find(item);
                        TempData["RowVersion"] = LKup.RowVersion;
                        Auth = LKup.DBTrack.IsModified;

                    }
                    return this.Json(new Object[] { r, benifitnominee1, benifitnominee, W, Auth, JsonRequestBehavior.AllowGet });
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

        [HttpPost]
        public async Task<ActionResult> EditSave(BenefitNominees c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    string nominee = form["NomineeNamelist"] == "0" ? "" : form["NomineeNamelist"];
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string benefit = form["BenefitTypelist"] == "0" ? "" : form["BenefitTypelist"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];

                    //  string FacultyType = form["Categorylist"] == "0" ? "" : form["Categorylist"];
                    // string TrainingInstitute = form["TrainingInstitutelist"] == "0" ? "" : form["TrainingInstitutelist"];
                    // string ContactDetails = form["FacultySpecializationlist"] == "0" ? "" : form["FacultySpecializationlist"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    if (DateTime.Now < c.DateofBirth)
                    {
                        Msg.Add("Birth Should Not be Greater than Current Date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    var db_Data = db.Employee.Include(e => e.Nominees)
                                  .Include(e => e.Nominees.Select(lm => lm.ContactDetails))
                  .Include(e => e.Nominees.Select(lm => lm.Address))
                  .Include(e => e.Nominees.Select(lm => lm.Relation))
                  .Include(e => e.Nominees.Select(lm => lm.NomineeName))
                  .Include(e => e.Nominees.Select(lm => lm.BenefitList))
                  .Include(e => e.Nominees.Select(lm => lm.BenefitList.Select(f => f.BenefitType)))
                                 .Where(e => e.Id == data).SingleOrDefault();
                    int AID = db_Data.Nominees.Select(e => e.Id).SingleOrDefault();

                    BenefitNominees corporates = db.BenefitNominees
                   .Include(e => e.ContactDetails)
                   .Include(e => e.Address)
                   .Include(e => e.Relation)
                   .Include(e => e.NomineeName)
                   .Include(e => e.BenefitList)
                   .Include(e => e.ContactDetails).Where(e => e.Id == AID).SingleOrDefault();

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    //var db_Data = db.BenefitNominees.Include(e => e.Address).Include(e => e.ContactDetails).Include(e => e.BenefitList).Include(e => e.NomineeName).Include(e => e.Relation)
                    //     .Where(e => e.Id == data).SingleOrDefault();

                    if (Category != null && Category != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "107").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault();//db.LookupValue.Find(int.Parse(Category));
                        corporates.Relation = val;
                    }

                    List<NomineeBenefit> lookupLang = new List<NomineeBenefit>();
                    string Lang = form["BenefitTypelist"];

                    if (Lang != null)
                    {
                        var ids = Utility.StringIdsToListIds(Lang);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.NomineeBenefit.Find(ca);

                            lookupLang.Add(Lookup_val);
                            corporates.BenefitList = lookupLang;
                        }
                    }
                    else
                    {
                        corporates.BenefitList = null;
                    }


                    if (nominee != null && nominee != "")
                    {
                        int ContId = Convert.ToInt32(nominee);
                        var val = db.NameSingle.Where(e => e.Id == ContId).SingleOrDefault();
                        corporates.NomineeName = val;
                    }
                    if (Addrs != null && Addrs != "")
                    {
                        int ContId = Convert.ToInt32(Addrs);
                        var val = db.Address.Where(e => e.Id == ContId).SingleOrDefault();
                        corporates.Address = val;
                    }
                    if (ContactDetails != null && ContactDetails != "")
                    {
                        int ContId = Convert.ToInt32(ContactDetails);
                        var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
                        corporates.ContactDetails = val;
                    }

                    //   var dob = corporates.DateofBirth;
                    var bentype = corporates.BenefitList;
                    var nomname = corporates.NomineeName;
                    var addrs = corporates.Address;
                    var cntct = corporates.ContactDetails;
                    var reln = corporates.Relation;


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
                                        db.BenefitNominees.Attach(corporates);

                                        db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        TempData["RowVersion"] = corporates.RowVersion;
                                        db.Entry(corporates).State = System.Data.Entity.EntityState.Detached;

                                        var Curr_OBJ = db.BenefitNominees.Find(AID);
                                        TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                        db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                        {
                                            BenefitNominees blog = null; // to retrieve old data
                                            DbPropertyValues originalBlogValues = null;


                                            blog = context.BenefitNominees.Where(e => e.Id == AID).Include(e => e.Relation)
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
                                                Id = AID,
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
                                blog = context.BenefitNominees.Where(e => e.Id == AID).SingleOrDefault();
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
                                Id = AID,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "BenefitNominees", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.BenefitNominees.Where(e => e.Id == AID).Include(e => e.Relation)
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
        }

        public int EditS(string Category, string nominee, string Addrs, string ContactDetails, string benefit, int data, BenefitNominees c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
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
        }


        [HttpPost]
        public async Task<ActionResult> Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var EmpSocialInfo1 = db.BenefitNominees
                  .Include(e => e.BenefitList)
                  .Where(e => e.Id == data).SingleOrDefault();
                   // var AID = EmpSocialInfo1.Nominees.Select(e => e.Id).ToList();


                    // var EmpSocialInfo = db.EmpSocialInfo.Include(e=>e.Caste).Include(e=>e.Category).Include(e=>e.SubCaste).Include(e=>e.Religion).Include(e => e.SocialActivities).Where(e => e.Id == data).SingleOrDefault();

                    if (EmpSocialInfo1.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = EmpSocialInfo1.DBTrack.CreatedBy != null ? EmpSocialInfo1.DBTrack.CreatedBy : null,
                                CreatedOn = EmpSocialInfo1.DBTrack.CreatedOn != null ? EmpSocialInfo1.DBTrack.CreatedOn : null,
                                IsModified = EmpSocialInfo1.DBTrack.IsModified == true ? true : false
                            };
                            EmpSocialInfo1.DBTrack = dbT;
                            db.Entry(EmpSocialInfo1).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, EmpSocialInfo1.DBTrack);
                            DT_Nominees DT_OBJ = (DT_Nominees)rtn_Obj;
                            db.Create(DT_OBJ);

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

                            //  var v = EmpSocialInfo.Where(a => a.EmpSocialInfo.Id == EmpSocialInfo.Id).ToList();
                            //  db.Employee.RemoveRange(v);
                            // db.SaveChanges();


                            //var selectedValues = EmpSocialInfo1.BenefitList;
                            //var lkValue = new HashSet<int>(EmpSocialInfo1.Nominees.Select(q => q.BenefitList.Select(aq => aq.Id)).SingleOrDefault());
                            //if (lkValue.Count > 0)
                            //{
                            //    Msg.Add(" Child record exists.Cannot remove it..  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //    //return this.Json(new Object[] { "", "Child record exists.Cannot delete.", JsonRequestBehavior.AllowGet });
                            //}
                            db.Entry(EmpSocialInfo1).State = System.Data.Entity.EntityState.Deleted;


                            //if (v!=null)
                            //{
                            //Msg.Add("child record Exist in Employee Master");
                            //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //}
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
        //public async Task<ActionResult> EditSave(BenefitNominees c, int data, FormCollection form) // Edit submit
        //{
        //    string nominee = form["NomineeNamelist"] == "0" ? "" : form["NomineeNamelist"];
        //    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
        //    string benefit = form["BenefitTypelist"] == "0" ? "" : form["BenefitTypelist"];
        //    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
        //    string Category = form["Categorylist"] == "0" ? "" : form["Categorylist"];
        //    //  bool Auth = form["Autho_Action"] == "" ? false : true;
        //    bool Auth = form["Autho_Allow"] == "true" ? true : false;



        //    if (Category != null)
        //    {
        //        if (Category != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(Category));
        //            c.Relation = val;
        //        }
        //    }
        //    if (nominee != null)
        //    {
        //        if (nominee != "")
        //        {
        //            //  var val = db.LookupValue.Find(int.Parse(nominee));
        //            // c.NomineeName = val;
        //            int nomineeid = Convert.ToInt32(nominee);
        //            var val = db.NameSingle.Include(e => e.EmpTitle).Where(e => e.Id == nomineeid).SingleOrDefault();
        //            c.NomineeName = val;

        //        }
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

        //    if (benefit != null)
        //    {
        //        if (benefit != "")
        //        {
        //            // var val = db.LookupValue.Find(int.Parse(benefit));
        //            //c.BenefitList = val;
        //            int benefitid = Convert.ToInt32(benefit);
        //            var val = db.NomineeBenefit.Include(e => e.BenefitType).Where(e => e.Id == benefitid).ToList();
        //            c.BenefitList = val;

        //        }
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
        //                    BenefitNominees blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.BenefitNominees.Where(e => e.Id == data).Include(e => e.NomineeName)
        //                                                 .Include(e => e.BenefitList)
        //                                                .Include(e => e.Address)
        //                                                .Include(e => e.ContactDetails).Include(e => e.Relation).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }

        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    int a = EditS(Category, nominee, benefit, Addrs, ContactDetails, data, c, c.DBTrack);



        //                    using (var context = new DataBaseContext())
        //                    {
        //                        //c.Id = data;

        //                        /// DBTrackFile.DBTrackSave("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
        //                        //PropertyInfo[] fi = null;
        //                        //Dictionary<string, object> rt = new Dictionary<string, object>();
        //                        //fi = c.GetType().GetProperties();
        //                        ////foreach (var Prop in fi)
        //                        ////{
        //                        ////    if (Prop.Name != "Id" && Prop.Name != "DBTrack" && Prop.Name != "RowVersion")
        //                        ////    {
        //                        ////        rt.Add(Prop.Name, Prop.GetValue(c));
        //                        ////    }
        //                        ////}
        //                        //rt = blog.DetailedCompare(c);
        //                        //rt.Add("Orig_Id", c.Id);
        //                        //rt.Add("Action", "M");
        //                        //rt.Add("DBTrack", c.DBTrack);
        //                        //rt.Add("RowVersion", c.RowVersion);
        //                        //var aa = DBTrackFile.GetInstance("Core/P2b.Global", "DT_" + "Corporate", rt);
        //                        //DT_Corporate d = (DT_Corporate)aa;
        //                        //db.DT_Corporate.Add(d);
        //                        //db.SaveChanges();

        //                        //To save data in history table 
        //                        //var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
        //                        //DT_Corporate DT_Corp = (DT_Corporate)Obj;
        //                        //db.DT_Corporate.Add(DT_Corp);
        //                        //db.SaveChanges();\


        //                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                        DT_BenefitNominees DT_Corp = (DT_BenefitNominees)obj;
        //                        DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
        //                        DT_Corp.Relation_Id = blog.Relation == null ? 0 : blog.Relation.Id;
        //                        DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;

        //                        DT_Corp.NomineeName_Id = blog.NomineeName == null ? 0 : blog.NomineeName.Id;
        //                        // DT_Corp.BenefitList_Id = blog.BenefitList == null ? 0 : blog.BenefitList.Id;
        //                        db.Create(DT_Corp);
        //                        db.SaveChanges();
        //                    }
        //                    await db.SaveChangesAsync();
        //                    ts.Complete();


        //                    return Json(new Object[] { c.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

        //                }
        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (BenefitNominees)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                }
        //                else
        //                {
        //                    var databaseValues = (BenefitNominees)databaseEntry.ToObject();
        //                    c.RowVersion = databaseValues.RowVersion;

        //                }
        //            }

        //            return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    else
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {

        //            BenefitNominees blog = null; // to retrieve old data
        //            DbPropertyValues originalBlogValues = null;
        //            BenefitNominees Old_Corp = null;

        //            using (var context = new DataBaseContext())
        //            {
        //                blog = context.BenefitNominees.Where(e => e.Id == data).SingleOrDefault();
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

        //            BenefitNominees corp = new BenefitNominees()
        //            {

        //                Id = c.Id,
        //                FullDetails = c.FullDetails,


        //                //  Id = data,
        //                DBTrack = c.DBTrack,
        //                RowVersion = (Byte[])TempData["RowVersion"]
        //            };


        //            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //            using (var context = new DataBaseContext())
        //            {
        //                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "BenefitNominees", c.DBTrack);
        //                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);



        //                Old_Corp = context.BenefitNominees.Where(e => e.Id == data).Include(e => e.NomineeName)
        //                                                 .Include(e => e.BenefitList)
        //                                                .Include(e => e.Address)
        //                                                .Include(e => e.ContactDetails).Include(e => e.Relation).SingleOrDefault();
        //                DT_BenefitNominees DT_Corp = (DT_BenefitNominees)obj;
        //                DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
        //                DT_Corp.Relation_Id = blog.Relation == null ? 0 : blog.Relation.Id;
        //                DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;

        //                DT_Corp.NomineeName_Id = blog.NomineeName == null ? 0 : blog.NomineeName.Id;
        //                //    DT_Corp.BenefitList_Id = blog.BenefitList == null ? 0 : blog.BenefitList.Id;


        //                db.Create(DT_Corp);
        //                //db.SaveChanges();
        //            }
        //            blog.DBTrack = c.DBTrack;
        //            db.BenefitNominees.Attach(blog);
        //            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            db.SaveChanges();
        //            ts.Complete();
        //            return Json(new Object[] { blog.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
        //        }

        //    }
        //    return View();

        //}

        
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
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);

                    //var list1 = db.Corporate.ToList().Select(e => e.Address);
                    //var list2 = fall.Except(list1);

                    //var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    //return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullAddress }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult GetLookupBenefitNominees(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.NomineeBenefit.ToList();
        //        IEnumerable<NomineeBenefit> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.NomineeBenefit.ToList().Where(d => d.FullDetails.Contains(data));
        //        }
        //        else
        //        {
        //            var list1 = db.BenefitNominees.ToList();

        //            var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //public ActionResult GetLookupBenefitNominees(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.NomineeBenefit.ToList();
        //        IEnumerable<NomineeBenefit> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.NomineeBenefit.Include(d=>d.BenefitType).ToList().Where(d => d.FullDetails.Contains(data));
        //        }
        //        else
        //        {
        //            var list1 = db.NomineeBenefit.Include(d => d.BenefitType).ToList();

        //            var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult GetLookupBenefitNominees(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
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

    }
}