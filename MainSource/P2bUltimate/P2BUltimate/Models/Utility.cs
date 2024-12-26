using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using P2BUltimate.App_Start;
using P2b.Global;
using System.Data.Entity;
using P2BUltimate.Security;
using P2BUltimate.Models;
using System.ComponentModel.DataAnnotations;
using System.IO;
using NLog;
using System.Data.SqlClient;
namespace P2BUltimate.Models
{
    public enum LogType
    {
        INFO,
        DEBUG,
        TRACE,
        WARNING,
        ERROR,
        EXCEPTION,
        FATAL
    };

    [AuthoriseManger]
    public static class Utility
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public class returndataclass
        {
            public string code { get; set; }
            public string value { get; set; }
        }
        public class GridParaStructIdClass
        {
            public string GeoStruct { get; set; }
            public string PayStruct { get; set; }
            public string FunStruct { get; set; }
            public string Filter { get; set; }
            public string CheckAll { get; set; }
        }
        public class GetUserDataClass
        {
            public string EmpName { get; set; }
            public string EmpCode { get; set; }
            public string GeoStruct_Company_Name { get; set; }
            public string GeoStruct_Company_Code { get; set; }
            public string GeoStruct_Corporate_Code { get; set; }
            public string GeoStruct_Corporate_Name { get; set; }
            public string GeoStruct_Department_Code { get; set; }
            public string GeoStruct_Department_Name { get; set; }
            public string GeoStruct_Division_Code { get; set; }
            public string GeoStruct_Division_Name { get; set; }
            public string GeoStruct_Location_Code { get; set; }
            public string GeoStruct_Location_Name { get; set; }
            public string GeoStruct_Region_Code { get; set; }
            public string PayStruct_Grade_Code { get; set; }
            public string PayStruct_Grade_Name { get; set; }
            public string GeoStruct_Region_Name { get; set; }
            public string GeoStruct_Unit_Code { get; set; }
            public string GeoStruct_Unit_Name { get; set; }
            public string FuncStruct_Job_Code { get; set; }
            public string FuncStruct_Job_Name { get; set; }
            public string PayStruct_JobStatus { get; set; }
            public string FuncStruct_JobPosition_JobPositionCode { get; set; }
            public string FuncStruct_JobPosition_JobPositionDesc { get; set; }
            public string EmpOffInfo_AccountNo { get; set; }
            public string EmpOffInfo_NationalityID_PFNo { get; set; }
            public string EmpOffInfo_NationalityID_AdharNo { get; set; }
            public string EmpOffInfo_NationalityID_UANNo { get; set; }
            public string EmpOffInfo_NationalityID_PANNo { get; set; }

