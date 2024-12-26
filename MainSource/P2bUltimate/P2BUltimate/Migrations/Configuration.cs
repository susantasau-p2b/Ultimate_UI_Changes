namespace P2BUltimate.Migrations
{
    using System;
    using System.Configuration;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<P2BUltimate.App_Start.DataBaseContext>
    {
        public Configuration()
        {
            var Migration = true;
            var migVal = ConfigurationManager.AppSettings["migration"];
            if (!String.IsNullOrEmpty(migVal))
            {
                var temp = Convert.ToBoolean(migVal);
                Migration = temp;
            }
            AutomaticMigrationsEnabled = Migration;
            AutomaticMigrationDataLossAllowed = Migration;
            ContextKey = "P2BUltimate.App_Start.DataBaseContext";
        }

        protected override void Seed(P2BUltimate.App_Start.DataBaseContext context)
        {
          
        }
    }
}
