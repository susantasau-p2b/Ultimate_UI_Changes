using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
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
using System.Collections.ObjectModel;
using Payroll;
using Training;
using Attendance;
using Leave;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Leave.MainController
{
    [AuthoriseManger]
    public class LvDebitPolicyController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Leave/MainViews/LvDebitPolicy/Index.cshtml");
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/Leave/_LvSharingPolicy.cshtml");
        }

        public ActionResult PartialPrefix()
        {
            return View("~/Views/Shared/Leave/_PrefixSuffixAction.cshtml");
        }

        [HttpPost]
        public ActionResult GetLvSharingPolicyLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.LvSharingPolicy.Include(e => e.LvHead).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LvSharingPolicy.Include(e => e.LvHead).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }



                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult GetPrefixSuffixActionLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.PrefixSuffixAction.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.PrefixSuffixAction.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookupLvHeadObj(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvHead.ToList();
                IEnumerable<LvHead> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LvHead.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);


            } 
        }


        public ActionResult GetLookupLvHeadObjNew(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.LvHead.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }


        [HttpPost]
        public ActionResult Create(LvDebitPolicy L, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    int comp_Id = 0;
                    //#ConvertLeaveHeadBallist,#ConvertLeaveHeadlist
                    if (L.MinUtilDays > L.MaxUtilDays)
                    {
                        Msg.Add(" Please Enter The Min Days Greater than Max Days. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    //if (db.LvDebitPolicy.Any(e => e.PolicyName.Replace(" ", String.Empty) == L.PolicyName.Replace(" ", String.Empty)))
                    //{
                    //    Msg.Add(" Policy with this name alredy exist.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //}

                    var LeaveHeadslist = form["Head_id"] == "0" ? "" : form["Head_id"];
                    var lvh = Convert.ToInt32(LeaveHeadslist);
                    if (db.LvDebitPolicy.Any(e => e.LvHead.Id == lvh && e.PolicyName == L.PolicyName))
                    {
                        Msg.Add(" Policy name with this Leave Head already exist.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    var CombineLeaveHeadslist = form["CombineLeaveHeadslist"] == "0" ? "" : form["CombineLeaveHeadslist"];
                    var Combined = form["Combined"] == "0" ? "" : form["Combined"];

                    if ((CombineLeaveHeadslist != null) && (L.Combined == false))
                    {
                        Msg.Add(" Combined field contains data though selection is No.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if ((CombineLeaveHeadslist == null) && (L.Combined == true))
                    {
                        Msg.Add(" Combined field contains no data though selection is Yes.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    var LvSharePolicylist = form["LvSharingPolicylist"] == "0" ? "" : form["LvSharingPolicylist"];
                    var DebitShare = form["Sharing"] == "0" ? "" : form["Sharing"];
                    L.IsDebitShare = Convert.ToBoolean(DebitShare);

                    var AutoSanctionAppl = form["IsAutoSanctionAppl"] == "0" ? "" : form["IsAutoSanctionAppl"];
                    L.IsAutoSanctionAppl = Convert.ToBoolean(AutoSanctionAppl);
                    var AutoRecommendAppl = form["IsAutoRecommendAppl"] == "0" ? "" : form["IsAutoRecommendAppl"];
                    L.IsAutoRecommendAppl = Convert.ToBoolean(AutoRecommendAppl);
                    var AutoHRApprovalAppl = form["IsAutoHRApprovalAppl"] == "0" ? "" : form["IsAutoHRApprovalAppl"];
                    L.IsAutoHRApprovalAppl = Convert.ToBoolean(AutoHRApprovalAppl);


                    if ((LvSharePolicylist != null) && (L.IsDebitShare == false))
                    {
                        Msg.Add(" Debit Share field contains data though selection is No.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if ((LvSharePolicylist == null) && (L.IsDebitShare == true))
                    {
                        Msg.Add(" Debit Share field contains no data though selection is Yes.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    var PreApplyPrefixSuffixActionlist = form["PreApplyPrefixSuffixActionlist"] == "0" ? "" : form["PreApplyPrefixSuffixActionlist"];
                    var PostApplyPrefixSuffixActionlist = form["PostApplyPrefixSuffixActionlist"] == "0" ? "" : form["PostApplyPrefixSuffixActionlist"];
                    var PrefixSuffixActionlist = form["PrefixSuffixActionlist"] == "0" ? "" : form["PrefixSuffixActionlist"];
                    var IsHalfDaySession = form["IsHalfDaySession"] == "0" ? "" : form["IsHalfDaySession"];

                    var HalfDayAppl = form["HalfDayAppl"] == "0" ? "" : form["HalfDayAppl"];
                    var HolidayInclusive = form["HolidayInclusive"] == "0" ? "" : form["HolidayInclusive"];
                    var PostApplied = form["PostApplied"] == "0" ? "" : form["PostApplied"];
                    var PostApplyPrefixSuffix = form["PostApplyPrefixSuffix"] == "0" ? "" : form["PostApplyPrefixSuffix"];

                    var PreApplied = form["PreApplied"] == "0" ? "" : form["PreApplied"];
                    var PreApplyPrefixSuffix = form["PreApplyPrefixSuffix"] == "0" ? "" : form["PreApplyPrefixSuffix"];

                    var ServiceLink = form["ServiceLink"] == "0" ? "" : form["ServiceLink"];
                    var PrefixSuffix = form["PrefixSuffix"] == "0" ? "" : form["PrefixSuffix"];
                    var Sandwich = form["Sandwich"] == "0" ? "" : form["Sandwich"];
                    var ExemptAppl = form["ExemptAppl"] == "0" ? "" : form["ExemptAppl"];
                    var ReqseqAppl = form["ReqseqAppl"] == "0" ? "" : form["ReqseqAppl"];
                    var CertificateAppl = form["IsCertificate"] == "0" ? "" : form["IsCertificate"];
                    var PrefixSuffixAction = form["PrefixSuffix"] == "0" ? "" : form["PrefixSuffix"];

                    L.Combined = Convert.ToBoolean(Combined);
                    L.HalfDayAppl = Convert.ToBoolean(HalfDayAppl);
                    L.HolidayInclusive = Convert.ToBoolean(HolidayInclusive);
                    L.PostApplied = Convert.ToBoolean(PostApplied);
                    L.PostApplyPrefixSuffix = Convert.ToBoolean(PostApplyPrefixSuffix);
                    L.IsHalfDaySession_WaiveOffPrefixSuffix = Convert.ToBoolean(IsHalfDaySession);

                    L.PreApplied = Convert.ToBoolean(PreApplied);
                    L.PreApplyPrefixSuffix = Convert.ToBoolean(PreApplyPrefixSuffix);

                    L.PrefixSuffix = Convert.ToBoolean(PrefixSuffix);
                    L.Sandwich = Convert.ToBoolean(Sandwich);

                    L.IsLVCertExemptAppl = Convert.ToBoolean(ExemptAppl);
                    L.IsLVReqSeqexemptAppl = Convert.ToBoolean(ReqseqAppl);

                    L.IsCertificateAppl = Convert.ToBoolean(CertificateAppl);
                    

                    List<CombinedLvHead> ObjLvhead = new List<CombinedLvHead>();
                    if (CombineLeaveHeadslist != null && CombineLeaveHeadslist != " ")
                    {
                        var ids = one_ids(CombineLeaveHeadslist);
                        foreach (var ca in ids)
                        {
                            CombinedLvHead New_Val = db.CombinedLvHead.Include(e => e.LvHead).Where(e => e.LvHead.Id == ca).FirstOrDefault();
                            if (New_Val == null)
                            {
                                L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                LvHead CombLvHead = db.LvHead.Find(ca);
                                CombinedLvHead OBJLVDP = new CombinedLvHead()
                                {
                                    LvHead = CombLvHead,
                                    DBTrack = L.DBTrack
                                };

                                db.CombinedLvHead.Add(OBJLVDP);
                                db.SaveChanges();
                                New_Val = db.CombinedLvHead.Include(e => e.LvHead).Where(e => e.LvHead.Id == ca).FirstOrDefault();
                            }
                            ObjLvhead.Add(New_Val);
                            L.CombinedLvHead = ObjLvhead;
                        }

                    }
                    if (LeaveHeadslist != null && LeaveHeadslist != "")
                    {
                        var value = db.LvHead.Find(int.Parse(LeaveHeadslist));
                        L.LvHead = value;
                    }

                    List<LvSharingPolicy> ObjLvSharePolicy = new List<LvSharingPolicy>();
                    if (LvSharePolicylist != null && LvSharePolicylist != " ")
                    {
                        var ids = one_ids(LvSharePolicylist);
                        foreach (var ca in ids)
                        {
                            LvSharingPolicy New_Val = db.LvSharingPolicy.Where(e => e.Id == ca).FirstOrDefault();

                            ObjLvSharePolicy.Add(New_Val);
                            L.LvSharingPolicy = ObjLvSharePolicy;
                        }

                    }

                    List<PrefixSuffixAction> ObjPreApply = new List<PrefixSuffixAction>();
                    if (PreApplyPrefixSuffixActionlist != null && PreApplyPrefixSuffixActionlist != " ")
                    {
                        var ids = Convert.ToInt32(PreApplyPrefixSuffixActionlist); 
                        PrefixSuffixAction New_Val = db.PrefixSuffixAction.Where(e => e.Id == ids).FirstOrDefault();
                        L.PreApplyPrefixSuffix_PrefixSuffixAction = New_Val;
                    }

                    if ((PreApplyPrefixSuffixActionlist == null) && (L.PreApplyPrefixSuffix == true))
                    {
                        Msg.Add(" Debit Share field contains no data though selection is Yes.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    List<PrefixSuffixAction> ObjPostApply = new List<PrefixSuffixAction>();
                    if (PostApplyPrefixSuffixActionlist != null && PostApplyPrefixSuffixActionlist != " ")
                    {
                        var ids = Convert.ToInt32(PreApplyPrefixSuffixActionlist);
                        PrefixSuffixAction New_Val = db.PrefixSuffixAction.Where(e => e.Id == ids).FirstOrDefault();
                        L.PreApplyPrefixSuffix_PrefixSuffixAction = New_Val;

                    }

                    if ((PostApplyPrefixSuffixActionlist == null) && (L.PostApplyPrefixSuffix == true))
                    {
                        Msg.Add(" Debit Share field contains no data though selection is Yes.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    List<PrefixSuffixAction> ObjPrefixSuffix = new List<PrefixSuffixAction>();
                    if (PrefixSuffixActionlist != null && PrefixSuffixActionlist != " ")
                    {
                        var ids = Convert.ToInt32(PrefixSuffixActionlist);
                        PrefixSuffixAction New_Val = db.PrefixSuffixAction.Where(e => e.Id == ids).FirstOrDefault();
                        L.PrefixSuffix_PrefixSuffixAction = New_Val;

                    }

                    if ((PrefixSuffixActionlist == null) && (L.PrefixSuffix == true))
                    {
                        Msg.Add(" Debit Share field contains no data though selection is Yes.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var companyleave = new CompanyLeave();
                    companyleave = db.CompanyLeave.Where(e => e.Company.Id == comp_Id).SingleOrDefault();

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId };

                            LvDebitPolicy OBJLVDP = new LvDebitPolicy()
                            {
                                ApplyFutureGraceMonths = L.ApplyFutureGraceMonths,
                                ApplyPastGraceMonths = L.ApplyPastGraceMonths,
                                Combined = L.Combined,
                                CombinedLvHead = L.CombinedLvHead,
                                HalfDayAppl = L.HalfDayAppl,
                                HolidayInclusive = L.HolidayInclusive,
                                MaxUtilDays = L.MaxUtilDays,
                                MinUtilDays = L.MinUtilDays,
                                PolicyName = L.PolicyName,
                                PostApplied = L.PostApplied,
                                PostApplyPrefixSuffix = L.PostApplyPrefixSuffix,
                                PostDays = L.PostDays,
                                PreApplied = L.PreApplied,
                                PreApplyPrefixSuffix = L.PreApplyPrefixSuffix,
                                PreDays = L.PreDays,
                                PrefixGraceCount = L.PrefixGraceCount,
                                PrefixMaxCount = L.PrefixMaxCount,
                                PrefixSuffix = L.PrefixSuffix,
                                Sandwich = L.Sandwich,

                                IsLVReqSeqexemptAppl = L.IsLVReqSeqexemptAppl,
                                IsLVCertExemptAppl = L.IsLVCertExemptAppl,

                                YearlyOccurances = L.YearlyOccurances,
                                LvHead = L.LvHead,
                                DBTrack = L.DBTrack,
                                IsDebitShare = L.IsDebitShare,
                                LvSharingPolicy = L.LvSharingPolicy, 
                                PostApplyPrefixSuffix_PrefixSuffixAction = L.PostApplyPrefixSuffix_PrefixSuffixAction, 
                                PreApplyPrefixSuffix_PrefixSuffixAction = L.PreApplyPrefixSuffix_PrefixSuffixAction,
                                PrefixSuffix_PrefixSuffixAction = L.PrefixSuffix_PrefixSuffixAction,
                                MinLvDays = L.MinLvDays,
                                LVReqseqexemptCount = L.LVReqseqexemptCount,
                                LVReqexemptmaxcelling = L.LVReqexemptmaxcelling,
                                IsCertificateAppl = L.IsCertificateAppl,
                                IsHalfDaySession_WaiveOffPrefixSuffix = L.IsHalfDaySession_WaiveOffPrefixSuffix,
                                IsAutoSanctionAppl = L.IsAutoSanctionAppl,
                                IsAutoRecommendAppl = L.IsAutoRecommendAppl,
                                IsAutoHRApprovalAppl = L.IsAutoHRApprovalAppl,
                                AutoSanctionGracePeriod = L.AutoSanctionGracePeriod,
                                AutoRecommendGracePeriod = L.AutoRecommendGracePeriod,
                                AutoHRApprovalGracePeriod = L.AutoHRApprovalGracePeriod,

                            };
                            try
                            {
                                db.LvDebitPolicy.Add(OBJLVDP);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, L.DBTrack);
                                //DT_LvDebitPolicy DT_OBJ = (DT_LvDebitPolicy)rtn_Obj;
                                //db.Create(DT_OBJ);
                                db.SaveChanges();
                                List<LvDebitPolicy> lvdebit_list = new List<LvDebitPolicy>();
                                lvdebit_list.Add(OBJLVDP);
                                if (companyleave != null)
                                {

                                    companyleave.LvDebitPolicy = lvdebit_list;
                                    db.Entry(companyleave).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companyleave).State = System.Data.Entity.EntityState.Detached;

                                }


                                ts.Complete();
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = OBJLVDP.Id, Val = OBJLVDP.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { OBJLVDP.Id, OBJLVDP.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = L.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new { msg = errorMsg });
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

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<LvDebitPolicy> lencash = null;
                if (gp.IsAutho == true)
                {
                    lencash = db.LvDebitPolicy.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    lencash = db.LvDebitPolicy.AsNoTracking().ToList();
                }

                IEnumerable<LvDebitPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = lencash;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString() == gp.searchString)
                            || (e.PolicyName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.MaxUtilDays.ToString() == (gp.searchString))
                            || (e.MinUtilDays.ToString() == (gp.searchString))
                            || (e.YearlyOccurances.ToString() == (gp.searchString))
                            ).Select(a => new Object[] { a.PolicyName, a.MaxUtilDays, a.MinUtilDays, a.YearlyOccurances, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.PolicyName, a.MaxUtilDays, a.MinUtilDays, a.YearlyOccurances, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = lencash;
                    Func<LvDebitPolicy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "PolicyName" ? c.PolicyName.ToString() :
                            gp.sidx == "Get_LvEncashReq" ? c.PolicyName.ToString() :
                             gp.sidx == "MinUtilDays" ? c.PolicyName.ToString() :
                              gp.sidx == "YearlyOccurances" ? c.PolicyName.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.PolicyName), Convert.ToString(a.MaxUtilDays), Convert.ToString(a.MinUtilDays), Convert.ToString(a.YearlyOccurances), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.PolicyName), Convert.ToString(a.MaxUtilDays), Convert.ToString(a.MinUtilDays), Convert.ToString(a.YearlyOccurances), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.PolicyName), Convert.ToString(a.MaxUtilDays), Convert.ToString(a.MinUtilDays), Convert.ToString(a.YearlyOccurances), a.Id }).ToList();
                    }
                    totalRecords = lencash.Count();
                }
                if (totalRecords > 0)
                {
                    totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
                }
                if (gp.page > totalPages)
                {
                    gp.page = totalPages;
                }
                var JsonData = new
                {
                    page = gp.page,
                    rows = jsonData,
                    records = totalRecords,
                    total = totalPages
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public class LvDebitPolicyEditDetails
        {
            public string LvHeadObj_Id { get; set; }

            public string LvHeadObj_FullDetails { get; set; }


        }

        public class LvSharingPolicyEditDetails
        {
            public string LvSharingPolicy_Id { get; set; }

            public string LvSharingPolicy_FullDetails { get; set; }


        }

        //  [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.LvDebitPolicy
                    .Include(e => e.CombinedLvHead)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        ApplyFutureGrace = e.ApplyFutureGraceMonths,
                        ApplyPastGrace = e.ApplyPastGraceMonths,
                        Combined = e.Combined,
                        HalfDayAppl = e.HalfDayAppl,
                        HolidayInclusive = e.HolidayInclusive,
                        IsHalfDaySession_WaiveOffPrefixSuffix = e.IsHalfDaySession_WaiveOffPrefixSuffix,
                        MaxUtilDays = e.MaxUtilDays,
                        MinUtilDays = e.MinUtilDays,
                        PolicyName = e.PolicyName,
                        PostApplied = e.PostApplied,
                        PostApplyPrefixSuffix = e.PostApplyPrefixSuffix,
                        PostDays = e.PostDays,
                        PreApplied = e.PreApplied,
                        PreApplyPrefixSuffix = e.PreApplyPrefixSuffix,
                        PreDays = e.PreDays,
                        PrefixGraceCount = e.PrefixGraceCount,
                        PrefixMaxCount = e.PrefixMaxCount,
                        PrefixSuffix = e.PrefixSuffix,
                        Sandwich = e.Sandwich,
                        IsLVReqSeqexemptAppl = e.IsLVReqSeqexemptAppl,
                        IsLVCertExemptAppl = e.IsLVCertExemptAppl,
                        Lvhead_Id = e.LvHead.Id != null ? e.LvHead.Id : 0,
                        Lvhead_FullDetails = e.LvHead.FullDetails,
                        YearlyOccurances = e.YearlyOccurances,
                        Sharing = e.IsDebitShare,
                        Action = e.DBTrack.Action,
                        PrefixSuffixAction_Id = e.PrefixSuffix_PrefixSuffixAction.Id != null ? e.PrefixSuffix_PrefixSuffixAction.Id :0,
                        PrefixSuffixAction_FullDetails = e.PrefixSuffix_PrefixSuffixAction.FullDetails,
                        PreApplyPrefixSuffixAction_Id = e.PreApplyPrefixSuffix_PrefixSuffixAction.Id != null ? e.PreApplyPrefixSuffix_PrefixSuffixAction.Id : 0,
                        PreApplyPrefixSuffixAction_FullDetails = e.PreApplyPrefixSuffix_PrefixSuffixAction.FullDetails,
                        PostApplyPrefixSuffixAction_Id = e.PostApplyPrefixSuffix_PrefixSuffixAction.Id != null ? e.PostApplyPrefixSuffix_PrefixSuffixAction.Id : 0,
                        PostApplyPrefixSuffixAction_FullDetails = e.PostApplyPrefixSuffix_PrefixSuffixAction.FullDetails,
                        IsCertificateAppl = e.IsCertificateAppl,
                        MinLvDays = e.MinLvDays,
                         LVReqseqexemptCount = e.LVReqseqexemptCount,
                         LVReqexemptmaxcelling = e.LVReqexemptmaxcelling,
                        IsAutoSanctionAppl = e.IsAutoSanctionAppl,
                        IsAutoRecommendAppl = e.IsAutoRecommendAppl,
                        IsAutoHRApprovalAppl = e.IsAutoHRApprovalAppl,
                        AutoSanctionGracePeriod = e.AutoSanctionGracePeriod,
                        AutoRecommendGracePeriod = e.AutoRecommendGracePeriod,
                        AutoHRApprovalGracePeriod = e.AutoHRApprovalGracePeriod,
                    }).SingleOrDefault();

                List<LvDebitPolicyEditDetails> LvHeadObj = new List<LvDebitPolicyEditDetails>();

                var DebPolicy = db.LvDebitPolicy.Include(e => e.CombinedLvHead).Include(e => e.CombinedLvHead.Select(r => r.LvHead)).Where(e => e.Id == data).SingleOrDefault();
                var CLH = DebPolicy.CombinedLvHead.Select(r => r.LvHead).ToList();
                if (CLH != null && CLH.Count > 0)
                {
                    foreach (var ca in CLH)
                    {
                        LvHeadObj.Add(new LvDebitPolicyEditDetails
                        {
                            LvHeadObj_Id = ca.Id.ToString(),
                            LvHeadObj_FullDetails = ca.FullDetails

                        });
                    }

                }

                List<LvSharingPolicyEditDetails> LvSharingPolicy = new List<LvSharingPolicyEditDetails>();

                var SharingPolicyPolicy = db.LvDebitPolicy.Include(e => e.LvSharingPolicy).Include(e => e.LvSharingPolicy.Select(r => r.LvHead)).Where(e => e.Id == data).SingleOrDefault();
                var SharePol = SharingPolicyPolicy.LvSharingPolicy.ToList();
                if (SharePol != null && SharePol.Count > 0)
                {
                    foreach (var ca in SharePol)
                    {
                        LvSharingPolicy.Add(new LvSharingPolicyEditDetails
                        {
                            LvSharingPolicy_Id = ca.Id.ToString(),
                            LvSharingPolicy_FullDetails = ca.FullDetails

                        });
                    }

                }


                var Corp = db.LvDebitPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, LvHeadObj, LvSharingPolicy, Auth }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(LvDebitPolicy L, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    if (L.MinUtilDays > L.MaxUtilDays)
                    {
                        Msg.Add(" Please Enter The Min Days Greater than Max Days. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    var LeaveHeadslist = form["Head_id"] == "0" ? "" : form["Head_id"];
                    var CombineLeaveHeadslist = form["CombineLeaveHeadslist"] == "0" ? "" : form["CombineLeaveHeadslist"];
                    var Combined = form["Combined"] == "0" ? "" : form["Combined"];
                    var DebitShare = form["Sharing"] == "0" ? "" : form["Sharing"];
                    var LvSharePolicylist = form["LvSharingPolicylist"] == "0" ? "" : form["LvSharingPolicylist"];
                    //  L.Combined = Convert.ToBoolean(Combined);
                    if ((CombineLeaveHeadslist != null) && (L.Combined == false))
                    {
                        Msg.Add(" Combined field contains data though selection is No.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if ((LvSharePolicylist == null) && (L.IsDebitShare == true))
                    {
                        Msg.Add(" Sharing Policy field contains no data though selection is Yes.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (LeaveHeadslist != null && LeaveHeadslist != "")
                    {
                        var value = db.LvHead.Find(int.Parse(LeaveHeadslist));

                        L.LvHead = value;

                    }
                    if (LeaveHeadslist != null && LeaveHeadslist != "")
                    { 
                        L.LvHead_Id = int.Parse(LeaveHeadslist); 
                    }

                    var HalfDayAppl = form["HalfDayAppl"] == "0" ? "" : form["HalfDayAppl"];
                    var HolidayInclusive = form["HolidayInclusive"] == "0" ? "" : form["HolidayInclusive"];
                    var IsHalfDaySession = form["IsHalfDaySession"] == "0" ? "" : form["IsHalfDaySession"];
                    var PostApplied = form["PostApplied"] == "0" ? "" : form["PostApplied"];
                    var PostApplyPrefixSuffix = form["PostApplyPrefixSuffix"] == "0" ? "" : form["PostApplyPrefixSuffix"];

                    var PreApplied = form["PreApplied"] == "0" ? "" : form["PreApplied"];
                    var PreApplyPrefixSuffix = form["PreApplyPrefixSuffix"] == "0" ? "" : form["PreApplyPrefixSuffix"];

                    var ServiceLink = form["ServiceLink"] == "0" ? "" : form["ServiceLink"];
                    var PrefixSuffix = form["PrefixSuffix"] == "0" ? "" : form["PrefixSuffix"];
                    var Sandwich = form["Sandwich"] == "0" ? "" : form["Sandwich"];
                    var ExemptAppl = form["ExemptAppl"] == "0" ? "" : form["ExemptAppl"];
                    var ReqseqAppl = form["ReqseqAppl"] == "0" ? "" : form["ReqseqAppl"];
                    var CertificateAppl = form["IsCertificate"] == "0" ? "" : form["IsCertificate"];

                    var AutoSanctionAppl = form["IsAutoSanctionAppl"] == "0" ? "" : form["IsAutoSanctionAppl"];
                    L.IsAutoSanctionAppl = Convert.ToBoolean(AutoSanctionAppl);
                    var AutoRecommendAppl = form["IsAutoRecommendAppl"] == "0" ? "" : form["IsAutoRecommendAppl"];
                    L.IsAutoRecommendAppl = Convert.ToBoolean(AutoRecommendAppl);
                    var AutoHRApprovalAppl = form["IsAutoHRApprovalAppl"] == "0" ? "" : form["IsAutoHRApprovalAppl"];
                    L.IsAutoHRApprovalAppl = Convert.ToBoolean(AutoHRApprovalAppl);


                    L.Combined = Convert.ToBoolean(Combined);
                    L.HalfDayAppl = Convert.ToBoolean(HalfDayAppl);
                    L.HolidayInclusive = Convert.ToBoolean(HolidayInclusive);
                    L.PostApplied = Convert.ToBoolean(PostApplied);
                    L.PostApplyPrefixSuffix = Convert.ToBoolean(PostApplyPrefixSuffix);
                    L.IsHalfDaySession_WaiveOffPrefixSuffix = Convert.ToBoolean(IsHalfDaySession);
                    L.PreApplied = Convert.ToBoolean(PreApplied);
                    L.PreApplyPrefixSuffix = Convert.ToBoolean(PreApplyPrefixSuffix);

                    L.PrefixSuffix = Convert.ToBoolean(PrefixSuffix);
                    L.Sandwich = Convert.ToBoolean(Sandwich);

                    L.IsLVCertExemptAppl = Convert.ToBoolean(ExemptAppl);
                    L.IsLVReqSeqexemptAppl = Convert.ToBoolean(ReqseqAppl);

                    L.IsDebitShare = Convert.ToBoolean(DebitShare);
                    L.IsCertificateAppl = Convert.ToBoolean(CertificateAppl);

                    var PreApplyPrefixSuffixActionlist = form["PreApplyPrefixSuffixActionlist"] == "0" ? "" : form["PreApplyPrefixSuffixActionlist"];
                    var PostApplyPrefixSuffixActionlist = form["PostApplyPrefixSuffixActionlist"] == "0" ? "" : form["PostApplyPrefixSuffixActionlist"];
                    var PrefixSuffixActionlist = form["PrefixSuffixActionlist"] == "0" ? "" : form["PrefixSuffixActionlist"];


                    List<CombinedLvHead> ObjLvhead = new List<CombinedLvHead>();
                    if (CombineLeaveHeadslist != null && CombineLeaveHeadslist != " ")
                    {
                        var ids = one_ids(CombineLeaveHeadslist);
                        foreach (var ca in ids)
                        {
                            CombinedLvHead New_Val = db.CombinedLvHead.Include(e => e.LvHead).Where(e => e.LvHead.Id == ca).FirstOrDefault();
                            if (New_Val == null)
                            {
                                L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                LvHead CombLvHead = db.LvHead.Find(ca);

                                CombinedLvHead OBJLVDP = new CombinedLvHead()
                                {
                                    LvHead = CombLvHead,
                                    DBTrack = L.DBTrack
                                };

                                db.CombinedLvHead.Add(OBJLVDP);
                                db.SaveChanges();
                                New_Val = db.CombinedLvHead.Include(e => e.LvHead).Where(e => e.LvHead.Id == ca).FirstOrDefault();
                            }
                            ObjLvhead.Add(New_Val);
                            L.CombinedLvHead = ObjLvhead;
                        }

                    }

                   
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    LvDebitPolicy blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.LvDebitPolicy.Where(e => e.Id == data).Include(e => e.CombinedLvHead)
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    L.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };


                                    List<CombinedLvHead> LvHeadObj = new List<CombinedLvHead>();
                                    LvDebitPolicy lvheaddetails = null;
                                    lvheaddetails = db.LvDebitPolicy.Include(e => e.CombinedLvHead).Where(e => e.Id == data).SingleOrDefault();
                                    if (CombineLeaveHeadslist != null && CombineLeaveHeadslist != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(CombineLeaveHeadslist);
                                        foreach (var ca in ids)
                                        {
                                            var CombineLeaveHeadslistvalue = db.CombinedLvHead.Include(e => e.LvHead).Where(e => e.LvHead.Id == ca).FirstOrDefault();
                                            LvHeadObj.Add(CombineLeaveHeadslistvalue);
                                            lvheaddetails.CombinedLvHead = LvHeadObj;
                                        }
                                    }
                                    else
                                    {
                                        lvheaddetails.CombinedLvHead = null;
                                    }

                                    List<LvSharingPolicy> LvSharingPolicy = new List<LvSharingPolicy>();
                                    lvheaddetails = null;
                                    lvheaddetails = db.LvDebitPolicy.Include(e => e.LvSharingPolicy).Where(e => e.Id == data).SingleOrDefault();
                                    if (LvSharePolicylist != null && LvSharePolicylist != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(LvSharePolicylist);
                                        foreach (var ca in ids)
                                        {
                                            var LvSharingPolicylistvalue = db.LvSharingPolicy.Include(e => e.LvHead).Where(e => e.Id == ca).FirstOrDefault();
                                            LvSharingPolicy.Add(LvSharingPolicylistvalue);
                                            lvheaddetails.LvSharingPolicy = LvSharingPolicy;
                                        }
                                    }
                                    else
                                    {
                                        lvheaddetails.LvSharingPolicy = null;
                                    }

                                    List<PrefixSuffixAction> ObjPreApply = new List<PrefixSuffixAction>();
                                    if (PreApplyPrefixSuffixActionlist != null && PreApplyPrefixSuffixActionlist != " ")
                                    {
                                        var ids = Convert.ToInt32(PreApplyPrefixSuffixActionlist);
                                        PrefixSuffixAction New_Val = db.PrefixSuffixAction.Where(e => e.Id == ids).FirstOrDefault();
                                        lvheaddetails.PreApplyPrefixSuffix_PrefixSuffixAction = New_Val;
                                    }

                                    if (PrefixSuffixActionlist != null && PrefixSuffixActionlist != " ")
                                    {
                                        var ids = Convert.ToInt32(PrefixSuffixActionlist);
                                        PrefixSuffixAction New_Val = db.PrefixSuffixAction.Where(e => e.Id == ids).FirstOrDefault();
                                        lvheaddetails.PrefixSuffix_PrefixSuffixAction = New_Val;
                                    }

                                    List<PrefixSuffixAction> ObjPostApply = new List<PrefixSuffixAction>();
                                    if (PostApplyPrefixSuffixActionlist != null && PostApplyPrefixSuffixActionlist != " ")
                                    {
                                        var ids = Convert.ToInt32(PostApplyPrefixSuffixActionlist);
                                        PrefixSuffixAction New_Val = db.PrefixSuffixAction.Where(e => e.Id == ids).FirstOrDefault();
                                        lvheaddetails.PostApplyPrefixSuffix_PrefixSuffixAction = New_Val;

                                    }

                                    db.LvDebitPolicy.Attach(lvheaddetails);
                                    db.Entry(lvheaddetails).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = lvheaddetails.RowVersion;
                                    db.Entry(lvheaddetails).State = System.Data.Entity.EntityState.Detached;

                                    var CurCorp = db.LvDebitPolicy.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        LvDebitPolicy LvDebitPolicy = new LvDebitPolicy()
                                        {
                                            LvHead = L.LvHead,
                                            LvHead_Id = L.LvHead_Id,
                                            ApplyFutureGraceMonths = L.ApplyFutureGraceMonths,
                                            ApplyPastGraceMonths = L.ApplyPastGraceMonths,
                                            Combined = L.Combined,
                                            //CombinedLvHead = L.CombinedLvHead,
                                            HalfDayAppl = L.HalfDayAppl,
                                            HolidayInclusive = L.HolidayInclusive,
                                            MaxUtilDays = L.MaxUtilDays,
                                            MinUtilDays = L.MinUtilDays,
                                            PolicyName = L.PolicyName,
                                            PostApplied = L.PostApplied,
                                            PostApplyPrefixSuffix = L.PostApplyPrefixSuffix,
                                            PostDays = L.PostDays,
                                            PreApplied = L.PreApplied,
                                            PreApplyPrefixSuffix = L.PreApplyPrefixSuffix,
                                            PreDays = L.PreDays,
                                            PrefixGraceCount = L.PrefixGraceCount,
                                            PrefixMaxCount = L.PrefixMaxCount,
                                            PrefixSuffix = L.PrefixSuffix,
                                            Sandwich = L.Sandwich,
                                            IsLVReqSeqexemptAppl = L.IsLVReqSeqexemptAppl,
                                            IsLVCertExemptAppl = L.IsLVCertExemptAppl,
                                            YearlyOccurances = L.YearlyOccurances,
                                            IsDebitShare = L.IsDebitShare,
                                            Id = data,
                                            DBTrack = L.DBTrack,
                                            MinLvDays = L.MinLvDays,
                                            LVReqseqexemptCount = L.LVReqseqexemptCount,
                                            LVReqexemptmaxcelling = L.LVReqexemptmaxcelling,
                                            IsCertificateAppl = L.IsCertificateAppl,
                                            IsHalfDaySession_WaiveOffPrefixSuffix=L.IsHalfDaySession_WaiveOffPrefixSuffix,
                                            IsAutoSanctionAppl = L.IsAutoSanctionAppl,
                                            IsAutoRecommendAppl = L.IsAutoRecommendAppl,
                                            IsAutoHRApprovalAppl = L.IsAutoHRApprovalAppl,
                                            AutoSanctionGracePeriod = L.AutoSanctionGracePeriod,
                                            AutoRecommendGracePeriod = L.AutoRecommendGracePeriod,
                                            AutoHRApprovalGracePeriod = L.AutoHRApprovalGracePeriod,

                                        };
                                        db.LvDebitPolicy.Attach(LvDebitPolicy);
                                        db.Entry(LvDebitPolicy).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(LvDebitPolicy).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }
                                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                    using (var context = new DataBaseContext())
                                    {
                                        //var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                        //DT_LvDebitPolicy DT_Corp = (DT_LvDebitPolicy)obj;
                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (LvCreditPolicy)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (LvEncashReq)databaseEntry.ToObject();
                                    L.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            LvDebitPolicy blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            LvDebitPolicy Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.LvDebitPolicy.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            L.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            LvDebitPolicy corp = new LvDebitPolicy()
                            {
                                ApplyFutureGraceMonths = L.ApplyFutureGraceMonths,
                                ApplyPastGraceMonths = L.ApplyPastGraceMonths,
                                Combined = L.Combined,
                                CombinedLvHead = L.CombinedLvHead,
                                HalfDayAppl = L.HalfDayAppl,
                                HolidayInclusive = L.HolidayInclusive,
                                MaxUtilDays = L.MaxUtilDays,
                                MinUtilDays = L.MinUtilDays,
                                PolicyName = L.PolicyName,
                                PostApplied = L.PostApplied,
                                PostApplyPrefixSuffix = L.PostApplyPrefixSuffix,
                                PostDays = L.PostDays,
                                PreApplied = L.PreApplied,
                                PreApplyPrefixSuffix = L.PreApplyPrefixSuffix,
                                PreDays = L.PreDays,
                                PrefixGraceCount = L.PrefixGraceCount,
                                PrefixMaxCount = L.PrefixMaxCount,
                                PrefixSuffix = L.PrefixSuffix,
                                Sandwich = L.Sandwich,
                                IsLVReqSeqexemptAppl = L.IsLVReqSeqexemptAppl,
                                IsLVCertExemptAppl = L.IsLVCertExemptAppl,
                                YearlyOccurances = L.YearlyOccurances,
                                IsHalfDaySession_WaiveOffPrefixSuffix=L.IsHalfDaySession_WaiveOffPrefixSuffix,
                                
                                IsAutoSanctionAppl = L.IsAutoSanctionAppl,
                                IsAutoRecommendAppl = L.IsAutoRecommendAppl,
                                IsAutoHRApprovalAppl = L.IsAutoHRApprovalAppl,
                                AutoSanctionGracePeriod = L.AutoSanctionGracePeriod,
                                AutoRecommendGracePeriod = L.AutoRecommendGracePeriod,
                                AutoHRApprovalGracePeriod = L.AutoHRApprovalGracePeriod,
                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvDebitPolicy", L.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.LvDebitPolicy.Where(e => e.Id == data).Include(e => e.CombinedLvHead)
                                    .SingleOrDefault();
                                DT_LvDebitPolicy DT_Corp = (DT_LvDebitPolicy)obj;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = L.DBTrack;
                            db.LvDebitPolicy.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                        }

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
                return View();
            }
        }


        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    LvDebitPolicy OBJLvDebitPolicy = db.LvDebitPolicy.Include(e => e.CombinedLvHead).Include(e => e.LvHead)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    var a = db.LvNewReq.Include(e => e.LeaveHead)
                     .Where(e => e.LeaveHead.Id == OBJLvDebitPolicy.LvHead.Id).OrderBy(e => e.Id).AsNoTracking().AsParallel().ToList();
                    if (a.Count > 0)
                    {
                        Msg.Add(" Reference for this record exists.Cannot remove it..  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (OBJLvDebitPolicy.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = OBJLvDebitPolicy.DBTrack.CreatedBy != null ? OBJLvDebitPolicy.DBTrack.CreatedBy : null,
                                CreatedOn = OBJLvDebitPolicy.DBTrack.CreatedOn != null ? OBJLvDebitPolicy.DBTrack.CreatedOn : null,
                                IsModified = OBJLvDebitPolicy.DBTrack.IsModified == true ? true : false
                            };
                            OBJLvDebitPolicy.DBTrack = dbT;
                            db.Entry(OBJLvDebitPolicy).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, OBJLvDebitPolicy.DBTrack);
                            DT_LvDebitPolicy DT_Corp = (DT_LvDebitPolicy)rtn_Obj;

                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        var CombineLeaveHeads = OBJLvDebitPolicy.CombinedLvHead;


                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (CombineLeaveHeads != null)
                            {
                                var LvDebitPolicy = new HashSet<int>(OBJLvDebitPolicy.CombinedLvHead.Select(e => e.Id));
                                if (LvDebitPolicy.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                }
                            }
                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = OBJLvDebitPolicy.DBTrack.CreatedBy != null ? OBJLvDebitPolicy.DBTrack.CreatedBy : null,
                                    CreatedOn = OBJLvDebitPolicy.DBTrack.CreatedOn != null ? OBJLvDebitPolicy.DBTrack.CreatedOn : null,
                                    IsModified = OBJLvDebitPolicy.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.EmpId,
                                    //AuthorizedOn = DateTime.Now
                                };
                                db.Entry(OBJLvDebitPolicy).State = System.Data.Entity.EntityState.Deleted;
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, OBJLvDebitPolicy.DBTrack);
                                //DT_LvDebitPolicy DT_Corp = (DT_LvDebitPolicy)rtn_Obj;
                                //db.Create(DT_Corp);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                            }
                        }
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