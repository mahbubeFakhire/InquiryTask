# Inquiry Service

A .NET 8 ASP.NET Core Web API for submitting a read-only eligibility inquiry. The service validates the request, checks idempotency, and obtains a result from the in-memory cache or from two simulated providers. SQL Server stores the request, its result, and the history of provider attempts.

## Run locally

Prerequisites are the .NET 8 SDK and a reachable SQL Server instance. Run the commands below in PowerShell from the directory containing `Inquiry.sln`.

```powershell
dotnet restore WebApi/Server/WebApi.Server.csproj
dotnet build WebApi/Server/WebApi.Server.csproj --no-restore

# Install once if the EF Core CLI is missing.
dotnet tool install --global dotnet-ef --version 8.0.20

# Example for local SQL Server using Windows authentication.
$env:ConnectionStrings__DefaultConnection = 'Server=localhost;Database=InquiryDemo;Integrated Security=True;TrustServerCertificate=True'
$env:ASPNETCORE_ENVIRONMENT = 'Development'

# The design-time factory reads appsettings.Development.json from this directory.
Push-Location WebApi/Server
dotnet ef database update --project ../../Infrastructure/Infrastructure.csproj --startup-project WebApi.Server.csproj --connection "$env:ConnectionStrings__DefaultConnection"
Pop-Location

dotnet run --project WebApi/Server/WebApi.Server.csproj --no-launch-profile -- --urls http://localhost:5080
```

Adjust the connection string for your instance. The migration creates the schema in the target database. The migration account needs the corresponding database permissions. Swagger UI is available in Development at [http://localhost:5080/swagger/index.html](http://localhost:5080/swagger/index.html).

To generate the database script from the committed migration, run the following from `WebApi/Server`. Run the generated script against the intended SQL Server database if applying migrations through SQL.

```powershell
dotnet ef migrations script --idempotent --project ../../Infrastructure/Infrastructure.csproj --startup-project WebApi.Server.csproj --output ../../InquiryDatabase.sql
```

## API and smoke check

Send `POST /api/inquiries/sample` with the `X-Client-Idempotency-Key` header. The request body accepts `referenceNumber` and `bypassCache`. References are trimmed and uppercased. The response contains the inquiry ID, business outcome, reason code, and result source. The current JSON contract encodes the enums as numbers. `source` is `0` for Provider and `1` for Cache. `outcome` is `0` for Eligible, `1` for Ineligible, and `2` for Pending.

With a fresh process and the default TTLs, run these calls within 60 seconds to observe a cache hit and a bypass.

```powershell
function Invoke-SampleInquiry {
    param(
        [string]$Reference = 'REF-SUCCESS',
        [bool]$BypassCache = $false,
        [string]$Key = [guid]::NewGuid().ToString('N')
    )
    Invoke-RestMethod -Method Post -Uri 'http://localhost:5080/api/inquiries/sample' `
        -Headers @{ 'X-Client-Idempotency-Key' = $Key } `
        -ContentType 'application/json' `
        -Body (@{ referenceNumber = $Reference; bypassCache = $BypassCache } | ConvertTo-Json)
}

$first = Invoke-SampleInquiry                         # Provider, source = 0
$second = Invoke-SampleInquiry                        # Cache, source = 1, a different inquiry ID
$replayKey = [guid]::NewGuid().ToString('N')
$bypass = Invoke-SampleInquiry -BypassCache $true -Key $replayKey
$replay = Invoke-SampleInquiry -BypassCache $true -Key $replayKey
$first, $second, $bypass, $replay
```

The bypass call uses a provider. Its replay returns the same inquiry ID and recorded result. Reusing its key with a changed reference or cache flag returns HTTP 409.

| Reference | Expected result with default provider order and a fresh key |
| --- | --- |
| `REF-SUCCESS` | HTTP 200, eligible. |
| `REF-INELIGIBLE` | HTTP 200, ineligible. Provider A gives a valid business response and processing stops. |
| `REF-PENDING` | HTTP 200, pending. Provider A gives a valid business response and processing stops. |
| `REF-FALLBACK` | Provider A reports a technical failure. Provider B returns an ineligible result. |
| `REF-TIMEOUT` | Provider A times out. Provider B returns an eligible result. |
| `REF-ALL-FAIL` | Both providers fail technically. The API returns HTTP 503 with `inquiry_providers_unavailable`. |

Use `-BypassCache $true` with a fresh key when inspecting provider attempts for a reference already in the cache. Missing or invalid input returns HTTP 400. A matching request still being processed returns HTTP 409 with `inquiry_in_progress`. A processing deadline that elapsed before a final result was saved returns HTTP 503 with `inquiry_interrupted`.

## Architecture and technical decisions

- **Web API and Application.** The controller forwards the request and cancellation token to MediatR. FluentValidation checks the reference. The query handler resolves idempotency and maps the persisted result to the response. The API exception filter maps expected errors to Problem Details.
- **Domain and Infrastructure.** The domain models the inquiry state and provider attempts. Infrastructure contains EF Core persistence, the provider adapters, provider routing, and the cache. The adapters translate different simulated response formats to a shared outcome and reason code.
- **Ordered failover.** The order of `InquiryRequestSettings:Providers` sets priority. Each provider has its own timeout within the overall processing deadline. Technical failures and timeouts can move processing to the next provider. A valid business response completes processing, including ineligible and pending outcomes.
- **Idempotency.** SQL Server enforces a unique index on `IdempotencyKey`. The SHA-256 request hash covers the normalized reference and cache flag. The insert grants processing ownership. A matching key and hash replay the persisted result once finalized. A different hash for the same key returns HTTP 409.
- **Persistence and transactions.** A factory creates a short-lived EF Core context for an isolated write. A provider attempt is committed before the provider call. The final attempt outcome and final inquiry state are saved in one transaction. A conditional update on the processing row serializes competing state changes. Provider calls execute outside database transactions. An expired inquiry can be finalized on replay, with any open attempt marked unknown.
- **Cache-aside.** The cache key is based on the normalized reference. Eligible and ineligible results have absolute TTLs of 60 and 10 seconds by default. A cache hit is persisted as a new inquiry with its own ID and no provider attempts. A fresh bypass request goes to a provider and refreshes a cacheable result after the database commit. Pending invalidates an older cached result. Technical failures leave an existing entry to expire.
- **Observability.** The repository logs the inquiry ID, provider key, attempt number, outcome, and elapsed time. The tables `Ayan.InquiryRequests` and `Ayan.InquiryProviderAttempts` provide the persistent audit trail.

## Assumptions

- The sample inquiry is read-only and uses simulated providers. Eligibility is the example business domain.
- Provider order and timeouts are configured in `WebApi/Server/appsettings.json`. The default overall deadline is five minutes. `EligibleCacheTtlSeconds` and `IneligibleCacheTtlSeconds` can be overridden under `InquiryRequestSettings`.
- Idempotency keys are globally unique in this sample. `IMemoryCache` is local to the API process, so a restart or another process starts with a different cache. Distinct keys can independently call a provider on a concurrent cache miss.
- Recovery of an interrupted request is triggered by replay. Provider calls and SQL commits have separate failure boundaries, so a stopped process can leave an attempt with an unknown outcome.

## Verification

The project built successfully against .NET 8. A separate integration run against SQL Server exercised business results, failover after a timeout, cache hit and expiry, bypass and refresh, idempotent replay, concurrent requests with the same key, and rollback of a failed completion. The smoke commands and scenario table above give a repeatable way to inspect the HTTP behavior.
