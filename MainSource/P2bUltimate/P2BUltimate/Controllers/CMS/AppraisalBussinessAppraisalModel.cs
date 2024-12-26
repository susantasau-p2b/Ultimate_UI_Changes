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
    public class AppraisalBusinessAppraisalModelController : Controller
    {
        // GET: AppraisalBusinessAppraisalModel
        public ActionResult Index()
        {
            return View("~/Views/CMS/MainViews/AppraisalBussinesAppriasalModel/Index.cshtml");
        }
        public ActionResult AppraisalBusinessAppraisalModelPartial()
        {
            return View("~/Views/Shared/CMS/AppraisalBussinessAppraisalModelObject.cshtml");
        }
        public class returnDataClass
        {
            public int Id { get; set; }
            public string BusinessModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public ActionResult GetLookupAppraisalBusinessAppraisalModelObject(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var dbdata = db.AppraisalBusinessAppraisalModelObject.Include(e => e.AppraisalBusinessAppraisalModel)
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
                            BusinessModel = item.AppraisalBusinessAppraisalModel != null ? db.BA_Category.Where(e => e.Id == item.AppraisalBusinessAppraisalModel.Id).Select(r => r.FullDetails).FirstOrDefault() : " ",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                        });

                    }
                    var res = (from ca in returndata
                               select new
                               {
                                   srno = ca.Id,
                                   lookupvalue = ca.BusinessModel + " " + ca.Creteriases + " " + ca.CreteriaTypeses + " " + ca.DataStepses
                               }).ToList();

                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                return null;

            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(AppraisalBusinessAppraisalModel c, int data, FormCollection form)
        {

            List<string> Msg = new List<string>();
            string AppraisalBusinessAppraisalModelObjectList = form["AppraisalBusinessAppraisalModelObjectList"] == "0" ? null : form["AppraisalBusinessAppraisalModelObjectList"];
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (AppraisalBusinessAppraisalModelObjectList != "" && AppraisalBusinessAppraisalModelObjectList != null)
                        {
                            var ids = Utility.StringIdsToListIds(AppraisalBusinessAppraisalModelObjectList);
                            List<int> AuthorList = new List<int>();
                            foreach (var ca in ids)
                            {
                                var BusinessModelList = db.AppraisalBusinessAppraisalModelObject.Include(e => e.AppraisalBusinessAppraisalModel).Where(e => e.Id == ca).FirstOrDefault();
                                if (BusinessModelList != null)
                                {
                                    int BusinessModelId = BusinessModelList.AppraisalBusinessAppraisalModel != null ? BusinessModelList.AppraisalBusinessAppraisalModel.Id : 0;
                                    int BusinessFullDetailsId = db.BA_Category.Where(e => e.Id == BusinessModelId).SingleOrDefault().Id;
                                    AuthorList.Add(BusinessFullDetailsId);

                                }

                            }

                            var duplicateExists = AuthorList.GroupBy(x => x)
                                                 .Where(g => g.Count() > 1)
                                                  .Select(y => y.Key)
                                                   .ToList();
                            if (duplicateExists.Count() > 0)
                            {
                                Msg.Add("duplicate business object not allow");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                        }


                        var db_data = db.AppraisalBusinessAppraisalModel.Include(e => e.AppraisalBusinessAppraisalModelObject).Where(e => e.Id == data).SingleOrDefault();
                        List<AppraisalBusinessAppraisalModelObject> CMS_BusinessModelObjectList = new List<AppraisalBusinessAppraisalModelObject>();

                        if (AppraisalBusinessAppraisalModelObjectList != "" && AppraisalBusinessAppraisalModelObjectList != null)
                        {
                            var ids = Utility.StringIdsToListIds(AppraisalBusinessAppraisalModelObjectList);
                            foreach (var ca in ids)
                            {

                                var BusinessModelObject_val = db.AppraisalBusinessAppraisalModelObject.Find(ca);
                                CMS_BusinessModelObjectList.Add(BusinessModelObject_val);
                                
                            }
                            db_data.AppraisalBusinessAppraisalModelObject = CMS_BusinessModelObjectList;
                        }
                        else
                        {
                            Msg.Add("please select attribute object");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }


                        db.AppraisalBusinessAppraisalModel.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;

                        AppraisalBusinessAppraisalModel CMS_BusinessAppraisalModel = db.AppraisalBusinessAppraisalModel.Find(data);
                        TempData["CurrRowVersion"] = CMS_BusinessAppraisalModel.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = CMS_BusinessAppraisalModel.DBTrack.CreatedBy == null ? null : CMS_BusinessAppraisalModel.DBTrack.CreatedBy,
                                CreatedOn = CMS_BusinessAppraisalModel.DBTrack.CreatedOn == null ? null : CMS_BusinessAppraisalModel.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            CMS_BusinessAppraisalModel.Id = data;
                            CMS_BusinessAppraisalModel.Code = c.Code;
                            CMS_BusinessAppraisalModel.ModelName = c.ModelName;
                            CMS_BusinessAppraisalModel.ModelDescription = c.ModelDescription;
                            CMS_BusinessAppraisalModel.CreatedDate = c.CreatedDate;
                            CMS_BusinessAppraisalModel.DBTrack = c.DBTrack;
                            CMS_BusinessAppraisalModel.AppraisalBusinessAppraisalModelObject = db_data.AppraisalBusinessAppraisalModelObject;
                            db.AppraisalBusinessAppraisalModel.Attach(CMS_BusinessAppraisalModel);
                            db.Entry(CMS_BusinessAppraisalModel).State = System.Data.Entity.EntityState.Modified;
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
        public ActionResult Create(AppraisalBusinessAppraisalModel c, FormCollection form)
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    CompanyCMS_SPS companycms_sps = new CompanyCMS_SPS();
                    companycms_sps = db.CompanyCMS_SPS.Include(e => e.AppraisalBusinessApprisalModel).Where(e => e.Company.Id == company_Id).FirstOrDefault();

                    string AppraisalBusinessAppraisalModelObjectList = form["AppraisalBusinessAppraisalModelObjectList"] == "0" ? null : form["AppraisalBusinessAppraisalModelObjectList"];
                    if (AppraisalBusinessAppraisalModelObjectList != null && AppraisalBusinessAppraisalModelObjectList != "")
                    {
                        var ids = Utility.StringIdsToListIds(AppraisalBusinessAppraisalModelObjectList);
                        var BusinessModelObjectlist = new List<AppraisalBusinessAppraisalModelObject>();
                        foreach (var item in ids)
                        {
                            var returndata = db.AppraisalBusinessAppraisalModelObject.Include(e => e.AppraisalBusinessAppraisalModel)
                                .Include(e => e.CompetencyEvaluationModel).Include(e => e.CompetencyEvaluationModel.Criteria)
                                .Include(e => e.CompetencyEvaluationModel.CriteriaType).Include(e => e.CompetencyEvaluationModel.DataSteps)
                                .Where(e => e.Id == item).SingleOrDefault();

                            if (returndata != null && returndata.AppraisalBusinessAppraisalModel != null && returndata.CompetencyEvaluationModel != null)
                            {
                                var businessmodel = returndata.AppraisalBusinessAppraisalModel;
                                var evaluationmodel = returndata.CompetencyEvaluationModel;
                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
                                AppraisalBusinessAppraisalModelObject objbusinessmodel = new AppraisalBusinessAppraisalModelObject()
                                {
                                    AppraisalBusinessAppraisalModel = businessmodel,
                                    CompetencyEvaluationModel = evaluationmodel,
                                    DBTrack = c.DBTrack
                                };
                                BusinessModelObjectlist.Add(objbusinessmodel);
                            }
                        }
                        c.AppraisalBusinessAppraisalModelObject = BusinessModelObjectlist;
                    }

                    else
                    {
                        Msg.Add("please select business object");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (AppraisalBusinessAppraisalModelObjectList != null && AppraisalBusinessAppraisalModelObjectList != "")
                    {
                        var ids = Utility.StringIdsToListIds(AppraisalBusinessAppraisalModelObjectList);
                        List<int> AuthorList = new List<int>();
                        foreach (var ca in ids)
                        {
                            var BusinessModelList = db.AppraisalBusinessAppraisalModelObject.Include(e => e.AppraisalBusinessAppraisalModel).Where(e => e.Id == ca).FirstOrDefault();
                            if (BusinessModelList != null)
                            {
                                int BusinessModelId = BusinessModelList.AppraisalBusinessAppraisalModel != null ? BusinessModelList.AppraisalBusinessAppraisalModel.Id : 0;
                                int BusinessFullDetailsId = db.BA_Category.Where(e => e.Id == BusinessModelId).SingleOrDefault().Id;
                                AuthorList.Add(BusinessFullDetailsId);

                            }

                        }

                        var duplicateExists = AuthorList.GroupBy(x => x)
                                             .Where(g => g.Count() > 1)
                                              .Select(y => y.Key)
                                               .ToList();
                        if (duplicateExists.Count() > 0)
                        {
                            Msg.Add("duplicate business object not allow");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }


                    var CodeValue = db.AppraisalBusinessAppraisalModel.Where(e => e.Code == c.Code).FirstOrDefault();
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
                            AppraisalBusinessAppraisalModel AppraisalBusinessAppraisalModel = new AppraisalBusinessAppraisalModel()
                            {
                                Code = c.Code,
                                ModelName = c.ModelName,
                                ModelDescription = c.ModelDescription,
                                CreatedDate = DateTime.Now.Date,
                                AppraisalBusinessAppraisalModelObject = c.AppraisalBusinessAppraisalModelObject,
                                DBTrack = c.DBTrack
                            };

                            db.AppraisalBusinessAppraisalModel.Add(AppraisalBusinessAppraisalModel);
                            db.SaveChanges();

                            if (companycms_sps != null)
                            {
                                List<AppraisalBusinessAppraisalModel> appraisalbusinessmodel_list = new List<AppraisalBusinessAppraisalModel>();
                                appraisalbusinessmodel_list.Add(AppraisalBusinessAppraisalModel);
                                if (companycms_sps.AppraisalBusinessApprisalModel != null)
                                {
                                    appraisalbusinessmodel_list.AddRange(companycms_sps.AppraisalBusinessApprisalModel);
                                }
                                companycms_sps.AppraisalBusinessApprisalModel = appraisalbusinessmodel_list;
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
            public string AppraisalBusinessAppraisalModel { get; set; }
            public string EvaluationModel { get; set; }
        }

        public class returnEditClass
        {
            public string AppraisalBusinessAppraisalModelObject_Id { get; set; }
            public string AppraisalBusinessAppraisalModelObject_FullDetails { get; set; }
        }
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    AppraisalBusinessAppraisalModel businessappraisalmodel = db.AppraisalBusinessAppraisalModel
                                                      .Include(e => e.AppraisalBusinessAppraisalModelObject)
                                                     .Where(e => e.Id == data).SingleOrDefault();

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {



                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = businessappraisalmodel.DBTrack.CreatedBy != null ? businessappraisalmodel.DBTrack.CreatedBy : null,
                            CreatedOn = businessappraisalmodel.DBTrack.CreatedOn != null ? businessappraisalmodel.DBTrack.CreatedOn : null,
                            IsModified = businessappraisalmodel.DBTrack.IsModified == true ? false : false//,

                        };
                        db.Entry(businessappraisalmodel).State = System.Data.Entity.EntityState.Deleted;
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
        public ActionResult Edit(int data)
        {           
            using (DataBaseContext db = new DataBaseContext())
            {
                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                var returndata = db.AppraisalBusinessAppraisalModel.Include(e => e.AppraisalBusinessAppraisalModelObject)
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
                var return_data = db.AppraisalBusinessAppraisalModel.Include(e => e.AppraisalBusinessAppraisalModelObject)
                    .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.AppraisalBusinessAppraisalModel))
                      .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel))
                        .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                          .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                    .Where(e => e.Id == data && e.AppraisalBusinessAppraisalModelObject.Count > 0).SingleOrDefault();

                List<returnDataClass> returndatases = new List<returnDataClass>();
                if (return_data != null && return_data.AppraisalBusinessAppraisalModelObject != null)
                {
                    foreach (var item in return_data.AppraisalBusinessAppraisalModelObject)
                    {
                        returndatases.Add(new returnDataClass
                        {
                            Id = item.Id,
                            BusinessModel = item.AppraisalBusinessAppraisalModel != null ? db.BA_Category.Where(e => e.Id == item.AppraisalBusinessAppraisalModel.Id).Select(r => r.FullDetails).FirstOrDefault() : "",
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
                            AppraisalBusinessAppraisalModelObject_Id = item1.Id.ToString(),
                            AppraisalBusinessAppraisalModelObject_FullDetails = item1.BusinessModel + " " + item1.Creteriases + " " + item1.CreteriaTypeses + "" + item1.DataStepses
                        });
                    }
                }

                return Json(new Object[] { result, oreturnEditClass, JsonRequestBehavior.AllowGet });
            }
        }
        
        public ActionResult AppraisalBusinessAppraisalModel_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.AppraisalBusinessAppraisalModel.Include(e => e.AppraisalBusinessAppraisalModelObject)
                       .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.AppraisalBusinessAppraisalModel))
                       .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel))
                       .ToList();

                    IEnumerable<AppraisalBusinessAppraisalModel> fall;
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

                    Func<AppraisalBusinessAppraisalModel, string> orderfunc = (c =>
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
        public ActionResult A_AppraisalBusinessAppraisalModel_Grid(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.AppraisalBusinessAppraisalModel
                        .Include(e => e.AppraisalBusinessAppraisalModelObject)
                        .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.AppraisalBusinessAppraisalModel))
                        .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel))
                        .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                        .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                        .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                        .Where(e => e.Id == data).SingleOrDefault();

                    if (db_data.AppraisalBusinessAppraisalModelObject != null)
                    {

                        List<returnDataClass> returndata = new List<returnDataClass>();
                        foreach (var item in db_data.AppraisalBusinessAppraisalModelObject)
                        {
                            returndata.Add(new returnDataClass
                            {
                                Id = item.Id,
                                BusinessModel = item.AppraisalBusinessAppraisalModel != null ? db.BA_Category.Where(e => e.Id == item.AppraisalBusinessAppraisalModel.Id).Select(r => r.FullDetails).FirstOrDefault() : "",
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
                                AppraisalBusinessAppraisalModel = item1.BusinessModel,
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