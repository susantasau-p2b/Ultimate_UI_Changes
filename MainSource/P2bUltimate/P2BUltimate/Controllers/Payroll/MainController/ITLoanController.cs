using Payroll;
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
using P2b.Global;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class ITLoanController : Controller
    {
      //  private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(ITLoan IT, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string IntPrincAppl = form["IntPrincAppl"] == "0" ? "" : form["IntPrincAppl"];
                    string IntAppl = form["IntAppl"] == "0" ? "" : form["IntAppl"];
                    string PrincAppl = form["PrincAppl"] == "0" ? "" : form["PrincAppl"];

                    IT.IntPrincAppl = Convert.ToBoolean(IntPrincAppl);
                    IT.IntAppl = Convert.ToBoolean(IntAppl);
                    IT.PrincAppl = Convert.ToBoolean(PrincAppl);

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            IT.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            ITLoan itloan = new ITLoan()
                            {
                                IntAppl = IT.IntAppl,
                                IntPrincAppl = IT.IntPrincAppl,
                                PrincAppl = IT.PrincAppl,
                                DBTrack = IT.DBTrack
                            };
                            try
                            {
                                db.ITLoan.Add(itloan);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);
                                //DT_LoanAdvancePolicy DT_LoanAdvPolicy = (DT_LoanAdvancePolicy)rtn_Obj;
                                //DT_LoanAdvPolicy.InterestType_Id = c.InterestType == null ? 0 : c.InterestType.Id;
                                //DT_LoanAdvPolicy.LoanSanctionWages_Id = c.LoanSanctionWages == null ? 0 : c.LoanSanctionWages.Id;
                                //db.Create(DT_LoanAdvPolicy);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = itloan.Id, Val = itloan.FullDetails , success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = IT.Id });
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
	}
}