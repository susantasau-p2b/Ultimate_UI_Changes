using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Process;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2BUltimate.Security;
using System.Data.Entity.Core.Objects;
using System.Web.Script.Serialization;
using System.Collections.Specialized;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;

namespace P2BUltimate.Controllers.Core.MainController
{
    public class EmailSendDataController : Controller
    {
        //
        // GET: /EmailSendData/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/EmailSendData/Index.cshtml");
        }

        //public class P2BGridData
        //{
        //    public int Id { get; set; }
        //    public string Code { get; set; }
        //    public string Name { get; set; }
        //    public string Row { get; set; }
        //    public double TotalEarning { get; set; }
        //    public double TotalDeduction { get; set; }
        //    public double TotalNet { get; set; }
        //    public string ReleaseDate { get; set; }
        //}

        public class EmailAddress1
        {
            public int Address_Id { get; set; }
            public string Address_val { get; set; }

        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            List<string> Msg = new List<string>();

            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EmailSendData> EmailSendData = null;
                if (gp.IsAutho == true)
                {
                    EmailSendData = db.EmailSendData.Include(e => e.EmailAction).Include(e => e.EmailTemplateCode).Include(e => e.EntityRefCode).Where(e => e.DBTrack.IsModified == true && e.EmailAction.LookupVal.ToUpper() == "PREVIEW").AsNoTracking().ToList();
                }
                else
                {
                    EmailSendData = db.EmailSendData.Include(e => e.EmailAction).Include(e => e.EmailTemplateCode).Include(e => e.EntityRefCode).Where(e => e.EmailAction.LookupVal.ToUpper() == "PREVIEW").AsNoTracking().ToList();
                }

