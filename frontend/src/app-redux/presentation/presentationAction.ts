// Updated presentationAction.ts
import { createAsyncThunk } from '@reduxjs/toolkit';
import { RequestType, sendRequest } from '@/api/apiClient';
import { AxiosError } from 'axios';
import {
  FinalizeSlideRequest,
  FinalizeSlideResponse,
  GetPptStatusData,
  PptStartProcessingData,
  UploadResponse,
} from './presentationTypes';
import { ApiResponse } from '@/api/apiTypes';

const handleError = (err: unknown): ApiResponse => {
  if (err instanceof AxiosError) {
    const apiError = err.response?.data;
    const statusCode = err?.response?.status;
    return {
      // Backend ResponseBase returns failure text in `error`, not `message`.
      message: apiError?.message || apiError?.error || "Something went wrong",
      error: apiError?.error || null,
      status: false,
      statusCode: statusCode
    };
  }
  return { message: "Unknown error", error: "Unhandled exception", status: false };
};


export const uploadPresentationAction = createAsyncThunk<
  ApiResponse<UploadResponse>,        // 👈 expected return type: data: { jobId: string }
  FormData,
  { rejectValue: ApiResponse<null> }
>(
  "presentation/upload",
  async (formData, { rejectWithValue }) => {
    try {
      const res = await sendRequest<string>(
        RequestType.POST,
        "api/PPT/upload",
        formData
      );

      // Wrap raw jobId string into expected format
      const wrapped: ApiResponse<UploadResponse> = {
        ...res,
        data: { jobId: res.data },
      };

      return wrapped;
    } catch (err) {
      return rejectWithValue(handleError(err) as ApiResponse<null>);
    }
  }
);



export const startPresentationAnalyzeAction = createAsyncThunk<
  ApiResponse<PptStartProcessingData>,
  { jobId: string },
  { rejectValue: ApiResponse<null> }
>(
  'presentation/startPresentationAnalyzeAction',
  async ({ jobId }, { rejectWithValue }) => {
    try {
      const res = await sendRequest<PptStartProcessingData>(
        RequestType.GET,
        `/api/PPT/start?JobId=${jobId}`
      );
      return res;
    } catch (err) {
      return rejectWithValue(handleError(err) as ApiResponse<null>);
    }
  }
);

export const getPresentationStatusAction = createAsyncThunk<
  ApiResponse<GetPptStatusData>,
  { jobId: string },
  { rejectValue: ApiResponse<null> }
>(
  'presentation/getPresentationStatusAction',
  async ({ jobId }, { rejectWithValue }) => {
    try {
      const res = await sendRequest<GetPptStatusData>(
        RequestType.GET,
        `/api/PPT/status?JobId=${jobId}`
      );
      return res;
    } catch (err) {
      return rejectWithValue(handleError(err) as ApiResponse<null>);
    }
  }
);

export const finalizeSlidesAction = createAsyncThunk<
  ApiResponse<FinalizeSlideResponse>,
  FinalizeSlideRequest,
  { rejectValue: ApiResponse<null> }
>(
  'presentation/finalizeSlidesAction',
  async (payload, { rejectWithValue }) => {
    try {
      const res = await sendRequest<FinalizeSlideResponse>(
        RequestType.POST,
        '/api/PPT/finalize-slides',
        payload
      );
      return res;
    } catch (err) {
      return rejectWithValue(handleError(err) as ApiResponse<null>);
    }
  }
);