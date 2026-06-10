# Deploying PPT Revive

## Recommended setup (single VM + Docker Compose)

Because job state currently lives in the backend's in-memory cache and uploaded/finalized decks live on its local disk, the app must run as **one always-on backend instance**. A single small cloud VM is therefore the best fit today — simple, cheap (~$15–25/month), and matches how the app works. Serverless/auto-scaling platforms (Cloud Run, App Service with multiple instances, Kubernetes) will silently lose jobs until persistent job storage is implemented (see "Scaling later").

Everything needed is in this repository: `docker-compose.yml` runs MySQL, the backend, the frontend, and a Caddy reverse proxy that obtains **HTTPS certificates automatically**.

### Steps

1. **Create a VM** — e.g. GCP `e2-small`, AWS `t3.small`, DigitalOcean/Hetzner 2 GB. Ubuntu 22.04+. Open ports 80 and 443.
2. **Point DNS** — create A records for your two domains (e.g. `app.nucleusdigitalis.ai` and `api.nucleusdigitalis.ai`) to the VM's IP. Do this *before* starting, so Caddy can issue certificates.
3. **Install Docker** on the VM:
   ```bash
   curl -fsSL https://get.docker.com | sh
   ```
4. **Clone and configure**:
   ```bash
   git clone <your-repo-url> ppt-revive && cd ppt-revive
   cp .env.example .env
   nano .env        # fill in domains, keys, and generated secrets
   ```
   Generate secrets with `openssl rand -hex 32`.
5. **Launch**:
   ```bash
   docker compose up -d --build
   ```
   First start takes a few minutes (builds + DB migration/seed). Check health:
   ```bash
   docker compose ps
   docker compose logs -f backend
   curl https://api.<your-domain>/health
   ```
6. **Try it** — open `https://app.<your-domain>`, register, upload a small test deck, and walk a slide through approve → edit → finalize → download before announcing it.

### Updating to a new version

```bash
git pull && docker compose up -d --build
```
Note: any analysis jobs in progress at restart are lost (in-memory cache) — deploy during quiet hours.

### Backups

The two named volumes that matter are `mysql_data` (users/accounts) and `ppt_files` (decks, auto-purged daily by the app). Minimum viable backup:
```bash
docker compose exec mysql sh -c 'mysqldump -uroot -p"$MYSQL_ROOT_PASSWORD" ppt_revive' > backup-$(date +%F).sql
```
Schedule it with cron and copy the file off the VM.

## Alternatives considered

| Option | Verdict |
|---|---|
| **Single VM + Compose (this guide)** | ✅ Best fit for the current architecture. |
| Railway / Render / Fly.io (single instance + managed MySQL) | Fine alternative if you prefer no VM management; pick a plan that keeps **exactly one** always-on backend instance and attach a persistent volume for `wwwroot`. |
| Google Cloud Run / Azure Container Apps | ❌ Not yet — instances are recycled and scaled, which loses in-memory jobs and local files. Revisit after persistent job storage. |
| Vercel/Netlify for the frontend only | ✅ Optional refinement: host `frontend/` there (set `VITE_API_BASE_URL`) and keep only the API + MySQL on the VM. |

## Production checklist

- [ ] Old leaked keys revoked (done) and **new** keys used only in `.env` on the server
- [ ] `.env` not committed; file permissions `chmod 600 .env`
- [ ] DNS A records for both domains point at the VM; `https://` loads with a valid certificate
- [ ] `/health` returns healthy
- [ ] Test account registered; full upload → review → edit → finalize → download cycle verified with a real medical deck
- [ ] Database backup cron in place
- [ ] VM auto-updates enabled (`unattended-upgrades`) and SSH locked to key auth

## Scaling later

When you outgrow one instance, the prerequisite work (in priority order):

1. **Persist job state** in MySQL instead of `IMemoryCache` (Hangfire is already wired and is the natural home for the background analysis jobs).
2. **Move deck files to object storage** (GCS/S3) — an `S3FileRepository` skeleton already exists in `backend/src/Infrastructure/Repositories/`.
3. Then the backend becomes stateless and can run on Cloud Run/Container Apps with autoscaling, with the frontend on a CDN.
