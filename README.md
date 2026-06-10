# PPT Revive v2

AI-powered medical presentation updater. Upload an old medical PowerPoint, let Claude analyze each slide against current PubMed literature, review every suggested update side-by-side (approve / reject / **edit**), and download a refreshed `.pptx` with a compiled references slide for the approved changes.

This is a monorepo combining a hardened version of the original `PPT-revive-backend-claude` (.NET 8) and `PPT-revive-frontend-claude` (React + Vite) repositories. **The original repositories were not modified.**

```
ppt-revive-v2/
├── backend/    ASP.NET Core 8 API — PPTX parsing (OpenXML), Claude analysis,
│               PubMed search, slide regeneration, JWT auth, MySQL
└── frontend/   React 18 + Vite + Redux Toolkit — upload, side-by-side review,
                approve/reject/edit, references list, download
```

## How the pipeline works

1. **Upload** (`POST /api/ppt/upload`) — validates a `.pptx` (max 5 MB), stores it, returns a `jobId`.
2. **Analyze** (`GET /api/ppt/start`, polled via `GET /api/ppt/status`) — for each slide (3 in parallel by default):
   - Claude extracts the key medical terms from the slide text.
   - Those terms are searched on **PubMed** (NCBI E-utilities, rate-limit aware); up to 8 real articles with PMID/DOI/links are retrieved.
   - Claude rewrites the slide lines **using only the retrieved abstracts** (structured-output JSON, numeric-integrity guard against invented statistics).
3. **Review** — the frontend shows original vs. suggested content per slide with the reason for the change and its citations. The user approves, rejects, or edits each slide.
4. **Finalize** (`POST /api/ppt/finalize-slides`) — approved/edited text is written back into the *original* PPTX in place (bullet levels and run formatting preserved; images/layout untouched), reference slides are appended containing the citations of approved slides (with live hyperlinks), and a per-job output file `{jobId}-revived.pptx` is produced for download.

## What was fixed vs. the original repositories

- **Compile error** in `PubMedRepository.CleanJsonResponse` (stray character + missing semicolon) — the previous backend HEAD did not build.
- **Secrets scrubbed**: the original repo committed live OpenAI/Gemini/PubMed keys, a MySQL password, and JWT secrets in `appsettings*.json`. All are now empty and must be supplied via environment variables. **Rotate the old keys — they remain in the old repo's git history.**
- **Claude integration modernized**: model is configurable (`Claude:Model`, default `claude-opus-4-8`); responses use **structured outputs** (JSON schema) instead of regex-cleaning free-form text; removed sampling parameters that current models reject.
- **Concurrent-user bug**: finalized decks were all written to one shared filename — now per-job.
- **PubMed search quality**: the hardcoded `"OR bone fractures OR …"` orthopedic enrichment is gone; the Claude-extracted key terms are now actually used as the query (previously extracted and discarded).
- **Per-slide citations**: each slide now carries only its own supporting articles (previously a shared, growing list was attached to every slide), and the final references slides include only citations from approved slides.
- **Bounded parallelism + thread-safe collections** in slide processing (configurable via `Processing:MaxParallelSlides`).
- **Edit feature implemented end-to-end**: the UI's Edit button (previously commented out) lets the user modify the suggested text; `finalize-slides` accepts `editedContent` per slide.
- **Browser-side AI removed**: the frontend no longer bundles Anthropic/OpenAI SDK calls or `VITE_*_API_KEY` variables — all AI/PubMed traffic goes through the backend.
- **CORS origins** moved from hardcoded values into configuration.
- Abandoned GPT-4 regenerate endpoint rewritten to use Claude; dead code and duplicate files removed; vulnerable AutoMapper upgraded.

## Running locally

### Backend (`backend/`)

Requirements: .NET 8 SDK, MySQL 8.

```bash
cd backend
# Configuration via environment variables (double underscore = section separator):
export ConnectionStrings__DefaultConnection="server=127.0.0.1;port=3306;user=root;password=...;database=ppt_revive;Connection Timeout=60;Allow User Variables=True;"
export Jwt__AdminSecretKey="<generate a long random string>"
export Jwt__SecretKey="<generate a long random string>"
export Claude__ApiKey="sk-ant-..."
export PubMed__ApiKey="<NCBI API key>"   # free: https://www.ncbi.nlm.nih.gov/account/
dotnet run --project src/Web
```

Swagger UI is served at `/swagger`. The schema in `migration 18-06-25.sql` seeds the user/role/subscription tables.

### Frontend (`frontend/`)

Requirements: Node 18+.

```bash
cd frontend
cp .env.example .env    # set VITE_API_BASE_URL to the backend URL
npm install
npm run dev
```

## Configuration reference (backend)

| Key | Purpose | Default |
|---|---|---|
| `Claude:ApiKey` | Anthropic API key (required) | — |
| `Claude:Model` | Claude model for analysis & keyword extraction | `claude-opus-4-8` |
| `PubMed:ApiKey` | NCBI E-utilities key (raises rate limit to 10 req/s) | — |
| `Processing:MaxParallelSlides` | Concurrent slide analyses per job | `3` |
| `Cors:AllowedOrigins` | Frontend origins allowed to call the API | localhost |
| `FileUploadSettings:MaxFileSizeMB` | Upload size cap | `5` |
| `Jwt:AdminSecretKey` / `Jwt:SecretKey` | JWT signing keys (required, app refuses to start without) | — |

## Adding Scopus later

Literature search is encapsulated in `backend/src/Infrastructure/Repositories/PubMedRepository.cs` behind the `IPubMedRepository` interface (`SearchRelevantArticlesAsync`). To add Scopus, implement an equivalent search against the Elsevier API (`https://api.elsevier.com/content/search/scopus`, requires an institutional API key), merge its results into the article list passed to `AnalyzeMedicalSlideAsync`, and include the Scopus EID/DOI link in the citation line. No frontend change is needed — citations flow through the same `references` field.

## Known limitations / next steps

- **Job state lives in an in-memory cache (2 h TTL)** — an app restart loses in-flight jobs. Replace with a database-backed job table (Hangfire is already wired) before scaling beyond one instance.
- Slide regeneration is text-only: images, charts, and complex layouts are preserved as-is but not analyzed. Claude vision analysis of slide images is a natural next step.
- Reference slides use a simple generated layout rather than the deck's theme.
- Theme switching for the output deck is not yet implemented.
- Test suite still covers only legacy scaffolding; the PPT pipeline has no automated tests yet.

## Medical disclaimer

Suggested updates are AI-generated from retrieved literature and must be verified by a qualified medical professional before use. Every suggestion links to its source (PubMed PMID/DOI) for verification. Avoid uploading presentations containing patient-identifiable information.
