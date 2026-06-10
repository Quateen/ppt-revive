import axios, {
  AxiosError,
  AxiosHeaders,
  AxiosInstance,
  AxiosResponse,
  InternalAxiosRequestConfig,
} from "axios";
import { getCurrentUser, removeCurrentUser } from "@/common/utils/userAttribs4mLocalStorage";
import { AppConfig } from "@/config";
import { ApiResponse } from "./apiTypes";
// import store from "@/app-redux/store";
import { showAuthModal } from "@/app-redux/ui/uiSlice";
import { EventEmitter } from "@/common/eventEmitter";

// --- Enum for method types ---
export enum RequestType {
  GET = "GET",
  POST = "POST",
  PUT = "PUT",
  DELETE = "DELETE",
}

// --- Axios instance ---
const ax: AxiosInstance = axios.create({
  baseURL: AppConfig.API_BASE_URL,
});

// --- Generic API caller that unwraps data only ---
export async function sendRequest<T>(
  method: RequestType,
  endpoint: string,
  payload?: unknown
): Promise<ApiResponse<T>> {
  const config = {
    method,
    url: endpoint,
    data: payload,
    headers: {} as Record<string, string>,
  };

  // Automatically omit setting headers if payload is FormData
  if (payload instanceof FormData) {
    delete config.headers; // Let Axios set the right multipart boundary
  }

  const response = await ax.request<ApiResponse<T>>(config);

  if (response.data.status && response.data !== undefined) {
    return response.data;
  }

  throw new Error(response.data.message || "Unknown error");
}

// --- Request Interceptor ---
ax.interceptors.request.use(
  (configa: InternalAxiosRequestConfig): InternalAxiosRequestConfig => {
    const data = getCurrentUser(AppConfig.STORAGE_KEY);

    if (!configa.headers || typeof configa.headers.set !== "function") {
      configa.headers = AxiosHeaders.from({});
    }

    if (data?.accessToken) {
      configa.headers.set("Authorization", `Bearer ${data.accessToken}`);
    } else {
      configa.headers.set("Accept", "application/json");
    }

    return configa;
  },
  (error: AxiosError) => Promise.reject(error)
);

// --- Response Interceptor ---
ax.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    const status = error?.response?.status;
    if (status === 401) {
      removeCurrentUser(AppConfig.STORAGE_KEY);
      EventEmitter.emit("unauthorized"); 
    }

    return Promise.reject(error);
  }
);

export default ax;
