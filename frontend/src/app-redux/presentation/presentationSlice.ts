import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import type { RootState } from "../store";
import {
  finalizeSlidesAction,
  getPresentationStatusAction,
  startPresentationAnalyzeAction,
  uploadPresentationAction,
} from "./presentationAction";
import {
  FinalizeSlideResponse,
  GetPptStatusData,
  PptStartProcessingData,
  PresentationState,
  UploadResponse,
} from "./presentationTypes";
import { ApiResponse } from "@/api/apiTypes";

const initialState: PresentationState = {
  status: "idle",
  data: null,
  uploadPresentationResponse: null,
  presentationStatusResponse: null,
  presentationProcessingStatus: null,
  finalizedSlidesResponse: null,
};

const presentationSlice = createSlice({
  name: "presentation",
  initialState,
  reducers: {
    resetErrorAction: (state) => {
      delete state.presentationError;
    },
    // Stores user-edited content on a slide (kept in Redux so it survives re-renders
    // and is available when building the finalize payload).
    setSlideEditedContent: (
      state,
      action: PayloadAction<{ slideId: number; editedContent: string | null }>
    ) => {
      const slidePages = state.presentationStatusResponse?.result?.slidePages;
      if (!slidePages) return;
      const slide = slidePages.find((s) => s.slideId === action.payload.slideId);
      if (slide) {
        slide.editedContent = action.payload.editedContent;
      }
    },
    resetPresentationState: (state) => {
      state.uploadPresentationResponse = null;
      state.presentationStatusResponse = null;
      state.finalizedSlidesResponse = null;
      state.presentationError = undefined;
      state.status = "idle";
      state.statusCode = undefined;
    },
  },
  extraReducers: (builder) => {
    builder
      // Upload Presentation
      .addCase(uploadPresentationAction.pending, (state) => {
        state.status = "loading";
      })
      .addCase(uploadPresentationAction.fulfilled, (state, action: PayloadAction<ApiResponse<UploadResponse>>) => {
        state.status = "idle";
        state.uploadPresentationResponse = action.payload.data!;
      })

      .addCase(uploadPresentationAction.rejected, (state, action: PayloadAction<ApiResponse<null> | undefined>) => {
        state.status = "failed";
        state.presentationError =
          action.payload?.message || action.payload?.error || "Upload failed";
        state.statusCode = action.payload?.statusCode
      })

      // Start Analyze
      .addCase(startPresentationAnalyzeAction.pending, (state) => {
        state.status = "loading";
      })
      .addCase(startPresentationAnalyzeAction.fulfilled, (state, action: PayloadAction<ApiResponse<PptStartProcessingData>>) => {
        state.status = "idle";
        if (action.payload.status) {
          state.presentationProcessingStatus = action.payload.data!;
        } else {
          state.presentationError = action.payload.message || "Start analyze failed";
        }
      })
      .addCase(startPresentationAnalyzeAction.rejected, (state, action: PayloadAction<ApiResponse<null> | undefined>) => {
        state.status = "failed";
        state.presentationError =
          action.payload?.message || action.payload?.error || "Start analyze failed";
      })

      // Poll Presentation Status
      .addCase(getPresentationStatusAction.pending, (state) => {
        state.status = "loading";
      })
      .addCase(getPresentationStatusAction.fulfilled, (state, action: PayloadAction<ApiResponse<GetPptStatusData>>) => {
        state.status = "idle";
        if (action.payload.status) {
          state.presentationStatusResponse = action.payload.data!;
        } else {
          state.presentationError = action.payload.message || "Status fetch failed";
        }
      })
      .addCase(getPresentationStatusAction.rejected, (state, action: PayloadAction<ApiResponse<null> | undefined>) => {
        state.status = "failed";
        state.presentationError =
          action.payload?.message || action.payload?.error || "Status fetch failed";
      })

      // Finalize Slides
      .addCase(finalizeSlidesAction.pending, (state) => {
        state.status = "loading";
      })
      .addCase(finalizeSlidesAction.fulfilled, (state, action: PayloadAction<ApiResponse<FinalizeSlideResponse>>) => {
        state.status = "idle";
        if (action.payload.status) {
          state.finalizedSlidesResponse = action.payload.data!;
        } else {
          state.presentationError = action.payload.message || "Finalize failed";
        }
      })
      .addCase(finalizeSlidesAction.rejected, (state, action: PayloadAction<ApiResponse<null> | undefined>) => {
        state.status = "failed";
        state.presentationError =
          action.payload?.message || action.payload?.error || "Finalize failed";
      });
  },
});

export const { resetErrorAction, resetPresentationState, setSlideEditedContent } = presentationSlice.actions;
export default presentationSlice.reducer;

// Selectors
export const selectUploadPresentationStatus = (state: RootState): UploadResponse | null =>
  state.presentation.uploadPresentationResponse;

export const selectPresentationProcessingStatus = (state: RootState): PptStartProcessingData | null =>
  state.presentation.presentationProcessingStatus;

export const selectPresentatiionDetialsStatus = (state: RootState): GetPptStatusData | null =>
  state.presentation.presentationStatusResponse;

export const selectFinalizedSlidesStatus = (state: RootState): FinalizeSlideResponse | null =>
  state.presentation.finalizedSlidesResponse;
