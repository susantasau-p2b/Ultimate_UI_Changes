using System;
using System.Collections.Generic;
using System.Linq;
using EssPortal.App_Start;
using System.Data.Entity;
using EssPortal.Security;
using System.IO;
namespace EssPortal.Models
{

    public static class Utility
    {
        private static DataBaseContext db = new DataBaseContext();

        public class returndataclass
        {
            public string code { get; set; }
            public string value { get; set; }
        }
        public class GridParaStructIdClass
        {
            public string GeoStruct { get; set; }
            public string PayStruct { get; set; }
            public string FuncStruct { get; set; }
            public string Filter { get; set; }
        }
        public class GetUserDataClass
        {
            public string EMPTITLE { get; set; }
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
            public string BloodGroup { get; set; }
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
                var qurey = db.Employee.Include(e=>e.EmpOffInfo)
                    .Include(e=>e.EmpOffInfo.NationalityID)
                    .Include(e=>e.EmpmedicalInfo)
                    .Where(a => a.Id == id)
                .Select(a => new GetUserDataClass
                {
                    EMPTITLE = a.EmpName.EmpTitle != null ? a.EmpName.EmpTitle.LookupVal : null,
                    EmpName = a.EmpName != null ? a.EmpName.FullNameFML : null,
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
                    PayStruct_JobStatus = a.PayStruct != null && a.PayStruct.JobStatus != null && a.PayStruct.JobStatus.FullDetails != null ? a.PayStruct.JobStatus.FullDetails : null,

                    FuncStruct_JobPosition_JobPositionCode = a.FuncStruct != null && a.FuncStruct.JobPosition != null ? a.FuncStruct.JobPosition.JobPositionCode : null,
                    FuncStruct_JobPosition_JobPositionDesc = a.FuncStruct != null && a.FuncStruct.JobPosition != null ? a.FuncStruct.JobPosition.JobPositionDesc : null,
                    EmpOffInfo_AccountNo = a.EmpOffInfo != null ? a.EmpOffInfo.AccountNo : null,
                    EmpOffInfo_NationalityID_PFNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.PFNo : null,
                    EmpOffInfo_NationalityID_AdharNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.AdharNo : null,
                    EmpOffInfo_NationalityID_UANNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.UANNo : null,
                    EmpOffInfo_NationalityID_PANNo = a.EmpOffInfo != null && a.EmpOffInfo.NationalityID != null ? a.EmpOffInfo.NationalityID.PANNo : null,
                    BloodGroup=a.EmpmedicalInfo!=null && a.EmpmedicalInfo.BloodGroup!=null?a.EmpmedicalInfo.BloodGroup.LookupVal:null,
                }).SingleOrDefault();

                var servicebookdata = db.Employee
                    .Include(e => e.ServiceBookDates)
                    .Where(e => e.Id == id).ToList().Select(a => new GetUserDataClass
                    {
                        ServiceBookDates_JoiningDate = a.ServiceBookDates != null && a.ServiceBookDates.JoiningDate != null ? a.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                        ServiceBookDates_ProbationDate = a.ServiceBookDates != null && a.ServiceBookDates.ProbationDate != null ? a.ServiceBookDates.ProbationDate.Value.ToShortDateString() : null,
                        ServiceBookDates_ConfirmationDate = a.ServiceBookDates != null && a.ServiceBookDates.ConfirmationDate != null ? a.ServiceBookDates.ConfirmationDate.Value.ToShortDateString() : null,
                        ServiceBookDates_RetirementDate = a.ServiceBookDates != null && a.ServiceBookDates.RetirementDate != null ? a.ServiceBookDates.RetirementDate.Value.ToShortDateString() : null,
                    }).SingleOrDefault();
                if (qurey != null)
                {
                    oGetUserDataClass.EmpName = qurey.EMPTITLE + " " + qurey.EmpName;
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
                    oGetUserDataClass.BloodGroup = qurey.BloodGroup;
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
            if (data == "COMPANY")
            {
                var id = Convert.ToInt32(SessionManager.CompanyId);
                var Comp = db.Company.Where(e => e.Id == id).SingleOrDefault();
                localPath = requiredPath + @"\" + Comp.Code + ".jpg";
            }
            if (data == "EMPLOYEE")
            {
                var id = Convert.ToInt32(SessionManager.EmpId);
                var Emp = db.Employee.
                    Include(e => e.EmployeeDocuments)
                    .Include(e => e.EmployeeDocuments.Select(t => t.DocumentType))
                    .Where(e => e.Id == id).SingleOrDefault();
                //var db_Emp_path = db.EmpDocument.Where(e => e.EmpCode == Emp.EmpCode && e.Type.LookupVal.ToUpper() == "PROFILE").ToList().OrderByDescending(e => e.Id).FirstOrDefault();
                var db_Emp_path = Emp.EmployeeDocuments.Where(e => e.DocumentType.LookupVal.ToUpper() == "PROFILE").ToList().OrderByDescending(e => e.Id).FirstOrDefault();
                if (db_Emp_path != null)
                {
                    localPath = db_Emp_path.DocumentPath;
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
        public class JsonClass
        {
            public bool status { get; set; }
            public String responseText { get; set; }
            public dynamic Data { get; set; }
        }
        public class JsonNewObjClass
        {
            public bool status { get; set; }
            public String responseText { get; set; }
            public dynamic Data1 { get; set; }
            public dynamic Data2 { get; set; }
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
                    new KeyValuePair<string, string>("7", "Recomendation"),
                    new KeyValuePair<string, string>("8", "Recomendation Rejected"),
                };
            return LookupStatus;
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
    }
}