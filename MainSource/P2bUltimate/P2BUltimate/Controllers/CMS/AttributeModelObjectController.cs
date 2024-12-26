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
    public class AttributeModelObjectController : Controller
    {
        //
        // GET: /AttributeModelObject/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Attribute(string valueId)
        {
            TempData["AttributeId"] = valueId;
            return View();
        }
        public class returnEditClass
        {
            public int Id { get; set; }
            public int CompetencyEvaluationModelId { get; set; }
            public int AppraisalAttributeModelId { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }

        [HttpPost]
        public ActionResult Create(AppraisalAttributeModelObject c, FormCollection form)
        {
           
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {                   
                    var AttributeFullDetails = "";
                    var Creteriases = "";
                    var CriteriaTypeses = "";
                    var DataStepses = "";
                    string AppraisalAttributeModellist = form["AppraisalAttributeModellist"] == "" ? null : form["AppraisalAttributeModellist"];
                    string CompetencyEvaluationModel = form["CompetencyEvaluationModelList"] == "" ? null : form["CompetencyEvaluationModelList"];
                    int AttributeFullDetailsId = Convert.ToInt32(AppraisalAttributeModellist);
                    var AttributeModel = db.AppraisalAttributeModelObject.Include(t => t.AppraisalAttributeModel).ToList();
                    if (AttributeModel != null)
                    {
                        foreach (var item in AttributeModel)
                        {
                            if (item != null && item.AppraisalAttributeModel != null)
                            {
                                int ids = item.AppraisalAttributeModel.Id;
                                if (AttributeFullDetailsId == ids)
                                {
                                    Msg.Add("Already Attribute Model Object Exits");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }

                    }

                    if (AppraisalAttributeModellist != null && AppraisalAttributeModellist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(AppraisalAttributeModellist));
                        c.AppraisalAttributeModel = val;

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

                    AttributeFullDetails = db.AppCategory.Where(e => e.Id == AttributeFullDetailsId).SingleOrDefault().FullDetails;

                    using (TransactionScope ts = new TransactionScope())
                    {
                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        AppraisalAttributeModelObject AppraisalAttributeModelObject = new AppraisalAttributeModelObject()
                        {


                            AppraisalAttributeModel = c.AppraisalAttributeModel,
                            CompetencyEvaluationModel = c.CompetencyEvaluationModel,
                            DBTrack = c.DBTrack
                        };

                        db.AppraisalAttributeModelObject.Add(AppraisalAttributeModelObject);                       
                        try
                        {

                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = AppraisalAttributeModelObject.Id, Val = AttributeFullDetails + " " + Creteriases + " " + CriteriaTypeses + " " + DataStepses, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                var dbdata = db.AppraisalAttributeModelObject
                                  .Include(e => e.AppraisalAttributeModel)
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
                            AppraisalAttributeModelId = item.AppraisalAttributeModel != null ? item.AppraisalAttributeModel.Id : 0,
                            CompetencyEvaluationModelId = item.CompetencyEvaluationModel != null ? item.CompetencyEvaluationModel.Id : 0,
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                        });
                    }
                    var returndata = dbdatases.Select(e => new
                    {

                        AppraisalAttributeModelObject_Id = e.AppraisalAttributeModelId,
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
        public async Task<ActionResult> EditSave(AppraisalAttributeModelObject c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var AttributeLookupvaluesId = TempData["AttributeId"];
                    int AttributeIds = Convert.ToInt32(AttributeLookupvaluesId);
                    var AttributeFullDetails = "";
                    var Creteriases = "";
                    var CriteriaTypeses = "";
                    var DataStepses = "";
                    string AppraisalAttributeModellist = form["AppraisalAttributeModellist"] == "" ? null : form["AppraisalAttributeModellist"];
                    string CompetencyEvaluationModel = form["CompetencyEvaluationModelList"] == "" ? null : form["CompetencyEvaluationModelList"];
                    int AttributeFullDetailsId = Convert.ToInt32(AppraisalAttributeModellist);
                    if (AttributeIds != 0)
                    {
                        var AppraisalAttributeModel = db.AppraisalAttributeModelObject.Include(t => t.AppraisalAttributeModel).ToList();
                        if (AppraisalAttributeModel != null)
                        {
                            foreach (var item in AppraisalAttributeModel)
                            {
                                if (item != null && item.AppraisalAttributeModel != null)
                                {
                                    int ids = item.AppraisalAttributeModel.Id;
                                    if (AttributeIds == ids)
                                    {
                                        Msg.Add("Already Attribute Model Object Exits");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }

                        }
                    }

                    AttributeFullDetails = db.AppCategory.Where(e => e.Id == AttributeFullDetailsId).SingleOrDefault().FullDetails;
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (AppraisalAttributeModellist != null)
                        {
                            if (AppraisalAttributeModellist != "")
                            {
                                var val = db.LookupValue.Find(int.Parse(AppraisalAttributeModellist));
                                c.AppraisalAttributeModel = val;

                                var type = db.AppraisalAttributeModelObject.Include(e => e.AppraisalAttributeModel).Where(e => e.Id == data).FirstOrDefault();
                                IList<AppraisalAttributeModelObject> typedetails = null;
                                if (type.AppraisalAttributeModel != null)
                                {
                                    typedetails = db.AppraisalAttributeModelObject.Where(x => x.AppraisalAttributeModel.Id == type.AppraisalAttributeModel.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    typedetails = db.AppraisalAttributeModelObject.Where(x => x.Id == data).ToList();
                                }

                                foreach (var s in typedetails)
                                {
                                    s.AppraisalAttributeModel = c.AppraisalAttributeModel;
                                    db.AppraisalAttributeModelObject.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            else
                            {
                                var AppraisalAttributeModelDetails = db.AppraisalAttributeModelObject.Include(e => e.AppraisalAttributeModel).Where(x => x.Id == data).ToList();
                                foreach (var s in AppraisalAttributeModelDetails)
                                {
                                    s.AppraisalAttributeModel = null;
                                    db.AppraisalAttributeModelObject.Attach(s);
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

                                var add = db.AppraisalAttributeModelObject.Include(e => e.CompetencyEvaluationModel).Where(e => e.Id == data).FirstOrDefault();
                                IList<AppraisalAttributeModelObject> competencyevaluationmodeldetails = null;
                                if (add.CompetencyEvaluationModel != null)
                                {
                                    competencyevaluationmodeldetails = db.AppraisalAttributeModelObject.Where(x => x.CompetencyEvaluationModel.Id == add.CompetencyEvaluationModel.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    competencyevaluationmodeldetails = db.AppraisalAttributeModelObject.Where(x => x.Id == data).ToList();
                                }
                                if (competencyevaluationmodeldetails != null)
                                {
                                    foreach (var s in competencyevaluationmodeldetails)
                                    {
                                        s.CompetencyEvaluationModel = c.CompetencyEvaluationModel;
                                        db.AppraisalAttributeModelObject.Attach(s);
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
                                var competencyevaluationmodeldetails = db.AppraisalAttributeModelObject.Include(e => e.CompetencyEvaluationModel).Where(x => x.Id == data).ToList();
                                foreach (var s in competencyevaluationmodeldetails)
                                {
                                    s.CompetencyEvaluationModel = null;
                                    db.AppraisalAttributeModelObject.Attach(s);
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

                        AppraisalAttributeModelObject CMS_AppraisalAttributeModelObject = db.AppraisalAttributeModelObject.Find(data);
                        TempData["CurrRowVersion"] = CMS_AppraisalAttributeModelObject.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = CMS_AppraisalAttributeModelObject.DBTrack.CreatedBy == null ? null : CMS_AppraisalAttributeModelObject.DBTrack.CreatedBy,
                                CreatedOn = CMS_AppraisalAttributeModelObject.DBTrack.CreatedOn == null ? null : CMS_AppraisalAttributeModelObject.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            CMS_AppraisalAttributeModelObject.Id = data;
                            CMS_AppraisalAttributeModelObject.DBTrack = c.DBTrack;
                            db.AppraisalAttributeModelObject.Attach(CMS_AppraisalAttributeModelObject);
                            db.Entry(CMS_AppraisalAttributeModelObject).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        ts.Complete();

                        Msg.Add("  Record Updated");
                        return Json(new Utility.JsonReturnClass { Id = data, Val = AttributeFullDetails + "" + Creteriases + " " + CriteriaTypeses + " " + DataStepses, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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