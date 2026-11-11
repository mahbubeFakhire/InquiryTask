using Application.Common;
using Domain.Common.Interfaces;
using Domain.Enums.InquiryRequests;
using Domain.InquiryRequests;
using Domain.Shared.Enums.InquiryRequests;
using Exceptions;
using Extensions;
using MediatR;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Contracts.InquiryRequests.Queries.SampleInquiry;

public record SampleInquiryRequestQueryHandler : IRequestHandler<SampleInquiryRequestQuery, SampleInquiryOutputDto>
{
    private readonly IClientHeaders _headers;
    private readonly IInquiryRequestDomainService _domain;
    private readonly IInquiryRequestRepository _repository;
    private readonly TimeProvider _timeProvider;

    public SampleInquiryRequestQueryHandler(IClientHeaders headers,
                                            IInquiryRequestDomainService domain,
                                            IInquiryRequestRepository repository,
                                            TimeProvider timeProvider)
    {
        _headers = headers;
        _domain = domain;
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<SampleInquiryOutputDto> Handle(SampleInquiryRequestQuery request, CancellationToken cancellationToken)
    {
        var key = _headers.ClientIdempotencyKey;
        if (key.IsNullOrWhiteSpace())
            throw new BusinessException("X-Client-Idempotency-Key cannot be empty.");

        string referenceNumber, hash;
        GenerateHashRequest(request, out referenceNumber, out hash);

        var resolved = await _domain.ResolveRequest(key, hash, cancellationToken);
        switch (resolved.ResolvationType)
        {
            case InquiryRequestResolvationType.IdempotentRetry:
                return await HandleExistingRequestAsync(resolved.ExistingRequest, cancellationToken);


            case InquiryRequestResolvationType.NewRequest:
                return await HandleNewRequestAsync(request, key, referenceNumber, hash, cancellationToken);

            default:
                throw new InvalidOperationException("Operation is not valid");

        }
    }

    private async Task<SampleInquiryOutputDto> HandleNewRequestAsync(SampleInquiryRequestQuery request, string key, string referenceNumber, string hash, CancellationToken cancellationToken)
    {
        var model = new InquiryRequestDomainModel(key, hash, referenceNumber, request.BypassCache);
        var entity = await _domain.CreateEntity(model, cancellationToken);
        var saved = await _repository.SaveOutOfContextAsync(entity, cancellationToken);

        if (!saved.IsChanged)
        {
            var resolved = await _domain.ResolveRequest(key, hash, cancellationToken);
            if (resolved.ResolvationType == InquiryRequestResolvationType.IdempotentRetry)
            {
                return await HandleExistingRequestAsync(resolved.ExistingRequest, cancellationToken);
            }

            throw new ConflictException("The inquiry could not be saved or resolved by its idempotency key.");
        }

        var processed = await _repository.RunInquiryAsync(saved.Request, cancellationToken);
        return MapResult(processed);
    }

    private static void GenerateHashRequest(SampleInquiryRequestQuery request, out string referenceNumber, out string hash)
    {
        referenceNumber = request.ReferenceNumber.Trim().ToUpperInvariant();
        var payload = JsonSerializer.SerializeToUtf8Bytes(new
        {
            ReferenceNumber = referenceNumber,
            request.BypassCache
        });
        hash = RequestHashGenerator.Generate(payload);
    }

    private async Task<SampleInquiryOutputDto> HandleExistingRequestAsync(InquiryRequest request, CancellationToken cancellationToken)
    {
        if (request.Status == InquiryState.Processing &&
            request.ProcessingDeadlineUtc <= _timeProvider.GetUtcNow().UtcDateTime)
        {
            request = await _repository.InterruptExpiredAsync(request.Id, cancellationToken);
        }

        return MapResult(request);
    }

    private static SampleInquiryOutputDto MapResult(InquiryRequest request)
    {
        return request.Status switch
        {
            InquiryState.Completed when request.ResultOutcome is { } outcome &&
                                        request.ResultSource is { } source => new SampleInquiryOutputDto
                                        {
                                            InquiryId = request.Id,
                                            Outcome = outcome,
                                            ReasonCode = request.ReasonCode ?? string.Empty,
                                            Source = source
                                        },

            InquiryState.Processing => throw new ConflictException("The inquiry is still processing.", "inquiry_in_progress"),

            InquiryState.Interrupted => throw new ExternalServiceUnavailableException("Processing ended before an inquiry result was recorded.", "inquiry_interrupted"),

            InquiryState.TechnicalFailed => throw new ExternalServiceUnavailableException("No inquiry provider returned a usable result.", "inquiry_providers_unavailable"),

            _ => throw new InvalidOperationException("The inquiry does not have a valid persisted result.")
        };
    }
}
