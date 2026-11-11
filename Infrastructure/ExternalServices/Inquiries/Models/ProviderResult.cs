using Domain.Shared.Enums.InquiryRequests;

namespace Infrastructure.ExternalServices.Inquiries.Models;

public abstract record ProviderResult
{
    public sealed record BusinessResponse(InquiryOutcome Outcome, string? ReasonCode) : ProviderResult;

    public sealed record TechnicalFailure(string ErrorCode) : ProviderResult;
}
