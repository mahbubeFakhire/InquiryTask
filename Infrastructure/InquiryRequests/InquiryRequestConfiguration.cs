using Domain.InquiryRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.InquiryRequests;


public sealed class InquiryRequestConfiguration : IEntityTypeConfiguration<InquiryRequest>
{
    public void Configure(EntityTypeBuilder<InquiryRequest> builder)
    {
        builder.ToTable("InquiryRequests");

        builder.HasIndex(request => request.IdempotencyKey)
            .IsUnique();

        builder.Property(request => request.RequestHash).HasColumnType("char(64)").IsRequired();
        builder.Property(request => request.ReferenceNumber).HasMaxLength(64).IsUnicode(false).IsRequired();
        builder.Property(request => request.Status).HasConversion<byte>().HasMaxLength(24).IsUnicode(false);
        builder.Property(request => request.ResultOutcome).HasConversion<byte>().HasMaxLength(24).IsUnicode(false);
        builder.Property(request => request.ReasonCode).HasMaxLength(64).IsUnicode(false);
        builder.Property(request => request.ResultProviderKey).HasMaxLength(32).IsUnicode(false);
        builder.Property(request => request.ResultSource).HasConversion<byte>().HasMaxLength(12).IsUnicode(false);
    }
}