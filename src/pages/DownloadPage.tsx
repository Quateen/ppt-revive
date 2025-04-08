
import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import Header from '@/components/Header';
import Footer from '@/components/Footer';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Check, Download, FileType, Home } from 'lucide-react';
import { Presentation } from '@/types/presentation';

const DownloadPage = () => {
  const [downloading, setDownloading] = useState(true);
  const [presentation, setPresentation] = useState<Presentation | null>(null);
  
  useEffect(() => {
    // Get the final presentation data from sessionStorage
    const storedData = sessionStorage.getItem('finalPresentationData');
    
    if (storedData) {
      try {
        const parsedData = JSON.parse(storedData);
        setPresentation(parsedData);
      } catch (error) {
        console.error('Error parsing final presentation data:', error);
      }
    }
    
    // Simulate download completion after 2 seconds
    const timer = setTimeout(() => {
      setDownloading(false);
    }, 2000);
    
    return () => clearTimeout(timer);
  }, []);
  
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
    // In a real implementation, this would generate and download the file
    // For now, we'll just simulate a download with an alert
    alert(`Downloading ${downloadFileName}`);
    
    // In a real app, you would do something like:
    // const blob = new Blob([data], { type: 'application/vnd.openxmlformats-officedocument.presentationml.presentation' });
    // const url = URL.createObjectURL(blob);
    // const a = document.createElement('a');
    // a.href = url;
    // a.download = downloadFileName;
    // a.click();
    // URL.revokeObjectURL(url);
  };
  
  return (
    <div className="flex flex-col min-h-screen">
      <Header />
      
      <main className="container mx-auto px-4 py-12 flex-1">
        <div className="max-w-lg mx-auto text-center">
          <div className="mb-6">
            {downloading ? (
              <div className="w-16 h-16 mx-auto bg-blue-100 rounded-full flex items-center justify-center">
                <FileType className="h-8 w-8 text-blue-600 animate-pulse" />
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
            <Card className="mb-8">
              <CardContent className="pt-6">
                <div className="flex items-center justify-between">
                  <div className="flex items-center">
                    <div className="bg-blue-100 p-2 rounded-md">
                      <FileType className="h-6 w-6 text-blue-600" />
                    </div>
                    <div className="ml-3 text-left">
                      <p className="font-medium text-gray-900">{downloadFileName}</p>
                      <p className="text-xs text-gray-500">PPTX - {fileSize}</p>
                    </div>
                  </div>
                  
                  <Button size="sm" onClick={handleDownload}>
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
              </div>
              
              <div className="flex flex-col sm:flex-row justify-center gap-4">
                <Button variant="outline" asChild>
                  <Link to="/" className="flex items-center">
                    <Home className="h-4 w-4 mr-2" />
                    Return Home
                  </Link>
                </Button>
                
                <Button asChild>
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
