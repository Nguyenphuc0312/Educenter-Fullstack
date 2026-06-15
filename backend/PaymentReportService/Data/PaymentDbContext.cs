using Microsoft.EntityFrameworkCore;
using PaymentReportService.Entities;

namespace PaymentReportService.Data;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<TuitionInvoice> TuitionInvoices => Set<TuitionInvoice>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<RevenueReportSnapshot> RevenueReportSnapshots => Set<RevenueReportSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<UserAccount>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<TuitionInvoice>().HasIndex(x => x.InvoiceCode).IsUnique();
        modelBuilder.Entity<TuitionInvoice>().HasIndex(x => x.EnrollmentId).IsUnique().HasFilter("[EnrollmentId] IS NOT NULL");
        modelBuilder.Entity<PaymentTransaction>().HasOne(x => x.Invoice).WithMany(x => x.Payments).HasForeignKey(x => x.InvoiceId);
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()).Where(p => p.ClrType == typeof(decimal)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }
    }
}
