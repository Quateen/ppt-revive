
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Header from '@/components/Header';
import Footer from '@/components/Footer';
import FileUploader from '@/components/FileUploader';
import { Button } from '@/components/ui/button';
import { useToast } from '@/components/ui/use-toast';
import { processPresentationFile } from '@/utils/fileParser';
import { Brain } from 'lucide-react';

const Index = () => {
  const [isLoading, setIsLoading] = useState(false);
  const { toast } = useToast();
  const navigate = useNavigate();

  const handleFileProcessed = async (file: File) => {
    setIsLoading(true);

    try {
      const presentationData = await processPresentationFile(file);
      
      if (presentationData) {
        // Store the data in session storage
        sessionStorage.setItem('presentationData', JSON.stringify(presentationData));
        
        toast({
          title: "Success!",
          description: "Your presentation has been analyzed. Redirecting...",
        });
        
        // Navigate to the analyzer page
        setTimeout(() => {
          navigate('/analyzer');
        }, 1500);
      }
    } catch (error) {
      console.error('Error processing file:', error);
      toast({
        variant: "destructive",
        title: "Error",
        description: "There was a problem processing your file. Please try again.",
      });
      setIsLoading(false);
    }
  };

  return (
    <div className="flex flex-col min-h-screen bg-black">
      <Header />
      
      <main className="flex-1 container mx-auto px-4 py-8">
        <div className="max-w-3xl mx-auto">
          <div className="glass-card rounded-lg p-6 mb-8 neuro-glow">
            <h1 className="text-3xl font-bold mb-2 text-white">Enhance Your Presentations with AI</h1>
            <p className="text-lg mb-6 text-gray-300">
              Upload your presentation and our neural AI will analyze and suggest improvements for your slides.
            </p>
            
            <div className="mb-6">
              <FileUploader onFileProcessed={handleFileProcessed} isLoading={isLoading} />
            </div>
            
            <div className="text-center">
              <Button
                variant="outline" 
                className="bg-neuro-gradient text-white border border-purple-700/50 hover:bg-purple-800"
                disabled={isLoading}
                onClick={() => {
                  // Use sample data for demo
                  const sampleData = require('@/mockData/samplePresentation').default;
                  sessionStorage.setItem('presentationData', JSON.stringify(sampleData));
                  
                  toast({
                    title: "Demo mode",
                    description: "Loading sample presentation data...",
                  });
                  
                  setTimeout(() => {
                    navigate('/analyzer');
                  }, 1500);
                }}
              >
                <Brain className="mr-2 h-5 w-5" />
                Try with Demo Presentation
              </Button>
            </div>
          </div>
          
          <div className="grid md:grid-cols-3 gap-6">
            <div className="glass-card rounded-lg p-6">
              <h3 className="text-xl font-semibold mb-2 text-white">AI-Powered Analysis</h3>
              <p className="text-gray-300">Our neural network examines your content for clarity, impact, and engagement potential.</p>
            </div>
            
            <div className="glass-card rounded-lg p-6">
              <h3 className="text-xl font-semibold mb-2 text-white">Smart Suggestions</h3>
              <p className="text-gray-300">Get recommendations for improvements based on best presentation practices.</p>
            </div>
            
            <div className="glass-card rounded-lg p-6">
              <h3 className="text-xl font-semibold mb-2 text-white">Easy Updates</h3>
              <p className="text-gray-300">Accept or reject suggestions and download your enhanced presentation.</p>
            </div>
          </div>
        </div>
      </main>
      
      <Footer />
    </div>
  );
};

export default Index;
