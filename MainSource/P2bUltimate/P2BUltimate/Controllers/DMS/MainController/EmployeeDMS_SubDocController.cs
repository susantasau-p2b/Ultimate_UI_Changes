using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using P2BUltimate.Models;
using P2b.Global;
using System.Text;
using System.Data;
using System.Transactions;
using P2BUltimate.Security;
using System.Threading.Tasks;


namespace P2BUltimate.Controllers.DMS.MainController
{
    public class EmployeeDMS_SubDocController : Controller
    {
        //
        // GET: /EmployeeDMS_SubDoc/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Partial(string FuncModule, string DocType)
        {
            Session["FuncModule"] = FuncModule;
            Session["DocType"] = DocType;
            return View("~/Views/Shared/DMS/_EmployeeDMS_SubDoc.cshtml");
        }

        

        public ActionResult GetLookupValue(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                int FuncModuleId = Convert.ToInt32(Session["FuncModule"]);
                int DocTypeId = Convert.ToInt32(Session["DocType"]);
                var query = db.LookupValue.Where(e => e.Id == DocTypeId).FirstOrDefault().LookupVal;
                var query1 = db.Lookup.Where(e => e.Code == "591").Include(e => e.LookupValues).FirstOrDefault();
                if (query1 != null)
                {
                    var query2 = query1.LookupValues.Where(t => t.LookupValData.ToUpper() == query.ToUpper());
                    if (SkipIds != null)
                    {
                        foreach (var a in SkipIds)
                        {
                            if (query2 == null)
                                query2 = db.LookupValue.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList().OrderBy(e => e.Id);
                            else
                                query2 = query2.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList().OrderBy(e => e.Id);
                        }

                        var r = (from ca in query2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();

                        return Json(r, JsonRequestBehavior.AllowGet);
                    }
                    var r1 = (from ca in query2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();

                    return Json(r1, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(EmployeeDMS_SubDoc EmpSubDoc, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string SubDocumentType = form["SubDocumentType"] == "0" ? "" : form["SubDocumentType"];
                    

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            EmpSubDoc.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //if (SubDocumentType != null && SubDocumentType != "")
                            //{
                            //    var val = db.LookupValue.Where(e => e.LookupVal.ToUpper() == SubDocumentType.ToUpper()).FirstOrDefault();

                            //    if (db.EmployeeDMS_SubDoc.Any(o => o.SubDocName == val))
                            //    {
                            //        Msg.Add("  Document Already Exists.  ");
                            //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //    }

                            //    if (val == null)
                            //    {
                            //        int DocTypeId = Convert.ToInt32(Session["DocType"]);
                            //        var query = db.LookupValue.Where(e => e.Id == DocTypeId).FirstOrDefault().LookupVal;
                                    
                            //        LookupValue LookupValue = new LookupValue
                            //        {
                            //            LookupVal = SubDocumentType,
                            //            IsActive = true,
                            //            DeleteValue = false,
                            //            DBTrack = EmpSubDoc.DBTrack,
                            //            LookupValData = query
                            //        };
                            //        db.LookupValue.Add(LookupValue);
                            //        db.SaveChanges();

                            //        List<LookupValue> lkval = new List<P2b.Global.LookupValue>();
                            //        lkval.Add(LookupValue);
                            //        var Lookup = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "591").FirstOrDefault();
                            //        if (Lookup != null)
                            //        {
                            //            Lookup.LookupValues = lkval;

                            //            db.Lookup.Attach(Lookup);
                            //            db.Entry(Lookup).State = System.Data.Entity.EntityState.Modified;
                            //            db.SaveChanges();
                            //            db.Entry(Lookup).State = System.Data.Entity.EntityState.Detached;
                            //        }
                            //        else 
                            //        {
                            //            Lookup lookup = new Lookup()
                            //            {
                            //                Code = "591",
                            //                Name = "SubDocument Type",
                            //                LookupValues = lkval,
                            //                DBTrack = EmpSubDoc.DBTrack
                            //            };
                            //            db.Lookup.Add(lookup);
                            //            db.SaveChanges();
                            //        }

                            //    }
                            //    EmpSubDoc.SubDocName = val;
                            //}

                            EmployeeDMS_SubDoc EmployeeDMS_SubDoc = new EmployeeDMS_SubDoc()
                            {
                                SubDocName = EmpSubDoc.SubDocName,
                                SubDocumentDesc = EmpSubDoc.SubDocumentDesc,
                                DBTrack = EmpSubDoc.DBTrack

                            };
                            try
                            {
                                db.EmployeeDMS_SubDoc.Add(EmployeeDMS_SubDoc);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = EmployeeDMS_SubDoc.Id, Val=EmployeeDMS_SubDoc.FullDetails,  success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = EmpSubDoc.Id });
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

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.EmployeeDMS_SubDoc 
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        SubDocumentDesc = e.SubDocumentDesc,
                        SubDocName = e.SubDocName,
                        Action = e.DBTrack.Action,
                    }).ToList();
               
                var Corp = db.EmployeeDMS_SubDoc.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        public ActionResult EditSubDoc_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = (from ca in db.EmployeeDMS_SubDoc
                         .Where(e => e.Id == data)
                         select new
                         {
                             Id = ca.Id,
                             SubDocumentName = ca.SubDocName,
                             SubDocumentDesc = ca.SubDocumentDesc
                         }).ToList();



                TempData["RowVersion"] = db.EmployeeDMS_SubDoc.Find(data).RowVersion;
                return Json(new object[] { r, "", JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(EmployeeDMS_SubDoc c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();
            string SubDocumentType = form["SubDocumentType"] == "0" ? "" : form["SubDocumentType"];

            
           

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                    //    if (SubDocumentType != null && SubDocumentType == "")
                    //{
                    //        var val = db.LookupValue.Where(e => e.LookupVal.ToUpper() == SubDocumentType.ToUpper()).FirstOrDefault();

                    //        c.SubDocumentName_Id = SubDocumentType != null && SubDocumentType != "" ? val.Id : 0;  
                    //    }
                        

                        EmployeeDMS_SubDoc SubDoc = db.EmployeeDMS_SubDoc.Find(data);
                        TempData["CurrRowVersion"] = SubDoc.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = SubDoc.DBTrack.CreatedBy == null ? null : SubDoc.DBTrack.CreatedBy,
                                CreatedOn = SubDoc.DBTrack.CreatedOn == null ? null : SubDoc.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            SubDoc.SubDocumentDesc = c.SubDocumentDesc;
                            SubDoc.SubDocName = c.SubDocName;
                            SubDoc.Id = data;
                            SubDoc.DBTrack = c.DBTrack;
                            db.Entry(SubDoc).State = System.Data.Entity.EntityState.Modified;
                           
                            db.SaveChanges();
                        }
                        //}
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
	}
}