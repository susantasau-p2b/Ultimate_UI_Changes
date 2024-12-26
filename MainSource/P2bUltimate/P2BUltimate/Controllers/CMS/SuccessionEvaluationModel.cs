using CMS_SPS;
using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;

namespace P2BUltimate.Controllers.CMS

{
    public class SuccessionEvaluationModelController : Controller
    {
        //
        // GET: /SuccessionEvaluationModel/
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(SuccessionEvaluationModel c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Criterialist = form["Criterialist"] == "" ? null : form["Criterialist"];
                    string CriteriaTypelist = form["CriteriaTypelist"] == "" ? null : form["CriteriaTypelist"];
                    string DataStepslist = form["DataStepslist"] == "" ? null : form["DataStepslist"];

                    if (Criterialist != null && Criterialist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Criterialist));
                        c.Criteria = val;

                    }


                    if (CriteriaTypelist != null && CriteriaTypelist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(CriteriaTypelist));
                        c.CriteriaType = val;

                    }

                    if (DataStepslist != null && DataStepslist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(DataStepslist));
                        c.DataSteps = val;

                    }

                    using (TransactionScope ts = new TransactionScope())
                    {
                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        SuccessionEvaluationModel SuccessionEvaluationModel = new SuccessionEvaluationModel()
                        {

                            Criteria = c.Criteria,
                            CriteriaType = c.CriteriaType,
                            DataSteps = c.DataSteps,
                            DataStepsAppl = c.DataStepsAppl,
                            DBTrack = c.DBTrack
                        };



                        db.SuccessionEvaluationModel.Add(SuccessionEvaluationModel);

                        try
                        {

                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = SuccessionEvaluationModel.Id, Val = SuccessionEvaluationModel.Criteria.LookupVal.ToString() + "," + SuccessionEvaluationModel.CriteriaType.LookupVal.ToString() + "," + SuccessionEvaluationModel.DataSteps.LookupVal.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var returndata = db.SuccessionEvaluationModel.Include(e => e.Criteria).Include(e => e.CriteriaType)
                    .Include(e => e.DataSteps)
                                  .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        DataStepsAppl = e.DataStepsAppl,
                        Criteria_Id = e.Criteria != null ? e.Criteria.Id : 0,
                        CriteriaType_Id = e.CriteriaType != null ? e.CriteriaType.Id : 0,
                        DataSteps_Id = e.DataSteps != null ? e.DataSteps.Id : 0,

                    }).ToList();

                return Json(new Object[] { returndata, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(SuccessionEvaluationModel c, int data, FormCollection form)
        {

            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        string Criterialist = form["Criterialist"] == "" ? null : form["Criterialist"];
                        string CriteriaTypelist = form["CriteriaTypelist"] == "" ? null : form["CriteriaTypelist"];
                        string DataStepslist = form["DataStepslist"] == "" ? null : form["DataStepslist"];

                        if (Criterialist != null && Criterialist != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(Criterialist));
                            c.Criteria = val;

                        }
                        if (CriteriaTypelist != null && CriteriaTypelist != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(CriteriaTypelist));
                            c.CriteriaType = val;

                        }
                        if (DataStepslist != null && DataStepslist != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(DataStepslist));
                            c.DataSteps = val;

                        }

                        var db_data = db.SuccessionEvaluationModel.Where(e => e.Id == data).SingleOrDefault();
                        TempData["RowVersion"] = db_data.RowVersion;
                        SuccessionEvaluationModel SuccessionEvaluationModel = db.SuccessionEvaluationModel.Find(data);
                        TempData["CurrRowVersion"] = SuccessionEvaluationModel.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = SuccessionEvaluationModel.DBTrack.CreatedBy == null ? null : SuccessionEvaluationModel.DBTrack.CreatedBy,
                                CreatedOn = SuccessionEvaluationModel.DBTrack.CreatedOn == null ? null : SuccessionEvaluationModel.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            SuccessionEvaluationModel.Id = data;
                            SuccessionEvaluationModel.Criteria = c.Criteria;
                            SuccessionEvaluationModel.CriteriaType = c.CriteriaType;
                            SuccessionEvaluationModel.DataSteps = c.DataSteps;
                            SuccessionEvaluationModel.DataStepsAppl = c.DataStepsAppl;
                            SuccessionEvaluationModel.DBTrack = c.DBTrack;

                            db.Entry(SuccessionEvaluationModel).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = data, Val = c.Criteria.LookupVal.ToString() + "," + c.CriteriaType.LookupVal.ToString() + "," + c.DataSteps.LookupVal.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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