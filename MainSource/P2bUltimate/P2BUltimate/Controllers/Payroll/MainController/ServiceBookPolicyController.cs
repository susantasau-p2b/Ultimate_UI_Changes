using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using Payroll;
using P2BUltimate.Process;
using System.Diagnostics;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class ServiceBookPolicyController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ServiceBookPolicy/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult PopulateDropDownFunction(string data, string data2, string data3)
        {
            //data parent-id
            //data2-empid
            //data3-type
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = null;
                int CompId = Convert.ToInt32(Session["CompId"]);
                int EmpId = Convert.ToInt32(data2);
                data3 = data3.ToUpper();
                switch (data3)
                {
                    case "OTHERACTIVITY":
                        var OPayscale = db.Employee.Where(e => e.Id == EmpId)//modified by prashant 15042017
                            .Select(r => r.EmpOffInfo.PayScale).FirstOrDefault();//modified by prashant 15042017
                        var OPromo = db.PayScaleAgreement.Where(e => e.EndDate == null && e.PayScale.Id == OPayscale.Id)
                            .Include(e => e.OthServiceBookActivity)
                            .SingleOrDefault();//modified by prashant 15042017
                        // .Select(r => r.OthServiceBookActivity).FirstOrDefault();//modified by prashant 15042017
                        var qurey = OPromo.OthServiceBookActivity.ToList();//modified by prashant 15042017
                        var selected = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected = Convert.ToInt32(data2);
                        }

                        s = new SelectList(qurey, "Id", "Name");
                        return Json(s, JsonRequestBehavior.AllowGet);
                        break;
                    case "PROMOTIONACTIVITY":
                        var pPayscale = db.Employee.Where(e => e.Id == EmpId)//modified by prashant 15042017
                           .Select(r => r.EmpOffInfo.PayScale).FirstOrDefault();//modified by prashant 15042017
                        var pPromo = db.PayScaleAgreement.Where(e => e.EndDate == null && e.PayScale.Id == pPayscale.Id)
                            .Include(e => e.PromoActivity)
                            .SingleOrDefault();
                        var pqurey = pPromo.PromoActivity.ToList();
                        var pselected = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected = Convert.ToInt32(data2);
                        }

                        s = new SelectList(pqurey, "Id", "Name");
                        return Json(s, JsonRequestBehavior.AllowGet);
                        break;
                    case "PROMOTIONPOLICY":
                        int pPolicyId = Convert.ToInt32(data);
                        List<PromoPolicy> pOthSerBookPly = new List<PromoPolicy>();
                        var pPolicy = db.PromoActivity.Where(e => e.Id == pPolicyId)
                            .Include(e => e.PromoPolicy)
                            .SingleOrDefault();//modified by prashant 15042017
                        // .Select(r => r.OthServiceBookActivity).FirstOrDefault();//modified by prashant 15042017
                        // var qurey1 = OPolicy.OthServiceBookPolicy;//modified by prashant 15042017
                        pOthSerBookPly.Add(pPolicy.PromoPolicy);
                        var pselected1 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            pselected1 = "";
                        }

                        s = new SelectList(pOthSerBookPly, "Id", "Name", pselected1);
                        return Json(s, JsonRequestBehavior.AllowGet);
                        break;
                    case "OTHERPOLICY":
                        int PolicyId = Convert.ToInt32(data);
                        List<OthServiceBookPolicy> OthSerBookPly = new List<OthServiceBookPolicy>();
                        var OPolicy = db.OthServiceBookActivity.Where(e => e.Id == PolicyId)
                            .Include(e => e.OthServiceBookPolicy)
                            .SingleOrDefault();//modified by prashant 15042017
                        // .Select(r => r.OthServiceBookActivity).FirstOrDefault();//modified by prashant 15042017
                        // var qurey1 = OPolicy.OthServiceBookPolicy;//modified by prashant 15042017
                        OthSerBookPly.Add(OPolicy.OthServiceBookPolicy);
                        var selected1 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected1 = "";
                        }

                        s = new SelectList(OthSerBookPly, "Id", "Name", selected1);
                        return Json(s, JsonRequestBehavior.AllowGet);
                        break;
                    case "INCRACTIVITY":
                        var OPayscaleTa = db.Employee.Where(e => e.Id == EmpId)//modified by prashant 15042017
                            .Select(r => r.EmpOffInfo.PayScale).FirstOrDefault();//modified by prashant 15042017
                        var OTransa = db.PayScaleAgreement.Where(e => e.EndDate == null && e.PayScale.Id == OPayscaleTa.Id)
                            .Include(e => e.IncrActivity)
                            .SingleOrDefault();
                        var TransQuerya = OTransa.IncrActivity.ToList();//modified by prashant 15042017
                        var selectedTa = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selectedTa = Convert.ToInt32(data2);
                        }
                        s = new SelectList(TransQuerya, "Id", "Name");
                        return Json(s, JsonRequestBehavior.AllowGet);
                        break;
                    case "INCRPOLICY":
                        int TransPolicyIdb = Convert.ToInt32(data);
                        List<IncrPolicy> TransSerBookPlyb = new List<IncrPolicy>();
                        var OPolicyTb = db.IncrActivity.Where(e => e.Id == TransPolicyIdb)
                            .Include(e => e.IncrPolicy)
                            .SingleOrDefault();//modified by prashant 15042017
                        // .Select(r => r.OthServiceBookActivity).FirstOrDefault();//modified by prashant 15042017
                        // var qurey1 = OPolicy.OthServiceBookPolicy;//modified by prashant 15042017
                        TransSerBookPlyb.Add(OPolicyTb.IncrPolicy);
                        var selectedTPb = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected1 = "";
                        }

                        s = new SelectList(TransSerBookPlyb, "Id", "Name", selectedTPb);
                        return Json(s, JsonRequestBehavior.AllowGet);
                        break;
                    case "TRANSACTIVITY":
                        var OPayscaleT = db.Employee.Where(e => e.Id == EmpId)//modified by prashant 15042017
                            .Select(r => r.EmpOffInfo.PayScale).FirstOrDefault();//modified by prashant 15042017
                        var OTrans = db.PayScaleAgreement.Where(e => e.EndDate == null && e.PayScale.Id == OPayscaleT.Id)
                            .Include(e => e.TransActivity)
                            .SingleOrDefault();//modified by prashant 15042017
                        // .Select(r => r.OthServiceBookActivity).FirstOrDefault();//modified by prashant 15042017
                        var TransQuery = OTrans.TransActivity.ToList();//modified by prashant 15042017
                        var selectedT = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selectedT = Convert.ToInt32(data2);
                        }

                        s = new SelectList(TransQuery, "Id", "Name");
                        return Json(s, JsonRequestBehavior.AllowGet);
                        break;
                    case "TRANSPOLICY":
                        int TransPolicyId = Convert.ToInt32(data);
                        List<TransPolicy> TransSerBookPly = new List<TransPolicy>();
                        var OPolicyT = db.TransActivity.Where(e => e.Id == TransPolicyId)
                            .Include(e => e.TranPolicy)
                            .SingleOrDefault();//modified by prashant 15042017
                        // .Select(r => r.OthServiceBookActivity).FirstOrDefault();//modified by prashant 15042017
                        // var qurey1 = OPolicy.OthServiceBookPolicy;//modified by prashant 15042017
                        TransSerBookPly.Add(OPolicyT.TranPolicy);
                        var selectedTP = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected1 = Convert.ToInt32(data2);
                        }

                        s = new SelectList(TransSerBookPly, "Id", "Name", selectedTP);
                        return Json(s, JsonRequestBehavior.AllowGet);
                        break;
                    case "PAYSCALE":
                        var OPayscaleData = db.Company.Where(e => e.Id == CompId)//modified by prashant 15042017
                            .Select(r => r.PayScale).SingleOrDefault();//modified by prashant 15042017
                        var qurey2 = OPayscaleData.ToList();//modified by prashant 15042017
                        var selected2 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }

                        s = new SelectList(qurey2, "Id", "Name", selected2);
                        return Json(s, JsonRequestBehavior.AllowGet);
                        break;
                    case "PAYSCALEAGREEMENT":
                        int PayscaleId = Convert.ToInt32(data);
                        var OAgreementData = db.PayScaleAgreement.Where(e => e.PayScale.Id == PayscaleId && e.EndDate != null)//modified by prashant 15042017
                            .ToList();//modified by prashant 15042017
                        var qurey3 = OAgreementData.ToList();//modified by prashant 15042017
                        var selected3 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected3 = Convert.ToInt32(data2);
                        }

                        s = new SelectList(qurey3, "Id", "Name", selected3);
                        return Json(s, JsonRequestBehavior.AllowGet);
                        break;

                    case "NEWJOBSTATUS":

                        var EmpData = db.Employee.Where(e => e.Id == EmpId)
                            .Select(e => new { grade = e.PayStruct.Grade, level = e.PayStruct.Level }).SingleOrDefault();
                        var gradeid = EmpData.grade == null ? 0 : EmpData.grade.Id;
                        var levelid = EmpData.level == null ? 0 : EmpData.level.Id;
                        if (levelid != 0)
                        {
                            var fun_data = db.PayStruct.Include(e => e.JobStatus.EmpStatus).Include(e => e.JobStatus.EmpActingStatus).Where(e => e.Grade.Id == gradeid && e.Level.Id == levelid)
                                .Select(e => new
                                {
                                    code = e.Id.ToString(),
                                    value = "EmpStatus:" + e.JobStatus.EmpStatus.LookupVal.ToUpper() + ",EmpActingStatus: " + e.JobStatus.EmpActingStatus.LookupVal.ToUpper()

                                }).ToList();
                            //s = new SelectList(fun_data, "Id", "Name", "");
                            return Json(fun_data, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var fun_data = db.PayStruct.Include(e => e.JobStatus.EmpStatus).Include(e => e.JobStatus.EmpActingStatus).Where(e => e.Grade.Id == gradeid && e.JobStatus != null)
                                .Select(e => new
                                {
                                    code = e.Id.ToString(),
                                    value = "EmpStatus:" + e.JobStatus.EmpStatus.LookupVal.ToUpper() + ",EmpActingStatus: " + e.JobStatus.EmpActingStatus.LookupVal.ToUpper()

                                }).ToList();
                            return Json(fun_data, JsonRequestBehavior.AllowGet);

                        }

                        break;

                    case "OLDJOBSTATUS":

                        var EmpData1 = db.Employee.Where(e => e.Id == EmpId)
                            .Select(e => new { id = e.PayStruct.Id, grade = e.PayStruct.Grade, level = e.PayStruct.Level }).SingleOrDefault();
                        var grid = EmpData1.grade == null ? 0 : EmpData1.grade.Id;
                        var lVLid = EmpData1.level == null ? 0 : EmpData1.level.Id;
                        if (lVLid != 0)
                        {
                            var fun_data = db.PayStruct.Include(e => e.JobStatus.EmpStatus).Include(e => e.JobStatus.EmpActingStatus).Where(e => e.Id == EmpData1.id)
                                .Select(e => new
                                {
                                    code = e.Id.ToString(),
                                    value = "EmpStatus:" + e.JobStatus.EmpStatus.LookupVal.ToUpper() + ",EmpActingStatus: " + e.JobStatus.EmpActingStatus.LookupVal.ToUpper()

                                }).ToList();
                            s = new SelectList(fun_data, "Id", "Name", "");
                        }
                        else
                        {
                            var fun_data = db.PayStruct.Include(e => e.JobStatus.EmpStatus).Include(e => e.JobStatus.EmpActingStatus).Where(e => e.Id == EmpData1.id && e.JobStatus != null)
                                .Select(e => new
                                {
                                    code = e.Id.ToString(),
                                    value = "EmpStatus:" + e.JobStatus.EmpStatus.LookupVal.ToUpper() + ",EmpActingStatus: " + e.JobStatus.EmpActingStatus.LookupVal.ToUpper()

                                }).ToList();
                            return Json(fun_data, JsonRequestBehavior.AllowGet);

                        }

                        break;
                    case "NEWPAY":
                        var pay_data = db.PayStruct.Where(e => e.Company.Id == CompId)
                        .Select(e => new
                        {
                            code = e.Id.ToString(),
                            value = "Grade:" + e.Grade.Code.ToString() + " - " + e.Grade.Name.ToString() + ", Level:" + e.Level.Name.ToString() + ",JobStatus:" + e.JobStatus.FullDetails,
                        }).ToList();

                        return Json(pay_data, JsonRequestBehavior.AllowGet);
                    case "NEWGEO":
                        var geo_data = db.GeoStruct.Where(e => e.Company.Id == CompId && e.Location != null)
                        .Select(e => new
                        {
                            code = e.Id.ToString(),
                            value = e.Location.LocationObj.LocCode.ToString() + " " + e.Location.LocationObj.LocDesc.ToString() + " " + e.Department.DepartmentObj.DeptDesc.ToString()
                        }).ToList();

                        return Json(geo_data, JsonRequestBehavior.AllowGet);
                    case "NEWFUNC":
                        var func_data = db.FuncStruct.Where(e => e.Company.Id == CompId)
                        .Select(e => new
                        {
                            code = e.Id.ToString(),
                            value = e.Job.Code + " " + e.Job.Name + " " + e.JobPosition.JobPositionDesc
                        }).ToList();

                        return Json(func_data, JsonRequestBehavior.AllowGet);
                    case "OLDFUNC":

                        var EmpData2 = db.Employee.Where(e => e.Id == EmpId).Include(e => e.FuncStruct).SingleOrDefault();

                        var Oldfunc_data = db.FuncStruct.Where(e => e.Company.Id == CompId && e.Id == EmpData2.FuncStruct.Id)
                        .Select(e => new
                        {
                            code = e.Id.ToString(),
                            value = e.Id.ToString() + " " + e.Job.Name + " " + e.JobPosition.JobPositionDesc
                        }).ToList();

                        return Json(Oldfunc_data, JsonRequestBehavior.AllowGet);

                    case "OLDPAYSCALE":
                        var OldPayscaleData = db.Company.Where(e => e.Id == CompId)
                       .Select(r => r.PayScale).SingleOrDefault();
                        var oldpayscale_qurey2 = OldPayscaleData.ToList();
                        var oldpayscale_selected2 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }

                        s = new SelectList(oldpayscale_qurey2, "Id", "Name", oldpayscale_selected2);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    case "OLDPAYSCALEAGREEMENT":
                        int Old_payscaleagreement_PayscaleId = Convert.ToInt32(data);
                        var Old_payscaleagreement_OAgreementData = db.PayScaleAgreement.Where(e => e.PayScale.Id == Old_payscaleagreement_PayscaleId && e.EndDate != null)//modified by prashant 15042017
                            .ToList();
                        var Old_payscaleagreement_qurey3 = Old_payscaleagreement_OAgreementData.ToList();//modified by prashant 15042017
                        var Old_payscaleagreement_selected3 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            Old_payscaleagreement_selected3 = Convert.ToInt32(data2);
                        }

                        s = new SelectList(Old_payscaleagreement_qurey3, "Id", "Name", Old_payscaleagreement_selected3);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    case "NEWPAYSCALE":
                        var new_payscalePayscaleData = db.Company.Where(e => e.Id == CompId)
                    .Select(r => r.PayScale).SingleOrDefault();
                        var new_payscalequrey2 = new_payscalePayscaleData.ToList();
                        var new_payscaleselected2 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }

                        s = new SelectList(new_payscalequrey2, "Id", "Name", new_payscaleselected2);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    case "NEWPAYSCALEAGREEMENT":
                        int New_payscaleagreement_PayscaleId = Convert.ToInt32(data);
                        var New_payscaleagreement_OAgreementData = db.PayScaleAgreement.Where(e => e.PayScale.Id == New_payscaleagreement_PayscaleId && e.EndDate != null)
                         .ToList();
                        var New_payscaleagreement_qurey3 = New_payscaleagreement_OAgreementData.ToList();//modified by prashant 15042017
                        var New_payscaleagreement_selected3 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            Old_payscaleagreement_selected3 = Convert.ToInt32(data2);
                        }

                        s = new SelectList(New_payscaleagreement_qurey3, "Id", "Name", New_payscaleagreement_selected3);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    case "EXTNREDNACTIVITY":
                        var OPayscaleTe = db.Employee.Where(e => e.Id == EmpId)//modified by prashant 15042017
                            .Select(r => r.EmpOffInfo.PayScale).FirstOrDefault();//modified by prashant 15042017
                        var OExtnRedn = db.PayScaleAgreement.Where(e => e.EndDate == null && e.PayScale.Id == OPayscaleTe.Id)
                            .Include(e => e.ExtnRednActivity)
                            .SingleOrDefault();//modified by prashant 15042017
                        // .Select(r => r.OthServiceBookActivity).FirstOrDefault();//modified by prashant 15042017
                        var ExtnRednQuery = OExtnRedn.ExtnRednActivity.ToList();//modified by prashant 15042017
                        var selectedTe = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selectedTe = Convert.ToInt32(data2);
                        }

                        s = new SelectList(ExtnRednQuery, "Id", "Name");
                        return Json(s, JsonRequestBehavior.AllowGet);
                        break;
                    case "EXTNREDNPOLICY":
                        int ExtnRednPolicyId = Convert.ToInt32(data);
                        List<ExtnRednPolicy> ExtnRednSerBookPly = new List<ExtnRednPolicy>();
                        var OExtnRednPolicyT = db.ExtnRednActivity.Where(e => e.Id == ExtnRednPolicyId)
                            .Include(e => e.ExtnRednPolicy)
                            .SingleOrDefault();//modified by prashant 15042017
                        // .Select(r => r.OthServiceBookActivity).FirstOrDefault();//modified by prashant 15042017
                        // var qurey1 = OPolicy.OthServiceBookPolicy;//modified by prashant 15042017
                        ExtnRednSerBookPly.Add(OExtnRednPolicyT.ExtnRednPolicy);
                        var selectedTE = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selectedTE = Convert.ToInt32(data2);
                        }

                        s = new SelectList(ExtnRednSerBookPly, "Id", "Name", selectedTE);
                        return Json(s, JsonRequestBehavior.AllowGet);
                        break;
                    default:
                        return null;
                }

                //var qurey = db.PromoActivity.ToList();//modified by prashant 15042017
                // var qurey = db.OthServiceBookActivity.ToList();//modified by prashant 15042017

                return null;
                //return View();
            }
        }

        public ActionResult PopulateDropDownFunctionForIncr(string data, string data2, string data3)
        {
            //data parent-id
            //data2-empid
            //data3-type
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = null;
              //  int CompId = Convert.ToInt32(Session["CompId"]);
              //  int EmpId = Convert.ToInt32(data2);
                data3 = data3.ToUpper();
                var selected1 = (Object)null;
                switch (data3)
                {
                    
                    case "INCRACTIVITY":
                        //var OPayscaleTa = db.Employee.Where(e => e.Id == EmpId)//modified by prashant 15042017
                        //    .Select(r => r.EmpOffInfo.PayScale).FirstOrDefault();//modified by prashant 15042017

                        var OTransa = db.PayScaleAgreement.Where(e => e.EndDate == null)
                            .Include(e => e.IncrActivity)
                            .SingleOrDefault();
                        var TransQuerya = OTransa.IncrActivity.ToList();//modified by prashant 15042017
                        var selectedTa = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selectedTa = Convert.ToInt32(data2);
                        }
                        s = new SelectList(TransQuerya, "Id", "Name");
                        return Json(s, JsonRequestBehavior.AllowGet);
                        break;
                    case "INCRPOLICY":
                        int TransPolicyIdb = Convert.ToInt32(data);
                        List<IncrPolicy> TransSerBookPlyb = new List<IncrPolicy>();
                        var OPolicyTb = db.IncrActivity.Where(e => e.Id == TransPolicyIdb)
                            .Include(e => e.IncrPolicy)
                            .SingleOrDefault();//modified by prashant 15042017
                        // .Select(r => r.OthServiceBookActivity).FirstOrDefault();//modified by prashant 15042017
                        // var qurey1 = OPolicy.OthServiceBookPolicy;//modified by prashant 15042017
                        TransSerBookPlyb.Add(OPolicyTb.IncrPolicy);
                        var selectedTPb = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected1 = "";
                        }

                        s = new SelectList(TransSerBookPlyb, "Id", "Name", selectedTPb);
                        return Json(s, JsonRequestBehavior.AllowGet);
                        break;
                   
                    default:
                        return null;
                }

                //var qurey = db.PromoActivity.ToList();//modified by prashant 15042017
                // var qurey = db.OthServiceBookActivity.ToList();//modified by prashant 15042017

                return null;
                //return View();
            }
        }

    }
}