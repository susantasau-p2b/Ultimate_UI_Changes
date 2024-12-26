using P2b.Global;
using P2BUltimate.App_Start;
using Payroll;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Web;
namespace P2BUltimate.Models
{
    public static class DBTrackFile
    {
        private static DataBaseContext db = new DataBaseContext();

        public static object DBTrackSave(string AssemblyName, DbPropertyValues originalBlogValues, DbChangeTracker changeTrack, DBTrack dbTrck)
        {
            var entries = changeTrack.Entries().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted);
            
            foreach (var entry in entries)
            {

                if (entry.Entity != null)
                {
                    string entityName = string.Empty;
                    string state = string.Empty;

                    switch (entry.State)
                    {
                        case System.Data.Entity.EntityState.Modified:
                            Dictionary<string, object> openWith =
                                                  new Dictionary<string, object>();
                            entityName = ObjectContext.GetObjectType(entry.Entity.GetType()).Name;
                            state = entry.State.ToString();

                            if (originalBlogValues == null)
                            {
                                foreach (string prop in entry.CurrentValues.PropertyNames)
                                {
                                    if (entry.CurrentValues[prop] != null)
                                    {
                                        if (prop != "RowVersion")
                                        {
                                            if (prop == "DBTrack")
                                            {

                                                openWith.Add(prop, dbTrck);
                                            }
                                            else
                                            {
                                                openWith.Add(prop, entry.CurrentValues[prop].ToString());
                                            }
                                        }
                                    }

                                }
                                openWith.Add("RowVersion", entry.OriginalValues["RowVersion"]);
                                openWith.Add("Orig_Id", entry.OriginalValues["Id"]);
                            }
                            else
                            {
                                foreach (string prop in entry.OriginalValues.PropertyNames)
                                {
                                    if (prop != "Id")
                                    {
                                        object currentValue = entry.CurrentValues[prop];
                                        object originalValue = originalBlogValues[prop];
                                        if (currentValue != null)
                                        {
                                            if (originalValue != null)
                                            {
                                                if (!currentValue.Equals(originalValue))
                                                {
                                                    if (prop == "DBTrack")
                                                    {

                                                        openWith.Add(prop, dbTrck);
                                                    }
                                                    else
                                                    {
                                                        openWith.Add(prop, originalBlogValues[prop].ToString());
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }
                                openWith.Add("RowVersion", originalBlogValues["RowVersion"]);
                                openWith.Add("Orig_Id", originalBlogValues["Id"]);
                            }
                           
                            openWith.Add("Action", "M");
                            var a = DBTrackFile.GetInstance(AssemblyName, "DT_" + entityName, openWith);
                            return a;
                            //db.Create(a);
                          //  break;
                        case System.Data.Entity.EntityState.Added:
                            entityName = ObjectContext.GetObjectType(entry.Entity.GetType()).Name;
                            state = entry.State.ToString();
                            openWith = new Dictionary<string, object>();
                            foreach (string prop in entry.CurrentValues.PropertyNames)
                            {
                                    if (entry.CurrentValues[prop] != null)
                                    {

                                        if (prop == "DBTrack")
                                        {

                                            openWith.Add(prop, dbTrck);
                                        }

                                        else
                                        {
                                            openWith.Add(prop, entry.CurrentValues[prop].ToString());
                                        }
                                    }
                            }

                            //openWith.Add("RowVersion", originalBlogValues["RowVersion"]);
                            //openWith.Add("Orig_Id", originalBlogValues["Id"]);
                            openWith.Add("Action", "C");
                            a = DBTrackFile.GetInstance(AssemblyName, "DT_" + entityName, openWith);
                           // db.Create(a);
                            return a;
                            //break;
                        case System.Data.Entity.EntityState.Deleted:
                            entityName = ObjectContext.GetObjectType(entry.Entity.GetType()).Name;
                            state = entry.State.ToString();
                            openWith = new Dictionary<string, object>();
                            foreach (string prop in entry.OriginalValues.PropertyNames)
                            {
                                if (prop == "DBTrack")
                                {
                                    openWith.Add(prop, dbTrck);
                                }
                                else
                                {
                                    openWith.Add(prop, entry.OriginalValues[prop]);
                                }
                            }
                            openWith.Add("Orig_Id", entry.OriginalValues["Id"]);
                            openWith.Add("Action", "D");
                            a = DBTrackFile.GetInstance(AssemblyName, "DT_" + entityName, openWith);
                           // db.Create(a);
                            return a;
                            //break;
                    }

                }
                return "";
                //break;
                
            }
            return "";
        }

          public static object ModifiedDataHistory(string AssemblyName, string Action, dynamic Old_Obj, dynamic Curr_Obj, string EntityName, DBTrack dbTrack)
        {
            Dictionary<string, object> rt = new Dictionary<string, object>();
            PropertyInfo[] fi = null;
            if (Curr_Obj != null)
            {

                List<Variance> variances = new List<Variance>();
                fi = Old_Obj.GetType().GetProperties();
                foreach (PropertyInfo f in fi)
                {
                    Variance v = new Variance();
                    v.Prop = f.Name;
                    v.valA = f.GetValue(Old_Obj);
                    v.valB = f.GetValue(Curr_Obj);

                    if (v.valA != null || v.valB != null)
                    {
                        if (!v.Prop.Equals("Id") && !v.Prop.Equals("RowVersion") && !v.Prop.Equals("DBTrack"))
                        {
                            if (v.valA != null && v.valB == null)
                                rt.Add(v.Prop, v.valB);
                            else if (v.valA == null && v.valB != null)
                                rt.Add(v.Prop, v.valB);
                            else if (!v.valA.Equals(v.valB))
                                rt.Add(v.Prop, v.valB);

                        }
                    }

                }
                // rt = Old_Obj.DetailedCompare(Curr_Obj);
            }
            else
            {
                fi = Old_Obj.GetType().GetProperties();
                foreach (var Prop in fi)
                {
                    if (Prop.Name != "Id" && Prop.Name != "DBTrack" && Prop.Name != "RowVersion")
                    {
                        rt.Add(Prop.Name, Prop.GetValue(Old_Obj));
                    }

                }
            }
            rt.Add("Orig_Id", Old_Obj.Id);
            rt.Add("Action", "M");
            rt.Add("DBTrack", dbTrack);
            rt.Add("RowVersion", Old_Obj.RowVersion);
            var a = DBTrackFile.GetInstance(AssemblyName, "DT_" + EntityName, rt);
            return a;
        }

        public static object GetInstance(string AssemblyName, string className, Dictionary<string, object> properties)
        {
            //var assembly = Assembly.GetExecutingAssembly();

            //var type = assembly.GetTypes()
            //    .First(t => t.Name == className);
            //   var assemblyName = AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().FullName).Name;


            Assembly assembly = Assembly.Load(AssemblyName.Split('/')[0]);


            Type t = assembly.GetType(AssemblyName.Split('/')[1] + "." + className);
            var type = Activator.CreateInstance(t);

            foreach (var b in properties)
            {

                PropertyInfo property = type.GetType().GetProperty(b.Key);
                if (property != null)
                {
                    Type t1 = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    if (t1 == typeof(DBTrack))
                    {
                        property.SetValue(type, b.Value, null);
                        //object safeValue = (b.Value == null) ? null : Convert.ChangeType(b.Value, t1);
                        //property.SetValue(type, safeValue, null);
                    }
                    else
                    {
                        object safeValue = (b.Value == null) ? null : Convert.ChangeType(b.Value, t1);
                        property.SetValue(type, safeValue, null);
                    }
                }
            }
            return type;
        }

        public static void Create<T>(this DataBaseContext db, T entityToCreate)
   where T : class
        {
            db.Entry<T>(entityToCreate).State = System.Data.Entity.EntityState.Added;
            db.SaveChanges();
        }

        public static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(a1, a2);
        }

        public static int ValCompare(dynamic Val1, dynamic Val2)
        {
            if (Val1 == null && Val2 == null)
            {
                return 0;
            }
            if (Val1 == null && Val2 != null)
            {
                return Val2.Id;
            }
            if (Val1 != null && Val2 == null)
            {
                return 0;
            }
            if (Val1 != null && Val2 != null)
            {
                if (Val1.Id == Val2.Id )
                {
                    return Val2.Id;
                }
                if (Val1.Id != Val2.Id)
                {
                    return Val2.Id;
                }
            }
          
            return 0;
        }
    }
}