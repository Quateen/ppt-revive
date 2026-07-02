import { configureStore, combineReducers } from "@reduxjs/toolkit";
import authReducer from "./auth/authSlice";
import presentationReducer from "./presentation/presentationSlice";
import uiReducer from "@/app-redux/ui/uiSlice";
import { loadState } from "./localstorage"; // ✅ Now safe

const rootReducer = combineReducers({
  auth: authReducer,
  presentation: presentationReducer,
  ui: uiReducer,
});

const preloadedState = loadState(); // ✅ returns { auth: {...} } or undefined

export const store = configureStore({
  reducer: rootReducer,
  middleware: (getDefaultMiddleware) => getDefaultMiddleware(),
  devTools: import.meta.env.MODE !== "production",
  preloadedState,
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

export default store;
