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
using P2BUltimate.Process;
using System.Diagnostics;
using P2BUltimate.Security;


namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class OtherEarningTController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/OtherEarningT/Index.cshtml");
        }

        //public ActionResult ChkProcess(string typeofbtn, string month, string EmpCode)
        //{
        //    bool selected = false;

        //    var query1 = db.EmployeePayroll.Include(e => e.OtherEarningDeductionT).ToList();
        //    foreach (var b in query1)
        //    {
        //        foreach (var c in b.OtherEarningDeductionT)
        //        {
        //            if (c.Id.ToString() == EmpCode)
        //            {
        //                var queryT = db.EmployeePayroll.Include(e => e.SalaryT).Where(e => e.Id == b.Id).SingleOrDefault();
        //                if (queryT != null)
        //                {
        //                    var query = queryT.SalaryT.Where(e => e.PayMonth == c.PayMonth).ToList();
        //                    if (query.Count > 0)
        //                    {
        //                        selected = true;
        //                    }
        //                   //var data = new
        //                   // {
        //                   //     status = selected,
        //                   // };
        //                   // return Json(data, JsonRequestBehavior.AllowGet);
        //                }

        //            }
        //        }

        //    }
        //    if (query1.Count > 0)
        //    {
        //        selected = false;
        //    }
        //    var data = selected;
        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult ChkProcess(string typeofbtn, string month, string EmpCode)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool selected = false;
                int Empcode = int.Parse(EmpCode);
                var d = db.EmployeePayroll.Include(a => a.SalaryT).Include(a => a.Employee).Where(a => a.Id == Empcode).FirstOrDefault();
                var d1 = d.SalaryT.Where(e => e.PayMonth == month).ToList();
                //   var query = db.SalaryT.Where(e => e.PayMonth == month).ToList();

                if (d1.Count > 0)
                {
                    selected = true;
                }
                else
                {
                    selected = false;
                }
                var data = new
                {
                    status = selected,
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.SalaryHead.Where(e => e.Type.LookupVal.ToUpper() == "EARNING" && e.SalHeadOperationType.LookupVal.ToUpper() == "NONREGULAR").ToList();

                // var qurey = db.SalaryHead.Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH").ToList();

                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_OtherEarningTGridPartial.cshtml");
            //E:\P2bUltimate-Saidapet\P2bUltimate\P2BUltimate\Views\Shared\Payroll\_LoanAdvRequestGridPartiall.cshtml
            //E:\P2bUltimate-Saidapet\P2bUltimate\P2BUltimate\Views\Shared\Payroll\_OtherEarningTGridPartial.cshtml

        }


        //public ActionResult GridEditSave(int data, OtherEarningT oOtherEarningT, FormCollection form)
        //{
        //    var qurey = db.OtherEarningT.Where(e => e.Id == data).SingleOrDefault();
        //    qurey.Amount = oOtherEarningT.Amount;
        //    qurey.TDS = oOtherEarningT.TDS;
        //    db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
        //    db.SaveChanges();
        //    db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
        //    return Json( new {status=true,responseText="Record Updated"},JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public ActionResult GetSalaryHeadDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.SalaryHead.Where(e => e.Type.LookupVal.ToUpper() == "EARNING" && e.SalHeadOperationType.LookupVal.ToUpper() == "NONREGULAR").ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.SalaryHead.Where(e => e.Frequency.LookupVal.ToUpper() == "HOURLY" || e.Frequency.LookupVal.ToUpper() == "DAILY").ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GridEditSave(OthEarningDeductionT ITP, FormCollection form, string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var TDSAmount = form["TDSAmount"] == " 0" ? "" : form["TDSAmount"];
                    var SalAmount = form["SalAmount"] == "0" ? "" : form["SalAmount"];
                    ITP.TDSAmount = Convert.ToDouble(TDSAmount);
                    ITP.SalAmount = Convert.ToDouble(SalAmount);

                    if (data != null)
                    {
                        var id = Convert.ToInt32(data);

                        int EmpPayrollId = 0;
                        OthEarningDeductionT db_data = null;
                        var OEmployeePayroll = db.EmployeePayroll.Include(e => e.OtherEarningDeductionT).ToList();
                        foreach (var Emp in OEmployeePayroll)
                        {
                            db_data = Emp.OtherEarningDeductionT.Where(r => r.Id == id).SingleOrDefault();
                            if (db_data != null)
                            {
                                EmpPayrollId = Emp.Id;
                                var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == Emp.Id).Include(e => e.SalaryT).SingleOrDefault();
                                var EmpSalT = OEmpSalT.SalaryT != null ? OEmpSalT.SalaryT.Where(e => e.PayMonth == db_data.PayMonth && e.ReleaseDate != null) : null;
                                if (EmpSalT.Count() > 0)
                                {
                                    //return Json(new Object[] { "", "", "Salary released for employee " + Emp.Employee.EmpCode + ". You can't make any changes now." }, JsonRequestBehavior.AllowGet);
                                    return Json(new { success = true, responseText = "Salary released for employee " + Emp.Employee.EmpCode + ". You can't make any changes now." }, JsonRequestBehavior.AllowGet);
                                }
                                break;
                            }
                        }


                        db_data.TDSAmount = ITP.TDSAmount;
                        db_data.SalAmount = ITP.SalAmount;

                        try
                        {
                            db.OthEarningDeductionT.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                            var OEmpSalT = db.EmployeePayroll.AsNoTracking().Where(e => e.Id == EmpPayrollId).Include(e => e.SalaryT).SingleOrDefault();
                            var EmpSalDelT = OEmpSalT.SalaryT != null ? OEmpSalT.SalaryT.Where(e => e.PayMonth == db_data.PayMonth).SingleOrDefault() : null;

                            if (EmpSalDelT != null)
                            {
                                SalaryGen.DeleteSalary(EmpSalDelT.Id, db_data.PayMonth);
                            }
                            return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception e)
                        {

                            throw e;
                        }
                    }
                    else
                    {
                        return Json(new { status = true, data = "", responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
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

        public ActionResult GridEdit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.OthEarningDeductionT.Where(e => e.Id == data).FirstOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        #region create

        public ActionResult Create(OthEarningDeductionT OEDT, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                string Emp = form["Employee-Table"] == null ? null : form["Employee-Table"];
                string SalaryHeadlist = form["SalaryHeadlist"] == "0" ? "" : form["SalaryHeadlist"];
                using (DataBaseContext db = new DataBaseContext())
                {
                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = Utility.StringIdsToListIds(Emp);
                    }
                    else
                    {
                        List<string> Msgu = new List<string>();
                        Msgu.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                    }
                    //if (SalaryHeadlist != null && SalaryHeadlist != "")
                    //{
                    //    var Val = db.SalaryHead.Find(int.Parse(SalaryHeadlist));
                    //    OEDT.SalaryHead = Val;
                    //}
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    string EmpCode = null;
                    string EmpRel = null;
                    string EmpPro = null;
                    string PayMonth = OEDT.PayMonth;

                    List<int> SalHead = null;

                    if (SalaryHeadlist != null && SalaryHeadlist != "")
                    {
                        SalHead = one_ids(SalaryHeadlist);
                    }

                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {
                            foreach (int SH in SalHead)
                            {
                                var val = db.SalaryHead.Find(SH);
                                OEDT.SalaryHead = val;
                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                           .Where(r => r.Id == i).SingleOrDefault();

                                OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == i).SingleOrDefault();

                                var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.OtherEarningDeductionT.Select(a => a.SalaryHead)).SingleOrDefault();
                                var EmpSalT = OEmpSalT.OtherEarningDeductionT != null ? OEmpSalT.OtherEarningDeductionT.Where(e => e.PayMonth == PayMonth && e.SalaryHead.Id == OEDT.SalaryHead.Id) : null;
                                if (EmpSalT != null && EmpSalT.Count() > 0)
                                {
                                    if (EmpCode == null || EmpCode == "")
                                    {
                                        EmpCode = OEmployee.EmpCode;
                                    }
                                    else
                                    {
                                        EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                                    }
                                }

                                var OEmpSalProcess = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.OtherEarningDeductionT).Include(e=>e.SalaryT).SingleOrDefault();
                                var EmpSalRelTPro = OEmpSalProcess.SalaryT != null ? OEmpSalProcess.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate == null) : null;

                                if (EmpSalRelTPro != null && EmpSalRelTPro.Count() > 0)
                                {
                                    if (EmpPro == null || EmpPro == "")
                                    {
                                        EmpPro = OEmployee.EmpCode;
                                    }
                                    else
                                    {
                                        EmpPro = EmpPro + ", " + OEmployee.EmpCode;
                                    }
                                }


                                var OEmpSalRelT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.OtherEarningDeductionT).Include(e => e.SalaryT).SingleOrDefault();
                                var EmpSalRelT = OEmpSalRelT.SalaryT != null ? OEmpSalRelT.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate != null) : null;

                                if (EmpSalRelT != null && EmpSalRelT.Count() > 0)
                                {
                                    if (EmpRel == null || EmpRel == "")
                                    {
                                        EmpRel = OEmployee.EmpCode;
                                    }
                                    else
                                    {
                                        EmpRel = EmpRel + ", " + OEmployee.EmpCode;
                                    }
                                }
                            }
                        }
                    }
                    if (EmpCode != null)
                    {
                        Msg.Add("OtherEarning already exists for employee " + EmpCode + ".");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    // return Json(new { success = true, responseText = "OtherEarning already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "", "OtherEarning already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);

                    if (EmpPro != null)
                    {               //  return Json(new { success = true, responseText = "Salary released for employee " + EmpRel + ". You can't change attendance now." }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Salary released for employee " + EmpRel + ". You can't change Earning now." }, JsonRequestBehavior.AllowGet);
                        Msg.Add("Salary Processed for employee " + EmpPro + ". You can't Enter Earning now.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (EmpRel != null)
                    {               //  return Json(new { success = true, responseText = "Salary released for employee " + EmpRel + ". You can't change attendance now." }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Salary released for employee " + EmpRel + ". You can't change Earning now." }, JsonRequestBehavior.AllowGet);
                        Msg.Add("Salary released for employee " + EmpRel + ". You can't change Earning now.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (!string.IsNullOrEmpty(Emp))
                    {

                        if (ModelState.IsValid)
                        {
                            foreach (var i in ids)
                            {
                                foreach (int SH in SalHead)
                                {
                                    var val = db.SalaryHead.Find(SH);
                                    OEDT.SalaryHead = val;
                                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                                 .Where(r => r.Id == i).SingleOrDefault();

                                    OEmployeePayroll
                                     = db.EmployeePayroll
                                   .Where(e => e.Employee.Id == i).SingleOrDefault();

                                    OEDT.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                    OthEarningDeductionT ObjOEDT = new OthEarningDeductionT();
                                    {
                                        ObjOEDT.SalaryHead = OEDT.SalaryHead;
                                        ObjOEDT.PayMonth = OEDT.PayMonth;
                                        ObjOEDT.SalAmount = OEDT.SalAmount;
                                        ObjOEDT.TDSAmount = OEDT.TDSAmount;
                                        ObjOEDT.Remark = OEDT.Remark;
                                        ObjOEDT.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct != null ? OEmployee.GeoStruct.Id : 0);
                                        ObjOEDT.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct != null ? OEmployee.FuncStruct.Id : 0);
                                        ObjOEDT.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id != null ? OEmployee.PayStruct.Id : 0);
                                        ObjOEDT.DBTrack = OEDT.DBTrack;

                                    }

                                    using (TransactionScope ts = new TransactionScope())
                                    {
                                        try
                                        {
                                            db.OthEarningDeductionT.Add(ObjOEDT);
                                            db.SaveChanges();
                                            List<OthEarningDeductionT> OFAT = new List<OthEarningDeductionT>();
                                            OFAT.Add(db.OthEarningDeductionT.Find(ObjOEDT.Id));

                                            var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                            OFAT.AddRange(aa.OtherEarningDeductionT);
                                            aa.OtherEarningDeductionT = OFAT;
                                            db.EmployeePayroll.Attach(aa);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                            ts.Complete();

                                        }
                                        catch (DataException ex)
                                        {
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
                                            return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
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
                            //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            List<string> Msgs = new List<string>();
                            Msgs.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
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
                            //return this.Json(new { msg = errorMsg });
                            List<string> MsgB = new List<string>();
                            var errorMsg = sb.ToString();
                            MsgB.Add(errorMsg);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);
                        }

                    }
                    else
                    {
                        // return Json(new Object[] { "", "", "Select Employee" }, JsonRequestBehavior.AllowGet);
                        List<string> Msgu = new List<string>();
                        Msgu.Add(" Select Employee ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);

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

        }
        #endregion

        #region Edit & EditSave
        public class EditData
        {
            public int Id { get; set; }
            public Employee Employee { get; set; }
            public string Remark { get; set; }
            public SalaryHead SalaryHead { get; set; }
            public bool Editable { get; set; }
            public double SalAmount { get; set; }
            public double TDSAmount { get; set; }
        }

        public ActionResult P2BInlineGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EditData> OtherEarningT = null;

                List<EditData> model = new List<EditData>();

                var OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                    .Include(o => o.OtherEarningDeductionT).ToList();

                var view = new EditData();

                foreach (var z in OEmployeePayroll)
                {
                    foreach (var S in z.OtherEarningDeductionT)
                    {
                        bool EditAppl = true;
                        view = new EditData()
                        {
                            Id = S.Id,
                            Employee = z.Employee,
                            SalAmount = S.SalAmount,
                            Editable = EditAppl
                        };

                        model.Add(view);
                    }
                }

                OtherEarningT = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = OtherEarningT;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.SalAmount, a.Remark, a.SalaryHead, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.SalAmount, a.Remark, a.SalaryHead, a.Editable }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = OtherEarningT;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "SalAmount" ? c.SalAmount.ToString() :
                                         gp.sidx == "Remark" ? c.Remark.ToString() :
                                         gp.sidx == "SalaryHead" ? c.SalaryHead.ToString() :
                                         gp.sidx == "Editable" ? c.Editable.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.SalAmount, a.Remark, a.SalaryHead, a.Editable }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.SalAmount, a.Remark, a.SalaryHead, a.Editable }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.SalAmount, a.Remark, a.SalaryHead, a.Editable }).ToList();
                    }
                    totalRecords = OtherEarningT.Count();
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
        public class DeserializeClass
        {
            public String Id { get; set; }
            public String SalAmount { get; set; }

        }

        public async Task<ActionResult> EditSave(SalAttendanceT S, String forwarddata, FormCollection form, string data) // Edit submit
        {
            var serialize = new JavaScriptSerializer();
            using (DataBaseContext db = new DataBaseContext())
            {
                var obj = serialize.Deserialize<List<DeserializeClass>>(forwarddata);

                if (obj.Count < 0)
                {
                    return Json(new { sucess = false, responseText = "You have to change days to update attendance." }, JsonRequestBehavior.AllowGet);
                }
                List<int> ids = obj.Select(e => int.Parse(e.Id)).ToList();

                string PayMonth = form["PayMonth"] != "" ? form["PayMonth"] : "";



                using (TransactionScope ts = new TransactionScope())
                {
                    foreach (int ca in ids)
                    {
                        try
                        {
                            OthEarningDeductionT OBJOET = db.OthEarningDeductionT.Find(ca);
                            OBJOET.SalAmount = Convert.ToInt32(obj.Where(e => e.Id == ca.ToString()).Select(e => e.SalAmount).Single());
                            db.OthEarningDeductionT.Attach(OBJOET);
                            db.Entry(OBJOET).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(OBJOET).State = System.Data.Entity.EntityState.Detached;


                        }
                        catch (DataException ex)
                        {
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
                            return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
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
                    ts.Complete();
                    return Json(new { sucess = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                }

            }
        }
        #endregion

        #region P2BGRID Details

        public ActionResult EditSaveGridData(OthEarningDeductionT oOthEarningDeductionT, int data, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.OthEarningDeductionT.Where(e => e.Id == data).FirstOrDefault();
                qurey.SalAmount = oOthEarningDeductionT.SalAmount;
                qurey.TDSAmount = oOthEarningDeductionT.TDSAmount;
                qurey.Remark = oOthEarningDeductionT.Remark;

                db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
            }
        }
        public class P2bGridOtherEarningT
        {
            public int Id { get; set; }
            public Employee Employee { get; set; }
            public string PayMonth { get; set; }
            public string Remark { get; set; }
            public double TDSAmount { get; set; }
            public SalaryHead SalaryHead { get; set; }
        }

        public class TransChildDataClass
        {
            public int Id { get; set; }
            public double SalAmount { get; set; }
            public string Remark { get; set; }
            public string Salaryhead { get; set; }
            public string PayMonth { get; set; }
            public double TDSAmount { get; set; }
        }

        public ActionResult Get_OtherEarningT(int data, string filter)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.OtherEarningDeductionT.Select(r => r.SalaryHead.Type))
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<TransChildDataClass> returndata = new List<TransChildDataClass>();
                        List<OthEarningDeductionT> LoanAdvRepaymentTData = new List<OthEarningDeductionT>();

                        if (filter != "")
                        {
                            LoanAdvRepaymentTData = db_data.OtherEarningDeductionT.Where(e => e.PayMonth == filter).OrderByDescending(e=>e.Id).ToList();
                        }
                        else
                        {
                            LoanAdvRepaymentTData = db_data.OtherEarningDeductionT.OrderByDescending(e=>e.Id).ToList();
                        }
                        foreach (var item in LoanAdvRepaymentTData)
                        {
                            if (item.SalaryHead.Type.LookupVal.ToUpper() == "EARNING")
                            {
                                returndata.Add(new TransChildDataClass
                                {
                                    Id = item.Id,
                                    SalAmount = item.SalAmount,
                                    Remark = item.Remark,
                                    Salaryhead = item.SalaryHead != null ? item.SalaryHead.Name : null,
                                    PayMonth = item.PayMonth,
                                    TDSAmount = item.TDSAmount
                                });
                            }
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

                IEnumerable<P2bGridOtherEarningT> OtherEarningT = null;
                List<P2bGridOtherEarningT> model = new List<P2bGridOtherEarningT>();
                P2bGridOtherEarningT view = null;

                var OEmployee = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).ToList();

                foreach (var z in OEmployee)
                {
                    var ObjOtherEarningdeductionT = db.EmployeePayroll.Where(e => e.Id == z.Id).Select(e => e.OtherEarningDeductionT)
                                        .SingleOrDefault();


                    //DateTime? Eff_Date = null;
                    //PayScaleAgreement PayScaleAgr = null;
                    foreach (var a in ObjOtherEarningdeductionT)
                    {
                        //Eff_Date = Convert.ToDateTime(a.ProcessMonth);
                        var aa = db.OthEarningDeductionT.Where(e => e.Id == a.Id).SingleOrDefault();
                        view = new P2bGridOtherEarningT()
                        {
                            Id = a.Id,
                            Employee = z.Employee,
                            PayMonth = a.PayMonth,
                            Remark = a.Remark,
                            TDSAmount = a.TDSAmount,
                            SalaryHead = a.SalaryHead,
                        };

                        model.Add(view);
                    }

                }

                OtherEarningT = model;

                IEnumerable<P2bGridOtherEarningT> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = OtherEarningT;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")          // a.Id,a.Employeee.EmpCode,a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                || (e.Employee.EmpCode.ToString().Contains(gp.searchString))
                                || (e.Employee.EmpName.FullNameFML.ToString().Contains(gp.searchString))
                                || (e.SalaryHead.Name.ToString().Contains(gp.searchString))
                                || (e.PayMonth.ToString().Contains(gp.searchString))
                                || (e.Remark.ToString().Contains(gp.searchString))
                                || (e.TDSAmount.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.SalaryHead.Name, a.PayMonth, a.Remark, a.TDSAmount }).ToList();
                        //jsonData = IE.Select(a => new  { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.SalaryHead.Name, a.PayMonth, a.Remark, a.TDSAmount }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = OtherEarningT;
                    Func<P2bGridOtherEarningT, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "SalaryHead" ? c.SalaryHead.Name :
                                         gp.sidx == "PayMonth" ? c.PayMonth.ToString() :
                                         gp.sidx == "Remark" ? c.Remark.ToString() :
                                         gp.sidx == "TDSAmount" ? c.TDSAmount.ToString() : ""
                                          );
                    }
                    if (gp.sord == "asc")  //Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : ""
                    {
                        IE = IE.OrderBy(orderfuc);  // a.Id,a.Employeee.EmpCode,a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, Convert.ToString(a.SalaryHead) != null ? Convert.ToString(a.SalaryHead) : "", Convert.ToString(a.PayMonth) != null ? Convert.ToString(a.PayMonth) : "", Convert.ToString(a.Remark) != null ? Convert.ToString(a.Remark) : "", Convert.ToString(a.TDSAmount) != null ? Convert.ToString(a.TDSAmount) : "" }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, Convert.ToString(a.SalaryHead) != null ? Convert.ToString(a.SalaryHead) : "", Convert.ToString(a.PayMonth) != null ? Convert.ToString(a.PayMonth) : "", Convert.ToString(a.Remark) != null ? Convert.ToString(a.Remark) : "", Convert.ToString(a.TDSAmount) != null ? Convert.ToString(a.TDSAmount) : "" }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, Convert.ToString(a.SalaryHead) != null ? Convert.ToString(a.SalaryHead) : "", Convert.ToString(a.PayMonth) != null ? Convert.ToString(a.PayMonth) : "", Convert.ToString(a.Remark) != null ? Convert.ToString(a.Remark) : "", Convert.ToString(a.TDSAmount) != null ? Convert.ToString(a.TDSAmount) : "" }).ToList();
                    }
                    totalRecords = OtherEarningT.Count();
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


        #endregion

        #region DELETE
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        //var Emp = db.EmployeePayroll.Where(e => e.Employee.Id == data).Select(e => e.SalAttendance).SingleOrDefault();
                        //SalAttendanceT SalAttendanceT = Emp.Where(e => e.LWPDays == null).SingleOrDefault();
                        OthEarningDeductionT OthEarningDeductionT = db.OthEarningDeductionT.Find(data);
                        db.Entry(OthEarningDeductionT).State = System.Data.Entity.EntityState.Deleted;
                        await db.SaveChangesAsync();
                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data removed.", JsonRequestBehavior.AllowGet });

                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
        #endregion

        public ActionResult ChkProcesscarry(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                if (month != null)
                {
                    //  var finanialyear = db.Calendar.Find(int.Parse(month));
                    bool selected = false;
                    // var query = db.AnnualSalaryR.Include(e => e.FinancialYear).ToList();
                    var query = db.OthEarningDeductionT.Where(e => e.PayMonth == month).ToList();

                    //var financialyear = query.Select(e => e.FinancialYear == finanialyear).SingleOrDefault();
                    // var financialyearR = query.Where(f => f.FinancialYear == finanialyear);

                    if (query == null || query.Count() == 0)
                    {
                        selected = true;
                    }
                    else
                    {
                        selected = true;
                    }
                    var data = new
                    {
                        status = selected,
                    };
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }

        public ActionResult AddCarryForwad(string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                DateTime CurDate = Convert.ToDateTime("01/" + month);
                string curmon = CurDate.ToString("MM/yyyy");
                DateTime prevmn = Convert.ToDateTime("01/" + month).AddDays(-1).Date;
                string prevmnon = prevmn.ToString("MM/yyyy");

                var DataChkcur = db.SalaryT.Where(e => e.PayMonth == curmon).ToList();
                if (DataChkcur.Count > 0)
                {
                    // Msg.Add(" Income tax Not available for month =" + prevmn.ToString("MM/yyyy"));
                    return Json(new Utility.JsonReturnClass { success = true, responseText = "Please delete salary for month :" + curmon + ". and Try Again.." }, JsonRequestBehavior.AllowGet);

                }
                var DataChkprv = db.SalaryT.Where(e => e.PayMonth == prevmnon).ToList();
                if (DataChkprv.Count() == 0)
                {
                    // Msg.Add(" Income tax Not available for month =" + prevmn.ToString("MM/yyyy"));
                    return Json(new Utility.JsonReturnClass { success = true, responseText = "Please First Process salary for month :" + prevmnon + ". Then Go to Next Month" }, JsonRequestBehavior.AllowGet);

                }

                var _temp_Paymonth = db.OthEarningDeductionT.Include(e => e.SalaryHead).Include(e => e.SalaryHead.Type)
                    .Where(e => e.SalaryHead.Type.LookupVal.ToString().ToUpper() == "EARNING").Select(e => e.PayMonth).Distinct().ToList();

                DateTime PrevSal = Utility._Convert_PayMonth_To_DateTime(_temp_Paymonth).OrderByDescending(e => e.Date).FirstOrDefault();
                var PrevSal_string = Utility._Convert_DateTime_To_PayMonth(new List<DateTime>() { PrevSal }).LastOrDefault();
                string Month = PrevSal.AddMonths(1).Month.ToString().Length == 1 ? "0" + PrevSal.AddMonths(1).Month.ToString() : PrevSal.AddMonths(1).Month.ToString();
                string CurMonth = Month + "/" + PrevSal.AddMonths(1).Year.ToString();
                var DataChk = db.OthEarningDeductionT.Include(e => e.SalaryHead).Include(e => e.SalaryHead.Type).Where(e => e.PayMonth == CurMonth && e.SalaryHead.Type.LookupVal.ToString().ToUpper() == "EARNING").ToList();

                if (DataChk != null && DataChk.Count > 0)
                {
                    return this.Json(new { success = true, responseText = "Data already carry forwarded for month =" + CurMonth, JsonRequestBehavior.AllowGet });
                }

               
                    var EmpList = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Include(e => e.Employee.FuncStruct)
                                    .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.PayStruct)
                                  .Include(e => e.SalaryT).Include(e => e.OtherEarningDeductionT).Include(e => e.OtherEarningDeductionT.Select(r => r.SalaryHead))
                                  .Include(e => e.OtherEarningDeductionT.Select(r => r.SalaryHead.Type)).Include(e => e.EmpSalStruct).ToList();

                    foreach (var i in EmpList)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                        //foreach (var j in i.OtherEarningDeductionT)
                        //{
                            // string PrevMonth = PrevSal;
                            //  PrevDate = Convert.ToDateTime("01/" + PrevMonth);
                            var PrevOthEarnDed = i.OtherEarningDeductionT.Where(e => e.PayMonth == PrevSal_string && e.SalaryHead.Type.LookupVal.ToString().ToUpper() == "EARNING").ToList();
                            //Month = PrevDate.AddMonths(1).Month.ToString().Length == 1 ? "0" + PrevDate.AddMonths(1).Month.ToString() : PrevDate.AddMonths(1).Month.ToString();
                            //CurMonth = Month + "/" + PrevDate.AddMonths(1).Year.ToString();

                            if (i.Employee.ServiceBookDates.ServiceLastDate == null || i.Employee.ServiceBookDates.ServiceLastDate != null && Convert.ToDateTime("01/" + i.Employee.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + CurMonth))
                            {
                                EmpSalStruct SalStruct = i.EmpSalStruct.Where(e => e.EffectiveDate.Value.ToString("MM/yyyy") == CurMonth).OrderByDescending(e => e.EffectiveDate).FirstOrDefault();
                                DateTime ToDate = Convert.ToDateTime(DateTime.DaysInMonth(SalStruct.EffectiveDate.Value.Year, SalStruct.EffectiveDate.Value.Month) + "/" + CurMonth);
                                if (SalStruct != null)
                                {
                                    foreach (var F in PrevOthEarnDed)
                                    {
                                        DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        OthEarningDeductionT OthEarnDedT = new OthEarningDeductionT()
                                        {
                                            DBTrack = DBTrack,
                                            FuncStruct = i.Employee.FuncStruct,
                                            GeoStruct = i.Employee.GeoStruct,
                                            PayMonth = CurMonth,
                                            Remark = F.Remark,
                                            PayStruct = i.Employee.PayStruct,
                                            SalAmount = F.SalAmount,
                                            TDSAmount = F.TDSAmount,
                                            SalaryHead = F.SalaryHead
                                        };
                                        db.OthEarningDeductionT.Add(OthEarnDedT);
                                        db.SaveChanges();

                                        List<OthEarningDeductionT> OFAT = new List<OthEarningDeductionT>();
                                        OFAT.Add(db.OthEarningDeductionT.Find(OthEarnDedT.Id));

                                        var aa = db.EmployeePayroll.Find(i.Id);
                                        OFAT.AddRange(aa.OtherEarningDeductionT);
                                        aa.OtherEarningDeductionT = OFAT;
                                        //OEmployeePayroll.DBTrack = dbt;

                                        db.EmployeePayroll.Attach(aa);
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                            }
                        //    break;
                        //}
                        ts.Complete();
                    }
                   
                }

                return this.Json(new { success = true, responseText = "Data carry forwarded for month =" + CurMonth, JsonRequestBehavior.AllowGet });
            }

        }

    }

}