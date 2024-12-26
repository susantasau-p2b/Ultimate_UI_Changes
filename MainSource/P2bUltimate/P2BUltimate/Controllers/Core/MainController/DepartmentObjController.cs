using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
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
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Core.MainController
{
     [AuthoriseManger]
	public class DepartmentObjController : Controller
	{
		//
		// GET: /DepartmentObj/
		public ActionResult Index()
		{
			return View();
		}
		public ActionResult Partial()
		{
			return View("~/Views/Shared/Core/_DepartmentObj.cshtml");
		}
		//private DataBaseContext db = new DataBaseContext();
	
		public ActionResult PopulateDropDownList(string data, string data2)
		{
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.DepartmentObj.ToList();
                int selected = 0;
                List<SelectListItem> drop = new List<SelectListItem>();
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }
                foreach (var item in qurey)
                {
                    drop.Add(new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = "DeptCode :" + item.DeptCode + " DeptDesc :" + item.DeptDesc,
                        Selected = item.Id == selected ? true : false
                    });
                }
                return Json(drop, JsonRequestBehavior.AllowGet);
            }
		}
        public ActionResult Create(DepartmentObj c, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {

                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.DepartmentObj.Any(o => o.DeptCode == c.DeptCode))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };


                            DepartmentObj DepartmentObj = new DepartmentObj()
                            {
                                DeptCode = c.DeptCode == null ? "" : c.DeptCode.Trim(),
                                DeptDesc = c.DeptDesc == null ? "" : c.DeptDesc.Trim(),
                                DBTrack = c.DBTrack
                            };
                            try
                            {

                                db.DepartmentObj.Add(DepartmentObj);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                                DT_DepartmentObj DT_DeptObj = (DT_DepartmentObj)rtn_Obj;
                                db.Create(DT_DeptObj);
                                db.SaveChanges();
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = DepartmentObj.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { DepartmentObj.Id, null, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
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