using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace WebApi.Server.Filters;


public sealed class ApiProblemDetails : ProblemDetails
{
    public string Code { get; init; }

    public string TraceId { get; init; }

    public IReadOnlyCollection<ApiErrorDetail> Errors { get; init; }
        = Array.Empty<ApiErrorDetail>();
}

public sealed record ApiErrorDetail
{
    public string Code { get; init; }

    public string Field { get; init; }

    public string Path { get; init; }

    public string Message { get; init; }
}