                IEnumerable<EmailSendData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmailSendData;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmailAction != null ? e.EmailAction.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.EmailTemplateCode != null ? e.EmailTemplateCode.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.EntityRefCode != null ? e.EntityRefCode.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.EmailAction != null ? a.EmailAction.LookupVal : "", a.EmailTemplateCode != null ? a.EmailTemplateCode.LookupVal : "", a.EntityRefCode != null ? a.EntityRefCode.LookupVal : "", a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmailAction != null ? a.EmailAction.LookupVal : "", a.EmailTemplateCode != null ? a.EmailTemplateCode.LookupVal : "", a.EntityRefCode != null ? a.EntityRefCode.LookupVal : "", a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmailSendData;
                    Func<EmailSendData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c =>
                                         gp.sidx == "EmailAction" ? c.EmailAction.LookupVal :
                                         gp.sidx == "EmailTemplateCode" ? c.EmailTemplateCode.LookupVal :
                                         gp.sidx == "EntityRefCode" ? c.EntityRefCode.LookupVal : "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmailAction != null ? a.EmailAction.LookupVal : "", a.EmailTemplateCode != null ? a.EmailTemplateCode.LookupVal : "", a.EntityRefCode != null ? a.EntityRefCode.LookupVal : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmailAction != null ? a.EmailAction.LookupVal : "", a.EmailTemplateCode != null ? a.EmailTemplateCode.LookupVal : "", a.EntityRefCode != null ? a.EntityRefCode.LookupVal : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmailAction != null ? a.EmailAction.LookupVal : "", a.EmailTemplateCode != null ? a.EmailTemplateCode.LookupVal : "", a.EntityRefCode != null ? a.EntityRefCode.LookupVal : "", a.Id }).ToList();
                    }
                    totalRecords = EmailSendData.Count();
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
        public ActionResult Edit(int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.EmailSendData.Include(e => e.EmailCompose)
                    .Include(e => e.EmailCompose.ToAddresses)
                    .Include(e => e.EmailAction)
                    .Include(e => e.EmailTemplateCode)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        EmailAction = e.EmailAction,
                        EmailTemplateCode = e.EmailTemplateCode,
                        FromAddress = e.EmailCompose.FromAddress,
                        Subject = e.EmailCompose.Subject,
                        Body = e.EmailCompose.Body.Replace("<br />", System.Environment.NewLine),
                        Narration = e.EmailCompose.Narration,
                        Signature = e.EmailCompose.Signature.Replace("<br />", System.Environment.NewLine),
                        SignatureLogo = e.EmailCompose.SignatureLogo,
                        EmailAction_Id = e.EmailAction.Id == null ? 0 : e.EmailAction.Id,
                        EmailTemplateCode_Id = e.EmailTemplateCode.Id == null ? 0 : e.EmailTemplateCode.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                int EmailId = db.EmailSendData.Include(e => e.EmailCompose).Where(e => e.Id == data).SingleOrDefault().EmailCompose.Id;

                List<EmailAddress1> cat = new List<EmailAddress1>();

                //var a = db.Grade.Include(e => e.Levels).Where(e => e.Id == data).Select(e => e.Levels).ToList();

                //foreach (var ca in a)
                //{
                //    return_data.Add(
                //new Level_CD
                //{
                //    Level_Id = ca.Select(e => e.Id.ToString()).ToArray(),
                //    Level_FullDetails = ca.Select(e => e.FullDetails).ToArray()
                //});
                //}

                var add_data = db.Email
                               .Include(e => e.ToAddresses).Include(e => e.ToAddresses.Select(r => r.EmailAddressType)).Where(e => e.Id == EmailId).SingleOrDefault();

                foreach (var ca in add_data.ToAddresses)
                {
                    cat.Add(new EmailAddress1
                    {
                        Address_Id = ca.Id,
                        Address_val = ca.FullDetails,
                    });
                }




                var Corp = db.EmailSendData.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, cat, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        public ActionResult ChangeProcess(string forwardata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                List<int> ids = null; try
                {
                    if (forwardata != null && forwardata != "0" && forwardata != "false")
                    {
                        ids = Utility.StringIdsToListIds(forwardata);
                    }
                    else
                    {
                        Msg.Add("  Kindly Select Record.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }


                    List<EmailSendData> emailSendData = new List<EmailSendData>();
                    foreach (var i in ids)
                    {
                        EmailSendData EmailSendDataT = db.EmailSendData.Include(e => e.EmailCompose).Where(e => e.Id == i).SingleOrDefault();
                        if (EmailSendDataT != null)
                        {
                            emailSendData.Add(EmailSendDataT);
                        }
                       
                    }
                    LookupValue LookupValId = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "800").SingleOrDefault().LookupValues.Where(r => r.LookupVal.ToUpper() == "SEND").FirstOrDefault();
                    if (emailSendData.Count > 0)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                        {

                            emailSendData.ForEach(q => q.EmailAction = LookupValId);
                            // db.Entry;
                            //  db.Entry(salaryt).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            ts.Complete();
                            // TempData["RowVersion"] = SalT.RowVersion;
                            //  db.Entry(salaryt).State = System.Data.Entity.EntityState.Detached;
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
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                return Json(new { success = true, responseText = "Status changed for selected records." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(Email c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                    string EmailActionlist = form["EmailActionlist"] == "0" ? "" : form["EmailActionlist"];
                    string EmailTemplateCodelist = form["EmailTemplateCodelist"] == "0" ? "" : form["EmailTemplateCodelist"];
                    string EmailAddrsTypelist = form["EmailAddrsTypelist"] == "0" ? "" : form["EmailAddrsTypelist"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (EmailActionlist != null && EmailActionlist != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "800").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmailActionlist)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(EmailActionlist));
                        c.EmailAction = val;
                    }



                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {


                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                Email blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.Email.Where(e => e.Id == data).Include(e => e.ToAddresses)
                                                            .Include(e => e.EmailAction).Include(e => e.EmailTemplateCode)
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

                                //  int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);

                                if (EmailActionlist != null && EmailActionlist != "")
                                {
                                    var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "800").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmailActionlist)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(EmailActionlist));
                                    c.EmailAction = val;

                                    var type = db.Email.Include(e => e.EmailAction).Where(e => e.Id == data).SingleOrDefault();
                                    Email typedetails = null;
                                    if (type.EmailAction != null)
                                    {
                                        typedetails = db.Email.Where(x => x.EmailAction.Id == type.EmailAction.Id && x.Id == data).SingleOrDefault();
                                    }
                                    else
                                    {
                                        typedetails = db.Email.Where(x => x.Id == data).SingleOrDefault();
                                    }
                                    typedetails.EmailAction = c.EmailAction;
                                    db.Email.Attach(typedetails);
                                    db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = typedetails.RowVersion;
                                    db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;

                                }
                                else
                                {
                                    var BusiTypeDetails = db.Email.Include(e => e.EmailAction).Where(x => x.Id == data).SingleOrDefault();
                                    BusiTypeDetails.EmailAction = null;
                                    db.Email.Attach(BusiTypeDetails);
                                    db.Entry(BusiTypeDetails).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = BusiTypeDetails.RowVersion;
                                    db.Entry(BusiTypeDetails).State = System.Data.Entity.EntityState.Detached;
                                }


                                if (EmailTemplateCodelist != null && EmailTemplateCodelist != "")
                                {
                                    var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "806").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmailTemplateCodelist)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(EmailTemplateCodelist));
                                    c.EmailTemplateCode = val;

                                    var type = db.Email.Include(e => e.EmailTemplateCode).Where(e => e.Id == data).SingleOrDefault();
                                    Email typedetails = null;
                                    if (type.EmailTemplateCode != null)
                                    {
                                        typedetails = db.Email.Where(x => x.EmailTemplateCode.Id == type.EmailTemplateCode.Id && x.Id == data).SingleOrDefault();
                                    }
                                    else
                                    {
                                        typedetails = db.Email.Where(x => x.Id == data).SingleOrDefault();
                                    }
                                    typedetails.EmailTemplateCode = c.EmailTemplateCode;
                                    db.Email.Attach(typedetails);
                                    db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = typedetails.RowVersion;
                                    db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;

                                }
                                else
                                {
                                    var BusiTypeDetails = db.Email.Include(e => e.EmailTemplateCode).Where(x => x.Id == data).SingleOrDefault();
                                    BusiTypeDetails.EmailTemplateCode = null;
                                    db.Email.Attach(BusiTypeDetails);
                                    db.Entry(BusiTypeDetails).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = BusiTypeDetails.RowVersion;
                                    db.Entry(BusiTypeDetails).State = System.Data.Entity.EntityState.Detached;
                                }

                                var emailser = db.EmailServer.FirstOrDefault().Id;

                                if (emailser != null)
                                {
                                    if (emailser != 0)
                                    {
                                        var val = db.EmailServer.Find(emailser);
                                        c.EmailServer = val;

                                        var add = db.Email.Include(e => e.EmailServer).Where(e => e.Id == data).SingleOrDefault();
                                        IList<Email> contactsdetails = null;
                                        if (add.EmailServer != null)
                                        {
                                            contactsdetails = db.Email.Where(x => x.EmailServer.Id == add.EmailServer.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            contactsdetails = db.Email.Where(x => x.Id == data).ToList();
                                        }
                                        foreach (var s in contactsdetails)
                                        {
                                            s.EmailServer = c.EmailServer;
                                            db.Email.Attach(s);
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
                                    var contactsdetails = db.Email.Include(e => e.EmailServer).Where(x => x.Id == data).ToList();
                                    foreach (var s in contactsdetails)
                                    {
                                        s.EmailServer = null;
                                        db.Email.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }


                                List<EmailAddress> ObjEmailAddressList = new List<EmailAddress>();
                                Email EmailAddressdetails = null;
                                EmailAddressdetails = db.Email.Include(e => e.ToAddresses).Where(e => e.Id == data).SingleOrDefault();
                                if (Addrs != null && Addrs != "")
                                {


                                    var ids = Utility.StringIdsToListIds(Addrs);
                                    foreach (var ca in ids)
                                    {
                                        var EmailAddressvalue = db.EmailAddress.Find(ca);

                                        if (EmailAddressvalue != null && db.EmailAddress.Any(e => e.Address == EmailAddressvalue.Address))
                                        {
                                            ObjEmailAddressList.Add(EmailAddressvalue);
                                            EmailAddressdetails.ToAddresses = ObjEmailAddressList;
                                        }
                                        else
                                        {
                                            if (EmailAddrsTypelist != null && EmailAddrsTypelist != "")
                                            {
                                                var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "801").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmailAddrsTypelist)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(EmailAddrsTypelist));
                                                string addrs = db.Employee.Include(e => e.PerContact).Where(e => e.PerContact.Id == ca).SingleOrDefault().PerContact.EmailId;

                                                //c.EmailAction = val;

                                                DBTrack dbt = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                EmailAddress emailaddress = new EmailAddress()
                                                {

                                                    EmailAddressType = val,
                                                    Address = addrs,
                                                    DBTrack = dbt
                                                };
                                                db.EmailAddress.Add(emailaddress);
                                                ObjEmailAddressList.Add(emailaddress);
                                                EmailAddressdetails.ToAddresses = ObjEmailAddressList;
                                            }

                                        }

                                    }
                                }
                                else
                                {
                                    EmailAddressdetails.ToAddresses = null;
                                }




                                db.Email.Attach(EmailAddressdetails);
                                db.Entry(EmailAddressdetails).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = EmailAddressdetails.RowVersion;
                                db.Entry(EmailAddressdetails).State = System.Data.Entity.EntityState.Detached;


                                //var CurCorp = db.Email.Find(data);
                                //TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                //{

                                //model.ChallengeDescription = model.ChallengeDescription.Replace(System.Environment.NewLine, "<br />");

                                var myString = c.Body.Replace(System.Environment.NewLine, "<br />"); //add a line terminating ;

                                var myString1 = c.Signature.Replace(System.Environment.NewLine, "<br />"); //add a line terminating ;


                                Email corp = new Email()
                                {
                                    EmailAction = c.EmailAction,
                                    FromAddress = c.FromAddress,
                                    Subject = c.Subject,
                                    Body = myString,
                                    Narration = c.Narration,
                                    Signature = myString1,
                                    SignatureLogo = c.SignatureLogo,
                                    DBTrack = c.DBTrack,
                                    Id = data
                                };


                                db.Email.Attach(corp);
                                db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                //}

                                //using (var context = new DataBaseContext())
                                //{
                                //c.Id = data;

                                /// DBTrackFile.DBTrackSave("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
                                //PropertyInfo[] fi = null;
                                //Dictionary<string, object> rt = new Dictionary<string, object>();
                                //fi = c.GetType().GetProperties();
                                ////foreach (var Prop in fi)
                                ////{
                                ////    if (Prop.Name != "Id" && Prop.Name != "DBTrack" && Prop.Name != "RowVersion")
                                ////    {
                                ////        rt.Add(Prop.Name, Prop.GetValue(c));
                                ////    }
                                ////}
                                //rt = blog.DetailedCompare(c);
                                //rt.Add("Orig_Id", c.Id);
                                //rt.Add("Action", "M");
                                //rt.Add("DBTrack", c.DBTrack);
                                //rt.Add("RowVersion", c.RowVersion);
                                //var aa = DBTrackFile.GetInstance("Core/P2b.Global", "DT_" + "Corporate", rt);
                                //DT_Corporate d = (DT_Corporate)aa;
                                //db.DT_Corporate.Add(d);
                                //db.SaveChanges();

                                //To save data in history table 
                                //var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "Corporate", c.DBTrack);
                                //DT_Corporate DT_Corp = (DT_Corporate)Obj;
                                //db.DT_Corporate.Add(DT_Corp);
                                //db.SaveChanges();\


                                //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                //DT_Corporate DT_Corp = (DT_Corporate)obj;
                                //DT_Corp.Address_Id = blog.Address == null ? 0 : blog.Address.Id;
                                //DT_Corp.BusinessType_Id = blog.BusinessType == null ? 0 : blog.BusinessType.Id;
                                //DT_Corp.ContactDetails_Id = blog.ContactDetails == null ? 0 : blog.ContactDetails.Id;
                                //db.Create(DT_Corp);
                                //}
                                db.SaveChanges();
                                await db.SaveChangesAsync();
                                ts.Complete();

                                Msg.Add("Record Updated Successfully.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            List<string> MsgB = new List<string>();
                            MsgB.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            Email blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            //Email Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Email.Where(e => e.Id == data).Include(e => e.EmailAction).Include(e => e.ToAddresses).SingleOrDefault();
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

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            Email corp = new Email()
                            {
                                EmailAction = c.EmailAction,
                                FromAddress = c.FromAddress,
                                ToAddresses = c.ToAddresses,
                                Subject = c.Subject,
                                Body = c.Body,
                                Narration = c.Narration,
                                Signature = c.Signature,
                                SignatureLogo = c.SignatureLogo,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]

                            };



                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            //using (var context = new DataBaseContext())
                            //{
                            //    var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "Corporate", c.DBTrack);
                            //    // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                            //    Old_Corp = context.Corporate.Where(e => e.Id == data).Include(e => e.BusinessType)
                            //        .Include(e => e.Address).Include(e => e.ContactDetails).SingleOrDefault();
                            //    DT_Corporate DT_Corp = (DT_Corporate)obj;
                            //    DT_Corp.Address_Id = DBTrackFile.ValCompare(Old_Corp.Address, c.Address);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                            //    DT_Corp.BusinessType_Id = DBTrackFile.ValCompare(Old_Corp.BusinessType, c.BusinessType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                            //    DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                            //    db.Create(DT_Corp);
                            //    //db.SaveChanges();
                            //}
                            blog.DBTrack = c.DBTrack;
                            db.Email.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Record Updated Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (Corporate)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (Corporate)databaseEntry.ToObject();
                        c.RowVersion = databaseValues.RowVersion;

                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return View();
            }

        }
	}
}