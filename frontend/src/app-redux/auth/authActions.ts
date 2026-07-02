import { createAsyncThunk } from '@reduxjs/toolkit';
import { AxiosError } from 'axios';
import {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  GoogleLoginRequest,
  PasswordForm,
} from './authTypes';
import { RequestType, sendRequest } from '@/api/apiClient';
import { ApiResponse } from '@/api/apiTypes';

export const loginAction = createAsyncThunk<ApiResponse<LoginResponse>, LoginRequest>(
  'auth/loginAction',
  async (loginData, { rejectWithValue }) => {
    try {
      const res = await sendRequest<LoginResponse>(RequestType.POST, 'api/Auth/login', loginData);
      return res;
    } catch (err) {
      const axiosError = err as AxiosError<ApiResponse<null>>;
      return rejectWithValue(axiosError.response?.data ?? { status: false, message: 'Unknown error', data: null });
    }
  }
);

export const registerAction = createAsyncThunk<ApiResponse<number>, RegisterRequest>(
  'auth/registerAction',
  async (registerData, { rejectWithValue }) => {
    try {
      const res = await sendRequest<number>(RequestType.POST, 'api/Auth/register', registerData);
      return res;
    } catch (err) {
      const axiosError = err as AxiosError<ApiResponse<null>>;
      return rejectWithValue(axiosError.response?.data ?? { status: false, message: 'Unknown error', data: null });
    }
  }
);

export const verifyEmailAction = createAsyncThunk<ApiResponse<null>, { userId: number; code: string }>(
  'auth/verifyEmailAction',
  async (payload, { rejectWithValue }) => {
    try {
      const res = await sendRequest<null>(RequestType.POST, 'api/Auth/verify-email', payload);
      return res;
    } catch (err) {
      const axiosError = err as AxiosError<ApiResponse<null>>;
      return rejectWithValue(axiosError.response?.data ?? { status: false, message: 'Unknown error', data: null });
    }
  }
);

export const googleLoginAction = createAsyncThunk<ApiResponse<LoginResponse>, GoogleLoginRequest>(
  'auth/googleLoginAction',
  async (payload, { rejectWithValue }) => {
    try {
      const res = await sendRequest<LoginResponse>(RequestType.POST, 'api/Auth/google-login', payload);
      return res;
    } catch (err) {
      const axiosError = err as AxiosError<ApiResponse<null>>;
      return rejectWithValue(axiosError.response?.data ?? { status: false, message: 'Google login failed', data: null });
    }
  }
);

export const updatePasswordAction = createAsyncThunk<
  ApiResponse<null>,
  PasswordForm
>(
  'auth/updatePasswordAction',
  async (payload, { rejectWithValue }) => {
    try {
      const res = await sendRequest<null>(
        RequestType.POST,
        'api/Auth/change-password',
        payload
      );
      return res;
    } catch (err) {
      const axiosError = err as AxiosError<ApiResponse<null>>;
      return rejectWithValue(
        axiosError.response?.data ?? {
          status: false,
          message: 'Password update failed',
          data: null,
        }
      );
    }
  }
);

export const forgotPasswordAction = createAsyncThunk<
  ApiResponse<{ userId: number; token: string }>,
  { email: string }
>(
  'auth/forgotPasswordAction',
  async (payload, { rejectWithValue }) => {
    try {
      const res = await sendRequest<{ userId: number; token: string }>(
        RequestType.POST,
        'api/Auth/forget-password',
        payload
      );
      return res;
    } catch (err) {
      const axiosError = err as AxiosError<ApiResponse<null>>;
      return rejectWithValue(
        axiosError.response?.data ?? {
          status: false,
          message: 'Forgot password failed',
          data: null,
        }
      );
    }
  }
);

export const resetPasswordAction = createAsyncThunk<
  ApiResponse<null>,
  { userId: number; token: string; password: string }
>(
  'auth/resetPasswordAction',
  async (payload, { rejectWithValue }) => {
    try {
      const res = await sendRequest<null>(RequestType.POST, 'api/Auth/reset-password', payload);
      return res;
    } catch (err) {
      const axiosError = err as AxiosError<ApiResponse<null>>;
      return rejectWithValue(
        axiosError.response?.data ?? {
          status: false,
          message: 'Reset failed',
          data: null,
        }
      );
    }
  }
);


