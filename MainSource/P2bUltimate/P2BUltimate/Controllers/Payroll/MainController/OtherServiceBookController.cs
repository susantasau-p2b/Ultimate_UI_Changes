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
using P2B.SERVICES.Interface;
using P2B.SERVICES.Factory;


namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class OtherServiceBookController : Controller
    {

          readonly IP2BINI p2BINI;
         readonly LevelSettings LevelSettings;

         private readonly Type Type;

         public OtherServiceBookController()
        {
            p2BINI = P2BINI.RegisterSettings();
            LevelSettings = new LevelSettings(p2BINI.GetSectionValues("LevelSettings"));
            Type = typeof(OtherServiceBookController); 
        }
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashnat 13042017
            return View("~/Views/Payroll/MainViews/OtherServiceBook/Index.cshtml");
        }

        public class EditData
        {
            public int Id { get; set; }
            public SalaryHead SalaryHead { get; set; }
            public bool Editable { get; set; }
            public double Amount { get; set; }
            public string FormulaEditable { get; set; }
            public string FormulaType { get; set; }
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
                List<int> OthAct_Id = one_ids(data1);
                if (extraeditdata.Count() == 1 && OthAct_Id.Count() == 1)
                {
                    int EmpId = extraeditdata.FirstOrDefault();
                    int OthActId = OthAct_Id.FirstOrDefault();
                    if (EmpId != 0 && OthActId != 0)
                    {
                        EmpSalStruct Onewstruct = new EmpSalStruct();
                        var OthBook = db.OtherServiceBook.Find(OthActId);
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

                                Onewstruct.GeoStruct = db.GeoStruct.Find(OthBook.GeoStruct_Id);
                                Onewstruct.PayStruct = db.PayStruct.Find(OthBook.NewPayStruct_Id);
                                Onewstruct.FuncStruct = db.FuncStruct.Find(OthBook.NewFuncStruct_Id);
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
        //                var OEmployeeSalStruct = db.EmployeePayroll.Where(e => e.Employee.Id == i).Include(e => e.Employee).Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
        //                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
        //                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
        //                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
        //                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Type)))
        //                                       .SingleOrDefault();

        //                var OEmpSalStruct = OEmployeeSalStruct.EmpSalStruct.Where(e => e.EndDate == null).FirstOrDefault();



        //                var OEmpSalStructDet = OEmpSalStruct.EmpSalStructDetails;
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
        //        //if (extraeditdata.Count() > 1 && model.Count() > 1)
        //        //{
        //        //    Msg = "Nonstandard heads are defined. So you can't select multiple employees.";
        //        //}
        //        else
        //        {
        //            //Msg = "Nonstandard heads are defined. So you can't select multiple employees.";
        //            Msg = "Please note that in the case of multiple selections of employee releases, the nonstandard salary component value has to be changed manually...";
        //        }

        //    }



        //    var result = new { Sal = model, msg = Msg };

        //    return Json(result, JsonRequestBehavior.AllowGet);

        //}

        public ActionResult PopulateDropDownActivityList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var qurey = db.IncrActivity.Include(e => e.IncrList).ToList();
                int OEmp_Id = Convert.ToInt32(data2);
                var query = db.EmployeePolicyStruct.AsNoTracking().Where(e => e.EmployeePayroll.Employee.Id == OEmp_Id && e.EndDate == null).Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.EmployeePolicyStructDetails)
                    .Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula)).Include(e => e.EmployeePolicyStructDetails.
                    Select(r => r.PolicyFormula.OthServiceBookActivity)).FirstOrDefault();
                var OthActList = query.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.OthServiceBookActivity);

                var selected = (Object)null;
                //if (data2 != "" && data != "0" && data2 != "0")
                //{
                //    selected = Convert.ToInt32(data2);
                //}

                List<OthServiceBookActivity> OtherAct = new List<OthServiceBookActivity>();
                if (OthActList.Count() > 0)
                {
                    foreach (var item in OthActList)
                    {
                        if (item.FirstOrDefault() != null)
                        {
                            foreach (var item1 in item)
                            {
                                OtherAct.Add(item1);
                            }
                        }

                    }
                }
                SelectList s = new SelectList(OtherAct, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PopulateDropDownList1(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //String selected = null;
                if (data != "" && data != null && data != "0")
                {
                    int? selected = null;
                    var filter = Convert.ToInt32(data);
                    var qurey = db.Taluka.Include(e => e.Cities).Where(e => e.Id == filter).SingleOrDefault();
                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);

                    }

                    SelectList s = new SelectList(qurey.Cities, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (data2 != "")
                    {
                        int selected = Convert.ToInt32(data2);
                        var qurey = db.City.Where(e => e.Id == selected).ToList();

                        SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var qurey = db.City.ToList();

                        SelectList s = new SelectList(qurey, "Id", "FullDetails", "");
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    return Json("", JsonRequestBehavior.AllowGet);
                }

            }
        }

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                // var selected = (Object)null;

                if (data2 != "" && data2 != null && data2 != "0")
                {
                    int? selected = null;
                    var filter = Convert.ToInt32(data2);
                    var query = db.OthServiceBookActivity.Include(e => e.OthServiceBookPolicy).Where(e => e.OthServiceBookPolicy.Id == filter).SingleOrDefault();
                    List<LookupValue> lkValList = null;
                    if (query != null)
                    {
                        lkValList = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "2001").Select(e => e.LookupValues.ToList()).SingleOrDefault();
                    }
                    if (data2 != "" && data2 != null && data2 != "0")
                    {
                        //  selected = Convert.ToInt32(data2);
                    }
                    if (lkValList != null)
                    {
                        SelectList s = new SelectList(lkValList, "Id", "LookupVal", selected);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
            }
            return null;
        }

        public ActionResult PopulateDropDownFunction(string data, string data2, string data3)
        {
            //data parent-id
            //data2-empid
            //data3-type
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = null;
                switch (data3)
                {
                    case "OTHERACTIVITY":
                        int EmpId = Convert.ToInt32(data2);// form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);//modified by prashant 15042017

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

                        s = new SelectList(qurey, "Id", "Name", selected);
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
                            selected1 = Convert.ToInt32(data2);
                        }

                        s = new SelectList(OthSerBookPly, "Id", "Name", selected1);
                        break;
                    default:
                        break;
                }

                //var qurey = db.PromoActivity.ToList();//modified by prashant 15042017
                // var qurey = db.OthServiceBookActivity.ToList();//modified by prashant 15042017

                return Json(s, JsonRequestBehavior.AllowGet);
                //return View();
            }
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        #region DDLIST
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
                    .Include(e => e.OthServiceBookActivity)
                    .Include(e => e.OthServiceBookActivity.Select(a => a.OthServiceBookPolicy))
                    .SingleOrDefault();//modified by prashant 15042017
                // .Select(r => r.OthServiceBookActivity).FirstOrDefault();//modified by prashant 15042017
                var id = Convert.ToInt32(data);

                var qurey = OPromo.OthServiceBookActivity.Where(e =>e.OthServiceBookPolicy!= null && e.OthServiceBookPolicy.Id == id).Select(e => e.OthServiceBookPolicy).ToList();//modified by prashant 15042017
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }
        //public ActionResult PopulateDropDownPolicyList(string data, string data2)
        //{
        //    //data2-empid
        //    //data-activityid
        //    int EmpId = Convert.ToInt32(data2);// form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);//modified by prashant 15042017

        //    var OPayscale = db.Employee.Where(e => e.Id == EmpId)//modified by prashant 15042017
        //        .Select(r => r.EmpOffInfo.PayScale).FirstOrDefault();//modified by prashant 15042017
        //    var OPromo = db.PayScaleAgreement.Where(e => e.EndDate == null && e.PayScale.Id == OPayscale.Id)
        //        .Include(e => e.OthServiceBookActivity)
        //        .Include(e => e.OthServiceBookActivity.Select(a => a.OthServiceBookPolicy))
        //        .SingleOrDefault();//modified by prashant 15042017
        //    // .Select(r => r.OthServiceBookActivity).FirstOrDefault();//modified by prashant 15042017
        //    var activityid = Convert.ToInt32(data);
        //    var qurey = OPromo.OthServiceBookActivity.Where(e => e.Id == activityid).Select(e => e.OthServiceBookPolicy).ToList();//modified by prashant 15042017

        //    //var qurey = db.PromoActivity.ToList();//modified by prashant 15042017
        //    // var qurey = db.OthServiceBookActivity.ToList();//modified by prashant 15042017
        //    var selected = (Object)null;

        //    SelectList s = new SelectList(qurey, "Id", "Name", selected);
        //    return Json(s, JsonRequestBehavior.AllowGet);
        //}
        //public ActionResult PopulateDropDownActivityList(string data, string data2)//modified by prashant 15042017
        //{

        //    int EmpId = Convert.ToInt32(data);// form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);//modified by prashant 15042017

        //    var OPayscale = db.Employee.Where(e => e.Id == EmpId)//modified by prashant 15042017
        //        .Select(r => r.EmpOffInfo.PayScale).FirstOrDefault();//modified by prashant 15042017
        //    var OPromo = db.PayScaleAgreement.Where(e => e.EndDate == null && e.PayScale.Id == OPayscale.Id)
        //        .Include(e => e.OthServiceBookActivity)
        //        .SingleOrDefault();//modified by prashant 15042017
        //    // .Select(r => r.OthServiceBookActivity).FirstOrDefault();//modified by prashant 15042017
        //    var qurey = OPromo.OthServiceBookActivity.ToList();//modified by prashant 15042017

        //    //var qurey = db.PromoActivity.ToList();//modified by prashant 15042017
        //    // var qurey = db.OthServiceBookActivity.ToList();//modified by prashant 15042017
        //    var selected = (Object)null;
        //    if (data2 != "" && data != "0" && data2 != "0")
        //    {
        //        selected = Convert.ToInt32(data2);
        //    }

        //    SelectList s = new SelectList(qurey, "Id", "Name", selected);
        //    return Json(s, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult PopulateDropDownPayscaleList(string data, string data2)
        //{


        //    int CompId = Convert.ToInt32(Session["CompId"]);//modified by prashant 15042017
        //    var qurey1 = db.Company.Where(e => e.Id == CompId)//modified by prashant 15042017
        //        .Include(e => e.PayScale).FirstOrDefault();//modified by prashant 15042017
        //    var qurey = qurey1.PayScale.ToList();//modified by prashant 15042017

        //    // var qurey = db.PayScale.ToList();//modified by prashant 15042017
        //    var selected = (Object)null;
        //    if (data2 != "" && data != "0" && data2 != "0")
        //    {
        //        selected = Convert.ToInt32(data2);
        //    }

        //    SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
        //    return Json(s, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult PopulateDropDownPayscaleAgreementList(string data, string data2)
        //{
        //    int EmpId = Convert.ToInt32(data);// form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);

        //    var OPayscale = db.Employee.Where(e => e.Id == EmpId)
        //        .Include(r => r.EmpOffInfo.PayScale).FirstOrDefault();
        //    var OPayscaleAgreement = db.PayScaleAgreement.Where(e => e.EndDate == null && e.PayScale.Id == OPayscale.EmpOffInfo.PayScale.Id)
        //        .Select(r => r.PromoActivity).FirstOrDefault();
        //    var qurey = db.PayScaleAgreement.ToList();


        //    var selected = (Object)null;
        //    if (data2 != "" && data != "0" && data2 != "0")
        //    {
        //        selected = Convert.ToInt32(data2);
        //    }

        //    SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
        //    return Json(s, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult GetStruct(String empid)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var emp_id = Convert.ToInt32(empid);
                var emp_data = db.Employee.Where(e => e.Id == emp_id).Include(e => e.PayStruct).Include(e => e.PayStruct.Grade).Include(e => e.PayStruct.Level)
                                    .Include(e => e.FuncStruct).Include(e => e.FuncStruct.Job).Include(e => e.FuncStruct.JobPosition)
                                    .Include(e => e.GeoStruct)
                                    .Include(e => e.GeoStruct.Location).Include(e => e.GeoStruct.Department)
                                   .SingleOrDefault();
                var pay_data = (Object)null;
                if (emp_data.PayStruct != null)
                {
                    pay_data = db.PayStruct.Where(e => e.Id == emp_data.PayStruct.Id)
                       .Select(e => new
                       {
                           code = e.Id.ToString(),
                           value = e.Grade.Code + " - " + e.Grade.Name + e.JobStatus.FullDetails.ToString()
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
                        value = e.Location.LocationObj.LocCode.ToString() + " " + e.Location.LocationObj.LocDesc.ToString() + " " + e.Department.DepartmentObj.DeptDesc.ToString()
                    }).ToList();
                }
                //var geo_data = db.GeoStruct.Where(e=>e.Id==emp_data.GeoStruct.Id)
                return Json(new Object[] { fun_data, pay_data, geo_data }, JsonRequestBehavior.AllowGet);
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
                var fun_data = db.FuncStruct.Select(e => new
                {
                    code = e.Id.ToString(),
                    value = "Id:" + e.Id.ToString() + " " + e.Job.Name.ToString() + "" + e.JobPosition.JobPositionDesc.ToString(),

                }).ToList();
                return Json(fun_data, JsonRequestBehavior.AllowGet);
            }
        }


        //public ActionResult GetNewPayStructDetails(string data)
        //{
        //    var pay_data = db.PayStruct
        //    .Select(e => new
        //    {
        //        code = e.Id.ToString(),
        //        value = e.Grade.Name.ToString() + " " + e.Level.Name.ToString()
        //    }).ToList();

        //    return Json(pay_data, JsonRequestBehavior.AllowGet);



        ////public ActionResult GetNewPayStructDetails(string data)
        ////{
        ////    var pay_data = db.PayStruct
        ////    .Select(e => new
        ////    {
        ////        code = e.Id.ToString(),
        ////        value = e.Grade.Name.ToString() + " " + e.Level.Name.ToString()
        ////    }).ToList();

        ////    return Json(pay_data, JsonRequestBehavior.AllowGet);


        ////}


        public ActionResult GetNewPayStructDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var pay_data = db.JobStatus.Include(r => r.EmpActingStatus).Include(r => r.EmpStatus)
                .Select(e => new
                {
                    code = e.Id.ToString(),
                    value = "EmpStatus :" + e.EmpStatus.LookupVal.ToString() + " " + "EmpActingStatus:" + e.EmpActingStatus.LookupVal.ToString()
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
                    var query = db.OthServiceBookActivity.Include(e => e.OtherSerBookActList).Where(e => e.OtherSerBookActList.Id == id).ToList();
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
        public ActionResult Create(OtherServiceBook OtherServiceBook, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string NewFuncStruct = form["NewFuncT-table"] == "0" ? "" : form["NewFuncT-table"];
                    string NewPayStruct = form["NewPayT-table"] == "0" ? "" : form["NewPayT-table"];
                    string OtherActivity = form["OtherActivitylist"] == "0" ? "" : form["OtherActivitylist"];
                    string OtherPolicy = form["otherpolicy"] == "0" ? "" : form["otherpolicy"];
                    string ProcessOthDate = form["ProcessOthDate"] == "0" ? "" : form["ProcessOthDate"];
                    string ReasonOfLeaving = form["leavingreason_drop"] == "0" ? "" : form["leavingreason_drop"];

                    var date = Convert.ToDateTime(ProcessOthDate).ToString("MM/yyyy");

                    try
                    {
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
                            // return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                            Msg.Add(" Kindly select employee...  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }

                        ////////new 21/08/2019
                        var check = db.CPIEntryT.Where(e => e.PayMonth == date).ToList();

                        if (check.Count() == 0)
                        {
                            Msg.Add("Kindly run CPI first and then try again");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        ///////////

                        int OtherActivityId = 0, ReasonId = 0;
                        string LookupVal = "", Narration = "";
                        if (OtherActivity != null && OtherActivity != "")
                        {
                            OtherActivityId = int.Parse(OtherActivity);
                            LookupVal = db.LookupValue.Where(e => e.Id == OtherActivityId).Select(e => e.LookupVal.ToUpper()).SingleOrDefault();
                        }

                        if (ReasonOfLeaving != null && ReasonOfLeaving != "")
                        {
                            ReasonId = int.Parse(ReasonOfLeaving);
                            Narration = db.LookupValue.Where(e => e.Id == ReasonId).Select(e => e.LookupVal.ToUpper()).SingleOrDefault();
                            if (Narration != "OTHER")
                                OtherServiceBook.Narration = Narration;

                        }




                        Employee OEmployee = null;
                        EmployeePayroll OEmployeePayroll = null;
                        //int PayScaleAgrId = int.Parse(PayScaleAgr);
                        //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();

                        OEmployee = db.Employee.Include(e => e.EmpName).Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus).Include(e => e.PayStruct.JobStatus.EmpStatus)
                                        .Include(e => e.EmpOffInfo).Include(e => e.EmpOffInfo.PayScale)
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

                            //var Seniorityobj = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus).Include(e => e.PayStruct.JobStatus.EmpStatus).Where(e => e.ServiceBookDates.ServiceLastDate == null && e.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT" && (e.ServiceBookDates.SeniorityNo != "0" && e.ServiceBookDates.SeniorityNo != null && e.ServiceBookDates.SeniorityNo != "")).ToList();
                            var Seniorityobj = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus).Include(e => e.PayStruct.JobStatus.EmpStatus).Where(e => e.ServiceBookDates.ServiceLastDate == null && EmpActingStatus.Contains(e.PayStruct.JobStatus.Id) && (e.ServiceBookDates.SeniorityNo != "0" && e.ServiceBookDates.SeniorityNo != null && e.ServiceBookDates.SeniorityNo != "")).ToList();

                            if (Seniorityobj.Count() > 0)
                            {
                                var OEmployeecheck = db.Employee.Include(e => e.EmpName).Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus).Include(e => e.PayStruct.JobStatus.EmpStatus)
                                                                 .Include(e => e.ServiceBookDates)
                                                                 .Where(e => e.ServiceBookDates.ServiceLastDate == null && EmpActingStatus.Contains(e.PayStruct.JobStatus.Id) && (e.ServiceBookDates.SeniorityNo == "0" || e.ServiceBookDates.SeniorityNo == "" || e.ServiceBookDates.SeniorityNo == null)).ToList();
                                //  .Where(e => e.ServiceBookDates.ServiceLastDate == null && e.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT" && (e.ServiceBookDates.SeniorityNo == "0" || e.ServiceBookDates.SeniorityNo == "" || e.ServiceBookDates.SeniorityNo == null)).ToList();
                                if (OEmployeecheck.Count() > 0)
                                {
                                    foreach (var item in OEmployeecheck)
                                    {

                                        //if (item.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT" && (item.ServiceBookDates.SeniorityNo == "0" || item.ServiceBookDates.SeniorityNo == "" || item.ServiceBookDates.SeniorityNo == null))
                                        if ((item.ServiceBookDates.SeniorityNo == "0" || item.ServiceBookDates.SeniorityNo == "" || item.ServiceBookDates.SeniorityNo == null))
                                        {

                                            Msg.Add("Please Assign Seniority No in Personal info:" + item.EmpCode + " " + item.EmpName.FullNameFML);

                                        }
                                        // if (item.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT" && item.ServiceBookDates.SeniorityDate == null)
                                        if (item.ServiceBookDates.SeniorityDate == null)
                                        {

                                            Msg.Add("Please Assign Seniority date in Personal info:" + item.EmpCode + " " + item.EmpName.FullNameFML);

                                        }
                                    }
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }

                        OEmployeePayroll = db.EmployeePayroll.Include(e => e.OtherServiceBook)
                            .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity))
                            .Where(e => e.Employee.Id == Emp).SingleOrDefault();

                        if (OEmployeePayroll.OtherServiceBook.Any(d => d.ProcessOthDate.Value.ToShortDateString() == ProcessOthDate.ToString() && d.ReleaseFlag == false))
                        {
                            {

                                Msg.Add("Please Release The Activity and Try Again:" + OEmployee.EmpCode + " " + OEmployee.EmpName.FullNameFML + " on date= " + ProcessOthDate);
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }



                        if (OEmployeePayroll.OtherServiceBook.Any(a => a.ProcessOthDate.Value.ToShortDateString() == ProcessOthDate.ToString() && a.OthServiceBookActivity.Id == OtherActivityId))
                        {
                            Msg.Add("Already Defined For Date=" + ProcessOthDate);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        //check activity policy

                        int OtherActivityPolicyId = 0;


                        if (OtherPolicy != null && OtherPolicy != "")
                        {
                            int PayScaleAgrId = db.PayScaleAgreement.Where(e => e.EndDate == null).Select(e => e.Id).SingleOrDefault();
                            var Newagreement = db.PayScaleAgreement.Include(x => x.OthServiceBookActivity)
                      .Include(x => x.OthServiceBookActivity.Select(y => y.OthServiceBookPolicy))
                       .Include(x => x.OthServiceBookActivity.Select(y => y.OtherSerBookActList))
                      .Where(x => x.Id == PayScaleAgrId).FirstOrDefault();
                            List<OthServiceBookActivity> Otheract = Newagreement.OthServiceBookActivity.ToList();
                            OtherActivityPolicyId = int.Parse(OtherPolicy);
                            // OtherServiceBook.OthServiceBookActivity = db.OthServiceBookActivity.Include(e => e.OthServiceBookPolicy).Where(e => e.OthServiceBookPolicy.Id == OtherActivityPolicyId).SingleOrDefault();
                            OtherServiceBook.OthServiceBookActivity = Otheract.Where(e => e.OthServiceBookPolicy.Id == OtherActivityPolicyId).SingleOrDefault();

                        }

                        if (OtherServiceBook.OthServiceBookActivity.OthServiceBookPolicy.IsFuncStructChange == true)
                            if (NewFuncStruct != "" && NewFuncStruct != null)
                                OtherServiceBook.NewFuncStruct = db.FuncStruct.Find(int.Parse(NewFuncStruct));
                            else
                            //return Json(new Object[] { "", "", "Kindly select NewFuncStruct." }, JsonRequestBehavior.AllowGet);
                            {
                                List<string> Msgn = new List<string>();
                                Msgn.Add("Kindly select NewFuncStruct.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msgn }, JsonRequestBehavior.AllowGet);
                            }
                        else
                            OtherServiceBook.NewFuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);


                        if (OtherServiceBook.OthServiceBookActivity.OthServiceBookPolicy.IsPayJobStatusChange == true)
                            if (NewPayStruct != "" && NewPayStruct != null)
                                OtherServiceBook.NewPayStruct = db.PayStruct.Find(int.Parse(NewPayStruct));
                            else
                            //return Json(new Object[] { "", "", "Kindly select NewPayStruct." }, JsonRequestBehavior.AllowGet);
                            {
                                List<string> Msgn = new List<string>();
                                Msgn.Add("Kindly select NewPayStruct.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msgn }, JsonRequestBehavior.AllowGet);
                            }
                        else
                            OtherServiceBook.NewPayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);

                       


                        if (OEmployee.EmpOffInfo != null && OEmployee.EmpOffInfo.PayScale != null)
                            OtherServiceBook.NewPayScale = db.PayScale.Find(OEmployee.EmpOffInfo.PayScale.Id);

                        if (OEmployee.EmpOffInfo != null && OEmployee.EmpOffInfo.PayScale != null)
                        {
                            var PayScaleAgr = db.PayScaleAgreement.Where(e => e.PayScale.Id == OEmployee.EmpOffInfo.PayScale.Id && e.EndDate == null).SingleOrDefault();
                            OtherServiceBook.NewPayScaleAgreement = PayScaleAgr;
                        }

                        if (OEmployee.FuncStruct != null)
                            OtherServiceBook.OldFuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);

                        if (OEmployee.EmpOffInfo.PayScale != null)
                            OtherServiceBook.OldPayScale = db.PayScale.Find(OEmployee.EmpOffInfo.PayScale.Id);

                        if (OEmployee.PayStruct != null)
                            OtherServiceBook.OldPayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);

                        if (OEmployee.GeoStruct != null)
                            OtherServiceBook.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);

                        // Hindustan bank
                        var CompCode = db.Company.Where(e => e.Id == CompId).AsNoTracking().AsParallel().Select(a => a.Code.ToUpper()).SingleOrDefault();  //asd
                        if (OtherServiceBook.OthServiceBookActivity.OthServiceBookPolicy.IsFuncStructChange == true)
                        {
                            if (NewFuncStruct != "" && NewFuncStruct != null)
                            {
                                if (CompCode == "HCBL")
                                {
                                    int funid = Convert.ToInt32(NewFuncStruct);
                                    var newjobposition = db.FuncStruct.Include(e => e.JobPosition).Where(e => e.Company.Id == CompId && e.Id == funid).FirstOrDefault();
                                    var oldjobposition = db.FuncStruct.Include(e => e.JobPosition).Where(e => e.Company.Id == CompId && e.Id == OEmployee.FuncStruct.Id).FirstOrDefault();
                                    if ((newjobposition.JobPosition == null ? "" : newjobposition.JobPosition.JobPositionDesc.ToUpper()) != oldjobposition.JobPosition.JobPositionDesc.ToUpper())
                                    {
                                        //   List<string> Msg = new List<string>();
                                        Msg.Add("Job Position Should be Same old and new function struct ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                        }

                        if (LevelSettings != null && LevelSettings.CCode == CompCode)
                        {
                            int payid = Convert.ToInt32(NewPayStruct);
                            var newlevel = db.PayStruct.Include(e => e.Level).Where(e => e.Company.Id == CompId && e.Id == payid).FirstOrDefault();
                            var oldlevel = db.PayStruct.Include(e => e.Level).Where(e => e.Company.Id == CompId && e.Id == OtherServiceBook.NewPayStruct.Id).FirstOrDefault();
                            if ((newlevel.Level == null ? "" : newlevel.Level.Code.ToUpper()) != (oldlevel.Level == null ? "" : oldlevel.Level.Code.ToUpper()))
                            {

                                Msg.Add("Level Should be Same old and new pay struct ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        // check if index proces and salary not process do activity retirement give message
                        if (OtherServiceBook.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "RETIRED")
                        {
                            if (check.Count() != 0)
                            {
                                var checkSal = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == date).SingleOrDefault();
                                if (checkSal == null)
                                {
                                    Msg.Add("Please Process Salary and Lock For month " + date + " Then Try.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                var checkSallock = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == date && e.ReleaseDate == null).SingleOrDefault();
                                if (checkSallock != null)
                                {
                                    Msg.Add("Please Lock Salary For the month " + date + " Then Try.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }

                        }
                        if (OtherServiceBook.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "RESIGNED")
                        {
                            if (check.Count() != 0)
                            {
                                var checkSal = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == date).SingleOrDefault();
                                if (checkSal == null)
                                {
                                    Msg.Add("Please Process Salary and Lock For month " + date + " Then Try.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                var checkSallock = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == date && e.ReleaseDate == null).SingleOrDefault();
                                if (checkSallock != null)
                                {
                                    Msg.Add("Please Lock Salary For the month " + date + " Then Try.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                        }
                        if (OtherServiceBook.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "EXPIRED")
                        {
                            if (check.Count() != 0)
                            {
                                var checkSal = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == date).SingleOrDefault();
                                if (checkSal == null)
                                {
                                    Msg.Add("Please Process Salary and Lock For month " + date + " Then Try.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                var checkSallock = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == date && e.ReleaseDate == null).SingleOrDefault();
                                if (checkSallock != null)
                                {
                                    Msg.Add("Please Lock Salary For the month " + date + " Then Try.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                        }
                        if (OtherServiceBook.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "TERMINATION")
                        {
                            if (check.Count() != 0)
                            {
                                var checkSal = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == date).SingleOrDefault();
                                if (checkSal == null)
                                {
                                    Msg.Add("Please Process Salary and Lock For month " + date + " Then Try.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                var checkSallock = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == date && e.ReleaseDate == null).SingleOrDefault();
                                if (checkSallock != null)
                                {
                                    Msg.Add("Please Lock Salary For the month " + date + " Then Try.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                        }


                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                          new System.TimeSpan(0, 30, 0)))
                        {
                            OtherServiceBook.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            OtherServiceBook OthServBook = new OtherServiceBook()
                            {
                                EmployeeCTC = null,
                                GeoStruct = OtherServiceBook.GeoStruct,
                                Narration = OtherServiceBook.Narration,
                                NewFuncStruct = OtherServiceBook.NewFuncStruct,
                                NewPayScale = OtherServiceBook.NewPayScale,
                                NewPayScaleAgreement = OtherServiceBook.NewPayScaleAgreement,
                                NewPayStruct = OtherServiceBook.NewPayStruct,//db.PayStruct.Find(PromotionServiceBook.NewPayStruct), 
                                OldFuncStruct = OtherServiceBook.OldFuncStruct,
                                OldPayScale = OtherServiceBook.OldPayScale,
                                OldPayScaleAgreement = OtherServiceBook.OldPayScaleAgreement,
                                OldPayStruct = OtherServiceBook.OldPayStruct,
                                ProcessOthDate = OtherServiceBook.ProcessOthDate,
                                OthServiceBookActivity = OtherServiceBook.OthServiceBookActivity,
                                DBTrack = OtherServiceBook.DBTrack
                            };


                            db.OtherServiceBook.Add(OthServBook);
                            db.SaveChanges();


                            List<OtherServiceBook> OthServBkList = new List<OtherServiceBook>();
                            OthServBkList.Add(OthServBook);

                            OthServBkList.AddRange(OEmployeePayroll.OtherServiceBook);

                            OEmployeePayroll.OtherServiceBook = OthServBkList;
                            db.EmployeePayroll.Attach(OEmployeePayroll);
                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;

                            //  DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
                            // Process.ServiceBook.ServiceBookProcess(CompId, "OTHER_RELEASE", null, null, null, OthServBook, OEmployeePayroll.Id, "OTHER", OthServBook.ProcessOthDate, false, false);

                            // DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
                            // db.RefreshAllEntites(RefreshMode.StoreWins);
                            ts.Complete();
                            List<string> Msgs = new List<string>();
                            Msgs.Add("Data Saved successfully");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        //   DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
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
                        //return Json(new Object[] { "", "", Msg }, JsonRequestBehavior.AllowGet);
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
        #endregion


        #region Release
        public ActionResult Release(OtherServiceBook OtherServiceBook, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var Emp = form["emp_Id"] == "0" ? "" : form["emp_Id"];
                    var ReleaseFlag = form["ReleaseFlag"] == "0" ? "" : form["ReleaseFlag"];
                    var Activityid = form["Activity_Id"] == "0" ? "" : form["Activity_Id"];
                    var SalHeadCount = form["SalHeadCount"] == "0" ? "0" : form["SalHeadCount"];
                    var ids = form["input_hidden_field"] != "" ? one_ids(form["input_hidden_field"]) : null;
                    var ReleaseDateStr = form["ReleaseDate"] == "0" ? "" : form["ReleaseDate"];
                    DateTime? ReleaseDate = !string.IsNullOrEmpty(ReleaseDateStr) ? (DateTime?)Convert.ToDateTime(ReleaseDateStr) : null;
                    try
                    {

                        int CompId = 0;
                        if (Session["CompId"] != null)
                        {
                            CompId = Convert.ToInt32(Session["CompId"]);
                        }
                        //List<int> ids = null; 
                        //if (forwarddata != null && forwarddata != "0" && forwarddata != "false")
                        //{
                        //    ids = Utility.StringIdsToListIds(forwarddata);
                        //}

                        //int Empid = 0;
                        //if (Emp != null && Emp !="")
                        //{
                        //    Empid = Convert.ToInt32(Emp);
                        //}
                        List<int> Empids = null;
                        //List<string> Msg = new List<string>();
                        if (Emp != null && Emp != "0" && Emp != "false")
                        {
                            Empids = Utility.StringIdsToListIds(Emp);
                        }
                        foreach (var Empid in Empids)
                        {
                            Employee OEmployee = null;
                            OEmployee = db.Employee
                                            .Where(r => r.Id == Empid).SingleOrDefault();

                            EmployeePayroll OEmployeePayroll = null;

                            OEmployeePayroll = db.EmployeePayroll.Include(e => e.OtherServiceBook).Include(e => e.OtherServiceBook.Select(r => r.OthServiceBookActivity))
                                .Include(e => e.OtherServiceBook.Select(r => r.OthServiceBookActivity.OtherSerBookActList))
                         .Where(e => e.Employee.Id == Empid).SingleOrDefault();
                            OtherServiceBook OtherId = OEmployeePayroll.OtherServiceBook.Where(e => e.ReleaseFlag == false).SingleOrDefault();
                            EmpSalStruct OEmpSalStruct = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).Where(e => e.EndDate == null && e.EmployeePayroll.Id == OEmployeePayroll.Id).SingleOrDefault();
                            if (OtherId.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "EXPIRED")
                            {
                                if (OEmpSalStruct != null)
                                {
                                    if (OtherId.ProcessOthDate < OEmpSalStruct.EffectiveDate)
                                    {
                                        string PayMonth = OEmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy");
                                        SalaryT OSalT = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.EmployeePayroll.Id == OEmployeePayroll.Id).SingleOrDefault();
                                        if (OSalT != null)
                                        {
                                            Msg.Add(" Delete salary first and then try this activity.  ");
                                        }
                                    }
                                }
                            }
                            if (ReleaseDate.Value.Date < OtherId.ProcessOthDate.Value.Date)
                            {
                                Msg.Add(" The " + OEmployee.EmpCode + " has Release date should be greater than the process date.");
                            }
                        }
                        if (Msg.Count > 0)
                        {
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }


                        foreach (var Empid in Empids)
                        {

                            // Employee OEmployee = null;
                            EmployeePayroll OEmployeePayroll = null;
                            //int PayScaleAgrId = int.Parse(PayScaleAgr);
                            //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();
                            //foreach (int item in ids)
                            //{
                            //   // int OtherId = item;

                            //OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                            //                .Include(e => e.EmpOffInfo)
                            //                .Where(r => r.Id == Empid).SingleOrDefault();

                            OEmployeePayroll = db.EmployeePayroll.Include(e => e.OtherServiceBook)
                          .Where(e => e.Employee.Id == Empid).SingleOrDefault();



                            int OtherId = OEmployeePayroll.OtherServiceBook.Where(e => e.ReleaseFlag == false).SingleOrDefault().Id;


                            OtherServiceBook.ReleaseFlag = Convert.ToBoolean(ReleaseFlag);
                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                              new System.TimeSpan(0, 30, 0)))
                            {



                                OtherServiceBook OOtherServiceBook = db.OtherServiceBook.Where(e => e.Id == OtherId).SingleOrDefault();

                                Process.ServiceBook.ServiceBookProcess("",CompId, "OTHER_RELEASE", null, null, null, OOtherServiceBook.Id, OEmployeePayroll.Id, "OTHER", OOtherServiceBook.ProcessOthDate, false, false, 0, null);

                                if (ids != null && ids.Count() > 0)
                                {
                                    using (DataBaseContext db2=new DataBaseContext())
                                    {
                                        List<EmpSalStruct> OEmpSalStruct = db2.EmpSalStruct.Include(e => e.EmpSalStructDetails).Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead)).Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula))
                                 .Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id && e.EffectiveDate >= OOtherServiceBook.ProcessOthDate).ToList();
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
                                                        db2.EmpSalStructDetails.Attach(OEmpSalStructDet);
                                                        db2.Entry(OEmpSalStructDet).State = System.Data.Entity.EntityState.Modified;
                                                        db2.SaveChanges();
                                                        db2.Entry(OEmpSalStructDet).State = System.Data.Entity.EntityState.Detached;
                                                    }

                                                }
                                            }
                                        } 
                                    }
                                    
                                }
                                //  DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
                                ts.Complete();

                                //   return Json(new { success = true, responseText = "Data Updated Successfully." }, JsonRequestBehavior.AllowGet);

                            }
                            //}
                        }
                        Msg.Add(" Data Updated Successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                    }
                    catch (Exception ex)
                    {
                        //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
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
        #endregion



        public ActionResult Release1(OtherServiceBook OtherServiceBook, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var Emp = form["emp_Id"] == "0" ? "" : form["emp_Id"];
                    var ReleaseFlag = form["ReleaseFlag"] == "0" ? "" : form["ReleaseFlag"];
                    try
                    {

                        int CompId = 0;
                        if (Session["CompId"] != null)
                        {
                            CompId = Convert.ToInt32(Session["CompId"]);
                        }

                        int Empid = 0;
                        if (Emp != null && Emp != "")
                        {
                            Empid = Convert.ToInt32(Emp);
                        }

                        Employee OEmployee = null;
                        EmployeePayroll OEmployeePayroll = null;
                        //int PayScaleAgrId = int.Parse(PayScaleAgr);
                        //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();

                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                        .Include(e => e.EmpOffInfo)
                                        .Where(r => r.Id == Empid).SingleOrDefault();

                        OEmployeePayroll = db.EmployeePayroll
                      .Where(e => e.Employee.Id == Empid).SingleOrDefault();

                        List<int> ids = null;
                        if (forwarddata != null && forwarddata != "0" && forwarddata != "false")
                        {
                            ids = Utility.StringIdsToListIds(forwarddata);
                        }

                        foreach (int item in ids)
                        {
                            int OtherId = item;



                            OtherServiceBook.ReleaseFlag = Convert.ToBoolean(ReleaseFlag);
                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                              new System.TimeSpan(0, 30, 0)))
                            {



                                OtherServiceBook OOtherServiceBook = db.OtherServiceBook.Where(e => e.Id == OtherId).SingleOrDefault();
                                Process.ServiceBook.ServiceBookProcess("",CompId, "OTHER_RELEASE", null, null, null, OOtherServiceBook.Id, OEmployeePayroll.Id, "OTHER", OOtherServiceBook.ProcessOthDate, false, false, 0, null);
                                //  DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
                                ts.Complete();

                                //   return Json(new { success = true, responseText = "Data Updated Successfully." }, JsonRequestBehavior.AllowGet);

                            }
                        }
                        Msg.Add(" Data Updated Successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                    }
                    catch (Exception ex)
                    {
                        //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
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

        public JsonResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.PromotionServiceBook
                    .Include(e => e.PromotionActivity.PromoList).Include(e => e.OldPayStruct.JobStatus)
                                .Include(e => e.NewPayStruct).Include(e => e.NewPayStruct.Level).Include(e => e.NewPayStruct.Grade)
                                .Include(e => e.NewPayStruct.JobStatus)
                                .Include(e => e.OldFuncStruct).Include(e => e.OldFuncStruct.Job).Include(e => e.OldFuncStruct.JobPosition)
                                .Include(e => e.NewFuncStruct).Include(e => e.NewFuncStruct.Job).Include(e => e.NewFuncStruct.JobPosition)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        PromotionActivity = e.PromotionActivity.PromoList.LookupVal,
                        ProcessPromoDate = e.ProcessPromoDate,
                        OldPayStruct_FullDetails = e.OldPayStruct == null ? null : e.OldPayStruct.FullDetails,
                        OldPayStruct_Id = e.OldPayStruct.Id == null ? "" : e.OldPayStruct.Id.ToString(),
                        NewPayStruct_FullDetails = e.NewPayStruct == null ? null : e.NewPayStruct.FullDetails,
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
                        Action = e.DBTrack.Action
                    }).ToList();

                var PromoServBook = db.PromotionServiceBook.Find(data);
                Session["RowVersion"] = PromoServBook.RowVersion;
                var Auth = PromoServBook.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(new Object[] { Q, Auth }, JsonRequestBehavior.AllowGet);
            }
        }

        public class OthServBookGridData
        {
            //added by nikhil 14/4/2017
            public int Id { get; set; }
            public int EmpId { get; set; }
            public string EmpCode { get; set; }
            public string Employee { get; set; }
            public string OthServiceBookActivity { get; set; }
            public string ActivityDate { get; set; }
            public string OldFuncStruct { get; set; }
            public string NewFuncStruct { get; set; }
            public string OldPayStruct { get; set; }
            public string NewPayStruct { get; set; }
            public string IsBasicChangeApp { get; set; }
            public string NewBasic { get; set; }
            public string Narration { get; set; }

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

        public class OthChildDataClass
        {
            public int Id { get; set; }
            public bool Release { get; set; }
            public string ReleaseDate { get; set; }
            public string Activity { get; set; }
            public string ActivityDate { get; set; }
            public string OldFunc { get; set; }
            public string NewFunc { get; set; }
            public string OldPay { get; set; }
            public string NewPay { get; set; }
            public string Narration { get; set; }
        }

        public ActionResult P2BGridRelease(P2BGrid_Parameters gp)
        {
            //Added by nikhil 14/4/2017
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<OthServBookGridData> PromoServBook = null;
                List<OthServBookGridData> model = new List<OthServBookGridData>();
                OthServBookGridData view = null;

                var OEmployee = db.EmployeePayroll
                              .Include(e => e.Employee)
                              .Include(e => e.OtherServiceBook)
                              .Include(e => e.Employee.EmpName).ToList();

                foreach (var z in OEmployee)
                {
                    if (z.OtherServiceBook != null && z.OtherServiceBook.Count > 0)
                    {
                        var ObjPromoServBook = db.EmployeePayroll.Where(e => e.Id == z.Id).Include(e => e.OtherServiceBook)
                            .Select(e => e.OtherServiceBook.Where(r => r.ReleaseFlag != true))
                                            .SingleOrDefault();


                        //DateTime? Eff_Date = null;
                        //PayScaleAgreement PayScaleAgr = null;

                        foreach (var a in ObjPromoServBook)
                        {

                            //Eff_Date = Convert.ToDateTime(a.ProcessMonth);
                            var aa = db.OtherServiceBook.Include(e => e.OldPayStruct).Include(e => e.OldPayStruct.Level).Include(e => e.OldPayStruct.Grade)
                                .Include(e => e.OldPayStruct.JobStatus)
                                .Include(e => e.NewPayStruct).Include(e => e.NewPayStruct.Level).Include(e => e.NewPayStruct.Grade)
                                .Include(e => e.NewPayStruct.JobStatus)
                                .Include(e => e.OldFuncStruct).Include(e => e.OldFuncStruct.Job).Include(e => e.OldFuncStruct.JobPosition)
                                .Include(e => e.NewFuncStruct).Include(e => e.NewFuncStruct.Job).Include(e => e.NewFuncStruct.JobPosition)
                                .Include(e => e.OthServiceBookActivity).Include(e => e.OthServiceBookActivity.OtherSerBookActList)
                                .Where(e => e.Id == a.Id && e.ReleaseFlag != true).SingleOrDefault();


                            view = new OthServBookGridData()
                            {
                                Id = a.Id,
                                EmpId = z.Employee.Id,
                                EmpCode = z.Employee.EmpCode.ToString(),
                                Employee = z.Employee.EmpName.FullNameFML.ToString(),
                                OthServiceBookActivity = a.OthServiceBookActivity != null ? a.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToString() : "",
                                ActivityDate = a.ProcessOthDate.ToString(),
                                NewFuncStruct = a.NewFuncStruct == null ? null : a.NewFuncStruct.FullDetails,
                                OldFuncStruct = a.OldFuncStruct == null ? null : a.OldFuncStruct.FullDetails,
                                NewPayStruct = a.NewPayStruct == null ? null : a.NewPayStruct.FullDetails,
                                OldPayStruct = a.OldPayStruct == null ? null : a.OldPayStruct.FullDetails,
                                IsBasicChangeApp = a.IsBasicChangeAppl.ToString(),
                                NewBasic = a.NewBasic.ToString(),
                                Narration = a.Narration

                            };

                            model.Add(view);
                        }
                    }

                }

                PromoServBook = model;

                IEnumerable<OthServBookGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = PromoServBook;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpId.ToString().Contains(gp.searchString))
                                || (e.EmpCode.ToString().Contains(gp.searchString))
                                || (e.Employee.ToString().Contains(gp.searchString))
                                || (e.ActivityDate.ToString().Contains(gp.searchString))
                                || (e.OldFuncStruct.ToString().Contains(gp.searchString))
                                || (e.NewFuncStruct.ToString().Contains(gp.searchString))
                                || (e.OldPayStruct.ToString().Contains(gp.searchString))
                                || (e.NewPayStruct.ToString().Contains(gp.searchString))
                                || (e.IsBasicChangeApp.ToString().Contains(gp.searchString))
                                || (e.NewBasic.ToString().Contains(gp.searchString))
                                || (e.Narration.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new { a.EmpCode, a.Employee, a.OthServiceBookActivity, a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct, a.IsBasicChangeApp, a.NewBasic, a.Narration, a.Id, a.EmpId }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.Employee, a.OthServiceBookActivity != null ? Convert.ToString(a.OthServiceBookActivity) : "", a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct, a.IsBasicChangeApp, a.NewBasic, a.Narration, a.Id, a.EmpId }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = PromoServBook;
                    Func<OthServBookGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpId" ? c.EmpId.ToString() :
                                         gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.ToString() :
                                         gp.sidx == "Activity" ? c.OthServiceBookActivity :
                                         gp.sidx == "Process Date" ? c.ActivityDate.ToString() :
                                         gp.sidx == "Old FuncStruct" ? c.OldFuncStruct :
                                         gp.sidx == "New FuncStruct" ? c.NewFuncStruct :
                                         gp.sidx == "Old PayStruct" ? c.OldPayStruct :
                                         gp.sidx == "New PayStruct" ? c.NewPayStruct :
                                         gp.sidx == "Is Basic Change App" ? c.IsBasicChangeApp :
                                         gp.sidx == "New Basic" ? c.NewBasic :
                                         gp.sidx == "Narration" ? c.Narration : ""
                                          );
                    }
                    if (gp.sord == "asc")  //Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : ""
                    {
                        IE = IE.OrderBy(orderfuc);  // a.Id,a.Employeee.EmpCode,a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.Employee, a.OthServiceBookActivity != null ? Convert.ToString(a.OthServiceBookActivity) : "", a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct, a.IsBasicChangeApp, a.NewBasic, a.Narration, a.Id, a.EmpId }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.Employee, a.OthServiceBookActivity != null ? Convert.ToString(a.OthServiceBookActivity) : "", a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct, a.IsBasicChangeApp, a.NewBasic, a.Narration, a.Id, a.EmpId }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.Employee, a.OthServiceBookActivity != null ? Convert.ToString(a.OthServiceBookActivity) : "", a.ActivityDate, a.OldFuncStruct, a.NewFuncStruct, a.OldPayStruct, a.NewPayStruct, a.IsBasicChangeApp, a.NewBasic, a.Narration, a.Id, a.EmpId }).ToList();
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
                        fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
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
                                Name = item.Employee.EmpName.FullNameFML,
                                JoiningDate = item.Employee.ServiceBookDates.JoiningDate.Value.Date.ToString(),
                                Job = item.Employee.FuncStruct.Job.Name,
                                Grade = item.Employee.PayStruct.Grade.Name,
                                Location = item.Employee.GeoStruct.Location.LocationObj.LocDesc
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

        public ActionResult Get_OthServBook(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.OtherServiceBook.Select(r => r.OthServiceBookActivity))
                        .Include(e => e.OtherServiceBook.Select(r => r.OthServiceBookActivity.OtherSerBookActList))
                        .Include(e => e.OtherServiceBook.Select(r => r.OldFuncStruct))
                        .Include(e => e.OtherServiceBook.Select(r => r.OldFuncStruct.Job))
                        .Include(e => e.OtherServiceBook.Select(r => r.OldFuncStruct.JobPosition))
                        .Include(e => e.OtherServiceBook.Select(r => r.NewFuncStruct))
                        .Include(e => e.OtherServiceBook.Select(r => r.NewFuncStruct.Job))
                        .Include(e => e.OtherServiceBook.Select(r => r.NewFuncStruct.JobPosition))
                        .Include(e => e.OtherServiceBook.Select(r => r.OldPayStruct))
                        .Include(e => e.OtherServiceBook.Select(r => r.OldPayStruct.Grade))
                        .Include(e => e.OtherServiceBook.Select(r => r.OldPayStruct.Level))
                        .Include(e => e.OtherServiceBook.Select(r => r.OldPayStruct.JobStatus))
                        .Include(e => e.OtherServiceBook.Select(r => r.NewPayStruct))
                        .Include(e => e.OtherServiceBook.Select(r => r.NewPayStruct.Grade))
                        .Include(e => e.OtherServiceBook.Select(r => r.NewPayStruct.Level))
                        .Include(e => e.OtherServiceBook.Select(r => r.NewPayStruct.JobStatus))
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<OthChildDataClass> returndata = new List<OthChildDataClass>();
                        foreach (var item in db_data.OtherServiceBook)
                        {
                            returndata.Add(new OthChildDataClass
                            {
                                Id = item.Id,
                                Release = item.ReleaseFlag,
                                ReleaseDate = item.ReleaseDate != null ? item.ReleaseDate.Value.ToShortDateString() : null,
                                Activity = item.OthServiceBookActivity != null ? item.OthServiceBookActivity.OtherSerBookActList.LookupVal : null,
                                ActivityDate = item.ProcessOthDate != null ? item.ProcessOthDate.Value.ToShortDateString() : null,
                                Narration = item.Narration,
                                OldFunc = item.OldFuncStruct != null ? item.OldFuncStruct.FullDetails : null,
                                NewFunc = item.NewFuncStruct != null ? item.NewFuncStruct.FullDetails : null,
                                OldPay = item.OldPayStruct != null ? item.OldPayStruct.FullDetails : null,
                                NewPay = item.NewPayStruct != null ? item.NewPayStruct.FullDetails : null
                            });
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception ex)
                {
                    List<string> Msg = new List<string>();
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
        public ActionResult GridDelete(int data)
        {
            List<string> Msg = new List<string>();

            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    OtherServiceBook OthServBook = db.OtherServiceBook.Where(e => e.Id == data).SingleOrDefault();

                    if (OthServBook.ReleaseFlag == true)
                    {
                        return this.Json(new { status = true, valid = true, responseText = "You cannot delete as activity is already released.", JsonRequestBehavior.AllowGet });
                    }

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            db.OtherServiceBook.Attach(OthServBook);
                            db.Entry(OthServBook).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            db.Entry(OthServBook).State = System.Data.Entity.EntityState.Detached;
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
    }

    
}