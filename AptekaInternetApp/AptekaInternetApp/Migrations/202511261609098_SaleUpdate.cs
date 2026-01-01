namespace AptekaInternetApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SaleUpdate : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Sales", "PaymentType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Sales", "PaymentType", c => c.Int(nullable: false));
        }
    }
}
