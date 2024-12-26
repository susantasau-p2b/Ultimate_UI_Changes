using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IR;
using P2b.Global;
using System.Data.Entity.Infrastructure;
using System.Text;
using System.Transactions;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using P2BUltimate.App_Start;
using System.Threading.Tasks;
using P2BUltimate.Security;
//using System.Web.Script.Serialization;
using Payroll;


using System.ComponentModel.DataAnnotations;

namespace P2BUltimate.Controllers.IR.MainController
{
    public class OffenseObjectController : Controller
    {
        //
        // GET: /OffenseObject/
        public ActionResult Index()
        {
            return View("~/views/IR/MainViews/OffenseObject/Index.cshtml");
        }

        public List<string> ValidateObj(Object obj)
        {
            var errorList = new List<String>();
            var context = new ValidationContext(obj, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(obj, context, results, true);
            if (!isValid)
            {
                foreach (var validationResult in results)
                {
                    errorList.Add(validationResult.ErrorMessage);
                }
                return errorList;
            }
            else
            {
                return errorList;
            }
        }

        [HttpPost]
        public ActionResult create(OffenseObject c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {

                try
                {
                    int comp_Id = 0;
                    comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Company = new Company();
                    Company = db.Company.Where(e => e.Id == comp_Id).SingleOrDefault();

                    string OffenceName = form["OffenceNameList"] == "" ? null : form["OffenceNameList"];

                    if (OffenceName != null && OffenceName != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(OffenceName));
                        c.OffenceName = val;

                    }
                    string OffenceType = form["OffenceTypeList"] == "" ? null : form["OffenceTypeList"];

                    if (OffenceType != null && OffenceType != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(OffenceType));
                        c.OffenceType = val;

                    }
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            OffenseObject OffenseObject = new OffenseObject()
                            {
                                DischargeOffencesCount = c.DischargeOffencesCount,
                                OffenceName=c.OffenceName,
                                OffenceType=c.OffenceType,                               
                                DBTrack = c.DBTrack
                            };
                            var OffenseObjectValidation = ValidateObj(OffenseObject);
                            if (OffenseObjectValidation.Count > 0)
                            {
                                foreach (var item in OffenseObjectValidation)
                                {

                                    Msg.Add("OffenseObject" + item);
                                }
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            db.OffenseObject.Add(OffenseObject);
                            db.SaveChanges();
                           
                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
              var ModuleName = System.Web.HttpContext.Current.Session["ModuleType"];
              var OffenseObject = new List<OffenseObject>();
              OffenseObject = db.OffenseObject                                   
                                     .Include(e => e.OffenceType)
                                    .Include(e => e.OffenceName)
                                    .ToList();
              IEnumerable<OffenseObject> IE;
              if (!string.IsNullOrEmpty(gp.searchField))
              {
                  IE = OffenseObject;
                  if (gp.searchOper.Equals("eq"))
                  {
                      jsonData = IE
                       .Where((e => (e.OffenceName.LookupVal.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.OffenceType.LookupVal.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.DischargeOffencesCount.ToString().ToUpper().Contains(gp.searchString))
                                || (e.Id.ToString().Contains(gp.searchString))
                               )).ToList()
                      .Select(a => new Object[] { a.OffenceName.LookupVal.ToString(), a.OffenceType.LookupVal.ToString(), a.DischargeOffencesCount, a.Id });
                  }
                  if (pageIndex > 1)
                  {
                      int h = pageIndex * pageSize;
                      jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.OffenceType.LookupVal.ToString(), a.OffenceName.LookupVal.ToString(), a.DischargeOffencesCount, a.Id }).ToList();
                  }
                  totalRecords = IE.Count();
                  

              }
              else
              {
                  IE = OffenseObject;
                  Func<OffenseObject, dynamic> orderfuc;
                  if (gp.sidx == "Id")
                  {
                      orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                  }
                  else
                  {
                      orderfuc = (c => gp.sidx == "OffenceName" ? c.OffenceName.LookupVal.ToString() :
                                        gp.sidx == "OffenceType" ? c.OffenceType.LookupVal.ToString() :
                                         gp.sidx == "DischargeOffencesCount" ? c.DischargeOffencesCount.ToString() : "");

                  }
                  if (gp.sord == "asc")
                  {
                      IE = IE.OrderBy(orderfuc);
                      jsonData = IE.Select(a => new Object[] { a.OffenceName.LookupVal.ToString(), a.OffenceType.LookupVal.ToString(), a.DischargeOffencesCount, a.Id }).ToList();
                  }
                  else if (gp.sord == "desc")
                  {
                      IE = IE.OrderByDescending(orderfuc);
                      jsonData = IE.Select(a => new Object[] { a.OffenceName.LookupVal.ToString(), a.OffenceType.LookupVal.ToString(), a.DischargeOffencesCount, a.Id }).ToList();
                  }
                  if (pageIndex > 1)
                  {
                      int h = pageIndex * pageSize;
                      jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.OffenceName.LookupVal.ToString(), a.OffenceType.LookupVal.ToString(), a.DischargeOffencesCount, a.Id }).ToList();
                  }
                  totalRecords = OffenseObject.Count();
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

