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
    public class AppraisalAttributeModelController : Controller
    {
        //
        // GET: /AttributeModelObject/
        public ActionResult Index()
        {
            return View("~/Views/CMS/MainViews/AppraisalAttributeModel/Index.cshtml");
        }
        public ActionResult AttributeModelObjectPartial()
        {
            return View("~/Views/Shared/CMS/AttributeModelObject.cshtml");
        }
        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var AppModeLookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1069").SingleOrDefault();
                int AppModeId = AppModeLookup.LookupValues.Where(e => e.LookupVal.ToUpper() == "ATTRIBUTE").SingleOrDefault().Id;
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
        public ActionResult GetLookupAttributeModelObject(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var dbdata = db.AppraisalAttributeModelObject.Include(e => e.AppraisalAttributeModel)
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
                            AttributeModel = item.AppraisalAttributeModel != null ? db.AppCategory.Where(e => e.Id == item.AppraisalAttributeModel.Id).Select(r => r.FullDetails).FirstOrDefault() : " ",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                        });

                    }
                    var res = (from ca in returndata
                               select new
                               {
                                   srno = ca.Id,
                                   lookupvalue = ca.AttributeModel + " " + ca.Creteriases + " " + ca.CreteriaTypeses + " " + ca.DataStepses
                               }).ToList();

                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                return null;

            }
        }

        [HttpPost]
        public ActionResult Create(AppraisalAttributeModel c, FormCollection form)
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    CompanyCMS_SPS companycms_sps = new CompanyCMS_SPS();
                    companycms_sps = db.CompanyCMS_SPS.Include(e => e.AppraisalAttributeModel).Where(e => e.Company.Id == company_Id).FirstOrDefault();

                    string AppraisalAttributeModelObjectList = form["AppraisalAttributeModelObjectList"] == "0" ? null : form["AppraisalAttributeModelObjectList"];

                    if (AppraisalAttributeModelObjectList != null)
                    {
                        var ids = Utility.StringIdsToListIds(AppraisalAttributeModelObjectList);
                        var AppraisalAttributeModelObjectlist = new List<AppraisalAttributeModelObject>();
                        foreach (var item in ids)
                        {
                            var returndata = db.AppraisalAttributeModelObject.Include(e => e.AppraisalAttributeModel)
                                .Include(e => e.CompetencyEvaluationModel).Include(e => e.CompetencyEvaluationModel.Criteria)
                                .Include(e => e.CompetencyEvaluationModel.CriteriaType).Include(e => e.CompetencyEvaluationModel.DataSteps)
                                .Where(e => e.Id == item).SingleOrDefault();

                            if (returndata != null && returndata.AppraisalAttributeModel != null && returndata.CompetencyEvaluationModel != null)
                            {
                                var appraisalattributemodel = returndata.AppraisalAttributeModel;
                                var evaluationmodel = returndata.CompetencyEvaluationModel;
                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
                                AppraisalAttributeModelObject objattributemodel = new AppraisalAttributeModelObject()
                                {
                                    AppraisalAttributeModel = appraisalattributemodel,
                                    CompetencyEvaluationModel = evaluationmodel,
                                    DBTrack = c.DBTrack
                                };
                                AppraisalAttributeModelObjectlist.Add(objattributemodel);
                            }
                        }
                        c.AppraisalAttributeModelObject = AppraisalAttributeModelObjectlist;
                    }
                    else
                    {
                        Msg.Add("please select attribute object");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (AppraisalAttributeModelObjectList != null && AppraisalAttributeModelObjectList != "")
                    {
                        var ids = Utility.StringIdsToListIds(AppraisalAttributeModelObjectList);
                        List<int> AuthorList = new List<int>();
                        foreach (var ca in ids)
                        {
                            var AttributeModelList = db.AppraisalAttributeModelObject.Include(e => e.AppraisalAttributeModel).Where(e => e.Id == ca).FirstOrDefault();
                            if (AttributeModelList != null)
                            {
                                int AppraisalAttributeModelId = AttributeModelList.AppraisalAttributeModel != null ? AttributeModelList.AppraisalAttributeModel.Id : 0;
                                int AppCategoryFullDetailsId = db.AppCategory.Where(e => e.Id == AppraisalAttributeModelId).SingleOrDefault().Id;
                                AuthorList.Add(AppCategoryFullDetailsId);

                            }

                        }

                        var duplicateExists = AuthorList.GroupBy(x => x)
                                             .Where(g => g.Count() > 1)
                                              .Select(y => y.Key)
                                               .ToList();
                        if (duplicateExists.Count()>0)
                        {
                            Msg.Add("duplicate attribute object not allow");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }


                    var CodeValue = db.AppraisalAttributeModel.Where(e => e.Code == c.Code).FirstOrDefault();
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

                            AppraisalAttributeModel AppraisalAttributeModel = new AppraisalAttributeModel()
                            {

                                Code = c.Code,
                                ModelName = c.ModelName,
                                ModelDescription = c.ModelDescription,
                                CreatedDate = DateTime.Now.Date,
                                AppraisalAttributeModelObject = c.AppraisalAttributeModelObject,
                                DBTrack = c.DBTrack
                            };

                            db.AppraisalAttributeModel.Add(AppraisalAttributeModel);
                            db.SaveChanges();

                            if (companycms_sps != null)
                            {
                                List<AppraisalAttributeModel> appraisalattributemodel_list = new List<AppraisalAttributeModel>();
                                appraisalattributemodel_list.Add(AppraisalAttributeModel);
                                if (companycms_sps.AppraisalAttributeModel != null)
                                {
                                    appraisalattributemodel_list.AddRange(companycms_sps.AppraisalAttributeModel);
                                }
                                companycms_sps.AppraisalAttributeModel = appraisalattributemodel_list;
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
        public class returnEditClass
        {
            public string AppraisalAttributeModelObject_Id { get; set; }
            public string AppraisalAttributeModelObject_FullDetails { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                var returndata = db.AppraisalAttributeModel.Include(e => e.AppraisalAttributeModelObject)
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
                var return_data = db.AppraisalAttributeModel.Include(e => e.AppraisalAttributeModelObject)
                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.AppraisalAttributeModel))
                      .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel))
                        .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                          .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                    .Where(e => e.Id == data && e.AppraisalAttributeModelObject.Count > 0).SingleOrDefault();

                List<returnDataClass> returndatases = new List<returnDataClass>();
                if (return_data != null && return_data.AppraisalAttributeModelObject != null)
                {
                    foreach (var item in return_data.AppraisalAttributeModelObject)
                    {
                        returndatases.Add(new returnDataClass
                        {
                            Id = item.Id,
                            AttributeModel = item.AppraisalAttributeModel != null ? db.AppCategory.Where(e => e.Id == item.AppraisalAttributeModel.Id).Select(r => r.FullDetails).FirstOrDefault() : "",
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
                            AppraisalAttributeModelObject_Id = item1.Id.ToString(),
                            AppraisalAttributeModelObject_FullDetails = item1.AttributeModel + " " + item1.Creteriases + " " + item1.CreteriaTypeses + "" + item1.DataStepses
                        });
                    }
                }

                return Json(new Object[] { result, oreturnEditClass, JsonRequestBehavior.AllowGet });
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
            public string AppraisalAttributeModel { get; set; }
            public string EvaluationModel { get; set; }

        }

        public class returnDataClass
        {
            public int Id { get; set; }
            public string AttributeModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }

        public ActionResult AppraisalAttributeModel_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.AppraisalAttributeModel.Include(e => e.AppraisalAttributeModelObject)
                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.AppraisalAttributeModel))
                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel))
                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                    .ToList();

                    IEnumerable<AppraisalAttributeModel> fall;

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

                    Func<AppraisalAttributeModel, string> orderfunc = (c =>
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
                    }//for data reterivation
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
        public ActionResult A_AppraisalAttributeModel_Grid(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.AppraisalAttributeModel
                        .Include(e => e.AppraisalAttributeModelObject)
                        .Include(e => e.AppraisalAttributeModelObject.Select(r => r.AppraisalAttributeModel))
                        .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel))
                        .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                        .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                        .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                        .Where(e => e.Id == data).SingleOrDefault();

                    if (db_data.AppraisalAttributeModelObject != null)
                    {

                        List<returnDataClass> returndata = new List<returnDataClass>();
                        foreach (var item in db_data.AppraisalAttributeModelObject)
                        {
                            returndata.Add(new returnDataClass
                            {
                                Id = item.Id,
                                AttributeModel = item.AppraisalAttributeModel != null ? db.AppCategory.Where(e => e.Id == item.AppraisalAttributeModel.Id).Select(r => r.FullDetails).FirstOrDefault() : "",
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
                                AppraisalAttributeModel = item1.AttributeModel,
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
        [HttpPost]
        public async Task<ActionResult> EditSave(AppraisalAttributeModel c, int data, FormCollection form)
        {

            List<string> Msg = new List<string>();
            string AppraisalAttributeModelObjectList = form["AppraisalAttributeModelObjectList"] == "0" ? null : form["AppraisalAttributeModelObjectList"];
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {


                        if (AppraisalAttributeModelObjectList != null && AppraisalAttributeModelObjectList != "")
                        {
                            var ids = Utility.StringIdsToListIds(AppraisalAttributeModelObjectList);
                            List<int> AuthorList = new List<int>();
                            foreach (var ca in ids)
                            {
                                var AttributeModelList = db.AppraisalAttributeModelObject.Include(e => e.AppraisalAttributeModel).Where(e => e.Id == ca).FirstOrDefault();
                                if (AttributeModelList != null)
                                {
                                    int AppraisalAttributeModelId = AttributeModelList.AppraisalAttributeModel != null ? AttributeModelList.AppraisalAttributeModel.Id : 0;
                                    int AppCategoryFullDetailsId = db.AppCategory.Where(e => e.Id == AppraisalAttributeModelId).SingleOrDefault().Id;
                                    AuthorList.Add(AppCategoryFullDetailsId);

                                }

                            }

                            var duplicateExists = AuthorList.GroupBy(x => x)
                                                 .Where(g => g.Count() > 1)
                                                  .Select(y => y.Key)
                                                   .ToList();
                            if (duplicateExists.Count() > 0)
                            {
                                Msg.Add("duplicate attribute Object not allow");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                       
                        var db_data = db.AppraisalAttributeModel.Include(e => e.AppraisalAttributeModelObject).Where(e => e.Id == data).SingleOrDefault();

                        List<AppraisalAttributeModelObject> CMS_AppraisalAttributeModelObjectList = new List<AppraisalAttributeModelObject>();
                        if (AppraisalAttributeModelObjectList != "" && AppraisalAttributeModelObjectList != null)
                        {
                            var ids = Utility.StringIdsToListIds(AppraisalAttributeModelObjectList);
                            foreach (var ca in ids)
                            {

                                var AttributeModelObject_val = db.AppraisalAttributeModelObject.Find(ca);
                                CMS_AppraisalAttributeModelObjectList.Add(AttributeModelObject_val);
                                
                            }
                            db_data.AppraisalAttributeModelObject = CMS_AppraisalAttributeModelObjectList;
                        }
                        else
                        {
                            Msg.Add("please select attribute object");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        db.AppraisalAttributeModel.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;
                        AppraisalAttributeModel CMS_AppraisalAttributeModel = db.AppraisalAttributeModel.Find(data);
                        TempData["CurrRowVersion"] = CMS_AppraisalAttributeModel.RowVersion;

                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = CMS_AppraisalAttributeModel.DBTrack.CreatedBy == null ? null : CMS_AppraisalAttributeModel.DBTrack.CreatedBy,
                                CreatedOn = CMS_AppraisalAttributeModel.DBTrack.CreatedOn == null ? null : CMS_AppraisalAttributeModel.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            CMS_AppraisalAttributeModel.Id = data;
                            CMS_AppraisalAttributeModel.Code = c.Code;
                            CMS_AppraisalAttributeModel.ModelName = c.ModelName;
                            CMS_AppraisalAttributeModel.ModelDescription = c.ModelDescription;
                            CMS_AppraisalAttributeModel.CreatedDate = c.CreatedDate;
                            CMS_AppraisalAttributeModel.DBTrack = c.DBTrack;
                            CMS_AppraisalAttributeModel.AppraisalAttributeModelObject = db_data.AppraisalAttributeModelObject;
                            db.AppraisalAttributeModel.Attach(CMS_AppraisalAttributeModel);
                            db.Entry(CMS_AppraisalAttributeModel).State = System.Data.Entity.EntityState.Modified;
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
                    AppraisalAttributeModel appraisalattributemodels = db.AppraisalAttributeModel
                                                      .Include(e => e.AppraisalAttributeModelObject)
                                                     .Where(e => e.Id == data).SingleOrDefault();
                    
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {



                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = appraisalattributemodels.DBTrack.CreatedBy != null ? appraisalattributemodels.DBTrack.CreatedBy : null,
                            CreatedOn = appraisalattributemodels.DBTrack.CreatedOn != null ? appraisalattributemodels.DBTrack.CreatedOn : null,
                            IsModified = appraisalattributemodels.DBTrack.IsModified == true ? false : false//,

                        };
                        db.Entry(appraisalattributemodels).State = System.Data.Entity.EntityState.Deleted;
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

    }
}