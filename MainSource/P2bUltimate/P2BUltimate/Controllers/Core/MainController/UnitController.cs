using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text;
using P2BUltimate.Models;
using System.Threading.Tasks;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class UnitController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Unit/Index.cshtml");
        }

        //private DataBaseContext db = new DataBaseContext();

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
                IEnumerable<Unit> Location = null;
                IEnumerable<Unit> IE;
                if (gp.IsAutho == true)
                {
                    Location = db.Company.Include(e => e.Unit).SelectMany(e => e.Unit).AsNoTracking().ToList();
                }
                else
                {
                    FilterSession.Session a = new FilterSession.Session();
                    var b = a.Check_flow();
                    if (b != null)
                    {
                        if (b.type == "master")
                        {
                            Location = db.Company.Include(e => e.Unit).Where(e => e.Id == b.comp_code).SelectMany(e => e.Unit).ToList();
                        }
                        else
                        {
                            Location = db.Unit.ToList();
                        }
                    }
                    else
                    {
                        Location = db.Unit.ToList();
                    }
                }

                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Location;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                               || (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                               ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id, Convert.ToString(a.OpeningDate) }).ToList();

                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id, Convert.ToString(a.OpeningDate) }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Location;
                    Func<Unit, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code :
                                                               gp.sidx == "Name" ? c.Name :

                                         "");
                    }
                    //Func<Unit, string> orderfuc = (c =>
                    //                                           gp.sidx == "ID" ? c.Id.ToString() :
                    //                                           gp.sidx == "Code" ? c.Code :
                    //                                           gp.sidx == "Name" ? c.Name :

                    //                                            "");
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id, Convert.ToString(a.OpeningDate) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id, Convert.ToString(a.OpeningDate) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id, Convert.ToString(a.OpeningDate) }).ToList();
                    }
                    totalRecords = Location.Count();
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
        // [ValidateAntiForgeryToken]
        public ActionResult Create(Unit U, FormCollection form) //Create submit
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
                    string Type = form["Type_drop"] == "0" ? "" : form["Type_drop"];
                    string Code = form["Code"] == "0" ? "" : form["Code"];
                    string Name = form["Name"] == "0" ? "" : form["Name"];
                    string Incharge = form["Inchargelist"] == "0" ? "" : form["Inchargelist"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    string InchargeContactNos = form["InchargeContactNoslist"] == "0" ? "" : form["InchargeContactNoslist"];
                    if (Type != null)
                    {
                        if (Type != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "304").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Type)).FirstOrDefault();  ///db.LookupValue.Find(int.Parse(Type));
                            U.Type = val;
                        }
                    }
                    if (Code != null)
                    {
                        if (Code != "")
                        {
                            var val = Code;
                            U.Code = val;
                        }
                    }
                    if (Name != null)
                    {
                        if (Name != "")
                        {
                            var val = Name;
                            U.Name = val;
                        }
                    }


                    if (ContactDetails != null)
                    {
                        if (ContactDetails != "")
                        {
                            int ContId = Convert.ToInt32(ContactDetails);
                            var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            U.ContactDetails = val;
                        }
                    }
                    if (Incharge != null)
                    {
                        if (Incharge != "")
                        {
                            int ContId = Convert.ToInt32(Incharge);
                            var val = db.Employee.Where(e => e.Id == ContId).SingleOrDefault();
                            U.Incharge = val;
                        }
                    }
                    if (InchargeContactNos != null)
                    {
                        List<int> IDs = InchargeContactNos.Split(',').Select(e => int.Parse(e)).ToList();
                        foreach (var k in IDs)
                        {
                            var value = db.ContactNumbers.Find(k);
                            U.InchargeContactNos = new List<ContactNumbers>();
                            U.InchargeContactNos.Add(value);
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.Unit.Any(o => o.Code == U.Code))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            U.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            Unit unit = new Unit()
                            {
                                Code = U.Code == null ? "" : U.Code.Trim(),
                                Name = U.Name == null ? "" : U.Name.Trim(),
                                Type = U.Type,
                                Incharge = U.Incharge,
                                ContactDetails = U.ContactDetails,
                                InchargeContactNos = U.InchargeContactNos,
                                DBTrack = U.DBTrack

                            };
                            try
                            {
                                db.Unit.Add(unit);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, U.DBTrack);
                                DT_Unit DT_U = (DT_Unit)rtn_Obj;
                                DT_U.Incharge_Id = U.Incharge == null ? 0 : U.Incharge.Id;
                                DT_U.Type_Id = U.Type == null ? 0 : U.Type.Id;
                                DT_U.ContactDetails_Id = U.ContactDetails == null ? 0 : U.ContactDetails.Id;
                                db.Create(DT_U);
                                db.SaveChanges();



                                if (Company != null)
                                {
                                    var Objjob = new List<Unit>();
                                    Objjob.Add(unit);
                                    Company.Unit = Objjob;
                                    db.Company.Attach(Company);
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(Company).State = System.Data.Entity.EntityState.Detached;

                                }
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = U.Id });
                            }
                            catch (DataException /* dex */)
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
        public class ContactNumbersUnits
        {
            public Array InchargeContactNos_Id { get; set; }
            public Array InchargeContactNos_Details { get; set; }
            public String Incharge_Id { get; set; }
            public String Incharge_Fulldetails { get; set; }
            public String Cont_Id { get; set; }
            public String FullContactDetails { get; set; }
        }


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Unit
                    .Include(e => e.ContactDetails)
                    .Include(e => e.Incharge)
                    .Include(e => e.InchargeContactNos)
                    .Include(e => e.ContactDetails)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Code = e.Code,
                        Name = e.Name,
                        type_Id = e.Type.Id == null ? 0 : e.Type.Id,
                        Action = e.DBTrack.Action
                    }).ToList();
                List<ContactNumbersUnits> contno = new List<ContactNumbersUnits>();

                var add_data = db.Unit
                  .Include(e => e.ContactDetails)
                    .Include(e => e.Incharge)
                    .Include(e => e.InchargeContactNos)
                    .Where(e => e.Id == data).ToList();

                foreach (var i in add_data)
                {
                    contno.Add(new ContactNumbersUnits
                    {
                        Cont_Id = i.ContactDetails == null ? "" : i.ContactDetails.Id.ToString(),
                        FullContactDetails = i.ContactDetails == null ? "" : i.ContactDetails.FullContactDetails,
                        Incharge_Id = i.Incharge == null ? "" : i.Incharge.Id.ToString(),
                        Incharge_Fulldetails = i.Incharge == null ? "" : i.Incharge.EmpCode
                    });
                }

                var k = db.Unit.Include(e => e.InchargeContactNos).Where(e => e.Id == data).Select(e => new { e.Id, e.InchargeContactNos }).ToList();
                foreach (var val in k)
                {
                    contno.Add(new ContactNumbersUnits
                    {
                        InchargeContactNos_Id = val.InchargeContactNos.Select(e => e.Id.ToString()).ToArray(),
                        InchargeContactNos_Details = val.InchargeContactNos.Select(e => e.FullContactNumbers).ToArray()
                    });
                }

                var W = db.DT_Unit
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Code = e.Code == null ? "" : e.Code,
                         Name = e.Name == null ? "" : e.Name,
                         Type_Val = e.Type_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.Type_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),

                         Incharge_Val = e.Incharge_Id == 0 ? "" : db.NameDetails.Where(x => x.Id == e.Incharge_Id).Select(x => x.FullDetails).FirstOrDefault(),
                         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.Unit.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, contno, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(Unit c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Type = form["Type_drop"] == "0" ? "" : form["Type_drop"];
                    string Code = form["Code"] == "0" ? "" : form["Code"];
                    string Name = form["Name"] == "0" ? "" : form["Name"];
                    string Incharge = form["Inchargelist"] == "0" ? "" : form["Inchargelist"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    string InchargeContactNos = form["InchargeContactNoslist"] == "0" ? "" : form["InchargeContactNoslist"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (Type != null)
                    {
                        if (Type != "")
                        {
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "304").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Type)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Type));
                            c.Type = val;
                            c.Type_Id = int.Parse(Type);
                        }
                    }
                    if (ContactDetails != null)
                    {
                        if (ContactDetails != "")
                        {

                            c.ContactDetails_Id = int.Parse(ContactDetails);
                        }
                    }
                    if (Incharge != null)
                    {
                        if (Incharge != "")
                        {

                            c.Incharge_Id = int.Parse(Incharge);
                        }
                    }

                    if (Code != null)
                    {
                        if (Code != "")
                        {
                            var val = Code;
                            c.Code = val;
                        }
                    }
                    if (Name != null)
                    {
                        if (Name != "")
                        {
                            var val = Name;
                            c.Name = val;
                        }
                    }


                    if (ContactDetails != null)
                    {
                        if (ContactDetails != "")
                        {
                            int ContId = Convert.ToInt32(ContactDetails);
                            var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.ContactDetails = val;
                        }
                    }



                    //if (payscaletype != null)
                    //{
                    //    if (payscaletype != "")
                    //    {
                    //        int ContId = Convert.ToInt32(payscaletype);
                    //        var val = db.PayScale.Where(e => e.Id == ContId).SingleOrDefault();
                    //        c.PayScale = val;
                    //    }
                    //}

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    Unit typedetails = null;

                                    if (Type != null & Type != "")
                                    {
                                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "304").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Type)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Type));
                                        c.Type = val;

                                        var type = db.Unit.Include(e => e.Type).Where(e => e.Id == data).SingleOrDefault();

                                        if (type.Type != null)
                                        {
                                            typedetails = db.Unit.Where(x => x.Type.Id == type.Type.Id && x.Id == data).SingleOrDefault();
                                        }
                                        else
                                        {
                                            typedetails = db.Unit.Where(x => x.Id == data).SingleOrDefault();
                                        }
                                        typedetails.Type = c.Type;
                                    }
                                    else
                                    {
                                        typedetails = db.Unit.Include(e => e.Type).Where(x => x.Id == data).SingleOrDefault();
                                        typedetails.Type = null;
                                    }

                                    if (ContactDetails != null & ContactDetails != "")
                                    {
                                        var val = db.ContactDetails.Find(int.Parse(ContactDetails));
                                        c.ContactDetails = val;

                                        var type = db.Unit.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();

                                        if (type.ContactDetails != null)
                                        {
                                            typedetails = db.Unit.Where(x => x.ContactDetails.Id == type.ContactDetails.Id && x.Id == data).SingleOrDefault();
                                        }
                                        else
                                        {
                                            typedetails = db.Unit.Where(x => x.Id == data).SingleOrDefault();
                                        }
                                        typedetails.ContactDetails = c.ContactDetails;
                                    }
                                    else
                                    {
                                        typedetails = db.Unit.Include(e => e.ContactDetails).Where(x => x.Id == data).SingleOrDefault();
                                        typedetails.ContactDetails = null;
                                    }

                                    if (Incharge != null & Incharge != "")
                                    {
                                        var val = db.Employee.Find(int.Parse(Incharge));
                                        c.Incharge = val;
                                        c.Incharge_Id = int.Parse(Incharge);
                                        var type = db.Unit.Include(e => e.Incharge).Where(e => e.Id == data).SingleOrDefault();

                                        if (type.Incharge != null)
                                        {
                                            typedetails = db.Unit.Where(x => x.Incharge.Id == type.Incharge.Id && x.Id == data).SingleOrDefault();
                                        }
                                        else
                                        {
                                            typedetails = db.Unit.Where(x => x.Id == data).SingleOrDefault();
                                        }
                                        typedetails.Incharge = c.Incharge;
                                    }
                                    else
                                    {
                                        typedetails = db.Unit.Where(x => x.Id == data).SingleOrDefault();
                                        typedetails.Incharge = null;
                                    }

                                    List<ContactNumbers> ContactNumbers = new List<ContactNumbers>();
                                    typedetails = db.Unit.Include(e => e.InchargeContactNos).Where(e => e.Id == data).SingleOrDefault();
                                    if (InchargeContactNos != null && InchargeContactNos != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(InchargeContactNos);
                                        foreach (var ca in ids)
                                        {
                                            var ContactNumbers_val = db.ContactNumbers.Find(ca);
                                            ContactNumbers.Add(ContactNumbers_val);
                                            typedetails.InchargeContactNos = ContactNumbers;
                                        }
                                    }
                                    else
                                    {
                                        typedetails.InchargeContactNos = null;
                                    }




                                    db.Unit.Attach(typedetails);
                                    db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = typedetails.RowVersion;
                                    db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;


                                    var Curr_OBJ = db.Unit.Find(data);
                                    TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                    db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {
                                        Unit blog = blog = null;
                                        DbPropertyValues originalBlogValues = null;

                                        using (var context = new DataBaseContext())
                                        {
                                            blog = context.Unit.Where(e => e.Id == data).SingleOrDefault();
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
                                        Unit lk = new Unit
                                        {
                                            Code = c.Code == null ? "" : c.Code,
                                            Name = c.Name == null ? "" : c.Name,
                                            Type_Id = c.Type_Id,
                                            Incharge_Id = c.Incharge_Id,
                                            ContactDetails_Id = c.ContactDetails_Id,
                                            InchargeContactNos = c.InchargeContactNos,
                                            Id = data,
                                            DBTrack = c.DBTrack,

                                        };


                                        db.Unit.Attach(lk);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                        // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                        //db.SaveChanges();
                                        db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_Unit DT_Corp = (DT_Unit)obj;
                                        DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                        //DT_Corp.PayScale_Id = blog.PayScale == null ? 0 : blog.PayScale.Id;
                                        DT_Corp.Incharge_Id = blog.Incharge == null ? 0 : blog.Incharge.Id;
                                        db.Create(DT_Corp);
                                        // db.SaveChanges();
                                        ////DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, look.DBTrack);
                                        await db.SaveChangesAsync();
                                        //DisplayTrackedEntities(db.ChangeTracker);
                                        db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { lk.Id, lk.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                    }
                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Unit)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    var databaseValues = (Unit)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            Unit blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Unit Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Unit.Where(e => e.Id == data).SingleOrDefault();
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
                            Unit unit = new Unit()
                            {
                                Code = c.Code,
                                Name = c.Name,
                                Incharge_Id = c.Incharge_Id,
                                InchargeContactNos = c.InchargeContactNos,
                                Id = data,
                                Type_Id = c.Type_Id,
                                ContactDetails_Id = c.ContactDetails_Id,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, unit, "Unit", c.DBTrack);
                                Old_Corp = context.Unit.Where(e => e.Id == data).Include(e => e.InchargeContactNos)
                                    .Include(e => e.Incharge).Include(e => e.ContactDetails).SingleOrDefault();
                                DT_Unit DT_Corp = (DT_Unit)obj;
                                DT_Corp.Incharge_Id = DBTrackFile.ValCompare(Old_Corp.Incharge, c.Incharge);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //DT_Corp.PayScale_Id = DBTrackFile.ValCompare(Old_Corp.PayScale, c.PayScale); //Old_Corp.PayScale == c.PayScale ? 0 : Old_Corp.PayScale == null && c.PayScale != null ? c.PayScale.Id : Old_Corp.PayScale.Id;
                                DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);

                            }
                            blog.DBTrack = c.DBTrack;
                            db.Unit.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { blog.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
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



        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    Unit unit = db.Unit.Include(e => e.InchargeContactNos)
                                                       .Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();


                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (unit.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = unit.DBTrack.CreatedBy != null ? unit.DBTrack.CreatedBy : null,
                                CreatedOn = unit.DBTrack.CreatedOn != null ? unit.DBTrack.CreatedOn : null,
                                IsModified = unit.DBTrack.IsModified == true ? true : false
                            };
                            unit.DBTrack = dbT;
                            db.Entry(unit).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, unit.DBTrack);
                            DT_Unit DT_U = (DT_Unit)rtn_Obj;
                            DT_U.ContactDetails_Id = unit.ContactDetails == null ? 0 : unit.ContactDetails.Id;
                            DT_U.Incharge_Id = unit.Incharge == null ? 0 : unit.Incharge.Id;

                            db.Create(DT_U);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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
                                    CreatedBy = unit.DBTrack.CreatedBy != null ? unit.DBTrack.CreatedBy : null,
                                    CreatedOn = unit.DBTrack.CreatedOn != null ? unit.DBTrack.CreatedOn : null,
                                    IsModified = unit.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(unit).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_Unit DT_U = (DT_Unit)rtn_Obj;
                                DT_U.ContactDetails_Id = unit.ContactDetails == null ? 0 : unit.Id;
                                DT_U.Incharge_Id = unit.Incharge == null ? 0 : unit.Id;
                                db.Create(DT_U);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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


        //EditS(Type, Incharge, ContactDetails, data, U, U.DBTrack);

        public int EditS(string Type, string Incharge, string ContactDetails, int data, Unit U, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Type != null)
                {
                    if (Type != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Type));
                        U.Type = val;

                        var type = db.Unit.Include(e => e.Type).Where(e => e.Id == data).SingleOrDefault();
                        IList<Unit> typedetails = null;
                        if (type.Type != null)
                        {
                            typedetails = db.Unit.Where(x => x.Type.Id == type.Type.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Unit.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.Type = U.Type;
                            db.Unit.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var TypeDetails = db.Unit.Include(e => e.Type).Where(x => x.Id == data).ToList();
                        foreach (var s in TypeDetails)
                        {
                            s.Type = null;
                            db.Unit.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var TypeDetails = db.Unit.Include(e => e.Type).Where(x => x.Id == data).ToList();
                    foreach (var s in TypeDetails)
                    {
                        s.Type = null;
                        db.Unit.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (Incharge != null)
                {
                    if (Incharge != "")
                    {
                        var val = db.Employee.Find(int.Parse(Incharge));
                        U.Incharge = val;

                        var add = db.Unit.Include(e => e.Incharge).Where(e => e.Id == data).SingleOrDefault();
                        IList<Unit> Inchargedetails = null;
                        if (add.Incharge != null)
                        {
                            Inchargedetails = db.Unit.Where(x => x.Incharge.Id == add.Incharge.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            Inchargedetails = db.Unit.Where(x => x.Id == data).ToList();
                        }
                        if (Inchargedetails != null)
                        {
                            foreach (var s in Inchargedetails)
                            {
                                s.Incharge = U.Incharge;
                                db.Unit.Attach(s);
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
                    var Inchargedetails = db.Unit.Include(e => e.Incharge).Where(x => x.Id == data).ToList();
                    foreach (var s in Inchargedetails)
                    {
                        s.Incharge = null;
                        db.Unit.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (ContactDetails != null)
                {
                    if (ContactDetails != "")
                    {
                        var val = db.ContactDetails.Find(int.Parse(ContactDetails));
                        U.ContactDetails = val;

                        var add = db.Unit.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<Unit> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.Unit.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.Unit.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = U.ContactDetails;
                            db.Unit.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var contactsdetails = db.Unit.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.Unit.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                //if (InchargeContactNos != null)
                //{
                //    if (InchargeContactNos != "")
                //    {

                //        List<int> IDs = InchargeContactNos.Split(',').Select(e => int.Parse(e)).ToList();
                //        foreach (var k in IDs)
                //        {
                //            var value = db.ContactNumbers.Find(k);
                //            U.InchargeContactNos = new List<ContactNumbers>();
                //            U.InchargeContactNos.Add(value);
                //        }
                //    }
                //}
                //else
                //{
                //    var InchargeContactNosdetails = db.Unit.Include(e => e.InchargeContactNos).Where(x => x.Id == data).ToList();
                //    foreach (var s in InchargeContactNosdetails)
                //    {
                //        s.InchargeContactNos = null;
                //        db.Unit.Attach(s);
                //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //        //await db.SaveChangesAsync();
                //        db.SaveChanges();
                //        TempData["RowVersion"] = s.RowVersion;
                //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //    }
                //}

                var CurCorp = db.Unit.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    U.DBTrack = dbT;
                    Unit unit = new Unit()
                    {
                        Code = U.Code,
                        Name = U.Name,
                        Id = data,
                        DBTrack = U.DBTrack
                    };


                    db.Unit.Attach(unit);
                    db.Entry(unit).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(unit).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }



        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            Unit unit = db.Unit.Include(e => e.ContactDetails)
                                .Include(e => e.InchargeContactNos)
                                .Include(e => e.Incharge).FirstOrDefault(e => e.Id == auth_id);

                            unit.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = unit.DBTrack.ModifiedBy != null ? unit.DBTrack.ModifiedBy : null,
                                CreatedBy = unit.DBTrack.CreatedBy != null ? unit.DBTrack.CreatedBy : null,
                                CreatedOn = unit.DBTrack.CreatedOn != null ? unit.DBTrack.CreatedOn : null,
                                IsModified = unit.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Unit.Attach(unit);
                            db.Entry(unit).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(unit).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, unit.DBTrack);
                            DT_Unit DT_U = (DT_Unit)rtn_Obj;
                            DT_U.ContactDetails_Id = unit.ContactDetails == null ? 0 : unit.ContactDetails.Id;
                            db.Create(DT_U);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = unit.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { unit.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        Unit Old_U = db.Unit.Include(e => e.ContactDetails)
                                                          .Include(e => e.Incharge)
                                                          .Include(e => e.InchargeContactNos).Where(e => e.Id == auth_id).SingleOrDefault();

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

                        DT_Unit Curr_U = db.DT_Unit
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_U != null)
                        {
                            Unit unit = new Unit();



                            string ContactDetails = Curr_U.ContactDetails_Id == null ? null : Curr_U.ContactDetails_Id.ToString();
                            unit.Name = Curr_U.Name == null ? Old_U.Name : Curr_U.Name;
                            unit.Code = Curr_U.Code == null ? Old_U.Code : Curr_U.Code;
                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        unit.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_U.DBTrack.CreatedBy == null ? null : Old_U.DBTrack.CreatedBy,
                                            CreatedOn = Old_U.DBTrack.CreatedOn == null ? null : Old_U.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_U.DBTrack.ModifiedBy == null ? null : Old_U.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_U.DBTrack.ModifiedOn == null ? null : Old_U.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };
                                        await db.SaveChangesAsync();
                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = unit.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { unit.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Corporate)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    }
                                    else
                                    {
                                        var databaseValues = (Corporate)databaseEntry.ToObject();
                                        unit.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                        {
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            Unit unit = db.Unit.AsNoTracking().Include(e => e.ContactDetails)
                                                                        .Include(e => e.Incharge)
                                                                        .Include(e => e.InchargeContactNos).FirstOrDefault(e => e.Id == auth_id);



                            unit.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = unit.DBTrack.ModifiedBy != null ? unit.DBTrack.ModifiedBy : null,
                                CreatedBy = unit.DBTrack.CreatedBy != null ? unit.DBTrack.CreatedBy : null,
                                CreatedOn = unit.DBTrack.CreatedOn != null ? unit.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Unit.Attach(unit);
                            db.Entry(unit).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, unit.DBTrack);
                            DT_Unit DT_U = (DT_Unit)rtn_Obj;
                            DT_U.ContactDetails_Id = unit.ContactDetails == null ? 0 : unit.ContactDetails.Id;
                            db.Create(DT_U);
                            await db.SaveChangesAsync();
                            db.Entry(unit).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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


        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var changedEntries = db.ChangeTracker.Entries()
                    .Where(x => x.State != System.Data.Entity.EntityState.Unchanged).ToList();

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Modified))
                {
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Added))
                {
                    entry.State = System.Data.Entity.EntityState.Detached;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Deleted))
                {
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

            }
        }
    }
}
