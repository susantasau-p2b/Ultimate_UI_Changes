using CMS_SPS;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Transactions;
using P2b.Global;
using P2BUltimate.Security;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;

namespace P2BUltimate.Controllers.CMS
{
    public class TrainingModelController : Controller
    {
        // GET: TrainingModel
        public ActionResult Index()
        {
            return View("~/Views/CMS/MainViews/TrainingModel/Index.cshtml");
        }
        public ActionResult TrainingModelPartial()
        {
            return View("~/Views/Shared/CMS/TrainingModelObject.cshtml");
        }
        public class returnDataClass
        {
            public int Id { get; set; }
            public string TrainingModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public ActionResult GetLookupTrainingModelObject(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var dbdata = db.TrainingModelObject.Include(e => e.TrainingModel)
                .Include(e => e.CompetencyEvaluationModel).Include(e => e.CompetencyEvaluationModel.Criteria)
                .Include(e => e.CompetencyEvaluationModel.CriteriaType)
                .Include(e => e.CompetencyEvaluationModel.DataSteps).ToList();
                if (dbdata != null)
                {
                    List<returnDataClass> returndata = new List<returnDataClass>();

                    foreach (var item in dbdata)
                    {
                        returndata.Add(new returnDataClass
                        {
                            Id = item.Id,
                            TrainingModel = item.TrainingModel != null ? db.ProgramList.Where(e => e.Id == item.TrainingModel.Id).Select(r => r.FullDetails).FirstOrDefault() : " ",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                        });

                    }
                    var res = (from ca in returndata
                               select new
                               {
                                   srno = ca.Id,
                                   lookupvalue = ca.TrainingModel + " " + ca.Creteriases + " " + ca.CreteriaTypeses + " " + ca.DataStepses
                               }).ToList();

                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                return null;

            }

        }
        [HttpPost]
        public ActionResult Create(TrainingModel c, FormCollection form)
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    CompanyCMS_SPS companycms_sps = new CompanyCMS_SPS();
                    companycms_sps = db.CompanyCMS_SPS.Include(e => e.TrainingModel).Where(e => e.Company.Id == company_Id).FirstOrDefault();

