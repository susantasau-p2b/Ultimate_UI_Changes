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

namespace P2BUltimate.Controllers.CMS
{
    public class CompetencyObjectMappingController : Controller
    {
        // GET: CompetencyObjectMapping
        public ActionResult Index()
        {
            return View("~/Views/CMS/MainViews/CompetencyObjectMapping/index.cshtml");
        }
        public ActionResult GetAppCategory(List<int> SkipIds, string Apprlist)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int item = Convert.ToInt32(Apprlist);
                var fall = db.AppCategory.Where(e => e.AppMode.Id == item).ToList();
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult GetModelList(string data, string data2, string Apprlist)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                switch (Apprlist)
                {
                    case "Attribute":
                        var qurey1 = db.AppraisalAttributeModel.ToList();
                        var selected1 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected1 = Convert.ToInt32(data2);
                        }

                        SelectList s1 = new SelectList(qurey1, "Id", "Code", selected1);

                        return Json(s1, JsonRequestBehavior.AllowGet);
                        break;

                    case "Potential":
                        var qurey2 = db.AppraisalPotentialModel.ToList();
                        var selected2 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }

                        SelectList s2 = new SelectList(qurey2, "Id", "Code", selected2);

                        return Json(s2, JsonRequestBehavior.AllowGet);
                        break;

