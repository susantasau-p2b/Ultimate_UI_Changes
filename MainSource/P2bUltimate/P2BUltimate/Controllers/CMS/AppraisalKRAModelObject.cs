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
    public class AppraisalKRAModelObjectController : Controller
    {
        // GET: AppraisalKRAModelObject

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult KRA(string valueId)
        {
            TempData["KRAId"] = valueId;
            return View();
        }
        [HttpPost]
        public ActionResult Create(AppraisalKRAModelObject c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var KRAFullDetails = "";
                    var Creteriases = "";
                    var CriteriaTypeses = "";
                    var DataStepses = "";
                    string AppraisalKRAModellist = form["AppraisalKRAModellist"] == "" ? null : form["AppraisalKRAModellist"];
                    string CompetencyEvaluationModel = form["CompetencyEvaluationModelList"] == "" ? null : form["CompetencyEvaluationModelList"];
                    int KRAFullDetailsId = Convert.ToInt32(AppraisalKRAModellist);
                   
                    var AppraisalkraModel = db.AppraisalKRAModelObject.Include(t => t.AppraisalKRAModel).ToList();
                    if (AppraisalkraModel != null)
                    {
                        foreach (var item in AppraisalkraModel)
                        {
                            if (item != null && item.AppraisalKRAModel != null)
                            {
                                int ids = item.AppraisalKRAModel.Id;
                                if (KRAFullDetailsId == ids)
                                {
                                    Msg.Add("Already KRA Model Object Exits");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }

                    }
                   
                    if (AppraisalKRAModellist != null && AppraisalKRAModellist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(AppraisalKRAModellist));
                        c.AppraisalKRAModel = val;
                    }

                    if (CompetencyEvaluationModel != null && CompetencyEvaluationModel != "")
                    {
                        int CompetencyEvaluationModelId = int.Parse(CompetencyEvaluationModel);
                        var val = db.CompetencyEvaluationModel.Include(e => e.DataSteps).
                            Include(e => e.Criteria).Include(e => e.CriteriaType)
                           .Where(e => e.Id == CompetencyEvaluationModelId).SingleOrDefault();
                        c.CompetencyEvaluationModel = val;
                    }

                    else
                    {
                        Msg.Add("please select evaluation model");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    KRAFullDetails = db.AppCategory.Where(e => e.Id == KRAFullDetailsId).SingleOrDefault().FullDetails;
                    Creteriases = c.CompetencyEvaluationModel != null ? c.CompetencyEvaluationModel.Criteria.LookupVal : " ";
                    CriteriaTypeses = c.CompetencyEvaluationModel != null ? c.CompetencyEvaluationModel.CriteriaType.LookupVal : " ";
                    DataStepses = c.CompetencyEvaluationModel != null && c.CompetencyEvaluationModel.DataSteps != null ? c.CompetencyEvaluationModel.DataSteps.LookupVal : " ";
                    using (TransactionScope ts = new TransactionScope())
                    {
                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        AppraisalKRAModelObject AppraisalKRAModelObject = new AppraisalKRAModelObject()
                        {
                            AppraisalKRAModel = c.AppraisalKRAModel,
                            CompetencyEvaluationModel = c.CompetencyEvaluationModel,
                            DBTrack = c.DBTrack
                        };
                        db.AppraisalKRAModelObject.Add(AppraisalKRAModelObject);
                        try
                        {
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = AppraisalKRAModelObject.Id, Val = KRAFullDetails + " " + Creteriases + " " + CriteriaTypeses + " " + DataStepses, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
            public int AppraisalKRAModelId { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var dbdata = db.AppraisalKRAModelObject
                                  .Include(e => e.AppraisalKRAModel)
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
                            AppraisalKRAModelId = item.AppraisalKRAModel != null ? item.AppraisalKRAModel.Id : 0,
                            CompetencyEvaluationModelId = item.CompetencyEvaluationModel != null ? item.CompetencyEvaluationModel.Id : 0,
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                        });
                    }
                    var returndata = dbdatases.Select(e => new
                    {

                        AppraisalKRAModelObject_Id = e.AppraisalKRAModelId,
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
        public async Task<ActionResult> EditSave(AppraisalKRAModelObject c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var KRALookupvaluesId = TempData["KRAId"];
                    int KRAIds = Convert.ToInt32(KRALookupvaluesId);
                    var KRAFullDetails = "";
                    var Creteriases = "";
                    var CriteriaTypeses = "";
                    var DataStepses = "";
                    string AppraisalKRAModellist = form["AppraisalKRAModellist"] == "" ? null : form["AppraisalKRAModellist"];
                    string CompetencyEvaluationModel = form["CompetencyEvaluationModelList"] == "" ? null : form["CompetencyEvaluationModelList"];
                    int KRAFullDetailsId = Convert.ToInt32(AppraisalKRAModellist);

                    
                    if (KRAIds != 0)
                    {
                        var AppraisalkraModel = db.AppraisalKRAModelObject.Include(t => t.AppraisalKRAModel).ToList();
                        if (AppraisalkraModel != null)
                        {
                            foreach (var item in AppraisalkraModel)
                            {
                                if (item != null && item.AppraisalKRAModel != null)
                                {
                                    int ids = item.AppraisalKRAModel.Id;
                                    if (KRAIds == ids)
                                    {
                                        Msg.Add("Already KRA Model Object Exits");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }

                        }
                    }

                    KRAFullDetails = db.AppCategory.Where(e => e.Id == KRAFullDetailsId).SingleOrDefault().FullDetails;

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {


                        if (AppraisalKRAModellist != null)
                        {
                            if (AppraisalKRAModellist != "")
                            {
                                var val = db.LookupValue.Find(int.Parse(AppraisalKRAModellist));
                                c.AppraisalKRAModel = val;

                                var type = db.AppraisalKRAModelObject.Include(e => e.AppraisalKRAModel).Where(e => e.Id == data).FirstOrDefault();
                                IList<AppraisalKRAModelObject> typedetails = null;
                                if (type.AppraisalKRAModel != null)
                                {
                                    typedetails = db.AppraisalKRAModelObject.Where(x => x.AppraisalKRAModel.Id == type.AppraisalKRAModel.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    typedetails = db.AppraisalKRAModelObject.Where(x => x.Id == data).ToList();
                                }

                                foreach (var s in typedetails)
                                {
                                    s.AppraisalKRAModel = c.AppraisalKRAModel;
                                    db.AppraisalKRAModelObject.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            else
                            {
                                var AppraisalKRAModelDetails = db.AppraisalKRAModelObject.Include(e => e.AppraisalKRAModel).Where(x => x.Id == data).ToList();
                                foreach (var s in AppraisalKRAModelDetails)
                                {
                                    s.AppraisalKRAModel = null;
                                    db.AppraisalKRAModelObject.Attach(s);
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

                                var add = db.AppraisalKRAModelObject.Include(e => e.CompetencyEvaluationModel).Where(e => e.Id == data).FirstOrDefault();
                                IList<AppraisalKRAModelObject> competencyevaluationmodeldetails = null;
                                if (add.CompetencyEvaluationModel != null)
                                {
                                    competencyevaluationmodeldetails = db.AppraisalKRAModelObject.Where(x => x.CompetencyEvaluationModel.Id == add.CompetencyEvaluationModel.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    competencyevaluationmodeldetails = db.AppraisalKRAModelObject.Where(x => x.Id == data).ToList();
                                }
                                if (competencyevaluationmodeldetails != null)
                                {
                                    foreach (var s in competencyevaluationmodeldetails)
                                    {
                                        s.CompetencyEvaluationModel = c.CompetencyEvaluationModel;
                                        db.AppraisalKRAModelObject.Attach(s);
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
                                var competencyevaluationmodeldetails = db.AppraisalKRAModelObject.Include(e => e.CompetencyEvaluationModel).Where(x => x.Id == data).ToList();
                                foreach (var s in competencyevaluationmodeldetails)
                                {
                                    s.CompetencyEvaluationModel = null;
                                    db.AppraisalKRAModelObject.Attach(s);
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


                        AppraisalKRAModelObject CMS_AppraisalKRAModelObject = db.AppraisalKRAModelObject.Find(data);
                        TempData["CurrRowVersion"] = CMS_AppraisalKRAModelObject.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = CMS_AppraisalKRAModelObject.DBTrack.CreatedBy == null ? null : CMS_AppraisalKRAModelObject.DBTrack.CreatedBy,
                                CreatedOn = CMS_AppraisalKRAModelObject.DBTrack.CreatedOn == null ? null : CMS_AppraisalKRAModelObject.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            CMS_AppraisalKRAModelObject.Id = data;
                            CMS_AppraisalKRAModelObject.DBTrack = c.DBTrack;
                            db.AppraisalKRAModelObject.Attach(CMS_AppraisalKRAModelObject);
                            db.Entry(CMS_AppraisalKRAModelObject).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        ts.Complete();
                        Msg.Add("  Record Updated");
                        return Json(new Utility.JsonReturnClass { Id = data, Val = KRAFullDetails + "" + Creteriases + " " + CriteriaTypeses + " " + DataStepses, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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