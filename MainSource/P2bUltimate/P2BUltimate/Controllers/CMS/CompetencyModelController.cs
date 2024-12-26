using CMS_SPS;
using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Web.Services;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;

namespace P2BUltimate.Controllers.CMS
{
    public class CompetencyModelController : Controller
    {
        //
        // GET: /CompetencyModel/
        public ActionResult Index()
        {
            Session["ModelData"] = "";
            return View("~/Views/CMS/MainViews/CompetencyModel/Index.cshtml");
        }

        public class EmpAnalysisDataClass
        {
            public string Id { get; set; }
            public string ModelObjectType { get; set; }
            public string ModelObjectList { get; set; }
            public string ModelSubTypeList { get; set; }

        }

        [HttpPost]
        public ActionResult EditSave(CompetencyModel c, int data, FormCollection form)
        {

            List<string> Msg = new List<string>();
            
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                         List<string> OModelList = new List<string>();
                         if (Session["ModelData"] != null && Session["ModelData"] != "")
                         {
                             List<EmpAnalysisDataClass> OModel = (List<EmpAnalysisDataClass>)Session["ModelData"];
                             foreach (var item in OModel)
                             {
                                 int ModelSubId = Convert.ToInt32(item.ModelObjectList.Split('-')[0]);
                                 switch (item.ModelObjectType)
                                 {
                                     case "AppraisalAttributeModel":
                                             var db_dataattribute = db.AppraisalAttributeModel.Find(ModelSubId);
                                             if (db_dataattribute != null)
                                             {
                                                 c.AppraisalAttributeModel = db_dataattribute;
                                             }
                                         break;

                                     case "AppraisalBusinessAppraisalModel":
                                         var db_databusiness = db.AppraisalBusinessAppraisalModel.Find(ModelSubId);
                                         if (db_databusiness != null)
                                         {
                                             c.AppraisalBusinessApprisalModel = db_databusiness;
                                         }
                                         break;

                                     case "AppraisalKRAModel":
                                         var db_datakra = db.AppraisalKRAModel.Find(ModelSubId);
                                        if (db_datakra !=null)
	                                    {
		                                     c.AppraisalKRAModel = db_datakra;
	                                    }
                                         break;

                                     case "PastExperienceModel":
                                         var db_datapastexperience = db.PastExperienceModel.Find(ModelSubId);
                                         if (db_datapastexperience != null)
                                         {
                                             c.PastExperienceModel = db_datapastexperience;
                                         }
                                        
                                         break;

                                     case "PersonnelModel":
                                         var db_datapersonnel = db.PersonnelModel.Find(ModelSubId);
                                         if (db_datapersonnel != null)
                                         {
                                             c.PersonnelModel = db_datapersonnel;
                                         }
                                         break;

                                     case "QualificationModel":

                                         var db_dataqualification = db.QualificationModel.Find(ModelSubId);
                                         if (db_dataqualification != null)
                                         {
                                             c.QualificationModel = db_dataqualification;
                                         }

                                        break;

                                     case "ServiceModel":
                                         var db_dataservice = db.ServiceModel.Find(ModelSubId);
                                         if (db_dataservice != null)
                                         {
                                             c.ServiceModel = db_dataservice;
                                         }
                                         break;

                                     case "SkillModel":
                                         var db_dataskill = db.SkillModel.Find(ModelSubId);
                                         if (db_dataskill != null)
                                         {
                                             c.SkillModel = db_dataskill;
                                         }
                                         break;

                                     case "TrainingModel":
                                         TrainingModel db_datatraining = db.TrainingModel.Find(ModelSubId);
                                         if (db_datatraining != null)
                                         {
                                             c.TrainingModel = db_datatraining;
                                         }
                                         break;
                                 }
                             }
                         }

                         var db_data = db.CompetencyModel.Where(e => e.Id == data).FirstOrDefault();
                         TempData["RowVersion"] = db_data.RowVersion;
                        CompetencyModel CMS_CompetencyModel = db.CompetencyModel.Find(data);
                        TempData["CurrRowVersion"] = CMS_CompetencyModel.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = CMS_CompetencyModel.DBTrack.CreatedBy == null ? null : CMS_CompetencyModel.DBTrack.CreatedBy,
                                CreatedOn = CMS_CompetencyModel.DBTrack.CreatedOn == null ? null : CMS_CompetencyModel.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            CMS_CompetencyModel.Id = data;
                            CMS_CompetencyModel.Code = c.Code;
                            CMS_CompetencyModel.ModelName = c.ModelName;
                            CMS_CompetencyModel.ModelDescription = c.ModelDescription;
                            CMS_CompetencyModel.CreatedDate = c.CreatedDate;
                            CMS_CompetencyModel.DBTrack = c.DBTrack;
                            CMS_CompetencyModel.AppraisalAttributeModel = c.AppraisalAttributeModel;
                            CMS_CompetencyModel.AppraisalBusinessApprisalModel = c.AppraisalBusinessApprisalModel;
                            CMS_CompetencyModel.AppraisalKRAModel = c.AppraisalKRAModel;
                            CMS_CompetencyModel.AppraisalPotentialModel = c.AppraisalPotentialModel;
                            CMS_CompetencyModel.PastExperienceModel = c.PastExperienceModel;
                            CMS_CompetencyModel.PersonnelModel = c.PersonnelModel;
                            CMS_CompetencyModel.QualificationModel = c.QualificationModel;
                            CMS_CompetencyModel.ServiceModel = c.ServiceModel;
                            CMS_CompetencyModel.SkillModel = c.SkillModel;
                            CMS_CompetencyModel.TrainingModel = c.TrainingModel;
                            db.CompetencyModel.Attach(CMS_CompetencyModel);
                            db.Entry(CMS_CompetencyModel).State = System.Data.Entity.EntityState.Modified;
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
        public ActionResult SaveData(string ModelObjectType, string ModelObjectList,string ModelObjectSubTypeList)
        {
            
            List<EmpAnalysisDataClass> OEvalDataClass = new List<EmpAnalysisDataClass>();
            if (Session["ModelData"] != null && Session["ModelData"] != "")
            {
                OEvalDataClass = (List<EmpAnalysisDataClass>)Session["ModelData"];
            }
            EmpAnalysisDataClass OData = new EmpAnalysisDataClass() 
            { 
                ModelObjectType = ModelObjectType,
                ModelObjectList = ModelObjectList,
                ModelSubTypeList = ModelObjectSubTypeList
            };
            OEvalDataClass.Add(OData);
            Session["ModelData"] = OEvalDataClass;
            return Json(OEvalDataClass, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CMS(string selectedcmn)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var all = db.CompetencyModel
                .Include(e => e.AppraisalAttributeModel)
                .Include(e => e.AppraisalBusinessApprisalModel)
                .Include(e => e.AppraisalKRAModel)
                .Include(e => e.AppraisalPotentialModel)
                .Include(e => e.PastExperienceModel)
                .Include(e => e.PersonnelModel)
                .Include(e => e.QualificationModel)
                .Include(e => e.ServiceModel)
                .Include(e => e.SkillModel)
                .Include(e => e.TrainingModel)
                .Where(e => e.ModelName == selectedcmn).SingleOrDefault();

                if (all.AppraisalAttributeModel != null)
                {
                    TempData["CMSOBJECTTYPE"] = "AttributeModel";
                }
                if (all.AppraisalBusinessApprisalModel != null)
                {
                    TempData["CMSOBJECTTYPE"] = "BusinessModel";
                }
                if (all.AppraisalKRAModel != null)
                {
                    TempData["CMSOBJECTTYPE"] = "KRAModel";
                }
                if (all.AppraisalPotentialModel != null)
                {
                    TempData["CMSOBJECTTYPE"] = "PotentialModel";
                }
                if (all.PastExperienceModel != null)
                {
                    TempData["CMSOBJECTTYPE"] = "PastExperienceModel";
                }
                if (all.PersonnelModel != null)
                {
                    TempData["CMSSUBOBJECTTYPE"] = "PersonnelModel";
                }
                if (all.QualificationModel != null)
                {
                    TempData["CMSOBJECTTYPE"] = "QualificationModel";
                }
                if (all.ServiceModel != null)
                {
                    TempData["CMSOBJECTTYPE"] = "ServiceModel";
                }
                if (all.SkillModel != null)
                {
                    TempData["CMSOBJECTTYPE"] = "SkillModel";
                }
                if (all.TrainingModel != null)
                {
                    TempData["CMSOBJECTTYPE"] = "TrainingModel";
                }

                return Json(TempData["CMSOBJECTTYPE"], JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetCompetencyData(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var qurey = db.CompetencyModel.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "ModelName", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DropdownlistModels(string Modellist,string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                switch (Modellist)
                {
                    case "AppraisalAttributeModel":
                          var qurey1 = db.AppraisalAttributeModel.ToList();
                          var selected = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected = Convert.ToInt32(data2);
                        }

                SelectList s = new SelectList(qurey1, "Id", "ModelName", selected);
                return Json(s, JsonRequestBehavior.AllowGet);

                        

                        break;

                    case "AppraisalBusinessAppraisalModel":

                        var qurey2 = db.AppraisalBusinessAppraisalModel.ToList();
                           selected = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected = Convert.ToInt32(data2);
                        }

                        s = new SelectList(qurey2, "Id", "ModelName", selected);
                        return Json(s, JsonRequestBehavior.AllowGet);

                        break;

                    case "AppraisalKRAModel":

                        var qurey3 = db.AppraisalKRAModel.ToList();
                         selected = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected = Convert.ToInt32(data2);
                        }

                 s = new SelectList(qurey3, "Id", "ModelName", selected);
                return Json(s, JsonRequestBehavior.AllowGet);

                        break;

                    case "AppraisalPotentialModel":

                        var qurey4 = db.AppraisalPotentialModel.ToList();
                            selected = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected = Convert.ToInt32(data2);
                        }

                  s = new SelectList(qurey4, "Id", "ModelName", selected);
                return Json(s, JsonRequestBehavior.AllowGet);


                        break;


                    case "PastExperienceModel":

                        var qurey5 = db.PastExperienceModel.ToList();
                           selected = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected = Convert.ToInt32(data2);
                        }

                 s = new SelectList(qurey5, "Id", "ModelName", selected);
                return Json(s, JsonRequestBehavior.AllowGet);


                        break;

                    case "PersonnelModel":

                        var qurey6 = db.PersonnelModel.ToList();
                           selected = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected = Convert.ToInt32(data2);
                        }

                        s = new SelectList(qurey6, "Id", "ModelName", selected);
                return Json(s, JsonRequestBehavior.AllowGet);

                        break;

                    case "QualificationModel":

                        var qurey7 = db.QualificationModel.ToList();
                           selected = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected = Convert.ToInt32(data2);
                        }

                        s = new SelectList(qurey7, "Id", "ModelName", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
                        break;

                    case "ServiceModel":

                        var qurey8 = db.ServiceModel.ToList();
                           selected = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected = Convert.ToInt32(data2);
                        }

                 s = new SelectList(qurey8, "Id", "ModelName", selected);
                return Json(s, JsonRequestBehavior.AllowGet);


                        break;

                    case "SkillModel":

                        var qurey9 = db.SkillModel.ToList();
                           selected = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected = Convert.ToInt32(data2);
                        }

                 s = new SelectList(qurey9, "Id", "ModelName", selected);
                return Json(s, JsonRequestBehavior.AllowGet);

                        break;

                    case "TrainingModel":

