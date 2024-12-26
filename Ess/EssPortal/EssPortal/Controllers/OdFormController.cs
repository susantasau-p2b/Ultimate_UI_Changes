using P2b.Global;
using Attendance;
using EssPortal.App_Start;
using EssPortal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Text;
using EssPortal.Security;
using Leave;


namespace EssPortal.Controllers
{
    public class OdFormController : Controller
    {
        //
        // GET: /OdForm/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult view_partial()
        {
            return View("~/Views/Shared/_OdDetailsView.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/_OdDetails.cshtml");
        }
        public ActionResult view_partial1()
        {
            return View("~/Views/Shared/_OdDetailsViewForAuthority.cshtml");
        }
        public ActionResult Emp_Punch_History_Partial()
        {
            return View("~/Views/Shared/_EmpPunchDetails.cshtml");
        }
        public ActionResult Partial_OdReqHistoryGrid()
        {
            return View("~/Views/Shared/_OdReqHistory.cshtml");
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
        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }
            public string OdReqId { get; set; }

        }

        public class GetODClass
        {
            public string SwipeDate { get; set; }
            public string ReqDate { get; set; }
            public string CInTime { get; set; }
            public string COutTime { get; set; }
            public string InTime { get; set; }
            public string OutTime { get; set; }
            public string Remark { get; set; }
            public string Status { get; set; }
            public string ID { get; set; }
            public string MINswLocation { get; set; }
            public string MAXswLocation { get; set; }
            public string IsODTimEtryAppl { get; set; }
            public ChildGetLvNewReqClass RowData { get; set; }
        }
        public class GetPunchDet
        {
            public string SwipeDate { get; set; }
            public string SwipeTime { get; set; }
            public string InputType { get; set; }
            public string Location { get; set; }

        }
        public class GetODClass1
        {
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string SwipeDate { get; set; }
            public string CInTime { get; set; }
            public string COutTime { get; set; }
            public string Remark { get; set; }
            public ChildGetLvNewReqClass RowData { get; set; }
        }


        public ActionResult GetMyOdDetails1()
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
                    .Include(e => e.OutDoorDutyReq.Select(b => b.WFDetails))
                    .Include(e => e.ProcessedData.Select(b => b.MusterRemarks))
                    .Where(e => e.Employee.Id == Id).SingleOrDefault();


