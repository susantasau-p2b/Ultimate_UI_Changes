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
    public class PastExperienceModelController : Controller
    {
        // GET: PastExperienceModel
        public ActionResult Index()
        {
            return View("~/Views/CMS/MainViews/PastExperienceModel/index.cshtml");
        }
        public ActionResult PastExperienceModelPartial()
        {
            return View("~/Views/Shared/CMS/PastExperienceModelObject.cshtml");
        }
        public class returnDataClass
        {
            public int Id { get; set; }
            public string PastExperienceModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public ActionResult GetLookupPastExperienceModelObject(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.PastExperienceModelObject.Include(e => e.PastExperienceModel)
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
                            PastExperienceModel = item.PastExperienceModel != null ? item.PastExperienceModel.LookupVal : "",
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                        });

                    }
                    var res = (from ca in returndata
                               select new
                               {
                                   srno = ca.Id,
                                   lookupvalue = ca.PastExperienceModel + " " + ca.Creteriases + " " + ca.CreteriaTypeses + " " + ca.DataStepses
                               }).ToList();

                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                return null;

            }
        }
        [HttpPost]
        public ActionResult Create(PastExperienceModel c, FormCollection form)
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    CompanyCMS_SPS companycms_sps = new CompanyCMS_SPS();
                    companycms_sps = db.CompanyCMS_SPS.Include(e => e.PastExperienceModel).Where(e => e.Company.Id == company_Id).FirstOrDefault();

                    string PastExperienceModelObjectList = form["PastExperienceModelObjectList"] == "0" ? null : form["PastExperienceModelObjectList"];

                    if (PastExperienceModelObjectList != null && PastExperienceModelObjectList != "")
                    {
                        var ids = Utility.StringIdsToListIds(PastExperienceModelObjectList);
                        var PastExperienceModelObjectlist = new List<PastExperienceModelObject>();
                        foreach (var item in ids)
                        {
                            var returndata = db.PastExperienceModelObject.Include(e => e.PastExperienceModel)
                                .Include(e => e.CompetencyEvaluationModel).Include(e => e.CompetencyEvaluationModel.Criteria)
                                .Include(e => e.CompetencyEvaluationModel.CriteriaType).Include(e => e.CompetencyEvaluationModel.DataSteps)
                                .Where(e => e.Id == item).SingleOrDefault();

                            if (returndata != null && returndata.PastExperienceModel != null && returndata.CompetencyEvaluationModel != null)
                            {
                                var pastexperiencemodel = returndata.PastExperienceModel;
                                var evaluationmodel = returndata.CompetencyEvaluationModel;
                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
                                PastExperienceModelObject objpastexperiencemodel = new PastExperienceModelObject()
                                {
                                    PastExperienceModel = pastexperiencemodel,
                                    CompetencyEvaluationModel = evaluationmodel,
                                    DBTrack = c.DBTrack
                                };
                                PastExperienceModelObjectlist.Add(objpastexperiencemodel);
                            }
                        }
                        c.PastExperienceModelObject = PastExperienceModelObjectlist;
                    }
                    else
                    {
                        Msg.Add("please select past experience object");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (PastExperienceModelObjectList != null && PastExperienceModelObjectList != "")
                    {
                        var ids = Utility.StringIdsToListIds(PastExperienceModelObjectList);
                        List<int> AuthorList = new List<int>();
                        foreach (var ca in ids)
                        {
                            var pastexperienceModelList = db.PastExperienceModelObject.Include(e => e.PastExperienceModel).Where(e => e.Id == ca).FirstOrDefault();
                            if (pastexperienceModelList != null)
                            {
                                int PastexperienceModelId = pastexperienceModelList.PastExperienceModel != null ? pastexperienceModelList.PastExperienceModel.Id : 0;
                                int AppCategoryFullDetailsId = db.LookupValue.Where(e => e.Id == PastexperienceModelId).SingleOrDefault().Id;
                                AuthorList.Add(AppCategoryFullDetailsId);

                            }

                        }

                        var duplicateExists = AuthorList.GroupBy(x => x)
                                             .Where(g => g.Count() > 1)
                                              .Select(y => y.Key)
                                               .ToList();
                        if (duplicateExists.Count() > 0)
                        {
                            Msg.Add("duplicate past experience Object not allow");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }


                    var CodeValue = db.PastExperienceModel.Where(e => e.Code == c.Code).FirstOrDefault();
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

                            PastExperienceModel ObjpastexperienceModel = new PastExperienceModel()
                            {

                                Code = c.Code,
                                ModelName = c.ModelName,
                                ModelDescription = c.ModelDescription,
                                CreatedDate = DateTime.Now.Date,
                                PastExperienceModelObject = c.PastExperienceModelObject,
                                DBTrack = c.DBTrack
                            };

                            db.PastExperienceModel.Add(ObjpastexperienceModel);
                            db.SaveChanges();

                            if (companycms_sps != null)
                            {
                                List<PastExperienceModel> pastexperiencemodel_list = new List<PastExperienceModel>();
                                pastexperiencemodel_list.Add(ObjpastexperienceModel);
                                if (companycms_sps.PastExperienceModel != null)
                                {
                                    pastexperiencemodel_list.AddRange(companycms_sps.PastExperienceModel);
                                }
                                companycms_sps.PastExperienceModel = pastexperiencemodel_list;
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
            public string PastExperienceModelObject_Id { get; set; }
            public string PastExperienceModelObject_FullDetails { get; set; }
        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                var returndata = db.PastExperienceModel.Include(e => e.PastExperienceModelObject)
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
                var return_data = db.PastExperienceModel.Include(e => e.PastExperienceModelObject)
                    .Include(e => e.PastExperienceModelObject.Select(r => r.PastExperienceModel))
                      .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel))
                        .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                          .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                    .Where(e => e.Id == data && e.PastExperienceModelObject.Count > 0).SingleOrDefault();

                List<returnDataClass> returndatases = new List<returnDataClass>();
                if (return_data != null && return_data.PastExperienceModelObject != null)
                {
                    foreach (var item in return_data.PastExperienceModelObject)
                    {
                        returndatases.Add(new returnDataClass
                        {
                            Id = item.Id,
                            PastExperienceModel = item.PastExperienceModel != null ? item.PastExperienceModel.LookupVal : "",
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
                            PastExperienceModelObject_Id = item1.Id.ToString(),
                            PastExperienceModelObject_FullDetails = item1.PastExperienceModel + " " + item1.Creteriases + " " + item1.CreteriaTypeses + "" + item1.DataStepses
                        });
                    }
                }

                return Json(new Object[] { result, oreturnEditClass, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(PastExperienceModel c, int data, FormCollection form)
        {

            List<string> Msg = new List<string>();
            string PastExperienceModelObjectList = form["PastExperienceModelObjectList"] == "0" ? null : form["PastExperienceModelObjectList"];
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (PastExperienceModelObjectList != null && PastExperienceModelObjectList != "")
                    {
                        var ids = Utility.StringIdsToListIds(PastExperienceModelObjectList);
                        List<int> AuthorList = new List<int>();
                        foreach (var ca in ids)
                        {
                            var pastexperienceModelList = db.PastExperienceModelObject.Include(e => e.PastExperienceModel).Where(e => e.Id == ca).FirstOrDefault();
                            if (pastexperienceModelList != null)
                            {
                                int PastexperienceModelId = pastexperienceModelList.PastExperienceModel != null ? pastexperienceModelList.PastExperienceModel.Id : 0;
                                int AppCategoryFullDetailsId = db.LookupValue.Where(e => e.Id == PastexperienceModelId).SingleOrDefault().Id;
                                AuthorList.Add(AppCategoryFullDetailsId);

                            }

                        }

                        var duplicateExists = AuthorList.GroupBy(x => x)
                                             .Where(g => g.Count() > 1)
                                              .Select(y => y.Key)
                                               .ToList();
                        if (duplicateExists.Count() > 0)
                        {
                            Msg.Add("duplicate past experience Object not allow");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }


                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.PastExperienceModel.Include(e => e.PastExperienceModelObject).Where(e => e.Id == data).SingleOrDefault();
                        List<PastExperienceModelObject> CMS_PastExperienceModelObject = new List<PastExperienceModelObject>();
                        if (PastExperienceModelObjectList != "" && PastExperienceModelObjectList != null)
                        {
                            var ids = Utility.StringIdsToListIds(PastExperienceModelObjectList);
                            foreach (var ca in ids)
                            {
                                var Values_val = db.PastExperienceModelObject.Find(ca);
                                CMS_PastExperienceModelObject.Add(Values_val);
                            }
                            db_data.PastExperienceModelObject = CMS_PastExperienceModelObject;
                        }
                        else
                        {
                            db_data.PastExperienceModelObject = null;
                        }

                        db.PastExperienceModel.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;
                        PastExperienceModel CMS_PastExperienceModel = db.PastExperienceModel.Find(data);
                        TempData["CurrRowVersion"] = CMS_PastExperienceModel.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = CMS_PastExperienceModel.DBTrack.CreatedBy == null ? null : CMS_PastExperienceModel.DBTrack.CreatedBy,
                                CreatedOn = CMS_PastExperienceModel.DBTrack.CreatedOn == null ? null : CMS_PastExperienceModel.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            CMS_PastExperienceModel.Id = data;
                            CMS_PastExperienceModel.Code = c.Code;
                            CMS_PastExperienceModel.ModelName = c.ModelName;
                            CMS_PastExperienceModel.ModelDescription = c.ModelDescription;
                            CMS_PastExperienceModel.CreatedDate = c.CreatedDate;
                            CMS_PastExperienceModel.DBTrack = c.DBTrack;
                            CMS_PastExperienceModel.PastExperienceModelObject = db_data.PastExperienceModelObject;
                            db.PastExperienceModel.Attach(CMS_PastExperienceModel);
                            db.Entry(CMS_PastExperienceModel).State = System.Data.Entity.EntityState.Modified;
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
            public string PastExperienceModel { get; set; }
            public string EvaluationModel { get; set; }

        }
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    PastExperienceModel pastexperiencemodels = db.PastExperienceModel
                                                      .Include(e => e.PastExperienceModelObject)
                                                     .Where(e => e.Id == data).SingleOrDefault();

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {



                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = pastexperiencemodels.DBTrack.CreatedBy != null ? pastexperiencemodels.DBTrack.CreatedBy : null,
                            CreatedOn = pastexperiencemodels.DBTrack.CreatedOn != null ? pastexperiencemodels.DBTrack.CreatedOn : null,
                            IsModified = pastexperiencemodels.DBTrack.IsModified == true ? false : false//,

                        };
                        db.Entry(pastexperiencemodels).State = System.Data.Entity.EntityState.Deleted;
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
        public ActionResult PastExperienceModel_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.PastExperienceModel.Include(e => e.PastExperienceModelObject)
                       .Include(e => e.PastExperienceModelObject.Select(r => r.PastExperienceModel))
                       .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel))
                       .ToList();

                    IEnumerable<PastExperienceModel> fall;
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

                    Func<PastExperienceModel, string> orderfunc = (c =>
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
        public ActionResult A_PastExperienceModel_Grid(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.PastExperienceModel
                        .Include(e => e.PastExperienceModelObject)
                        .Include(e => e.PastExperienceModelObject.Select(r => r.PastExperienceModel))
                        .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel))
                        .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                        .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                        .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                        .Where(e => e.Id == data).SingleOrDefault();

                    if (db_data.PastExperienceModelObject != null)
                    {

                        List<returnDataClass> returndata = new List<returnDataClass>();
                        foreach (var item in db_data.PastExperienceModelObject)
                        {
                            returndata.Add(new returnDataClass
                            {
                                Id = item.Id,
                                PastExperienceModel = item.PastExperienceModel != null ? item.PastExperienceModel.LookupVal : "",
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
                                PastExperienceModel = item1.PastExperienceModel,
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
