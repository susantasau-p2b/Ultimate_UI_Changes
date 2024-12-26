using P2b.Global;
using P2BUltimate.App_Start;
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
using Appraisal;
using P2BUltimate.Models;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Appraisal.MainController
{
    public class EmpAppraisalCancelReqController : Controller
    {
        
        // GET: /EmpAppraisalCancelReq/
        public ActionResult Index()
        {
            return View("~/Views/Appraisal/MainViews/EmpAppraisalCancelReq/Index.cshtml");
        }

        public class returnDataClass //childgrid
        {
            public int Id { get; set; }
            public string BA_EmpTarget_Fulldetails { get; set; }
            public string AccountNo { get; set; }
            public string CustomerId { get; set; }
            public string TargetAmount { get; set; }
            public string TargetAmountCredit { get; set; }
            public string TargetAmountBalance { get; set; }
        }

        public class returndatagridDataClass
        {
            public int Id { get; set; }
            public string FulDetails { get; set; }
        }
       

        public class returnCalendarClass
        {
            public int Id { get; set; }
            public string FulDetails { get; set; }
        }

        public ActionResult AppraisalCalenderDropdownlist(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "AppraisalCalendar".ToUpper() && e.Default == true).AsEnumerable().ToList();
                List<returnCalendarClass> returndata = new List<returnCalendarClass>();
                foreach (var item in qurey)
                {
                    returndata.Add(new returnCalendarClass
                    {
                        Id = item.Id,
                        FulDetails ="From Date:"+" "+item.FromDate.Value.ToShortDateString()+"  ,"+"To Date:"+" "+item.ToDate.Value.ToShortDateString()

                    });
                }
                if (data2 != "" && data2 != "0")
                {
                    selected = data2;
                }
                if (returndata != null)
                {
                    s = new SelectList(returndata, "Id", "FulDetails", selected);
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult BA_TargetTDropdownlist(string data, string data2, string ApprCalendarIds, string EmpId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                int ids = EmpId != null ? Convert.ToInt32(EmpId) : 0;
                int calendarids=Convert.ToInt32(ApprCalendarIds);
                Calendar BA_Calendar = db.Calendar.Where(e => e.Id == calendarids).SingleOrDefault();
                DateTime? StartDate = BA_Calendar.FromDate;
                DateTime? EndDate = BA_Calendar.ToDate;

                var EmpAppraisal = db.EmployeeAppraisal
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.BA_EmpTarget)
                    .Include(e => e.BA_EmpTarget.Select(r => r.BA_Category))
                    .Include(e => e.BA_EmpTarget.Select(r => r.BA_SubCategory))
                    .Include(e => e.BA_TargetT)
                    .Include(e => e.BA_TargetT.Select(r => r.BA_WorkFlow))
                    .Where(e => e.Employee.Id == ids).SingleOrDefault();

                var BA_EmpTargetids = EmpAppraisal.BA_EmpTarget.Where(e => e.StartDate == StartDate && e.EndDate == EndDate).ToList().Select(r => r.Id);
                List<int> WFStatuslist = new List<int> { 2, 4, 6, 8 };
                var qurey = EmpAppraisal.BA_TargetT.ToList().Where(r => r.TrClosed == true && !WFStatuslist.Contains(r.BA_WorkFlow.ToList().OrderByDescending(t => t.Id).FirstOrDefault().WFStatus) && BA_EmpTargetids.Contains(r.BA_EmpTarget_Id));
                List<returnDataClass> returndata = new List<returnDataClass>();
                foreach (var item in qurey)
                {
                    returndata.Add(new returnDataClass
                    {
                        Id = item.Id,
                        BA_EmpTarget_Fulldetails = db.BA_EmpTarget.Where(e => e.Id == item.BA_EmpTarget_Id).FirstOrDefault().BA_SubCategory.Code,
                        AccountNo = item.AccountNo,
                        CustomerId = item.CustomerId,
                        TargetAmount = db.BA_EmpTarget.Where(e => e.Id == item.BA_EmpTarget_Id).FirstOrDefault().TargetAmount.ToString(),
                        TargetAmountCredit = item.TargetAmountCredit.ToString(),
                        TargetAmountBalance = item.TargetAmountBalance.ToString()

                    });
                }

                List<returndatagridDataClass> returndatases = new List<returndatagridDataClass>();
                foreach (var item1 in returndata)
                {
                    returndatases.Add(new returndatagridDataClass
                    {
                        Id = item1.Id,
                        FulDetails = item1.BA_EmpTarget_Fulldetails + " ," + "AccountNo:" + item1.AccountNo + ", " + "CustomerId:" + item1.CustomerId + ", " + "TargetAmount:" + item1.TargetAmount + " ," + "TargetAmountCredit:" + item1.TargetAmountCredit + " ," + "TargetAmountBalance:" + item1.TargetAmountBalance

                    });
                }

                if (data2 != "" && data2 != "0")
                {
                    selected = data2;
                }
                if (returndatases != null)
                {
                    s = new SelectList(returndatases, "Id", "FulDetails", selected);
                }
                return Json(s, JsonRequestBehavior.AllowGet);
               
            }
        }

        public class returndatagridclass
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }           
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {   
                    var BA_TargetT = db.BA_TargetT.Where(e => e.TrClosed == true && e.InputMethod==0 && e.BA_WorkFlow.ToList().OrderByDescending(t => t.Id).FirstOrDefault().WFStatus==6).ToList().Select(r => r.BA_EmpTarget_Id);
                    var BA_EmpTarget = db.BA_EmpTarget
                        .Include(e => e.EmployeeAppraisal)
                        .Include(e => e.EmployeeAppraisal.Employee)
                        .Include(e => e.EmployeeAppraisal.Employee.EmpName)
                        .Where(e => BA_TargetT.Contains(e.Id) && e.EmployeeAppraisal != null).ToList();
                    var all = BA_EmpTarget.GroupBy(e => e.EmployeeAppraisal_Id).Select(z => z.First()).ToList();

                    IEnumerable<BA_EmpTarget> fall;

                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.EmployeeAppraisal.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<BA_EmpTarget, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.EmployeeAppraisal.Employee.EmpCode :

                                                                "");
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
                                Id = item.EmployeeAppraisal.Id.ToString(),
                                Code = item.EmployeeAppraisal.Employee.EmpCode,
                                Name = item.EmployeeAppraisal.Employee.EmpName != null ? item.EmployeeAppraisal.Employee.EmpName.FullNameFML : "",
                                
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

                                     select new[] { null, Convert.ToString(c.Id), c.EmployeeAppraisal.Employee.EmpCode, };
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
                    throw e;
                }
            }
        }
        public class EmpTargetCancelReqChildDataClass
        {
            public int Id { get; set; }
            public string EntryDate { get; set; }           
            public string Reason { get; set; }
        }
        public ActionResult Get_EmpTargetCancelReq(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeAppraisal
                        .Include(e=>e.BA_EmpTarget)
                        .Include(e=>e.BA_TargetT)
                        .Include(e=>e.BA_TargetT.Select(r=>r.BA_WorkFlow))
                        .Where(e => e.Id == data).SingleOrDefault();
                    var BA_TargetTDs = db_data.BA_TargetT.ToList().Where(r=>r.InputMethod==0 && r.TrClosed==true && r.BA_WorkFlow.ToList().OrderByDescending(t=>t.Id).FirstOrDefault().WFStatus==6);
                    if (BA_TargetTDs != null)
                    {
                        List<EmpTargetCancelReqChildDataClass> returndata = new List<EmpTargetCancelReqChildDataClass>();
                        foreach (var item in BA_TargetTDs)
                        {

                            returndata.Add(new EmpTargetCancelReqChildDataClass
                                {
                                    Id = item.Id,
                                    EntryDate =db.BA_EmpTarget.Where(e=>e.Id==item.BA_EmpTarget_Id).FirstOrDefault().EndDate.Value.ToShortDateString(),
                                    Reason = item.BA_WorkFlow.ToList().OrderByDescending(t => t.Id).FirstOrDefault().Comments
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
                    return null;
            }
        }
        public ActionResult SanctionOrApproveCancel(BA_TargetT c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        string BA_TargetTDatalistids = form["BA_TargetTDatalist"] == "0" ? null : form["BA_TargetTDatalist"];
                        string Reason = form["Reason"] == "0" ? null : form["Reason"];
                        int ids = Convert.ToInt32(BA_TargetTDatalistids);
                        var x = db.BA_TargetT.Where(e => e.Id == ids).FirstOrDefault();       
                        List<AppWFDetails> oAppWFDetails_List = new List<AppWFDetails>();
                        //int WfStatusNew = x.BA_WorkFlow.ToList().OrderByDescending(e => e.Id).FirstOrDefault().WFStatus;
                        AppWFDetails AppWFDetailss = new AppWFDetails
                        {
                            WFStatus = 6,
                            Comments = Reason,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                        };
                        oAppWFDetails_List.Add(AppWFDetailss);
                        x.BA_WorkFlow = oAppWFDetails_List;
                        x.InputMethod = 0;
                        x.TargetAmountBalance = db.BA_EmpTarget.Find(x.BA_EmpTarget_Id).TargetAmount;
                        x.TargetAccountsBalance = db.BA_EmpTarget.Find(x.BA_EmpTarget_Id).TargetAccounts;
                        x.TargetComplianceBalance = db.BA_EmpTarget.Find(x.BA_EmpTarget_Id).TargetCompliance;
                        db.BA_TargetT.Attach(x);
                        db.Entry(x).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add("  Record Is Cancel");
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


       

    }
}




