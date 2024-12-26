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
    public class DIARYAdvanceClaimController : Controller
    {
        //
        // GET: /DIARYAdvanceClaim/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/DIARYAdvanceClaim/Index.cshtml");
        }
        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_DiaryAdvClaimGridPartial.cshtml");
        }
        public class returnDataClass
        {
            public string ProposedPlace { get; set; }
            public double EligibleAmount { get; set; }
            public double AdvanceAmount { get; set; }
            public string Remark { get; set; }

        }

        public ActionResult GridEditData(int data)
        {
            var returnlist = new List<returnDataClass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != 0)
                {
                    var retrundataList = db.DIARYAdvanceClaim.Where(e => e.Id == data).FirstOrDefault();

                    returnlist.Add(new returnDataClass()
                    {
                        ProposedPlace = retrundataList.ProposedPlace,
                        EligibleAmount = retrundataList.EligibleAmt,
                        AdvanceAmount = retrundataList.AdvAmt,
                        Remark = retrundataList.Remark

                    });

                    return Json(new { returndata = returnlist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }
        public JsonResult GetEmpSalStruct(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var a = db.PayScaleAgreement.Find(int.Parse(data));
                int Emp_Id = int.Parse(data);

                var QEmpSalStruct1 = db.EmpSalStruct//.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.EffectiveDate >= comparedate)
                                            .Include(e => e.EmpSalStructDetails)
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead));//.ToList();
                var EmpSalStructTotal = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == Emp_Id && e.EndDate == null)
                                              .ToList();
                var OEmpSalStruct = EmpSalStructTotal.Where(e => e.EndDate == null).SingleOrDefault();
                if (OEmpSalStruct != null)
                {

                    var b = OEmpSalStruct.EmpSalStructDetails
                        .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                     && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                     && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "DIARY"

                     )).FirstOrDefault().Amount;
                    return Json(b, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

            }
        }
        public ActionResult GridEditSave(DIARYAdvanceClaim DIARYAdvanceClaim, FormCollection form, string data)
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                var DIARYAdvClaimData = db.DIARYAdvanceClaim.Where(e => e.Id == Id).SingleOrDefault();
                string ProposedPlace = form["ProposedPlace"];

                double DIARYAdvanceAmount = Convert.ToDouble(form["DIARYAdvanceAmount"]);
                string Remark = form["Remark"];

                DIARYAdvClaimData.ProposedPlace = ProposedPlace;
                DIARYAdvClaimData.AdvAmt = DIARYAdvanceAmount;
                DIARYAdvClaimData.Remark = Remark;

                using (TransactionScope ts = new TransactionScope())
                {

                    DIARYAdvClaimData.DBTrack = new DBTrack
                    {
                        CreatedBy = DIARYAdvClaimData.DBTrack.CreatedBy == null ? null : DIARYAdvClaimData.DBTrack.CreatedBy,
                        CreatedOn = DIARYAdvClaimData.DBTrack.CreatedOn == null ? null : DIARYAdvClaimData.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };


                    try
                    {
                        db.DIARYAdvanceClaim.Attach(DIARYAdvClaimData);
                        db.Entry(DIARYAdvClaimData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(DIARYAdvClaimData).State = System.Data.Entity.EntityState.Detached;

                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data Updated Successfully.", JsonRequestBehavior.AllowGet });
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = DIARYAdvClaimData.Id });
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
        [HttpPost]
        public ActionResult Create(DIARYAdvanceClaim DIARYAdvClaim, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                try
                {
                    // string LvReqList = form["LvReqList"] == "0" ? "" : form["LvReqList"];
                    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                    string DIARYType = form["DIARYTypelist"] == "0" ? "" : form["DIARYTypelist"];
                    string advamt = form["AdvAmt"] == "0" ? "0" : form["AdvAmt"];
                    string Sancamt = form["SanctionedAmt"] == "0" ? "0" : form["SanctionedAmt"];
                    if (Convert.ToDouble(Sancamt) > Convert.ToDouble(advamt))
                    {
                        Msg.Add("  Sanction amount should not be greater than DIARY advance amount.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    // string LtcBlock = form["BlockId"] == "0" ? "" : form["BlockId"];

                    //if (LvReqList != null && LvReqList != "")
                    //{
                    //    var val = db.LvNewReq.Find(int.Parse(LvReqList));
                    //    DIARYAdvClaim.LvNewReq = val;
                    //}
                    //else
                    //{
                    //    Msg.Add("  Please select leave requisition.  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    //if (LtcBlock == null && LtcBlock == "")
                    //{
                    //    Msg.Add("  Please select LTC Block.  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}


                    if (DIARYType != null && DIARYType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(DIARYType));
                        DIARYAdvClaim.DIARYType = val;
                    }

                    //int Blockid = int.Parse(LtcBlock);
                    //EmpLTCBlockT OEmpLTCBlockT = db.EmpLTCBlockT.Find(Blockid);

                    //    var a = db.EmployeePayroll.Include(e => e.EmpLTCBlock).Include(e => e.Employee)
                    //.Include(e => e.EmpLTCBlock.Select(y => y.EmpLTCBlockT))
                    //.Where(e => e.Employee.Id == Emp).AsNoTracking().SingleOrDefault().EmpLTCBlock.Where(e => e.IsBlockClose == false).FirstOrDefault();

                    //    var test = a.EmpLTCBlockT.Where(e => e.BlockPeriodStart >= OEmpLTCBlockT.BlockPeriodStart
                    //       && e.BlockPeriodEnd <= OEmpLTCBlockT.BlockPeriodEnd).FirstOrDefault();


                    //    if (test.DIARYAdvanceClaim != null)
                    //    {
                    //        Msg.Add("Record Already Exists.");
                    //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    }

                    //if (DIARYAdvClaim.LvNewReq.FromDate <= DIARYAdvClaim.TravelStartDate && DIARYAdvClaim.LvNewReq.ToDate >= DIARYAdvClaim.TravelEndDate)
                    //{
                    //    Msg.Add("Leave requisition date should be between travel start date & end date.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    EmployeePayroll EmpData;
                    if (Emp != 0)
                    {
                        int em = Convert.ToInt32(Emp);
                        // EmpData = db.Employee.Include(q => q.FamilyDetails).Where(q => q.Id == em).SingleOrDefault();
                        EmpData = db.EmployeePayroll.Include(q => q.DIARYAdvanceClaim).Where(e => e.Employee.Id == em).SingleOrDefault();

                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (DIARYAdvClaim.TravelEndDate < DIARYAdvClaim.TravelStartDate)
                    {
                        Msg.Add("Travel End date Should Not be Less than Start Date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    Employee OEmployee = null;

                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                           .Where(r => r.Id == Emp).SingleOrDefault();




                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            DIARYAdvClaim.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<DIARYAdvanceClaim> DIARYAdvanceClaim = new List<DIARYAdvanceClaim>();

                            DIARYAdvanceClaim DIARYAdvanceClaim1 = new DIARYAdvanceClaim()
                            {
                                DateOfAppl = DIARYAdvClaim.DateOfAppl,
                                IsClaimSettle = DIARYAdvClaim.IsClaimSettle,
                                EligibleAmt = DIARYAdvClaim.EligibleAmt,
                                DIARYType = DIARYAdvClaim.DIARYType,
                                AdvAmt = DIARYAdvClaim.AdvAmt,
                                // LvNewReq = DIARYAdvClaim.LvNewReq,
                                ProposedPlace = DIARYAdvClaim.ProposedPlace,
                                Remark = DIARYAdvClaim.Remark,
                                DBTrack = DIARYAdvClaim.DBTrack,
                                TravelStartDate = DIARYAdvClaim.TravelStartDate,
                                TravelEndDate = DIARYAdvClaim.TravelEndDate,
                                SanctionedAmt = DIARYAdvClaim.SanctionedAmt
                            };
                            try
                            {
                                db.DIARYAdvanceClaim.Add(DIARYAdvanceClaim1);
                                db.SaveChanges();
                                DIARYAdvanceClaim.Add(db.DIARYAdvanceClaim.Find(DIARYAdvanceClaim1.Id));

                                //  List<DIARYAdvanceClaim> DIARYAdvanceClaimlist = new List<DIARYAdvanceClaim>();

                                if (EmpData.DIARYAdvanceClaim.Count() > 0)
                                {
                                    DIARYAdvanceClaim.AddRange(EmpData.DIARYAdvanceClaim);
                                }

                                EmpData.DIARYAdvanceClaim = DIARYAdvanceClaim;
                                db.EmployeePayroll.Attach(EmpData);
                                db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(EmpData).State = System.Data.Entity.EntityState.Detached;

                                ts.Complete();
                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                List<string> Msgs = new List<string>();
                                Msgs.Add("Data Saved successfully");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = DIARYAdvanceClaim1.Id });
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

        public class DIARYAdvClaimChildDataClass //childgrid
        {
            public int Id { get; set; }

            public string ReqDate { get; set; }
            public string DIARYType { get; set; }
            public double EligibleAmt { get; set; }
            public double AdvanceAmt { get; set; }
            public string Remark { get; set; }
            public string ProposedPlace { get; set; }
            public string ClaimSettleStatus { get; set; }
            public string TravelStartDate { get; set; }
            public string TravelEndDate { get; set; }
        }


        public ActionResult Get_DIARYAdvClaim(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.DIARYAdvanceClaim)
                        .Include(e => e.DIARYAdvanceClaim.Select(c => c.DIARYType))

                        .Where(e => e.Id == data).FirstOrDefault();
                    if (db_data.DIARYAdvanceClaim != null)
                    {
                        List<DIARYAdvClaimChildDataClass> returndata = new List<DIARYAdvClaimChildDataClass>();
                        var LTCAdvClaimlist = db_data.DIARYAdvanceClaim;

                        foreach (var item2 in LTCAdvClaimlist)
                        {
                            returndata.Add(new DIARYAdvClaimChildDataClass
                            {
                                Id = item2.Id,

                                ReqDate = item2.DateOfAppl.Value != null ? item2.DateOfAppl.Value.ToShortDateString() : "",
                                DIARYType = item2.DIARYType != null ? item2.DIARYType.LookupVal.ToString() : null,
                                EligibleAmt = item2.EligibleAmt,
                                AdvanceAmt = item2.AdvAmt,
                                Remark = item2.Remark,
                                ProposedPlace = item2.ProposedPlace,
                                ClaimSettleStatus = item2.IsClaimSettle == true ? "true" : "false",
                                TravelStartDate = item2.TravelStartDate.Value != null ? item2.TravelStartDate.Value.ToShortDateString() : "",
                                TravelEndDate = item2.TravelEndDate.Value != null ? item2.TravelEndDate.Value.ToShortDateString() : ""
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
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DIARYAdvanceClaim DIARYAdvance = db.DIARYAdvanceClaim.Where(e => e.Id == data).SingleOrDefault();



                //  var selectedRegions = HotelbookReq.EmployeeDocuments;

                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {


                    try
                    {
                        // db.EmployeeDocuments.RemoveRange(selectedRegions);
                        db.DIARYAdvanceClaim.Remove(DIARYAdvance);

                        //   db.Entry(LoanAdvReq).State = System.Data.Entity.EntityState.Deleted;

                        db.SaveChanges();
                        //await db.SaveChangesAsync();


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

    }
}