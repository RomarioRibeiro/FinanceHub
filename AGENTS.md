# Repository Guidelines

## Project Structure & Module Organization
The solution entry point is `FinanceHub.sln`. The web application lives in `FinanceHub/` and follows a standard ASP.NET Core MVC layout:
- `Controllers/`: request handlers for each feature area (`Transacoes`, `Contas`, `LancamentosRecorrentes`, etc.).
- `Models/`: domain entities, enums, exceptions, and view models.
- `Services/`: business rules and orchestration.
- `Repositories/` and `Data/`: EF Core access, `FinanceHubContext`, generic repository, and unit of work.
- `Views/`: Razor views grouped by controller.
- `wwwroot/`: static CSS, JS, and vendor assets.
- `Migrations/`: EF Core schema history.
- `Tests/`: reserved folder; no test project is wired yet.

## Build, Test, and Development Commands
- `dotnet build .\FinanceHub.sln`: build the full solution.
- `dotnet run --project .\FinanceHub\FinanceHub.csproj`: start the app locally.
- `dotnet watch run --project .\FinanceHub\FinanceHub.csproj`: run with hot reload during UI and controller work.
- `dotnet ef database update --project .\FinanceHub\FinanceHub.csproj --startup-project .\FinanceHub\FinanceHub.csproj`: apply migrations.

Use SQL Server for local development; the connection string is read from `FinanceHub/appsettings*.json`.

## Coding Style & Naming Conventions
Use 4-space indentation and keep nullable reference types enabled. Follow existing C# naming:
- `PascalCase` for classes, methods, enums, properties, and Razor view folders.
- `camelCase` for locals and parameters.
- Suffix services with `Service`, repositories with `Repository`, and controllers with `Controller`.

Prefer thin controllers and keep business rules in `Services/`. Reuse the existing decimal model binder for money inputs instead of ad hoc parsing.

## Testing Guidelines
There is no active automated test project yet. For now, validate changes with:
- `dotnet build`
- targeted manual checks in the affected MVC flow
- migration verification when schema changes are involved

When adding tests, create a dedicated test project under `Tests/` and name files after the target class, for example `TransacaoServiceTests.cs`.

## Commit & Pull Request Guidelines
Recent history uses short, imperative Portuguese commit messages, for example `Ajusta recorrencias, saldos e transacoes`. Keep that style and scope each commit to one logical change.

PRs should include:
- a direct summary of the functional change
- affected screens or modules
- migration notes, if any
- screenshots for Razor UI changes
- manual validation steps performed

## Security & Configuration Tips
Do not commit real credentials. Move local secrets out of `appsettings.json` when possible. Any admin-only controller must be protected explicitly with role-based authorization; authenticated access alone is not enough for sensitive CRUD areas.
