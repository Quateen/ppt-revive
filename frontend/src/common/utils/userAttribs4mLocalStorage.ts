import { UserDto } from "@/app-redux/auth/authTypes";

// --- Generic get from sessionStorage ---
export function getCurrentUser<T = UserDto>(key: string): T | null {
  const attribs = window.sessionStorage.getItem(key);
  if (!attribs) return null;

  try {
    return JSON.parse(attribs) as T;
  } catch {
    return null;
  }
}

// --- Remove from sessionStorage ---
export function removeCurrentUser(key: string): void {
  window.sessionStorage.removeItem(key);
}

// --- Save to sessionStorage ---
export function setCurrentUser<T = UserDto>(key: string, data: T): void {
  window.sessionStorage.setItem(key, JSON.stringify(data));
}
