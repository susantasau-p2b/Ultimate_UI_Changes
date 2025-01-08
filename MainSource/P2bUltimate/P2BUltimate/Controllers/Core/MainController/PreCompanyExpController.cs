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

namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class PreCompanyExpController : Controller
    {
        //
        // GET: /PreCompanyExp/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/PrevCompExp/Index.cshtml");
            //  return View("~/Views/Core/MainViews/PreCompanyExp/Index.cshtml");
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
        public ActionResult GetContactDetLKDetails(List<int> SkipIds)
        {
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
                var list1 = db.Corporate.ToList().Select(e => e.ContactDetails);
                var list2 = fall.Except(list1);

                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }
        [HttpPost]
        public ActionResult GetAddressLKDetails(List<int> SkipIds)
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

                var list1 = db.Corporate.ToList().Select(e => e.Address);
                var list2 = fall.Except(list1);

                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        //private DataBaseContext db = new DataBaseContext();

        [HttpPost]
        public ActionResult Create(PrevCompExp c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string ExpDet = form["ExperienceDetailsList_DDL"] == "0" ? "" : form["ExperienceDetailsList_DDL"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);

                    Employee empdata;
                    if (Emp != null && Emp != 0)
                    {
                        empdata = db.Employee.Find(Emp);
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
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
                            c.CompAddress = val;
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

                    if (ExpDet != null && ExpDet != "")
                    {
                        int ExpId = Convert.ToInt32(ExpDet);
                        var val = db.LookupValue.Where(e => e.Id == ExpId).SingleOrDefault();
                        c.ExperienceDetails = val;
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

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            var EmpSocialInfo1 = db.Employee.Include(e => e.PreCompExp).Include(e => e.PreCompExp.Select(q => q.CompAddress))
                                                   .Include(e => e.PreCompExp.Select(q => q.ContactDetails))
                                                      .Where(e => e.Id != null).ToList();
                            foreach (var item in EmpSocialInfo1)
                            {
                                if (item.PreCompExp.Count != 0 && empdata.PreCompExp.Count != 0)
                                {
                                    int aid = item.PreCompExp.Select(a => a.Id).SingleOrDefault();
                                    int bid = empdata.PreCompExp.Select(a => a.Id).SingleOrDefault();

                                    if (aid == bid)
                                    {
                                        var v = empdata.EmpCode;
                                        Msg.Add("Record Alredy Exist For Employee Code=" + v);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }

                            }
                            if (c.FromDate > c.ToDate)
                            {
                                Msg.Add(" From Date Should be less than To Date.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { null, null, "Issue Date Should be less than Expiry Date.", JsonRequestBehavior.AllowGet });
                            }
                            DateTime current = DateTime.Now;
                            if (c.FromDate > current || c.ToDate > current)
                            {

                                Msg.Add(" From Date and To Date should be less than Current Date.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            PrevCompExp pce = new PrevCompExp()
                            {
                                CompName = c.CompName == null ? "" : c.CompName.Trim(),
                                CompAddress = c.CompAddress,
                                ContactDetails = c.ContactDetails,
                                FromDate = c.FromDate,
                                JoiningJobPosition = c.JoiningJobPosition,
                                LastDrawnSalary = c.LastDrawnSalary,
                                LeaveingJobPosition = c.LeaveingJobPosition,
                                Reason = c.Reason,
                                ToDate = c.ToDate,
                                SpecialAchievements = c.SpecialAchievements,
                                YrOfService = c.YrOfService,
                                DBTrack = c.DBTrack,
                                ExperienceDetails = c.ExperienceDetails
                            };
                            try
                            {
                                db.PrevCompExp.Add(pce);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                DT_PrevCompExp DT_Corp = (DT_PrevCompExp)rtn_Obj;
                                DT_Corp.CompAddress_Id = c.CompAddress == null ? 0 : c.CompAddress.Id;
                                DT_Corp.ContactDetails_Id = c.ContactDetails == null ? 0 : c.ContactDetails.Id;
                                db.Create(DT_Corp);
                                db.SaveChanges();


                                List<PrevCompExp> empVisaDetails = new List<PrevCompExp>();
                                empVisaDetails.Add(pce);
                                empdata.PreCompExp = empVisaDetails;
                                db.Employee.Attach(empdata);
                                db.Entry(empdata).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();



                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });      

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
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {


                    var prevcomp = db.Employee.Include(e => e.PreCompExp).Include(e => e.PreCompExp.Select(q => q.CompAddress))
                                   .Include(e => e.PreCompExp.Select(q => q.ContactDetails))
                                   .Where(e => e.Id == data).SingleOrDefault();
                    int AID = prevcomp.PreCompExp.Select(s => s.Id).SingleOrDefault();
                    PrevCompExp pce = db.PrevCompExp.Include(e => e.CompAddress)
                                                                 .Include(e => e.ContactDetails)
                                                                 .Where(e => e.Id == AID).SingleOrDefault();

                    Address add = pce.CompAddress;
                    ContactDetails conDet = pce.ContactDetails;
                    if (pce.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = pce.DBTrack.CreatedBy != null ? pce.DBTrack.CreatedBy : null,
                                CreatedOn = pce.DBTrack.CreatedOn != null ? pce.DBTrack.CreatedOn : null,
                                IsModified = pce.DBTrack.IsModified == true ? true : false
                            };
                            pce.DBTrack = dbT;
                            db.Entry(pce).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                            using (var context = new DataBaseContext())
                            {
                            }
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

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = pce.DBTrack.CreatedBy != null ? pce.DBTrack.CreatedBy : null,
                                    CreatedOn = pce.DBTrack.CreatedOn != null ? pce.DBTrack.CreatedOn : null,
                                    IsModified = pce.DBTrack.IsModified == true ? false : false,
                                    AuthorizedBy = SessionManager.UserName,
                                    AuthorizedOn = DateTime.Now
                                };


                                db.Entry(pce).State = System.Data.Entity.EntityState.Deleted;
                                await db.SaveChangesAsync();


                                using (var context = new DataBaseContext())
                                {
                                    pce.CompAddress = add;
                                    pce.ContactDetails = conDet;
                                }
                                ts.Complete();
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
            /// return new EmptyResult();
        }

        public ActionResult P2BGrid1(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<PrevCompExp> PrevCompExp = null;
                if (gp.IsAutho == true)
                {
                    PrevCompExp = db.PrevCompExp.Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    PrevCompExp = db.PrevCompExp.ToList();
                }

                IEnumerable<PrevCompExp> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = PrevCompExp;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.CompName, a.YrOfService }).Where((e => (e.Id.ToString() == gp.searchString) || (e.CompName.ToLower() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.CompName, a.YrOfService }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = PrevCompExp;
                    Func<PrevCompExp, int> orderfuc = (c =>
                                                               gp.sidx == "Id" ? c.Id : 0);
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.CompName), Convert.ToString(a.YrOfService) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.CompName), Convert.ToString(a.YrOfService) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.CompName, a.YrOfService }).ToList();
                    }
                    totalRecords = PrevCompExp.Count();
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
                IEnumerable<Employee> VisaDetails = null;
                if (gp.IsAutho == true)
                {
                    VisaDetails = db.Employee.Include(e => e.PreCompExp).Where(e => e.DBTrack.IsModified == true).ToList();
                }
                else
                {
                    VisaDetails = db.Employee.Include(q => q.PreCompExp).Include(q => q.EmpName).Where(q => q.PreCompExp.Count > 0).ToList();
                }

                IEnumerable<Employee> IE;

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = VisaDetails;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString.ToString()))  
                                || (e.EmpName.FullNameFML.ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString.ToString())))
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
                    IE = VisaDetails;
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
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = VisaDetails.Count();
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
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var prevcomp = db.Employee.Include(e => e.PreCompExp).Include(e => e.PreCompExp.Select(q => q.CompAddress))
                    .Include(e => e.PreCompExp.Select(q => q.ContactDetails))
                                    .Where(e => e.Id == data).SingleOrDefault();
                int AID = prevcomp.PreCompExp.Select(s => s.Id).SingleOrDefault();

                var Q = db.PrevCompExp
                    .Include(e => e.ContactDetails).Include(e => e.ExperienceDetails)
                    .Include(e => e.CompAddress)
                    .Where(e => e.Id == AID).Select
                    (e => new
                    {
                        CompName = e.CompName,
                        FromDate = e.FromDate,
                        JoiningJobPosition = e.JoiningJobPosition,
                        LastDrawnSalary = e.LastDrawnSalary,
                        ToDate = e.ToDate,
                        YrOfService = e.YrOfService,
                        LeaveingJobPosition = e.LeaveingJobPosition,
                        Reason = e.Reason,
                        SpecialAchievements = e.SpecialAchievements,
                        ExpDet_Id = e.ExperienceDetails != null ? e.ExperienceDetails.Id.ToString() : "",
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.PrevCompExp
                  .Include(e => e.ContactDetails)
                    .Include(e => e.CompAddress)
                    .Include(e => e.ContactDetails)
                    .Where(e => e.Id == AID)
                    .Select(e => new
                    {
                        Address_FullAddress = e.CompAddress.FullAddress == null ? "" : e.CompAddress.FullAddress,
                        Add_Id = e.CompAddress.Id == null ? "" : e.CompAddress.Id.ToString(),
                        Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                        FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
                        ExpDet_Id = e.ExperienceDetails == null ? "" : e.ExperienceDetails.Id.ToString(),
                        ExpDetails = e.ExperienceDetails == null ? "" : e.ExperienceDetails.LookupVal.ToString()
                    }).ToList();


                var W = db.DT_PrevCompExp
                     .Where(e => e.Orig_Id == AID && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         CompName = e.CompName,
                         FromDate = e.FromDate,
                         JoiningJobPosition = e.JoiningJobPosition,
                         LastDrawnSalary = e.LastDrawnSalary,
                         ToDate = e.ToDate,
                         YrOfService = e.YrOfService,
                         SpecialAchievements = e.SpecialAchievements,
                         Address_Val = e.CompAddress_Id == 0 ? "" : db.Address.Where(x => x.Id == e.CompAddress_Id).Select(x => x.FullAddress).FirstOrDefault(),
                         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault(),
                         ExpDet_Val = e.ExperienceDetails_Id == 0 ? "" : db.LookupValue.Where(x => x.Id == e.ExperienceDetails_Id).Select(x => x.LookupVal).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.PrevCompExp.Find(AID);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(PrevCompExp c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string ExpDet = form["ExperienceDetailsList_DDL"] == "0" ? "" : form["ExperienceDetailsList_DDL"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    var prevcomp = db.Employee.Include(e => e.PreCompExp).Include(e => e.PreCompExp.Select(q => q.CompAddress))
                                 .Include(e => e.PreCompExp.Select(q => q.ContactDetails))
                                                 .Where(e => e.Id == data).SingleOrDefault();
                    int AID = prevcomp.PreCompExp.Select(s => s.Id).SingleOrDefault();

                    //if (Addrs != null)
                    //{
                    //    if (Addrs != "")
                    //    {
                    //        int AddId = Convert.ToInt32(Addrs);
                    //        var val = db.Address.Include(e => e.Area)
                    //                            .Include(e => e.City)
                    //                            .Include(e => e.Country)
                    //                            .Include(e => e.District)
                    //                            .Include(e => e.State)
                    //                            .Include(e => e.StateRegion)
                    //                            .Include(e => e.Taluka)
                    //                            .Where(e => e.Id == AddId).SingleOrDefault();
                    //        c.CompAddress = val;
                    //    }
                    //}

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
                    if (Addrs != null)
                    {
                        if (Addrs != "")
                        {
                            var val = db.Address.Find(int.Parse(Addrs));
                            c.CompAddress = val;

                            var add = db.PrevCompExp.Include(e => e.CompAddress).Where(e => e.Id == AID).SingleOrDefault();
                            IList<PrevCompExp> addressdetails = null;
                            if (add.CompAddress != null)
                            {
                                addressdetails = db.PrevCompExp.Where(x => x.CompAddress.Id == add.CompAddress.Id && x.Id == AID).ToList();
                            }
                            else
                            {
                                addressdetails = db.PrevCompExp.Where(x => x.Id == AID).ToList();
                            }
                            if (addressdetails != null)
                            {
                                foreach (var s in addressdetails)
                                {
                                    s.CompAddress = c.CompAddress;
                                    db.PrevCompExp.Attach(s);
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
                        var addressdetails = db.PrevCompExp.Include(e => e.CompAddress).Where(x => x.Id == AID).ToList();
                        foreach (var s in addressdetails)
                        {
                            s.CompAddress = null;
                            db.PrevCompExp.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;

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

                            var add = db.PrevCompExp.Include(e => e.ContactDetails).Where(e => e.Id == AID).SingleOrDefault();
                            IList<PrevCompExp> contactsdetails = null;
                            if (add.ContactDetails != null)
                            {
                                contactsdetails = db.PrevCompExp.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == AID).ToList();
                            }
                            else
                            {
                                contactsdetails = db.PrevCompExp.Where(x => x.Id == AID).ToList();
                            }
                            foreach (var s in contactsdetails)
                            {
                                s.ContactDetails = c.ContactDetails;
                                db.PrevCompExp.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                    else
                    {
                        var contactsdetails = db.PrevCompExp.Include(e => e.ContactDetails).Where(x => x.Id == AID).ToList();
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = null;
                            db.PrevCompExp.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }

                    if (ContactDetails != null)
                    {
                       
                            var val = db.LookupValue.Find(int.Parse(ExpDet));
                            c.ExperienceDetails = val;

                            var add = db.PrevCompExp.Include(e => e.ExperienceDetails).Where(e => e.Id == AID).FirstOrDefault();
                            PrevCompExp expdetails = null;
                            if (add.ExperienceDetails != null)
                            {
                                expdetails = db.PrevCompExp.Where(x => x.ExperienceDetails.Id == add.ExperienceDetails.Id && x.Id == AID).FirstOrDefault();
                            }
                            else
                            {
                                expdetails = db.PrevCompExp.Where(x => x.Id == AID).FirstOrDefault();
                            }
                            expdetails.ExperienceDetails = c.ExperienceDetails;
                            db.PrevCompExp.Attach(expdetails);
                            db.Entry(expdetails).State = System.Data.Entity.EntityState.Modified;

                                db.SaveChanges();
                                TempData["RowVersion"] = expdetails.RowVersion;
                                db.Entry(expdetails).State = System.Data.Entity.EntityState.Detached;
                            
                        
                    }

                    if (Auth == false)
                    {

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    PrevCompExp blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.PrevCompExp.Where(e => e.Id == AID)
                                                                .Include(e => e.CompAddress)
                                                                .Include(e => e.ContactDetails).SingleOrDefault();
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

                                    //    int a = EditS(Addrs, ContactDetails, AID, c, c.DBTrack);
                                    var CurCorp = db.PrevCompExp.Find(AID);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                     //   c.DBTrack = dbT;
                                        PrevCompExp corp = new PrevCompExp()
                                        {
                                            CompName = c.CompName,
                                            FromDate = c.FromDate,
                                            JoiningJobPosition = c.JoiningJobPosition,
                                            LastDrawnSalary = c.LastDrawnSalary,
                                            LeaveingJobPosition = c.LeaveingJobPosition,
                                            Reason = c.Reason,
                                            SpecialAchievements = c.SpecialAchievements,
                                            ExperienceDetails = c.ExperienceDetails,
                                            ToDate = c.ToDate,
                                            YrOfService = c.YrOfService,
                                            Id = AID,
                                            DBTrack = c.DBTrack
                                        };


                                        db.PrevCompExp.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                      //  return 1;
                                    }
                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_PrevCompExp DT_Corp = (DT_PrevCompExp)obj;
                                        DT_Corp.CompAddress_Id = blog.CompAddress == null ? 0 : blog.CompAddress.Id;
                                        DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                        DT_Corp.ExperienceDetails_Id = blog.ExperienceDetails == null ? 0 : blog.ExperienceDetails.Id;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    //return Json(new Object[] { c.Id, c.CompName, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (PrevCompExp)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PrevCompExp)databaseEntry.ToObject();
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

                            PrevCompExp blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            PrevCompExp Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.PrevCompExp.Where(e => e.Id == AID).SingleOrDefault();
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
                            PrevCompExp corp = new PrevCompExp()
                            {
                                CompName = c.CompName,
                                FromDate = c.FromDate,
                                JoiningJobPosition = c.JoiningJobPosition,
                                LastDrawnSalary = c.LastDrawnSalary,
                                LeaveingJobPosition = c.LeaveingJobPosition,
                                Reason = c.Reason,
                                ToDate = c.ToDate,
                                YrOfService = c.YrOfService,
                                Id = AID,
                                DBTrack = c.DBTrack,
                                SpecialAchievements = c.SpecialAchievements,
                                ExperienceDetails = c.ExperienceDetails,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "PrevCompExp", c.DBTrack);

                                Old_Corp = context.PrevCompExp.Where(e => e.Id == AID)
                                    .Include(e => e.CompAddress).Include(e => e.ContactDetails).SingleOrDefault();
                                DT_PrevCompExp DT_Corp = (DT_PrevCompExp)obj;
                                DT_Corp.CompAddress_Id = DBTrackFile.ValCompare(Old_Corp.CompAddress, c.CompAddress);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                DT_Corp.ExperienceDetails_Id = DBTrackFile.ValCompare(Old_Corp.ExperienceDetails, c.ExperienceDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                            }
                            blog.DBTrack = c.DBTrack;
                            db.PrevCompExp.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, c.CompName, "Record Updated", JsonRequestBehavior.AllowGet });
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
                List<string> Msg = new List<string>();
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            PrevCompExp corp = db.PrevCompExp.Include(e => e.CompAddress)
                                .Include(e => e.ContactDetails)
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

                            db.PrevCompExp.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_PrevCompExp DT_Corp = (DT_PrevCompExp)rtn_Obj;
                            DT_Corp.CompAddress_Id = corp.CompAddress == null ? 0 : corp.CompAddress.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            DT_Corp.ExperienceDetails_Id = corp.ExperienceDetails == null ? 0 : corp.ExperienceDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        PrevCompExp Old_Corp = db.PrevCompExp
                                                          .Include(e => e.CompAddress)
                                                          .Include(e => e.ContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();



                        DT_PrevCompExp Curr_Corp = db.DT_PrevCompExp
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            PrevCompExp corp = new PrevCompExp();

                            string Addrs = Curr_Corp.CompAddress_Id == null ? null : Curr_Corp.CompAddress_Id.ToString();
                            string ContactDetails = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();
                            string ExpDetails = Curr_Corp.ExperienceDetails_Id == null ? null : Curr_Corp.ExperienceDetails_Id.ToString();
                            corp.CompName = Curr_Corp.CompName == null ? Old_Corp.CompName : Curr_Corp.CompName;

                            corp.FromDate = Curr_Corp.FromDate;
                            corp.JoiningJobPosition = Curr_Corp.JoiningJobPosition;
                            corp.LastDrawnSalary = Curr_Corp.LastDrawnSalary;
                            corp.LeaveingJobPosition = Curr_Corp.LeaveingJobPosition;
                            corp.Reason = Curr_Corp.Reason;
                            corp.ToDate = Curr_Corp.ToDate;
                            corp.YrOfService = Curr_Corp.YrOfService;
                            corp.SpecialAchievements = Curr_Corp.SpecialAchievements;
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

                                        int a = EditS(Addrs, ContactDetails, auth_id, corp, corp.DBTrack);
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (PrevCompExp)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (PrevCompExp)databaseEntry.ToObject();
                                        corp.RowVersion = databaseValues.RowVersion;
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
                            PrevCompExp corp = db.PrevCompExp.AsNoTracking().Include(e => e.CompAddress)
                                                                        .Include(e => e.ContactDetails).FirstOrDefault(e => e.Id == auth_id);

                            Address add = corp.CompAddress;
                            ContactDetails conDet = corp.ContactDetails;

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

                            db.PrevCompExp.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_PrevCompExp DT_Corp = (DT_PrevCompExp)rtn_Obj;
                            DT_Corp.CompAddress_Id = corp.CompAddress == null ? 0 : corp.CompAddress.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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

        public int EditS(string Addrs, string ContactDetails, int AID, PrevCompExp c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.Address.Find(int.Parse(Addrs));
                        c.CompAddress = val;

                        var add = db.PrevCompExp.Include(e => e.CompAddress).Where(e => e.Id == AID).SingleOrDefault();
                        IList<PrevCompExp> addressdetails = null;
                        if (add.CompAddress != null)
                        {
                            addressdetails = db.PrevCompExp.Where(x => x.CompAddress.Id == add.CompAddress.Id && x.Id == AID).ToList();
                        }
                        else
                        {
                            addressdetails = db.PrevCompExp.Where(x => x.Id == AID).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.CompAddress = c.CompAddress;
                                db.PrevCompExp.Attach(s);
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
                    var addressdetails = db.PrevCompExp.Include(e => e.CompAddress).Where(x => x.Id == AID).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.CompAddress = null;
                        db.PrevCompExp.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;

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

                        var add = db.PrevCompExp.Include(e => e.ContactDetails).Where(e => e.Id == AID).SingleOrDefault();
                        IList<PrevCompExp> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.PrevCompExp.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == AID).ToList();
                        }
                        else
                        {
                            contactsdetails = db.PrevCompExp.Where(x => x.Id == AID).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = c.ContactDetails;
                            db.PrevCompExp.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var contactsdetails = db.PrevCompExp.Include(e => e.ContactDetails).Where(x => x.Id == AID).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.PrevCompExp.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.PrevCompExp.Find(AID);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    PrevCompExp corp = new PrevCompExp()
                    {
                        CompName = c.CompName,
                        FromDate = c.FromDate,
                        JoiningJobPosition = c.JoiningJobPosition,
                        LastDrawnSalary = c.LastDrawnSalary,
                        LeaveingJobPosition = c.LeaveingJobPosition,
                        Reason = c.Reason,
                        SpecialAchievements = c.SpecialAchievements,
                        ToDate = c.ToDate,
                        YrOfService = c.YrOfService,
                        Id = AID,
                        DBTrack = c.DBTrack
                    };


                    db.PrevCompExp.Attach(corp);
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

    }
}