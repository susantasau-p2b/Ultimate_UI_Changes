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
using Payroll;
using P2BUltimate.Process;
using P2B.SERVICES.Interface;
using System.Globalization;
namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class EmpOffController : Controller
    {
       
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /EmpOff/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/EmpOff/Index.cshtml");
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

        public ActionResult DropdownPFTrustEstablishment(string data, string data2)
        {
            using(DataBaseContext db = new DataBaseContext())
            {
                var getPFmaster = db.PFMaster.ToList();
                var Selected = (Object)null;

                int getFirstSelectedVal = db.PFMaster.FirstOrDefault().Id;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    Selected = Convert.ToInt32(data2);
                }
                else 
                {
                    Selected = getFirstSelectedVal != 0 ? getFirstSelectedVal : 0;
                }


                SelectList ss = new SelectList(getPFmaster, "Id", "EstablishmentID", Selected);
                return Json(ss, JsonRequestBehavior.AllowGet);
            }
            
        }

        //public ActionResult PopulateDropDownBranchList(string data, string data2)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var qurey = db.Branch.ToList();
        //        var selected = (Object)null;
        //        if (data2 != "" && data != "0" && data2 != "0")
        //        {
        //            selected = Convert.ToInt32(data2);
        //        }

        //        SelectList s = new SelectList(qurey, "Id", "Name", selected);
        //        return Json(s, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult PopulateDropDownBranchList(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Branch.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Branch.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }
                var P = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(P, JsonRequestBehavior.AllowGet);
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
                    string pfTrustEstablishment = form["PFTrust_EstablishmentId_drop"] == "0" ? "" : form["PFTrust_EstablishmentId_drop"];
                    string PayMode = form["PayMode_drop"] == "0" ? "" : form["PayMode_drop"];
                    string Bank = form["Bank_drop"] == "0" ? "" : form["Bank_drop"];
                    string Branch = form["Branchlist"] == "0" ? "" : form["Branchlist"];
                    string AccountType = form["AccountType_drop"] == "0" ? "" : form["AccountType_drop"];
                    // string PayScale = form["PayScalelist"] == "0" ? "" : form["PayScalelist"];
                    string PayProcessGrp = form["AddressList"] == "0" ? "" : form["PayProcessGrouplist"];
                    string PayScale_ddl = form["PayScalelist"] == "0" ? "" : form["PayScalelist"];
                    if (PayScale_ddl != null || PayScale_ddl != "" || PayScale_ddl != "0")
                    {
                        var value = db.PayScale.Find(int.Parse(PayScale_ddl));
                        EOff.PayScale = value;

                    }
                    int emp_Id = form["emp_Id"] == "0" ? 0 : Convert.ToInt32(form["emp_Id"]);


                    if (PayMode != null && PayMode != "" && PayMode != "-Select-")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "404").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(PayMode)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(PayMode));
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
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "405").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(AccountType)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(AccountType));
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

                    if (pfTrustEstablishment != null && pfTrustEstablishment != "")
                    {
                        int getPfmasterId = Convert.ToInt32(pfTrustEstablishment);
                        EOff.PFTrust_EstablishmentId = db.PFMaster.Find(getPfmasterId).EstablishmentID != null ? db.PFMaster.Find(getPfmasterId).EstablishmentID : "";
                    }

                    if (ModelState.IsValid)
                    {
                        Employee OEmployee = db.Employee.Find(emp_Id);

                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db..Any(o => o.Code == c.Code))
                            //{
                            //        Msg.Add("  Code Already Exists.  ");
                            // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            EOff.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            EmpOff EmpOfficial = new EmpOff()
                            {
                                PFTrust_EstablishmentId = EOff.PFTrust_EstablishmentId,
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
                                PensionAppl = EOff.PensionAppl,
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


                                if (OEmployee != null)
                                {
                                    OEmployee.EmpOffInfo = EmpOfficial;
                                    db.Employee.Attach(OEmployee);
                                    db.Entry(OEmployee).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(OEmployee).State = System.Data.Entity.EntityState.Detached;
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
            //TempData["editdata"] = data;
            using (DataBaseContext db = new DataBaseContext())
            {

                //IP2BSecurity security = new P2B.SERVICES.Factory.P2BSecurity().RegisterSettings();

                //var AdharandPAN = db.EmpOff.Include(e => e.AccountType).Include(e => e.PayMode).Include(e => e.PayScale)
                //  .Where(e => e.Id == data).Select(s => new { adhar = s.NationalityID.AdharNo.ToString(),
                //                                              pan = s.NationalityID.PANNo.ToString()}).FirstOrDefault();

                //var AdharPlaintext = security.Masking(AdharandPAN.adhar, 4);
                //var AdharMask = AdharPlaintext.First(e => e.Key.Equals("MaskedText")).Value.ToString();

                //var PanPlaintext = security.Masking(AdharandPAN.pan, 4);
                //var PANMask = PanPlaintext.First(e => e.Key.Equals("MaskedText")).Value.ToString();



                //NationalityID NationalityDetails = db.EmpOff.Include(e => e.AccountType).Include(e => e.PayMode).Include(e => e.PayScale)
                //   .Where(e => e.Id == data).Select(n => new
                //   {
                //       id = n.NationalityID.Id,
                //       adharno = AdharMask,
                //       panno = PANMask,
                //       dlno = n.NationalityID.DLNO,
                //       edlino = n.NationalityID.EDLINo,
                //       esicno = n.NationalityID.ESICNo,
                //       gino = n.NationalityID.GINo,
                //       higePension = n.NationalityID.HigherPension,
                //       highPensionper = n.NationalityID.HigherPensionPer,
                //       lwfno = n.NationalityID.LWFNo,
                //       no1 = n.NationalityID.No1,
                //       no2 = n.NationalityID.No2,
                //       no3 = n.NationalityID.No3,
                //       no4 = n.NationalityID.No4,
                //       no5 = n.NationalityID.No5,
                //       no6 = n.NationalityID.No6,
                //       no7 = n.NationalityID.No7,
                //       no8 = n.NationalityID.No8,
                //       pensionno = n.NationalityID.PensionNo,
                //       pfno = n.NationalityID.PFNo,
                //       ppno = n.NationalityID.PPNO,
                //       ptno = n.NationalityID.PTNo,
                //       rcno = n.NationalityID.RCNo,
                //       uano = n.NationalityID.UANNo,
                //       vcno = n.NationalityID.VCNo,


                //   });


                var Q = db.EmpOff.Include(e => e.AccountType).Include(e => e.PayMode).Include(e => e.PayScale)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        PayMode_Id = e.PayMode.Id == null ? 0 : e.PayMode.Id,
                        Paymode_details = e.PayMode != null ? e.PayMode.LookupVal : null,
                        PayScale_Id = e.PayScale.Id == null ? 0 : e.PayScale.Id,
                        Payscale_FullDetails = e.PayScale.FullDetails,
                        Bank_Id = e.Bank.Id == null ? 0 : e.Bank.Id,
                        //Branch_Id = e.Branch.Id == null ? 0 : e.Branch.Id,
                        AccountType_Id = e.AccountType.Id == null ? 0 : e.AccountType.Id,
                        AccountNo = e.AccountNo == null ? "" : e.AccountNo,
                        PFAppl = e.PFAppl,
                        PensionAppl = e.PensionAppl,
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
                        NationalityID = e.NationalityID != null ? e.NationalityID : null,
                        Branch_Id = e.Branch != null ? e.Branch.Id : 0,
                        Branch_FullDetails = e.Branch != null ? e.Branch.FullDetails : null,
                        pfEstablishmentValue = e.PFTrust_EstablishmentId != "" ? e.PFTrust_EstablishmentId : "",
                        

                    }).ToList();
                string getpfEstablishmentValueStr = db.EmpOff.Where(e => e.Id == data).FirstOrDefault().PFTrust_EstablishmentId;

                var pfMasterId = getpfEstablishmentValueStr != null ? db.PFMaster.Where(e => e.EstablishmentID == getpfEstablishmentValueStr).FirstOrDefault().Id.ToString() : "";

                //var a = Q.FirstOrDefault().NationalityID.AdharNo.ToString();
                //var AdharPlaintext = security.Masking(a, 4);
                //var AdharMask = AdharPlaintext.First(e => e.Key.Equals("MaskedText")).Value.ToString();

                //Q.FirstOrDefault().NationalityID.AdharNo = AdharMask;

                //var b = Q.FirstOrDefault().NationalityID.PANNo.ToString();
                //var PanPlaintext = security.Masking(b, 4);
                //var PANMask = PanPlaintext.First(e => e.Key.Equals("MaskedText")).Value.ToString();

                //Q.FirstOrDefault().NationalityID.PANNo = PANMask;




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
                return Json(new Object[] { Q, add_data, W, Auth, pfMasterId, JsonRequestBehavior.AllowGet });
            }
        }

        //public ActionResult getMaskValue(string a)
        //{
        //    var geteditdata = Convert.ToInt32(TempData["editdata"]);
        //    try
        //    {
        //        using (DataBaseContext db = new DataBaseContext())
        //        {
        //            EmpOff tempEmpOff = db.EmpOff.Include(e => e.NationalityID).Where(e => e.Id == geteditdata).FirstOrDefault();
        //            var b = tempEmpOff.NationalityID.PANNo.ToUpper().ToString();
        //            return Json(new Object[] { b, JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    catch (Exception Ex)
        //    {
        //        throw Ex;
        //    }


        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(EmpOff EOff, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string PayMode = form["PayMode_drop"] == "0" ? "" : form["PayMode_drop"];
                    string pfTrustEstablishment = form["PFTrust_EstablishmentId_drop"] == "0" ? "" : form["PFTrust_EstablishmentId_drop"];
                    string Bank = form["Bank_drop"] == "0" ? "" : form["Bank_drop"];
                    string Branch = form["Branchlist"] == "0" ? "" : form["Branchlist"];
                    string AccountType = form["AccountType_drop"] == "0" ? "" : form["AccountType_drop"];
                    string PayScale = form["PayScalelist"] == "0" ? "" : form["PayScalelist"];
                    string PayProcessGrp = form["AddressList"] == "0" ? "" : form["PayProcessGrouplist"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string Taxappl = form["TaxAppl"] == "0" ? "" : form["TaxAppl"];
                    Taxappl = Taxappl == "true" ? "Yes" : "No";
                    string HigherPension = form["HigherPension"] == "0" ? "" : form["HigherPension"];
                    HigherPension = HigherPension == "true" ? "true" : "false";
                    bool HigherPension1 = Convert.ToBoolean(HigherPension);

                    EOff.PayMode_Id = PayMode != null && PayMode != "" ? int.Parse(PayMode) : 0;
                    EOff.Bank_Id = Bank != null && Bank != "" && Bank != "-Select-" ? int.Parse(Bank) : 0;
                    EOff.Branch_Id = Branch != null && Branch != "" && Branch != "-Select-" ? int.Parse(Branch) : 0;
                    EOff.AccountType_Id = AccountType != null && AccountType != "" ? int.Parse(AccountType) : 0;
                    EOff.PayProcessGroup_Id = PayProcessGrp != null && PayProcessGrp != "" ? int.Parse(PayProcessGrp) : 0;
                    EOff.PayScale_Id = PayScale != null && PayScale != "" ? int.Parse(PayScale) : 0;

                    if (pfTrustEstablishment != null && pfTrustEstablishment != "")
                    {
                        int getPfmasterId = Convert.ToInt32(pfTrustEstablishment);
                        EOff.PFTrust_EstablishmentId = db.PFMaster.Find(getPfmasterId).EstablishmentID != null ? db.PFMaster.Find(getPfmasterId).EstablishmentID : "";
                    }
                    

                    EmpOff tempEmpOff = db.EmpOff.Include(e => e.NationalityID).Where(e => e.Id == data).FirstOrDefault();

                    // var tempPAN = ModelState.First(x => x.Key.Equals("NationalityID.PANNo")).Value.ToString();
                    // var a = tempEmpOff.NationalityID.PANNo.ToUpper().ToString();

                    //  ModelState.SetModelValue("NationalityID.PANNo", new ValueProviderResult(a, a, CultureInfo.CurrentCulture));

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

                                    blog = db.EmpOff.Include(e => e.AccountType)
                                                   .Include(e => e.Bank).Include(e => e.Branch).Include(e => e.NationalityID)
                                                   .Include(e => e.PayMode).Include(e => e.PayProcessGroup).Include(e => e.PayScale)
                                                   .Where(e => e.Id == data).SingleOrDefault();
                                    originalBlogValues = db.Entry(blog).OriginalValues;

                                    EOff.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };




                                    if (EOff.NationalityID != null)
                                    {
                                        EmpOff OEmpOffN = db.EmpOff.Include(e => e.NationalityID).Where(e => e.Id == data).FirstOrDefault();

                                        NationalityID NationId = OEmpOffN.NationalityID;
                                        //EOff.NationalityID.Id = NationId.Id;

                                        NationId.AdharNo = EOff.NationalityID.AdharNo;
                                        NationId.DLNO = EOff.NationalityID.DLNO;
                                        NationId.EDLINo = EOff.NationalityID.EDLINo;
                                        NationId.ESICNo = EOff.NationalityID.ESICNo;
                                        NationId.GINo = EOff.NationalityID.GINo;
                                        NationId.LWFNo = EOff.NationalityID.LWFNo;
                                        NationId.No1 = EOff.NationalityID.No1;
                                        NationId.No2 = Taxappl;
                                        NationId.No3 = EOff.NationalityID.No3;
                                        NationId.No4 = EOff.NationalityID.No4;
                                        NationId.PANNo = EOff.NationalityID.PANNo;
                                        NationId.PensionNo = EOff.NationalityID.PensionNo;
                                        NationId.PFNo = EOff.NationalityID.PFNo;
                                        NationId.PPNO = EOff.NationalityID.PPNO;
                                        NationId.PTNo = EOff.NationalityID.PPNO;
                                        NationId.RCNo = EOff.NationalityID.RCNo;
                                        NationId.UANNo = EOff.NationalityID.UANNo;
                                        NationId.VCNo = EOff.NationalityID.VCNo;
                                        NationId.No5 = EOff.NationalityID.No5;
                                        NationId.No6 = EOff.NationalityID.No6;
                                        NationId.No7 = EOff.NationalityID.No7;
                                        NationId.No8 = EOff.NationalityID.No8;
                                        NationId.HigherPension = HigherPension1;
                                        NationId.HigherPensionPer = EOff.NationalityID.HigherPensionPer;
                                       

                                        if (EOff.PayScale_Id == 0)
                                        {
                                            OEmpOffN.PayScale = null;
                                        }
                                        if (EOff.PayMode_Id == 0)
                                        {
                                            OEmpOffN.PayMode_Id = null;
                                        }
                                        if (EOff.PayProcessGroup_Id == 0)
                                        {
                                            OEmpOffN.PayProcessGroup = null;
                                        }
                                        if (EOff.Bank_Id == 0)
                                        {
                                            OEmpOffN.Bank = null;
                                        }
                                        if (EOff.Branch_Id == 0)
                                        {
                                            OEmpOffN.Branch = null;
                                        }

                                        if (EOff.AccountType_Id == 0)
                                        {
                                            OEmpOffN.AccountType_Id = null;
                                        }

                                        db.NationalityID.Attach(NationId);
                                        db.Entry(NationId).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();

                                        OEmpOffN.NationalityID = NationId;
                                        db.EmpOff.Attach(OEmpOffN);
                                        db.Entry(OEmpOffN).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        TempData["RowVersion"] = OEmpOffN.RowVersion;

                                        //db.EmpOff.Attach(EmpOff);
                                        //db.Entry(EmpOff).State = System.Data.Entity.EntityState.Modified;
                                        //db.Entry(EmpOff).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }


                                    var EmpOfficial = db.EmpOff.Find(data);
                                    TempData["CurrRowVersion"] = EmpOfficial.RowVersion;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        EmpOfficial.AccountNo = EOff.AccountNo == null ? "" : EOff.AccountNo.Trim();
                                        EmpOfficial.ESICAppl = EOff.ESICAppl;
                                        EmpOfficial.FamilyHandicap = EOff.FamilyHandicap;
                                        EmpOfficial.HandicapRemark = EOff.HandicapRemark;
                                        EmpOfficial.LWFAppl = EOff.LWFAppl;
                                        EmpOfficial.PFAppl = EOff.PFAppl;
                                        EmpOfficial.PensionAppl = EOff.PensionAppl;
                                        EmpOfficial.PTAppl = EOff.PTAppl;
                                        EmpOfficial.SelfHandicap = EOff.SelfHandicap;
                                        EmpOfficial.VPFAmt = EOff.VPFAmt;
                                        EmpOfficial.VPFAppl = EOff.VPFAppl;
                                        EmpOfficial.VPFPerc = EOff.VPFPerc;
                                        EmpOfficial.DBTrack = EOff.DBTrack;
                                        EmpOfficial.Id = data;
                                        //EmpOfficial.AccountType_Id = EOff.AccountType_Id;
                                        //if (EOff.AccountType_Id == 0)
                                        //{
                                        //    EmpOfficial.AccountType_Id = null;
                                        //}
                                        //if (EOff.AccountType_Id != 0)
                                        //{
                                        //    EmpOfficial.Bank_Id = EOff.Bank_Id;
                                        //}
                                        if (EOff.AccountType_Id != 0)
                                            EmpOfficial.AccountType_Id = EOff.AccountType_Id != null ? EOff.AccountType_Id : 0;

                                        if (EOff.Bank_Id != 0)
                                        {
                                            EmpOfficial.Bank = db.Bank.Find(EOff.Bank_Id);
                                            EmpOfficial.Bank_Id = EOff.Bank_Id != null ? EOff.Bank_Id : 0;
                                        }

                                        if (EOff.Branch_Id != 0)
                                            EmpOfficial.Branch_Id = EOff.Branch_Id != null ? EOff.Branch_Id : 0;

                                        if (EOff.PayMode_Id != 0)
                                            EmpOfficial.PayMode_Id = EOff.PayMode_Id != null ? EOff.PayMode_Id : 0;

                                        if (EOff.PayProcessGroup_Id != 0)
                                            EmpOfficial.PayProcessGroup_Id = EOff.PayProcessGroup_Id != null ? EOff.PayProcessGroup_Id : 0;

                                        if (EOff.PayScale_Id != 0)
                                            EmpOfficial.PayScale_Id = EOff.PayScale_Id != null ? EOff.PayScale_Id : 0;

                                        if (EOff.PFTrust_EstablishmentId != "" && EOff.PFTrust_EstablishmentId != null)
                                            EmpOfficial.PFTrust_EstablishmentId = EOff.PFTrust_EstablishmentId;

                                        //if (EOff.AccountType_Id != 0)
                                        //    EmpOfficial.AccountType_Id = EOff.AccountType_Id != null ? EOff.AccountType_Id : 0;


                                        //if (EOff.Bank_Id == 0)
                                        //{
                                        //    //EmpOfficial.Bank_Id = null; 
                                        //    EmpOfficial.Branch_Id = EOff.Branch_Id;
                                        //}
                                        //EmpOfficial.Branch_Id = EOff.Branch_Id;
                                        //if (EOff.Branch_Id == 0)
                                        //{
                                        //    //EmpOfficial.Branch_Id = null;
                                        //   EmpOfficial.PayProcessGroup_Id = EOff.PayProcessGroup_Id;
                                        //}
                                        //EmpOfficial.PayProcessGroup_Id = EOff.PayProcessGroup_Id;
                                        //if (EOff.PayProcessGroup_Id == 0)
                                        //{
                                        //    //EmpOfficial.PayProcessGroup_Id = null;
                                        //    EmpOfficial.PayScale_Id = EOff.PayScale_Id;
                                        //}
                                        //EmpOfficial.PayScale_Id = EOff.PayScale_Id;
                                        //if (EOff.PayScale_Id == 0)
                                        //{
                                        //    //EmpOfficial.PayScale_Id = null;
                                        //    EmpOfficial.PayMode_Id = EOff.PayMode_Id;
                                        //}
                                        //EmpOfficial.PayMode_Id = EOff.PayMode_Id;
                                        //if (EOff.PayMode_Id == 0)
                                        //{
                                        //    EmpOfficial.PayMode_Id = EOff.PayMode_Id;
                                        //    //EmpOfficial.PayMode_Id = null;
                                        //}

                                        db.Entry(EmpOfficial).State = System.Data.Entity.EntityState.Modified;
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, EOff.DBTrack);
                                        DT_EmpOff DT_EmpOff = (DT_EmpOff)obj;
                                        DT_EmpOff.AccountType_Id = blog.AccountType == null ? 0 : blog.AccountType.Id;
                                        DT_EmpOff.Bank_Id = blog.Bank == null ? 0 : blog.Bank.Id;
                                        DT_EmpOff.Branch_Id = blog.Branch == null ? 0 : blog.Branch.Id;
                                        DT_EmpOff.NationalityID = blog.NationalityID == null ? 0 : blog.NationalityID.Id;
                                        DT_EmpOff.PayMode_Id = blog.PayMode == null ? 0 : blog.PayMode.Id;
                                        DT_EmpOff.PayProcessGroup_Id = blog.PayProcessGroup == null ? 0 : blog.PayProcessGroup.Id;
                                        DT_EmpOff.PayScale_Id = blog.PayScale == null ? 0 : blog.PayScale.Id;
                                        db.Create(DT_EmpOff);
                                        db.SaveChanges();
                                    }


                                    //                            //vpf amount/percentage update in structure start
                                    EmployeePayroll emppayroll = null;
                                    if (EOff.VPFAppl == true)
                                    {
                                        emppayroll = db.EmployeePayroll.Where(e => e.Employee.EmpOffInfo.Id == data)
                                           .SingleOrDefault();
                                        DateTime paydate = DateTime.Now.Date;
                                        var OEmpSalStruct = db.EmpSalStruct.Where(e => e.EndDate == null && e.EmployeePayroll.Id == emppayroll.Id).SingleOrDefault();
                                        if (OEmpSalStruct != null)
                                        {

                                            string paymonth = OEmpSalStruct.EffectiveDate.Value.ToString("MM/yyyy");
                                            string paymonth1 = OEmpSalStruct.EffectiveDate.Value.ToString("dd/MM/yyyy");
                                            //string paymonth = Convert.ToDateTime(paydate).ToString("MM/yyyy");
                                            if (db.CPIEntryT.AsNoTracking().Where(o => o.PayMonth == paymonth && o.EmployeePayroll_Id == emppayroll.Id).Count() == 0)
                                            {
                                                Msg.Add(" Please Create CPIEntry  For current month. ");
                                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                            }
                                            if (db.SalaryT.AsNoTracking().Where(o => o.PayMonth == paymonth && o.EmployeePayroll_Id == emppayroll.Id && o.ReleaseDate != null).Count() > 0)
                                            {
                                                Msg.Add(" Salary has Release  For current month. You can not change VPF amount/Percentage ");
                                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                            }

                                            if (db.SalaryT.AsNoTracking().Where(o => o.PayMonth == paymonth && o.EmployeePayroll_Id == emppayroll.Id && o.ReleaseDate == null).Count() > 0)
                                            {
                                                Msg.Add(" Kindly delete salary current month and try Again. ");
                                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                            }

                                            SalaryHeadGenProcess.EmployeeSalaryStructUpdateForCPI(emppayroll, Convert.ToDateTime(paymonth1));
                                        }



                                    }
                                    //                            //vpf amount/percentage update in structure end

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
                                PensionAppl = EOff.PensionAppl,
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

                                Old_EmpOff = context.EmpOff.Where(e => e.Id == data).Include(e => e.AccountType).Include(e => e.PayScale)
                                                       .Include(e => e.Bank).Include(e => e.Branch).Include(e => e.NationalityID)
                                                       .Include(e => e.PayMode).Include(e => e.PayProcessGroup).SingleOrDefault();
                                DT_EmpOff DT_EmpOff = (DT_EmpOff)obj;
                                DT_EmpOff.AccountType_Id = DBTrackFile.ValCompare(Old_EmpOff.AccountType, EOff.AccountType);
                                DT_EmpOff.Bank_Id = DBTrackFile.ValCompare(Old_EmpOff.Bank, EOff.Bank);
                                DT_EmpOff.Branch_Id = DBTrackFile.ValCompare(Old_EmpOff.Branch, EOff.Branch);
                                DT_EmpOff.NationalityID = DBTrackFile.ValCompare(Old_EmpOff.NationalityID, EOff.NationalityID);
                                DT_EmpOff.PayMode_Id = DBTrackFile.ValCompare(Old_EmpOff.PayMode, EOff.PayMode);
                                DT_EmpOff.PayProcessGroup_Id = DBTrackFile.ValCompare(Old_EmpOff.PayProcessGroup, EOff.PayProcessGroup);
                                DT_EmpOff.PayScale_Id = DBTrackFile.ValCompare(Old_EmpOff.PayScale, EOff.PayScale);
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
        //public async Task<ActionResult> EditSave(EmpOff c, int data, FormCollection form)
        //{
        //    List<string> Msg = new List<string>();

        //    //string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
        //    //string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
        //    //bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //    //string Values = form["Branch_List"];

        //    //c.Address_Id = Addrs != null && Addrs != "" ? int.Parse(Addrs) : 0;
        //    //c.ContactDetails_Id = ContactDetails != null && ContactDetails != "" ? int.Parse(ContactDetails) : 0;
        //    string PayMode = form["PayMode_drop"] == "0" ? "" : form["PayMode_drop"];
        //    string Bank = form["Bank_drop"] == "0" ? "" : form["Bank_drop"];
        //    string Branch = form["Branch_drop"] == "0" ? "" : form["Branch_drop"];
        //    string AccountType = form["AccountType_drop"] == "0" ? "" : form["AccountType_drop"];
        //    string PayScale = form["PayScalelist"] == "0" ? "" : form["PayScalelist"];
        //    string PayProcessGrp = form["AddressList"] == "0" ? "" : form["PayProcessGrouplist"];
        //    bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //    string Taxappl = form["TaxAppl"] == "0" ? "" : form["TaxAppl"];
        //    Taxappl = Taxappl == "true" ? "Yes" : "No";

        //    c.PayMode_Id = PayMode != null && PayMode != "" ? int.Parse(PayMode) : 0;
        //    c.Bank_Id = Bank != null && Bank != "" && Bank != "-Select-" ? int.Parse(Bank) : 0;
        //    c.Branch_Id = Branch != null && Branch != "" && Branch != "-Select-" ? int.Parse(Branch) : 0;
        //    c.AccountType_Id = AccountType != null && AccountType != "" ? int.Parse(AccountType) : 0;
        //    c.PayProcessGroup_Id = PayProcessGrp != null && PayProcessGrp != "" ? int.Parse(PayProcessGrp) : 0;
        //    c.PayScale_Id = PayScale != null && PayScale != "" ? int.Parse(PayScale) : 0;


        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {
        //                var db_data = db.EmpOff.Include(e => e.PayMode).Include(e =>e.PayProcessGroup).Include(e=>e.AccountType).Include(e =>e.PayScale).Include(e=>e.Branch).Include(e=>e.Bank).Where(e => e.Id == data).SingleOrDefault();
        //                //List<Branch> branchno = new List<Branch>();
        //                //if (Values != "" && Values != null)
        //                //{
        //                //    var ids = Utility.StringIdsToListIds(Values);
        //                //    foreach (var ca in ids)
        //                //    {
        //                //        var Values_val = db.Branch.Find(ca);
        //                //        branchno.Add(Values_val);
        //                //    }
        //                //    db_data.Branches = branchno;
        //                //}
        //                //else
        //                //{
        //                //    db_data.Branches = null;
        //                //}

        //                if (c.Bank_Id == 0)
        //                {
        //                    db_data.Bank = null;
        //                }

        //                if (c.Branch_Id == 0)
        //                {
        //                    db_data.Branch = null;
        //                }
        //                if (c.AccountType_Id == 0)
        //                {
        //                    db_data.AccountType = null;
        //                }
        //                if (c.PayMode_Id == 0)
        //                {
        //                    db_data.PayMode = null;
        //                }
        //                if (c.PayProcessGroup_Id == 0)
        //                {
        //                    db_data.PayProcessGroup = null;
        //                }
        //                if (c.PayScale_Id == 0)
        //                {
        //                    db_data.PayScale = null;
        //                }

        //                db.EmpOff.Attach(db_data);
        //                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //                db.SaveChanges();
        //                TempData["RowVersion"] = db_data.RowVersion;

        //                EmpOff empoff = db.EmpOff.Find(data);
        //                TempData["CurrRowVersion"] = empoff.RowVersion;
        //                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                {
        //                    EmpOff blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;

        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = empoff.DBTrack.CreatedBy == null ? null : empoff.DBTrack.CreatedBy,
        //                        CreatedOn = empoff.DBTrack.CreatedOn == null ? null : empoff.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };

        //                    if (c.PayMode_Id != 0)
        //                        empoff.PayMode_Id = c.PayMode_Id != null ? c.PayMode_Id : 0;
        //                    if (c.PayProcessGroup_Id != 0)
        //                        empoff.PayProcessGroup_Id = c.PayProcessGroup_Id != null ? c.PayProcessGroup_Id : 0;
        //                    if (c.PayScale_Id != 0)
        //                        empoff.PayScale_Id = c.PayScale_Id != null ? c.PayScale_Id : 0;
        //                    if (c.Bank_Id != 0)
        //                        empoff.Bank_Id = c.Bank_Id != null ? c.Bank_Id: 0;
        //                    if (c.AccountType_Id != 0)
        //                        empoff.AccountType_Id = c.AccountType_Id != null ? c.AccountType_Id : 0;
        //                    if (c.Branch_Id != 0)
        //                        empoff.Branch_Id = c.Branch_Id != null ? c.Branch_Id : 0;
        //                    empoff.Id = data;
        //                    //bank.Code = c.Code;
        //                    //bank.Name = c.Name;
        //                    empoff.DBTrack = c.DBTrack;

        //                    db.Entry(empoff).State = System.Data.Entity.EntityState.Modified;


        //                    //using (var context = new DataBaseContext())
        //                    //{


        //                    blog = db.EmpOff.Where(e => e.Id == data).Include(e => e.PayScale)
        //                                            .Include(e => e.PayMode).Include(e=>e.PayProcessGroup).Include(e=>e.Bank).Include(e=>e.Branch).Include(e=>e.AccountType)
        //                                            .SingleOrDefault();
        //                    originalBlogValues = db.Entry(blog).OriginalValues;
        //                    db.ChangeTracker.DetectChanges();
        //                    var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                    DT_EmpOff DT_Corp = (DT_EmpOff)obj;
        //                    DT_Corp.AccountType_Id = blog.AccountType_Id == null ? 0 : blog.AccountType.Id;
        //                    DT_Corp.Bank_Id = blog.Bank_Id == null ? 0 : blog.Bank.Id;
        //                    DT_Corp.Branch_Id= blog.Branch_Id == null ? 0 : blog.Branch.Id;
        //                    DT_Corp.PayMode_Id = blog.PayMode_Id == null ? 0 : blog.PayMode.Id;
        //                    DT_Corp.PayScale_Id= blog.PayScale_Id == null ? 0 : blog.PayScale.Id;
        //                    DT_Corp.PayProcessGroup_Id = blog.PayProcessGroup_Id== null ? 0 : blog.PayProcessGroup.Id;

        //                    db.Create(DT_Corp);
        //                    db.SaveChanges();
        //                }
        //                //}
        //                ts.Complete();
        //            }
        //            Msg.Add("  Record Updated");
        //            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }

        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //}


        public int EditS(string AccType, string PayMode, string bank, string branch, string national, string PayProcGrp, int data, EmpOff EOff, DBTrack dbT)
        {
            EmpOff typedetails = null;
            using (DataBaseContext db = new DataBaseContext())
            {

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
                        PensionAppl = EOff.PensionAppl,
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
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
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
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
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




        public class EmpClass
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }

        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Acc_No { get; set; }
            public string PANCARD { get; set; }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //IP2BSecurity security = new P2B.SERVICES.Factory.P2BSecurity().RegisterSettings();
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

                    var BindEmpList = db.Employee.Include(e => e.EmpOffInfo).Include(e => e.EmpName)
                                                  .Include(e => e.EmpOffInfo.NationalityID)
                                                  .ToList();

                    foreach (var z in BindEmpList)
                    {
                        if (z.EmpOffInfo != null)
                        {
                            //var PANCARDNo = z.EmpOffInfo != null && z.EmpOffInfo.NationalityID != null ? z.EmpOffInfo.NationalityID.PANNo.ToString() : null;
                            //var PANCARDNoPlaintext = security.Masking(PANCARDNo, 4);
                            //var PANCARDNoMask = PANCARDNoPlaintext.First(e => e.Key.Equals("MaskedText")).Value.ToString();
                            view = new P2BGridData()
                            {
                                Id = z.EmpOffInfo.Id,
                                Code = z.EmpCode,
                                Name = z.EmpName != null ? z.EmpName.FullNameFML : null,
                                Acc_No = z.EmpOffInfo.AccountNo != null ? z.EmpOffInfo.AccountNo : null,
                                PANCARD = z.EmpOffInfo.NationalityID.PANNo != null ? z.EmpOffInfo.NationalityID.PANNo : null,
                                //PANCARD = PANCARDNoMask

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
                            jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                              || (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.Acc_No.ToString().Contains(gp.searchString))
                              || (e.PANCARD.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString))
                              ).Select(a => new Object[] { a.Code, a.Name, a.Acc_No, a.PANCARD, a.Id }).ToList();
                            //if (gp.searchField == "PANCARD")
                            //    jsonData = IE.Select(a => new { a.Code, a.Name, a.Acc_No, a.PANCARD, a.Id }).Where((e => (e.PANCARD.ToString().Contains(gp.searchString)))).ToList();
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Acc_No != null ? Convert.ToString(a.Acc_No) : null, a.PANCARD != null ? Convert.ToString(a.PANCARD) : null, a.Id }).ToList();
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
                                             gp.sidx == "Acc_No" ? c.Acc_No.ToString() :
                                             gp.sidx == "PANCARD" ? c.PANCARD.ToString() :
                                             "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.Acc_No != null ? Convert.ToString(a.Acc_No) : null, a.PANCARD != null ? Convert.ToString(a.PANCARD) : null, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.Acc_No != null ? Convert.ToString(a.Acc_No) : null, a.PANCARD != null ? Convert.ToString(a.PANCARD) : null, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Acc_No != null ? Convert.ToString(a.Acc_No) : null, a.PANCARD != null ? Convert.ToString(a.PANCARD) : null, a.Id }).ToList();
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

                    IEnumerable<EmpClass> EmployeeList = null;
                    List<EmpClass> model = new List<EmpClass>();
                    EmpClass view = null;


                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

                    var BindEmpList = db.Employee.Where(e => e.EmpOffInfo == null).Include(e => e.EmpName)
                                                  .ToList();


                    foreach (var z in BindEmpList)
                    {
                        if (z != null)
                        {

                            view = new EmpClass()
                            {
                                Id = z.Id,
                                EmpCode = z.EmpCode.ToString() != null ? z.EmpCode.ToString() : "",
                                EmpName = z.EmpName.FullNameFML.ToString() != null ? z.EmpName.FullNameFML.ToString() : ""

                            };
                            model.Add(view);


                        }

                    }

                    EmployeeList = model;

                    IEnumerable<EmpClass> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = EmployeeList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                             || (e.EmpName.ToUpper().Contains(gp.searchString.ToUpper()))
                             || (e.Id.ToString().Contains(gp.searchString))
                             ).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Id }).ToList();
                            //if (gp.searchField == "Id")
                            //    jsonData = IE.Select(a => new { a.Id, a.EmpCode, a.EmpName }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                            //if (gp.searchField == "EmpCode")
                            //    jsonData = IE.Select(a => new { a.Id, a.EmpCode, a.EmpName }).Where((e => (e.EmpCode.ToString().Contains(gp.searchString)))).ToList();
                            //if (gp.searchField == "EmpName")
                            //    jsonData = IE.Select(a => new { a.Id, a.EmpCode, a.EmpName }).Where((e => (e.EmpName.ToString().Contains(gp.searchString)))).ToList();



                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode != null ? Convert.ToString(a.EmpCode) : "", a.EmpName != null ? Convert.ToString(a.EmpName) : "", a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = EmployeeList;
                        Func<EmpClass, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Code" ? c.EmpCode.ToString() :
                                             gp.sidx == "Name" ? c.EmpName.ToString() :
                                             "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.EmpCode != null ? Convert.ToString(a.EmpCode) : "", a.EmpName != null ? Convert.ToString(a.EmpName) : "", a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.EmpCode != null ? Convert.ToString(a.EmpCode) : "", a.EmpName != null ? Convert.ToString(a.EmpName) : "", a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode != null ? Convert.ToString(a.EmpCode) : "", a.EmpName != null ? Convert.ToString(a.EmpName) : "", a.Id }).ToList();
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
        }
    }
}