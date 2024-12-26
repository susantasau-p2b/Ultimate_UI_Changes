using Attendance;
using EssPortal.App_Start;
using EssPortal.Models;
using Leave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2b.Global;
using EssPortal.Security;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace EssPortal.Controllers
{
    public class FutureODController : Controller
    {
        //
        // GET: /FutureOD/
        public ActionResult Index()
        {
            return View("~/Views/FutureOD/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/_FutureOdDetails.cshtml");
        }
        public ActionResult view_partial()
        {
            return View("~/Views/Shared/_FutureOdDetailsView.cshtml");
        }
        public ActionResult view_partial1()
        {
            return View("~/Views/Shared/_FutureOdDetailsViewForAuthority.cshtml");
        }

        public class returnDataClass
        {
            public Int32 EmpId { get; set; }
            public String val { get; set; }
            public List<string> vals { get; set; }
            public Int32 EmpLVid { get; set; }
            public bool Id3 { get; set; }
            public Int32 LvHead_Id { get; set; }
        }

        public class GetFutureODClass1
        {
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string ReqDate { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public ChildGetLvNewReqClass RowData { get; set; }
        }

        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }
        }

        public class GetFutureODClass
        {
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string ReqDate { get; set; }
            public string Reason { get; set; }
            public string Status { get; set; }
            public string ID { get; set; }
            public ChildGetLvNewReqClass RowData { get; set; }
        }



        public ActionResult GetMyFutureODReq()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);


                var db_data = db.EmployeeAttendance
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.ReportingStructRights)
                    .Include(e => e.Employee.ReportingStructRights.Select(b => b.AccessRights))
                    .Include(e => e.Employee.ReportingStructRights.Select(b => b.AccessRights.ActionName))
                    .Include(e => e.ProcessedData)
                    .Include(e => e.FutureOD.Select(b => b.WFDetails))
                    .Include(e => e.ProcessedData.Select(b => b.MusterRemarks))
                    .Where(e => e.Employee.Id == Id).SingleOrDefault();


                if (db_data != null)
                {
                    List<GetFutureODClass> returndata = new List<GetFutureODClass>();
                    returndata.Add(new GetFutureODClass
                    {
                        ReqDate = "Req Date",
                        FromDate = "From Date",
                        ToDate = "To Date",
                        Reason = "Reason",
                        Status = "Status"
                    });

                    List<string> remark = new List<string>();
                    List<RemarkConfig> RC = db.RemarkConfig.Include(e => e.MusterRemarks).Where(e => e.IsODAppl == true).ToList();
                    foreach (var item in RC)
                    {
                        remark.Add(item.MusterRemarks.LookupVal.ToUpper());
                    }

                    foreach (var a in db_data.FutureOD.OrderByDescending(e => e.FromDate))
                    {
                        var futureodreq = db.FutureOD.OrderByDescending(e => e.Id).Select(e => e.WFDetails).FirstOrDefault();

                        var status = "-";
                        if (futureodreq != null)
                        {
                            var stat = futureodreq.Select(e => e.WFStatus).LastOrDefault();
                            status = Utility.GetStatusName().Where(e => e.Key == stat.ToString()).Select(e => e.Value).SingleOrDefault();

                        }
                        var session = Session["auho"].ToString().ToUpper();

                        var EmpR = db_data.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session)
                            .Select(e => e.AccessRights.IsClose).FirstOrDefault();

                        returndata.Add(new GetFutureODClass
                        {
                            RowData = new ChildGetLvNewReqClass
                            {
                                LvNewReq = a.Id.ToString(),
                                EmpLVid = db_data.Id.ToString(),
                                IsClose = EmpR.ToString(),
                                LvHead_Id = "",
                            },
                            ReqDate = a.ReqDate != null ? a.ReqDate.Value.ToShortDateString() : "--",
                            FromDate = a.FromDate != null ? a.FromDate.Value.ToShortDateString() : "--",
                            ToDate = a.ToDate != null ? a.ToDate.Value.ToShortDateString() : "--",
                            Reason = a.Reason != null ? a.Reason.ToString() : "--",
                            Status = status.ToString(),

                            //Utility.GetStatusName().Where(a => e.LvWFDetails.Count > 0 && a.Key == e.LvWFDetails.LastOrDefault().WFStatus.ToString()).Select(a => a.Value).SingleOrDefault()
                        });
                    }


                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        public ActionResult GetMyFutureOdDetails1()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);


                var db_data = db.EmployeeAttendance
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.ReportingStructRights)
                    .Include(e => e.Employee.ReportingStructRights.Select(b => b.AccessRights))
                    .Include(e => e.Employee.ReportingStructRights.Select(b => b.AccessRights.ActionName))
                    .Include(e => e.ProcessedData)
                    .Include(e => e.FutureOD.Select(b => b.WFDetails))
                    .Include(e => e.ProcessedData.Select(b => b.MusterRemarks))
                    .Where(e => e.Employee.Id == Id).SingleOrDefault();


                if (db_data != null)
                {
                    List<GetFutureODClass> returndata = new List<GetFutureODClass>();
                    returndata.Add(new GetFutureODClass
                    {
                        ReqDate = "Req Date",
                        FromDate = "From Date",
                        ToDate = "To Date",
                        Reason = "Reason",
                        Status = "Status"

                    });
                    // var remark = new string[] { "UA", "I?", "O?", "**", "IL", "OE" };

                    List<string> remark = new List<string>();
                    List<RemarkConfig> RC = db.RemarkConfig.Include(e => e.MusterRemarks).Where(e => e.IsODAppl == true).ToList();
                    foreach (var item in RC)
                    {
                        remark.Add(item.MusterRemarks.LookupVal.ToUpper());
                    }

                    foreach (var a in db_data.FutureOD.OrderByDescending(e => e.FromDate))
                    {
                        var futureodreq = a.WFDetails.OrderByDescending(e => e.Id).FirstOrDefault();

                        var status = "-";
                        if (futureodreq != null)
                        {
                            var stat = futureodreq.WFStatus;
                            status = Utility.GetStatusName().Where(e => e.Key == stat.ToString()).Select(e => e.Value).SingleOrDefault();

                        }
                        var session = Session["auho"].ToString().ToUpper();

                        var EmpR = db_data.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session)
                            .Select(e => e.AccessRights.IsClose).FirstOrDefault();

                        returndata.Add(new GetFutureODClass
                        {
                            RowData = new ChildGetLvNewReqClass
                            {
                                LvNewReq = a.Id.ToString(),
                                EmpLVid = db_data.Id.ToString(),
                                IsClose = EmpR.ToString(),
                                LvHead_Id = "",
                            },
                            ReqDate = a.ReqDate != null ? a.ReqDate.Value.ToShortDateString() : "--",
                            FromDate = a.FromDate != null ? a.FromDate.Value.ToShortDateString() : "--",
                            ToDate = a.ToDate != null ? a.ToDate.Value.ToShortDateString() : "--",
                            Reason = a.Reason != null ? a.Reason.ToString() : "--",
                            Status = status.ToString(),

                            //Utility.GetStatusName().Where(a => e.LvWFDetails.Count > 0 && a.Key == e.LvWFDetails.LastOrDefault().WFStatus.ToString()).Select(a => a.Value).SingleOrDefault()
                        });
                    }


                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        public ActionResult UpdateStatus(FutureOD FutureOdReq, FormCollection form, String data)
        {
            var ids = Utility.StringIdsToListString(data);
            var processdataid = Convert.ToInt32(ids[0]);
            var empAttid = Convert.ToInt32(ids[1]);

            string authority = form["authority"] == null ? null : form["authority"];
            var isClose = form["isClose"] == null ? null : form["isClose"];
            string Sanction = form["Sanction"] == null ? "false" : form["Sanction"];
            string Approval = form["Approval"] == null ? "false" : form["Approval"];
            string HR = form["HR"] == null ? null : form["HR"];

            string ReasonSanction = form["ReasonSanction"] == null ? null : form["ReasonSanction"];
            string ReasonApproval = form["ReasonApproval"] == null ? null : form["ReasonApproval"];
            string ReasonHr = form["ReasonHr"] == null ? null : form["ReasonHr"];
            if (authority == null && isClose == null)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
            bool SanctionRejected = false;
            bool HrRejected = false;

            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.FutureOD
                    .Include(e => e.ProcessedData)
                    .Include(e => e.WFDetails)
                    .Include(e => e.WFStatus)
                    .Include(e => e.EmployeeAttendance)
                    .Include(e => e.EmployeeAttendance.Employee)
                    .Include(e => e.EmployeeAttendance.Employee.GeoStruct)
                    .Include(e => e.EmployeeAttendance.Employee.PayStruct)
                    .Include(e => e.EmployeeAttendance.Employee.FuncStruct)
                    .Where(e => e.Id == processdataid).OrderByDescending(e => e.Id).FirstOrDefault();




                if (authority.ToUpper() == "MYSELF")
                {
                    AttWFDetails oAttWFDetails = new AttWFDetails
                    {
                        WFStatus = 6,
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };

                    List<AttWFDetails> oAttWFDetails_List = new List<AttWFDetails>();
                    query.TrClosed = true;
                    query.isCancel = true;
                    query.WFDetails.Add(oAttWFDetails);
                    query.WFStatus = db.Lookup.Where(e => e.Code == "1000").Include(e => e.LookupValues).FirstOrDefault().LookupValues.Where(e => e.LookupVal == "6").SingleOrDefault();
                    //query.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                }
                if (authority.ToUpper() == "SANCTION")
                {
                    if (Convert.ToBoolean(Sanction) == true)
                    {
                        AttWFDetails oAttWFDetails = new AttWFDetails
                        {
                            WFStatus = 1,
                            Comments = ReasonSanction,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                        };

                        List<AttWFDetails> oAttWFDetails_List = new List<AttWFDetails>();

                        query.WFDetails.Add(oAttWFDetails);
                        query.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                        //query.WFStatus = db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault();
                        query.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").SingleOrDefault();

                    }
                    else if (Convert.ToBoolean(Sanction) == false)
                    {
                        AttWFDetails oAttWFDetails = new AttWFDetails
                        {
                            WFStatus = 2,
                            Comments = ReasonSanction,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                        };

                        List<AttWFDetails> oAttWFDetails_List = new List<AttWFDetails>();
                        query.WFDetails.Add(oAttWFDetails);
                        query.TrClosed = true;
                        SanctionRejected = true;
                        //query.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                        query.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "2").SingleOrDefault();
                    }
                }
                if (authority.ToUpper() == "APPROVAL")
                {
                    if (Convert.ToBoolean(Approval) == true)
                    {
                        AttWFDetails oAttWFDetails = new AttWFDetails
                        {
                            WFStatus = 3,
                            Comments = ReasonApproval,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                        };

                        List<AttWFDetails> oAttWFDetails_List = new List<AttWFDetails>();
                        query.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                        query.WFDetails.Add(oAttWFDetails);
                        //query.WFStatus = db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();
                        query.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").SingleOrDefault();




                    }
                    else if (Convert.ToBoolean(Approval) == false)
                    {
                        AttWFDetails oAttWFDetails = new AttWFDetails
                        {
                            WFStatus = 4,
                            Comments = ReasonApproval,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                        };

                        List<AttWFDetails> oAttWFDetails_List = new List<AttWFDetails>();
                        query.WFDetails.Add(oAttWFDetails);
                        query.TrClosed = true;
                        HrRejected = true;

                        //query.WFStatus = db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault();
                        query.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "4").SingleOrDefault();
                    }
                }
                else
                {

                }
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //if someone reject lv
                        if (SanctionRejected == true || HrRejected == true)
                        {
                            //var OodEmployeeDetails = db.EmployeeAttendance
                            //    .Where(e => e.Id == empAttid)
                            //     .Select(e => new { EmpId = e.Id, OdReq = e.OutDoorDutyReq })
                            //      .SingleOrDefault();

                            //var PrevReq = OodEmployeeDetails.OdReq
                            //    .Where(e => e.WFDetails != null && e.ReqDate == query.ReqDate
                            //    //&& e.IsCancel == false
                            //        ).OrderByDescending(e => e.Id).FirstOrDefault();

                            //ProcessedData oLvNewReq = new ProcessedData()
                            //{
                            //    AttendProcessDate = query.ReqDate,
                            //    //MInTime = "",
                            //    //MOutTime = 

                            //};
                            //try
                            //{
                            //    db.ProcessedData.Add(oLvNewReq);
                            //    db.SaveChanges();
                            //    var emplv = db.EmployeeAttendance
                            //        .Include(e => e.OutDoorDutyReq)
                            //        //.Include(e => e.LvNewReq.Select(a => a.GeoStruct))
                            //        //.Include(e => e.LvNewReq.Select(a => a.PayStruct))
                            //        //.Include(e => e.LvNewReq.Select(a => a.FuncStruct))
                            //        .Where(e => e.Employee.Id == OodEmployeeDetails.EmpId)
                            //        .SingleOrDefault();
                            //    emplv.ProcessedData.Add(oLvNewReq);
                            //    db.Entry(emplv).State = System.Data.Entity.EntityState.Modified;
                            //    db.SaveChanges();
                            //    db.Entry(emplv).State = System.Data.Entity.EntityState.Detached;
                            //}
                            //catch (Exception e)
                            //{
                            //    return Json(new Utility.JsonClass { status = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                            //}
                            query.TrReject = true;
                            query.isCancel = true;
                        }
                        List<ProcessedData> OProcDatalist = new List<ProcessedData>();
                        List<ProcessedData> OProcDatalistexistrec = new List<ProcessedData>();
                        if (Convert.ToBoolean(isClose) == true)
                        {
                            DateTime? start = query.FromDate;
                            DateTime? end = query.ToDate;

                            RemarkConfig get_remark = db.RemarkConfig
                                                          .Include(e => e.AlterMusterRemark)
                                                          .Include(e => e.PresentStatus)
                                                          .Include(e => e.MusterRemarks)
                                                          .Where(e => e.MusterRemarks.LookupVal.ToUpper() == "M").FirstOrDefault();

                            DateTime? startc = query.FromDate;
                            DateTime? endc = query.ToDate;
                            for (DateTime? counter = startc; counter <= endc; counter = counter.Value.AddDays(1))
                            {
                                //var onlyStartDate = start.Value.ToShortDateString();
                                var existPunch = db.RawData.Where(e => DbFunctions.TruncateTime(e.SwipeDate) == startc && e.EmployeeAttendance_Id == query.EmployeeAttendance.Id).FirstOrDefault();
                                if (existPunch != null)
                                {

                                    return Json(new Utility.JsonClass { status = true, responseText = "Please kindly cancel future OD or inform to employee cancel applied future OD because he/she present on this " + existPunch.SwipeDate.Value.ToShortDateString() + " Date...!" }, JsonRequestBehavior.AllowGet);
                                }
                                startc = startc.Value.AddDays(1);

                            }


                            for (DateTime? counter = start; counter <= end; counter = counter.Value.AddDays(1))
                            {
                                var existrec = db.ProcessedData.Where(e => e.SwipeDate == start && e.EmployeeAttendance_Id == query.EmployeeAttendance.Id).SingleOrDefault();
                                if (existrec == null)
                                {
                                    ProcessedData OProcData = new ProcessedData()
                                    {
                                        AttendProcessDate = query.ReqDate,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                        SwipeDate = start,
                                        PresentStatus = get_remark.PresentStatus,
                                        FuncStruct = query.EmployeeAttendance.Employee.FuncStruct,
                                        GeoStruct = query.EmployeeAttendance.Employee.GeoStruct,
                                        ManualReason = query.Reason,
                                        MusterRemarks = get_remark.AlterMusterRemark,
                                        Narration = "Future OD",
                                        PayStruct = query.EmployeeAttendance.Employee.PayStruct,

                                    };
                                    db.ProcessedData.Add(OProcData);
                                    db.SaveChanges();
                                    OProcDatalist.Add(OProcData);
                                    var emplv = db.EmployeeAttendance
                                        .Include(e => e.FutureOD)
                                        .Include(e => e.ProcessedData)
                                        .Where(e => e.Id == query.EmployeeAttendance.Id)
                                        .SingleOrDefault();
                                    if (emplv.ProcessedData != null)
                                    {
                                        emplv.ProcessedData.Add(OProcData);
                                    }
                                    //else
                                    //{
                                    //    emplv.ProcessedData = new ProcessedData { OProcData };
                                    //}

                                    db.Entry(emplv).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    //OProcDatalistexistrec.Add(existrec);
                                    //query.ProcessedData = OProcDatalistexistrec;
                                    if (existrec.InTime == null && existrec.OutTime == null)
                                    {
                                        existrec.PresentStatus = get_remark.PresentStatus;
                                        existrec.MusterRemarks = get_remark.AlterMusterRemark;
                                        existrec.ManualReason = query.Reason;
                                        existrec.Narration = "Future OD";
                                        OProcDatalist.Add(existrec);
                                        query.ProcessedData = OProcDatalist;
                                    }

                                }
                                start = start.Value.AddDays(1);
                                //db.Entry(emplv).State = System.Data.Entity.EntityState.Detached;
                            }
                        }

                        query.ProcessedData = OProcDatalist;
                        db.FutureOD.Attach(query);
                        db.Entry(query).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(query).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                        //return Json(new { status = true, responseText = "Record Updated..!" });
                    }
                }
                catch (Exception e)
                {
                    return Json(new Utility.JsonClass { status = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        public ActionResult AddOrEdit(FutureOD lkval, FormCollection form, String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Add = form["Add"] != null && form["Add"] != "" ? Convert.ToBoolean(form["Add"]) : true;
                var Id = form["auth_id"] != null && form["auth_id"] != "" ? Convert.ToInt32(form["auth_id"]) : 0;
                string authority = form["authority"] == null ? null : form["authority"];
                var caclreq = form["CancelReq"] == null ? null : form["CancelReq"];


                // bool caclreq = form["CancelReq"] == "true" ? true : false;



                if (authority.ToUpper() == "MYSELF")
                {
                    if (Convert.ToBoolean(caclreq) == true)
                    {
                        var returnobj = UpdateStatus(lkval, form, data);
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                        //return Json(returnobj, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }





        public class GetODClass
        {

            public string ReqDate { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string Remark { get; set; }
            public string Status { get; set; }
            public string ID { get; set; }
            public ChildGetLvNewReqClass RowData { get; set; }
        }

        public ActionResult Edit(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);

                var returndata = (Object)null;
                List<GetODClass> odclassdata = new List<GetODClass>();




                var FutureOD = db.FutureOD.Where(e => e.Id == id).FirstOrDefault();
                var stat = db.FutureOD.Where(e => (e.Id == id) && (e.isCancel != true)).Select(e => e.WFDetails).SingleOrDefault();
                int statss;

                if (stat != null)
                {
                    statss = stat.Select(a1 => a1.WFStatus).LastOrDefault();
                }
                else
                {
                    statss = 99;
                }

                odclassdata.Add(new GetODClass
                {

                    FromDate = FutureOD.FromDate != null ? FutureOD.FromDate.Value.ToShortDateString() : null,
                    ToDate = FutureOD.FromDate != null ? FutureOD.FromDate.Value.ToShortDateString() : null,
                    ReqDate = FutureOD.ReqDate != null ? FutureOD.ReqDate.Value.ToShortDateString() : null,
                    ID = id.ToString(),
                    Remark = FutureOD.Reason.ToString(),
                    Status = statss.ToString()
                });
                returndata = new
                {
                    Add = true,
                };

                return Json(new Object[] { odclassdata, returndata, "", JsonRequestBehavior.AllowGet });
            }
        }

        public class EmpLvClass
        {
            public string EmpName { get; set; }
            public List<ReqLvHeadWise> LvHeadName { get; set; }
        }
        public class ReqLvHeadWise
        {
            public string LvHeadName { get; set; }
            public string LvHeadCode { get; set; }
            public string LvHeadBal { get; set; }
            public Array LvReq { get; set; }
        }

        public class tempClass
        {
            public string LvName { get; set; }
            public string LvCode { get; set; }
            public string LvBal { get; set; }
            public string FullDetails { get; set; }
        }

        public ActionResult GetEmpFutureODHistory()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
                }
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                //var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                //if (EmpIds == null && EmpIds.Count == 0)
                //{
                //    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                //}
                List<EmployeeAttendance> Emps = new List<EmployeeAttendance>();
                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                foreach (var item in EmpidsWithfunsub)
                {


                    var Empid = db.EmployeeAttendance
                        .Include(e => e.Employee)
                         .Include(e => e.Employee.EmpName)
                        .Include(e => e.FutureOD.Select(a => a.WFStatus))
                        .Include(e => e.FutureOD.Select(a => a.WFDetails));
                    //.Where(e => EmpIds.Contains(e.Employee.Id)&& e.ITInvestmentPayment !=null).ToList();

                    Emps = Empid.Where(e => item.ReportingEmployee.Contains(e.Employee.Id)).AsNoTracking().ToList();
                }

                var allLvHead = db.ProgramList.ToList();
                List<EmpLvClass> ListEmpLvClass = new List<EmpLvClass>();

                foreach (var ca in Emps)
                {
                    var item1 = ca.FutureOD.ToList();
                    var oEmpLvClass = new EmpLvClass();
                    //foreach (var item2 in item1)
                    //{
                    //if (item2.ITInvestment != null)
                    //{
                    //foreach (var lvhead in allLvHead)
                    //{
                    //    var temp = new List<tempClass>();
                    // var LvData = item1.Where(e => e.ProgramList.Id == lvhead.Id).OrderByDescending(e => e.RequisitionDate).ToList();
                    var temp = new List<tempClass>();
                    foreach (var item in item1)
                    {
                        var Status = "--";
                        if (item.WFDetails.Count() > 0)
                        {
                            Status = Utility.GetStatusName().Where(e => e.Key == item.WFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus.ToString())
                            .Select(e => e.Value).FirstOrDefault();
                        }

                        //if (item.InputMethod == 0)
                        //{
                        //    Status = "Approved By HRM (M)";
                        //}
                        var ReqDate = item.ReqDate != null ? item.ReqDate.Value.ToString("dd/MM/yyyy") : null;
                        var FromDate = item.FromDate != null ? item.FromDate.Value.ToString("dd/MM/yyyy") : null;
                        var ToDate = item.ToDate != null ? item.ToDate.Value.ToString("dd/MM/yyyy") : null;
                        var Reason = item.Reason != null ? item.Reason : null;
                        temp.Add(new tempClass
                        {
                            FullDetails =
                                   "ReqDate :" + ReqDate +
                                   " FromDate :" + FromDate +
                                   " ToDate :" + ToDate +
                                   "Reason:" + Reason +
                                  " Status :" + Status
                        });
                    }

                    if (temp != null && temp.Count > 0)
                    {
                        oEmpLvClass.EmpName = ca.Employee.EmpCode + " " + ca.Employee.EmpName.FullNameFML;
                        if (oEmpLvClass.LvHeadName == null)
                        {
                            oEmpLvClass.LvHeadName = new List<ReqLvHeadWise>{new ReqLvHeadWise {
                                           LvReq =temp.Select(e=>e.FullDetails).ToArray(),
                                        }};
                        }
                        else
                        {
                            oEmpLvClass.LvHeadName.Add(new ReqLvHeadWise
                            {
                                LvReq = temp.Select(e => e.FullDetails).ToArray(),
                            });
                        }

                    }
                    //}
                    if (oEmpLvClass.EmpName != null)
                    {
                        ListEmpLvClass.Add(oEmpLvClass);
                    }
                    //  }
                    // }
                }
                if (ListEmpLvClass.Count == 0)
                {
                    return Json(new Utility.JsonClass { status = false }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new Utility.JsonClass { status = true, Data = ListEmpLvClass }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetEmpReqDataFutureODDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var maintableid = Convert.ToInt32(ids[1]);
                var status = ids.Count > 0 ? ids[2] : null;


                var empfind = db.EmployeeAttendance.Where(e => e.Id == maintableid)
                    .Include(e => e.Employee)
                    .Select(e => new
                    {
                        EmpName = e.Employee.EmpCode + " " + e.Employee.EmpName.FullNameFML
                    }).SingleOrDefault();

                var OdData = db.FutureOD.Where(e => e.Id == id).OrderByDescending(e => e.Id)
                    .Select(e => new
                    {
                        ReqDate = e.ReqDate,
                        FromDate = e.FromDate,
                        ToDate = e.ToDate,
                        Reason = e.Reason
                    }).FirstOrDefault();


                var stat = db.FutureOD.Where(e => e.Id == id).OrderByDescending(e => e.Id).Select(e => e.WFDetails).FirstOrDefault();
                if (stat != null)
                {
                    int statss = stat.Select(a1 => a1.WFStatus).LastOrDefault();

                    var oVar = db.DT_PassportDetails.Where(e => e.Id == id).SingleOrDefault();
                    var list = new
                    {
                        EmployeeName = empfind.EmpName,
                        ReqDate = OdData.ReqDate.Value.ToShortDateString(),
                        FromDate = OdData.FromDate.Value.ToShortDateString(),
                        ToDate = OdData.ToDate.Value.ToShortDateString(),
                        Status = Utility.GetStatusName().Where(e => e.Key == statss.ToString()).Select(e => e.Value).SingleOrDefault(),
                        isClose = status.ToString(),
                        Reason = OdData.Reason
                    };
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }



        public ActionResult GetNewEmpSancFutureOdDetailsReq(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                              .Include(e => e.LookupValues)
                              .Where(e => e.Code == "601").AsNoTracking().FirstOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
                    //FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }
                // var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                if (EmpidsWithfunsub == null && EmpidsWithfunsub.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var returnDataClass = new List<returnDataClass>();
                int count = 0;
                List<GetFutureODClass1> returndata = new List<GetFutureODClass1>();
                returndata.Add(new GetFutureODClass1
                {
                    EmpCode = "EmpCode",
                    EmpName = "EmpName",
                    ReqDate = "Req Date",
                    FromDate = "From Date",
                    ToDate = "To Date",
                    RowData = new ChildGetLvNewReqClass
                    {
                        LvNewReq = "0",
                        EmpLVid = "0",
                        IsClose = "0",
                        LvHead_Id = "",
                    },
                });

                foreach (var item1 in EmpidsWithfunsub)
                {


                    var Emps = db.EmployeeAttendance
                        .Where(e => (item1.ReportingEmployee.Contains(e.Employee.Id)))
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.ReportingStructRights)
                        .Include(e => e.Employee.ReportingStructRights.Select(b => b.AccessRights.ActionName))
                        .Include(e => e.Employee.ReportingStructRights.Select(b => b.FuncModules))
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.FutureOD)
                        .Include(e => e.FutureOD.Select(b => b.ProcessedData))
                        .Include(e => e.ProcessedData)
                        .Include(e => e.FutureOD.Select(b => b.WFDetails)).ToList();






                    foreach (var item in Emps)
                    {
                        if (item.FutureOD != null && item.FutureOD.Count() > 0)
                        {
                            var LvIds = UserManager.FutureodFilter(item.FutureOD.OrderByDescending(e => e.ReqDate).ToList(),
                               Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                            if (LvIds.Count() > 0)
                            {
                                var outdoorreqdata = item.FutureOD.Where(e => LvIds.Contains(e.Id)).ToList();
                                foreach (var singleODDetails in outdoorreqdata)
                                {

                                    var session = Session["auho"].ToString().ToUpper();
                                    var EmpR = item.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session && e.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                    .Select(e => e.AccessRights.IsClose).FirstOrDefault();
                                    //if (stts.WFStatus == 0)
                                    //{
                                    returndata.Add(new GetFutureODClass1
                                    {
                                        RowData = new ChildGetLvNewReqClass
                                        {
                                            LvNewReq = singleODDetails.Id.ToString(),
                                            EmpLVid = item.Id.ToString(),
                                            IsClose = EmpR.ToString(),
                                            LvHead_Id = "",
                                        },
                                        EmpCode = item.Employee.EmpCode.ToString(),
                                        EmpName = item.Employee.EmpName.FullNameFML.ToString(),
                                        ReqDate = singleODDetails.ReqDate != null ? singleODDetails.ReqDate.Value.ToShortDateString() : "--",
                                        FromDate = singleODDetails.FromDate != null ? singleODDetails.FromDate.Value.ToShortDateString() : "--",
                                        ToDate = singleODDetails.ToDate != null ? singleODDetails.ToDate.Value.ToShortDateString() : "--",

                                    });
                                    //count += 1;
                                }
                            }
                        }
                    }
                }
                //if (count ==0)
                //{
                //    returndata = null; 
                //}
                if (returndata != null && returndata.Count > 0)
                {

                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = returndata }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetNewEmpApproveOdDetailsReq(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null && EmpIds.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                var Emps = db.EmployeeAttendance
                    .Where(e => (EmpIds.Contains(e.Employee.Id)))
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.ReportingStructRights)
                    .Include(e => e.Employee.ReportingStructRights.Select(b => b.AccessRights))
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.OutDoorDutyReq)
                    .Include(e => e.OutDoorDutyReq.Select(b => b.ProcessedData))
                    .Include(e => e.ProcessedData)
                    .Include(e => e.OutDoorDutyReq.Select(b => b.WFDetails)).ToList();


                var returnDataClass = new List<returnDataClass>();

                List<GetFutureODClass1> returndata = new List<GetFutureODClass1>();
                returndata.Add(new GetFutureODClass1
                {
                    EmpCode = "EmpCode",
                    EmpName = "EmpName",
                    ReqDate = "Req Date",
                    FromDate = "From Date",
                    ToDate = "To Date",
                    RowData = new ChildGetLvNewReqClass
                    {
                        LvNewReq = "0",
                        EmpLVid = "0",
                        IsClose = "0",
                        LvHead_Id = "",
                    },
                });


                foreach (var item in Emps)
                {
                    if (item.FutureOD != null)
                    {
                        foreach (var singleODDetails in item.FutureOD)
                        {

                            int QId = singleODDetails.Id;

                            var stts = singleODDetails.WFDetails.LastOrDefault();
                            var session = Session["auho"].ToString().ToUpper();
                            var EmpR = item.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session)
                       .Select(e => e.AccessRights.IsClose).FirstOrDefault();

                            if (stts.WFStatus == 1)
                            {
                                returndata.Add(new GetFutureODClass1
                                {
                                    RowData = new ChildGetLvNewReqClass
                                    {
                                        LvNewReq = singleODDetails.Id.ToString(),
                                        EmpLVid = item.Id.ToString(),
                                        IsClose = EmpR.ToString(),
                                        LvHead_Id = "",
                                    },
                                    EmpCode = item.Employee.EmpCode.ToString(),
                                    EmpName = item.Employee.EmpName.FullNameFML.ToString(),
                                    ReqDate = singleODDetails.ReqDate.Value.ToShortDateString(),
                                    FromDate = singleODDetails.FromDate.Value.ToShortDateString(),
                                    ToDate = singleODDetails.ToDate.Value.ToShortDateString(),

                                });
                            }
                        }
                    }
                }

                if (returndata != null && returndata.Count > 0)
                {

                    return Json(new { status = true, data = returndata, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = returndata, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        public ActionResult Create(FutureOD FutureOD, FormCollection form)
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                if (FutureOD.FromDate < DateTime.Now || FutureOD.ToDate < DateTime.Now)
                {
                    return Json(new { status = false, responseText = "Future OD can be applied for Future Dates only." }, JsonRequestBehavior.AllowGet);
                }

                if (FutureOD.FromDate > FutureOD.ToDate)
                {
                    return Json(new { status = false, responseText = "To Date should be greater than From Date." }, JsonRequestBehavior.AllowGet);
                }

                DateTime mFromPeriod = Convert.ToDateTime(FutureOD.FromDate);
                DateTime mEndDate = Convert.ToDateTime(FutureOD.ToDate);
                var empid1 = Convert.ToInt32(SessionManager.EmpId);
                var emp = db.Employee.Where(e => e.Id == empid1).FirstOrDefault();
                for (DateTime mTempDate = mFromPeriod; mTempDate <= mEndDate; mTempDate = mTempDate.AddDays(1))
                {

                    int FutureODrec = db.FutureOD.Where(e => e.EmployeeAttendance.Employee.Id == empid1 && e.FromDate.Value <= mTempDate.Date && e.ToDate.Value >= mTempDate.Date
                                               && (e.isCancel == false && e.TrReject == false)).Select(e => e.Id).SingleOrDefault();
                    if (FutureODrec > 0)
                    {
                        Msg.Add("You have already enter " + mTempDate.Date);
                        return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    // Leave Apply then od can not apply start

                    EmployeeLeave oEmployeeLeave = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.LvNewReq)
                    .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                    .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                   .Where(e => e.Employee.Id == empid1).OrderBy(e => e.Id).SingleOrDefault();

                    if (oEmployeeLeave != null)
                    {
                        // var LvCalendarFilter = oEmployeeLeave.LvNewReq.Where(e => e.LeaveCalendar.Id == OLeaveCalendar.Id).OrderBy(e => e.Id).ToList();
                        var LvCalendarFilter = oEmployeeLeave.LvNewReq.OrderBy(e => e.Id).ToList();

                        var LvOrignal_id = LvCalendarFilter.Where(e => e.LvOrignal != null).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();
                        var AntCancel = LvCalendarFilter.Where(e => e.IsCancel == false && e.TrReject == false).OrderBy(e => e.Id).ToList();
                        var listLvs = AntCancel.Where(e => !LvOrignal_id.Contains(e.Id) && e.WFStatus.LookupVal != "2" && e.WFStatus.LookupVal != "3").OrderBy(e => e.Id).ToList();

                        if (listLvs.Where(e => e.FromDate <= mTempDate && e.ToDate >= mTempDate).Count() != 0)
                        {
                            //already exits
                            Msg.Add("You have already apply leave on " + mTempDate.ToShortDateString() + " So you can not apply Future OD.");
                            return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }
                    // Leave Apply then od can not apply end

                }

                AttWFDetails oAttWFDetails = new AttWFDetails
                {
                    WFStatus = 0,
                    Comments = FutureOD.Reason.ToString(),
                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                };

                List<AttWFDetails> oAttWFDetails_List = new List<AttWFDetails>();
                oAttWFDetails_List.Add(oAttWFDetails);
                FutureOD.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };



                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                               new System.TimeSpan(0, 30, 0)))
                    {
                        try
                        {

                            FutureOD OFutureOD = new FutureOD()
                            {
                                FromDate = FutureOD.FromDate,
                                Reason = FutureOD.Reason,
                                ReqDate = FutureOD.ReqDate,
                                InputMethod = 0,
                                ToDate = FutureOD.ToDate,
                                DBTrack = FutureOD.DBTrack,
                                TrClosed = false,
                                // WFStatus = db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(),
                                WFDetails = oAttWFDetails_List
                            };

                            db.FutureOD.Add(OFutureOD);
                            db.SaveChanges();
                            var empid = Convert.ToInt32(SessionManager.EmpId);
                            var EmpAttendance = db.EmployeeAttendance.Include(e => e.FutureOD)
                                .Include(e => e.ProcessedData).Where(e => e.Employee.Id == empid).SingleOrDefault();

                            if (EmpAttendance != null && EmpAttendance.FutureOD != null)
                            {
                                if (EmpAttendance.FutureOD != null)
                                {
                                    EmpAttendance.FutureOD.Add(OFutureOD);
                                }
                                else
                                {
                                    EmpAttendance.FutureOD = new List<FutureOD> { OFutureOD };
                                }
                            }
                            else
                            {
                                var oEmpAttendace = new EmployeeAttendance();
                                oEmpAttendace.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                oEmpAttendace.FutureOD = new List<FutureOD> { OFutureOD };
                            }
                            db.Entry(EmpAttendance).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();


                            ts.Complete();
                            return Json(new { status = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

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
                            return Json(new { status = false, responseText = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
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
                    var errorMsg = sb.ToString();
                    return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                    //return this.Json(new { msg = errorMsg });
                }
            }
        }

    }
}