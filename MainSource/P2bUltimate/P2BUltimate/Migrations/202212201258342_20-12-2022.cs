namespace P2BUltimate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20122022 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Address", name: "DepartmentObj_Id", newName: "District_Id");
            RenameIndex(table: "dbo.Address", name: "IX_DepartmentObj_Id", newName: "IX_District_Id");
            CreateTable(
                "dbo.FutureOD",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReqDate = c.DateTime(),
                        FromDate = c.DateTime(),
                        ToDate = c.DateTime(),
                        Reason = c.String(),
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
                        InputMethod = c.Int(nullable: false),
                        isCancel = c.Boolean(nullable: false),
                        TrClosed = c.Boolean(nullable: false),
                        TrReject = c.Boolean(nullable: false),
                        WFStatus_Id = c.Int(),
                        EmployeeAttendance_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EmployeeAttendance", t => t.EmployeeAttendance_Id)
                .ForeignKey("dbo.LookupValue", t => t.WFStatus_Id)
                .Index(t => t.WFStatus_Id)
                .Index(t => t.EmployeeAttendance_Id);
            
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
            
            AddColumn("dbo.AccessRights", "LvNoOfDaysFrom", c => c.Int(nullable: false));
            AddColumn("dbo.AccessRights", "LvNoOfDaysTo", c => c.Int(nullable: false));
            AddColumn("dbo.Department", "GeoFencingParameter_Id", c => c.Int());
            AddColumn("dbo.ProcessedData", "FutureOD_Id", c => c.Int());
            AddColumn("dbo.AttWFDetails", "FutureOD_Id", c => c.Int());
            CreateIndex("dbo.Department", "GeoFencingParameter_Id");
            CreateIndex("dbo.ProcessedData", "FutureOD_Id");
            CreateIndex("dbo.AttWFDetails", "FutureOD_Id");
            AddForeignKey("dbo.Department", "GeoFencingParameter_Id", "dbo.GeoFencing", "Id");
            AddForeignKey("dbo.ProcessedData", "FutureOD_Id", "dbo.FutureOD", "Id");
            AddForeignKey("dbo.AttWFDetails", "FutureOD_Id", "dbo.FutureOD", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FutureOD", "WFStatus_Id", "dbo.LookupValue");
            DropForeignKey("dbo.AttWFDetails", "FutureOD_Id", "dbo.FutureOD");
            DropForeignKey("dbo.ProcessedData", "FutureOD_Id", "dbo.FutureOD");
            DropForeignKey("dbo.FutureOD", "EmployeeAttendance_Id", "dbo.EmployeeAttendance");
            DropForeignKey("dbo.Department", "GeoFencingParameter_Id", "dbo.GeoFencing");
            DropIndex("dbo.AttWFDetails", new[] { "FutureOD_Id" });
            DropIndex("dbo.FutureOD", new[] { "EmployeeAttendance_Id" });
            DropIndex("dbo.FutureOD", new[] { "WFStatus_Id" });
            DropIndex("dbo.ProcessedData", new[] { "FutureOD_Id" });
            DropIndex("dbo.Department", new[] { "GeoFencingParameter_Id" });
            DropColumn("dbo.AttWFDetails", "FutureOD_Id");
            DropColumn("dbo.ProcessedData", "FutureOD_Id");
            DropColumn("dbo.Department", "GeoFencingParameter_Id");
            DropColumn("dbo.AccessRights", "LvNoOfDaysTo");
            DropColumn("dbo.AccessRights", "LvNoOfDaysFrom");
            DropTable("dbo.DT_TimingPolicyBatchAssignment");
            DropTable("dbo.DT_ITForm24QFileFormatDefinition");
            DropTable("dbo.DT_ExtnRednPolicy");
            DropTable("dbo.FutureOD");
            RenameIndex(table: "dbo.Address", name: "IX_District_Id", newName: "IX_DepartmentObj_Id");
            RenameColumn(table: "dbo.Address", name: "District_Id", newName: "DepartmentObj_Id");
        }
    }
}
