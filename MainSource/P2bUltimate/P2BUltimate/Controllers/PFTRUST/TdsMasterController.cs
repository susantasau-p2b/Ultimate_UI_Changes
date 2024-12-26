using P2b.Global;
using P2B.API;
using P2B.PFTRUST;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers.PFTrust
{
    public class TdsMasterController : Controller
    {
        // GET: InterestRate
        public ActionResult Index()
        {
            return View();
        }



        public ActionResult CreateSave(PFTTDSMaster c, FormCollection form)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                string TDSRate = form["TDSRate"] == "0" ? "0" : form["TDSRate"];
                c.TDSRate = Convert.ToDouble(TDSRate);
                string TaxableAccountCelling = form["TaxableAccountCelling"] == "0" ? "0" : form["TaxableAccountCelling"];
                c.TaxableAccountCelling = Convert.ToDouble(TaxableAccountCelling);
                string IntOnIntTDSAppl = form["IntOnIntTDSAppl"] == "0" ? "0" : form["IntOnIntTDSAppl"];
                string OwnerTDSAppl = form["OwnerTDSAppl"] == "0" ? "0" : form["OwnerTDSAppl"];
                string OwnTDSAppl = form["OwnTDSAppl"] == "0" ? "0" : form["OwnTDSAppl"];
                string VPFTDSAppl = form["VPFTDSAppl"] == "0" ? "0" : form["VPFTDSAppl"];
                c.IsIntOnIntTDSAppl = Convert.ToBoolean(IntOnIntTDSAppl);
                c.IsOwnerTDSAppl = Convert.ToBoolean(OwnerTDSAppl);
                c.IsOwnTDSAppl = Convert.ToBoolean(OwnTDSAppl);
                c.IsVPFTDSAppl = Convert.ToBoolean(VPFTDSAppl);



                List<String> Msg = new List<String>();
                try
                {

                    //if (ModelState.IsValid)
                    //{
                    using (TransactionScope ts = new TransactionScope())
                    {

                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        PFTTDSMaster ir = new PFTTDSMaster()
                        {
                            TDSRate = c.TDSRate,
                            TaxableAccountCelling=c.TaxableAccountCelling,
                            IsIntOnIntTDSAppl = c.IsIntOnIntTDSAppl,
                            IsOwnerTDSAppl = c.IsOwnerTDSAppl,
                            IsOwnTDSAppl = c.IsOwnTDSAppl,
                            IsVPFTDSAppl = c.IsVPFTDSAppl,
                            DBTrack = c.DBTrack
                        };

                        db.PFTTDSMaster.Add(ir);

                        db.SaveChanges();

                        ts.Complete();
                        Msg.Add("Data Saved Successfully.");
                        return Json(new Utility.JsonReturnClass { Id = ir.Id, Val = ir.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }



                    //}
                    //else
                    //{
                    //StringBuilder sb = new StringBuilder("");
                    //foreach (ModelState modelState in ModelState.Values)
                    //{
                    //    foreach (ModelError error in modelState.Errors)
                    //    {
                    //        sb.Append(error.ErrorMessage);
                    //        sb.Append("." + "\n");
                    //    }
                    //}
                    //var errorMsg = sb.ToString();
                    //Msg.Add("Code Already Exists.");
                    //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //}
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }


        }


        //===================================================================================================




        //================================================================================================================





    }
}