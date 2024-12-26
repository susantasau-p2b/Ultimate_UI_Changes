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
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class PFMasterController : Controller
    {
      //  private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/PFMaster/Index.cshtml");
        }
        #region CRUD OPERATION
        #region CREATE

        [HttpPost]
        public ActionResult Create(PFMaster PF, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var day = PF.EffectiveDate;
                    // var Msg = new List<String>();

                    string PFTrustTypeList = form["PFTrustTypeList"] == "0" ? "" : form["PFTrustTypeList"];
                    string EPSWages = form["EPSWagesList"] == "0" ? "" : form["EPSWagesList"];
                    string PFEDLIWages = form["PFEDLIWagesList"] == "0" ? "" : form["PFEDLIWagesList"];
                    string PFWages = form["PFWagesList"] == "0" ? "" : form["PFWagesList"];

                    //
                    var id = int.Parse(Session["CompId"].ToString());
                    var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();

                    if (PF.EPSPerc > 100)
                    {
                        // return Json(new Object[] { "", "", "Please Enter EPSPerc Below 100%", JsonRequestBehavior.AllowGet });
                        Msg.Add(" Please Enter EPSPerc Below 100%..  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    if (PF.EmpPFPerc > 100)
                    {
                        Msg.Add(" Please Enter EmpPFPerc Below 100%..  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //    return Json(new Object[] { "", "", "Please Enter EmpPFPerc Below 100%", JsonRequestBehavior.AllowGet });
                    }

                    if (PFTrustTypeList != null && PFTrustTypeList != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(PFTrustTypeList));
                        PF.PFTrustType = val;
                    }
                    if (PFTrustTypeList == null && PFTrustTypeList == "")
                    {   // return Json(new Object[] { "", "", "Please select PFTrustType.", JsonRequestBehavior.AllowGet });
                        Msg.Add("Please select PFTrustType...  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (PFEDLIWages != null)
                    {
                        if (PFEDLIWages != "")
                        {
                            int WagesPFDLIId = Convert.ToInt32(PFEDLIWages);
                            var val = db.Wages.Where(e => e.Id == WagesPFDLIId).SingleOrDefault();
                            PF.PFEDLIWages = val;
                        }
                    }
                    if (PFWages != null)
                    {
                        if (PFWages != "")
                        {
                            int WagesPFId = Convert.ToInt32(PFWages);
                            var val = db.Wages.Where(e => e.Id == WagesPFId).SingleOrDefault();
                            PF.EPFWages = val;
                        }
                    }
                    if (EPSWages != null)
                    {
                        if (EPSWages != "")
                        {
                            int WagesEPSId = Convert.ToInt32(EPSWages);
                            var val = db.Wages.Where(e => e.Id == WagesEPSId).SingleOrDefault();
                            PF.EPSWages = val;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        if (db.PFMaster.Any(o => o.EffectiveDate == null))
                        {
                            day -= TimeSpan.FromDays(1);
                        }

                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.PFMaster.Any(o => o.EffectiveDate >= PF.EffectiveDate))
                            {
                                // return Json(new Object[] { "", "", "Effective Date Should Be Greater Than Previous Effective Date.", JsonRequestBehavior.AllowGet });
                                Msg.Add("Effective Date Should Be Greater Than Previous Effective Date....  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }
                            PF.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                            PFMaster pfmaster = new PFMaster()
                            {
                                CompPFCeiling = PF.CompPFCeiling,
                                CompPFNo = PF.CompPFNo,
                                CPFPerc = PF.CPFPerc,
                                EDLIAdmin = PF.EDLIAdmin,
                                EDLIAdminMin = PF.EDLIAdminMin,
                                EDLICharges = PF.EDLICharges,
                                EDLIInspMin = PF.EDLIInspMin,
                                EDLISInsp = PF.EDLISInsp,
                                EffectiveDate = PF.EffectiveDate,
                                EmpPFPerc = PF.EmpPFPerc,
                                EPFAdminCharges = PF.EPFAdminCharges,
                                EPFAdminMin = PF.EPFAdminMin,
                                EPFCeiling = PF.EPFCeiling,
                                EPFInspCharges = PF.EPFInspCharges,
                                EPFInspMin = PF.EPFInspMin,
                                EPFWages = PF.EPFWages,
                                EPSCeiling = PF.EPSCeiling,
                                EPSPerc = PF.EPSPerc,
                                EstablishmentID = PF.EstablishmentID,
                                PensionAge = PF.PensionAge,
                                PFEDLIWages = PF.PFEDLIWages,
                                PFTrustType = PF.PFTrustType,
                                RegDate = PF.RegDate,
                                DBTrack = PF.DBTrack,
                                EPSWages = PF.EPSWages
                            };
                            try
                            {
                                db.PFMaster.Add(pfmaster);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, PF.DBTrack);
                                //DT_PFMaster DT_OBJ = (DT_PFMaster)rtn_Obj;
                                //DT_OBJ. = E.WageMasterPay == null ? 0 : E.WageMasterPay.Id;
                                //DT_OBJ.Wage_Id = E.WageMasterQualify == null ? 0 : E.WageMasterQualify.Id;
                                ////DT_OBJ.EffectMonths_Id = E.e == null ? 0 : E.StatutoryEffectiveMonths.;
                                //db.Create(DT_OBJ);
                                db.SaveChanges();
                                if (companypayroll != null)
                                {
                                    List<PFMaster> pfmasterlist = new List<PFMaster>();
                                    pfmasterlist.Add(pfmaster);
                                    companypayroll.PFMaster = pfmasterlist;
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                                }
                                ts.Complete();
                                //   return this.Json(new Object[] { pfmaster.Id, pfmaster.CompPFNo, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Created successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = pfmaster.Id, Val = pfmaster.CompPFNo, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = PF.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                // return Json(new Utility.JsonReturnClass { success = false, responseText = "Unable to create. Try again, and if the problem persists contact your system administrator.." }, JsonRequestBehavior.AllowGet);
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
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
                        var errorMsg = sb.ToString();
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        #region EDIT AND EDITSAVE
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.PFMaster
                  .Include(e => e.PFAdminWages)
                  .Include(e => e.EPSWages)
                  .Include(e => e.PFEDLIWages)
                  .Include(e => e.PFInspWages)
                  .Include(e => e.PFTrustType)
                  .Include(e => e.EPFWages)
                  .Include(e => e.PFTrustType)
                  .Where(e => e.Id == data).Select
                  (e => new
                  {
                      CompPFCeiling = e.CompPFCeiling,
                      CompPFNo = e.CompPFNo,
                      CPFPerc = e.CPFPerc,
                      EDLIAdmin = e.EDLIAdmin,
                      EDLIAdminMin = e.EDLIAdminMin,
                      EDLICharges = e.EDLICharges,
                      EDLIInspMin = e.EDLIInspMin,
                      EDLISInsp = e.EDLISInsp,
                      EffectiveDate = e.EffectiveDate,
                      EmpPFPerc = e.EmpPFPerc,
                      EPFAdminCharges = e.EPFAdminCharges,
                      EPFAdminMin = e.EPFAdminMin,
                      EPFCeiling = e.EPFCeiling,
                      EPFInspCharges = e.EPFInspCharges,
                      EPFInspMin = e.EPFInspMin,
                      EPFWages = e.EPFWages,
                      EPSCeiling = e.EPSCeiling,
                      EPSPerc = e.EPSPerc,
                      EstablishmentID = e.EstablishmentID,
                      PensionAge = e.PensionAge,
                      PFEDLIWages = e.PFEDLIWages,
                      PFTrustType_Id = e.PFTrustType != null ? e.PFTrustType.Id : 0,
                      RegDate = e.RegDate,
                      Action = e.DBTrack.Action
                  }).ToList();

                var add_data = db.PFMaster
                .Include(e => e.PFTrustType)
                  .Include(e => e.EPSWages)
                  .Include(e => e.PFEDLIWages)
                  .Include(e => e.EPFWages)
                  .Where(e => e.Id == data)
                  .Select(e => new
                  {
                      EPSWages_Id = e.EPSWages == null ? 0 : e.EPSWages.Id,
                      EPSWages_FullDetails = e.EPSWages.FullDetails == null ? "" : e.EPSWages.FullDetails,
                      PFEDLIWages_Id = e.PFEDLIWages == null ? 0 : e.PFEDLIWages.Id,
                      PFEDLIWages_FullDetails = e.PFEDLIWages.FullDetails == null ? "" : e.PFEDLIWages.FullDetails,
                      PFWages_Id = e.EPFWages == null ? 0 : e.EPFWages.Id,
                      PFWages_FullDetails = e.EPFWages.FullDetails == null ? "" : e.EPFWages.FullDetails,
                  }).ToList();

                var W = db.DT_PFMaster
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         //Wages_PayVal = e. == 0 ? "" : db.Wages.Where(x => x.Id == e.Wage_Id).Select(x => x.WagesName).FirstOrDefault(),
                         //Wages_QualifyVal = e.Wage_Id == 0 ? "" : db.Wages.Where(x => x.Id == e.Wage_Id).Select(x => x.WagesName).FirstOrDefault(),
                         //Range_Val = e.Range_Id == 0 ? "" : db.Range.Where(x => x.Id == e.Range_Id).Select(x => x.FullDetails).FirstOrDefault(),
                         //Range_Val = e.Range_Id == null ? "" : db.Range.Where(x => x.Id == e.Range_Id).Select(x => x.Id).FirstOrDefault(),
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.PFMaster.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }



        [HttpPost]
        //public async Task<ActionResult> EditSave(PFMaster PF, FormCollection form, int data)
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            //  var Msg = new List<String>();
        //            string PFTrustTypeList = form["PFTrustTypeList"] == "0" ? "" : form["PFTrustTypeList"];
        //            string EPSWages = form["EPSWagesList"] == "0" ? "" : form["EPSWagesList"];
        //            string PFEDLIWages = form["PFEDLIWagesList"] == "0" ? "" : form["PFEDLIWagesList"];
        //            string PFWages = form["PFWagesList"] == "0" ? "" : form["PFWagesList"];
        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;



        //            if (PFTrustTypeList != null)
        //            {
        //                if (PFTrustTypeList != "")
        //                {
        //                    var val = db.LookupValue.Find(int.Parse(PFTrustTypeList));
        //                    PF.PFTrustType = val;
        //                }
        //            }

        //            if (PFEDLIWages != null)
        //            {
        //                if (PFEDLIWages != "")
        //                {
        //                    int WagesPFDLIId = Convert.ToInt32(PFEDLIWages);
        //                    var val = db.Wages.Where(e => e.Id == WagesPFDLIId).SingleOrDefault();
        //                    PF.PFEDLIWages = val;
        //                }
        //            }
        //            if (PFWages != null)
        //            {
        //                if (PFWages != "")
        //                {
        //                    int WagesPFId = Convert.ToInt32(PFWages);
        //                    var val = db.Wages.Where(e => e.Id == WagesPFId).SingleOrDefault();
        //                    PF.EPFWages = val;
        //                }
        //            }
        //            if (EPSWages != null)
        //            {
        //                if (EPSWages != "")
        //                {
        //                    int WagesEPSId = Convert.ToInt32(EPSWages);
        //                    var val = db.Wages.Where(e => e.Id == WagesEPSId).SingleOrDefault();
        //                    PF.EPSWages = val;
        //                }
        //            }


        //            if (Auth == false)
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {
        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            PFMaster blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.PFMaster.Where(e => e.Id == data)
        //                                                        .Include(e => e.PFEDLIWages)
        //                                                        .Include(e => e.PFTrustType)
        //                                                        .Include(e => e.EPFWages)
        //                                                        .Include(e => e.EPSWages)
        //                                                        .SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            PF.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            PFMaster typedetails = null;

        //                            if (PFTrustTypeList != null & PFTrustTypeList != "")
        //                            {
        //                                var val = db.LookupValue.Find(int.Parse(PFTrustTypeList));
        //                                PF.PFTrustType = val;

        //                                var type = db.PFMaster.Include(e => e.PFTrustType).Where(e => e.Id == data).SingleOrDefault();

        //                                if (type.PFTrustType != null)
        //                                {
        //                                    typedetails = db.PFMaster.Where(x => x.PFTrustType.Id == type.PFTrustType.Id && x.Id == data).SingleOrDefault();
        //                                }
        //                                else
        //                                {
        //                                    typedetails = db.PFMaster.Where(x => x.Id == data).SingleOrDefault();
        //                                }
        //                                typedetails.PFTrustType = PF.PFTrustType;
        //                            }
        //                            else
        //                            {
        //                                typedetails = db.PFMaster.Include(e => e.PFTrustType).Where(x => x.Id == data).SingleOrDefault();
        //                                typedetails.PFTrustType = null;
        //                            }
        //                            if (EPSWages != null & EPSWages != "")
        //                            {
        //                                var val = db.Wages.Find(int.Parse(EPSWages));
        //                                PF.EPSWages = val;

        //                                var type = db.PFMaster.Include(e => e.EPSWages).Where(e => e.Id == data).SingleOrDefault();

        //                                if (type.EPSWages != null)
        //                                {
        //                                    typedetails = db.PFMaster.Where(x => x.EPSWages.Id == type.EPSWages.Id && x.Id == data).SingleOrDefault();
        //                                }
        //                                else
        //                                {
        //                                    typedetails = db.PFMaster.Where(x => x.Id == data).SingleOrDefault();
        //                                }
        //                                typedetails.EPSWages = PF.EPSWages;
        //                            }
        //                            else
        //                            {
        //                                typedetails = db.PFMaster.Include(e => e.EPSWages).Where(x => x.Id == data).SingleOrDefault();
        //                                typedetails.EPSWages = null;
        //                            }

        //                            if (PFEDLIWages != null & PFEDLIWages != "")
        //                            {
        //                                var val = db.Wages.Find(int.Parse(PFEDLIWages));
        //                                PF.PFEDLIWages = val;

        //                                var type = db.PFMaster.Include(e => e.PFEDLIWages).Where(e => e.Id == data).SingleOrDefault();

        //                                if (type.PFEDLIWages != null)
        //                                {
        //                                    typedetails = db.PFMaster.Where(x => x.PFEDLIWages.Id == type.PFEDLIWages.Id && x.Id == data).SingleOrDefault();
        //                                }
        //                                else
        //                                {
        //                                    typedetails = db.PFMaster.Where(x => x.Id == data).SingleOrDefault();
        //                                }
        //                                typedetails.PFEDLIWages = PF.PFEDLIWages;
        //                            }
        //                            else
        //                            {
        //                                typedetails = db.PFMaster.Include(e => e.PFEDLIWages).Where(x => x.Id == data).SingleOrDefault();
        //                                typedetails.PFEDLIWages = null;
        //                            }

        //                            if (PFWages != null & PFWages != "")
        //                            {
        //                                var val = db.Wages.Find(int.Parse(PFWages));
        //                                PF.EPFWages = val;

        //                                var type = db.PFMaster.Include(e => e.EPFWages).Where(e => e.Id == data).SingleOrDefault();

        //                                if (type.EPFWages != null)
        //                                {
        //                                    typedetails = db.PFMaster.Where(x => x.EPFWages.Id == type.EPFWages.Id && x.Id == data).SingleOrDefault();
        //                                }
        //                                else
        //                                {
        //                                    typedetails = db.PFMaster.Where(x => x.Id == data).SingleOrDefault();
        //                                }
        //                                typedetails.EPFWages = PF.EPFWages;
        //                            }
        //                            else
        //                            {
        //                                typedetails = db.PFMaster.Include(e => e.EPFWages).Where(x => x.Id == data).SingleOrDefault();
        //                                typedetails.EPFWages = null;
        //                            }


        //                            db.PFMaster.Attach(typedetails);
        //                            db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
        //                            db.SaveChanges();
        //                            TempData["RowVersion"] = typedetails.RowVersion;
        //                            db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;


        //                            var Curr_OBJ = db.PFMaster.Find(data);
        //                            TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
        //                            db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;
        //                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                            {

        //                                PFMaster PFMaster = new PFMaster()
        //                                {
        //                                    CompPFCeiling = PF.CompPFCeiling,
        //                                    CompPFNo = PF.CompPFNo,
        //                                    CPFPerc = PF.CPFPerc,
        //                                    EDLIAdmin = PF.EDLIAdmin,
        //                                    EDLIAdminMin = PF.EDLIAdminMin,
        //                                    EDLICharges = PF.EDLICharges,
        //                                    EDLIInspMin = PF.EDLIInspMin,
        //                                    EDLISInsp = PF.EDLISInsp,
        //                                    EffectiveDate = PF.EffectiveDate,
        //                                    EmpPFPerc = PF.EmpPFPerc,
        //                                    EPFAdminCharges = PF.EPFAdminCharges,
        //                                    EPFAdminMin = PF.EPFAdminMin,
        //                                    EPFCeiling = PF.EPFCeiling,
        //                                    EPFInspCharges = PF.EPFInspCharges,
        //                                    EPFInspMin = PF.EPFInspMin,
        //                                    EPFWages = PF.EPFWages,
        //                                    EPSCeiling = PF.EPSCeiling,
        //                                    EPSPerc = PF.EPSPerc,
        //                                    EstablishmentID = PF.EstablishmentID,
        //                                    PensionAge = PF.PensionAge,
        //                                    PFEDLIWages = PF.PFEDLIWages,
        //                                    PFTrustType = PF.PFTrustType,
        //                                    RegDate = PF.RegDate,
        //                                    Id = data,
        //                                    DBTrack = PF.DBTrack
        //                                };
        //                                db.PFMaster.Attach(PFMaster);
        //                                db.Entry(PFMaster).State = System.Data.Entity.EntityState.Modified;
        //                                db.Entry(PFMaster).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                            }

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, PF.DBTrack);
        //                                //DT_ESIC DT_OBJ = (DT_ESIC)obj;
        //                                //DT_OBJ.Wage_Id = blog.WageMasterPay == null ? 0 : blog.WageMasterPay.Id;
        //                                //DT_OBJ.Wage_Id = blog.WageMasterQualify == null ? 0 : blog.WageMasterQualify.Id;
        //                                //db.Create(DT_OBJ);
        //                                db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();


        //                            //  return Json(new Object[] { PF.Id, PF.CompPFNo, "Record Updated", JsonRequestBehavior.AllowGet });
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = PF.Id, Val = PF.CompPFNo, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                    }

        //                    //catch (DbUpdateException e) { throw e; }
        //                    //catch (DataException e) { throw e; }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (ESICMaster)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            Msg.Add("Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (ESICMaster)databaseEntry.ToObject();
        //                            PF.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    StringBuilder sb = new StringBuilder("");
        //                    foreach (ModelState modelState in ModelState.Values)
        //                    {
        //                        foreach (ModelError error in modelState.Errors)
        //                        {
        //                            sb.Append(error.ErrorMessage);
        //                            sb.Append("." + "\n");
        //                        }
        //                    }
        //                    var errorMsg = sb.ToString();
        //                    Msg.Add(errorMsg);
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
        //                }
        //            }


        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    PFMaster blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    PFMaster Old_Obj = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.PFMaster.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    PF.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };
        //                    PFMaster pfmaster = new PFMaster()
        //                    {
        //                        Id = data,
        //                        CompPFCeiling = PF.CompPFCeiling,
        //                        CompPFNo = PF.CompPFNo,
        //                        CPFPerc = PF.CPFPerc,
        //                        EDLIAdmin = PF.EDLIAdmin,
        //                        EDLIAdminMin = PF.EDLIAdminMin,
        //                        EDLICharges = PF.EDLICharges,
        //                        EDLIInspMin = PF.EDLIInspMin,
        //                        EDLISInsp = PF.EDLISInsp,
        //                        EffectiveDate = PF.EffectiveDate,
        //                        EmpPFPerc = PF.EmpPFPerc,
        //                        EPFAdminCharges = PF.EPFAdminCharges,
        //                        EPFAdminMin = PF.EPFAdminMin,
        //                        EPFCeiling = PF.EPFCeiling,
        //                        EPFInspCharges = PF.EPFInspCharges,
        //                        EPFInspMin = PF.EPFInspMin,
        //                        EPFWages = PF.EPFWages,
        //                        EPSCeiling = PF.EPSCeiling,
        //                        EPSPerc = PF.EPSPerc,
        //                        EstablishmentID = PF.EstablishmentID,
        //                        PensionAge = PF.PensionAge,
        //                        PFEDLIWages = PF.PFEDLIWages,
        //                        PFTrustType = PF.PFTrustType,
        //                        RegDate = PF.RegDate,
        //                        DBTrack = PF.DBTrack,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, pfmaster, "PFMaster", PF.DBTrack);
        //                        // var obj = DBTrackFile.DBTrackSave("CPayroll/Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        Old_Obj = context.PFMaster.Where(e => e.Id == data).Include(e => e.EPSWages)
        //                            .Include(e => e.PFAdminWages).Include(e => e.PFEDLIWages).Include(e => e.PFInspWages).Include(e => e.EPFWages)
        //                            .Include(e => e.PFTrustType)
        //                            .SingleOrDefault();
        //                        //DT_PFMaster DT_OBJ = (DT_PFMaster)obj;
        //                        //DT_O = DBTrackFile.ValCompare(Old_Obj.WageMasterPay, E.WageMasterPay);//Old_Obj.Address == c.Address ? 0 : Old_Obj.Address == null && c.Address != null ? c.Address.Id : Old_Obj.Address.Id;
        //                        //DT_OBJ.Wage_Id = DBTrackFile.ValCompare(Old_Obj.WageMasterQualify, E.WageMasterQualify);

        //                        //db.Create(DT_OBJ);
        //                        //db.SaveChanges();
        //                    }
        //                    blog.DBTrack = PF.DBTrack;
        //                    db.PFMaster.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    return Json(new Object[] { blog.Id, PF.CompPFNo, "Record Updated", JsonRequestBehavior.AllowGet });
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = PF.CompPFNo, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }

        //            }
        //            return View();
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
        //}
        public async Task<ActionResult> EditSave(PFMaster PF ,FormCollection form, int data)
        {
            List<string> Msg = new List<string>();

            string PFTrustTypeList = form["PFTrustTypeList"] == "0" ? "" : form["PFTrustTypeList"];
            string EPSWages = form["EPSWagesList"] == "0" ? "" : form["EPSWagesList"];
            string PFEDLIWages = form["PFEDLIWagesList"] == "0" ? "" : form["PFEDLIWagesList"];
            string PFWages = form["PFWagesList"] == "0" ? "" : form["PFWagesList"];
            bool Auth = form["Autho_Allow"] == "true" ? true : false;

            PF.PFTrustType_Id = PFTrustTypeList != null && PFTrustTypeList != "" ? int.Parse(PFTrustTypeList) : 0;
            PF.EPSWages_Id = EPSWages != null && EPSWages != "" ? int.Parse(EPSWages) : 0;
            PF.PFEDLIWages_Id = PFEDLIWages != null && PFEDLIWages != "" ? int.Parse(PFEDLIWages) : 0;
            PF.EPFWages_Id = PFWages != null && PFWages != "" ? int.Parse(PFWages) : 0;

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.PFMaster.Include(e => e.PFTrustType).Include(e => e.EPSWages).Include(e =>e.PFEDLIWages).Include(e=>e.EPFWages).Where(e => e.Id == data).SingleOrDefault();
                        
                        if (PF.PFTrustType_Id== 0)
                        {
                            db_data.PFTrustType = null;
                        }

                        if (PF.EPSWages_Id == 0)
                        {
                            db_data.EPSWages = null;
                        }
                        if (PF.PFEDLIWages_Id == 0)
                        {
                            db_data.PFEDLIWages = null;
                        }
                        if (PF.EPFWages_Id == 0)
                        {
                            db_data.EPFWages = null;
                        }

                        db.PFMaster.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;

                        PFMaster MFPrets = db.PFMaster.Find(data);
                        TempData["CurrRowVersion"] = MFPrets.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            PFMaster blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            PF.DBTrack = new DBTrack
                            {
                                CreatedBy = MFPrets.DBTrack.CreatedBy == null ? null : MFPrets.DBTrack.CreatedBy,
                                CreatedOn = MFPrets.DBTrack.CreatedOn == null ? null : MFPrets.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (PF.PFTrustType_Id != 0)
                                MFPrets.PFTrustType_Id = PF.PFTrustType_Id != null ? PF.PFTrustType_Id : 0;

                            if (PF.EPSWages_Id!= 0)
                                MFPrets.EPSWages_Id = PF.EPSWages_Id != null ? PF.EPSWages_Id : 0;

                            if (PF.PFEDLIWages_Id != 0)
                                MFPrets.PFEDLIWages_Id = PF.PFEDLIWages_Id != null ? PF.PFEDLIWages_Id : 0;

                            if (PF.EPFWages_Id != 0)
                                MFPrets.EPFWages_Id = PF.EPFWages_Id != null ? PF.EPFWages_Id : 0;

                            MFPrets.Id = data;
                            MFPrets.CompPFCeiling = PF.CompPFCeiling;
                            MFPrets.CompPFNo = PF.CompPFNo;
                            MFPrets.CPFPerc = PF.CPFPerc;
                            MFPrets.EDLIAdmin = PF.EDLIAdmin;
                            MFPrets.EDLIAdminMin = PF.EDLIAdminMin;
                            MFPrets.EDLICharges = PF.EDLICharges;
                            MFPrets.EDLIInspMin = PF.EDLIInspMin;
                            MFPrets.EDLISInsp = PF.EDLISInsp;
                            MFPrets.EffectiveDate = PF.EffectiveDate;
                            MFPrets.EmpPFPerc = PF.EmpPFPerc;
                            MFPrets.EPFAdminCharges = PF.EPFAdminCharges;
                            MFPrets.EPFAdminMin = PF.EPFAdminMin;
                            MFPrets.EPFCeiling = PF.EPFCeiling;
                            MFPrets.EPFInspCharges = PF.EPFInspCharges;
                            MFPrets.EPFInspMin = PF.EPFInspMin;
                           
                            MFPrets.EPSCeiling = PF.EPSCeiling;
                            MFPrets.EPSPerc = PF.EPSPerc;
                            MFPrets.EstablishmentID = PF.EstablishmentID;
                            MFPrets.PensionAge = PF.PensionAge;
                            
                           
                            MFPrets.RegDate = PF.RegDate;


                            db.Entry(MFPrets).State = System.Data.Entity.EntityState.Modified;


                            //using (var context = new DataBaseContext())
                            //{


                            blog = db.PFMaster.Where(e => e.Id == data).Include(e => e.PFTrustType)
                                                    .Include(e => e.EPFWages).Include(e=>e.EPSWages)
                                                    .Include(e=>e.PFEDLIWages)
                                                    .SingleOrDefault();
                            originalBlogValues = db.Entry(blog).OriginalValues;
                            db.ChangeTracker.DetectChanges();
                            var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, PF.DBTrack);
                            DT_PFMaster DT_Corp = (DT_PFMaster)obj;

                            DT_Corp.PFTrustType_Id = blog.PFTrustType == null ? 0 : blog.PFTrustType.Id;
                            DT_Corp.EPSWages_Id = blog.EPSWages == null ? 0 : blog.EPSWages.Id;
                            DT_Corp.EPFWages_Id = blog.EPFWages == null ? 0 : blog.EPFWages.Id;
                            DT_Corp.PFEDLIWages_Id = blog.PFEDLIWages == null ? 0 : blog.PFEDLIWages.Id;

                            
                            db.Create(DT_Corp);
                            db.SaveChanges();
                        }
                        //}
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = PF.Id, Val = PF.CompPFNo, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        #endregion

        #region DELETE

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                PFMaster PFMaster = db.PFMaster.Include(e => e.PFTrustType)
                                                   .Include(e => e.EPSWages)
                                                   .Include(e => e.PFEDLIWages)
                                                   .Include(e => e.EPFWages)
                                                   .Where(e => e.Id == data).SingleOrDefault();
                var id = int.Parse(Session["CompId"].ToString());
                var companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
                companypayroll.PFMaster.Where(e => e.Id == PFMaster.Id);
                companypayroll.PFMaster = null;
                db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                //if(companypayroll.PFMaster.Select(e=>e.Id)==PFMaster.Id)
                //companypayroll.PFMaster.Where(e => e.Id == PFMaster.Id);

                // db.Entry(s).State = System.Data.Entity.EntityState.Detached;


                if (PFMaster.DBTrack.IsModified == true)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            CreatedBy = PFMaster.DBTrack.CreatedBy != null ? PFMaster.DBTrack.CreatedBy : null,
                            CreatedOn = PFMaster.DBTrack.CreatedOn != null ? PFMaster.DBTrack.CreatedOn : null,
                            IsModified = PFMaster.DBTrack.IsModified == true ? true : false
                        };
                        PFMaster.DBTrack = dbT;
                        db.Entry(PFMaster).State = System.Data.Entity.EntityState.Modified;
                        var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, PFMaster.DBTrack);
                        //DT_PFMaster DT_Corp = (DT_PFMaster)rtn_Obj;
                        //DT_Corp. = corporates.Address == null ? 0 : corporates.Address.Id;
                        //DT_Corp.BusinessType_Id = corporates.BusinessType == null ? 0 : corporates.BusinessType.Id;
                        //DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                        //db.Create(DT_Corp);
                        // db.SaveChanges();
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                        await db.SaveChangesAsync();
                        //using (var context = new DataBaseContext())
                        //{
                        //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                        //}
                        ts.Complete();
                        //    return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                }
                else
                {
                    //  return Json(new Object[] { "", "Cannot Delete  Data.", JsonRequestBehavior.AllowGet });
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //{

                    //    try
                    //    {
                    //        DBTrack dbT = new DBTrack
                    //        {
                    //            Action =r "D",
                    //            ModifiedBy = SessionManager.UserName,
                    //            ModifiedOn = DateTime.Now,
                    //            CreatedBy = PFMaster.DBTrack.CreatedBy != null ? PFMaster.DBTrack.CreatedBy : null,
                    //            CreatedOn = PFMaster.DBTrack.CreatedOn != null ? PFMaster.DBTrack.CreatedOn : null,
                    //            IsModified = PFMaster.DBTrack.IsModified == true ? false : false//,
                    //            //AuthorizedBy = SessionManager.UserName,
                    //            //AuthorizedOn = DateTime.Now
                    //        };

                    //        // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                    //        db.Entry(PFMaster).State = System.Data.Entity.EntityState.Deleted;
                    //        var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
                    //        //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                    //        //DT_Corp.Address_Id = add == null ? 0 : add.Id;
                    //        //DT_Corp.BusinessType_Id = val == null ? 0 : val.Id;
                    //        //DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                    //        //db.Create(DT_Corp);

                    //        await db.SaveChangesAsync();


                    //        //using (var context = new DataBaseContext())
                    //        //{
                    //        //    corporates.Address = add;
                    //        //    corporates.ContactDetails = conDet;
                    //        //    corporates.BusinessType = val;
                    //        //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                    //        //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
                    //        //}
                    //        ts.Complete();
                    //        return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });


                    //catch (RetryLimitExceededException /* dex */)
                    //{
                    //    //Log the error (uncomment dex variable name and add a line here to write a log.)
                    //    //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                    //    //return RedirectToAction("Delete");
                    //    return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                    //}



                }
            }
        }
        #endregion

        #region AUTHSAVE
        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    //  var Msg = new List<String>(); 
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);
                            PFMaster PFMaster = db.PFMaster.Include(e => e.PFTrustType)
                                                       .Include(e => e.EPSWages)
                                                       .Include(e => e.PFAdminWages)
                                                       .Include(e => e.PFEDLIWages)
                                                       .Include(e => e.PFInspWages)
                                                       .Include(e => e.EPFWages)
                                                       .FirstOrDefault(e => e.Id == auth_id);

                            PFMaster.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = PFMaster.DBTrack.ModifiedBy != null ? PFMaster.DBTrack.ModifiedBy : null,
                                CreatedBy = PFMaster.DBTrack.CreatedBy != null ? PFMaster.DBTrack.CreatedBy : null,
                                CreatedOn = PFMaster.DBTrack.CreatedOn != null ? PFMaster.DBTrack.CreatedOn : null,
                                IsModified = PFMaster.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.PFMaster.Attach(PFMaster);
                            db.Entry(PFMaster).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(PFMaster).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, PFMaster.DBTrack);
                            //DT_PFMaster DT_Corp = (DT_PFMaster)rtn_Obj;
                            //DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            //DT_Corp.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            //db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            //return Json(new Object[] { PFMaster.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = PFMaster.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else if (auth_action == "M")
                    {

                        PFMaster Old_Corp = db.PFMaster.Include(e => e.PFTrustType)
                                                       .Include(e => e.EPSWages)
                                                       .Include(e => e.PFAdminWages)
                                                       .Include(e => e.PFEDLIWages)
                                                       .Include(e => e.PFInspWages)
                                                       .Include(e => e.EPFWages)
                                                       .Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_PFMaster Curr_Corp = db.DT_PFMaster
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            PFMaster pfmaster = new PFMaster();


                            //pfmaster.AdminCharges = Curr_Corp.AdminCharges == null ? Old_Corp. : Curr_Corp.AdminCharges;
                            pfmaster.CompPFCeiling = Curr_Corp.CompPFCeiling == null ? Old_Corp.CompPFCeiling : Curr_Corp.CompPFCeiling;
                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        pfmaster.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                            CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        //int a = EditS(Corp, Addrs, ContactDetails, auth_id, corp, corp.DBTrack);                             

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        //   return Json(new Object[] { pfmaster.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = pfmaster.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Corporate)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add("Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        var databaseValues = (Corporate)databaseEntry.ToObject();
                                        pfmaster.RowVersion = databaseValues.RowVersion;
                                    }
                                }

                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                            Msg.Add("Data removed from history");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            PFMaster pfmaster = db.PFMaster.AsNoTracking()
                                                .Include(e => e.PFTrustType)
                                                .Include(e => e.EPSWages)
                                                .Include(e => e.PFAdminWages)
                                                .Include(e => e.PFEDLIWages)
                                                .Include(e => e.PFInspWages)
                                                .Include(e => e.EPFWages)
                                                .FirstOrDefault(e => e.Id == auth_id);

                            pfmaster.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = pfmaster.DBTrack.ModifiedBy != null ? pfmaster.DBTrack.ModifiedBy : null,
                                CreatedBy = pfmaster.DBTrack.CreatedBy != null ? pfmaster.DBTrack.CreatedBy : null,
                                CreatedOn = pfmaster.DBTrack.CreatedOn != null ? pfmaster.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.PFMaster.Attach(pfmaster);
                            db.Entry(pfmaster).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, pfmaster.DBTrack);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            //DT_Corp.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            //db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(pfmaster).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //  return Json(new Utility.JsonReturnClass { success = true, responseText = "Record Authorised" }, JsonRequestBehavior.AllowGet);
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }

                    }
                    return View();

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

        #endregion

        #region P2BGRIDVIEW
        public class GridClass
        {
            public string EffectiveDate { get; set; }
            public string EstablishmentID { get; set; }
            public int id { get; set; }

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
                IEnumerable<GridClass> PFMasterList = null;

                var compid = int.Parse(Session["CompId"].ToString());
                var data = db.CompanyPayroll.Include(e => e.PFMaster).Where(e => e.Company.Id == compid).SingleOrDefault();
                List<GridClass> model = new List<GridClass>();
                if (data.PFMaster != null && data.PFMaster.Count > 0)
                {
                    foreach (var item in data.PFMaster)
                    {
                        model.Add(new GridClass
                        {
                            id = item.Id,
                            EstablishmentID = item.EstablishmentID != null ? item.EstablishmentID.ToString() : null,
                            EffectiveDate = item.EffectiveDate != null ? item.EffectiveDate.Value.ToShortDateString() : null,
                        });
                    }
                }
                PFMasterList = model;
                //if (gp.IsAutho == true)
                //{
                //    PFMaster = db.CompanyPayroll.Include(e => e.PFMaster.Select(a => a.PFTrustType)).AsNoTracking().ToList();
                //}
                //else
                //{
                // PFMaster = db.CompanyPayroll.Include(e=>e.PFMaster.Select(a=>a.PFTrustType)).AsNoTracking().ToList();
                //}

                IEnumerable<GridClass> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = PFMasterList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EffectiveDate.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.EstablishmentID.ToString().Contains(gp.searchString))
                            || (e.id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.EffectiveDate, a.EstablishmentID, a.id }).ToList();
                                               
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EffectiveDate, a.EstablishmentID, a.id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = PFMasterList;
                    Func<GridClass, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EffectiveDate" ? c.EffectiveDate.ToString() :
                                         gp.sidx == "EstablishmentID" ? c.EstablishmentID.ToString() :
                                          "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EffectiveDate), Convert.ToString(a.EstablishmentID), a.id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EffectiveDate), Convert.ToString(a.EstablishmentID), a.id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EffectiveDate, a.EstablishmentID, a.id }).ToList();
                    }
                    totalRecords = PFMasterList.Count();
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

        #endregion
    }
}