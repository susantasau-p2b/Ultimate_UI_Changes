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
    public class CompetencyPostActionController : Controller
    {
        // GET: CompetencyPostAction
        public ActionResult Index()
        {
            return View("~/Views/CMS/MainViews/CompetencyPostAction/index.cshtml");
        }

        public class returndatagridDataclass //Parentgrid
        {
            public string Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
        }

        public ActionResult GetTrainingCategory(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var fall = db.Category.ToList();
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult Create(CompetencyPostAction c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string BatchName = form["BatchNameList"] == "0" ? "" : form["BatchNameList"];
                string ProcessDate = form["ProcessDateList"] == "0" ? "" : form["ProcessDateList"];

                int ProcDateId = Convert.ToInt32(ProcessDate);
                int BatchId = Convert.ToInt32(BatchName);
                List<int> ids = null;
                DateTime? ProcDate = db.CompetencyEmployeeDataT.Find(ProcDateId).ProcessDate;
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    ids = Utility.StringIdsToListIds(Emp);
                }
                else
                {
                    Msg.Add(" Kindly select employee  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                }
                try
                {
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            try
                            {
                                CompetencyPostAction CompetencyPostAction = new CompetencyPostAction()
                             {
                                 Name = c.Name,
                                 IsTransferRecomment = c.IsTransferRecomment,
                                 IsTrainingRecommend = c.IsTrainingRecommend,
                                 IsPromotionRecomment = c.IsPromotionRecomment,
                                 IsOfficiatingRecomment = c.IsOfficiatingRecomment,
                                 IsOfficiatingComplete = c.IsOfficiatingComplete,
                                 IsPromotionComplete = c.IsPromotionComplete,
                                 IsTrainingComplete = c.IsTrainingComplete,
                                 IsTransferComplete = c.IsTransferComplete,
                                 TransferCloseDate = c.TransferCloseDate,
                                 TrainingCloseDate = c.TrainingCloseDate,
                                 OfficiatingCloseDate = c.OfficiatingCloseDate,
                                 PromotionCloseDate = c.PromotionCloseDate,
                                 DBTrack = c.DBTrack
                             };
                                db.CompetencyPostAction.Add(CompetencyPostAction);
                                db.SaveChanges();

                             
                                foreach (var item in ids)
                                {
                                    var OCompEmpData = db.CompetencyEmployeeDataT.Include(e => e.Employee).Include(e => e.BatchName)
                                        .Where(e => e.Employee.Id == item && e.BatchName.Id == BatchId).FirstOrDefault();
                                    if (OCompEmpData != null && OCompEmpData.ProcessDate.Value.Date == ProcDate.Value.Date)
                                    {
                                        OCompEmpData.CompetencyPostAction = CompetencyPostAction;
                                        db.CompetencyEmployeeDataT.Attach(OCompEmpData);
                                        db.Entry(OCompEmpData).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(OCompEmpData).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                                string disc = form["TrainingLookuplist"] == "0" ? "" : form["TrainingLookuplist"];
                                if (disc != null || disc!= "0")
                                {
                                    int trainingprogramid = Convert.ToInt32(disc);
                                    PostActionTraining cmaorg = null;

                                    cmaorg = new PostActionTraining()
                                    {
                                        TrainingProgramId = trainingprogramid,
                                    };

                                    List<PostActionTraining> PostActionTraininglist = new List<PostActionTraining>();
                                    db.PostActionTraining.Add(cmaorg);
                                    PostActionTraininglist.Add(cmaorg);
                                    CompetencyPostAction.PostActionTraining = PostActionTraininglist;
                                    db.CompetencyPostAction.Attach(CompetencyPostAction);
                                    db.Entry(CompetencyPostAction).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(CompetencyPostAction).State = System.Data.Entity.EntityState.Detached;
                                }
                                
                                ts.Complete();
                                Msg.Add("Data Saved successfully");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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


        public ActionResult Get_Employelist(string BatchName, string ProcessDate, string ProcessBatch)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                 

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                int BatchId = Convert.ToInt32(BatchName);
                DateTime ProcDate = Convert.ToDateTime(ProcessDate);
                var OCompBatch = db.CompetencyBatchProcessT.Include(e => e.CompetencyEmployeeDataT).Include(e => e.CompetencyEmployeeDataT.Select(t => t.Employee))
                    .Include(e => e.CompetencyEmployeeDataT.Select(t => t.Employee.EmpName))
                    .Where(e => e.BatchName_Id == BatchId && e.ProcessBatch == ProcessBatch)
                    .FirstOrDefault();

                
                var muidsa = OCompBatch.CompetencyEmployeeDataT.Where(e => e.ProcessDate.Value.Date == ProcDate.Date).ToList();

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

        public class returnGridChildClass
        {
            public int Id { get; set; }
            public string IsTrainingRecommend { get; set; }
            public string IsTransferRecomment { get; set; }
            public string IsOfficiatingRecomment { get; set; }
            public string IsPromotionRecomment { get; set; }
            

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
                        IsOfficiatingRecomment = db_data.CompetencyPostAction.IsOfficiatingRecomment.ToString(),
                        IsPromotionRecomment = db_data.CompetencyPostAction.IsPromotionRecomment.ToString(),
                        IsTrainingRecommend = db_data.CompetencyPostAction.IsTrainingRecommend.ToString(),
                        IsTransferRecomment = db_data.CompetencyPostAction.IsTransferRecomment.ToString(),
                       
                    });
                }
                return Json(returndatases, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetProcessBatch(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = Convert.ToInt32(data);
                var qurey = db.CompetencyBatchProcessT.Include(e => e.BatchName).Where(e => e.BatchName.Id == Id).ToList();

                var Grp = qurey.GroupBy(item => item.BatchName)
                  .Select(group => new
                  {
                      Id = group.First().Id,
                      ProcessBatch = group.First().ProcessBatch
                  })
                  .ToList();

                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(Grp, "Id", "ProcessBatch", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetProcessDate(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = Convert.ToInt32(data);
                var qurey = db.CompetencyEmployeeDataT.Include(e => e.BatchName).Where(e => e.BatchName.Id == Id).ToList();

                var Grp = qurey.GroupBy(item => item.BatchName)
                  .Select(group => new
                    {
                        Id = group.First().Id,
                        ProcDate = group.First().ProcessDate.Value.Date.ToString("dd/MM/yyyy")
                    })
                  .ToList();

                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(Grp, "Id", "ProcDate", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
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