namespace EssPortal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class kdcc : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ITSectionIncomeTax", "ITSection_Id", "dbo.ITSection");
            DropForeignKey("dbo.ITSectionIncomeTax", "IncomeTax_Id", "dbo.IncomeTax");
            DropForeignKey("dbo.ITTDSIncomeTax", "ITTDS_Id", "dbo.ITTDS");
            DropForeignKey("dbo.ITTDSIncomeTax", "IncomeTax_Id", "dbo.IncomeTax");
            DropForeignKey("dbo.DT_ManpowerRequestPost", "Gender_Id", "dbo.LookupValue");
            DropForeignKey("dbo.DT_ManpowerRequestPost", "MaritalStatus_Id", "dbo.LookupValue");
            DropIndex("dbo.DT_ManpowerRequestPost", new[] { "Gender_Id" });
            DropIndex("dbo.DT_ManpowerRequestPost", new[] { "MaritalStatus_Id" });
            DropIndex("dbo.ITSectionIncomeTax", new[] { "ITSection_Id" });
            DropIndex("dbo.ITSectionIncomeTax", new[] { "IncomeTax_Id" });
            DropIndex("dbo.ITTDSIncomeTax", new[] { "ITTDS_Id" });
            DropIndex("dbo.ITTDSIncomeTax", new[] { "IncomeTax_Id" });
            AddColumn("dbo.ITSection", "IncomeTax_Id", c => c.Int());
            AddColumn("dbo.ITTDS", "IncomeTax_Id", c => c.Int());
            CreateIndex("dbo.ITSection", "IncomeTax_Id");
            CreateIndex("dbo.ITTDS", "IncomeTax_Id");
            AddForeignKey("dbo.ITSection", "IncomeTax_Id", "dbo.IncomeTax", "Id");
            AddForeignKey("dbo.ITTDS", "IncomeTax_Id", "dbo.IncomeTax", "Id");
            //DropTable("dbo.DT_ManpowerRequestPost");
            //DropTable("dbo.DT_ShortlistingCriteria");
            DropTable("dbo.ITSectionIncomeTax");
            DropTable("dbo.ITTDSIncomeTax");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ITTDSIncomeTax",
                c => new
                    {
                        ITTDS_Id = c.Int(nullable: false),
                        IncomeTax_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ITTDS_Id, t.IncomeTax_Id });
            
            CreateTable(
                "dbo.ITSectionIncomeTax",
                c => new
                    {
                        ITSection_Id = c.Int(nullable: false),
                        IncomeTax_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ITSection_Id, t.IncomeTax_Id });
            
            CreateTable(
                "dbo.DT_ShortlistingCriteria",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CriteriaName = c.String(),
                        FuncStruct_Id = c.Int(nullable: false),
                        NoOfVacancies = c.Int(nullable: false),
                        ExpFilter_Id = c.Int(nullable: false),
                        ExpYearFrom = c.Double(nullable: false),
                        ExpYearTo = c.Double(nullable: false),
                        RangeFilter_Id = c.Int(nullable: false),
                        AgeFrom = c.Int(nullable: false),
                        AgeTo = c.Int(nullable: false),
                        SpecialCategory_Id = c.Int(nullable: false),
                        Category_Id = c.Int(nullable: false),
                        Gender_Id = c.Int(nullable: false),
                        MaritalStatus_Id = c.Int(nullable: false),
                        RelaxationAge = c.Int(nullable: false),
                        Narration = c.String(),
                        RowVersion = c.Binary(),
                        DBTrack_Action = c.String(maxLength: 1),
                        DBTrack_IsModified = c.Boolean(nullable: false),
                        DBTrack_CreatedBy = c.String(maxLength: 6),
                        DBTrack_CreatedOn = c.DateTime(),
                        DBTrack_ModifiedBy = c.String(maxLength: 6),
                        DBTrack_ModifiedOn = c.DateTime(),
                        DBTrack_AuthorizedBy = c.String(maxLength: 6),
                        DBTrack_AuthorizedOn = c.DateTime(),
                        DBTrack_CreatedComment = c.String(maxLength: 50),
                        DBTrack_SanctionBy = c.String(maxLength: 6),
                        DBTrack_SanctionDate = c.DateTime(),
                        DBTrack_SanctionComment = c.String(maxLength: 50),
                        DBTrack_ApproveBy = c.String(maxLength: 6),
                        DBTrack_ApproveDate = c.DateTime(),
                        DBTrack_ApprovedComment = c.String(maxLength: 50),
                        DBTrack_Ess = c.Boolean(nullable: false),
                        DBTrack_Stage = c.Int(nullable: false),
                        DBTrack_TrClosed = c.Boolean(nullable: false),
                        DBTrack_IsAuthorized = c.Int(nullable: false),
                        Orig_Id = c.Int(nullable: false),
                        Action = c.String(),
                        FullDetails = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DT_ManpowerRequestPost",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PostCode = c.String(),
                        PostRequestDate = c.DateTime(),
                        PostSourceType_Id = c.Int(nullable: false),
                        FuncStruct_Id = c.Int(nullable: false),
                        RequestVacancies = c.Int(nullable: false),
                        SanctionedVacancies = c.Int(nullable: false),
                        ExpFilter_Id = c.Int(nullable: false),
                        ExpYearFrom = c.Double(nullable: false),
                        ExpYearTo = c.Double(nullable: false),
                        RangeFilter_Id = c.Int(nullable: false),
                        AgeFrom = c.Int(nullable: false),
                        AgeTo = c.Int(nullable: false),
                        Narration = c.String(),
                        TrClosed = c.Boolean(nullable: false),
                        TrReject = c.Boolean(nullable: false),
                        WFStatus_Id = c.Int(nullable: false),
                        RowVersion = c.Binary(),
                        DBTrack_Action = c.String(maxLength: 1),
                        DBTrack_IsModified = c.Boolean(nullable: false),
                        DBTrack_CreatedBy = c.String(maxLength: 6),
                        DBTrack_CreatedOn = c.DateTime(),
                        DBTrack_ModifiedBy = c.String(maxLength: 6),
                        DBTrack_ModifiedOn = c.DateTime(),
                        DBTrack_AuthorizedBy = c.String(maxLength: 6),
                        DBTrack_AuthorizedOn = c.DateTime(),
                        DBTrack_CreatedComment = c.String(maxLength: 50),
                        DBTrack_SanctionBy = c.String(maxLength: 6),
                        DBTrack_SanctionDate = c.DateTime(),
                        DBTrack_SanctionComment = c.String(maxLength: 50),
                        DBTrack_ApproveBy = c.String(maxLength: 6),
                        DBTrack_ApproveDate = c.DateTime(),
                        DBTrack_ApprovedComment = c.String(maxLength: 50),
                        DBTrack_Ess = c.Boolean(nullable: false),
                        DBTrack_Stage = c.Int(nullable: false),
                        DBTrack_TrClosed = c.Boolean(nullable: false),
                        DBTrack_IsAuthorized = c.Int(nullable: false),
                        Orig_Id = c.Int(nullable: false),
                        Action = c.String(),
                        FullDetails = c.String(),
                        Gender_Id = c.Int(),
                        MaritalStatus_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.ITTDS", "IncomeTax_Id", "dbo.IncomeTax");
            DropForeignKey("dbo.ITSection", "IncomeTax_Id", "dbo.IncomeTax");
            DropIndex("dbo.ITTDS", new[] { "IncomeTax_Id" });
            DropIndex("dbo.ITSection", new[] { "IncomeTax_Id" });
            DropColumn("dbo.ITTDS", "IncomeTax_Id");
            DropColumn("dbo.ITSection", "IncomeTax_Id");
            CreateIndex("dbo.ITTDSIncomeTax", "IncomeTax_Id");
            CreateIndex("dbo.ITTDSIncomeTax", "ITTDS_Id");
            CreateIndex("dbo.ITSectionIncomeTax", "IncomeTax_Id");
            CreateIndex("dbo.ITSectionIncomeTax", "ITSection_Id");
            CreateIndex("dbo.DT_ManpowerRequestPost", "MaritalStatus_Id");
            CreateIndex("dbo.DT_ManpowerRequestPost", "Gender_Id");
            AddForeignKey("dbo.DT_ManpowerRequestPost", "MaritalStatus_Id", "dbo.LookupValue", "Id");
            AddForeignKey("dbo.DT_ManpowerRequestPost", "Gender_Id", "dbo.LookupValue", "Id");
            AddForeignKey("dbo.ITTDSIncomeTax", "IncomeTax_Id", "dbo.IncomeTax", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ITTDSIncomeTax", "ITTDS_Id", "dbo.ITTDS", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ITSectionIncomeTax", "IncomeTax_Id", "dbo.IncomeTax", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ITSectionIncomeTax", "ITSection_Id", "dbo.ITSection", "Id", cascadeDelete: true);
        }
    }
}
