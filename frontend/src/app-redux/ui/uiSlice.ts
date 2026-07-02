// uiSlice.ts
import { createSlice } from "@reduxjs/toolkit";

interface UiState {
  isAuthModalVisible: boolean;
}

const initialState: UiState = {
  isAuthModalVisible: false,
};

const uiSlice = createSlice({
  name: "ui",
  initialState,
  reducers: {
    showAuthModal: (state) => {
      state.isAuthModalVisible = true;
    },
    hideAuthModal: (state) => {
      state.isAuthModalVisible = false;
    },
  },
});

export const { showAuthModal, hideAuthModal } = uiSlice.actions;
export default uiSlice.reducer;
