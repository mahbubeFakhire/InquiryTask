using Domain.AppSettings;
using Domain.Enums.InquiryRequests;
using Domain.Shared.Enums.InquiryRequests;
using Exceptions;
using Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.InquiryRequests.Models;

public class InquiryRequestDomainService : IInquiryRequestDomainService
{
    private readonly IInquiryRequestRepository _repository;
    private readonly InquiryRequestSetting _inquiryRequestSetting;

    public InquiryRequestDomainService(IInquiryRequestRepository repository,
                                       IOptions<InquiryRequestSetting> inquiryRequestSetting)
    {
        _repository = repository;
        _inquiryRequestSetting = inquiryRequestSetting.Value;
    }

    public Task<InquiryRequest> CreateEntity(
        InquiryRequestDomainModel model,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new InquiryRequest
        {
            IdempotencyKey = model.IdempotencyKey,
            RequestHash = model.RequestHash,
            ReferenceNumber = model.ReferenceNumber,
            BypassCache = model.BypassCache,
            Status = InquiryState.Processing,
            ProcessingDeadlineUtc = DateTime.UtcNow.AddMinutes(_inquiryRequestSetting.ProcessingDeadlineMin)
        });
    }

    public async Task<ResolvedRequestModel> ResolveRequest(string idempotencyKey, string hashRequest, CancellationToken cancellationToken)
    {
        var inquiryRequest = await _repository.GetByIdempotencyKey(idempotencyKey, cancellationToken);

        if (inquiryRequest is null)
        {
            return new(InquiryRequestResolvationType.NewRequest, null);
        }
        return ResolveExistingRequest(inquiryRequest, hashRequest);
    }

    private ResolvedRequestModel ResolveExistingRequest(InquiryRequest existingRequest, string requestHash)
    {
        if (existingRequest.RequestHash.Equals(requestHash, StringComparison.OrdinalIgnoreCase).Not())
        {
            throw new ConflictException($"X-Client-Idempotency-Key {existingRequest.IdempotencyKey} has already been used with different request data.");
        }

        return new(InquiryRequestResolvationType.IdempotentRetry, existingRequest);
    }

}
