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
    public class QualificationModelController : Controller
    {
        // GET: QualificationModel
        public ActionResult Index()
        {
            return View("~/Views/CMS/MainViews/QualificationModel/index.cshtml");
        }
        public ActionResult QualificationModelPartial()
        {
            return View("~/Views/Shared/CMS/QualificationModelObject.cshtml");
        }
        public class returnDataClass
        {
            public int Id { get; set; }
            public string QualificationModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public ActionResult GetLookupQualificationModelObject()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.QualificationModelObject.Include(e => e.QualificationModel)
                   .Include(e => e.CompetencyEvaluationModel).Include(e => e.CompetencyEvaluationModel.Criteria)
                   .Include(e => e.CompetencyEvaluationModel.CriteriaType)
                   .Include(e => e.CompetencyEvaluationModel.DataSteps).ToList();

                if (fall != null)
                {
                    List<returnDataClass> returndata = new List<returnDataClass>();

                    foreach (var item in fall)
                    {
                        returndata.Add(new returnDataClass
                        {
                            Id = item.Id,
                            QualificationModel = item.QualificationModel != null ? item.QualificationModel.LookupVal : "",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                        });

                    }
                    var res = (from ca in returndata
                               select new
                               {
                                   srno = ca.Id,
                                   lookupvalue = ca.QualificationModel + " " + ca.Creteriases + " " + ca.CreteriaTypeses + " " + ca.DataStepses
                               }).ToList();

                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                return null;

            }
        }
       
        public class returnEditClass
        {
            public string QualificationModelObject_Id { get; set; }
            public string QualificationObject_FullDetails { get; set; }
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
            public string QualificationModel { get; set; }
            public string EvaluationModel { get; set; }

        }
        public ActionResult QualificationModel_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.QualificationModel.Include(e => e.QualificationModelObject)
                       .Include(e => e.QualificationModelObject.Select(r => r.QualificationModel))
                       .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel))
                       .ToList();

                    IEnumerable<QualificationModel> fall;
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

                    Func<QualificationModel, string> orderfunc = (c =>
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

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                var returndata = db.QualificationModel.Include(e => e.QualificationModelObject)
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
                var return_data = db.QualificationModel.Include(e => e.QualificationModelObject)
                    .Include(e => e.QualificationModelObject.Select(r => r.QualificationModel))
                      .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel))
                        .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                          .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                    .Where(e => e.Id == data && e.QualificationModelObject.Count > 0).SingleOrDefault();

                List<returnDataClass> returndatases = new List<returnDataClass>();
                if (return_data != null && return_data.QualificationModelObject != null)
                {
                    foreach (var item in return_data.QualificationModelObject)
                    {
                        returndatases.Add(new returnDataClass
                        {
                            Id = item.Id,
                            QualificationModel = item.QualificationModel != null ? item.QualificationModel.LookupVal : "",
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
                            QualificationModelObject_Id = item1.Id.ToString(),
                            QualificationObject_FullDetails = item1.QualificationModel + " " + item1.Creteriases + " " + item1.CreteriaTypeses + "" + item1.DataStepses
                        });
                    }
                }

                return Json(new Object[] { result, oreturnEditClass, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(QualificationModel c, int data, FormCollection form)
        {

            List<string> Msg = new List<string>();
            string QualificationModelObjectList = form["QualificationModelObjectList"] == "0" ? null : form["QualificationModelObjectList"];
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {


                        if (QualificationModelObjectList != null && QualificationModelObjectList != "")
                        {
                            var ids = Utility.StringIdsToListIds(QualificationModelObjectList);
                            List<int> AuthorList = new List<int>();
                            foreach (var ca in ids)
                            {
                                var QualificationModelList = db.QualificationModelObject.Include(e => e.QualificationModel).Where(e => e.Id == ca).FirstOrDefault();
                                if (QualificationModelList != null)
                                {
                                    int AppraisalQualificationModelId = QualificationModelList.QualificationModel != null ? QualificationModelList.QualificationModel.Id : 0;
                                    int AppCategoryFullDetailsId = db.LookupValue.Where(e => e.Id == AppraisalQualificationModelId).SingleOrDefault().Id;
                                    AuthorList.Add(AppCategoryFullDetailsId);

                                }

                            }

                            var duplicateExists = AuthorList.GroupBy(x => x)
                                                 .Where(g => g.Count() > 1)
                                                  .Select(y => y.Key)
                                                   .ToList();
                            if (duplicateExists.Count() > 0)
                            {
                                Msg.Add("duplicate qualification Object not allow");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        var db_data = db.QualificationModel.Include(e => e.QualificationModelObject).Where(e => e.Id == data).SingleOrDefault();

                        List<QualificationModelObject> CMS_QualificationModelObjectList = new List<QualificationModelObject>();
                        if (QualificationModelObjectList != "" && QualificationModelObjectList != null)
                        {
                            var ids = Utility.StringIdsToListIds(QualificationModelObjectList);
                            foreach (var ca in ids)
                            {

                                var QualificationModelObject_val = db.QualificationModelObject.Find(ca);
                                CMS_QualificationModelObjectList.Add(QualificationModelObject_val);
                                
                            }
                            db_data.QualificationModelObject = CMS_QualificationModelObjectList;
                        }
                        else
                        {
                            Msg.Add("please select qualification object");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        db.QualificationModel.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;
                        QualificationModel CMS_QualificationModel = db.QualificationModel.Find(data);
                        TempData["CurrRowVersion"] = CMS_QualificationModel.RowVersion;

                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = CMS_QualificationModel.DBTrack.CreatedBy == null ? null : CMS_QualificationModel.DBTrack.CreatedBy,
                                CreatedOn = CMS_QualificationModel.DBTrack.CreatedOn == null ? null : CMS_QualificationModel.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            CMS_QualificationModel.Id = data;
                            CMS_QualificationModel.Code = c.Code;
                            CMS_QualificationModel.ModelName = c.ModelName;
                            CMS_QualificationModel.ModelDescription = c.ModelDescription;
                            CMS_QualificationModel.CreatedDate = c.CreatedDate;
                            CMS_QualificationModel.DBTrack = c.DBTrack;
                            CMS_QualificationModel.QualificationModelObject = db_data.QualificationModelObject;
                            db.QualificationModel.Attach(CMS_QualificationModel);
                            db.Entry(CMS_QualificationModel).State = System.Data.Entity.EntityState.Modified;
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
                    QualificationModel qualificationmodels = db.QualificationModel
                                                      .Include(e => e.QualificationModelObject)
                                                     .Where(e => e.Id == data).SingleOrDefault();

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {



                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = qualificationmodels.DBTrack.CreatedBy != null ? qualificationmodels.DBTrack.CreatedBy : null,
                            CreatedOn = qualificationmodels.DBTrack.CreatedOn != null ? qualificationmodels.DBTrack.CreatedOn : null,
                            IsModified = qualificationmodels.DBTrack.IsModified == true ? false : false//,

                        };
                        db.Entry(qualificationmodels).State = System.Data.Entity.EntityState.Deleted;
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
        [HttpPost]
        public ActionResult Create(QualificationModel c, FormCollection form)
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    CompanyCMS_SPS companycms_sps = new CompanyCMS_SPS();
                    companycms_sps = db.CompanyCMS_SPS.Include(e => e.QualificationModel).Where(e => e.Company.Id == company_Id).FirstOrDefault();

                    string QualificationModelObjectList = form["QualificationModelObjectList"] == "0" ? null : form["QualificationModelObjectList"];

                    if (QualificationModelObjectList != null && QualificationModelObjectList != "")
                    {
                        var ids = Utility.StringIdsToListIds(QualificationModelObjectList);
                        var QualificationModelObjectlist = new List<QualificationModelObject>();
                        foreach (var item in ids)
                        {
                            var returndata = db.QualificationModelObject.Include(e => e.QualificationModel)
                                .Include(e => e.CompetencyEvaluationModel).Include(e => e.CompetencyEvaluationModel.Criteria)
                                .Include(e => e.CompetencyEvaluationModel.CriteriaType).Include(e => e.CompetencyEvaluationModel.DataSteps)
                                .Where(e => e.Id == item).SingleOrDefault();

                            if (returndata != null && returndata.QualificationModel != null && returndata.CompetencyEvaluationModel != null)
                            {
                                var qualificationmodel = returndata.QualificationModel;
                                var evaluationmodel = returndata.CompetencyEvaluationModel;
                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
                                QualificationModelObject objqualificationmodel = new QualificationModelObject()
                                {
                                     QualificationModel= qualificationmodel,
                                    CompetencyEvaluationModel = evaluationmodel,
                                    DBTrack = c.DBTrack
                                };
                                QualificationModelObjectlist.Add(objqualificationmodel);
                            }
                        }
                        c.QualificationModelObject = QualificationModelObjectlist;
                    }
                    else
                    {
                        Msg.Add("please select qualification object");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (QualificationModelObjectList != null && QualificationModelObjectList != "")
                    {
                        var ids = Utility.StringIdsToListIds(QualificationModelObjectList);
                        List<int> AuthorList = new List<int>();
                        foreach (var ca in ids)
                        {
                            var qualificationModelList = db.QualificationModelObject.Include(e => e.QualificationModel).Where(e => e.Id == ca).FirstOrDefault();
                            if (qualificationModelList != null)
                            {
                                int QualificationModelId = qualificationModelList.QualificationModel != null ? qualificationModelList.QualificationModel.Id : 0;
                                int AppCategoryFullDetailsId = db.LookupValue.Where(e => e.Id == QualificationModelId).SingleOrDefault().Id;
                                AuthorList.Add(AppCategoryFullDetailsId);

                            }

                        }

                        var duplicateExists = AuthorList.GroupBy(x => x)
                                             .Where(g => g.Count() > 1)
                                              .Select(y => y.Key)
                                               .ToList();
                        if (duplicateExists.Count() > 0)
                        {
                            Msg.Add("duplicate qualification Object not allow");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }


                    var CodeValue = db.QualificationModel.Where(e => e.Code == c.Code).FirstOrDefault();
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

                            QualificationModel ObjQualificationModel = new QualificationModel()
                            {

                                Code = c.Code,
                                ModelName = c.ModelName,
                                ModelDescription = c.ModelDescription,
                                CreatedDate = DateTime.Now.Date,
                                QualificationModelObject = c.QualificationModelObject,
                                DBTrack = c.DBTrack
                            };

                            db.QualificationModel.Add(ObjQualificationModel);
                            db.SaveChanges();

                            if (companycms_sps != null)
                            {
                                List<QualificationModel> appraisalqualificationmodel_list = new List<QualificationModel>();
                                appraisalqualificationmodel_list.Add(ObjQualificationModel);
                                if (companycms_sps.QualificationModel != null)
                                {
                                    appraisalqualificationmodel_list.AddRange(companycms_sps.QualificationModel);
                                }
                                companycms_sps.QualificationModel = appraisalqualificationmodel_list;
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
                        // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        public ActionResult A_QualificationModel_Grid(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.QualificationModel
                        .Include(e => e.QualificationModelObject)
                        .Include(e => e.QualificationModelObject.Select(r => r.QualificationModel))
                        .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel))
                        .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                        .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                        .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                        .Where(e => e.Id == data).SingleOrDefault();

                    if (db_data.QualificationModelObject != null)
                    {

                        List<returnDataClass> returndata = new List<returnDataClass>();
                        foreach (var item in db_data.QualificationModelObject)
                        {
                            returndata.Add(new returnDataClass
                            {
                                Id = item.Id,
                                QualificationModel = item.QualificationModel != null ? item.QualificationModel.LookupVal : "",
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
                                QualificationModel = item1.QualificationModel,
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