                    string TrainingModelObjectList = form["TrainingModelObjectList"] == "0" ? null : form["TrainingModelObjectList"];
                    if (TrainingModelObjectList != null && TrainingModelObjectList != "")
                    {
                        var ids = Utility.StringIdsToListIds(TrainingModelObjectList);
                        var TrainingModelObjectlist = new List<TrainingModelObject>();
                        foreach (var item in ids)
                        {
                            var returndata = db.TrainingModelObject.Include(e => e.TrainingModel)
                                .Include(e => e.CompetencyEvaluationModel).Include(e => e.CompetencyEvaluationModel.Criteria)
                                .Include(e => e.CompetencyEvaluationModel.CriteriaType).Include(e => e.CompetencyEvaluationModel.DataSteps)
                                .Where(e => e.Id == item).SingleOrDefault();

                            if (returndata != null && returndata.TrainingModel != null && returndata.CompetencyEvaluationModel != null)
                            {
                                var trainingmodel = returndata.TrainingModel;
                                var evaluationmodel = returndata.CompetencyEvaluationModel;
                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
                                TrainingModelObject objtrainingmodel = new TrainingModelObject()
                                {
                                    TrainingModel = trainingmodel,
                                    CompetencyEvaluationModel = evaluationmodel,
                                    DBTrack = c.DBTrack
                                };
                                TrainingModelObjectlist.Add(objtrainingmodel);
                            }
                        }
                        c.TrainingModelObject = TrainingModelObjectlist;
                    }
                    else
                    {
                        Msg.Add("please select training object");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (TrainingModelObjectList != null && TrainingModelObjectList != "")
                    {
                        var ids = Utility.StringIdsToListIds(TrainingModelObjectList);
                        List<int> AuthorList = new List<int>();
                        foreach (var ca in ids)
                        {
                            var TrainingModelList = db.TrainingModelObject.Include(e => e.TrainingModel).Where(e => e.Id == ca).FirstOrDefault();
                            if (TrainingModelList != null)
                            {
                                int TrainingModelId = TrainingModelList.TrainingModel!= null ? TrainingModelList.TrainingModel.Id : 0;
                                //int TrainingFullDetailsId = db.ProgramList.Where(e => e.Id == TrainingModelId).SingleOrDefault().Id;
                                //AuthorList.Add(TrainingFullDetailsId);

                            }

                        }

                        var duplicateExists = AuthorList.GroupBy(x => x)
                                             .Where(g => g.Count() > 1)
                                              .Select(y => y.Key)
                                               .ToList();
                        if (duplicateExists.Count() > 0)
                        {
                            Msg.Add("duplicate training object not allow");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }


                    var CodeValue = db.TrainingModel.Where(e => e.Code == c.Code).FirstOrDefault();
                    if (CodeValue != null)
                    {
                        Msg.Add("Already Code Is Insert");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            TrainingModel TrainingModel = new TrainingModel()
                            {
                                Code = c.Code,
                                ModelName = c.ModelName,
                                ModelDescription = c.ModelDescription,
                                CreatedDate = DateTime.Now.Date,
                                TrainingModelObject = c.TrainingModelObject,
                                DBTrack = c.DBTrack
                            };

                            db.TrainingModel.Add(TrainingModel);
                            db.SaveChanges();

                            if (companycms_sps != null)
                            {
                                List<TrainingModel> appraisalTrainingModelmodel_list = new List<TrainingModel>();
                                appraisalTrainingModelmodel_list.Add(TrainingModel);
                                if (companycms_sps.TrainingModel != null)
                                {
                                    appraisalTrainingModelmodel_list.AddRange(companycms_sps.TrainingModel);
                                }
                                companycms_sps.TrainingModel = appraisalTrainingModelmodel_list;
                                db.CompanyCMS_SPS.Attach(companycms_sps);
                                db.Entry(companycms_sps).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(companycms_sps).State = System.Data.Entity.EntityState.Detached;

                            }
                            try
                            {
                                

                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
            public string TrainingModelObject_Id { get; set; }
            public string TrainingModelObject_FullDetails { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                var returndata = db.TrainingModel.Include(e => e.TrainingModelObject)
                     .Where(e => e.Id == data).ToList();

                foreach (var item in returndata)
                {
                    result.Add(new returndatagridDataclass
                    {
                        Id = item.Id.ToString(),
                        Code = item.Code,
                        ModelName = item.ModelName,
                        ModelDescription = item.ModelDescription,
                        CreatedDate = item.CreatedDate.Value.ToShortDateString()

                    });
                }
                var return_data = db.TrainingModel.Include(e => e.TrainingModelObject)
                    .Include(e => e.TrainingModelObject.Select(r => r.TrainingModel))
                      .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel))
                        .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                          .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                    .Where(e => e.Id == data && e.TrainingModelObject.Count > 0).SingleOrDefault();

                List<returnDataClass> returndatases = new List<returnDataClass>();
                if (return_data != null && return_data.TrainingModelObject != null)
                {
                    foreach (var item in return_data.TrainingModelObject)
                    {
                        returndatases.Add(new returnDataClass
                        {
                            Id = item.Id,
                            TrainingModel = item.TrainingModel != null ? db.ProgramList.Where(e => e.Id == item.TrainingModel.Id).Select(r => r.FullDetails).FirstOrDefault() : "",
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                        });
                    }
                }
                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                if (returndatases != null)
                {

                    foreach (var item1 in returndatases)
                    {
                        oreturnEditClass.Add(new returnEditClass
                        {
                            TrainingModelObject_Id = item1.Id.ToString(),
                            TrainingModelObject_FullDetails = item1.TrainingModel + " " + item1.Creteriases + " " + item1.CreteriaTypeses + "" + item1.DataStepses
                        });
                    }
                }

                return Json(new Object[] { result, oreturnEditClass, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(TrainingModel c, int data, FormCollection form)
        {

            List<string> Msg = new List<string>();
            string TrainingModelObjectList = form["TrainingModelObjectList"] == "0" ? null : form["TrainingModelObjectList"];
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {                       
                        if (TrainingModelObjectList != "" && TrainingModelObjectList != null)
                        {
                            var ids = Utility.StringIdsToListIds(TrainingModelObjectList);
                            List<int> AuthorList = new List<int>();
                            foreach (var ca in ids)
                            {
                                var TrainingModelList = db.TrainingModelObject.Include(e => e.TrainingModel).Where(e => e.Id == ca).FirstOrDefault();
                                if (TrainingModelList != null)
                                {
                                    int TrainingModelId = TrainingModelList.TrainingModel != null ? TrainingModelList.TrainingModel.Id : 0;
                                    //int TrainingFullDetailsId = db.ProgramList.Where(e => e.Id == TrainingModelId).SingleOrDefault().Id;
                                    //AuthorList.Add(TrainingFullDetailsId);

                                }

                            }

                            var duplicateExists = AuthorList.GroupBy(x => x)
                                                 .Where(g => g.Count() > 1)
                                                  .Select(y => y.Key)
                                                   .ToList();
                            if (duplicateExists.Count() > 0)
                            {
                                Msg.Add("duplicate training object not allow");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                        }


                        var db_data = db.TrainingModel.Include(e => e.TrainingModelObject).Where(e => e.Id == data).SingleOrDefault();

                        List<TrainingModelObject> CMS_TrainingModelObjectList = new List<TrainingModelObject>();
                        if (TrainingModelObjectList != "" && TrainingModelObjectList != null)
                        {
                            var ids = Utility.StringIdsToListIds(TrainingModelObjectList);
                            foreach (var ca in ids)
                            {

                                var TrainingModelObject_val = db.TrainingModelObject.Find(ca);
                                CMS_TrainingModelObjectList.Add(TrainingModelObject_val);
                                db_data.TrainingModelObject = CMS_TrainingModelObjectList;
                            }
                            db_data.TrainingModelObject = CMS_TrainingModelObjectList;
                        }
                        else
                        {
                            Msg.Add("please select attribute object");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }


                        db.TrainingModel.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;
                     
                        TrainingModel CMS_TrainingModel = db.TrainingModel.Find(data);
                        TempData["CurrRowVersion"] = CMS_TrainingModel.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = CMS_TrainingModel.DBTrack.CreatedBy == null ? null : CMS_TrainingModel.DBTrack.CreatedBy,
                                CreatedOn = CMS_TrainingModel.DBTrack.CreatedOn == null ? null : CMS_TrainingModel.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            CMS_TrainingModel.Id = data;
                            CMS_TrainingModel.Code = c.Code;
                            CMS_TrainingModel.ModelName = c.ModelName;
                            CMS_TrainingModel.ModelDescription = c.ModelDescription;
                            CMS_TrainingModel.CreatedDate = c.CreatedDate;
                            CMS_TrainingModel.DBTrack = c.DBTrack;
                            CMS_TrainingModel.TrainingModelObject = db_data.TrainingModelObject;
                            db.TrainingModel.Attach(CMS_TrainingModel);
                            db.Entry(CMS_TrainingModel).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    TrainingModel trainingmodels = db.TrainingModel
                                                      .Include(e => e.TrainingModelObject)
                                                     .Where(e => e.Id == data).SingleOrDefault();

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {



                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = trainingmodels.DBTrack.CreatedBy != null ? trainingmodels.DBTrack.CreatedBy : null,
                            CreatedOn = trainingmodels.DBTrack.CreatedOn != null ? trainingmodels.DBTrack.CreatedOn : null,
                            IsModified = trainingmodels.DBTrack.IsModified == true ? false : false//,

                        };
                        db.Entry(trainingmodels).State = System.Data.Entity.EntityState.Deleted;
                        await db.SaveChangesAsync();
                        ts.Complete();
                        Msg.Add("  Data removed.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    //}
                }
                catch (RetryLimitExceededException /* dex */)
                {

                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
        public class returndatagridDataclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string ModelName { get; set; }
            public string ModelDescription { get; set; }
            public string CreatedDate { get; set; }

        }
        public class returndatagridChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string TrainingModel { get; set; }
            public string EvaluationModel { get; set; }

        }
        public ActionResult TrainingModel_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.TrainingModel.Include(e => e.TrainingModelObject)
                       .Include(e => e.TrainingModelObject.Select(r => r.TrainingModel))
                       .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel))
                       .ToList();

                    IEnumerable<TrainingModel> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.Code.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.ModelName.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }
                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<TrainingModel, string> orderfunc = (c =>
                                                               Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                               sortindex == 1 ? c.Code : "");
                    var sortcolumn = Request["sSortDir_0"];
                    if (sortcolumn == "asc")
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    else
                    {
                        fall = fall.OrderByDescending(orderfunc);
                    }

                    // Paging 
                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridDataclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Code,
                                ModelName = item.ModelName,
                                ModelDescription = item.ModelDescription,
                                CreatedDate = item.CreatedDate.Value.ToShortDateString()
                            });
                        }
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies
                                     select new[] { null, Convert.ToString(c.Id), c.Code, };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result

                        }, JsonRequestBehavior.AllowGet);



                    }

                }
                catch (Exception e)
                {
                    List<string> Msg = new List<string>();
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult A_TrainingModel_Grid(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.TrainingModel
                        .Include(e => e.TrainingModelObject)
                        .Include(e => e.TrainingModelObject.Select(r => r.TrainingModel))
                        .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel))
                        .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                        .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                        .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                        .Where(e => e.Id == data).SingleOrDefault();

                    if (db_data.TrainingModelObject!= null)
                    {

                        List<returnDataClass> returndata = new List<returnDataClass>();
                        foreach (var item in db_data.TrainingModelObject)
                        {
                            returndata.Add(new returnDataClass
                            {
                                Id = item.Id,
                                TrainingModel = item.TrainingModel != null ? db.LookupValue.Where(e => e.Id == item.TrainingModel.Id).Select(r => r.LookupVal).FirstOrDefault() : "",
                                DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                                Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                                CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                            });
                        }
                        List<returndatagridChildDataClass> returndatases = new List<returndatagridChildDataClass>();
                        foreach (var item1 in returndata)
                        {
                            returndatases.Add(new returndatagridChildDataClass
                            {
                                Id = item1.Id,
                                TrainingModel= item1.TrainingModel,
                                EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

                            });
                        }

                        return Json(returndatases, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}