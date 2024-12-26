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
using P2BUltimate.Security;
using Payroll;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class ITForm24QFileFormatDefinitionController : Controller
    {
        private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ITForm24QFileFormatDefinition/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITForm24QFileFormatDefinition/Index.cshtml");
        }

        public ActionResult Create(ITForm24QFileFormatDefinition c, FormCollection form)
        {
            var FileType = form["FileTypelist"] == null ? "" : form["FileTypelist"];
            var DataType = form["DataTypelist"] == null ? "" : form["DataTypelist"];
            try
            {
                List<string> Msg = new List<string>();

                if (FileType != null)
                {
                    var val = db.LookupValue.Find(int.Parse(FileType));
                    c.Form24QFileType = val;
                }

                if (DataType != null)
                {
                    var val = db.LookupValue.Find(int.Parse(DataType));
                    c.DataType = val;
                }

               


                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        if (db.ITForm24QFileFormatDefinition.Any(o => o.SrNo == c.SrNo && o.Form24QFileType.LookupVal.ToUpper().ToString() == c.Form24QFileType.LookupVal.ToUpper().ToString()))
                        {
                            Msg.Add("  Sr No Already Exists.  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        

                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = true };

                        ITForm24QFileFormatDefinition Itform = new ITForm24QFileFormatDefinition()
                        {
                            SrNo = c.SrNo,
                            Form24QFileType  = c.Form24QFileType,
                            DataType = c.DataType,
                            ExcelColNo = c.ExcelColNo,
                            Field =c.Field,
                            InputType =c.InputType,
                            Narration =c.Narration,
                            Size = c.Size, 
                            DBTrack = c.DBTrack

                        };

                        db.ITForm24QFileFormatDefinition.Add(Itform);
                        var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);
                        db.SaveChanges();
                        ts.Complete();
                        Msg.Add("Data Created Successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                }
                else
                {
                    StringBuilder sb = new StringBuilder("");
                    foreach (ModelState modelstate in ModelState.Values)
                    {
                        foreach (var item in modelstate.Errors)
                        {
                            sb.Append(item.ErrorMessage);
                            sb.Append("." + "/n");
                        }
                    }
                    var errormsg = sb.ToString();
                    Msg.Add(errormsg);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult Edit(int data)
        {
            var itform = db.ITForm24QFileFormatDefinition.Include(a => a.Form24QFileType).Include(a => a.DataType).Where(a => a.Id == data).SingleOrDefault();

            var Q = db.ITForm24QFileFormatDefinition.Include(a => a.Form24QFileType)
                .Include(a => a.DataType)
                .Where(a => a.Id == data)
                .Select
                (e => new
                {
                    SrNo = e.SrNo,
                    Field = e.Field,
                    ExcelColNo = e.ExcelColNo,
                    InputType =  e.InputType,
                    Narration = e.Narration,
                    Size =e.Size, 
                    FileType_Id = e.Form24QFileType.Id == null ? 0 : e.Form24QFileType.Id,
                    DataType_Id = e.DataType.Id == null ? 0 : e.DataType.Id,
                    Action = e.DBTrack.Action
                }).ToList();



            var Corp = db.ITForm24QFileFormatDefinition.Find(data);
            TempData["RowVersion"] = Corp.RowVersion;
            var Auth = Corp.DBTrack.IsModified;
            return Json(new Object[] { Q, "", JsonRequestBehavior.AllowGet });

            // return View();
        }


        [HttpPost]
        //public async Task<ActionResult> EditSave(ITForm24QFileFormatDefinition FileFormat, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    try
        //    {
        //        bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //        var FileType = form["FileTypelist"] == null ? "" : form["FileTypelist"];
        //        var DataType = form["DataTypelist"] == null ? "" : form["DataTypelist"];


        //        if (FileType != null && FileType != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(FileType));
        //            FileFormat.Form24QFileType = val;
        //        }

        //        if (DataType != null && DataType != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(DataType));
        //            FileFormat.DataType  = val;
        //        }

            

        //        if (Auth == false)
        //        {

        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {

        //                    //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        ITForm24QFileFormatDefinition blog = null; // to retrieve old data
        //                        DbPropertyValues originalBlogValues = null;

        //                        using (var context = new DataBaseContext())
        //                        {
        //                            blog = context.ITForm24QFileFormatDefinition.Where(e => e.Id == data).Include(e => e.Form24QFileType).Include(e => e.DataType)
        //                                                  .SingleOrDefault();
        //                            originalBlogValues = context.Entry(blog).OriginalValues;
        //                        }

        //                        FileFormat.DBTrack = new DBTrack
        //                        {
        //                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                            Action = "M",
        //                            ModifiedBy = SessionManager.UserName,
        //                            ModifiedOn = DateTime.Now
        //                        };

        //                        int a = EditS(FileType, DataType, data, FileFormat, FileFormat.DBTrack);



        //                        using (var context = new DataBaseContext())
        //                        {
        //                            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                            //DT_Corporate DT_Corp = (DT_Corporate)obj;
        //                            //DT_Corp.Address_Id = blog.PerquisiteName == null ? 0 : blog.PerquisiteName.Id;
        //                            //DT_Corp.BusinessType_Id = blog.BusinessType == null ? 0 : blog.BusinessType.Id;
        //                            //DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
        //                            //db.Create(DT_Corp);
        //                            db.SaveChanges();
        //                        }
        //                        await db.SaveChangesAsync();
        //                        ts.Complete();


        //                        //   return Json(new Object[] { c.Id, c.EffectiveDate, "Record Updated", JsonRequestBehavior.AllowGet });
        //                        Msg.Add("  Record Updated");
        //                        return Json(new Utility.JsonReturnClass { Id = FileFormat.Id, Val = FileFormat.SrNo.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (ITForm12BACaptionMapping)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                        Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (ITForm12BACaptionMapping)databaseEntry.ToObject();
        //                        //   c.RowVersion = databaseValues.RowVersion;

        //                    }
        //                }

        //                //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //            }
        //        }
        //        else
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {

        //                ITForm24QFileFormatDefinition blog = null; // to retrieve old data
        //                DbPropertyValues originalBlogValues = null;
        //                ITForm24QFileFormatDefinition Old_Corp = null;

        //                using (var context = new DataBaseContext())
        //                {
        //                    blog = context.ITForm24QFileFormatDefinition.Where(e => e.Id == data).SingleOrDefault();
        //                    originalBlogValues = context.Entry(blog).OriginalValues;
        //                }
        //                //c.DBTrack = new DBTrack
        //                //{
        //                //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                //    Action = "M",
        //                //    IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                //    ModifiedBy = SessionManager.UserName,
        //                //    ModifiedOn = DateTime.Now
        //                //};

        //                //if (TempData["RowVersion"] == null)
        //                //{
        //                //    TempData["RowVersion"] = blog.RowVersion;
        //                //}

        //                ITForm24QFileFormatDefinition corp = new ITForm24QFileFormatDefinition()
        //                {
        //                    SrNo = FileFormat.SrNo,
        //                    Id = data,
        //                    //   DBTrack = c.DBTrack,
        //                    //    RowVersion = (Byte[])TempData["RowVersion"]
        //                };


        //                //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                using (var context = new DataBaseContext())
        //                {
        //                    //var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "ITForm12BACaptionMapping", c.DBTrack);
        //                    //// var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                    //Old_Corp = context.ITForm12BACaptionMapping.Where(e => e.Id == data).Include(e => e.BusinessType)
        //                    //    .Include(e => e.PerquisiteName).Include(e => e.ContactDetails).SingleOrDefault();
        //                    //DT_Corporate DT_Corp = (DT_Corporate)obj;
        //                    //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.PerquisiteName, c.PerquisiteName);//Old_Corp.PerquisiteName == c.PerquisiteName ? 0 : Old_Corp.PerquisiteName == null && c.PerquisiteName != null ? c.PerquisiteName.Id : Old_Corp.PerquisiteName.Id;
        //                    //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
        //                    //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
        //                    //db.Create(DT_Corp);
        //                    //db.SaveChanges();
        //                }
        //                //blog.DBTrack = c.DBTrack;
        //                //db.ITForm12BACaptionMapping.Attach(blog);
        //                db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                //   db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                db.SaveChanges();
        //                ts.Complete();
        //                // return Json(new Object[] { blog.Id, c.EffectiveDate, "Record Updated", JsonRequestBehavior.AllowGet });
        //                Msg.Add("  Record Updated");
        //                return Json(new Utility.JsonReturnClass { Id = FileFormat.Id, Val = FileFormat.SrNo.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //            }

        //        }
        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
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
        public async Task<ActionResult> EditSave(ITForm24QFileFormatDefinition c, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            bool Auth = form["Autho_Allow"] == "true" ? true : false;
            var FileType = form["FileTypelist"] == null ? "" : form["FileTypelist"];
            var DataType = form["DataTypelist"] == null ? "" : form["DataTypelist"];

            c.Form24QFileType_Id = FileType != null && FileType != "" ? int.Parse(FileType) : 0;
            c.DataType_Id = DataType != null && DataType != "" ? int.Parse(DataType) : 0;

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.ITForm24QFileFormatDefinition.Include(e =>e.Form24QFileType).Include(e =>e.DataType).Where(e => e.Id == data).SingleOrDefault();
                       
                        if (c.Form24QFileType_Id == 0)
                        {
                            db_data.Form24QFileType = null;
                        }

                        if (c.DataType_Id == 0)
                        {
                            db_data.DataType = null;
                        }

                        db.ITForm24QFileFormatDefinition.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = db_data.RowVersion;

                        ITForm24QFileFormatDefinition itform24filedefinition = db.ITForm24QFileFormatDefinition.Find(data);
                        TempData["CurrRowVersion"] = itform24filedefinition.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            ITForm24QFileFormatDefinition blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = itform24filedefinition.DBTrack.CreatedBy == null ? null : itform24filedefinition.DBTrack.CreatedBy,
                                CreatedOn = itform24filedefinition.DBTrack.CreatedOn == null ? null : itform24filedefinition.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (c.Form24QFileType_Id != 0)
                                itform24filedefinition.Form24QFileType_Id = c.Form24QFileType_Id != null ? c.Form24QFileType_Id : 0;
                            if (c.DataType_Id != 0)
                                itform24filedefinition.DataType_Id = c.DataType_Id != null ? c.DataType_Id : 0;

                            itform24filedefinition.Id = data;
                            itform24filedefinition.SrNo = c.SrNo;
                            itform24filedefinition.Field =c.Field;
                            itform24filedefinition.ExcelColNo =c.ExcelColNo;
                            itform24filedefinition.InputType =c.InputType;
                            itform24filedefinition.Narration =c.Narration;
                            itform24filedefinition.Size =c.Size;
                  
                            itform24filedefinition.DBTrack = c.DBTrack;

                            db.Entry(itform24filedefinition).State = System.Data.Entity.EntityState.Modified;


                            //using (var context = new DataBaseContext())
                            //{


                            blog = db.ITForm24QFileFormatDefinition.Where(e => e.Id == data).Include(e => e.Form24QFileType)
                                                    .Include(e => e.DataType)
                                                    .SingleOrDefault();
                            originalBlogValues = db.Entry(blog).OriginalValues;
                            db.ChangeTracker.DetectChanges();
                            var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            DT_ITForm24QFileFormatDefinition DT_Corp = (DT_ITForm24QFileFormatDefinition)obj;

                            DT_Corp.Form24QFileType_Id = blog.Form24QFileType == null ? 0 : blog.Form24QFileType.Id;
                            DT_Corp.DataType_Id = blog.DataType == null ? 0 : blog.DataType.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();
                        }
                        //}
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val =c.ExcelColNo, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public int EditS(string FileType, string DataType, int data, ITForm24QFileFormatDefinition c, DBTrack dbT)
        {
            ITForm24QFileFormatDefinition typedetails = null;
            if (FileType != null & FileType != "")
            {
                var val = db.LookupValue.Find(int.Parse(FileType));
                c.Form24QFileType = val;

                var type = db.ITForm24QFileFormatDefinition.Include(e => e.Form24QFileType).Where(e => e.Id == data).SingleOrDefault();

                if (type.Form24QFileType != null)
                {
                    typedetails = db.ITForm24QFileFormatDefinition.Where(x => x.Form24QFileType.Id == type.Form24QFileType.Id && x.Id == data).SingleOrDefault();
                }
                else
                {
                    typedetails = db.ITForm24QFileFormatDefinition.Where(x => x.Id == data).SingleOrDefault();
                }
                typedetails.Form24QFileType = c.Form24QFileType;
            }
            else
            {
                typedetails = db.ITForm24QFileFormatDefinition.Include(e => e.Form24QFileType).Where(x => x.Id == data).SingleOrDefault();
                typedetails.Form24QFileType = null;
            }

            if (DataType != null & DataType != "")
            {
                var val = db.LookupValue.Find(int.Parse(DataType));
                c.DataType = val;

                var type = db.ITForm24QFileFormatDefinition.Include(e => e.DataType).Where(e => e.Id == data).SingleOrDefault();

                if (type.DataType != null)
                {
                    typedetails = db.ITForm24QFileFormatDefinition.Where(x => x.DataType.Id == type.DataType.Id && x.Id == data).SingleOrDefault();
                }
                else
                {
                    typedetails = db.ITForm24QFileFormatDefinition.Where(x => x.Id == data).SingleOrDefault();
                }
                typedetails.DataType = c.DataType;
            }
            else
            {
                typedetails = db.ITForm24QFileFormatDefinition.Include(e => e.DataType).Where(x => x.Id == data).SingleOrDefault();
                typedetails.DataType = null;
            }


            db.ITForm24QFileFormatDefinition.Attach(typedetails);
            db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            TempData["RowVersion"] = typedetails.RowVersion;
            db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;



                var CurCorp = db.ITForm24QFileFormatDefinition.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    ITForm24QFileFormatDefinition form24 = new ITForm24QFileFormatDefinition()
                    {
                        SrNo = c.SrNo,
                        Id = data,
                        DBTrack = c.DBTrack,
                        ExcelColNo = c.ExcelColNo,
                        Field = c.Field, 
                        InputType = c.InputType,
                        Narration = c.Narration,
                        Size = c.Size 
                    };


                    db.ITForm24QFileFormatDefinition.Attach(form24);
                    db.Entry(form24).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(form24).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0; 
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
                IEnumerable<ITForm24QFileFormatDefinition> ITForm24QFileFormatDefinition = null;
                if (gp.IsAutho == true)
                {
                    ITForm24QFileFormatDefinition = db.ITForm24QFileFormatDefinition.Include(e => e.DataType).Include(e => e.Form24QFileType).ToList();
                }
                else
                {
                    ITForm24QFileFormatDefinition = db.ITForm24QFileFormatDefinition.Include(e => e.DataType).Include(e => e.Form24QFileType).AsNoTracking().ToList();
                }

                IEnumerable<ITForm24QFileFormatDefinition> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ITForm24QFileFormatDefinition;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.SrNo.ToString().Contains(gp.searchString))
                            || (e.Form24QFileType.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.Field.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.ExcelColNo.ToString().Contains(gp.searchString))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.SrNo, a.Form24QFileType.LookupVal, a.Field, a.ExcelColNo, a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.SrNo.ToString(), a.Form24QFileType != null ? Convert.ToString(a.Form24QFileType.LookupVal) : "", a.Field, a.ExcelColNo, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ITForm24QFileFormatDefinition;
                    Func<ITForm24QFileFormatDefinition, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "SrNo" ? c.SrNo.ToString() :
                                         gp.sidx == "Form24QFileType" ? c.Form24QFileType.LookupVal.ToString() :
                                         gp.sidx == "Field" ? c.Field.ToString() :
                                         gp.sidx == "ExcelColNo" ? c.ExcelColNo.ToString() :
                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(e=>e.Form24QFileType_Id).ThenBy(x=>x.SrNo);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.SrNo), a.Form24QFileType != null ? Convert.ToString(a.Form24QFileType.LookupVal) : "", a.Field, a.ExcelColNo, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.SrNo), a.Form24QFileType != null ? Convert.ToString(a.Form24QFileType.LookupVal) : "", a.Field, a.ExcelColNo, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.SrNo), a.Form24QFileType != null ? Convert.ToString(a.Form24QFileType.LookupVal) : "", a.Field, a.ExcelColNo, a.Id }).ToList();
                    }
                    totalRecords = ITForm24QFileFormatDefinition.Count();
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


        [HttpPost]
        public async Task<ActionResult> Delete(int? data)
        {
            List<string> Msg = new List<string>();
            try
            {

                var Form24Q = db.ITForm24QFileFormatDefinition.Include(a => a.Form24QFileType).Include(a => a.DataType).Where(a => a.Id == data).SingleOrDefault();

                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {

                    db.ITForm24QFileFormatDefinition.Remove(Form24Q);
                    db.SaveChanges();
                    ts.Complete();
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
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
}