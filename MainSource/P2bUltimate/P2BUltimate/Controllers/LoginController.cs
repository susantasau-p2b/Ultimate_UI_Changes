using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using P2b.Global;
using System.Data.Entity;
using P2BUltimate.App_Start;
using P2BUltimate.Security;
using System.Net;
using P2BUltimate.Models;
using Payroll;
//using P2BUltimate.Process;
using System.Transactions;
using System.IO;
using EMS;
using Appraisal;
using Training;
using Recruitment;
using Attendance;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.Web.UI;
using P2B.SAML;
using System.Configuration;
using P2B.UTILS;
using P2B.PFTRUST;
namespace P2BUltimate.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        Response xmlResponse;
        public ActionResult Index()
        {
            string readstr = "";
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2bUltimate\\App_Data\\Menu_Json";
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
                //var samlResponse = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4KPHNhbWwycDpSZXNwb25zZSBEZXN0aW5hdGlvbj0iaHR0cHM6Ly9ocm1zdWF0LmtlcmFsYWJhbmsuY28uaW4vSFJNU1VBVC9Mb2dpbi9pbmRleCIgSUQ9ImNPY19lNWk2Y1hzTjBhcUd5SnM4cEJ3aHJOVGgyNGVKSXd3R215SEpBZU00NSIgSW5SZXNwb25zZVRvPSIyQ1BoWG4yVTlkT2E4YlJzY1NQN3IyR1F4LTUxZlhKSUYtWDBJbFIyUTl3IiBJc3N1ZUluc3RhbnQ9IjIwMjMtMDgtMzFUMDY6NTc6MzEuNTA1WiIgVmVyc2lvbj0iMi4wIiB4bWxuczpzYW1sMnA9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjIuMDpwcm90b2NvbCI+PHNhbWwyOklzc3VlciB4bWxuczpzYW1sMj0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOmFzc2VydGlvbiI+ZW1JREFNRW50aXR5SWQwMTwvc2FtbDI6SXNzdWVyPjxzYW1sMnA6U3RhdHVzPjxzYW1sMnA6U3RhdHVzQ29kZSBWYWx1ZT0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOnN0YXR1czpTdWNjZXNzIi8+PHNhbWwycDpTdGF0dXNNZXNzYWdlPmFjY2Vzc0dyYW50ZWQ8L3NhbWwycDpTdGF0dXNNZXNzYWdlPjwvc2FtbDJwOlN0YXR1cz48c2FtbDI6QXNzZXJ0aW9uIElEPSJjT2NfZTVpNmNYc04wYXFHeUpzOHBCd2hyTlRoMjRlSkl3d0dteUhKQWVNIiBJc3N1ZUluc3RhbnQ9IjIwMjMtMDgtMzFUMDY6NTc6MzEuNTA1WiIgVmVyc2lvbj0iMi4wIiB4bWxuczpzYW1sMj0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOmFzc2VydGlvbiI+PHNhbWwyOklzc3Vlcj5lbUlEQU1FbnRpdHlJZDAxPC9zYW1sMjpJc3N1ZXI+PHNhbWwyOlN1YmplY3Q+PHNhbWwyOk5hbWVJRCBGb3JtYXQ9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjEuMTpuYW1laWQtZm9ybWF0OmVtYWlsQWRkcmVzcyI+c3VyZXNoPC9zYW1sMjpOYW1lSUQ+PHNhbWwyOlN1YmplY3RDb25maXJtYXRpb24gTWV0aG9kPSJ1cm46b2FzaXM6bmFtZXM6dGM6U0FNTDoyLjA6Y206YmVhcmVyIj48c2FtbDI6U3ViamVjdENvbmZpcm1hdGlvbkRhdGEgSW5SZXNwb25zZVRvPSIyQ1BoWG4yVTlkT2E4YlJzY1NQN3IyR1F4LTUxZlhKSUYtWDBJbFIyUTl3IiBOb3RPbk9yQWZ0ZXI9IjIwMjMtMDgtMzFUMDc6MTI6MzEuNTA1WiIgUmVjaXBpZW50PSJodHRwczovL2hybXN1YXQua2VyYWxhYmFuay5jby5pbi9IUk1TVUFUL0xvZ2luL2luZGV4Ii8+PC9zYW1sMjpTdWJqZWN0Q29uZmlybWF0aW9uPjwvc2FtbDI6U3ViamVjdD48c2FtbDI6Q29uZGl0aW9ucyBOb3RPbk9yQWZ0ZXI9IjIwMjMtMDgtMzFUMDc6MTI6MzEuNTA1WiI+PHNhbWwyOkF1ZGllbmNlUmVzdHJpY3Rpb24+PHNhbWwyOkF1ZGllbmNlPndpcHIwMTwvc2FtbDI6QXVkaWVuY2U+PC9zYW1sMjpBdWRpZW5jZVJlc3RyaWN0aW9uPjwvc2FtbDI6Q29uZGl0aW9ucz48c2FtbDI6QXV0aG5TdGF0ZW1lbnQgQXV0aG5JbnN0YW50PSIyMDIzLTA4LTMxVDA2OjU3OjMxLjUwNVoiIFNlc3Npb25JbmRleD0iYTFIVU1kOWRmeFFjdnM3TTk1N2ZQZGhodzdRR253c1JaaG8tNzZ5N3FSZyIgU2Vzc2lvbk5vdE9uT3JBZnRlcj0iMjAyMy0wOC0zMVQwNzoxMjozMS41MDVaIj48c2FtbDI6QXV0aG5Db250ZXh0PjxzYW1sMjpBdXRobkNvbnRleHRDbGFzc1JlZi8+PC9zYW1sMjpBdXRobkNvbnRleHQ+PC9zYW1sMjpBdXRoblN0YXRlbWVudD48c2FtbDI6QXR0cmlidXRlU3RhdGVtZW50PjxzYW1sMjpBdHRyaWJ1dGUgTmFtZT0iYXBwX25hbWUiIE5hbWVGb3JtYXQ9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjIuMDphdHRybmFtZS1mb3JtYXQ6YmFzaWMiPjxzYW1sMjpBdHRyaWJ1dGVWYWx1ZSB4bWxuczp4cz0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEiIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhzaTp0eXBlPSJ4czpzdHJpbmciPkhSTVM8L3NhbWwyOkF0dHJpYnV0ZVZhbHVlPjwvc2FtbDI6QXR0cmlidXRlPjxzYW1sMjpBdHRyaWJ1dGUgTmFtZT0iZW1haWwiIE5hbWVGb3JtYXQ9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjIuMDphdHRybmFtZS1mb3JtYXQ6YmFzaWMiPjxzYW1sMjpBdHRyaWJ1dGVWYWx1ZSB4bWxuczp4cz0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEiIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhzaTp0eXBlPSJ4czpzdHJpbmciPnN1cmVzaC5hZWx1Z3VydUB3aXByby5jb208L3NhbWwyOkF0dHJpYnV0ZVZhbHVlPjwvc2FtbDI6QXR0cmlidXRlPjxzYW1sMjpBdHRyaWJ1dGUgTmFtZT0iYWNjZXNzX21vZGUiIE5hbWVGb3JtYXQ9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjIuMDphdHRybmFtZS1mb3JtYXQ6YmFzaWMiPjxzYW1sMjpBdHRyaWJ1dGVWYWx1ZSB4bWxuczp4cz0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEiIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhzaTp0eXBlPSJ4czpzdHJpbmciPndyaXRlPC9zYW1sMjpBdHRyaWJ1dGVWYWx1ZT48L3NhbWwyOkF0dHJpYnV0ZT48c2FtbDI6QXR0cmlidXRlIE5hbWU9InVzZXJfbmFtZSIgTmFtZUZvcm1hdD0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOmF0dHJuYW1lLWZvcm1hdDpiYXNpYyI+PHNhbWwyOkF0dHJpYnV0ZVZhbHVlIHhtbG5zOnhzPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYSIgeG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeHNpOnR5cGU9InhzOnN0cmluZyI+c3VyZXNoPC9zYW1sMjpBdHRyaWJ1dGVWYWx1ZT48L3NhbWwyOkF0dHJpYnV0ZT48c2FtbDI6QXR0cmlidXRlIE5hbWU9Im1vYmlsZU51bWJlciIgTmFtZUZvcm1hdD0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOmF0dHJuYW1lLWZvcm1hdDpiYXNpYyI+PHNhbWwyOkF0dHJpYnV0ZVZhbHVlIHhtbG5zOnhzPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYSIgeG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeHNpOnR5cGU9InhzOnN0cmluZyI+OTU1MzI1MzY1Mzwvc2FtbDI6QXR0cmlidXRlVmFsdWU+PC9zYW1sMjpBdHRyaWJ1dGU+PHNhbWwyOkF0dHJpYnV0ZSBOYW1lPSJyb2xlIiBOYW1lRm9ybWF0PSJ1cm46b2FzaXM6bmFtZXM6dGM6U0FNTDoyLjA6YXR0cm5hbWUtZm9ybWF0OmJhc2ljIj48c2FtbDI6QXR0cmlidXRlVmFsdWUgeG1sbnM6eHM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hIiB4bWxuczp4c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIiB4c2k6dHlwZT0ieHM6c3RyaW5nIj5hZG1pbjwvc2FtbDI6QXR0cmlidXRlVmFsdWU+PC9zYW1sMjpBdHRyaWJ1dGU+PHNhbWwyOkF0dHJpYnV0ZSBOYW1lPSJmaXJzdE5hbWUiIE5hbWVGb3JtYXQ9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjIuMDphdHRybmFtZS1mb3JtYXQ6YmFzaWMiPjxzYW1sMjpBdHRyaWJ1dGVWYWx1ZSB4bWxuczp4cz0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEiIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhzaTp0eXBlPSJ4czpzdHJpbmciPnN1cmVzaDwvc2FtbDI6QXR0cmlidXRlVmFsdWU+PC9zYW1sMjpBdHRyaWJ1dGU+PHNhbWwyOkF0dHJpYnV0ZSBOYW1lPSJsYXN0TmFtZSIgTmFtZUZvcm1hdD0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOmF0dHJuYW1lLWZvcm1hdDpiYXNpYyI+PHNhbWwyOkF0dHJpYnV0ZVZhbHVlIHhtbG5zOnhzPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYSIgeG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeHNpOnR5cGU9InhzOnN0cmluZyI+YWVsdWd1cnU8L3NhbWwyOkF0dHJpYnV0ZVZhbHVlPjwvc2FtbDI6QXR0cmlidXRlPjxzYW1sMjpBdHRyaWJ1dGUgTmFtZT0iRW1wbG95ZWVJRCIgTmFtZUZvcm1hdD0idXJuOm9hc2lzOm5hbWVzOnRjOlNBTUw6Mi4wOmF0dHJuYW1lLWZvcm1hdDpiYXNpYyI+PHNhbWwyOkF0dHJpYnV0ZVZhbHVlIHhtbG5zOnhzPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYSIgeG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeHNpOnR5cGU9InhzOnN0cmluZyI+MjAzMzExMTE8L3NhbWwyOkF0dHJpYnV0ZVZhbHVlPjwvc2FtbDI6QXR0cmlidXRlPjwvc2FtbDI6QXR0cmlidXRlU3RhdGVtZW50Pjwvc2FtbDI6QXNzZXJ0aW9uPjwvc2FtbDJwOlJlc3BvbnNlPg==";//context.Params.GetValues("SAMLResponse")[0];
                
                var samlResponse = context.Params.GetValues("SAMLResponse");
                
                //var samlResponse = string.Empty;
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
                        var request = string.Empty; //var request = p2BAuthenticationReq.GetRedirectURL(destination, entity, P2B.SAML.Helper.P2BRequestFormatType.EmailFormat);
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
            if (!String.IsNullOrEmpty(SessionManager.UserName) && !String.IsNullOrEmpty(SessionManager.FinancialYear) && String.IsNullOrEmpty(readstr))
            {
                return RedirectToAction("index", "dashboard");
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

        public void CreatePassWordPolicy()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CompId = Convert.ToInt32(SessionManager.CompanyId);
                PasswordPolicy _passwordpolicy = new PasswordPolicy();
                _passwordpolicy.MinPwdLength = 8;
                _passwordpolicy.OldPwdAllow = false;
                _passwordpolicy.PasswordSecurityQuestion = true;
                _passwordpolicy.LoginSecurityQuestionAppl = true;
                _passwordpolicy.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                _passwordpolicy.AllowAttempt = 3;
                db.PasswordPolicy.Add(_passwordpolicy);
                db.SaveChanges();

                var Company = db.Company.Include(e => e.PasswordPolicy).Where(e => e.Id == CompId).SingleOrDefault();
                var PasswordPolicy_list = new List<PasswordPolicy>();
                PasswordPolicy_list.Add(_passwordpolicy);

                Company.PasswordPolicy = PasswordPolicy_list;
                db.Company.Attach(Company);
                db.Entry(Company).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }
        public void CreateLogRegisterEntry(Int32 EmpId, Int32 type)
        {
            /*
             * 0 -pass correct,
             * 1-wrong
             */
            using (DataBaseContext db = new DataBaseContext())
            {
                if (EmpId != 0)
                {
                    var LogRegData = db.Employee.Include(e => e.Login).Include(e => e.LogRegister).Where(e => e.Id == EmpId).SingleOrDefault();
                    var last_LogRegData = LogRegData.LogRegister.LastOrDefault();
                    LogRegister oLogRegister = new LogRegister();
                    if (LogRegData.LogRegister != null && LogRegData.LogRegister.Count() >0)
                    {
                        if (type == 0)
                        {
                            oLogRegister.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            oLogRegister.LogInAttempt = 1;
                            oLogRegister.LogInDate = DateTime.Now;
                            oLogRegister.LogInTime = DateTime.Now.TimeOfDay.ToString();
                            //oLogRegister.LogInDate = DateTime.Now;
                            //oLogRegister.LogInTime = DateTime.Now.TimeOfDay.ToString();
                            oLogRegister.IP = Dns.GetHostByName(Dns.GetHostName().ToString()).AddressList[0].ToString();
                            oLogRegister.ComputerName = Dns.GetHostName().ToString();
                            oLogRegister.Browser = HttpContext.Request.Browser.Browser;
                            LogRegData.LogRegister.Add(oLogRegister);
                            db.SaveChanges();

                        }
                        else
                        {
                            oLogRegister.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            oLogRegister.LogInAttempt = last_LogRegData.LogInAttempt + 1;
                            oLogRegister.LogInDate = DateTime.Now;
                            oLogRegister.LogInTime = DateTime.Now.TimeOfDay.ToString();
                            //oLogRegister.LogInDate = DateTime.Now;
                            //oLogRegister.LogInTime = DateTime.Now.TimeOfDay.ToString();
                            oLogRegister.IP = Dns.GetHostByName(Dns.GetHostName().ToString()).AddressList[0].ToString();
                            oLogRegister.ComputerName = Dns.GetHostName().ToString();
                            oLogRegister.Browser = HttpContext.Request.Browser.Browser;
                            LogRegData.LogRegister.Add(oLogRegister);
                            db.SaveChanges();

                        }
                    }
                    else
                    {
                        oLogRegister.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        oLogRegister.LogInAttempt =  1;
                        oLogRegister.LogInDate = DateTime.Now;
                        oLogRegister.LogInTime = DateTime.Now.TimeOfDay.ToString();
                        //oLogRegister.LogInDate = DateTime.Now;
                        //oLogRegister.LogInTime = DateTime.Now.TimeOfDay.ToString();
                        oLogRegister.IP = Dns.GetHostByName(Dns.GetHostName().ToString()).AddressList[0].ToString();
                        oLogRegister.ComputerName = Dns.GetHostName().ToString();
                        oLogRegister.Browser = HttpContext.Request.Browser.Browser;
                        LogRegData.LogRegister.Add(oLogRegister);
                        db.SaveChanges();

                    }
                 
                }
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
            try
            {


                if (P2BUltimate.Models.Utility.CheckNetworkIssue.Networkissue == 1)
                {
                    return Json(new { success = false, data = "Network Related Issue" }, JsonRequestBehavior.AllowGet);
                }
                using (DataBaseContext db = new DataBaseContext())
                {
                    var getCompany = db.Company.Where(e => e.Code.ToUpper() != "KB").FirstOrDefault(); // No check For Kerala bank because they are using SAML Login.
                    if (getCompany != null)
                    {
                        if (String.IsNullOrEmpty(x) || String.IsNullOrEmpty(y))
                        {
                            return Json(new { success = false, data = "User Id or Password should not be blanks..!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                   

                    #region Normal Login
                    if (!String.IsNullOrEmpty(x) && !String.IsNullOrEmpty(y))
                    {
                        #region If User IsAdmin
                        if (x == "admin")
                        {

                            #region If Database IsExists
                            if (db.Database.Exists())
                            {
                                db.Database.Connection.Open();
                                PasswordPolicy login_policy = new PasswordPolicy();
                                var oLogRegister = new Employee();
                                var CompId = 0;

                                #region UserId == admin
                                if (db.Login.Any(e => e.UserId == "admin"))
                                {
                                    var LoginUser = db.Employee.Include(e => e.Login).Where(e => e.Login.UserId.Equals(x.Trim())).FirstOrDefault();
                                    //var LoginUser = db.Employee.Where(e => e.Login.UserId.Equals(x.Trim())).FirstOrDefault(); 
                                  

                                    login_policy = db.PasswordPolicy.Include(e => e.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO").FirstOrDefault();
                                   
                                    if (LoginUser != null)
                                    {
                                        SessionManager.UserName = SessionManager.EmpId = LoginUser.Id.ToString();
                                    }
                                    if (login_policy == null)
                                    {
                                        SessionManager.UserName = "admin";
                                        if (db.Company.Count() == 1)
                                        {
                                            Session["CompId"] = SessionManager.CompanyId = db.Company.Select(e => e.Id.ToString()).FirstOrDefault();
                                        }
                                        TempData["Firsttimeloginkey"] = "Firsttimelogin";
                                        return Json(new { success = true, mod = 0, data = Url.Action("index", "PasswordPolicy") }, JsonRequestBehavior.AllowGet);
                                    }

                                    string loginPwd = "";
                                    if (LoginUser != null && LoginUser.Login != null)
                                    {
                                        loginPwd = login_policy.AllowEncryption == true ? P2BSecurity.Decrypt(LoginUser.Login.Password) : LoginUser.Login.Password;
                                    }

                                    if (LoginUser != null && LoginUser.Login != null && y != loginPwd)
                                    {
                                        if (!string.IsNullOrEmpty(SessionManager.EmpId))
                                        {
                                            SessionManager.UserName = SessionManager.EmpId = LoginUser.Id.ToString();
                                            CreateLogRegisterEntry(Convert.ToInt32(SessionManager.EmpId), 1);
                                            var EmpId = Convert.ToInt32(SessionManager.EmpId);
                                            oLogRegister = db.Employee
                                               .Include(e => e.Login)
                                               .Include(e => e.LogRegister)
                                               .Where(e => e.Id == EmpId)
                                               .SingleOrDefault();
                                            var Last_oLogRegister = oLogRegister.LogRegister.LastOrDefault();
                                            if (Last_oLogRegister.LogInAttempt > 3)
                                            {
                                                Last_oLogRegister.Lock = true;
                                                oLogRegister.Login.IsActive = false;

                                                db.Employee.Attach(oLogRegister);
                                                db.Entry(oLogRegister).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                return Json(new { success = false, data = "Account is Locked..!" }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                        return Json(new { success = false, data = "UserId or PassWord Wrong..!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    else if (LoginUser != null && LoginUser.Login != null && LoginUser.Login.IsActive == false)
                                    {
                                        return Json(new { success = false, data = "Account is Locked..!" }, JsonRequestBehavior.AllowGet);
                                    }
                                  

                                    if (LoginUser != null)
                                    {
                                        SessionManager.UserName = SessionManager.EmpId = LoginUser.Id.ToString();
                                        //SessionManager.EmpLvId = db.EmployeeLeave.Where(e => e.Employee != null && e.Employee.Id == LoginUser.Id).Select(e => e.Id).FirstOrDefault().ToString();
                                        SessionManager.EmpLvId = db.EmployeeLeave.Find(LoginUser.Id).Id.ToString();
                                        var id = Convert.ToInt32(SessionManager.UserName);
                                        // var EmpPayrollData = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == id).FirstOrDefault();
                                        var EmpPayrollData = db.EmployeePayroll.Where(e => e.Employee.Id == id).FirstOrDefault();
                                        EmpPayrollData.Employee = db.Employee.Find(EmpPayrollData.Employee_Id);
                                        //   var Comp = db.CompanyPayroll.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll.Any(a => a.Id == EmpPayrollData.Id)).Select(e => e.Company).ToList();
                                        var Comp = db.CompanyPayroll.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll.Any(a => a.Id == EmpPayrollData.Id)).Select(e => e.Company).ToList();
                                        if (Comp != null && Comp.Count > 0)
                                        {
                                            Session["CompId"] = SessionManager.CompanyId = Comp.Select(e => e.Id.ToString()).FirstOrDefault();
                                        }
                                    }
                                    else
                                    {
                                        SessionManager.UserName = "admin";
                                    }
                                    if (db.Company.Count() == 0)
                                    {
                                        return Json(new { success = true, mod = 0, data = Url.Action("index", "InitialCompanyCreate") }, JsonRequestBehavior.AllowGet);
                                    }
                                    else if (db.Company.Count() == 1)
                                    {
                                        //employee is not assign but comp create
                                        Session["CompId"] = SessionManager.CompanyId = db.Company.Select(e => e.Id.ToString()).FirstOrDefault();
                                    }
                                    else
                                    {
                                        //Company Selection
                                    }
                                    //check password policy define
                                    var checkpass = db.PasswordPolicy.ToList();
                                    if (checkpass.Count() == 0)
                                    {
                                        TempData["Firsttimeloginkey"] = "Firsttimelogin";
                                        return Json(new { success = true, mod = 0, data = Url.Action("index", "PasswordPolicy") }, JsonRequestBehavior.AllowGet);
                                    }
                                    if (login_policy == null)
                                    {
                                        return Json(new { success = false, data = "Password policy not assign.." }, JsonRequestBehavior.AllowGet);
                                    }
                                    if (!string.IsNullOrEmpty(SessionManager.CompanyId) && SessionManager.CompanyId != "0")
                                    {
                                        var id = Convert.ToInt32(Session["CompId"].ToString());
                                        //var fyear = db.Company.Include(e => e.Calendar).Include(e => e.Calendar.Select(a => a.Name)).Where(e => e.Id == id).FirstOrDefault();
                                        //if (fyear != null)
                                        //{
                                        //    //SessionManager.FinancialYear = fyear.Calendar.Where(e => e.Name != null && e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).Select(e => e.Id).SingleOrDefault().ToString();
                                        //}
                                        //SessionManager.CompPayrollId = db.CompanyPayroll.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        //SessionManager.CompLvId = db.CompanyLeave.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        //SessionManager.CompTrainingId = db.CompanyTraining.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        //SessionManager.CompRecruitId = db.CompanyRecruitment.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        //SessionManager.CompApprId = db.CompanyAppraisal.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        //SessionManager.CompAttId = db.CompanyAttendance.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        //SessionManager.CompExitId = db.CompanyExit.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();

                                        SessionManager.CompPayrollId = db.CompanyPayroll.Where(e => e.Company != null && e.Company_Id == id).FirstOrDefault().Id.ToString();
                                        SessionManager.CompLvId = db.CompanyLeave.Where(e => e.Company != null && e.Company_Id == id).FirstOrDefault().Id.ToString();
                                        SessionManager.CompTrainingId = db.CompanyTraining.Where(e => e.Company != null && e.Company.Id == id).FirstOrDefault().Id.ToString();
                                        SessionManager.CompRecruitId = db.CompanyRecruitment.Where(e => e.Company != null && e.Company.Id == id).FirstOrDefault().Id.ToString();
                                        SessionManager.CompApprId = db.CompanyAppraisal.Where(e => e.Company != null && e.Company.Id == id).FirstOrDefault().Id.ToString();
                                        SessionManager.CompAttId = db.CompanyAttendance.Where(e => e.Company != null && e.Company.Id == id).FirstOrDefault().Id.ToString();
                                        SessionManager.CompExitId = db.CompanyExit.Where(e => e.Company != null && e.Company.Id == id).FirstOrDefault().Id.ToString();

                                    }
                                    var AdminDataChk = db.Employee.Include(e => e.Login).Where(e => e.Login != null && e.Login.UserId == "admin").FirstOrDefault();
                                    if (AdminDataChk == null && db.Employee.ToList().Count > 0)
                                    {
                                        return Json(new { success = true, mod = 0, data = Url.Action("Partial", "InitialAdminAssignment") }, JsonRequestBehavior.AllowGet);
                                    }
                                    if (db.PasswordPolicy.ToList().Count == 0)
                                    {
                                        CreatePassWordPolicy();
                                    }

                                    int Compid = Convert.ToInt32(Session["CompId"].ToString());
                                    if (!db.CompanyTraining.Any(e => e.Company.Id == Compid))
                                    {
                                        var oCompanyTraining = new CompanyTraining();
                                        oCompanyTraining.Company = db.Company.Find(Compid);
                                        oCompanyTraining.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.CompanyTraining.Add(oCompanyTraining);
                                        db.SaveChanges();
                                    }
                                    if (!db.CompanyRecruitment.Any(e => e.Company.Id == Compid))
                                    {
                                        var oCompanyRecruitment = new CompanyRecruitment();
                                        oCompanyRecruitment.Company = db.Company.Find(Compid);
                                        oCompanyRecruitment.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.CompanyRecruitment.Add(oCompanyRecruitment);
                                        db.SaveChanges();
                                    }
                                    if (!db.CompanyAttendance.Any(e => e.Company.Id == Compid))
                                    {
                                        var oCompanyAttendance = new CompanyAttendance();
                                        oCompanyAttendance.Company = db.Company.Find(Compid);
                                        oCompanyAttendance.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.CompanyAttendance.Add(oCompanyAttendance);
                                        db.SaveChanges();
                                    }
                                    if (!db.CompanyAppraisal.Any(e => e.Company.Id == Compid))
                                    {
                                        var oCompanyAppraisal = new CompanyAppraisal();
                                        oCompanyAppraisal.Company = db.Company.Find(Compid);
                                        oCompanyAppraisal.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.CompanyAppraisal.Add(oCompanyAppraisal);
                                        db.SaveChanges();
                                    }
                                    if (!db.CompanyExit.Any(e => e.Company.Id == Compid))
                                    {
                                        var oCompanyExit = new CompanyExit();
                                        oCompanyExit.Company = db.Company.Find(Compid);
                                        oCompanyExit.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.CompanyExit.Add(oCompanyExit);
                                        db.SaveChanges();
                                    }
                                    if (!db.CompanyPFTrust.Any(e => e.Company.Id == Compid))
                                    {
                                        var oCompanyPFTrust = new CompanyPFTrust();
                                        oCompanyPFTrust.Company = db.Company.Find(Compid);
                                        oCompanyPFTrust.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.CompanyPFTrust.Add(oCompanyPFTrust);
                                        db.SaveChanges();
                                    }
                                    CreateLogRegisterEntry(Convert.ToInt32(SessionManager.EmpId), 0);
                                    return Json(new { success = true, mod = 1, data = Url.Action("index", "dashboard") }, JsonRequestBehavior.AllowGet);
                                }
                                #endregion UserId == admin

                                else
                                {

                                    /*
                                     *IsUltimateAppl 0-access main/ultimate added from main
                                     *IsUltimateAppl 1-access only ultimate added from ultimate                                 
                                     */
                                    string loginPwd = login_policy.AllowEncryption == true ? P2BSecurity.Encrypt("p2b@1234") : "p2b@1234";
                                    Login L = new Login();
                                    L.UserId = "admin";
                                    L.Password = loginPwd;
                                    L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    L.IsUltimateAppl = false;
                                    L.IsUltimateHOAppl = true;
                                    L.IsActive = true;
                                    db.Login.Add(L);
                                    db.SaveChanges();

                                    List<LookupValue> LKValList = new List<LookupValue>();
                                    DBTrack dbT = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    if (db.Lookup.Count() == 0)
                                    {
                                        LookupValue LKVal = new LookupValue();
                                        LKVal.LookupVal = "FinancialYear";
                                        LKVal.IsActive = true;
                                        LKVal.DeleteValue = false;
                                        LKValList.Add(LKVal);
                                        LKVal.DBTrack = dbT;

                                        Lookup LK = new Lookup();
                                        LK.Code = "500";
                                        LK.Name = "Calendar";
                                        LK.LookupValues = LKValList;
                                        LK.DBTrack = dbT;
                                        db.Lookup.Add(LK);
                                        db.SaveChanges();
                                    }
                                    // Application UI

                                    List<LookupValue> LKValListU = new List<LookupValue>();
                                    DBTrack dbTU = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    if (db.Lookup.Where(e=>e.Code=="595").Count() == 0)
                                    {
                                        LookupValue LKVal = new LookupValue();
                                        LKVal.LookupVal = "UltimateHo";
                                        LKVal.IsActive = true;
                                        LKVal.DeleteValue = false;
                                        LKVal.DBTrack = dbT;
                                        LKValListU.Add(LKVal);

                                        LookupValue LKValE = new LookupValue();
                                        LKValE.LookupVal = "ESS";
                                        LKValE.IsActive = true;
                                        LKValE.DeleteValue = false;
                                        LKValE.DBTrack = dbT;
                                        LKValListU.Add(LKValE);

                                        LookupValue LKValM = new LookupValue();
                                        LKValM.LookupVal = "Mobile";
                                        LKValM.IsActive = true;
                                        LKValM.DeleteValue = false;
                                        LKValM.DBTrack = dbT;
                                        LKValListU.Add(LKValM);
                                       

                                        Lookup LK = new Lookup();
                                        LK.Code = "595";
                                        LK.Name = "PasswordPolicyModulewise";
                                        LK.LookupValues = LKValListU;
                                        LK.DBTrack = dbT;
                                        db.Lookup.Add(LK);
                                        db.SaveChanges();
                                    }

                                    var LoginUser = db.Employee.Include(e => e.Login).Where(e => e.Login.UserId.Equals(x.Trim())).FirstOrDefault();
                                    if (LoginUser != null)
                                    {
                                        SessionManager.UserName = SessionManager.EmpId = LoginUser.Id.ToString();
                                    }
                                    else
                                    {
                                        SessionManager.UserName = "admin";
                                    }
                                    if (LoginUser != null)
                                    {
                                        SessionManager.UserName = SessionManager.EmpId = LoginUser.Id.ToString();
                                        SessionManager.EmpLvId = db.EmployeeLeave.Where(e => e.Employee == LoginUser).Select(e => e.Id).FirstOrDefault().ToString();
                                        var id = Convert.ToInt32(SessionManager.UserName);
                                        var EmpPayrollData = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == id).FirstOrDefault();
                                        var Comp = db.CompanyPayroll.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll.Any(a => a.Id == EmpPayrollData.Id)).Select(e => e.Company).ToList();
                                        if (Comp != null && Comp.Count > 0)
                                        {
                                            Session["CompId"] = SessionManager.CompanyId = Comp.Select(e => e.Id.ToString()).FirstOrDefault();
                                        }

                                    }
                                    if (db.Company.Count() == 0)
                                    {
                                        return Json(new { success = true, mod = 0, data = Url.Action("index", "InitialCompanyCreate") }, JsonRequestBehavior.AllowGet);
                                    }
                                    else if (db.Company.Count() == 1)
                                    {
                                        //employee is not assign but comp create
                                        Session["CompId"] = SessionManager.CompanyId = db.Company.Select(e => e.Id.ToString()).FirstOrDefault();
                                    }
                                    if (!string.IsNullOrEmpty(SessionManager.CompanyId) && SessionManager.CompanyId != "0")
                                    {
                                        var id = Convert.ToInt32(Session["CompId"].ToString());
                                        var fif = db.Company.Include(e => e.Calendar).Include(e => e.Calendar.Select(a => a.Name)).Where(e => e.Id == id).FirstOrDefault();
                                        if (fif != null)
                                        {
                                            SessionManager.FinancialYear = fif.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).Select(e => e.Id).FirstOrDefault().ToString();
                                        }
                                        SessionManager.CompPayrollId = db.CompanyPayroll.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        SessionManager.CompLvId = db.CompanyLeave.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        SessionManager.CompTrainingId = db.CompanyTraining.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        SessionManager.CompRecruitId = db.CompanyRecruitment.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        SessionManager.CompApprId = db.CompanyAppraisal.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        SessionManager.CompAttId = db.CompanyAttendance.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        SessionManager.CompExitId = db.CompanyExit.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                    }


                                    /*
                                     Log Register entry
                                     */

                                    return Json(new { success = true, mod = 1, data = Url.Action("index", "dashboard") }, JsonRequestBehavior.AllowGet);

                                }

                            }
                            #endregion If Database IsExists


                            else
                            {
                                try
                                {
                                    //create db only when this credential

                                    if (x == "admin" && y == "p2b@1234")
                                    {
                                        db.Database.Create();
                                        return Json(new { success = false, data = "DataBase Created..!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        return Json(new { success = false, data = "UserName or PassWord Wrong..!" }, JsonRequestBehavior.AllowGet);
                                    }

                                }
                                catch (Exception e)
                                {
                                    return Json(new { success = false, data = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        #endregion If User IsAdmin
                        //User Exitance check
                       
                        PasswordPolicy login_policyu = new PasswordPolicy();
                        login_policyu = db.PasswordPolicy.Include(e => e.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO").FirstOrDefault();
                                  
                        var EmpData = db.Employee.Include(e => e.Login).Include(e => e.LogRegister).Where(e => e.Login.UserId == x.Trim()).FirstOrDefault();
                        if (login_policyu==null)
                        {
                             return Json(new { success = false, data = "Password policy not assign.." }, JsonRequestBehavior.AllowGet);
                        }
                        if (EmpData.Login.IsUltimateHOAppl == false)
                        {
                            return Json(new { success = false, data = "Not Permission for Utimate.." }, JsonRequestBehavior.AllowGet);
                        }
                        if (EmpData != null)
                        {
                            SessionManager.UserName = SessionManager.EmpId = EmpData.Id.ToString();
                            SessionManager.EmpLvId = db.EmployeeLeave.Where(e => e.Employee.Id == EmpData.Id).Select(e => e.Id).FirstOrDefault().ToString();
                            var id = Convert.ToInt32(SessionManager.UserName);
                            var EmpPayrollData = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == id).FirstOrDefault();
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
                            var fif = db.Company.Include(e => e.Calendar).Include(e => e.Calendar.Select(a => a.Name)).Where(e => e.Id == id).FirstOrDefault();
                            if (fif != null)
                            {
                                SessionManager.FinancialYear = fif.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).Select(e => e.Id).FirstOrDefault().ToString();
                            }
                            SessionManager.CompPayrollId = db.CompanyPayroll.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                            SessionManager.CompLvId = db.CompanyLeave.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                            SessionManager.CompTrainingId = db.CompanyTraining.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                            SessionManager.CompRecruitId = db.CompanyRecruitment.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                            SessionManager.CompApprId = db.CompanyAppraisal.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                            SessionManager.CompAttId = db.CompanyAttendance.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                            SessionManager.CompExitId = db.CompanyExit.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                        }


                        Leave.MainController.LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy> responseDeserializeData = new Leave.MainController.LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy>();
                        int? errorno = 0;
                        int? Infono = 0;
                        var ShowMessage = "";

                        string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                        using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                        {
                            var response = p2BHttpClient.request("Global/getLoginPolicy",
                                new CORE_LoginPolicy()
                                {
                                    ApplicationUI = login_policyu.ApplicationUI.LookupVal.ToUpper(),
                                    LoginPolicy_Id = login_policyu.Id,
                                    ProcessType = "LOGINCHECK",
                                    UserId = EmpData.EmpCode.ToString(),
                                    Password = y,
                                    Employee_Id = EmpData.Id,
                                    GUID = EmpData.Login.Guid
                                });

                            responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<Leave.MainController.LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy>>(response.Content.ReadAsStringAsync().Result);
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

                            if (errorno == 0 && (Infono == 0 || Infono==null))
                            {
                                return Json(new { success = true, mod = 1, data = Url.Action("index", "dashboard") }, JsonRequestBehavior.AllowGet);
                            }

                            if (Infono != 0)
                            {
                               
                                if (Infono==2 && ShowMessage=="Change Default Password")
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

                           

                        }

                        // In API CHECK
                        //if (!db.Login.Any(e => e.UserId == x.Trim()))
                        //{
                        //    return Json(new { success = false, data = "User Not Exits..!" }, JsonRequestBehavior.AllowGet);
                        //}
                        ////GetEmpData
                        //var EmpData = db.Employee.Include(e => e.Login).Include(e => e.LogRegister).Where(e => e.Login.UserId == x.Trim()).FirstOrDefault();
                        //if (EmpData == null)
                        //{
                        //    return Json(new { success = false, data = "Invalid Account..!" }, JsonRequestBehavior.AllowGet);
                        //}
                        //var Retiredemp = db.Employee.Include(e => e.Login).Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.ServiceLastDate != null && e.Login.UserId == x.Trim()).FirstOrDefault();
                        //if (Retiredemp != null)
                        //{
                        //    return Json(new { success = false, data = "Retired Employee Cannot Login....!" }, JsonRequestBehavior.AllowGet);
                        //}
                        //var ChkRetiredate = db.Employee.Include(e => e.Login).Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.RetirementDate != null && e.Login.UserId == x.Trim() && e.ServiceBookDates.RetirementDate < DateTime.Now).FirstOrDefault();
                        //if (ChkRetiredate != null)
                        //{
                        //    return Json(new { success = false, data = "Retired Employee Cannot Login....!" }, JsonRequestBehavior.AllowGet);
                        //}
                        //var ChkSuspend = db.Employee.Include(e => e.Login).Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.RetirementDate != null && e.Login.UserId == x.Trim() && e.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() == "SUSPEND").FirstOrDefault();
                        //if (ChkSuspend != null)
                        //{
                        //    return Json(new { success = false, data = "Suspended Employee Cannot Login....!" }, JsonRequestBehavior.AllowGet);
                        //}
                        ///*
                        // * User Access Check
                        // * IsUltimateAppl==0 Can only access main
                        // */

                        //if (EmpData.Login.IsUltimateAppl != false)
                        //{
                        //    return Json(new { success = false, data = "Invalid Account..!" }, JsonRequestBehavior.AllowGet);
                        //}
                        //LogRegister log_reg = new LogRegister();
                        //log_reg.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        //var log = EmpData.LogRegister.LastOrDefault();
                        //if (EmpData != null && EmpData.Login.IsActive == false)
                        //{
                        //    return Json(new { success = false, data = "Account is Locked..!" }, JsonRequestBehavior.AllowGet);
                        //}
                        //int login_hits = log == null ? 1 : log.LogInAttempt;

                        // In API CHECK

                        if (EmpData != null)
                        {
                            return Json(new { success = true, mod = 1, data = Url.Action("index", "dashboard") }, JsonRequestBehavior.AllowGet);
                            //if (log != null && log.Lock == true)
                            //{
                            //    return Json(new { success = false, data = "Account is Locked..!" }, JsonRequestBehavior.AllowGet);
                            //}

                            // this code above use
                            //if (EmpData != null)
                            //{
                            //    SessionManager.UserName = SessionManager.EmpId = EmpData.Id.ToString();
                            //    SessionManager.EmpLvId = db.EmployeeLeave.Where(e => e.Employee.Id == EmpData.Id).Select(e => e.Id).FirstOrDefault().ToString();
                            //    var id = Convert.ToInt32(SessionManager.UserName);
                            //    var EmpPayrollData = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == id).FirstOrDefault();
                            //    var Comp = db.CompanyPayroll.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll.Any(a => a.Id == EmpPayrollData.Id)).Select(e => e.Company).ToList();
                            //    if (Comp != null && Comp.Count > 0)
                            //    {
                            //        Session["CompId"] = SessionManager.CompanyId = Comp.Select(e => e.Id.ToString()).FirstOrDefault();
                            //    }
                            //}
                            //else
                            //{
                            //    SessionManager.UserName = EmpData.Login.UserId;
                            //}
                            //if (!string.IsNullOrEmpty(SessionManager.CompanyId) && SessionManager.CompanyId != "0")
                            //{
                            //    var id = Convert.ToInt32(Session["CompId"].ToString());
                            //    var fif = db.Company.Include(e => e.Calendar).Include(e => e.Calendar.Select(a => a.Name)).Where(e => e.Id == id).FirstOrDefault();
                            //    if (fif != null)
                            //    {
                            //        SessionManager.FinancialYear = fif.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).Select(e => e.Id).FirstOrDefault().ToString();
                            //    }
                            //    SessionManager.CompPayrollId = db.CompanyPayroll.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                            //    SessionManager.CompLvId = db.CompanyLeave.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                            //    SessionManager.CompTrainingId = db.CompanyTraining.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                            //    SessionManager.CompRecruitId = db.CompanyRecruitment.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                            //    SessionManager.CompApprId = db.CompanyAppraisal.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                            //    SessionManager.CompAttId = db.CompanyAttendance.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                            //    SessionManager.CompExitId = db.CompanyExit.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                            //}
                            // this code above use

                            // In API CHECK
                            //var CompIdc = Convert.ToInt32(SessionManager.CompanyId);
                            //var login_policylistc = db.Company.Include(e => e.PasswordPolicy)
                            //    .Include(e => e.PasswordPolicy.Select(z => z.ApplicationUI))
                            //    .Where(e => e.Id == CompIdc).SingleOrDefault();

                            //var login_policyc = login_policylistc.PasswordPolicy.Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO").SingleOrDefault();



                            //if ((login_policyc.AllowEncryption == true ? P2BSecurity.Decrypt(EmpData.Login.Password) : EmpData.Login.Password) == y.Trim())
                            //{


                            //    using (TransactionScope ts = new TransactionScope())
                            //    {
                            //        log_reg.LogInAttempt = 1;
                            //        log_reg.LogInDate = DateTime.Now;
                            //        log_reg.LogInTime = DateTime.Now.TimeOfDay.ToString();
                            //        log_reg.LogInDate = DateTime.Now;
                            //        log_reg.LogInTime = DateTime.Now.TimeOfDay.ToString();
                            //        log_reg.IP = Dns.GetHostByName(Dns.GetHostName().ToString()).AddressList[0].ToString();
                            //        log_reg.ComputerName = Dns.GetHostName().ToString();
                            //        log_reg.Browser = HttpContext.Request.Browser.Browser;
                            //        EmpData.LogRegister.Add(log_reg);
                            //        db.SaveChanges();
                            //        ts.Complete();
                            //    }

                            //    //if username and password are same consider the user is 1st loged in ,and force to change password
                            //    if (EmpData.Login.UserId == (login_policyc.AllowEncryption == true ? P2BSecurity.Decrypt(EmpData.Login.Password) : EmpData.Login.Password))
                            //    {
                            //        return Json(new { success = true, mod = 1, data = Url.Action("Index", "welcomescreen") }, JsonRequestBehavior.AllowGet);
                            //    }
                            //    else
                            //    {
                            //        return Json(new { success = true, mod = 1, data = Url.Action("index", "dashboard") }, JsonRequestBehavior.AllowGet);
                            //    }
                            //}
                            //else
                            //{
                            //    var CompId = Convert.ToInt32(SessionManager.CompanyId);
                            //    var login_policylist = db.Company.Include(e => e.PasswordPolicy)
                            //        .Include(e => e.PasswordPolicy.Select(z => z.ApplicationUI))
                            //        .Where(e => e.Id == CompId && e.PasswordPolicy != null).OrderByDescending(e => e.Id).FirstOrDefault();
                            //    var login_policy = login_policylist.PasswordPolicy.Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO").SingleOrDefault();
                            //    if (login_policy.AllowAttempt < login_hits)
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
                            return Json(new { success = false, data = "User Name Not Exist..!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    #endregion Normal Login

                    else
                    {
                       

                        #region SAML Login
                        #region If User IsAdmin
                        if (x == null)
                        {

                            #region If Database IsExists
                            if (db.Database.Exists())
                            {
                                db.Database.Connection.Open();
                                PasswordPolicy login_policy = new PasswordPolicy();
                                var oLogRegister = new Employee();
                                var CompId = 0;

                                #region UserId == admin for SAML

                                // string SMMLloginuser = xmlResponse.Assertion.AttributeStatement[3].AttributeValue.ToString();
                                //string SMMLloginuser = xmlResponse.Assertion.AttributeStatement.ToList()
                                //    .Where(z => z.Name.Equals("EmployeeID"))
                                //    .Select(s => s.AttributeValue).FirstOrDefault();
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
                                    //var LoginUser = db.Employee.Where(e => e.Login.UserId.Equals(x.Trim())).FirstOrDefault(); 

                                    login_policy = db.PasswordPolicy.FirstOrDefault();
                                    string loginPwd = "";
                                    if (LoginUser != null && LoginUser.Login != null)
                                    {
                                        loginPwd = login_policy.AllowEncryption == true ? P2BSecurity.Decrypt(LoginUser.Login.Password) : LoginUser.Login.Password;
                                    }

                                    if (LoginUser != null && LoginUser.Login != null && getdbUserID.UserId != LoginUser.Login.UserId)
                                    {
                                        if (!string.IsNullOrEmpty(SessionManager.EmpId))
                                        {
                                            SessionManager.UserName = SessionManager.EmpId = LoginUser.Id.ToString();
                                            CreateLogRegisterEntry(Convert.ToInt32(SessionManager.EmpId), 1);
                                            var EmpId = Convert.ToInt32(SessionManager.EmpId);
                                            oLogRegister = db.Employee
                                               .Include(e => e.Login)
                                               .Include(e => e.LogRegister)
                                               .Where(e => e.Id == EmpId)
                                               .SingleOrDefault();
                                            var Last_oLogRegister = oLogRegister.LogRegister.LastOrDefault();
                                            if (Last_oLogRegister.LogInAttempt > 3)
                                            {
                                                Last_oLogRegister.Lock = true;
                                                oLogRegister.Login.IsActive = false;

                                                db.Employee.Attach(oLogRegister);
                                                db.Entry(oLogRegister).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                return Json(new { success = false, data = "Account is Locked..!" }, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                        return Json(new { success = false, data = "UserId or PassWord Wrong..!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    else if (LoginUser != null && LoginUser.Login != null && LoginUser.Login.IsActive == false)
                                    {
                                        return Json(new { success = false, data = "Account is Locked..!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    if (LoginUser != null)
                                    {
                                        SessionManager.UserName = SessionManager.EmpId = LoginUser.Id.ToString();
                                        //SessionManager.EmpLvId = db.EmployeeLeave.Where(e => e.Employee != null && e.Employee.Id == LoginUser.Id).Select(e => e.Id).FirstOrDefault().ToString();
                                        SessionManager.EmpLvId = db.EmployeeLeave.Find(LoginUser.Id).Id.ToString();
                                        var id = Convert.ToInt32(SessionManager.UserName);
                                        // var EmpPayrollData = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == id).FirstOrDefault();
                                        var EmpPayrollData = db.EmployeePayroll.Where(e => e.Employee.Id == id).FirstOrDefault();
                                        EmpPayrollData.Employee = db.Employee.Find(EmpPayrollData.Employee_Id);
                                        //   var Comp = db.CompanyPayroll.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll.Any(a => a.Id == EmpPayrollData.Id)).Select(e => e.Company).ToList();
                                        var Comp = db.CompanyPayroll.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll.Any(a => a.Id == EmpPayrollData.Id)).Select(e => e.Company).ToList();
                                        if (Comp != null && Comp.Count > 0)
                                        {
                                            Session["CompId"] = SessionManager.CompanyId = Comp.Select(e => e.Id.ToString()).FirstOrDefault();
                                        }
                                    }
                                    else
                                    {
                                        SessionManager.UserName = getdbUserID.UserId;
                                    }
                                    if (db.Company.Count() == 0)
                                    {
                                        return Json(new { success = true, mod = 0, data = Url.Action("index", "InitialCompanyCreate") }, JsonRequestBehavior.AllowGet);
                                    }
                                    else if (db.Company.Count() == 1)
                                    {
                                        //employee is not assign but comp create
                                        Session["CompId"] = SessionManager.CompanyId = db.Company.Select(e => e.Id.ToString()).FirstOrDefault();
                                    }
                                    else
                                    {
                                        //Company Selection
                                    }
                                    if (!string.IsNullOrEmpty(SessionManager.CompanyId) && SessionManager.CompanyId != "0")
                                    {
                                        var id = Convert.ToInt32(Session["CompId"].ToString());
                                        //var fyear = db.Company.Include(e => e.Calendar).Include(e => e.Calendar.Select(a => a.Name)).Where(e => e.Id == id).FirstOrDefault();
                                        //if (fyear != null)
                                        //{
                                        //    //SessionManager.FinancialYear = fyear.Calendar.Where(e => e.Name != null && e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).Select(e => e.Id).SingleOrDefault().ToString();
                                        //}
                                        //SessionManager.CompPayrollId = db.CompanyPayroll.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        //SessionManager.CompLvId = db.CompanyLeave.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        //SessionManager.CompTrainingId = db.CompanyTraining.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        //SessionManager.CompRecruitId = db.CompanyRecruitment.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        //SessionManager.CompApprId = db.CompanyAppraisal.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        //SessionManager.CompAttId = db.CompanyAttendance.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        //SessionManager.CompExitId = db.CompanyExit.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();

                                        SessionManager.CompPayrollId = db.CompanyPayroll.Where(e => e.Company != null && e.Company_Id == id).FirstOrDefault().Id.ToString();
                                        SessionManager.CompLvId = db.CompanyLeave.Where(e => e.Company != null && e.Company_Id == id).FirstOrDefault().Id.ToString();
                                        SessionManager.CompTrainingId = db.CompanyTraining.Where(e => e.Company != null && e.Company.Id == id).FirstOrDefault().Id.ToString();
                                        SessionManager.CompRecruitId = db.CompanyRecruitment.Where(e => e.Company != null && e.Company.Id == id).FirstOrDefault().Id.ToString();
                                        SessionManager.CompApprId = db.CompanyAppraisal.Where(e => e.Company != null && e.Company.Id == id).FirstOrDefault().Id.ToString();
                                        SessionManager.CompAttId = db.CompanyAttendance.Where(e => e.Company != null && e.Company.Id == id).FirstOrDefault().Id.ToString();
                                        SessionManager.CompExitId = db.CompanyExit.Where(e => e.Company != null && e.Company.Id == id).FirstOrDefault().Id.ToString();
                                    }
                                    var AdminDataChk = db.Employee.Include(e => e.Login).Where(e => e.Login != null && e.Login.UserId == getdbUserID.UserId).FirstOrDefault();
                                    if (AdminDataChk == null && db.Employee.ToList().Count > 0)
                                    {
                                        return Json(new { success = true, mod = 0, data = Url.Action("Partial", "InitialAdminAssignment") }, JsonRequestBehavior.AllowGet);
                                    }
                                    if (db.PasswordPolicy.ToList().Count == 0)
                                    {
                                        CreatePassWordPolicy();
                                    }

                                    int Compid = Convert.ToInt32(Session["CompId"].ToString());
                                    if (!db.CompanyTraining.Any(e => e.Company.Id == Compid))
                                    {
                                        var oCompanyTraining = new CompanyTraining();
                                        oCompanyTraining.Company = db.Company.Find(Compid);
                                        oCompanyTraining.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.CompanyTraining.Add(oCompanyTraining);
                                        db.SaveChanges();
                                    }
                                    if (!db.CompanyRecruitment.Any(e => e.Company.Id == Compid))
                                    {
                                        var oCompanyRecruitment = new CompanyRecruitment();
                                        oCompanyRecruitment.Company = db.Company.Find(Compid);
                                        oCompanyRecruitment.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.CompanyRecruitment.Add(oCompanyRecruitment);
                                        db.SaveChanges();
                                    }
                                    if (!db.CompanyAttendance.Any(e => e.Company.Id == Compid))
                                    {
                                        var oCompanyAttendance = new CompanyAttendance();
                                        oCompanyAttendance.Company = db.Company.Find(Compid);
                                        oCompanyAttendance.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.CompanyAttendance.Add(oCompanyAttendance);
                                        db.SaveChanges();
                                    }
                                    if (!db.CompanyAppraisal.Any(e => e.Company.Id == Compid))
                                    {
                                        var oCompanyAppraisal = new CompanyAppraisal();
                                        oCompanyAppraisal.Company = db.Company.Find(Compid);
                                        oCompanyAppraisal.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.CompanyAppraisal.Add(oCompanyAppraisal);
                                        db.SaveChanges();
                                    }
                                    if (!db.CompanyExit.Any(e => e.Company.Id == Compid))
                                    {
                                        var oCompanyExit = new CompanyExit();
                                        oCompanyExit.Company = db.Company.Find(Compid);
                                        oCompanyExit.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        db.CompanyExit.Add(oCompanyExit);
                                        db.SaveChanges();
                                    }

                                    CreateLogRegisterEntry(Convert.ToInt32(SessionManager.EmpId), 0);
                                    return RedirectToAction("index", "dashboard");
                                    //return Json(new { success = true, mod = 1, data = Url.Action("index", "dashboard") }, JsonRequestBehavior.AllowGet);
                                }
                                #endregion UserId == admin

                                else
                                {

                                    /*
                                     *IsUltimateAppl 0-access main/ultimate added from main
                                     *IsUltimateAppl 1-access only ultimate added from ultimate                                 
                                     */
                                    string loginPwd = login_policy.AllowEncryption == true ? P2BSecurity.Decrypt("p2b@1234") : "p2b@1234";
                                    Login L = new Login();
                                    L.UserId = getdbUserID.UserId;
                                    L.Password = loginPwd;
                                    L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    L.IsUltimateAppl = false;
                                    L.IsActive = true;
                                    db.Login.Add(L);
                                    db.SaveChanges();

                                    List<LookupValue> LKValList = new List<LookupValue>();
                                    DBTrack dbT = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    if (db.Lookup.Count() == 0)
                                    {
                                        LookupValue LKVal = new LookupValue();
                                        LKVal.LookupVal = "FinancialYear";
                                        LKVal.IsActive = true;
                                        LKVal.DeleteValue = false;
                                        LKValList.Add(LKVal);
                                        LKVal.DBTrack = dbT;

                                        Lookup LK = new Lookup();
                                        LK.Code = "500";
                                        LK.Name = "Calendar";
                                        LK.LookupValues = LKValList;
                                        LK.DBTrack = dbT;
                                        db.Lookup.Add(LK);
                                        db.SaveChanges();
                                    }

                                    var LoginUser = db.Employee.Include(e => e.Login).Where(e => e.Login.UserId.Equals(x.Trim())).FirstOrDefault();
                                    if (LoginUser != null)
                                    {
                                        SessionManager.UserName = SessionManager.EmpId = LoginUser.Id.ToString();
                                    }
                                    else
                                    {
                                        SessionManager.UserName = getdbUserID.UserId;
                                    }
                                    if (LoginUser != null)
                                    {
                                        SessionManager.UserName = SessionManager.EmpId = LoginUser.Id.ToString();
                                        SessionManager.EmpLvId = db.EmployeeLeave.Where(e => e.Employee == LoginUser).Select(e => e.Id).FirstOrDefault().ToString();
                                        var id = Convert.ToInt32(SessionManager.UserName);
                                        var EmpPayrollData = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == id).FirstOrDefault();
                                        var Comp = db.CompanyPayroll.Include(e => e.EmployeePayroll).Where(e => e.EmployeePayroll.Any(a => a.Id == EmpPayrollData.Id)).Select(e => e.Company).ToList();
                                        if (Comp != null && Comp.Count > 0)
                                        {
                                            Session["CompId"] = SessionManager.CompanyId = Comp.Select(e => e.Id.ToString()).FirstOrDefault();
                                        }

                                    }
                                    if (db.Company.Count() == 0)
                                    {
                                        return Json(new { success = true, mod = 0, data = Url.Action("index", "InitialCompanyCreate") }, JsonRequestBehavior.AllowGet);
                                    }
                                    else if (db.Company.Count() == 1)
                                    {
                                        //employee is not assign but comp create
                                        Session["CompId"] = SessionManager.CompanyId = db.Company.Select(e => e.Id.ToString()).FirstOrDefault();
                                    }
                                    if (!string.IsNullOrEmpty(SessionManager.CompanyId) && SessionManager.CompanyId != "0")
                                    {
                                        var id = Convert.ToInt32(Session["CompId"].ToString());
                                        var fif = db.Company.Include(e => e.Calendar).Include(e => e.Calendar.Select(a => a.Name)).Where(e => e.Id == id).FirstOrDefault();
                                        if (fif != null)
                                        {
                                            SessionManager.FinancialYear = fif.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).Select(e => e.Id).FirstOrDefault().ToString();
                                        }
                                        SessionManager.CompPayrollId = db.CompanyPayroll.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        SessionManager.CompLvId = db.CompanyLeave.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        SessionManager.CompTrainingId = db.CompanyTraining.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        SessionManager.CompRecruitId = db.CompanyRecruitment.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        SessionManager.CompApprId = db.CompanyAppraisal.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        SessionManager.CompAttId = db.CompanyAttendance.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                        SessionManager.CompExitId = db.CompanyExit.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                    }


                                    /*
                                     Log Register entry
                                     */

                                    return Json(new { success = true, mod = 1, data = Url.Action("index", "dashboard") }, JsonRequestBehavior.AllowGet);

                                }

                            }
                            #endregion If Database IsExists


                            else
                            {
                                try
                                {
                                    //create db only when this credential

                                    if (x == "admin" && y == "p2b@1234")
                                    {
                                        db.Database.Create();
                                        return Json(new { success = false, data = "DataBase Created..!" }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        return Json(new { success = false, data = "UserName or PassWord Wrong..!" }, JsonRequestBehavior.AllowGet);
                                    }

                                }
                                catch (Exception e)
                                {
                                    return Json(new { success = false, data = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        #endregion If User IsAdmin
                        //User Exitance check
                        if (!db.Login.Any(e => e.UserId == x.Trim()))
                        {
                            return Json(new { success = false, data = "User Not Exits..!" }, JsonRequestBehavior.AllowGet);
                        }
                        //GetEmpData
                        var EmpData = db.Employee.Include(e => e.Login).Include(e => e.LogRegister).Where(e => e.Login.UserId == x.Trim()).FirstOrDefault();
                        if (EmpData == null)
                        {
                            return Json(new { success = false, data = "Invalid Account..!" }, JsonRequestBehavior.AllowGet);
                        }
                        var Retiredemp = db.Employee.Include(e => e.Login).Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.ServiceLastDate != null && e.Login.UserId == x.Trim()).FirstOrDefault();
                        if (Retiredemp != null)
                        {
                            return Json(new { success = false, data = "Retired Employee Cannot Login....!" }, JsonRequestBehavior.AllowGet);
                        }
                        var ChkRetiredate = db.Employee.Include(e => e.Login).Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.RetirementDate != null && e.Login.UserId == x.Trim() && e.ServiceBookDates.RetirementDate < DateTime.Now).FirstOrDefault();
                        if (ChkRetiredate != null)
                        {
                            return Json(new { success = false, data = "Retired Employee Cannot Login....!" }, JsonRequestBehavior.AllowGet);
                        }
                        var ChkSuspend = db.Employee.Include(e => e.Login).Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.RetirementDate != null && e.Login.UserId == x.Trim() && e.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() == "SUSPEND").FirstOrDefault();
                        if (ChkSuspend != null)
                        {
                            return Json(new { success = false, data = "Suspended Employee Cannot Login....!" }, JsonRequestBehavior.AllowGet);
                        }
                        /*
                         * User Access Check
                         * IsUltimateAppl==0 Can only access main
                         */

                        if (EmpData.Login.IsUltimateAppl != false)
                        {
                            return Json(new { success = false, data = "Invalid Account..!" }, JsonRequestBehavior.AllowGet);
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
                            //if (log != null && log.Lock == true)
                            //{
                            //    return Json(new { success = false, data = "Account is Locked..!" }, JsonRequestBehavior.AllowGet);
                            //}
                            if (EmpData != null)
                            {
                                SessionManager.UserName = SessionManager.EmpId = EmpData.Id.ToString();
                                SessionManager.EmpLvId = db.EmployeeLeave.Where(e => e.Employee.Id == EmpData.Id).Select(e => e.Id).FirstOrDefault().ToString();
                                var id = Convert.ToInt32(SessionManager.UserName);
                                var EmpPayrollData = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == id).FirstOrDefault();
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
                                var fif = db.Company.Include(e => e.Calendar).Include(e => e.Calendar.Select(a => a.Name)).Where(e => e.Id == id).FirstOrDefault();
                                if (fif != null)
                                {
                                    SessionManager.FinancialYear = fif.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).Select(e => e.Id).FirstOrDefault().ToString();
                                }
                                SessionManager.CompPayrollId = db.CompanyPayroll.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                SessionManager.CompLvId = db.CompanyLeave.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                                SessionManager.CompTrainingId = db.CompanyTraining.Where(e => e.Company != null && e.Company.Id == id).Select(e => e.Company.Id.ToString()).FirstOrDefault();
                            }
                            if (EmpData.Login.Password == y.Trim())
                            {


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
                                    db.SaveChanges();
                                    ts.Complete();
                                }

                                //if username and password are same consider the user is 1st loged in ,and force to change password
                                if (EmpData.Login.UserId == EmpData.Login.Password)
                                {
                                    return Json(new { success = true, mod = 1, data = Url.Action("Index", "welcomescreen") }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json(new { success = true, mod = 1, data = Url.Action("index", "dashboard") }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                var CompId = Convert.ToInt32(SessionManager.CompanyId);
                                var login_policylist = db.Company.Include(e => e.PasswordPolicy)
                                    .Include(e => e.PasswordPolicy.Select(z => z.ApplicationUI))
                                    .Where(e => e.Id == CompId && e.PasswordPolicy != null).OrderByDescending(e => e.Id).FirstOrDefault();
                                var login_policy = login_policylist.PasswordPolicy.Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO").SingleOrDefault();
                                if (login_policy.AllowAttempt < login_hits)
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
                            return Json(new { success = false, data = "User Name Not Exist..!" }, JsonRequestBehavior.AllowGet);
                        }
                        #endregion SAML Login


                        return Json(new { success = false, data = "Something Is Wrong..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception e)
            {

                return Json(new { success = false, data = e.Message }, JsonRequestBehavior.AllowGet);
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
                Session.Clear();
                Session.Abandon();
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
        public class GetUserInfo
        {
            public string EmpName { get; set; }
            public string EmpCode { get; set; }
            public string GeoStruct_Company_Name { get; set; }
            public string PayStruct_Grade_Name { get; set; }
        }

        public ActionResult GetUserinfo()
        {
            //var a = Utility.GetUserData();
            GetUserInfo oGetUserDataClass = new GetUserInfo();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (SessionManager.EmpId == "0" || SessionManager.EmpId == "")
                {
                    return null;
                }
                var id = Convert.ToInt32(SessionManager.EmpId);
                var qurey = db.Employee
                    .Where(t => t.Id == id).Select(t => new GetUserInfo
                {
                    EmpCode = t.EmpCode,
                    EmpName = t.EmpName.FName + " " + t.EmpName.LName,
                    GeoStruct_Company_Name = t.GeoStruct != null && t.GeoStruct.Company != null ? t.GeoStruct.Company.Name : null,
                    PayStruct_Grade_Name = t.PayStruct != null && t.PayStruct.Grade != null ? t.PayStruct.Grade.Name : null
                }).SingleOrDefault();
                if (qurey != null)
                {
                    oGetUserDataClass.EmpCode = qurey.EmpCode;
                    oGetUserDataClass.EmpName = qurey.EmpName;
                    oGetUserDataClass.GeoStruct_Company_Name = qurey.GeoStruct_Company_Name;
                    oGetUserDataClass.PayStruct_Grade_Name = qurey.PayStruct_Grade_Name;
                }

                if (qurey != null)
                {
                    return Json(new { success = true, data = oGetUserDataClass }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, data = "" }, JsonRequestBehavior.AllowGet);

                }
            }
        }
        public JsonResult GetRole()
        {
            var a = UserManager.RoleCheck();
            if (a != "0")
            {
                return Json(new { success = true, data = a }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetEmpImage()
        {
            var a = Utility.GetImage("employee");
            if (!string.IsNullOrEmpty(a))
            {
                return Json(new { data = a, status = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetCompImage()
        {
            var a = Utility.GetImage("company");
            if (!string.IsNullOrEmpty(a))
            {
                return Json(new { data = a, status = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CheckSessionExitance()
        {
            if (String.IsNullOrEmpty(SessionManager.UserName))
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult GetEmpProfileImage(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";

                EmployeeDocuments employeedocuments = null;


                int empcode1 = 0;
                if (SessionManager.EmpId != null)
                {
                    empcode1 = Convert.ToInt32(SessionManager.EmpId);
                    //var employeeid = db.Employee.Include(e => e.EmployeeDocuments).Include(e => e.EmployeeDocuments.Select(t => t.DocumentType)).Where(e => e.Id == empcode1).FirstOrDefault().EmployeeDocuments;
                    var employeeid = db.Employee.Where(e => e.Id == empcode1).Select(r => new
                    {
                        EmployeeDocuments = r.EmployeeDocuments.Select(x => new {
                            Id=x.Id,
                           DocumentTypeLookupVal= x.DocumentType.LookupVal
                        }).Where(e => e.DocumentTypeLookupVal.ToUpper() == "PROFILE").ToList()

                    }).SingleOrDefault();

                    var empdocid1 = employeeid.EmployeeDocuments.Count()>0? employeeid.EmployeeDocuments.FirstOrDefault().Id:0;
                    int empdocid = 0;
                    if (empdocid1 != null)
                    {
                        empdocid = empdocid1;
                    }

                    int subid = Convert.ToInt32(empdocid);

                    employeedocuments = db.EmployeeDocuments.Where(e => e.Id == subid).AsNoTracking().FirstOrDefault();
                    if (employeedocuments != null)
                    {
                        localpath = employeedocuments.DocumentPath;

                        if (localpath != null)
                        {
                            FileInfo File = new FileInfo(localpath);
                            bool iExists = File.Exists;
                            if (iExists)
                            {
                                localpath = localpath;
                            }
                            else
                            {
                                localpath = ConfigurationManager.AppSettings["EmployeeDocuments"] + localpath;
                            }
                        }
                    }
                    else
                    {
                        return View("File Not Found");
                        //return Content("File Not Found");
                        //return Json(new { success = false, responseText = "File Not Found..!" }, JsonRequestBehavior.AllowGet);
                    }

                    FileInfo file = new FileInfo(localpath);
                    bool exists = file.Exists;
                    string extension = Path.GetExtension(file.Name);

                    if (exists)
                    {
                        if (extension.ToUpper() == ".PDF")
                        {
                            return File(file.FullName, "application/pdf", file.Name + " ");


                            //string pdf="pdf";
                            //byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");

                            //string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                            //return Json(new { data = base64ImageRepresentation, status = pdf }, JsonRequestBehavior.AllowGet);
                        }
                        if (extension.ToUpper() == ".JPG")
                        {
                            // return File(file.FullName, "image/png", file.Name + " ");
                            byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                            //return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);

                            JsonRequestBehavior behaviou = new JsonRequestBehavior();
                            return new JsonResult()
                            {
                                Data = base64ImageRepresentation,
                                MaxJsonLength = 86753090,
                                JsonRequestBehavior = behaviou
                            };

                        }
                        if (extension.ToUpper() == ".PNG")
                        {
                            //return File(file.FullName, "image/png", file.Name + " ");
                            byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                            //return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);

                            JsonRequestBehavior behaviou = new JsonRequestBehavior();
                            return new JsonResult()
                            {
                                Data = base64ImageRepresentation,
                                MaxJsonLength = 86753090,
                                JsonRequestBehavior = behaviou
                            };

                        }
                    }
                    else
                    {
                        return Content("File Not Found");
                        //return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                return null;
            }


        }
    }
}

