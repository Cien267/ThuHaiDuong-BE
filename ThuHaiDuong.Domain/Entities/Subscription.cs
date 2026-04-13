using Microsoft.EntityFrameworkCore;

namespace ThuHaiDuong.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
 
    // "Basic" | "Premium" | ...
    public string PlanCode { get; set; } = null!;
 
    // "Active" | "Expired" | "Cancelled" | "PendingPayment"
    public string Status { get; set; } = "PendingPayment";
 
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
 
    public decimal Amount { get; set; }
 
    // "VNPay" | "Momo" | "Stripe" | "Manual"
    public string? PaymentProvider { get; set; }
    public string? TransactionId { get; set; }
 
    // Navigation
    public virtual User User { get; set; } = null!;
}
 
public static class SubscriptionModelBuilderExtensions
{
    public static void BuildSubscriptionModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable("subscriptions");
            entity.HasKey(e => e.Id);
 
            entity.Property(e => e.UserId).IsRequired();
 
            entity.Property(e => e.PlanCode)
                .IsRequired()
                .HasMaxLength(50);
 
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("PendingPayment");
 
            entity.Property(e => e.StartDate)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.EndDate)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
 
            entity.Property(e => e.PaymentProvider)
                .HasMaxLength(50);
 
            entity.Property(e => e.TransactionId)
                .HasMaxLength(200);
 
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.Property(e => e.DeletedAt)
                .IsRequired()
                .HasColumnType("datetime2");
 
            entity.HasIndex(e => new { e.UserId, e.Status, e.EndDate })
                .HasDatabaseName("IX_Subscription_UserId_Status_EndDate");
 
            entity.HasIndex(e => e.TransactionId)
                .HasDatabaseName("IX_Subscription_TransactionId");
 
            entity.HasIndex(e => e.EndDate)
                .HasDatabaseName("IX_Subscription_EndDate");
 
            entity.HasOne(e => e.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}