namespace Domain.Shared.Enums.InquiryRequests;

public enum InquiryProviderAttemptOutcome : byte
{
    Started,
    BusinessResponse,
    TechnicalFailure,
    Timeout,
    Unknown
}
