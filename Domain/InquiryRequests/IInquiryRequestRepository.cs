using Domain.InquiryRequests.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.InquiryRequests;

public interface IInquiryRequestRepository
{
    Task<InquiryRequest> GetByIdempotencyKey(string idempotencyKey, CancellationToken cancellationToken);
    Task<InquiryRequest> InterruptExpiredAsync(long inquiryId, CancellationToken cancellationToken);
    Task<InquiryRequest> RunInquiryAsync(InquiryRequest request, CancellationToken cancellationToken);
    Task<SaveInquiryResult> SaveOutOfContextAsync(InquiryRequest entity, CancellationToken cancellationToken);
}
