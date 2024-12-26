using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using P2b.Global;
using System.Data.Entity;
using EssPortal.App_Start;
using EssPortal.Security;
using System.Net;
using EssPortal.Models;
using System.Transactions;
using EssPortal.Process;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Collections;
using System.Configuration;
using Newtonsoft.Json;
using System.Text;
using P2B.SAML;
using P2B.UTILS;
using System.Xml.Serialization;
using P2BUltimate.Models;
namespace EssPortal.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        Response xmlResponse;
        public ActionResult Index()
        {

            string readstr = "";
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            P2B.UTILS.P2BLogger logger = new P2B.UTILS.P2BLogger();
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\SAML" + ".ini";
            localPath = new Uri(path).LocalPath;
            if (!System.IO.File.Exists(localPath))
            {

                using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                {
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);

                    str.Flush();
                    str.Close();
                    fs.Close();
                }

            }

            else
            {
                using (var streamReader = new StreamReader(localPath))
                {
                    // ArrayList moduleArray = new ArrayList();
                    string line;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        readstr = line;
                    }

                }
            }



            if (readstr != "" && readstr != null)
            {

                var context = HttpContext.Request;
                string value = JsonConvert.SerializeObject(context.Params);
                logger.Logging(value);
              // var samlResponse = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4KPHNhbWwycDpSZXNwb25zZSBEZXN0aW5hdGlvbj0iaHR0cHM6Ly9ocm1zdWF0LmtlcmFsYWJhbmsuY28uaW4vSFJNU1VBVC9Mb2dpbi9pbmRleCIgSUQ9ImNPY19lNWk2Y1hzTjBhcUd5SnM4cEJ3aHJOVGgyNGVKSXd3R215SEpBZU00NSIgSW5SZXNwb25zZVRvPSIyQ1BoWG4yVTlkT2E4YlJzY1NQN3IyR1F4LTUxZlhKSUYtWDBJbFIyUTl3IiBJc3N1ZUluc3RhbnQ9IjIwMjMtMDgtMzFUMDY6NTc6MzEuNTA1WiIgVmVyc2lvbj0iMi4wIiB4bWxuczpzYW1sMnA9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjIuMDpwcm90b2NvbCI+PHNhbWwyOklzc3VlciB4bWxuczpzYW1sMj0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOmFzc2VydGlvbiI+ZW1JREFNRW50aXR5SWQwMTwvc2FtbDI6SXNzdWVyPjxzYW1sMnA6U3RhdHVzPjxzYW1sMnA6U3RhdHVzQ29kZSBWYWx1ZT0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOnN0YXR1czpTdWNjZXNzIi8+PHNhbWwycDpTdGF0dXNNZXNzYWdlPmFjY2Vzc0dyYW50ZWQ8L3NhbWwycDpTdGF0dXNNZXNzYWdlPjwvc2FtbDJwOlN0YXR1cz48c2FtbDI6QXNzZXJ0aW9uIElEPSJjT2NfZTVpNmNYc04wYXFHeUpzOHBCd2hyTlRoMjRlSkl3d0dteUhKQWVNIiBJc3N1ZUluc3RhbnQ9IjIwMjMtMDgtMzFUMDY6NTc6MzEuNTA1WiIgVmVyc2lvbj0iMi4wIiB4bWxuczpzYW1sMj0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOmFzc2VydGlvbiI+PHNhbWwyOklzc3Vlcj5lbUlEQU1FbnRpdHlJZDAxPC9zYW1sMjpJc3N1ZXI+PHNhbWwyOlN1YmplY3Q+PHNhbWwyOk5hbWVJRCBGb3JtYXQ9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjEuMTpuYW1laWQtZm9ybWF0OmVtYWlsQWRkcmVzcyI+c3VyZXNoPC9zYW1sMjpOYW1lSUQ+PHNhbWwyOlN1YmplY3RDb25maXJtYXRpb24gTWV0aG9kPSJ1cm46b2FzaXM6bmFtZXM6dGM6U0FNTDoyLjA6Y206YmVhcmVyIj48c2FtbDI6U3ViamVjdENvbmZpcm1hdGlvbkRhdGEgSW5SZXNwb25zZVRvPSIyQ1BoWG4yVTlkT2E4YlJzY1NQN3IyR1F4LTUxZlhKSUYtWDBJbFIyUTl3IiBOb3RPbk9yQWZ0ZXI9IjIwMjMtMDgtMzFUMDc6MTI6MzEuNTA1WiIgUmVjaXBpZW50PSJodHRwczovL2hybXN1YXQua2VyYWxhYmFuay5jby5pbi9IUk1TVUFUL0xvZ2luL2luZGV4Ii8+PC9zYW1sMjpTdWJqZWN0Q29uZmlybWF0aW9uPjwvc2FtbDI6U3ViamVjdD48c2FtbDI6Q29uZGl0aW9ucyBOb3RPbk9yQWZ0ZXI9IjIwMjMtMDgtMzFUMDc6MTI6MzEuNTA1WiI+PHNhbWwyOkF1ZGllbmNlUmVzdHJpY3Rpb24+PHNhbWwyOkF1ZGllbmNlPndpcHIwMTwvc2FtbDI6QXVkaWVuY2U+PC9zYW1sMjpBdWRpZW5jZVJlc3RyaWN0aW9uPjwvc2FtbDI6Q29uZGl0aW9ucz48c2FtbDI6QXV0aG5TdGF0ZW1lbnQgQXV0aG5JbnN0YW50PSIyMDIzLTA4LTMxVDA2OjU3OjMxLjUwNVoiIFNlc3Npb25JbmRleD0iYTFIVU1kOWRmeFFjdnM3TTk1N2ZQZGhodzdRR253c1JaaG8tNzZ5N3FSZyIgU2Vzc2lvbk5vdE9uT3JBZnRlcj0iMjAyMy0wOC0zMVQwNzoxMjozMS41MDVaIj48c2FtbDI6QXV0aG5Db250ZXh0PjxzYW1sMjpBdXRobkNvbnRleHRDbGFzc1JlZi8+PC9zYW1sMjpBdXRobkNvbnRleHQ+PC9zYW1sMjpBdXRoblN0YXRlbWVudD48c2FtbDI6QXR0cmlidXRlU3RhdGVtZW50PjxzYW1sMjpBdHRyaWJ1dGUgTmFtZT0iYXBwX25hbWUiIE5hbWVGb3JtYXQ9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjIuMDphdHRybmFtZS1mb3JtYXQ6YmFzaWMiPjxzYW1sMjpBdHRyaWJ1dGVWYWx1ZSB4bWxuczp4cz0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEiIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhzaTp0eXBlPSJ4czpzdHJpbmciPkhSTVM8L3NhbWwyOkF0dHJpYnV0ZVZhbHVlPjwvc2FtbDI6QXR0cmlidXRlPjxzYW1sMjpBdHRyaWJ1dGUgTmFtZT0iZW1haWwiIE5hbWVGb3JtYXQ9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjIuMDphdHRybmFtZS1mb3JtYXQ6YmFzaWMiPjxzYW1sMjpBdHRyaWJ1dGVWYWx1ZSB4bWxuczp4cz0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEiIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhzaTp0eXBlPSJ4czpzdHJpbmciPnN1cmVzaC5hZWx1Z3VydUB3aXByby5jb208L3NhbWwyOkF0dHJpYnV0ZVZhbHVlPjwvc2FtbDI6QXR0cmlidXRlPjxzYW1sMjpBdHRyaWJ1dGUgTmFtZT0iYWNjZXNzX21vZGUiIE5hbWVGb3JtYXQ9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjIuMDphdHRybmFtZS1mb3JtYXQ6YmFzaWMiPjxzYW1sMjpBdHRyaWJ1dGVWYWx1ZSB4bWxuczp4cz0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEiIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhzaTp0eXBlPSJ4czpzdHJpbmciPndyaXRlPC9zYW1sMjpBdHRyaWJ1dGVWYWx1ZT48L3NhbWwyOkF0dHJpYnV0ZT48c2FtbDI6QXR0cmlidXRlIE5hbWU9InVzZXJfbmFtZSIgTmFtZUZvcm1hdD0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOmF0dHJuYW1lLWZvcm1hdDpiYXNpYyI+PHNhbWwyOkF0dHJpYnV0ZVZhbHVlIHhtbG5zOnhzPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYSIgeG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeHNpOnR5cGU9InhzOnN0cmluZyI+c3VyZXNoPC9zYW1sMjpBdHRyaWJ1dGVWYWx1ZT48L3NhbWwyOkF0dHJpYnV0ZT48c2FtbDI6QXR0cmlidXRlIE5hbWU9Im1vYmlsZU51bWJlciIgTmFtZUZvcm1hdD0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOmF0dHJuYW1lLWZvcm1hdDpiYXNpYyI+PHNhbWwyOkF0dHJpYnV0ZVZhbHVlIHhtbG5zOnhzPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYSIgeG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeHNpOnR5cGU9InhzOnN0cmluZyI+OTU1MzI1MzY1Mzwvc2FtbDI6QXR0cmlidXRlVmFsdWU+PC9zYW1sMjpBdHRyaWJ1dGU+PHNhbWwyOkF0dHJpYnV0ZSBOYW1lPSJyb2xlIiBOYW1lRm9ybWF0PSJ1cm46b2FzaXM6bmFtZXM6dGM6U0FNTDoyLjA6YXR0cm5hbWUtZm9ybWF0OmJhc2ljIj48c2FtbDI6QXR0cmlidXRlVmFsdWUgeG1sbnM6eHM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hIiB4bWxuczp4c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIiB4c2k6dHlwZT0ieHM6c3RyaW5nIj5hZG1pbjwvc2FtbDI6QXR0cmlidXRlVmFsdWU+PC9zYW1sMjpBdHRyaWJ1dGU+PHNhbWwyOkF0dHJpYnV0ZSBOYW1lPSJmaXJzdE5hbWUiIE5hbWVGb3JtYXQ9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjIuMDphdHRybmFtZS1mb3JtYXQ6YmFzaWMiPjxzYW1sMjpBdHRyaWJ1dGVWYWx1ZSB4bWxuczp4cz0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEiIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhzaTp0eXBlPSJ4czpzdHJpbmciPnN1cmVzaDwvc2FtbDI6QXR0cmlidXRlVmFsdWU+PC9zYW1sMjpBdHRyaWJ1dGU+PHNhbWwyOkF0dHJpYnV0ZSBOYW1lPSJsYXN0TmFtZSIgTmFtZUZvcm1hdD0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOmF0dHJuYW1lLWZvcm1hdDpiYXNpYyI+PHNhbWwyOkF0dHJpYnV0ZVZhbHVlIHhtbG5zOnhzPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYSIgeG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeHNpOnR5cGU9InhzOnN0cmluZyI+YWVsdWd1cnU8L3NhbWwyOkF0dHJpYnV0ZVZhbHVlPjwvc2FtbDI6QXR0cmlidXRlPjxzYW1sMjpBdHRyaWJ1dGUgTmFtZT0iRW1wbG95ZWVJRCIgTmFtZUZvcm1hdD0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOmF0dHJuYW1lLWZvcm1hdDpiYXNpYyI+PHNhbWwyOkF0dHJpYnV0ZVZhbHVlIHhtbG5zOnhzPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYSIgeG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeHNpOnR5cGU9InhzOnN0cmluZyI+MjAzMzExMTE8L3NhbWwyOkF0dHJpYnV0ZVZhbHVlPjwvc2FtbDI6QXR0cmlidXRlPjwvc2FtbDI6QXR0cmlidXRlU3RhdGVtZW50Pjwvc2FtbDI6QXNzZXJ0aW9uPjwvc2FtbDJwOlJlc3BvbnNlPg==";//context.Params.GetValues("SAMLResponse")[0];

                var samlResponse = context.Params.GetValues("SAMLResponse");

               
                if (samlResponse == null)
                {
                    P2B.UTILS.P2BLogger p2BLogger = new P2B.UTILS.P2BLogger();
                    try
                    {
                        string assertion = ConfigurationManager.AppSettings["hrmsURL"];
                        p2BLogger.Logging("assertion " + assertion);
                        string destination = ConfigurationManager.AppSettings["destination"];
                        p2BLogger.Logging("destination " + destination);
                        string entity = ConfigurationManager.AppSettings["entity"];
                        p2BLogger.Logging("entity " + entity);
                        string client = ConfigurationManager.AppSettings["client"];
                        p2BLogger.Logging("client " + client);
                        IP2BAuthenticationReq p2BAuthenticationReq = new P2BAuthenticationReq(assertion, destination,
                                Utils.GetGuid().ToString(), "false", "false");
                        var request = p2BAuthenticationReq.GetRedirectURL(destination, entity, P2B.SAML.Helper.P2BRequestFormatType.EmailFormat);
                        p2BLogger.Logging("Without URL SAFE " + request);
                        var response = HttpContext.Response;
                        response.Clear();
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.AppendFormat("<form name='form' action='{0}' method='post'>", destination);
                        stringBuilder.AppendFormat("<input type='hidden' name='SAMLRequest' value='{0}' />", request);
                        stringBuilder.AppendFormat("<input type='hidden' name='clientID' value='{0}' />", client);
                        stringBuilder.Append("</form>");
                        stringBuilder.Append("<script type='text/javascript'>");
                        stringBuilder.Append("document.forms[0].submit();");
                        stringBuilder.Append("</script>");
                        response.Write(stringBuilder.ToString());
                        response.End();
                        return null;
                    }
                    catch (Exception ex)
                    {
                        p2BLogger.Logging("Exception " + ex);
                        return Content("Exception :  " + ex.InnerException + " Exception Message :  " + ex.Message, "text/html; charset=utf-8");
                    }
                }

                //var samlResponse = string.Empty;
                logger.Logging(samlResponse);

                byte[] responseBytes = Convert.FromBase64String(samlResponse[0]);
                string fromBytesToString = Encoding.UTF8.GetString(responseBytes);
                logger.Logging(fromBytesToString);
                //LOGFController(fromytesToString)
                xmlResponse = GetResponse(fromBytesToString);


                return Check();

            }



            if (!String.IsNullOrEmpty(SessionManager.UserName) && String.IsNullOrEmpty(readstr))
            {
                return RedirectToAction("index", "Login");
            }
            return View();
        }


        public Response GetResponse(string response)
        {
            Response responseObject;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Response));
            if (response != null && response != "")
            {
                using (StringReader reader = new StringReader(response))
                {
                    responseObject = (Response)xmlSerializer.Deserialize(reader);

                }
                return responseObject;
            }
            else
            {
                return null;
            }


        }
        public class CORE_LoginPolicy
        {

            public int? InfoNo { get; set; }
            public string ErrMsg { get; set; }
            public int Employee_Id { get; set; }
            public bool IsLoginSecurityAppl { get; set; }
            public string GUID { get; set; }
            public bool LoginSecurityQuestionAppl { get; set; }
            public int? ErrNo { get; set; }
            public bool DefaultPasswordChangeInit { get; set; }
            public bool IsPasswordSecurityAppl { get; set; }
            public bool PasswordOTPAppl { get; set; }
            public bool PasswordRandomNoGenAppl { get; set; }
            public bool LoginRandomNoGenAppl { get; set; }
            public bool AttendGeoFencingAppl { get; set; }
            public int ResendOTPCount { get; set; }
            public bool LoginOTPAppl { get; set; }
            public int OTPTimer { get; set; }
            public int LogInAttempt { get; set; }
            public bool Lock { get; set; }
            public string ApplicationUI { get; set; }
            public int LoginPolicy_Id { get; set; }
            public string ProcessType { get; set; }
            public string UserId { get; set; }
            public string Password { get; set; }
            public string MobileNumber { get; set; }
            public bool Suspend { get; set; }
            public string OriginalCaptcha { get; set; }
            public string SecurityQ { get; set; }
            public string SecurityAns { get; set; }
            public string OTPRecd { get; set; }
            public string OTPSend { get; set; }
            public string OldPassword { get; set; }
            public string MaxNoBadLogin { get; set; }
            public string InputCaptcha { get; set; }
            public bool forgotPassword { get; set; }
        }

        public ActionResult Check(String x = null, String y = null)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    #region Normal Login
                    if (!String.IsNullOrEmpty(x) && !String.IsNullOrEmpty(y))
                    {

                        #region If User IsAdmin
                        if (x == "admin")
                        {
                            //admin login scope

                            #region If Database IsExists
                            if (db.Database.Exists())
                            {
                                db.Database.Connection.Open();
                                PasswordPolicy login_policy = new PasswordPolicy();
                                if (db.Login.Any(e => e.UserId == "admin"))
                                {
                                    var LoginUser = db.Employee.Include(e => e.Login).Where(e => e.Login.UserId.Equals(x.Trim())).FirstOrDefault();

                                    login_policy = db.PasswordPolicy.Include(e => e.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ESS").FirstOrDefault();

                                    if (login_policy == null)
                                    {
                                        return Json(new { success = false, data = "Contact to Administrator to Define ESS Login Policy..!" }, JsonRequestBehavior.AllowGet);
                                    }


                                    string AdminLoginPassword = login_policy.AllowEncryption == true ? P2BSecurity.Decrypt(LoginUser.Login.Password) : LoginUser.Login.Password;

                                    if (LoginUser != null && LoginUser.Login != null && y != AdminLoginPassword)
                                    {
                                        return Json(new { success = false, data = "UserId or PassWord Wrong..!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    if (LoginUser != null)
                                    {
                                        SessionManager.UserName = SessionManager.EmpId = LoginUser.Id.ToString();
                                        SessionManager.EmpLvId = db.EmployeeLeave.Where(e => e.Employee != null && e.Employee.Id == LoginUser.Id).Select(e => e.Id).SingleOrDefault().ToString();
                                        var id = Convert.ToInt32(SessionManager.UserName);
                                        var EmpPayrollData = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == id).SingleOrDefault();
                                        var Comp = db.CompanyPayroll.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll.Any(a => a.Id == EmpPayrollData.Id)).Select(e => e.Company).ToList();
                                        if (Comp != null && Comp.Count > 0)
                                        {
                                            Session["CompId"] = SessionManager.CompanyId = Comp.Select(e => e.Id.ToString()).FirstOrDefault();
                                        }
                                    }
                                    else
                                    {
                                        SessionManager.UserName = "0";
                                    }

                                    if (db.Company.Count() == 1)
                                    {
                                        //employee is not assign but comp create
                                        Session["CompId"] = SessionManager.CompanyId = db.Company.Select(e => e.Id.ToString()).FirstOrDefault();
                                    }
                                    if (!string.IsNullOrEmpty(SessionManager.CompanyId) && SessionManager.CompanyId != "0")
                                    {
                                        var id = Convert.ToInt32(Session["CompId"].ToString());
                                        var fyear = db.Company.Include(e => e.Calendar).Include(e => e.Calendar.Select(a => a.Name)).Where(e => e.Id == id).SingleOrDefault();
                                        if (fyear != null)
                                        {
                                            SessionManager.FinancialYear = fyear.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).Select(e => e.Id).SingleOrDefault().ToString();
                                        }
                                        SessionManager.CompPayrollId = db.CompanyPayroll.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).SingleOrDefault();
                                        SessionManager.CompLvId = db.CompanyLeave.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).SingleOrDefault();
                                    }
                                    var CompId = int.Parse(SessionManager.CompanyId);
                                    //var EmpAll = db.CompanyPayroll
                                    //    .Include(e => e.Company)
                                    //    .Include(e => e.EmployeePayroll)
                                    //    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                                    //    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                                    //    .Include(e => e.EmployeePayroll.Select(a => a.Employee.Login))
                                    //    .Where(e => e.Company.Id == CompId)
                                    //    .FirstOrDefault();
                                    /*
                                     * insert record in login table
                                     */
                                    // ** New Employee Password will be create from Payroll Software
                                    ////var Emps = EmpAll.EmployeePayroll.Where(e => e.Employee.Login == null && e.Employee.ServiceBookDates != null && e.Employee.ServiceBookDates.ServiceLastDate == null).Select(e => new
                                    ////{
                                    ////    _Id = e.Employee.Id,
                                    ////    _EmpCode = e.Employee.EmpCode,
                                    ////    _EmpPwd = e.Employee.EmpCode
                                    ////}).ToList();

                                    


                                    ////foreach (var item in Emps)
                                    ////{
                                    ////    // For Encryption and Decryption of User Password
                                    ////    string EncryptedPassword = "";
                                    ////    if (item._Id != null)
                                    ////    {
                                    ////        EncryptedPassword = login_policy.AllowEncryption == true ? P2BSecurity.Encrypt(item._EmpPwd) : item._EmpPwd;

                                    ////    }

                                    ////    //if (item.Login == null)
                                    ////    //{
                                    ////    var newLogin = new Login();
                                    ////    newLogin.UserId = item._EmpCode;
                                    ////    newLogin.Password = EncryptedPassword;
                                    ////    newLogin.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    ////    newLogin.IsUltimateAppl = 1;
                                    ////    newLogin.IsActive = true;
                                    ////    db.Login.Add(newLogin);
                                    ////    db.SaveChanges();

                                    ////    var emp = db.Employee.Include(e => e.Login).Where(e => e.Id == item._Id).SingleOrDefault();
                                    ////    emp.Login = newLogin;

                                    ////    db.Entry(emp).State = System.Data.Entity.EntityState.Modified;
                                    ////    db.SaveChanges();
                                    ////    //}
                                    ////}
                                    return Json(new { success = true, data = Url.Action("index", "Home") }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            #endregion If Database IsExists
                            else
                            {
                                try
                                {
                                    return Json(new { success = false, data = "DataBase Not Exists..!" }, JsonRequestBehavior.AllowGet);
                                }
                                catch (Exception e)
                                {
                                    return Json(new { success = false, data = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        #endregion If User IsAdmin
                        //User Exitance check
                    //    if (!db.Login.Any(e => e.UserId == x.Trim()))
                        
                        //if (db.Login.Where(e => e.UserId == x.Trim()).AsNoTracking().FirstOrDefault()==null)
                        //{
                        //    return Json(new { success = false, data = "User Not Exists..!" }, JsonRequestBehavior.AllowGet);
                        //}


                        //GetEmpData
                        PasswordPolicy PassLogin_Policy = db.PasswordPolicy.Include(e => e.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ESS").FirstOrDefault();
                        var EmpData = db.Employee.Include(e => e.Login).Include(e => e.LogRegister).Where(e => e.Login != null && e.Login.UserId == x.Trim()).SingleOrDefault();
                        if (EmpData == null)
                        {
                            return Json(new { success = false, data = "Invalid Account..!" }, JsonRequestBehavior.AllowGet);
                        }
                        var Retiredemp = db.Employee.Include(e => e.Login).Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.ServiceLastDate != null && e.Login.UserId == x.Trim()).SingleOrDefault();
                        if (Retiredemp != null)
                        {
                            return Json(new { success = false, data = "Retired Employee Cannot Login....!" }, JsonRequestBehavior.AllowGet);
                        }

                        var CheckRetired = db.Employee.Include(e => e.Login).Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.RetirementDate != null && e.Login.UserId == x.Trim() && e.ServiceBookDates.RetirementDate < DateTime.Today).SingleOrDefault();
                        if (CheckRetired != null)
                        {
                            return Json(new { success = false, data = "Retired Employee Cannot Login....!" }, JsonRequestBehavior.AllowGet);
                        }

                        //Check Ultimate access
                        if (EmpData.Login.IsESSAppl == false)
                        {
                            return Json(new { success = false, data = "You Don't Have Permission To Access..!" }, JsonRequestBehavior.AllowGet);
                        }

                        LogRegister log_reg = new LogRegister();
                        log_reg.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        var log = EmpData.LogRegister.LastOrDefault();
                        if (EmpData != null && EmpData.Login.IsActive == false)
                        {
                            return Json(new { success = false, data = "Account is Locked..!" }, JsonRequestBehavior.AllowGet);
                        }
                        int login_hits = log == null ? 1 : log.LogInAttempt;

                        if (EmpData != null)
                        {
                            if (EmpData != null)
                            {
                                SessionManager.UserName = SessionManager.EmpId = EmpData.Id.ToString();
                                SessionManager.EmpLvId = db.EmployeeLeave.Where(e => e.Employee.Id == EmpData.Id).Select(e => e.Id).SingleOrDefault().ToString();
                                var id = Convert.ToInt32(SessionManager.UserName);
                                var EmpPayrollData = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == id).SingleOrDefault();
                                var Comp = db.CompanyPayroll.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll.Any(a => a.Id == EmpPayrollData.Id)).Select(e => e.Company).ToList();
                                if (Comp != null && Comp.Count > 0)
                                {
                                    Session["CompId"] = SessionManager.CompanyId = Comp.Select(e => e.Id.ToString()).FirstOrDefault();
                                }
                            }
                            else
                            {
                                SessionManager.UserName = EmpData.Login.UserId;
                            }
                            if (!string.IsNullOrEmpty(SessionManager.CompanyId) && SessionManager.CompanyId != "0")
                            {
                                var id = Convert.ToInt32(Session["CompId"].ToString());
                                var fif = db.Company.Include(e => e.Calendar).Include(e => e.Calendar.Select(a => a.Name)).Where(e => e.Id == id).SingleOrDefault();
                                if (fif != null)
                                {
                                    SessionManager.FinancialYear = fif.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).Select(e => e.Id).SingleOrDefault().ToString();
                                }
                                SessionManager.CompPayrollId = db.CompanyPayroll.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).SingleOrDefault();
                                SessionManager.CompLvId = db.CompanyLeave.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).SingleOrDefault();
                            }
                            if (!string.IsNullOrEmpty(SessionManager.CompanyId) && !string.IsNullOrEmpty(SessionManager.EmpId))
                            {
                                /*
                                 * Reporting Struct Scope
                                 */
                                List<ReportingStructRights> oReportingStructRights = PortalRights.ScanRights(0, Convert.ToInt32(SessionManager.EmpId), Convert.ToInt32(SessionManager.CompanyId));
                            }

                            // Decryption of Password

                            EssPortal.Controllers.ELMSController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy> responseDeserializeData = new EssPortal.Controllers.ELMSController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy>();
                            int? errorno = 0;
                            int? Infono = 0;
                            var ShowMessage = "";

                            string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                            using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                            {
                                var response = p2BHttpClient.request("Global/getLoginPolicy",
                                    new CORE_LoginPolicy()
                                    {
                                        ApplicationUI = PassLogin_Policy.ApplicationUI.LookupVal.ToUpper(),
                                        LoginPolicy_Id = PassLogin_Policy.Id,
                                        ProcessType = "LOGINCHECK",
                                        UserId = EmpData.EmpCode.ToString(),
                                        Password = y,
                                        Employee_Id = EmpData.Id,
                                        GUID = EmpData.Login.Guid
                                    });

                                responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<EssPortal.Controllers.ELMSController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy>>(response.Content.ReadAsStringAsync().Result);
                                if (responseDeserializeData.Data != null)
                                {
                                    errorno = responseDeserializeData.Data.ErrNo;
                                    ShowMessage = responseDeserializeData.Data.ErrMsg;
                                    Infono = responseDeserializeData.Data.InfoNo;
                                }
                                else
                                {
                                    errorno = 1;
                                    ShowMessage = responseDeserializeData.Message.ToString();
                                }

                                if (errorno != 0)
                                {
                                    return Json(new { success = false, data = ShowMessage }, JsonRequestBehavior.AllowGet);
                                }

                                if (errorno == 0 && (Infono == 0 || Infono == null))
                                {
                                    return Json(new { success = true, mod = 1, data = Url.Action("index", "home") }, JsonRequestBehavior.AllowGet);
                                }

                                if (Infono != 0)
                                {

                                    if (Infono == 2 && ShowMessage == "Change Default Password")
                                    {
                                        return Json(new { success = true, mod = 0, data = Url.Action("Index", "welcomescreen") }, JsonRequestBehavior.AllowGet);
                                    }
                                    if (Infono == 1 && ShowMessage == "Security Question and OTP Appl")
                                    {
                                        TempData["Skey"] = Infono;
                                        return Json(new { success = true, mod = 0, data = Url.Action("Index", "SecurityQOTP") }, JsonRequestBehavior.AllowGet);
                                    }
                                    if (Infono == 2 && ShowMessage == "Security Question Appl")
                                    {
                                        TempData["Skey"] = Infono;
                                        return Json(new { success = true, mod = 0, data = Url.Action("Index", "SecurityQOTP") }, JsonRequestBehavior.AllowGet);
                                    }
                                    if (Infono == 3 && ShowMessage == "Security OTP Appl")
                                    {
                                        TempData["Skey"] = Infono;
                                        return Json(new { success = true, mod = 0, data = Url.Action("Index", "SecurityQOTP") }, JsonRequestBehavior.AllowGet);
                                    }
                                    return Json(new { success = false, data = ShowMessage }, JsonRequestBehavior.AllowGet);
                                }

                              //  return Json(new { success = false, data = "User Name Not Exists..!" }, JsonRequestBehavior.AllowGet);

                            }
                            return Json(new { success = false, data = "User Name Not Exists..!" }, JsonRequestBehavior.AllowGet);
                            // In API CHECK
                            //string DecryptionPassword = "";
                            //if (!String.IsNullOrEmpty(y))
                            //{
                            //    DecryptionPassword = PassLogin_Policy.AllowEncryption == true ? P2BSecurity.Decrypt(EmpData.Login.Password) : EmpData.Login.Password;
                            //}
                            // In API CHECK

                            //if (EmpData.Login.Password == y.Trim())
                            // In API CHECK
                            //if (DecryptionPassword == y.Trim())
                            //{
                            //    try
                            //    {
                            //        if (db.PasswordPolicy.ToList().Count() == 0)
                            //        {
                            //            PasswordPolicy _passwordpolicy = new PasswordPolicy();
                            //            _passwordpolicy.MinPwdLength = 8;
                            //            _passwordpolicy.OldPwdAllow = false;
                            //            _passwordpolicy.PasswordSecurityQuestion = true;
                            //            _passwordpolicy.LoginSecurityQuestionAppl = true;
                            //            _passwordpolicy.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            //            _passwordpolicy.AllowAttempt = 3;
                            //            db.PasswordPolicy.Add(_passwordpolicy);
                            //            db.SaveChanges();
                            //        }
                            //        using (TransactionScope ts = new TransactionScope())
                            //        {
                            //            log_reg.LogInAttempt = 1;
                            //            log_reg.LogInDate = DateTime.Now;
                            //            log_reg.LogInTime = DateTime.Now.TimeOfDay.ToString();
                            //            log_reg.LogInDate = DateTime.Now;
                            //            log_reg.LogInTime = DateTime.Now.TimeOfDay.ToString();
                            //            log_reg.IP = Dns.GetHostByName(Dns.GetHostName().ToString()).AddressList[0].ToString();
                            //            log_reg.ComputerName = Dns.GetHostName().ToString();
                            //            log_reg.Browser = HttpContext.Request.Browser.Browser;
                            //            EmpData.LogRegister.Add(log_reg);
                            //            //  db.LogRegister.Add(log_reg);

                            //            db.SaveChanges();
                            //            ts.Complete();
                            //        }
                            //    }

                            //    catch (DbUpdateConcurrencyException ex)
                            //    {
                            //        ex.Entries.Single().Reload();

                            //        db.SaveChanges();

                            //    }
                            //    catch (Exception e)
                            //    {
                            //        return Json(new { success = false, data = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);

                            //    }
                            //    //if username and password are same consider the user is 1st loged in ,and force to change password
                            //    if (EmpData.Login.UserId == EmpData.Login.Password)
                            //    {
                            //        return Json(new { success = true, data = Url.Action("Index", "welcomescreen") }, JsonRequestBehavior.AllowGet);
                            //    }
                            //    else
                            //    {
                            //        // PortalRights.ScanRights();
                            //        return Json(new { success = true, data = Url.Action("index", "home") }, JsonRequestBehavior.AllowGet);
                            //    }
                            //}
                            //else
                            //{
                            //    var CompId = Convert.ToInt32(SessionManager.CompanyId);
                            //    var login_policy = db.Company.Include(e => e.PasswordPolicy).Where(e => e.Id == CompId && e.PasswordPolicy != null).OrderByDescending(e => e.Id).Select(e => e.PasswordPolicy).SingleOrDefault();
                            //    //if (login_policy != null && login_policy.AllowAttempt < login_hits)
                            //    //{
                            //        if (login_policy != null)
                            //    {
                            //        log_reg.LogInAttempt = login_hits;
                            //        log_reg.LogInDate = DateTime.Now;
                            //        log_reg.LogInTime = DateTime.Now.TimeOfDay.ToString();
                            //        log_reg.IP = Dns.GetHostByName(Dns.GetHostName().ToString()).AddressList[0].ToString();
                            //        log_reg.ComputerName = Dns.GetHostName().ToString();
                            //        log_reg.Lock = true;
                            //        log_reg.Browser = HttpContext.Request.Browser.Browser;
                            //        EmpData.LogRegister.Add(log_reg);
                            //        db.SaveChanges();

                            //        EmpData.Login.IsActive = false;
                            //        db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                            //        db.SaveChanges();
                            //    }
                            //    else
                            //    {
                            //        log_reg.LogInAttempt = login_hits + 1;
                            //        log_reg.LogInDate = DateTime.Now;
                            //        log_reg.LogInTime = DateTime.Now.TimeOfDay.ToString();
                            //        var HostName = Dns.GetHostName().ToString();
                            //        log_reg.IP = Dns.GetHostByName(HostName).AddressList[0].ToString();
                            //        log_reg.ComputerName = HostName;
                            //        log_reg.Browser = HttpContext.Request.Browser.Browser;
                            //        EmpData.LogRegister.Add(log_reg);
                            //        db.SaveChanges();
                            //    }
                            //    return Json(new { success = false, data = "UserName Or PassWord is Incorrect..!" }, JsonRequestBehavior.AllowGet);
                            //}

                            // In API CHECK
                        }
                        else
                        {
                            return Json(new { success = false, data = "User Name Not Exists..!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    #endregion Normal Login
                    else
                    {

                        #region SAML Login
                        //if (!String.IsNullOrEmpty(x) && !String.IsNullOrEmpty(y))
                        //{

                            #region If User IsAdmin
                            if (x == null)
                            {
                                //admin login scope

                                #region If Database IsExists
                                if (db.Database.Exists())
                                {
                                    db.Database.Connection.Open();
                                    PasswordPolicy login_policy = new PasswordPolicy();


                                    string SMMLloginuser = "";
                                    if (xmlResponse != null)
                                    {
                                        SMMLloginuser = xmlResponse.Assertion.AttributeStatement.ToList()
                                           .Where(z => z.Name.Equals("EmployeeID"))
                                           .Select(s => s.AttributeValue).FirstOrDefault();
                                    }


                                    var getdbUserID = db.Employee.Include(e => e.Login).Where(e => e.EmpCode == SMMLloginuser).Select(l => l.Login).SingleOrDefault();

                                    if (getdbUserID == null)
                                    {
                                        return this.Json(new { success = false, data = " The Entered User NOT Found !!! " }, "text/html; charset=utf-8", JsonRequestBehavior.AllowGet);
                                    }




                                    if (db.Login.Any(e => e.UserId == getdbUserID.UserId))
                                    {
                                        var LoginUser = db.Employee.Include(e => e.Login).Where(e => e.Login.UserId.Equals(getdbUserID.UserId.Trim())).FirstOrDefault();
                                        //var LoginUser = db.Employee.Include(e => e.Login).Where(e => e.Login.UserId.Equals(x.Trim())).SingleOrDefault();

                                        login_policy = db.PasswordPolicy.FirstOrDefault();
                                        string AdminLoginPassword = login_policy.AllowEncryption == true ? P2BSecurity.Decrypt(LoginUser.Login.Password) : LoginUser.Login.Password;

                                        if (LoginUser != null && LoginUser.Login != null && y != AdminLoginPassword)
                                        {
                                            return Json(new { success = false, data = "UserId or PassWord Wrong..!" }, JsonRequestBehavior.AllowGet);
                                        }
                                        if (LoginUser != null)
                                        {
                                            SessionManager.UserName = SessionManager.EmpId = LoginUser.Id.ToString();
                                            SessionManager.EmpLvId = db.EmployeeLeave.Where(e => e.Employee != null && e.Employee.Id == LoginUser.Id).Select(e => e.Id).SingleOrDefault().ToString();
                                            var id = Convert.ToInt32(SessionManager.UserName);
                                            var EmpPayrollData = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == id).SingleOrDefault();
                                            var Comp = db.CompanyPayroll.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll.Any(a => a.Id == EmpPayrollData.Id)).Select(e => e.Company).ToList();
                                            if (Comp != null && Comp.Count > 0)
                                            {
                                                Session["CompId"] = SessionManager.CompanyId = Comp.Select(e => e.Id.ToString()).FirstOrDefault();
                                            }
                                        }
                                        else
                                        {
                                            SessionManager.UserName = "0";
                                        }

                                        if (db.Company.Count() == 1)
                                        {
                                            //employee is not assign but comp create
                                            Session["CompId"] = SessionManager.CompanyId = db.Company.Select(e => e.Id.ToString()).FirstOrDefault();
                                        }
                                        if (!string.IsNullOrEmpty(SessionManager.CompanyId) && SessionManager.CompanyId != "0")
                                        {
                                            var id = Convert.ToInt32(Session["CompId"].ToString());
                                            var fyear = db.Company.Include(e => e.Calendar).Include(e => e.Calendar.Select(a => a.Name)).Where(e => e.Id == id).SingleOrDefault();
                                            if (fyear != null)
                                            {
                                                SessionManager.FinancialYear = fyear.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).Select(e => e.Id).SingleOrDefault().ToString();
                                            }
                                            SessionManager.CompPayrollId = db.CompanyPayroll.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).SingleOrDefault();
                                            SessionManager.CompLvId = db.CompanyLeave.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).SingleOrDefault();
                                        }
                                        var CompId = int.Parse(SessionManager.CompanyId);
                                        var EmpAll = db.CompanyPayroll
                                            .Include(e => e.Company)
                                            .Include(e => e.EmployeePayroll)
                                            .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                                            .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                                            .Include(e => e.EmployeePayroll.Select(a => a.Employee.Login))
                                            .Where(e => e.Company.Id == CompId)
                                            .FirstOrDefault();
                                        /*
                                         * insert record in login table
                                         */
                                        var Emps = EmpAll.EmployeePayroll.Where(e => e.Employee.Login == null && e.Employee.ServiceBookDates != null && e.Employee.ServiceBookDates.ServiceLastDate == null).Select(e => new
                                        {
                                            _Id = e.Employee.Id,
                                            _EmpCode = e.Employee.EmpCode,
                                            _EmpPwd = e.Employee.EmpCode
                                        }).ToList();




                                        foreach (var item in Emps)
                                        {
                                            // For Encryption and Decryption of User Password
                                            string EncryptedPassword = "";
                                            if (item._Id != null)
                                            {
                                                EncryptedPassword = login_policy.AllowEncryption == true ? P2BSecurity.Encrypt(item._EmpPwd) : item._EmpPwd;

                                            }

                                            //if (item.Login == null)
                                            //{
                                            var newLogin = new Login();
                                            newLogin.UserId = item._EmpCode;
                                            newLogin.Password = EncryptedPassword;
                                            newLogin.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                            newLogin.IsUltimateAppl = true;
                                            newLogin.IsActive = true;
                                            db.Login.Add(newLogin);
                                            db.SaveChanges();

                                            var emp = db.Employee.Include(e => e.Login).Where(e => e.Id == item._Id).SingleOrDefault();
                                            emp.Login = newLogin;

                                            db.Entry(emp).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            //}
                                        }
                                        return RedirectToAction("index", "Home");
                                       // return Json(new { success = true, data = Url.Action("index", "Home") }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                #endregion If Database IsExists
                                else
                                {
                                    try
                                    {
                                        return Json(new { success = false, data = "DataBase Not Exists..!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    catch (Exception e)
                                    {
                                        return Json(new { success = false, data = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            #endregion If User IsAdmin
                            //User Exitance check
                            //    if (!db.Login.Any(e => e.UserId == x.Trim()))

                            if (db.Login.Where(e => e.UserId == x.Trim()).AsNoTracking().FirstOrDefault() == null)
                            {
                                return Json(new { success = false, data = "User Not Exists..!" }, JsonRequestBehavior.AllowGet);
                            }


                            //GetEmpData
                            PasswordPolicy PassLogin_Policy = db.PasswordPolicy.FirstOrDefault();
                            var EmpData = db.Employee.Include(e => e.Login).Include(e => e.LogRegister).Where(e => e.Login != null && e.Login.UserId == x.Trim()).SingleOrDefault();
                            if (EmpData == null)
                            {
                                return Json(new { success = false, data = "Invalid Account..!" }, JsonRequestBehavior.AllowGet);
                            }
                            var Retiredemp = db.Employee.Include(e => e.Login).Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.ServiceLastDate != null && e.Login.UserId == x.Trim()).SingleOrDefault();
                            if (Retiredemp != null)
                            {
                                return Json(new { success = false, data = "Retired Employee Cannot Login....!" }, JsonRequestBehavior.AllowGet);
                            }

                            var CheckRetired = db.Employee.Include(e => e.Login).Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.RetirementDate != null && e.Login.UserId == x.Trim() && e.ServiceBookDates.RetirementDate < DateTime.Today).SingleOrDefault();
                            if (CheckRetired != null)
                            {
                                return Json(new { success = false, data = "Retired Employee Cannot Login....!" }, JsonRequestBehavior.AllowGet);
                            }

                            //Check Ultimate access
                            if (EmpData.Login.IsUltimateAppl != true)
                            {
                                return Json(new { success = false, data = "You Don't Have Permission To Access..!" }, JsonRequestBehavior.AllowGet);
                            }

                            LogRegister log_reg = new LogRegister();
                            log_reg.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var log = EmpData.LogRegister.LastOrDefault();
                            if (EmpData != null && EmpData.Login.IsActive == false)
                            {
                                return Json(new { success = false, data = "Account is Locked..!" }, JsonRequestBehavior.AllowGet);
                            }
                            int login_hits = log == null ? 1 : log.LogInAttempt;

                            if (EmpData != null)
                            {
                                if (EmpData != null)
                                {
                                    SessionManager.UserName = SessionManager.EmpId = EmpData.Id.ToString();
                                    SessionManager.EmpLvId = db.EmployeeLeave.Where(e => e.Employee.Id == EmpData.Id).Select(e => e.Id).SingleOrDefault().ToString();
                                    var id = Convert.ToInt32(SessionManager.UserName);
                                    var EmpPayrollData = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == id).SingleOrDefault();
                                    var Comp = db.CompanyPayroll.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll.Any(a => a.Id == EmpPayrollData.Id)).Select(e => e.Company).ToList();
                                    if (Comp != null && Comp.Count > 0)
                                    {
                                        Session["CompId"] = SessionManager.CompanyId = Comp.Select(e => e.Id.ToString()).FirstOrDefault();
                                    }
                                }
                                else
                                {
                                    SessionManager.UserName = EmpData.Login.UserId;
                                }
                                if (!string.IsNullOrEmpty(SessionManager.CompanyId) && SessionManager.CompanyId != "0")
                                {
                                    var id = Convert.ToInt32(Session["CompId"].ToString());
                                    var fif = db.Company.Include(e => e.Calendar).Include(e => e.Calendar.Select(a => a.Name)).Where(e => e.Id == id).SingleOrDefault();
                                    if (fif != null)
                                    {
                                        SessionManager.FinancialYear = fif.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).Select(e => e.Id).SingleOrDefault().ToString();
                                    }
                                    SessionManager.CompPayrollId = db.CompanyPayroll.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).SingleOrDefault();
                                    SessionManager.CompLvId = db.CompanyLeave.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).SingleOrDefault();
                                }
                                if (!string.IsNullOrEmpty(SessionManager.CompanyId) && !string.IsNullOrEmpty(SessionManager.EmpId))
                                {
                                    /*
                                     * Reporting Struct Scope
                                     */
                                    List<ReportingStructRights> oReportingStructRights = PortalRights.ScanRights(0, Convert.ToInt32(SessionManager.EmpId), Convert.ToInt32(SessionManager.CompanyId));
                                }

                                // Decryption of Password
                                string DecryptionPassword = "";
                                if (!String.IsNullOrEmpty(y))
                                {
                                    DecryptionPassword = PassLogin_Policy.AllowEncryption == true ? P2BSecurity.Decrypt(EmpData.Login.Password) : EmpData.Login.Password;
                                }


                                //if (EmpData.Login.Password == y.Trim())
                                if (DecryptionPassword == y.Trim())
                                {
                                    try
                                    {
                                        if (db.PasswordPolicy.ToList().Count() == 0)
                                        {
                                            PasswordPolicy _passwordpolicy = new PasswordPolicy();
                                            _passwordpolicy.MinPwdLength = 8;
                                            _passwordpolicy.OldPwdAllow = false;
                                            _passwordpolicy.PasswordSecurityQuestion = true;
                                            _passwordpolicy.LoginSecurityQuestionAppl = true;
                                            _passwordpolicy.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                            _passwordpolicy.AllowAttempt = 3;
                                            db.PasswordPolicy.Add(_passwordpolicy);
                                            db.SaveChanges();
                                        }
                                        using (TransactionScope ts = new TransactionScope())
                                        {
                                            log_reg.LogInAttempt = 1;
                                            log_reg.LogInDate = DateTime.Now;
                                            log_reg.LogInTime = DateTime.Now.TimeOfDay.ToString();
                                            log_reg.LogInDate = DateTime.Now;
                                            log_reg.LogInTime = DateTime.Now.TimeOfDay.ToString();
                                            log_reg.IP = Dns.GetHostByName(Dns.GetHostName().ToString()).AddressList[0].ToString();
                                            log_reg.ComputerName = Dns.GetHostName().ToString();
                                            log_reg.Browser = HttpContext.Request.Browser.Browser;
                                            EmpData.LogRegister.Add(log_reg);
                                            //  db.LogRegister.Add(log_reg);

                                            db.SaveChanges();
                                            ts.Complete();
                                        }
                                    }

                                    catch (DbUpdateConcurrencyException ex)
                                    {
                                        ex.Entries.Single().Reload();

                                        db.SaveChanges();

                                    }
                                    catch (Exception e)
                                    {
                                        return Json(new { success = false, data = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);

                                    }
                                    //if username and password are same consider the user is 1st loged in ,and force to change password
                                    if (EmpData.Login.UserId == EmpData.Login.Password)
                                    {
                                        return Json(new { success = true, data = Url.Action("Index", "welcomescreen") }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        // PortalRights.ScanRights();
                                        return Json(new { success = true, data = Url.Action("index", "home") }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    var CompId = Convert.ToInt32(SessionManager.CompanyId);
                                    var login_policy = db.Company.Include(e => e.PasswordPolicy).Where(e => e.Id == CompId && e.PasswordPolicy != null).OrderByDescending(e => e.Id).Select(e => e.PasswordPolicy).SingleOrDefault();
                                    //if (login_policy != null && login_policy.AllowAttempt < login_hits)
                                    //{
                                        if (login_policy != null)
                                    {
                                        log_reg.LogInAttempt = login_hits;
                                        log_reg.LogInDate = DateTime.Now;
                                        log_reg.LogInTime = DateTime.Now.TimeOfDay.ToString();
                                        log_reg.IP = Dns.GetHostByName(Dns.GetHostName().ToString()).AddressList[0].ToString();
                                        log_reg.ComputerName = Dns.GetHostName().ToString();
                                        log_reg.Lock = true;
                                        log_reg.Browser = HttpContext.Request.Browser.Browser;
                                        EmpData.LogRegister.Add(log_reg);
                                        db.SaveChanges();

                                        EmpData.Login.IsActive = false;
                                        db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        log_reg.LogInAttempt = login_hits + 1;
                                        log_reg.LogInDate = DateTime.Now;
                                        log_reg.LogInTime = DateTime.Now.TimeOfDay.ToString();
                                        var HostName = Dns.GetHostName().ToString();
                                        log_reg.IP = Dns.GetHostByName(HostName).AddressList[0].ToString();
                                        log_reg.ComputerName = HostName;
                                        log_reg.Browser = HttpContext.Request.Browser.Browser;
                                        EmpData.LogRegister.Add(log_reg);
                                        db.SaveChanges();
                                    }
                                    return Json(new { success = false, data = "UserName Or PassWord is Incorrect..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                return Json(new { success = false, data = "User Name Not Exists..!" }, JsonRequestBehavior.AllowGet);
                            }
                      //  }
                        #endregion SAML Login


                        return Json(new { success = false, data = "Something Is Wrong..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
                    return Json(new { success = false, data = e.Message.ToString() }, JsonRequestBehavior.AllowGet);

                }

            }
        }


        #region FOR SAML LOGIN RESPONSE START
        static string SResponse(string destination, string request, string clientId)
        {
            P2B.UTILS.P2BLogger loggerObj = new P2B.UTILS.P2BLogger();
            try
            {
                //P2BLogger p2BLogger = new P2BLogger();

                loggerObj.Logging("Web Request Start ----");
                loggerObj.Logging("URL : " + destination);
                loggerObj.Logging("SAMLRequest  : " + request);
                loggerObj.Logging("clientID : " + clientId);
                //  System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(destination);
                httpWebRequest.Method = "POST";
                string smalREquest = "SAMLRequest=" + request.Replace("+", "-").Replace("/", "_");
                string clId = "&clientID=" + clientId;
                string postdata = smalREquest;
                postdata += clId;
                loggerObj.Logging("Request Url Safe " + postdata);
                byte[] data = Encoding.UTF8.GetBytes(postdata);

                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.ContentLength = data.Length;

                Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();


                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                Stream responseStream = httpWebResponse.GetResponseStream();

                StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);

                string pageContent = myStreamReader.ReadToEnd();

                myStreamReader.Close();
                responseStream.Close();

                httpWebResponse.Close();

                loggerObj.Logging("RESPONSE ::: " + pageContent);
                return pageContent;
            }
            catch (Exception ex)
            {
                loggerObj.Logging(ex.Message);
                loggerObj.Logging(ex.InnerException);
                loggerObj.Logging(ex.StackTrace);
                throw ex;
            }
            loggerObj.Logging("Web Request End ----");
            return null;
        }
        #endregion FOR SAML LOGIN RESPONSE END


        public ActionResult Logout()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!String.IsNullOrEmpty(SessionManager.EmpId))
                {
                    var emp = Convert.ToInt32(SessionManager.EmpId);
                    var emplogregister = db.Employee.Include(e => e.Login).Include(e => e.LogRegister).Where(e => e.Id == emp).SingleOrDefault();
                    var last_empregister = emplogregister.LogRegister.LastOrDefault();
                    if (last_empregister != null)
                    {
                        last_empregister.LogOutDate = DateTime.Now;
                        last_empregister.LogOutTime = Convert.ToString(DateTime.Now.ToLocalTime());
                        db.LogRegister.Attach(last_empregister);
                        db.Entry(last_empregister).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(last_empregister).State = System.Data.Entity.EntityState.Detached;

                    }
                }
                Session.Abandon();
                Session.Clear();
                db.Database.Connection.Close();
                db.Dispose();
                return RedirectToAction("index", "login");
            }
        }

        public String check_session(String data)
        {
            var val = Session["captchaText"].ToString();
            if (data == val)
            {
                return "true";
            }
            else
            {
                return "false";
            }
        }
        public ActionResult GetUserinfo()
        {
            var a = Utility.GetUserData();
            if (a != null)
            {
                return Json(new { success = true, data = a }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, data = "" }, JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult CheckReportingStructure()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(SessionManager.EmpId);
                var AdminChk = db.Employee.Include(e => e.Login).Where(e => e.Id == id && e.Login != null && e.Login.UserId.ToLower() == "admin").SingleOrDefault();
                if (AdminChk != null)
                {
                    var qurey = db.Employee.Include(e => e.EmpName).Include(e => e.EmpName.EmpTitle).Include(e => e.ReportingStructRights).Where(e => e.ReportingStructRights.Count() == 0 && e.Login != null).ToList();
                    List<EssPortal.Controllers.HomeController.returnEmpPwdClass> ListreturnEmpPwdClass = new List<EssPortal.Controllers.HomeController.returnEmpPwdClass>();

                    foreach (var item in qurey)
                    {
                        ListreturnEmpPwdClass.Add(new EssPortal.Controllers.HomeController.returnEmpPwdClass()
                        {
                            Id = item.Id,
                            val = "EmpCode :" + item.EmpCode + " EmpName :" + item.EmpName.FullNameFML
                        });
                    }

                    if (qurey != null)
                    {
                        return Json(new { status = true, data = ListreturnEmpPwdClass }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult Checkvisiblebtn()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
                bool existschk = System.IO.Directory.Exists(requiredPathchk);
                string localPathchk;
                if (!existschk)
                {
                    localPathchk = new Uri(requiredPathchk).LocalPath;
                    System.IO.Directory.CreateDirectory(localPathchk);
                }
                string pathchk = requiredPathchk + @"\ButtonVisible" + ".ini";
                localPathchk = new Uri(pathchk).LocalPath;
                using (var streamReader = new StreamReader(localPathchk))
                {
                    string line;

                    while ((line = streamReader.ReadLine()) != null)
                    {

                        return Json(line, JsonRequestBehavior.AllowGet);
                    }
                }

             
              
                return null;

            }
        }
        public ActionResult EssModuleAccess()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                // button visible ini file

                string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
                bool existschk = System.IO.Directory.Exists(requiredPathchk);
                string localPathchk;
                if (!existschk)
                {
                    string localPathc = new Uri(requiredPathchk).LocalPath;
                    System.IO.Directory.CreateDirectory(localPathc);
                }
                string pathchk = requiredPathchk + @"\ButtonVisible" + ".ini";
                localPathchk = new Uri(pathchk).LocalPath;

                if (!System.IO.File.Exists(localPathchk))
                {

                    using (var fs = new FileStream(localPathchk, FileMode.OpenOrCreate))
                    {
                        StreamWriter str = new StreamWriter(fs);
                        str.BaseStream.Seek(0, SeekOrigin.Begin);

                        str.Flush();
                        str.Close();
                        fs.Close();
                    }


                }
                // button visible ini file

                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
                bool exists = System.IO.Directory.Exists(requiredPath);
                string localPath;
                if (!exists)
                {
                    localPath = new Uri(requiredPath).LocalPath;
                    System.IO.Directory.CreateDirectory(localPath);
                }
                string path = requiredPath + @"\ModuleAccess" + ".ini";
                localPath = new Uri(path).LocalPath;
                if (!System.IO.File.Exists(localPath))
                {

                    using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                    {
                        StreamWriter str = new StreamWriter(fs);
                        str.BaseStream.Seek(0, SeekOrigin.Begin);

                        str.Flush();
                        str.Close();
                        fs.Close();
                    }

                }

                else
                {
                    using (var streamReader = new StreamReader(localPath))
                    {
                        ArrayList moduleArray = new ArrayList();
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            int a = 0;
                            var moduleNames = line.Split('_');


                            while (moduleNames != null && a < moduleNames.Length)
                            {
                                var modulename = moduleNames[a].ToUpper();
                                moduleArray.Add(modulename);
                                a++;
                            }

                            TempData["Module"] = moduleArray;

                        }

                    }
                }

                var ModuleName = TempData["Module"];
                return Json(ModuleName, JsonRequestBehavior.AllowGet);


               
                
            }
            
        }


    }
}

