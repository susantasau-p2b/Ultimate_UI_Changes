using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace EssPortal.Security
{
    public static class SessionManager
    {
        static string UserNameSessionVar = "username";
        static string CompanyIdVar = "CompId";
        static string EmpIdVar = "EmpId";
        static string FinancialYearVar = "FinancialYear";
        static string LeaveYearVar = "LeaveYear";
        static string CompPayrollIdVar = "CompPayrollId";
        static string EmpPayrollIdVar = "EmpPayrollId";
        static string CompLvVar = "CompLvVar";
        static string EmpLvVar = "EmpLvVar";

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
    }
}