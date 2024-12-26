using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace P2BUltimate.App_Start
{
    public class CreatingDatabase : CreateDatabaseIfNotExists<DataBaseContext>
    {
        protected override void Seed(DataBaseContext context)
        {
            base.Seed(context);
        }
    }
}