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
    public class AppraisalKRAModelController : Controller
    {
        // GET: AppraisalKRAModel
        public ActionResult Index()
        {
            return View("~/Views/CMS/MainViews/AppraisalKRAModel/Index.cshtml");
        }
        public ActionResult AppraisalKRAModelPartial()
        {
            return View("~/Views/Shared/CMS/AppraisalKRAModelObject.cshtml");
        }
        public class returnDataClass
        {
            public int Id { get; set; }
            public string KRAModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var AppModeLookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1069").SingleOrDefault();
                int AppModeId = AppModeLookup.LookupValues.Where(e => e.LookupVal.ToUpper() == "KRA").SingleOrDefault().Id;
                var qurey = db.AppCategory.Include(e => e.AppMode).Where(e => e.AppMode.Id == AppModeId).ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookupAppraisalKRAModelObject(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var dbdata = db.AppraisalKRAModelObject.Include(e => e.AppraisalKRAModel)
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
                            KRAModel = item.AppraisalKRAModel != null ? db.AppCategory.Where(e => e.Id == item.AppraisalKRAModel.Id).Select(r => r.FullDetails).FirstOrDefault() : " ",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                        });

                    }
                    var res = (from ca in returndata
                               select new
                               {
                                   srno = ca.Id,
                                   lookupvalue = ca.KRAModel + " " + ca.Creteriases + " " + ca.CreteriaTypeses + " " + ca.DataStepses
                               }).ToList();

                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                return null;

            }
        }
        [HttpPost]
        public ActionResult Create(AppraisalKRAModel c, FormCollection form)
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    CompanyCMS_SPS companycms_sps = new CompanyCMS_SPS();
                    companycms_sps = db.CompanyCMS_SPS.Include(e => e.AppraisalKRAModel).Where(e => e.Company.Id == company_Id).FirstOrDefault();

                    string AppraisalKRAModelObjectList = form["AppraisalKRAModelObjectList"] == "0" ? null : form["AppraisalKRAModelObjectList"];

                    if (AppraisalKRAModelObjectList != null && AppraisalKRAModelObjectList != "")
                    {
                        var ids = Utility.StringIdsToListIds(AppraisalKRAModelObjectList);
                        List<int> AuthorList = new List<int>();
                        foreach (var ca in ids)
                        {
                            var KRAModelList = db.AppraisalKRAModelObject.Include(e => e.AppraisalKRAModel).Where(e => e.Id == ca).FirstOrDefault();
                            if (KRAModelList != null)
                            {
                                int AppraisalKRAModelId = KRAModelList.AppraisalKRAModel != null ? KRAModelList.AppraisalKRAModel.Id : 0;
                                int AppCategoryFullDetailsId = db.AppCategory.Where(e => e.Id == AppraisalKRAModelId).SingleOrDefault().Id;
                                AuthorList.Add(AppCategoryFullDetailsId);

                            }

                        }

                        var duplicateExists = AuthorList.GroupBy(x => x)
                                             .Where(g => g.Count() > 1)
                                              .Select(y => y.Key)
                                               .ToList();
                        if (duplicateExists.Count() > 0)
                        {
                            Msg.Add("duplicate kra object not allow");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (AppraisalKRAModelObjectList != null)
                    {
                        var ids = Utility.StringIdsToListIds(AppraisalKRAModelObjectList);
                        var AppraisalKRAModelObjectlist = new List<AppraisalKRAModelObject>();
                        foreach (var item in ids)
                        {
                            var returndata = db.AppraisalKRAModelObject.Include(e => e.AppraisalKRAModel)
                                .Include(e => e.CompetencyEvaluationModel).Include(e => e.CompetencyEvaluationModel.Criteria)
                                .Include(e => e.CompetencyEvaluationModel.CriteriaType).Include(e => e.CompetencyEvaluationModel.DataSteps)
                                .Where(e => e.Id == item).SingleOrDefault();

                            if (returndata != null && returndata.AppraisalKRAModel != null && returndata.CompetencyEvaluationModel!=null)
                            {   var appraisalkramodel=returndata.AppraisalKRAModel;
                                var evaluationmodel=returndata.CompetencyEvaluationModel;
                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
                                AppraisalKRAModelObject objkramodel = new AppraisalKRAModelObject()
                                {
                                    AppraisalKRAModel =appraisalkramodel,
                                    CompetencyEvaluationModel=evaluationmodel,
                                    DBTrack = c.DBTrack
                                };
                                AppraisalKRAModelObjectlist.Add(objkramodel);
                            }
                        }
                        c.AppraisalKRAModelObject = AppraisalKRAModelObjectlist;
                    }
                    else
                    {
                        Msg.Add("please select KRA object");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    var CodeValue = db.AppraisalKRAModel.Where(e => e.Code == c.Code).FirstOrDefault();
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
                            AppraisalKRAModel AppraisalKRAModel = new AppraisalKRAModel()
                            {
                                Code = c.Code,
                                ModelName = c.ModelName,
                                ModelDescription = c.ModelDescription,
                                CreatedDate = DateTime.Now.Date,
                                AppraisalKRAModelObject = c.AppraisalKRAModelObject,
                                DBTrack = c.DBTrack
                            };

                            db.AppraisalKRAModel.Add(AppraisalKRAModel);
                            db.SaveChanges();

                            if (companycms_sps != null)
                            {
                                List<AppraisalKRAModel> appraisalkramodel_list = new List<AppraisalKRAModel>();
                                appraisalkramodel_list.Add(AppraisalKRAModel);
                                if (companycms_sps.AppraisalKRAModel != null)
                                {
                                    appraisalkramodel_list.AddRange(companycms_sps.AppraisalKRAModel);
                                }
                                companycms_sps.AppraisalKRAModel = appraisalkramodel_list;
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
            public string AppraisalKRAModel { get; set; }
            public string EvaluationModel { get; set; }
        }
        public class returnEditClass
        {
            public string AppraisalKRAModelObject_Id { get; set; }
            public string AppraisalKRAModelObject_FullDetails { get; set; }
        }
       
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                var returndata = db.AppraisalKRAModel.Include(e => e.AppraisalKRAModelObject)
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

                var return_data = db.AppraisalKRAModel.Include(e => e.AppraisalKRAModelObject)
                    .Include(e => e.AppraisalKRAModelObject.Select(r => r.AppraisalKRAModel))
                      .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel))
                        .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                          .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                    .Where(e => e.Id == data && e.AppraisalKRAModelObject.Count > 0).SingleOrDefault();

                List<returnDataClass> returndatases = new List<returnDataClass>();
                if (return_data != null && return_data.AppraisalKRAModelObject != null)
                {
                    foreach (var item in return_data.AppraisalKRAModelObject)
                    {
                        returndatases.Add(new returnDataClass
                        {
                            Id = item.Id,
                            KRAModel = item.AppraisalKRAModel != null ? db.AppCategory.Where(e => e.Id == item.AppraisalKRAModel.Id).Select(r => r.FullDetails).FirstOrDefault() : "",
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
                            AppraisalKRAModelObject_Id = item1.Id.ToString(),
                            AppraisalKRAModelObject_FullDetails = item1.KRAModel + " " + item1.Creteriases + " " + item1.CreteriaTypeses + "" + item1.DataStepses
                        });
                    }
                }
                return Json(new Object[] { result, oreturnEditClass, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(AppraisalKRAModel c, int data, FormCollection form)
        {

            List<string> Msg = new List<string>();
            string AppraisalKRAModelObjectList = form["AppraisalKRAModelObjectList"] == "0" ? null : form["AppraisalKRAModelObjectList"];

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        

                        if (AppraisalKRAModelObjectList != null && AppraisalKRAModelObjectList != "")
                        {
                            var ids = Utility.StringIdsToListIds(AppraisalKRAModelObjectList);
                            List<int> AuthorList = new List<int>();
                            foreach (var ca in ids)
                            {
                                var KRAModelList = db.AppraisalKRAModelObject.Include(e => e.AppraisalKRAModel).Where(e => e.Id == ca).FirstOrDefault();
                                if (KRAModelList != null)
                                {
                                    int AppraisalKRAModelId = KRAModelList.AppraisalKRAModel != null ? KRAModelList.AppraisalKRAModel.Id : 0;
                                    int AppCategoryFullDetailsId = db.AppCategory.Where(e => e.Id == AppraisalKRAModelId).SingleOrDefault().Id;
                                    AuthorList.Add(AppCategoryFullDetailsId);

                                }

                            }

                            var duplicateExists = AuthorList.GroupBy(x => x)
                                                 .Where(g => g.Count() > 1)
                                                  .Select(y => y.Key)
                                                   .ToList();
                            if (duplicateExists.Count() > 0)
                            {
                                Msg.Add("duplicate kra object not allow");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        
                        var db_data = db.AppraisalKRAModel.Include(e => e.AppraisalKRAModelObject).Where(e => e.Id == data).SingleOrDefault();

                        List<AppraisalKRAModelObject> CMS_AppraisalKRAModelObjectList = new List<AppraisalKRAModelObject>();
                        if (AppraisalKRAModelObjectList != "" && AppraisalKRAModelObjectList != null)
                        {
                            var ids = Utility.StringIdsToListIds(AppraisalKRAModelObjectList);
                            foreach (var ca in ids)
                            {

                                var KRAModelObject_val = db.AppraisalKRAModelObject.Find(ca);
                                CMS_AppraisalKRAModelObjectList.Add(KRAModelObject_val);
                               
                            }
                            db_data.AppraisalKRAModelObject = CMS_AppraisalKRAModelObjectList;
                        }
                        else
                        {
                            Msg.Add("please select KRA object");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        db.AppraisalKRAModel.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;
               
                        AppraisalKRAModel CMS_AppraisalKRAModel = db.AppraisalKRAModel.Find(data);
                        TempData["CurrRowVersion"] = CMS_AppraisalKRAModel.RowVersion;
                   
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = CMS_AppraisalKRAModel.DBTrack.CreatedBy == null ? null : CMS_AppraisalKRAModel.DBTrack.CreatedBy,
                                CreatedOn = CMS_AppraisalKRAModel.DBTrack.CreatedOn == null ? null : CMS_AppraisalKRAModel.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            CMS_AppraisalKRAModel.Id = data;
                            CMS_AppraisalKRAModel.Code = c.Code;
                            CMS_AppraisalKRAModel.ModelName = c.ModelName;
                            CMS_AppraisalKRAModel.ModelDescription = c.ModelDescription;
                            CMS_AppraisalKRAModel.CreatedDate = c.CreatedDate;
                            CMS_AppraisalKRAModel.DBTrack = c.DBTrack;
                            CMS_AppraisalKRAModel.AppraisalKRAModelObject = db_data.AppraisalKRAModelObject;
                            db.AppraisalKRAModel.Attach(CMS_AppraisalKRAModel);
                            db.Entry(CMS_AppraisalKRAModel).State = System.Data.Entity.EntityState.Modified;
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
                    AppraisalKRAModel appraisalkramodels = db.AppraisalKRAModel
                                                       .Include(e => e.AppraisalKRAModelObject)
                                                      .Where(e => e.Id == data).SingleOrDefault();

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {



                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = appraisalkramodels.DBTrack.CreatedBy != null ? appraisalkramodels.DBTrack.CreatedBy : null,
                            CreatedOn = appraisalkramodels.DBTrack.CreatedOn != null ? appraisalkramodels.DBTrack.CreatedOn : null,
                            IsModified = appraisalkramodels.DBTrack.IsModified == true ? false : false//,
                        };
                        db.Entry(appraisalkramodels).State = System.Data.Entity.EntityState.Deleted;
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

        public ActionResult AppraisalKRAModel_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.AppraisalKRAModel.Include(e => e.AppraisalKRAModelObject)
                       .Include(e => e.AppraisalKRAModelObject.Select(r => r.AppraisalKRAModel))
                       .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel))
                       .ToList();

                    IEnumerable<AppraisalKRAModel> fall;
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

                    Func<AppraisalKRAModel, string> orderfunc = (c =>
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
        public ActionResult A_AppraisalKRAModel_Grid(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.AppraisalKRAModel
                       .Include(e => e.AppraisalKRAModelObject)
                       .Include(e => e.AppraisalKRAModelObject.Select(r => r.AppraisalKRAModel))
                       .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel))
                       .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                       .Where(e => e.Id == data).SingleOrDefault();

                    if (db_data.AppraisalKRAModelObject!= null)
                    {

                        List<returnDataClass> returndata = new List<returnDataClass>();
                        foreach (var item in db_data.AppraisalKRAModelObject)
                        {
                            returndata.Add(new returnDataClass
                            {
                                Id = item.Id,
                                KRAModel = item.AppraisalKRAModel != null ? db.AppCategory.Where(e => e.Id == item.AppraisalKRAModel.Id).Select(r => r.FullDetails).FirstOrDefault() : "",
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
                                AppraisalKRAModel = item1.KRAModel,
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