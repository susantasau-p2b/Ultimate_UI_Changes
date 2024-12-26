using Attendance;
using EssPortal.App_Start;
using EssPortal.Models;
using EssPortal.Security;
using P2b.Global;
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
using System.Configuration;
using System.IO;
using EssPortal.Process;

namespace EssPortal.Controllers
{
    public class AttendanceDetailsController : Controller
    {
        //
        // GET: /AttendanceDetails/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/_AttendanceDetails.cshtml");
        }

        public Object Create(FormCollection form, String data)
        {
            var Emp = Convert.ToInt32(SessionManager.EmpId);
            
            using (DataBaseContext db = new DataBaseContext())
            {

                var db_data = db.EmployeeAttendance.Include(e => e.Employee)
                  .Where(e => e.Employee_Id == Emp).SingleOrDefault();

            
                    
                        using (TransactionScope ts = new TransactionScope())
                        {
                             DBTrack dbT= new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            AttWFDetails oAttWFDetails = new AttWFDetails
                            {
                                WFStatus = 0,
                                Comments = "Applied",
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                            };

                            List<AttWFDetails> oAttWFDetails_List = new List<AttWFDetails>();
                            oAttWFDetails_List.Add(oAttWFDetails);

                            RawData ppdtl = new RawData
                            {
                                CardCode = db_data.Employee.CardCode,
                                DownloadDate = DateTime.Now,
                                EmployeeAttendance_Id = db_data.Id,
                                SwipeDate = DateTime.Now.Date,
                                SwipeTime = DateTime.Now,
                                DBTrack = dbT,
                                InputType = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "5000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "ESS").FirstOrDefault(),
                                WFStatus = 0,
                            };
                            try
                            {
                                db.RawData.Add(ppdtl);
                                db.SaveChanges();
                                string fileName = ppdtl.Id.ToString();

                                string ServerSavePath = ConfigurationManager.AppSettings["EmployeeDocuments"];
                                string targetImagepath = ServerSavePath + "EmpCode" + db_data.Employee.EmpCode + "\\ETRM\\Attendance\\" + fileName + ".jpg";

                                string newFileName = ServerSavePath + "EmpCode" + db_data.Employee.EmpCode + "\\ETRM\\Attendance\\" + fileName + ".jpg";
                                string oldFileName = ServerSavePath + "EmpCode" + db_data.Employee.EmpCode + "\\ETRM\\Attendance\\" + Session["FileName"].ToString() + ".jpg";

                                System.IO.File.Move(oldFileName, newFileName);
                                System.IO.File.Delete(oldFileName); // Delete the existing file if exists

                                ts.Complete();
                                return new { status = true, responseText = "Data Created Successfully." }; 
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = ppdtl.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                            }

                        }
                   
                
            }


        }

        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }
        }

        public class GetFutureODClass
        { 
            public string SwipeDate { get; set; }
            public string SwipeTime { get; set; } 
            public string ID { get; set; }
            public string Status { get; set; }
            public ChildGetLvNewReqClass RowData { get; set; }
        }


        public ActionResult GetMyAttendanceDetails()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpId);


                var db_data = db.EmployeeAttendance
                    .Include(e => e.Employee)
                    .Include(e => e.RawData)
                    .Include(e => e.RawData.Select(t => t.InputType))
                    .Include(e => e.RawDataFailure)
                    .Where(e => e.Employee.Id == Id).SingleOrDefault();


                if (db_data != null)
                {
                    List<GetFutureODClass> returndata = new List<GetFutureODClass>();
                    returndata.Add(new GetFutureODClass
                    {
                        SwipeDate = "Swipe Date",
                        SwipeTime = "Swipe Time" ,
                        Status = "Status" 
                    });


                    DateTime LastDate = DateTime.Now.AddDays(-7).Date;
                    List<RawData> ORawDataList = db_data.RawData.Where(e => e.SwipeDate.Value >= LastDate && e.InputType != null && e.InputType.LookupVal.ToUpper() == "ESS").OrderByDescending(e => e.SwipeTime).ToList();
                    foreach (var a in ORawDataList)
                    {
                        
                        var session = Session["auho"].ToString().ToUpper();

                       

                        returndata.Add(new GetFutureODClass
                        {
                            RowData = new ChildGetLvNewReqClass
                            {
                                LvNewReq = a.Id.ToString(),
                                EmpLVid = db_data.Id.ToString(),
                                IsClose = "",
                                LvHead_Id = "",
                            },
                            SwipeDate = a.SwipeDate != null ? a.SwipeDate.Value.ToShortDateString() : "--",
                            SwipeTime = a.SwipeTime != null ? a.SwipeTime.Value.ToShortTimeString() : "--", 
                            Status = a.Narration != null ? a.Narration : "Not Verified"
                        });
                    }

                    List<RawDataFailure> ORawDataFailureList = db_data.RawDataFailure.Where(e => e.SwipeDate.Value >= LastDate).OrderByDescending(e => e.SwipeTime).ToList();
                     foreach (var a in ORawDataFailureList)
                    {

                        var session = Session["auho"].ToString().ToUpper();



                        returndata.Add(new GetFutureODClass
                        {
                            RowData = new ChildGetLvNewReqClass
                            {
                                LvNewReq = a.Id.ToString(),
                                EmpLVid = db_data.Id.ToString(),
                                IsClose = "",
                                LvHead_Id = "",
                            },
                            SwipeDate = a.SwipeDate != null ? a.SwipeDate.Value.ToShortDateString() : "--",
                            SwipeTime = a.SwipeTime != null ? a.SwipeTime.Value.ToShortTimeString() : "--",
                            Status = "Failed"
                        });
                    }


                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public ActionResult SaveCapture(string data)
        {
             var Emp = Convert.ToInt32(SessionManager.EmpId);

             using (DataBaseContext db = new DataBaseContext())
             {

                 var db_data = db.EmployeeAttendance.Include(e => e.Employee)
                   .Where(e => e.Employee_Id == Emp).SingleOrDefault();



                 using (TransactionScope ts = new TransactionScope())
                 {
                     DBTrack dbT = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                     AttWFDetails oAttWFDetails = new AttWFDetails
                     {
                         WFStatus = 0,
                         Comments = "Applied",
                         DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                     };

                     List<AttWFDetails> oAttWFDetails_List = new List<AttWFDetails>();
                     oAttWFDetails_List.Add(oAttWFDetails);

                     RawData ppdtl = new RawData
                     {
                         CardCode = db_data.Employee.CardCode,
                         DownloadDate = DateTime.Now,
                         EmployeeAttendance_Id = db_data.Id,
                         SwipeDate = DateTime.Now.Date,
                         SwipeTime = DateTime.Now,
                         DBTrack = dbT,
                         InputType = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "5000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "ESS").FirstOrDefault(),
                         WFStatus = 0,
                     };
                     try
                     {
                         db.RawData.Add(ppdtl);
                         db.SaveChanges();

                         string fileName = ppdtl.Id.ToString();

                         //Convert Base64 Encoded string to Byte Array.
                         byte[] imageBytes = Convert.FromBase64String(data.Split(',')[1]);
                         string ServerSavePath = ConfigurationManager.AppSettings["EmployeeDocuments"];
                         string targetImagepath = ServerSavePath + "EmpCode" + db_data.Employee.EmpCode + "\\ETRM\\Attendance\\";
                         if (!Directory.Exists(targetImagepath))
                         {
                             Directory.CreateDirectory(targetImagepath);
                         }
                         string ret = CompressImage.Compressimage(targetImagepath + fileName + ".jpg", "", imageBytes);
                         if (ret == null)
                         {
                             ts.Complete();
                             return Json(new { success = true, responseText = "File Uploaded And Record Saved Successfully..!" }, JsonRequestBehavior.AllowGet);
                         }
                         else { return Json(new { success = false, responseText = ret }, JsonRequestBehavior.AllowGet); }
                         //Save the Byte Array as Image File.

                         //string filePath = Server.MapPath(targetImagepath + fileName + ".jpg");
                         //System.IO.File.WriteAllBytes(filePath, imageBytes);



                     }
                     catch (DataException /* dex */)
                     {
                         return Json(new { success = true, responseText = "Unable to create. Try again, and if the problem persists contact your system administrator.." }, JsonRequestBehavior.AllowGet);
                     }

                 }

             }
            
        }

	}
}