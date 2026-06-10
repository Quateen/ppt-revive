// --- State Shape ---
export interface PresentationState {
  status: "idle" | "loading" | "failed";
  data: PresentationDto | null;
  uploadPresentationResponse: UploadResponse | null;
  presentationError?: string;
  presentationStatusResponse: GetPptStatusData | null;
  presentationProcessingStatus: PptStartProcessingData | null;
  finalizedSlidesResponse: FinalizeSlideResponse | null,
  statusCode?: number
}

// --- Upload API Response ---
export interface UploadResponse {
  jobId: string;
}

// --- Create Form Model (optional usage) ---
export interface CreatePresentationRequest {
  name: string;
  email: string;
  password: string;
}

// --- Simple DTO (not used by upload/status flow) ---
export interface PresentationDto {
  id: number;
  name: string;
  email: string;
}

export interface ApiErrorResponse {
  message: string;
  [key: string]: unknown;
}

// --- Slide Info ---
export interface SlidePage {
  slideId: number;
  originalSlideContent: string;
  updatedSlideContent: string;
  titleText: string;
  references: string[];       // right sidebar
  explanation: string;        // yellow box
  source: string;             // inline source shown at bottom
  editedContent?: string | null; // user-edited update text (set during review)
}


export interface SlideInfo {
  fileName: string;
  slidePageCount: number;
  processedAt: string;
  slidePages: SlidePage[];
}

// --- Result Object from /status ---
export interface PresentationResult {
  fileUrl: string;
  fileName: string;
  slidePageCount: number;
  processedAt: string;
  slidePages: SlidePage[];
}

export interface GetPptStatusData {
  status: number;
  result: PresentationResult | null;
  error: string | null;
}

export interface PptStartProcessingData {
  status: number;
  result: string | null;
  error: string | null;
  fileName: string | null;
}

export interface FinalizeSlideRequest {
  id: string;
  slides: {
    id: number;
    isApproved: boolean;
    editedContent?: string | null; // user-edited text; null/undefined when not edited
  }[];
}

export interface FinalizeSlideResponse {
  newFilePath: string;
}

