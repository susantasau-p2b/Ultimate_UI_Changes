using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using P2BUltimate.Security;
using Recruitement;

namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class CanOffController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /EmpOff/
        public ActionResult Index()
        {
            return View("~/Views/Recruitement/MainView/CanOff/Index.cshtml");
        }

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Bank.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PopulateDropDownBranchList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Branch.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public ActionResult GetPayScaleLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.PayScale.Include(e => e.PayScaleType).Include(e => e.PayScaleArea.Select(r => r.LocationObj)).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.PayScale.Include(e => e.PayScaleArea).Include(e => e.PayScaleArea.Select(r => r.LocationObj)).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var P = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(P, JsonRequestBehavior.AllowGet);
            }
        }
        public class EmpoffDetails
        {
            public int Id { get; set; }
            public string NationalityID { get; set; }
            public string Employee { get; set; }
            public string DBTrack { get; set; }
            public string AccountNo { get; set; }

        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(EmpOff EOff, FormCollection form, string data) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string PayMode = form["PayMode_drop"] == "0" ? "" : form["PayMode_drop"];
                    string Bank = form["Bank_drop"] == "0" ? "" : form["Bank_drop"];
                    string Branch = form["Branch_drop"] == "0" ? "" : form["Branch_drop"];
                    string AccountType = form["AccountType_drop"] == "0" ? "" : form["AccountType_drop"];
                    // string PayScale = form["PayScalelist"] == "0" ? "" : form["PayScalelist"];
                    string PayProcessGrp = form["Addresslist"] == "0" ? "" : form["PayProcessGrouplist"];
                    string PayScale_ddl = form["PayScalelist"] == "0" ? "" : form["PayScalelist"];
                    if (PayScale_ddl != null || PayScale_ddl != "" || PayScale_ddl != "0")
                    {
                        var value = db.PayScale.Find(int.Parse(PayScale_ddl));
                        EOff.PayScale = value;

                    }
                    int emp_Id = form["emp_Id"] == "0" ? 0 : Convert.ToInt32(form["emp_Id"]);


                    if (PayMode != null && PayMode != "" && PayMode != "-Select-")
                    {
                        var val = db.LookupValue.Find(int.Parse(PayMode));
                        EOff.PayMode = val;
                    }
                    if (Bank != null && Bank != "" && Bank != "-Select-")
                    {
                        var val = db.Bank.Find(int.Parse(Bank));
                        EOff.Bank = val;
                    }

                    if (Branch != null && Branch != "" && Branch != "-Select-")
                    {
                        var val = db.Branch.Find(int.Parse(Branch));
                        EOff.Branch = val;
                    }

                    if (AccountType != null && AccountType != "" && AccountType != "-Select-")
                    {
                        var val = db.LookupValue.Find(int.Parse(AccountType));
                        EOff.AccountType = val;
                    }

                    //if (PayScale != null && PayScale != null)
                    //{
                    //    var val = db.PayScale.Find(int.Parse(PayScale));
                    //    EOff.PayScale = val;
                    //}

                    if (PayProcessGrp != null && PayProcessGrp != "" && PayProcessGrp != "-Select-")
                    {
                        var val = db.PayProcessGroup.Find(int.Parse(PayProcessGrp));
                        EOff.PayProcessGroup = val;
                    }

                    if (ModelState.IsValid)
                    {
                        Candidate OCandidate = db.Candidate.Find(emp_Id);

                        using (TransactionScope ts = new TransactionScope())
                        {
                            EOff.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            EmpOff EmpOfficial = new EmpOff()
                            {
                                AccountNo = EOff.AccountNo == null ? "" : EOff.AccountNo.Trim(),
                                AccountType = EOff.AccountType,
                                Bank = EOff.Bank,
                                Branch = EOff.Branch,
                                ESICAppl = EOff.ESICAppl,
                                FamilyHandicap = EOff.FamilyHandicap,
                                HandicapRemark = EOff.HandicapRemark,
                                LWFAppl = EOff.LWFAppl,
                                NationalityID = EOff.NationalityID,
                                PayMode = EOff.PayMode,
                                PayProcessGroup = EOff.PayProcessGroup,
                                //PayScale = EOff.PayScale,
                                PFAppl = EOff.PFAppl,
                                PTAppl = EOff.PTAppl,
                                SelfHandicap = EOff.SelfHandicap,
                                VPFAmt = EOff.VPFAmt,
                                VPFAppl = EOff.VPFAppl,
                                VPFPerc = EOff.VPFPerc,
                                PayScale = EOff.PayScale,
                                DBTrack = EOff.DBTrack
                            };
                            try
                            {
                                db.EmpOff.Add(EmpOfficial);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, EOff.DBTrack);
                                DT_EmpOff DT_EOff = (DT_EmpOff)rtn_Obj;
                                DT_EOff.AccountType_Id = EOff.AccountType == null ? 0 : EOff.AccountType.Id;
                                DT_EOff.Bank_Id = EOff.Bank == null ? 0 : EOff.Bank.Id;
                                DT_EOff.Branch_Id = EOff.Branch == null ? 0 : EOff.Branch.Id;
                                DT_EOff.NationalityID = EOff.NationalityID == null ? 0 : EOff.NationalityID.Id;
                                DT_EOff.PayMode_Id = EOff.PayMode == null ? 0 : EOff.PayMode.Id;
                                DT_EOff.PayProcessGroup_Id = EOff.PayProcessGroup == null ? 0 : EOff.PayProcessGroup.Id;
                                //DT_EOff.PayScale_Id = EOff.PayScale == null ? 0 : EOff.PayScale.Id; 
                                db.Create(DT_EOff);
                                db.SaveChanges();


                                if (OCandidate != null)
                                {
                                    OCandidate.CanOffInfo = EmpOfficial;
                                    db.Candidate.Attach(OCandidate);
                                    db.Entry(OCandidate).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(OCandidate).State = System.Data.Entity.EntityState.Detached;
                                }
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = EOff.Id });
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
                        //var errorMsg = sb.ToString();
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
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
                //return new EmptyResult();
            }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.EmpOff.Include(e => e.AccountType).Include(e => e.PayMode).Include(e => e.PayScale)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        PayMode_Id = e.PayMode.Id == null ? 0 : e.PayMode.Id,
                        Paymode_details = e.PayMode != null ? e.PayMode.LookupVal : null,
                        PayScale_Id = e.PayScale.Id == null ? 0 : e.PayScale.Id,
                        Payscale_FullDetails = e.PayScale.FullDetails,
                        Bank_Id = e.Bank.Id == null ? 0 : e.Bank.Id,
                        Branch_Id = e.Branch.Id == null ? 0 : e.Branch.Id,
                        AccountType_Id = e.AccountType.Id == null ? 0 : e.AccountType.Id,
                        AccountNo = e.AccountNo == null ? "" : e.AccountNo,
                        PFAppl = e.PFAppl,
                        SelfHandicap = e.SelfHandicap,
                        FamilyHandicap = e.FamilyHandicap,
                        HandicapRemark = e.HandicapRemark == null ? "" : e.HandicapRemark,
                        PTAppl = e.PTAppl,
                        ESICAppl = e.ESICAppl,
                        LWFAppl = e.LWFAppl,
                        VPFAppl = e.VPFAppl,
                        VPFAmt = e.VPFAmt,
                        VPFPerc = e.VPFPerc,
                        Action = e.DBTrack.Action,
                        NationalityID = e.NationalityID == null ? null : e.NationalityID,

                    }).ToList();

                var add_data = db.EmpOff.Include(e => e.PayProcessGroup)
                                .Include(e => e.NationalityID).Where(e => e.Id == data)
                    .Select(e => new
                    {
                        PayProcessGroup_Id = e.PayProcessGroup.Id == null ? "" : e.PayProcessGroup.Id.ToString(),
                        PayProcessGroup_FullDetails = e.PayProcessGroup.FullDetails == null ? "" : e.PayProcessGroup.FullDetails
                    }).ToList();


                var W = db.DT_EmpOff
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         AccountNo = e.AccountNo == null ? "" : e.AccountNo,
                         PFAppl = e.PFAppl,
                         SelfHandicap = e.SelfHandicap,
                         FamilyHandicap = e.FamilyHandicap,
                         HandicapRemark = e.HandicapRemark == null ? "" : e.HandicapRemark,
                         PTAppl = e.PTAppl,
                         ESICAppl = e.ESICAppl,
                         LWFAppl = e.LWFAppl,
                         VPFAppl = e.VPFAppl,
                         VPFAmt = e.VPFAmt,
                         VPFPerc = e.VPFPerc,
                         PayMode_Val = e.PayMode_Id == 0 ? "" : db.LookupValue
                                 .Where(x => x.Id == e.PayMode_Id)
                                 .Select(x => x.LookupVal).FirstOrDefault(),
                         AccountType_Val = e.AccountType_Id == 0 ? "" : db.LookupValue
                                 .Where(x => x.Id == e.AccountType_Id)
                                 .Select(x => x.LookupVal).FirstOrDefault(),
                         Bank_Val = e.Bank_Id == 0 ? "" : db.Bank
                              .Where(x => x.Id == e.Bank_Id)
                              .Select(x => x.FullDetails).FirstOrDefault(),
                         Branch_Val = e.Branch_Id == 0 ? "" : db.Branch
                              .Where(x => x.Id == e.Branch_Id)
                              .Select(x => x.FullDetails).FirstOrDefault(),
                         PayProcessGroup_Val = e.PayProcessGroup_Id == 0 ? "" : db.PayProcessGroup
                              .Where(x => x.Id == e.PayProcessGroup_Id)
                              .Select(x => x.FullDetails).FirstOrDefault()


                         //Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                         //Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var EmpOff = db.EmpOff.Find(data);
                TempData["RowVersion"] = EmpOff.RowVersion;
                var Auth = EmpOff.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(EmpOff EOff, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string PayMode = form["PayMode_drop"] == "0" ? "" : form["PayMode_drop"];
                    string Bank = form["Bank_drop"] == "0" ? "" : form["Bank_drop"];
                    string Branch = form["Branch_drop"] == "0" ? "" : form["Branch_drop"];
                    string AccountType = form["AccountType_drop"] == "0" ? "" : form["AccountType_drop"];
                    string PayScale = form["PayScalelist"] == "0" ? "" : form["PayScalelist"];
                    string PayProcessGrp = form["Addresslist"] == "0" ? "" : form["PayProcessGrouplist"];
                    bool Auth = form["autho_allow"] == "true" ? true : false;


                    if (PayMode != null && PayMode != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(PayMode));
                        EOff.PayMode = val;
                    }

                    if (Bank != null && Bank != "" && Bank != "-Select-")
                    {
                        var val = db.Bank.Find(int.Parse(Bank));
                        EOff.Bank = val;
                    }

                    if (Branch != null && Branch != "" && Branch != "-Select-")
                    {
                        var val = db.Branch.Find(int.Parse(Branch));
                        EOff.Branch = val;
                    }

                    if (AccountType != null && AccountType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(AccountType));
                        EOff.AccountType = val;
                    }

                    //if (PayScale != null && PayScale != null)
                    //{
                    //    var val = db.PayScale.Find(int.Parse(PayScale));
                    //    EOff.PayScale = val;
                    //}

                    if (PayProcessGrp != null && PayProcessGrp != "")
                    {
                        var val = db.PayProcessGroup.Find(int.Parse(PayProcessGrp));
                        EOff.PayProcessGroup = val;
                    }


                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    EmpOff blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.EmpOff.Include(e => e.AccountType)
                                                       .Include(e => e.Bank).Include(e => e.Branch).Include(e => e.NationalityID)
                                                       .Include(e => e.PayMode).Include(e => e.PayProcessGroup)
                                                       .Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    EOff.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    if (PayScale != null)
                                    {
                                        if (PayScale != "")
                                        {
                                            var val = db.PayScale.Find(int.Parse(PayScale));
                                            EOff.PayScale = val;

                                            var type = db.EmpOff.Include(e => e.PayScale).Where(e => e.Id == data).SingleOrDefault();
                                            IList<EmpOff> typedetails = null;
                                            if (type.PayScale != null)
                                            {
                                                typedetails = db.EmpOff.Where(x => x.PayScale.Id == type.PayScale.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.EmpOff.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.PayScale = EOff.PayScale;
                                                db.EmpOff.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.EmpOff.Include(e => e.PayScale).Where(x => x.Id == data).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.PayScale = null;
                                                db.EmpOff.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var CreditdateypeDetails = db.EmpOff.Include(e => e.PayScale).Where(x => x.Id == data).ToList();
                                        foreach (var s in CreditdateypeDetails)
                                        {
                                            s.PayScale = null;
                                            db.EmpOff.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    //public int EditS(string AccType, string PayMode, string bank,string branch, string national, string PAyProcGrp,int data, EmpOff EOff, DBTrack dbT)
                                    int a = EditS(AccountType, PayMode, Bank, Branch, "", PayProcessGrp, data, EOff, EOff.DBTrack);



                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, EOff.DBTrack);
                                        DT_EmpOff DT_EmpOff = (DT_EmpOff)obj;
                                        DT_EmpOff.AccountType_Id = blog.AccountType == null ? 0 : blog.AccountType.Id;
                                        DT_EmpOff.Bank_Id = blog.AccountType == null ? 0 : blog.AccountType.Id;
                                        DT_EmpOff.Branch_Id = blog.AccountType == null ? 0 : blog.AccountType.Id;
                                        DT_EmpOff.NationalityID = blog.AccountType == null ? 0 : blog.AccountType.Id;
                                        DT_EmpOff.PayMode_Id = blog.AccountType == null ? 0 : blog.AccountType.Id;
                                        DT_EmpOff.PayProcessGroup_Id = blog.AccountType == null ? 0 : blog.AccountType.Id;

                                        db.Create(DT_EmpOff);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = EOff.Id, Val = EOff.AccountNo, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { EOff.Id, EOff.AccountNo, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (EmpOff)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (EmpOff)databaseEntry.ToObject();
                                    EOff.RowVersion = databaseValues.RowVersion;

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

                            EmpOff blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            EmpOff Old_EmpOff = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.EmpOff.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            EOff.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            EmpOff EmpOfficial = new EmpOff()
                            {
                                AccountNo = EOff.AccountNo == null ? "" : EOff.AccountNo.Trim(),
                                ESICAppl = EOff.ESICAppl,
                                FamilyHandicap = EOff.FamilyHandicap,
                                HandicapRemark = EOff.HandicapRemark,
                                LWFAppl = EOff.LWFAppl,
                                PFAppl = EOff.PFAppl,
                                PTAppl = EOff.PTAppl,
                                SelfHandicap = EOff.SelfHandicap,
                                VPFAmt = EOff.VPFAmt,
                                VPFAppl = EOff.VPFAppl,
                                VPFPerc = EOff.VPFPerc,
                                DBTrack = EOff.DBTrack,
                                Id = data
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, EmpOfficial, "EmpOff", EOff.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_EmpOff = context.EmpOff.Where(e => e.Id == data).Include(e => e.AccountType)
                                                       .Include(e => e.Bank).Include(e => e.Branch).Include(e => e.NationalityID)
                                                       .Include(e => e.PayMode).Include(e => e.PayProcessGroup).SingleOrDefault();
                                DT_EmpOff DT_EmpOff = (DT_EmpOff)obj;
                                DT_EmpOff.AccountType_Id = DBTrackFile.ValCompare(Old_EmpOff.AccountType, EOff.AccountType);
                                DT_EmpOff.Bank_Id = DBTrackFile.ValCompare(Old_EmpOff.Bank, EOff.Bank);
                                DT_EmpOff.Branch_Id = DBTrackFile.ValCompare(Old_EmpOff.Branch, EOff.Branch);
                                DT_EmpOff.NationalityID = DBTrackFile.ValCompare(Old_EmpOff.NationalityID, EOff.NationalityID);
                                DT_EmpOff.PayMode_Id = DBTrackFile.ValCompare(Old_EmpOff.PayMode, EOff.PayMode);
                                DT_EmpOff.PayProcessGroup_Id = DBTrackFile.ValCompare(Old_EmpOff.PayProcessGroup, EOff.PayProcessGroup);
                                db.Create(DT_EmpOff);
                            }
                            blog.DBTrack = EOff.DBTrack;
                            db.EmpOff.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = EOff.AccountNo, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, EOff.AccountNo, "Record Updated", JsonRequestBehavior.AllowGet });
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


        public int EditS(string AccType, string PayMode, string bank, string branch, string national, string PayProcGrp, int data, EmpOff EOff, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                EmpOff typedetails = null;

                //AccountType
                if (AccType != null & AccType != "")
                {
                    var val = db.LookupValue.Find(int.Parse(AccType));
                    EOff.AccountType = val;

                    var type = db.EmpOff.Include(e => e.AccountType).Where(e => e.Id == data).SingleOrDefault();

                    if (type.AccountType != null)
                    {
                        typedetails = db.EmpOff.Where(x => x.AccountType.Id == type.AccountType.Id && x.Id == data).SingleOrDefault();
                    }
                    else
                    {
                        typedetails = db.EmpOff.Where(x => x.Id == data).SingleOrDefault();
                    }
                    typedetails.AccountType = EOff.AccountType;
                }
                else
                {
                    typedetails = db.EmpOff.Include(e => e.AccountType).Where(x => x.Id == data).SingleOrDefault();
                    typedetails.AccountType = null;
                }
                /* end */

                //Paymode
                if (PayMode != null & PayMode != "")
                {
                    var val = db.LookupValue.Find(int.Parse(PayMode));
                    EOff.PayMode = val;

                    var type = db.EmpOff.Include(e => e.PayMode).Where(e => e.Id == data).SingleOrDefault();

                    if (type.PayMode != null)
                    {
                        typedetails = db.EmpOff.Where(x => x.PayMode.Id == type.PayMode.Id && x.Id == data).SingleOrDefault();
                    }
                    else
                    {
                        typedetails = db.EmpOff.Where(x => x.Id == data).SingleOrDefault();
                    }
                    typedetails.PayMode = EOff.PayMode;
                }
                else
                {
                    typedetails = db.EmpOff.Include(e => e.PayMode).Where(x => x.Id == data).SingleOrDefault();
                    typedetails.PayMode = null;
                }
                // Paymode end */


                if (bank != null & bank != "")
                {
                    var val = db.Bank.Find(int.Parse(bank));
                    EOff.Bank = val;

                    var type = db.EmpOff.Include(e => e.Bank).Where(e => e.Id == data).SingleOrDefault();

                    if (type.Bank != null)
                    {
                        typedetails = db.EmpOff.Include(e => e.Bank).Where(x => x.Bank.Id == type.Bank.Id && x.Id == data).SingleOrDefault();
                    }
                    else
                    {
                        typedetails = db.EmpOff.Where(x => x.Id == data).SingleOrDefault();
                    }
                    typedetails.Bank = EOff.Bank;
                }
                else
                {
                    typedetails = db.EmpOff.Include(e => e.Bank).Where(x => x.Id == data).SingleOrDefault();
                    typedetails.Bank = null;
                }

                if (branch != null & branch != "")
                {
                    var val = db.Branch.Find(int.Parse(branch));
                    EOff.Branch = val;

                    var type = db.EmpOff.Include(e => e.Branch).Where(e => e.Id == data).SingleOrDefault();

                    if (type.Branch != null)
                    {
                        typedetails = db.EmpOff.Where(x => x.Branch.Id == type.Branch.Id && x.Id == data).SingleOrDefault();
                    }
                    else
                    {
                        typedetails = db.EmpOff.Where(x => x.Id == data).SingleOrDefault();
                    }
                    typedetails.Branch = EOff.Branch;
                }
                else
                {
                    typedetails = db.EmpOff.Include(e => e.Branch).Where(x => x.Id == data).SingleOrDefault();
                    typedetails.Branch = null;
                }


                if (EOff.NationalityID != null)
                {
                    EmpOff EmpOff = db.EmpOff.Include(e => e.NationalityID)
                                                     .Where(e => e.Id == data).SingleOrDefault();

                    EmpOff.NationalityID = new NationalityID
                    {
                        AdharNo = EOff.NationalityID.AdharNo,
                        DLNO = EOff.NationalityID.DLNO,
                        EDLINo = EOff.NationalityID.EDLINo,
                        ESICNo = EOff.NationalityID.ESICNo,
                        GINo = EOff.NationalityID.GINo,
                        LWFNo = EOff.NationalityID.LWFNo,
                        No1 = EOff.NationalityID.No1,
                        No2 = EOff.NationalityID.No2,
                        No3 = EOff.NationalityID.No3,
                        No4 = EOff.NationalityID.No4,
                        PANNo = EOff.NationalityID.PANNo,
                        PensionNo = EOff.NationalityID.PensionNo,
                        PFNo = EOff.NationalityID.PFNo,
                        PPNO = EOff.NationalityID.PPNO,
                        PTNo = EOff.NationalityID.PPNO,
                        RCNo = EOff.NationalityID.RCNo,
                        UANNo = EOff.NationalityID.UANNo,
                        VCNo = EOff.NationalityID.VCNo,
                    };

                    //db.EmpOff.Attach(EmpOff);
                    //db.Entry(EmpOff).State = System.Data.Entity.EntityState.Modified;
                    //db.Entry(EmpOff).OriginalValues["RowVersion"] = TempData["RowVersion"];
                }


                if (PayProcGrp != null & PayProcGrp != "")
                {
                    var val = db.PayProcessGroup.Find(int.Parse(PayProcGrp));
                    EOff.PayProcessGroup = val;

                    var type = db.EmpOff.Include(e => e.PayProcessGroup).Where(e => e.Id == data).SingleOrDefault();

                    if (type.PayProcessGroup != null)
                    {
                        typedetails = db.EmpOff.Where(x => x.PayProcessGroup.Id == type.PayProcessGroup.Id && x.Id == data).SingleOrDefault();
                    }
                    else
                    {
                        typedetails = db.EmpOff.Where(x => x.Id == data).SingleOrDefault();
                    }
                    typedetails.PayProcessGroup = EOff.PayProcessGroup;
                }
                else
                {
                    typedetails = db.EmpOff.Include(e => e.PayProcessGroup).Where(x => x.Id == data).SingleOrDefault();
                    typedetails.PayProcessGroup = null;
                }

                db.EmpOff.Attach(typedetails);
                db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["RowVersion"] = typedetails.RowVersion;
                db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;


                var CurEmpOff = db.EmpOff.Find(data);
                TempData["CurrRowVersion"] = CurEmpOff.RowVersion;
                db.Entry(CurEmpOff).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    EOff.DBTrack = dbT;
                    EmpOff EmpOfficial = new EmpOff()
                    {
                        AccountNo = EOff.AccountNo == null ? "" : EOff.AccountNo.Trim(),
                        ESICAppl = EOff.ESICAppl,
                        FamilyHandicap = EOff.FamilyHandicap,
                        HandicapRemark = EOff.HandicapRemark,
                        LWFAppl = EOff.LWFAppl,
                        PFAppl = EOff.PFAppl,
                        PTAppl = EOff.PTAppl,
                        SelfHandicap = EOff.SelfHandicap,
                        VPFAmt = EOff.VPFAmt,
                        VPFAppl = EOff.VPFAppl,
                        VPFPerc = EOff.VPFPerc,
                        DBTrack = EOff.DBTrack,
                        Id = data
                    };


                    db.EmpOff.Attach(EmpOfficial);
                    db.Entry(EmpOfficial).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(EmpOfficial).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save Authorized data
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            EmpOff EmpOff = db.EmpOff.Include(e => e.AccountType)
                                                        .Include(e => e.Bank).Include(e => e.Branch).Include(e => e.NationalityID)
                                                        .Include(e => e.PayMode).Include(e => e.PayProcessGroup)
                                                        .Where(e => e.Id == auth_id).SingleOrDefault();

                            EmpOff.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = EmpOff.DBTrack.ModifiedBy != null ? EmpOff.DBTrack.ModifiedBy : null,
                                CreatedBy = EmpOff.DBTrack.CreatedBy != null ? EmpOff.DBTrack.CreatedBy : null,
                                CreatedOn = EmpOff.DBTrack.CreatedOn != null ? EmpOff.DBTrack.CreatedOn : null,
                                IsModified = EmpOff.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.EmpOff.Attach(EmpOff);
                            db.Entry(EmpOff).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(EmpOff).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, EmpOff.DBTrack);
                            DT_EmpOff DT_EmpOff = (DT_EmpOff)rtn_Obj;
                            DT_EmpOff.AccountType_Id = EmpOff.AccountType == null ? 0 : EmpOff.AccountType.Id;
                            DT_EmpOff.Bank_Id = EmpOff.Bank == null ? 0 : EmpOff.Bank.Id;
                            DT_EmpOff.Branch_Id = EmpOff.Branch == null ? 0 : EmpOff.Branch.Id;
                            DT_EmpOff.NationalityID = EmpOff.NationalityID == null ? 0 : EmpOff.NationalityID.Id;
                            DT_EmpOff.PayMode_Id = EmpOff.PayMode == null ? 0 : EmpOff.PayMode.Id;
                            DT_EmpOff.PayProcessGroup_Id = EmpOff.PayProcessGroup == null ? 0 : EmpOff.PayProcessGroup.Id;

                            db.Create(DT_EmpOff);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorized");
                            return Json(new Utility.JsonReturnClass { Id = EmpOff.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { EmpOff.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        EmpOff Old_EmpOff = db.EmpOff.Include(e => e.AccountType)
                                                        .Include(e => e.Bank).Include(e => e.Branch).Include(e => e.NationalityID)
                                                        .Include(e => e.PayMode).Include(e => e.PayProcessGroup)
                                                        .Where(e => e.Id == auth_id).SingleOrDefault();

                        DT_EmpOff Curr_EmpOff = db.DT_EmpOff
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_EmpOff != null)
                        {
                            EmpOff EmpOff = new EmpOff();

                            string AccType = Curr_EmpOff.AccountType_Id == null ? null : Curr_EmpOff.AccountType_Id.ToString();
                            string PayMode = Curr_EmpOff.PayMode_Id == null ? null : Curr_EmpOff.PayMode_Id.ToString();
                            string bank = Curr_EmpOff.Bank_Id == null ? null : Curr_EmpOff.Bank_Id.ToString();
                            string branch = Curr_EmpOff.Branch_Id == null ? null : Curr_EmpOff.Branch_Id.ToString();
                            string national = Curr_EmpOff.NationalityID == null ? null : Curr_EmpOff.NationalityID.ToString();
                            string ProcGrp = Curr_EmpOff.PayProcessGroup_Id == null ? null : Curr_EmpOff.PayProcessGroup_Id.ToString();
                            EmpOff.AccountNo = Curr_EmpOff.AccountNo == null ? Old_EmpOff.AccountNo : Curr_EmpOff.AccountNo;
                            EmpOff.ESICAppl = Curr_EmpOff.ESICAppl == false ? Old_EmpOff.ESICAppl : Curr_EmpOff.ESICAppl;
                            EmpOff.FamilyHandicap = Curr_EmpOff.FamilyHandicap == false ? Old_EmpOff.FamilyHandicap : Curr_EmpOff.FamilyHandicap;
                            EmpOff.HandicapRemark = Curr_EmpOff.HandicapRemark == null ? Old_EmpOff.HandicapRemark : Curr_EmpOff.HandicapRemark;
                            EmpOff.LWFAppl = Curr_EmpOff.LWFAppl == false ? Old_EmpOff.LWFAppl : Curr_EmpOff.LWFAppl;
                            EmpOff.PFAppl = Curr_EmpOff.PFAppl == false ? Old_EmpOff.PFAppl : Curr_EmpOff.PFAppl;
                            EmpOff.PTAppl = Curr_EmpOff.PTAppl == false ? Old_EmpOff.PTAppl : Curr_EmpOff.PTAppl;
                            EmpOff.SelfHandicap = Curr_EmpOff.SelfHandicap == false ? Old_EmpOff.SelfHandicap : Curr_EmpOff.SelfHandicap;
                            EmpOff.VPFAmt = Curr_EmpOff.VPFAmt == 0 ? Old_EmpOff.VPFAmt : Curr_EmpOff.VPFAmt;
                            EmpOff.VPFAppl = Curr_EmpOff.VPFAppl == false ? Old_EmpOff.VPFAppl : Curr_EmpOff.VPFAppl;
                            EmpOff.VPFPerc = Curr_EmpOff.VPFPerc == 0 ? Old_EmpOff.VPFPerc : Curr_EmpOff.VPFPerc;

                            if (ModelState.IsValid)
                            {
                                try
                                {
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        EmpOff.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_EmpOff.DBTrack.CreatedBy == null ? null : Old_EmpOff.DBTrack.CreatedBy,
                                            CreatedOn = Old_EmpOff.DBTrack.CreatedOn == null ? null : Old_EmpOff.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_EmpOff.DBTrack.ModifiedBy == null ? null : Old_EmpOff.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_EmpOff.DBTrack.ModifiedOn == null ? null : Old_EmpOff.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        int a = EditS(AccType, PayMode, bank, branch, "", ProcGrp, auth_id, EmpOff, EmpOff.DBTrack);
                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Authorized");
                                        return Json(new Utility.JsonReturnClass { Id = EmpOff.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { EmpOff.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Corporate)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (Corporate)databaseEntry.ToObject();
                                        EmpOff.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                        {
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        //return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            EmpOff EmpOff = db.EmpOff.Include(e => e.AccountType)
                                                        .Include(e => e.Bank).Include(e => e.Branch).Include(e => e.NationalityID)
                                                        .Include(e => e.PayMode).Include(e => e.PayProcessGroup)
                                                        .Where(e => e.Id == auth_id).SingleOrDefault();

                            LookupValue AccType = EmpOff.AccountType;
                            LookupValue PayMode = EmpOff.PayMode;
                            Bank bank = EmpOff.Bank;
                            Branch branch = EmpOff.Branch;
                            NationalityID national = EmpOff.NationalityID;
                            PayProcessGroup ProcGrp = EmpOff.PayProcessGroup;

                            EmpOff.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = EmpOff.DBTrack.ModifiedBy != null ? EmpOff.DBTrack.ModifiedBy : null,
                                CreatedBy = EmpOff.DBTrack.CreatedBy != null ? EmpOff.DBTrack.CreatedBy : null,
                                CreatedOn = EmpOff.DBTrack.CreatedOn != null ? EmpOff.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.EmpOff.Attach(EmpOff);
                            db.Entry(EmpOff).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, EmpOff.DBTrack);
                            DT_EmpOff DT_EmpOff = (DT_EmpOff)rtn_Obj;
                            DT_EmpOff.AccountType_Id = EmpOff.AccountType == null ? 0 : EmpOff.AccountType.Id;
                            DT_EmpOff.Bank_Id = EmpOff.Bank == null ? 0 : EmpOff.Bank.Id;
                            DT_EmpOff.Branch_Id = EmpOff.Branch == null ? 0 : EmpOff.Branch.Id;
                            DT_EmpOff.NationalityID = EmpOff.NationalityID == null ? 0 : EmpOff.NationalityID.Id;
                            DT_EmpOff.PayMode_Id = EmpOff.PayMode == null ? 0 : EmpOff.PayMode.Id;
                            DT_EmpOff.PayProcessGroup_Id = EmpOff.PayProcessGroup == null ? 0 : EmpOff.PayProcessGroup.Id;
                            db.Create(DT_EmpOff);
                            await db.SaveChangesAsync();
                            db.Entry(EmpOff).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorized ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Record Authorized", JsonRequestBehavior.AllowGet });
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
                    EmpOff EmpOff = db.EmpOff.Include(e => e.AccountType)
                                                       .Include(e => e.Bank).Include(e => e.Branch).Include(e => e.NationalityID)
                                                       .Include(e => e.PayMode).Include(e => e.PayProcessGroup)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    LookupValue AccType = EmpOff.AccountType;
                    LookupValue PayMode = EmpOff.PayMode;
                    Bank bank = EmpOff.Bank;
                    Branch branch = EmpOff.Branch;
                    NationalityID national = EmpOff.NationalityID;
                    PayProcessGroup ProcGrp = EmpOff.PayProcessGroup;

                    if (EmpOff.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = EmpOff.DBTrack.CreatedBy != null ? EmpOff.DBTrack.CreatedBy : null,
                                CreatedOn = EmpOff.DBTrack.CreatedOn != null ? EmpOff.DBTrack.CreatedOn : null,
                                IsModified = EmpOff.DBTrack.IsModified == true ? true : false
                            };
                            EmpOff.DBTrack = dbT;
                            db.Entry(EmpOff).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, EmpOff.DBTrack);
                            DT_EmpOff DT_EmpOff = (DT_EmpOff)rtn_Obj;
                            DT_EmpOff.AccountType_Id = EmpOff.AccountType == null ? 0 : EmpOff.AccountType.Id;
                            DT_EmpOff.PayMode_Id = EmpOff.PayMode == null ? 0 : EmpOff.PayMode.Id;
                            DT_EmpOff.Bank_Id = EmpOff.Bank == null ? 0 : EmpOff.Bank.Id;
                            DT_EmpOff.Branch_Id = EmpOff.Branch == null ? 0 : EmpOff.Branch.Id;
                            DT_EmpOff.NationalityID = EmpOff.NationalityID == null ? 0 : EmpOff.NationalityID.Id;
                            DT_EmpOff.PayProcessGroup_Id = EmpOff.PayProcessGroup == null ? 0 : EmpOff.PayProcessGroup.Id;
                            db.Create(DT_EmpOff);

                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                        }
                    }
                    else
                    {


                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            var EmpOffDetails = db.Employee.Include(e => e.EmpOffInfo).Where(e => e.EmpOffInfo.Id == EmpOff.Id);
                            if (EmpOffDetails != null)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            }
                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = EmpOff.DBTrack.CreatedBy != null ? EmpOff.DBTrack.CreatedBy : null,
                                    CreatedOn = EmpOff.DBTrack.CreatedOn != null ? EmpOff.DBTrack.CreatedOn : null,
                                    IsModified = EmpOff.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(EmpOff).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_EmpOff DT_EmpOff = (DT_EmpOff)rtn_Obj;
                                DT_EmpOff.AccountType_Id = AccType == null ? 0 : AccType.Id;
                                DT_EmpOff.PayMode_Id = PayMode == null ? 0 : PayMode.Id;
                                DT_EmpOff.Bank_Id = bank == null ? 0 : bank.Id;
                                DT_EmpOff.Branch_Id = branch == null ? 0 : branch.Id;
                                DT_EmpOff.NationalityID = national == null ? 0 : national.Id;
                                DT_EmpOff.PayProcessGroup_Id = ProcGrp == null ? 0 : ProcGrp.Id;

                                db.Create(DT_EmpOff);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
        }


        public ActionResult GetProcessGrpLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.PayProcessGroup.Include(e => e.PayFrequency).Include(e => e.PayrollPeriod).ToList();
                IEnumerable<PayProcessGroup> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.PayProcessGroup.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    ////var list1 = db.PayProcessGroup.ToList().Select(e => e.Address);
                    ////var list2 = fall.Except(list1);

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    //// var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }




        public class CanClass
        {
            public int Id { get; set; }
            public string CanCode { get; set; }
            public string CanName { get; set; }

        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Acc_No { get; set; }
            public string PANCARD { get; set; }
        }

        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;

        //        IEnumerable<P2BGridData> SalaryList = null;
        //        List<P2BGridData> model = new List<P2BGridData>();
        //        P2BGridData view = null;
        //        string PayMonth = "";
        //        string Month = "";
        //        if (gp.filter != null)
        //            PayMonth = gp.filter;
        //        else
        //        {
        //            if (DateTime.Now.Date.Month < 10)
        //                Month = "0" + DateTime.Now.Date.Month;
        //            else
        //                Month = DateTime.Now.Date.Month.ToString();
        //            PayMonth = Month + "/" + DateTime.Now.Date.Year;
        //        }

        //        //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
        //        //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

        //        var BindEmpList = db.Candidate.Include(e => e.CanOffInfo).Include(e => e.CanName)
        //                                      .Include(e => e.CanOffInfo.NationalityID)
        //                                      .ToList();

        //        foreach (var z in BindEmpList)
        //        {
        //            if (z.CanOffInfo != null)
        //            {

        //                view = new P2BGridData()
        //                {
        //                    Id = z.CanOffInfo.Id,
        //                    Code = z.CanCode,
        //                    Name = z.CanName != null ? z.CanName.FullNameFML : null,
        //                    Acc_No = z.CanOffInfo.AccountNo != null ? z.CanOffInfo.AccountNo : null,
        //                    PANCARD = z.CanOffInfo.NationalityID.PANNo != null ? z.CanOffInfo.NationalityID.PANNo : null,

        //                };
        //                model.Add(view);


        //            }

        //        }

        //        SalaryList = model;

        //        IEnumerable<P2BGridData> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = SalaryList;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.Acc_No, a.PANCARD }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "Code")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.Acc_No, a.PANCARD }).Where((e => (e.Code.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "Name")
        //                    jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.Acc_No, a.PANCARD }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();



        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.Acc_No != null ? Convert.ToString(a.Acc_No) : null, a.PANCARD != null ? Convert.ToString(a.PANCARD) : null }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = SalaryList;
        //            Func<P2BGridData, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
        //                                 gp.sidx == "Name" ? c.Name.ToString() :
        //                                 "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name, a.Acc_No != null ? Convert.ToString(a.Acc_No) : null, a.PANCARD != null ? Convert.ToString(a.PANCARD) : null }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name, a.Acc_No != null ? Convert.ToString(a.Acc_No) : null, a.PANCARD != null ? Convert.ToString(a.PANCARD) : null }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.Acc_No != null ? Convert.ToString(a.Acc_No) : null, a.PANCARD != null ? Convert.ToString(a.PANCARD) : null }).ToList();
        //            }
        //            totalRecords = SalaryList.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}


        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> SalaryList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    string PayMonth = "";
                    string Month = "";
                    if (gp.filter != null)
                        PayMonth = gp.filter;
                    else
                    {
                        if (DateTime.Now.Date.Month < 10)
                            Month = "0" + DateTime.Now.Date.Month;
                        else
                            Month = DateTime.Now.Date.Month.ToString();
                        PayMonth = Month + "/" + DateTime.Now.Date.Year;
                    }

                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

                    var BindEmpList = db.Candidate.Include(e => e.CanOffInfo).Include(e => e.CanName)
                        .Include(e => e.CanOffInfo.NationalityID)
                        .ToList();

                    foreach (var z in BindEmpList)
                    {
                        if (z.CanOffInfo != null)
                        {

                            view = new P2BGridData()
                            {
                                Id = z.CanOffInfo.Id,
                                Code = z.CanCode,
                                Name = z.CanName != null ? z.CanName.FullNameFML : null,
                                Acc_No = z.CanOffInfo.AccountNo != null ? z.CanOffInfo.AccountNo : null,
                                PANCARD = z.CanOffInfo.NationalityID.PANNo != null ? z.CanOffInfo.NationalityID.PANNo : null,

                            };
                            model.Add(view);


                        }

                    }

                    SalaryList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = SalaryList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            if (gp.searchField == "Id")
                                jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.Acc_No, a.PANCARD }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                            if (gp.searchField == "Code")
                                jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.Acc_No, a.PANCARD }).Where((e => (e.Code.ToString().Contains(gp.searchString)))).ToList();
                            if (gp.searchField == "Name")
                                jsonData = IE.Select(a => new { a.Id, a.Code, a.Name, a.Acc_No, a.PANCARD }).Where((e => (e.Name.ToString().Contains(gp.searchString)))).ToList();



                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.Acc_No != null ? Convert.ToString(a.Acc_No) : null, a.PANCARD != null ? Convert.ToString(a.PANCARD) : null }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = SalaryList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
                                             gp.sidx == "Name" ? c.Name.ToString() :
                                             "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name, a.Acc_No != null ? Convert.ToString(a.Acc_No) : null, a.PANCARD != null ? Convert.ToString(a.PANCARD) : null }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name, a.Acc_No != null ? Convert.ToString(a.Acc_No) : null, a.PANCARD != null ? Convert.ToString(a.PANCARD) : null }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.Acc_No != null ? Convert.ToString(a.Acc_No) : null, a.PANCARD != null ? Convert.ToString(a.PANCARD) : null }).ToList();
                        }
                        totalRecords = SalaryList.Count();
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
        }




        //public ActionResult P2BEmpGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;

        //        IEnumerable<CanClass> EmployeeList = null;
        //        List<CanClass> model = new List<CanClass>();
        //        CanClass view = null;


        //        //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
        //        //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

        //        var BindEmpList = db.Candidate.Where(e => e.CanOffInfo == null).Include(e => e.CanName)
        //                                      .ToList();


        //        foreach (var z in BindEmpList)
        //        {
        //            if (z != null)
        //            {

        //                view = new CanClass()
        //                {
        //                    Id = z.Id,
        //                    CanCode = z.CanCode.ToString() != null ? z.CanCode.ToString() : "",
        //                    CanName = z.CanName.FullNameFML.ToString() != null ? z.CanName.FullNameFML.ToString() : ""

        //                };
        //                model.Add(view);


        //            }

        //        }

        //        EmployeeList = model;

        //        IEnumerable<CanClass> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = EmployeeList;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.CanCode, a.CanName }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "CanCode")
        //                    jsonData = IE.Select(a => new { a.Id, a.CanCode, a.CanName }).Where((e => (e.CanCode.ToString().Contains(gp.searchString)))).ToList();
        //                if (gp.searchField == "CanName")
        //                    jsonData = IE.Select(a => new { a.Id, a.CanCode, a.CanName }).Where((e => (e.CanCode.ToString().Contains(gp.searchString)))).ToList();



        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.CanCode != null ? Convert.ToString(a.CanCode) : "", a.CanName != null ? Convert.ToString(a.CanName) : "" }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = EmployeeList;
        //            Func<CanClass, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Code" ? c.CanCode.ToString() :
        //                                 gp.sidx == "Name" ? c.CanName.ToString() :
        //                                 "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.CanCode != null ? Convert.ToString(a.CanCode) : "", a.CanName != null ? Convert.ToString(a.CanName) : "" }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.CanCode != null ? Convert.ToString(a.CanCode) : "", a.CanName != null ? Convert.ToString(a.CanName) : "" }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.CanCode != null ? Convert.ToString(a.CanCode) : "", a.CanName != null ? Convert.ToString(a.CanName) : "" }).ToList();
        //            }
        //            totalRecords = EmployeeList.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public ActionResult P2BEmpGrid(P2BGrid_Parameters gp)
        //{

        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<EmpClass> Employee = null;

        //        List<EmpClass> EmpClassList = new List<EmpClass>();
        //        var data = db.Employee.Where(e=>e.EmpOffInfo == null).Include(e => e.EmpName).ToList();
        //        foreach (var item in data)
        //        {
        //            EmpClassList.Add(new EmpClass()
        //            {
        //                Id = item.Id,
        //                EmpName = item.EmpName != null ? item.EmpName.FullNameFML : null,
        //                EmpCode = item.EmpCode!= null ? item.EmpCode:null
        //            });
        //        }
        //        IEnumerable<EmpClass> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {

        //            IE = Employee;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.EmpCode, a.EmpName }).Where((e => (e.Id.ToString() == gp.searchString) || (e.EmpCode.ToLower() == gp.searchString.ToLower()) || (e.EmpName.ToLower() == gp.searchString.ToLower()))).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpCode, a.EmpName }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {

        //                IE = Employee;
        //                Func<EmpClass, dynamic> orderfuc;
        //                if (gp.sidx == "Id")
        //                {
        //                    orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //                }
        //                else
        //                {
        //                    orderfuc = (c => gp.sidx == "Code" ? c.EmpCode :
        //                                     gp.sidx == "Name" ? c.EmpName : "");
        //                }
        //                if (gp.sord == "asc")
        //                {
        //                    IE = IE.OrderBy(orderfuc);
        //                    jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.EmpCode), Convert.ToString(a.EmpName) }).ToList();
        //                }
        //                else if (gp.sord == "desc")
        //                {
        //                    IE = IE.OrderByDescending(orderfuc);
        //                    jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.EmpCode), Convert.ToString(a.EmpName) }).ToList();
        //                }
        //                if (pageIndex > 1)
        //                {
        //                    int h = pageIndex * pageSize;
        //                    jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpCode, a.EmpName }).ToList();
        //                }
        //                totalRecords = Employee.Count();


        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public ActionResult P2BEmpGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<CanClass> EmployeeList = null;
                    List<CanClass> model = new List<CanClass>();
                    CanClass view = null;


                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

                    var BindEmpList = db.Candidate.Where(e => e.CanOffInfo == null).Include(e => e.CanName)
                        .ToList();

                    foreach (var z in BindEmpList)
                    {
                        if (z != null && z.CanName != null)
                        {

                            view = new CanClass()
                            {
                                Id = z.Id,
                                CanCode = z.CanCode.ToString() != null ? z.CanCode.ToString() : "",
                                CanName = z.CanName.FullNameFML.ToString() != null ? z.CanName.FullNameFML.ToString() : ""

                            };
                            model.Add(view);


                        }

                    }

                    EmployeeList = model;

                    IEnumerable<CanClass> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = EmployeeList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            if (gp.searchField == "Id")
                                jsonData = IE.Select(a => new { a.Id, a.CanCode, a.CanName }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                            if (gp.searchField == "CanCode")
                                jsonData = IE.Select(a => new { a.Id, a.CanCode, a.CanName }).Where((e => (e.CanCode.ToString().Contains(gp.searchString)))).ToList();
                            if (gp.searchField == "CanName")
                                jsonData = IE.Select(a => new { a.Id, a.CanCode, a.CanName }).Where((e => (e.CanName.ToString().Contains(gp.searchString)))).ToList();



                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.CanCode != null ? Convert.ToString(a.CanCode) : "", a.CanName != null ? Convert.ToString(a.CanName) : "" }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = EmployeeList;
                        Func<CanClass, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Code" ? c.CanCode.ToString() :
                                             gp.sidx == "Name" ? c.CanName.ToString() :
                                             "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.CanCode != null ? Convert.ToString(a.CanCode) : "", a.CanName != null ? Convert.ToString(a.CanName) : "" }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.CanCode != null ? Convert.ToString(a.CanCode) : "", a.CanName != null ? Convert.ToString(a.CanName) : "" }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.CanCode != null ? Convert.ToString(a.CanCode) : "", a.CanName != null ? Convert.ToString(a.CanName) : "" }).ToList();
                        }
                        totalRecords = EmployeeList.Count();
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
        }
    }
}