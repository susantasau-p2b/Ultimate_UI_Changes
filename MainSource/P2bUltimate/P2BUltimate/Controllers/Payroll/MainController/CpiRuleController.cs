using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Threading.Tasks;
using P2BUltimate.Security;


namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class CpiruleController : Controller
    {
        //
        // GET: /Cpi_rule/
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            //return View("~/Views/CpiRule/Index.cshtml");
            return View("~/Views/Payroll/MainViews/Cpirule/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Payroll/_CpiRule.cshtml");
        }
        public ActionResult GetCPIRuleLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.CPIRule.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.CPIRule.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Create(CPIRule cp, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Name = form["Name"] == "0" ? "" : form["Name"];
                    string CPIRuleDetailslist = form["CPIRuleDetailslist"] == "0" ? "" : form["CPIRuleDetailslist"];
                    string CPIUnitCalclist = form["CPIUnitCalclist"] == "0" ? "" : form["CPIUnitCalclist"];
                    string IBaseDigit = form["IBaseDigit"] == "0" ? "" : form["IBaseDigit"];
                    string MinAmountIBase = form["MinAmountIBase"] == "0" ? "" : form["MinAmountIBase"];
                    string MaxAmountIBase = form["MaxAmountIBase"] == "0" ? "" : form["MaxAmountIBase"];
                    string RoundingMethodlist = form["RoundingMethodlist"] == "0" ? "" : form["RoundingMethodlist"];

                    var id = int.Parse(Session["CompId"].ToString());
                    var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
                    if (db.CPIRule.Any(e => e.Name == Name))
                    {
                        Msg.Add("Name is already exist");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    if (Name != "" && Name != null)
                    {
                        var val = Name;
                        cp.Name = val;

                    }

                    List<CPIRuleDetails> CPIRuleDetails = new List<CPIRuleDetails>();
                    if (CPIRuleDetailslist != "" && CPIRuleDetailslist != null)
                    {
                        var ids = Utility.StringIdsToListIds(CPIRuleDetailslist);
                        foreach (var ca in ids)
                        {
                            var val = db.CPIRuleDetails.Find(ca);
                            CPIRuleDetails.Add(val);
                            cp.CPIRuleDetails = CPIRuleDetails;
                        }
                    }

                    List<CPIUnitCalc> CPIUnitCalc = new List<CPIUnitCalc>();
                    if (CPIUnitCalclist != "" && CPIUnitCalclist != null)
                    {
                        var ids = Utility.StringIdsToListIds(CPIUnitCalclist);
                        foreach (var ca in ids)
                        {
                            var val = db.CPIUnitCalc.Find(ca);
                            CPIUnitCalc.Add(val);
                            cp.CPIUnitCalc = CPIUnitCalc;
                        }
                    }

                    if (IBaseDigit != "" && IBaseDigit != null)
                    {
                        var val = int.Parse(IBaseDigit);
                        cp.IBaseDigit = val;

                    }
                    //if (MinAmountIBase != "" && MinAmountIBase != null)
                    //{
                    //    var val = double.Parse(MinAmountIBase);
                    //    cp.MinAmountIBase = val;

                    //}
                    //if (MaxAmountIBase != "" && MaxAmountIBase != null)
                    //{
                    //    var val = double.Parse(MaxAmountIBase);
                    //    cp.MaxAmountIBase = val;

                    //}
                    List<LookupValue> LookupValue = new List<LookupValue>();
                    if (RoundingMethodlist != "0" && RoundingMethodlist != "" && RoundingMethodlist != null && RoundingMethodlist != "-Select-")
                    {
                        var ids = Utility.StringIdsToListIds(RoundingMethodlist);
                        foreach (var ca in ids)
                        {
                            var val = db.LookupValue.Find(ca);
                            LookupValue.Add(val);
                            cp.RoundingMethod = val;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            cp.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            CPIRule cpirule = new CPIRule()
                            {
                                Name = cp.Name,
                                CPIRuleDetails = cp.CPIRuleDetails,
                                CPIUnitCalc = cp.CPIUnitCalc,
                                IBaseDigit = cp.IBaseDigit,
                                VDAOnDirectBasic=cp.VDAOnDirectBasic,
                                //MinAmountIBase = cp.MinAmountIBase,
                                //MaxAmountIBase = cp.MaxAmountIBase,
                                RoundingMethod = cp.RoundingMethod,
                                DBTrack = cp.DBTrack
                            };
                            try
                            {
                                db.CPIRule.Add(cpirule);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, cp.DBTrack);
                                DT_CPIRule DT_CPIRule = (DT_CPIRule)rtn_Obj;
                                //DT_CPIRule.CPIRuleDeatils_Id = cp.CPIRuleDetails == null ? 0 : cp.CPIRuleDetails.ToString();

                                //DT_PTaxMaster.StatutoryEffectiveMonths_Id = PTMaster.StatutoryEffectiveMonths == null ? 0 : Convert.ToInt32(PTMaster.StatutoryEffectiveMonths); ;
                                db.Create(DT_CPIRule);
                                db.SaveChanges();
                                if (companypayroll != null)
                                {
                                    List<CPIRule> pfmasterlist = new List<CPIRule>();
                                    pfmasterlist.Add(cpirule);
                                    companypayroll.CPIRule = pfmasterlist;
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                                }

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = cp.Id });
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
                        // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                    CPIRule corporates = db.CPIRule
                        .Include(e => e.CPIRuleDetails)
                        .Include(e => e.CPIUnitCalc)
                        .Include(e => e.RoundingMethod)
                        .Where(e => e.Id == data).SingleOrDefault();

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    var id = int.Parse(Session["CompId"].ToString());
                    var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
                    companypayroll.CPIRule.Where(e => e.Id == corporates.Id);
                    companypayroll.CPIRule = null;
                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            corporates.DBTrack = dbT;
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, corporates.DBTrack);
                            DT_CPIRule DT_Corp = (DT_CPIRule)rtn_Obj;
                            //DT_Corp.Language_Id = corporates.Language == null ? 0 : corporates.Language.Id;


                            db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        var selectedRegions = corporates.CPIRuleDetails;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (selectedRegions != null)
                            {
                                var corpRegion = new HashSet<int>(corporates.CPIRuleDetails.Select(e => e.Id));
                                if (corpRegion.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
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
                                    CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                    CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                    IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
                                DT_CPIRule DT_Corp = (DT_CPIRule)rtn_Obj;
                                //DT_Corp.Language_Id = add == null ? 0 : add.Id;

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


        public class cpiruledetalis
        {
            public Array cpiruledetails_Id { get; set; }
            public Array cpiruledetails_Fulldetails { get; set; }

        };
        public class cpiunitcalc
        {
            public Array cpiunitcalc_Id { get; set; }
            public Array cpiunitcalc_Details { get; set; }

        };

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.CPIRule
                   .Include(e => e.CPIRuleDetails)
                   .Include(e => e.RoundingMethod)
                   .Where(e => e.Id == data).Select
                   (e => new
                   {
                       Name = e.Name,

                       CPIUnitCalc = e.CPIUnitCalc,
                       IBaseDigit = e.IBaseDigit,
                       VDAOnDirectBasic=e.VDAOnDirectBasic,
                       //MinAmountIBase = e.MinAmountIBase,
                       //MaxAmountIBase = e.MaxAmountIBase,
                       RoundingMethod_Id = e.RoundingMethod.Id == null ? 0 : e.RoundingMethod.Id,
                       Action = e.DBTrack.Action
                   }).ToList();

                List<cpiruledetalis> cont = new List<cpiruledetalis>();
                var k = db.CPIRule.Include(e => e.CPIRuleDetails).Where(e => e.Id == data).Select(e => e.CPIRuleDetails).ToList();
                foreach (var val in k)
                {
                    cont.Add(new cpiruledetalis
                    {
                        cpiruledetails_Id = val.Select(e => e.Id.ToString()).ToArray(),
                        cpiruledetails_Fulldetails = val.Select(e => e.FullDetails).ToArray()

                    });
                }

                List<cpiunitcalc> cu = new List<cpiunitcalc>();
                var Ku = db.CPIRule.Include(e => e.CPIRuleDetails).Where(e => e.Id == data).Select(e => e.CPIUnitCalc).ToList();
                foreach (var val in Ku)
                {
                    cu.Add(new cpiunitcalc
                    {
                        cpiunitcalc_Id = val.Select(e => e.Id.ToString()).ToArray(),
                        cpiunitcalc_Details = val.Select(e => e.FullDetails).ToArray()
                    });
                }
                var W = db.DT_CPIRule
                        .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                        (e => new
                        {
                            DT_Id = e.Id,
                            CPIRuleDetails_val = e.CPIRuleDetails_Id == 0 ? "" : db.CPIRuleDetails.Where(x => x.Id == e.CPIRuleDetails_Id).Select(x => x.FullDetails).FirstOrDefault(),

                        }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.CPIRule.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, cont, cu, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        //[HttpPost]
        //public async Task<ActionResult> EditSave(CPIRule cp, int data, FormCollection form) // Edit submit
        //{
        //    string Name = form["Name"] == "0" ? "" : form["Name"];
        //    string CPIRuleDetailslist = form["CPIRuleDetailslist"] == "0" ? "" : form["CPIRuleDetailslist"];
        //    string CPIUnitCalclist = form["CPIUnitCalclist"] == "0" ? "" : form["CPIUnitCalclist"];
        //    string IBaseDigit = form["IBaseDigit"] == "0" ? "" : form["IBaseDigit"];
        //    string MinAmountIBase = form["MinAmountIBase"] == "0" ? "" : form["MinAmountIBase"];
        //    string MaxAmountIBase = form["MaxAmountIBase"] == "0" ? "" : form["MaxAmountIBase"];
        //    string RoundingMethodlist = form["RoundingMethodlist"] == "0" ? "" : form["RoundingMethodlist"];
        //    //  bool Auth = form["Autho_Action"] == "" ? false : true;
        //    bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //    if (Name != "" && Name != null)
        //    {
        //        var val = Name;
        //        cp.Name = val;

        //    }

        //    List<CPIRuleDetails> CPIRuleDetails = new List<CPIRuleDetails>();
        //    if (CPIRuleDetailslist != "" && CPIRuleDetailslist != null)
        //    {
        //        var ids = Utility.StringIdsToListIds(CPIRuleDetailslist);
        //        foreach (var ca in ids)
        //        {
        //            var val = db.CPIRuleDetails.Find(ca);
        //            CPIRuleDetails.Add(val);
        //            cp.CPIRuleDetails = CPIRuleDetails;
        //        }
        //    }
        //    else
        //    {
        //        cp.CPIRuleDetails = null;
        //    }

        //    List<CPIUnitCalc> CPIUnitCalc = new List<CPIUnitCalc>();
        //    if (CPIUnitCalclist != "" && CPIUnitCalclist != null)
        //    {
        //        var ids = Utility.StringIdsToListIds(CPIUnitCalclist);
        //        foreach (var ca in ids)
        //        {
        //            var val = db.CPIUnitCalc.Find(ca);
        //            CPIUnitCalc.Add(val);
        //            cp.CPIUnitCalc = CPIUnitCalc;
        //        }
        //    }
        //    else
        //    {
        //        cp.CPIUnitCalc = null;
        //    }

        //    if (IBaseDigit != "" && IBaseDigit != null)
        //    {
        //        var val = int.Parse(IBaseDigit);
        //        cp.IBaseDigit = val;

        //    }
        //    if (MinAmountIBase != "" && MinAmountIBase != null)
        //    {
        //        var val = double.Parse(MinAmountIBase);
        //        cp.MinAmountIBase = val;

        //    }
        //    if (MaxAmountIBase != "" && MaxAmountIBase != null)
        //    {
        //        var val = double.Parse(MaxAmountIBase);
        //        cp.MaxAmountIBase = val;

        //    }
        //    List<LookupValue> LookupValue = new List<LookupValue>();
        //    if (RoundingMethodlist != "0" && RoundingMethodlist != "" && RoundingMethodlist != null && RoundingMethodlist != "-Select-")
        //    {
        //        var ids = Utility.StringIdsToListIds(RoundingMethodlist);
        //        foreach (var ca in ids)
        //        {
        //            var val = db.LookupValue.Find(ca);
        //            LookupValue.Add(val);
        //            cp.RoundingMethod = val;
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
        //                    CPIRule blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.CPIRule.Where(e => e.Id == data).Include(e => e.CPIRuleDetails)
        //                                                .Include(e => e.CPIUnitCalc)
        //                                               .SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }

        //                    cp.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    //int a = EditS(RoundingMethodlist,data, cp, cp.DBTrack);

        //                    if (RoundingMethodlist != null)
        //                    {
        //                        if (RoundingMethodlist != "")
        //                        {
        //                            var val = db.LookupValue.Find(int.Parse(RoundingMethodlist));
        //                            cp.RoundingMethod = val;

        //                            var type = db.CPIRule.Include(e => e.RoundingMethod).Where(e => e.Id == data).SingleOrDefault();
        //                            IList<CPIRule> typedetails = null;
        //                            if (type.RoundingMethod != null)
        //                            {
        //                                typedetails = db.CPIRule.Where(x => x.RoundingMethod.Id == type.RoundingMethod.Id && x.Id == data).ToList();
        //                            }
        //                            else
        //                            {
        //                                typedetails = db.CPIRule.Where(x => x.Id == data).ToList();
        //                            }
        //                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                            foreach (var s in typedetails)
        //                            {
        //                                s.RoundingMethod = cp.RoundingMethod;
        //                                db.CPIRule.Attach(s);
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                //await db.SaveChangesAsync();
        //                                db.SaveChanges();
        //                                TempData["RowVersion"] = s.RowVersion;
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            var BusiTypeDetails = db.CPIRule.Include(e => e.RoundingMethod).Where(x => x.Id == data).ToList();
        //                            foreach (var s in BusiTypeDetails)
        //                            {
        //                                s.RoundingMethod = null;
        //                                db.CPIRule.Attach(s);
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                //await db.SaveChangesAsync();
        //                                db.SaveChanges();
        //                                TempData["RowVersion"] = s.RowVersion;
        //                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                            }
        //                        }
        //                    }

        //                    if (CPIRuleDetailslist != null)
        //                    {
        //                        if (CPIRuleDetailslist != "")
        //                        {

        //                            List<int> IDs = CPIRuleDetailslist.Split(',').Select(e => int.Parse(e)).ToList();
        //                            foreach (var k in IDs)
        //                            {
        //                                var value = db.CPIRuleDetails.Find(k);
        //                                cp.CPIRuleDetails = new List<CPIRuleDetails>();
        //                                cp.CPIRuleDetails.Add(value);
        //                                db.Entry(value).State = System.Data.Entity.EntityState.Modified;
        //                                //await db.SaveChangesAsync();
        //                                db.SaveChanges();
        //                                TempData["RowVersion"] = value.RowVersion;
        //                                db.Entry(value).State = System.Data.Entity.EntityState.Detached;

        //                            }



        //                        }
        //                    }
        //                    else
        //                    {
        //                        var CPIRULEdetails = db.CPIRule.Include(e => e.CPIRuleDetails).Where(x => x.Id == data).ToList();
        //                        foreach (var s in CPIRULEdetails)
        //                        {
        //                            s.CPIRuleDetails = null;
        //                            db.CPIRule.Attach(s);
        //                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                            //await db.SaveChangesAsync();
        //                            db.SaveChanges();
        //                            TempData["RowVersion"] = s.RowVersion;
        //                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                        }
        //                    }

        //                    if (CPIUnitCalclist != null)
        //                    {
        //                        if (CPIUnitCalclist != "")
        //                        {

        //                            List<int> IDs = CPIUnitCalclist.Split(',').Select(e => int.Parse(e)).ToList();
        //                            foreach (var k in IDs)
        //                            {
        //                                var value = db.CPIUnitCalc.Find(k);
        //                                cp.CPIUnitCalc = new List<CPIUnitCalc>();
        //                                cp.CPIUnitCalc.Add(value);
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        var CPIUnitCalcdetails = db.CPIRule.Include(e => e.CPIUnitCalc).Where(x => x.Id == data).ToList();
        //                        foreach (var s in CPIUnitCalcdetails)
        //                        {
        //                            s.CPIUnitCalc = null;
        //                            db.CPIRule.Attach(s);
        //                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                            //await db.SaveChangesAsync();
        //                            db.SaveChanges();
        //                            TempData["RowVersion"] = s.RowVersion;
        //                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                        }
        //                    }

        //                    var CurCorp = db.CPIRule.Find(data);
        //                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                    {

        //                        CPIRule corp = new CPIRule()
        //                        {
        //                            Id = data,
        //                            Name = cp.Name,
        //                            CPIRuleDetails = cp.CPIRuleDetails,
        //                            CPIUnitCalc = cp.CPIUnitCalc,
        //                            IBaseDigit = cp.IBaseDigit,
        //                            MinAmountIBase = cp.MinAmountIBase,
        //                            MaxAmountIBase = cp.MaxAmountIBase,
        //                            RoundingMethod = cp.RoundingMethod,
        //                            DBTrack = cp.DBTrack
        //                        };

        //                        db.CPIRule.Attach(corp);
        //                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    }

        //                    await db.SaveChangesAsync();
        //                    ts.Complete();


        //                    return Json(new Object[] { cp.Id, cp.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

        //                }
        //            }
        //            catch (DbUpdateConcurrencyException ex)
        //            {
        //                var entry = ex.Entries.Single();
        //                var clientValues = (Corporate)entry.Entity;
        //                var databaseEntry = entry.GetDatabaseValues();
        //                if (databaseEntry == null)
        //                {
        //                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                }
        //                else
        //                {
        //                    var databaseValues = (Corporate)databaseEntry.ToObject();
        //                    cp.RowVersion = databaseValues.RowVersion;

        //                }
        //            }

        //            return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    else
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {

        //            CPIRule blog = null; // to retrieve old data
        //            DbPropertyValues originalBlogValues = null;
        //            CPIRule Old_Corp = null;

        //            using (var context = new DataBaseContext())
        //            {
        //                blog = context.CPIRule.Where(e => e.Id == data).SingleOrDefault();
        //                originalBlogValues = context.Entry(blog).OriginalValues;
        //            }
        //            cp.DBTrack = new DBTrack
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

        //            CPIRule cpi = new CPIRule()
        //            {
        //                Name = cp.Name,
        //                CPIRuleDetails = cp.CPIRuleDetails,
        //                CPIUnitCalc = cp.CPIUnitCalc,
        //                IBaseDigit = cp.IBaseDigit,
        //                MinAmountIBase = cp.MinAmountIBase,
        //                MaxAmountIBase = cp.MaxAmountIBase,
        //                RoundingMethod = cp.RoundingMethod,
        //                DBTrack = cp.DBTrack,
        //                Id = data,
        //                RowVersion = (Byte[])TempData["RowVersion"]
        //            };


        //            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //            using (var context = new DataBaseContext())
        //            {
        //                var obj = DBTrackFile.ModifiedDataHistory("Payroll", "M", blog, cpi, "CPIRule", cp.DBTrack);
        //                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                Old_Corp = context.CPIRule.Where(e => e.Id == data).Include(e => e.CPIRuleDetails)
        //                    .Include(e => e.CPIUnitCalc).SingleOrDefault();
        //                DT_CPIRule DT_Cp = (DT_CPIRule)obj;
        //                DT_Cp.CPIRuleDeatils_Id = DBTrackFile.ValCompare(Old_Corp.CPIRuleDetails, cp.CPIRuleDetails);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;

        //                db.Create(DT_Cp);
        //                //db.SaveChanges();
        //            }
        //            blog.DBTrack = cp.DBTrack;
        //            db.CPIRule.Attach(blog);
        //            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            db.SaveChanges();
        //            ts.Complete();
        //            return Json(new Object[] { blog.Id, cp.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //        }

        //    }
        //    return View();

        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(CPIRule cp, int data, FormCollection form) // Edit submit
        {
   			List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Name = form["Name"] == "0" ? "" : form["Name"];
                    string CPIRuleDetailslist = form["CPIRuleDetailslist"] == "0" ? "" : form["CPIRuleDetailslist"];
                    string CPIUnitCalclist = form["CPIUnitCalclist"] == "0" ? "" : form["CPIUnitCalclist"];
                    string IBaseDigit = form["IBaseDigit"] == "0" ? "" : form["IBaseDigit"];
                    string MinAmountIBase = form["MinAmountIBase"] == "0" ? "" : form["MinAmountIBase"];
                    string MaxAmountIBase = form["MaxAmountIBase"] == "0" ? "" : form["MaxAmountIBase"];
                    string RoundingMethodlist = form["RoundingMethodlist"] == "0" ? "" : form["RoundingMethodlist"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                     
                    if (Name != "" && Name != null)
                    {
                        var val = Name;
                        cp.Name = val;

                    }
                    if (IBaseDigit != "" && IBaseDigit != null)
                    {
                        var val = int.Parse(IBaseDigit);
                        cp.IBaseDigit = val;

                    }
                    cp.RoundingMethod_Id = RoundingMethodlist != null && RoundingMethodlist != "" ? int.Parse(RoundingMethodlist) : 0;

                    
                   
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    CPIRule typedetails = null;

                                    List<CPIRuleDetails> CPIRuleDetails = new List<CPIRuleDetails>();
                                    typedetails = db.CPIRule.Include(e => e.CPIRuleDetails).Where(e => e.Id == data).SingleOrDefault();
                                    if (CPIRuleDetailslist != null && CPIRuleDetailslist != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(CPIRuleDetailslist);
                                        foreach (var ca in ids)
                                        {
                                            var CPIRuleDetails_val = db.CPIRuleDetails.Find(ca);
                                            CPIRuleDetails.Add(CPIRuleDetails_val);
                                            typedetails.CPIRuleDetails = CPIRuleDetails;
                                        }
                                    }
                                    else
                                    {
                                        typedetails.CPIRuleDetails = null;
                                    }

                                    List<CPIUnitCalc> CPIUnitCalc = new List<CPIUnitCalc>();
                                    typedetails = db.CPIRule.Include(e => e.CPIUnitCalc).Where(e => e.Id == data).SingleOrDefault();
                                    if (CPIUnitCalclist != null && CPIUnitCalclist != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(CPIUnitCalclist);
                                        foreach (var ca in ids)
                                        {
                                            var CPIUnitCalc_val = db.CPIUnitCalc.Find(ca);
                                            CPIUnitCalc.Add(CPIUnitCalc_val);
                                            typedetails.CPIUnitCalc = CPIUnitCalc;
                                        }
                                    }
                                    else
                                    {
                                        typedetails.CPIUnitCalc = null;
                                    }




                                    db.CPIRule.Attach(typedetails);
                                    db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = typedetails.RowVersion; 


                                    var Curr_OBJ = db.CPIRule.Find(data);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                     

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        CPIRule blog = blog = null;
                                        DbPropertyValues originalBlogValues = null;

                                        
                                            blog = db.CPIRule.Where(e => e.Id == data).SingleOrDefault();
                                            originalBlogValues = db.Entry(blog).OriginalValues;
                                        
                                        cp.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };
                                         
                                            Curr_OBJ.Name = cp.Name;
                                            Curr_OBJ.RoundingMethod_Id = cp.RoundingMethod_Id != 0 ? cp.RoundingMethod_Id : null;
                                            Curr_OBJ.IBaseDigit = cp.IBaseDigit;
                                            Curr_OBJ.VDAOnDirectBasic = cp.VDAOnDirectBasic;
                                            Curr_OBJ.Id = data;
                                            Curr_OBJ.DBTrack = cp.DBTrack;


                                            db.CPIRule.Attach(Curr_OBJ);
                                            db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Modified;  
                                        var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, cp.DBTrack);
                                        DT_CPIRule DT_Corp = (DT_CPIRule)obj;
                                        DT_Corp.RoundingMethod_Id = blog.RoundingMethod == null ? 0 : blog.RoundingMethod.Id;
                                        //DT_Corp.PayScale_Id = blog.PayScale == null ? 0 : blog.PayScale.Id;

                                        db.Create(DT_Corp);
                                       
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = Curr_OBJ.Id, Val = Curr_OBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet); 
                                    }
                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Unit)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (CPIRule)databaseEntry.ToObject();
                                    cp.RowVersion = databaseValues.RowVersion;

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

                            CPIRule blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            CPIRule Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.CPIRule.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            cp.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            CPIRule cpirule = new CPIRule()
                            {
                                Name = cp.Name,
                                CPIRuleDetails = cp.CPIRuleDetails,
                                CPIUnitCalc = cp.CPIUnitCalc,
                                RoundingMethod = cp.RoundingMethod,
                                IBaseDigit = cp.IBaseDigit,
                                //MaxAmountIBase = cp.MaxAmountIBase,
                                //MinAmountIBase = cp.MinAmountIBase,
                                Id = data,
                                DBTrack = cp.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Payroll", "M", blog, cpirule, "CPIRule", cp.DBTrack);
                                Old_Corp = context.CPIRule.Where(e => e.Id == data).Include(e => e.CPIRuleDetails)
                                    .Include(e => e.CPIUnitCalc).Include(e => e.RoundingMethod).SingleOrDefault();
                                DT_CPIRule DT_Corp = (DT_CPIRule)obj;
                                DT_Corp.CPIRuleDetails_Id = DBTrackFile.ValCompare(Old_Corp.CPIRuleDetails, cp.CPIRuleDetails);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.PayScale_Id = DBTrackFile.ValCompare(Old_Corp.PayScale, c.PayScale); //Old_Corp.PayScale == c.PayScale ? 0 : Old_Corp.PayScale == null && c.PayScale != null ? c.PayScale.Id : Old_Corp.PayScale.Id;

                                db.Create(DT_Corp);

                            }
                            blog.DBTrack = cp.DBTrack;
                            db.CPIRule.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = cp.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, cp.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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


        //public ActionResult GetCPIDTLKDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.CPIRuleDetails.ToList();
        //        IEnumerable<CPIRuleDetails> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.CPIRuleDetails.ToList().Where(d => d.FullDetails.Contains(data));

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


        public ActionResult GetCPIDTLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.CPIRuleDetails.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.CPIRuleDetails.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.CPIRule.SelectMany(e => e.CPIRuleDetails).ToList();
                var list2 = fall.Except(list1);

                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }

        //public ActionResult GetCPIUnitCalc(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.CPIUnitCalc.ToList();
        //        IEnumerable<CPIUnitCalc> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.CPIUnitCalc.ToList();

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

        public ActionResult GetCPIUnitCalc(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.CPIUnitCalc.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.CPIUnitCalc.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.CPIRule.SelectMany(e => e.CPIUnitCalc).ToList();
                var list2 = fall.Except(list1);

                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);

            }
            // return View();
        }
        public class GridClass
        {
            public double MaxAmountIBase { get; set; }
            public int IBaseDigit { get; set; }
            public double MinAmountIBase { get; set; }
            public string Name { get; set; }
            public int Id { get; set; }
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
                ///var ITSection10 = db.CPIRule.ToList();
                IEnumerable<GridClass> CPIRule = null;
                var compid = int.Parse(Session["CompId"].ToString());
                var data = db.CompanyPayroll.Include(e => e.CPIRule).Where(e => e.Company.Id == compid).Select(e => e.CPIRule).SingleOrDefault();
                List<GridClass> model = new List<GridClass>();
                foreach (var item in data)
                {
                    model.Add(new GridClass
                    {
                        Id = item.Id,
                        IBaseDigit = item.IBaseDigit,
                        //MaxAmountIBase = item.MaxAmountIBase,
                        //MinAmountIBase = item.MinAmountIBase,
                        Name = item.Name,
                    });
                }
                CPIRule = model;


                //if (gp.IsAutho == true)
                //{
                //    IE = db.CPIRule.Include(e => e.CPIRuleDetails).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                //}
                //else
                //{
                //    IE = db.CPIRule.Include(e => e.CPIRuleDetails).AsNoTracking().ToList();
                //}
                IEnumerable<GridClass> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = CPIRule;
                    if (gp.searchOper.Equals("eq"))
                    {
                       // jsonData = IE.Select(a => new Object[] { a.Id, a.Name, a.MinAmountIBase, a.MaxAmountIBase, a.IBaseDigit }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                              || (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                              //|| (e.MinAmountIBase.ToString().Contains(gp.searchString))
                              //|| (e.MaxAmountIBase.ToString().Contains(gp.searchString))
                              || (e.IBaseDigit.ToString().Contains(gp.searchString))
                              ).Select(a => new { a.Id, a.Name,  a.IBaseDigit }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Name, a.IBaseDigit, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = CPIRule;
                    Func<GridClass, string> orderfuc = (c =>
                                                               gp.sidx == "Id" ? c.Id.ToString() :
                                                               gp.sidx == "Name" ? c.Name.ToString() :
                                                               //gp.sidx == "MinAmountIBase" ? c.MinAmountIBase.ToString() :
                                                               //gp.sidx == "MaxAmountIBase" ? c.MaxAmountIBase.ToString() :
                                                               gp.sidx == "IBaseDigit" ? c.IBaseDigit.ToString() :
                                                                "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Name, a.IBaseDigit, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.Name, a.IBaseDigit, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Name, a.IBaseDigit, a.Id }).ToList();
                    }
                    totalRecords = CPIRule.Count();
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
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            	List<string> Msg = new List<string>();
                using (DataBaseContext db = new DataBaseContext())
                {
                    try
                    {
                        if (auth_action == "C")
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                //Corporate corp = db.Corporate.Find(auth_id);
                                //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                                CPIRule corp = db.CPIRule
                                    .Include(e => e.CPIRuleDetails)
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

                                db.CPIRule.Attach(corp);
                                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                //db.SaveChanges();
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, corp.DBTrack);
                                DT_CPIRule DT_Corp = (DT_CPIRule)rtn_Obj;

                                //DT_Corp.Hobby_Id = corp.Hobby == null ? 0 : corp.Hobby;
                                //DT_Corp.LanguageSkill_Id = corp.LanguageSkill == null ? 0 : corp.LanguageSkill;
                                db.Create(DT_Corp);
                                await db.SaveChangesAsync();

                                ts.Complete();
                                Msg.Add("  Record Authorised");
                                return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else if (auth_action == "M")
                        {

                            CPIRule Old_Corp = db.CPIRule
                                    .Include(e => e.CPIRuleDetails)
                                   .Where(e => e.Id == auth_id).SingleOrDefault();


                            DT_CPIRule Curr_Corp = db.DT_CPIRule
                                                        .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                        .OrderByDescending(e => e.Id)
                                                        .FirstOrDefault();

                            if (Curr_Corp != null)
                            {
                                CPIRule corp = new CPIRule();



                                if (ModelState.IsValid)
                                {
                                    try
                                    {

                                        //DbContextTransaction transaction = db.Database.BeginTransaction();

                                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                        {
                                            // db.Configuration.AutoDetectChangesEnabled = false;
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

                                            int a = EditS("", auth_id, corp, corp.DBTrack);

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
                                        var clientValues = (CPIRule)entry.Entity;
                                        var databaseEntry = entry.GetDatabaseValues();
                                        if (databaseEntry == null)
                                        {
                                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        }
                                        else
                                        {
                                            var databaseValues = (CPIRule)databaseEntry.ToObject();
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
                                //Corporate corp = db.Corporate.Find(auth_id);
                                CPIRule corp = db.CPIRule.AsNoTracking()
                                   .Include(e => e.CPIRuleDetails)
                                    .FirstOrDefault(e => e.Id == auth_id);

                                //Awards add = corp.Awards.ToString();
                                //Hobby conDet = corp.Hobby;
                                //LanguageSkill val = corp.LanguageSkill;

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

                                db.CPIRule.Attach(corp);
                                db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, corp.DBTrack);
                                DT_CPIRule DT_Corp = (DT_CPIRule)rtn_Obj;
                                //DT_Corp.SkillType_Id = corp.SkillType == null ? 0 : corp.SkillType.Id;
                                //DT_Corp.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                await db.SaveChangesAsync();
                                db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                                Msg.Add(" Record Authorised ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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


        public int EditS(string RoundingMethodlist, int data, CPIRule cp, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (RoundingMethodlist != null)
                {
                    if (RoundingMethodlist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(RoundingMethodlist));
                        cp.RoundingMethod = val;

                        var type = db.CPIRule.Include(e => e.RoundingMethod).Where(e => e.Id == data).SingleOrDefault();
                        IList<CPIRule> typedetails = null;
                        if (type.RoundingMethod != null)
                        {
                            typedetails = db.CPIRule.Where(x => x.RoundingMethod.Id == type.RoundingMethod.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.CPIRule.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.RoundingMethod = cp.RoundingMethod;
                            s.VDAOnDirectBasic = cp.VDAOnDirectBasic;
                            db.CPIRule.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.CPIRule.Include(e => e.RoundingMethod).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.RoundingMethod = null;
                            s.VDAOnDirectBasic = cp.VDAOnDirectBasic;
                            db.CPIRule.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }

                var CurCorp = db.CPIRule.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    cp.DBTrack = dbT;
                    CPIRule corp = new CPIRule()
                    {
                        Id = data,
                        Name = cp.Name,
                        CPIRuleDetails = cp.CPIRuleDetails,
                        CPIUnitCalc = cp.CPIUnitCalc,
                        IBaseDigit = cp.IBaseDigit,
                        VDAOnDirectBasic = cp.VDAOnDirectBasic,
                        //MinAmountIBase = cp.MinAmountIBase,
                        //MaxAmountIBase = cp.MaxAmountIBase,
                        RoundingMethod = cp.RoundingMethod,
                        DBTrack = cp.DBTrack
                    };

                    db.CPIRule.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

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
