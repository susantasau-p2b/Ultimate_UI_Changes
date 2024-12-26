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

namespace P2BUltimate.Controllers.Core.MainController
{
    public class NameDetailsController : Controller
    {

        //  private DataBaseContext db = new DataBaseContext();

        // GET: /NameDetails/

        public ActionResult Index()
        {
            return View();
        }


        private MultiSelectList GetNameDetails(int selectedValues)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<NameDetails> namedetails = new List<NameDetails>();
                namedetails = db.NameDetails.ToList();
                return new SelectList(namedetails, "Id", "Name", selectedValues);
            }

        }


        [HttpPost]
        //validation data
        public ActionResult PopulateLookupDropDownList(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var lookupQuery = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "100").SingleOrDefault();

                List<SelectListItem> values = new List<SelectListItem>();

                if (lookupQuery != null)
                {
                    foreach (var item in lookupQuery.LookupValues)
                    {
                        if (item.IsActive == true)
                        {
                            values.Add(new SelectListItem
                            {
                                Text = item.LookupVal,
                                Value = item.Id.ToString(),
                                Selected = (item.Id == Convert.ToInt32(data) ? true : false)
                            });
                        }
                    }
                }
                return Json(values, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult CreateSave(NameDetails td, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Category = form["Namedetails"];

                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "100").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(Category));
                            td.EmpTitle = val;
                        }
                    }



                    if (ModelState.IsValid)
                    {
                        NameDetails nds = new NameDetails()
                      {

                          EmpFName = td.EmpFName == null ? "" : td.EmpFName.Trim(),
                          EmpMName = td.EmpMName == null ? "" : td.EmpMName.Trim(),
                          EmpTitle = td.EmpTitle,
                          FatherLName = td.FatherLName == null ? "" : td.FatherLName.Trim(),
                          FatherFName = td.FatherFName == null ? "" : td.FatherFName.Trim(),
                          FatherMName = td.FatherMName == null ? "" : td.FatherMName.Trim(),
                          EmpLName = td.EmpLName == null ? "" : td.EmpLName.Trim(),
                          HusbandFName = td.HusbandFName == null ? "" : td.HusbandFName.Trim(),
                          HusbandLName = td.HusbandLName == null ? "" : td.HusbandLName.Trim(),
                          HusbandMName = td.HusbandMName == null ? "" : td.HusbandMName.Trim(),
                          MotherFName = td.MotherFName == null ? "" : td.MotherFName.Trim(),
                          MotherLName = td.MotherLName == null ? "" : td.MotherLName.Trim(),
                          MotherMName = td.MotherMName == null ? "" : td.MotherMName.Trim(),
                          PreviousFName = td.PreviousFName == null ? "" : td.PreviousFName.Trim(),
                          PreviousLName = td.PreviousLName == null ? "" : td.PreviousLName.Trim(),
                          PreviousMName = td.PreviousMName == null ? "" : td.PreviousMName.Trim()

                      };
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.NameDetails.Add(nds);
                                db.SaveChanges();
                                ts.Complete();

                            }
                            // return Json(new Object[] { nds.Id, nds.FullNameFML, "Data saved successfully." }, JsonRequestBehavior.AllowGet);
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = nds.Id, Val = nds.FullNameFML, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = td.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //ModelState.AddModelError(string.Empty, "Unable to create. Try again, and if the problem persists contact your system administrator.");
                            return View(td);
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
            }
        }




        /*--------------------------- Edit Save ----------------------------*/

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult EditSave(NameDetails ca, int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        NameDetails contact = new NameDetails()
                        {
                            EmpFName = ca.EmpFName,
                            EmpMName = ca.EmpMName,
                            EmpLName = ca.EmpLName,
                            FatherFName = ca.FatherFName,
                            FatherLName = ca.FatherLName,
                            FatherMName = ca.FatherMName,
                            HusbandFName = ca.HusbandFName,
                            HusbandLName = ca.HusbandLName,
                            HusbandMName = ca.HusbandMName,
                            MotherFName = ca.MotherFName,
                            MotherLName = ca.MotherLName,
                            MotherMName = ca.MotherMName,
                            PreviousFName = ca.PreviousFName,
                            PreviousLName = ca.PreviousLName,
                            PreviousMName = ca.PreviousMName,

                            Id = data
                        };

                        db.Entry(ca).State = System.Data.Entity.EntityState.Detached;
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.Entry(contact).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                ts.Complete();
                            }

                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { null, null, "Data saved successfully." }, JsonRequestBehavior.AllowGet);
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Edit", new { concurrencyError = true, id = ca.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                            ModelState.AddModelError(string.Empty, "Unable to edit. Try again, and if the problem persists contact your system administrator.");
                            return View(ca);
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
            }
        }


    }
}

