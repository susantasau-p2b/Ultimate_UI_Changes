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
using P2B.SERVICES.Factory;
using P2B.SERVICES.Interface;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class PromotionServiceBookController : Controller
    {
         readonly IP2BINI p2BINI;
         readonly LevelSettings LevelSettings;

         private readonly Type Type;

        public PromotionServiceBookController()
        {
            p2BINI = P2BINI.RegisterSettings();
            LevelSettings = new LevelSettings(p2BINI.GetSectionValues("LevelSettings"));
            Type = typeof(PromotionServiceBookController); 
        }

        // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/PromotionServiceBook/Index.cshtml");
        }
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        #region DDLIST

        public ActionResult PopulateDropDownActivityList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var qurey = db.IncrActivity.Include(e => e.IncrList).ToList();
                int OEmp_Id = Convert.ToInt32(data2);
                var query = db.EmployeePolicyStruct.AsNoTracking().Where(e => e.EmployeePayroll.Employee.Id == OEmp_Id && e.EndDate == null).Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.EmployeePolicyStructDetails)
                    .Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula)).Include(e => e.EmployeePolicyStructDetails.
                    Select(r => r.PolicyFormula.PromoActivity)).FirstOrDefault();
                var PromoActList = query.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.PromoActivity);

                var selected = (Object)null;
                //if (data2 != "" && data != "0" && data2 != "0")
                //{
                //    selected = Convert.ToInt32(data2);
                //}

                List<PromoActivity> PromoAct = new List<PromoActivity>();
                if (PromoActList.Count() > 0)
                {
                    foreach (var item in PromoActList)
                    {
                        if (item.FirstOrDefault() != null)
                        {
                            foreach (var item1 in item)
                            {
                                PromoAct.Add(item1);
                            }
                        }

                    }
                }
                SelectList s = new SelectList(PromoAct, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
            //using (DataBaseContext db = new DataBaseContext())
            //{
            //    var qurey = db.PromoActivity.ToList();
            //    var selected = (Object)null;
            //    if (data2 != "" && data != "0" && data2 != "0")
            //    {
            //        selected = Convert.ToInt32(data2);
            //    }

            //    SelectList s = new SelectList(qurey, "Id", "Name", selected);
            //    return Json(s, JsonRequestBehavior.AllowGet);
            //}
        }
        public ActionResult PopulateDropDownPayscaleAgreementList(string data2, string data3)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.PayScaleAgreement.Include(e => e.PayScale.PayScaleType).ToList();


                var selected = (Object)null;
                if (data3 != "" && data2 != "0" && data3 != "0")
                {
                    selected = Convert.ToInt32(data3);
                }

                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDropDownPayscaleList(string data2, string data3)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.PayScale.Include(e => e.PayScaleArea).Include(e => e.PayScaleType
                    ).Include(e => e.PayScaleArea.Select(r => r.LocationObj)).ToList();


                var selected = (Object)null;
                if (data3 != "" && data2 != "0" && data3 != "0")
                {
                    selected = Convert.ToInt32(data3);
                }

                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetStruct(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var emp_id = Convert.ToInt32(data);
                var emp_data = db.Employee.Include(e => e.PayStruct)
                                    .Include(e => e.FuncStruct)
                                    .Include(e => e.GeoStruct)
                                    .Where(e => e.Id == emp_id).SingleOrDefault();
                var pay_data = (Object)null;
                if (emp_data.PayStruct != null)
                {
                    pay_data = db.PayStruct
                        .Where(e => e.Id == emp_data.PayStruct.Id)
                       .Select(e => new
                       {
                           code = e.Id.ToString(),
                           value = e.Grade.Code + " - " + e.Grade.Name.ToString() + " " + e.Level.Name.ToString()
                       }).ToList();

                }
                var fun_data = (Object)null;
                if (emp_data.FuncStruct != null)
                {

                    fun_data = db.FuncStruct.Where(e => e.Id == emp_data.FuncStruct.Id).Select(e => new
                    {
                        code = e.Id.ToString(),
                        value = e.Job.Code + " - " + e.Job.Name.ToString() + "" + e.JobPosition.JobPositionDesc.ToString(),

                    }).ToList();
                }
                var geo_data = (Object)null;
                if (emp_data.GeoStruct != null)
                {
                    geo_data = db.GeoStruct.Where(e => e.Id == emp_data.GeoStruct.Id).Select(e => new
                    {
                        Code = e.Id.ToString(),
                        value = e.Corporate.Name + e.Region.Name + e.Company.FullDetails + "Division : " + e.Division.FullDetails + " "
                        + "Location : " + e.Location.FullDetails + " Department : " + e.Department.FullDetails
                    }).ToList();
                }
                var jsondata = new
                {
                    pay = pay_data,
                    fun = fun_data,
                    geo = geo_data,
                };
                //var geo_data = db.GeoStruct.Where(e=>e.Id==emp_data.GeoStruct.Id)
                return Json(new { jsondata }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDropDownPayscaleAgreementListOld(string data2, string data3)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data2 != null && data2 != "")
                {
                    var id = Convert.ToInt32(data2);
                    var query = db.EmployeePayroll.Include(e => e.EmpSalStruct.Select(a => a.EmpSalStructDetails))
                        .Include(e => e.EmpSalStruct.Select(a => a.EmpSalStructDetails.Select(z => z.PayScaleAssignment.PayScaleAgreement)))
                                .Where(e => e.Employee.Id == id)
                        .SingleOrDefault();
                    var payscaleagreementlist = new List<PayScaleAgreement>();
                    var selected = "";
                    foreach (var item in query.EmpSalStruct)
                    {
                        foreach (var item1 in item.EmpSalStructDetails)
                        {
                            payscaleagreementlist.Add(item1.PayScaleAssignment.PayScaleAgreement);
                            selected = item1.PayScaleAssignment.PayScaleAgreement.Id.ToString();
                            break;
                        }
                        break;
                    }

                    SelectList s = new SelectList(payscaleagreementlist, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    return null;
                }
            }
        }
        public ActionResult PopulateDropDownPayscaleListOld(string data2, string data3)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data2 != null && data2 != "")
                {
                    var id = Convert.ToInt32(data2);
                    var qurey = db.Employee
                        .Include(e => e.EmpOffInfo)
                        .Include(e => e.EmpOffInfo.PayScale)
                        .Include(e => e.EmpOffInfo.PayScale.PayScaleType)
                        .Include(e => e.EmpOffInfo.PayScale.PayScaleArea.Select(r => r.LocationObj))
                        .Where(e => e.Id == id).SingleOrDefault();


                    var selected = (Object)null;
                    if (data3 != "" && data2 != "0" && data3 != "0")
                    {
                        selected = Convert.ToInt32(data3);
                    }
                    var paylist = new List<PayScale>();
                    if (qurey.EmpOffInfo != null && qurey.EmpOffInfo.PayScale != null)
                    {

                        paylist.Add(qurey.EmpOffInfo.PayScale);
                        selected = qurey.EmpOffInfo.PayScale.Id;
                    }
                    SelectList s = new SelectList(paylist, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }
            }
        }
        public ActionResult Getdetails(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);


                var val = db.Employee.Select(e => e.EmpOffInfo.PayScale).Where(e => e.Id == Emp);




                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetNewFuncStructDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(SessionManager.CompanyId);

                var fun_data = db.FuncStruct.Where(e => e.Company.Id == id).Select(e => new
                {
                    code = e.Id.ToString(),
                    value = e.Job.Code + " - " + e.Job.Name.ToString() + "" + e.JobPosition.JobPositionDesc.ToString(),

                }).ToList();
                return Json(fun_data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetNewPayStructDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(SessionManager.CompanyId);
                var pay_data = db.PayStruct.Where(e => e.Company.Id == id && e.JobStatus_Id != null)
                .Select(e => new
                {
                    code = e.Id.ToString(),
                    value = e.Grade.Code + " - " + e.Grade.Name.ToString() + " " + e.Level.Name.ToString() + e.JobStatus.EmpActingStatus.LookupVal + " " + e.JobStatus.EmpStatus.LookupVal
                }).ToList();

                return Json(pay_data, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetNewGeoStructDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var geo_data = db.GeoStruct.Select(e => new
                {
                    code = e.Id.ToString(),

                    value = e.Corporate.Name + e.Region.Name + e.Company.FullDetails + "Division : " + e.Division.FullDetails + " "
                    + "Location : " + e.Location.FullDetails + " Department : " + e.Department.FullDetails

                }).ToList();

                return Json(geo_data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetEmployeeDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var geo_data = db.Employee.Select(e => new
                {
                    code = e.Id.ToString(),

                    value = "Code :" + e.EmpCode + " " + "Emp Name:" + e.EmpName.FullNameFML

                }).ToList();

                return Json(geo_data, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult getpolicy(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null || data != "")
                {
                    var id = Convert.ToInt32(data);
                    var query = db.PromoActivity.Include(e => e.PromoList).Where(e => e.PromoList.Id == id).ToList();
                    var selected = "";
                    if (data2 != "")
                    {
                        selected = data2;
                    }
                    SelectList s = new SelectList(query, "Id", "Name", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region Create
        public ActionResult Create(PromotionServiceBook PromotionServiceBook, FormCollection form, String forwarddata) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string NewFuncStruct = form["NewFuncT-table"] == "0" ? "" : form["NewFuncT-table"];
                    string NewPayScale = form["NewPayScalelist"] == "0" ? "" : form["NewPayScalelist"];
                    string NewPayScaleAgreement = form["NewPayScaleAgreementlist"] == "0" ? "" : form["NewPayScaleAgreementlist"];
                    string NewPayStruct = form["NewPayT-table"] == "0" ? "" : form["NewPayT-table"];
                    string PromotionActivity = form["PromotionActivitylist"] == "0" ? "" : form["PromotionActivitylist"];
                    string NewGeoStruct = form["NewGeoT-table"] == "0" ? "" : form["NewGeoT-table"];
                    string PromotionPolicy = form["promopolicy"] == "0" ? "" : form["promopolicy"];
                    string OldPayScaleAgreementlist = form["OldPayScaleAgreementlist"] == "0" ? "" : form["OldPayScaleAgreementlist"];
                    string ProcessPromoDate = form["ProcessPromoDate"] == "0" ? "" : form["ProcessPromoDate"];
                    string Irregular = form["Irregular"] == "0" ? "" : form["Irregular"];

                    var date = Convert.ToDateTime(ProcessPromoDate).ToString("MM/yyyy");

                    List<string> Msgs = new List<string>();

                    if (OldPayScaleAgreementlist != null && OldPayScaleAgreementlist != "")
                    {
                        var value = db.PayScaleAgreement.Find(int.Parse(OldPayScaleAgreementlist));
                        PromotionServiceBook.OldPayScaleAgreement = value;

                    }
                    int CompId = 0;
                    if (Session["CompId"] != null)
                    {
                        CompId = Convert.ToInt32(Session["CompId"]);
                    }

                    int id = 0;
                    if (Emp != null)
                    {
                        id = Emp;
                    }
                    else
                    {
                        return Json(new { sucess = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    }

                    ////////new 21/08/2019
                    var check = db.CPIEntryT.Where(e => e.PayMonth == date).ToList();

                    if (check.Count() == 0)
                    {
                        Msgs.Add("Kindly run CPI first and then try again");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                    }
                    ///////////

                    Employee OEmployee = null;

                    EmployeePayroll OEmployeePayroll = null;
                    int PayScaleAgrId = int.Parse(NewPayScaleAgreement);
                    var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();

                    OEmployee = db.Employee.Include(e => e.EmpName).Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus).Include(e => e.PayStruct.JobStatus.EmpStatus)
                                    .Include(e => e.EmpOffInfo)
                                    .Include(e => e.ServiceBookDates)
                                    .Where(r => r.Id == Emp).SingleOrDefault();
                    var seniorityapp = db.Senioritypolicy.Include(e => e.JobStatus).ToList();
                    List<int> EmpActingStatus = new List<int>();
                    if (seniorityapp != null && seniorityapp.Count() > 0)
                    {
                        foreach (var Jobst in seniorityapp)
                        {
                            var Actingjobstat = Jobst.JobStatus.ToList();
                            foreach (var item in Actingjobstat)
                            {
                                EmpActingStatus.Add(item.Id);
                            }
                        }

                      
                        // var Seniorityobj = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus).Include(e => e.PayStruct.JobStatus.EmpStatus).Where(e => e.ServiceBookDates.ServiceLastDate == null && e.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT" && (e.ServiceBookDates.SeniorityNo != "0" && e.ServiceBookDates.SeniorityNo != null && e.ServiceBookDates.SeniorityNo != "")).ToList();
                        var Seniorityobj = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus).Include(e => e.PayStruct.JobStatus.EmpStatus).Where(e => e.ServiceBookDates.ServiceLastDate == null && EmpActingStatus.Contains(e.PayStruct.JobStatus.Id) && (e.ServiceBookDates.SeniorityNo != "0" && e.ServiceBookDates.SeniorityNo != null && e.ServiceBookDates.SeniorityNo != "")).ToList();

                        if (Seniorityobj.Count() > 0)
                        {
                            var OEmployeecheck = db.Employee.Include(e => e.EmpName).Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus).Include(e => e.PayStruct.JobStatus.EmpStatus)
                                                             .Include(e => e.ServiceBookDates)
                                                             .Where(e => e.ServiceBookDates.ServiceLastDate == null && EmpActingStatus.Contains(e.PayStruct.JobStatus.Id) && (e.ServiceBookDates.SeniorityNo == "0" || e.ServiceBookDates.SeniorityNo == "" || e.ServiceBookDates.SeniorityNo == null)).ToList();
                            // .Where(e => e.ServiceBookDates.ServiceLastDate == null && e.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT" && (e.ServiceBookDates.SeniorityNo == "0" || e.ServiceBookDates.SeniorityNo == "" || e.ServiceBookDates.SeniorityNo == null)).ToList();
                            if (OEmployeecheck.Count() > 0)
                            {
                                List<string> Msg = new List<string>();
                                foreach (var item in OEmployeecheck)
                                {

                                    //if (item.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT" && (item.ServiceBookDates.SeniorityNo == "0" || item.ServiceBookDates.SeniorityNo == "" || item.ServiceBookDates.SeniorityNo == null))
                                    if ((item.ServiceBookDates.SeniorityNo == "0" || item.ServiceBookDates.SeniorityNo == "" || item.ServiceBookDates.SeniorityNo == null))
                                    {

                                        Msg.Add("Please Assign Seniority No in Personal info:" + item.EmpCode + " " + item.EmpName.FullNameFML);

                                    }
                                    //  if (item.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT" && item.ServiceBookDates.SeniorityDate == null)
                                    if (item.ServiceBookDates.SeniorityDate == null)
                                    {

                                        Msg.Add("Please Assign Seniority date in Personal info:" + item.EmpCode + " " + item.EmpName.FullNameFML);

                                    }
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    OEmployeePayroll = db.EmployeePayroll.Include(r => r.PromotionServiceBook)
                  .Where(e => e.Employee.Id == Emp).SingleOrDefault();

                    if (OEmployeePayroll.PromotionServiceBook.Any(d => d.ProcessPromoDate.Value.ToShortDateString() == ProcessPromoDate.ToString() && d.ReleaseFlag == false))
                    {
                        {
                            List<string> Msg = new List<string>();
                            Msg.Add("Please Release The Activity and Try Again:" + OEmployee.EmpCode + " " + OEmployee.EmpName.FullNameFML + " on date= " + ProcessPromoDate);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    int PromActivityIds = 0;

                    if (PromotionActivity != null && PromotionActivity != "")
                    {
                        PromActivityIds = int.Parse(PromotionActivity);
                    }

                    if (OEmployeePayroll.PromotionServiceBook.Any(a => a.ProcessPromoDate.Value.ToShortDateString() == ProcessPromoDate.ToString() && a.PromotionActivity_Id == PromActivityIds))
                    {
                        List<string> Msg = new List<string>();
                        Msg.Add("Alredy PromotionServiceBook  for Date= " + ProcessPromoDate);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    if (NewFuncStruct != "" && NewFuncStruct != null)
                        PromotionServiceBook.NewFuncStruct = db.FuncStruct.Find(int.Parse(NewFuncStruct));

                    if (NewPayScale != "" && NewPayScale != null)
                        PromotionServiceBook.NewPayScale = db.PayScale.Find(int.Parse(NewPayScale));

                    if (NewPayScaleAgreement != "" && NewPayScaleAgreement != null)
                        PromotionServiceBook.NewPayScaleAgreement = db.PayScaleAgreement.Find(PayScaleAgrId);

                    if (NewPayStruct != "" && NewPayStruct != null)
                        PromotionServiceBook.NewPayStruct = db.PayStruct.Find(int.Parse(NewPayStruct));

                    if (OEmployee.FuncStruct != null)
                        PromotionServiceBook.OldFuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);

                    if (OEmployee.PayStruct.JobStatus != null)
                        PromotionServiceBook.OldJobStatus = db.JobStatus.Find(OEmployee.PayStruct.JobStatus.Id);

                    if (OEmployee.EmpOffInfo.PayScale != null)
                        PromotionServiceBook.OldPayScale = db.PayScale.Find(OEmployee.EmpOffInfo.PayScale.Id);

                    if (OEmployee.EmpOffInfo.PayScale != null)
                        if (OEmployee.PayStruct != null)
                            PromotionServiceBook.OldPayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);

                    if (OEmployee.GeoStruct != null)
                        PromotionServiceBook.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);


                    if (NewFuncStruct == "" || NewFuncStruct == null)
                    {
                        PromotionServiceBook.NewFuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);
                    }
                    if (NewPayStruct == "" || NewPayStruct == null)
                    {
                        PromotionServiceBook.NewPayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);
                    }


                    // Hindustan bank
                    var CompCode = db.Company.Where(e => e.Id == CompId).AsNoTracking().AsParallel().Select(a => a.Code.ToUpper()).SingleOrDefault();  //asd
                    if (CompCode == "HCBL")
                    {
                        int funid = Convert.ToInt32(NewFuncStruct);
                        var newjobposition = db.FuncStruct.Include(e => e.JobPosition).Where(e => e.Company.Id == CompId && e.Id == funid).FirstOrDefault();
                        var oldjobposition = db.FuncStruct.Include(e => e.JobPosition).Where(e => e.Company.Id == CompId && e.Id == OEmployee.FuncStruct.Id).FirstOrDefault();
                        if ((newjobposition.JobPosition == null ? "" : newjobposition.JobPosition.JobPositionDesc.ToUpper()) != oldjobposition.JobPosition.JobPositionDesc.ToUpper())
                        {
                            List<string> Msg = new List<string>();
                            Msg.Add("Job Position Should be Same old and new function struct ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }

                   

                    int PromoActivityPolicyId = 0;
                    int PromoActivityId = 0;
                    string LookupVal = "";
                    if (PromotionActivity != null && PromotionActivity != "")
                    {
                        PromoActivityId = int.Parse(PromotionActivity);
                        LookupVal = db.LookupValue.Where(e => e.Id == PromoActivityId).Select(e => e.LookupVal.ToUpper()).SingleOrDefault();
                    }

                    if (PromotionPolicy != null && PromotionPolicy != "")
                    {
                        PromoActivityId = int.Parse(PromotionPolicy);
                        var Newagreement = db.PayScaleAgreement.Include(x => x.PromoActivity)
                       .Include(x => x.PromoActivity.Select(y => y.PromoPolicy))
                        .Include(x => x.PromoActivity.Select(y => y.PromoList))
                       .Where(x => x.Id == PayScaleAgrId).FirstOrDefault();
                        List<PromoActivity> Promoact = Newagreement.PromoActivity.ToList();
                        PromotionServiceBook.PromotionActivity = Promoact.Where(e => e.PromoPolicy.Id == PromoActivityId).SingleOrDefault();
                        //PromotionServiceBook.PromotionActivity = db.PromoActivity.Include(e => e.PromoPolicy).Where(e => e.PromoPolicy.Id == PromoActivityId).SingleOrDefault();

                        // Checking New Funcstruct,paystruct,GeoStruct is true and tick or not
                        var qurey = Promoact.Where(e => e.PromoPolicy.Id == PromoActivityId).Select(e => e.PromoPolicy).SingleOrDefault();
                        if (qurey != null)
                        {
                            if (qurey.IsFuncStructChange == true)
                            {
                                if (NewFuncStruct == "" || NewFuncStruct == null)
                                {
                                    List<string> Msg = new List<string>();
                                    Msg.Add("Please Select new function struct ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (qurey.IsFuncStructChange == false)
                            {
                                if (OEmployee.FuncStruct != null)
                                    PromotionServiceBook.NewFuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);
                            }
                            if (qurey.IsPayStructChange == false)
                            {
                                if (OEmployee.PayStruct != null)
                                    PromotionServiceBook.NewPayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);
                            }

                            if (qurey.IsPayStructChange == true)
                            {
                                if (NewPayStruct == "" || NewPayStruct == null)
                                {
                                    List<string> Msg = new List<string>();
                                    Msg.Add("Please Select new paystruct struct ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                                if (LevelSettings != null && LevelSettings.CCode == CompCode)
                                {
                                    int payid = Convert.ToInt32(NewPayStruct);
                                    var newlevel = db.PayStruct.Include(e => e.Level).Where(e => e.Company.Id == CompId && e.Id == payid).FirstOrDefault();
                                    var oldlevel = db.PayStruct.Include(e => e.Level).Where(e => e.Company.Id == CompId && e.Id == PromotionServiceBook.NewPayStruct.Id).FirstOrDefault();
                                    if ((newlevel.Level == null ? "" : newlevel.Level.Code.ToUpper()) != (oldlevel.Level == null ? "" : oldlevel.Level.Code.ToUpper()))
                                    {
                                        List<string> Msg = new List<string>();
                                        Msg.Add("Level Should be Same old and new pay struct ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                        }

                    }


                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                      new System.TimeSpan(0, 30, 0)))
                    {
                        PromotionServiceBook.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        if (true)
                        {
                            //irregular

                        }
                        else
                        {

                        }
                        PromotionServiceBook PromoServBook = new PromotionServiceBook()
                        {
                            EmployeeCTC = null,
                            Fittment = 0.0,
                            GeoStruct = PromotionServiceBook.GeoStruct,
                            IncrNewBasic = 0.0,
                            IncrOldBasic = 0.0,
                            Narration = PromotionServiceBook.Narration,
                            NewBasic = 0.0,
                            NewFuncStruct = PromotionServiceBook.NewFuncStruct,
                            NewPayScale = PromotionServiceBook.NewPayScale,
                            NewPayScaleAgreement = PromotionServiceBook.NewPayScaleAgreement,
                            NewPayStruct = PromotionServiceBook.NewPayStruct,
                            OldBasic = PromotionServiceBook.OldBasic,
                            OldFuncStruct = PromotionServiceBook.OldFuncStruct,
                            OldJobStatus = PromotionServiceBook.OldJobStatus,
                            OldPayScale = PromotionServiceBook.OldPayScale,
                            OldPayScaleAgreement = PromotionServiceBook.OldPayScaleAgreement,
                            OldPayStruct = PromotionServiceBook.OldPayStruct,
                            ProcessPromoDate = PromotionServiceBook.ProcessPromoDate,
                            PromotionActivity = PromotionServiceBook.PromotionActivity,
                            ReleaseDate = null,
                            ReleaseFlag = false,
                            StagnancyCount = 0,
                            StagnancyAppl = false,
                            DBTrack = PromotionServiceBook.DBTrack
                        };


                        db.PromotionServiceBook.Add(PromoServBook);
                        db.SaveChanges();


                        List<PromotionServiceBook> PromoBkList = new List<PromotionServiceBook>();
                        PromoBkList.AddRange(OEmployeePayroll.PromotionServiceBook);
                        PromoBkList.Add(PromoServBook);
                        OEmployeePayroll.PromotionServiceBook = PromoBkList;
                        db.EmployeePayroll.Attach(OEmployeePayroll);
                        db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;

                        // DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
                        Process.ServiceBook.ServiceBookProcess("",CompId, "PROMOTION_PROCESS", null, PromoServBook.Id, null, null, OEmployeePayroll.Id, "REGULAR", PromotionServiceBook.ProcessPromoDate, true, false, PayScaleAgrId, null, true);
                        //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
                        Msgs.Add("Data Saved successfully");
                        //var reporting_chk = db.Employee.Include(e => e.ReportingStructRights).Where(e => e.Id == OEmployee.Id).SingleOrDefault();
                        //if (reporting_chk.ReportingStructRights.Count() > 0)
                        //{
                        //    Msgs.Add("Reporting is also define for employee,Please change ");
                        //}
                        //var EmpReportingTimingStruct = db.EmployeeAttendance.Where(e => e.Employee.Id == OEmployee.Id).Include(e => e.EmpReportingTimingStruct).SingleOrDefault();
                        //if (EmpReportingTimingStruct != null && EmpReportingTimingStruct.EmpReportingTimingStruct.Count > 0)
                        //{
                        //    Msgs.Add("Attendance Reporting is also define for employee,Please change ");
                        //}
                        ts.Complete();
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

                    }
                }
                catch (Exception ex)
                {
                    // DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);

                    List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);

                    // return Json(new Object[] { "", "", Msg }, JsonRequestBehavior.AllowGet);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion


        public class DeserializeClassManual
        {
            public String Id { get; set; }
            public String SalaryHead { get; set; }
            public String Amount { get; set; }
            public String Frequency { get; set; }
            public String Type { get; set; }
            public String SalHeadOperationType { get; set; }
            public String SalaryHeadId { get; set; }
        }

        public ActionResult GetNonstdData(string data, string data1)
        {
            string Msg = "";
            List<EditData> model = new List<EditData>();
            List<EditData> modelNew = new List<EditData>();
            var view = new EditData();

            using (DataBaseContext db = new DataBaseContext())
            {
                bool EditAppl = true;
                string FormulaActive = "Y";
                string FormulaType = "";
                List<int> extraeditdata = one_ids(data);
                List<int> Promo_Id = one_ids(data1);
                if (extraeditdata.Count() == 1 && Promo_Id.Count() == 1)
                {
                    int EmpId = extraeditdata.FirstOrDefault();
                    int PromoId = Promo_Id.FirstOrDefault();
                    if (EmpId != 0 && PromoId != 0)
                    {
                        EmpSalStruct Onewstruct = new EmpSalStruct();
                        var PromoBook = db.PromotionServiceBook.Find(PromoId);
                        int EmpPayrollId = db.EmployeePayroll.Where(e => e.Employee_Id == EmpId).FirstOrDefault().Id;

                        FormulaType = "NONSTANDARDFORMULA";
                        var OSalHeadNonStd = db.PayScaleAssignment.Include(e => e.SalaryHead).Include(e => e.SalaryHead.SalHeadOperationType).Include(e => e.SalHeadFormula)
                            .Include(e => e.SalHeadFormula.Select(r => r.GeoStruct)).Include(e => e.SalHeadFormula.Select(r => r.PayStruct)).Include(e => e.SalHeadFormula.Select(r => r.FuncStruct))
                            .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                                .Include(e => e.SalHeadFormula.Select(t => t.FormulaType)).ToList();
                        foreach (var item in OSalHeadNonStd)
                        {
                            bool NonStdForm = item.SalHeadFormula.Any(e => e.FormulaType.LookupVal.ToUpper() == "NONSTANDARDFORMULA");
                            if (NonStdForm == true)
                            {

                                var OEmpSalStruct = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).Where(e => e.EndDate == null && e.EmployeePayroll_Id == EmpPayrollId).FirstOrDefault().EmpSalStructDetails.Where(e => e.SalaryHead_Id == item.SalaryHead_Id).FirstOrDefault();
                                if (OEmpSalStruct != null)
                                {
                                    FormulaActive = OEmpSalStruct.SalHeadFormula_Id != null ? "Y" : "N";
                                    EditAppl = false;

                                    view = new EditData()
                                    {
                                        Id = OEmpSalStruct.Id,
                                        SalaryHead = OEmpSalStruct.SalaryHead,
                                        Amount = OEmpSalStruct.Amount,
                                        Editable = EditAppl,
                                        FormulaEditable = FormulaActive,
                                        FormulaType = FormulaType
                                    };

                                    model.Add(view);

                                }
                                else
                                {
                                    FormulaActive = "N";
                                    EditAppl = false;
                                    view = new EditData()
                                    {
                                        Id = item.Id,
                                        SalaryHead = item.SalaryHead,
                                        Amount = 0,
                                        Editable = EditAppl,
                                        FormulaEditable = FormulaActive,
                                        FormulaType = FormulaType
                                    };

                                    model.Add(view);
                                }
                               
                                //PromoBook = db.PromotionServiceBook.Find(PromoBook.Narration);
                                Onewstruct.GeoStruct = db.GeoStruct.Find(PromoBook.GeoStruct_Id);
                                Onewstruct.PayStruct = db.PayStruct.Find(PromoBook.NewPayStruct_Id);
                                Onewstruct.FuncStruct = db.FuncStruct.Find(PromoBook.NewFuncStruct_Id);
                                SalHeadFormula Salformula = SalaryHeadGenProcess.SalFormulaFinderNew(Onewstruct, item, item.SalaryHead.Id);
                                FormulaActive = Salformula != null ? "Y" : "N";
                                EditAppl = true;
                                view = new EditData()
                                {
                                    Id = item.Id,
                                    SalaryHead = item.SalaryHead,
                                    Amount = OEmpSalStruct != null && Salformula == null ? OEmpSalStruct.Amount : 0,
                                    Editable = EditAppl,
                                    FormulaEditable = Salformula != null ? FormulaActive : "N",
                                    FormulaType = FormulaType
                                };

                                modelNew.Add(view);

                            }
                        }
                    }
                }
                else
                {
                    //Msg = "Nonstandard heads are defined. So you can't select multiple employees.";
                    Msg = "NonStandard heads one define in salary structure,Multiple employees selected for transfer,Any change the amount in salarystructure.";
                }



            }
            var result = new { Sal = model, SalNew = modelNew, msg = Msg };

            return Json(result, JsonRequestBehavior.AllowGet);

        }



        //public ActionResult GetNonstdData(string data)
        //{
        //    string Msg = "";
        //    List<EditData> model = new List<EditData>();
        //    var view = new EditData();

        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        bool EditAppl = true;
        //        string FormulaActive = "Y";
        //        string FormulaType = "";
        //        List<int> extraeditdata = one_ids(data);
        //        if (extraeditdata.Count() == 1)
        //        {
        //            foreach (int i in extraeditdata)
        //            {
        //                var OEmployeeSalStruct = db.EmployeePayroll.Where(e => e.Id == i).Include(e => e.Employee).Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
        //                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
        //                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
        //                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
        //                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Type)))
        //                                       .SingleOrDefault();

        //                var OEmpSalStruct = OEmployeeSalStruct.EmpSalStruct.Where(e => e.EndDate == null).FirstOrDefault();



        //                var OEmpSalStructDet = OEmpSalStruct.EmpSalStructDetails.Where(t => t.SalHeadFormula_Id != null);
        //                foreach (var SalForAppl in OEmpSalStructDet)
        //                {
        //                    var m = db.EmpSalStructDetails.Include(e => e.SalaryHead).Include(e => e.SalHeadFormula).Include(e => e.SalHeadFormula.FormulaType).Where(e => e.Id == SalForAppl.Id).SingleOrDefault();


        //                    var SalHeadForm = m.SalHeadFormula; //SalaryHeadGenProcess.SalFormulaFinder(x, m.SalaryHead.Id);


        //                    if (SalHeadForm != null)
        //                    {
        //                        if (SalHeadForm.FormulaType.LookupVal.ToUpper() == "NONSTANDARDFORMULA" && SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "BASIC")
        //                        {
        //                            FormulaActive = "Y";
        //                            EditAppl = true;
        //                            FormulaType = "NONSTANDARDFORMULA";

        //                            view = new EditData()
        //                            {
        //                                Id = SalForAppl.Id,
        //                                SalaryHead = SalForAppl.SalaryHead,
        //                                Amount = SalForAppl.Amount,
        //                                Editable = EditAppl,
        //                                FormulaEditable = FormulaActive,
        //                                FormulaType = FormulaType
        //                            };

        //                            model.Add(view);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            //Msg = "Nonstandard heads are defined. So you can't select multiple employees.";
        //            Msg = "Please note that in the case of multiple selections of employee releases, the nonstandard salary component value has to be changed manually...";
        //        }
        //    }



        //    var result = new { Sal = model, msg = Msg };

        //    return Json(result, JsonRequestBehavior.AllowGet);

        //}

        #region Release
        public ActionResult Release(PromotionServiceBook PromotionServiceBook, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var Emp = form["emp_Id"] == "0" ? "" : form["emp_Id"];
                    bool ReleaseFlag = form["ReleaseFlag"] == "0" ? false : Convert.ToBoolean(form["ReleaseFlag"]);
                    var Activityid = form["releas_Id"] == "0" ? "" : form["releas_Id"];
                    var SalHeadCount = form["SalHeadCount"] == "0" ? "0" : form["SalHeadCount"];
                    var ids = form["input_hidden_field"] != "" ? one_ids(form["input_hidden_field"]) : null;
                    var ReleaseDateStr = form["ReleaseDate"] == "0" ? "" : form["ReleaseDate"];
                    DateTime? ReleaseDate = !string.IsNullOrEmpty(ReleaseDateStr) ? (DateTime?)Convert.ToDateTime(ReleaseDateStr) : null; 


                    int CompId = 0;
                    if (Session["CompId"] != null)
                    {
                        CompId = Convert.ToInt32(Session["CompId"]);
                    }

                    //int Empid = 0;
                    //if (Emp != null)
                    //{
                    //    Empid = int.Parse(Emp);
                    //}14072023
                    int promotionId = -1;
                    List<int> PromotionIdList = null;
                    if (Activityid != null && Activityid != "0" && Activityid != "false")
                    {
                        PromotionIdList = Utility.StringIdsToListIds(Activityid);
                    }
                    List<int> Empids = null;
                    if (forwarddata != null && forwarddata != "0" && forwarddata != "false")
                    {
                        Empids = Utility.StringIdsToListIds(forwarddata);
                    }

                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;

                    foreach (var Empid in Empids)
                    {
                        Employee OEmployeedate = null;
                        OEmployeedate = db.Employee
                                        .Where(r => r.Id == Empid).SingleOrDefault();


                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                    .Include(e => e.EmpOffInfo)
                                    .Where(r => r.Id == Empid).SingleOrDefault();

                        OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == Empid).SingleOrDefault();

                       
                        //if (ReleaseDate.Value.Date < ProcessOthDate.Value.Date)
                        //{
                        //    Msg.Add(" This " + OEmployee.EmpCode + "Release date should be greater than the process date.");
                        //}

                        //int PayScaleAgrId = int.Parse(PayScaleAgr);
                        //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();


                        // int PromoId = int.Parse(forwarddata);
                        //int PromoId = int.Parse(Activityid);14072023
                        promotionId = promotionId + 1;
                        int PromoId = PromotionIdList[promotionId];

                        PromotionServiceBook.ReleaseFlag = Convert.ToBoolean(ReleaseFlag);
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                          new System.TimeSpan(0, 30, 0)))
                        {
                            if (ReleaseFlag != false)
                            {

                                PromotionServiceBook OPromotionServiceBook = db.PromotionServiceBook.Where(e => e.Id == PromoId).SingleOrDefault();

                                if (ReleaseDate.Value.Date < OPromotionServiceBook.ProcessPromoDate.Value.Date)
                                {
                                    Msg.Add(" The " + OEmployeedate.EmpCode + " has Release date should be greater than the process date.");
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                Process.ServiceBook.ServiceBookProcess("",CompId, "PROMOTION_RELEASE", null,
                                OPromotionServiceBook.Id, null, null, OEmployeePayroll.Id, "REGULAR", PromotionServiceBook.ReleaseDate, true, false, 0, null);


                                if (ids != null && ids.Count() > 0)
                                {
                                    List<EmpSalStruct> OEmpSalStruct = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead)).Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula))
                                .Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id && e.EffectiveDate >= OPromotionServiceBook.ProcessPromoDate).ToList();
                                    if (OEmpSalStruct.Count() > 0)
                                    {
                                        foreach (var item in OEmpSalStruct)
                                        {
                                            for (int i = 0; i < ids.Count(); i++)
                                            {
                                                int SalHeadId = ids[i];
                                                double amount = Convert.ToDouble(form["AmtNew" + i]);
                                                string FormulaEdit = form["FNew" + i];
                                                EmpSalStructDetails OEmpSalStructDet = item.EmpSalStructDetails.Where(e => e.SalaryHead.Id == SalHeadId).FirstOrDefault();


                                                if (FormulaEdit.ToUpper() == "N" && OEmpSalStructDet != null)
                                                {
                                                    OEmpSalStructDet.Amount = amount;
                                                    OEmpSalStructDet.SalHeadFormula = null;
                                                    db.EmpSalStructDetails.Attach(OEmpSalStructDet);
                                                    db.Entry(OEmpSalStructDet).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    db.Entry(OEmpSalStructDet).State = System.Data.Entity.EntityState.Detached;
                                                }
                                            }
                                        }
                                    }
                                }
                            
                                ts.Complete();

                                //var reporting_chk = db.Employee.Include(e => e.ReportingStructRights).Where(e => e.Id == OEmployee.Id).SingleOrDefault();
                                //if (reporting_chk.ReportingStructRights.Count() > 0)
                                //{
                                //    Msg.Add("  Data Updated successfully,<br /> Click here to <a style='color:blue' href='ReportingStruct/PartialPopUp?param=" + OEmployeePayroll.Id + "'target='_blank'>Reporting Structure</a>,");
                                //    // +"<br /> Click here to <a style='color:blue' href='EmpReportingTimingStruct/PartialPopUp?param=" + 1 + "'target='_blank'>Assign Authority</a>");
                                //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //}

                                //var EmpReportingTimingStruct = db.EmployeeAttendance.Where(e => e.Employee.Id == OEmployee.Id).Include(e => e.EmpReportingTimingStruct).SingleOrDefault();
                                //if (EmpReportingTimingStruct != null && EmpReportingTimingStruct.EmpReportingTimingStruct.Count > 0)
                                //{
                                //    // Msg.Add("  Data Updated successfully,<br /> Click here to <a style='color:blue' href='ReportingStruct/PartialPopUp?param=" + 1 + "'target='_blank'>Reporting Structure</a>," +
                                //    Msg.Add("<br /> Click here to <a style='color:blue' href='EmpReportingTimingStruct/PartialPopUp?param=" + OEmployeePayroll.Id + "'target='_blank'>Reporting Timing Structure</a>");
                                //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //}
                                //// DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);

                                ////  return Json(new { success = true, responseText = "Data Updated Successfully." }, JsonRequestBehavior.AllowGet);
                                //Msg.Add("  Data Updated successfully  ");
                                //// Msg.Add("  Data Updated successfully  "+);
                                //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                        }
                    }
                    var reporting_chk = db.Employee.Include(e => e.ReportingStructRights).Where(e => e.Id == OEmployee.Id).SingleOrDefault();
                    if (reporting_chk.ReportingStructRights.Count() > 0)
                    {
                        Msg.Add("  Data Updated successfully,<br /> Click here to <a style='color:blue' href='ReportingStruct/PartialPopUp?param=" + OEmployeePayroll.Id + "'target='_blank'>Reporting Structure</a>,");
                        // +"<br /> Click here to <a style='color:blue' href='EmpReportingTimingStruct/PartialPopUp?param=" + 1 + "'target='_blank'>Assign Authority</a>");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    var EmpReportingTimingStruct = db.EmployeeAttendance.Where(e => e.Employee.Id == OEmployee.Id).Include(e => e.EmpReportingTimingStruct).SingleOrDefault();
                    if (EmpReportingTimingStruct != null && EmpReportingTimingStruct.EmpReportingTimingStruct.Count > 0)
                    {
                        // Msg.Add("  Data Updated successfully,<br /> Click here to <a style='color:blue' href='ReportingStruct/PartialPopUp?param=" + 1 + "'target='_blank'>Reporting Structure</a>," +
                        Msg.Add("<br /> Click here to <a style='color:blue' href='EmpReportingTimingStruct/PartialPopUp?param=" + OEmployeePayroll.Id + "'target='_blank'>Reporting Timing Structure</a>");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    // DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
                    //  return Json(new { success = true, responseText = "Data Updated Successfully." }, JsonRequestBehavior.AllowGet);
                    Msg.Add("  Data Updated successfully  ");
                    // Msg.Add("  Data Updated successfully  "+);
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new { success = false, responseText = "Kindly enter release date." }, JsonRequestBehavior.AllowGet);
                    Msg.Add(" Kindly enter release date. ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                catch (Exception ex)
                {
                    // DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    //   List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion

        //GetZeroStepEmp
        public JsonResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.PromotionServiceBook
                    .Include(e => e.PromotionActivity.PromoList).Include(e => e.OldPayStruct).Include(e => e.OldPayStruct.Level).Include(e => e.OldPayStruct.Grade).Include(e => e.OldPayStruct.JobStatus)
                                .Include(e => e.NewPayStruct).Include(e => e.NewPayStruct.Level).Include(e => e.NewPayStruct.Grade)
                                .Include(e => e.NewPayStruct.JobStatus)
                                .Include(e => e.OldFuncStruct).Include(e => e.OldFuncStruct.Job).Include(e => e.OldFuncStruct.JobPosition)
                                .Include(e => e.NewFuncStruct).Include(e => e.NewFuncStruct.Job).Include(e => e.NewFuncStruct.JobPosition)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        PromotionActivity = e.PromotionActivity.PromoList.LookupVal,
                        ProcessPromoDate = e.ProcessPromoDate,
                        ReleaseDate = e.ReleaseDate,
                        OldPayStruct_FullDetails = e.OldPayStruct == null ? null : "Grade : " + e.OldPayStruct.Grade.Name + " , Level :" + e.OldPayStruct.Level.Name + " " + e.OldPayStruct.FullDetails,
                        OldPayStruct_Id = e.OldPayStruct.Id == null ? "" : e.OldPayStruct.Id.ToString(),
                        NewPayStruct_FullDetails = e.NewPayStruct == null ? null : "Grade : " + e.NewPayStruct.Grade.Name + " , Level :" + e.NewPayStruct.Level.Name + " " + e.NewPayStruct.FullDetails,
                        NewPayStruct_Id = e.NewPayStruct.Id == null ? "" : e.NewPayStruct.Id.ToString(),
                        OldFuncStruct_FullDetails = e.OldFuncStruct == null ? null : e.OldFuncStruct.FullDetails,
                        OldFuncStruct_Id = e.OldFuncStruct.Id == null ? "" : e.OldFuncStruct.Id.ToString(),
                        NewFuncStruct_FullDetails = e.NewFuncStruct == null ? null : e.NewFuncStruct.FullDetails,
                        NewFuncStruct_Id = e.NewFuncStruct.Id == null ? "" : e.NewFuncStruct.Id.ToString(),
                        NewPayScale_FullDetails = e.NewPayScale == null ? null : e.NewPayScale.FullDetails,
                        NewPayScale_Id = e.NewPayScale.Id == null ? "" : e.NewPayScale.Id.ToString(),
                        OldPayScale_FullDetails = e.OldPayScale == null ? null : e.OldPayScale.FullDetails,
                        OldPayScale_Id = e.OldPayScale.Id == null ? "" : e.OldPayScale.Id.ToString(),
                        NewPayScaleAgreement_FullDetails = e.NewPayScaleAgreement == null ? null : e.NewPayScaleAgreement.FullDetails,
                        NewPayScaleAgreement_Id = e.NewPayScaleAgreement.Id == null ? "" : e.NewPayScaleAgreement.Id.ToString(),
                        OldPayScaleAgreement_FullDetails = e.OldPayScaleAgreement == null ? null : e.OldPayScaleAgreement.FullDetails,
                        OldPayScaleAgreement_Id = e.OldPayScaleAgreement.Id == null ? "" : e.OldPayScaleAgreement.Id.ToString(),
                        Narration = e.Narration,
                        Action = e.DBTrack.Action
                    }).ToList();

                var PromoServBook = db.PromotionServiceBook.Find(data);
                Session["RowVersion"] = PromoServBook.RowVersion;
                var Auth = PromoServBook.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(new Object[] { Q, Auth }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult EditInline(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.PromotionServiceBook
                    .Include(e => e.PromotionActivity.PromoList).Include(e => e.OldPayStruct).Include(e => e.OldPayStruct.Level).Include(e => e.OldPayStruct.Grade).Include(e => e.OldPayStruct.JobStatus)
                                .Include(e => e.NewPayStruct).Include(e => e.NewPayStruct.Level).Include(e => e.NewPayStruct.Grade)
                                .Include(e => e.NewPayStruct.JobStatus)
                                .Include(e => e.OldFuncStruct).Include(e => e.OldFuncStruct.Job).Include(e => e.OldFuncStruct.JobPosition)
                                .Include(e => e.NewFuncStruct).Include(e => e.NewFuncStruct.Job).Include(e => e.NewFuncStruct.JobPosition)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        PromotionActivity = e.PromotionActivity.PromoList.LookupVal,
                        ProcessPromoDate = e.ProcessPromoDate,
                        ReleaseDate = e.ReleaseDate,
                        OldPayStruct_FullDetails = e.OldPayStruct == null ? null : "Grade : " + e.OldPayStruct.Grade.Name + " , Level :" + e.OldPayStruct.Level.Name + " " + e.OldPayStruct.FullDetails,
                        OldPayStruct_Id = e.OldPayStruct.Id == null ? "" : e.OldPayStruct.Id.ToString(),
                        NewPayStruct_FullDetails = e.NewPayStruct == null ? null : "Grade : " + e.NewPayStruct.Grade.Name + " , Level :" + e.NewPayStruct.Level.Name + " " + e.NewPayStruct.FullDetails,
                        NewPayStruct_Id = e.NewPayStruct.Id == null ? "" : e.NewPayStruct.Id.ToString(),
                        OldFuncStruct_FullDetails = e.OldFuncStruct == null ? null : e.OldFuncStruct.FullDetails,
                        OldFuncStruct_Id = e.OldFuncStruct.Id == null ? "" : e.OldFuncStruct.Id.ToString(),
                        NewFuncStruct_FullDetails = e.NewFuncStruct == null ? null : e.NewFuncStruct.FullDetails,
                        NewFuncStruct_Id = e.NewFuncStruct.Id == null ? "" : e.NewFuncStruct.Id.ToString(),
                        NewPayScale_FullDetails = e.NewPayScale == null ? null : e.NewPayScale.FullDetails,
                        NewPayScale_Id = e.NewPayScale.Id == null ? "" : e.NewPayScale.Id.ToString(),
                        OldPayScale_FullDetails = e.OldPayScale == null ? null : e.OldPayScale.FullDetails,
                        OldPayScale_Id = e.OldPayScale.Id == null ? "" : e.OldPayScale.Id.ToString(),
                        NewPayScaleAgreement_FullDetails = e.NewPayScaleAgreement == null ? null : e.NewPayScaleAgreement.FullDetails,
                        NewPayScaleAgreement_Id = e.NewPayScaleAgreement.Id == null ? "" : e.NewPayScaleAgreement.Id.ToString(),
                        OldPayScaleAgreement_FullDetails = e.OldPayScaleAgreement == null ? null : e.OldPayScaleAgreement.FullDetails,
                        OldPayScaleAgreement_Id = e.OldPayScaleAgreement.Id == null ? "" : e.OldPayScaleAgreement.Id.ToString(),
                        Narration = e.Narration,
                        Action = e.DBTrack.Action
                    }).ToList();

                var PromoServBook = db.PromotionServiceBook.Find(data);
                Session["RowVersion"] = PromoServBook.RowVersion;
                var Auth = PromoServBook.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(new Object[] { Q, Auth }, JsonRequestBehavior.AllowGet);
            }
        }

        public class returnDataClass
        {
            public Array id { get; set; }
            public Array val { get; set; }
        }

        public ActionResult EditDisplay(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int eid = Convert.ToInt32(data);
                var empp = db.EmployeePayroll.Include(q => q.Employee).Where(a => a.Id == eid).SingleOrDefault();
                var add_data = db.Employee
                    .Where(e => e.Id == empp.Employee.Id)
                    .Include(e => e.ReportingStructRights)
                    .Include(e => e.ReportingStructRights.Select(a => a.AccessRights))
                    .Include(e => e.ReportingStructRights.Select(a => a.ReportingStruct))
                  .ToList();

                var ListreturnDataClass = new List<returnDataClass>();
                foreach (var item in add_data)
                {
                    ListreturnDataClass.Add(new returnDataClass
                    {
                        id = item.ReportingStructRights.Select(e => e.Id.ToString()).ToArray(),
                        val = item.ReportingStructRights.Select(e => e.FullDetails).ToArray()
                    });
                }

                var Emp = db.Employee.Find(empp.Employee.Id);
                TempData["RowVersion"] = Emp.RowVersion;
                var Auth = Emp.DBTrack.IsModified;
                return Json(new Object[] { ListreturnDataClass, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }


        public class PromoServBookGridData
        {
            public int Id { get; set; }
            public int EmpId { get; set; }
            public Employee Employee { get; set; }
            public PromoActivity PromoActivity { get; set; }
            public string ActivityDate { get; set; }
            public string OldFuncStruct { get; set; }
            public string NewFuncStruct { get; set; }
            public string OldPayStruct { get; set; }
            public string NewPayStruct { get; set; }
            public double OldBasic { get; set; }
            public double NewBasic { get; set; }
        }

        public class returndatagridclass
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }

        public class PromoChildDataClass
        {
            public int Id { get; set; }
            public bool Release { get; set; }
            public string ReleaseDate { get; set; }
            public String Activity { get; set; }
            public string ProcessDate { get; set; }
            public double OldBasic { get; set; }
            public double Fittment { get; set; }
            public double NewBasic { get; set; }
            public string OldPayStruct { get; set; }
            public string NewPayStruct { get; set; }
            public string OldFuncStruct { get; set; }
            public string NewFuncStruct { get; set; }
        }
        public ActionResult GetApplicableData(string data, string data2)
        {
            //data-selected
            //data2-empid
            using (DataBaseContext db = new DataBaseContext())
            {
                int EmpId = Convert.ToInt32(data2);// form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);//modified by prashant 15042017

                var OPayscale = db.Employee.Where(e => e.Id == EmpId)//modified by prashant 15042017
                    .Select(r => r.EmpOffInfo.PayScale).FirstOrDefault();//modified by prashant 15042017
                var OPromo = db.PayScaleAgreement.Where(e => e.EndDate == null && e.PayScale.Id == OPayscale.Id)
                    .Include(e => e.PromoActivity)
                    .Include(e => e.PromoActivity.Select(a => a.PromoPolicy))
                    .SingleOrDefault();//modified by prashant 15042017
                // .Select(r => r.PromotionServiceBook).FirstOrDefault();//modified by prashant 15042017
                var id = Convert.ToInt32(data);

                var qurey = OPromo.PromoActivity.Where(e =>e.PromoPolicy != null && e.PromoPolicy.Id == id).Select(e => e.PromoPolicy).ToList();//modified by prashant 15042017
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }
        //public ActionResult P2BGridRelease(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;

        //        IEnumerable<PromoServBookGridData> PromoServBook = null;
        //        List<PromoServBookGridData> model = new List<PromoServBookGridData>();
        //        PromoServBookGridData view = null;

        //        var OEmployee = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).ToList();

        //        foreach (var z in OEmployee)
        //        {
        //            var ObjPromoServBook = db.EmployeePayroll.Where(e => e.Id == z.Id)
        //                .Select(e => e.PromotionServiceBook.Where(r => r.ReleaseFlag != true))
        //                                .SingleOrDefault();


        //            //DateTime? Eff_Date = null;
        //            //PayScaleAgreement PayScaleAgr = null;
        //            foreach (var a in ObjPromoServBook)
        //            {
        //                //Eff_Date = Convert.ToDateTime(a.ProcessMonth);
        //                var aa = db.PromotionServiceBook.Include(e => e.OldPayStruct).Include(e => e.OldPayStruct.Level).Include(e => e.OldPayStruct.Grade)
        //                    .Include(e => e.OldPayStruct.JobStatus)
        //                    .Include(e => e.NewPayStruct).Include(e => e.NewPayStruct.Level).Include(e => e.NewPayStruct.Grade)
        //                    .Include(e => e.NewPayStruct.JobStatus)
        //                    .Include(e => e.OldFuncStruct).Include(e => e.OldFuncStruct.Job).Include(e => e.OldFuncStruct.JobPosition)
        //                    .Include(e => e.NewFuncStruct).Include(e => e.NewFuncStruct.Job).Include(e => e.NewFuncStruct.JobPosition)
        //                    .Include(e => e.PromotionActivity).Include(e => e.PromotionActivity.PromoList)
        //                    .Where(e => e.Id == a.Id && e.ReleaseFlag != true).SingleOrDefault();
        //                view = new PromoServBookGridData()
        //                {
        //                    Id = a.Id,
        //                    EmpId = z.Employee.Id,
        //                    Employee = z.Employee,
        //                    PromoActivity = a.PromotionActivity,
        //                    ActivityDate = a.ProcessPromoDate.ToString(),
        //                    NewFuncStruct = a.NewFuncStruct == null ? null : a.NewFuncStruct.FullDetails,
        //                    OldFuncStruct = a.OldFuncStruct == null ? null : a.OldFuncStruct.FullDetails,
        //                    NewPayStruct = a.NewPayStruct == null ? null : a.NewPayStruct.FullDetails,
        //                    OldPayStruct = a.OldPayStruct == null ? null : a.OldPayStruct.FullDetails

        //                };

        //                model.Add(view);
        //            }

        //        }

        //        PromoServBook = model;

        //        IEnumerable<PromoServBookGridData> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = PromoServBook;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")          // a.Id,a.Employeee.EmpCode,a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration
        //                    jsonData = IE.Select(a => new { a.Id, a.EmpId, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList.LookupVal, a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "EmpCode")
        //                    jsonData = IE.Select(a => new { a.Id, a.EmpId, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList.LookupVal, a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct }).Where((e => (e.EmpCode.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "EmpName")
        //                    jsonData = IE.Select(a => new { a.Id, a.EmpId, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList.LookupVal, a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct }).Where((e => (e.FullNameFML.ToString().Contains(gp.searchString)))).ToList();
        //                //else if (gp.searchField == "PromoActivity")
        //                //jsonData = IE.Select(a => new { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList.LookupVal, a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct }).Where((e => (e.PromoActivity..ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "ActivityDate")
        //                    jsonData = IE.Select(a => new { a.Id, a.EmpId, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList.LookupVal, a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct }).Where((e => (e.ActivityDate.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "ActivityDate")
        //                    jsonData = IE.Select(a => new { a.Id, a.EmpId, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList.LookupVal, a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct }).Where((e => (e.OldFuncStruct.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "OldFuncStruct")
        //                    jsonData = IE.Select(a => new { a.Id, a.EmpId, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList.LookupVal, a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct }).Where((e => (e.NewFuncStruct.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "NewFuncStruct")
        //                    jsonData = IE.Select(a => new { a.Id, a.EmpId, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList.LookupVal, a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct }).Where((e => (e.OldPayStruct.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "OldPayStruct")
        //                    jsonData = IE.Select(a => new { a.Id, a.EmpId, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList.LookupVal, a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct }).Where((e => (e.NewPayStruct.ToString().Contains(gp.searchString)))).ToList();


        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpId, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList.LookupVal, a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = PromoServBook;
        //            Func<PromoServBookGridData, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
        //                                 gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
        //                                 gp.sidx == "PromoActivity" ? c.PromoActivity.PromoList.LookupVal :
        //                                 gp.sidx == "OldFuncStruct" ? c.ActivityDate.ToString() :
        //                                 gp.sidx == "ToPeriod" ? c.OldFuncStruct :
        //                                 gp.sidx == "NewFuncStruct" ? c.NewFuncStruct :
        //                                 gp.sidx == "OldPayStruct" ? c.OldPayStruct :
        //                                 gp.sidx == "NewPayStruct" ? c.NewPayStruct : ""
        //                                  );
        //            }
        //            if (gp.sord == "asc")  //Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : ""
        //            {
        //                IE = IE.OrderBy(orderfuc);  // a.Id,a.Employeee.EmpCode,a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.EmpId, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList != null ? a.PromoActivity.PromoList.LookupVal : "", a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.EmpId, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList != null ? a.PromoActivity.PromoList.LookupVal : "", a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpId, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList != null ? a.PromoActivity.PromoList.LookupVal : "", a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct }).ToList();
        //            }
        //            totalRecords = PromoServBook.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public class EditData
        {
            public int Id { get; set; }
            public SalaryHead SalaryHead { get; set; }
            public bool Editable { get; set; }
            public double Amount { get; set; }
            public string FormulaEditable { get; set; }
            public string FormulaType { get; set; }
            public string Narration { get; set; }
        }

        public ActionResult P2BGridReleaseNonStd(P2BGrid_Parameters gp, int extraeditdata)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EditData> EmpSalStruct = null;

                List<EditData> model = new List<EditData>();
                var view = new EditData();


                bool EditAppl = true;
                string FormulaActive = "Y";
                string FormulaType = "";


                var OEmployeeSalStruct = db.EmployeePayroll.Where(e => e.Id == extraeditdata).Include(e => e.Employee).Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Type)))
                                       .SingleOrDefault();

                var OEmpSalStruct = OEmployeeSalStruct.EmpSalStruct.Where(e => e.EndDate == null).FirstOrDefault();



                var OEmpSalStructDet = OEmpSalStruct.EmpSalStructDetails;
                foreach (var SalForAppl in OEmpSalStructDet)
                {
                    var m = db.EmpSalStructDetails.Include(e => e.SalaryHead).Include(e => e.SalHeadFormula).Include(e => e.SalHeadFormula.FormulaType).Where(e => e.Id == SalForAppl.Id).SingleOrDefault();


                    var SalHeadForm = m.SalHeadFormula; //SalaryHeadGenProcess.SalFormulaFinder(x, m.SalaryHead.Id);


                    if (SalHeadForm != null)
                    {
                        if (SalHeadForm.FormulaType.LookupVal.ToUpper() == "NONSTANDARDFORMULA" && SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "BASIC")
                        {
                            FormulaActive = "Y";
                            EditAppl = true;
                            FormulaType = "NONSTANDARDFORMULA";

                            view = new EditData()
                            {
                                Id = SalForAppl.Id,
                                SalaryHead = SalForAppl.SalaryHead,
                                Amount = SalForAppl.Amount,
                                Editable = EditAppl,
                                FormulaEditable = FormulaActive,
                                FormulaType = FormulaType
                            };

                            model.Add(view);
                        }
                    }



                }
                EmpSalStruct = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpSalStruct;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.SalaryHead.Name.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Amount.ToString().Contains(gp.searchString.ToUpper()))

                               || (e.Editable.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.FormulaType, a.FormulaEditable, a.Editable, a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency.LookupVal, a.SalaryHead.Type.LookupVal, a.SalaryHead.SalHeadOperationType.LookupVal, a.Editable, a.Id }).Where((e => (e.Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.FormulaType, a.FormulaEditable, a.Editable, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpSalStruct;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        //orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        orderfuc = (c => gp.sidx == "Id" ? c.SalaryHead.Type.LookupVal.ToString() : "");

                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "SalaryHead" ? c.SalaryHead.Name.ToString() :
                                         gp.sidx == "Amount" ? c.Amount.ToString() :
                                         gp.sidx == "FormulaType" ? c.FormulaType.ToString() :
                                         gp.sidx == "FormulaEditable" ? c.FormulaEditable.ToString() :
                                         gp.sidx == "Editable" ? c.Editable.ToString() : "");
                    }
                    gp.sord = "desc";
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.FormulaType, a.FormulaEditable, a.Editable, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.FormulaType, a.FormulaEditable, a.Editable, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.FormulaType, a.FormulaEditable, a.Editable, a.Id }).ToList();
                    }
                    totalRecords = EmpSalStruct.Count();
                }
                if (totalRecords > 0)
                {
                    totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
                }
                if (gp.page > totalPages)
                {
                    gp.page = totalPages;
                }
                var JsonData = new
                {
                    page = gp.page,
                    rows = jsonData,
                    records = totalRecords,
                    total = totalPages
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        public ActionResult P2BGridRelease(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<PromoServBookGridData> PromoServBook = null;
                List<PromoServBookGridData> model = new List<PromoServBookGridData>();
                PromoServBookGridData view = null;

                var OEmployee = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).ToList();

                foreach (var z in OEmployee)
                {
                    var ObjPromoServBook = db.EmployeePayroll.Where(e => e.Id == z.Id)
                        .Select(e => e.PromotionServiceBook.Where(r => r.ReleaseFlag != true))
                                        .SingleOrDefault();


                    //DateTime? Eff_Date = null;
                    //PayScaleAgreement PayScaleAgr = null;
                    foreach (var a in ObjPromoServBook)
                    {
                        //Eff_Date = Convert.ToDateTime(a.ProcessMonth);
                        var aa = db.PromotionServiceBook.Include(e => e.OldPayStruct).Include(e => e.OldPayStruct.Level).Include(e => e.OldPayStruct.Grade)
                            .Include(e => e.OldPayStruct.JobStatus)
                            .Include(e => e.NewPayStruct).Include(e => e.NewPayStruct.Level).Include(e => e.NewPayStruct.Grade)
                            .Include(e => e.NewPayStruct.JobStatus)
                            .Include(e => e.OldFuncStruct).Include(e => e.OldFuncStruct.Job).Include(e => e.OldFuncStruct.JobPosition)
                            .Include(e => e.NewFuncStruct).Include(e => e.NewFuncStruct.Job).Include(e => e.NewFuncStruct.JobPosition)
                            .Include(e => e.PromotionActivity).Include(e => e.PromotionActivity.PromoList)
                            .Where(e => e.Id == a.Id && e.ReleaseFlag != true).SingleOrDefault();
                        view = new PromoServBookGridData()
                        {
                            Id = a.Id,
                            EmpId = z.Employee.Id,
                            Employee = z.Employee,
                            PromoActivity = a.PromotionActivity,
                            ActivityDate = a.ProcessPromoDate.Value.ToString("dd/MM/yyyy"),
                            NewFuncStruct = a.NewFuncStruct == null ? null : a.NewFuncStruct.FullDetails,
                            OldFuncStruct = a.OldFuncStruct == null ? null : a.OldFuncStruct.FullDetails,
                            NewPayStruct = a.NewPayStruct == null ? null : a.NewPayStruct.FullDetails,
                            OldPayStruct = a.OldPayStruct == null ? null : a.OldPayStruct.FullDetails,
                            OldBasic = a.OldBasic,
                            NewBasic = a.NewBasic

                        };

                        model.Add(view);
                    }

                }

                PromoServBook = model;

                IEnumerable<PromoServBookGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = PromoServBook;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpId.ToString().Contains(gp.searchString))
                                || (e.Employee.EmpCode.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.Employee.EmpName.FullNameFML.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.PromoActivity.PromoList.LookupVal.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.ActivityDate.ToString().Contains(gp.searchString))
                                || (e.OldBasic.ToString().Contains(gp.searchString))
                                || (e.NewBasic.ToString().Contains(gp.searchString))
                                || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList.LookupVal, a.ActivityDate, a.OldBasic, a.NewBasic, a.Id, a.EmpId }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList.LookupVal, a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldBasic, a.OldBasic, a.Id, a.EmpId }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = PromoServBook;
                    Func<PromoServBookGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpId" ? c.EmpId.ToString() :
                                         gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "Activity" ? c.PromoActivity.PromoList.LookupVal :
                                         gp.sidx == "Activity Date" ? c.ActivityDate.ToString() :
                                         gp.sidx == "Old Basic" ? c.OldBasic.ToString() :
                                         gp.sidx == "New Basic" ? c.NewBasic.ToString() : ""
                                          );
                    }
                    if (gp.sord == "asc")  //Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : ""
                    {
                        IE = IE.OrderBy(orderfuc);  // a.Id,a.Employeee.EmpCode,a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList != null ? a.PromoActivity.PromoList.LookupVal : "", a.ActivityDate, a.OldBasic, a.NewBasic, a.Id, a.EmpId }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList != null ? a.PromoActivity.PromoList.LookupVal : "", a.ActivityDate, a.OldBasic, a.NewBasic, a.Id, a.EmpId }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PromoActivity.PromoList != null ? a.PromoActivity.PromoList.LookupVal : "", a.ActivityDate, a.OldBasic, a.NewBasic, a.Id, a.EmpId }).ToList();
                    }
                    totalRecords = PromoServBook.Count();
                }
                if (totalRecords > 0)
                {
                    totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
                }
                if (gp.page > totalPages)
                {
                    gp.page = totalPages;
                }
                var JsonData = new
                {
                    page = gp.page,
                    rows = jsonData,
                    records = totalRecords,
                    total = totalPages
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                        // .Include(e => e.PromotionServiceBook)
                        .ToList();
                    // for searchs
                    IEnumerable<EmployeePayroll> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                              || (e.Employee.EmpCode.ToUpper().Contains(param.sSearch.ToUpper()))
                                              || (e.Employee.EmpName.FullNameFML.ToUpper().Contains(param.sSearch.ToUpper()))
                                              ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeePayroll, string> orderfunc = (c =>
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
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Employee.EmpCode,
                                Name = item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                                JoiningDate = item.Employee.ServiceBookDates != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                                Job = item.Employee.FuncStruct != null ? item.Employee.FuncStruct.Job.Name : null,
                                Grade = item.Employee.PayStruct != null ? item.Employee.PayStruct.Grade.Name : null,
                                Location = item.Employee.GeoStruct != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null,

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
                    throw e;
                }
            }
        }

        public ActionResult Get_PromoServBook(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.PromotionServiceBook.Select(r => r.PromotionActivity))
                        .Include(e => e.PromotionServiceBook.Select(r => r.PromotionActivity.PromoList))
                        .Include(e => e.PromotionServiceBook.Select(r => r.NewPayStruct))
                        .Include(e => e.PromotionServiceBook.Select(r => r.NewPayStruct.Grade))
                        .Include(e => e.PromotionServiceBook.Select(r => r.NewPayStruct.Level))
                        .Include(e => e.PromotionServiceBook.Select(r => r.NewPayStruct.JobStatus))
                        .Include(e => e.PromotionServiceBook.Select(r => r.OldPayStruct.Grade))
                        .Include(e => e.PromotionServiceBook.Select(r => r.OldPayStruct.Level))
                        .Include(e => e.PromotionServiceBook.Select(r => r.OldPayStruct.JobStatus))
                        .Include(e => e.PromotionServiceBook.Select(r => r.OldPayStruct))
                        .Include(e => e.PromotionServiceBook.Select(r => r.OldPayStruct.Grade))
                        .Include(e => e.PromotionServiceBook.Select(r => r.OldPayStruct.Level))
                        .Include(e => e.PromotionServiceBook.Select(r => r.OldPayStruct.JobStatus))
                        .Include(e => e.PromotionServiceBook.Select(r => r.OldFuncStruct))
                        .Include(e => e.PromotionServiceBook.Select(r => r.OldFuncStruct.Job))
                        .Include(e => e.PromotionServiceBook.Select(r => r.OldFuncStruct.JobPosition))
                        .Include(e => e.PromotionServiceBook.Select(r => r.NewFuncStruct))
                        .Include(e => e.PromotionServiceBook.Select(r => r.NewFuncStruct.Job))
                        .Include(e => e.PromotionServiceBook.Select(r => r.NewFuncStruct.JobPosition))
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<PromoChildDataClass> returndata = new List<PromoChildDataClass>();
                        foreach (var item in db_data.PromotionServiceBook)
                        {
                            returndata.Add(new PromoChildDataClass
                            {
                                Id = item.Id,
                                Release = item.ReleaseFlag,
                                ReleaseDate = item.ReleaseDate != null ? item.ReleaseDate.Value.ToShortDateString() : null,
                                Activity = item.PromotionActivity != null ? item.PromotionActivity.PromoList.LookupVal : null,
                                ProcessDate = item.ProcessPromoDate != null ? item.ProcessPromoDate.Value.ToShortDateString() : null,
                                OldBasic = item.OldBasic,
                                Fittment = item.Fittment,
                                NewBasic = item.NewBasic,
                                NewFuncStruct = item.NewFuncStruct != null ? item.NewFuncStruct.FullDetails : null,
                                OldFuncStruct = item.OldFuncStruct != null ? item.OldFuncStruct.FullDetails : null,
                                NewPayStruct = item.NewPayStruct != null ? item.NewPayStruct.FullDetails : null,
                                OldPayStruct = item.OldPayStruct != null ? item.OldPayStruct.FullDetails : null
                            });
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
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

        [HttpPost]
        public ActionResult GridDelete(int data)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    PromotionServiceBook PromoServBook = db.PromotionServiceBook.Where(e => e.Id == data).SingleOrDefault();

                    if (PromoServBook.ReleaseFlag == true)
                    {
                        return this.Json(new { status = true, valid = true, responseText = "You cannot delete as activity is already released.", JsonRequestBehavior.AllowGet });
                    }

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            db.PromotionServiceBook.Attach(PromoServBook);
                            db.Entry(PromoServBook).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            db.Entry(PromoServBook).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            return this.Json(new { status = true, responseText = "Data removed successfully.", JsonRequestBehavior.AllowGet });



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
                            // List<string> Msg = new List<string>();
                            Msg.Add(ex.Message);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }
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

        public ActionResult GetZeroStepIncrEmp(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var JanYear = Convert.ToDateTime("01/" + "01/" + DateTime.Now.Year);
                var geo_data = db.EmployeePayroll
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.PromotionServiceBook)
                    .Include(e => e.PromotionServiceBook.Select(a => a.PromotionActivity.PromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails))
                    //.Include(e => e.IncrementServiceBook.Select(z => z.IncrActivity.IncrPolicy.IncrPolicyDetails))
                    .Where(e => e.PromotionServiceBook.Any(a => a.ReleaseFlag == true && (a.ProcessPromoDate >= JanYear && a.ProcessPromoDate < DateTime.Now) &&
                        a.PromotionActivity.PromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps == 0))
                    .Select(e => new
                {
                    code = e.Id.ToString(),

                    value = "EmpCode :" + e.Employee.EmpCode + "EmpName : " + e.Employee.EmpName.FullNameFML

                }).ToList();

                return Json(geo_data, JsonRequestBehavior.AllowGet);
            }
        }
    }

    public class LevelSettings
    {
        private string CompCode; 

        public LevelSettings(IDictionary<string, string> settinigs)
        {
            if (settinigs.Count() > 0)
            {
                 this.CompCode = settinigs.First(x => x.Key.Equals("CompCode")).Value; 
            }
           
        }

        public string CCode { get { return CompCode; } } 
    }
}