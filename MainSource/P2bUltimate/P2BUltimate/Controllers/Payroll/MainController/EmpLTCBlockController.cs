﻿using P2b.Global;
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
    public class EmpLTCBlockController : Controller
    {
        //
        // GET: /EmpLTCBlock/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/EmpLTCBlock/Index.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_EmpLTCBlockTGridPartial.cshtml");
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

        public JsonResult GetGlobalLTCBlockDetails(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int empid = int.Parse(data);
                int Blockid = 0;
                var LtcStruct = db.EmployeeLTCStruct
                                           .Include(e => e.EmployeeLTCStructDetails)
                                            .Include(e => e.EmployeeLTCStructDetails.Select(t => t.PolicyName))
                                            .Include(e => e.EmployeeLTCStructDetails.Select(t => t.LTCFormula))
                                             .Include(e => e.EmployeeLTCStructDetails.Select(t => t.LTCFormula.GlobalLTCBlock))
                                           .Include(e => e.EmployeeLTCStructDetails.Select(t => t.LTCPolicyAssignment))
                                           .Include(e => e.EmployeeLTCStructDetails.Select(t => t.LTCPolicyAssignment.PayScaleAgreement))
                                           .Where(e => e.EmployeePayroll.Employee.Id == empid && e.EndDate == null).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();
                if (LtcStruct != null)
                {
                    var OEmpLtcStructDet = LtcStruct.EmployeeLTCStructDetails.Where(e => e.PolicyName.LookupVal.ToUpper() == "LTCBLOCK").Select(r => r.LTCFormula).FirstOrDefault();
                    if (OEmpLtcStructDet != null)
                    {
                        var GLTC = OEmpLtcStructDet.GlobalLTCBlock.FirstOrDefault();

                        if (GLTC != null)
                        {
                             Blockid = GLTC.Id;
                        }
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "Block Structure not define" }, JsonRequestBehavior.AllowGet);

                    }
                }

                var query = db.GlobalLTCBlock.Where(e => e.Id == Blockid).ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetGlobalLTCBlock(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = Convert.ToInt32(data);
                var query = db.GlobalLTCBlock.Find(Id);

                return Json(query, JsonRequestBehavior.AllowGet);
            }
        }

        

        public class EMPLTCBlockChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string BlockStart { get; set; }
            public string BlockEnd { get; set; }
            public int Occurances { get; set; }
            public bool BlockStatus { get; set; }
        }

        public class returnDataClass
        {
            public string BlockStart { get; set; }
            public string BlockEnd { get; set; }
            public int Occurances { get; set; }
        }

        public ActionResult Get_EmpLTCBlock(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.EmpLTCBlock)
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data.EmpLTCBlock != null)
                    {
                        List<EMPLTCBlockChildDataClass> returndata = new List<EMPLTCBlockChildDataClass>();
                        var Blocklist = db_data.EmpLTCBlock;
                        foreach (var item in db_data.EmpLTCBlock.OrderByDescending(e => e.Id))
                        {
                            returndata.Add(new EMPLTCBlockChildDataClass
                            {
                                Id = item.Id,
                                BlockStart = item.BlockStart.Value != null ? item.BlockStart.Value.ToShortDateString() : null,
                                BlockEnd = item.BlockEnd.Value != null ? item.BlockEnd.Value.ToShortDateString() : null,
                                Occurances = item.Occurances,
                                BlockStatus = item.IsBlockClose == true ? true : false

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

        public ActionResult GridEditData(int data)
        {
            var returnlist = new List<returnDataClass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != 0)
                {
                    var retrundataList = db.EmpLTCBlock.Where(e => e.Id == data).FirstOrDefault();

                    returnlist.Add(new returnDataClass()
                    {
                        BlockStart = retrundataList.BlockStart.Value != null ? retrundataList.BlockStart.Value.ToShortDateString() : null,
                        BlockEnd = retrundataList.BlockEnd.Value != null ? retrundataList.BlockEnd.Value.ToShortDateString() : null,
                        Occurances = retrundataList.Occurances

                    });

                    return Json(new { returndata = returnlist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }

        public ActionResult GridEditSave(LTCAdvanceClaim LTCAdvanceClaim, FormCollection form, string data)
        {
            List<string> Msgs = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                var LTCBlockData = db.EmpLTCBlock.Include(e => e.EmpLTCBlockT).Include(e => e.EmpLTCBlockT.Select(r => r.LTCAdvanceClaim))
                    .Include(e => e.EmpLTCBlockT.Select(r => r.LTCSettlementClaim))
                    .Where(e => e.Id == Id).SingleOrDefault();
                List<EmpLTCBlockT> OBJ = new List<EmpLTCBlockT>();

                if (LTCBlockData.EmpLTCBlockT.Count() > 0)
                {
                    foreach (var item in LTCBlockData.EmpLTCBlockT.ToList())
                    {
                        if (item.LTCAdvanceClaim != null || item.LTCSettlementClaim != null)
                        {
                            return this.Json(new { status = false, responseText = "You can't edit record as claim has been processed", JsonRequestBehavior.AllowGet });
                        }
                        else
                        {
                            db.EmpLTCBlockT.Attach(item);
                            db.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();

                        }


                    }

                }


                DateTime BlockStart = Convert.ToDateTime(form["BlockFrom"]);
                DateTime BlockEnd = Convert.ToDateTime(form["BlockEnd"]);

                int Occurances = Convert.ToInt32(form["Occurances"]);


                LTCBlockData.BlockStart = BlockStart;
                LTCBlockData.BlockEnd = BlockEnd;
                LTCBlockData.Occurances = Occurances;

                using (TransactionScope ts = new TransactionScope())
                {


                    LTCBlockData.DBTrack = new DBTrack
                    {
                        CreatedBy = LTCBlockData.DBTrack.CreatedBy == null ? null : LTCBlockData.DBTrack.CreatedBy,
                        CreatedOn = LTCBlockData.DBTrack.CreatedOn == null ? null : LTCBlockData.DBTrack.CreatedOn,
                        Action = "M",
                        ModifiedBy = SessionManager.UserName,
                        ModifiedOn = DateTime.Now
                    };

                    if (LTCBlockData.Occurances == 1)
                    {
                        EmpLTCBlockT OEmpLTCBlockT = new EmpLTCBlockT()
                        {
                            BlockPeriodStart = LTCBlockData.BlockStart,
                            BlockPeriodEnd = LTCBlockData.BlockEnd,
                            Occurances = 1,
                            DBTrack = LTCBlockData.DBTrack
                        };
                        OBJ.Add(OEmpLTCBlockT);
                    }

                    if (LTCBlockData.Occurances == 2)
                    {
                        EmpLTCBlockT OEmpLTCBlockT = new EmpLTCBlockT()
                        {
                            BlockPeriodStart = LTCBlockData.BlockStart,
                            BlockPeriodEnd = LTCBlockData.BlockEnd.Value.Date.AddYears(-2),
                            Occurances = 1,
                            DBTrack = LTCBlockData.DBTrack
                        };

                        EmpLTCBlockT OEmpLTCBlockT1 = new EmpLTCBlockT()
                        {
                            BlockPeriodStart = LTCBlockData.BlockStart.Value.Date.AddYears(2),
                            BlockPeriodEnd = LTCBlockData.BlockEnd,
                            Occurances = 1,
                            DBTrack = LTCBlockData.DBTrack
                        };
                        OBJ.Add(OEmpLTCBlockT);
                        OBJ.Add(OEmpLTCBlockT1);
                    }


                    try
                    {
                        LTCBlockData.EmpLTCBlockT = OBJ;
                        db.EmpLTCBlock.Attach(LTCBlockData);
                        db.Entry(LTCBlockData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(LTCBlockData).State = System.Data.Entity.EntityState.Detached;

                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data Updated Successfully.", JsonRequestBehavior.AllowGet });
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = LTCBlockData.Id });
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

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        [HttpPost]
        public ActionResult Create(EmpLTCBlock EmpLTCBlock, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string LtcBlock = form["GlobalBlockList"] == "0" ? "" : form["GlobalBlockList"];
                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];

                    if (LtcBlock == null && LtcBlock == "")
                    {
                        Msg.Add("  Please select LTC Block.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    int Blockid = int.Parse(LtcBlock);

                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    }

                    if (db.EmpLTCBlock.Count() == 0)
                    {
                        Msg.Add("  You have to process all employees block first.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    foreach (var Emp1 in ids)
                    {
                        EmpLTCBlockT OEmpLTCBlockT = db.EmpLTCBlockT.Find(Blockid);

                        var a = db.EmployeePayroll.Include(e => e.EmpLTCBlock).Include(e => e.Employee)
                                 .Include(e => e.EmpLTCBlock.Select(y => y.EmpLTCBlockT))
                                 .Where(e => e.Employee.Id == Emp1).AsNoTracking().SingleOrDefault().EmpLTCBlock.FirstOrDefault();
                        var b = db.EmployeePayroll.Include(e => e.EmpLTCBlock).Include(e => e.Employee)
                             .Include(e => e.EmpLTCBlock.Select(y => y.EmpLTCBlockT))
                             .Where(e => e.Employee.Id == Emp1).AsNoTracking().SingleOrDefault().Employee;
                        if (a != null)
                        {
                            var test = a.EmpLTCBlockT.Where(e => e.BlockPeriodStart >= EmpLTCBlock.BlockStart
                               && e.BlockPeriodEnd <= EmpLTCBlock.BlockEnd).FirstOrDefault();
                            if (test != null)
                            {
                                Msg.Add(b.FullDetails + ", block allready created ,Please don't select this employee,");

                            }
                        }
                    }
                    if (Msg.Count() > 0)
                    {
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    GlobalLTCBlock OGlobalLTCBlockT = db.GlobalLTCBlock.Find(Blockid);
                    EmpLTCBlock.GlobalLTCBlock = OGlobalLTCBlockT;
                    EmpLTCBlock.BlockStart = OGlobalLTCBlockT.BlockStart;
                    EmpLTCBlock.BlockEnd = OGlobalLTCBlockT.BlockEnd;

                    DateTime? firstDate = EmpLTCBlock.BlockStart;
                    DateTime? secondDate = EmpLTCBlock.BlockEnd;
                    List<DateTime?> dates = new List<DateTime?>();
                    List<DateTime?> datenew = new List<DateTime?>();
                    int totalYears = secondDate.Value.Year - firstDate.Value.Year;
                    int block = 0;

                    block = (totalYears / OGlobalLTCBlockT.NoOfTimes);




                    // using (TransactionScope ts = new TransactionScope())
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                      new System.TimeSpan(0, 30, 0)))
                    {
                        foreach (var i in ids)
                        {
                            if (ModelState.IsValid)
                            {

                                EmpLTCBlock.EmpLTCBlockT = null;
                                List<EmpLTCBlockT> OBJ = new List<EmpLTCBlockT>();
                                EmpLTCBlock.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                for (int j = 1; j <= OGlobalLTCBlockT.NoOfTimes; j++)
                                {
                                    if (OGlobalLTCBlockT.NoOfTimes == 1)
                                    {
                                        dates.Add(firstDate);
                                        datenew.Add(firstDate);
                                        dates.Add(secondDate);
                                        datenew.Add(secondDate);
                                    }

                                    if (j == 1 && OGlobalLTCBlockT.NoOfTimes > 1)
                                    {
                                        dates.Add(firstDate);
                                        datenew.Add(firstDate);
                                        dates.Add(firstDate.Value.Date.AddYears(OGlobalLTCBlockT.NoOfTimes).AddDays(-1));
                                        datenew.Add(firstDate.Value.Date.AddYears(OGlobalLTCBlockT.NoOfTimes).AddDays(-1));
                                    }
                                    if (j != 1 && OGlobalLTCBlockT.NoOfTimes > 1)
                                    {
                                        dates.Add(dates.LastOrDefault().Value.AddDays(1));
                                        datenew.Add(dates.LastOrDefault().Value);
                                        dates.Add(dates.LastOrDefault().Value.AddYears(OGlobalLTCBlockT.NoOfTimes).AddDays(-1));
                                        datenew.Add(dates.LastOrDefault().Value);
                                    }
                                    EmpLTCBlockT OEmpLTCBlockT = new EmpLTCBlockT()
                                    {
                                        BlockPeriodStart = datenew[0],
                                        BlockPeriodEnd = datenew[1],
                                        Occurances = EmpLTCBlock.Occurances,
                                        DBTrack = EmpLTCBlock.DBTrack
                                    };
                                    OBJ.Add(OEmpLTCBlockT);
                                    datenew.Clear();
                                };


                                EmpLTCBlock OEmpLTCBlock = new EmpLTCBlock()
                                {
                                    BlockEnd = EmpLTCBlock.BlockEnd,
                                    BlockStart = EmpLTCBlock.BlockStart,
                                    GlobalLTCBlock = EmpLTCBlock.GlobalLTCBlock,
                                    Occurances = EmpLTCBlock.Occurances,
                                    DBTrack = EmpLTCBlock.DBTrack,
                                    EmpLTCBlockT = OBJ
                                };

                                db.EmpLTCBlock.Add(OEmpLTCBlock);
                                db.SaveChanges();




                                //OEmpLTCBlockT.LTCAdvanceClaim = LTCAdvanceClaim;
                                List<EmpLTCBlock> EmpLTCBlocklist = new List<EmpLTCBlock>();
                                var aa = db.EmployeePayroll.Include(e => e.EmpLTCBlock).Where(e => e.Employee.Id == i).SingleOrDefault();
                                if (aa.EmpLTCBlock.Count() > 0)
                                {
                                    EmpLTCBlocklist.AddRange(aa.EmpLTCBlock);
                                }
                                else
                                {
                                    EmpLTCBlocklist.Add(OEmpLTCBlock);
                                }
                                aa.EmpLTCBlock = EmpLTCBlocklist;
                                //OEmployeePayroll.DBTrack = dbt;
                                db.EmployeePayroll.Attach(aa);
                                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
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
                        ts.Complete();
                        // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                        List<string> Msgs = new List<string>();
                        Msgs.Add("Data Saved successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                    }


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

        [HttpPost]
        public async Task<ActionResult> Delete(int data, int data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var LTCBlockData = db.EmpLTCBlock.Include(e => e.EmpLTCBlockT).Include(e => e.EmpLTCBlockT.Select(r => r.LTCAdvanceClaim))
                    .Include(e => e.EmpLTCBlockT.Select(r => r.LTCSettlementClaim))
                    .Where(e => e.Id == data).SingleOrDefault();
                List<EmpLTCBlockT> OBJ = new List<EmpLTCBlockT>();

                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        if (LTCBlockData.EmpLTCBlockT.Count() > 0)
                        {
                            foreach (var item in LTCBlockData.EmpLTCBlockT.ToList())
                            {
                                if (item.LTCAdvanceClaim != null || item.LTCSettlementClaim != null)
                                {
                                    return this.Json(new { status = false, responseText = "You can't delete record as claim has been processed", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    db.EmpLTCBlockT.Attach(item);
                                    db.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                                    db.SaveChanges();

                                }


                            }

                            db.EmpLTCBlock.Attach(LTCBlockData);
                            db.Entry(LTCBlockData).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();

                        }
                        ts.Complete();

                    }


                    //await db.SaveChangesAsync();



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