namespace Domain.Enums.InquiryRequests;

public enum InquiryRequestResolvationType : byte
{
    NewRequest,
    IdempotentRetry
}
