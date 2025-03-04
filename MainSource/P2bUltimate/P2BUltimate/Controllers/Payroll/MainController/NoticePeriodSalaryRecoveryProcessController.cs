﻿using P2b.Global;
using P2BUltimate.Models;
using P2BUltimate.Process;
using Payroll;
using Leave;
using EMS;
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
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using P2BUltimate.App_Start;
using P2BUltimate.Security;
using Newtonsoft.Json;
using P2BUltimate.Process;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class NoticePeriodSalaryRecoveryProcessController : Controller
    {
        List<String> Msg = new List<string>();
        //
        // GET: /FFSSettlementDetailT/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/NoticePeriodSalaryRecoveryProcess/Index.cshtml");
        }
        //public class P2BGridData
        //{
        //    public int Id { get; set; }
        //    public string Code { get; set; }
        //    public string Name { get; set; }
        //    public string LastWorkDateByComp { get; set; }
        //    public string LastWorkDateApproved { get; set; }


        //}
        public class P2BGridData
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string Salcode { get; set; }
            public string SalType { get; set; }
            public double PayAmount { get; set; }
            public string PayMonth { get; set; }
            // public string ProcessMonth { get; set; }
            public string Paydate { get; set; }


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
                IEnumerable<P2BGridData> SalaryList = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;

                //   IEnumerable<NoticePeriodProcess> Corporate = null;

                var empresig = db.EmployeeExit.Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(x => x.ProcessType))
                    .Include(e => e.SeperationProcessT.NoticePeriodProcess)
                    .AsNoTracking().ToList();

                foreach (var z in empresig)
                {
                    if (z.SeperationProcessT != null && z.SeperationProcessT.FFSSettlementDetailT != null && z.SeperationProcessT.NoticePeriodProcess != null)
                    {
                        foreach (var item in z.SeperationProcessT.FFSSettlementDetailT)
                        {
                            if (item.ProcessType.LookupVal.ToUpper() == "NOTICE PERIOD SALARY RECOVERY")
                            {
                                view = new P2BGridData()
                                {
                                    Id = item.Id,
                                    EmpCode = z.Employee.EmpCode,
                                    EmpName = z.Employee.EmpName.FullNameFML,
                                    Salcode = item.SalaryHead,
                                    SalType = item.SalType,
                                    PayAmount = item.PayAmount,
                                    PayMonth = item.PayMonth.Value.Date.ToShortDateString(),
                                    //   ProcessMonth = item.ProcessMonth != null ? item.ProcessMonth.Value.Date.ToShortDateString() : "",
                                    Paydate = item.PayDate.Value.Date.ToShortDateString(),

                                };

                                model.Add(view);
                            }
                        }
                    }
                }

                SalaryList = model;

                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = SalaryList;
                    if (gp.searchOper.Equals("eq"))
                    {

                        //jsonData = IE.Where(e => (e.Narration.ToUpper().ToString().Contains(gp.searchString.ToUpper()))                             
                        //      || (e.Id.ToString().Contains(gp.searchString))
                        //      ).Select(a => new Object[] { a.Narration, a.Id }).ToList();
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                             || (e.EmpName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                              || (e.Salcode != null ? e.Salcode.ToString().Contains(gp.searchString) : false)
                             || (e.SalType != null ? e.SalType.ToString().Contains(gp.searchString) : false)
                              || (e.PayAmount != null ? e.PayAmount.ToString().Contains(gp.searchString) : false)
                               || (e.PayMonth != null ? e.PayMonth.ToString().Contains(gp.searchString) : false)
                            //|| (e.ProcessMonth != null ? e.ProcessMonth.ToString().Contains(gp.searchString) : false)
                               || (e.Paydate != null ? e.Paydate.ToString().Contains(gp.searchString) : false)

                             || (e.Id.ToString().Contains(gp.searchString))
                             ).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Salcode, a.SalType, a.PayAmount, a.PayMonth, a.Paydate != null ? Convert.ToString(a.Paydate) : "", a.Id }).ToList();



                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Salcode, a.SalType, a.PayAmount, a.PayMonth, a.Paydate != null ? Convert.ToString(a.Paydate) : "", a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = SalaryList;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.ToString() :
                                          gp.sidx == "Salcode" ? c.Salcode.ToString() :
                                           gp.sidx == "SalType" ? c.SalType.ToString() :
                                            gp.sidx == "PayAmount" ? c.PayAmount.ToString() :
                                             gp.sidx == "PayMonth" ? c.PayMonth.ToString() :

                                         gp.sidx == "Paydate" ? c.Paydate.ToString() :


                                    "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName, a.Salcode, a.SalType, a.PayAmount, a.PayMonth, a.Paydate != null ? Convert.ToString(a.Paydate) : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName, a.Salcode, a.SalType, a.PayAmount, a.PayMonth, a.Paydate != null ? Convert.ToString(a.Paydate) : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Salcode, a.SalType, a.PayAmount, a.PayMonth, a.Paydate != null ? Convert.ToString(a.Paydate) : "", a.Id }).ToList();
                    }
                    totalRecords = SalaryList.Count();
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
        //        IEnumerable<P2BGridData> SalaryList = null;
        //        List<P2BGridData> model = new List<P2BGridData>();
        //        P2BGridData view = null;

        //        //   IEnumerable<NoticePeriodProcess> Corporate = null;

        //        var empresig = db.EmployeeExit.Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
        //            .Include(e => e.SeperationProcessT.NoticePeriodProcess)
        //            .AsNoTracking().ToList();

        //        foreach (var z in empresig)
        //        {
        //            if (z.SeperationProcessT != null && z.SeperationProcessT.NoticePeriodProcess != null)
        //            {
        //                view = new P2BGridData()
        //                {
        //                    Id = z.SeperationProcessT.NoticePeriodProcess.Id,
        //                    Code = z.Employee.EmpCode,
        //                    Name = z.Employee.EmpName.FullNameFML,
        //                    LastWorkDateByComp = z.SeperationProcessT.NoticePeriodProcess.LastWorkDateByComp.Value.Date.ToShortDateString(),
        //                    LastWorkDateApproved = z.SeperationProcessT.NoticePeriodProcess.LastWorkDateApproved.Value.Date.ToShortDateString(),

        //                };
        //                model.Add(view);
        //            }
        //        }

        //        SalaryList = model;

        //        IEnumerable<P2BGridData> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = SalaryList;
        //            if (gp.searchOper.Equals("eq"))
        //            {

        //                //jsonData = IE.Where(e => (e.Narration.ToUpper().ToString().Contains(gp.searchString.ToUpper()))                             
        //                //      || (e.Id.ToString().Contains(gp.searchString))
        //                //      ).Select(a => new Object[] { a.Narration, a.Id }).ToList();
        //                jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
        //                     || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
        //                      || (e.LastWorkDateByComp != null ? e.LastWorkDateByComp.ToString().Contains(gp.searchString) : false)
        //                     || (e.LastWorkDateApproved != null ? e.LastWorkDateApproved.ToString().Contains(gp.searchString) : false)

        //                     || (e.Id.ToString().Contains(gp.searchString))
        //                     ).Select(a => new Object[] { a.Code, a.Name, a.LastWorkDateByComp != null ? Convert.ToString(a.LastWorkDateByComp) : "", a.LastWorkDateApproved != null ? Convert.ToString(a.LastWorkDateApproved) : "", a.Id }).ToList();



        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.LastWorkDateByComp != null ? Convert.ToString(a.LastWorkDateByComp) : "", a.LastWorkDateApproved != null ? Convert.ToString(a.LastWorkDateApproved) : "", a.Id }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = SalaryList;
        //            Func<P2BGridData, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
        //                                 gp.sidx == "Name" ? c.Name.ToString() :
        //                                 gp.sidx == "LastWorkDateByComp" ? c.LastWorkDateByComp.ToString() :
        //                                 gp.sidx == "LastWorkDateApproved" ? c.LastWorkDateApproved.ToString() :


        //                            "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.LastWorkDateByComp != null ? Convert.ToString(a.LastWorkDateByComp) : "", a.LastWorkDateApproved != null ? Convert.ToString(a.LastWorkDateApproved) : "", a.Id }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.LastWorkDateByComp != null ? Convert.ToString(a.LastWorkDateByComp) : "", a.LastWorkDateApproved != null ? Convert.ToString(a.LastWorkDateApproved) : "", a.Id }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.LastWorkDateByComp != null ? Convert.ToString(a.LastWorkDateByComp) : "", a.LastWorkDateApproved != null ? Convert.ToString(a.LastWorkDateApproved) : "", a.Id }).ToList();
        //            }
        //            totalRecords = SalaryList.Count();
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
        public class ProcType
        {
            public int Id { get; set; }
            public string Text { get; set; }
        };
        public ActionResult Polulate_ProcTypeChk(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    List<ProcType> a = new List<ProcType>();
                    if (data.ToString().Split('/')[0] == "03")
                    {
                        a = new List<ProcType>()
                    {
                        new ProcType() { Id = 0, Text = "Actual Investment & Actual Income" }
                    };
                    }
                    else
                    {
                        a = new List<ProcType>()
                    {
                        new ProcType() { Id = 0, Text = "Actual Investment & Actual Income" },
                        new ProcType() { Id = 1, Text = "Declare Investment & Projected Income" },
                        new ProcType() { Id = 2, Text = "Actual Investment & Projected Income" }
                    };
                    }

                    SelectList s = new SelectList(a, "Id", "Text");
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }


        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        public EmployeePayroll _returnEmployeePayrollOne(Int32 empid)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                EmployeePayroll oEmployeePayroll = db.EmployeePayroll
                                   .Include(e => e.Employee.EmpName)
                                   .Include(e => e.Employee.ServiceBookDates)
                                   .Where(e => e.Id == empid)
                                   .FirstOrDefault();
                return oEmployeePayroll;
            }
        }
        public ActionResult ChkIFManual(string forwardata, string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                List<int> ids = null;

                //string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string Emp = forwardata == "0" ? "" : forwardata;

                if (Emp != null && Emp != "0" && Emp != "" && Emp != "false")
                {
                    ids = one_ids(Emp);
                }
                else
                {
                    Msg.Add("Kindly select employee");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }


                var ItaxCheck = db.ITaxTransT
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Employee)
                    .AsNoTracking().Where(e => e.PayMonth == PayMonth && e.Mode.ToUpper() == "MAN" && ids.Contains(e.EmployeePayroll.Employee.Id)).Select(e => e.EmployeePayroll.Id).ToList();

                foreach (var EmpId in ItaxCheck)
                {
                    EmployeePayroll EmpPay = _returnEmployeePayrollOne(EmpId);


                    if ((EmpPay.Employee.ServiceBookDates.ServiceLastDate == null))
                    {
                        Msg.Add(EmpPay.Employee.FullDetails + ", Manual IncomeTax entry of this employee is entered ,");
                    }
                }
                if (Msg.Count() > 0)
                {
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return Json(new Utility.JsonReturnClass { success = true, responseText = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Create(FFSSettlementDetailT S, FormCollection form, String forwarddata) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string Emp = forwarddata == "0" ? "" : forwarddata;
                    string Lvhead = form["LvHead"] == "" ? "0" : Convert.ToString(form["LvHead"]);
                    string ProcTypeList = form["ProcTypeList"] == "" ? "" : form["ProcTypeList"];
                    string AmountList = "";
                    List<int> LvHead_ids = null;
                    if (Lvhead != null && Lvhead != "0")
                    {
                        LvHead_ids = Utility.StringIdsToListIds(Lvhead);
                    }



                    //  string Empstruct_drop = form["Empstruct_drop"] == "0" ? "" : form["Empstruct_drop"];
                    bool AutoIncomeTax = false;
                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }

                    List<int> ids = null;
                    if (Emp != "null" && Emp != "0" && Emp != "" && Emp != "false")
                    {
                        ids = one_ids(Emp);

                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    var GetProcessType = db.Lookup
                                         .Include(e => e.LookupValues)
                                          .Where(e => e.Code == "3030").FirstOrDefault();
                    if (GetProcessType == null)
                    {
                        Msg.Add("  Kindly Define in Lookup FFSETTLEMENT Process Type and code will 3030  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Boolean prtype = false;
                        var processtype = GetProcessType.LookupValues.ToList();
                        foreach (var item in processtype)
                        {
                            if (item.LookupVal.ToUpper() == "NOTICE PERIOD SALARY RECOVERY")
                            {
                                prtype = true;
                                S.ProcessType = db.LookupValue.Find(item.Id);
                            }

                        }

                        if (prtype == false)
                        {
                            Msg.Add(" Kindly Define in Lookup 'Notice Period Salary Recovery' under FFSETTLEMENT ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    string ProcessMonth = form["Create_Processmonth"] == "0 " ? "" : form["Create_Processmonth"];


                    string PaymentMonth = form["Create_Paymonth"] == "0" ? "" : form["Create_Paymonth"];
                    if (PaymentMonth == "")
                    {
                        Msg.Add(" Kindly Select Pay Month ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    string Paymentdate = form["Create_Paymentdate"] == "0" ? "" : form["Create_Paymentdate"];
                    if (Paymentdate == "")
                    {
                        Msg.Add(" Kindly Select Payment date ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    //  string Reason = form["Create_Reason"] == "0" ? "" : form["Create_Reason"];

                    // List<int> SalHead = null;
                    int SalaryHead = form["SalaryHeadlist"] == "0" ? 0 : Convert.ToInt32(form["SalaryHeadlist"]);
                    Employee OEmployee = null;

                    string fromdate = form["fromdate"] == "0 " ? "" : form["fromdate"];
                    string Todate = form["Todate"] == "0 " ? "" : form["Todate"];
                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {

                            OEmployee = db.Employee
                          .Include(e => e.EmpName)
                          .Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company)
                          .Include(e => e.FuncStruct).Include(e => e.PayStruct)
                          .Include(e => e.ServiceBookDates)
                          .Where(r => r.Id == i).SingleOrDefault();
                            EmployeePayroll OEmployeePayrollid = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == OEmployee.Id).FirstOrDefault();

                            
                            ///
                            var OEmpSalT = db.EmployeeExit.Where(e => e.Employee.Id == i)
                                  .Include(e => e.SeperationProcessT)
                                  .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                                  .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(x => x.ProcessType))
                                  .AsNoTracking()
                                  .SingleOrDefault();
                            if (OEmpSalT != null)
                            {

                                var processtypechk = OEmpSalT.SeperationProcessT.FFSSettlementDetailT.Select(x => x.ProcessType).ToList();
                                Boolean salprocessed = false;
                                if (processtypechk != null)
                                {
                                    foreach (var item in processtypechk)
                                    {
                                        if (item.LookupVal.ToUpper() == "NOTICE PERIOD SALARY RECOVERY")
                                        {
                                            salprocessed = true;
                                        }
                                    }
                                    if (salprocessed == true)
                                    {
                                        Msg.Add(OEmployee.EmpCode + " " + OEmployee.EmpName.FullNameFML + "Notice Period Salary Recovery processed you can not reprocess ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                        }

                    }


                    var salaryheadret = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == SalaryHead).FirstOrDefault();



                    EmployeePayroll OEmployeePayroll = null;
                    string EmpCode = null;
                    string EmpRel = null;




                    if (PaymentMonth != null && PaymentMonth != "")
                    {
                        S.PayMonth = Convert.ToDateTime(PaymentMonth);

                    }




                    DateTime Todaydate = DateTime.Now;

                    S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };



                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {
                            List<FFSSettlementDetailT> ObjFAT = new List<FFSSettlementDetailT>();
                            // Salary Process Start
                            DateTime mFromPeriod = Convert.ToDateTime(fromdate).Date;

                            DateTime mEndDate = Convert.ToDateTime(Todate).Date;
                            double SalAttendanceT_PayableDays = (mEndDate - mFromPeriod).TotalDays + 1;
                            DateTime mEndDatepayble = Convert.ToDateTime("01/" + Convert.ToDateTime(fromdate).ToString("MM/yyyy")).AddMonths(1).Date;
                            mEndDatepayble = mEndDatepayble.AddDays(-1).Date;

                            double SalAttendanceT_monthDays = mEndDatepayble.Day;
                            OEmployee = db.Employee
                        .Include(e => e.EmpName)
                        .Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company)
                        .Include(e => e.FuncStruct).Include(e => e.PayStruct)
                        .Include(e => e.ServiceBookDates)
                        .Where(r => r.Id == i).SingleOrDefault();
                            EmployeePayroll OEmployeePayrollid = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == OEmployee.Id).FirstOrDefault();


                            var QEmpSalStruct1 = db.EmpSalStruct
                                            .Include(e => e.EmpSalStructDetails)
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.ProcessType));

                            var EmpSalStructTotal = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == OEmployeePayrollid.Id).OrderByDescending(e => e.Id)
                                                        .FirstOrDefault();


                            // var EmpSalStructTotal = EmpSalStructList; //Total salary structure
                            var OEmpSalStruct = EmpSalStructTotal; //single salary structure

                            List<EmpSalStructDetails> OEmpsalhead = OEmpSalStruct.EmpSalStructDetails.Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                  && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                  && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "RESIGNRECOVERY"
                  )).OrderBy(e => e.SalaryHead.SeqNo).ToList();
                            if (OEmpsalhead.Count == 0)
                            {
                                Msg.Add(" Kindly Define RESIGNRECOVERY in salary structure.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            PayProcessGroup OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayrollPeriod).Include(e => e.PayFrequency).SingleOrDefault();
                            double LvDays = 0;
                            foreach (EmpSalStructDetails ca in OEmpsalhead)
                            {

                                if (ca.SalaryHead.OnAttend == true && ca.SalaryHead.OnLeave == true)
                                {
                                    //1040
                                    //Utility.DumpProcessStatus(LineNo: 1042);

                                    LvDays = SalaryGen.LvChk(ca.SalaryHead, OPayProcGrp, Convert.ToDateTime(fromdate).ToString("MM/yyyy"), OEmployeePayrollid.Id, OEmpSalStruct);
                                    //1043
                                    //Utility.DumpProcessStatus(LineNo: 1046);

                                    SalAttendanceT_PayableDays = SalAttendanceT_PayableDays - LvDays;
                                }
                                //1048
                                double CalAmount = 0;
                                if (OPayProcGrp != null)
                                {
                                    //Utility.DumpProcessStatus(LineNo: 1084);

                                    if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                                    {
                                        //Utility.DumpProcessStatus(LineNo: 1058);

                                        if (ca.SalaryHead.OnAttend == true)
                                        {
                                            //Utility.DumpProcessStatus(LineNo: 1062);
                                            CalAmount = (ca.Amount / 30) * SalAttendanceT_PayableDays;
                                        }
                                        else
                                        {
                                            //Utility.DumpProcessStatus(LineNo: 1067);
                                            CalAmount = ca.Amount;
                                        }

                                    }
                                    if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                                    {
                                        //Utility.DumpProcessStatus(LineNo: 1074);

                                        if (ca.SalaryHead.OnAttend == true)
                                        {
                                            //Utility.DumpProcessStatus(LineNo: 1078);

                                            CalAmount = (ca.Amount) - ((SalAttendanceT_monthDays - SalAttendanceT_PayableDays) / 30) * ca.Amount;
                                        }
                                        else
                                        {
                                            //Utility.DumpProcessStatus(LineNo: 1084);

                                            CalAmount = ca.Amount;
                                        }

                                    }
                                    if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                                    {
                                        //Utility.DumpProcessStatus(LineNo: 1092);
                                        if (ca.SalaryHead.OnAttend == true)
                                        {
                                            //Utility.DumpProcessStatus(LineNo: 1095);

                                            CalAmount = ca.Amount * ((SalAttendanceT_PayableDays) / (SalAttendanceT_monthDays));
                                        }
                                        else
                                        {
                                            //Utility.DumpProcessStatus(LineNo: 1101);
                                            CalAmount = ca.Amount;
                                        }
                                    }
                                }




                                CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount); //rounding function
                                //details save query

                                if (CalAmount != 0)
                                {

                                    var val = db.SalaryHead
                                    .Include(e => e.Type)
                                    .Include(e => e.ProcessType)
                                     .Include(e => e.SalHeadOperationType)
                                    .Where(e => e.Id == ca.SalaryHead.Id).FirstOrDefault();

                                    S.SalaryHead = val.Code;
                                    S.SalaryHeadDesc = val.Name;
                                    S.SalType = val.Type.LookupVal.ToUpper().ToString();
                                    FFSSettlementDetailT ObjFATnew = new FFSSettlementDetailT();
                                    {

                                        ObjFATnew.PayAmount = CalAmount;
                                        ObjFATnew.PayMonth = S.PayMonth;
                                        ObjFATnew.SalaryHead = S.SalaryHead;
                                        ObjFATnew.SalaryHeadDesc = S.SalaryHeadDesc;
                                        ObjFATnew.ProcessType = S.ProcessType;
                                        ObjFATnew.SalType = S.SalType;
                                        ObjFATnew.PayDate = Convert.ToDateTime(Paymentdate);
                                        ObjFATnew.IsPaid = false;
                                        ObjFATnew.DBTrack = S.DBTrack;

                                    }
                                    ObjFAT.Add(ObjFATnew);
                                }

                            }





                            // Salary Process End



                            using (TransactionScope ts = new TransactionScope())
                            {
                                try
                                {
                                    List<FFSSettlementDetailT> OFFSList = new List<FFSSettlementDetailT>();
                                    db.FFSSettlementDetailT.AddRange(ObjFAT);
                                    db.SaveChanges();
                                    OFFSList.AddRange(ObjFAT);
                                    var aa = db.EmployeeExit.Include(x => x.SeperationProcessT).Include(e => e.SeperationProcessT.FFSSettlementDetailT).Where(e => e.Employee.Id == i).FirstOrDefault();
                                    if (aa.SeperationProcessT == null)
                                    {
                                        SeperationProcessT OTEP = new SeperationProcessT()
                                        {
                                            FFSSettlementDetailT = OFFSList,
                                            DBTrack = S.DBTrack
                                        };


                                        db.SeperationProcessT.Add(OTEP);

                                        db.SaveChanges();

                                        aa.SeperationProcessT = OTEP;
                                        db.EmployeeExit.Attach(aa);
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                    }
                                    else
                                    {
                                        OFFSList.AddRange(aa.SeperationProcessT.FFSSettlementDetailT);
                                        aa.SeperationProcessT.FFSSettlementDetailT = OFFSList;
                                        db.EmployeeExit.Attach(aa);
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    ts.Complete();

                                }
                                catch (Exception ex)
                                {
                                    //   List<string> Msg = new List<string>();
                                    Msg.Add(ex.Message);
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
                                    //return Json(new { sucess = false, Msg }, JsonRequestBehavior.AllowGet);
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }


                            }

                        }
                        // List<string> Msgs = new List<string>();
                        Msg.Add("Data Saved successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { success = true, responseText = "Data Saved Successfully...", JsonRequestBehavior.AllowGet });
                    }
                    // List<string> Msg = new List<string>();
                    Msg.Add("  Unable to create...  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { success = false, responseText = "Unable to create...", JsonRequestBehavior.AllowGet });
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



        public ActionResult getResigndate(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                ResignPerioddt ResignPerioddt = new ResignPerioddt();
                int Id = int.Parse(data);

                if (Id != 0)
                {

                    var query = db.EmployeeExit.Where(e => e.Employee.Id == Id).Include(e => e.SeperationProcessT)
                     .Include(e => e.SeperationProcessT.NoticePeriodProcess)
                      .AsEnumerable()
                      .Select(e => new
                      {

                          LastWorkDateByComp = e.SeperationProcessT.NoticePeriodProcess.LastWorkDateByComp.Value.ToShortDateString(),
                          LastWorkDateApproved = e.SeperationProcessT.NoticePeriodProcess.LastWorkDateApproved.Value.ToShortDateString(),
                          Waive = e.SeperationProcessT.NoticePeriodProcess.WaiveDays.ToString()
                      })

                      .SingleOrDefault();

                    ResignPerioddt = new ResignPerioddt()
                    {
                        Fromperiod = query.LastWorkDateByComp.ToString(),
                        Toperiod = query.LastWorkDateApproved.ToString(),
                        Waive = query.Waive.ToString(),

                    };

                    return Json(new { ResignPerioddt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }

            }
        }
        public class ResignPerioddt
        {

            public string Fromperiod { get; set; }
            public string Toperiod { get; set; }
            public string Waive { get; set; }

        }
        public class Perioddt
        {

            public string Periodfrom { get; set; }

        }

        public ActionResult getSalaryProcessdate(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Perioddt Perioddtt = new Perioddt();
                DateTime FromPerioddt;
                int Id = int.Parse(data);
                if (Id != 0)
                {


                    int OEmployeePayrollId
                             = db.EmployeeLeave.Where(e => e.Employee.Id == Id).SingleOrDefault().Id;

                    var query = db.SalaryT.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId).OrderByDescending(e => e.Id)
                        .Select(e => new
                        {
                            Fromperiod = e.PayMonth.ToString()
                        })
                        .FirstOrDefault();
                    FromPerioddt = Convert.ToDateTime("01/" + query.Fromperiod).AddMonths(1).Date;

                    Perioddtt = new Perioddt()
                    {
                        Periodfrom = FromPerioddt.ToShortDateString(),

                    };

                    return Json(new { Perioddtt }, JsonRequestBehavior.AllowGet);
                    //   return Json(query, JsonRequestBehavior.AllowGet);
                }
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }





        public class P2BCrGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }


        }
        public ActionResult LoadEmp(P2BGrid_Parameters gp)
        {
            DataBaseContext db = new DataBaseContext();

            List<int> lvheadids = null;



            try
            {
                DateTime? dt = null;
                string monthyr = "";
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<P2BCrGridData> EmpList = null;
                List<P2BCrGridData> model = new List<P2BCrGridData>();
                P2BCrGridData view = null;

                List<Employee> EmpList1 = new List<Employee>();

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


                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var CompanyPayroll_Id = db.CompanyPayroll.Where(e => e.Company.Id == compid).SingleOrDefault();
                CompanyExit CompanyExit = new CompanyExit();
                CompanyExit = db.CompanyExit.Include(e => e.Company).Where(e => e.Company.Id == compid).FirstOrDefault();

                var empdata = db.CompanyExit.Where(e => e.Company.Id == compid)
                    .Include(e => e.EmployeeExit)
                    .Include(e => e.EmployeeExit.Select(a => a.Employee))
                    .Include(e => e.EmployeeExit.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeeExit.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeeExit.Select(a => a.SeperationProcessT))
                    .Include(e => e.EmployeeExit.Select(a => a.SeperationProcessT.NoticePeriodProcess))
                    .AsNoTracking().OrderBy(e => e.Id)
                   .SingleOrDefault();

                var emp = empdata.EmployeeExit.ToList();

                foreach (var z in emp)
                {
                    if (z.SeperationProcessT != null && z.SeperationProcessT.NoticePeriodProcess != null)
                    {
                        if (z.SeperationProcessT.NoticePeriodProcess.WaiveDays == 0 && z.SeperationProcessT.NoticePeriodProcess.LastWorkDateApproved < z.SeperationProcessT.NoticePeriodProcess.LastWorkDateByComp)
                        {


                            var EmployeeSeperationStruct = db.EmployeeSeperationStruct
                                           .Include(e => e.EmployeeSeperationStructDetails)
                                           .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationMaster))
                                            .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula))
                                            .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula.ExitProcess_Config_Policy))
                                           .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment))
                                           .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment.PayScaleAgreement))
                                           .Where(e => e.EmployeeExit.Id == z.Id && e.EndDate == null).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();


                            view = new P2BCrGridData()
                            {
                                Id = z.Employee.Id,
                                Code = z.Employee.EmpCode,
                                Name = z.Employee.EmpName.FullNameFML
                            };
                            // Retire employee
                            if (EmployeeSeperationStruct != null)
                            {
                                model.Add(view);

                            }

                        }
                    }
                }




                EmpList = model;

                IEnumerable<P2BCrGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                                || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))

                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpList;
                    Func<P2BCrGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "EmpCode" ? c.Code.ToString() :
                                         gp.sidx == "EmpName" ? c.Name.ToString() : ""


                        );
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.Id }).ToList();
                    }
                    totalRecords = EmpList.Count();
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