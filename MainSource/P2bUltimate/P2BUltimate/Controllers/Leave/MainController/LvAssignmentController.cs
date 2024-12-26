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

namespace P2BUltimate.Controllers.Leave.MainController
{
       [AuthoriseManger]
    public class LvAssignmentController : Controller
    {
       // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Leave/MainViews/LvAssignment/Index.cshtml");
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        public ActionResult PopulateLvHeadDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.LvHead.OrderBy(e => e.LvCode).ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "LvName", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetLookuplvheadformula(List<int> SkipIds)
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
                var fall = db.LvHeadFormula.GroupBy(x => x.Name).Select(y => y.FirstOrDefault()).ToList();


                if (SkipIds != null)
                {
                    foreach (int a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LvHeadFormula
                                        .Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }
                var result = (from c in fall
                              select new { srno = c.Id, lookupvalue = c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult Create(LvAssignment L, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<LvHeadFormula> SalHeadForm = new List<LvHeadFormula>();
                    var LvHeadlist = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];
                    var LvHeadFormulalist = form["LvHeadFormulalist"] == "0" ? "" : form["LvHeadFormulalist"];
                    var PayScaleAgreementlist = form["PayScaleAgreementlist"] == "0" ? "" : form["PayScaleAgreementlist"];
                    var PolicyName = form["PolicyNamelist"] == "0" ? 0 : Convert.ToInt32(form["PolicyNamelist"]);
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    CompanyLeave companylv = new CompanyLeave();
                    companylv = db.CompanyLeave.Include(e => e.LvAssignment).Where(e => e.Company.Id == company_Id).FirstOrDefault();
                    if (PayScaleAgreementlist == null)
                    {
                        Msg.Add("Please Select PayscaleAgreeMent!");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (PolicyName != 0)
                    {
                        L.PolicyName = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "805").FirstOrDefault().LookupValues.Where(e => e.Id == PolicyName).FirstOrDefault();  //db.LookupValue.Find(PolicyName);
                    }
                    if (LvHeadFormulalist != null)
                    {
                        var ids = Utility.StringIdsToListIds(LvHeadFormulalist);
                        foreach (var ca in ids)
                        {
                            var LvHeadForm_val = db.LvHeadFormula.Find(ca);
                            var LVHeadFormList = db.LvHeadFormula.Where(e => e.Name == LvHeadForm_val.Name).ToList();

                            SalHeadForm.AddRange(LVHeadFormList);
                            L.LvHeadFormula = SalHeadForm;
                        }
                    }
                    else
                    {
                        L.LvHeadFormula = null;
                    }


                    if (PayScaleAgreementlist != null && PayScaleAgreementlist != "")
                    {
                        L.PayScaleAgreement = db.PayScaleAgreement.Find(int.Parse(PayScaleAgreementlist));
                    }


                    if (LvHeadlist != null && LvHeadlist != "")
                    {
                        L.LvHead = db.LvHead.Find(int.Parse(LvHeadlist));
                    }

                    if (PolicyName != null && PolicyName != 0 && LvHeadlist != null && LvHeadlist != "" && PayScaleAgreementlist != null && PayScaleAgreementlist != "")
                    {
                        int LVhead_id = Convert.ToInt32(LvHeadlist);
                        int payscaleagreement = Convert.ToInt32(PayScaleAgreementlist);
                        int PolicyName_id = Convert.ToInt32(PolicyName);

                        CompanyLeave compnydata = db.CompanyLeave
                                                      .Include(e => e.LvAssignment)
                                                      .Include(e => e.LvAssignment.Select(t => t.LvHead))
                                                      .Include(e => e.LvAssignment.Select(t => t.PayScaleAgreement))
                                                      .Include(e => e.LvAssignment.Select(t => t.PolicyName))
                                                      .Where(e => e.Company.Id == company_Id).FirstOrDefault();
                        var CheckSameSalaryheadAssignment = compnydata.LvAssignment.Where(e => e.PayScaleAgreement.Id == payscaleagreement && e.LvHead.Id == LVhead_id && e.PolicyName.Id == PolicyName_id).FirstOrDefault();

                        if (CheckSameSalaryheadAssignment != null)
                        {
                            Msg.Add("AllReady PayscaleAssignment Is Done For this Leave Head!");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            LvAssignment LvAssignment = new LvAssignment()
                            {
                                PayScaleAgreement = L.PayScaleAgreement,
                                LvHead = L.LvHead,
                                PolicyName = L.PolicyName,
                                LvHeadFormula = L.LvHeadFormula,
                                DBTrack = L.DBTrack
                            };
                            try
                            {
                                db.LvAssignment.Add(LvAssignment);
                                //var rtn_obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, SalaryHeadCon.DBTrack);
                                //DT_pays DT_SalaryHeadConfig = (DT_SalaryHeadConfig)rtn_obj;
                                //DT_SalaryHeadConfig.PayScaleConfig_Id = SalaryHeadCon.PayScaleConfig == null ? 0 : SalaryHeadCon.PayScaleConfig.Id;
                                //DT_SalaryHeadConfig.SalHeadFormula_Id = SalaryHeadCon.SalHeadFormula == null ? 0 : SalaryHeadCon.SalHeadFormula.Id;
                                //db.Create(DT_SalaryHeadConfig);
                                db.SaveChanges();
                                if (companylv != null)
                                {
                                    List<LvAssignment> lvassignment_list = new List<LvAssignment>();
                                    lvassignment_list.Add(LvAssignment);
                                    if (companylv.LvAssignment != null)
                                    {
                                        lvassignment_list.AddRange(companylv.LvAssignment);
                                    }
                                    companylv.LvAssignment = lvassignment_list;
                                    db.CompanyLeave.Attach(companylv);
                                    db.Entry(companylv).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companylv).State = System.Data.Entity.EntityState.Detached;


                                }
                                ts.Complete();
                                Msg.Add("  Data Created successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { null, null, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = LvAssignment.Id });
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

        public class LvassignemntEditDetails
        {
            public Array Lvheadformula_Id { get; set; }
            public Array Lvheadformula_fulldetails { get;set;}
        
        }


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<LvassignemntEditDetails> return_data = new List<LvassignemntEditDetails>();
                var Q = db.LvAssignment
                    .Include(e => e.PayScaleAgreement)
                    .Include(e => e.LvHeadFormula)
                    .Include(e => e.LvHead)
                    .Include(e => e.PolicyName)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        PolicyName = e.PolicyName != null ? e.PolicyName.Id : 0,
                        LvHead_Id = e.LvHead.Id == null ? 0 : e.LvHead.Id, 
                        PayScaleAgreement_Id = e.PayScaleAgreement.Id == null ? 0 : e.PayScaleAgreement.Id,
                        PayScaleAgreement_FullDetails = e.PayScaleAgreement.FullDetails == null ? "" : e.PayScaleAgreement.FullDetails,
                        Action = e.DBTrack.Action
                    }).ToList();

                var a = db.LvAssignment.Include(e => e.LvHeadFormula).Include(e => e.PayScaleAgreement).Where(e => e.Id == data).ToList();

                foreach (var ca in a)
                {
                    var c = ca.LvHeadFormula.GroupBy(e => e.Name).Select(y => y.FirstOrDefault()).ToList();
                    return_data.Add(
                new LvassignemntEditDetails
                {
                    Lvheadformula_Id = c.Select(e => e.Id).ToArray(),
                    Lvheadformula_fulldetails = c.Select(e => e.FullDetails).ToArray(),
                });
                }

                var W = db.DT_LvAssignment
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,



                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.LvAssignment.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(LvAssignment L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.LvAssignment.Include(e => e.LvHeadFormula).Include(e => e.PolicyName).Where(e => e.Id == data).FirstOrDefault();
                    List<LvHeadFormula> LvHeadForm = new List<LvHeadFormula>();
                    string Values = form["LvHeadFormulalist"];

                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var LvHeadFor_val = db.LvHeadFormula.Find(ca);
                            var LvHeadFormList = db.LvHeadFormula.Where(e => e.Name == LvHeadFor_val.Name).ToList();

                            LvHeadForm.AddRange(LvHeadFormList);
                            db_data.LvHeadFormula = LvHeadForm;
                        }
                    }
                    else
                    {
                        db_data.LvHeadFormula = null;
                    }

                    db.LvAssignment.Attach(db_data);
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

                                LvAssignment blog = null; // to retrieve old data                           
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.LvAssignment.Where(e => e.Id == data)
                                                            .Include(e => e.LvHeadFormula).SingleOrDefault();
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
                    LvAssignment OBJLvCreditPolicy = db.LvAssignment.Include(e => e.LvHead)
                                                       .Include(e => e.LvHeadFormula)
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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, OBJLvCreditPolicy.DBTrack);
                            DT_LvAssignment DT_Corp = (DT_LvAssignment)rtn_Obj;
                            DT_Corp.LvHead_Id = OBJLvCreditPolicy.LvHead == null ? 0 : OBJLvCreditPolicy.LvHead.Id;
                            DT_Corp.PayScaleAgreement_Id = OBJLvCreditPolicy.PayScaleAgreement == null ? 0 : OBJLvCreditPolicy.PayScaleAgreement.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        var LvHeadFormula = OBJLvCreditPolicy.LvHeadFormula;


                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (LvHeadFormula != null)
                            {
                                var Lvheadformula = new HashSet<int>(OBJLvCreditPolicy.LvHeadFormula.Select(e => e.Id));
                                if (Lvheadformula.Count > 0)
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
                                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, OBJLvCreditPolicy.DBTrack);
                                DT_LvAssignment DT_Corp = (DT_LvAssignment)rtn_Obj;
                                DT_Corp.LvHead_Id = OBJLvCreditPolicy.LvHead == null ? 0 : OBJLvCreditPolicy.LvHead.Id;
                                DT_Corp.PayScaleAgreement_Id = OBJLvCreditPolicy.PayScaleAgreement == null ? 0 : OBJLvCreditPolicy.PayScaleAgreement.Id;
                                db.Create(DT_Corp);
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
            public String PayScaleAgreemnt { get; set; }
            public String LvHead { get; set; }
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
                var data = db.LvAssignment
                    .Include(e => e.PayScaleAgreement).Include(e => e.LvHead).Include(e => e.PolicyName).ToList();
                List<GridClass> model = new List<GridClass>();
            
                    foreach (var item in data)
                    {
                        model.Add(new GridClass
                        {
                            Id = item.Id,
                            PayScaleAgreemnt = item.PayScaleAgreement != null ? item.PayScaleAgreement.FullDetails : null,
                            LvHead = item.LvHead != null ? item.LvHead.FullDetails : null,
                            PolicyName = item.PolicyName != null ? item.PolicyName.LookupVal.ToString() : null,
                        });
                    }
              
                PFMasterList = model;
              
                IEnumerable<GridClass> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = PFMasterList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.LvHead.ToUpper().Contains(gp.searchString.ToUpper()))
                            || (e.PayScaleAgreemnt.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.PolicyName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { Convert.ToString(a.LvHead), Convert.ToString(a.PolicyName), Convert.ToString(a.PayScaleAgreemnt), a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.LvHead,a.PolicyName, a.PayScaleAgreemnt, a.Id }).ToList();
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
                        orderfuc = (c => gp.sidx == "LeaveHeadName" ? c.LvHead.ToString() :
                                         gp.sidx == "PolicyName" ? c.PolicyName.ToString() :
                                         gp.sidx == "PayScaleAgreemntFullDetails" ? c.PayScaleAgreemnt.ToString() :
                                          "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.LvHead), Convert.ToString(a.PolicyName), Convert.ToString(a.PayScaleAgreemnt), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.LvHead), Convert.ToString(a.PolicyName), Convert.ToString(a.PayScaleAgreemnt), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.LvHead), Convert.ToString(a.PolicyName), Convert.ToString(a.PayScaleAgreemnt), a.Id }).ToList();
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

	}
}