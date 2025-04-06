
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import Header from '@/components/Header';
import Footer from '@/components/Footer';
import SlideComparison from '@/components/SlideComparison';
import ReferencesList from '@/components/ReferencesList';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { useToast } from '@/components/ui/use-toast';
import { mockPresentation, mockReferences } from '@/mockData/samplePresentation';
import { Presentation, Slide } from '@/types/presentation';
import { Download, ChevronLeft, ChevronRight } from 'lucide-react';

const PresentationAnalyzer = () => {
  const [presentation, setPresentation] = useState<Presentation>({
    ...mockPresentation,
    id: '1',
    title: '',
    author: 'You',
    originalFileName: '',
    uploadDate: new Date(),
    isAnalysisComplete: true,
    slides: mockPresentation.slides // We'll keep the mock slides for demonstration
  });
  const [currentIndex, setCurrentIndex] = useState(0);
  const { toast } = useToast();
  const navigate = useNavigate();
  
  // Load file information from sessionStorage
  useEffect(() => {
    const fileName = sessionStorage.getItem('uploadedFileName');
    const fileSize = sessionStorage.getItem('uploadedFileSize');
    const fileDate = sessionStorage.getItem('uploadedFileDate');
    
    if (fileName) {
      setPresentation(prev => ({
        ...prev,
        title: fileName.split('.')[0] || 'Untitled Presentation',
        originalFileName: fileName,
        uploadDate: fileDate ? new Date(fileDate) : new Date()
      }));
    }
  }, []);
  
  const completedCount = presentation.slides.filter(slide => 
    slide.status === 'approved' || slide.status === 'rejected' || slide.status === 'modified'
  ).length;
  
  const progressPercentage = (completedCount / presentation.slides.length) * 100;
  
  const handleApprove = (slideId: string) => {
    setPresentation(prev => ({
      ...prev,
      slides: prev.slides.map(slide => 
        slide.id === slideId ? { ...slide, status: 'approved' } : slide
      )
    }));
    
    toast({
      title: "Slide approved",
      description: "The suggested updates will be included in your final presentation."
    });
    
    // Move to next slide if available
    if (currentIndex < presentation.slides.length - 1) {
      setCurrentIndex(currentIndex + 1);
    }
  };
  
  const handleReject = (slideId: string) => {
    setPresentation(prev => ({
      ...prev,
      slides: prev.slides.map(slide => 
        slide.id === slideId ? { ...slide, status: 'rejected' } : slide
      )
    }));
    
    toast({
      title: "Slide rejected",
      description: "The original slide content will be preserved in your final presentation."
    });
    
    // Move to next slide if available
    if (currentIndex < presentation.slides.length - 1) {
      setCurrentIndex(currentIndex + 1);
    }
  };
  
  const handleEdit = (slideId: string) => {
    // In a real app, this would open a slide editor
    setPresentation(prev => ({
      ...prev,
      slides: prev.slides.map(slide => 
        slide.id === slideId ? { ...slide, status: 'modified' } : slide
      )
    }));
    
    toast({
      title: "Slide marked for manual editing",
      description: "You can edit this slide content in the final step."
    });
  };
  
  const handleGeneratePresentation = () => {
    // In a real app, this would generate and download the updated presentation
    toast({
      title: "Generating presentation",
      description: "Your updated presentation is being prepared for download."
    });
    
    // Simulate processing delay
    setTimeout(() => {
      toast({
        title: "Presentation ready",
        description: "Your updated presentation has been generated successfully."
      });
      
      // In a real app, this would trigger the actual download
      navigate('/download');
    }, 2000);
  };
  
  const currentSlide: Slide = presentation.slides[currentIndex];
  
  return (
    <div className="flex flex-col min-h-screen">
      <Header />
      
      <main className="container mx-auto px-4 py-8 flex-1">
        <div className="flex flex-col lg:flex-row gap-8">
          <div className="lg:w-3/4">
            <div className="mb-6">
              <h1 className="text-2xl font-bold text-gray-900">{presentation.title}</h1>
              <div className="text-sm text-gray-500 mt-1">
                {presentation.author && <span>By {presentation.author} • </span>}
                <span>Original file: {presentation.originalFileName}</span>
              </div>
              
              <div className="mt-4 space-y-2">
                <div className="flex justify-between text-sm">
                  <span>Review progress</span>
                  <span>{completedCount} of {presentation.slides.length} slides reviewed</span>
                </div>
                <Progress value={progressPercentage} className="h-2" />
              </div>
            </div>
            
            {/* Navigation controls */}
            <div className="flex justify-between mb-6">
              <Button 
                variant="outline" 
                onClick={() => setCurrentIndex(Math.max(0, currentIndex - 1))}
                disabled={currentIndex === 0}
                className="flex items-center"
              >
                <ChevronLeft className="h-4 w-4 mr-1" />
                Previous Slide
              </Button>
              
              <Button 
                variant="outline" 
                onClick={() => setCurrentIndex(Math.min(presentation.slides.length - 1, currentIndex + 1))}
                disabled={currentIndex === presentation.slides.length - 1}
                className="flex items-center"
              >
                Next Slide
                <ChevronRight className="h-4 w-4 ml-1" />
              </Button>
            </div>
            
            {/* Current slide comparison */}
            <SlideComparison 
              slide={currentSlide}
              onApprove={handleApprove}
              onReject={handleReject}
              onEdit={handleEdit}
            />
            
            {/* Generate button */}
            <div className="mt-8 text-center">
              <Button 
                size="lg" 
                onClick={handleGeneratePresentation}
                disabled={completedCount < presentation.slides.length}
                className="px-8"
              >
                <Download className="h-5 w-5 mr-2" />
                Generate Updated Presentation
              </Button>
              
              {completedCount < presentation.slides.length && (
                <p className="text-sm text-gray-500 mt-2">
                  Please review all slides before generating your updated presentation.
                </p>
              )}
            </div>
          </div>
          
          <div className="lg:w-1/4">
            <ReferencesList references={mockReferences} />
          </div>
        </div>
      </main>
      
      <Footer />
    </div>
  );
};

export default PresentationAnalyzer;
