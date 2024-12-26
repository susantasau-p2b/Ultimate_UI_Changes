using P2b.Global;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using P2BUltimate.App_Start;
using P2B.PFTRUST;
using System.Data.Entity;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace P2BUltimate.Controllers.PFTrust
{
    public class LoanAdvanceHeadPFTController : Controller
    {
        // GET: LoanAdvanceHeadPFT
        public ActionResult Index()
        {
            return View("~/Views/PFTrust/LoanAdvanceHeadPFT/index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/PFTrust/_LoanAdvanceHeadPFT.cshtml");
        }
        public ActionResult ITpartial()
        {
            return View("~/Views/Shared/Payroll/_itloan.cshtml");
        }


        public ActionResult PopulateSalHeadDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.SalaryHead.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulatePerkHeadDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "PERK").ToList();
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
        public ActionResult GetITSectionLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.ITSection.Include(e => e.ITSectionList).Include(e => e.ITSectionListType).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ITSection.Include(e => e.ITSectionList).Include(e => e.ITSectionListType).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetLoanAdvPolicyLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.LoanAdvancePolicyPFT.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LoanAdvancePolicyPFT.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                //var list1 = db.LoanAdvanceHeadPFT.Include(e=>e.LoanAdvancePolicyPFT).Select(e => e.LoanAdvancePolicyPFT).ToList();
                //var list2 = fall.Except(list1);

                //var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.PFLoanType.LookupVal }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetITLoanLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.ITLoan.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ITLoan.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(LoanAdvanceHeadPFT c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string SalaryHead = form["SalaryHeadlist"] == "0" ? "" : form["SalaryHeadlist"];
                    string LoanAdvancePolicy = form["LoanAdvancePolicylist"] == "0" ? "" : form["LoanAdvancePolicylist"];
                    string ITLoan = form["ITLoanlist"] == "0" ? "" : form["ITLoanlist"];

                    var id = int.Parse(Session["CompId"].ToString());
                    var CompanyPFTrust = db.CompanyPFTrust.Where(e => e.Company.Id == id).SingleOrDefault();

                    if (SalaryHead != null && SalaryHead != "")
                    {
                        var val = db.SalaryHead.Find(int.Parse(SalaryHead));
                        c.SalaryHead_Id = val.Id;
                    }


                    if (LoanAdvancePolicy != null && LoanAdvancePolicy != "")
                    {
                        var ids = Convert.ToInt32(LoanAdvancePolicy);
                        var LoanPolicy_val = db.LoanAdvancePolicyPFT.Where(e => e.Id == ids).SingleOrDefault();
                        c.LoanAdvancePolicyPFT = LoanPolicy_val;

                    }
                    else
                    {
                        c.LoanAdvancePolicyPFT = null;
                    }

                    if (ITLoan != null && ITLoan != "")
                    {
                        var ids = Convert.ToInt32(ITLoan);
                        var LoanPolicy_val = db.ITLoan.Find(ids);
                        c.ITLoan_Id = LoanPolicy_val.Id;

                    }
                    else
                    {
                        c.ITLoan_Id = null;
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.LoanAdvanceHead.Any(o => o.Code == c.Code))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            LoanAdvanceHeadPFT LoanAdvanceHead = new LoanAdvanceHeadPFT()
                            {
                                Code = c.Code == null ? "" : c.Code.Trim(),
                                Name = c.Name == null ? "" : c.Name.Trim(),
                                //ITSection = c.ITSection,
                                LoanAdvancePolicyPFT = c.LoanAdvancePolicyPFT,
                                SalaryHead_Id = c.SalaryHead_Id,
                                //PerkHead = c.PerkHead,
                                ITLoan_Id = c.ITLoan_Id,
                                //  CompanyPFTrust_Id= CompanyPFTrust,

                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.LoanAdvanceHeadPFT.Add(LoanAdvanceHead);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
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

        public class LAHCollection
        {
            public Array ITLoan_Id { get; set; }
            public Array ITLoan_FullDetails { get; set; }
            public Array LoanAdvancePolicyPFT_Id { get; set; }
            public string LoanAdvancePolicyPFT_FullDetails { get; set; }


        }
        [HttpPost]

        public ActionResult EditsaveLoanAdvancePolicyPFT(LoanAdvancePolicyPFT ir, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();

            string EffectiveDate = form["EffectiveDate"] == "0" ? "0" : form["EffectiveDate"];
            var effFrom = Convert.ToDateTime(EffectiveDate);
            ir.EffectiveDate = effFrom;

            string YrsOfServ = form["YrsOfServ"] == "0" ? "0" : form["YrsOfServ"];
            ir.YrsOfServ = Convert.ToInt32(YrsOfServ);

            string IsLoanLimit = form["IsLoanLimit"] == "0" ? "0" : form["IsLoanLimit"];
            ir.IsLoanLimit = Convert.ToBoolean(IsLoanLimit);

            string IsFixAmount = form["IsFixAmount"] == "0" ? "0" : form["IsFixAmount"];
            ir.IsFixAmount = Convert.ToBoolean(IsFixAmount);

            string MaxLoanAmount = form["MaxLoanAmount"] == "0.0" ? "0" : form["MaxLoanAmount"];
            ir.MaxLoanAmount = Convert.ToDouble(MaxLoanAmount);

            string IsOnWages = form["IsOnWages"] == "0" ? "0" : form["IsOnWages"];
            ir.IsOnWages = Convert.ToBoolean(IsOnWages);

            string IntAppl = form["IntAppl"] == "0" ? "0" : form["IntAppl"];
            ir.IntAppl = Convert.ToBoolean(IntAppl);

            string IntRate = form["IntRate"] == "0.0" ? "0" : form["IntRate"];
            ir.IntRate = Convert.ToDouble(IntRate);

            string IsPerkOnInt = form["IsPerkOnInt"] == "0" ? "0" : form["IsPerkOnInt"];
            ir.IsPerkOnInt = Convert.ToBoolean(IsPerkOnInt);

            string GovtIntRate = form["GovtIntRate"] == "0" ? "0" : form["GovtIntRate"];
            ir.GovtIntRate = Convert.ToDouble(GovtIntRate);

            string IsPFTContibution = form["IsPFTContibution"] == "0" ? "0" : form["IsPFTContibution"];
            ir.IsPFTContibution = Convert.ToBoolean(IsPFTContibution);

            string IsOwnContibution = form["IsOwnContibution"] == "0" ? "0" : form["IsOwnContibution"];
            ir.IsOwnContibution = Convert.ToBoolean(IsOwnContibution);

            string IsOwnerContibution = form["IsOwnerContibution"] == "0" ? "0" : form["IsOwnerContibution"];
            ir.IsOwnerContibution = Convert.ToBoolean(IsOwnerContibution);

            string IsOwnerIntContibution = form["IsOwnerIntContibution"] == "0" ? "0" : form["IsOwnerIntContibution"];
            ir.IsOwnerIntContibution = Convert.ToBoolean(IsOwnerIntContibution);

            string IsOwnIntContibution = form["IsOwnIntContibution"] == "0" ? "0" : form["IsOwnIntContibution"];
            ir.IsOwnIntContibution = Convert.ToBoolean(IsOwnIntContibution);

            string IsVPFIntContibution = form["IsVPFIntContibution"] == "0" ? "0" : form["IsVPFIntContibution"];
            ir.IsVPFIntContibution = Convert.ToBoolean(IsVPFIntContibution);

            string IsPFIntContibution = form["IsPFIntContibution"] == "0" ? "0" : form["IsPFIntContibution"];
            ir.IsPFIntContibution = Convert.ToBoolean(IsPFIntContibution);

            string NoOfTimesPFTContribution = form["NoOfTimesPFTContribution"] == "0" ? "0" : form["NoOfTimesPFTContribution"];
            ir.NoOfTimesPFTContribution = Convert.ToInt32(NoOfTimesPFTContribution);

            string InterestTpyelist = form["InterestTpyelist"] == "0" ? "" : form["InterestTpyelist"];
            string PFLoanTypelist = form["PFLoanType"] == "0" ? "" : form["PFLoanType"];
            string LoanSanctionWages = form["WagesList"] == "0" ? "" : form["WagesList"];

            ir.InterestType_Id = InterestTpyelist != null && InterestTpyelist != "" ? int.Parse(InterestTpyelist) : 0;
            ir.PFLoanType_Id = PFLoanTypelist != null && PFLoanTypelist != "" ? int.Parse(PFLoanTypelist) : 0;
            ir.LoanSanctionWages_Id = LoanSanctionWages != null && LoanSanctionWages != "" ? int.Parse(LoanSanctionWages) : 0;
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.LoanAdvancePolicyPFT.Include(e => e.InterestType).Include(e => e.PFLoanType).Where(e => e.Id == data).SingleOrDefault();

                        if (ir.InterestType_Id == 0)
                        {
                            db_data.InterestType = null;
                        }
                        if (ir.PFLoanType_Id == 0)
                        {
                            db_data.PFLoanType_Id = null;
                        }
                        if (ir.LoanSanctionWages_Id == 0)
                        {
                            db_data.LoanSanctionWages_Id = null;
                        }

                        db.LoanAdvancePolicyPFT.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        TempData["RowVersion"] = db_data.RowVersion;
                        LoanAdvancePolicyPFT LoanAdvancePolicyPFTRate = db.LoanAdvancePolicyPFT.Find(data);
                        TempData["CurrRowVersion"] = LoanAdvancePolicyPFTRate.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            ir.DBTrack = new DBTrack
                            {
                                CreatedBy = LoanAdvancePolicyPFTRate.DBTrack.CreatedBy == null ? null : LoanAdvancePolicyPFTRate.DBTrack.CreatedBy,
                                CreatedOn = LoanAdvancePolicyPFTRate.DBTrack.CreatedOn == null ? null : LoanAdvancePolicyPFTRate.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (ir.InterestType_Id != 0)
                            {
                                LoanAdvancePolicyPFTRate.InterestType_Id = ir.InterestType_Id != null ? ir.InterestType_Id : 0;
                            }

                            if (ir.PFLoanType_Id != 0)
                            {
                                LoanAdvancePolicyPFTRate.PFLoanType_Id = ir.PFLoanType_Id != null ? ir.PFLoanType_Id : 0;
                            }

                            if (ir.LoanSanctionWages_Id != 0)
                            {
                                LoanAdvancePolicyPFTRate.LoanSanctionWages_Id = ir.LoanSanctionWages_Id != null ? ir.LoanSanctionWages_Id : 0;
                            }

                            LoanAdvancePolicyPFTRate.Name = ir.Name;
                            LoanAdvancePolicyPFTRate.EffectiveDate = ir.EffectiveDate;
                            LoanAdvancePolicyPFTRate.YrsOfServ = ir.YrsOfServ;
                            LoanAdvancePolicyPFTRate.IsLoanLimit = ir.IsLoanLimit;
                            LoanAdvancePolicyPFTRate.IsFixAmount = ir.IsFixAmount;
                            LoanAdvancePolicyPFTRate.MaxLoanAmount = ir.MaxLoanAmount;
                            LoanAdvancePolicyPFTRate.IsOnWages = ir.IsOnWages;
                            LoanAdvancePolicyPFTRate.IntAppl = ir.IntAppl;
                            LoanAdvancePolicyPFTRate.IsPerkOnInt = ir.IsPerkOnInt;
                            LoanAdvancePolicyPFTRate.GovtIntRate = ir.GovtIntRate;
                            LoanAdvancePolicyPFTRate.IsPFTContibution = ir.IsPFTContibution;
                            LoanAdvancePolicyPFTRate.IsOwnContibution = ir.IsOwnContibution;
                            LoanAdvancePolicyPFTRate.IsOwnerContibution = ir.IsOwnerContibution;
                            LoanAdvancePolicyPFTRate.IsOwnerIntContibution = ir.IsOwnerIntContibution;
                            LoanAdvancePolicyPFTRate.IsOwnIntContibution = ir.IsOwnIntContibution;
                            LoanAdvancePolicyPFTRate.IsVPFIntContibution = ir.IsVPFIntContibution;
                            LoanAdvancePolicyPFTRate.IsPFIntContibution = ir.IsPFIntContibution;
                            LoanAdvancePolicyPFTRate.NoOfTimesPFTContribution = ir.NoOfTimesPFTContribution;
                            LoanAdvancePolicyPFTRate.PFTContibutionPercent = ir.PFTContibutionPercent;
                            LoanAdvancePolicyPFTRate.OwnContibutionPercent = ir.OwnContibutionPercent;
                            LoanAdvancePolicyPFTRate.OwnerContibutionPercent = ir.OwnerContibutionPercent;
                            LoanAdvancePolicyPFTRate.OwnIntContibutionPercent = ir.OwnIntContibutionPercent;
                            LoanAdvancePolicyPFTRate.OwnerIntContibutionPercent = ir.OwnerIntContibutionPercent;
                            LoanAdvancePolicyPFTRate.PFIntContibutionPercent = ir.PFIntContibutionPercent;
                            LoanAdvancePolicyPFTRate.VPFIntContibutionPercent = ir.VPFIntContibutionPercent;
                            LoanAdvancePolicyPFTRate.Id = data;
                            LoanAdvancePolicyPFTRate.DBTrack = ir.DBTrack;
                            db.LoanAdvancePolicyPFT.Attach(LoanAdvancePolicyPFTRate);
                            db.Entry(LoanAdvancePolicyPFTRate).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = ir.Id, Val = ir.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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


        public ActionResult EditLoanAdvancePolicyPFT_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = (from c in db.LoanAdvancePolicyPFT.Include(e => e.InterestType).Include(e => e.PFLoanType)
                         .Where(e => e.Id == data)
                         select new
                         {
                             Name = c.Name,
                             EffectiveDate = c.EffectiveDate,
                             YrsOfServ = c.YrsOfServ,
                             IsLoanLimit = c.IsLoanLimit,
                             IsFixAmount = c.IsFixAmount,
                             MaxLoanAmount = c.MaxLoanAmount,
                             IsOnWages = c.IsOnWages,
                             LoanSanctionWages = c.LoanSanctionWages_Id,
                             IntAppl = c.IntAppl,
                             InterestType_Id = c.InterestType_Id,
                             IntRate = c.IntRate,
                             IsPerkOnInt = c.IsPerkOnInt,
                             GovtIntRate = c.GovtIntRate,
                             IsPFTContibution = c.IsPFTContibution,
                             IsOwnContibution = c.IsOwnContibution,
                             IsOwnerContibution = c.IsOwnerContibution,
                             PFLoanType_Id = c.PFLoanType_Id,
                             IsOwnerIntContibution = c.IsOwnerIntContibution,
                             IsOwnIntContibution = c.IsOwnIntContibution,
                             IsVPFIntContibution = c.IsVPFIntContibution,
                             IsPFIntContibution = c.IsPFIntContibution,
                             PFTContibutionPercent = c.PFTContibutionPercent,
                             OwnContibutionPercent = c.OwnContibutionPercent,
                             OwnerContibutionPercent = c.OwnerContibutionPercent,
                             OwnIntContibutionPercent = c.OwnIntContibutionPercent,
                             OwnerIntContibutionPercent = c.OwnerIntContibutionPercent,
                             PFIntContibutionPercent = c.PFIntContibutionPercent,
                             VPFIntContibutionPercent = c.VPFIntContibutionPercent,
                             NoOfTimesPFTContribution = c.NoOfTimesPFTContribution,

                             Id = data
                         }).ToList();

                var sanwageid = db.LoanAdvancePolicyPFT.Where(e => e.Id == data).SingleOrDefault();
            
                if (sanwageid != null)
                {
                   var  q = (from c in db.Wages
                       .Where(e => e.Id == sanwageid.LoanSanctionWages_Id)
                             select new
                             {
                                 Wagesid=c.Id,
                                 LoanSanctionWages = c.FullDetails,
                             }).ToList();
                     return Json(new object[] { r, q }, JsonRequestBehavior.AllowGet);
                }
                return Json(new object[] { r, null }, JsonRequestBehavior.AllowGet);


                
            }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.LoanAdvanceHeadPFT
                    .Include(e => e.SalaryHead_Id)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Code = e.Code,
                        Name = e.Name,
                        SalaryHead_Id = e.SalaryHead_Id == null ? 0 : e.SalaryHead_Id,
                        //ITLoan_FullDetails = e.ITLoan.FullDetails == null ? "" : e.ITLoan.FullDetails,
                        ITLoan_Id = e.ITLoan_Id == null ? "" : e.ITLoan_Id.ToString(),
                        Action = e.DBTrack.Action,

                    }).ToList();
                var ITLoan_Id = db.LoanAdvanceHeadPFT.Where(e => e.Id == data).Select(e => e.ITLoan_Id).FirstOrDefault();
                var OBJ = db.ITLoan.Where(e => e.Id == ITLoan_Id).Select(r => r.FullDetails.ToString()).ToArray();
                List<LAHCollection> objLAH = new List<LAHCollection>();
                var LAHP = db.LoanAdvanceHeadPFT
                          .Where(e => e.Id == data)
                          .Include(e => e.LoanAdvancePolicyPFT)
                    //.Include(e => e.LoanAdvancePolicyPFT.Select(f => f.PFLoanType))
                          .Include(e => e.LoanAdvancePolicyPFT.PFLoanType)
                    //.Select(e => e.LoanAdvancePolicyPFT)
                          .ToList();
                foreach (var ca in LAHP)
                {


                    //foreach (var item in ca.LoanAdvancePolicyPFT)
                    //{
                    objLAH.Add(new LAHCollection
                    {
                        LoanAdvancePolicyPFT_Id = ca.LoanAdvancePolicyPFT.Id.ToString().ToArray(),
                        LoanAdvancePolicyPFT_FullDetails = (ca.LoanAdvancePolicyPFT.PFLoanType.LookupVal.ToString()),
                        ITLoan_Id = ca.ITLoan_Id.ToString().ToArray(),
                        ITLoan_FullDetails = OBJ




                    });
                    // }


                }




                //var LAHITS = db.LoanAdvanceHead.Where(e => e.Id == data)
                //               .Include(e => e.ITSection)
                //               .Select(e => e.ITSection)
                //               .ToList();
                //foreach (var ca in LAHITS)
                //{
                //    objLAH.Add(new LAHCollection
                //    {
                //        ITSection_Id = ca.Select(e => e.Id).ToArray(),
                //        ITsection_FullDetails = ca.Select(e => e.FullDetails).ToArray()



                //    });

                //}


                //var add_data = db.LoanAdvanceHead
                //  .Include(e => e.ITLoan)
                //    .Include(e => e.ITSection)
                //    .Include(e => e.LoanAdvancePolicy)
                //    .Where(e => e.Id == data)
                //    .Select(e => new
                //    {
                //        ITLoan_FullDetails = e.ITLoan.FullDetails == null ? "" : e.ITLoan.FullDetails,
                //        ITLoan_Id = e.ITLoan.Id == null ? "" : e.ITLoan.Id.ToString(),
                //        ITSection_FullDetails = e.ITSection.Select(r => r.FullDetails),
                //        ITSection_Id = e.ITSection.Select(r => r.Id),
                //        LoanAdvPolicy_FullDetails = e.LoanAdvancePolicy.Select(r => r.FullDetails),
                //        LoanAdvPolicy_Id = e.LoanAdvancePolicy.Select(r => r.Id)
                //    }).ToList();

                //var W = db.DT_LoanAdvanceHead
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         Code = e.Code == null ? "" : e.Code,
                //         Name = e.Name == null ? "" : e.Name,
                //         SalHead_Val = e.SalaryHead_Id == 0 ? "" : db.SalaryHead.Where(x => x.Id == e.SalaryHead_Id).Select(x => x.Name).FirstOrDefault()//,
                //         //LoanSanctionWages_Val = e.LoanSanctionWages_Id == 0 ? "" : db.Wages.Where(x => x.Id == e.LoanSanctionWages_Id).Select(x => x.FullDetails).FirstOrDefault(),
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.LoanAdvanceHeadPFT.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, objLAH, OBJ, JsonRequestBehavior.AllowGet });
            }
        }
        //==================================================================================================

        //[HttpPost]
        //public ActionResult Edit(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        var Q = db.LoanAdvanceHeadPFT
        //            .Include(e => e.SalaryHead_Id)
        //            .Where(e => e.Id == data).Select
        //            (e => new
        //            {
        //                Code = e.Code,
        //                Name = e.Name,
        //                SalaryHead_Id = e.SalaryHead_Id == null ? 0 : e.SalaryHead_Id,
        //                //ITLoan_FullDetails = e.ITLoan.FullDetails == null ? "" : e.ITLoan.FullDetails,
        //                ITLoan_Id = e.ITLoan_Id == null ? "" : e.ITLoan_Id.ToString(),
        //                Action = e.DBTrack.Action,

        //            }).ToList();
        //        var ITLoan_Id =db.LoanAdvanceHeadPFT.Where(e=>e.Id==data).Select(e=>e.ITLoan_Id).FirstOrDefault();
        //        var OBJ = db.ITLoan.Where(e => e.Id == ITLoan_Id).Select(r => r.FullDetails.ToString()).ToArray();
        //        List<LAHCollection> objLAH = new List<LAHCollection>();
        //        var LAHP = db.LoanAdvanceHeadPFT
        //                  .Where(e => e.Id == data)
        //                  .Include(e => e.LoanAdvancePolicyPFT)
        //                  .Include(e => e.ITLoan_Id)
        //                  .Select(e => e.LoanAdvancePolicyPFT)
        //                  .ToList();
        //        foreach (var ca in LAHP)
        //        {

        //            objLAH.Add(new LAHCollection
        //            {
        //                LoanAdvancePolicyPFT_Id = ca.Select(e => e.Id).ToArray(),
        //                LoanAdvancePolicyPFT_FullDetails = ca.Select(e => e.IsPerkOnInt).ToArray(),
        //                ITLoan_Id = ca.Select(e => e.Id).ToArray(),
        //                ITLoan_FullDetails = OBJ

        //            }) ; 


        //        }

        //        var Corp = db.LoanAdvanceHeadPFT.Find(data);
        //        TempData["RowVersion"] = Corp.RowVersion;
        //        var Auth = Corp.DBTrack.IsModified;
        //        return Json(new Object[] { Q, objLAH, OBJ, JsonRequestBehavior.AllowGet });
        //    }
        //}

        [HttpPost]
        public ActionResult EditSave(LoanAdvanceHeadPFT head, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    string SalaryHead = form["SalaryHeadlist"] == "0" ? "" : form["SalaryHeadlist"];
                    string LoanAdvancePolicy = form["LoanAdvancePolicylist"] == "0" ? "" : form["LoanAdvancePolicylist"];
                    string ITLoan = form["ITLoanlist"] == "0" ? "" : form["ITLoanlist"];

                    head.SalaryHead_Id = SalaryHead != null && SalaryHead != "" ? int.Parse(SalaryHead) : 0;
                    head.ITLoan_Id = ITLoan != null && ITLoan != "" ? int.Parse(ITLoan) : 0;

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    LoanAdvanceHeadPFT blog = null; // to retrieve old data

                                    blog = db.LoanAdvanceHeadPFT.Where(e => e.Id == data)
                                                            .SingleOrDefault();

                                    head.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };


                                    var db_data = db.LoanAdvanceHeadPFT.Include(e => e.LoanAdvancePolicyPFT).Where(e => e.Id == data).SingleOrDefault();
                                    if (LoanAdvancePolicy != null && LoanAdvancePolicy != "")
                                    {
                                        var ids = Convert.ToInt32(LoanAdvancePolicy);
                                        var value = db.LoanAdvancePolicyPFT.Find(ids);
                                        db_data.LoanAdvancePolicyPFT = value;
                                    }
                                    else
                                    {
                                        db_data.LoanAdvancePolicyPFT = null;
                                    }

                                    TempData["RowVersion"] = db_data.RowVersion;

                                    var CurCorp = db.LoanAdvanceHeadPFT.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        CurCorp.Code = head.Code;
                                        CurCorp.Name = head.Name;
                                        CurCorp.Id = data;
                                        CurCorp.DBTrack = head.DBTrack;

                                        if (head.ITLoan_Id != 0)
                                            CurCorp.ITLoan_Id = head.ITLoan_Id;
                                        else
                                            CurCorp.ITLoan_Id = null;

                                        if (head.SalaryHead_Id != 0)
                                            CurCorp.SalaryHead_Id = head.SalaryHead_Id;
                                        else
                                            CurCorp.SalaryHead_Id = null;

                                        CurCorp.LoanAdvancePolicyPFT = db_data.LoanAdvancePolicyPFT;

                                        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }

                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = head.Id, Val = head.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (LoanAdvanceHead)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (LoanAdvanceHead)databaseEntry.ToObject();
                                    head.RowVersion = databaseValues.RowVersion;

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

                            LoanAdvanceHeadPFT blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            LoanAdvanceHead Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.LoanAdvanceHeadPFT.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            head.DBTrack = new DBTrack
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

                            LoanAdvanceHeadPFT corp = new LoanAdvanceHeadPFT()
                            {
                                Code = head.Code,
                                ITLoan_Id = head.ITLoan_Id,
                                //ITSection_ = L.ITSection,
                                LoanAdvancePolicyPFT = head.LoanAdvancePolicyPFT,
                                SalaryHead_Id = head.SalaryHead_Id,
                                Id = data,
                                DBTrack = head.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            //using (var context = new DataBaseContext())
                            //{
                            //    var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, L.DBTrack);
                            //    DT_LoanAdvanceHead DT_LoanAdvHead = (DT_LoanAdvanceHead)obj;
                            //    db.Create(DT_LoanAdvHead);
                            //    db.SaveChanges();
                            //}
                            blog.DBTrack = head.DBTrack;
                            db.LoanAdvanceHeadPFT.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = head.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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

        //[HttpPost]
        //public async Task<ActionResult> EditSave(LoanAdvanceHead c, int data, FormCollection form) // Edit submit
        //{

        //    bool Auth = form["Autho_Allow"] == "true" ? true : false;

        //    string SalaryHead = form["SalaryHeadlist"] == "0" ? "" : form["SalaryHeadlist"];
        //    string LoanAdvancePolicy = form["LoanAdvancePolicylist"] == "0" ? "" : form["LoanAdvancePolicylist"];
        //    string ITLoan = form["ITLoanlist"] == "0" ? "" : form["ITLoanlist"];
        //    string ITSection = form["ITSectionlist"] == "0" ? "" : form["ITSectionlist"];

        //    var db_Data = db.LoanAdvanceHead.Include(e => e.SalaryHead).Include(e => e.ITLoan).Include(e => e.ITSection).Include(e => e.LoanAdvancePolicy)
        //        .Where(e => e.Id == data).SingleOrDefault();
        //    db_Data.SalaryHead = null;
        //    db_Data.ITLoan = null;
        //    db_Data.ITSection = null;
        //    db_Data.LoanAdvancePolicy = null;

        //    if (SalaryHead != null && SalaryHead != "")
        //    {
        //        var val = db.SalaryHead.Find(int.Parse(SalaryHead));
        //        c.SalaryHead = val;
        //    }

        //    if (LoanAdvancePolicy != null && LoanAdvancePolicy != "")
        //    {
        //        var ids = Utility.StringIdsToListIds(LoanAdvancePolicy);
        //        List<LoanAdvancePolicy> lookupval = new List<LoanAdvancePolicy>();
        //        foreach (var ca in ids)
        //        {
        //            var LoanPolicy_val = db.LoanAdvancePolicy.Find(ca);
        //            lookupval.Add(LoanPolicy_val);
        //            c.LoanAdvancePolicy = lookupval;
        //        }
        //    }


        //    if (ITSection != null && ITSection != "")
        //    {
        //        var ids = Utility.StringIdsToListIds(ITSection);
        //        List<ITSection> lookupval = new List<ITSection>();
        //        foreach (var ca in ids)
        //        {
        //            var ITSection_val = db.ITSection.Find(ca);
        //            lookupval.Add(ITSection_val);
        //            c.ITSection = lookupval;
        //        }
        //    }


        //    if (ITLoan != null && ITLoan != "")
        //    {
        //        var val = db.ITLoan.Find(int.Parse(ITLoan));
        //        c.ITLoan = val;
        //    }


        //    if (Auth == false)
        //    {
        //        if (ModelState.IsValid)
        //        {

        //            try
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    db.LoanAdvanceHead.Attach(db_Data);
        //                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = db_Data.RowVersion;
        //                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

        //                    var Curr_OBJ = db.LoanAdvanceHead.Find(data);
        //                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
        //                    db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

        //                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                    {
        //                        LoanAdvanceHead blog = blog = null;
        //                        DbPropertyValues originalBlogValues = null;

        //                        using (var context = new DataBaseContext())
        //                        {
        //                            blog = context.LoanAdvanceHead.Where(e => e.Id == data).SingleOrDefault();
        //                            originalBlogValues = context.Entry(blog).OriginalValues;
        //                        }

        //                        c.DBTrack = new DBTrack
        //                        {
        //                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                            Action = "M",
        //                            ModifiedBy = SessionManager.UserName,
        //                            ModifiedOn = DateTime.Now
        //                        };
        //                        LoanAdvanceHead LoanAdvHead = new LoanAdvanceHead
        //                        {
        //                            Id = data,
        //                            SalaryHead = db_Data.SalaryHead,
        //                            ITLoan = db_Data.ITLoan,
        //                            ITSection = db_Data.ITSection,
        //                            LoanAdvancePolicy = db_Data.LoanAdvancePolicy,
        //                            Code = c.Code,
        //                            Name = c.Name,
        //                            DBTrack = c.DBTrack
        //                        };


        //                        db.LoanAdvanceHead.Attach(LoanAdvHead);
        //                        db.Entry(LoanAdvHead).State = System.Data.Entity.EntityState.Modified;
        //                        db.Entry(LoanAdvHead).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                        var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                        DT_LoanAdvanceHead DT_LoanAdvHead = (DT_LoanAdvanceHead)obj;
        //                        db.Create(DT_LoanAdvHead);
        //                        db.SaveChanges();

        //                        await db.SaveChangesAsync();
        //                        db.Entry(LoanAdvHead).State = System.Data.Entity.EntityState.Detached;
        //                        ts.Complete();
        //                        return Json(new Object[] { LoanAdvHead.Id, LoanAdvHead.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
        //                    }
        //                }
        //            }

        //            catch (DbUpdateException e) { throw e; }
        //            catch (DataException e) { throw e; }
        //        }
        //        else
        //        {
        //            StringBuilder sb = new StringBuilder("");
        //            foreach (ModelState modelState in ModelState.Values)
        //            {
        //                foreach (ModelError error in modelState.Errors)
        //                {
        //                    sb.Append(error.ErrorMessage);
        //                    sb.Append("." + "\n");
        //                }
        //            }
        //            var errorMsg = sb.ToString();
        //            Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    else
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {

        //            LoanAdvanceHead blog = null; // to retrieve old data
        //            DbPropertyValues originalBlogValues = null;
        //            LoanAdvanceHead Old_OBJ = null;

        //            using (var context = new DataBaseContext())
        //            {
        //                blog = context.LoanAdvanceHead
        //                    .Where(e => e.Id == data).SingleOrDefault();
        //                TempData["RowVersion"] = blog.RowVersion;
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
        //            LoanAdvanceHead LoanAdvHead = new LoanAdvanceHead()
        //            {
        //                SalaryHead = db_Data.SalaryHead,
        //                ITLoan = db_Data.ITLoan,
        //                ITSection = db_Data.ITSection,
        //                LoanAdvancePolicy = db_Data.LoanAdvancePolicy,
        //                Code = blog.Code,
        //                Name = blog.Name,
        //                Id = data,
        //                DBTrack = c.DBTrack,
        //                RowVersion = (Byte[])TempData["RowVersion"]
        //            };


        //            db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;
        //            db.LoanAdvanceHead.Attach(LoanAdvHead);
        //            db.Entry(LoanAdvHead).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(LoanAdvHead).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            db.SaveChanges();
        //            TempData["RowVersion"] = db_Data.RowVersion;
        //            db.Entry(LoanAdvHead).State = System.Data.Entity.EntityState.Detached;

        //            using (var context = new DataBaseContext())
        //            {
        //                var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, c, "LoanAdvanceHead", c.DBTrack);
        //                Old_OBJ = context.LoanAdvanceHead.Where(e => e.Id == data).Include(e => e.ITLoan)
        //                    .Include(e => e.ITSection).Include(e => e.LoanAdvancePolicy).Include(e => e.SalaryHead).SingleOrDefault();
        //                DT_LoanAdvanceHead DT_OBJ = (DT_LoanAdvanceHead)obj;
        //                DT_OBJ.SalaryHead_Id = DBTrackFile.ValCompare(Old_OBJ.SalaryHead, c.SalaryHead);//Old_OBJ.Address == c.Address ? 0 : Old_OBJ.Address == null && c.Address != null ? c.Address.Id : Old_OBJ.Address.Id;
        //                DT_OBJ.ITLoan_Id = DBTrackFile.ValCompare(Old_OBJ.ITLoan, c.ITLoan); //Old_OBJ.BusinessType == c.BusinessType ? 0 : Old_OBJ.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_OBJ.BusinessType.Id;
        //                DT_OBJ.LoanAdvancePolicy_Id = DBTrackFile.ValCompare(Old_OBJ.LoanAdvancePolicy, c.LoanAdvancePolicy); //Old_OBJ.ContactDetails == c.ContactDetails ? 0 : Old_OBJ.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_OBJ.ContactDetails.Id;
        //                db.Create(DT_OBJ);
        //            }
        //            ts.Complete();
        //            return Json(new Object[] { blog.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
        //        }

        //    }

        //    return View();
        //}



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

                            LoanAdvanceHead corp = db.LoanAdvanceHead.Include(e => e.ITLoan)
                                .Include(e => e.ITSection).Include(e => e.LoanAdvancePolicy).Include(e => e.SalaryHead)
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

                            db.LoanAdvanceHead.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, corp.DBTrack);
                            DT_LoanAdvanceHead DT_Corp = (DT_LoanAdvanceHead)rtn_Obj;
                            DT_Corp.SalaryHead_Id = corp.SalaryHead == null ? 0 : corp.SalaryHead.Id;
                            DT_Corp.ITLoan_Id = corp.ITLoan == null ? 0 : corp.ITLoan.Id;
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

                        //LoanAdvanceHead Old_Corp = db.LoanAdvanceHead.Include(e => e.InterestTpye)
                        //                                  .Include(e => e.LoanSanctionWages)
                        //                                  .Where(e => e.Id == auth_id).SingleOrDefault();


                        //DT_LoanAdvanceHead Curr_Corp = db.DT_LoanAdvanceHead
                        //                            .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                        //                            .OrderByDescending(e => e.Id)
                        //                            .FirstOrDefault();

                        //if (Curr_Corp != null)
                        //{
                        //    LoanAdvanceHead corp = new LoanAdvanceHead();

                        //    string interesttpye = Curr_Corp.InterestTpye_Id == null ? null : Curr_Corp.InterestTpye_Id.ToString();
                        //    string loansanctionwages = Curr_Corp.LoanSanctionWages_Id == null ? null : Curr_Corp.LoanSanctionWages_Id.ToString();
                        //    corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                        //    corp.Code = Curr_Corp.Code == null ? Old_Corp.Code : Curr_Corp.Code;
                        //        corp.FineAmt = Curr_Corp.FineAmt;
                        //        corp.GovtIntRate = Curr_Corp.GovtIntRate;
                        //        corp.IntAppl = Curr_Corp.IntAppl;
                        //        corp.IntRate = Curr_Corp.IntRate;
                        //        corp.IsFine = Curr_Corp.IsFine;
                        //        corp.IsFixAmount = Curr_Corp.IsFixAmount;
                        //        corp.IsLoanLimit = Curr_Corp.IsLoanLimit;
                        //        corp.IsOnWages = Curr_Corp.IsOnWages;
                        //        corp.IsPerkOnInt = Curr_Corp.IsPerkOnInt;
                        //        corp.MaxLoanAmount = Curr_Corp.MaxLoanAmount;
                        //        corp.NoOfTimes = Curr_Corp.NoOfTimes;
                        //        corp.YrsOfServ = Curr_Corp.YrsOfServ;

                        //    if (ModelState.IsValid)
                        //    {
                        //        try
                        //        {


                        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        //            {
                        //                corp.DBTrack = new DBTrack
                        //                {
                        //                    CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                        //                    CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                        //                    Action = "M",
                        //                    ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                        //                    ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                        //                    AuthorizedBy = SessionManager.UserName,
                        //                    AuthorizedOn = DateTime.Now,
                        //                    IsModified = false
                        //                };

                        //                int a = EditS(interesttpye, loansanctionwages, auth_id, corp, corp.DBTrack);
                        //                await db.SaveChangesAsync();
                        //                ts.Complete();
                        //                return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        //            }
                        //        }
                        //        catch (DbUpdateConcurrencyException ex)
                        //        {
                        //            var entry = ex.Entries.Single();
                        //            var clientValues = (LoanAdvanceHead)entry.Entity;
                        //            var databaseEntry = entry.GetDatabaseValues();
                        //            if (databaseEntry == null)
                        //            {
                        //                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                        //            }
                        //            else
                        //            {
                        //                var databaseValues = (LoanAdvanceHead)databaseEntry.ToObject();
                        //                corp.RowVersion = databaseValues.RowVersion;
                        //            }
                        //        }

                        //        return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        //    }
                        //}
                        //else
                        //    return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            LoanAdvanceHead LoanAdvHead = db.LoanAdvanceHead.AsNoTracking().Include(e => e.ITLoan)
                                                                        .Include(e => e.ITSection).Include(e => e.LoanAdvancePolicy).Include(e => e.SalaryHead)
                                                                        .FirstOrDefault(e => e.Id == auth_id);


                            LoanAdvHead.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = LoanAdvHead.DBTrack.ModifiedBy != null ? LoanAdvHead.DBTrack.ModifiedBy : null,
                                CreatedBy = LoanAdvHead.DBTrack.CreatedBy != null ? LoanAdvHead.DBTrack.CreatedBy : null,
                                CreatedOn = LoanAdvHead.DBTrack.CreatedOn != null ? LoanAdvHead.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.LoanAdvanceHead.Attach(LoanAdvHead);
                            db.Entry(LoanAdvHead).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("PFTrust/PFTrust", null, db.ChangeTracker, LoanAdvHead.DBTrack);
                            DT_LoanAdvanceHead DT_LoanAdvHead = (DT_LoanAdvanceHead)rtn_Obj;
                            DT_LoanAdvHead.ITLoan_Id = LoanAdvHead.ITLoan == null ? 0 : LoanAdvHead.ITLoan.Id;
                            // DT_LoanAdvHead.LoanAdvancePolicy_Id = LoanAdvHead.LoanAdvancePolicy == null ? 0 : LoanAdvHead.LoanAdvancePolicy.Id;
                            DT_LoanAdvHead.SalaryHead_Id = LoanAdvHead.SalaryHead == null ? 0 : LoanAdvHead.SalaryHead.Id;
                            db.Create(DT_LoanAdvHead);
                            await db.SaveChangesAsync();
                            db.Entry(DT_LoanAdvHead).State = System.Data.Entity.EntityState.Detached;
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
                IEnumerable<LoanAdvanceHeadPFT> LoanAdvanceHead = null;
                if (gp.IsAutho == true)
                {
                    LoanAdvanceHead = db.LoanAdvanceHeadPFT.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    LoanAdvanceHead = db.LoanAdvanceHeadPFT.AsNoTracking().ToList();
                }

                IEnumerable<LoanAdvanceHeadPFT> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = LoanAdvanceHead;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where((e => (e.Id.ToString().Contains(gp.searchString))
                            || (e.Code.ToLower().Contains(gp.searchString.ToLower()))
                            || (e.Name.ToLower().Contains(gp.searchString.ToLower())))
                            ).Select(a => new { a.Id, a.Code, a.Name }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = LoanAdvanceHead;
                    Func<LoanAdvanceHeadPFT, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                         gp.sidx == "Name" ? c.Name :
                                         gp.sidx == "Name" ? c.Name : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                    }
                    totalRecords = LoanAdvanceHead.Count();
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
                    LoanAdvanceHeadPFT objjvparam = db.LoanAdvanceHeadPFT
                        .Include(e => e.LoanAdvancePolicyPFT)

                                              .Where(e => e.Id == data).SingleOrDefault();

                    var LoanAdvhead = objjvparam.LoanAdvancePolicyPFT;


                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //if (ITSection != null)
                        //{
                        //    var objITSection = new HashSet<int>(objjvparam.ITSection.Select(e => e.Id));
                        //    if (objITSection.Count > 0)
                        //    {
                        //        Msg.Add(" Child record exists.Cannot remove it..  ");
                        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //        // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                        //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                        //    }
                        //}
                        if (LoanAdvhead != null)
                        {
                            //var objLoanAdvhead = new HashSet<int>(objjvparam.LoanAdvancePolicyPFT.Select(e => e.Id));
                            //if (objLoanAdvhead.Count > 0)
                            //{
                            //    Msg.Add(" Child record exists.Cannot remove it..  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //    // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //}
                        }

                        try
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = objjvparam.DBTrack.CreatedBy != null ? objjvparam.DBTrack.CreatedBy : null,
                                CreatedOn = objjvparam.DBTrack.CreatedOn != null ? objjvparam.DBTrack.CreatedOn : null,
                                IsModified = objjvparam.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };
                            db.Entry(objjvparam).State = System.Data.Entity.EntityState.Deleted;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, objjvparam.DBTrack);
                            //DT_JVParameter DT_Corp = (DT_JVParameter)rtn_Obj;
                            //db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

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
            //[HttpPost]
            //public async Task<ActionResult> Delete(int data)
            //{

            //    LoanAdvanceHead LoanAdvanceHeads = db.LoanAdvanceHead.Include(e => e.ITLoan)
            //                                       .Include(e => e.ITSection).Include(e => e.LoanAdvancePolicy).Include(e => e.SalaryHead)
            //                                       .Where(e => e.Id == data).SingleOrDefault();



            //    if (LoanAdvanceHeads.DBTrack.IsModified == true)
            //    {
            //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //        {
            //            DBTrack dbT = new DBTrack
            //            {
            //                Action = "D",
            //                CreatedBy = LoanAdvanceHeads.DBTrack.CreatedBy != null ? LoanAdvanceHeads.DBTrack.CreatedBy : null,
            //                CreatedOn = LoanAdvanceHeads.DBTrack.CreatedOn != null ? LoanAdvanceHeads.DBTrack.CreatedOn : null,
            //                IsModified = LoanAdvanceHeads.DBTrack.IsModified == true ? true : false
            //            };
            //            LoanAdvanceHeads.DBTrack = dbT;
            //            db.Entry(LoanAdvanceHeads).State = System.Data.Entity.EntityState.Modified;
            //            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, LoanAdvanceHeads.DBTrack);
            //            DT_LoanAdvanceHead DT_LoanAdvHead = (DT_LoanAdvanceHead)rtn_Obj;
            //            //DT_LoanAdvHead.LoanAdvancePolicy_Id = LoanAdvanceHeads.LoanAdvancePolicy == null ? 0 : LoanAdvanceHeads.LoanAdvancePolicy.Id;
            //            DT_LoanAdvHead.SalaryHead_Id = LoanAdvanceHeads.SalaryHead == null ? 0 : LoanAdvanceHeads.SalaryHead.Id;
            //            DT_LoanAdvHead.ITLoan_Id = LoanAdvanceHeads.ITLoan == null ? 0 : LoanAdvanceHeads.ITLoan.Id;

            //            db.Create(DT_LoanAdvHead);
            //            await db.SaveChangesAsync();
            //            ts.Complete();
            //            return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
            //        }
            //    }
            //    else
            //    {

            //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //        {
            //            try
            //            {
            //                DBTrack dbT = new DBTrack
            //                {
            //                    Action = "D",
            //                    ModifiedBy = SessionManager.UserName,
            //                    ModifiedOn = DateTime.Now,
            //                    CreatedBy = LoanAdvanceHeads.DBTrack.CreatedBy != null ? LoanAdvanceHeads.DBTrack.CreatedBy : null,
            //                    CreatedOn = LoanAdvanceHeads.DBTrack.CreatedOn != null ? LoanAdvanceHeads.DBTrack.CreatedOn : null,
            //                    IsModified = LoanAdvanceHeads.DBTrack.IsModified == true ? false : false//,

            //                };

            //                db.Entry(LoanAdvanceHeads).State = System.Data.Entity.EntityState.Deleted;
            //                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
            //                DT_LoanAdvanceHead DT_LoanAdvHead = (DT_LoanAdvanceHead)rtn_Obj;


            //                db.Create(DT_LoanAdvHead);

            //                await db.SaveChangesAsync();
            //                ts.Complete();
            //                return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

            //            }
            //            catch (RetryLimitExceededException /* dex */)
            //            {
            //                return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
            //            }
            //        }
            //    }
            //}


            //    public int EditS(string interesttpye, string loansanctionwages, int data, LoanAdvanceHead c, DBTrack dbT)
            //    {
            //        if (interesttpye != null)
            //        {
            //            if (interesttpye != "")
            //            {
            //                var val = db.LookupValue.Find(int.Parse(interesttpye));
            //                c.InterestTpye = val;

            //                var type = db.LoanAdvanceHead.Include(e => e.InterestTpye).Where(e => e.Id == data).SingleOrDefault();
            //                IList<LoanAdvanceHead> typedetails = null;
            //                if (type.InterestTpye != null)
            //                {
            //                    typedetails = db.LoanAdvanceHead.Where(x => x.InterestTpye.Id == type.InterestTpye.Id && x.Id == data).ToList();
            //                }
            //                else
            //                {
            //                    typedetails = db.LoanAdvanceHead.Where(x => x.Id == data).ToList();
            //                }
            //                foreach (var s in typedetails)
            //                {
            //                    s.InterestTpye = c.InterestTpye;
            //                    db.LoanAdvanceHead.Attach(s);
            //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //                    db.SaveChanges();
            //                    TempData["RowVersion"] = s.RowVersion;
            //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //                }
            //            }
            //            else
            //            {
            //                var BusiTypeDetails = db.LoanAdvanceHead.Include(e => e.InterestTpye).Where(x => x.Id == data).ToList();
            //                foreach (var s in BusiTypeDetails)
            //                {
            //                    s.InterestTpye = null;
            //                    db.LoanAdvanceHead.Attach(s);
            //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //                    db.SaveChanges();
            //                    TempData["RowVersion"] = s.RowVersion;
            //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            var BusiTypeDetails = db.LoanAdvanceHead.Include(e => e.InterestTpye).Where(x => x.Id == data).ToList();
            //            foreach (var s in BusiTypeDetails)
            //            {
            //                s.InterestTpye = null;
            //                db.LoanAdvanceHead.Attach(s);
            //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //                db.SaveChanges();
            //                TempData["RowVersion"] = s.RowVersion;
            //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //            }
            //        }

            //        if (loansanctionwages != null)
            //        {
            //            if (loansanctionwages != "")
            //            {
            //                var val = db.Wages.Find(int.Parse(loansanctionwages));
            //                c.LoanSanctionWages = val;

            //                var add = db.LoanAdvanceHead.Include(e => e.LoanSanctionWages).Where(e => e.Id == data).SingleOrDefault();
            //                IList<LoanAdvanceHead> contactsdetails = null;
            //                if (add.LoanSanctionWages != null)
            //                {
            //                    contactsdetails = db.LoanAdvanceHead.Where(x => x.LoanSanctionWages.Id == add.LoanSanctionWages.Id && x.Id == data).ToList();
            //                }
            //                else
            //                {
            //                    contactsdetails = db.LoanAdvanceHead.Where(x => x.Id == data).ToList();
            //                }
            //                foreach (var s in contactsdetails)
            //                {
            //                    s.LoanSanctionWages = c.LoanSanctionWages;
            //                    db.LoanAdvanceHead.Attach(s);
            //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //                    db.SaveChanges();
            //                    TempData["RowVersion"] = s.RowVersion;
            //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            var contactsdetails = db.LoanAdvanceHead.Include(e => e.LoanSanctionWages).Where(x => x.Id == data).ToList();
            //            foreach (var s in contactsdetails)
            //            {
            //                s.LoanSanctionWages = null;
            //                db.LoanAdvanceHead.Attach(s);
            //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //                db.SaveChanges();
            //                TempData["RowVersion"] = s.RowVersion;
            //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //            }
            //        }

            //        var CurCorp = db.LoanAdvanceHead.Find(data);
            //        TempData["CurrRowVersion"] = CurCorp.RowVersion;
            //        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
            //        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
            //        {
            //            c.DBTrack = dbT;
            //            LoanAdvanceHead corp = new LoanAdvanceHead()
            //            {
            //                Code = c.Code,
            //                Name = c.Name,
            //                Id = data,
            //                DBTrack = c.DBTrack
            //            };

            //            db.LoanAdvanceHead.Attach(corp);
            //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
            //            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
            //            return 1;
            //        }
            //        return 0;
            //    }



            //    public ActionResult GetLookupWageslist(string data)
            //    {
            //        using (DataBaseContext db = new DataBaseContext())
            //        {
            //            var fall = db.Wages.ToList();
            //            IEnumerable<Wages> all;
            //            if (!string.IsNullOrEmpty(data))
            //            {
            //                all = db.Wages.ToList().Where(d => d.FullDetails.Contains(data));

            //            }
            //            else
            //            {
            //                var list1 = db.LoanAdvanceHead.ToList().Select(e => e.LoanSanctionWages);
            //                var list2 = fall.Except(list1);

            //                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
            //                return Json(r, JsonRequestBehavior.AllowGet);
            //            }
            //            var result = (from c in all
            //                          select new { c.Id, c.FullDetails }).Distinct();
            //            return Json(result, JsonRequestBehavior.AllowGet);
            //        }
            //    }

        }
    }
}

