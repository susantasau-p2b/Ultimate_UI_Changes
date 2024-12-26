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
    public class QualificationModelObjectController : Controller
    {
        // GET: QualificationModelObject
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Qualification(string valueId)
        {
            TempData["QualificationId"] = valueId;
            return View();
        }
        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var QualificationLookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "314").SingleOrDefault();
                var QualificationLookupvalues = QualificationLookup.LookupValues.ToList();

                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(QualificationLookupvalues, "Id", "LookupVal", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult Create(QualificationModelObject c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var LookupvalDetails = "";
                    var Creteriases = "";
                    var CriteriaTypeses = "";
                    var DataStepses = "";
                    string QualificationModellist = form["QualificationModelList"] == "" ? null : form["QualificationModelList"];
                    string CompetencyEvaluationModel = form["CompetencyEvaluationModelList"] == "" ? null : form["CompetencyEvaluationModelList"];
                    int QualificationLookupValuesId = Convert.ToInt32(QualificationModellist);
                    var AppraisalQualificationModel = db.QualificationModelObject.Include(t => t.QualificationModel).ToList().Select(r => r.QualificationModel).Where(e => e.Id == QualificationLookupValuesId).FirstOrDefault();
                    if (AppraisalQualificationModel != null)
                    {
                        Msg.Add("Already Qualification Model Object Exits");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (QualificationModellist != null && QualificationModellist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(QualificationModellist));
                        c.QualificationModel = val;
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
                    var QualificationLookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "314").SingleOrDefault();
                    LookupvalDetails = QualificationLookup.LookupValues.ToList().Where(e => e.Id == QualificationLookupValuesId).Select(r => r.LookupVal).FirstOrDefault();
                  
                    using (TransactionScope ts = new TransactionScope())
                    {
                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        QualificationModelObject QualificationModelObject = new QualificationModelObject()
                        {
                            QualificationModel = c.QualificationModel,
                            CompetencyEvaluationModel = c.CompetencyEvaluationModel,
                            DBTrack = c.DBTrack
                        };
                        db.QualificationModelObject.Add(QualificationModelObject);
                        try
                        {
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = QualificationModelObject.Id, Val = LookupvalDetails + " " + Creteriases + " " + CriteriaTypeses + " " + DataStepses, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
            public int QualificationModelId { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<returnEditClass> dbdatases = new List<returnEditClass>();

                var dbdata = db.QualificationModelObject
                                  .Include(e => e.QualificationModel)
                                  .Include(e => e.CompetencyEvaluationModel)
                                  .Include(e => e.CompetencyEvaluationModel.Criteria)
                                  .Include(e => e.CompetencyEvaluationModel.CriteriaType)
                                  .Include(e => e.CompetencyEvaluationModel.DataSteps)
                                  .Where(e => e.Id == data).ToList();

                if (dbdata != null)
                {
                    foreach (var item in dbdata)
                    {
                        dbdatases.Add(new returnEditClass
                        {
                            Id = item.Id,
                            QualificationModelId = item.QualificationModel != null ? item.QualificationModel.Id : 0,
                            CompetencyEvaluationModelId = item.CompetencyEvaluationModel != null ? item.CompetencyEvaluationModel.Id : 0,
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                        });
                    }
                    var returndata = dbdatases.Select(e => new
                    {

                        QualificationModel_Id = e.QualificationModelId,
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
        public async Task<ActionResult> EditSave(QualificationModelObject c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var QualificationModelId = TempData["QualificationId"];
                    int QualificationIds = Convert.ToInt32(QualificationModelId);
                    var LookupvalDetails = "";
                    var Creteriases = "";
                    var CriteriaTypeses = "";
                    var DataStepses = "";
                    string QualificationModellist = form["QualificationModelList"] == "" ? null : form["QualificationModelList"];
                    string CompetencyEvaluationModel = form["CompetencyEvaluationModelList"] == "" ? null : form["CompetencyEvaluationModelList"];
                    int QualificationLookupValuesId = Convert.ToInt32(QualificationModellist);

                    if(QualificationIds!=0)
                    {
                        var AppraisalQualificationModel = db.QualificationModelObject.Include(t => t.QualificationModel).ToList().Select(r => r.QualificationModel).Where(e => e.Id == QualificationIds).FirstOrDefault();
                        if (AppraisalQualificationModel != null)
                        {
                            Msg.Add("Already Qualification Model Object Exits");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                   
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (QualificationModellist != null)
                            {
                                if (QualificationModellist != "")
                                {
                                    var val = db.LookupValue.Find(int.Parse(QualificationModellist));
                                    c.QualificationModel = val;

                                    var type = db.QualificationModelObject.Include(e => e.QualificationModel).Where(e => e.Id == data).FirstOrDefault();
                                    IList<QualificationModelObject> typedetails = null;
                                    if (type.QualificationModel != null)
                                    {
                                        typedetails = db.QualificationModelObject.Where(x => x.QualificationModel.Id == type.QualificationModel.Id && x.Id == data).ToList();
                                    }
                                    else
                                    {
                                        typedetails = db.QualificationModelObject.Where(x => x.Id == data).ToList();
                                    }

                                    foreach (var s in typedetails)
                                    {
                                        s.QualificationModel = c.QualificationModel;
                                        db.QualificationModelObject.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                                else
                                {
                                    var AppraisalQualificationModelDetails = db.QualificationModelObject.Include(e => e.QualificationModel).Where(x => x.Id == data).ToList();
                                    foreach (var s in AppraisalQualificationModelDetails)
                                    {
                                        s.QualificationModel = null;
                                        db.QualificationModelObject.Attach(s);
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

                                    var add = db.QualificationModelObject.Include(e => e.CompetencyEvaluationModel).Where(e => e.Id == data).FirstOrDefault();
                                    IList<QualificationModelObject> competencyevaluationmodeldetails = null;
                                    if (add.CompetencyEvaluationModel != null)
                                    {
                                        competencyevaluationmodeldetails = db.QualificationModelObject.Where(x => x.CompetencyEvaluationModel.Id == add.CompetencyEvaluationModel.Id && x.Id == data).ToList();
                                    }
                                    else
                                    {
                                        competencyevaluationmodeldetails = db.QualificationModelObject.Where(x => x.Id == data).ToList();
                                    }
                                    if (competencyevaluationmodeldetails != null)
                                    {
                                        foreach (var s in competencyevaluationmodeldetails)
                                        {
                                            s.CompetencyEvaluationModel = c.CompetencyEvaluationModel;
                                            db.QualificationModelObject.Attach(s);
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
                                    var competencyevaluationmodeldetails = db.QualificationModelObject.Include(e => e.CompetencyEvaluationModel).Where(x => x.Id == data).ToList();
                                    foreach (var s in competencyevaluationmodeldetails)
                                    {
                                        s.CompetencyEvaluationModel = null;
                                        db.QualificationModelObject.Attach(s);
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

                            var QualificationLookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "314").SingleOrDefault();
                            LookupvalDetails = QualificationLookup.LookupValues.ToList().Where(e => e.Id == QualificationLookupValuesId).Select(r => r.LookupVal).FirstOrDefault();                          
                            QualificationModelObject CMS_QualificationModelObject = db.QualificationModelObject.Find(data);
                            TempData["CurrRowVersion"] = CMS_QualificationModelObject.RowVersion;
                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {
                                c.DBTrack = new DBTrack
                                {
                                    CreatedBy = CMS_QualificationModelObject.DBTrack.CreatedBy == null ? null : CMS_QualificationModelObject.DBTrack.CreatedBy,
                                    CreatedOn = CMS_QualificationModelObject.DBTrack.CreatedOn == null ? null : CMS_QualificationModelObject.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                CMS_QualificationModelObject.Id = data;
                                CMS_QualificationModelObject.DBTrack = c.DBTrack;
                                db.QualificationModelObject.Attach(CMS_QualificationModelObject);
                                db.Entry(CMS_QualificationModelObject).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            ts.Complete();

                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = data, Val = LookupvalDetails + "" + Creteriases + " " + CriteriaTypeses + " " + DataStepses, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);                        
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