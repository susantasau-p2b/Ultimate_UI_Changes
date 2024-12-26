using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
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

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class PerkTransTController : Controller
    {
        //
        // GET: /PerkTransT/
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/PerkTransT/Index.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_PerkTransGridPartial.cshtml");
        }

        public ActionResult PopulateDropDownSalHeadList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "PERK").ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PopulateDropDownSalHeadList1(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "PERK").ToList();
                var selected = (Object)null;
                //if (data2 != "" && data != "0" && data2 != "0")
                //{
                //    selected = Convert.ToInt32(data2);
                //}

                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }

        public class PerkTransTChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string SalaryHead { get; set; }
            public string PayMonth { get; set; }
            public double ActualAmount { get; set; }
            public double ProjectedAmount { get; set; }
        }
        public ActionResult ValidateForm(FormCollection form)
        {
            string LoanAdvhead = form["SalHeadlist"] == "0" ? "" : form["SalHeadlist"];
            int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
            List<string> Msg = new List<string>();


            if (LoanAdvhead == null)
            {
                Msg.Add("Please select salary head");
            }
            if (Emp == null)
            {
                Msg.Add("Please select employee");
            }

            if (Msg.Count > 0)
            {
                return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        public ActionResult Create(PerkTransT PerkTransT, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Salhead = form["SalHeadlist"] == "0" ? "" : form["SalHeadlist"];
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    if (Emp == 0)
                    {
                        Msg.Add("Please Select Employee");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    if (Salhead != null && Salhead != "")
                    {
                        var val = db.SalaryHead.Find(int.Parse(Salhead));
                        PerkTransT.SalaryHead = val;
                    }
                    else
                    {
                        Msg.Add("Please select SalaryHead");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    var CHK1 = db.EmployeePayroll.Include(e => e.PerkTransT)
                                .Include(e => e.PerkTransT.Select(q => q.SalaryHead)).Where(e => e.Employee.Id == Emp).SingleOrDefault();

                    var CHK2 = CHK1.PerkTransT.Where(e => e.SalaryHead.Id == PerkTransT.SalaryHead.Id && e.PayMonth == PerkTransT.PayMonth).ToList();
                    //var CHK = db.PerkTransT.Include(q => q.SalaryHead).Any(e => (e.SalaryHead.Id == PerkTransT.SalaryHead.Id) && (e.PayMonth == PerkTransT.PayMonth));
                    if (CHK2.Count() > 0)
                    {
                        Msg.Add(" The Perk already generated for this employee for this month.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                              .Where(r => r.Id == Emp).SingleOrDefault();

                    OEmployeePayroll
                    = db.EmployeePayroll.Include(e => e.EmpSalStruct)
                  .Where(e => e.Employee.Id == Emp).SingleOrDefault();

                    var SalT = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == PerkTransT.PayMonth).ToList();
                    if (SalT.Count() > 0)
                    {
                        Msg.Add(" Salary Already generated for " + " PayMonth :" + PerkTransT.PayMonth + "  " + " this Employee So that You Can not any changes");
                        return Json(new { status = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            PerkTransT.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            PerkTransT PerkTrans = new PerkTransT()
                            {
                                ActualAmount = PerkTransT.ActualAmount,
                                PayMonth = PerkTransT.PayMonth,
                                ProjectedAmount = PerkTransT.ProjectedAmount,
                                SalaryHead = PerkTransT.SalaryHead,
                                DBTrack = PerkTransT.DBTrack
                            };
                            try
                            {
                                db.PerkTransT.Add(PerkTrans);
                                db.SaveChanges();
                                List<PerkTransT> OFAT = new List<PerkTransT>();
                                OFAT.Add(db.PerkTransT.Find(PerkTrans.Id));

                                if (OEmployeePayroll == null)
                                {
                                    EmployeePayroll OTEP = new EmployeePayroll()
                                    {
                                        Employee = db.Employee.Find(OEmployee.Id),
                                        PerkTransT = OFAT,
                                        DBTrack = PerkTrans.DBTrack

                                    };
                                    db.EmployeePayroll.Add(OTEP);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                    if (aa.PerkTransT != null)
                                        OFAT.AddRange(aa.PerkTransT);
                                    aa.PerkTransT = OFAT;
                                    db.EmployeePayroll.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                }
                                ts.Complete();
                                List<string> Msgs = new List<string>();
                                Msgs.Add("Data Saved successfully");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = PerkTrans.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                                //    List<string> Msg = new List<string>();
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
                        //var errorMsg = sb.ToString();
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        List<string> MsgB = new List<string>();
                        var errorMsg = sb.ToString();
                        MsgB.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);
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
        //
        public class returnDataClass
        {

            public double ActualAmount { get; set; }
            public double ProjectedAmount { get; set; }

            public string SalaryHead { get; set; }

        }
        public ActionResult GridEditData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returnlist = new List<returnDataClass>();
                if (data != null && data != 0)
                {
                    var retrundataList = db.PerkTransT.Include(e => e.SalaryHead).Where(e => e.Id == data).ToList();
                    foreach (var a in retrundataList)
                    {
                        returnlist.Add(new returnDataClass()
                        {

                            ActualAmount = a.ActualAmount,
                            SalaryHead = a.SalaryHead.FullDetails != null ? a.SalaryHead.FullDetails : null,
                            ProjectedAmount = a.ProjectedAmount
                        });
                    }
                    return Json(new { returndata = returnlist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }

        public ActionResult GridEditSave1(PerkTransT LoanAdvReq, FormCollection form, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                var LoanAdvReqData = db.PerkTransT.Include(e => e.SalaryHead).Where(e => e.Id == Id).SingleOrDefault();
                //int ActualAmount = Convert.ToInt32(form["ActualAmount"]);
                //int ProjectedAmount = Convert.ToInt32(form["ProjectedAmount"]);
                string SalaryHead = form["SalaryHead"] == "0" ? "" : form["SalaryHead"];

                if (SalaryHead != "0")
                {
                    var id = Convert.ToInt32(SalaryHead);
                    var Branchdata = db.SalaryHead.Where(e => e.Id == id).SingleOrDefault();
                    LoanAdvReqData.SalaryHead = Branchdata;
                }


                using (TransactionScope ts = new TransactionScope())
                {

                    LoanAdvReqData.DBTrack = new DBTrack
                    {
                        CreatedBy = LoanAdvReqData.DBTrack.CreatedBy == null ? null : LoanAdvReqData.DBTrack.CreatedBy,
                        CreatedOn = LoanAdvReqData.DBTrack.CreatedOn == null ? null : LoanAdvReqData.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };


                    try
                    {
                        db.PerkTransT.Attach(LoanAdvReqData);
                        db.Entry(LoanAdvReqData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(LoanAdvReqData).State = System.Data.Entity.EntityState.Detached;
                        return Json(new { status = true, data = LoanAdvReqData, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {

                        return Json(new { status = false, responseText = e.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
                    }


                    try
                    {

                        if (LoanAdvReqData.Id != null)
                        {

                            PerkTransT perk = new PerkTransT()
                            {
                                ActualAmount = LoanAdvReq.ActualAmount,
                                ProjectedAmount = LoanAdvReq.ProjectedAmount,
                                SalaryHead = LoanAdvReqData.SalaryHead,
                                PayMonth = LoanAdvReqData.PayMonth,
                                DBTrack = LoanAdvReqData.DBTrack,
                                Id = Id

                            };

                            db.PerkTransT.Attach(perk);
                            db.Entry(perk).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            //  db.SaveChanges();
                            db.Entry(perk).State = System.Data.Entity.EntityState.Detached;
                        }


                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data Updated Successfully.", JsonRequestBehavior.AllowGet });
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = LoanAdvReq.Id });
                    }
                    catch (DataException /* dex */)
                    {
                        return this.Json(new { status = false, responseText = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }
        //

        public ActionResult GridEditSave(PerkTransT ypay, FormCollection from, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                var ids = Utility.StringIdsToListIds(data);
                Employee OEmployee = null;
                EmployeePayroll OEmployeePayroll = null;

                //OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                //                .Include(e => e.EmpOffInfo)
                //                .Where(r => r.Id == Emp).SingleOrDefault();
                int empidintrnal = Convert.ToInt32(ids[0]);
                int empidMain = Convert.ToInt32(ids[1]);
                OEmployeePayroll = db.EmployeePayroll.Include(q => q.SalaryT).Where(e => e.Id == empidMain).SingleOrDefault();
                var mnth = db.PerkTransT.Where(e => e.Id == empidintrnal).Select(q => q.PayMonth).SingleOrDefault();
                var calf = OEmployeePayroll.SalaryT.Any(e => e.PayMonth == mnth);
                if (calf == true)
                {
                    Msg.Add("  Salary Already generated for tHis Employee. Now you cannot make any changes. ");
                    return Json(new { status = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                if (data != null)
                {
                    var id = Convert.ToInt32(ids[0]);
                    var parent = Convert.ToInt32(ids[1]);
                    var db_data = db.PerkTransT.Include(e => e.SalaryHead).Where(e => e.Id == id).SingleOrDefault();

                    var OEmployeePayroll1 = db.EmployeePayroll.Include(e => e.PerkTransT).ToList();


                    foreach (var a in OEmployeePayroll1)
                    {
                        foreach (var b in a.PerkTransT)
                        {

                            if (b.Id == id)
                            {

                                var SalT = db.SalaryT.Where(e => e.EmployeePayroll_Id == a.Id && e.PayMonth == b.PayMonth).ToList();
                                if (SalT.Count() > 0)
                                {
                                    Msg.Add("Salary is released for this month. You can't change amount now..!");
                                    return Json(new { status = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new { status = false, data = db_data, responseText = "Salary is released for this month. You can't change amount now..!" }, JsonRequestBehavior.AllowGet);
                                    // return Json(new { status = false, responseText = "Salary is released for this month. You can't change amount now." }, JsonRequestBehavior.AllowGet);

                                }
                            }

                        }
                    }

                    db_data.ActualAmount = ypay.ActualAmount;
                    db_data.ProjectedAmount = ypay.ProjectedAmount;

                    try
                    {
                        db.PerkTransT.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {

                        return Json(new { status = false, responseText = e.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { status = false, responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                        .ToList();
                    // for searchs
                    IEnumerable<EmployeePayroll> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeePayroll, string> orderfunc = (c =>
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
                                Code = item.Employee != null ? item.Employee.EmpCode : null,
                                Name = item.Employee != null && item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                                JoiningDate = item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy") : null,
                                Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                                Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
                                Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null,
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
                    List<string> Msg = new List<string>();
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Get_PerkTransT(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.PerkTransT)
                        .Include(e => e.PerkTransT.Select(a => a.SalaryHead))
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data.PerkTransT != null)
                    {
                        List<PerkTransTChildDataClass> returndata = new List<PerkTransTChildDataClass>();

                        foreach (var item in db_data.PerkTransT)
                        {
                            returndata.Add(new PerkTransTChildDataClass
                            {
                                Id = item.Id,
                                SalaryHead = item.SalaryHead != null ? item.SalaryHead.Name : "",
                                PayMonth = item.PayMonth,
                                ActualAmount = item.ActualAmount,
                                ProjectedAmount = item.ProjectedAmount
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


        [HttpPost]
        public async Task<ActionResult> Delete(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListIds(data);
                List<string> Msg = new List<string>();
                //Employee OEmployee = null;
                EmployeePayroll OEmployeePayroll = null;

                //OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                //                .Include(e => e.EmpOffInfo)
                //                .Where(r => r.Id == Emp).SingleOrDefault();
                int empidintrnal = Convert.ToInt32(ids[0]);
                int empidMain = Convert.ToInt32(ids[1]);
                OEmployeePayroll = db.EmployeePayroll.Include(q => q.SalaryT).Where(e => e.Id == empidMain).SingleOrDefault();
                var mnth = db.PerkTransT.Where(e => e.Id == empidintrnal).Select(q => q.PayMonth).SingleOrDefault();
                var calf = OEmployeePayroll.SalaryT.Any(e => e.PayMonth == mnth);
                if (calf == true)
                {
                    Msg.Add("  Salary Already generated for tHis Employee. Now you cannot make any changes. ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                PerkTransT LoanAdvReq = db.PerkTransT.Include(e => e.SalaryHead).Where(e => e.Id == empidintrnal).SingleOrDefault();


                var selectedRegions = LoanAdvReq.Id;

                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {

                    try
                    {
                        //   db.PerkTransT.RemoveRange(selectedRegions);
                        db.PerkTransT.Remove(LoanAdvReq);

                        //   db.Entry(LoanAdvReq).State = System.Data.Entity.EntityState.Deleted;


                        await db.SaveChangesAsync();


                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data removed.", JsonRequestBehavior.AllowGet });

                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        //Log the error (uncomment dex variable name and add a line here to write a log.)
                        //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                        //return RedirectToAction("Delete");
                        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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