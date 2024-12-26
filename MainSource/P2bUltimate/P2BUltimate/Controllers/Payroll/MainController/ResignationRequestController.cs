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
    public class ResignationRequestController : Controller
    {
        //
        // GET: /ResignationRequest/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ResignationRequest/Index.cshtml");
        }

        public class Getpolicydate //childgrid
        {

            public string LastWorkingDateByPolicy { get; set; }
           
        }
        [HttpPost]
        public ActionResult GetPolicyDate(FormCollection form, int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    

                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string ResignDate = form["ResignDate"] == "0" ? "" : form["ResignDate"];
                    DateTime reqdatec = Convert.ToDateTime(ResignDate);
                    DateTime LastWorkingDateByPolicy = DateTime.Now;
                    var EmployeeSeperationStruct = db.EmployeeSeperationStruct
                        .Include(e=>e.EmployeeExit)
                        .Include(e => e.EmployeeExit.Employee)
                               .Include(e => e.EmployeeSeperationStructDetails)
                               .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationMaster))
                                .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula))
                                .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula.NoticePeriod_Object))
                               .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment))
                               .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment.PayScaleAgreement))
                               .Where(e => e.EmployeeExit.Employee.Id == Emp && e.EndDate == null).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();
                    if (EmployeeSeperationStruct != null)
                    {
                        var OEmployeeSeperationStructDet = EmployeeSeperationStruct.EmployeeSeperationStructDetails.Where(r => r.SeperationPolicyFormula.NoticePeriod_Object.Count() > 0).FirstOrDefault();
                        if (OEmployeeSeperationStructDet != null)
                        {
                            var exitRetirepolicyday = OEmployeeSeperationStructDet.SeperationPolicyFormula.NoticePeriod_Object.FirstOrDefault();
                            LastWorkingDateByPolicy = Convert.ToDateTime(reqdatec.AddMonths(exitRetirepolicyday.NoticePeriod)).Date;
                           
                        }
                    }


                    List<Getpolicydate> returndata = new List<Getpolicydate>();

                    returndata.Add(new Getpolicydate
                    {
                        LastWorkingDateByPolicy = LastWorkingDateByPolicy.ToShortDateString()
                       
                    });
                    return Json(returndata, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public ActionResult Create(ResignationRequest c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                List<String> Msg = new List<String>();
                try
                {
                    string Reasonofleave = form["ReasonOfLeaving"] == "0" ? "" : form["ReasonOfLeaving"];
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);

                    if (Reasonofleave == null && Reasonofleave == "")
                    {
                        Msg.Add("  Please Enter Reason of Leaving.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    var a = db.EmployeeExit.Include(e => e.Employee)
                        .Include(e => e.SeperationProcessT)
                .Include(e => e.SeperationProcessT.ResignationRequest)
                .Where(e => e.Employee.Id == Emp).AsNoTracking().FirstOrDefault();

                    var test = a.SeperationProcessT != null && a.SeperationProcessT.ResignationRequest != null ? a.SeperationProcessT.ResignationRequest.ReasonOfLeaving.ToString() : null;


                    if (test != null)
                    {
                        Msg.Add("Record Already Exists.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    var EmployeeSeperationStruct = db.EmployeeSeperationStruct
                         .Include(e => e.EmployeeExit)
                        .Include(e => e.EmployeeExit.Employee)
                                 .Include(e => e.EmployeeSeperationStructDetails)
                                 .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationMaster))
                                  .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula))
                                  .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula.NoticePeriod_Object))
                                 .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment))
                                 .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment.PayScaleAgreement))
                                 .Where(e => e.EmployeeExit.Employee.Id == Emp && e.EndDate == null).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();
                    if (EmployeeSeperationStruct != null)
                    {
                        var OEmployeeSeperationStructDet = EmployeeSeperationStruct.EmployeeSeperationStructDetails.Where(r => r.SeperationPolicyFormula.NoticePeriod_Object.Count() > 0).FirstOrDefault();
                        if (OEmployeeSeperationStructDet != null)
                        {
                            var exitRetirepolicyday = OEmployeeSeperationStructDet.SeperationPolicyFormula.NoticePeriod_Object.FirstOrDefault();
                            DateTime resigndate = c.ResignDate;
                            DateTime Empenterpolicydt = c.LastWorkingDateByPolicy;
                            DateTime actRetdate = Convert.ToDateTime(resigndate.AddMonths(exitRetirepolicyday.NoticePeriod));
                            if (Empenterpolicydt!=actRetdate)
                            {
                                Msg.Add("Company policy date not match which you have enter last workig day policy. compant policy date is " + actRetdate.Date.ToShortDateString());
                                 return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            ResignationRequest sep = new ResignationRequest()
                            {
                                ReasonOfLeaving = c.ReasonOfLeaving,
                                Narration = c.Narration,
                                RequestAckDate = c.RequestAckDate,
                                LastWorkingDateByEmp = c.LastWorkingDateByEmp,
                                LastWorkingDateByPolicy = c.LastWorkingDateByPolicy,
                                ResignDate = c.ResignDate,
                                IsRequestAck = c.IsRequestAck,
                                IsReadyForNoticePeriodPay = c.IsReadyForNoticePeriodPay,
                                IsRequestForNoticePeriodWaiveOff = c.IsRequestForNoticePeriodWaiveOff,
                                DBTrack = c.DBTrack,
                                TrClosed = true,
                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                            };

                            db.ResignationRequest.Add(sep);
                            db.SaveChanges();

                            EmployeeExit OEmployeePayroll = null;
                            OEmployeePayroll
                               = db.EmployeeExit
                             .Where(e => e.Employee.Id == Emp).SingleOrDefault();

                            //List<ResignationRequest> OFAT = new List<ResignationRequest>();
                            //OFAT.Add(db.ResignationRequest.Find(sep.Id));
                          //  var aa = db.EmployeeExit.Include(x=>x.SeperationProcessT).Find(OEmployeePayroll.Id);
                            var aa = db.EmployeeExit.Include(x => x.SeperationProcessT).Where(e=>e.Employee.Id==Emp).FirstOrDefault();
                            if (aa.SeperationProcessT == null)
                            {
                                SeperationProcessT OTEP = new SeperationProcessT()
                                {
                                    ResignationRequest = sep,
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
                                aa.SeperationProcessT.ResignationRequest = sep;
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


        public ActionResult Edit(int data, string param)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.ResignationRequest.Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {
                        ReasonOfLeaving = e.ReasonOfLeaving,
                        Narration = e.Narration,
                        RequestAckDate = e.RequestAckDate,
                        LastWorkingDateByEmp = e.LastWorkingDateByEmp,
                        LastWorkingDateByPolicy = e.LastWorkingDateByPolicy,
                        ResignDate = e.ResignDate,
                        IsRequestAck = e.IsRequestAck,
                        IsReadyForNoticePeriodPay = e.IsReadyForNoticePeriodPay,
                        IsRequestForNoticePeriodWaiveOff = e.IsRequestForNoticePeriodWaiveOff,
                    }).ToList();
                return Json(new Object[] { returndata, "", "", "", JsonRequestBehavior.AllowGet });
            }
        }


        public async Task<ActionResult> EditSave(ResignationRequest c, int data, FormCollection form, string param) // Edit submit
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
                    if (Auth == false)
                    {

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    ResignationRequest blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ResignationRequest.Where(e => e.Id == data)
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
                                    var m1 = db.ResignationRequest
                                         .Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.ResignationRequest.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);
                                    var CurCorp = db.ResignationRequest.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {


                                        ResignationRequest corp = new ResignationRequest()
                                        {
                                            ReasonOfLeaving = c.ReasonOfLeaving,
                                            Narration = c.Narration,
                                            RequestAckDate = c.RequestAckDate,
                                            LastWorkingDateByEmp = c.LastWorkingDateByEmp,
                                            LastWorkingDateByPolicy = c.LastWorkingDateByPolicy,
                                            ResignDate = c.ResignDate,
                                            IsRequestAck = c.IsRequestAck,
                                            IsReadyForNoticePeriodPay = c.IsReadyForNoticePeriodPay,
                                            IsRequestForNoticePeriodWaiveOff = c.IsRequestForNoticePeriodWaiveOff,
                                            Id = data,
                                            TrClosed = true,
                                            WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                            DBTrack = c.DBTrack
                                        };

                                        db.ResignationRequest.Attach(corp);
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
                                        Val = c.ReasonOfLeaving,
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
            public string ResignDate { get; set; }
            public string EmplastWorkdate { get; set; }
            public string CompPolicyLastWorkdate { get; set; }


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

                //   IEnumerable<ResignationRequest> Corporate = null;

                var empresig = db.EmployeeExit.Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
                    .Include(e => e.SeperationProcessT.ResignationRequest)
                    .AsNoTracking().ToList();

                foreach (var z in empresig)
                {
                    if (z.SeperationProcessT != null && z.SeperationProcessT.ResignationRequest != null)
                    {
                        view = new P2BGridData()
                           {
                               Id = z.SeperationProcessT.ResignationRequest.Id,
                               Code = z.Employee.EmpCode,
                               Name = z.Employee.EmpName.FullNameFML,
                               ResignDate = z.SeperationProcessT.ResignationRequest.ResignDate.ToShortDateString(),
                               EmplastWorkdate = z.SeperationProcessT.ResignationRequest.LastWorkingDateByEmp.ToShortDateString(),
                               CompPolicyLastWorkdate = z.SeperationProcessT.ResignationRequest.LastWorkingDateByPolicy.ToShortDateString(),
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
                              || (e.ResignDate != null ? e.ResignDate.ToString().Contains(gp.searchString) : false)
                             || (e.EmplastWorkdate != null ? e.EmplastWorkdate.ToString().Contains(gp.searchString) : false)
                              || (e.CompPolicyLastWorkdate != null ? e.CompPolicyLastWorkdate.ToString().Contains(gp.searchString) : false)

                             || (e.Id.ToString().Contains(gp.searchString))
                             ).Select(a => new Object[] { a.Code, a.Name, a.ResignDate != null ? Convert.ToString(a.ResignDate) : "", a.EmplastWorkdate != null ? Convert.ToString(a.EmplastWorkdate) : "", a.CompPolicyLastWorkdate != null ? Convert.ToString(a.CompPolicyLastWorkdate) : "", a.Id }).ToList();



                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.ResignDate != null ? Convert.ToString(a.ResignDate) : "", a.EmplastWorkdate != null ? Convert.ToString(a.EmplastWorkdate) : "", a.CompPolicyLastWorkdate != null ? Convert.ToString(a.CompPolicyLastWorkdate) : "", a.Id }).ToList();
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
                                         gp.sidx == "ResignDate" ? c.ResignDate.ToString() :
                                         gp.sidx == "EmplastWorkdate" ? c.EmplastWorkdate.ToString() :
                                         gp.sidx == "CompPolicyLastWorkdate" ? c.CompPolicyLastWorkdate.ToString() :

                                    "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.ResignDate != null ? Convert.ToString(a.ResignDate) : "", a.EmplastWorkdate != null ? Convert.ToString(a.EmplastWorkdate) : "", a.CompPolicyLastWorkdate != null ? Convert.ToString(a.CompPolicyLastWorkdate) : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.ResignDate != null ? Convert.ToString(a.ResignDate) : "", a.EmplastWorkdate != null ? Convert.ToString(a.EmplastWorkdate) : "", a.CompPolicyLastWorkdate != null ? Convert.ToString(a.CompPolicyLastWorkdate) : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.ResignDate != null ? Convert.ToString(a.ResignDate) : "", a.EmplastWorkdate != null ? Convert.ToString(a.EmplastWorkdate) : "", a.CompPolicyLastWorkdate != null ? Convert.ToString(a.CompPolicyLastWorkdate) : "", a.Id }).ToList();
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
                        ResignationRequest ResignationRequest = db.ResignationRequest
                                                            .Where(e => e.Id == data)
                                                           .SingleOrDefault();
                        SeperationProcessT SeperationProcess = db.SeperationProcessT.Include(e => e.ResignationRequest)
                                                            .Where(e => e.ResignationRequest.Id == data)
                                    .SingleOrDefault();

                        if (SeperationProcess != null)
                        {
                            SeperationProcess.ResignationRequest = null;
                            db.SeperationProcessT.Attach(SeperationProcess);
                            db.Entry(SeperationProcess).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ResignationRequest).State = System.Data.Entity.EntityState.Deleted;

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