using P2b.Global;
using P2B.PFTRUST;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace P2BUltimate.Controllers.PFTRUST
{
    public class PFMasterPFTController : Controller
    {
        //
        // GET: /PFMasterPFT/
        public ActionResult Index()
        {
            return View("~/Views/PFTrust/PFMasterPFT/Index.cshtml");
        }

        public ActionResult DropMenuEstablishmentID(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var PFTrustType = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "438").SingleOrDefault().LookupValues.ToList();
                var PFTrustTypeIds = PFTrustType.Where(e => e.LookupVal.ToUpper() == "EXEMPTED").Select(r => r.Id).ToList();
                var qurey = db.PFMaster.Include(e => e.PFTrustType).Where(e => PFTrustTypeIds.Contains(e.PFTrustType.Id)).ToList();
                var selected = (Object)null;
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = Convert.ToInt32(data2);
                }
                SelectList s = new SelectList(qurey, "Id", "EstablishmentID", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LookupInterestPolicies(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.InterestPolicies.ToList();
                IEnumerable<InterestPolicies> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.InterestPolicies.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public class returndatagridDataclass //Parentgrid
        {
            public string Id { get; set; }
            public string EstablishmentID { get; set; }
        }
        public ActionResult PFMasterPFTModel_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.PFMasterPFT.ToList();
                    IEnumerable<PFMasterPFT> fall;

                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {

                        fall = all.Where(e => (db.PFMaster.Where(a => a.Id == e.PFMaster_Id).SingleOrDefault().EstablishmentID.ToString().Contains(param.sSearch))).ToList();
                    }
                    List<returndatagridDataclass> result = new List<returndatagridDataclass>();

                    foreach (var item in fall)
                    {
                        result.Add(new returndatagridDataclass
                        {
                            Id = item.Id.ToString(),
                            EstablishmentID = db.PFMaster.Where(e => e.Id == item.PFMaster_Id).SingleOrDefault().EstablishmentID
                        });
                    }

                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = all.Count(),
                        iTotalDisplayRecords = fall.Count(),
                        data = result
                    }, JsonRequestBehavior.AllowGet);

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
        }
        public class returndatagridChildDataClass //Childgrid
        {
            public int Id { get; set; }
            public string InterestPolicies { get; set; }
        }
        public ActionResult A_PFMasterPFTModel_Grid(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<returndatagridChildDataClass> returndata = new List<returndatagridChildDataClass>();

                    var db_data = db.PFMasterPFT.Include(e => e.InterestPolicies)
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data.InterestPolicies.Count() > 0)
                    {
                        foreach (var item in db_data.InterestPolicies)
                        {
                            returndata.Add(new returndatagridChildDataClass
                            {
                                Id = item.Id,
                                InterestPolicies = item.FullDetails
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
        [HttpPost]
        public ActionResult create(FormCollection form, PFMasterPFT c)
        {
            List<string> Msg = new List<string>();
            string PFMasterID = form["PFMasterIDList"] == "0" ? null : form["PFMasterIDList"];
            string InterestPolicies = form["InterestPoliciesList"] == "0" ? null : form["InterestPoliciesList"];
            string EstablishmentID = form["EstablishmentID"] == "0" ? null : form["EstablishmentID"];
            string CompPFNo = form["CompPFNo"] == "0" ? null : form["CompPFNo"];
            string RegDate = form["RegDate"] == "0" ? null : form["RegDate"];
            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
            
            using (DataBaseContext db = new DataBaseContext())
            {
               
                if (!string.IsNullOrEmpty(PFMasterID))
                {
                    var PFMasterId = db.PFMaster.Find(Convert.ToInt32(PFMasterID)).Id;
                    c.PFMaster_Id = PFMasterId;
                }
                int pfid = Convert.ToInt32(PFMasterID);
                var checkduplicate = db.PFMasterPFT.Where(e => e.PFMaster_Id == pfid).SingleOrDefault();
                if (checkduplicate!=null)
                {
                     Msg.Add("Already Assigned ");
                      return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                List<InterestPolicies> Ip = new List<InterestPolicies>();
                if (!string.IsNullOrEmpty(InterestPolicies))
                {
                    var ids = Utility.StringIdsToListIds(InterestPolicies);
                    foreach (var ca in ids)
                    {
                        var Ip_val = db.InterestPolicies.Find(ca);
                        Ip.Add(Ip_val);
                    }
                    c.InterestPolicies = Ip;
                }
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        try
                        {
                            PFMasterPFT pfmasterpft = new PFMasterPFT()
                            {
                                PFMaster_Id = c.PFMaster_Id,
                                InterestPolicies = c.InterestPolicies,
                                CompPFNo = c.CompPFNo,
                                EstablishmentID = c.EstablishmentID,
                                RegDate = c.RegDate,
                                DBTrack = c.DBTrack
                            };
                            db.PFMasterPFT.Add(pfmasterpft);
                            db.SaveChanges();
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

                        ts.Complete();
                        Msg.Add(" Data Saved successfully ");
                    }
                }

            }

            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }

        public class OutergridDataclass
        {
            public string Id { get; set; }
            public string PFMaster_Id { get; set; }
        }

        public class InnergridDataclass
        {
            public string InterestPoliciesId { get; set; }
            public string InterestPoliciesFullDetails { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<OutergridDataclass> result = new List<OutergridDataclass>();
                 var ObjPFMasterPFT = db.PFMasterPFT.Include(e => e.InterestPolicies)
                     .Where(e => e.Id == data).SingleOrDefault();

                    result.Add(new OutergridDataclass
                    {
                        Id = ObjPFMasterPFT.Id.ToString(),
                        PFMaster_Id = ObjPFMasterPFT.PFMaster_Id.ToString()
                    });
               
                List<InnergridDataclass> oreturndata = new List<InnergridDataclass>();

                var return_data = db.PFMasterPFT.Include(e => e.InterestPolicies).Where(e => e.Id == data && e.InterestPolicies.Count > 0).SingleOrDefault();

                if (return_data != null)
                {
                    foreach (var item in return_data.InterestPolicies)
                    {
                        oreturndata.Add(new InnergridDataclass
                        {
                            InterestPoliciesId = item.Id.ToString(),
                            InterestPoliciesFullDetails = item.FullDetails

                        });
                    }
                }

                return Json(new Object[] { result, oreturndata, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public ActionResult EditSave(PFMasterPFT m, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            string PFMasterID = form["PFMasterIDList"] == "0" ? null : form["PFMasterIDList"];
            string InterestPolicies = form["InterestPoliciesList"] == "0" ? null : form["InterestPoliciesList"];
            string EstablishmentID = form["EstablishmentID"] == "0" ? null : form["EstablishmentID"];
            string CompPFNo = form["CompPFNo"] == "0" ? null : form["CompPFNo"];
            string RegDate = form["RegDate"] == "0" ? null : form["RegDate"];
            m.DBTrack = new DBTrack { Action = "M", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };

            using (DataBaseContext db = new DataBaseContext())
            {
                if (!string.IsNullOrEmpty(PFMasterID))
                {
                    var PFMasterId = db.PFMaster.Find(Convert.ToInt32(PFMasterID)).Id;
                    m.PFMaster_Id = PFMasterId;
                }
                List<InterestPolicies> Ip = new List<InterestPolicies>();
                if (!string.IsNullOrEmpty(InterestPolicies))
                {
                    var ids = Utility.StringIdsToListIds(InterestPolicies);
                    foreach (var ca in ids)
                    {
                        var Ip_val = db.InterestPolicies.Find(ca);
                        Ip.Add(Ip_val);
                    }
                    m.InterestPolicies = Ip;
                }
                var db_data = db.PFMasterPFT.Include(e=>e.InterestPolicies).Where(e => e.Id == data).SingleOrDefault();
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        try
                        {
                            db_data.Id = data;
                            db_data.InterestPolicies = m.InterestPolicies;
                            db_data.PFMaster_Id = m.PFMaster_Id;
                            db_data.CompPFNo = m.CompPFNo;
                            db_data.EstablishmentID = m.EstablishmentID;
                            db_data.RegDate = m.RegDate;
                            db_data.DBTrack = m.DBTrack;
                            db.PFMasterPFT.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;                                                      
                            db.SaveChanges();
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

                        ts.Complete();
                        Msg.Add(" Record Updated ");
                    }
                }

            }

            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                      PFMasterPFT Pfmasterpft = db.PFMasterPFT.Include(e => e.InterestPolicies).Where(e => e.Id == data).SingleOrDefault();

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {                           
                            db.Entry(Pfmasterpft).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    
                }
                catch (RetryLimitExceededException /* dex */)
                {

                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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