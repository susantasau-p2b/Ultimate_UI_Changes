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
    public class LoanAdvRequestController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /LoanAdvRequest/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/LoanAdvRequest/Index.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_LoanAdvRequestGridPartiall.cshtml");
        }

        public ActionResult PopulateDropDownLocList(List<int> SkipIds)
        {
            //using (DataBaseContext db = new DataBaseContext())
            //{
            //    var qurey = db.Location.Include(e => e.LocationObj).ToList();
            //    var selected = (Object)null;
            //    if (data2 != "" && data != "0" && data2 != "0")
            //    {
            //        selected = Convert.ToInt32(data2);
            //    }

            //    SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
            //    return Json(s, JsonRequestBehavior.AllowGet);
            //}

            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Location.Include(e => e.LocationObj).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Location.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetLoanAdvHeadLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.LoanAdvanceHead.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LoanAdvanceHead.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ValidateForm(FormCollection form)
        {
            string LoanAdvhead = form["LoanAdvanceHeadlist"] == "0" ? "" : form["LoanAdvanceHeadlist"];
            int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
            List<string> Msg = new List<string>();


            if (LoanAdvhead == null)
            {
                Msg.Add("Please select loan advance head");
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
        public ActionResult Create(LoanAdvRequest LoanAdvReq, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                try
                {
                    string LoanAdvhead = form["LoanAdvanceHeadlist"] == "0" ? "" : form["LoanAdvanceHeadlist"];
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string LoanAccBranch = form["LoanAccBranchlist"] == "0" ? "" : form["LoanAccBranchlist"];

                    if (LoanAdvhead != null && LoanAdvhead != "")
                    {
                        var val = db.LoanAdvanceHead.Find(int.Parse(LoanAdvhead));
                        LoanAdvReq.LoanAdvanceHead = val;
                    }
                    else
                    //return this.Json(new Object[] { "", "", "Please select LoanAdvanceHead.", JsonRequestBehavior.AllowGet });
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
                    else
                    {
                        Msg.Add("  Please select Loan Account Branch.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            LoanAdvReq.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            LoanAdvRequest LoanAdvRequest = new LoanAdvRequest()
                            {
                                CloserDate = LoanAdvReq.CloserDate,
                                EnforcementDate = LoanAdvReq.EnforcementDate,
                                IsActive = LoanAdvReq.IsActive,
                                IsInstallment = LoanAdvReq.IsInstallment,
                                LoanAccBranch = LoanAdvReq.LoanAccBranch,
                                LoanAccNo = LoanAdvReq.LoanAccNo,
                                LoanAdvanceHead = LoanAdvReq.LoanAdvanceHead,
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
                                DBTrack = LoanAdvReq.DBTrack
                            };
                            try
                            {


                                List<LoanAdvRepaymentT> LoanAdvRepaymentTDet = new List<LoanAdvRepaymentT>();

                                if (LoanAdvRequest.EnforcementDate != null && LoanAdvReq.TotalInstall != 0)
                                {

                                    for (int i = 0; i <= LoanAdvReq.TotalInstall - 1; i++)
                                    {
                                        double TotalLoanPaid = 0;

                                        string Month = LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Month.ToString().Length == 1 ? "0" + LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Month.ToString() : LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Month.ToString();
                                        string PayMonth = Month + "/" + LoanAdvRequest.EnforcementDate.Value.AddMonths(i).Year;
                                        LoanAdvRepaymentT LoanAdvRepayT = new LoanAdvRepaymentT()
                                        {
                                            InstallementDate = LoanAdvRequest.EnforcementDate.Value.AddMonths(i),
                                            InstallmentAmount = LoanAdvReq.MonthlyInstallmentAmount,
                                            InstallmentCount = i + 1,
                                            InstallmentPaid = LoanAdvReq.MonthlyPricipalAmount + LoanAdvReq.MonthlyInterest,
                                            PayMonth = PayMonth,
                                            TotalLoanBalance = LoanAdvRequest.LoanSanctionedAmount,//LoanAdvRequest.LoanSanctionedAmount - installment_Paid,
                                            TotalLoanPaid = 0,//installment_Paid
                                            DBTrack = LoanAdvReq.DBTrack,
                                            MonthlyInterest = LoanAdvReq.MonthlyInterest,
                                            MonthlyPricipalAmount = LoanAdvReq.MonthlyPricipalAmount
                                        };
                                        TotalLoanPaid = TotalLoanPaid + LoanAdvRepayT.InstallmentPaid;
                                        LoanAdvRepaymentTDet.Add(LoanAdvRepayT);
                                    }

                                }
                                LoanAdvRequest.LoanAdvRepaymentT = LoanAdvRepaymentTDet;
                                db.LoanAdvRequest.Add(LoanAdvRequest);
                                db.SaveChanges();

                                var OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == Emp).SingleOrDefault();

                                List<LoanAdvRequest> LoanAdvReqList = new List<LoanAdvRequest>();
                                LoanAdvReqList.Add(LoanAdvRequest);
                                OEmployeePayroll.LoanAdvRequest = LoanAdvReqList;
                                db.EmployeePayroll.Attach(OEmployeePayroll);
                                db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;

                                ts.Complete();
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                List<string> Msgs = new List<string>();
                                Msgs.Add("Data Saved successfully");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = LoanAdvReq.Id });
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

        public class returnDataClass
        {
            public string LoanAdvanceHead { get; set; }
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
                    var retrundataList = db.LoanAdvRequest.Where(e => e.Id == data && e.IsActive == true).Include(e => e.LoanAccBranch).Include(e => e.LoanAdvanceHead).ToList();
                    foreach (var a in retrundataList)
                    {
                        returnlist.Add(new returnDataClass()
                        {
                            LoanAdvanceHead = a.LoanAdvanceHead.Name,
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
        public ActionResult GridEditSave(LoanAdvRequest LoanAdvReq, FormCollection form, string data)
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                var LoanAdvReqData = db.LoanAdvRequest.Include(e => e.LoanAdvRepaymentT).Where(e => e.Id == Id).SingleOrDefault();
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
                    //var ChkSalGen = LoanAdvReqData.LoanAdvRepaymentT.Where(e => e.RepaymentDate == null).ToList();

                    //if (ChkSalGen == null)
                    //{
                    //    return this.Json(new Object[] { "", "", "You cannot change as salary is generated.", JsonRequestBehavior.AllowGet });
                    //}
                    LoanAdvReqData.DBTrack = new DBTrack
                    {
                        CreatedBy = LoanAdvReqData.DBTrack.CreatedBy == null ? null : LoanAdvReqData.DBTrack.CreatedBy,
                        CreatedOn = LoanAdvReqData.DBTrack.CreatedOn == null ? null : LoanAdvReqData.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };

                    //if (LoanAdvReq.CloserDate != null)
                    //{
                    //    LoanAdvReqData.CloserDate = LoanAdvReq.CloserDate;
                    //    LoanAdvReqData.IsActive = LoanAdvReq.IsActive;
                    //    LoanAdvReqData.DBTrack = LoanAdvReq.DBTrack;
                    //}
                    //else
                    //{ 
                    //    LoanAdvReqData.TotalInstall = LoanAdvReq.TotalInstall;
                    //    LoanAdvReqData.DBTrack = LoanAdvReq.DBTrack;
                    //}


                    try
                    {

                        if (LoanAdvReqData.CloserDate != null)
                        {
                            IEnumerable<LoanAdvRepaymentT> LoanAdvRepaymentTDet = LoanAdvReqData.LoanAdvRepaymentT.Where(e => e.InstallementDate > LoanAdvReqData.CloserDate).ToList();
                            db.LoanAdvRepaymentT.RemoveRange(LoanAdvRepaymentTDet);
                            //foreach (var LoanAdvRepay in LoanAdvRepaymentTDet)
                            //{
                            //    db.Entry(LoanAdvRepay).State = System.Data.Entity.EntityState.Deleted;
                            //    db.SaveChanges();
                            //}

                            db.LoanAdvRequest.Attach(LoanAdvReqData);
                            db.Entry(LoanAdvReqData).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(LoanAdvReqData).State = System.Data.Entity.EntityState.Detached;
                        }
                        else
                        {
                            IEnumerable<LoanAdvRepaymentT> LoanAdvRepaymentTDetails = LoanAdvReqData.LoanAdvRepaymentT.ToList();
                            double LoanPaid = LoanAdvRepaymentTDetails.LastOrDefault().InstallmentPaid;
                            DateTime? LastInstallmentDt = LoanAdvRepaymentTDetails.LastOrDefault().InstallementDate;
                            double TotalLoanBalance = LoanAdvRepaymentTDetails.LastOrDefault().TotalLoanBalance;

                            List<LoanAdvRepaymentT> LoanAdvRepaymentTDet = new List<LoanAdvRepaymentT>();

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
                                        LoanAdvRepaymentT LoanAdvRepayT = new LoanAdvRepaymentT()
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
                                    var RepayDel = LoanAdvReqData.LoanAdvRepaymentT.OrderByDescending(e => e.Id).Take(NewCount).ToList();
                                    db.LoanAdvRepaymentT.RemoveRange(RepayDel);
                                    LoanAdvReqData.TotalInstall = NewInsCount;
                                }


                            }

                            db.LoanAdvRequest.Attach(LoanAdvReqData);
                            db.Entry(LoanAdvReqData).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(LoanAdvReqData).State = System.Data.Entity.EntityState.Detached;


                            //////IEnumerable<LoanAdvRepaymentT> LoanAdvRepaymentTDetails = LoanAdvReqData.LoanAdvRepaymentT.ToList();


                            //////foreach (var LoanAdvRepay in LoanAdvRepaymentTDetails)
                            //////{
                            //////    db.Entry(LoanAdvRepay).State = System.Data.Entity.EntityState.Deleted;
                            //////    db.SaveChanges();
                            //////}

                            //////List<LoanAdvRepaymentT> LoanAdvRepaymentTDet = new List<LoanAdvRepaymentT>();

                            //////if (LoanAdvReqData.EnforcementDate != null && LoanAdvReqData.TotalInstall != 0)
                            //////{

                            //////    for (int i = 0; i <= LoanAdvReqData.TotalInstall - 1; i++)
                            //////    {
                            //////        //double installment_Paid = 0;
                            //////        //installment_Paid = installment_Paid + InstallmentAmount;
                            //////        string Month = LoanAdvReqData.EnforcementDate.Value.AddMonths(i).Month.ToString().Length == 1 ? "0" + LoanAdvReqData.EnforcementDate.Value.AddMonths(i).Month.ToString() : LoanAdvReqData.EnforcementDate.Value.AddMonths(i).Month.ToString();
                            //////        string PayMonth = Month + "/" + LoanAdvReqData.EnforcementDate.Value.AddMonths(i).Year;
                            //////        LoanAdvRepaymentT LoanAdvRepayT = new LoanAdvRepaymentT()
                            //////        {
                            //////            InstallementDate = LoanAdvReqData.EnforcementDate.Value.AddMonths(i),
                            //////            InstallmentAmount = LoanAdvReqData.MonthlyInstallmentAmount,
                            //////            InstallmentCount = i + 1,
                            //////            InstallmentPaid = 0,
                            //////            PayMonth = PayMonth,
                            //////            RepaymentDate = null,
                            //////            TotalLoanBalance = 0,//LoanAdvRequest.LoanSanctionedAmount - installment_Paid,
                            //////            TotalLoanPaid = 0,//installment_Paid
                            //////            DBTrack = LoanAdvReqData.DBTrack
                            //////        };
                            //////        LoanAdvRepaymentTDet.Add(LoanAdvRepayT);
                            //////    }

                            //////}
                            //////LoanAdvReqData.LoanAdvRepaymentT = LoanAdvRepaymentTDet;
                            //////db.LoanAdvRequest.Attach(LoanAdvReqData);
                            //////db.Entry(LoanAdvReqData).State = System.Data.Entity.EntityState.Modified;
                            //////db.SaveChanges();
                            //////db.Entry(LoanAdvReqData).State = System.Data.Entity.EntityState.Detached;

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

        public class LoanAdvReqChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string LoanAdvHead { get; set; }
            public string ReqDate { get; set; }
            public string EnforcementDate { get; set; }
            public double LoanAppliedAmt { get; set; }
            public double LoanSancAmt { get; set; }
            public double MonthlyInstallAmt { get; set; }
            public double TotalInstall { get; set; }
            public string MonthlyPricipalAmount { get; set; }
            public string MonthlyInterest { get; set; }
            public string LoanAccBranch { get; set; }
            public string OriginalDate { get; set; }
            public string LoanAccNo { get; set; }
            public string LoanStatus { get; set; }
            public string CloserDate { get; set; }
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
                        //.Include(e => e.Employee.FuncStruct)
                        //   .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct)
                        //  .Include(e => e.Employee.PayStruct.Grade)
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
                        // fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.Employee.EmpCode.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.Employee.EmpName.FullNameFML.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            //   || (e.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy").Contains(param.sSearch))
                            //   || (e.Employee.FuncStruct.Job.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            // || (e.Employee.PayStruct.Grade.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            //  || (e.Employee.GeoStruct.Location.LocationObj.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
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
                                //    JoiningDate = item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy") : null,
                                //    Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                                //   Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
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

        public ActionResult Get_LoanAdvReq(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.LoanAdvRequest)
                        .Include(e => e.LoanAdvRequest.Select(a => a.LoanAdvanceHead))
                        .Include(e => e.LoanAdvRequest.Select(a => a.LoanAccBranch))
                        .Include(e => e.LoanAdvRequest.Select(a => a.LoanAccBranch.LocationObj))
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data.LoanAdvRequest != null)
                    {
                        List<LoanAdvReqChildDataClass> returndata = new List<LoanAdvReqChildDataClass>();

                        foreach (var item in db_data.LoanAdvRequest.OrderByDescending(e => e.Id))
                        {
                            returndata.Add(new LoanAdvReqChildDataClass
                            {
                                Id = item.Id,
                                EnforcementDate = item.EnforcementDate.Value != null ? item.EnforcementDate.Value.ToShortDateString() : "",
                                LoanAdvHead = item.LoanAdvanceHead != null ? item.LoanAdvanceHead.Name : "",
                                LoanAppliedAmt = item.LoanAppliedAmount,
                                LoanAccNo = item.LoanAccNo,
                                LoanAccBranch = item.LoanAccBranch != null ? item.LoanAccBranch.FullDetails : null,
                                LoanSancAmt = item.LoanSanctionedAmount,
                                MonthlyInstallAmt = item.MonthlyInstallmentAmount,
                                ReqDate = item.RequisitionDate.Value != null ? item.RequisitionDate.Value.ToShortDateString() : "",
                                TotalInstall = item.TotalInstall,
                                LoanStatus = item.IsActive == true ? "Active" : "Closed",
                                MonthlyInterest=item.MonthlyInterest.ToString(),
                                MonthlyPricipalAmount=item.MonthlyPricipalAmount.ToString(),
                                CloserDate = item.CloserDate !=null ? item.CloserDate.Value.ToShortDateString() : ""

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

        public ActionResult CheckSalaryOfMonth(string Paymonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var chksalaryt = db.SalaryT.Where(q => q.PayMonth == Paymonth).FirstOrDefault();
                //var chkcpit = db.CPIEntryT.Where(q => q.PayMonth == Paymonth).FirstOrDefault();// coi checking because perk upload update structure current month
                //if (chkcpit==null)
                //{
                //     return this.Json(new { status = false, responseText = "Kindly Genrate CPI before proceeding further.", JsonRequestBehavior.AllowGet });
                //}
                if (chksalaryt != null)
                {
                    if (chksalaryt.ReleaseDate == null)
                    {
                        return this.Json(new { status = false, responseText = "Kindly delete the salary before proceeding further.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        return this.Json(new { status = false, responseText = "Salary is released for this month, you can't make changes now", JsonRequestBehavior.AllowGet });

                    }
                }

                return this.Json(new { status = true, JsonRequestBehavior.AllowGet });
            }

        }


        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LoanAdvRequest LoanAdvReq = db.LoanAdvRequest.Include(e => e.LoanAdvRepaymentT).Where(e => e.Id == data).SingleOrDefault();

                if (LoanAdvReq.CloserDate != null)
                {
                    return this.Json(new { status = true, valid = true, responseText = "You cannot delete as loan is already closed.", JsonRequestBehavior.AllowGet });
                }

                var selectedRegions = LoanAdvReq.LoanAdvRepaymentT;

                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    if (selectedRegions != null)
                    {
                        var corpRegion = new HashSet<int>(LoanAdvReq.LoanAdvRepaymentT.Select(e => e.Id));
                        if (corpRegion.Count > 0)
                        {
                            var ChkSalGen = LoanAdvReq.LoanAdvRepaymentT.Where(e => e.RepaymentDate == null).ToList();

                            if (ChkSalGen != null)
                            {
                                return this.Json(new { status = true, valid = true, responseText = "You cannot delete as salary is generated.", JsonRequestBehavior.AllowGet });
                            }


                        }
                    }

                    try
                    {
                        db.LoanAdvRepaymentT.RemoveRange(selectedRegions);
                        db.LoanAdvRequest.Remove(LoanAdvReq);

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
    }
}