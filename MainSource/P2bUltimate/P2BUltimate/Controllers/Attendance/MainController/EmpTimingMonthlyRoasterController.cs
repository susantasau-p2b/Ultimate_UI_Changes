using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Threading.Tasks;
using Attendance;
using System.Web.Script.Serialization;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using P2BUltimate.Security;
using P2BUltimate.Process;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using P2B.UTILS;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Configuration;

namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class EmpTimingMonthlyRoasterController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/EmpTimingMonthlyRoaster/Index.cshtml");
        }
        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Attendance/_EmpTimingMonthlyRoasterGridPartial.cshtml");
        }
        public ActionResult GetTimingMonRosterDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TimingMonthlyRoaster.Include(e => e.TimingGroup).ToList();
                IEnumerable<TimingMonthlyRoaster> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TimingMonthlyRoaster.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    var list1 = db.EmpTimingMonthlyRoaster.Include(e => e.TimingMonthlyRoaster.TimingGroup).ToList().Select(e => e.TimingMonthlyRoaster);
                    var list2 = fall.Except(list1);
                    var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }
        public ActionResult GetTimingPolicyDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TimingPolicy.ToList();
                IEnumerable<TimingGroup> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TimingGroup.ToList().Where(d => d.GroupName.Contains(data));

                }
                else
                {
                    //var list1 = db.TimingGroup.ToList().SelectMany(e => e.TimingPolicy);
                    //var list2 = fall.Except(list1);
                    //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                    //var list1 = db.TimingGroup.Include(e => e.TimingPolicy).SelectMany(e => e.TimingPolicy);
                    //var list2 = fall.Except(list1);
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.GroupName }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
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
                IEnumerable<EmpTimingMonthlyRoaster> timingMonthlyRoaster = null;
                if (gp.IsAutho == true)
                {
                    timingMonthlyRoaster = db.EmpTimingMonthlyRoaster.Include(e => e.TimingMonthlyRoaster).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    timingMonthlyRoaster = db.EmpTimingMonthlyRoaster.Include(e => e.TimingMonthlyRoaster).AsNoTracking().ToList();
                }

                IEnumerable<EmpTimingMonthlyRoaster> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = timingMonthlyRoaster;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.RoasterDate }).Where((e => (e.Id.ToString() == gp.searchString) || (e.RoasterDate.ToString() == gp.searchString.ToString()))).ToList();

                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.RoasterDate }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = timingMonthlyRoaster;
                    Func<EmpTimingMonthlyRoaster, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "RoasterDate" ? c.RoasterDate.Value.ToShortDateString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.RoasterDate.Value.ToShortDateString() }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.RoasterDate.Value.ToShortDateString() }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.RoasterDate.Value.ToShortDateString() }).ToList();
                    }
                    totalRecords = timingMonthlyRoaster.Count();
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
        public class ETRM_RoasterProcess
        {
            public List<int> Emp_Ids { get; set; }
            public DateTime? FromPeriod { get; set; }
            public DateTime? ToPeriod { get; set; }

        }

        public class ServiceResultList<T>
        {
            /// <summary>
            /// 
            /// </summary>
            public HttpStatusCode MessageCode { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Message { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<T> Data { get; set; }
        }


        public class ReturnData_RoasterProcess
        {
            public int? ErrNo { get; set; }
            public int? InfoNo { get; set; }
            public string EmpCode { get; set; }
            public string ErrMsg { get; set; }

        }

        public ActionResult Create(EmpTimingMonthlyRoaster c, FormCollection form) //Create submit
        {
            List<string> ErrMsg = new List<string>();
            List<string> SucMsg = new List<string>();
            try
            {
                //string _PayMonth = form["PayMonth"] != null ? form["PayMonth"] : string.Empty;
                string PeriodFrom = form["PeriodFrom"] != null ? form["PeriodFrom"] : string.Empty;
                string PeriodTo = form["PeriodTo"] != null ? form["PeriodTo"] : string.Empty;
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                List<int> ids = null;
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    ids = Utility.StringIdsToListIds(Emp);
                }
                else
                {
                    ErrMsg.Add(" Kindly select employee  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);
                }

                if (PeriodFrom != null && PeriodFrom != "" && PeriodTo != null && PeriodTo != "")
                {

                    if (Convert.ToDateTime(PeriodTo) < Convert.ToDateTime(PeriodFrom))
                    {
                        ErrMsg.Add(" To date Should be grater than from date  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    ErrMsg.Add(" Please Select From date and To Date  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);
                }
               
                if (ModelState.IsValid)
                {
                    c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    var _PeriodFrom = Convert.ToDateTime(PeriodFrom);
                    var _PeriodTo = Convert.ToDateTime(PeriodTo);

                    ////////// ====================================== Api call Start ==============================================

                    ReturnData_RoasterProcess returnDATA = new ReturnData_RoasterProcess();

                    var ShowMessageCode = "";
                    var ShowMessage = "";
                    var ShowErrorMessage = "";

                    ServiceResultList<ReturnData_RoasterProcess> responseDeserializeData = new ServiceResultList<ReturnData_RoasterProcess>();
                    string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                    using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                    {
                        var response = p2BHttpClient.request("ETRM/getUserAttendanceRoasterProcessRequest",
                            new ETRM_RoasterProcess() { Emp_Ids = ids, FromPeriod = _PeriodFrom, ToPeriod = _PeriodTo });

                        var data = response.Content.ReadAsStringAsync().Result;
                        // var result = data.;

                        responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResultList<ReturnData_RoasterProcess>>(response.Content.ReadAsStringAsync().Result);


                        ShowMessageCode = responseDeserializeData.MessageCode.ToString();
                        ShowMessage = responseDeserializeData.Message.ToString();

                        foreach (var item in responseDeserializeData.Data)
                        {
                            ShowErrorMessage = item.ErrMsg.ToString();
                        }

                        if (responseDeserializeData != null && ShowMessageCode != "OK")
                        {
                            using (DataBaseContext db = new DataBaseContext())
                            {
                                if (responseDeserializeData.Data.Count() > 0)
                                {


                                    string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\LogFile";
                                    //System.Web.HttpContext.Current.Server.MapPath("\\JVFile");
                                    string localPath = new Uri(requiredPath).LocalPath;
                                    if (!System.IO.Directory.Exists(localPath))
                                    {
                                        localPath = new Uri(requiredPath).LocalPath;
                                        System.IO.Directory.CreateDirectory(localPath);
                                    }
                                    string path = requiredPath + @"\MAN_ROASTER_" + Convert.ToDateTime(DateTime.Now.Date).ToString("ddMMyyyy") + ".txt";
                                    //System.Web.HttpContext.Current.Server.MapPath("\\JVFile")
                                    path = new Uri(path).LocalPath;
                                    if (System.IO.File.Exists(path))
                                    {
                                        System.IO.File.Delete(path);
                                    }

                                    using (var fs = new FileStream(path, System.IO.FileMode.OpenOrCreate))
                                    {
                                        StreamWriter str = new StreamWriter(fs);
                                        str.BaseStream.Seek(0, SeekOrigin.Begin);
                                        foreach (var item in responseDeserializeData.Data)
                                        {
                                            string empid = item.EmpCode;
                                            int eid = Convert.ToInt32(empid);
                                            var oemployee = db.Employee.Include(e => e.EmpName).Where(e => e.Id == eid).FirstOrDefault();

                                            str.WriteLine(oemployee.EmpCode.ToString() + ' ' + oemployee.EmpName.FullNameFML.ToString() + ' '
                                             + item.ErrNo + ' ' + item.ErrMsg + ' ' + DateTime.Now);

                                        }
                                        str.Flush();
                                        str.Close();
                                        fs.Close();
                                    }
                                    System.Diagnostics.Process.Start("notepad.exe", path);

                                }
                            }
                        }




                    }


                    // ====================================== Api call End ==============================================

                    // =============================== Ultimate Call Start ============================================
                    //string ErrMsgL = "";
                    //string SuccessMsg = "";
                    //List<AttendanceProcess.ReturnData_AttendanceProcess> msglist = new List<AttendanceProcess.ReturnData_AttendanceProcess>();

                    //msglist = AttendanceProcess.Generate_Monthly_Roaster(_EmpIds: ids, _PeriodFrom: _PeriodFrom,
                    //        _PeriodTo: _PeriodTo);
                    // =============================== Ultimate Call End ============================================

                    //AttendanceProcess.Genrate_Monthly_Roaster(_CompId: Convert.ToInt32(SessionManager.CompanyId), _EmpIds: ids, _PeriodFrom: _PeriodFrom,
                    //        _PeriodTo: _PeriodTo);

                    //AttendanceProcess.Monthly_Roaster_check(_CompId: Convert.ToInt32(SessionManager.CompanyId), _EmpIds: ids, _PeriodFrom: _PeriodFrom,
                    //       _PeriodTo: _PeriodTo);
                    try
                    {
                        // ====================================== Api call Start ==============================================
                        if (responseDeserializeData == null && ShowMessageCode == "OK")
                        {
                            ErrMsg.Add(ShowMessage);
                            return Json(new Utility.JsonReturnClass { success = true, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            ErrMsg.Add(ShowMessage + " ,  " + ShowErrorMessage);
                            return Json(new Utility.JsonReturnClass { success = true, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);
                        }

                        // ====================================== Api call End ==============================================


                        // =============================== Ultimate Call Start ============================================
                        //if (msglist.Count() > 0)
                        //{
                        //    foreach (var item in msglist)
                        //    {
                        //        if (item.ErrNo != 0)
                        //        {
                        //            using (DataBaseContext db = new DataBaseContext())
                        //            {
                        //                ErrMsgL = item.EmpCode + " - " + db.ErrorLookup.Where(e => (e.Message_Code == item.ErrNo && item.ErrNo > 0)).Select(e => e.Message_Description).FirstOrDefault();
                        //            }
                        //            ErrMsg.Add(ErrMsgL);
                        //        }
                        //        else
                        //        {
                        //            using (DataBaseContext db = new DataBaseContext())
                        //            {
                        //                SuccessMsg = item.EmpCode + " - " + db.ErrorLookup.Where(e => e.Info_code == item.InfoNo && item.InfoNo > 0).Select(e => e.Message_Description).FirstOrDefault();
                        //            }
                        //            SucMsg.Add(SuccessMsg);

                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    SucMsg.Add("  Data Saved successfully  ");
                        //    return Json(new Utility.JsonReturnClass { success = true, responseText = SucMsg }, JsonRequestBehavior.AllowGet);
                        //}

                        //if (ErrMsg.Count() > 0)
                        //{
                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);
                        //}
                        //else
                        //{
                        //    return Json(new Utility.JsonReturnClass { success = true, responseText = SucMsg }, JsonRequestBehavior.AllowGet);
                        //}
                        //return Json(new Utility.JsonReturnClass { success = true, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);

                        // =============================== Ultimate Call End ============================================
                       

                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                    }
                    catch (DataException /* dex */)
                    {
                        // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                        ErrMsg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);

                    }
                    // }
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
                    var errorMsg = sb.ToString();
                    return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                    //return this.Json(new { msg = errorMsg });
                }

            }
            catch (Exception ex)
            {
                throw;
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
                ErrMsg.Add(ex.Message);
                return Json(new Utility.JsonReturnClass { success = false, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {


                try
                {

                    EmpTimingMonthlyRoaster corporates = db.EmpTimingMonthlyRoaster.Include(e => e.TimingMonthlyRoaster)
                                                       .Include(e => e.TimingPolicy)
                                                       .Include(e => e.DayType).Where(e => e.Id == data).SingleOrDefault();

                    TimingMonthlyRoaster add = corporates.TimingMonthlyRoaster;
                    TimingPolicy conDet = corporates.TimingPolicy;
                    LookupValue val = corporates.DayType;
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            corporates.DBTrack = dbT;
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, corporates.DBTrack);
                            DT_EmpTimingMonthlyRoaster DT_Corp = (DT_EmpTimingMonthlyRoaster)rtn_Obj;
                            DT_Corp.TimingMonthlyRoaster_Id = corporates.TimingMonthlyRoaster == null ? 0 : corporates.TimingMonthlyRoaster.Id;
                            DT_Corp.DayType_Id = corporates.DayType == null ? 0 : corporates.DayType.Id;
                            DT_Corp.TimingPolicy_Id = corporates.TimingPolicy == null ? 0 : corporates.TimingPolicy.Id;
                            db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                        }
                    }
                    else
                    {
                        //var selectedRegions = corporates.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(corporates.Regions.Select(e => e.Id));
                            //    if (corpRegion.Count > 0)
                            //    {
                            //        return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    }
                            //}

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                    CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                    IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, dbT);
                                DT_EmpTimingMonthlyRoaster DT_Corp = (DT_EmpTimingMonthlyRoaster)rtn_Obj;
                                DT_Corp.TimingMonthlyRoaster_Id = add == null ? 0 : add.Id;
                                DT_Corp.DayType_Id = val == null ? 0 : val.Id;
                                DT_Corp.TimingPolicy_Id = conDet == null ? 0 : conDet.Id;
                                db.Create(DT_Corp);

                                await db.SaveChangesAsync();


                                //using (var context = new DataBaseContext())
                                //{
                                //    corporates.Address = add;
                                //    corporates.ContactDetails = conDet;
                                //    corporates.BusinessType = val;
                                //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
                                //}
                                ts.Complete();
                                // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
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
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.EmpTimingMonthlyRoaster
                    .Include(e => e.TimingMonthlyRoaster)
                    .Include(e => e.TimingPolicy)
                    .Include(e => e.DayType)
                    //    .Include(e => e.EmployeeID)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        RoasterDate = e.RoasterDate,
                        DayType_Id = e.DayType.Id == null ? 0 : e.DayType.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.EmpTimingMonthlyRoaster
                  .Include(e => e.TimingMonthlyRoaster)
                    .Include(e => e.TimingPolicy)
                    .Include(e => e.DayType)
                    //   .Include(e => e.EmployeeID)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        TimingMonthlyRoaster_FullDetails = e.TimingMonthlyRoaster.Id == null ? "" : e.TimingMonthlyRoaster.FullDetails,
                        TimingMonthlyRoaster_Id = e.TimingMonthlyRoaster.Id == null ? "" : e.TimingMonthlyRoaster.Id.ToString(),
                        TimingPolicy_Id = e.TimingPolicy.Id == null ? "" : e.TimingPolicy.Id.ToString(),
                        TimingPolicy_FullDetails = e.TimingPolicy.FullDetails == null ? "" : e.TimingPolicy.FullDetails
                        //Employee_Id = e.EmployeeID.Id == null ? "" : e.EmployeeID.Id.ToString(),
                        //Employee_FullDetails = e.EmployeeID.EmpName == null ? "" : e.EmployeeID.EmpName.ToString()
                    }).ToList();


                var W = db.DT_EmpTimingMonthlyRoaster
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         RoasterDate = e.RoasterDate,
                         DayType_Val = e.DayType_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.DayType_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),

                         TimingMonthlyRoaster_Val = e.TimingMonthlyRoaster_Id == 0 ? "" : db.TimingMonthlyRoaster.Where(x => x.Id == e.TimingMonthlyRoaster_Id).Select(x => x.RoasterName).FirstOrDefault(),
                         TimingPolicy_Val = e.TimingPolicy_Id == 0 ? "" : db.TimingPolicy.Where(x => x.Id == e.TimingPolicy_Id).Select(x => x.FullDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.EmpTimingMonthlyRoaster.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }
        public int EditS(/*string Corp,*/ string Addrs, string ContactDetails, int data, EmpTimingMonthlyRoaster c, DBTrack dbT)
        {
            //if (Corp != null)
            //{
            //    if (Corp != "")
            //    {
            //        var val = db.LookupValue.Find(int.Parse(Corp));
            //        c.DayType = val;

            //        var type = db.EmpTimingMonthlyRoaster.Include(e => e.DayType).Where(e => e.Id == data).SingleOrDefault();
            //        IList<EmpTimingMonthlyRoaster> typedetails = null;
            //        if (type.DayType != null)
            //        {
            //            typedetails = db.EmpTimingMonthlyRoaster.Where(x => x.DayType.Id == type.DayType.Id && x.Id == data).ToList();
            //        }
            //        else
            //        {
            //            typedetails = db.EmpTimingMonthlyRoaster.Where(x => x.Id == data).ToList();
            //        }
            //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
            //        foreach (var s in typedetails)
            //        {
            //            s.DayType = c.DayType;
            //            db.EmpTimingMonthlyRoaster.Attach(s);
            //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //            //await db.SaveChangesAsync();
            //            db.SaveChanges();
            //            TempData["RowVersion"] = s.RowVersion;
            //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //        }
            //    }
            //    else
            //    {
            //        var BusiTypeDetails = db.EmpTimingMonthlyRoaster.Include(e => e.DayType).Where(x => x.Id == data).ToList();
            //        foreach (var s in BusiTypeDetails)
            //        {
            //            s.DayType = null;
            //            db.EmpTimingMonthlyRoaster.Attach(s);
            //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //            //await db.SaveChangesAsync();
            //            db.SaveChanges();
            //            TempData["RowVersion"] = s.RowVersion;
            //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //        }
            //    }
            //}
            //else
            //{
            //    var BusiTypeDetails = db.EmpTimingMonthlyRoaster.Include(e => e.DayType).Where(x => x.Id == data).ToList();
            //    foreach (var s in BusiTypeDetails)
            //    {
            //        s.DayType = null;
            //        db.EmpTimingMonthlyRoaster.Attach(s);
            //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
            //        //await db.SaveChangesAsync();
            //        db.SaveChanges();
            //        TempData["RowVersion"] = s.RowVersion;
            //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
            //    }
            //}
            using (DataBaseContext db = new DataBaseContext())
            {

                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.TimingMonthlyRoaster.Find(int.Parse(Addrs));
                        c.TimingMonthlyRoaster = val;

                        var add = db.EmpTimingMonthlyRoaster.Include(e => e.TimingMonthlyRoaster).Where(e => e.Id == data).SingleOrDefault();
                        IList<EmpTimingMonthlyRoaster> addressdetails = null;
                        if (add.TimingMonthlyRoaster != null)
                        {
                            addressdetails = db.EmpTimingMonthlyRoaster.Where(x => x.TimingMonthlyRoaster.Id == add.TimingMonthlyRoaster.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.EmpTimingMonthlyRoaster.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.TimingMonthlyRoaster = c.TimingMonthlyRoaster;
                                db.EmpTimingMonthlyRoaster.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                // await db.SaveChangesAsync(false);
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var addressdetails = db.EmpTimingMonthlyRoaster.Include(e => e.TimingMonthlyRoaster).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.TimingMonthlyRoaster = null;
                        db.EmpTimingMonthlyRoaster.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (ContactDetails != null)
                {
                    if (ContactDetails != "")
                    {
                        var val = db.TimingPolicy.Find(int.Parse(ContactDetails));
                        c.TimingPolicy = val;

                        var add = db.EmpTimingMonthlyRoaster.Include(e => e.TimingPolicy).Where(e => e.Id == data).SingleOrDefault();
                        IList<EmpTimingMonthlyRoaster> contactsdetails = null;
                        if (add.TimingPolicy != null)
                        {
                            contactsdetails = db.EmpTimingMonthlyRoaster.Where(x => x.TimingPolicy.Id == add.TimingPolicy.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.EmpTimingMonthlyRoaster.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.TimingPolicy = c.TimingPolicy;
                            db.EmpTimingMonthlyRoaster.Attach(s);
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
                    var contactsdetails = db.EmpTimingMonthlyRoaster.Include(e => e.TimingPolicy).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.TimingPolicy = null;
                        db.EmpTimingMonthlyRoaster.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.EmpTimingMonthlyRoaster.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    EmpTimingMonthlyRoaster corp = new EmpTimingMonthlyRoaster()
                    {
                        DayType = c.DayType,
                        RoasterDate = c.RoasterDate,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.EmpTimingMonthlyRoaster.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
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
        public class EmpTimeChildDataClass
        {
            public int Id { get; set; }
            public string RoasterDate { get; set; }
            public string TimingPolicy { get; set; }
            public string DayType { get; set; }
        }
        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.EmployeeAttendance.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                                                .Where(e => e.Employee != null && e.EmpTimingMonthlyRoaster.Count > 0)

                        //.Include(e => e.Employee.ServiceBookDates)
                        //.Include(e => e.Employee.GeoStruct)
                        //.Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        //.Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                        .ToList();
                    // for searchs
                    IEnumerable<EmployeeAttendance> fall;

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

                    Func<EmployeeAttendance, string> orderfunc = (c =>
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
                                //JoiningDate = item.Employee.ServiceBookDates != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                                //Job = item.Employee.FuncStruct != null ? item.Employee.FuncStruct.Job.Name : null,
                                //Grade = item.Employee.PayStruct != null ? item.Employee.PayStruct.Grade.Name : null,
                                //Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null,
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
        public ActionResult Get_EmpTimingMonthlyRoasterData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeAttendance.Include(e => e.EmpTimingMonthlyRoaster).Include(e => e.EmpTimingMonthlyRoaster.Select(q => q.TimingPolicy))
                        .Include(e => e.EmpTimingMonthlyRoaster.Select(q => q.DayType)).Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<EmpTimeChildDataClass> returndata = new List<EmpTimeChildDataClass>();
                        foreach (var item in db_data.EmpTimingMonthlyRoaster.ToList())
                        {
                            //if (item.IsCancel == false)
                            //{
                            returndata.Add(new EmpTimeChildDataClass
                            {
                                Id = item.Id,
                                RoasterDate = item.RoasterDate != null ? item.RoasterDate.Value.ToString("dd/MM/yyyy") : "",
                                TimingPolicy = item.TimingPolicy != null ? item.TimingPolicy.FullDetails : "",
                                DayType = item.DayType != null ? item.DayType.LookupVal.ToString() : "",
                            });
                            // }
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
        public async Task<ActionResult> EditSave(EmpTimingMonthlyRoaster c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {

                try
                {
                    // string Corp = form["DayTypelist"] == "0" ? "" : form["DayTypelist"];
                    string Addrs = form["TimingMonthlyRoasterlist"] == "0" ? "" : form["TimingMonthlyRoasterlist"];
                    string ContactDetails = form["TimingPolicylist1"] == "0" ? "" : form["TimingPolicylist1"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    //if (Corp != null)
                    //{
                    //    if (Corp != "")
                    //    {
                    //        var val = db.LookupValue.Find(int.Parse(Corp));
                    //        c.DayType = val;
                    //    }
                    //}

                    if (Addrs != null)
                    {
                        if (Addrs != "")
                        {
                            int AddId = Convert.ToInt32(Addrs);
                            var val = db.TimingMonthlyRoaster.Include(e => e.TimingGroup)
                                                .Where(e => e.Id == AddId).SingleOrDefault();
                            c.TimingMonthlyRoaster = val;
                        }
                    }

                    if (ContactDetails != null)
                    {
                        if (ContactDetails != "")
                        {
                            int ContId = Convert.ToInt32(ContactDetails);
                            var val = db.TimingPolicy.Where(e => e.Id == ContId).SingleOrDefault();
                            c.TimingPolicy = val;
                        }
                    }


                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    EmpTimingMonthlyRoaster blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.EmpTimingMonthlyRoaster.Where(e => e.Id == data).Include(e => e.DayType)
                                                                .Include(e => e.TimingMonthlyRoaster)
                                                                .Include(e => e.TimingPolicy).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    c.DayType = blog.DayType;
                                    c.RoasterDate = blog.RoasterDate;
                                    //int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);
                                    int a = EditS(Addrs, ContactDetails, data, c, c.DBTrack);


                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Attendance/Attendance", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_EmpTimingMonthlyRoaster DT_Corp = (DT_EmpTimingMonthlyRoaster)obj;
                                        DT_Corp.TimingMonthlyRoaster_Id = blog.TimingMonthlyRoaster == null ? 0 : blog.TimingMonthlyRoaster.Id;
                                        DT_Corp.TimingPolicy_Id = blog.TimingPolicy == null ? 0 : blog.TimingPolicy.Id;
                                        DT_Corp.DayType_Id = blog.DayType == null ? 0 : blog.DayType.Id;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();


                                    //return Json(new Object[] { c.Id, c.RoasterDate, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.RoasterDate.Value.ToShortDateString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (EmpTimingMonthlyRoaster)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (EmpTimingMonthlyRoaster)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //   return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            EmpTimingMonthlyRoaster blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            EmpTimingMonthlyRoaster Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.EmpTimingMonthlyRoaster.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            EmpTimingMonthlyRoaster corp = new EmpTimingMonthlyRoaster()
                            {
                                RoasterDate = c.RoasterDate,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Attendance/Attendance", "M", blog, corp, "EmpTimingMonthlyRoaster", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.EmpTimingMonthlyRoaster.Where(e => e.Id == data).Include(e => e.DayType)
                                    .Include(e => e.TimingMonthlyRoaster).Include(e => e.TimingPolicy).SingleOrDefault();
                                DT_EmpTimingMonthlyRoaster DT_Corp = (DT_EmpTimingMonthlyRoaster)obj;
                                DT_Corp.TimingMonthlyRoaster_Id = DBTrackFile.ValCompare(Old_Corp.TimingMonthlyRoaster, c.TimingMonthlyRoaster);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                DT_Corp.DayType_Id = DBTrackFile.ValCompare(Old_Corp.DayType, c.DayType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                DT_Corp.TimingPolicy_Id = DBTrackFile.ValCompare(Old_Corp.TimingPolicy, c.TimingPolicy); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.EmpTimingMonthlyRoaster.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            //return Json(new Object[] { blog.Id, c.RoasterDate, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.RoasterDate.Value.ToShortDateString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }

                    }
                    return View();

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
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {

                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            EmpTimingMonthlyRoaster corp = db.EmpTimingMonthlyRoaster.Include(e => e.TimingMonthlyRoaster)
                                .Include(e => e.TimingPolicy)
                                .Include(e => e.DayType).FirstOrDefault(e => e.Id == auth_id);

                            corp.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = corp.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.EmpTimingMonthlyRoaster.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, corp.DBTrack);
                            DT_EmpTimingMonthlyRoaster DT_Corp = (DT_EmpTimingMonthlyRoaster)rtn_Obj;
                            DT_Corp.TimingMonthlyRoaster_Id = corp.TimingMonthlyRoaster == null ? 0 : corp.TimingMonthlyRoaster.Id;
                            DT_Corp.DayType_Id = corp.DayType == null ? 0 : corp.DayType.Id;
                            DT_Corp.TimingPolicy_Id = corp.TimingPolicy == null ? 0 : corp.TimingPolicy.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            // return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                        }
                    }
                    else if (auth_action == "M")
                    {

                        EmpTimingMonthlyRoaster Old_Corp = db.EmpTimingMonthlyRoaster.Include(e => e.DayType)
                                                          .Include(e => e.TimingMonthlyRoaster)
                                                          .Include(e => e.TimingPolicy).Where(e => e.Id == auth_id).SingleOrDefault();



                        DT_EmpTimingMonthlyRoaster Curr_Corp = db.DT_EmpTimingMonthlyRoaster
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            EmpTimingMonthlyRoaster corp = new EmpTimingMonthlyRoaster();

                            //   string Corp = Curr_Corp.DayType_Id == null ? null : Curr_Corp.DayType_Id.ToString();
                            //   string Addrs = Curr_Corp.TimingMonthlyRoaster_Id == null ? null : Curr_Corp.TimingMonthlyRoaster_Id.ToString();
                            //   string ContactDetails = Curr_Corp.TimingPolicy_Id == null ? null : Curr_Corp.TimingPolicy_Id.ToString();
                            corp.RoasterDate = Curr_Corp.RoasterDate == null ? Old_Corp.RoasterDate : Curr_Corp.RoasterDate;

                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        corp.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                            CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        //   int a = EditS(Corp, Addrs, ContactDetails, auth_id, corp, corp.DBTrack);


                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add(" Record Authorised ");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (EmpTimingMonthlyRoaster)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                    else
                                    {
                                        var databaseValues = (EmpTimingMonthlyRoaster)databaseEntry.ToObject();
                                        corp.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //   return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                            //return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed from history.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            EmpTimingMonthlyRoaster corp = db.EmpTimingMonthlyRoaster.AsNoTracking().Include(e => e.TimingMonthlyRoaster)
                                                                        .Include(e => e.DayType)
                                                                        .Include(e => e.TimingPolicy).FirstOrDefault(e => e.Id == auth_id);

                            TimingMonthlyRoaster add = corp.TimingMonthlyRoaster;
                            TimingPolicy conDet = corp.TimingPolicy;
                            LookupValue val = corp.DayType;

                            corp.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.EmpTimingMonthlyRoaster.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, corp.DBTrack);
                            DT_EmpTimingMonthlyRoaster DT_Corp = (DT_EmpTimingMonthlyRoaster)rtn_Obj;
                            DT_Corp.TimingMonthlyRoaster_Id = corp.TimingMonthlyRoaster == null ? 0 : corp.TimingMonthlyRoaster.Id;
                            DT_Corp.DayType_Id = corp.DayType == null ? 0 : corp.DayType.Id;
                            DT_Corp.TimingPolicy_Id = corp.TimingPolicy == null ? 0 : corp.TimingPolicy.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            // return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }

                    }
                    return View();
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
        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                //  var context = DataContextFactory.GetDataContext();
                var changedEntries = db.ChangeTracker.Entries()
                    .Where(x => x.State != System.Data.Entity.EntityState.Unchanged).ToList();

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Modified))
                {
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Added))
                {
                    entry.State = System.Data.Entity.EntityState.Detached;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Deleted))
                {
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

            }
        }
    }
}