using Attendance;
using P2b.Global;
using P2B.PFTRUST;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers.PFTrust
{
    public class LoanAdvancePolicyPFTController : Controller
    {
        // GET: LoanAdvancePolicyPFT
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create(LoanAdvancePolicyPFT c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string LoanSanctionWages = form["WagesList"] == "0" ? "" : form["WagesList"];
                    string InterestType = form["InterestTpyelist"] == "0" ? "" : form["InterestTpyelist"];
                    string EffectiveDate = form["EffectiveDate"] == "0" ? "" : form["EffectiveDate"];
                    string YrsOfServ = form["YrsOfServ"] == "0" ? "" : form["YrsOfServ"];
                    string IsLoanLimit = form["IsLoanLimit"] == "0" ? "" : form["IsLoanLimit"];
                    string PFLoanType = form["PFLoanType"] == "0" ? "" : form["PFLoanType"];
                    string IsFixAmount = form["IsFixAmount"] == "0" ? "" : form["IsFixAmount"];
                    string MaxLoanAmount = form["MaxLoanAmount"] == "0" ? "" : form["MaxLoanAmount"];
                    string IsOnWages = form["IsOnWages"] == "0" ? "" : form["IsOnWages"];
                    string IntAppl = form["IntAppl"] == "0" ? "" : form["IntAppl"];
                    string IntRate = form["IntRate"] == "0" ? "" : form["IntRate"];
                    string IsPerkOnInt = form["IsPerkOnInt"] == "0" ? "" : form["IsPerkOnInt"];
                    string GovtIntRate = form["GovtIntRate"] == "0" ? "" : form["GovtIntRate"];
                    string IsPFTContibution = form["IsPFTContibution"] == "0" ? "" : form["IsPFTContibution"];
                    string IsOwnContibution = form["IsOwnContibution"] == "0" ? "" : form["IsOwnContibution"];
                    string IsOwnerContibution = form["IsOwnerContibution"] == "0" ? "" : form["IsOwnerContibution"];
                    string IsOwnerIntContibution = form["IsOwnerIntContibution"] == "0" ? "" : form["IsOwnerIntContibution"];
                    string IsVPFIntContibution = form["IsVPFIntContibution"] == "0" ? "" : form["IsVPFIntContibution"];
                    string IsPFIntContibution = form["IsPFIntContibution"] == "0" ? "" : form["IsPFIntContibution"];

                    if (LoanSanctionWages != null && LoanSanctionWages != "")
                    {
                        var val = db.Wages.Find(int.Parse(LoanSanctionWages));
                        c.LoanSanctionWages_Id = val.Id;
                    }

                    if (InterestType != null && InterestType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(InterestType));
                        c.InterestType_Id = val.Id;
                    }
                    if (PFLoanType != null && PFLoanType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(PFLoanType));
                        c.PFLoanType_Id = val.Id;
                    }
                    using (TransactionScope ts = new TransactionScope())
                    {

                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        LoanAdvancePolicyPFT ObjLoanAdvPolicyPFT = new LoanAdvancePolicyPFT()
                        {   Name=c.Name,
                            EffectiveDate = c.EffectiveDate,
                            EndDate = c.EndDate,
                            FineAmt = c.FineAmt,
                            GovtIntRate = c.GovtIntRate,
                            IntAppl = c.IntAppl,
                            InterestType_Id = c.InterestType_Id,
                            IntRate = c.IntRate,
                            IsFine = c.IsFine,
                            IsFixAmount = c.IsFixAmount,
                            IsLoanLimit = c.IsLoanLimit,
                            IsOnWages = c.IsOnWages,
                            IsPerkOnInt = c.IsPerkOnInt,
                            LoanSanctionWages_Id = c.LoanSanctionWages_Id,
                            MaxLoanAmount = c.MaxLoanAmount,
                            NoOfTimes = c.NoOfTimes,
                            YrsOfServ = c.YrsOfServ,
                            IsPFTContibution = c.IsPFTContibution,
                            IsOwnContibution = c.IsOwnContibution,
                            IsOwnerContibution = c.IsOwnerContibution,
                            PFLoanType_Id = c.PFLoanType_Id,
                            IsOwnerIntContibution = c.IsOwnerIntContibution,
                            IsOwnIntContibution = c.IsOwnIntContibution,
                            IsVPFIntContibution = c.IsVPFIntContibution,
                            IsPFIntContibution = c.IsPFIntContibution,
                            NoOfTimesPFTContribution = c.NoOfTimesPFTContribution,
                            PFTContibutionPercent=c.PFTContibutionPercent,
                            OwnContibutionPercent=c.OwnContibutionPercent,
                            OwnerContibutionPercent=c.OwnerContibutionPercent,
                            OwnIntContibutionPercent=c.OwnIntContibutionPercent,
                            OwnerIntContibutionPercent=c.OwnerIntContibutionPercent,
                            PFIntContibutionPercent=c.PFIntContibutionPercent,
                            VPFIntContibutionPercent = c.VPFIntContibutionPercent,
                            DBTrack = c.DBTrack
                        };
                        try
                        {
                            db.LoanAdvancePolicyPFT.Add(ObjLoanAdvPolicyPFT);
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = ObjLoanAdvPolicyPFT.Id, Val = ObjLoanAdvPolicyPFT.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                           
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                       
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






