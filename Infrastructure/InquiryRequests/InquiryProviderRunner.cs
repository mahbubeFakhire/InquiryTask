using Domain.InquiryRequests;
using Domain.Shared.Enums.InquiryRequests;
using Infrastructure.ExternalServices.Inquiries.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.InquiryRequests;

public partial class InquiryRequestRepository
{
    private static readonly object CacheWriteLock = new();

    public async Task<InquiryRequest> RunInquiryAsync(InquiryRequest request, CancellationToken cancellationToken)
    {
        var cacheKey = $"inquiry:v1:{request.ReferenceNumber}";

        if (!request.BypassCache &&
            _cache.TryGetValue(cacheKey, out CachedInquiryResult cached) &&
            cached.ExpiresAtUtc > _timeProvider.GetUtcNow())
        {
            var current = await ChangeRequestAsync(request.Id, (inquiry, now) =>
            {
                if (cached.ExpiresAtUtc > now)
                {
                    inquiry.CompleteFromCache(cached.ProviderKey, cached.Outcome, cached.ReasonCode,
                        cached.ObservedAtUtc.UtcDateTime, now.UtcDateTime);
                }
            }, cancellationToken);

            if (current.Status != InquiryState.Processing)
                return current;
        }

        var remaining = request.ProcessingDeadlineUtc - _timeProvider.GetUtcNow().UtcDateTime;
        if (remaining <= TimeSpan.Zero)
            return await InterruptExpiredAsync(request.Id, cancellationToken);

        using var deadline = new CancellationTokenSource(remaining, _timeProvider);
        using var processing = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, deadline.Token);

