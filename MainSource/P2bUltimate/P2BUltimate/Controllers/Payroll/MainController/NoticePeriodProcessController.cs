using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
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
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Payroll;
using EMS;
using Training;
using Attendance;
using Leave;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers
{
    public class NoticePeriodProcessController : Controller
    {
        //
        // GET: /NoticePeriodProcess/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/NoticePeriodProcess/Index.cshtml");
        }

        public class Getpolicydate //childgrid
        {

            public string LastWorkDateApproved { get; set; }

        }
        [HttpPost]
        public ActionResult GetPolicyDate(FormCollection form, int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string LastworkdaycompDate = form["LastWorkDateByComp"] == "0" ? "" : form["LastWorkDateByComp"];
                    string waiveday = form["WaiveDays"] == "0" ? "" : form["WaiveDays"];
                    int waived = Convert.ToInt32(waiveday);
                    DateTime reqdatec = Convert.ToDateTime(LastworkdaycompDate);
                   
                    List<Getpolicydate> returndata = new List<Getpolicydate>();

                    returndata.Add(new Getpolicydate
                    {
                        LastWorkDateApproved = Convert.ToDateTime(reqdatec.AddDays(-(waived))).ToShortDateString()

                    });
                    return Json(returndata, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }


        public ActionResult Create(NoticePeriodProcess c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                List<String> Msg = new List<String>();
                try
                {
                    string Reasonofleave = form["Reason"] == "0" ? "" : form["Reason"];
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);

                    if (Reasonofleave == null && Reasonofleave == "")
                    {
                        Msg.Add("  Please Enter Reason of Leaving.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    var a = db.EmployeeExit.Include(e => e.Employee)
                        .Include(e => e.SeperationProcessT)
                .Include(e => e.SeperationProcessT.NoticePeriodProcess)
                .Where(e => e.Employee.Id == Emp).AsNoTracking().FirstOrDefault();

                    var test = a.SeperationProcessT != null && a.SeperationProcessT.NoticePeriodProcess != null ? a.SeperationProcessT.NoticePeriodProcess.Reason.ToString() : null;


                    if (test != null)
                    {
                        Msg.Add("Record Already Exists.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    string ddlNoticePeriodlist = form["NoticePeriodlist"] == "" ? null : form["NoticePeriodlist"];

                    if (ddlNoticePeriodlist != null && ddlNoticePeriodlist != "")
                    {
                        var id = Convert.ToInt32(ddlNoticePeriodlist);
                        var val = db.NoticePeriod_Object.Find(id);
                        c.NoticePeriod_Object = val;
                    }

                    //var EmployeeSeperationStruct = db.EmployeeSeperationStruct
                    //             .Include(e => e.EmployeeSeperationStructDetails)
                    //             .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationMaster))
                    //              .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula))
                    //              .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula.NoticePeriod_Object))
                    //             .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment))
                    //             .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment.PayScaleAgreement))
                    //             .Where(e => e.EmployeeExit.Id == Emp && e.EndDate == null).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();
                    //if (EmployeeSeperationStruct != null)
                    //{
                    //    var OEmployeeSeperationStructDet = EmployeeSeperationStruct.EmployeeSeperationStructDetails.Where(r => r.SeperationPolicyFormula.NoticePeriod_Object.Count() > 0).FirstOrDefault();
                    //    if (OEmployeeSeperationStructDet != null)
                    //    {
                    //        var exitRetirepolicyday = OEmployeeSeperationStructDet.SeperationPolicyFormula.NoticePeriod_Object.FirstOrDefault();
                    //        DateTime resigndate = c.ResignDate;
                    //        DateTime Empenterpolicydt = c.LastWorkingDateByPolicy;
                    //        DateTime actRetdate = Convert.ToDateTime(resigndate.AddMonths(exitRetirepolicyday.NoticePeriod));
                    //        if (Empenterpolicydt!=actRetdate)
                    //        {
                    //            Msg.Add("Company policy date not match which you have enter last workig day policy. compant policy date is " + actRetdate.Date.ToShortDateString());
                    //             return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //        }
                    //    }
                    //}

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            NoticePeriodProcess sep = new NoticePeriodProcess()
                            {
                                Reason = c.Reason,
                                WaiveDays = c.WaiveDays,
                                LeaveAdjustDays = c.LeaveAdjustDays,
                                LastWorkDateByComp = c.LastWorkDateByComp,
                                LastWorkDateApproved = c.LastWorkDateApproved,
                                IsNoticePeriodWaived = c.IsNoticePeriodWaived,
                                IsLeaveAdjustInNoticePeriodRecovery = c.IsLeaveAdjustInNoticePeriodRecovery,
                                NoticePeriod_Object=c.NoticePeriod_Object,
                                DBTrack = c.DBTrack

                            };

                            db.NoticePeriodProcess.Add(sep);
                            db.SaveChanges();

                            EmployeeExit OEmployeePayroll = null;
                            OEmployeePayroll
                               = db.EmployeeExit
                             .Where(e => e.Employee.Id == Emp).SingleOrDefault();

                            //List<NoticePeriodProcess> OFAT = new List<NoticePeriodProcess>();
                            //OFAT.Add(db.NoticePeriodProcess.Find(sep.Id));
                            //  var aa = db.EmployeeExit.Include(x=>x.SeperationProcessT).Find(OEmployeePayroll.Id);
                            var aa = db.EmployeeExit.Include(x => x.SeperationProcessT).Where(e => e.Employee.Id == Emp).FirstOrDefault();
                            if (aa.SeperationProcessT == null)
                            {
                                SeperationProcessT OTEP = new SeperationProcessT()
                                {
                                    NoticePeriodProcess = sep,
                                    DBTrack = c.DBTrack
                                };


                                db.SeperationProcessT.Add(OTEP);

                                db.SaveChanges();

                                aa.SeperationProcessT = OTEP;
                                db.EmployeeExit.Attach(aa);
                                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                            }
                            else
                            {
                                aa.SeperationProcessT.NoticePeriodProcess = sep;
                                db.EmployeeExit.Attach(aa);
                                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                            }



                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                        //return this.Json(new { msg = errorMsg });
                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }

        }
        public class returnClass
        {
            public string srno { get; set; }
            public string lookupvalue { get; set; }
        }
        public ActionResult GetLookupDetailsNoticeobject(string data, string Empid)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<HolidayCalendar> fall = new List<HolidayCalendar>();
                int empids = Convert.ToInt32(Empid);

                var EmployeeSeperationStruct = db.EmployeeSeperationStruct
                    .Include(e => e.EmployeeExit)
                        .Include(e => e.EmployeeExit.Employee)
                             .Include(e => e.EmployeeSeperationStructDetails)
                             .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationMaster))
                              .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula))
                              .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula.NoticePeriod_Object))
                             .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment))
                             .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment.PayScaleAgreement))
                             .Where(e => e.EmployeeExit.Employee.Id == empids && e.EndDate == null).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();
                IEnumerable<NoticePeriod_Object> all;
                List<returnClass> oreturnClass = new List<returnClass>();
                if (EmployeeSeperationStruct != null)
                {
                    var OEmployeeSeperationStructDet = EmployeeSeperationStruct.EmployeeSeperationStructDetails.Where(r => r.SeperationPolicyFormula.NoticePeriod_Object.Count() > 0).FirstOrDefault();
                    if (OEmployeeSeperationStructDet != null)
                    {
                        var fall = OEmployeeSeperationStructDet.SeperationPolicyFormula.NoticePeriod_Object.ToList();


                        // string hdate = "";



                        foreach (var Noticeperiod in fall)
                        {


                            var calendar = " Name :" + Noticeperiod.PolicyName + " Notice Period in Month :" + Noticeperiod.NoticePeriod;
                            oreturnClass.Add(new returnClass
                            {
                                srno = Noticeperiod.Id.ToString(),
                                lookupvalue = calendar
                            });
                        }


                        return Json(oreturnClass, JsonRequestBehavior.AllowGet);


                    }

                }

                return Json(null, JsonRequestBehavior.AllowGet);

            }
            // return View();
        }


        public ActionResult Edit(int data, string param)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.NoticePeriodProcess.Include(e => e.NoticePeriod_Object).Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {

                        Reason = e.Reason,
                        WaiveDays = e.WaiveDays,
                        LeaveAdjustDays = e.LeaveAdjustDays,
                        LastWorkDateByComp = e.LastWorkDateByComp,
                        LastWorkDateApproved = e.LastWorkDateApproved,
                        IsNoticePeriodWaived = e.IsNoticePeriodWaived,
                        IsLeaveAdjustInNoticePeriodRecovery = e.IsLeaveAdjustInNoticePeriodRecovery,
                    }).ToList();
                List<NoticePeriod_Object> NoticePeriod_Object = new List<NoticePeriod_Object>();
                   var noticeobject = db.NoticePeriodProcess.Include(e => e.NoticePeriod_Object).Where(e => e.Id == data).ToList();

                if (noticeobject != null)
                {
                    foreach (var item in noticeobject)
                    {
                        
                    NoticePeriod_Object.Add(new NoticePeriod_Object
                    {
                        PolicyName = item.NoticePeriod_Object.PolicyName == null ? "" : item.NoticePeriod_Object.PolicyName,
                        Id =  item.NoticePeriod_Object.Id == null ? 0 : item.NoticePeriod_Object.Id
                      

                    });

                   
                    }

                }
                return Json(new Object[] { returndata, NoticePeriod_Object, "", "", JsonRequestBehavior.AllowGet });
            }
        }

        public JsonResult GetEmpLastworkdaybyComp(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var a = db.PayScaleAgreement.Find(int.Parse(data));
                int Emp_Id = int.Parse(data);
                var empresig = db.EmployeeExit.Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
                   .Include(e => e.SeperationProcessT.ResignationRequest)
                   .Where(e => e.Employee.Id == Emp_Id).FirstOrDefault();
                if (empresig != null)
                {
                    if (empresig.SeperationProcessT != null && empresig.SeperationProcessT.ResignationRequest != null)
                    {
                        var b = empresig.SeperationProcessT.ResignationRequest.LastWorkingDateByPolicy.Date.ToShortDateString();
                        return Json(b, JsonRequestBehavior.AllowGet);
                    }

                }


            }
           // return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> EditSave(NoticePeriodProcess c, int data, FormCollection form, string param) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //string typeofseparation = form["TypeOfSeperationList"] == "0" ? "" : form["TypeOfSeperationList"];
                    //string subtypeofseparation = form["SubTypeOfSeperation"] == "0" ? "" : form["SubTypeOfSeperation"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string Addrs = form["NoticePeriodlist"] == "0" ? "" : form["NoticePeriodlist"];
                    if (Addrs != null)
                    {
                        if (Addrs != "")
                        {
                            int AddId = Convert.ToInt32(Addrs);
                            var val = db.NoticePeriod_Object
                                                .Where(e => e.Id == AddId).SingleOrDefault();
                            c.NoticePeriod_Object = val;
                        }
                    }

                    if (Auth == false)
                    {

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    NoticePeriodProcess blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.NoticePeriodProcess.Where(e => e.Id == data)
                                                                .SingleOrDefault();
                                        //originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    var m1 = db.NoticePeriodProcess
                                         .Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.NoticePeriodProcess.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    var CurCorp = db.NoticePeriodProcess.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {


                                        NoticePeriodProcess corp = new NoticePeriodProcess()
                                        {
                                            NoticePeriod_Object = c.NoticePeriod_Object,
                                            Reason = c.Reason,
                                            WaiveDays = c.WaiveDays,
                                            LeaveAdjustDays = c.LeaveAdjustDays,
                                            LastWorkDateByComp = c.LastWorkDateByComp,
                                            LastWorkDateApproved = c.LastWorkDateApproved,
                                            IsNoticePeriodWaived = c.IsNoticePeriodWaived,
                                            IsLeaveAdjustInNoticePeriodRecovery = c.IsLeaveAdjustInNoticePeriodRecovery,
                                            Id = data,

                                            DBTrack = c.DBTrack
                                        };

                                        db.NoticePeriodProcess.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;
                                    }

                                    using (var context = new DataBaseContext())
                                    {
                                        //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //DT_ExitProcess_Process_Policy DT_Corp = (DT_IncrActivity)obj;
                                        //DT_Corp.IncrList_Id = blog.IncrList == null ? 0 : blog.IncrList.Id;
                                        //DT_Corp.IncrPolicy_Id = blog.IncrPolicy == null ? 0 : blog.IncrPolicy.Id;

                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass
                                    {
                                        Id = c.Id,
                                        Val = c.Reason,
                                        success = true,
                                        responseText = Msg
                                    }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (PromoActivity)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PromoActivity)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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

                return View();
            }
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string LastWorkDateByComp { get; set; }
            public string LastWorkDateApproved { get; set; }


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
                IEnumerable<P2BGridData> SalaryList = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;

                //   IEnumerable<NoticePeriodProcess> Corporate = null;

                var empresig = db.EmployeeExit.Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
                    .Include(e => e.SeperationProcessT.NoticePeriodProcess)
                    .AsNoTracking().ToList();

                foreach (var z in empresig)
                {
                    if (z.SeperationProcessT != null && z.SeperationProcessT.NoticePeriodProcess != null)
                    {
                        view = new P2BGridData()
                           {
                               Id = z.SeperationProcessT.NoticePeriodProcess.Id,
                               Code = z.Employee.EmpCode,
                               Name = z.Employee.EmpName.FullNameFML,
                               LastWorkDateByComp = z.SeperationProcessT.NoticePeriodProcess.LastWorkDateByComp.Value.Date.ToShortDateString(),
                               LastWorkDateApproved = z.SeperationProcessT.NoticePeriodProcess.LastWorkDateApproved.Value.Date.ToShortDateString(),

                           };
                        model.Add(view);
                    }
                }

                SalaryList = model;

                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = SalaryList;
                    if (gp.searchOper.Equals("eq"))
                    {

                        //jsonData = IE.Where(e => (e.Narration.ToUpper().ToString().Contains(gp.searchString.ToUpper()))                             
                        //      || (e.Id.ToString().Contains(gp.searchString))
                        //      ).Select(a => new Object[] { a.Narration, a.Id }).ToList();
                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                             || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                              || (e.LastWorkDateByComp != null ? e.LastWorkDateByComp.ToString().Contains(gp.searchString) : false)
                             || (e.LastWorkDateApproved != null ? e.LastWorkDateApproved.ToString().Contains(gp.searchString) : false)

                             || (e.Id.ToString().Contains(gp.searchString))
                             ).Select(a => new Object[] { a.Code, a.Name, a.LastWorkDateByComp != null ? Convert.ToString(a.LastWorkDateByComp) : "", a.LastWorkDateApproved != null ? Convert.ToString(a.LastWorkDateApproved) : "", a.Id }).ToList();



                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.LastWorkDateByComp != null ? Convert.ToString(a.LastWorkDateByComp) : "", a.LastWorkDateApproved != null ? Convert.ToString(a.LastWorkDateApproved) : "", a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = SalaryList;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
                                         gp.sidx == "Name" ? c.Name.ToString() :
                                         gp.sidx == "LastWorkDateByComp" ? c.LastWorkDateByComp.ToString() :
                                         gp.sidx == "LastWorkDateApproved" ? c.LastWorkDateApproved.ToString() :


                                    "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.LastWorkDateByComp != null ? Convert.ToString(a.LastWorkDateByComp) : "", a.LastWorkDateApproved != null ? Convert.ToString(a.LastWorkDateApproved) : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.LastWorkDateByComp != null ? Convert.ToString(a.LastWorkDateByComp) : "", a.LastWorkDateApproved != null ? Convert.ToString(a.LastWorkDateApproved) : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.LastWorkDateByComp != null ? Convert.ToString(a.LastWorkDateByComp) : "", a.LastWorkDateApproved != null ? Convert.ToString(a.LastWorkDateApproved) : "", a.Id }).ToList();
                    }
                    totalRecords = SalaryList.Count();
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



        public async Task<ActionResult> Delete(int data)
        {
            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                List<string> Msg = new List<string>();
                using (DataBaseContext db = new DataBaseContext())
                {
                    try
                    {
                        NoticePeriodProcess NoticePeriodProcess = db.NoticePeriodProcess
                                                            .Where(e => e.Id == data)
                                                           .SingleOrDefault();
                        SeperationProcessT SeperationProcess = db.SeperationProcessT.Include(e => e.NoticePeriodProcess)
                                                            .Where(e => e.NoticePeriodProcess.Id == data)
                                    .SingleOrDefault();

                        if (SeperationProcess != null)
                        {
                            SeperationProcess.NoticePeriodProcess = null;
                            db.SeperationProcessT.Attach(SeperationProcess);
                            db.Entry(SeperationProcess).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(NoticePeriodProcess).State = System.Data.Entity.EntityState.Deleted;

                            await db.SaveChangesAsync();
                            ts.Complete();
                        }
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });


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
}