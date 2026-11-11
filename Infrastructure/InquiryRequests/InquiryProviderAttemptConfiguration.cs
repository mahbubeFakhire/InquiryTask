using Domain.InquiryRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.InquiryRequests;

public sealed class InquiryProviderAttemptConfiguration : IEntityTypeConfiguration<InquiryProviderAttempt>
{
    public void Configure(EntityTypeBuilder<InquiryProviderAttempt> builder)
    {
        builder.ToTable("InquiryProviderAttempts");

        builder.HasKey(attempt => new { attempt.InquiryRequestId, attempt.AttemptNumber });

        builder.HasOne(attempt => attempt.InquiryRequest)
            .WithMany(request => request.ProviderAttempts)
            .HasForeignKey(attempt => attempt.InquiryRequestId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(attempt => attempt.ProviderKey).HasMaxLength(32).IsUnicode(false).IsRequired();
        builder.Property(attempt => attempt.Outcome).HasConversion<byte>().HasMaxLength(24).IsUnicode(false);
        builder.Property(attempt => attempt.ErrorCode).HasMaxLength(64).IsUnicode(false);

    }
}