                if (db_data != null)
                {
                    List<GetODClass> returndata = new List<GetODClass>();
                    returndata.Add(new GetODClass
                    {
                        SwipeDate = "Swipe Date",
                        InTime = "InTime",
                        OutTime = "OutTime",
                        Remark = "Remark",
                        Status = "Status"
                    });
                    // var remark = new string[] { "UA", "I?", "O?", "**", "IL", "OE" };

                    List<string> remark = new List<string>();
                    List<RemarkConfig> RC = db.RemarkConfig.Include(e => e.MusterRemarks).Where(e => e.IsODAppl == true).ToList();
                    foreach (var item in RC)
                    {
                        remark.Add(item.MusterRemarks.LookupVal.ToUpper());
                    }

                    foreach (var a in db_data.ProcessedData.Where(e => remark.Contains(e.MusterRemarks.LookupVal.ToString())).OrderByDescending(e => e.SwipeDate))
                    {
                        var outdutyreq = db.OutDoorDutyReq.Where(e => e.ProcessedData.Id == a.Id).OrderByDescending(e => e.Id).Select(e => e.WFDetails).FirstOrDefault();

                        var status = "-";
                        if (outdutyreq != null)
                        {
                            var stat = outdutyreq.Select(e => e.WFStatus).LastOrDefault();
                            status = Utility.GetStatusName().Where(e => e.Key == stat.ToString()).Select(e => e.Value).SingleOrDefault();

                        }
                        var session = Session["auho"].ToString().ToUpper();

                        var EmpR = db_data.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session)
                            .Select(e => e.AccessRights.IsClose).FirstOrDefault();

                        returndata.Add(new GetODClass
                        {
                            RowData = new ChildGetLvNewReqClass
                            {
                                LvNewReq = a.Id.ToString(),
                                EmpLVid = db_data.Id.ToString(),
                                IsClose = EmpR.ToString(),
                                LvHead_Id = "",
                                //OdReqId = 
                            },
                            SwipeDate = a.SwipeDate != null ? a.SwipeDate.Value.ToShortDateString() : "--",
                            InTime = a.InTime != null ? a.InTime.Value.ToShortTimeString() : "--",
                            OutTime = a.OutTime != null ? a.OutTime.Value.ToShortTimeString() : "--",
                            Remark = a.MusterRemarks != null ? a.MusterRemarks.LookupVal.ToString() : "--",
                            Status = status.ToString()
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



        public ActionResult UpdateStatus(OutDoorDutyReq OdReq, FormCollection form, String data)
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
                var query = db.OutDoorDutyReq
                    .Include(e => e.ProcessedData)
                    .Include(e => e.WFDetails)
                    .Include(e => e.WFStatus)
                    .Where(e => e.ProcessedData.Id == processdataid).OrderByDescending(e => e.Id).FirstOrDefault();


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
                    query.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "6").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();//db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();
                    query.ProcessedData = null;
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
                        query.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault();
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
                        query.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "2").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault(); //db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
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



                        var pdata = db.ProcessedData
                            .Where(e => e.Id == processdataid).SingleOrDefault();
                        pdata.InTime = query.InTime;
                        pdata.OutTime = query.OutTime;
                        pdata.DBTrack = query.DBTrack;

                        db.ProcessedData.Attach(pdata);
                        db.Entry(pdata).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        query.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();//db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();
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

                        query.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "4").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();//db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault();
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
                            // query.ProcessedData = null;
                        }
                        db.OutDoorDutyReq.Attach(query);
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



        public ActionResult GetNewEmpSancOdDetailsReq(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "601").FirstOrDefault()
                        .LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
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

                List<GetODClass1> returndata = new List<GetODClass1>();
                returndata.Add(new GetODClass1
                {
                    EmpCode = "EmpCode",
                    EmpName = "EmpName",
                    SwipeDate = "SwipeDate",
                    CInTime = "Comp InTime",
                    COutTime = "Comp OutTime",
                    Remark = "Remark",
                    RowData = new ChildGetLvNewReqClass
                    {
                        LvNewReq = "0",
                        EmpLVid = "0",
                        IsClose = "0",
                        LvHead_Id = "",
                    }
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
                        .Include(e => e.OutDoorDutyReq)
                        .Include(e => e.OutDoorDutyReq.Select(b => b.ProcessedData))
                        .Include(e => e.ProcessedData)
                        .Include(e => e.ProcessedData.Select(r => r.TimingCode))
                        .Include(e => e.OutDoorDutyReq.Select(b => b.WFDetails)).ToList();






                    foreach (var item in Emps)
                    {
                        if (item.OutDoorDutyReq != null && item.OutDoorDutyReq.Count() > 0)
                        {
                            var LvIds = UserManager.OutdoorodFilter(item.OutDoorDutyReq.OrderByDescending(e => e.ReqDate).ToList(),
                               Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                            if (LvIds.Count() > 0)
                            {
                                var outdoorreqdata = item.OutDoorDutyReq.Where(e => LvIds.Contains(e.Id)).ToList();
                                foreach (var singleODDetails in outdoorreqdata)
                                {
                                    if (singleODDetails.ProcessedData != null)
                                    {
                                        //int QId = singleODDetails.Id;
                                        var procd = db.ProcessedData.Include(e => e.MusterRemarks).Where(e => e.Id == singleODDetails.ProcessedData.Id).SingleOrDefault();
                                        var session = Session["auho"].ToString().ToUpper();
                                        var EmpR = item.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session && e.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                        .Select(e => e.AccessRights.IsClose).FirstOrDefault();
                                        //if (stts.WFStatus == 0)
                                        //{

                                        returndata.Add(new GetODClass1
                                        {
                                            RowData = new ChildGetLvNewReqClass
                                            {
                                                LvNewReq = singleODDetails.ProcessedData.Id.ToString(),
                                                EmpLVid = item.Id.ToString(),
                                                IsClose = EmpR.ToString(),
                                                LvHead_Id = "",
                                            },
                                            EmpCode = item.Employee.EmpCode.ToString(),
                                            EmpName = item.Employee.EmpName.FullNameFML.ToString(),
                                            SwipeDate = procd.SwipeDate != null ? procd.SwipeDate.Value.ToShortDateString() : "--",
                                            CInTime = procd.TimingCode.InTime != null ? procd.TimingCode.InTime.Value.ToShortTimeString() : "--",
                                            COutTime = procd.TimingCode.OutTime != null ? procd.TimingCode.OutTime.Value.ToShortTimeString() : "--",
                                            Remark = procd.MusterRemarks != null ? procd.MusterRemarks.LookupVal.ToUpper() : ""
                                        });
                                        //  }
                                    }
                                }
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

        public ActionResult GetNewEmpApproveOdDetailsReq(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    //FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                    FuncModule = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "601").FirstOrDefault()
                        .LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
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

                List<GetODClass1> returndata = new List<GetODClass1>();
                returndata.Add(new GetODClass1
                {
                    EmpCode = "EmpCode",
                    EmpName = "EmpName",
                    SwipeDate = "SwipeDate",
                    CInTime = "Comp InTime",
                    COutTime = "Comp OutTime",
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
                    if (item.OutDoorDutyReq != null)
                    {
                        foreach (var singleODDetails in item.OutDoorDutyReq)
                        {
                            if (singleODDetails.ProcessedData != null)
                            {
                                int QId = singleODDetails.Id;
                                var procd = db.ProcessedData.Include(e => e.MusterRemarks).Where(e => e.Id == singleODDetails.ProcessedData.Id).SingleOrDefault();
                                var stts = singleODDetails.WFDetails.LastOrDefault();
                                var session = Session["auho"].ToString().ToUpper();
                                var EmpR = item.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session)
                           .Select(e => e.AccessRights.IsClose).FirstOrDefault();




                                if (stts.WFStatus == 1)
                                {
                                    returndata.Add(new GetODClass1
                                    {
                                        RowData = new ChildGetLvNewReqClass
                                        {
                                            LvNewReq = singleODDetails.ProcessedData.Id.ToString(),
                                            EmpLVid = item.Id.ToString(),
                                            IsClose = EmpR.ToString(),
                                            LvHead_Id = "",
                                        },
                                        EmpCode = item.Employee.EmpCode.ToString(),
                                        EmpName = item.Employee.EmpName.FullNameFML.ToString(),
                                        SwipeDate = procd.SwipeDate.Value.ToShortDateString(),
                                        CInTime = procd.InTime.Value.ToShortTimeString(),
                                        COutTime = procd.OutTime.Value.ToShortTimeString(),

                                    });
                                }

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

        public class tempClass
        {
            public string LvName { get; set; }
            public string LvCode { get; set; }
            public string LvBal { get; set; }
            public string FullDetails { get; set; }
        }
        public class EmpLvClass
        {
            public string EmpName { get; set; }
            public string EmpId { get; set; }
            public string EmpCode { get; set; }
            public string LocCode { get; set; }
            public string LocDesc { get; set; }
            public string DeptCode { get; set; }
            public string DeptDesc { get; set; }
            public List<ReqLvHeadWise> LvHeadName { get; set; }
        }
        public class ReqLvHeadWise
        {
            public string LvHeadName { get; set; }
            public string LvHeadCode { get; set; }
            public string LvHeadBal { get; set; }
            public Array LvReq { get; set; }
        }

        public class OdHistory100
        {
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
           // public string LocCode { get; set; }
            public string LocDesc { get; set; }
           // public string DeptCode { get; set; }
            public string DeptDesc { get; set; }

            public ChildGetOdHistoryClass RowData { get; set; }
        }


        public class ChildGetOdHistoryClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }
        }

        public ActionResult GetEmpOdHistory()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    //FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
                    FuncModule = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "601").FirstOrDefault()
                        .LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
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
                         .Include(e => e.Employee.GeoStruct)
                         .Include(e => e.Employee.GeoStruct.Location)
                         .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                         .Include(e => e.Employee.GeoStruct.Department)
                         .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                         .Include(e => e.OutDoorDutyReq.Select(a => a.ProcessedData))
                          .Include(e => e.OutDoorDutyReq.Select(a => a.ProcessedData.MusterRemarks))

                        .Include(e => e.OutDoorDutyReq.Select(a => a.WFStatus))
                        .Include(e => e.OutDoorDutyReq.Select(a => a.WFDetails));
                    //.Where(e => EmpIds.Contains(e.Employee.Id)&& e.ITInvestmentPayment !=null).ToList();

                    Emps = Empid.Where(e => item.ReportingEmployee.Contains(e.Employee.Id)).AsNoTracking().ToList();
                }

                var allLvHead = db.ProgramList.ToList();
                List<EmpLvClass> ListEmpLvClass = new List<EmpLvClass>();

                foreach (var ca in Emps)
                {
                    var item1 = ca.OutDoorDutyReq.ToList();
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
                        var swipedate = item.ProcessedData != null ? item.ProcessedData.SwipeDate.Value.ToString("dd/MM/yyyy") : null;
                        var swiperemark = item.ProcessedData != null && item.ProcessedData.MusterRemarks != null ? item.ProcessedData.MusterRemarks.LookupVal.ToUpper() : null;

                        var ReqDate = item.ReqDate != null ? item.ReqDate.Value.ToString("dd/MM/yyyy") : null;
                        var intime = item.InTime != null ? item.InTime.Value.ToShortTimeString() : null;
                        var outtime = item.OutTime != null ? item.OutTime.Value.ToShortTimeString() : null;
                        var Reason = item.Reason != null ? item.Reason : null;
                        temp.Add(new tempClass
                        {
                            FullDetails =
                                   "ReqDate :" + ReqDate +
                                   " swipedate :" + swipedate +
                                   " swiperemark :" + swiperemark +
                                   " Intime :" + intime +
                                   " Outtime :" + outtime +
                                   " Reason:" + Reason +
                                  " Status :" + Status
                        });
                    }

                    if (temp != null && temp.Count > 0)
                    {
                        oEmpLvClass.EmpId = ca.Employee.Id.ToString();
                        oEmpLvClass.EmpCode = ca.Employee.EmpCode;
                        oEmpLvClass.EmpName = ca.Employee.EmpName.FullNameFML;
                        oEmpLvClass.LocCode = ca.Employee.GeoStruct.Location.LocationObj.LocCode;
                        oEmpLvClass.LocDesc = ca.Employee.GeoStruct.Location.LocationObj.LocDesc;
                        oEmpLvClass.DeptCode = ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode;
                        oEmpLvClass.DeptDesc = ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc;

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


                if (ListEmpLvClass.Count() != 0)
                {
                    List<OdHistory100> returndata = new List<OdHistory100>();
                    returndata.Add(new OdHistory100
                    {
                        EmpCode = "EmpCode",
                        EmpName = "EmpName",
                       // LocCode = "LocCode",
                        LocDesc = "LocDesc",
                       // DeptCode = "DeptCode",
                        DeptDesc = "DeptDesc"
                        
                    });

                    

                    foreach (var item in ListEmpLvClass)
                    {

                        returndata.Add(new OdHistory100
                        {
                            RowData = new ChildGetOdHistoryClass
                            {
                                LvNewReq = "",
                                EmpLVid = item.EmpId,
                                IsClose = "",
                                Status = "",
                                LvHead_Id = "",
                            },
                            EmpName = item.EmpName,
                            EmpCode = item.EmpCode,
                           // LocCode = item.LocCode,
                            LocDesc = item.LocDesc,
                           // DeptCode = item.DeptCode,
                            DeptDesc = item.DeptDesc,

                        });

                    }
                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                return null;
                // return Json(new Utility.JsonClass { status = true, Data = ListEmpLvClass }, JsonRequestBehavior.AllowGet);
            }
        }

        public class ODHistoryofalldataclass
        {
            public string EmpName { get; set; }
            public string EmpCode { get; set; }
            public string ReqDate { get; set; }
            public string swipedate { get; set; }
            public string swiperemark { get; set; }
            public string intime { get; set; }
            public string outtime { get; set; }
            public string Reason { get; set; }
            public string Status { get; set; }
          
        }

        public ActionResult GetEmpOdReqHistory(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id1 = Convert.ToInt32(ids[1]);
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);

                    FuncModule = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "601").FirstOrDefault()
                        .LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
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
                         .Include(e => e.OutDoorDutyReq.Select(a => a.ProcessedData))
                          .Include(e => e.OutDoorDutyReq.Select(a => a.ProcessedData.MusterRemarks))

                        .Include(e => e.OutDoorDutyReq.Select(a => a.WFStatus))
                        .Include(e => e.OutDoorDutyReq.Select(a => a.WFDetails));
                    //.Where(e => EmpIds.Contains(e.Employee.Id)&& e.ITInvestmentPayment !=null).ToList();

                    Emps = Empid.Where(e => item.ReportingEmployee.Contains(e.Employee.Id)).AsNoTracking().ToList();
                   // Emps = Emps.Where(e => e.Employee.Id == id1).FirstOrDefault();
                }


                var allLvHead = db.ProgramList.ToList();
                List<EmpLvClass> ListEmpLvClass = new List<EmpLvClass>();

                foreach (var ca in Emps.Where(e=>e.Employee.Id == id1))
                {
                    var item1 = ca.OutDoorDutyReq.ToList();
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
                        var swipedate = item.ProcessedData != null ? item.ProcessedData.SwipeDate.Value.ToString("dd/MM/yyyy") : null;
                        var swiperemark = item.ProcessedData != null && item.ProcessedData.MusterRemarks != null ? item.ProcessedData.MusterRemarks.LookupVal.ToUpper() : null;

                        var ReqDate = item.ReqDate != null ? item.ReqDate.Value.ToString("dd/MM/yyyy") : null;
                        var intime = item.InTime != null ? item.InTime.Value.ToShortTimeString() : null;
                        var outtime = item.OutTime != null ? item.OutTime.Value.ToShortTimeString() : null;
                        var Reason = item.Reason != null ? item.Reason : null;
                        temp.Add(new tempClass
                        {
                            FullDetails =
                                   " ," + ReqDate +
                                   " ," + swipedate +
                                   " ," + swiperemark +
                                   " ," + intime +
                                   " ," + outtime +
                                   " ," + Reason +
                                   " ," + Status
                        });
                    }

                    if (temp != null && temp.Count > 0)
                    {
                        oEmpLvClass.EmpName = ca.Employee.EmpName.FullNameFML;
                        oEmpLvClass.EmpCode = ca.Employee.EmpCode;
                       
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

                    if (ListEmpLvClass.Count() != 0)
                    {
                        List<ODHistoryofalldataclass> returndata = new List<ODHistoryofalldataclass>();
                        foreach (var item in ListEmpLvClass)
                        {
                            foreach (var item2 in item.LvHeadName)
                            {
                                foreach (var item3 in item2.LvReq)
                                {
                                    string[] values = (item3.ToString().Split(new string[] { "," }, StringSplitOptions.None));

                                    returndata.Add(new ODHistoryofalldataclass
                                    {
                                        EmpCode = item.EmpCode,
                                        EmpName = item.EmpName,
                                        ReqDate = values[1],
                                        swipedate = values[2],
                                        swiperemark = values[3],
                                        intime = values[4],
                                        outtime = values[5],
                                        Reason = values[6],
                                        Status = values[7],

                                    });
                                }
                               
                            }

                        }
                       // return Json(new Object[] { returndata, oEmpLvClass.EmpName, JsonRequestBehavior.AllowGet });
                        return Json(new Utility.JsonClass { status = true, Data = returndata }, JsonRequestBehavior.AllowGet);
                    }
                    
                }
                return null;
               // return Json(new Utility.JsonClass { status = true, Data = returndata }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetEmpInvestmentHistoryPunch(string data, string data1)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    //FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
                    FuncModule = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "601").FirstOrDefault()
                        .LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
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
                List<EmployeeAttendance> Empsp = new List<EmployeeAttendance>();
                List<EmployeeAttendance> Emps = new List<EmployeeAttendance>();
                List<int> Employeeid = new List<int>();
                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                foreach (var item in EmpidsWithfunsub)
                {
                    if (item.ReportingEmployee.Count() > 0)
                    {

                        var Empid = db.EmployeeAttendance
                            .Include(e => e.Employee)
                             .Include(e => e.Employee.EmpName)
                            //.Include(e => e.RawData.Select(a => a.WFStatus))
                            ;


                        Empsp = Empid.Where(e => item.ReportingEmployee.Contains(e.Employee.Id)).AsNoTracking().ToList();
                        foreach (var Attid in Empsp)
                        {
                            Employeeid.Add(Attid.Id);
                        }

                    }

                }

                var Empidn = db.EmployeeAttendance
                           .Include(e => e.Employee)
                            .Include(e => e.Employee.EmpName).Where(e => Employeeid.Contains(e.Id))

                           ;
                if (Empidn != null && Empidn.Count() > 0)
                {
                    Emps = Empidn.ToList();
                }


                // var allLvHead = db.ProgramList.ToList();
                List<EmpLvClass> ListEmpLvClass = new List<EmpLvClass>();
                //  DateTime pFromDate = DateTime.Now.AddDays(-10);
                if (data == null)
                {
                    data = Convert.ToString(DateTime.Now);
                }
                if (data1 == null)
                {
                    data1 = Convert.ToString(DateTime.Now);
                }
                DateTime pFromDate = DateTime.Parse(data);
                DateTime pToDate = DateTime.Parse(data1);
                foreach (var ca in Emps)
                {
                    // var item1 = ca.RawData.ToList();
                    //var item1 = db.RawData.Where(e => e.EmployeeAttendance.Employee.Id == ca.Employee.Id && DbFunctions.TruncateTime(e.SwipeDate.Value) >= pFromDate).OrderBy(e => e.SwipeDate.Value).ToList();
                    //var item1 = db.RawData.Where(e => e.EmployeeAttendance.Employee.Id == ca.Employee.Id && DbFunctions.TruncateTime(e.SwipeDate.Value) >= pFromDate && DbFunctions.TruncateTime(e.SwipeDate.Value) <= pToDate).OrderBy(e => e.SwipeDate.Value).ToList();
                    //var item2 = item1.OrderBy(x=>x.SwipeDate.Value).ThenBy(e => e.SwipeTime.Value.ToShortTimeString()).ToList();
                    var item1 = db.ProcessedData.Include(e => e.MusterRemarks).Where(e => e.EmployeeAttendance.Employee.Id == ca.Employee.Id && DbFunctions.TruncateTime(e.SwipeDate.Value) >= pFromDate && DbFunctions.TruncateTime(e.SwipeDate.Value) <= pToDate).OrderBy(e => e.SwipeDate.Value).ToList();
                    // var item2 = item1.OrderBy(x=>x.SwipeDate.Value).ThenBy(e => e.SwipeTime.Value.ToShortTimeString()).ToList();

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
                        //if (item.WFDetails.Count() > 0)
                        //{
                        //    Status = Utility.GetStatusName().Where(e => e.Key == item.WFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus.ToString())
                        //    .Select(e => e.Value).FirstOrDefault();
                        //}

                        //if (item.InputMethod == 0)
                        //{
                        //    Status = "Approved By HRM (M)";
                        //}
                        var swipedate = item.SwipeDate != null ? item.SwipeDate.Value.ToString("dd/MM/yyyy") : " ";
                        var intime = item.InTime != null ? item.InTime.Value.ToShortTimeString() : " ";
                        var outtime = item.OutTime != null ? item.OutTime.Value.ToShortTimeString() : " ";
                        //var Reason =  "";
                        var remark = item.MusterRemarks != null ? item.MusterRemarks.LookupVal.ToUpper() : " ";
                        temp.Add(new tempClass
                        {
                            FullDetails =
                                   "SwipeDate :" + swipedate + "   " +
                                   "Intime :" + intime + "   " +
                                   "Outtime :" + outtime + "   " +
                                   "Remark :" + remark
                            //"Reason:" + Reason +                    
                            //" Status :" + Status
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
                return Json(new Utility.JsonClass { status = true, Data = ListEmpLvClass }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Get_Employee_History(string data, string data1)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime PunchDate = Convert.ToDateTime(data1);
                int EmpAttId = db.EmployeeAttendance.Include(e => e.Employee).Where(e => e.Employee.EmpCode == data).FirstOrDefault().Id;
                //if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpId)))
                //{
                //    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                //}
                //var Id = Convert.ToInt32(SessionManager.EmpId);

                List<GetPunchDet> returndata = new List<GetPunchDet>();
                returndata.Add(new GetPunchDet
                {
                    SwipeDate = "Swipe Date",
                    SwipeTime = "Swipe Time",
                    InputType = "Input Type",
                    Location = "Location"
                });

                var processdata = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == EmpAttId && DbFunctions.TruncateTime(e.SwipeDate) == PunchDate).FirstOrDefault();

                if (processdata.InTime != null && processdata.OutTime != null)
                {
                    var db_data1 = db.RawData.Include(e => e.InputType)
                      .Where(e => e.EmployeeAttendance.Id == EmpAttId && DbFunctions.TruncateTime(e.SwipeDate) == PunchDate).ToList();

                    var db_data = db_data1.Where(e => e.SwipeTime.Value.ToString("hh:mm") == processdata.InTime.Value.ToString("hh:mm") || e.SwipeTime.Value.ToString("hh:mm") == processdata.OutTime.Value.ToString("hh:mm")).OrderBy(e => e.SwipeTime).ToList();
                    if (db_data != null)
                    {



                        foreach (var a in db_data)
                        {
                            string location = "";
                            if (a.InputType.LookupVal.ToUpper() == "MACHINE")
                            {
                                string getUnitCode = a.UnitCode.ToString();

                                var getUnitIdAssignment = db.UnitIdAssignment.Select(z => new
                                {
                                    UnitId = z.UnitId,
                                    oGeoStruct = z.GeoStruct.Select(y => new
                                    {
                                        oLocDesc = y.Location.LocationObj.LocDesc,
                                        oDeptNm = y.Department.DepartmentObj.DeptDesc,
                                    }).FirstOrDefault(),

                                }).Where(e => e.UnitId == getUnitCode).FirstOrDefault();

                                location = getUnitIdAssignment != null && getUnitIdAssignment.oGeoStruct != null ? getUnitIdAssignment.oGeoStruct.oLocDesc + " , " + getUnitIdAssignment.oGeoStruct.oDeptNm : " ";
                            }
                            else if (a.InputType.LookupVal.ToUpper() == "MOBILE")
                            {
                                location = a.Narration;
                            }


                            returndata.Add(new GetPunchDet
                            {
                                SwipeDate = a.SwipeDate != null ? a.SwipeDate.Value.ToShortDateString() : "--",
                                SwipeTime = a.SwipeTime != null ? a.SwipeTime.Value.ToString("HH:mm:ss") : "--",
                                InputType = a.InputType != null ? a.InputType.LookupVal.ToString() : "Machine",
                                Location = location,
                            });

                        }


                        return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }



        public ActionResult GetEmpReqDataODDetails(string data)
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

                var OdData = db.OutDoorDutyReq.Where(e => e.ProcessedData.Id == id).OrderByDescending(e => e.Id)
                    .Select(e => new
                    {
                        ReqDate = e.ReqDate,
                        InTime = e.InTime,
                        OutTime = e.OutTime,
                        SwipeDate = e.ProcessedData.SwipeDate,
                        Remark = e.ProcessedData.MusterRemarks,
                        Reason = e.Reason
                    }).FirstOrDefault();


                var stat = db.OutDoorDutyReq.Where(e => e.ProcessedData.Id == id).OrderByDescending(e => e.Id).Select(e => e.WFDetails).FirstOrDefault();
                if (stat != null)
                {
                    int statss = stat.Select(a1 => a1.WFStatus).LastOrDefault();

                    var oVar = db.DT_PassportDetails.Where(e => e.Id == id).SingleOrDefault();
                    var list = new
                    {
                        EmployeeName = empfind.EmpName,
                        SwipeDate = OdData.SwipeDate.Value.ToShortDateString(),
                        ReqDate = OdData.ReqDate.Value.ToShortDateString(),
                        InTime = OdData.InTime.Value.ToString("hh:mm tt"),
                        OutTime = OdData.OutTime.Value.ToString("hh:mm tt"),
                        Remark = OdData.Remark.LookupVal.ToString(),
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






        public ActionResult Edit(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);

                var returndata = (Object)null;
                List<GetODClass> odclassdata = new List<GetODClass>();

                var Q = db.ProcessedData.Include(e => e.MusterRemarks).Where(e => e.Id == id).ToList();

                var db_data = db.EmployeeAttendance
                    .Include(e => e.Employee)
                    .Include(e => e.ProcessedData).Include(e => e.ProcessedData.Select(a => a.TimingCode))
                    .Include(e => e.OutDoorDutyReq.Select(b => b.WFDetails))
                    .Include(e => e.ProcessedData.Select(b => b.MusterRemarks))

                    .Where(e => e.Employee.Id == Emp).SingleOrDefault();

                foreach (var item in db_data.ProcessedData.Where(e => e.Id == id))
                {
                    var intm = item.TimingCode;


                    var stat = db.OutDoorDutyReq.Where(e => (e.ProcessedData.Id == item.Id) && (e.isCancel != true)).Select(e => e.WFDetails).SingleOrDefault();
                    int statss;
                    var remrk = item.MusterRemarks.LookupVal.ToString();

                    Boolean IsODTimEtryAppl = false;
                    var remarkconfig = db.RemarkConfig.Where(e => e.MusterRemarks_Id == item.MusterRemarks_Id).FirstOrDefault();
                    if (remarkconfig != null)
                    {
                        IsODTimEtryAppl = remarkconfig.IsODTimeEntryAppl;
                    }
                    if (stat != null)
                    {
                        statss = stat.Select(a1 => a1.WFStatus).LastOrDefault();
                    }
                    else
                    {
                        statss = 99;
                    }

                    string MinSWtimeLocation = string.Empty;
                    string MaxSWtimeLocation = string.Empty;

                    var getODRequest = db.RawData.Select(b => new
                    {
                        oSwipeDate = b.SwipeDate,
                        oswipetime = b.SwipeTime,
                        oUnitcode = b.UnitCode.ToString(),
                        oInputType = b.InputType.LookupVal.ToUpper().ToString(),
                        oNarration = b.Narration,
                        oempAttendanceId = b.EmployeeAttendance_Id,
                    }).Where(e => DbFunctions.TruncateTime(e.oSwipeDate) == item.SwipeDate && e.oempAttendanceId == db_data.Id).ToList();

                    if (getODRequest.Count() > 0 && item.InTime != null && item.OutTime != null)
                    {
                        var machineIn = getODRequest.Where(e => e.oswipetime.Value.ToString("hh:mm") == item.InTime.Value.ToString("hh:mm") && e.oInputType == "MACHINE").FirstOrDefault();

                        string getMINswipeTimeData = machineIn != null ? getODRequest.Where(e => e.oswipetime.Value.ToString("hh:mm") == item.InTime.Value.ToString("hh:mm") && e.oInputType == "MACHINE").FirstOrDefault().oUnitcode : null;

                        var machineout = getODRequest.Where(e => e.oswipetime.Value.ToString("hh:mm") == item.OutTime.Value.ToString("hh:mm") && e.oInputType == "MACHINE").FirstOrDefault();

                        string getMAXswipeTimeData = machineout != null ? getODRequest.Where(e => e.oswipetime.Value.ToString("hh:mm") == item.OutTime.Value.ToString("hh:mm") && e.oInputType == "MACHINE").FirstOrDefault().oUnitcode : null;

                        //string getMINswipeTimeData1 = getODRequest.OrderBy(o => o.oswipetime).ToList().FirstOrDefault().oUnitcode;
                        //string getMAXswipeTimeData1 = getODRequest.OrderBy(o => o.oswipetime).ToList().LastOrDefault().oUnitcode;

                        if (!String.IsNullOrEmpty(getMINswipeTimeData))
                        {
                            var getUnitIdAssignment = db.UnitIdAssignment.Select(z => new
                            {
                                UnitId = z.UnitId,
                                oGeoStruct = z.GeoStruct.Select(y => new
                                {
                                    oLocDescMin = y.Location.LocationObj.LocDesc,
                                    oDeptNm = y.Department.DepartmentObj.DeptDesc,
                                }).FirstOrDefault(),

                            }).Where(e => e.UnitId == getMINswipeTimeData).FirstOrDefault();

                            MinSWtimeLocation = getUnitIdAssignment != null && getUnitIdAssignment.oGeoStruct != null ? getUnitIdAssignment.oGeoStruct.oLocDescMin.ToString() : "";
                            MinSWtimeLocation += getUnitIdAssignment != null && getUnitIdAssignment.oGeoStruct != null && getUnitIdAssignment.oGeoStruct.oDeptNm != null ? "/" + getUnitIdAssignment.oGeoStruct.oDeptNm.ToString() : "";

                        }
                        else
                        {
                            string getInputType = getODRequest.Where(e => e.oswipetime.Value.ToString("hh:mm") == item.InTime.Value.ToString("hh:mm") && e.oInputType == "MOBILE").FirstOrDefault().oInputType;
                            //string getInputType1 = getODRequest.OrderBy(o => o.oswipetime).ToList().FirstOrDefault().oInputType;
                            if (getInputType == "MOBILE")
                            {
                                MinSWtimeLocation = getODRequest.Where(e => e.oswipetime.Value.ToString("hh:mm") == item.InTime.Value.ToString("hh:mm") && e.oInputType == "MOBILE").FirstOrDefault().oNarration;
                                // MinSWtimeLocation = getODRequest.OrderBy(o => o.oswipetime).ToList().FirstOrDefault().oNarration;
                            }
                        }
                        if (!String.IsNullOrEmpty(getMAXswipeTimeData))
                        {
                            var getUnitIdAssignment = db.UnitIdAssignment.Select(z => new
                            {
                                UnitId = z.UnitId,
                                oGeoStruct = z.GeoStruct.Select(y => new
                                {
                                    oLocDescMax = y.Location.LocationObj.LocDesc,
                                    oDeptNm = y.Department.DepartmentObj.DeptDesc,
                                }).FirstOrDefault(),

                            }).Where(e => e.UnitId == getMAXswipeTimeData).FirstOrDefault();

                            MaxSWtimeLocation = getUnitIdAssignment != null && getUnitIdAssignment.oGeoStruct != null ? getUnitIdAssignment.oGeoStruct.oLocDescMax.ToString() : "";
                            MaxSWtimeLocation += getUnitIdAssignment != null && getUnitIdAssignment.oGeoStruct != null && getUnitIdAssignment.oGeoStruct.oDeptNm != null ? "/" + getUnitIdAssignment.oGeoStruct.oDeptNm.ToString() : "";
                        }
                        else
                        {
                            string getInputType = getODRequest.Where(e => e.oswipetime.Value.ToString("hh:mm") == item.OutTime.Value.ToString("hh:mm") && e.oInputType == "MOBILE").FirstOrDefault().oInputType;
                            // string getInputType1 = getODRequest.OrderBy(o => o.oswipetime).ToList().LastOrDefault().oInputType;
                            if (getInputType == "MOBILE")
                            {
                                MaxSWtimeLocation = getODRequest.Where(e => e.oswipetime.Value.ToString("hh:mm") == item.OutTime.Value.ToString("hh:mm") && e.oInputType == "MOBILE").FirstOrDefault().oNarration;
                                // MaxSWtimeLocation = getODRequest.OrderBy(o => o.oswipetime).ToList().LastOrDefault().oNarration;
                            }
                        }
                    }


                    odclassdata.Add(new GetODClass
                    {
                        SwipeDate = item.SwipeDate != null ? item.SwipeDate.Value.ToShortDateString() : null,
                        CInTime = intm.InTime != null ? intm.InTime.Value.ToShortTimeString() : null, //Comp intime
                        COutTime = intm.OutTime != null ? intm.OutTime.Value.ToShortTimeString() : null, //Comp Outtime
                        InTime = item.InTime != null ? item.InTime.Value.ToShortTimeString() : null, //My intime
                        MINswLocation = !String.IsNullOrEmpty(MinSWtimeLocation) ? MinSWtimeLocation : "--",
                        OutTime = item.OutTime != null ? item.OutTime.Value.ToShortTimeString() : null, //My outtime
                        MAXswLocation = !String.IsNullOrEmpty(MaxSWtimeLocation) ? MaxSWtimeLocation : "--",
                        ID = id.ToString(),
                        Remark = remrk.ToString(),
                        Status = statss.ToString(),
                        IsODTimEtryAppl = IsODTimEtryAppl.ToString()
                    });


                }
                returndata = new
                {
                    Add = true,
                };

                return Json(new Object[] { odclassdata, returndata, "", JsonRequestBehavior.AllowGet });
            }
        }

        public Object EditSave(ProcessedData c, int data, FormCollection form) // Edit submit
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                //MyInTime
                string intime = form["MyInTime"] == "0" ? "" : form["MyInTime"];
                string outtime = form["MyOutTime"] == "0" ? "" : form["MyOutTime"];
                string resn = form["Reason"] == "0" ? "" : form["Reason"];


                //// bool Auth = form["autho_allow"] == "true" ? true : false;
                //bool Auth = true;


                //if (Auth == false)
                //{
                if (ModelState.IsValid)
                {
                    try
                    {

                        var db_data = db.ProcessedData.Where(e => e.Id == data).SingleOrDefault();


                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            db.ProcessedData.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = db_data.RowVersion;
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                            var Curr_Lookup = db.Employee.Find(data);
                            TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                            db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {

                                // PassportDetails blog = null; // to retrieve old data
                                ProcessedData blog = null;

                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    // blog = context.PassportDetails.Where(e => e.Id == data).SingleOrDefault();
                                    blog = context.ProcessedData.Where(e => e.Id == data).SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }

                                c.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = "0029",
                                    ModifiedOn = DateTime.Now
                                };
                                OutDoorDutyReq lk = new OutDoorDutyReq
                                {

                                    InTime = c.InTime,
                                    OutTime = c.OutTime,
                                    ReqDate = c.SwipeDate,


                                    DBTrack = c.DBTrack,

                                };


                                db.OutDoorDutyReq.Attach(lk);

                                db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];


                                //using (var context = new DataBaseContext())
                                //{

                                //    var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                //    DT_PassportDetails DT_Corp = (DT_PassportDetails)obj;

                                //    db.Create(DT_Corp);
                                //    db.SaveChanges();
                                //}
                                ////  await db.SaveChangesAsync();
                                ts.Complete();


                                return new { status = true, responseText = "Record Saved" };
                            }
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var entry = ex.Entries.Single();
                        var clientValues = (OutDoorDutyReq)entry.Entity;
                        var databaseEntry = entry.GetDatabaseValues();
                        if (databaseEntry == null)
                        {
                            return new { status = true, responseText = "Unable to save changes. The record was deleted by another user." };
                        }
                        else
                        {
                            var databaseValues = (OutDoorDutyReq)databaseEntry.ToObject();
                            c.RowVersion = databaseValues.RowVersion;

                        }
                    }

                    return new { status = true, responseText = "Record modified by another user.So refresh it and try to save again." };
                }

                return new Object[] { };
            }
        }





        public Object Create(OutDoorDutyReq lkval, FormCollection form, String data)
        {
            var Emp = Convert.ToInt32(SessionManager.EmpId);
            var ids = Utility.StringIdsToListString(data);
            var id = Convert.ToInt32(ids[0]);

            string intime = form["MyInTime"] == "0" ? "" : form["MyInTime"];
            string outtime = form["MyOutTime"] == "0" ? "" : form["MyOutTime"];
            string resn = form["Reason"] == "0" ? "" : form["Reason"];
            string prcdid = form["procsname"] == "0" ? "" : form["procsname"];
            bool Status = form["status"] == "true" ? true : false;
            bool MyselfCncl = form["CancelReq"] == "true" ? true : false;

            int prcd = Convert.ToInt32(prcdid);

            using (DataBaseContext db = new DataBaseContext())
            {

                var db_data = db.EmployeeAttendance
                  .Include(e => e.EmpTimingMonthlyRoaster.Select(b => b.TimingPolicy))
                  .Where(e => e.Employee.Id == Emp).SingleOrDefault();

                var proccsd = db.ProcessedData.Include(e => e.TimingCode).Where(e => e.Id == prcd).SingleOrDefault();
                if (intime == "")
                {
                    if (proccsd.InTime != null)
                    {
                        intime = proccsd.InTime.Value.ToShortTimeString();
                    }
                    else if (proccsd.TimingCode.InTime != null)
                    {
                        intime = proccsd.TimingCode.InTime.Value.ToShortTimeString();
                    }

                    // intime = proccsd.InTime != null ? proccsd.InTime.Value.ToShortTimeString() : "";
                }

                if (outtime == "")
                {
                    if (proccsd.OutTime != null)
                    {
                        outtime = proccsd.OutTime.Value.ToShortTimeString();
                    }
                    else if (proccsd.TimingCode.OutTime != null)
                    {
                        outtime = proccsd.TimingCode.OutTime.Value.ToShortTimeString();
                    }
                }


                var intm = proccsd.TimingCode;
                var myintime = Convert.ToDateTime(intime);
                var cintime = intm.InTime.Value.ToShortTimeString();
                var citime = Convert.ToDateTime(cintime);


                //if (myintime > citime)
                //{
                //    return new { status = true, valid = false, responseText = "InTime Should Not Be Greater Than Company InTime." };
                //}

                var myouttime = Convert.ToDateTime(outtime);
                var couttime = intm.OutTime.Value.ToShortTimeString();
                var cotime = Convert.ToDateTime(couttime);

                //if (myouttime < cotime)
                //{
                //    return new Utility.JsonClass { status = true, responseText = "OutTime Should Not Be Less Than Company OutTime." };
                //}

                var reqcount = db.OutDoorDutyReq.Include(e => e.ProcessedData).Include(e => e.WFDetails).Where(e => (e.ProcessedData.Id == prcd) && (e.isCancel != true)).Count();

                int stck = 0;
                if (reqcount != 0)
                {
                    var stschk = db.OutDoorDutyReq.Include(e => e.WFDetails).Where(e => (e.ProcessedData.Id == prcd) && (e.isCancel == true)).SingleOrDefault();
                    stck = stschk.WFDetails.Select(e => e.WFStatus).LastOrDefault();
                }

                // Leave Apply then od can not apply start
                var swipedate = Convert.ToDateTime(proccsd.SwipeDate);
                EmployeeLeave oEmployeeLeave = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.LvNewReq)
                .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                .Include(e => e.LvNewReq.Select(a => a.WFStatus))
               .Where(e => e.Employee.Id == Emp).OrderBy(e => e.Id).SingleOrDefault();

                if (oEmployeeLeave != null)
                {
                    // var LvCalendarFilter = oEmployeeLeave.LvNewReq.Where(e => e.LeaveCalendar.Id == OLeaveCalendar.Id).OrderBy(e => e.Id).ToList();
                    var LvCalendarFilter = oEmployeeLeave.LvNewReq.OrderBy(e => e.Id).ToList();

                    var LvOrignal_id = LvCalendarFilter.Where(e => e.LvOrignal != null).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();
                    var AntCancel = LvCalendarFilter.Where(e => e.IsCancel == false && e.TrReject == false).OrderBy(e => e.Id).ToList();
                    var listLvs = AntCancel.Where(e => !LvOrignal_id.Contains(e.Id) && e.WFStatus.LookupVal != "2" && e.WFStatus.LookupVal != "3").OrderBy(e => e.Id).ToList();

                    if (listLvs.Where(e => e.FromDate <= swipedate && e.ToDate >= swipedate).Count() != 0)
                    {
                        //already exits
                        //MSG.Add("You have already apply leave on " + swipedate.ToShortDateString() + " So you can not apply OD.");
                        //return Json(new { status = 0, MSG = MSG }, JsonRequestBehavior.AllowGet);
                        return new { status = false, responseText = "You have already apply leave on " + swipedate.ToShortDateString() + " So you can not apply OD." };
                    }

                }
                // Leave Apply then od can not apply end

                if ((reqcount == 0) || (stck == 6))
                {
                    //var proccsd = db.ProcessedData.Where(e => e.Id == prcd).SingleOrDefault();

                    //if (intime == " ")
                    //{
                    //    intime = proccsd.InTime.Value.ToShortTimeString();
                    //}

                    //if (outtime == " ")
                    //{
                    //    outtime = proccsd.OutTime.Value.ToShortTimeString();
                    //}
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            lkval.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            AttWFDetails oAttWFDetails = new AttWFDetails
                            {
                                WFStatus = 0,
                                Comments = "Applied",
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                            };

                            List<AttWFDetails> oAttWFDetails_List = new List<AttWFDetails>();
                            oAttWFDetails_List.Add(oAttWFDetails);



                            var pdata = db.ProcessedData.Where(e => e.Id == prcd).SingleOrDefault();
                            OutDoorDutyReq ppdtl = new OutDoorDutyReq
                            {
                                InTime = myintime,
                                OutTime = myouttime,
                                ReqDate = DateTime.Now,
                                Reason = resn,
                                WFDetails = oAttWFDetails_List,
                                ProcessedData = pdata,
                                DBTrack = lkval.DBTrack,
                                InputMethod = 1,
                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(), //db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                TrClosed = false
                            };
                            try
                            {
                                db.OutDoorDutyReq.Add(ppdtl);
                                db.SaveChanges();

                                db.SaveChanges();

                                var empid = Convert.ToInt32(SessionManager.EmpId);
                                var EmpAttendance = db.EmployeeAttendance.Include(e => e.OutDoorDutyReq)
                                    .Include(e => e.ProcessedData).Where(e => e.Employee.Id == empid).SingleOrDefault();

                                if (EmpAttendance != null && EmpAttendance.OutDoorDutyReq != null)
                                {
                                    if (EmpAttendance.OutDoorDutyReq != null)
                                    {
                                        EmpAttendance.OutDoorDutyReq.Add(ppdtl);
                                    }
                                    else
                                    {
                                        EmpAttendance.OutDoorDutyReq = new List<OutDoorDutyReq> { ppdtl };
                                    }
                                }
                                else
                                {
                                    var oEmpAttendace = new EmployeeAttendance();
                                    oEmpAttendace.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    oEmpAttendace.OutDoorDutyReq = new List<OutDoorDutyReq> { ppdtl };
                                }
                                db.Entry(EmpAttendance).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();




                                ts.Complete();
                                return new { status = true, responseText = "Data Created Successfully." };



                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = lkval.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        return new { status = false, responseText = errorMsg };
                    }
                }
                else
                {
                    return new { status = true, responseText = "You have already added requisition for this." };
                }
            }


        }


        public ActionResult AddOrEdit(OutDoorDutyReq lkval, FormCollection form, String data)
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
                    else
                    {
                        //Create
                        var returnobj = Create(lkval, form, data);
                        return Json(returnobj, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}