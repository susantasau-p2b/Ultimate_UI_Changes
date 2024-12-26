using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
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

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class JVFileNameController : Controller
    {
        //
        // GET: /JVFile/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/JVFileName/Index.cshtml");
        }

        

        public class returnEditClass
        {
            public int JVField_Id { get; set; }
            public string JVField_FullDetails { get; set; }
            public int JVHead_Id { get; set; }
            public string JVHead_FullDetails { get; set; }
        }


        [HttpPost]
        public ActionResult GetJVHeadLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.JVParameter.Include(e => e.PaymentBank).ToList(); 

                if (SkipIds != null)
                {
                    foreach (int a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.JVParameter.Include(e => e.PaymentBank).Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();

                    }
                }

               var list1 = db.JVFileName.Where(e => e.JVHeadList.Count() > 0).ToList().SelectMany(e => e.JVHeadList);
               
                var list3 = fall.Except(list1);

                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in list3 select new { srno = ca.Id, lookupvalue = ca.JVName + " - " + ca.CreditDebitFlag + " - " + (ca.PaymentBank != null ? ca.PaymentBank.Code : "") }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetJVFileFormatLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.JVFileFormat.Include(e => e.FormatType).Include(e => e.CBS).Include(e => e.Version).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.JVFileFormat.Include(e => e.FormatType).Include(e => e.CBS).Include(e => e.Version).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }



                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetJVFieldLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.JVField.Include(e => e.Name).Include(e => e.PaddingChar).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.JVField.Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();

                    }
                }



                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(JVFileName c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string JVHead = form["JVHeadList"] == "0" ? "" : form["JVHeadList"];
                    string JVFileFormat = form["JVFileFormatList"] == "0" ? "" : form["JVFileFormatList"];
                    string JVField = form["JVFieldList"] == "0" ? "" : form["JVFieldList"];
                  
                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var companypayroll = new CompanyPayroll();
                    companypayroll = db.CompanyPayroll.Where(e => e.Company.Id == company_Id).SingleOrDefault();



                    if (JVFileFormat != null && JVFileFormat != "")
                    {
                        var val = db.JVFileFormat.Find(int.Parse(JVFileFormat));
                        c.JVFileFormat = val;
                    }

                    List<JVField> lookupval = new List<JVField>();
                    if (JVField != null && JVField != "")
                    {
                        var ids = Utility.StringIdsToListIds(JVField);
                        foreach (var ca in ids)
                        {
                            var LvHead_val = db.JVField.Find(ca);
                            lookupval.Add(LvHead_val);
                            c.JVField = lookupval;
                        }
                    }


                    List<JVParameter> lookupvalJV = new List<JVParameter>();
                    if (JVHead != null && JVHead != "")
                    {
                        var ids = Utility.StringIdsToListIds(JVHead);
                        foreach (var ca in ids)
                        {
                            var LvHead_val = db.JVParameter.Find(ca);
                            lookupvalJV.Add(LvHead_val);
                            c.JVHeadList = lookupvalJV;
                        }
                    }

                    //if (ModelState.IsValid)
                    //{
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.JVFileName.Any(o => o.Name == c.Name))
                            {
                                Msg.Add("  Name Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                         

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            JVFileName JVFileName = new JVFileName()
                            { 
                                Name = c.Name == null ? "" : c.Name.Trim(),
                                JVField = c.JVField,
                                JVFileFormat = c.JVFileFormat,
                                JVHeadList = c.JVHeadList,
                                DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.JVFileName.Add(JVFileName); 
                                db.SaveChanges();


                                //if (companypayroll != null)
                                //{
                                //    var SalaryHead_list = new List<SalaryHead>();
                                //    SalaryHead_list.Add(SalaryHead);
                                //    companypayroll.SalaryHead = SalaryHead_list;
                                //    db.CompanyPayroll.Attach(companypayroll);
                                //    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Modified;
                                //    db.SaveChanges();
                                //    db.Entry(companypayroll).State = System.Data.Entity.EntityState.Detached;
                                //}


                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");

                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                    //}
                    //else
                    //{
                    //    StringBuilder sb = new StringBuilder("");
                    //    foreach (ModelState modelState in ModelState.Values)
                    //    {
                    //        foreach (ModelError error in modelState.Errors)
                    //        {
                    //            sb.Append(error.ErrorMessage);
                    //            sb.Append("." + "\n");
                    //        }
                    //    }
                    //    var errorMsg = sb.ToString();
                    //    Msg.Add(errorMsg);
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //    //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                    //}
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
                var Q = db.JVFileName.Include(e => e.JVFileFormat)
                       .Include(e => e.JVFileFormat.FormatType.LookupVal)
                       .Where(e => e.Id == data).Select
                       (e => new
                       {
                           Name = e.Name,
                           JVFileFormat_Id = e.JVFileFormat_Id,
                           JVFileFormat_FullDetails = e.JVFileFormat.FormatType.LookupVal
                       }).ToList();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();

                var k = db.JVFileName.Include(e => e.JVField).Include(e => e.JVField.Select(t => t.Name)).Include(e => e.JVHeadList).Include(e => e.JVHeadList.Select(t => t.PaymentBank))
                         .Where(e => e.Id == data).SingleOrDefault();
                var query11 = k.JVField.OrderBy(e => e.SeqNo).ToList();
                foreach (var item1 in query11)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {

                        JVField_Id = item1.Id,
                        JVField_FullDetails = item1.FullDetails
                    });
                }
                var query12 = k.JVHeadList.ToList();
                foreach (var item2 in query12)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        JVHead_Id = item2.Id,
                        JVHead_FullDetails = item2.JVName + " - " + item2.CreditDebitFlag + " - " + (item2.PaymentBank != null ? item2.PaymentBank.Code : ""),
                    });
                }                   
                var Corp = db.SalaryHead.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, oreturnEditClass, null, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(JVFileName c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();
            string JVHead = form["JVHeadList"] == "0" ? "" : form["JVHeadList"];
            string JVFileFormat = form["JVFileFormatList"] == "0" ? "" : form["JVFileFormatList"];
            string JVField = form["JVFieldList"] == "0" ? "" : form["JVFieldList"];

            c.JVFileFormat_Id = JVFileFormat != null && JVFileFormat != "" ? int.Parse(JVFileFormat) : 0;
             



            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.JVFileName.Include(e => e.JVField).Include(e => e.JVHeadList).Where(e => e.Id == data).FirstOrDefault();
                        List<JVField> JVF = new List<JVField>(); 
                        List<JVParameter> JVParam = new List<JVParameter>();

                        if (JVHead != null)
                        {
                            var ids = Utility.StringIdsToListIds(JVHead);
                            foreach (var ca in ids)
                            {
                                var JVHead_val = db.JVParameter.Find(ca);
                                JVParam.Add(JVHead_val);
                                db_data.JVHeadList = JVParam;
                            }
                        }
                        else
                        {
                            db_data.JVHeadList = null;
                        }


                        if (JVField != null)
                        {
                            var ids = Utility.StringIdsToListIds(JVField);
                            foreach (var ca in ids)
                            {
                                var JVField_val = db.JVField.Find(ca);
                                JVF.Add(JVField_val);
                                db_data.JVField = JVF;
                            }
                        }
                        else
                        {
                            db_data.JVField = null;
                        }


                        db.JVFileName.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;

                        JVFileName JVFileName = db.JVFileName.Find(data);
                        TempData["CurrRowVersion"] = JVFileName.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = JVFileName.DBTrack.CreatedBy == null ? null : JVFileName.DBTrack.CreatedBy,
                                CreatedOn = JVFileName.DBTrack.CreatedOn == null ? null : JVFileName.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            JVFileName.Name = c.Name;
                            JVFileName.JVFileFormat_Id = c.JVFileFormat_Id;
                            JVFileName.Id = data;
                            JVFileName.DBTrack = c.DBTrack;
                            // SalHead.LvHead = LvH;
                            db.Entry(JVFileName).State = System.Data.Entity.EntityState.Modified;
                            // db.SaveChanges();

                            //using (var context = new DataBaseContext())
                            ////{
                            //SalaryHead blog = null; // to retrieve old data
                            //DbPropertyValues originalBlogValues = null;

                            //blog = db.SalaryHead.Where(e => e.Id == data).Include(e => e.Frequency)
                            //                        .Include(e => e.RoundingMethod)
                            //                        .Include(e => e.Type).SingleOrDefault();
                            //originalBlogValues = db.Entry(blog).OriginalValues;
                            //db.ChangeTracker.DetectChanges();
                            //var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                            //DT_SalaryHead DT_Corp = (DT_SalaryHead)obj;
                            //DT_Corp.Type_Id = blog.Type == null ? 0 : blog.Type.Id;
                            //DT_Corp.Frequency_Id = blog.Frequency == null ? 0 : blog.Frequency.Id;
                            //DT_Corp.RoundingMethod_Id = blog.RoundingMethod == null ? 0 : blog.RoundingMethod.Id;
                            //DT_Corp.SalHeadOperationType_Id = blog.SalHeadOperationType == null ? 0 : blog.SalHeadOperationType.Id;
                            //DT_Corp.ProcessType_Id = blog.ProcessType == null ? 0 : blog.ProcessType.Id;


                            //db.Create(DT_Corp);
                            db.SaveChanges();
                        }
                        //}
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                int ParentId = 2;
                var jsonData = (Object)null;
                var JVFile = db.JVFileName.ToList();

                if (gp.IsAutho == true)
                {
                    JVFile = db.JVFileName.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    JVFile = db.JVFileName.AsNoTracking().ToList();
                }


                IEnumerable<JVFileName> IE;
                if (!string.IsNullOrEmpty(gp.searchString))
                {
                    IE = JVFile;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] {  a.Name, a.Id }).ToList();

                        //jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code == gp.searchString) || (e.Name.ToLower() == gp.searchString.ToLower())));
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Name, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = JVFile;
                    Func<JVFileName, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Name" ? c.Name : "");
                    }

                    //Func<Lookup, string> orderfuc = (c =>
                    //                                           gp.sidx == "Id" ? c.Id.ToString() :
                    //                                           gp.sidx == "Code" ? c.Code :
                    //                                           gp.sidx == "Name" ? c.Name : "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  a.Name, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Name, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Name, a.Id }).ToList();
                    }
                    totalRecords = JVFile.Count();
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
                    total = totalPages,
                    p2bparam = ParentId
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
	}
}