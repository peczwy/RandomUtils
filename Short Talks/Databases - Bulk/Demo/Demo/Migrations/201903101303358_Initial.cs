namespace Demo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Simple",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    BusinessKey = c.Int(nullable: false),
                    Payload = c.String(maxLength: 100),
                    ValidFrom = c.DateTime(nullable: false),
                    ValidTo = c.DateTime(),
                    Active = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id, clustered: false);

            CreateTable(
                "dbo.SimpleSnapshot",
                c => new
                {
                    BusinessKey = c.Int(nullable: false, identity: false),
                    Payload = c.String(maxLength: 100),
                })
                .PrimaryKey(t => t.BusinessKey);

            Sql("CREATE CLUSTERED INDEX c_index ON dbo.Simple([Active], [BusinessKey])");
        }

        public override void Down()
        {
            DropTable("dbo.SimpleSnapshot");
            DropTable("dbo.Simple");
        }
    }
}
