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
    public class PersonnelModelObjectController : Controller
    {
        // GET: PersonnelModelObject
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Personnel(string valueId)
        {
            TempData["PersonnelId"] = valueId;
            return View();
        }
        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var PersonnelModelLookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "719").SingleOrDefault();
                var PersonnelModelLookupvalues = PersonnelModelLookup.LookupValues.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(PersonnelModelLookupvalues, "Id", "LookupVal", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }  
    
        [HttpPost]
        public ActionResult Create(PersonnelModelObject c, FormCollection form)
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
                    string PersonnelModellist = form["PersonnelModellist"] == "" ? null : form["PersonnelModellist"];
                    string CompetencyEvaluationModel = form["CompetencyEvaluationModelList"] == "" ? null : form["CompetencyEvaluationModelList"];
                    int PersonnelLookupValuesIds = Convert.ToInt32(PersonnelModellist);
                    var PersonnelModel = db.PersonnelModelObject.Include(t => t.PersonnelModel).ToList().Select(r => r.PersonnelModel).Where(e => e.Id == PersonnelLookupValuesIds).FirstOrDefault();
                    if (PersonnelModel != null)
                    {
                        Msg.Add("Already Personnel Model Object Exits");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (PersonnelModellist != null && PersonnelModellist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(PersonnelModellist));
                        c.PersonnelModel = val;
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
                    var PersonnelLookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "719").SingleOrDefault();
                    LookupvalDetails = PersonnelLookup.LookupValues.ToList().Where(e => e.Id == PersonnelLookupValuesIds).Select(r => r.LookupVal).FirstOrDefault();

                    using (TransactionScope ts = new TransactionScope())
                    {
                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        PersonnelModelObject PersonnelModelObject = new PersonnelModelObject()
                        {
                            PersonnelModel = c.PersonnelModel,
                            CompetencyEvaluationModel = c.CompetencyEvaluationModel,
                            DBTrack = c.DBTrack
                        };
                        db.PersonnelModelObject.Add(PersonnelModelObject);
                        try
                        {
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = PersonnelModelObject.Id, Val = LookupvalDetails + " " + Creteriases + " " + CriteriaTypeses + " " + DataStepses, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
            public int PersonnelModelId { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var returndata = db.PersonnelModelObject
                //                  .Include(e => e.PersonnelModel)
                //                  .Include(e => e.CompetencyEvaluationModel)
                //                  .Include(e => e.CompetencyEvaluationModel.Criteria)
                //                  .Include(e => e.CompetencyEvaluationModel.CriteriaType)
                //                  .Include(e => e.CompetencyEvaluationModel.DataSteps)
                //                  .Where(e => e.Id == data)
                //    .Select(e => new
                //    {
                //        PersonnelModel_Id = e.PersonnelModel != null ? e.PersonnelModel.Id : 0,
                //        CompetencyEvaluationModel_Id = e.CompetencyEvaluationModel.Id == null ? "" : e.CompetencyEvaluationModel.Id.ToString(),
                //        CompetencyEvaluationModelDetails = e.CompetencyEvaluationModel.Criteria.LookupVal.ToString() + "," + e.CompetencyEvaluationModel.CriteriaType.LookupVal.ToString() + "," + e.CompetencyEvaluationModel.DataSteps.LookupVal.ToString() == null ? "" : e.CompetencyEvaluationModel.Criteria.LookupVal.ToString() + "," + e.CompetencyEvaluationModel.CriteriaType.LookupVal.ToString() + "," + e.CompetencyEvaluationModel.DataSteps.LookupVal.ToString()
                //    }).ToList();

                //return Json(new Object[] { returndata, JsonRequestBehavior.AllowGet });

                List<returnEditClass> dbdatases = new List<returnEditClass>();

                var dbdata = db.PersonnelModelObject
                                  .Include(e => e.PersonnelModel)
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
                            PersonnelModelId = item.PersonnelModel != null ? item.PersonnelModel.Id : 0,
                            CompetencyEvaluationModelId = item.CompetencyEvaluationModel != null ? item.CompetencyEvaluationModel.Id : 0,
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                        });
                    }
                    var returndata = dbdatases.Select(e => new
                    {

                        PersonnelModel_Id = e.PersonnelModelId,
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
        public async Task<ActionResult> EditSave(PersonnelModelObject c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var PersonnelModelId = TempData["PersonnelId"];
                    int PersonnelIds = Convert.ToInt32(PersonnelModelId);
                    var LookupvalDetails = "";
                    var Creteriases = "";
                    var CriteriaTypeses = "";
                    var DataStepses = "";
                    string PersonnelModellist = form["PersonnelModellist"] == "" ? null : form["PersonnelModellist"];
                    string CompetencyEvaluationModel = form["CompetencyEvaluationModelList"] == "" ? null : form["CompetencyEvaluationModelList"];

                    int PersonelLookupValuesId = Convert.ToInt32(PersonnelModellist);

                    if (PersonnelIds != 0)
                    {
                        var AppraisalPersonnelModel = db.PersonnelModelObject.Include(t => t.PersonnelModel).ToList().Select(r => r.PersonnelModel).Where(e => e.Id == PersonnelIds).FirstOrDefault();
                        if (AppraisalPersonnelModel != null)
                        {
                            Msg.Add("Already Personnel Model Object Exits");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (PersonnelModellist != null)
                        {
                            if (PersonnelModellist != "")
                            {
                                var val = db.LookupValue.Find(int.Parse(PersonnelModellist));
                                c.PersonnelModel = val;

                                var type = db.PersonnelModelObject.Include(e => e.PersonnelModel).Where(e => e.Id == data).FirstOrDefault();
                                IList<PersonnelModelObject> typedetails = null;
                                if (type.PersonnelModel != null)
                                {
                                    typedetails = db.PersonnelModelObject.Where(x => x.PersonnelModel.Id == type.PersonnelModel.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    typedetails = db.PersonnelModelObject.Where(x => x.Id == data).ToList();
                                }

                                foreach (var s in typedetails)
                                {
                                    s.PersonnelModel = c.PersonnelModel;
                                    db.PersonnelModelObject.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            else
                            {
                                var AppraisalPersonnelModelDetails = db.PersonnelModelObject.Include(e => e.PersonnelModel).Where(x => x.Id == data).ToList();
                                foreach (var s in AppraisalPersonnelModelDetails)
                                {
                                    s.PersonnelModel = null;
                                    db.PersonnelModelObject.Attach(s);
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

                                var add = db.PersonnelModelObject.Include(e => e.CompetencyEvaluationModel).Where(e => e.Id == data).FirstOrDefault();
                                IList<PersonnelModelObject> competencyevaluationmodeldetails = null;
                                if (add.CompetencyEvaluationModel != null)
                                {
                                    competencyevaluationmodeldetails = db.PersonnelModelObject.Where(x => x.CompetencyEvaluationModel.Id == add.CompetencyEvaluationModel.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    competencyevaluationmodeldetails = db.PersonnelModelObject.Where(x => x.Id == data).ToList();
                                }
                                if (competencyevaluationmodeldetails != null)
                                {
                                    foreach (var s in competencyevaluationmodeldetails)
                                    {
                                        s.CompetencyEvaluationModel = c.CompetencyEvaluationModel;
                                        db.PersonnelModelObject.Attach(s);
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
                                var competencyevaluationmodeldetails = db.PersonnelModelObject.Include(e => e.CompetencyEvaluationModel).Where(x => x.Id == data).ToList();
                                foreach (var s in competencyevaluationmodeldetails)
                                {
                                    s.CompetencyEvaluationModel = null;
                                    db.PersonnelModelObject.Attach(s);
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

                        var PersonnelLookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "719").SingleOrDefault();
                        LookupvalDetails = PersonnelLookup.LookupValues.ToList().Where(e => e.Id == PersonelLookupValuesId).Select(r => r.LookupVal).FirstOrDefault();

                        PersonnelModelObject CMS_PersonnelModelObject = db.PersonnelModelObject.Find(data);

                        TempData["CurrRowVersion"] = CMS_PersonnelModelObject.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = CMS_PersonnelModelObject.DBTrack.CreatedBy == null ? null : CMS_PersonnelModelObject.DBTrack.CreatedBy,
                                CreatedOn = CMS_PersonnelModelObject.DBTrack.CreatedOn == null ? null : CMS_PersonnelModelObject.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            CMS_PersonnelModelObject.Id = data;
                            CMS_PersonnelModelObject.DBTrack = c.DBTrack;
                            db.PersonnelModelObject.Attach(CMS_PersonnelModelObject);
                            db.Entry(CMS_PersonnelModelObject).State = System.Data.Entity.EntityState.Modified;
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