using EssPortal.App_Start;
using EssPortal.Models;
using EssPortal.Security;
using P2b.Global;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Leave;
using System.Data;
using System.Text;
using P2B.SERVICES.Interface;
using P2B.SERVICES.Factory;
using System.IO;

namespace EssPortal.Controllers
{
    public class FunctAttendanceTController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/FunctAttendanceT/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_FunctAttendanceTView.cshtml");
        }

        readonly IP2BINI p2BINI;
        readonly FuncAllowanceSettings FuncAllowanceSettings;

        private readonly Type Type;

        public FunctAttendanceTController()
        {
            p2BINI = P2BINI.RegisterSettings();
            FuncAllowanceSettings = new FuncAllowanceSettings(p2BINI.GetSectionValues("FuncAllowanceSettings"));

            Type = typeof(FunctAttendanceTController);

        }

        public ActionResult DateChecking()
        {
            if (FuncAllowanceSettings.CCode != null || FuncAllowanceSettings.DFromDate != null || FuncAllowanceSettings.DToDate != null)
            {
                int currentDay = DateTime.Now.Day;
                int fromDay = int.Parse(FuncAllowanceSettings.DFromDate);
                int toDay = int.Parse(FuncAllowanceSettings.DToDate);

                var comcode = FuncAllowanceSettings.CCode;

                return Json(new Object[] { currentDay, fromDay, toDay, comcode, JsonRequestBehavior.AllowGet });
            }

            return null;
        }


        public ActionResult PopulateTransactionDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.PayProcessGroup.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDropDownStructureList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var serialize = new JavaScriptSerializer();

                var json = SessionManager.EmpId;

                var Processmonth = data;

                SelectList k = null;
                //foreach (var ca in json)
                //{
                int id = Convert.ToInt32(json);
                //  var a = db.EmployeePayroll.Include(e => e.EmpSalStruct).Where(e => e.Employee.Id == id).ToList();
                var a = (from s in db.EmployeePayroll.Include(e => e.EmpSalStruct)
                                 .Where(e => e.Employee.Id == id)
                                .ToList()
                         from p in s.EmpSalStruct
                         select new
                         {
                             Id = p.Id,
                             EffectiveDate = p.EffectiveDate.Value.ToString("MM/yyyy"),
                             Effectivedate_Enddate = p.FullDetails
                         }).Where(e => e.EffectiveDate == Processmonth).Distinct().OrderBy(e => e.EffectiveDate);
                int[] Selected = a.Select(e => e.Id).ToArray();
                k = new SelectList(a, "Id", "Effectivedate_Enddate", Selected[0]);
                //  }

                return Json(k, JsonRequestBehavior.AllowGet);


            }

        }
        public ActionResult GetSalaryHeadDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.SalaryHead.Include(q => q.Frequency).Where(e => e.Frequency.LookupVal.ToUpper() == "HOURLY" || e.Frequency.LookupVal.ToUpper() == "DAILY").ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.SalaryHead.Where(e => e.Frequency.LookupVal.ToUpper() == "HOURLY" || e.Frequency.LookupVal.ToUpper() == "DAILY").ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetStructureDates(string data, string data2)
        {
            //var serialize = new JavaScriptSerializer();

            var json = SessionManager.EmpId;
            //serialize.DeserializeObject(data2);

            var Processmonth = data;
            String FromDate = null;
            String ToDate = null;
            using (DataBaseContext db = new DataBaseContext())
            {
                //foreach (var ca in json)
                //{
                int id = Convert.ToInt32(json);
                var a = (from s in db.EmployeePayroll.Include(e => e.EmpSalStruct)
                                 .Where(e => e.Employee.Id == id)
                                .ToList()
                         from p in s.EmpSalStruct
                         select new
                         {

                             Id = p.Id,
                             EffectiveDate = p.EffectiveDate.Value.ToString("MM/yyyy"),
                             FromDate = p.EffectiveDate.Value.ToString("dd/MM/yyyy"),
                             Enddate = p.EndDate != null ? p.EndDate.Value.ToString("dd/MM/yyyy") : null
                         }).Where(e => e.EffectiveDate == Processmonth).Distinct().OrderBy(e => e.EffectiveDate).LastOrDefault();
                if (a != null)
                {
                    FromDate = a.FromDate.ToString();
                    if (a.Enddate != null)
                        ToDate = a.Enddate.ToString();
                }

                //}
                var jsondata = new
                {
                    FromDate = FromDate,
                    ToDate = ToDate
                };
                return Json(new { data = jsondata }, JsonRequestBehavior.AllowGet);
            }
        }
        public class P2BCrGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.SalaryHead.Where(e => e.Frequency.LookupVal.ToUpper() == "HOURLY" || e.Frequency.LookupVal.ToUpper() == "DAILY").ToList();


                // var qurey = db.SalaryHead.Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH").ToList();

                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }


        public class EditData
        {
            public int Id { get; set; }
            public string EmpName { get; set; }
            public string EmpCode { get; set; }
            public string PayMonth { get; set; }
            public string ProcessMonth { get; set; }
            public bool Editable { get; set; }
            public int HourDays { get; set; }
            public string Reason { get; set; }
            public string Salcode { get; set; }
        }
        public class DeserializeClass
        {
            public String Id { get; set; }
            public int HourDays { get; set; }
            public string EmpCode { get; set; }
            public string Reason { get; set; }
        }
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        public ActionResult Create(FunctAttendanceT S, FormCollection form, String forwarddata)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Emp = SessionManager.EmpId;
                    string PayProcessgropp = form["payscaleagreement_drop"] == "0" ? "" : form["payscaleagreement_drop"];
                    string ProcessMonth = form["Create_Processmonth"] == "0 " ? "" : form["Create_Processmonth"];
                    string PayMonth = form["Create_Paymonth"] == "0" ? "" : form["Create_Paymonth"];
                    string HourDays = form["Create_HourDays"] == "0" ? "" : form["Create_HourDays"];
                    string Reason = form["Create_Reason"] == "0" ? "" : form["Create_Reason"];
                    string SalaryHead = form["SalaryHeadlist"] == "0" ? "" : form["SalaryHeadlist"];
                    if (SalaryHead == null)
                    {
                        return Json(new { status = false, responseText = "Please Select The Salary Head......" }, JsonRequestBehavior.AllowGet);
                    }
                    string Empstruct_drop = form["Empstruct_drop"] == "0" ? "" : form["Empstruct_drop"];
                    string fromdate = form["fromdate"] == "0 " ? "" : form["fromdate"];
                    string Todate = form["Todate"] == "0 " ? "" : form["Todate"];
                    string ReqDate = form["ReqDate"] == "0 " ? "" : form["ReqDate"];

                    //int currentDay = DateTime.Now.Day;
                    //int fromDay = int.Parse(FuncAllowanceSettings.DFromDate);
                    //int toDay = int.Parse(FuncAllowanceSettings.DToDate);

                    //if (currentDay < fromDay || currentDay > toDay)
                    //{
                    //    Msg.Add("Currently You Can not apply for Functional Allowance");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    List<String> promonthsplits = ProcessMonth.Split('/').Select(e => e).ToList();
                    int monch = Convert.ToInt32(promonthsplits[0]);
                    int yerch = Convert.ToInt32(promonthsplits[1]);
                    double days = DateTime.DaysInMonth(yerch, monch);
                    double HourDaysch = Convert.ToDouble(HourDays);

                    var EmpVariable = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location)
                                    .Where(r => r.Id == 1).SingleOrDefault();

                    if (Empstruct_drop != null && Empstruct_drop != "" && Empstruct_drop != "-Select-")
                    {
                        var value = db.EmpSalStruct.Find(int.Parse(Empstruct_drop));
                        S.EmpSalStruct = value;

                    }
                    if (S.ToDate.Value < S.FromDate.Value)
                    {
                        return Json(new { status = false, responseText = "To Date should be more than start date" }, JsonRequestBehavior.AllowGet);
                    }
                    if (ReqDate != null)
                    {
                        S.ReqDate = Convert.ToDateTime(ReqDate);
                    }
                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    string EmpCode = null;
                    string EmpRel = null;
                    List<int> SalHead = null;

                    if (SalaryHead != null && SalaryHead != "")
                    {
                        SalHead = one_ids(SalaryHead);
                    }
                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {
                            foreach (int SH in SalHead)
                            {
                                var fall = db.SalaryHead.Include(e => e.Frequency).Where(e => e.Id == SH).FirstOrDefault();
                                if (fall.Frequency.LookupVal.ToUpper() == "DAILY")
                                {
                                    if (HourDaysch > days)
                                    {
                                        Msg.Add("Check Hour Days");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                var val = db.SalaryHead.Find(SH);

                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                           .Where(r => r.Id == i).SingleOrDefault();

                                OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == i).SingleOrDefault();

                                var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id)
                                    .Include(e => e.FunctAttendanceT)
                                    .Include(e => e.FunctAttendanceT.Select(a => a.SalaryHead))
                                    .SingleOrDefault();
                                var oemp = OEmpSalT.FunctAttendanceT.Where(e => e.isCancel == false && e.TrReject == false).ToList();

                                if (oemp.Count() > 0)
                                {
                                    if ((oemp.Where(e => e.FromDate <= S.FromDate && e.ToDate <= S.ToDate).Count() != 0 ||
                                        oemp.Where(e => e.FromDate <= S.FromDate && e.ToDate >= S.ToDate).Count() != 0) && oemp.Where(e => e.ToDate >= S.FromDate).Count() != 0
                                      && oemp.Where(e => e.SalaryHead.Id == SH && ProcessMonth.Contains(e.ProcessMonth)).Count() != 0)
                                    {
                                        if (EmpCode == null || EmpCode == "")
                                        {
                                            EmpCode = OEmployee.EmpCode;
                                        }
                                        else
                                        {
                                            EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                                        }
                                    }
                                }
                                //var EmpSalT = OEmpSalT.FunctAttendanceT != null ? OEmpSalT.FunctAttendanceT.Where(e => e.SalaryHead.Id == SH) : null;
                                //if (EmpSalT != null && EmpSalT.Count() > 0)
                                //{
                                //    if (EmpCode == null || EmpCode == "")
                                //    {
                                //        EmpCode = OEmployee.EmpCode;
                                //    }
                                //    else
                                //    {
                                //        EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                                //    }
                                //}

                                var OEmpSalRelT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.FunctAttendanceT).Include(e => e.SalaryT).SingleOrDefault();
                                var EmpSalRelT = OEmpSalRelT.SalaryT != null ? OEmpSalRelT.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate != null) : null;

                                if (EmpSalRelT != null && EmpSalRelT.Count() > 0)
                                {
                                    if (EmpRel == null || EmpRel == "")
                                    {
                                        EmpRel = OEmployee.EmpCode;
                                    }
                                    else
                                    {
                                        EmpRel = EmpRel + ", " + OEmployee.EmpCode;
                                    }
                                }
                            }
                        }
                    }
                    if (EmpCode != null)
                    {
                        return Json(new { status = false, responseText = "FunctAttendance already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                    }

                    if (EmpRel != null)
                    {
                        return Json(new { status = true, responseText = "Salary released for employee " + EmpRel + ". You can't change attendance now." }, JsonRequestBehavior.AllowGet);
                    }
                    if (PayMonth != null && PayMonth != "")
                    {
                        S.PayMonth = PayMonth;
                        int mon = int.Parse(PayMonth.Split('/')[0].StartsWith("0") == true ? PayMonth.Split('/')[0].Remove(0, 1) : PayMonth.Split('/')[0]);
                        int DaysInMonth = System.DateTime.DaysInMonth(int.Parse(PayMonth.Split('/')[1]), mon);
                    }

                    if (ProcessMonth != null && ProcessMonth != "")
                    {
                        S.ProcessMonth = ProcessMonth;
                    }
                    if (fromdate != null && fromdate != "")
                    {
                        var value = Convert.ToDateTime(fromdate);
                        S.FromDate = value;
                    }
                    if (Todate != null && Todate != "")
                    {
                        var value = Convert.ToDateTime(Todate);
                        S.ToDate = value;
                    }
                    if (HourDays != null && HourDays != "")
                    {
                        var val = Convert.ToDouble(HourDays);
                        S.HourDays = val;
                    }
                    if (Reason != null && Reason != "")
                    {
                        var val = Reason;
                        S.Reason = val;
                    }


                    S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                    FunctAllWFDetails oLvWFDetails = new FunctAllWFDetails
                    {
                        WFStatus = 0,
                        Comments = S.Reason,
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };
                    List<FunctAllWFDetails> oLvWFDetails_List = new List<FunctAllWFDetails>();
                    oLvWFDetails_List.Add(oLvWFDetails);

                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {
                            foreach (int SH in SalHead)
                            {
                                var val = db.SalaryHead.Find(SH);
                                S.SalaryHead = val;
                                FunctAttendanceT ObjFAT = new FunctAttendanceT();
                                {
                                    ObjFAT.PayMonth = S.PayMonth;
                                    ObjFAT.HourDays = S.HourDays;

                                    ObjFAT.ProcessMonth = S.ProcessMonth;
                                    ObjFAT.SalaryHead = S.SalaryHead;
                                    ObjFAT.Reason = S.Reason;

                                    ObjFAT.FromDate = S.FromDate;
                                    ObjFAT.ToDate = S.ToDate;
                                    ObjFAT.DBTrack = S.DBTrack;
                                    ObjFAT.ReqDate = S.ReqDate;
                                    ObjFAT.InputMethod = 1;//apply through ess
                                    ObjFAT.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "0").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();//db.LookupValue.Where(e => e.LookupVal == "0").SingleOrDefault();
                                    ObjFAT.FunctAllWFDetails = oLvWFDetails_List;

                                }
                                int Id = int.Parse(Empstruct_drop);

                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                            .Where(r => r.Id == i).SingleOrDefault();

                                OEmployeePayroll
                                = db.EmployeePayroll.Include(e => e.EmpSalStruct)
                              .Where(e => e.Employee.Id == i).SingleOrDefault();

                                ObjFAT.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);

                                ObjFAT.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);


                                ObjFAT.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id == null ? 0 : OEmployee.PayStruct.Id);

                                var k = db.EmpSalStruct.Where(e => e.Id == Id).Select(e => new
                                {
                                    EffectiveDate = e.EffectiveDate,
                                    EndDate = e.EndDate,
                                    EffectiveDate_EndDate = e.EffectiveDate != null ? e.EffectiveDate.ToString() : "" + e.EndDate != null ? e.EndDate.ToString() : ""
                                }).SingleOrDefault();
                                var Q = db.EmployeePayroll.Where(e => e.Employee.Id == i).Select(e => e.EmpSalStruct.Where(r => r.EffectiveDate == k.EffectiveDate && r.EndDate == k.EndDate)).SingleOrDefault();
                                var empsal_Id = Q.Select(r => r.Id).SingleOrDefault();
                                ObjFAT.EmpSalStruct = db.EmpSalStruct.Find(empsal_Id);


                                using (TransactionScope ts = new TransactionScope())
                                {
                                    try
                                    {
                                        db.FunctAttendanceT.Add(ObjFAT);
                                        db.SaveChanges();
                                        List<FunctAttendanceT> OFAT = new List<FunctAttendanceT>();
                                        OFAT.Add(db.FunctAttendanceT.Find(ObjFAT.Id));

                                        if (OEmployeePayroll == null)
                                        {
                                            EmployeePayroll OTEP = new EmployeePayroll()
                                            {
                                                Employee = db.Employee.Find(OEmployee.Id),
                                                FunctAttendanceT = OFAT,
                                                DBTrack = S.DBTrack

                                            };
                                            db.EmployeePayroll.Add(OTEP);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                            OFAT.AddRange(aa.FunctAttendanceT);
                                            aa.FunctAttendanceT = OFAT;
                                            //OEmployeePayroll.DBTrack = dbt;

                                            db.EmployeePayroll.Attach(aa);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                        }
                                        ts.Complete();

                                    }
                                    catch (Exception ex)
                                    {
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
                                        return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }


                                }
                            }
                        }
                        Msg.Add("Data Saved successfully");
                        return Json(new { status = true, responseText = "Data Saved successfully" }, JsonRequestBehavior.AllowGet);
                    }
                    Msg.Add("  Unable to create...  ");
                    return Json(new { status = false, responseText = "Unable to create..." }, JsonRequestBehavior.AllowGet);
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

        public class GetItInvestmentClass
        {
            public string ItinvestmentId { get; set; }
            public string value { get; set; }
            public string Status { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }


        //public ActionResult GetMyFunctAttendanceT()
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var Emp = Convert.ToInt32(SessionManager.EmpId);
        //        var qurey = db.EmployeePayroll
        //            .Include(e => e.FunctAttendanceT)
        //            .Include(e => e.FunctAttendanceT.Select(a => a.WFStatus))
        //            .Where(e => e.Employee.Id == Emp)
        //            .SingleOrDefault();

        //        var ListreturnDataClass = new List<ELMSController.AttachDataClass>();
        //        if (qurey != null && qurey.FunctAttendanceT != null && qurey.FunctAttendanceT.Count() > 0)
        //        {
        //            foreach (var item in qurey.FunctAttendanceT)
        //            {
        //                var InvestmentDate = item.ReqDate != null ? item.ReqDate.Value.ToShortDateString() : null;
        //                //var LoanAdvanceHead = item.LoanAdvanceHead != null ? item.LoanAdvanceHead.FullDetails : null;
        //                //var Narration = item.Narration != null ? item.Narration : null;
        //                //var ITSection = item.ITSection != null ? item.ITSection.FullDetails : null;
        //                //var ITSubInvestmentPayment_m = item.ITSubInvestmentPayment.Count > 0 ? item.ITSubInvestmentPayment.Select(e => e.ActualInvestment).ToArray() : null;
        //                //var ITSubInvestmentPayment = ITSubInvestmentPayment_m != null ? string.Join(",", ITSubInvestmentPayment_m) : null;
        //                ListreturnDataClass.Add(new ELMSController.AttachDataClass
        //                {
        //                    ItinvestmentId = item.Id,
        //                    val =
        //                    "ReqDate :" + InvestmentDate + " " +
        //                    " Status :" + Utility.GetStatusName().Where(e => item.WFStatus != null && e.Key == item.WFStatus.LookupVal).Select(e => e.Value).SingleOrDefault(),
        //                    status = item.WFStatus != null && new[] { "2", "4" }.Contains(item.WFStatus.LookupVal) == true ? "0" : "1",
        //                });
        //            }
        //        }
        //        if (ListreturnDataClass != null && ListreturnDataClass.Count > 0)
        //        {
        //            return Json(new { status = true, data = ListreturnDataClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { status = false, data = ListreturnDataClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        //public ActionResult GetMyFunctAttendanceT()
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }
        //        var Emp = Convert.ToInt32(SessionManager.EmpId);
        //        var db_data = db.EmployeePayroll
        //            .Include(e => e.FunctAttendanceT)
        //            .Include(e => e.FunctAttendanceT.Select(a => a.WFStatus))
        //            .Where(e => e.Employee.Id == Emp)
        //            .SingleOrDefault();

        //        List<GetItInvestmentClass> returndata = new List<GetItInvestmentClass>();
        //        returndata.Add(new GetItInvestmentClass
        //        {
        //            ItinvestmentId = "Id",
        //            value = "Details",
        //            Status = "Status",
        //        });

        //        if (db_data != null && db_data.FunctAttendanceT != null && db_data.FunctAttendanceT.Count() > 0)
        //        {
        //            foreach (var item in db_data.FunctAttendanceT)
        //            {
        //                var InvestmentDate = item.ReqDate != null ? item.ReqDate.Value.ToShortDateString() : null;

        //                returndata.Add(new GetItInvestmentClass
        //                {
        //                    RowData = new ChildGetLvNewReqClass
        //                    {
        //                        LvNewReq = item.Id.ToString(),
        //                        EmpLVid = db_data.Id.ToString(),
        //                        IsClose = "",
        //                        Status = "",
        //                        LvHead_Id = "",
        //                    },
        //                    ItinvestmentId = item.Id.ToString(),
        //                    value = "InvestmentDate :" + InvestmentDate + " " +
        //                    " Status :" + Utility.GetStatusName().Where(e => item.WFStatus != null && e.Key == item.WFStatus.LookupVal).Select(e => e.Value).SingleOrDefault(),
        //                    Status = item.WFStatus != null && new[] { "2", "4" }.Contains(item.WFStatus.LookupVal) == true ? "0" : "1",

        //                });
        //            }

        //            return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        public class GetFunctionAttendanceClass
        {

            public string Id { get; set; }
            public string PayMonth { get; set; }
            //  public string ProcessMonth { get; set; }
            //  public string FromDate { get; set; }
            //  public string ToDate { get; set; }
            public string ReqDate { get; set; }
            public string SalaryHead { get; set; }
            public string HourDays { get; set; }
            //public string PayMonth { get; set; }
            public string Status { get; set; }


            public ChildGetLvNewReqClass RowData { get; set; }
        }

        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }
        }

        public ActionResult GetMyFunctAttendanceT()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                int Emp = Convert.ToInt32(SessionManager.EmpId);
                int OEmployee = db.Employee.AsNoTracking().Where(r => r.Id == Emp).Select(q => q.Id).SingleOrDefault();

                var db_data = db.EmployeePayroll
                    .Include(e => e.FunctAttendanceT)
                    .Include(e => e.FunctAttendanceT.Select(q => q.SalaryHead))
                     .Include(e => e.FunctAttendanceT.Select(a => a.FunctAllWFDetails))
                    .Include(e => e.FunctAttendanceT.Select(a => a.WFStatus))
                    .Where(e => e.Employee.Id == Emp)
                    .SingleOrDefault();

                List<GetFunctionAttendanceClass> returndata = new List<GetFunctionAttendanceClass>();
                returndata.Add(new GetFunctionAttendanceClass
                {

                    Id = "Id",
                    PayMonth = " Pay Month",
                    SalaryHead = " Sal Head",
                    HourDays = " HourDays",
                    // ProcessMonth = "Process Month",
                    //    FromDate = "FromDate",
                    //    ToDate = "ToDate",
                    Status = " Status",

                });

                if (db_data != null && db_data.FunctAttendanceT.Count() > 0)
                {
                    foreach (var item in db_data.FunctAttendanceT.OrderByDescending(e => e.Id))
                    {
                        var Status = "--";
                        if (item.InputMethod == 1 && item.FunctAllWFDetails.Count > 0)
                        {
                            Status = Utility.GetStatusName().Where(e => e.Key == item.FunctAllWFDetails.LastOrDefault().WFStatus.ToString())
                            .Select(e => e.Value).SingleOrDefault();
                        }
                        if (item.InputMethod == 0)
                        {
                            Status = "Approved By HRM (M)";
                        }
                        returndata.Add(new GetFunctionAttendanceClass
                        {
                            RowData = new ChildGetLvNewReqClass
                            {
                                LvNewReq = item.Id.ToString(),
                                EmpLVid = Emp.ToString(),
                                IsClose = "",
                                Status = Status,
                                LvHead_Id = "",
                            },
                            Id = item.Id.ToString(),
                            PayMonth = item.PayMonth.ToString(),
                            SalaryHead = item.SalaryHead != null ? item.SalaryHead.Name.ToString() : "",
                            HourDays = item.HourDays.ToString(),
                            Status = Status
                            // ProcessMonth = item.ProcessMonth.ToString(),
                            // Reason  =item.Reason,
                            // FromDate = item.FromDate.Value.ToShortDateString(),
                            // ToDate = item.ToDate.Value.ToShortDateString(),
                            // ReqDate = item.ReqDate.Value.ToShortDateString(),

                            //     Status = item.WFStatus != null && new[] { "2", "4" }.Contains(item.WFStatus.LookupVal) == true ? "0" : "1",

                        });
                    }

                   // var result = returndata.OrderByDescending(e => e.Id);

                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = returndata }, JsonRequestBehavior.AllowGet);
                }
            }
        }




        public ActionResult Create_CancelReq(FunctAttendanceT c, FormCollection form, String data) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    string LvCancellist = form["IsCancel"] == "0" ? "" : form["IsCancel"];
                    string LvId = form["FunctionAttId"] == "0" ? "" : form["FunctionAttId"];
                    string Reason = form["ReasonIsCancel"] == "0" ? "" : form["ReasonIsCancel"];
                    var LvCancelchk = false;
                    var LeaveId = 0;
                    if (LvId != null)
                    {
                        LeaveId = Convert.ToInt32(LvId);
                    }
                    else
                    {
                        return Json(new { status = true, responseText = "Try Again" }, JsonRequestBehavior.AllowGet);
                    }

                    if (LvCancellist != null)
                    {
                        LvCancelchk = Convert.ToBoolean(LvCancellist);
                        if (LvCancelchk == false)
                        {
                            return Json(new { status = true, responseText = "Set cancel True" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { status = true, responseText = "Apply leave cancel" }, JsonRequestBehavior.AllowGet);
                    }

                    int EmpId = Convert.ToInt32(SessionManager.EmpId);


                    int lvnewreqid = Convert.ToInt32(data);

                    var query = db.FunctAttendanceT
                    .Include(e => e.FunctAllWFDetails)
                    .Where(e => e.Id == lvnewreqid).FirstOrDefault();

                    FunctAllWFDetails oFunctAllWFDetails = new FunctAllWFDetails
                    {
                        WFStatus = 6,
                        Comments = Reason,
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };
                    List<FunctAllWFDetails> oFuncAllWFDetails_List = new List<FunctAllWFDetails>();
                    if (query.FunctAllWFDetails.Count() > 0)
                    {
                        oFuncAllWFDetails_List.AddRange(query.FunctAllWFDetails);
                    }
                    oFuncAllWFDetails_List.Add(oFunctAllWFDetails);

                    query.isCancel = true;
                    query.TrClosed = true;
                    query.FunctAllWFDetails = oFuncAllWFDetails_List;

                    db.FunctAttendanceT.Attach(query);
                    db.Entry(query).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { status = true, responseText = "Record updated successfully.." }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { status = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { status = true, responseText = "Record updated successfully.." }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetMyFunctAttendanceTData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);
                var WfStatus = new List<Int32>();
                var SanctionStatus = new List<Int32>();
                var ApporvalStatus = new List<Int32>();
                if (authority.ToUpper() == "SANCTION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "APPROVAL")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "MYSELF")
                {
                    SanctionStatus.Add(1);
                    SanctionStatus.Add(2);

                    ApporvalStatus.Add(3);
                    ApporvalStatus.Add(4);
                }
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var status = ids.Count > 0 ? ids[2] : null;
                var qurey = db.FunctAttendanceT.Where(e => e.Id == id).SingleOrDefault();
                var list_salhead = db.FunctAttendanceT.Include(e => e.SalaryHead).Where(e =>
                    e.FromDate == qurey.FromDate &&
                    e.HourDays == qurey.HourDays &&
                    e.ToDate == e.ToDate &&
                    e.ReqDate == qurey.ReqDate &&
                    e.Reason == qurey.Reason)
                    .Select(e => e.SalaryHead.FullDetails).ToList();
                var Arr_salhead_list = list_salhead.ToArray();
                var salheadlist = Arr_salhead_list != null ? string.Join(",", Arr_salhead_list) : null;
                var listOfObject = db.FunctAttendanceT
                    .Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Location)
                    .Include(e => e.GeoStruct.Location.LocationObj)
                    .Include(e => e.GeoStruct.Location.BusinessCategory)
                    .Include(e => e.PayStruct)
                    .Include(e => e.PayStruct.Grade)
                    .Include(e => e.FuncStruct)
                    .Include(e => e.FuncStruct.Job)
                    .Include(e => e.EmployeePayroll.Employee.EmpName)
                    .Include(e => e.FunctAllWFDetails)
                    .Include(e => e.PayProcessGroup)
                    .Include(e => e.WFStatus)
                    .Include(e => e.SalaryHead)
                .Where(e => e.Id == id).AsEnumerable().Select
                (e => new
                {
                    EmployeeName = e.EmployeePayroll.Employee.EmpCode + " " + e.EmployeePayroll.Employee.EmpName.FullNameFML,
                    Status = status,
                    HourDays = e.HourDays,
                    PayMonth = e.PayMonth,
                    ProcessMonth = e.ProcessMonth,
                    ReqDate = e.ReqDate != null ? e.ReqDate.Value.ToShortDateString() : null,
                    Narration = e.Reason,
                    Location = e.GeoStruct != null && e.GeoStruct.Location != null && e.GeoStruct.Location.LocationObj != null ? e.GeoStruct.Location.LocationObj.LocDesc : "",
                    CategoryWise = e.GeoStruct != null && e.GeoStruct.Location != null && e.GeoStruct.Location.BusinessCategory != null ? e.GeoStruct.Location.BusinessCategory.LookupVal : "",
                    Grade = e.PayStruct != null && e.PayStruct.Grade != null ? e.PayStruct.Grade.Name : "",
                    Designation = e.FuncStruct != null && e.FuncStruct.Job != null ? e.FuncStruct.Job.Name : "",
                    FromDate = e.FromDate != null ? e.FromDate.Value.ToShortDateString() : null,
                    ToDate = e.ToDate != null ? e.ToDate.Value.ToShortDateString() : null,
                    SalaryHead = e.SalaryHead != null ? e.SalaryHead.FullDetails : null,
                    isCancel = e.isCancel,
                    TrClosed = e.TrClosed,
                    SanctionComment = e.FunctAllWFDetails != null && e.FunctAllWFDetails.Count > 0 ? e.FunctAllWFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(a => a.Id).Select(s => s.Comments).LastOrDefault() : null,
                    ApporavalComment = e.FunctAllWFDetails != null && e.FunctAllWFDetails.Count > 0 ? e.FunctAllWFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(a => a.Id).Select(s => s.Comments).LastOrDefault() : null,
                    Wf = e.FunctAllWFDetails != null && e.FunctAllWFDetails.Count > 0 ? e.FunctAllWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(s => s.Id).Select(a => a.Comments).LastOrDefault() : null,
                    Id = e.Id
                }).ToList();

                return Json(listOfObject, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetFunctAttendanceTData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);
                var WfStatus = new List<Int32>();
                var SanctionStatus = new List<Int32>();
                var ApporvalStatus = new List<Int32>();
                if (authority.ToUpper() == "SANCTION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "APPROVAL")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "MYSELF")
                {
                    SanctionStatus.Add(1);
                    SanctionStatus.Add(2);

                    ApporvalStatus.Add(3);
                    ApporvalStatus.Add(4);
                }
                var ids = Utility.StringIdsToListString(data);
                var status = ids.Count > 0 ? ids[2] : null;
                var id = Convert.ToInt32(ids[0]);
                var Emp = Convert.ToInt32(ids[1]);
                var qurey = db.FunctAttendanceT.Where(e => e.Id == id).SingleOrDefault();
                var list_salhead = db.FunctAttendanceT.Include(e => e.SalaryHead).Where(e =>
                    e.FromDate == qurey.FromDate &&
                    e.HourDays == qurey.HourDays &&
                    e.ToDate == e.ToDate &&
                    e.ReqDate == qurey.ReqDate &&
                    e.Reason == qurey.Reason)
                    .Select(e => e.SalaryHead.FullDetails).ToList();
                var Arr_salhead_list = list_salhead.ToArray().Distinct();
                var salheadlist = Arr_salhead_list != null ? string.Join(",", Arr_salhead_list) : null;
                var listOfObject = db.EmployeePayroll
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.FunctAttendanceT)
                    .Include(e => e.FunctAttendanceT.Select(r => r.PayStruct))
                    .Include(e => e.FunctAttendanceT.Select(r => r.PayStruct.Grade))
                    .Include(e => e.FunctAttendanceT.Select(r => r.GeoStruct))
                    .Include(e => e.FunctAttendanceT.Select(r => r.FuncStruct))
                    .Include(e => e.FunctAttendanceT.Select(r => r.FuncStruct.Job))
                    .Include(e => e.FunctAttendanceT.Select(r => r.GeoStruct.Location))
                    .Include(e => e.FunctAttendanceT.Select(r => r.GeoStruct.Location.LocationObj))
                    .Include(e => e.FunctAttendanceT.Select(r => r.GeoStruct.Location.BusinessCategory))
                    //.Include(e => e.FunctAttendanceT.Select(a => a.FinancialYear))
                    //.Include(e => e.FunctAttendanceT.Select(a => a.ITSection))
                    //.Include(e => e.FunctAttendanceT.Select(a => a.ITSection.ITSectionList))
                    //.Include(e => e.FunctAttendanceT.Select(a => a.ITSection.ITSectionListType))
                    //.Include(e => e.FunctAttendanceT.Select(a => a.ITInvestment))
                    .Include(e => e.FunctAttendanceT.Select(a => a.FunctAllWFDetails))
                    .Include(e => e.FunctAttendanceT.Select(a => a.WFStatus))
                .Where(e => e.Employee.Id == Emp && e.FunctAttendanceT.Any(a => a.Id == id)).SingleOrDefault();

                var ad = listOfObject.FunctAttendanceT.Where(e => e.Id == id).Select(e => new
                {
                    EmployeeName = listOfObject.Employee.EmpCode + " " + listOfObject.Employee.EmpName.FullNameFML,
                    Status = status,
                    HourDays = e.HourDays,
                    PayMonth = e.PayMonth,
                    ProcessMonth = e.ProcessMonth,
                    ReqDate = e.ReqDate != null ? e.ReqDate.Value.ToShortDateString() : null,
                    Narration = e.Reason,
                    Location = e.GeoStruct != null && e.GeoStruct.Location != null && e.GeoStruct.Location.LocationObj != null ? e.GeoStruct.Location.LocationObj.LocDesc : "",
                    CategoryWise = e.GeoStruct != null && e.GeoStruct.Location != null && e.GeoStruct.Location.BusinessCategory != null ? e.GeoStruct.Location.BusinessCategory.LookupVal : "",
                    Grade = e.PayStruct != null && e.PayStruct.Grade != null ? e.PayStruct.Grade.Name : "",
                    Designation = e.FuncStruct != null && e.FuncStruct.Job != null ? e.FuncStruct.Job.Name : "",
                    FromDate = e.FromDate != null ? e.FromDate.Value.ToShortDateString() : null,
                    ToDate = e.ToDate != null ? e.ToDate.Value.ToShortDateString() : null,
                    SalaryHead = salheadlist,
                    //isCancel = e.isCancel,
                    //TrClosed=e.TrClosed,
                    SanctionComment = e.FunctAllWFDetails != null && e.FunctAllWFDetails.Count > 0 ? e.FunctAllWFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(a => a.Id).Select(s => s.Comments).LastOrDefault() : null,
                    ApporavalComment = e.FunctAllWFDetails != null && e.FunctAllWFDetails.Count > 0 ? e.FunctAllWFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(a => a.Id).Select(s => s.Comments).LastOrDefault() : null,
                    Wf = e.FunctAllWFDetails != null && e.FunctAllWFDetails.Count > 0 ? e.FunctAllWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(s => s.Id).Select(a => a.Comments).LastOrDefault() : null
                }).ToList();

                return Json(ad, JsonRequestBehavior.AllowGet);
            }
        }
        public class FunctionattendanceClass1
        {
            public string Emp { get; set; }
            public string SalName { get; set; }
            public string ReqDate { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string HourDays { get; set; }

            public ChildGetLvNewReqClass2 RowData { get; set; }
        }
        public class ChildGetLvNewReqClass2
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }

        }
        public ActionResult GetFunctAttendanceT()
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                               .Include(e => e.LookupValues)
                               .Where(e => e.Code == "601").SingleOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                if (EmpidsWithfunsub.Any(e => e.SubModuleName != null))
                {
                    EmpidsWithfunsub = EmpidsWithfunsub.Where(e => e.SubModuleName == "FUNCALL").ToList();
                }
                if (EmpIds == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                //var LvList =
                //    db.EmployeePayroll
                //    .Include(e => e.Employee)
                //    .Include(e => e.Employee.ReportingStructRights)
                //    .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights))
                //    .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights.ActionName))
                //    .Include(e => e.FunctAttendanceT)
                //    .Include(e => e.FunctAttendanceT.Select(a => a.WFDetails))
                //    .Include(e => e.FunctAttendanceT.Select(a => a.WFStatus))
                //   .Where(e => EmpIds.Contains(e.Employee.Id)).ToList();
                //  List<EssPortal.Controllers.ELMSController.AttachDataClass> ListreturnLvnewClass = new List<EssPortal.Controllers.ELMSController.AttachDataClass>();
                List<FunctionattendanceClass1> ListreturnLvnewClass = new List<FunctionattendanceClass1>();
                ListreturnLvnewClass.Add(new FunctionattendanceClass1
                {
                    Emp = "Employee",
                    SalName = "SalHead Name",
                    ReqDate = "Requisition Date",
                    FromDate = "From Date",
                    ToDate = "To Date",
                    HourDays = "Hour Days"

                });

                foreach (var item1 in EmpidsWithfunsub)
                {
                    if (item1.ReportingEmployee.Count() > 0)
                    {
                        var temp =
                   db.EmployeePayroll
                   .Include(e => e.Employee)
                   .Include(e => e.Employee.ReportingStructRights)
                   .Include(e => e.Employee.ReportingStructRights.Select(a => a.FuncModules))
                   .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights))
                   .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights.ActionName))
                   .Include(e => e.FunctAttendanceT)
                   .Include(e => e.FunctAttendanceT.Select(a => a.FunctAllWFDetails));
                        // .Include(e => e.FunctAttendanceT.Select(a => a.WFStatus));
                        //.Where(e => EmpIds.Contains(e.Employee.Id)).ToList();
                        var LvList = temp.Where(e => item1.ReportingEmployee.Contains(e.Employee.Id)).ToList();

                        var LvIds = UserManager.FilterFunctAttendanceT(LvList.SelectMany(e => e.FunctAttendanceT).ToList(), Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));
                        // var session = Session["auho"].ToString().ToUpper();

                        foreach (var item in LvIds)
                        {

                            var query = db.EmployeePayroll
                               .Where(e => e.FunctAttendanceT.Any(a => a.Id == item))
                                .Select(e => new
                                {
                                    FunctAttendanceT = e.FunctAttendanceT.Where(a => a.Id == item),
                                    emppayrollid = e.Employee.Id,
                                    IsClose = e.Employee.ReportingStructRights
                                    .Where(a => a.AccessRights != null && a.AccessRights.Id == item1.AccessRights && a.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                    .Select(a => a.AccessRights.IsClose).FirstOrDefault(),
                                    EmpId = e.Employee.Id,
                                    EmpName = e.Employee.EmpName.FullNameFML,
                                    EmpCode = e.Employee.EmpCode
                                }).SingleOrDefault();

                            //ListreturnLvnewClass.Add(new EssPortal.Controllers.ELMSController.AttachDataClass
                            //{
                            //    ItinvestmentId = query.FunctAttendanceT.Select(e => e.Id).FirstOrDefault(),
                            //    emppayrollid = query.emppayrollid,
                            //    status = query.IsClose.ToString(),
                            //    val = query.EmpCode + " " + query.EmpName + "ReqDate :" + query.FunctAttendanceT.Select(e => e.ReqDate).SingleOrDefault() + "",
                            //});
                            string SalHname = string.Empty;
                            var fuctid = query.FunctAttendanceT.Where(a => a.Id == item).FirstOrDefault();
                            int? Salheadid = query.FunctAttendanceT.Where(a => a.Id == item).FirstOrDefault().SalaryHead_Id;

                            if (Salheadid != 0) { SalHname = db.SalaryHead.Where(e => e.Id == Salheadid).FirstOrDefault().Name; }
                            ListreturnLvnewClass.Add(new FunctionattendanceClass1
                            {
                                RowData = new ChildGetLvNewReqClass2
                                {
                                    LvNewReq = fuctid.Id.ToString(),
                                    EmpLVid = query.emppayrollid.ToString(),
                                    IsClose = query.IsClose.ToString(),
                                    LvHead_Id = "",
                                },
                                Emp = query.EmpCode + " " + query.EmpName,
                                SalName = SalHname != null ? SalHname : "",
                                ReqDate = fuctid.ReqDate.Value.ToShortDateString(),
                                FromDate = fuctid.FromDate.Value.ToShortDateString() + "",
                                ToDate = fuctid.ToDate.Value.ToShortDateString() + "",
                                HourDays = fuctid.HourDays.ToString()

                            });
                        }
                    }
                }
                if (ListreturnLvnewClass != null && ListreturnLvnewClass.Count > 0)
                {
                    if (FuncAllowanceSettings.CCode != null || FuncAllowanceSettings.DFromDate != null || FuncAllowanceSettings.DToDate != null)
                    {
                        int currentDay = DateTime.Now.Day;
                        int fromDay = int.Parse(FuncAllowanceSettings.DFromDate);
                        int toDay = int.Parse(FuncAllowanceSettings.DToDate);

                        var comcode = FuncAllowanceSettings.CCode;

                        if (comcode.ToString() == "NKGSB")
                        {
                            if (currentDay < fromDay || currentDay > toDay)
                            {
                                return null;
                            }
                            else
                            {
                                return Json(new { status = true, data = ListreturnLvnewClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        else
                        {
                            return Json(new { status = true, data = ListreturnLvnewClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { status = true, data = ListreturnLvnewClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { status = false, data = ListreturnLvnewClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public class tempClass
        {
            public string LvName { get; set; }
            public string LvCode { get; set; }
            public string LvBal { get; set; }
            public string FullDetails { get; set; }
        }
        public class EmpLvClass
        {
            public string EmpName { get; set; }
            public List<ReqLvHeadWise> LvHeadName { get; set; }
        }
        public class ReqLvHeadWise
        {
            public string LvHeadName { get; set; }
            public string LvHeadCode { get; set; }
            public string LvHeadBal { get; set; }
            public Array LvReq { get; set; }
        }

        public ActionResult GetEmpFuncattendanceHistory()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                               .Include(e => e.LookupValues)
                               .Where(e => e.Code == "601").SingleOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Emps = db.EmployeePayroll
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.FunctAttendanceT)
                    .Include(e => e.FunctAttendanceT.Select(t => t.SalaryHead))
                    .Include(e => e.FunctAttendanceT.Select(a => a.FunctAllWFDetails))
                    .Include(e => e.FunctAttendanceT.Select(a => a.WFStatus))
                   .Where(e => EmpIds.Contains(e.Employee.Id)).ToList();

                var allLvHead = db.SalaryHead.ToList();
                List<EmpLvClass> ListEmpLvClass = new List<EmpLvClass>();

                foreach (var ca in Emps)
                {
                    var oEmpLvClass = new EmpLvClass();
                    foreach (var lvhead in allLvHead)
                    {
                        var temp = new List<tempClass>();
                        var LvData = ca.FunctAttendanceT.Where(e => e.SalaryHead.Id == lvhead.Id).OrderByDescending(e => e.ReqDate).ToList();
                        foreach (var item in LvData)
                        {
                            var Status = "--";
                            if (item.InputMethod == 1 && item.FunctAllWFDetails.Count > 0)
                            {
                                Status = Utility.GetStatusName().Where(e => e.Key == item.FunctAllWFDetails.LastOrDefault().WFStatus.ToString())
                                .Select(e => e.Value).SingleOrDefault();
                            }
                            if (item.InputMethod == 0)
                            {
                                Status = "Approved By HRM (M)";
                            }

                            temp.Add(new tempClass
                            {
                                LvName = item.SalaryHead.Name,
                                LvCode = item.ReqDate.Value.ToShortDateString(),
                                LvBal = "From Date:" + item.FromDate.Value.ToShortDateString() + "To Date :" + item.ToDate.Value.ToShortDateString() + "Hour Days :" + item.HourDays + "Status :" + Status,
                            });

                            if (LvData != null && LvData.Count > 0)
                            {
                                oEmpLvClass.EmpName = ca.Employee.EmpCode + " " + ca.Employee.EmpName.FullNameFML;
                                if (oEmpLvClass.LvHeadName == null)
                                {
                                    oEmpLvClass.LvHeadName = new List<ReqLvHeadWise>{new ReqLvHeadWise {
                                            LvHeadName=temp.Select(e=>e.LvName).FirstOrDefault().ToString(),
                                            LvHeadCode=temp.Select(e=>e.LvCode).FirstOrDefault().ToString(),
                                            LvHeadBal=temp.Select(e=>e.LvBal).FirstOrDefault().ToString()
                                        }};
                                }
                                else
                                {
                                    oEmpLvClass.LvHeadName.Add(new ReqLvHeadWise
                                    {
                                        LvHeadName = temp.Select(e => e.LvName).FirstOrDefault().ToString(),
                                        LvHeadCode = temp.Select(e => e.LvCode).FirstOrDefault().ToString(),
                                        LvHeadBal = temp.Select(e => e.LvBal).LastOrDefault().ToString()
                                    });
                                }

                            }
                        }
                    }
                    if (oEmpLvClass.EmpName != null)
                    {
                        ListEmpLvClass.Add(oEmpLvClass);
                    }
                }
                return Json(new Utility.JsonClass { status = true, Data = ListEmpLvClass }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateStatus(FunctAttendanceT LvReq, FormCollection form, String data)
        {
            try
            {
                string authority = form["authority"] == null ? null : form["authority"];
                var isClose = form["isClose"] == null ? null : form["isClose"];
                if (authority == null && isClose == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                string MySelf = form["MySelf"] == null ? "false" : form["MySelf"];
                string Sanction = form["Sanction"];
                string Approval = form["Approval"];
                string HR = form["HR"] == null ? null : form["HR"];

                string ReasonMySelf = form["ReasonMySelf"] == null ? null : form["ReasonMySelf"];
                string ReasonSanction = form["ReasonSanction"];
                string ReasonApproval = form["ReasonApproval"];
                string ReasonHr = form["ReasonHr"] == null ? null : form["ReasonHr"];
                bool SanctionRejected = false;
                bool HrRejected = false;
                var ids = Utility.StringIdsToListString(data);
                var status = ids.Count > 0 ? ids[2] : null;
                var id = Convert.ToInt32(ids[0]);
                var Emp = Convert.ToInt32(ids[1]);
                using (DataBaseContext db = new DataBaseContext())
                {
                    var qurey = db.FunctAttendanceT.Include(e => e.FunctAllWFDetails)
                        .Include(e => e.WFStatus).Where(e => e.Id == id).SingleOrDefault();
                    if (authority.ToUpper() == "MYSELF")
                    {
                        qurey.Reason = ReasonMySelf;
                        qurey.isCancel = true;
                        qurey.TrClosed = true;
                        qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "6").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();//db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();
                    }
                    else if (authority.ToUpper() == "SANCTION")
                    {
                        if (Sanction == null)
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Sanction Status..!" }, JsonRequestBehavior.AllowGet);

                        }
                        if (ReasonSanction == "")
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason..!" }, JsonRequestBehavior.AllowGet);

                        }
                        if (Convert.ToBoolean(Sanction) == true)
                        {
                            //sanction yes -1
                            var LvWFDetails = new FunctAllWFDetails
                            {
                                WFStatus = 1,
                                Comments = ReasonSanction,
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                            };
                            qurey.FunctAllWFDetails.Add(LvWFDetails);
                            qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault(); //db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault();
                            qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                        }
                        else if (Convert.ToBoolean(Sanction) == false)
                        {
                            //sanction no -2
                            var LvWFDetails = new FunctAllWFDetails
                            {
                                WFStatus = 2,
                                Comments = ReasonSanction,
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                            };
                            qurey.FunctAllWFDetails.Add(LvWFDetails);
                            qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "2").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();//db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                            qurey.TrClosed = true;
                            SanctionRejected = true;
                        }
                    }
                    else if (authority.ToUpper() == "APPROVAL")//Hr
                    {
                        if (Approval == null)
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Approval Status..!" }, JsonRequestBehavior.AllowGet);

                        }
                        if (ReasonApproval == "")
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason..!" }, JsonRequestBehavior.AllowGet);

                        }
                        if (Convert.ToBoolean(Approval) == true)
                        {
                            //approval yes-3
                            var LvWFDetails = new FunctAllWFDetails
                            {
                                WFStatus = 3,
                                Comments = ReasonApproval,
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                            };
                            qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                            qurey.FunctAllWFDetails.Add(LvWFDetails);
                            qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault(); //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();
                        }
                        else if (Convert.ToBoolean(Approval) == false)
                        {
                            //approval no-4
                            var LvWFDetails = new FunctAllWFDetails
                            {
                                WFStatus = 4,
                                Comments = ReasonApproval,
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                            };
                            qurey.FunctAllWFDetails.Add(LvWFDetails);
                            qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "4").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();//db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault();
                            qurey.TrClosed = true;
                            HrRejected = true;
                        }
                    }
                    else if (authority.ToUpper() == "RECOMMAND")
                    {

                    }

                    using (TransactionScope ts = new TransactionScope())
                    {
                        //if someone reject lv
                        if (SanctionRejected == true || HrRejected == true)
                        {
                            qurey.TrReject = true;
                        }
                        //    ts.Complete();
                        //    return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                        //}

                        db.FunctAttendanceT.Attach(qurey);
                        db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception e)
            {
                return Json(new Utility.JsonClass { status = false, responseText = e.Message }, JsonRequestBehavior.AllowGet);

            }
            //   return View();
        }
    }

    public class FuncAllowanceSettings
    {
        private string CompCode;
        private string FromDate;
        private string ToDate;


        //public FuncAllowanceSettings(IDictionary<string, string> settinigs)
        //{
        //    this.CompCode = settinigs.First(x => x.Key.Equals("CompCode")).Value;
        //    this.FromDate = settinigs.First(x => x.Key.Equals("FromDate")).Value;
        //    this.ToDate = settinigs.First(x => x.Key.Equals("ToDate")).Value;
        //}

        public FuncAllowanceSettings(IDictionary<string, string> settinigs)
        {
            if (settinigs.Count() > 0)
            {
                this.CompCode = settinigs.First(x => x.Key.Equals("CompCode")).Value;
                this.FromDate = settinigs.First(x => x.Key.Equals("FromDate")).Value;
                this.ToDate = settinigs.First(x => x.Key.Equals("ToDate")).Value;
            }

        }


        public string CCode { get { return CompCode; } }
        public string DFromDate { get { return FromDate; } }
        public string DToDate { get { return ToDate; } }


    }
}