                        var qurey10 = db.TrainingModel.ToList();
                           selected = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected = Convert.ToInt32(data2);
                        }

                 s = new SelectList(qurey10, "Id", "ModelName", selected);
                return Json(s, JsonRequestBehavior.AllowGet);

                        break;

                }

                return null;


            }
        }

        public class returnDataClass1
        {
            public int Id { get; set; }
            public string AttributeModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public class returnEditDataClass1
        {
            public int Id { get; set; }
            public string AttributeModel { get; set; }
            public string EvaluationModel { get; set; }

        }
        public class returnDataClass2
        {
            public int Id { get; set; }
            public string BusinessModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public class returnEditDataClass2
        {
            public int Id { get; set; }
            public string BusinessModel { get; set; }
            public string EvaluationModel { get; set; }

        }
        public class returnDataClass3
        {
            public int Id { get; set; }
            public string KRAModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public class returnEditDataClass3
        {
            public int Id { get; set; }
            public string KRAModel { get; set; }
            public string EvaluationModel { get; set; }

        }
        public class returnDataClass4
        {
            public int Id { get; set; }
            public string PotentialModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public class returnEditDataClass4
        {
            public int Id { get; set; }
            public string PotentialModel { get; set; }
            public string EvaluationModel { get; set; }

        }
        public class returnDataClass5
        {
            public int Id { get; set; }
            public string ServiceModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }

        public class returnEditDataClass5
        {
            public int Id { get; set; }
            public string ServiceModel { get; set; }
            public string EvaluationModel { get; set; }

        }
        public class returnDataClass6
        {
            public int Id { get; set; }
            public string QualificationModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public class returnEditDataClass6
        {
            public int Id { get; set; }
            public string QualificationModel { get; set; }
            public string EvaluationModel { get; set; }

        }
        public class returnDataClass7
        {
            public int Id { get; set; }
            public string SkillModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public class returnEditDataClass7
        {
            public int Id { get; set; }
            public string SkillModel { get; set; }
            public string EvaluationModel { get; set; }

        }
        public class returnDataClass8
        {
            public int Id { get; set; }
            public string PastExperienceModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public class returnEditDataClass8
        {
            public int Id { get; set; }
            public string PastExperienceModel { get; set; }
            public string EvaluationModel { get; set; }

        }
        public class returnDataClass9
        {
            public int Id { get; set; }
            public string PersonnelModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public class returnEditDataClass9
        {
            public int Id { get; set; }
            public string PersonnelModel { get; set; }
            public string EvaluationModel { get; set; }

        }
        public class returnDataClass9New
        {
            public int Id { get; set; }
            public string ModelObjectType { get; set; }
            public string ModelObject { get; set; }
            public string PersonnelModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public class returnEditDataClass9New
        {
            public int Id { get; set; }
            public string ModelObjectType { get; set; }
            public string ModelObject { get; set; }
            public string ModelObjectSubType { get; set; }
            public string EvaluationModel { get; set; }
            public int ModelId { get; set; }

        }

        public class returnDataClass10
        {
            public int Id { get; set; }
            public string TrainingModel { get; set; }
            public string DataStepses { get; set; }
            public string Creteriases { get; set; }
            public string CreteriaTypeses { get; set; }
        }
        public class returnEditDataClass10
        {
            public int Id { get; set; }
            public string TrainingModel { get; set; }
            public string EvaluationModel { get; set; }

        }
                                                            

        public ActionResult GetModelData(string Modellist, string ModelobjId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
               
                //if (Val != "" && Modellist != "")
                //{
                //    TempData["ModelObjectListId"] = Val;
                //}
                //else
                //{ 
                //    Modellist = TempData["ModelName"].ToString();
                //    Val = TempData["ModelId"].ToString();
                //}
                
             
                switch (Modellist)
                {
                    case "AppraisalAttributeModel":
                        int id1 = Convert.ToInt32(ModelobjId);
                        var ObjAttributeModel = db.AppraisalAttributeModel
                            .Include(e => e.AppraisalAttributeModelObject)
                            .Include(e => e.AppraisalAttributeModelObject.Select(r => r.AppraisalAttributeModel))
                            .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel))
                            .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == id1).FirstOrDefault();
                        if (ObjAttributeModel.AppraisalAttributeModelObject != null)
                        {
                            List<returnDataClass1> returndata = new List<returnDataClass1>();
                            foreach (var item in ObjAttributeModel.AppraisalAttributeModelObject)
                            {
                                returndata.Add(new returnDataClass1
                                {
                                    Id = item.Id,
                                    AttributeModel = item.AppraisalAttributeModel != null ? db.AppCategory.Where(e => e.Id == item.AppraisalAttributeModel.Id).Select(r => r.FullDetails).FirstOrDefault() : " ",
                                    DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                                    Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                                    CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                                });
                            }
                            List<returnEditDataClass1> returndatases = new List<returnEditDataClass1>();
                            foreach (var item1 in returndata)
                            {
                                returndatases.Add(new returnEditDataClass1
                                {
                                    Id = item1.Id,
                                    AttributeModel = item1.AttributeModel,
                                    EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

                                });
                            }
                            var res = (from ca in returndatases
                                       select new
                                       {
                                           srno = ca.Id,
                                           lookupvalue = ca.AttributeModel + ca.EvaluationModel
                                       }).ToList();

                            var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

                            return Json(result, JsonRequestBehavior.AllowGet);
                        }

                        break;

                    case "AppraisalBusinessAppraisalModel":

                        int id2 = Convert.ToInt32(ModelobjId);
                        var ObjBusinessModel = db.AppraisalBusinessAppraisalModel
                            .Include(e => e.AppraisalBusinessAppraisalModelObject)
                            .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.AppraisalBusinessAppraisalModel))
                            .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel))
                            .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == id2).SingleOrDefault();
                        if (ObjBusinessModel.AppraisalBusinessAppraisalModelObject != null)
                        {
                            List<returnDataClass2> returndata = new List<returnDataClass2>();
                            foreach (var item in ObjBusinessModel.AppraisalBusinessAppraisalModelObject)
                            {
                                returndata.Add(new returnDataClass2
                                {
                                    Id = item.Id,
                                    BusinessModel = item.AppraisalBusinessAppraisalModel != null ? db.BA_Category.Where(e => e.Id == item.AppraisalBusinessAppraisalModel.Id).Select(r => r.FullDetails).FirstOrDefault() : " ",
                                    DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                                    Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                                    CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                                });
                            }
                            List<returnEditDataClass2> returndatases = new List<returnEditDataClass2>();
                            foreach (var item1 in returndata)
                            {
                                returndatases.Add(new returnEditDataClass2
                                {
                                    Id = item1.Id,
                                    BusinessModel = item1.BusinessModel,
                                    EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

                                });
                            }
                            var res = (from ca in returndatases
                                       select new
                                       {
                                           srno = ca.Id,
                                           lookupvalue = ca.BusinessModel + ca.EvaluationModel
                                       }).ToList();

                            var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

                            return Json(result, JsonRequestBehavior.AllowGet);
                        }

                        break;

                    case "AppraisalKRAModel":

                        int id3 = Convert.ToInt32(ModelobjId);
                        var ObjKRAModel = db.AppraisalKRAModel
                            .Include(e => e.AppraisalKRAModelObject)
                            .Include(e => e.AppraisalKRAModelObject.Select(r => r.AppraisalKRAModel))
                            .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel))
                            .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == id3).SingleOrDefault();
                        if (ObjKRAModel.AppraisalKRAModelObject != null)
                        {
                            List<returnDataClass3> returndata = new List<returnDataClass3>();
                            foreach (var item in ObjKRAModel.AppraisalKRAModelObject)
                            {
                                returndata.Add(new returnDataClass3
                                {
                                    Id = item.Id,
                                    KRAModel = item.AppraisalKRAModel != null ? db.AppCategory.Where(e => e.Id == item.AppraisalKRAModel.Id).Select(r => r.FullDetails).FirstOrDefault() : " ",
                                    DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                                    Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                                    CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                                });
                            }
                            List<returnEditDataClass3> returndatases = new List<returnEditDataClass3>();
                            foreach (var item1 in returndata)
                            {
                                returndatases.Add(new returnEditDataClass3
                                {
                                    Id = item1.Id,
                                    KRAModel = item1.KRAModel,
                                    EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

                                });
                            }
                            var res = (from ca in returndatases
                                       select new
                                       {
                                           srno = ca.Id,
                                           lookupvalue = ca.KRAModel + ca.EvaluationModel
                                       }).ToList();

                            var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

                            return Json(result, JsonRequestBehavior.AllowGet);
                        }

                        break;

                    case "AppraisalPotentialModel":

                        int id4 = Convert.ToInt32(ModelobjId);
                        var ObjPotentialmodel = db.AppraisalPotentialModel
                            .Include(e => e.AppraisalPotentialModelObject)
                            .Include(e => e.AppraisalPotentialModelObject.Select(r => r.AppraisalPotentialModel))
                            .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel))
                            .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == id4).SingleOrDefault();
                        if (ObjPotentialmodel.AppraisalPotentialModelObject != null)
                        {
                            List<returnDataClass4> returndata = new List<returnDataClass4>();
                            foreach (var item in ObjPotentialmodel.AppraisalPotentialModelObject)
                            {
                                returndata.Add(new returnDataClass4
                                {
                                    Id = item.Id,
                                    PotentialModel = item.AppraisalPotentialModel != null ? db.AppCategory.Where(e => e.Id == item.AppraisalPotentialModel.Id).Select(r => r.FullDetails).FirstOrDefault() : " ",
                                    DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                                    Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                                    CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                                });
                            }
                            List<returnEditDataClass4> returndatases = new List<returnEditDataClass4>();
                            foreach (var item1 in returndata)
                            {
                                returndatases.Add(new returnEditDataClass4
                                {
                                    Id = item1.Id,
                                    PotentialModel = item1.PotentialModel,
                                    EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

                                });
                            }
                            var res = (from ca in returndatases
                                       select new
                                       {
                                           srno = ca.Id,
                                           lookupvalue = ca.PotentialModel + ca.EvaluationModel
                                       }).ToList();

                            var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                        break;

                    case "ServiceModel":

                        int id5 = Convert.ToInt32(ModelobjId);
                        var ObjServicemodel = db.ServiceModel
                            .Include(e => e.ServiceModelObject)
                            .Include(e => e.ServiceModelObject.Select(r => r.ServiceModel))
                            .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel))
                            .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == id5).SingleOrDefault();
                        if (ObjServicemodel.ServiceModelObject != null)
                        {
                            List<returnDataClass5> returndata = new List<returnDataClass5>();
                            foreach (var item in ObjServicemodel.ServiceModelObject)
                            {
                                returndata.Add(new returnDataClass5
                                {
                                    Id = item.Id,
                                    ServiceModel = item.ServiceModel != null ? item.ServiceModel.LookupVal : "",
                                    DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                                    Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                                    CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                                });
                            }
                            List<returnEditDataClass5> returndatases = new List<returnEditDataClass5>();
                            foreach (var item1 in returndata)
                            {
                                returndatases.Add(new returnEditDataClass5
                                {
                                    Id = item1.Id,
                                    ServiceModel = item1.ServiceModel,
                                    EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

                                });
                            }
                            var res = (from ca in returndatases
                                       select new
                                       {
                                           srno = ca.Id,
                                           lookupvalue = ca.ServiceModel + ca.EvaluationModel
                                       }).ToList();

                            var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                        break;

                    case "QualificationModel":
                        int id6 = Convert.ToInt32(ModelobjId);
                        var ObjQualificationModel = db.QualificationModel
                            .Include(e => e.QualificationModelObject)
                            .Include(e => e.QualificationModelObject.Select(r => r.QualificationModel))
                            .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel))
                            .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == id6).SingleOrDefault();
                        if (ObjQualificationModel.QualificationModelObject != null)
                        {
                            List<returnDataClass6> returndata = new List<returnDataClass6>();
                            foreach (var item in ObjQualificationModel.QualificationModelObject)
                            {
                                returndata.Add(new returnDataClass6
                                {
                                    Id = item.Id,
                                    QualificationModel = item.QualificationModel != null ? item.QualificationModel.LookupVal : "",
                                    DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                                    Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                                    CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                                });
                            }
                            List<returnEditDataClass6> returndatases = new List<returnEditDataClass6>();
                            foreach (var item1 in returndata)
                            {
                                returndatases.Add(new returnEditDataClass6
                                {
                                    Id = item1.Id,
                                    QualificationModel = item1.QualificationModel,
                                    EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

                                });
                            }
                            var res = (from ca in returndatases
                                       select new
                                       {
                                           srno = ca.Id,
                                           lookupvalue = ca.QualificationModel + ca.EvaluationModel
                                       }).ToList();
                            var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();
                            return Json(result, JsonRequestBehavior.AllowGet);

                        }
                        break;

                    case "SkillModel":
                        int id7 = Convert.ToInt32(ModelobjId);
                        var ObjSkillModel = db.SkillModel
                            .Include(e => e.SkillModelObject)
                            .Include(e => e.SkillModelObject.Select(r => r.SkillModel))
                            .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel))
                            .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == id7).SingleOrDefault();
                        if (ObjSkillModel.SkillModelObject != null)
                        {
                            List<returnDataClass7> returndata = new List<returnDataClass7>();
                            foreach (var item in ObjSkillModel.SkillModelObject)
                            {
                                returndata.Add(new returnDataClass7
                                {
                                    Id = item.Id,
                                    SkillModel = item.SkillModel != null ? item.SkillModel.LookupVal : "",
                                    DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                                    Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                                    CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                                });
                            }
                            List<returnEditDataClass7> returndatases = new List<returnEditDataClass7>();
                            foreach (var item1 in returndata)
                            {
                                returndatases.Add(new returnEditDataClass7
                                {
                                    Id = item1.Id,
                                    SkillModel = item1.SkillModel,
                                    EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

                                });
                            }
                            var res = (from ca in returndatases
                                       select new
                                       {
                                           srno = ca.Id,
                                           lookupvalue = ca.SkillModel + ca.EvaluationModel
                                       }).ToList();
                            var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();
                            return Json(result, JsonRequestBehavior.AllowGet);

                        }

                        break;

                    case "PastExperienceModel":

                        int id8 = Convert.ToInt32(ModelobjId);
                        var ObjPastExperienceModel = db.PastExperienceModel
                            .Include(e => e.PastExperienceModelObject)
                            .Include(e => e.PastExperienceModelObject.Select(r => r.PastExperienceModel))
                            .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel))
                            .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == id8).SingleOrDefault();
                        if (ObjPastExperienceModel.PastExperienceModelObject != null)
                        {
                            List<returnDataClass8> returndata = new List<returnDataClass8>();
                            foreach (var item in ObjPastExperienceModel.PastExperienceModelObject)
                            {
                                returndata.Add(new returnDataClass8
                                {
                                    Id = item.Id,
                                    PastExperienceModel = item.PastExperienceModel != null ? item.PastExperienceModel.LookupVal : "",
                                    DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                                    Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                                    CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                                });
                            }
                            List<returnEditDataClass8> returndatases = new List<returnEditDataClass8>();
                            foreach (var item1 in returndata)
                            {
                                returndatases.Add(new returnEditDataClass8
                                {
                                    Id = item1.Id,
                                    PastExperienceModel = item1.PastExperienceModel,
                                    EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

                                });
                            }
                            var res = (from ca in returndatases
                                       select new
                                       {
                                           srno = ca.Id,
                                           lookupvalue = ca.PastExperienceModel + ca.EvaluationModel
                                       }).ToList();
                            var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();
                            return Json(result, JsonRequestBehavior.AllowGet);

                        }
                        break;

                    case "PersonnelModel":
                        int id9 = Convert.ToInt32(ModelobjId);
                        var ObjPersonnelModel = db.PersonnelModel
                           .Include(e => e.PersonnelModelObject)
                           .Include(e => e.PersonnelModelObject.Select(r => r.PersonnelModel))
                           .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel))
                           .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                           .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                           .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == id9).SingleOrDefault();
                        if (ObjPersonnelModel.PersonnelModelObject != null)
                        {
                            List<returnDataClass9> returndata = new List<returnDataClass9>();
                            foreach (var item in ObjPersonnelModel.PersonnelModelObject)
                            {
                                returndata.Add(new returnDataClass9
                                {
                                    Id = item.Id,
                                    PersonnelModel = item.PersonnelModel != null ? item.PersonnelModel.LookupVal : "",
                                    DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                                    Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                                    CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                                });
                            }
                            List<returnEditDataClass9> returndatases = new List<returnEditDataClass9>();
                            foreach (var item1 in returndata)
                            {
                                returndatases.Add(new returnEditDataClass9
                                {
                                    Id = item1.Id,
                                    PersonnelModel = item1.PersonnelModel,
                                    EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

                                });
                            }
                            var res = (from ca in returndatases
                                       select new
                                       {
                                           srno = ca.Id,
                                           lookupvalue = ca.PersonnelModel + ca.EvaluationModel
                                       }).ToList();
                            var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();
                            return Json(result, JsonRequestBehavior.AllowGet);

                        }

                        break;
                    case "TrainingModel":
                        int id10 = Convert.ToInt32(ModelobjId);
                        var ObjTrainingModel = db.TrainingModel
                            .Include(e => e.TrainingModelObject)
                            .Include(e => e.TrainingModelObject.Select(r => r.TrainingModel))
                            .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel))
                            .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                            .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                            .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == id10).SingleOrDefault();
                        if (ObjTrainingModel.TrainingModelObject != null)
                        {
                            List<returnDataClass10> returndata = new List<returnDataClass10>();
                            foreach (var item in ObjTrainingModel.TrainingModelObject)
                            {
                                returndata.Add(new returnDataClass10
                                {
                                    Id = item.Id,
                                    TrainingModel = item.TrainingModel != null ? db.LookupValue.Where(e => e.Id == item.TrainingModel.Id).Select(r => r.LookupVal).FirstOrDefault() : " ",
                                    DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                                    Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                                    CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                                });
                            }
                            List<returnEditDataClass10> returndatases = new List<returnEditDataClass10>();
                            foreach (var item1 in returndata)
                            {
                                returndatases.Add(new returnEditDataClass10
                                {
                                    Id = item1.Id,
                                    TrainingModel = item1.TrainingModel,
                                    EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

                                });
                            }
                            var res = (from ca in returndatases
                                       select new
                                       {
                                           srno = ca.Id,
                                           lookupvalue = ca.TrainingModel + ca.EvaluationModel
                                       }).ToList();

                            var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

                            return Json(result, JsonRequestBehavior.AllowGet);
                        }

                        break;


                }
                return null;

            }

        }

        public class returndatagridDataclass //Parentgrid
        {
            public string Id { get; set; }
            public string ModelObjectList { get; set; }
            public string CompetencyModelObjectType { get; set; }
            public string Code { get; set; }
            public string ModelName { get; set; }
            public string ModelDescription { get; set; }
            public string CreatedDate { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    CompetencyModel competencymodels = db.CompetencyModel
                        .Include(e => e.AppraisalAttributeModel)
                        .Include(e => e.AppraisalBusinessApprisalModel)
                        .Include(e => e.AppraisalKRAModel)
                        .Include(e => e.AppraisalPotentialModel)
                        .Include(e => e.PastExperienceModel)
                        .Include(e => e.PersonnelModel)
                        .Include(e => e.QualificationModel)
                        .Include(e => e.ServiceModel)
                        .Include(e => e.SkillModel)
                        .Include(e => e.TrainingModel)
                        .Where(e => e.Id == data).SingleOrDefault();
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = competencymodels.DBTrack.CreatedBy != null ? competencymodels.DBTrack.CreatedBy : null,
                            CreatedOn = competencymodels.DBTrack.CreatedOn != null ? competencymodels.DBTrack.CreatedOn : null,
                            IsModified = competencymodels.DBTrack.IsModified == true ? false : false//,

                        };
                        db.Entry(competencymodels).State = System.Data.Entity.EntityState.Deleted;
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

        public ActionResult CompetencyModel_Grid(ParamModel param, string y, string selectedtextcmn)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.CompetencyModel
                        .Include(e => e.AppraisalAttributeModel)
                        .Include(e => e.AppraisalBusinessApprisalModel)
                        .Include(e => e.AppraisalKRAModel)
                        .Include(e => e.AppraisalPotentialModel)
                        .Include(e => e.PastExperienceModel)
                        .Include(e => e.PersonnelModel)
                        .Include(e => e.QualificationModel)
                        .Include(e => e.ServiceModel)
                        .Include(e => e.SkillModel)
                        .Include(e => e.TrainingModel)
                        .ToList().Where(e => e.ModelName == selectedtextcmn);

                    IEnumerable<CompetencyModel> fall;
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

                    Func<CompetencyModel, string> orderfunc = (c =>
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
                                //ModelObjectList = ModelObjectList,
                                //CompetencyModelObjectType = CompetencyModelObjectType,
                                Code = item.Code,
                                ModelName = item.ModelName,
                                ModelDescription = item.ModelDescription,
                                CreatedDate = item.CreatedDate.Value.ToShortDateString(),

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
        public ActionResult A_CompetencyModel_Grid(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                var db_data = db.CompetencyModel
                    .Include(e => e.AppraisalAttributeModel)
                    .Include(e => e.AppraisalBusinessApprisalModel)
                    .Include(e => e.AppraisalKRAModel)
                    .Include(e => e.AppraisalPotentialModel)
                    .Include(e => e.PastExperienceModel)
                    .Include(e => e.PersonnelModel)
                    .Include(e => e.QualificationModel)
                    .Include(e => e.ServiceModel)
                    .Include(e => e.SkillModel)
                    .Include(e => e.TrainingModel)
                    .Where(e => e.Id == data).FirstOrDefault();

                List<returnEditDataClass9New> returndatases = new List<returnEditDataClass9New>();
                string DataStepses = "";
                string Creteriases = "";
                string CreteriaTypeses = "";

                if (db_data.AppraisalAttributeModel != null)
                {
                    int ids = db_data.AppraisalAttributeModel.Id;
                    var db_dataattribute = db.AppraisalAttributeModel
                   .Include(e => e.AppraisalAttributeModelObject)
                   .Include(e => e.AppraisalAttributeModelObject.Select(r => r.AppraisalAttributeModel))
                   .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel))
                   .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                   .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                   .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                   .Where(e => e.Id == ids).FirstOrDefault();

                    if (db_dataattribute.AppraisalAttributeModelObject != null)
                    {
                        returndatases.Add(new returnEditDataClass9New
                        {
                            Id = db_dataattribute.Id,
                            ModelObjectType = "AppraisalAttributeModel",
                            ModelObject = db_dataattribute.Code + "-" + db_dataattribute.ModelName,
                            //ModelObjectSubType = item.AppraisalAttributeModel.LookupVal,
                            //EvaluationModel = Creteriases + " " + CreteriaTypeses + " " + DataStepses,
                            //ModelId = db_dataattribute.Id

                        });

                        //foreach (var item in db_dataattribute.AppraisalAttributeModelObject)
                        //{
                        //    DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "";
                        //    Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "";
                        //    CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "";

                        //    returndatases.Add(new returnEditDataClass9New
                        //    {
                        //        Id = item.Id,
                        //        ModelObjectType = "AppraisalAttributeModel",
                        //        ModelObject = db_dataattribute.Code + "-" + db_dataattribute.ModelName,
                        //        ModelObjectSubType = item.AppraisalAttributeModel.LookupVal,
                        //        EvaluationModel = Creteriases + " " + CreteriaTypeses + " " + DataStepses,
                        //        ModelId = db_dataattribute.Id

                        //    });
                        //}
                     
                    }
                }
                if (db_data.AppraisalBusinessApprisalModel != null)
                {
                    int ids = db_data.AppraisalBusinessApprisalModel.Id;
                    var db_databusiness = db.AppraisalBusinessAppraisalModel
                   .Include(e => e.AppraisalBusinessAppraisalModelObject)
                   .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.AppraisalBusinessAppraisalModel))
                   .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel))
                   .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                   .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                   .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                   .Where(e => e.Id == ids).FirstOrDefault();

                    if (db_databusiness.AppraisalBusinessAppraisalModelObject != null)
                    {
                        foreach (var item in db_databusiness.AppraisalBusinessAppraisalModelObject)
                        {
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "";
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "";
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "";

                            returndatases.Add(new returnEditDataClass9New
                            {
                                Id = item.Id,
                                ModelObjectType = "AppraisalBusinessApprisalModel",
                                ModelObject = db_databusiness.Code + "-" + db_databusiness.ModelName,
                                ModelObjectSubType = item.AppraisalBusinessAppraisalModel.LookupVal,
                                EvaluationModel = Creteriases + " " + CreteriaTypeses + " " + DataStepses,
                                ModelId = db_databusiness.Id

                            });
                        } 
                    }
                }
                if (db_data.AppraisalKRAModel != null)
                {
                    int ids = db_data.AppraisalKRAModel.Id;
                    var db_datakra = db.AppraisalKRAModel
                   .Include(e => e.AppraisalKRAModelObject)
                   .Include(e => e.AppraisalKRAModelObject.Select(r => r.AppraisalKRAModel))
                   .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel))
                   .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                   .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                   .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                   .Where(e => e.Id == ids).FirstOrDefault();

                    if (db_datakra.AppraisalKRAModelObject != null)
                    {
                        foreach (var item in db_datakra.AppraisalKRAModelObject)
                        {
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "";
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "";
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "";

                            returndatases.Add(new returnEditDataClass9New
                            {
                                Id = item.Id,
                                ModelObjectType = "AppraisalKRAModel",
                                ModelObject = db_datakra.Code + "-" + db_datakra.ModelName,
                                ModelObjectSubType = item.AppraisalKRAModel.LookupVal,
                                EvaluationModel = Creteriases + " " + CreteriaTypeses + " " + DataStepses,
                                ModelId = db_datakra.Id

                            });
                        }
                    }
                }
                if (db_data.AppraisalPotentialModel != null)
                {
                    int ids = db_data.AppraisalPotentialModel.Id;
                    var db_datapotential = db.AppraisalPotentialModel
                   .Include(e => e.AppraisalPotentialModelObject)
                   .Include(e => e.AppraisalPotentialModelObject.Select(r => r.AppraisalPotentialModel))
                   .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel))
                   .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                   .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                   .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                   .Where(e => e.Id == ids).FirstOrDefault();

                    if (db_datapotential.AppraisalPotentialModelObject != null)
                    {
                        foreach (var item in db_datapotential.AppraisalPotentialModelObject)
                        {
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "";
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "";
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "";

                            returndatases.Add(new returnEditDataClass9New
                            {
                                Id = item.Id,
                                ModelObjectType = "AppraisalPotentialModel",
                                ModelObject = db_datapotential.Code + "-" + db_datapotential.ModelName,
                                ModelObjectSubType = item.AppraisalPotentialModel.LookupVal,
                                EvaluationModel = Creteriases + " " + CreteriaTypeses + " " + DataStepses,
                                ModelId = db_datapotential.Id

                            });
                        }

                        
                    }
                }
                if (db_data.PastExperienceModel != null)
                {
                    int ids = db_data.PastExperienceModel.Id;
                    var db_datapastexperience = db.PastExperienceModel
                   .Include(e => e.PastExperienceModelObject)
                   .Include(e => e.PastExperienceModelObject.Select(r => r.PastExperienceModel))
                   .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel))
                   .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                   .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                   .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                   .Where(e => e.Id == ids).FirstOrDefault();

                    if (db_datapastexperience.PastExperienceModelObject != null)
                    {
                        foreach (var item in db_datapastexperience.PastExperienceModelObject)
                        {
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "";
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "";
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "";

                            returndatases.Add(new returnEditDataClass9New
                            {
                                Id = item.Id,
                                ModelObjectType = "PastExperienceModel",
                                ModelObject = db_datapastexperience.Code + "-" + db_datapastexperience.ModelName,
                                ModelObjectSubType = item.PastExperienceModel.LookupVal,
                                EvaluationModel = Creteriases + " " + CreteriaTypeses + " " + DataStepses,
                                ModelId = db_datapastexperience.Id

                            });
                        }

                    }
                }
                if (db_data.PersonnelModel != null)
                {
                    int ids = db_data.PersonnelModel.Id;
                    var db_datapersonnel = db.PersonnelModel
                   .Include(e => e.PersonnelModelObject)
                   .Include(e => e.PersonnelModelObject.Select(r => r.PersonnelModel))
                   .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel))
                   .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                   .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                   .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                   .Where(e => e.Id == ids).FirstOrDefault();

                    if (db_datapersonnel.PersonnelModelObject != null)
                    {
                        returndatases.Add(new returnEditDataClass9New
                        {
                            Id = db_datapersonnel.Id,
                            ModelObjectType = "PersonnelModel",
                            ModelObject = db_datapersonnel.Code + "-" + db_datapersonnel.ModelName,
                            //ModelObjectSubType = item.PersonnelModel.LookupVal,
                            //EvaluationModel = Creteriases + " " + CreteriaTypeses + " " + DataStepses,
                            //ModelId = db_datapersonnel.Id

                        });
                        //foreach (var item in db_datapersonnel.PersonnelModelObject)
                        //{
                        //    DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "";
                        //    Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "";
                        //    CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "";

                        //    returndatases.Add(new returnEditDataClass9New
                        //    {
                        //        Id = item.Id,
                        //        ModelObjectType = "PersonnelModel",
                        //        ModelObject = db_datapersonnel.Code + "-" + db_datapersonnel.ModelName,
                        //        ModelObjectSubType = item.PersonnelModel.LookupVal,
                        //        EvaluationModel = Creteriases + " " + CreteriaTypeses + " " + DataStepses,
                        //        ModelId = db_datapersonnel.Id

                        //    });
                        //}

                    }
                }
                if (db_data.ServiceModel != null)
                {
                    int ids = db_data.ServiceModel.Id;
                    var db_dataservice = db.ServiceModel
                   .Include(e => e.ServiceModelObject)
                   .Include(e => e.ServiceModelObject.Select(r => r.ServiceModel))
                   .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel))
                   .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                   .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                   .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                   .Where(e => e.Id == ids).FirstOrDefault();

                    if (db_dataservice.ServiceModelObject != null)
                    {
                        foreach (var item in db_dataservice.ServiceModelObject)
                        {
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "";
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "";
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "";

                            returndatases.Add(new returnEditDataClass9New
                            {
                                Id = item.Id,
                                ModelObjectType = "ServiceModel",
                                ModelObject = db_dataservice.Code + "-" + db_dataservice.ModelName,
                                ModelObjectSubType = item.ServiceModel.LookupVal,
                                EvaluationModel = Creteriases + " " + CreteriaTypeses + " " + DataStepses,
                                ModelId = db_dataservice.Id

                            });
                        }
                    }
                }
                if (db_data.SkillModel != null)
                {
                    int ids = db_data.SkillModel.Id;
                    var db_dataskill = db.SkillModel
                   .Include(e => e.SkillModelObject)
                   .Include(e => e.SkillModelObject.Select(r => r.SkillModel))
                   .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel))
                   .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                   .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                   .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                   .Where(e => e.Id == ids).FirstOrDefault();

                    if (db_dataskill.SkillModelObject != null)
                    {
                        returndatases.Add(new returnEditDataClass9New
                        {
                            Id = db_dataskill.Id,
                            ModelObjectType = "SkillModel",
                            ModelObject = db_dataskill.Code + "-" + db_dataskill.ModelName,
                            //ModelObjectSubType = item.SkillModel.LookupVal,
                            //EvaluationModel = Creteriases + " " + CreteriaTypeses + " " + DataStepses,
                            //ModelId = db_dataskill.Id
                        });
                        //foreach (var item in db_dataskill.SkillModelObject)
                        //{
                        //    DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "";
                        //    Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "";
                        //    CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "";

                        //    returndatases.Add(new returnEditDataClass9New
                        //    {
                        //        Id = item.Id,
                        //        ModelObjectType = "SkillModel",
                        //        ModelObject = db_dataskill.Code + "-" + db_dataskill.ModelName,
                        //        ModelObjectSubType = item.SkillModel.LookupVal,
                        //        EvaluationModel = Creteriases + " " + CreteriaTypeses + " " + DataStepses,
                        //        ModelId = db_dataskill.Id
                        //    });
                        //}
                    }
                }

                if (db_data.QualificationModel != null)
                {
                    int ids = db_data.QualificationModel.Id;
                    var db_dataqualification = db.QualificationModel
                   .Include(e => e.QualificationModelObject)
                   .Include(e => e.QualificationModelObject.Select(r => r.QualificationModel))
                   .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel))
                   .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                   .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                   .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                   .Where(e => e.Id == ids).FirstOrDefault();

                    if (db_dataqualification.QualificationModelObject != null)
                    {
                        foreach (var item in db_dataqualification.QualificationModelObject)
                        {
                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "";
                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "";
                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "";

                            returndatases.Add(new returnEditDataClass9New
                            {
                                Id = item.Id,
                                ModelObjectType = "QualificationModel",
                                ModelObject = db_dataqualification.Code + "-" + db_dataqualification.ModelName,
                                ModelObjectSubType = item.QualificationModel.LookupVal,
                                EvaluationModel = Creteriases + " " + CreteriaTypeses + " " + DataStepses,
                                ModelId = db_dataqualification.Id

                            });
                        }
                    }
                }

                if (db_data.TrainingModel != null)
                {
                    int ids = db_data.TrainingModel.Id;
                    var db_datatraining = db.TrainingModel
                   .Include(e => e.TrainingModelObject)
                   .Include(e => e.TrainingModelObject.Select(r => r.TrainingModel))
                   .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel))
                   .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                   .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                   .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                   .Where(e => e.Id == ids).FirstOrDefault();

                    if (db_datatraining.TrainingModelObject != null)
                    {
                        returndatases.Add(new returnEditDataClass9New
                        {
                            Id = db_datatraining.Id,
                            ModelObjectType = "TrainingModel",
                            ModelObject = db_datatraining.Code + "-" + db_datatraining.ModelName

                        });
                       
                    }
                }

                return Json(returndatases, JsonRequestBehavior.AllowGet);
            }
        }
        //public ActionResult Edit(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        var itemdata = db.CompetencyModel
        //            .Include(e => e.AppraisalAttributeModel)
        //             .Include(e => e.AppraisalBusinessApprisalModel)
        //             .Include(e => e.AppraisalKRAModel)
        //             .Include(e => e.AppraisalPotentialModel)
        //             .Include(e => e.PastExperienceModel)
        //             .Include(e => e.PersonnelModel)
        //             .Include(e => e.QualificationModel)
        //             .Include(e => e.ServiceModel)
        //             .Include(e => e.SkillModel)
        //             .Include(e => e.TrainingModel)
        //             .Where(e => e.Id == data).SingleOrDefault();

        //        if (itemdata.AppraisalAttributeModel != null)
        //        {

        //            var Lookupideasim = db.Lookup.Include(e=>e.LookupValues).Where(e => e.Code == "507").SingleOrDefault();
        //            var LookupValueideasim = Lookupideasim.LookupValues.ToList();
        //            int lookupvalideasim = LookupValueideasim.Where(e => e.LookupVal == "AppraisalAttributeModel").SingleOrDefault().Id;
        //            TempData["CMSOBJECTTYPE2"] = lookupvalideasim;
        //            List<returndatagridDataclass> result = new List<returndatagridDataclass>();

        //            result.Add(new returndatagridDataclass
        //            {
        //                Id = itemdata.Id.ToString(),
        //                Code = itemdata.Code,
        //                ModelName = itemdata.ModelName,
        //                ModelDescription = itemdata.ModelDescription,
        //                CreatedDate = itemdata.CreatedDate.Value.ToShortDateString(),
        //                CompetencyModelObjectType = TempData["CMSOBJECTTYPE2"].ToString()                 
        //            });

        //            return Json(new Object[] { result, JsonRequestBehavior.AllowGet });

        //        }


        //        return null;

        //    }
        //}

       

        [HttpPost]
        public ActionResult Create(CompetencyModel c, string Modellist, FormCollection form)
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (db.CompetencyModel.Any(e => e.ModelName == c.ModelName))
                    {
                        Msg.Add("Record Already Exists.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    string disc = form["ModelObjectList"] == "0" ? "" : form["ModelObjectList"];
                    //int modelobjectlistids = Convert.ToInt32(TempData["ModelObjectListId"]);
                    int modellist = Convert.ToInt32(Modellist);
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    CompanyCMS_SPS companycms_sps = new CompanyCMS_SPS();
                    List<string> OModelList = new List<string>();
                    if (Session["ModelData"] != null)
                    {
                        List<EmpAnalysisDataClass> OModel = (List<EmpAnalysisDataClass>)Session["ModelData"];
                        foreach (var item in OModel)
                        {
                            int ModelSubId = Convert.ToInt32(item.ModelObjectList.Split('-')[0]);
                            switch (item.ModelObjectType)
                            {
                                case "AppraisalAttributeModel":
                                    if (disc != null && disc != "")
                                    {
                                        List<AppraisalAttributeModelObject> CMS_AttributeModelObjectList = new List<AppraisalAttributeModelObject>();
                                        var db_dataattribute = db.AppraisalAttributeModel.Include(e => e.AppraisalAttributeModelObject).Where(e => e.Id == ModelSubId).FirstOrDefault();
                                        //if (item.ModelSubTypeList != null && item.ModelSubTypeList != "")
                                        //{
                                        //    var ids = Utility.StringIdsToListIds(item.ModelSubTypeList);
                                        //        foreach (var ca in ids)
                                        //        {
                                        //            var AttributeModelObject_val = db.AppraisalAttributeModelObject.Find(ca);
                                        //            CMS_AttributeModelObjectList.Add(AttributeModelObject_val);
                                        //            db_dataattribute.AppraisalAttributeModelObject = CMS_AttributeModelObjectList;
                                        //        }
                                        //}
                                        db.AppraisalAttributeModel.Attach(db_dataattribute);
                                        db.Entry(db_dataattribute).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        AppraisalAttributeModel CMS_AppraisalAttributeModel = db.AppraisalAttributeModel.Find(ModelSubId);
                                        CMS_AppraisalAttributeModel.Id = ModelSubId;
                                        CMS_AppraisalAttributeModel.AppraisalAttributeModelObject = db_dataattribute.AppraisalAttributeModelObject;
                                        db.AppraisalAttributeModel.Attach(CMS_AppraisalAttributeModel);
                                        db.Entry(CMS_AppraisalAttributeModel).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        c.AppraisalAttributeModel = CMS_AppraisalAttributeModel;
                                    }
                                    break;

                                case "AppraisalBusinessAppraisalModel":

                                    List<AppraisalBusinessAppraisalModelObject> CMS_BusinessAppraisalModelObjectList = new List<AppraisalBusinessAppraisalModelObject>();
                                    var db_databusiness = db.AppraisalBusinessAppraisalModel.Include(e => e.AppraisalBusinessAppraisalModelObject).Where(e => e.Id == ModelSubId).FirstOrDefault();
                                    //if (item.ModelSubTypeList != null && item.ModelSubTypeList != "")
                                    //{
                                    //    var ids = Utility.StringIdsToListIds(item.ModelSubTypeList);
                                    //        foreach (var ca in ids)
                                    //        {
                                    //            var BusinessAppraisalModelObject_val = db.AppraisalBusinessAppraisalModelObject.Find(ca);
                                    //            CMS_BusinessAppraisalModelObjectList.Add(BusinessAppraisalModelObject_val);
                                    //            db_databusiness.AppraisalBusinessAppraisalModelObject = CMS_BusinessAppraisalModelObjectList;
                                    //        }
                                    //}
                                    db.AppraisalBusinessAppraisalModel.Attach(db_databusiness);
                                    db.Entry(db_databusiness).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    AppraisalBusinessAppraisalModel CMS_AppraisalBusinessAppraisalModel = db.AppraisalBusinessAppraisalModel.Find(ModelSubId);
                                    CMS_AppraisalBusinessAppraisalModel.Id = ModelSubId;
                                    CMS_AppraisalBusinessAppraisalModel.AppraisalBusinessAppraisalModelObject = db_databusiness.AppraisalBusinessAppraisalModelObject;
                                    db.AppraisalBusinessAppraisalModel.Attach(CMS_AppraisalBusinessAppraisalModel);
                                    db.Entry(CMS_AppraisalBusinessAppraisalModel).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    c.AppraisalBusinessApprisalModel = CMS_AppraisalBusinessAppraisalModel;
                                    break;

                                case "AppraisalKRAModel":

                                    List<AppraisalKRAModelObject> CMS_AppraisalKRAModelObjectList = new List<AppraisalKRAModelObject>();
                                    var db_datakra = db.AppraisalKRAModel.Include(e => e.AppraisalKRAModelObject).Where(e => e.Id == ModelSubId).SingleOrDefault();
                                    //if (item.ModelSubTypeList != null && item.ModelSubTypeList != "")
                                    //{

                                    //    var ids = Utility.StringIdsToListIds(item.ModelSubTypeList);
                                    //        foreach (var ca in ids)
                                    //        {
                                    //            var AppraisalKRAModelObject_val = db.AppraisalKRAModelObject.Find(ca);
                                    //            CMS_AppraisalKRAModelObjectList.Add(AppraisalKRAModelObject_val);
                                    //            db_datakra.AppraisalKRAModelObject = CMS_AppraisalKRAModelObjectList;
                                    //        }
                                    //}
                                    db.AppraisalKRAModel.Attach(db_datakra);
                                    db.Entry(db_datakra).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    AppraisalKRAModel CMS_AppraisalKRAModel = db.AppraisalKRAModel.Find(ModelSubId);
                                    CMS_AppraisalKRAModel.Id = ModelSubId;
                                    CMS_AppraisalKRAModel.AppraisalKRAModelObject = db_datakra.AppraisalKRAModelObject;
                                    db.AppraisalKRAModel.Attach(CMS_AppraisalKRAModel);
                                    db.Entry(CMS_AppraisalKRAModel).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    c.AppraisalKRAModel = CMS_AppraisalKRAModel;

                                    break;

                                case "PastExperienceModel":

                                    List<PastExperienceModelObject> CMS_PastExperienceModelObjectList = new List<PastExperienceModelObject>();
                                    var db_datapastexperience = db.PastExperienceModel.Include(e => e.PastExperienceModelObject).Where(e => e.Id == ModelSubId).FirstOrDefault();
                                    //if (item.ModelSubTypeList != null && item.ModelSubTypeList != "")
                                    //{
                                    //    var ids = Utility.StringIdsToListIds(item.ModelSubTypeList);
                                    //        foreach (var ca in ids)
                                    //        {
                                    //            var PastExperienceModelObject_val = db.PastExperienceModelObject.Find(ca);
                                    //            CMS_PastExperienceModelObjectList.Add(PastExperienceModelObject_val);
                                    //            db_datapastexperience.PastExperienceModelObject = CMS_PastExperienceModelObjectList;
                                    //        }
                                    //}
                                    db.PastExperienceModel.Attach(db_datapastexperience);
                                    db.Entry(db_datapastexperience).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    PastExperienceModel CMS_PastExperienceModel = db.PastExperienceModel.Find(ModelSubId);
                                    CMS_PastExperienceModel.Id = ModelSubId;
                                    CMS_PastExperienceModel.PastExperienceModelObject = db_datapastexperience.PastExperienceModelObject;
                                    db.PastExperienceModel.Attach(CMS_PastExperienceModel);
                                    db.Entry(CMS_PastExperienceModel).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    c.PastExperienceModel = CMS_PastExperienceModel;
                                    break;

                                case "PersonnelModel":

                                    List<PersonnelModelObject> CMS_PersonnelModelObjectList = new List<PersonnelModelObject>();
                                    var db_datapersonnel = db.PersonnelModel.Include(e => e.PersonnelModelObject).Where(e => e.Id == ModelSubId).FirstOrDefault();
                                    //if (item.ModelSubTypeList != null && item.ModelSubTypeList != "")
                                    //{
                                    //    var ids = Utility.StringIdsToListIds(item.ModelSubTypeList);
                                    //        foreach (var ca in ids)
                                    //        {
                                    //            var PersonnelModelObject_val = db.PersonnelModelObject.Find(ca);
                                    //            CMS_PersonnelModelObjectList.Add(PersonnelModelObject_val);
                                    //            db_datapersonnel.PersonnelModelObject = CMS_PersonnelModelObjectList;
                                    //        }
                                    //}
                                    db.PersonnelModel.Attach(db_datapersonnel);
                                    db.Entry(db_datapersonnel).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    PersonnelModel CMS_PersonnelModel = db.PersonnelModel.Find(ModelSubId);
                                    CMS_PersonnelModel.Id = ModelSubId;
                                    CMS_PersonnelModel.PersonnelModelObject = db_datapersonnel.PersonnelModelObject;
                                    db.PersonnelModel.Attach(CMS_PersonnelModel);
                                    db.Entry(CMS_PersonnelModel).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    c.PersonnelModel = CMS_PersonnelModel;

                                    break;

                                case "QualificationModel":
                                    List<QualificationModelObject> CMS_QualificationModelObjectList = new List<QualificationModelObject>();
                                    var db_dataqualification = db.QualificationModel.Include(e => e.QualificationModelObject).Where(e => e.Id == ModelSubId).FirstOrDefault();
                                    //if (item.ModelSubTypeList != null && item.ModelSubTypeList != "")
                                    //{
                                    //    var ids = Utility.StringIdsToListIds(item.ModelSubTypeList);
                                    //        foreach (var ca in ids)
                                    //        {
                                    //            var QualificationModelObject_val = db.QualificationModelObject.Find(ca);
                                    //            CMS_QualificationModelObjectList.Add(QualificationModelObject_val);
                                    //            db_dataqualification.QualificationModelObject = CMS_QualificationModelObjectList;
                                    //        }
                                    //}
                                    db.QualificationModel.Attach(db_dataqualification);
                                    db.Entry(db_dataqualification).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    QualificationModel CMS_QualificationModel = db.QualificationModel.Find(ModelSubId);
                                    CMS_QualificationModel.Id = ModelSubId;
                                    CMS_QualificationModel.QualificationModelObject = db_dataqualification.QualificationModelObject;
                                    db.QualificationModel.Attach(CMS_QualificationModel);
                                    db.Entry(CMS_QualificationModel).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    c.QualificationModel = CMS_QualificationModel;
                                    break;

                                case "ServiceModel":

                                    List<ServiceModelObject> CMS_ServiceModelObjectList = new List<ServiceModelObject>();
                                    var db_dataservice = db.ServiceModel.Include(e => e.ServiceModelObject).Where(e => e.Id == ModelSubId).FirstOrDefault();

                                    //if (item.ModelSubTypeList != null && item.ModelSubTypeList != "")
                                    //    {
                                    //        var ids = Utility.StringIdsToListIds(item.ModelSubTypeList);
                                    //        foreach (var ca in ids)
                                    //        {
                                    //            var ServiceModelObject_val = db.ServiceModelObject.Find(ca);
                                    //            CMS_ServiceModelObjectList.Add(ServiceModelObject_val);
                                    //            db_dataservice.ServiceModelObject = CMS_ServiceModelObjectList;
                                    //        }

                                    //    }
                                    
                                    db.ServiceModel.Attach(db_dataservice);
                                    db.Entry(db_dataservice).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    ServiceModel CMS_ServiceModel = db.ServiceModel.Find(ModelSubId);
                                    CMS_ServiceModel.Id = ModelSubId;
                                    CMS_ServiceModel.ServiceModelObject = db_dataservice.ServiceModelObject;
                                    db.ServiceModel.Attach(CMS_ServiceModel);
                                    db.Entry(CMS_ServiceModel).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    c.ServiceModel = CMS_ServiceModel;

                                    break;

                                case "SkillModel":

                                    List<SkillModelObject> CMS_SkillModelObjectList = new List<SkillModelObject>();
                                    var db_dataskill = db.SkillModel.Include(e => e.SkillModelObject).Where(e => e.Id == ModelSubId).FirstOrDefault();
                                    //if (item.ModelSubTypeList != null && item.ModelSubTypeList != "")
                                    //{

                                    //    var ids = Utility.StringIdsToListIds(item.ModelSubTypeList);
                                    //        foreach (var ca in ids)
                                    //        {
                                    //            var SkillModelObjectt_val = db.SkillModelObject.Find(ca);
                                    //            CMS_SkillModelObjectList.Add(SkillModelObjectt_val);
                                    //            db_dataskill.SkillModelObject = CMS_SkillModelObjectList;
                                    //        }
                                    //}
                                    db.SkillModel.Attach(db_dataskill);
                                    db.Entry(db_dataskill).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    SkillModel CMS_SkillModel = db.SkillModel.Find(ModelSubId);
                                    CMS_SkillModel.Id = ModelSubId;
                                    CMS_SkillModel.SkillModelObject = db_dataskill.SkillModelObject;
                                    db.SkillModel.Attach(CMS_SkillModel);
                                    db.Entry(CMS_SkillModel).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    c.SkillModel = CMS_SkillModel;

                                    break;

                                case "TrainingModel":

                                    List<TrainingModelObject> CMS_TrainingModelObjectList = new List<TrainingModelObject>();
                                    var db_datatraining = db.TrainingModel.Include(e => e.TrainingModelObject).Where(e => e.Id == ModelSubId).FirstOrDefault();
                                    //if (item.ModelSubTypeList != null && item.ModelSubTypeList != "")
                                    //{
                                    //    var ids = Utility.StringIdsToListIds(item.ModelSubTypeList);
                                    //        foreach (var ca in ids)
                                    //        {
                                    //            var TrainingModelObject_val = db.TrainingModelObject.Find(ca);
                                    //            CMS_TrainingModelObjectList.Add(TrainingModelObject_val);
                                    //            db_datatraining.TrainingModelObject = CMS_TrainingModelObjectList;
                                    //        }
                                    //}
                                    db.TrainingModel.Attach(db_datatraining);
                                    db.Entry(db_datatraining).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TrainingModel CMS_TrainingModel = db.TrainingModel.Find(ModelSubId);
                                    CMS_TrainingModel.Id = ModelSubId;
                                    CMS_TrainingModel.TrainingModelObject = db_datatraining.TrainingModelObject;
                                    db.TrainingModel.Attach(CMS_TrainingModel);
                                    db.Entry(CMS_TrainingModel).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    c.TrainingModel = CMS_TrainingModel;

                                    break;
                            }
                        }
  
                    }
                    
                    companycms_sps = db.CompanyCMS_SPS.Include(e => e.CompetencyModel).Where(e => e.Company.Id == company_Id).FirstOrDefault();
                    

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            CompetencyModel CompetencyModel = new CompetencyModel()
                            {

                                Code = c.Code,
                                ModelName = c.ModelName,
                                ModelDescription = c.ModelDescription,
                                CreatedDate = DateTime.Now.Date,
                                AppraisalAttributeModel = c.AppraisalAttributeModel,
                                AppraisalBusinessApprisalModel = c.AppraisalBusinessApprisalModel,
                                AppraisalKRAModel = c.AppraisalKRAModel,
                                PastExperienceModel = c.PastExperienceModel,
                                PersonnelModel = c.PersonnelModel,
                                QualificationModel = c.QualificationModel,
                                ServiceModel = c.ServiceModel,
                                SkillModel = c.SkillModel,
                                TrainingModel = c.TrainingModel,
                                DBTrack = c.DBTrack
                            };

                            db.CompetencyModel.Add(CompetencyModel);
                            db.SaveChanges();
                            if (companycms_sps != null)
                            {
                                List<CompetencyModel> competencymodel_list = new List<CompetencyModel>();
                                competencymodel_list.Add(CompetencyModel);
                                if (companycms_sps.CompetencyModel != null)
                                {
                                    competencymodel_list.AddRange(companycms_sps.CompetencyModel);
                                }
                                companycms_sps.CompetencyModel = competencymodel_list;
                                db.CompanyCMS_SPS.Attach(companycms_sps);
                                db.Entry(companycms_sps).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(companycms_sps).State = System.Data.Entity.EntityState.Detached;

                            }
                            try
                            {

                                ts.Complete();
                                Session["ModelData"] = "";
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

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                var returndata =db.CompetencyModel
                     .Where(e => e.Id == data).FirstOrDefault();

                
                    result.Add(new returndatagridDataclass
                    {
                        Id = returndata.Id.ToString(),
                        Code = returndata.Code,
                        ModelName = returndata.ModelName,
                        ModelDescription = returndata.ModelDescription,
                        CreatedDate = returndata.CreatedDate.Value.ToShortDateString()

                    });
                
                //var return_data = db.PersonnelModel.Include(e => e.PersonnelModelObject)
                //    .Include(e => e.PersonnelModelObject.Select(r => r.PersonnelModel))
                //      .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel))
                //        .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
                //          .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
                //            .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps))
                //    .Where(e => e.Id == data && e.PersonnelModelObject.Count > 0).SingleOrDefault();

                //List<returnDataClass> returndatases = new List<returnDataClass>();
                //if (return_data != null && return_data.PersonnelModelObject != null)
                //{
                //    foreach (var item in return_data.PersonnelModelObject)
                //    {
                //        returndatases.Add(new returnDataClass
                //        {
                //            Id = item.Id,
                //            PersonnelModel = item.PersonnelModel != null ? item.PersonnelModel.LookupVal : "",
                //            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
                //            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
                //            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
                //        });
                //    }
                //}
                //List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                //if (returndatases != null)
                //{

                //    foreach (var item1 in returndatases)
                //    {
                //        oreturnEditClass.Add(new returnEditClass
                //        {
                //            PersonnelModelObject_Id = item1.Id.ToString(),
                //            PersonnelModelObject_FullDetails = item1.PersonnelModel + " " + item1.Creteriases + " " + item1.CreteriaTypeses + "" + item1.DataStepses
                //        });
                //    }
                //}

                    return Json(new Object[] { result, "", JsonRequestBehavior.AllowGet });
            }
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/CMS/_CompetencyModelGridPartial.cshtml");

        }

        public class Model_CD
        {
            public int Object_Id { get; set; }
            public string Object_FullDetails { get; set; }
        }

        //public JsonResult EditGridDetails(int data, string data2)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        string ModelName = data2.Split('-')[0];
        //        TempData["ModelName"] = ModelName;
        //        int ModelId =  Convert.ToInt32(data2.Split('-')[1]);
        //        TempData["ModelId"] = ModelId;
        //        List<Model_CD> return_data = new List<Model_CD>();

        //        switch (ModelName)
        //        {
        //            case "AppraisalAttributeModel":
                       
        //                var ObjAttributeModel = db.AppraisalAttributeModel
        //                    .Include(e => e.AppraisalAttributeModelObject)
        //                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.AppraisalAttributeModel))
        //                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel))
        //                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
        //                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
        //                    .Include(e => e.AppraisalAttributeModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == ModelId).SingleOrDefault();
        //                if (ObjAttributeModel.AppraisalAttributeModelObject != null)
        //                {
        //                    List<returnDataClass1> returndata = new List<returnDataClass1>();
        //                    foreach (var item in ObjAttributeModel.AppraisalAttributeModelObject)
        //                    {
        //                        returndata.Add(new returnDataClass1
        //                        {
        //                            Id = item.Id,
        //                            AttributeModel = item.AppraisalAttributeModel != null ? db.AppCategory.Where(e => e.Id == item.AppraisalAttributeModel.Id).Select(r => r.FullDetails).FirstOrDefault() : " ",
        //                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
        //                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
        //                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
        //                        });
        //                    }
        //                    List<returnEditDataClass1> returndatases = new List<returnEditDataClass1>();
        //                    foreach (var item1 in returndata)
        //                    {
        //                        returndatases.Add(new returnEditDataClass1
        //                        {
        //                            Id = item1.Id,
        //                            AttributeModel = item1.AttributeModel,
        //                            EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

        //                        });
        //                    }
        //                    var res = (from ca in returndatases
        //                               select new
        //                               {
        //                                   srno = ca.Id,
        //                                   lookupvalue = ca.AttributeModel + ca.EvaluationModel
        //                               }).ToList();

        //                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

        //                    return Json(result, JsonRequestBehavior.AllowGet);
        //                }

        //                break;

        //            case "AppraisalBusinessAppraisalModel":
        //                var ObjBusinessModel = db.AppraisalBusinessAppraisalModel
        //                    .Include(e => e.AppraisalBusinessAppraisalModelObject)
        //                    .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.AppraisalBusinessAppraisalModel))
        //                    .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel))
        //                    .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
        //                    .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
        //                    .Include(e => e.AppraisalBusinessAppraisalModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == ModelId).SingleOrDefault();
        //                if (ObjBusinessModel.AppraisalBusinessAppraisalModelObject != null)
        //                {
        //                    List<returnDataClass2> returndata = new List<returnDataClass2>();
        //                    foreach (var item in ObjBusinessModel.AppraisalBusinessAppraisalModelObject)
        //                    {
        //                        returndata.Add(new returnDataClass2
        //                        {
        //                            Id = item.Id,
        //                            BusinessModel = item.AppraisalBusinessAppraisalModel != null ? db.BA_Category.Where(e => e.Id == item.AppraisalBusinessAppraisalModel.Id).Select(r => r.FullDetails).FirstOrDefault() : " ",
        //                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
        //                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
        //                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
        //                        });
        //                    }
        //                    List<returnEditDataClass2> returndatases = new List<returnEditDataClass2>();
        //                    foreach (var item1 in returndata)
        //                    {
        //                        returndatases.Add(new returnEditDataClass2
        //                        {
        //                            Id = item1.Id,
        //                            BusinessModel = item1.BusinessModel,
        //                            EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

        //                        });
        //                    }
        //                    var res = (from ca in returndatases
        //                               select new
        //                               {
        //                                   srno = ca.Id,
        //                                   lookupvalue = ca.BusinessModel + ca.EvaluationModel
        //                               }).ToList();

        //                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

        //                    return Json(result, JsonRequestBehavior.AllowGet);
        //                }

        //                break;

        //            case "AppraisalKRAModel":
        //                var ObjKRAModel = db.AppraisalKRAModel
        //                    .Include(e => e.AppraisalKRAModelObject)
        //                    .Include(e => e.AppraisalKRAModelObject.Select(r => r.AppraisalKRAModel))
        //                    .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel))
        //                    .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
        //                    .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
        //                    .Include(e => e.AppraisalKRAModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == ModelId).SingleOrDefault();
        //                if (ObjKRAModel.AppraisalKRAModelObject != null)
        //                {
        //                    List<returnDataClass3> returndata = new List<returnDataClass3>();
        //                    foreach (var item in ObjKRAModel.AppraisalKRAModelObject)
        //                    {
        //                        returndata.Add(new returnDataClass3
        //                        {
        //                            Id = item.Id,
        //                            KRAModel = item.AppraisalKRAModel != null ? db.AppCategory.Where(e => e.Id == item.AppraisalKRAModel.Id).Select(r => r.FullDetails).FirstOrDefault() : " ",
        //                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
        //                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
        //                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
        //                        });
        //                    }
        //                    List<returnEditDataClass3> returndatases = new List<returnEditDataClass3>();
        //                    foreach (var item1 in returndata)
        //                    {
        //                        returndatases.Add(new returnEditDataClass3
        //                        {
        //                            Id = item1.Id,
        //                            KRAModel = item1.KRAModel,
        //                            EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

        //                        });
        //                    }
        //                    var res = (from ca in returndatases
        //                               select new
        //                               {
        //                                   srno = ca.Id,
        //                                   lookupvalue = ca.KRAModel + ca.EvaluationModel
        //                               }).ToList();

        //                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

        //                    return Json(result, JsonRequestBehavior.AllowGet);
        //                }

        //                break;

        //            case "AppraisalPotentialModel":

        //                var ObjPotentialmodel = db.AppraisalPotentialModel
        //                    .Include(e => e.AppraisalPotentialModelObject)
        //                    .Include(e => e.AppraisalPotentialModelObject.Select(r => r.AppraisalPotentialModel))
        //                    .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel))
        //                    .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
        //                    .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
        //                    .Include(e => e.AppraisalPotentialModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == ModelId).SingleOrDefault();
        //                if (ObjPotentialmodel.AppraisalPotentialModelObject != null)
        //                {
        //                    List<returnDataClass4> returndata = new List<returnDataClass4>();
        //                    foreach (var item in ObjPotentialmodel.AppraisalPotentialModelObject)
        //                    {
        //                        returndata.Add(new returnDataClass4
        //                        {
        //                            Id = item.Id,
        //                            PotentialModel = item.AppraisalPotentialModel != null ? db.AppCategory.Where(e => e.Id == item.AppraisalPotentialModel.Id).Select(r => r.FullDetails).FirstOrDefault() : " ",
        //                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
        //                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
        //                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
        //                        });
        //                    }
        //                    List<returnEditDataClass4> returndatases = new List<returnEditDataClass4>();
        //                    foreach (var item1 in returndata)
        //                    {
        //                        returndatases.Add(new returnEditDataClass4
        //                        {
        //                            Id = item1.Id,
        //                            PotentialModel = item1.PotentialModel,
        //                            EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

        //                        });
        //                    }
        //                    var res = (from ca in returndatases
        //                               select new
        //                               {
        //                                   srno = ca.Id,
        //                                   lookupvalue = ca.PotentialModel + ca.EvaluationModel
        //                               }).ToList();

        //                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

        //                    return Json(result, JsonRequestBehavior.AllowGet);
        //                }
        //                break;

        //            case "ServiceModel":

        //                var ObjServicemodel = db.ServiceModel
        //                    .Include(e => e.ServiceModelObject)
        //                    .Include(e => e.ServiceModelObject.Select(r => r.ServiceModel))
        //                    .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel))
        //                    .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
        //                    .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
        //                    .Include(e => e.ServiceModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == ModelId).SingleOrDefault();
        //                if (ObjServicemodel.ServiceModelObject != null)
        //                {
        //                    List<returnDataClass5> returndata = new List<returnDataClass5>();
        //                    foreach (var item in ObjServicemodel.ServiceModelObject)
        //                    {
        //                        returndata.Add(new returnDataClass5
        //                        {
        //                            Id = item.Id,
        //                            ServiceModel = item.ServiceModel != null ? item.ServiceModel.LookupVal : "",
        //                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
        //                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
        //                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
        //                        });
        //                    }
        //                    List<returnEditDataClass5> returndatases = new List<returnEditDataClass5>();
        //                    foreach (var item1 in returndata)
        //                    {
        //                        returndatases.Add(new returnEditDataClass5
        //                        {
        //                            Id = item1.Id,
        //                            ServiceModel = item1.ServiceModel,
        //                            EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

        //                        });
        //                    }
        //                    var res = (from ca in returndatases
        //                               select new
        //                               {
        //                                   srno = ca.Id,
        //                                   lookupvalue = ca.ServiceModel + ca.EvaluationModel
        //                               }).ToList();

        //                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

        //                    return Json(result, JsonRequestBehavior.AllowGet);
        //                }
        //                break;

        //            case "QualificationModel":
                        
        //                var ObjQualificationModel = db.QualificationModel
        //                    .Include(e => e.QualificationModelObject)
        //                    .Include(e => e.QualificationModelObject.Select(r => r.QualificationModel))
        //                    .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel))
        //                    .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
        //                    .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
        //                    .Include(e => e.QualificationModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == ModelId).SingleOrDefault();
        //                if (ObjQualificationModel.QualificationModelObject != null)
        //                {
        //                    List<returnDataClass6> returndata = new List<returnDataClass6>();
        //                    foreach (var item in ObjQualificationModel.QualificationModelObject)
        //                    {
        //                        returndata.Add(new returnDataClass6
        //                        {
        //                            Id = item.Id,
        //                            QualificationModel = item.QualificationModel != null ? item.QualificationModel.LookupVal : "",
        //                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
        //                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
        //                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
        //                        });
        //                    }
        //                    List<returnEditDataClass6> returndatases = new List<returnEditDataClass6>();
        //                    foreach (var item1 in returndata)
        //                    {
        //                        returndatases.Add(new returnEditDataClass6
        //                        {
        //                            Id = item1.Id,
        //                            QualificationModel = item1.QualificationModel,
        //                            EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

        //                        });
        //                    }
        //                    var res = (from ca in returndatases
        //                               select new
        //                               {
        //                                   srno = ca.Id,
        //                                   lookupvalue = ca.QualificationModel + ca.EvaluationModel
        //                               }).ToList();
        //                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();
        //                    return Json(result, JsonRequestBehavior.AllowGet);

        //                }
        //                break;

        //            case "SkillModel":
                        
        //                var ObjSkillModel = db.SkillModel
        //                    .Include(e => e.SkillModelObject)
        //                    .Include(e => e.SkillModelObject.Select(r => r.SkillModel))
        //                    .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel))
        //                    .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
        //                    .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
        //                    .Include(e => e.SkillModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == ModelId).SingleOrDefault();
        //                if (ObjSkillModel.SkillModelObject != null)
        //                {
        //                    List<returnDataClass7> returndata = new List<returnDataClass7>();
        //                    foreach (var item in ObjSkillModel.SkillModelObject)
        //                    {
        //                        returndata.Add(new returnDataClass7
        //                        {
        //                            Id = item.Id,
        //                            SkillModel = item.SkillModel != null ? item.SkillModel.LookupVal : "",
        //                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
        //                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
        //                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
        //                        });
        //                    }
        //                    List<returnEditDataClass7> returndatases = new List<returnEditDataClass7>();
        //                    foreach (var item1 in returndata)
        //                    {
        //                        returndatases.Add(new returnEditDataClass7
        //                        {
        //                            Id = item1.Id,
        //                            SkillModel = item1.SkillModel,
        //                            EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

        //                        });
        //                    }
        //                    var res = (from ca in returndatases
        //                               select new
        //                               {
        //                                   srno = ca.Id,
        //                                   lookupvalue = ca.SkillModel + ca.EvaluationModel
        //                               }).ToList();
        //                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();
        //                    return Json(result, JsonRequestBehavior.AllowGet);

        //                }

        //                break;

        //            case "PastExperienceModel":
        //                var ObjPastExperienceModel = db.PastExperienceModel
        //                    .Include(e => e.PastExperienceModelObject)
        //                    .Include(e => e.PastExperienceModelObject.Select(r => r.PastExperienceModel))
        //                    .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel))
        //                    .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
        //                    .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
        //                    .Include(e => e.PastExperienceModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == ModelId).SingleOrDefault();
        //                if (ObjPastExperienceModel.PastExperienceModelObject != null)
        //                {
        //                    List<returnDataClass8> returndata = new List<returnDataClass8>();
        //                    foreach (var item in ObjPastExperienceModel.PastExperienceModelObject)
        //                    {
        //                        returndata.Add(new returnDataClass8
        //                        {
        //                            Id = item.Id,
        //                            PastExperienceModel = item.PastExperienceModel != null ? item.PastExperienceModel.LookupVal : "",
        //                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
        //                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
        //                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
        //                        });
        //                    }
        //                    List<returnEditDataClass8> returndatases = new List<returnEditDataClass8>();
        //                    foreach (var item1 in returndata)
        //                    {
        //                        returndatases.Add(new returnEditDataClass8
        //                        {
        //                            Id = item1.Id,
        //                            PastExperienceModel = item1.PastExperienceModel,
        //                            EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

        //                        });
        //                    }
        //                    var res = (from ca in returndatases
        //                               select new
        //                               {
        //                                   srno = ca.Id,
        //                                   lookupvalue = ca.PastExperienceModel + ca.EvaluationModel
        //                               }).ToList();
        //                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();
        //                    return Json(result, JsonRequestBehavior.AllowGet);

        //                }
        //                break;

        //            case "PersonnelModel":
        //                var ObjPersonnelModel = db.PersonnelModel
        //                   .Include(e => e.PersonnelModelObject)
        //                   .Include(e => e.PersonnelModelObject.Select(r => r.PersonnelModel))
        //                   .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel))
        //                   .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
        //                   .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
        //                   .Include(e => e.PersonnelModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == ModelId).SingleOrDefault();
        //                if (ObjPersonnelModel.PersonnelModelObject != null)
        //                {
        //                    List<returnDataClass9> returndata = new List<returnDataClass9>();
        //                    foreach (var item in ObjPersonnelModel.PersonnelModelObject)
        //                    {
        //                        returndata.Add(new returnDataClass9
        //                        {
        //                            Id = item.Id,
        //                            PersonnelModel = item.PersonnelModel != null ? item.PersonnelModel.LookupVal : "",
        //                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
        //                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
        //                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
        //                        });
        //                    }
        //                    List<returnEditDataClass9> returndatases = new List<returnEditDataClass9>();
        //                    foreach (var item1 in returndata)
        //                    {
        //                        returndatases.Add(new returnEditDataClass9
        //                        {
        //                            Id = item1.Id,
        //                            PersonnelModel = item1.PersonnelModel,
        //                            EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

        //                        });
        //                    }
        //                    var res = (from ca in returndatases
        //                               select new
        //                               {
        //                                   srno = ca.Id,
        //                                   lookupvalue = ca.PersonnelModel + ca.EvaluationModel
        //                               }).ToList();
        //                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();
        //                    return Json(result, JsonRequestBehavior.AllowGet);

        //                }

        //                break;

        //            case "TrainingModel":
        //                var ObjTrainingModel = db.TrainingModel
        //                    .Include(e => e.TrainingModelObject)
        //                    .Include(e => e.TrainingModelObject.Select(r => r.TrainingModel))
        //                    .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel))
        //                    .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.Criteria))
        //                    .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.CriteriaType))
        //                    .Include(e => e.TrainingModelObject.Select(r => r.CompetencyEvaluationModel.DataSteps)).Where(e => e.Id == ModelId).SingleOrDefault();
        //                if (ObjTrainingModel.TrainingModelObject != null)
        //                {
        //                    List<returnDataClass10> returndata = new List<returnDataClass10>();
        //                    foreach (var item in ObjTrainingModel.TrainingModelObject)
        //                    {
        //                        returndata.Add(new returnDataClass10
        //                        {
        //                            Id = item.Id,
        //                            TrainingModel = item.TrainingModel != null ? db.ProgramList.Where(e => e.Id == item.TrainingModel.Id).Select(r => r.FullDetails).FirstOrDefault() : " ",
        //                            DataStepses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.DataSteps != null ? item.CompetencyEvaluationModel.DataSteps.LookupVal : "",
        //                            Creteriases = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.Criteria != null ? item.CompetencyEvaluationModel.Criteria.LookupVal : "",
        //                            CreteriaTypeses = item.CompetencyEvaluationModel != null && item.CompetencyEvaluationModel.CriteriaType != null ? item.CompetencyEvaluationModel.CriteriaType.LookupVal : "",
        //                        });
        //                    }
        //                    List<returnEditDataClass10> returndatases = new List<returnEditDataClass10>();
        //                    foreach (var item1 in returndata)
        //                    {
        //                        returndatases.Add(new returnEditDataClass10
        //                        {
        //                            Id = item1.Id,
        //                            TrainingModel = item1.TrainingModel,
        //                            EvaluationModel = item1.Creteriases + " " + item1.CreteriaTypeses + " " + item1.DataStepses

        //                        });
        //                    }
        //                    var res = (from ca in returndatases
        //                               select new
        //                               {
        //                                   srno = ca.Id,
        //                                   lookupvalue = ca.TrainingModel + ca.EvaluationModel
        //                               }).ToList();

        //                    var result = res.GroupBy(x => x.lookupvalue).Select(y => y.First()).ToList();

        //                    return Json(result, JsonRequestBehavior.AllowGet);

        //                }
        //                break;
        //        }
        //        return Json(return_data, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //public ActionResult GridEditSave(CompetencyModel ypay, FormCollection form, string data, string data2)
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            string ModelName = data2.Split('-')[0];
        //            int ModelId = Convert.ToInt32(data2.Split('-')[1]);
        //            var ModelObjectlist = form["ModelObjectListE"] == " 0" ? "" : form["ModelObjectListE"];
                   
        //            if (data != null)
        //            {
        //                int ModelObjectId = int.Parse(data);
        //                switch (ModelName)
        //                {
        //                    case "AppraisalAttributeModel":
        //                        if (ModelObjectlist != null && ModelObjectlist != "")
        //                        {
        //                            List<AppraisalAttributeModelObject> CMS_AttributeModelObjectList = new List<AppraisalAttributeModelObject>();
        //                            var db_dataattribute = db.AppraisalAttributeModel.Include(e => e.AppraisalAttributeModelObject).Where(e => e.Id == ModelId).FirstOrDefault();

        //                            var ids = Utility.StringIdsToListIds(ModelObjectlist);
        //                            foreach (var ca in ids)
        //                            {
        //                                var AttributeModelObject_val = db.AppraisalAttributeModelObject.Find(ca);
        //                                CMS_AttributeModelObjectList.Add(AttributeModelObject_val);
        //                                db_dataattribute.AppraisalAttributeModelObject = CMS_AttributeModelObjectList;
        //                            }
        //                            db.AppraisalAttributeModel.Attach(db_dataattribute);
        //                            db.Entry(db_dataattribute).State = System.Data.Entity.EntityState.Modified;
        //                            db.SaveChanges();
        //                            AppraisalAttributeModel CMS_AppraisalAttributeModel = db.AppraisalAttributeModel.Find(ModelId);
        //                            CMS_AppraisalAttributeModel.Id = ModelId;
        //                            CMS_AppraisalAttributeModel.AppraisalAttributeModelObject = db_dataattribute.AppraisalAttributeModelObject;
        //                            db.AppraisalAttributeModel.Attach(CMS_AppraisalAttributeModel);
        //                            db.Entry(CMS_AppraisalAttributeModel).State = System.Data.Entity.EntityState.Modified;
        //                            db.SaveChanges();
                                    
        //                        }
        //                        break;

        //                    case "AppraisalBusinessAppraisalModel":

        //                        List<AppraisalBusinessAppraisalModelObject> CMS_BusinessAppraisalModelObjectList = new List<AppraisalBusinessAppraisalModelObject>();
        //                        var db_databusiness = db.AppraisalBusinessAppraisalModel.Include(e => e.AppraisalBusinessAppraisalModelObject).Where(e => e.Id == ModelId).FirstOrDefault();
        //                        if (ModelObjectlist != null && ModelObjectlist != "")
        //                        {
        //                            var ids = Utility.StringIdsToListIds(ModelObjectlist);
        //                            foreach (var ca in ids)
        //                            {
        //                                var BusinessAppraisalModelObject_val = db.AppraisalBusinessAppraisalModelObject.Find(ca);
        //                                CMS_BusinessAppraisalModelObjectList.Add(BusinessAppraisalModelObject_val);
        //                                db_databusiness.AppraisalBusinessAppraisalModelObject = CMS_BusinessAppraisalModelObjectList;
        //                            }
        //                        }
        //                        db.AppraisalBusinessAppraisalModel.Attach(db_databusiness);
        //                        db.Entry(db_databusiness).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        AppraisalBusinessAppraisalModel CMS_AppraisalBusinessAppraisalModel = db.AppraisalBusinessAppraisalModel.Find(ModelId);
        //                        CMS_AppraisalBusinessAppraisalModel.Id = ModelId;
        //                        CMS_AppraisalBusinessAppraisalModel.AppraisalBusinessAppraisalModelObject = db_databusiness.AppraisalBusinessAppraisalModelObject;
        //                        db.AppraisalBusinessAppraisalModel.Attach(CMS_AppraisalBusinessAppraisalModel);
        //                        db.Entry(CMS_AppraisalBusinessAppraisalModel).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        //c.AppraisalBusinessApprisalModel = CMS_AppraisalBusinessAppraisalModel;
        //                        break;

        //                    case "AppraisalKRAModel":

        //                        List<AppraisalKRAModelObject> CMS_AppraisalKRAModelObjectList = new List<AppraisalKRAModelObject>();
        //                        var db_datakra = db.AppraisalKRAModel.Include(e => e.AppraisalKRAModelObject).Where(e => e.Id == ModelId).SingleOrDefault();
        //                        if (ModelObjectlist != null && ModelObjectlist != "")
        //                        {

        //                            var ids = Utility.StringIdsToListIds(ModelObjectlist);
        //                            foreach (var ca in ids)
        //                            {
        //                                var AppraisalKRAModelObject_val = db.AppraisalKRAModelObject.Find(ca);
        //                                CMS_AppraisalKRAModelObjectList.Add(AppraisalKRAModelObject_val);
        //                                db_datakra.AppraisalKRAModelObject = CMS_AppraisalKRAModelObjectList;
        //                            }
        //                        }
        //                        db.AppraisalKRAModel.Attach(db_datakra);
        //                        db.Entry(db_datakra).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        AppraisalKRAModel CMS_AppraisalKRAModel = db.AppraisalKRAModel.Find(ModelId);
        //                        CMS_AppraisalKRAModel.Id = ModelId;
        //                        CMS_AppraisalKRAModel.AppraisalKRAModelObject = db_datakra.AppraisalKRAModelObject;
        //                        db.AppraisalKRAModel.Attach(CMS_AppraisalKRAModel);
        //                        db.Entry(CMS_AppraisalKRAModel).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                       // c.AppraisalKRAModel = CMS_AppraisalKRAModel;

        //                        break;

        //                    case "PastExperienceModel":

        //                        List<PastExperienceModelObject> CMS_PastExperienceModelObjectList = new List<PastExperienceModelObject>();
        //                        var db_datapastexperience = db.PastExperienceModel.Include(e => e.PastExperienceModelObject).Where(e => e.Id == ModelId).FirstOrDefault();
        //                        if (ModelObjectlist != null && ModelObjectlist != "")
        //                        {
        //                            var ids = Utility.StringIdsToListIds(ModelObjectlist);
        //                            foreach (var ca in ids)
        //                            {
        //                                var PastExperienceModelObject_val = db.PastExperienceModelObject.Find(ca);
        //                                CMS_PastExperienceModelObjectList.Add(PastExperienceModelObject_val);
        //                                db_datapastexperience.PastExperienceModelObject = CMS_PastExperienceModelObjectList;
        //                            }
        //                        }
        //                        db.PastExperienceModel.Attach(db_datapastexperience);
        //                        db.Entry(db_datapastexperience).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        PastExperienceModel CMS_PastExperienceModel = db.PastExperienceModel.Find(ModelId);
        //                        CMS_PastExperienceModel.Id = ModelId;
        //                        CMS_PastExperienceModel.PastExperienceModelObject = db_datapastexperience.PastExperienceModelObject;
        //                        db.PastExperienceModel.Attach(CMS_PastExperienceModel);
        //                        db.Entry(CMS_PastExperienceModel).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
                               
        //                        break;

        //                    case "PersonnelModel":

        //                        List<PersonnelModelObject> CMS_PersonnelModelObjectList = new List<PersonnelModelObject>();
        //                        var db_datapersonnel = db.PersonnelModel.Include(e => e.PersonnelModelObject).Where(e => e.Id == ModelId).FirstOrDefault();
        //                        if (ModelObjectlist != null && ModelObjectlist != "")
        //                        {
        //                            var ids = Utility.StringIdsToListIds(ModelObjectlist);
        //                            foreach (var ca in ids)
        //                            {
        //                                var PersonnelModelObject_val = db.PersonnelModelObject.Find(ca);
        //                                CMS_PersonnelModelObjectList.Add(PersonnelModelObject_val);
        //                                db_datapersonnel.PersonnelModelObject = CMS_PersonnelModelObjectList;
        //                            }
        //                        }
        //                        db.PersonnelModel.Attach(db_datapersonnel);
        //                        db.Entry(db_datapersonnel).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        PersonnelModel CMS_PersonnelModel = db.PersonnelModel.Find(ModelId);
        //                        CMS_PersonnelModel.Id = ModelId;
        //                        CMS_PersonnelModel.PersonnelModelObject = db_datapersonnel.PersonnelModelObject;
        //                        db.PersonnelModel.Attach(CMS_PersonnelModel);
        //                        db.Entry(CMS_PersonnelModel).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
                               

        //                        break;

        //                    case "QualificationModel":
        //                        List<QualificationModelObject> CMS_QualificationModelObjectList = new List<QualificationModelObject>();
        //                        var db_dataqualification = db.QualificationModel.Include(e => e.QualificationModelObject).Where(e => e.Id == ModelId).FirstOrDefault();
        //                        if (ModelObjectlist != null && ModelObjectlist != "")
        //                        {
        //                            var ids = Utility.StringIdsToListIds(ModelObjectlist);
        //                            foreach (var ca in ids)
        //                            {
        //                                var QualificationModelObject_val = db.QualificationModelObject.Find(ca);
        //                                CMS_QualificationModelObjectList.Add(QualificationModelObject_val);
        //                                db_dataqualification.QualificationModelObject = CMS_QualificationModelObjectList;
        //                            }
        //                        }
        //                        db.QualificationModel.Attach(db_dataqualification);
        //                        db.Entry(db_dataqualification).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        QualificationModel CMS_QualificationModel = db.QualificationModel.Find(ModelId);
        //                        CMS_QualificationModel.Id = ModelId;
        //                        CMS_QualificationModel.QualificationModelObject = db_dataqualification.QualificationModelObject;
        //                        db.QualificationModel.Attach(CMS_QualificationModel);
        //                        db.Entry(CMS_QualificationModel).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
                               
        //                        break;

        //                    case "ServiceModel":

        //                        List<ServiceModelObject> CMS_ServiceModelObjectList = new List<ServiceModelObject>();
        //                        var db_dataservice = db.ServiceModel.Include(e => e.ServiceModelObject).Where(e => e.Id == ModelId).FirstOrDefault();

        //                        if (ModelObjectlist != null && ModelObjectlist != "")
        //                        {
        //                            var ids = Utility.StringIdsToListIds(ModelObjectlist);
        //                            foreach (var ca in ids)
        //                            {
        //                                var ServiceModelObject_val = db.ServiceModelObject.Find(ca);
        //                                CMS_ServiceModelObjectList.Add(ServiceModelObject_val);
        //                                db_dataservice.ServiceModelObject = CMS_ServiceModelObjectList;
        //                            }

        //                        }

        //                        db.ServiceModel.Attach(db_dataservice);
        //                        db.Entry(db_dataservice).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        ServiceModel CMS_ServiceModel = db.ServiceModel.Find(ModelId);
        //                        CMS_ServiceModel.Id = ModelId;
        //                        CMS_ServiceModel.ServiceModelObject = db_dataservice.ServiceModelObject;
        //                        db.ServiceModel.Attach(CMS_ServiceModel);
        //                        db.Entry(CMS_ServiceModel).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        //c.ServiceModel = CMS_ServiceModel;

        //                        break;

        //                    case "SkillModel":

        //                        List<SkillModelObject> CMS_SkillModelObjectList = new List<SkillModelObject>();
        //                        var db_dataskill = db.SkillModel.Include(e => e.SkillModelObject).Where(e => e.Id == ModelId).FirstOrDefault();
        //                        if (ModelObjectlist != null && ModelObjectlist != "")
        //                        {

        //                            var ids = Utility.StringIdsToListIds(ModelObjectlist);
        //                            foreach (var ca in ids)
        //                            {
        //                                var SkillModelObjectt_val = db.SkillModelObject.Find(ca);
        //                                CMS_SkillModelObjectList.Add(SkillModelObjectt_val);
        //                                db_dataskill.SkillModelObject = CMS_SkillModelObjectList;
        //                            }
        //                        }
        //                        db.SkillModel.Attach(db_dataskill);
        //                        db.Entry(db_dataskill).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        SkillModel CMS_SkillModel = db.SkillModel.Find(ModelId);
        //                        CMS_SkillModel.Id = ModelId;
        //                        CMS_SkillModel.SkillModelObject = db_dataskill.SkillModelObject;
        //                        db.SkillModel.Attach(CMS_SkillModel);
        //                        db.Entry(CMS_SkillModel).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        //c.SkillModel = CMS_SkillModel;

        //                        break;

        //                    case "TrainingModel":

        //                        List<TrainingModelObject> CMS_TrainingModelObjectList = new List<TrainingModelObject>();
        //                        var db_datatraining = db.TrainingModel.Include(e => e.TrainingModelObject).Where(e => e.Id == ModelId).FirstOrDefault();
        //                        if (ModelObjectlist != null && ModelObjectlist != "")
        //                        {
        //                            var ids = Utility.StringIdsToListIds(ModelObjectlist);
        //                            foreach (var ca in ids)
        //                            {
        //                                var TrainingModelObject_val = db.TrainingModelObject.Find(ca);
        //                                CMS_TrainingModelObjectList.Add(TrainingModelObject_val);
        //                                db_datatraining.TrainingModelObject = CMS_TrainingModelObjectList;
        //                            }
        //                        }
        //                        db.TrainingModel.Attach(db_datatraining);
        //                        db.Entry(db_datatraining).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        TrainingModel CMS_TrainingModel = db.TrainingModel.Find(ModelId);
        //                        CMS_TrainingModel.Id = ModelId;
        //                        CMS_TrainingModel.TrainingModelObject = db_datatraining.TrainingModelObject;
        //                        db.TrainingModel.Attach(CMS_TrainingModel);
        //                        db.Entry(CMS_TrainingModel).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                      //  c.TrainingModel = CMS_TrainingModel;

        //                        break;
        //                }
        //                return this.Json(new { status = true, responseText = "Record Updated Successfully.", JsonRequestBehavior.AllowGet });
                      
        //            }
        //            else
        //            {
        //                return this.Json(new { status = false, responseText = "  Data Is Null", JsonRequestBehavior.AllowGet });
        //                //     Msg.Add("  Data Is Null  ");
        //                //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return Json(new { responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        public ActionResult GridDelete(int data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                string ModelName = data2.Split('-')[1];
                int CompModelId = Convert.ToInt32(data2.Split('-')[0]);
                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    switch (ModelName)
                    {
                        case "AppraisalAttributeModel":
                            var db_dataattribute = db.CompetencyModel.Include(e => e.AppraisalAttributeModel).Where(e => e.Id == CompModelId).FirstOrDefault();

                            if (db_dataattribute != null)
                            {
                                db_dataattribute.AppraisalAttributeModel = null;
                                db.CompetencyModel.Attach(db_dataattribute);
                                db.Entry(db_dataattribute).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(db_dataattribute).State = System.Data.Entity.EntityState.Detached;
                            }
                            break;
                        case "AppraisalBusinessApprisalModel":
                            var db_databusiness = db.CompetencyModel.Include(e => e.AppraisalBusinessApprisalModel).Where(e => e.Id == CompModelId).FirstOrDefault();

                            if (db_databusiness != null)
                            {
                                db_databusiness.AppraisalBusinessApprisalModel = null;
                                db.CompetencyModel.Attach(db_databusiness);
                                db.Entry(db_databusiness).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(db_databusiness).State = System.Data.Entity.EntityState.Detached;
                            }
                            break;
                        case "AppraisalKRAModel":
                            var db_dataKRA = db.CompetencyModel.Include(e => e.AppraisalKRAModel).Where(e => e.Id == CompModelId).FirstOrDefault();

                            if (db_dataKRA != null)
                            {
                                db_dataKRA.AppraisalKRAModel = null;
                                db.CompetencyModel.Attach(db_dataKRA);
                                db.Entry(db_dataKRA).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(db_dataKRA).State = System.Data.Entity.EntityState.Detached;
                            }
                            break;
                        case "AppraisalPotentialModel":
                            var db_datapotential = db.CompetencyModel.Include(e => e.AppraisalPotentialModel).Where(e => e.Id == CompModelId).FirstOrDefault();

                            if (db_datapotential != null)
                            {
                                db_datapotential.AppraisalPotentialModel = null;
                                db.CompetencyModel.Attach(db_datapotential);
                                db.Entry(db_datapotential).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(db_datapotential).State = System.Data.Entity.EntityState.Detached;
                            }
                            break;
                        case "PastExperienceModel":
                            var db_datapastexp = db.CompetencyModel.Include(e => e.PastExperienceModel).Where(e => e.Id == CompModelId).FirstOrDefault();

                            if (db_datapastexp != null)
                            {
                                db_datapastexp.PastExperienceModel = null;
                                db.CompetencyModel.Attach(db_datapastexp);
                                db.Entry(db_datapastexp).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(db_datapastexp).State = System.Data.Entity.EntityState.Detached;
                            }
                            break;
                        case "PersonnelModel":
                            var db_datapersonnel = db.CompetencyModel.Include(e => e.PersonnelModel).Where(e => e.Id == CompModelId).FirstOrDefault();

                            if (db_datapersonnel != null)
                            {
                                db_datapersonnel.PersonnelModel = null;
                                db.CompetencyModel.Attach(db_datapersonnel);
                                db.Entry(db_datapersonnel).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(db_datapersonnel).State = System.Data.Entity.EntityState.Detached;
                            }
                            break;
                        case "QualificationModel":
                            var db_dataqualification = db.CompetencyModel.Include(e => e.QualificationModel).Where(e => e.Id == CompModelId).FirstOrDefault();

                            if (db_dataqualification != null)
                            {
                                db_dataqualification.PersonnelModel = null;
                                db.CompetencyModel.Attach(db_dataqualification);
                                db.Entry(db_dataqualification).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(db_dataqualification).State = System.Data.Entity.EntityState.Detached;
                            }
                            break;
                        case "ServiceModel":
                            var db_dataservice = db.CompetencyModel.Include(e => e.ServiceModel).Where(e => e.Id == CompModelId).FirstOrDefault();

                            if (db_dataservice != null)
                            {
                                db_dataservice.ServiceModel = null;
                                db.CompetencyModel.Attach(db_dataservice);
                                db.Entry(db_dataservice).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(db_dataservice).State = System.Data.Entity.EntityState.Detached;
                            }
                            break;
                        case "SkillModel":
                            var db_dataskill = db.CompetencyModel.Include(e => e.SkillModel).Where(e => e.Id == CompModelId).FirstOrDefault();

                            if (db_dataskill != null)
                            {
                                db_dataskill.SkillModel = null;
                                db.CompetencyModel.Attach(db_dataskill);
                                db.Entry(db_dataskill).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(db_dataskill).State = System.Data.Entity.EntityState.Detached;
                            }
                            break;
                        case "TrainingModel":
                            var db_datatraining = db.CompetencyModel.Include(e => e.TrainingModel).Where(e => e.Id == CompModelId).FirstOrDefault();

                            if (db_datatraining != null)
                            {
                                db_datatraining.TrainingModel = null;
                                db.CompetencyModel.Attach(db_datatraining);
                                db.Entry(db_datatraining).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(db_datatraining).State = System.Data.Entity.EntityState.Detached;
                            }
                            break;
                    }
                    ts.Complete();
                    return this.Json(new { status = true, responseText = "Data removed successfully.", JsonRequestBehavior.AllowGet });
                }
               
                
               
            }
        }


    }
}