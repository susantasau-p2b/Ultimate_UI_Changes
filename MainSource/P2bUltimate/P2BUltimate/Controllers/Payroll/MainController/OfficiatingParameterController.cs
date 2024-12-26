using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class OfficiatingParameterController : Controller
    {
        //
        // GET: /OfficiatingParameter/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/OfficiatingParameter/Index.cshtml");
        }


        [HttpPost]
        public ActionResult GetSalHeadLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.SalaryHead.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.SalaryHead.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                } 
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.PayScaleAgreement.Include(e => e.PromoActivity).ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(OfficiatingParameter OffP, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string AllowanceList = form["AllowanceList"] == "0" ? "" : form["AllowanceList"];
                    var PayscaleagreementdetailsCreatelist = form["PayscaleagreementdetailsCreatelist"] == "0" ? "" : form["PayscaleagreementdetailsCreatelist"];

                    OffP.AllowanceList = null;
                    List<SalaryHead> OBJ = new List<SalaryHead>();

                    if (AllowanceList != null)
                    {
                        var ids = one_ids(AllowanceList);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.SalaryHead.Find(ca);
                            OBJ.Add(OBJ_val);
                            OffP.AllowanceList = OBJ;
                        }
                    }

                    var PsAgreement = new PayScaleAgreement();
                    if (PayscaleagreementdetailsCreatelist != null && PayscaleagreementdetailsCreatelist != "")
                    {
                        PsAgreement = db.PayScaleAgreement.Find(int.Parse(PayscaleagreementdetailsCreatelist));
                    }

                    //if (ModelState.IsValid)
                    //{
                        using (TransactionScope ts = new TransactionScope())
                        {

                            OffP.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };



                            OfficiatingParameter OfficiatingPara = new OfficiatingParameter()
                            {
                                Name = OffP.Name == null ? "" : OffP.Name.Trim(),
                                 AllowanceAppl = OffP.AllowanceAppl,
                                 AllowanceList = OffP.AllowanceList,
                                 FixedAmount = OffP.FixedAmount,
                                 FixedAmountAppl = OffP.FixedAmountAppl,
                                 NewGradeBasicAppl = OffP.NewGradeBasicAppl,
                                 OfficiatingToSalary = OffP.OfficiatingToSalary,
                                 ScaleFirstBasic = OffP.ScaleFirstBasic,
                                OfficiatingEmpPayStructAppl = OffP.OfficiatingEmpPayStructAppl,
                                NewPayStructOnScreenAppl = OffP.NewPayStructOnScreenAppl,
                                PayAmountuppergradediffAppl=OffP.PayAmountuppergradediffAppl,
                                GradeShiftCount = OffP.GradeShiftCount,
                                IncrementCount = OffP.IncrementCount,
                                DBTrack = OffP.DBTrack
                            };
                            try
                            {
                                db.OfficiatingParameter.Add(OfficiatingPara);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, OffP.DBTrack);
                                DT_OfficiatingParameter DT_OBJ = (DT_OfficiatingParameter)rtn_Obj; 
                                db.Create(DT_OBJ);
                                db.SaveChanges();

                                List<OfficiatingParameter> officiatingpolicylist = new List<OfficiatingParameter>();
                                officiatingpolicylist.Add(OfficiatingPara);
                                if (PsAgreement != null)
                                {
                                    PsAgreement.OfficiatingParameter = officiatingpolicylist;
                                    db.PayScaleAgreement.Attach(PsAgreement);
                                    db.Entry(PsAgreement).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(PsAgreement).State = System.Data.Entity.EntityState.Detached;

                                }

                                ts.Complete();
                                //  return this.Json(new Object[] { null, null, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data Created successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            } 
                            catch (DataException /* dex */)
                            {
                                //  return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

                    //    // return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                    //    // return this.Json(new { msg = errorMsg });
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

        public class SalHead_CD
        {
            public Array SalHead_Id { get; set; }
            public Array SalHead_FullDetails { get; set; }
            public int Payscaleagg_Id { get; set; }
            public string PayscaleagreementDetails { get; set; }
        }

        public class Payscalegreement_details
        {

            public int Payscaleagg_Id { get; set; }
            public string PayscaleagreementDetails { get; set; }

        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<SalHead_CD> return_data = new List<SalHead_CD>();
                var Grade = db.OfficiatingParameter
                    .Include(e => e.AllowanceList)
                    .Where(e => e.Id == data).ToList();
                var r = (from ca in Grade
                         select new
                         {
                             Id = ca.Id,
                             Name=ca.Name,
                             AllowanceAppl = ca.AllowanceAppl,
                             FixedAmount = ca.FixedAmount,
                             FixedAmountAppl = ca.FixedAmountAppl,
                             NewGradeBasicAppl = ca.NewGradeBasicAppl,
                             OfficiatingToSalary = ca.OfficiatingToSalary,
                             ScaleFirstBasic = ca.ScaleFirstBasic,
                             OfficiatingEmpPayStructAppl = ca.OfficiatingEmpPayStructAppl,
                             NewPayStructOnScreenAppl = ca.NewPayStructOnScreenAppl,
                             GradeShiftCount = ca.GradeShiftCount,
                             IncrementCount = ca.IncrementCount,
                             PayAmountuppergradediffAppl = ca.PayAmountuppergradediffAppl,
                             Action = ca.DBTrack.Action
                         }).Distinct();


                var a = db.OfficiatingParameter.Include(e => e.AllowanceList).Where(e => e.Id == data).Select(e => e.AllowanceList).ToList();

                foreach (var ca in a)
                {
                    return_data.Add(
                new SalHead_CD
                {
                    SalHead_Id = ca.Select(e => e.Id.ToString()).ToArray(),
                    SalHead_FullDetails = ca.Select(e => e.FullDetails).ToArray()
                });
                }

                var PSA = db.PayScaleAgreement.Include(e => e.OfficiatingParameter).Where(e => e.OfficiatingParameter.Any(t => t.Id == data)).ToList();
                List<Payscalegreement_details> ObjPayscaleAgg = new List<Payscalegreement_details>();
                if (PSA != null)
                {
                    foreach (var ca in PSA)
                    {
                        return_data.Add(
               new SalHead_CD
               {
                   Payscaleagg_Id = ca.Id,
                   PayscaleagreementDetails = ca.FullDetails
               }); 
                    } 
                }
               

                var Old_Data = db.DT_OfficiatingParameter
                 .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M")
                 .Select
                 (e => new
                 {
                     DT_Id = e.Id,
                     AllowanceAppl = e.AllowanceAppl,
                     FixedAmount = e.FixedAmount,
                     FixedAmountAppl = e.FixedAmountAppl,
                     NewGradeBasicAppl = e.NewGradeBasicAppl,
                     OfficiatingToSalary = e.OfficiatingToSalary,
                     ScaleFirstBasic = e.ScaleFirstBasic,
                   
                 }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Lkval = db.OfficiatingParameter.Find(data);
                TempData["RowVersion"] = Lkval.RowVersion;
                var Auth = Lkval.DBTrack.IsModified;

                return Json(new Object[] { r, return_data, Old_Data, Auth, JsonRequestBehavior.AllowGet }); 
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(OfficiatingParameter c, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string AllowanceList = form["AllowanceList"] == "0" ? "" : form["AllowanceList"]; 
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    var db_data = db.OfficiatingParameter.Include(e => e.AllowanceList).Where(e => e.Id == data).SingleOrDefault();
                    List<SalaryHead> SalHead = new List<SalaryHead>();

                    if (AllowanceList != null)
                    {
                        var ids = Utility.StringIdsToListIds(AllowanceList);
                        foreach (var ca in ids)
                        {
                            var SalHead_val = db.SalaryHead.Find(ca);
                            SalHead.Add(SalHead_val);
                            db_data.AllowanceList = SalHead;
                        }
                    }
                    else
                    {
                        db_data.AllowanceList = null;
                    }

                    db.OfficiatingParameter.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["RowVersion"] = db_data.RowVersion;
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
 

                    //if (Auth == false)
                    //{


                        //if (ModelState.IsValid)
                        //{
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    OfficiatingParameter blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.OfficiatingParameter.Where(e => e.Id == data).Include(e => e.AllowanceList) 
                                                                .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    var CurCorp = db.OfficiatingParameter.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        //c.DBTrack = dbT;
                                        OfficiatingParameter corp = new OfficiatingParameter()
                                        {
                                            AllowanceAppl = c.AllowanceAppl,
                                            Name=c.Name,
                                            Id = data,
                                            FixedAmount = c.FixedAmount,
                                            FixedAmountAppl = c.FixedAmountAppl,
                                            NewGradeBasicAppl = c.NewGradeBasicAppl,
                                            OfficiatingToSalary = c.OfficiatingToSalary,
                                            ScaleFirstBasic = c.ScaleFirstBasic,
                                            OfficiatingEmpPayStructAppl = c.OfficiatingEmpPayStructAppl,
                                            NewPayStructOnScreenAppl = c.NewPayStructOnScreenAppl,
                                            GradeShiftCount = c.GradeShiftCount,
                                            IncrementCount = c.IncrementCount,
                                            PayAmountuppergradediffAppl=c.PayAmountuppergradediffAppl,
                                            DBTrack = c.DBTrack
                                        };


                                        db.OfficiatingParameter.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;
                                    }

                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_OfficiatingParameter DT_Corp = (DT_OfficiatingParameter)obj;
                                        
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.AllowanceAppl.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (PromoActivity)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PromoActivity)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        //}
                    //}
                    //else
                    //{
                    //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //    {

                    //        OfficiatingParameter blog = null; // to retrieve old data
                    //        DbPropertyValues originalBlogValues = null;
                    //        OfficiatingParameter Old_Corp = null;

                    //        using (var context = new DataBaseContext())
                    //        {
                    //            blog = context.OfficiatingParameter.Where(e => e.Id == data).SingleOrDefault();
                    //            originalBlogValues = context.Entry(blog).OriginalValues;
                    //        }
                    //        c.DBTrack = new DBTrack
                    //        {
                    //            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                    //            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                    //            Action = "M",
                    //            IsModified = blog.DBTrack.IsModified == true ? true : false,s
                    //            ModifiedBy = SessionManager.UserName,
                    //            ModifiedOn = DateTime.Now
                    //        };
                    //        OfficiatingParameter corp = new OfficiatingParameter()
                    //        {
                    //            AllowanceAppl = c.AllowanceAppl,
                    //            Id = data,
                    //            FixedAmount = c.FixedAmount,
                    //            FixedAmountAppl = c.FixedAmountAppl,
                    //            NewGradeBasicAppl = c.NewGradeBasicAppl,
                    //            OfficiatingToSalary = c.OfficiatingToSalary,
                    //            ScaleFirstBasic = c.ScaleFirstBasic,
                    //            DBTrack = c.DBTrack
                    //        };

                    //        using (var context = new DataBaseContext())
                    //        {
                    //            var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "PromoActivity", c.DBTrack);

                    //            Old_Corp = context.OfficiatingParameter.Where(e => e.Id == data).Include(e => e.AllowanceList)
                    //                .SingleOrDefault();
                    //            DT_OfficiatingParameter DT_Corp = (DT_OfficiatingParameter)obj;
                             
                    //            db.Create(DT_Corp);
                    //        }
                    //        blog.DBTrack = c.DBTrack;
                    //        db.OfficiatingParameter.Attach(blog);
                    //        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                    //        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //        db.SaveChanges();
                    //        ts.Complete();
                    //        Msg.Add("  Record Updated");
                    //        return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.AllowanceAppl.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //        // return Json(new Object[] { blog.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                    //    }

                    //}
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    OfficiatingParameter OffParameter = db.OfficiatingParameter.Include(e => e.AllowanceList)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    if (OffParameter.AllowanceList != null && OffParameter.AllowanceList.Count() > 0)
                    {
                        Msg.Add(" Allowance list assigned So can't delete this record.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        try
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = OffParameter.DBTrack.CreatedBy != null ? OffParameter.DBTrack.CreatedBy : null,
                                CreatedOn = OffParameter.DBTrack.CreatedOn != null ? OffParameter.DBTrack.CreatedOn : null,
                                IsModified = OffParameter.DBTrack.IsModified == true ? false : false//,
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(OffParameter).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
                            DT_OfficiatingParameter DT_Corp = (DT_OfficiatingParameter)rtn_Obj;
                           
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
            }
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string FullDetails { get; set; }

        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData>OfficiatingParaList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    var Payscaleagreement = (String)null;
                    // Payscaleagreement = form["Payscaleagreementdetailslist"] == "0" ? "" : form["Payscaleagreementdetailslist"];
                    if (gp.filter != null)
                    {
                        Payscaleagreement = gp.filter;
                    }
                    int Id = Convert.ToInt32(Payscaleagreement);

                    var BindPayscaleagreementList = db.PayScaleAgreement.Include(e => e.OfficiatingParameter).Where(e => e.Id == Id).ToList();

                    foreach (var z in BindPayscaleagreementList)
                    {
                        if (z.OfficiatingParameter != null)
                        { 
                            foreach (var Promo in z.OfficiatingParameter)
                            { 
                                view = new P2BGridData()
                                {
                                    Id = Promo.Id,
                                    Name = Promo.Name,
                                    FullDetails = Promo.FullDetails,
                                };
                                model.Add(view);

                            }
                        }

                    }

                    OfficiatingParaList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = OfficiatingParaList;
                        if (gp.searchOper.Equals("eq"))
                        {

                            jsonData = IE.Where(e => (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.Name, a.Id }).ToList();
                          
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Name), a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = OfficiatingParaList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Name" ? c.FullDetails.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Name, a.Id }).ToList();
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
                        totalRecords = OfficiatingParaList.Count();
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
        }

        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<OfficiatingParameter> OfficiatingParameter = null;
        //        if (gp.IsAutho == true)
        //        {
        //            OfficiatingParameter = db.OfficiatingParameter.Where(e => e.DBTrack.IsModified == false).AsNoTracking().ToList();
        //        }
        //        else
        //        {

        //            OfficiatingParameter = db.OfficiatingParameter.AsNoTracking().ToList();
                       
        //        }

        //        IEnumerable<OfficiatingParameter> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = OfficiatingParameter;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Where(e => (e.AllowanceAppl.ToString().Contains(gp.searchString))
        //                       || (e.FixedAmountAppl.ToString().Contains(gp.searchString.ToUpper()))
        //                       || (e.NewGradeBasicAppl.ToString().Contains(gp.searchString.ToUpper()))
        //                       || (e.OfficiatingToSalary.ToString().Contains(gp.searchString.ToUpper()))
        //                       || (e.ScaleFirstBasic.ToString().Contains(gp.searchString.ToUpper()))
        //                       || (e.Id.ToString().Contains(gp.searchString.ToUpper()))
        //                       ).Select(a => new Object[] { a.AllowanceAppl, a.FixedAmountAppl,a.NewGradeBasicAppl,a.OfficiatingToSalary,a.ScaleFirstBasic, a.Id }).ToList(); 
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.AllowanceAppl, a.FixedAmountAppl, a.NewGradeBasicAppl, a.OfficiatingToSalary, a.ScaleFirstBasic, a.Id }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = OfficiatingParameter;
        //            Func<OfficiatingParameter, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "AllowanceAppl" ? c.AllowanceAppl.ToString() :
        //                                 gp.sidx == "FixedAmountAppl" ? c.FixedAmountAppl.ToString() :
        //                                 gp.sidx == "NewGradeBasicAppl" ? c.NewGradeBasicAppl.ToString()  :
        //                                 gp.sidx == "OfficiatingToSalary" ? c.OfficiatingToSalary.ToString() :
        //                                 gp.sidx == "ScaleFirstBasic" ? c.ScaleFirstBasic.ToString() : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.AllowanceAppl, a.FixedAmountAppl, a.NewGradeBasicAppl, a.OfficiatingToSalary, a.ScaleFirstBasic, a.Id }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.AllowanceAppl, a.FixedAmountAppl, a.NewGradeBasicAppl, a.OfficiatingToSalary, a.ScaleFirstBasic, a.Id }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.AllowanceAppl, a.FixedAmountAppl, a.NewGradeBasicAppl, a.OfficiatingToSalary, a.ScaleFirstBasic, a.Id }).ToList();
        //            }
        //            totalRecords = OfficiatingParameter.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        List<string> Msg = new List<string>();
        //        LogFile Logfile = new LogFile();
        //        ErrorLog Err = new ErrorLog()
        //        {
        //            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //            ExceptionMessage = ex.Message,
        //            ExceptionStackTrace = ex.StackTrace,
        //            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //            LogTime = DateTime.Now
        //        };
        //        Logfile.CreateLogFile(Err);
        //        Msg.Add(ex.Message);
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //    }
        //}
    }
}