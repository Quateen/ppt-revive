import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { AppConfig } from '@/config';
import { ApiResponse } from '@/api/apiTypes';
import { LoginResponse, AuthState } from './authTypes';
import {
  forgotPasswordAction,
  googleLoginAction,
  loginAction,
  registerAction,
  resetPasswordAction,
  updatePasswordAction,
  verifyEmailAction,
} from './authActions';
import { setCurrentUser } from '@/common/utils/userAttribs4mLocalStorage';


const initialState: AuthState = {
  status: 'idle',
  data: undefined,
  registerResponse: null,
  verifyEmailResponse: null,
  authError: null,
  updatePasswordResponse: null,
  forgotPasswordError: null,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    resetErrorAction: (state) => {
      state.authError = null;
    },
    resetRegisterResponse: (state) => {
      state.registerResponse = null;
    },
    resetVerifyEmailResponse: (state) => {
      state.verifyEmailResponse = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // 🔐 Login
      .addCase(loginAction.pending, (state) => {
        state.status = 'loading';
        state.authError = null;
      })
      .addCase(loginAction.fulfilled, (state, action: PayloadAction<ApiResponse<LoginResponse>>) => {
        state.status = 'idle';
        setCurrentUser(AppConfig.STORAGE_KEY, action.payload.data);
        state.data = action.payload.data;
      })
      .addCase(loginAction.rejected, (state, action) => {
        state.status = 'failed';
        state.authError = action.payload as ApiResponse<null>;
      });

    // 📝 Register
    builder
      .addCase(registerAction.pending, (state) => {
        state.status = 'loading';
        state.authError = null;
      })
      .addCase(registerAction.fulfilled, (state, action: PayloadAction<ApiResponse<number>>) => {
        state.status = 'idle';
        state.registerResponse = action.payload;
      })
      .addCase(registerAction.rejected, (state, action) => {
        state.status = 'failed';
        state.authError = action.payload as ApiResponse<null>;
      });

    // ✅ Verify Email
    builder
      .addCase(verifyEmailAction.pending, (state) => {
        state.status = 'loading';
        state.authError = null;
      })
      .addCase(verifyEmailAction.fulfilled, (state, action: PayloadAction<ApiResponse<null>>) => {
        state.status = 'idle';
        state.verifyEmailResponse = action.payload;
      })
      .addCase(verifyEmailAction.rejected, (state, action) => {
        state.status = 'failed';
        state.authError = action.payload as ApiResponse<null>;
      });

    // 🔓 Google Login
    builder
      .addCase(googleLoginAction.fulfilled, (state, action: PayloadAction<ApiResponse<LoginResponse>>) => {
        setCurrentUser(AppConfig.STORAGE_KEY, action.payload.data);
        state.data = action.payload.data;
        state.status = 'idle';
      })
      .addCase(googleLoginAction.rejected, (state, action) => {
        state.status = 'failed';
        state.authError = action.payload as ApiResponse<null>;
      });

    builder
      .addCase(updatePasswordAction.pending, (state) => {
        state.status = 'loading';
      })
      .addCase(updatePasswordAction.fulfilled, (state) => {
        state.status = 'idle';
        state.authError = null;
      })
      .addCase(updatePasswordAction.rejected, (state, action) => {
        state.status = 'failed';
        state.authError = action.payload as ApiResponse<null>;
      });

    builder
      .addCase(forgotPasswordAction.pending, (state) => {
        state.status = 'loading';
        state.forgotPasswordError = null;
      })
      .addCase(forgotPasswordAction.fulfilled, (state) => {
        state.status = 'idle';
      })
      .addCase(forgotPasswordAction.rejected, (state, action) => {
        state.status = 'failed';
        state.forgotPasswordError = action.payload as ApiResponse<null>;
      });

    builder
      .addCase(resetPasswordAction.pending, (state) => {
        state.status = 'loading';
        state.authError = null;
      })
      .addCase(resetPasswordAction.fulfilled, (state) => {
        state.status = 'idle';
      })
      .addCase(resetPasswordAction.rejected, (state, action) => {
        state.status = 'failed';
        state.authError = action.payload as ApiResponse<null>;
      });


  },
});

export const {
  resetErrorAction,
  resetRegisterResponse,
  resetVerifyEmailResponse,
} = authSlice.actions;

export default authSlice.reducer;

export const selectStatus = (state: { auth: AuthState }) => state.auth.status;
export const selectAuthError = (state: { auth: AuthState }) => state.auth.authError;
export const selectUser = (state: { auth: AuthState }) => state.auth.data;
export const selectRegisterResponse = (state: { auth: AuthState }) => state.auth.registerResponse;
export const selectVerifyEmailResponse = (state: { auth: AuthState }) => state.auth.verifyEmailResponse;
