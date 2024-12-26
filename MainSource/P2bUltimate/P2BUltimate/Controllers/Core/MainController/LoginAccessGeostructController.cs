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
using System.Web.Script.Serialization;

namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class LoginAccessGeostructController : Controller
    {
        //
        // GET: /PerkTransT/
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/LoginAccessGeostruct/Index.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_PerkTransGridPartial.cshtml");
        }

        public ActionResult PopulateDropDownPackageList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var qurey = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "PERK").ToList();

                var query = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "601").FirstOrDefault().LookupValues;
                var selected = (Object)null;
                //if (data2 != "" && data != "0" && data2 != "0")
                //{
                //    selected = Convert.ToInt32(data2);
                //}

                SelectList s = new SelectList(query, "Id", "LookupVal", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }


        public class A
        {
            public List<FuncStruct> Fun { get; set; }
            public List<PayStruct> Pay { get; set; }
        }

        public ActionResult PopulateDropDownLoginAccessTypeList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Employee.Include(e => e.FuncStruct).Include(e => e.PayStruct).FirstOrDefault();
                var droplist = db.Login.Include(e => e.LoginAccessFuncstruct).
                                        Include(e => e.LoginAccessFuncstruct)
                                        .Where(e => e.Id == qurey.Login_Id).
                                        ToList();
                List<A> LoginAcc = new List<A>();

                foreach (var item in droplist)
                {

                    LoginAcc.Add(new A
                    {
                        Fun = item.LoginAccessFuncstruct.Select(e => e.FuncStruct).ToList(),
                        Pay = item.LoginAccessPaystruct.Select(s => s.PayStruct).ToList()
                    });

                }

                //var droplist1 = db.LoginAccessPaystruct.Where(e => e.PayStruct_Id == qurey.PayStruct_Id).FirstOrDefault();
                //var selected = (Object)null;
                //if (data2 != "" && data != "0" && data2 != "0")
                //{
                //    selected = Convert.ToInt32(data2);
                //}

                SelectList ss = new SelectList(LoginAcc);
                return Json(ss, JsonRequestBehavior.AllowGet);
            }
        }

        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            // public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }


        public ActionResult ValidateForm(FormCollection form)
        {
            string Packagelist = form["Packagelist"] == "0" ? "" : form["Packagelist"];
            int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
            List<string> Msg = new List<string>();


            if (Packagelist == null)
            {
                Msg.Add("Please select Package");
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
        public ActionResult Create(LoginAccessGeostruct LoginAccess, LoginAccessPaystruct LoginPay, LoginAccessFuncstruct LoginFunc, FormCollection form)
        {
            List<string> Msg = new List<string>();
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                string Packagelist = form["Packagelist"] == "0" ? "" : form["Packagelist"];
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];


                if (Emp == null && Emp == "")
                {
                    Msg.Add("Please Select Employee");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                var geo_id = form["geo_id"] == "0" ? "" : form["geo_id"];
                var pay_id = form["pay_id"] == "0" ? "" : form["pay_id"];
                var fun_id = form["fun_id"] == "0" ? "" : form["fun_id"];

                if ((geo_id == "" || geo_id == null))
                {
                    Msg.Add("Please Select GeoStruct , Paystruct OR Funcstruct Filter");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }

                
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);
                var GeoList = deserialize.GeoStruct != null ? deserialize.GeoStruct : "";

                var SerializePay = new JavaScriptSerializer();
                var Paydeserialize = SerializePay.Deserialize<Utility.GridParaStructIdClass>(pay_id);
                var PayList = Paydeserialize.PayStruct != null ? Paydeserialize.PayStruct : "";

                var SerializeFunc = new JavaScriptSerializer();
                var Funcdeserialize = SerializeFunc.Deserialize<Utility.GridParaStructIdClass>(fun_id);
                var FuncList = Funcdeserialize.FunStruct != null ? Funcdeserialize.FunStruct : "";


                if (Packagelist != null && Packagelist != "")
                {

                    var val = db.LookupValue.Find(int.Parse(Packagelist));
                    if (GeoList != "" && GeoList != null)
                    {
                        LoginAccess.Package = val;
                    }
                    if (FuncList != "" && FuncList != null)
                    {
                        LoginFunc.Package = val;
                    }
                    if (PayList != "" && PayList != null)
                    {
                        LoginPay.Package = val;
                    }
                }
                else
                {
                    Msg.Add("Please select Package");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }


                Login DBloginIds = null;
                //Employee OEmployee = null;

                var UserSelectedEmpIds = Utility.StringIdsToListIds(Emp);
                var OEmployee = new List<Employee>();
                foreach (var itemEmpId in UserSelectedEmpIds)
                {
                    var OOEmployee = db.Employee.Include(e => e.Login).Where(r => r.Id == itemEmpId).FirstOrDefault();

                    DBloginIds = db.Login.Include(e => e.LoginAccessGeostruct).Include(e => e.LoginAccessPaystruct).Include(e => e.LoginAccessFuncstruct)
                                       .Where(e => e.Id == OOEmployee.Login.Id).FirstOrDefault();

                    if (GeoList != "" && GeoList != null)
                    {
                        var Geoidss = Utility.StringIdsToListIds(GeoList);
                        foreach (var itemid in Geoidss)
                        {

                            var DBLoginAccessGeoIds = DBloginIds.LoginAccessGeostruct.Where(e => e.GeoStruct_Id == itemid
                                              && e.Package.LookupVal.ToUpper() == LoginAccess.Package.LookupVal.ToUpper()).FirstOrDefault();

                            if (DBLoginAccessGeoIds != null)
                            {
                                Msg.Add("Selected Record Already Exists...");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                break;
                            }

                        }
                    }

                    if (PayList != "" && PayList != null)
                    {
                        var Payidss = Utility.StringIdsToListIds(PayList);
                        foreach (var itemId in Payidss)
                        {

                            var DBLoginAccessPayIds = DBloginIds.LoginAccessPaystruct.Where(e => e.PayStruct_Id == itemId
                                              && e.Package.LookupVal.ToUpper() == LoginAccess.Package.LookupVal.ToUpper()).FirstOrDefault();

                            if (DBLoginAccessPayIds != null)
                            {
                                Msg.Add("Selected Record Already Exists...");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                break;
                            }

                        }
                    }


                    if (FuncList != "" && FuncList != null)
                    {
                        var Funcidss = Utility.StringIdsToListIds(FuncList);
                        foreach (var itemidd in Funcidss)
                        {

                            var DBLoginAccessFuncIds = DBloginIds.LoginAccessFuncstruct != null ? DBloginIds.LoginAccessFuncstruct.Where(e => e.FuncStruct_Id == itemidd
                                              && e.Package.LookupVal.ToUpper() == LoginAccess.Package.LookupVal.ToUpper()).FirstOrDefault() : null;

                            if (DBLoginAccessFuncIds != null)
                            {
                                Msg.Add("Selected Record Already Exists...");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                break;
                            }

                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                      new System.TimeSpan(0, 30, 0)))
                        {
                            LoginAccess.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<LoginAccessGeostruct> OFAT = new List<LoginAccessGeostruct>();

                            LoginPay.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<LoginAccessPaystruct> OPAY = new List<LoginAccessPaystruct>();

                            LoginFunc.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<LoginAccessFuncstruct> OFUNC = new List<LoginAccessFuncstruct>();
                            try
                            {
                                #region Geostruct
                                if (GeoList != null && GeoList != "")
                                {
                                    List<GeoStruct> GeoStructList = new List<GeoStruct>();
                                    var ids = Utility.StringIdsToListIds(GeoList);

                                    foreach (var ca in ids)
                                    {

                                        LoginAccessGeostruct LoginAccessed = new LoginAccessGeostruct()
                                        {
                                            Package = LoginAccess.Package,
                                            GeoStruct = db.GeoStruct.Find(ca),
                                            DBTrack = LoginAccess.DBTrack
                                        };

                                        db.LoginAccessGeostruct.Add(LoginAccessed);
                                        db.SaveChanges();

                                        OFAT.Add(db.LoginAccessGeostruct.Find(LoginAccessed.Id));
                                    }
                                }
                                else
                                {
                                    LoginAccess.GeoStruct = null;
                                }


                                #endregion Geostruct

                                // FOR PAYSTRUCT
                                #region PAYSTRUCT
                                if (PayList != null && PayList != "")
                                {
                                    List<PayStruct> PayStructList = new List<PayStruct>();
                                    var ids = Utility.StringIdsToListIds(PayList);

                                    foreach (var ca in ids)
                                    {

                                        LoginAccessPaystruct LoginAccessedPAY = new LoginAccessPaystruct()
                                        {
                                            Package = LoginPay.Package,
                                            PayStruct = db.PayStruct.Find(ca),
                                            DBTrack = LoginPay.DBTrack
                                        };

                                        db.LoginAccessPaystruct.Add(LoginAccessedPAY);
                                        db.SaveChanges();

                                        OPAY.Add(db.LoginAccessPaystruct.Find(LoginAccessedPAY.Id));

                                    }
                                }
                                else
                                {
                                    LoginPay.PayStruct = null;
                                }


                                #endregion PAYSTRUCT

                                // FOR FUNCSTRUCT
                                #region FUNCSTRUCT
                                if (FuncList != null && FuncList != "")
                                {
                                    List<FuncStruct> FuncStructList = new List<FuncStruct>();
                                    var ids = Utility.StringIdsToListIds(FuncList);

                                    foreach (var ca in ids)
                                    {

                                        LoginAccessFuncstruct LoginAccessedFUNC = new LoginAccessFuncstruct()
                                        {
                                            Package = LoginFunc.Package,
                                            FuncStruct = db.FuncStruct.Find(ca),
                                            DBTrack = LoginFunc.DBTrack
                                        };

                                        db.LoginAccessFuncstruct.Add(LoginAccessedFUNC);
                                        db.SaveChanges();

                                        OFUNC.Add(db.LoginAccessFuncstruct.Find(LoginAccessedFUNC.Id));
                                    }
                                }
                                else
                                {
                                    LoginFunc.FuncStruct = null;
                                }

                                if (DBloginIds != null)
                                { 
                                    Login OLogin = db.Login.Where(e => e.Id == DBloginIds.Id).FirstOrDefault();
                                    if (OLogin != null)
                                    {
                                        //if (OLogin.LoginAccessPaystruct != null)
                                        //{
                                        //    OPAY.AddRange(OLogin.LoginAccessPaystruct);
                                        //    OLogin.LoginAccessPaystruct = OPAY;
                                        //}

                                        //if (OLogin.LoginAccessGeostruct != null)
                                        //{
                                        //    OFAT.AddRange(OLogin.LoginAccessGeostruct);
                                        //    OLogin.LoginAccessGeostruct = OFAT;
                                        //}

                                        //if (OLogin.LoginAccessFuncstruct != null)
                                        //{
                                        //    OFUNC.AddRange(OLogin.LoginAccessFuncstruct);
                                        //    OLogin.LoginAccessFuncstruct = OFUNC;
                                        //}


                                        if (OPAY != null)
                                        {
                                            OPAY.AddRange(OLogin.LoginAccessPaystruct);
                                            OLogin.LoginAccessPaystruct = OPAY;
                                        }

                                        if (OFAT != null)
                                        {
                                            OFAT.AddRange(OLogin.LoginAccessGeostruct);
                                            OLogin.LoginAccessGeostruct = OFAT;
                                        }

                                        if (OFUNC != null)
                                        {
                                            OFUNC.AddRange(OLogin.LoginAccessFuncstruct);
                                            OLogin.LoginAccessFuncstruct = OFUNC;
                                        }
                                        db.Login.Attach(OLogin);
                                        db.Entry(OLogin).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(OLogin).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //ts.Complete();

                                }
                                #endregion FUNCSTRUCT

                                ts.Complete();
                                
                                
                                
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
            }
            Msgs.Add("Data Saved successfully");
            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
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

                                var SalT = db.EmployeePayroll.Where(e => e.Id == a.Id).Select(e => e.SalaryT.Where(r => r.PayMonth == ypay.PayMonth && r.ReleaseDate != null).FirstOrDefault()).SingleOrDefault();
                                if (SalT != null)
                                {
                                    return Json(new { status = false, data = db_data, responseText = "Salary is released for this month. You can't change amount now..!" }, JsonRequestBehavior.AllowGet);
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


        public ActionResult Emp_GridOne(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                        .AsNoTracking().ToList();
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
                                //JoiningDate = item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy") : null,
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
        } // For GeoStruct

        public ActionResult Emp_GridTwo(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                        .AsNoTracking().ToList();
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
                                //JoiningDate = item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy") : null,
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
        } // For PayStruct

        public ActionResult Emp_GridThree(ParamModel param, string y)  // For FuncStruct
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                        .AsNoTracking().ToList();
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
                                // JoiningDate = item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy") : null,
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


        public class LoginAccessGeoChildDataClass //Geochildgrid
        {
            public int Id { get; set; }
            public string PackageCode { get; set; }
            public string Geostrut { get; set; }

        }

        public class LoginAccessPayChildDataClass //Paychildgrid
        {

            public string PackageCode { get; set; }
            public string Paystruct { get; set; }
            //public int PayAccessId { get; set; }
            public int Id { get; set; }

        }

        public class LoginAccessFuncChildDataClass //Funcchildgrid
        {
            public string PackageCode { get; set; }
            public string Funcstruct { get; set; }
            //public int FuncAccessId { get; set; }
            public int Id { get; set; }

        }


        //public ActionResult Get_LoginAcess(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        #region try
        //        try
        //        {
        //            var db_data = db.Employee.Include(e => e.Login.LoginAccessGeostruct)
        //                                     .Include(e => e.Login.LoginAccessPaystruct)
        //                                     .Include(e => e.Login.LoginAccessFuncstruct).Where(e => e.Id == data).AsEnumerable()
        //                                        .Select(t => new Login
        //                                        {
        //                                            LoginAccessGeostruct = t.Login.LoginAccessGeostruct,
        //                                            LoginAccessPaystruct = t.Login.LoginAccessPaystruct,
        //                                            LoginAccessFuncstruct = t.Login.LoginAccessFuncstruct

        //                                        }).ToList();
        //            //.Select(t => t.Login.LoginAccessGeostruct).AsNoTracking().FirstOrDefault();

        //            //var db_data = db.Employee.Where(e => e.Id == data).Select(t => t.Login.LoginAccessGeostruct).AsNoTracking().FirstOrDefault();

        //            if (db_data != null)
        //            {
        //                List<LoginAccessChildDataClass> returndata = new List<LoginAccessChildDataClass>();

        //                LoginAccessGeostruct LoginAccessIds = null ;
        //                LoginAccessPaystruct LoginAccessPayIds = null;
        //                LoginAccessFuncstruct LoginAccessFuncIds = null;
        //                foreach (var item in db_data)
        //                {
        //                    #region GeoStruct
        //                    foreach (var item1 in item.LoginAccessGeostruct)
        //                    {
        //                         LoginAccessIds = db.LoginAccessGeostruct.Include(e => e.GeoStruct)
        //                                                                                    .Include(e => e.GeoStruct.Location)
        //                                                                                    .Include(e => e.GeoStruct.Location.LocationObj)
        //                                                                                    .Include(e => e.Package)
        //                                                                                    .Where(e => e.Id == item1.Id).AsNoTracking().FirstOrDefault();
        //                         //if (LoginAccessIds != null)
        //                         //{
        //                         //    returndata.Add(new LoginAccessChildDataClass
        //                         //    {
        //                         //        Id = LoginAccessIds.Id,
        //                         //        PackageCode = LoginAccessIds.Package.LookupVal != null ? LoginAccessIds.Package.LookupVal : null,
        //                         //        Geostrut = LoginAccessIds.GeoStruct != null && LoginAccessIds.GeoStruct.Location != null && LoginAccessIds.GeoStruct.Location != null && LoginAccessIds.GeoStruct.Location.LocationObj != null ? LoginAccessIds.GeoStruct.Location.LocationObj.LocCode.ToString() + " " + LoginAccessIds.GeoStruct.Location.LocationObj.LocDesc.ToString() : ""
        //                         //    });

        //                         //}
        //                    }


        //                    #endregion GeoStruct


        //                     #region PayStruct
        //                    foreach (var item1 in item.LoginAccessPaystruct)
        //                    {
        //                          LoginAccessPayIds = db.LoginAccessPaystruct.Include(e => e.PayStruct)
        //                                                                .Include(e => e.PayStruct.Grade)
        //                                                                .Include(e => e.PayStruct.JobStatus)
        //                                                                .Include(e => e.Package)
        //                                                                .Where(e => e.Id == item1.Id).AsNoTracking().FirstOrDefault();
        //                        // if (LoginAccessPayIds != null)
        //                        //{
        //                        //    returndata.Add(new LoginAccessChildDataClass
        //                        //    {
        //                        //        PayAccessId = LoginAccessPayIds.Id,
        //                        //        Paystruct = LoginAccessPayIds.PayStruct != null && LoginAccessPayIds.PayStruct.Grade != null && LoginAccessPayIds.PayStruct.JobStatus != null ? LoginAccessPayIds.PayStruct.Grade.Name.ToString() + " " + LoginAccessPayIds.PayStruct.JobStatus.FullDetails.ToString() : ""
        //                        //    });

        //                        //}
        //                    }
        //                     #endregion PayStruct


        //                     #region FuncStruct
        //                    foreach (var item1 in item.LoginAccessFuncstruct)
        //                    {
        //                      LoginAccessFuncIds = db.LoginAccessFuncstruct.Include(e => e.FuncStruct)
        //                                                                .Include(e => e.FuncStruct.Job)
        //                                                                .Include(e => e.Package)
        //                                                                .Where(e => e.Id == item1.Id).AsNoTracking().FirstOrDefault();

        //                        //if (LoginAccessFuncIds != null)
        //                        //{
        //                        //    returndata.Add(new LoginAccessChildDataClass
        //                        //    {
        //                        //        FuncAccessId = LoginAccessFuncIds.Id,
        //                        //        Funcstruct = LoginAccessFuncIds.FuncStruct != null && LoginAccessFuncIds.FuncStruct.Job != null ? LoginAccessFuncIds.FuncStruct.Job.FullDetails.ToString() : ""
        //                        //    });

        //                        //}
        //                    }
        //                    if (LoginAccessIds != null || LoginAccessPayIds != null || LoginAccessFuncIds != null)
        //                    {
        //                        returndata.Add(new LoginAccessChildDataClass
        //                        {
        //                            Id = LoginAccessIds.Id,
        //                            PackageCode = LoginAccessIds.Package.LookupVal != null ? LoginAccessIds.Package.LookupVal : null,
        //                            Geostrut = LoginAccessIds.GeoStruct != null && LoginAccessIds.GeoStruct.Location != null && LoginAccessIds.GeoStruct.Location != null && LoginAccessIds.GeoStruct.Location.LocationObj != null ? LoginAccessIds.GeoStruct.Location.LocationObj.LocCode.ToString() + " " + LoginAccessIds.GeoStruct.Location.LocationObj.LocDesc.ToString() : "",
        //                            PayAccessId = LoginAccessPayIds.Id,
        //                            Paystruct = LoginAccessPayIds.PayStruct != null && LoginAccessPayIds.PayStruct.Grade != null && LoginAccessPayIds.PayStruct.JobStatus != null ? LoginAccessPayIds.PayStruct.Grade.Name.ToString() + " " + LoginAccessPayIds.PayStruct.JobStatus.FullDetails.ToString() : "",
        //                            FuncAccessId = LoginAccessFuncIds.Id,
        //                            Funcstruct = LoginAccessFuncIds.FuncStruct != null && LoginAccessFuncIds.FuncStruct.Job != null ? LoginAccessFuncIds.FuncStruct.Job.FullDetails.ToString() : ""
        //                        });

        //                    }

        //                     #endregion FuncStruct

        //                }
        //                return Json(returndata, JsonRequestBehavior.AllowGet);

        //            }
        //            else
        //            {
        //                return null;
        //            }

        //        }
        //        #endregion try

        //        catch (Exception e)
        //        {
        //            throw e;
        //        }
        //    }
        //}

        public ActionResult Get_LoginAcessGeo(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                #region try
                try
                {
                    var db_data = db.Employee.Where(e => e.Id == data).Select(t => t.Login.LoginAccessGeostruct).AsNoTracking().FirstOrDefault();

                    if (db_data != null)
                    {
                        List<LoginAccessGeoChildDataClass> returndata = new List<LoginAccessGeoChildDataClass>();

                        foreach (var item in db_data)
                        {


                            var LoginAccessIds = db.LoginAccessGeostruct.Include(e => e.GeoStruct)
                                                                                        .Include(e => e.GeoStruct.Location)
                                                                                        .Include(e => e.GeoStruct.Location.LocationObj)
                                                                                        .Include(e => e.Package)
                                                                                        .Where(e => e.Id == item.Id).AsNoTracking().FirstOrDefault();
                            if (LoginAccessIds != null)
                            {
                                returndata.Add(new LoginAccessGeoChildDataClass
                                {
                                    Id = LoginAccessIds.Id,
                                    PackageCode = LoginAccessIds.Package.LookupVal != null ? LoginAccessIds.Package.LookupVal : null,
                                    Geostrut = LoginAccessIds.GeoStruct != null && LoginAccessIds.GeoStruct.Location != null && LoginAccessIds.GeoStruct.Location != null && LoginAccessIds.GeoStruct.Location.LocationObj != null ? LoginAccessIds.GeoStruct.Location.LocationObj.LocCode.ToString() + " " + LoginAccessIds.GeoStruct.Location.LocationObj.LocDesc.ToString() : ""
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
                #endregion try

                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public ActionResult Get_LoginAcessPay(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                #region try
                try
                {
                    var db_data = db.Employee.Where(e => e.Id == data).Select(t => t.Login.LoginAccessPaystruct).AsNoTracking().FirstOrDefault();

                    if (db_data != null)
                    {
                        List<LoginAccessPayChildDataClass> returndata = new List<LoginAccessPayChildDataClass>();

                        foreach (var item in db_data)
                        {

                            var LoginAccessPayIds = db.LoginAccessPaystruct.Include(e => e.PayStruct)
                                                                   .Include(e => e.PayStruct.Grade)
                                                                   .Include(e => e.PayStruct.JobStatus)
                                                                   .Include(e => e.Package)
                                                                   .Where(e => e.Id == item.Id).AsNoTracking().FirstOrDefault();
                            if (LoginAccessPayIds != null)
                            {
                                returndata.Add(new LoginAccessPayChildDataClass
                                {
                                    Id = LoginAccessPayIds.Id,
                                    PackageCode = LoginAccessPayIds.Package.LookupVal != null ? LoginAccessPayIds.Package.LookupVal : null,
                                    Paystruct = LoginAccessPayIds.PayStruct != null && LoginAccessPayIds.PayStruct.Grade != null && LoginAccessPayIds.PayStruct.JobStatus != null ? LoginAccessPayIds.PayStruct.Grade.Name.ToString() + " " + LoginAccessPayIds.PayStruct.JobStatus.FullDetails.ToString() : ""
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
                #endregion try

                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public ActionResult Get_LoginAcessFunc(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                #region try
                try
                {
                    var db_data = db.Employee.Where(e => e.Id == data).Select(t => t.Login.LoginAccessFuncstruct).AsNoTracking().FirstOrDefault();

                    if (db_data != null)
                    {
                        List<LoginAccessFuncChildDataClass> returndata = new List<LoginAccessFuncChildDataClass>();

                        foreach (var item in db_data)
                        {
                            var LoginAccessFuncIds = db.LoginAccessFuncstruct.Include(e => e.FuncStruct)
                                                                       .Include(e => e.FuncStruct.Job)
                                                                       .Include(e => e.Package)
                                                                       .Where(e => e.Id == item.Id).AsNoTracking().FirstOrDefault();

                            if (LoginAccessFuncIds != null)
                            {
                                returndata.Add(new LoginAccessFuncChildDataClass
                                {
                                    Id = LoginAccessFuncIds.Id,
                                    PackageCode = LoginAccessFuncIds.Package.LookupVal != null ? LoginAccessFuncIds.Package.LookupVal : null,
                                    Funcstruct = LoginAccessFuncIds.FuncStruct != null && LoginAccessFuncIds.FuncStruct.Job != null ? LoginAccessFuncIds.FuncStruct.Job.FullDetails.ToString() : ""
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
                #endregion try

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

                int empidintrnal = Convert.ToInt32(ids[0]);
                int empidMain = Convert.ToInt32(ids[1]);

                var EmpId = db.Employee.Include(e => e.Login.LoginAccessFuncstruct)
                                       .Include(e => e.Login.LoginAccessPaystruct)
                                       .Include(e => e.Login.LoginAccessGeostruct)
                                       .Where(e => e.Id == empidMain).ToList();

                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {

                    try
                    {
                        if (EmpId != null)
                        {
                            string PackageCode = "";
                            LoginAccessGeostruct OLoginAccessGeostruct = db.LoginAccessGeostruct.Find(empidintrnal);
                            LoginAccessFuncstruct OLoginAccessFuncstruct = db.LoginAccessFuncstruct.Find(empidintrnal);
                            LoginAccessPaystruct OLoginAccessPaystruct = db.LoginAccessPaystruct.Find(empidintrnal);
                            if (OLoginAccessGeostruct != null)
                            {
                                PackageCode = db.LookupValue.Where(e => e.Id == OLoginAccessGeostruct.Package_Id).FirstOrDefault().LookupVal.ToString().ToUpper();
                            }
                            else if (OLoginAccessFuncstruct != null)
                            {
                                PackageCode = db.LookupValue.Where(e => e.Id == OLoginAccessFuncstruct.Package_Id).FirstOrDefault().LookupVal.ToString().ToUpper();
                            }
                            else
                            {
                                PackageCode = db.LookupValue.Where(e => e.Id == OLoginAccessPaystruct.Package_Id).FirstOrDefault().LookupVal.ToString().ToUpper();
                            }
                            //string PackageCode = db.LookupValue.Where(e => e.Id == OLoginAccessGeostruct.Package_Id).FirstOrDefault().LookupVal.ToString().ToUpper();
                            if (PackageCode != "")
                            {
                                List<LoginAccessGeostruct> LoginAccessGeostructList = EmpId.SelectMany(t => t.Login.LoginAccessGeostruct.Where(y => y.Package.LookupVal.ToUpper() == PackageCode).ToList()).ToList();
                                db.LoginAccessGeostruct.RemoveRange(LoginAccessGeostructList);
                                List<LoginAccessFuncstruct> LoginAccessFuncstructList = EmpId.SelectMany(t => t.Login.LoginAccessFuncstruct.Where(y => y.Package.LookupVal.ToUpper() == PackageCode).ToList()).ToList();
                                db.LoginAccessFuncstruct.RemoveRange(LoginAccessFuncstructList);
                                List<LoginAccessPaystruct> LoginAccessPaystructList = EmpId.SelectMany(t => t.Login.LoginAccessPaystruct.Where(y => y.Package.LookupVal.ToUpper() == PackageCode).ToList()).ToList();
                                db.LoginAccessPaystruct.RemoveRange(LoginAccessPaystructList);

                                await db.SaveChangesAsync();
                            }


                        }
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


        //[HttpPost]
        //public async Task<ActionResult> DeleteOne(string dataOne)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var ids = Utility.StringIdsToListIds(dataOne);
        //        List<string> Msg = new List<string>();

        //        int empidintrnal = Convert.ToInt32(ids[0]);
        //        int empidMain = Convert.ToInt32(ids[1]);

        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {

        //            try
        //            {
        //                LoginAccessPaystruct OLoginAccessPaystruct = db.LoginAccessPaystruct.Find(empidintrnal);
        //                db.Entry(OLoginAccessPaystruct).State = System.Data.Entity.EntityState.Deleted;
        //                await db.SaveChangesAsync();
        //                ts.Complete();
        //                return this.Json(new { status = true, responseText = "Data removed.", JsonRequestBehavior.AllowGet });

        //            }
        //            catch (RetryLimitExceededException /* dex */)
        //            {
        //                //Log the error (uncomment dex variable name and add a line here to write a log.)
        //                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
        //                //return RedirectToAction("Delete");
        //                return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
        //            }
        //            catch (Exception ex)
        //            {
        //                LogFile Logfile = new LogFile();
        //                ErrorLog Err = new ErrorLog()
        //                {
        //                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                    ExceptionMessage = ex.Message,
        //                    ExceptionStackTrace = ex.StackTrace,
        //                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                    LogTime = DateTime.Now
        //                };
        //                Logfile.CreateLogFile(Err);
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //    }
        //}



    }
}