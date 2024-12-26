
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
using Payroll;
using P2BUltimate.Security;
using Leave;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class LeaveDependPolicyController : Controller
    {
        //
        // GET: /LeaveDependPolicy/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PopulateDropDownList(string data, string data2)
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

        [HttpPost]
        public ActionResult Create(LeaveDependPolicy c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string LvHead = form["LvHead_drop"] == "0" ? "" : form["LvHead_drop"];


                    if (LvHead != null && LvHead != "")
                    {
                        int LvHead_Id = int.Parse(LvHead);
                        var LvHead_val = db.LvHead.Find(LvHead_Id);
                        c.LvHead = LvHead_val;
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.LeaveDependPolicy.Any(o => o.LvHead.LvCode == c.LvHead.LvCode))
                            //{
                            //    Msg.Add("  Code Already Exists.  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            LeaveDependPolicy LvDependPolicy = new LeaveDependPolicy()
                            {
                                AccMinLvDaysAppl = c.AccMinLvDaysAppl,
                                IsAccumulated = c.IsAccumulated,
                                IsContinous = c.IsContinous,
                                LvHead = c.LvHead,
                                MaxDays = c.MaxDays,
                                MinLvDaysAppl = c.MinLvDaysAppl,
                                DBTrack = c.DBTrack,

                            };
                            try
                            {
                                db.LeaveDependPolicy.Add(LvDependPolicy);
                                db.SaveChanges();


                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = LvDependPolicy.Id, Val = LvDependPolicy.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
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
                var Q = db.LeaveDependPolicy
                    .Include(e => e.LvHead)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        AccMinLvDaysAppl = e.AccMinLvDaysAppl,
                        IsAccumulated = e.IsAccumulated,
                        IsContinous = e.IsContinous,
                        MinLvDaysAppl = e.MinLvDaysAppl,
                        MaxDays = e.MaxDays,
                        LvHead_Id = e.LvHead.Id == null ? 0 : e.LvHead.Id
                    }).ToList();




                var Corp = db.LeaveDependPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(LeaveDependPolicy c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string LvHead = form["LvHead_drop"] == "0" ? "" : form["LvHead_drop"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (Auth == false)
                    {

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    LeaveDependPolicy blog = db.LeaveDependPolicy.Include(e => e.LvHead).Where(e => e.Id == data).FirstOrDefault(); // to retrieve old data

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    //Saltype
                                    if (LvHead != null & LvHead != "")
                                    {
                                        var val = db.LvHead.Find(int.Parse(LvHead));
                                        c.LvHead = val;

                                        var type = db.LeaveDependPolicy.Include(e => e.LvHead).Where(e => e.Id == data).SingleOrDefault();

                                        if (type.LvHead != null)
                                        {
                                            blog = db.LeaveDependPolicy.Where(x => x.LvHead.Id == type.LvHead.Id && x.Id == data).SingleOrDefault();
                                        }
                                        else
                                        {
                                            blog = db.LeaveDependPolicy.Where(x => x.Id == data).SingleOrDefault();
                                        }
                                        blog.LvHead = c.LvHead;
                                    }
                                    else
                                    {
                                        blog = db.LeaveDependPolicy.Include(e => e.LvHead).Where(x => x.Id == data).SingleOrDefault();
                                        blog.LvHead = null;
                                    }
                                    /* end */



                                    db.LeaveDependPolicy.Attach(blog);
                                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = blog.RowVersion;
                                    db.Entry(blog).State = System.Data.Entity.EntityState.Detached;

                                    //int a = EditS(type, frequency, roundingmethod, SalHeadOperType, ProcessType, data, c, c.DBTrack);
                                    var CurCorp = db.LeaveDependPolicy.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        LeaveDependPolicy corp = new LeaveDependPolicy()
                                        {
                                            AccMinLvDaysAppl = c.AccMinLvDaysAppl,
                                            IsAccumulated = c.IsAccumulated,
                                            IsContinous = c.IsContinous,
                                            MaxDays = c.MaxDays,
                                            MinLvDaysAppl = c.MinLvDaysAppl,
                                            Id = data,
                                            DBTrack = c.DBTrack,

                                        };

                                        db.LeaveDependPolicy.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;
                                    }

                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (SalaryHead)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (SalaryHead)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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

                            Msg.Add(" Please enter a valid input");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    return null;

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