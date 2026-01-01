namespace AptekaInternetApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StockProductUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "PackageContentsQuantity", c => c.String());
            AlterColumn("dbo.Products", "StorageConditions", c => c.String());
            DropColumn("dbo.Stocks", "ProductionDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Stocks", "ProductionDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Products", "StorageConditions", c => c.String(maxLength: 100));
            DropColumn("dbo.Products", "PackageContentsQuantity");
        }
    }
}
