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
    public class CompetencyEvaluationModelController : Controller
    {
        //
        // GET: /CompetencyEvaluationModel/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CompetencyEvaluationModelPartial()
        {
            return View("~/Views/Shared/CMS/CompetencyEvaluationModel.cshtml");
        }
        public class returnDataClass
        {
            public int Id { get; set; }
            public string DataStepses { get; set; }
            public string EvaluationModel { get; set; }

        }
        public ActionResult GetLookupCompetencyEvaluationModel()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var db_data = db.CompetencyEvaluationModel.Include(e => e.Criteria)
                .Include(e => e.CriteriaType).Include(e => e.DataSteps).ToList();
                if (db_data != null)
                {
                    List<returnDataClass> returndata = new List<returnDataClass>();

                    foreach (var item in db_data)
                    {
                        returndata.Add(new returnDataClass
                        {
                            Id = item.Id,
                            DataStepses = item.DataSteps != null ? item.DataSteps.LookupVal : "",
                            EvaluationModel = item.Criteria.LookupVal + " " + item.CriteriaType.LookupVal

                        });
                    }

                    var res = (from ca in returndata
                               select new
                               {
                                   srno = ca.Id,
                                   lookupvalue = ca.DataStepses + " " + ca.EvaluationModel
                               }).ToList();

                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

                    return Json(result, JsonRequestBehavior.AllowGet);
                }


                return null;

            }
        }
        [HttpPost]
        public ActionResult Create(CompetencyEvaluationModel c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var DataStepses = "";
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
                    if (c.DataStepsAppl == true)
                    {
                        if (DataStepslist != null && DataStepslist != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(DataStepslist));
                            c.DataSteps = val;

                        }
                    }
                    else
                    {
                        c.DataSteps = null;
                    }
                    DataStepses = c.DataSteps != null ? c.DataSteps.LookupVal : "";

                    using (TransactionScope ts = new TransactionScope())
                    {
                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        CompetencyEvaluationModel CompetencyEvaluationModel = new CompetencyEvaluationModel()
                        {

                            Criteria = c.Criteria,
                            CriteriaType = c.CriteriaType,
                            DataSteps = c.DataSteps,
                            DataStepsAppl = c.DataStepsAppl,
                            InitialValue=c.InitialValue,
                            DBTrack = c.DBTrack
                        };

                        db.CompetencyEvaluationModel.Add(CompetencyEvaluationModel);

                        try
                        {

                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = CompetencyEvaluationModel.Id, Val = CompetencyEvaluationModel.Criteria.LookupVal + "," + CompetencyEvaluationModel.CriteriaType.LookupVal + " " + DataStepses, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

                var returndata = db.CompetencyEvaluationModel.Include(e => e.Criteria).Include(e => e.CriteriaType)
                    .Include(e => e.DataSteps)
                                  .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        InitialValue = e.InitialValue,
                        DataStepsAppl = e.DataStepsAppl,
                        Criteria_Id = e.Criteria != null ? e.Criteria.Id : 0,
                        CriteriaType_Id = e.CriteriaType != null ? e.CriteriaType.Id : 0,
                        DataSteps_Id = e.DataSteps != null ? e.DataSteps.Id : 0,

                    }).ToList();

                return Json(new Object[] { returndata, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(CompetencyEvaluationModel c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();
            string Criterialist = form["Criterialist"] == "0" ? null : form["Criterialist"];
            string CriteriaTypelist = form["CriteriaTypelist"] == "0" ? null : form["CriteriaTypelist"];
            string DataStepslist = c.DataStepsAppl == true ? form["DataStepslist"] : "";
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var DataStepses = "";

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (Criterialist != null)
                        {
                            if (Criterialist != "")
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "501").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Criterialist)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(BusinessCategory));
                                c.Criteria = val;

                                var type = db.CompetencyEvaluationModel.Include(e => e.Criteria).Where(e => e.Id == data).SingleOrDefault();
                                IList<CompetencyEvaluationModel> typedetails = null;
                                if (type.Criteria != null)
                                {
                                    typedetails = db.CompetencyEvaluationModel.Where(x => x.Criteria.Id == type.Criteria.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    typedetails = db.CompetencyEvaluationModel.Where(x => x.Id == data).ToList();
                                }

                                foreach (var s in typedetails)
                                {
                                    s.Criteria = c.Criteria;
                                    db.CompetencyEvaluationModel.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            else
                            {
                                var CriteriaDetails = db.CompetencyEvaluationModel.Include(e => e.Criteria).Where(x => x.Id == data).ToList();
                                foreach (var s in CriteriaDetails)
                                {
                                    s.Criteria = null;
                                    db.CompetencyEvaluationModel.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }
                        if (CriteriaTypelist != null)
                        {
                            if (CriteriaTypelist != "")
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "503").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(CriteriaTypelist)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(BusinessCategory));
                                c.CriteriaType = val;

                                var type = db.CompetencyEvaluationModel.Include(e => e.CriteriaType).Where(e => e.Id == data).SingleOrDefault();
                                IList<CompetencyEvaluationModel> typedetails = null;
                                if (type.CriteriaType != null)
                                {
                                    typedetails = db.CompetencyEvaluationModel.Where(x => x.CriteriaType.Id == type.CriteriaType.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    typedetails = db.CompetencyEvaluationModel.Where(x => x.Id == data).ToList();
                                }

                                foreach (var s in typedetails)
                                {
                                    s.CriteriaType = c.CriteriaType;
                                    db.CompetencyEvaluationModel.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            else
                            {
                                var CriteriaTypeDetails = db.CompetencyEvaluationModel.Include(e => e.CriteriaType).Where(x => x.Id == data).ToList();
                                foreach (var s in CriteriaTypeDetails)
                                {
                                    s.CriteriaType = null;
                                    db.CompetencyEvaluationModel.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }
                        if (DataStepslist != null)
                        {
                            if (DataStepslist != "")
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "504").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(DataStepslist)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(BusinessCategory));
                                c.DataSteps = val;

                                var type = db.CompetencyEvaluationModel.Include(e => e.DataSteps).Where(e => e.Id == data).SingleOrDefault();
                                IList<CompetencyEvaluationModel> typedetails = null;
                                if (type.DataSteps != null)
                                {
                                    typedetails = db.CompetencyEvaluationModel.Where(x => x.DataSteps.Id == type.DataSteps.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    typedetails = db.CompetencyEvaluationModel.Where(x => x.Id == data).ToList();
                                }

                                foreach (var s in typedetails)
                                {
                                    s.DataSteps = c.DataSteps;
                                    db.CompetencyEvaluationModel.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            else
                            {
                                var DataStepslDetails = db.CompetencyEvaluationModel.Include(e => e.DataSteps).Where(x => x.Id == data).ToList();
                                foreach (var s in DataStepslDetails)
                                {
                                    s.DataSteps = null;
                                    db.CompetencyEvaluationModel.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }

                        DataStepses = c.DataSteps != null ? c.DataSteps.LookupVal : "";
                        CompetencyEvaluationModel CMS_CompetencyEvaluationModel = db.CompetencyEvaluationModel.Find(data);
                        TempData["CurrRowVersion"] = CMS_CompetencyEvaluationModel.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = CMS_CompetencyEvaluationModel.DBTrack.CreatedBy == null ? null : CMS_CompetencyEvaluationModel.DBTrack.CreatedBy,
                                CreatedOn = CMS_CompetencyEvaluationModel.DBTrack.CreatedOn == null ? null : CMS_CompetencyEvaluationModel.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            CMS_CompetencyEvaluationModel.Id = data;
                            CMS_CompetencyEvaluationModel.DataStepsAppl = c.DataStepsAppl;
                            CMS_CompetencyEvaluationModel.InitialValue= c.InitialValue;
                            CMS_CompetencyEvaluationModel.DBTrack = c.DBTrack;
                            db.CompetencyEvaluationModel.Attach(CMS_CompetencyEvaluationModel);
                            db.Entry(CMS_CompetencyEvaluationModel).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = data, Val = c.Criteria.LookupVal.ToString() + " " + c.CriteriaType.LookupVal.ToString() + " " + DataStepses, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }



    }
}