using P2b.Global;
using P2BUltimate.Models;
using Payroll;
using Leave;
using EMS;
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
using Newtonsoft.Json;
using P2BUltimate.Process;

namespace P2BUltimate.Controllers
{
    public class FFSCheckListComplianceController : Controller
    {
        //
        // GET: /NoticePeriodProcess/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/FFSCheckListCompliance/Index.cshtml");
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


        public ActionResult Create(FFSCheckListCompliance c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                List<String> Msg = new List<String>();
                try
                {
                    string Remark = form["Remark"] == "0" ? "" : form["Remark"];
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);

                    if (Remark == null && Remark == "")
                    {
                        Msg.Add("  Please Enter Reason of Leaving.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }




                    string ddlNoticePeriodlist = form["NoticePeriodlist"] == "" ? null : form["NoticePeriodlist"];
                    List<int> CheckListobjidchk = null;
                    if (ddlNoticePeriodlist != null && ddlNoticePeriodlist != "")
                    {
                        CheckListobjidchk = Utility.StringIdsToListIds(ddlNoticePeriodlist);

                        var a = db.EmployeeExit.Include(e => e.Employee)
                       .Include(e => e.SeperationProcessT)
               .Include(e => e.SeperationProcessT.FFSCheckListCompliance)
                .Include(e => e.SeperationProcessT.FFSCheckListCompliance.Select(x => x.ExitProcess_CheckList_Object))
               .Where(e => e.Employee.Id == Emp).AsNoTracking().FirstOrDefault();
                        if (a.SeperationProcessT != null)
                        {
                            var test = a.SeperationProcessT.FFSCheckListCompliance.ToList();
                            foreach (var chkob in test)
                            {
                                foreach (var chkobjt in CheckListobjidchk)
                                {
                                    if (chkobjt == chkob.ExitProcess_CheckList_Object.Id)
                                    {
                                        Msg.Add(chkob.ExitProcess_CheckList_Object.CheckListItemDesc + ", is entered.Please remove from list,");

                                    }
                                }

                            }
                        }
                    }
                    if (Msg.Count() > 0)
                    {
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    List<int> CheckListobjid = null;
                    List<FFSCheckListCompliance> OFFSList = new List<FFSCheckListCompliance>();
                    c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };


                    if (ModelState.IsValid)
                    {

                        using (TransactionScope ts = new TransactionScope())
                        {
                            try
                            {
                                if (ddlNoticePeriodlist != null && ddlNoticePeriodlist != "")
                                {
                                    CheckListobjid = Utility.StringIdsToListIds(ddlNoticePeriodlist);
                                    foreach (var item in CheckListobjid)
                                    {
                                        FFSCheckListCompliance ObjFAT = new FFSCheckListCompliance()
                                        {
                                            Remark = c.Remark,
                                            IsDone = c.IsDone,
                                            ExitProcess_CheckList_Object = db.ExitProcess_CheckList_Object.Find(item),
                                            DBTrack = c.DBTrack,

                                        };

                                        db.FFSCheckListCompliance.Add(ObjFAT);
                                        db.SaveChanges();
                                        OFFSList.Add(ObjFAT);

                                    }

                                }
                                var aa = db.EmployeeExit.Include(x => x.SeperationProcessT).Include(e => e.SeperationProcessT.FFSCheckListCompliance).Where(e => e.Employee.Id == Emp).FirstOrDefault();
                                if (aa.SeperationProcessT == null)
                                {
                                    SeperationProcessT OTEP = new SeperationProcessT()
                                    {
                                        FFSCheckListCompliance = OFFSList,
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
                                    OFFSList.AddRange(aa.SeperationProcessT.FFSCheckListCompliance);
                                    aa.SeperationProcessT.FFSCheckListCompliance = OFFSList;
                                    db.EmployeeExit.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                }
                                ts.Complete();

                            }
                            catch (Exception ex)
                            {
                                //   List<string> Msg = new List<string>();
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
                                //return Json(new { sucess = false, Msg }, JsonRequestBehavior.AllowGet);
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }


                        }
                        Msg.Add("Data Saved successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
                        Msg.Add("Code Already Exists.");
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
                              .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula.ExitProcess_CheckList_Policy))
                              .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula.ExitProcess_CheckList_Policy.Select(x => x.ExitProcess_CheckList_Object)))
                              .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula.ExitProcess_CheckList_Policy.Select(x => x.ExitProcess_CheckList_Object.Select(z => z.CheckListItem))))
                             .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment))
                             .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment.PayScaleAgreement))
                             .Where(e => e.EmployeeExit.Employee.Id == empids && e.EndDate == null).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();
                IEnumerable<NoticePeriod_Object> all;
                List<returnClass> oreturnClass = new List<returnClass>();
                if (EmployeeSeperationStruct != null)
                {
                    var OEmployeeSeperationStructDet = EmployeeSeperationStruct.EmployeeSeperationStructDetails.Where(r => r.SeperationPolicyFormula.ExitProcess_CheckList_Policy.Count() > 0).FirstOrDefault();
                    if (OEmployeeSeperationStructDet != null)
                    {
                        var fall = OEmployeeSeperationStructDet.SeperationPolicyFormula.ExitProcess_CheckList_Policy.ToList();


                        // string hdate = "";



                        foreach (var Noticeperiod in fall)
                        {

                            var CheckList_Object = Noticeperiod.ExitProcess_CheckList_Object.ToList();
                            foreach (var item in CheckList_Object)
                            {
                                var calendar = " Code :" + item.CheckListItem.LookupVal.ToUpper() + " Desc :" + item.CheckListItemDesc;
                                oreturnClass.Add(new returnClass
                                {
                                    srno = item.Id.ToString(),
                                    lookupvalue = calendar
                                });
                            }

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
                var returndata = db.FFSCheckListCompliance.Include(e => e.ExitProcess_CheckList_Object).Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {
                        Remark = e.Remark,
                        IsDone = e.IsDone,
                       
                    }).ToList();
                List<ExitProcess_CheckList_Object> ExitProcess_CheckList_Object = new List<ExitProcess_CheckList_Object>();
                var noticeobject = db.FFSCheckListCompliance.Include(e => e.ExitProcess_CheckList_Object).Where(e => e.Id == data).ToList();

                if (noticeobject != null)
                {
                    foreach (var item in noticeobject)
                    {

                        ExitProcess_CheckList_Object.Add(new ExitProcess_CheckList_Object
                        {
                            CheckListItemDesc = item.ExitProcess_CheckList_Object.CheckListItemDesc == null ? "" : item.ExitProcess_CheckList_Object.CheckListItemDesc,
                            Id = item.ExitProcess_CheckList_Object.Id == null ? 0 : item.ExitProcess_CheckList_Object.Id


                        });


                    }

                }
                return Json(new Object[] { returndata, ExitProcess_CheckList_Object, "", "", JsonRequestBehavior.AllowGet });
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
                   .Where(e => e.Id == Emp_Id).FirstOrDefault();
                if (empresig != null)
                {
                    if (empresig.SeperationProcessT != null && empresig.SeperationProcessT.ResignationRequest != null)
                    {
                        var b = empresig.SeperationProcessT.ResignationRequest.LastWorkingDateByPolicy.Date.ToShortDateString();
                        return Json(b, JsonRequestBehavior.AllowGet);
                    }

                }


            }
            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            // return Json(null, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> EditSave(FFSCheckListCompliance c, int data, FormCollection form, string param) // Edit submit
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
                            var val = db.ExitProcess_CheckList_Object
                                                .Where(e => e.Id == AddId).SingleOrDefault();
                            c.ExitProcess_CheckList_Object = val;
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
                                    FFSCheckListCompliance blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.FFSCheckListCompliance.Where(e => e.Id == data)
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
                                    var m1 = db.FFSCheckListCompliance
                                         .Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.FFSCheckListCompliance.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    var CurCorp = db.FFSCheckListCompliance.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {


                                        FFSCheckListCompliance corp = new FFSCheckListCompliance()
                                        {
                                            ExitProcess_CheckList_Object = c.ExitProcess_CheckList_Object,
                                            Remark = c.Remark,
                                            IsDone = c.IsDone,
                                            Id = data,

                                            DBTrack = c.DBTrack
                                        };

                                        db.FFSCheckListCompliance.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;
                                    }

                                    using (var context = new DataBaseContext())
                                    {
                                      
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass
                                    {
                                        Id = c.Id,
                                        Val = c.Remark,
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
            public string CheckListItemDesc { get; set; }
            public string IsDone { get; set; }


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
                    .Include(e => e.SeperationProcessT.FFSCheckListCompliance)
                     .Include(e => e.SeperationProcessT.FFSCheckListCompliance.Select(x => x.ExitProcess_CheckList_Object))
                    .AsNoTracking().ToList();

                foreach (var z in empresig)
                {
                    if (z.SeperationProcessT != null && z.SeperationProcessT.FFSCheckListCompliance != null)
                    {
                        var chkobj = z.SeperationProcessT.FFSCheckListCompliance.ToList();
                        foreach (var item in chkobj)
                        {
                            view = new P2BGridData()
                            {
                                Id = item.Id,
                                Code = z.Employee.EmpCode,
                                Name = z.Employee.EmpName.FullNameFML,
                                CheckListItemDesc = item.ExitProcess_CheckList_Object.CheckListItemDesc.ToString(),
                                IsDone = item.IsDone==true ? "Yes" :"No"

                            };
                            model.Add(view);
                        }
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
                              || (e.CheckListItemDesc != null ? e.CheckListItemDesc.ToString().Contains(gp.searchString) : false)
                             || (e.IsDone != null ? e.IsDone.ToString().Contains(gp.searchString) : false)

                             || (e.Id.ToString().Contains(gp.searchString))
                             ).Select(a => new Object[] { a.Code, a.Name, a.CheckListItemDesc != null ? Convert.ToString(a.CheckListItemDesc) : "", a.IsDone != null ? Convert.ToString(a.IsDone) : "", a.Id }).ToList();



                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.CheckListItemDesc != null ? Convert.ToString(a.CheckListItemDesc) : "", a.IsDone != null ? Convert.ToString(a.IsDone) : "", a.Id }).ToList();
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
                                         gp.sidx == "CheckListItemDesc" ? c.CheckListItemDesc.ToString() :
                                         gp.sidx == "IsDone" ? c.IsDone.ToString() :


                                    "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.CheckListItemDesc != null ? Convert.ToString(a.CheckListItemDesc) : "", a.IsDone != null ? Convert.ToString(a.IsDone) : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.CheckListItemDesc != null ? Convert.ToString(a.CheckListItemDesc) : "", a.IsDone != null ? Convert.ToString(a.IsDone) : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.CheckListItemDesc != null ? Convert.ToString(a.CheckListItemDesc) : "", a.IsDone != null ? Convert.ToString(a.IsDone) : "", a.Id }).ToList();
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
                        FFSCheckListCompliance FFSCheckListCompliance = db.FFSCheckListCompliance
                                                            .Where(e => e.Id == data)
                                                           .SingleOrDefault();
                        //SeperationProcessT SeperationProcess = db.SeperationProcessT.Include(e => e.NoticePeriodProcess)
                        //                                    .Where(e => e.NoticePeriodProcess.Id == data)
                        //            .SingleOrDefault();

                        if (FFSCheckListCompliance != null)
                        {

                            db.Entry(FFSCheckListCompliance).State = System.Data.Entity.EntityState.Deleted;

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