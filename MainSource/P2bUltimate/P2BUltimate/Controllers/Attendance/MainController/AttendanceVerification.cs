///
/// Created by Anandrao
///

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
using P2BUltimate.Security;
using Leave;
using Attendance;
using System.Configuration;
using System.IO;

namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class AttendanceVerificationController : Controller
    {
        //
        // GET: /AttendanceVerification/
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/AttendanceVerification/Index.cshtml");
        }

        public class EvaluationDataClass
        {
            public int Id { get; set; }
            public string SwipeDate { get; set; }
            public string SwipeTime { get; set; }
            public string EmpPhoto { get; set; }
            public bool Checked { get; set; }
        }

        public ActionResult GetAttData(string FromDate, string ToDate, int EmpId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime FromDate1 = Convert.ToDateTime(FromDate);
                DateTime ToDate1 = Convert.ToDateTime(ToDate);
                EmployeeAttendance EmpAtt = db.EmployeeAttendance.Include(e => e.Employee).Where(e => e.Employee_Id == EmpId).FirstOrDefault();
                var OAtt = db.RawData.Include(e => e.InputType).Where(e => e.SwipeDate.Value >= FromDate1 && e.SwipeDate.Value <= ToDate1 && e.EmployeeAttendance_Id == EmpAtt.Id
                    && (e.InputType != null && e.InputType.LookupVal.ToUpper() == "ESS" && (e.Narration == "" || e.Narration == null))).ToList();

                if (OAtt != null && OAtt.Count() > 0)
                {
                    //string ServerSavePath = ConfigurationManager.AppSettings["DocumentPath"];
                   
                    List<EvaluationDataClass> returndata = new List<EvaluationDataClass>();
                    foreach (var item in OAtt)
                    {
                        string targetImagepath =  "\\EmployeeDocuments\\EmpCode" + EmpAtt.Employee.EmpCode + "\\ETRM\\Attendance\\";
                        string fileName = item.Id.ToString();
                        returndata.Add(new EvaluationDataClass
                        {
                            Id = item.Id,
                            SwipeDate = item.SwipeDate.Value.ToShortDateString(),
                            SwipeTime = item.SwipeTime.Value.ToShortTimeString(),
                            EmpPhoto = targetImagepath + fileName + ".jpg"
                        });
 
                    }

                    return Json(new { returndata = returndata, msg = "" }, JsonRequestBehavior.AllowGet);
                     
                }
                return Json(new { returndata = "", msg = "No Record Found.." }, JsonRequestBehavior.AllowGet);
            }
            
        }

        public ActionResult GetEmpImage(string Id)
        {
            var a = GetProImage("employee", Id);


            if (!string.IsNullOrEmpty(a))
            {
                JsonRequestBehavior behaviou = new JsonRequestBehavior();
                //return Json(new { data = a, status = true,ma }, JsonRequestBehavior.AllowGet);
                return new JsonResult()
                {
                    Data = a,
                    MaxJsonLength = 86753090,
                    JsonRequestBehavior = behaviou
                };

            }
            else
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
             
        }

        public static String GetProImage(String data, string Id)
        {
 
            String localPath = "";
            data = data.ToUpper();

         
            using (DataBaseContext db = new DataBaseContext())
            { 
                if (data == "EMPLOYEE")
                {
                    var id = Convert.ToInt32(Id);
                    var Emp = db.Employee.Where(e => e.Id == id).SingleOrDefault();
                    var Employee = db.Employee
                           .Include(e => e.EmployeeDocuments)
                           .Include(e => e.EmployeeDocuments.Select(t => t.DocumentType))
                           .Where(e => e.EmpCode == Emp.EmpCode).SingleOrDefault();
                    var db_Emp_path = Employee.EmployeeDocuments.Where(z => z.DocumentType.LookupVal.ToUpper() == "PROFILE")
                        .ToList().OrderByDescending(z => z.Id).FirstOrDefault();
                    if (db_Emp_path != null)
                    {
                        localPath = db_Emp_path.DocumentPath;
                    }
                }
            }
            string newPath = "";
            if (data == "COMPANY")
            {
                newPath = new Uri(localPath).LocalPath;
            }
            else
            {
                newPath = localPath;
            }
            System.IO.FileInfo file = new System.IO.FileInfo(newPath);
            if (file.Exists)
            {
                byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                return base64ImageRepresentation;
            }
            else
            {
                return null;
            }
        }


          [HttpPost]
        public ActionResult SaveData(List<EvaluationDataClass> data)
        {
            List<EvaluationDataClass> OEvaluationData = new List<EvaluationDataClass>();

            using (DataBaseContext db = new DataBaseContext())
            {
                List<RawDataFailure> oList_RawData = new List<RawDataFailure>();
                try
                {
                    using (TransactionScope ts=new TransactionScope())
                    {
                        foreach (var item in data)
                        {
                            RawData oRawData = db.RawData.Find(item.Id);
                            if (item.Checked == false)
                            { 
                                RawDataFailure oRawDataFailure = new RawDataFailure();
                                oRawDataFailure.EmployeeAttendance_Id = oRawData.EmployeeAttendance_Id;
                                oRawDataFailure.CardCode = oRawData.CardCode;
                                oRawDataFailure.UnitCode = oRawData.UnitCode;
                                oRawDataFailure.SwipeDate = oRawData.SwipeDate;
                                oRawDataFailure.SwipeTime = oRawData.SwipeTime;
                                oRawDataFailure.DownloadDate = DateTime.Now;

                                oRawDataFailure.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                oList_RawData.Add(oRawDataFailure);
                                db.RawData.Remove(oRawData);
                                db.SaveChanges();
                            }
                            else 
                            {
                                oRawData.Narration = "Verified";
                                db.Entry(oRawData).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            db.RawDataFailure.AddRange(oList_RawData);
                            db.SaveChanges();
                        }

                       
                        ts.Complete();
                    }
                    
                    return Json(new { MSG = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { MSG = ex.Message }, JsonRequestBehavior.AllowGet);
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
                  int ParentId = 2;
                  DateTime? dt = null;
                  string monthyr = "";
                  DateTime? dtChk = null;

                  dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy")).AddMonths(-1);
                  monthyr = dt.Value.ToString("MM/yyyy");
                  dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);

                  var jsonData = (Object)null;
                  var empdata = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location).Include(e => e.GeoStruct.Location.GeoFencingParameter)
                    .Include(e => e.ServiceBookDates).Include(e => e.EmpName)
                    .Where(e => e.GeoStruct != null && e.GeoStruct.Location != null && e.GeoStruct.Location.GeoFencingParameter != null && e.GeoStruct.Location.GeoFencingParameter.ESSAttAppl == true)
                    .AsNoTracking().AsParallel().ToList();


                  var emp = empdata.Where(e => e.GeoStruct_Id != null && (e.ServiceBookDates.ServiceLastDate == null || Convert.ToDateTime("01/" + e.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + monthyr))).Select(e => e).AsParallel().ToList();

                  if (gp.IsAutho == true)
                  {
                      empdata = empdata.Where(e => e.DBTrack.IsModified == true).ToList();
                  }
                  else
                  {
                      empdata = empdata.ToList();
                  }


                  IEnumerable<Employee> IE;
                  if (!string.IsNullOrEmpty(gp.searchString))
                  {
                      IE = empdata;
                      if (gp.searchOper.Equals("eq"))
                      {

                          jsonData = IE.Where(e => (e.EmpCode.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                 || (e.EmpName.FullNameFML.Contains(gp.searchString))

                                 ).Select(a => new Object[] { a.EmpCode, a.EmpName != null ? a.EmpName.FullNameFML : "", a.Id }).ToList();

                          //jsonData = IE.Select(a => new { a.Id, a.OTPoilicyName, a.BreakTime, a.compulsoryStay, a.COffOTHours, a.CompensatoryOff }).Where((e => (e.Id.ToString() == gp.searchString) || (e.BreakTime.ToString() == gp.searchString.ToLower()) || (e.compulsoryStay.ToString() == gp.searchString.ToLower()) || (e.COffOTHours.ToString() == gp.searchString.ToLower()) || (e.CompensatoryOff.ToString() == gp.searchString.ToLower())));
                      }
                      if (pageIndex > 1)
                      {
                          int h = pageIndex * pageSize;
                          jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName != null ? a.EmpName.FullNameFML : "", a.Id }).ToList();
                      }
                      totalRecords = IE.Count();
                  }
                  else
                  {
                      IE = empdata;
                      Func<Employee, dynamic> orderfuc;
                      if (gp.sidx == "Id")
                      {
                          orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                      }
                      else
                      {
                          orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                           gp.sidx == "BreakTime" ? c.EmpName.FullNameFML.ToString() : 
                                           "");
                      }

                      if (gp.sord == "asc")
                      {
                          IE = IE.OrderBy(orderfuc);
                          jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName != null ? a.EmpName.FullNameFML : "", a.Id }).ToList();
                      }
                      else if (gp.sord == "desc")
                      {
                          IE = IE.OrderByDescending(orderfuc);
                          jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName != null ? a.EmpName.FullNameFML : "", a.Id }).ToList();
                      }
                      if (pageIndex > 1)
                      {
                          int h = pageIndex * pageSize;
                          jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName != null ? a.EmpName.FullNameFML : "", a.Id }).ToList();
                      }
                      totalRecords = empdata.Count();
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
                      total = totalPages,
                      p2bparam = ParentId
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