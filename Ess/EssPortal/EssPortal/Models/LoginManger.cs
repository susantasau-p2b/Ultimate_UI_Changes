using EssPortal.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using P2b.Global;

namespace EssPortal.Models
{
    public static class LoginManger
    {
       // private static DataBaseContext db = new DataBaseContext();
        
        public static LogRegister Get_log(string user_id) 
        {
            var id = Convert.ToInt32(user_id);
          //  var user_details = db.LogRegister.Where(e => e.Employee.Id == id).SingleOrDefault();
          //  return user_details;
            return new LogRegister();
        }
       
        public static string Get_Attempt(String user_id)
        {
           if (!String.IsNullOrEmpty(user_id))
           {
               var user_data = Get_log(user_id);
               return user_data.LogInAttempt.ToString();
           }
           else
           {
               return string.Empty;
           }
        }
        public static string Set_Attempt(String user_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!String.IsNullOrEmpty(user_id))
                {
                    var user_data = Get_log(user_id);
                    user_data.LogInDate = DateTime.Now;
                    user_data.LogOutDate = DateTime.Now;
                    user_data.LogInAttempt = user_data.LogInAttempt + 1;
                    try
                    {
                        db.LogRegister.Attach(user_data);
                        db.Entry(user_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(user_data).State = System.Data.Entity.EntityState.Detached;
                        return "";
                    }
                    catch (Exception e)
                    {

                        return e.InnerException.ToString();
                    }
                }
                else
                {
                    return String.Empty;
                }
            }
        }
    }
}