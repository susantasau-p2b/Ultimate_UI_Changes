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
    public class PastExperienceModelObjectController : Controller
    {
        // GET: PastExperienceObject
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult PastExperience(string valueId)
        {
            TempData["PastExprienceId"] = valueId;
            return View();
        }
        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var PastExperienceLookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "721").SingleOrDefault();
                var PastExperienceLookupvalues = PastExperienceLookup.LookupValues.ToList();

                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(PastExperienceLookupvalues, "Id", "LookupVal", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult Create(PastExperienceModelObject c, FormCollection form)
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
                    string PastExperienceModellist = form["PastExperienceModellist"] == "" ? null : form["PastExperienceModellist"];
                    string CompetencyEvaluationModel = form["CompetencyEvaluationModelList"] == "" ? null : form["CompetencyEvaluationModelList"];
                    int PastExperienceLookupValuesIds = Convert.ToInt32(PastExperienceModellist);
                    var PastExperienceModel = db.PastExperienceModelObject.Include(t => t.PastExperienceModel).ToList().Select(r => r.PastExperienceModel).Where(e => e.Id == PastExperienceLookupValuesIds).FirstOrDefault();
                    if (PastExperienceModel != null)
                    {
                        Msg.Add("Already Past Experience Model Object Exits");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (PastExperienceModellist != null && PastExperienceModellist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(PastExperienceModellist));
                        c.PastExperienceModel = val;
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
                    var PastExperienceLookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "721").SingleOrDefault();
                    LookupvalDetails = PastExperienceLookup.LookupValues.ToList().Where(e => e.Id == PastExperienceLookupValuesIds).Select(r => r.LookupVal).FirstOrDefault();

                    using (TransactionScope ts = new TransactionScope())
                    {
                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        PastExperienceModelObject PastExperienceModelObjectobj = new PastExperienceModelObject()
                        {
                            PastExperienceModel = c.PastExperienceModel,
                            CompetencyEvaluationModel = c.CompetencyEvaluationModel,
                            DBTrack = c.DBTrack
                        };
                        db.PastExperienceModelObject.Add(PastExperienceModelObjectobj);
                        try
                        {
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = PastExperienceModelObjectobj.Id, Val = LookupvalDetails + " " + Creteriases + " " + CriteriaTypeses + " " + DataStepses, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
            public int PastExperienceModelId { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var returndata = db.PastExperienceModelObject
                //                  .Include(e => e.PastExperienceModel)
                //                  .Include(e => e.CompetencyEvaluationModel)
                //                  .Include(e => e.CompetencyEvaluationModel.Criteria)
                //                  .Include(e => e.CompetencyEvaluationModel.CriteriaType)
                //                  .Include(e => e.CompetencyEvaluationModel.DataSteps)
                //                  .Where(e => e.Id == data)
                //    .Select(e => new
                //    {
                //        PastExperienceModelObject_Id = e.PastExperienceModel != null ? e.PastExperienceModel.Id : 0,
                //        CompetencyEvaluationModel_Id = e.CompetencyEvaluationModel.Id == null ? "" : e.CompetencyEvaluationModel.Id.ToString(),
                //        CompetencyEvaluationModelDetails = e.CompetencyEvaluationModel.Criteria.LookupVal.ToString() + "," + e.CompetencyEvaluationModel.CriteriaType.LookupVal.ToString() + "," + e.CompetencyEvaluationModel.DataSteps.LookupVal.ToString() == null ? "" : e.CompetencyEvaluationModel.Criteria.LookupVal.ToString() + "," + e.CompetencyEvaluationModel.CriteriaType.LookupVal.ToString() + "," + e.CompetencyEvaluationModel.DataSteps.LookupVal.ToString()
                //    }).ToList();

                List<returnEditClass> dbdatases = new List<returnEditClass>();

                var dbdata = db.PastExperienceModelObject
                                  .Include(e => e.PastExperienceModel)
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
                            PastExperienceModelId = item.PastExperienceModel != null ? item.PastExperienceModel.Id : 0,
                            CompetencyEvaluationModelId = item.CompetencyEvaluationModel != null ? item.CompetencyEvaluationModel.Id : 0,
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                        });
                    }
                    var returndata = dbdatases.Select(e => new
                    {

                        PastExperienceModelObject_Id = e.PastExperienceModelId,
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
        public async Task<ActionResult> EditSave(PastExperienceModelObject c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var PastExperienceModelId = TempData["PastExprienceId"];
                    int PastExperienceIds = Convert.ToInt32(PastExperienceModelId);
                    var LookupvalDetails = "";
                    var Creteriases = "";
                    var CriteriaTypeses = "";
                    var DataStepses = "";
                    string PastExperienceModellist = form["PastExperienceModellist"] == "" ? null : form["PastExperienceModellist"];
                    string CompetencyEvaluationModel = form["CompetencyEvaluationModelList"] == "" ? null : form["CompetencyEvaluationModelList"];
                    int PastExperienceLookupValuesIds = Convert.ToInt32(PastExperienceModellist);

                    if (PastExperienceIds != 0)
                    {
                        var PastExperienceModel = db.PastExperienceModelObject.Include(t => t.PastExperienceModel).ToList().Select(r => r.PastExperienceModel).Where(e => e.Id == PastExperienceIds).FirstOrDefault();
                        if (PastExperienceModel != null)
                        {
                            Msg.Add("Already Past Experience Model Object Exits");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (PastExperienceModellist != null)
                        {
                            if (PastExperienceModellist != "")
                            {
                                var val = db.LookupValue.Find(int.Parse(PastExperienceModellist));
                                c.PastExperienceModel = val;

                                var type = db.PastExperienceModelObject.Include(e => e.PastExperienceModel).Where(e => e.Id == data).FirstOrDefault();
                                IList<PastExperienceModelObject> typedetails = null;
                                if (type.PastExperienceModel != null)
                                {
                                    typedetails = db.PastExperienceModelObject.Where(x => x.PastExperienceModel.Id == type.PastExperienceModel.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    typedetails = db.PastExperienceModelObject.Where(x => x.Id == data).ToList();
                                }

                                foreach (var s in typedetails)
                                {
                                    s.PastExperienceModel = c.PastExperienceModel;
                                    db.PastExperienceModelObject.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            else
                            {
                                var PastExperienceModelDetails = db.PastExperienceModelObject.Include(e => e.PastExperienceModel).Where(x => x.Id == data).ToList();
                                foreach (var s in PastExperienceModelDetails)
                                {
                                    s.PastExperienceModel = null;
                                    db.PastExperienceModelObject.Attach(s);
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

                                var add = db.PastExperienceModelObject.Include(e => e.CompetencyEvaluationModel).Where(e => e.Id == data).FirstOrDefault();
                                IList<PastExperienceModelObject> competencyevaluationmodeldetails = null;
                                if (add.CompetencyEvaluationModel != null)
                                {
                                    competencyevaluationmodeldetails = db.PastExperienceModelObject.Where(x => x.CompetencyEvaluationModel.Id == add.CompetencyEvaluationModel.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    competencyevaluationmodeldetails = db.PastExperienceModelObject.Where(x => x.Id == data).ToList();
                                }
                                if (competencyevaluationmodeldetails != null)
                                {
                                    foreach (var s in competencyevaluationmodeldetails)
                                    {
                                        s.CompetencyEvaluationModel = c.CompetencyEvaluationModel;
                                        db.PastExperienceModelObject.Attach(s);
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
                                var competencyevaluationmodeldetails = db.PastExperienceModelObject.Include(e => e.CompetencyEvaluationModel).Where(x => x.Id == data).ToList();
                                foreach (var s in competencyevaluationmodeldetails)
                                {
                                    s.CompetencyEvaluationModel = null;
                                    db.PastExperienceModelObject.Attach(s);
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

                        var PastExperienceLookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "721").SingleOrDefault();
                        LookupvalDetails = PastExperienceLookup.LookupValues.ToList().Where(e => e.Id == PastExperienceLookupValuesIds).Select(r => r.LookupVal).FirstOrDefault();
                        PastExperienceModelObject CMS_PastExperienceModelObject = db.PastExperienceModelObject.Find(data);
                        TempData["CurrRowVersion"] = CMS_PastExperienceModelObject.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = CMS_PastExperienceModelObject.DBTrack.CreatedBy == null ? null : CMS_PastExperienceModelObject.DBTrack.CreatedBy,
                                CreatedOn = CMS_PastExperienceModelObject.DBTrack.CreatedOn == null ? null : CMS_PastExperienceModelObject.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            CMS_PastExperienceModelObject.Id = data;
                            CMS_PastExperienceModelObject.DBTrack = c.DBTrack;
                            db.PastExperienceModelObject.Attach(CMS_PastExperienceModelObject);
                            db.Entry(CMS_PastExperienceModelObject).State = System.Data.Entity.EntityState.Modified;
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