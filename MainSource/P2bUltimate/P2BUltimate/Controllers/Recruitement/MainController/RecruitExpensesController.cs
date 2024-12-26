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
using System.Threading.Tasks;
using P2BUltimate.Security;
using Recruitment;


namespace P2BUltimate.Controllers.recruitment.MainController
{
    public class RecruitExpensesController : Controller
    {

     //   private DataBaseContext db = new DataBaseContext();

      public ActionResult Index()
        {
            return View("~/Views/Shared/Recruitement/_RecruitExpenses.cshtml");
        }

      public ActionResult Create1(RecruitExpenses R ,FormCollection form)
      { 
          List<string> Msg = new List<string>();
          using (DataBaseContext db = new DataBaseContext())
          {
              try
              {
                  string ExpenseAccount = form["ExpAccListP"] == "0" ? null : form["ExpAccListP"];
                  string SourceOfExpense = form["SourceOFExpListP"] == "0" ? null : form["SourceOFExpListP"];

                  if (ExpenseAccount != null && ExpenseAccount != "")
                  {
                      var val = db.LookupValue.Find(int.Parse(ExpenseAccount));
                      R.ExpenseAccount = val;
                  }

                  if (SourceOfExpense != null && SourceOfExpense != "")
                  {
                      var val = db.LookupValue.Find(int.Parse(SourceOfExpense));
                      R.SourceOfExpense = val;
                  }

                  R.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                  RecruitExpenses rep = new RecruitExpenses
                  {
                      ExpenseAccount = R.ExpenseAccount,
                      FeeAmount = R.FeeAmount != null ? R.FeeAmount : 0,
                      Narration = R.Narration != null ? R.Narration : "",
                      SourceOfExpense = R.SourceOfExpense,
                      DBTrack = R.DBTrack,
                  };
                  try
                  {
                      if (ModelState.IsValid)
                      {
                          using (TransactionScope ts = new TransactionScope())
                          {
                              db.RecruitExpenses.Add(rep);
                              db.SaveChanges();
                              ts.Complete();
                          }
                      }
                      Msg.Add("  Data Created successfully  ");
                      return Json(new Utility.JsonReturnClass { Id = R.Id, Val = R.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                  }
                  catch (DataException e) { throw e; }
                  catch (DBConcurrencyException e) { throw e; }

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
      // [ValidateAntiForgeryToken]
      public ActionResult Create(RecruitExpenses R, FormCollection form)
      {
          using (DataBaseContext db = new DataBaseContext())
          {
              List<string> Msg = new List<string>();
              try
              {
                  string ExpenseAccount = form["ExpAccListP"] == "0" ? null : form["ExpAccListP"];
                  string SourceOfExpense = form["SourceOFExpListP"] == "0" ? null : form["SourceOFExpListP"];

                  if (ExpenseAccount != null && ExpenseAccount != "")
                  {
                      var val = db.LookupValue.Find(int.Parse(ExpenseAccount));
                      R.ExpenseAccount = val;
                  }

                  if (SourceOfExpense != null && SourceOfExpense != "")
                  {
                      var val = db.LookupValue.Find(int.Parse(SourceOfExpense));
                      R.SourceOfExpense = val;
                  }

                  if (ModelState.IsValid)
                  {
                      using (TransactionScope ts = new TransactionScope())
                      {



                          R.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };

                          RecruitExpenses rep = new RecruitExpenses
                          {
                              ExpenseAccount = R.ExpenseAccount,
                              FeeAmount = R.FeeAmount != null ? R.FeeAmount : 0,
                              Narration = R.Narration != null ? R.Narration : "",
                              SourceOfExpense = R.SourceOfExpense,
                              DBTrack = R.DBTrack,
                          };
                          try
                          {
                              db.RecruitExpenses.Add(rep);
                              // DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, R.DBTrack);
                              //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", Allergy, null, "Allergy", null);
                              db.SaveChanges();
                              ts.Complete();
                              //  return Json(new Object[] { Allergy.Id, Allergy.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                              Msg.Add("  Data Created successfully  ");
                              return Json(new Utility.JsonReturnClass { Id = rep.Id, Val = rep.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                          }

                          catch (DbUpdateConcurrencyException)
                          {
                              return RedirectToAction("Create", new { concurrencyError = true, id = R.Id });
                          }
                          catch (DataException /* dex */)
                          {
                              //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                              Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
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
          List<string> Msg = new List<string>();
          using (DataBaseContext db = new DataBaseContext())
          {
              try
              {

                  RecruitExpenses Addrs = db.RecruitExpenses
                      .Include(e => e.ExpenseAccount)
                      .Include(e => e.SourceOfExpense)
                      .Where(e => e.Id == data).SingleOrDefault();



                  if (Addrs.DBTrack.IsModified == true)
                  {
                      using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                      {
                          //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                          try
                          {
                              DBTrack dbT = new DBTrack
                              {
                                  Action = "D",
                                  CreatedBy = Addrs.DBTrack.CreatedBy != null ? Addrs.DBTrack.CreatedBy : null,
                                  CreatedOn = Addrs.DBTrack.CreatedOn != null ? Addrs.DBTrack.CreatedOn : null,
                                  IsModified = Addrs.DBTrack.IsModified == true ? true : false
                              };
                              Addrs.DBTrack = dbT;
                              db.Entry(Addrs).State = System.Data.Entity.EntityState.Modified;
                              var rtn_Obj = DBTrackFile.DBTrackSave("recruitment/recruitment ", null, db.ChangeTracker, Addrs.DBTrack);
                              DT_RecruitExpenses DT_Addrs = (DT_RecruitExpenses)rtn_Obj;

                              DT_Addrs.ExpenseAccount_Id = Addrs.ExpenseAccount == null ? 0 : Addrs.ExpenseAccount.Id;
                              DT_Addrs.SourceOfExpense_Id = Addrs.SourceOfExpense == null ? 0 : Addrs.SourceOfExpense.Id;
                              db.Create(DT_Addrs);
                              await db.SaveChangesAsync();
                              ts.Complete();
                              Msg.Add("  Data removed successfully.  ");
                              return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                          }
                          catch (Exception ex)
                          {
                              throw ex;
                          }
                      }
                  }
                  else
                  {
                      using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                      {
                          try
                          {
                              DBTrack dbT = new DBTrack
                              {
                                  Action = "D",
                                  ModifiedBy = SessionManager.UserName,
                                  ModifiedOn = DateTime.Now,
                                  CreatedBy = Addrs.DBTrack.CreatedBy != null ? Addrs.DBTrack.CreatedBy : null,
                                  CreatedOn = Addrs.DBTrack.CreatedOn != null ? Addrs.DBTrack.CreatedOn : null,
                                  IsModified = Addrs.DBTrack.IsModified == true ? false : false//,
                                  //AuthorizedBy = SessionManager.UserName,
                                  //AuthorizedOn = DateTime.Now
                              };

                              // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                              db.Entry(Addrs).State = System.Data.Entity.EntityState.Deleted;
                              var rtn_Obj = DBTrackFile.DBTrackSave("recruitment/recruitment ", null, db.ChangeTracker, dbT);
                              DT_RecruitExpenses DT_Addrs = (DT_RecruitExpenses)rtn_Obj;
                              DT_Addrs.ExpenseAccount_Id = Addrs.ExpenseAccount == null ? 0 : Addrs.ExpenseAccount.Id;
                              DT_Addrs.SourceOfExpense_Id = Addrs.SourceOfExpense == null ? 0 : Addrs.SourceOfExpense.Id;
                              db.Create(DT_Addrs);
                              ts.Complete();
                              await db.SaveChangesAsync();
                              ts.Complete();
                              Msg.Add("  Data removed successfully.  ");
                              return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                          }
                          catch (RetryLimitExceededException /* dex */)
                          {
                              Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                              return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                          }
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

      public ActionResult Edit(int data)
      {
          using (DataBaseContext db = new DataBaseContext())
          {
              var r = (from ca in db.RecruitExpenses
                       .Where(e => e.Id == data)
                       select new
                       {
                           Id = ca.Id,
                           ExpenseAccount_Id = ca.ExpenseAccount.Id != null ? ca.ExpenseAccount.Id : 0,
                           FeeAmount = ca.FeeAmount != null ? ca.FeeAmount : 0,
                           Narration = ca.Narration != null ? ca.Narration : "",
                           SourceOfExpense_Id = ca.SourceOfExpense.Id != null ? ca.SourceOfExpense.Id : 0,

                           DBTrack = ca.DBTrack,
                       }).ToList();

              var Addrs = db.RecruitExpenses.Find(data);
              TempData["RowVersion"] = Addrs.RowVersion;
              var Auth = Addrs.DBTrack.IsModified;

              return Json(new Object[] { r, "", "", Auth, JsonRequestBehavior.AllowGet });
          }
      }

      [HttpPost]
      public async Task<ActionResult> EditSave(RecruitExpenses c, int data, FormCollection form) // Edit submit
      {
          List<string> Msg = new List<string>();
          using (DataBaseContext db = new DataBaseContext())
          {
              try
              {
                  var db_data = db.RecruitExpenses.Include(e => e.ExpenseAccount).Include(e => e.SourceOfExpense)
                                                     .Where(e => e.Id == data).SingleOrDefault();

                  bool Auth = form["Autho_Allow"] == "true" ? true : false;




                  string Category = form["ExpAccListP"] == "0" ? "" : form["ExpAccListP"];

                  if (Category != null)
                  {
                      if (Category != "")
                      {
                          var val = db.LookupValue.Find(int.Parse(Category));
                          c.ExpenseAccount = val;
                      }
                  }

                  string Category1 = form["SourceOFExpListP"] == "0" ? "" : form["SourceOFExpListP"];

                  if (Category1 != null)
                  {
                      if (Category1 != "")
                      {
                          var val = db.LookupValue.Find(int.Parse(Category1));
                          c.SourceOfExpense = val;
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
                                  db.RecruitExpenses.Attach(db_data);
                                  db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                  db.SaveChanges();
                                  TempData["RowVersion"] = db_data.RowVersion;
                                  db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                  var Curr_Lookup = db.RecruitExpenses.Find(data);
                                  TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                  db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                  if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                  {

                                      RecruitExpenses blog = null; // to retrieve old data
                                      DbPropertyValues originalBlogValues = null;

                                      using (var context = new DataBaseContext())
                                      {
                                          blog = context.RecruitExpenses.Where(e => e.Id == data).Include(e => e.ExpenseAccount).Include(e => e.SourceOfExpense)
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
                                      RecruitExpenses lk = new RecruitExpenses
                                      {
                                          Id = data,
                                          ExpenseAccount = c.ExpenseAccount,
                                          SourceOfExpense = c.SourceOfExpense,
                                          Narration = c.Narration,
                                          FeeAmount = c.FeeAmount,
                                          DBTrack = c.DBTrack
                                      };


                                      db.RecruitExpenses.Attach(lk);
                                      db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                      db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                      // int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                      // int a = EditS(Values, Valueshobby, Valuesls, Valuesqd, Valuesskl, Valuesscr, data, c, c.DBTrack);


                                      using (var context = new DataBaseContext())
                                      {

                                          //var obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                          //DT_RecruitExpenses DT_Corp = (DT_RecruitExpenses)obj;

                                          // db.Create(DT_Corp);
                                          db.SaveChanges();
                                      }
                                      await db.SaveChangesAsync();
                                      ts.Complete();

                                      Msg.Add("  Record Updated");
                                      return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                      //return Json(new Object[] { lk.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
                                  }
                              }
                          }
                          catch (DbUpdateConcurrencyException ex)
                          {
                              var entry = ex.Entries.Single();
                              var clientValues = (RecruitExpenses)entry.Entity;
                              var databaseEntry = entry.GetDatabaseValues();
                              if (databaseEntry == null)
                              {
                                  Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                  //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                              }
                              else
                              {
                                  var databaseValues = (RecruitExpenses)databaseEntry.ToObject();
                                  c.RowVersion = databaseValues.RowVersion;

                              }
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

                          RecruitExpenses blog = null; // to retrieve old data
                          DbPropertyValues originalBlogValues = null;
                          RecruitExpenses Old_Corp = null;

                          using (var context = new DataBaseContext())
                          {
                              blog = context.RecruitExpenses.Include(e => e.ExpenseAccount).Include(e => e.SourceOfExpense).Where(e => e.Id == data).SingleOrDefault();
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
                          RecruitExpenses qualificationDetails = new RecruitExpenses()
                          {

                              Id = data,
                              ExpenseAccount = c.ExpenseAccount,
                              SourceOfExpense = c.SourceOfExpense,
                              Narration = c.Narration,
                              FeeAmount = c.FeeAmount,
                              DBTrack = c.DBTrack
                          };

                          using (var context = new DataBaseContext())
                          {
                              var obj = DBTrackFile.ModifiedDataHistory("Recruitement/Recruitement", "M", blog, qualificationDetails, "RecruitExpenses", c.DBTrack);
                              // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                              Old_Corp = context.RecruitExpenses.Where(e => e.Id == data)
                                  .Include(e => e.ExpenseAccount).Include(e => e.SourceOfExpense).SingleOrDefault();
                              DT_RecruitExpenses DT_Corp = (DT_RecruitExpenses)obj;
                              //DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                              //DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                              //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                              db.Create(DT_Corp);
                              //db.SaveChanges();
                          }
                          blog.DBTrack = c.DBTrack;
                          db.RecruitExpenses.Attach(blog);
                          db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                          db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                          db.SaveChanges();
                          ts.Complete();
                          Msg.Add("  Record Updated");
                          return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                          //return Json(new Object[] { blog.Id, c.Institute, "Record Updated", JsonRequestBehavior.AllowGet });
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
	}
}