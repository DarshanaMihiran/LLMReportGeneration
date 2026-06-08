# AI Reporting System

AI Reporting System is a .NET 9 and React application that turns natural-language reporting prompts into validated report data, forecasts, and a written business summary.

The backend uses OpenAI for intent extraction, forecasting, and report writing, but it does not accept raw SQL from either the user or the model. OpenAI returns structured semantic JSON, and the API owns validation, authorization, SQL generation, execution, forecast checks, and response shaping.

## Features

- Natural-language report generation through `POST /api/ai-reports/generate`
- OpenAI-powered semantic query extraction
- Safe semantic validation for datasets, metrics, dimensions, filters, operators, and forecast periods
- Parameterized SQL generation with Dapper
- SQL Server reporting-view execution path
- Local sample-data execution path for development
- OpenAI-powered structured sales forecasting
- OpenAI-powered final business report writing from verified data
- React + Vite frontend
- Swagger UI
- xUnit backend tests

## Tech Stack

- .NET 9 Web API
- React 19
- Vite 6
- TypeScript
- SQL Server
- Dapper
- Microsoft.Data.SqlClient
- Swashbuckle / Swagger
- xUnit
- OpenAI Chat Completions API

## Repository Layout

```text
AiReporting.sln
README.md
plan.md

src/
  AiReporting.Api/
    Application/AiReports/
      Authorization/
      Execution/
      Forecasting/
      Intent/
      Orchestration/
      QueryBuilding/
      ReportWriting/
      SemanticModel/
      Validation/
    Controllers/
    Contracts/
    Infrastructure/
      Database/
      Errors/
      OpenAI/
    Program.cs
    appsettings.json
    appsettings.Development.json

  AiReporting.Web/
    src/
    package.json
    vite.config.ts
    README.md

tests/
  AiReporting.Api.Tests/

sql/
  reporting-setup.sql
```

## Architecture

```text
React UI
  -> POST /api/ai-reports/generate
  -> AiReportsController
  -> AiReportOrchestrator
  -> OpenAiReportIntentExtractor
  -> SemanticQueryValidator
  -> ReportAuthorizationService
  -> SqlQueryBuilder
  -> ReportingQueryExecutor or SampleReportingQueryExecutor
  -> OpenAiForecastService
  -> OpenAiReportWriter
  -> AiReportResponse
```

## Prerequisites

- .NET 9 SDK
- Node.js and npm
- SQL Server, only required when `Reporting:UseSampleData` is `false`
- OpenAI API key

## Configuration

The API configuration lives in `src/AiReporting.Api/appsettings.json`.

```json
{
  "ConnectionStrings": {
    "ReportingDb": "Server=.;Database=SalesDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Reporting": {
    "UseSampleData": false
  },
  "OpenAI": {
    "ApiKey": "",
    "Model": "gpt-4.1-mini"
  }
}
```

Development mode overrides `Reporting:UseSampleData` to `true` in `appsettings.Development.json`, so local API/UI testing does not require SQL Server data.

Set the OpenAI API key with user secrets:

```powershell
dotnet user-secrets set "OpenAI:ApiKey" "YOUR_KEY" --project src/AiReporting.Api
```

The committed configuration intentionally leaves `OpenAI:ApiKey` blank.

## Run Locally

Start the API:

```powershell
dotnet run --project src/AiReporting.Api --launch-profile http
```

Start the frontend:

```powershell
cd src/AiReporting.Web
npm install
npm run dev
```

Open the UI:

```text
http://localhost:5173
```

Open Swagger:

```text
http://localhost:5182/swagger
```

The frontend calls the API at `http://localhost:5182` by default. To use another API URL, create `src/AiReporting.Web/.env.local`:

```text
VITE_API_BASE_URL=http://localhost:5182
```

## API

### Generate Report

```http
POST /api/ai-reports/generate
Content-Type: application/json
```

Request:

