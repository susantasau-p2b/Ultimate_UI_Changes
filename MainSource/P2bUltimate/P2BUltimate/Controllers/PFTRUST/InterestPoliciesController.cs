using Attendance;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using IR;
using Microsoft.Ajax.Utilities;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using P2b.Global;
using P2B.PFTRUST;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;

namespace P2BUltimate.Controllers.PFTrust
{
    public class InterestPoliciesController : Controller
    {
        // GET: InterestPolicies
        public ActionResult Index()
        {
            return View("~/Views/PFTrust/InterestPolicies/Index.cshtml");

        }

        public ActionResult CreateInterestRate_Partial()
        {
            return View("~/Views/Shared/PFTrust/_InterestRate.cshtml");
        }
        public ActionResult CreateTdsmaster_Partial()
        {
            return View("~/Views/Shared/PFTrust/_TdsMaster.cshtml");
        }
        public class returnCalendarClass
        {
            public int Id { get; set; }
            public string FulDetails { get; set; }
        }
        public ActionResult PFCalenderDropdownlist(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                List<returnCalendarClass> returndata = new List<returnCalendarClass>();
                if (data2!="")
                {
                    int calid = Convert.ToInt32(data2);
                    var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "PFCalendar".ToUpper() && e.Id == calid).AsEnumerable().ToList();
                   
                    foreach (var item in qurey)
                    {
                        returndata.Add(new returnCalendarClass
                        {
                            Id = item.Id,
                            FulDetails = "From Date:" + " " + item.FromDate.Value.ToShortDateString() + "  ," + "To Date:" + " " + item.ToDate.Value.ToShortDateString()

                        });
                    }
                }
                else
                {
                 
                    var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "PFCalendar".ToUpper() ).AsEnumerable().ToList();
                   
                    foreach (var item in qurey)
                    {
                        returndata.Add(new returnCalendarClass
                        {
                            Id = item.Id,
                            FulDetails = "From Date:" + " " + item.FromDate.Value.ToShortDateString() + "  ," + "To Date:" + " " + item.ToDate.Value.ToShortDateString()

                        });
                    }
                }
             
                if (data2 != "" && data2 != "0")
                {
                    selected = data2;
                }
                if (returndata != null)
                {
                    s = new SelectList(returndata, "Id", "FulDetails", selected);
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public class returnClass
        {
            public string srno { get; set; }
            public string lookupvalue { get; set; }
        }

        [HttpPost]
        public ActionResult GetLookupDetailsCalendar(string data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Calendar.Include(e => e.Name)
                  .Where(e => e.Name.LookupVal.ToUpper() == "PFCALENDAR")
                  .ToList();


                IEnumerable<P2b.Global.Calendar> all;

                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Calendar.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }

                var result = (from singleRecord in all select new { singleRecord.Id, singleRecord.FullDetails }).Distinct();

                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }



        [HttpPost]
        public ActionResult GetLookupInterestRate(string data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {

                var fall = db.InterestRate.ToList();

                IEnumerable<InterestRate> all;

                if (!string.IsNullOrEmpty(data))
                {
                    all = db.InterestRate.ToList().Where(d => d.Name.Contains(data));
                }

                else
                {

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }

                var result = (from r in all select new { r.Id, r.FullDetails }).Distinct();

                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public ActionResult GetLookupTdsMaster(string data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {

                var fall = db.PFTTDSMaster.ToList();

                IEnumerable<PFTTDSMaster> all;

                if (!string.IsNullOrEmpty(data))
                {
                    //all = db.PFTTDSMaster.ToList().Where(d => d.Name.Contains(data));
                    all = db.PFTTDSMaster.Where(d => d.FullDetails.Contains(data)).ToList();
                }

                else
                {

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }

                var result = (from r in all select new { r.Id, r.FullDetails }).Distinct();

                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public ActionResult GetLookupEffectiveMonths(List<int> SkipIds)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.StatutoryEffectiveMonthsPFT.Include(e => e.EffectiveMonth).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.StatutoryEffectiveMonthsPFT.Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }
                var list1 = db.InterestPolicies.Include(e => e.StatutoryEffectiveMonthsPFT).SelectMany(e => e.StatutoryEffectiveMonthsPFT).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.EffectiveMonth.LookupVal }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }



        }

        //----------------------------------------------------------------------------------------------------------------------------------------

