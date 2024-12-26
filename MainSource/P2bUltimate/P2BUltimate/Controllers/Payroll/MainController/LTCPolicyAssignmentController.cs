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
using Training;
using Attendance;
using Leave;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class LTCPolicyAssignmentController : Controller
    {
        //
        // GET: /LTCPolicyAssignment/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/LTCPolicyAssignment/Index.cshtml");
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }


        public ActionResult GetLookupLTCformula(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LTCFormula.GroupBy(x => x.Name).Select(y => y.FirstOrDefault()).ToList();


                if (SkipIds != null)
                {
                    foreach (int a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LTCFormula
                                        .Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }
                var result = (from c in fall
                              select new { srno = c.Id, lookupvalue = c.Name }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetLookupPayscaleagreement(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.PayScaleAgreement.ToList();
                IEnumerable<PayScaleAgreement> all;
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


            }

        }

        [HttpPost]
        public ActionResult Create(LTCPolicyAssignment L, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<LTCFormula> LTCForm = new List<LTCFormula>();

                    var PolicyName = form["PolicyNamelist"] == "0" ? 0 : Convert.ToInt32(form["PolicyNamelist"]);
                    var LTCFormulalist = form["LTCFormulalist"] == "0" ? "" : form["LTCFormulalist"];
                    var PayScaleAgreementlist = form["PayScaleAgreementlist"] == "0" ? "" : form["PayScaleAgreementlist"];

                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    CompanyPayroll companypayroll = new CompanyPayroll();
                    companypayroll = db.CompanyPayroll.Include(e => e.LTCPolicyAssignment).Where(e => e.Company.Id == company_Id).FirstOrDefault();

                    if (PayScaleAgreementlist == null)
                    {
                        Msg.Add("Please Select PayscaleAgreeMent!");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (PolicyName != 0)
                    {
                        L.PolicyName = db.LookupValue.Find(PolicyName);
                    }

                    if (PayScaleAgreementlist != null && PayScaleAgreementlist != "")
                    {
                        L.PayScaleAgreement = db.PayScaleAgreement.Find(int.Parse(PayScaleAgreementlist));
                    }

                    if (LTCFormulalist != null)
                    {
                        var ids = Utility.StringIdsToListIds(LTCFormulalist);
                        foreach (var ca in ids)
                        {
                            var PolicyForm_val = db.LTCFormula.Find(ca);
                            var PolicyFormList = db.LTCFormula.Where(e => e.Name == PolicyForm_val.Name).ToList();

                            LTCForm.AddRange(PolicyFormList);
                            L.LTCFormula = LTCForm;
                        }
                    }
                    else
                    {
                        L.LTCFormula = null;
                    }


                    if (PolicyName != null && PolicyName != 0 && PayScaleAgreementlist != null && PayScaleAgreementlist != "")
                    {
                        int PolicyName_id = Convert.ToInt32(PolicyName);
                        int payscaleagreement = Convert.ToInt32(PayScaleAgreementlist);

                        CompanyPayroll compnydata = db.CompanyPayroll
                                                      .Include(e => e.LTCPolicyAssignment)
                                                      .Include(e => e.LTCPolicyAssignment.Select(t => t.PolicyName))
                                                      .Include(e => e.LTCPolicyAssignment.Select(t => t.PayScaleAgreement))
                                                      .Where(e => e.Company.Id == company_Id).FirstOrDefault();

                        var CheckSamePolicyAssignment = compnydata.LTCPolicyAssignment.Where(e => e.PayScaleAgreement.Id == payscaleagreement && e.PolicyName.Id == PolicyName_id).FirstOrDefault();

                        if (CheckSamePolicyAssignment != null)
                        {
                            Msg.Add("AllReady PolicyAssignment Is Done For this PolicyName!");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            LTCPolicyAssignment policyassignment = new LTCPolicyAssignment()
                            {
                                PayScaleAgreement = L.PayScaleAgreement,
                                PolicyName = L.PolicyName,
                                LTCFormula = L.LTCFormula,
                                DBTrack = L.DBTrack
                            };
                            try
                            {
                                db.LTCPolicyAssignment.Add(policyassignment);
                                //var rtn_obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, SalaryHeadCon.DBTrack);
                                //DT_pays DT_SalaryHeadConfig = (DT_SalaryHeadConfig)rtn_obj;
                                //DT_SalaryHeadConfig.PayScaleConfig_Id = SalaryHeadCon.PayScaleConfig == null ? 0 : SalaryHeadCon.PayScaleConfig.Id;
                                //DT_SalaryHeadConfig.SalHeadFormula_Id = SalaryHeadCon.SalHeadFormula == null ? 0 : SalaryHeadCon.SalHeadFormula.Id;
                                //db.Create(DT_SalaryHeadConfig);
                                db.SaveChanges();


                                if (companypayroll != null)
                                {
                                    List<LTCPolicyAssignment> PolicyAssignment_list = new List<LTCPolicyAssignment>();
                                    PolicyAssignment_list.Add(policyassignment);
                                    if (companypayroll.LTCPolicyAssignment != null)
                                    {
                                        PolicyAssignment_list.AddRange(companypayroll.LTCPolicyAssignment);
                                    }
                                    companypayroll.LTCPolicyAssignment = PolicyAssignment_list;
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
                                return RedirectToAction("Create", new
                                {
                                    concurrencyError = true
                                    //, id = PolicyAssignment.Id
                                });
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

        public class LTCPolicyAssignemntEditDetails
        {
            public Array LTCFormula_Id { get; set; }
            public Array LTCFormula_fulldetails { get; set; }

        }


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<LTCPolicyAssignemntEditDetails> return_data = new List<LTCPolicyAssignemntEditDetails>();

                var Q = db.LTCPolicyAssignment
                    .Include(e => e.PayScaleAgreement)
                    .Include(e => e.PayScaleAgreement.PayScale)
                    .Include(e => e.PolicyName)
                    .Include(e => e.LTCFormula)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        PolicyName = e.PolicyName != null ? e.PolicyName.Id : 0,
                        PayScaleAgreement_Id = e.PayScaleAgreement.Id == null ? 0 : e.PayScaleAgreement.Id,
                        PayScaleAgreement_FullDetails = e.PayScaleAgreement.FullDetails == null ? "" : e.PayScaleAgreement.FullDetails,
                        Action = e.DBTrack.Action

                    }).ToList();


                var a = db.LTCPolicyAssignment.Include(e => e.LTCFormula).Include(e => e.PayScaleAgreement).Where(e => e.Id == data).ToList();

                foreach (var ca in a)
                {
                    var c = ca.LTCFormula.GroupBy(e => e.Name).Select(y => y.FirstOrDefault()).ToList();
                    return_data.Add(
                    new LTCPolicyAssignemntEditDetails
                    {
                        LTCFormula_Id = c.Select(e => e.Id).ToArray(),
                        LTCFormula_fulldetails = c.Select(e => e.Name).ToArray(),

                    });
                }



                var Corp = db.LTCPolicyAssignment.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, "", "", JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(LTCPolicyAssignment L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.LTCPolicyAssignment
                        .Include(e => e.PolicyName)
                        .Include(e => e.LTCFormula).Where(e => e.Id == data).FirstOrDefault();
                    List<LTCFormula> LTCForm = new List<LTCFormula>();
                    string Values = form["LTCFormulalist"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var PolicyFor_val = db.LTCFormula.Find(ca);
                            var PolicyFormList = db.LTCFormula.Where(e => e.Name == PolicyFor_val.Name).ToList();

                            LTCForm.AddRange(PolicyFormList);
                            db_data.LTCFormula = LTCForm;
                        }
                    }
                    else
                    {
                        db_data.LTCFormula = null;
                    }

                    db.LTCPolicyAssignment.Attach(db_data);
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

                                LTCPolicyAssignment blog = null; // to retrieve old data                           
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.LTCPolicyAssignment.Where(e => e.Id == data)
                                                            .Include(e => e.LTCFormula).Include(e => e.PolicyName).SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }
                                L.DBTrack = new DBTrack
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
                                return Json(new Utility.JsonReturnClass { Id = L.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                                    L.RowVersion = databaseValues.RowVersion;
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


        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    LTCPolicyAssignment OBJLvCreditPolicy = db.LTCPolicyAssignment.Include(e => e.PolicyName)
                                                       .Include(e => e.LTCFormula)
                                                       .Include(e => e.PayScaleAgreement)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (OBJLvCreditPolicy.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = OBJLvCreditPolicy.DBTrack.CreatedBy != null ? OBJLvCreditPolicy.DBTrack.CreatedBy : null,
                                CreatedOn = OBJLvCreditPolicy.DBTrack.CreatedOn != null ? OBJLvCreditPolicy.DBTrack.CreatedOn : null,
                                IsModified = OBJLvCreditPolicy.DBTrack.IsModified == true ? true : false
                            };
                            OBJLvCreditPolicy.DBTrack = dbT;
                            db.Entry(OBJLvCreditPolicy).State = System.Data.Entity.EntityState.Modified;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, OBJLvCreditPolicy.DBTrack);
                            //DT_LvAssignment DT_Corp = (DT_LvAssignment)rtn_Obj;
                            //DT_Corp.LvHead_Id = OBJLvCreditPolicy.LvHead == null ? 0 : OBJLvCreditPolicy.LvHead.Id;
                            //DT_Corp.PayScaleAgreement_Id = OBJLvCreditPolicy.PayScaleAgreement == null ? 0 : OBJLvCreditPolicy.PayScaleAgreement.Id;
                            //db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        var LvHeadFormula = OBJLvCreditPolicy.LTCFormula;


                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (LvHeadFormula != null)
                            {
                                var PolicyFormula = new HashSet<int>(OBJLvCreditPolicy.LTCFormula.Select(e => e.Id));
                                if (PolicyFormula.Count > 0)
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
                                    CreatedBy = OBJLvCreditPolicy.DBTrack.CreatedBy != null ? OBJLvCreditPolicy.DBTrack.CreatedBy : null,
                                    CreatedOn = OBJLvCreditPolicy.DBTrack.CreatedOn != null ? OBJLvCreditPolicy.DBTrack.CreatedOn : null,
                                    IsModified = OBJLvCreditPolicy.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };
                                db.Entry(OBJLvCreditPolicy).State = System.Data.Entity.EntityState.Deleted;
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, OBJLvCreditPolicy.DBTrack);
                                //DT_LvAssignment DT_Corp = (DT_LvAssignment)rtn_Obj;
                                //DT_Corp.LvHead_Id = OBJLvCreditPolicy.LvHead == null ? 0 : OBJLvCreditPolicy.LvHead.Id;
                                //DT_Corp.PayScaleAgreement_Id = OBJLvCreditPolicy.PayScaleAgreement == null ? 0 : OBJLvCreditPolicy.PayScaleAgreement.Id;
                                //db.Create(DT_Corp);
                                await db.SaveChangesAsync();
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
                                // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

        public class GridClass
        {
            public int Id { get; set; }
            public String PayScaleAgreement { get; set; }
            public String PolicyName { get; set; }

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
                var data = db.LTCPolicyAssignment
                    .Include(e => e.PayScaleAgreement).Include(e => e.PolicyName).ToList();
                List<GridClass> model = new List<GridClass>();

                foreach (var item in data)
                {
                    model.Add(new GridClass
                    {
                        Id = item.Id,
                        PayScaleAgreement = item.PayScaleAgreement != null ? item.PayScaleAgreement.FullDetails : null,
                        PolicyName = item.PolicyName != null ? item.PolicyName.LookupVal : null,
                    });
                }

                PFMasterList = model;

                IEnumerable<GridClass> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = PFMasterList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.PolicyName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.PayScaleAgreement.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { Convert.ToString(a.PolicyName), Convert.ToString(a.PayScaleAgreement), a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.PolicyName, a.PayScaleAgreement, a.Id }).ToList();
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
                        orderfuc = (c => gp.sidx == "PolicyName" ? c.PolicyName.ToString() :
                            gp.sidx == "PayScaleAgreementFullDetails" ? c.PayScaleAgreement.ToString() :
                                          "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.PolicyName), Convert.ToString(a.PayScaleAgreement), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.PolicyName), Convert.ToString(a.PayScaleAgreement), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.PolicyName), Convert.ToString(a.PayScaleAgreement), a.Id }).ToList();
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

        public ActionResult GetLookupValue(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                if (data != "" && data != null)
                {
                    // var qurey = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == data).SingleOrDefault();
                    var qurey = db.Lookup.Where(e => e.Code == data).Select(e => e.LookupValues.Where(r => (r.IsActive == true))).SingleOrDefault(); // added by rekha 26-12-16
                    if (data2 != "" && data2 != "0")
                    {
                        selected = data2;
                    }
                    if (qurey != null)
                    {
                        s = new SelectList(qurey, "Id", "LookupVal", selected);
                    }
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }

        }

    }
}