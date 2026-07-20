using Microsoft.EntityFrameworkCore;
using SlipperIS.Core.Models;

namespace SlipperIS.Data.DbContext;

public class SlipperDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public SlipperDbContext(DbContextOptions<SlipperDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }
    public DbSet<Quotation> Quotations { get; set; }
    public DbSet<QuotationDetail> QuotationDetails { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<StockRecord> StockRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.Permissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SalesOrder>()
            .HasOne(so => so.Customer)
            .WithMany(c => c.SalesOrders)
            .HasForeignKey(so => so.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SalesOrderDetail>()
            .HasOne(sod => sod.SalesOrder)
            .WithMany(so => so.OrderDetails)
            .HasForeignKey(sod => sod.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SalesOrderDetail>()
            .HasOne(sod => sod.Product)
            .WithMany(p => p.SalesOrderDetails)
            .HasForeignKey(sod => sod.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Quotation>()
            .HasOne(q => q.Customer)
            .WithMany()
            .HasForeignKey(q => q.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<QuotationDetail>()
            .HasOne(qd => qd.Quotation)
            .WithMany(q => q.QuotationDetails)
            .HasForeignKey(qd => qd.QuotationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuotationDetail>()
            .HasOne(qd => qd.Product)
            .WithMany()
            .HasForeignKey(qd => qd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductImage>()
            .HasOne(pi => pi.Product)
            .WithMany(p => p.ProductImages)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StockRecord>()
            .HasOne(sr => sr.Product)
            .WithMany(p => p.StockRecords)
            .HasForeignKey(sr => sr.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Roles
        var adminRole = new Role { Id = 1, Name = "Admin", Description = "Administrator" };
        var salesRole = new Role { Id = 2, Name = "Sales", Description = "Sales Employee" };
        var warehouseRole = new Role { Id = 3, Name = "Warehouse", Description = "Warehouse Manager" };

        modelBuilder.Entity<Role>().HasData(adminRole, salesRole, warehouseRole);

        // Seed Permissions
        var permissions = new[]
        {
            new Permission { Id = 1, Name = "Products.View", Module = "Products", Action = "View" },
            new Permission { Id = 2, Name = "Products.Create", Module = "Products", Action = "Create" },
            new Permission { Id = 3, Name = "Products.Edit", Module = "Products", Action = "Edit" },
            new Permission { Id = 4, Name = "Products.Delete", Module = "Products", Action = "Delete" },
            new Permission { Id = 5, Name = "Products.Import", Module = "Products", Action = "Import" },
            new Permission { Id = 6, Name = "Products.Export", Module = "Products", Action = "Export" },
            new Permission { Id = 7, Name = "Sales.View", Module = "Sales", Action = "View" },
            new Permission { Id = 8, Name = "Sales.Create", Module = "Sales", Action = "Create" },
            new Permission { Id = 9, Name = "Sales.Edit", Module = "Sales", Action = "Edit" },
            new Permission { Id = 10, Name = "Sales.Delete", Module = "Sales", Action = "Delete" },
            new Permission { Id = 11, Name = "Quotations.View", Module = "Quotations", Action = "View" },
            new Permission { Id = 12, Name = "Quotations.Create", Module = "Quotations", Action = "Create" },
            new Permission { Id = 13, Name = "Quotations.Edit", Module = "Quotations", Action = "Edit" },
            new Permission { Id = 14, Name = "Quotations.Delete", Module = "Quotations", Action = "Delete" },
            new Permission { Id = 15, Name = "Quotations.EditPrice", Module = "Quotations", Action = "EditPrice" },
            new Permission { Id = 16, Name = "Quotations.Export", Module = "Quotations", Action = "Export" },
            new Permission { Id = 17, Name = "Reports.View", Module = "Reports", Action = "View" },
            new Permission { Id = 18, Name = "Reports.Export", Module = "Reports", Action = "Export" },
            new Permission { Id = 19, Name = "Users.View", Module = "Users", Action = "View" },
            new Permission { Id = 20, Name = "Users.Create", Module = "Users", Action = "Create" },
            new Permission { Id = 21, Name = "Users.Edit", Module = "Users", Action = "Edit" },
            new Permission { Id = 22, Name = "Users.Delete", Module = "Users", Action = "Delete" }
        };

        modelBuilder.Entity<Permission>().HasData(permissions);

        // Assign all permissions to Admin
        for (int i = 1; i <= permissions.Length; i++)
        {
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { Id = i, RoleId = 1, PermissionId = i }
            );
        }

        // Assign specific permissions to Sales
        modelBuilder.Entity<RolePermission>().HasData(
            new RolePermission { Id = 23, RoleId = 2, PermissionId = 1 },
            new RolePermission { Id = 24, RoleId = 2, PermissionId = 7 },
            new RolePermission { Id = 25, RoleId = 2, PermissionId = 8 },
            new RolePermission { Id = 26, RoleId = 2, PermissionId = 9 },
            new RolePermission { Id = 27, RoleId = 2, PermissionId = 11 },
            new RolePermission { Id = 28, RoleId = 2, PermissionId = 12 },
            new RolePermission { Id = 29, RoleId = 2, PermissionId = 17 }
        );

        // Assign specific permissions to Warehouse
        modelBuilder.Entity<RolePermission>().HasData(
            new RolePermission { Id = 30, RoleId = 3, PermissionId = 1 },
            new RolePermission { Id = 31, RoleId = 3, PermissionId = 2 },
            new RolePermission { Id = 32, RoleId = 3, PermissionId = 3 },
            new RolePermission { Id = 33, RoleId = 3, PermissionId = 6 }
        );

        // Seed Users
        var adminUser = new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = SlipperIS.Core.Services.PasswordHasher.HashPassword("admin123"),
            FullName = "Administrator",
            RoleId = 1,
            IsActive = true
        };

        var salesUser = new User
        {
            Id = 2,
            Username = "sales",
            PasswordHash = SlipperIS.Core.Services.PasswordHasher.HashPassword("sales123"),
            FullName = "Sales User",
            RoleId = 2,
            IsActive = true
        };

        var warehouseUser = new User
        {
            Id = 3,
            Username = "warehouse",
            PasswordHash = SlipperIS.Core.Services.PasswordHasher.HashPassword("warehouse123"),
            FullName = "Warehouse User",
            RoleId = 3,
            IsActive = true
        };

        modelBuilder.Entity<User>().HasData(adminUser, salesUser, warehouseUser);

        // Seed sample customers
        var customers = new[]
        {
            new Customer { Id = 1, CustomerCode = "C001", CustomerName = "ABC Company", ContactPerson = "John", PhoneNumber = "0371-1234567", Email = "john@abc.com", City = "Zhengzhou", IsActive = true },
            new Customer { Id = 2, CustomerCode = "C002", CustomerName = "XYZ Corporation", ContactPerson = "Mary", PhoneNumber = "0371-2345678", Email = "mary@xyz.com", City = "Zhengzhou", IsActive = true }
        };

        modelBuilder.Entity<Customer>().HasData(customers);

        // Seed sample products
        var products = new[]
        {
            new Product { Id = 1, ProductCode = "P001", ProductName = "Running Slipper", Specification = "Size 40-45", Unit = "Pair", CostPrice = 15m, SalesPrice = 25m, VipPrice = 22m, StockQuantity = 100, MinStockLevel = 20, Description = "Comfortable running slipper" },
            new Product { Id = 2, ProductCode = "P002", ProductName = "Comfort Slipper", Specification = "Size 36-42", Unit = "Pair", CostPrice = 12m, SalesPrice = 20m, VipPrice = 18m, StockQuantity = 150, MinStockLevel = 30, Description = "Indoor comfort slipper" },
            new Product { Id = 3, ProductCode = "P003", ProductName = "Outdoor Slipper", Specification = "Size 38-44", Unit = "Pair", CostPrice = 18m, SalesPrice = 30m, VipPrice = 27m, StockQuantity = 80, MinStockLevel = 15, Description = "Outdoor wear slipper" }
        };

        modelBuilder.Entity<Product>().HasData(products);
    }
}
