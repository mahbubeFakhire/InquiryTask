namespace Domain.Shared.Enums.InquiryRequests;

public enum InquiryState : byte
{
    Processing,
    Completed,
    TechnicalFailed,
    Interrupted
}