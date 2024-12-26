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
    [AuthoriseManger]
    public class TransferServiceBookController : Controller
    {
     
         readonly IP2BINI p2BINI;
         readonly LevelSettings LevelSettings;

         private readonly Type Type;

         public TransferServiceBookController()
        {
            p2BINI = P2BINI.RegisterSettings();
            LevelSettings = new LevelSettings(p2BINI.GetSectionValues("LevelSettings"));
            Type = typeof(TransferServiceBookController); 
        }
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/TransferServiceBook/Index.cshtml");
        }
        public ActionResult GetNewFuncStructDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fun_data = db.FuncStruct.Select(e => new
                {
                    code = e.Id.ToString(),
                    value = e.Job.Code + " - " + e.Job.Name.ToString() + "" + e.JobPosition.JobPositionDesc.ToString(),

                }).ToList();
                return Json(fun_data, JsonRequestBehavior.AllowGet);
            }

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

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
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
                List<int> Trans_Id = one_ids(data1);
                if (extraeditdata.Count() == 1 && Trans_Id.Count() == 1)
                {
                    int EmpId = extraeditdata.FirstOrDefault();
                    int TransId = Trans_Id.FirstOrDefault();
                    if (EmpId != 0 && TransId != 0)
                    {
                        EmpSalStruct Onewstruct = new EmpSalStruct();
                        var TransBook = db.TransferServiceBook.Find(TransId);
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

                                Onewstruct.GeoStruct = db.GeoStruct.Find(TransBook.NewGeoStruct_Id);
                                Onewstruct.PayStruct = db.PayStruct.Find(TransBook.NewPayStruct_Id);
                                Onewstruct.FuncStruct = db.FuncStruct.Find(TransBook.NewFuncStruct_Id);
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
        //        if (extraeditdata.Count()== 1)
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
             
        //        else 
        //        {
        //            //Msg = "Nonstandard heads are defined. So you can't select multiple employees.";
        //            Msg = "NonStandard heads one define in salary structure,Multiple employees selected for transfer,Any change the amount in salarystructure.";
        //        }

        //    }

        //    var result = new { Sal = model, msg = Msg };

        //    return Json(result, JsonRequestBehavior.AllowGet);

        //}


        public ActionResult GetNewPayStructDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var pay_data = db.PayStruct
                .Select(e => new
                {
                    code = e.Id.ToString(),
                    value = e.Grade.Code + " - " + e.Grade.Name.ToString() + " " + e.Level.Name.ToString()
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
                    value = e.FullDetails
                    //value = e.Corporate.Name + e.Region.Name + e.Company.FullDetails + "Division : " + e.Division.FullDetails + " "
                    //+ "Location : " + e.Location.FullDetails + " Department : " + e.Department.FullDetails

                }).ToList();

                return Json(geo_data, JsonRequestBehavior.AllowGet);

            }
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
                    .Include(e => e.TransActivity)
                    .Include(e => e.TransActivity.Select(a => a.TranPolicy))
                    .SingleOrDefault();//modified by prashant 15042017
                // .Select(r => r.PromotionServiceBook).FirstOrDefault();//modified by prashant 15042017
                var id = Convert.ToInt32(data);

                var qurey = OPromo.TransActivity.Where(e => e.TranPolicy != null && e.TranPolicy.Id == id).Select(e => e.TranPolicy).ToList();//modified by prashant 15042017
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDropDownPolicyList(string data, string data2)
        {
            //data2-empid
            //data-activityid
            using (DataBaseContext db = new DataBaseContext())
            {
                int EmpId = Convert.ToInt32(data2);// form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);//modified by prashant 15042017

                var OPayscale = db.Employee.Where(e => e.Id == EmpId)//modified by prashant 15042017
                    .Select(r => r.EmpOffInfo.PayScale).FirstOrDefault();//modified by prashant 15042017
                var OPromo = db.PayScaleAgreement.Where(e => e.EndDate == null && e.PayScale.Id == OPayscale.Id)
                    .Include(e => e.TransActivity)
                    .Include(e => e.TransActivity.Select(a => a.TranPolicy))
                    .SingleOrDefault();//modified by prashant 15042017
                // .Select(r => r.PromotionServiceBook).FirstOrDefault();//modified by prashant 15042017
                var activityid = Convert.ToInt32(data);
                var qurey = OPromo.TransActivity.Where(e => e.Id == activityid).Select(e => e.TranPolicy).ToList();//modified by prashant 15042017

                //var qurey = db.PromoActivity.ToList();//modified by prashant 15042017
                // var qurey = db.PromotionServiceBook.ToList();//modified by prashant 15042017
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDropDownActivityList(string data, string data2)//modified by prashant 15042017
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var qurey = db.IncrActivity.Include(e => e.IncrList).ToList();
                int OEmp_Id = Convert.ToInt32(data2);
                var query = db.EmployeePolicyStruct.AsNoTracking().Where(e => e.EmployeePayroll.Employee.Id == OEmp_Id && e.EndDate == null).Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.EmployeePolicyStructDetails)
                    .Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula)).Include(e => e.EmployeePolicyStructDetails.
                    Select(r => r.PolicyFormula.TransActivity)).FirstOrDefault();
                var TransActList = query.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.TransActivity);

                var selected = (Object)null;
                //if (data2 != "" && data != "0" && data2 != "0")
                //{
                //    selected = Convert.ToInt32(data2);
                //}

                List<TransActivity> TransAct = new List<TransActivity>();
                if (TransActList.Count() > 0)
                {
                    foreach (var item in TransActList)
                    {
                        if (item.FirstOrDefault() != null)
                        {
                            foreach (var item1 in item)
                            {
                                TransAct.Add(item1);
                            }
                        }

                    }
                }
                SelectList s = new SelectList(TransAct, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
            ////using (DataBaseContext db = new DataBaseContext())
            ////{
            ////    int EmpId = Convert.ToInt32(data);// form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);//modified by prashant 15042017

            ////    var OPayscale = db.Employee.Where(e => e.Id == EmpId)//modified by prashant 15042017
            ////        .Select(r => r.EmpOffInfo.PayScale).FirstOrDefault();//modified by prashant 15042017
            ////    var OPromo = db.PayScaleAgreement.Where(e => e.EndDate == null && e.PayScale.Id == OPayscale.Id)
            ////        .Include(e => e.TransActivity)
            ////        .SingleOrDefault();//modified by prashant 15042017
            ////    // .Select(r => r.PromotionServiceBook).FirstOrDefault();//modified by prashant 15042017
            ////    var qurey = OPromo.TransActivity.ToList();//modified by prashant 15042017

            ////    //var qurey = db.PromoActivity.ToList();//modified by prashant 15042017
            ////    // var qurey = db.PromotionServiceBook.ToList();//modified by prashant 15042017
            ////    var selected = (Object)null;
            ////    if (data2 != "" && data != "0" && data2 != "0")
            ////    {
            ////        selected = Convert.ToInt32(data2);
            ////    }

            ////    SelectList s = new SelectList(qurey, "Id", "Name", selected);
            ////    return Json(s, JsonRequestBehavior.AllowGet);
            ////}
        }


        #region Create
        public ActionResult Create(TransferServiceBook TransferServiceBook, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string NewGeoStruct = form["NewGeoT-table"] == "0" ? "" : form["NewGeoT-table"];
                    string NewFuncStruct = form["NewFuncT-table"] == "0" ? "" : form["NewFuncT-table"];
                    string NewPayStruct = form["NewPayT-table"] == "0" ? "" : form["NewPayT-table"];
                    string TransActivity = form["TransActivitylist"] == "0" ? "" : form["TransActivitylist"];
                    string TransPolicy = form["transpolicy"] == "0" ? "" : form["transpolicy"];
                    string ProcessTransDate = form["ProcessTransDate"] == "0" ? "" : form["ProcessTransDate"];

                    var date = Convert.ToDateTime(ProcessTransDate).ToString("MM/yyyy");

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
                        Msg.Add("Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    }

                    ////////new 21/08/2019
                    var check = db.CPIEntryT.Where(e => e.PayMonth == date).ToList();

                    if (check.Count() == 0)
                    {
                        Msg.Add("Kindly run CPI first and then try again");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    ///////////

                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;


                    OEmployee = db.Employee.Include(e=>e.EmpName).Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                    .Include(e => e.EmpOffInfo)
                                    .Where(r => r.Id == Emp).SingleOrDefault();

                    OEmployeePayroll = db.EmployeePayroll.Include(e => e.TransferServiceBook).Where(e => e.Employee.Id == Emp).SingleOrDefault();

                    if (OEmployeePayroll.TransferServiceBook.Any(d => d.ProcessTransDate.Value.ToShortDateString() == ProcessTransDate.ToString() && d.ReleaseFlag == false))
                    {
                        {
                            
                            Msg.Add("Please Release The Activity and Try Again:" + OEmployee.EmpCode + " " + OEmployee.EmpName.FullNameFML + " on date= " + ProcessTransDate);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    int TransActivityIds = 0;

                    if (TransActivity != null && TransActivity != "")
                    {
                        TransActivityIds = int.Parse(TransActivity);
                    }

                    if (OEmployeePayroll.TransferServiceBook.Any(a => a.ProcessTransDate.Value.ToShortDateString() == ProcessTransDate.ToString() && a.TransActivity_Id == TransActivityIds))
                    {
                        Msg.Add("Already transfer Policy for Date= " + ProcessTransDate);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    int TransActivityPolicyId = 0;
                    int TransActivityId = 0;
                    string LookupVal = "";
                    if (TransActivity != null && TransActivity != "")
                    {
                        TransActivityId = int.Parse(TransActivity);
                        LookupVal = db.LookupValue.Where(e => e.Id == TransActivityId).Select(e => e.LookupVal.ToUpper()).SingleOrDefault();
                        TransferServiceBook.TransActivity = db.TransActivity.Include(e => e.TranPolicy).Where(e => e.Id == TransActivityId).SingleOrDefault(); ;
                    }

                    if (TransPolicy != null && TransPolicy != "")
                    {
                        TransActivityPolicyId = int.Parse(TransPolicy);
                        //TransferServiceBook.TransActivity = db.TransActivity.Include(e => e.TranPolicy).Where(e => e.Id == TransActivityPolicyId).SingleOrDefault();
                    }

                    if (TransferServiceBook.TransActivity.TranPolicy.IsFuncStructChange == true)
                        if (NewFuncStruct != "" && NewFuncStruct != null)
                            TransferServiceBook.NewFuncStruct = db.FuncStruct.Find(int.Parse(NewFuncStruct));
                        else
                        // return Json(new Object[] { "", "", "Kindly select NewFuncStruct." }, JsonRequestBehavior.AllowGet);
                        {

                            Msg.Add("Kindly select NewFuncStruct.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    else
                        TransferServiceBook.NewFuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);


                    if (TransferServiceBook.TransActivity.TranPolicy.IsDepartmentChange == true || TransferServiceBook.TransActivity.TranPolicy.IsDivsionChange == true ||
                        TransferServiceBook.TransActivity.TranPolicy.IsGroupChange == true || TransferServiceBook.TransActivity.TranPolicy.IsLocationChange == true ||
                        TransferServiceBook.TransActivity.TranPolicy.IsUnitChange == true)
                        if (NewGeoStruct != "" && NewGeoStruct != null)
                            TransferServiceBook.NewGeoStruct = db.GeoStruct.Find(int.Parse(NewGeoStruct));
                        else
                        //return Json(new Object[] { "", "", "Kindly select NewGeoStruct." }, JsonRequestBehavior.AllowGet);
                        {
                            List<string> Msgn = new List<string>();
                            Msgn.Add("Kindly select NewGeoStruct.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msgn }, JsonRequestBehavior.AllowGet);
                        }
                    else
                        TransferServiceBook.NewGeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);

                    if (OEmployee.PayStruct != null)
                        TransferServiceBook.NewPayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);

                    if (OEmployee.GeoStruct != null)
                        TransferServiceBook.OldGeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);

                    if (OEmployee.FuncStruct != null)
                        TransferServiceBook.OldFuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);

                    if (OEmployee.PayStruct != null)
                        TransferServiceBook.OldPayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);

                    if (TransferServiceBook.OldGeoStruct == TransferServiceBook.NewGeoStruct)
                    {
                        Msg.Add("Can Not Tranfer To Same Location and Same Department");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                            //   List<string> Msg = new List<string>();
                            Msg.Add("Job Position Should be Same old and new function struct ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }

                    if (LevelSettings != null && LevelSettings.CCode == CompCode)
                    {
                        int payid = Convert.ToInt32(NewPayStruct);
                        var newlevel = db.PayStruct.Include(e => e.Level).Where(e => e.Company.Id == CompId && e.Id == payid).FirstOrDefault();
                        var oldlevel = db.PayStruct.Include(e => e.Level).Where(e => e.Company.Id == CompId && e.Id == TransferServiceBook.NewPayStruct.Id).FirstOrDefault();
                        if ((newlevel.Level == null ? "" : newlevel.Level.Code.ToUpper()) != (oldlevel.Level == null ? "" : oldlevel.Level.Code.ToUpper()))
                        {
                             
                            Msg.Add("Level Should be Same old and new pay struct ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                      new System.TimeSpan(0, 30, 0)))
                    {
                        TransferServiceBook.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        TransferServiceBook TrnsServBook = new TransferServiceBook()
                        {
                            EmployeeCTC = null,
                            OldGeoStruct = TransferServiceBook.OldGeoStruct,
                            OldFuncStruct = TransferServiceBook.OldFuncStruct,
                            OldPayStruct = TransferServiceBook.OldPayStruct,
                            NewGeoStruct = TransferServiceBook.NewGeoStruct,
                            NewFuncStruct = TransferServiceBook.NewFuncStruct,
                            NewPayStruct = TransferServiceBook.NewPayStruct,
                            ProcessTransDate = TransferServiceBook.ProcessTransDate,
                            TransActivity = TransferServiceBook.TransActivity,
                            Narration = TransferServiceBook.Narration,
                            DBTrack = TransferServiceBook.DBTrack
                        };


                        db.TransferServiceBook.Add(TrnsServBook);
                        db.SaveChanges();


                        List<TransferServiceBook> TransferBkList = new List<TransferServiceBook>();
                        TransferBkList.AddRange(OEmployeePayroll.TransferServiceBook);
                        TransferBkList.Add(TrnsServBook);
                        OEmployeePayroll.TransferServiceBook = TransferBkList;
                        db.EmployeePayroll.Attach(OEmployeePayroll);
                        db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;

                        //  DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
                        Process.ServiceBook.ServiceBookProcess("",CompId, "TRANSFER_PROCESS", null, null, TrnsServBook.Id, null, OEmployeePayroll.Id, "TRANSFER", TrnsServBook.ProcessTransDate, false, false, 0, null);

                        // db.RefreshAllEntites(RefreshMode.StoreWins);
                        List<string> Msgs = new List<string>();
                        Msgs.Add("Data Saved successfully");

                        var InchargeChk = db.GeoStruct.Include(e => e.Location).Include(e => e.Location.Incharge).Include(e => e.Location.LocationObj)
                            .Where(e => e.Id == TransferServiceBook.OldGeoStruct.Id && e.Location.Incharge.Id == OEmployee.Id).SingleOrDefault();
                        string location = "";
                        if (InchargeChk != null)
                        {
                            Msgs.Add("Transfer Employee ,he/she is In charge of " + location + " location,Please change In charge in location page.");
                        }
                        var reporting_chk = db.Employee.Include(e => e.ReportingStructRights).Where(e => e.Id == OEmployee.Id).SingleOrDefault();
                        if (reporting_chk.ReportingStructRights.Count() > 0)
                        {
                            Msgs.Add("Reporting is also define for employee,Please change ");
                        }
                        var EmpReportingTimingStruct = db.EmployeeAttendance.Where(e => e.Employee.Id == OEmployee.Id).Include(e => e.EmpReportingTimingStruct).SingleOrDefault();
                        if (EmpReportingTimingStruct != null && EmpReportingTimingStruct.EmpReportingTimingStruct.Count > 0)
                        {
                            Msgs.Add("Attendance Reporting is also define for employee,Please change ");
                        }
                        if (CompCode == "JSBL")
                        {
                            Employee OEmployeeAdd = null;
                            OEmployeeAdd = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                      .Include(e => e.EmpOffInfo)
                                      .Include(e => e.PerAddr)
                                      .Include(e => e.PerAddr.City)
                                      .Where(r => r.Id == Emp).SingleOrDefault();
                            var Pcity = OEmployeeAdd.PerAddr != null && OEmployeeAdd.PerAddr.City != null ? OEmployeeAdd.PerAddr.City.Name.ToString() : null;


                            int Jeoid = Convert.ToInt32(NewGeoStruct);
                            var newCity = db.GeoStruct.Include(e => e.Location)
                                .Include(e => e.Location.Address)
                                .Include(e => e.Location.Address.City)
                                .Where(e => e.Company.Id == CompId && e.Id == Jeoid).SingleOrDefault();
                            var Transfercity = newCity.Location.Address != null && newCity.Location.Address.City != null ? newCity.Location.Address.City.Name.ToString() : null;

                            if (Pcity == null || Transfercity == null || (Pcity.ToUpper() != Transfercity.ToUpper()))
                            {
                                Msgs.Add("Please Confirm Discomfort Allowance For This Employee ");
                            }

                        }

                        ts.Complete();

                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

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

                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "", Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion


        #region Release
        public ActionResult Release(TransferServiceBook TransferServiceBook, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            List<string> msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //  var Emp = form["emp_Id"] == "0" ? "" : form["emp_Id"];
                    var ReleaseFlag = form["ReleaseFlag"] == null ? false : Convert.ToBoolean(form["ReleaseFlag"]);
                    var ReleaseDatestr = form["ReleaseDate"] == null ? null : form["ReleaseDate"];
                    var txtNarrationRelease = form["txtNarrationRelease"] == null ? null : form["txtNarrationRelease"];
                    var SalHeadCount = form["SalHeadCount"] == "0" ? "0" : form["SalHeadCount"];
                    var ids = form["input_hidden_field"] != "" ? one_ids(form["input_hidden_field"]) : null;
                    var ReleaseDateStr = form["ReleaseDate"] == "0" ? "" : form["ReleaseDate"];
                    DateTime? ReleaseDate = !string.IsNullOrEmpty(ReleaseDateStr) ? (DateTime?)Convert.ToDateTime(ReleaseDateStr) : null; 


                    int CompId = 1;
                    if (!String.IsNullOrEmpty(SessionManager.UserName))
                    {
                        //CompId = int.Parse(SessionManager.UserName.ToString());
                        CompId = Convert.ToInt32(Session["CompId"]);
                    }

                    //int Empid = 0;
                    //if (Emp != null)
                    //{
                    //    Empid = int.Parse(Emp);
                    //}
                    List<int> Empids = null;
                    //if (Emp != null && Emp != "0" && Emp != "false")
                    //{
                    //    Empids = Utility.StringIdsToListIds(Emp);
                    //}
                    if (forwarddata != null && forwarddata != "0" && forwarddata != "false")
                    {
                        Empids = Utility.StringIdsToListIds(forwarddata);
                    }
                    foreach (var Empid in Empids)
                    {
                        Employee OEmployeedate = null;
                        OEmployeedate = db.Employee
                                        .Where(r => r.Id == Empid).SingleOrDefault();

                        Employee OEmployee = null;
                        EmployeePayroll OEmployeePayroll = null;
                        //int PayScaleAgrId = int.Parse(PayScaleAgr);
                        //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();

                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                        .Include(e => e.EmpOffInfo)
                                        .Where(r => r.Id == Empid).SingleOrDefault();

                        OEmployeePayroll
                   = db.EmployeePayroll.Include(e => e.TransferServiceBook)
                      .Where(e => e.Employee.Id == Empid).SingleOrDefault();
                        //  int TransId = int.Parse(forwarddata);
                        //List<int> ids = null;
                        //if (forwarddata != null && forwarddata != "0" && forwarddata != "false")
                        //{
                        //    ids = Utility.StringIdsToListIds(forwarddata);
                        //}

                        //foreach (int item in ids)
                        //{
                        int TransId = OEmployeePayroll.TransferServiceBook.Where(e => e.ReleaseFlag == false).SingleOrDefault().Id;

                        TransferServiceBook.ReleaseFlag = Convert.ToBoolean(ReleaseFlag);
                        TransferServiceBook.Narration = txtNarrationRelease;
                        //TransferServiceBook.ReleaseDate = Convert.ToDateTime(ReleaseDatestr);
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                          new System.TimeSpan(0, 30, 0)))
                        {


                            if (ReleaseFlag != false)
                            {
                               

                                TransferServiceBook OTransferServiceBook = db.TransferServiceBook.Where(e => e.Id == TransId).SingleOrDefault();

                                if (ReleaseDate.Value.Date < OTransferServiceBook.ProcessTransDate.Value.Date)
                                {
                                    Msg.Add(" The " + OEmployeedate.EmpCode + " has Release date should be greater than the process date.");
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                Process.ServiceBook.ServiceBookProcess("",CompId, "TRANSFER_RELEASE", null, null, OTransferServiceBook.Id, null, OEmployeePayroll.Id, "TRANSFER", TransferServiceBook.ReleaseDate, false, false, 0, null);

                            

                                if (ids != null && ids.Count() > 0)
                                {
                                    List<EmpSalStruct> OEmpSalStruct = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead)).Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula))
                                .Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id && e.EffectiveDate >= OTransferServiceBook.ProcessTransDate).ToList();
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
                            // DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);



                            // return Json(new { success = true, responseText = "Data Updated Successfully." }, JsonRequestBehavior.AllowGet);

                        }
                        //13/07/2023
                        var reporting_chk = db.Employee.Include(e => e.ReportingStructRights).Where(e => e.Id == OEmployee.Id).SingleOrDefault();
                        if (reporting_chk.ReportingStructRights.Count() > 0)
                        {
                            Msg.Add("  Data Updated successfully,<br /> Click here to <a style='color:blue' href='Employee?param=" + OEmployeePayroll.Id + "'target='_blank'>Reporting Structure</a>,");
                            // +"<br /> Click here to <a style='color:blue' href='EmpReportingTimingStruct/PartialPopUp?param=" + 1 + "'target='_blank'>Assign Authority</a>");
                            //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        //13/07/2023
                        var EmpReportingTimingStruct = db.EmployeeAttendance.Where(e => e.Employee.Id == OEmployee.Id).Include(e => e.EmpReportingTimingStruct).SingleOrDefault();
                        if (EmpReportingTimingStruct != null && EmpReportingTimingStruct.EmpReportingTimingStruct.Count > 0)
                        {
                            // Msg.Add("  Data Updated successfully,<br /> Click here to <a style='color:blue' href='ReportingStruct/PartialPopUp?param=" + 1 + "'target='_blank'>Reporting Structure</a>," +
                            Msg.Add("<br /> Click here to <a style='color:blue' href='EmpReportingTimingStruct/PartialPopUp?param=" + OEmployeePayroll.Id + "'target='_blank'>Reporting Timing Structure</a>");
                            //  return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        if (Msg.Count()==0)
                        {
                             Msg.Add("  Data Updated successfully");// if ReportingStructRights and EmpReportingTimingStruct not define
                        }
                        //13/07/2023
                        // }
                    }

                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                msg.Add("Data Updated successfully");
                return Json(new Utility.JsonReturnClass { success = true, responseText = msg }, JsonRequestBehavior.AllowGet);

            }
        }
        #endregion


        public JsonResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.TransferServiceBook
                    .Include(e => e.TransActivity.TransList).Include(e => e.OldPayStruct.JobStatus)
                                .Include(e => e.NewPayStruct).Include(e => e.NewPayStruct.Level).Include(e => e.NewPayStruct.Grade)
                                .Include(e => e.NewPayStruct.JobStatus)
                                .Include(e => e.OldFuncStruct).Include(e => e.OldFuncStruct.Job).Include(e => e.OldFuncStruct.JobPosition)
                                .Include(e => e.NewFuncStruct).Include(e => e.NewFuncStruct.Job).Include(e => e.NewFuncStruct.JobPosition)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        TransActivity = e.TransActivity.TransList.LookupVal,
                        ProcessTransDate = e.ProcessTransDate,
                        OldGeoStruct_FullDetails = e.OldPayStruct == null ? null : e.OldPayStruct.FullDetails,
                        OldGeoStruct_Id = e.OldPayStruct.Id == null ? "" : e.OldPayStruct.Id.ToString(),
                        NewGeoStruct_FullDetails = e.NewPayStruct == null ? null : e.NewPayStruct.FullDetails,
                        NewGeoStruct_Id = e.NewPayStruct.Id == null ? "" : e.NewPayStruct.Id.ToString(),
                        OldPayStruct_FullDetails = e.OldPayStruct == null ? null : e.OldPayStruct.FullDetails,
                        OldPayStruct_Id = e.OldPayStruct.Id == null ? "" : e.OldPayStruct.Id.ToString(),
                        NewPayStruct_FullDetails = e.NewPayStruct == null ? null : e.NewPayStruct.FullDetails,
                        NewPayStruct_Id = e.NewPayStruct.Id == null ? "" : e.NewPayStruct.Id.ToString(),
                        OldFuncStruct_FullDetails = e.OldFuncStruct == null ? null : e.OldFuncStruct.FullDetails,
                        OldFuncStruct_Id = e.OldFuncStruct.Id == null ? "" : e.OldFuncStruct.Id.ToString(),
                        NewFuncStruct_FullDetails = e.NewFuncStruct == null ? null : e.NewFuncStruct.FullDetails,
                        NewFuncStruct_Id = e.NewFuncStruct.Id == null ? "" : e.NewFuncStruct.Id.ToString(),
                        Narration = e.Narration
                    }).ToList();

                var TransServBook = db.TransferServiceBook.Find(data);
                Session["RowVersion"] = TransServBook.RowVersion;
                var Auth = TransServBook.DBTrack.IsModified;
                return Json(new Object[] { Q, Auth }, JsonRequestBehavior.AllowGet);
            }
        }

        public class YearlypaymentGridData
        {
            public int Id { get; set; }
            public Employee Employee { get; set; }
            public SalaryHead SalaryHead { get; set; }
            public string FromPeriod { get; set; }
            public string ToPeriod { get; set; }
            public string ProcessMonth { get; set; }
            public double AmountPaid { get; set; }
            public double TDSAmount { get; set; }
            public string Narration { get; set; }
            public double OtherDeduction { get; set; }

        }

        public class TransServBookGridData
        {
            public int Id { get; set; }
            public int EmpId { get; set; }
            public Employee Employee { get; set; }
            public TransActivity TransActivity { get; set; }
            public string ActivityDate { get; set; }
            public string OldGeoStruct { get; set; }
            public string NewGeoStruct { get; set; }
            public int TransId { get; set; }

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

        public class TransChildDataClass
        {
            public int Id { get; set; }
            public bool Release { get; set; }
            public string ReleaseDate { get; set; }
            public string Activity { get; set; }
            public string TransferDate { get; set; }
            public string OldGeoStruct { get; set; }
            public string NewGeoStruct { get; set; }
        }

        public ActionResult P2BGridRelease(P2BGrid_Parameters gp)
        {
            List<string> Msg = new List<string>();

            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<TransServBookGridData> TransServBook = null;
                List<TransServBookGridData> model = new List<TransServBookGridData>();
                TransServBookGridData view = null;

                var OEmployee = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).ToList();

                foreach (var z in OEmployee)
                {
                    var ObjTransServBook = db.EmployeePayroll.Where(e => e.Id == z.Id)
                        .Select(e => e.TransferServiceBook.Where(r => r.ReleaseFlag == false))
                                        .SingleOrDefault();


                    //DateTime? Eff_Date = null;
                    //PayScaleAgreement PayScaleAgr = null;
                    foreach (var a in ObjTransServBook)
                    {
                        //Eff_Date = Convert.ToDateTime(a.ProcessMonth);
                        var aa = db.TransferServiceBook.Include(e => e.OldPayStruct).Include(e => e.OldPayStruct.Level).Include(e => e.OldPayStruct.Grade)
                            .Include(e => e.OldPayStruct.JobStatus)
                            .Include(e => e.NewPayStruct).Include(e => e.NewPayStruct.Level).Include(e => e.NewPayStruct.Grade)
                            .Include(e => e.NewPayStruct.JobStatus)
                            .Include(e => e.OldFuncStruct).Include(e => e.OldFuncStruct.Job).Include(e => e.OldFuncStruct.JobPosition)
                            .Include(e => e.NewFuncStruct).Include(e => e.NewFuncStruct.Job).Include(e => e.NewFuncStruct.JobPosition)
                            .Include(e => e.OldGeoStruct.Company).Include(e => e.OldGeoStruct.Corporate).Include(e => e.OldGeoStruct.Department)
                            .Include(e => e.OldGeoStruct.Division).Include(e => e.OldGeoStruct.Group).Include(e => e.OldGeoStruct.Location).Include(e => e.OldGeoStruct.Location.LocationObj)
                            .Include(e => e.OldGeoStruct.Region).Include(e => e.OldGeoStruct.Unit)
                            .Include(e => e.NewGeoStruct.Company).Include(e => e.NewGeoStruct.Corporate).Include(e => e.NewGeoStruct.Department)
                            .Include(e => e.NewGeoStruct.Division).Include(e => e.NewGeoStruct.Group).Include(e => e.NewGeoStruct.Location).Include(e => e.NewGeoStruct.Location.LocationObj)
                            .Include(e => e.NewGeoStruct.Region).Include(e => e.NewGeoStruct.Unit)
                            .Include(e => e.TransActivity).Include(e => e.TransActivity.TransList)
                            .Where(e => e.Id == a.Id).SingleOrDefault();
                        view = new TransServBookGridData()
                        {
                            Id = z.Employee.Id,
                            Employee = z.Employee,
                            TransActivity = aa.TransActivity,
                            ActivityDate = aa.ProcessTransDate.Value.ToString(),
                            OldGeoStruct = aa.OldGeoStruct.Location != null && aa.OldGeoStruct.Location.LocationObj.LocDesc != null ? aa.OldGeoStruct.Location.LocationObj.LocDesc.ToString() : "",
                            NewGeoStruct = aa.NewGeoStruct.Location.LocationObj.LocDesc != null ? aa.NewGeoStruct.Location.LocationObj.LocDesc.ToString() : "",
                            TransId = a.Id
                        };

                        model.Add(view);
                    }

                }

                TransServBook = model;

                IEnumerable<TransServBookGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = TransServBook;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Employee.EmpCode.ToString().Contains(gp.searchString))
                            || (e.Employee.EmpName.FullNameFML.ToString().Contains(gp.searchString))
                            || (e.TransActivity.TransList.LookupVal.ToString().Contains(gp.searchString))
                            || (e.ActivityDate.ToString().Contains(gp.searchString))
                            || (e.OldGeoStruct.ToString().Contains(gp.searchString))
                            || (e.NewGeoStruct.ToString().Contains(gp.searchString))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.TransActivity.TransList.LookupVal, a.ActivityDate, a.OldGeoStruct, a.NewGeoStruct, a.TransId, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.TransActivity.TransList.LookupVal, a.ActivityDate, a.OldGeoStruct, a.NewGeoStruct, a.TransId, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = TransServBook;
                    Func<TransServBookGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "Activity" ? c.TransActivity.TransList.LookupVal :
                                         gp.sidx == "Process Date" ? c.ActivityDate.ToString() :
                                         gp.sidx == "Old Location" ? c.OldGeoStruct.ToString() :
                                         gp.sidx == "New Location" ? c.NewGeoStruct : ""
                                          );
                    }
                    if (gp.sord == "asc")  //Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : ""
                    {
                        IE = IE.OrderBy(orderfuc);  // a.Id,a.Employeee.EmpCode,a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.TransActivity.TransList != null ? a.TransActivity.TransList.LookupVal : "", a.ActivityDate, a.OldGeoStruct, a.NewGeoStruct, a.TransId, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.TransActivity.TransList != null ? a.TransActivity.TransList.LookupVal : "", a.ActivityDate, a.OldGeoStruct, a.NewGeoStruct, a.TransId, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.TransActivity.TransList != null ? a.TransActivity.TransList.LookupVal : "", a.ActivityDate, a.OldGeoStruct, a.NewGeoStruct, a.TransId, a.Id }).ToList();
                    }
                    totalRecords = TransServBook.Count();
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

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                        // .Include(e => e.TransferServiceBook)
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
                        foreach (var item in all)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Employee.EmpCode,
                                Name = item.Employee.EmpName.FullNameFML,
                                JoiningDate = item.Employee.ServiceBookDates.JoiningDate.Value.ToString(),
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

        public ActionResult Get_TransServBook(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.TransferServiceBook.Select(r => r.TransActivity))
                        .Include(e => e.TransferServiceBook.Select(r => r.TransActivity.TransList))
                        .Include(e => e.TransferServiceBook.Select(r => r.OldGeoStruct))
                        //.Include(e => e.TransferServiceBook.Select(r => r.OldGeoStruct.Company))
                        //.Include(e => e.TransferServiceBook.Select(r => r.OldGeoStruct.Corporate))
                        //.Include(e => e.TransferServiceBook.Select(r => r.OldGeoStruct.Department))
                        //.Include(e => e.TransferServiceBook.Select(r => r.OldGeoStruct.Division))
                        //.Include(e => e.TransferServiceBook.Select(r => r.OldGeoStruct.Group))
                         .Include(e => e.TransferServiceBook.Select(r => r.OldGeoStruct.Location))
                         .Include(e => e.TransferServiceBook.Select(r => r.OldGeoStruct.Location.LocationObj))
                         .Include(e=>e.TransferServiceBook.Select(r=>r.OldGeoStruct.Department))
                         .Include(e=>e.TransferServiceBook.Select(r=>r.OldGeoStruct.Department.DepartmentObj))
                        //.Include(e => e.TransferServiceBook.Select(r => r.OldGeoStruct.Region))
                        //.Include(e => e.TransferServiceBook.Select(r => r.OldGeoStruct.Unit))
                        //.Include(e => e.TransferServiceBook.Select(r => r.NewGeoStruct))
                        //.Include(e => e.TransferServiceBook.Select(r => r.NewGeoStruct.Company))
                        //.Include(e => e.TransferServiceBook.Select(r => r.NewGeoStruct.Corporate))
                        //.Include(e => e.TransferServiceBook.Select(r => r.NewGeoStruct.Department))
                        //.Include(e => e.TransferServiceBook.Select(r => r.NewGeoStruct.Division))
                        //.Include(e => e.TransferServiceBook.Select(r => r.NewGeoStruct.Group))
                         .Include(e => e.TransferServiceBook.Select(r => r.NewGeoStruct.Location))
                         .Include(e => e.TransferServiceBook.Select(r => r.NewGeoStruct.Location.LocationObj))
                         .Include(e=>e.TransferServiceBook.Select(r=>r.NewGeoStruct.Department))
                         .Include(e=>e.TransferServiceBook.Select(r=>r.NewGeoStruct.Department.DepartmentObj))
                        //.Include(e => e.TransferServiceBook.Select(r => r.NewGeoStruct.Region))
                        //.Include(e => e.TransferServiceBook.Select(r => r.NewGeoStruct.Unit))
                        .Where(e => e.Id == data).SingleOrDefault();

                    var db_data1 = db.EmployeePayroll
                       .Include(e => e.TransferServiceBook.Select(r => r.TransActivity))
                       .Include(e => e.TransferServiceBook.Select(r => r.TransActivity.TranPolicy))
                       .Where(e => e.Id == data).SingleOrDefault();

                    if (db_data1 != null)
                    {
                        List<TransChildDataClass> returndata = new List<TransChildDataClass>();
                        foreach (var item in db_data1.TransferServiceBook)
                        {
                            returndata.Add(new TransChildDataClass
                            {
                                Id = item.Id,
                                Release = item.ReleaseFlag,
                                ReleaseDate = item.ReleaseDate != null ? item.ReleaseDate.Value.ToShortDateString() : "",
                                Activity = item.TransActivity != null ? item.TransActivity.TransList.LookupVal : "",
                                TransferDate = item.ProcessTransDate != null ? item.ProcessTransDate.Value.ToShortDateString() : "",
                                OldGeoStruct = (item.OldGeoStruct.Location != null && item.OldGeoStruct.Location.LocationObj.LocDesc != null ? item.OldGeoStruct.Location.LocationObj.LocDesc.ToString(): " " ) + (item.OldGeoStruct.Department!=null? "/": " ") + (item.OldGeoStruct!=null && item.OldGeoStruct.Department!=null && item.OldGeoStruct.Department.DepartmentObj!=null ? item.OldGeoStruct.Department.DepartmentObj.DeptDesc : ""),
                                NewGeoStruct = (item.NewGeoStruct.Location != null && item.NewGeoStruct.Location.LocationObj.LocDesc != null ? item.NewGeoStruct.Location.LocationObj.LocDesc.ToString() : " ") + (item.NewGeoStruct.Department != null ? "/" : " ") + (item.NewGeoStruct != null && item.NewGeoStruct.Department != null && item.NewGeoStruct.Department.DepartmentObj != null ? item.NewGeoStruct.Department.DepartmentObj.DeptDesc : ""),
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
                    throw e;
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
                    TransferServiceBook TransServBook = db.TransferServiceBook.Where(e => e.Id == data).SingleOrDefault();

                    if (TransServBook.ReleaseFlag == true)
                    {
                        return this.Json(new { status = true, valid = true, responseText = "You cannot delete as activity is already released.", JsonRequestBehavior.AllowGet });
                    }

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            db.TransferServiceBook.Attach(TransServBook);
                            db.Entry(TransServBook).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            db.Entry(TransServBook).State = System.Data.Entity.EntityState.Detached;
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