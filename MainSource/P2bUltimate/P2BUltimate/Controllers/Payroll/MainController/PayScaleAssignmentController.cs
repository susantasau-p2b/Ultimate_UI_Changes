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
using Payroll;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class PayScaleAssignmentController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /PayScaleAssignment/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/PayScaleAssignment/Index.cshtml");
        }

        public class returndataclass
        {
            public string code { get; set; }
            public string value { get; set; }
        }

        public ActionResult GetPayScaleAgreement(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.PayScaleAgreement.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.PayScaleAgreement.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);

                //////using (DataBaseContext db = new DataBaseContext())
                //////{
                //////    int CompId = 0;
                //////    if (!String.IsNullOrEmpty(SessionManager.UserName))
                //////    {
                //////        CompId = int.Parse(SessionManager.UserName.ToString());
                //////    }

                //////    var fall = db.PayScaleAgreement.Include(e => e.PayScale).ToList();

                //////   // var fall = db.Company.Include(e => e.PayScaleAgreement.Select(r => r.PayScale)).Select(e => e.PayScaleAgreement).ToList();

                //////    IEnumerable<PayScaleAgreement> all;
                //////    if (!string.IsNullOrEmpty(data))
                //////    {
                //////        all = db.PayScaleAgreement.ToList().Where(d => d.FullDetails.Contains(data));

                //////    }
                //////    else
                //////    {

                //////        var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                //////        return Json(r, JsonRequestBehavior.AllowGet);
                //////    }
                //////    var result = (from c in all
                //////                  select new { c.Id, c.FullDetails }).Distinct();
                //////    return Json(result, JsonRequestBehavior.AllowGet);
                //////}
                // return View();
            }
        }



        public ActionResult PopulateSalHeadDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = int.Parse(Session["CompId"].ToString());
                var query1 = db.CompanyPayroll.Include(x => x.SalaryHead).Where(e=>e.Id==id).FirstOrDefault();
                var query = query1.SalaryHead.OrderBy(e => e.Code).ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetPayScaleLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.PayScale.Include(e => e.PayScaleArea).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.PayScale.Include(e => e.PayScaleArea).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetSalHeadFormulaLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var fall = db.SalHeadFormula.Distinct().GroupBy(x => new { x.Name, x.FullDetails, x.Id })
                //                         .Select(g => new
                //                         {
                //                             g.Key.Name,
                //                             g.Key.FullDetails,
                //                             g.Key.Id
                //                         }).ToList();
                var fall = db.SalHeadFormula.GroupBy(x => x.Name).Select(y => y.FirstOrDefault()).ToList();


                if (SkipIds != null)
                {
                    foreach (int a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.SalHeadFormula
                                        .Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }
                var result = (from c in fall
                              select new { srno = c.Id, lookupvalue = c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }

        

        //public ActionResult GetPayScaleLKDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.PayScale.Include(e => e.PayScaleArea).ToList();
        //        IEnumerable<PayScale> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.PayScale.ToList().Where(d => d.FullDetails.Contains(data));

        //        }
        //        else
        //        {
        //            var list1 = db.PayScale.ToList();
        //            //var list2 = fall.Except(list1);
        //            //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
        //            var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            //var result_1 = (from c in fall
        //            //                select new { c.Id, c.CorporateCode, c.CorporateName });
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    // return View();
        //}

        public class PayScaleAssign_SalHeadFormula
        {
            public Array SalHeadFormula_id { get; set; }
            public Array SalHeadFormula_val { get; set; }
            public string PayScaleAgr_id { get; set; }
            public string PayScaleAgr_val { get; set; }

        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<PayScaleAssign_SalHeadFormula> return_data = new List<PayScaleAssign_SalHeadFormula>();
                //var fall = db.SalHeadFormula.GroupBy(x => x.Name).Select(y => y.FirstOrDefault()).ToList();
                var Q = db.PayScaleAssignment.Include(e => e.PayScaleAgreement).Include(e => e.SalaryHead).Include(e => e.SalHeadFormula)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        SalaryHead_Id = e.SalaryHead.Id == null ? 0 : e.SalaryHead.Id
                    }).ToList();


                var a = db.PayScaleAssignment.Include(e => e.SalHeadFormula).Include(e => e.PayScaleAgreement).Where(e => e.Id == data).ToList();

                foreach (var ca in a)
                {
                    var c = ca.SalHeadFormula.GroupBy(e => e.Name).Select(y => y.FirstOrDefault()).ToList();
                    return_data.Add(
                new PayScaleAssign_SalHeadFormula
                {
                    SalHeadFormula_id = c.Select(e => e.Id).ToArray(),
                    SalHeadFormula_val = c.Select(e => e.FullDetails).ToArray(),
                    PayScaleAgr_id = ca.PayScaleAgreement.Id.ToString(),
                    PayScaleAgr_val = ca.PayScaleAgreement.FullDetails
                });
                }



                //var EmpOff = db.EmpOff.Find(data);
                //TempData["RowVersion"] = EmpOff.RowVersion;
                //var Auth = EmpOff.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, "", "", JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(PayScaleAssignment PayScaleAssign, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<SalHeadFormula> SalHeadForm = new List<SalHeadFormula>();
                    string selectedPayScaleAgr = form["PayScaleAgreementlist"];
                    string selectedSalHead = form["SalaryHeadlist"];
                    string SalHeadFormula_Values = form["SalHeadFormulalist"];
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var companypayroll = new CompanyPayroll();
                    companypayroll = db.CompanyPayroll.Include(e => e.PayScaleAssignment).Where(e => e.Company.Id == company_Id).SingleOrDefault();
                    if (selectedPayScaleAgr == null)
                    {
                        Msg.Add("Please Select PayscaleAgreeMent!");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (SalHeadFormula_Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(SalHeadFormula_Values);
                        foreach (var ca in ids)
                        {
                            var SalHeadForm_val = db.SalHeadFormula.Find(ca);
                            var SalHeadFormList = db.SalHeadFormula.Where(e => e.Name == SalHeadForm_val.Name).ToList();

                            SalHeadForm.AddRange(SalHeadFormList);
                            PayScaleAssign.SalHeadFormula = SalHeadForm;
                        }
                    }
                    else
                    {
                        PayScaleAssign.SalHeadFormula = null;
                    }


                    if (selectedPayScaleAgr != null && selectedPayScaleAgr != "")
                    {
                        PayScaleAssign.PayScaleAgreement = db.PayScaleAgreement.Find(int.Parse(selectedPayScaleAgr));
                    }


                    if (selectedSalHead != null && selectedSalHead != "")
                    {
                        PayScaleAssign.SalaryHead = db.SalaryHead.Find(int.Parse(selectedSalHead));
                    }

                    if (selectedSalHead != null && selectedSalHead != "" && selectedPayScaleAgr != null && selectedPayScaleAgr != "")
                    {
                        int salaryhead_id = Convert.ToInt32(selectedSalHead);
                        int payscaleagreement = Convert.ToInt32(selectedPayScaleAgr);


                        var compnydata = db.CompanyPayroll
                                                      .Include(e => e.PayScaleAssignment)
                                                      .Include(e => e.PayScaleAssignment.Select(t => t.SalaryHead))
                                                      .Include(e => e.PayScaleAssignment.Select(t => t.PayScaleAgreement))
                                                      .Where(e => e.Company.Id == company_Id).SingleOrDefault();
                        var CheckSameSalaryheadAssignment = compnydata.PayScaleAssignment.Where(e => e.PayScaleAgreement.Id == payscaleagreement && e.SalaryHead.Id == salaryhead_id).SingleOrDefault();

                        if (CheckSameSalaryheadAssignment != null)
                        {
                            Msg.Add("AllReady PayscaleAssignment Is Done For this SalaryHead!");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            PayScaleAssign.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            PayScaleAssignment PayScaleAssignment = new PayScaleAssignment()
                            {
                                PayScaleAgreement = PayScaleAssign.PayScaleAgreement,
                                SalaryHead = PayScaleAssign.SalaryHead,
                                SalHeadFormula = PayScaleAssign.SalHeadFormula,
                                DBTrack = PayScaleAssign.DBTrack
                            };
                            try
                            {
                                db.PayScaleAssignment.Add(PayScaleAssignment);
                                //var rtn_obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, SalaryHeadCon.DBTrack);
                                //DT_pays DT_SalaryHeadConfig = (DT_SalaryHeadConfig)rtn_obj;
                                //DT_SalaryHeadConfig.PayScaleConfig_Id = SalaryHeadCon.PayScaleConfig == null ? 0 : SalaryHeadCon.PayScaleConfig.Id;
                                //DT_SalaryHeadConfig.SalHeadFormula_Id = SalaryHeadCon.SalHeadFormula == null ? 0 : SalaryHeadCon.SalHeadFormula.Id;
                                //db.Create(DT_SalaryHeadConfig);
                                db.SaveChanges();
                                if (companypayroll != null)
                                {
                                    var payscaleassignment_list = new List<PayScaleAssignment>();
                                    payscaleassignment_list.Add(PayScaleAssignment);
                                    if (companypayroll.PayScaleAssignment != null)
                                    {
                                        payscaleassignment_list.AddRange(companypayroll.PayScaleAssignment);
                                    }
                                    companypayroll.PayScaleAssignment = payscaleassignment_list;
                                    db.CompanyPayroll.Attach(companypayroll);
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;


                                }
                                ts.Complete();
                                Msg.Add("  Data Created successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { null, null, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = PayScaleAssignment.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        //  return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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


        [HttpPost]
        public async Task<ActionResult> EditSave(PayScaleAssignment ESOBJ, int data, FormCollection form) // Edit submit
        {

            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.PayScaleAssignment.Include(e => e.SalHeadFormula).Where(e => e.Id == data).SingleOrDefault();
                    List<SalHeadFormula> SalHeadForm = new List<SalHeadFormula>();
                    string Values = form["SalHeadFormulalist"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var SalHeadFor_val = db.SalHeadFormula.Find(ca);
                            var SalHeadFormList = db.SalHeadFormula.Where(e => e.Name == SalHeadFor_val.Name).ToList();

                            SalHeadForm.AddRange(SalHeadFormList);
                            db_data.SalHeadFormula = SalHeadForm;
                        }
                    }
                    else
                    {
                        db_data.SalHeadFormula = null;
                    }

                    db.PayScaleAssignment.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["RowVersion"] = db_data.RowVersion;
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;



                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            try
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                PayScaleAssignment blog = null; // to retrieve old data                           
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.PayScaleAssignment.Where(e => e.Id == data)
                                                            .Include(e => e.SalHeadFormula).SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }
                                ESOBJ.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { ESOBJ.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Grade)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Grade)databaseEntry.ToObject();
                                    ESOBJ.RowVersion = databaseValues.RowVersion;
                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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




        public class GridClass
        {
            public int Id { get; set; }
            public String PayScaleAgreemnt { get; set; }
            public String SalaryHead { get; set; }

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
                IEnumerable<GridClass> PFMasterList = null;

                var compid = int.Parse(Session["CompId"].ToString());
                var data = db.CompanyPayroll.Include(e => e.PayScaleAssignment.Select(z => z.PayScaleAgreement))
                    .Include(e => e.PayScaleAssignment.Select(z => z.SalaryHead))
                    .Where(e => e.Id == compid).SingleOrDefault();
                List<GridClass> model = new List<GridClass>();
                if (data.PayScaleAssignment != null && data.PayScaleAssignment.Count > 0)
                {


                    foreach (var item in data.PayScaleAssignment)
                    {
                        model.Add(new GridClass
                        {
                            Id = item.Id,
                            PayScaleAgreemnt = item.PayScaleAgreement != null ? item.PayScaleAgreement.FullDetails : null,
                            SalaryHead = item.SalaryHead != null ? item.SalaryHead.Name : null,
                        });
                    }
                }
                PFMasterList = model;
                //if (gp.IsAutho == true)
                //{
                //    PFMaster = db.CompanyPayroll.Include(e => e.PFMaster.Select(a => a.PFTrustType)).AsNoTracking().ToList();
                //}
                //else
                //{
                // PFMaster = db.CompanyPayroll.Include(e=>e.PFMaster.Select(a=>a.PFTrustType)).AsNoTracking().ToList();
                //}

                IEnumerable<GridClass> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = PFMasterList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.SalaryHead.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.PayScaleAgreemnt.ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { Convert.ToString(a.SalaryHead), Convert.ToString(a.PayScaleAgreemnt), a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.SalaryHead, a.PayScaleAgreemnt, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = PFMasterList;
                    Func<GridClass, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "SalaryHead" ? Convert.ToString(c.SalaryHead) :
                                         gp.sidx == "PayScaleAgreement" ? Convert.ToString(c.PayScaleAgreemnt) :

                                         "");
                    }
                    //Func<GridClass, dynamic> orderfuc;
                    //if (gp.sidx == "Id")
                    //{
                    //    orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    //}
                    //else
                    //{
                    //    orderfuc = (c => gp.sidx == "SalaryHeadName" ? c.SalaryHead.ToString() :
                    //                     gp.sidx == "PayScaleAgreemntFullDetails" ? c.PayScaleAgreemnt.ToString() :
                    //                      "");
                    //}
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.SalaryHead), Convert.ToString(a.PayScaleAgreemnt), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.SalaryHead), Convert.ToString(a.PayScaleAgreemnt), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.SalaryHead), Convert.ToString(a.PayScaleAgreemnt), a.Id }).ToList();
                    }
                    totalRecords = PFMasterList.Count();
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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    PayScaleAssignment PayScaleAssign = db.PayScaleAssignment.Include(e => e.SalHeadFormula)
                                                       .Include(e => e.SalaryHead)
                                                       .Where(e => e.Id == data).SingleOrDefault();
                    var id = int.Parse(Session["CompId"].ToString());
                    var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
                    companypayroll.PayScaleAssignment.Where(e => e.Id == PayScaleAssign.Id);
                    companypayroll.PayScaleAssignment = null;
                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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

            //public ActionResult P2BGrid(P2BGrid_Parameters gp)
            //{
            //    try
            //    {
            //        DataBaseContext db = new DataBaseContext();
            //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
            //        int pageSize = gp.rows;
            //        int totalPages = 0;
            //        int totalRecords = 0;
            //        int ParentId = 2;
            //        var jsonData = (Object)null;
            //        var LKVal = db.PayScaleAssignment.ToList();

            //        if (gp.IsAutho == true)
            //        {
            //            LKVal = db.PayScaleAssignment.Include(e => e.PayScaleAgreemnt).Include(e => e.SalaryHead).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
            //        }
            //        else
            //        {
            //            LKVal = db.PayScaleAssignment.Include(e => e.PayScaleAgreemnt).Include(e => e.SalaryHead).AsNoTracking().ToList();
            //        }


            //        IEnumerable<PayScaleAssignment> IE;
            //        if (!string.IsNullOrEmpty(gp.searchString))
            //        {
            //            IE = LKVal;
            //            if (gp.searchOper.Equals("eq"))
            //            {
            //                jsonData = IE.Select(a => new { a.Id, a.SalaryHead.Name, a.PayScaleAgreemnt.FullDetails }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Name.ToLower() == gp.searchString.ToLower())));
            //            }
            //            if (pageIndex > 1)
            //            {
            //                int h = pageIndex * pageSize;
            //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.PayScaleAgreemnt.FullDetails }).ToList();
            //            }
            //            totalRecords = IE.Count();
            //        }
            //        else
            //        {
            //            IE = LKVal;
            //            Func<PayScaleAssignment, dynamic> orderfuc;
            //            if (gp.sidx == "Id")
            //            {
            //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
            //            }
            //            else
            //            {
            //                orderfuc = (c =>
            //                                 gp.sidx == "SalaryHead" ? c.SalaryHead.Name : "");
            //            }

            //            //Func<BasicScale, string> orderfuc = (c =>
            //            //                                           gp.sidx == "Id" ? c.Id.ToString() :
            //            //                                           gp.sidx == "Code" ? c.Code :
            //            //                                           gp.sidx == "ScaleName" ? c.ScaleName : "");
            //            if (gp.sord == "asc")
            //            {
            //                IE = IE.OrderBy(orderfuc);
            //                jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.PayScaleAgreemnt.FullDetails }).ToList();
            //            }
            //            else if (gp.sord == "desc")
            //            {
            //                IE = IE.OrderByDescending(orderfuc);
            //                jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.PayScaleAgreemnt.FullDetails }).ToList();
            //            }
            //            if (pageIndex > 1)
            //            {
            //                int h = pageIndex * pageSize;
            //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.PayScaleAgreemnt.FullDetails }).ToList();
            //            }
            //            totalRecords = LKVal.Count();
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
            //            total = totalPages,
            //            p2bparam = ParentId
            //        };
            //        return Json(JsonData, JsonRequestBehavior.AllowGet);
            //    }
            //    catch (Exception ex)
            //    {
            //        throw ex;
            //    }
            //}

        }
    }
}