                    case "Kra":
                        var qurey3 = db.AppraisalKRAModel.ToList();
                        var selected3 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected3 = Convert.ToInt32(data2);
                        }

                        SelectList s3 = new SelectList(qurey3, "Id", "Code", selected3);

                        return Json(s3, JsonRequestBehavior.AllowGet);
                        break;

                    case "BusinessAppraisal":
                        var qurey4 = db.AppraisalBusinessAppraisalModel.ToList();
                        var selected4 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected3 = Convert.ToInt32(data2);
                        }

                        SelectList s4 = new SelectList(qurey4, "Id", "Code", selected4);

                        return Json(s4, JsonRequestBehavior.AllowGet);
                        break;

                    case "Skill":
                        var qurey5 = db.SkillModel.ToList();
                        var selected5 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected3 = Convert.ToInt32(data2);
                        }

                        SelectList s5 = new SelectList(qurey5, "Id", "Code", selected5);

                        return Json(s5, JsonRequestBehavior.AllowGet);
                        break;

                    case "Service":
                        var qurey6 = db.ServiceModel.ToList();
                        var selected6 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected3 = Convert.ToInt32(data2);
                        }

                        SelectList s6 = new SelectList(qurey6, "Id", "Code", selected6);

                        return Json(s6, JsonRequestBehavior.AllowGet);
                        break;

                    case "PastExperience":
                        var qurey7 = db.PastExperienceModel.ToList();
                        var selected7 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected3 = Convert.ToInt32(data2);
                        }

                        SelectList s7 = new SelectList(qurey7, "Id", "Code", selected7);

                        return Json(s7, JsonRequestBehavior.AllowGet);
                        break;

                    case "Qualification":
                        var qurey8 = db.QualificationModel.ToList();
                        var selected8 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected3 = Convert.ToInt32(data2);
                        }

                        SelectList s8 = new SelectList(qurey8, "Id", "Code", selected8);

                        return Json(s8, JsonRequestBehavior.AllowGet);
                        break;

                    case "Training":
                        var qurey9 = db.TrainingModel.ToList();
                        var selected9 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected3 = Convert.ToInt32(data2);
                        }

                        SelectList s9 = new SelectList(qurey9, "Id", "Code", selected9);

                        return Json(s9, JsonRequestBehavior.AllowGet);
                        break;

                    case "Personnel":
                        var qurey10 = db.PersonnelModel.ToList();
                        var selected10 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected3 = Convert.ToInt32(data2);
                        }

                        SelectList s10 = new SelectList(qurey10, "Id", "Code", selected10);

                        return Json(s10, JsonRequestBehavior.AllowGet);
                        break;








                }
                return null;


            }

        }
        public ActionResult GetModelObjList(string data, string data2, string Apprlist, string Objmlist)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                switch (Apprlist)
                {
                    case "Attribute":
                        var qurey11 = db.AppraisalAttributeModel.Include(e => e.AppraisalAttributeModelObject)
                           .Include(e => e.AppraisalAttributeModelObject.Select(r => r.AppraisalAttributeModel))
                           .Where(e => e.Code == Objmlist).FirstOrDefault();
                        var qurey12 = qurey11.AppraisalAttributeModelObject.ToList();
                        var query13 = qurey12.Select(r => r.AppraisalAttributeModel).ToList();
                        var selected1 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected1 = Convert.ToInt32(data2);
                        }

                        SelectList s1 = new SelectList(query13, "Id", "LookupVal", selected1);
                        return Json(s1, JsonRequestBehavior.AllowGet);
                        break;

                    case "Potential":
                        var qurey21 = db.AppraisalPotentialModel.Include(e => e.AppraisalPotentialModelObject)
                        .Include(e => e.AppraisalPotentialModelObject.Select(r => r.AppraisalPotentialModel))
                        .Where(e => e.Code == Objmlist).FirstOrDefault();
                        var qurey22 = qurey21.AppraisalPotentialModelObject.ToList();
                        var query23 = qurey22.Select(r => r.AppraisalPotentialModel).ToList();
                        var selected2 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }

                        SelectList s2 = new SelectList(query23, "Id", "LookupVal", selected2);
                        return Json(s2, JsonRequestBehavior.AllowGet);
                        break;

                    case "Kra":
                        var qurey31 = db.AppraisalKRAModel.Include(e => e.AppraisalKRAModelObject)
                       .Include(e => e.AppraisalKRAModelObject.Select(r => r.AppraisalKRAModel))
                       .Where(e => e.Code == Objmlist).FirstOrDefault();
                        var qurey32 = qurey31.AppraisalKRAModelObject.ToList();
                        var query33 = qurey32.Select(r => r.AppraisalKRAModel).ToList();
                        var selected3 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }
                        SelectList s3 = new SelectList(query33, "Id", "LookupVal", selected3);
                        return Json(s3, JsonRequestBehavior.AllowGet);
                        break;

                    case "AppraisalBusinessAppraisal":
                        var qurey41 = db.AppraisalBusinessAppraisalModel.Include(e => e.AppraisalBusinessAppraisalModelObject)
                       .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.AppraisalBusinessAppraisalModel))
                       .Where(e => e.Code == Objmlist).FirstOrDefault();
                        var qurey42 = qurey41.AppraisalBusinessAppraisalModelObject.ToList();
                        var query43 = qurey42.Select(r => r.AppraisalBusinessAppraisalModel).ToList();
                        var selected4 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }
                        SelectList s4 = new SelectList(query43, "Id", "LookupVal", selected4);
                        return Json(s4, JsonRequestBehavior.AllowGet);
                        break;

                    case "Skill":
                        var qurey51 = db.SkillModel.Include(e => e.SkillModelObject)
                       .Include(e => e.SkillModelObject.Select(r => r.SkillModel))
                       .Where(e => e.Code == Objmlist).FirstOrDefault();
                        var qurey52 = qurey51.SkillModelObject.ToList();
                        var query53 = qurey52.Select(r => r.SkillModel).ToList();
                        var selected5 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }
                        SelectList s5 = new SelectList(query53, "Id", "LookupVal", selected5);
                        return Json(s5, JsonRequestBehavior.AllowGet);
                        break;

                    case "Service":
                        var qurey61 = db.ServiceModel.Include(e => e.ServiceModelObject)
                       .Include(e => e.ServiceModelObject.Select(r => r.ServiceModel))
                       .Where(e => e.Code == Objmlist).FirstOrDefault();
                        var qurey62 = qurey61.ServiceModelObject.ToList();
                        var query63 = qurey62.Select(r => r.ServiceModel).ToList();
                        var selected6 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }
                        SelectList s6 = new SelectList(query63, "Id", "LookupVal", selected6);
                        return Json(s6, JsonRequestBehavior.AllowGet);
                        break;

                    case "PastExperience":
                        var qurey71 = db.PastExperienceModel.Include(e => e.PastExperienceModelObject)
                       .Include(e => e.PastExperienceModelObject.Select(r => r.PastExperienceModel))
                       .Where(e => e.Code == Objmlist).FirstOrDefault();
                        var qurey72 = qurey71.PastExperienceModelObject.ToList();
                        var query73 = qurey72.Select(r => r.PastExperienceModel).ToList();
                        var selected7 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }
                        SelectList s7 = new SelectList(query73, "Id", "LookupVal", selected7);
                        return Json(s7, JsonRequestBehavior.AllowGet);
                        break;
                    case "Qualification":
                        var qurey81 = db.QualificationModel.Include(e => e.QualificationModelObject)
                       .Include(e => e.QualificationModelObject.Select(r => r.QualificationModel))
                       .Where(e => e.Code == Objmlist).FirstOrDefault();
                        var qurey82 = qurey81.QualificationModelObject.ToList();
                        var query83 = qurey82.Select(r => r.QualificationModel).ToList();
                        var selected8 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }
                        SelectList s8 = new SelectList(query83, "Id", "LookupVal", selected8);
                        return Json(s8, JsonRequestBehavior.AllowGet);
                        break;

                    case "Training":
                        var qurey91 = db.TrainingModel.Include(e => e.TrainingModelObject)
                       .Include(e => e.TrainingModelObject.Select(r => r.TrainingModel))
                       .Where(e => e.Code == Objmlist).FirstOrDefault();
                        var qurey92 = qurey91.TrainingModelObject.ToList();
                        var query93 = qurey92.Select(r => r.TrainingModel).ToList();
                        var selected9 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }
                        SelectList s9 = new SelectList(query93, "Id", "LookupVal", selected9);
                        return Json(s9, JsonRequestBehavior.AllowGet);
                        break;

                    case "Personnel":
                        var qurey101 = db.PersonnelModel.Include(e => e.PersonnelModelObject)
                       .Include(e => e.PersonnelModelObject.Select(r => r.PersonnelModel))
                       .Where(e => e.Code == Objmlist).FirstOrDefault();
                        var qurey102 = qurey101.PersonnelModelObject.ToList();
                        var query103 = qurey102.Select(r => r.PersonnelModel).ToList();
                        var selected10 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }
                        SelectList s10 = new SelectList(query103, "Id", "LookupVal", selected10);
                        return Json(s10, JsonRequestBehavior.AllowGet);
                        break;









                }
                return null;


            }

        }
        public class returndatagridDataclass //Parentgrid
        {
            public string Id { get; set; }
            public string LookupVal { get; set; }
            public string LookupValData { get; set; }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(LookupValue c, FormCollection form)
        {

            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string AppCategoryList = form["AppCategoryList"] == "" ? null : form["AppCategoryList"];
                    string MAPPIDList = form["MAPPIDList"] == "" ? null : form["MAPPIDList"];
                    int LookupvalueId = Convert.ToInt32(MAPPIDList);

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.LookupValue.Where(e => e.Id == LookupvalueId).SingleOrDefault();
                        TempData["RowVersion"] = db_data.RowVersion;
                        LookupValue LookupValue = db.LookupValue.Find(LookupvalueId);
                        TempData["CurrRowVersion"] = LookupValue.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = LookupValue.DBTrack.CreatedBy == null ? null : LookupValue.DBTrack.CreatedBy,
                                CreatedOn = LookupValue.DBTrack.CreatedOn == null ? null : LookupValue.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            LookupValue.Id = LookupvalueId;
                            LookupValue.LookupValData = AppCategoryList;
                            LookupValue.DBTrack = c.DBTrack;
                            db.Entry(LookupValue).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
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
        public ActionResult CompetencyObjectMapping_Grid(ParamModel param, string y, string NewModellist)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {


                    switch (NewModellist)
                    {
                        case "--Select--":
                                
                                return Json(new
                                {                                   
                                    data=0
                                }, JsonRequestBehavior.AllowGet);
                            break;

                        case "AppraisalAttributeModel":
                            var query11 = db.AppraisalAttributeModelObject.Include(a => a.AppraisalAttributeModel).ToList();
                            var query12 = query11.Select(r => r.AppraisalAttributeModel).ToList();
                            IEnumerable<LookupValue> fall1;

                            if (param.sSearch == null)
                            {
                                fall1 = query12;
                            }
                            else
                            {

                                fall1 = query12.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                          || (e.LookupVal.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                          || (e.LookupValData.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                          ).ToList();
                            }

                            var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                            Func<LookupValue, string> orderfunc = (c =>
                                                                        Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                        sortindex == 1 ? c.LookupVal : "");
                            var sortcolumn = Request["sSortDir_0"];
                            if (sortcolumn == "asc")
                            {
                                fall1 = fall1.OrderBy(orderfunc);
                            }
                            else
                            {
                                fall1 = fall1.OrderByDescending(orderfunc);
                            }
                            var dcompanies = fall1
                                                        .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

                            if (dcompanies.Count == 0)
                            {
                                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                                foreach (var item in fall1)
                                {
                                    result.Add(new returndatagridDataclass
                                    {
                                        Id = item.Id.ToString(),
                                        LookupVal = item.LookupVal,
                                        LookupValData = db.AppCategory.Where(e => e.Id.ToString() == item.LookupValData).Select(r => r.FullDetails).FirstOrDefault().ToString(),
                                    });
                                }
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query12.Count(),
                                    iTotalDisplayRecords = fall1.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }

                            else
                            {
                                var result = from c in dcompanies

                                             select new[] { null, Convert.ToString(c.Id), c.LookupVal, };
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query12.Count(),
                                    iTotalDisplayRecords = fall1.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case "AppraisalBusinessAppraisalModel":

                            var query21 = db.AppraisalBusinessAppraisalModelObject.Include(a => a.AppraisalBusinessAppraisalModel).ToList();
                            var query22 = query21.Select(r => r.AppraisalBusinessAppraisalModel).ToList();
                            IEnumerable<LookupValue> fall2;
                            if (param.sSearch == null)
                            {
                                fall2 = query22;
                            }
                            else
                            {

                                fall2 = query22.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                          || (e.LookupVal.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                          || (e.LookupValData.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                          ).ToList();
                            }

                            var sortindex2 = Convert.ToInt32(Request["iSortCol_0"]);

                            Func<LookupValue, string> orderfunc2 = (c =>
                                                                        Convert.ToUInt32(sortindex2) == 0 ? c.Id.ToString() :
                                                                        sortindex2 == 1 ? c.LookupVal : "");
                            var sortcolumn2 = Request["sSortDir_0"];
                            if (sortcolumn2 == "asc")
                            {
                                fall2 = fall2.OrderBy(orderfunc2);
                            }
                            else
                            {
                                fall2 = fall2.OrderByDescending(orderfunc2);
                            }
                            var dcompanies2 = fall2
                                                        .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

                            if (dcompanies2.Count == 0)
                            {
                                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                                foreach (var item in fall2)
                                {
                                    result.Add(new returndatagridDataclass
                                    {
                                        Id = item.Id.ToString(),
                                        LookupVal = item.LookupVal,
                                        LookupValData = db.AppCategory.Where(e => e.Id.ToString() == item.LookupValData).Select(r => r.FullDetails).FirstOrDefault().ToString(),
                                    });
                                }
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query22.Count(),
                                    iTotalDisplayRecords = fall2.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }

                            else
                            {
                                var result = from c in dcompanies2

                                             select new[] { null, Convert.ToString(c.Id), c.LookupVal, };
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query22.Count(),
                                    iTotalDisplayRecords = fall2.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case "AppraisalKRAModel":

                            var query31 = db.AppraisalKRAModelObject.Include(a => a.AppraisalKRAModel).ToList();
                            var query32 = query31.Select(r => r.AppraisalKRAModel).ToList();
                            IEnumerable<LookupValue> fall3;
                            if (param.sSearch == null)
                            {
                                fall3 = query32;
                            }
                            else
                            {

                                fall3 = query32.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                          || (e.LookupVal.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                          || (e.LookupValData.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                          ).ToList();
                            }

                            var sortindex3 = Convert.ToInt32(Request["iSortCol_0"]);

                            Func<LookupValue, string> orderfunc3 = (c =>
                                                                        Convert.ToUInt32(sortindex3) == 0 ? c.Id.ToString() :
                                                                        sortindex3 == 1 ? c.LookupVal : "");
                            var sortcolumn3 = Request["sSortDir_0"];
                            if (sortcolumn3 == "asc")
                            {
                                fall3 = fall3.OrderBy(orderfunc3);
                            }
                            else
                            {
                                fall3 = fall3.OrderByDescending(orderfunc3);
                            }
                            var dcompanies3 = fall3
                                                        .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

                            if (dcompanies3.Count == 0)
                            {
                                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                                foreach (var item in fall3)
                                {
                                    result.Add(new returndatagridDataclass
                                    {
                                        Id = item.Id.ToString(),
                                        LookupVal = item.LookupVal,
                                        LookupValData = db.AppCategory.Where(e => e.Id.ToString() == item.LookupValData).Select(r => r.FullDetails).FirstOrDefault().ToString(),
                                    });
                                }
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query32.Count(),
                                    iTotalDisplayRecords = fall3.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }

                            else
                            {
                                var result = from c in dcompanies3

                                             select new[] { null, Convert.ToString(c.Id), c.LookupVal, };
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query32.Count(),
                                    iTotalDisplayRecords = fall3.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        case "AppraisalPotentialModel":

                             var query41 = db.AppraisalPotentialModelObject.Include(a => a.AppraisalPotentialModel).ToList();
                             var query42 = query41.Select(r => r.AppraisalPotentialModel).ToList();
                            IEnumerable<LookupValue> fall4;
                            if (param.sSearch == null)
                            {
                                fall4 = query42;
                            }
                            else
                            {

                                fall4 = query42.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                          || (e.LookupVal.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                          || (e.LookupValData.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                          ).ToList();
                            }

                            var sortindex4 = Convert.ToInt32(Request["iSortCol_0"]);

                            Func<LookupValue, string> orderfunc4 = (c =>
                                                                        Convert.ToUInt32(sortindex4) == 0 ? c.Id.ToString() :
                                                                        sortindex4 == 1 ? c.LookupVal : "");
                            var sortcolumn4 = Request["sSortDir_0"];
                            if (sortcolumn4 == "asc")
                            {
                                fall4 = fall4.OrderBy(orderfunc4);
                            }
                            else
                            {
                                fall4 = fall4.OrderByDescending(orderfunc4);
                            }
                            var dcompanies4 = fall4
                                                        .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

                            if (dcompanies4.Count == 0)
                            {
                                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                                foreach (var item in fall4)
                                {
                                    result.Add(new returndatagridDataclass
                                    {
                                        Id = item.Id.ToString(),
                                        LookupVal = item.LookupVal,
                                        LookupValData = db.AppCategory.Where(e => e.Id.ToString() == item.LookupValData).Select(r => r.FullDetails).FirstOrDefault().ToString(),
                                    });
                                }
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query42.Count(),
                                    iTotalDisplayRecords = fall4.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }

                            else
                            {
                                var result = from c in dcompanies4

                                             select new[] { null, Convert.ToString(c.Id), c.LookupVal, };
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query42.Count(),
                                    iTotalDisplayRecords = fall4.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case "PersonnelModel":

                            var query51 = db.PersonnelModelObject.Include(a => a.PersonnelModel).ToList();
                            var query52 = query51.Select(r => r.PersonnelModel).ToList();
                            IEnumerable<LookupValue> fall5;
                            if (param.sSearch == null)
                            {
                                fall5= query52;
                            }
                            else
                            {

                                fall5 = query52.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                          || (e.LookupVal.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                          || (e.LookupValData.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                          ).ToList();
                            }

                            var sortindex5 = Convert.ToInt32(Request["iSortCol_0"]);

                            Func<LookupValue, string> orderfunc5 = (c =>
                                                                        Convert.ToUInt32(sortindex5) == 0 ? c.Id.ToString() :
                                                                        sortindex5== 1 ? c.LookupVal : "");
                            var sortcolumn5 = Request["sSortDir_0"];
                            if (sortcolumn5 == "asc")
                            {
                                fall5 = fall5.OrderBy(orderfunc5);
                            }
                            else
                            {
                                fall5 = fall5.OrderByDescending(orderfunc5);
                            }
                            var dcompanies5= fall5
                                                        .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

                            if (dcompanies5.Count == 0)
                            {
                                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                                foreach (var item in fall5)
                                {
                                    result.Add(new returndatagridDataclass
                                    {
                                        Id = item.Id.ToString(),
                                        LookupVal = item.LookupVal,
                                        LookupValData = db.AppCategory.Where(e => e.Id.ToString() == item.LookupValData).Select(r => r.FullDetails).FirstOrDefault().ToString(),
                                    });
                                }
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query52.Count(),
                                    iTotalDisplayRecords = fall5.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }

                            else
                            {
                                var result = from c in dcompanies5

                                             select new[] { null, Convert.ToString(c.Id), c.LookupVal, };
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query52.Count(),
                                    iTotalDisplayRecords = fall5.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        case "ServiceModel":

                            var query61 = db.ServiceModelObject.Include(a => a.ServiceModel).ToList();
                            var query62 = query61.Select(r => r.ServiceModel).ToList();
                            IEnumerable<LookupValue> fall6;
                            if (param.sSearch == null)
                            {
                                fall6 = query62;
                            }
                            else
                            {

                                fall6 = query62.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                          || (e.LookupVal.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                          || (e.LookupValData.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                          ).ToList();
                            }

                            var sortindex6= Convert.ToInt32(Request["iSortCol_0"]);

                            Func<LookupValue, string> orderfunc6 = (c =>
                                                                        Convert.ToUInt32(sortindex6) == 0 ? c.Id.ToString() :
                                                                        sortindex6 == 1 ? c.LookupVal : "");
                            var sortcolumn6 = Request["sSortDir_0"];
                            if (sortcolumn6 == "asc")
                            {
                                fall6 = fall6.OrderBy(orderfunc6);
                            }
                            else
                            {
                                fall6 = fall6.OrderByDescending(orderfunc6);
                            }
                            var dcompanies6 = fall6
                                                        .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

                            if (dcompanies6.Count == 0)
                            {
                                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                                foreach (var item in fall6)
                                {
                                    result.Add(new returndatagridDataclass
                                    {
                                        Id = item.Id.ToString(),
                                        LookupVal = item.LookupVal,
                                        LookupValData = db.AppCategory.Where(e => e.Id.ToString() == item.LookupValData).Select(r => r.FullDetails).FirstOrDefault().ToString(),
                                    });
                                }
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query62.Count(),
                                    iTotalDisplayRecords = fall6.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }

                            else
                            {
                                var result = from c in dcompanies6

                                             select new[] { null, Convert.ToString(c.Id), c.LookupVal, };
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query62.Count(),
                                    iTotalDisplayRecords = fall6.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        case "SkillModel":

                            var query71 = db.SkillModelObject.Include(a => a.SkillModel).ToList();
                            var query72 = query71.Select(r => r.SkillModel).ToList();
                            IEnumerable<LookupValue> fall7;
                            if (param.sSearch == null)
                            {
                                fall7= query72;
                            }
                            else
                            {

                                fall7 = query72.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                          || (e.LookupVal.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                          || (e.LookupValData.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                          ).ToList();
                            }

                            var sortindex7 = Convert.ToInt32(Request["iSortCol_0"]);

                            Func<LookupValue, string> orderfunc7 = (c =>
                                                                        Convert.ToUInt32(sortindex7) == 0 ? c.Id.ToString() :
                                                                        sortindex7== 1 ? c.LookupVal : "");
                            var sortcolumn7= Request["sSortDir_0"];
                            if (sortcolumn7 == "asc")
                            {
                                fall7= fall7.OrderBy(orderfunc7);
                            }
                            else
                            {
                                fall7 = fall7.OrderByDescending(orderfunc7);
                            }
                            var dcompanies7 = fall7
                                                        .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

                            if (dcompanies7.Count == 0)
                            {
                                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                                foreach (var item in fall7)
                                {
                                    result.Add(new returndatagridDataclass
                                    {
                                        Id = item.Id.ToString(),
                                        LookupVal = item.LookupVal,
                                        LookupValData = db.AppCategory.Where(e => e.Id.ToString() == item.LookupValData).Select(r => r.FullDetails).FirstOrDefault().ToString(),
                                    });
                                }
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query72.Count(),
                                    iTotalDisplayRecords = fall7.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }

                            else
                            {
                                var result = from c in dcompanies7

                                             select new[] { null, Convert.ToString(c.Id), c.LookupVal, };
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query72.Count(),
                                    iTotalDisplayRecords = fall7.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }
                            break;
                        case "TrainingModel":

                            var query81 = db.TrainingModelObject.Include(a => a.TrainingModel).ToList();
                            var query82 = query81.Select(r => r.TrainingModel).ToList();

                            IEnumerable<LookupValue> fall8;
                            if (param.sSearch == null)
                            {
                                fall8 = query82;
                            }
                            else
                            {

                                fall8 = query82.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                          || (e.LookupVal.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                          || (e.LookupValData.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                          ).ToList();
                            }

                            var sortindex8 = Convert.ToInt32(Request["iSortCol_0"]);

                            Func<LookupValue, string> orderfunc8 = (c =>
                                                                        Convert.ToUInt32(sortindex8) == 0 ? c.Id.ToString() :
                                                                        sortindex8 == 1 ? c.LookupVal : "");
                            var sortcolumn8 = Request["sSortDir_0"];
                            if (sortcolumn8 == "asc")
                            {
                                fall8 = fall8.OrderBy(orderfunc8);
                            }
                            else
                            {
                                fall8 = fall8.OrderByDescending(orderfunc8);
                            }
                            var dcompanies8 = fall8
                                                        .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

                            if (dcompanies8.Count == 0)
                            {
                                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                                foreach (var item in fall8)
                                {
                                    result.Add(new returndatagridDataclass
                                    {
                                        Id = item.Id.ToString(),
                                        LookupVal = item.LookupVal,
                                        LookupValData = db.AppCategory.Where(e => e.Id.ToString() == item.LookupValData).Select(r => r.FullDetails).FirstOrDefault().ToString(),
                                    });
                                }
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query82.Count(),
                                    iTotalDisplayRecords = fall8.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }

                            else
                            {
                                var result = from c in dcompanies8

                                             select new[] { null, Convert.ToString(c.Id), c.LookupVal, };
                                return Json(new
                                {
                                    sEcho = param.sEcho,
                                    iTotalRecords = query82.Count(),
                                    iTotalDisplayRecords = fall8.Count(),
                                    data = result
                                }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                    }

                    return null;
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

    }
}