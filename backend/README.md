# PPT Revive — Backend

ASP.NET Core 8 API (Clean Architecture: Domain → Application → Infrastructure → Web) that powers PPT Revive: PPTX parsing and regeneration via DocumentFormat.OpenXml, slide analysis with the Claude API (structured outputs), real medical-literature search via PubMed E-utilities, JWT auth with role/right management, and MySQL persistence.

See the repository root `README.md` for the full pipeline description, configuration reference, and setup instructions.

## Projects

| Project | Purpose |
|---|---|
| `src/Domain` | Entities, enums, constants |
| `src/Application` | MediatR commands/queries (PPT pipeline lives in `Application/PPT`), validators, interfaces |
| `src/Infrastructure` | EF Core (MySQL), Claude + PubMed integration (`Repositories/PubMedRepository.cs`), slide processing (`Services/ProcessPptJobService.cs`) |
| `src/Web` | Controllers (`PPTController`, `AuthController`, `UserController`, …), middleware, Swagger |

## PPT endpoints

| Endpoint | Method | Description |
|---|---|---|
| `/api/ppt/upload` | POST (multipart) | Upload a `.pptx`, returns `jobId` |
| `/api/ppt/start?JobId=` | GET | Kick off background analysis |
| `/api/ppt/status?JobId=` | GET | Poll job status / results |
| `/api/ppt/finalize-slides` | POST | `{ id, slides: [{ id, isApproved, editedContent? }] }` → builds `{jobId}-revived.pptx` with reference slides |
| `/api/ppt/regenerate` | POST | One-off Claude rewrite of a single slide's text |

## Build & test

```bash
dotnet build ppt-revive.sln
dotnet test ppt-revive.sln
dotnet run --project src/Web   # Swagger at /swagger
```

All secrets (connection string, JWT keys, `Claude__ApiKey`, `PubMed__ApiKey`) must come from environment variables or a secret store — `appsettings*.json` intentionally contains no credentials.

## Related

- Frontend: `../frontend` in this repository (originally `PPT-revive-frontend-claude`)
