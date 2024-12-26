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
    public class PassportDetailsController : Controller
    {
        // GET: PassportDetails
        public ActionResult Index()
        {
            return View("~/Views/PassportDetails/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/_PassportDetails.cshtml");
        }
        public ActionResult view_partial()
        {
            return View("~/Views/Shared/_PassportDetailsView.cshtml");
        }

        public  Object Create(PassportDetails lkval, FormCollection form)
        {
            using(DataBaseContext db = new DataBaseContext())
            {
                if (ModelState.IsValid)
                {
                    using(TransactionScope ts = new TransactionScope())
                    {
                        lkval.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        PassportDetails ppdtl = new PassportDetails
                        {
                             PassportNo = lkval.PassportNo,
                             IssuePlace = lkval.IssuePlace,
                             IssueDate = lkval.IssueDate,
                             ExpiryDate = lkval.ExpiryDate,
                             DBTrack = lkval.DBTrack
                        };
                        try
                        {
                            db.PassportDetails.Add(ppdtl);
                            var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, lkval.DBTrack);
                            db.SaveChanges();
                            DT_PassportDetails DT_Corp = (DT_PassportDetails)a;
                            DT_Corp.Orig_Id = ppdtl.Id;
                            // DT_Corp.SkillType_Id = lkval.SkillType == null ? 0 : lkval.SkillType.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();

                            var empid = Convert.ToInt32(SessionManager.EmpId);

                            var EmpPassportDetails = db.Employee.Include(e => e.PassportDetails).Where(e => e.Id == empid).SingleOrDefault();

                            if (EmpPassportDetails != null && EmpPassportDetails.PassportDetails != null)
                            {
                                if (EmpPassportDetails.PassportDetails != null)
                                {
                                    EmpPassportDetails.PassportDetails.Add(ppdtl);
                                }
                                else
                                {
                                    EmpPassportDetails.PassportDetails = new List<PassportDetails> { ppdtl };
                                }
                            }
                            else
                            {
                                var oEmpPassportDetails = new Employee();
                                oEmpPassportDetails.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                oEmpPassportDetails.PassportDetails = new List<PassportDetails> { ppdtl };
      
                            }
                            db.Entry(EmpPassportDetails).State = System.Data.Entity.EntityState.Modified;
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
           

                var Q = db.PassportDetails.Where(e => e.Id == id).Select
                    (e => new
                    {
                        id = e.Id,
                        PassportNo = e.PassportNo,
                        IssuePlace = e.IssuePlace,
                        IssueDate = e.IssueDate,
                        ExpiryDate = e.ExpiryDate,
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
        public Object EditSave(PassportDetails c, int data, FormCollection form) // Edit submit
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

                            var db_data = db.PassportDetails.Where(e => e.Id == data).SingleOrDefault();


                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                db.PassportDetails.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_data.RowVersion;
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_Lookup = db.Employee.Find(data);
                                TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    PassportDetails blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.PassportDetails.Where(e => e.Id == data).SingleOrDefault();
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
                                    PassportDetails lk = new PassportDetails
                                    {
                                        Id = data,
                                        ExpiryDate = c.ExpiryDate,
                                        IssueDate = c.IssueDate,
                                        PassportNo = c.PassportNo,
                                        IssuePlace = c.IssuePlace,
                                        DBTrack = c.DBTrack,

                                    };


                                    db.PassportDetails.Attach(lk);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                    db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];


                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_PassportDetails DT_Corp = (DT_PassportDetails)obj;

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
                            var clientValues = (PassportDetails)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return new { status = true, responseText = "Unable to save changes. The record was deleted by another user." };
                            }
                            else
                            {
                                var databaseValues = (PassportDetails)databaseEntry.ToObject();
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

                        PassportDetails blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        PassportDetails Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.PassportDetails.Where(e => e.Id == data).SingleOrDefault();
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
                        PassportDetails passportDetails = new PassportDetails()
                        {

                            Id = data,
                            ExpiryDate = c.ExpiryDate,
                            IssueDate = c.IssueDate,
                            PassportNo = c.PassportNo,
                            IssuePlace = c.IssuePlace,
                            DBTrack = c.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, passportDetails, "PassportDetails", c.DBTrack);

                            Old_Corp = context.PassportDetails.Where(e => e.Id == data).SingleOrDefault();

                            DT_PassportDetails DT_Corp = (DT_PassportDetails)obj;
                            db.Create(DT_Corp);
                        }
                        blog.DBTrack = c.DBTrack;
                        db.PassportDetails.Attach(blog);
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



        public ActionResult AddOrEdit(PassportDetails lkval, FormCollection form)
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
                PassportDetails passportDetails = db.PassportDetails.Where(e => e.Id == data).SingleOrDefault();

                //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                if (passportDetails.DBTrack.IsModified == true)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            CreatedBy = passportDetails.DBTrack.CreatedBy != null ? passportDetails.DBTrack.CreatedBy : null,
                            CreatedOn = passportDetails.DBTrack.CreatedOn != null ? passportDetails.DBTrack.CreatedOn : null,
                            IsModified = passportDetails.DBTrack.IsModified == true ? true : false
                        };
                        passportDetails.DBTrack = dbT;
                        db.Entry(passportDetails).State = System.Data.Entity.EntityState.Modified;
                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, passportDetails.DBTrack);
                        DT_Employee DT_Corp = (DT_Employee)rtn_Obj;
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
                                CreatedBy = passportDetails.DBTrack.CreatedBy != null ? passportDetails.DBTrack.CreatedBy : null,
                                CreatedOn = passportDetails.DBTrack.CreatedOn != null ? passportDetails.DBTrack.CreatedOn : null,
                                IsModified = passportDetails.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = "0029",
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = "0029", ModifiedOn = DateTime.Now };

                            db.Entry(passportDetails).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            DT_PassportDetails DT_Corp = (DT_PassportDetails)rtn_Obj;

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
        public ActionResult GetMyEmpPassportDetails()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var qurey = db.Employee.Include(e => e.PassportDetails)
                 .Where(e => e.Id == Emp ).SingleOrDefault();



                var ListreturnDataClass = new List<returnDataClass>();
                if (qurey != null && qurey.PassportDetails != null && qurey.PassportDetails.Count > 0)
                {
                    foreach (var item in qurey.PassportDetails)
                    {
                        var PassportNo = item.PassportNo != null ? item.PassportNo.ToString() : null;
                        //var PassportNoString = PassportNo != null ? string.Join(",", PassportNo) : null;
                        var IssueDate = item.IssueDate.Value.ToShortDateString() != null ? item.IssueDate.Value.ToShortDateString() : null;
                        var ExpDate = item.ExpiryDate.Value.ToShortDateString() != null ? item.ExpiryDate.Value.ToShortDateString() : null;
                        ListreturnDataClass.Add(new returnDataClass
                        {
                            EmpId = item.Id,
                            val =
                            "PassportNo : " + PassportNo +
                            "IssueDate : " + IssueDate +
                            "ExpiryDate : " + ExpDate + ""

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
        public class GetLvNewReqClass
        {
            public string Emp { get; set; }
            public string PassportNo { get; set; }
            public string IssuePlace { get; set; }
            public string IssueDate { get; set; }
            public string ExpiryDate { get; set; }
            public string PassportDetails { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }




        public ActionResult GetMyEmpPassportDetails1()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var db_data = db.Employee.Include(e => e.PassportDetails)
                 .Where(e => e.Id == Id).SingleOrDefault();


                if (db_data != null)
                {
                    List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();
                    returndata.Add(new GetLvNewReqClass
                    {
                        PassportNo = "Passport No",
                        IssuePlace = "Issue Place",
                        IssueDate = "Issue Date",
                        ExpiryDate = "Expiry Date",
                    });
                    foreach (var item in db_data.PassportDetails.OrderByDescending(a => a.Id).ToList())
                    {
                        var passd = item.FullDetails.ToList();
                        if (passd != null)
                        {
                            var arralist = passd.ToArray().Distinct();
                            var passdetl = arralist != null ? string.Join(",", arralist) : null;

                            returndata.Add(new GetLvNewReqClass
                            {
                                RowData = new ChildGetLvNewReqClass
                                {
                                    LvNewReq = item.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString(),
                                    LvHead_Id = "",
                                },
                    
                                PassportNo = item.PassportNo == null ? "" : item.PassportNo.ToString(),
                                IssuePlace = item.IssuePlace == null ? "" : item.IssuePlace.ToString(),
                                IssueDate = item.IssueDate == null ? "" : item.IssueDate.Value.ToShortDateString(),
                                ExpiryDate = item.ExpiryDate == null ? "" : item.ExpiryDate.Value.ToShortDateString(),
                             
                            });
                        }
                    }
                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }




        //public class ChildGetLvNewReqClass
        //{
        //    public string Passportreq { get; set; }
        //    public string EmpPassportid { get; set; }
        //    public string IsClose { get; set; }
        //    public string Passport_Id { get; set; }

        //}

        //public class GetPassportDetailsClass
        //{
        //    public string Emp { get; set; }
        //    public string PassportNo { get; set; }
        //    public string Issueplace { get; set; }
        //    public string Issuedate { get; set; }
        //    public string expirydate { get; set; }
        //    public ChildGetLvNewReqClass RowData { get; set; }
        //}



        //public ActionResult GetMyPassportDetails()
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpId)))
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }
        //        var Id = Convert.ToInt32(SessionManager.EmpId);
             
        //        var db_data = db.Employee
        //            .Where(e => e.Id == Id)
        //           .Include(e => e.PassportDetails).SingleOrDefault();



        //        if (db_data != null)
        //        {
        //            List<GetPassportDetailsClass> returndata = new List<GetPassportDetailsClass>();
        //            returndata.Add(new GetPassportDetailsClass
        //            {
        //                PassportNo = "Passport No",
        //                Issueplace = "Issue Place",
        //                Issuedate = "Issue Date",
        //                expirydate = "Expiry Date"
        //            });
        //            foreach (var item in db_data.PassportDetails.OrderByDescending(a => a.IssueDate).ToList())
        //            {
        //                var IssueDate = item.IssueDate != null ? item.IssueDate.Value.ToShortDateString() : null;
        //                var ExpiryDate = item.ExpiryDate != null ? item.ExpiryDate.Value.ToShortDateString() : null;
        //                returndata.Add(new GetPassportDetailsClass
        //                {
        //                    RowData = new ChildGetLvNewReqClass
        //                    {
        //                        Passportreq = item.Id.ToString(),
        //                        EmpPassportid = item.Id.ToString(),
                       
        //                    },
                          
        //                    PassportNo = item.PassportNo.ToString(),
        //                    Issueplace = item.IssuePlace.ToString(),
        //                    Issuedate = item.IssueDate == null ? "" : item.IssueDate.Value.ToShortDateString(),
        //                    expirydate = item.ExpiryDate == null ? "" : item.ExpiryDate.Value.ToShortDateString(),

        //                });

        //            }
        //            return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}


    }
}