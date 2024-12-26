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
    public class AppraisalBusinessAppraisalModelObjectController : Controller
    {
        // GET: AppraisalBusinessAppraisalModelObject
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult BusinessApp(string valueId)
        {
            TempData["BusinessAppId"] = valueId;
            return View();
        }
        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.BA_Category.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(fall, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult Create(AppraisalBusinessAppraisalModelObject c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var BADetails = "";
                    var Creteriases = "";
                    var CriteriaTypeses = "";
                    var DataStepses = "";
                    string AppraisalBusinessAppraisalModellist = form["AppraisalBusinessAppraisalModellist"] == "" ? null : form["AppraisalBusinessAppraisalModellist"];
                    string CompetencyEvaluationModel = form["CompetencyEvaluationModelList"] == "" ? null : form["CompetencyEvaluationModelList"];
                    int BAFullDetailsId = Convert.ToInt32(AppraisalBusinessAppraisalModellist);
                    var BAModel = db.AppraisalBusinessAppraisalModelObject.Include(t => t.AppraisalBusinessAppraisalModel).ToList();
                    if (BAModel != null)
                    {
                        foreach (var item in BAModel)
                        {
                            if (item != null && item.AppraisalBusinessAppraisalModel != null)
                            {
                                int ids = item.AppraisalBusinessAppraisalModel.Id;
                                if (BAFullDetailsId == ids)
                                {
                                    Msg.Add("Already Business Model Object Exits");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }

                    }

                    if (AppraisalBusinessAppraisalModellist != null && AppraisalBusinessAppraisalModellist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(AppraisalBusinessAppraisalModellist));
                        c.AppraisalBusinessAppraisalModel = val;
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

                    BADetails = db.BA_Category.Where(e => e.Id == BAFullDetailsId).SingleOrDefault().FullDetails;
                    using (TransactionScope ts = new TransactionScope())
                    {
                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        AppraisalBusinessAppraisalModelObject AppraisalBusinessAppraisalModelObject = new AppraisalBusinessAppraisalModelObject()
                        {
                            AppraisalBusinessAppraisalModel = c.AppraisalBusinessAppraisalModel,
                            CompetencyEvaluationModel = c.CompetencyEvaluationModel,
                            DBTrack = c.DBTrack
                        };
                        db.AppraisalBusinessAppraisalModelObject.Add(AppraisalBusinessAppraisalModelObject);
                        try
                        {
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = AppraisalBusinessAppraisalModelObject.Id, Val = BADetails + " " + Creteriases + " " + CriteriaTypeses + " " + DataStepses, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
            public int BAModelId { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var dbdata = db.AppraisalBusinessAppraisalModelObject
                                  .Include(e => e.AppraisalBusinessAppraisalModel)
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
                            BAModelId = item.AppraisalBusinessAppraisalModel != null ? item.AppraisalBusinessAppraisalModel.Id : 0,
                            CompetencyEvaluationModelId = item.CompetencyEvaluationModel != null ? item.CompetencyEvaluationModel.Id : 0,
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                        });
                    }
                    var returndata = dbdatases.Select(e => new
                    {
                        AppraisalBusinessAppraisalModelObject_Id = e.BAModelId,
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
        public async Task<ActionResult> EditSave(AppraisalBusinessAppraisalModelObject c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var BALookupvaluesId = TempData["BusinessAppId"];
                    int BAIds = Convert.ToInt32(BALookupvaluesId);
                    var BADetails = "";
                    var Creteriases = "";
                    var CriteriaTypeses = "";
                    var DataStepses = "";
                    string AppraisalBusinessAppraisalModellist = form["AppraisalBusinessAppraisalModellist"] == "" ? null : form["AppraisalBusinessAppraisalModellist"];
                    string CompetencyEvaluationModel = form["CompetencyEvaluationModelList"] == "" ? null : form["CompetencyEvaluationModelList"];
                    int BADetailsId = Convert.ToInt32(AppraisalBusinessAppraisalModellist);

                    if (BAIds != 0)
                    {
                        var BAModel = db.AppraisalBusinessAppraisalModelObject.Include(t => t.AppraisalBusinessAppraisalModel).ToList();
                        if (BAModel != null)
                        {
                            foreach (var item in BAModel)
                            {
                                if (item != null && item.AppraisalBusinessAppraisalModel != null)
                                {
                                    int ids = item.AppraisalBusinessAppraisalModel.Id;
                                    if (BAIds == ids)
                                    {
                                        Msg.Add("Already Business Model Object Exits");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }

                        }
                    }


                    BADetails = db.BA_Category.Where(e => e.Id == BADetailsId).SingleOrDefault().FullDetails;
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (AppraisalBusinessAppraisalModellist != null)
                        {
                            if (AppraisalBusinessAppraisalModellist != "")
                            {
                                var val = db.LookupValue.Find(int.Parse(AppraisalBusinessAppraisalModellist));
                                c.AppraisalBusinessAppraisalModel = val;

                                var type = db.AppraisalBusinessAppraisalModelObject.Include(e => e.AppraisalBusinessAppraisalModel).Where(e => e.Id == data).FirstOrDefault();
                                IList<AppraisalBusinessAppraisalModelObject> typedetails = null;
                                if (type.AppraisalBusinessAppraisalModel != null)
                                {
                                    typedetails = db.AppraisalBusinessAppraisalModelObject.Where(x => x.AppraisalBusinessAppraisalModel.Id == type.AppraisalBusinessAppraisalModel.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    typedetails = db.AppraisalBusinessAppraisalModelObject.Where(x => x.Id == data).ToList();
                                }

                                foreach (var s in typedetails)
                                {
                                    s.AppraisalBusinessAppraisalModel = c.AppraisalBusinessAppraisalModel;
                                    db.AppraisalBusinessAppraisalModelObject.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            else
                            {
                                var BAModelDetails = db.AppraisalBusinessAppraisalModelObject.Include(e => e.AppraisalBusinessAppraisalModel).Where(x => x.Id == data).ToList();
                                foreach (var s in BAModelDetails)
                                {
                                    s.AppraisalBusinessAppraisalModel = null;
                                    db.AppraisalBusinessAppraisalModelObject.Attach(s);
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

                                var add = db.AppraisalBusinessAppraisalModelObject.Include(e => e.CompetencyEvaluationModel).Where(e => e.Id == data).FirstOrDefault();
                                IList<AppraisalBusinessAppraisalModelObject> competencyevaluationmodeldetails = null;
                                if (add.CompetencyEvaluationModel != null)
                                {
                                    competencyevaluationmodeldetails = db.AppraisalBusinessAppraisalModelObject.Where(x => x.CompetencyEvaluationModel.Id == add.CompetencyEvaluationModel.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    competencyevaluationmodeldetails = db.AppraisalBusinessAppraisalModelObject.Where(x => x.Id == data).ToList();
                                }
                                if (competencyevaluationmodeldetails != null)
                                {
                                    foreach (var s in competencyevaluationmodeldetails)
                                    {
                                        s.CompetencyEvaluationModel = c.CompetencyEvaluationModel;
                                        db.AppraisalBusinessAppraisalModelObject.Attach(s);
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
                                var competencyevaluationmodeldetails = db.AppraisalBusinessAppraisalModelObject.Include(e => e.CompetencyEvaluationModel).Where(x => x.Id == data).ToList();
                                foreach (var s in competencyevaluationmodeldetails)
                                {
                                    s.CompetencyEvaluationModel = null;
                                    db.AppraisalBusinessAppraisalModelObject.Attach(s);
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

                        AppraisalBusinessAppraisalModelObject CMS_AppraisalBusinessAppraisalModelObject = db.AppraisalBusinessAppraisalModelObject.Find(data);
                        TempData["CurrRowVersion"] = CMS_AppraisalBusinessAppraisalModelObject.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = CMS_AppraisalBusinessAppraisalModelObject.DBTrack.CreatedBy == null ? null : CMS_AppraisalBusinessAppraisalModelObject.DBTrack.CreatedBy,
                                CreatedOn = CMS_AppraisalBusinessAppraisalModelObject.DBTrack.CreatedOn == null ? null : CMS_AppraisalBusinessAppraisalModelObject.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            CMS_AppraisalBusinessAppraisalModelObject.Id = data;
                            CMS_AppraisalBusinessAppraisalModelObject.DBTrack = c.DBTrack;
                            db.AppraisalBusinessAppraisalModelObject.Attach(CMS_AppraisalBusinessAppraisalModelObject);
                            db.Entry(CMS_AppraisalBusinessAppraisalModelObject).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        ts.Complete();

                        Msg.Add("  Record Updated");
                        return Json(new Utility.JsonReturnClass { Id = data, Val = BADetails + "" + Creteriases + " " + CriteriaTypeses + " " + DataStepses, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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