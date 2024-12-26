using P2b.Global;
using EssPortal.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using EssPortal.Models;
using System.Threading.Tasks;
using System.Collections;
using EssPortal.Security;

namespace EssPortal.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class FamilyDetailsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_FamilyDetailsPartial.cshtml");
        }
        public ActionResult view_partial()
        {
            return View("~/Views/Shared/_FamilyDetailsView.cshtml");
        }
        public ActionResult Create(FamilyDetails fd, FormCollection form)
        {
            string reln = form["Relationlist"] == "0" ? "" : form["Relationlist"];
            string proffesion = form["Professionlist"] == "0" ? "" : form["Professionlist"];
            string Addrs = form["Addresslist"] == "0" ? "" : form["Addresslist"];
            string nominee = form["MemberNameId"] == "0" ? "" : form["MemberNameId"];
            string ContactDetailslist = form["ContactDetailslist"] == "0" ? "" : form["ContactDetailslist"];
            using (DataBaseContext db = new DataBaseContext())
            {
                if (nominee != null)
                {
                    if (nominee != "")
                    {
                        int nomineeId = Convert.ToInt32(nominee);
                        var vals = db.NameSingle.Where(e => e.Id == nomineeId).SingleOrDefault();
                        fd.MemberName = vals;
                    }
                }
                if (reln != null)
                {
                    if (reln != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(reln));
                        fd.Relation = val;
                    }
                }
                if (proffesion != null)
                {
                    if (proffesion != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(proffesion));
                        fd.Profession = val;
                    }
                }
                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        int AddId = Convert.ToInt32(Addrs);
                        var val = db.Address//.Include(e => e.Area)
                            //.Include(e => e.City)
                            //.Include(e => e.Country)
                            //.Include(e => e.District)
                            //.Include(e => e.State)
                            //.Include(e => e.StateRegion)
                            //.Include(e => e.Taluka)
                                            .Where(e => e.Id == AddId).SingleOrDefault();
                        fd.Address = val;
                    }
                }



                List<ContactDetails> lookupLang = new List<ContactDetails>();
                string Lang = form["ContactDetailslist"];

                if (Lang != null)
                {
                    if (Lang != "")
                    {

                        //  var ids = Utility.StringIdsToListIds(Lang);
                        // foreach (var ca in ids)
                        // {
                        //var lookup_val = db.ContactDetails.Find(ca);
                        int contid = Convert.ToInt32(Lang);
                        //lookupLang.Add(lookup_val);
                        var contactdata = db.ContactDetails//.Include(e => e.Area)
                            //.Include(e => e.City)
                            //.Include(e => e.Country)
                            //.Include(e => e.District)
                            //.Include(e => e.State)
                            //.Include(e => e.StateRegion)
                            //.Include(e => e.Taluka)
                                           .Where(e => e.Id == contid).SingleOrDefault();
                        // }
                        fd.ContactDetails = contactdata;
                    }
                }
                else
                {
                    fd.ContactDetails = null;
                }

                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        fd.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        FamilyDetails familydetails = new FamilyDetails()
                        {
                            MemberName = fd.MemberName,
                            DateofBirth = fd.DateofBirth,
                            Profession = fd.Profession,
                            Relation = fd.Relation,
                            IsHandicap = fd.IsHandicap,
                            HandicapRemark = fd.HandicapRemark,
                            Address = fd.Address,
                            ContactDetails = fd.ContactDetails,
                            DBTrack = fd.DBTrack
                        };
                        try
                        {
                            db.FamilyDetails.Add(familydetails);
                            var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, fd.DBTrack);
                            db.SaveChanges();
                            DT_FamilyDetails DT_Corp = (DT_FamilyDetails)a;
                            DT_Corp.Orig_Id = familydetails.Id;
                            db.Create(DT_Corp);
                            db.SaveChanges();
                            var id = Convert.ToInt32(SessionManager.EmpId);
                            var empdata = db.Employee.Include(e => e.FamilyDetails).Where(e => e.Id == id).SingleOrDefault();
                            if (empdata.FamilyDetails != null && empdata.FamilyDetails.Count > 0)
                            {
                                empdata.FamilyDetails.Add(familydetails);
                            }
                            else
                            {
                                empdata.FamilyDetails = new List<FamilyDetails> { familydetails };
                            }
                            db.Entry(empdata).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            ts.Complete();
                            return Json(new { status = true, responseText = "Data Created Successfully." });
                        }

                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = fd.Id });
                        }
                        catch (DataException e)
                        {
                            return Json(new { status = false, responseText = e.InnerException.Message.ToString() });
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
                    return Json(new { status = true, valid = true, responseText = errorMsg });
                }
            }
        }
        public ActionResult AddOrEdit(FamilyDetails lkval, FormCollection form)
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
        public class QualDetails
        {
            public Array Qual_Id { get; set; }
            public Array Qual_FullDetails { get; set; }
        }
        public ActionResult Edit(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                string[] para = { };
                var qurey = db.Employee.Include(e => e.FamilyDetails)
                    .Include(e => e.FamilyDetails.Select(a => a.Address))
                    .Include(e => e.FamilyDetails.Select(a => a.Profession))
                    .Include(e => e.FamilyDetails.Select(a => a.ContactDetails))
                    .Include(e => e.FamilyDetails.Select(a => a.ContactDetails.ContactNumbers))
                    .Include(e => e.FamilyDetails.Select(a => a.MemberName))
                    .Include(e => e.FamilyDetails.Select(a => a.Relation))
                    .Where(e => e.Id == Emp && e.FamilyDetails.Any(a => a.Id == id))
                    .AsEnumerable().Select(e => new
                    {
                        FamilyDetails = e.FamilyDetails.Where(a => a.Id == id).SingleOrDefault(),
                        ContactDetails = e.FamilyDetails.Where(a => a.Id == id).Select(a => a.ContactDetails),
                    }).SingleOrDefault();

                var returndata = (Object)null;
                var ListQualDetails = new List<QualDetails>();
                var returnCurrentData = (Object)null;

                if (qurey != null)
                {
                    if (qurey.FamilyDetails != null)
                    {
                        returndata = new
                        {
                            id = qurey.FamilyDetails.Id,
                            MemberName_Id = qurey.FamilyDetails.MemberName == null ? 0 : qurey.FamilyDetails.MemberName.Id,
                            MemberName_FullNameFML = qurey.FamilyDetails.MemberName == null ? null : qurey.FamilyDetails.MemberName.FullNameFML,
                            Profession_Id = qurey.FamilyDetails.Profession == null ? 0 : qurey.FamilyDetails.Profession.Id,
                            Relation_Id = qurey.FamilyDetails.Relation != null ? qurey.FamilyDetails.Relation.Id : 0,
                            Address_Id = qurey.FamilyDetails.Address != null ? qurey.FamilyDetails.Address.Id : 0,
                            FullAddress = qurey.FamilyDetails.Address != null ? qurey.FamilyDetails.Address.FullAddress : null,
                            DateofBirth = qurey.FamilyDetails.DateofBirth == null ? null : qurey.FamilyDetails.DateofBirth.Value.ToShortDateString(),
                            IsHandicap = qurey.FamilyDetails.IsHandicap,
                            HandicapRemark = qurey.FamilyDetails.HandicapRemark == null ? "" : qurey.FamilyDetails.HandicapRemark,
                            isauth = true,
                            Add = false
                        };
                        var k = qurey.ContactDetails.ToList();
                        foreach (var val in k)
                        {
                            if (val != null)
                            {
                                ListQualDetails.Add(new QualDetails
                                {
                                    Qual_Id = k == null ? para : k.Select(e => e.Id.ToString()).ToArray(),
                                    Qual_FullDetails = k == null ? para : k.Select(e => e.FullContactDetails).ToArray(),
                                });

                            }
                        }
                        //curr data
                        var dt_data = db.DT_FamilyDetails.Where(e => e.Orig_Id == qurey.FamilyDetails.Id && e.DBTrack.IsAuthorized == 0).OrderByDescending(e => e.Id).FirstOrDefault();
                        if (dt_data != null)
                        {
                            returnCurrentData = new
                            {
                                //Institute = dt_data.Institute == null ? qurey.oQualificationDetails.Institute == null ? null : qurey.oQualificationDetails.Institute : dt_data.Institute,
                                //SpecilisedBranch = dt_data.SpecilisedBranch == null ? qurey.oQualificationDetails.SpecilisedBranch == null ? null : qurey.oQualificationDetails.SpecilisedBranch : dt_data.SpecilisedBranch,
                                //University = dt_data.University == null ? qurey.oQualificationDetails.University == null ? null : qurey.oQualificationDetails.University : dt_data.University,
                                //PasingYear = dt_data.PasingYear != null ? dt_data.PasingYear.Value.ToShortDateString() : qurey.oQualificationDetails.PasingYear != null ? qurey.oQualificationDetails.PasingYear.Value.ToShortDateString() : null,
                                //ResultPercentage = dt_data.ResultPercentage == 0 ? qurey.oQualificationDetails.ResultPercentage == 0 ? 0 : qurey.oQualificationDetails.ResultPercentage : dt_data.ResultPercentage,
                                //ResultGradation = dt_data.ResultGradation == null ? qurey.oQualificationDetails.ResultGradation == null ? null : qurey.oQualificationDetails.ResultGradation : dt_data.ResultGradation,
                                //SpecialFeature = dt_data.SpecialFeature == null ? qurey.oQualificationDetails.SpecialFeature == null ? null : qurey.oQualificationDetails.SpecialFeature : dt_data.SpecialFeature,
                                //ResultSubmissionDate = dt_data.ResultSubmissionDate != null ? dt_data.ResultSubmissionDate.Value.ToShortDateString() : qurey.oQualificationDetails.ResultSubmissionDate != null ? qurey.oQualificationDetails.ResultSubmissionDate.Value.ToShortDateString() : null,
                                //Action = qurey.DBTrack.Action,
                            };
                        }
                    }
                    else
                    {
                        returndata = new
                        {
                            Add = true,
                        };
                    }

                    return Json(new Object[] { returndata, ListQualDetails, returnCurrentData, "", JsonRequestBehavior.AllowGet });
                }
                return Json(new Object[] { returndata, ListQualDetails, returnCurrentData, "", JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave1(FamilyDetails fd, int data, FormCollection form) // Edit submit
        {
            string reln = form["Relationlist"] == "0" ? "" : form["Relationlist"];
            string proffesion = form["Professionlist"] == "0" ? "" : form["Professionlist"];
            string Addrs = form["Addresslist"] == "0" ? "" : form["Addresslist"];
            string membname = form["MemberNameId"] == "0" ? "" : form["MemberNameId"];

            bool Auth = form["autho_allow"] == "true" ? true : false;

            using (DataBaseContext db = new DataBaseContext())
            {
                if (reln != null)
                {
                    if (reln != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(reln));
                        fd.Relation = val;
                    }
                }
                if (proffesion != null)
                {
                    if (proffesion != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(proffesion));
                        fd.Profession = val;
                    }
                }
                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        int AddId = Convert.ToInt32(Addrs);
                        var val = db.Address//.Include(e => e.Area)
                            //.Include(e => e.City)
                            //.Include(e => e.Country)
                            //.Include(e => e.District)
                            //.Include(e => e.State)
                            //.Include(e => e.StateRegion)
                            //.Include(e => e.Taluka)
                                            .Where(e => e.Id == AddId).SingleOrDefault();
                        fd.Address = val;
                    }
                }


                List<ContactDetails> lookupLang = new List<ContactDetails>();
                string Lang = form["ContactDetailslist"];

                //if (Lang != null)
                //{
                //    var ids = Utility.StringIdsToListIds(Lang);
                //    foreach (var ca in ids)
                //    {
                //        var lookup_val = db.ContactDetails.Find(ca);
                //        lookupLang.Add(lookup_val);
                //        fd.ContactDetails = lookupLang;
                //    }
                //}
                if (Lang != null)
                {
                    if (Lang != "")
                    {

                        //  var ids = Utility.StringIdsToListIds(Lang);
                        // foreach (var ca in ids)
                        // {
                        //var lookup_val = db.ContactDetails.Find(ca);
                        int contid = Convert.ToInt32(Lang);
                        //lookupLang.Add(lookup_val);
                        var contactdata = db.ContactDetails//.Include(e => e.Area)
                            //.Include(e => e.City)
                            //.Include(e => e.Country)
                            //.Include(e => e.District)
                            //.Include(e => e.State)
                            //.Include(e => e.StateRegion)
                            //.Include(e => e.Taluka)
                                           .Where(e => e.Id == contid).SingleOrDefault();
                        // }
                        fd.ContactDetails = contactdata;
                    }
                }
                else
                {
                    fd.ContactDetails = null;
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
                                FamilyDetails blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.FamilyDetails.Where(e => e.Id == data).Include(e => e.MemberName).Include(e => e.Relation)
                                                            .Include(e => e.Profession)
                                                            .Include(e => e.Address)
                                                            .Include(e => e.ContactDetails).SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }

                                fd.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };

                                int a = EditS(reln, Addrs, membname, Lang, proffesion, data, fd, fd.DBTrack);


                                using (var context = new DataBaseContext())
                                {


                                    var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, fd.DBTrack);
                                    DT_FamilyDetails DT_Corp = (DT_FamilyDetails)obj;
                                    DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                    DT_Corp.Relation_Id = blog.Relation == null ? 0 : blog.Relation.Id;
                                    DT_Corp.Profession_Id = blog.Profession == null ? 0 : blog.Profession.Id;
                                    DT_Corp.MemberName_Id = blog.MemberName == null ? 0 : blog.MemberName.Id;
                                    // DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;

                                    db.Create(DT_Corp);
                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                ts.Complete();


                                return Json(new Object[] { fd.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });

                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (FamilyDetails)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (Corporate)databaseEntry.ToObject();
                                fd.RowVersion = databaseValues.RowVersion;

                            }
                        }

                        return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        FamilyDetails blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        FamilyDetails Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.FamilyDetails.Where(e => e.Id == data).SingleOrDefault();
                            originalBlogValues = context.Entry(blog).OriginalValues;
                        }
                        fd.DBTrack = new DBTrack
                        {
                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            Action = "M",
                            IsModified = blog.DBTrack.IsModified == true ? true : false,
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now
                        };

                        if (TempData["RowVersion"] == null)
                        {
                            TempData["RowVersion"] = blog.RowVersion;
                        }

                        FamilyDetails corp = new FamilyDetails()
                        {
                            DateofBirth = fd.DateofBirth,
                            //   MemberName = fd.MemberName,
                            Id = data,
                            DBTrack = fd.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };


                        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "FamilyDetails", fd.DBTrack);
                            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            Old_Corp = context.FamilyDetails.Where(e => e.Id == data).Include(e => e.Relation).Include(e => e.Profession)
                                .Include(e => e.Address).Include(e => e.ContactDetails).Include(e => e.MemberName).SingleOrDefault();
                            DT_FamilyDetails DT_Corp = (DT_FamilyDetails)obj;
                            DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, fd.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                            DT_Corp.Profession_Id = DBTrackFile.ValCompare(Old_Corp.Profession, fd.Profession); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                            DT_Corp.Relation_Id = DBTrackFile.ValCompare(Old_Corp.Relation, fd.Relation);
                            DT_Corp.MemberName_Id = DBTrackFile.ValCompare(Old_Corp.MemberName, fd.MemberName);
                            //  DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, fd.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            //db.SaveChanges();
                        }
                        blog.DBTrack = fd.DBTrack;
                        db.FamilyDetails.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        ts.Complete();
                        return Json(new Object[] { blog.Id, "", "Record Updated", JsonRequestBehavior.AllowGet });
                    }

                }
                return View();

            }
        }





        public Object EditSave(FamilyDetails c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                string reln = form["Relationlist"] == "0" ? "" : form["Relationlist"];
                string proffesion = form["Professionlist"] == "0" ? "" : form["Professionlist"];
                string Addrs = form["Addresslist"] == "0" ? "" : form["Addresslist"];
                string membname = form["MemberNameId"] == "0" ? "" : form["MemberNameId"];
                string contact = form["ContactDetailslist"] == "0" ? "" : form["ContactDetailslist"];
                // bool Auth = form["autho_allow"] == "true" ? true : false;
                bool Auth = true;


                if (Auth == false)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            var db_data = db.FamilyDetails.Include(e => e.MemberName).Include(e => e.Relation)
                                                            .Include(e => e.Profession)
                                                            .Include(e => e.Address)
                                                            .Include(e => e.ContactDetails)
                                                 .Where(e => e.Id == data).SingleOrDefault();



                            if (reln != null)
                            {
                                if (reln != "")
                                {
                                    var val = db.LookupValue.Find(int.Parse(reln));
                                    db_data.Relation = val;
                                }
                            }
                            if (proffesion != null)
                            {
                                if (proffesion != "")
                                {
                                    var val = db.LookupValue.Find(int.Parse(proffesion));
                                    db_data.Profession = val;
                                }
                            }
                            if (Addrs != null)
                            {
                                if (Addrs != "")
                                {
                                    int AddId = Convert.ToInt32(Addrs);
                                    var val = db.Address//.Include(e => e.Area)
                                        //.Include(e => e.City)
                                        //.Include(e => e.Country)
                                        //.Include(e => e.District)
                                        //.Include(e => e.State)
                                        //.Include(e => e.StateRegion)
                                        //.Include(e => e.Taluka)
                                                        .Where(e => e.Id == AddId).SingleOrDefault();
                                    db_data.Address = val;
                                }
                            }









                            List<ContactDetails> lookupval = new List<ContactDetails>();
                            string Values = form["ContactDetailslist"];

                            //if (Values != null)
                            //{
                            //    var ids = Utility.StringIdsToListIds(Values);
                            //    foreach (var ca in ids)
                            //    {
                            //        var lookup_val = db.ContactDetails.Find(ca);
                            //        lookupval.Add(lookup_val);
                            //        db_data.ContactDetails = lookupval;
                            //    }
                            //}
                            if (Values != null)
                            {
                                if (Values != "")
                                {

                                    //  var ids = Utility.StringIdsToListIds(Lang);
                                    // foreach (var ca in ids)
                                    // {
                                    //var lookup_val = db.ContactDetails.Find(ca);
                                    int contid = Convert.ToInt32(Values);
                                    //lookupLang.Add(lookup_val);
                                    var contactdata = db.ContactDetails//.Include(e => e.Area)
                                        //.Include(e => e.City)
                                        //.Include(e => e.Country)
                                        //.Include(e => e.District)
                                        //.Include(e => e.State)
                                        //.Include(e => e.StateRegion)
                                        //.Include(e => e.Taluka)
                                                       .Where(e => e.Id == contid).SingleOrDefault();
                                    // }
                                    db_data.ContactDetails = contactdata;
                                }
                            }
                            else
                            {
                                db_data.ContactDetails = null;
                            }

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                db.FamilyDetails.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_data.RowVersion;
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_Lookup = db.QualificationDetails.Find(data);
                                TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    FamilyDetails blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.FamilyDetails.Where(e => e.Id == data).Include(e => e.MemberName).Include(e => e.Relation)
                                                            .Include(e => e.Profession)
                                                            .Include(e => e.Address)
                                                            .Include(e => e.ContactDetails)
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
                                    FamilyDetails lk = new FamilyDetails
                                    {
                                        Id = data,
                                        ContactDetails = c.ContactDetails,
                                        DateofBirth = c.DateofBirth,
                                        MemberName = c.MemberName,
                                        Profession = c.Profession,
                                        Relation = c.Relation,
                                        DBTrack = c.DBTrack,
                                        IsHandicap = c.IsHandicap,
                                        HandicapRemark = c.HandicapRemark,
                                        FullDetails = c.FullDetails
                                    };


                                    db.FamilyDetails.Attach(lk);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                    db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];


                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_FamilyDetails DT_Corp = (DT_FamilyDetails)obj;

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
                            var clientValues = (FamilyDetails)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return new { status = true, responseText = "Unable to save changes. The record was deleted by another user." };
                            }
                            else
                            {
                                var databaseValues = (FamilyDetails)databaseEntry.ToObject();
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

                       // bool handicap = Convert.ToBoolean(Ishandicap);
                        FamilyDetails blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.FamilyDetails.Where(e => e.Id == data).Include(e => e.MemberName).Include(e => e.Relation)
                                                .Include(e => e.Profession)
                                                .Include(e => e.Address)
                                                .Include(e => e.ContactDetails)
                                              .SingleOrDefault();
                            originalBlogValues = context.Entry(blog).OriginalValues;
                        }

                        if (proffesion != null)
                        {
                            if (proffesion != "")
                            {
                                var val = db.LookupValue.Find(int.Parse(proffesion));
                                c.Profession = val;
                            }
                        }
                        c.DBTrack = new DBTrack
                        {
                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            Action = "M",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now
                        };
                        if (Addrs != null)
                        {
                            if (Addrs != "")
                            {
                                var val = db.Address.Find(int.Parse(Addrs));
                                c.Address = val;

                                var add = db.FamilyDetails.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                                IList<FamilyDetails> addressdetails = null;
                                if (add.Address != null)
                                {
                                    addressdetails = db.FamilyDetails.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    addressdetails = db.FamilyDetails.Where(x => x.Id == data).ToList();
                                }
                                if (addressdetails != null)
                                {
                                    foreach (var s in addressdetails)
                                    {
                                        s.Address = c.Address;
                                        db.FamilyDetails.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        // await db.SaveChangesAsync(false);
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var addressdetails = db.FamilyDetails.Include(e => e.Address).Where(x => x.Id == data).ToList();
                            foreach (var s in addressdetails)
                            {
                                s.Address = null;
                                db.FamilyDetails.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        if (contact != null)
                        {
                            if (contact != "")
                            {
                                var val = db.ContactDetails.Find(int.Parse(contact));
                                c.ContactDetails = val;

                                var add = db.FamilyDetails.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                                IList<FamilyDetails> addressdetails = null;
                                if (add.Address != null)
                                {
                                    addressdetails = db.FamilyDetails.Where(x => x.ContactDetails.Id == add.Address.Id && x.Id == data).ToList();
                                }
                                else
                                {
                                    addressdetails = db.FamilyDetails.Where(x => x.Id == data).ToList();
                                }
                                if (addressdetails != null)
                                {
                                    foreach (var s in addressdetails)
                                    {
                                        s.ContactDetails = c.ContactDetails;
                                        db.FamilyDetails.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        // await db.SaveChangesAsync(false);
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var addressdetails = db.FamilyDetails.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                            foreach (var s in addressdetails)
                            {
                                s.ContactDetails = null;
                                db.FamilyDetails.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        if (reln != null && reln != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(reln));
                            c.Relation = val;
                        }
                        if (membname != null && membname != "")
                        {
                            int ContId = Convert.ToInt32(membname);
                            var val = db.NameSingle.Where(e => e.Id == ContId).SingleOrDefault();
                            c.MemberName = val;
                        }
                        var db_data = db.FamilyDetails.Where(e => e.Id == data).SingleOrDefault();
                        db_data.DateofBirth = c.DateofBirth;
                        db_data.MemberName = c.MemberName;
                        db_data.Relation = c.Relation;
                        db_data.HandicapRemark = c.HandicapRemark;
                        db_data.Address = c.Address;
                        db_data.ContactDetails = c.ContactDetails;
                        db_data.Profession = c.Profession;
                        db_data.IsHandicap = c.IsHandicap;
                        db_data.DBTrack = c.DBTrack;
                        try
                        {
                            db.FamilyDetails.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            return new { status = true, responseText = "Record Updated" };
                        }
                        catch (Exception e)
                        {

                            throw e;
                        }
                        
                    }
                }
                return new Object[] { };
            }
        }








        public int EditS(string reln, string Addrs, string membname, string Lang, string proffesion, int data, FamilyDetails fd, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (reln != null)
                {
                    if (reln != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(reln));
                        fd.Relation = val;

                        var type = db.FamilyDetails.Include(e => e.Relation).Where(e => e.Id == data).SingleOrDefault();
                        IList<FamilyDetails> typedetails = null;
                        if (type.Relation != null)
                        {
                            typedetails = db.FamilyDetails.Where(x => x.Relation.Id == type.Relation.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.FamilyDetails.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.Relation = fd.Relation;
                            db.FamilyDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.FamilyDetails.Include(e => e.Relation).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.Relation = null;
                            db.FamilyDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.FamilyDetails.Include(e => e.Relation).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.Relation = null;
                        db.FamilyDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (proffesion != null)
                {
                    if (proffesion != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(proffesion));
                        fd.Profession = val;

                        var type = db.FamilyDetails.Include(e => e.Profession).Where(e => e.Id == data).SingleOrDefault();
                        IList<FamilyDetails> typedetails = null;
                        if (type.Profession != null)
                        {
                            typedetails = db.FamilyDetails.Where(x => x.Profession.Id == type.Profession.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.FamilyDetails.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in typedetails)
                        {
                            s.Profession = fd.Profession;
                            db.FamilyDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var proffdetails = db.FamilyDetails.Include(e => e.Profession).Where(x => x.Id == data).ToList();
                        foreach (var s in proffdetails)
                        {
                            s.Profession = null;
                            db.FamilyDetails.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var proffdetails = db.FamilyDetails.Include(e => e.Profession).Where(x => x.Id == data).ToList();
                    foreach (var s in proffdetails)
                    {
                        s.Profession = null;
                        db.FamilyDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.Address.Find(int.Parse(Addrs));
                        //fd.Address = val.Address;                  ///
                        fd.Address = val;
                        var add = db.FamilyDetails.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                        IList<FamilyDetails> addressdetails = null;
                        if (add.Address != null)
                        {
                            addressdetails = db.FamilyDetails.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.FamilyDetails.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Address = fd.Address;
                                db.FamilyDetails.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                // await db.SaveChangesAsync(false);
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var addressdetails = db.FamilyDetails.Include(e => e.Address).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Address = null;
                        db.FamilyDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                if (membname != null)
                {
                    if (membname != "")
                    {
                        var val = db.NameSingle.Find(int.Parse(membname));
                        fd.MemberName = val;         ///

                        var add = db.FamilyDetails.Include(e => e.MemberName).Where(e => e.Id == data).SingleOrDefault();
                        IList<FamilyDetails> memberdetails = null;
                        if (add.MemberName != null)
                        {
                            memberdetails = db.FamilyDetails.Where(x => x.MemberName.Id == add.MemberName.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            memberdetails = db.FamilyDetails.Where(x => x.Id == data).ToList();
                        }
                        if (memberdetails != null)
                        {
                            foreach (var s in memberdetails)
                            {
                                s.MemberName = fd.MemberName;
                                db.FamilyDetails.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                // await db.SaveChangesAsync(false);
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var memberdetails = db.FamilyDetails.Include(e => e.MemberName).Where(x => x.Id == data).ToList();
                    foreach (var s in memberdetails)
                    {
                        s.MemberName = null;
                        db.FamilyDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (Lang != null)
                {
                    if (Lang != "")
                    {
                        var val = db.ContactDetails.Find(int.Parse(Lang));
                        //var val = db.Hobby.Where(e => e.Id == data).SingleOrDefault();

                        var r = (from ca in db.ContactDetails
                                 select new
                                 {
                                     Id = ca.Id,
                                     LookupVal = ca.FullContactDetails
                                 }).Where(e => e.Id == data).Distinct();

                        var add = db.FamilyDetails.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<FamilyDetails> contactdetails = null;
                        if (add.ContactDetails != null)
                        //{
                        //    contactdetails = db.FamilyDetails.Where(x => x.ContactDetails == add.ContactDetails && x.Id == data).ToList();
                        //}
                        //else
                        {
                            contactdetails = db.FamilyDetails.Where(x => x.Id == data).ToList();
                        }
                        if (contactdetails != null)
                        {
                            foreach (var s in contactdetails)
                            {
                                s.ContactDetails = fd.ContactDetails;
                                db.FamilyDetails.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                // await db.SaveChangesAsync(false);
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var contactdetails = db.FamilyDetails.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactdetails)
                    {
                        s.ContactDetails = null;
                        db.FamilyDetails.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurCorp = db.FamilyDetails.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    fd.DBTrack = dbT;
                    FamilyDetails corp = new FamilyDetails()
                    {
                        //  Code = fd.Code,
                        DateofBirth = fd.DateofBirth,
                        Id = data,
                        DBTrack = fd.DBTrack
                    };


                    db.FamilyDetails.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    db.SaveChanges();
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (auth_action == "C")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //Corporate corp = db.Corporate.Find(auth_id);
                        //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                        FamilyDetails corp = db.FamilyDetails.Include(e => e.Address)
                            .Include(e => e.ContactDetails)
                            .Include(e => e.Relation).Include(e => e.Profession).Include(e => e.MemberName).FirstOrDefault(e => e.Id == auth_id);

                        corp.DBTrack = new DBTrack
                        {
                            Action = "C",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = corp.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.FamilyDetails.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();
                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                        DT_FamilyDetails DT_Corp = (DT_FamilyDetails)rtn_Obj;
                        DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                        DT_Corp.Relation_Id = corp.Relation == null ? 0 : corp.Relation.Id;
                        DT_Corp.Profession_Id = corp.Profession == null ? 0 : corp.Profession.Id;
                        DT_Corp.MemberName_Id = corp.MemberName == null ? 0 : corp.MemberName.Id;
                        // DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {

                    FamilyDetails Old_Corp = db.FamilyDetails.Include(e => e.Relation).Include(e => e.Profession).Include(e => e.MemberName)
                                                      .Include(e => e.Address)
                                                      .Include(e => e.ContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();

                    //var W = db.DT_Corporate
                    //.Include(e => e.ContactDetails)
                    //.Include(e => e.Address)
                    //.Include(e => e.BusinessType)
                    //.Include(e => e.ContactDetails)
                    //.Where(e => e.Orig_Id == auth_id && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                    //(e => new
                    //{
                    //    DT_Id = e.Id,
                    //    Code = e.Code == null ? "" : e.Code,
                    //    Name = e.Name == null ? "" : e.Name,
                    //    BusinessType_Val = e.BusinessType.Id == null ? "" : e.BusinessType.LookupVal,
                    //    Address_Val = e.Address.Id == null ? "" : e.Address.FullAddress,
                    //    Contact_Val = e.ContactDetails.Id == null ? "" : e.ContactDetails.FullContactDetails,
                    //}).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                    DT_FamilyDetails Curr_Corp = db.DT_FamilyDetails
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_Corp != null)
                    {
                        FamilyDetails corp = new FamilyDetails();

                        string Corp = Curr_Corp.Relation_Id == null ? null : Curr_Corp.Relation_Id.ToString();
                        string Corp1 = Curr_Corp.Profession_Id == null ? null : Curr_Corp.Profession_Id.ToString();
                        string Addrs = Curr_Corp.Address_Id == null ? null : Curr_Corp.Address_Id.ToString();
                        string membr = Curr_Corp.MemberName_Id == null ? null : Curr_Corp.MemberName_Id.ToString();
                        //string ContactDetails = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();
                        //  corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                        corp.DateofBirth = Curr_Corp.DateofBirth == null ? Old_Corp.DateofBirth : Curr_Corp.DateofBirth;
                        //      corp.Id = auth_id;

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
                                    corp.DBTrack = new DBTrack
                                    {
                                        CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                        CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                        ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                        AuthorizedBy = SessionManager.UserName,
                                        AuthorizedOn = DateTime.Now,
                                        IsModified = false
                                    };


                                    //int a = EditS(Corp, Addrs, Corp1,Lang, membr, auth_id, corp, corp.DBTrack);
                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (FamilyDetails)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (FamilyDetails)databaseEntry.ToObject();
                                    corp.RowVersion = databaseValues.RowVersion;
                                }
                            }

                            return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                        return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                }
                else if (auth_action == "D")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //Corporate corp = db.Corporate.Find(auth_id);
                        FamilyDetails corp = db.FamilyDetails.AsNoTracking().Include(e => e.Address)
                                                                    .Include(e => e.Relation)
                                                                    .Include(e => e.Profession)
                                                                    .Include(e => e.MemberName)
                                                                    .Include(e => e.ContactDetails).FirstOrDefault(e => e.Id == auth_id);

                        Address add = corp.Address;
                        NameSingle nm = corp.MemberName;
                        //ContactDetails conDet = corp.ContactDetails;
                        LookupValue val = corp.Relation;
                        LookupValue val1 = corp.Profession;

                        corp.DBTrack = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.FamilyDetails.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                        DT_FamilyDetails DT_Corp = (DT_FamilyDetails)rtn_Obj;
                        DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                        DT_Corp.Relation_Id = corp.Relation == null ? 0 : corp.Relation.Id;
                        DT_Corp.Profession_Id = corp.Profession == null ? 0 : corp.Profession.Id;
                        DT_Corp.MemberName_Id = corp.MemberName == null ? 0 : corp.MemberName.Id;
                        // DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();
                        db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                    }

                }
                return View();
            }
        }
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                FamilyDetails corporates = db.FamilyDetails.Include(e => e.Address)
                                                   .Include(e => e.ContactDetails).Include(e => e.MemberName).Include(e => e.Profession)
                                                   .Include(e => e.Relation).Where(e => e.Id == data).SingleOrDefault();

                Address add = corporates.Address;
                NameSingle nm = corporates.MemberName;
                //  ContactDetails conDet = corporates.ContactDetails;
                LookupValue val = corporates.Relation;
                LookupValue val1 = corporates.Profession;
                //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                if (corporates.DBTrack.IsModified == true)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                            CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                            IsModified = corporates.DBTrack.IsModified == true ? true : false
                        };
                        corporates.DBTrack = dbT;
                        db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack);
                        DT_FamilyDetails DT_Corp = (DT_FamilyDetails)rtn_Obj;
                        DT_Corp.Address_Id = corporates.Address == null ? 0 : corporates.Address.Id;
                        DT_Corp.Relation_Id = corporates.Relation == null ? 0 : corporates.Relation.Id;
                        DT_Corp.Profession_Id = corporates.Profession == null ? 0 : corporates.Profession.Id;
                        DT_Corp.MemberName_Id = corporates.MemberName == null ? 0 : corporates.MemberName.Id;

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
                    //var selectedRegions = corporates.regions;

                    //using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //{
                    //    if (selectedRegions != null)
                    //    {
                    //        var corpRegion = new HashSet<int>(corporates.regions.Select(e => e.Id));
                    //        if (corpRegion.Count > 0)
                    //        {
                    //            return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                    //            // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                    //        }
                    //    }

                    try
                    {
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now,
                            CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                            CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                            IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                            //AuthorizedBy = SessionManager.UserName,
                            //AuthorizedOn = DateTime.Now
                        };

                        // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                        db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                        var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                        DT_FamilyDetails DT_Corp = (DT_FamilyDetails)rtn_Obj;
                        DT_Corp.Address_Id = add == null ? 0 : add.Id;
                        DT_Corp.Relation_Id = val == null ? 0 : val.Id;
                        DT_Corp.Profession_Id = val1 == null ? 0 : val1.Id;
                        DT_Corp.MemberName_Id = nm == null ? 0 : nm.Id;
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
                        // ts.Complete();
                        return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        //Log the error (uncomment dex variable name and add a line here to write a log.)
                        //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                        //return RedirectToAction("Delete");
                        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                    }
                }
            }
        }
        public ActionResult Editcontactdetails_partial(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var r = (from ca in db.ContactDetails
                         .Where(e => e.Id == data)
                         select new
                         {
                             Id = ca.Id,
                             EmailId = ca.EmailId,
                             FaxNo = ca.FaxNo,
                             Website = ca.Website
                         }).ToList();

                var a = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => e.Id == data).SingleOrDefault();
                var b = a.ContactNumbers;

                var r1 = (from s in b
                          select new
                          {
                              Id = s.Id,
                              FullContactNumbers = s.FullContactNumbers
                          }).ToList();

                TempData["RowVersion"] = db.ContactDetails.Find(data).RowVersion;
                return Json(new object[] { r, r1 }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Createcontactdetails_partial()
        {
            return View("~/Views/Shared/Core/_Contactdetails.cshtml");
        }
        public ActionResult GetContactDetLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactDetails.Include(e => e.ContactNumbers).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ContactDetails.Include(e => e.ContactNumbers).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list1 = db.Corporate.ToList().Select(e => e.ContactDetails);
                var list2 = fall.Except(list1);

                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }
        public ActionResult GetLookupNomineesName(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.NameSingle.ToList();
                IEnumerable<NameSingle> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.NameSingle.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    var list1 = db.NameSingle.ToList();

                    var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetAddressLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                                     .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
                                     .Include(e => e.Taluka).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                                    .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
                                    .Include(e => e.Taluka).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var list1 = db.Corporate.ToList().Select(e => e.Address);
                var list2 = fall.Except(list1);

                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
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
        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string EmpId { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }

        }
        public class GetLvNewReqClass
        {
            public string Emp { get; set; }
            public string DateofBirth { get; set; }
            public string MemberName { get; set; }
            public string Profession { get; set; }
            public string Relation { get; set; }
            public string FullAddress { get; set; }
            public string ContactDetails { get; set; }
            public ChildGetLvNewReqClass RowData { get; set; }
        }
        //public ActionResult GetMyFamilyDeatils()
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var Emp = Convert.ToInt32(SessionManager.EmpId);
        //        var qurey = db.Employee.Include(e => e.FamilyDetails)
        //            .Include(e => e.FamilyDetails.Select(a => a.Address))
        //            .Include(e => e.FamilyDetails.Select(a => a.ContactDetails))
        //            .Include(e => e.FamilyDetails.Select(a => a.ContactDetails.ContactNumbers))
        //            .Include(e => e.FamilyDetails.Select(a => a.MemberName))
        //            .Include(e => e.FamilyDetails.Select(a => a.Profession))
        //            .Where(e => e.Id == Emp)

        //            .SingleOrDefault();

        //        var ListreturnDataClass = new List<returnDataClass>();
        //        if (qurey != null && qurey.FamilyDetails != null && qurey.FamilyDetails.Count > 0)
        //        {
        //            foreach (var item in qurey.FamilyDetails)
        //            {
        //                var DateofBirth = item.DateofBirth != null ? item.DateofBirth.Value.ToShortDateString() : null;
        //                var MemberName = item.MemberName != null ? item.MemberName.FullNameFML : null;
        //                var Profession = item.Profession != null ? item.Profession.LookupVal : null;
        //                var FullAddress = item.Address != null ? item.Address.FullAddress : null;
        //                var ContactDetails = item.ContactDetails != null ? item.ContactDetails.FullContactDetails.ToArray() : null;
        //                var ContactDetailsstring = ContactDetails != null ? string.Join(",", ContactDetails) : null;
        //                ListreturnDataClass.Add(new returnDataClass
        //                {
        //                    EmpId = item.Id,
        //                    val =
        //                    "DateofBirth :" + DateofBirth +
        //                    ", MemberName :" + MemberName +
        //                    ", Profession :" + Profession +
        //                    ", FullAddress :" + FullAddress +
        //                    ", ContactDetails :" + ContactDetailsstring + ""
        //                });
        //            }
        //        }
        //        if (ListreturnDataClass != null && ListreturnDataClass.Count > 0)
        //        {
        //            return Json(new { status = true, data = ListreturnDataClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { status = false, data = ListreturnDataClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public ActionResult GetMyFamilyDeatils()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpId);
                var db_data = db.Employee.Include(e => e.FamilyDetails)
                    .Include(e => e.FamilyDetails.Select(a => a.Address))
                    .Include(e => e.FamilyDetails.Select(a => a.ContactDetails))
                    .Include(e => e.FamilyDetails.Select(a => a.ContactDetails.ContactNumbers))
                    .Include(e => e.FamilyDetails.Select(a => a.MemberName))
                    .Include(e => e.FamilyDetails.Select(a => a.Profession))
                    .Include(e => e.FamilyDetails.Select(a => a.Relation))
                    .Where(e => e.Id == Id)
                    .SingleOrDefault();

                if (db_data != null)
                {
                    List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();
                    returndata.Add(new GetLvNewReqClass
                    {
                        DateofBirth = "DateofBirth",
                        MemberName = "MemberName",
                        Profession = "Profession",
                        Relation = "Relation",
                        FullAddress = "FullAddress",
                        ContactDetails = "ContactDetails",

                    });
                    foreach (var item in db_data.FamilyDetails.OrderByDescending(a => a.Id).ToList())
                    {
                        returndata.Add(new GetLvNewReqClass
                        {
                            RowData = new ChildGetLvNewReqClass
                            {
                                LvNewReq = item.Id.ToString(),
                                EmpId = db_data.Id.ToString(),
                                LvHead_Id = "",
                            },
                            DateofBirth = item.DateofBirth == null ? "" : item.DateofBirth.Value.ToShortDateString(),
                            MemberName = item.MemberName == null ? "" : item.MemberName.FullNameFML,
                            Profession = item.Profession == null ? "" : item.Profession.LookupVal,
                            Relation = item.Relation == null ? "" : item.Relation.LookupVal,
                            FullAddress = item.Address == null ? "" : item.Address.FullAddress.ToString(),
                            ContactDetails = item.ContactDetails == null ? "" : item.ContactDetails.FullContactDetails,
                        });
                        //}
                    }
                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class Company_SelValues
        {
            public string Mem_Id { get; set; }
            public string Mem_FullDetails { get; set; }
            public string Add_Id { get; set; }
            public string Address_FullAddress { get; set; }
            public Array Cont_Id { get; set; }
            public Array FullContactDetails { get; set; }

        }
        public class ContactDetails1
        {
            public Array Cont_Id { get; set; }
            public Array FullContactDetails { get; set; }
        }
        public ActionResult GetNewEmpQualificationReq()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null && EmpIds.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Emps = db.Employee
                    .Where(e => EmpIds.Contains(e.Id))
                    .Include(e => e.FamilyDetails)
                    .Include(e => e.FamilyDetails.Select(a => a.Address))
                    .Include(e => e.FamilyDetails.Select(a => a.ContactDetails))
                    .Include(e => e.FamilyDetails.Select(a => a.MemberName))
                    .Include(e => e.FamilyDetails.Select(a => a.Profession))
                    .Include(e => e.FamilyDetails.Select(a => a.Relation))
                    .Include(e => e.EmpName)
                    .ToList();
                var returnDataClass = new List<returnDataClass>();
                foreach (var item in Emps)
                {
                    if (item.FamilyDetails != null)
                    {
                        foreach (var singleQalification in item.FamilyDetails)
                        {
                            int QId = singleQalification.Id;
                            var dt_data = db.DT_FamilyDetails.Where(e => e.Orig_Id == QId && e.DBTrack.IsAuthorized == 0 && e.DBTrack.TrClosed == false).OrderByDescending(e => e.Id).FirstOrDefault();
                            if (dt_data != null)
                            {
                                returnDataClass.Add(new returnDataClass
                                {
                                    EmpId = dt_data.Id,
                                    Id2 = QId,
                                    val = "EmpCode :" + item.EmpCode + "EmpName :" + item.EmpName.FullNameFML + " " + dt_data.DateofBirth
                                });
                            }

                        }
                    }
                }

                if (returnDataClass != null && returnDataClass.Count > 0)
                {
                    return Json(new { status = true, data = returnDataClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = returnDataClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}
