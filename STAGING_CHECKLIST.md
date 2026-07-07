# PPT-Revive — Staging Run & Go-Live Checklist

The goal of a staging run is to prove the app works **end-to-end on real infrastructure
with a real deck** before it fronts prospects at `revive.nucleusdigitalis.com`. Do this on a
throwaway **staging** subdomain first; only point the real domain at it once every box below
is checked.

The one thing that can only be verified live (not by build/type checks) is **slide fidelity**
— that the downloaded `.pptx` looks right, especially multi-textbox slides. Section 7 is the
critical check; don't skip it.

---

## 1. Prerequisites — gather before you start

- [ ] A VM you can SSH into (Ubuntu 22.04+, 2 GB RAM min; e.g. GCP e2-small / Hetzner CX22). Ports **80** and **443** open.
- [ ] Two DNS names you control, e.g. **staging.revive.nucleusdigitalis.com** (app) and **api.staging.revive.nucleusdigitalis.com** (API).
- [ ] **Anthropic API key** (`sk-ant-…`) — console.anthropic.com. Confirm it has credit.
- [ ] **NCBI / PubMed API key** — free at ncbi.nlm.nih.gov/account → Settings → API Key (raises the rate limit to 10 req/s; without it you're at 3 req/s and may see thin results).
- [ ] **Mailchimp** API key (ends in `-usXX`) and **Audience (List) ID** — Mailchimp → Audience → Settings → "Audience name and defaults".
- [ ] Two long random strings for JWT (`openssl rand -hex 32` each) and one for MySQL (`openssl rand -hex 16`).
- [ ] The real **FOUNDING_URL** the "Join Founding Members" CTA should point at (a landing/waitlist/Stripe/Calendly page). Until you have it, it defaults to `https://nucleusdigitalis.com`.
- [ ] *(Optional)* Google OAuth **Client ID** if you want the "Sign in with Google" button; and a **Plausible** domain if you want analytics.
- [ ] A **test deck** (`.pptx`) — see Section 7 for what it should contain.

> All secrets live only in the `.env` on the server. Never commit them. The keys leaked in the
> original repos have already been rotated — keep it that way.

---

## 2. Provision the staging server

```bash
ssh you@STAGING_VM_IP
curl -fsSL https://get.docker.com | sh          # installs Docker + compose plugin
sudo usermod -aG docker $USER && exit           # re-login so docker runs without sudo
ssh you@STAGING_VM_IP
```

- [ ] Point the two DNS **A records** at the VM's public IP **now** (before first launch — Caddy needs them resolvable to issue certificates). Verify: `dig +short staging.revive.nucleusdigitalis.com` returns the VM IP.

```bash
git clone <your-repo-url> ppt-revive && cd ppt-revive
git checkout claude/ppt-revive-nucleus-v3    # the current version
```

---

## 3. Configure secrets

```bash
cp .env.example .env
nano .env
```

Fill in (see `.env.example` for the full annotated list):

- [ ] `APP_DOMAIN=staging.revive.nucleusdigitalis.com`
- [ ] `API_DOMAIN=api.staging.revive.nucleusdigitalis.com`
- [ ] `MYSQL_ROOT_PASSWORD=` (generated)
- [ ] `JWT_ADMIN_SECRET_KEY=` / `JWT_SECRET_KEY=` (two different generated strings)
- [ ] `CLAUDE_API_KEY=sk-ant-…`   (optional `CLAUDE_MODEL`, default `claude-opus-4-8`)
- [ ] `PUBMED_API_KEY=`
- [ ] `MAILCHIMP_API_KEY=` and `MAILCHIMP_AUDIENCE_ID=`
- [ ] `FOUNDING_URL=` (your real CTA target)
- [ ] `FREE_TIER_SLIDES=5` · `MAX_PPT_SLIDES=100` · `MAX_UPLOAD_MB=25` (defaults are fine)
- [ ] *(optional)* `GOOGLE_OAUTH_CLIENT_ID=` · `PLAUSIBLE_DOMAIN=` · `SMTP_*` (only needed if you enable account signup/email)
- [ ] `chmod 600 .env`

---

## 4. Deploy

```bash
docker compose up -d --build      # first build takes a few minutes
docker compose ps                 # all services "running"/"healthy"
docker compose logs -f backend    # watch for "Now listening on…" and DB seeding
```

- [ ] All four services (`mysql`, `backend`, `frontend`, `caddy`) are up.
- [ ] In the backend logs, note the seeded admin password line: `[Seed] Generated admin password for … : <password>` — save it (only needed if you'll test the account/login area).

---

## 5. Smoke tests (infrastructure)

- [ ] **API health:** `curl https://api.staging.revive.nucleusdigitalis.com/health` → `Healthy`.
- [ ] **HTTPS/cert:** both domains load with a valid Let's Encrypt certificate (no browser warning).
- [ ] **Landing renders branded:** open `https://staging.revive.nucleusdigitalis.com` — warm-cream/teal theme, "Your 2019 slides…" hero, nucleus logo, Zodiak/General Sans fonts, founder line, footer with `NucleusDigitalis.com`. Browser tab shows the nucleus favicon and the branded title.
- [ ] **No dev slug / Lovable script:** View Source → `<title>` is the ND title; `grep`-check there is no `gpteng.co` reference.
- [ ] **Share preview:** paste the URL into Slack/LinkedIn/iMessage → the OG card shows the ND title + description (not a bare URL).

---

## 6. The core demo flow (run it as a physician would)

- [ ] Open the landing page in a **fresh incognito window** (proves the free tier needs **no login**).
- [ ] Read the upload box: the PHI/privacy advisory and "we'll revive the first 5 slides free" are present.
- [ ] Upload your test deck → click **Upload**, then **Revive my slides**.
- [ ] **Progress bar:** during analysis you see "Reviewing slide X of N…" advancing, not a bare spinner.
- [ ] You land on the review page: a **"You are the physician of record"** attestation banner is shown.
- [ ] Each slide shows **Original vs. Suggested** side-by-side with a reason and a **Sources** panel; the references sidebar lists citations.
- [ ] Test all three actions: **Approve** one, **Edit** one (change the text, save), **Reject** one.
- [ ] A slide with no relevant literature shows the calm **"No newer evidence found — current content looks up to date"** state (not a blank panel).
- [ ] Approve enough slides, click **Generate**, and the file downloads automatically.
- [ ] After download, the **post-download conversion CTA** appears ("Want this across your whole practice?").

---

## 7. Verify the downloaded deck — the critical fidelity check

Prepare the test deck so it exercises the risky cases. Include at least:
1. A **two-column / multi-textbox** slide (two separate text boxes with bullets).
2. A slide with **many bullets** (8–12) at mixed indent levels.
3. A slide with an **image, chart, or table** plus some text.
4. A slide with **numeric stats** in a sentence.
5. A slide with **no medical content** (e.g. a "Thank you" slide).

Open the downloaded `<yourdeck>-revived.pptx` in PowerPoint (or LibreOffice Impress) and verify:

- [ ] **Multi-textbox slide:** each text box was updated **in its own place** — text did **not** all collapse into one box, and the other box's original text is **not** duplicated or stale. *(This is the #1 thing the live run exists to confirm.)*
- [ ] **Bulleted slide:** bullets stayed as separate bullets at their original indent levels — **no run-on paragraph**.
- [ ] **Image/chart slide:** the image/chart/table is **untouched**; only text changed.
- [ ] **Formatting preserved:** fonts, sizes, colors, and bullet styles look like the original.
- [ ] **Stat slide:** numbers were only changed if the evidence supported it (no invented figures).
- [ ] **Every slide is footered** `NucleusDigitalis.com` (bottom-right, subtle).
- [ ] A **References slide** was appended, titled "References — evidence current as of \<month year\>", with clickable citation links.
- [ ] A **closing attribution slide** ("Updated with current evidence … revive.nucleusdigitalis.com") is the last slide.
- [ ] **File → Properties** shows Author/Creator "PPT-Revive by Nucleus Digitalis".
- [ ] The file opens **without a repair prompt** (confirms no corrupt XML).

> If anything here looks wrong, capture the deck and the slide number and send it over — this is
> exactly the feedback needed to finish the fidelity work.

---

## 8. Verify the funnel (lead capture → Mailchimp)

- [ ] On the landing conversion band, enter a **test email** → click **Join Founding Members**. It should confirm ("You're on the list") and open your `FOUNDING_URL` in a new tab.
- [ ] Repeat from the **post-download** CTA with a second test email.
- [ ] In **Mailchimp → Audience → Contacts**, both emails appear, tagged `ppt-revive:landing` and `ppt-revive:post-download`.
- [ ] *(Backup record)* `docker compose logs backend | grep LEAD_CAPTURE` shows both emails (the system-of-record even if Mailchimp is down).

---

## 9. Verify trust & evidence

- [ ] Open a citation link from a Sources/References entry — it resolves to a **real PubMed/DOI page** matching the claim (not a fabricated reference).
- [ ] The **medical disclaimer** appears in the footer and the attestation banner on the review page.
- [ ] Copy reads **anti-hype** and physician-led — no overclaiming, no "revolutionary AI" language.

---

## 10. Verify security

- [ ] **Swagger is NOT public:** `curl -i https://api.staging.revive.nucleusdigitalis.com/swagger` → 404/redirect, **not** the Swagger UI (it's dev-only now).
- [ ] **API root is closed:** `https://api.staging…/` does not redirect to Swagger in production.
- [ ] **No secrets in the bundle:** in the browser, view the built JS — search for `sk-ant`, `sk-proj`, your Mailchimp key → **none present** (all AI/PubMed/Mailchimp calls are server-side).
- [ ] **Rate limiting works:** rapidly refresh/hammer an endpoint (e.g. `for i in $(seq 1 30); do curl -s -o /dev/null -w "%{http_code}\n" https://api.staging…/api/ppt/status?JobId=x; done`) → some `429`s appear after the per-minute limit.
- [ ] **Download is scoped:** the download URL only works for a valid job id from your session; a random `jobId` returns not-found.
- [ ] *(If Google login enabled)* signing in with Google works and `GoogleAuth:ClientId` is set (a forged token is rejected).
- [ ] Security headers present: `curl -sI https://staging.revive…/ | grep -i "strict-transport\|content-security\|x-frame"` shows HSTS, CSP, X-Frame-Options.

---

## 11. Verify the raised limits

- [ ] Upload a **large real lecture** (e.g. 30+ slides, 10–20 MB). It should be **accepted** (not rejected at the door), and only the **first 5 slides** are revived; the rest pass through unchanged in the download.

---

## 12. Analytics (only if you set `PLAUSIBLE_DOMAIN`)

- [ ] In Plausible, live traffic shows during your test.
- [ ] A **"Join Founding Members"** custom event fires when you submit the email form.

---

## 13. Go / No-Go gate

Ship to the real domain only when **all** are true:
- [ ] Full demo flow works in incognito with no login (Sections 6).
- [ ] The downloaded deck passes **every** fidelity check (Section 7) — especially multi-textbox.
- [ ] Leads reach Mailchimp (Section 8).
- [ ] Citations are real and the disclaimer/attestation show (Section 9).
- [ ] Swagger is closed, no secrets in the bundle, rate limiting works (Section 10).
- [ ] You captured the seeded admin password and stored it safely.

---

## 14. Point the real domain live

- [ ] Change `.env`: `APP_DOMAIN=revive.nucleusdigitalis.com`, `API_DOMAIN=api.revive.nucleusdigitalis.com`; set the production `FOUNDING_URL`; switch Mailchimp to the **real** audience if staging used a test one.
- [ ] Update DNS A records for the production names to the (production) VM IP.
- [ ] `docker compose up -d --build`; re-run Sections 5 and a quick 6+7 on production.
- [ ] Schedule a database backup cron (see `DEPLOYMENT.md`) and enable `unattended-upgrades` on the VM.

---

## 15. Rollback

- [ ] To revert code: `git checkout claude/ppt-revive-nucleus-v2 && docker compose up -d --build` (or any prior version tag/branch — they're all preserved).
- [ ] Jobs live in memory (2-hour TTL) and are lost on restart — deploy during quiet hours; a redeploy only interrupts in-flight analyses, not captured leads (those are in Mailchimp + the DB logs).
- [ ] `docker compose down` stops everything without deleting the named volumes (`mysql_data`, `ppt_files`).

---

### Known limitations to keep in mind during the run
- Job state is an in-memory cache (2 h TTL, single instance) — fine for launch; move to a DB-backed queue before scaling to multiple instances.
- Slide analysis is text-only (images/charts preserved but not analyzed).
- The free tier caps at 5 revived slides by design (the wedge).
