using EssPortal.App_Start;
using EssPortal.Models;
using EssPortal.Security;
using Microsoft.Ajax.Utilities;
using P2b.Global;
using P2B.SERVICES.Factory;
using P2B.SERVICES.Interface;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;


namespace EssPortal.Controllers
{
    public class DiarySettlementClaimController : Controller
    {
        // GET: DiarySettlementClaim


        readonly IP2BINI p2BINI;
        readonly CASHSettings CASHSettings;
        readonly WOSettings WOSettings;
        readonly WOWORKSettings WOWORKSettings;
        readonly OFFICEWORKSettings OFFICEWORKSettings;
        readonly OTHERSettings OTHERSettings;
        private readonly Type Type;

        public DiarySettlementClaimController()
        {
            p2BINI = P2BINI.RegisterSettings();
            CASHSettings = new CASHSettings(p2BINI.GetSectionValues("CASHSettings"));
            WOSettings = new WOSettings(p2BINI.GetSectionValues("WOSettings"));
            WOWORKSettings = new WOWORKSettings(p2BINI.GetSectionValues("WOWORKSettings"));
            OFFICEWORKSettings = new OFFICEWORKSettings(p2BINI.GetSectionValues("OFFICEWORKSettings"));
            OTHERSettings = new OTHERSettings(p2BINI.GetSectionValues("OTHERSettings"));
            Type = typeof(DiarySettlementClaimController);

        }




