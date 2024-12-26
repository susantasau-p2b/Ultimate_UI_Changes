///
/// Created by Kapil
///

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



namespace P2BUltimate.Controllers.Core.MainController
{
     [AuthoriseManger]
	public class RegionController : Controller
	{

		//private DataBaseContext db = new DataBaseContext();
		public ActionResult Index()
		{
			return View("~/Views/Core/MainViews/Region/Index.cshtml");
		}
		public ActionResult Partial()
		{
			return View("~/Views/Shared/_Region.cshtml");
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
				var list1 = db.Region.ToList().Select(e => e.ContactDetails);
				var list2 = fall.Except(list1);

				var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();

				return Json(r, JsonRequestBehavior.AllowGet);


			}
			// return View();
		}

		[HttpPost]
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

				var list1 = db.Region.ToList().Select(e => e.Address);
				var list2 = fall.Except(list1);

				//  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
				var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();

				return Json(r, JsonRequestBehavior.AllowGet);
			}
		}

		public ActionResult PopulateDropDownList(string data)
		{
            using (DataBaseContext db = new DataBaseContext())
            {
                int? selected = null;
                if (!string.IsNullOrEmpty(data))
                {
                    var id = Convert.ToInt32(data);
                    selected = db.Region.Where(e => e.Id == id).Select(e => e.Id).First();
                }
                SelectList s = new SelectList(db.Region, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
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
				IEnumerable<Region> Region = null;
				IEnumerable<Region> IE;

				if (gp.IsAutho == true)
				{
					Region = db.Region.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
				}
				else
				{
					FilterSession.Session a = new FilterSession.Session();
					var b = a.Check_flow();
					if (b != null)
					{
						if (b.type == "master")
						{
							Region = db.Region.Where(e => e.Id == b.comp_code).ToList();
						}
						else
						{
							Region = db.Region.ToList();
						}
					}
					else
					{
						Region = db.Region.ToList();
					}
				}
				if (!string.IsNullOrEmpty(gp.searchField))
				{
					IE = Region;
					if (gp.searchOper.Equals("eq"))
					{
                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                              || (e.Name.ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                              ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                        //jsonData = IE.Select(a => new { a.Code, a.Name, a.Id }).Where((e => (e.Id.ToString() == gp.searchString) || (e.Code.ToLower() == gp.searchString.ToLower()) || (e.Name.ToLower() == gp.searchString.ToLower()))).ToList();
					}
					if (pageIndex > 1)
					{
						int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
					}
					totalRecords = IE.Count();
				}
				else
				{
					IE = Region;
					Func<Region, dynamic> orderfuc;
					if (gp.sidx == "Id")
					{
						orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
					}
					else
					{
						orderfuc = (c => gp.sidx == "Code" ? c.Code :
										 gp.sidx == "Name" ? c.Name : "");
					}
					if (gp.sord == "asc")
					{
						IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
					}
					else if (gp.sord == "desc")
					{
						IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
					}
					if (pageIndex > 1)
					{
						int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), Convert.ToString(a.Name), a.Id }).ToList();
					}
					totalRecords = Region.Count();
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
        public ActionResult Create(Region R, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Code = form["Code"] == "0" ? "" : form["Code"];
                    string OpeningDate = form["OpeningDate"] == "0" ? "" : form["OpeningDate"];
                    string Name = form["Name"] == "0" ? "" : form["Name"];
                    string AddressList = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string ContactDetailsList = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    string Incharge_List = form["Incharge_List"] == "0" ? "" : form["Incharge_List"];

                    if (Code != null && Code != "")
                    {
                        var val = Code;
                        R.Code = val;

                    }

                    if (OpeningDate != null && OpeningDate != "")
                    {

                        var val = DateTime.Parse(OpeningDate);

                        R.OpeningDate = val;

                    }

                    if (Name != null)
                    {
                        if (Name != "")
                        {

                            var val = Name;
                            R.Name = val;
                        }
                    }

                    if (AddressList != null)
                    {
                        if (AddressList != "")
                        {
                            int AddId = Convert.ToInt32(AddressList);
                            var val = db.Address//.Include(e => e.Area)
                                //.Include(e => e.City)
                                //.Include(e => e.Country)
                                //.Include(e => e.District)
                                //.Include(e => e.State)
                                //.Include(e => e.StateRegion)
                                //.Include(e => e.Taluka)
                                                .Where(e => e.Id == AddId).SingleOrDefault();
                            R.Address = val;
                        }
                    }
                    if (ContactDetailsList != null)
                    {
                        if (ContactDetailsList != "")
                        {
                            int ContId = Convert.ToInt32(ContactDetailsList);
                            var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            R.ContactDetails = val;
                        }
                    }
                    if (Incharge_List != null && Incharge_List != "0")
                    {
                        if (Incharge_List != "")
                        {
                            int inchargeId = Convert.ToInt32(Incharge_List);
                            var val = db.Employee.Find(inchargeId);
                            R.Incharge = val;
                        }
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.Region.Any(o => o.Code == R.Code))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            }

                            R.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            Region region = new Region()
                            {
                                Code = R.Code == null ? "" : R.Code.Trim(),
                                Name = R.Name == null ? "" : R.Name.Trim(),
                                OpeningDate = R.OpeningDate,
                                Address = R.Address,
                                ContactDetails = R.ContactDetails,
                                Incharge = R.Incharge,
                                DBTrack = R.DBTrack
                            };
                            try
                            {
                                db.Region.Add(region);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, R.DBTrack);
                                DT_Region DT_R = (DT_Region)rtn_Obj;
                                DT_R.Address_Id = R.Address == null ? 0 : R.Address.Id;
                                DT_R.Incharge_Id = R.Incharge == null ? 0 : R.Incharge.Id;
                                DT_R.ContactDetails_Id = R.ContactDetails == null ? 0 : R.ContactDetails.Id;
                                db.Create(DT_R);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);
                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = R.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add("  Unable to create. Try again, and if the problem persists contact your system administrator.. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
		public ActionResult Edit(int data)
		{
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                var Q = db.Region
                    .Include(e => e.ContactDetails)
                    .Include(e => e.Address)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Code = e.Code,
                        Name = e.Name,
                        OpeningDate = e.OpeningDate,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.Region
                  .Include(e => e.ContactDetails)
                    .Include(e => e.Address)
                    .Include(e => e.ContactDetails)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        Address_FullAddress = e.Address.FullAddress == null ? "" : e.Address.FullAddress,
                        Add_Id = e.Address.Id == null ? "" : e.Address.Id.ToString(),
                        Cont_Id = e.ContactDetails.Id == null ? "" : e.ContactDetails.Id.ToString(),
                        FullContactDetails = e.ContactDetails.FullContactDetails == null ? "" : e.ContactDetails.FullContactDetails,
                        Incharge_Id = e.Incharge.Id == null ? "" : e.Incharge.Id.ToString(),
                        InchargetDetails = e.Incharge.FullDetails == null ? "" : e.Incharge.FullDetails,
                    }).ToList();


                var W = db.DT_Region
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         Code = e.Code == null ? "" : e.Code,
                         Name = e.Name == null ? "" : e.Name,

                         Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.Region.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
		}

		public int EditS(string Incharge_List, string Addrs, string ContactDetails, int data, Region R, DBTrack dbT)
		{
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Incharge_List != null)
                {
                    if (Incharge_List != "")
                    {
                        var val = db.Employee.Find(int.Parse(Incharge_List));
                        R.Incharge = val;

                        var type = db.Region.Include(e => e.Incharge).Where(e => e.Id == data).SingleOrDefault();
                        IList<Region> typedetails = null;
                        if (type.Incharge != null)
                        {
                            typedetails = db.Region.Where(x => x.Incharge.Id == type.Incharge.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Region.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.Incharge = R.Incharge;
                            db.Region.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var InchargeTypeDetails = db.Region.Include(e => e.Incharge).Where(x => x.Id == data).ToList();
                        foreach (var s in InchargeTypeDetails)
                        {
                            s.Incharge = null;
                            db.Region.Attach(s);
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
                    var InchargeTypeDetails = db.Region.Include(e => e.Incharge).Where(x => x.Id == data).ToList();
                    foreach (var s in InchargeTypeDetails)
                    {
                        s.Incharge = null;
                        db.Region.Attach(s);
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
                        R.Address = val;

                        var add = db.Region.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                        IList<Region> addressdetails = null;
                        if (add.Address != null)
                        {
                            addressdetails = db.Region.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.Region.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Address = R.Address;
                                db.Region.Attach(s);
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
                    var addressdetails = db.Region.Include(e => e.Address).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Address = null;
                        db.Region.Attach(s);
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
                        R.ContactDetails = val;

                        var add = db.Region.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<Region> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.Region.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.Region.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = R.ContactDetails;
                            db.Region.Attach(s);
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
                    var contactsdetails = db.Region.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.Region.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.Region.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    R.DBTrack = dbT;
                    Region region = new Region()
                    {
                        Code = R.Code,
                        Name = R.Name,
                        OpeningDate = R.OpeningDate,
                        Id = data,
                        DBTrack = R.DBTrack
                    };


                    db.Region.Attach(region);
                    db.Entry(region).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(region).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
		}

		[HttpPost]
        public async Task<ActionResult> EditSave(Region R, int data, FormCollection form) // Edit submit
		{
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Code = form["Code"] == "0" ? "" : form["Code"];
                    string OpeningDate = form["OpeningDate"] == "0" ? "" : form["OpeningDate"];
                    string Name = form["Name"] == "0" ? "" : form["Name"];
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
                    string Incharge_List = form["Incharge_List"] == "0" ? "" : form["Incharge_List"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (Code != null && Code != "")
                    {
                        var val = Code;
                        R.Code = val;

                    }

                    if (OpeningDate != null && OpeningDate != "")
                    {

                        var val = DateTime.Parse(OpeningDate);
                        R.OpeningDate = val;

                    }

                    if (Name != null)
                    {
                        if (Name != "")
                        {

                            var val = Name;
                            R.Name = val;
                        }
                    }


                    if (Incharge_List != null && Incharge_List != "0")
                    {
                        if (Incharge_List != "")
                        {
                            int TnchargeId = Convert.ToInt32(Incharge_List);
                            var val = db.Employee.Find(TnchargeId);
                            R.Incharge = val;
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
                            R.Address = val;
                        }
                    }

                    if (ContactDetails != null)
                    {
                        if (ContactDetails != "")
                        {
                            int ContId = Convert.ToInt32(ContactDetails);
                            var val = db.ContactDetails.Include(e => e.ContactNumbers)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            R.ContactDetails = val;
                        }
                    }


                    if (Auth == false)
                    {


                        //if (ModelState.IsValid)
                        //{
                        //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        //    {
                        //        var Curr_LKValue = db.Region.Find(data);
                        //        TempData["CurrRowVersion"] = Curr_LKValue.RowVersion;
                        //        db.Entry(Curr_LKValue).State = System.Data.Entity.EntityState.Detached;
                        //        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        //        {
                        //            Region blog = blog = null;
                        //            DbPropertyValues originalBlogValues = null;

                        //            using (var context = new DataBaseContext())
                        //            {
                        //                blog = context.Region.Where(e => e.Id == data).SingleOrDefault();
                        //                originalBlogValues = context.Entry(blog).OriginalValues;
                        //            }

                        //            R.DBTrack = new DBTrack
                        //            {
                        //                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                        //                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                        //                Action = "M",
                        //                ModifiedBy = SessionManager.UserName,
                        //                ModifiedOn = DateTime.Now
                        //            };
                        //            var CurCorp = db.Region.Find(data);
                        //            TempData["CurrRowVersion"] = CurCorp.RowVersion;
                        //            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                        //            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        //            {
                        //                R.DBTrack = dbT;                                      
                        //                Region region = new Region()
                        //                {
                        //                    Code = R.Code,
                        //                    Name = R.Name,
                        //                    OpeningDate = R.OpeningDate,
                        //                    Id = data,
                        //                    DBTrack = R.DBTrack
                        //                };


                        //                db.Region.Attach(region);
                        //                db.Entry(region).State = System.Data.Entity.EntityState.Modified;

                        //                // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                        //                //db.SaveChanges();
                        //                db.Entry(region).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //                // DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, val.DBTrack);
                        //                await db.SaveChangesAsync();
                        //                //DisplayTrackedEntities(db.ChangeTracker);
                        //                db.Entry(region).State = System.Data.Entity.EntityState.Detached;
                        //                ts.Complete();
                        //                //return Json(new Object[] { lkval.Id, lkval.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                        //                Msg.Add("  Record Updated");
                        //                return Json(new Utility.JsonReturnClass { Id = region.Id, Val = region.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);



                        //                //db.Region.Attach(region);
                        //                //db.Entry(region).State = System.Data.Entity.EntityState.Modified;
                        //                //db.Entry(region).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //                //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                        //               // return 1;
                        //            }


                                    
                        //            // DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, val.DBTrack);
                        //            //await db.SaveChangesAsync();
                        //            ////DisplayTrackedEntities(db.ChangeTracker);
                        //            //db.Entry(region).State = System.Data.Entity.EntityState.Detached;
                        //            //ts.Complete();
                        //            ////return Json(new Object[] { lkval.Id, lkval.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                        //            //Msg.Add("  Record Updated");
                        //            //return Json(new Utility.JsonReturnClass { Id = lkval.Id, Val = lkval.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //        }
                        //    }
                        //}

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    Region blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.Region.Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    R.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    var m1 = db.Region.Where(e => e.Id == data).ToList();
                                    foreach (var s in m1)
                                    {
                                        // s.AppraisalPeriodCalendar = null;
                                        db.Region.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                    //int a = EditS(Category, PromoPolicy, data, c, c.DBTrack);

                                    if (Incharge_List != null)
                                    {
                                        if (Incharge_List != "")
                                        {
                                            var val = db.Employee.Find(int.Parse(Incharge_List));
                                            R.Incharge = val;

                                            var type = db.Region.Include(e => e.Incharge).Where(e => e.Id == data).SingleOrDefault();
                                            IList<Region> typedetails = null;
                                            if (type.Incharge != null)
                                            {
                                                typedetails = db.Region.Where(x => x.Incharge.Id == type.Incharge.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.Region.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.Incharge = R.Incharge;
                                                db.Region.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var InchargeTypeDetails = db.Region.Include(e => e.Incharge).Where(x => x.Id == data).ToList();
                                            foreach (var s in InchargeTypeDetails)
                                            {
                                                s.Incharge = null;
                                                db.Region.Attach(s);
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
                                        var InchargeTypeDetails = db.Region.Include(e => e.Incharge).Where(x => x.Id == data).ToList();
                                        foreach (var s in InchargeTypeDetails)
                                        {
                                            s.Incharge = null;
                                            db.Region.Attach(s);
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
                                            R.Address = val;

                                            var add = db.Region.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                                            IList<Region> addressdetails = null;
                                            if (add.Address != null)
                                            {
                                                addressdetails = db.Region.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                addressdetails = db.Region.Where(x => x.Id == data).ToList();
                                            }
                                            if (addressdetails != null)
                                            {
                                                foreach (var s in addressdetails)
                                                {
                                                    s.Address = R.Address;
                                                    db.Region.Attach(s);
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
                                        var addressdetails = db.Region.Include(e => e.Address).Where(x => x.Id == data).ToList();
                                        foreach (var s in addressdetails)
                                        {
                                            s.Address = null;
                                            db.Region.Attach(s);
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
                                            R.ContactDetails = val;

                                            var add = db.Region.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                                            IList<Region> contactsdetails = null;
                                            if (add.ContactDetails != null)
                                            {
                                                contactsdetails = db.Region.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                contactsdetails = db.Region.Where(x => x.Id == data).ToList();
                                            }
                                            foreach (var s in contactsdetails)
                                            {
                                                s.ContactDetails = R.ContactDetails;
                                                db.Region.Attach(s);
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
                                        var contactsdetails = db.Region.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                                        foreach (var s in contactsdetails)
                                        {
                                            s.ContactDetails = null;
                                            db.Region.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    var CurCorp = db.Region.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        Region corp = new Region()
                                        {
                                            Code = R.Code,
                                            Name = R.Name,
                                            OpeningDate = R.OpeningDate,
                                            Id = data,
                                            DBTrack = R.DBTrack
                                        };

                                        db.Region.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        // return 1;
                                    }

                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, R.DBTrack);
                                        DT_Region DT_Corp = (DT_Region)obj;
                                        DT_Corp.Incharge_Id = blog.Incharge == null ? 0 : blog.Incharge.Id;
                                        DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                        DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;

                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = R.Id, Val = R.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { c.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (PromoActivity)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (PromoActivity)databaseEntry.ToObject();
                                    R.RowVersion = databaseValues.RowVersion;

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

                            Region blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Region Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Region.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            R.DBTrack = new DBTrack
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

                            Region region = new Region()
                            {
                                Code = R.Code == null ? "" : R.Code.Trim(),
                                Name = R.Name == null ? "" : R.Name.Trim(),
                                OpeningDate = R.OpeningDate,
                                Address = R.Address,
                                ContactDetails = R.ContactDetails,
                                Incharge = R.Incharge,
                                Id = data,
                                DBTrack = R.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, region, "Region", R.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.Region.Where(e => e.Id == data).Include(e => e.Incharge)
                                    .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
                                DT_Region DT_R = (DT_Region)obj;
                                DT_R.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, R.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                DT_R.Incharge_Id = DBTrackFile.ValCompare(Old_Corp.Incharge, R.Incharge); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                DT_R.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, R.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_R);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = R.DBTrack;
                            db.Region.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = R.Id, Val = R.Name, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, R.Name, "Record Updated", JsonRequestBehavior.AllowGet });
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
		public async Task<ActionResult> Delete(int data)
		{
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    Region region = db.Region.Include(e => e.Address)
                                                       .Include(e => e.ContactDetails)
                                                       .Include(e => e.Incharge).Where(e => e.Id == data).SingleOrDefault();

                    Address add = region.Address;
                    ContactDetails conDet = region.ContactDetails;
                    Employee val = region.Incharge;
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (region.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = region.DBTrack.CreatedBy != null ? region.DBTrack.CreatedBy : null,
                                CreatedOn = region.DBTrack.CreatedOn != null ? region.DBTrack.CreatedOn : null,
                                IsModified = region.DBTrack.IsModified == true ? true : false
                            };
                            region.DBTrack = dbT;
                            db.Entry(region).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, region.DBTrack);
                            DT_Region DT_R = (DT_Region)rtn_Obj;
                            DT_R.Address_Id = region.Address == null ? 0 : region.Address.Id;
                            DT_R.Incharge_Id = region.Incharge == null ? 0 : region.Incharge.Id;
                            DT_R.ContactDetails_Id = region.ContactDetails == null ? 0 : region.ContactDetails.Id;
                            db.Create(DT_R);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed.  ");
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
                                    CreatedBy = region.DBTrack.CreatedBy != null ? region.DBTrack.CreatedBy : null,
                                    CreatedOn = region.DBTrack.CreatedOn != null ? region.DBTrack.CreatedOn : null,
                                    IsModified = region.DBTrack.IsModified == true ? false : false
                                };

                                db.Entry(region).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_Region DT_R = (DT_Region)rtn_Obj;
                                DT_R.Address_Id = add == null ? 0 : add.Id;
                                DT_R.Incharge_Id = val == null ? 0 : val.Id;
                                DT_R.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                                db.Create(DT_R);
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
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


         [HttpPost]
         public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
         {
             List<string> Msg = new List<string>();
             using (DataBaseContext db = new DataBaseContext())
             {
                 try
                 {
                     if (auth_action == "C")
                     {
                         using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                         {
                             //Corporate corp = db.Corporate.Find(auth_id);
                             //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                             Region region = db.Region.Include(e => e.Address)
                                 .Include(e => e.ContactDetails)
                                 .Include(e => e.Incharge).FirstOrDefault(e => e.Id == auth_id);

                             region.DBTrack = new DBTrack
                             {
                                 Action = "C",
                                 ModifiedBy = region.DBTrack.ModifiedBy != null ? region.DBTrack.ModifiedBy : null,
                                 CreatedBy = region.DBTrack.CreatedBy != null ? region.DBTrack.CreatedBy : null,
                                 CreatedOn = region.DBTrack.CreatedOn != null ? region.DBTrack.CreatedOn : null,
                                 IsModified = region.DBTrack.IsModified == true ? false : false,
                                 AuthorizedBy = SessionManager.UserName,
                                 AuthorizedOn = DateTime.Now
                             };

                             db.Region.Attach(region);
                             db.Entry(region).State = System.Data.Entity.EntityState.Modified;
                             db.Entry(region).OriginalValues["RowVersion"] = TempData["RowVersion"];
                             //db.SaveChanges();
                             var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, region.DBTrack);
                             DT_Region DT_R = (DT_Region)rtn_Obj;
                             DT_R.Address_Id = region.Address == null ? 0 : region.Address.Id;
                             DT_R.Incharge_Id = region.Incharge == null ? 0 : region.Incharge.Id;
                             DT_R.ContactDetails_Id = region.ContactDetails == null ? 0 : region.ContactDetails.Id;
                             db.Create(DT_R);
                             await db.SaveChangesAsync();

                             ts.Complete();
                             Msg.Add("  Record Authorised");
                             return Json(new Utility.JsonReturnClass { Id = region.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                             // return Json(new Object[] { region.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                         }
                     }
                     else if (auth_action == "M")
                     {

                         Region Old_Corp = db.Region.Include(e => e.Incharge)
                                                           .Include(e => e.Address)
                                                           .Include(e => e.ContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();


                         DT_Region Curr_Corp = db.DT_Region
                                                     .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                     .OrderByDescending(e => e.Id)
                                                     .FirstOrDefault();

                         if (Curr_Corp != null)
                         {
                             Region rg = new Region();

                             string Corp = Curr_Corp.Incharge_Id == null ? null : Curr_Corp.Incharge_Id.ToString();
                             string Addrs = Curr_Corp.Address_Id == null ? null : Curr_Corp.Address_Id.ToString();
                             string ContactDetails = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();
                             rg.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                             rg.Code = Curr_Corp.Code == null ? Old_Corp.Code : Curr_Corp.Code;
                             //      corp.Id = auth_id;

                             if (ModelState.IsValid)
                             {
                                 try
                                 {

                                     //DbContextTransaction transaction = db.Database.BeginTransaction();

                                     using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                     {
                                         // db.Configuration.AutoDetectChangesEnabled = false;
                                         rg.DBTrack = new DBTrack
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




                                         await db.SaveChangesAsync();

                                         ts.Complete();
                                         Msg.Add("  Record Authorised");
                                         return Json(new Utility.JsonReturnClass { Id = rg.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                         //return Json(new Object[] { rg.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
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
                                         //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                     }
                                     else
                                     {
                                         var databaseValues = (Corporate)databaseEntry.ToObject();
                                         rg.RowVersion = databaseValues.RowVersion;
                                     }
                                 }
                                 Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                 return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                 //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                             }
                         }
                         else
                         {
                             Msg.Add("  Data removed.  ");
                             return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                         }
                         //return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                     }
                     else if (auth_action == "D")
                     {
                         using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                         {
                             //Corporate corp = db.Corporate.Find(auth_id);
                             Region region = db.Region.AsNoTracking().Include(e => e.Address)
                                                                         .Include(e => e.Incharge)
                                                                         .Include(e => e.ContactDetails).FirstOrDefault(e => e.Id == auth_id);

                             Address add = region.Address;
                             ContactDetails conDet = region.ContactDetails;
                             Employee val = region.Incharge;

                             region.DBTrack = new DBTrack
                             {
                                 Action = "D",
                                 ModifiedBy = region.DBTrack.ModifiedBy != null ? region.DBTrack.ModifiedBy : null,
                                 CreatedBy = region.DBTrack.CreatedBy != null ? region.DBTrack.CreatedBy : null,
                                 CreatedOn = region.DBTrack.CreatedOn != null ? region.DBTrack.CreatedOn : null,
                                 IsModified = false,
                                 AuthorizedBy = SessionManager.UserName,
                                 AuthorizedOn = DateTime.Now
                             };

                             db.Region.Attach(region);
                             db.Entry(region).State = System.Data.Entity.EntityState.Deleted;


                             var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, region.DBTrack);
                             DT_Region DT_R = (DT_Region)rtn_Obj;
                             DT_R.Address_Id = region.Address == null ? 0 : region.Address.Id;
                             DT_R.Incharge_Id = region.Incharge == null ? 0 : region.Incharge.Id;
                             DT_R.ContactDetails_Id = region.ContactDetails == null ? 0 : region.ContactDetails.Id;
                             db.Create(DT_R);
                             await db.SaveChangesAsync();
                             db.Entry(region).State = System.Data.Entity.EntityState.Detached;
                             ts.Complete();
                             //return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                             Msg.Add(" Record Authorised ");
                             return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

	}
}
