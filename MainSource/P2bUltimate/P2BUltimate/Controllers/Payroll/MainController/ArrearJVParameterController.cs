using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class ArrearJVParameterController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ArrearJVParameter/Index.cshtml");
        }
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        public ActionResult GetLocation()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.Location.Include(a => a.LocationObj).ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.FullDetails
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetBranch()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.Branch.ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.FullDetails
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetEmployee()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.EmployeePayroll
                    .Include(a => a.Employee)
                    .Include(a => a.Employee.EmpName)
                    .Include(a => a.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.ServiceLastDate == null).ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.Employee.FullDetails
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookupSalaryHead(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.SalaryHead.ToList();
                IEnumerable<SalaryHead> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.SalaryHead.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDropDownListLocation(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Location.Include(e => e.LocationObj).ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetFuncStruct()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.JobPosition.ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.JobPositionCode + " - " + s.JobPositionDesc
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetPayStruct()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.Grade.ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.FullDetails
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Get_job(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var query = db.Job.Include(e => e.JobPosition).ToList();
                foreach (var ca in query)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = ca.Id.ToString(),
                        value = ca.Name.ToString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Get_jobPosition(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                //if (data.Count != 0 && data[0] != 0)
                //{
                //    foreach (var s in data)
                //    {
                //  int id = Convert.ToInt32(s);
                // var query = db.Job.Include(e => e.JobPosition).Where(e => e.Id == id).ToList();
                var query = db.JobPosition.ToList();
                foreach (var ca in query)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = ca.Id.ToString(),
                        value = "Code :" + ca.JobPositionCode.ToString() + ", Position :" + ca.JobPositionDesc.ToString()
                    });
                }
                //}
                //}
                //else
                //{
                //    var reference_data = db.Job.Include(e => e.JobPosition).SelectMany(e => e.JobPosition).ToList();
                //    var db_data = db.JobPosition.ToList();
                //    var expact_list = db_data.Except(reference_data);
                //    foreach (var s in expact_list)
                //    {
                //        returndata.Add(new Utility.returndataclass
                //        {
                //            code = s.Id.ToString(),
                //            value = s.JobPositionDesc.ToString()
                //        });
                //    }
                //}
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Create(ArrJVParameter jv, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string LocationIn_table = form["LocationIn-table"] != null ? form["LocationIn-table"] : null;
                    string JVIdForNonStd = form["JVIdForNonStd"] != null ? form["JVIdForNonStd"] : null;
                  //  string JVLocIdForNonStd = form["JVLocIdForNonStd"];
                    string LocationOut_table = form["LocationOut-table"] != null ? form["LocationOut-table"] : null;
                    bool Irregular = form["Irregular"] != null ? Convert.ToBoolean(form["Irregular"]) : false;
                    Int32 Struct_Sel = Convert.ToInt32(form["Group"]);
                    string job_table = form["job-table"] != null ? form["job-table"] : null;
                    string jobposition_table = form["jobposition-table"] != null ? form["jobposition-table"] : null;
                    List<int> LocationIn = new List<int>();
                    Int32 LocationOut = 0;
                    Int32 Job = 0;
                    Int32 JobPosition = 0;
                    if (LocationIn_table != null)
                    {
                        LocationIn = Utility.StringIdsToListIds(LocationIn_table);
                    }
                    //else if (JVLocIdForNonStd != null)
                    //{
                    //    LocationIn = Utility.StringIdsToListIds(JVLocIdForNonStd);
                    //}

                    if (LocationOut_table != null)
                    {
                        var temp = Utility.StringIdsToListIds(LocationOut_table);
                        LocationOut = temp[0];
                    }
                    //else if (JVLocIdForNonStd != null)
                    //{
                    //    LocationOut = Convert.ToInt32(JVLocIdForNonStd);
                    //}

                    if (job_table != null)
                    {
                        var temp = Utility.StringIdsToListIds(job_table);
                        Job = temp[0];
                    }
                    if (jobposition_table != null)
                    {
                        var temp = Utility.StringIdsToListIds(jobposition_table);
                        List<string> Msg = new List<string>();
                        if (temp.Count() > 1)
                        {
                            Msg.Add("Select only one record");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        else if (temp.Count == 0)
                        {
                            Msg.Add("Select at least one record");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        JobPosition = temp[0];
                    }



                    string CreditDebitFlag = form["ArrCreditDebitFlag"] != null ? form["ArrCreditDebitFlag"] : "";
                    jv.ArrCreditDebitFlag = CreditDebitFlag;
                    string salaryheadslist = form["salaryheadslist"] != null ? form["salaryheadslist"] : "";
                    string GroupList_DDL = form["GroupList_DDL"] != null ? form["GroupList_DDL"] : "";
                    string Branch_drop = form["Branch_drop"] != null ? form["Branch_drop"] : "";
                    if (Branch_drop != "-Select-")
                    {
                        jv.ArrCreditDebitBranchName = Branch_drop;

                    }
                    if (GroupList_DDL != null && GroupList_DDL != "-Select-")
                    {
                        var value = db.LookupValue.Find(int.Parse(GroupList_DDL));
                        jv.JVGroup = value;

                    }
                    List<SalaryHead> objsalaryhead = new List<SalaryHead>();
                    List<ArrJVNonStandardEmp> objJVNonStandardEmp = new List<ArrJVNonStandardEmp>();

                    if (salaryheadslist == "")
                    {

                        List<string> Msg = new List<string>();
                        Msg.Add("Select SalaryHead ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    if (salaryheadslist != null && salaryheadslist != "")
                    {
                        var ids = one_ids(salaryheadslist);
                        foreach (var ca in ids)
                        {
                            var value = db.SalaryHead.Find(ca);
                            objsalaryhead.Add(value);
                            jv.ArrSalaryHead = objsalaryhead;
                        }
                    }

                    if (JVIdForNonStd != null && JVIdForNonStd != "")
                    {
                        var ids = one_ids(JVIdForNonStd);
                        foreach (var ca in ids)
                        {
                            var value = db.ArrJVNonStandardEmp.Find(ca);
                            objJVNonStandardEmp.Add(value);
                            jv.ArrJVNonStandardEmp = objJVNonStandardEmp;

                        }
                    }
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var companypayroll = new CompanyPayroll();
                    companypayroll = db.CompanyPayroll.Include(a => a.ArrJVParameter).Where(e => e.Company.Id == company_Id).SingleOrDefault();

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (Irregular == true)
                            {
                                if (LocationIn.Count > 0)
                                {
                                    foreach (var item in LocationIn)
                                    {

                                        jv.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        ArrJVParameter jvparam = new ArrJVParameter()
                                        {
                                            ArrAccountNo = jv.ArrAccountNo,
                                            ArrCreditDebitBranchCode = jv.ArrCreditDebitBranchCode,
                                            ArrCreditDebitBranchName = jv.ArrCreditDebitBranchName,
                                            ArrCreditDebitFlag = jv.ArrCreditDebitFlag,
                                            JVGroup = jv.JVGroup,
                                            ArrJVName = jv.ArrJVName,
                                            ArrJVProductCode = jv.ArrJVProductCode,
                                            ArrSalaryHead = jv.ArrSalaryHead,
                                            ArrJVNonStandardEmp = jv.ArrJVNonStandardEmp,
                                            ArrSubAccountNo = jv.ArrSubAccountNo,
                                            DBTrack = jv.DBTrack,
                                            ArrLocationIn = db.Location.Include(a => a.LocationObj).Where(a => a.Id == item).SingleOrDefault().LocationObj.LocCode,
                                            ArrLocationOut = db.Location.Include(a => a.LocationObj).Where(a => a.Id == LocationOut).SingleOrDefault().LocationObj.LocCode,
                                            ArrIrregular = true,
                                        };

                                        db.ArrJVParameter.Add(jvparam);
                                        db.SaveChanges();
                                        var Objjvparam = new List<ArrJVParameter>();
                                        Objjvparam.Add(jvparam);
                                        if (companypayroll != null)
                                        {
                                            if (companypayroll.ArrJVParameter.Count > 0)
                                            {

                                                companypayroll.ArrJVParameter.Add(jvparam);
                                            }
                                            else
                                            {
                                                companypayroll.ArrJVParameter = Objjvparam;

                                            }

                                        }
                                    }
                                }
                                else
                                {
                                    jv.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    ArrJVParameter jvparam = new ArrJVParameter()
                                    {
                                        //AccountNo = jv.AccountNo,
                                        //CreditDebitBranchCode = jv.CreditDebitBranchCode,
                                        //CreditDebitBranchName = jv.CreditDebitBranchName,
                                        //CreditDebitFlag = jv.CreditDebitFlag,
                                        //JVGroup = jv.JVGroup,
                                        //JVName = jv.JVName,
                                        //JVProductCode = jv.JVProductCode,
                                        //SalaryHead = jv.SalaryHead,
                                        //JVNonStandardEmp = jv.JVNonStandardEmp,
                                        //SubAccountNo = jv.SubAccountNo,
                                        //DBTrack = jv.DBTrack,
                                        //Irregular = true,
                                        ArrAccountNo = jv.ArrAccountNo,
                                        ArrCreditDebitBranchCode = jv.ArrCreditDebitBranchCode,
                                        ArrCreditDebitBranchName = jv.ArrCreditDebitBranchName,
                                        ArrCreditDebitFlag = jv.ArrCreditDebitFlag,
                                        JVGroup = jv.JVGroup,
                                        ArrJVName = jv.ArrJVName,
                                        ArrJVProductCode = jv.ArrJVProductCode,
                                        ArrSalaryHead = jv.ArrSalaryHead,
                                        ArrJVNonStandardEmp = jv.ArrJVNonStandardEmp,
                                        ArrSubAccountNo = jv.ArrSubAccountNo,
                                        DBTrack = jv.DBTrack,
                                        ArrIrregular = true,
                                    };

                                    db.ArrJVParameter.Add(jvparam);
                                    db.SaveChanges();
                                    var Objjvparam = new List<ArrJVParameter>();
                                    Objjvparam.Add(jvparam);
                                    if (companypayroll != null)
                                    {
                                        if (companypayroll.ArrJVParameter.Count > 0)
                                        {

                                            companypayroll.ArrJVParameter.Add(jvparam);
                                        }
                                        else
                                        {
                                            companypayroll.ArrJVParameter = Objjvparam;

                                        }

                                    }
                                }
                            }
                            else if (Struct_Sel != 0)
                            {
                                if (JobPosition != 0)
                                {
                                    Int32 FuncList = db.JobPosition.Where(e => e.Id == JobPosition).SingleOrDefault().Id;
                                    if (FuncList != null)
                                    {

                                        jv.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        ArrJVParameter jvparam = new ArrJVParameter()
                                        {
                                            //AccountNo = jv.AccountNo,
                                            //CreditDebitBranchCode = jv.CreditDebitBranchCode,
                                            //CreditDebitBranchName = jv.CreditDebitBranchName,
                                            //CreditDebitFlag = jv.CreditDebitFlag,
                                            //JVGroup = jv.JVGroup,
                                            //JVName = jv.JVName,
                                            //JVProductCode = jv.JVProductCode,
                                            //SalaryHead = jv.SalaryHead,
                                            //JVNonStandardEmp = jv.JVNonStandardEmp,
                                            //SubAccountNo = jv.SubAccountNo,
                                            //DBTrack = jv.DBTrack,
                                            //Irregular = false,
                                            //JobPosition = db.JobPosition.Find(FuncList)
                                            ArrAccountNo = jv.ArrAccountNo,
                                            ArrCreditDebitBranchCode = jv.ArrCreditDebitBranchCode,
                                            ArrCreditDebitBranchName = jv.ArrCreditDebitBranchName,
                                            ArrCreditDebitFlag = jv.ArrCreditDebitFlag,
                                            JVGroup = jv.JVGroup,
                                            ArrJVName = jv.ArrJVName,
                                            ArrJVProductCode = jv.ArrJVProductCode,
                                            ArrSalaryHead = jv.ArrSalaryHead,
                                            ArrJVNonStandardEmp = jv.ArrJVNonStandardEmp,
                                            ArrSubAccountNo = jv.ArrSubAccountNo,
                                            DBTrack = jv.DBTrack,
                                            ArrIrregular = false,
                                            ArrJobPosition = db.JobPosition.Find(FuncList)
                                        };

                                        db.ArrJVParameter.Add(jvparam);
                                        db.SaveChanges();
                                        var Objjvparam = new List<ArrJVParameter>();
                                        Objjvparam.Add(jvparam);
                                        if (companypayroll != null)
                                        {
                                            if (companypayroll.ArrJVParameter.Count > 0)
                                            {

                                                companypayroll.ArrJVParameter.Add(jvparam);
                                            }
                                            else
                                            {
                                                companypayroll.ArrJVParameter = Objjvparam;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                jv.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                ArrJVParameter jvparam = new ArrJVParameter()
                                {
                                //    AccountNo = jv.AccountNo,
                                //    CreditDebitBranchCode = jv.CreditDebitBranchCode,
                                //    CreditDebitBranchName = jv.CreditDebitBranchName,
                                //    CreditDebitFlag = jv.CreditDebitFlag,
                                //    JVGroup = jv.JVGroup,
                                //    JVName = jv.JVName,
                                //    JVProductCode = jv.JVProductCode,
                                //    SalaryHead = jv.SalaryHead,
                                //    JVNonStandardEmp = jv.JVNonStandardEmp,
                                //    SubAccountNo = jv.SubAccountNo,
                                //    DBTrack = jv.DBTrack,
                                    ArrAccountNo = jv.ArrAccountNo,
                                    ArrCreditDebitBranchCode = jv.ArrCreditDebitBranchCode,
                                    ArrCreditDebitBranchName = jv.ArrCreditDebitBranchName,
                                    ArrCreditDebitFlag = jv.ArrCreditDebitFlag,
                                    JVGroup = jv.JVGroup,
                                    ArrJVName = jv.ArrJVName,
                                    ArrJVProductCode = jv.ArrJVProductCode,
                                    ArrSalaryHead = jv.ArrSalaryHead,
                                    ArrJVNonStandardEmp = jv.ArrJVNonStandardEmp,
                                    ArrSubAccountNo = jv.ArrSubAccountNo,
                                    DBTrack = jv.DBTrack,
                                };

                                db.ArrJVParameter.Add(jvparam);
                                db.SaveChanges();
                                List<ArrJVParameter> Objjvparam = new List<ArrJVParameter>();
                                Objjvparam.Add(jvparam);
                                if (companypayroll != null)
                                {

                                    if (companypayroll.ArrJVParameter.Count > 0)
                                    {

                                        companypayroll.ArrJVParameter.Add(jvparam);
                                    }
                                    else
                                    {
                                        companypayroll.ArrJVParameter = Objjvparam;

                                    }
                                }
                            }
                            db.CompanyPayroll.Attach(companypayroll);
                            db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            List<string> Msgs = new List<string>();
                            Msgs.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new Object[] { null, null, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
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
                        //var errorMsg = sb.ToString();
                        // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        // return this.Json(new { msg = errorMsg });
                        List<string> MsgB = new List<string>();
                        var errorMsg = sb.ToString();
                        MsgB.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);

                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    return RedirectToAction("Create", new { concurrencyError = true, id = jv.Id });
                }
                catch (Exception ex)
                {
                    throw ex;
                    List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                    //return this.Json(new Object[] { null, null, Msg, JsonRequestBehavior.AllowGet });
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

            }
        }
        public class salheadlistICollection
        {
            public Array Id { get; set; }
            public Array fulldetails { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var R = db.ArrJVParameter.Include(e => e.ArrSalaryHead)
                                      .Include(a => a.JVGroup)
                                      .Include(e => e.ArrPayStruct)
                                      .Include(e => e.ArrPayStruct.Grade)
                                      .Include(e => e.ArrFuncStruct)
                                      .Include(e => e.ArrFuncStruct.Job)
                                      .Include(e => e.ArrFuncStruct.JobPosition)
                                      .Include(q => q.ArrJVNonStandardEmp)
                                    .Include(q => q.ArrJVNonStandardEmp.Select(qa => qa.EmployeePayroll))
                         .Where(e => e.Id == data).AsNoTracking().AsParallel().SingleOrDefault();

                string LocIn = "";
                string LocOut = "";
                var JVNonStandardEmpdata = R.ArrJVNonStandardEmp.Where(q => q.EmployeePayroll != null).Select(a => a.EmployeePayroll.Id).ToList();
                //foreach (var item in JVNonStandardEmpdata)
                //{

                //}
                if (R.ArrLocationIn != null)
                {

                    var LocIna = db.Location.Include(a => a.LocationObj).ToList();
                    if (LocIna != null)
                    {
                        var qLocIn = LocIna.Where(r => r.LocationObj.LocCode == R.ArrLocationIn).SingleOrDefault();
                        if (qLocIn != null)
                        {
                            LocIn = qLocIn.Id.ToString();
                        }
                    }
                }
                if (R.ArrLocationOut != null)
                {
                    var LocOuta = db.Location.Include(a => a.LocationObj).ToList();
                    if (LocOuta != null)
                    {
                        var qLocOut = LocOuta.Where(r => r.LocationObj.LocCode == R.ArrLocationOut).SingleOrDefault();
                        //var qLocOut = LocOuta.Where(a => a.LocCode == R.LocationOut).SingleOrDefault();
                        if (qLocOut != null)
                        {
                            LocOut = qLocOut.Id.ToString();
                        }
                    }
                }
                var Q = new
                {
                    JVCode = R.ArrJVProductCode,
                    JVName = R.ArrJVName,
                    SubAccountNo = R.ArrSubAccountNo,
                    CreditDebitFlag = R.ArrCreditDebitFlag,
                    Group_Id = R.JVGroup != null ? R.JVGroup.Id : 0,
                    Group_Name = R.JVGroup.LookupVal,
                    AccountNo = R.ArrAccountNo,
                    CreditDebitBranchCode = R.ArrCreditDebitBranchCode != null ? R.ArrCreditDebitBranchCode : "",
                    Irregular = R.ArrIrregular,
                    CreditDebitBranchName = R.ArrCreditDebitBranchName != null ? R.ArrCreditDebitBranchName : "",
                    Action = R.DBTrack.Action,
                    LocationIn = LocIn,
                    LocationOut = LocOut,
                    FuncStruct_Id = R.ArrFuncStruct != null ? R.ArrFuncStruct.Id : 0,
                    Job_Id = R.ArrFuncStruct != null ? R.ArrFuncStruct.Job.Id : 0,
                    JobPosition_Id = R.ArrFuncStruct != null ? R.ArrFuncStruct.JobPosition != null ? R.ArrFuncStruct.JobPosition.Id : 0 : 0,
                    PayStruct_Id = R.ArrPayStruct != null ? R.ArrPayStruct.Id : 0,
                    Grade_Id = R.ArrPayStruct != null ? R.ArrPayStruct.Grade.Id : 0
                };
                //Q.Add()
                List<salheadlistICollection> ObjSalhead = new List<salheadlistICollection>();
                var salheadlist = db.ArrJVParameter.Include(e => e.ArrSalaryHead).Where(e => e.Id == data).Select(e => e.ArrSalaryHead).ToList();

                if (salheadlist != null)
                {
                    foreach (var ca in salheadlist)
                    {

                        ObjSalhead.Add(new salheadlistICollection
                        {
                            Id = ca.Select(e => e.Id).ToArray(),
                            fulldetails = ca.Select(e => e.FullDetails).ToArray(),

                        });


                    }


                }

                var JVP = db.ArrJVParameter.Find(data);
                TempData["RowVersion"] = JVP.RowVersion;
                var Auth = JVP.DBTrack.IsModified;
                return Json(new Object[] { Q, ObjSalhead, "", Auth, JsonRequestBehavior.AllowGet });

            }
        }
        public async Task<ActionResult> EditSave(ArrJVParameter jv, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string CreditDebitFlag = form["CreditDebitFlag"] != null ? form["CreditDebitFlag"] : "";
                    string salaryheadslist = form["salaryheadslist"] != null ? form["salaryheadslist"] : "";
                    string GroupList_DDL = form["GroupList_DDL"] != null ? form["GroupList_DDL"] : "";
                    string Branch_drop = form["Branch_drop"] != null ? form["Branch_drop"] : "";

                    string LocationIn_table = form["LocationIn-table"] != null ? form["LocationIn-table"] : null;
                    string LocationOut_table = form["LocationOut-table"] != null ? form["LocationOut-table"] : null;
                    bool Irregular = form["Irregular"] != null ? Convert.ToBoolean(form["Irregular"]) : false;
                    Int32 Struct_Sel = Convert.ToInt32(form["Group"]);
                    string job_table = form["job-table"] != null ? form["job-table"] : null;
                    string jobposition_table = form["jobposition-table"] != null ? form["jobposition-table"] : null;

                    Int32 LocationIn = 0;
                    Int32 LocationOut = 0;
                    Int32 Job = 0;
                    Int32 JobPosition = 0;
                    var db_Data = db.ArrJVParameter.Include(e => e.ArrSalaryHead).Include(e => e.JVGroup).Where(e => e.Id == data).SingleOrDefault();
                    if (LocationIn_table != null)
                    {
                        var temp = Utility.StringIdsToListIds(LocationIn_table);
                        LocationIn = temp[0];
                        var loc = db.Location.Include(a => a.LocationObj).Where(a => a.Id == LocationIn).SingleOrDefault();
                        db_Data.ArrLocationIn = loc.LocationObj.LocCode.ToString();
                        jv.ArrLocationIn = loc.LocationObj.LocCode.ToString();
                    }
                    if (LocationOut_table != null)
                    {
                        var temp = Utility.StringIdsToListIds(LocationOut_table);
                        LocationOut = temp[0];
                        var loc = db.Location.Include(a => a.LocationObj).Where(a => a.Id == LocationOut).SingleOrDefault();
                        db_Data.ArrLocationOut = loc.LocationObj.LocCode.ToString();
                        jv.ArrLocationOut = loc.LocationObj.LocCode.ToString();
                    }

                    if (job_table != null)
                    {
                        var temp = Utility.StringIdsToListIds(job_table);
                        Job = temp[0];
                    }
                    if (jobposition_table != null)
                    {
                        var temp = Utility.StringIdsToListIds(jobposition_table);
                        List<string> Msg = new List<string>();
                        if (temp.Count() > 1)
                        {
                            Msg.Add("Select only one record");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        else if (temp.Count == 0)
                        {
                            Msg.Add("Select at least one record");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        JobPosition = temp[0];
                    }


                    db_Data.ArrSalaryHead = null;
                    List<SalaryHead> SalHeadVal = new List<SalaryHead>();
                    if (salaryheadslist == "")
                    {
                        List<string> Msg = new List<string>();
                        Msg.Add("Select SalaryHead");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    if (salaryheadslist != "")
                    {
                        var ids = Utility.StringIdsToListIds(salaryheadslist);
                        foreach (var ca in ids)
                        {
                            var SalHead_val = db.SalaryHead.Find(ca);
                            SalHeadVal.Add(SalHead_val);
                            db_Data.ArrSalaryHead = SalHeadVal;
                        }
                    }

                    if (Branch_drop != null && Branch_drop != "")
                    {
                        jv.ArrCreditDebitBranchName = Branch_drop;
                    }

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        db.ArrJVParameter.Attach(db_Data);
                        db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_Data.RowVersion;
                        db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                        ArrJVParameter blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.ArrJVParameter.Where(e => e.Id == data).Include(e => e.ArrSalaryHead)
                                                    .SingleOrDefault();
                            originalBlogValues = context.Entry(blog).OriginalValues;
                        }

                        jv.DBTrack = new DBTrack
                        {
                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            Action = "M",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now
                        };
                        if (GroupList_DDL != null)
                        {
                            if (GroupList_DDL != "" && GroupList_DDL != "0" && GroupList_DDL != "-Select-")
                            {
                                var val = db.LookupValue.Find(int.Parse(GroupList_DDL));
                                jv.JVGroup = val;

                                var type = db.ArrJVParameter.Include(e => e.JVGroup).Where(e => e.Id == data).SingleOrDefault();
                                IList<ArrJVParameter> typedetails = null;
                                if (type.JVGroup != null)
                                {
                                    typedetails = db.ArrJVParameter.Where(x => x.JVGroup.Id == type.JVGroup.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    typedetails = db.ArrJVParameter.Where(x => x.Id == data).ToList();
                                }
                                //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                foreach (var s in typedetails)
                                {
                                    s.JVGroup = jv.JVGroup;
                                    db.ArrJVParameter.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            else
                            {
                                var gTypeDetails = db.ArrJVParameter.Include(e => e.JVGroup).Where(x => x.Id == data).ToList();
                                foreach (var s in gTypeDetails)
                                {
                                    s.JVGroup = null;
                                    db.ArrJVParameter.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }
                        else
                        {
                            var jvgroupDetails = db.ArrJVParameter.Include(e => e.JVGroup).Where(x => x.Id == data).ToList();
                            foreach (var s in jvgroupDetails)
                            {
                                s.JVGroup = null;
                                db.ArrJVParameter.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }

                        int company_Id = 0;
                        company_Id = Convert.ToInt32(Session["CompId"]);
                        var companypayroll = new CompanyPayroll();
                        companypayroll = db.CompanyPayroll.Include(a => a.ArrJVParameter).Where(e => e.Company.Id == company_Id).SingleOrDefault();
                        if (GroupList_DDL == "535")
                        {
                            var CurCorp = db.ArrJVParameter.Find(data);
                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {

                                ArrJVParameter ObjJvParam = new ArrJVParameter()
                                {
                                    //JVProductCode = jv.JVProductCode,
                                    //JVName = jv.JVName,
                                    //CreditDebitFlag = jv.CreditDebitFlag,
                                    //CreditDebitBranchCode = jv.CreditDebitBranchCode,
                                    //CreditDebitBranchName = jv.CreditDebitBranchName,
                                    //SubAccountNo = jv.SubAccountNo,
                                    //AccountNo = jv.AccountNo,
                                    //JVGroup = jv.JVGroup,
                                    ArrLocationIn = jv.ArrLocationIn,
                                    ArrLocationOut = jv.ArrLocationOut,
                                    //Irregular = true,
                                    ArrAccountNo = jv.ArrAccountNo,
                                    ArrCreditDebitBranchCode = jv.ArrCreditDebitBranchCode,
                                    ArrCreditDebitBranchName = jv.ArrCreditDebitBranchName,
                                    ArrCreditDebitFlag = jv.ArrCreditDebitFlag,
                                    JVGroup = jv.JVGroup,
                                    ArrJVName = jv.ArrJVName,
                                    ArrJVProductCode = jv.ArrJVProductCode,
                                    ArrSubAccountNo = jv.ArrSubAccountNo,
                                    DBTrack = jv.DBTrack,
                                    Id = data,
                                    //DBTrack = jv.DBTrack
                                };
                                db.ArrJVParameter.Attach(ObjJvParam);
                                db.Entry(ObjJvParam).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(ObjJvParam).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                //var Objjvparam = new List<JVParameter>();
                                //Objjvparam.Add(ObjJvParam);
                                //if (companypayroll != null)
                                //{
                                //    if (companypayroll.JVParameter.Count > 0)
                                //    {

                                //        companypayroll.JVParameter.Add(ObjJvParam);
                                //    }
                                //    else
                                //    {
                                //        companypayroll.JVParameter = Objjvparam;

                                //    }

                                //}
                            }
                        }
                        else
                        {
                            var CurCorp = db.ArrJVParameter.Find(data);
                            TempData["CurrRowVersion"] = CurCorp.RowVersion;
                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {

                                ArrJVParameter ObjJvParam = new ArrJVParameter()
                                {
                                    ArrJVProductCode = jv.ArrJVProductCode,
                                    ArrJVName = jv.ArrJVName,
                                    ArrCreditDebitFlag = jv.ArrCreditDebitFlag,
                                    ArrCreditDebitBranchCode = jv.ArrCreditDebitBranchCode,
                                    ArrCreditDebitBranchName = jv.ArrCreditDebitBranchName,
                                    ArrSubAccountNo = jv.ArrSubAccountNo,
                                    ArrAccountNo = jv.ArrAccountNo,
                                    JVGroup = jv.JVGroup,
                                    ArrLocationIn = jv.ArrLocationIn,
                                    ArrLocationOut = jv.ArrLocationOut,
                                    ArrIrregular = false,
                                    Id = data,
                                    DBTrack = jv.DBTrack
                                };
                                db.ArrJVParameter.Attach(ObjJvParam);
                                db.Entry(ObjJvParam).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(ObjJvParam).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                //var Objjvparam = new List<JVParameter>();
                                //Objjvparam.Add(ObjJvParam);
                                //if (companypayroll != null)
                                //{
                                //    if (companypayroll.JVParameter.Count > 0)
                                //    {

                                //        companypayroll.JVParameter.Add(ObjJvParam);
                                //    }
                                //    else
                                //    {
                                //        companypayroll.JVParameter = Objjvparam;

                                //    }

                                //}
                            }
                        }

                        await db.SaveChangesAsync();
                        ts.Complete();
                        List<string> Msg = new List<string>();
                        Msg.Add("Record Updated");
                        return Json(new Utility.JsonReturnClass { Id = jv.Id, Val = jv.ArrJVName, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                }
                catch (Exception e)
                {
                    // return Json(new Object[] { null, null, e.Message.ToString(), JsonRequestBehavior.AllowGet }); 
                    List<string> Msg = new List<string>();
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    ArrJVParameter objjvparam = db.ArrJVParameter.Include(e => e.ArrSalaryHead).Where(e => e.Id == data).SingleOrDefault();
                    var salheadlist = objjvparam.ArrSalaryHead;


                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (salheadlist != null)
                        {
                            var objsalheadlist = new HashSet<int>(objjvparam.ArrSalaryHead.Select(e => e.Id));
                            objjvparam.ArrSalaryHead = null;
                            db.ArrJVParameter.Attach(objjvparam);
                            db.Entry(objjvparam).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = objjvparam.RowVersion;
                            db.Entry(objjvparam).State = System.Data.Entity.EntityState.Detached;
                            //if (objsalheadlist.Count > 0)
                            //{
                            //    return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //}
                        }


                        try
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = objjvparam.DBTrack.CreatedBy != null ? objjvparam.DBTrack.CreatedBy : null,
                                CreatedOn = objjvparam.DBTrack.CreatedOn != null ? objjvparam.DBTrack.CreatedOn : null,
                                IsModified = objjvparam.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };
                            db.Entry(objjvparam).State = System.Data.Entity.EntityState.Deleted;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, objjvparam.DBTrack);
                            //DT_JVParameter DT_Corp = (DT_JVParameter)rtn_Obj;
                            //db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            //Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            List<string> Msgr = new List<string>();
                            Msgr.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            //Log the error (uncomment dex variable name and add a line here to write a log.)
                            //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                            //return RedirectToAction("Delete");
                            Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                        }
                        catch (Exception e)
                        {
                            //return Json(new Object[] { null, null, e.Message.ToString(), JsonRequestBehavior.AllowGet });
                            Msg.Add(e.Message);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        public class P2BGridData
        {
            public int Id { get; set; }
            public string Jvcode { get; set; }
            public string JvName { get; set; }
        }
        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> JVParameterList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;


                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);

                    var BindCompList = db.CompanyPayroll.Include(e => e.ArrJVParameter)
                                      .Where(e => e.Company.Id == company_Id).ToList();

                    foreach (var z in BindCompList)
                    {
                        if (z.ArrJVParameter != null)
                        {

                            foreach (var J in z.ArrJVParameter)
                            {
                                //var aa = db.Calendar.Where(e => e.Id == Sal.Id).SingleOrDefault();
                                view = new P2BGridData()
                                {
                                    Id = J.Id,
                                    Jvcode = J.ArrJVProductCode != null ? J.ArrJVProductCode.ToString() : "",
                                    JvName = J.ArrJVName != null ? J.ArrJVName.ToString() : ""
                                };
                                model.Add(view);
                            }
                        }

                    }

                    JVParameterList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = JVParameterList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Jvcode.ToString().Contains(gp.searchString))
                                 || (e.JvName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                 || (e.Id.ToString().Contains(gp.searchString))
                                 ).Select(a => new Object[] { a.Jvcode, a.JvName, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Jvcode, a.JvName, a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = JVParameterList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Code" ? c.Jvcode.ToString() :
                                             gp.sidx == "Name" ? c.JvName.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Jvcode, a.JvName, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Jvcode, a.JvName, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Jvcode, a.JvName, a.Id }).ToList();
                        }
                        totalRecords = JVParameterList.Count();
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
        }
    }
}