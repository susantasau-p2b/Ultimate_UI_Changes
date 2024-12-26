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
using Payroll;
using P2BUltimate.Process;
using System.Diagnostics;
using P2BUltimate.Security;
using System.Globalization;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class ITChallanController : Controller
    {
        //
        // GET: /ItChalan/
        // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/Itchallan/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Payroll/_Paymonth.cshtml");
        }
        public ActionResult partial1()
        {
            return View("~/Views/Shared/Payroll/_PayCal.cshtml");
        }
        static double sum;
        static double sumYr;
        static double sum1;

        public class empdetails
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public double Amount { get; set; }
            public double Gross { get; set; }

        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult CreateAmt(FormCollection form, string forwarddata) //Create submit
        {
            List<String> Msg = new List<String>();
            try
            {
                var p = sum;
                return Json(p, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Msg.Add(e.Message);
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult CreateYearlyAmt(FormCollection form, string forwarddata) //Create submit
        {
            List<String> Msg = new List<String>();
            try
            {
                var p = sumYr;
                return Json(p, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Msg.Add(e.Message);
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult CreateAmt1(FormCollection form) //Create submit
        {
            List<String> Msg = new List<String>();
            try
            {
                var p = tot;
                return Json(p, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Msg.Add(e.Message);
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetYearlyPay(string param)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();

                // List<SalaryHead> testre = db.YearlyPaymentT
                //                          .Include(e => e.ITChallan)
                //                          .Include(e => e.SalaryHead)
                //                          .Where(e => e.ITChallan == null && e.PayMonth == param && e.TDSAmount >= 0)
                //                          .Select(r => r.SalaryHead).Distinct()//&& q.TDSAmount >= 0 && q.PayMonth == param)
                //                          .ToList();
                //if (testre.Count() > 0)
                //{ }

                List<SalaryHead> data = db.YearlyPaymentT
                                            .Include(q => q.ITChallan)
                                            .Include(q => q.SalaryHead)
                                            .Where(e => e.ITChallan == null && e.PayMonth == param && e.TDSAmount >= 0)//&& q.TDSAmount >= 0 && q.PayMonth == param)
                                            .Select(x => x.SalaryHead).Distinct().ToList();

                List<SalaryHead> data1 = db.OthEarningDeductionT
                                                    .Include(q => q.SalaryHead)
                                                    .Include(q => q.SalaryHead.Type)
                                                    .Where(q => q.TDSAmount != 0 && q.PayMonth == param)
                                                    .Select(q => q.SalaryHead).Distinct().ToList();

                List<SalaryHead> data2 = db.PerkTransT
                                                   .Include(q => q.SalaryHead)
                                                   .Include(q => q.SalaryHead.Type)
                                                   .Where(q => q.ActualAmount != 0 && q.PayMonth == param)
                                                   .Select(q => q.SalaryHead).Distinct().ToList();

                var test = data.Union(data1).Union(data2).Distinct().ToList();
                foreach (var s in test)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.FullDetails
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult edit1(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var v = form["SalaryMonth"] == null ? "" : form["SalaryMonth"];

                var v1 = db.EmployeePayroll.Include(a => a.Employee).Include(a => a.Employee.EmpName).Include(a => a.ITaxTransT).Include(a => a.SalaryT.Select(b => b.SalEarnDedT)).ToList();
                List<empdetails> p = new List<empdetails>();
                double sum = 0;
                foreach (var emp in v1)
                {
                    var vv = emp.ITaxTransT.Where(a => a.PayMonth == v).FirstOrDefault();
                    var v2 = emp.SalaryT.Where(a => a.PayMonth == v).Select(r => r.SalEarnDedT).FirstOrDefault();
                    var aa = db.SalEarnDedT.Include(b => b.SalaryHead).Include(b => b.SalaryHead.SalHeadOperationType).Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ITAX").FirstOrDefault();
                    if (vv != null)
                    {

                        if (aa != null)
                        {

                            sum = sum + vv.TaxPaid;
                            p.Add(new empdetails
                            {
                                EmpCode = emp.Employee.EmpCode,
                                EmpName = emp.Employee.EmpName.FullNameFML,
                                Amount = vv.TaxPaid == null ? 0 : vv.TaxPaid,
                                Gross = aa.Amount

                            });

                        }
                    }

                }
                var result = new { p, sum };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        //[HttpPost]
        //public ActionResult edit2(FormCollection form)
        //{
        //    var v = form["SalaryMonth"] == null ? "" : form["SalaryMonth"];

        //    var v1 = db.EmployeePayroll.Include(a => a.Employee).Include(a => a.Employee.EmpName).Include(a => a.YearlyPaymentT).Include(a => a.SalaryT.Select(b => b.SalEarnDedT)).ToList();

        //    List<empdetails> p = new List<empdetails>();
        //    double sum = 0;
        //    foreach (var emp in v1)
        //    {
        //        var vv = emp.YearlyPaymentT.Where(a => a.PayMonth == v).FirstOrDefault();
        //        var v2 = emp.SalaryT.Where(a => a.PayMonth == v).Select(r => r.SalEarnDedT).FirstOrDefault();

        //        //foreach (var item1 in v2)
        //        //{
        //        var aa = db.SalEarnDedT.Include(b => b.SalaryHead).Include(b => b.SalaryHead.SalHeadOperationType).Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ITAX").FirstOrDefault();
        //        if (vv != null)
        //        {

        //            if (aa != null)
        //            {

        //                sum = sum + vv.TaxPaid;
        //                p.Add(new empdetails
        //                {
        //                    EmpCode = emp.Employee.EmpCode,
        //                    EmpName = emp.Employee.EmpName.FullNameFML,
        //                    Amount = vv.TaxPaid == null ? 0 : vv.TaxPaid,
        //                    Gross = aa.Amount

        //                });

        //            }
        //        }

        //    }
        //    var result = new { p, sum };
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}
        ////foreach (var item in v1)
        //{
        //    var v2 = item.ITaxTransT.Where(b => b.PayMonth == v).FirstOrDefault();

        //    var v3 = db.SalaryT.Where(a => a.PayMonth == v).FirstOrDefault();
        //    var v4 = v3.TotalNet;
        //    var v5 = item.Employee.EmpCode;
        //    var v6 = item.Employee.EmpName;
        //    p.Add(new empdetails
        //    {
        //      EmpCode=v5,
        //    EmpName=v6,
        //    Amount=v2.TaxPaid,
        //    Gross=v4


        //    });

        // }
        // return Json(12);


        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(ITChallan c, FormCollection form) //Create submit
        {
            string Category = form["txtPayMonth1"] == "0" ? "" : form["txtPayMonth1"];
            //string taxamtextrachallanfalse = form["taxamt"] == "0" ? "" : form["taxamt"];
            string taxamtextrachallanfalse = form["TaxAmount"] == "" ? "0" : form["TaxAmount"];
            string taxamtextrachallanTrue = form["taxamt1"] == "" ? "0" : form["taxamt1"];
            string TaxYrAmount = form["TaxYrAmount"] == "" ? "0" : form["TaxYrAmount"];
            string TaxYrAmount1 = form["TaxYrAmount1"] == "" ? "0" : form["TaxYrAmount1"]; ///for extra challan
            string TaxAmount2 = form["TaxAmount"] == "" ? "0" : form["TaxAmount"]; ///for extra challan false                                                                            
            string HeadLoad_table = form["HeadLoad-table"] == "0" ? "" : form["HeadLoad-table"];
            string HeadLoad1_table = form["HeadLoad1-table"] == "0" ? "" : form["HeadLoad1-table"];
            string ContactDetails = form["ContactDetailsList"] == "0" ? "" : form["ContactDetailsList"];
            string OtherTax = form["Othertax"] == "0" ? "" : form["Othertax"]; /// other tax
            string ExtraChallan = form["ExtraChallan"] == "0" ? "" : form["ExtraChallan"];
            string finance = form["FinancialYearList"] == "0" ? "" : form["FinancialYearList"];

            int Fiyr = int.Parse(finance);
            List<String> Msg = new List<String>();
            double TotalTaxAmount = 0;
            DateTime Paymonth = new DateTime();
            if (Category != "")
            {
                Paymonth = Convert.ToDateTime(Category);
            }
            using (DataBaseContext db = new DataBaseContext())
            {

                var OFinancialYear = db.Calendar.Where(e => e.Id == Fiyr).SingleOrDefault();
                if (c.ExtraChallan == false)
                {
                    DateTime salarymonth = Convert.ToDateTime("01/" + c.SalaryMonth);
                    if (!(salarymonth >= OFinancialYear.FromDate && salarymonth <= OFinancialYear.ToDate))
                    {
                        Msg.Add("Salary Month Is Out Of Financial Year");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                if (c.ExtraChallan == false)
                {
                    c.TaxAmount = Convert.ToDouble(taxamtextrachallanfalse);
                }
                else
                {
                    c.TaxAmount = Convert.ToDouble(taxamtextrachallanTrue);
                }
                if (Category != "")
                {
                    c.SalaryMonth = Paymonth.ToString("MM/yyyy");
                }
                if (TaxYrAmount != "0" && OtherTax.ToUpper() == "TRUE")
                {
                    TotalTaxAmount = c.TaxAmount + Convert.ToDouble(TaxYrAmount);
                }
                if (TaxYrAmount == "0" && OtherTax.ToUpper() == "TRUE")
                {
                    TotalTaxAmount = c.TaxAmount + Convert.ToDouble(TaxYrAmount);
                }
                if (TaxYrAmount1 != "0")
                {
                    TotalTaxAmount = Convert.ToDouble(TaxYrAmount1);
                }
                if (TaxAmount2 != "0" && OtherTax.ToUpper() == "FALSE")
                {
                    TotalTaxAmount = Convert.ToDouble(TaxAmount2);
                }
                //else
                //{
                //    TotalTaxAmount = c.TaxAmount;
                //}
                //Msg.Add("Data Saved Successfully.");
                //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                try
                {

                    if ((ModelState.IsValid) || !(ModelState.IsValid))
                    {
                        //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(2, 0, 0)))
                        //using (TransactionScope ts = new TransactionScope())
                        //{
                        //if (db.ITChallan.Any(o => o.Code == c.Code))
                        //{
                        //    Msg.Add("Code Already Exists.");
                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //}

                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        ITChallan corporate = new ITChallan()
                        {
                            ChallanNo = c.ChallanNo == null ? "" : c.ChallanNo,
                            BankBSRCode = c.BankBSRCode == null ? "" : c.BankBSRCode,
                            TaxDepositDate = c.TaxDepositDate,
                            ExtraChallan = c.ExtraChallan,
                            SalaryMonth = c.SalaryMonth == null ? "" : c.SalaryMonth,
                            TaxAmount = TotalTaxAmount,
                            ChallanNarration = c.ChallanNarration,
                            DBTrack = c.DBTrack,
                            FinancialYear = OFinancialYear
                        };

                        db.ITChallan.Add(corporate);
                        //   var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, c.DBTrack);
                        db.SaveChanges();
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);

                        //
                        if (ExtraChallan == "false")
                        {
                            var geoIds = db.ITaxTransT.Include(e => e.GeoStruct).Where(e => e.PayMonth == c.SalaryMonth).Select(e => e.GeoStruct.Id).Distinct().ToList();

                            foreach (var gID in geoIds)
                            {

                                List<ITaxTransT> OITaxTransT = db.ITaxTransT
                                    .Include(q => q.ITChallan)
                                    //.Include(q => q.EmployeePayroll)
                                    .Where(q => q.PayMonth == c.SalaryMonth && q.GeoStruct_Id == gID).ToList();
                                if (OITaxTransT.Count > 0)
                                {
                                    //OITaxTransT.ForEach(q =>
                                    //{
                                    //    q.ITChallan = corporate;
                                    //    q.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    //    db.ITaxTransT.Attach(q);
                                    //    db.Entry(q).State = System.Data.Entity.EntityState.Modified;

                                    //});

                                    OITaxTransT.ForEach(q =>
                                    {
                                        using (TransactionScope ts = new TransactionScope())
                                        {

                                            q.ITChallan = corporate;
                                            db.SaveChanges();
                                            ts.Complete();
                                        }

                                    });

                                }
                            }
                            if (HeadLoad_table != null && HeadLoad_table != "")
                            {
                                var Ohead_ids = Utility.StringIdsToListIds(HeadLoad_table);

                                foreach (var item in Ohead_ids)
                                {
                                    List<YearlyPaymentT> OOYearlyPaymentT = db.YearlyPaymentT
                                         .Where(a => a.PayMonth == c.SalaryMonth && a.ReleaseFlag == true && a.TDSAmount >= 0 && a.SalaryHead != null && a.SalaryHead.Id == item).ToList();
                                    if (OOYearlyPaymentT.Count > 0)
                                    {
                                        OOYearlyPaymentT.ForEach(q =>
                                        {
                                            using (TransactionScope ts1 = new TransactionScope())
                                            {
                                                q.ITChallan = corporate;
                                                q.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                db.YearlyPaymentT.Attach(q);
                                                db.Entry(q).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                ts1.Complete();
                                            }
                                        });
                                    }
                                    else
                                    {
                                        List<OthEarningDeductionT> OOtherEarningDeductionT = db.OthEarningDeductionT
                                            .Where(a => a.PayMonth == c.SalaryMonth && a.TDSAmount >= 0 && a.SalaryHead != null && a.SalaryHead.Id == item).ToList();

                                        if (OOtherEarningDeductionT.Count > 0)
                                        {
                                            OOtherEarningDeductionT.ForEach(q =>
                                            {
                                                using (TransactionScope ts2 = new TransactionScope())
                                                {
                                                    q.ITChallan = corporate;
                                                    q.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                    db.OthEarningDeductionT.Attach(q);
                                                    db.Entry(q).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    ts2.Complete();
                                                }
                                            });
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (HeadLoad1_table != null && HeadLoad1_table != "")
                            {
                                var Ohead_ids = Utility.StringIdsToListIds(HeadLoad1_table);

                                foreach (var item in Ohead_ids)
                                {
                                    List<YearlyPaymentT> OOYearlyPaymentT = db.YearlyPaymentT
                                         .Where(a => a.PayMonth == c.SalaryMonth && a.ReleaseFlag == true && a.TDSAmount >= 0 && a.SalaryHead != null && a.SalaryHead.Id == item).ToList();
                                    if (OOYearlyPaymentT.Count > 0)
                                    {
                                        OOYearlyPaymentT.ForEach(q =>
                                        {
                                            using (TransactionScope ts3 = new TransactionScope())
                                            {
                                                q.ITChallan = corporate;
                                                q.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                db.YearlyPaymentT.Attach(q);
                                                db.Entry(q).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                ts3.Complete();
                                            }
                                        });
                                    }
                                    else
                                    {
                                        List<OthEarningDeductionT> OOtherEarningDeductionT = db.OthEarningDeductionT
                                            .Where(a => a.PayMonth == c.SalaryMonth && a.TDSAmount >= 0 && a.SalaryHead != null && a.SalaryHead.Id == item).ToList();

                                        if (OOtherEarningDeductionT.Count > 0)
                                        {
                                            OOtherEarningDeductionT.ForEach(q =>
                                            {
                                                using (TransactionScope ts4 = new TransactionScope())
                                                {
                                                    q.ITChallan = corporate;
                                                    q.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                    db.OthEarningDeductionT.Attach(q);
                                                    db.Entry(q).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    ts4.Complete();
                                                }
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        //ts.Complete();
                        Msg.Add("Data Saved Successfully.");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // }
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
                        Msg.Add(errorMsg);
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

        [HttpPost]
        public ActionResult Edit(int data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ITChallan

                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        ChallanNo = e.ChallanNo,
                        BankBSRCode = e.BankBSRCode,
                        TaxDepositDate = e.TaxDepositDate,
                        ExtraChallan = e.ExtraChallan,
                        SalaryMonth = e.SalaryMonth,
                        TaxAmount = e.TaxAmount,
                        ChallanNarration = e.ChallanNarration,

                        Action = e.DBTrack.Action
                    }).ToList();

                var Corp = db.ITChallan.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave1(ITChallan c, int data, FormCollection form) // Edit submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                ITChallan blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.ITChallan.Where(e => e.Id == data)
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

                                int a = EditS(data, c, c.DBTrack);

                                using (var context = new DataBaseContext())
                                {
                                    // db.SaveChanges();
                                }
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

                            ITChallan blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            ITChallan Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.ITChallan.Where(e => e.Id == data).SingleOrDefault();
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

                            ITChallan corp = new ITChallan()
                            {
                                ChallanNo = c.ChallanNo == null ? "" : c.ChallanNo,
                                BankBSRCode = c.BankBSRCode == null ? "" : c.BankBSRCode,
                                TaxDepositDate = c.TaxDepositDate,
                                ExtraChallan = c.ExtraChallan,
                                SalaryMonth = c.SalaryMonth == null ? "" : c.SalaryMonth,
                                TaxAmount = c.TaxAmount,
                                ChallanNarration = c.ChallanNarration,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, corp, "ITChallan", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.ITChallan.Where(e => e.Id == data).SingleOrDefault();
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.ITChallan.Attach(blog);
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
                    var clientValues = (ITChallan)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (ITChallan)databaseEntry.ToObject();
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
        public int EditS(int data, ITChallan c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var CurCorp = db.ITChallan.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    ITChallan corp = new ITChallan()
                    {
                        ChallanNo = c.ChallanNo == null ? "" : c.ChallanNo,
                        BankBSRCode = c.BankBSRCode == null ? "" : c.BankBSRCode,
                        TaxDepositDate = c.TaxDepositDate,
                        ExtraChallan = c.ExtraChallan,
                        SalaryMonth = c.SalaryMonth == null ? "" : c.SalaryMonth,
                        TaxAmount = c.TaxAmount,
                        ChallanNarration = c.ChallanNarration,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.ITChallan.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    ITChallanEmpDetails cop = new ITChallanEmpDetails()
                    {
                        ChallanNo = c.ChallanNo == null ? "" : c.ChallanNo,
                        BankBSRCode = c.BankBSRCode == null ? "" : c.BankBSRCode,
                        TaxAmount = c.TaxAmount,
                        TaxDepositDate = c.TaxDepositDate,

                        DBTrack = c.DBTrack
                    };
                    db.ITChallanEmpDetails.Add(cop);

                    //db.Entry(cop).State = System.Data.Entity.EntityState.Modified;


                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(ITChallan c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Addrs = form["taxamt1"] == "0" ? "" : form["taxamt1"];
                    //string Addrs = form["TaxYrAmount1"] == "0" ? "" : form["TaxYrAmount1"];
                    string finance = form["FinancialYearList"] == "0" ? "" : form["FinancialYearList"];

                    int Fiyr = int.Parse(finance);
                    var OFinancialYear = db.Calendar.Where(e => e.Id == Fiyr).FirstOrDefault();

                    if (Addrs != "")
                    {
                        c.TaxAmount = Convert.ToInt32(Addrs);
                    }
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    ITChallan db_Data = db.ITChallan.Where(e => e.Id == data).FirstOrDefault();

                    List<ITChallanEmpDetails> itchallanid = db.ITChallanEmpDetails.Where(e => e.ChallanNo == c.ChallanNo).AsNoTracking().AsParallel().ToList();
                    DateTime salarymonth = Convert.ToDateTime("01/" + db_Data.SalaryMonth);
                    if (!(salarymonth >= OFinancialYear.FromDate && salarymonth <= OFinancialYear.ToDate))
                    {
                        Msg.Add("Salary Month Is Out Of Financial Year");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (itchallanid.Count() > 0)
                    {
                        Msg.Add("Record Updated");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    // if record create in challan and edit and save that challan. after that re edit then please dete this challan record then re create and edit.
                    List<ITChallanEmpDetails> itchallanidreedit = db.ITChallanEmpDetails.Where(e => e.ChallanNo == db_Data.ChallanNo).AsNoTracking().AsParallel().ToList();
                    if (itchallanidreedit.Count() > 0)
                    {
                        Msg.Add("This Record Already Edit and save.Please Delete This Record. Recreate for this challan No. After Recreating Edit and Save");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (db_Data.ExtraChallan == true)
                    {
                        List<YearlyPaymentT> monthlytax = db.YearlyPaymentT.Where(e => e.PayMonth == db_Data.SalaryMonth && e.TDSAmount != 0 && e.SalaryHead.InPayslip.ToString().ToUpper() == "FALSE" && e.ITChallan == null).AsNoTracking().AsParallel().ToList();
                        if (monthlytax.Count() > 0)
                        {
                            Msg.Add("Please delete this month challan and recreate it");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        List<ITaxTransT> monthlytax = db.ITaxTransT.Where(e => e.PayMonth == db_Data.SalaryMonth && e.ITChallan == null).AsNoTracking().AsParallel().ToList();
                        if (monthlytax.Count() > 0)
                        {
                            Msg.Add("Please delete this month challan and recreate it");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                //           new System.TimeSpan(0, 60, 0)))
                                //{
                                //db.ITChallan.Attach(db_Data);
                                //db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                                //TempData["RowVersion"] = db_Data.RowVersion;
                                //db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_OBJ = db.ITChallan.Find(data);
                                TempData["CurrRowVersion"] = Curr_OBJ.RowVersion;
                                db.Entry(Curr_OBJ).State = System.Data.Entity.EntityState.Detached;

                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    ITChallan blog = blog = null;
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ITChallan.Where(e => e.Id == data).FirstOrDefault();
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    ITChallan lk = new ITChallan
                                    {
                                        ChallanNo = c.ChallanNo == null ? "" : c.ChallanNo,
                                        BankBSRCode = db_Data.BankBSRCode == null ? "" : db_Data.BankBSRCode,
                                        TaxDepositDate = db_Data.TaxDepositDate,
                                        ExtraChallan = db_Data.ExtraChallan,
                                        SalaryMonth = db_Data.SalaryMonth == null ? "" : db_Data.SalaryMonth,
                                        TaxAmount = db_Data.TaxAmount,
                                        ChallanNarration = db_Data.ChallanNarration,
                                        Id = data,
                                        DBTrack = c.DBTrack
                                    };


                                    db.ITChallan.Attach(lk);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                    db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    db.SaveChanges();
                                    List<ITChallanEmpDetails> empccount = db.ITChallanEmpDetails.Where(t => t.ChallanNo == blog.ChallanNo).ToList();
                                    //  if (blog.ChallanNo == c.ChallanNo)
                                    if (empccount.Count() == 0)
                                    {
                                        if (db_Data.ExtraChallan == true)
                                        {

                                            //var OFinYr = db.Calendar.Include(e => e.Name)
                                            //  .Where(e => e.Default == true && e.Name.LookupVal.ToString().ToUpper() == "FINANCIALYEAR").SingleOrDefault();
                                            List<EmployeePayroll> v1 = db.EmployeePayroll
                                                //.Include(a => a.OtherEarningDeductionT)
                                                //.Include(a => a.OtherEarningDeductionT.Select(a1 => a1.ITChallan))
                                                //.Include(a => a.OtherEarningDeductionT.Select(a1 => a1.SalaryHead))
                                                .Include(e => e.ITChallanEmpDetails)
                                                //.Include(e => e.YearlyPaymentT.Select(a2 => a2.ITChallan))
                                                //.Include(e => e.YearlyPaymentT.Select(a2 => a2.SalaryHead))
                                                .AsNoTracking().AsParallel().ToList();
                                            foreach (EmployeePayroll emp in v1)
                                            {
                                                //var d = emp.YearlyPaymentT.Where(a => a.PayMonth == lk.SalaryMonth && a.SalaryHead.InPayslip.ToString().ToUpper() == "FALSE").FirstOrDefault();
                                                List<OthEarningDeductionT> d1 = db.OthEarningDeductionT.Include(e => e.ITChallan).Include(e => e.SalaryHead)
                                                    .Where(a => a.PayMonth == lk.SalaryMonth
                                                         && a.TDSAmount != 0 && a.SalaryHead.InPayslip.ToString().ToUpper() == "FALSE"
                                                        && a.ITChallan.Id == lk.Id && a.EmployeePayroll.Id == emp.Id).ToList();

                                                double d = 0;
                                                double yearylygross = 0;
                                                var OYrPay = db.YearlyPaymentT.Include(e => e.ITChallan).Include(e => e.SalaryHead)
                                                        .Where(a => a.PayMonth == lk.SalaryMonth
                                                            && a.ITChallan != null && a.SalaryHead.InPayslip.ToString().ToUpper() == "FALSE"
                                                            && a.ITChallan.Id == lk.Id && a.EmployeePayroll.Id == emp.Id).ToList();
                                                // .Sum(e => e.TDSAmount);
                                                if (OYrPay != null && OYrPay.Count() > 0)
                                                {
                                                    d = db.YearlyPaymentT.Include(e => e.ITChallan).Include(e => e.SalaryHead)
                                                                                                      .Where(a => a.PayMonth == lk.SalaryMonth
                                                                                                          && a.ITChallan != null && a.SalaryHead.InPayslip.ToString().ToUpper() == "FALSE"
                                                                                                          && a.ITChallan.Id == lk.Id && a.EmployeePayroll.Id == emp.Id)
                                                   .Sum(e => e.TDSAmount);

                                                    yearylygross = db.YearlyPaymentT.Include(e => e.ITChallan).Include(e => e.SalaryHead)
                                                      .Where(a => a.PayMonth == lk.SalaryMonth
                                                            && a.ITChallan != null
                                                            && a.SalaryHead.InPayslip.ToString().ToUpper() == "FALSE"
                                                          && a.ITChallan.Id == lk.Id && a.EmployeePayroll.Id == emp.Id)
                                                          .Sum(e => e.AmountPaid);
                                                }

                                                //double amountpaid = emp.YearlyPaymentT.
                                                //    Where(a => a.PayMonth == lk.SalaryMonth
                                                //        && a.SalaryHead.InPayslip.ToString().ToUpper() == "FALSE"
                                                //        && a.ITChallan.ChallanNo == c.ChallanNo)
                                                //        .Sum(e => e.AmountPaid);

                                                foreach (OthEarningDeductionT item1 in d1)
                                                {
                                                    double amt = item1.TDSAmount == null ? 0 : item1.TDSAmount;
                                                    sum1 = sum1 + amt;
                                                    //double Gramt = item1.SalAmount == null ? 0 : item1.SalAmount;
                                                    //Grsum = Grsum + Gramt;
                                                }
                                                //   double d2 = d == null ? 0 : d.TDSAmount == null ? 0 : d.TDSAmount;
                                                // var vv = emp.ITaxTransT.Where(a => a.PayMonth == lk.SalaryMonth).FirstOrDefault();
                                                if (d >= 0 || d1.Count() > 0)
                                                {
                                                    ITChallanEmpDetails cop = new ITChallanEmpDetails()
                                                    {
                                                        ChallanNo = c.ChallanNo == null ? "" : c.ChallanNo,
                                                        BankBSRCode = db_Data.BankBSRCode == null ? "" : db_Data.BankBSRCode,
                                                        TaxAmount = d + sum1,
                                                        TaxDepositDate = db_Data.TaxDepositDate,
                                                        DBTrack = c.DBTrack,
                                                        TaxableIncome = yearylygross,
                                                        Calendar = OFinancialYear
                                                    };
                                                    db.ITChallanEmpDetails.Add(cop);
                                                    db.SaveChanges();

                                                    EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Include(e => e.ITChallanEmpDetails).Where(e => e.Id == emp.Id).FirstOrDefault();

                                                    List<ITChallanEmpDetails> ITChallanEmpDet = new List<ITChallanEmpDetails>();
                                                    if (OEmployeePayroll.ITChallanEmpDetails != null)
                                                    {
                                                        ITChallanEmpDet.AddRange(OEmployeePayroll.ITChallanEmpDetails);
                                                    }
                                                    ITChallanEmpDet.Add(cop);

                                                    //int OEmpId = emp.Id;

                                                    //var aa = db.EmployeePayroll.Find(OEmpId);

                                                    OEmployeePayroll.ITChallanEmpDetails = ITChallanEmpDet;
                                                    db.EmployeePayroll.Attach(OEmployeePayroll);
                                                    db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                            }
                                            db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                        }
                                        else
                                        {
                                            //var OFinYr = db.Calendar.Include(e => e.Name)
                                            //    .Where(e => e.Default == true && e.Name.LookupVal.ToString().ToUpper() == "FINANCIALYEAR").SingleOrDefault();
                                            List<int> idsE = new List<int>();
                                            var OYearlypaymentt = db.EmployeePayroll.Include(t => t.YearlyPaymentT).Include(t => t.SalaryT).AsNoTracking().AsParallel().ToList();
                                            foreach (var item in OYearlypaymentt)
                                            {
                                                if (item.YearlyPaymentT.Any(e => e.PayMonth == db_Data.SalaryMonth))
                                                {
                                                    idsE.Add(item.Id);
                                                }
                                            }

                                            // var OItTranst = db.EmployeePayroll.AsNoTracking().AsParallel().ToList();
                                            foreach (var item in OYearlypaymentt)
                                            {
                                                var test = item.SalaryT.Where(e => e.PayMonth == db_Data.SalaryMonth).ToList();
                                                foreach (var t1 in test)
                                                {
                                                    idsE.Add(item.Id);
                                                }
                                            }
                                            List<int> employeeidlist = idsE.Distinct().ToList();

                                            //List<EmployeePayroll> employeeidlist = db.EmployeePayroll.AsNoTracking().ToList();
                                            foreach (int item in employeeidlist)
                                            {

                                                EmployeePayroll emp = db.EmployeePayroll.Where(e => e.Id == item)
                                                    //.Include(a => a.OtherEarningDeductionT)
                                                    //.Include(a => a.OtherEarningDeductionT.Select(a1 => a1.ITChallan))
                                                    //.Include(a => a.OtherEarningDeductionT.Select(a1 => a1.SalaryHead))
                                                    //  .Include(e => e.ITChallanEmpDetails)
                                                    //.Include(e => e.YearlyPaymentT)
                                                    //.Include(e => e.ITaxTransT)
                                                    //.Include(e => e.ITaxTransT.Select(t => t.ITChallan))
                                                    //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(s => s.SalaryHead.Type)))
                                                    //.Include(e => e.SalaryArrearT)
                                                    //.Include(e => e.SalaryArrearT.Select(t => t.SalaryArrearPaymentT.Select(q => q.SalaryHead.Type)))
                                                    //.Include(e => e.YearlyPaymentT.Select(a2 => a2.ITChallan))
                                                    //.Include(e => e.YearlyPaymentT.Select(a2 => a2.FinancialYear))
                                                    //.Include(e => e.YearlyPaymentT.Select(a2 => a2.SalaryHead))
                                                    //.Include(e => e.PerkTransT).Include(e => e.PerkTransT.Select(t => t.SalaryHead))
                                                    //.Include(e => e.PerkTransT.Select(t => t.SalaryHead.Frequency))
                                                     .AsNoTracking().FirstOrDefault();

                                                sum = 0;
                                                double OTaxableAmt1 = 0;
                                                //foreach (var emp in v1)
                                                //{
                                                OTaxableAmt1 = 0;
                                                SalaryT OTaxableAmt = db.SalaryT.Include(e => e.SalEarnDedT).Include(e => e.SalEarnDedT.Select(t => t.SalaryHead.Type)).Where(e => e.PayMonth == lk.SalaryMonth && e.EmployeePayroll.Id == emp.Id).AsNoTracking().AsParallel().FirstOrDefault();

                                                List<SalaryArrearT> OtaxableArrear = db.SalaryArrearT.Include(e => e.SalaryArrearPaymentT).Include(e => e.SalaryArrearPaymentT.Select(t => t.SalaryHead.Type)).Where(e => e.PayMonth == lk.SalaryMonth && e.EmployeePayroll.Id == emp.Id).AsNoTracking().AsParallel().ToList();

                                                List<OthEarningDeductionT> d1 = db.OthEarningDeductionT.Include(e => e.ITChallan).Include(e => e.SalaryHead)
                                                  .Where(a => a.PayMonth == lk.SalaryMonth
                                                     && a.TDSAmount >= 0 && (a.ITChallan != null && a.ITChallan.Id == lk.Id && a.EmployeePayroll_Id == emp.Id)).AsNoTracking().AsParallel().ToList();

                                                List<YearlyPaymentT> checkre = db.YearlyPaymentT.Include(e => e.ITChallan).Include(e => e.SalaryHead).Include(e => e.FinancialYear)
                                                    .Where(a => a.PayMonth == lk.SalaryMonth
                                                       && a.TDSAmount >= 0 && (a.ITChallan != null && a.ITChallan.Id == lk.Id && a.EmployeePayroll_Id == emp.Id)).AsNoTracking().AsParallel().ToList();
                                                double d = 0;
                                                if (checkre.Count() > 0)
                                                {

                                                    d = checkre.Sum(e => e.TDSAmount);
                                                }


                                                //double amountpaid = emp.YearlyPaymentT.
                                                //    Where(a => a.PayMonth == lk.SalaryMonth
                                                //        && a.ITChallan.Id == lk.Id)
                                                //        .Sum(e => e.AmountPaid);
                                                double Gramt = 0;
                                                foreach (OthEarningDeductionT item1 in d1)
                                                {
                                                    double amt = item1.TDSAmount == null ? 0 : item1.TDSAmount;
                                                    //sum1 = sum1 + amt;
                                                    // double Gramt = item1.SalAmount == null ? 0 : item1.SalAmount;

                                                    Gramt = Gramt + amt;
                                                }

                                                if (d >= 0)
                                                {
                                                    Gramt = Gramt + d;
                                                }

                                                ITaxTransT vv = db.ITaxTransT.Include(e => e.ITChallan).Where(a => a.ITChallan != null && a.PayMonth == lk.SalaryMonth && a.ITChallan.Id == lk.Id && a.EmployeePayroll.Id == emp.Id).AsNoTracking().AsParallel().FirstOrDefault();
                                                if (vv != null)
                                                {
                                                    if (OTaxableAmt != null)
                                                    {
                                                        double OTaxableSal = 0;
                                                        double OTaxableArrear = 0;
                                                        var smd = OtaxableArrear.SelectMany(r => r.SalaryArrearPaymentT.Where(t => t.SalaryHead.InITax == true && t.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"));
                                                        OTaxableArrear = smd.Sum(t => t.SalHeadAmount);
                                                        OTaxableSal = OTaxableAmt.SalEarnDedT.Where(r => r.SalaryHead.InITax == true && r.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").Sum(e => e.Amount);
                                                        OTaxableAmt1 = OTaxableArrear + OTaxableSal;
                                                    }
                                                }
                                                else// manual tax not enter but salary has done
                                                {
                                                    if (OTaxableAmt != null)
                                                    {
                                                        double OTaxableSal = 0;
                                                        double OTaxableArrear = 0;
                                                        var smd = OtaxableArrear.SelectMany(r => r.SalaryArrearPaymentT.Where(t => t.SalaryHead.InITax == true && t.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"));
                                                        OTaxableArrear = smd.Sum(t => t.SalHeadAmount);
                                                        OTaxableSal = OTaxableAmt.SalEarnDedT.Where(r => r.SalaryHead.InITax == true && r.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").Sum(e => e.Amount);
                                                        OTaxableAmt1 = OTaxableArrear + OTaxableSal;
                                                    }
                                                }
                                                //perk value taken in salearndedt so this code comment
                                                //PerkTransT OPerkTransT = db.PerkTransT.Include(e => e.SalaryHead.Frequency).Where(a => a.PayMonth == lk.SalaryMonth && a.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY" && a.EmployeePayroll.Id == emp.Id).AsNoTracking().AsParallel().FirstOrDefault();
                                                //if (OPerkTransT != null)
                                                //{
                                                //    double OPerkAmt = 0;
                                                //    OPerkAmt = db.PerkTransT.Include(e => e.SalaryHead.Frequency)
                                                //    .Where(a => a.PayMonth == lk.SalaryMonth && a.ActualAmount >= 0 &&
                                                //    a.SalaryHead.InITax == true && a.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY" && a.EmployeePayroll.Id == emp.Id).Sum(e => e.ActualAmount);
                                                //    OTaxableAmt1 = OTaxableAmt1 + OPerkAmt;
                                                //}

                                                //if (lk.SalaryMonth.ToString().Split('/')[0] == "03")
                                                //{
                                                //    List<string> mPeriodYear = new List<string>();
                                                //    string mPeriodRangeYear = "";
                                                //    for (DateTime? mTempDate = OFinancialYear.FromDate; mTempDate <= OFinancialYear.ToDate; mTempDate = mTempDate.Value.AddMonths(1))
                                                //    {
                                                //        if (mPeriodRangeYear == "")
                                                //        {
                                                //            mPeriodRangeYear = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                                                //            mPeriodYear.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                                                //        }
                                                //        else
                                                //        {
                                                //            mPeriodRangeYear = mPeriodRangeYear + "," + Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                                                //            mPeriodYear.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                                                //        }
                                                //    }

                                                //    // OPerkTransT = db.PerkTransT.Include(e => e.SalaryHead.Frequency).Where(a => a.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY" && a.EmployeePayroll.Id == emp.Id 
                                                //    //   && Convert.ToDateTime("01/" + a.PayMonth).Date >= OFinancialYear.FromDate &&
                                                //    // Convert.ToDateTime("01/" + a.PayMonth).Date <= OFinancialYear.ToDate).FirstOrDefault();
                                                //    OPerkTransT = db.PerkTransT.Include(e => e.SalaryHead.Frequency).Where(a => a.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY" && a.EmployeePayroll.Id == emp.Id
                                                //        && mPeriodYear.Contains(a.PayMonth)).FirstOrDefault();
                                                //    if (OPerkTransT != null)
                                                //    {
                                                //        double OPerkAmt = 0;
                                                //        OPerkAmt = db.PerkTransT.Include(e => e.SalaryHead.Frequency).Where(a => a.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY" && a.EmployeePayroll.Id == emp.Id
                                                //        && mPeriodYear.Contains(a.PayMonth)).Sum(e => e.ActualAmount);
                                                //        OTaxableAmt1 = OTaxableAmt1 + OPerkAmt;
                                                //    }
                                                //}



                                                if (vv != null)
                                                {
                                                    ITChallanEmpDetails cop = new ITChallanEmpDetails()
                                                    {
                                                        ChallanNo = c.ChallanNo == null ? "" : c.ChallanNo,
                                                        BankBSRCode = db_Data.BankBSRCode == null ? "" : db_Data.BankBSRCode,
                                                        TaxAmount = vv.TaxPaid,
                                                        TaxDepositDate = db_Data.TaxDepositDate,
                                                        DBTrack = c.DBTrack,
                                                        TaxableIncome = OTaxableAmt1,
                                                        Calendar = OFinancialYear
                                                    };
                                                    db.ITChallanEmpDetails.Add(cop);
                                                    db.SaveChanges();

                                                    EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Include(e => e.ITChallanEmpDetails).Where(e => e.Id == emp.Id).FirstOrDefault();

                                                    List<ITChallanEmpDetails> ITChallanEmpDet = new List<ITChallanEmpDetails>();
                                                    if (OEmployeePayroll.ITChallanEmpDetails != null)
                                                    {
                                                        ITChallanEmpDet.AddRange(OEmployeePayroll.ITChallanEmpDetails);
                                                    }
                                                    ITChallanEmpDet.Add(cop);

                                                    //int OEmpId = emp.Id;

                                                    //var aa = db.EmployeePayroll.Find(OEmpId);
                                                    OEmployeePayroll.ITChallanEmpDetails = ITChallanEmpDet;
                                                    db.EmployeePayroll.Attach(OEmployeePayroll);
                                                    db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                                else
                                                {
                                                    ITChallanEmpDetails cop = new ITChallanEmpDetails()
                                                    {
                                                        ChallanNo = c.ChallanNo == null ? "" : c.ChallanNo,
                                                        BankBSRCode = db_Data.BankBSRCode == null ? "" : db_Data.BankBSRCode,
                                                        TaxAmount = 0,
                                                        TaxDepositDate = db_Data.TaxDepositDate,
                                                        DBTrack = c.DBTrack,
                                                        TaxableIncome = OTaxableAmt1,
                                                        Calendar = OFinancialYear
                                                    };
                                                    db.ITChallanEmpDetails.Add(cop);
                                                    db.SaveChanges();

                                                    EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Include(e => e.ITChallanEmpDetails).Where(e => e.Id == emp.Id).FirstOrDefault();

                                                    List<ITChallanEmpDetails> ITChallanEmpDet = new List<ITChallanEmpDetails>();
                                                    if (OEmployeePayroll.ITChallanEmpDetails != null)
                                                    {
                                                        ITChallanEmpDet.AddRange(OEmployeePayroll.ITChallanEmpDetails);
                                                    }
                                                    ITChallanEmpDet.Add(cop);

                                                    //int OEmpId = emp.Id;

                                                    //var aa = db.EmployeePayroll.Find(OEmpId);
                                                    OEmployeePayroll.ITChallanEmpDetails = ITChallanEmpDet;
                                                    db.EmployeePayroll.Attach(OEmployeePayroll);
                                                    db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                                if (Gramt >= 0)
                                                {
                                                    List<YearlyPaymentT> yearlygross = db.YearlyPaymentT.Include(e => e.SalaryHead).Include(e => e.ITChallan).Include(e => e.FinancialYear)
                                                        .Where(e => e.ITChallan != null && e.ITChallan.Id == lk.Id && e.EmployeePayroll.Id == emp.Id
                                                    && e.SalaryHead.InPayslip == false
                                                    && e.FinancialYear.Id == OFinancialYear.Id && e.PayMonth == lk.SalaryMonth).ToList();
                                                    double Oyearlygross = 0;
                                                    Oyearlygross = yearlygross.Sum(e => e.AmountPaid);
                                                    if (yearlygross.Count() > 0)
                                                    {
                                                        ITChallanEmpDetails cop = new ITChallanEmpDetails()
                                                        {
                                                            ChallanNo = c.ChallanNo == null ? "" : c.ChallanNo,
                                                            BankBSRCode = db_Data.BankBSRCode == null ? "" : db_Data.BankBSRCode,
                                                            TaxAmount = Gramt,
                                                            TaxDepositDate = db_Data.TaxDepositDate,
                                                            TaxableIncome = Oyearlygross,
                                                            DBTrack = c.DBTrack,
                                                            Calendar = OFinancialYear
                                                        };
                                                        db.ITChallanEmpDetails.Add(cop);
                                                        db.SaveChanges();

                                                        EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Include(e => e.ITChallanEmpDetails).Where(e => e.Id == emp.Id).FirstOrDefault();

                                                        List<ITChallanEmpDetails> ITChallanEmpDet = new List<ITChallanEmpDetails>();
                                                        if (OEmployeePayroll.ITChallanEmpDetails != null)
                                                        {
                                                            ITChallanEmpDet.AddRange(OEmployeePayroll.ITChallanEmpDetails);
                                                        }
                                                        ITChallanEmpDet.Add(cop);

                                                        //int OEmpId = emp.Id;

                                                        //var aa = db.EmployeePayroll.Find(OEmpId);

                                                        OEmployeePayroll.ITChallanEmpDetails = ITChallanEmpDet;
                                                        db.EmployeePayroll.Attach(OEmployeePayroll);
                                                        db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                                                        db.SaveChanges();
                                                    }

                                                }
                                                // }
                                                //}
                                            }
                                            db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                            //ts.Complete();
                                        }
                                    }
                                    else
                                    {
                                        db.ITChallanEmpDetails.Where(t => t.ChallanNo == blog.ChallanNo).ToList().ForEach(s => s.ChallanNo = c.ChallanNo);
                                        db.SaveChanges();
                                    }
                                    Msg.Add("Record Updated");
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    // return Json(new Object[] { lk.Id, lk.PreferredHospital, "Record Updated", JsonRequestBehavior.AllowGet });
                                    //}
                                }
                            }

                            catch (DbUpdateException e) { throw e; }
                            catch (DataException e) { throw e; }
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

                            //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        }
                    }


                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            ITChallan blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            ITChallan Old_OBJ = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.ITChallan
                                    .Where(e => e.Id == data).SingleOrDefault();
                                TempData["RowVersion"] = blog.RowVersion;
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
                            ITChallan ITChallans = new ITChallan()
                            {


                                ChallanNo = blog.ChallanNo == null ? "" : blog.ChallanNo,
                                BankBSRCode = blog.BankBSRCode == null ? "" : blog.BankBSRCode,
                                TaxDepositDate = blog.TaxDepositDate,
                                ExtraChallan = blog.ExtraChallan,
                                SalaryMonth = blog.SalaryMonth == null ? "" : blog.SalaryMonth,
                                TaxAmount = blog.TaxAmount,
                                ChallanNarration = blog.ChallanNarration,
                                Id = data,
                                DBTrack = blog.DBTrack,


                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;
                            db.ITChallan.Attach(ITChallans);
                            db.Entry(ITChallans).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ITChallans).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            TempData["RowVersion"] = db_Data.RowVersion;
                            db.Entry(ITChallans).State = System.Data.Entity.EntityState.Detached;

                            using (var context = new DataBaseContext())
                            {


                                Old_OBJ = context.ITChallan.Where(e => e.Id == data)
                             .SingleOrDefault();

                                //db.SaveChanges();
                            }

                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { blog.Id, c.PreferredHospital, "Record Updated", JsonRequestBehavior.AllowGet });
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
                IEnumerable<ITChallan> ITChallan = null;
                if (gp.IsAutho == true)
                {
                    ITChallan = db.ITChallan.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    ITChallan = db.ITChallan.AsNoTracking().ToList();
                }

                IEnumerable<ITChallan> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ITChallan;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.ChallanNo.ToString().Contains(gp.searchString))
                                || (e.BankBSRCode.ToString().Contains(gp.searchString))
                                || (e.TaxDepositDate.Value.ToShortDateString().ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.TaxAmount.ToString().Contains(gp.searchString))
                                || (e.SalaryMonth.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.ExtraChallan.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.ChallanNarration != null ? e.ChallanNarration.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.ChallanNo, a.BankBSRCode, a.TaxDepositDate.Value.ToShortDateString(), a.TaxAmount, a.SalaryMonth, a.ExtraChallan.ToString(), a.ChallanNarration != null ? a.ChallanNarration : "", a.Id }).ToList();

                        //jsonData = IE.Select(a => new { a.ChallanNo, a.BankBSRCode, a.Id }).Where((e => (e.BankBSRCode.ToString().Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ChallanNo, a.BankBSRCode, a.TaxDepositDate.Value.ToShortDateString(), a.TaxAmount, a.SalaryMonth, a.ExtraChallan, a.ChallanNarration, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ITChallan;
                    Func<ITChallan, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "ChallanNo" ? c.ChallanNo :
                                         gp.sidx == "BankBSRCode" ? c.BankBSRCode :
                                          gp.sidx == "TaxDepositDate" ? c.TaxDepositDate.Value.ToShortDateString() :
                                           gp.sidx == "TaxAmount" ? c.TaxAmount.ToString() :
                                            gp.sidx == "SalaryMonth" ? c.SalaryMonth :
                                             gp.sidx == "ExtraChallan" ? c.ExtraChallan.ToString() :
                                             gp.sidx == "ChallanNarration" ? c.ChallanNarration :
                                      "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.ChallanNo), Convert.ToString(a.BankBSRCode), a.TaxDepositDate.Value.ToShortDateString(), Convert.ToString(a.TaxAmount), Convert.ToString(a.SalaryMonth), Convert.ToString(a.ExtraChallan), Convert.ToString(a.ChallanNarration), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.ChallanNo), Convert.ToString(a.BankBSRCode), a.TaxDepositDate.Value.ToShortDateString(), Convert.ToString(a.TaxAmount), Convert.ToString(a.SalaryMonth), Convert.ToString(a.ExtraChallan), Convert.ToString(a.ChallanNarration), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ChallanNo, a.BankBSRCode, a.TaxDepositDate.Value.ToShortDateString(), a.TaxAmount, a.SalaryMonth, a.ExtraChallan, a.ChallanNarration, a.Id }).ToList();
                    }
                    totalRecords = ITChallan.Count();
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
        public class P2BCrGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }

        }

        public ActionResult LoadEmp(P2BGrid_Parameters gp, FormCollection form)
        {
            try
            {
                DateTime? dt = null;
                string monthyr = "";
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                double totalSum = 0;
                var jsonData = (Object)null;
                IEnumerable<empdetails> EmpList = null;
                List<empdetails> model = new List<empdetails>();
                empdetails view = null;
                string v = "";
                string Month = "";
                if (gp.filter == null)
                {
                    List<string> Msg = new List<string>();
                    Msg.Add("Select Paymonth");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                if (gp.filter != null)
                    v = gp.filter;
                else
                {
                    if (DateTime.Now.Date.Month < 10)
                        Month = "0" + DateTime.Now.Date.Month;
                    else
                        Month = DateTime.Now.Date.Month.ToString();
                    //  PayMonth = Month + "/" + DateTime.Now.Date.Year;
                }
                if (!db.SalaryT.Any(a => a.PayMonth == v))
                {
                    List<string> Msg = new List<string>();
                    Msg.Add("Salary is not defined For this Month=" + v);

                    // return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                var v1 = db.EmployeePayroll.Select(q => q.Id).ToList().AsParallel();
                sum = 0;
                SalEarnDedT aa = db.SalEarnDedT.Include(b => b.SalaryHead).Include(b => b.SalaryHead.SalHeadOperationType).Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ITAX").FirstOrDefault();
                foreach (var emp in v1)
                {
                    EmployeePayroll va1 = db.EmployeePayroll.Include(a => a.Employee).Include(a => a.Employee.EmpName).Include(a => a.ITaxTransT).Include(a => a.SalaryT)
                        .Include(q => q.YearlyPaymentT).AsNoTracking()
                        .Where(q => q.Id == emp).SingleOrDefault();

                    ITaxTransT vv = va1.ITaxTransT.Where(a => a.PayMonth == v).FirstOrDefault();
                    // YearlyPaymentT vvv = va1.YearlyPaymentT.Where(a => a.PayMonth == v && a.ReleaseFlag == true).FirstOrDefault();
                    //  var v2 = emp.SalaryT.Where(a => a.PayMonth == v).Select(r => r.SalEarnDedT).FirstOrDefault();
                    SalaryT v3 = va1.SalaryT.Where(a => a.PayMonth == v).FirstOrDefault();
                    double totgross = 0;
                    if (v3 != null)
                    {
                        totgross = v3.TotalEarning + v3.TotalDeduction;
                    }

                    if (vv != null)
                    {
                        if (aa != null)
                        {
                            double tdsamt = 0;
                            //if (vvv != null)
                            //{
                            //    tdsamt = vvv.TDSAmount;
                            //}
                            sum = sum + vv.TaxPaid + tdsamt;
                            //   model.Add(new empdetails
                            view = new empdetails()
                            {
                                Id = va1.Employee.Id,
                                EmpCode = va1.Employee.EmpCode,
                                EmpName = va1.Employee.EmpName.FullNameFML,
                                Amount = vv.TaxPaid == null ? 0 : vv.TaxPaid,
                                //Gross = aa.Amount
                                Gross = totgross

                            };
                            model.Add(view);
                        }
                    }

                }

                EmpList = model;

                IEnumerable<empdetails> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                            || (e.EmpName.ToString().Contains(gp.searchString))
                            || (e.Amount.ToString().Contains(gp.searchString))
                             || (e.Gross.ToString().Contains(gp.searchString))
                            ).Select(a => new { a.EmpCode, a.EmpName, a.Amount, a.Gross }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Amount, a.Gross, sum }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpList;
                    Func<empdetails, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.ToString() :
                                         gp.sidx == "Amount" ? c.Amount.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.EmpCode, Convert.ToString(a.EmpName), Convert.ToString(a.Amount), Convert.ToString(a.Gross), Convert.ToString(sum) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.EmpCode, Convert.ToString(a.EmpName), Convert.ToString(a.Amount), Convert.ToString(a.Gross), Convert.ToString(sum) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpCode, Convert.ToString(a.EmpName), a.Amount, a.Gross, sum }).ToList();
                    }
                    totalRecords = EmpList.Count();
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
                    total = totalPages,
                    totalSum = sum
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult LoadEmpYearly(P2BGrid_Parameters gp, string paym, string head_ids)
        {
            try
            {
                var Ohead_ids = Utility.StringIdsToListIds(head_ids);

                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                double totalSum = 0;
                var jsonData = (Object)null;
                IEnumerable<empdetails> EmpList = null;
                List<empdetails> model = new List<empdetails>();
                empdetails view = null;

                if (!db.SalaryT.Any(a => a.PayMonth == paym))
                {
                    List<string> Msg = new List<string>();
                    Msg.Add("Salary is not defined For this Month=" + paym);
                    // return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                var v1 = db.EmployeePayroll.AsNoTracking().AsParallel().Select(q => q.Id).ToList();
                sumYr = 0;
                //YearlyPaymentT aa = db.YearlyPaymentT
                //                     .Include(b => b.SalaryHead)
                //                     .Include(b => b.SalaryHead.Type)
                //                     .Where(e => e.TDSAmount != 0 && e.PayMonth == paym)
                //                     .AsNoTracking().AsParallel().FirstOrDefault();

                foreach (var emp in v1)
                {
                    Utility.DumpProcessStatus(LineNo: emp);
                    EmployeePayroll va1 = db.EmployeePayroll
                                            .Include(a => a.Employee)
                                            .Include(a => a.Employee.EmpName)
                                            .Include(q => q.YearlyPaymentT)
                                            .Include(q => q.YearlyPaymentT.Select(qa => qa.SalaryHead))
                                            .Include(q => q.OtherEarningDeductionT)
                                            .Include(q => q.OtherEarningDeductionT.Select(qa => qa.SalaryHead))
                                            .Where(q => q.Id == emp).AsNoTracking().AsParallel()
                                            .SingleOrDefault();

                    double TotalTDSAmt = 0;
                    double TotalAmtGross = 0;

                    foreach (var item in Ohead_ids)
                    {

                        List<YearlyPaymentT> OYearlyPaymentT = va1.YearlyPaymentT
                            .Where(a => a.PayMonth == paym && a.ReleaseFlag == true && a.TDSAmount >= 0 && a.SalaryHead != null && a.SalaryHead.Id == item).ToList();
                        double yearlyTDSAmt = 0;
                        double yearlyAmtPaid = 0;
                        double OthTDSAmt = 0;
                        double OthAmtPaid = 0;

                        if (OYearlyPaymentT.Count > 0)
                        {
                            yearlyTDSAmt = OYearlyPaymentT.Sum(q => q.TDSAmount);
                            yearlyAmtPaid = OYearlyPaymentT.Sum(q => q.AmountPaid);
                        }
                        List<OthEarningDeductionT> OOtherEarningDeductionT = va1.OtherEarningDeductionT
                            .Where(a => a.PayMonth == paym && a.TDSAmount >= 0 && a.SalaryHead != null && a.SalaryHead.Id == item).ToList();
                        if (OOtherEarningDeductionT.Count > 0)
                        {
                            OthTDSAmt = OOtherEarningDeductionT.Sum(q => q.TDSAmount);
                            OthAmtPaid = OOtherEarningDeductionT.Sum(q => q.SalAmount);
                        }

                        SalaryHead sa = db.SalaryHead.Where(q => q.Id == item).SingleOrDefault();
                        if (sa.InPayslip == true)
                        {
                            TotalTDSAmt = yearlyTDSAmt + OthTDSAmt;
                        }
                        else
                        {
                            TotalTDSAmt = yearlyTDSAmt + OthTDSAmt;
                            TotalAmtGross = yearlyAmtPaid + OthAmtPaid;
                        }

                        if (TotalTDSAmt >= 0 || TotalAmtGross >= 0)
                        {
                            sumYr = sumYr + TotalTDSAmt;
                            view = new empdetails()
                            {
                                Id = va1.Employee.Id,
                                EmpCode = va1.Employee.EmpCode,
                                EmpName = va1.Employee.EmpName.FullNameFML,
                                Amount = TotalTDSAmt,
                                Gross = TotalAmtGross
                            };
                            model.Add(view);
                        }
                    }
                }

                EmpList = model;

                IEnumerable<empdetails> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                            || (e.EmpName.ToString().Contains(gp.searchString))
                            || (e.Amount.ToString().Contains(gp.searchString))
                             || (e.Gross.ToString().Contains(gp.searchString))
                            ).Select(a => new { a.EmpCode, a.EmpName, a.Amount, a.Gross }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Amount, a.Gross, sumYr }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpList;
                    Func<empdetails, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.ToString() :
                                         gp.sidx == "Amount" ? c.Amount.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.EmpCode, Convert.ToString(a.EmpName), a.Amount, a.Gross, sumYr }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.EmpCode, Convert.ToString(a.EmpName), a.Amount, a.Gross, sumYr }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpCode, Convert.ToString(a.EmpName), a.Amount, a.Gross, sumYr }).ToList();
                    }
                    totalRecords = EmpList.Count();
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
                    total = totalPages,
                    totalSum = sumYr
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static double tot;

        [HttpPost]
        public ActionResult editd2(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var v = "09/2017";
                //  var v = form["SalaryMonth"] == null ? "" : form["SalaryMonth"];
                var v1 = db.EmployeePayroll.Include(a => a.Employee.EmpName).Include(a => a.YearlyPaymentT).Include(a => a.OtherEarningDeductionT).ToList();
                var aa = db.SalEarnDedT.Include(b => b.SalaryHead).Include(b => b.SalaryHead.SalHeadOperationType).Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ITAX").FirstOrDefault();
                List<empdetails> p = new List<empdetails>();
                // double sum1 = 0;
                tot = 0;
                foreach (var item in v1)
                {

                    var d = item.YearlyPaymentT.Where(a => a.PayMonth == v).FirstOrDefault();
                    var d1 = item.OtherEarningDeductionT.Where(a => a.PayMonth == v).ToList();
                    foreach (var item1 in d1)
                    {
                        double amt = item1.SalAmount == null ? 0 : item1.SalAmount;
                        sum1 = sum1 + amt;
                    }
                    double d2 = d == null ? 0 : d.AmountPaid == null ? 0 : d.AmountPaid;
                    tot = tot + d2 + sum;
                    if (d != null || d1 != null)
                    {

                        p.Add(new empdetails
                        {
                            EmpCode = item.Employee.EmpCode,
                            EmpName = item.Employee.EmpName.FullNameFML,
                            Amount = d2 + sum,
                            Gross = aa.Amount

                        });


                    }

                }
                return Json(p, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LoadEmp1(P2BGrid_Parameters gp, FormCollection form, string param, string head_ids)
        {
            try
            {
                var Ohead_ids = Utility.StringIdsToListIds(head_ids);

                DateTime? dt = null;
                string monthyr = "";
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                double totalSum = 0;
                var jsonData = (Object)null;
                IEnumerable<empdetails> EmpList = null;
                List<empdetails> model = new List<empdetails>();
                empdetails view = null;
                string v = "";
                string Month = "";

                //if (gp.filter == "-")
                //{
                //    List<string> Msg = new List<string>();
                //    Msg.Add("Select From Date and To Date Both");
                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                //}

                if (param != null)
                {

                    var filter1 = param.Split('-')[0];
                    var filter2 = param.Split('-')[1];

                    DateTime startDate = DateTime.ParseExact(filter1, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime endDate = DateTime.ParseExact(filter2, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    DateTime temp = startDate;
                    DateTime dt3 = new DateTime();
                    List<string> result = new List<string>();
                    endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                    //while (temp <= endDate)
                    //{
                    //    // Console.WriteLine((string.Format("{0}/{1}", temp.Month, temp.Year)));
                    //    var mnt = (string.Format("{0}/{1}", temp.Month, temp.Year));
                    //    temp = temp.AddMonths(1);
                    //    result.Add(mnt);
                    //}

                    for (dt3 = startDate.AddMonths(1); dt3 <= endDate; dt3 = dt3.AddMonths(1))
                    {
                        var mnth = dt3.ToString("MM/yyyy");
                        result.Add(mnth);
                    }
                    if (startDate.ToString("MM/yyyy") == endDate.ToString("MM/yyyy"))
                    {
                        var mnth1 = startDate.ToString("MM/yyyy");
                        result.Add(mnth1);
                    }

                    //if (gp.filter != null)
                    //    v = gp.filter;
                    //else
                    //{
                    //    if (DateTime.Now.Date.Month < 10)
                    //        Month = "0" + DateTime.Now.Date.Month;
                    //    else
                    //        Month = DateTime.Now.Date.Month.ToString();
                    //    //  PayMonth = Month + "/" + DateTime.Now.Date.Year;
                    //}


                    var v1 = db.EmployeePayroll
                        .Include(a => a.Employee.EmpName)
                        .Include(a => a.YearlyPaymentT.Select(aa2 => aa2.SalaryHead))
                        .Include(a => a.OtherEarningDeductionT.Select(aa1 => aa1.SalaryHead)).AsNoTracking().AsParallel().ToList();
                    //var aa = db.SalEarnDedT.Include(b => b.SalaryHead).Include(b => b.SalaryHead.SalHeadOperationType).Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ITAX").FirstOrDefault();
                    tot = 0;
                    double etot = 0;
                    foreach (var paymon in result)
                    {
                        foreach (var emp in v1)
                        {
                            sum1 = 0;
                            //  var aa1 = db.SalEarnDedT.Include(b => b.SalaryHead).Include(b => b.SalaryHead.SalHeadOperationType).Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ITAX").FirstOrDefault();

                            //var v3 = emp.SalaryT.Where(a => a.PayMonth == paymon).FirstOrDefault();
                            double totgross = 0;
                            double Grsum = 0;
                            etot = 0;
                            //if (v3 != null)
                            //{
                            //  //  totgross = v3.TotalEarning + v3.TotalDeduction;
                            //    totgross = v3.TotalEarning ;
                            //}

                            foreach (var item in Ohead_ids)
                            {

                                double d = emp.YearlyPaymentT.Where(a => a.PayMonth == paymon && a.SalaryHead.Id == item && a.ITChallan == null).Sum(e => e.TDSAmount);
                                double amountpaid = emp.YearlyPaymentT.Where(a => a.PayMonth == paymon && a.SalaryHead.Id == item && a.ITChallan == null).Sum(e => e.AmountPaid);
                                var d1 = emp.OtherEarningDeductionT.Where(a => a.PayMonth == paymon && a.SalaryHead.Id == item && a.ITChallan == null).ToList();
                                //var d = emp.YearlyPaymentT.Where(a => a.PayMonth == paymon ).FirstOrDefault();
                                //var d1 = emp.OtherEarningDeductionT.Where(a => a.PayMonth == paymon && a.SalaryHead.InPayslip.ToString().ToUpper() == "TRUE").ToList();
                                foreach (var item1 in d1)
                                {
                                    double amt = item1.TDSAmount;
                                    sum1 = amt;
                                    double Gramt = item1.SalAmount;
                                    Grsum = Gramt;
                                }
                                //  double d2 = d == null ? 0 : d.TDSAmount == null ? 0 : d.TDSAmount;
                                //totgross = d == null ? 0 : d.AmountPaid == null ? 0 : d.AmountPaid;
                                Grsum = Grsum + amountpaid;
                                etot = etot + d + sum1;
                            }
                            //   var aa = db.SalEarnDedT.Include(b => b.SalaryHead).Include(b => b.SalaryHead.SalHeadOperationType).Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ITAX").FirstOrDefault();
                            if (etot != null)
                            {
                                //   model.Add(new empdetails
                                view = new empdetails()
                                {
                                    EmpCode = emp.Employee.EmpCode,
                                    EmpName = emp.Employee.EmpName.FullNameFML,
                                    Amount = etot,
                                    //Gross = aa.Amount
                                    Gross = Grsum

                                };
                                model.Add(view);
                            }
                            tot = tot + etot;
                        }

                    }
                }

                EmpList = model;

                IEnumerable<empdetails> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                            || (e.EmpName.ToString().Contains(gp.searchString))
                            || (e.Amount.ToString().Contains(gp.searchString))
                             || (e.Gross.ToString().Contains(gp.searchString))
                            ).Select(a => new { a.EmpCode, a.EmpName, a.Amount, a.Gross }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Amount, a.Gross, sum }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpList;
                    Func<empdetails, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.EmpCode.ToString() : "0");
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.ToString() :
                                         gp.sidx == "Amount" ? c.Amount.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, Convert.ToString(a.EmpName), a.Amount, a.Gross, sum }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, Convert.ToString(a.EmpName), a.Amount, a.Gross, sum }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, Convert.ToString(a.EmpName), a.Amount, a.Gross, sum }).ToList();
                    }
                    totalRecords = EmpList.Count();
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
                    total = totalPages,
                    totalSum = sum
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
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
                    ITChallan ITChallans = db.ITChallan.Where(e => e.Id == data).SingleOrDefault();

                    if (ITChallans != null)
                    {
                        db.ITaxTransT.Where(t => t.ITChallan.Id == ITChallans.Id).ToList().ForEach(s => s.ITChallan = null);
                        db.SaveChanges();

                        db.YearlyPaymentT.Where(t => t.ITChallan.Id == ITChallans.Id).ToList().ForEach(s => s.ITChallan = null);
                        db.SaveChanges();

                        db.OthEarningDeductionT.Where(t => t.ITChallan.Id == ITChallans.Id).ToList().ForEach(s => s.ITChallan = null);
                        db.SaveChanges();

                        IEnumerable<ITChallanEmpDetails> ITChallanEmpDetailsdata = db.ITChallanEmpDetails.Where(e => e.ChallanNo == ITChallans.ChallanNo).ToList();

                        db.ITChallanEmpDetails.RemoveRange(ITChallanEmpDetailsdata);
                        db.SaveChanges();
                    }
                    //ITChallan ITChallans = db.ITChallan.Where(e => e.Id == data).SingleOrDefault();
                    if (ITChallans.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, ITChallans.DBTrack, ITChallans, null, "ITChallan");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = ITChallans.DBTrack.CreatedBy != null ? ITChallans.DBTrack.CreatedBy : null,
                                CreatedOn = ITChallans.DBTrack.CreatedOn != null ? ITChallans.DBTrack.CreatedOn : null,
                                IsModified = ITChallans.DBTrack.IsModified == true ? true : false
                            };
                            //ITChallans.DBTrack = dbT;
                            //db.Entry(ITChallans).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ITChallans).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();
                            //  var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ITChallans.DBTrack);


                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", ITChallans, null, "ITChallan", ITChallans.DBTrack);
                            //  await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", ITChallans, null, "ITChallan", ITChallans.DBTrack );
                            //}
                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        //var selectedRegions = ITChallans.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {



                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = ITChallans.DBTrack.CreatedBy != null ? ITChallans.DBTrack.CreatedBy : null,
                                CreatedOn = ITChallans.DBTrack.CreatedOn != null ? ITChallans.DBTrack.CreatedOn : null,
                                IsModified = ITChallans.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            //   DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(ITChallans).State = System.Data.Entity.EntityState.Deleted;
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);


                            await db.SaveChangesAsync();


                            //using (var context = new DataBaseContext())
                            //{
                            //    ITChallans.Address = add;
                            //    ITChallans.ContactDetails = conDet;
                            //    ITChallans.BusinessType = val;
                            //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", ITChallans, null, "ITChallan", dbT);
                            //}
                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
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


        public class EditData
        {
            public int Id { get; set; }
            public Employee Employee { get; set; }
            public string PayMonth { get; set; }
            public double LWPDays { get; set; }
            public string Editable { get; set; }
            public double PaybleDays { get; set; }
            public double WeeklyOff_Cnt { get; set; }
        }



        public ActionResult P2BInlineGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EditData> SalAttendanceT = null;

                List<EditData> model = new List<EditData>();
                var view = new EditData();
                string PayMonth = "";
                string Month = "";
                if (gp.filter != null)
                    PayMonth = gp.filter;
                else
                {
                    if (DateTime.Now.Date.Month < 10)
                        Month = "0" + DateTime.Now.Date.Month;
                    else
                        Month = DateTime.Now.Date.Month.ToString();
                    PayMonth = Month + "/" + DateTime.Now.Date.Year;
                }
                var OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                    .Include(o => o.SalAttendance).ToList();



                foreach (var z in OEmployeePayroll)
                {
                    foreach (var S in z.SalAttendance)
                    {
                        if (S.PayMonth == PayMonth)
                        {

                            bool EditAppl = true;
                            view = new EditData()
                            {
                                Id = S.Id,
                                Employee = z.Employee != null ? z.Employee : null,
                                PaybleDays = S.PaybleDays,
                                PayMonth = S.PayMonth != null ? S.PayMonth : null,
                                Editable = EditAppl.ToString().ToLower()
                            };

                            model.Add(view);
                        }
                    }
                }

                SalAttendanceT = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = SalAttendanceT;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where((e => (e.Id.ToString().Contains(gp.searchString))
                           || (e.Employee.EmpCode.ToString().Contains(gp.searchString))
                           || (e.Employee.EmpName.FullNameFML.ToString().Contains(gp.searchString))
                           || (e.PayMonth.ToString().Contains(gp.searchString))
                           || (e.PaybleDays.ToString().Contains(gp.searchString))
                           || (e.Editable.ToString().Contains(gp.searchString))
                       )).Select(a => new Object[] { a.Id, a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.PaybleDays, a.Editable }).ToList();
                        //jsonData = IE.Where((e => (e.Contains(gp.searchString)))).Select(a => new Object[] { a.Id, a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.PaybleDays, a.Editable }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.PaybleDays, a.Editable }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = SalAttendanceT;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "PaybleDays" ? c.PaybleDays.ToString() :
                                         gp.sidx == "LWPDays" ? c.LWPDays.ToString() :
                                         gp.sidx == "PayMonth" ? c.PayMonth.ToString() :
                                         gp.sidx == "Editable" ? c.Editable.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.PaybleDays, a.Editable }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.PaybleDays, a.Editable }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.PaybleDays, a.Editable }).ToList();
                    }
                    totalRecords = SalAttendanceT.Count();
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
}