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

namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class AttendanceAbsentPolicyController : Controller
    {
        //
        // GET: /AttendanceAbsentPolicy/
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/AttendanceAbsentPolicy/Index.cshtml");
        }


        [HttpPost]
        public ActionResult Create(AttendanceAbsentPolicy c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string frequency = form["Frequencylist"] == "0" ? "" : form["Frequencylist"];
                    string type = form["Typelist"] == "0" ? "" : form["Typelist"];
                    string roundingmethod = form["RoundingMethodlist"] == "0" ? "" : form["RoundingMethodlist"];
                    string SalHeadOperType = form["SalHeadOperationTypelist"] == "0" ? "" : form["SalHeadOperationTypelist"];
                    string ProcessType = form["ProcessTypelist"] == "0" ? "" : form["ProcessTypelist"];
                    string LvHead = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var companypayroll = new CompanyPayroll();
                    companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == company_Id).SingleOrDefault();



                    //if (frequency != null && frequency != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(frequency));
                    //    c.Frequency = val;
                    //}

                    //if (type != null && type != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(type));
                    //    c.Type = val;
                    //}

                    //if (roundingmethod != null && roundingmethod != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(roundingmethod));
                    //    c.RoundingMethod = val;
                    //}

                    //if (SalHeadOperType != null && SalHeadOperType != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(SalHeadOperType));
                    //    c.SalHeadOperationType = val;
                    //}

                    //if (ProcessType != null && ProcessType != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(ProcessType));
                    //    c.ProcessType = val;
                    //}
                    //List<LeaveDependPolicy> lookupval = new List<LeaveDependPolicy>();
                    //if (LvHead != null && LvHead != "")
                    //{
                    //    var ids = Utility.StringIdsToListIds(LvHead);
                    //    foreach (var ca in ids)
                    //    {
                    //        var LvHead_val = db.LeaveDependPolicy.Find(ca);
                    //        lookupval.Add(LvHead_val);
                    //        c.LeaveDependPolicy = lookupval;
                    //    }
                    //}

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.SalaryHead.Any(o => o.Code == c.Code))
                            //{
                            //    Msg.Add("  Code Already Exists.  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //    // return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            //}

                            //if (c.OnLeave == true && c.LeaveDependPolicy == null)
                            //{
                            //    Msg.Add("  Please Select LeaveDependPolicy.  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            AttendanceAbsentPolicy OBJ_AttendAbsPolicy = new AttendanceAbsentPolicy()
                            {
                                
                                OnAttendancePolicyAppl = c.OnAttendancePolicyAppl,
                                OnNormalHolidayPolicyApp = c.OnNormalHolidayPolicyApp,
                                OnPublicHoldayPolicyAppl = c.OnPublicHoldayPolicyAppl,
                                AuthorizedAbsentPolicyApp = c.AuthorizedAbsentPolicyApp,
                                UnauthorizedAbsentPolicyApp = c.UnauthorizedAbsentPolicyApp,
                               
                                DBTrack = c.DBTrack,
                                
                            };
                            try
                            {
                                db.AttendanceAbsentPolicy.Add(OBJ_AttendAbsPolicy);
                                
                                db.SaveChanges();


                                //if (companypayroll != null)
                                //{
                                //    var SalaryHead_list = new List<SalaryHead>();
                                //    SalaryHead_list.Add(SalaryHead);
                                //    companypayroll.SalaryHead = SalaryHead_list;
                                //    db.CompanyPayroll.Attach(companypayroll);
                                //    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                //    db.SaveChanges();
                                //    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                                //}


                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        var errorMsg = sb.ToString();
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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
                var Q = db.AttendanceAbsentPolicy
                    
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Action = e.DBTrack.Action,
                        OnAttendancePolicyAppl = e.OnAttendancePolicyAppl,
                        OnNormalHolidayPolicyApp = e.OnNormalHolidayPolicyApp,
                        OnPublicHoldayPolicyAppl = e.OnPublicHoldayPolicyAppl,
                        AuthorizedAbsentPolicyApp = e.AuthorizedAbsentPolicyApp,
                        UnauthorizedAbsentPolicyApp = e.UnauthorizedAbsentPolicyApp
                        
                    }).ToList();




                var Corp = db.AttendanceAbsentPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }



        [HttpPost]
        public async Task<ActionResult> EditSave(AttendanceAbsentPolicy c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();
            string frequency = form["Frequencylist"] == "0" ? "" : form["Frequencylist"];
            string Saltype = form["Typelist"] == "0" ? "" : form["Typelist"];
            string roundingmethod = form["RoundingMethodlist"] == "0" ? "" : form["RoundingMethodlist"];
            string SalHeadOperType = form["SalHeadOperationTypelist"] == "0" ? "" : form["SalHeadOperationTypelist"];
            string ProcessType = form["ProcessTypelist"] == "0" ? "" : form["ProcessTypelist"];
            string LvHead = form["LvDependPolicylist"] == "0" ? "" : form["LvDependPolicylist"];



            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.AttendanceAbsentPolicy.Where(e => e.Id == data).FirstOrDefault();
                        List<LeaveDependPolicy> LvH = new List<LeaveDependPolicy>();

                        //if (LvHead != null)
                        //{
                        //    var ids = Utility.StringIdsToListIds(LvHead);
                        //    foreach (var ca in ids)
                        //    {
                        //        var LvHead_val = db.LeaveDependPolicy.Find(ca);
                        //        LvH.Add(LvHead_val);
                        //        db_data.LeaveDependPolicy = LvH;
                        //    }
                        //}
                        //else
                        //{
                        //    db_data.LeaveDependPolicy = null;
                        //}


                        //db.SalaryHead.Attach(db_data);
                        //db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();

                        TempData["RowVersion"] = db_data.RowVersion;

                        AttendanceAbsentPolicy AttAbsPolicy = db.AttendanceAbsentPolicy.Find(data);
                        TempData["CurrRowVersion"] = AttAbsPolicy.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = AttAbsPolicy.DBTrack.CreatedBy == null ? null : AttAbsPolicy.DBTrack.CreatedBy,
                                CreatedOn = AttAbsPolicy.DBTrack.CreatedOn == null ? null : AttAbsPolicy.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            AttAbsPolicy.OnAttendancePolicyAppl = c.OnAttendancePolicyAppl;
                            AttAbsPolicy.OnNormalHolidayPolicyApp = c.OnNormalHolidayPolicyApp;
                            AttAbsPolicy.OnPublicHoldayPolicyAppl = c.OnPublicHoldayPolicyAppl;
                            AttAbsPolicy.AuthorizedAbsentPolicyApp = c.AuthorizedAbsentPolicyApp;
                            AttAbsPolicy.UnauthorizedAbsentPolicyApp = c.UnauthorizedAbsentPolicyApp;
                            AttAbsPolicy.Id = data;
                            AttAbsPolicy.DBTrack = c.DBTrack;

                            db.Entry(AttAbsPolicy).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        ts.Complete();
                        Msg.Add(" Record Updated Successfully. ");
                        return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }


        public class P2BGridData
        {
            public int Id { get; set; }
            public bool OnAttendancePolicyAppl { get; set; }
            public bool OnNormalHolidayPolicyApp { get; set; }
            public bool OnPublicHoldayPolicyAppl { get; set; }
            public bool AuthorizedAbsentPolicyApp { get; set; }
            public bool UnauthorizedAbsentPolicyApp { get; set; }
          

        }


        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> salheadList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;


                    
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);

                    var BindCompList = db.AttendanceAbsentPolicy.ToList();



                    foreach (var s in BindCompList)
                    {
                        if (s != null)
                        {

                            
                                view = new P2BGridData()
                                {
                                    Id = s.Id,
                                    OnAttendancePolicyAppl = s.OnAttendancePolicyAppl,
                                    OnNormalHolidayPolicyApp = s.OnNormalHolidayPolicyApp,
                                    OnPublicHoldayPolicyAppl = s.OnPublicHoldayPolicyAppl,
                                    AuthorizedAbsentPolicyApp = s.AuthorizedAbsentPolicyApp,
                                    UnauthorizedAbsentPolicyApp = s.UnauthorizedAbsentPolicyApp,
                                   

                                };
                                model.Add(view);

                            }
                       

                    }

                    salheadList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = salheadList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                || (e.OnNormalHolidayPolicyApp.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.OnPublicHoldayPolicyAppl.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               
                                || (e.OnAttendancePolicyAppl.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.AuthorizedAbsentPolicyApp.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.UnauthorizedAbsentPolicyApp.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               
                                ).Select(a => new Object[] { a.AuthorizedAbsentPolicyApp, a.OnAttendancePolicyAppl, a.OnNormalHolidayPolicyApp, a.OnPublicHoldayPolicyAppl, a.UnauthorizedAbsentPolicyApp, a.Id }).ToList();
                            
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.OnAttendancePolicyAppl), Convert.ToString(a.AuthorizedAbsentPolicyApp), Convert.ToString(a.OnNormalHolidayPolicyApp), Convert.ToString(a.OnPublicHoldayPolicyAppl), Convert.ToString(a.UnauthorizedAbsentPolicyApp), a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = salheadList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "OnAttendancePolicyAppl" ? c.OnAttendancePolicyAppl.ToString() :
                                             gp.sidx == "AuthorizedAbsentPolicyApp" ? c.AuthorizedAbsentPolicyApp.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.OnAttendancePolicyAppl), Convert.ToString(a.AuthorizedAbsentPolicyApp), Convert.ToString(a.OnNormalHolidayPolicyApp), Convert.ToString(a.OnPublicHoldayPolicyAppl), Convert.ToString(a.UnauthorizedAbsentPolicyApp), a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.OnAttendancePolicyAppl), Convert.ToString(a.AuthorizedAbsentPolicyApp), Convert.ToString(a.OnNormalHolidayPolicyApp), Convert.ToString(a.OnPublicHoldayPolicyAppl), Convert.ToString(a.UnauthorizedAbsentPolicyApp), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.OnAttendancePolicyAppl), Convert.ToString(a.AuthorizedAbsentPolicyApp), Convert.ToString(a.OnNormalHolidayPolicyApp), Convert.ToString(a.OnPublicHoldayPolicyAppl), Convert.ToString(a.UnauthorizedAbsentPolicyApp), a.Id }).ToList();
                        }
                        totalRecords = salheadList.Count();
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


        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    AttendanceAbsentPolicy AttenAbsentPolicys = db.AttendanceAbsentPolicy
                                                      
                                                     .Where(e => e.Id == data).SingleOrDefault();

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {



                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = AttenAbsentPolicys.DBTrack.CreatedBy != null ? AttenAbsentPolicys.DBTrack.CreatedBy : null,
                            CreatedOn = AttenAbsentPolicys.DBTrack.CreatedOn != null ? AttenAbsentPolicys.DBTrack.CreatedOn : null,
                            IsModified = AttenAbsentPolicys.DBTrack.IsModified == true ? false : false//,

                        };
                        db.Entry(AttenAbsentPolicys).State = System.Data.Entity.EntityState.Deleted;
                        await db.SaveChangesAsync();
                        ts.Complete();
                        Msg.Add("  Data removed.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    //}
                }
                catch (RetryLimitExceededException /* dex */)
                {

                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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