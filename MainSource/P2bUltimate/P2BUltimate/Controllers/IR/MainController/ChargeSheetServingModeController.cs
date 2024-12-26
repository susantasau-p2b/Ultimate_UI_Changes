using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IR;
using P2b.Global;
using System.Data.Entity.Infrastructure;
using System.Text;
using System.Transactions;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using P2BUltimate.App_Start;
using System.Threading.Tasks;
using P2BUltimate.Security;
using Payroll;

namespace P2BUltimate.Controllers.IR.MainController
{
    public class ChargeSheetServingModeController : Controller
    {   
    [HttpPost]
        public ActionResult Create(  ChargeSheetServingMode c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string ChargeSheetServingModeName = form["ChargeSheetServingModelist"] == "0" ? "" : form["ChargeSheetServingModelist"];
                    if (ChargeSheetServingModeName != null && ChargeSheetServingModeName != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(ChargeSheetServingModeName));
                        c.ChargeSheetServingModeName = val;
                    }

                    string ServingSeqNo = form["ServingSeq"] == "0" ? "" : form["ServingSeq"];
                    if (ServingSeqNo != null && ServingSeqNo != "")
                    {
                        int ServingSeqNum = Convert.ToInt32(ServingSeqNo);
                        c.ServingSeq = ServingSeqNum;
                        
                    }



                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            ChargeSheetServingMode ChargeSheetServingMode = new ChargeSheetServingMode()
                            {
                                ChargeSheetServingModeName = c.ChargeSheetServingModeName,
                                ServingSeq = c.ServingSeq,
                                DBTrack = c.DBTrack
                            };

                            db.ChargeSheetServingMode.Add(ChargeSheetServingMode);
                           
                            try
                            {
                               
                                db.SaveChanges();

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = ChargeSheetServingMode.Id, Val = ChargeSheetServingMode.ChargeSheetServingModeName.LookupVal.ToString().ToUpper()+ ", "+ChargeSheetServingMode.ServingSeq, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                               
                            }
                            
                            catch (DataException)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
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
                        return Json(new Utility.JsonReturnClass {success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                       
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