        [HttpPost]
        public ActionResult Create(InterestPolicies ip, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> list = new List<string>();
                try
                {

                    string calendar = form["PFTACCalendar"] == "0" ? "" : form["PFTACCalendar"];
                    string name = form["IntPolicyName"] == "0" ? "" : form["IntPolicyName"];
                    ip.IntPolicyName = name;
                     string SettlementProductmonthAdj = form["SettlementProductmonthAdj"] == "0" ? "0": form["SettlementProductmonthAdj"];
                     ip.SettlementProductmonthAdj = Convert.ToInt32(SettlementProductmonthAdj);
                    string interestRate = form["InterestList"] == "0" ? "" : form["InterestList"];
                    string effectiveMonths = form["EffectiveMonthsList"] == "0" ? "" : form["EffectiveMonthsList"];
                    string interestFrequency = form["InterestFrequency"] == "0" ? "" : form["InterestFrequency"];
                    string interestPostingtype = form["InterestPostingType"] == "0" ? "" : form["InterestPostingType"];
                    string IntPostingMethod = form["IntPostingMethod"] == "0" ? "" : form["IntPostingMethod"];

                    string IntCarryForward = form["IntCarryForward"] == "0" ? "0" : form["IntCarryForward"];
                    string IntMergePF = form["IntMergePF"] == "0" ? "0" : form["IntMergePF"];
                    ip.IsIntCarryForward = Convert.ToBoolean(IntCarryForward);
                    ip.IsIntMergePF = Convert.ToBoolean(IntMergePF);

                    string TDSMASTERList = form["TDSMASTERList"] == "0" ? "" : form["TDSMASTERList"];
                    int tdsmastid = Convert.ToInt32(TDSMASTERList);
                    int CalId = 0;
                    if (calendar != null)
                    {
                        var ids = Utility.StringIdsToListIds(calendar);

                        foreach (var item in ids)
                        {

                            int CalendarListid = Convert.ToInt32(item);

                            var CalendarId = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal == "PFCalendar" && e.Id == CalendarListid).FirstOrDefault();

                            // ip.PFTACCalendar.Id = val.Id;
                            CalId = CalendarId.Id;

                        }

                    }

                    var k = db.PFTACCalendar.Include(e => e.PFTCalendar).Include(e => e.InterestPolicies)
                 .Include(e => e.InterestPolicies.InterestRate)
                .Include(e => e.InterestPolicies.StatutoryEffectiveMonthsPFT)
                .Include(e => e.InterestPolicies.StatutoryEffectiveMonthsPFT.Select(x => x.EffectiveMonth))
                .Where(e => e.PFTCalendar.Id == CalId)
                .SingleOrDefault();
                    if (k != null)
                    {

                        if (k.InterestPolicies != null)
                        {
                            Msg.Add("This Calendar Record allready Exist.  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }

                    ip.InterestRate = null;
                    List<InterestRate> InterestObject = new List<InterestRate>();
                    string values = form["InterestList"];

                    if (values != null)
                    {
                        var ids = Utility.StringIdsToListIds(values);
                        foreach (var item in ids)
                        {
                            var InterestObject_val = db.InterestRate.Where(e => e.Id == item).SingleOrDefault();
                            InterestObject.Add(InterestObject_val);
                            ip.InterestRate = InterestObject;

                        }


                    }







                    ip.StatutoryEffectiveMonthsPFT = null;
                    List<StatutoryEffectiveMonthsPFT> EffectiveMonthsObject = new List<StatutoryEffectiveMonthsPFT>();
                    String fromValues = form["EffectiveMonthsList"];



                    if (fromValues != null)
                    {
                        var ids = Utility.StringIdsToListIds(fromValues);
                        foreach (var item in ids)
                        {
                            var EffectiveMonthsObject_val = db.StatutoryEffectiveMonthsPFT.Where(e => e.Id == item).SingleOrDefault();
                            EffectiveMonthsObject.Add(EffectiveMonthsObject_val);
                            ip.StatutoryEffectiveMonthsPFT = EffectiveMonthsObject;
                        }
                    }










                    if (interestFrequency != null && interestFrequency != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(interestFrequency));
                        ip.InterestFrequency = val;

                    }

                    if (interestPostingtype != null && interestPostingtype != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(interestPostingtype));
                        ip.InterestPostingType = val;
                    }
                    if (IntPostingMethod != null && IntPostingMethod != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(IntPostingMethod));
                        ip.IntPostingMethod = val;
                    }

                    if (!ModelState.IsValid)
                    {
                        var errors = ModelState.SelectMany(x => x.Value.Errors.Select(z => z.Exception));

                        // Breakpoint, Log or examine the list with Exceptions.

                    }

                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);

                    var Company = new CompanyPFTrust();
                    Company = db.CompanyPFTrust.Include(e => e.Company).Where(e => e.Company.Id == comp_Id).SingleOrDefault();


                    using (TransactionScope ts = new TransactionScope())
                    {

                        ip.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        InterestPolicies interestPolicy = new InterestPolicies()
                        {

                            IntPolicyName = ip.IntPolicyName,
                            SettlementProductmonthAdj=ip.SettlementProductmonthAdj,
                            //  PFTACCalendar = ip.PFTACCalendar,
                            InterestRate = ip.InterestRate,
                            InterestFrequency = ip.InterestFrequency,
                            StatutoryEffectiveMonthsPFT = ip.StatutoryEffectiveMonthsPFT,
                            InterestPostingType = ip.InterestPostingType,
                            IntPostingMethod=ip.IntPostingMethod,
                            DBTrack = ip.DBTrack,
                            IsIntCarryForward=ip.IsIntCarryForward,
                            IsIntMergePF=ip.IsIntMergePF
                        };

                        try
                        {
                            db.InterestPolicies.Add(interestPolicy);

                            db.SaveChanges();
                            PFTACCalendar pftacaledar = new PFTACCalendar()
                            {
                                InterestPolicies = interestPolicy,
                                CompanyPFTrust = db.CompanyPFTrust.Include(e => e.Company).Where(e => e.Company_Id == comp_Id).SingleOrDefault(),
                                PFTCalendar = db.Calendar.Include(e => e.Name).Where(y => y.Name.LookupVal.ToUpper() == "PFCALENDAR" && y.Id == CalId).SingleOrDefault(),
                                PFTTDSMaster=db.PFTTDSMaster.Where(e=>e.Id==tdsmastid).SingleOrDefault(),
                                DBTrack = ip.DBTrack
                            };
                            db.PFTACCalendar.Add(pftacaledar);
                            db.SaveChanges();

                            if (Company != null)
                            {
                                var Objjob = new List<InterestPolicies>();
                                Objjob.Add(interestPolicy);
                                Company.InterestPolicies = Objjob;
                                var aa = ValidateObj(Company);
                                // var aa = ValidateObj(Company);
                                // var aa = ValidateObj(Company);
                                if (aa.Count > 0)
                                {
                                    foreach (var item in aa)
                                    {

                                        Msg.Add("Company" + item);
                                    }
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                db.CompanyPFTrust.Attach(Company);
                                db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

                            }


                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                        }








                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = ip.Id });
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
        //----------------------------------------------------------------------------------------------------------------------------------------


