using P2b.Global;
using EssPortal.App_Start;
using EssPortal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Text;
using System.Threading.Tasks;
using EssPortal.Security;

namespace EssPortal.Controllers
{
    public class HobbyController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Hobby/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/_Hobby.cshtml");
        }
        public ActionResult view_partial()
        {
            return View("~/Views/Shared/_HobbyView.cshtml");
        }
        public Object Create(Hobby lkval, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        lkval.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        Hobby qualdtl = new Hobby
                        {
                            HobbyName = lkval.HobbyName,
                            DBTrack = lkval.DBTrack
                        };
                        try
                        {
                            db.Hobby.Add(qualdtl);
                            var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, lkval.DBTrack);
                            db.SaveChanges();
                            DT_Hobby DT_Corp = (DT_Hobby)a;
                            DT_Corp.Orig_Id = qualdtl.Id;
                            // DT_Corp.SkillType_Id = lkval.SkillType == null ? 0 : lkval.SkillType.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();

                            var empid = Convert.ToInt32(SessionManager.EmpId);

                            var EmpHobbyDataChk = db.Employee.Include(e => e.EmpAcademicInfo)
                                .Include(e => e.EmpAcademicInfo.Hobby).Where(e => e.Id == empid).SingleOrDefault();


                            if (EmpHobbyDataChk != null && EmpHobbyDataChk.EmpAcademicInfo != null)
                            {
                                if (EmpHobbyDataChk.EmpAcademicInfo.Hobby != null)
                                {
                                    EmpHobbyDataChk.EmpAcademicInfo.Hobby.Add(qualdtl);
                                }
                                else
                                {
                                    EmpHobbyDataChk.EmpAcademicInfo.Hobby = new List<Hobby> { qualdtl };
                                }
                            }
                            else
                            {
                                var oEmpAcademicInfo = new EmpAcademicInfo();
                                oEmpAcademicInfo.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                oEmpAcademicInfo.Hobby = new List<Hobby> { qualdtl };
                                EmpHobbyDataChk.EmpAcademicInfo = oEmpAcademicInfo;
                            }
                            db.Entry(EmpHobbyDataChk).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();


                            ts.Complete();
                            return new { status = true, responseText = "Data Created Successfully." };
                        }

                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = lkval.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                    return new { status = false, responseText = errorMsg };
                }
            }
        }

        public ActionResult Edit(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);


                var returndata = (Object)null;
                var Q = db.Hobby.Where(e => e.Id == id).Select
                (e => new
                {
                    id = e.Id,
                    HobbyName = e.HobbyName,
                    isauth = true,
                    Add = false,
                    DBTrack = e.DBTrack.Action
                }).ToList();

                returndata = new
                {
                    Add = true,
                };

                return Json(new Object[] { Q, returndata, "", JsonRequestBehavior.AllowGet });
            }
        }
        public Object EditSave(Hobby c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                // bool Auth = form["autho_allow"] == "true" ? true : false;
                bool Auth = true;


                if (Auth == false)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {

                            var db_data = db.Hobby.Where(e => e.Id == data).SingleOrDefault();


                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                db.Hobby.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_data.RowVersion;
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_Lookup = db.QualificationDetails.Find(data);
                                TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    Hobby blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.Hobby.Where(e => e.Id == data)
                                                          .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = "0029",
                                        ModifiedOn = DateTime.Now
                                    };
                                    Hobby lk = new Hobby
                                    {
                                        Id = data,
                                        HobbyName = c.HobbyName,
                                        DBTrack = c.DBTrack,

                                    };


                                    db.Hobby.Attach(lk);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                    db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];


                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_QualificationDetails DT_Corp = (DT_QualificationDetails)obj;

                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    //  await db.SaveChangesAsync();
                                    ts.Complete();


                                    return new { status = true, responseText = "Record Updated" };
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (Hobby)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return new { status = true, responseText = "Unable to save changes. The record was deleted by another user." };
                            }
                            else
                            {
                                var databaseValues = (Hobby)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;

                            }
                        }

                        return new { status = true, responseText = "Record modified by another user.So refresh it and try to save again." };
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        Hobby blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        Hobby Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.Hobby.Where(e => e.Id == data).SingleOrDefault();
                            originalBlogValues = context.Entry(blog).OriginalValues;
                        }
                        c.DBTrack = new DBTrack
                        {
                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            Action = "M",
                            IsModified = blog.DBTrack.IsModified == true ? true : false,
                            ModifiedBy = "0029",
                            ModifiedOn = DateTime.Now
                        };
                        Hobby hobby = new Hobby()
                        {

                            Id = data,
                            HobbyName = c.HobbyName,
                            DBTrack = c.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, hobby, "Hobby", c.DBTrack);

                            Old_Corp = context.Hobby.Where(e => e.Id == data).SingleOrDefault();

                            DT_Hobby DT_Corp = (DT_Hobby)obj;
                            db.Create(DT_Corp);
                        }
                        blog.DBTrack = c.DBTrack;
                        db.Hobby.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        // db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        ts.Complete();
                        return new { status = true, responseText = "Record Updated" };
                    }
                }
                return new Object[] { };
            }
        }
        public ActionResult AddOrEdit(Hobby lkval, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Add = form["Add"] != null && form["Add"] != "" ? Convert.ToBoolean(form["Add"]) : true;
                var Id = form["auth_id"] != null && form["auth_id"] != "" ? Convert.ToInt32(form["auth_id"]) : 0;
                if (Add == true)
                {
                    //Add
                    var returnobj = Create(lkval, form);
                    return Json(returnobj, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //Edit
                    var returnobj = EditSave(lkval, Id, form);
                    return Json(returnobj, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Hobby hobby = db.Hobby.Where(e => e.Id == data).SingleOrDefault();

                //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                if (hobby.DBTrack.IsModified == true)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            CreatedBy = hobby.DBTrack.CreatedBy != null ? hobby.DBTrack.CreatedBy : null,
                            CreatedOn = hobby.DBTrack.CreatedOn != null ? hobby.DBTrack.CreatedOn : null,
                            IsModified = hobby.DBTrack.IsModified == true ? true : false
                        };
                        hobby.DBTrack = dbT;
                        db.Entry(hobby).State = System.Data.Entity.EntityState.Modified;
                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, hobby.DBTrack);
                        DT_QualificationDetails DT_Corp = (DT_QualificationDetails)rtn_Obj;
                        //DT_Corp.Language_Id = corporates.Language == null ? 0 : corporates.Language.Id;


                        db.Create(DT_Corp);
                        // db.SaveChanges();
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                        await db.SaveChangesAsync();
                        //using (var context = new DataBaseContext())
                        //{
                        //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                        //}
                        ts.Complete();
                        return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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
                                ModifiedBy = "0029",
                                ModifiedOn = DateTime.Now,
                                CreatedBy = hobby.DBTrack.CreatedBy != null ? hobby.DBTrack.CreatedBy : null,
                                CreatedOn = hobby.DBTrack.CreatedOn != null ? hobby.DBTrack.CreatedOn : null,
                                IsModified = hobby.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = "0029",
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = "0029", ModifiedOn = DateTime.Now };

                            db.Entry(hobby).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            DT_Hobby DT_Corp = (DT_Hobby)rtn_Obj;

                            db.Create(DT_Corp);

                            await db.SaveChangesAsync();


                            //using (var context = new DataBaseContext())
                            //{
                            //    corporates.Address = add;
                            //    corporates.ContactDetails = conDet;
                            //    corporates.BusinessType = val;
                            //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
                            //}
                            ts.Complete();
                            return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });


                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            //Log the error (uncomment dex variable name and add a line here to write a log.)
                            //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                            //return RedirectToAction("Delete");
                            // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });

                            return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });

                        }
                    }
                }
            }
        }

        public class returnDataClass
        {
            public Int32 EmpId { get; set; }
            public String val { get; set; }
            public Int32 Id2 { get; set; }
            public bool Id3 { get; set; }
            public Int32 LvHead_Id { get; set; }
        }
        public ActionResult GetMyEmpHobby()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var qurey = db.Employee.Include(e => e.EmpAcademicInfo)
                 .Include(e => e.EmpAcademicInfo.Hobby)
                 .Where(e => e.Id == Emp && e.EmpAcademicInfo != null).SingleOrDefault();



                var ListreturnDataClass = new List<returnDataClass>();
                if (qurey != null && qurey.EmpAcademicInfo != null && qurey.EmpAcademicInfo.Hobby.Count > 0)
                {
                    foreach (var item in qurey.EmpAcademicInfo.Hobby)
                    {
                        var HobbyName = item.HobbyName != null ? item.HobbyName.ToString() : null;
                        var HobbbyString = HobbyName != null ? string.Join(",", HobbyName) : null;
                        ListreturnDataClass.Add(new returnDataClass
                        {
                            EmpId = item.Id,
                            val =
                            "Hobby :" + HobbyName
                        });
                    }
                }
                if (ListreturnDataClass != null && ListreturnDataClass.Count > 0)
                {
                    return Json(new { status = true, data = ListreturnDataClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = ListreturnDataClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }

        }

        public class GetHobbyClass
        {
            public string Emp { get; set; }
            public string HobbyName { get; set; }
            public string LvHead { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public ChildGetLvNewReqClass RowData { get; set; }
        }


        public ActionResult GetMyHobbyNew()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                //var db_data = db.EmployeeLeave
                //      .Where(e => e.Id == Id)
                //      .Include(e => e.LvNewReq.Select(a => a.LvWFDetails))
                //      .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                //     .SingleOrDefault();

                var db_data = db.Employee
                 .Where(e => e.Id == Id).Include(e => e.EmpAcademicInfo)
                 .Include(e => e.EmpAcademicInfo.Hobby)
                 .SingleOrDefault();

                if (db_data.EmpAcademicInfo != null)
                {
                    List<GetHobbyClass> returndata = new List<GetHobbyClass>();
                    returndata.Add(new GetHobbyClass
                    {
                        HobbyName = "Hobby Name",
                        //LvHead = "Leave Head",
                        //FromDate = "From Date",
                        //ToDate = "To Date"
                    });
                    foreach (var item in db_data.EmpAcademicInfo.Hobby.OrderByDescending(a => a.HobbyName).ToList())
                    {
                        var HobbyName = item.HobbyName != null ? item.HobbyName.ToString() : null;


                        returndata.Add(new GetHobbyClass
                        {
                            RowData = new ChildGetLvNewReqClass
                            {
                                LvNewReq = item.Id.ToString(),
                                EmpLVid = db_data.Id.ToString(),
                                LvHead_Id = "",
                            },
                            HobbyName = item.HobbyName.ToString()

                        });


                    }
                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }





    }
}