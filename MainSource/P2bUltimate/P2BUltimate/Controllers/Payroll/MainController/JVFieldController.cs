using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class JVFieldController : Controller
    {
        //
        // GET: /JVField/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _JVFieldPartial()
        {
            return View("~/Views/Shared/Payroll/_JVField.cshtml");
        }

        [HttpPost]
        public ActionResult GetValueLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1081").FirstOrDefault().LookupValues;

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LookupValue.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }



                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public class returnEditClass
        {
            public Array Value_Id { get; set; }
            public Array ValueFullDetails { get; set; }
        }

        public ActionResult Create(JVField c, FormCollection form)
        {
            List<string> Msg = new List<string>();

            string Name = form["Name_drop"] == "0" ? "" : form["Name_drop"];
            string PaddingChar = form["PaddingChar_drop"] == "0" ? "" : form["PaddingChar_drop"];
            string Value = form["Value_drop"] == "0" ? "" : form["Value_drop"];
            string PaddingSide = form["PaddingSide_drop"] == "0" ? "" : form["PaddingSide_drop"];
            string ConcatData = form["ConcatData_drop"] == "0" ? "" : form["ConcatData_drop"];
            string ConcatDataPaddingSide = form["ConcatDataPaddingSide_drop"] == "0" ? "" : form["ConcatDataPaddingSide_drop"];
            string ConcatDataValue = form["ConcatDataValue_drop"] == "0" ? "" : form["ConcatDataValue_drop"];
           
            string SplitData = form["SplitData_drop"] == "0" ? "" : form["SplitData_drop"];
            string SplitDataPaddingSide = form["SplitDataPaddingSide_drop"] == "0" ? "" : form["SplitDataPaddingSide_drop"];
            string SplitDataValue = form["SplitDataValue_drop"] == "0" ? "" : form["SplitDataValue_drop"];
            string HeaderName = form["HeaderName"] == "" ? "" : form["HeaderName"];
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {

                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            if (Name != "" && Name != null)
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1079").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Name)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailTemplateFieldPlist));
                                c.Name = val;
                            }

                            if (PaddingChar != "" && PaddingChar != null)
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1080").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(PaddingChar)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailTemplateFieldPlist));
                                c.PaddingChar = val;
                            }

                            
                            if (Value != null && Value != "")
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1081").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Value)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailTemplateFieldPlist));
                                c.Value = val;

                               // var ids = Utility.StringIdsToListIds(Value);
                                //foreach (var ca in ids)
                                //{
                                //    var Value_val = db.LookupValue.Find(ca);
                                //    lookupval.Add(Value_val);
                                //    c.Value = lookupval;
                                //}
                            }

                            if (PaddingSide != "" && PaddingSide != null)
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1082").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(PaddingSide)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailTemplateFieldPlist));
                                c.PaddingSide = val;
                            }

                            if (ConcatData != "" && ConcatData != null)
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1083").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(ConcatData)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailTemplateFieldPlist));
                                c.ConcatData = val;
                            }

                            if (ConcatDataPaddingSide != "" && ConcatDataPaddingSide != null)
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1082").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(ConcatDataPaddingSide)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailTemplateFieldPlist));
                                c.ConcatDataPaddingSide = val;
                            }

                            if (ConcatDataValue != "" && ConcatDataValue != null)
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1081").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(ConcatDataValue)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailTemplateFieldPlist));
                                c.ConcatDataValue = val;
                            }

                            if (SplitData != "" && SplitData != null)
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1083").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(SplitData)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailTemplateFieldPlist));
                                c.SplitData = val;
                            }

                            if (SplitDataValue != "" && SplitDataValue != null)
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1081").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(SplitDataValue)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailTemplateFieldPlist));
                                c.SplitDataValue = val;
                            }

                            if (SplitDataPaddingSide != "" && SplitDataPaddingSide != null)
                            {
                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1082").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(SplitDataPaddingSide)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailTemplateFieldPlist));
                                c.SplitDataPaddingSide = val;
                            }


                            JVField JVField = new JVField()
                            {
                                Name = c.Name,
                                PaddingAppl = c.PaddingAppl,
                                PaddingChar = c.PaddingChar,
                                SeqNo = c.SeqNo,
                                Size = c.Size,
                                Value = c.Value,
                                PaddingSide = c.PaddingSide,
                                ConcatData = c.ConcatData,
                                SplitData = c.SplitData,
                                ConcatDataPaddingAppl = c.ConcatDataPaddingAppl,
                                ConcatDataPaddingSide = c.ConcatDataPaddingSide,
                                SplitDataPaddingAppl = c.SplitDataPaddingAppl,
                                SplitDataPaddingSide = c.SplitDataPaddingSide,
                                ConcatDataValue = c.ConcatDataValue,
                                SplitDataValue = c.SplitDataValue,
                                SkipSeperator = c.SkipSeperator,
                                HeaderAppl = c.HeaderAppl,
                                HeaderName=HeaderName,
                                DBTrack = c.DBTrack
                            };
                            try
                            {

                                db.JVField.Add(JVField);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);
                                DT_JVField DT_DeptObj = (DT_JVField)rtn_Obj;
                                db.Create(DT_DeptObj);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = JVField.Id, Val = JVField.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Utility.JsonReturnClass { Id = JVField.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { LocationObj.Id, null, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }

                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException)
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

        [HttpPost]
        public ActionResult Edit(int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.JVField
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Name_Id = e.Name_Id,
                        PaddingAppl = e.PaddingAppl,
                        PaddingChar_Id = e.PaddingChar_Id,
                        Value_Id = e.Value_Id,
                        SeqNo = e.SeqNo,
                        Size = e.Size,
                        PaddingSide_Id = e.PaddingSide_Id,
                        Action = e.DBTrack.Action,
                        ConcatData_Id = e.ConcatData_Id,
                        SplitData_Id = e.SplitData_Id,
                        ConcatDataPaddingAppl = e.ConcatDataPaddingAppl,
                        ConcatDataPaddingSide_Id = e.ConcatDataPaddingSide_Id,
                        ConcatDataValue_Id = e.ConcatDataValue_Id,
                        SplitDataPaddingAppl = e.SplitDataPaddingAppl,
                        SplitDataPaddingSide_Id = e.SplitDataPaddingSide_Id,
                        SplitDataValue_Id = e.SplitDataValue_Id,
                        ConcatDataSize = e.ConcatDataSize,
                        SplitDataSize = e.SplitDataSize,
                        SkipSeperator = e.SkipSeperator,
                        HeaderAppl = e.HeaderAppl,
                        HeaderName = e.HeaderName,
                        
                    }).ToList();

               // List<returnEditClass> oreturnEditClass = new List<returnEditClass>();

                //var k = db.JVField.Include(e => e.Value) 
                //         .Where(e => e.Id == data && e.Value.Count() > 0).ToList();
                //foreach (var e in k)
                //{
                //    oreturnEditClass.Add(new returnEditClass
                //    {

                //        Value_Id = e.Value.Select(a => a.Id.ToString()).ToArray(),
                //        ValueFullDetails = e.Value.Select(a => a.LookupVal).ToArray(),
                //    });
                //}


                var Corp = db.JVField.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        public async Task<ActionResult> EditSave(JVField ESOBJ, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();

            string Name = form["Name_drop"] == "0" ? "" : form["Name_drop"];
            string PaddingChar = form["PaddingChar_drop"] == "0" ? "" : form["PaddingChar_drop"];
            string Value = form["Value_drop"] == "0" ? "" : form["Value_drop"];
            string PaddingSide = form["PaddingSide_drop"] == "0" ? "" : form["PaddingSide_drop"];
            string ConcatData = form["ConcatData_drop"] == "0" ? "" : form["ConcatData_drop"];
            string SplitData = form["SplitData_drop"] == "0" ? "" : form["SplitData_drop"];
            string ConcatDataPaddingSide = form["ConcatDataPaddingSide_drop"] == "0" ? "" : form["ConcatDataPaddingSide_drop"];
            string SplitDataPaddingSide = form["SplitDataPaddingSide_drop"] == "0" ? "" : form["SplitDataPaddingSide_drop"];
            string ConcatDataValue = form["ConcatDataValue_drop"] == "0" ? "" : form["ConcatDataValue_drop"];
            string SplitDataValue = form["SplitDataValue_drop"] == "0" ? "" : form["SplitDataValue_drop"];
            string HeaderName = form["HeaderName"] == "" ? "" : form["HeaderName"];

            ESOBJ.Name_Id = Name != null && Name != "" ? int.Parse(Name) : 0;
            ESOBJ.PaddingChar_Id = PaddingChar != null && PaddingChar != "" ? int.Parse(PaddingChar) : 0;
            ESOBJ.Value_Id = Value != null && Value != "" ? int.Parse(Value) : 0;
            ESOBJ.PaddingSide_Id = PaddingSide != null && PaddingSide != "" ? int.Parse(PaddingSide) : 0;
            ESOBJ.ConcatData_Id = ConcatData != null && ConcatData != "" ? int.Parse(ConcatData) : 0;
            ESOBJ.SplitData_Id = SplitData != null && SplitData != "" ? int.Parse(SplitData) : 0;
            ESOBJ.ConcatDataPaddingSide_Id = ConcatDataPaddingSide != null && ConcatDataPaddingSide != "" ? int.Parse(ConcatDataPaddingSide) : 0;
            ESOBJ.ConcatDataValue_Id = ConcatDataValue != null && ConcatDataValue != "" ? int.Parse(ConcatDataValue) : 0;
            ESOBJ.SplitDataPaddingSide_Id = SplitDataPaddingSide != null && SplitDataPaddingSide != "" ? int.Parse(SplitDataPaddingSide) : 0;
            ESOBJ.SplitDataValue_Id = SplitDataValue != null && SplitDataValue != "" ? int.Parse(SplitDataValue) : 0;
            ESOBJ.HeaderName = HeaderName != null && HeaderName != "" ? HeaderName : "";

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //var db_data = db.JVField.Where(e => e.Id == data).FirstOrDefault();
                        //List<LookupValue> LvH = new List<LookupValue>();

                        //if (Value != null)
                        //{
                        //    var ids = Utility.StringIdsToListIds(Value);
                        //    foreach (var ca in ids)
                        //    {
                        //        //var LvHead_val = db.LookupValue.Find(ca);
                        //        //LvH.Add(LvHead_val);
                        //        //db_data.Value = LvH;
                        //    }
                        //}
                        //else
                        //{
                        //    db_data.Value = null;
                        //}


                        //db.JVField.Attach(db_data);
                        //db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                        //TempData["RowVersion"] = db_data.RowVersion;

                        JVField JVField = db.JVField.Find(data);
                        TempData["CurrRowVersion"] = JVField.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            ESOBJ.DBTrack = new DBTrack
                            {
                                CreatedBy = JVField.DBTrack.CreatedBy == null ? null : JVField.DBTrack.CreatedBy,
                                CreatedOn = JVField.DBTrack.CreatedOn == null ? null : JVField.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            JVField.PaddingAppl = ESOBJ.PaddingAppl;
                            JVField.SeqNo = ESOBJ.SeqNo;
                            JVField.Size = ESOBJ.Size;
                            JVField.Name_Id = ESOBJ.Name_Id;
                            JVField.ConcatDataSize = ESOBJ.ConcatDataSize;
                            JVField.SplitDataSize = ESOBJ.SplitDataSize;
                            JVField.SkipSeperator = ESOBJ.SkipSeperator;
                            JVField.HeaderAppl = ESOBJ.HeaderAppl;
                            JVField.HeaderName = ESOBJ.HeaderName;

                            if (ESOBJ.PaddingChar_Id != 0) // Padding Character
                            {
                                JVField.PaddingChar_Id = ESOBJ.PaddingChar_Id;
                            }
                            else
                            {
                                JVField.PaddingChar_Id = null;
                            }

                            if (ESOBJ.Value_Id != 0) // JVField Value
                            {
                                JVField.Value_Id = ESOBJ.Value_Id;
                            }
                            else
                            {
                                JVField.Value_Id = null;
                            }

                            if (ESOBJ.PaddingSide_Id != 0) // Padding Side
                            {
                                JVField.PaddingSide_Id = ESOBJ.PaddingSide_Id;
                            }
                            else
                            {
                                JVField.PaddingSide_Id = null; 
                            }

                            if (ESOBJ.ConcatData_Id != 0) // Concat Data
                            {
                                JVField.ConcatData_Id = ESOBJ.ConcatData_Id;
                            }
                            else
                            {
                                JVField.ConcatData_Id = null;
                            }

                            if (ESOBJ.ConcatDataPaddingSide_Id != 0) // Concat Data Padding Side
                            {
                                JVField.ConcatDataPaddingSide_Id = ESOBJ.ConcatDataPaddingSide_Id; 
                            }
                            else
                            {
                                JVField.ConcatDataPaddingSide_Id = null;
                            }

                            if (ESOBJ.ConcatDataValue_Id != 0) // Concat Data Value
                            {
                                JVField.ConcatDataValue_Id = ESOBJ.ConcatDataValue_Id;
                            }
                            else
                            {
                                JVField.ConcatDataValue_Id = null;
                            }

                            if (ESOBJ.SplitData_Id != 0) // Split Data 
                            {
                                JVField.SplitData_Id = ESOBJ.SplitData_Id;
                            }
                            else
                            {
                                JVField.SplitData_Id = null;
                            }

                            if (ESOBJ.SplitDataPaddingSide_Id != 0) // Split Data Padding Side
                            {
                                JVField.SplitDataPaddingSide_Id = ESOBJ.SplitDataPaddingSide_Id;
                            }
                            else
                            {
                                JVField.SplitDataPaddingSide_Id = null;
                            }

                            if (ESOBJ.SplitDataValue_Id != 0) // Split Data Value
                            {
                                JVField.SplitDataValue_Id = ESOBJ.SplitDataValue_Id;
                            }
                            else
                            {
                                JVField.SplitDataValue_Id = null;
                            }
                            JVField.ConcatDataPaddingAppl = ESOBJ.ConcatDataPaddingAppl;
                            JVField.SplitDataPaddingAppl = ESOBJ.SplitDataPaddingAppl;
                            JVField.DBTrack = ESOBJ.DBTrack;

                            db.JVField.Attach(JVField);
                            db.Entry(JVField).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        //}
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = ESOBJ.Id, Val = ESOBJ.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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