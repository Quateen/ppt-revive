export const AppConfig = {
  STORAGE_KEY: import.meta.env.VITE_STORAGE_KEY,
  API_BASE_URL: import.meta.env.VITE_API_BASE_URL,
  GOOGLE_OAUTH_CLIENT_ID: import.meta.env.VITE_GOOGLE_OAUTH_CLIENT_ID,
  // Free lead-magnet tier: revive up to this many slides with no account.
  MAX_PPT_SLIDES: Number(import.meta.env.VITE_MAX_PPT_SLIDES || 20),
  FREE_TIER_SLIDES: Number(import.meta.env.VITE_FREE_TIER_SLIDES || 5),
  MAX_PPT_FILE_SIZE_MB: Number(import.meta.env.VITE_MAX_PPT_FILE_SIZE_MB || 5),
  // Where the "Join Founding Members" CTA sends prospects (swap for the live URL).
  FOUNDING_URL: import.meta.env.VITE_FOUNDING_URL || 'https://nucleusdigitalis.com',
};
