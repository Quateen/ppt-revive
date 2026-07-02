# PPT Revive — Frontend

React 18 + Vite + TypeScript + Redux Toolkit frontend for **PPT Revive**.

Users upload an old medical PowerPoint (.pptx). The .NET backend analyzes each
slide (Claude + PubMed) and the UI shows the original and the suggested update
side by side, with citations. The user can **approve**, **reject**, or **edit**
each suggested slide. Finalizing generates a new PPTX (including a references
slide) that the user downloads.

> **Security note:** ALL AI and PubMed calls happen on the backend.
> The frontend holds **no API keys** — it only talks to the backend API at
> `VITE_API_BASE_URL`.

## Setup

```bash
# 1. Install dependencies (Node 22 recommended)
npm install

# 2. Configure environment variables
cp .env.example .env
# then edit .env (backend URL, Google OAuth client ID, etc.)

# 3. Start the dev server
npm run dev
```

### Environment variables

See `.env.example` for the full list with comments:

| Variable | Description |
| --- | --- |
| `VITE_API_BASE_URL` | Base URL of the .NET backend API |
| `VITE_GOOGLE_OAUTH_CLIENT_ID` | Google OAuth client ID for sign-in |
| `VITE_STORAGE_KEY` | localStorage key for the auth session |
| `VITE_MAX_PPT_SLIDES` | Max slides per upload (default 20) |
| `VITE_MAX_PPT_FILE_SIZE_MB` | Max upload size in MB (default 5) |

## Scripts

```bash
npm run dev      # start Vite dev server
npm run build    # production build (output in dist/)
npm run lint     # run ESLint
npm run preview  # preview the production build
```

## Review flow

1. Upload a `.pptx` → backend returns a job ID and processes the slides.
2. Review each slide: **Approve**, **Reject**, or **Edit** (modify the
   suggested text — edited slides count as approved).
3. When every slide is reviewed, **Generate Updated Presentation** sends
   `POST /api/PPT/finalize-slides` with
   `{ id, slides: [{ id, isApproved, editedContent }] }` and downloads the
   resulting PPTX.
