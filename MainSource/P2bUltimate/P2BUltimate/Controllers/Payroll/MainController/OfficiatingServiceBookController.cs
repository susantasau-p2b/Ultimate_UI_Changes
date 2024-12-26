using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Transactions;
using System.IO;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class OfficiatingServiceBookController : Controller
    {
        //
        // GET: /OfficiatingServiceBook/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/OfficiatingServiceBook/Index.cshtml");
        }

        public ActionResult Create(OfficiatingServiceBook OfficiatingServiceBook, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    var EmpOff = form["Incharge_Id"] == "0" ? "" : form["Incharge_Id"];
                    string OffActivity = form["OfficiatingParameterlist"] == "0" ? "" : form["OfficiatingParameterlist"];
                    string NewPayStruct = form["NewPayT-table"] == "0" ? "" : form["NewPayT-table"];
                    int CompId = 0;
                    if (SessionManager.UserName != null)
                    {
                        CompId = Convert.ToInt32(Session["CompId"]);
                    }
                    Employee OEmployeeOff = null;
                    EmployeePayroll OEmployeePayrollOff = null;
                    int EmpOffId = Convert.ToInt32(EmpOff);
                    if (EmpOff != null && EmpOff != "0" && EmpOff != "false")
                    {

                        List<string> MsgCheck = new List<string>();

                        OEmployeeOff = db.Employee.Include(q => q.EmpName).Where(r => r.Id == EmpOffId).AsNoTracking().AsParallel().SingleOrDefault();
                        OEmployeePayrollOff = db.EmployeePayroll.Include(e => e.OfficiatingServiceBook).Where(e => e.Employee.Id == OEmployeeOff.Id).AsNoTracking().AsParallel().SingleOrDefault();

                        if (OEmployeePayrollOff.OfficiatingServiceBook.Any(d => d.FromDate.Value.ToShortDateString() == OfficiatingServiceBook.FromDate.ToString() && d.ToDate.Value.ToShortDateString() == OfficiatingServiceBook.ToDate.ToString()))
                        {
                            {
                                MsgCheck.Add("Already officiating done for employee " + OEmployeeOff.EmpCode + " " + OEmployeeOff.EmpName.FullNameFML + " from date= " + OfficiatingServiceBook.FromDate);
                            }
                        }

                    }
                    else
                    {
                        Msg.Add(" Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (OffActivity == null || OffActivity == "")
                    {
                        Msg.Add(" Kindly select Officiating Parameter  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    Employee OEmployeec = null;
                    EmployeePayroll OEmployeePayrollc = null;
                    string EmpRel = null;
                    string EmpPro = null;
                    string EmpCPI = null;
                    string PayMonth = OfficiatingServiceBook.PayMonth;
                    OEmployeec = db.Employee.Where(r => r.Id == EmpOffId).SingleOrDefault();

                    OEmployeePayrollc = db.EmployeePayroll.Where(e => e.Employee.Id == EmpOffId).SingleOrDefault();


                    var OEmpSalProcess = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollc.Id).Include(e => e.SalaryT).SingleOrDefault();
                    var EmpSalRelTPro = OEmpSalProcess.SalaryT != null ? OEmpSalProcess.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate == null) : null;

                    if (EmpSalRelTPro != null && EmpSalRelTPro.Count() > 0)
                    {
                        if (EmpPro == null || EmpPro == "")
                        {
                            EmpPro = OEmployeec.EmpCode;
                        }
                        else
                        {
                            EmpPro = EmpPro + ", " + OEmployeec.EmpCode;
                        }
                    }

                    var OEmpCPIProcess = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollc.Id).Include(e => e.CPIEntryT).SingleOrDefault();
                    var EmpCPIPro = db.CPIEntryT.Where(e => e.EmployeePayroll_Id == OEmployeePayrollc.Id && e.PayMonth == PayMonth).Select(e => e.Id).FirstOrDefault();


                    if (EmpCPIPro == 0)
                    {
                        if (EmpCPI == null || EmpCPI == "")
                        {
                            EmpCPI = OEmployeec.EmpCode;
                        }
                        else
                        {
                            EmpCPI = EmpCPI + ", " + OEmployeec.EmpCode;
                        }
                    }



                    var OEmpSalRelT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollc.Id).Include(e => e.SalaryT).SingleOrDefault();
                    var EmpSalRelT = OEmpSalRelT.SalaryT != null ? OEmpSalRelT.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate != null) : null;

                    if (EmpSalRelT != null && EmpSalRelT.Count() > 0)
                    {
                        if (EmpRel == null || EmpRel == "")
                        {
                            EmpRel = OEmployeec.EmpCode;
                        }
                        else
                        {
                            EmpRel = EmpRel + ", " + OEmployeec.EmpCode;
                        }
                    }
                    if (EmpPro != null)
                    {
                        Msg.Add("Salary Processed for employee " + EmpPro + ". You can't Enter Officating now.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (EmpRel != null)
                    {
                        Msg.Add("Salary released for employee " + EmpRel + ". You can't change Officating now.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (EmpCPI != null)
                    {
                        Msg.Add("Please Run CPI for employee " + EmpCPI + ". And Try again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    int TransActivityId = 0;

                    if (OffActivity != null && OffActivity != "")
                    {
                        TransActivityId = int.Parse(OffActivity);

                        OfficiatingServiceBook.OfficiatingParameter = db.OfficiatingParameter.Where(e => e.Id == TransActivityId).SingleOrDefault();
                    }
                    // NKGSB Start
                    var cid = Convert.ToInt32(SessionManager.CompanyId);
                    var offpara = db.OfficiatingParameter.Where(e => e.Id == TransActivityId).SingleOrDefault();
                    if (offpara.NewPayStructOnScreenAppl == false)
                    {
                        int EmpIdw = Convert.ToInt32(Emp);
                        Employee employeew = null;
                        OEmployeeOff = db.Employee.Include(q => q.EmpName).Where(r => r.Id == EmpOffId).SingleOrDefault();
                        employeew = db.Employee.Include(q => q.EmpName).Where(r => r.Id == EmpIdw).SingleOrDefault();
                        if (offpara.GradeShiftCount != 0)
                        {
                            if (employeew.PayStruct_Id > OEmployeeOff.PayStruct_Id)
                            {

                                var pay_dataoff = db.PayStruct.Where(e => e.Company.Id == cid && e.JobStatus_Id != null && e.Id == OEmployeeOff.PayStruct_Id)
                                       .Select(e => new
                                       {
                                           code = e.Id.ToString(),
                                           GCode = e.Grade.Code,
                                           GName = e.Grade.Name.ToString(),
                                           Levelname = e.Level.Name.ToString(),
                                           EmpActingStatus = e.JobStatus.EmpActingStatus.LookupVal,
                                           EmpStatus = e.JobStatus.EmpStatus.LookupVal


                                       }).SingleOrDefault();

                                var pay_datalist = db.PayStruct.Include(e => e.Level).Include(e => e.JobStatus).Include(e => e.JobStatus.EmpActingStatus).Include(e => e.JobStatus.EmpStatus).Where(e => e.Company.Id == cid && e.JobStatus_Id != null && e.Id < employeew.PayStruct_Id).OrderByDescending(e => e.Id).ToList();
                                if (pay_dataoff.Levelname == "")
                                {
                                    var pay_data = pay_datalist.Where(e => e.JobStatus.EmpActingStatus.LookupVal == pay_dataoff.EmpActingStatus && e.JobStatus.EmpStatus.LookupVal == pay_dataoff.EmpStatus).OrderByDescending(e => e.Id).Skip(offpara.GradeShiftCount - 1).Take(1).SingleOrDefault();
                                    if (pay_data == null)
                                    {
                                        Msg.Add("Auto shift Grade not avaialble in paystructure");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                    OfficiatingServiceBook.StructureRefId = pay_data.Id;
                                }
                                else
                                {
                                    var pay_data = pay_datalist.Where(e => e.Level.Name == pay_dataoff.Levelname && e.JobStatus.EmpActingStatus.LookupVal == pay_dataoff.EmpActingStatus && e.JobStatus.EmpStatus.LookupVal == pay_dataoff.EmpStatus).OrderByDescending(e => e.Id).Skip(offpara.GradeShiftCount - 1).Take(1).SingleOrDefault();
                                    if (pay_data == null)
                                    {
                                        Msg.Add("Auto shift Grade not avaialble in paystructure");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                    OfficiatingServiceBook.StructureRefId = pay_data.Id;
                                }



                            }
                            else
                            {
                                var pay_dataoff = db.PayStruct.Where(e => e.Company.Id == cid && e.JobStatus_Id != null && e.Id == OEmployeeOff.PayStruct_Id)
                                                                      .Select(e => new
                                                                      {
                                                                          code = e.Id.ToString(),
                                                                          GCode = e.Grade.Code,
                                                                          GName = e.Grade.Name.ToString(),
                                                                          Levelname = e.Level.Name.ToString(),
                                                                          EmpActingStatus = e.JobStatus.EmpActingStatus.LookupVal,
                                                                          EmpStatus = e.JobStatus.EmpStatus.LookupVal


                                                                      }).SingleOrDefault();


                                var pay_datalist = db.PayStruct.Include(e => e.Level).Include(e => e.JobStatus).Include(e => e.JobStatus.EmpActingStatus).Include(e => e.JobStatus.EmpStatus).Where(e => e.Company.Id == cid && e.JobStatus_Id != null && e.Id < employeew.PayStruct_Id).OrderBy(e => e.Id).ToList();
                                if (pay_dataoff.Levelname == "")
                                {
                                    var pay_data = pay_datalist.Where(e => e.JobStatus.EmpActingStatus.LookupVal == pay_dataoff.EmpActingStatus && e.JobStatus.EmpStatus.LookupVal == pay_dataoff.EmpStatus).OrderBy(e => e.Id).Skip(offpara.GradeShiftCount - 1).Take(1).SingleOrDefault();
                                    if (pay_data == null)
                                    {
                                        Msg.Add("Auto shift Grade not avaialble in paystructure");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                    OfficiatingServiceBook.StructureRefId = pay_data.Id;
                                }
                                else
                                {
                                    var pay_data = pay_datalist.Where(e => e.Level.Name == pay_dataoff.Levelname && e.JobStatus.EmpActingStatus.LookupVal == pay_dataoff.EmpActingStatus && e.JobStatus.EmpStatus.LookupVal == pay_dataoff.EmpStatus).OrderBy(e => e.Id).Skip(offpara.GradeShiftCount - 1).Take(1).SingleOrDefault();
                                    if (pay_data == null)
                                    {
                                        Msg.Add("Auto shift Grade not avaialble in paystructure");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                    OfficiatingServiceBook.StructureRefId = pay_data.Id;
                                }


                            }
                        }


                    }
                    if (NewPayStruct != "" && NewPayStruct != null)
                    {
                        var payid = db.PayStruct.Find(int.Parse(NewPayStruct));
                        OfficiatingServiceBook.StructureRefId = payid.Id;
                    }

                    // NKGSB End


                    FunctAllWFDetails oAttWFDetails = new FunctAllWFDetails
                    {
                        WFStatus = 5,
                        Comments = OfficiatingServiceBook.Remark.ToString(),
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };

                    List<FunctAllWFDetails> oAttWFDetails_List = new List<FunctAllWFDetails>();
                    oAttWFDetails_List.Add(oAttWFDetails);

                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                            new System.TimeSpan(0, 30, 0)))
                    {
                        Employee OEmployee = null;
                        int EmpId = Convert.ToInt32(Emp);
                        OfficiatingServiceBook.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        OEmployee = db.Employee.Include(q => q.EmpName).Where(r => r.Id == EmpId).AsNoTracking().AsParallel().SingleOrDefault();
                        OfficiatingServiceBook.EmpFuncStruct_Id = Convert.ToInt32(OEmployee.FuncStruct_Id);
                        OfficiatingServiceBook.EmpGeoStruct_Id = Convert.ToInt32(OEmployee.GeoStruct_Id);
                        OfficiatingServiceBook.EmployeeId = Convert.ToInt32(OEmployee.Id);
                        //OfficiatingServiceBook.EmployeePayroll_Id = Convert.ToInt32(OEmployee.em);
                        OfficiatingServiceBook.EmpPayStruct_Id = Convert.ToInt32(OEmployee.PayStruct_Id);
                        OfficiatingServiceBook.OfficiatingEmpFuncStruct_Id = Convert.ToInt32(OEmployeeOff.FuncStruct_Id);
                        OfficiatingServiceBook.OfficiatingEmpGeoStruct_Id = Convert.ToInt32(OEmployeeOff.GeoStruct_Id);
                        OfficiatingServiceBook.OfficiatingEmployeeId = Convert.ToInt32(OEmployeeOff.Id);
                        OfficiatingServiceBook.OfficiatingEmpPayStruct_Id = Convert.ToInt32(OEmployeeOff.PayStruct_Id);

                        OfficiatingServiceBook OOffServiceBook = new OfficiatingServiceBook()
                        {
                            EmpFuncStruct_Id = OfficiatingServiceBook.EmpFuncStruct_Id,
                            EmpGeoStruct_Id = OfficiatingServiceBook.EmpGeoStruct_Id,
                            EmployeeId = OfficiatingServiceBook.EmployeeId,
                            //EmployeePayroll_Id = OfficiatingServiceBook.EmployeePayroll_Id,
                            EmpPayStruct_Id = OfficiatingServiceBook.EmpPayStruct_Id,
                            FromDate = OfficiatingServiceBook.FromDate,
                            OfficiatingEmpFuncStruct_Id = OfficiatingServiceBook.OfficiatingEmpFuncStruct_Id,
                            OfficiatingEmpGeoStruct_Id = OfficiatingServiceBook.OfficiatingEmpGeoStruct_Id,
                            OfficiatingEmployeeId = OfficiatingServiceBook.OfficiatingEmployeeId,
                            OfficiatingEmpPayStruct_Id = OfficiatingServiceBook.OfficiatingEmpPayStruct_Id,
                            Remark = OfficiatingServiceBook.Remark,
                            ToDate = OfficiatingServiceBook.ToDate,
                            DBTrack = OfficiatingServiceBook.DBTrack,
                            ReleaseDate = null,
                            PayMonth = OfficiatingServiceBook.PayMonth,
                            OfficiatingParameter = OfficiatingServiceBook.OfficiatingParameter,
                            StructureRefId = OfficiatingServiceBook.StructureRefId,
                            WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                            OffWFDetails = oAttWFDetails_List
                        };
                        db.OfficiatingServiceBook.Add(OOffServiceBook);
                        db.SaveChanges();

                        List<OfficiatingServiceBook> OffServiceBooklist = new List<OfficiatingServiceBook>();
                        var aa = db.EmployeePayroll.Include(e => e.OfficiatingServiceBook).Where(e => e.Employee_Id == EmpId).FirstOrDefault();
                        if (aa.OfficiatingServiceBook.Count() > 0)
                        {
                            OffServiceBooklist.AddRange(aa.OfficiatingServiceBook);
                        }
                        OffServiceBooklist.Add(OOffServiceBook);

                        aa.OfficiatingServiceBook = OffServiceBooklist;
                        db.EmployeePayroll.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();

                        Msg.Add(" Data Saved Successfully. ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                    LogTime = DateTime.Now
                };
                Logfile.CreateLogFile(Err);

                //     List<string> Msg = new List<string>();
                Msg.Add(ex.Message);

                //return Json(new Object[] { "", "", Msg }, JsonRequestBehavior.AllowGet);
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
            return View();
        }

        [HttpPost]
        public ActionResult GridDelete(int data)
        {
            List<string> Msg = new List<string>();

            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    OfficiatingServiceBook OffServBook = db.OfficiatingServiceBook.Include(e=>e.OffWFDetails).Where(e => e.Id == data).SingleOrDefault();

                    var functwfdetailsObj = OffServBook.OffWFDetails.ToList();

                    if (OffServBook.Release == true)
                    {
                        return this.Json(new { status = true, valid = true, responseText = "You cannot delete as activity is already released.", JsonRequestBehavior.AllowGet });
                    }

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            foreach (var functitem in functwfdetailsObj)
                            {
                                FunctAllWFDetails funcTdata = functitem;
                                db.Entry(funcTdata).State = System.Data.Entity.EntityState.Deleted;
                                //functwfdetails.FunctAllWFDetails.Remove(funcTdata);
                            }

                            db.OfficiatingServiceBook.Attach(OffServBook);
                            db.Entry(OffServBook).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            db.Entry(OffServBook).State = System.Data.Entity.EntityState.Detached;
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

        public ActionResult PopulateDropDownActivityList(string data, string data2)//modified by prashant 15042017
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var qurey = db.IncrActivity.Include(e => e.IncrList).ToList();
                int OEmp_Id = Convert.ToInt32(data2);
                var query = db.EmployeePolicyStruct.AsNoTracking().Where(e => e.EmployeePayroll.Employee.Id == OEmp_Id && e.EndDate == null).Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.EmployeePolicyStructDetails)
                    .Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula)).Include(e => e.EmployeePolicyStructDetails.
                    Select(r => r.PolicyFormula.OfficiatingParameter)).FirstOrDefault();
                var TransActList = query.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.OfficiatingParameter);

                var selected = (Object)null;
                //if (data2 != "" && data != "0" && data2 != "0")
                //{
                //    selected = Convert.ToInt32(data2);
                //}

                List<OfficiatingParameter> TransAct = new List<OfficiatingParameter>();
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

        }

        public class OffServBookGridData //releasegrid
        {
            public int Id { get; set; }
            public int EmpId { get; set; }
            public Employee Employee { get; set; }
            public Employee EmployeeOff { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string PayMonth { get; set; }

        }

        public class OffChildDataClass //childgrid
        {
            public int Id { get; set; }
            public bool Release { get; set; }
            public string ReleaseDate { get; set; }
            public string Activity { get; set; }
            public string EmployeeOff { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string PayMonth { get; set; }
        }

        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }

        public JsonResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.OfficiatingServiceBook
                    .Where(e => e.Id == data).SingleOrDefault();

                var returnObj = new
                {
                    Remark = Q.Remark == null ? "" : Q.Remark,
                    Releasedate = Q.ReleaseDate == null ? "" : Q.ReleaseDate.Value.ToShortDateString()
                };

                var OffServBook = db.OfficiatingServiceBook.Find(data);
                Session["RowVersion"] = OffServBook.RowVersion;
                var Auth = OffServBook.DBTrack.IsModified;
                return Json(new Object[] { returnObj, Auth }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Release(OfficiatingServiceBook OfficiatingServiceBook, FormCollection form, String forwarddata, string param) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var Id = form["releas_Id"] == "0" ? "" : form["releas_Id"];

                    var Emp = form["emp_Id"] == "0" ? "" : form["emp_Id"];
                    var ReleaseFlag = form["ReleaseFlag"] == "0" ? "" : form["ReleaseFlag"];
                    var chkrelease = Convert.ToBoolean(ReleaseFlag);

                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = Convert.ToInt32(Session["CompId"]);
                    }

                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = Utility.StringIdsToListIds(Emp);
                    }
                    else
                    {
                        Msg.Add(" Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    //  Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;



                    foreach (var Empid in ids)
                    {

                        OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == Empid).AsNoTracking().SingleOrDefault();

                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                        {
                            int OffId = Convert.ToInt32(Id.ToString().Replace(",", " "));
                            OfficiatingServiceBook OfficiatingServiceBook1 = db.OfficiatingServiceBook.Include(e => e.OfficiatingParameter).Where(e => e.Id == OffId).SingleOrDefault();
                            //  officiateprocess(OfficiatingServiceBook1);
                            OfficiatingServiceBook1.ReleaseDate = OfficiatingServiceBook.ReleaseDate;
                            OfficiatingServiceBook1.Release = Convert.ToBoolean(ReleaseFlag);
                            db.OfficiatingServiceBook.Attach(OfficiatingServiceBook1);
                            db.Entry(OfficiatingServiceBook1).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            ts.Complete();
                        }
                    }
                    Msg.Add("  Data Updated successfully  ");
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
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    //  List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult GetApplicableData(string data, string data2)
        {
            //data-selected
            //data2-empid
            using (DataBaseContext db = new DataBaseContext())
            {
                if ((data2 != null && data2 != "0") && (data != null && data != "0"))
                {
                    var Empid = Convert.ToInt32(data);
                    var Policyid = Convert.ToInt32(data2);
                    var query = db.OfficiatingParameter.Where(e => e.Id == Policyid).FirstOrDefault();

                    DateTime? ExpDate = null;
                    DateTime? ProcActDate = null;
                    string Period = null;
                    string OfficiatingEmpPayStructAppl = null;
                    if (query != null)
                    {
                        if (query.NewPayStructOnScreenAppl == true)
                        {
                            OfficiatingEmpPayStructAppl = "Manual Select Grade";
                            return Json(OfficiatingEmpPayStructAppl, JsonRequestBehavior.AllowGet);
                        }
                    }

                    return Json(null, JsonRequestBehavior.AllowGet);


                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }

            }
        }

        public ActionResult GetNewPayStructDetails(string data, string data2)
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
        //   public static void officiateprocess(OfficiatingServiceBook OfficiatingServiceBook1)
        public static double officiateprocess(EmployeePayroll OEmpPayroll, string PayMonth, double CalAmount,string SalHeadoperationtype)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                // officiate code start
                double officiateamtTotal = 0;
                var offservicebook = db.OfficiatingServiceBook.Where(e => e.EmployeeId == OEmpPayroll.Employee.Id && e.Release == true && e.PayMonth == PayMonth).ToList();
                foreach (var offserid in offservicebook)
                {

                    Boolean supannapplicable = false;
                    OfficiatingServiceBook OfficiatingServiceBook1 = db.OfficiatingServiceBook.Include(e => e.OfficiatingParameter).Where(e => e.Id == offserid.Id).SingleOrDefault();

                    var Empofficate = OfficiatingServiceBook1;
                    if (Empofficate != null)
                    {
                        List<SalEarnDedTMultiStructure> OSalEarnDedTMultiStructureoff = new List<SalEarnDedTMultiStructure>();


                        int onoffemployee = Empofficate.OfficiatingEmployeeId;
                        int onoffemployeecur = Empofficate.EmployeeId;

                        EmployeePayroll oEmployeePayrolloffobj = db.EmployeePayroll
                                 .Include(e => e.Employee.EmpName)
                                 .Include(e => e.Employee.ServiceBookDates)
                                 .Where(e => e.Employee.Id == onoffemployee)
                                 .FirstOrDefault();
                        int oEmployeePayrolloff = oEmployeePayrolloffobj.Id;

                        EmployeePayroll oEmployeePayrolloffobjcur = db.EmployeePayroll
                               .Include(e => e.Employee.EmpName)
                               .Include(e => e.Employee.ServiceBookDates)
                               .Where(e => e.Employee.Id == onoffemployeecur)
                               .FirstOrDefault();
                        int oEmployeePayrolloffcur = oEmployeePayrolloffobjcur.Id;

                        DateTime mofffrom = Convert.ToDateTime(Empofficate.FromDate.ToString());
                        DateTime moffto = Convert.ToDateTime(Empofficate.ToDate.ToString());
                        int monthsApart = 12 * (moffto.Year - mofffrom.Year) + moffto.Month - mofffrom.Month;
                        double offemptotal = 0;
                        double Actualemptotal = 0;
                        for (int iq = 0; iq <= monthsApart; iq++)
                        {
                            DateTime Dofffrom = DateTime.Now;
                            DateTime DoffTo = DateTime.Now;
                            if (iq == 0)
                            {
                                Dofffrom = mofffrom;
                            }
                            else
                            {
                                var DArrfrom1 = mofffrom.AddMonths(iq);
                                var Fd = new DateTime(DArrfrom1.Year, DArrfrom1.Month, 1);
                                Dofffrom = Fd;
                            }

                            if (moffto < new DateTime(Dofffrom.Year, Dofffrom.Month, 1).AddMonths(1).AddDays(-1))
                            {
                                DoffTo = moffto;
                            }
                            else
                            {
                                DoffTo = new DateTime(Dofffrom.Year, Dofffrom.Month, 1).AddMonths(1).AddDays(-1);

                            }


                            var comparedateoff = (Convert.ToDateTime("01/" + Dofffrom.ToString("MM/yyyy")).Date);
                            var comparedateoffend = new DateTime(Dofffrom.Year, Dofffrom.Month, 1).AddMonths(1).AddDays(-1);
                            double SalAttendanceT_monthDaysoff = comparedateoffend.Day;

                            // offemptotal salary Start
                            List<EmpSalStruct> EmpSalStructTotaloff = new List<EmpSalStruct>();
                            List<EmpSalStructDetails> EmpSalStructDetailsoff = new List<EmpSalStructDetails>();
                            List<PayScaleAssignment> PayScaleAssignmentoff = new List<PayScaleAssignment>();
                            var PayScaleAssignmentObjoff = new PayScaleAssignment();
                            List<SalaryHead> SalaryHeadoff = new List<SalaryHead>();
                            var SalaryHeadObjoff = new SalaryHead();
                            List<SalHeadFormula> SalHeadFormulaoff = new List<SalHeadFormula>();
                            var SalaryHeadFormulaObjoff = new SalHeadFormula();
                            if (Empofficate.OfficiatingParameter.OfficiatingEmpPayStructAppl == true)
                            {


                                EmpSalStructTotaloff = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == oEmployeePayrolloff && e.EffectiveDate >= comparedateoff && e.EffectiveDate <= comparedateoffend).ToList();
                                foreach (var i in EmpSalStructTotaloff)
                                {
                                    EmpSalStructDetailsoff = db.EmpSalStructDetails.Include(e => e.SalaryHead.SalHeadOperationType)
                                        .Where(e => e.EmpSalStruct.Id == i.Id).ToList();
                                    foreach (var j in EmpSalStructDetailsoff)
                                    {

                                        var SalHeadTmp = new SalaryHead();
                                        j.PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == j.PayScaleAssignment_Id).FirstOrDefault();
                                        j.SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == j.SalHeadFormula_Id).FirstOrDefault();
                                        j.SalaryHead = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id).FirstOrDefault();
                                        var id = db.EmpSalStructDetails.Include(e => e.SalaryHead).Where(r => r.Id == j.Id).Select(r => r.SalaryHead.Id).FirstOrDefault();
                                        var SalHeadData = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id)
                                            .Select(r => new
                                            {
                                                Id = r.Id,
                                                SalHeadOperationType = r.SalHeadOperationType,
                                                Frequency = r.Frequency,
                                                Type = r.Type,
                                                RoundingMethod = r.RoundingMethod,
                                                ProcessType = r.ProcessType,
                                                LvHead = r.LvHead

                                            }).FirstOrDefault();
                                        //For NKGSB Code Comment
                                        //if (SalHeadData.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING")
                                        //{
                                        //    j.SalHeadFormula = db.SalHeadFormula.Where(e => e.PayStruct_Id == i.PayStruct_Id && e.Name.ToUpper() == "OFFICIATING").FirstOrDefault();
                                        //}
                                        

                                        if (SalHeadData.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                                        {
                                            if (Empofficate.OfficiatingParameter.ScaleFirstBasic == true)
                                            {
                                                var OSalHeadFormulaResult = j.SalHeadFormula;
                                                List<BasicScaleDetails> OBasicScale = null;
                                                if (OSalHeadFormulaResult != null)
                                                {


                                                    var OSalHeadFormula = db.SalHeadFormula
                                                                        .Include(e => e.BASICDependRule)
                                                                        .Include(e => e.BASICDependRule.BasicScale)
                                                                        .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                                                                        .Where(e => e.Id == OSalHeadFormulaResult.Id).SingleOrDefault();

                                                    OBasicScale = OSalHeadFormula.BASICDependRule.BasicScale.BasicScaleDetails
                                                                                .Select(t => new BasicScaleDetails
                                                                                {
                                                                                    StartingSlab = t.StartingSlab,
                                                                                    EndingSlab = t.EndingSlab,
                                                                                    IncrementAmount = t.IncrementAmount,
                                                                                    IncrementCount = t.IncrementCount,
                                                                                    EBMark = t.EBMark,
                                                                                }).ToList();



                                                    OBasicScale = OBasicScale.OrderBy(e => e.StartingSlab).ToList();
                                                    double mNewBasic = 0;

                                                    foreach (var OBasicScaleRange in OBasicScale)
                                                    {
                                                        mNewBasic = OBasicScaleRange.StartingSlab;
                                                        break;
                                                    }
                                                    j.Amount = mNewBasic;// scale first basic


                                                }

                                            }
                                        }
                                        j.SalaryHead.SalHeadOperationType = SalHeadData.SalHeadOperationType;
                                        j.SalaryHead.Frequency = SalHeadData.Frequency;
                                        j.SalaryHead.Type = SalHeadData.Type;
                                        j.SalaryHead.RoundingMethod = SalHeadData.RoundingMethod;
                                        j.SalaryHead.ProcessType = SalHeadData.ProcessType;
                                        SalHeadTmp.LeaveDependPolicy = db.SalaryHead.Where(e => e.Id == id).Select(e => e.LeaveDependPolicy).FirstOrDefault();
                                        foreach (var item in SalHeadTmp.LeaveDependPolicy)
                                        {
                                            item.LvHead = db.LeaveDependPolicy.Where(e => e.Id == item.Id).Select(t => t.LvHead).FirstOrDefault();
                                        }


                                    }

                                    i.EmpSalStructDetails = EmpSalStructDetailsoff;

                                    Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(i, oEmployeePayrolloffobj, i.EffectiveDate);
                                }

                            }
                            else // ofiiciate on selected grade/shift grade
                            {
                                // new grade sturcture create and formula update code start
                               
                               
                                int structid = 0;
                                List<EmpSalStruct> EmpSalStructTotaloffsel = new List<EmpSalStruct>();
                                EmpSalStructTotaloffsel = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == oEmployeePayrolloffcur && e.EffectiveDate >= comparedateoff && e.EffectiveDate <= comparedateoffend).ToList();
                               
                                foreach (var shiftgrade in EmpSalStructTotaloffsel)
                                {
                                    var OEmpSalStructDetailsNew = new List<EmpSalStructDetails>();
                                    EmpSalStruct OEmpSalStructNew = new EmpSalStruct();
                                    DateTime? mEffectiveDate = shiftgrade.EffectiveDate;
                                    structid=structid + 1;
                                    List<EmpSalStructDetails> OEmpSalStructDetailsCurrent1 = db.EmpSalStructDetails
                                        .Include(e => e.SalaryHead)
                                        .Include(e => e.SalaryHead.SalHeadOperationType)
                                          .Include(e => e.SalaryHead.RoundingMethod)
                                        .Include(e => e.SalHeadFormula)
                                        .Include(e => e.PayScaleAssignment)
                                        .Include(e => e.PayScaleAssignment.SalHeadFormula)
                                        .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.GeoStruct))
                                         .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.PayStruct))
                                          .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.FuncStruct))
                                            .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.FormulaType))
                                        .Where(e => e.EmpSalStruct_Id == shiftgrade.Id).ToList();
                                    OEmpSalStructNew.Id = structid ;
                                    OEmpSalStructNew.EffectiveDate = shiftgrade.EffectiveDate;
                                    OEmpSalStructNew.EndDate = shiftgrade.EndDate;
                                    OEmpSalStructNew.GeoStruct = db.GeoStruct.Find(shiftgrade.GeoStruct_Id);
                                    OEmpSalStructNew.FuncStruct = db.FuncStruct.Find(shiftgrade.FuncStruct_Id);
                                    OEmpSalStructNew.PayStruct = db.PayStruct.Where(e => e.Id == Empofficate.StructureRefId).SingleOrDefault();
                                    foreach (var ca in OEmpSalStructDetailsCurrent1)
                                    {
                                      
                                        var OEmpSalStructDetail = new EmpSalStructDetails();
                                        OEmpSalStructDetail.Amount = ca.Amount;
                                        OEmpSalStructDetail.DBTrack = ca.DBTrack;
                                        OEmpSalStructDetail.PayScaleAssignment = ca.PayScaleAssignment;
                                        OEmpSalStructDetail.SalaryHead = ca.SalaryHead;
                                        OEmpSalStructDetail.SalHeadFormula = Process.SalaryHeadGenProcess.SalFormulaFinderNew(OEmpSalStructNew, ca.PayScaleAssignment, ca.SalaryHead.Id);// ca.SalHeadFormula;
                                        OEmpSalStructDetailsNew.Add(OEmpSalStructDetail);
                                    }
                                    OEmpSalStructNew.EmpSalStructDetails = OEmpSalStructDetailsNew;
                                    Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OEmpSalStructNew, oEmployeePayrolloffobjcur, mEffectiveDate);
                                    EmpSalStructTotaloff.Add(OEmpSalStructNew);
                                    
                                }
                                // new grade sturcture create and formula update code end

                                // new grade sturcture basic update(increment) dependent head value update start
                                foreach (var i in EmpSalStructTotaloff)
                                {
                                    //EmpSalStructDetailsoff = db.EmpSalStructDetails.Include(e => e.SalaryHead.SalHeadOperationType)
                                    //    .Where(e => e.EmpSalStruct.Id == i.Id).ToList();


                                    //  foreach (var j in EmpSalStructDetailsoff)
                                    foreach (var j in i.EmpSalStructDetails)
                                    {

                                        var SalHeadTmp = new SalaryHead();
                                        j.PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == j.PayScaleAssignment.Id).FirstOrDefault();
                                        if (j.SalHeadFormula != null)
                                        {
                                            j.SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == j.SalHeadFormula.Id).FirstOrDefault();
                                        }
                                        else
                                        {
                                            j.SalHeadFormula = null;
                                        }
                                        j.SalaryHead = db.SalaryHead.Where(e => e.Id == j.SalaryHead.Id).FirstOrDefault();
                                        //  var id = db.EmpSalStructDetails.Include(e => e.SalaryHead).Where(r => r.Id == j.Id).Select(r => r.SalaryHead.Id).FirstOrDefault();
                                        var SalHeadData = db.SalaryHead.Where(e => e.Id == j.SalaryHead.Id)
                                            .Select(r => new
                                            {
                                                Id = r.Id,
                                                SalHeadOperationType = r.SalHeadOperationType,
                                                Frequency = r.Frequency,
                                                Type = r.Type,
                                                RoundingMethod = r.RoundingMethod,
                                                ProcessType = r.ProcessType,
                                                LvHead = r.LvHead

                                            }).FirstOrDefault();

                                      
                                        if (SalHeadData.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                                        {
                                            if (Empofficate.OfficiatingParameter.NewGradeBasicAppl == true)
                                            {
                                                var OSalHeadFormulaResult = j.SalHeadFormula;
                                                List<BasicScaleDetails> OBasicScale = null;
                                                if (OSalHeadFormulaResult != null)
                                                {


                                                    var OSalHeadFormula = db.SalHeadFormula
                                                                        .Include(e => e.BASICDependRule)
                                                                        .Include(e => e.BASICDependRule.BasicScale)
                                                                        .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                                                                        .Where(e => e.Id == OSalHeadFormulaResult.Id).SingleOrDefault();

                                                    OBasicScale = OSalHeadFormula.BASICDependRule.BasicScale.BasicScaleDetails
                                                                                .Select(t => new BasicScaleDetails
                                                                                {
                                                                                    StartingSlab = t.StartingSlab,
                                                                                    EndingSlab = t.EndingSlab,
                                                                                    IncrementAmount = t.IncrementAmount,
                                                                                    IncrementCount = t.IncrementCount,
                                                                                    EBMark = t.EBMark,
                                                                                }).ToList();



                                                    OBasicScale = OBasicScale.OrderBy(e => e.StartingSlab).ToList();
                                                    double mNewBasic = 0;
                                                    double mOldBasic = j.Amount;
                                                    List<BasicScaleDetails> OBasicScaleNew = OBasicScale;

                                                   double mFittmentAmount = Process.ServiceBook.BasicFittmentSelector(mOldBasic, OBasicScaleNew);

                                                   mOldBasic = mOldBasic + mFittmentAmount;

                                                    for (int k = 0; k < Empofficate.OfficiatingParameter.IncrementCount; k++)
                                                    {
                                                        foreach (var OBasicScaleRange in OBasicScale)
                                                        {
                                                            mNewBasic = mOldBasic + OBasicScaleRange.IncrementAmount;
                                                            if (mNewBasic > OBasicScaleRange.StartingSlab && mNewBasic <= OBasicScaleRange.EndingSlab)
                                                            {
                                                                mOldBasic = mNewBasic;
                                                                break;
                                                            }
                                                            else if (mNewBasic <= OBasicScaleRange.StartingSlab)
                                                            {
                                                                mNewBasic = OBasicScaleRange.StartingSlab;
                                                                break;
                                                            }
                                                            else if (mNewBasic > OBasicScaleRange.EndingSlab)
                                                            {
                                                                mNewBasic = mOldBasic;

                                                            }
                                                        }
                                                    }

                                                    j.Amount = mNewBasic;// scale first basic


                                                }

                                            }
                                        }
                                        j.SalaryHead.SalHeadOperationType = SalHeadData.SalHeadOperationType;
                                        j.SalaryHead.Frequency = SalHeadData.Frequency;
                                        j.SalaryHead.Type = SalHeadData.Type;
                                        j.SalaryHead.RoundingMethod = SalHeadData.RoundingMethod;
                                        j.SalaryHead.ProcessType = SalHeadData.ProcessType;
                                        SalHeadTmp.LeaveDependPolicy = db.SalaryHead.Where(e => e.Id == j.SalaryHead.Id).Select(e => e.LeaveDependPolicy).FirstOrDefault();
                                        foreach (var item in SalHeadTmp.LeaveDependPolicy)
                                        {
                                            item.LvHead = db.LeaveDependPolicy.Where(e => e.Id == item.Id).Select(t => t.LvHead).FirstOrDefault();
                                        }


                                    }

                                    i.EmpSalStructDetails = i.EmpSalStructDetails;

                                    Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(i, oEmployeePayrolloffobj, i.EffectiveDate);
                                }
                                // new grade sturcture basic update(increment) dependent head value update  end


                            }


                            List<EmpSalStruct> mEmpSalStructTotaloff = EmpSalStructTotaloff.Where(e => e.EffectiveDate.Value.Date >= comparedateoff && e.EffectiveDate.Value.Date <= comparedateoffend)
                                                 .OrderBy(r => r.EffectiveDate)
                                                 .ToList();
                            if (mEmpSalStructTotaloff.Count() == 1)
                            {
                                foreach (EmpSalStruct OMultiEmpStruct in mEmpSalStructTotaloff)
                                {
                                    int mPayDaysRunningoff = 0;
                                    mPayDaysRunningoff = (DoffTo.Date - Dofffrom.Date).Days + 1;

                                    var OEmpsalhead = OMultiEmpStruct.EmpSalStructDetails.OrderBy(e => e.SalaryHead.SeqNo).ToList();
                                    var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayrollPeriod).Include(e => e.PayFrequency).SingleOrDefault();
                                    foreach (var OSalStructDetails in OEmpsalhead)
                                    {
                                        if (OSalStructDetails.SalHeadFormula != null && OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == SalHeadoperationtype)
                                        {
                                            double SalAmount = 0;
                                            bool appearhead = false;
                                            appearhead = OSalStructDetails.SalaryHead.OnAttend;
                                            SalAmount = SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpsalhead, oEmployeePayrolloffobj, OMultiEmpStruct.EffectiveDate.Value.ToString("MM/yyyy"), false);

                                            //offemptotal
                                            offemptotal = offemptotal + offAmountCalc(SalAmount, appearhead, mPayDaysRunningoff, SalAttendanceT_monthDaysoff, Dofffrom.Date);



                                        }
                                    }


                                }

                            }
                            else
                            {


                                foreach (EmpSalStruct OMultiEmpStruct in mEmpSalStructTotaloff)
                                {
                                    int mPayDaysRunningoff = 0;

                                    if (OMultiEmpStruct.EffectiveDate.Value.Date <= Dofffrom.Date && OMultiEmpStruct.EndDate != null)
                                    {
                                        mPayDaysRunningoff = (OMultiEmpStruct.EndDate.Value.Date - Dofffrom.Date).Days + 1;

                                    }
                                    else
                                    {
                                        if (OMultiEmpStruct.EndDate != null && OMultiEmpStruct.EffectiveDate.Value.Date <= DoffTo.Date)
                                        {
                                            mPayDaysRunningoff = (DoffTo.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1;
                                        }
                                        else
                                        {
                                            mPayDaysRunningoff = (DoffTo.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1;
                                        }
                                    }
                                    Dofffrom = Dofffrom.Date.AddDays(mPayDaysRunningoff);

                                    var OEmpsalhead = OMultiEmpStruct.EmpSalStructDetails.OrderBy(e => e.SalaryHead.SeqNo).ToList();

                                    foreach (var OSalStructDetails in OEmpsalhead)
                                    {
                                        if (OSalStructDetails.SalHeadFormula != null && OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == SalHeadoperationtype)
                                        {
                                            double SalAmount = 0;
                                            bool appearhead = false;
                                            appearhead = OSalStructDetails.SalaryHead.OnAttend;
                                            SalAmount = SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpsalhead, oEmployeePayrolloffobj, OMultiEmpStruct.EffectiveDate.Value.ToString("MM/yyyy"), false);
                                            //offemptotal
                                            offemptotal = offemptotal + offAmountCalc(SalAmount, appearhead, mPayDaysRunningoff, SalAttendanceT_monthDaysoff, Dofffrom.Date);


                                        }
                                    }




                                }

                            }

                            // offemptotal salary end


                            // Currentemptotal salary end
                           
                            List<EmpSalStruct> EmpSalStructTotaloffcur = new List<EmpSalStruct>();
                            List<EmpSalStructDetails> EmpSalStructDetailsoffcur = new List<EmpSalStructDetails>();
                            List<PayScaleAssignment> PayScaleAssignmentoffcur = new List<PayScaleAssignment>();
                            var PayScaleAssignmentObjoffcur = new PayScaleAssignment();
                            List<SalaryHead> SalaryHeadoffcur = new List<SalaryHead>();
                            var SalaryHeadObjoffcur = new SalaryHead();
                            List<SalHeadFormula> SalHeadFormulaoffcur = new List<SalHeadFormula>();
                            var SalaryHeadFormulaObjoffcur = new SalHeadFormula();

                            EmpSalStructTotaloffcur = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == oEmployeePayrolloffcur && e.EffectiveDate >= comparedateoff && e.EffectiveDate <= comparedateoffend).ToList();
                            foreach (var i in EmpSalStructTotaloffcur)
                            {
                                EmpSalStructDetailsoffcur = db.EmpSalStructDetails.Include(e => e.SalaryHead.SalHeadOperationType)
                                    .Where(e => e.EmpSalStruct.Id == i.Id).ToList();
                                foreach (var j in EmpSalStructDetailsoffcur)
                                {

                                    var SalHeadTmp = new SalaryHead();
                                    j.PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == j.PayScaleAssignment_Id).FirstOrDefault();
                                    j.SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == j.SalHeadFormula_Id).FirstOrDefault();
                                    j.SalaryHead = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id).FirstOrDefault();
                                    var id = db.EmpSalStructDetails.Include(e => e.SalaryHead).Where(r => r.Id == j.Id).Select(r => r.SalaryHead.Id).FirstOrDefault();
                                    var SalHeadData = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id)
                                        .Select(r => new
                                        {
                                            Id = r.Id,
                                            SalHeadOperationType = r.SalHeadOperationType,
                                            Frequency = r.Frequency,
                                            Type = r.Type,
                                            RoundingMethod = r.RoundingMethod,
                                            ProcessType = r.ProcessType,
                                            LvHead = r.LvHead

                                        }).FirstOrDefault();
                                    //For NKGSB Code Comment
                                    //if (SalHeadData.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING")
                                    //{
                                    //    j.SalHeadFormula = db.SalHeadFormula.Where(e => e.PayStruct_Id == i.PayStruct_Id && e.Name.ToUpper() == "OFFICIATING").FirstOrDefault();
                                    //}
                                    j.SalaryHead.SalHeadOperationType = SalHeadData.SalHeadOperationType;
                                    j.SalaryHead.Frequency = SalHeadData.Frequency;
                                    j.SalaryHead.Type = SalHeadData.Type;
                                    j.SalaryHead.RoundingMethod = SalHeadData.RoundingMethod;
                                    j.SalaryHead.ProcessType = SalHeadData.ProcessType;
                                    SalHeadTmp.LeaveDependPolicy = db.SalaryHead.Where(e => e.Id == id).Select(e => e.LeaveDependPolicy).FirstOrDefault();
                                    foreach (var item in SalHeadTmp.LeaveDependPolicy)
                                    {
                                        item.LvHead = db.LeaveDependPolicy.Where(e => e.Id == item.Id).Select(t => t.LvHead).FirstOrDefault();
                                    }


                                }

                                i.EmpSalStructDetails = EmpSalStructDetailsoffcur;


                            }




                            List<EmpSalStruct> mEmpSalStructTotaloffcur = EmpSalStructTotaloffcur.Where(e => e.EffectiveDate.Value.Date >= comparedateoff && e.EffectiveDate.Value.Date <= comparedateoffend)
                                                 .OrderBy(r => r.EffectiveDate)
                                                 .ToList();
                            if (mEmpSalStructTotaloffcur.Count() == 1)
                            {
                                foreach (EmpSalStruct OMultiEmpStruct in mEmpSalStructTotaloffcur)
                                {
                                    int mPayDaysRunningoff = 0;
                                    mPayDaysRunningoff = (DoffTo.Date - Dofffrom.Date).Days + 1;

                                    var OEmpsalhead = OMultiEmpStruct.EmpSalStructDetails.OrderBy(e => e.SalaryHead.SeqNo).ToList();
                                    var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayrollPeriod).Include(e => e.PayFrequency).SingleOrDefault();
                                    foreach (var OSalStructDetails in OEmpsalhead)
                                    {
                                        if (OSalStructDetails.SalHeadFormula != null && OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == SalHeadoperationtype)
                                        {
                                            double SalAmount = 0;
                                            bool appearhead = false;
                                            appearhead = OSalStructDetails.SalaryHead.OnAttend;
                                            SalAmount = SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpsalhead, oEmployeePayrolloffobjcur, OMultiEmpStruct.EffectiveDate.Value.ToString("MM/yyyy"), false);
                                            //Actualemptotal
                                            Actualemptotal = Actualemptotal + offAmountCalc(SalAmount, appearhead, mPayDaysRunningoff, SalAttendanceT_monthDaysoff, Dofffrom.Date);



                                        }
                                    }


                                }

                            }
                            else
                            {


                                foreach (EmpSalStruct OMultiEmpStruct in mEmpSalStructTotaloffcur)
                                {
                                    if (iq == 0)
                                    {
                                        Dofffrom = mofffrom;
                                    }
                                    else
                                    {
                                        var DArrfrom1 = mofffrom.AddMonths(iq);
                                        var Fd = new DateTime(DArrfrom1.Year, DArrfrom1.Month, 1);
                                        Dofffrom = Fd;
                                    }
                                    if (moffto < new DateTime(Dofffrom.Year, Dofffrom.Month, 1).AddMonths(1).AddDays(-1))
                                    {
                                        DoffTo = moffto;
                                    }
                                    else
                                    {
                                        DoffTo = new DateTime(Dofffrom.Year, Dofffrom.Month, 1).AddMonths(1).AddDays(-1);

                                    }
                                    int mPayDaysRunningoff = 0;

                                    if (OMultiEmpStruct.EffectiveDate.Value.Date <= Dofffrom.Date && OMultiEmpStruct.EndDate != null)
                                    {
                                        mPayDaysRunningoff = (OMultiEmpStruct.EndDate.Value.Date - Dofffrom.Date).Days + 1;

                                    }
                                    else
                                    {
                                        if (OMultiEmpStruct.EndDate != null && OMultiEmpStruct.EffectiveDate.Value.Date <= DoffTo.Date)
                                        {
                                            mPayDaysRunningoff = (DoffTo.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1;
                                        }
                                        else
                                        {
                                            mPayDaysRunningoff = (DoffTo.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1;
                                        }
                                    }
                                    Dofffrom = Dofffrom.Date.AddDays(mPayDaysRunningoff);

                                    var OEmpsalhead = OMultiEmpStruct.EmpSalStructDetails.OrderBy(e => e.SalaryHead.SeqNo).ToList();

                                    foreach (var OSalStructDetails in OEmpsalhead)
                                    {
                                        if (OSalStructDetails.SalHeadFormula != null && OSalStructDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == SalHeadoperationtype)
                                        {
                                            double SalAmount = 0;
                                            bool appearhead = false;
                                            appearhead = OSalStructDetails.SalaryHead.OnAttend;
                                            SalAmount = SalHeadAmountCalc(OSalStructDetails.SalHeadFormula.Id, null, OEmpsalhead, oEmployeePayrolloffobjcur, OMultiEmpStruct.EffectiveDate.Value.ToString("MM/yyyy"), false);
                                            //Actualemptotal
                                            Actualemptotal = Actualemptotal + offAmountCalc(SalAmount, appearhead, mPayDaysRunningoff, SalAttendanceT_monthDaysoff, Dofffrom.Date);


                                        }
                                    }




                                }

                            }


                            // Currentemptotal salary end

                            //supannu applicable
                           
                            List<EmpSalStruct> mEmpSalStructTotaloffcursupann = EmpSalStructTotaloffcur.Where(e => e.EffectiveDate.Value.Date >= comparedateoff && e.EffectiveDate.Value.Date <= comparedateoffend)
                                               .OrderBy(r => r.EffectiveDate)
                                               .ToList();
                            if (mEmpSalStructTotaloffcursupann.Count()>0)
                            {
                                foreach (EmpSalStruct OMultiEmpStruct in mEmpSalStructTotaloffcursupann)
                                {
                                    var OEmpsalhead = OMultiEmpStruct.EmpSalStructDetails.Where(e => e.SalaryHead.Code == "SUPANNU" && e.Amount!=0).FirstOrDefault();
                                    if (OEmpsalhead!=null)
                                    {
                                        supannapplicable = true;
                                    }
                                    else
                                    {
                                        supannapplicable = false;
                                    }
                                }
                            }



                        }



                        // update off amount in starture


                        // officiateamt = offemptotal - Actualemptotal;
                        double officiateamt = 0;
                        var offservicebookcheck = db.OfficiatingServiceBook.Include(e => e.OfficiatingParameter).Where(e => e.EmployeeId == OEmpPayroll.Employee.Id && e.Release == true && e.PayMonth == PayMonth).FirstOrDefault();

                        if (offservicebookcheck.OfficiatingParameter.PayAmountuppergradediffAppl == true)
                        {
                            officiateamt = offemptotal - Actualemptotal;
                        }
                        else
                        {
                            officiateamt = offemptotal;
                        }
                        if (SalHeadoperationtype=="SUPANNOFFICIATING")
                        {
                            if (supannapplicable==false)
                            {
                                officiateamt = 0;
                            }

                        }
                        if (officiateamt <= 0)
                        {
                            officiateamt = 0;
                        }
                        officiateamtTotal = officiateamtTotal + officiateamt;

                        //var comparedateoffupdate = (Convert.ToDateTime("01/" + Empofficate.PayMonth).Date);
                        //EmpSalStruct OEmpSalStructCurrent = null;
                        //OEmpSalStructCurrent = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == oEmployeePayrolloffcur).Where(e => e.EndDate == null).SingleOrDefault();
                        //List<EmpSalStructDetails> EmpSalStructDetailsList = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OEmpSalStructCurrent.Id).ToList();
                        //OEmpSalStructCurrent.EmpSalStructDetails = EmpSalStructDetailsList;
                        //GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmpSalStructCurrent.GeoStruct_Id).SingleOrDefault();
                        //OEmpSalStructCurrent.GeoStruct = GeoStruct;
                        //FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmpSalStructCurrent.FuncStruct_Id).SingleOrDefault();
                        //OEmpSalStructCurrent.FuncStruct = FuncStruct;
                        //PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmpSalStructCurrent.PayStruct_Id).SingleOrDefault();
                        //OEmpSalStructCurrent.PayStruct = PayStruct;
                        //foreach (var EmpSalStructDetailsListitem in EmpSalStructDetailsList)
                        //{
                        //    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsListitem.SalaryHead_Id).SingleOrDefault();
                        //    EmpSalStructDetailsListitem.SalaryHead = SalaryHead;
                        //    LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                        //    EmpSalStructDetailsListitem.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                        //    LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                        //    EmpSalStructDetailsListitem.SalaryHead.RoundingMethod = RoundingMethod;
                        //    SalHeadFormula SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == EmpSalStructDetailsListitem.SalHeadFormula_Id).SingleOrDefault();
                        //    EmpSalStructDetailsListitem.SalHeadFormula = SalHeadFormula;
                        //    PayScaleAssignment PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == EmpSalStructDetailsListitem.PayScaleAssignment_Id).SingleOrDefault();
                        //    EmpSalStructDetailsListitem.PayScaleAssignment = PayScaleAssignment;

                        //}

                        //EmpSalStructDetails OEmpSalStructDetailsCurrent = OEmpSalStructCurrent.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING").FirstOrDefault();
                        //if (OEmpSalStructCurrent != null)
                        //{
                        //    OEmpSalStructDetailsCurrent.Amount = OEmpSalStructDetailsCurrent.Amount + officiateamt; // if previous two month officiating payment in current month
                        //    db.EmpSalStructDetails.Attach(OEmpSalStructDetailsCurrent);
                        //    db.Entry(OEmpSalStructDetailsCurrent).State = System.Data.Entity.EntityState.Modified;
                        //    db.SaveChanges();
                        //}




                    }
                }
                CalAmount = officiateamtTotal;
                // officiate code End
                return CalAmount;
            }

        }
        public static double SalHeadAmountCalc(Int32 OSalHeadFormula, List<SalEarnDedT> OSalaryDetails, List<EmpSalStructDetails> OEmpSalStructDetails,
                                              EmployeePayroll OEmployeePayroll, string mCPIMonth, bool VPFApplicable = false)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var OSalHeadFormualQuery = Process.SalaryHeadGenProcess._returnSalHeadFormula(OSalHeadFormula);
                double mSalWages = 0;
                double mSalHeadAmount = 0;
                mSalWages = Process.SalaryHeadGenProcess.Wagecalc(OSalHeadFormualQuery, OSalaryDetails, OEmpSalStructDetails);
                mSalHeadAmount = Process.SalaryHeadGenProcess.CellingCkeck(OSalHeadFormualQuery, mSalWages);
                mSalHeadAmount = Math.Round(mSalHeadAmount + 0.01, 0);
                return mSalHeadAmount;
            }
        }
        public static double offAmountCalc(double SalAmount, bool appearhead, int mPayDaysRunningoff, double SalAttendanceT_monthDaysoff, DateTime Dofffromdate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int mon = Dofffromdate.Month;
                double mSalHeadAmount = 0;
                var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayrollPeriod).Include(e => e.PayFrequency).SingleOrDefault();

                if (OPayProcGrp != null)
                {

                    if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                    {

                        if (appearhead == true)
                        {

                            if (mPayDaysRunningoff > 30)
                            {
                                mPayDaysRunningoff = 30;
                            }
                            if (mon == 2)//feb month
                            {
                                if (mPayDaysRunningoff == 28 || mPayDaysRunningoff == 29)
                                {
                                    mPayDaysRunningoff = 30;
                                }
                            }
                            mSalHeadAmount = ((SalAmount / 30) * mPayDaysRunningoff);
                        }
                        else
                        {
                            mSalHeadAmount = SalAmount;
                        }

                    }
                    if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                    {

                        if (appearhead == true)
                        {

                            mSalHeadAmount = ((SalAmount) - ((SalAttendanceT_monthDaysoff - mPayDaysRunningoff) / 30) * SalAmount);
                        }
                        else
                        {
                            mSalHeadAmount = SalAmount;
                        }

                    }
                    if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                    {

                        if (appearhead == true)
                        {

                            mSalHeadAmount = (SalAmount * ((mPayDaysRunningoff) / (SalAttendanceT_monthDaysoff)));
                        }
                        else
                        {
                            mSalHeadAmount = SalAmount;
                        }
                    }
                    mSalHeadAmount = Math.Round(mSalHeadAmount, 0);
                }
                // NkGSB Code Start
                
                string requiredPathLoanNet = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool existscdNet = System.IO.Directory.Exists(requiredPathLoanNet);
                string localPathLoanNet;
                if (!existscdNet)
                {
                    localPathLoanNet = new Uri(requiredPathLoanNet).LocalPath;
                    System.IO.Directory.CreateDirectory(localPathLoanNet);
                }
                string pathLoanNet = requiredPathLoanNet + @"\OFFICIATINGPAYMONTHCONCEPT" + ".ini";
                localPathLoanNet = new Uri(pathLoanNet).LocalPath;
                if (!System.IO.File.Exists(localPathLoanNet))
                {

                    using (var fs = new FileStream(localPathLoanNet, FileMode.OpenOrCreate))
                    {
                        StreamWriter str = new StreamWriter(fs);
                        str.BaseStream.Seek(0, SeekOrigin.Begin);

                        str.Flush();
                        str.Close();
                        fs.Close();
                    }


                }
                string PayMonthConcept = "";
                string JvHeadparameterName = "";
              
                string requiredPathchkNet = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool existschkNet = System.IO.Directory.Exists(requiredPathchkNet);
                string localPathchkNet;
                if (!existschkNet)
                {
                    localPathchkNet = new Uri(requiredPathchkNet).LocalPath;
                    System.IO.Directory.CreateDirectory(localPathchkNet);
                }
                string pathchkNet = requiredPathchkNet + @"\OFFICIATINGPAYMONTHCONCEPT" + ".ini";
                localPathchkNet = new Uri(pathchkNet).LocalPath;
                using (var streamReader = new StreamReader(localPathchkNet))
                {
                    string line;

                    while ((line = streamReader.ReadLine()) != null)
                    {

                        var PaymonCon = line;
                        PayMonthConcept = PaymonCon;
                       

                    }
                }
                if (PayMonthConcept!="")
                {
                    if (PayMonthConcept.ToUpper() == "FIXED30DAYS")
                    {

                        if (appearhead == true)
                        {

                            if (mPayDaysRunningoff > 30)
                            {
                                mPayDaysRunningoff = 30;
                            }
                            if (mon == 2)//feb month
                            {
                                if (mPayDaysRunningoff == 28 || mPayDaysRunningoff == 29)
                                {
                                    mPayDaysRunningoff = 30;
                                }
                            }
                            mSalHeadAmount = ((SalAmount / 30) * mPayDaysRunningoff);
                        }
                        else
                        {
                            mSalHeadAmount = SalAmount;
                        }

                    }
                    if (PayMonthConcept.ToUpper() == "30DAYS")
                    {

                        if (appearhead == true)
                        {

                            mSalHeadAmount = ((SalAmount) - ((SalAttendanceT_monthDaysoff - mPayDaysRunningoff) / 30) * SalAmount);
                        }
                        else
                        {
                            mSalHeadAmount = SalAmount;
                        }

                    }
                    if (PayMonthConcept.ToUpper() == "CALENDAR")
                    {

                        if (appearhead == true)
                        {

                            mSalHeadAmount = (SalAmount * ((mPayDaysRunningoff) / (SalAttendanceT_monthDaysoff)));
                        }
                        else
                        {
                            mSalHeadAmount = SalAmount;
                        }
                    }
                    mSalHeadAmount = Math.Round(mSalHeadAmount, 0);

                    
                }
                               
                // NKGSB Code End
                return mSalHeadAmount;
            }
        }
        public ActionResult P2BGridRelease(P2BGrid_Parameters gp, string param)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<OffServBookGridData> OffServBook = null;
                List<OffServBookGridData> model = new List<OffServBookGridData>();
                OffServBookGridData view = null;

                var OEmployee = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                    .AsNoTracking().Where(q => q.OfficiatingServiceBook.Count > 0).AsParallel().ToList();
                string PayMonth = "";
                string Month = "";
                if (gp.filter != null)
                    PayMonth = gp.filter;
                else
                {
                    if (DateTime.Now.Date.Month < 10)
                        Month = "0" + DateTime.Now.Date.Month;
                    else
                        Month = DateTime.Now.Date.Month.ToString();
                    PayMonth = Month + "/" + DateTime.Now.Date.Year;
                }
                foreach (var z in OEmployee)
                {
                    var ObjOffServBook = db.EmployeePayroll.Where(e => e.Id == z.Id)
                        .Select(e => e.OfficiatingServiceBook.Where(r => r.Release == false))
                                        .SingleOrDefault();


                    //DateTime? Eff_Date = null;
                    //PayScaleAgreement PayScaleAgr = null;
                    foreach (var a in ObjOffServBook)
                    {
                        //Eff_Date = Convert.ToDateTime(a.ProcessMonth);
                        OfficiatingServiceBook aa = new OfficiatingServiceBook();
                        aa = db.OfficiatingServiceBook
                              .Where(e => e.Id == a.Id).SingleOrDefault();

                        if (aa != null)
                        {
                            //if (aa.ProcessIncrDate.Value.ToString("MM/yyyy") == PayMonth)
                            //{
                            view = new OffServBookGridData()
                            {
                                Id = a.Id,
                                EmpId = z.Employee.Id,
                                Employee = z.Employee,
                                FromDate = aa.FromDate != null ? aa.FromDate.Value.ToString("dd/MM/yyyy") : null,
                                ToDate = aa.ToDate != null ? aa.ToDate.Value.ToString("dd/MM/yyyy") : null,
                                PayMonth = aa.PayMonth
                            };

                            model.Add(view);
                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                            //}
                        }
                    }

                }

                OffServBook = model;

                IEnumerable<OffServBookGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = OffServBook;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpId.ToString().Contains(gp.searchString))
                                || (e.Employee.EmpCode.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.Employee.EmpName.FullNameFML.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.FromDate.ToString().Contains(gp.searchString))
                                || (e.ToDate.ToString().Contains(gp.searchString))
                                || (e.PayMonth.ToString().Contains(gp.searchString))
                                || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.FromDate, a.ToDate, a.Id, a.EmpId }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.FromDate, a.ToDate, a.PayMonth, a.Id, a.EmpId }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = OffServBook;
                    Func<OffServBookGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpId" ? c.EmpId.ToString() :
                                         gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "FromDate" ? c.FromDate.ToString() :
                                         gp.sidx == "ToDate" ? c.ToDate.ToString() :
                                         gp.sidx == "PayMonth" ? c.PayMonth.ToString() : ""
                                          );
                    }
                    if (gp.sord == "asc")  //Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : ""
                    {
                        IE = IE.OrderBy(orderfuc);  // a.Id,a.Employeee.EmpCode,a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.FromDate, a.ToDate, a.PayMonth, a.Id, a.EmpId }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.FromDate, a.ToDate, a.PayMonth, a.Id, a.EmpId }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.FromDate, a.ToDate, a.PayMonth, a.Id, a.EmpId }).ToList();
                    }
                    totalRecords = OffServBook.Count();
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

        public ActionResult Get_OffServBook(int data)
        {
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.OfficiatingServiceBook)
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<OffChildDataClass> returndata = new List<OffChildDataClass>();
                        foreach (var item in db_data.OfficiatingServiceBook)
                        {
                            returndata.Add(new OffChildDataClass
                            {
                                Id = item.Id,
                                EmployeeOff = db.Employee.Include(e => e.EmpName).Where(e => e.Id == item.EmployeeId).FirstOrDefault().EmpName.FullNameFML,
                                Release = item.Release,
                                ReleaseDate = item.ReleaseDate != null ? item.ReleaseDate.Value.ToString() : null,
                                FromDate = item.FromDate != null ? item.FromDate.Value.ToString("dd/MM/yyyy") : null,
                                ToDate = item.ToDate != null ? item.ToDate.Value.ToString("dd/MM/yyyy") : null,
                                PayMonth = item.PayMonth
                            });
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.ServiceBookDates)
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null).ToList();
                    // for searchs
                    IEnumerable<EmployeePayroll> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        //fall = all.Where(e => (e.Employee.EmpCode == param.sSearch) || (e.Employee.EmpName.FullNameFML.ToUpper() == param.sSearch.ToUpper())).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                               || (e.Employee.EmpCode.Contains(param.sSearch))
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
                                //JoiningDate = item.Employee.ServiceBookDates != null && item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                                //Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                                //Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
                                //Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null
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

    }
}