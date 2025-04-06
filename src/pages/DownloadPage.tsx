
import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import Header from '@/components/Header';
import Footer from '@/components/Footer';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Check, Download, FilePresentation, Home } from 'lucide-react';

const DownloadPage = () => {
  const [downloading, setDownloading] = useState(true);
  
  useEffect(() => {
    // Simulate download completion after 2 seconds
    const timer = setTimeout(() => {
      setDownloading(false);
    }, 2000);
    
    return () => clearTimeout(timer);
  }, []);
  
  return (
    <div className="flex flex-col min-h-screen">
      <Header />
      
      <main className="container mx-auto px-4 py-12 flex-1">
        <div className="max-w-lg mx-auto text-center">
          <div className="mb-6">
            {downloading ? (
              <div className="w-16 h-16 mx-auto bg-medical-100 rounded-full flex items-center justify-center">
                <FilePresentation className="h-8 w-8 text-medical-600 animate-pulse-slow" />
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
          
          {!downloading && (
            <Card className="mb-8">
              <CardContent className="pt-6">
                <div className="flex items-center justify-between">
                  <div className="flex items-center">
                    <div className="bg-medical-100 p-2 rounded-md">
                      <FilePresentation className="h-6 w-6 text-medical-600" />
                    </div>
                    <div className="ml-3 text-left">
                      <p className="font-medium text-gray-900">Type 2 Diabetes Management (Updated)</p>
                      <p className="text-xs text-gray-500">PPTX - 2.4MB</p>
                    </div>
                  </div>
                  
                  <Button size="sm">
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
                  Your presentation has been updated with 3 approved changes and includes a references slide with all sources properly cited.
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
