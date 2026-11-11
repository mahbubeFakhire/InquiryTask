using Infrastructure.ExternalServices.Inquiries.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.ExternalServices.Inquiries;

public interface IInquiryProvider
{
    string Key { get; }

    Task<ProviderResult> InquireAsync(string referenceNumber, CancellationToken cancellationToken);
}