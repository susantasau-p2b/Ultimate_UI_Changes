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
    public class InterestRateController : Controller
    {
        // GET: InterestRate
        public ActionResult Index()
        {
            return View();
        }



        public ActionResult CreateSave(InterestRate c, FormCollection form)
        {

            using (DataBaseContext db = new DataBaseContext())
            {

                List<String> Msg = new List<String>();
                try
                {

                    //if (ModelState.IsValid)
                    //{
                    using (TransactionScope ts = new TransactionScope())
                    {

                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        InterestRate ir = new InterestRate()
                        {
                            Name = c.Name,
                            EffectiveFrom = c.EffectiveFrom,
                            EffectiveTo = c.EffectiveTo,
                            GovtEMPPFInt = c.GovtEMPPFInt,
                            GovtVPFInt = c.GovtVPFInt,
                            TrustVPFInt = c.TrustVPFInt,
                            TrustEMPPFInt = c.TrustEMPPFInt,
                            DBTrack = c.DBTrack
                        };

                        db.InterestRate.Add(ir);

                        db.SaveChanges();

                        ts.Complete();
                        Msg.Add("Data Saved Successfully.");
                        return Json(new Utility.JsonReturnClass {Id= ir.Id, Val=ir.FullDetails,success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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