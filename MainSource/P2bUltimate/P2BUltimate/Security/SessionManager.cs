using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace P2BUltimate.Security
{
    public static class SessionManager
    {
        static string UserNameSessionVar = "username";
        static string CompanyIdVar = "CompId";
        static string EmpIdVar = "EmpId";
        static string FinancialYearVar = "FinancialYear";
        static string LeaveYearVar = "LeaveYear";
        static string CompPayrollIdVar = "CompPayrollId";
        static string CompTrainingIdVar = "CompTrainingId";
        static string EmpPayrollIdVar = "EmpPayrollId";
        static string CompLvVar = "CompLvVar";
        static string EmpLvVar = "EmpLvVar";
        static string CompRecruitIdVar = "CompRecruitId";
        static string CompAttIdVar = "CompAttId";
        static string CompApprIdVar = "CompApprId";
        static string CompExitIdVar = "CompExitId";
        public static String CheckProcessStatus = "";
        public static String CheckProcessStatusFunc
        {
            get
            {
                if (CheckProcessStatus == "")
                {
                    return "";
                }
                else
                {
                    return CheckProcessStatus + " Process is Ongoing, Can't make changes.";
                }
            }
            set
            {
                CheckProcessStatus = value;
            }
        }

        public static string UserName
        {
            get
            {
                if (HttpContext.Current.Session == null)
                {
                    return string.Empty;
                }
                var SessionVar = HttpContext.Current.Session[UserNameSessionVar];
                if (SessionVar != null)
                {
                    return SessionVar as string;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                //HttpContext.Current.Session["username"] = value;  vr
           HttpContext.Current.Session[UserNameSessionVar] = value;
 }
        }
        public static string CompanyId
        {
            get
            {
                if (HttpContext.Current.Session == null)
                {
                    return string.Empty;
                }
                var SessionVar = HttpContext.Current.Session[CompanyIdVar];
                if (SessionVar != null)
                {
                    return SessionVar as string;
                }
                else
                {
                    return null;
                }
            }
            set
            {
               // HttpContext.Current.Session["CompId"] = value;  vr
                 HttpContext.Current.Session[CompanyIdVar] = value;
            }
        }
        public static string EmpId
        {
            get
            {
                if (HttpContext.Current.Session == null)
                {
                    return string.Empty;
                }
                var SessionVar = HttpContext.Current.Session[EmpIdVar];
                if (SessionVar != null)
                {
                    return SessionVar as string;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session[EmpIdVar] = value;
            }
        }
        public static string FinancialYear
        {
            get
            {
                if (HttpContext.Current.Session == null)
                {
                    return string.Empty;
                }
                var SessionVar = HttpContext.Current.Session[FinancialYearVar];
                if (SessionVar != null)
                {
                    return SessionVar as string;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session[FinancialYearVar] = value;
            }
        }
        public static string LeaveYear
        {
            get
            {
                if (HttpContext.Current.Session == null)
                {
                    return string.Empty;
                }
                var SessionVar = HttpContext.Current.Session[LeaveYearVar];
                if (SessionVar != null)
                {
                    return SessionVar as string;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session[LeaveYearVar] = value;
            }
        }
        public static string CompPayrollId
        {
            get
            {
                if (HttpContext.Current.Session == null)
                {
                    return string.Empty;
                }
                var SessionVar = HttpContext.Current.Session[CompPayrollIdVar];
                if (SessionVar != null)
                {
                    return SessionVar as string;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session[CompPayrollIdVar] = value;
            }
        }

        public static string CompTrainingId
        {
            get
            {
                if (HttpContext.Current.Session == null)
                {
                    return string.Empty;
                }
                var SessionVar = HttpContext.Current.Session[CompTrainingIdVar];
                if (SessionVar != null)
                {
                    return SessionVar as string;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session[CompTrainingIdVar] = value;
            }
        }
        public static string EmpPayrollId
        {
            get
            {
                if (HttpContext.Current.Session == null)
                {
                    return string.Empty;
                }
                var SessionVar = HttpContext.Current.Session[EmpPayrollIdVar];
                if (SessionVar != null)
                {
                    return SessionVar as string;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session[EmpPayrollIdVar] = value;
            }
        }
        public static string EmpLvId
        {
            get
            {
                if (HttpContext.Current.Session == null)
                {
                    return string.Empty;
                }
                var SessionVar = HttpContext.Current.Session[EmpLvVar];
                if (SessionVar != null)
                {
                    return SessionVar as string;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session[EmpLvVar] = value;
            }
        }
        public static string CompLvId
        {
            get
            {
                if (HttpContext.Current.Session == null)
                {
                    return string.Empty;
                }
                var SessionVar = HttpContext.Current.Session[CompLvVar];
                if (SessionVar != null)
                {
                    return SessionVar as string;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session[CompLvVar] = value;
            }
        }

        public static string CompRecruitId
        {
            get
            {
                if (HttpContext.Current.Session == null)
                {
                    return string.Empty;
                }
                var SessionVar = HttpContext.Current.Session[CompRecruitIdVar];
                if (SessionVar != null)
                {
                    return SessionVar as string;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session[CompRecruitIdVar] = value;
            }
        }

        public static string CompAttId
        {
            get
            {
                if (HttpContext.Current.Session == null)
                {
                    return string.Empty;
                }
                var SessionVar = HttpContext.Current.Session[CompAttIdVar];
                if (SessionVar != null)
                {
                    return SessionVar as string;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session[CompAttIdVar] = value;
            }
        }

        public static string CompApprId
        {
            get
            {
                if (HttpContext.Current.Session == null)
                {
                    return string.Empty;
                }
                var SessionVar = HttpContext.Current.Session[CompApprIdVar];
                if (SessionVar != null)
                {
                    return SessionVar as string;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session[CompApprIdVar] = value;
            }
        }

        public static string CompExitId
        {
            get
            {
                if (HttpContext.Current.Session == null)
                {
                    return string.Empty;
                }
                var SessionVar = HttpContext.Current.Session[CompExitIdVar];
                if (SessionVar != null)
                {
                    return SessionVar as string;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                HttpContext.Current.Session[CompExitIdVar] = value;
            }
        }
    }
}