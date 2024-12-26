using P2b.Global;
using P2BUltimate.Models;
using Payroll;
using Leave;
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
using P2BUltimate.Process;
using System.Collections.Specialized;
using System.IO;
using Newtonsoft.Json;
using P2B.UTILS;
using P2B.API.Models;
using System.Net;

namespace P2BUltimate.Controllers.Leave.MainController
{
    public class LvCreditProcessController : Controller
    {
        public static List<string> LeaveCreditProcessMsg = new List<string>();
        public static List<string> LeaveCreditProcessReportMsg = new List<string>();
        //public static List<string> LeaveCreditProcessReportPendingLeaveMsg = new List<string>();
        public ActionResult Index()
        {
            return View();
        }
        public List<KeyValuePair<Int32, string>> ErrorMsg()
        {
            List<KeyValuePair<Int32, string>> LookupStatus = new List<KeyValuePair<Int32, string>>()
                {
                    new KeyValuePair<Int32, string>(0, "Wrong Entry"),
                    new KeyValuePair<Int32, string>(1, "In time Missing"),
                    new KeyValuePair<Int32, string>(2, "Out time Missing"),
                    new KeyValuePair<Int32, string>(3, "Holiday WO"),
                    new KeyValuePair<Int32, string>(4, "In Late"),
                     new KeyValuePair<Int32, string>(5, "Manual entry"),
                     new KeyValuePair<Int32, string>(6, "Out Early"),
                };
            return LookupStatus;
        }
        public class P2BCrGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string LvCreditDate { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }

        }
        public class P2BCrGridData1
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string LvCreditDate { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }

        }

        public ActionResult FindLeaveEmpId()
        {
            dynamic EmpLvIds = System.Web.HttpContext.Current.Session["LeaveCreditProcessEmpids"];
            return Json(EmpLvIds, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadEmp(P2BGrid_Parameters gp, string param1, string param2, string param3, string param4)
        {
            DataBaseContext db = new DataBaseContext();

            List<int> lvheadids = null;
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\LvCreditround" + ".ini";
            localPath = new Uri(path).LocalPath;
            if (!System.IO.File.Exists(localPath))
            {

                using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                {
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);

                    str.Flush();
                    str.Close();
                    fs.Close();
                }


            }

            if (param4 != null && param4 != "0" && param4 != "false")
            {
                lvheadids = Utility.StringIdsToListIds(param4);
            }

            DateTime? fromdate = null;
            DateTime? todate = null;
            if (param1 != "" && param2 != "")
            {
                fromdate = Convert.ToDateTime(param1);
                todate = Convert.ToDateTime(param2);
            }
            var lookupid = Convert.ToInt32(param3);
            var CreditDatelist = db.LookupValue.Where(e => e.Id == lookupid).SingleOrDefault();

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
                //var empdata = db.CompanyPayroll
                //    .Include(e => e.EmployeePayroll)
                //    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                //    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                //    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                //    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                //    .Where(e => e.Company.Id == compid).SingleOrDefault();

                //var emp = empdata.EmployeePayroll.Select(e => e.Employee).ToList();

                var empdata = db.CompanyLeave
                   .Include(e => e.EmployeeLeave)
                   .Include(e => e.EmployeeLeave.Select(a => a.Employee))
                   .Include(e => e.EmployeeLeave.Select(a => a.Employee.EmpName))
                   .Include(e => e.EmployeeLeave.Select(a => a.Employee.ServiceBookDates))
                   .Where(e => e.Company.Id == compid).SingleOrDefault();


                var emp = empdata.EmployeeLeave.Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null || e.Employee.ServiceBookDates.ServiceLastDate >= System.DateTime.Now.Date).Select(e => e.Employee).ToList();

                ///////////////
                if (CreditDatelist.LookupVal.ToUpper() == "CALENDAR" || CreditDatelist.LookupVal.ToUpper() == "YEARLY")
                {
                    Calendar LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                   e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                    fromdate = LvCalendar.FromDate;
                    todate = LvCalendar.ToDate;

                    foreach (var z in emp)
                    {

                        int OEmpLvID = db.EmployeeLeave.Where(e => e.Employee.Id == z.Id).FirstOrDefault().Id;

                        //EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                        //      .Include(e => e.EmployeeLvStructDetails)
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.CreditDate))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.LvHead))
                        //      .Where(e => e.EndDate == null && e.EmployeeLeave_Id == OEmpLvID).AsNoTracking().FirstOrDefault();

                        var OLvSalStruct = db.EmployeeLvStruct.Select(d => new
                        {
                            OEndDate = d.EndDate,
                            OEffectiveDate = d.EffectiveDate,
                            OEmployeeLeaveId = d.EmployeeLeave_Id,
                            OEmployeeLvStructDetails = d.EmployeeLvStructDetails.Select(r => new
                            {
                                OLvHeadFormula = r.LvHeadFormula,
                                OEmployeeLvStructDetailsLvHeadId = r.LvHead_Id,
                                OEmployeeLvStructDetailsLvHead = r.LvHead,
                                OLvCreditPolicy = r.LvHeadFormula.LvCreditPolicy,
                                OCreditDate = r.LvHeadFormula.LvCreditPolicy.CreditDate,
                                OLvCreditPolicyLvHead = r.LvHeadFormula.LvCreditPolicy.LvHead,

                            }).ToList()
                        }).Where(e => e.OEndDate == null && e.OEmployeeLeaveId == OEmpLvID).FirstOrDefault();


                        LvCreditPolicy oLvCreditPolicy = null;
                        if (OLvSalStruct != null)
                        {


                            foreach (var LvHead_ids in lvheadids)
                            {
                                double mSusDayFull = 0;
                                int lastyearid = 0;
                                var EmpLvHeadList = db.LvHead.Where(e => e.Id == LvHead_ids)
                                    .Select(a => new { Id = a.Id, LvCode = a.LvCode }).Distinct().ToList();


                                foreach (var item in EmpLvHeadList)
                                {
                                    oLvCreditPolicy = OLvSalStruct.OEmployeeLvStructDetails.Where(e => e.OEmployeeLvStructDetailsLvHeadId == item.Id && e.OLvHeadFormula != null && e.OLvCreditPolicy != null).Select(r => r.OLvCreditPolicy).FirstOrDefault();
                                    if (oLvCreditPolicy != null && (oLvCreditPolicy.CreditDate.LookupVal.ToUpper() == "CALENDAR" || oLvCreditPolicy.CreditDate.LookupVal.ToUpper() == "YEARLY"))
                                    {
                                        //check leave credit or not which not credit that employee load 
                                        // var LvNewReqData = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == z.Id).AsNoTracking().FirstOrDefault();
                                        var LvNewReqData = db.LvNewReq.Where(q => q.EmployeeLeave_Id == OEmpLvID && q.LvCreditNextDate != null && q.LvOrignal_Id == null && q.LeaveHead_Id == item.Id).AsNoTracking().OrderByDescending(q => q.Id).FirstOrDefault();
                                        LvNewReq aa = null;
                                        DateTime lvnxcrdt;
                                        if (LvNewReqData != null)
                                        {
                                            lvnxcrdt = System.DateTime.Now;
                                            //&&  q.LvCreditNextDate <= lvnxcrdt
                                            aa = LvNewReqData;//LvNewReqData.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvOrignal_Id == null && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                            if (aa != null)
                                            {
                                                if (aa.LvCreditNextDate.Value.Date <= lvnxcrdt.Date)
                                                {
                                                    if (aa.LvCreditNextDate.Value.Date >= fromdate.Value.Date && aa.LvCreditNextDate.Value.Date <= todate.Value.Date)
                                                    {
                                                        if (!model.Select(q => q.Id).Contains(z.Id))
                                                        {
                                                            view = new P2BCrGridData()
                                                            {
                                                                Id = z.Id,
                                                                Code = z.EmpCode,
                                                                Name = z.EmpName.FullNameFML,

                                                            };
                                                            model.Add(view);

                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            else// first time leave credit check lvstruct credit policy defien or not. if define then leave credit
                                            {
                                                if (!model.Select(q => q.Id).Contains(z.Id))
                                                {
                                                    view = new P2BCrGridData()
                                                    {
                                                        Id = z.Id,
                                                        Code = z.EmpCode,
                                                        Name = z.EmpName.FullNameFML,

                                                    };
                                                    model.Add(view);

                                                    break;
                                                }
                                            }
                                        }
                                        else // Credit Policy available first time Leave Credit
                                        {
                                            if (!model.Select(q => q.Id).Contains(z.Id))
                                            {
                                                view = new P2BCrGridData()
                                                {
                                                    Id = z.Id,
                                                    Code = z.EmpCode,
                                                    Name = z.EmpName.FullNameFML,

                                                };
                                                model.Add(view);

                                                break;
                                            }
                                        }

                                    }
                                }
                            }
                        }

                    }
                }




                else if (CreditDatelist.LookupVal.ToUpper() == "JOININGDATE")
                {
                    // from leave structure start
                    foreach (var z in emp)
                    {

                        int OEmpLvID = db.EmployeeLeave.Where(e => e.Employee.Id == z.Id).FirstOrDefault().Id;

                        //EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                        //      .Include(e => e.EmployeeLvStructDetails)
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.CreditDate))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.LvHead))
                        //      .Where(e => e.EndDate == null && e.EmployeeLeave_Id == OEmpLvID).AsNoTracking().FirstOrDefault();
                        var OLvSalStruct = db.EmployeeLvStruct.Select(d => new
                        {
                            OEndDate = d.EndDate,
                            OEffectiveDate = d.EffectiveDate,
                            OEmployeeLeaveId = d.EmployeeLeave_Id,
                            OEmployeeLvStructDetails = d.EmployeeLvStructDetails.Select(r => new
                            {
                                OLvHeadFormula = r.LvHeadFormula,
                                OEmployeeLvStructDetailsLvHeadId = r.LvHead_Id,
                                OEmployeeLvStructDetailsLvHead = r.LvHead,
                                OLvCreditPolicy = r.LvHeadFormula.LvCreditPolicy,
                                OCreditDate = r.LvHeadFormula.LvCreditPolicy.CreditDate,
                                OLvCreditPolicyLvHead = r.LvHeadFormula.LvCreditPolicy.LvHead,

                            }).ToList()
                        }).Where(e => e.OEndDate == null && e.OEmployeeLeaveId == OEmpLvID).SingleOrDefault();


                        LvCreditPolicy oLvCreditPolicy = null;
                        if (OLvSalStruct != null)
                        {


                            //foreach (var LvHead_ids in lvheadids)
                            //{
                            //    double mSusDayFull = 0;
                            //    int lastyearid = 0;
                            //    var EmpLvHeadList = db.LvHead.Where(e => e.Id == LvHead_ids)
                            //        .Select(a => new { Id = a.Id, LvCode = a.LvCode }).Distinct().ToList();


                            foreach (var item in lvheadids)
                            {

                                // oLvCreditPolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == item && e.LvHeadFormula != null && e.LvHeadFormula.LvCreditPolicy != null).Select(r => r.LvHeadFormula.LvCreditPolicy).FirstOrDefault();
                                oLvCreditPolicy = OLvSalStruct.OEmployeeLvStructDetails.Where(e => e.OEmployeeLvStructDetailsLvHeadId == item && e.OLvHeadFormula != null && e.OLvCreditPolicy != null).Select(r => r.OLvCreditPolicy).FirstOrDefault();
                                if (oLvCreditPolicy != null && oLvCreditPolicy.CreditDate.LookupVal.ToUpper() == "JOININGDATE")
                                {
                                    //check leave credit or not which not credit that employee load 
                                    var LvNewReqData = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == z.Id).AsNoTracking().FirstOrDefault();
                                    LvNewReq aa = null;
                                    DateTime lvnxcrdt;
                                    if (LvNewReqData != null)
                                    {
                                        lvnxcrdt = System.DateTime.Now;
                                        //&&  q.LvCreditNextDate <= lvnxcrdt
                                        aa = LvNewReqData.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvOrignal_Id == null && q.LeaveHead.Id == item).OrderByDescending(q => q.Id).FirstOrDefault();
                                        if (aa != null)
                                        {
                                            if (aa.LvCreditNextDate.Value.Date <= lvnxcrdt.Date)
                                            {
                                                if (aa.LvCreditNextDate.Value.Date >= fromdate.Value.Date && aa.LvCreditNextDate.Value.Date <= todate.Value.Date)
                                                {
                                                    if (!model.Select(q => q.Id).Contains(z.Id))
                                                    {
                                                        view = new P2BCrGridData()
                                                        {
                                                            Id = z.Id,
                                                            Code = z.EmpCode,
                                                            Name = z.EmpName.FullNameFML,

                                                        };
                                                        model.Add(view);

                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        else// first time leave credit check lvstruct credit policy defien or not. if define then leave credit
                                        {

                                            var ServiceBookData = db.Employee.Include(e => e.ServiceBookDates).Where(q => q.ServiceBookDates.JoiningDate != null && q.Id == z.Id).ToList();

                                            if (ServiceBookData != null)
                                            {
                                                var ServiceBookData1 = ServiceBookData.Where(q => q.ServiceBookDates.JoiningDate.Value.Month >= fromdate.Value.Month && q.ServiceBookDates.JoiningDate.Value.Month <= todate.Value.Month).ToList();

                                                foreach (var item1 in ServiceBookData1)
                                                {

                                                    if (fromdate.Value.Year - item1.ServiceBookDates.JoiningDate.Value.Year != 0)
                                                    {

                                                        var Fulldate = item1.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + fromdate.Value.Year;

                                                        DateTime newdate = Convert.ToDateTime(Fulldate);

                                                        if (newdate >= fromdate && newdate <= todate)
                                                        {

                                                            EmployeeLeave OEmployeeLeave = null;
                                                            OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                                               .Include(e => e.LvNewReq.Select(r => r.LeaveHead)).Include(e => e.LvNewReq.Select(r => r.LeaveCalendar)).Where(e => e.Employee.Id == item1.Id).SingleOrDefault();

                                                            var LvNewReqDatacreditedempnotload = OEmployeeLeave.LvNewReq.Where(q => q.LvCreditDate >= fromdate && q.LvCreditDate <= todate && q.LeaveHead.Id == item).ToList();
                                                            if (LvNewReqDatacreditedempnotload.Count() == 0)
                                                            {

                                                                if (!model.Select(q => q.Id).Contains(z.Id))
                                                                {
                                                                    view = new P2BCrGridData()
                                                                    {
                                                                        Id = z.Id,
                                                                        Code = z.EmpCode,
                                                                        Name = z.EmpName.FullNameFML,

                                                                    };
                                                                    model.Add(view);

                                                                    break;
                                                                }

                                                            }
                                                        }
                                                    }
                                                }
                                            }



                                        }
                                    }
                                    else // Credit Policy available first time Leave Credit
                                    {
                                        if (!model.Select(q => q.Id).Contains(z.Id))
                                        {
                                            view = new P2BCrGridData()
                                            {
                                                Id = z.Id,
                                                Code = z.EmpCode,
                                                Name = z.EmpName.FullNameFML,

                                            };
                                            model.Add(view);

                                            break;
                                        }
                                    }

                                }
                            }
                            // }
                        }

                    }
                    // from leave structure end

                    //var LvNewReqData = db.LvNewReq.Include(x => x.LeaveHead).Where(q => q.LvCreditNextDate != null).ToList();

                    //if (LvNewReqData != null)
                    //{
                    //    var lvdata = LvNewReqData.Where(q => q.LvCreditNextDate >= fromdate && q.LvCreditNextDate <= todate && lvheadids.Contains(q.LeaveHead.Id)).ToList();

                    //    var Emp_lvcr = db.EmployeeLeave.Include(a => a.Employee).Include(e => e.LvNewReq).ToList();

                    //    foreach (var item in Emp_lvcr)
                    //    {
                    //        var fgds = item.LvNewReq.Where(q => lvdata.Select(t => t.Id).Contains(q.Id)).FirstOrDefault();

                    //        if (fgds != null)
                    //        {

                    //            // EmpList1.Add(item.Employee);
                    //            var Fulldate = fgds.LvCreditNextDate.Value.ToShortDateString();

                    //            P2BCrGridData pb = new P2BCrGridData()
                    //            {
                    //                Id = item.Employee.Id,
                    //                Code = item.Employee.EmpCode,
                    //                Name = item.Employee.EmpName.FullNameFML,
                    //                LvCreditDate = Fulldate
                    //            };
                    //            model.Add(pb);


                    //        }
                    //    }
                    //}

                    //var ServiceBookData = db.Employee.Include(e => e.ServiceBookDates).Where(q => q.ServiceBookDates.JoiningDate != null).ToList();

                    //if (ServiceBookData != null)
                    //{
                    //    var ServiceBookData1 = ServiceBookData.Where(q => q.ServiceBookDates.JoiningDate.Value.Month >= fromdate.Value.Month && q.ServiceBookDates.JoiningDate.Value.Month <= todate.Value.Month).ToList();

                    //    foreach (var item in ServiceBookData1)
                    //    {

                    //        if (fromdate.Value.Year - item.ServiceBookDates.JoiningDate.Value.Year != 0)
                    //        {

                    //            var Fulldate = item.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + fromdate.Value.Year;

                    //            DateTime newdate = Convert.ToDateTime(Fulldate);

                    //            if (newdate >= fromdate && newdate <= todate)
                    //            {
                    //                var EmployeeList = db.EmployeeLeave.Select(e => e.Employee)
                    //                               .Where(e => e.Id == item.Id).SingleOrDefault();
                    //                EmployeeLeave OEmployeeLeave = null;
                    //                OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                    //   .Include(e => e.LvNewReq.Select(r => r.LeaveHead)).Include(e => e.LvNewReq.Select(r => r.LeaveCalendar)).Where(e => e.Employee.Id == item.Id).SingleOrDefault();

                    //                var LvNewReqDatacreditedempnotload = OEmployeeLeave.LvNewReq.Where(q => q.LvCreditDate >= fromdate && q.LvCreditDate <= todate && lvheadids.Contains(q.LeaveHead.Id)).ToList();
                    //                if (LvNewReqDatacreditedempnotload.Count() == 0)
                    //                {
                    //                    if (EmployeeList != null)
                    //                    {
                    //                        if (!model.Select(q => q.Id).Contains(item.Id))
                    //                        {
                    //                            P2BCrGridData pb = new P2BCrGridData()
                    //                            {
                    //                                Id = EmployeeList.Id,
                    //                                Code = EmployeeList.EmpCode,
                    //                                Name = EmployeeList.EmpName.FullNameFML,
                    //                                LvCreditDate = newdate.ToShortDateString()
                    //                            };
                    //                            // EmpList1.Add(EmployeeList);
                    //                            if (EmployeeList.ServiceBookDates.ServiceLastDate == null)
                    //                            {
                    //                                model.Add(pb);
                    //                            }
                    //                            else if (Convert.ToDateTime("01/" + EmployeeList.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + PayMonth))
                    //                            {
                    //                                model.Add(pb);
                    //                            }
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                }


                else if (CreditDatelist.LookupVal.ToUpper() == "CONFIRMATIONDATE")
                {
                    // from leave structure start
                    foreach (var z in emp)
                    {

                        int OEmpLvID = db.EmployeeLeave.Where(e => e.Employee.Id == z.Id).FirstOrDefault().Id;

                        //EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                        //      .Include(e => e.EmployeeLvStructDetails)
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.CreditDate))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.LvHead))
                        //      .Where(e => e.EndDate == null && e.EmployeeLeave_Id == OEmpLvID).AsNoTracking().FirstOrDefault();
                        var OLvSalStruct = db.EmployeeLvStruct.Select(d => new
                        {
                            OEndDate = d.EndDate,
                            OEffectiveDate = d.EffectiveDate,
                            OEmployeeLeaveId = d.EmployeeLeave_Id,
                            OEmployeeLvStructDetails = d.EmployeeLvStructDetails.Select(r => new
                            {
                                OLvHeadFormula = r.LvHeadFormula,
                                OEmployeeLvStructDetailsLvHeadId = r.LvHead_Id,
                                OEmployeeLvStructDetailsLvHead = r.LvHead,
                                OLvCreditPolicy = r.LvHeadFormula.LvCreditPolicy,
                                OCreditDate = r.LvHeadFormula.LvCreditPolicy.CreditDate,
                                OLvCreditPolicyLvHead = r.LvHeadFormula.LvCreditPolicy.LvHead,

                            }).ToList()
                        }).Where(e => e.OEndDate == null && e.OEmployeeLeaveId == OEmpLvID).SingleOrDefault();



                        LvCreditPolicy oLvCreditPolicy = null;
                        if (OLvSalStruct != null)
                        {


                            //foreach (var LvHead_ids in lvheadids)
                            //{
                            //    double mSusDayFull = 0;
                            //    int lastyearid = 0;
                            //    var EmpLvHeadList = db.LvHead.Where(e => e.Id == LvHead_ids)
                            //        .Select(a => new { Id = a.Id, LvCode = a.LvCode }).Distinct().ToList();


                            foreach (var item in lvheadids)
                            {
                                //oLvCreditPolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == item && e.LvHeadFormula != null && e.LvHeadFormula.LvCreditPolicy != null).Select(r => r.LvHeadFormula.LvCreditPolicy).FirstOrDefault();
                                oLvCreditPolicy = OLvSalStruct.OEmployeeLvStructDetails.Where(e => e.OEmployeeLvStructDetailsLvHeadId == item && e.OLvHeadFormula != null && e.OLvCreditPolicy != null).Select(r => r.OLvCreditPolicy).FirstOrDefault();
                                if (oLvCreditPolicy != null && oLvCreditPolicy.CreditDate.LookupVal.ToUpper() == "CONFIRMATIONDATE")
                                {
                                    //check leave credit or not which not credit that employee load 
                                    var LvNewReqData = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == z.Id).AsNoTracking().FirstOrDefault();
                                    LvNewReq aa = null;
                                    DateTime lvnxcrdt;
                                    if (LvNewReqData != null)
                                    {
                                        lvnxcrdt = System.DateTime.Now;
                                        //&&  q.LvCreditNextDate <= lvnxcrdt
                                        aa = LvNewReqData.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvOrignal_Id == null && q.LeaveHead.Id == item).OrderByDescending(q => q.Id).FirstOrDefault();
                                        if (aa != null)
                                        {
                                            if (aa.LvCreditNextDate.Value.Date <= lvnxcrdt.Date)
                                            {
                                                if (aa.LvCreditNextDate.Value.Date >= fromdate.Value.Date && aa.LvCreditNextDate.Value.Date <= todate.Value.Date)
                                                {
                                                    if (!model.Select(q => q.Id).Contains(z.Id))
                                                    {
                                                        view = new P2BCrGridData()
                                                        {
                                                            Id = z.Id,
                                                            Code = z.EmpCode,
                                                            Name = z.EmpName.FullNameFML,

                                                        };
                                                        model.Add(view);

                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        else// first time leave credit check lvstruct credit policy defien or not. if define then leave credit
                                        {
                                            var ServiceBookData = db.Employee.Include(e => e.ServiceBookDates).Where(q => q.ServiceBookDates.ConfirmationDate != null && q.Id == z.Id).ToList();

                                            if (ServiceBookData != null)
                                            {
                                                var ServiceBookData1 = ServiceBookData.Where(q => q.ServiceBookDates.ConfirmationDate.Value.Month >= fromdate.Value.Month && q.ServiceBookDates.ConfirmationDate.Value.Month <= todate.Value.Month).ToList();

                                                foreach (var item1 in ServiceBookData1)
                                                {
                                                    if (fromdate.Value.Year - item1.ServiceBookDates.ConfirmationDate.Value.Year != 0)
                                                    {

                                                        var Fulldate = item1.ServiceBookDates.ConfirmationDate.Value.ToString("dd/MM") + "/" + fromdate.Value.Year;

                                                        DateTime newdate = Convert.ToDateTime(Fulldate);


                                                        if (newdate >= fromdate && newdate <= todate)
                                                        {


                                                            EmployeeLeave OEmployeeLeave = null;
                                                            OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                                               .Include(e => e.LvNewReq.Select(r => r.LeaveHead)).Include(e => e.LvNewReq.Select(r => r.LeaveCalendar)).Where(e => e.Employee.Id == item1.Id).SingleOrDefault();

                                                            var LvNewReqDatacreditedempnotload = OEmployeeLeave.LvNewReq.Where(q => q.LvCreditDate >= fromdate && q.LvCreditDate <= todate && q.LeaveHead.Id == item).ToList();
                                                            if (LvNewReqDatacreditedempnotload.Count() == 0)
                                                            {

                                                                if (!model.Select(q => q.Id).Contains(z.Id))
                                                                {
                                                                    view = new P2BCrGridData()
                                                                    {
                                                                        Id = z.Id,
                                                                        Code = z.EmpCode,
                                                                        Name = z.EmpName.FullNameFML,

                                                                    };
                                                                    model.Add(view);

                                                                    break;
                                                                }

                                                            }
                                                        }
                                                    }
                                                }
                                            }




                                        }
                                    }
                                    else // Credit Policy available first time Leave Credit
                                    {
                                        if (!model.Select(q => q.Id).Contains(z.Id))
                                        {
                                            view = new P2BCrGridData()
                                            {
                                                Id = z.Id,
                                                Code = z.EmpCode,
                                                Name = z.EmpName.FullNameFML,

                                            };
                                            model.Add(view);

                                            break;
                                        }
                                    }

                                }
                                //}
                            }
                        }

                    }
                    // from leave structure End
                    //var LvNewReqData = db.LvNewReq.Include(x => x.LeaveHead).Where(q => q.LvCreditNextDate != null).ToList();
                    //if (LvNewReqData != null)
                    //{
                    //    var lvdata = LvNewReqData.Where(q => q.LvCreditNextDate >= fromdate && q.LvCreditNextDate <= todate && lvheadids.Contains(q.LeaveHead.Id)).ToList();

                    //    var Emp_lvcr = db.EmployeeLeave.Include(a => a.Employee).Include(e => e.LvNewReq).ToList();

                    //    foreach (var item in Emp_lvcr)
                    //    {
                    //        var fgds = item.LvNewReq.Where(q => lvdata.Select(t => t.Id).Contains(q.Id)).FirstOrDefault();

                    //        if (fgds != null)
                    //        {
                    //            // EmpList1.Add(item.Employee);
                    //            var Fulldate = fgds.LvCreditNextDate.Value.ToShortDateString();

                    //            P2BCrGridData pb = new P2BCrGridData()
                    //            {
                    //                Id = item.Employee.Id,
                    //                Code = item.Employee.EmpCode,
                    //                Name = item.Employee.EmpName.FullNameFML,
                    //                LvCreditDate = Fulldate
                    //            };
                    //            model.Add(pb);

                    //        }

                    //    }
                    //}



                    //var ServiceBookData = db.Employee.Include(e => e.ServiceBookDates).Where(q => q.ServiceBookDates.ConfirmationDate != null).ToList();

                    //if (ServiceBookData != null)
                    //{
                    //    var ServiceBookData1 = ServiceBookData.Where(q => q.ServiceBookDates.ConfirmationDate.Value.Month >= fromdate.Value.Month && q.ServiceBookDates.ConfirmationDate.Value.Month <= todate.Value.Month).ToList();

                    //    foreach (var item in ServiceBookData1)
                    //    {
                    //        if (fromdate.Value.Year - item.ServiceBookDates.ConfirmationDate.Value.Year != 0)
                    //        {

                    //            var Fulldate = item.ServiceBookDates.ConfirmationDate.Value.ToString("dd/MM") + "/" + fromdate.Value.Year;

                    //            DateTime newdate = Convert.ToDateTime(Fulldate);


                    //            if (newdate >= fromdate && newdate <= todate)
                    //            {
                    //                var EmployeeList = db.EmployeeLeave.Select(e => e.Employee)
                    //                             .Where(e => e.Id == item.Id).SingleOrDefault();

                    //                EmployeeLeave OEmployeeLeave = null;
                    //                OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                    //   .Include(e => e.LvNewReq.Select(r => r.LeaveHead)).Include(e => e.LvNewReq.Select(r => r.LeaveCalendar)).Where(e => e.Employee.Id == item.Id).SingleOrDefault();

                    //                var LvNewReqDatacreditedempnotload = OEmployeeLeave.LvNewReq.Where(q => q.LvCreditDate >= fromdate && q.LvCreditDate <= todate && lvheadids.Contains(q.LeaveHead.Id)).ToList();
                    //                if (LvNewReqDatacreditedempnotload.Count() == 0)
                    //                {

                    //                    if (EmployeeList != null)
                    //                    {
                    //                        if (!model.Select(q => q.Id).Contains(item.Id))
                    //                        {
                    //                            P2BCrGridData pb = new P2BCrGridData()
                    //                            {
                    //                                Id = EmployeeList.Id,
                    //                                Code = EmployeeList.EmpCode,
                    //                                Name = EmployeeList.EmpName.FullNameFML,
                    //                                LvCreditDate = newdate.ToShortDateString()
                    //                            };
                    //                            // EmpList1.Add(EmployeeList);
                    //                            if (EmployeeList.ServiceBookDates.ServiceLastDate == null)
                    //                            {
                    //                                model.Add(pb);
                    //                            }
                    //                            else if (Convert.ToDateTime("01/" + EmployeeList.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + PayMonth))
                    //                            {
                    //                                model.Add(pb);
                    //                            }
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                }

                else if (CreditDatelist.LookupVal.ToUpper() == "INCREMENTDATE")
                {

                    var Emp_lvcr = db.EmployeePayroll
                        .Include(a => a.Employee)
                        .Include(e => e.IncrementServiceBook)
                        .Include(e => e.IncrementServiceBook.Select(q => q.IncrActivity))
                        .ToList();

                    foreach (var item in Emp_lvcr)
                    {
                        var fgds = item.IncrementServiceBook.Where(q => q.ReleaseDate != null
                            && q.IncrActivity.Id == 1).OrderBy(q => q.Id).LastOrDefault();

                        if (fgds != null)
                        {
                            if (fgds.ReleaseDate.Value.Month == fromdate.Value.Month)
                            {
                                // EmpList1.Add(item.Employee);
                                var Fulldate = fgds.ReleaseDate.Value.ToString("dd/MM") + "/" + fromdate.Value.Year;

                                P2BCrGridData pb = new P2BCrGridData()
                                {
                                    Id = item.Employee.Id,
                                    Code = item.Employee.EmpCode,
                                    Name = item.Employee.EmpName.FullNameFML,
                                    LvCreditDate = Fulldate
                                };
                                model.Add(pb);

                            }
                        }
                    }


                    var ServiceBookData = db.Employee.Include(e => e.ServiceBookDates).Where(q => q.ServiceBookDates.LastIncrementDate == null).ToList();

                    if (ServiceBookData.Count() > 0)
                    {
                        var ServiceBookData1 = ServiceBookData.Where(q => q.ServiceBookDates.JoiningDate.Value.Month >= fromdate.Value.Month && q.ServiceBookDates.JoiningDate.Value.Month <= todate.Value.Month).ToList();

                        foreach (var item in ServiceBookData1)
                        {

                            if (fromdate.Value.Year - item.ServiceBookDates.JoiningDate.Value.Year != 0)
                            {
                                var Fulldate = item.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + fromdate.Value.Year;

                                DateTime newdate = Convert.ToDateTime(Fulldate);


                                if (newdate >= fromdate && newdate <= todate)
                                {
                                    var EmployeeList = db.EmployeeLeave.Select(e => e.Employee)
                                                 .Where(e => e.Id == item.Id).SingleOrDefault();

                                    if (EmployeeList != null)
                                    {
                                        if (!model.Select(q => q.Id).Contains(item.Id))
                                        {

                                            P2BCrGridData pb = new P2BCrGridData()
                                            {
                                                Id = EmployeeList.Id,
                                                Code = EmployeeList.EmpCode,
                                                Name = EmployeeList.EmpName.FullNameFML,
                                                LvCreditDate = newdate.ToShortDateString()
                                            };
                                            // EmpList1.Add(EmployeeList);
                                            if (EmployeeList.ServiceBookDates.ServiceLastDate == null)
                                            {
                                                model.Add(pb);
                                            }
                                            else if (Convert.ToDateTime("01/" + EmployeeList.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + PayMonth))
                                            {
                                                model.Add(pb);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

                else if (CreditDatelist.LookupVal.ToUpper() == "PROMOTIONDATE")
                {

                    var Emp_lvcr = db.EmployeePayroll
                       .Include(a => a.Employee)
                       .Include(e => e.PromotionServiceBook)
                       .Include(e => e.PromotionServiceBook.Select(q => q.PromotionActivity))
                       .ToList();

                    foreach (var item in Emp_lvcr)
                    {
                        var fgds = item.PromotionServiceBook.Where(q => q.ReleaseDate != null
                            && q.PromotionActivity.Id == 2).OrderBy(q => q.Id).LastOrDefault();

                        if (fgds != null)
                        {
                            if (fgds.ReleaseDate.Value.Month == fromdate.Value.Month)
                            {
                                //EmpList1.Add(item.Employee);
                                var Fulldate = fgds.ReleaseDate.Value.ToString("dd/MM") + "/" + fromdate.Value.Year;

                                P2BCrGridData pb = new P2BCrGridData()
                                {
                                    Id = item.Employee.Id,
                                    Code = item.Employee.EmpCode,
                                    Name = item.Employee.EmpName.FullNameFML,
                                    LvCreditDate = Fulldate
                                };
                                model.Add(pb);

                            }

                        }

                    }

                    var ServiceBookData = db.Employee.Include(e => e.ServiceBookDates).Where(q => q.ServiceBookDates.LastPromotionDate == null).ToList();

                    if (ServiceBookData.Count() > 0)
                    {
                        var ServiceBookData1 = ServiceBookData.Where(q => q.ServiceBookDates.JoiningDate.Value.Month >= fromdate.Value.Month && q.ServiceBookDates.JoiningDate.Value.Month <= todate.Value.Month).ToList();

                        foreach (var item in ServiceBookData1)
                        {
                            if (fromdate.Value.Year - item.ServiceBookDates.JoiningDate.Value.Year != 0)
                            {
                                var Fulldate = item.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + fromdate.Value.Year;

                                DateTime newdate = Convert.ToDateTime(Fulldate);


                                if (newdate >= fromdate && newdate <= todate)
                                {
                                    var EmployeeList = db.EmployeeLeave.Select(e => e.Employee)
                                                 .Where(e => e.Id == item.Id).SingleOrDefault();

                                    if (EmployeeList != null)
                                    {
                                        if (!model.Select(q => q.Id).Contains(item.Id))
                                        {
                                            P2BCrGridData pb = new P2BCrGridData()
                                            {
                                                Id = EmployeeList.Id,
                                                Code = EmployeeList.EmpCode,
                                                Name = EmployeeList.EmpName.FullNameFML,
                                                LvCreditDate = newdate.ToShortDateString()
                                            };
                                            // EmpList1.Add(EmployeeList);
                                            if (EmployeeList.ServiceBookDates.ServiceLastDate == null)
                                            {
                                                model.Add(pb);
                                            }
                                            else if (Convert.ToDateTime("01/" + EmployeeList.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + PayMonth))
                                            {
                                                model.Add(pb);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                else if (CreditDatelist.LookupVal.ToUpper() == "FIXDAYS")
                {
                    // from leave structure start
                    foreach (var z in emp)
                    {

                        int OEmpLvID = db.EmployeeLeave.Where(e => e.Employee.Id == z.Id).FirstOrDefault().Id;

                        //EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                        //      .Include(e => e.EmployeeLvStructDetails)
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.CreditDate))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.LvHead))
                        //      .Where(e => e.EndDate == null && e.EmployeeLeave_Id == OEmpLvID).AsNoTracking().FirstOrDefault();
                        var OLvSalStruct = db.EmployeeLvStruct.Select(d => new
                        {
                            OEndDate = d.EndDate,
                            OEffectiveDate = d.EffectiveDate,
                            OEmployeeLeaveId = d.EmployeeLeave_Id,
                            OEmployeeLvStructDetails = d.EmployeeLvStructDetails.Select(r => new
                            {
                                OLvHeadFormula = r.LvHeadFormula,
                                OEmployeeLvStructDetailsLvHeadId = r.LvHead_Id,
                                OEmployeeLvStructDetailsLvHead = r.LvHead,
                                OLvCreditPolicy = r.LvHeadFormula.LvCreditPolicy,
                                OCreditDate = r.LvHeadFormula.LvCreditPolicy.CreditDate,
                                OLvCreditPolicyLvHead = r.LvHeadFormula.LvCreditPolicy.LvHead,

                            }).ToList()
                        }).Where(e => e.OEndDate == null && e.OEmployeeLeaveId == OEmpLvID).SingleOrDefault();


                        LvCreditPolicy oLvCreditPolicy = null;
                        if (OLvSalStruct != null)
                        {


                            foreach (var LvHead_ids in lvheadids)
                            {
                                double mSusDayFull = 0;
                                int lastyearid = 0;
                                var EmpLvHeadList = db.LvHead.Where(e => e.Id == LvHead_ids)
                                    .Select(a => new { Id = a.Id, LvCode = a.LvCode }).Distinct().ToList();


                                foreach (var item in EmpLvHeadList)
                                {
                                    //  oLvCreditPolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == item.Id && e.LvHeadFormula != null && e.LvHeadFormula.LvCreditPolicy != null).Select(r => r.LvHeadFormula.LvCreditPolicy).FirstOrDefault();
                                    oLvCreditPolicy = OLvSalStruct.OEmployeeLvStructDetails.Where(e => e.OEmployeeLvStructDetailsLvHeadId == item.Id && e.OLvHeadFormula != null && e.OLvCreditPolicy != null).Select(r => r.OLvCreditPolicy).FirstOrDefault();
                                    if (oLvCreditPolicy != null && oLvCreditPolicy.CreditDate.LookupVal.ToUpper() == "FIXDAYS")
                                    {
                                        //check leave credit or not which not credit that employee load 
                                        var LvNewReqData = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == z.Id).AsNoTracking().FirstOrDefault();
                                        LvNewReq aa = null;
                                        DateTime lvnxcrdt;
                                        if (LvNewReqData != null)
                                        {
                                            lvnxcrdt = System.DateTime.Now;
                                            //&&  q.LvCreditNextDate <= lvnxcrdt
                                            aa = LvNewReqData.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvOrignal_Id == null && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                            if (aa != null)
                                            {
                                                if (aa.LvCreditNextDate.Value.Date <= lvnxcrdt.Date)
                                                {
                                                    if (aa.LvCreditNextDate.Value.Date >= fromdate.Value.Date && aa.LvCreditNextDate.Value.Date <= todate.Value.Date)
                                                    {
                                                        if (!model.Select(q => q.Id).Contains(z.Id))
                                                        {
                                                            view = new P2BCrGridData()
                                                            {
                                                                Id = z.Id,
                                                                Code = z.EmpCode,
                                                                Name = z.EmpName.FullNameFML,

                                                            };
                                                            model.Add(view);

                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            else// first time leave credit check lvstruct credit policy defien or not. if define then leave credit
                                            {
                                                if (!model.Select(q => q.Id).Contains(z.Id))
                                                {
                                                    view = new P2BCrGridData()
                                                    {
                                                        Id = z.Id,
                                                        Code = z.EmpCode,
                                                        Name = z.EmpName.FullNameFML,

                                                    };
                                                    model.Add(view);

                                                    break;
                                                }
                                            }
                                        }
                                        else // Credit Policy available first time Leave Credit
                                        {
                                            if (!model.Select(q => q.Id).Contains(z.Id))
                                            {
                                                view = new P2BCrGridData()
                                                {
                                                    Id = z.Id,
                                                    Code = z.EmpCode,
                                                    Name = z.EmpName.FullNameFML,

                                                };
                                                model.Add(view);

                                                break;
                                            }
                                        }

                                    }
                                }
                            }
                        }

                    }
                    // from leave structure end

                    //foreach (var z in emp)
                    //{
                    //    view = new P2BCrGridData()
                    //    {
                    //        Id = z.Id,
                    //        Code = z.EmpCode,
                    //        Name = z.EmpName.FullNameFML,

                    //    };
                    //    if (z.ServiceBookDates.ServiceLastDate == null)
                    //    {
                    //        model.Add(view);
                    //    }
                    //    else if (Convert.ToDateTime("01/" + z.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + PayMonth))
                    //    {
                    //        model.Add(view);
                    //    }
                    //}
                }
                else
                {
                    // from leave structure start
                    foreach (var z in emp)
                    {

                        int OEmpLvID = db.EmployeeLeave.Where(e => e.Employee.Id == z.Id).FirstOrDefault().Id;

                        //EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                        //      .Include(e => e.EmployeeLvStructDetails)
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.CreditDate))
                        //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.LvHead))
                        //      .Where(e => e.EndDate == null && e.EmployeeLeave_Id == OEmpLvID).AsNoTracking().FirstOrDefault();
                        var OLvSalStruct = db.EmployeeLvStruct.Select(d => new
                        {
                            OEndDate = d.EndDate,
                            OEffectiveDate = d.EffectiveDate,
                            OEmployeeLeaveId = d.EmployeeLeave_Id,
                            OEmployeeLvStructDetails = d.EmployeeLvStructDetails.Select(r => new
                            {
                                OLvHeadFormula = r.LvHeadFormula,
                                OEmployeeLvStructDetailsLvHeadId = r.LvHead_Id,
                                OEmployeeLvStructDetailsLvHead = r.LvHead,
                                OLvCreditPolicy = r.LvHeadFormula.LvCreditPolicy,
                                OCreditDate = r.LvHeadFormula.LvCreditPolicy.CreditDate,
                                OLvCreditPolicyLvHead = r.LvHeadFormula.LvCreditPolicy.LvHead,

                            }).ToList()
                        }).Where(e => e.OEndDate == null && e.OEmployeeLeaveId == OEmpLvID).FirstOrDefault();


                        LvCreditPolicy oLvCreditPolicy = null;
                        if (OLvSalStruct != null)
                        {


                            foreach (var LvHead_ids in lvheadids)
                            {
                                double mSusDayFull = 0;
                                int lastyearid = 0;
                                var EmpLvHeadList = db.LvHead.Where(e => e.Id == LvHead_ids)
                                    .Select(a => new { Id = a.Id, LvCode = a.LvCode }).Distinct().ToList();


                                foreach (var item in EmpLvHeadList)
                                {
                                    //  oLvCreditPolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == item.Id && e.LvHeadFormula != null && e.LvHeadFormula.LvCreditPolicy != null).Select(r => r.LvHeadFormula.LvCreditPolicy).FirstOrDefault();
                                    oLvCreditPolicy = OLvSalStruct.OEmployeeLvStructDetails.Where(e => e.OEmployeeLvStructDetailsLvHeadId == item.Id && e.OLvHeadFormula != null && e.OLvCreditPolicy != null).Select(r => r.OLvCreditPolicy).FirstOrDefault();
                                    if (oLvCreditPolicy != null && oLvCreditPolicy.CreditDate.LookupVal.ToUpper() == CreditDatelist.LookupVal.ToUpper())
                                    {
                                        //check leave credit or not which not credit that employee load 
                                        var LvNewReqData = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == z.Id).AsNoTracking().FirstOrDefault();
                                        LvNewReq aa = null;
                                        DateTime lvnxcrdt;
                                        if (LvNewReqData != null)
                                        {
                                            lvnxcrdt = System.DateTime.Now;
                                            //&&  q.LvCreditNextDate <= lvnxcrdt
                                            aa = LvNewReqData.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvOrignal_Id == null && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                            if (aa != null)
                                            {
                                                if (aa.LvCreditNextDate.Value.Date <= lvnxcrdt.Date)
                                                {
                                                    if (aa.LvCreditNextDate.Value.Date >= fromdate.Value.Date && aa.LvCreditNextDate.Value.Date <= todate.Value.Date)
                                                    {
                                                        if (!model.Select(q => q.Id).Contains(z.Id))
                                                        {
                                                            view = new P2BCrGridData()
                                                            {
                                                                Id = z.Id,
                                                                Code = z.EmpCode,
                                                                Name = z.EmpName.FullNameFML,

                                                            };
                                                            model.Add(view);

                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            else// first time leave credit check lvstruct credit policy defien or not. if define then leave credit
                                            {
                                                if (!model.Select(q => q.Id).Contains(z.Id))
                                                {
                                                    view = new P2BCrGridData()
                                                    {
                                                        Id = z.Id,
                                                        Code = z.EmpCode,
                                                        Name = z.EmpName.FullNameFML,

                                                    };
                                                    model.Add(view);

                                                    break;
                                                }
                                            }
                                        }
                                        else // Credit Policy available first time Leave Credit
                                        {
                                            if (!model.Select(q => q.Id).Contains(z.Id))
                                            {
                                                view = new P2BCrGridData()
                                                {
                                                    Id = z.Id,
                                                    Code = z.EmpCode,
                                                    Name = z.EmpName.FullNameFML,

                                                };
                                                model.Add(view);

                                                break;
                                            }
                                        }

                                    }
                                }
                            }
                        }

                    }
                    // from leave structure end

                    //var LvNewReqData = db.LvNewReq.Include(x => x.LeaveHead).Where(q => q.LvCreditDate != null).ToList();

                    //if (LvNewReqData != null)
                    //{
                    //    LvNewReqData.Where(q => q.LvCreditDate >= fromdate && q.LvCreditDate <= todate && lvheadids.Contains(q.LeaveHead.Id)).ToList();

                    //    var Emp_lvcr = db.EmployeeLeave.Include(a => a.Employee).Include(e => e.LvNewReq).ToList();

                    //    foreach (var item in Emp_lvcr)
                    //    {
                    //        var fgds = item.LvNewReq.Where(q => LvNewReqData.Select(t => t.Id).Contains(q.Id));

                    //        foreach (var item1 in fgds)
                    //        {
                    //            if (item1 != null)
                    //            {
                    //                EmpList1.Add(item.Employee);
                    //            }
                    //        }
                    //    }
                    //}


                    //foreach (var z in EmpList1)
                    //{
                    //    view = new P2BCrGridData()
                    //    {
                    //        Id = z.Id,
                    //        Code = z.EmpCode,
                    //        Name = z.EmpName.FullNameFML
                    //    };
                    //    if (z.ServiceBookDates.ServiceLastDate == null)
                    //    {
                    //        model.Add(view);
                    //    }
                    //    else if (Convert.ToDateTime("01/" + z.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + PayMonth))
                    //    {
                    //        model.Add(view);
                    //    }
                    //}
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
                                || (e.LvCreditDate.ToString().Contains(gp.searchString))
                                || (e.Status.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.Code, a.Name, a.LvCreditDate, a.Status, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.LvCreditDate, a.Status, a.Id }).ToList();
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
                                         gp.sidx == "EmpName" ? c.Name.ToString() :
                                         gp.sidx == "Creditdate" ? c.LvCreditDate.ToString() :
                                         gp.sidx == "Status" ? c.Status.ToString() : ""
                        );
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.LvCreditDate, a.Status, a.Id }).ToList();
                    }

                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.LvCreditDate, a.Status, a.Id }).ToList();
                    }

                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.LvCreditDate, a.Status, a.Id }).ToList();
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
                System.Web.HttpContext.Current.Session["LeaveCreditProcessEmpids"] = JsonData;
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult LoadEmpWithCreditDate(P2BGrid_Parameters gp)
        {
            try
            {
                DateTime? dt = null;
                string monthyr = "";
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<P2BCrGridData> EmpList = null;
                List<P2BCrGridData> model = new List<P2BCrGridData>();
                P2BCrGridData view = null;
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

                //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var empdata = db.CompanyPayroll
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates)).AsNoTracking()
                    .Where(e => e.Company.Id == compid).SingleOrDefault();

                var emp = empdata.EmployeePayroll.Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null || e.Employee.ServiceBookDates.ServiceLastDate >= System.DateTime.Now.Date).Select(e => e.Employee).ToList();

                foreach (var z in emp)
                {
                    var EmplvList1 = db.EmployeeLeave.Include(e => e.LvNewReq).AsNoTracking().Where(e => e.Employee.Id == z.Id && e.LvNewReq.Any(a => a.LvCreditDate != null)).SingleOrDefault();
                    if (EmplvList1 != null)
                    {
                        view = new P2BCrGridData()
                        {
                            Id = z.Id,
                            Code = z.EmpCode,
                            Name = z.EmpName.FullNameFML
                        };
                        if (z.ServiceBookDates.ServiceLastDate == null || Convert.ToDateTime("01/" + z.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + PayMonth))
                        {
                            //var EmplvList = db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvNewReq.Select(a => a.WFStatus)).AsNoTracking().Where(e => e.Employee.Id == z.Id && e.LvNewReq.Any(a => a.LvCreditDate != null)).SingleOrDefault();
                            var EmplvList = db.LvNewReq.Include(e => e.WFStatus).Where(e => e.EmployeeLeave_Id == EmplvList1.Id && e.LvCreditDate != null && e.WFStatus.LookupVal == "3").OrderByDescending(e => e.Id).FirstOrDefault();

                            if (EmplvList != null)
                            {
                                // var LvNewReqList = EmplvList.LvNewReq.Where(e => e.LvCreditDate != null && e.WFStatus.LookupVal == "3").OrderByDescending(e => e.Id).FirstOrDefault();
                                //if (LvNewReqList != null)
                                //{
                                view.LvCreditDate = EmplvList.LvCreditDate.Value.ToShortDateString();
                                view.Status = "true";

                                //}
                            }
                            model.Add(view);
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
                                || (e.LvCreditDate != null ? e.LvCreditDate.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                                || (e.Status != null ? e.Status.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.Code, a.Name, a.LvCreditDate != null ? a.LvCreditDate : "", a.Status != null ? a.Status : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.LvCreditDate, a.Status, a.Id }).ToList();
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
                                         gp.sidx == "EmpName" ? c.Name.ToString() :
                                         gp.sidx == "LvCreditDate" ? c.LvCreditDate.ToString() :
                                         gp.sidx == "Status" ? c.Status.ToString() : ""

                                         );
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.LvCreditDate, a.Status, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.LvCreditDate, a.Status, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.LvCreditDate, a.Status, a.Id }).ToList();
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
        //public ActionResult GetLvhead()
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var a = db.LvHead.ToList();
        //        if (a.Count > 0)
        //        {
        //            SelectList s = new SelectList(a.ToList(), "id", "FullDetails");
        //            return Json(s, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(null, JsonRequestBehavior.AllowGet);

        //        }
        //    }
        //}


        public ActionResult PendingLvCheck(String LvHead, String pendingleaveempids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //string parm = Request["emplvid"].ToString();
                List<int> ids = new List<int>();
                List<dynamic> Empids = JsonConvert.DeserializeObject<List<dynamic>>(pendingleaveempids);

                foreach (dynamic item in Empids)
                {
                    int ids1 = Convert.ToInt32(item);
                    ids.Add(ids1);
                }

                if (ids == null)
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "Please select the employee and try again..!" }, JsonRequestBehavior.AllowGet);
                }

                Session["EmployeeLvIdReport"] = ids;
                //int emlvid = Convert.ToInt32(Empids);
                //List<int> ids = null;
                //if (Empids != null && Empids != "0" && Empids != "false")
                //{
                //    ids = Utility.StringIdsToListIds(Empids);
                //}            
                //List<int> lvheadids = null;

                //string lvheadList = form["LvHead"] == "0" ? "" : form["LvHead"];

                //if (param != null && param != "0" && param != "false")
                //{
                //    // ids = Utility.StringIdsToListString(parm);
                //    ids = Utility.StringIdsToListIds(param);
                //}


                //if (lvheadList != null && lvheadList != "0" && lvheadList != "false")
                //{
                //    lvheadids = Utility.StringIdsToListIds(lvheadList);
                //}

                // var emp = db.Employee.Where(e => ids.Contains(e.EmpCode)).ToList().Select(q => q.Id);

                List<dynamic> lvheadid = JsonConvert.DeserializeObject<List<dynamic>>(LvHead);

                List<int> lvheadids = new List<int>();

                foreach (dynamic item1 in lvheadid)
                {
                    int lvheadids1 = Convert.ToInt32(item1);
                    lvheadids.Add(lvheadids1);
                }

                var emp = db.Employee.Where(e => ids.Contains(e.Id)).ToList().Select(q => q.Id);
                foreach (var oEmployeeId in emp)
                {
                    EmployeeLeave oEmployeeLeave = db.EmployeeLeave
                        // .Include(e => e.LvNewReq)
                        //.Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                        //.Include(e => e.LvNewReq.Select(a => a.WFStatus))
                  .Where(e => e.Employee_Id == oEmployeeId).SingleOrDefault();

                    //var LvCalendarFilter = oEmployeeLeave.LvNewReq.OrderBy(e => e.Id).ToList();

                    // var LvOrignal_id = LvCalendarFilter.Where(e => e.LvOrignal != null).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();
                    var LvOrignal_id = db.LvNewReq.Where(e => e.EmployeeLeave_Id == oEmployeeLeave.Id && e.LvOrignal != null).Select(e => e.LvOrignal.Id).ToList();

                    //EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == oEmployeeId)
                    //               .Include(a => a.LvNewReq)
                    //               .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                    //               .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                    //               .Include(a => a.LvNewReq.Select(e => e.GeoStruct))
                    //               .Include(a => a.LvNewReq.Select(e => e.PayStruct))
                    //               .Include(a => a.LvNewReq.Select(e => e.FuncStruct))
                    //               .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                    //               .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.TrClosed == false && a.WFStatus.LookupVal != "0" && a.WFStatus.LookupVal != "2" && a.WFStatus.LookupVal != "3" && lvheadids.Contains(a.LeaveHead.Id)))
                    //                   .SingleOrDefault();
                    var _Prv_EmpLvData = db.LvNewReq.Select(d => new
                    {
                        Id = d.Id,
                        EmployeeLeave_Id = d.EmployeeLeave_Id,
                        LeaveHead = d.LeaveHead,
                        WFStatus = d.WFStatus.LookupVal,
                        TrClosed = d.TrClosed,
                        Narration = d.Narration,
                        InputMethod = d.InputMethod
                    })
                        .Where(a => a.EmployeeLeave_Id == oEmployeeLeave.Id && a.LeaveHead != null && a.TrClosed == false && a.WFStatus != "0" && a.WFStatus != "2" && a.WFStatus != "3"
                            && lvheadids.Contains(a.LeaveHead.Id)
                            && !LvOrignal_id.Contains(a.Id) && a.Narration.ToUpper() == "Leave Requisition".ToUpper()
                            && (a.InputMethod == 1 || a.InputMethod == 2)
                            ).ToList();

                    //if (_Prv_EmpLvData != null)
                    //{
                    //  var prev_emplvdata_final = _Prv_EmpLvData.LvNewReq.Where(e => !LvOrignal_id.Contains(e.Id) && e.Narration.ToUpper() == "Leave Requisition".ToUpper()).ToList();

                    if (_Prv_EmpLvData != null && _Prv_EmpLvData.Count() > 0)
                    {
                        //foreach (var ca1 in _Prv_EmpLvData)
                        //{
                        //    if (ca1.TrClosed == false && (ca1.InputMethod == 1 || ca1.InputMethod == 2))
                        //    {
                        bool status1 = true;
                        var data1 = new
                        {
                            status = status1,

                        };
                        return Json(data1, JsonRequestBehavior.AllowGet);
                        //    }
                        //}


                        // }
                    }
                }
                //var LvNewReqData = db.LvNewReq.Where(e => e.InputMethod == 1 && e.TrClosed == false && lvheadids.Contains(e.LeaveHead.Id)).ToList();

                //if (LvNewReqData.Count() > 0)
                //{
                //    bool status1 = true;
                //    var data1 = new
                //    {
                //        status = status1,

                //    };
                //    return Json(data1, JsonRequestBehavior.AllowGet);
                //}
                return null;
            }
        }


        [HttpPost]
        public ActionResult GetLvhead(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool exists = System.IO.Directory.Exists(requiredPath);
                string localPath;
                if (!exists)
                {
                    localPath = new Uri(requiredPath).LocalPath;
                    System.IO.Directory.CreateDirectory(localPath);
                }
                string path = requiredPath + @"\ConvertLeaveLock" + ".ini";
                localPath = new Uri(path).LocalPath;
                if (!System.IO.File.Exists(localPath))
                {

                    using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                    {
                        StreamWriter str = new StreamWriter(fs);
                        str.BaseStream.Seek(0, SeekOrigin.Begin);

                        str.Flush();
                        str.Close();
                        fs.Close();
                    }


                }

                int forwardata = Convert.ToInt32(data);

                var dd = db.LookupValue.Where(e => e.Id == forwardata).Select(q => q.LookupVal).SingleOrDefault();
                if (dd.ToUpper() == "CALENDAR")
                {

                    var a = db.LvCreditPolicy.Include(e => e.LvHead).Where(e => e.CreditDate.Id == forwardata).ToList();
                    if (a.Count > 0)
                    {
                        //  SelectList s = new SelectList(a.Select(e => e.LvHead).ToList(), "id", "FullDetails");
                        SelectList s = new SelectList(a.Select(e => e.LvHead).Distinct().ToList(), "id", "FullDetails");
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    var a = db.LvCreditPolicy.Include(e => e.LvHead).Where(e => e.CreditDate.Id == forwardata).ToList();
                    if (a.Count > 0)
                    {

                        SelectList s = new SelectList(a.Select(e => e.LvHead).Distinct().ToList(), "id", "FullDetails");
                        //SelectList s = new SelectList(a.Select(e => e.LvHead).ToList(), "id", "FullDetails");
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(null, JsonRequestBehavior.AllowGet);

                    }
                }
                return null;
            }
        }
        [HttpPost]
        public ActionResult Create(FormCollection form, String forwarddata)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    string Emp = forwarddata == "0" ? "" : forwarddata;
                    string Lvhead = form["LvHead"] == "" ? "0" : Convert.ToString(form["LvHead"]);
                    string FromDate1 = form["FromDate"] == "" ? "" : form["FromDate"];
                    string ToDate1 = form["ToDate"] == "" ? "" : form["ToDate"];
                    string CreditDatelist = form["CreditDatelist"] == "" ? "0" : Convert.ToString(form["CreditDatelist"]);
                    DateTime? FromDate = null;
                    DateTime? ToDate = null;
                    if (FromDate1 != "")
                    {
                        FromDate = Convert.ToDateTime(FromDate1);
                        ToDate = Convert.ToDateTime(ToDate1);

                    }

                    List<int> ids = null;
                    List<int> LvHead_ids = null;

                    if (Emp == "null")
                    {
                        return Json(new Utility.JsonReturnClass { success = false, responseText = "Please select the employee and try again..!" }, JsonRequestBehavior.AllowGet);
                    }

                    else
                    {
                        ids = Utility.StringIdsToListIds(Emp);
                    }

                    if (Lvhead != null && Lvhead != "0")
                    {
                        LvHead_ids = Utility.StringIdsToListIds(Lvhead);
                    }


                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }
                    //DateTime? joinincrpromdate=
                    CompanyPayroll OCompanyPayroll = null;
                    OCompanyPayroll = db.CompanyPayroll.Include(e => e.Company).Include(e => e.Company.Calendar).Include(e => e.Company.Calendar.Select(r => r.Name)).Where(e => e.Company.Id == CompId).SingleOrDefault();


                    //var LeavenewReq = db.LvNewReq.Where(e => e.InputMethod == 1 && e.TrClosed == false && LvHead_ids.Contains(e.LeaveHead.Id)).ToList();

                    //if (LeavenewReq.Count() > 0)
                    //{
                    //    var url = "rptview/rptpendingleavecreditlvr?parm=22";
                    //    return JavaScript("alert('Please Approve the pending leave and try again..!!') ;window.location = '" + url + "'");
                    //}

                    var emp = db.Employee.Where(e => ids.Contains(e.Id)).ToList().Select(q => q.Id);
                    int pend = 0;
                    foreach (var oEmployeeId in emp)
                    {
                        EmployeeLeave oEmployeeLeavechk = db.EmployeeLeave
                            //.Include(e => e.LvNewReq)
                            //.Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                            //.Include(e => e.LvNewReq.Select(a => a.WFStatus))
                            .Where(e => e.Employee.Id == oEmployeeId).OrderBy(e => e.Id).SingleOrDefault();
                        //var LvCalendarFilter = oEmployeeLeavechk.LvNewReq.OrderBy(e => e.Id).ToList();

                        //var LvOrignal_id = LvCalendarFilter.Where(e => e.LvOrignal != null).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();
                        var LvOrignal_id = db.LvNewReq.Where(e => e.EmployeeLeave_Id == oEmployeeLeavechk.Id && e.LvOrignal != null).Select(e => e.LvOrignal.Id).ToList();

                        //EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == oEmployeeId)
                        //    .Include(a => a.Employee)
                        //               .Include(a => a.LvNewReq)
                        //               .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                        //               .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                        //               .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                        //               .Include(e => e.LvNewReq.Select(q => q.LvWFDetails))
                        //                .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.TrClosed == false && a.WFStatus.LookupVal != "0" && a.WFStatus.LookupVal != "2" && a.WFStatus.LookupVal != "3" && LvHead_ids.Contains(a.LeaveHead.Id)))
                        //                   .SingleOrDefault();

                        var _Prv_EmpLvData = db.LvNewReq.Select(d => new
                        {
                            Id = d.Id,
                            EmployeeLeave_Id = d.EmployeeLeave_Id,
                            LeaveHead = d.LeaveHead,
                            LvCode = d.LeaveHead.LvCode,
                            WFStatus = d.WFStatus.LookupVal,
                            TrClosed = d.TrClosed,
                            Narration = d.Narration,
                            InputMethod = d.InputMethod,
                            LvWFDetails = d.LvWFDetails.ToList(),
                            EmpCode = d.EmployeeLeave.Employee.EmpCode,
                            FromDate = d.FromDate,
                            ToDate = d.ToDate,
                        }).Where(a => a.EmployeeLeave_Id == oEmployeeLeavechk.Id && a.LeaveHead != null && a.TrClosed == false && a.WFStatus != "0" && a.WFStatus != "2" && a.WFStatus != "3"
                           && LvHead_ids.Contains(a.LeaveHead.Id)
                           && !LvOrignal_id.Contains(a.Id) && a.Narration.ToUpper() == "Leave Requisition".ToUpper()
                           && (a.InputMethod == 1 || a.InputMethod == 2)
                           ).ToList();

                        //if (_Prv_EmpLvData != null)
                        //{
                        // var Lv = _Prv_EmpLvData.LvNewReq.Where(a => LvHead_ids.Contains(a.LeaveHead.Id)).ToList();
                        //var Lv = _Prv_EmpLvData.LvNewReq.Where(e => !LvOrignal_id.Contains(e.Id) && e.Narration.ToUpper() == "Leave Requisition".ToUpper()).ToList();
                        foreach (var ca1 in _Prv_EmpLvData)
                        {
                            //if (ca1.TrClosed == false && (ca1.InputMethod == 1 || ca1.InputMethod == 2))
                            //{
                            pend = 1;
                            foreach (var ca2 in ca1.LvWFDetails)
                            {
                                Msg.Add("Please Approve the pending leave and try again..!!");
                                //Msg.Add(_Prv_EmpLvData.Employee.EmpCode + "-" + ca1.FromDate + " - " + ca1.ToDate + " - " + ca1.LeaveHead.LvCode);
                                Msg.Add(ca1.EmpCode + "-" + ca1.FromDate + " - " + ca1.ToDate + " - " + ca1.LvCode);
                            }
                            //}
                        }


                        //}


                    }
                    if (pend == 1)
                    {
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    Employee OEmployee = null;
                    EmployeeLeave oEmployeeLeave = null;
                    var CompLvId = Convert.ToInt32(SessionManager.CompLvId);
                    //using (DataBaseContext db = new DataBaseContext())
                    //{
                    //var CompCreditPolicyList = db.CompanyLeave
                    //     .Include(e => e.LvCreditPolicy)
                    //     .Include(e => e.LvCreditPolicy.Select(a => a.ConvertLeaveHead))
                    //     .Include(e => e.LvCreditPolicy.Select(a => a.LvHead))
                    //     .Include(e => e.LvCreditPolicy.Select(a => a.CreditDate))
                    //     .Include(e => e.LvCreditPolicy.Select(a => a.ConvertLeaveHeadBal))
                    //     .Include(e => e.LvCreditPolicy.Select(a => a.ExcludeLeaveHeads))
                    //     .Where(e => e.Id == CompLvId).SingleOrDefault();
                    var lvcalendarid = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").Select(e => e.Id).SingleOrDefault();

                    if (ids != null)
                    {
                        //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                        //               new System.TimeSpan(0, 30, 0)))
                        //{
                        foreach (var i in ids)
                        {
                            //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                            //                                                                new System.TimeSpan(0, 30, 0)))
                            //{
                            //foreach (var LvHead_id in LvHead_ids)
                            //{
                            // Int32 _returnParm = Delete_CreditRecord(i, lvcalendarid, LvHead_id);
                            // delete function comment because converted leave has to delete for new year
                            //if (_returnParm == 0)
                            //{
                            //    continue;
                            //}

                            var ErrNo = LvCreditProcessController.LvCreditProceess(i, CompLvId, null, LvHead_ids, lvcalendarid, 1, OCompanyPayroll.Id, FromDate, ToDate, CreditDatelist, false);

                            var ErrNoLB = LvCreditProcessController.LvCreditProceessLeaveBank(i, CompLvId, null, LvHead_ids, lvcalendarid, 1, OCompanyPayroll.Id, FromDate, ToDate, CreditDatelist, false);

                            //}
                            //    ts.Complete();
                            //}
                        }
                        //  ts.Complete();
                        string lvcreditprocessmsg = "";
                        dynamic tempmesg = System.Web.HttpContext.Current.Session["LeaveCreditProcessMsg"];
                        if (tempmesg != null)
                        {
                            lvcreditprocessmsg = string.Join(",", tempmesg);
                        }

                        return Json(new Utility.JsonReturnClass { success = true, responseText = "Data Saved SuccessFully..!" + "  " + lvcreditprocessmsg }, JsonRequestBehavior.AllowGet);
                        //}
                    }
                }
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
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
            return View();
        }

        public static Int32 LvCreditProceess(Int32 oEmployeeId, Int32 CompLvId, CompanyLeave CompCreditPolicyLists, List<Int32> LvHead_ids_list, Int32 LvCalendarId, int trial, int compid, DateTime? FromDate, DateTime? ToDate, string CreditDatelist, Boolean settlementemp)
        {
            try
            {
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                                             new System.TimeSpan(0, 30, 0)))
                {
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        int OEmpLvID = db.EmployeeLeave.Where(e => e.Employee.Id == oEmployeeId).Select(e => e.Id).FirstOrDefault();
                        //Employee Lv Data
                        int crd = Convert.ToInt32(CreditDatelist);
                        //string creditdtlist = db.LookupValue.Where(e => e.Id == crd).FirstOrDefault().LookupVal.ToUpper();
                        string creditdtlist = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "447").FirstOrDefault().LookupValues.Where(e => e.Id == crd).FirstOrDefault().LookupVal.ToUpper();
                        foreach (var LvHead_ids in LvHead_ids_list)
                        {
                            var EmpLvHeadList = db.LvHead.Where(e => e.Id == LvHead_ids)
                                .Select(a => new { Id = a.Id, LvCode = a.LvCode }).Distinct().AsParallel().ToList();
                            int lastyearid = 0;
                            double mSusDayFull = 0;


                            Company OCompany = null;
                            OCompany = db.Company.Find(compid);

                            foreach (var item in EmpLvHeadList)
                            {
                                //prev credit process check
                                //end
                                //Get LvCredit Policy For Particular Lv

                                LvNewReq OLvCreditRecord = new LvNewReq();
                                //EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                                //     .Include(e => e.EmployeeLvStructDetails)
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.CreditDate))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ConvertLeaveHead))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.LvHead))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ConvertLeaveHeadBal))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ExcludeLeaveHeads))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead.LvHeadOprationType))
                                //         .Where(e => e.EndDate == null && e.EmployeeLeave_Id == OEmpLvID).SingleOrDefault();
                                var OLvSalStruct = db.EmployeeLvStruct.Select(d => new
                                {
                                    OEndDate = d.EndDate,
                                    OEffectiveDate = d.EffectiveDate,
                                    OEmployeeLeaveId = d.EmployeeLeave_Id,
                                    OEmployeeLvStructDetails = d.EmployeeLvStructDetails.Select(r => new
                                    {
                                        OLvHeadFormula = r.LvHeadFormula,
                                        OEmployeeLvStructDetailsLvHeadId = r.LvHead_Id,
                                        OEmployeeLvStructDetailsLvHead = r.LvHead,
                                        OLvHeadOprationType = r.LvHead.LvHeadOprationType,
                                        OLvBankPolicy = r.LvHeadFormula.LvBankPolicy,
                                        OLvHeadCollection = r.LvHeadFormula.LvBankPolicy.LvHeadCollection,
                                        OLvCreditPolicy = r.LvHeadFormula.LvCreditPolicy,
                                        OCreditDate = r.LvHeadFormula.LvCreditPolicy.CreditDate,
                                        OConvertLeaveHead = r.LvHeadFormula.LvCreditPolicy.ConvertLeaveHead,
                                        OLvCreditPolicyLvHead = r.LvHeadFormula.LvCreditPolicy.LvHead,
                                        OConvertLeaveHeadBal = r.LvHeadFormula.LvCreditPolicy.ConvertLeaveHeadBal,
                                        OExcludeLeaveHeads = r.LvHeadFormula.LvCreditPolicy.ExcludeLeaveHeads

                                    }).ToList()
                                }).Where(e => e.OEndDate == null && e.OEmployeeLeaveId == OEmpLvID).SingleOrDefault();

                                LvCreditPolicy oLvCreditPolicy = null;
                                if (OLvSalStruct != null)
                                {
                                    oLvCreditPolicy = OLvSalStruct.OEmployeeLvStructDetails.Where(e => e.OEmployeeLvStructDetailsLvHeadId == item.Id && e.OLvHeadFormula != null && e.OLvCreditPolicy != null).Select(r => r.OLvCreditPolicy).FirstOrDefault();

                                }

                                if (oLvCreditPolicy == null)
                                {
                                    continue;
                                }
                                if (oLvCreditPolicy.CreditDate.LookupVal.ToUpper() != creditdtlist)
                                {
                                    continue;
                                }
                                // settlement Leave Process start
                                //EmployeeLeave _Prv_EmpLvDatasetemp = db.EmployeeLeave.Where(a => a.Employee.Id == oEmployeeId)
                                //    .Include(a => a.LvNewReq)
                                //    .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                                //    .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                                //    .Include(a => a.LvNewReq.Select(e => e.GeoStruct))
                                //    .Include(a => a.LvNewReq.Select(e => e.PayStruct))
                                //    .Include(a => a.LvNewReq.Select(e => e.FuncStruct))
                                //    .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                                //    .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.LeaveHead.Id == item.Id
                                //        ))
                                //        .SingleOrDefault();
                                // if settlement employee leave credit then leave should not credit
                                var _Prv_EmpLvDatasetemp = db.LvNewReq.Where(a => a.LeaveHead != null && a.LeaveHead.Id == item.Id && a.EmployeeLeave_Id == OEmpLvID && a.Narration == "Settlement Process").FirstOrDefault();

                                if (_Prv_EmpLvDatasetemp != null)
                                {
                                    //var LvCreditedInNewYrChksetemp = _Prv_EmpLvDatasetemp.LvNewReq
                                    //        .Where(a => a.Narration == "Settlement Process"
                                    //        && a.LeaveHead.Id == item.Id).FirstOrDefault();
                                    //if (LvCreditedInNewYrChksetemp != null)
                                    //{
                                    continue;
                                    // }
                                }
                                // settlement Leave Process End

                                DateTime? Lastyear = null, CreditDate = null, tempCreditDate = null, Cal_Wise_Date = null, Lastyearj = null;
                                Double CreditDays = 0, SumDays = 0, oOpenBal = 0, oCloseingbal = 0, LWPDays = 0;
                                Calendar LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                    e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                                DateTime? tempRetireDate = null, tempRetirecrDate = null, leaveyearfrom = null, leaveyearto = null;

                                switch (oLvCreditPolicy.CreditDate.LookupVal.ToUpper())
                                {
                                    case "CALENDAR":
                                        Cal_Wise_Date = LvCalendar.FromDate;
                                        CreditDate = LvCalendar.FromDate.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearid1 = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyear
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearid1 != null)
                                        {

                                            lastyearid = lastyearid1.Id;
                                        }

                                        break;
                                    case "YEARLY":
                                        Cal_Wise_Date = LvCalendar.FromDate;
                                        CreditDate = LvCalendar.FromDate.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearid11 = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyear
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearid11 != null)
                                        {

                                            lastyearid = lastyearid11.Id;
                                        }

                                        break;
                                    case "JOININGDATE":
                                        //var JoiningDate = db.Employee.Include(e => e.ServiceBookDates)
                                        //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        //tempCreditDate = JoiningDate.ServiceBookDates.JoiningDate;


                                        var LvNewReqData = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aa = null;
                                        DateTime? joinincr = null;
                                        var Fulldate = "";
                                        if (LvNewReqData != null)
                                        {
                                            aa = LvNewReqData.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aa != null)
                                        {
                                            Fulldate = aa.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.JoiningDate != null && q.Employee.Id == oEmployeeId).SingleOrDefault();

                                            if (ServiceBookData != null)
                                            {
                                                if (ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month <= ToDate.Value.Month)
                                                {

                                                    Fulldate = ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                }
                                            }
                                        }
                                        joinincr = Convert.ToDateTime(Fulldate);
                                        CreditDate = joinincr.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidj = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidj != null)
                                        {
                                            lastyearid = lastyearidj.Id;
                                        }
                                        Cal_Wise_Date = joinincr;
                                        break;
                                    case "CONFIRMATIONDATE":
                                        //var ConfirmationDate = db.Employee.Include(e => e.ServiceBookDates)
                                        //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        //tempCreditDate = ConfirmationDate.ServiceBookDates.ConfirmationDate;

                                        var LvNewReqDatac = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aac = null;
                                        DateTime? joinincrc = null;
                                        var Fulldatec = "";
                                        if (LvNewReqDatac != null)
                                        {
                                            aac = LvNewReqDatac.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aac != null)
                                        {
                                            Fulldatec = aac.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.ConfirmationDate != null && q.Employee.Id == oEmployeeId).SingleOrDefault();

                                            if (ServiceBookData != null)
                                            {
                                                if (ServiceBookData.Employee.ServiceBookDates.ConfirmationDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.ConfirmationDate.Value.Month <= ToDate.Value.Month)
                                                {

                                                    Fulldatec = ServiceBookData.Employee.ServiceBookDates.ConfirmationDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                }
                                            }
                                        }
                                        joinincrc = Convert.ToDateTime(Fulldatec);
                                        CreditDate = joinincrc.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincrc.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidc = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidc != null)
                                        {
                                            lastyearid = lastyearidc.Id;
                                        }
                                        Cal_Wise_Date = joinincrc;



                                        break;
                                    case "INCREMENTDATE":
                                        //var LastIncrementDate = db.Employee.Include(e => e.ServiceBookDates)
                                        //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        //tempCreditDate = LastIncrementDate.ServiceBookDates.LastIncrementDate;
                                        DateTime? joinincrI = null;
                                        var FulldateI = "";
                                        var Emp_lvcr = db.EmployeePayroll
                                            .Include(a => a.Employee)
                                            .Include(e => e.IncrementServiceBook)
                                            .Include(e => e.IncrementServiceBook.Select(q => q.IncrActivity))
                                            .Where(q => q.Employee.Id == oEmployeeId).SingleOrDefault();
                                        if (Emp_lvcr != null)
                                        {
                                            var fgds = Emp_lvcr.IncrementServiceBook.Where(q => q.ReleaseDate != null
                                                 && q.IncrActivity.Id == 1).OrderBy(q => q.Id).LastOrDefault();
                                            if (fgds != null)
                                            {
                                                if (fgds.ReleaseDate.Value.Month == FromDate.Value.Month)
                                                {
                                                    FulldateI = fgds.ReleaseDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                }
                                            }
                                            else
                                            {
                                                var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.LastIncrementDate == null && q.Employee.Id == oEmployeeId).SingleOrDefault();

                                                if (ServiceBookData != null)
                                                {
                                                    if (ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month <= ToDate.Value.Month)
                                                    {

                                                        FulldateI = ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                    }
                                                }
                                            }

                                        }

                                        joinincrI = Convert.ToDateTime(FulldateI);
                                        CreditDate = joinincrI.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincrI.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidI = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidI != null)
                                        {
                                            lastyearid = lastyearidI.Id;
                                        }
                                        Cal_Wise_Date = joinincrI;

                                        break;
                                    case "PROMOTIONDATE":
                                        //var LastPromotionDate = db.Employee.Include(e => e.ServiceBookDates)
                                        //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        //tempCreditDate = LastPromotionDate.ServiceBookDates.LastPromotionDate;

                                        DateTime? joinincrP = null;
                                        var FulldateP = "";
                                        var Emp_lvcrP = db.EmployeePayroll
                                            .Include(a => a.Employee)
                                            .Include(e => e.PromotionServiceBook)
                                            .Include(e => e.PromotionServiceBook.Select(q => q.PromotionActivity))
                                            .Where(q => q.Employee.Id == oEmployeeId).SingleOrDefault();
                                        if (Emp_lvcrP != null)
                                        {
                                            var fgds = Emp_lvcrP.PromotionServiceBook.Where(q => q.ReleaseDate != null
                                                 && q.PromotionActivity.Id == 2).OrderBy(q => q.Id).LastOrDefault();
                                            if (fgds != null)
                                            {
                                                if (fgds.ReleaseDate.Value.Month == FromDate.Value.Month)
                                                {
                                                    FulldateP = fgds.ReleaseDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                }
                                            }
                                            else
                                            {
                                                var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.LastPromotionDate == null && q.Employee.Id == oEmployeeId).SingleOrDefault();

                                                if (ServiceBookData != null)
                                                {
                                                    if (ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month <= ToDate.Value.Month)
                                                    {

                                                        FulldateP = ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                    }
                                                }
                                            }

                                        }

                                        joinincrP = Convert.ToDateTime(FulldateP);
                                        CreditDate = joinincrP.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincrP.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidP = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidP != null)
                                        {
                                            lastyearid = lastyearidP.Id;
                                        }
                                        Cal_Wise_Date = joinincrP;

                                        break;
                                    case "FIXDAYS":
                                        if (OCompany.Code.ToString() == "ACABL")
                                        {
                                            DateTime? Fixdays = null;
                                            Fixdays = Convert.ToDateTime("01/01/" + DateTime.Now.Year);

                                            Cal_Wise_Date = Fixdays;
                                            CreditDate = Fixdays.Value.AddDays(-1);
                                            Lastyear = Convert.ToDateTime(Fixdays.Value.AddYears(-1));

                                        }
                                        break;
                                    case "QUARTERLY":
                                        var LvNewReqDataQUARTERLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aaQUARTERLY = null;
                                        var FulldateQUARTERLY = "";
                                        if (LvNewReqDataQUARTERLY != null)
                                        {
                                            aaQUARTERLY = LvNewReqDataQUARTERLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aaQUARTERLY != null)
                                        {
                                            FulldateQUARTERLY = aaQUARTERLY.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            FulldateQUARTERLY = OLvSalStruct.OEffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

                                        }
                                        joinincr = Convert.ToDateTime(FulldateQUARTERLY);
                                        CreditDate = joinincr.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidjQUARTERLY = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidjQUARTERLY != null)
                                        {
                                            lastyearid = lastyearidjQUARTERLY.Id;
                                        }
                                        Cal_Wise_Date = joinincr;

                                        break;
                                    case "HALFYEARLY":
                                        var LvNewReqDataHALFYEARLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aaHALFYEARLY = null;
                                        var FulldateHALFYEARLY = "";
                                        if (LvNewReqDataHALFYEARLY != null)
                                        {
                                            aaHALFYEARLY = LvNewReqDataHALFYEARLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aaHALFYEARLY != null)
                                        {
                                            FulldateHALFYEARLY = aaHALFYEARLY.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            FulldateHALFYEARLY = OLvSalStruct.OEffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

                                        }
                                        joinincr = Convert.ToDateTime(FulldateHALFYEARLY);
                                        CreditDate = joinincr.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidjHALFYEARLY = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidjHALFYEARLY != null)
                                        {
                                            lastyearid = lastyearidjHALFYEARLY.Id;
                                        }
                                        Cal_Wise_Date = joinincr;
                                        break;
                                    case "MONTHLY":
                                        var LvNewReqDataMONTHLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aaMONTHLY = null;
                                        var FulldateMONTHLY = "";
                                        if (LvNewReqDataMONTHLY != null)
                                        {
                                            aaMONTHLY = LvNewReqDataMONTHLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aaMONTHLY != null)
                                        {
                                            FulldateMONTHLY = aaMONTHLY.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            FulldateMONTHLY = OLvSalStruct.OEffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

                                        }
                                        joinincr = Convert.ToDateTime(FulldateMONTHLY);
                                        CreditDate = joinincr.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincr.Value.AddMonths(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidjMONTHLY = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidjMONTHLY != null)
                                        {
                                            lastyearid = lastyearidjMONTHLY.Id;
                                        }
                                        Cal_Wise_Date = joinincr;
                                        break;
                                    case "BIMONTHLY":
                                        var LvNewReqDataBIMONTHLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aaBIMONTHLY = null;
                                        var FulldateBIMONTHLY = "";
                                        if (LvNewReqDataBIMONTHLY != null)
                                        {
                                            aaBIMONTHLY = LvNewReqDataBIMONTHLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aaBIMONTHLY != null)
                                        {
                                            FulldateBIMONTHLY = aaBIMONTHLY.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            FulldateBIMONTHLY = OLvSalStruct.OEffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

                                        }
                                        joinincr = Convert.ToDateTime(FulldateBIMONTHLY);
                                        CreditDate = joinincr.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidjBIMONTHLY = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidjBIMONTHLY != null)
                                        {
                                            lastyearid = lastyearidjBIMONTHLY.Id;
                                        }
                                        Cal_Wise_Date = joinincr;
                                        break;

                                    case "OTHER":
                                        //var EmpService = db.Employee.Include(e => e.ServiceBookDates)
                                        //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        //tempCreditDate = EmpService.ServiceBookDates.JoiningDate;
                                        break;
                                    default:
                                        break;
                                }

                                // settlement Leave Process start
                                double retmonthResignRetire = 0;
                                Double oldcreditlv = 0;
                                Double oldcreditlvclose = 0;
                                if (settlementemp == true)
                                {


                                    var LvNewReqDatasettle = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                    LvNewReq aaSettle = null;
                                    DateTime? Settletilldate = null;
                                    var Onworkingcreditednextcreditdate = "";
                                    var FulldateSettleStartdate = "";

                                    if (LvNewReqDatasettle != null)
                                    {
                                        aaSettle = LvNewReqDatasettle.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                    }
                                    if (aaSettle != null)
                                    {
                                        Onworkingcreditednextcreditdate = aaSettle.LvCreditNextDate.Value.ToShortDateString();
                                        FulldateSettleStartdate = aaSettle.LvCreditDate.Value.ToShortDateString();
                                        oldcreditlv = aaSettle.CreditDays;
                                        oldcreditlvclose = aaSettle.CloseBal;
                                    }

                                    // Resign,EXPIRED,TERMINATION
                                    var OOtherServiceBook = db.EmployeePayroll.Where(e => e.Employee.Id == oEmployeeId)
                                    .Include(e => e.OtherServiceBook)

                                     .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity))
                                     .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity.OtherSerBookActList))
                                   .SingleOrDefault();
                                    var OOOtherServiceBook = OOtherServiceBook.OtherServiceBook.Where(e => e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "RESIGNED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "EXPIRED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "TERMINATION").Select(x => x.OthServiceBookActivity.OtherSerBookActList)
                                                             .FirstOrDefault();
                                    if (OOOtherServiceBook != null)
                                    {
                                        string check = OOOtherServiceBook.LookupVal.ToUpper().ToString();
                                        if (check != null)
                                        {
                                            if (check == "RESIGNED" || check == "EXPIRED" || check == "TERMINATION")
                                            {
                                                var OOOtherServiceBookdate = OOtherServiceBook.OtherServiceBook.Where(e => e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "RESIGNED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "EXPIRED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "TERMINATION")
                                                            .FirstOrDefault();

                                                Settletilldate = Convert.ToDateTime(OOOtherServiceBookdate.ProcessOthDate);// Resign employee leave credit on request date of resignation
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var OOtherServiceBookret = db.Employee.Where(e => e.Id == oEmployeeId)
                                                                            .Include(e => e.ServiceBookDates)
                                                                           .SingleOrDefault();
                                        if (OOtherServiceBookret != null)
                                        {
                                            Settletilldate = Convert.ToDateTime(OOtherServiceBookret.ServiceBookDates.RetirementDate);
                                        }

                                    }

                                    CreditDate = Convert.ToDateTime(Settletilldate); //31/12/2020   
                                    Lastyear = Convert.ToDateTime(FulldateSettleStartdate); //01/12/2020
                                    Cal_Wise_Date = CreditDate;
                                    // settle emp retire/resign month start

                                    if (Lastyear.Value.Day >= 15)
                                    {
                                        // retmonth=
                                        int compMonthResignRetire = (Lastyear.Value.Month + Lastyear.Value.Year * 12) - (Settletilldate.Value.Month + Settletilldate.Value.Year * 12);

                                        retmonthResignRetire = compMonthResignRetire + 1;

                                    }
                                    else
                                    {
                                        int compMonthResignRetire = (Lastyear.Value.Month + Lastyear.Value.Year * 12) - (Settletilldate.Value.Month + Settletilldate.Value.Year * 12);

                                        retmonthResignRetire = compMonthResignRetire;
                                    }
                                    if (retmonthResignRetire < 0)
                                    {
                                        retmonthResignRetire = 0;
                                    }
                                    retmonthResignRetire = Math.Round(retmonthResignRetire, 0);

                                    // settle emp retire/resign month end
                                }
                                // settlement Leave Process end

                                if (CreditDate == null && Lastyear == null)
                                {
                                    CreditDate = Convert.ToDateTime(tempCreditDate.Value.Day + "/" + tempCreditDate.Value.Month + "/" + DateTime.Now.Year);
                                    leaveyearfrom = CreditDate;
                                    leaveyearto = leaveyearfrom.Value.AddDays(-1);
                                    CreditDate = CreditDate.Value.AddDays(-1);
                                    Lastyear = CreditDate.Value.AddYears(-1);
                                }


                                leaveyearfrom = LvCalendar.FromDate;
                                leaveyearto = LvCalendar.ToDate;
                                double retmonth = 0;
                                var retiredate = db.Employee.Include(e => e.ServiceBookDates)
                                            .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                tempRetireDate = retiredate.ServiceBookDates.RetirementDate;
                                if (tempRetireDate != null)
                                {
                                    if (leaveyearto > tempRetireDate)
                                    {
                                        tempRetirecrDate = tempRetireDate;
                                    }
                                    else
                                    {
                                        tempRetirecrDate = leaveyearto;
                                    }
                                }
                                else
                                {
                                    tempRetirecrDate = leaveyearto;
                                }
                                if (tempRetirecrDate.Value.Day >= 15)
                                {
                                    // retmonth=
                                    int compMonth = (tempRetirecrDate.Value.Month + tempRetirecrDate.Value.Year * 12) - (leaveyearfrom.Value.Month + leaveyearfrom.Value.Year * 12);
                                    //double daysInEndMonth = (tempRetirecrDate - tempRetirecrDate.Value.AddMonths(1)).Value.Days;
                                    //retmonth = compMonth + (leaveyearfrom.Value.Day - tempRetirecrDate.Value.Day) / daysInEndMonth;
                                    retmonth = compMonth + 1;

                                }
                                else
                                {
                                    int compMonth = (tempRetirecrDate.Value.Month + tempRetirecrDate.Value.Year * 12) - (leaveyearfrom.Value.Month + leaveyearfrom.Value.Year * 12);
                                    //double daysInEndMonth = (tempRetirecrDate - tempRetirecrDate.Value.AddMonths(1)).Value.Days;
                                    //retmonth = compMonth + (leaveyearfrom.Value.Day - tempRetirecrDate.Value.Day) / daysInEndMonth;
                                    retmonth = compMonth;
                                }
                                if (retmonth < 0)
                                {
                                    retmonth = 0;
                                }
                                retmonth = Math.Round(retmonth, 0);

                                int compMonthLYR = (leaveyearto.Value.Month + leaveyearto.Value.Year * 12) - (leaveyearfrom.Value.Month + leaveyearfrom.Value.Year * 12);
                                double daysInEndMonthLYR = (leaveyearto - leaveyearto.Value.AddMonths(1)).Value.Days;
                                double LYRmonth = compMonthLYR + (leaveyearfrom.Value.Day - leaveyearto.Value.Day) / daysInEndMonthLYR;
                                LYRmonth = Math.Round(LYRmonth, 0);

                                // DateTime NextCreditDays = Cal_Wise_Date.Value.AddYears(1); //comment line because credit frequeny avilable in system if yearly then 12 half yearly then 6 if monthyly then 1 frequency 
                                DateTime NextCreditDays = Cal_Wise_Date.Value.AddMonths(oLvCreditPolicy.ProCreditFrequency);

                                EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == oEmployeeId)
                                    .Include(a => a.LvNewReq)
                                    .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                                    .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                                    .Include(a => a.LvNewReq.Select(e => e.GeoStruct))
                                    .Include(a => a.LvNewReq.Select(e => e.PayStruct))
                                    .Include(a => a.LvNewReq.Select(e => e.FuncStruct))
                                    .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                                    .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.LeaveHead.Id == item.Id
                                        //  && a.LeaveCalendar.FromDate == Lastyear && a.LeaveCalendar.ToDate == CreditDate
                                        ))
                                        .SingleOrDefault();
                                List<LvNewReq> Filter_oEmpLvData = new List<LvNewReq>();
                                // Check if leave Credited for that year then should not credit start
                                if (_Prv_EmpLvData != null)
                                {
                                    var LvCreditedInNewYrChk1 = _Prv_EmpLvData.LvNewReq
                                            .Where(a => a.LvCreditNextDate != null && a.LvOrignal_Id == null && a.LvCreditNextDate.Value.Date == NextCreditDays.Date
                                            && a.LeaveHead.Id == item.Id).FirstOrDefault();
                                    if (LvCreditedInNewYrChk1 != null)
                                    {
                                        continue;
                                    }
                                }
                                // Check if leave Credited for that year then should not credit end

                                if (_Prv_EmpLvData != null)
                                {
                                    LvNewReq LvCreditedInNewYrChk = _Prv_EmpLvData.LvNewReq
                                        .Where(a => a.LeaveCalendar.FromDate == LvCalendar.FromDate && a.LvCreditNextDate == NextCreditDays.Date
                                        && a.LeaveHead.Id == item.Id).SingleOrDefault();
                                    if (LvCreditedInNewYrChk == null)
                                    {
                                        Filter_oEmpLvData = _Prv_EmpLvData.LvNewReq
                                            .Where(a => a.LeaveHead != null && a.LeaveHead.Id == item.Id)
                                            //      && a.ReqDate >= Lastyear &&
                                            //a.ReqDate <= CreditDate)
                                          .ToList();
                                    }
                                    else
                                    {
                                        Filter_oEmpLvData.Add(LvCreditedInNewYrChk);
                                    }
                                }
                                double oLvClosingData = 0;
                                double UtilizedLv = 0;
                                Int32 GeoStruct = 0;
                                Int32 PayStruct = 0;
                                Int32 FuncStruct = 0;
                                if (Filter_oEmpLvData.Count == 0)
                                {
                                    //get Data from opening
                                    EmployeeLeave _Emp_EmpOpeningData = db.EmployeeLeave.Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == item.Id))
                                        .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                        .Include(e => e.Employee.GeoStruct)
                                        .Include(e => e.Employee.PayStruct)
                                        .Include(e => e.Employee.FuncStruct)
                                        .SingleOrDefault();

                                    double _EmpOpeningData = 0;
                                    if (_Emp_EmpOpeningData != null)
                                    {
                                        _EmpOpeningData = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == item.Id).Select(e => e.LvOpening).SingleOrDefault();

                                    }
                                    if (_EmpOpeningData == 0)
                                    {
                                        // continue;
                                    }
                                    oLvClosingData = _EmpOpeningData;
                                    UtilizedLv = 0;
                                    if (_Emp_EmpOpeningData != null)
                                    {
                                        GeoStruct = _Emp_EmpOpeningData.Employee.GeoStruct.Id;
                                        PayStruct = _Emp_EmpOpeningData.Employee.PayStruct.Id;
                                        FuncStruct = _Emp_EmpOpeningData.Employee.FuncStruct.Id;

                                    }
                                    else
                                    {
                                        EmployeeLeave _Emp = db.EmployeeLeave.Where(e => e.Employee.Id == oEmployeeId)
                                      .Include(e => e.Employee.GeoStruct)
                                      .Include(e => e.Employee.PayStruct)
                                      .Include(e => e.Employee.FuncStruct)
                                      .SingleOrDefault();

                                        GeoStruct = _Emp.Employee.GeoStruct.Id;
                                        PayStruct = _Emp.Employee.PayStruct.Id;
                                        FuncStruct = _Emp.Employee.FuncStruct.Id;
                                    }
                                }
                                else
                                {
                                    var LastLvData = Filter_oEmpLvData.OrderByDescending(a => a.Id).FirstOrDefault();

                                    oLvClosingData = LastLvData.CloseBal;
                                    UtilizedLv = LastLvData.LVCount;
                                    GeoStruct = LastLvData.GeoStruct != null ? LastLvData.GeoStruct.Id : db.Employee.Find(oEmployeeId).GeoStruct.Id;
                                    PayStruct = LastLvData.PayStruct != null ? LastLvData.PayStruct.Id : db.Employee.Find(oEmployeeId).PayStruct.Id;
                                    FuncStruct = LastLvData.FuncStruct != null ? LastLvData.FuncStruct.Id : db.Employee.Find(oEmployeeId).FuncStruct.Id;
                                }

                                if (oLvCreditPolicy.ProdataFlag == true)
                                {

                                    var AttendaceData = db.EmployeePayroll.Include(e => e.SalAttendance)
                                        .Where(e => e.Employee.Id == oEmployeeId
                                        //&&
                                        //e.SalAttendance.Any(a => Convert.ToDateTime("01/" + a.PayMonth) >= Lastyear &&
                                        //Convert.ToDateTime("01/" + a.PayMonth) <= CreditDate)
                                        ).FirstOrDefault();

                                    if (AttendaceData != null)
                                    {
                                        Boolean newjoinconfermationdate = false;
                                        var confermationdate = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.ConfirmationDate != null && q.Employee.Id == oEmployeeId).SingleOrDefault();
                                        double daysdiff = 0;
                                        if (confermationdate != null)
                                        {
                                            if (confermationdate.Employee.ServiceBookDates.ConfirmationDate > Lastyear)
                                            {
                                                string Confirmmonthyear = confermationdate.Employee.ServiceBookDates.ConfirmationDate.Value.ToString("MM/yyyy");
                                                DateTime monthend = Convert.ToDateTime("01/" + Confirmmonthyear).AddMonths(1).AddDays(-1);
                                                daysdiff = (monthend.Date - confermationdate.Employee.ServiceBookDates.ConfirmationDate.Value.Date).Days + 1;
                                                Lastyear = confermationdate.Employee.ServiceBookDates.ConfirmationDate.Value.Date;
                                                newjoinconfermationdate = true;
                                            }
                                        }
                                        if (newjoinconfermationdate == false)
                                        {
                                            SumDays = AttendaceData.SalAttendance.Where(a => Convert.ToDateTime("01/" + a.PayMonth) >= Lastyear &&
                                     Convert.ToDateTime("01/" + a.PayMonth) <= CreditDate && a.PaybleDays != 0).Select(a => a.PaybleDays).ToList().Sum();
                                            //LWP Leave process button on manual attendance page Goa urban bank
                                            LWPDays = AttendaceData.SalAttendance.Where(a => Convert.ToDateTime("01/" + a.PayMonth) >= Lastyear &&
                                       Convert.ToDateTime("01/" + a.PayMonth) <= CreditDate && a.LWPDays != 0).Select(a => a.LWPDays).ToList().Sum();
                                        }
                                        else
                                        {
                                            SumDays = AttendaceData.SalAttendance.Where(a => Convert.ToDateTime("01/" + a.PayMonth) > Lastyear &&
                                                                               Convert.ToDateTime("01/" + a.PayMonth) <= CreditDate && a.PaybleDays != 0).Select(a => a.PaybleDays).ToList().Sum();
                                            //LWP Leave process button on manual attendance page Goa urban bank
                                            LWPDays = AttendaceData.SalAttendance.Where(a => Convert.ToDateTime("01/" + a.PayMonth) > Lastyear &&
                                       Convert.ToDateTime("01/" + a.PayMonth) <= CreditDate && a.LWPDays != 0).Select(a => a.LWPDays).ToList().Sum();
                                        }

                                        SumDays = (SumDays + daysdiff) - LWPDays;
                                    }

                                    ////suspended days check

                                    EmployeePayroll othser = db.EmployeePayroll.Where(e => e.Employee.Id == oEmployeeId)
                                                            .Include(e => e.OtherServiceBook)
                                                            .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity)).AsNoTracking().OrderBy(e => e.Id)
                                                            .SingleOrDefault();

                                    if (othser.OtherServiceBook != null && othser.OtherServiceBook.Count() > 0)
                                    {
                                        List<OtherServiceBook> OthServBkSus = othser.OtherServiceBook.Where(e => e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED" || e.OthServiceBookActivity.Name.ToUpper() == "REJOIN").OrderByDescending(e => e.ReleaseDate).ToList();
                                        if (OthServBkSus.Count() > 0)
                                        {
                                            var checkSuspenddays = OthServBkSus.Where(e => e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED").Select(e => e.ReleaseDate).SingleOrDefault();
                                            var checkRejoindays_temp = OthServBkSus.Where(e => e.OthServiceBookActivity.Name.ToUpper() == "REJOIN").Select(e => e.ReleaseDate).SingleOrDefault();
                                            var checkRejoindays = "";
                                            if (checkRejoindays_temp != null)
                                            {
                                                checkRejoindays = checkRejoindays_temp.ToString();
                                            }
                                            else
                                            {
                                                checkRejoindays = CreditDate.ToString();
                                            }
                                            if (checkSuspenddays != null && checkRejoindays != null)
                                            {
                                                if (Convert.ToDateTime(checkSuspenddays).Date < Lastyear)
                                                {
                                                    checkSuspenddays = Lastyear;
                                                }
                                                if (Convert.ToDateTime(checkRejoindays).Date >= Lastyear && Convert.ToDateTime(checkRejoindays).Date <= CreditDate)
                                                {
                                                    mSusDayFull = Math.Round((Convert.ToDateTime(checkRejoindays).Date - Convert.ToDateTime(checkSuspenddays).Date).TotalDays) + 1;
                                                    SumDays = SumDays - mSusDayFull;
                                                }
                                                if (SumDays < 0)
                                                {
                                                    SumDays = 0;
                                                }
                                            }
                                        }
                                    }


                                    var OSalArrT = db.EmployeePayroll
                                        .Include(e => e.SalaryArrearT)
                                        .Include(e => e.SalaryArrearT.Select(q => q.ArrearType))
                                        .Where(e => e.Employee.Id == oEmployeeId)
                                     .FirstOrDefault();
                                    if (OSalArrT != null)
                                    {
                                        double ArrDays = OSalArrT.SalaryArrearT.Where(q => q.ArrearType.LookupVal.ToUpper() == "LWP"
                                              && q.FromDate >= Lastyear
                                              && q.FromDate <= CreditDate && q.IsRecovery == false).Select(q => q.TotalDays).Sum();
                                        SumDays = SumDays + ArrDays;


                                        double ArrDaysrec = OSalArrT.SalaryArrearT.Where(q => q.ArrearType.LookupVal.ToUpper() == "LWP"
                                        && q.FromDate >= Lastyear
                                        && q.FromDate <= CreditDate && q.IsRecovery == true).Select(q => q.TotalDays).Sum();
                                        SumDays = SumDays - ArrDaysrec;

                                        if (SumDays < 0)
                                        {
                                            SumDays = 0;
                                        }
                                    }

                                }

                                if (oLvCreditPolicy.ExcludeLeaves == true)
                                {
                                    List<LvHead> GetExculdeLvHeads = oLvCreditPolicy.ExcludeLeaveHeads.ToList();
                                    foreach (var GetExculdeLvHead in GetExculdeLvHeads)
                                    {

                                        var _Prv_EmpLvData_exclude = db.EmployeeLeave.AsNoTracking()
                                            .Where(a => a.Employee.Id == oEmployeeId)
                                            .Include(a => a.LvNewReq)
                                            .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                                            .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                                             .Include(a => a.LvNewReq.Select(e => e.LvOrignal))
                                            .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                                            .SingleOrDefault();

                                        var _Prv_EmpLvData_exclude1 = _Prv_EmpLvData_exclude.LvNewReq
                                           .Where(a => a.LeaveHead != null
                                               && a.LeaveHead.Id == GetExculdeLvHead.Id
                                               //&& a.LeaveCalendar.Id == lastyearid && a.IsCancel == false && a.WFStatus.LookupVal != "2"
                                                && a.IsCancel == false && a.WFStatus.LookupVal != "2"
                                       ).ToList();

                                        var LvOrignal_id = _Prv_EmpLvData_exclude.LvNewReq.Where(e => e.LvOrignal != null && e.WFStatus.LookupVal != "2").Select(e => e.LvOrignal.Id).ToList();
                                        var listLvs = _Prv_EmpLvData_exclude1.Where(e => !LvOrignal_id.Contains(e.Id) && e.FromDate != null && e.ToDate != null).OrderBy(e => e.Id).ToList();
                                        double DebitSum = 0;
                                        if (listLvs != null)
                                        {
                                            for (DateTime _Date = Lastyear.Value; _Date <= CreditDate; _Date = _Date.AddDays(1))
                                            {
                                                var xyz = listLvs.Where(q => _Date >= q.FromDate && _Date <= q.ToDate).FirstOrDefault();
                                                if (xyz != null)
                                                {
                                                    DebitSum = DebitSum + 1;
                                                }
                                            }
                                            // double DebitSum = listLvs.Sum(e => e.DebitDays);
                                            SumDays = SumDays - DebitSum;
                                        }
                                        else
                                        {
                                            EmployeeLeave _Emp_EmpOpeningData_exclude = db.EmployeeLeave.Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == GetExculdeLvHead.Id))
                                                                                .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                                                                .Include(e => e.Employee.GeoStruct)
                                                                                .Include(e => e.Employee.PayStruct)
                                                                                .Include(e => e.Employee.FuncStruct)
                                                                                .SingleOrDefault();

                                            double _EmpOpeningData_exclude = 0;
                                            if (_Emp_EmpOpeningData_exclude != null)
                                            {
                                                _EmpOpeningData_exclude = _Emp_EmpOpeningData_exclude.LvOpenBal.Where(e => e.LvHead.Id == GetExculdeLvHead.Id).Select(e => e.LVCount).SingleOrDefault();

                                            }
                                            SumDays = SumDays - _EmpOpeningData_exclude;

                                        }
                                        //double CloseBalSum = Filter_oEmpLvData.Where(e => e.LeaveHead.Id == GetExculdeLvHead.Id).Select(e => e.CloseBal).ToList().Sum();
                                        //SumDays = SumDays - CloseBalSum;
                                    }
                                    if (SumDays < 0)
                                    {
                                        SumDays = 0;
                                    }
                                }

                                //if (SumDays == 0)
                                //{
                                //    return 0;
                                //}

                                List<LvNewReq> _List_oLvNewReq = new List<LvNewReq>();
                                if (oLvCreditPolicy.LVConvert == true)
                                {
                                    //if (oLvClosingData > 0)
                                    //{
                                    double LastMonthBal = 0, _LvLapsed = 0, Prv_bal = 0;
                                    LvHead ConvertLeaveHead = oLvCreditPolicy.ConvertLeaveHead;
                                    //Check Exitance
                                    List<LvNewReq> Filter_oEmpLvDataCon = new List<LvNewReq>();
                                    if (_Prv_EmpLvData != null)
                                    {
                                        //LvNewReq DefaultyearLvHeadCreditedCHeck = _Prv_EmpLvData.LvNewReq
                                        //      .Where(a => a.LeaveHead != null
                                        //          && a.LeaveHead.Id == ConvertLeaveHead.Id
                                        //          && a.ReqDate >= LvCalendar.FromDate).SingleOrDefault();
                                        LvNewReq DefaultyearLvHeadCreditedCHeck = _Prv_EmpLvData.LvNewReq
                                        .Where(a => a.LvCreditNextDate != null && a.LvOrignal_Id != null && a.LvCreditNextDate.Value.Date == NextCreditDays.Date
                                        && a.LeaveHead.Id == ConvertLeaveHead.Id).SingleOrDefault();


                                        if (DefaultyearLvHeadCreditedCHeck == null)
                                        {
                                            Filter_oEmpLvDataCon = _Prv_EmpLvData.LvNewReq
                                                .Where(a => a.LeaveHead != null && a.LeaveHead.Id == ConvertLeaveHead.Id
                                                //      && a.ReqDate >= Lastyear &&
                                                //a.ReqDate <= CreditDate
                                              ).ToList();
                                        }
                                        else
                                        {
                                            Filter_oEmpLvDataCon.Add(DefaultyearLvHeadCreditedCHeck);
                                        }
                                    }
                                    var _LvNewReq_Prv_bal = Filter_oEmpLvDataCon.Count() > 0 ? Filter_oEmpLvDataCon.Where(e => e.LeaveHead.Id == ConvertLeaveHead.Id)
                                        .OrderByDescending(e => e.Id).Select(e => e.CloseBal).FirstOrDefault() : 0;
                                    if (_LvNewReq_Prv_bal == 0)
                                    {
                                        EmployeeLeave _Emp_LvOpenBal = db.EmployeeLeave
                                            .Include(e => e.LvOpenBal)
                                            .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                            .Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == ConvertLeaveHead.Id)).FirstOrDefault();
                                        if (_Emp_LvOpenBal != null)
                                        {
                                            Prv_bal = _Emp_LvOpenBal.LvOpenBal.Where(a => a.LvHead.Id == ConvertLeaveHead.Id).Select(e => e.LvOpening).LastOrDefault();
                                        }
                                        else
                                        {
                                            Prv_bal = _LvNewReq_Prv_bal;
                                        }
                                    }
                                    else
                                    {
                                        Prv_bal = _LvNewReq_Prv_bal;
                                    }


                                    double conevrtedlv = 0;
                                    double afterconvrtedremainlv = 0;
                                    if (oLvClosingData > oLvCreditPolicy.LvConvertLimit)
                                    {
                                        conevrtedlv = oLvCreditPolicy.LvConvertLimit;
                                        afterconvrtedremainlv = oLvClosingData - conevrtedlv;
                                    }
                                    else
                                    {
                                        conevrtedlv = oLvClosingData;
                                    }

                                    //  LastMonthBal = Prv_bal + oLvClosingData;
                                    LastMonthBal = Prv_bal + conevrtedlv;
                                    //-------------------------------------

                                    LvNewReq newLvConvertobj = new LvNewReq
                                    {
                                        InputMethod = 0,
                                        ReqDate = DateTime.Now,
                                        CloseBal = LastMonthBal,
                                        OpenBal = Prv_bal,
                                        LvOccurances = 0,
                                        IsLock = true,
                                        LvLapsed = _LvLapsed,
                                        //CreditDays = oLvClosingData,
                                        CreditDays = conevrtedlv,
                                        //ToDate = CreditDate,
                                        //FromDate = CreditDate,
                                        //LvCreditDate = Cal_Wise_Date,
                                        LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                            e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
                                        GeoStruct = db.GeoStruct.Where(e => e.Id == GeoStruct).SingleOrDefault(),
                                        PayStruct = db.PayStruct.Where(e => e.Id == PayStruct).SingleOrDefault(),
                                        FuncStruct = db.FuncStruct.Where(e => e.Id == FuncStruct).SingleOrDefault(),
                                        LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHead.Id).SingleOrDefault(),
                                        Narration = settlementemp == true ? "Settlement Process" : "Credit Process",
                                        // LvCreditNextDate = NextCreditDays,
                                        Reason = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault().LvCode + " Converted",
                                        WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault(), //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
                                        DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                    };
                                    _List_oLvNewReq.Add(newLvConvertobj);

                                    // after conversion remaining leave conevrt another leave
                                    if (oLvCreditPolicy.LVConvertBal == true)
                                    {
                                        if (afterconvrtedremainlv > 0)
                                        {
                                            // 20/01/2020 start
                                            double LastMonthBalrm = 0, _LvLapsedrm = 0, Prv_balrm = 0;
                                            LvHead ConvertLeaveHeadrm = oLvCreditPolicy.ConvertLeaveHeadBal;
                                            //Check Exitance
                                            List<LvNewReq> Filter_oEmpLvDataConrm = new List<LvNewReq>();
                                            if (_Prv_EmpLvData != null)
                                            {
                                                LvNewReq DefaultyearLvHeadCreditedCHeck = _Prv_EmpLvData.LvNewReq
                                                      .Where(a => a.LeaveHead != null
                                                          && a.LeaveHead.Id == ConvertLeaveHeadrm.Id
                                                          && a.ReqDate >= LvCalendar.FromDate).SingleOrDefault();


                                                if (DefaultyearLvHeadCreditedCHeck == null)
                                                {
                                                    Filter_oEmpLvDataCon = _Prv_EmpLvData.LvNewReq
                                                        .Where(a => a.LeaveHead != null && a.LeaveHead.Id == ConvertLeaveHeadrm.Id && a.ReqDate >= Lastyear &&
                                                      a.ReqDate <= CreditDate).ToList();
                                                }
                                                else
                                                {
                                                    Filter_oEmpLvDataCon.Add(DefaultyearLvHeadCreditedCHeck);
                                                }
                                            }
                                            var _LvNewReq_Prv_balrm = Filter_oEmpLvDataCon.Count() > 0 ? Filter_oEmpLvDataCon.Where(e => e.LeaveHead.Id == ConvertLeaveHeadrm.Id)
                                                .OrderByDescending(e => e.Id).Select(e => e.CloseBal).FirstOrDefault() : 0;
                                            if (_LvNewReq_Prv_balrm == 0)
                                            {
                                                EmployeeLeave _Emp_LvOpenBal = db.EmployeeLeave
                                                    .Include(e => e.LvOpenBal)
                                                    .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                                    .Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == ConvertLeaveHeadrm.Id)).FirstOrDefault();
                                                if (_Emp_LvOpenBal != null)
                                                {
                                                    Prv_balrm = _Emp_LvOpenBal.LvOpenBal.Where(a => a.LvHead.Id == ConvertLeaveHeadrm.Id).Select(e => e.LvOpening).LastOrDefault();
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                Prv_balrm = _LvNewReq_Prv_balrm;
                                            }


                                            double conevrtedlvrm = 0;

                                            if (afterconvrtedremainlv > oLvCreditPolicy.LvConvertLimitBal)
                                            {
                                                conevrtedlvrm = oLvCreditPolicy.LvConvertLimitBal;

                                            }
                                            else
                                            {
                                                conevrtedlvrm = afterconvrtedremainlv;
                                            }

                                            //  LastMonthBal = Prv_bal + oLvClosingData;
                                            LastMonthBalrm = Prv_balrm + conevrtedlvrm;
                                            //-------------------------------------

                                            LvNewReq newLvConvertobjrm = new LvNewReq
                                            {
                                                InputMethod = 0,
                                                ReqDate = DateTime.Now,
                                                CloseBal = LastMonthBalrm,
                                                OpenBal = Prv_balrm,
                                                LvOccurances = 0,
                                                IsLock = true,
                                                LvLapsed = _LvLapsed,
                                                //CreditDays = oLvClosingData,
                                                CreditDays = conevrtedlvrm,
                                                //ToDate = CreditDate,
                                                //FromDate = CreditDate,
                                                // LvCreditDate = Cal_Wise_Date,
                                                LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                                    e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
                                                GeoStruct = db.GeoStruct.Where(e => e.Id == GeoStruct).SingleOrDefault(),
                                                PayStruct = db.PayStruct.Where(e => e.Id == PayStruct).SingleOrDefault(),
                                                FuncStruct = db.FuncStruct.Where(e => e.Id == FuncStruct).SingleOrDefault(),
                                                LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHeadrm.Id).SingleOrDefault(),
                                                Narration = settlementemp == true ? "Settlement Process" : "Credit Process",
                                                //  LvCreditNextDate = NextCreditDays,
                                                Reason = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault().LvCode + " Converted",
                                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault(),  //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
                                                DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                            };
                                            _List_oLvNewReq.Add(newLvConvertobjrm);


                                            // 20/01/2020 end

                                        }
                                    }

                                    //}
                                }

                                // round parameter
                                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                                bool exists = System.IO.Directory.Exists(requiredPath);
                                string localPath;
                                if (!exists)
                                {
                                    localPath = new Uri(requiredPath).LocalPath;
                                    System.IO.Directory.CreateDirectory(localPath);
                                }
                                string path = requiredPath + @"\LvCreditround" + ".ini";
                                localPath = new Uri(path).LocalPath;
                                string roundp = "";
                                int rounddigit = 0;
                                string rndg = "";
                                using (var streamReader = new StreamReader(localPath))
                                {
                                    string line;

                                    while ((line = streamReader.ReadLine()) != null)
                                    {
                                        roundp = line.Split('_')[0];
                                        rndg = line.Split('_')[1];
                                        rounddigit = Convert.ToInt32(rndg);
                                        if (roundp == "ROUND")
                                        { }
                                    }
                                }


                                if (oLvCreditPolicy.FixedCreditDays == true)
                                {
                                    //CreditDays += oLvCreditPolicy.CreditDays;
                                    if (roundp == "")
                                    {
                                        CreditDays += Math.Round((oLvCreditPolicy.CreditDays / LYRmonth) * retmonth, 0);
                                    }
                                    else
                                    {
                                        CreditDays += Math.Round((oLvCreditPolicy.CreditDays / LYRmonth) * retmonth, rounddigit);
                                    }

                                }
                                else
                                {
                                    if (OCompany.Code.ToString() == "KDCC")
                                    {
                                        double WorkingDays1 = oLvCreditPolicy.WorkingDays;
                                        DateTime CreditDate1 = LvCalendar.FromDate.Value.AddDays(-1);
                                        DateTime Lastyear1 = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        int totactday = (CreditDate1 - Lastyear1).Days + 1;
                                        double TotcreditDays = Convert.ToDouble(totactday / 11.40);
                                        TotcreditDays = Convert.ToInt32(TotcreditDays);
                                        double totleaveLwp = (totactday - SumDays);
                                        Double LWPLeave = Convert.ToDouble(totleaveLwp / WorkingDays1);

                                        int LWPLeave1 = (int)LWPLeave;
                                        if (LWPLeave > LWPLeave1)
                                        {
                                            LWPLeave1 = LWPLeave1 + 1;
                                        }

                                        CreditDays += TotcreditDays - LWPLeave1;
                                    }
                                    else
                                    {
                                        double WorkingDays = oLvCreditPolicy.WorkingDays;
                                        double MinMiseWorkingDays = Convert.ToDouble(SumDays / WorkingDays);
                                        if (roundp == "")
                                        {
                                            CreditDays += Convert.ToInt32(MinMiseWorkingDays);
                                        }
                                        else
                                        {
                                            CreditDays += Math.Round(MinMiseWorkingDays, rounddigit);
                                        }

                                    }
                                }
                                if (roundp == "NEARESTFIFTY")
                                {
                                    var Actamt = CreditDays.ToString();
                                    string rs = Actamt.Split('.')[0];
                                    string Ps = Actamt.Split('.')[1];
                                    int pais = Convert.ToInt32(Ps);
                                    if (pais >= 50)
                                    {
                                        CreditDays = Convert.ToDouble(rs + "." + "50");
                                    }
                                    else
                                    {
                                        CreditDays = Convert.ToDouble(rs + "." + "00");
                                    }

                                }

                                if (CreditDays < 0)
                                {
                                    CreditDays = 0;
                                }
                                // settlement Leave Process start
                                if (settlementemp == true)
                                {
                                    if (oLvCreditPolicy.FixedCreditDays == true)
                                    {
                                        CreditDays += Math.Round((oLvCreditPolicy.CreditDays / LYRmonth) * retmonthResignRetire, 0);
                                        if (oldcreditlv > CreditDays)
                                        {
                                            CreditDays = CreditDays - oldcreditlvclose - oLvClosingData;
                                        }
                                    }

                                }
                                // settlement Leave Process end

                                double oLvOccurances = 0;
                                if (oLvCreditPolicy.ServiceLink == true)
                                {
                                    var JoiningDate = db.Employee.Include(e => e.ServiceBookDates)
                                           .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                    DateTime tempCreditDate1 = Convert.ToDateTime(JoiningDate.ServiceBookDates.JoiningDate);
                                    DateTime till = CreditDate.Value.AddDays(1);
                                    int compMonths = (till.Month + till.Year * 12) - (tempCreditDate1.Month + tempCreditDate1.Year * 12);
                                    double daysInEndMonths = (till - till.AddMonths(1)).Days;
                                    double monthss = compMonths + (tempCreditDate1.Day - till.Day) / daysInEndMonths;
                                    var GetServiceYear = Math.Round((monthss / 12) + 0.001, 0);

                                    // var GetServiceYear = Convert.ToDateTime(DateTime.Now - tempCreditDate).Year;
                                    if (GetServiceYear > oLvCreditPolicy.ServiceYearsLimit && oLvCreditPolicy.AboveServiceMaxYears == false)
                                    {
                                        CreditDays = 0;
                                    }
                                    if (oLvCreditPolicy.AboveServiceMaxYears == true)
                                    {
                                        if (GetServiceYear > oLvCreditPolicy.ServiceYearsLimit)
                                        {
                                            //double FinalServiceyear = GetServiceYear + oLvCreditPolicy.AboveServiceSteps;
                                            //if (GetServiceYear >= FinalServiceyear && GetServiceYear <= FinalServiceyear)
                                            //{
                                            //    CreditDays = 0;
                                            //}
                                            bool Chkcrdyr = false;
                                            for (double i = oLvCreditPolicy.ServiceYearsLimit; i <= GetServiceYear; )
                                            {
                                                var updatenew = i + oLvCreditPolicy.AboveServiceSteps;
                                                i = updatenew;
                                                if (updatenew == GetServiceYear)
                                                {
                                                    //will credit
                                                    Chkcrdyr = true;
                                                    break;
                                                }
                                            }
                                            if (Chkcrdyr == false)
                                            {
                                                CreditDays = 0;
                                            }
                                        }
                                    }

                                    if (oLvClosingData > oLvCreditPolicy.MaxLeaveDebitInService)
                                    {
                                        CreditDays = 0;
                                    }
                                    if (GetServiceYear > oLvCreditPolicy.ServicemaxYearsLimit)
                                    {
                                        CreditDays = 0;
                                    }

                                }
                                if (oLvCreditPolicy.OccInServAppl == true)
                                {
                                    if (item.LvCode.ToUpper() == "ML" || item.LvCode.ToUpper() == "PTL")
                                    {
                                        if (UtilizedLv >= oLvCreditPolicy.OccInServ)
                                        {
                                            CreditDays = 0;
                                        }
                                        else if (oLvCreditPolicy.OccCarryForward == true)
                                        {
                                            oLvOccurances = UtilizedLv;
                                        }
                                    }
                                }


                                // Satara DCC bank convert encash leave 90 day lock
                                string requiredPathLCK = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                                bool existsLCK = System.IO.Directory.Exists(requiredPathLCK);
                                string localPathLCK;
                                if (!existsLCK)
                                {
                                    localPathLCK = new Uri(requiredPathLCK).LocalPath;
                                    System.IO.Directory.CreateDirectory(localPathLCK);
                                }
                                string pathLCK = requiredPathLCK + @"\ConvertLeaveLock" + ".ini";
                                localPathLCK = new Uri(pathLCK).LocalPath;
                                string Lockdays = "";
                                string ConvertEncashlvcode = "";

                                using (var streamReader = new StreamReader(localPathLCK))
                                {
                                    string line;

                                    while ((line = streamReader.ReadLine()) != null)
                                    {
                                        Lockdays = line.Split('_')[0];
                                        ConvertEncashlvcode = line.Split('_')[1];
                                        if (oLvCreditPolicy.LVConvert == true)
                                        {
                                            double Lockday = Convert.ToDouble(Lockdays);
                                            LvHead ConvertLeaveHead = oLvCreditPolicy.ConvertLeaveHead;
                                            var LVname = db.LvHead.Where(e => e.Id == ConvertLeaveHead.Id).SingleOrDefault();
                                            if (ConvertLeaveHead.LvCode.ToUpper() == ConvertEncashlvcode.ToUpper())
                                            {
                                                double OB = 0;
                                                double EL = 0;
                                                double CB = 0;
                                                OB = CreditDays + oLvClosingData;
                                                if (OB > Lockday)
                                                {
                                                    if (oLvClosingData > Lockday)
                                                    {
                                                        if (CreditDays > oLvCreditPolicy.LvConvertLimit)
                                                        {
                                                            EL = oLvCreditPolicy.LvConvertLimit;
                                                            oLvClosingData = OB - EL;
                                                            CreditDays = 0;
                                                        }
                                                        else
                                                        {
                                                            EL = CreditDays;
                                                            CreditDays = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        EL = OB - Lockday;

                                                        if (EL > oLvCreditPolicy.LvConvertLimit)
                                                        {
                                                            EL = oLvCreditPolicy.LvConvertLimit;
                                                            oLvClosingData = OB - EL;
                                                            CreditDays = 0;// oLvClosingData - Lockday;
                                                        }
                                                        else
                                                        {
                                                            EL = EL;
                                                            oLvClosingData = OB - EL;
                                                            CreditDays = oLvClosingData - Lockday;
                                                        }
                                                    }
                                                    if (_List_oLvNewReq.Where(e => e.LeaveHead.Id == LVname.Id).Count() >= 0)
                                                    {
                                                        var removeconvertedrec = _List_oLvNewReq.Where(e => e.LeaveHead.Id == LVname.Id).FirstOrDefault();
                                                        _List_oLvNewReq.Remove(removeconvertedrec);
                                                    }
                                                    LvNewReq newLvConvertobj = new LvNewReq
                                                    {
                                                        InputMethod = 0,
                                                        ReqDate = DateTime.Now,
                                                        CloseBal = EL,
                                                        OpenBal = 0,
                                                        LvOccurances = 0,
                                                        IsLock = true,
                                                        LvLapsed = 0,
                                                        //CreditDays = oLvClosingData,
                                                        CreditDays = EL,
                                                        //ToDate = CreditDate,
                                                        //FromDate = CreditDate,
                                                        LvCreditDate = Cal_Wise_Date,
                                                        LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                                            e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
                                                        GeoStruct = db.GeoStruct.Where(e => e.Id == GeoStruct).SingleOrDefault(),
                                                        PayStruct = db.PayStruct.Where(e => e.Id == PayStruct).SingleOrDefault(),
                                                        FuncStruct = db.FuncStruct.Where(e => e.Id == FuncStruct).SingleOrDefault(),
                                                        LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHead.Id).SingleOrDefault(),
                                                        Narration = settlementemp == true ? "Settlement Process" : "Credit Process",
                                                        LvCreditNextDate = NextCreditDays,
                                                        WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault(), //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
                                                        DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                                    };
                                                    _List_oLvNewReq.Add(newLvConvertobj);



                                                }
                                                else
                                                {
                                                    if (_List_oLvNewReq.Where(e => e.LeaveHead.Id == LVname.Id).Count() >= 0)
                                                    {
                                                        var removeconvertedrec = _List_oLvNewReq.Where(e => e.LeaveHead.Id == LVname.Id).FirstOrDefault();
                                                        _List_oLvNewReq.Remove(removeconvertedrec);
                                                    }
                                                    LvNewReq newLvConvertobj = new LvNewReq
                                                    {
                                                        InputMethod = 0,
                                                        ReqDate = DateTime.Now,
                                                        CloseBal = EL,
                                                        OpenBal = 0,
                                                        LvOccurances = 0,
                                                        IsLock = true,
                                                        LvLapsed = 0,
                                                        //CreditDays = oLvClosingData,
                                                        CreditDays = EL,
                                                        //ToDate = CreditDate,
                                                        //FromDate = CreditDate,
                                                        LvCreditDate = Cal_Wise_Date,
                                                        LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                                            e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
                                                        GeoStruct = db.GeoStruct.Where(e => e.Id == GeoStruct).SingleOrDefault(),
                                                        PayStruct = db.PayStruct.Where(e => e.Id == PayStruct).SingleOrDefault(),
                                                        FuncStruct = db.FuncStruct.Where(e => e.Id == FuncStruct).SingleOrDefault(),
                                                        LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHead.Id).SingleOrDefault(),
                                                        Narration = settlementemp == true ? "Settlement Process" : "Credit Process",
                                                        LvCreditNextDate = NextCreditDays,
                                                        WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault(), //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
                                                        DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                                    };
                                                    _List_oLvNewReq.Add(newLvConvertobj);

                                                }

                                            }

                                        }

                                    }
                                }
                                // Satara DCC bank convert encash leave 90 day lock


                                double newBal = 0, LvLapsed = 0;
                                if (oLvCreditPolicy.Accumalation == true)
                                {
                                    double tempcreditdays = CreditDays;
                                    CreditDays += oLvClosingData;

                                    if (CreditDays > oLvCreditPolicy.AccumalationLimit)
                                    {
                                        LvLapsed = CreditDays - oLvCreditPolicy.AccumalationLimit;
                                        CreditDays = oLvCreditPolicy.AccumalationLimit;
                                    }
                                    if (oLvCreditPolicy.AccumulationWithCredit == true)
                                    {
                                        if (CreditDays >= oLvCreditPolicy.AccumalationLimit)
                                        {
                                            double diff = oLvCreditPolicy.AccumalationLimit - oLvClosingData;
                                            tempcreditdays = diff;
                                            //newBal = oLvClosingData - diff;
                                            //LvLapsed = newBal;
                                            //CreditDays += newBal;
                                            //if (CreditDays > oLvCreditPolicy.AccumalationLimit)
                                            //{
                                            //    CreditDays = oLvCreditPolicy.AccumalationLimit;
                                            //}
                                        }
                                    }
                                    OLvCreditRecord.CreditDays = tempcreditdays;
                                    OLvCreditRecord.OpenBal = oLvClosingData;
                                    CreditDays -= OLvCreditRecord.DebitDays;
                                }
                                else
                                {
                                    // OLvCreditRecord.OpenBal = CreditDays;
                                    OLvCreditRecord.CreditDays = CreditDays;
                                    CreditDays -= OLvCreditRecord.DebitDays;
                                }


                                if (CreditDays != 0)
                                {
                                    //  var NextCreditDays = CreditDate.Value.AddYears(1);
                                    if (OLvCreditRecord.CreditDays == 0)
                                    {
                                        // OLvCreditRecord.CreditDays = CreditDays;
                                    }
                                    if (OLvCreditRecord.OpenBal == 0)
                                    {
                                        //OLvCreditRecord.OpenBal = CreditDays;
                                    }
                                    OLvCreditRecord.LvCreditDate = Cal_Wise_Date;
                                    OLvCreditRecord.InputMethod = 0;
                                    OLvCreditRecord.IsLock = true;
                                    OLvCreditRecord.ReqDate = DateTime.Now;
                                    OLvCreditRecord.CloseBal = CreditDays;
                                    OLvCreditRecord.LVCount = oLvOccurances;
                                    OLvCreditRecord.LvLapsed = LvLapsed;
                                    //OLvCreditRecord.ToDate = CreditDate;
                                    //OLvCreditRecord.FromDate = CreditDate;
                                    OLvCreditRecord.LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                    OLvCreditRecord.GeoStruct = db.GeoStruct.Where(e => e.Id == GeoStruct).SingleOrDefault();
                                    OLvCreditRecord.PayStruct = db.PayStruct.Where(e => e.Id == PayStruct).SingleOrDefault();
                                    OLvCreditRecord.FuncStruct = db.FuncStruct.Where(e => e.Id == FuncStruct).SingleOrDefault();
                                    OLvCreditRecord.LeaveHead = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OLvCreditRecord.Narration = settlementemp == true ? "Settlement Process" : "Credit Process";
                                    OLvCreditRecord.LvCreditNextDate = NextCreditDays;
                                    OLvCreditRecord.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();
                                    OLvCreditRecord.DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                    _List_oLvNewReq.Add(OLvCreditRecord);

                                    if (_List_oLvNewReq.Count > 0)
                                    {
                                        // LvOrignal id use foer leave cancel
                                        //if (_List_oLvNewReq.Count >= 2)
                                        //{
                                        //    _List_oLvNewReq[0].LvOrignal = OLvCreditRecord;
                                        //    if (_List_oLvNewReq.Count == 3)
                                        //    {
                                        //        _List_oLvNewReq[1].LvOrignal = OLvCreditRecord;
                                        //    }
                                        //}
                                        var _Emp = db.EmployeeLeave.Include(e => e.LvNewReq)
                                            .Where(e => e.Employee.Id == oEmployeeId).SingleOrDefault();
                                        for (int i = 0; i < _List_oLvNewReq.Count; i++)
                                        {
                                            _Emp.LvNewReq.Add(_List_oLvNewReq[i]);
                                        }
                                        db.Entry(_Emp).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                        if (trial == 1)
                        {
                            ts.Complete();
                        }
                    }
                }
            }
            catch (Exception e)
            {

                //throw e;
                using (DataBaseContext db = new DataBaseContext())
                {
                    P2B.UTILS.P2BLogger logger = new P2B.UTILS.P2BLogger();
                    var EmpCode = db.Employee.Find(oEmployeeId).EmpCode;
                    string ErrorMsg = e.Message;
                    string ErrorInnerMsg = e.InnerException != null ? e.InnerException.Message : "";
                    string LineNo = Convert.ToString(new StackTrace(e, true).GetFrame(0).GetFileLineNumber());
                    logger.Logging("Leave Credit Process " + "  " + "EmpCode :: " + EmpCode + " " + "ErrorMsg : " + ErrorMsg + " " + " ErrorInnerMsg : " + ErrorInnerMsg + " LineNo : " + LineNo);
                    string errorleavecreditprocess = ("Leave Credit Process " + "  " + "EmpCode :: " + EmpCode + " " + "ErrorMsg : " + ErrorMsg + " " + " ErrorInnerMsg : " + ErrorInnerMsg + " LineNo : " + LineNo);
                    LeaveCreditProcessMsg.Add(errorleavecreditprocess);
                    System.Web.HttpContext.Current.Session["LeaveCreditProcessMsg"] = LeaveCreditProcessMsg;
                }
            }

            return 0;
        }

        public static Int32 LvCreditProceessLeaveBank(Int32 oEmployeeId, Int32 CompLvId, CompanyLeave CompCreditPolicyLists, List<Int32> LvHead_ids_list, Int32 LvCalendarId, int trial, int compid, DateTime? FromDate, DateTime? ToDate, string CreditDatelist, Boolean settlementemp)
        {
            try
            {
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                                             new System.TimeSpan(0, 30, 0)))
                {
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        int OEmpLvID = db.EmployeeLeave.Where(e => e.Employee.Id == oEmployeeId).Select(e => e.Id).FirstOrDefault();
                        //Employee Lv Data
                        int crd = Convert.ToInt32(CreditDatelist);
                        //string creditdtlist = db.LookupValue.Where(e => e.Id == crd).FirstOrDefault().LookupVal.ToUpper();
                        string creditdtlist = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "447").FirstOrDefault().LookupValues.Where(e => e.Id == crd).FirstOrDefault().LookupVal.ToUpper();
                        foreach (var LvHead_ids in LvHead_ids_list)
                        {
                            var EmpLvHeadList = db.LvHead.Where(e => e.Id == LvHead_ids)
                                .Select(a => new { Id = a.Id, LvCode = a.LvCode }).Distinct().AsParallel().ToList();
                            int lastyearid = 0;
                            double mSusDayFull = 0;


                            Company OCompany = null;
                            OCompany = db.Company.Find(compid);

                            foreach (var item in EmpLvHeadList)
                            {
                                //prev credit process check
                                //end
                                //Get LvCredit Policy For Particular Lv

                                LvNewReq OLvCreditRecord = new LvNewReq();
                                //EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                                //     .Include(e => e.EmployeeLvStructDetails)
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvBankPolicy))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvBankPolicy.LvHeadCollection))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.CreditDate))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ConvertLeaveHead))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.LvHead))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ConvertLeaveHeadBal))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ExcludeLeaveHeads))
                                //     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead.LvHeadOprationType))
                                //         .Where(e => e.EndDate == null && e.EmployeeLeave_Id == OEmpLvID).SingleOrDefault();

                                var OLvSalStruct = db.EmployeeLvStruct.Select(d => new
                                {
                                    OEndDate = d.EndDate,
                                    OEffectiveDate = d.EffectiveDate,
                                    OEmployeeLeaveId = d.EmployeeLeave_Id,
                                    OEmployeeLvStructDetails = d.EmployeeLvStructDetails.Select(r => new
                                    {
                                        OLvHeadFormula = r.LvHeadFormula,
                                        OEmployeeLvStructDetailsLvHeadId = r.LvHead_Id,
                                        OEmployeeLvStructDetailsLvHead = r.LvHead,
                                        OLvHeadOprationType = r.LvHead.LvHeadOprationType,
                                        OLvBankPolicy = r.LvHeadFormula.LvBankPolicy,
                                        OLvHeadCollection = r.LvHeadFormula.LvBankPolicy.LvHeadCollection,
                                        OLvCreditPolicy = r.LvHeadFormula.LvCreditPolicy,
                                        OCreditDate = r.LvHeadFormula.LvCreditPolicy.CreditDate,
                                        OConvertLeaveHead = r.LvHeadFormula.LvCreditPolicy.ConvertLeaveHead,
                                        OLvCreditPolicyLvHead = r.LvHeadFormula.LvCreditPolicy.LvHead,
                                        OConvertLeaveHeadBal = r.LvHeadFormula.LvCreditPolicy.ConvertLeaveHeadBal,
                                        OExcludeLeaveHeads = r.LvHeadFormula.LvCreditPolicy.ExcludeLeaveHeads

                                    }).ToList()
                                }).Where(e => e.OEndDate == null && e.OEmployeeLeaveId == OEmpLvID).SingleOrDefault();


                                LvCreditPolicy oLvCreditPolicy = null;
                                if (OLvSalStruct != null)
                                {
                                    oLvCreditPolicy = OLvSalStruct.OEmployeeLvStructDetails.Where(e => e.OEmployeeLvStructDetailsLvHeadId == item.Id && e.OLvHeadFormula != null && e.OLvCreditPolicy != null).Select(r => r.OLvCreditPolicy).FirstOrDefault();

                                }

                                if (oLvCreditPolicy == null)
                                {
                                    continue;
                                }
                                if (oLvCreditPolicy.CreditDate.LookupVal.ToUpper() != creditdtlist)
                                {
                                    continue;
                                }
                                // settlement Leave Process start
                                //EmployeeLeave _Prv_EmpLvDatasetemp = db.EmployeeLeave.Where(a => a.Employee.Id == oEmployeeId)
                                //    .Include(a => a.LvNewReq)
                                //    .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                                //    .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                                //    .Include(a => a.LvNewReq.Select(e => e.GeoStruct))
                                //    .Include(a => a.LvNewReq.Select(e => e.PayStruct))
                                //    .Include(a => a.LvNewReq.Select(e => e.FuncStruct))
                                //    .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                                //    .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.LeaveHead.Id == item.Id
                                //        ))
                                //        .SingleOrDefault();
                                // if settlement employee leave credit then leave should not credit
                                var _Prv_EmpLvDatasetemp = db.LvNewReq.Where(a => a.LeaveHead != null && a.LeaveHead.Id == item.Id && a.EmployeeLeave_Id == OEmpLvID && a.Narration == "Settlement Process").FirstOrDefault();

                                if (_Prv_EmpLvDatasetemp != null)
                                {
                                    //var LvCreditedInNewYrChksetemp = _Prv_EmpLvDatasetemp.LvNewReq
                                    //        .Where(a => a.Narration == "Settlement Process"
                                    //        && a.LeaveHead.Id == item.Id).FirstOrDefault();
                                    //if (LvCreditedInNewYrChksetemp != null)
                                    //{
                                    continue;
                                    //  }
                                }
                                // settlement Leave Process End

                                DateTime? Lastyear = null, CreditDate = null, tempCreditDate = null, Cal_Wise_Date = null, Lastyearj = null;
                                Double CreditDays = 0, SumDays = 0, oOpenBal = 0, oCloseingbal = 0;
                                Calendar LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                    e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                                DateTime? tempRetireDate = null, tempRetirecrDate = null, leaveyearfrom = null, leaveyearto = null;

                                switch (oLvCreditPolicy.CreditDate.LookupVal.ToUpper())
                                {
                                    case "CALENDAR":
                                        Cal_Wise_Date = LvCalendar.FromDate;
                                        CreditDate = LvCalendar.FromDate.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearid1 = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyear
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearid1 != null)
                                        {

                                            lastyearid = lastyearid1.Id;
                                        }

                                        break;
                                    case "YEARLY":
                                        Cal_Wise_Date = LvCalendar.FromDate;
                                        CreditDate = LvCalendar.FromDate.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearid11 = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyear
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearid11 != null)
                                        {

                                            lastyearid = lastyearid11.Id;
                                        }

                                        break;
                                    case "JOININGDATE":
                                        //var JoiningDate = db.Employee.Include(e => e.ServiceBookDates)
                                        //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        //tempCreditDate = JoiningDate.ServiceBookDates.JoiningDate;


                                        var LvNewReqData = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aa = null;
                                        DateTime? joinincr = null;
                                        var Fulldate = "";
                                        if (LvNewReqData != null)
                                        {
                                            aa = LvNewReqData.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aa != null)
                                        {
                                            Fulldate = aa.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.JoiningDate != null && q.Employee.Id == oEmployeeId).SingleOrDefault();

                                            if (ServiceBookData != null)
                                            {
                                                if (ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month <= ToDate.Value.Month)
                                                {

                                                    Fulldate = ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                }
                                            }
                                        }
                                        joinincr = Convert.ToDateTime(Fulldate);
                                        CreditDate = joinincr.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidj = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidj != null)
                                        {
                                            lastyearid = lastyearidj.Id;
                                        }
                                        Cal_Wise_Date = joinincr;
                                        break;
                                    case "CONFIRMATIONDATE":
                                        //var ConfirmationDate = db.Employee.Include(e => e.ServiceBookDates)
                                        //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        //tempCreditDate = ConfirmationDate.ServiceBookDates.ConfirmationDate;

                                        var LvNewReqDatac = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aac = null;
                                        DateTime? joinincrc = null;
                                        var Fulldatec = "";
                                        if (LvNewReqDatac != null)
                                        {
                                            aac = LvNewReqDatac.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aac != null)
                                        {
                                            Fulldatec = aac.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.ConfirmationDate != null && q.Employee.Id == oEmployeeId).SingleOrDefault();

                                            if (ServiceBookData != null)
                                            {
                                                if (ServiceBookData.Employee.ServiceBookDates.ConfirmationDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.ConfirmationDate.Value.Month <= ToDate.Value.Month)
                                                {

                                                    Fulldatec = ServiceBookData.Employee.ServiceBookDates.ConfirmationDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                }
                                            }
                                        }
                                        joinincrc = Convert.ToDateTime(Fulldatec);
                                        CreditDate = joinincrc.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincrc.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidc = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidc != null)
                                        {
                                            lastyearid = lastyearidc.Id;
                                        }
                                        Cal_Wise_Date = joinincrc;



                                        break;
                                    case "INCREMENTDATE":
                                        //var LastIncrementDate = db.Employee.Include(e => e.ServiceBookDates)
                                        //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        //tempCreditDate = LastIncrementDate.ServiceBookDates.LastIncrementDate;
                                        DateTime? joinincrI = null;
                                        var FulldateI = "";
                                        var Emp_lvcr = db.EmployeePayroll
                                            .Include(a => a.Employee)
                                            .Include(e => e.IncrementServiceBook)
                                            .Include(e => e.IncrementServiceBook.Select(q => q.IncrActivity))
                                            .Where(q => q.Employee.Id == oEmployeeId).SingleOrDefault();
                                        if (Emp_lvcr != null)
                                        {
                                            var fgds = Emp_lvcr.IncrementServiceBook.Where(q => q.ReleaseDate != null
                                                 && q.IncrActivity.Id == 1).OrderBy(q => q.Id).LastOrDefault();
                                            if (fgds != null)
                                            {
                                                if (fgds.ReleaseDate.Value.Month == FromDate.Value.Month)
                                                {
                                                    FulldateI = fgds.ReleaseDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                }
                                            }
                                            else
                                            {
                                                var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.LastIncrementDate == null && q.Employee.Id == oEmployeeId).SingleOrDefault();

                                                if (ServiceBookData != null)
                                                {
                                                    if (ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month <= ToDate.Value.Month)
                                                    {

                                                        FulldateI = ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                    }
                                                }
                                            }

                                        }

                                        joinincrI = Convert.ToDateTime(FulldateI);
                                        CreditDate = joinincrI.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincrI.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidI = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidI != null)
                                        {
                                            lastyearid = lastyearidI.Id;
                                        }
                                        Cal_Wise_Date = joinincrI;

                                        break;
                                    case "PROMOTIONDATE":
                                        //var LastPromotionDate = db.Employee.Include(e => e.ServiceBookDates)
                                        //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        //tempCreditDate = LastPromotionDate.ServiceBookDates.LastPromotionDate;

                                        DateTime? joinincrP = null;
                                        var FulldateP = "";
                                        var Emp_lvcrP = db.EmployeePayroll
                                            .Include(a => a.Employee)
                                            .Include(e => e.PromotionServiceBook)
                                            .Include(e => e.PromotionServiceBook.Select(q => q.PromotionActivity))
                                            .Where(q => q.Employee.Id == oEmployeeId).SingleOrDefault();
                                        if (Emp_lvcrP != null)
                                        {
                                            var fgds = Emp_lvcrP.PromotionServiceBook.Where(q => q.ReleaseDate != null
                                                 && q.PromotionActivity.Id == 2).OrderBy(q => q.Id).LastOrDefault();
                                            if (fgds != null)
                                            {
                                                if (fgds.ReleaseDate.Value.Month == FromDate.Value.Month)
                                                {
                                                    FulldateP = fgds.ReleaseDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                }
                                            }
                                            else
                                            {
                                                var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.LastPromotionDate == null && q.Employee.Id == oEmployeeId).SingleOrDefault();

                                                if (ServiceBookData != null)
                                                {
                                                    if (ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month <= ToDate.Value.Month)
                                                    {

                                                        FulldateP = ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                    }
                                                }
                                            }

                                        }

                                        joinincrP = Convert.ToDateTime(FulldateP);
                                        CreditDate = joinincrP.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincrP.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidP = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidP != null)
                                        {
                                            lastyearid = lastyearidP.Id;
                                        }
                                        Cal_Wise_Date = joinincrP;

                                        break;
                                    case "FIXDAYS":
                                        if (OCompany.Code.ToString() == "ACABL")
                                        {
                                            DateTime? Fixdays = null;
                                            Fixdays = Convert.ToDateTime("01/01/" + DateTime.Now.Year);

                                            Cal_Wise_Date = Fixdays;
                                            CreditDate = Fixdays.Value.AddDays(-1);
                                            Lastyear = Convert.ToDateTime(Fixdays.Value.AddYears(-1));

                                        }
                                        break;
                                    case "QUARTERLY":
                                        var LvNewReqDataQUARTERLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aaQUARTERLY = null;
                                        var FulldateQUARTERLY = "";
                                        if (LvNewReqDataQUARTERLY != null)
                                        {
                                            aaQUARTERLY = LvNewReqDataQUARTERLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aaQUARTERLY != null)
                                        {
                                            FulldateQUARTERLY = aaQUARTERLY.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            FulldateQUARTERLY = OLvSalStruct.OEffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

                                        }
                                        joinincr = Convert.ToDateTime(FulldateQUARTERLY);
                                        CreditDate = joinincr.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidjQUARTERLY = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidjQUARTERLY != null)
                                        {
                                            lastyearid = lastyearidjQUARTERLY.Id;
                                        }
                                        Cal_Wise_Date = joinincr;

                                        break;
                                    case "HALFYEARLY":
                                        var LvNewReqDataHALFYEARLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aaHALFYEARLY = null;
                                        var FulldateHALFYEARLY = "";
                                        if (LvNewReqDataHALFYEARLY != null)
                                        {
                                            aaHALFYEARLY = LvNewReqDataHALFYEARLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aaHALFYEARLY != null)
                                        {
                                            FulldateHALFYEARLY = aaHALFYEARLY.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            FulldateHALFYEARLY = OLvSalStruct.OEffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

                                        }
                                        joinincr = Convert.ToDateTime(FulldateHALFYEARLY);
                                        CreditDate = joinincr.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidjHALFYEARLY = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidjHALFYEARLY != null)
                                        {
                                            lastyearid = lastyearidjHALFYEARLY.Id;
                                        }
                                        Cal_Wise_Date = joinincr;
                                        break;
                                    case "MONTHLY":
                                        var LvNewReqDataMONTHLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aaMONTHLY = null;
                                        var FulldateMONTHLY = "";
                                        if (LvNewReqDataMONTHLY != null)
                                        {
                                            aaMONTHLY = LvNewReqDataMONTHLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aaMONTHLY != null)
                                        {
                                            FulldateMONTHLY = aaMONTHLY.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            FulldateMONTHLY = OLvSalStruct.OEffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

                                        }
                                        joinincr = Convert.ToDateTime(FulldateMONTHLY);
                                        CreditDate = joinincr.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidjMONTHLY = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidjMONTHLY != null)
                                        {
                                            lastyearid = lastyearidjMONTHLY.Id;
                                        }
                                        Cal_Wise_Date = joinincr;
                                        break;
                                    case "BIMONTHLY":
                                        var LvNewReqDataBIMONTHLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aaBIMONTHLY = null;
                                        var FulldateBIMONTHLY = "";
                                        if (LvNewReqDataBIMONTHLY != null)
                                        {
                                            aaBIMONTHLY = LvNewReqDataBIMONTHLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aaBIMONTHLY != null)
                                        {
                                            FulldateBIMONTHLY = aaBIMONTHLY.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            FulldateBIMONTHLY = OLvSalStruct.OEffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

                                        }
                                        joinincr = Convert.ToDateTime(FulldateBIMONTHLY);
                                        CreditDate = joinincr.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidjBIMONTHLY = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidjBIMONTHLY != null)
                                        {
                                            lastyearid = lastyearidjBIMONTHLY.Id;
                                        }
                                        Cal_Wise_Date = joinincr;
                                        break;

                                    case "OTHER":
                                        //var EmpService = db.Employee.Include(e => e.ServiceBookDates)
                                        //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        //tempCreditDate = EmpService.ServiceBookDates.JoiningDate;
                                        break;
                                    default:
                                        break;
                                }

                                // settlement Leave Process start
                                double retmonthResignRetire = 0;
                                Double oldcreditlv = 0;
                                Double oldcreditlvclose = 0;
                                if (settlementemp == true)
                                {


                                    var LvNewReqDatasettle = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                    LvNewReq aaSettle = null;
                                    DateTime? Settletilldate = null;
                                    var Onworkingcreditednextcreditdate = "";
                                    var FulldateSettleStartdate = "";

                                    if (LvNewReqDatasettle != null)
                                    {
                                        aaSettle = LvNewReqDatasettle.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                    }
                                    if (aaSettle != null)
                                    {
                                        Onworkingcreditednextcreditdate = aaSettle.LvCreditNextDate.Value.ToShortDateString();
                                        FulldateSettleStartdate = aaSettle.LvCreditDate.Value.ToShortDateString();
                                        oldcreditlv = aaSettle.CreditDays;
                                        oldcreditlvclose = aaSettle.CloseBal;
                                    }

                                    // Resign,EXPIRED,TERMINATION
                                    var OOtherServiceBook = db.EmployeePayroll.Where(e => e.Employee.Id == oEmployeeId)
                                    .Include(e => e.OtherServiceBook)

                                     .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity))
                                     .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity.OtherSerBookActList))
                                   .SingleOrDefault();
                                    var OOOtherServiceBook = OOtherServiceBook.OtherServiceBook.Where(e => e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "RESIGNED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "EXPIRED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "TERMINATION").Select(x => x.OthServiceBookActivity.OtherSerBookActList)
                                                             .FirstOrDefault();
                                    if (OOOtherServiceBook != null)
                                    {
                                        string check = OOOtherServiceBook.LookupVal.ToUpper().ToString();
                                        if (check != null)
                                        {
                                            if (check == "RESIGNED" || check == "EXPIRED" || check == "TERMINATION")
                                            {
                                                var OOOtherServiceBookdate = OOtherServiceBook.OtherServiceBook.Where(e => e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "RESIGNED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "EXPIRED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "TERMINATION")
                                                            .FirstOrDefault();

                                                Settletilldate = Convert.ToDateTime(OOOtherServiceBookdate.ProcessOthDate);// Resign employee leave credit on request date of resignation
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var OOtherServiceBookret = db.Employee.Where(e => e.Id == oEmployeeId)
                                                                            .Include(e => e.ServiceBookDates)
                                                                           .SingleOrDefault();
                                        if (OOtherServiceBookret != null)
                                        {
                                            Settletilldate = Convert.ToDateTime(OOtherServiceBookret.ServiceBookDates.RetirementDate);
                                        }

                                    }

                                    CreditDate = Convert.ToDateTime(Settletilldate); //31/12/2020   
                                    Lastyear = Convert.ToDateTime(FulldateSettleStartdate); //01/12/2020
                                    Cal_Wise_Date = CreditDate;
                                    // settle emp retire/resign month start

                                    if (Lastyear.Value.Day >= 15)
                                    {
                                        // retmonth=
                                        int compMonthResignRetire = (Lastyear.Value.Month + Lastyear.Value.Year * 12) - (Settletilldate.Value.Month + Settletilldate.Value.Year * 12);

                                        retmonthResignRetire = compMonthResignRetire + 1;

                                    }
                                    else
                                    {
                                        int compMonthResignRetire = (Lastyear.Value.Month + Lastyear.Value.Year * 12) - (Settletilldate.Value.Month + Settletilldate.Value.Year * 12);

                                        retmonthResignRetire = compMonthResignRetire;
                                    }
                                    if (retmonthResignRetire < 0)
                                    {
                                        retmonthResignRetire = 0;
                                    }
                                    retmonthResignRetire = Math.Round(retmonthResignRetire, 0);

                                    // settle emp retire/resign month end
                                }
                                // settlement Leave Process end

                                if (CreditDate == null && Lastyear == null)
                                {
                                    CreditDate = Convert.ToDateTime(tempCreditDate.Value.Day + "/" + tempCreditDate.Value.Month + "/" + DateTime.Now.Year);
                                    leaveyearfrom = CreditDate;
                                    leaveyearto = leaveyearfrom.Value.AddDays(-1);
                                    CreditDate = CreditDate.Value.AddDays(-1);
                                    Lastyear = CreditDate.Value.AddYears(-1);
                                }


                                leaveyearfrom = LvCalendar.FromDate;
                                leaveyearto = LvCalendar.ToDate;
                                double retmonth = 0;
                                var retiredate = db.Employee.Include(e => e.ServiceBookDates)
                                            .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                tempRetireDate = retiredate.ServiceBookDates.RetirementDate;
                                if (tempRetireDate != null)
                                {
                                    if (leaveyearto > tempRetireDate)
                                    {
                                        tempRetirecrDate = tempRetireDate;
                                    }
                                    else
                                    {
                                        tempRetirecrDate = leaveyearto;
                                    }
                                }
                                else
                                {
                                    tempRetirecrDate = leaveyearto;
                                }
                                if (tempRetirecrDate.Value.Day >= 15)
                                {
                                    // retmonth=
                                    int compMonth = (tempRetirecrDate.Value.Month + tempRetirecrDate.Value.Year * 12) - (leaveyearfrom.Value.Month + leaveyearfrom.Value.Year * 12);
                                    //double daysInEndMonth = (tempRetirecrDate - tempRetirecrDate.Value.AddMonths(1)).Value.Days;
                                    //retmonth = compMonth + (leaveyearfrom.Value.Day - tempRetirecrDate.Value.Day) / daysInEndMonth;
                                    retmonth = compMonth + 1;

                                }
                                else
                                {
                                    int compMonth = (tempRetirecrDate.Value.Month + tempRetirecrDate.Value.Year * 12) - (leaveyearfrom.Value.Month + leaveyearfrom.Value.Year * 12);
                                    //double daysInEndMonth = (tempRetirecrDate - tempRetirecrDate.Value.AddMonths(1)).Value.Days;
                                    //retmonth = compMonth + (leaveyearfrom.Value.Day - tempRetirecrDate.Value.Day) / daysInEndMonth;
                                    retmonth = compMonth;
                                }
                                if (retmonth < 0)
                                {
                                    retmonth = 0;
                                }
                                retmonth = Math.Round(retmonth, 0);

                                int compMonthLYR = (leaveyearto.Value.Month + leaveyearto.Value.Year * 12) - (leaveyearfrom.Value.Month + leaveyearfrom.Value.Year * 12);
                                double daysInEndMonthLYR = (leaveyearto - leaveyearto.Value.AddMonths(1)).Value.Days;
                                double LYRmonth = compMonthLYR + (leaveyearfrom.Value.Day - leaveyearto.Value.Day) / daysInEndMonthLYR;
                                LYRmonth = Math.Round(LYRmonth, 0);

                                // DateTime NextCreditDays = Cal_Wise_Date.Value.AddYears(1); //comment line because credit frequeny avilable in system if yearly then 12 half yearly then 6 if monthyly then 1 frequency 
                                DateTime NextCreditDays = Cal_Wise_Date.Value.AddMonths(oLvCreditPolicy.ProCreditFrequency);

                                EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == oEmployeeId)
                                    .Include(a => a.LvNewReq)
                                    .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                                    .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                                    .Include(a => a.LvNewReq.Select(e => e.GeoStruct))
                                    .Include(a => a.LvNewReq.Select(e => e.PayStruct))
                                    .Include(a => a.LvNewReq.Select(e => e.FuncStruct))
                                    .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                                    .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.LeaveHead.Id == item.Id
                                        //  && a.LeaveCalendar.FromDate == Lastyear && a.LeaveCalendar.ToDate == CreditDate
                                        ))
                                        .SingleOrDefault();
                                List<LvNewReq> Filter_oEmpLvData = new List<LvNewReq>();
                                double oLvClosingData = 0;
                                double UtilizedLv = 0;
                                Int32 GeoStruct = 0;
                                Int32 PayStruct = 0;
                                Int32 FuncStruct = 0;
                                double oLvOccurances = 0;

                                EmployeeLeave _Empobj = db.EmployeeLeave.Where(e => e.Employee.Id == oEmployeeId)
                                    .Include(e => e.Employee.GeoStruct)
                                    .Include(e => e.Employee.PayStruct)
                                    .Include(e => e.Employee.FuncStruct)
                                    .SingleOrDefault();

                                GeoStruct = _Empobj.Employee.GeoStruct.Id;
                                PayStruct = _Empobj.Employee.PayStruct.Id;
                                FuncStruct = _Empobj.Employee.FuncStruct.Id;

                                // Check if leave Credited for that year then should not credit start
                                List<LvNewReq> _List_oLvNewReq = new List<LvNewReq>();
                                LvNewReq newLvbankdebtreq = new LvNewReq();
                                if (_Prv_EmpLvData != null)
                                {
                                    var LvCreditedInNewYrChk1 = _Prv_EmpLvData.LvNewReq
                                            .Where(a => a.LvCreditNextDate != null && a.LvOrignal_Id == null && a.LvCreditNextDate.Value.Date == NextCreditDays.Date
                                            && a.LeaveHead.Id == item.Id).FirstOrDefault();
                                    if (LvCreditedInNewYrChk1 != null)
                                    {
                                        if (LvCreditedInNewYrChk1.CreditDays != 0)
                                        {

                                            //lvbank start
                                            //var _LvBankPolicy = db.LvBankPolicy.Include(e => e.LvHeadCollection)
                                            //    .Where(q => q.LvHeadCollection.Any(r => r.Id == item.Id)).SingleOrDefault();

                                            //LvBankPolicy _LvBankPolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == item.Id && e.LvHeadFormula.LvCreditPolicy != null).Select(r => r.LvHeadFormula.LvBankPolicy).FirstOrDefault();
                                            var _LvBankPolicylchead = OLvSalStruct.OEmployeeLvStructDetails.Where(e => e.OLvHeadFormula != null && e.OLvBankPolicy != null).SingleOrDefault();
                                            var _LvBankPolicylv = _LvBankPolicylchead != null ? _LvBankPolicylchead.OLvHeadFormula.LvBankPolicy.LvHeadCollection.Where(e => e.Id == item.Id).SingleOrDefault() : null;
                                            if (_LvBankPolicylv != null)
                                            {
                                                LvBankPolicy _LvBankPolicy = _LvBankPolicylchead.OLvHeadFormula.LvBankPolicy;

                                                double debitpolicydays = 0;
                                                var JoiningDate = db.Employee.Include(e => e.ServiceBookDates)
                                                  .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                                DateTime tempCreditDate1 = Convert.ToDateTime(JoiningDate.ServiceBookDates.JoiningDate);
                                                DateTime till = CreditDate.Value.AddDays(1);
                                                int compMonths = (till.Month + till.Year * 12) - (tempCreditDate1.Month + tempCreditDate1.Year * 12);
                                                double daysInEndMonths = (till - till.AddMonths(1)).Days;
                                                double monthss = compMonths + (tempCreditDate1.Day - till.Day) / daysInEndMonths;
                                                var GetServiceYear = Math.Round(monthss / 12, 0);
                                                if (_LvBankPolicy.IsSeviceLockOnDebit == true)
                                                {
                                                    if (GetServiceYear <= _LvBankPolicy.MaxServiceForDebit)
                                                    {
                                                        LvBankOpenBal OLvCreditRecordLvBankOpenBal = new LvBankOpenBal();

                                                        double lvbankcloseingbal = 0;
                                                        var _LvBankOpenBalprv = db.LvBankLedger
                                                          .OrderByDescending(e => e.Id)
                                                         .FirstOrDefault();
                                                        if (_LvBankOpenBalprv != null)
                                                        {
                                                            lvbankcloseingbal = _LvBankOpenBalprv.ClosingBalance;
                                                        }


                                                        // reqution code
                                                        if (LvCreditedInNewYrChk1 != null)
                                                        {

                                                            newLvbankdebtreq = new LvNewReq
                                                            {
                                                                InputMethod = 0,
                                                                ReqDate = DateTime.Now,
                                                                CloseBal = LvCreditedInNewYrChk1.CloseBal - _LvBankPolicy.LvDebitInCredit,
                                                                OpenBal = LvCreditedInNewYrChk1.CloseBal,
                                                                LvOccurances = 0,
                                                                IsLock = true,
                                                                LvLapsed = 0,
                                                                DebitDays = _LvBankPolicy.LvDebitInCredit,
                                                                CreditDays = 0,
                                                                //ToDate = CreditDate,
                                                                //FromDate = CreditDate,
                                                                LvCreditDate = Cal_Wise_Date,
                                                                LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                                                    e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
                                                                GeoStruct = db.GeoStruct.Where(e => e.Id == GeoStruct).SingleOrDefault(),
                                                                PayStruct = db.PayStruct.Where(e => e.Id == PayStruct).SingleOrDefault(),
                                                                FuncStruct = db.FuncStruct.Where(e => e.Id == FuncStruct).SingleOrDefault(),
                                                                LeaveHead = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault(),
                                                                Narration = settlementemp == true ? "Settlement Process" : "Credit Process",
                                                                Reason = "Leave Bank Debit Request",
                                                                LvCreditNextDate = NextCreditDays,
                                                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault(),  //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
                                                                DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                                            };
                                                            _List_oLvNewReq.Add(newLvbankdebtreq);

                                                            // LvBankLedger OLvCreditRecordLvBankLedger = new LvBankLedger();
                                                            // var _LvBankLedger = db.LvBankLedger.Where(e => e.EmployeeLeave_Id == OEmpLvID).SingleOrDefault();
                                                            var calid = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                                                            //    var LvBankOpenBal_data = db.LvBankOpenBal.Where(e => e.LvCalendar_Id == calid.Id).SingleOrDefault();

                                                            LvBankLedger _OLvCreditRecordLvBankLedger = new LvBankLedger()
                                                            {
                                                                OpeningBalance = lvbankcloseingbal,
                                                                CreditDays = _LvBankPolicy.LvDebitInCredit,
                                                                ClosingBalance = lvbankcloseingbal + _LvBankPolicy.LvDebitInCredit,
                                                                CreditDate = DateTime.Now,
                                                                DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                                                LvNewReq = newLvbankdebtreq,
                                                                Narration = "Credit Process",
                                                                EmployeeLeave_Id = OEmpLvID,

                                                            };

                                                            db.LvBankLedger.Add(_OLvCreditRecordLvBankLedger);
                                                            db.SaveChanges();


                                                            var companyLeave = new CompanyLeave();
                                                            companyLeave = db.CompanyLeave.Include(e => e.LvBankLedger).Where(e => e.Company.Id == OCompany.Id).SingleOrDefault();
                                                            if (companyLeave != null)
                                                            {
                                                                var LvBankLedger_list = new List<LvBankLedger>();
                                                                if (companyLeave.LvBankLedger.Count() > 0)
                                                                {
                                                                    LvBankLedger_list.AddRange(companyLeave.LvBankLedger);
                                                                }

                                                                LvBankLedger_list.Add(_OLvCreditRecordLvBankLedger);
                                                                companyLeave.LvBankLedger = LvBankLedger_list;
                                                                db.CompanyLeave.Attach(companyLeave);
                                                                db.Entry(companyLeave).State = System.Data.Entity.EntityState.Modified;
                                                                db.SaveChanges();
                                                                db.Entry(companyLeave).State = System.Data.Entity.EntityState.Detached;
                                                            }



                                                        }




                                                    }
                                                }//service not depend start
                                                else
                                                {

                                                    LvBankOpenBal OLvCreditRecordLvBankOpenBal = new LvBankOpenBal();
                                                    //var _LvBankOpenBalprv = db.LvBankOpenBal
                                                    //   .Where(a => a.CreditDate >= Lastyear &&
                                                    //              a.CreditDate <= CreditDate
                                                    //  ).SingleOrDefault();

                                                    double lvbankcloseingbal = 0;
                                                    var _LvBankOpenBalprv = db.LvBankLedger
                                                      .OrderByDescending(e => e.Id)
                                                     .FirstOrDefault();
                                                    if (_LvBankOpenBalprv != null)
                                                    {
                                                        lvbankcloseingbal = _LvBankOpenBalprv.ClosingBalance;
                                                    }

                                                    //if (_LvBankOpenBalprv != null)
                                                    //{
                                                    //    var _LvBankOpenBal = db.LvBankOpenBal
                                                    //    .Where(e => e.LvCalendar.Id == LvCalendarId
                                                    //   ).SingleOrDefault();

                                                    //    if (_LvBankOpenBal == null)
                                                    //    {
                                                    //        // OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
                                                    //        debitpolicydays = _LvBankPolicy.LvDebitInCredit;
                                                    //        LvBankOpenBal _OLvCreditRecordLvBankOpenBal = new LvBankOpenBal()
                                                    //        {
                                                    //            OpeningBalance = _LvBankOpenBalprv.ClosingBalance,
                                                    //            CreditDays = _LvBankPolicy.LvDebitInCredit,

                                                    //            UtilizedDays = 0,
                                                    //            CreditDate = DateTime.Now,
                                                    //            DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                                    //            LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault()
                                                    //        };

                                                    //        db.LvBankOpenBal.Add(_OLvCreditRecordLvBankOpenBal);
                                                    //        db.SaveChanges();
                                                    //    }
                                                    //    else
                                                    //    {
                                                    //        // OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
                                                    //        debitpolicydays = _LvBankPolicy.LvDebitInCredit;
                                                    //        _LvBankOpenBal.CreditDays = _LvBankOpenBal.CreditDays + _LvBankPolicy.LvDebitInCredit;
                                                    //        _LvBankOpenBal.UtilizedDays = 0;
                                                    //        _LvBankOpenBal.CreditDate = DateTime.Now;
                                                    //        _LvBankOpenBal.DBTrack = new DBTrack() { Action = "M", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                                    //        _LvBankOpenBal.LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                                                    //        db.LvBankOpenBal.Attach(_LvBankOpenBal);
                                                    //        db.Entry(_LvBankOpenBal).State = System.Data.Entity.EntityState.Modified;
                                                    //        db.SaveChanges();
                                                    //    }

                                                    //}
                                                    //else
                                                    //{
                                                    //    var _LvBankOpenBal = db.LvBankOpenBal
                                                    //   .Where(e => e.LvCalendar.Id == LvCalendarId
                                                    //  ).SingleOrDefault();

                                                    //    if (_LvBankOpenBal == null)
                                                    //    {
                                                    //        //  OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
                                                    //        debitpolicydays = _LvBankPolicy.LvDebitInCredit;
                                                    //        LvBankOpenBal _OLvCreditRecordLvBankOpenBal = new LvBankOpenBal()
                                                    //        {
                                                    //            OpeningBalance = 0,
                                                    //            CreditDays = _LvBankPolicy.LvDebitInCredit,
                                                    //            UtilizedDays = 0,
                                                    //            CreditDate = DateTime.Now,
                                                    //            DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                                    //            LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault()
                                                    //        };

                                                    //        db.LvBankOpenBal.Add(_OLvCreditRecordLvBankOpenBal);
                                                    //        db.SaveChanges();
                                                    //    }
                                                    //    else
                                                    //    {
                                                    //        //  OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
                                                    //        debitpolicydays = _LvBankPolicy.LvDebitInCredit;
                                                    //        _LvBankOpenBal.CreditDays = _LvBankOpenBal.CreditDays + _LvBankPolicy.LvDebitInCredit;
                                                    //        _LvBankOpenBal.UtilizedDays = 0;
                                                    //        _LvBankOpenBal.CreditDate = DateTime.Now;
                                                    //        _LvBankOpenBal.DBTrack = new DBTrack() { Action = "M", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                                    //        _LvBankOpenBal.LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                                                    //        db.LvBankOpenBal.Attach(_LvBankOpenBal);
                                                    //        db.Entry(_LvBankOpenBal).State = System.Data.Entity.EntityState.Modified;
                                                    //        db.SaveChanges();

                                                    //    }
                                                    //}

                                                    // reqution code

                                                    if (LvCreditedInNewYrChk1 != null)
                                                    {

                                                        newLvbankdebtreq = new LvNewReq
                                                        {
                                                            InputMethod = 0,
                                                            ReqDate = DateTime.Now,
                                                            CloseBal = LvCreditedInNewYrChk1.CloseBal - _LvBankPolicy.LvDebitInCredit,
                                                            OpenBal = LvCreditedInNewYrChk1.CloseBal,
                                                            LvOccurances = 0,
                                                            IsLock = true,
                                                            LvLapsed = 0,
                                                            DebitDays = _LvBankPolicy.LvDebitInCredit,
                                                            CreditDays = 0,
                                                            //ToDate = CreditDate,
                                                            //FromDate = CreditDate,
                                                            LvCreditDate = Cal_Wise_Date,
                                                            LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                                                e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
                                                            GeoStruct = db.GeoStruct.Where(e => e.Id == GeoStruct).SingleOrDefault(),
                                                            PayStruct = db.PayStruct.Where(e => e.Id == PayStruct).SingleOrDefault(),
                                                            FuncStruct = db.FuncStruct.Where(e => e.Id == FuncStruct).SingleOrDefault(),
                                                            LeaveHead = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault(),
                                                            Narration = settlementemp == true ? "Settlement Process" : "Credit Process",
                                                            Reason = "Leave Bank Debit Request",
                                                            LvCreditNextDate = NextCreditDays,
                                                            WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault(),  //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
                                                            DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                                        };
                                                        _List_oLvNewReq.Add(newLvbankdebtreq);

                                                        // LvBankLedger OLvCreditRecordLvBankLedger = new LvBankLedger();
                                                        // var _LvBankLedger = db.LvBankLedger.Where(e => e.EmployeeLeave_Id == OEmpLvID).SingleOrDefault();
                                                        var calid = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                                                        // var LvBankOpenBal_data = db.LvBankOpenBal.Where(e => e.LvCalendar_Id == calid.Id).SingleOrDefault();

                                                        LvBankLedger _OLvCreditRecordLvBankLedger = new LvBankLedger()
                                                        {
                                                            OpeningBalance = _LvBankPolicy.LvDebitInCredit,
                                                            CreditDays = _LvBankPolicy.LvDebitInCredit,
                                                            ClosingBalance = _LvBankPolicy.LvDebitInCredit + _LvBankPolicy.LvDebitInCredit,
                                                            CreditDate = DateTime.Now,
                                                            DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                                            LvNewReq = newLvbankdebtreq,
                                                            Narration = "Credit Process",
                                                            EmployeeLeave_Id = OEmpLvID,

                                                        };

                                                        db.LvBankLedger.Add(_OLvCreditRecordLvBankLedger);
                                                        db.SaveChanges();


                                                        var companyLeave = new CompanyLeave();
                                                        companyLeave = db.CompanyLeave.Include(e => e.LvBankLedger).Where(e => e.Company.Id == OCompany.Id).SingleOrDefault();
                                                        if (companyLeave != null)
                                                        {
                                                            var LvBankLedger_list = new List<LvBankLedger>();
                                                            if (companyLeave.LvBankLedger.Count() > 0)
                                                            {
                                                                LvBankLedger_list.AddRange(companyLeave.LvBankLedger);
                                                            }

                                                            LvBankLedger_list.Add(_OLvCreditRecordLvBankLedger);
                                                            companyLeave.LvBankLedger = LvBankLedger_list;
                                                            db.CompanyLeave.Attach(companyLeave);
                                                            db.Entry(companyLeave).State = System.Data.Entity.EntityState.Modified;
                                                            db.SaveChanges();
                                                            db.Entry(companyLeave).State = System.Data.Entity.EntityState.Detached;
                                                        }




                                                    }


                                                }



                                            }
                                            //lvbank end

                                            if (_List_oLvNewReq.Count > 0)
                                            {
                                                if (_List_oLvNewReq.Count >= 2)
                                                {
                                                    _List_oLvNewReq[0].LvOrignal = OLvCreditRecord;
                                                    if (_List_oLvNewReq.Count == 3)
                                                    {
                                                        _List_oLvNewReq[1].LvOrignal = OLvCreditRecord;
                                                    }
                                                }
                                                var _Emp = db.EmployeeLeave.Include(e => e.LvNewReq)
                                                    .Where(e => e.Employee.Id == oEmployeeId).SingleOrDefault();
                                                for (int i = 0; i < _List_oLvNewReq.Count; i++)
                                                {
                                                    _Emp.LvNewReq.Add(_List_oLvNewReq[i]);
                                                }
                                                db.Entry(_Emp).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                }
                                // Check if leave Credited for that year then should not credit end


                            }
                        }
                        if (trial == 1)
                        {
                            ts.Complete();
                        }
                    }
                }
            }
            catch (Exception e)
            {

                //throw e;
                using (DataBaseContext db = new DataBaseContext())
                {
                    P2B.UTILS.P2BLogger logger = new P2B.UTILS.P2BLogger();
                    var EmpCode = db.Employee.Find(oEmployeeId).EmpCode;
                    string ErrorMsg = e.Message;
                    string ErrorInnerMsg = e.InnerException != null ? e.InnerException.Message : "";
                    string LineNo = Convert.ToString(new StackTrace(e, true).GetFrame(0).GetFileLineNumber());
                    logger.Logging("Leave Credit Trial Report " + "  " + "EmpCode :: " + EmpCode + " " + "ErrorMsg : " + ErrorMsg + " " + " ErrorInnerMsg : " + ErrorInnerMsg + " LineNo : " + LineNo);
                    string errorleavebankcreditprocess = ("Leave Credit Process " + "  " + "EmpCode :: " + EmpCode + " " + "ErrorMsg : " + ErrorMsg + " " + " ErrorInnerMsg : " + ErrorInnerMsg + " LineNo : " + LineNo);
                    LeaveCreditProcessMsg.Add(errorleavebankcreditprocess);
                    System.Web.HttpContext.Current.Session["LeaveCreditProcessMsg"] = LeaveCreditProcessMsg;


                }
            }

            return 0;
        }
        public class LvNewReqForReport
        {
            public string LeaveHead { get; set; }
            public string OpenBal { get; set; }
            public string CreditDays { get; set; }
            public string CloseBal { get; set; }
            public string LastLeave { get; set; }
            public string LvLapsed { get; set; }
            public string LvCount { get; set; }
            public string DebitDays { get; set; }

        }

        public ActionResult GetTrialData(string val, String forwardata)
        {
            var Employeelist = new List<string>();
            var Emplist = new List<string>();
            var Lvheadlist = new List<string>();
            var Empid = new List<string>();
            var Leaveid = (String)null;
            var Lvheadid = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var NewObj = new NameValueCollection();



                string Emp = forwardata == "0" ? "" : forwardata;


                if (Emp != null)
                {
                    Emplist = Utility.StringIdsToListString(Emp);
                    foreach (var item in Emplist)
                    {
                        Employeelist.Add(Convert.ToString(item));
                    }
                }

                //////////////////

                NewObj = HttpUtility.ParseQueryString(val);
                if (NewObj != null)
                {
                    Leaveid = NewObj["LvHead"];
                    if (Leaveid != null)
                    {
                        Lvheadid = Utility.StringIdsToListString(Leaveid);
                        foreach (var item in Lvheadid)
                        {
                            Lvheadlist.Add(Convert.ToString(item));
                        }
                    }

                }

                return Json(new Object[] { Employeelist, Lvheadlist, JsonRequestBehavior.AllowGet });
            }
        }


        public static List<LvNewReqForReport> LvCreditProceessForReport(Int32 oEmployeeId, Int32 CompLvId, CompanyLeave CompCreditPolicyLists, List<Int32> LvHead_ids_list, Int32 LvCalendarId, int compid, DateTime? FromDate, DateTime? ToDate, List<string> creditdatelist, Boolean settlementemp)
        {
            try
            {
                List<LvNewReqForReport> _List_oLvNewReqRpt = new List<LvNewReqForReport>();
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                                             new System.TimeSpan(0, 30, 0)))
                {
                    using (DataBaseContext db = new DataBaseContext())
                    {

                        //Employee Lv Data
                        foreach (var LvHead_ids in LvHead_ids_list)
                        {
                            double mSusDayFull = 0;
                            int lastyearid = 0;
                            var EmpLvHeadList = db.LvHead.Where(e => e.Id == LvHead_ids)
                                .Select(a => new { Id = a.Id, LvCode = a.LvCode }).Distinct().ToList();
                            Company OCompany = null;
                            OCompany = db.Company.Find(compid);
                            int crd = Convert.ToInt32(creditdatelist.FirstOrDefault());
                            string creditdtlist = db.LookupValue.Where(e => e.Id == crd).FirstOrDefault().LookupVal.ToUpper();


                            foreach (var item in EmpLvHeadList)
                            {
                                //prev credit process check
                                //end
                                //Get LvCredit Policy For Particular Lv
                                LvNewReq OLvCreditRecord = new LvNewReq();
                                LvNewReq newLvbankdebtreq = new LvNewReq();
                                LvNewReqForReport OLvCreditRecordForRpt = new LvNewReqForReport();

                                int OEmpLvID = db.EmployeeLeave.Where(e => e.Employee.Id == oEmployeeId).FirstOrDefault().Id;

                                //EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                                //      .Include(e => e.EmployeeLvStructDetails)
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvBankPolicy))
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvBankPolicy.LvHeadCollection))
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy))
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.CreditDate))
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ConvertLeaveHead))
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.LvHead))
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ConvertLeaveHeadBal))
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ExcludeLeaveHeads))
                                //      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead.LvHeadOprationType))
                                //          .Where(e => e.EndDate == null && e.EmployeeLeave_Id == OEmpLvID).SingleOrDefault();14022024

                                var OLvSalStruct = db.EmployeeLvStruct.Select(d => new
                                {
                                    OEndDate = d.EndDate,
                                    OEffectiveDate = d.EffectiveDate,
                                    OEmployeeLeaveId = d.EmployeeLeave_Id,
                                    OEmployeeLvStructDetails = d.EmployeeLvStructDetails.Select(r => new
                                    {
                                        OLvHeadFormula = r.LvHeadFormula,
                                        OEmployeeLvStructDetailsLvHeadId = r.LvHead_Id,
                                        OEmployeeLvStructDetailsLvHead = r.LvHead,
                                        OLvHeadOprationType = r.LvHead.LvHeadOprationType,
                                        OLvBankPolicy = r.LvHeadFormula.LvBankPolicy,
                                        OLvHeadCollection = r.LvHeadFormula.LvBankPolicy.LvHeadCollection,
                                        OLvCreditPolicy = r.LvHeadFormula.LvCreditPolicy,
                                        OCreditDate = r.LvHeadFormula.LvCreditPolicy.CreditDate,
                                        OConvertLeaveHead = r.LvHeadFormula.LvCreditPolicy.ConvertLeaveHead,
                                        OLvCreditPolicyLvHead = r.LvHeadFormula.LvCreditPolicy.LvHead,
                                        OConvertLeaveHeadBal = r.LvHeadFormula.LvCreditPolicy.ConvertLeaveHeadBal,
                                        OExcludeLeaveHeads = r.LvHeadFormula.LvCreditPolicy.ExcludeLeaveHeads

                                    }).ToList()
                                }).Where(e => e.OEndDate == null && e.OEmployeeLeaveId == OEmpLvID).SingleOrDefault();


                                LvCreditPolicy oLvCreditPolicy = null;
                                if (OLvSalStruct != null)
                                {
                                    oLvCreditPolicy = OLvSalStruct.OEmployeeLvStructDetails.Where(e => e.OEmployeeLvStructDetailsLvHeadId == item.Id && e.OLvHeadFormula != null && e.OLvCreditPolicy != null).Select(r => r.OLvCreditPolicy).FirstOrDefault();

                                }

                                //LvCreditPolicy oLvCreditPolicy = CompCreditPolicyLists.LvCreditPolicy.Where(e => e.CreditDate != null &&
                                //    e.LvHead != null && e.LvHead.Id == item.Id).SingleOrDefault();
                                if (oLvCreditPolicy == null)
                                {
                                    continue;
                                }
                                if (oLvCreditPolicy.CreditDate.LookupVal.ToUpper() != creditdtlist)
                                {
                                    continue;
                                }
                                // settlement Leave Process start
                                //EmployeeLeave _Prv_EmpLvDatasetemp = db.EmployeeLeave.Where(a => a.Employee.Id == oEmployeeId)
                                //    .Include(a => a.LvNewReq)
                                //    .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                                //    .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                                //    .Include(a => a.LvNewReq.Select(e => e.GeoStruct))
                                //    .Include(a => a.LvNewReq.Select(e => e.PayStruct))
                                //    .Include(a => a.LvNewReq.Select(e => e.FuncStruct))
                                //    .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                                //    .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.LeaveHead.Id == item.Id
                                //        ))
                                //        .SingleOrDefault();
                                var _Prv_EmpLvDatasetemp = db.LvNewReq.Where(a => a.LeaveHead != null && a.LeaveHead.Id == item.Id && a.EmployeeLeave_Id == OEmpLvID && a.Narration == "Settlement Process").FirstOrDefault();
                                // if settlement employee leave credit then leave should not credit
                                if (_Prv_EmpLvDatasetemp != null)
                                {
                                    //var LvCreditedInNewYrChksetemp = _Prv_EmpLvDatasetemp.LvNewReq
                                    //        .Where(a => a.Narration == "Settlement Process"
                                    //        && a.LeaveHead.Id == item.Id).FirstOrDefault();
                                    continue;

                                }
                                // settlement Leave Process End

                                DateTime? Lastyear = null, CreditDate = null, tempCreditDate = null, Cal_Wise_Date = null, Lastyearj = null;
                                Double CreditDays = 0, SumDays = 0, oOpenBal = 0, oCloseingbal = 0, LWPDays = 0;
                                Calendar LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                    e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                                DateTime? tempRetireDate = null, tempRetirecrDate = null, leaveyearfrom = null, leaveyearto = null;

                                switch (oLvCreditPolicy.CreditDate.LookupVal.ToUpper())
                                {
                                    case "CALENDAR":
                                        Cal_Wise_Date = LvCalendar.FromDate;
                                        CreditDate = LvCalendar.FromDate.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearid1 = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyear
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearid1 != null)
                                        {

                                            lastyearid = lastyearid1.Id;
                                        }

                                        break;
                                    case "YEARLY":
                                        Cal_Wise_Date = LvCalendar.FromDate;
                                        CreditDate = LvCalendar.FromDate.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearid11 = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyear
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearid11 != null)
                                        {

                                            lastyearid = lastyearid11.Id;
                                        }

                                        break;
                                    case "JOININGDATE":
                                        //var JoiningDate = db.Employee.Include(e => e.ServiceBookDates)
                                        //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        //tempCreditDate = JoiningDate.ServiceBookDates.JoiningDate;


                                        var LvNewReqData = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aa = null;
                                        DateTime? joinincr = null;
                                        var Fulldate = "";
                                        if (LvNewReqData != null)
                                        {
                                            aa = LvNewReqData.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aa != null)
                                        {
                                            Fulldate = aa.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.JoiningDate != null && q.Employee.Id == oEmployeeId).SingleOrDefault();

                                            if (ServiceBookData != null)
                                            {
                                                if (ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month <= ToDate.Value.Month)
                                                {

                                                    Fulldate = ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                }
                                            }
                                        }
                                        joinincr = Convert.ToDateTime(Fulldate);
                                        CreditDate = joinincr.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidj = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidj != null)
                                        {
                                            lastyearid = lastyearidj.Id;
                                        }
                                        Cal_Wise_Date = joinincr;
                                        break;
                                    case "CONFIRMATIONDATE":
                                        //var ConfirmationDate = db.Employee.Include(e => e.ServiceBookDates)
                                        //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        //tempCreditDate = ConfirmationDate.ServiceBookDates.ConfirmationDate;

                                        var LvNewReqDatac = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aac = null;
                                        DateTime? joinincrc = null;
                                        var Fulldatec = "";
                                        if (LvNewReqDatac != null)
                                        {
                                            aac = LvNewReqDatac.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aac != null)
                                        {
                                            Fulldatec = aac.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.ConfirmationDate != null && q.Employee.Id == oEmployeeId).SingleOrDefault();

                                            if (ServiceBookData != null)
                                            {
                                                if (ServiceBookData.Employee.ServiceBookDates.ConfirmationDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.ConfirmationDate.Value.Month <= ToDate.Value.Month)
                                                {

                                                    Fulldatec = ServiceBookData.Employee.ServiceBookDates.ConfirmationDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                }
                                            }
                                        }
                                        joinincrc = Convert.ToDateTime(Fulldatec);
                                        CreditDate = joinincrc.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincrc.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidc = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidc != null)
                                        {
                                            lastyearid = lastyearidc.Id;
                                        }
                                        Cal_Wise_Date = joinincrc;



                                        break;
                                    case "INCREMENTDATE":
                                        //var LastIncrementDate = db.Employee.Include(e => e.ServiceBookDates)
                                        //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        //tempCreditDate = LastIncrementDate.ServiceBookDates.LastIncrementDate;
                                        DateTime? joinincrI = null;
                                        var FulldateI = "";
                                        var Emp_lvcr = db.EmployeePayroll
                                            .Include(a => a.Employee)
                                            .Include(e => e.IncrementServiceBook)
                                            .Include(e => e.IncrementServiceBook.Select(q => q.IncrActivity))
                                            .Where(q => q.Employee.Id == oEmployeeId).SingleOrDefault();
                                        if (Emp_lvcr != null)
                                        {
                                            var fgds = Emp_lvcr.IncrementServiceBook.Where(q => q.ReleaseDate != null
                                                 && q.IncrActivity.Id == 1).OrderBy(q => q.Id).LastOrDefault();
                                            if (fgds != null)
                                            {
                                                if (fgds.ReleaseDate.Value.Month == FromDate.Value.Month)
                                                {
                                                    FulldateI = fgds.ReleaseDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                }
                                            }
                                            else
                                            {
                                                var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.LastIncrementDate == null && q.Employee.Id == oEmployeeId).SingleOrDefault();

                                                if (ServiceBookData != null)
                                                {
                                                    if (ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month <= ToDate.Value.Month)
                                                    {

                                                        FulldateI = ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                    }
                                                }
                                            }

                                        }

                                        joinincrI = Convert.ToDateTime(FulldateI);
                                        CreditDate = joinincrI.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincrI.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidI = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidI != null)
                                        {
                                            lastyearid = lastyearidI.Id;
                                        }
                                        Cal_Wise_Date = joinincrI;

                                        break;
                                    case "PROMOTIONDATE":
                                        //var LastPromotionDate = db.Employee.Include(e => e.ServiceBookDates)
                                        //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        //tempCreditDate = LastPromotionDate.ServiceBookDates.LastPromotionDate;

                                        DateTime? joinincrP = null;
                                        var FulldateP = "";
                                        var Emp_lvcrP = db.EmployeePayroll
                                            .Include(a => a.Employee)
                                            .Include(e => e.PromotionServiceBook)
                                            .Include(e => e.PromotionServiceBook.Select(q => q.PromotionActivity))
                                            .Where(q => q.Employee.Id == oEmployeeId).SingleOrDefault();
                                        if (Emp_lvcrP != null)
                                        {
                                            var fgds = Emp_lvcrP.PromotionServiceBook.Where(q => q.ReleaseDate != null
                                                 && q.PromotionActivity.Id == 2).OrderBy(q => q.Id).LastOrDefault();
                                            if (fgds != null)
                                            {
                                                if (fgds.ReleaseDate.Value.Month == FromDate.Value.Month)
                                                {
                                                    FulldateP = fgds.ReleaseDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                }
                                            }
                                            else
                                            {
                                                var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.LastPromotionDate == null && q.Employee.Id == oEmployeeId).SingleOrDefault();

                                                if (ServiceBookData != null)
                                                {
                                                    if (ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month <= ToDate.Value.Month)
                                                    {

                                                        FulldateP = ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
                                                    }
                                                }
                                            }

                                        }

                                        joinincrP = Convert.ToDateTime(FulldateP);
                                        CreditDate = joinincrP.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincrP.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidP = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidP != null)
                                        {
                                            lastyearid = lastyearidP.Id;
                                        }
                                        Cal_Wise_Date = joinincrP;

                                        break;
                                    case "FIXDAYS":
                                        if (OCompany.Code.ToString() == "ACABL")
                                        {
                                            DateTime? Fixdays = null;
                                            Fixdays = Convert.ToDateTime("01/01/" + DateTime.Now.Year);

                                            Cal_Wise_Date = Fixdays;
                                            CreditDate = Fixdays.Value.AddDays(-1);
                                            Lastyear = Convert.ToDateTime(Fixdays.Value.AddYears(-1));

                                        }
                                        break;
                                    case "QUARTERLY":
                                        var LvNewReqDataQUARTERLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aaQUARTERLY = null;
                                        var FulldateQUARTERLY = "";
                                        if (LvNewReqDataQUARTERLY != null)
                                        {
                                            aaQUARTERLY = LvNewReqDataQUARTERLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aaQUARTERLY != null)
                                        {
                                            FulldateQUARTERLY = aaQUARTERLY.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            FulldateQUARTERLY = OLvSalStruct.OEffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

                                        }
                                        joinincr = Convert.ToDateTime(FulldateQUARTERLY);
                                        CreditDate = joinincr.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidjQUARTERLY = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidjQUARTERLY != null)
                                        {
                                            lastyearid = lastyearidjQUARTERLY.Id;
                                        }
                                        Cal_Wise_Date = joinincr;

                                        break;
                                    case "HALFYEARLY":
                                        var LvNewReqDataHALFYEARLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aaHALFYEARLY = null;
                                        var FulldateHALFYEARLY = "";
                                        if (LvNewReqDataHALFYEARLY != null)
                                        {
                                            aaHALFYEARLY = LvNewReqDataHALFYEARLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aaHALFYEARLY != null)
                                        {
                                            FulldateHALFYEARLY = aaHALFYEARLY.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            FulldateHALFYEARLY = OLvSalStruct.OEffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

                                        }
                                        joinincr = Convert.ToDateTime(FulldateHALFYEARLY);
                                        CreditDate = joinincr.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidjHALFYEARLY = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidjHALFYEARLY != null)
                                        {
                                            lastyearid = lastyearidjHALFYEARLY.Id;
                                        }
                                        Cal_Wise_Date = joinincr;
                                        break;
                                    case "MONTHLY":
                                        var LvNewReqDataMONTHLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aaMONTHLY = null;
                                        var FulldateMONTHLY = "";
                                        if (LvNewReqDataMONTHLY != null)
                                        {
                                            aaMONTHLY = LvNewReqDataMONTHLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aaMONTHLY != null)
                                        {
                                            FulldateMONTHLY = aaMONTHLY.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            FulldateMONTHLY = OLvSalStruct.OEffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

                                        }
                                        joinincr = Convert.ToDateTime(FulldateMONTHLY);
                                        CreditDate = joinincr.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincr.Value.AddMonths(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidjMONTHLY = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidjMONTHLY != null)
                                        {
                                            lastyearid = lastyearidjMONTHLY.Id;
                                        }
                                        Cal_Wise_Date = joinincr;
                                        break;
                                    case "BIMONTHLY":
                                        var LvNewReqDataBIMONTHLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                        LvNewReq aaBIMONTHLY = null;
                                        var FulldateBIMONTHLY = "";
                                        if (LvNewReqDataBIMONTHLY != null)
                                        {
                                            aaBIMONTHLY = LvNewReqDataBIMONTHLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                        }

                                        if (aaBIMONTHLY != null)
                                        {
                                            FulldateBIMONTHLY = aaBIMONTHLY.LvCreditNextDate.Value.ToShortDateString();
                                        }
                                        else
                                        {
                                            FulldateBIMONTHLY = OLvSalStruct.OEffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

                                        }
                                        joinincr = Convert.ToDateTime(FulldateBIMONTHLY);
                                        CreditDate = joinincr.Value.AddDays(-1);
                                        Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
                                        Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                        Calendar lastyearidjBIMONTHLY = db.Calendar.Include(e => e.Name).Where(e =>
                                            e.FromDate == Lastyearj
                                            && e.Default == false
                                            && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                        if (lastyearidjBIMONTHLY != null)
                                        {
                                            lastyearid = lastyearidjBIMONTHLY.Id;
                                        }
                                        Cal_Wise_Date = joinincr;
                                        break;
                                    case "OTHER":
                                        //var EmpService = db.Employee.Include(e => e.ServiceBookDates)
                                        //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        //tempCreditDate = EmpService.ServiceBookDates.JoiningDate;
                                        break;
                                    default:
                                        break;
                                }

                                // settlement Leave Process start
                                double retmonthResignRetire = 0;
                                Double oldcreditlv = 0;
                                Double oldcreditlvclose = 0;
                                if (settlementemp == true)
                                {


                                    var LvNewReqDatasettle = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
                                    LvNewReq aaSettle = null;
                                    DateTime? Settletilldate = null;
                                    var Onworkingcreditednextcreditdate = "";
                                    var FulldateSettleStartdate = "";

                                    if (LvNewReqDatasettle != null)
                                    {
                                        aaSettle = LvNewReqDatasettle.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
                                    }
                                    if (aaSettle != null)
                                    {
                                        Onworkingcreditednextcreditdate = aaSettle.LvCreditNextDate.Value.ToShortDateString();
                                        FulldateSettleStartdate = aaSettle.LvCreditDate.Value.ToShortDateString();
                                        oldcreditlv = aaSettle.CreditDays;
                                        oldcreditlvclose = aaSettle.CloseBal;
                                    }

                                    // Resign,EXPIRED,TERMINATION
                                    var OOtherServiceBook = db.EmployeePayroll.Where(e => e.Employee.Id == oEmployeeId)
                                    .Include(e => e.OtherServiceBook)

                                     .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity))
                                     .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity.OtherSerBookActList))
                                   .SingleOrDefault();
                                    var OOOtherServiceBook = OOtherServiceBook.OtherServiceBook.Where(e => e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "RESIGNED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "EXPIRED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "TERMINATION").Select(x => x.OthServiceBookActivity.OtherSerBookActList)
                                                             .FirstOrDefault();
                                    if (OOOtherServiceBook != null)
                                    {
                                        string check = OOOtherServiceBook.LookupVal.ToUpper().ToString();
                                        if (check != null)
                                        {
                                            if (check == "RESIGNED" || check == "EXPIRED" || check == "TERMINATION")
                                            {
                                                var OOOtherServiceBookdate = OOtherServiceBook.OtherServiceBook.Where(e => e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "RESIGNED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "EXPIRED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "TERMINATION")
                                                            .FirstOrDefault();

                                                Settletilldate = Convert.ToDateTime(OOOtherServiceBookdate.ProcessOthDate);// Resign employee leave credit on request date of resignation
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var OOtherServiceBookret = db.Employee.Where(e => e.Id == oEmployeeId)
                                                                            .Include(e => e.ServiceBookDates)
                                                                           .SingleOrDefault();
                                        if (OOtherServiceBookret != null)
                                        {
                                            Settletilldate = Convert.ToDateTime(OOtherServiceBookret.ServiceBookDates.RetirementDate);
                                        }

                                    }

                                    CreditDate = Convert.ToDateTime(Settletilldate); //31/12/2020   
                                    Lastyear = Convert.ToDateTime(FulldateSettleStartdate); //01/12/2020
                                    Cal_Wise_Date = CreditDate;
                                    // settle emp retire/resign month start

                                    if (Lastyear.Value.Day >= 15)
                                    {
                                        // retmonth=
                                        int compMonthResignRetire = (Lastyear.Value.Month + Lastyear.Value.Year * 12) - (Settletilldate.Value.Month + Settletilldate.Value.Year * 12);

                                        retmonthResignRetire = compMonthResignRetire + 1;

                                    }
                                    else
                                    {
                                        int compMonthResignRetire = (Lastyear.Value.Month + Lastyear.Value.Year * 12) - (Settletilldate.Value.Month + Settletilldate.Value.Year * 12);

                                        retmonthResignRetire = compMonthResignRetire;
                                    }
                                    if (retmonthResignRetire < 0)
                                    {
                                        retmonthResignRetire = 0;
                                    }
                                    retmonthResignRetire = Math.Round(retmonthResignRetire, 0);

                                    // settle emp retire/resign month end
                                }
                                // settlement Leave Process end


                                if (CreditDate == null && Lastyear == null)
                                {
                                    CreditDate = Convert.ToDateTime(tempCreditDate.Value.Day + "/" + tempCreditDate.Value.Month + "/" + DateTime.Now.Year);
                                    leaveyearfrom = CreditDate;
                                    leaveyearto = leaveyearfrom.Value.AddDays(-1);
                                    CreditDate = CreditDate.Value.AddDays(-1);
                                    Lastyear = CreditDate.Value.AddYears(-1);
                                }


                                leaveyearfrom = LvCalendar.FromDate;
                                leaveyearto = LvCalendar.ToDate;
                                double retmonth = 0;
                                var retiredate = db.Employee.Include(e => e.ServiceBookDates)
                                            .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                tempRetireDate = retiredate.ServiceBookDates.RetirementDate;

                                if (tempRetireDate != null)
                                {
                                    if (leaveyearto > tempRetireDate)
                                    {
                                        tempRetirecrDate = tempRetireDate;
                                    }
                                    else
                                    {
                                        tempRetirecrDate = leaveyearto;
                                    }
                                }
                                else
                                {
                                    tempRetirecrDate = leaveyearto;
                                }
                                if (tempRetirecrDate.Value.Day >= 15)
                                {
                                    // retmonth=
                                    int compMonth = (tempRetirecrDate.Value.Month + tempRetirecrDate.Value.Year * 12) - (leaveyearfrom.Value.Month + leaveyearfrom.Value.Year * 12);
                                    //double daysInEndMonth = (tempRetirecrDate - tempRetirecrDate.Value.AddMonths(1)).Value.Days;
                                    //retmonth = compMonth + (leaveyearfrom.Value.Day - tempRetirecrDate.Value.Day) / daysInEndMonth;
                                    retmonth = compMonth + 1;

                                }
                                else
                                {
                                    int compMonth = (tempRetirecrDate.Value.Month + tempRetirecrDate.Value.Year * 12) - (leaveyearfrom.Value.Month + leaveyearfrom.Value.Year * 12);
                                    //double daysInEndMonth = (tempRetirecrDate - tempRetirecrDate.Value.AddMonths(1)).Value.Days;
                                    //retmonth = compMonth + (leaveyearfrom.Value.Day - tempRetirecrDate.Value.Day) / daysInEndMonth;
                                    retmonth = compMonth;
                                }
                                if (retmonth < 0)
                                {
                                    retmonth = 0;
                                }
                                retmonth = Math.Round(retmonth, 0);

                                int compMonthLYR = (leaveyearto.Value.Month + leaveyearto.Value.Year * 12) - (leaveyearfrom.Value.Month + leaveyearfrom.Value.Year * 12);
                                double daysInEndMonthLYR = (leaveyearto - leaveyearto.Value.AddMonths(1)).Value.Days;
                                double LYRmonth = compMonthLYR + (leaveyearfrom.Value.Day - leaveyearto.Value.Day) / daysInEndMonthLYR;
                                LYRmonth = Math.Round(LYRmonth, 0);

                                //   DateTime NextCreditDays = Cal_Wise_Date.Value.AddYears(1); //comment line because credit frequeny avilable in system if yearly then 12 half yearly then 6 if monthyly then 1 frequency define
                                DateTime NextCreditDays = Cal_Wise_Date.Value.AddMonths(oLvCreditPolicy.ProCreditFrequency);

                                EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == oEmployeeId)
                                    .Include(a => a.LvNewReq)
                                    .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                                    .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                                    .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                                    .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.LeaveHead.Id == item.Id
                                        //  && a.LeaveCalendar.FromDate == Lastyear && a.LeaveCalendar.ToDate == CreditDate
                                        ))
                                        .SingleOrDefault();

                                List<LvNewReq> Filter_oEmpLvData = new List<LvNewReq>();
                                // Check if leave Credited for that year then should not credit start
                                if (_Prv_EmpLvData != null)
                                {
                                    var LvCreditedInNewYrChk1 = _Prv_EmpLvData.LvNewReq
                                            .Where(a => a.LvCreditNextDate != null && a.LvOrignal_Id == null && a.LvCreditNextDate.Value.Date == NextCreditDays.Date
                                            && a.LeaveHead.Id == item.Id).FirstOrDefault();
                                    if (LvCreditedInNewYrChk1 != null)
                                    {
                                        continue;
                                    }
                                }
                                // Check if leave Credited for that year then should not credit end
                                if (_Prv_EmpLvData != null)
                                {
                                    var LvCreditedInNewYrChk = _Prv_EmpLvData.LvNewReq
                                        .Where(a => a.LeaveCalendar.FromDate == LvCalendar.FromDate
                                        && a.LeaveHead.Id == item.Id).ToList();
                                    if (LvCreditedInNewYrChk.Count() == 0)
                                    {
                                        Filter_oEmpLvData = _Prv_EmpLvData.LvNewReq
                                            .Where(a => a.LeaveHead != null && a.LeaveHead.Id == item.Id)
                                            //      && a.ReqDate >= Lastyear &&
                                            //a.ReqDate <= CreditDate)
                                          .ToList();
                                    }
                                    else
                                    {
                                        Filter_oEmpLvData.AddRange(LvCreditedInNewYrChk);
                                    }
                                }
                                double oLvClosingData = 0;
                                double UtilizedLv = 0;
                                if (Filter_oEmpLvData.Count == 0)
                                {
                                    //get Data from opening
                                    EmployeeLeave _Emp_EmpOpeningData = db.EmployeeLeave.Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == item.Id))
                                        .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                        .Include(e => e.Employee.GeoStruct)
                                        .Include(e => e.Employee.PayStruct)
                                        .Include(e => e.Employee.FuncStruct)
                                        .SingleOrDefault();

                                    double _EmpOpeningData = 0;
                                    if (_Emp_EmpOpeningData != null)
                                    {
                                        _EmpOpeningData = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == item.Id).Select(e => e.LvOpening).SingleOrDefault();

                                    }
                                    if (_EmpOpeningData == 0)
                                    {
                                        // continue;
                                    }
                                    oLvClosingData = _EmpOpeningData;
                                    UtilizedLv = 0;
                                }
                                else
                                {
                                    var LastLvData = Filter_oEmpLvData.OrderByDescending(a => a.Id).FirstOrDefault();

                                    oLvClosingData = LastLvData.CloseBal;
                                    UtilizedLv = LastLvData.LVCount;
                                }

                                if (oLvCreditPolicy.ProdataFlag == true)
                                {

                                    var AttendaceData = db.EmployeePayroll.Include(e => e.SalAttendance)
                                        .Where(e => e.Employee.Id == oEmployeeId
                                        //&&
                                        //e.SalAttendance.Any(a => Convert.ToDateTime("01/" + a.PayMonth) >= Lastyear &&
                                        //Convert.ToDateTime("01/" + a.PayMonth) <= CreditDate)
                                        ).FirstOrDefault();

                                    if (AttendaceData != null)
                                    {
                                        Boolean newjoinconfermationdate = false;
                                        var confermationdate = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.ConfirmationDate != null && q.Employee.Id == oEmployeeId).SingleOrDefault();
                                        double daysdiff = 0;
                                        if (confermationdate != null)
                                        {
                                            if (confermationdate.Employee.ServiceBookDates.ConfirmationDate > Lastyear)
                                            {
                                                string Confirmmonthyear = confermationdate.Employee.ServiceBookDates.ConfirmationDate.Value.ToString("MM/yyyy");
                                                DateTime monthend = Convert.ToDateTime("01/" + Confirmmonthyear).AddMonths(1).AddDays(-1);
                                                daysdiff = (monthend.Date - confermationdate.Employee.ServiceBookDates.ConfirmationDate.Value.Date).Days + 1;
                                                Lastyear = confermationdate.Employee.ServiceBookDates.ConfirmationDate.Value.Date;
                                                newjoinconfermationdate = true;
                                            }
                                        }
                                        if (newjoinconfermationdate == false)
                                        {
                                            SumDays = AttendaceData.SalAttendance.Where(a => Convert.ToDateTime("01/" + a.PayMonth) >= Lastyear &&
                                   Convert.ToDateTime("01/" + a.PayMonth) <= CreditDate && a.PaybleDays != 0).Select(a => a.PaybleDays).ToList().Sum();
                                            //LWP Leave process button on manual attendance page Goa urban bank
                                            LWPDays = AttendaceData.SalAttendance.Where(a => Convert.ToDateTime("01/" + a.PayMonth) >= Lastyear &&
                                       Convert.ToDateTime("01/" + a.PayMonth) <= CreditDate && a.LWPDays != 0).Select(a => a.LWPDays).ToList().Sum();

                                        }
                                        else
                                        {
                                            SumDays = AttendaceData.SalAttendance.Where(a => Convert.ToDateTime("01/" + a.PayMonth) > Lastyear &&
                                  Convert.ToDateTime("01/" + a.PayMonth) <= CreditDate && a.PaybleDays != 0).Select(a => a.PaybleDays).ToList().Sum();
                                            //LWP Leave process button on manual attendance page Goa urban bank
                                            LWPDays = AttendaceData.SalAttendance.Where(a => Convert.ToDateTime("01/" + a.PayMonth) > Lastyear &&
                                       Convert.ToDateTime("01/" + a.PayMonth) <= CreditDate && a.LWPDays != 0).Select(a => a.LWPDays).ToList().Sum();

                                        }

                                        SumDays = (SumDays + daysdiff) - LWPDays;
                                    }

                                    ////suspended days check

                                    EmployeePayroll othser = db.EmployeePayroll.Where(e => e.Employee.Id == oEmployeeId)
                                                            .Include(e => e.OtherServiceBook)
                                                            .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity)).AsNoTracking().OrderBy(e => e.Id)
                                                            .SingleOrDefault();

                                    if (othser.OtherServiceBook != null && othser.OtherServiceBook.Count() > 0)
                                    {
                                        List<OtherServiceBook> OthServBkSus = othser.OtherServiceBook.Where(e => e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED" || e.OthServiceBookActivity.Name.ToUpper() == "REJOIN").OrderByDescending(e => e.ReleaseDate).ToList();
                                        if (OthServBkSus.Count() > 0)
                                        {
                                            var checkSuspenddays = OthServBkSus.Where(e => e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED").Select(e => e.ReleaseDate).SingleOrDefault();
                                            var checkRejoindays_temp = OthServBkSus.Where(e => e.OthServiceBookActivity.Name.ToUpper() == "REJOIN").Select(e => e.ReleaseDate).SingleOrDefault();
                                            var checkRejoindays = "";
                                            if (checkRejoindays_temp != null)
                                            {
                                                checkRejoindays = checkRejoindays_temp.ToString();
                                            }
                                            else
                                            {
                                                checkRejoindays = CreditDate.ToString();
                                            }
                                            if (checkSuspenddays != null && checkRejoindays != null)
                                            {
                                                if (Convert.ToDateTime(checkSuspenddays).Date < Lastyear)
                                                {
                                                    checkSuspenddays = Lastyear;
                                                }
                                                if (Convert.ToDateTime(checkRejoindays).Date >= Lastyear && Convert.ToDateTime(checkRejoindays).Date <= CreditDate)
                                                {

                                                    mSusDayFull = Math.Round((Convert.ToDateTime(checkRejoindays).Date - Convert.ToDateTime(checkSuspenddays).Date).TotalDays) + 1;
                                                    SumDays = SumDays - mSusDayFull;
                                                }
                                                if (SumDays < 0)
                                                {
                                                    SumDays = 0;
                                                }
                                            }
                                        }
                                    }

                                    var OSalArrT = db.EmployeePayroll
                                        .Include(e => e.SalaryArrearT)
                                        .Include(e => e.SalaryArrearT.Select(q => q.ArrearType))
                                        .Where(e => e.Employee.Id == oEmployeeId)
                                     .FirstOrDefault();
                                    if (OSalArrT != null)
                                    {
                                        double ArrDays = OSalArrT.SalaryArrearT.Where(q => q.ArrearType.LookupVal.ToUpper() == "LWP"
                                              && q.FromDate >= Lastyear
                                              && q.FromDate <= CreditDate && q.IsRecovery == false).Select(q => q.TotalDays).Sum();
                                        SumDays = SumDays + ArrDays;

                                        double ArrDaysrec = OSalArrT.SalaryArrearT.Where(q => q.ArrearType.LookupVal.ToUpper() == "LWP"
                                        && q.FromDate >= Lastyear
                                        && q.FromDate <= CreditDate && q.IsRecovery == true).Select(q => q.TotalDays).Sum();
                                        SumDays = SumDays - ArrDaysrec;

                                        if (SumDays < 0)
                                        {
                                            SumDays = 0;
                                        }
                                    }

                                }


                                if (oLvCreditPolicy.ExcludeLeaves == true)
                                {
                                    List<LvHead> GetExculdeLvHeads = oLvCreditPolicy.ExcludeLeaveHeads.ToList();
                                    foreach (var GetExculdeLvHead in GetExculdeLvHeads)
                                    {

                                        var _Prv_EmpLvData_exclude = db.EmployeeLeave.AsNoTracking()
                                            .Where(a => a.Employee.Id == oEmployeeId)
                                            .Include(a => a.LvNewReq)
                                            .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                                            .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                                             .Include(a => a.LvNewReq.Select(e => e.LvOrignal))
                                            .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                                            .SingleOrDefault();

                                        var _Prv_EmpLvData_exclude1 = _Prv_EmpLvData_exclude.LvNewReq
                                            .Where(a => a.LeaveHead != null
                                                && a.LeaveHead.Id == GetExculdeLvHead.Id
                                                // && a.LeaveCalendar.Id == lastyearid && a.IsCancel == false && a.WFStatus.LookupVal != "2"
                                                && a.IsCancel == false && a.WFStatus.LookupVal != "2"

                                        ).ToList();

                                        var LvOrignal_id = _Prv_EmpLvData_exclude.LvNewReq.Where(e => e.LvOrignal != null && e.WFStatus.LookupVal != "2").Select(e => e.LvOrignal.Id).ToList();
                                        var listLvs = _Prv_EmpLvData_exclude1.Where(e => !LvOrignal_id.Contains(e.Id) && e.FromDate != null && e.ToDate != null).OrderBy(e => e.Id).ToList();
                                        double DebitSum = 0;
                                        if (listLvs != null)
                                        {
                                            for (DateTime _Date = Lastyear.Value; _Date <= CreditDate; _Date = _Date.AddDays(1))
                                            {
                                                var xyz = listLvs.Where(q => _Date >= q.FromDate && _Date <= q.ToDate).FirstOrDefault();
                                                if (xyz != null)
                                                {
                                                    DebitSum = DebitSum + 1;
                                                }
                                            }
                                            //  double DebitSum = listLvs.Sum(e => e.DebitDays);
                                            SumDays = SumDays - DebitSum;
                                        }
                                        else
                                        {
                                            EmployeeLeave _Emp_EmpOpeningData_exclude = db.EmployeeLeave
                                                .Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == GetExculdeLvHead.Id))
                                                .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                                .SingleOrDefault();

                                            double _EmpOpeningData_exclude = 0;
                                            if (_Emp_EmpOpeningData_exclude != null)
                                            {
                                                _EmpOpeningData_exclude = _Emp_EmpOpeningData_exclude.LvOpenBal.Where(e => e.LvHead.Id == GetExculdeLvHead.Id).Select(e => e.LVCount).SingleOrDefault();
                                            }
                                            SumDays = SumDays - _EmpOpeningData_exclude;

                                        }
                                        //double CloseBalSum = Filter_oEmpLvData.Where(e => e.LeaveHead.Id == GetExculdeLvHead.Id).Select(e => e.CloseBal).ToList().Sum();
                                        //SumDays = SumDays - CloseBalSum;
                                    }
                                    if (SumDays < 0)
                                    {
                                        SumDays = 0;
                                    }
                                }

                                //if (SumDays == 0)
                                //{
                                //    return 0;
                                //}

                                List<LvNewReq> _List_oLvNewReq = new List<LvNewReq>();
                                if (oLvCreditPolicy.LVConvert == true)
                                {
                                    //if (oLvClosingData > 0)
                                    //{
                                    double LastMonthBal = 0, _LvLapsed = 0, Prv_bal = 0;
                                    LvHead ConvertLeaveHead = oLvCreditPolicy.ConvertLeaveHead;
                                    //Check Exitance
                                    List<LvNewReq> Filter_oEmpLvDataCon = new List<LvNewReq>();
                                    if (_Prv_EmpLvData != null)
                                    {
                                        //LvNewReq DefaultyearLvHeadCreditedCHeck = _Prv_EmpLvData.LvNewReq
                                        //      .Where(a => a.LeaveHead != null
                                        //          && a.LeaveHead.Id == ConvertLeaveHead.Id
                                        //          && a.ReqDate >= LvCalendar.FromDate).SingleOrDefault();

                                        LvNewReq DefaultyearLvHeadCreditedCHeck = _Prv_EmpLvData.LvNewReq
                                          .Where(a => a.LvCreditNextDate != null && a.LvOrignal_Id != null && a.LvCreditNextDate.Value.Date == NextCreditDays.Date
                                          && a.LeaveHead.Id == ConvertLeaveHead.Id).SingleOrDefault();



                                        if (DefaultyearLvHeadCreditedCHeck == null)
                                        {
                                            Filter_oEmpLvDataCon = _Prv_EmpLvData.LvNewReq
                                                .Where(a => a.LeaveHead != null && a.LeaveHead.Id == ConvertLeaveHead.Id
                                                //      && a.ReqDate >= Lastyear &&
                                                //a.ReqDate <= CreditDate
                                              ).ToList();
                                        }
                                        else
                                        {
                                            Filter_oEmpLvDataCon.Add(DefaultyearLvHeadCreditedCHeck);
                                        }
                                    }
                                    var _LvNewReq_Prv_bal = Filter_oEmpLvDataCon.Count() > 0 ? Filter_oEmpLvDataCon.Where(e => e.LeaveHead.Id == ConvertLeaveHead.Id)
                                        .OrderByDescending(e => e.Id).Select(e => e.CloseBal).FirstOrDefault() : 0;
                                    if (_LvNewReq_Prv_bal == 0)
                                    {
                                        EmployeeLeave _Emp_LvOpenBal = db.EmployeeLeave
                                            .Include(e => e.LvOpenBal)
                                            .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                            .Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == ConvertLeaveHead.Id)).FirstOrDefault();
                                        if (_Emp_LvOpenBal != null)
                                        {
                                            Prv_bal = _Emp_LvOpenBal.LvOpenBal.Where(a => a.LvHead.Id == ConvertLeaveHead.Id).Select(e => e.LvOpening).LastOrDefault();
                                        }
                                        else
                                        {
                                            Prv_bal = _LvNewReq_Prv_bal;
                                        }
                                    }
                                    else
                                    {
                                        Prv_bal = _LvNewReq_Prv_bal;
                                    }


                                    double conevrtedlv = 0;
                                    double afterconvrtedremainlv = 0;
                                    if (oLvClosingData > oLvCreditPolicy.LvConvertLimit)
                                    {
                                        conevrtedlv = oLvCreditPolicy.LvConvertLimit;
                                        afterconvrtedremainlv = oLvClosingData - conevrtedlv;
                                    }
                                    else
                                    {
                                        conevrtedlv = oLvClosingData;
                                    }

                                    //  LastMonthBal = Prv_bal + oLvClosingData;
                                    LastMonthBal = Prv_bal + conevrtedlv;
                                    //-------------------------------------

                                    LvNewReq newLvConvertobj = new LvNewReq
                                    {
                                        InputMethod = 0,
                                        ReqDate = DateTime.Now,
                                        CloseBal = LastMonthBal,
                                        OpenBal = Prv_bal,
                                        LvOccurances = 0,
                                        IsLock = true,
                                        LvLapsed = _LvLapsed,
                                        //CreditDays = oLvClosingData,
                                        CreditDays = conevrtedlv,
                                        //ToDate = CreditDate,
                                        //FromDate = CreditDate,
                                        // LvCreditDate = Cal_Wise_Date,
                                        LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                            e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
                                        LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHead.Id).SingleOrDefault(),
                                        Narration = "Credit Process",
                                        //  LvCreditNextDate = NextCreditDays,
                                        Reason = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault().LvCode + " Converted",
                                        WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault(),  //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
                                        DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                    };

                                    _List_oLvNewReq.Add(newLvConvertobj);

                                    LvNewReqForReport newLvConvertobjRpt = new LvNewReqForReport
                                    {
                                        // ReqDate = DateTime.Now,
                                        CloseBal = Convert.ToString(LastMonthBal),
                                        LvLapsed = Convert.ToString(_LvLapsed),
                                        CreditDays = Convert.ToString(conevrtedlv),
                                        OpenBal = Convert.ToString(Prv_bal),
                                        LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHead.Id).SingleOrDefault().LvName,
                                        // WFStatus = db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
                                    };
                                    _List_oLvNewReqRpt.Add(newLvConvertobjRpt);

                                    // after conversion remaining leave conevrt another leave
                                    if (oLvCreditPolicy.LVConvertBal == true)
                                    {
                                        if (afterconvrtedremainlv > 0)
                                        {
                                            // 20/01/2020 start
                                            double LastMonthBalrm = 0, _LvLapsedrm = 0, Prv_balrm = 0;
                                            LvHead ConvertLeaveHeadrm = oLvCreditPolicy.ConvertLeaveHeadBal;
                                            //Check Exitance
                                            List<LvNewReq> Filter_oEmpLvDataConrm = new List<LvNewReq>();
                                            if (_Prv_EmpLvData != null)
                                            {
                                                LvNewReq DefaultyearLvHeadCreditedCHeck = _Prv_EmpLvData.LvNewReq
                                                      .Where(a => a.LeaveHead != null
                                                          && a.LeaveHead.Id == ConvertLeaveHeadrm.Id
                                                          && a.ReqDate >= LvCalendar.FromDate).SingleOrDefault();


                                                if (DefaultyearLvHeadCreditedCHeck == null)
                                                {
                                                    Filter_oEmpLvDataCon = _Prv_EmpLvData.LvNewReq
                                                        .Where(a => a.LeaveHead != null && a.LeaveHead.Id == ConvertLeaveHeadrm.Id && a.ReqDate >= Lastyear &&
                                                      a.ReqDate <= CreditDate).ToList();
                                                }
                                                else
                                                {
                                                    Filter_oEmpLvDataCon.Add(DefaultyearLvHeadCreditedCHeck);
                                                }
                                            }
                                            var _LvNewReq_Prv_balrm = Filter_oEmpLvDataCon.Count() > 0 ? Filter_oEmpLvDataCon.Where(e => e.LeaveHead.Id == ConvertLeaveHeadrm.Id)
                                                .OrderByDescending(e => e.Id).Select(e => e.CloseBal).FirstOrDefault() : 0;
                                            if (_LvNewReq_Prv_balrm == 0)
                                            {
                                                EmployeeLeave _Emp_LvOpenBal = db.EmployeeLeave
                                                    .Include(e => e.LvOpenBal)
                                                    .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                                    .Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == ConvertLeaveHeadrm.Id)).FirstOrDefault();
                                                if (_Emp_LvOpenBal != null)
                                                {
                                                    Prv_balrm = _Emp_LvOpenBal.LvOpenBal.Where(a => a.LvHead.Id == ConvertLeaveHeadrm.Id).Select(e => e.LvOpening).LastOrDefault();
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                Prv_balrm = _LvNewReq_Prv_balrm;
                                            }


                                            double conevrtedlvrm = 0;

                                            if (afterconvrtedremainlv > oLvCreditPolicy.LvConvertLimitBal)
                                            {
                                                conevrtedlvrm = oLvCreditPolicy.LvConvertLimitBal;

                                            }
                                            else
                                            {
                                                conevrtedlvrm = afterconvrtedremainlv;
                                            }

                                            //  LastMonthBal = Prv_bal + oLvClosingData;
                                            LastMonthBalrm = Prv_balrm + conevrtedlvrm;
                                            //-------------------------------------

                                            LvNewReq newLvConvertobjrm = new LvNewReq
                                            {
                                                InputMethod = 0,
                                                ReqDate = DateTime.Now,
                                                CloseBal = LastMonthBalrm,
                                                OpenBal = Prv_balrm,
                                                LvOccurances = 0,
                                                IsLock = true,
                                                LvLapsed = _LvLapsed,
                                                //CreditDays = oLvClosingData,
                                                CreditDays = conevrtedlvrm,
                                                //ToDate = CreditDate,
                                                //FromDate = CreditDate,
                                                // LvCreditDate = Cal_Wise_Date,
                                                LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                                    e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
                                                LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHeadrm.Id).SingleOrDefault(),
                                                Narration = "Credit Process",
                                                //LvCreditNextDate = NextCreditDays,
                                                Reason = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault().LvCode + " Converted",
                                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault(), //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
                                                DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                            };
                                            _List_oLvNewReq.Add(newLvConvertobjrm);

                                            LvNewReqForReport newLvConvertobjrmRpt = new LvNewReqForReport
                                            {
                                                CloseBal = Convert.ToString(LastMonthBalrm),
                                                OpenBal = Convert.ToString(Prv_balrm),
                                                LvLapsed = Convert.ToString(_LvLapsed),
                                                CreditDays = Convert.ToString(conevrtedlvrm),
                                                LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHeadrm.Id).SingleOrDefault().LvName,
                                            };
                                            _List_oLvNewReqRpt.Add(newLvConvertobjrmRpt);


                                            // 20/01/2020 end

                                        }
                                    }

                                    //}
                                }

                                // round parameter
                                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                                bool exists = System.IO.Directory.Exists(requiredPath);
                                string localPath;
                                if (!exists)
                                {
                                    localPath = new Uri(requiredPath).LocalPath;
                                    System.IO.Directory.CreateDirectory(localPath);
                                }
                                string path = requiredPath + @"\LvCreditround" + ".ini";
                                localPath = new Uri(path).LocalPath;
                                string roundp = "";
                                int rounddigit = 0;
                                string rndg = "";
                                using (var streamReader = new StreamReader(localPath))
                                {
                                    string line;

                                    while ((line = streamReader.ReadLine()) != null)
                                    {
                                        roundp = line.Split('_')[0];
                                        rndg = line.Split('_')[1];
                                        rounddigit = Convert.ToInt32(rndg);
                                        if (roundp == "ROUND")
                                        { }
                                    }
                                }

                                if (oLvCreditPolicy.FixedCreditDays == true)
                                {
                                    //CreditDays += oLvCreditPolicy.CreditDays;
                                    if (roundp == "")
                                    {
                                        CreditDays += Math.Round((oLvCreditPolicy.CreditDays / LYRmonth) * retmonth, 0);
                                    }
                                    else
                                    {
                                        CreditDays += Math.Round((oLvCreditPolicy.CreditDays / LYRmonth) * retmonth, rounddigit);
                                    }

                                }
                                else
                                {
                                    DateTime CreditDate1 = LvCalendar.FromDate.Value.AddDays(-1);
                                    DateTime Lastyear1 = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
                                    if (OCompany.Code.ToString() == "KDCC")
                                    {
                                        double WorkingDays1 = oLvCreditPolicy.WorkingDays;
                                        int totactday = (CreditDate1 - Lastyear1).Days + 1;
                                        double TotcreditDays = Convert.ToDouble(totactday / 11.40);
                                        TotcreditDays = Convert.ToInt32(TotcreditDays);
                                        double totleaveLwp = (totactday - SumDays);
                                        Double LWPLeave = Convert.ToDouble(totleaveLwp / WorkingDays1);

                                        int LWPLeave1 = (int)LWPLeave;
                                        if (LWPLeave > LWPLeave1)
                                        {
                                            LWPLeave1 = LWPLeave1 + 1;
                                        }

                                        CreditDays += TotcreditDays - LWPLeave1;

                                    }
                                    else
                                    {

                                        double WorkingDays = oLvCreditPolicy.WorkingDays;
                                        double MinMiseWorkingDays = Convert.ToDouble(SumDays / WorkingDays);
                                        if (roundp == "")
                                        {
                                            CreditDays += Convert.ToInt32(MinMiseWorkingDays);
                                        }
                                        else
                                        {
                                            CreditDays += Math.Round(MinMiseWorkingDays, rounddigit);
                                        }

                                    }
                                }
                                if (roundp == "NEARESTFIFTY")
                                {
                                    var Actamt = CreditDays.ToString();
                                    string rs = Actamt.Split('.')[0];
                                    string Ps = Actamt.Split('.')[1];
                                    int pais = Convert.ToInt32(Ps);
                                    if (pais >= 50)
                                    {
                                        CreditDays = Convert.ToDouble(rs + "." + "50");
                                    }
                                    else
                                    {
                                        CreditDays = Convert.ToDouble(rs + "." + "00");
                                    }

                                }
                                if (CreditDays < 0)
                                {
                                    CreditDays = 0;
                                }
                                // settlement Leave Process start
                                if (settlementemp == true)
                                {
                                    if (oLvCreditPolicy.FixedCreditDays == true)
                                    {
                                        CreditDays += Math.Round((oLvCreditPolicy.CreditDays / LYRmonth) * retmonthResignRetire, 0);
                                        if (oldcreditlv > CreditDays)
                                        {
                                            CreditDays = CreditDays - oldcreditlvclose - oLvClosingData;
                                        }
                                    }

                                }
                                // settlement Leave Process end
                                double oLvOccurances = 0;
                                if (oLvCreditPolicy.ServiceLink == true)
                                {
                                    var JoiningDate = db.Employee.Include(e => e.ServiceBookDates)
                                           .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                    DateTime tempCreditDate1 = Convert.ToDateTime(JoiningDate.ServiceBookDates.JoiningDate);
                                    DateTime till = CreditDate.Value.AddDays(1);
                                    int compMonths = (till.Month + till.Year * 12) - (tempCreditDate1.Month + tempCreditDate1.Year * 12);
                                    double daysInEndMonths = (till - till.AddMonths(1)).Days;
                                    double monthss = compMonths + (tempCreditDate1.Day - till.Day) / daysInEndMonths;
                                    var GetServiceYear = Math.Round((monthss / 12)+0.001, 0);

                                    // var GetServiceYear = Convert.ToDateTime(DateTime.Now - tempCreditDate).Year;
                                    if (GetServiceYear > oLvCreditPolicy.ServiceYearsLimit && oLvCreditPolicy.AboveServiceMaxYears == false)
                                    {
                                        CreditDays = 0;
                                    }
                                    if (oLvCreditPolicy.AboveServiceMaxYears == true)
                                    {
                                        if (GetServiceYear > oLvCreditPolicy.ServiceYearsLimit)
                                        {
                                            //double FinalServiceyear = GetServiceYear + oLvCreditPolicy.AboveServiceSteps;
                                            //if (GetServiceYear >= FinalServiceyear && GetServiceYear <= FinalServiceyear)
                                            //{
                                            //    CreditDays = 0;
                                            //}
                                            bool Chkcrdyr = false;
                                            for (double i = oLvCreditPolicy.ServiceYearsLimit; i <= GetServiceYear; )
                                            {
                                                var updatenew = i + oLvCreditPolicy.AboveServiceSteps;
                                                i = updatenew;
                                                if (updatenew == GetServiceYear)
                                                {
                                                    //will credit
                                                    Chkcrdyr = true;
                                                    break;
                                                }
                                            }
                                            if (Chkcrdyr == false)
                                            {
                                                CreditDays = 0;
                                            }
                                        }
                                    }

                                    if (oLvClosingData > oLvCreditPolicy.MaxLeaveDebitInService)
                                    {
                                        CreditDays = 0;
                                    }
                                    if (GetServiceYear > oLvCreditPolicy.ServicemaxYearsLimit)
                                    {
                                        CreditDays = 0;
                                    }


                                }
                                if (oLvCreditPolicy.OccInServAppl == true)
                                {
                                    if (item.LvCode.ToUpper() == "ML" || item.LvCode.ToUpper() == "PTL")
                                    {
                                        if (UtilizedLv >= oLvCreditPolicy.OccInServ)
                                        {
                                            CreditDays = 0;
                                        }
                                        else if (oLvCreditPolicy.OccCarryForward == true)
                                        {
                                            oLvOccurances = UtilizedLv;
                                        }
                                    }
                                }



                                // Satara DCC bank convert encash leave 90 day lock
                                string requiredPathLCK = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                                bool existsLCK = System.IO.Directory.Exists(requiredPathLCK);
                                string localPathLCK;
                                if (!existsLCK)
                                {
                                    localPathLCK = new Uri(requiredPathLCK).LocalPath;
                                    System.IO.Directory.CreateDirectory(localPathLCK);
                                }
                                string pathLCK = requiredPathLCK + @"\ConvertLeaveLock" + ".ini";
                                localPathLCK = new Uri(pathLCK).LocalPath;
                                string Lockdays = "";
                                string ConvertEncashlvcode = "";

                                using (var streamReader = new StreamReader(localPathLCK))
                                {
                                    string line;

                                    while ((line = streamReader.ReadLine()) != null)
                                    {
                                        Lockdays = line.Split('_')[0];
                                        ConvertEncashlvcode = line.Split('_')[1];
                                        if (oLvCreditPolicy.LVConvert == true)
                                        {
                                            double Lockday = Convert.ToDouble(Lockdays);
                                            LvHead ConvertLeaveHead = oLvCreditPolicy.ConvertLeaveHead;
                                            var LVname = db.LvHead.Where(e => e.Id == ConvertLeaveHead.Id).SingleOrDefault().LvName;
                                            if (ConvertLeaveHead.LvCode.ToUpper() == ConvertEncashlvcode.ToUpper())
                                            {
                                                double OB = 0;
                                                double EL = 0;
                                                double CB = 0;
                                                OB = CreditDays + oLvClosingData;
                                                if (OB > Lockday)
                                                {
                                                    if (oLvClosingData > Lockday)
                                                    {
                                                        if (CreditDays > oLvCreditPolicy.LvConvertLimit)
                                                        {
                                                            EL = oLvCreditPolicy.LvConvertLimit;
                                                            oLvClosingData = OB - EL;
                                                            CreditDays = 0;
                                                        }
                                                        else
                                                        {
                                                            EL = CreditDays;
                                                            CreditDays = 0;
                                                        }
                                                    }
                                                    else
                                                    {

                                                        EL = OB - Lockday;

                                                        if (EL > oLvCreditPolicy.LvConvertLimit)
                                                        {
                                                            EL = oLvCreditPolicy.LvConvertLimit;
                                                            oLvClosingData = OB - EL;
                                                            CreditDays = 0;//oLvClosingData - Lockday;
                                                        }
                                                        else
                                                        {
                                                            EL = EL;
                                                            oLvClosingData = OB - EL;
                                                            CreditDays = oLvClosingData - Lockday;
                                                        }
                                                    }
                                                    if (_List_oLvNewReqRpt.Where(e => e.LeaveHead.ToUpper() == LVname.ToUpper()).Count() >= 0)
                                                    {
                                                        var removeconvertedrec = _List_oLvNewReqRpt.Where(e => e.LeaveHead.ToUpper() == LVname.ToUpper()).FirstOrDefault();
                                                        _List_oLvNewReqRpt.Remove(removeconvertedrec);
                                                    }
                                                    LvNewReqForReport newLvConvertobjrmRpt = new LvNewReqForReport
                                                    {
                                                        CloseBal = Convert.ToString(EL),
                                                        OpenBal = Convert.ToString("0"),
                                                        LvLapsed = Convert.ToString("0"),
                                                        CreditDays = Convert.ToString(EL),
                                                        LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHead.Id).SingleOrDefault().LvName,
                                                    };
                                                    _List_oLvNewReqRpt.Add(newLvConvertobjrmRpt);

                                                }
                                                else
                                                {
                                                    if (_List_oLvNewReqRpt.Where(e => e.LeaveHead.ToUpper() == LVname.ToUpper()).Count() >= 0)
                                                    {
                                                        var removeconvertedrec = _List_oLvNewReqRpt.Where(e => e.LeaveHead.ToUpper() == LVname.ToUpper()).FirstOrDefault();
                                                        _List_oLvNewReqRpt.Remove(removeconvertedrec);
                                                    }
                                                    LvNewReqForReport newLvConvertobjrmRpt = new LvNewReqForReport
                                                    {
                                                        CloseBal = Convert.ToString(EL),
                                                        OpenBal = Convert.ToString("0"),
                                                        LvLapsed = Convert.ToString("0"),
                                                        CreditDays = Convert.ToString(EL),
                                                        LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHead.Id).SingleOrDefault().LvName,
                                                    };
                                                    _List_oLvNewReqRpt.Add(newLvConvertobjrmRpt);
                                                }

                                            }

                                        }

                                    }
                                }
                                // Satara DCC bank convert encash leave 90 day lock

                                double newBal = 0, LvLapsed = 0;
                                if (oLvCreditPolicy.Accumalation == true)
                                {
                                    double tempcreditdays = CreditDays;
                                    CreditDays += oLvClosingData;

                                    if (CreditDays > oLvCreditPolicy.AccumalationLimit)
                                    {
                                        LvLapsed = CreditDays - oLvCreditPolicy.AccumalationLimit;
                                        CreditDays = oLvCreditPolicy.AccumalationLimit;
                                    }
                                    if (oLvCreditPolicy.AccumulationWithCredit == true)
                                    {
                                        if (CreditDays >= oLvCreditPolicy.AccumalationLimit)
                                        {
                                            double diff = oLvCreditPolicy.AccumalationLimit - oLvClosingData;
                                            tempcreditdays = diff;
                                            //newBal = oLvClosingData - diff;
                                            // LvLapsed = newBal;
                                            // CreditDays += newBal;
                                            //if (CreditDays > oLvCreditPolicy.AccumalationLimit)
                                            //{
                                            //    CreditDays = oLvCreditPolicy.AccumalationLimit;
                                            //}
                                        }
                                    }
                                    OLvCreditRecord.CreditDays = tempcreditdays;
                                    OLvCreditRecord.OpenBal = oLvClosingData;
                                    CreditDays -= OLvCreditRecord.DebitDays;
                                }
                                else
                                {
                                    // OLvCreditRecord.OpenBal = CreditDays;
                                    OLvCreditRecord.CreditDays = CreditDays;
                                    CreditDays -= OLvCreditRecord.DebitDays;
                                }


                                if (CreditDays != 0)
                                {
                                    //  var NextCreditDays = CreditDate.Value.AddYears(1);
                                    if (OLvCreditRecord.CreditDays == 0)
                                    {
                                        // OLvCreditRecord.CreditDays = CreditDays;
                                    }
                                    if (OLvCreditRecord.OpenBal == 0)
                                    {
                                        //OLvCreditRecord.OpenBal = CreditDays;
                                    }
                                    OLvCreditRecord.LvCreditDate = Cal_Wise_Date;
                                    OLvCreditRecord.InputMethod = 0;
                                    OLvCreditRecord.IsLock = true;
                                    OLvCreditRecord.ReqDate = DateTime.Now;
                                    OLvCreditRecord.CloseBal = CreditDays;
                                    OLvCreditRecord.LVCount = oLvOccurances;
                                    OLvCreditRecord.LvLapsed = LvLapsed;
                                    //OLvCreditRecord.ToDate = CreditDate;
                                    //OLvCreditRecord.FromDate = CreditDate;
                                    OLvCreditRecord.LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                                    OLvCreditRecord.LeaveHead = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OLvCreditRecord.Narration = "Credit Process";
                                    OLvCreditRecord.LvCreditNextDate = NextCreditDays;
                                    OLvCreditRecord.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();
                                    OLvCreditRecord.DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                    _List_oLvNewReq.Add(OLvCreditRecord);

                                    OLvCreditRecordForRpt.OpenBal = Convert.ToString(OLvCreditRecord.OpenBal);
                                    OLvCreditRecordForRpt.CreditDays = Convert.ToString(OLvCreditRecord.CreditDays);
                                    OLvCreditRecordForRpt.DebitDays = Convert.ToString(OLvCreditRecord.DebitDays);
                                    OLvCreditRecordForRpt.CloseBal = Convert.ToString(CreditDays);
                                    OLvCreditRecordForRpt.LvCount = Convert.ToString(oLvOccurances);
                                    OLvCreditRecordForRpt.LvLapsed = Convert.ToString(LvLapsed);
                                    OLvCreditRecordForRpt.LeaveHead = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault().LvName;
                                    _List_oLvNewReqRpt.Add(OLvCreditRecordForRpt);

                                    //lvbank start

                                    //var _LvBankPolicy = db.LvBankPolicy.Include(e => e.LvHeadCollection)
                                    //    .Where(q => q.LvHeadCollection.Any(r => r.Id == item.Id)).SingleOrDefault();

                                    //LvBankPolicy _LvBankPolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == item.Id && e.LvHeadFormula.LvCreditPolicy != null).Select(r => r.LvHeadFormula.LvBankPolicy).FirstOrDefault();
                                    var _LvBankPolicylchead = OLvSalStruct.OEmployeeLvStructDetails.Where(e => e.OLvHeadFormula != null && e.OLvBankPolicy != null).SingleOrDefault();
                                    var _LvBankPolicylv = _LvBankPolicylchead != null ? _LvBankPolicylchead.OLvHeadFormula.LvBankPolicy.LvHeadCollection.Where(e => e.Id == item.Id).SingleOrDefault() : null;
                                    if (_LvBankPolicylv != null)
                                    {
                                        LvBankPolicy _LvBankPolicy = _LvBankPolicylchead.OLvHeadFormula.LvBankPolicy;

                                        double debitpolicydays = 0;
                                        var JoiningDate = db.Employee.Include(e => e.ServiceBookDates)
                                          .Where(a => a.Id == oEmployeeId).SingleOrDefault();
                                        DateTime tempCreditDate1 = Convert.ToDateTime(JoiningDate.ServiceBookDates.JoiningDate);
                                        DateTime till = CreditDate.Value.AddDays(1);
                                        int compMonths = (till.Month + till.Year * 12) - (tempCreditDate1.Month + tempCreditDate1.Year * 12);
                                        double daysInEndMonths = (till - till.AddMonths(1)).Days;
                                        double monthss = compMonths + (tempCreditDate1.Day - till.Day) / daysInEndMonths;
                                        var GetServiceYear = Math.Round(monthss / 12, 0);
                                        if (_LvBankPolicy.IsSeviceLockOnDebit == true)
                                        {
                                            if (GetServiceYear <= _LvBankPolicy.MaxServiceForDebit)
                                            {
                                                double lvbankcloseingbal = 0;
                                                var _LvBankOpenBalprv = db.LvBankLedger
                                                  .OrderByDescending(e => e.Id)
                                                 .FirstOrDefault();
                                                if (_LvBankOpenBalprv != null)
                                                {
                                                    lvbankcloseingbal = _LvBankOpenBalprv.ClosingBalance;
                                                }



                                                // reqution code
                                                if (OLvCreditRecord != null)
                                                {
                                                    newLvbankdebtreq = new LvNewReq
                                                    {
                                                        InputMethod = 0,
                                                        ReqDate = DateTime.Now,
                                                        CloseBal = OLvCreditRecord.CloseBal - _LvBankPolicy.LvDebitInCredit,
                                                        OpenBal = OLvCreditRecord.CloseBal,
                                                        LvOccurances = 0,
                                                        IsLock = true,
                                                        LvLapsed = 0,
                                                        DebitDays = _LvBankPolicy.LvDebitInCredit,
                                                        CreditDays = 0,
                                                        //ToDate = CreditDate,
                                                        //FromDate = CreditDate,
                                                        LvCreditDate = Cal_Wise_Date,
                                                        LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                                            e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),

                                                        LeaveHead = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault(),
                                                        Narration = settlementemp == true ? "Settlement Process" : "Credit Process",
                                                        Reason = "Leave Bank Debit Request",
                                                        LvCreditNextDate = NextCreditDays,
                                                        WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault(),  //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
                                                        DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                                    };
                                                    _List_oLvNewReq.Add(newLvbankdebtreq);

                                                    LvNewReqForReport newLvConvertobjrmRpt = new LvNewReqForReport
                                                    {
                                                        CloseBal = Convert.ToString(OLvCreditRecord.CloseBal - _LvBankPolicy.LvDebitInCredit),
                                                        OpenBal = Convert.ToString(OLvCreditRecord.CloseBal),
                                                        DebitDays = Convert.ToString(_LvBankPolicy.LvDebitInCredit),
                                                        LeaveHead = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault().LvName,
                                                    };
                                                    _List_oLvNewReqRpt.Add(newLvConvertobjrmRpt);


                                                    // LvBankLedger OLvCreditRecordLvBankLedger = new LvBankLedger();
                                                    //  var _LvBankLedger = db.LvBankLedger.Where(e => e.EmployeeLeave_Id == OEmpLvID).SingleOrDefault();
                                                    var calid = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                                                    //  var LvBankOpenBal_data = db.LvBankOpenBal.Where(e => e.LvCalendar_Id == calid.Id).SingleOrDefault();

                                                    LvBankLedger _OLvCreditRecordLvBankLedger = new LvBankLedger()
                                                    {
                                                        OpeningBalance = lvbankcloseingbal,
                                                        CreditDays = _LvBankPolicy.LvDebitInCredit,
                                                        ClosingBalance = lvbankcloseingbal + _LvBankPolicy.LvDebitInCredit,
                                                        CreditDate = DateTime.Now,
                                                        DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                                        LvNewReq = newLvbankdebtreq,
                                                        Narration = "Credit Process",
                                                        EmployeeLeave_Id = OEmpLvID,

                                                    };

                                                    db.LvBankLedger.Add(_OLvCreditRecordLvBankLedger);
                                                    db.SaveChanges();


                                                    var companyLeave = new CompanyLeave();
                                                    companyLeave = db.CompanyLeave.Include(e => e.LvBankLedger).Where(e => e.Company.Id == OCompany.Id).SingleOrDefault();
                                                    if (companyLeave != null)
                                                    {
                                                        var LvBankLedger_list = new List<LvBankLedger>();
                                                        if (companyLeave.LvBankLedger.Count() > 0)
                                                        {
                                                            LvBankLedger_list.AddRange(companyLeave.LvBankLedger);
                                                        }

                                                        LvBankLedger_list.Add(_OLvCreditRecordLvBankLedger);
                                                        companyLeave.LvBankLedger = LvBankLedger_list;
                                                        db.CompanyLeave.Attach(companyLeave);
                                                        db.Entry(companyLeave).State = System.Data.Entity.EntityState.Modified;
                                                        db.SaveChanges();
                                                        db.Entry(companyLeave).State = System.Data.Entity.EntityState.Detached;
                                                    }





                                                }




                                            }
                                        }
                                        else
                                        {

                                            //LvBankOpenBal OLvCreditRecordLvBankOpenBal = new LvBankOpenBal();
                                            //var _LvBankOpenBalprv = db.LvBankOpenBal
                                            //   .Where(a => a.CreditDate >= Lastyear &&
                                            //              a.CreditDate <= CreditDate
                                            //  ).SingleOrDefault();
                                            double lvbankcloseingbal = 0;
                                            var _LvBankOpenBalprv = db.LvBankLedger
                                              .OrderByDescending(e => e.Id)
                                             .FirstOrDefault();
                                            if (_LvBankOpenBalprv != null)
                                            {
                                                lvbankcloseingbal = _LvBankOpenBalprv.ClosingBalance;
                                            }

                                            //if (_LvBankOpenBalprv != null)
                                            //{
                                            //    var _LvBankOpenBal = db.LvBankOpenBal
                                            //    .Where(e => e.LvCalendar.Id == LvCalendarId
                                            //   ).SingleOrDefault();

                                            //    if (_LvBankOpenBal == null)
                                            //    {
                                            //        // OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
                                            //        debitpolicydays = _LvBankPolicy.LvDebitInCredit;
                                            //        LvBankOpenBal _OLvCreditRecordLvBankOpenBal = new LvBankOpenBal()
                                            //        {
                                            //            OpeningBalance = _LvBankOpenBalprv.ClosingBalance,
                                            //            CreditDays = _LvBankPolicy.LvDebitInCredit,

                                            //            UtilizedDays = 0,
                                            //            CreditDate = DateTime.Now,
                                            //            DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                            //            LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault()
                                            //        };

                                            //        db.LvBankOpenBal.Add(_OLvCreditRecordLvBankOpenBal);
                                            //        db.SaveChanges();
                                            //    }
                                            //    else
                                            //    {
                                            //        // OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
                                            //        debitpolicydays = _LvBankPolicy.LvDebitInCredit;
                                            //        _LvBankOpenBal.CreditDays = _LvBankOpenBal.CreditDays + _LvBankPolicy.LvDebitInCredit;
                                            //        _LvBankOpenBal.UtilizedDays = 0;
                                            //        _LvBankOpenBal.CreditDate = DateTime.Now;
                                            //        _LvBankOpenBal.DBTrack = new DBTrack() { Action = "M", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                            //        _LvBankOpenBal.LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                                            //        db.LvBankOpenBal.Attach(_LvBankOpenBal);
                                            //        db.Entry(_LvBankOpenBal).State = System.Data.Entity.EntityState.Modified;
                                            //        db.SaveChanges();
                                            //    }

                                            //}
                                            //else
                                            //{
                                            //    var _LvBankOpenBal = db.LvBankOpenBal
                                            //   .Where(e => e.LvCalendar.Id == LvCalendarId
                                            //  ).SingleOrDefault();

                                            //    if (_LvBankOpenBal == null)
                                            //    {
                                            //        //  OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
                                            //        debitpolicydays = _LvBankPolicy.LvDebitInCredit;
                                            //        LvBankOpenBal _OLvCreditRecordLvBankOpenBal = new LvBankOpenBal()
                                            //        {
                                            //            OpeningBalance = 0,
                                            //            CreditDays = _LvBankPolicy.LvDebitInCredit,
                                            //            UtilizedDays = 0,
                                            //            CreditDate = DateTime.Now,
                                            //            DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                            //            LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault()
                                            //        };

                                            //        db.LvBankOpenBal.Add(_OLvCreditRecordLvBankOpenBal);
                                            //        db.SaveChanges();
                                            //    }
                                            //    else
                                            //    {
                                            //        //  OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
                                            //        debitpolicydays = _LvBankPolicy.LvDebitInCredit;
                                            //        _LvBankOpenBal.CreditDays = _LvBankOpenBal.CreditDays + _LvBankPolicy.LvDebitInCredit;
                                            //        _LvBankOpenBal.UtilizedDays = 0;
                                            //        _LvBankOpenBal.CreditDate = DateTime.Now;
                                            //        _LvBankOpenBal.DBTrack = new DBTrack() { Action = "M", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                            //        _LvBankOpenBal.LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                                            //        db.LvBankOpenBal.Attach(_LvBankOpenBal);
                                            //        db.Entry(_LvBankOpenBal).State = System.Data.Entity.EntityState.Modified;
                                            //        db.SaveChanges();

                                            //    }
                                            //}

                                            // reqution code

                                            if (OLvCreditRecord != null)
                                            {
                                                newLvbankdebtreq = new LvNewReq
                                                {
                                                    InputMethod = 0,
                                                    ReqDate = DateTime.Now,
                                                    CloseBal = OLvCreditRecord.CloseBal - _LvBankPolicy.LvDebitInCredit,
                                                    OpenBal = OLvCreditRecord.CloseBal,
                                                    LvOccurances = 0,
                                                    IsLock = true,
                                                    LvLapsed = 0,
                                                    DebitDays = _LvBankPolicy.LvDebitInCredit,
                                                    CreditDays = 0,
                                                    //ToDate = CreditDate,
                                                    //FromDate = CreditDate,
                                                    LvCreditDate = Cal_Wise_Date,
                                                    LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                                        e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),

                                                    LeaveHead = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault(),
                                                    Narration = settlementemp == true ? "Settlement Process" : "Credit Process",
                                                    Reason = "Leave Bank Debit Request",
                                                    LvCreditNextDate = NextCreditDays,
                                                    WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault(),  //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
                                                    DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                                };
                                                _List_oLvNewReq.Add(newLvbankdebtreq);

                                                LvNewReqForReport newLvConvertobjrmRpt = new LvNewReqForReport
                                                {
                                                    CloseBal = Convert.ToString(OLvCreditRecord.CloseBal - _LvBankPolicy.LvDebitInCredit),
                                                    OpenBal = Convert.ToString(OLvCreditRecord.CloseBal),
                                                    DebitDays = Convert.ToString(_LvBankPolicy.LvDebitInCredit),
                                                    LeaveHead = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault().LvName,
                                                };
                                                _List_oLvNewReqRpt.Add(newLvConvertobjrmRpt);


                                                // LvBankLedger OLvCreditRecordLvBankLedger = new LvBankLedger();
                                                var _LvBankLedger = db.LvBankLedger.Where(e => e.EmployeeLeave_Id == OEmpLvID).SingleOrDefault();
                                                var calid = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                                                var LvBankOpenBal_data = db.LvBankOpenBal.Where(e => e.LvCalendar_Id == calid.Id).SingleOrDefault();

                                                LvBankLedger _OLvCreditRecordLvBankLedger = new LvBankLedger()
                                                {
                                                    OpeningBalance = lvbankcloseingbal,
                                                    CreditDays = _LvBankPolicy.LvDebitInCredit,
                                                    ClosingBalance = lvbankcloseingbal + _LvBankPolicy.LvDebitInCredit,
                                                    CreditDate = DateTime.Now,
                                                    DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                                    LvNewReq = newLvbankdebtreq,
                                                    Narration = "Credit Process",
                                                    EmployeeLeave_Id = OEmpLvID,

                                                };

                                                db.LvBankLedger.Add(_OLvCreditRecordLvBankLedger);
                                                db.SaveChanges();


                                                var companyLeave = new CompanyLeave();
                                                companyLeave = db.CompanyLeave.Include(e => e.LvBankLedger).Where(e => e.Company.Id == OCompany.Id).SingleOrDefault();
                                                if (companyLeave != null)
                                                {
                                                    var LvBankLedger_list = new List<LvBankLedger>();
                                                    if (companyLeave.LvBankLedger.Count() > 0)
                                                    {
                                                        LvBankLedger_list.AddRange(companyLeave.LvBankLedger);
                                                    }

                                                    LvBankLedger_list.Add(_OLvCreditRecordLvBankLedger);
                                                    companyLeave.LvBankLedger = LvBankLedger_list;
                                                    db.CompanyLeave.Attach(companyLeave);
                                                    db.Entry(companyLeave).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    db.Entry(companyLeave).State = System.Data.Entity.EntityState.Detached;
                                                }





                                            }


                                        }



                                    }
                                    //lvbank end

                                    if (_List_oLvNewReq.Count > 0)
                                    {
                                        // LvOrignal id use for leave cancel
                                        //if (_List_oLvNewReq.Count >= 2)
                                        //{
                                        //    if (newLvbankdebtreq == null)
                                        //    {
                                        //        _List_oLvNewReq[0].LvOrignal = OLvCreditRecord;
                                        //    }

                                        //    if (_List_oLvNewReq.Count == 3)
                                        //    {
                                        //        _List_oLvNewReq[1].LvOrignal = OLvCreditRecord;
                                        //    }
                                        //}
                                        var _Emp = db.EmployeeLeave.Include(e => e.LvNewReq)
                                            .Where(e => e.Employee.Id == oEmployeeId).SingleOrDefault();
                                        for (int i = 0; i < _List_oLvNewReq.Count; i++)
                                        {
                                            _Emp.LvNewReq.Add(_List_oLvNewReq[i]);
                                        }
                                        db.Entry(_Emp).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(_Emp).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                            }
                        }

                    }
                }
                if (_List_oLvNewReqRpt.Count > 0)
                {
                    return _List_oLvNewReqRpt;
                }
            }
            catch (Exception e)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    P2B.UTILS.P2BLogger logger = new P2B.UTILS.P2BLogger();
                    var EmpCode = db.Employee.Find(oEmployeeId).EmpCode;
                    string ErrorMsg = e.Message;
                    string ErrorInnerMsg = e.InnerException != null ? e.InnerException.Message : "";
                    string LineNo = Convert.ToString(new StackTrace(e, true).GetFrame(0).GetFileLineNumber());
                    //string errorconcat = ErrorMsg + ErrorInnerMsg + LineNo;
                    //var ServiceResult = new ServiceResult<string>() { Message = errorconcat, MessageCode = HttpStatusCode.BadRequest };
                    //Utils.Log("New Leave Request : " + JsonConvert.SerializeObject(ServiceResult));
                    //return Json(new Object[] {ServiceResult, JsonRequestBehavior.AllowGet});
                    logger.Logging("Leave Credit Trial Report " + "  " + "EmpCode :: " + EmpCode + " " + "ErrorMsg : " + ErrorMsg + " " + " ErrorInnerMsg : " + ErrorInnerMsg + " LineNo : " + LineNo);
                    string errorleavecreditprocessreport = ("Leave Credit Process Report " + "  " + "EmpCode :: " + EmpCode + " " + "ErrorMsg : " + ErrorMsg + " " + " ErrorInnerMsg : " + ErrorInnerMsg + " LineNo : " + LineNo);
                    LeaveCreditProcessReportMsg.Add(errorleavecreditprocessreport);
                    System.Web.HttpContext.Current.Session["LeaveCreditProcessReportMsg"] = LeaveCreditProcessReportMsg;

                }

            }
            return null;
        }

        public static Int32 Delete_CreditRecord(Int32 EmpId, Int32 LvCalendarId, Int32 LvHead_ids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                EmployeeLeave oEmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == EmpId)
                      .Include(a => a.LvNewReq)
                      .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                      .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                      .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                      .Include(a => a.LvNewReq.Select(e => e.LvOrignal))
                      .Where(e => e.LvNewReq.Any(a => a.LeaveCalendar.Id == LvCalendarId))
                      .AsParallel()
                      .SingleOrDefault();
                var EmpLvHeadList = db.LvHead.Where(e => e.Id == LvHead_ids)
                      .Select(a => new { Id = a.Id, LvCode = a.LvCode }).Distinct().AsParallel().ToList();
                foreach (var item in EmpLvHeadList)
                {
                    List<LvNewReq> prevCreditProcessData = new List<LvNewReq>();
                    if (oEmpLvData != null)
                    {
                        Int32 _Chk_lv_apply_or_not = oEmpLvData.LvNewReq.Where(a =>
                            a.LeaveCalendar.Id == LvCalendarId &&
                              a.LeaveHead.Id == item.Id
                              && a.WFStatus.LookupVal != "3").Count();

                        if (_Chk_lv_apply_or_not == 0)
                        {
                            List<LvNewReq> _temp_prevCreditProcessData = new List<LvNewReq>();

                            _temp_prevCreditProcessData = oEmpLvData.LvNewReq.Where(a => a.LeaveCalendar.Id == LvCalendarId && a.LeaveHead.Id == item.Id &&
                               a.WFStatus.LookupVal == "3").ToList();
                            foreach (var s in _temp_prevCreditProcessData)
                            {
                                var bb = oEmpLvData.LvNewReq.Where(a => a.LeaveCalendar.Id == LvCalendarId && a.LvOrignal != null && a.LvOrignal.Id == s.Id).ToList();
                                if (bb.Count > 0)
                                {
                                    prevCreditProcessData.AddRange(bb);
                                }
                            }
                            prevCreditProcessData.AddRange(_temp_prevCreditProcessData);
                        }
                        else
                        {
                            return 0;
                            //lv apply can't credit for head
                        }
                    }
                    else
                    {
                        return 1;
                    }
                    if (prevCreditProcessData != null && prevCreditProcessData.Count > 0)
                    {
                        db.LvNewReq.RemoveRange(prevCreditProcessData);
                        db.SaveChanges();
                        return 1;
                    }
                    else
                    {
                        return 1;

                    }
                }
                return 1;
            }
        }

    }
}