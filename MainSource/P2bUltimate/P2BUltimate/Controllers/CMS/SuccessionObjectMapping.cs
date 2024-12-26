using CMS_SPS;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Transactions;
using P2b.Global;
using P2BUltimate.Security;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace P2BUltimate.Controllers.CMS
{
    public class SuccessionObjectMappingController : Controller
    {
        // GET: SuccessionObjectMapping
        public ActionResult Index()
        {
            return View("~/Views/CMS/MainViews/SuccessionObjectMapping/index.cshtml");
        }
        public ActionResult GetAppCategory(List<int> SkipIds, string Apprlist)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int item = Convert.ToInt32(Apprlist);
                var fall = db.AppCategory.Where(e => e.AppMode.Id == item).ToList();
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult GetModelList(string data, string data2, string Apprlist)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                switch (Apprlist)
                {
                    case "Attribute":
                        var qurey1 = db.AppraisalAttributeModel.ToList();
                        var selected1 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected1 = Convert.ToInt32(data2);
                        }

                        SelectList s1 = new SelectList(qurey1, "Id", "Code", selected1);

                        return Json(s1, JsonRequestBehavior.AllowGet);
                        break;

                    case "Potential":
                        var qurey2 = db.AppraisalPotentialModel.ToList();
                        var selected2 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }

                        SelectList s2 = new SelectList(qurey2, "Id", "Code", selected2);

                        return Json(s2, JsonRequestBehavior.AllowGet);
                        break;

                    case "Kra":
                        var qurey3 = db.AppraisalKRAModel.ToList();
                        var selected3 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected3 = Convert.ToInt32(data2);
                        }

                        SelectList s3 = new SelectList(qurey3, "Id", "Code", selected3);

                        return Json(s3, JsonRequestBehavior.AllowGet);
                        break;
                }
                return null;


            }

        }
        public ActionResult GetModelObjList(string data, string data2, string Apprlist, string Objmlist)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                switch (Apprlist)
                {
                    case "Attribute":
                        var qurey11 = db.AppraisalAttributeModel.Include(e => e.AppraisalAttributeModelObject)
                           .Include(e => e.AppraisalAttributeModelObject.Select(r => r.AppraisalAttributeModel))
                           .Where(e => e.Code == Objmlist).FirstOrDefault();
                        var qurey12 = qurey11.AppraisalAttributeModelObject.ToList();
                        var query13 = qurey12.Select(r => r.AppraisalAttributeModel).ToList();
                        var selected1 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected1 = Convert.ToInt32(data2);
                        }

                        SelectList s1 = new SelectList(query13, "Id", "LookupVal", selected1);
                        return Json(s1, JsonRequestBehavior.AllowGet);
                        break;

                    case "Potential":
                        var qurey21 = db.AppraisalPotentialModel.Include(e => e.AppraisalPotentialModelObject)
                        .Include(e => e.AppraisalPotentialModelObject.Select(r => r.AppraisalPotentialModel))
                        .Where(e => e.Code == Objmlist).FirstOrDefault();
                        var qurey22 = qurey21.AppraisalPotentialModelObject.ToList();
                        var query23 = qurey22.Select(r => r.AppraisalPotentialModel).ToList();
                        var selected2 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }

                        SelectList s2 = new SelectList(query23, "Id", "LookupVal", selected2);
                        return Json(s2, JsonRequestBehavior.AllowGet);
                        break;

                    case "Kra":
                        var qurey31 = db.AppraisalKRAModel.Include(e => e.AppraisalKRAModelObject)
                       .Include(e => e.AppraisalKRAModelObject.Select(r => r.AppraisalKRAModel))
                       .Where(e => e.Code == Objmlist).FirstOrDefault();
                        var qurey32 = qurey31.AppraisalKRAModelObject.ToList();
                        var query33 = qurey32.Select(r => r.AppraisalKRAModel).ToList();
                        var selected3 = (Object)null;
                        if (data2 != "" && data != "0" && data2 != "0")
                        {
                            selected2 = Convert.ToInt32(data2);
                        }
                        SelectList s3 = new SelectList(query33, "Id", "LookupVal", selected3);
                        return Json(s3, JsonRequestBehavior.AllowGet);
                        break;
                }
                return null;


            }

        }
        [HttpPost]
        public async Task<ActionResult> EditSave(LookupValue c, FormCollection form)
        {

            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string AppCategoryList = form["AppCategoryList"] == "" ? null : form["AppCategoryList"];
                    string MAPPIDList = form["MAPPIDList"] == "" ? null : form["MAPPIDList"];
                    int LookupvalueId = Convert.ToInt32(MAPPIDList);

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.LookupValue.Where(e => e.Id == LookupvalueId).SingleOrDefault();
                        TempData["RowVersion"] = db_data.RowVersion;
                        LookupValue LookupValue = db.LookupValue.Find(LookupvalueId);
                        TempData["CurrRowVersion"] = LookupValue.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = LookupValue.DBTrack.CreatedBy == null ? null : LookupValue.DBTrack.CreatedBy,
                                CreatedOn = LookupValue.DBTrack.CreatedOn == null ? null : LookupValue.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            LookupValue.Id = LookupvalueId;
                            LookupValue.LookupValData = AppCategoryList;
                            LookupValue.DBTrack = c.DBTrack;
                            db.Entry(LookupValue).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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