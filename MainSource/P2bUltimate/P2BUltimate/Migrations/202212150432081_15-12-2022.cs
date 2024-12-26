namespace P2BUltimate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _15122022 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.IncomeTaxITSection", newName: "ITSectionIncomeTax");
            DropForeignKey("dbo.LvDebitPolicy", "PostApplyPrefixSuffix_PrefixSuffixAction_Id", "dbo.PrefixSuffixAction");
            DropForeignKey("dbo.LvDebitPolicy", "PreApplyPrefixSuffix_PrefixSuffixAction_Id", "dbo.PrefixSuffixAction");
            DropForeignKey("dbo.LvDebitPolicy", "PrefixSuffix_PrefixSuffixAction_Id", "dbo.PrefixSuffixAction");
            DropForeignKey("dbo.HRATransT", "HRAExemptionMaster_Id", "dbo.HRAExemptionMaster");
            DropForeignKey("dbo.ITSection10SalHeads", "ITSection10_Id", "dbo.ITSection10");
            DropIndex("dbo.LvDebitPolicy", new[] { "PostApplyPrefixSuffix_PrefixSuffixAction_Id" });
            DropIndex("dbo.LvDebitPolicy", new[] { "PreApplyPrefixSuffix_PrefixSuffixAction_Id" });
            DropIndex("dbo.LvDebitPolicy", new[] { "PrefixSuffix_PrefixSuffixAction_Id" });
            DropIndex("dbo.HRATransT", new[] { "HRAExemptionMaster_Id" });
            RenameColumn(table: "dbo.Address", name: "DepartmentObj_Id", newName: "District_Id");
            RenameIndex(table: "dbo.Address", name: "IX_DepartmentObj_Id", newName: "IX_District_Id");
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
            AddColumn("dbo.LvSharingPolicy", "Preference", c => c.Int(nullable: false));
            AddColumn("dbo.ProcessedData", "FutureOD_Id", c => c.Int());
            AddColumn("dbo.AttWFDetails", "FutureOD_Id", c => c.Int());
            //AddColumn("dbo.ITSection10SalHeads", "ITSection10_Id1", c => c.Int());
            AddPrimaryKey("dbo.ITSectionIncomeTax", new[] { "ITSection_Id", "IncomeTax_Id" });
            CreateIndex("dbo.Department", "GeoFencingParameter_Id");
            CreateIndex("dbo.ProcessedData", "FutureOD_Id");
            CreateIndex("dbo.AttWFDetails", "FutureOD_Id");
            //CreateIndex("dbo.ITSection10SalHeads", "ITSection10_Id1");
            AddForeignKey("dbo.Department", "GeoFencingParameter_Id", "dbo.GeoFencing", "Id");
            AddForeignKey("dbo.ProcessedData", "FutureOD_Id", "dbo.FutureOD", "Id");
            AddForeignKey("dbo.AttWFDetails", "FutureOD_Id", "dbo.FutureOD", "Id");
            //AddForeignKey("dbo.ITSection10SalHeads", "ITSection10_Id1", "dbo.ITSection10", "Id");
            DropColumn("dbo.LvDebitPolicy", "IsHalfDaySession_WaiveOffPrefixSuffix");
            DropColumn("dbo.LvDebitPolicy", "PostApplyPrefixSuffix_PrefixSuffixAction_Id");
            DropColumn("dbo.LvDebitPolicy", "PreApplyPrefixSuffix_PrefixSuffixAction_Id");
            DropColumn("dbo.LvDebitPolicy", "PrefixSuffix_PrefixSuffixAction_Id");
            DropColumn("dbo.LvSharingPolicy", "Perference");
            DropColumn("dbo.LvNewReq", "IsDebitSharing");
            DropColumn("dbo.LvNewReq", "DebitSharingNarration");
            DropColumn("dbo.LvNewReq", "LvCountPrefixSuffix");
            DropColumn("dbo.HRATransT", "HRAExemptionMaster_Id");
            DropColumn("dbo.DT_LvNewReq", "IsDebitSharing");
            DropColumn("dbo.DT_LvNewReq", "DebitSharingNarration");
            DropTable("dbo.PrefixSuffixAction");
            DropTable("api.APIUsers");
            DropTable("dbo.LvErrorMessage");
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
                "dbo.LvErrorMessage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Message_Code = c.Int(nullable: false),
                        Message_Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "api.APIUsers",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                        Password = c.String(),
                    })
                .PrimaryKey(t => t.Username);
            
            CreateTable(
                "dbo.PrefixSuffixAction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsFixedDayDebit_PrefixSuffixAction = c.Boolean(nullable: false),
                        FixedDebitDay = c.Int(nullable: false),
                        IsActualDayDebit_PrefixSuffixAction = c.Boolean(nullable: false),
                        IsWaiveOffDayDebit_PrefixSuffixAction = c.Boolean(nullable: false),
                        WaiveOffDebitDay = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.DT_LvNewReq", "DebitSharingNarration", c => c.String());
            AddColumn("dbo.DT_LvNewReq", "IsDebitSharing", c => c.Boolean(nullable: false));
            AddColumn("dbo.HRATransT", "HRAExemptionMaster_Id", c => c.Int());
            AddColumn("dbo.LvNewReq", "LvCountPrefixSuffix", c => c.Double(nullable: false));
            AddColumn("dbo.LvNewReq", "DebitSharingNarration", c => c.String());
            AddColumn("dbo.LvNewReq", "IsDebitSharing", c => c.Boolean(nullable: false));
            AddColumn("dbo.LvSharingPolicy", "Perference", c => c.Int(nullable: false));
            AddColumn("dbo.LvDebitPolicy", "PrefixSuffix_PrefixSuffixAction_Id", c => c.Int());
            AddColumn("dbo.LvDebitPolicy", "PreApplyPrefixSuffix_PrefixSuffixAction_Id", c => c.Int());
            AddColumn("dbo.LvDebitPolicy", "PostApplyPrefixSuffix_PrefixSuffixAction_Id", c => c.Int());
            AddColumn("dbo.LvDebitPolicy", "IsHalfDaySession_WaiveOffPrefixSuffix", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.ITSection10SalHeads", "ITSection10_Id1", "dbo.ITSection10");
            DropForeignKey("dbo.LeaveDependPolicy", "SalaryHead_Id", "dbo.SalaryHead");
            DropForeignKey("dbo.LeaveDependPolicy", "LvHead_Id", "dbo.LvHead");
            DropForeignKey("dbo.FutureOD", "WFStatus_Id", "dbo.LookupValue");
            DropForeignKey("dbo.AttWFDetails", "FutureOD_Id", "dbo.FutureOD");
            DropForeignKey("dbo.ProcessedData", "FutureOD_Id", "dbo.FutureOD");
            DropForeignKey("dbo.FutureOD", "EmployeeAttendance_Id", "dbo.EmployeeAttendance");
            DropForeignKey("dbo.Department", "GeoFencingParameter_Id", "dbo.GeoFencing");
            DropIndex("dbo.ITSection10SalHeads", new[] { "ITSection10_Id1" });
            DropIndex("dbo.AttWFDetails", new[] { "FutureOD_Id" });
            DropIndex("dbo.FutureOD", new[] { "EmployeeAttendance_Id" });
            DropIndex("dbo.FutureOD", new[] { "WFStatus_Id" });
            DropIndex("dbo.ProcessedData", new[] { "FutureOD_Id" });
            DropIndex("dbo.LeaveDependPolicy", new[] { "SalaryHead_Id" });
            DropIndex("dbo.LeaveDependPolicy", new[] { "LvHead_Id" });
            DropIndex("dbo.Department", new[] { "GeoFencingParameter_Id" });
            DropPrimaryKey("dbo.ITSectionIncomeTax");
            DropColumn("dbo.ITSection10SalHeads", "ITSection10_Id1");
            DropColumn("dbo.AttWFDetails", "FutureOD_Id");
            DropColumn("dbo.ProcessedData", "FutureOD_Id");
            DropColumn("dbo.LvSharingPolicy", "Preference");
            DropColumn("dbo.Department", "GeoFencingParameter_Id");
            DropColumn("dbo.AccessRights", "LvNoOfDaysTo");
            DropColumn("dbo.AccessRights", "LvNoOfDaysFrom");
            DropTable("dbo.DT_TimingPolicyBatchAssignment");
            DropTable("dbo.DT_ITForm24QFileFormatDefinition");
            DropTable("dbo.DT_ExtnRednPolicy");
            DropTable("dbo.FutureOD");
            DropTable("dbo.LeaveDependPolicy");
            AddPrimaryKey("dbo.ITSectionIncomeTax", new[] { "IncomeTax_Id", "ITSection_Id" });
            RenameIndex(table: "dbo.Address", name: "IX_District_Id", newName: "IX_DepartmentObj_Id");
            RenameColumn(table: "dbo.Address", name: "District_Id", newName: "DepartmentObj_Id");
            CreateIndex("dbo.HRATransT", "HRAExemptionMaster_Id");
            CreateIndex("dbo.LvDebitPolicy", "PrefixSuffix_PrefixSuffixAction_Id");
            CreateIndex("dbo.LvDebitPolicy", "PreApplyPrefixSuffix_PrefixSuffixAction_Id");
            CreateIndex("dbo.LvDebitPolicy", "PostApplyPrefixSuffix_PrefixSuffixAction_Id");
            AddForeignKey("dbo.ITSection10SalHeads", "ITSection10_Id", "dbo.ITSection10", "Id");
            AddForeignKey("dbo.HRATransT", "HRAExemptionMaster_Id", "dbo.HRAExemptionMaster", "Id");
            AddForeignKey("dbo.LvDebitPolicy", "PrefixSuffix_PrefixSuffixAction_Id", "dbo.PrefixSuffixAction", "Id");
            AddForeignKey("dbo.LvDebitPolicy", "PreApplyPrefixSuffix_PrefixSuffixAction_Id", "dbo.PrefixSuffixAction", "Id");
            AddForeignKey("dbo.LvDebitPolicy", "PostApplyPrefixSuffix_PrefixSuffixAction_Id", "dbo.PrefixSuffixAction", "Id");
            RenameTable(name: "dbo.ITSectionIncomeTax", newName: "IncomeTaxITSection");
        }
    }
}
