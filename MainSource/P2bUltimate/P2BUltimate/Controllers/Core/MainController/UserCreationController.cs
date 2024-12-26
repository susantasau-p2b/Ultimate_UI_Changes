using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2b.Global;
using System.Transactions;
using P2BUltimate.Security;
using P2BUltimate.Models;
using System.Text;

namespace P2BUltimate.Controllers
{
    public class UserCreationController : Controller
    {
        //
        // GET: /UserCreation/
        public ActionResult Index()
        {
            return View("~/Views/UserCreation/Index.cshtml");
        }
        public ActionResult Create(Login Cr, FormCollection form)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                string Emps = form["Employee-Table"] == "0" ? "0" : form["Employee-Table"];
                string MobileNumberList = form["MobileNumberList"] == "0" ? "0" : form["MobileNumberList"];
                string IsMobileGeoFenceAttendAAppl = form["IsMobileGeoFenceAttendAAppl"] == "0" ? "0" : form["IsMobileGeoFenceAttendAAppl"];
                string IsUltimateHOAppl = form["IsUltimateHOAppl"] == "0" ? "0" : form["IsUltimateHOAppl"];
                var Empids = Utility.StringIdsToListIds(Emps);
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {

                        try
                        {
                            PasswordPolicy objofpasswordpolicy = new PasswordPolicy();
                            objofpasswordpolicy = db.PasswordPolicy.Include(e => e.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO").FirstOrDefault();
                            string EncryptedPassword = "";
                            string CorContactMobileNo = "";
                            foreach (var item in Empids)
                            {
                                var dbEmployee = db.Employee.Include(e => e.CorContact).Include(e => e.CorContact.ContactNumbers).Where(e => e.Id == item).FirstOrDefault();

                                if (objofpasswordpolicy != null)
                                {
                                    EncryptedPassword = objofpasswordpolicy.AllowEncryption == true ? P2BSecurity.Encrypt(dbEmployee.EmpCode) : dbEmployee.EmpCode;
                                }
                                if (dbEmployee.CorContact != null)
                                {
                                    CorContactMobileNo = dbEmployee.CorContact.ContactNumbers.ToList().OrderBy(e => e.Id).FirstOrDefault().MobileNo;
                                }
                                Login ObjOfLn = new Login()
                                {
                                    IsESSAppl = true,
                                    IsMobileAppl = true,
                                    IsActive=true,
                                    MobileNumber = !string.IsNullOrEmpty(CorContactMobileNo) ? CorContactMobileNo : null,
                                    UserId = dbEmployee.EmpCode,
                                    Password = !string.IsNullOrEmpty(EncryptedPassword) ? EncryptedPassword : null,
                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }

                                };
                                db.Login.Add(ObjOfLn);
                                db.SaveChanges();
                                var db_data = db.Employee.Where(e => e.Id == item).FirstOrDefault();
                                db_data.Id = item;
                                db_data.Login = ObjOfLn;
                                db.Employee.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
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
                                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                LogTime = DateTime.Now
                            };
                            Logfile.CreateLogFile(Err);
                            Msg.Add(ex.Message);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }
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

                    List<string> MsgB = new List<string>();
                    var errorMsg = sb.ToString();
                    MsgB.Add(errorMsg);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);
                }

            }
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string MobileNumber { get; set; }
            public string IsUltimateHOAppl { get; set; }
            public string IsESSAppl { get; set; }
            public string IsMobileAppl { get; set; }
            public string IsMobileGeoFenceAttendAAppl { get; set; }
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

                IEnumerable<P2BGridData> Employee = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;

                var Employeedata = db.Employee
                    .Include(e => e.Login).
                    Include(e => e.EmpName).
                    Include(e => e.ServiceBookDates).
                    Where(e => e.ServiceBookDates.ServiceLastDate == null).ToList();

                foreach (var z in Employeedata)
                {
                    view = new P2BGridData()
                    {
                        Id = z.Id,
                        EmpCode = z.EmpCode,
                        EmpName = z.EmpName.FullNameFML,
                        MobileNumber = z.Login == null ? " " : z.Login.MobileNumber == null ? " " : z.Login.MobileNumber,
                        IsUltimateHOAppl = z.Login == null ? " " : z.Login.IsUltimateHOAppl.ToString(),
                        IsESSAppl = z.Login == null ? " " : z.Login.IsESSAppl.ToString(),
                        IsMobileAppl = z.Login == null ? " " : z.Login.IsMobileAppl.ToString(),
                        IsMobileGeoFenceAttendAAppl = z.Login == null ? " " : z.Login.IsMobileGeoFenceAttendAAppl.ToString()
                    };

                    model.Add(view);
                }

                Employee = model;

                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = Employee;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                              || (e.EmpCode.ToString().Contains(gp.searchString))
                              || (e.EmpName.ToString().Contains(gp.searchString))
                              || (e.MobileNumber.ToString().Contains(gp.searchString))
                              || (e.IsUltimateHOAppl.ToString().Contains(gp.searchString))
                              || (e.IsESSAppl.ToString().Contains(gp.searchString))
                              || (e.IsMobileAppl.ToString().Contains(gp.searchString))
                              || (e.IsMobileGeoFenceAttendAAppl.ToString().Contains(gp.searchString)))
                              .Select(a => new Object[] { a.Id, a.EmpCode, a.EmpName, a.MobileNumber, a.IsUltimateHOAppl, a.IsESSAppl, a.IsMobileAppl, a.IsMobileGeoFenceAttendAAppl }).ToList();


                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpCode, a.EmpName, a.MobileNumber, a.IsUltimateHOAppl, a.IsESSAppl, a.IsMobileAppl, a.IsMobileGeoFenceAttendAAppl }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Employee;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id.ToString() :
                                         gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.ToString() :
                                         gp.sidx == "MobileNumber" ? c.MobileNumber.ToString() :
                                         gp.sidx == "IsUltimateHOAppl" ? c.IsUltimateHOAppl.ToString() :
                                         gp.sidx == "IsESSAppl" ? c.IsESSAppl.ToString() :
                                         gp.sidx == "IsMobileAppl" ? c.IsMobileAppl.ToString() :
                                         gp.sidx == "IsMobileGeoFenceAttendAAppl" ? c.IsMobileGeoFenceAttendAAppl.ToString() :
                                         "");


                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.EmpCode, a.EmpName, a.MobileNumber, a.IsUltimateHOAppl, a.IsESSAppl, a.IsMobileAppl, a.IsMobileGeoFenceAttendAAppl }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.EmpCode, a.EmpName, a.MobileNumber, a.IsUltimateHOAppl, a.IsESSAppl, a.IsMobileAppl, a.IsMobileGeoFenceAttendAAppl }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpCode, a.EmpName, a.MobileNumber, a.IsUltimateHOAppl, a.IsESSAppl, a.IsMobileAppl, a.IsMobileGeoFenceAttendAAppl }).ToList();
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
        public ActionResult LoginContactNumbers(string data, string data2, string Empids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int ids = Convert.ToInt32(Empids);
                var qurey1 = db.Employee.Include(e => e.CorContact).Include(e => e.CorContact.ContactNumbers).Where(e => e.Id == ids).FirstOrDefault();
                var query2 = qurey1.CorContact;
                if (query2 != null)
                {
                    var query3 = query2.ContactNumbers.ToList();
                    if (query3 != null)
                    {
                        var selected = (Object)null;
                        if (!string.IsNullOrEmpty(data2))
                        {
                            selected = Convert.ToInt32(data2);
                        }
                        SelectList s = new SelectList(query3, "Id", "MobileNo", selected);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                else
                {
                    return null;
                }

            }
        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.Employee.Include(e => e.Login).Where(e => e.Id == data)
                    .Select(e => new
                    {
                        IsMobileGeoFenceAttendAAppl = e.Login.IsMobileGeoFenceAttendAAppl,
                        IsUltimateHOAppl = e.Login.IsUltimateHOAppl,
                    }).ToList();

                return Json(new Object[] { returndata, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }

        public ActionResult EditSave(Login c, int data, FormCollection form)
        {

            List<string> Msg = new List<string>();
            string MobileNumberList = form["MobileNumberList"] == "0" ? "0" : form["MobileNumberList"];
            string IsMobileGeoFenceAttendAAppl = form["IsMobileGeoFenceAttendAAppl"] == "0" ? "0" : form["IsMobileGeoFenceAttendAAppl"];
            string IsUltimateHOAppl = form["IsUltimateHOAppl"] == "0" ? "0" : form["IsUltimateHOAppl"];
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        int contactnoids = Convert.ToInt32(MobileNumberList);
                        var db_data = db.Employee.Include(e => e.Login).Where(e => e.Id == data).FirstOrDefault();
                        int Loginids = db_data.Login.Id;
                        Login LnObj = db.Login.Find(Loginids);

                        c.DBTrack = new DBTrack
                        {
                            CreatedBy = LnObj.DBTrack.CreatedBy == null ? null : LnObj.DBTrack.CreatedBy,
                            CreatedOn = LnObj.DBTrack.CreatedOn == null ? null : LnObj.DBTrack.CreatedOn,
                            Action = "M",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now
                        };

                        LnObj.Id = Loginids;
                        LnObj.IsUltimateHOAppl = c.IsUltimateHOAppl;
                        LnObj.IsMobileGeoFenceAttendAAppl = c.IsMobileGeoFenceAttendAAppl;
                        LnObj.MobileNumber = db.ContactNumbers.Find(contactnoids) == null ? null : db.ContactNumbers.Find(contactnoids).MobileNo;
                        LnObj.DBTrack = c.DBTrack;
                        db.Login.Attach(LnObj);
                        db.Entry(LnObj).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
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
}