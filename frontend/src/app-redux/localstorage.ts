import { getCurrentUser } from "@/common/utils/userAttribs4mLocalStorage";
import { AppConfig } from "@/config";
import { AuthState } from "./auth/authTypes";

export const loadState = (): { auth: AuthState } | undefined => {
  try {
    const user = getCurrentUser(AppConfig.STORAGE_KEY);
    if (!user) return undefined;

    return {
      auth: {
        status: 'idle',
        data: user,
        registerResponse: null,
        verifyEmailResponse: null,
        authError: null,
      },
    };
  } catch (error) {
    console.error("loadState error", error);
    return undefined;
  }
};
