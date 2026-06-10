import { useState, useEffect, useRef } from 'react';
import { AppConfig } from '@/config';
import SlideComparison from '@/components/SlideComparison';
import ReferencesList from '@/components/ReferencesList';
import PresentationHeader from '@/components/PresentationHeader';
import ProgressDisplay from '@/components/ProgressDisplay';
import SlideNavigation from '@/components/SlideNavigation';
import GeneratePresentationButton from '@/components/GeneratePresentationButton';
import { Progress } from '@/components/ui/progress';
import { useToast } from '@/components/ui/use-toast';
import { Presentation, Slide, Reference } from '@/types/presentation';
import { useAppDispatch, useAppSelector } from '@/hooks/reduxHooks';
import { selectFinalizedSlidesStatus, selectPresentatiionDetialsStatus, selectUploadPresentationStatus, setSlideEditedContent } from '@/app-redux/presentation/presentationSlice';
import { finalizeSlidesAction } from '@/app-redux/presentation/presentationAction';
import { useLocation } from 'react-router-dom';



const PresentationAnalyzer = () => {

  const dispatch = useAppDispatch();

  const location = useLocation();
  const originalFileNameFromUpload = location.state?.originalFileName as string | undefined;
  const generateButtonRef = useRef<HTMLDivElement>(null);
  const finalizedSlides = useAppSelector(selectFinalizedSlidesStatus);
  const details = useAppSelector(selectPresentatiionDetialsStatus);
  const uploadResponse = useAppSelector(selectUploadPresentationStatus);
  const jobId = uploadResponse?.jobId;
  const [presentation, setPresentation] = useState<Presentation | null>(null);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isDirty, setIsDirty] = useState(false);
  const [generating, setGenerating] = useState(false);
  const { toast } = useToast();

  useEffect(() => {
    if (details === undefined) return; // wait for details to exist

    const result = details?.result;

    if (!result || !result.slidePages || result.slidePages.length === 0) {
      window.location.href = '/';

      // toast({
      //   variant: 'destructive',
      //   title: 'Slide data missing',
      //   description: 'No slide information available from backend.'
      // });
      // // navigate('/');
      // window.location.href = '/';
      return;
    }

    const convertedSlides: Slide[] = result.slidePages.map((s) => ({
      id: `slide-${s.slideId}`,
      number: s.slideId,
      title: s.titleText, // placeholder title, if needed
      originalContent: s.originalSlideContent,
      suggestedUpdate: s.updatedSlideContent,
      references: s.references,
      updateReason: s.explanation,
      sourceCitations: [s.source], // or flatten later
      status: "pending",
      editedContent: s.editedContent ?? null,
    }));
    // Flatten and deduplicate references across all slides
    const referenceMap = new Map<string, Reference>();

    result.slidePages.forEach((s, idx) => {
      s.references.forEach((ref) => {
        const [citationPart, linkPart] = ref.split(" – ");
        const citation = citationPart?.trim() ?? ref;
        const link = linkPart?.trim() ?? "";

        if (!referenceMap.has(ref)) {
          referenceMap.set(ref, {
            id: `ref-${idx}-${ref.slice(0, 10)}`,
            citation,
            year: extractYearFromText(citation),
            journal: extractJournalFromText(citation),
            type: classifyReference(citation),
            link, // 👈 Add this
          });
        }
      });

    });

    const references = Array.from(referenceMap.values());

    const newPresentation: Presentation = {
      id: jobId ?? generateUUID(),
      title: (originalFileNameFromUpload || result.fileName).replace(/\.[^/.]+$/, ''),
      originalFileName: originalFileNameFromUpload || result.fileName,
      uploadDate: new Date(result.processedAt),
      isAnalysisComplete: true,
      slides: convertedSlides,
      references: references, // can be enriched later
    };

    setPresentation(newPresentation);
  }, [details]);

  const generateUUID = (): string => {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
      const r = Math.random() * 16 | 0;
      const v = c === 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }

  const handleApprove = (slideNumber: number) => {
    setIsDirty(true);
    // Approving as-is discards any saved edit for this slide
    dispatch(setSlideEditedContent({ slideId: slideNumber, editedContent: null }));
    setPresentation(prev => {
      if (!prev) return null;
      const updatedSlides: Slide[] = prev.slides.map(slide =>
        slide.number === slideNumber
          ? {
            ...slide,
            status: slide.status === "approved" ? "pending" as const : "approved" as const,
            isApproved: slide.status === "approved" ? undefined : true,
            editedContent: null,
          }
          : slide
      );
      return { ...prev, slides: updatedSlides };
    });
    handleNavigateNext();
  };

  const handleEdit = (slideNumber: number, editedContent: string) => {
    setIsDirty(true);
    // Keep the edited text in Redux so it is part of app state, not just local UI state
    dispatch(setSlideEditedContent({ slideId: slideNumber, editedContent }));
    setPresentation(prev => {
      if (!prev) return null;
      const updatedSlides: Slide[] = prev.slides.map(slide =>
        slide.number === slideNumber
          ? {
            ...slide,
            status: "modified" as const, // approved with edits
            isApproved: true,
            editedContent,
          }
          : slide
      );
      return { ...prev, slides: updatedSlides };
    });
    handleNavigateNext();
  };

  function extractYearFromText(citation: string): number {
    const match = citation.match(/(\d{4})/);
    return match ? parseInt(match[1]) : 0;
  }

  function extractJournalFromText(citation: string): string {
    const match = citation.match(/Journal of [^.,]+/);
    return match ? match[0] : 'Unknown Journal';
  }

  function classifyReference(citation: string): Reference['type'] {
    const lowered = citation.toLowerCase();
    if (lowered.includes('guideline')) return 'guideline';
    if (lowered.includes('meta-analysis')) return 'meta-analysis';
    if (lowered.includes('review')) return 'review';
    if (lowered.includes('journal')) return 'journal';
    return 'other';
  }

  const handleReject = (slideNumber: number) => {
    setIsDirty(true);
    // Rejecting discards any saved edit for this slide
    dispatch(setSlideEditedContent({ slideId: slideNumber, editedContent: null }));
    setPresentation(prev => {
      if (!prev) return null;
      const updatedSlides: Slide[] = prev.slides.map(slide =>
        slide.number === slideNumber
          ? {
            ...slide,
            status: slide.status === "rejected" ? "pending" as const : "rejected" as const,
            isApproved: slide.status === "rejected" ? undefined : false,
            editedContent: null,
          }
          : slide
      );
      return { ...prev, slides: updatedSlides };
    });

    handleNavigateNext();
  };

  useEffect(() => {
    if (!presentation) return;

    const completedCount = presentation.slides.filter(slide =>
      slide.status === 'approved' || slide.status === 'rejected' || slide.status === 'modified'
    ).length;

    const isComplete = completedCount === presentation.slides.length;

    if (isComplete && generateButtonRef.current) {
      generateButtonRef.current.scrollIntoView({ behavior: 'smooth', block: 'center' });
      const btn = generateButtonRef.current.querySelector('button');
      if (btn) (btn as HTMLButtonElement).focus();
    }
  }, [presentation?.slides]);

  const handleReset = (slideId: string) => {
    setIsDirty(true);

    setPresentation(prev => {
      if (!prev) return null;
      const updatedSlides: Slide[] = prev.slides.map(slide =>
        slide.id === slideId
          ? { ...slide, status: "pending" as const, isApproved: undefined, editedContent: null }
          : slide
      );
      return { ...prev, slides: updatedSlides };
    });
  };

  const handleNavigatePrevious = () => setCurrentIndex(Math.max(0, currentIndex - 1));
  const handleNavigateNext = () => setCurrentIndex(Math.min(presentation!.slides.length - 1, currentIndex + 1));

  const handleGeneratePresentation = async () => {
    if (!presentation || !jobId) {
      toast({
        variant: "destructive",
        title: "Missing Data",
        description: "Presentation or Job ID is not available.",
      });
      return;
    }

    setGenerating(true);

    const finalizePayload = {
      id: jobId,
      slides: presentation.slides.map((s) => ({
        id: s.number,
        // Edited slides count as approved
        isApproved: s.status === "approved" || s.status === "modified",
        // Only send edited text for slides the user actually edited
        editedContent: s.status === "modified" ? s.editedContent ?? null : null,
      })),
    };

    try {
      await dispatch(finalizeSlidesAction(finalizePayload)).unwrap();
    } catch (error) {
      console.error("Finalize failed:", error);
      toast({
        variant: "destructive",
        title: "Finalize Error",
        description: "Failed to generate updated presentation.",
      });
    } finally {
      setGenerating(false); // ✅ Stop loader
    }
  };

  useEffect(() => {
    const handler = (e: BeforeUnloadEvent) => {
      if (isDirty || generating) {
        e.preventDefault();
        e.returnValue = '';
      }
    };
    window.addEventListener('beforeunload', handler);
    return () => window.removeEventListener('beforeunload', handler);
  }, [isDirty, generating]);

  // usePrompt(isDirty || generating);


  useEffect(() => {
    const downloadFinalizedFile = async () => {

      if (!finalizedSlides?.newFilePath) return;
      try {
        setGenerating(true);
        const filePath = finalizedSlides.newFilePath;
        const fileName = filePath.split('/').pop() || 'UpdatedPresentation.pptx';

        const response = await fetch(AppConfig.API_BASE_URL + filePath);
        if (!response.ok) throw new Error("Failed to fetch finalized file");

        const blob = await response.blob();
        const url = URL.createObjectURL(blob);

        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.click();
        URL.revokeObjectURL(url);
      } catch (error) {
        console.error("Download error:", error);
        toast({
          variant: "destructive",
          title: "Download failed",
          description: "Could not download the finalized presentation.",
        });
      }
      finally{
        setGenerating(false);
      }
    };

    if (finalizedSlides?.newFilePath) {
      downloadFinalizedFile();
    }
  }, [finalizedSlides]);

  if (!presentation) {
    return (

      <main className="container mx-auto px-4 py-8 flex-1 flex items-center justify-center">
        <div className="text-center">
          <h2 className="text-xl font-medium mb-2">Loading presentation...</h2>
          <Progress value={50} className="w-64 h-2" />
        </div>
      </main>
    );
  }

  const safeIndex = Math.min(currentIndex, presentation.slides.length - 1);
  const currentSlide: Slide = presentation.slides[safeIndex];
  const references: Reference[] = presentation.references || [];

  return (

    <main className="container mx-auto px-4 py-8 flex-1">
      <div className="flex flex-col lg:flex-row gap-8">
        <div className="lg:w-3/4">
          <PresentationHeader title={presentation.title} author="" originalFileName={originalFileNameFromUpload || presentation.originalFileName} />
          <ProgressDisplay slides={presentation.slides} />
          <SlideNavigation currentIndex={currentIndex} totalSlides={presentation.slides.length} onPrevious={handleNavigatePrevious} onNext={handleNavigateNext} />
          {currentSlide && (
            <SlideComparison
              slide={currentSlide}
              onApprove={() => handleApprove(currentSlide.number)}
              onReject={() => handleReject(currentSlide.number)}
              onEdit={(_slideId, editedContent) => handleEdit(currentSlide.number, editedContent)}
            />
          )}
          <div ref={generateButtonRef} className="mt-4 flex justify-center">
            <GeneratePresentationButton
              slides={presentation.slides}
              onGenerate={handleGeneratePresentation}
              loading={generating}
            />
          </div>
        </div>
        <div className="lg:w-1/4">
          <ReferencesList references={references} />
        </div>
      </div>
    </main>

  );
};

export default PresentationAnalyzer;
