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
using System.Diagnostics;
using P2B.MOBILE;
using System.Configuration;
using P2B.UTILS;
using System.IO;
using System.Xml.Linq;
namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class RecoveryAttendanceController : Controller
    {
        // GET: RecoveryAttendance
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/RecoveryAttendance/Index.cshtml");
        }

        public class ETRM_AttendanceRecovery
        {
            public int? CompId { get; set; }

            public List<string> CardCodes { get; set; }

            public DateTime? FromPeriod { get; set; }

            public DateTime? ToPeriod { get; set; }

            public int? RecoveryType { get; set; }

            public string UserCode { get; set; }
        }

        public class ETRM_AttendanceProcess
        {
            public List<int> Emp_Ids { get; set; }

            public DateTime? FromPeriod { get; set; }

            public DateTime? ToPeriod { get; set; }
        }

        public class ReturnData_AttendanceProcess
        {
            public int? ErrNo { get; set; }

            public int? InfoNo { get; set; }

            public string EmpCode { get; set; }

            public string ErrMsg { get; set; }
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

        public class RecoveryData
        {
            public string value { get; set; }
            public string LookupVal { get; set; }
        }

        //private DataBaseContext db = new DataBaseContext();

        public List<RecoveryData> GetReportname(string data)
        {
            String path = Server.MapPath("~/App_Data/Menu_Json/AttendanceRecovery.xml");

            try
            {
                List<string> query = new List<string>();

                var ttt = XElement.Load(path).Elements("Rptname");
                foreach (XElement level1Element in ttt)
                {
                    var chk = level1Element.Attribute("name").Value;
                    if (chk == data)
                    {
                        var lv2 = level1Element.Elements("column");
                        foreach (XElement level2Element in lv2)
                        {
                            query.Add(level2Element.Attribute("name").Value);
                        }
                    }
                }
                if (query.Count != 0)
                {
                    var result = (from c in query
                                  select new { value = c, LookupVal = c });
                    List<RecoveryData> ORecovery = new List<RecoveryData>();
                    foreach (var item in result)
	                {
		                RecoveryData ORecData = new RecoveryData(){ value = item.value, LookupVal = item.LookupVal};
                        ORecovery.Add(ORecData);
                    } 
                    return ORecovery;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception)
            {

                throw;
            }
            return null;
        }
       
              
        public ActionResult Create(EmpTimingMonthlyRoaster c, FormCollection form) //Create submit
        {

          
                List<string> Msg = new List<string>();
                try
                {
                    string PeriodFrom = form["PeriodFrom"] != null ? form["PeriodFrom"] : string.Empty;
                    string PeriodTo = form["PeriodTo"] != null ? form["PeriodTo"] : string.Empty;
                    string type = form["type"] != null ? form["type"] : string.Empty;
                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    List<int> ids = null;
                    var db_type = 0;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = Utility.StringIdsToListIds(Emp);
                    }
                    else
                    {
                        Msg.Add(" Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (PeriodFrom != null && PeriodFrom != "" && PeriodTo != null && PeriodTo != "")
                    {

                        if (Convert.ToDateTime(PeriodTo) < Convert.ToDateTime(PeriodFrom))
                        {
                            Msg.Add(" To date Should be grater than from date  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        Msg.Add(" Please Select From date and To Date  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    List<string> CardCode = new List<string>();
                    LookupValue temp = null;
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        //string PayMonth = form["PayMonth"] != null ? form["PayMonth"] : string.Empty;
                       
                        
                        foreach (var i in ids)
                        {
                            CardCode.Add(db.EmployeeAttendance.Where(e => e.Employee.Id == i).Select(e => e.Employee.CardCode).SingleOrDefault());
                        }
                        Boolean crdass = false;
                        foreach (var i in ids)
                        {
                            var cardcodeassing = db.EmployeeAttendance.Include(e => e.Employee).Where(e => e.Employee.Id == i).SingleOrDefault();
                            if (cardcodeassing.Employee.CardCode == null || cardcodeassing.Employee.CardCode == "")
                            {
                                crdass = true;
                                Msg.Add("Please Define cardCode For " + cardcodeassing.Employee.EmpCode);
                            }
                        }
                        if (crdass == true)
                        {
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        var id = Convert.ToInt32(type);
                      
                        temp = db.LookupValue.Where(e => e.Id == id).FirstOrDefault();
                        if (temp != null)
                        {
                            db_type = temp.Id;
                        }
                        else
                        {
                            Msg.Add("Please Select Db Type..!");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        var InputType = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "5000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "MACHINE").FirstOrDefault();//db.LookupValue.Where(e => e.LookupVal.ToUpper() == "MACHINE").SingleOrDefault();
                        if (InputType == null)
                        {
                            Msg.Add("Please Define Input Type - Machine/Mobile..!");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                        //                    new System.TimeSpan(0, 30, 0)))
                        //{
                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        //var _PayMonth = Convert.ToDateTime(PayMonth);
                        
                    }

                    var _PeriodFrom = Convert.ToDateTime(PeriodFrom);
                    var _PeriodTo = Convert.ToDateTime(PeriodTo);

                   // List<RecoveryData> ORecoveryData = GetReportname(temp.LookupVal);

                    //string Err = AttendanceProcess.Recovery_AttendanceData(
                    //            _CompId: Convert.ToInt32(SessionManager.CompanyId),
                    //            _CardCodes: CardCode,
                    //            _PeriodFrom: _PeriodFrom,
                    //            _PeriodTo: _PeriodTo,
                    //     //_PayMonth: _PayMonth,
                    //            _ReCoveryType: db_type
                    //            );

                    //ts.Complete();
                    //if (Err == "0")
                    //{
                    //    Msg.Add("Data Saved Successfully");
                    //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    //else
                    //{
                    //    Msg.Add(Err);
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                        // ====================================== Api call Start ==============================================
                    ReturnData_AttendanceProcess returnDATA = new ReturnData_AttendanceProcess();

                    var ShowMessageCode = "";
                    var ShowMessage = "";
                    var ShowErrorMessage = "";


                  //  var _PeriodFrom = Convert.ToDateTime(PeriodFrom);
                  //  var _PeriodTo = Convert.ToDateTime(PeriodTo);

                    ServiceResultList<ReturnData_AttendanceProcess> responseDeserializeData = new ServiceResultList<ReturnData_AttendanceProcess>();
                    string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                    using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                    {
                        var response = p2BHttpClient.request("ETRM/getUserAttendanceRecoveryProcessRequest",
                            new ETRM_AttendanceRecovery() { CompId = Convert.ToInt32(SessionManager.CompanyId), CardCodes = CardCode, FromPeriod = _PeriodFrom, ToPeriod = _PeriodTo, RecoveryType = db_type, UserCode = SessionManager.UserName });

                        var data = response.Content.ReadAsStringAsync().Result;
                        // var result = data.;

                        responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResultList<ReturnData_AttendanceProcess>>(response.Content.ReadAsStringAsync().Result);


                        ShowMessageCode = responseDeserializeData.MessageCode.ToString();
                        ShowMessage = responseDeserializeData.Message;

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
                                    string path = requiredPath + @"\MAN_RECOVERY_" + Convert.ToDateTime(DateTime.Now.Date).ToString("ddMMyyyy") + ".txt";
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

                    try
                    {

                        if (responseDeserializeData == null && ShowMessageCode == "OK")
                        {
                            Msg.Add(ShowMessage);
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            Msg.Add(ShowMessage + " ,  " + ShowErrorMessage);
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = 0 });
                    }
                    catch (DataException /* dex */)
                    {
                        // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                        Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }



                    // ====================================== Api call End ==============================================
                        
                    //}
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
                return null;
            
        }
        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.EmployeeAttendance.Where(e => e.Employee != null).Include(e => e.Employee.EmpName)
                        //.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                        //.Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
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
                                // JoiningDate = item.Employee.ServiceBookDates != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
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

        public class EmpManulattChildDataClass
        {
            public int Id { get; set; }
            public string TimingCode { get; set; }
            public string SwipeDate { get; set; }
            public string InTime { get; set; }
            public string OutTime { get; set; }
            public string MusterRemarks { get; set; }
            public string IsLocked { get; set; }
            public string EarlyCount { get; set; }
            public string LateCount { get; set; }

            public int UnitCode { get; set; }

            public string DownloadDate { get; set; }

            public bool SwipeStatus { get; set; }

            public string SwipeTime { get; set; }
        }
        public ActionResult Get_ManualAttendanceProcessData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeAttendance
                        .Include(e => e.RawData)
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<EmpManulattChildDataClass> returndata = new List<EmpManulattChildDataClass>();
                        foreach (var item in db_data.RawData.ToList())
                        {

                            returndata.Add(new EmpManulattChildDataClass
                            {
                                Id = item.Id,
                                DownloadDate = item.DownloadDate != null ? item.DownloadDate.Value.ToShortDateString() : null,
                                SwipeDate = item.SwipeDate != null ? item.SwipeDate.Value.ToString("dd/MM/yyyy") : "",
                                SwipeTime = item.SwipeTime != null ? Utility._returnTimeSpan(item.SwipeTime.Value).ToString() : "",
                                SwipeStatus = item.SwipeStatus != null ? item.SwipeStatus : false,
                                UnitCode = item.UnitCode,
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

    }
}