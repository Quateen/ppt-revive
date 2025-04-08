
import React, { useState } from 'react';
import { useToast } from '@/components/ui/use-toast';
import FileUploader from '@/components/FileUploader';
import Header from '@/components/Header';
import Footer from '@/components/Footer';
import { Button } from '@/components/ui/button';
import { ArrowLeft } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

const Analyzer = () => {
  const [file, setFile] = useState<File | null>(null);
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const { toast } = useToast();
  const navigate = useNavigate();

  const handleFileUploaded = (uploadedFile: File) => {
    setFile(uploadedFile);
    toast({
      title: "File uploaded successfully",
      description: `${uploadedFile.name} is ready for analysis.`,
    });
  };

  const handleAnalyze = () => {
    if (!file) {
      toast({
        variant: "destructive",
        title: "No file uploaded",
        description: "Please upload a presentation file first.",
      });
      return;
    }

    setIsAnalyzing(true);
    
    // Simulate analysis process
    setTimeout(() => {
      setIsAnalyzing(false);
      navigate('/download');
      toast({
        title: "Analysis complete",
        description: "Your presentation has been analyzed and recommendations are ready.",
      });
    }, 3000);
  };

  return (
    <div className="flex flex-col min-h-screen bg-white">
      <Header />
      
      <main className="flex-1 container mx-auto px-4 py-8">
        <Button 
          variant="ghost" 
          className="mb-6 flex items-center gap-2" 
          onClick={() => navigate('/')}
        >
          <ArrowLeft size={16} />
          Back to Home
        </Button>
        
        <div className="max-w-3xl mx-auto">
          <h1 className="text-3xl font-bold text-center mb-2">Upload Your Presentation</h1>
          <p className="text-gray-600 text-center mb-8">
            Upload your PowerPoint presentation and let our AI analyze it for possible updates.
          </p>
          
          <div className="bg-white shadow-md rounded-lg p-6 mb-8">
            <FileUploader onFileUploaded={handleFileUploaded} isLoading={isAnalyzing} />
          </div>
          
          {file && (
            <div className="flex justify-center">
              <Button 
                onClick={handleAnalyze} 
                disabled={isAnalyzing}
                className="px-8"
              >
                {isAnalyzing ? 'Analyzing...' : 'Analyze Presentation'}
              </Button>
            </div>
          )}
        </div>
      </main>
      
      <Footer />
    </div>
  );
};

export default Analyzer;
