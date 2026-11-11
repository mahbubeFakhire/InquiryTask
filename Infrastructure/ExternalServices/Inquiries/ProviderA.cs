using Domain.Shared.Enums.InquiryRequests;
using Infrastructure.ExternalServices.Inquiries.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.ExternalServices.Inquiries;

public sealed class ProviderA : IInquiryProvider
{
    public string Key => "provider-a";

    public async Task<ProviderResult> InquireAsync(string referenceNumber,CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (referenceNumber == "REF-TIMEOUT")
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

        if (referenceNumber is "REF-FALLBACK" or "REF-ALL-FAIL")
            return new ProviderResult.TechnicalFailure("provider_unavailable");

        var response = referenceNumber switch
        {
            "REF-INELIGIBLE" => new RawResponse("REJECTED", "AGE_LIMIT"),
            "REF-PENDING" => new RawResponse("UNDER_REVIEW", null),
            _ => new RawResponse("APPROVED", null)
        };

        return response.Decision switch
        {
            "APPROVED" => new ProviderResult.BusinessResponse(
                InquiryOutcome.Eligible, null),
            "REJECTED" => new ProviderResult.BusinessResponse(
                InquiryOutcome.Ineligible,
                response.Reason == "AGE_LIMIT" ? "age_requirement_not_met" : null),
            "UNDER_REVIEW" => new ProviderResult.BusinessResponse(
                InquiryOutcome.Pending, null),
            _ => new ProviderResult.TechnicalFailure("invalid_provider_response")
        };
    }

    private sealed record RawResponse(string Decision, string? Reason);
}
