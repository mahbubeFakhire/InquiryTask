using Domain.InquiryRequests.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.InquiryRequests;

public interface IInquiryRequestDomainService
{
    Task<InquiryRequest> CreateEntity(InquiryRequestDomainModel inquiryRequestModel, CancellationToken cancellationToken);
    Task<ResolvedRequestModel> ResolveRequest(string idempotencyKey, string hashRequest, CancellationToken cancellationToken);
}
