using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
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
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using Payroll;
using P2BUltimate.Security;
using EMS;
using P2BUltimate.Process;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class EmployeeSeperationStructController : Controller
    {
        //
        // GET: /EmployeeSeperationStruct/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/EmployeeSeperationStruct/Index.cshtml");
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        [HttpPost]
        public ActionResult Create(EmployeeSeperationStruct EmployeeSeperationStruct, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                string Emp = forwarddata == "0" ? "" : forwarddata;
                string PayScaleAgr = form["payscaleagreement_drop"] == "0" ? "" : form["payscaleagreement_drop"];
                string Effective_date = form["EffectiveDate"] == "0" ? "" : form["EffectiveDate"];
                using (DataBaseContext db = new DataBaseContext())
                {


                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    }

                    Employee OEmployee = null;
                    EmployeeExit OEmployeePayroll = null;
                    PayScaleAgreement OPayScalAgreement = null;

                    if (PayScaleAgr != null && PayScaleAgr != "")
                    {
                        int PayScaleAgrId = int.Parse(PayScaleAgr);
                        OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();
                        if (OPayScalAgreement != null)
                        {
                            var PayScaleAssign = db.SeperationPolicyAssignment.Where(e => e.PayScaleAgreement.Id == PayScaleAgrId).ToList();
                            if (PayScaleAssign.Count == 0)
                            {
                                return Json(new { success = false, responseText = "SeperationPolicyAssignment not defined." }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "Kindly select PayScaleAgreement." }, JsonRequestBehavior.AllowGet);
                    }
                    int Comp_Id = int.Parse(Session["CompId"].ToString());
                    var CompanyPayroll_Id = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).SingleOrDefault();
                    CompanyExit CompanyExit = new CompanyExit();
                    CompanyExit = db.CompanyExit.Include(e => e.Company).Where(e => e.Company.Id == Comp_Id).FirstOrDefault();


                    foreach (var i in ids)
                    {


                        OEmployeePayroll = db.EmployeeExit.Where(e => e.Employee.Id == i).SingleOrDefault();



                        using (TransactionScope ts = new TransactionScope())
                        {
                            try
                            {
                                //uncomment
                                //LeaveStructureProcess.EmployeeLeaveStructCreationTest(OEmployeeLeave, i, OPayScalAgreement.Id, Convert.ToDateTime(EmployeeSeperationStruct.EffectiveDate), ComPanyLeave_Id.Id);
                                ServiceBook.EmployeeSeperationStructCreationTest(OEmployeePayroll, i, OPayScalAgreement.Id, Convert.ToDateTime(EmployeeSeperationStruct.EffectiveDate), CompanyExit.Id);

                            }

                            catch (DataException ex)
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
                                return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                            }
                            ts.Complete();
                            //SalaryHeadGenProcess.EmployeeSalaryStructUpdate(OEmployeePayroll, DateTime.Now);
                        }

                    }
                }
                return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

                //return View();
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

        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<EmployeeSeperationStruct> lencash = null;
        //        List<P2BGridData> model = new List<P2BGridData>();
        //        P2BGridData view = null;
        //        var oemployeedata = db.EmployeeSeperationStruct.Where(e => e.EmployeePayroll.Employee.ServiceBookDates != null
        //           && e.EmployeePayroll.Employee.ServiceBookDates.ServiceLastDate == null)
        //                            .Include(e => e.EmployeePayroll.Employee)
        //                            .Include(e => e.EmployeePayroll.Employee.EmpName).ToList();

        //        //var oemployeedata = db.EmployeePayroll
        //        //        .Join(db.Employee, p => p.Employee.Id, pc => pc.Id, (p, pc) => new { p, pc })
        //        //        .Where(p => p.p.Employee.ServiceBookDates != null && p.p.Employee.ServiceBookDates.ServiceLastDate == null)
        //        //       .Select(m => new EmployeePolicyStructdata
        //        //       {
        //        //           Id = m.p.Employee.Id,
        //        //           EmpCode = m.p.Employee.EmpCode,
        //        //           FullNameFML = m.p.Employee.EmpName.FullNameFML,
        //        //           EmployeeSeperationStruct = m.p.EmployeeSeperationStruct
        //        //       }).ToList();
        //        foreach (var item in oemployeedata)
        //        {
        //            view = new P2BGridData()
        //            {
        //                Id = item.EmployeePayroll.Employee.Id,
        //                struct_Id = item.Id.ToString(),
        //                EmpCode = item.EmployeePayroll.Employee.EmpCode,
        //                FullNameFML = item.EmployeePayroll.Employee.EmpName.FullNameFML,
        //                //Employee = z.Employee,
        //                EffectiveDate = item.EffectiveDate.Value.ToString("dd/MM/yyyy"),
        //                EndDate = null,
        //                //     PayScaleAgreement_Id = PayScaleAgr.Id
        //            };
        //            model.Add(view);

        //        }
        //        //if (gp.IsAutho == true)
        //        //{
        //        //    lencash = db.EmployeeSeperationStruct.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        //}
        //        //else
        //        //{
        //        //    lencash = db.EmployeeSeperationStruct.AsNoTracking().ToList();
        //        //}
        //        EmployeeSeperationStruct = model;
        //        IEnumerable<P2BGridData> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = EmployeeSeperationStruct;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
        //                        || (e.FullNameFML.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
        //                        || (e.EffectiveDate.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
        //                        || (e.EndDate != null ? e.EndDate.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
        //                        || (e.struct_Id.ToString().Contains(gp.searchString))
        //                        || (e.Id.ToString().Contains(gp.searchString))
        //                        ).Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate != null ? a.EndDate : "", a.struct_Id, a.Id }).ToList();

        //                //jsonData = IE.Where(e => e.struct_Id.ToString().Contains(gp.searchString)
        //                //    ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id }).ToList();

        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = EmployeeSeperationStruct;
        //            Func<P2BGridData, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
        //                                        gp.sidx == "FullNameFML" ? c.FullNameFML.ToString() :
        //                                         gp.sidx == "EffectiveDate" ? c.EffectiveDate.ToString() :
        //                                        gp.sidx == "EndDate" ? c.EndDate.ToString() :
        //                                          gp.sidx == "struct_Id" ? c.struct_Id.ToString() :
        //                                         "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
        //            }
        //            totalRecords = EmployeeSeperationStruct.Count();
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
        public class P2BGridData
        {
            public int Id { get; set; }

            public string struct_Id { get; set; }
            //public Employee Employee { get; set; }
            public string EmpCode { get; set; }//FullNameFML
            public string FullNameFML { get; set; }//FullNameFML
            public string EffectiveDate { get; set; }
            public string EndDate { get; set; }
            //  public int PayScaleAgreement_Id { get; set; }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<P2BGridData> EmployeeSeperationStruct = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                var oemployeedata = db.EmployeeSeperationStruct.Where(e => e.EmployeeExit.Employee.ServiceBookDates != null
                   && e.EmployeeExit.Employee.ServiceBookDates.ServiceLastDate == null)
                                    .Include(e => e.EmployeeExit.Employee)
                                    .Include(e => e.EmployeeExit.Employee.EmpName).ToList();

                //var oemployeedata = db.EmployeePayroll
                //        .Join(db.Employee, p => p.Employee.Id, pc => pc.Id, (p, pc) => new { p, pc })
                //        .Where(p => p.p.Employee.ServiceBookDates != null && p.p.Employee.ServiceBookDates.ServiceLastDate == null)
                //       .Select(m => new EmployeePolicyStructdata
                //       {
                //           Id = m.p.Employee.Id,
                //           EmpCode = m.p.Employee.EmpCode,
                //           FullNameFML = m.p.Employee.EmpName.FullNameFML,
                //           EmployeePolicyStruct = m.p.EmployeePolicyStruct
                //       }).ToList();
                foreach (var item in oemployeedata)
                {
                    view = new P2BGridData()
                    {
                        Id = item.EmployeeExit.Employee.Id,
                        struct_Id = item.Id.ToString(),
                        EmpCode = item.EmployeeExit.Employee.EmpCode,
                        FullNameFML = item.EmployeeExit.Employee.EmpName.FullNameFML,
                        //Employee = z.Employee,
                        EffectiveDate = item.EffectiveDate.Value.ToString("dd/MM/yyyy"),
                        EndDate = null,
                        //     PayScaleAgreement_Id = PayScaleAgr.Id
                    };
                    model.Add(view);

                }
                //if (gp.IsAutho == true)
                //{
                //    lencash = db.EmployeePolicyStruct.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                //}
                //else
                //{
                //    lencash = db.EmployeePolicyStruct.AsNoTracking().ToList();
                //}
                EmployeeSeperationStruct = model;
                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = EmployeeSeperationStruct;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                                || (e.FullNameFML.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.EffectiveDate.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.EndDate != null ? e.EndDate.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                                || (e.struct_Id.ToString().Contains(gp.searchString))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate != null ? a.EndDate : "", a.struct_Id, a.Id }).ToList();

                        //jsonData = IE.Where(e => e.struct_Id.ToString().Contains(gp.searchString)
                        //    ).Select(a => new Object[] { a.Id, a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id }).ToList();

                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmployeeSeperationStruct;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                                gp.sidx == "FullNameFML" ? c.FullNameFML.ToString() :
                                                 gp.sidx == "EffectiveDate" ? c.EffectiveDate.ToString() :
                                                gp.sidx == "EndDate" ? c.EndDate.ToString() :
                                                  gp.sidx == "struct_Id" ? c.struct_Id.ToString() :
                                                 "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.FullNameFML, a.EffectiveDate, a.EndDate, a.struct_Id, a.Id }).ToList();
                    }
                    totalRecords = EmployeeSeperationStruct.Count();
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


         public ActionResult LoadEmp(P2BGrid_Parameters gp)
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
                IEnumerable<Employee> Employee = null;
                if (gp.IsAutho == true)
                {
                    Employee = db.Employee.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    db.Database.CommandTimeout = 300;

                    var uids = db.EmployeeExit
                        .Include(e => e.EmployeeSeperationStruct)
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.ServiceBookDates)
                        .AsNoTracking()
                        //.Select(e => e.Employee.Id)
                        .ToList();
                    var muids1 = uids.Where(e => e.Employee.ServiceBookDates != null && e.Employee.ServiceBookDates.ServiceLastDate == null && e.EmployeeSeperationStruct.Count() == 0).ToList();
                    if (muids1 != null && muids1.Count() > 0)
                    {
                        var mUids = muids1.Select(e => e.Employee.Id).ToList();
                        Employee = db.Employee.Include(e => e.EmpName).Where(t => mUids.Contains(t.Id));
                    }
                    else
                    {
                        Employee = null;
                        Msg.Add(" Employee Null  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        // return this.Json(new Object[] { "", "", " ", JsonRequestBehavior.AllowGet });
                    }
                    //Employee = db.Employee.Include(e => e.EmpName).ToList();
                }

                IEnumerable<Employee> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = Employee;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                               || (e.EmpName.FullNameFML.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Employee;


                    Func<Employee, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.FullNameFML.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                       // jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpName != null ? a.EmpName.FullNameFML : "", a.Id }).ToList();
                        jsonData = IE.Select(a => new Object[] { a.Id,Convert.ToString(a.EmpCode), a.EmpName != null ? a.EmpName.FullNameFML : "" }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                       // jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpName != null ? a.EmpName.FullNameFML : "", a.Id }).ToList();
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.EmpCode), a.EmpName != null ? a.EmpName.FullNameFML : "" }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id,Convert.ToString(a.EmpCode), a.EmpName != null ? a.EmpName.FullNameFML : "" }).ToList();
                    }
                    totalRecords = Employee.Count();
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


        public ActionResult CreateEmpSeparationStructDet_partial()
        {
            return View("~/Views/Shared/Payroll/_EmployeeSeparationStructDetails.cshtml");
        }

        public class  SeperationPolicyStructEditDetails
        {
            public Array EmpSeperationStructDetails_Id { get; set; }
            public Array EmpSeperationStructDetails_FullDetails { get; set; }
        }


         public ActionResult GetLookupLvstructdetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.EmployeeSeperationStructDetails.Include(e => e.EmployeeSeperationStruct).ToList();
                IEnumerable<EmployeeSeperationStructDetails> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.EmployeeSeperationStructDetails.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id
                    //, lookupvalue = ca.EmployeeSeperationStruct
                }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            }

        }

         public JsonResult GetPayscaleagreement(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var a = db.PayScaleAgreement.Find(int.Parse(data));
                int Struct_Id = int.Parse(data);
                var a = db.EmployeeSeperationStruct.Include(e => e.EmployeeSeperationStructDetails).Include(e => e.EmployeeSeperationStructDetails.Select(r => r.SeperationPolicyAssignment))
                    .Include(e => e.EmployeeSeperationStructDetails.Select(r => r.SeperationPolicyAssignment.PayScaleAgreement))
                    .Where(e => e.Id == Struct_Id).AsNoTracking().SingleOrDefault()
                    .EmployeeSeperationStructDetails.FirstOrDefault().SeperationPolicyAssignment.PayScaleAgreement;
                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }

    public class EmployeeeparationStructEditDetails
        {
            public Array EmpSeparationStructDetails_Id { get; set; }
            public Array EmpSeparationStructDetails_FullDetails { get; set; }
        }
        public class EditData
        {
            public int Id { get; set; }
            public String SeparationFormula { get; set; }
            public bool Editable { get; set; }
            public String SeparationName {get; set;}

            //public double Amount { get; set; }
        }



         public ActionResult P2BInlineGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EditData> EmployeeSeperationStruct = null;

                List<EditData> model = new List<EditData>();
                var view = new EditData();

                int EmpId = int.Parse(gp.id);
                bool EditAppl = true;
                 


                var OEmployeeSalStruct = db.EmployeeExit.Where(e => e.Employee.Id == EmpId).Include(e => e.Employee)
                 .Include(e => e.EmployeeSeperationStruct)
                    .Include(e => e.EmployeeSeperationStruct.Select(r => r.EmployeeSeperationStructDetails))
                    .Include(e => e.EmployeeSeperationStruct.Select(r => r.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment)))
                    .Include(e => e.EmployeeSeperationStruct.Select(r => r.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment.SeperationFormula)))
                    .SingleOrDefault();

                var id = Convert.ToInt32(gp.filter);
                var OEmpSalStruct = OEmployeeSalStruct.EmployeeSeperationStruct.Where(e => e.Id == id);

                foreach (var x in OEmpSalStruct)
                {

                    var OEmpSalStructDet = x.EmployeeSeperationStructDetails;
                    foreach (var SalForAppl in OEmpSalStructDet)
                    {
                        var m = db.EmployeeSeperationStructDetails.Include(z=>z.SeperationMaster.TypeOfSeperation).Include(e => e.SeperationPolicyAssignment).Where(e => e.Id == SalForAppl.Id).SingleOrDefault();


                        //var OSalHeadFormula = x.EmpSalStructDetails
                        //    .Where(e => e.SalaryHead.Id == m.SalaryHead.Id).Select(e => e.PayScaleAssignment.SalHeadFormula
                        //         .Where(r => (r.GeoStruct.Corporate == null || r.GeoStruct.Corporate == x.GeoStruct.Corporate)
                        //             && (r.GeoStruct.Region == null || r.GeoStruct.Region == x.GeoStruct.Region)
                        //             && (r.GeoStruct.Company == null || r.GeoStruct.Company == x.GeoStruct.Company)
                        //            && (r.GeoStruct.Division == null || r.GeoStruct.Division == x.GeoStruct.Division)
                        //            && (r.GeoStruct.Location == null || r.GeoStruct.Location == x.GeoStruct.Location)
                        //            && (r.GeoStruct.Department == null || r.GeoStruct.Department == x.GeoStruct.Department)
                        //            && (r.GeoStruct.Group == null || r.GeoStruct.Group == x.GeoStruct.Group)
                        //            && (r.GeoStruct.Unit == null || r.GeoStruct.Unit == x.GeoStruct.Unit)
                        //            && (r.FuncStruct.Job == null || r.FuncStruct.Job == x.FuncStruct.Job)
                        //            && (r.FuncStruct.JobPosition == null || r.FuncStruct.JobPosition == x.FuncStruct.JobPosition)
                        //            && (r.PayStruct.Grade == null || r.PayStruct.Grade == x.PayStruct.Grade)
                        //            && (r.PayStruct.Level == null || r.PayStruct.Level == x.PayStruct.Level)
                        //             )).ToList();


                        var SalHeadForm = m.SeperationPolicyFormula; //SalaryHeadGenProcess.SalFormulaFinder(x, m.SalaryHead.Id);

                        EditAppl = false;
                        //if (SalHeadForm != null)
                        //{
                        //    if (SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "BASIC")
                        //        EditAppl = true;
                        //    else
                        //        EditAppl = false;
                        //}
                        //else
                        //{
                        //    if (SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "EPF" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PT" ||
                        //        SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ITAX" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "LWF" ||
                        //        SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ESIC" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "CPF" ||
                        //        SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PENSION")
                        //        EditAppl = false;
                        //    else
                        //        EditAppl = true;
                        //}
                        //var test = db.PayScaleAssignment.AsNoTracking().Where(e => e.SalaryHead.Id == SalForAppl.SalaryHead.Id && e.SalaryHead.SalHeadOperationType.LookupVal.ToString() != "BASIC").Include(e => e.SalHeadFormula.Select(r => r.FormulaType)).FirstOrDefault();
                        //if (test != null)
                        //{
                        //    if (test.SalHeadFormula.Count() > 0 && test.SalHeadFormula.Any(r => r.FormulaType.LookupVal.ToUpper() == "STANDARDFORMULA"))
                        //    {
                        //        EditAppl = false;
                        //    }
                        //}
                        if (SalForAppl.SeperationPolicyFormula!=null && SalForAppl.SeperationPolicyFormula.Name.ToUpper() != "")
                        {
                            view = new EditData()
                            {
                                Id = SalForAppl.Id,
                                SeparationFormula = m.SeperationPolicyFormula.Name.ToString(),
                               SeparationName=m.SeperationMaster.TypeOfSeperation.LookupVal.ToUpper(),
                                Editable = EditAppl
                            };

                            model.Add(view);
                        }
                    }
                }

                EmployeeSeperationStruct = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmployeeSeperationStruct;
                    if (gp.searchOper.Equals("eq"))
                    {
                        //if (gp.searchField == "Id")
                        //    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency.LookupVal, a.SalaryHead.Type.LookupVal, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
                        //else if (gp.searchField == "SalaryHead")
                        //    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency, a.SalaryHead.Type, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
                        //else if (gp.searchField == "Amount")
                        //    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency, a.SalaryHead.Type, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
                        //else if (gp.searchField == "Frequency")
                        //    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency, a.SalaryHead.Type, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
                        //else if (gp.searchField == "Type")
                        //    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency, a.SalaryHead.Type, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
                        //else if (gp.searchField == "SalHeadOperationType")
                        //    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency, a.SalaryHead.Type, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();

                        jsonData = IE.Select(a => new Object[] { a.SeparationFormula, a.SeparationName, a.Editable, a.Id, }).Where((e => (e.Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {a.SeparationFormula, a.SeparationName, a.Editable, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmployeeSeperationStruct;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c=> gp.sidx == "Separation Name" ? c.SeparationName.ToString() : ""
                                     //gp.sidx == "Id" ? c.Id.ToString() :""
                             );
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "SeparationFormula" ? c.SeparationFormula.ToString() :
                                       ///  gp.sidx == "Amount" ? c.Amount.ToString() :
                                       //  gp.sidx == "Frequency" ? c.SalaryHead.Frequency.LookupVal.ToString() :
                                         gp.sidx == "SeparationName" ? c.SeparationName.ToString() :"");
                                      //   gp.sidx == "SalHeadOperationType" ? c.SalaryHead.SalHeadOperationType.LookupVal.ToString() :
                                       //  gp.sidx == "Editable" ? c.Editable.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.SeparationFormula, a.SeparationName, a.Editable, a.Id  }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {a.SeparationFormula, a.SeparationName, a.Editable, a.Id  }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {a.SeparationFormula, a.SeparationName, a.Editable, a.Id  }).ToList();
                    }
                    totalRecords = EmployeeSeperationStruct.Count();
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


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.EmployeeSeperationStruct
                    .Include(e => e.EmployeeSeperationStructDetails)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        EffectiveDate = e.EffectiveDate,
                        EmployeeSeperationStructDetails = e.EmployeeSeperationStructDetails,
                        EndDate = e.EndDate,
                        Action = e.DBTrack.Action
                    }).ToList();

                List<EmployeeeparationStructEditDetails> EmpSeparationStruct = new List<EmployeeeparationStructEditDetails>();
                

                var EmpPolicyStruct_Details = db.EmployeeSeperationStruct.Include(e => e.EmployeeSeperationStructDetails).Where(e => e.Id == data).Select(e => e.EmployeeSeperationStructDetails).ToList();
                if (EmpPolicyStruct_Details != null)
                {
                   EmpSeparationStruct.Add(
                       new EmployeeeparationStructEditDetails
                        {
                            EmpSeparationStructDetails_Id = EmpPolicyStruct_Details.ToString().ToArray(),
                            //uncomment
                           // EmpSeparationStructDetails_FullDetails = EmpPolicyStruct_Details.Select(e => e.).ToArray()

                        });


                    }

              
                var Corp = db.EmployeeSeperationStruct.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, EmpSeparationStruct, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }


        public class SeperationPolicyFormulaEditdetails
        {
            public Array EmpSeparationStructDetails_Id { get; set; }
            public Array EmpSeparationStructDetails_FullDetails { get; set; }
     
        }




        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    EmployeeSeperationStruct dellvencash = db.EmployeeSeperationStruct.Where(e => e.Id == data).SingleOrDefault();

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (dellvencash.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = dellvencash.DBTrack.CreatedBy != null ? dellvencash.DBTrack.CreatedBy : null,
                                CreatedOn = dellvencash.DBTrack.CreatedOn != null ? dellvencash.DBTrack.CreatedOn : null,
                                IsModified = dellvencash.DBTrack.IsModified == true ? true : false
                            };
                            dellvencash.DBTrack = dbT;
                            db.Entry(dellvencash).State = System.Data.Entity.EntityState.Modified;

                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            var Empstructdetails = dellvencash.EmployeeSeperationStructDetails;
                            if (Empstructdetails != null)
                            {
                                var LvBankPolicy = new HashSet<int>(dellvencash.EmployeeSeperationStructDetails.Select(e => e.Id));
                                if (LvBankPolicy.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                }
                            }
                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = dellvencash.DBTrack.CreatedBy != null ? dellvencash.DBTrack.CreatedBy : null,
                                    CreatedOn = dellvencash.DBTrack.CreatedOn != null ? dellvencash.DBTrack.CreatedOn : null,
                                    IsModified = dellvencash.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = "0029",
                                    //AuthorizedOn = DateTime.Now
                                };


                                db.Entry(dellvencash).State = System.Data.Entity.EntityState.Deleted;

                                await db.SaveChangesAsync();


                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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


        [HttpPost]
        public async Task<ActionResult> EditSave(EmployeeSeperationStruct E, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var EmployeeSeperationStructDetailslist = form["EmployeeSeperationStructDetailslist"] == "0" ? "" : form["EmployeeSeperationStructDetailslist"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (Auth == false)
                    {

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    EmployeeSeperationStruct blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.EmployeeSeperationStruct.Where(e => e.Id == data).Include(e => e.EmployeeSeperationStructDetails)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    E.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };


                                    List<EmployeeSeperationStructDetails> ObjEmployeeSeparationStructDetails = new List<EmployeeSeperationStructDetails>();
                                    EmployeeSeperationStruct PolicyNamedetails = null;
                                    PolicyNamedetails = db.EmployeeSeperationStruct.Include(e => e.EmployeeSeperationStructDetails).Where(e => e.Id == data).SingleOrDefault();
                                    if (EmployeeSeperationStructDetailslist != null && EmployeeSeperationStructDetailslist != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(EmployeeSeperationStructDetailslist);
                                        foreach (var ca in ids)
                                        {
                                            var value = db.EmployeeSeperationStructDetails.Find(ca);
                                            ObjEmployeeSeparationStructDetails.Add(value);
                                            PolicyNamedetails.EmployeeSeperationStructDetails = ObjEmployeeSeparationStructDetails;
                                        }
                                    }
                                    else
                                    {
                                        PolicyNamedetails.EmployeeSeperationStructDetails = null;
                                    }

                                    var CurCorp = db.EmployeeSeperationStruct.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        EmployeeSeperationStruct ObjELVS = new EmployeeSeperationStruct()
                                        {
                                            EffectiveDate = E.EffectiveDate,
                                            EndDate = E.EndDate,

                                            Id = data,
                                            DBTrack = E.DBTrack
                                        };
                                        db.EmployeeSeperationStruct.Attach(ObjELVS);
                                        db.Entry(ObjELVS).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(ObjELVS).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }
                                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                    using (var context = new DataBaseContext())
                                    {

                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = E.Id, Val = E.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { E.Id, E.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (IncrPolicy)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (TransPolicy)databaseEntry.ToObject();
                                    E.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            EmployeeSeperationStruct blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            EmployeeSeperationStruct Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.EmployeeSeperationStruct.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            E.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            EmployeeSeperationStruct corp = new EmployeeSeperationStruct()
                            {
                                EffectiveDate = E.EffectiveDate,
                                EndDate = E.EndDate,
                                EmployeeSeperationStructDetails = E.EmployeeSeperationStructDetails,
                                Id = data,
                                DBTrack = E.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };



                            blog.DBTrack = E.DBTrack;
                            db.EmployeeSeperationStruct.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = E.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, E.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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
                return View();
            }
        }

        public ActionResult Polulate_payscale_agreement(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.PayScaleAgreement.Where(e => e.EndDate == null).ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult Update_Struct(int PayScaleAgreementId) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgreementId).AsNoTracking().SingleOrDefault();

                    var oemployee = db.EmployeeExit
                       .Include(e => e.Employee.EmpOffInfo)
                       .Include(e => e.Employee.EmpName)
                       .Include(e => e.Employee.ServiceBookDates).ToList();

                    var EmpList = oemployee.Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                               .ToList().OrderBy(d => d.Id);

                    //var EmpList = db.EmployeePayroll.AsNoTracking().OrderBy(e => e.Id)
                    //            .ToList();
                    int Comp_Id = int.Parse(Session["CompId"].ToString());
                    var CompanyPayroll_Id = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).SingleOrDefault();
                    CompanyExit CompanyExit = new CompanyExit();
                    CompanyExit = db.CompanyExit.Include(e => e.Company).Where(e => e.Company.Id == Comp_Id).FirstOrDefault();
                    foreach (var a in EmpList)
                    {
                        //Utility.DumpProcessStatus("Emp started structure update " + a.Id);


                        var EmployeeSeperationStruct = db.EmployeeSeperationStruct
                                        .Include(e => e.EmployeeSeperationStructDetails)
                                        .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationMaster))
                                         .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula))
                                        .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment))
                                        .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment.PayScaleAgreement))
                                        .Where(e => e.EmployeeExit.Id == a.Id && e.EndDate == null).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();

                        //  var SalStruct = EmployeePolicyStruct.EmployeePolicyStruct.Where(r => r.EndDate == null).SingleOrDefault();
                        if (EmployeeSeperationStruct != null)
                        {
                            var OEmployeeSeperationStructDet = EmployeeSeperationStruct.EmployeeSeperationStructDetails.Select(r => r.SeperationPolicyAssignment.PayScaleAgreement.Id == PayScaleAgreementId).ToList();
                            if (OEmployeeSeperationStructDet.Count > 0)
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                              new System.TimeSpan(0, 30, 0)))
                                {
                                    try
                                    {
                                        List<int> q = new List<int>();
                                        //if (!a.Employee.EmpOffInfo.VPFAppl)
                                        //{
                                        //    q = db.PayScaleAssignment.Where(e => e.PayScaleAgreement.Id == PayScaleAgreementId && e.SalaryHead.Code.ToUpper() != "VPF").Select(e => e.SalaryHead.Id).ToList();
                                        //}
                                        //else
                                        //{
                                        // q = db.PolicyAssignment.Where(e => e.PayScaleAgreement.Id == PayScaleAgreementId).Select(e => e.PolicyName.Id).ToList();
                                        //  }
                                        // var q = db.PayScaleAssignment.Where(e => e.PayScaleAgreement.Id == PayScaleAgreementId ).OrderBy(e => e.SalaryHead.Id).Select(e => e.SalaryHead.Id).ToList();
                                        //var t = EmployeePolicyStruct.EmployeeSeperationStructDetails.OrderBy(e => e.PolicyName.Id).Select(r => r.PolicyName.Id).Distinct().ToList();
                                        //var w = q.Except(t);

                                        //if (w.Count() > 0)
                                        //{
                                        //uncomment
                                        //LeaveStructureProcess.EmployeeLvStructureCreationWithUpdationTest(EmployeePolicyStruct.Id, 0, PayScaleAgreementId, Convert.ToDateTime(EmployeePolicyStruct.EffectiveDate), a.Id, ComPanyLeave_Id.Id);

                                        ServiceBook.EmployeeSeparationStructureCreationWithUpdationTest(EmployeeSeperationStruct.Id, 0, PayScaleAgreementId, Convert.ToDateTime(EmployeeSeperationStruct.EffectiveDate), a.Id, CompanyExit.Id);

                                        //}

                                        //uncomment
                                        //LeaveStructureProcess.EmployeeLvStructureCreationWithUpdateFormulaTest(EmployeePolicyStruct.Id, 0, PayScaleAgreementId, Convert.ToDateTime(EmployeePolicyStruct.EffectiveDate), a.Id, ComPanyLeave_Id.Id);

                                        ServiceBook.EmployeeSeparationStructureCreationWithUpdateFormulaTest(EmployeeSeperationStruct.Id, 0, PayScaleAgreementId, Convert.ToDateTime(EmployeeSeperationStruct.EffectiveDate), a.Id, CompanyExit.Id);

                                        //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 14042017
                                        ts.Complete();
                                        //Utility.DumpProcessStatus("Emp ended structure update " + a.Id);
                                    }
                                    catch (DataException ex)
                                    {
                                        LogFile Logfile = new LogFile();
                                        ErrorLog Err = new ErrorLog()
                                        {
                                            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                            ExceptionMessage = ex.InnerException.Message,
                                            ExceptionStackTrace = ex.StackTrace,
                                            LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                            LogTime = DateTime.Now
                                        };
                                        Logfile.CreateLogFile(Err);
                                        return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                                    }


                                }
                            }
                        }

                    }

                    // return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                    Msg.Add("  Data Saved successfully  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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