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
    public class AttendanceLeavePriorityController : Controller
    {
        //
        // GET: /AttendanceLeavePriority/
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/AttendanceLeavePriority/Index.cshtml");
        }


        public ActionResult GetLeaveHeadDrop(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool Flag = true;
                var qurey1 = db.LvHead.Include(e => e.CompanyLeave)
                                      .Include(e => e.LvActionOnAtt)
                                      .Include(e => e.LvHeadOprationType).ToList();
                var selected1 = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected1 = Convert.ToInt32(data2);
                }

                SelectList s1 = new SelectList(qurey1, "Id", "FullDetails", selected1);

                return Json(s1, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult Create(AttendanceLeavePriority c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string LvHead = form["LeaveHeadlist"] == "0" ? "" : form["LeaveHeadlist"];
                    string sequenceNo = form["Seqno"] == "0" ? "" : form["Seqno"];
                   
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var companypayroll = new CompanyPayroll();
                    companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == company_Id).SingleOrDefault();

                    if (LvHead != null && LvHead != "")
                    {
                        var val = db.LvHead.Find(int.Parse(LvHead));
                        c.LvHead_Id = val.Id;
                    }
                    if (sequenceNo != null && sequenceNo != "")
                    {
                        int SEQno = Convert.ToInt32(sequenceNo);
                        c.Seqno = SEQno;
                    }
                    if (LvHead != null && LvHead != "")
                    {
                        var val1 = db.LvHead.Find(int.Parse(LvHead));
                        if (val1.LvCode.ToUpper() == "LWP")
                        {
                            Msg.Add("Please don't add LWP leave!!..");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
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

                            AttendanceLeavePriority OBJ_AttendLvPriority = new AttendanceLeavePriority()
                            {

                                LvHead_Id = c.LvHead_Id,
                                Seqno = c.Seqno,
                                
                                DBTrack = c.DBTrack,

                            };
                            try
                            {
                                db.AttendanceLeavePriority.Add(OBJ_AttendLvPriority);

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


        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.AttendanceLeavePriority

                            .Include(e => e.AttendanceActionPolicyFormula)
                           
                                  .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        //AttendanceActionPolicyFormula = e.AttendanceActionPolicyFormula != null ? e.AttendanceActionPolicyFormula : null,
                        LvHead_Id = e.LvHead_Id,
                        Seqno = e.Seqno,
                        

                    }).ToList();




                return Json(new Object[] { returndata, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(AttendanceLeavePriority AttendLvP, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {


                        string LeaveHeadlistType = form["LeaveHeadlist"] == "0" ? "" : form["LeaveHeadlist"];
                        string Seqno = form["Seqno"] == "0" ? "" : form["Seqno"];

                        if (LeaveHeadlistType != null && LeaveHeadlistType != "")
                        {
                            int Lvids = Convert.ToInt32(LeaveHeadlistType);
                            LvHead LvHeadq = db.LvHead.Find(Lvids);
                            AttendLvP.LvHead_Id = LvHeadq.Id;
                        }

                        if (Seqno != null && Seqno != "")
                        {
                            var Sequence_no = Convert.ToInt32(Seqno);
                            AttendLvP.Seqno = Sequence_no;
                        }

                        //var Geodata = db.ExpenseBudget.Include(e => e.GeoStruct).Where(e => e.Id == data).Select(g => g.GeoStruct).FirstOrDefault();

                        var db_data = db.AttendanceLeavePriority.Where(e => e.Id == data).SingleOrDefault();
                        TempData["RowVersion"] = db_data.RowVersion;
                        AttendanceLeavePriority AttLeavePrioritydata = db.AttendanceLeavePriority.Find(data);
                        TempData["CurrRowVersion"] = AttLeavePrioritydata.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            AttendLvP.DBTrack = new DBTrack
                            {
                                CreatedBy = AttLeavePrioritydata.DBTrack.CreatedBy == null ? null : AttLeavePrioritydata.DBTrack.CreatedBy,
                                CreatedOn = AttLeavePrioritydata.DBTrack.CreatedOn == null ? null : AttLeavePrioritydata.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            AttLeavePrioritydata.Id = data;
                            AttLeavePrioritydata.LvHead_Id = AttendLvP.LvHead_Id;
                            AttLeavePrioritydata.Seqno = AttendLvP.Seqno;
                            AttLeavePrioritydata.DBTrack = AttendLvP.DBTrack;


                            db.Entry(AttLeavePrioritydata).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        ts.Complete();
                        Msg.Add(" Record Updated Successfully. ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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



        public class P2BGridData
        {
            public int Id { get; set; }
            public string LvHead_Id { get; set; }
            public int Seqno { get; set; }

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

                    var BindCompList = db.AttendanceLeavePriority.ToList();



                    foreach (var s in BindCompList)
                    {
                        if (s != null)
                        {
                            view = new P2BGridData()
                            {
                                Id = s.Id,
                                LvHead_Id = db.LvHead.Where(e => e.Id == s.LvHead_Id).Select(a => a.LvCode).SingleOrDefault(),
                                Seqno = s.Seqno,

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
                               
                                || (e.LvHead_Id.ToString().Contains(gp.searchString))
                                || (e.Seqno.ToString().Contains(gp.searchString))

                                ).Select(a => new Object[] { a.Seqno, Convert.ToString(a.LvHead_Id), a.Id }).ToList();

                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Seqno, Convert.ToString(a.LvHead_Id), a.Id }).ToList();
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
                            orderfuc = (c => gp.sidx == "SequenceNo" ? c.Seqno.ToString() :
                                             gp.sidx == "LeaveHeadID" ? c.LvHead_Id.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Seqno, Convert.ToString(a.LvHead_Id), a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Seqno, Convert.ToString(a.LvHead_Id), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Seqno, Convert.ToString(a.LvHead_Id), a.Id }).ToList();
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
                    AttendanceLeavePriority AttenLeavePrioritys = db.AttendanceLeavePriority

                                                     .Where(e => e.Id == data).SingleOrDefault();

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {



                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = AttenLeavePrioritys.DBTrack.CreatedBy != null ? AttenLeavePrioritys.DBTrack.CreatedBy : null,
                            CreatedOn = AttenLeavePrioritys.DBTrack.CreatedOn != null ? AttenLeavePrioritys.DBTrack.CreatedOn : null,
                            IsModified = AttenLeavePrioritys.DBTrack.IsModified == true ? false : false//,

                        };
                        db.Entry(AttenLeavePrioritys).State = System.Data.Entity.EntityState.Deleted;
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