using P2b.Global;
using P2BUltimate.Process;
using Payroll;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using P2BUltimate.App_Start;
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
using P2BUltimate.Security;
using Attendance;

namespace P2BUltimate.Controllers.Core.MainController
{
    public class DeputationAccessController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /TrainingPresenty/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/DeputationAccess/Index.cshtml");
        }

        //public ActionResult GridPartial()
        //{
        //    return View("~/Views/Shared/Core/_LoginAccessPartial.cshtml");
        //}

        public class P2BCrGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }

        }

        public ActionResult Get_Employelist(P2BGrid_Parameters gp, bool EmployeeSource)
        {

            try
            {
                //string monthyr = param;
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                List<Employee> data = new List<Employee>();
                List<Employee> model = new List<Employee>();
                DateTime Processdatefrom;
                DateTime ProcessdateTo;
                Calendar lvyr = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                if (EmployeeSource == true)
                {
                    var Processeddata = db.TransferServiceBook.Where(e => e.Narration.Contains("ONDEPUTATION")).OrderByDescending(e => e.Id).FirstOrDefault();
                    if (Processeddata != null)
                    {
                        Processdatefrom = Convert.ToDateTime(Processeddata.ProcessTransDate.ToString());
                        ProcessdateTo = DateTime.Now;

                        // After First Time For Deputation

                        var empL = db.Employee.Include(e => e.GeoStruct)
                     .Include(e => e.EmpName)
                     .ToList();
                        foreach (var eid in empL)
                        {
                            int empid = Convert.ToInt32(eid.Id);
                            var LwfProcessdataemp = db.EmployeeLeave.Include(e => e.LvNewReq)
                                .Include(e => e.LvNewReq.Select(c => c.WFStatus))
                                 .Include(e => e.LvNewReq.Select(c => c.Incharge))
                                 .Include(e => e.LvNewReq.Select(x => x.LvWFDetails)).Where(e => e.Employee.Id == empid).FirstOrDefault();

                            var LwfProcessdata = LwfProcessdataemp.LvNewReq.Where(e => e.TrClosed == true && e.IsCancel == false && e.TrReject == false && e.WFStatus.LookupVal != "2" && e.Incharge != null && e.LeaveCalendar.Id == lvyr.Id).ToList();
                            var lvwfdata = LwfProcessdata.Where(e => e.LvWFDetails.Count > 0 && (e.LvWFDetails.LastOrDefault().WFStatus == 3 || e.LvWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == true).OrderByDescending(x => x.Id).ToList();
                            foreach (var item in lvwfdata)
                            {
                                var approvedate = item.LvWFDetails.Where(e => e.DBTrack.CreatedOn > Processdatefrom && e.DBTrack.CreatedOn <= ProcessdateTo).FirstOrDefault();
                                if (approvedate != null)
                                {
                                    int inchid = item.Incharge.Id;
                                    var empinch = db.Employee.Include(e => e.GeoStruct).Where(e => e.Id == inchid).FirstOrDefault();

                                    if (empinch.GeoStruct.Id != eid.GeoStruct.Id)
                                    {
                                        model.Add(empinch);
                                    }
                                }

                            }

                        }


                    }
                    else
                    {
                        // First Time For Deputation
                        var empL = db.Employee.Include(e => e.GeoStruct)
                            .Include(e => e.EmpName)
                            .ToList();
                        foreach (var eid in empL)
                        {
                            int empid = Convert.ToInt32(eid.Id);
                            var LwfProcessdataemp = db.EmployeeLeave.Include(e => e.LvNewReq)
                                .Include(e => e.LvNewReq.Select(c => c.WFStatus))
                                 .Include(e => e.LvNewReq.Select(c => c.Incharge))
                                 .Include(e => e.LvNewReq.Select(x => x.LvWFDetails)).Where(e => e.Employee.Id == empid).FirstOrDefault();

                            var LwfProcessdata = LwfProcessdataemp.LvNewReq.Where(e => e.TrClosed == true && e.IsCancel == false && e.TrReject == false && e.WFStatus.LookupVal != "2" && e.Incharge != null && e.LeaveCalendar.Id == lvyr.Id).ToList();
                            var lvwfdata = LwfProcessdata.Where(e => e.LvWFDetails.Count > 0 && (e.LvWFDetails.LastOrDefault().WFStatus == 3 || e.LvWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == true).OrderByDescending(x => x.Id).ToList();
                            foreach (var item in lvwfdata)
                            {
                                int inchid = item.Incharge.Id;
                                var empinch = db.Employee.Include(e => e.GeoStruct).Where(e => e.Id == inchid).FirstOrDefault();

                                if (empinch.GeoStruct.Id != eid.GeoStruct.Id)
                                {
                                    model.Add(empinch);
                                }


                            }

                        }


                    }
                }
                else
                {
                    // deputaion off employee
                    var empdata = db.CompanyPayroll.Where(e => e.Company.Id == compid)
                        .Include(e => e.EmployeePayroll)
                        .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                        .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName)
                        //.Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                        //.Include(e => e.EmployeePayroll.Select(a => a.TransferServiceBook))
                        // .Include(e => e.EmployeePayroll.Select(a => a.Employee.Login)
                        ).AsNoTracking().OrderBy(e => e.Id)
                       .SingleOrDefault();
                    var emp = empdata.EmployeePayroll.ToList();
                    DateTime tilldate = DateTime.Now.Date;
                    foreach (var z in emp)
                    {
                        var Processeddata = db.TransferServiceBook.Where(e => e.Narration.Contains("ONDEPUTATION") && e.EmployeePayroll_Id == z.Id && e.ReleaseDate < tilldate && e.ReleaseFlag == false).FirstOrDefault();
                        if (Processeddata != null)
                        {
                            var items = Processeddata.Narration.Split(',');
                            string lvnewreqid = items[0];
                            string Narr = items[1];
                            int reqid = Convert.ToInt32(lvnewreqid);
                            var lvnewrequest = db.LvNewReq.Include(x => x.Incharge).Where(e => e.Id == reqid).FirstOrDefault();
                            if (lvnewrequest != null)
                            {

                                var empinch = db.Employee.Include(e => e.GeoStruct).Include(e => e.EmpName).Where(e => e.Id == lvnewrequest.Incharge.Id).FirstOrDefault();
                                model.Add(empinch);
                            }

                            //int empid = Convert.ToInt32(z.Employee.Id);
                            //var LwfProcessdataemp = db.EmployeeLeave.Include(e => e.LvNewReq)
                            //    .Include(e => e.LvNewReq.Select(c => c.WFStatus))
                            //     .Include(e => e.LvNewReq.Select(c => c.Incharge))
                            //     .Include(e => e.LvNewReq.Select(x => x.LvWFDetails)).Where(e => e.Employee.Id == empid).FirstOrDefault();

                            //var LwfProcessdata = LwfProcessdataemp.LvNewReq.Where(e => e.TrClosed == true && e.IsCancel == false && e.TrReject == false && e.WFStatus.LookupVal != "2" && e.Incharge != null && e.LeaveCalendar.Id == lvyr.Id).ToList();
                            //var lvwfdata = LwfProcessdata.Where(e => e.LvWFDetails.Count > 0 && (e.LvWFDetails.LastOrDefault().WFStatus == 3 || e.LvWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == true && e.FromDate <= DateTime.Now.Date).OrderByDescending(x => x.Id).ToList();
                            //foreach (var item in lvwfdata)
                            //{
                            //    int inchid = item.Incharge.Id;
                            //    var empinch = db.Employee.Include(e => e.GeoStruct).Where(e => e.Id == inchid).FirstOrDefault();


                            //    model.Add(empinch);



                            //}

                        }

                    }


                }



                data = model;

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data != null && data.Count != 0)
                {
                    foreach (var item in data.Distinct().OrderBy(e => e.EmpCode))
                    {

                        returndata.Add(new Utility.returndataclass
                        {
                            code = item.Id.ToString(),
                            value = item.FullDetails,
                        });

                    }
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "No Record Found...!", data = "Employee-Table" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        public class returnDataClass
        {

            public bool IsPresent { get; set; }
            public bool IsCancelled { get; set; }
            public string CancelReason { get; set; }
            public string Batchname { get; set; }
        }







        public List<string> transferActivity(string ReleaseDatestr, string txtNarrationRelease, string NewGeoStruct, String NewFuncStruct, String NewPayStruct, String TransActivity, String TransPolicy, DateTime ProcessTransDate, Int32 Emp, TransferServiceBook TransferServiceBook)
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {

                Msg.Clear();
                // var date = Convert.ToDateTime(ProcessTransDate).ToString("MM/yyyy");
                var date = Convert.ToDateTime(ReleaseDatestr).ToString("MM/yyyy");
                int CompId = 0;

                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    CompId = int.Parse(Session["CompId"].ToString());
                }
                var check = db.CPIEntryT.Where(e => e.PayMonth == date).ToList();

                var empcode = db.Employee.Include(e => e.GeoStruct)
                                                          .Include(e => e.PayStruct)
                                                          .Include(e => e.FuncStruct)
                                                          .Where(e => e.Id == Emp).FirstOrDefault();

                if (check.Count() == 0)
                {
                    Msg.Add("Kindly run CPI first and then try again" + "For emp code" + empcode.EmpCode);

                    //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    return Msg;
                }
                Employee OEmployee = null;
                EmployeePayroll OEmployeePayroll = null;


                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                .Include(e => e.EmpOffInfo)
                                .Where(r => r.Id == Emp).SingleOrDefault();

                OEmployeePayroll = db.EmployeePayroll.Include(e => e.TransferServiceBook).Where(e => e.Employee.Id == Emp).SingleOrDefault();
                if (OEmployeePayroll.TransferServiceBook.Any(a => a.ProcessTransDate.Value.ToShortDateString() == ProcessTransDate.ToString()))
                {
                    Msg.Add("Already transfer Policy for Date= " + ProcessTransDate + "For emp code" + empcode.EmpCode);
                    return Msg;

                    //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                TransferServiceBook.Narration = txtNarrationRelease;
                TransferServiceBook.ProcessTransDate = Convert.ToDateTime(ProcessTransDate);
                TransferServiceBook.ReleaseDate = Convert.ToDateTime(ReleaseDatestr);

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

                        Msg.Add("Kindly select NewFuncStruct." + "For emp code" + empcode.EmpCode);
                        return Msg;

                        // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                        //    List<string> Msgn = new List<string>();
                        Msg.Add("Kindly select NewGeoStruct." + "For emp code" + empcode.EmpCode);
                        return Msg;
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                    Msg.Add("Can Not Tranfer To Same Location and Same Department" + "For emp code" + empcode.EmpCode);
                    return Msg;
                    //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                    //  Msgs.Add("Data Saved successfully");

                    //var InchargeChk = db.GeoStruct.Include(e => e.Location).Include(e => e.Location.Incharge).Include(e => e.Location.LocationObj)
                    //    .Where(e => e.Id == TransferServiceBook.OldGeoStruct.Id && e.Location.Incharge.Id == OEmployee.Id).SingleOrDefault();
                    //string location = "";
                    //if (InchargeChk != null)
                    //{
                    //    Msgs.Add("Transfer Employee ,he/she is In charge of " + location + " location,Please change In charge in location page.");
                    //}
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
                    //var CompCode = db.Company.Where(e => e.Id == CompId).AsNoTracking().AsParallel().Select(a => a.Code.ToUpper()).SingleOrDefault();  //asd
                    //if (CompCode == "JSBL")
                    //{
                    //    Employee OEmployeeAdd = null;
                    //    OEmployeeAdd = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                    //              .Include(e => e.EmpOffInfo)
                    //              .Include(e => e.PerAddr)
                    //              .Include(e => e.PerAddr.City)
                    //              .Where(r => r.Id == Emp).SingleOrDefault();
                    //    var Pcity = OEmployeeAdd.PerAddr != null && OEmployeeAdd.PerAddr.City != null ? OEmployeeAdd.PerAddr.City.Name.ToString() : null;


                    //    int Jeoid = Convert.ToInt32(NewGeoStruct);
                    //    var newCity = db.GeoStruct.Include(e => e.Location)
                    //        .Include(e => e.Location.Address)
                    //        .Include(e => e.Location.Address.City)
                    //        .Where(e => e.Company.Id == CompId && e.Id == Jeoid).SingleOrDefault();
                    //    var Transfercity = newCity.Location.Address != null && newCity.Location.Address.City != null ? newCity.Location.Address.City.Name.ToString() : null;

                    //    if (Pcity == null || Transfercity == null || (Pcity.ToUpper() != Transfercity.ToUpper()))
                    //    {
                    //        Msgs.Add("Please Confirm Discomfort Allowance For This Employee ");
                    //    }

                    //}

                    ts.Complete();

                    // return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

                }



            }
            return Msg;

        }

        public static void Incharge(string OldIncharge, string NewIncharge)
        {


            using (DataBaseContext db = new DataBaseContext())
            {
                int oldinc = Convert.ToInt32(OldIncharge);
                int Newinc = Convert.ToInt32(NewIncharge);
                var Division = db.Division.Include(e => e.Incharge).Where(e => e.Incharge.Id == oldinc).ToList();
                
                foreach (var Divs in Division)
                {
                    Division Division_update = db.Division.Find(Divs.Id);
                    Division_update.Incharge = db.Employee.Find(Newinc);;
                    db.Division.Attach(Division_update);
                    db.Entry(Division_update).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }


                var dept = db.Department.Include(e => e.Incharge).Where(e => e.Incharge.Id == oldinc).ToList();
                
                foreach (var D in dept)
                {
                    Department Department_update = db.Department.Find(D.Id);
                    Department_update.Incharge = db.Employee.Find(Newinc); ;
                    db.Department.Attach(Department_update);
                    db.Entry(Department_update).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                var Loc = db.Location.Include(e => e.Incharge).Where(e => e.Incharge.Id == oldinc).ToList();
                
                foreach (var L in Loc)
                {
                     Location Location_update = db.Location.Find(L.Id);
                    Location_update.Incharge = db.Employee.Find(Newinc); ;
                    db.Location.Attach(Location_update);
                    db.Entry(Location_update).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                
            }

        }
        [HttpPost]
        public async Task<ActionResult> Create(TransferServiceBook TransferServiceBook, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                List<string> Msgall = new List<string>();
                var Emplist = form["Employee-Table"] == null ? "" : form["Employee-Table"];
                var Deputation = form["Deputation"];
                var WorkingLocation = form["WorkingLocation"];
                bool Auth = form["Autho_Allow"] == "true" ? true : false;

                DateTime Processdatefrom = DateTime.Now;
                DateTime ProcessdateTo = DateTime.Now;
                Calendar lvyr = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                var compid = Convert.ToInt32(Session["CompId"].ToString());


                List<int> EmpId = null;
                if (Emplist != null && Emplist != "")
                {

                    EmpId = Utility.StringIdsToListIds(Emplist);

                }
                if (EmpId == null)
                {
                    Msg.Add("Please select employee...! ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }


                var CompId = int.Parse(SessionManager.CompanyId);



                if (Auth == false)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {


                            if (Deputation == "true")
                            {
                                var Processeddata = db.TransferServiceBook.Where(e => e.Narration.Contains("ONDEPUTATION")).OrderByDescending(e => e.Id).FirstOrDefault();
                                if (Processeddata != null)
                                {
                                    Processdatefrom = Convert.ToDateTime(Processeddata.ProcessTransDate.ToString());
                                    ProcessdateTo = DateTime.Now;
                                }


                                int sid = 0;
                                string sfull = null;
                                foreach (var a in EmpId)
                                {
                                    int empid = Convert.ToInt32(a);
                                    if (Processeddata != null)
                                    {


                                        //====================================================== Process date not null

                                        var Empleave = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.LvNewReq)
                                          .Include(e => e.LvNewReq.Select(x => x.LvWFDetails))
                                          .Include(e => e.LvNewReq.Select(z => z.Incharge))
                                          .Include(e => e.LvNewReq.Select(z => z.WFStatus))
                                          .ToList();

                                        foreach (var eid in Empleave)
                                        {
                                            using (TransactionScope ts1 = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                            {
                                                var LwfProcessdata = eid.LvNewReq.Where(e => e.TrClosed == true && e.IsCancel == false && e.TrReject == false && e.WFStatus.LookupVal != "2" && e.Incharge != null && e.LeaveCalendar.Id == lvyr.Id && e.Incharge.Id == empid).ToList();
                                                var lvwfdata = LwfProcessdata.Where(e => e.LvWFDetails.Count > 0 && (e.LvWFDetails.LastOrDefault().WFStatus == 3 || e.LvWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == true).OrderByDescending(x => x.Id).ToList();
                                                foreach (var item in lvwfdata)
                                                {
                                                    var approvedate = item.LvWFDetails.Where(e => e.DBTrack.CreatedOn > Processdatefrom && e.DBTrack.CreatedOn <= ProcessdateTo).FirstOrDefault();
                                                    if (approvedate != null)
                                                    {


                                                        int lvreqid = item.Id;


                                                        if (eid.LvNewReq.Where(e => e.Id == lvreqid).Any() == true)
                                                        {
                                                            int emptakenleave = eid.Employee.Id;
                                                            var empLoc = db.Employee.Include(e => e.GeoStruct)
                                                                .Include(e => e.PayStruct)
                                                                .Include(e => e.FuncStruct)
                                                                .Where(e => e.Id == emptakenleave).FirstOrDefault();

                                                            var inchempLoc = db.Employee.Include(e => e.GeoStruct)
                                                                .Include(e => e.PayStruct)
                                                                .Include(e => e.FuncStruct)
                                                                .Where(e => e.Id == empid).FirstOrDefault();

                                                            // transfer  Code Start
                                                            DateTime mReleaseDate = Convert.ToDateTime(item.ToDate.Value.Date);// update last in process
                                                            string ReleaseDatestr = item.FromDate.ToString(); // Release date take From date because we have require Structure
                                                            string txtNarrationRelease = lvreqid + "," + "ONDEPUTATION";

                                                            string NewGeoStruct = empLoc.GeoStruct.Id.ToString(); //form["NewGeoT-table"] == "0" ? "" : form["NewGeoT-table"];
                                                            string NewFuncStruct = inchempLoc.FuncStruct.Id.ToString(); //form["NewFuncT-table"] == "0" ? "" : form["NewFuncT-table"];
                                                            string NewPayStruct = inchempLoc.PayStruct.Id.ToString();//form["NewPayT-table"] == "0" ? "" : form["NewPayT-table"];

                                                            var qurey = db.TransActivity.Where(e => e.Name.ToUpper() == "DEPUTATION").SingleOrDefault();
                                                            int TransPolicyId = Convert.ToInt32(qurey.Id);

                                                            var OPolicyT = db.TransActivity.Where(e => e.Id == TransPolicyId)
                                                               .Include(e => e.TranPolicy)
                                                              .SingleOrDefault();
                                                            int tPolicyid = OPolicyT.TranPolicy.Id;

                                                            string TransActivity = qurey.Id.ToString();
                                                            string TransPolicy = tPolicyid.ToString();
                                                            //DateTime ProcessTransDate = ProcessdateTo;//form["ProcessTransDate"] == "0" ? "" : form["ProcessTransDate"];
                                                            DateTime ProcessTransDate = Convert.ToDateTime(ReleaseDatestr);

                                                            transferActivity(ReleaseDatestr, txtNarrationRelease, NewGeoStruct, NewFuncStruct, NewPayStruct, TransActivity, TransPolicy, ProcessTransDate, inchempLoc.Id, TransferServiceBook);
                                                            // transfer  Code Start
                                                            if (Msg.Count() == 0)
                                                            {
                                                                // transfer release Code Start
                                                                var ReleaseFlag = Convert.ToBoolean("False");

                                                                if (!String.IsNullOrEmpty(SessionManager.UserName))
                                                                {
                                                                    CompId = Convert.ToInt32(Session["CompId"]);
                                                                }

                                                                Employee OEmployee = null;
                                                                EmployeePayroll OEmployeePayroll = null;

                                                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                                                 .Include(e => e.EmpOffInfo)
                                                                .Where(r => r.Id == empid).SingleOrDefault();

                                                                OEmployeePayroll
                                                                = db.EmployeePayroll.Include(e => e.TransferServiceBook)
                                                                 .Where(e => e.Employee.Id == empid).SingleOrDefault();

                                                                int TransId = OEmployeePayroll.TransferServiceBook.Where(e => e.ReleaseFlag == false && e.Narration.Contains("ONDEPUTATION")).SingleOrDefault().Id;

                                                                TransferServiceBook.ReleaseFlag = Convert.ToBoolean(ReleaseFlag);
                                                                TransferServiceBook.Narration = txtNarrationRelease;
                                                                TransferServiceBook.ReleaseDate = Convert.ToDateTime(ReleaseDatestr);

                                                                if (ReleaseFlag == false)
                                                                {
                                                                    TransferServiceBook OTransferServiceBook = db.TransferServiceBook.Where(e => e.Id == TransId).SingleOrDefault();
                                                                    Process.ServiceBook.ServiceBookProcess("",CompId, "TRANSFER_RELEASE", null, null, OTransferServiceBook.Id, null, OEmployeePayroll.Id, "TRANSFER", TransferServiceBook.ReleaseDate, false, false, 0, null);

                                                                    // Release date update todate for off deputation to date require
                                                                    using (DataBaseContext db1 = new DataBaseContext())
                                                                    {
                                                                        TransferServiceBook OTransferServiceBookRel = db1.TransferServiceBook.Find(TransId);

                                                                        OTransferServiceBookRel.ReleaseFlag = false;
                                                                        OTransferServiceBookRel.ReleaseDate = mReleaseDate;
                                                                        OTransferServiceBookRel.ProcessTransDate = ProcessdateTo;
                                                                        db1.TransferServiceBook.Attach(OTransferServiceBookRel);
                                                                        db1.Entry(OTransferServiceBookRel).State = System.Data.Entity.EntityState.Modified;
                                                                        db1.SaveChanges();
                                                                        //db.Entry(OTransferServiceBookRel).State = System.Data.Entity.EntityState.Detached;
                                                                    }
                                                                    // Incharge updattion
                                                                    string OldIncharge = empLoc.Id.ToString();
                                                                    string NewIncharge = inchempLoc.Id.ToString();
                                                                    Incharge(OldIncharge, NewIncharge);

                                                                }

                                                                // transfer release Code End

                                                            }
                                                            else
                                                            {
                                                                Msgall.Add("" + Msg[0]);
                                                            }


                                                        }
                                                    }
                                                    //  }
                                                }
                                                ts1.Complete();
                                            }
                                        }
                                        //====================================================== Process date not null

                                    }
                                    else
                                    {


                                        var Empleave = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.LvNewReq)
                                                       .Include(e => e.LvNewReq.Select(x => x.LvWFDetails))
                                                       .Include(e => e.LvNewReq.Select(z => z.Incharge))
                                                       .Include(e => e.LvNewReq.Select(z => z.WFStatus))
                                            .ToList();

                                        foreach (var eid in Empleave)
                                        {
                                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                            {
                                                var LwfProcessdata = eid.LvNewReq.Where(e => e.TrClosed == true && e.IsCancel == false && e.TrReject == false && e.WFStatus.LookupVal != "2" && e.Incharge != null && e.LeaveCalendar.Id == lvyr.Id && e.Incharge.Id == empid).ToList();
                                                var lvwfdata = LwfProcessdata.Where(e => e.LvWFDetails.Count > 0 && (e.LvWFDetails.LastOrDefault().WFStatus == 3 || e.LvWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == true).OrderByDescending(x => x.Id).ToList();
                                                foreach (var item in lvwfdata)
                                                {
                                                    int lvreqid = item.Id;


                                                    if (eid.LvNewReq.Where(e => e.Id == lvreqid).Any() == true)
                                                    {
                                                        int emptakenleave = eid.Employee.Id;
                                                        var empLoc = db.Employee.Include(e => e.GeoStruct)
                                                            .Include(e => e.PayStruct)
                                                            .Include(e => e.FuncStruct)
                                                            .Where(e => e.Id == emptakenleave).FirstOrDefault();

                                                        var inchempLoc = db.Employee.Include(e => e.GeoStruct)
                                                            .Include(e => e.PayStruct)
                                                            .Include(e => e.FuncStruct)
                                                            .Where(e => e.Id == empid).FirstOrDefault();

                                                        // transfer  Code Start
                                                        DateTime mReleaseDate = Convert.ToDateTime(item.ToDate.Value.Date);// update last in process
                                                        string ReleaseDatestr = item.FromDate.ToString(); // Release date take From date because we have require Structure
                                                        string txtNarrationRelease = lvreqid + "," + "ONDEPUTATION";

                                                        string NewGeoStruct = empLoc.GeoStruct.Id.ToString(); //form["NewGeoT-table"] == "0" ? "" : form["NewGeoT-table"];
                                                        string NewFuncStruct = inchempLoc.FuncStruct.Id.ToString(); //form["NewFuncT-table"] == "0" ? "" : form["NewFuncT-table"];
                                                        string NewPayStruct = inchempLoc.PayStruct.Id.ToString();//form["NewPayT-table"] == "0" ? "" : form["NewPayT-table"];

                                                        var qurey = db.TransActivity.Where(e => e.Name.ToUpper() == "DEPUTATION").SingleOrDefault();
                                                        int TransPolicyId = Convert.ToInt32(qurey.Id);

                                                        var OPolicyT = db.TransActivity.Where(e => e.Id == TransPolicyId)
                                                           .Include(e => e.TranPolicy)
                                                          .SingleOrDefault();
                                                        int tPolicyid = OPolicyT.TranPolicy.Id;

                                                        string TransActivity = qurey.Id.ToString();
                                                        string TransPolicy = tPolicyid.ToString();
                                                        // DateTime ProcessTransDate = ProcessdateTo;
                                                        DateTime ProcessTransDate = Convert.ToDateTime(ReleaseDatestr);

                                                        Msg = transferActivity(ReleaseDatestr, txtNarrationRelease, NewGeoStruct, NewFuncStruct, NewPayStruct, TransActivity, TransPolicy, ProcessTransDate, inchempLoc.Id, TransferServiceBook);
                                                        // transfer  Code Start
                                                        if (Msg.Count() == 0)
                                                        {
                                                            // transfer release Code Start
                                                            var ReleaseFlag = Convert.ToBoolean("False");

                                                            if (!String.IsNullOrEmpty(SessionManager.UserName))
                                                            {
                                                                CompId = Convert.ToInt32(Session["CompId"]);
                                                            }

                                                            Employee OEmployee = null;
                                                            EmployeePayroll OEmployeePayroll = null;

                                                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                                             .Include(e => e.EmpOffInfo)
                                                            .Where(r => r.Id == empid).SingleOrDefault();

                                                            OEmployeePayroll
                                                            = db.EmployeePayroll.Include(e => e.TransferServiceBook)
                                                             .Where(e => e.Employee.Id == empid).SingleOrDefault();

                                                            int TransId = OEmployeePayroll.TransferServiceBook.Where(e => e.ReleaseFlag == false && e.Narration.Contains("ONDEPUTATION")).SingleOrDefault().Id;

                                                            TransferServiceBook.ReleaseFlag = Convert.ToBoolean(ReleaseFlag);
                                                            TransferServiceBook.Narration = txtNarrationRelease;
                                                            TransferServiceBook.ReleaseDate = Convert.ToDateTime(ReleaseDatestr);

                                                            if (ReleaseFlag == false)
                                                            {
                                                                TransferServiceBook OTransferServiceBook = db.TransferServiceBook.Where(e => e.Id == TransId).SingleOrDefault();
                                                                Process.ServiceBook.ServiceBookProcess("",CompId, "TRANSFER_RELEASE", null, null, OTransferServiceBook.Id, null, OEmployeePayroll.Id, "TRANSFER", TransferServiceBook.ReleaseDate, false, false, 0, null);

                                                                // Release date update todate for off deputation to date require
                                                                using (DataBaseContext db1 = new DataBaseContext())
                                                                {
                                                                    TransferServiceBook OTransferServiceBookRel = db1.TransferServiceBook.Find(TransId);

                                                                    OTransferServiceBookRel.ReleaseFlag = false;
                                                                    OTransferServiceBookRel.ReleaseDate = mReleaseDate;
                                                                    OTransferServiceBookRel.ProcessTransDate = ProcessdateTo;
                                                                    db1.TransferServiceBook.Attach(OTransferServiceBookRel);
                                                                    db1.Entry(OTransferServiceBookRel).State = System.Data.Entity.EntityState.Modified;
                                                                    db1.SaveChanges();
                                                                    //db.Entry(OTransferServiceBookRel).State = System.Data.Entity.EntityState.Detached;
                                                                }
                                                                // Incharge updattion
                                                                string OldIncharge = empLoc.Id.ToString();
                                                                string NewIncharge = inchempLoc.Id.ToString();
                                                                Incharge(OldIncharge, NewIncharge);


                                                            }

                                                            // transfer release Code End

                                                        }
                                                        else
                                                        {
                                                            Msgall.Add("" + Msg[0]);
                                                        }


                                                    }
                                                    //  }
                                                }
                                                ts.Complete();
                                            }


                                        }





                                    }
                                    //   ts.Complete();
                                    if (Msgall.Count() == 0)
                                    {
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = sid, Val = sfull, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                    else
                                    {
                                        Msgall.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = sid, Val = sfull, success = true, responseText = Msgall }, JsonRequestBehavior.AllowGet);

                                    }


                                }
                            }
                            if (WorkingLocation == "true")
                            {
                                int sid = 0;
                                string sfull = null;
                                foreach (var a in EmpId)
                                {
                                    using (TransactionScope ts3 = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        int empid = Convert.ToInt32(a);
                                        //==========Deputaion off actvity start
                                        var ReleaseFlag = Convert.ToBoolean("False");

                                        if (!String.IsNullOrEmpty(SessionManager.UserName))
                                        {
                                            CompId = Convert.ToInt32(Session["CompId"]);
                                        }

                                        Employee OEmployee = null;
                                        EmployeePayroll OEmployeePayroll = null;

                                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                         .Include(e => e.EmpOffInfo)
                                        .Where(r => r.Id == empid).SingleOrDefault();

                                        OEmployeePayroll
                                        = db.EmployeePayroll
                                        .Include(e => e.TransferServiceBook)
                                        .Include(e => e.TransferServiceBook.Select(x => x.OldGeoStruct))
                                         .Where(e => e.Employee.Id == empid).SingleOrDefault();

                                        var EmpDeputation = OEmployeePayroll.TransferServiceBook.Where(e => e.ReleaseFlag == false && e.Narration.Contains("ONDEPUTATION")).SingleOrDefault();

                                        var items = EmpDeputation.Narration.Split(',');
                                        string lvnewreqid = items[0];
                                        string Narr = items[1];
                                        int reqid = Convert.ToInt32(lvnewreqid);


                                        string ReleaseDatestr = EmpDeputation.ReleaseDate.Value.AddDays(1).ToString(); // Release date take From date because we have require Structure
                                        string txtNarrationRelease = reqid + "," + "OFFDEPUTATION";

                                        string NewGeoStruct = EmpDeputation.OldGeoStruct.Id.ToString(); //form["NewGeoT-table"] == "0" ? "" : form["NewGeoT-table"];
                                        string NewFuncStruct = OEmployee.FuncStruct.Id.ToString(); //form["NewFuncT-table"] == "0" ? "" : form["NewFuncT-table"];
                                        string NewPayStruct = OEmployee.PayStruct.Id.ToString();//form["NewPayT-table"] == "0" ? "" : form["NewPayT-table"];

                                        var qurey = db.TransActivity.Where(e => e.Name.ToUpper() == "DEPUTATION").SingleOrDefault();
                                        int TransPolicyId = Convert.ToInt32(qurey.Id);

                                        var OPolicyT = db.TransActivity.Where(e => e.Id == TransPolicyId)
                                           .Include(e => e.TranPolicy)
                                          .SingleOrDefault();
                                        int tPolicyid = OPolicyT.TranPolicy.Id;

                                        string TransActivity = qurey.Id.ToString();
                                        string TransPolicy = tPolicyid.ToString();

                                        DateTime ProcessTransDate = Convert.ToDateTime(ReleaseDatestr);

                                        Msg = transferActivity(ReleaseDatestr, txtNarrationRelease, NewGeoStruct, NewFuncStruct, NewPayStruct, TransActivity, TransPolicy, ProcessTransDate, OEmployee.Id, TransferServiceBook);

                                        //==========Deputaion off actvity End
                                        if (Msg.Count() == 0)
                                        {
                                            // transfer release Code Start

                                            if (!String.IsNullOrEmpty(SessionManager.UserName))
                                            {
                                                CompId = Convert.ToInt32(Session["CompId"]);
                                            }

                                            var ReleaseFlagoff = Convert.ToBoolean("True");

                                            Employee OEmployeeoff = null;
                                            EmployeePayroll OEmployeePayrolloff = null;

                                            OEmployeeoff = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                             .Include(e => e.EmpOffInfo)
                                            .Where(r => r.Id == empid).SingleOrDefault();

                                            OEmployeePayrolloff
                                            = db.EmployeePayroll
                                            .Include(e => e.TransferServiceBook)
                                            .Include(e => e.TransferServiceBook.Select(x => x.OldGeoStruct))
                                             .Where(e => e.Employee.Id == empid).SingleOrDefault();

                                            int TransId = OEmployeePayrolloff.TransferServiceBook.Where(e => e.ReleaseFlag == false && e.Narration.Contains("OFFDEPUTATION")).SingleOrDefault().Id;

                                            TransferServiceBook.ReleaseFlag = Convert.ToBoolean(ReleaseFlagoff);
                                            TransferServiceBook.Narration = txtNarrationRelease;
                                            TransferServiceBook.ReleaseDate = Convert.ToDateTime(ReleaseDatestr);

                                            if (ReleaseFlagoff != false)
                                            {
                                                TransferServiceBook OTransferServiceBook = db.TransferServiceBook.Where(e => e.Id == TransId).SingleOrDefault();
                                                Process.ServiceBook.ServiceBookProcess("",CompId, "TRANSFER_RELEASE", null, null, OTransferServiceBook.Id, null, OEmployeePayroll.Id, "TRANSFER", TransferServiceBook.ReleaseDate, false, false, 0, null);

                                                // Release date update todate for off deputation to date require
                                                using (DataBaseContext db1 = new DataBaseContext())
                                                {
                                                    TransferServiceBook OTransferServiceBookRel = db1.TransferServiceBook.Find(TransId);

                                                    OTransferServiceBookRel.ReleaseFlag = true;
                                                    OTransferServiceBookRel.ReleaseDate = EmpDeputation.ReleaseDate;
                                                    OTransferServiceBookRel.ProcessTransDate = EmpDeputation.ProcessTransDate;
                                                    db1.TransferServiceBook.Attach(OTransferServiceBookRel);
                                                    db1.Entry(OTransferServiceBookRel).State = System.Data.Entity.EntityState.Modified;
                                                    db1.SaveChanges();
                                                    //db.Entry(OTransferServiceBookRel).State = System.Data.Entity.EntityState.Detached;
                                                }
                                                //oon depotation flag true
                                                EmpDeputation.ReleaseFlag = true;
                                                db.TransferServiceBook.Attach(EmpDeputation);
                                                db.Entry(EmpDeputation).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();

                                                // Incharge updattion
                                                var Empleave = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.LvNewReq)
                                                  .Include(e => e.LvNewReq.Select(z => z.Incharge))
                                                   .ToList();

                                                foreach (var eid in Empleave)
                                                {
                                                    var LwfProcessdata = eid.LvNewReq.Where(e => e.Id == reqid).FirstOrDefault();
                                                    if (LwfProcessdata != null)
                                                    {
                                                        int emptakenleave = eid.Employee.Id;
                                                        string OldIncharge = empid.ToString();
                                                        string NewIncharge = emptakenleave.ToString();
                                                        Incharge(OldIncharge, NewIncharge);
                                                        break;
                                                    }

                                                   
                                                }




                                            }

                                            // transfer release Code End

                                        }
                                        else
                                        {
                                            Msgall.Add("" + Msg[0]);
                                        }
                                        ts3.Complete();
                                    }


                                }

                                if (Msgall.Count() == 0)
                                {
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = sid, Val = sfull, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    Msgall.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = sid, Val = sfull, success = true, responseText = Msgall }, JsonRequestBehavior.AllowGet);

                                }

                            }
                            if (WorkingLocation == "false")
                            {
                                Msg.Add("To make Ultimate Applicable Please Go to menu User Accountablity");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (TransferServiceBook)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var databaseValues = (TransferServiceBook)databaseEntry.ToObject();
                                TransferServiceBook.RowVersion = databaseValues.RowVersion;
                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }

                return View();
            }

        }

        public ActionResult Get_Deputation(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var compid = Convert.ToInt32(Session["CompId"].ToString());
                    var ab = db.CompanyPayroll.Where(e => e.Company.Id == compid)
                     .Include(e => e.EmployeePayroll)
                     .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                     .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                     .Include(e => e.EmployeePayroll.Select(a => a.Employee.Login))
                     .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))

                     .AsNoTracking().AsParallel()
                     .SingleOrDefault();

                    var db_data = ab.EmployeePayroll.Where(e => e.Employee.Id == data).Select(e => e.Employee).SingleOrDefault();



                    if (db_data != null)
                    {
                        List<LoanAdvReqChildDataClass> returndata = new List<LoanAdvReqChildDataClass>();



                        if (db_data.ServiceBookDates.ServiceLastDate == null && db_data.Login != null)
                        {

                            returndata.Add(new LoanAdvReqChildDataClass
                            {
                                Id = db_data.Id,
                                UserId = db_data.Login.UserId,
                                IsUltimateAppl = db_data.Login.IsUltimateHOAppl.ToString(),
                                IsActive = db_data.Login.IsActive.ToString(),

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


        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class LoanAdvReqChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string UserId { get; set; }
            public string IsUltimateAppl { get; set; }
            public string IsActive { get; set; }

        }



        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }

            public string BatchName { get; set; }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                try
                {
                    var compid = Convert.ToInt32(Session["CompId"].ToString());
                    var FilterEmployee = db.Employee
                      .Include(e => e.Login)
                      .Include(e => e.EmpName).AsNoTracking().AsParallel()
                      .ToList();


                    var all = FilterEmployee;
                    //for searchs
                    IEnumerable<Employee> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.EmpCode.ToString().Contains(param.sSearch))
                                  || (e.EmpName.FullNameFML.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<Employee, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.EmpCode : "");
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
                                 EmpCode = item.EmpCode,
                                 EmpName = item.EmpName.FullNameFML,

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

                                     select new[] { null, Convert.ToString(c.Id), c.EmpCode };
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


        public class DeserializeClass
        {
            public string Id { get; set; }
            public string BatchName { get; set; }
            public bool IsPresent { get; set; }
            public bool IsCancelled { get; set; }
            public string CancelReason { get; set; }

        }

        public ActionResult Get_AppAssignData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeTraining
                        .Include(e => e.TrainingDetails)
                        //.Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion))
                        //.Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating)))
                        //.Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment))))
                        //.Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppCategory))))
                        //.Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppSubCategory))))
                        //.Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppRatingObjective))))
                        //.Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppRatingObjective.Select(q => q.ObjectiveWordings)))))
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data.TrainingDetails != null)
                    {
                        List<DeserializeClass> returndata = new List<DeserializeClass>();

                        foreach (var item in db_data.TrainingDetails)
                        {

                            // var obj = item2.AppAssignment.AppRatingObjective.Select(e => e.ObjectiveWordings.LookupVal.ToString()).SingleOrDefault();


                            returndata.Add(new DeserializeClass
                            {
                                //Id = item.Id.ToString(),
                                //BatchName = item.BatchName,
                                //IsPresent = item.IsPresent,
                                //IsCancelled = item.IsCancelled,
                                //CancelReason = item.CancelReason

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
                IEnumerable<TransferServiceBook> Venue = null;
                if (gp.IsAutho == true)
                {
                    Venue = db.TransferServiceBook.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Venue = db.TransferServiceBook.AsNoTracking().ToList();
                }

                IEnumerable<TransferServiceBook> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Venue;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id }).Where((e => (e.Id.ToString() == gp.searchString))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Fees), Convert.ToString(a.Name), Convert.ToString(a.VenuType) != null ? Convert.ToString(a.VenuType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Venue;
                    Func<TransferServiceBook, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }

                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id }).ToList();
                    }
                    totalRecords = Venue.Count();
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

        public class EditData
        {
            public int Id { get; set; }
            public Employee Employee { get; set; }

            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string Editable { get; set; }
            public string ProgranList { get; set; }
            public bool present { get; set; }
            public bool Cancel { get; set; }
            public string CancelReason { get; set; }
        }





        public ActionResult ChkProcess(string typeofbtn, string month, string EmpCode)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool selected = false;
                var query1 = db.EmployeeTraining.Include(e => e.Employee).Include(e => e.TrainingDetails).Include(e => e.TrainingDetails.Select(q => q.TrainingSchedule)).Where(e => e.Employee.EmpCode == EmpCode).Select(e => e.TrainingDetails.Select(x => x.TrainingSchedule)).SingleOrDefault();
                var query = query1.Where(e => e.TrainingBatchName == month).ToList();

                if (query.Count > 0)
                {
                    selected = true;
                }
                var data = new
                {
                    status = selected,
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var LvEP = db.TrainigDetailSessionInfo.Find(data);
                db.TrainigDetailSessionInfo.Remove(LvEP);
                db.SaveChanges();
                List<string> Msgs = new List<string>();
                Msgs.Add("Record Deleted Successfully ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

            }
        }


    }
}