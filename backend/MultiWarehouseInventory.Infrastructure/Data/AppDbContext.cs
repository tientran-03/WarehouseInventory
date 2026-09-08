using Microsoft.EntityFrameworkCore;
using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<WarehouseInventory> WarehouseInventories => Set<WarehouseInventory>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();
        public DbSet<StockDocument> StockDocuments => Set<StockDocument>();
        public DbSet<StockDocumentLine> StockDocumentLines => Set<StockDocumentLine>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<SubOrder> SubOrders => Set<SubOrder>();
        public DbSet<SubOrderItem> SubOrderItems => Set<SubOrderItem>();
        public DbSet<StockReservation> StockReservations => Set<StockReservation>();
        public DbSet<User> Users => Set<User>();
        public DbSet<WarehouseTransfer> WarehouseTransfers => Set<WarehouseTransfer>();
        public DbSet<Stocktake> Stocktakes => Set<Stocktake>();
        public DbSet<WarehouseZone> WarehouseZones => Set<WarehouseZone>();
        public DbSet<Invoice> Invoices => Set<Invoice>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<WarehouseInventory>()
                .HasIndex(x => new { x.WarehouseId, x.ProductId, x.WarehouseZoneId })
                .IsUnique();
            modelBuilder.Entity<WarehouseInventory>()
                .HasOne(x => x.WarehouseZone)
                .WithMany(x => x.Inventories)
                .HasForeignKey(x => x.WarehouseZoneId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StockDocument>()
                .HasIndex(x => new { x.TenantId, x.DocumentCode })
                .IsUnique();
            modelBuilder.Entity<StockDocument>()
                .HasIndex(x => new { x.TenantId, x.WarehouseId, x.DocumentDate });
            modelBuilder.Entity<StockDocument>()
                .Property(x => x.DocumentCode)
                .HasMaxLength(64);
            modelBuilder.Entity<StockDocument>()
                .Property(x => x.Type)
                .HasMaxLength(16);
            modelBuilder.Entity<StockDocument>()
                .Property(x => x.Status)
                .HasMaxLength(16);
            modelBuilder.Entity<StockDocument>()
                .HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StockDocumentLine>()
                .HasIndex(x => new { x.StockDocumentId, x.ProductId, x.WarehouseZoneId })
                .IsUnique();
            modelBuilder.Entity<StockDocumentLine>()
                .HasOne(x => x.StockDocument)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.StockDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<StockDocumentLine>()
                .HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StockDocumentLine>()
                .HasOne(x => x.WarehouseZone)
                .WithMany()
                .HasForeignKey(x => x.WarehouseZoneId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StockMovement>()
                .HasOne(x => x.StockDocument)
                .WithMany(x => x.Movements)
                .HasForeignKey(x => x.StockDocumentId)
                .OnDelete(DeleteBehavior.Restrict);
                 modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(254);
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasMaxLength(254);
            modelBuilder.Entity<User>()
                .HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Invoice>()
                .HasIndex(x => new { x.TenantId, x.InvoiceCode })
                .IsUnique();
            modelBuilder.Entity<Invoice>()
                .HasIndex(x => x.StockDocumentId)
                .IsUnique();
            modelBuilder.Entity<Invoice>()
                .HasOne(x => x.StockDocument)
                .WithMany()
                .HasForeignKey(x => x.StockDocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