        try
        {
            for (var index = 0; index < _routes.Count; index++)
            {
                processing.Token.ThrowIfCancellationRequested();

                var route = _routes[index];
                var attemptNumber = index + 1;
                var current = await ChangeRequestAsync(request.Id, (inquiry, now) =>
                {
                    inquiry.ProviderAttempts.Add(new InquiryProviderAttempt
                    {
                        InquiryRequestId = inquiry.Id,
                        AttemptNumber = attemptNumber,
                        ProviderKey = route.Provider.Key,
                        StartedAtUtc = now
                    });
                }, cancellationToken);

                if (current.Status != InquiryState.Processing)
                    return current;

                processing.Token.ThrowIfCancellationRequested();
                var started = _timeProvider.GetTimestamp();
                var result = await InvokeAsync(route, current.ReferenceNumber, processing.Token);
                var observedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
                var duration = _timeProvider.GetElapsedTime(started);

                var lastAttempt = attemptNumber == _routes.Count;
                current = await ChangeRequestAsync(request.Id, (inquiry, now) =>
                {
                    var attempt = inquiry.ProviderAttempts.Single(item => item.AttemptNumber == attemptNumber);
                    attempt.Outcome = result.Outcome;
                    attempt.ErrorCode = result.ErrorCode;
                    attempt.FinishedAtUtc = now;

                    if (result.BusinessResponse is { } business)
                        inquiry.CompleteFromProvider(attempt.ProviderKey, business.Outcome, business.ReasonCode,
                            now.UtcDateTime, observedAtUtc);
                    else if (lastAttempt)
                        inquiry.MarkTechnicalFailure(now.UtcDateTime);
                }, cancellationToken);

                var persistedOutcome = current.ProviderAttempts.Single(item => item.AttemptNumber == attemptNumber).Outcome;

                _logger.LogInformation(
                    "Inquiry {InquiryId} provider {ProviderKey} attempt {AttemptNumber} ended with {Outcome} in {ElapsedMs} ms",
                    current.Id, route.Provider.Key, attemptNumber, persistedOutcome, duration.TotalMilliseconds);

                if (current.Status != InquiryState.Processing)
                {
                    if (current.Status == InquiryState.Completed && current.ResultSource == ResultSource.Provider)
                        CacheProviderResult(cacheKey, current);

                    return current;
                }
            }

            throw new InvalidOperationException("Provider routing did not produce a final inquiry status.");
        }
        catch (OperationCanceledException) when (
            deadline.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            return await InterruptExpiredAsync(request.Id, cancellationToken);
        }
    }

    private void CacheProviderResult(string cacheKey, InquiryRequest request)
    {
        var outcome = request.ResultOutcome.Value;
        var eligibleTtl = Math.Max(0, _settings.EligibleCacheTtlSeconds);
        var ineligibleTtl = Math.Max(0, _settings.IneligibleCacheTtlSeconds);
        var retentionSeconds = Math.Max(eligibleTtl, ineligibleTtl);
        if (retentionSeconds == 0)
            return;

        var ttlSeconds = outcome switch
        {
            InquiryOutcome.Eligible => eligibleTtl,
            InquiryOutcome.Ineligible => ineligibleTtl,
            _ => 0
        };

        var observedAt = new DateTimeOffset(
            DateTime.SpecifyKind(request.ResultObservedAtUtc.Value, DateTimeKind.Utc));
        var retainUntil = observedAt.AddSeconds(retentionSeconds);
        if (retainUntil <= _timeProvider.GetUtcNow())
            return;

        var entry = new CachedInquiryResult(outcome, request.ReasonCode, request.ResultProviderKey,
            observedAt, observedAt.AddSeconds(ttlSeconds));

        lock (CacheWriteLock)
        {
            if (_cache.TryGetValue(cacheKey, out CachedInquiryResult current) &&
                current.ObservedAtUtc > observedAt)
                return;

            // A non-cacheable result suppresses older observations without being served.
            _cache.Set(cacheKey, entry, retainUntil);
        }
    }

    private sealed record CachedInquiryResult(
        InquiryOutcome Outcome,
        string ReasonCode,
        string ProviderKey,
        DateTimeOffset ObservedAtUtc,
        DateTimeOffset ExpiresAtUtc);

    public Task<InquiryRequest> InterruptExpiredAsync(long inquiryId, CancellationToken cancellationToken)
    {
        return ChangeRequestAsync(inquiryId, (_, _) => { }, cancellationToken);
    }

    private async Task<InquiryRequest> ChangeRequestAsync(long inquiryId, Action<InquiryRequest, DateTimeOffset> change, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Database.SetCommandTimeout(10);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var inquiries = dbContext.Set<InquiryRequest>();

        var affectedRows = await inquiries
            .Where(request =>
                request.Id == inquiryId &&
                request.Status == InquiryState.Processing)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    request => request.Status,
                    request => request.Status),
                cancellationToken);

        var inquiry = await inquiries
            .AsTracking()
            .Include(request => request.ProviderAttempts)
            .SingleAsync(request => request.Id == inquiryId, cancellationToken);

        if (affectedRows == 1)
        {
            var now = _timeProvider.GetUtcNow();
            inquiry.InterruptIfExpired(now);

            if (inquiry.Status == InquiryState.Processing)
            {
                change(inquiry, now);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return inquiry;
    }
    private async Task<AttemptResult> InvokeAsync(InquiryProviderRoute route, string referenceNumber, CancellationToken processingToken)
    {
        using var timeout = new CancellationTokenSource(route.Timeout, _timeProvider);
        using var attempt = CancellationTokenSource.CreateLinkedTokenSource(processingToken, timeout.Token);

        try
        {
            var response = await route.Provider.InquireAsync(referenceNumber, attempt.Token);
            attempt.Token.ThrowIfCancellationRequested();

            return response switch
            {
                ProviderResult.BusinessResponse business =>
                    new(InquiryProviderAttemptOutcome.BusinessResponse, null, business),

                ProviderResult.TechnicalFailure failure =>
                    new(InquiryProviderAttemptOutcome.TechnicalFailure, failure.ErrorCode, null),

                _ => throw new InvalidOperationException("The inquiry provider returned an unsupported result.")
            };
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested && !processingToken.IsCancellationRequested)
        {
            return new(InquiryProviderAttemptOutcome.Timeout, "provider_timeout", null);
        }
    }

    private sealed record AttemptResult(InquiryProviderAttemptOutcome Outcome,
                                        string ErrorCode,
                                        ProviderResult.BusinessResponse BusinessResponse);
}
