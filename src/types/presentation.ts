
export interface Slide {
  id: string;
  number: number;
  title: string;
  originalContent: string;
  suggestedUpdate: string;
  updateReason?: string;
  sourceCitations?: string[];
  status: 'pending' | 'approved' | 'rejected' | 'modified';
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
  type: 'journal' | 'guideline' | 'review' | 'meta-analysis' | 'other';
}
