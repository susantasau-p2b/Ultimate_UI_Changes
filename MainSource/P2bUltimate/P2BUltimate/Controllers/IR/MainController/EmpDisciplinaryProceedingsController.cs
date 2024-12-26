using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using IR;
using System.Diagnostics;

namespace P2BUltimate.Controllers.IR.MainController
{
    [AuthoriseManger]
    public class EmpDisciplinaryProceedingsController : Controller
    {


        public ActionResult Index()
        {
            return View("~/Views/IR/MainViews/EmpDisciplinaryProceedings/Index.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/IR/EmpDisciplinaryProceedingPartial.cshtml");
        }

        public JsonResult PopulateDropDownCaseList(string data)
        {
            //int id = int.Parse(data);
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.EmpDisciplineProcedings.ToList();
                var CaseId = a.Select(c => c.CaseNo).Distinct().ToList();
                SelectList s = (SelectList)null;
                //var selectedVal = "";
                if (CaseId != null)
                {
                    s = new SelectList(CaseId);

                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult PopulateDropDownCaseList(string data, string data2)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        SelectList s = (SelectList)null;
        //        var selected = "";
        //        //if (data != "" && data != null)
        //        //{
        //            var a = db.EmpDisciplineProcedings.ToList();
        //                    var CaseId = a.Select(c => c.CaseNo).ToList();
        //           // var qurey = db.Lookup.Where(e => e.Code == data).Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault(); // added by rekha 26-12-16
        //            //if (data2 != "" && data2 != "0")
        //            //{
        //            //    selected = data2;
        //            //}
        //            if (CaseId != null)
        //            {
        //                s = new SelectList(CaseId, "Id", "CaseNo", selected);
        //            }
        //       // }
        //        return Json(s, JsonRequestBehavior.AllowGet);
        //    }
        //}


        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string VictimName { get; set; }
            public string CaseOpeningDate { get; set; }
            public string LastStage { get; set; }

        }


        public ActionResult ValidateForm(FormCollection form)
        {
            string Packagelist = form["Packagelist"] == "0" ? "" : form["Packagelist"];
            int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
            List<string> Msg = new List<string>();


            if (Packagelist == null)
            {
                Msg.Add("Please select Package");
            }
            if (Emp == null)
            {
                Msg.Add("Please select employee");
            }

            if (Msg.Count > 0)
            {
                return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }

        }

        //[HttpPost]
        //public ActionResult Create(EmpDisciplineProcedings EmpDiscipline, FormCollection form)
        //{
        //    List<string> Msg = new List<string>();
        //    List<string> Msgs = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {


        //        var empdisciplineData = db.EmpDisciplineProcedings.FirstOrDefault();

        //        if (empdisciplineData != null)
        //        {
        //            string Case = "";
        //            var checkFKids = db.EmpDisciplineProcedings.Include(e => e.MisconductComplaint).Where(e => e.Id == empdisciplineData.Id).FirstOrDefault();
        //            if (checkFKids.MisconductComplaint.Id == null)
        //            {
        //                return RedirectToAction("Create", "MisConductComplaint");
        //            }


        //        }


        //        string CaseNo = form["CaseNo"] == "0" ? "" : form["CaseNo"];
        //        string CaseOpeningDate = form["CaseOpeningDate"] == "0" ? "" : form["CaseOpeningDate"];
        //        string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];


        //        if (Emp == null && Emp == "")
        //        {
        //            Msg.Add("Please Select Victim");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //        }
        //        //var geo_id = form["geo_id"] == "0" ? "" : form["geo_id"];
        //        //var pay_id = form["pay_id"] == "0" ? "" : form["pay_id"];
        //        //var fun_id = form["fun_id"] == "0" ? "" : form["fun_id"];

        //        //if ((geo_id == "" || geo_id == null))
        //        //{
        //        //    Msg.Add("Please Select GeoStruct , Paystruct OR Funcstruct Filter");
        //        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //        //}


        //        //var Serialize = new JavaScriptSerializer();
        //        //var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);
        //        //var GeoList = deserialize.GeoStruct != null ? deserialize.GeoStruct : "";

        //        //var SerializePay = new JavaScriptSerializer();
        //        //var Paydeserialize = SerializePay.Deserialize<Utility.GridParaStructIdClass>(pay_id);
        //        //var PayList = Paydeserialize.PayStruct != null ? Paydeserialize.PayStruct : "";

        //        //var SerializeFunc = new JavaScriptSerializer();
        //        //var Funcdeserialize = SerializeFunc.Deserialize<Utility.GridParaStructIdClass>(fun_id);
        //        //var FuncList = Funcdeserialize.FunStruct != null ? Funcdeserialize.FunStruct : "";


        //        if (CaseNo != null && CaseNo != "")
        //        {
        //            var val = CaseNo;
        //            EmpDiscipline.CaseNo = val;
        //        }


        //        if (CaseOpeningDate != null && CaseOpeningDate != "")
        //        {
        //            var CaseDate = Convert.ToDateTime(CaseOpeningDate);
        //            EmpDiscipline.CaseOpeningDate = CaseDate;
        //        }
        //        //else
        //        //{
        //        //    Msg.Add("Please Enter CaseOpeningDate");
        //        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        //}



        //        Login DBloginIds = null;
        //        //Employee OEmployee = null;

        //        var UserSelectedEmpIds = Utility.StringIdsToListIds(Emp);
        //        var OEmployee = new List<Employee>();
        //        foreach (var itemEmpId in UserSelectedEmpIds)
        //        {
        //            var OOEmployee = db.Employee.Include(e => e.Login).Where(r => r.Id == itemEmpId).FirstOrDefault();

        //            //DBloginIds = db.Login.Include(e => e.LoginAccessGeostruct).Include(e => e.LoginAccessPaystruct).Include(e => e.LoginAccessFuncstruct)
        //            //                   .Where(e => e.Id == OOEmployee.Login.Id).FirstOrDefault();

        //            //if (GeoList != "" && GeoList != null)
        //            //{
        //            //    var Geoidss = Utility.StringIdsToListIds(GeoList);
        //            //    foreach (var itemid in Geoidss)
        //            //    {

        //            //        var DBLoginAccessGeoIds = DBloginIds.LoginAccessGeostruct.Where(e => e.GeoStruct_Id == itemid
        //            //                          && e.Package.LookupVal.ToUpper() == LoginAccess.Package.LookupVal.ToUpper()).FirstOrDefault();

        //            //        if (DBLoginAccessGeoIds != null)
        //            //        {
        //            //            Msg.Add("Selected Record Already Exists...");
        //            //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //        }
        //            //        else
        //            //        {
        //            //            break;
        //            //        }

        //            //    }
        //            //}


        //            if (ModelState.IsValid)
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
        //                              new System.TimeSpan(0, 30, 0)))
        //                {
        //                    EmpDiscipline.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

        //                    EmpDisciplineProcedings EmpDisciplinePro = new EmpDisciplineProcedings()
        //                    {
        //                        CaseNo = EmpDiscipline.CaseNo,
        //                        CaseOpeningDate = EmpDiscipline.CaseOpeningDate,
        //                        DBTrack = EmpDiscipline.DBTrack
        //                    };
        //                    db.EmpDisciplineProcedings.Add(EmpDisciplinePro);
        //                    try
        //                    {




        //                        db.SaveChanges();

        //                        ts.Complete();
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        LogFile Logfile = new LogFile();
        //                        ErrorLog Err = new ErrorLog()
        //                        {
        //                            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                            ExceptionMessage = ex.Message,
        //                            ExceptionStackTrace = ex.StackTrace,
        //                            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                            LogTime = DateTime.Now
        //                        };
        //                        Logfile.CreateLogFile(Err);
        //                        //    List<string> Msg = new List<string>();
        //                        Msg.Add(ex.Message);
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                StringBuilder sb = new StringBuilder("");
        //                foreach (ModelState modelState in ModelState.Values)
        //                {
        //                    foreach (ModelError error in modelState.Errors)
        //                    {
        //                        sb.Append(error.ErrorMessage);
        //                        sb.Append("." + "\n");
        //                    }
        //                }
        //                //var errorMsg = sb.ToString();
        //                //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
        //                List<string> MsgB = new List<string>();
        //                var errorMsg = sb.ToString();
        //                MsgB.Add(errorMsg);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //    }
        //    Msgs.Add("Data Saved successfully");
        //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public ActionResult Create(int Id)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {

                try
                {
                    string EmpIridstr = Convert.ToString(Id);
                    string caseNO = Convert.ToString(Session["findcase"]);
                    //var empdisciplin = db.EmpDisciplineProcedings.Where(e => e.CaseNo == caseNO).ToList().FirstOrDefault();
                    //EmpDisciplineProcedings empdisciplineData = db.EmpDisciplineProcedings
                    //                                            .Include(e => e.MisconductComplaint)
                    //                                            .Include(e => e.PreminaryEnquiry)
                    //                                            .Include(e => e.PreminaryEnquiryAction)
                    //                                            .Include(e => e.ChargeSheet)
                    //                                            .Include(e => e.ChargeSheetEnquiryNotice)
                    //                                            .Include(e => e.ChargeSheetEnquiryNoticeServing)
                    //                                            .Include(e => e.ChargeSheetEnquiryProceedings)
                    //                                            .Include(e => e.ChargeSheetEnquiryReport)
                    //                                            .Include(e => e.ChargeSheetReply)
                    //                                            .Include(e => e.ChargeSheetServing)
                    //                                            .Include(e => e.FinalShowCauseNotice)
                    //                                            .Include(e => e.FinalShowCauseNoticeClarificarionServing)
                    //                                            .Include(e => e.FinalShowCauseNoticeClarification)
                    //                                            .Include(e => e.FinalShowCauseNoticeReply)
                    //                                            .Include(e => e.FinalShowCauseNoticeServing)
                    //                                            .Include(e => e.PostEnquiryPrerquisite)
                    //                                            .Include(e => e.PunishmentOrder)
                    //                                            .Include(e => e.PunishmentOrderDelivery)
                    //                                            .Include(e => e.PunishmentOrderApeal)
                    //                                            .Include(e => e.PunishmentOrderApealReply)
                    //                                            .Include(e => e.PunishmentOrderImplementation)
                    //                                            .Where(e => e.CaseNo == caseNO).ToList().LastOrDefault();
                   
                    var EmployeeIrData = db.EmployeeIR.Select(a => new
                    {
                        EmpIrid = a.Id.ToString(),
                        ObjEmpDisProceedings = a.EmpDisciplineProcedings.Select(b => new 
                        {
                            getStageNo = b.ProceedingStage != 0 ? b.ProceedingStage : 0,
                            getCaseNo = b.CaseNo,
                            //getMisconductComplaint = b.MisconductComplaint.Id.ToString() != "" ? b.MisconductComplaint.Id.ToString() : "",
                            //getPreminaryEnquiry = b.PreminaryEnquiry.Id.ToString() != "" ? b.PreminaryEnquiry.Id.ToString() : "",
                            //getPreminaryEnquiryAction = b.PreminaryEnquiryAction.Id.ToString() != "" ? b.PreminaryEnquiryAction.Id.ToString() : "",

                            //getChargeSheet = b.ChargeSheet.Id.ToString() != "" ? b.ChargeSheet.Id.ToString() : "",
                            //getChargeSheetServing = b.ChargeSheetServing.Select(c => c.Id).ToString() != "" ? b.ChargeSheetServing.Select(c => c.Id).ToString() : "",
                            //getChargeSheetReply = b.ChargeSheetReply.Id.ToString() != "" ? b.ChargeSheetReply.Id.ToString() : "",
                            //getChargeSheetEnquiryNotice = b.ChargeSheetEnquiryNotice.Id.ToString() != "" ? b.ChargeSheetEnquiryNotice.Id.ToString() : "",
                            //getChargeSheetEnquiryNoticeServing = b.ChargeSheetEnquiryNoticeServing.Id.ToString() != "" ? b.ChargeSheetEnquiryNoticeServing.Id.ToString() : "",

                            //getChargeSheetEnquiryProceedings = b.ChargeSheetEnquiryProceedings.Id.ToString() != "" ? b.ChargeSheetEnquiryProceedings.Id.ToString() : "",
                            //getChargeSheetEnquiryReport = b.ChargeSheetEnquiryReport.Id.ToString() != "" ? b.ChargeSheetEnquiryReport.Id.ToString() : "",
                            //getPostEnquiryPrerquisite = b.PostEnquiryPrerquisite.Id.ToString() != "" ? b.PostEnquiryPrerquisite.Id.ToString() : "",
                            //getFinalShowCauseNotice = b.FinalShowCauseNotice.Id.ToString() != "" ? b.FinalShowCauseNotice.Id.ToString() : "",
                            //getFinalShowCauseNoticeServing = b.FinalShowCauseNoticeServing.Id.ToString() != "" ? b.FinalShowCauseNoticeServing.Id.ToString() : "",

                            //getFinalShowCauseNoticeReply = b.FinalShowCauseNoticeReply.Id.ToString() != "" ? b.FinalShowCauseNoticeReply.Id.ToString() : "",
                            //getFinalShowCauseNoticeClarificarion = b.FinalShowCauseNoticeClarification.Id.ToString() != "" ? b.FinalShowCauseNoticeClarification.Id.ToString() : "",
                            //getFinalShowCauseNoticeClarificarionServing = b.FinalShowCauseNoticeClarificarionServing.Id.ToString() != "" ? b.FinalShowCauseNoticeClarificarionServing.Id.ToString() : "",
                            //getPunishmentOrder = b.PunishmentOrder.Id.ToString() != "" ? b.PunishmentOrder.Id.ToString() : "",
                            //getPunishmentOrderDelivery = b.PunishmentOrderDelivery.Id.ToString() != "" ? b.PunishmentOrderDelivery.Id.ToString() : "",

                            //getPunishmentOrderApeal = b.PunishmentOrderApeal.Id.ToString() != "" ? b.PunishmentOrderApeal.Id.ToString() : "",
                            //getPunishmentOrderApealReply = b.PunishmentOrderApealReply.Id.ToString() != "" ? b.PunishmentOrderApealReply.Id.ToString() : "",
                            //getPunishmentOrderImplementation = b.PunishmentOrderImplementation.Id.ToString() != "" ? b.PunishmentOrderImplementation.Id.ToString() : "",

                            getMisconductComplaint = b.MisconductComplaint.Id.ToString(),
                            getPreminaryEnquiry = b.PreminaryEnquiry.Id.ToString(),
                            getPreminaryEnquiryAction = b.PreminaryEnquiryAction.Id.ToString(),

                            getChargeSheet = b.ChargeSheet.Id.ToString(),
                            getChargeSheetServing = b.ChargeSheetServing.FirstOrDefault().Id.ToString(),
                            getChargeSheetReply = b.ChargeSheetReply.Id.ToString(),
                            getChargeSheetEnquiryNotice = b.ChargeSheetEnquiryNotice.Id.ToString(),
                            getChargeSheetEnquiryNoticeServing = b.ChargeSheetEnquiryNoticeServing.Id.ToString(),

                            getChargeSheetEnquiryProceedings = b.ChargeSheetEnquiryProceedings.Id.ToString(),
                            getChargeSheetEnquiryReport = b.ChargeSheetEnquiryReport.Id.ToString(),
                            getPostEnquiryPrerquisite = b.PostEnquiryPrerquisite.Id.ToString(),
                            getFinalShowCauseNotice = b.FinalShowCauseNotice.Id.ToString(),
                            getFinalShowCauseNoticeServing = b.FinalShowCauseNoticeServing.Id.ToString(),

                            getFinalShowCauseNoticeReply = b.FinalShowCauseNoticeReply.Id.ToString(),
                            getFinalShowCauseNoticeClarificarion = b.FinalShowCauseNoticeClarification.Id.ToString(),
                            getFinalShowCauseNoticeClarificarionServing = b.FinalShowCauseNoticeClarificarionServing.Id.ToString(),
                            getPunishmentOrder = b.PunishmentOrder.Id.ToString(),
                            getPunishmentOrderDelivery = b.PunishmentOrderDelivery.Id.ToString(),

                            getPunishmentOrderApeal = b.PunishmentOrderApeal.Id.ToString(),
                            getPunishmentOrderApealReply = b.PunishmentOrderApealReply.Id.ToString(),
                            getPunishmentOrderImplementation = b.PunishmentOrderImplementation.Id.ToString(),

                        }).Where(e => e.getCaseNo == caseNO).ToList(),

                    }).Where(e => e.EmpIrid == EmpIridstr).FirstOrDefault();

                    var Stages = EmployeeIrData == null ? 0 : EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getStageNo;//empdisciplineData.ProceedingStage;

                    #region old code
                    //var Misid = empdisciplineData != null && empdisciplineData.MisconductComplaint != null ? empdisciplineData.MisconductComplaint.Id.ToString() : null;
                    //var PreminaryEnqid = empdisciplineData != null && empdisciplineData.PreminaryEnquiry != null ? empdisciplineData.PreminaryEnquiry.Id.ToString() : null;
                    //var PreminaryEnqActionid = empdisciplineData != null && empdisciplineData.PreminaryEnquiryAction != null ? empdisciplineData.PreminaryEnquiryAction.Id.ToString() : null;
                    //var ChargeSheetid = empdisciplineData != null && empdisciplineData.ChargeSheet != null ? empdisciplineData.ChargeSheet.Id.ToString() : null;

                    //var ChargeSheetServingTemp = empdisciplineData.ChargeSheetServing.FirstOrDefault();
                    //var ChargeSheetServingid = ChargeSheetServingTemp != null ? ChargeSheetServingTemp.Id.ToString() : null;
                    //// var ChargeSheetServingStatusid = ChargeSheetServingid != null ? db.ChargeSheetServingStatus.Where(e => e.Id != null).LastOrDefault() : null;
                    //var ChargeSheetReplyid = empdisciplineData != null && empdisciplineData.ChargeSheetReply != null ? empdisciplineData.ChargeSheetReply.Id.ToString() : null;

                    //// Stage 8
                    //var ChargeSheetEnquiryNoticeId = empdisciplineData != null && empdisciplineData.ChargeSheetEnquiryNotice != null ? empdisciplineData.ChargeSheetEnquiryNotice.Id.ToString() : null;
                    //var ChargeSheetEnquiryNoticeServingId = empdisciplineData != null && empdisciplineData.ChargeSheetEnquiryNoticeServing != null ? empdisciplineData.ChargeSheetEnquiryNoticeServing.Id.ToString() : null;

                    //var ChargeSheetEnquiryProceedingId = empdisciplineData != null && empdisciplineData.ChargeSheetEnquiryProceedings != null ? empdisciplineData.ChargeSheetEnquiryProceedings.Id.ToString() : null;
                    //var ChargeSheetEnquiryReportId = empdisciplineData != null && empdisciplineData.ChargeSheetEnquiryReport != null ? empdisciplineData.ChargeSheetEnquiryReport.Id.ToString() : null;
                    //var PostEnquiryPrerquisiteId = empdisciplineData != null && empdisciplineData.PostEnquiryPrerquisite != null ? empdisciplineData.PostEnquiryPrerquisite.Id.ToString() : null;
                    //var FinalShowCauseNoticeId = empdisciplineData != null && empdisciplineData.FinalShowCauseNotice != null ? empdisciplineData.FinalShowCauseNotice.Id.ToString() : null;
                    //var FinalShowCauseNoticeServingId = empdisciplineData != null && empdisciplineData.FinalShowCauseNoticeServing != null ? empdisciplineData.FinalShowCauseNoticeServing.Id.ToString() : null;

                    //var FinalShowCauseNoticeReplyId = empdisciplineData != null && empdisciplineData.FinalShowCauseNoticeReply != null ? empdisciplineData.FinalShowCauseNoticeReply.Id.ToString() : null;
                    //var FinalShowCauseNoticeClarificationId = empdisciplineData != null && empdisciplineData.FinalShowCauseNoticeClarification != null ? empdisciplineData.FinalShowCauseNoticeClarification.Id.ToString() : null;
                    //var FinalShowCauseNoticeClarificationServingId = empdisciplineData != null && empdisciplineData.FinalShowCauseNoticeClarificarionServing != null ? empdisciplineData.FinalShowCauseNoticeClarificarionServing.Id.ToString() : null;
                    //var PunishmentOrderId = empdisciplineData != null && empdisciplineData.PunishmentOrder != null ? empdisciplineData.PunishmentOrder.Id.ToString() : null;
                    //var PunishmentOrderDeliveryId = empdisciplineData != null && empdisciplineData.PunishmentOrderDelivery != null ? empdisciplineData.PunishmentOrderDelivery.Id.ToString() : null;
                    //var PunishmentOrderAppealId = empdisciplineData != null && empdisciplineData.PunishmentOrderApeal != null ? empdisciplineData.PunishmentOrderApeal.Id.ToString() : null;


                    //var PunishmentOrderAppealReplyId = empdisciplineData != null && empdisciplineData.PunishmentOrderApealReply != null ? empdisciplineData.PunishmentOrderApealReply.Id.ToString() : null;
                    //var PunishmentOrderImplementationId = empdisciplineData != null && empdisciplineData.PunishmentOrderImplementation != null ? empdisciplineData.PunishmentOrderImplementation.Id.ToString() : null;
                    #endregion
                    string getInitialMisconduct = EmployeeIrData != null && EmployeeIrData.ObjEmpDisProceedings != null ?  EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getMisconductComplaint : null;
                    switch (Stages)
                    {
                        case 0:
                            if (string.IsNullOrEmpty(getInitialMisconduct))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "MisconductComplaint" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 1:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getPreminaryEnquiry))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "PreminaryEnquiry" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 2:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getPreminaryEnquiryAction))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "PreminaryEnquiryAction" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 3:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getChargeSheet))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "ChargeSheet" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 4:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getChargeSheetServing))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "ChargeSheetServing" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        //case 5:
                        //    if (ChargeSheetServingStatusid == null)
                        //    {
                        //        var temp = 1; var tempT = 2; var tempTT = 3; var tempTTT = 4; var tempTTTa = 5;
                        //        return Json(new Object[] { temp, tempT, tempTT, tempTTT, tempTTTa, ChargeSheetServingStatusid, JsonRequestBehavior.AllowGet });
                        //    }
                        //    break;

                        case 5:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getChargeSheetReply))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "ChargeSheetReply" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 6:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getChargeSheetEnquiryNotice))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "ChargeSheetEnquiryNotice" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 7:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getChargeSheetEnquiryNoticeServing))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "ChargeSheetEnquiryNoticeServing" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 8:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getChargeSheetEnquiryProceedings))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "ChargeSheetEnquiryProceedings" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 9:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getChargeSheetEnquiryReport))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "ChargeSheetEnquiryReport" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 10:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getPostEnquiryPrerquisite))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "PostEnquiryPrerquisite" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 11:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getFinalShowCauseNotice))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "FinalShowCauseNotice" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 12:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getFinalShowCauseNoticeServing))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "FinalShowCauseNoticeServing" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 13:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getFinalShowCauseNoticeReply))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "FinalShowCauseNoticeReply" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 14:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getFinalShowCauseNoticeClarificarion))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "FinalShowCauseNoticeClarificarion" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 15:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getFinalShowCauseNoticeClarificarionServing))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "FinalShowCauseNoticeClarificarionServing" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 16:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getPunishmentOrder))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "PunishmentOrder" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 17:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getPunishmentOrderDelivery))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "PunishmentOrderDelivery" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 18:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getPunishmentOrderApeal))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "PunishmentOrderApeal" }, JsonRequestBehavior.AllowGet);
                            }
  
                            break;

                    // For PunishmentOrderAppealReply
                        case 19:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getPunishmentOrderApealReply))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "PunishmentOrderApealReply" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        // For PunishmentOrderImplemetation
                        case 20:
                            if (string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getPunishmentOrderImplementation))
                            {
                                return Json(new Utility.JsonReturnClass { success = true, responseText = "PunishmentOrderImplementation" }, JsonRequestBehavior.AllowGet);
                            }
                            break;

                        case 21:
                            if (!string.IsNullOrEmpty(EmployeeIrData.ObjEmpDisProceedings.LastOrDefault().getPunishmentOrderImplementation))
                            {
                                List<string> Message = new List<string>();
                                TempData["Stage"] = "Closed";
                                Message.Add(" Please create New case, the selected case already Closed. !!! ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Message, data = TempData["Stage"].ToString() }, JsonRequestBehavior.AllowGet);
                            }
                            break;



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

                }
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }

        }



        [HttpPost]
        public ActionResult NewCase(EmpDisciplineProcedings c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string CaseNo = form["CaseNo"] == "0" ? "" : form["CaseNo"];
                    string CaseOpeningDate = form["CaseOpeningDate"] == "0" ? "" : form["CaseOpeningDate"];

                    //string frequency = form["Frequencylist"] == "0" ? "" : form["Frequencylist"];
                    //string type = form["Typelist"] == "0" ? "" : form["Typelist"];
                    //string roundingmethod = form["RoundingMethodlist"] == "0" ? "" : form["RoundingMethodlist"];
                    //string SalHeadOperType = form["SalHeadOperationTypelist"] == "0" ? "" : form["SalHeadOperationTypelist"];
                    //string ProcessType = form["ProcessTypelist"] == "0" ? "" : form["ProcessTypelist"];
                    //string LvHead = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];
                    //int company_Id = 0;
                    //company_Id = Convert.ToInt32(Session["CompId"]);
                    //var companypayroll = new CompanyPayroll();
                    //companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == company_Id).SingleOrDefault();



                    if (CaseNo != null && CaseNo != "")
                    {
                        var val = Convert.ToString(CaseNo);
                        c.CaseNo = val;
                    }

                    if (CaseOpeningDate != null && CaseOpeningDate != "")
                    {
                        var val = Convert.ToDateTime(CaseOpeningDate);
                        c.CaseOpeningDate = val;
                    }

                    //if (roundingmethod != null && roundingmethod != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(roundingmethod));
                    //    c.RoundingMethod = val;
                    //}

                    //if (SalHeadOperType != null && SalHeadOperType != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(SalHeadOperType));
                    //    c.SalHeadOperationType = val;
                    //}

                    //if (ProcessType != null && ProcessType != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(ProcessType));
                    //    c.ProcessType = val;
                    //}
                    //List<LeaveDependPolicy> lookupval = new List<LeaveDependPolicy>();
                    //if (LvHead != null && LvHead != "")
                    //{
                    //    var ids = Utility.StringIdsToListIds(LvHead);
                    //    foreach (var ca in ids)
                    //    {
                    //        var LvHead_val = db.LeaveDependPolicy.Find(ca);
                    //        lookupval.Add(LvHead_val);
                    //        c.LeaveDependPolicy = lookupval;
                    //    }
                    //}

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.EmpDisciplineProcedings.Any(o => o.CaseNo == c.CaseNo))
                            //{
                            //    Msg.Add("  Case No. Already Exists.  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //    // return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            //}

                            //if (c.OnLeave == true && c.LeaveDependPolicy == null)
                            //{
                            //    Msg.Add("  Please Select LeaveDependPolicy.  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //SalaryHead SalaryHead = new SalaryHead()
                            //{
                            //    Code = c.Code == null ? "" : c.Code.Trim(),
                            //    Name = c.Name == null ? "" : c.Name.Trim(),
                            //    Type = c.Type,
                            //    Frequency = c.Frequency,
                            //    OnAttend = c.OnAttend,
                            //    OnLeave = c.OnLeave,
                            //    InPayslip = c.InPayslip,
                            //    InITax = c.InITax,
                            //    RoundingMethod = c.RoundingMethod,
                            //    RoundDigit = c.RoundDigit,
                            //    DBTrack = c.DBTrack,
                            //    SalHeadOperationType = c.SalHeadOperationType,
                            //    ProcessType = c.ProcessType,
                            //    SeqNo = c.SeqNo,
                            //    LeaveDependPolicy = c.LeaveDependPolicy
                            //};
                            EmpDisciplineProcedings EMPDPro = new EmpDisciplineProcedings()
                            {
                                CaseNo = c.CaseNo,
                                CaseOpeningDate = c.CaseOpeningDate,
                                DBTrack = c.DBTrack
                            };

                            try
                            {
                                db.EmpDisciplineProcedings.Add(EMPDPro);
                                // var rtn_Obj = DBTrackFile.DBTrackSave("IR/IR", null, db.ChangeTracker, c.DBTrack);
                                //DT_SalaryHead DT_Corp = (DT_SalaryHead)rtn_Obj;
                                //DT_Corp.Type_Id = c.Type == null ? 0 : c.Type.Id;
                                //DT_Corp.RoundingMethod_Id = c.RoundingMethod == null ? 0 : c.RoundingMethod.Id;
                                //DT_Corp.Frequency_Id = c.Frequency == null ? 0 : c.Frequency.Id;
                                //DT_Corp.SalHeadOperationType_Id = c.SalHeadOperationType == null ? 0 : c.SalHeadOperationType.Id;
                                //DT_Corp.ProcessType_Id = c.ProcessType == null ? 0 : c.ProcessType.Id;
                                //db.Create(DT_Corp);
                                db.SaveChanges();

                                //if (companypayroll != null)
                                //{
                                //    var SalaryHead_list = new List<SalaryHead>();
                                //    SalaryHead_list.Add(SalaryHead);
                                //    companypayroll.SalaryHead = SalaryHead_list;
                                //    db.CompanyPayroll.Attach(companypayroll);
                                //    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                //    db.SaveChanges();
                                //    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                                //}
                                
                                ts.Complete();
                                Session["EmpdisId"] = EMPDPro.Id;
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            //catch (DataException /* dex */)
                            //{
                            //    Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //    //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                            //}
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
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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




        //
        public class returnDataClass
        {

            public bool IsCaseClosed { get; set; }
            public DateTime? CaseClosingDate { get; set; }

            //public string SalaryHead { get; set; }

        }

        public ActionResult GridEditData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returnlist = new List<returnDataClass>();
                if (data != null && data != 0)
                {
                    string caseN = Convert.ToString(Session["findcase"]);

                    var retrundataList = db.EmpDisciplineProcedings.Where(e => e.Id == data && e.CaseNo == caseN).ToList();
                   // var retrundataList = db.PerkTransT.Include(e => e.SalaryHead).Where(e => e.Id == data).ToList();
                    foreach (var a in retrundataList)
                    {
                        returnlist.Add(new returnDataClass()
                        {

                            IsCaseClosed = a.IsCaseClosed,
                            CaseClosingDate = a.CaseClosingDate
                            //SalaryHead = a.SalaryHead.FullDetails != null ? a.SalaryHead.FullDetails : null,
                            //ProjectedAmount = a.ProjectedAmount
                        });
                    }
                    return Json(new { returndata = returnlist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }

        
        //

        public ActionResult GridEditSave(EmpDisciplineProcedings EDP, FormCollection form, string data)
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                string caseN = Convert.ToString(Session["findcase"]);
                var edp_Data = db.EmpDisciplineProcedings.Where(e => e.Id == Id && e.CaseNo == caseN).SingleOrDefault();
                edp_Data.IsCaseClosed = EDP.IsCaseClosed;
                if (edp_Data.IsCaseClosed == true)
                {
                    edp_Data.CaseClosingDate = EDP.CaseClosingDate;
                }
                else
                {
                    edp_Data.CaseClosingDate = null;
                }
                
                //BA_EmpTargetData.TargetCompliance = BA_EmpTarget.TargetCompliance;


                //string Category = form["Category"] == "0" ? "" : form["Category"];
                //if (Category != "0")
                //{
                //    var id = Convert.ToInt32(Category);
                //    var Categorydata = db.BA_Category.Where(e => e.Id == id).SingleOrDefault();
                //    BA_EmpTargetData.BA_Category = Categorydata;
                //}
                //string SubCategory = form["Subcategory"] == "0" ? "" : form["Subcategory"];
                //if (SubCategory != "0")
                //{
                //    var id = Convert.ToInt32(SubCategory);
                //    var SubCategorydata = db.BA_SubCategory.Where(e => e.Id == id).SingleOrDefault();
                //    BA_EmpTargetData.BA_SubCategory = SubCategorydata;
                //}
                using (TransactionScope ts = new TransactionScope())
                {

                    edp_Data.DBTrack = new DBTrack
                    {
                        CreatedBy = edp_Data.DBTrack.CreatedBy == null ? null : edp_Data.DBTrack.CreatedBy,
                        CreatedOn = edp_Data.DBTrack.CreatedOn == null ? null : edp_Data.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };
                    try
                    {
                        db.EmpDisciplineProcedings.Attach(edp_Data);
                        db.Entry(edp_Data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(edp_Data).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data Updated Successfully.", JsonRequestBehavior.AllowGet });
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
                        List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult Load(string CaseList) //// For Load Button
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    int EmpDis_Id = 0;
                    EmpDis_Id = Convert.ToInt32(Session["EmpdisId"]);
                    string Caseno = CaseList;
                    var findcase = (Object)null;
                    if (Caseno != null && Caseno != "")
                    {
                        findcase = db.EmpDisciplineProcedings.Where(e => e.CaseNo == Caseno).FirstOrDefault().CaseNo;
                    }

                    Session["findcase"] = findcase;

                    return Json(new Object[] { findcase, JsonRequestBehavior.AllowGet });

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
                    //    List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }

        }


        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                    //    .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                    //    .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                    //    .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                    //    .AsNoTracking().ToList();
                    string caseNO = Convert.ToString(Session["findcase"]);

                    var all = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings).Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id != null).ToList();


                    // for searchs
                    IEnumerable<EmployeeIR> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeeIR, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Employee.EmpCode : "");
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

                        List<returndatagridclass> result = new List<returndatagridclass>();


                        var findCase = Convert.ToString(Session["findcase"]);



                        foreach (var item in fall)
                        {
                            var GetCaseno = item.EmpDisciplineProcedings.Where(e => e.CaseNo == findCase).Select(s => s.CaseOpeningDate).FirstOrDefault();
                            if (GetCaseno != null)
                            {
                                var caseopendate = item.EmpDisciplineProcedings.Where(e => e.CaseNo == findCase).Select(s => s.CaseOpeningDate).FirstOrDefault();
                                var lastStage = item.EmpDisciplineProcedings.Where(e => e.CaseNo == findCase).Select(s => s.ProceedingStage).FirstOrDefault();

                                var EmpIR_Id = item.Id.ToString();
                               // Session["EmpIR_Id"] = EmpIR_Id;

                                result.Add(new returndatagridclass
                                {
                                    Id = item.Id.ToString(),
                                    Code = item.Employee != null ? item.Employee.EmpCode : null,
                                    VictimName = item.Employee != null && item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                                    CaseOpeningDate = caseopendate != null ? caseopendate.ToString() : null,
                                    LastStage = lastStage != null ? lastStage.ToString() : null


                                });

                            }


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

                                     select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode, };
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
        } // For EmpDiscipline




        public class EmpDisciplineChildDataClass // For EmpDisciplinechildgrid
        {
            public int Id { get; set; }
            public string Stage { get; set; }
            public string Description { get; set; }
            public bool IsCaseClosed { get; set; }

        }




        //public ActionResult Get_LoginAcess(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        #region try
        //        try
        //        {
        //            var db_data = db.Employee.Include(e => e.Login.LoginAccessGeostruct)
        //                                     .Include(e => e.Login.LoginAccessPaystruct)
        //                                     .Include(e => e.Login.LoginAccessFuncstruct).Where(e => e.Id == data).AsEnumerable()
        //                                        .Select(t => new Login
        //                                        {
        //                                            LoginAccessGeostruct = t.Login.LoginAccessGeostruct,
        //                                            LoginAccessPaystruct = t.Login.LoginAccessPaystruct,
        //                                            LoginAccessFuncstruct = t.Login.LoginAccessFuncstruct

        //                                        }).ToList();
        //            //.Select(t => t.Login.LoginAccessGeostruct).AsNoTracking().FirstOrDefault();

        //            //var db_data = db.Employee.Where(e => e.Id == data).Select(t => t.Login.LoginAccessGeostruct).AsNoTracking().FirstOrDefault();

        //            if (db_data != null)
        //            {
        //                List<LoginAccessChildDataClass> returndata = new List<LoginAccessChildDataClass>();

        //                LoginAccessGeostruct LoginAccessIds = null ;
        //                LoginAccessPaystruct LoginAccessPayIds = null;
        //                LoginAccessFuncstruct LoginAccessFuncIds = null;
        //                foreach (var item in db_data)
        //                {
        //                    #region GeoStruct
        //                    foreach (var item1 in item.LoginAccessGeostruct)
        //                    {
        //                         LoginAccessIds = db.LoginAccessGeostruct.Include(e => e.GeoStruct)
        //                                                                                    .Include(e => e.GeoStruct.Location)
        //                                                                                    .Include(e => e.GeoStruct.Location.LocationObj)
        //                                                                                    .Include(e => e.Package)
        //                                                                                    .Where(e => e.Id == item1.Id).AsNoTracking().FirstOrDefault();
        //                         //if (LoginAccessIds != null)
        //                         //{
        //                         //    returndata.Add(new LoginAccessChildDataClass
        //                         //    {
        //                         //        Id = LoginAccessIds.Id,
        //                         //        PackageCode = LoginAccessIds.Package.LookupVal != null ? LoginAccessIds.Package.LookupVal : null,
        //                         //        Geostrut = LoginAccessIds.GeoStruct != null && LoginAccessIds.GeoStruct.Location != null && LoginAccessIds.GeoStruct.Location != null && LoginAccessIds.GeoStruct.Location.LocationObj != null ? LoginAccessIds.GeoStruct.Location.LocationObj.LocCode.ToString() + " " + LoginAccessIds.GeoStruct.Location.LocationObj.LocDesc.ToString() : ""
        //                         //    });

        //                         //}
        //                    }


        //                    #endregion GeoStruct


        //                     #region PayStruct
        //                    foreach (var item1 in item.LoginAccessPaystruct)
        //                    {
        //                          LoginAccessPayIds = db.LoginAccessPaystruct.Include(e => e.PayStruct)
        //                                                                .Include(e => e.PayStruct.Grade)
        //                                                                .Include(e => e.PayStruct.JobStatus)
        //                                                                .Include(e => e.Package)
        //                                                                .Where(e => e.Id == item1.Id).AsNoTracking().FirstOrDefault();
        //                        // if (LoginAccessPayIds != null)
        //                        //{
        //                        //    returndata.Add(new LoginAccessChildDataClass
        //                        //    {
        //                        //        PayAccessId = LoginAccessPayIds.Id,
        //                        //        Paystruct = LoginAccessPayIds.PayStruct != null && LoginAccessPayIds.PayStruct.Grade != null && LoginAccessPayIds.PayStruct.JobStatus != null ? LoginAccessPayIds.PayStruct.Grade.Name.ToString() + " " + LoginAccessPayIds.PayStruct.JobStatus.FullDetails.ToString() : ""
        //                        //    });

        //                        //}
        //                    }
        //                     #endregion PayStruct


        //                     #region FuncStruct
        //                    foreach (var item1 in item.LoginAccessFuncstruct)
        //                    {
        //                      LoginAccessFuncIds = db.LoginAccessFuncstruct.Include(e => e.FuncStruct)
        //                                                                .Include(e => e.FuncStruct.Job)
        //                                                                .Include(e => e.Package)
        //                                                                .Where(e => e.Id == item1.Id).AsNoTracking().FirstOrDefault();

        //                        //if (LoginAccessFuncIds != null)
        //                        //{
        //                        //    returndata.Add(new LoginAccessChildDataClass
        //                        //    {
        //                        //        FuncAccessId = LoginAccessFuncIds.Id,
        //                        //        Funcstruct = LoginAccessFuncIds.FuncStruct != null && LoginAccessFuncIds.FuncStruct.Job != null ? LoginAccessFuncIds.FuncStruct.Job.FullDetails.ToString() : ""
        //                        //    });

        //                        //}
        //                    }
        //                    if (LoginAccessIds != null || LoginAccessPayIds != null || LoginAccessFuncIds != null)
        //                    {
        //                        returndata.Add(new LoginAccessChildDataClass
        //                        {
        //                            Id = LoginAccessIds.Id,
        //                            PackageCode = LoginAccessIds.Package.LookupVal != null ? LoginAccessIds.Package.LookupVal : null,
        //                            Geostrut = LoginAccessIds.GeoStruct != null && LoginAccessIds.GeoStruct.Location != null && LoginAccessIds.GeoStruct.Location != null && LoginAccessIds.GeoStruct.Location.LocationObj != null ? LoginAccessIds.GeoStruct.Location.LocationObj.LocCode.ToString() + " " + LoginAccessIds.GeoStruct.Location.LocationObj.LocDesc.ToString() : "",
        //                            PayAccessId = LoginAccessPayIds.Id,
        //                            Paystruct = LoginAccessPayIds.PayStruct != null && LoginAccessPayIds.PayStruct.Grade != null && LoginAccessPayIds.PayStruct.JobStatus != null ? LoginAccessPayIds.PayStruct.Grade.Name.ToString() + " " + LoginAccessPayIds.PayStruct.JobStatus.FullDetails.ToString() : "",
        //                            FuncAccessId = LoginAccessFuncIds.Id,
        //                            Funcstruct = LoginAccessFuncIds.FuncStruct != null && LoginAccessFuncIds.FuncStruct.Job != null ? LoginAccessFuncIds.FuncStruct.Job.FullDetails.ToString() : ""
        //                        });

        //                    }

        //                     #endregion FuncStruct

        //                }
        //                return Json(returndata, JsonRequestBehavior.AllowGet);

        //            }
        //            else
        //            {
        //                return null;
        //            }

        //        }
        //        #endregion try

        //        catch (Exception e)
        //        {
        //            throw e;
        //        }
        //    }
        //}

        public ActionResult Get_EmpDiscipline(int data)  // Child Grid which will be open on + click
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                #region try
                try
                {
                    string caseN = Convert.ToString(Session["findcase"]);
                    
                    var db_data = db.EmployeeIR.Where(e => e.Id == data).Select(t => t.EmpDisciplineProcedings.Where(s => s.CaseNo == caseN).ToList()).AsNoTracking().FirstOrDefault();

                    if (db_data != null)
                    {
                        List<EmpDisciplineChildDataClass> returndata = new List<EmpDisciplineChildDataClass>();

                        foreach (var item in db_data)
                        {
                            string caseNO = Convert.ToString(Session["findcase"]);
                            var EmpDisciplineIds = db.EmpDisciplineProcedings.Where(e => e.Id == item.Id && e.CaseNo == caseNO).AsNoTracking().FirstOrDefault();
                            var Stages = EmpDisciplineIds.ProceedingStage;
                            if (EmpDisciplineIds != null)
                            {

                                switch (Stages)
                                {
                                    case 1:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "MisconductComplaint",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 2:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "PreliminaryEnquiry",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 3:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "PreliminaryEnquiryAction",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 4:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "ChargeSheet",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 5:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "ChargeSheetServing",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    //case 6:
                                    //    returndata.Add(new EmpDisciplineChildDataClass
                                    //    {
                                    //        Id = EmpDisciplineIds.Id,
                                    //        Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                    //        Description = "ChargeSheetServingStatus",
                                    //        IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                    //    });

                                    //    break;

                                    case 6:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "ChargeSheetReply",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;


                                    case 7:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "ChargeSheetEnquiryNotice",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 8:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "ChargeSheetEnquiryNoticeServing",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 9:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "ChargeSheetEnquiryProceeding",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 10:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "ChargeSheetEnquiryReport",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 11:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "PostEnquiryPrerquisite",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 12:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "FinalShowCauseNotice",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 13:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "FinalShowCauseNoticeServing",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 14:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "FinalShowCauseNoticeReply",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 15:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "FinalShowCauseNoticeClarification",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 16:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "FinalShowCauseNoticeClarificationServing",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 17:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "PunishmentOrder",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 18:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "PunishmentOrderDelivery",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 19:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "PunishmentOrderAppeal",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 20:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "PunishmentOrderAppealReply",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                    case 21:
                                        returndata.Add(new EmpDisciplineChildDataClass
                                        {
                                            Id = EmpDisciplineIds.Id,
                                            Stage = EmpDisciplineIds.ProceedingStage != null ? EmpDisciplineIds.ProceedingStage.ToString() : "",
                                            Description = "PunishmentOrderImplementation",
                                            IsCaseClosed = EmpDisciplineIds.IsCaseClosed

                                        });

                                        break;

                                }






                            }
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                #endregion try

                catch (Exception e)
                {
                    throw e;
                }
            }
        }




        //[HttpPost]
        //public async Task<ActionResult> Delete(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var ids = Utility.StringIdsToListIds(data);
        //        List<string> Msg = new List<string>();

        //        int empidintrnal = Convert.ToInt32(ids[0]);
        //        int empidMain = Convert.ToInt32(ids[1]);

        //        var EmpId = db.Employee.Include(e => e.Login.LoginAccessFuncstruct)
        //                               .Include(e => e.Login.LoginAccessPaystruct)
        //                               .Include(e => e.Login.LoginAccessGeostruct)
        //                               .Where(e => e.Id == empidMain).ToList();

        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {

        //            try
        //            {
        //                if (EmpId != null)
        //                {
        //                    string PackageCode = "";
        //                    LoginAccessGeostruct OLoginAccessGeostruct = db.LoginAccessGeostruct.Find(empidintrnal);
        //                    LoginAccessFuncstruct OLoginAccessFuncstruct = db.LoginAccessFuncstruct.Find(empidintrnal);
        //                    LoginAccessPaystruct OLoginAccessPaystruct = db.LoginAccessPaystruct.Find(empidintrnal);
        //                    if (OLoginAccessGeostruct != null)
        //                    {
        //                        PackageCode = db.LookupValue.Where(e => e.Id == OLoginAccessGeostruct.Package_Id).FirstOrDefault().LookupVal.ToString().ToUpper();
        //                    }
        //                    else if (OLoginAccessFuncstruct != null)
        //                    {
        //                        PackageCode = db.LookupValue.Where(e => e.Id == OLoginAccessFuncstruct.Package_Id).FirstOrDefault().LookupVal.ToString().ToUpper();
        //                    }
        //                    else
        //                    {
        //                        PackageCode = db.LookupValue.Where(e => e.Id == OLoginAccessPaystruct.Package_Id).FirstOrDefault().LookupVal.ToString().ToUpper();
        //                    }
        //                    //string PackageCode = db.LookupValue.Where(e => e.Id == OLoginAccessGeostruct.Package_Id).FirstOrDefault().LookupVal.ToString().ToUpper();
        //                    if (PackageCode != "")
        //                    {
        //                        List<LoginAccessGeostruct> LoginAccessGeostructList = EmpId.SelectMany(t => t.Login.LoginAccessGeostruct.Where(y => y.Package.LookupVal.ToUpper() == PackageCode).ToList()).ToList();
        //                        db.LoginAccessGeostruct.RemoveRange(LoginAccessGeostructList);
        //                        List<LoginAccessFuncstruct> LoginAccessFuncstructList = EmpId.SelectMany(t => t.Login.LoginAccessFuncstruct.Where(y => y.Package.LookupVal.ToUpper() == PackageCode).ToList()).ToList();
        //                        db.LoginAccessFuncstruct.RemoveRange(LoginAccessFuncstructList);
        //                        List<LoginAccessPaystruct> LoginAccessPaystructList = EmpId.SelectMany(t => t.Login.LoginAccessPaystruct.Where(y => y.Package.LookupVal.ToUpper() == PackageCode).ToList()).ToList();
        //                        db.LoginAccessPaystruct.RemoveRange(LoginAccessPaystructList);

        //                        await db.SaveChangesAsync();
        //                    }


        //                }
        //                ts.Complete();
        //                return this.Json(new { status = true, responseText = "Data removed.", JsonRequestBehavior.AllowGet });



        //            }
        //            catch (RetryLimitExceededException /* dex */)
        //            {
        //                //Log the error (uncomment dex variable name and add a line here to write a log.)
        //                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
        //                //return RedirectToAction("Delete");
        //                return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
        //            }
        //            catch (Exception ex)
        //            {
        //                LogFile Logfile = new LogFile();
        //                ErrorLog Err = new ErrorLog()
        //                {
        //                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                    ExceptionMessage = ex.Message,
        //                    ExceptionStackTrace = ex.StackTrace,
        //                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                    LogTime = DateTime.Now
        //                };
        //                Logfile.CreateLogFile(Err);
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //    }
        //}



        //[HttpPost]
        //public ActionResult BA_SubCategoryDetails(List<int> SkipIds)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        //List<string> Ids = SkipIds.ToString();
        //        var fall = db.BA_SubCategory.ToList();

        //        if (SkipIds != null)
        //        {
        //            foreach (var a in SkipIds)
        //            {
        //                if (fall == null)
        //                    fall = db.BA_SubCategory.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
        //                else
        //                    fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
        //            }
        //        }
        //        var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

        //        return Json(r, JsonRequestBehavior.AllowGet);
        //    }
        //}


        //[HttpPost]
        //public ActionResult GetLookupStageDetails(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        //var fall = db.Address.Include(e => e.Country).Include(e => e.State).Include(e => e.StateRegion)
        //        //    .Include(e => e.District).Include(e => e.Taluka).Include(e => e.City).Include(e => e.Area).ToList();
        //        var falls = db.EmpDisciplineProcedings.Select(t => new 
        //                                        {
        //                                            MisconductComplaint = t.MisconductComplaint,
        //                                            PostEnquiryPrerquisite = t.PostEnquiryPrerquisite,
        //                                            PreminaryEnquiry = t.PreminaryEnquiry,
        //                                            PreminaryEnquiryAction = t.PreminaryEnquiryAction,
        //                                            PunishmentOrder = t.PunishmentOrder,
        //                                            PunishmentOrderApeal = t.PunishmentOrderApeal,
        //                                            PunishmentOrderApealReply = t.PunishmentOrderApealReply,
        //                                            PunishmentOrderDelivery = t.PunishmentOrderDelivery,
        //                                            PunishmentOrderImplementation = t.PunishmentOrderImplementation,
        //                                            ChargeSheet = t.ChargeSheet,
        //                                            ChargeSheetEnquiryNotice = t.ChargeSheetEnquiryNotice,
        //                                            ChargeSheetEnquiryNoticeServing = t.ChargeSheetEnquiryNoticeServing,
        //                                            ChargeSheetEnquiryProceedings = t.ChargeSheetEnquiryProceedings,
        //                                            ChargeSheetEnquiryReport = t.ChargeSheetEnquiryReport,
        //                                            ChargeSheetReply = t.ChargeSheetReply,
        //                                            ChargeSheetServing = t.ChargeSheetServing,
        //                                            FinalShowCauseNotice = t.FinalShowCauseNotice,
        //                                            t.FinalShowCauseNoticeClarificarionServing,
        //                                            t.FinalShowCauseNoticeClarification,
        //                                            t.FinalShowCauseNoticeReply,
        //                                            t.FinalShowCauseNoticeServing

        //                                        }).ToList();

        //        IEnumerable<EmpDisciplineProcedings> all = null;


        //            List<EmpDisciplineProcedings> model = new List<EmpDisciplineProcedings>();
        //            EmpDisciplineProcedings view = null;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            //all = db.Address.ToList().Where(d => d.FullAddress.Contains(data));
        //            var getCaseId = db.EmpDisciplineProcedings.Where(e => e.Id == Convert.ToInt32(data)).FirstOrDefault();
        //            all = falls

        //        }
        //        else
        //        {
        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.Address3 }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }

        //}

    }
}