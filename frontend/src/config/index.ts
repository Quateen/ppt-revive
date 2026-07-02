export const AppConfig = {
  STORAGE_KEY: import.meta.env.VITE_STORAGE_KEY,
  API_BASE_URL: import.meta.env.VITE_API_BASE_URL,
  GOOGLE_OAUTH_CLIENT_ID: import.meta.env.VITE_GOOGLE_OAUTH_CLIENT_ID,
  MAX_PPT_SLIDES: Number(import.meta.env.VITE_MAX_PPT_SLIDES || 20),
  MAX_PPT_FILE_SIZE_MB: Number(import.meta.env.VITE_MAX_PPT_FILE_SIZE_MB || 5),
};
