using Microsoft.EntityFrameworkCore;
using WholesaleHub.Models;

namespace WholesaleHub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Inventory> Inventories { get; set; } = null!;
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; } = null!;
        public DbSet<SalesOrder> SalesOrders { get; set; } = null!;
        public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; } = null!;
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; } = null!;
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = null!;
        public DbSet<Delivery> Deliveries { get; set; } = null!;
        public DbSet<AccountsReceivable> AccountsReceivables { get; set; } = null!;
        public DbSet<AccountsReceivable> AccountsReceivable { get; set; } = null!;
        public DbSet<CustomerPayment> CustomerPayments { get; set; } = null!;
        public DbSet<CustomerCrmInteraction> CustomerCrmInteractions { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AccountsReceivable>()
                .HasOne(ar => ar.Customer)
                .WithMany()
                .HasForeignKey(ar => ar.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AccountsReceivable>()
                .HasOne(ar => ar.SalesOrder)
                .WithOne(order => order.AccountsReceivable)
                .HasForeignKey<AccountsReceivable>(ar => ar.SalesOrderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasOne(customer => customer.User)
                .WithOne()
                .HasForeignKey<Customer>(customer => customer.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasIndex(customer => customer.UserID)
                .IsUnique();
        }
    }
}