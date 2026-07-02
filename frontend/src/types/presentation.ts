
export interface Slide {
  id: string;
  number: number;
  title: string;
  originalContent: string;
  suggestedUpdate: string;
  updateReason: string;       // explanation (maps to C# Explanation)
  sourceCitations: string[];  // source (maps to C# Source or List<string>)
  references: string[];       // right sidebar
  status: "pending" | "approved" | "rejected" | "modified";
  isApproved?: boolean;
  editedContent?: string | null; // user-edited update text (status === "modified")
}

export interface Presentation {
  id: string;
  title: string;
  author?: string;
  originalFileName: string;
  uploadDate: Date;
  slides: Slide[];
  isAnalysisComplete: boolean;
  references?: Reference[];
}

export interface Reference {
  id: string;
  citation: string;
  year: number;
  journal: string;
  type: 'journal' | 'meta-analysis' | 'review' | 'guideline' | 'other';
  link?: string;
}

export interface AnalysisResult {
  suggestedUpdate: string;
  explanation: string;
  source: string;
}