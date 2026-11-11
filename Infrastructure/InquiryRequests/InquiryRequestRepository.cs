using Domain.AppSettings;
using Domain.InquiryRequests;
using Domain.InquiryRequests.Models;
using Infrastructure.ExternalServices.Inquiries.Models;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.InquiryRequests;

public partial class InquiryRequestRepository : IInquiryRequestRepository
{
    protected readonly ApplicationDbContext _dbContext;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<InquiryRequestRepository> _logger;
    private readonly IReadOnlyList<InquiryProviderRoute> _routes;
    private readonly IMemoryCache _cache;
    private readonly InquiryRequestSetting _settings;

    public InquiryRequestRepository(ApplicationDbContext dbContext,
                                    IDbContextFactory<ApplicationDbContext> dbContextFactory,
                                    TimeProvider timeProvider,
                                    ILogger<InquiryRequestRepository> logger,
                                    IReadOnlyList<InquiryProviderRoute> routes,
                                    IMemoryCache cache,
                                    IOptions<InquiryRequestSetting> settings)
    {
        _dbContext = dbContext;
        _dbContextFactory = dbContextFactory;
        _timeProvider = timeProvider;
        _logger = logger;
        _routes = routes;
        _cache = cache;
        _settings = settings.Value;
    }

    public Task<InquiryRequest> GetByIdempotencyKey(string idempotencyKey, CancellationToken cancellationToken)
    {
        return GetAllAsQueryable()
            .AsNoTracking()
            .Include(x => x.ProviderAttempts)
            .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public virtual Task<InquiryRequest> GetByIdAsync(long id, CancellationToken cancellationToken = default, bool asTracking = true)
    {
        var entities = _dbContext.Set<InquiryRequest>();

        var query = asTracking
            ? entities.AsTracking()
            : entities.AsNoTracking();

        return query.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public async Task<InquiryRequest> SaveAsync(InquiryRequest entity, CancellationToken cancellationToken)
    {
        var existingEntity = await GetByIdAsync(entity.Id, cancellationToken: cancellationToken);

        if (existingEntity is null)
        {
            await CreateAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }

        _dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return existingEntity;
    }

    private IQueryable<InquiryRequest> GetAllAsQueryable()
    {
        return _dbContext.Set<InquiryRequest>();
    }

    private async Task<InquiryRequest> CreateAsync(InquiryRequest entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<InquiryRequest>().AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<SaveInquiryResult> SaveOutOfContextAsync(InquiryRequest entity, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Database.SetCommandTimeout(10);

        var inquiries = dbContext.Set<InquiryRequest>();

        try
        {
            if (entity.Id == 0)
                inquiries.Add(entity);
            else
                inquiries.Update(entity);

            await dbContext.SaveChangesAsync(cancellationToken);
            return new SaveInquiryResult(IsChanged: true, Request: entity);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return new SaveInquiryResult(IsChanged: false);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException
        {
            Number: 2601 or 2627
        };
    }
}
