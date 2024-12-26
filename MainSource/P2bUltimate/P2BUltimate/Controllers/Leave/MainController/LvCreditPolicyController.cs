
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
    public class LvCreditPolicyController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Leave/MainViews/LvCreditPolicy/Index.cshtml");
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
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

        #region CREATE

        [HttpPost]
        public ActionResult Create(LvCreditPolicy L, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    // List<string>aa2=new List<string>();
                    // var   aa21 = db.LvCreditPolicy.ToList().ForEach(a => a.PolicyName = a.PolicyName.Trim().Replace(" ", String.Empty));
                    //   var aas=L.PolicyName.Replace(" ", String.Empty);
                    //if (db.LvCreditPolicy.Any(e => e.PolicyName.Replace(" ", String.Empty) == L.PolicyName.Replace(" ", String.Empty)))
                    //{
                    //    Msg.Add(" Policy with this name alredy exist.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //}
                    var lvhead = form["Head_id"] == "0" ? "" : form["Head_id"];
                    var lvh = Convert.ToInt32(lvhead);
                    //if (db.LvCreditPolicy.Any(e => e.LvHead.Id == lvh))
                    //{
                    //    Msg.Add(" Policy with this Leave Head already exist.");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                    //#ConvertLeaveHeadBallist,#ConvertLeaveHeadlist
                    var ExcludeLeaveHeadslist = form["ExcludeLeaveHeadslist"] == "0" ? "" : form["ExcludeLeaveHeadslist"];
                    var ConvertLeaveHeadBallist = form["ConvertLeaveHeadBallist"] == "0" ? "" : form["ConvertLeaveHeadBallist"];
                    var ConvertLeaveHeadlist = form["ConvertLeaveHeadlist"] == "0" ? "" : form["ConvertLeaveHeadlist"];
                    var CreditDatelist = form["CreditDatelist"] == "0" ? "" : form["CreditDatelist"];
                    var AboveServiceMaxYears = form["AboveServiceMaxYears"] == "0" ? "" : form["AboveServiceMaxYears"];
                    var Accumalation = form["Accumalation"] == "0" ? "" : form["Accumalation"];
                    var ExcludeLeaves = form["ExcludeLeaves"] == "0" ? "" : form["ExcludeLeaves"];
                    var IsCreditDatePolicy = form["IsCreditDatePolicy"] == "0" ? "" : form["IsCreditDatePolicy"];
                    var FixedCreditDays = form["FixedCreditDays"] == "0" ? "" : form["FixedCreditDays"];
                    var LVConvert = form["LVConvert"] == "0" ? "" : form["LVConvert"];
                    var LVConvertBal = form["LVConvertBal"] == "0" ? "" : form["LVConvertBal"];
                    var OccCarryForward = form["OccCarryForward"] == "0" ? "" : form["OccCarryForward"];
                    var OccInServAppl = form["OccInServAppl"] == "0" ? "" : form["OccInServAppl"];
                    var ServiceLink = form["ServiceLink"] == "0" ? "" : form["ServiceLink"];
                    var AccumulationWithCredit = form["AccumulationWithCredit"] == "0" ? "" : form["AccumulationWithCredit"];
                    var ProdataFlag = form["ProdataFlag"] == "0" ? "" : form["ProdataFlag"];
                    var ServicemaxYearsLimit = form["ServicemaxYearsLimit"] == "0" ? "" : form["ServicemaxYearsLimit"];
                    L.AboveServiceMaxYears = Convert.ToBoolean(AboveServiceMaxYears);
                    L.Accumalation = Convert.ToBoolean(Accumalation);
                    L.ExcludeLeaves = Convert.ToBoolean(ExcludeLeaves);
                    L.IsCreditDatePolicy = Convert.ToBoolean(IsCreditDatePolicy);
                    L.FixedCreditDays = Convert.ToBoolean(FixedCreditDays);
                    L.LVConvert = Convert.ToBoolean(LVConvert);
                    L.LVConvertBal = Convert.ToBoolean(LVConvertBal);
                    L.OccCarryForward = Convert.ToBoolean(OccCarryForward);
                    L.OccInServAppl = Convert.ToBoolean(OccInServAppl);
                    L.ServiceLink = Convert.ToBoolean(ServiceLink);
                    L.AccumulationWithCredit = Convert.ToBoolean(AccumulationWithCredit);
                    L.ProdataFlag = Convert.ToBoolean(ProdataFlag);
                   

                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);

                    var companyLeave = new CompanyLeave();
                    companyLeave = db.CompanyLeave.Where(e => e.Company.Id == comp_Id).SingleOrDefault();
                    if (CreditDatelist != null && CreditDatelist != "-Select-" && CreditDatelist != "")
                    {
                        var value = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "447").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(CreditDatelist)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(CreditDatelist));
                        L.CreditDate = value;


                    }

                    if (lvhead != null && lvhead != "")
                    {
                        var value = db.LvHead.Find(int.Parse(lvhead));
                        L.LvHead = value;
                    }
                    if (ConvertLeaveHeadBallist != null && ConvertLeaveHeadBallist != "")
                    {
                        var value = db.LvHead.Find(int.Parse(ConvertLeaveHeadBallist));
                        L.ConvertLeaveHead = value;
                    }
                    if (ConvertLeaveHeadlist != null && ConvertLeaveHeadlist != "")
                    {
                        var value = db.LvHead.Find(int.Parse(ConvertLeaveHeadlist));
                        L.ConvertLeaveHeadBal = value;
                    }
                    List<LvHead> ObjLvheadELH = new List<LvHead>();


                    if (ExcludeLeaveHeadslist != null && ExcludeLeaveHeadslist != " ")
                    {
                        var ids = one_ids(ExcludeLeaveHeadslist);
                        foreach (var ca in ids)
                        {
                            var value = db.LvHead.Find(ca);
                            ObjLvheadELH.Add(value);
                            L.ExcludeLeaveHeads = ObjLvheadELH;
                        }

                    }
                    
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {


                            L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId };

                            LvCreditPolicy OBJLVCP = new LvCreditPolicy()
                            {
                                LvHead = L.LvHead,
                                AboveServiceMaxYears = L.AboveServiceMaxYears,
                                AboveServiceSteps = L.AboveServiceSteps,
                                Accumalation = L.Accumalation,
                                AccumalationLimit = L.AccumalationLimit,
                                AccumulationWithCredit = L.AccumulationWithCredit,
                                ConvertedDays = L.ConvertedDays,
                                ConvertLeaveHead = L.ConvertLeaveHead,
                                ConvertLeaveHeadBal = L.ConvertLeaveHeadBal,
                                CreditDate = L.CreditDate,
                                CreditDays = L.CreditDays,
                                ExcludeLeaveHeads = L.ExcludeLeaveHeads,
                                ExcludeLeaves = L.ExcludeLeaves,
                                FixedCreditDays = L.FixedCreditDays,
                                IsCreditDatePolicy = L.IsCreditDatePolicy,
                                LVConvert = L.LVConvert,
                                LVConvertBal = L.LVConvertBal,
                                LvConvertLimit = L.LvConvertLimit,
                                LvConvertLimitBal = L.LvConvertLimitBal,
                                MaxLeaveDebitInService = L.MaxLeaveDebitInService,
                                ServicemaxYearsLimit = L.ServicemaxYearsLimit,
                                OccCarryForward = L.OccCarryForward,
                                OccInServAppl = L.OccInServAppl,
                                OccInServ = L.OccInServ,
                                PolicyName = L.PolicyName,
                                ProCreditFrequency = L.ProCreditFrequency,
                                ProdataFlag = L.ProdataFlag,
                                ServiceLink = L.ServiceLink,
                                ServiceYearsLimit = L.ServiceYearsLimit,
                                WorkingDays = L.WorkingDays,
                                DBTrack = L.DBTrack
                            };
                            try
                            {
                                db.LvCreditPolicy.Add(OBJLVCP);
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, L.DBTrack);
                                //DT_LvCreditPolicy DT_OBJ = (DT_LvCreditPolicy)rtn_Obj;
                                //db.Create(DT_OBJ);
                                db.SaveChanges();
                                List<LvCreditPolicy> crditpolicylist = new List<LvCreditPolicy>();
                                crditpolicylist.Add(OBJLVCP);
                                if (companyLeave != null)
                                {
                                    companyLeave.LvCreditPolicy = crditpolicylist;
                                    db.Entry(companyLeave).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companyLeave).State = System.Data.Entity.EntityState.Detached;


                                }
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = OBJLVCP.Id, Val = OBJLVCP.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { OBJLVCP.Id, OBJLVCP.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
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

                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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
        #endregion

        #region EDIT & EDITSAVE

        public class LvCreditPolicyEditDetails
        {
            public Array LvHeadObjELH_Id { get; set; }

            public Array LvHeadObjELH_FullDetails { get; set; }

            public string LvHeadObjCLH_Id { get; set; }

            public string LvHeadObjCLH_FullDetails { get; set; }

            public string LvHeadObjCLHB_Id { get; set; }

            public string LvHeadObjCLHB_FullDetails { get; set; }
        }



        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.LvCreditPolicy
                    .Include(e => e.LvHead)
                    .Include(e => e.CreditDate)
                    .Include(e => e.ConvertLeaveHead)
                    .Include(e => e.ConvertLeaveHeadBal)
                    .Include(e => e.ExcludeLeaveHeads)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        AboveServiceMaxYears = e.AboveServiceMaxYears,
                        AboveServiceSteps = e.AboveServiceSteps,
                        Accumalation = e.Accumalation,
                        AccumalationLimit = e.AccumalationLimit,
                        AccumulationWithCredit = e.AccumulationWithCredit,
                        ConvertedDays = e.ConvertedDays,
                        CreditDays = e.CreditDays,
                        ExcludeLeaves = e.ExcludeLeaves,
                        FixedCreditDays = e.FixedCreditDays,
                        IsCreditDatePolicy = e.IsCreditDatePolicy,
                        LVConvert = e.LVConvert,
                        LVConvertBal = e.LVConvertBal,
                        LvConvertLimit = e.LvConvertLimit,
                        LvConvertLimitBal = e.LvConvertLimitBal,
                        MaxLeaveDebitInService = e.MaxLeaveDebitInService,
                        ServicemaxYearsLimit= e.ServicemaxYearsLimit,
                        OccCarryForward = e.OccCarryForward,
                        OccInServAppl = e.OccInServAppl,
                        OccInServ = e.OccInServ,
                        PolicyName = e.PolicyName,
                        ProCreditFrequency = e.ProCreditFrequency,
                        ProdataFlag = e.ProdataFlag,
                        ServiceLink = e.ServiceLink,
                        ServiceYearsLimit = e.ServiceYearsLimit,
                        WorkingDays = e.WorkingDays,
                        CreditDate_Id = e.CreditDate.Id == null ? 0 : e.CreditDate.Id,
                        //ConvertLvHead_Id = e.ConvertLeaveHead.Id == null ? 0 : e.ConvertLeaveHead.Id,
                        //ConvertLvHeadBal_Id = e.ConvertLeaveHeadBal.Id == null ? 0 : e.ConvertLeaveHeadBal.Id,
                        LvHead_Id = e.LvHead == null ? 0 : e.LvHead.Id,
                        Lvhead_FullDetails = e.LvHead.LvName,
                        Action = e.DBTrack.Action
                    }).ToList();

                List<LvCreditPolicyEditDetails> LvHeadObj = new List<LvCreditPolicyEditDetails>();

               
                var ELH = db.LvCreditPolicy.Include(e => e.ExcludeLeaveHeads).Where(e => e.Id == data).Select(e => e.ExcludeLeaveHeads).ToList();
                if (ELH != null)
                {
                    foreach (var ca in ELH)
                    {
                        LvHeadObj.Add(new LvCreditPolicyEditDetails
                        {
                            LvHeadObjELH_Id = ca.Select(e => e.Id).ToArray(),
                            LvHeadObjELH_FullDetails = ca.Select(e => e.FullDetails).ToArray(),

                        });
                    } 
                }

                var CLH = db.LvCreditPolicy.Include(e => e.ConvertLeaveHead).Where(e => e.Id == data).FirstOrDefault();
                if (CLH != null && CLH.ConvertLeaveHead != null)
                {
                    LvHeadObj.Add(new LvCreditPolicyEditDetails
                    {
                        LvHeadObjCLH_Id = CLH.ConvertLeaveHead.Id.ToString(),
                        LvHeadObjCLH_FullDetails = CLH.ConvertLeaveHead.FullDetails

                    });
                }

                var CLHB = db.LvCreditPolicy.Include(e => e.ConvertLeaveHeadBal).Where(e => e.Id == data).FirstOrDefault();
                if (CLHB != null && CLHB.ConvertLeaveHeadBal != null)
                { 
                        LvHeadObj.Add(new LvCreditPolicyEditDetails
                        {
                            LvHeadObjCLHB_Id = CLHB.ConvertLeaveHeadBal.Id.ToString(),
                            LvHeadObjCLHB_FullDetails = CLHB.ConvertLeaveHeadBal.FullDetails

                        }); 
                }
                var W = db.DT_LvCreditPolicy
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         WFStatus_Val = e.CreditDate_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.CreditDate_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.LvCreditPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, LvHeadObj, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(LvCreditPolicy L, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string ExcludeLeaveHeadslist = form["ExcludeLeaveHeadslist"] == "0" ? "" : form["ExcludeLeaveHeadslist"];
                    string ConvertLeaveHeadBallist = form["ConvertLeaveHeadBallist"] == "0" ? "" : form["ConvertLeaveHeadBallist"];
                    string ConvertLeaveHeadlist = form["ConvertLeaveHeadlist"] == "0" ? "" : form["ConvertLeaveHeadlist"];
                    string CreditDatelist = form["CreditDatelist"] == "0" ? "" : form["CreditDatelist"];
                    var AboveServiceMaxYears = form["AboveServiceMaxYears"] == "0" ? "" : form["AboveServiceMaxYears"];
                    var Accumalation = form["Accumalation"] == "0" ? "" : form["Accumalation"];
                    var ExcludeLeaves = form["ExcludeLeaves"] == "0" ? "" : form["ExcludeLeaves"];
                    var IsCreditDatePolicy = form["IsCreditDatePolicy"] == "0" ? "" : form["IsCreditDatePolicy"];
                    var FixedCreditDays = form["FixedCreditDays"] == "0" ? "" : form["FixedCreditDays"];
                    var LVConvert = form["LVConvert"] == "0" ? "" : form["LVConvert"];
                    var LVConvertBal = form["LVConvertBal"] == "0" ? "" : form["LVConvertBal"];
                    var OccCarryForward = form["OccCarryForward"] == "0" ? "" : form["OccCarryForward"];
                    var OccInServAppl = form["OccInServAppl"] == "0" ? "" : form["OccInServAppl"];
                    var ServiceLink = form["ServiceLink"] == "0" ? "" : form["ServiceLink"];
                    var AccumulationWithCredit = form["AccumulationWithCredit"] == "0" ? "" : form["AccumulationWithCredit"];
                    var ProdataFlag = form["ProdataFlag"] == "0" ? "" : form["ProdataFlag"];
                    var lvhead = form["Head_id"] == "0" ? "" : form["Head_id"];
                   // var ServicemaxYearsLimit = form["ServicemaxYearsLimit"] == "0" ? "" : form["ServicemaxYearsLimit"];
                    //var MaxLeaveDebitInService = form["MaxLeaveDebitInService"] == "0" ? "" : form["MaxLeaveDebitInService"];
                   
                    //L.LvHead_Id = int.Parse(lvhead);
                    L.AboveServiceMaxYears = Convert.ToBoolean(AboveServiceMaxYears);
                    L.Accumalation = Convert.ToBoolean(Accumalation);
                    L.ExcludeLeaves = Convert.ToBoolean(ExcludeLeaves);
                    L.IsCreditDatePolicy = Convert.ToBoolean(IsCreditDatePolicy);
                    L.FixedCreditDays = Convert.ToBoolean(FixedCreditDays);
                    L.LVConvert = Convert.ToBoolean(LVConvert);
                    L.LVConvertBal = Convert.ToBoolean(LVConvertBal);
                    L.OccCarryForward = Convert.ToBoolean(OccCarryForward);
                    L.OccInServAppl = Convert.ToBoolean(OccInServAppl);
                    L.ServiceLink = Convert.ToBoolean(ServiceLink);
                    L.AccumulationWithCredit = Convert.ToBoolean(AccumulationWithCredit);
                    L.ProdataFlag = Convert.ToBoolean(ProdataFlag);
                    
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (CreditDatelist != null && CreditDatelist != "")
                    {
                        var id = Convert.ToInt32(CreditDatelist);
                        var value = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "447").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(CreditDatelist)).FirstOrDefault();  //db.LookupValue.Where(e => e.Id == id).SingleOrDefault();
                        L.CreditDate = value;
                    }
                    if (CreditDatelist != null && CreditDatelist != "")
                    {
                        L.CreditDate_Id = Convert.ToInt32(CreditDatelist);
                    }
                    else
                    {
                        L.CreditDate_Id = null;
                    }
                    if (lvhead != null && lvhead != "")
                    {
                        L.LvHead_Id = Convert.ToInt32(lvhead);
                    }
                    else
                    {
                        L.LvHead_Id = null;
                    }
                    if (ConvertLeaveHeadlist != null && ConvertLeaveHeadlist != "")
                    {
                        L.ConvertLeaveHead_Id = Convert.ToInt32(ConvertLeaveHeadlist);
                    }
                    else
                    {
                        L.ConvertLeaveHead_Id = null;
                    }
                    if (ConvertLeaveHeadBallist != null && ConvertLeaveHeadBallist != "")
                    {
                        L.ConvertLeaveHeadBal_Id = Convert.ToInt32(ConvertLeaveHeadBallist);
                    }
                    else
                    {
                        L.ConvertLeaveHeadBal_Id = null;
                    }


                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    LvCreditPolicy blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.LvCreditPolicy.Where(e => e.Id == data).Include(e => e.ConvertLeaveHead)
                                                                .Include(e => e.ConvertLeaveHeadBal)
                                                                .Include(e => e.ExcludeLeaveHeads)
                                                                .Include(e => e.CreditDate)
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

                                    if (CreditDatelist != null && CreditDatelist != "")
                                    {
                                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "447").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(CreditDatelist)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(CreditDatelist));
                                        L.CreditDate = val;

                                        var type = db.LvCreditPolicy.Include(e => e.CreditDate).Where(e => e.Id == data).SingleOrDefault();
                                        LvCreditPolicy typedetails = null;
                                        if (type.CreditDate != null)
                                        {
                                            typedetails = db.LvCreditPolicy.Where(x => x.CreditDate.Id == type.CreditDate.Id && x.Id == data).FirstOrDefault();
                                        }
                                        else
                                        {
                                            typedetails = db.LvCreditPolicy.Where(x => x.Id == data).FirstOrDefault();
                                        }

                                        typedetails.CreditDate = L.CreditDate;
                                        db.LvCreditPolicy.Attach(typedetails);
                                        db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = typedetails.RowVersion;
                                        db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;

                                    }
                                    else
                                    {
                                        var CreditdateypeDetails = db.LvCreditPolicy.Include(e => e.CreditDate).Where(x => x.Id == data).FirstOrDefault();
                                        CreditdateypeDetails.CreditDate = null;
                                        db.LvCreditPolicy.Attach(CreditdateypeDetails);
                                        db.Entry(CreditdateypeDetails).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = CreditdateypeDetails.RowVersion;
                                        db.Entry(CreditdateypeDetails).State = System.Data.Entity.EntityState.Detached;
                                    }

                                    if (ConvertLeaveHeadlist != null && ConvertLeaveHeadlist != "")
                                    {
                                        var type = db.LvCreditPolicy.Include(e => e.ConvertLeaveHead).Where(e => e.Id == data).SingleOrDefault();
                                        LvCreditPolicy typedetails = null;
                                        if (type.CreditDate != null)
                                        {
                                            typedetails = db.LvCreditPolicy.Where(x => x.ConvertLeaveHead.Id == type.ConvertLeaveHead.Id && x.Id == data).FirstOrDefault();
                                        }
                                        else
                                        {
                                            typedetails = db.LvCreditPolicy.Where(x => x.Id == data).FirstOrDefault();
                                        }

                                        typedetails.ConvertLeaveHead = L.ConvertLeaveHead;
                                        db.LvCreditPolicy.Attach(typedetails);
                                        db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = typedetails.RowVersion;
                                        db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;

                                    }
                                    else
                                    {
                                        var CreditdateypeDetails = db.LvCreditPolicy.Include(e => e.ConvertLeaveHead).Where(x => x.Id == data).FirstOrDefault();
                                        CreditdateypeDetails.ConvertLeaveHead = null;
                                        db.LvCreditPolicy.Attach(CreditdateypeDetails);
                                        db.Entry(CreditdateypeDetails).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = CreditdateypeDetails.RowVersion;
                                        db.Entry(CreditdateypeDetails).State = System.Data.Entity.EntityState.Detached;
                                    }

                                    if (ConvertLeaveHeadBallist != null && ConvertLeaveHeadBallist != "")
                                    {
                                         
                                        var type = db.LvCreditPolicy.Include(e => e.ConvertLeaveHeadBal).Where(e => e.Id == data).SingleOrDefault();
                                        LvCreditPolicy typedetails = null;
                                        if (type.CreditDate != null)
                                        {
                                            typedetails = db.LvCreditPolicy.Where(x => x.ConvertLeaveHeadBal.Id == type.ConvertLeaveHeadBal.Id && x.Id == data).FirstOrDefault();
                                        }
                                        else
                                        {
                                            typedetails = db.LvCreditPolicy.Where(x => x.Id == data).FirstOrDefault();
                                        }

                                        typedetails.ConvertLeaveHeadBal = L.ConvertLeaveHeadBal;
                                        db.LvCreditPolicy.Attach(typedetails);
                                        db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = typedetails.RowVersion;
                                        db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;

                                    }
                                    else
                                    {
                                        var CreditdateypeDetails = db.LvCreditPolicy.Include(e => e.CreditDate).Where(x => x.Id == data).FirstOrDefault();
                                        CreditdateypeDetails.ConvertLeaveHeadBal = null;
                                        db.LvCreditPolicy.Attach(CreditdateypeDetails);
                                        db.Entry(CreditdateypeDetails).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = CreditdateypeDetails.RowVersion;
                                        db.Entry(CreditdateypeDetails).State = System.Data.Entity.EntityState.Detached;
                                    }

                                    List<LvHead> LvHeadObjELH = new List<LvHead>();
                                    LvCreditPolicy lvheaddetails = null;
                                    lvheaddetails = db.LvCreditPolicy.Include(e => e.ExcludeLeaveHeads).Where(e => e.Id == data).SingleOrDefault();
                                    if (ExcludeLeaveHeadslist != null && ExcludeLeaveHeadslist != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(ExcludeLeaveHeadslist);
                                        foreach (var ca in ids)
                                        {
                                            var ExcludeLeaveHeadslistvalue = db.LvHead.Find(ca);
                                            LvHeadObjELH.Add(ExcludeLeaveHeadslistvalue);
                                            lvheaddetails.ExcludeLeaveHeads = LvHeadObjELH;
                                        }
                                    }
                                    else
                                    {
                                        lvheaddetails.ExcludeLeaveHeads = null;
                                    }
                                    

                                    var CurCorp = db.LvCreditPolicy.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        LvCreditPolicy LvCreditPolicy = new LvCreditPolicy()
                                        {
                                            LvHead_Id=L.LvHead_Id,
                                            AboveServiceMaxYears = L.AboveServiceMaxYears,
                                            AboveServiceSteps = L.AboveServiceSteps,
                                            Accumalation = L.Accumalation,
                                            AccumalationLimit = L.AccumalationLimit,
                                            AccumulationWithCredit = L.AccumulationWithCredit,
                                            ConvertedDays = L.ConvertedDays,
                                            ConvertLeaveHead = L.ConvertLeaveHead,
                                            ConvertLeaveHeadBal = L.ConvertLeaveHeadBal,
                                            CreditDate = L.CreditDate,
                                            CreditDate_Id = L.CreditDate_Id,
                                            CreditDays = L.CreditDays,
                                            ExcludeLeaveHeads = L.ExcludeLeaveHeads,
                                            ExcludeLeaves = L.ExcludeLeaves,
                                            FixedCreditDays = L.FixedCreditDays,
                                            IsCreditDatePolicy = L.IsCreditDatePolicy,
                                            LVConvert = L.LVConvert,
                                            LVConvertBal = L.LVConvertBal,
                                            LvConvertLimit = L.LvConvertLimit,
                                            LvConvertLimitBal = L.LvConvertLimitBal,
                                            MaxLeaveDebitInService = L.MaxLeaveDebitInService,
                                            ServicemaxYearsLimit = L.ServicemaxYearsLimit,
                                            OccCarryForward = L.OccCarryForward,
                                            OccInServAppl = L.OccInServAppl,
                                            OccInServ = L.OccInServ,
                                            PolicyName = L.PolicyName,
                                            ProCreditFrequency = L.ProCreditFrequency,
                                            ProdataFlag = L.ProdataFlag,
                                            ServiceLink = L.ServiceLink,
                                            ServiceYearsLimit = L.ServiceYearsLimit,
                                            WorkingDays = L.WorkingDays,
                                            Id = data,
                                            DBTrack = L.DBTrack,
                                            ConvertLeaveHead_Id = L.ConvertLeaveHead_Id,
                                            ConvertLeaveHeadBal_Id = L.ConvertLeaveHeadBal_Id
                                        };
                                        db.LvCreditPolicy.Attach(LvCreditPolicy);
                                        db.Entry(LvCreditPolicy).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(LvCreditPolicy).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }
                                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                        DT_LvCreditPolicy DT_Corp = (DT_LvCreditPolicy)obj;
                                        DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    //return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            LvCreditPolicy blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            LvCreditPolicy Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.LvCreditPolicy.Where(e => e.Id == data).SingleOrDefault();
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

                            LvCreditPolicy corp = new LvCreditPolicy()
                            {
                                AboveServiceMaxYears = L.AboveServiceMaxYears,
                                AboveServiceSteps = L.AboveServiceSteps,
                                Accumalation = L.Accumalation,
                                AccumalationLimit = L.AccumalationLimit,
                                AccumulationWithCredit = L.AccumulationWithCredit,
                                ConvertedDays = L.ConvertedDays,
                                ConvertLeaveHead = L.ConvertLeaveHead,
                                ConvertLeaveHeadBal = L.ConvertLeaveHeadBal,
                                CreditDate = L.CreditDate,
                                CreditDays = L.CreditDays,
                                ExcludeLeaveHeads = L.ExcludeLeaveHeads,
                                ExcludeLeaves = L.ExcludeLeaves,
                                FixedCreditDays = L.FixedCreditDays,
                                IsCreditDatePolicy = L.IsCreditDatePolicy,
                                LVConvert = L.LVConvert,
                                LVConvertBal = L.LVConvertBal,
                                LvConvertLimit = L.LvConvertLimit,
                                LvConvertLimitBal = L.LvConvertLimitBal,
                                MaxLeaveDebitInService = L.MaxLeaveDebitInService,
                                ServicemaxYearsLimit = L.ServicemaxYearsLimit,
                                OccCarryForward = L.OccCarryForward,
                                OccInServAppl = L.OccInServAppl,
                                OccInServ = L.OccInServ,
                                PolicyName = L.PolicyName,
                                ProCreditFrequency = L.ProCreditFrequency,
                                ProdataFlag = L.ProdataFlag,
                                ServiceLink = L.ServiceLink,
                                ServiceYearsLimit = L.ServiceYearsLimit,
                                WorkingDays = L.WorkingDays,
                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvEncashReq", L.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.LvCreditPolicy.Where(e => e.Id == data).Include(e => e.ConvertLeaveHead)
                                    .Include(e => e.ConvertLeaveHeadBal).Include(e => e.ExcludeLeaveHeads).Include(e => e.CreditDate).SingleOrDefault();
                                DT_LvCreditPolicy DT_Corp = (DT_LvCreditPolicy)obj;
                                DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;

                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = L.DBTrack;
                            db.LvCreditPolicy.Attach(blog);
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

        //public int EditS(string CreditDatelist, string ExcludeLeaveHeadslist, string ConvertLeaveHeadBallist, string ConvertLeaveHeadlist, int data, LvCreditPolicy c, DBTrack dbT)
        //{
        //    if (CreditDatelist != null)
        //    {
        //        if (CreditDatelist != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(CreditDatelist));
        //            c.CreditDate = val;

        //            var type = db.LvEncashReq.Include(e => e.WFStatus).Where(e => e.Id == data).SingleOrDefault();
        //            IList<LvCreditPolicy> typedetails = null;
        //            if (type.WFStatus != null)
        //            {
        //                typedetails = db.LvCreditPolicy.Where(x => x.CreditDate.Id == type.WFStatus.Id && x.Id == data).ToList();
        //            }
        //            else
        //            {
        //                typedetails = db.LvCreditPolicy.Where(x => x.Id == data).ToList();
        //            }
        //            db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //            foreach (var s in typedetails)
        //            {
        //                s.CreditDate = c.CreditDate;
        //                db.LvCreditPolicy.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //        else
        //        {
        //            var WFTypeDetails = db.LvCreditPolicy.Include(e => e.CreditDate).Where(x => x.Id == data).ToList();
        //            foreach (var s in WFTypeDetails)
        //            {
        //                s.CreditDate = null;
        //                db.LvCreditPolicy.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var CreditdateypeDetails = db.LvCreditPolicy.Include(e => e.CreditDate).Where(x => x.Id == data).ToList();
        //        foreach (var s in CreditdateypeDetails)
        //        {
        //            s.CreditDate = null;
        //            db.LvCreditPolicy.Attach(s);
        //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //            await db.SaveChangesAsync();
        //            db.SaveChanges();
        //            TempData["RowVersion"] = s.RowVersion;
        //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //        }
        //    }

        //    if (LvNewReqlist != null)
        //    {
        //        if (LvNewReqlist != "")
        //        {
        //            var val = db.LvNewReq.Find(int.Parse(LvNewReqlist));
        //            c.LvNewReq = val;

        //            var add = db.LvEncashReq.Include(e => e.LvNewReq).Where(e => e.Id == data).SingleOrDefault();
        //            IList<LvEncashReq> LvNewReqdetails = null;
        //            if (add.LvNewReq != null)
        //            {
        //                LvNewReqdetails = db.LvEncashReq.Where(x => x.LvNewReq.Id == add.LvNewReq.Id && x.Id == data).ToList();
        //            }
        //            else
        //            {
        //                LvNewReqdetails = db.LvEncashReq.Where(x => x.Id == data).ToList();
        //            }
        //            if (LvNewReqdetails != null)
        //            {
        //                foreach (var s in LvNewReqdetails)
        //                {
        //                    s.LvNewReq = c.LvNewReq;
        //                    db.LvEncashReq.Attach(s);
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                    await db.SaveChangesAsync(false);
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = s.RowVersion;
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var LvNewReqdetails = db.LvEncashReq.Include(e => e.LvNewReq).Where(x => x.Id == data).ToList();
        //        foreach (var s in LvNewReqdetails)
        //        {
        //            s.LvNewReq = null;
        //            db.LvEncashReq.Attach(s);
        //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //            await db.SaveChangesAsync();
        //            db.SaveChanges();
        //            TempData["RowVersion"] = s.RowVersion;
        //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //        }
        //    }

        //    if (LvWFDetailslist != null)
        //    {
        //        if (LvWFDetailslist != "")
        //        {

        //            List<int> IDs = LvWFDetailslist.Split(',').Select(e => int.Parse(e)).ToList();
        //            foreach (var k in IDs)
        //            {
        //                var value = db.LvWFDetails.Find(k);
        //                c.LvWFDetails = new List<LvWFDetails>();
        //                c.LvWFDetails.Add(value);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var LvWdetails = db.LvEncashReq.Include(e => e.LvWFDetails).Where(x => x.Id == data).ToList();
        //        foreach (var s in LvWdetails)
        //        {
        //            s.LvWFDetails = null;
        //            db.LvEncashReq.Attach(s);
        //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //            await db.SaveChangesAsync();
        //            db.SaveChanges();
        //            TempData["RowVersion"] = s.RowVersion;
        //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //        }
        //    }

        //    var CurCorp = db.LvCreditPolicy.Find(data);
        //    TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //    {

        //        c.DBTrack = dbT;
        //        LvEncashReq lvencashreq = new LvEncashReq()
        //        {
        //            EncashDays = c.EncashDays,
        //            FromPeriod = c.FromPeriod,
        //            ToPeriod = c.ToPeriod,
        //            Narration = c.Narration,
        //            Id = data,
        //            DBTrack = c.DBTrack
        //        };
        //        db.LvEncashReq.Attach(lvencashreq);
        //        db.Entry(lvencashreq).State = System.Data.Entity.EntityState.Modified;
        //        db.Entry(lvencashreq).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //        // DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //        return 1;
        //    }
        //    return 0;
        //}
        #endregion

        #region P2BDETAILS

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
                IEnumerable<LvCreditPolicy> lencash = null;
                if (gp.IsAutho == true)
                {
                    lencash = db.LvCreditPolicy.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    lencash = db.LvCreditPolicy.AsNoTracking().ToList();
                }

                IEnumerable<LvCreditPolicy> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = lencash;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.PolicyName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.ProCreditFrequency.ToString().Contains(gp.searchString))
                            || (e.MaxLeaveDebitInService.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.PolicyName, a.ProCreditFrequency, a.MaxLeaveDebitInService, a.Id }).ToList();

                        //  jsonData = IE.Select(a => new { , a.PerquisiteName.LookupVal, a.Id }).ToList();

                        //  jsonData = IE.Select(a => new { .ProCreditFrequency, a.MaxLeaveDebitInService }).Where((e => (e.Id.ToString() == gp.searchString) || (e.PolicyName.ToLower() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.PolicyName), Convert.ToString(a.ProCreditFrequency), Convert.ToString(a.MaxLeaveDebitInService), a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = lencash;
                    Func<LvCreditPolicy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "PolicyName" ? c.PolicyName.ToString() :
                             gp.sidx == "ProCreditFrequency" ? c.PolicyName.ToString() :
                              gp.sidx == "MaxLeaveDebitInService" ? c.PolicyName.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.PolicyName), Convert.ToString(a.ProCreditFrequency), Convert.ToString(a.MaxLeaveDebitInService), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.PolicyName), Convert.ToString(a.ProCreditFrequency), Convert.ToString(a.MaxLeaveDebitInService), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.PolicyName), Convert.ToString(a.ProCreditFrequency), Convert.ToString(a.MaxLeaveDebitInService), a.Id }).ToList();
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
        #endregion

        #region DELETE


        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    LvCreditPolicy OBJLvCreditPolicy = db.LvCreditPolicy.Include(e => e.ConvertLeaveHead)
                                                       .Include(e => e.ConvertLeaveHeadBal)
                                                       .Include(e => e.ExcludeLeaveHeads)
                                                       .Include(e => e.CreditDate)
                                                       .Include(e => e.LvHead)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    var a = db.LvNewReq.Include(e => e.LeaveCalendar).Include(e => e.LeaveHead)
                        .Where(e => e.LeaveHead.Id == OBJLvCreditPolicy.LvHead.Id).ToList();
                    if (a.Count > 0)
                    {
                        Msg.Add(" Reference for this record exists.Cannot remove it..  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    //         db.LvNewReq.Include(e=>e.LeaveCalendar).Include(e=>e.LeaveHead).Where()
                    if (OBJLvCreditPolicy.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = OBJLvCreditPolicy.DBTrack.CreatedBy != null ? OBJLvCreditPolicy.DBTrack.CreatedBy : null,
                                CreatedOn = OBJLvCreditPolicy.DBTrack.CreatedOn != null ? OBJLvCreditPolicy.DBTrack.CreatedOn : null,
                                IsModified = OBJLvCreditPolicy.DBTrack.IsModified == true ? true : false
                            };
                            OBJLvCreditPolicy.DBTrack = dbT;
                            db.Entry(OBJLvCreditPolicy).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, OBJLvCreditPolicy.DBTrack);
                            DT_LvCreditPolicy DT_Corp = (DT_LvCreditPolicy)rtn_Obj;
                            DT_Corp.CreditDate_Id = OBJLvCreditPolicy.CreditDate == null ? 0 : OBJLvCreditPolicy.CreditDate.Id;
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
                        var ConvertLeaveHead = OBJLvCreditPolicy.ConvertLeaveHead;
                        var ConvertLeaveHeadBal = OBJLvCreditPolicy.ConvertLeaveHeadBal;
                        var ExcludeLeaveHeads = OBJLvCreditPolicy.ExcludeLeaveHeads;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                           
                            if (ExcludeLeaveHeads != null)
                            {
                                var LvCreditPolicy = new HashSet<int>(OBJLvCreditPolicy.ExcludeLeaveHeads.Select(e => e.Id));
                                if (LvCreditPolicy.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = OBJLvCreditPolicy.DBTrack.CreatedBy != null ? OBJLvCreditPolicy.DBTrack.CreatedBy : null,
                                    CreatedOn = OBJLvCreditPolicy.DBTrack.CreatedOn != null ? OBJLvCreditPolicy.DBTrack.CreatedOn : null,
                                    IsModified = OBJLvCreditPolicy.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.EmpId,
                                    //AuthorizedOn = DateTime.Now
                                };
                                db.Entry(OBJLvCreditPolicy).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, OBJLvCreditPolicy.DBTrack);
                                DT_LvCreditPolicy DT_Corp = (DT_LvCreditPolicy)rtn_Obj;
                                DT_Corp.CreditDate_Id = OBJLvCreditPolicy.CreditDate == null ? 0 : OBJLvCreditPolicy.CreditDate.Id;
                                db.Create(DT_Corp);
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
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

        #endregion

            #region AUTHORIZATION
            #endregion

        }
    }
}