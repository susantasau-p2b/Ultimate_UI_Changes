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
using System.IO;
using System.Configuration;
namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class EmailController : Controller
    {

        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Email/Index.cshtml");
        }

        public ActionResult UploadLogo()
        {
            return View("~/Views/Shared/Core/_SignatureLogo.cshtml");
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(Email c, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string EmailActionlist = form["EmailActionlist"] == "0" ? "" : form["EmailActionlist"];
                string EmailTemplateCodelist = form["EmailTemplateCodelist"] == "0" ? "" : form["EmailTemplateCodelist"];
                string EmailAddrsTypelist = form["EmailAddrsTypelist"] == "0" ? "" : form["EmailAddrsTypelist"];
                string Addrs = form["AddressList"] == "0" ? "" : form["AddressList"];
                string geo_id = form["geo_id"] == "0" ? "" : form["geo_id"];
                string fun_id = form["fun_id"] == "0" ? "" : form["fun_id"];
                string pay_id = form["pay_id"] == "0" ? "" : form["pay_id"];
                List<String> Msg = new List<String>();

                if (geo_id == "" && fun_id == "" && pay_id == "")
                {
                    Msg.Add("  Kindly Apply Advance Filter..!!! ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                
                
                try
                {
                    if (EmailActionlist != null && EmailActionlist != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "800").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmailActionlist)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailActionlist));
                        c.EmailAction = val;
                    }
                    if (EmailTemplateCodelist != null && EmailTemplateCodelist != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "806").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmailTemplateCodelist)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailTemplateCodelist));
                        c.EmailTemplateCode = val;
                    }


                    c.ToAddresses = null;
                    List<EmailAddress> OBJ = new List<EmailAddress>();

                    if (Addrs != null && Addrs != "")
                    {
                        var ids = Utility.StringIdsToListIds(Addrs);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.EmailAddress.Find(ca);
                            OBJ.Add(OBJ_val);
                            c.ToAddresses = OBJ;
                        }
                    }
                    EmailTemplateAssign EmailTemp = new EmailTemplateAssign() { };
                    
                    List<EmailTemplateAssign> OFAT = new List<EmailTemplateAssign>();
                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }
                    var myString = c.Body.Replace(System.Environment.NewLine, "<br />"); //add a line terminating ;

                    var myString1 = c.Signature.Replace(System.Environment.NewLine, "<br />"); //add a line terminating ;
                    var emailser = db.EmailServer.FirstOrDefault();
                    if (ModelState.IsValid)
                    {
                        //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                        //                 new System.TimeSpan(0, 30, 0)))
                        //{
                            

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                         

                            Email corporate = new Email()
                            {
                                EmailAction = c.EmailAction,
                                EmailTemplateCode = c.EmailTemplateCode,
                                FromAddress = c.FromAddress,
                                ToAddresses = c.ToAddresses,
                                Subject = c.Subject,
                                Body = myString,
                                Narration = c.Narration,
                                Signature = myString1,
                                SignatureLogo = c.SignatureLogo,
                                EmailServer = emailser,
                                DBTrack = c.DBTrack
                            };

                            db.Email.Add(corporate);

                            

                            if ((geo_id != "") && (pay_id != "") && (fun_id != ""))
                            {
                                var ids = Utility.StringIdsToListIds(geo_id);
                                var ids1 = Utility.StringIdsToListIds(pay_id);
                                var ids2 = Utility.StringIdsToListIds(fun_id);
                                foreach (var G in ids)
                                {
                                    foreach (var P in ids1)
                                    {
                                        foreach (var F in ids2)
                                        {
                                            GeoStruct geo = db.GeoStruct.Find(G);
                                            EmailTemp.GeoStruct = geo;

                                            PayStruct pay = db.PayStruct.Find(P);
                                            EmailTemp.PayStruct = pay;

                                            FuncStruct fun = db.FuncStruct.Find(F);
                                            EmailTemp.FuncStruct = fun;

                                            EmailTemplateAssign EmailTempAss = new EmailTemplateAssign() 
                                            {
                                                Email = corporate,
                                                FuncStruct = EmailTemp.FuncStruct,
                                                GeoStruct = EmailTemp.GeoStruct,
                                                PayStruct = EmailTemp.PayStruct,
                                                DBTrack = c.DBTrack
                                            };
                                            db.EmailTemplateAssign.Add(EmailTempAss);
                                            db.SaveChanges();

                                            //OFAT.Add(db.EmailTemplateAssign.Find(EmailTempAss.Id));
                                        }

                                    }

                                }

                            }

                            if ((geo_id != "") && (pay_id != "") && (fun_id == ""))
                            {
                                var ids = Utility.StringIdsToListIds(geo_id);
                                var ids1 = Utility.StringIdsToListIds(pay_id);
                                foreach (var G in ids)
                                {
                                    foreach (var P in ids1)
                                    {
                                        GeoStruct geo = db.GeoStruct.Find(G);
                                        EmailTemp.GeoStruct = geo;

                                        PayStruct pay = db.PayStruct.Find(P);
                                        EmailTemp.PayStruct = pay;

                                        EmailTemp.FuncStruct = null;

                                        EmailTemplateAssign EmailTempAss = new EmailTemplateAssign()
                                        {
                                            Email = corporate,
                                            FuncStruct = EmailTemp.FuncStruct,
                                            GeoStruct = EmailTemp.GeoStruct,
                                            PayStruct = EmailTemp.PayStruct,
                                            DBTrack = c.DBTrack
                                        };
                                        db.EmailTemplateAssign.Add(EmailTempAss);
                                        db.SaveChanges();

                                        //OFAT.Add(db.EmailTemplateAssign.Find(EmailTempAss.Id));
                                    }

                                }

                            }

                            if ((geo_id != "") && (fun_id != "") && (pay_id == ""))
                            {
                                var ids = Utility.StringIdsToListIds(geo_id);
                                var ids1 = Utility.StringIdsToListIds(fun_id);
                                foreach (var G in ids)
                                {
                                    foreach (var F in ids1)
                                    {
                                        GeoStruct geo = db.GeoStruct.Find(G);
                                        EmailTemp.GeoStruct = geo;

                                        EmailTemp.PayStruct = null;

                                        FuncStruct fun = db.FuncStruct.Find(F);
                                        EmailTemp.FuncStruct = fun;

                                        EmailTemplateAssign EmailTempAss = new EmailTemplateAssign()
                                        {
                                            Email = corporate,
                                            FuncStruct = EmailTemp.FuncStruct,
                                            GeoStruct = EmailTemp.GeoStruct,
                                            PayStruct = EmailTemp.PayStruct,
                                            DBTrack = c.DBTrack
                                        };
                                        db.EmailTemplateAssign.Add(EmailTempAss);
                                        db.SaveChanges();

                                      //  OFAT.Add(db.EmailTemplateAssign.Find(EmailTempAss.Id));
                                    }

                                }

                            }

                            if ((geo_id != "") && (fun_id == "") && (pay_id == ""))
                            {
                                var ids = Utility.StringIdsToListIds(geo_id);

                                foreach (var G in ids)
                                {
                                    GeoStruct geo = db.GeoStruct.Find(G);
                                    EmailTemp.GeoStruct = geo;

                                    EmailTemp.PayStruct = null;
                                    EmailTemp.FuncStruct = null;

                                    EmailTemplateAssign EmailTempAss = new EmailTemplateAssign()
                                    {
                                        Email = corporate,
                                        FuncStruct = EmailTemp.FuncStruct,
                                        GeoStruct = EmailTemp.GeoStruct,
                                        PayStruct = EmailTemp.PayStruct,
                                        DBTrack = c.DBTrack
                                    };
                                    db.EmailTemplateAssign.Add(EmailTempAss);
                                    db.SaveChanges();

                                   // OFAT.Add(db.EmailTemplateAssign.Find(EmailTempAss.Id));
                                }

                            }

                            if ((fun_id != "") && (pay_id == "") && (geo_id == ""))
                            {
                                

                                var ids = Utility.StringIdsToListIds(fun_id);
                                foreach (var F in ids)
                                {
                                    EmailTemp.GeoStruct = null;

                                    EmailTemp.PayStruct = null;

                                    FuncStruct fun = db.FuncStruct.Find(F);
                                    EmailTemp.FuncStruct = fun;

                                    EmailTemplateAssign EmailTempAss = new EmailTemplateAssign()
                                    {
                                        Email = corporate,
                                        FuncStruct = EmailTemp.FuncStruct,
                                        GeoStruct = EmailTemp.GeoStruct,
                                        PayStruct = EmailTemp.PayStruct,
                                        DBTrack = c.DBTrack
                                    };
                                    db.EmailTemplateAssign.Add(EmailTempAss);
                                    db.SaveChanges();

                                    //OFAT.Add(db.EmailTemplateAssign.Find(EmailTempAss.Id));
                                }
                            }

                            if ((pay_id != "") && (geo_id == "") && (fun_id == ""))
                            {
                                var ids = Utility.StringIdsToListIds(pay_id);


                                foreach (var P in ids)
                                {
                                    
                                    EmailTemp.GeoStruct = null;
                                    EmailTemp.FuncStruct = null;

                                    PayStruct pay = db.PayStruct.Find(P);
                                    EmailTemp.PayStruct = pay;
                                    

                                    EmailTemplateAssign EmailTempAss = new EmailTemplateAssign()
                                    {
                                        Email = corporate,
                                        FuncStruct = EmailTemp.FuncStruct,
                                        GeoStruct = EmailTemp.GeoStruct,
                                        PayStruct = EmailTemp.PayStruct,
                                        DBTrack = c.DBTrack
                                    };
                                    db.EmailTemplateAssign.Add(EmailTempAss);
                                    db.SaveChanges();

                                    //OFAT.Add(db.EmailTemplateAssign.Find(EmailTempAss.Id));
                                }
                            }

                            if (geo_id == "" && pay_id == "" && fun_id == "")
                            {
                                var geoList = db.GeoStruct.Where(e => e.Company != null && e.Company.Id == CompId).ToList();

                                foreach (var geo in geoList)
                                {
                                    EmailTemp.GeoStruct = geo;

                                    EmailTemp.PayStruct = null;
                                    EmailTemp.FuncStruct = null;

                                    EmailTemplateAssign EmailTempAss = new EmailTemplateAssign()
                                    {
                                        Email = corporate,
                                        FuncStruct = EmailTemp.FuncStruct,
                                        GeoStruct = EmailTemp.GeoStruct,
                                        PayStruct = EmailTemp.PayStruct,
                                        DBTrack = c.DBTrack
                                    };
                                    db.EmailTemplateAssign.Add(EmailTempAss);
                                    db.SaveChanges();

                                    //OFAT.Add(db.EmailTemplateAssign.Find(EmailTempAss.Id));
                                }
                            }

                            //var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, c.DBTrack);
                            //DT_Email email = (DT_Email)rtn_Obj;
                            //email.EmailAction_Id = c.EmailAction == null ? 0 : c.EmailAction.Id;
                            //email.EmailServer_Id = c.EmailServer == null ? 0 : c.EmailServer.Id;
                            //email.EmailTemplateCode_Id = c.EmailTemplateCode == null ? 0 : c.EmailTemplateCode.Id;
                            //db.Create(email);
                            //db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);

                            //ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //}
                        //catch (DbUpdateConcurrencyException)
                        //{
                        //    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        //}


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
                        Msg.Add("Code Already Exists.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }

        }

       

        public class EmailAddress1
        {
            public int Address_Id { get; set; }
            public string Address_val { get; set; }

        }

        [HttpPost]
        public ActionResult Edit(int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Email
                    .Include(e => e.ToAddresses)
                    .Include(e => e.EmailAction)
                    .Include(e => e.EmailTemplateCode)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        EmailAction = e.EmailAction,
                        EmailTemplateCode = e.EmailTemplateCode,
                        FromAddress = e.FromAddress,
                        Subject = e.Subject,
                        Body = e.Body.Replace("<br />", System.Environment.NewLine),
                        Narration = e.Narration,
                        Signature = e.Signature.Replace("<br />", System.Environment.NewLine),
                        SignatureLogo = e.SignatureLogo,
                        EmailAction_Id = e.EmailAction.Id == null ? 0 : e.EmailAction.Id,
                        EmailTemplateCode_Id = e.EmailTemplateCode.Id == null ? 0 : e.EmailTemplateCode.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

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
                               .Include(e => e.ToAddresses).Include(e => e.ToAddresses.Select(r => r.EmailAddressType)).Where(e => e.Id == data).SingleOrDefault();

                foreach (var ca in add_data.ToAddresses)
                {
                    cat.Add(new EmailAddress1
                    {
                        Address_Id = ca.Id,
                        Address_val = ca.FullDetails,
                    });
                }




                var Corp = db.Email.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, cat, "", Auth, JsonRequestBehavior.AllowGet });
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
                                    var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "800").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmailActionlist)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(EmailActionlist));
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
                                    var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "806").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(EmailActionlist)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(EmailTemplateCodelist));
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
                                                var val = db.LookupValue.Find(int.Parse(EmailAddrsTypelist));
                                                string addrs = db.Employee.Include(e => e.PerContact).Where(e => e.PerContact.Id  == ca).SingleOrDefault().PerContact.EmailId;

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

                                var myString1 = c.Signature == null ? null : c.Signature.Replace(System.Environment.NewLine, "<br />"); //add a line terminating ;


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

                            Corporate corp = db.Corporate.Include(e => e.Address)
                                .Include(e => e.ContactDetails)
                                .Include(e => e.BusinessType).FirstOrDefault(e => e.Id == auth_id);

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

                            db.Corporate.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        Corporate Old_Corp = db.Corporate.Include(e => e.BusinessType)
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

                        DT_Corporate Curr_Corp = db.DT_Corporate
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            Corporate corp = new Corporate();

                            string Corp = Curr_Corp.BusinessType_Id == null ? null : Curr_Corp.BusinessType_Id.ToString();
                            string Addrs = Curr_Corp.Address_Id == null ? null : Curr_Corp.Address_Id.ToString();
                            string ContactDetails = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();
                            corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                            corp.Code = Curr_Corp.Code == null ? Old_Corp.Code : Curr_Corp.Code;
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

                                        int a = EditS(Corp, Addrs, ContactDetails, auth_id, corp, corp.DBTrack);
                                        //var CurCorp = db.Corporate.Find(auth_id);
                                        //TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                        //db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                        //if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                        //{
                                        //    c.DBTrack = new DBTrack
                                        //    {
                                        //        CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                        //        CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                        //        Action = "M",
                                        //        ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                        //        ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                        //        AuthorizedBy = SessionManager.UserName,
                                        //        AuthorizedOn = DateTime.Now,
                                        //        IsModified = false
                                        //    };
                                        //    Corporate corp = new Corporate()
                                        //    {
                                        //        Code = c.Code,
                                        //        Name = c.Name,
                                        //        Id = Convert.ToInt32(auth_id),
                                        //        DBTrack = c.DBTrack
                                        //    };


                                        //    db.Corporate.Attach(corp);
                                        //    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;

                                        //    // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                        //    //db.SaveChanges();
                                        //    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        //    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //    await db.SaveChangesAsync();
                                        //    //DisplayTrackedEntities(db.ChangeTracker);
                                        //    db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                                        //    ts.Complete();
                                        //    return Json(new Object[] { corp.Id, corp.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                                        //}

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add(" Record Authorised ");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
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
                                        corp.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                        {
                            List<string> Msgr = new List<string>();
                            Msgr.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);

                        }
                        // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            Corporate corp = db.Corporate.AsNoTracking().Include(e => e.Address)
                                                                        .Include(e => e.BusinessType)
                                                                        .Include(e => e.ContactDetails).FirstOrDefault(e => e.Id == auth_id);

                            Address add = corp.Address;
                            ContactDetails conDet = corp.ContactDetails;
                            LookupValue val = corp.BusinessType;

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

                            db.Corporate.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                            Msg.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        public class P2BGridData
        {
            public int Id { get; set; }
            public string EmailAction { get; set; }
            public string Subject { get; set; }
            public string EmailTemplateCode { get; set; }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> SalaryList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    
                    var BindEmpList = db.EmailTemplateAssign.Include(e => e.Email).GroupBy(e => e.Email).AsParallel().ToList();

                    //var fall = db.SalHeadFormula.GroupBy(x => x.Name).Select(y => y.FirstOrDefault()).ToList();

                    foreach (var z in BindEmpList)
                    {
                        int id =  z.FirstOrDefault().Email.Id;
                        Email email = db.Email.Include(e => e.EmailAction).Include(e => e.EmailTemplateCode).Where(e => e.Id == id).AsNoTracking().SingleOrDefault();
                        if (email != null)
                        {
                            view = new P2BGridData()
                            {
                                Id = email.Id,
                                EmailTemplateCode = email.EmailTemplateCode != null ? email.EmailTemplateCode.LookupVal.ToString() : "",
                                EmailAction = email.EmailAction != null ? email.EmailAction.LookupVal.ToString() : "",
                                Subject = email.Subject
                            };
                            model.Add(view);
                            //break; 
                        }

                    }

                    SalaryList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = SalaryList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.EmailAction.ToUpper().ToString().Contains(gp.searchString))
                                  || (e.EmailTemplateCode.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                  || (e.Subject.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                  || (e.Id.ToString().Contains(gp.searchString))
                                  )
                              .Select(a => new Object[] { a.EmailAction, a.EmailTemplateCode, a.Subject, a.Id }).ToList();



                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmailAction, a.EmailTemplateCode, a.Subject, a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = SalaryList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "EmailAction" ? c.EmailAction.ToString() :
                                             gp.sidx == "EmailTemplateCode" ? c.EmailTemplateCode.ToString() :
                                             gp.sidx == "Subject" ? c.Subject.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.EmailAction, a.EmailTemplateCode, a.Subject, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.EmailAction, a.EmailTemplateCode, a.Subject, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmailAction, a.EmailTemplateCode, a.Subject, a.Id }).ToList();
                        }
                        totalRecords = SalaryList.Count();
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
        }

        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    List<string> Msg = new List<string>();

        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<Email> Corporate = null;
        //        if (gp.IsAutho == true)
        //        {
        //              var emailTempAssign = db.EmailTemplateAssign.Include(e => e.Email).Include(e => e.Email.EmailAction)
        //                  .Include(e => e.Email.ToAddresses).ToList().Distinct();
        //            Corporate = db.Email.Include(e => e.EmailAction).Include(e => e.ToAddresses).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            Corporate = db.Email.Include(e => e.EmailAction).Include(e => e.ToAddresses).AsNoTracking().ToList();
        //        }

        //        IEnumerable<Email> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = Corporate;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Where(e => (e.EmailAction != null ? e.EmailAction.LookupVal.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
        //                       || (e.Subject.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
        //                       || (e.Id.ToString().Contains(gp.searchString))
        //                       ).Select(a => new Object[] { a.EmailAction != null ? a.EmailAction.LookupVal : "", a.Subject, a.Id }).ToList();

        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmailAction != null ? Convert.ToString(a.EmailAction.LookupVal) : "", a.Subject, a.Id }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = Corporate;
        //            Func<Email, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => 
        //                                 gp.sidx == "EmailAction" ? c.EmailAction.LookupVal :
        //                                 gp.sidx == "Subject" ? c.Subject : "");

        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.EmailAction != null ? Convert.ToString(a.EmailAction.LookupVal) : "", Convert.ToString(a.Subject), a.Id }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.EmailAction != null ? Convert.ToString(a.EmailAction.LookupVal) : "", Convert.ToString(a.Subject), a.Id }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmailAction != null ? Convert.ToString(a.EmailAction.LookupVal) : "", Convert.ToString(a.Subject), a.Id }).ToList();
        //            }
        //            totalRecords = Corporate.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                  
                    Email email = db.Email.Include(e => e.EmailAction)
                                                       .Include(e => e.EmailServer)
                                                       .Include(e => e.EmailTemplateCode).Where(e => e.Id == data).SingleOrDefault();

                    LookupValue EmailAction = email.EmailAction;
                    LookupValue EmailTemCode = email.EmailTemplateCode;
                    EmailServer EmailServ = email.EmailServer;
                    var selectedRegions = email.ToAddresses;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (selectedRegions != null)
                            {
                                var corpRegion = new HashSet<int>(email.ToAddresses.Select(e => e.Id));
                                if (corpRegion.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                }
                            }


                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = email.DBTrack.CreatedBy != null ? email.DBTrack.CreatedBy : null,
                                CreatedOn = email.DBTrack.CreatedOn != null ? email.DBTrack.CreatedOn : null,
                                IsModified = email.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(email).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            DT_Email DT_Corp = (DT_Email)rtn_Obj;
                            DT_Corp.EmailAction_Id = EmailAction == null ? 0 : EmailAction.Id;
                            DT_Corp.EmailServer_Id = EmailServ == null ? 0 : EmailServ.Id;
                            DT_Corp.EmailTemplateCode_Id = EmailTemCode == null ? 0 : EmailTemCode.Id;
                             
                            db.Create(DT_Corp);

                            await db.SaveChangesAsync();


                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    
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



        public class MergeAddrs
        {
            public int Id { get; set; }
            public string Type { get; set; } // or enum?
            public string Address { get; set; }
        }


        [HttpPost]
        public ActionResult GetAddressLKDetails(List<int> SkipIds, int AddrsType)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (AddrsType != 0)
                {
                    LookupValue lkValType = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "801").FirstOrDefault().LookupValues.Where(e => e.Id == AddrsType).FirstOrDefault(); //db.LookupValue.Find(AddrsType);
                    List<EmailAddress> Addrs1 = db.EmailAddress.Include(e => e.EmailAddressType).ToList();
                    List<Employee> Addrs2 = db.Employee.Include(e => e.PerContact).Where(e => e.PerContact != null && e.PerContact.EmailId != null).ToList();

                    int Count = 0;
                    
                    foreach (var A1 in Addrs2)
                    {
                        foreach (var A2 in Addrs1)
                        {
                            if (A1.PerContact.EmailId == A2.Address)
                            {
                                Count = 0;
                                break; 
                            }
                            else
                                Count = 1;
                        }
                        if (Count == 1)
                        {
                            DBTrack dbt = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            EmailAddress emailaddress = new EmailAddress()
                            {
                                EmailAddressType = lkValType,
                                Address = A1.PerContact.EmailId,
                                DBTrack = dbt
                            };
                            db.EmailAddress.Add(emailaddress);
                            db.SaveChanges();
                        }
                    }

                    var fall = db.EmailAddress.Include(e => e.EmailAddressType).ToList();

                    if (SkipIds != null)
                    {
                        foreach (var a in SkipIds)
                        {
                            if (fall == null)
                                fall = db.Email.Include(e => e.ToAddresses).Where(e => !e.Id.ToString().Contains(a.ToString())).SelectMany(h => h.ToAddresses).ToList();
                            else
                                fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                        }
                    }



                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var fall = db.EmailAddress.Include(e => e.EmailAddressType).ToList();
                    if (SkipIds != null)
                    {
                        foreach (var a in SkipIds)
                        {
                            if (fall == null)
                                fall = db.Email.Include(e => e.ToAddresses).Where(e => !e.Id.ToString().Contains(a.ToString())).SelectMany(h => h.ToAddresses).ToList();
                            else
                            {
                                fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                            }

                        }
                    }

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.EmailAddressType.LookupVal + " - " + ca.Address }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                
            }
        }





        public int EditS(string Corp, string Addrs, string ContactDetails, int data, Corporate c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        c.BusinessType = val;

                        var type = db.Corporate.Include(e => e.BusinessType).Where(e => e.Id == data).SingleOrDefault();
                        IList<Corporate> typedetails = null;
                        if (type.BusinessType != null)
                        {
                            typedetails = db.Corporate.Where(x => x.BusinessType.Id == type.BusinessType.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Corporate.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.BusinessType = c.BusinessType;
                            db.Corporate.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.Corporate.Include(e => e.BusinessType).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.BusinessType = null;
                            db.Corporate.Attach(s);
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
                    var BusiTypeDetails = db.Corporate.Include(e => e.BusinessType).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.BusinessType = null;
                        db.Corporate.Attach(s);
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
                        c.Address = val;

                        var add = db.Corporate.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                        IList<Corporate> addressdetails = null;
                        if (add.Address != null)
                        {
                            addressdetails = db.Corporate.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.Corporate.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Address = c.Address;
                                db.Corporate.Attach(s);
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
                    var addressdetails = db.Corporate.Include(e => e.Address).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Address = null;
                        db.Corporate.Attach(s);
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
                        c.ContactDetails = val;

                        var add = db.Corporate.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<Corporate> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.Corporate.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.Corporate.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = c.ContactDetails;
                            db.Corporate.Attach(s);
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
                    var contactsdetails = db.Corporate.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.Corporate.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.Corporate.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Corporate corp = new Corporate()
                    {
                        Code = c.Code,
                        Name = c.Name,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.Corporate.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public ActionResult DeleteContactDetails(int? data, int? forwarddata)
        //{
        //    ContactDetails contDet = db.ContactDetails.Find(data);
        //    Corporate corp = db.Corporate.Find(forwarddata);
        //    try
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            corp.ContactDetails = null;
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            ts.Complete();
        //        }
        //        //return Json(new Object[] { "", "", "Data removed.", JsonRequestBehavior.AllowGet });
        //        return Json(new { msg = "Data removed.", JsonRequestBehavior.AllowGet });
        //    }

        //    catch (DataException /* dex */)
        //    {
        //        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
        //        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
        //        //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
        //        //return RedirectToAction("Index");
        //    }
        //}


        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public ActionResult DeleteAddress(int? data, int? forwarddata)
        //{
        //    Address addrs = db.Address.Find(data);
        //    Corporate corp = db.Corporate.Find(forwarddata);
        //    try
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            corp.Address = null;
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            ts.Complete();
        //        }
        //        return Json(new Object[] { "", "", "Data removed.", JsonRequestBehavior.AllowGet });
        //       // return this.Json(new { msg = "Data deleted." });
        //    }

        //    catch (DataException /* dex */)
        //    {
        //        //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
        //        return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists contact your system administrator." });
        //        //ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
        //        //return RedirectToAction("Index");
        //    }
        //}

        //public 

        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //  var context = DataContextFactory.GetDataContext();
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

        public ActionResult GetSubject(List<int> SkipIds)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = new List<LookupValue>();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Lookup.Include(e => e.LookupValues).Where(e => !e.Id.ToString().Contains(a.ToString())).SelectMany(h => h.LookupValues).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list11 = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "5007").FirstOrDefault();
                var list1 = list11.LookupValues.ToList();
                var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetBody(List<int> SkipIds)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = new List<LookupValue>();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Lookup.Include(e => e.LookupValues).Where(e => !e.Id.ToString().Contains(a.ToString())).SelectMany(h => h.LookupValues).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list11 = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "808").FirstOrDefault();
                var list1 = list11.LookupValues.ToList();
                var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);

            }
        }


        public string InvestmentUploadFile(string FolderName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\EmailDocuments\\" + FolderName + "\\";
            String localPath = "";
            bool exists = System.IO.Directory.Exists(requiredPath);
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            return localPath;
        }

        [HttpPost]
        public ActionResult CreateSignatureLogo(HttpPostedFileBase[] files, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    string extension;
                    Int32 Count = 0;
                    //string ServerSavePath = "";
                    string NewPath = "";
                    string deletefilepath = "";


                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", ".jpeg", ".pdf" };

                            foreach (HttpPostedFileBase file in files)
                            {
                                if (file == null)
                                {
                                    return Json(new { success = false, responseText = "Please Select The File..!" }, JsonRequestBehavior.AllowGet);
                                }
                                extension = Path.GetExtension(file.FileName);
                                if (!allowedExtensions.Contains(extension))
                                {
                                    return Json(new { success = false, responseText = "File Type Is Not Supported..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }


                            foreach (HttpPostedFileBase file in files)
                            {

                                if (file != null)
                                {
                                    extension = Path.GetExtension(file.FileName);
                                    String FolderName = "SignatureLogo"+"\\";
                                    var newfilename = extension;
                                    //var InputFileName = Path.GetFileName(file.FileName);
                                    //ServerSavePath = InvestmentUploadFile(FolderName);
                                    //string ServerMappath = ServerSavePath + InputFileName;
                                    var ServerSavePath = ConfigurationManager.AppSettings["EmployeeDocuments"];
                                    if (ServerSavePath == null)
                                    {
                                        return Json(new { success = false, responseText = "Please contact the admin to define the document path." }, JsonRequestBehavior.AllowGet);
                                    }
                                    string ServerMappath = ServerSavePath + FolderName + newfilename;

                                    deletefilepath = ServerMappath;
                                    if (deletefilepath != null)
                                    {
                                        FileInfo File = new FileInfo(deletefilepath);
                                        bool exists = File.Exists;
                                        if (exists)
                                        {
                                            System.IO.File.Delete(deletefilepath);
                                        }
                                    }
                                    if (!Directory.Exists(ServerSavePath + FolderName))
                                    {
                                        Directory.CreateDirectory(ServerSavePath + FolderName);
                                    }
                                    file.SaveAs(Path.Combine(ServerMappath));
                                    NewPath = ServerMappath;
                                    //NewPath = Path.Combine(ServerSavePath, InputFileName);
                                    //file.SaveAs(Path.Combine(ServerSavePath, InputFileName));

                                    Count++;
                                    int idpass = 0;
                                    var returndata = NewPath;
                                    //return Json(returndata, JsonRequestBehavior.AllowGet);
                                    Msg.Add("Data Saved successfully");
                                    return Json(new Utility.JsonReturnClass { Id = idpass, Val = returndata, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                            }

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
