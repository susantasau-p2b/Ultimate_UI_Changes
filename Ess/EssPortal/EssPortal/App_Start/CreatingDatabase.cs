using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using EssPortal.App_Start;

namespace EssPortal.App_Start
{
    public class CreatingDatabase : CreateDatabaseIfNotExists<DataBaseContext>
    {
        protected override void Seed(DataBaseContext context)
        {
            base.Seed(context);
        }
    }
}