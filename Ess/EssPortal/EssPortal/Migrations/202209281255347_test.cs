namespace EssPortal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FFSSettlementDetailT",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SalaryHead = c.String(),
                        SalaryHeadDesc = c.String(),
                        PayAmount = c.Double(nullable: false),
                        SalType = c.String(),
                        ProcessMonth = c.DateTime(),
                        IsPaid = c.Boolean(nullable: false),
                        PayMonth = c.DateTime(),
                        PayDate = c.DateTime(),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        DBTrack_Action = c.String(maxLength: 1),
                        DBTrack_IsModified = c.Boolean(nullable: false),
                        DBTrack_CreatedBy = c.String(maxLength: 15),
                        DBTrack_CreatedOn = c.DateTime(),
                        DBTrack_ModifiedBy = c.String(maxLength: 15),
                        DBTrack_ModifiedOn = c.DateTime(),
                        DBTrack_AuthorizedBy = c.String(maxLength: 15),
                        DBTrack_AuthorizedOn = c.DateTime(),
                        DBTrack_CreatedComment = c.String(maxLength: 50),
                        DBTrack_SanctionBy = c.String(maxLength: 15),
                        DBTrack_SanctionDate = c.DateTime(),
                        DBTrack_SanctionComment = c.String(maxLength: 50),
                        DBTrack_ApproveBy = c.String(maxLength: 15),
                        DBTrack_ApproveDate = c.DateTime(),
                        DBTrack_ApprovedComment = c.String(maxLength: 50),
                        DBTrack_Ess = c.Boolean(nullable: false),
                        DBTrack_Stage = c.Int(nullable: false),
                        DBTrack_TrClosed = c.Boolean(nullable: false),
                        DBTrack_IsAuthorized = c.Int(nullable: false),
                        ProcessType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LookupValue", t => t.ProcessType_Id)
                .Index(t => t.ProcessType_Id);
            
            AddColumn("dbo.SeperationProcessT", "FFSSettlementDetailT_Id", c => c.Int());
            CreateIndex("dbo.SeperationProcessT", "FFSSettlementDetailT_Id");
            AddForeignKey("dbo.SeperationProcessT", "FFSSettlementDetailT_Id", "dbo.FFSSettlementDetailT", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SeperationProcessT", "FFSSettlementDetailT_Id", "dbo.FFSSettlementDetailT");
            DropForeignKey("dbo.FFSSettlementDetailT", "ProcessType_Id", "dbo.LookupValue");
            DropIndex("dbo.FFSSettlementDetailT", new[] { "ProcessType_Id" });
            DropIndex("dbo.SeperationProcessT", new[] { "FFSSettlementDetailT_Id" });
            DropColumn("dbo.SeperationProcessT", "FFSSettlementDetailT_Id");
            DropTable("dbo.FFSSettlementDetailT");
        }
    }
}
