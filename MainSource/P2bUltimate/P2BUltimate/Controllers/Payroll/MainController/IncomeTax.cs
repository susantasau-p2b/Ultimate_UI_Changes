using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Payroll;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class IncomeTaxController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/IncomeTax/Index.cshtml");
        }
        public ActionResult itsection_partial()
        {
            return View("~/Views/Shared/Payroll/_ITSection.cshtml");
        }

        public ActionResult tds_partial()
        {
            return View("~/Views/Shared/Payroll/Tds_master.cshtml");
        }
        public class IncomeTax_EditDetails
        {
            public Array ITSection_Id { get; set; }
            public Array ITSection_FullDetails { get; set; }
            public Array ITTDS_Id { get; set; }
            public Array ITTDS_FullDetails { get; set; }
            public Array ITQuarter_Id { get; set; }
            public Array ITQuarter_FullDetails { get; set; }
            public Array ITSignPers_Id { get; set; }
            public Array ITSignPers_FullDetails { get; set; }
        }

        public class P2BGridData
        {
            public int Id { get; set; }

            public string FullDetails { get; set; }

        }

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

                    IEnumerable<P2BGridData> IncomeTaxList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;


                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);

                    var BindCompList = db.CompanyPayroll.Include(e => e.IncomeTax).Include(e => e.IncomeTax.Select(t => t.FyCalendar))
                                       .Include(e => e.IncomeTax.Select(t => t.FyCalendar.Name)).
                                       Where(e => e.Company.Id == company_Id).ToList();

                    foreach (var z in BindCompList)
                    {
                        if (z.IncomeTax != null)
                        {

                            foreach (var E in z.IncomeTax)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = E.Id,
                                    FullDetails = E.FyCalendar != null ? Convert.ToString(E.FyCalendar.FullDetails) : ""

                                };
                                model.Add(view);

                            }
                        }

                    }

                    IncomeTaxList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = IncomeTaxList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            //if (gp.searchField == "Id")
                            jsonData = IE.Where(e => (e.FullDetails.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                ||  (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.FullDetails, a.Id }).ToList();
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = IncomeTaxList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "FullDetails" ? c.FullDetails.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.FullDetails, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.FullDetails, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.FullDetails, a.Id }).ToList();
                        }
                        totalRecords = IncomeTaxList.Count();
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

        public ActionResult Delete(int? data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                IncomeTax ITSection = db.IncomeTax.Find(data);
                try
                {
                    if (data != null)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.Entry(ITSection).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();
                        }
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { msg = "Data deleted." });
                    }
                    else
                    {
                        Msg.Add("  Data not deleted.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { msg = "Data not deleted." });
                    }

                }

                catch (DataException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
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

        public ActionResult GetLookupDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LookupValue.Where(w => w.IsActive == true).ToList();
                IEnumerable<LookupValue> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.LookupValue.ToList().Where(d => d.LookupVal.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { ca.Id, ca.LookupVal }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);

                }
                var result = (from c in all
                              select new { c.Id, c.LookupVal }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        public ActionResult GetLookupDetailsITSec(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection.Include(e => e.ITInvestments).Include(e => e.ITSectionList).Include(e => e.ITSectionListType).ToList();
                IEnumerable<ITSection> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ITSection.ToList().Where(d => d.FullDetails.ToString().Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookupDetailsITTDS(string data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITTDS.Include(e => e.Category).ToList();
                IEnumerable<ITTDS> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ITTDS.Include(e => e.Category).ToList().Where(d => d.FullDetails.ToString().Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "Category: " + ca.Category.LookupVal.ToString() + ", IncomeRangeFrom : " + ca.IncomeRangeFrom + ", IncomeRangeTo : " + ca.IncomeRangeTo + ", Percentage : " + ca.Percentage + ", Amount : " + ca.Amount }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult GetLookupDetailsITForm16Quarter(List<int> SkipIds)
        {
            //using (DataBaseContext db = new DataBaseContext())
            //{
            //    var fall = db.ITForm16Quarter.Include(e => e.QuarterName).ToList();
            //    IEnumerable<ITForm16Quarter> all;
            //    if (!string.IsNullOrEmpty(data))
            //    {
            //        all = db.ITForm16Quarter.ToList().Where(d => d.FullDetails.ToString().Contains(data));
            //    }
            //    else
            //    {
            //        var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
            //        return Json(r, JsonRequestBehavior.AllowGet);
            //    }
            //    var result = (from c in all
            //                  select new { c.Id, c.FullDetails }).Distinct();
            //    return Json(result, JsonRequestBehavior.AllowGet);
            //}

            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.ITForm16Quarter.Include(e => e.QuarterName).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ITForm16Quarter.ToList().Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList(); ;

                    }
                }

                var list1 = db.IncomeTax.ToList().Select(e => e.ITForm16Quarter);
                var list2 = fall.Except(fall);

                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookupDetailsSignPers(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITForm16SigningPerson.ToList();
                IEnumerable<ITForm16SigningPerson> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ITForm16SigningPerson.ToList().Where(d => d.FullDetails.ToString().Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(IncomeTax IT, FormCollection form)
        {

            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int comp_Id = Convert.ToInt32(Session["CompId"]);
                    var financialyear = form["FinancialYearList"];
                    var fy = Convert.ToInt32(financialyear);
                    var ITSectionlist = form["ITSectionlist"];
                    var tdslist = form["tdslist"];

                    List<ITSection> ObjITsection = new List<ITSection>();

                    if (ITSectionlist == null && tdslist == null)
                    {
                        Msg.Add(" Kindly fill atleast one field.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    var dba = db.IncomeTax.Include(e => e.FyCalendar).Any(e => e.FyCalendar.Id == fy);
                    if (dba == true)
                    {
                        Msg.Add(" Data with this financial year already exist.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (ITSectionlist != null && ITSectionlist != "")
                    {
                        var ids = Utility.StringIdsToListIds(ITSectionlist);
                        foreach (var ca in ids)
                        {
                            var value = db.ITSection.Find(ca);
                            ObjITsection.Add(value);
                            IT.ITSection = ObjITsection;
                        }

                    }

                    List<ITTDS> ObjITTDS = new List<ITTDS>();
                    if (tdslist != null && tdslist != "")
                    {
                        var ids = Utility.StringIdsToListIds(tdslist);
                        foreach (var ca in ids)
                        {
                            var value = db.ITTDS.Find(ca);
                            ObjITTDS.Add(value);
                            IT.ITTDS = ObjITTDS;
                        }
                    }
                    if (financialyear != null && financialyear != "")
                    {
                        var value = db.Calendar.Find(int.Parse(financialyear));
                        IT.FyCalendar = value;

                    }

                    var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == comp_Id).SingleOrDefault();
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            IT.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            IncomeTax it = new IncomeTax()
                            {
                                ITTDS = IT.ITTDS,
                                ITSection = IT.ITSection,
                                FyCalendar = IT.FyCalendar,
                                DBTrack = IT.DBTrack

                            };
                            try
                            {

                                db.IncomeTax.Add(it);
                                db.SaveChanges();
                                List<IncomeTax> incomtax = new List<IncomeTax>();
                                incomtax.Add(it);
                                if (companypayroll != null)
                                {
                                    companypayroll.IncomeTax = incomtax;
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;

                                }

                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] {"", "", "Data Created Successfully.", JsonRequestBehavior.AllowGet });

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("create", new { concurrencyError = true, id = IT.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                // return this.Json(new { msg = "Unable to edit. Try again, and if the problem persists contact your system administrator." });
                                Msg.Add(" Unable to edit. Try again, and if the problem persists, contact your system administrator");
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
                        //var errorMsg = sb.ToString();
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        // return this.Json(new { msg = errorMsg });
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


        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.IncomeTax
                    .Include(e => e.ITTDS)
                    .Include(e => e.ITSection)
                    .Include(e => e.FyCalendar)
                    .Include(e => e.FyCalendar.Name)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        FyCalendar_Id = e.FyCalendar.Id == null ? 0 : e.FyCalendar.Id,
                        FyCalendar_FullDetails = e.FyCalendar.FullDetails == null ? "" : e.FyCalendar.FullDetails,
                        Action = e.DBTrack.Action
                    }).ToList();

                List<IncomeTax_EditDetails> ObjITSectionEdit = new List<IncomeTax_EditDetails>();

                var ITSectionDet = db.IncomeTax.Include(e => e.ITSection).Include(e => e.ITSection.Select(r => r.ITSectionList))
                                    .Include(e => e.ITSection.Select(r => r.ITSectionListType)).Include(e => e.ITTDS.Select(r => r.Category)).Where(e => e.Id == data).ToList();
                if (ITSectionDet != null)
                {
                    foreach (var a in ITSectionDet.Select(e => e.ITSection))
                    {
                        ObjITSectionEdit.Add(new IncomeTax_EditDetails
                        {
                            ITSection_Id = a.Select(e => e.Id.ToString()).ToArray(),
                            ITSection_FullDetails = a.Select(e => e.FullDetails).ToArray()
                        });
                    }
                }

                var ITTDSDetails = db.IncomeTax.Include(e => e.ITTDS).Include(e => e.ITTDS.Select(r => r.Category)).Where(e => e.Id == data).Select(e => e.ITTDS).ToList();
                if (ITTDSDetails != null)
                {
                    foreach (var ca in ITTDSDetails)
                    {
                        ObjITSectionEdit.Add(new IncomeTax_EditDetails
                        {
                            ITTDS_Id = ca.Select(e => e.Id.ToString()).ToArray(),
                            ITTDS_FullDetails = ca.Select(e => "Category: " + e.Category.LookupVal.ToString() + ", IncomeRangeFrom : " + e.IncomeRangeFrom + ", IncomeRangeTo : " + e.IncomeRangeTo + ", Percentage : " + e.Percentage + ", Amount : " + e.Amount).ToArray()
                        });
                    }
                }

                var ITQuarterDetails = db.IncomeTax.Include(e => e.ITForm16Quarter).Include(e => e.ITForm16Quarter.Select(r => r.QuarterName))
                    .Where(e => e.Id == data).ToList();
                var ITQuarterDetails1 = ITQuarterDetails.Select(r => r.ITForm16Quarter);
                if (ITQuarterDetails1 != null)
                {
                    foreach (var ca in ITQuarterDetails1)
                    {
                        ObjITSectionEdit.Add(new IncomeTax_EditDetails
                        {
                            ITQuarter_Id = ca.Select(e => e.Id.ToString()).ToArray(),
                            ITQuarter_FullDetails = ca.Select(e => e.FullDetails).ToArray()
                        });
                    }
                }

                var ITSignPersDetails = db.IncomeTax.Include(e => e.ITForm16SigningPerson).Where(e => e.Id == data).Select(e => e.ITForm16SigningPerson).ToList();
                if (ITSignPersDetails != null)
                {
                    foreach (var ca in ITSignPersDetails)
                    {
                        ObjITSectionEdit.Add(new IncomeTax_EditDetails
                        {
                            ITSignPers_Id = ca.Select(e => e.Id.ToString()).ToArray(),
                            ITSignPers_FullDetails = ca.Select(e => e.FullDetails).ToArray()
                        });
                    }
                }

                var Corp = db.IncomeTax.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, ObjITSectionEdit, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(IncomeTax IT, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var ITSectionlist = form["ITSectionlist"];
                    var tdslist = form["tdslist"];
                    var SignPerslist = form["SignPerslist"];
                    var Quarterlist = form["Quarterlist"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    IncomeTax blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.IncomeTax.Where(e => e.Id == data)
                                                                .Include(e => e.ITSection)
                                                                .Include(e => e.ITTDS)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    IT.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };


                                    List<ITSection> ObjITSection = new List<ITSection>();

                                    IncomeTax ITdetails = null;
                                    ITdetails = db.IncomeTax.Include(e => e.ITSection).Where(e => e.Id == data).SingleOrDefault();
                                    if (ITSectionlist != null && ITSectionlist != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(ITSectionlist);
                                        foreach (var ca in ids)
                                        {
                                            var ITSeclist = db.ITSection.Find(ca);
                                            ObjITSection.Add(ITSeclist);
                                            ITdetails.ITSection = ObjITSection;
                                        }
                                    }
                                    else
                                    {
                                        ITdetails.ITSection = null;
                                    }

                                    List<ITTDS> ObjITTDS = new List<ITTDS>();

                                    ITdetails = db.IncomeTax.Include(e => e.ITTDS).Where(e => e.Id == data).SingleOrDefault();
                                    if (tdslist != null && tdslist != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(tdslist);
                                        foreach (var ca in ids)
                                        {
                                            var ITTDSlist = db.ITTDS.Find(ca);
                                            ObjITTDS.Add(ITTDSlist);
                                            ITdetails.ITTDS = ObjITTDS;
                                        }
                                    }
                                    else
                                    {
                                        ITdetails.ITTDS = null;
                                    }

                                    List<ITForm16Quarter> ObjITQuarter = new List<ITForm16Quarter>();

                                    ITdetails = db.IncomeTax.Include(e => e.ITForm16Quarter).Where(e => e.Id == data).SingleOrDefault();
                                    if (Quarterlist != null && Quarterlist != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(Quarterlist);
                                        foreach (var ca in ids)
                                        {
                                            var ITQuarterlist = db.ITForm16Quarter.Find(ca);
                                            ObjITQuarter.Add(ITQuarterlist);
                                            ITdetails.ITForm16Quarter = ObjITQuarter;
                                        }
                                    }
                                    else
                                    {
                                        ITdetails.ITForm16Quarter = null;
                                    }

                                    List<ITForm16SigningPerson> ObjSignPers = new List<ITForm16SigningPerson>();

                                    ITdetails = db.IncomeTax.Include(e => e.ITForm16SigningPerson).Where(e => e.Id == data).SingleOrDefault();
                                    if (SignPerslist != null && SignPerslist != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(SignPerslist);
                                        foreach (var ca in ids)
                                        {
                                            var ITSignPerslist = db.ITForm16SigningPerson.Find(ca);
                                            ObjSignPers.Add(ITSignPerslist);
                                            ITdetails.ITForm16SigningPerson = ObjSignPers;
                                        }
                                    }
                                    else
                                    {
                                        ITdetails.ITForm16SigningPerson = null;
                                    }
                                    //var CurCorp = db.IncomeTax.Find(data);
                                    //TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;

                                    db.IncomeTax.Attach(ITdetails);
                                    db.Entry(ITdetails).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = ITdetails.RowVersion;
                                    db.Entry(ITdetails).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        IncomeTax Incometax = new IncomeTax()
                                        {

                                            Id = data,
                                            DBTrack = IT.DBTrack
                                        };
                                        db.IncomeTax.Attach(Incometax);
                                        db.Entry(Incometax).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(Incometax).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }
                                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, IT.DBTrack);
                                        //DT_IncomeTax DT_Corp = (DT_IncomeTax)obj;
                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = IT.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { , "", "Record Updated", JsonRequestBehavior.AllowGet });

                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (IncomeTax)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (IncomeTax)databaseEntry.ToObject();
                                    IT.RowVersion = databaseValues.RowVersion;

                                }
                            }

                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            IncomeTax blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            IncomeTax Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.IncomeTax.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            IT.DBTrack = new DBTrack
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

                            IncomeTax Incometax = new IncomeTax()
                            {

                                Id = data,
                                DBTrack = IT.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, Incometax, "IncomeTax", IT.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.IncomeTax.Where(e => e.Id == data)
                                    .Include(e => e.ITSection).Include(e => e.ITTDS)
                                    .SingleOrDefault();
                                DT_IncomeTax DT_Corp = (DT_IncomeTax)obj;
                                db.Create(DT_Corp);
                                db.SaveChanges();
                            }
                            blog.DBTrack = IT.DBTrack;
                            db.IncomeTax.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { blog.Id,"", "Record Updated", JsonRequestBehavior.AllowGet });
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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    IncomeTax OBJIncomeTax = db.IncomeTax.Include(e => e.ITTDS).Include(e => e.ITSection)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (OBJIncomeTax.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = OBJIncomeTax.DBTrack.CreatedBy != null ? OBJIncomeTax.DBTrack.CreatedBy : null,
                                CreatedOn = OBJIncomeTax.DBTrack.CreatedOn != null ? OBJIncomeTax.DBTrack.CreatedOn : null,
                                IsModified = OBJIncomeTax.DBTrack.IsModified == true ? true : false
                            };
                            OBJIncomeTax.DBTrack = dbT;
                            db.Entry(OBJIncomeTax).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, OBJIncomeTax.DBTrack);
                            DT_IncomeTax DT_Corp = (DT_IncomeTax)rtn_Obj;

                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        var ITSection = OBJIncomeTax.ITSection;
                        var ITTDS = OBJIncomeTax.ITTDS;


                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (ITSection != null && ITTDS != null)
                            {
                                var ObjItsection = new HashSet<int>(OBJIncomeTax.ITSection.Select(e => e.Id));
                                var ObjITTDS = new HashSet<int>(OBJIncomeTax.ITTDS.Select(e => e.Id));
                                if (ObjItsection.Count > 0 && ObjITTDS.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                }
                            }
                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = OBJIncomeTax.DBTrack.CreatedBy != null ? OBJIncomeTax.DBTrack.CreatedBy : null,
                                    CreatedOn = OBJIncomeTax.DBTrack.CreatedOn != null ? OBJIncomeTax.DBTrack.CreatedOn : null,
                                    IsModified = OBJIncomeTax.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };
                                db.Entry(OBJIncomeTax).State = System.Data.Entity.EntityState.Deleted;
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, OBJIncomeTax.DBTrack);
                                //DT_IncomeTax DT_Corp = (DT_IncomeTax)rtn_Obj;

                                //db.Create(DT_Corp);
                                await db.SaveChangesAsync();
                                //using (var context = new DataBaseContext())
                                //{
                                //    context.Database.ExecuteSqlCommand(
                                // "dbcc checkident('[dbo].[LvNewReq]',reseed,0)"); 
                                //}
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
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
        }
    }
}
