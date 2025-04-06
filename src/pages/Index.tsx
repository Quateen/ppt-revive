
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Header from '@/components/Header';
import Footer from '@/components/Footer';
import FileUploader from '@/components/FileUploader';
import { Button } from '@/components/ui/button';
import { CircleHelp, FileText, Search, RotateCcw, FileCheck } from 'lucide-react';
import { useToast } from '@/components/ui/use-toast';

const Index = () => {
  const [fileUploaded, setFileUploaded] = useState(false);
  const [analyzing, setAnalyzing] = useState(false);
  const [uploadedFile, setUploadedFile] = useState<File | null>(null);
  const navigate = useNavigate();
  const { toast } = useToast();
  
  const handleFileUploaded = (file: File) => {
    setFileUploaded(true);
    setUploadedFile(file);
  };
  
  const handleAnalyze = () => {
    if (!uploadedFile) return;

    // In a real app, this would analyze the presentation
    setAnalyzing(true);
    
    // Simulate analysis
    toast({
      title: "Analysis in progress",
      description: "We're analyzing your presentation content. This may take a moment..."
    });
    
    // In a real implementation, you would process the file here
    // For now, we'll just simulate a delay and navigate to the analyzer page
    setTimeout(() => {
      setAnalyzing(false);
      
      // Store the file name in sessionStorage so we can display it in the analyzer page
      sessionStorage.setItem('uploadedFileName', uploadedFile.name);
      sessionStorage.setItem('uploadedFileSize', uploadedFile.size.toString());
      sessionStorage.setItem('uploadedFileDate', new Date().toISOString());
      
      navigate('/analyzer');
    }, 3000);
  };
  
  return (
    <div className="flex flex-col min-h-screen">
      <Header />
      
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
              
              <FileUploader onFileUploaded={handleFileUploaded} />
              
              {fileUploaded && (
                <div className="mt-8 text-center">
                  <Button 
                    size="lg" 
                    onClick={handleAnalyze} 
                    disabled={analyzing}
                    className="px-8"
                  >
                    {analyzing ? "Analyzing..." : "Analyze Presentation"}
                  </Button>
                  
                  <div className="mt-4 flex items-center justify-center text-sm text-gray-500">
                    <CircleHelp className="h-4 w-4 mr-1" />
                    <span>Your presentation will be analyzed slide by slide with AI</span>
                  </div>
                </div>
              )}
            </div>
          </div>
        </section>
      </main>
      
      <Footer />
    </div>
  );
};

export default Index;
