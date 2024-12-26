using CMS_SPS;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Transactions;
using P2BUltimate.Models;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Text;
using P2b.Global;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.CMS
{
    public class CompetencyCloseActionController : Controller
    {

        public ActionResult Index()
        {
            return View("~/Views/CMS/MainViews/CompetencyCloseAction/index.cshtml");
        }

        public class returndatagridDataclass //Parentgrid
        {
            public string Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
        }

        public class returnGridChildClass
        {
            public int Id { get; set; }
            public string IsTrainingComplete { get; set; }
            public string TrainingCompleteDate { get; set; }
            public string IsTransferComplete { get; set; }
            public string TransferCompleteDate { get; set; }
            public string IsOfficiatingComplete { get; set; }
            public string OfficiatingCompleteDate { get; set; }
            public string IsPromotionComplete { get; set; }
            public string PromotionCompleteDate { get; set; }

        }

        public ActionResult A_CompetencyModel_Grid(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var db_data = db.CompetencyEmployeeDataT
                 .Include(e => e.CompetencyPostAction)
                    .Where(e => e.Id == data).FirstOrDefault();

                List<returnGridChildClass> returndatases = new List<returnGridChildClass>();
                if (db_data != null)
                {
                    returndatases.Add(new returnGridChildClass
                        {
                            Id = db_data.Id,
                            IsOfficiatingComplete  = db_data.CompetencyPostAction.IsOfficiatingComplete.ToString(),
                            IsPromotionComplete = db_data.CompetencyPostAction.IsPromotionComplete.ToString() ,
                            IsTrainingComplete = db_data.CompetencyPostAction.IsTrainingComplete.ToString(),
                            IsTransferComplete = db_data.CompetencyPostAction.IsTransferComplete.ToString(),
                            OfficiatingCompleteDate =db_data.CompetencyPostAction.TrainingCloseDate.ToString(),
                            PromotionCompleteDate = db_data.CompetencyPostAction.PromotionCloseDate.ToString(),
                            TrainingCompleteDate = db_data.CompetencyPostAction.TrainingCloseDate.ToString(),
                            TransferCompleteDate = db_data.CompetencyPostAction.TransferCloseDate.ToString()
                        });
                }
                return Json(returndatases, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPostActionInfo(string EmpId,string BatchName, string ProcessDate) //Pass leavehead id here
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Emp = int.Parse(EmpId);
                int BatchId = int.Parse(BatchName);
                DateTime ProcDate = Convert.ToDateTime(ProcessDate);
               

                var OCompEmpData = db.CompetencyEmployeeDataT.Include(e => e.Employee).Include(e => e.BatchName).Include(e => e.CompetencyPostAction)
                                       .Where(e => e.Employee.Id == Emp && e.BatchName.Id == BatchId).FirstOrDefault();
                if (OCompEmpData != null && OCompEmpData.ProcessDate.Value.Date == ProcDate)
                {

                    return Json(OCompEmpData.CompetencyPostAction  , JsonRequestBehavior.AllowGet);
                }

                return null;  

            }
        }

        public class EmpDataTClass
        {
            public string EmpId { get; set; }
            public string BatchName { get; set; }
            public string ProcessDate { get; set; }
            public string IsTraining { get; set; }
            public string IsPromotion { get; set; }
            public string IsTransfer { get; set; }
            public string IsOfficiating { get; set; }
            public string TrainingCloseDate { get; set; }
            public string PromotionCloseDate { get; set; }
            public string TransferCloseDate { get; set; }
            public string OfficiatingCloseDate { get; set; }
        }
        
        [HttpPost]
        public ActionResult Create(List<EmpDataTClass> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                foreach (var item in data)
                {
                   
                    int EmpId = int.Parse(item.EmpId);
                    int BatchId = int.Parse(item.BatchName);
                    DateTime ProcDate = DateTime.Parse(item.ProcessDate);
                    using (TransactionScope ts = new TransactionScope())
                    {
                        try
                        {
                            var OCompEmpData = db.CompetencyEmployeeDataT.Include(e => e.Employee).Include(e => e.BatchName).Include(e => e.CompetencyPostAction)
                                     .Where(e => e.Employee.Id == EmpId && e.BatchName.Id == BatchId).FirstOrDefault();
                            if (OCompEmpData != null && OCompEmpData.ProcessDate.Value.Date == ProcDate)
                            {
                                CompetencyPostAction OCompPostAction = OCompEmpData.CompetencyPostAction;
                                OCompPostAction.DBTrack = new DBTrack
                                {
                                    CreatedBy = OCompEmpData.DBTrack.CreatedBy == null ? null : OCompEmpData.DBTrack.CreatedBy,
                                    CreatedOn = OCompEmpData.DBTrack.CreatedOn == null ? null : OCompEmpData.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };

                                if (OCompPostAction != null)
                                {
                                    OCompPostAction.IsOfficiatingComplete = Convert.ToBoolean(item.IsOfficiating);
                                    OCompPostAction.IsPromotionComplete = Convert.ToBoolean(item.IsPromotion);
                                    OCompPostAction.IsTrainingComplete = Convert.ToBoolean(item.IsTraining);
                                    OCompPostAction.IsTransferComplete = Convert.ToBoolean(item.IsTransfer);
                                    if (OCompPostAction.IsOfficiatingComplete == true)
                                        OCompPostAction.OfficiatingCloseDate = Convert.ToDateTime(item.OfficiatingCloseDate);
                                    if (OCompPostAction.IsPromotionComplete == true)
                                        OCompPostAction.PromotionCloseDate = Convert.ToDateTime(item.PromotionCloseDate);
                                    if (OCompPostAction.IsTrainingComplete == true)
                                        OCompPostAction.TrainingCloseDate = Convert.ToDateTime(item.TrainingCloseDate);
                                    if (OCompPostAction.IsTransferComplete == true)
                                        OCompPostAction.TransferCloseDate = Convert.ToDateTime(item.TransferCloseDate);
                                }
                                if (OCompPostAction.IsOfficiatingComplete == true || OCompPostAction.IsPromotionComplete == true || OCompPostAction.IsTrainingComplete == true || OCompPostAction.IsTransferComplete == true)
                                {
                                    OCompEmpData.CloseDate = DateTime.Now;
                                    db.CompetencyEmployeeDataT.Attach(OCompEmpData);
                                    db.Entry(OCompEmpData).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(OCompEmpData).State = System.Data.Entity.EntityState.Detached;
                                }


                                db.CompetencyPostAction.Attach(OCompPostAction);
                                db.Entry(OCompPostAction).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(OCompPostAction).State = System.Data.Entity.EntityState.Detached;
                               
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
                            //    List<string> Msg = new List<string>();
                            Msg.Add(ex.Message);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        ts.Complete();
                    }

                } 
                Msg.Add("Data Saved Successfully.");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Get_Employelist(string BatchName, string ProcessDate, string ProcessBatch)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                int BatchId = Convert.ToInt32(BatchName);
                DateTime ProcDate = Convert.ToDateTime(ProcessDate);

                var OCompBatch = db.CompetencyBatchProcessT.Include(e => e.CompetencyEmployeeDataT).Include(e => e.CompetencyEmployeeDataT.Select(t => t.CompetencyPostAction))
                    .Include(e => e.CompetencyEmployeeDataT.Select(t => t.Employee))
                   .Include(e => e.CompetencyEmployeeDataT.Select(t => t.Employee.EmpName))
                   .Where(e => e.BatchName_Id == BatchId && e.ProcessBatch == ProcessBatch)
                   .FirstOrDefault();


                var muidsa = OCompBatch.CompetencyEmployeeDataT.Where(e => e.ProcessDate.Value.Date == ProcDate.Date && e.CompetencyPostAction != null
                    && (e.CompetencyPostAction.IsOfficiatingComplete == false && e.CompetencyPostAction.IsTransferComplete == false &&
                    e.CompetencyPostAction.IsPromotionComplete == false && e.CompetencyPostAction.IsTrainingComplete == false)).ToList();

                if (muidsa != null && muidsa.Count() > 0)
                {
                    foreach (var item in muidsa)
                    {

                        returndata.Add(new Utility.returndataclass
                        {
                            code = item.Employee.Id.ToString(),
                            value = item.Employee.FullDetails,
                        });
                    }

                }

                if (returndata.Count() > 0)
                {
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }


                return null;
            }
        }

        public ActionResult CompetencyEmployeeDataT_Grid(ParamModel param, string selectedtextcmn)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.CompetencyEmployeeDataT.Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName).Where(e => e.CompetencyPostAction != null && e.BatchName.BatchName == selectedtextcmn)
                        .ToList();



                    IEnumerable<CompetencyEmployeeDataT> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {

                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.BatchName.BatchName.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.BatchName.BatchDescription.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<CompetencyEmployeeDataT, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.BatchName.BatchName : "");
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
                        List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridDataclass
                            {
                                Id = item.Id.ToString(),
                                EmpCode = item.Employee.EmpCode,
                                EmpName = item.Employee.EmpName.FullNameFML,
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

                                     select new[] { null, Convert.ToString(c.Id), c.BatchName.BatchName, };
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