        public ActionResult hidebutton(string DiaryType)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var comp = db.Company.FirstOrDefault();
                if (DiaryType.ToUpper() == "CASH")
                {
                    if (CASHSettings.CCode.ToUpper() == comp.Code.ToUpper() && CASHSettings.DType.ToUpper() == DiaryType.ToUpper())
                    {
                        string s = CASHSettings.DInVisibleControlName;
                        string[] values = s.Split(',');

                        string en = CASHSettings.DDisbleControlName;
                        string[] enValues = en.Split(',');

                        string vis = CASHSettings.DVisibleControlName;
                        string[] visValues = vis.Split(',');

                        return Json(new { values, enValues, visValues }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (DiaryType.ToUpper() == "WEEKLYOFF")
                {
                    if (WOSettings.CCode.ToUpper() == comp.Code.ToUpper() && WOSettings.DType.ToUpper() == DiaryType.ToUpper())
                    {
                        string s = WOSettings.DInVisibleControlName;
                        string[] values = s.Split(',');

                        string en = WOSettings.DDisbleControlName;
                        string[] enValues = en.Split(',');

                        string vis = WOSettings.DVisibleControlName;
                        string[] visValues = vis.Split(',');

                        return Json(new { values, enValues, visValues }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (DiaryType.ToUpper() == "OFFICEWORK")
                {
                    if (OFFICEWORKSettings.CCode.ToUpper() == comp.Code.ToUpper() && OFFICEWORKSettings.DType.ToUpper() == DiaryType.ToUpper())
                    {
                        string s = OFFICEWORKSettings.DInVisibleControlName;
                        string[] values = s.Split(',');

                        string en = OFFICEWORKSettings.DDisbleControlName;
                        string[] enValues = en.Split(',');

                        string vis = OFFICEWORKSettings.DVisibleControlName;
                        string[] visValues = vis.Split(',');

                        return Json(new { values, enValues, visValues }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (DiaryType.ToUpper() == "WEEKLYOFFWITHWORK")
                {
                    if (WOWORKSettings.CCode.ToUpper() == comp.Code.ToUpper() && WOWORKSettings.DType.ToUpper() == DiaryType.ToUpper())
                    {
                        string s = WOWORKSettings.DInVisibleControlName;
                        string[] values = s.Split(',');

                        string en = WOWORKSettings.DDisbleControlName;
                        string[] enValues = en.Split(',');

                        string vis = WOWORKSettings.DVisibleControlName;
                        string[] visValues = vis.Split(',');

                        return Json(new { values, enValues, visValues }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (DiaryType.ToUpper().Contains("OTHER"))
                {
                    if (OTHERSettings.CCode.ToUpper() == comp.Code.ToUpper() && OTHERSettings.DType.ToUpper().Contains("OTHER") == DiaryType.ToUpper().Contains("OTHER"))
                    {
                        string s = OTHERSettings.DInVisibleControlName;
                        string[] values = s.Split(',');

                        string en = OTHERSettings.DDisbleControlName;
                        string[] enValues = en.Split(',');

                        string vis = OTHERSettings.DVisibleControlName;
                        string[] visValues = vis.Split(',');

                        return Json(new { values, enValues, visValues }, JsonRequestBehavior.AllowGet);
                    }
                }


            }

            return null;

        }
        public ActionResult Index()
        {
            return View("~/Views/DiarySettlementClaim/index.cshtml");
        }
        public ActionResult Partial_JourneyDetails()
        {
            return View("~/Views/Shared/_JourneyDetails.cshtml");
        }

        public ActionResult Partial_TravelHotelBooking()
        {
            return View("~/Views/Shared/_TravelHotelBooking.cshtml");
        }

        public ActionResult DIARYSettlementClaimPartialSanction()
        {
            return View("~/Views/Shared/DIARYSettlementClaim.cshtml");
        }
        public ActionResult Partial_DiarySettelmentGrid()
        {
            return View("~/Views/Shared/_DiarySettelmentGrid.cshtml");
        }

        public class ChildGetDiarysettleReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

        }
        public class GetDiarysettleReqClass
        {
            public string Emp { get; set; }
            public string ReqDate { get; set; }
            public string DIARYType { get; set; }
            public string SettlementAmount { get; set; }
            public string AdvanceAmount { get; set; }
            public string Status { get; set; }
            public ChildGetDiarysettleReqClass RowData { get; set; }
        }

        public ActionResult GetMyDIARYsettlementClaimReq()   /// Get Created Data on Grid
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);

                List<GetDiarysettleReqClass> DIARYSettlementlist = new List<GetDiarysettleReqClass>();
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);


                var db_data = db.EmployeePayroll
                      .Where(e => e.Id == Id)
                      .Include(e => e.DIARYSettlementClaim)
                      .Include(e => e.DIARYSettlementClaim.Select(a => a.LTCWFDetails))
                      .Include(e => e.DIARYSettlementClaim.Select(a => a.DIARYType))

                     .FirstOrDefault();


                if (db_data != null)
                {
                    List<GetDiarysettleReqClass> returndata = new List<GetDiarysettleReqClass>();
                    returndata.Add(new GetDiarysettleReqClass
                    {

                        ReqDate = "Requisition Date",
                        DIARYType = "DIARYType",
                        SettlementAmount = "SettlementAmount",
                        AdvanceAmount = "AdvanceAmount",
                        Status = "Status"
                    });

                    var DiarysettlementClaimlist = db_data.DIARYSettlementClaim.ToList();

                    foreach (var Diarysettleitems in DiarysettlementClaimlist)
                    {
                        int WfStatusNew = Diarysettleitems.LTCWFDetails.Select(w => w.WFStatus).LastOrDefault();
                        string Comments = Diarysettleitems.LTCWFDetails.Select(c => c.Comments).LastOrDefault();

                        string StatusNarration = "";
                        if (WfStatusNew == 0)
                            StatusNarration = "Applied";
                        else if (WfStatusNew == 1)
                            StatusNarration = "Sanctioned";
                        else if (WfStatusNew == 2)
                            StatusNarration = "Rejected by Sanction";
                        else if (WfStatusNew == 3)
                            StatusNarration = "Approved";
                        else if (WfStatusNew == 4)
                            StatusNarration = "Rejected by Approval";
                        else if (WfStatusNew == 5)
                            StatusNarration = "Approved By HRM (M)";


                        if (authority.ToUpper() == "SANCTION" && WfStatusNew == 0)
                        {
                            GetDiarysettleReqClass ObjDiarysettleClaimRequest = new GetDiarysettleReqClass()
                            {
                                RowData = new ChildGetDiarysettleReqClass
                                {
                                    LvNewReq = Diarysettleitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },

                                DIARYType = Diarysettleitems.DIARYType.LookupVal.ToString(),
                                SettlementAmount = Diarysettleitems.SettlementAmt.ToString() != "0" ? Diarysettleitems.SettlementAmt.ToString() : "0",
                                ReqDate = Diarysettleitems.DateOfAppl.Value.ToShortDateString(),
                                AdvanceAmount = Diarysettleitems.AdvAmt.ToString() != "0" ? Diarysettleitems.AdvAmt.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjDiarysettleClaimRequest);
                        }
                        else if (authority.ToUpper() == "APPROVAL" && WfStatusNew == 1)
                        {
                            GetDiarysettleReqClass ObjDiarysettleClaimRequest = new GetDiarysettleReqClass()
                            {
                                RowData = new ChildGetDiarysettleReqClass
                                {
                                    LvNewReq = Diarysettleitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },

                                DIARYType = Diarysettleitems.DIARYType.LookupVal.ToString(),
                                SettlementAmount = Diarysettleitems.SettlementAmt.ToString() != "0" ? Diarysettleitems.SettlementAmt.ToString() : "0",
                                ReqDate = Diarysettleitems.DateOfAppl.Value.ToShortDateString(),
                                AdvanceAmount = Diarysettleitems.AdvAmt.ToString() != "0" ? Diarysettleitems.AdvAmt.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjDiarysettleClaimRequest);
                        }
                        else if (authority.ToUpper() == "MYSELF")
                        {
                            GetDiarysettleReqClass ObjDiarysettleClaimRequest = new GetDiarysettleReqClass()
                            {
                                RowData = new ChildGetDiarysettleReqClass
                                {
                                    LvNewReq = Diarysettleitems.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString()

                                },

                                DIARYType = Diarysettleitems.DIARYType.LookupVal.ToString(),
                                SettlementAmount = Diarysettleitems.SettlementAmt.ToString() != "0" ? Diarysettleitems.SettlementAmt.ToString() : "0",
                                ReqDate = Diarysettleitems.DateOfAppl.Value.ToShortDateString(),
                                AdvanceAmount = Diarysettleitems.AdvAmt.ToString() != "0" ? Diarysettleitems.AdvAmt.ToString() : "0",
                                Status = StatusNarration
                            };
                            returndata.Add(ObjDiarysettleClaimRequest);
                        }

                    }

                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

            }

        }

        public ActionResult GetLookupDiaryAdv(string data, string Empid)
        {
            int empids = Convert.ToInt32(Empid);


            using (DataBaseContext db = new DataBaseContext())
            {
                var employeedata = db.EmployeePayroll.Include(e => e.Employee)
                    .Include(e => e.DIARYAdvanceClaim)
                    .Include(e => e.DIARYAdvanceClaim.Select(c => c.DIARYType))
                    .Where(e => e.Employee.Id == empids).ToList();


                var fall = employeedata.SelectMany(e => e.DIARYAdvanceClaim).ToList();

                IEnumerable<DIARYAdvanceClaim> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.DIARYAdvanceClaim.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    //var list1 = db.FamilyDetails.ToList();

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookupJourneyDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.JourneyDetails.ToList();
                IEnumerable<TravelHotelBooking> all;
                if (SkipIds != null)
                {
                    foreach (int a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.JourneyDetails.ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();

                    }
                }

                var list1 = db.LTCSettlementClaim.Where(e => e.JourneyDetails != null).ToList().Select(e => e.JourneyDetails);
                var list2 = db.DIARYSettlementClaim.Where(e => e.JourneyDetails != null).ToList().Select(e => e.JourneyDetails);
                var list3 = fall.Except(list1).Except(list2);

                var r = (from ca in list3 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetTravel_Hotel_Booking(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TravelHotelBooking.Include(e => e.HotelEligibilityPolicy).ToList();
                // IEnumerable<TravelHotelBooking> all;
                if (SkipIds != null)
                {
                    foreach (int a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TravelHotelBooking.ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();

                    }
                }

                var list1 = db.LTCSettlementClaim.Where(e => e.Travel_Hotel_Booking.Count() > 0).ToList().SelectMany(e => e.Travel_Hotel_Booking);
                var list2 = db.DIARYSettlementClaim.Where(e => e.Travel_Hotel_Booking.Count() > 0).ToList().SelectMany(e => e.Travel_Hotel_Booking);
                var list3 = fall.Except(list1).Except(list2);

                var r = (from ca in list3 select new { srno = ca.Id, lookupvalue = ca.HotelName + ", StartDate :" + ca.StartDate.Value.ToString("dd/MM/yyyy") }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public class returnEditClass
        {
            public int Id { get; set; }
            public string LookupVal { get; set; }
        }

        public ActionResult GetLookupPolicyname(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<returnEditClass> query1 = new List<returnEditClass>();

                query1.Add(new returnEditClass
                {
                    Id = 1,
                    LookupVal = "DIARY"
                });
                SelectList s = (SelectList)null;
                var selected = "";
                if (data != "" && data != null)
                {
                    if (data2 != "" && data2 != "0")
                    {
                        selected = data2;
                    }
                    if (query1 != null)
                    {
                        s = new SelectList(query1, "Id", "LookupVal", selected);
                    }
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(DIARYSettlementClaim c, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();
                    int Emp = form["EmpLvnereq_Id"] == "0" ? 0 : Convert.ToInt32(form["EmpLvnereq_Id"]);
                    string DIARYadvanceclaimlist = form["DIARYAdvanceClaimList"] == "0" ? "" : form["DIARYAdvanceClaimList"];
                    string DIARYtypelist = form["DIARYTypelist"] == "0" ? "" : form["DIARYTypelist"];
                    // string lvreqlist = form["LvReqList"] == "0" ? "" : form["LvReqList"];
                    string journewdetailslist = form["JourneyDetailsList"] == "0" ? "" : form["JourneyDetailsList"];
                    string trhotelbookinglist = form["Travel_Hotel_BookingList"] == "0" ? "" : form["Travel_Hotel_BookingList"];
                    //  string LtcBlock = form["BlockId"] == "0" ? "" : form["BlockId"];


                    //string ddlIncharge = form["ddlIncharge"] == "" ? null : form["ddlIncharge"];
                    string ddlIncharge = form["Incharge_id"] == "" ? null : form["Incharge_id"];

                    if (DIARYadvanceclaimlist != null && DIARYadvanceclaimlist != "")
                    {
                        int DIARYadvanceid = Convert.ToInt32(DIARYadvanceclaimlist);
                        var value = db.DIARYAdvanceClaim.Where(e => e.Id == DIARYadvanceid).SingleOrDefault();
                        c.DIARYAdvanceClaim = value;

                    }
                    if (trhotelbookinglist != null)
                    {
                        var ids = Utility.StringIdsToListIds(trhotelbookinglist);
                        var travelhotelbookinglist = new List<TravelHotelBooking>();
                        foreach (var item in ids)
                        {

                            int hotellistid = Convert.ToInt32(item);
                            var val = db.TravelHotelBooking.Include(e => e.HotelEligibilityPolicy).Where(e => e.Id == hotellistid).SingleOrDefault();
                            if (val != null)
                            {
                                travelhotelbookinglist.Add(val);
                            }
                        }
                        c.Travel_Hotel_Booking = travelhotelbookinglist;
                    }



                    if (journewdetailslist != null && journewdetailslist != "")
                    { 
                            int ids = Convert.ToInt32(journewdetailslist);
                            var val = db.JourneyDetails.Where(e => e.Id == ids).SingleOrDefault();
                            c.JourneyDetails = val; 
                    }
                    else
                    {
                        Msg.Add("Select Journey Details.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                 

                    if (DIARYtypelist != null && DIARYtypelist != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(DIARYtypelist));
                        c.DIARYType = val;
                    }
                    var employeedata = db.Employee.Include(e => e.GeoStruct).Include(e => e.PayStruct).Include(e => e.FuncStruct).Where(e => e.Id == Emp).SingleOrDefault();
                    c.PayStruct = employeedata.PayStruct;
                    c.GeoStruct = employeedata.GeoStruct;
                    c.FuncStruct = employeedata.FuncStruct;
                    // int Blockid = int.Parse(LtcBlock);
                    // EmpLTCBlockT OEmpLTCBlockT = db.EmpLTCBlockT.Find(Blockid);

                    var a = db.EmployeePayroll.Include(e => e.EmpLTCBlock).Include(e => e.Employee)
                             .Include(e => e.EmpLTCBlock.Select(y => y.EmpLTCBlockT))
                             .Where(e => e.Employee.Id == Emp).AsNoTracking().SingleOrDefault().EmpLTCBlock.Where(e => e.IsBlockClose == false).FirstOrDefault();

                    ////var test = a.EmpLTCBlockT.Where(e => e.BlockPeriodStart >= OEmpLTCBlockT.BlockPeriodStart
                    ////   && e.BlockPeriodEnd <= OEmpLTCBlockT.BlockPeriodEnd).FirstOrDefault();
                    //if (c.SettlementAmt > c.Eligible_Amt)
                    //{
                    //    Msg.Add("Settlement amount Should not be greater than Eligible amount.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    //if (c.SanctionAmt > c.Eligible_Amt)
                    //{
                    //    Msg.Add("Sanction amount Should not be greater than Eligible amount.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    //if (c.SettlementAmt > c.SanctionAmt)
                    //{
                    //    Msg.Add("Settlement amount Should not be greater than sanction amount.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    // yearlypayment

                    var emppi = db.EmployeePayroll.Where(e => e.Employee.Id == Emp).SingleOrDefault();
                    int salhdid = 0;
                    var fyyr = db.Calendar.Include(x => x.Name).Where(x => x.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && x.Default == true).SingleOrDefault();

                    //var QEmpSalStruct1 = db.EmpSalStruct//.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.EffectiveDate >= comparedate)
                    //                      .Include(e => e.EmpSalStructDetails)
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                    //                      .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead));//.ToList();
                    //var EmpSalStructTotal = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == emppi.Id && e.EndDate == null)
                    //                              .ToList();
                    //var OEmpSalStruct = EmpSalStructTotal.Where(e => e.EndDate == null).SingleOrDefault();
                    //if (OEmpSalStruct != null)
                    //{

                    //    salhdid = OEmpSalStruct.EmpSalStructDetails
                    //       .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                    //    && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                    //    && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "DIARY"
                    //    )).FirstOrDefault() != null ? OEmpSalStruct.EmpSalStructDetails
                    //       .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                    //    && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                    //    && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "DIARY"
                    //    )).FirstOrDefault().SalaryHead.Id : 0;
                    //}


                    //if (salhdid == 0)
                    //{
                    //    Msg.Add("Please Define DIARY in salary Structure.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    FunctAllWFDetails oAttWFDetails = new FunctAllWFDetails
                    {
                        WFStatus = 0,
                        Comments = c.Remark.ToString(),
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };

                    List<FunctAllWFDetails> oAttWFDetails_List = new List<FunctAllWFDetails>();
                    oAttWFDetails_List.Add(oAttWFDetails);

                    if (ModelState.IsValid)
                    {


                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            DIARYSettlementClaim DIARYsettlement = new DIARYSettlementClaim()
                            {
                                DateOfAppl = c.DateOfAppl,
                                //  EncashmentAmount = c.EncashmentAmount,
                                JourneyDetails = c.JourneyDetails,
                                Travel_Hotel_Booking = c.Travel_Hotel_Booking,
                                DIARYAdvanceClaim = c.DIARYAdvanceClaim,
                                NoOfDays = c.NoOfDays,
                                DIARYType = c.DIARYType,
                                //LvNewReq = c.LvNewReq,
                                Claim_Amt = c.Claim_Amt,
                                Eligible_Amt = c.Eligible_Amt,
                                AdvAmt = c.AdvAmt,
                                SanctionAmt = c.SanctionAmt,
                                SettlementAmt = c.SettlementAmt,
                                Remark = c.Remark,
                                FuncStruct = c.FuncStruct,
                                GeoStruct = c.GeoStruct,
                                PayStruct = c.PayStruct,
                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                LTCWFDetails = oAttWFDetails_List,
                                DBTrack = c.DBTrack
                            };

                            db.DIARYSettlementClaim.Add(DIARYsettlement);
                            db.SaveChanges();

                            // yearly payment strat
                            //YearlyPaymentT ObjYPT = new YearlyPaymentT();
                            //{
                            //    ObjYPT.SalaryHead = db.SalaryHead.Find(salhdid);
                            //    ObjYPT.AmountPaid = c.SettlementAmt;
                            //    ObjYPT.FromPeriod = fyyr.FromDate;
                            //    ObjYPT.ToPeriod = fyyr.ToDate;
                            //    ObjYPT.ProcessMonth = Convert.ToDateTime(c.DateOfAppl).ToString("MM/yyyy");
                            //    ObjYPT.PayMonth = Convert.ToDateTime(c.DateOfAppl).ToString("MM/yyyy");
                            //    ObjYPT.ReleaseDate = c.DateOfAppl;
                            //    ObjYPT.ReleaseFlag = true;
                            //    ObjYPT.TDSAmount = 0;
                            //    ObjYPT.OtherDeduction = 0;
                            //    ObjYPT.FinancialYear = fyyr;
                            //    ObjYPT.DBTrack = c.DBTrack;
                            //    ObjYPT.FuncStruct = c.FuncStruct;
                            //    ObjYPT.GeoStruct = c.GeoStruct;
                            //    ObjYPT.PayStruct = c.PayStruct;

                            //}
                            //List<YearlyPaymentT> OYrlyPaylist = new List<YearlyPaymentT>();
                            //db.YearlyPaymentT.Add(ObjYPT);
                            //db.SaveChanges();
                            //OYrlyPaylist.Add(ObjYPT);

                            //yearly payment end

                            List<DIARYSettlementClaim> DIARYSettlementlist = new List<DIARYSettlementClaim>();
                            DIARYSettlementlist.Add(db.DIARYSettlementClaim.Find(DIARYsettlement.Id));
                            EmployeePayroll aa = db.EmployeePayroll.Include(e => e.DIARYSettlementClaim).Include(e => e.YearlyPaymentT).Include(e => e.Employee).Where(e => e.Employee.Id == Emp).SingleOrDefault();
                            if (aa.DIARYSettlementClaim.Count() > 0)
                            {
                                DIARYSettlementlist.AddRange(aa.DIARYSettlementClaim);
                            }
                            else
                            {
                                DIARYSettlementlist.Add(DIARYsettlement);
                            }
                            //if (aa.YearlyPaymentT.Count() > 0)
                            //{
                            //    OYrlyPaylist.AddRange(aa.YearlyPaymentT);
                            //}

                            aa.DIARYSettlementClaim = DIARYSettlementlist;
                            db.EmployeePayroll.Attach(aa);
                            //db.EmpLTCBlockT.Attach(aa);
                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonClass { status = true, responseText = "  Data Saved successfully  " }, JsonRequestBehavior.AllowGet);
                            //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        public class GetDiarysettlamt //childgrid
        {

            public double AdvanceAmt { get; set; }
            public double DIARYClaimamt { get; set; }
            public double DIARYElgmamt { get; set; }
            public double SanctionAmt { get; set; }
            public double SettlementAmt { get; set; }
        }

        [HttpPost]
        public ActionResult GetDIARYAmount(FormCollection form, int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string journewdetailslist = form["JourneyDetailsList"] == "0" ? "" : form["JourneyDetailsList"];
                    string Diaryadvanceclaimlist = form["DIARYAdvanceClaimList"] == "0" ? "" : form["DIARYAdvanceClaimList"];
                    string travelhotelbookinglist = form["Travel_Hotel_BookingList"] == "0" ? "" : form["Travel_Hotel_BookingList"];
                    string tadatypelist = form["TADATypelist"] == "0" ? "" : form["TADATypelist"]; 
                    string DIARYadvanceclaimlist = form["DIARYAdvanceClaimList"] == "0" ? "" : form["DIARYAdvanceClaimList"];
 

                    List<BMSModuleProcess.GetDIARYsettlamt> returndata = new List<BMSModuleProcess.GetDIARYsettlamt>();
                    returndata = BMSModuleProcess.Calculate_GetDiaryAmount(data, DIARYadvanceclaimlist, travelhotelbookinglist, journewdetailslist, null);

                    return Json(returndata, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    throw e;
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

        public class DIARYSettlClaimChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string ReqDate { get; set; }
            public string DIARYType { get; set; }
            public double EligibleAmt { get; set; }
            public double AdvanceAmt { get; set; }
            public string Remark { get; set; }

        }

        public ActionResult Get_DIARYSettlementClaim(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var db_data = db.EmployeePayroll
                       .Include(e => e.DIARYSettlementClaim)
                        .Include(e => e.DIARYSettlementClaim.Select(c => c.DIARYType))
                       .Where(e => e.Id == data).FirstOrDefault();

                    if (db_data.DIARYSettlementClaim != null)
                    {
                        List<DIARYSettlClaimChildDataClass> returndata = new List<DIARYSettlClaimChildDataClass>();
                        var LTCAdvClaimlist = db_data.DIARYSettlementClaim;
                        foreach (var item in db_data.DIARYSettlementClaim.OrderByDescending(e => e.Id))
                        {

                            returndata.Add(new DIARYSettlClaimChildDataClass
                            {
                                Id = item.Id,
                                ReqDate = item.DateOfAppl.Value != null ? item.DateOfAppl.Value.ToShortDateString() : "",
                                DIARYType = item.DIARYType != null ? item.DIARYType.LookupVal.ToString() : null,
                                EligibleAmt = item.Eligible_Amt,
                                AdvanceAmt = item.AdvAmt,
                                Remark = item.Remark,

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

        public ActionResult GetLookupHotelEligibilityPolicy(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.HotelEligibilityPolicy.ToList();
                IEnumerable<HotelEligibilityPolicy> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.HotelEligibilityPolicy.ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public class ChildGetDIARYSettlementReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }
            public string OdReqId { get; set; }

        }
        public class GetDIARYSettlementClaimClass1
        {
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string ReqDate { get; set; }
            public string DIARYType { get; set; }
            public string SettlementAmount { get; set; }
            public string AdvanceAmount { get; set; }
            public ChildGetDIARYSettlementReqClass RowData { get; set; }
        }

        public ActionResult GetDiarySettlementClaimReqonSanction(FormCollection form)
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
                // var returnDataClass = new List<returnDataClass>();

                List<GetDIARYSettlementClaimClass1> returndata = new List<GetDIARYSettlementClaimClass1>();
                returndata.Add(new GetDIARYSettlementClaimClass1
                {
                    ReqDate = "Requisition Date",
                    DIARYType = "DIARY Type",
                    SettlementAmount = "Settlement Amount",
                    AdvanceAmount = "Advance Amount",
                    EmpCode = "EmpCode",
                    EmpName = "EmpName",

                    RowData = new ChildGetDIARYSettlementReqClass
                    {
                        LvNewReq = "0",
                        EmpLVid = "0",
                        IsClose = "0",
                        LvHead_Id = "",
                    }
                });
                foreach (var item1 in EmpidsWithfunsub)
                {


                    var Emps = db.EmployeePayroll
                        .Where(e => (item1.ReportingEmployee.Contains(e.Employee.Id)))
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.ReportingStructRights)
                        .Include(e => e.Employee.ReportingStructRights.Select(b => b.AccessRights.ActionName))
                        .Include(e => e.Employee.ReportingStructRights.Select(b => b.FuncModules))
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.DIARYSettlementClaim)
                        .Include(e => e.DIARYSettlementClaim.Select(b => b.LTCWFDetails))
                        .Include(e => e.DIARYSettlementClaim.Select(w => w.WFStatus))
                        .Include(e => e.DIARYSettlementClaim.Select(w => w.DIARYType))
                        .ToList();

                    foreach (var item in Emps)
                    {
                        if (item.DIARYSettlementClaim != null && item.DIARYSettlementClaim.Count() > 0)
                        {
                            var LvIds = UserManager.FilterDIARYSettlementClaim(item.DIARYSettlementClaim.OrderByDescending(e => e.DateOfAppl.Value.ToShortDateString()).ToList(),
                               Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                            if (LvIds.Count() > 0)
                            {
                                var Diarysettlementclaimreqdata = item.DIARYSettlementClaim.Where(e => LvIds.Contains(e.Id)).ToList();
                                foreach (var singleDiaryDetails in Diarysettlementclaimreqdata)
                                {
                                    if (singleDiaryDetails.LTCWFDetails != null)
                                    {

                                        var session = Session["auho"].ToString().ToUpper();
                                        var EmpR = item.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session && e.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                        .Select(e => e.AccessRights.IsClose).FirstOrDefault();
                                        //if (stts.WFStatus == 0)
                                        //{

                                        returndata.Add(new GetDIARYSettlementClaimClass1
                                        {
                                            RowData = new ChildGetDIARYSettlementReqClass
                                            {
                                                LvNewReq = singleDiaryDetails.Id.ToString(),
                                                EmpLVid = item.Id.ToString(),
                                                IsClose = EmpR.ToString(),
                                                LvHead_Id = "",
                                            },

                                            DIARYType = singleDiaryDetails.DIARYType.LookupVal.ToString(),
                                            SettlementAmount = singleDiaryDetails.SettlementAmt.ToString() != "0" ? singleDiaryDetails.SettlementAmt.ToString() : "0",
                                            ReqDate = singleDiaryDetails.DateOfAppl.Value.ToShortDateString(),
                                            AdvanceAmount = singleDiaryDetails.AdvAmt.ToString() != "0" ? singleDiaryDetails.AdvAmt.ToString() : "0",
                                            EmpCode = item.Employee.EmpCode,
                                            EmpName = item.Employee.EmpName.FullNameFML

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
        public ActionResult GetDiaryClaimAttendanceReqonSanction(FormCollection form)
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
                // var returnDataClass = new List<returnDataClass>();

                List<GetDIARYSettlementClaimClass1> returndata = new List<GetDIARYSettlementClaimClass1>();
                returndata.Add(new GetDIARYSettlementClaimClass1
                {
                    ReqDate = "Requisition Date",
                    DIARYType = "DIARY Type",
                    SettlementAmount = "Settlement Amount",
                    AdvanceAmount = "Advance Amount",
                    EmpCode = "EmpCode",
                    EmpName = "EmpName",

                    RowData = new ChildGetDIARYSettlementReqClass
                    {
                        LvNewReq = "0",
                        EmpLVid = "0",
                        IsClose = "0",
                        LvHead_Id = "",
                    }
                });
                foreach (var item1 in EmpidsWithfunsub)
                {


                    var Emps = db.EmployeePayroll
                        .Where(e => (item1.ReportingEmployee.Contains(e.Employee.Id)))
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.ReportingStructRights)
                        .Include(e => e.Employee.ReportingStructRights.Select(b => b.AccessRights.ActionName))
                        .Include(e => e.Employee.ReportingStructRights.Select(b => b.FuncModules))
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.DIARYSettlementClaim)
                        .Include(e => e.DIARYSettlementClaim.Select(b => b.LTCWFDetails))
                        .Include(e => e.DIARYSettlementClaim.Select(w => w.WFStatus))
                        .Include(e => e.DIARYSettlementClaim.Select(w => w.DIARYType))
                        .ToList();

                    foreach (var item in Emps)
                    {
                        if (item.DIARYSettlementClaim != null && item.DIARYSettlementClaim.Count() > 0)
                        {
                            var LvIds = UserManager.FilterDIARYSettlementClaim(item.DIARYSettlementClaim.OrderByDescending(e => e.DateOfAppl.Value.ToShortDateString()).ToList(),
                               Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                            if (LvIds.Count() > 0)
                            {
                                var Diarysettlementclaimreqdata = item.DIARYSettlementClaim.Where(e => LvIds.Contains(e.Id)).ToList();
                                foreach (var singleDiaryDetails in Diarysettlementclaimreqdata)
                                {
                                    if (singleDiaryDetails.LTCWFDetails != null)
                                    {

                                        var session = Session["auho"].ToString().ToUpper();
                                        var EmpR = item.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session && e.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                        .Select(e => e.AccessRights.IsClose).FirstOrDefault();
                                        //if (stts.WFStatus == 0)
                                        //{

                                        returndata.Add(new GetDIARYSettlementClaimClass1
                                        {
                                            RowData = new ChildGetDIARYSettlementReqClass
                                            {
                                                LvNewReq = singleDiaryDetails.Id.ToString(),
                                                EmpLVid = item.Id.ToString(),
                                                IsClose = EmpR.ToString(),
                                                LvHead_Id = "",
                                            },

                                            DIARYType = singleDiaryDetails.DIARYType.LookupVal.ToString(),
                                            SettlementAmount = singleDiaryDetails.SettlementAmt.ToString() != "0" ? singleDiaryDetails.SettlementAmt.ToString() : "0",
                                            ReqDate = singleDiaryDetails.DateOfAppl.Value.ToShortDateString(),
                                            AdvanceAmount = singleDiaryDetails.AdvAmt.ToString() != "0" ? singleDiaryDetails.AdvAmt.ToString() : "0",
                                            EmpCode = item.Employee.EmpCode,
                                            EmpName = item.Employee.EmpName.FullNameFML

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
        public class DIARYSettlementClaimRequestData
        {
            public int JourneyDetailsID { get; set; }
            public string JourneyDetailsFulldetails { get; set; }
            public int Travel_Hotel_Bookingid { get; set; }
            public string Travel_Hotel_Bookingdetails { get; set; }
            public string Status { get; set; }

            public string Emplvhead { get; set; }
            public int JourneyDetailsId { get; set; }
            public string JourneyDetails { get; set; }

            public int ReferencedocumentId { get; set; }
            public string Referencedocument { get; set; }
            public string Billno { get; set; }
            public string ReqDate { get; set; }

            public string Narration { get; set; }
            public string SanctionCode { get; set; }
            public string SanctionEmpname { get; set; }
            public string RecomendationCode { get; set; }
            public string RecomendationEmpname { get; set; }
            public double DIARYAdvanceClaim { get; set; }
            public double Claim_Amt { get; set; }

            public double Travel_Hotel_Booking { get; set; }
            public double NoOfDays { get; set; }

            public double AdvAmt { get; set; }
            public double SettlementAmt { get; set; }
            public double SanctionAmt { get; set; }
            public double Eligible_Amt { get; set; }
            public int DIARYType { get; set; }
            public bool TrClosed { get; set; }
            public bool TrReject { get; set; }
            public string SanctionComment { get; set; }
            public string ApporavalComment { get; set; }
            public string Wf { get; set; }
            public Int32 Id { get; set; }
            public Int32 Lvnewreq { get; set; }
            public double RatePerDay { get; set; }
            public string EmployeeName { get; set; }
            public string Remark { get; set; }
            public string Empcode { get; set; }
            public string Isclose { get; set; }
            public int EmployeeId { get; set; }
            public string Incharge { get; set; }
            public int DIARYAdvanceClaimId { get; set; }
            public string DIARYAdvanceClaimFulldetails { get; set; }


        }
        public ActionResult GetDIARYSettlementClaimData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);
                var WfStatus = new List<Int32>();
                var SanctionStatus = new List<Int32>();
                var ApporvalStatus = new List<Int32>();
                var RecomendationStatus = new List<Int32>();
                if (authority.ToUpper() == "SANCTION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "APPROVAL")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                    RecomendationStatus.Add(7);
                    RecomendationStatus.Add(8);
                }
                else if (authority.ToUpper() == "RECOMMENDATION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "MYSELF")
                {
                    SanctionStatus.Add(1);
                    SanctionStatus.Add(2);

                    ApporvalStatus.Add(3);
                    ApporvalStatus.Add(4);
                }
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var status = ids.Count > 0 ? ids[2] : null;
                var emplvId = ids.Count > 0 ? ids[1] : null;
                //var LvHeadId = ids.Count > 0 ? ids[3] : null;

                //var lvheadidint = Convert.ToInt32(LvHeadId);
                var EmpLvIdint = Convert.ToInt32(emplvId);

                var W = db.EmployeePayroll
                    .Include(e => e.TADASettlementClaim)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.DIARYSettlementClaim.Select(t => t.GeoStruct))
                    .Include(e => e.DIARYSettlementClaim.Select(t => t.FuncStruct))
                    .Include(e => e.DIARYSettlementClaim.Select(t => t.PayStruct))
                    .Include(e => e.DIARYSettlementClaim.Select(t => t.LTCWFDetails))
                    .Include(e => e.DIARYSettlementClaim.Select(t => t.WFStatus))
                    .Include(e => e.DIARYSettlementClaim.Select(t => t.DIARYType))
                    .Include(e => e.DIARYSettlementClaim.Select(t => t.DIARYAdvanceClaim))
                    //.Include(e => e.TADASettlementClaim.Select(t => t.JourneyDetails_Id))
                    .Include(e => e.DIARYSettlementClaim.Select(t => t.JourneyDetails))
                    .Include(e => e.DIARYSettlementClaim.Select(t => t.Travel_Hotel_Booking))
                    //.Include(e => e.TADASettlementClaim.Select(t => t.Travel_Hotel_Booking.Select(a => a.HotelEligibilityPolicy)))


                    .Where(e => e.Employee.Id == EmpLvIdint).SingleOrDefault();

                if (W.DIARYSettlementClaim.Count() > 0)
                {

                    var v = W.DIARYSettlementClaim.Where(e => e.Id == id).Select(s => new DIARYSettlementClaimRequestData
                    {
                        EmployeeId = W.Employee.Id,
                        EmployeeName = W.Employee.EmpCode + " " + W.Employee.EmpName.FullNameFML,
                        Lvnewreq = s.Id,
                        Empcode = W.Employee.EmpCode,

                        //Branch = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Location != null && W.Employee.GeoStruct.Location.LocationObj != null ? W.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString() : null,
                        //Department = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Department != null && W.Employee.GeoStruct.Department.DepartmentObj != null ? W.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString() : null,
                        //Designation = W.Employee.FuncStruct != null && W.Employee.FuncStruct.Job != null ? W.Employee.FuncStruct.Job.Name : null,
                        //Travel_Hotel_Booking = s.Travel_Hotel_Booking != null && s.Travel_Hotel_Booking.Count > 0 ? s.Travel_Hotel_Booking.Select(m => m.Id).FirstOrDefault() : 0,
                        DIARYAdvanceClaimId = s.DIARYAdvanceClaim != null ? s.DIARYAdvanceClaim.Id : 0,
                        DIARYAdvanceClaimFulldetails = s.DIARYAdvanceClaim != null ? s.DIARYAdvanceClaim.FullDetails.ToString() : "",

                        JourneyDetailsID = s.JourneyDetails != null ? s.JourneyDetails.Id : 0,
                        JourneyDetailsFulldetails = s.JourneyDetails != null ? s.JourneyDetails.FullDetails.ToString() : "",
                        Travel_Hotel_Bookingid = s.Travel_Hotel_Booking != null && s.Travel_Hotel_Booking.Count() > 0 ? s.Travel_Hotel_Booking.Select(i => i.Id).FirstOrDefault() : 0,
                        Travel_Hotel_Bookingdetails = s.Travel_Hotel_Booking != null && s.Travel_Hotel_Booking.Count() > 0 ? s.Travel_Hotel_Booking.Select(h => h.HotelName + ", StartDate :" + h.StartDate.Value.ToShortDateString()).FirstOrDefault() : null,
                        //TADAType = s.TADAType,

                        DIARYType = s.DIARYType != null ? s.DIARYType.Id : 0,

                        Eligible_Amt = s.Eligible_Amt,
                        SanctionAmt = s.SanctionAmt,
                        SettlementAmt = s.SettlementAmt,
                        AdvAmt = s.AdvAmt,
                        Claim_Amt = s.Claim_Amt,
                        Remark = s.Remark,
                        //TADAAdvanceClaim = s.TADAAdvanceClaim,
                        //Travel_Hotel_Booking = s.Travel_Hotel_Booking,
                        NoOfDays = s.NoOfDays,
                        ReqDate = s.DateOfAppl != null ? s.DateOfAppl.Value.ToShortDateString() : null,

                        //Status = status,


                        Isclose = status.ToString(),
                        TrClosed = s.TrClosed,
                        SanctionCode = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                        SanctionComment = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                        ApporavalComment = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                        Wf = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).LastOrDefault() : null,
                        RecomendationCode = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                        RecomendationEmpname = s.LTCWFDetails != null && s.LTCWFDetails.Count > 0 ? s.LTCWFDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                        // Incharge = s.Incharge != null ? s.Incharge.EmpCode + ' ' + s.Incharge.EmpName.FullDetails.ToString() : null
                    }).ToList();

                    TempData["DIARYsettleClaim"] = v;


                }
                var DIARYsettleClaimreturn = TempData["DIARYsettleClaim"];
                return Json(DIARYsettleClaimreturn, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Update_DIARYSettlementClaim(DIARYSettlementClaim HbReq, FormCollection form, String data)
        {

            string authority = form["authority"] == null ? null : form["authority"];
            var isClose = form["isClose"] == null ? null : form["isClose"];
            if (authority == null && isClose == null)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
            var ids = Utility.StringIdsToListString(data);
            var Hbnewreqid = Convert.ToInt32(ids[0]);
            var EmpPayrollId = Convert.ToInt32(ids[1]);
            string Sanction = form["Sanction"];
            string ReasonSanction = form["ReasonSanction"];
            string HR = form["HR"] == null ? null : form["HR"];
            string MySelf = form["MySelf"] == null ? "false" : form["MySelf"];
            string ReasonMySelf = form["ReasonMySelf"] == null ? null : form["ReasonMySelf"];
            string Approval = form["Approval"];
            string ReasonApproval = form["ReasonApproval"];
            string ReasonHr = form["ReasonHr"] == null ? null : form["ReasonHr"];
            string Recomendation = form["Recomendation"];
            string ReasonRecomendation = form["ReasonRecomendation"];
            bool SanctionRejected = false;
            bool HrRejected = false;
            string SanInchargeid = form["SanIncharge_id"];
            string RecInchargeid = form["RecIncharge_id"];
            string AppInchargeid = form["AppIncharge_id"];



            //bool self = false;
            using (DataBaseContext db = new DataBaseContext())
            {
                //  access right no of levaefrom days and to days check start
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";

                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                               .Include(e => e.LookupValues)
                               .Where(e => e.Code == "601").AsNoTracking().FirstOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();


                }

                var query = db.EmployeePayroll.Where(e => e.Id == EmpPayrollId)
                    .Include(e => e.DIARYSettlementClaim)
                    .Include(e => e.Employee.EmpName)
                    //.Include(e => e.TADASettlementClaim.Select(t => t.GeoStruct))
                    //.Include(e => e.TADASettlementClaim.Select(t => t.FuncStruct))
                    //.Include(e => e.TADASettlementClaim.Select(t => t.PayStruct))
                    .Include(e => e.DIARYSettlementClaim.Select(t => t.LTCWFDetails))
                    .Include(e => e.DIARYSettlementClaim.Select(t => t.WFStatus))
                    .Include(e => e.DIARYSettlementClaim.Select(t => t.DIARYType))
                    .Include(e => e.DIARYSettlementClaim.Select(t => t.DIARYAdvanceClaim))
                    //.Include(e => e.TADASettlementClaim.Select(t => t.JourneyDetails_Id))
                    .Include(e => e.DIARYSettlementClaim.Select(t => t.JourneyDetails))
                    .Include(e => e.DIARYSettlementClaim.Select(t => t.Travel_Hotel_Booking))
                    //.Include(e => e.TADASettlementClaim.Select(t => t.Travel_Hotel_Booking.Select(a => a.HotelEligibilityPolicy)))
                    .FirstOrDefault();



                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }

                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);


                bool TrClosed = false;

                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        var DIARYSettlementClaimList = query.DIARYSettlementClaim.Where(e => e.Id == Hbnewreqid).ToList();
                        //if someone reject lv
                        foreach (var Hbitems in DIARYSettlementClaimList)
                        {
                            List<FunctAllWFDetails> oFunctWFDetails_List = new List<FunctAllWFDetails>();
                            FunctAllWFDetails objFunctAllWFDetails = new FunctAllWFDetails();
                            if (authority.ToUpper() == "MYSELF")
                            {
                                TrClosed = false;
                            }
                            if (authority.ToUpper() == "SANCTION")
                            {

                                if (Sanction == null)
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Select the Sanction Status" }, JsonRequestBehavior.AllowGet);
                                }
                                if (ReasonSanction == "")
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Enter the Reason" }, JsonRequestBehavior.AllowGet);
                                }
                                if (Convert.ToBoolean(Sanction) == true)
                                {
                                    //sanction yes -1

                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 1,
                                        Comments = ReasonSanction,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };

                                    oFunctWFDetails_List.Add(objFunctAllWFDetails);

                                    Hbitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                    Hbitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").SingleOrDefault();


                                }
                                else if (Convert.ToBoolean(Sanction) == false)
                                {

                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 2,
                                        Comments = ReasonSanction,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                    oFunctWFDetails_List.Add(objFunctAllWFDetails);


                                    SanctionRejected = true;
                                }
                            }
                            else if (authority.ToUpper() == "APPROVAL")//Hr
                            {
                                if (Approval == null)
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Select the Approval Status" }, JsonRequestBehavior.AllowGet);
                                }
                                if (ReasonApproval == "")
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Enter the Reason" }, JsonRequestBehavior.AllowGet);
                                }
                                if (Convert.ToBoolean(Approval) == true)
                                {
                                    //approval yes-3
                                    //var CheckAllreadySanction = BAAppTarget.Where(e => e.BA_WorkFlow.Any(r => r.WFStatus == 3)).ToList();
                                    //if (CheckAllreadySanction.Count() > 0)
                                    //{
                                    //    return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Approved....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                    //}
                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 3,
                                        Comments = ReasonApproval,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };
                                    oFunctWFDetails_List.Add(objFunctAllWFDetails);

                                    Hbitems.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                    Hbitems.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").SingleOrDefault();
                                }
                                else if (Convert.ToBoolean(Approval) == false)
                                {
                                    //approval no-4
                                    //var LvWFDetails = new LvWFDetails
                                    //{
                                    //    WFStatus = 4,
                                    //    Comments = ReasonApproval,
                                    //    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    //};
                                    //qurey.LvWFDetails.Add(LvWFDetails);

                                    //qurey.LvWFDetails.Add(LvWFDetails);
                                    //var CheckAllreadySanction = BAAppTarget.Where(e => e.BA_WorkFlow.Any(r => r.WFStatus == 4)).ToList();
                                    //if (CheckAllreadySanction.Count() > 0)
                                    //{
                                    //    return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Rejected....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                    //}
                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 4,
                                        Comments = ReasonApproval,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                    oFunctWFDetails_List.Add(objFunctAllWFDetails);
                                    //  qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                                    TrClosed = true;
                                    HrRejected = true;
                                }
                            }
                            else if (authority.ToUpper() == "RECOMMENDATION")
                            {

                                if (Recomendation == null)
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Recomendation Status" }, JsonRequestBehavior.AllowGet);
                                }
                                if (ReasonRecomendation == "")
                                {
                                    return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason" }, JsonRequestBehavior.AllowGet);
                                }
                                if (Convert.ToBoolean(Recomendation) == true)
                                {
                                    //Recomendation yes -7
                                    var CheckAllreadyRecomendation = DIARYSettlementClaimList.Where(e => e.LTCWFDetails.Any(r => r.WFStatus == 7)).ToList();
                                    if (CheckAllreadyRecomendation.Count() > 0)
                                    {
                                        return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Recomendation....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 7,
                                        Comments = ReasonRecomendation,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                    };
                                    //qurey.BA_WorkFlow.Add(AppWFDetails);

                                }
                                else if (Convert.ToBoolean(Recomendation) == false)
                                {
                                    //Recommendation no -8

                                    var CheckAllreadyRecomendation = DIARYSettlementClaimList.Where(e => e.LTCWFDetails.Any(r => r.WFStatus == 8)).ToList();
                                    if (CheckAllreadyRecomendation.Count() > 0)
                                    {
                                        return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Rejected....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    objFunctAllWFDetails = new FunctAllWFDetails
                                    {
                                        WFStatus = 8,
                                        Comments = ReasonRecomendation,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                    oFunctWFDetails_List.Add(objFunctAllWFDetails);

                                    //   qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                                    TrClosed = true;
                                    SanctionRejected = true;
                                }
                            }
                            if (Hbitems.LTCWFDetails != null)
                            {
                                oFunctWFDetails_List.AddRange(Hbitems.LTCWFDetails);
                            }

                            Hbitems.SettlementAmt = HbReq.SettlementAmt;
                            Hbitems.SanctionAmt = HbReq.SanctionAmt;
                            Hbitems.LTCWFDetails = oFunctWFDetails_List;
                            db.DIARYSettlementClaim.Attach(Hbitems);
                            db.Entry(Hbitems).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            //db.Entry(x).State = System.Data.Entity.EntityState.Detached;   
                        }
                        //qurey.ToList().ForEach(x =>/
                        //{

                        //});

                        //db.BA_TargetT.Attach(qurey);
                        //db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                        //db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
                    return Json(new Utility.JsonClass { status = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public class GetRawDataGrid
        {
            public string PunchDate { get; set; }
            public string PunchInLocation { get; set; }
            public string PunchOutLocation { get; set; }
            public string InTime { get; set; }
            public string OutTime { get; set; }

        }
        public ActionResult GetDiarySettlementGrid(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<GetRawDataGrid> return_data = new List<GetRawDataGrid>();
                var ids = Utility.StringIdsToListString(data);
                int Dirsetclmid = Convert.ToInt32(ids[0]);
                int EmployeeId = Convert.ToInt32(ids[1]);
                var item = db.EmployeePayroll
               .Include(e => e.Employee)
               .Include(e => e.Employee.EmpName)
               .Include(e => e.DIARYSettlementClaim)
               .Include(e => e.DIARYSettlementClaim.Select(b => b.LTCWFDetails))
               .Include(e => e.DIARYSettlementClaim.Select(w => w.WFStatus))
               .Include(e => e.DIARYSettlementClaim.Select(w => w.DIARYType))
               .Include(e => e.DIARYSettlementClaim.Select(r => r.JourneyDetails)).Where(e => e.Employee.Id == EmployeeId).SingleOrDefault();

                if (item.DIARYSettlementClaim != null && item.DIARYSettlementClaim.Count() > 0)
                {
                    var collection = item.DIARYSettlementClaim.Where(e => e.Id == Dirsetclmid).ToList();
                    foreach (var ODiarysettlementclaim in collection)
                    {
                        DateTime _min = new DateTime();
                        DateTime _max = new DateTime();
                        DateTime iJourneyStart = Convert.ToDateTime(ODiarysettlementclaim.JourneyDetails.JourneyStart);
                        DateTime iJourneyEnd = Convert.ToDateTime(ODiarysettlementclaim.JourneyDetails.JourneyEnd);
                        string MinSWtimeLocation = string.Empty;
                        string MaxSWtimeLocation = string.Empty;
                        for (DateTime mTempDate = iJourneyStart; mTempDate <= iJourneyEnd; mTempDate = mTempDate.AddDays(1))
                        {

                            var getRawData = db.RawData.Select(b => new
                            {
                                oCrdCode = b.CardCode,
                                oSwipeDate = b.SwipeDate,
                                oswipetime = b.SwipeTime,
                                oUnitcode = b.UnitCode.ToString(),
                                oInputType = b.InputType.LookupVal.ToUpper().ToString(),
                                oNarration = b.Narration,

                            }).Where(e => (e.oSwipeDate == mTempDate) && e.oCrdCode == item.Employee.CardCode).ToList();
                            if (getRawData.Count() != 0)
                            {
                                _min = Convert.ToDateTime(getRawData.Min(a => a.oswipetime));
                                _max = Convert.ToDateTime(getRawData.Max(a => a.oswipetime));
                                var getRawDataFirstunitcode = getRawData.Where(x => x.oswipetime == _min).FirstOrDefault();
                                var getRawDataLastunitcode = getRawData.Where(x => x.oswipetime == _max).FirstOrDefault();

                                if (!String.IsNullOrEmpty(getRawDataFirstunitcode.oUnitcode))
                                {
                                    var getUnitIdAssignment = db.UnitIdAssignment.Select(z => new
                                    {
                                        UnitId = z.UnitId,
                                        oGeoStruct = z.GeoStruct.Select(y => new
                                        {
                                            oLocDescMin = y.Location.LocationObj.LocDesc,
                                            oDeptNm = y.Department.DepartmentObj.DeptDesc,
                                        }).FirstOrDefault(),

                                    }).Where(e => e.UnitId == getRawDataFirstunitcode.oUnitcode).FirstOrDefault();

                                    MinSWtimeLocation = getUnitIdAssignment != null && getUnitIdAssignment.oGeoStruct != null ? getUnitIdAssignment.oGeoStruct.oLocDescMin.ToString() : "";
                                }

                                if (!String.IsNullOrEmpty(getRawDataLastunitcode.oUnitcode))
                                {
                                    var getUnitIdAssignment = db.UnitIdAssignment.Select(z => new
                                    {
                                        UnitId = z.UnitId,
                                        oGeoStruct = z.GeoStruct.Select(y => new
                                        {
                                            oLocDescMin = y.Location.LocationObj.LocDesc,
                                            oDeptNm = y.Department.DepartmentObj.DeptDesc,
                                        }).FirstOrDefault(),

                                    }).Where(e => e.UnitId == getRawDataLastunitcode.oUnitcode).FirstOrDefault();

                                    MaxSWtimeLocation = getUnitIdAssignment != null && getUnitIdAssignment.oGeoStruct != null ? getUnitIdAssignment.oGeoStruct.oLocDescMin.ToString() : "";
                                }

                                return_data.Add(new GetRawDataGrid
                                {
                                    InTime = _min.ToShortTimeString(),
                                    OutTime = _max.ToShortTimeString(),
                                    PunchInLocation = MinSWtimeLocation,
                                    PunchOutLocation = MaxSWtimeLocation,
                                    PunchDate = mTempDate.ToShortDateString()
                                });
                            }

                        }

                    }


                }



                return Json(return_data, JsonRequestBehavior.AllowGet);

            }

        }

        public class returnEditClass1
        {
            public Array JourneyObject_Id { get; set; }
            public Array JourneyObjectFullDetails { get; set; }
            public Array Family_Id { get; set; }
            public Array FamilyFullDetails { get; set; }
            public Array MisExpenseObject_Id { get; set; }
            public Array MisExpenseObjectFullDetails { get; set; }
            public Array TravelHotelBooking_Id { get; set; }
            public Array TravelHotelBookingFullDetails { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                //var empltcblockdata = db.EmpLTCBlockT.Select(e => e.LTCSettlementClaim.Where(t => t.Id == data)).ToList();

                var q1 = db.JourneyDetails.Include(e => e.JourneyObject).Include(e => e.TravelModeEligibilityPolicy)
                    .Include(e => e.TravelModeEligibilityPolicy)
                    .Where(e => e.Id == data).Select(x => new
                    {
                        PayMonth = x.JourneyStart.Value.Day + "/" + x.JourneyStart.Value.Year,
                        //MonthStart = "01/" + x.JourneyStart.Value.Day + "/" + x.JourneyStart.Value.Year,
                        //MonthEnd = LastDayOfMonth("01/" + x.JourneyStart.Value.Day + "/" + x.JourneyStart.Value.Year),
                        JourneyStart = x.JourneyStart,
                        JourneyEnd = x.JourneyEnd,
                        NoOfDays = x.NoOfDays,
                        // TravelModeEligibilityPolicy=x.TravelModeEligibilityPolicy.TA_TM_Elligibilty_Code,
                        TravelModeEligibilityPolicy = x.TravelModeEligibilityPolicy == null ? "0" : x.TravelModeEligibilityPolicy.Id.ToString(),
                        DAElligibleAmt = x.DAElligibleAmt,
                        DAClaimAmt = x.DAClaimAmt,
                        DASettleAmt = x.DASettleAmt,
                        TAElligibleAmt = x.TAElligibleAmt,
                        TAClaimAmt = x.TAClaimAmt,
                        TASettleAmt = x.TASettleAmt,
                        MisExpenseElligibleAmt = x.MisExpenseElligibleAmt,
                        MisExpenseClaimAmt = x.MisExpenseClaimAmt,
                        MisExpenseSettleAmt = x.MisExpenseSettleAmt,
                        HotelElligibleAmt = x.HotelElligibleAmt,
                        HotelClaimAmt = x.HotelClaimAmt,
                        HotelSettleAmt = x.HotelSettleAmt,
                        TotFamilyMembers = x.TotFamilyMembers,

                    }).SingleOrDefault();

                List<returnEditClass1> oreturnEditClass = new List<returnEditClass1>();

                string MonthStart = ("01/" + (q1.JourneyStart.Value.Day.ToString().Length != 1 ? q1.JourneyStart.Value.Day.ToString() : "0" + q1.JourneyStart.Value.Day.ToString()) + "/" + q1.JourneyStart.Value.Year).ToString();
                string MonthEnd = LastDayOfMonth(MonthStart).ToShortDateString();

                var k = db.JourneyDetails.Include(e => e.JourneyObject)
                   .Where(e => e.Id == data && e.JourneyObject.Count > 0).ToList();
                foreach (var e in k)
                {
                    oreturnEditClass.Add(new returnEditClass1
                    {
                        JourneyObject_Id = e.JourneyObject.Select(a => a.Id.ToString()).ToArray(),
                        JourneyObjectFullDetails = e.JourneyObject.Select(a => "JourneyDistance:" + a.JourneyDist + ", " + "PlaceFrom:" + a.PlaceFrom + ", " + "PlaceTo:" + a.PlaceTo + ", " + "FromDate:" + a.FromDate.Value.ToString("MM/dd/yyyy hh:mm") + ", " + "ToDate:" + a.ToDate.Value.ToString("MM/dd/yyyy hh:mm")).ToArray()
                    });
                }

                var y = db.JourneyDetails.Include(e => e.FamilyDetails)
                      .Where(e => e.Id == data && e.FamilyDetails.Count > 0).ToList();
                foreach (var e in k)
                {
                    if (e.FamilyDetails != null)
                    {
                        oreturnEditClass.Add(new returnEditClass1
                        {
                            Family_Id = e.FamilyDetails.Select(a => a.Id.ToString()).ToArray(),
                            FamilyFullDetails = e.FamilyDetails.Select(a => a.FullDetails).ToArray()
                        });
                    }

                }

                var m = db.JourneyDetails.Include(e => e.MisExpenseObject)
                         .Where(e => e.Id == data && e.MisExpenseObject.Count > 0).ToList();
                foreach (var e in k)
                {
                    if (e.MisExpenseObject != null)
                    {
                        oreturnEditClass.Add(new returnEditClass1
                        {
                            MisExpenseObject_Id = e.MisExpenseObject.Select(a => a.Id.ToString()).ToArray(),
                            MisExpenseObjectFullDetails = e.MisExpenseObject.Select(a => a.MisExpenseDate + " " + a.MisExpenseElligibleAmt + " " + a.MisExpenseClaimAmt + " " + a.MisExpenseSettleAmt).ToArray()
                        });
                    }

                }

                var t = db.JourneyDetails.Include(e => e.Travel_Hotel_Booking)
                             .Where(e => e.Id == data && e.Travel_Hotel_Booking.Count > 0).ToList();
                foreach (var e in k)
                {
                    if (e.Travel_Hotel_Booking != null)
                    {
                        oreturnEditClass.Add(new returnEditClass1
                        {
                            TravelHotelBooking_Id = e.Travel_Hotel_Booking.Select(a => a.Id.ToString()).ToArray(),
                            TravelHotelBookingFullDetails = e.Travel_Hotel_Booking.Select(a => a.StartDate + " " + a.EndDate + " " + a.HotelName + " " + a.BillAmount).ToArray()
                        });
                    }

                }


                return Json(new Object[] { q1, oreturnEditClass, "", MonthStart, MonthEnd, JsonRequestBehavior.AllowGet });

                //return Json(new Object[] { Q, return_data, return_dataDoc, JsonRequestBehavior.AllowGet });
                //return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }

        public static DateTime LastDayOfMonth(string dt)
        {
            DateTime dtnw = Convert.ToDateTime(dt);
            DateTime ss = new DateTime(dtnw.Year, dtnw.Month, 1); // Creating a new DateTime object representing the first day of the input month and year
            return ss.AddMonths(1).AddDays(-1); // Returning the last day of the input month by adding one month to the first day and then subtracting one day
        }

        public ActionResult TravelModeEligibilityPolicylistd(string data, string data2, string DiaryType)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //SelectList s = (SelectList)null;
                var selected = data2 != "" ? Convert.ToInt32(data2) : 0;
                List<TravelModeEligibilityPolicy> TransMode = new List<TravelModeEligibilityPolicy>();
                if (data != "" && data != null)
                {
                    int empid = Convert.ToInt32(data);
                    int DiaryTypeId = Convert.ToInt32(DiaryType);
                    var EmpLTCStruct_Details = db.EmployeeLTCStruct
                        .Include(e => e.EmployeeLTCStructDetails)
                        .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula))
                        .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula.TravelModeEligibilityPolicy))
                        .Where(e => e.EmployeePayroll.Employee_Id == empid && e.EndDate == null).FirstOrDefault();

                    var travelmodlist = EmpLTCStruct_Details.EmployeeLTCStructDetails.Select(r => r.LTCFormula.TravelModeEligibilityPolicy).ToList();


                    if (travelmodlist.Count() > 0)
                    {
                        foreach (var item in travelmodlist)
                        {
                            if (item.FirstOrDefault() != null)
                            {
                                foreach (var item1 in item)
                                {
                                    var BMSModuleAssign = db.BMSModuleTypePolicyAssignment.Include(e => e.TravelModeEligibilityPolicy).Where(e => e.PolicyType_Id == DiaryTypeId).FirstOrDefault();
                                    if (BMSModuleAssign != null && BMSModuleAssign.TravelModeEligibilityPolicy.Count() > 0)
                                    {
                                        foreach (var item2 in BMSModuleAssign.TravelModeEligibilityPolicy)
                                        {
                                            if (item2.Id == item1.Id)
                                            {
                                                TransMode.Add(item1);
                                            }

                                        }
                                    }

                                }
                            }

                        }
                    }

                }
                SelectList s = new SelectList(TransMode, "Id", "TA_TM_Elligibilty_Code", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
    }

    public class CASHSettings
    {
        private string CompCode;
        private string Type;
        private string InVisibleControlName;
        private string DisbleControlName;
        private string VisibleControlName;

        public CASHSettings(IDictionary<string, string> settinigs)
        {
            this.Type = settinigs.First(x => x.Key.Equals("Type")).Value;
            this.CompCode = settinigs.First(x => x.Key.Equals("CompCode")).Value;
            this.InVisibleControlName = settinigs.First(x => x.Key.Equals("InVisibleControlName")).Value;
            this.DisbleControlName = settinigs.First(x => x.Key.Equals("DisbleControlName")).Value;
            this.VisibleControlName = settinigs.First(x => x.Key.Equals("VisibleControlName")).Value;
        }

        public string CCode { get { return CompCode; } }
        public string DType { get { return Type; } }
        public string DInVisibleControlName { get { return InVisibleControlName; } }
        public string DDisbleControlName { get { return DisbleControlName; } }
        public string DVisibleControlName { get { return VisibleControlName; } }

    }

    public class WOSettings
    {
        private string CompCode;
        private string Type;
        private string InVisibleControlName;
        private string DisbleControlName;
        private string VisibleControlName;

        public WOSettings(IDictionary<string, string> settinigs)
        {
            this.Type = settinigs.First(x => x.Key.Equals("Type")).Value;
            this.CompCode = settinigs.First(x => x.Key.Equals("CompCode")).Value;
            this.InVisibleControlName = settinigs.First(x => x.Key.Equals("InVisibleControlName")).Value;
            this.DisbleControlName = settinigs.First(x => x.Key.Equals("DisbleControlName")).Value;
            this.VisibleControlName = settinigs.First(x => x.Key.Equals("VisibleControlName")).Value;
        }

        public string CCode { get { return CompCode; } }
        public string DType { get { return Type; } }
        public string DInVisibleControlName { get { return InVisibleControlName; } }
        public string DDisbleControlName { get { return DisbleControlName; } }
        public string DVisibleControlName { get { return VisibleControlName; } }

    }

    public class WOWORKSettings
    {
        private string CompCode;
        private string Type;
        private string InVisibleControlName;
        private string DisbleControlName;
        private string VisibleControlName;

        public WOWORKSettings(IDictionary<string, string> settinigs)
        {
            this.Type = settinigs.First(x => x.Key.Equals("Type")).Value;
            this.CompCode = settinigs.First(x => x.Key.Equals("CompCode")).Value;
            this.InVisibleControlName = settinigs.First(x => x.Key.Equals("InVisibleControlName")).Value;
            this.DisbleControlName = settinigs.First(x => x.Key.Equals("DisbleControlName")).Value;
            this.VisibleControlName = settinigs.First(x => x.Key.Equals("VisibleControlName")).Value;
        }

        public string CCode { get { return CompCode; } }
        public string DType { get { return Type; } }
        public string DInVisibleControlName { get { return InVisibleControlName; } }
        public string DDisbleControlName { get { return DisbleControlName; } }
        public string DVisibleControlName { get { return VisibleControlName; } }

    }

    public class OFFICEWORKSettings
    {
        private string CompCode;
        private string Type;
        private string InVisibleControlName;
        private string DisbleControlName;
        private string VisibleControlName;

        public OFFICEWORKSettings(IDictionary<string, string> settinigs)
        {
            this.Type = settinigs.First(x => x.Key.Equals("Type")).Value;
            this.CompCode = settinigs.First(x => x.Key.Equals("CompCode")).Value;
            this.InVisibleControlName = settinigs.First(x => x.Key.Equals("InVisibleControlName")).Value;
            this.DisbleControlName = settinigs.First(x => x.Key.Equals("DisbleControlName")).Value;
            this.VisibleControlName = settinigs.First(x => x.Key.Equals("VisibleControlName")).Value;
        }

        public string CCode { get { return CompCode; } }
        public string DType { get { return Type; } }
        public string DInVisibleControlName { get { return InVisibleControlName; } }
        public string DDisbleControlName { get { return DisbleControlName; } }
        public string DVisibleControlName { get { return VisibleControlName; } }

    }

    public class OTHERSettings
    {
        private string CompCode;
        private string Type;
        private string InVisibleControlName;
        private string DisbleControlName;
        private string VisibleControlName;

        public OTHERSettings(IDictionary<string, string> settinigs)
        {
            this.Type = settinigs.First(x => x.Key.Equals("Type")).Value;
            this.CompCode = settinigs.First(x => x.Key.Equals("CompCode")).Value;
            this.InVisibleControlName = settinigs.First(x => x.Key.Equals("InVisibleControlName")).Value;
            this.DisbleControlName = settinigs.First(x => x.Key.Equals("DisbleControlName")).Value;
            this.VisibleControlName = settinigs.First(x => x.Key.Equals("VisibleControlName")).Value;
        }

        public string CCode { get { return CompCode; } }
        public string DType { get { return Type; } }
        public string DInVisibleControlName { get { return InVisibleControlName; } }
        public string DDisbleControlName { get { return DisbleControlName; } }
        public string DVisibleControlName { get { return VisibleControlName; } }

    }
}



















