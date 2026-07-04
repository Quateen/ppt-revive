import { useEffect, useRef, useState } from 'react';
import FileUploader from '@/components/FileUploader';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import JoinFoundingMembers from '@/components/JoinFoundingMembers';
import { NucleusMark } from '@/components/BrandLogo';
import { CircleHelp, FileText, Search, RotateCcw, FileCheck, ShieldCheck, Stethoscope, ArrowRight } from 'lucide-react';
import { useToast } from '@/components/ui/use-toast';
import { getPresentationStatusAction, startPresentationAnalyzeAction, uploadPresentationAction } from '@/app-redux/presentation/presentationAction';
import { useAppDispatch, useAppSelector } from '@/hooks/reduxHooks';
import { selectPresentatiionDetialsStatus, selectPresentationProcessingStatus, selectUploadPresentationStatus } from '@/app-redux/presentation/presentationSlice';
import { ProcessingStatus } from '@/common/enums/app.enum';
import { useNavigate } from 'react-router-dom';
import { resetPresentationState } from '@/app-redux/presentation/presentationSlice';
import { AppConfig } from '@/config';


const Index = () => {
  const navigate = useNavigate();

  const hasNavigated = useRef(false);

  const [analyzing, setAnalyzing] = useState(false);
  const [uploadedFile, setUploadedFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);

  const { toast } = useToast();
  const dispatch = useAppDispatch();

  const uploadResponse = useAppSelector(selectUploadPresentationStatus);
  const startStatus = useAppSelector(selectPresentationProcessingStatus);
  const details = useAppSelector(selectPresentatiionDetialsStatus);
  const uploadError = useAppSelector((state) => state.presentation.presentationError);
  const uploadStatusCode = useAppSelector((state) => state.presentation.statusCode);


  useEffect(() => {
    if (uploading && uploadError) {
      setUploading(false);
      if (uploadStatusCode !== 401) {
        toast({
          variant: "destructive",
          title: "Upload failed",
          description: uploadError || "Something went wrong while uploading.",
        });
      }
    }
  }, [uploading, uploadError]);


  const jobId = uploadResponse?.jobId;

  const handleAnalyze = async () => {
    if (!uploadedFile && jobId != null) return;

    setAnalyzing(true);

    dispatch(startPresentationAnalyzeAction({ jobId }));
    toast({
      title: "Analysis in progress",
      description: "We're checking each slide against current evidence. This takes a moment..."
    });
  };

  useEffect(() => {
    dispatch(resetPresentationState());
  }, [dispatch]);


  useEffect(() => {

    if (!startStatus || !jobId || hasNavigated.current) return;

    if (startStatus.status === ProcessingStatus.Failed) {
      // The /start call itself failed: stop the spinner, tell the user, and
      // clear the stale job so they can re-upload/retry.
      setAnalyzing(false);
      toast({
        variant: "destructive",
        title: "Processing failed",
        description:
          startStatus.error || "The presentation could not be processed. Please try again.",
      });
      dispatch(resetPresentationState());
      return;
    }

    // Stop polling once a terminal status has been reached, so we don't poll forever.
    const currentStatus = details?.status;
    if (
      currentStatus === ProcessingStatus.Completed ||
      currentStatus === ProcessingStatus.Failed
    ) {
      return;
    }

    // Poll frequently enough that the per-slide progress bar feels live.
    const interval = setInterval(() => {
      dispatch(getPresentationStatusAction({ jobId }));
    }, 4000); // every 4 seconds

    return () => clearInterval(interval);
  }, [startStatus?.status, startStatus?.error, jobId, dispatch, details?.status]);

  useEffect(() => {
    if (uploading && jobId) {
      setUploading(false);
    }
  }, [jobId]);


  // auto stop analyzing when status is Completed or Failed
  useEffect(() => {
    const currentStatus = details?.status;

    if (typeof currentStatus !== "number") return;

    const isFinalStatus =
      currentStatus === ProcessingStatus.Completed ||
      currentStatus === ProcessingStatus.Failed;

    if (!isFinalStatus) return;

    setAnalyzing(false);

    if (currentStatus === ProcessingStatus.Completed) {
      const slidePages = details?.result?.slidePages;

      if (slidePages && slidePages.length > 0) {
        if (!hasNavigated.current) {
          hasNavigated.current = true;
          navigate("/analyzer", {
            state: {
              jobId,
              originalFileName: uploadedFile?.name,
            },
          });
        }
        return;
      }

      // Backend can report Completed but return no slides — treat as a failure.
      toast({
        variant: "destructive",
        title: "No slides found",
        description:
          details?.error ||
          "The presentation was processed but no slides were returned. Please try another file.",
      });
      dispatch(resetPresentationState());
      return;
    }

    // ProcessingStatus.Failed
    toast({
      variant: "destructive",
      title: "Processing failed",
      description:
        details?.error || "The presentation could not be processed. Please try again.",
    });
    dispatch(resetPresentationState());
  }, [details?.status]);


  useEffect(() => {
    const beforeUnloadHandler = (e: BeforeUnloadEvent) => {
      if (analyzing) {
        e.preventDefault();
        e.returnValue = '';
      }
    };

    window.addEventListener('beforeunload', beforeUnloadHandler);
    return () => window.removeEventListener('beforeunload', beforeUnloadHandler);
  }, [analyzing]);

  const freeSlides = AppConfig.FREE_TIER_SLIDES;

  const steps = [
    { icon: FileText, title: 'Upload your deck', body: `Drop in a .pptx lecture. The free tier revives up to ${freeSlides} slides — no account needed.` },
    { icon: Search, title: 'Checked against the literature', body: 'Each slide is matched to current papers retrieved from PubMed — real citations, not invented ones.' },
    { icon: RotateCcw, title: 'You approve every change', body: 'Old vs. proposed, side by side. Approve, edit, or reject each slide. Nothing ships without your sign-off.' },
    { icon: FileCheck, title: 'Download, fully cited', body: 'Export the revived deck with a references slide — every update traceable to its source.' },
  ];

  return (

    <main className="flex-1">
      {/* Hero */}
      <section className="relative overflow-hidden">
        <div className="absolute inset-0 -z-10 bg-gradient-to-b from-secondary/50 to-background" />
        <div className="container mx-auto px-4 py-20 md:py-24">
          <div className="max-w-3xl mx-auto text-center">
            <span className="inline-flex items-center gap-2 rounded-full border border-border bg-card px-4 py-1.5 text-sm text-muted-foreground mb-6">
              <Stethoscope className="h-4 w-4 text-primary" />
              Physician-led · AI-enabled · Human-reviewed
            </span>
            <h1 className="font-display text-4xl md:text-6xl font-semibold text-foreground leading-tight mb-6">
              Your 2019 slides are<br className="hidden sm:block" />{' '}
              <span className="nucleus-gradient-text">quietly costing you credibility.</span>
            </h1>
            <p className="text-lg md:text-xl text-foreground/75 mb-8 max-w-2xl mx-auto">
              PPT-Revive rewrites your medical lecture against current, cited evidence — one slide at a
              time, with you approving every change. No AI hype. Just a clear path back to current.
            </p>
            <div className="flex flex-col sm:flex-row gap-3 justify-center items-center">
              <Button size="lg" asChild className="font-medium px-8">
                <a href="#upload">Revive {freeSlides} slides free <ArrowRight className="h-4 w-4" /></a>
              </Button>
              <Button size="lg" variant="outline" asChild className="font-medium">
                <a href="#how">See how it works</a>
              </Button>
            </div>
            <p className="text-sm text-muted-foreground mt-4">Free to try · No account needed · Your file is never shared</p>
          </div>
        </div>
      </section>

      {/* Trust bar */}
      <section className="border-y border-border bg-card">
        <div className="container mx-auto px-4 py-5">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center text-sm">
            {['Evidence from PubMed', 'You approve every change', 'Real citations — no hallucinations', 'Free, no account'].map((t) => (
              <div key={t} className="flex items-center justify-center gap-2 text-muted-foreground">
                <ShieldCheck className="h-4 w-4 text-primary shrink-0" />
                <span>{t}</span>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* How it works */}
      <section id="how" className="py-20">
        <div className="container mx-auto px-4">
          <div className="text-center max-w-2xl mx-auto mb-14">
            <h2 className="font-display text-3xl md:text-4xl font-semibold mb-3">From dated to defensible in four steps</h2>
            <p className="text-muted-foreground">Most clinicians don’t need more AI hype. They need a clear path — this is one concrete step on it.</p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {steps.map(({ icon: Icon, title, body }, i) => (
              <div key={title} className="rounded-2xl border border-border bg-card p-6">
                <div className="flex items-center gap-3 mb-4">
                  <div className="nucleus-gradient text-white w-10 h-10 rounded-xl flex items-center justify-center font-display font-semibold">
                    {i + 1}
                  </div>
                  <Icon className="h-5 w-5 text-primary" />
                </div>
                <h3 className="text-lg font-semibold mb-2">{title}</h3>
                <p className="text-muted-foreground text-sm">{body}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Founder credibility */}
      <section className="py-4">
        <div className="container mx-auto px-4">
          <div className="max-w-3xl mx-auto rounded-3xl border border-border bg-secondary/40 p-8 flex flex-col sm:flex-row items-center gap-6 text-center sm:text-left">
            <div className="nucleus-gradient rounded-2xl p-4 shrink-0">
              <NucleusMark className="h-10 w-10" />
            </div>
            <div>
              <p className="text-foreground/90 text-lg font-display mb-2">
                “Keeping your teaching current shouldn’t cost a weekend — and the fastest way to trust a
                tool is to see it show its evidence.”
              </p>
              <p className="text-sm text-muted-foreground">
                Built by <span className="font-medium text-foreground">Ahmed Quateen, MD</span> — neurosurgeon &amp; spine surgeon,
                adjunct professor, UAE University. Founder, Nucleus Digitalis.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Upload */}
      <section id="upload" className="py-20">
        <div className="container mx-auto px-4">
          <div className="max-w-2xl mx-auto">
            <div className="text-center mb-8">
              <h2 className="font-display text-3xl md:text-4xl font-semibold mb-2">Revive your first {freeSlides} slides — free</h2>
              <p className="text-muted-foreground">Upload a .pptx and see the quality for yourself. No account required.</p>
            </div>

            <div className="rounded-xl border border-border bg-secondary/40 text-foreground/80 p-4 mb-6 text-sm">
              <ul className="space-y-1.5">
                <li>🔬 <strong>Free tier:</strong> up to {freeSlides} slides revived per deck</li>
                <li>📂 <strong>Max file size:</strong> {AppConfig.MAX_PPT_FILE_SIZE_MB} MB · <strong>Format:</strong> .pptx only</li>
                <li>📝 <strong>Text-focused:</strong> slide text is updated; images, charts and tables are preserved as-is.</li>
                <li>🔒 <strong>No patient data, please:</strong> don't upload slides with patient-identifiable information (PHI). Slide text is sent to AI and PubMed services for analysis, processed transiently, and not shared.</li>
                <li>✅ <strong>You stay in control:</strong> every change is evidence-linked and needs your approval.</li>
              </ul>
            </div>

            <FileUploader
              selectedFile={uploadedFile}
              onFileSelected={(file) => setUploadedFile(file)}
              onRemoveFile={() => {
                setUploadedFile(null);
                setUploading(false);
              }}
              disabled={uploading}
            />

            {uploadedFile && (
              <>
                {(uploading || analyzing) && (
                  <div className="mt-6 flex flex-col items-center justify-center" role="status" aria-live="polite">
                    <svg className="animate-spin h-6 w-6 text-primary mb-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" aria-hidden="true">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
                    </svg>
                    {(() => {
                      const total = details?.totalSlides ?? 0;
                      const done = details?.processedSlides ?? 0;
                      const showBar = analyzing && total > 0;
                      return (
                        <div className="w-full max-w-sm text-center">
                          <p className="text-sm text-muted-foreground mb-2">
                            {uploading
                              ? "Uploading your deck. Please wait..."
                              : showBar
                                ? `Reviewing slide ${Math.min(done + 1, total)} of ${total} against current evidence...`
                                : "Checking each slide against current evidence..."}
                          </p>
                          {showBar && (
                            <>
                              <Progress value={Math.round((done / total) * 100)} className="h-2" />
                              <p className="text-xs text-muted-foreground mt-1">{done} of {total} slides reviewed</p>
                            </>
                          )}
                        </div>
                      );
                    })()}
                  </div>
                )}

                {!jobId && !uploading && (
                  <div className="mt-6 text-center">
                    <Button
                      size="lg"
                      className="w-full max-w-xs mx-auto font-medium"
                      onClick={() => {
                        setUploading(true);
                        const formData = new FormData();
                        formData.append("File", uploadedFile);
                        dispatch(uploadPresentationAction(formData));
                      }}
                    >
                      Upload
                    </Button>
                  </div>
                )}

                {jobId && !uploading && (
                  <div className="mt-8 text-center">
                    <Button
                      size="lg"
                      className="w-full max-w-xs mx-auto font-medium"
                      onClick={handleAnalyze}
                      disabled={analyzing}
                    >
                      Revive my slides
                    </Button>

                    <div className="mt-4 flex items-center justify-center text-sm text-muted-foreground">
                      <CircleHelp className="h-4 w-4 mr-1" />
                      <span>Analyzed slide by slide against current literature</span>
                    </div>
                  </div>
                )}
              </>
            )}
          </div>
        </div>
      </section>

      {/* Conversion band */}
      <section className="pb-20">
        <div className="container mx-auto px-4">
          <div className="max-w-4xl mx-auto">
            <JoinFoundingMembers source="landing" />
          </div>
        </div>
      </section>
    </main>

  );
};

export default Index;
