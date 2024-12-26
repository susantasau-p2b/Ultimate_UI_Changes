using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using P2BUltimate.Security;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class RateMasterController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Payroll/_RateMaster.cshtml");
        }
        public ActionResult Edit(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Id = Convert.ToInt32(data);
                var qurey = db.RateMaster
                    .Include(e => e.SalHead)
                    .Where(e => e.Id == Id)
                    .Select(e => new
                    {
                        Percentage = e.Percentage,
                        RateCode = e.Code,
                        Amount = e.Amount,
                        SalHead_Id = e.SalHead.Id.ToString()
                    }).ToList();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookupDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.RateMaster.ToList();
                IEnumerable<RateMaster> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.RateMaster.ToList().Where(d => d.Code.ToString().Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Percentage }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Create(RateMaster RateMast, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string SalHead = form["SalHead_drop"] == "0" ? "" : form["SalHead_drop"];

                    if (SalHead != null && SalHead != "")
                    {
                        var val = db.SalaryHead.Find(int.Parse(SalHead));
                        RateMast.SalHead = val;
                    }

                    RateMast.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                    RateMaster RateMaster = new RateMaster()
                    {
                        Code = RateMast.Code == null ? "" : RateMast.Code.Trim(),
                        Percentage = RateMast.Percentage,
                        SalHead = RateMast.SalHead,
                        Amount = RateMast.Amount,
                        DBTrack = RateMast.DBTrack
                    };
                    try
                    {
                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.RateMaster.Add(RateMaster);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, RateMast.DBTrack);
                                DT_RateMaster DT_RateMast = (DT_RateMaster)rtn_Obj;
                                DT_RateMast.SalHead_Id = RateMast.SalHead == null ? 0 : RateMast.SalHead.Id;
                                db.Create(DT_RateMast);
                                db.SaveChanges();
                                ts.Complete();
                            }
                        }
                        Msg.Add("  Data Saved successfully  ");
                        return Json(new Utility.JsonReturnClass { Id = RateMaster.Id, Val = RateMaster.Code, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { RateMaster.Id, RateMaster.Code, "Data saved successfully.", JsonRequestBehavior.AllowGet });
                    }

                    catch (DbUpdateConcurrencyException)
                    {
                        return RedirectToAction("Create", new { concurrencyError = true, id = RateMast.Id });
                    }
                    catch (DataException /* dex */)
                    {
                        Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                var RateMaster = db.RateMaster.Include(e => e.SalHead).ToList();
                IEnumerable<RateMaster> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = RateMaster;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Percentage), Convert.ToString(a.Amount), Convert.ToString(a.SalHead.Id) }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Percentage, a.Amount, a.SalHead.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = RateMaster;
                    Func<RateMaster, string> orderfuc = (OBJ =>
                                                               gp.sidx == "Id" ? OBJ.Id.ToString() :
                                                               gp.sidx == "RateCode" ? OBJ.Code :
                                                               gp.sidx == "Percentage" ? OBJ.Percentage.ToString() :
                                                                gp.sidx == "Amount" ? OBJ.Amount.ToString() :
                                                               gp.sidx == "SalHead" ? OBJ.SalHead.Id.ToString() : "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Percentage), Convert.ToString(a.Amount), Convert.ToString(a.SalHead.Id) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Percentage), Convert.ToString(a.Amount), Convert.ToString(a.SalHead.Id) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Percentage, a.Amount, a.SalHead.Id }).ToList();
                    }
                    totalRecords = RateMaster.Count();
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
        public ActionResult EditSave(RateMaster OBJ, int data, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var db_data = db.RateMaster
                        .Include(e => e.SalHead)
                        .Where(e => e.Id == data).SingleOrDefault();

                    var sal_id = Convert.ToInt32(form["SalHead_drop"]);
                    var sal_val = db.SalaryHead.Find(sal_id);

                    db_data.SalHead = sal_val;

                    db.RateMaster.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                    RateMaster RateMaster = new RateMaster()
                    {
                        Code = OBJ.Code == null ? "" : OBJ.Code.Trim(),
                        Percentage = OBJ.Percentage,
                        SalHead = OBJ.SalHead,
                        Amount = OBJ.Amount,
                        Id = data
                    };

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                db.RateMaster.Attach(RateMaster);
                                db.Entry(RateMaster).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(RateMaster).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                            }
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = RateMaster.Id, Val = RateMaster.Code, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { RateMaster.Id, RateMaster.Code, "Data Updated.", JsonRequestBehavior.AllowGet });
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Edit", new { concurrencyError = true, id = OBJ.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                            ModelState.AddModelError(string.Empty, "Unable to Edit. Try again, and if the problem persists contact your system administrator.");
                            return RedirectToAction("Edit");
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
        public ActionResult Delete(string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var id = Convert.ToInt32(data);
                    var qurey = db.RateMaster.Find(id);
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            db.RateMaster.Attach(qurey);
                            db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            db.Entry(qurey).State = System.Data.Entity.EntityState.Deleted;
                            ts.Complete();
                        }
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new { msg = "Record Deleted", JsonRequestBehavior.AllowGet });
                    }
                }
                catch (DataException e) { throw e; }
                catch (DBConcurrencyException e) { throw e; }
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
                return RedirectToAction("index");
            }
        }

        public class EditData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string SalHead { get; set; }
            public string Percentage { get; set; }
            public string Amount { get; set; }
            public bool Editable { get; set; }
        }


        public ActionResult P2BInlineGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EditData> RateMaster = null;

                List<EditData> model = new List<EditData>();
                var view = new EditData();
                int ID = 0;
                var serialize = new JavaScriptSerializer();


                //string Month = "";
                if (gp.filter != null)
                {
                    string[] A = gp.filter.Split(',');
                    if (A.Count() > 0)
                    {
                        foreach (var ca in A)
                        {
                            ID = Convert.ToInt32(ca);

                            var ORateMaster = db.RateMaster.Include(e => e.SalHead).Where(e => e.Id == ID).ToList();


                            foreach (var z in ORateMaster)
                            {

                                bool EditAppl = true;
                                view = new EditData()
                                {
                                    Id = z.Id,
                                    Amount = z.Amount != null ? z.Amount.ToString() : null,
                                    Code = z.Code != null ? z.Code.ToString() : null,
                                    SalHead = z.SalHead != null ? z.SalHead.Name : "",
                                    Percentage = z.Percentage != null ? z.Percentage.ToString() : "",
                                    Editable = EditAppl
                                };

                                model.Add(view);
                            }

                        }
                    }
                }

                RateMaster = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = RateMaster;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Code != null ? Convert.ToString(a.Code) : "", a.SalHead != null ? Convert.ToString(a.SalHead) : "", a.Percentage != null ? a.Percentage.ToString() : "", a.Amount.ToString(), a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code != null ? Convert.ToString(a.Code) : "", a.SalHead != null ? Convert.ToString(a.SalHead) : "", a.Percentage != null ? a.Percentage.ToString() : "", a.Amount.ToString(), a.Editable }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = RateMaster;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.Code :
                                         gp.sidx == "EmpName" ? c.Percentage : ""
                                        );
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Code != null ? Convert.ToString(a.Code) : "", a.SalHead != null ? Convert.ToString(a.SalHead) : "", a.Percentage != null ? a.Percentage.ToString() : "", a.Amount.ToString(), a.Editable }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Code != null ? Convert.ToString(a.Code) : "", a.SalHead != null ? Convert.ToString(a.SalHead) : "", a.Percentage != null ? a.Percentage.ToString() : "", a.Amount.ToString(), a.Editable }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code != null ? Convert.ToString(a.Code) : "", a.SalHead != null ? Convert.ToString(a.SalHead) : "", a.Percentage != null ? a.Percentage.ToString() : "", a.Amount.ToString(), a.Editable }).ToList();
                    }
                    totalRecords = RateMaster.Count();
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


        public class DeserializeClass
        {
            public String Id { get; set; }
            public string Code { get; set; }
            public string SalHead { get; set; }
            public double Percentage { get; set; }
            public double Amount { get; set; }
        }

        public ActionResult EditVal(string Selected, FormCollection form, String forwarddata)
        {
            // string A = form["forwarddata"];
            // var serialize = new JavaScriptSerializer();
            // //string key = d["key"];
            // // string status = d["status"];
            // //dynamic[] members = d["members"];
            // //foreach (var member in json["members"])
            // //{
            // //    foreach (var item in member["items"])
            // //    {
            // //        Console.WriteLine(item["name"]);
            // //    }
            // //}
            // dynamic json = serialize.DeserializeObject(forwarddata);
            //    dynamic [] value = json;
            // // var Key = json["Key"];
            // //dynamic[] member = json["member"];
            // JObject obj = JObject.Parse(json);
            //// string name = (string)obj["Name"];
            // for (var i = 0; i < value.Length; i++)
            // {


            // }
            using (DataBaseContext db = new DataBaseContext())
            {
                var serialize = new JavaScriptSerializer();
                var obj = serialize.Deserialize<List<DeserializeClass>>(forwarddata);
                List<int> ids = obj.Select(e => int.Parse(e.Id)).ToList();
                using (TransactionScope ts = new TransactionScope())
                {
                    foreach (int ca in ids)
                    {
                        try
                        {
                            RateMaster RTM = db.RateMaster.Find(ca);
                            RTM.Percentage = Convert.ToInt32(obj.Where(e => e.Id == ca.ToString()).Select(e => e.Percentage).Single());
                            RTM.Amount = Convert.ToInt32(obj.Where(e => e.Id == ca.ToString()).Select(e => e.Amount).Single());
                            db.RateMaster.Attach(RTM);
                            db.Entry(RTM).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(RTM).State = System.Data.Entity.EntityState.Detached;

                        }
                        catch (DataException ex)
                        {
                            throw (ex);
                        }


                    }
                    ts.Complete();
                    return this.Json(new { success = true, responseText = "Rate Master Updated Successfully.. " }, JsonRequestBehavior.AllowGet);
                }

                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
    }
}