```json
{
  "prompt": "Generate a monthly minor sales report for the previous year and predict next year sales."
}
```

Response:

```json
{
  "query": {
    "dataset": "sales_summary",
    "metrics": ["minor_sales"],
    "dimensions": ["month"],
    "filters": [
      {
        "field": "year",
        "operator": "=",
        "value": "2025"
      }
    ],
    "includeForecast": true,
    "forecastPeriod": "next year"
  },
  "data": [],
  "forecast": {
    "forecastYear": 2026,
    "predictedTotal": 123456.78,
    "growthRate": 0.08,
    "method": "OpenAI structured forecast from verified report data"
  },
  "report": "..."
}
```

Errors use this shape:

```json
{
  "error": "...",
  "traceId": "..."
}
```

## Semantic Model

Implemented dataset:

```text
sales_summary -> vw_sales_summary
```

Metrics:

- `total_sales` -> `TotalSales`
- `minor_sales` -> `MinorSales`
- `order_count` -> `OrderCount`

Dimensions:

- `year` -> `Year`
- `month` -> `Month`
- `region` -> `Region`
- `category` -> `Category`

Filters:

- `year`
- `month`
- `region`
- `category`

Allowed operators:

```text
=, >=, <=, >, <
```

## Forecast Periods

The backend accepts user-friendly forecast periods and resolves them into a concrete forecast year.

Examples:

- `next_year`
- `next year`
- `2026`
- `26`
- `2 years`
- `next 2 years`

The resolved forecast year must be after the source year and inside the supported forecast horizon.

## SQL Safety

The SQL builder:

- Uses semantic registry mappings only
- Uses mapped reporting view names only
- Uses `SELECT TOP 500`
- Aggregates metrics with `SUM(...)`
- Adds `GROUP BY` and `ORDER BY` for dimensions
- Uses Dapper `DynamicParameters`
- Never concatenates user filter values into SQL

Example generated SQL:

```sql
SELECT TOP 500
    [Month] AS [month],
    SUM([MinorSales]) AS [minor_sales]
FROM vw_sales_summary
WHERE [Year] = @p0
GROUP BY [Month]
ORDER BY [Month]
```

## Database Setup

The SQL setup script is:

```text
sql/reporting-setup.sql
```

It creates:

```text
dbo.vw_sales_summary
```

The script assumes these source tables exist:

- `dbo.Orders`
- `dbo.Customers`
- `dbo.Products`

It also includes commented SQL for creating a read-only reporter login/user and granting `SELECT` on the reporting view.

For production, use a read-only database user with access only to approved reporting views.

## Tests

Run backend tests:

```powershell
dotnet test tests/AiReporting.Api.Tests/AiReporting.Api.Tests.csproj
```

Build the frontend:

```powershell
cd src/AiReporting.Web
npm run build
```

If the API is already running and locks the default build output on Windows, run tests with an alternate output folder:

```powershell
dotnet test tests/AiReporting.Api.Tests/AiReporting.Api.Tests.csproj --no-restore -p:OutDir=D:\Repos\ReportLLM\.build\verify\
```

Existing backend test coverage includes:

- Semantic query validation
- Forecast period validation
- SQL query generation
- Forecast fallback calculation
- OpenAI forecast parsing and validation
- Sample query execution
- Authorization behavior
- Orchestrator pipeline behavior

## Production Notes

Recommended production work:

- Add real authentication
- Replace the demo user fallback with authenticated user IDs
- Add dataset, metric, and row-level authorization
- Add persistent audit logging
- Add token usage logging when available
- Add rate limiting
- Add request size limits
- Add more datasets
- Add chart-ready response data
- Add integration tests with a disposable SQL Server database
- Add CI workflow
- Add deployment configuration

## Current Status

The system is implemented end to end for the sales summary reporting scenario. It supports local development with sample data, SQL Server-backed reporting through a constrained reporting view, OpenAI-backed semantic extraction, OpenAI-backed forecasting, and OpenAI-backed report writing.
