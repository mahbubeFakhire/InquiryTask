using Domain.Shared.Enums.InquiryRequests;
using Infrastructure.ExternalServices.Inquiries.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.ExternalServices.Inquiries;

public sealed class ProviderB : IInquiryProvider
{
    public string Key => "provider-b";

    public Task<ProviderResult> InquireAsync(string referenceNumber, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (referenceNumber == "REF-ALL-FAIL")
        {
            return Task.FromResult<ProviderResult>(new ProviderResult.TechnicalFailure("provider_unavailable"));
        }

        var response = referenceNumber is "REF-FALLBACK" or "REF-INELIGIBLE"
            ? new RawResponse(false, 17)
            : new RawResponse(true, null);

        ProviderResult result = response.Eligible
            ? new ProviderResult.BusinessResponse(InquiryOutcome.Eligible, null)
            : new ProviderResult.BusinessResponse(InquiryOutcome.Ineligible, response.RuleId == 17 ? "age_requirement_not_met" : null);

        return Task.FromResult(result);
    }
    private sealed record RawResponse(bool Eligible, int? RuleId);
}