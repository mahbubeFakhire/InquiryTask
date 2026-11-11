using Domain.Common;
using Domain.Shared.Enums.InquiryRequests;
using System;
using System.Collections.Generic;

namespace Domain.InquiryRequests;


public sealed class InquiryRequest : AuditableEntity
{
    public string IdempotencyKey { get; internal set; }
    public string RequestHash { get; internal set; }
    public string ReferenceNumber { get; internal set; }
    public bool BypassCache { get; internal set; }
    public InquiryState Status { get; internal set; } = InquiryState.Processing;

    public InquiryOutcome? ResultOutcome { get; internal set; }
    public string ReasonCode { get; set; }
    public string ResultProviderKey { get; internal set; }
    public ResultSource? ResultSource { get; internal set; }
    public DateTime? ResultObservedAtUtc { get; internal set; }

    public DateTime ProcessingDeadlineUtc { get; internal set; }
    public DateTime? CompletedAtUtc { get; internal set; }

    public ICollection<InquiryProviderAttempt> ProviderAttempts { get; } = new List<InquiryProviderAttempt>();

    public void CompleteFromProvider(string providerKey, InquiryOutcome outcome, string reasonCode, DateTime nowUtc, DateTime? observedAtUtc = null)
    {
        ResultProviderKey = providerKey;
        ResultOutcome = outcome;
        ReasonCode = reasonCode;
        ResultSource = Shared.Enums.InquiryRequests.ResultSource.Provider;
        ResultObservedAtUtc = observedAtUtc ?? nowUtc;
        CompletedAtUtc = nowUtc;
        Status = InquiryState.Completed;
    }

    public void CompleteFromCache(string providerKey, InquiryOutcome outcome, string reasonCode, DateTime observedAtUtc, DateTime nowUtc)
    {
        ResultProviderKey = providerKey;
        ResultOutcome = outcome;
        ReasonCode = reasonCode;
        ResultSource = Shared.Enums.InquiryRequests.ResultSource.Cache;
        ResultObservedAtUtc = observedAtUtc;
        CompletedAtUtc = nowUtc;
        Status = InquiryState.Completed;
    }

    public void MarkTechnicalFailure(DateTime nowUtc)
    {
        CompletedAtUtc = nowUtc;
        Status = InquiryState.TechnicalFailed;
    }

    public void InterruptIfExpired(DateTimeOffset now)
    {
        if (Status != InquiryState.Processing || now.UtcDateTime < ProcessingDeadlineUtc)
            return;

        foreach (var attempt in ProviderAttempts)
        {
            if (attempt.Outcome != InquiryProviderAttemptOutcome.Started)
                continue;

            attempt.Outcome = InquiryProviderAttemptOutcome.Unknown;
            attempt.ErrorCode = "inquiry_interrupted";
            attempt.FinishedAtUtc = now;
        }

        CompletedAtUtc = now.UtcDateTime;
        Status = InquiryState.Interrupted;
    }
}
