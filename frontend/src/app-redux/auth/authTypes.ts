import { ApiResponse } from "@/api/apiTypes";

export interface AuthState {
  status: 'idle' | 'loading' | 'failed';
  data?: UserDto;
  authError?: ApiResponse<null> | null;
  registerResponse?: ApiResponse<number> | null;
  verifyEmailResponse?: ApiResponse<null> | null;
  updatePasswordResponse?: ApiResponse<null> | null;
    forgotPasswordError: ApiResponse<null> | null;
}

export interface ApiErrorDetails {
  error?: string;
  errors?: string[];
  message?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  ipAddress: string;
  clientInformation: string;
  currentBrowserTimeZone: string;
}

export interface LoginResponse {
  isSuccess: boolean;
  userId: number;
  name: string;
  email: string;
  sessionId: number;
  accessToken: string;
  refreshToken: string;
  expiryDate: string;
  enforceEmailConfirmation: boolean;
  enforceMobileConfirmation: boolean;
  enforce2FactorConfiguration: boolean;
  enforce2FactorVerification: boolean;
  enforcePasswordChangeOnFirstLogin: boolean;
  enforceProfileCompletion: boolean;
  roleId: number;
  rights: unknown[];
}

export interface UserDto {
  userId: number;
  name: string;
  email: string;
  sessionId: number;
  accessToken: string;
  refreshToken: string;
  expiryDate: string; // Use `Date` type if you plan to parse it immediately
}

// For Register
export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}


export interface GoogleLoginRequest {
  socialLogin: string;
  givenName: string;
  familyName: string;
  googleName: string;
  googleLocal: string;
  googlePicture: string;
  googleAud: string;
  googleAzp: string;
  googleExp: string;
  googleIat: string;
  googleIss: string;
  googleSub: string;
  email: string;
  idToken: string;
  deviceToken: string;
  currentBrowserTimeZone: string;
}



export interface PasswordForm  {
    oldPassword: string;
    newPassword: string;
};