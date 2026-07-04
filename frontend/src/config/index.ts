export const AppConfig = {
  STORAGE_KEY: import.meta.env.VITE_STORAGE_KEY,
  API_BASE_URL: import.meta.env.VITE_API_BASE_URL,
  GOOGLE_OAUTH_CLIENT_ID: import.meta.env.VITE_GOOGLE_OAUTH_CLIENT_ID,
  // Client-side upload ceilings — the largest deck a physician may upload.
  // Real lecture decks routinely exceed the old 20-slide / 5 MB caps, so these
  // are generous. The free tier still only revives FREE_TIER_SLIDES slides
  // (enforced server-side); these limits are purely about what can be uploaded.
  MAX_PPT_SLIDES: Number(import.meta.env.VITE_MAX_PPT_SLIDES || 100),
  FREE_TIER_SLIDES: Number(import.meta.env.VITE_FREE_TIER_SLIDES || 5),
  MAX_PPT_FILE_SIZE_MB: Number(import.meta.env.VITE_MAX_PPT_FILE_SIZE_MB || 25),
  // Where the "Join Founding Members" CTA sends prospects (swap for the live URL).
  FOUNDING_URL: import.meta.env.VITE_FOUNDING_URL || 'https://nucleusdigitalis.com',
};
