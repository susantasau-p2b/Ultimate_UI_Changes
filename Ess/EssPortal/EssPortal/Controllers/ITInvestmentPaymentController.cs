using EssPortal.App_Start;
using EssPortal.Models;
using EssPortal.Security;
using P2b.Global;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Data.Entity;
using Leave;
using System.IO;
using System.Web;
using System.Configuration;

namespace EssPortal.Controllers
{
    public class ITInvestmentPaymentController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/ITInvestmentPayment/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/_ITInvestmentPaymentView.cshtml");

        }
        public ActionResult Uploadpartial()
        {
            return View("~/Views/Shared/_ITInvestmentUploadView.cshtml");
        }
        public ActionResult GetITInvestment(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITInvestment.Where(e => e.SalaryHead == null).ToList();
                var selected = (Object)null;
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = Convert.ToInt32(data2);
                }
                SelectList s = new SelectList(fall, "Id", "ITInvestmentName", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetItsection(Int32 Id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection.Include(e => e.ITInvestments).Include(e => e.ITSectionListType).Include(e => e.ITSectionList).Where(e => e.ITInvestments.Any(a => a.Id == Id)).SingleOrDefault();
                if (fall != null)
                {
                    var returndata = new
                    {
                        Id = fall.Id,
                        ITSection = fall.ITSectionList != null ? fall.ITSectionList : null,
                        Status = fall.ITSectionListType != null ? fall.ITSectionListType : null,
                    };
                    return Json(returndata, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
        }
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        public ActionResult EditSave(ITInvestmentPayment ITP, string forwardadtad, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            var Emp = Convert.ToInt32(SessionManager.EmpId);
            string[] values = (forwardadtad.Split(new string[] { "," }, StringSplitOptions.None));
            int data = Convert.ToInt32(values[0]);
            string ITInvestmentslist = form["ITInvestment_drop"] == null ? null : form["ITInvestment_drop"];
            string FinancialYearList = form["FinancialYear"] == null ? null : form["FinancialYear"];
            string LoanAdvHeadlist = form["LoanAdvanceHeadlist"] == null ? null : form["LoanAdvanceHeadlist"];
            string ItSection = form["ItSection_v"] == null ? null : form["ItSection_v"];
            string ITSubInvestmentPaymentlist = form["ITSubInvestmentPaymentlist"] == null ? null : form["ITSubInvestmentPaymentlist"];
            string InvestmentDate = form["InvestmentDate"] == "" ? null : form["InvestmentDate"];
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.ITInvestmentPayment
                            .Include(e => e.ITInvestment)
                            .Include(e => e.ITSubInvestmentPayment)
                            .Include(e => e.ITSubInvestmentPayment.Select(r => r.ITSubInvestment))
                            .Where(e => e.Id == data).SingleOrDefault();

                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {

                            db_data.Id = data;
                            db_data.DeclaredInvestment = ITP.DeclaredInvestment;
                            db_data.Narration = ITP.Narration;
                            db_data.DBTrack = new DBTrack
                            {
                                CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                                CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        ts.Complete();
                    }
                    Msg.Add("Record Updated");
                    return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create(ITInvestmentPayment ITP, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                string ITInvestmentslist = form["ITInvestment_drop"] == null ? null : form["ITInvestment_drop"];
                string FinancialYearList = form["FinancialYear"] == null ? null : form["FinancialYear"];
                string LoanAdvHeadlist = form["LoanAdvanceHeadlist"] == null ? null : form["LoanAdvanceHeadlist"];
                string ItSection = form["ItSection"] == null ? null : form["ItSection"];
                string ITSubInvestmentPaymentlist = form["ITSubInvestmentPaymentlist"] == null ? null : form["ITSubInvestmentPaymentlist"];
                string InvestmentDate = form["InvestmentDate"] == "" ? null : form["InvestmentDate"];


                if (InvestmentDate != null)
                {
                    var date = Convert.ToDateTime(InvestmentDate);
                    var value = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.FromDate <= date && e.ToDate >= date).SingleOrDefault();
                    if (value == null)
                    {
                        return Json(new { status = true, valid = true, responseText = "InvestmentDate should be between Financial year..!" }, JsonRequestBehavior.AllowGet);
                    }

                }
                string Narration = form["Narration"] == null ? null : form["Narration"];
                List<Int32> ids = new List<Int32>();
                List<Int32> Empids = new List<Int32>();
                try
                {
                    if (ITInvestmentslist != null && ITInvestmentslist != "")
                    {
                        var value = db.ITInvestment.Find(int.Parse(ITInvestmentslist));
                        ITP.ITInvestment = value;
                    }
                    else
                    {
                        return Json(new { status = true, valid = true, responseText = "Define ITInvestment..!" }, JsonRequestBehavior.AllowGet);

                    }
                    List<ITSubInvestmentPayment> ObjITSubInvestmentPayment = new List<ITSubInvestmentPayment>();
                    if (ITSubInvestmentPaymentlist != null && ITSubInvestmentPaymentlist != "")
                    {
                        ids = one_ids(ITSubInvestmentPaymentlist);
                        foreach (var ca in ids)
                        {
                            var value = db.ITSubInvestmentPayment.Find(ca);
                            ObjITSubInvestmentPayment.Add(value);
                            ITP.ITSubInvestmentPayment = ObjITSubInvestmentPayment;
                        }
                    }
                    //  else
                    //{
                    //    return Json(new { status = true, valid = true, responseText = "Define ITSubInvestmentPayment..!" }, JsonRequestBehavior.AllowGet);

                    //}
                    if (ItSection != null)
                    {
                        var value = db.ITSection.Find(int.Parse(ItSection));
                        ITP.ITSection = value;


                    }
                    else
                    {
                        return Json(new { status = true, valid = true, responseText = "Define ItSection..!" }, JsonRequestBehavior.AllowGet);

                    }
                    if (FinancialYearList != null && FinancialYearList != "")
                    {
                        var value = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "FINANCIALYEAR").SingleOrDefault();
                        ITP.FinancialYear = value;
                    }
                    if (Emp != 0)
                    {
                        Empids.Add(Emp);
                    }
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    ITP.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                    ITP.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "0").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "0").SingleOrDefault();
                    //bhagesh added code start 12-08-2019
                    LvWFDetails oLvWFDetails = new LvWFDetails
                    {
                        WFStatus = 0,
                        Comments = Narration,
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };
                    List<LvWFDetails> oLvWFDetails_List = new List<LvWFDetails>();
                    oLvWFDetails_List.Add(oLvWFDetails);
                    //bhagesh code end  12-08-2019
                    ITInvestmentPayment ObjITP = new ITInvestmentPayment();
                    {
                        ObjITP.ActualInvestment = ITP.ActualInvestment;
                        ObjITP.DeclaredInvestment = ITP.DeclaredInvestment;
                        ObjITP.FinancialYear = ITP.FinancialYear;
                        ObjITP.InvestmentDate = ITP.InvestmentDate;
                        ObjITP.ITInvestment = ITP.ITInvestment;
                        ObjITP.ITSection = ITP.ITSection;
                        ObjITP.ITSubInvestmentPayment = ITP.ITSubInvestmentPayment;
                        ObjITP.DBTrack = ITP.DBTrack;
                        ObjITP.Narration = Narration;
                        ObjITP.InputMethod = 1;//apply through ess
                        ObjITP.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "0").FirstOrDefault(); //db.LookupValue.Where(e => e.LookupVal == "0").SingleOrDefault();
                        ObjITP.WFDetails = oLvWFDetails_List;
                    }


                    foreach (var i in Empids)
                    {
                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                    .Where(r => r.Id == i).SingleOrDefault();

                        OEmployeePayroll
                        = db.EmployeePayroll
                      .Where(e => e.Employee.Id == i).SingleOrDefault();

                        using (TransactionScope ts = new TransactionScope())
                        {
                            try
                            {
                                db.ITInvestmentPayment.Add(ObjITP);
                                db.SaveChanges();
                                List<ITInvestmentPayment> OFAT = new List<ITInvestmentPayment>();
                                OFAT.Add(db.ITInvestmentPayment.Find(ObjITP.Id));

                                if (OEmployeePayroll == null)
                                {
                                    EmployeePayroll OTEP = new EmployeePayroll()
                                    {
                                        Employee = db.Employee.Find(OEmployee.Id),
                                        ITInvestmentPayment = OFAT,
                                        DBTrack = ITP.DBTrack

                                    };


                                    db.EmployeePayroll.Add(OTEP);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                    aa.ITInvestmentPayment = OFAT;
                                    //OEmployeePayroll.DBTrack = dbt;
                                    db.EmployeePayroll.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                }
                                ts.Complete();
                                return Json(new { status = true, responseText = "Data Created Successfully." }, JsonRequestBehavior.AllowGet);
                                //   return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

                            }
                            catch (DataException ex)
                            {
                                //    List<string> Msg = new List<string>();
                                //    Msg.Add(ex.Message);
                                //LogFile Logfile = new LogFile();
                                //ErrorLog Err = new ErrorLog()
                                //{
                                //    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                //    ExceptionMessage = ex.Message,
                                //    ExceptionStackTrace = ex.StackTrace,
                                //    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                //    LogTime = DateTime.Now
                                //};
                                //Logfile.CreateLogFile(Err);
                                //return Json(new { sucess = false,Msg }, JsonRequestBehavior.AllowGet);
                            }


                        }
                    }
                    return Json(new { status = false, responseText = "Unable to create..." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { status = false, responseText = e.Message }, JsonRequestBehavior.AllowGet);

                }
            }
        }

        [HttpPost]
        public ActionResult GetSubInvPayLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSubInvestmentPayment.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ITSubInvestmentPayment.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateStatus(ITInvestmentPayment LvReq, FormCollection form, String data)
        {
            try
            {
                string authority = form["authority"] == null ? null : form["authority"];
                var isClose = TempData["IsClose"];
                //var isClose = form["isClose"] == null ? null : form["isClose"];
                if (authority == null && isClose == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                string MySelf = form["MySelf"] == null ? "false" : form["MySelf"];
                string Sanction = form["Sanction"];
                string Approval = form["Approval"];
                string HR = form["HR"] == null ? null : form["HR"];
                string ReasonMySelf = form["ReasonMySelf"] == null ? null : form["ReasonMySelf"];
                string ReasonSanction = form["ReasonSanction"];
                string ActualInvestmentAmount = form["ActualInvestmentAmount"];
                string ReasonApproval = form["ReasonApproval"];
                string ReasonHr = form["ReasonHr"] == null ? null : form["ReasonHr"];
                bool SanctionRejected = false;
                bool HrRejected = false;
                var ids = Utility.StringIdsToListString(data);
                
                var status = ids.Count > 0 ? ids[2] : null;
                var id = Convert.ToInt32(ids[0]);
                var Emp = Convert.ToInt32(ids[1]);
                using (DataBaseContext db = new DataBaseContext())
                {
                    var qurey = db.ITInvestmentPayment.Include(e => e.WFDetails)
                        .Include(e => e.WFStatus).Where(e => e.Id == id).SingleOrDefault();
                    if (authority.ToUpper() == "MYSELF")
                    {
                        qurey.Reason = ReasonMySelf;
                        qurey.isCancel = true;
                        qurey.TrClosed = true;
                        qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "6").FirstOrDefault(); //db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();
                    }
                    else if (authority.ToUpper() == "SANCTION")
                    {
                        if (Sanction == null)
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Sanction Status....." }, JsonRequestBehavior.AllowGet);

                        }
                        if (ReasonSanction == "")
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason...." }, JsonRequestBehavior.AllowGet);

                        }
                        if (ActualInvestmentAmount == "")
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The ActualInvestment...." }, JsonRequestBehavior.AllowGet);

                        }
                        if (Convert.ToBoolean(Sanction) == true)
                        {
                            //sanction yes -1
                            var LvWFDetails = new LvWFDetails
                            {
                                WFStatus = 1,
                                Comments = ReasonSanction,
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                            };
                            qurey.WFDetails.Add(LvWFDetails);
                            qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(); // db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault();
                            qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                            qurey.ActualInvestment = Convert.ToDouble(ActualInvestmentAmount);
                        }
                        else if (Convert.ToBoolean(Sanction) == false)
                        {
                            //sanction no -2
                            var LvWFDetails = new LvWFDetails
                            {
                                WFStatus = 2,
                                Comments = ReasonSanction,
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                            };
                            qurey.WFDetails.Add(LvWFDetails);
                            qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "2").FirstOrDefault(); // db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                            qurey.TrClosed = true;
                            SanctionRejected = true;
                        }
                    }
                    else if (authority.ToUpper() == "APPROVAL")//Hr
                    {
                        if (Approval == null)
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Approval Status....." }, JsonRequestBehavior.AllowGet);

                        }
                        if (ReasonApproval == "")
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason...." }, JsonRequestBehavior.AllowGet);

                        }
                        if (ActualInvestmentAmount == "")
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The ActualInvestment...." }, JsonRequestBehavior.AllowGet);

                        }
                        if (Convert.ToBoolean(Approval) == true)
                        {
                            //approval yes-3
                            var LvWFDetails = new LvWFDetails
                            {
                                WFStatus = 3,
                                Comments = ReasonApproval,
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                            };
                            qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                            qurey.WFDetails.Add(LvWFDetails);
                            qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault(); // db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();
                            qurey.ActualInvestment = Convert.ToDouble(ActualInvestmentAmount);
                        }
                        else if (Convert.ToBoolean(Approval) == false)
                        {
                            //approval no-4
                            var LvWFDetails = new LvWFDetails
                            {
                                WFStatus = 4,
                                Comments = ReasonApproval,
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                            };
                            qurey.WFDetails.Add(LvWFDetails);
                            qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "4").FirstOrDefault(); // db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault();
                            qurey.TrClosed = true;
                            HrRejected = true;
                        }
                    }
                    else if (authority.ToUpper() == "RECOMMAND")
                    {

                    }

                    using (TransactionScope ts = new TransactionScope())
                    {
                        //if someone reject lv
                        if (SanctionRejected == true || HrRejected == true)
                        {
                            qurey.TrReject = true;
                        }
                        //    ts.Complete();
                        //    return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                        //}

                        db.ITInvestmentPayment.Attach(qurey);
                        db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception e)
            {
                return Json(new Utility.JsonClass { status = false, responseText = e.Message }, JsonRequestBehavior.AllowGet);

            }
            //   return View();
        }
        public class ITinvestmentPaymentChildDataClass
        {
            public string Id { get; set; }
            public string ITInvestmentName { get; set; }
            public string InvestmentDate { get; set; }
            public string DeclaredInvestment { get; set; }
            public string ActualInvestment { get; set; }

        }

        public ActionResult GetItInvestmentPaymentData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);
                var WfStatus = new List<Int32>();
                var SanctionStatus = new List<Int32>();
                var ApporvalStatus = new List<Int32>();
                if (authority.ToUpper() == "SANCTION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "APPROVAL")
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
                var status = ids.Count > 0 ? ids[2] : null;
                var id = Convert.ToInt32(ids[0]);
                var Emp = Convert.ToInt32(ids[1]);
                var listOfObject = db.EmployeePayroll
                    .Include(e => e.ITInvestmentPayment)
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.ITInvestmentPayment.Select(a => a.FinancialYear))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.ITSection))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.ITSection.ITSectionList))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.ITSection.ITSectionListType))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.ITInvestment))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.ITSubInvestmentPayment))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.WFDetails))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.WFStatus))                    
                   //.Where(e => e.Employee.Id == Emp && e.ITInvestmentPayment.Any(a => a.Id == id)).SingleOrDefault();
                   .Where(e => e.Id == Emp && e.ITInvestmentPayment.Any(a => a.Id == id)).SingleOrDefault();

                var ItInvestmentPayment11 = db.ITInvestmentPayment.Include(e => e.ITSubInvestmentPayment).ToList();
                foreach (var item11 in ItInvestmentPayment11.Where(e => e.Id == id))
                {
                    if (item11.Path != null)
                    {
                        var ad11 = listOfObject.ITInvestmentPayment.Where(e => e.Id == id).Select(e => new
                            {
                                ITSubInvestmentPayment = e.ITSubInvestmentPayment.Select(p => new
                                {
                                    filepath11 = p.Path == null ? "" : p.Path,
                                }).ToList(),
                                filepath12 = e.Path == null ? "" : e.Path,
                                EmployeeName = listOfObject.Employee.EmpCode + " " + listOfObject.Employee.EmpName.FullNameFML,
                                Status = status,
                                ActualInvestment = e.ActualInvestment,
                                FinancialYear = e.FinancialYear != null ? e.FinancialYear.FullDetails : null,
                                DeclaredInvestment = e.DeclaredInvestment,
                                ITInvestmentName = e.ITInvestment != null ? e.ITInvestment.ITInvestmentName : null,
                                ITSection = e.ITSection != null ? e.ITSection.ITSectionList : null,
                                ItStatus = e.ITSection != null ? e.ITSection.ITSectionListType : null,
                                isCancel = e.isCancel,
                                InvestmentDate = e.InvestmentDate != null ? e.InvestmentDate.Value.ToShortDateString() : null,
                                Narration = e.Narration,
                                SanctionComment = e.WFDetails != null && e.WFDetails.Count > 0 ? e.WFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(a => a.Id).Select(s => s.Comments).LastOrDefault() : null,
                                ApporavalComment = e.WFDetails != null && e.WFDetails.Count > 0 ? e.WFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(a => a.Id).Select(s => s.Comments).LastOrDefault() : null,
                                Wf = e.WFDetails != null && e.WFDetails.Count > 0 ? e.WFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(s => s.Id).Select(a => a.Comments).LastOrDefault() : null
                            }).ToList();
                        TempData["IsClose"] = status;

                        return Json(ad11, JsonRequestBehavior.AllowGet);
                    }
                    foreach (var item12 in item11.ITSubInvestmentPayment)
                    {
                        if (item12.Path != null)
                        {

                            var ad12 = listOfObject.ITInvestmentPayment.Where(e => e.Id == id).Select(e => new
                            {
                                ITSubInvestmentPayment = e.ITSubInvestmentPayment.Select(p => new
                                {
                                    filepath11 = p.Path == null ? "" : p.Path,
                                }).ToList(),
                                filepath12 = e.Path == null ? "" : e.Path,
                                EmployeeName = listOfObject.Employee.EmpCode + " " + listOfObject.Employee.EmpName.FullNameFML,
                                Status = status,
                                ActualInvestment = e.ActualInvestment,
                                FinancialYear = e.FinancialYear != null ? e.FinancialYear.FullDetails : null,
                                DeclaredInvestment = e.DeclaredInvestment,
                                ITInvestmentName = e.ITInvestment != null ? e.ITInvestment.ITInvestmentName : null,
                                ITSection = e.ITSection != null ? e.ITSection.ITSectionList : null,
                                ItStatus = e.ITSection != null ? e.ITSection.ITSectionListType : null,
                                isCancel = e.isCancel,
                                InvestmentDate = e.InvestmentDate != null ? e.InvestmentDate.Value.ToShortDateString() : null,
                                Narration = e.Narration,
                                SanctionComment = e.WFDetails != null && e.WFDetails.Count > 0 ? e.WFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(a => a.Id).Select(s => s.Comments).LastOrDefault() : null,
                                ApporavalComment = e.WFDetails != null && e.WFDetails.Count > 0 ? e.WFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(a => a.Id).Select(s => s.Comments).LastOrDefault() : null,
                                Wf = e.WFDetails != null && e.WFDetails.Count > 0 ? e.WFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(s => s.Id).Select(a => a.Comments).LastOrDefault() : null
                            }).ToList();

                            TempData["IsClose"] = status;

                            return Json(ad12, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                return null;
            }
        }
        public class ItinvestmentpaymentClass1
        {
            public string Emp { get; set; }
            public string Itinvestment { get; set; }
            public string Itsection { get; set; }
            public string Investmentdate { get; set; }
            public string ActualInvestment { get; set; }
            public string DeclaredInvestment { get; set; }
            public ChildGetLvNewReqClass2 RowData { get; set; }
        }
        public class ChildGetLvNewReqClass2
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }

        }

        public ActionResult GetItInvestmentPayment()
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


                }
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }

                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                if (EmpidsWithfunsub.Any(e => e.SubModuleName != null))
                {
                    EmpidsWithfunsub = EmpidsWithfunsub.Where(e => e.SubModuleName == "DECLINVEST").ToList();
                }
                List<int> EmpIds = new List<int>();

                List<EmployeePayroll> LvList = new List<EmployeePayroll>();

                List<ItinvestmentpaymentClass1> ListreturnLvnewClass = new List<ItinvestmentpaymentClass1>();
                ListreturnLvnewClass.Add(new ItinvestmentpaymentClass1
                {
                    Emp = "Employee",
                    Itinvestment = "It Investment",
                    Itsection = "ItSection",
                    Investmentdate = "Investment Date" + " ",
                    ActualInvestment = "Actual Investment" + " ",
                    DeclaredInvestment = "Declare Investment"

                });

                foreach (var item1 in EmpidsWithfunsub)
                {
                    //item.ReportingEmployee
                    if (item1.ReportingEmployee.Count() > 0)
                    {
                        List<string> Funcsubid = new List<string>();
                        var temp = db.EmployeePayroll
                          .Include(e => e.Employee)
                           .Include(e => e.Employee.EmpName)
                           .Include(e => e.ITInvestmentPayment)
                           .Include(e => e.ITInvestmentPayment.Select(a => a.WFStatus))
                           .Include(e => e.ITInvestmentPayment.Select(a => a.WFDetails)); 

                        LvList = temp.Where(e => item1.ReportingEmployee.Contains(e.Employee.Id)).AsNoTracking().ToList();

                        var LvIds = UserManager.FilterITInvestmentPayment(LvList.SelectMany(e => e.ITInvestmentPayment).OrderByDescending(e => e.InvestmentDate).ToList(),
                            Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                        var session = Session["auho"].ToString().ToUpper();

                        foreach (var item in LvIds)
                        {
                            
                         var query = db.EmployeePayroll.Include(e => e.Employee)
                         .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights))
                         .Include(e => e.Employee.ReportingStructRights.Select(a => a.FuncModules))
                         .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights.ActionName))
                         .Include(e => e.Employee.ReportingStructRights.Select(a => a.FuncSubModules))
                         .Include(e => e.Employee.EmpName)
                         .Include(e => e.ITInvestmentPayment)
                         .Include(e => e.ITInvestmentPayment.Select(t => t.ITSection))
                         .Include(e => e.ITInvestmentPayment.Select(r => r.ITSubInvestmentPayment))
                         .Include(e => e.ITInvestmentPayment.Select(t => t.ITSection.ITSectionList))
                         .Include(e => e.ITInvestmentPayment.Select(t => t.ITInvestment))
                         .Include(e => e.ITInvestmentPayment.Select(t => t.ITSection))
                         .Where(e => e.ITInvestmentPayment.Any(a => a.Id == item)).SingleOrDefault();

                                var Investmentid = query.ITInvestmentPayment.Where(a => a.Id == item).FirstOrDefault();
                                var ItInvestmentPayment12 = db.ITInvestmentPayment.Include(e => e.ITSubInvestmentPayment).ToList();

                                foreach (var item12 in ItInvestmentPayment12.Where(e => e.Id == item))
                                {
                                    if (item12.Path != null)
                                    {
                                        ListreturnLvnewClass.Add(new ItinvestmentpaymentClass1
                                        {
                                            RowData = new ChildGetLvNewReqClass2
                                            {
                                                LvNewReq = Investmentid.Id.ToString(),
                                                EmpLVid = query.Id.ToString(),
                                                IsClose = query.Employee.ReportingStructRights
                                                .Where(a => a.AccessRights != null && a.AccessRights.ActionName.LookupVal.ToUpper() == session && a.FuncSubModules != null && a.FuncSubModules.LookupVal == "DECLINVEST")
                                                .Select(a => a.AccessRights.IsClose).FirstOrDefault().ToString(),
                                                LvHead_Id = "",
                                            },
                                            Emp = query.Employee.EmpCode + " " + query.Employee.EmpName.FullNameFML,
                                            Itinvestment = Investmentid.ITInvestment.ITInvestmentName,
                                            Itsection = Investmentid.ITSection.ITSectionList.LookupVal.ToString(),
                                            Investmentdate = Investmentid.InvestmentDate.Value.ToShortDateString(),
                                            ActualInvestment = Investmentid.ActualInvestment.ToString(),
                                            DeclaredInvestment = Investmentid.DeclaredInvestment.ToString(),
                                        });
                                    }
                                }
                        }

                     
                    }
                }
                if (ListreturnLvnewClass != null && ListreturnLvnewClass.Count > 0)
                {
                    return Json(new { status = true, data = ListreturnLvnewClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = ListreturnLvnewClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        //public ActionResult GetItInvestmentPayment()
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        LookupValue FuncModule = new LookupValue();
        //        if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
        //        {
        //            var id = Convert.ToString(Session["user-module"]);
        //            var lookupdata = db.Lookup
        //                     .Include(e => e.LookupValues)
        //                     .Where(e => e.Code == "601").SingleOrDefault();
        //            FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
        //        }
        //        if (FuncModule == null)
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }
        //        var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
        //        if (EmpIds == null)
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }

        //        var LvList = db.EmployeePayroll
        //            .Include(e => e.Employee)
        //            .Include(e => e.Employee.ReportingStructRights)
        //            .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights))
        //            .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights.ActionName))
        //            .Include(e => e.ITInvestmentPayment)
        //            .Include(e => e.ITInvestmentPayment.Select(a => a.WFStatus))
        //            .Include(e => e.ITInvestmentPayment.Select(a => a.WFDetails))
        //           .Where(e => EmpIds.Contains(e.Employee.Id)).ToList();

        //        List<ItinvestmentpaymentClass1> ListreturnLvnewClass = new List<ItinvestmentpaymentClass1>();
        //        ListreturnLvnewClass.Add(new ItinvestmentpaymentClass1
        //        {
        //            Emp = "Employee",
        //            Itinvestment = "It Investment",
        //            Itsection = "ItSection",
        //            Investmentdate = "Investment Date" + " ",
        //            ActualInvestment = "Actual Investment" + " ",
        //            DeclaredInvestment = "Declare Investment"

        //        });
        //        var LvIds = UserManager.FilterITInvestmentPayment(LvList.SelectMany(e => e.ITInvestmentPayment).ToList(), Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));
        //        var session = Session["auho"].ToString().ToUpper();
        //        foreach (var item in LvIds)
        //        {
        //            var query = db.EmployeePayroll.Include(e => e.Employee)
        //                .Include(e => e.Employee.EmpName)
        //                .Include(e => e.ITInvestmentPayment)
        //                .Include(e => e.ITInvestmentPayment.Select(t => t.ITSection))
        //                .Include(e => e.ITInvestmentPayment.Select(r => r.ITSubInvestmentPayment))
        //                .Include(e => e.ITInvestmentPayment.Select(t => t.ITSection.ITSectionList))
        //                .Include(e => e.ITInvestmentPayment.Select(t => t.ITInvestment))
        //                .Include(e => e.ITInvestmentPayment.Select(t => t.ITSection))
        //                .Where(e => e.ITInvestmentPayment.Any(a => a.Id == item)).SingleOrDefault();

        //            var Investmentid = query.ITInvestmentPayment.Where(a => a.Id == item).FirstOrDefault();
        //            var ItInvestmentPayment12 = db.ITInvestmentPayment.Include(e => e.ITSubInvestmentPayment).ToList();

        //            foreach (var item12 in ItInvestmentPayment12.Where(e => e.Id == item))
        //            {
        //                if (item12.Path != null)
        //                {
        //                    ListreturnLvnewClass.Add(new ItinvestmentpaymentClass1
        //                    {
        //                        RowData = new ChildGetLvNewReqClass2
        //                        {
        //                            LvNewReq = Investmentid.Id.ToString(),
        //                            EmpLVid = query.Id.ToString(),
        //                            IsClose = query.Employee.ReportingStructRights
        //                            .Where(a => a.AccessRights != null && a.AccessRights.ActionName.LookupVal.ToUpper() == session)
        //                            .Select(a => a.AccessRights.IsClose).FirstOrDefault().ToString(),
        //                            LvHead_Id = "",
        //                        },
        //                        Emp = query.Employee.EmpCode + " " + query.Employee.EmpName.FullNameFML,
        //                        Itinvestment = Investmentid.ITInvestment.ITInvestmentName,
        //                        Itsection = Investmentid.ITSection.ITSectionList.LookupVal.ToString(),
        //                        Investmentdate = Investmentid.InvestmentDate.Value.ToShortDateString(),
        //                        ActualInvestment = Investmentid.ActualInvestment.ToString(),
        //                        DeclaredInvestment = Investmentid.DeclaredInvestment.ToString(),
        //                    });
        //                }
        //                //foreach (var item21 in item12.ITSubInvestmentPayment)
        //                //{
        //                //    if (item21.Path != null)
        //                //    {
        //                //        ListreturnLvnewClass.Add(new ItinvestmentpaymentClass1
        //                //        {
        //                //            RowData = new ChildGetLvNewReqClass2
        //                //            {
        //                //                LvNewReq = Investmentid.Id.ToString(),
        //                //                EmpLVid = query.Id.ToString(),
        //                //                IsClose = query.Employee.ReportingStructRights
        //                //                .Where(a => a.AccessRights != null && a.AccessRights.ActionName.LookupVal.ToUpper() == session)
        //                //                .Select(a => a.AccessRights.IsClose).FirstOrDefault().ToString(),
        //                //                LvHead_Id = "",
        //                //            },
        //                //            Emp = query.Employee.EmpCode + " " + query.Employee.EmpName.FullNameFML,
        //                //            Itinvestment = Investmentid.ITInvestment.ITInvestmentName,
        //                //            Itsection = Investmentid.ITSection.ITSectionList.LookupVal.ToString(),
        //                //            Investmentdate = Investmentid.InvestmentDate.Value.ToShortDateString(),
        //                //            ActualInvestment = Investmentid.ActualInvestment.ToString(),
        //                //            DeclaredInvestment = Investmentid.DeclaredInvestment.ToString(),
        //                //        });
        //                //    }
        //                //}
        //            }
        //        }

        //        if (ListreturnLvnewClass != null && ListreturnLvnewClass.Count > 0)
        //        {
        //            return Json(new { status = true, data = ListreturnLvnewClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { status = false, data = ListreturnLvnewClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

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
            public List<ReqLvHeadWise> LvHeadName { get; set; }
        }
        public class ReqLvHeadWise
        {
            public string LvHeadName { get; set; }
            public string LvHeadCode { get; set; }
            public string LvHeadBal { get; set; }
            public Array LvReq { get; set; }
        }

        public ActionResult GetEmpInvestmentHistory()
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                              .Include(e => e.LookupValues)
                              .Where(e => e.Code == "601").SingleOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Empid = db.EmployeePayroll
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.ITInvestmentPayment)
                    .Include(e => e.ITInvestmentPayment.Select(a => a.ITInvestment))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.ITSubInvestmentPayment))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.ITSubInvestmentPayment.Select(r => r.ITSubInvestment)))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.WFStatus))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.WFDetails));
                   // .Where(e => EmpIds.Contains(e.Employee.Id)&& e.ITInvestmentPayment !=null).ToList();
                 var Emps = Empid.Where(e => EmpIds.Contains(e.Id)).AsNoTracking().ToList();
                //var allLvHead = db.ITInvestment.ToList();
                List<EmpLvClass> ListEmpLvClass = new List<EmpLvClass>();
                foreach (var ca in Emps)
                {
                    var OITInvestmentPayment = ca.ITInvestmentPayment.Where(e => e.ITInvestment != null).ToList();
                    var oEmpLvClass = new EmpLvClass();
                    //foreach (var item2 in item1)
                    //{
                    //if (item2.ITInvestment != null)
                    //{
                    foreach (var item in OITInvestmentPayment.OrderBy(e => e.InvestmentDate))
                    {
                        if (item.Path != null)
                        {
                            //var temp = new List<tempClass>();
                            //var LvData = item1.Where(e => e.ITInvestment.Id == lvhead.Id).OrderByDescending(e => e.InvestmentDate).ToList();

                            //foreach (var item in LvData)
                            //{
                            var Status = "--";
                            if ((item.InputMethod == 1 && item.WFDetails.Count > 0) || item.InputMethod == 2 && item.WFDetails.Count > 0)
                            {
                                Status = Utility.GetStatusName().Where(e => e.Key == item.WFDetails.LastOrDefault().WFStatus.ToString())
                               .Select(e => e.Value).SingleOrDefault();
                            }
                            if (item.InputMethod == 0)
                            {
                                Status = "Approved By HRM (M)";
                            }
                            //if (item.ITSubInvestmentPayment.Count() > 0)
                            //{
                            //    foreach (var subinv in item.ITSubInvestmentPayment)
                            //    {
                            //        temp.Add(new tempClass
                            //        {
                            //            LvName = item.ITInvestment.ITInvestmentName + "- " + subinv.ITSubInvestment.SubInvestmentName,
                            //            LvCode = item.Id.ToString() + "," + subinv.Id.ToString(),
                            //            LvBal = "Actual Investment:" + subinv.ActualInvestment + "Declare Investment :" + subinv.DeclaredInvestment + " Status :" + Status,
                            //        });
                            //    }
                            //}
                            //else
                            //{
                            List<tempClass> temp = new List<tempClass>();
                            temp.Add(new tempClass
                            {
                                LvName = item.ITInvestment.ITInvestmentName,
                                LvCode = item.Id.ToString(),
                                LvBal = "Actual Investment:" + item.ActualInvestment + "Declare Investment :" + item.DeclaredInvestment + " Status :" + Status,
                            });
                            //}

                            //if (LvData != null && LvData.Count > 0)
                            //{                       
                            oEmpLvClass.EmpName = ca.Employee.EmpCode + " " + ca.Employee.EmpName.FullNameFML;
                            if (oEmpLvClass.LvHeadName == null)
                            {
                                oEmpLvClass.LvHeadName = new List<ReqLvHeadWise>
                            {
                                new ReqLvHeadWise
                                {                                          
                                    LvHeadName=temp.Select(e=>e.LvName).FirstOrDefault().ToString(),                                          
                                    LvHeadCode=temp.Select(e=>e.LvCode).FirstOrDefault().ToString(),
                                    LvHeadBal=temp.Select(e=>e.LvBal).FirstOrDefault().ToString()                                         
                                }
                            };
                            }
                            else
                            {
                                foreach (var ttt in temp)
                                {
                                    oEmpLvClass.LvHeadName.Add(new ReqLvHeadWise
                                    {
                                        LvHeadName = ttt.LvName,
                                        LvHeadCode = ttt.LvCode,
                                        LvHeadBal = ttt.LvBal
                                    });
                                }
                            }
                            //}
                            //}

                            if (oEmpLvClass.EmpName != null)
                            {
                                ListEmpLvClass.Add(oEmpLvClass);
                            }
                        }
                    }
                    //  }
                    // }
                    var result = ListEmpLvClass.GroupBy(x => x.EmpName).Select(y => y.First()).ToList();
                    return Json(new Utility.JsonClass { status = true, Data = result }, JsonRequestBehavior.AllowGet);
                }
                return null;
            }
        }
        public class returnDataClass
        {
            public Int32 Id { get; set; }
            public String val { get; set; }
            public Int32 Id2 { get; set; }
            public bool Id3 { get; set; }
            public Int32 LvHead_Id { get; set; }
        }

        public class AttachDataClass
        {
            public Int32 EmpId { get; set; }
            public Int32 EmpLVid { get; set; }
            public Int32 emppayrollid { get; set; }
            public Int32 LvNewReq { get; set; }
            public String status { get; set; }
            public string val { get; set; }
            public bool IsClose { get; set; }
            public Int32 LvHead_Id { get; set; }
            public Int32 ItinvestmentId { get; set; }

            public String Id { get; set; }
        }

        public class GetItInvestmentClass
        {
            public string InvestmentName { get; set; }
            public string DeclaredAmount { get; set; }
            public string ActualAmount { get; set; }
            public string ITSection { get; set; }
            public string ItSectionType { get; set; }
            public string Status { get; set; }           
            public string SubId { get; set; }
            public string SubInvestmentName { get; set; }
            public string InvestmentDate { get; set; }
            public string ItinvestmentId { get; set; }
            
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

        //public ActionResult GetMyItInvestment()
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }
        //        var Emp = Convert.ToInt32(SessionManager.EmpId);
        //         var EmpPayId = Convert.ToInt32(SessionManager.EmpPayrollId);;


        //        List<GetItInvestmentClass> returndata = new List<GetItInvestmentClass>();
        //        returndata.Add(new GetItInvestmentClass
        //        {
        //            ItinvestmentId = "Id",
        //            InvestmentDate = "InvestmentDate",
        //            InvestmentName = "InvestmentName",
        //            Status = "Status",
        //            SubId = "SubId",
        //            SubInvestmentName = "SubInvestmentName"

        //        });

        //        try
        //        {
        //            ServiceResult<List<EPMS_Investments>> responseDeserializeData = new ServiceResult<List<EPMS_Investments>>();
        //            string APIUrl = ConfigurationManager.AppSettings["APIURL"];
        //            string ShowMessage = "";
        //            using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
        //            {
        //                var response = p2BHttpClient.request("EPMS/getInvestmentHistoryRequest",
        //                    new EPMS_Investments()
        //                    {
        //                       Emp_ID = Emp
        //                    });



        //                responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject < ServiceResult<List<EPMS_Investments>>>(response.Content.ReadAsStringAsync().Result);

        //                if (responseDeserializeData.Data != null)
        //                {
        //                    foreach (var item in responseDeserializeData.Data)
        //                    {
        //                        returndata.Add(new GetItInvestmentClass
        //                                              {
        //                                                  RowData = new ChildGetLvNewReqClass
        //                                                  {
        //                                                      LvNewReq = item.Investment_ID.ToString(),
        //                                                      EmpLVid = EmpPayId.ToString(),
        //                                                      IsClose = "",
        //                                                      Status = item.Status,
        //                                                      LvHead_Id = "",
        //                                                  },
        //                                                  ItinvestmentId = item.Investment_ID.ToString(),
        //                                                  InvestmentDate = item.Invest_Date.ToShortDateString(),
        //                                                  InvestmentName = item.Investment_Name,
        //                                                  SubId = "",
        //                                                  SubInvestmentName = "",
        //                                                  //" Status :" + Utility.GetStatusName().Where(e => item.WFStatus != null && e.Key == item.WFStatus.LookupVal).Select(e => e.Value).SingleOrDefault(),
        //                                                  //Status = item.WFStatus != null && new[] { "2", "4" }.Contains(item.WFStatus.LookupVal) == true ? "0" : "1",
        //                                                  Status = item.InputMethod == 0 ? "Approved By HRM (M)" : item.WFStatusDetails

        //                                              });
        //                    }

        //                }

        //            }


        //        }
        //        catch (DataException ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            return Json(new { status = false, responseText = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
        //        }


        //            return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);

        //    }
        //}


        public ActionResult GetMyItInvestment()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var db_data = db.EmployeePayroll
                    .Include(e => e.ITInvestmentPayment)
                    .Include(e => e.ITInvestmentPayment.Select(z=>z.ITSection.ITSectionList))
                    .Include(e => e.ITInvestmentPayment.Select(z => z.ITSection.ITSectionListType))
                    .Include(e => e.ITInvestmentPayment.Select(t => t.ITInvestment))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.WFStatus))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.WFDetails))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.ITSubInvestmentPayment))
                    .Include(e => e.ITInvestmentPayment.Select(t => t.ITSubInvestmentPayment.Select(a => a.ITSubInvestment)))
                    //.Include(e => e.ITInvestmentPayment.Select(a => a.ITInvestment))
                    //.Include(e => e.ITInvestmentPayment.Select(a => a.ITInvestment.SalaryHead))
                    .Where(e => e.Employee.Id == Emp)
                    .SingleOrDefault();

                List<GetItInvestmentClass> returndata = new List<GetItInvestmentClass>();
                returndata.Add(new GetItInvestmentClass
                {                                 
                    InvestmentName = "InvestmentName",
                    DeclaredAmount = "DeclaredAmount",
                    ActualAmount = "ActualAmount",
                    ITSection = "ITSection",
                    ItSectionType = "ItSectionType",
                    Status = "Status",                                     
                    SubInvestmentName = "SubInvestmentName",
                    SubId = "SubId",
                    InvestmentDate = "InvestmentDate", 
                    ItinvestmentId = "Id"

                });

                if (db_data != null && db_data.ITInvestmentPayment != null && db_data.ITInvestmentPayment.Count() > 0)
                {
                    foreach (var item in db_data.ITInvestmentPayment.OrderByDescending(e => e.InvestmentDate))
                    {
                        var InvestmentDate = item.InvestmentDate != null ? item.InvestmentDate.Value.ToShortDateString() : null;
                        var Status = "--";
                        if (item.InputMethod == 1 && item.WFDetails.Count > 0)
                        {
                            Status = Utility.GetStatusName().Where(e => e.Key == item.WFDetails.LastOrDefault().WFStatus.ToString())
                            .Select(e => e.Value).SingleOrDefault();
                        }
                        if (item.InputMethod == 0)
                        {
                            Status = "Approved By HRM (M)";
                        }
                        if (item.ITSubInvestmentPayment.Count() > 0)
                        {
                            foreach (var item1 in item.ITSubInvestmentPayment)
                            {
                                returndata.Add(new GetItInvestmentClass
                                {
                                    RowData = new ChildGetLvNewReqClass
                                    {
                                        LvNewReq = item.Id.ToString(),
                                        EmpLVid = db_data.Id.ToString(),
                                        IsClose = "",
                                        Status = Status,
                                        LvHead_Id = item1.Id.ToString(),
                                    },
                                    InvestmentName = item.ITInvestment.ITInvestmentName,
                                    DeclaredAmount = item.DeclaredInvestment.ToString(),
                                    ActualAmount = item.ActualInvestment.ToString(),
                                    ITSection = item.ITSection.FullDetails,
                                    ItSectionType = item.ITSection.ITSectionListType.LookupVal,
                                    Status = Status,                 
                                    SubId = item1.Id.ToString(),
                                    SubInvestmentName = item1.ITSubInvestment == null ? "" : item1.ITSubInvestment.SubInvestmentName,
                                    //" Status :" + Utility.GetStatusName().Where(e => item.WFStatus != null && e.Key == item.WFStatus.LookupVal).Select(e => e.Value).SingleOrDefault(),
                                    //Status = item.WFStatus != null && new[] { "2", "4" }.Contains(item.WFStatus.LookupVal) == true ? "0" : "1",
                                    InvestmentDate = InvestmentDate,
                                    ItinvestmentId = item.Id.ToString()

                                });
                            }
                        }
                        else
                        {
                            returndata.Add(new GetItInvestmentClass
                            {
                                RowData = new ChildGetLvNewReqClass
                                {
                                    LvNewReq = item.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString(),
                                    IsClose = "",
                                    Status = Status,
                                    LvHead_Id = "",
                                },
                                                       
                                InvestmentName = item.ITInvestment == null ? "" : item.ITInvestment.ITInvestmentName,
                                DeclaredAmount = item.DeclaredInvestment == null ? "0" : item.DeclaredInvestment.ToString(),
                                ActualAmount = item.ActualInvestment == null ? "0" : item.ActualInvestment.ToString(),
                                ITSection = item.ITInvestment == null || item.ITInvestment.ITSection == null ? "" : item.ITInvestment.ITSection.ITSectionList.LookupVal.ToUpper(),
                                ItSectionType = item.ITInvestment == null || item.ITInvestment.ITSection == null ? "" : item.ITInvestment.ITSection.ITSectionListType.LookupVal.ToUpper(),
                                Status = Status,
                                SubId = "",
                                SubInvestmentName = "",
                                //" Status :" + Utility.GetStatusName().Where(e => item.WFStatus != null && e.Key == item.WFStatus.LookupVal).Select(e => e.Value).SingleOrDefault(),
                                //Status = item.WFStatus != null && new[] { "2", "4" }.Contains(item.WFStatus.LookupVal) == true ? "0" : "1",
                                InvestmentDate = InvestmentDate,
                                ItinvestmentId = item.Id.ToString()

                            });
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


        public ActionResult GetMyItInvestment1()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var qurey = db.EmployeePayroll
                    .Include(e => e.ITInvestmentPayment)
                    .Include(e => e.ITInvestmentPayment.Select(a => a.WFStatus))
                    //.Include(e => e.ITInvestmentPayment.Select(a => a.ITInvestment))
                    //.Include(e => e.ITInvestmentPayment.Select(a => a.ITInvestment))
                    //.Include(e => e.ITInvestmentPayment.Select(a => a.ITInvestment.SalaryHead))
                    .Where(e => e.Employee.Id == Emp)
                    .SingleOrDefault();

                var ListreturnDataClass = new List<AttachDataClass>();
                if (qurey != null && qurey.ITInvestmentPayment != null && qurey.ITInvestmentPayment.Count() > 0)
                {
                    foreach (var item in qurey.ITInvestmentPayment)
                    {
                        var InvestmentDate = item.InvestmentDate != null ? item.InvestmentDate.Value.ToShortDateString() : null;
                        //var LoanAdvanceHead = item.LoanAdvanceHead != null ? item.LoanAdvanceHead.FullDetails : null;
                        //var Narration = item.Narration != null ? item.Narration : null;
                        //var ITSection = item.ITSection != null ? item.ITSection.FullDetails : null;
                        //var ITSubInvestmentPayment_m = item.ITSubInvestmentPayment.Count > 0 ? item.ITSubInvestmentPayment.Select(e => e.ActualInvestment).ToArray() : null;
                        //var ITSubInvestmentPayment = ITSubInvestmentPayment_m != null ? string.Join(",", ITSubInvestmentPayment_m) : null;
                        ListreturnDataClass.Add(new AttachDataClass
                        {
                            ItinvestmentId = item.Id,
                            val =
                            "InvestmentDate :" + InvestmentDate + " " +
                            " Status :" + Utility.GetStatusName().Where(e => item.WFStatus != null && e.Key == item.WFStatus.LookupVal).Select(e => e.Value).SingleOrDefault(),
                            status = item.WFStatus != null && new[] { "2", "4" }.Contains(item.WFStatus.LookupVal) == true ? "0" : "1",
                        });
                    }
                }
                if (ListreturnDataClass != null && ListreturnDataClass.Count > 0)
                {
                    return Json(new { status = true, data = ListreturnDataClass, responseText = "True" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = ListreturnDataClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult GetMyITInvestmentData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                ITInvestmentPayment itinvestment = null;
                ITSubInvestmentPayment itsubinvestment = null;
                string localpath = "";
                string authority = Convert.ToString(Session["auho"]);
                var WfStatus = new List<Int32>();
                var SanctionStatus = new List<Int32>();
                var ApporvalStatus = new List<Int32>();
                if (authority.ToUpper() == "SANCTION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "APPROVAL")
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
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                if (ids[3] != "" && ids[3] != null)
                {
                    int subid = Convert.ToInt32(ids[3]);
                    itsubinvestment = db.ITSubInvestmentPayment.Where(e => e.Id == subid).SingleOrDefault();
                }
                var listOfObject = db.ITInvestmentPayment.Include(e => e.ITSubInvestmentPayment).Include(e => e.FinancialYear).Include(e => e.ITSection).Include(e => e.ITSection.ITSectionList)
                    .Include(e => e.ITSection.ITSectionListType).Include(e => e.ITInvestment).Include(e => e.WFDetails).Include(e => e.WFStatus)
                .Where(e => e.Id == id).AsEnumerable().Select
                (e => new
                {
                    ITSubInvestmentPayment = e.ITSubInvestmentPayment.Select(r => new
                    {
                        IdSub = r.Id,
                        FullDetailsSub = r.FullDetails
                    }).ToList(),
                    ActualInvestment = e.ActualInvestment,
                    FinancialYear = e.FinancialYear != null ? e.FinancialYear.FullDetails : null,
                    DeclaredInvestment = e.DeclaredInvestment,
                    ITInvestmentId = e.ITInvestment != null ? e.ITInvestment.Id.ToString() : null,
                    ITInvestmentName = e.ITInvestment != null ? e.ITInvestment.ITInvestmentName : null,
                    ITSection = e.ITSection != null ? e.ITSection.ITSectionList : null,
                    Status = e.ITSection != null ? e.ITSection.ITSectionListType : null,
                    InvestmentDate = e.InvestmentDate != null ? e.InvestmentDate.Value.ToShortDateString() : null,
                    Narration = e.Narration,
                    TrClosed = e.TrClosed,
                    Itinvestmentid = e.Id,
                    Itsubid = itsubinvestment != null ? itsubinvestment.Id.ToString() : null,
                    filepath = e.Path != null ? e.Path : null,
                    subfilepath = itsubinvestment != null && itsubinvestment.Path != null ? itsubinvestment.Path : null,
                    SanctionComment = e.WFDetails != null && e.WFDetails.Count > 0 ? e.WFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(a => a.Id).Select(s => s.Comments).LastOrDefault() : null,
                    ApporavalComment = e.WFDetails != null && e.WFDetails.Count > 0 ? e.WFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(a => a.Id).Select(s => s.Comments).LastOrDefault() : null,
                    Wf = e.WFDetails != null && e.WFDetails.Count > 0 ? e.WFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(s => s.Id).Select(a => a.Comments).LastOrDefault() : null
                }).ToList();

                return Json(new { data = listOfObject, status = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult InvestMentUpload(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var ids = Utility.StringIdsToListString(data);

                //ITSubInvestmentPayment listOfObjectsub = null;
                var id = Convert.ToInt32(ids[0]);
                //if (ids[1] != null && ids[1] != "")
                //{
                //    var subid = Convert.ToInt32(ids[1]);
                //    listOfObjectsub = db.ITSubInvestmentPayment.Include(e => e.ITSubInvestment)
                //  .Where(e => e.Id == subid).SingleOrDefault();
                //}
                var listOfObject = db.ITInvestmentPayment.Include(e => e.ITInvestment)
                 //.Include(e => e.FinancialYear).Include(e => e.ITSubInvestmentPayment)
                .Where(e => e.Id == id).AsEnumerable().Select
                (e => new
                {
                    Itid = e.Id,
                    //subu = listOfObjectsub != null ? listOfObjectsub.Id.ToString() : null,
                    //  ActualInvestment = e.ActualInvestment,
                    //  FinancialYear = e.FinancialYear != null ? e.FinancialYear.FullDetails : null,
                    //  DeclaredInvestment = e.DeclaredInvestment,
                    ITInvestmentName = e.ITInvestment != null ? e.ITInvestment.ITInvestmentName : null,
                    //ITSubInvestmentName = listOfObjectsub != null && listOfObjectsub.ITSubInvestment != null ? listOfObjectsub.ITSubInvestment.SubInvestmentName : null,
                    filepath = e.Path != null ? e.Path.ToString() : null,
                    //subfilepath = listOfObjectsub != null ? listOfObjectsub.Path : null,
                }).SingleOrDefault();

                return Json(new { data = listOfObject }, JsonRequestBehavior.AllowGet);
            }
        }


        public class ITInvestPayment_Val
        {
            public Array ITSubInvestmentPayment_Id { get; set; }
            public Array ITSubInvestmentPayment_Val { get; set; }
            public string ITInvestment_Id { get; set; }
            public string ITInvestment_Val { get; set; }
            public string ITSection_Id { get; set; }
            public string ITSection_Val { get; set; }
            public string LoanAdvHead_Id { get; set; }
            public string LoanAdvHead_Val { get; set; }

        }
        public ActionResult Edit(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var qurey = db.EmployeePayroll
                    .Include(e => e.ITInvestmentPayment)
                    .Include(e => e.ITInvestmentPayment.Select(a => a.ITInvestment))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.ITSection))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.ITSubInvestmentPayment))
                    .Include(e => e.ITInvestmentPayment.Select(a => a.LoanAdvanceHead))
                    .Where(e => e.Id == Emp && e.ITInvestmentPayment != null)
                    .AsEnumerable().Select(e => new
                    {
                        ITInvestmentPayment = e.ITInvestmentPayment.Where(w => w.Id == id).SingleOrDefault(),
                        ITInvestment = e.ITInvestmentPayment.Where(w => w.Id == id).Select(a => a.ITInvestment).SingleOrDefault(),
                        ITSection = e.ITInvestmentPayment.Where(a => a.Id == id).Select(q => q.ITSection).SingleOrDefault(),
                        LoanAdvanceHead = e.ITInvestmentPayment.Where(a => a.Id == id).Select(q => q.LoanAdvanceHead).SingleOrDefault(),
                        ITSubInvestmentPayment = e.ITInvestmentPayment.Where(a => a.Id == id).Select(q => q.ITSubInvestmentPayment).ToList(),
                        DBTrack = e.DBTrack
                    }).SingleOrDefault();
                // List<ITInvestPayment_Val> return_data = new List<ITInvestPayment_Val>();
                //var Q = db.ITInvestmentPayment
                //    .Where(e => e.Id == data).Select
                //    (e => new
                //    {
                //        ActualInvestment = e.ActualInvestment,
                //        DeclaredInvestment = e.DeclaredInvestment,
                //        InvestmentDate = e.InvestmentDate,
                //        Narration = e.Narration,
                //        Action = e.DBTrack.Action
                //    }).ToList();

                //var add_data = db.ITInvestmentPayment
                //  .Include(e => e.ITSubInvestmentPayment)
                //    .Include(e => e.FinancialYear)
                //    .Include(e => e.ITInvestment)
                //    .Include(e => e.ITSection)
                //    .Include(e => e.LoanAdvanceHead)
                //    .Where(e => e.Id == data)
                //   .ToList();

                //foreach (var ca in add_data)
                //{
                //    return_data.Add(
                //    new ITInvestPayment_Val
                //    {
                //        ITSubInvestmentPayment_Id = ca.ITSubInvestmentPayment == null ? null : ca.ITSubInvestmentPayment.Select(e => e.Id.ToString()).ToArray(),
                //        ITSubInvestmentPayment_Val = ca.ITSubInvestmentPayment == null ? null : ca.ITSubInvestmentPayment.Select(e => e.InvestmentDate.ToString()).ToArray(),
                //        ITInvestment_Id = ca.ITInvestment == null ? null : ca.ITInvestment.Id.ToString(),
                //        ITInvestment_Val = ca.ITInvestment == null ? null : ca.ITInvestment.FullDetails,
                //        ITSection_Id = ca.ITSection == null ? null : ca.ITSection.Id.ToString(),
                //        ITSection_Val = ca.ITSection == null ? null : ca.ITSection.FullDetails,
                //        LoanAdvHead_Id = ca.LoanAdvanceHead == null ? null : ca.LoanAdvanceHead.Id.ToString(),
                //        LoanAdvHead_Val = ca.LoanAdvanceHead == null ? null : ca.LoanAdvanceHead.FullDetails

                //    });
                //}

                //var W = db.DT_ITInvestmentPayment
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         ActualInvestment = e.ActualInvestment,
                //         DeclaredInvestment = e.DeclaredInvestment,
                //         InvestmentDate = e.InvestmentDate,
                //         Narration = e.Narration,
                //         ITInvestment_Val = e.ITInvestment_Id == 0 ? "" : db.ITInvestment.Where(x => x.Id == e.ITInvestment_Id).Select(x => x.FullDetails).FirstOrDefault(),
                //         ITSection_Val = e.ITSection_Id == 0 ? "" : db.ITSection.Where(x => x.Id == e.ITSection_Id).Select(x => x.FullDetails).FirstOrDefault(),
                //         LoanAdvHead_Val = e.LoanAdvanceHead_Id == 0 ? "" : db.LoanAdvanceHead.Where(x => x.Id == e.LoanAdvanceHead_Id).Select(x => x.FullDetails).FirstOrDefault(),
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                //var ITInv = db.ITInvestmentPayment.Find(data);
                //TempData["RowVersion"] = ITInv.RowVersion;
                //var Auth = ITInv.DBTrack.IsModified;
                //return Json(new Object[] { Q, return_data, W, Auth, JsonRequestBehavior.AllowGet });
                var returndata = (Object)null;
                var ListQualDetails = new List<ITInvestPayment_Val>();
                var returnCurrentData = (Object)null;
                if (qurey != null)
                {
                    if (qurey.ITInvestmentPayment != null)
                    {
                        returndata = new
                        {
                            id = qurey.ITInvestmentPayment.Id,
                            ActualInvestment = qurey.ITInvestmentPayment.ActualInvestment,
                            DeclaredInvestment = qurey.ITInvestmentPayment.DeclaredInvestment,
                            InvestmentDate = qurey.ITInvestmentPayment.InvestmentDate,
                            Narration = qurey.ITInvestmentPayment.Narration,
                            Action = qurey.DBTrack.Action,
                            ITInvestment_Id = qurey.ITInvestment == null ? null : qurey.ITInvestment.Id.ToString(),
                            ITInvestment_Val = qurey.ITInvestment == null ? null : qurey.ITInvestment.FullDetails,
                            ITSection_Id = qurey.ITSection == null ? null : qurey.ITSection.Id.ToString(),
                            ITSection_Val = qurey.ITSection == null ? null : qurey.ITSection.FullDetails,
                            LoanAdvHead_Id = qurey.LoanAdvanceHead == null ? null : qurey.LoanAdvanceHead.Id.ToString(),
                            LoanAdvHead_Val = qurey.LoanAdvanceHead == null ? null : qurey.LoanAdvanceHead.FullDetails,

                            isauth = true,
                            Add = false
                        };
                        var k = qurey.ITSubInvestmentPayment.ToList();
                        foreach (var val in k)
                        {
                            ListQualDetails.Add(new ITInvestPayment_Val
                            {
                                ITSubInvestmentPayment_Id = val.Select(e => e.Id.ToString()).ToArray(),
                                ITSubInvestmentPayment_Val = val == null ? null : val.Select(e => e.InvestmentDate.ToString()).ToArray(),
                            });
                        }
                        //curr data
                        var dt_data = db.DT_ITInvestmentPayment.Where(e => e.Orig_Id == qurey.ITInvestmentPayment.Id && e.DBTrack.IsAuthorized == 0).OrderByDescending(e => e.Id).FirstOrDefault();
                        if (dt_data != null)
                        {
                            returnCurrentData = new
                            {
                                DT_Id = dt_data.Id,
                                ActualInvestment = dt_data.ActualInvestment,
                                DeclaredInvestment = dt_data.DeclaredInvestment,
                                InvestmentDate = dt_data.InvestmentDate,
                                Narration = dt_data.Narration,
                                ITInvestment_Val = dt_data.ITInvestment_Id == 0 ? "" : db.ITInvestment.Where(x => x.Id == dt_data.ITInvestment_Id).Select(x => x.FullDetails).FirstOrDefault(),
                                ITSection_Val = dt_data.ITSection_Id == 0 ? "" : db.ITSection.Where(x => x.Id == dt_data.ITSection_Id).Select(x => x.FullDetails).FirstOrDefault(),
                                LoanAdvHead_Val = dt_data.LoanAdvanceHead_Id == 0 ? "" : db.LoanAdvanceHead.Where(x => x.Id == dt_data.LoanAdvanceHead_Id).Select(x => x.FullDetails).FirstOrDefault(),
                                Action = qurey.DBTrack.Action,
                            };
                        }
                    }
                    else
                    {
                        returndata = new
                        {
                            Add = true,
                        };
                    }
                    return Json(new Object[] { returndata, ListQualDetails, returnCurrentData, "", JsonRequestBehavior.AllowGet });
                }
                return Json(new Object[] { returndata, ListQualDetails, returnCurrentData, "", JsonRequestBehavior.AllowGet });
            }
        }
        public string InvestmentUploadFile(string FolderName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\Images\\" + FolderName + "\\";
            String localPath = "";
            bool exists = System.IO.Directory.Exists(requiredPath);
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            return localPath;
        }

        [HttpPost]
        public ActionResult ITnvestmentUpload(HttpPostedFileBase[] files, FormCollection form, string data)
        {
            if (ModelState.IsValid)
            {
                string Id = form["HiddenInvestmentid"] == null ? null : form["HiddenInvestmentid"];
                string SubId = form["HiddenSubinvestment_Id"] == null ? null : form["HiddenSubinvestment_Id"];
                string extension, newfilename, deletefilepath = "";
                Int32 Count = 0;
                string investmentid = "0", subinvestmentid = "0", fromdate = "", todate = "", OFinYr = "";
                ITInvestmentPayment itinvestment = null;
                ITSubInvestmentPayment itsubinvestment = null;
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (Id != null)
                    {
                        int itid = Convert.ToInt32(Id);
                        itinvestment = db.ITInvestmentPayment.Include(e => e.ITInvestment).Include(e => e.FinancialYear).Include(e => e.ITSubInvestmentPayment).Where(e => e.Id == itid).SingleOrDefault();
                        if (itinvestment.ITInvestment != null)
                        {
                            investmentid = itinvestment.ITInvestment.Id.ToString();
                        }
                        OFinYr = itinvestment.FinancialYear.Id.ToString();
                        fromdate = itinvestment.FinancialYear.FromDate.Value.Year.ToString();
                        todate = itinvestment.FinancialYear.ToDate.Value.Year.ToString();
                        deletefilepath = itinvestment.Path;
                    }
                    if (SubId != null && SubId != "")
                    {
                        int subid = Convert.ToInt32(SubId);
                        itsubinvestment = db.ITSubInvestmentPayment.Where(e => e.Id == subid).SingleOrDefault();
                        subinvestmentid = SubId;
                        deletefilepath = itsubinvestment.Path;
                    }
                    var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", ".jpeg", ".pdf" };
                    foreach (HttpPostedFileBase file in files)
                    {
                        if (file == null)
                        {
                            return Json(new { success = false, responseText = "Please Select File..!" }, JsonRequestBehavior.AllowGet);
                        }
                        extension = Path.GetExtension(file.FileName);
                        if (!allowedExtensions.Contains(extension))
                        {
                            return Json(new { success = false, responseText = "File Type Is Not Supported..!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    foreach (HttpPostedFileBase file in files)
                    {

                        if (file != null)
                        {
                            extension = Path.GetExtension(file.FileName);
                            newfilename = OFinYr + "_" + investmentid + "_" + subinvestmentid + "_" + Id + extension; ;
                            String FolderName = "FinancialYear" + fromdate + "-" + todate + "\\Investment\\";

                            //var InputFileName = Path.GetFileName(file.FileName);
                            //string ServerSavePath = InvestmentUploadFile(FolderName);
                            //string ServerMappath = ServerSavePath + newfilename;
                            string ServerSavePath = ConfigurationManager.AppSettings["EmployeeDocuments"];
                            if (ServerSavePath == null)
                            {
                                return Json(new { success = false, responseText = "Please contact the admin to define the document path." }, JsonRequestBehavior.AllowGet);
                            }
                            string ServerMappath = ServerSavePath + FolderName + newfilename;

                            deletefilepath = ServerMappath;

                            if (deletefilepath != null && deletefilepath != "")
                            {
                                FileInfo File = new FileInfo(deletefilepath);

                                bool exists = File.Exists;
                                if (exists)
                                {
                                    System.IO.File.Delete(deletefilepath);
                                }
                            }

                            if (!Directory.Exists(ServerSavePath + FolderName))
                            {
                                Directory.CreateDirectory(ServerSavePath + FolderName);
                            }

                            file.SaveAs(Path.Combine(ServerMappath));

                            //file.SaveAs(Path.Combine(ServerSavePath, newfilename));
                            if (Id != "" && Id != null && SubId == "")
                            {
                                db.ITInvestmentPayment.Attach(itinvestment);
                                db.Entry(itinvestment).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = itinvestment.RowVersion;
                                db.Entry(itinvestment).State = System.Data.Entity.EntityState.Detached;
                                itinvestment.DBTrack = new DBTrack
                                {
                                    CreatedBy = itinvestment.DBTrack.CreatedBy == null ? null : itinvestment.DBTrack.CreatedBy,
                                    CreatedOn = itinvestment.DBTrack.CreatedOn == null ? null : itinvestment.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                ITInvestmentPayment ContactDet = itinvestment;
                                ContactDet.Path = ServerMappath;
                                ContactDet.DBTrack = itinvestment.DBTrack;

                                db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                db.SaveChanges();
                            }
                            else
                            {
                                db.ITSubInvestmentPayment.Attach(itsubinvestment);
                                db.Entry(itsubinvestment).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = itsubinvestment.RowVersion;
                                db.Entry(itsubinvestment).State = System.Data.Entity.EntityState.Detached;
                                itsubinvestment.DBTrack = new DBTrack
                                {
                                    CreatedBy = itsubinvestment.DBTrack.CreatedBy == null ? null : itsubinvestment.DBTrack.CreatedBy,
                                    CreatedOn = itsubinvestment.DBTrack.CreatedOn == null ? null : itsubinvestment.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                ITSubInvestmentPayment ContactDet = itsubinvestment;
                                ContactDet.Path = ServerMappath;
                                ContactDet.DBTrack = itsubinvestment.DBTrack;

                                db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                db.SaveChanges();
                            }
                            Count++;
                        }
                        else
                        {

                        }
                    }
                    if (Count > 0)
                    {
                        return Json(new { success = true, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                    }
                    //else
                    //{
                    //    return Json(new { success = false, responseText = "Something is Wrong..!" }, JsonRequestBehavior.AllowGet);

                    //}
                }
                return View();

            }
            return View();
        }
        [HttpPost]
        public ActionResult ITnvestmentUploadUser(HttpPostedFileBase[] files, FormCollection form, string data)
        {
            if (ModelState.IsValid)
            {
                string Id = form["Investmentid_User"] == null ? null : form["Investmentid_User"];
                string SubId = form["Subinvestment_Id_User"] == null ? null : form["Subinvestment_Id_User"];
                string extension, newfilename, deletefilepath = "";
                Int32 Count = 0;
                string investmentid = "0", subinvestmentid = "0", fromdate = "", todate = "", OFinYr = "";
                ITInvestmentPayment itinvestment = null;
                ITSubInvestmentPayment itsubinvestment = null;
                string EmpId = "";
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (Id != null)
                    {
                        int itid = Convert.ToInt32(Id);
                        itinvestment = db.ITInvestmentPayment.Include(e=>e.EmployeePayroll).Include(e=>e.EmployeePayroll.Employee).Include(e => e.ITInvestment).Include(e => e.FinancialYear).Include(e => e.ITSubInvestmentPayment).Where(e => e.Id == itid).SingleOrDefault();
                        if (itinvestment.ITInvestment != null)
                        {
                            investmentid = itinvestment.ITInvestment.Id.ToString();
                            var EmployeeId = itinvestment.EmployeePayroll.Employee.Id;
                            EmpId = Convert.ToString(EmployeeId);
                        }
                        OFinYr = itinvestment.FinancialYear.Id.ToString();
                        fromdate = itinvestment.FinancialYear.FromDate.Value.Year.ToString();
                        todate = itinvestment.FinancialYear.ToDate.Value.Year.ToString();
                        deletefilepath = itinvestment.Path;
                    }
                    if (SubId != null && SubId != "")
                    {
                        int subid = Convert.ToInt32(SubId);
                        itsubinvestment = db.ITSubInvestmentPayment.Where(e => e.Id == subid).SingleOrDefault();
                        subinvestmentid = SubId;
                        deletefilepath = itsubinvestment.Path;
                    }

                    string Module_Name = Convert.ToString(Session["user-module"]);
                    string ModuleName = Module_Name.ToUpper();

                    var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", ".jpeg", ".pdf" };
                    foreach (HttpPostedFileBase file in files)
                    {
                        extension = Path.GetExtension(file.FileName);
                        if (!allowedExtensions.Contains(extension))
                        {
                            return Json(new { success = false, responseText = "File Type Is Not Supported..!" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    foreach (HttpPostedFileBase file in files)
                    {

                        if (file != null)
                        {
                            extension = Path.GetExtension(file.FileName);                 
                            newfilename = investmentid + "_" + subinvestmentid + "_" + Id + extension;

                            String FolderName = EmpId + "\\" + ModuleName + "\\" + "InvestmentCertificate" + "\\" + "FinancialYear" + fromdate + "-" + todate;
                            string ServerSavePath = ConfigurationManager.AppSettings["EmployeeDocuments"];
                            if (ServerSavePath==null)
                            {
                              return Json(new { success = false, responseText = "Please contact the admin to define the document path." }, JsonRequestBehavior.AllowGet);
                            }
                            //var InputFileName = Path.GetFileName(file.FileName);
                            //string ServerSavePath = InvestmentUploadFile(FolderName);                      
                            //string ServerMappath = ServerSavePath + newfilename;
                            string ServerMappath = ServerSavePath + FolderName +"\\"+ newfilename;
                            deletefilepath = ServerMappath;
                            if (deletefilepath != null && deletefilepath != "")
                            {
                                FileInfo File = new FileInfo(deletefilepath);

                                bool exists = File.Exists;
                                if (exists)
                                {
                                    System.IO.File.Delete(deletefilepath);
                                }
                            }
                            if (!Directory.Exists(ServerSavePath + FolderName))
                            {
                                Directory.CreateDirectory(ServerSavePath + FolderName);
                            }

                            //file.SaveAs(Path.Combine(ServerSavePath, newfilename));
                            file.SaveAs(Path.Combine(ServerMappath));

                            if (Id != "" && Id != null && SubId == "")
                            {
                                db.ITInvestmentPayment.Attach(itinvestment);
                                db.Entry(itinvestment).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = itinvestment.RowVersion;
                                db.Entry(itinvestment).State = System.Data.Entity.EntityState.Detached;
                                itinvestment.DBTrack = new DBTrack
                                {
                                    CreatedBy = itinvestment.DBTrack.CreatedBy == null ? null : itinvestment.DBTrack.CreatedBy,
                                    CreatedOn = itinvestment.DBTrack.CreatedOn == null ? null : itinvestment.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                ITInvestmentPayment ContactDet = itinvestment;
                                ContactDet.Path = FolderName + "\\" + newfilename;
                                ContactDet.DBTrack = itinvestment.DBTrack;

                                db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                db.SaveChanges();
                            }
                            else
                            {
                                db.ITSubInvestmentPayment.Attach(itsubinvestment);
                                db.Entry(itsubinvestment).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = itsubinvestment.RowVersion;
                                db.Entry(itsubinvestment).State = System.Data.Entity.EntityState.Detached;
                                itsubinvestment.DBTrack = new DBTrack
                                {
                                    CreatedBy = itsubinvestment.DBTrack.CreatedBy == null ? null : itsubinvestment.DBTrack.CreatedBy,
                                    CreatedOn = itsubinvestment.DBTrack.CreatedOn == null ? null : itsubinvestment.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                ITSubInvestmentPayment ContactDet = itsubinvestment;
                                ContactDet.Path = FolderName + "\\" + newfilename;
                                ContactDet.DBTrack = itsubinvestment.DBTrack;

                                db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                db.SaveChanges();
                            }
                            Count++;
                        }
                        else
                        {

                        }
                    }
                    if (Count > 0)
                    {
                        return Json(new { success = true, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                    }
                    //else
                    //{
                    //    return Json(new { success = false, responseText = "Something is Wrong..!" }, JsonRequestBehavior.AllowGet);

                    //}
                }
                return View();

            }
            return View();
        }

        [HttpPost]
        public ActionResult Filename(string filepath)
        {
            if (filepath != null && filepath != "")
            {

                return Json(new { data = filepath }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public ActionResult GetCompImage(string filepath)
        {
            //string id = form["HiddenInvestmentid"] == null ? null : form["HiddenInvestmentid"];
            //string SubId = form["HiddenSubinvestment_Id"] == null ? null : form["HiddenSubinvestment_Id"];
            using (DataBaseContext db = new DataBaseContext())
            {
                //string localpath = "";
                //ITInvestmentPayment itinvestment = null;
                //ITSubInvestmentPayment itsubinvestment = null;
                //if (id != null && id != "" && SubId == "")
                //{
                //    int itid = Convert.ToInt32(id);
                //    itinvestment = db.ITInvestmentPayment.Where(e => e.Id == itid).SingleOrDefault();
                //}
                //if (SubId == "")
                //{

                //    if (itinvestment.Path != null)
                //    {
                //        localpath = itinvestment.Path;
                //    }
                //    else
                //    {
                //        return Content("File Not Found");
                //        // return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                //    }
                //}
                //else
                //{
                //    int subid = Convert.ToInt32(SubId);
                //    itsubinvestment = db.ITSubInvestmentPayment.Where(e => e.Id == subid).SingleOrDefault();
                //    if (itsubinvestment.Path != null)
                //    {
                //        localpath = itsubinvestment.Path;
                //    }
                //    else
                //    {
                //        return Content("File Not Found");
                //        //return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                //    }
                //}
                if (filepath != null && filepath != "")
                {
                    FileInfo file1 = new FileInfo(filepath);
                    bool iExists = file1.Exists;
                    if (iExists)
                    {
                        filepath = filepath;
                    }
                    else
                    {
                        filepath = ConfigurationManager.AppSettings["EmployeeDocuments"] + filepath;
                    }
                    FileInfo file = new FileInfo(filepath);
                    bool exists = file.Exists;
                    string extension = Path.GetExtension(file.Name);

                    if (exists)
                    {
                        if (extension == ".pdf")
                        {
                            return File(file.FullName, "application/pdf", file.Name + " ");
                        }
                        if (extension == ".jpg")
                        {
                            return File(file.FullName, "image/png", file.Name + " ");
                            //byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                            //string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                            //return Json(new { data = base64ImageRepresentation, status = true }, JsonRequestBehavior.AllowGet);
                        }
                        if (extension == ".png")
                        {
                            return File(file.FullName, "image/png", file.Name + " ");
                            //byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                            //string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                            //return Json(new { data = base64ImageRepresentation, status = true }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Content("File Not Found");
                        //return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                return null;
            }


        }

        public FileResult PDFFlyer(string filepath)
        {         
            //string path = Server.MapPath(String.Format("~/Content/extras/PDFName.pdf"));
            string path = filepath;
            if (path!=null)
            {
                FileInfo file = new FileInfo(path);
                bool Exist = file.Exists;
                if (Exist)
                {
                    path = path;
                }
                else
                {
                    path = ConfigurationManager.AppSettings["EmployeeDocuments"] + path;
                }
            }
            string mime = MimeMapping.GetMimeMapping(path);
            return File(path, mime);
        }

    }
}