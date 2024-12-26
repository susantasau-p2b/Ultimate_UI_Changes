using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class Query_AnalyzerController : Controller
    {
        //
        // GET: /Query_Analyzer/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/Query_Analyzer/Index.cshtml");
        }
        public ActionResult Create(QueryParameter QP, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {

                QP.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };
                string QTypelist = form["QTypelist"] == "0" ? "" : form["QTypelist"];         
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        try
                        {
                            QueryParameter iQueryParameter = new QueryParameter()
                            {
                                QLabel = QP.QLabel,
                                QName = QP.QName,
                                QDesc = QP.QDesc,
                                Type=QTypelist,
                                DBTrack = QP.DBTrack
                            };
                            db.QueryParameter.Add(iQueryParameter);
                            db.SaveChanges();
                            ts.Complete();
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
                }
            }

            Msg.Add("Data Saved successfully");
            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }

        public class P2BGridData
        {
            public string Id { get; set; }
            public string QLabel { get; set; }
            public string QName { get; set; }
            public string QDesc { get; set; }
            public string QType { get; set; }
        }


        public class iQTypelist
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public ActionResult GetQTypelistData(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<iQTypelist> qurey = new List<iQTypelist>();
                Dictionary<int, string> items = new Dictionary<int, string>
                {
                      { 1, "Engineer" },
                      { 2, "Guest" }
                };
                foreach (var item in items)
                {
                    qurey.Add(new iQTypelist
                    {
                        Id=item.Key,
                        Name=item.Value
                    });
                    
                }
                var selected = (Object)null;
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;

                    SelectList s2 = new SelectList(qurey, "Name", "Name", selected);
                    return Json(s2, JsonRequestBehavior.AllowGet);

                }
                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult P2bGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                List<P2BGridData> model = new List<P2BGridData>();
                var iQueryParameter = db.QueryParameter.ToList();
                        
                foreach (var item in iQueryParameter)
                {
                    model.Add(new P2BGridData
                    {
                        Id = item.Id.ToString(),
                        QLabel = item.QLabel == null ? "" : item.QLabel,
                        QName = item.QName == null ? "" : item.QName,
                        QDesc = item.QDesc == null ? "" : item.QDesc,
                        QType = item.Type == null ? "" : item.Type
                    });
                }
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = model
                         .Where((e => (e.QLabel.Contains(gp.searchString))
                                     || (e.QName.Contains(gp.searchString))
                                     || (e.QDesc.Contains(gp.searchString))
                                     || (e.QType.Contains(gp.searchString))
                                     || (e.Id.Contains(gp.searchString))
                                 )).ToList()
                        .Select(a => new Object[] { a.QLabel, a.QName, a.QDesc, a.QType, a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = model.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.QLabel, a.QName, a.QDesc, a.QType, a.Id });
                    }
                    totalRecords = model.Count();
                }
                else
                {
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : "0");
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "QLabel" ? c.QLabel :
                                         gp.sidx == "QName" ? c.QName :
                                         gp.sidx == "QDesc" ? c.QDesc :
                                         gp.sidx == "QType" ? c.QType : ""
                                   );


                    }
                    jsonData = model.Select(a => new Object[] { a.QLabel, a.QName, a.QDesc, a.QType, a.Id }).ToList();
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = model.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.QLabel, a.QName, a.QDesc, a.QType, a.Id }).ToList();
                    }
                    totalRecords = model.Count();
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
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var item = db.QueryParameter.Where(e => e.Id == data).SingleOrDefault();
                var model = new P2BGridData();
                model.QLabel = item.QLabel;
                model.QName = item.QName;
                model.QDesc = item.QDesc;
                model.QType = item.Type;
                model.Id = item.Id.ToString();
                return Json(new Object[] { model, JsonRequestBehavior.AllowGet });
            }
        }
        public ActionResult EditSave(QueryParameter QP, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                QP.DBTrack = new DBTrack { Action = "M", CreatedBy = SessionManager.UserName, IsModified = true, CreatedOn = DateTime.Now };
                string QTypelist = form["QTypelist"] == "0" ? "" : form["QTypelist"];         
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        try
                        {
                            var db_data = db.QueryParameter.Where(e => e.Id == data).SingleOrDefault();

                            db_data.Id = data;
                            db_data.QLabel = QP.QLabel;
                            db_data.QName = QP.QName;
                            db_data.QDesc = QP.QDesc;
                            db_data.Type = QTypelist;
                            db_data.DBTrack = QP.DBTrack;
                            db.QueryParameter.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            ts.Complete();
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
                }
            }

            Msg.Add("Updated successfully");
            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    QueryParameter iQueryParameter = db.QueryParameter.Where(e => e.Id == data).SingleOrDefault();

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            IsModified = iQueryParameter.DBTrack.IsModified == true ? true : false
                        };
                        db.Entry(iQueryParameter).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add(" Data removed.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
