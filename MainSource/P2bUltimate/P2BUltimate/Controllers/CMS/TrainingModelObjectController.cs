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
    public class TrainingModelObjectController : Controller
    {
        // GET: TrainingModelObject
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Training(string valueId)
        {
            TempData["TrainingPId"] = valueId;
            return View();
        }
        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var TrainingModelLookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "725").SingleOrDefault();
                var TrainingModelLookupvalues = TrainingModelLookup.LookupValues.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(TrainingModelLookupvalues, "Id", "LookupVal", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
            //using (DataBaseContext db = new DataBaseContext())
            //{
            //    var fall = db.ProgramList.ToList();
            //    var selected = (Object)null;
            //    if (data2 != "" && data != "0" && data2 != "0")
            //    {
            //        selected = Convert.ToInt32(data2);
            //    }

            //    SelectList s = new SelectList(fall, "Id", "Subject", selected);
            //    return Json(s, JsonRequestBehavior.AllowGet);
            //}
        }

        [HttpPost]
        public ActionResult Create(TrainingModelObject c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var TPDetails = "";
                    var Creteriases = "";
                    var CriteriaTypeses = "";
                    var DataStepses = "";
                    string TrainingModellist = form["TrainingModellist"] == "" ? null : form["TrainingModellist"];
                    string CompetencyEvaluationModel = form["CompetencyEvaluationModelList"] == "" ? null : form["CompetencyEvaluationModelList"];
                    int TPFullDetailsId = Convert.ToInt32(TrainingModellist);
                    var TPTrainingModel = db.TrainingModelObject.Include(t => t.TrainingModel).ToList().Select(r => r.TrainingModel).Where(e => e.Id == TPFullDetailsId).FirstOrDefault();
                    if (TPTrainingModel != null)
                    {
                        Msg.Add("Already Training Model Object Exits");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (TrainingModellist != null && TrainingModellist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(TrainingModellist));
                        c.TrainingModel = val;
                    }
                    
                    if (CompetencyEvaluationModel != null && CompetencyEvaluationModel != "")
                    {
                        int CompetencyEvaluationModelId = int.Parse(CompetencyEvaluationModel);
                        var val = db.CompetencyEvaluationModel.Include(e => e.DataSteps).
                            Include(e => e.Criteria).Include(e => e.CriteriaType)
                           .Where(e => e.Id == CompetencyEvaluationModelId).SingleOrDefault();
                           c.CompetencyEvaluationModel = val;
                           Creteriases = c.CompetencyEvaluationModel != null ? c.CompetencyEvaluationModel.Criteria.LookupVal : " ";
                           CriteriaTypeses = c.CompetencyEvaluationModel != null ? c.CompetencyEvaluationModel.CriteriaType.LookupVal : " ";
                           DataStepses = c.CompetencyEvaluationModel != null && c.CompetencyEvaluationModel.DataSteps != null ? c.CompetencyEvaluationModel.DataSteps.LookupVal : " ";
                    }
                    else
                    {

                        Msg.Add("please select evaluation model");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                   // TPDetails = db.ProgramList.Where(e => e.Id == TPFullDetailsId).SingleOrDefault().FullDetails;
                    using (TransactionScope ts = new TransactionScope())
                    {
                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        TrainingModelObject TrainingModelObject = new TrainingModelObject()
                        {
                            TrainingModel = c.TrainingModel,
                            CompetencyEvaluationModel = c.CompetencyEvaluationModel,
                            DBTrack = c.DBTrack
                        };
                        db.TrainingModelObject.Add(TrainingModelObject);
                        try
                        {
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = TrainingModelObject.Id, Val = TPDetails + " " + Creteriases + " " + CriteriaTypeses + " " + DataStepses, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        public class returnEditClass
        {
            public int Id { get; set; }
            public int CompetencyEvaluationModelId { get; set; }
            public int TPModelId { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {               
                var dbdata = db.TrainingModelObject
                                  .Include(e => e.TrainingModel)
                                  .Include(e => e.CompetencyEvaluationModel)
                                  .Include(e => e.CompetencyEvaluationModel.Criteria)
                                  .Include(e => e.CompetencyEvaluationModel.CriteriaType)
                                  .Include(e => e.CompetencyEvaluationModel.DataSteps)
                                  .Where(e => e.Id == data).ToList();

                if (dbdata != null)
                {
                    List<returnEditClass> dbdatases = new List<returnEditClass>();
                    foreach (var item in dbdata)
                    {
                        dbdatases.Add(new returnEditClass
                        {
                            Id = item.Id,
                            TPModelId = item.TrainingModel != null ? item.TrainingModel.Id : 0,
                            CompetencyEvaluationModelId = item.CompetencyEvaluationModel != null ? item.CompetencyEvaluationModel.Id : 0,
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                        });
                    }
                    var returndata = dbdatases.Select(e => new
                    {

                        TrainingModelObject_Id = e.TPModelId,
                        CompetencyEvaluationModel_Id = e.CompetencyEvaluationModelId,
                        CompetencyEvaluationModelDetails = e.Creteriases + " " + e.CreteriaTypeses + " " + e.DataStepses

                    }).ToList();

                    return Json(new Object[] { returndata, JsonRequestBehavior.AllowGet });
                }
                else
                {
                    return null;
                }
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(TrainingModelObject c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var TPLookupvaluesId = TempData["TrainingPId"];
                    int TPIds = Convert.ToInt32(TPLookupvaluesId);
                    var TPDetails = "";
                    var Creteriases = "";
                    var CriteriaTypeses = "";
                    var DataStepses = "";
                    string TrainingModellist = form["TrainingModellist"] == "" ? null : form["TrainingModellist"];
                    string CompetencyEvaluationModel = form["CompetencyEvaluationModelList"] == "" ? null : form["CompetencyEvaluationModelList"];
                    int TPDetailsId = Convert.ToInt32(TrainingModellist);
                    if (TPIds != 0)
                    {
                        var TPModel = db.TrainingModelObject.Include(t => t.TrainingModel).ToList().Select(r => r.TrainingModel).Where(e => e.Id == TPIds).FirstOrDefault();
                        if (TPModel != null)
                        {
                            Msg.Add("Already Training Model Object Exits");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    TPDetails = db.ProgramList.Where(e => e.Id == TPDetailsId).SingleOrDefault().FullDetails;
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (TrainingModellist != null)
                        {
                            if (TrainingModellist != "")
                            {
                                var val = db.LookupValue.Find(int.Parse(TrainingModellist));
                                c.TrainingModel = val;

                                var type = db.TrainingModelObject.Include(e => e.TrainingModel).Where(e => e.Id == data).FirstOrDefault();
                                IList<TrainingModelObject> typedetails = null;
                                if (type.TrainingModel != null)
                                {
                                    typedetails = db.TrainingModelObject.Where(x => x.TrainingModel.Id == type.TrainingModel.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    typedetails = db.TrainingModelObject.Where(x => x.Id == data).ToList();
                                }

                                foreach (var s in typedetails)
                                {
                                    s.TrainingModel = c.TrainingModel;
                                    db.TrainingModelObject.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            else
                            {
                                var TPModelDetails = db.TrainingModelObject.Include(e => e.TrainingModel).Where(x => x.Id == data).ToList();
                                foreach (var s in TPModelDetails)
                                {
                                    s.TrainingModel = null;
                                    db.TrainingModelObject.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }


                        if (CompetencyEvaluationModel != null)
                        {
                            int CompetencyEvaluationModelId = Convert.ToInt32(CompetencyEvaluationModel);
                            var Creterias = db.CompetencyEvaluationModel.Include(e => e.Criteria).Where(e => e.Id == CompetencyEvaluationModelId).FirstOrDefault().Criteria;
                            var CreteriaTypes = db.CompetencyEvaluationModel.Include(e => e.CriteriaType).Where(e => e.Id == CompetencyEvaluationModelId).FirstOrDefault().CriteriaType;
                            var DataStepss = db.CompetencyEvaluationModel.Include(e => e.DataSteps).Where(e => e.Id == CompetencyEvaluationModelId).FirstOrDefault().CriteriaType;
                            Creteriases = Creterias != null ? Creterias.LookupVal : "";
                            CriteriaTypeses = CreteriaTypes != null ? CreteriaTypes.LookupVal : "";
                            DataStepses = DataStepss != null ? DataStepss.LookupVal : "";

                            if (CompetencyEvaluationModel != "")
                            {
                                var val = db.CompetencyEvaluationModel.Find(int.Parse(CompetencyEvaluationModel));
                                c.CompetencyEvaluationModel = val;

                                var add = db.TrainingModelObject.Include(e => e.CompetencyEvaluationModel).Where(e => e.Id == data).FirstOrDefault();
                                IList<TrainingModelObject> competencyevaluationmodeldetails = null;
                                if (add.CompetencyEvaluationModel != null)
                                {
                                    competencyevaluationmodeldetails = db.TrainingModelObject.Where(x => x.CompetencyEvaluationModel.Id == add.CompetencyEvaluationModel.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    competencyevaluationmodeldetails = db.TrainingModelObject.Where(x => x.Id == data).ToList();
                                }
                                if (competencyevaluationmodeldetails != null)
                                {
                                    foreach (var s in competencyevaluationmodeldetails)
                                    {
                                        s.CompetencyEvaluationModel = c.CompetencyEvaluationModel;
                                        db.TrainingModelObject.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        // await db.SaveChangesAsync(false);
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                            }

                            else
                            {
                                var competencyevaluationmodeldetails = db.TrainingModelObject.Include(e => e.CompetencyEvaluationModel).Where(x => x.Id == data).ToList();
                                foreach (var s in competencyevaluationmodeldetails)
                                {
                                    s.CompetencyEvaluationModel = null;
                                    db.TrainingModelObject.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }

                        else
                        {
                            Msg.Add("please select evaluation model");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        TrainingModelObject CMS_TrainingModelObject = db.TrainingModelObject.Find(data);
                        TempData["CurrRowVersion"] = CMS_TrainingModelObject.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = CMS_TrainingModelObject.DBTrack.CreatedBy == null ? null : CMS_TrainingModelObject.DBTrack.CreatedBy,
                                CreatedOn = CMS_TrainingModelObject.DBTrack.CreatedOn == null ? null : CMS_TrainingModelObject.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            CMS_TrainingModelObject.Id = data;
                            CMS_TrainingModelObject.DBTrack = c.DBTrack;
                            db.TrainingModelObject.Attach(CMS_TrainingModelObject);
                            db.Entry(CMS_TrainingModelObject).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        ts.Complete();

                        Msg.Add("  Record Updated");
                        return Json(new Utility.JsonReturnClass { Id = data, Val = TPDetails + "" + Creteriases + " " + CriteriaTypeses + " " + DataStepses, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

            return null;
        }

    }
}