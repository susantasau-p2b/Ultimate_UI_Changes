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
using P2B.PFTRUST;
using CMS_SPS;

namespace P2BUltimate.Controllers.PFTrust
{
    public class LoanAdvRequestPFTController : Controller
    {
        // GET: EmployeePF
        public ActionResult Index()
        {
            return View("~/Views/PFTrust/LoanAdvRequestPFT/index.cshtml");
        }
        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/PFTrust/_LoanAdvRequestPFTGridPartial.cshtml");
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

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj)
                       
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                        .ToList();
                   
                    IEnumerable<EmployeePayroll> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                       
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.Employee.EmpCode.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.Employee.EmpName.FullNameFML.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                 
                                  ).ToList();
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
        public ActionResult GridEditSave(LoanAdvRequestPFT LoanAdvReq, FormCollection form, string data)
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                var LoanAdvReqData = db.LoanAdvRequestPFT.Include(e => e.LoanAdvRepaymentTPFT).Where(e => e.Id == Id).SingleOrDefault();
                int NewInsCount = Convert.ToInt16(form["TotalInstallment"]);
                LoanAdvReqData.IsActive = Convert.ToBoolean(form["IsActive"]);
                LoanAdvReqData.LoanAccNo = LoanAdvReq.LoanAccNo;
                string CloserDate = form["CloserDate"] == "0" ? "" : form["CloserDate"];
                string LoanAccBranch = form["LoanAccBranch"] == "0" ? "" : form["LoanAccBranch"];
                if (LoanAccBranch != "0")
                {
                    var id = Convert.ToInt32(LoanAccBranch);
                    var Branchdata = db.Location.Where(e => e.Id == id).SingleOrDefault();
                    LoanAdvReqData.LoanAccBranch = Branchdata;
                }
                if (CloserDate != null && CloserDate != "")
                {

                    LoanAdvReqData.CloserDate = Convert.ToDateTime(CloserDate);
                }
                else
                {
                    LoanAdvReqData.CloserDate = null;
                }
                if (LoanAdvReqData.IsActive == false && CloserDate == "")
                {
                    Msgs.Add("Please enter closer date");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msgs }, JsonRequestBehavior.AllowGet);
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

                        if (LoanAdvReqData.CloserDate != null)
                        {
                            IEnumerable<LoanAdvRepaymentTPFT> LoanAdvRepaymentTDet = LoanAdvReqData.LoanAdvRepaymentTPFT.Where(e => e.InstallementDate > LoanAdvReqData.CloserDate).ToList();
                            db.LoanAdvRepaymentTPFT.RemoveRange(LoanAdvRepaymentTDet);
                            
                            db.LoanAdvRequestPFT.Attach(LoanAdvReqData);
                            db.Entry(LoanAdvReqData).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(LoanAdvReqData).State = System.Data.Entity.EntityState.Detached;
                        }
                        else
                        {
                            IEnumerable<LoanAdvRepaymentTPFT> LoanAdvRepaymentTDetails = LoanAdvReqData.LoanAdvRepaymentTPFT.ToList();
                           
                            if (LoanAdvRepaymentTDetails.Count() > 0)
                            {

                                double LoanPaid = LoanAdvRepaymentTDetails.LastOrDefault().InstallmentPaid;
                                DateTime? LastInstallmentDt = LoanAdvRepaymentTDetails.LastOrDefault().InstallementDate;
                                double TotalLoanBalance = LoanAdvRepaymentTDetails.LastOrDefault().TotalLoanBalance;

                                List<LoanAdvRepaymentTPFT> LoanAdvRepaymentTDet = new List<LoanAdvRepaymentTPFT>();

                                int NewCount = NewInsCount - LoanAdvReqData.TotalInstall;
                                if (NewCount > 0)
                                {

                                    if (LoanAdvReqData.EnforcementDate != null && NewCount != 0)
                                    {
                                        for (int i = 1; i <= NewCount; i++)
                                        {
                                            string Month = LastInstallmentDt.Value.AddMonths(i).Month.ToString().Length == 1 ? "0" + LastInstallmentDt.Value.AddMonths(i).Month.ToString() : LastInstallmentDt.Value.AddMonths(i).Month.ToString();
                                            //string PayMonth = Month + "/" + LoanAdvReqData.EnforcementDate.Value.AddMonths(i).Year;
                                            string PayMonth = Month + "/" + LastInstallmentDt.Value.AddMonths(i).Year;
                                            LoanAdvRepaymentTPFT LoanAdvRepayT = new LoanAdvRepaymentTPFT()
                                            {
                                                InstallementDate = LastInstallmentDt.Value.AddMonths(i),
                                                InstallmentAmount = LoanAdvReqData.MonthlyInstallmentAmount,
                                                InstallmentCount = LoanAdvReqData.TotalInstall + i,
                                                InstallmentPaid = LoanPaid,
                                                PayMonth = PayMonth,
                                                RepaymentDate = null,
                                                TotalLoanBalance = TotalLoanBalance,
                                                TotalLoanPaid = 0,
                                                DBTrack = LoanAdvReqData.DBTrack
                                            };
                                            LoanAdvRepaymentTDet.Add(LoanAdvRepayT);
                                        }
                                    }

                                    LoanAdvReqData.TotalInstall = LoanAdvReqData.TotalInstall + NewCount;
                                }
                                else
                                {
                                    NewCount = LoanAdvReqData.TotalInstall - NewInsCount;
                                    if (LoanAdvReqData.EnforcementDate != null && NewCount != 0)
                                    {
                                        var RepayDel = LoanAdvReqData.LoanAdvRepaymentTPFT.OrderByDescending(e => e.Id).Take(NewCount).ToList();
                                        db.LoanAdvRepaymentTPFT.RemoveRange(RepayDel);
                                        LoanAdvReqData.TotalInstall = NewInsCount;
                                    }


                                }

                                db.LoanAdvRequestPFT.Attach(LoanAdvReqData);
                                db.Entry(LoanAdvReqData).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(LoanAdvReqData).State = System.Data.Entity.EntityState.Detached;
                            }

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
        public class returnDataClass
        {
            public string LoanAdvanceHeadPFT { get; set; }
            public double TotalInstallement { get; set; }
            public bool ISActive { get; set; }
            public string CloserDate { get; set; }
            public string LoanAccBranch { get; set; }
            public string LoanAccNo { get; set; }
        }

        public ActionResult GridEditData(int data)
        {
            var returnlist = new List<returnDataClass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null && data != 0)
                {
                    var retrundataList = db.LoanAdvRequestPFT.Where(e => e.Id == data && e.IsActive == true).Include(e => e.LoanAccBranch).Include(e => e.LoanAdvanceHeadPFT).ToList();
                    foreach (var a in retrundataList)
                    {
                        returnlist.Add(new returnDataClass()
                        {
                            LoanAdvanceHeadPFT = a.LoanAdvanceHeadPFT.Name,
                            TotalInstallement = a.TotalInstall,
                            LoanAccBranch = a.LoanAccBranch != null ? a.LoanAccBranch.Id.ToString() : null,
                            LoanAccNo = a.LoanAccNo,
                            ISActive = a.IsActive,
                            CloserDate = a.CloserDate != null ? a.CloserDate.Value.ToString("dd/MM/yyyy") : ""
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
        public class LoanAdvReqChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string LoanAdvHeadPFT { get; set; }
            public string ReqDate { get; set; }
            public string EnforcementDate { get; set; }
            public double LoanAppliedAmt { get; set; }
            public double LoanSancAmt { get; set; }
            public double MonthlyInstallAmt { get; set; }
            public double TotalInstall { get; set; }

            public string LoanAccBranch { get; set; }
            public string OriginalDate { get; set; }
            public string LoanAccNo { get; set; }
            public string LoanStatus { get; set; }
            public double OwnPFAmount { get; set; }
            public double OwnerPFAmount { get; set; }
            public double VPFAmount { get; set; }
            public double OwnPFIntAmount { get; set; }
            public double OwnerPFIntAmount { get; set; }

            public double VPFIntAmount { get; set; }



        }
        public ActionResult Get_LoanAdvReq(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePFTrust
                        .Include(e => e.LoanAdvRequestPFT)
                        .Include(e => e.LoanAdvRequestPFT.Select(a => a.LoanAdvanceHeadPFT))
                        .Include(e => e.LoanAdvRequestPFT.Select(a => a.LoanAccBranch))
                        .Include(e => e.LoanAdvRequestPFT.Select(a => a.LoanAccBranch.LocationObj))
                        .Where(e => e.Employee_Id == data).SingleOrDefault();
                    if (db_data.LoanAdvRequestPFT != null)
                    {
                        List<LoanAdvReqChildDataClass> returndata = new List<LoanAdvReqChildDataClass>();

                        foreach (var item in db_data.LoanAdvRequestPFT.OrderByDescending(e => e.Id))
                        {
                            returndata.Add(new LoanAdvReqChildDataClass
                            {
                                Id = item.Id,
                                EnforcementDate = item.EnforcementDate.Value != null ? item.EnforcementDate.Value.ToShortDateString() : "",
                                LoanAdvHeadPFT = item.LoanAdvanceHeadPFT != null ? item.LoanAdvanceHeadPFT.Name : "",
                                LoanAppliedAmt = item.LoanAppliedAmount,
                                LoanAccNo = item.LoanAccNo,
                                LoanAccBranch = item.LoanAccBranch != null ? item.LoanAccBranch.FullDetails : null,
                                LoanSancAmt = item.LoanSanctionedAmount,
                                MonthlyInstallAmt = item.MonthlyInstallmentAmount,
                                ReqDate = item.RequisitionDate.Value != null ? item.RequisitionDate.Value.ToShortDateString() : "",
                                TotalInstall = item.TotalInstall,
                                LoanStatus = item.IsActive == true ? "Active" : "Closed",
                                OwnPFAmount=item.OwnPFAmount,
                                OwnerPFAmount=item.OwnerPFAmount,
                                VPFAmount=item.VPFAmount,
                                OwnPFIntAmount=item.OwnPFIntAmount,
                                OwnerPFIntAmount= item.OwnerPFIntAmount,
                                VPFIntAmount = item.VPFIntAmount






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
    
        public ActionResult GetContributionperct(string Emp,string Codes)
        {
            using (DataBaseContext db = new DataBaseContext())
            { 
                string nonref = "";
                double OwnContibutionPercent = 0;
                double OwnerContibutionPercent = 0;
                double VPFContibutionPercent = 0;
                double OwnIntContibutionPercent = 0;
                double OwnerIntContibutionPercent = 0;
                double VPFIntContibutionPercent = 0;

                //int Ids = Convert.ToInt32(ids);
                int EmpIds = Convert.ToInt32(Emp);

                double OwnCloseBal = 0;
                double OwnerCloseBal = 0;
                double VPFCloseBal = 0;
                double OwnIntCloseBal = 0;
                double OwnerIntCloseBal = 0;
                double VPFIntCloseBal = 0;
                var findtext = (String.Join(" ", Codes.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)));
                string iCode = findtext.ToString();
                var ObjLoanAdvanceHeadPFT = db.LoanAdvanceHeadPFT
                    .Include(e => e.LoanAdvancePolicyPFT)
                    .Include(e=>e.LoanAdvancePolicyPFT.PFLoanType)
                    .Where(e => e.Code == iCode).SingleOrDefault();
                if (ObjLoanAdvanceHeadPFT!=null)
                {
                     LoanAdvancePolicyPFT ObjLoanAdvancePolicyPFT = ObjLoanAdvanceHeadPFT.LoanAdvancePolicyPFT;
                     nonref = ObjLoanAdvancePolicyPFT.PFLoanType.LookupVal;
                     OwnContibutionPercent = ObjLoanAdvancePolicyPFT.OwnContibutionPercent;
                     OwnerContibutionPercent = ObjLoanAdvancePolicyPFT.OwnerContibutionPercent;
                     VPFContibutionPercent = ObjLoanAdvancePolicyPFT.VPFContibutionPercent;
                     OwnIntContibutionPercent = ObjLoanAdvancePolicyPFT.OwnIntContibutionPercent;
                     OwnerIntContibutionPercent = ObjLoanAdvancePolicyPFT.OwnerIntContibutionPercent;
                     VPFIntContibutionPercent = ObjLoanAdvancePolicyPFT.VPFIntContibutionPercent;                  
                }                
                
                var ObjEmployeePFTrust = db.EmployeePFTrust.Include(e => e.PFTEmployeeLedger).Where(e => e.Employee_Id == EmpIds).FirstOrDefault();
                if (ObjEmployeePFTrust!=null)
                {
                    PFTEmployeeLedger ObjPFTEmployeeLedger = ObjEmployeePFTrust.PFTEmployeeLedger.ToList().OrderByDescending(t => t.Id).FirstOrDefault();
                     OwnCloseBal = ObjPFTEmployeeLedger.OwnCloseBal;
                     OwnerCloseBal = ObjPFTEmployeeLedger.OwnerCloseBal;
                     VPFCloseBal = ObjPFTEmployeeLedger.VPFCloseBal;
                     OwnIntCloseBal = ObjPFTEmployeeLedger.OwnIntCloseBal;
                     OwnerIntCloseBal = ObjPFTEmployeeLedger.OwnerIntCloseBal;
                     VPFIntCloseBal = ObjPFTEmployeeLedger.VPFIntCloseBal;
                    
                }               
                //---------------------------------------------
                double FindOwnCloseBal = Math.Round((OwnCloseBal * OwnContibutionPercent) / 100,0);
                double FindOwnerCloseBal = Math.Round((OwnerCloseBal * OwnerContibutionPercent) / 100,0);
                double FindVPFCloseBal = Math.Round((VPFCloseBal * VPFContibutionPercent) / 100,0);
                double FindOwnIntCloseBal = Math.Round((OwnIntCloseBal * OwnIntContibutionPercent) / 100,0);
                double FindOwnerIntCloseBal = Math.Round((OwnerIntCloseBal * OwnerIntContibutionPercent) / 100,0);
                double FindVPFIntCloseBal = Math.Round((VPFIntCloseBal * VPFIntContibutionPercent) / 100,0);
                //---------------------------------------------

                return Json(new Object[] { FindOwnCloseBal, FindOwnerCloseBal, FindVPFCloseBal, FindOwnIntCloseBal, FindOwnerIntCloseBal, FindVPFIntCloseBal,OwnCloseBal,OwnerCloseBal,VPFCloseBal,OwnIntCloseBal,OwnerIntCloseBal,VPFIntCloseBal,nonref, JsonRequestBehavior.AllowGet });
            }
        }

        public ActionResult GetLoanAdvHeadLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.LoanAdvanceHeadPFT.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LoanAdvanceHeadPFT.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult Create(LoanAdvRequestPFT LoanAdvReq, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                try
                {                
                    double ObjLoanSanctionedAmount = LoanAdvReq.LoanSanctionedAmount;
                    double ObjLoanAppliedAmount = LoanAdvReq.LoanAppliedAmount;

                    double FindTotalAmount=(LoanAdvReq.OwnPFAmount+LoanAdvReq.OwnerPFAmount)
                                           +(LoanAdvReq.VPFAmount+LoanAdvReq.OwnPFIntAmount)
                                           +(LoanAdvReq.OwnerPFIntAmount+LoanAdvReq.VPFIntAmount);

                    if (ObjLoanSanctionedAmount>FindTotalAmount)
                    {
                         Msg.Add(" LoanSanctionedAmount Should not be greater than PF Loan Balance (OwnPFAmount+OwnerPFAmount+VPFAmount+OwnPFIntAmount+OwnerPFIntAmount+VPFIntAmount");
                         return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);                  
                    }

                    if (ObjLoanAppliedAmount > FindTotalAmount)
                    {
                        Msg.Add(" LoanAppliedAmount Should not be greater than PF Loan Balance (OwnPFAmount+OwnerPFAmount+VPFAmount+OwnPFIntAmount+OwnerPFIntAmount+VPFIntAmount");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    string LoanAdvhead = form["LoanAdvanceHeadlist"] == "0" ? "" : form["LoanAdvanceHeadlist"];

                    //string labelOwnpfcrta = form["labelOwnpfcrta"] == "0" ? "" : form["labelOwnpfcrta"];

                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string LoanAccBranch = form["LoanAccBranchlist"] == "0" ? "" : form["LoanAccBranchlist"];
                    string PaymentMode = form["PayMode_drop"] == "0" ? "" : form["PayMode_drop"];

                    if (PaymentMode != null && PaymentMode != "" && PaymentMode != "-Select-")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "404").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(PaymentMode)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(PayMode));
                        LoanAdvReq.PaymentMode = val;
                    }

                    if (LoanAdvhead != null && LoanAdvhead != "")
                    {
                        var val = db.LoanAdvanceHeadPFT.Find(int.Parse(LoanAdvhead));
                        LoanAdvReq.LoanAdvanceHeadPFT_Id = val.Id;
                    }
                    else
                    ////return this.Json(new Object[] { "", "", "Please select LoanAdvanceHead.", JsonRequestBehavior.AllowGet });
                    {
                        // List<string> Msgu = new List<string>();
                        Msg.Add("  Please select LoanAdvanceHead.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    if (LoanAccBranch != null && LoanAccBranch != "")
                    {
                        var val = db.Location.Find(int.Parse(LoanAccBranch));
                        LoanAdvReq.LoanAccBranch = val;
                    }
                    //var LoanAdvanceHeadPFT_Id = db.LoanAdvRequestPFT.Include(e => e.LoanAdvanceHeadPFT).Select(e => e.LoanAdvanceHeadPFT_Id).SingleOrDefault();


                    var EmployeePFTrust = db.EmployeePFTrust.Include(e =>e.Employee).Where(e=>e.Employee_Id==Emp).SingleOrDefault();
                    if(EmployeePFTrust == null)
                    {
                        Msg.Add("Please enter PF Opening Balance Then Try .");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }




                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            LoanAdvReq.DBTrack= new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<LoanAdvRequestPFT> LoanAdvRequestList = new List<LoanAdvRequestPFT>();
                            LoanAdvRequestPFT LoanAdvRequest = new LoanAdvRequestPFT()
                            {
                                CloserDate = LoanAdvReq.CloserDate,
                                EnforcementDate = LoanAdvReq.EnforcementDate == DateTime.Now.Date ? LoanAdvReq.SanctionedDate : LoanAdvReq.EnforcementDate,
                                IsActive = LoanAdvReq.IsActive,
                                IsInstallment = LoanAdvReq.IsInstallment,
                                LoanAccBranch = LoanAdvReq.LoanAccBranch,
                                LoanAccNo = LoanAdvReq.LoanAccNo,
                                LoanAdvanceHeadPFT_Id = LoanAdvReq.LoanAdvanceHeadPFT_Id,
                                LoanAppliedAmount = LoanAdvReq.LoanAppliedAmount,
                                LoanSanctionedAmount = LoanAdvReq.LoanSanctionedAmount,
                                LoanSubAccNo = LoanAdvReq.LoanSubAccNo,
                                MonthlyInstallmentAmount = LoanAdvReq.MonthlyInstallmentAmount,
                                MonthlyInterest = LoanAdvReq.MonthlyInterest,
                                MonthlyPricipalAmount = LoanAdvReq.MonthlyPricipalAmount,
                                Narration = LoanAdvReq.Narration,
                                RequisitionDate = LoanAdvReq.RequisitionDate,
                                SanctionedDate = LoanAdvReq.SanctionedDate,
                                TotalInstall = LoanAdvReq.TotalInstall,
                                OwnPFAmount= LoanAdvReq.OwnPFAmount,
                                OwnerPFAmount=LoanAdvReq.OwnerPFAmount,
                                VPFAmount= LoanAdvReq.VPFAmount,
                                OwnPFIntAmount= LoanAdvReq.OwnPFIntAmount,
                                OwnerPFIntAmount=LoanAdvReq.OwnerPFIntAmount,
                                VPFIntAmount=LoanAdvReq.VPFIntAmount,
                                BankBranchDetails=LoanAdvReq.BankBranchDetails,
                                ChequeNo=LoanAdvReq.ChequeNo,
                                ChequeDate=LoanAdvReq.ChequeDate,
                                PaymentMode=LoanAdvReq.PaymentMode,
                                DBTrack = LoanAdvReq.DBTrack
                            };
                            db.LoanAdvRequestPFT.Add(LoanAdvRequest);
                            LoanAdvRequestList.Add(LoanAdvRequest);
                            db.SaveChanges();
                          
                            List<EmployeePFTrust> EmployeePFTrustList = new List<EmployeePFTrust>();

                            EmployeePFTrustList.Add(EmployeePFTrust);
                            foreach(var emppftrustitem in EmployeePFTrustList)
                            {
                                emppftrustitem.LoanAdvRequestPFT = LoanAdvRequestList;
                                db.EmployeePFTrust.Attach(emppftrustitem);
                                db.Entry(emppftrustitem).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(emppftrustitem).State = System.Data.Entity.EntityState.Detached;
                            }
                           
                            List<LoanWFDetails> oAttWFDetails_List = new List<LoanWFDetails>();
                            LoanWFDetails LoanWFDetails = new LoanWFDetails
                            {
                                DBTrack = LoanAdvReq.DBTrack,
                                WFStatus = 0,
                                Comments = LoanAdvReq.Narration.ToString(),
                               
                                
                            };
                            db.LoanWFDetails.Add(LoanWFDetails);
                            db.SaveChanges();
                            oAttWFDetails_List.Add(LoanWFDetails);
                            LoanAdvRequest.LoanWFDetails = oAttWFDetails_List;

                           

                            //LoanAdvRequest.LoanWFDetails = oAttWFDetails_List;

                            //oAttWFDetails_List.Add(LoanWFDetails);
                            db.LoanAdvRequestPFT.Attach(LoanAdvRequest);
                            db.Entry(LoanAdvRequest).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            



                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //    try
                            //    {


                            //        List<LoanAdvRepaymentTPFT> LoanAdvRepaymentTDet = new List<LoanAdvRepaymentTPFT>();

                            //        if (LoanAdvRequest.EnforcementDate != null && LoanAdvReq.TotalInstall != 0)
                            //        {

                            //            for (int i = 0; i <= LoanAdvReq.TotalInstall - 1; i++)
                            //            {
                            //                double TotalLoanPaid = 0;

                            //                string Month = LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Month.ToString().Length == 1 ? "0" + LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Month.ToString() : LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Month.ToString();
                            //                string PayMonth = Month + "/" + LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Year;
                            //                LoanAdvRepaymentTPFT LoanAdvRepayT = new LoanAdvRepaymentTPFT()
                            //                {
                            //                    InstallementDate = LoanAdvRequest.EnforcementDate.Value.AddMonths(i),
                            //                    InstallmentAmount = LoanAdvReq.MonthlyInstallmentAmount,
                            //                    InstallmentCount = i + 1,
                            //                    InstallmentPaid = LoanAdvReq.MonthlyPricipalAmount + LoanAdvReq.MonthlyInterest,
                            //                    PayMonth = PayMonth,
                            //                    TotalLoanBalance = LoanAdvRequest.LoanSanctionedAmount,//LoanAdvRequest.LoanSanctionedAmount - installment_Paid,
                            //                    TotalLoanPaid = 0,//installment_Paid
                            //                    DBTrack = LoanAdvReq.DBTrack,
                            //                    MonthlyInterest = LoanAdvReq.MonthlyInterest,
                            //                    MonthlyPricipalAmount = LoanAdvReq.MonthlyPricipalAmount
                            //                };
                            //                TotalLoanPaid = TotalLoanPaid + LoanAdvRepayT.InstallmentPaid;
                            //                LoanAdvRepaymentTDet.Add(LoanAdvRepayT);
                            //            }

                            //        }
                            //        LoanAdvRequest.LoanAdvRepaymentTPFT = LoanAdvRepaymentTDet;
                            //db.LoanAdvRequestPFT.Add(LoanAdvRequest);
                            //db.SaveChanges();

                            //        //var OEmployeePayroll = db.LoanAdvRequestPFT.Where(e => e.LoanAdvanceHeadPFT_Id == Emp).SingleOrDefault();

                            //List<LoanAdvRequestPFT> LoanAdvReqList = new List<LoanAdvRequestPFT>();
                            //LoanAdvReqList.Add(LoanAdvRequest);
                            //        ////OEmployeePayroll.LoanAdvanceHeadPFT = LoanAdvReqList;
                            //        //db.LoanAdvRequestPFT.Attach(OEmployeePayroll);
                            //        //db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                            //        //db.SaveChanges();
                            //        //db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;

                            //        ts.Complete();
                            //        // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });

                            //        Msg.Add("Data Saved successfully");
                            //        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //    }
                            //    catch (DbUpdateConcurrencyException)
                            //    {
                            //        return RedirectToAction("Create", new { concurrencyError = true, id = LoanAdvReq.Id });
                            //    }
                            //    catch (DataException /* dex */)
                            //    {
                            //        return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        LogFile Logfile = new LogFile();
                            //        ErrorLog Err = new ErrorLog()
                            //        {
                            //            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                            //            ExceptionMessage = ex.Message,
                            //            ExceptionStackTrace = ex.StackTrace,
                            //            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                            //            LogTime = DateTime.Now
                            //        };
                            //        Logfile.CreateLogFile(Err);
                            //        //    List<string> Msg = new List<string>();
                            //        Msg.Add(ex.Message);
                            //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //    }
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
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LoanAdvRequestPFT LoanAdvReq = db.LoanAdvRequestPFT.Include(e => e.LoanAdvRepaymentTPFT).Where(e => e.Id == data).SingleOrDefault();

                if (LoanAdvReq.CloserDate != null)
                {
                    return this.Json(new { status = true, valid = true, responseText = "You cannot delete as loan is already closed.", JsonRequestBehavior.AllowGet });
                }

                var selectedRegions = LoanAdvReq.LoanAdvRepaymentTPFT;

                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    if (selectedRegions != null)
                    {
                        var corpRegion = new HashSet<int>(LoanAdvReq.LoanAdvRepaymentTPFT.Select(e => e.Id));
                        if (corpRegion.Count > 0)
                        {
                            var ChkSalGen = LoanAdvReq.LoanAdvRepaymentTPFT.Where(e => e.RepaymentDate == null).ToList();

                            if (ChkSalGen != null)
                            {
                                return this.Json(new { status = true, valid = true, responseText = "You cannot delete as salary is generated.", JsonRequestBehavior.AllowGet });
                            }


                        }
                    }

                    try
                    {
                        db.LoanAdvRepaymentTPFT.RemoveRange(selectedRegions);
                        db.LoanAdvRequestPFT.Remove(LoanAdvReq);

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
                        List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }
        public ActionResult LoanAccBranchDrop(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                //var id = Convert.ToInt32(data);
                var qurey = db.Location.Include(e => e.LocationObj).ToList(); // added by rekha 26-12-16
                if (data2 != "" && data2 != "0")
                {
                    selected = data2;
                }
                if (qurey != null) 
                {
                    s = new SelectList(qurey, "Id", "FullDetails", selected);
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }



    }
}