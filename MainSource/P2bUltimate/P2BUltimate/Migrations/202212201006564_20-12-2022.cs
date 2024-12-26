namespace P2BUltimate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20122022 : DbMigration
    {
        public override void Up()
        {
            //RenameTable(name: "dbo.IncomeTaxITSection", newName: "ITSectionIncomeTax");
            DropForeignKey("dbo.HRATransT", "HRAExemptionMaster_Id", "dbo.HRAExemptionMaster");
            DropIndex("dbo.HRATransT", new[] { "HRAExemptionMaster_Id" });
            DropPrimaryKey("dbo.ITSectionIncomeTax");
            CreateTable(
                "dbo.LeaveDependPolicy",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsContinous = c.Boolean(nullable: false),
                        MinLvDaysAppl = c.Int(nullable: false),
                        IsAccumulated = c.Boolean(nullable: false),
                        AccMinLvDaysAppl = c.Int(nullable: false),
                        MaxDays = c.Int(nullable: false),
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
                        FullDetails = c.String(maxLength: 250),
                        LvHead_Id = c.Int(),
                        SalaryHead_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LvHead", t => t.LvHead_Id)
                .ForeignKey("dbo.SalaryHead", t => t.SalaryHead_Id)
                .Index(t => t.LvHead_Id)
                .Index(t => t.SalaryHead_Id);
            
            CreateTable(
                "dbo.DT_ExtnRednPolicy",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                        IsExtn = c.Boolean(nullable: false),
                        IsRedn = c.Boolean(nullable: false),
                        ExtnRednCauseType_Id = c.Int(nullable: false),
                        ExtnRednPeriodUnit_Id = c.Int(nullable: false),
                        ExtnRednPeriod = c.Int(nullable: false),
                        MaxCount = c.Int(nullable: false),
                        RowVersion = c.Binary(),
                        Orig_Id = c.Int(nullable: false),
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
                        Action = c.String(maxLength: 1),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DT_ITForm24QFileFormatDefinition",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Form24QFileType_Id = c.Int(nullable: false),
                        SrNo = c.Int(nullable: false),
                        Field = c.String(),
                        ExcelColNo = c.String(),
                        DataType_Id = c.Int(nullable: false),
                        Size = c.Int(nullable: false),
                        InputType = c.String(),
                        Narration = c.String(),
                        RowVersion = c.Binary(),
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
                        Orig_Id = c.Int(nullable: false),
                        Action = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DT_TimingPolicyBatchAssignment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PolicyBatchName = c.String(),
                        IsWeeklyTimingSchedule = c.Boolean(nullable: false),
                        IsTimingGroup = c.Boolean(nullable: false),
                        IsRoaster = c.Boolean(nullable: false),
                        TimingGroup_Id = c.Int(nullable: false),
                        RowVersion = c.Binary(),
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
                        Orig_Id = c.Int(nullable: false),
                        Action = c.String(),
                        FullDetails = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddPrimaryKey("dbo.ITSectionIncomeTax", new[] { "ITSection_Id", "IncomeTax_Id" });
            DropColumn("dbo.HRATransT", "HRAExemptionMaster_Id");
            DropTable("api.APIUsers");
            DropTable("mobile.p2bMobileRegister");
            DropTable("dbo.OTPHistory");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.OTPHistory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OTP = c.Int(nullable: false),
                        MobileNumber = c.String(),
                        EmailId = c.String(),
                        Identifier = c.String(),
                        RequestDate = c.DateTime(nullable: false),
                        ExpiryDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "mobile.p2bMobileRegister",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        Guid = c.Guid(nullable: false),
                        EmployeeCode = c.String(),
                        MobileNumber = c.String(),
                        password = c.String(),
                        Active = c.Boolean(nullable: false),
                        Created_Date = c.DateTime(),
                        Updated_Date = c.DateTime(),
                        Audit_Employee_Code = c.String(),
                        Company = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "api.APIUsers",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                        Password = c.String(),
                    })
                .PrimaryKey(t => t.Username);
            
            AddColumn("dbo.HRATransT", "HRAExemptionMaster_Id", c => c.Int());
            DropForeignKey("dbo.LeaveDependPolicy", "SalaryHead_Id", "dbo.SalaryHead");
            DropForeignKey("dbo.LeaveDependPolicy", "LvHead_Id", "dbo.LvHead");
            DropIndex("dbo.LeaveDependPolicy", new[] { "SalaryHead_Id" });
            DropIndex("dbo.LeaveDependPolicy", new[] { "LvHead_Id" });
            DropPrimaryKey("dbo.ITSectionIncomeTax");
            DropTable("dbo.DT_TimingPolicyBatchAssignment");
            DropTable("dbo.DT_ITForm24QFileFormatDefinition");
            DropTable("dbo.DT_ExtnRednPolicy");
            DropTable("dbo.LeaveDependPolicy");
            AddPrimaryKey("dbo.ITSectionIncomeTax", new[] { "IncomeTax_Id", "ITSection_Id" });
            CreateIndex("dbo.HRATransT", "HRAExemptionMaster_Id");
            AddForeignKey("dbo.HRATransT", "HRAExemptionMaster_Id", "dbo.HRAExemptionMaster", "Id");
            RenameTable(name: "dbo.ITSectionIncomeTax", newName: "IncomeTaxITSection");
        }
    }
}