      public ActionResult Edit(int data)
      {
          using (DataBaseContext db = new DataBaseContext())
          {
              var returndata = db.OffenseObject
                               .Include(e => e.OffenceName)
                               .Include(e => e.OffenceType)
                                .Where(e => e.Id == data).AsEnumerable()
                  .Select(e => new
                  {
                      DischargeOffencesCount = e.DischargeOffencesCount,
                      OffenceName = e.OffenceName.Id,
                      OffenceType = e.OffenceType.Id,
                  }).ToList();
              return Json(new Object[] { returndata, "", "", "", JsonRequestBehavior.AllowGet });
          }
      }
      [HttpPost]
      public async Task<ActionResult> EditSave(OffenseObject c, int data, FormCollection form) // Edit submit
      {
          using (DataBaseContext db = new DataBaseContext())
          {
              List<string> Msg = new List<string>();
              try
              {
                  string OffenceName = form["OffenceNameList"] == "" ? null : form["OffenceNameList"];
                  string OffenceType = form["OffenceTypeList"] == "" ? null : form["OffenceTypeList"];
                  bool Auth = form["Autho_Allow"] == "true" ? true : false;

                  if (OffenceName != null)
                  {
                      if (OffenceName != "")
                      {
                          var val = db.LookupValue.Find(int.Parse(OffenceName));
                          c.OffenceName = val;

                          var type = db.OffenseObject.Include(e => e.OffenceName).Where(e => e.Id == data).SingleOrDefault();
                          IList<OffenseObject> typedetails = null;
                          if (type.OffenceName != null)
                          {
                              typedetails = db.OffenseObject.Where(x => x.OffenceName.Id == type.OffenceName.Id && x.Id == data).ToList();
                          }
                          else
                          {
                              typedetails = db.OffenseObject.Where(x => x.Id == data).ToList();
                          }

                          foreach (var s in typedetails)
                          {
                              s.OffenceName = c.OffenceName;
                              db.OffenseObject.Attach(s);
                              db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                              db.SaveChanges();
                              TempData["RowVersion"] = s.RowVersion;
                              db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                          }
                      }
                      else
                      {
                          var PunishmentTypeDetails = db.OffenseObject.Include(e => e.OffenceName).Where(x => x.Id == data).ToList();
                          foreach (var s in PunishmentTypeDetails)
                          {
                              s.OffenceName = null;
                              db.OffenseObject.Attach(s);
                              db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                              db.SaveChanges();
                              TempData["RowVersion"] = s.RowVersion;
                              db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                          }
                      }
                  }

                  if (OffenceType != null)
                  {
                      if (OffenceType != "")
                      {
                          var val = db.LookupValue.Find(int.Parse(OffenceType));
                          c.OffenceType = val;

                          var type = db.OffenseObject.Include(e => e.OffenceType).Where(e => e.Id == data).SingleOrDefault();
                          IList<OffenseObject> typedetails = null;
                          if (type.OffenceType != null)
                          {
                              typedetails = db.OffenseObject.Where(x => x.OffenceType.Id == type.OffenceType.Id && x.Id == data).ToList();
                          }
                          else
                          {
                              typedetails = db.OffenseObject.Where(x => x.Id == data).ToList();
                          }

                          foreach (var s in typedetails)
                          {
                              s.OffenceType = c.OffenceType;
                              db.OffenseObject.Attach(s);
                              db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                              db.SaveChanges();
                              TempData["RowVersion"] = s.RowVersion;
                              db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                          }
                      }
                      else
                      {
                          var PenaltyTypeDetails = db.OffenseObject.Include(e => e.OffenceType).Where(x => x.Id == data).ToList();
                          foreach (var s in PenaltyTypeDetails)
                          {
                              s.OffenceType = null;
                              db.OffenseObject.Attach(s);
                              db.Entry(s).State = System.Data.Entity.EntityState.Modified;

                              db.SaveChanges();
                              TempData["RowVersion"] = s.RowVersion;
                              db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                          }
                      }
                  }                 

                  if (Auth == false)
                  {
                      if (ModelState.IsValid)
                      {
                          try
                          {
                              //DbContextTransaction transaction = db.Database.BeginTransaction();

                              using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                              {
                                  OffenseObject blog = null; // to retrieve old data
                                  DbPropertyValues originalBlogValues = null;

                                  using (var context = new DataBaseContext())
                                  {
                                      blog = context.OffenseObject.Where(e => e.Id == data).SingleOrDefault();


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


                                  var CurCorp = db.OffenseObject.Find(data);
                                  TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                  db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                  if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                  {
                                      // c.DBTrack = dbT;
                                      OffenseObject corp = new OffenseObject()
                                      {

                                          DischargeOffencesCount = c.DischargeOffencesCount,
                                          OffenceName = c.OffenceName,
                                          OffenceType = c.OffenceType, 
                                          Id = data,
                                          DBTrack = c.DBTrack

                                      };


                                      db.OffenseObject.Attach(corp);
                                      db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                      db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];



                                      await db.SaveChangesAsync();
                                      ts.Complete();
                                      Msg.Add("  Record Updated  ");
                                      //  return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.LocationObj.LocDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                      return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                  }


                              }
                          }
                          catch (DbUpdateConcurrencyException ex)
                          {
                              var entry = ex.Entries.Single();
                              var clientValues = (OffenseObject)entry.Entity;
                              var databaseEntry = entry.GetDatabaseValues();
                              if (databaseEntry == null)
                              {
                                  Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                  //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                              }
                              else
                              {
                                  var databaseValues = (OffenseObject)databaseEntry.ToObject();
                                  c.RowVersion = databaseValues.RowVersion;

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
                          Msg.Add("Record modified by another user.So refresh it and try to save again.");
                          return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                          // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                      }
                  }
                  else
                  {
                      using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                      {

                          OffenseObject blog = null; // to retrieve old data
                          DbPropertyValues originalBlogValues = null;


                          using (var context = new DataBaseContext())
                          {
                              blog = context.OffenseObject.Where(e => e.Id == data).SingleOrDefault();
                              originalBlogValues = context.Entry(blog).OriginalValues;
                          }
                          c.DBTrack = new DBTrack
                          {
                              CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                              CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                              Action = "M",
                              IsModified = blog.DBTrack.IsModified == true ? true : false,
                              ModifiedBy = SessionManager.UserName,
                              ModifiedOn = DateTime.Now
                          };
                          OffenseObject corp = new OffenseObject()
                          {

                             
                                          DischargeOffencesCount = c.DischargeOffencesCount,
                                          OffenceName = c.OffenceName,
                                          OffenceType = c.OffenceType, 
                                          Id = data,
                                          DBTrack = c.DBTrack,
                                          RowVersion = (Byte[])TempData["RowVersion"]
                          };



                          blog.DBTrack = c.DBTrack;
                          db.OffenseObject.Attach(blog);
                          db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                          db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                          db.SaveChanges();
                          ts.Complete();
                          Msg.Add("  Record Updated  ");
                          return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                          //return Json(new Object[] { blog.Id, c.LocationObj.LocDesc, "Record Updated", JsonRequestBehavior.AllowGet });
                      }

                  }
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
      public ActionResult View(int data)
      {
          using (DataBaseContext db = new DataBaseContext())
          {
              var returndata = db.OffenseObject
                               .Include(e => e.OffenceName)
                               .Include(e => e.OffenceType)
                                .Where(e => e.Id == data).AsEnumerable()
                  .Select(e => new
                  {
                      Name = e.DischargeOffencesCount,
                      OffenceName = e.OffenceName.Id,
                      OffenceType = e.OffenceType.Id,
                  }).ToList();
              return Json(new Object[] { returndata, "", "", "", JsonRequestBehavior.AllowGet });
          }
      }
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    OffenseObject corporates = db.OffenseObject
                                                             .Include(e => e.OffenceName)
                                                             .Include(e=>e.OffenceType)
                                                       .Where(e => e.Id == data).SingleOrDefault();


                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };

                            await db.SaveChangesAsync();

                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {



                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? false : false//,

                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;

                            await db.SaveChangesAsync();

                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {

                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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
        

      
	
