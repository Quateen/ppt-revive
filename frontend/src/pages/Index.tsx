import { useEffect, useRef, useState } from 'react';
import FileUploader from '@/components/FileUploader';
import { Button } from '@/components/ui/button';
import { CircleHelp, FileText, Search, RotateCcw, FileCheck } from 'lucide-react';
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


  // const slideInfo = details?.result?.slidePages;
  const jobId = uploadResponse?.jobId;

  const handleAnalyze = async () => {
    if (!uploadedFile && jobId != null) return;

    setAnalyzing(true);

    dispatch(startPresentationAnalyzeAction({ jobId }));
    toast({
      title: "Analysis in progress",
      description: "We're analyzing your presentation content. This may take a moment..."
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

    // Otherwise keep polling for the job status.
    const interval = setInterval(() => {
      dispatch(getPresentationStatusAction({ jobId }));
    }, 10000); // every 10 seconds

    return () => clearInterval(interval);
  }, [startStatus?.status, startStatus?.error, jobId, dispatch, details?.status]);

  useEffect(() => {
    if (uploading && jobId) {
      setUploading(false);
    }
  }, [jobId]);

  //   useEffect(() => {
  //   if (!uploading && uploadResponse === undefined && startStatus?.status === "failed") {
  //     // You can trigger modal opening here too if not done by interceptor
  //     // dispatch(showAuthModal());
  //     toast({
  //       variant: "destructive",
  //       title: "Upload failed",
  //       description: "You may need to login first to upload.",
  //     });
  //   }
  // }, [uploadResponse, startStatus?.status, uploading]);


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
              originalFileName: uploadedFile?.name, // 👈 send file name
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


  return (

    <main className="flex-1">
      {/* Hero section */}
      <section className="bg-gradient-to-r from-medical-800 to-medical-600 text-white py-16">
        <div className="container mx-auto px-4">
          <div className="max-w-3xl mx-auto text-center">
            <h1 className="text-4xl font-bold mb-6">
              Keep Medical Presentations Up-to-Date with the Latest Research
            </h1>
            <p className="text-xl mb-8 text-medical-50">
              MediPresent Revive automatically analyzes your existing slides and suggests updates based on recent medical literature.
            </p>
            <Button size="lg" variant="secondary" asChild className="text-medical-800 font-medium">
              <a href="#upload" className="px-8">Get Started</a>
            </Button>
          </div>
        </div>
      </section>

      {/* How it works section */}
      <section className="py-16 bg-gray-50">
        <div className="container mx-auto px-4">
          <h2 className="text-3xl font-bold text-center mb-12">How It Works</h2>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
            <div className="text-center">
              <div className="bg-white p-4 rounded-full w-16 h-16 mx-auto mb-4 shadow-sm flex items-center justify-center">
                <FileText className="h-8 w-8 text-medical-600" />
              </div>
              <h3 className="text-xl font-semibold mb-2">Upload Presentation</h3>
              <p className="text-gray-600">Upload your PowerPoint presentation for analysis</p>
            </div>

            <div className="text-center">
              <div className="bg-white p-4 rounded-full w-16 h-16 mx-auto mb-4 shadow-sm flex items-center justify-center">
                <Search className="h-8 w-8 text-medical-600" />
              </div>
              <h3 className="text-xl font-semibold mb-2">AI Analysis</h3>
              <p className="text-gray-600">Our AI searches for recent relevant research</p>
            </div>

            <div className="text-center">
              <div className="bg-white p-4 rounded-full w-16 h-16 mx-auto mb-4 shadow-sm flex items-center justify-center">
                <RotateCcw className="h-8 w-8 text-medical-600" />
              </div>
              <h3 className="text-xl font-semibold mb-2">Review Updates</h3>
              <p className="text-gray-600">Approve or reject suggested changes</p>
            </div>

            <div className="text-center">
              <div className="bg-white p-4 rounded-full w-16 h-16 mx-auto mb-4 shadow-sm flex items-center justify-center">
                <FileCheck className="h-8 w-8 text-medical-600" />
              </div>
              <h3 className="text-xl font-semibold mb-2">Download Updated Slides</h3>
              <p className="text-gray-600">Get your presentation with all the latest research</p>
            </div>
          </div>
        </div>
      </section>




      {/* Upload section */}
      <section id="upload" className="py-16 bg-white">
        <div className="container mx-auto px-4">
          <div className="max-w-2xl mx-auto">
            <h2 className="text-3xl font-bold text-center mb-2">Update Your Presentation</h2>
            <p className="text-center text-gray-600 mb-8">
              Upload your medical presentation to get started
            </p>
            <div className="bg-yellow-50 border border-yellow-200 text-yellow-800 p-4 rounded-md mb-6 text-sm">
              <ul className="list-disc pl-5 space-y-1">
                <li>📂 <strong>Max file size:</strong> {AppConfig.MAX_PPT_FILE_SIZE_MB} MB</li>
                <li>🖼️ <strong>Max slides:</strong> {AppConfig.MAX_PPT_SLIDES} slides per upload</li>
                <li>📝 <strong>Text-only updates:</strong> Only the text in slides is analyzed and updated.</li>
                <li>🚫 <strong>Images, charts, and tables</strong> will be ignored during the update process.</li>
                <li>📄 <strong>Accepted format:</strong> .pptx files only</li>
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
                {/* Loader between file and button */}
                {(uploading || analyzing) && (
                  <div className="mt-6 text-center flex flex-col items-center justify-center">
                    <svg className="animate-spin h-6 w-6 text-medical-600 mb-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
                    </svg>
                    <p className="text-sm text-gray-500">
                      {uploading ? "Uploading presentation. Please wait..." : "Analyzing presentation. Please wait..."}
                    </p>
                  </div>
                )}

                {/* Show Upload button if jobId does not exist */}
                {!jobId && !uploading && (
                  <div className="mt-6 text-center">
                    <Button
                      size="lg"
                      className="w-full max-w-xs mx-auto"
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

                {/* Show Analyze button only if jobId exists */}
                {jobId && !uploading && (
                  <div className="mt-8 text-center">
                    <Button
                      size="lg"
                      className="w-full max-w-xs mx-auto"
                      onClick={handleAnalyze}
                      disabled={analyzing}
                    >
                      Analyze Presentation
                    </Button>

                    <div className="mt-4 flex items-center justify-center text-sm text-gray-500">
                      <CircleHelp className="h-4 w-4 mr-1" />
                      <span>Your presentation will be analyzed slide by slide with AI</span>
                    </div>
                  </div>
                )}
              </>
            )}
          </div>
        </div>
      </section>
    </main>

  );
};

export default Index;
