using Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Server.Filters;

public class ApiExceptionFilter : ExceptionFilterAttribute
{

    private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

    public ApiExceptionFilter()
    {
        // Register known exception types and handlers.
        _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(NotModifiedException), HandleNotModifiedException },
            { typeof(NotFoundException), HandleNotFoundException },
            { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException},
            { typeof(BusinessException),HandleBusinessException},
            { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
            { typeof(TaskCanceledException), HandleTaskCanceledException },
            { typeof(ConflictException), HandleConflictException },
            { typeof(ExternalServiceUnavailableException), HandleExternalServiceUnavailableException },
        };
    }

    public override void OnException(ExceptionContext context)
    {
        HandleException(context);

        base.OnException(context);
    }

    private void HandleException(ExceptionContext context)
    {
        Type type = context.Exception.GetType();
        if (_exceptionHandlers.ContainsKey(type))
        {
            _exceptionHandlers[type].Invoke(context);
            return;
        }

        HandleUnknownException(context);
    }

    private void HandleNotModifiedException(ExceptionContext context)
    {
        context.Result = new ObjectResult(null)
        {
            StatusCode = StatusCodes.Status304NotModified
        };

        context.ExceptionHandled = true;
    }

    private void HandleUnknownException(ExceptionContext context)
    {
        SetProblem(context,
            StatusCodes.Status500InternalServerError,
            code: "INTERNAL.SERVER_ERROR",
            title: "An unexpected error occurred.",
            detail: "An unexpected error occurred while processing the request.",
            type: "https://api.noban.ir/problems/internal-server-error");
    }

    private void HandleValidationException(ExceptionContext context)
    {
        var exception = (ValidationException)context.Exception;

        var errors = exception.Errors.SelectMany(x => x.Value.Select(message =>
                                                                        new ApiErrorDetail
                                                                        {
                                                                            Code = "VALIDATION.INVALID_VALUE",
                                                                            Field = x.Key,
                                                                            Path = $"$.{x.Key}",
                                                                            Message = message
                                                                        }))
                                      .ToArray();

        SetProblem(context,
            StatusCodes.Status400BadRequest,
            code: "VALIDATION.FAILED",
            title: "Request validation failed",
            detail: "One or more request fields are invalid.",
            type: "https://api.noban.ir/problems/validation-error",
            errors: errors);
    }

    private void HandleNotFoundException(ExceptionContext context)
    {
        var exception = (NotFoundException)context.Exception;

        SetProblem(context,
            StatusCodes.Status404NotFound,
            code: "RESOURCE.NOT_FOUND",
            title: "The specified resource was not found.",
            detail: exception.Message,
            type: "https://api.noban.ir/problems/not-found");
    }

    private void HandleUnauthorizedAccessException(ExceptionContext context)
    {
        var exception = (UnauthorizedAccessException)context.Exception;

        SetProblem(context,
            StatusCodes.Status401Unauthorized,
            code: "AUTH.UNAUTHORIZED",
            title: "Unauthorized",
            detail: "Authentication is required to access this resource.",
            type: "https://api.noban.ir/problems/unauthorized");
    }

    private void HandleBusinessException(ExceptionContext context)
    {
        var exception = (BusinessException)context.Exception;

        SetProblem(context,
            StatusCodes.Status400BadRequest,
            code: exception.ErrorCode
                ?? "BUSINESS.ERROR",
            title: "Request could not be processed",
            detail: exception.Message,
            type: "https://api.noban.ir/problems/business-error");
    }


    private void HandleForbiddenAccessException(ExceptionContext context)
    {
        SetProblem(context,
            StatusCodes.Status403Forbidden,
            code: "AUTH.FORBIDDEN",
            title: "Forbidden",
            detail: "You do not have permission to access this resource.",
            type: "https://api.noban.ir/problems/forbidden");
    }

    private void HandleTaskCanceledException(ExceptionContext context)
    {
        var details = new ProblemDetails()
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Error",
            Detail = "Task Canceled",
        };

        context.Result = new BadRequestObjectResult(details);

        context.ExceptionHandled = true;
    }

    private void HandleConflictException(ExceptionContext context)
    {
        var exception = (ConflictException)context.Exception;

        SetProblem(context,
            StatusCodes.Status409Conflict,
            code: exception.ErrorCode
                ?? "CONFLICT",
            title: "Request conflict",
            detail: exception.Message,
            type: "https://api.noban.ir/problems/conflict");
    }
    private void HandleExternalServiceUnavailableException(ExceptionContext context)
    {
        var exception = (ExternalServiceUnavailableException)context.Exception;

        SetProblem(context,
                   StatusCodes.Status503ServiceUnavailable,
                   code: exception.ErrorCode,
                   title: "Service temporarily unavailable",
                   detail: exception.Message,
                   type: "https://api.noban.ir/problems/service-unavailable");
    }


    private void SetProblem(ExceptionContext context,
                            int statusCode,
                            string code,
                            string title,
                            string detail,
                            string type,
                            IReadOnlyCollection<ApiErrorDetail> errors = null)
    {
        var problem = new ApiProblemDetails
        {
            Status = statusCode,
            Code = code,
            Title = title,

            Detail = detail,
            Type = type,
            Instance = context.HttpContext.Request.Path,
            TraceId = Activity.Current?.Id ??
                context.HttpContext.TraceIdentifier,
            Errors = errors ?? Array.Empty<ApiErrorDetail>()
        };

        var result = new ObjectResult(problem)
        {
            StatusCode = statusCode
        };

        result.ContentTypes.Add("application/problem+json");
        context.Result = result;
        context.ExceptionHandled = true;
    }
}
