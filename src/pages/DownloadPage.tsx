
import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import Header from '@/components/Header';
import Footer from '@/components/Footer';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Check, Download, FileText, Home } from 'lucide-react';
import { Presentation } from '@/types/presentation';
import { useToast } from '@/components/ui/use-toast';

const DownloadPage = () => {
  const [downloading, setDownloading] = useState(true);
  const [presentation, setPresentation] = useState<Presentation | null>(null);
  const [downloadReady, setDownloadReady] = useState(false);
  const { toast } = useToast();
  
  useEffect(() => {
    // Get the final presentation data from sessionStorage
    const storedData = sessionStorage.getItem('finalPresentationData');
    
    if (storedData) {
      try {
        const parsedData = JSON.parse(storedData);
        console.log("Loading presentation data for download:", parsedData);
        setPresentation(parsedData);
        setDownloadReady(true);
      } catch (error) {
        console.error('Error parsing final presentation data:', error);
        toast({
          variant: "destructive",
          title: "Error loading presentation",
          description: "Could not load your presentation data."
        });
      }
    } else {
      console.warn("No presentation data found in sessionStorage");
      toast({
        variant: "destructive",
        title: "No presentation found",
        description: "Could not find any presentation data to download."
      });
    }
    
    // Simulate download preparation completion after 2 seconds
    const timer = setTimeout(() => {
      setDownloading(false);
    }, 2000);
    
    return () => clearTimeout(timer);
  }, [toast]);
  
  // Count slides that were approved/modified
  const approvedCount = presentation?.slides.filter(
    slide => slide.status === 'approved' || slide.status === 'modified'
  ).length || 0;
  
  // Format file size for display (mock value)
  const fileSize = '2.4MB';
  
  // Get original filename without extension
  const fileNameBase = presentation?.originalFileName 
    ? presentation.originalFileName.replace(/\.[^/.]+$/, '')
    : 'presentation';
    
  const downloadFileName = `${fileNameBase} (Updated).pptx`;
  
  const handleDownload = () => {
    if (!presentation) {
      toast({
        variant: "destructive",
        title: "Download failed",
        description: "No presentation data available to download."
      });
      return;
    }
    
    toast({
      title: "Starting download",
      description: `Downloading ${downloadFileName}`
    });
    
    try {
      // In a real implementation, we would generate the PPTX file
      // For this demo, we'll create a JSON file to simulate the presentation download
      const content = JSON.stringify(presentation, null, 2);
      const blob = new Blob([content], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      
      // Create download link and trigger the download
      const a = document.createElement('a');
      a.href = url;
      a.download = downloadFileName.replace('.pptx', '.json'); // Use JSON extension for the demo
      document.body.appendChild(a);
      a.click();
      
      // Clean up
      setTimeout(() => {
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
      }, 100);
      
      toast({
        variant: "default", // Changed from "success" to "default" as only "default" and "destructive" are valid variants
        title: "Download complete",
        description: `${downloadFileName} has been downloaded successfully.`,
      });
    } catch (error) {
      console.error('Error downloading presentation:', error);
      toast({
        variant: "destructive",
        title: "Download failed",
        description: "An error occurred while downloading your presentation."
      });
    }
  };
  
  return (
    <div className="flex flex-col min-h-screen">
      <Header />
      
      <main className="container mx-auto px-4 py-12 flex-1">
        <div className="max-w-lg mx-auto text-center">
          <div className="mb-6">
            {downloading ? (
              <div className="w-16 h-16 mx-auto bg-blue-100 rounded-full flex items-center justify-center animate-pulse">
                <FileText className="h-8 w-8 text-blue-600" />
              </div>
            ) : (
              <div className="w-16 h-16 mx-auto bg-green-100 rounded-full flex items-center justify-center">
                <Check className="h-8 w-8 text-green-600" />
              </div>
            )}
          </div>
          
          <h1 className="text-2xl font-bold text-gray-900 mb-3">
            {downloading ? "Preparing Your Presentation" : "Your Presentation Is Ready"}
          </h1>
          
          <p className="text-gray-600 mb-6">
            {downloading 
              ? "Please wait while we generate your updated presentation file..." 
              : "Your updated presentation has been generated successfully with all approved changes."}
          </p>
          
          {!downloading && presentation && (
            <Card className="mb-8 border-2 border-blue-100 shadow-md">
              <CardContent className="pt-6">
                <div className="flex items-center justify-between">
                  <div className="flex items-center">
                    <div className="bg-blue-100 p-2 rounded-md">
                      <FileText className="h-6 w-6 text-blue-600" />
                    </div>
                    <div className="ml-3 text-left">
                      <p className="font-medium text-gray-900">{downloadFileName.replace('.pptx', '.json')}</p>
                      <p className="text-xs text-gray-500">JSON - {fileSize}</p>
                    </div>
                  </div>
                  
                  <Button 
                    size="sm" 
                    onClick={handleDownload} 
                    disabled={!downloadReady}
                    className="bg-blue-600 hover:bg-blue-700"
                  >
                    <Download className="h-4 w-4 mr-2" />
                    Download
                  </Button>
                </div>
              </CardContent>
            </Card>
          )}
          
          {!downloading && (
            <>
              <div className="mb-6">
                <p className="text-sm text-gray-500">
                  Your presentation has been updated with {approvedCount} approved changes and includes a references slide with all sources properly cited.
                </p>
                <p className="text-xs text-gray-400 mt-2">
                  Note: This demo downloads a JSON file. In a production app, a real PPTX file would be generated.
                </p>
              </div>
              
              <div className="flex flex-col sm:flex-row justify-center gap-4">
                <Button variant="outline" asChild>
                  <Link to="/" className="flex items-center">
                    <Home className="h-4 w-4 mr-2" />
                    Return Home
                  </Link>
                </Button>
                
                <Button asChild className="bg-blue-600 hover:bg-blue-700">
                  <Link to="/" className="flex items-center">
                    Start New Update
                  </Link>
                </Button>
              </div>
            </>
          )}
        </div>
      </main>
      
      <Footer />
    </div>
  );
};

export default DownloadPage;