        public List<string> ValidateObj(Object obj)
        {
            var errorList = new List<String>();
            var context = new ValidationContext(obj, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(obj, context, results, true);
            if (!isValid)
            {
                foreach (var validationResult in results)
                {
                    errorList.Add(validationResult.ErrorMessage);
                }
                return errorList;
            }
            else
            {
                return errorList;
            }
        }
        public class P2BGridData
        {
            public int Id { get; set; }
            public string IntPolicyName { get; set; }
            public string PFTACCalendar { get; set; }
            // public ICollection<InterestRate> InterestRate { get; set; }

            // public string InterestRate { get; set; }

            public string InterestFrequency_Id { get; set; }
            public string InterestPostingType { get; set; }

        }


        //======================================================================================================================================

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> interestList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;


                    var BindInterestList = db.PFTACCalendar.Include(e => e.PFTCalendar)
                        .Include(e => e.InterestPolicies)
                         .Include(e => e.InterestPolicies.InterestRate)
                         .Include(e => e.InterestPolicies.InterestFrequency)
                        .Include(e => e.InterestPolicies.InterestPostingType)
                        .Include(e => e.InterestPolicies.StatutoryEffectiveMonthsPFT)
                       .ToList();

                    //var BindInterestList = db.InterestPolicies.Include(e => e.InterestFrequency)
                    //                                           .Include(e => e.StatutoryEffectiveMonthsPFT)
                    //                                           // .Include(e => e.PFTACCalendar)
                    //                                            //.Include(e => e.InterestRate)

                    //                                            .Include(e => e.InterestPostingType).ToList();

                    foreach (var singleRecord in BindInterestList)
                    {
                        //var listInterestFrequency = singleRecord.InterestFrequency.ToString();
                        if (singleRecord != null)
                        {
                            //string ir = null;
                            //foreach (var item in list)
                            //{
                            //    ir = item.FullDetails.ToString();
                            //}
                            int lookupval = singleRecord.InterestPolicies.InterestFrequency.Id;
                            var IF = db.LookupValue.Find(lookupval);


                            view = new P2BGridData()
                            {



                                Id = singleRecord.PFTCalendar.Id,
                                IntPolicyName = singleRecord.InterestPolicies.IntPolicyName,
                                PFTACCalendar = singleRecord.PFTCalendar.FullDetails.ToString(),
                                //InterestRate = ir,

                                InterestFrequency_Id = IF.LookupVal.ToString(),
                                // InterestFrequency_Id = singleRecord.InterestFrequency.LookupVal,
                                //InterestRate = ir.Name.ToString(),
                                InterestPostingType = singleRecord.InterestPolicies.InterestPostingType.LookupVal


                            };

                            //var record = BindInterestList.Select(e => e.InterestRate).ToString();


                            model.Add(view);

                        }



                    }

                    interestList = model;

                    IEnumerable<P2BGridData> IL;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IL = interestList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IL.Where(e => (e.Id.ToString().Contains(gp.searchString))

                                || (e.IntPolicyName.ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.PFTACCalendar.ToString().Contains(gp.searchString))
                                || (e.InterestFrequency_Id.ToString().Contains(gp.searchString))
                                || (e.InterestPostingType.ToString().ToUpper().Contains(gp.searchString.ToUpper()))

                                ).Select(a => new Object[] { a.Id, a.IntPolicyName, a.PFTACCalendar, a.InterestFrequency_Id.ToString(), a.InterestPostingType.ToString() }).ToList();
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IL.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.IntPolicyName), Convert.ToString(a.PFTACCalendar), Convert.ToString(a.InterestFrequency_Id), Convert.ToString(a.InterestPostingType) }).ToList();
                        }
                        totalRecords = IL.Count();
                    }
                    else
                    {
                        IL = interestList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "IntPolicyName" ? c.IntPolicyName.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IL = IL.OrderBy(orderfuc);
                            jsonData = IL.Select(a => new Object[] { a.Id, Convert.ToString(a.IntPolicyName), Convert.ToString(a.PFTACCalendar), Convert.ToString(a.InterestFrequency_Id), Convert.ToString(a.InterestPostingType) }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IL = IL = IL.OrderByDescending(orderfuc);
                            jsonData = IL.Select(a => new Object[] { a.Id, Convert.ToString(a.IntPolicyName), Convert.ToString(a.PFTACCalendar), Convert.ToString(a.InterestFrequency_Id), Convert.ToString(a.InterestPostingType) }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IL.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.IntPolicyName), Convert.ToString(a.PFTACCalendar), Convert.ToString(a.InterestFrequency_Id), Convert.ToString(a.InterestPostingType) }).ToList();
                        }
                        totalRecords = interestList.Count();
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



        //======================================================================================================================================




        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    InterestPolicies intPolicies = db.InterestPolicies
                                                      .Include(e => e.InterestRate)
                                                      .Include(e => e.StatutoryEffectiveMonthsPFT)
                        //  .Include(e => e.PFTACCalendar)
                                                     .Where(e => e.Id == data).SingleOrDefault();


                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        db.Entry(intPolicies).State = System.Data.Entity.EntityState.Deleted;
                        await db.SaveChangesAsync();

                        ts.Complete();

                        Msg.Add("  Data removed.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    //if (intPolicies.DBTrack.IsModified == true)
                    //{
                    //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //    {
                    //        //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                    //        DBTrack dbT = new DBTrack
                    //        {
                    //            Action = "D",
                    //            CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                    //            CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                    //            IsModified = corporates.DBTrack.IsModified == true ? true : false
                    //        };

                    //        await db.SaveChangesAsync();

                    //        ts.Complete();
                    //        //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    //        Msg.Add("  Data removed.  ");
                    //        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    }
                    //}
                    //else
                    //{
                    // var selectedRegions = corporates.Regions;

                    //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //        {



                    //            DBTrack dbT = new DBTrack
                    //            {
                    //                Action = "D",
                    //                ModifiedBy = SessionManager.UserName,
                    //                ModifiedOn = DateTime.Now,
                    //                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                    //                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                    //                IsModified = corporates.DBTrack.IsModified == true ? false : false//,

                    //            };

                    //            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                    //            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;

                    //            await db.SaveChangesAsync();



                    //            ts.Complete();
                    //            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    //            Msg.Add("  Data removed.  ");
                    //            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //        }
                    //    }
                }
                catch (RetryLimitExceededException /* dex */)
                {

                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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



        //===================================================================================================================================

        public class returnEditClass
        {
            //public Array Calendar_Id { get; set; }
            //public Array Calendar_FullDetails { get; set; }

            public string Calendar_Id { get; set; }
            public string Calendar_FullDetails { get; set; }
            public string Interest_Id { get; set; }
            public string Interest_FullDetails { get; set; }
            public string EffectiveMonths_Id { get; set; }
            public string EffectiveMonths_FullDetails { get; set; }
        }



        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.PFTACCalendar.Include(e => e.PFTCalendar)
                    .Include(e=>e.PFTTDSMaster)
                    .Include(e => e.InterestPolicies)
                   .Include(e => e.InterestPolicies.InterestFrequency)
                   .Include(e => e.InterestPolicies.InterestPostingType)
                   .Include(e => e.InterestPolicies.IntPostingMethod)
                   .Include(e => e.PFTCalendar)
                   .Include(e => e.InterestPolicies.InterestRate)
                   .Include(e => e.InterestPolicies.StatutoryEffectiveMonthsPFT)
                    //.Include(e => e.PFTACCalendar)
                    //.Include(e => e.InterestRate)
                    //.Include(e => e.InterestFrequency)
                    //.Include(e => e.PTStatutoryEffectiveMonths)
                    //  .Include(e => e.InterestPostingType)
                      .Where(e => e.PFTCalendar.Id == data)
                      .Select(e => new
                      {

                          IntPolicyName = e.InterestPolicies.IntPolicyName == null ? "" : e.InterestPolicies.IntPolicyName,
                          SettlementProductmonthAdj = e.InterestPolicies.SettlementProductmonthAdj == null ? 0 : e.InterestPolicies.SettlementProductmonthAdj,
                          InterestFrequency = e.InterestPolicies.InterestFrequency == null ? null : e.InterestPolicies.InterestFrequency.Id.ToString(),
                          InterestPostingType = e.InterestPolicies.InterestPostingType == null ? "" : e.InterestPolicies.InterestPostingType.Id.ToString(),
                          Calendar_Id = e.PFTCalendar.Id == null ? "0" : e.PFTCalendar.Id.ToString(),
                          Calendar_FullDetails = e.PFTCalendar.FullDetails == null ? "" : e.PFTCalendar.FullDetails,
                          IntCarryForward=e.InterestPolicies.IsIntCarryForward,
                          IntMergePF=e.InterestPolicies.IsIntMergePF,
                          PFTTDS_Id = e.PFTTDSMaster.Id == null ? "" : e.PFTTDSMaster.Id.ToString(),
                          PFTTDS_FullDetails = e.PFTTDSMaster.FullDetails == null ? "" : e.PFTTDSMaster.FullDetails,
                          IntPostingMethod = e.InterestPolicies.IntPostingMethod == null ? "" : e.InterestPolicies.IntPostingMethod.Id.ToString(),
                      }).ToList();

                //var Q = db.InterestPolicies
                //    .Include(e => e.IntPolicyName)
                //     //.Include(e => e.PFTACCalendar)
                //     //.Include(e => e.InterestRate)
                //     //.Include(e => e.InterestFrequency)
                //     //.Include(e => e.PTStatutoryEffectiveMonths)
                //     .Include(e => e.InterestPostingType)
                //      .Where(e => e.Id == data)
                //      .Select(e => new
                //      {

                //          IntPolicyName = e.IntPolicyName == null ? "" : e.IntPolicyName,
                //          InterestFrequency = e.InterestFrequency == null ? null : e.InterestFrequency.Id.ToString(),
                //          InterestPostingType = e.InterestPostingType == null ? "" : e.InterestPostingType.Id.ToString()
                //          //Calendar_Id=e.PFTACCalendar.Id ==null ? "":e.PFTACCalendar.Id.ToString(),
                //          //Calendar_FullDetails=e.PFTACCalendar.FullDetails == null ? "" : e.PFTACCalendar.FullDetails
                //      }).ToList();





                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();

                var k = db.PFTACCalendar.Include(e => e.PFTCalendar).Include(e => e.InterestPolicies)
                     .Include(e => e.InterestPolicies.InterestRate)
                    .Include(e => e.InterestPolicies.StatutoryEffectiveMonthsPFT)
                    .Include(e => e.InterestPolicies.StatutoryEffectiveMonthsPFT.Select(x => x.EffectiveMonth))
                    .Where(e => e.PFTCalendar.Id == data)
                    .SingleOrDefault();



                //string cal_id = null;
                //string cal_details = null;
                //string cal_var = "";

                //foreach (var j in k)
                //{
                //    cal_id = j.PFTCalendar.Id.ToString();
                //    cal_details = j.PFTCalendar.FullDetails.ToString();

                //}
                //foreach (var item in cal_details)
                //{
                //    cal_var = cal_var + item;
                //}

                //oreturnEditClass.Add(new returnEditClass
                //{



                //    Calendar_Id = cal_id,

                //    Calendar_FullDetails = cal_var



                //});





                var interestObject = k.InterestPolicies.InterestRate.ToList();

                //var interestObject = db.InterestPolicies.Where(e => e.Id == data).Include(e => e.InterestRate)
                //                                           .ToList();


                foreach (var item in interestObject)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {

                        Interest_Id = item.Id.ToString(),
                        Interest_FullDetails = item.FullDetails.ToString()
                    });
                }

                var effectiveMonthsObject = k.InterestPolicies.StatutoryEffectiveMonthsPFT
                                                                .ToList();

                foreach (var item in effectiveMonthsObject)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        EffectiveMonths_Id = item.Id.ToString(),
                        EffectiveMonths_FullDetails = item.EffectiveMonth.LookupVal.ToUpper().ToString()
                        //EffectiveMonths_FullDetails = item.PTStatutoryEffectiveMonths.Select(f => f.EffectiveMonth.LookupVal.ToString()).ToArray()


                    }); ;
                }



                return Json(new Object[] { Q, oreturnEditClass, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }




        //===============================================================================================================================

        [HttpPost]
        public async Task<ActionResult> EditSave(InterestPolicies ip, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {


                    string _calendar = form["CalendarList"] == "0" ? "" : form["CalendarList"];
                    string _policyName = form["IntPolicyName"] == "0" ? "" : form["IntPolicyName"];
                    ip.IntPolicyName = _policyName;
                    string SettlementProductmonthAdj = form["SettlementProductmonthAdj"] == "0" ? "0" : form["SettlementProductmonthAdj"];
                    ip.SettlementProductmonthAdj = Convert.ToInt32(SettlementProductmonthAdj);
                    string _interestList = form["InterestList"] == "0" ? "" : form["InterestList"];
                    string _effectiveMonthsList = form["EffectiveMonthsList"] == "0" ? "" : form["EffectiveMonthsList"];
                    string _interestFrequency = form["InterestFrequency"] == "0" ? "" : form["InterestFrequency"];
                    string _interestPostingType = form["InterestPostingType"] == "0" ? "" : form["InterestPostingType"];
                    string _IntPostingMethod = form["IntPostingMethod"] == "0" ? "" : form["IntPostingMethod"];

                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);

                    string TDSMASTERList = form["TDSMASTERList"] == "0" ? "" : form["TDSMASTERList"];
                    int tdsmastid = Convert.ToInt32(TDSMASTERList);

                    string IntCarryForward = form["IntCarryForward"] == "0" ? "0" : form["IntCarryForward"];
                    string IntMergePF = form["IntMergePF"] == "0" ? "0" : form["IntMergePF"];
                    ip.IsIntCarryForward = Convert.ToBoolean(IntCarryForward);
                    ip.IsIntMergePF = Convert.ToBoolean(IntMergePF);

                    var k = db.PFTACCalendar.Include(e => e.PFTCalendar).Include(e => e.InterestPolicies)
                 .Include(e => e.InterestPolicies.InterestRate)
                .Include(e => e.InterestPolicies.StatutoryEffectiveMonthsPFT)
                .Include(e => e.InterestPolicies.StatutoryEffectiveMonthsPFT.Select(x => x.EffectiveMonth))
                .Where(e => e.PFTCalendar.Id == data)
                .SingleOrDefault();

                    int data1 = k.InterestPolicies.Id;


                    if (_interestPostingType != null)
                    {
                        if (_interestPostingType != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "511").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(_interestPostingType)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(BusinessCategory));
                            ip.InterestPostingType = val;

                            var type = db.InterestPolicies.Include(e => e.InterestPostingType).Where(e => e.Id == data1).SingleOrDefault();
                            IList<InterestPolicies> typedetails = null;
                            if (type.InterestPostingType != null)
                            {
                                typedetails = db.InterestPolicies.Where(x => x.InterestPostingType.Id == type.InterestPostingType.Id && x.Id == data1).ToList();
                            }
                            else
                            {
                                typedetails = db.InterestPolicies.Where(x => x.Id == data1).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.InterestPostingType = ip.InterestPostingType;
                                s.SettlementProductmonthAdj = ip.SettlementProductmonthAdj;
                                db.InterestPolicies.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                //TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var intPosting = db.InterestPolicies.Include(e => e.InterestPostingType).Where(x => x.Id == data1).ToList();
                            foreach (var s in intPosting)
                            {
                                s.InterestPostingType = null;
                                db.InterestPolicies.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                //TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }

                    if (_IntPostingMethod != null)
                    {
                        if (_IntPostingMethod != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "513").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(_IntPostingMethod)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(BusinessCategory));
                            ip.IntPostingMethod = val;

                            var type = db.InterestPolicies.Include(e => e.IntPostingMethod).Where(e => e.Id == data1).SingleOrDefault();
                            IList<InterestPolicies> typedetails = null;
                            if (type.IntPostingMethod != null)
                            {
                                typedetails = db.InterestPolicies.Where(x => x.IntPostingMethod.Id == type.IntPostingMethod.Id && x.Id == data1).ToList();
                            }
                            else
                            {
                                typedetails = db.InterestPolicies.Where(x => x.Id == data1).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.IntPostingMethod = ip.IntPostingMethod;
                                s.SettlementProductmonthAdj = ip.SettlementProductmonthAdj;
                                db.InterestPolicies.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                //TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var intPosting = db.InterestPolicies.Include(e => e.IntPostingMethod).Where(x => x.Id == data1).ToList();
                            foreach (var s in intPosting)
                            {
                                s.IntPostingMethod = null;
                                db.InterestPolicies.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                //TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }


                    if (_interestFrequency != null)
                    {
                        if (_interestFrequency != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "421").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(_interestFrequency)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(BusinessCategory));
                            ip.InterestFrequency = val;

                            var type = db.InterestPolicies.Include(e => e.InterestFrequency).Where(e => e.Id == data1).SingleOrDefault();
                            IList<InterestPolicies> typedetails = null;
                            if (type.InterestFrequency != null)
                            {
                                typedetails = db.InterestPolicies.Where(x => x.InterestFrequency.Id == type.InterestFrequency.Id && x.Id == data1).ToList();
                            }
                            else
                            {
                                typedetails = db.InterestPolicies.Where(x => x.Id == data1).ToList();
                            }
                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                            foreach (var s in typedetails)
                            {
                                s.InterestFrequency = ip.InterestFrequency;
                                s.SettlementProductmonthAdj = ip.SettlementProductmonthAdj;
                                db.InterestPolicies.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                //TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        else
                        {
                            var intFrequency = db.InterestPolicies.Include(e => e.InterestFrequency).Where(x => x.Id == data1).ToList();
                            foreach (var s in intFrequency)
                            {
                                s.InterestFrequency = null;
                                db.InterestPolicies.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                //TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }





                    var dbdata = db.InterestPolicies.Include(e => e.InterestRate).Where(e => e.Id == data1).SingleOrDefault();
                    List<InterestRate> lookupLang1 = new List<InterestRate>();

                    if (_interestList != null)
                    {
                        var ids = Utility.StringIdsToListIds(_interestList);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.InterestRate.Find(ca);

                            lookupLang1.Add(Lookup_val);
                            dbdata.InterestRate = lookupLang1;
                        }
                    }
                    else
                    {
                        dbdata.InterestRate = null;
                    }




                    //-------------------------------------OLD CODE----------------------------------------------//

                 //   var db_data1 = db.LookupValue.Find(int.Parse(_effectiveMonthsList));


                    var db_data = db.InterestPolicies.Include(e => e.StatutoryEffectiveMonthsPFT).Include(e => e.StatutoryEffectiveMonthsPFT.Select(y => y.EffectiveMonth)).Where(e => e.Id == data1).SingleOrDefault();
                    List<StatutoryEffectiveMonthsPFT> lookupLang2 = new List<StatutoryEffectiveMonthsPFT>();

                    if (_effectiveMonthsList != null)
                    {
                        var ids = Utility.StringIdsToListIds(_effectiveMonthsList);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.StatutoryEffectiveMonthsPFT.Where(e => e.Id == ca).SingleOrDefault();

                            lookupLang2.Add(Lookup_val);
                            db_data.StatutoryEffectiveMonthsPFT = lookupLang2;
                        }
                    }
                    else
                    {
                        db_data.StatutoryEffectiveMonthsPFT = null;
                    }

                    //--------------------------------OLD CODE-----------------------------------------//






                    try
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {




                            var s = db.InterestPolicies
                                // .Include(e => e.PFTACCalendar)
                                                        .Include(e => e.InterestRate)

                                                        .Include(e => e.StatutoryEffectiveMonthsPFT)

                                                        .Where(e => e.Id == data1).SingleOrDefault();

                            s.IntPolicyName = ip.IntPolicyName;
                            s.SettlementProductmonthAdj = ip.SettlementProductmonthAdj;
                            s.IsIntMergePF = ip.IsIntMergePF;
                            s.IsIntCarryForward = ip.IsIntCarryForward;
                            s.InterestRate = dbdata.InterestRate;
                            s.StatutoryEffectiveMonthsPFT = db_data.StatutoryEffectiveMonthsPFT;
                            s.Id = data1;
                            s.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            db.InterestPolicies.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                            db.SaveChanges();



                            var q = db.PFTACCalendar.Include(e => e.CompanyPFTrust).Include(e => e.PFTCalendar).Include(e => e.InterestPolicies)
                             .Include(e => e.InterestPolicies.InterestRate)
                            .Include(e => e.InterestPolicies.StatutoryEffectiveMonthsPFT)
                            .Include(e => e.InterestPolicies.StatutoryEffectiveMonthsPFT.Select(x => x.EffectiveMonth))
                            .Include(e=>e.PFTTDSMaster)
                            .Where(e => e.PFTCalendar.Id == data)
                            .SingleOrDefault();

                            //q.Id = data;
                            q.PFTTDSMaster = db.PFTTDSMaster.Where(e => e.Id == tdsmastid).SingleOrDefault();
                            q.PFTCalendar = k.PFTCalendar;
                            q.CompanyPFTrust = db.CompanyPFTrust.Include(e => e.Company).Where(e => e.Company_Id == comp_Id).SingleOrDefault();
                            q.InterestPolicies = s;
                            q.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            db.PFTACCalendar.Attach(q);
                            db.Entry(q).State = System.Data.Entity.EntityState.Modified;

                            db.SaveChanges();
                            // TempData["RowVersion"] = s.RowVersion;
                            //  db.Entry(s).State = System.Data.Entity.EntityState.Detached;

                            //var IntPolicy = db.InterestPolicies.Find(data1);
                            //// TempData["CurrRowVersion"] = CurCorp.RowVersion;
                            //db.Entry(IntPolicy).State = System.Data.Entity.EntityState.Detached;


                            //InterestPolicies interestPolicies = new InterestPolicies()
                            //{
                            //    IntPolicyName = ip.IntPolicyName,
                            //    // PFTACCalendar = ip.PFTACCalendar,
                            //    InterestRate = dbdata.InterestRate,
                            //   // InterestFrequency = ip.InterestFrequency,
                            //    // PTStatutoryEffectiveMonths= (ICollection<StatutoryEffectiveMonths>)db_data,
                            //    StatutoryEffectiveMonthsPFT = db_data.StatutoryEffectiveMonthsPFT,
                            //   // InterestPostingType = ip.InterestPostingType,
                            //    Id = data1,
                            //    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }

                            //};
                            //db.InterestPolicies.Attach(interestPolicies);
                            //db.Entry(interestPolicies).State = System.Data.Entity.EntityState.Modified;

                            //PFTACCalendar pftacaledar = new PFTACCalendar()
                            //{
                            //    Id=k.Id,
                            //    InterestPolicies = interestPolicies,
                            //    CompanyPFTrust = db.CompanyPFTrust.Include(e => e.Company).Where(e => e.Company_Id == comp_Id).SingleOrDefault(),
                            //    PFTCalendar = db.Calendar.Include(e => e.Name).Where(y => y.Name.LookupVal.ToUpper() == "PFCALENDAR" && y.Id == k.PFTCalendar.Id).SingleOrDefault(),
                            //    DBTrack = ip.DBTrack
                            //};
                            //db.PFTACCalendar.Attach(pftacaledar);
                            //db.Entry(pftacaledar).State = System.Data.Entity.EntityState.Modified;


                            //db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            // await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Record Updated  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);




                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var entry = ex.Entries.Single();
                        var clientValues = (InterestPolicies)entry.Entity;
                        var databaseEntry = entry.GetDatabaseValues();
                        if (databaseEntry == null)
                        {
                            Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {

                            // var databaseValues = ()databaseEntry.ToObject();
                            //c.RowVersion = databaseValues.RowVersion;

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
                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
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




        //===============================================================================================================================




        public ActionResult EditInterestRate_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = (from c in db.InterestRate
                         .Where(e => e.Id == data)
                         select new
                         {
                             Name = c.Name,
                             EffectiveFrom = c.EffectiveFrom,
                             EffectiveTo = c.EffectiveTo,
                             GovtEMPPFInt = c.GovtEMPPFInt,
                             GovtVPFInt = c.GovtVPFInt,
                             TrustEMPPFInt = c.TrustEMPPFInt,
                             TrustVPFInt = c.TrustVPFInt,

                             //DBTrack = c.DBTrack,
                             Id = data
                         }).ToList();










                return Json(new object[] { r }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EditTdsMaster_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = (from c in db.PFTTDSMaster
                         .Where(e => e.Id == data)
                         select new
                         {
                             TaxableAccountCelling=c.TaxableAccountCelling,
                             TDSRate = c.TDSRate,
                             IntOnIntTDSAppl = c.IsIntOnIntTDSAppl,
                             OwnerTDSAppl = c.IsOwnerTDSAppl,
                             OwnTDSAppl = c.IsOwnTDSAppl,
                             VPFTDSAppl = c.IsVPFTDSAppl,
                             
                             //DBTrack = c.DBTrack,
                             Id = data
                         }).ToList();










                return Json(new object[] { r }, JsonRequestBehavior.AllowGet);
            }
        }


        //==========================================================================================================================================


        //[HttpPost]
        //public async Task<ActionResult> EditSaveInterestRate(InterestRate ir, int data, FormCollection form) // Edit submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        try
        //        {




        //            string _policyName = form["Name"] == "0" ? "" : form["Name"];
        //            ir.Name = _policyName;



        //            string _EffectiveFrom = form["EffectiveFrom"] == "0" ? "" : form["EffectiveFrom"];
        //            var effFrom = Convert.ToDateTime(_EffectiveFrom);
        //            ir.EffectiveFrom = effFrom;


        //            string _EffectiveTo = form["EffectiveTo"] == "0" ? "" : form["EffectiveTo"];
        //            var effTo = Convert.ToDateTime(_EffectiveTo);
        //            ir.EffectiveTo = effTo;

        //            string _GovtEMPPFInt = form["GovtEMPPFInt"] == "0" ? "" : form["GovtEMPPFInt"];
        //            var gpf = Convert.ToInt32(_GovtEMPPFInt);
        //            ir.GovtEMPPFInt = gpf;


        //            string _GovtVPFInt = form["GovtVPFInt"] == "0" ? "" : form["GovtVPFInt"];
        //            var gvpf = Convert.ToInt32(_GovtVPFInt);
        //            ir.GovtVPFInt = gvpf;

        //            string _TrustEMPPFInt = form["TrustEMPPFInt"] == "0" ? "" : form["TrustEMPPFInt"];
        //            var tepf = Convert.ToInt32(_TrustEMPPFInt);
        //            ir.TrustEMPPFInt = tepf;


        //            string _TrustVPFInt = form["TrustVPFInt"] == "0" ? "" : form["TrustVPFInt"];
        //            var tvpf = Convert.ToInt32(_TrustVPFInt);
        //            ir.TrustVPFInt = tvpf;





        //            ir.DBTrack = new DBTrack { Action = "M", ModifiedBy = SessionManager.UserName, IsModified = true };

        //            try
        //            {

        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {




        //                    var m1 = db.InterestRate
        //                                            .Where(e => e.Id == data).ToList();




        //                    foreach (var s in m1)
        //                    {

        //                        db.InterestRate.Attach(s);
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;

        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = s.RowVersion;
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                    }
        //                    var IntRate = db.InterestRate.Find(data);
        //                    //TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                    db.Entry(IntRate).State = System.Data.Entity.EntityState.Detached;





        //                    InterestRate interestRate = new InterestRate()
        //                    {
        //                        Name = ir.Name,
        //                        EffectiveFrom = ir.EffectiveFrom,
        //                        EffectiveTo = ir.EffectiveTo,
        //                        GovtEMPPFInt = ir.GovtEMPPFInt,
        //                        GovtVPFInt = ir.GovtVPFInt,
        //                        TrustEMPPFInt = ir.TrustEMPPFInt,
        //                        TrustVPFInt = ir.TrustVPFInt,
        //                        DBTrack = ir.DBTrack,
        //                        Id = data

        //                    };
        //                    db.InterestRate.Attach(interestRate);
        //                    db.Entry(interestRate).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(interestRate).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    await db.SaveChangesAsync();
        //                    ts.Complete();
        //                    Msg.Add("  Record Updated  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);




        //                }
        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (InterestRate)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //                else
        //                {


        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                LogFile Logfile = new LogFile();
        //                ErrorLog Err = new ErrorLog()
        //                {
        //                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                    ExceptionMessage = ex.Message,
        //                    ExceptionStackTrace = ex.StackTrace,
        //                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                    LogTime = DateTime.Now
        //                };
        //                Logfile.CreateLogFile(Err);
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }
        //            Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
        //    }
        //}


        //=================================================================================================================================================================
        [HttpPost]

        public async Task<ActionResult> EditSaveInterestRate(InterestRate ir, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();

            string _policyName = form["Name"] == "0" ? "" : form["Name"];
            ir.Name = _policyName;



            string _EffectiveFrom = form["EffectiveFrom"] == "0" ? "" : form["EffectiveFrom"];
            var effFrom = Convert.ToDateTime(_EffectiveFrom);
            ir.EffectiveFrom = effFrom;


            string _EffectiveTo = form["EffectiveTo"] == "0" ? "" : form["EffectiveTo"];
            var effTo = Convert.ToDateTime(_EffectiveTo);
            ir.EffectiveTo = effTo;

            string _GovtEMPPFInt = form["GovtEMPPFInt"] == "0.0" ? "" : form["GovtEMPPFInt"];
            var gpf = Convert.ToDouble(_GovtEMPPFInt);
            ir.GovtEMPPFInt = gpf;


            string _GovtVPFInt = form["GovtVPFInt"] == "0.0" ? "" : form["GovtVPFInt"];
            var gvpf = Convert.ToDouble(_GovtVPFInt);
            ir.GovtVPFInt = gvpf;

            string _TrustEMPPFInt = form["TrustEMPPFInt"] == "0.0" ? "" : form["TrustEMPPFInt"];
            var tepf = Convert.ToDouble(_TrustEMPPFInt);
            ir.TrustEMPPFInt = tepf;


            string _TrustVPFInt = form["TrustVPFInt"] == "0.0" ? "" : form["TrustVPFInt"];
            var tvpf = Convert.ToDouble(_TrustVPFInt);
            ir.TrustVPFInt = tvpf;





            //  ir.DBTrack = new DBTrack { Action = "M", ModifiedBy = SessionManager.UserName, IsModified = true };







            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.InterestRate.Where(e => e.Id == data).FirstOrDefault();
                        List<InterestRate> InterestRateObject = new List<InterestRate>();


                        db.InterestRate.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;

                        InterestRate intRate = db.InterestRate.Find(data);
                        TempData["CurrRowVersion"] = intRate.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            ir.DBTrack = new DBTrack
                            {
                                CreatedBy = intRate.DBTrack.CreatedBy == null ? null : intRate.DBTrack.CreatedBy,
                                CreatedOn = intRate.DBTrack.CreatedOn == null ? null : intRate.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            intRate.Name = ir.Name;
                            intRate.EffectiveFrom = ir.EffectiveFrom;
                            intRate.EffectiveTo = ir.EffectiveTo;
                            intRate.GovtEMPPFInt = ir.GovtEMPPFInt;
                            intRate.GovtVPFInt = ir.GovtVPFInt;
                            intRate.TrustEMPPFInt = ir.TrustEMPPFInt;
                            intRate.TrustVPFInt = ir.TrustVPFInt;
                            intRate.Id = data;
                            intRate.DBTrack = ir.DBTrack;

                            // SalHead.LvHead = LvH;
                            db.Entry(intRate).State = System.Data.Entity.EntityState.Modified;
                            // db.SaveChanges();

                            //using (var context = new DataBaseContext())
                            //{
                            //SalaryHead blog = null; // to retrieve old data
                            //DbPropertyValues originalBlogValues = null;

                            //blog = db.SalaryHead.Where(e => e.Id == data).SingleOrDefault();

                            //originalBlogValues = db.Entry(blog).OriginalValues;
                            //db.ChangeTracker.DetectChanges();
                            //var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                            //DT_SalaryHead DT_Corp = (DT_SalaryHead)obj;
                            //DT_Corp.Type_Id = blog.Type == null ? 0 : blog.Type.Id;
                            //DT_Corp.Frequency_Id = blog.Frequency == null ? 0 : blog.Frequency.Id;
                            //DT_Corp.RoundingMethod_Id = blog.RoundingMethod == null ? 0 : blog.RoundingMethod.Id;
                            //DT_Corp.SalHeadOperationType_Id = blog.SalHeadOperationType == null ? 0 : blog.SalHeadOperationType.Id;
                            //DT_Corp.ProcessType_Id = blog.ProcessType == null ? 0 : blog.ProcessType.Id;


                            //db.Create(DT_Corp);
                            db.SaveChanges();
                        }
                        //}
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }
        //=================================================================================================================================================================

        [HttpPost]

        public async Task<ActionResult> EditSaveTdsMaster(PFTTDSMaster ir, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();

            string TDSRate = form["TDSRate"] == "0" ? "0" : form["TDSRate"];
            ir.TDSRate =Convert.ToDouble(TDSRate);
            string TaxableAccountCelling = form["TaxableAccountCelling"] == "0" ? "0" : form["TaxableAccountCelling"];
            ir.TaxableAccountCelling = Convert.ToDouble(TaxableAccountCelling);

            string IntOnIntTDSAppl = form["IntOnIntTDSAppl"] == "0" ? "0" : form["IntOnIntTDSAppl"];
            string OwnerTDSAppl = form["OwnerTDSAppl"] == "0" ? "0" : form["OwnerTDSAppl"];
            string OwnTDSAppl = form["OwnTDSAppl"] == "0" ? "0" : form["OwnTDSAppl"];
            string VPFTDSAppl = form["VPFTDSAppl"] == "0" ? "0" : form["VPFTDSAppl"];
            ir.IsIntOnIntTDSAppl = Convert.ToBoolean(IntOnIntTDSAppl);
            ir.IsOwnerTDSAppl = Convert.ToBoolean(OwnerTDSAppl);
            ir.IsOwnTDSAppl = Convert.ToBoolean(OwnTDSAppl);
            ir.IsVPFTDSAppl = Convert.ToBoolean(VPFTDSAppl);



            //  ir.DBTrack = new DBTrack { Action = "M", ModifiedBy = SessionManager.UserName, IsModified = true };







            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.PFTTDSMaster.Where(e => e.Id == data).FirstOrDefault();
                        List<PFTTDSMaster> PFTTDSMasterObject = new List<PFTTDSMaster>();


                        db.PFTTDSMaster.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;

                        PFTTDSMaster PFTTDS = db.PFTTDSMaster.Find(data);
                        TempData["CurrRowVersion"] = PFTTDS.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            ir.DBTrack = new DBTrack
                            {
                                CreatedBy = PFTTDS.DBTrack.CreatedBy == null ? null : PFTTDS.DBTrack.CreatedBy,
                                CreatedOn = PFTTDS.DBTrack.CreatedOn == null ? null : PFTTDS.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            PFTTDS.TDSRate = ir.TDSRate;
                            PFTTDS.IsIntOnIntTDSAppl = ir.IsIntOnIntTDSAppl;
                            PFTTDS.IsOwnerTDSAppl = ir.IsOwnerTDSAppl;
                            PFTTDS.IsOwnTDSAppl = ir.IsOwnTDSAppl;
                            PFTTDS.IsVPFTDSAppl = ir.IsVPFTDSAppl;
                            PFTTDS.TaxableAccountCelling = ir.TaxableAccountCelling;
                            PFTTDS.Id = data;
                            PFTTDS.DBTrack = ir.DBTrack;

                            // SalHead.LvHead = LvH;
                            db.Entry(PFTTDS).State = System.Data.Entity.EntityState.Modified;
                           


                            //db.Create(DT_Corp);
                            db.SaveChanges();
                        }
                        //}
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }

    }
}