using Domain.Common;
using Domain.Shared.Enums.InquiryRequests;
using System;

namespace Domain.InquiryRequests;

public sealed class InquiryProviderAttempt : AuditableEntity
{
    public int AttemptNumber { get; set; }
    public required string ProviderKey { get; set; }
    public InquiryProviderAttemptOutcome Outcome { get; set; } = InquiryProviderAttemptOutcome.Started;
    public string ErrorCode { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset? FinishedAtUtc { get; set; }

    public long InquiryRequestId { get; set; }
    public InquiryRequest InquiryRequest { get; set; } = null!;
}