            public string ServiceBookDates_JoiningDate { get; set; }
            public string ServiceBookDates_ProbationDate { get; set; }
            public string ServiceBookDates_ConfirmationDate { get; set; }
            public string ServiceBookDates_RetirementDate { get; set; }
        }
        public static GetUserDataClass GetUserData()
        {
            /*
             * Get All User Data
             */
            using (DataBaseContext db = new DataBaseContext())
            {
                if (SessionManager.EmpId == "0" || SessionManager.EmpId == "")
                {
                    return null;
                }
                var id = Convert.ToInt32(SessionManager.EmpId);
                var oGetUserDataClass = new GetUserDataClass();

                var qurey = db.Employee
                    .Where(a => a.Id == id)
                .Select(a => new GetUserDataClass
                {
                    EmpName = a.EmpName != null ? a.EmpName.FName + " " + a.EmpName.LName : null,
                    EmpCode = a.EmpCode != null ? a.EmpCode : null,
                    GeoStruct_Company_Name = a.GeoStruct != null && a.GeoStruct.Company != null ? a.GeoStruct.Company.Name : null,
                    GeoStruct_Company_Code = a.GeoStruct != null && a.GeoStruct.Company != null ? a.GeoStruct.Company.Code : null,
                    GeoStruct_Corporate_Code = a.GeoStruct != null && a.GeoStruct.Corporate != null ? a.GeoStruct.Corporate.Code : null,
                    GeoStruct_Corporate_Name = a.GeoStruct != null && a.GeoStruct.Corporate != null ? a.GeoStruct.Corporate.Name : null,
                    GeoStruct_Department_Code = a.GeoStruct != null && a.GeoStruct.Department != null && a.GeoStruct.Department.DepartmentObj != null ? a.GeoStruct.Department.DepartmentObj.DeptCode : null,
                    GeoStruct_Department_Name = a.GeoStruct != null && a.GeoStruct.Department != null && a.GeoStruct.Department.DepartmentObj != null ? a.GeoStruct.Department.DepartmentObj.DeptDesc : null,
                    GeoStruct_Division_Code = a.GeoStruct != null && a.GeoStruct.Division != null ? a.GeoStruct.Division.Code : null,
                    GeoStruct_Division_Name = a.GeoStruct != null && a.GeoStruct.Division != null ? a.GeoStruct.Division.Name : null,
                    GeoStruct_Location_Code = a.GeoStruct != null && a.GeoStruct.Location != null && a.GeoStruct.Location.LocationObj != null ? a.GeoStruct.Location.LocationObj.LocCode : null,
                    GeoStruct_Location_Name = a.GeoStruct != null && a.GeoStruct.Location != null && a.GeoStruct.Location.LocationObj != null ? a.GeoStruct.Location.LocationObj.LocDesc : null,
                    GeoStruct_Region_Code = a.GeoStruct != null && a.GeoStruct.Region != null ? a.GeoStruct.Region.Code : null,
                    PayStruct_Grade_Code = a.PayStruct != null && a.PayStruct.Grade != null ? a.PayStruct.Grade.Code : null,
                    PayStruct_Grade_Name = a.PayStruct != null && a.PayStruct.Grade != null ? a.PayStruct.Grade.Name : null,
                    GeoStruct_Region_Name = a.GeoStruct != null && a.GeoStruct.Region != null ? a.GeoStruct.Region.Name : null,
                    GeoStruct_Unit_Code = a.GeoStruct != null && a.GeoStruct.Unit != null ? a.GeoStruct.Unit.Code : null,
                    GeoStruct_Unit_Name = a.GeoStruct != null && a.GeoStruct.Unit != null ? a.GeoStruct.Unit.Name : null,
                    FuncStruct_Job_Code = a.FuncStruct != null && a.FuncStruct.Job != null ? a.FuncStruct.Job.Code : null,
                    FuncStruct_Job_Name = a.FuncStruct != null && a.FuncStruct.Job != null ? a.FuncStruct.Job.Name : null,
                    PayStruct_JobStatus = a.PayStruct != null && a.PayStruct.JobStatus != null && a.PayStruct.JobStatus.EmpActingStatus != null ? a.PayStruct.JobStatus.EmpActingStatus.LookupVal : null,

                    FuncStruct_JobPosition_JobPositionCode = a.FuncStruct != null && a.FuncStruct.JobPosition != null ? a.FuncStruct.JobPosition.JobPositionCode : null,
                    FuncStruct_JobPosition_JobPositionDesc = a.FuncStruct != null && a.FuncStruct.JobPosition != null ? a.FuncStruct.JobPosition.JobPositionDesc : null,
                    EmpOffInfo_AccountNo = a.EmpOffInfo != null ? a.EmpOffInfo.AccountNo : null,
                    EmpOffInfo_NationalityID_PFNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.PFNo : null,
                    EmpOffInfo_NationalityID_AdharNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.AdharNo : null,
                    EmpOffInfo_NationalityID_UANNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.UANNo : null,
                    EmpOffInfo_NationalityID_PANNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.PANNo : null,
                }).SingleOrDefault();

                var servicebookdata = db.Employee
                    .Include(e => e.ServiceBookDates)
                    .Where(e => e.Id == id).ToList().Select(a => new GetUserDataClass
                    {
                        ServiceBookDates_JoiningDate = a.ServiceBookDates != null && a.ServiceBookDates.JoiningDate != null ? a.ServiceBookDates.JoiningDate.Value.ToString("dd/mm/yyyy") : null,
                        ServiceBookDates_ProbationDate = a.ServiceBookDates != null && a.ServiceBookDates.ProbationDate != null ? a.ServiceBookDates.ProbationDate.Value.ToShortDateString() : null,
                        ServiceBookDates_ConfirmationDate = a.ServiceBookDates != null && a.ServiceBookDates.ConfirmationDate != null ? a.ServiceBookDates.ConfirmationDate.Value.ToShortDateString() : null,
                        ServiceBookDates_RetirementDate = a.ServiceBookDates != null && a.ServiceBookDates.RetirementDate != null ? a.ServiceBookDates.RetirementDate.Value.ToShortDateString() : null,
                    }).SingleOrDefault();
                if (qurey != null)
                {
                    oGetUserDataClass.EmpName = qurey.EmpName;
                    oGetUserDataClass.EmpCode = qurey.EmpCode;
                    oGetUserDataClass.GeoStruct_Company_Name = qurey.GeoStruct_Company_Name;
                    oGetUserDataClass.GeoStruct_Company_Code = qurey.GeoStruct_Company_Code;
                    oGetUserDataClass.GeoStruct_Corporate_Code = qurey.GeoStruct_Corporate_Code;
                    oGetUserDataClass.GeoStruct_Corporate_Name = qurey.GeoStruct_Corporate_Name;
                    oGetUserDataClass.GeoStruct_Department_Code = qurey.GeoStruct_Department_Code;
                    oGetUserDataClass.GeoStruct_Department_Name = qurey.GeoStruct_Department_Name;
                    oGetUserDataClass.GeoStruct_Division_Code = qurey.GeoStruct_Division_Code;
                    oGetUserDataClass.GeoStruct_Division_Name = qurey.GeoStruct_Division_Name;
                    oGetUserDataClass.GeoStruct_Location_Code = qurey.GeoStruct_Location_Code;
                    oGetUserDataClass.GeoStruct_Location_Name = qurey.GeoStruct_Location_Name;
                    oGetUserDataClass.GeoStruct_Region_Code = qurey.GeoStruct_Region_Code;
                    oGetUserDataClass.PayStruct_Grade_Code = qurey.PayStruct_Grade_Code;
                    oGetUserDataClass.PayStruct_Grade_Name = qurey.PayStruct_Grade_Name;
                    oGetUserDataClass.GeoStruct_Region_Name = qurey.GeoStruct_Region_Name;
                    oGetUserDataClass.GeoStruct_Unit_Code = qurey.GeoStruct_Unit_Code;
                    oGetUserDataClass.GeoStruct_Unit_Name = qurey.GeoStruct_Unit_Name;
                    oGetUserDataClass.FuncStruct_Job_Code = qurey.FuncStruct_Job_Code;
                    oGetUserDataClass.FuncStruct_Job_Name = qurey.FuncStruct_Job_Name;
                    oGetUserDataClass.PayStruct_JobStatus = qurey.PayStruct_JobStatus;
                    oGetUserDataClass.FuncStruct_JobPosition_JobPositionCode = qurey.FuncStruct_JobPosition_JobPositionCode;
                    oGetUserDataClass.FuncStruct_JobPosition_JobPositionDesc = qurey.FuncStruct_JobPosition_JobPositionDesc;
                    oGetUserDataClass.EmpOffInfo_AccountNo = qurey.EmpOffInfo_AccountNo;
                    oGetUserDataClass.EmpOffInfo_NationalityID_PFNo = qurey.EmpOffInfo_NationalityID_PFNo;
                    oGetUserDataClass.EmpOffInfo_NationalityID_AdharNo = qurey.EmpOffInfo_NationalityID_AdharNo;
                    oGetUserDataClass.EmpOffInfo_NationalityID_UANNo = qurey.EmpOffInfo_NationalityID_UANNo;
                    oGetUserDataClass.EmpOffInfo_NationalityID_PANNo = qurey.EmpOffInfo_NationalityID_PANNo;
                }
                if (servicebookdata != null)
                {
                    oGetUserDataClass.ServiceBookDates_JoiningDate = servicebookdata.ServiceBookDates_JoiningDate;
                    oGetUserDataClass.ServiceBookDates_ProbationDate = servicebookdata.ServiceBookDates_ProbationDate;
                    oGetUserDataClass.ServiceBookDates_ConfirmationDate = servicebookdata.ServiceBookDates_ConfirmationDate;
                    oGetUserDataClass.ServiceBookDates_RetirementDate = servicebookdata.ServiceBookDates_RetirementDate;
                }
                return oGetUserDataClass;
            }
        }
        public static List<int> StringIdsToListIds(String form)
        {
            /*
             * @form=string "1,2";
             * returns list of int
             */

            String itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);

        }
        public static List<String> StringIdsToListString(String form)
        {
            /*
             * @form=string "1,2";
             * returns list of int
             */
            if (form.IndexOfAny(new char[] { ',' }) >= 0)
            {
                String itsec_id = form;
                List<String> ids = itsec_id.Split(',').Select(e => e).ToList();
                return (ids);
            }
            else
            {
                return new List<String>() { form };
            }
        }
        public class ReportClass
        {
            public ReportClass()
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var Comp = Convert.ToInt32(SessionManager.CompanyId);
                    ReportCompanyName = db.Company.Where(e => e.Id == Comp).Select(e => e.Name).SingleOrDefault();
                    var id = Convert.ToInt32(SessionManager.EmpId);
                    var empname = db.Employee.Include(e => e.EmpName).Where(e => e.EmpName != null && e.Id == id).Select(e => e.EmpName.FullNameFML).FirstOrDefault();
                    ReportCreatedBy = "Created by " + empname + " " + "Process Date : " + DateTime.Now.Date.ToString("dd/MMMM/yyyy");
                }
            }

            public string ReportCompanyName { get; set; }
            public string ReportName { get; set; }
            public string ReportCreatedBy { get; set; }
            public Int32? CorporateId { get; set; }
            public Int32? RegionId { get; set; }
            public Int32? CompanyId { get; set; }
            public Int32? DivisionId { get; set; }
            public Int32? LocationId { get; set; }
            public Int32? DepartmentId { get; set; }
            public Int32? GroupId { get; set; }
            public Int32? UnitId { get; set; }

            public string CorporateCode { get; set; }
            public string CorporateName { get; set; }
            public string RegionCode { get; set; }
            public string RegionName { get; set; }
            public string CompanyCode { get; set; }
            public string CompanyName { get; set; }
            public string DivisionCode { get; set; }
            public string DivisionName { get; set; }
            public string LocationCode { get; set; }
            public string LocationName { get; set; }
            public string GroupCode { get; set; }
            public string GroupName { get; set; }
            public string UnitCode { get; set; }
            public string UnitName { get; set; }


            public Int32? AddressId { get; set; }
            public Int32? ContactDetailsId { get; set; }
            public Int32? InchargeId { get; set; }

            public string FullAddress { get; set; }
            public string FullContactDetails { get; set; }
            public string InchargeFullDetails { get; set; }

            public DateTime? OpeningDate { get; set; }


            //Company
            public string RegistrationNo { get; set; }
            public DateTime? RegistrationDate { get; set; }
            public string PTECNO { get; set; }
            public string PTRCNO { get; set; }
            public string VATNo { get; set; }
            public string LBTNO { get; set; }
            public string ESICNo { get; set; }
            public string PANNo { get; set; }
            public string TANNo { get; set; }
            public string CSTNo { get; set; }
            public string ServiceTaxNo { get; set; }
            public string EstablishmentNo { get; set; }
            public string DivisionType { get; set; }
            public string BusinessCategory { get; set; }

            //GrandSummary
            public string PayMonth { get; set; }
            public double TotalEarning { get; set; }
            public double TotalDeduction { get; set; }
            public double TotalNet { get; set; }
            public string EarnCode { get; set; }
            public string EarnHead { get; set; }
            public double EarnAmount { get; set; }
            public string LookupVal { get; set; }
            public string EmpCode { get; set; }
            public string FullNameFML { get; set; }
            // public string LocCode { get; set; }
            //  public string LocDesc { get; set; }
            public Int32? TotalEmployee { get; set; }
            public double LocTotalEarning { get; set; }
            public double LocTotalDeduction { get; set; }
            public double LocTotalNet { get; set; }
            public Int32? LocTotalEmployee { get; set; }
            public double JobPosTotalEarning { get; set; }
            public double JobPosTotalDeduction { get; set; }
            public double JobPosTotalNet { get; set; }
            public Int32? JobPosTotalEmployee { get; set; }
            // public string Grade_Code { get; set; }
            // public string Grade_Name { get; set; }
            // public string Job_Code { get; set; }
            // public Int32? Company_Id { get; set; }
            // public string Job_Name { get; set; }
            // public string JobPosition_Id { get; set; }
            //  public string JobPositionCode { get; set; }
            // public string JobPositionDesc { get; set; }
            public double EPS_Share { get; set; }
            public double ER_Share { get; set; }

            //Bhagesh Added
            //Geostruct
            public Int32? Geo_Struct_Id { get; set; }
            public Int32? Corporate_Id { get; set; }
            public string Corporate_Code { get; set; }
            public string Corporate_Name { get; set; }
            public Int32? Region_Id { get; set; }
            public string Region_Code { get; set; }
            public string Region_Name { get; set; }
            public Int32? Division_Id { get; set; }
            public string Division_Code { get; set; }
            public string Division_Name { get; set; }
            public Int32? Company_Id { get; set; }
            public string Comp_Code { get; set; }
            public string Company_Name { get; set; }
            public Int32? Group_Id { get; set; }
            public string Group_Code { get; set; }
            public string Group_Name { get; set; }
            public Int32? Unit_Id { get; set; }
            public string Unit_Code { get; set; }
            public string Unit_Name { get; set; }
            public Int32? Department_Id { get; set; }
            public string DeptCode { get; set; }
            public string DeptDesc { get; set; }
            public Int32? Location_Id { get; set; }
            public string LocCode { get; set; }
            public string LocDesc { get; set; }
            public Int32? BusinessCategory_Id { get; set; }


            //Funcstruct
            public Int32? FuncStruct_Id { get; set; }
            public string Comp_Name { get; set; }
            public Int32? Job_Id { get; set; }
            public string Job_Code { get; set; }
            public string Job_Name { get; set; }
            //  public Int32? Job_Id { get; set; }
            public Int32? JobPosition_Id { get; set; }
            public string JobPositionCode { get; set; }
            public string JobPositionDesc { get; set; }

            //Paystruct
            public Int32? PayStruct_Id { get; set; }
            public Int32? Grade_Id { get; set; }
            public string Grade_Code { get; set; }
            public string Grade_Name { get; set; }
            public Int32? JobStatus_Id { get; set; }
            public Int32? Level_Id { get; set; }
            public string Level_Code { get; set; }
            public string Level_Name { get; set; }


            ////jvsummary
            public double JVtransactionamt { get; set; }
            public string CreditDebitFlag { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string ProcessMonth { get; set; }
            public string BatchName { get; set; }
            //public double EarnAmount { get; set; }
            public double Diff { get; set; }



        }
        public static List<KeyValuePair<string, string>> GetRemarkName()
        {
            List<KeyValuePair<string, string>> LookupStatus = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("**", "Wrong Entry"),
                    new KeyValuePair<string, string>("?I", "In time Missing"),
                    new KeyValuePair<string, string>("?O", "Out time Missing"),
                    new KeyValuePair<string, string>("HO", "Holiday WO"),
                    new KeyValuePair<string, string>("IL", "In Late"),
                     new KeyValuePair<string, string>("M", "Manual entry"),
                     new KeyValuePair<string, string>("OE", "Out Early"),
                     new KeyValuePair<string, string>("PP", "Present"),
                     new KeyValuePair<string, string>("UA", "Unauthorized Absent"),
                     new KeyValuePair<string, string>("WO", "Weekly Off IL"),
                };
            return LookupStatus;
        }
        public static List<string> ValidateObj(Object obj)
        {
            var errorList = new List<String>();
            var context = new ValidationContext(obj, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(obj, context, results, true);
            if (!isValid)
            {
                foreach (var validationResult in results)
                {
                    errorList.Add(validationResult.ErrorMessage);
                }
                return errorList;
            }
            else
            {
                return errorList;
            }
        }
        public class JsonReturnClass
        {
            public string Val;

            public bool success { get; set; }
            public dynamic responseText { get; set; }
            public String data { get; set; }
            public Object obj { get; set; }

            public int Id { get; set; }
        }
        public static String GetImage(String data)
        {
            String FolderName = "";
            String localPath = "";
            data = data.ToUpper();

            if (data == "COMPANY")
            {
                FolderName = "\\CompProFiles";
            }
            else if (data == "EMPLOYEE")
            {
                FolderName = "\\EmpProFiles\\profile";
            }
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\Images" + FolderName;
            bool exists = System.IO.Directory.Exists(requiredPath);
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data == "COMPANY")
                {
                    var id = Convert.ToInt32(SessionManager.CompanyId);
                    var Comp = db.Company.Where(e => e.Id == id).SingleOrDefault();
                    localPath = requiredPath + @"\" + Comp.Code + ".jpg";
                }
                if (data == "EMPLOYEE")
                {
                    var id = Convert.ToInt32(SessionManager.EmpId);
                    var Emp = db.Employee.Where(e => e.Id == id).SingleOrDefault();
                    var Employee = db.Employee
                           .Include(e => e.EmployeeDocuments)
                           .Include(e => e.EmployeeDocuments.Select(t => t.DocumentType))
                           .Where(e => e.EmpCode == Emp.EmpCode).SingleOrDefault();
                    var db_Emp_path = Employee.EmployeeDocuments.Where(z => z.DocumentType.LookupVal.ToUpper() == "PROFILE")
                        .ToList().OrderByDescending(z => z.Id).FirstOrDefault();
                    if (db_Emp_path != null)
                    {
                        localPath = db_Emp_path.DocumentPath;
                    }
                }
            }
            string newPath = "";
            if (data == "COMPANY")
            {
                newPath = new Uri(localPath).LocalPath;
            }
            else
            {
                newPath = localPath;
            }
            System.IO.FileInfo file = new System.IO.FileInfo(newPath);
            if (file.Exists)
            {
                byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                return base64ImageRepresentation;
            }
            else
            {
                return null;
            }
        }
        public static List<KeyValuePair<string, string>> GetWFName()
        {
            List<KeyValuePair<string, string>> LookupStatus = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("1", "Approved By HRM (M)"),
                    new KeyValuePair<string, string>("2", "LvEncashment Payment By HRM(M)"),
                    new KeyValuePair<string, string>("3", "Leave Credit Process By HRM(M)"),
                    new KeyValuePair<string, string>("4", "Leave Credit Process By HRM(M)"),
                };
            return LookupStatus;
        }
        public static List<KeyValuePair<string, string>> GetStatusName()
        {
            List<KeyValuePair<string, string>> LookupStatus = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("0", "Applied"),
                    new KeyValuePair<string, string>("1", "Sanctioned"),
                    new KeyValuePair<string, string>("2", "Sanction Rejected"),
                    new KeyValuePair<string, string>("3", "Approved"),
                    new KeyValuePair<string, string>("4", "Approved Rejected"),
                    new KeyValuePair<string, string>("5", "Approved By HRM (M)"),
                    new KeyValuePair<string, string>("6", "Cancel"),
                   
                };
            return LookupStatus;
        }
        public static List<DateTime> _Convert_PayMonth_To_DateTime(List<string> PayMonth)
        {
            List<DateTime> temp = new List<DateTime>();
            foreach (var item in PayMonth)
            {
                temp.Add(Convert.ToDateTime("01/" + item));
            }
            return temp;
        }
        public static List<string> _Convert_DateTime_To_PayMonth(List<DateTime> PayMonth)
        {
            List<string> temp = new List<string>();
            foreach (var item in PayMonth)
            {
                string Month = item.Month.ToString();
                if (item.Month < 10)
                {
                    Month = "0" + item.Month;
                }
                temp.Add(Month + "/" + item.Year);
            }
            return temp;
        }
        public static TimeSpan _returnTimeSpan(DateTime? oDateTime)
        {
            return new TimeSpan(oDateTime.Value.Hour, oDateTime.Value.Minute, 0);
        }
        public static void LogMessage(string message, Exception ex = null, LogType level = LogType.INFO)
        {
            switch (level)
            {
                case LogType.INFO: logger.Info(message);
                    break;
                case LogType.DEBUG: logger.Debug(message);
                    break;
                case LogType.WARNING: logger.Warn(message);
                    break;
                case LogType.TRACE: logger.Trace(message);
                    break;
                case LogType.ERROR: logger.Error(message);
                    break;
                case LogType.EXCEPTION: logger.Error(ex, message);
                    break;
                case LogType.FATAL: logger.Fatal(message);
                    break;
            }
        }
        public static void DumpProcessStatus(string Control = "*", Int32? LineNo = 00)
        {
            LogMessage(" => " + Control + " => " + LineNo, level: LogType.ERROR);

            //logger.Debug(DateTime.Now + " => " + Control + " => " + LineNo + Environment.NewLine);
            ////string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\LogFile";
            ////bool exists = System.IO.Directory.Exists(requiredPath);
            ////string localPath;
            ////if (!exists)
            ////{
            ////    localPath = new Uri(requiredPath).LocalPath;
            ////    System.IO.Directory.CreateDirectory(localPath);
            ////}
            ////string path = requiredPath + @"\ErrorLog_" + DateTime.Now.ToString("ddMMyyyy") + ".txt";
            ////localPath = new Uri(path).LocalPath;
            ////using (var fs = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            ////{
            ////    using (StreamWriter str = new StreamWriter(fs))
            ////    {
            ////        str.BaseStream.Seek(0, SeekOrigin.End);
            ////        str.WriteLine(DateTime.Now + " => " + Control + " => " + LineNo + Environment.NewLine);
            ////        str.Flush();
            ////        str.Close();
            ////        fs.Close();
            ////    }
            ////}
        }
        public static class CheckNetworkIssue
        {
            public static Int32? Networkissue { get; set; }
        }


        public static int DumpRequest(string Url, string sEmpCode)
        {
            //string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\LogFile";
            //bool exists = System.IO.Directory.Exists(requiredPath);
            //string localPath;
            //if (!exists)
            //{
            //    localPath = new Uri(requiredPath).LocalPath;
            //    System.IO.Directory.CreateDirectory(localPath);
            //}
            var EmpCode = "";
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Database.CommandTimeout = 15;
                try
                {
                    if (sEmpCode != "")
                    {
                        var id = Convert.ToInt32(sEmpCode);
                        EmpCode = db.Employee.Where(e => e.Id == id).Select(e => e.EmpCode).AsNoTracking().AsParallel().SingleOrDefault();
                    }
                    LogMessage(" => " + EmpCode + "=>" + Url, level: LogType.INFO);
                }
                catch (SqlException ex)
                {
                    if (ex.Number == -2 || ex.Number == -1)
                    {
                        CheckNetworkIssue.Networkissue = 1;
                        LogMessage("Timeout occurred " + Url, level: LogType.INFO);
                    }
                    return 0;
                }
            }
            //logger.Debug(DateTime.Now + " => " + EmpCode + "=>" + Url + Environment.NewLine);
            //string path = requiredPath + @"\RequestLog_" + DateTime.Now.ToString("ddMMyyyy") + ".txt";
            //localPath = new Uri(path).LocalPath;
            //using (var fs = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            //{
            //    using (StreamWriter str = new StreamWriter(fs))
            //    {
            //        str.BaseStream.Seek(0, SeekOrigin.End);
            //        str.WriteLine(DateTime.Now + " => " + EmpCode + "=>" + Url + Environment.NewLine);
            //        str.Flush();
            //        str.Close();
            //        fs.Close();
            //    }
            //}
            return 1;
        }
        public class diaryleaveholidaylist
        {
            public string Amount { get; set; }
            public string Reason { get; set; }
            public string Remark { get; set; }
            public string Date { get; set; }
        }
        public class GetOrganizationDataClass
        {
            public string GeoStruct_Corporate_Id { get; set; }
            public string GeoStruct_Corporate_Code { get; set; }
            public string GeoStruct_Corporate_Name { get; set; }
            public string GeoStruct_Region_Id { get; set; }
            public string GeoStruct_Region_Code { get; set; }
            public string GeoStruct_Region_Name { get; set; }
            public string GeoStruct_Company_Id { get; set; }
            public string GeoStruct_Company_Name { get; set; }
            public string GeoStruct_Company_Code { get; set; }
            public string GeoStruct_Division_Id { get; set; }
            public string GeoStruct_Division_Code { get; set; }
            public string GeoStruct_Division_Name { get; set; }
            public string GeoStruct_Location_Id { get; set; }
            public string GeoStruct_Location_Code { get; set; }
            public string GeoStruct_Location_Name { get; set; }
            public string GeoStruct_Department_Id { get; set; }
            public string GeoStruct_Department_Code { get; set; }
            public string GeoStruct_Department_Name { get; set; }
            public string GeoStruct_Unit_Id { get; set; }
            public string GeoStruct_Unit_Code { get; set; }
            public string GeoStruct_Unit_Name { get; set; }
            public string GeoStruct_Group_Id { get; set; }
            public string GeoStruct_Group_Code { get; set; }
            public string GeoStruct_Group_Name { get; set; }
            public string GeoStruct_Location_State_Id { get; set; }
            public string GeoStruct_Location_State_Name { get; set; }

            public string PayStruct_Grade_Id { get; set; }
            public string PayStruct_Grade_Code { get; set; }
            public string PayStruct_Grade_Name { get; set; }
            public string PayStruct_Level_Id { get; set; }
            public string PayStruct_Level_Code { get; set; }
            public string PayStruct_Level_Name { get; set; }
            public string PayStruct_JobStatus_Id { get; set; }
            public string PayStruct_JobStatus_EmpStatus { get; set; }
            public string PayStruct_JobStatus_EmpActingStatus { get; set; }

            public string FuncStruct_Job_Id { get; set; }
            public string FuncStruct_Job_Code { get; set; }
            public string FuncStruct_Job_Name { get; set; }
            public string FuncStruct_JobPosition_Id { get; set; }
            public string FuncStruct_JobPosition_Code { get; set; }
            public string FuncStruct_JobPosition_Name { get; set; }

        }

        public class GetEmployeePersDataClass
        {
            public string Employee_Id { get; set; }
            public string Employee_Code { get; set; }
            public string Employee_Name { get; set; }
            public string Employee_Gender { get; set; }
        }

        public class GetEmployeePersDataDetailsClass
        {
            public string Employee_Id { get; set; }
            public string Employee_Code { get; set; }
            public string Employee_Name { get; set; }
            public string Employee_Gender { get; set; }
        }

        public class GetSalaryDataClass
        {
            public string GeoStruct_Id { get; set; }
            public string Company_Code { get; set; }
            public string Company_Name { get; set; }
            public string LocCode { get; set; }
            public string LocDesc { get; set; }
            public string PayMonth { get; set; }
            public string EarnCode { get; set; }
            public string EarnHead { get; set; }
            public string SalHead_FullDetails { get; set; }


            public string LookupVal { get; set; }
            public string InPayslip { get; set; }
            public string StdAmount { get; set; }
            public string EarnAmount { get; set; }
            public string EmployeePayroll_Id { get; set; }
            public string Employee_Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName_Id { get; set; }
            public string EmpName { get; set; }
            public string TotalEarning { get; set; }
            public string TotalDeduction { get; set; }
            public string TotalNet { get; set; }
            public string SalHead_SeqNo { get; set; }
            public string PayStruct_Id { get; set; }
            public string Grade_Code { get; set; }
            public string Grade_Name { get; set; }
            public string Job_Code { get; set; }

            public string Job_Name { get; set; }
            public string SalaryHead_Id { get; set; }
            public string EPS_Share { get; set; }
            public string ER_Share { get; set; }
            public string JobPositionCode { get; set; }
            public string JobPositionDesc { get; set; }
            public string DeptCode { get; set; }
            public string DeptDesc { get; set; }
            public string Division_Code { get; set; }

            public string Division_Name { get; set; }
            public string ProcessDate { get; set; }
            public string ReleaseDate { get; set; }


        }


        public class GetJobPosBrSummaryClass
        {
            public string Geostruct_Id { get; set; }
            public string Company_Code { get; set; }
            public string Company_Name { get; set; }
            public string Location_Id { get; set; }
            public string LocCode { get; set; }
            public string LocDesc { get; set; }
            public string BusinessCategory { get; set; }
            public string FuncStruct_Id { get; set; }
            public string FunStruct_Details { get; set; }
            public string Job_Id { get; set; }
            public string Job_Code { get; set; }
            public string Job_Name { get; set; }
            public string JobPosition_Id { get; set; }
            public string JobPositionCode { get; set; }
            public string JobPositionDesc { get; set; }

            public string PayMonth { get; set; }
            public string SalHeadCode { get; set; }
            public string EarnHead { get; set; }
            public string SequenceNo { get; set; }
            public string SalHead_FullDetails { get; set; }


            public string LookupVal { get; set; }
            public string InPayslip { get; set; }
            public string StdAmount { get; set; }
            public string EarnAmount { get; set; }
            public string LocTotalEarning { get; set; }
            public string LocTotalDeduction { get; set; }
            public string LocTotalNet { get; set; }
            public string LocTotalEmployee { get; set; }
            public string JobPosTotalEarning { get; set; }
            public string JobPosTotalDeduction { get; set; }
            public string JobPosTotalNet { get; set; }
            public string JobPosTotalEmployee { get; set; }

            public string TotalPFBankContribution { get; set; }
            public string TotalPfPensionAmount { get; set; }
            public string TotalReliefFundBankContribution { get; set; }
            public string TotalSWFBankContribution { get; set; }
            public string TotalBankCONTRIBUTION { get; set; }

        }
    }
}