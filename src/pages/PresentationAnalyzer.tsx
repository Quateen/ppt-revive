
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import Header from '@/components/Header';
import Footer from '@/components/Footer';
import SlideComparison from '@/components/SlideComparison';
import ReferencesList from '@/components/ReferencesList';
import PresentationHeader from '@/components/PresentationHeader';
import ProgressDisplay from '@/components/ProgressDisplay';
import SlideNavigation from '@/components/SlideNavigation';
import GeneratePresentationButton from '@/components/GeneratePresentationButton';
import { Progress } from '@/components/ui/progress';
import { useToast } from '@/components/ui/use-toast';
import { Presentation, Slide, Reference } from '@/types/presentation';

const PresentationAnalyzer = () => {
  const [presentation, setPresentation] = useState<Presentation | null>(null);
  const [currentIndex, setCurrentIndex] = useState(0);
  const { toast } = useToast();
  const navigate = useNavigate();
  
  useEffect(() => {
    const storedData = sessionStorage.getItem('presentationData');
    
    if (storedData) {
      try {
        const parsedData = JSON.parse(storedData);
        
        console.log("Loaded presentation data:", parsedData);
        
        // Ensure dates are properly converted from strings to Date objects
        if (parsedData.uploadDate) {
          parsedData.uploadDate = new Date(parsedData.uploadDate);
        }
        
        setPresentation(parsedData);
      } catch (error) {
        console.error('Error parsing presentation data:', error);
        navigate('/');
        toast({
          variant: "destructive",
          title: "Error loading presentation",
          description: "Please try uploading your file again."
        });
      }
    } else {
      navigate('/');
      toast({
        variant: "destructive",
        title: "No presentation found",
        description: "Please upload a presentation file first."
      });
    }
  }, [navigate, toast]);
  
  // Save presentation data to sessionStorage whenever it changes
  useEffect(() => {
    if (presentation) {
      try {
        // Save the current state of the presentation
        sessionStorage.setItem('presentationData', JSON.stringify(presentation));
        console.log("Saved updated presentation data to sessionStorage");
      } catch (error) {
        console.error('Error saving presentation data to sessionStorage:', error);
        toast({
          variant: "destructive",
          title: "Error saving progress",
          description: "We couldn't save your progress. Please don't refresh the page."
        });
      }
    }
  }, [presentation, toast]);
  
  if (!presentation) {
    return (
      <div className="flex flex-col min-h-screen">
        <Header />
        <main className="container mx-auto px-4 py-8 flex-1 flex items-center justify-center">
          <div className="text-center">
            <h2 className="text-xl font-medium mb-2">Loading presentation...</h2>
            <Progress value={50} className="w-64 h-2" />
          </div>
        </main>
        <Footer />
      </div>
    );
  }
  
  const handleApprove = (slideId: string) => {
    setPresentation(prev => {
      if (!prev) return prev;
      
      // Create a new slides array with the updated slide, ensuring correct typing
      const updatedSlides = prev.slides.map(slide => {
        if (slide.id === slideId) {
          return { ...slide, status: 'approved' as const };
        }
        return slide;
      });
      
      return {
        ...prev,
        slides: updatedSlides,
        lastUpdated: new Date()
      };
    });
    
    toast({
      title: "Slide approved",
      description: "The suggested updates will be included in your final presentation."
    });
    
    if (currentIndex < presentation.slides.length - 1) {
      setCurrentIndex(currentIndex + 1);
    }
  };
  
  const handleReject = (slideId: string) => {
    setPresentation(prev => {
      if (!prev) return prev;
      
      // Create a new slides array with the updated slide, ensuring correct typing
      const updatedSlides = prev.slides.map(slide => {
        if (slide.id === slideId) {
          return { ...slide, status: 'rejected' as const };
        }
        return slide;
      });
      
      return {
        ...prev,
        slides: updatedSlides,
        lastUpdated: new Date()
      };
    });
    
    toast({
      title: "Slide rejected",
      description: "The original slide content will be preserved in your final presentation."
    });
    
    if (currentIndex < presentation.slides.length - 1) {
      setCurrentIndex(currentIndex + 1);
    }
  };
  
  const handleEdit = (slideId: string) => {
    setPresentation(prev => {
      if (!prev) return prev;
      
      // Create a new slides array with the updated slide, ensuring correct typing
      const updatedSlides = prev.slides.map(slide => {
        if (slide.id === slideId) {
          return { ...slide, status: 'modified' as const };
        }
        return slide;
      });
      
      return {
        ...prev,
        slides: updatedSlides,
        lastUpdated: new Date()
      };
    });
    
    toast({
      title: "Slide marked for manual editing",
      description: "You can edit this slide content in the final step."
    });
  };
  
  const handleNavigatePrevious = () => {
    setCurrentIndex(Math.max(0, currentIndex - 1));
  };
  
  const handleNavigateNext = () => {
    setCurrentIndex(Math.min(presentation.slides.length - 1, currentIndex + 1));
  };
  
  const handleGeneratePresentation = () => {
    // Store the updated presentation data with all the slide status changes
    if (presentation) {
      // Make sure we're using the latest state when saving to sessionStorage
      const finalPresentationData = {
        ...presentation,
        generatedAt: new Date().toISOString(),
        completedCount: presentation.slides.filter(slide => 
          slide.status === 'approved' || slide.status === 'rejected' || slide.status === 'modified'
        ).length,
        isAnalysisComplete: true
      };
      
      try {
        sessionStorage.setItem('finalPresentationData', JSON.stringify(finalPresentationData));
        console.log("Saved final presentation data:", finalPresentationData);
        
        toast({
          title: "Generating presentation",
          description: "Your updated presentation is being prepared for download."
        });
        
        setTimeout(() => {
          navigate('/download');
        }, 1500);
      } catch (error) {
        console.error('Error saving final presentation data:', error);
        toast({
          variant: "destructive",
          title: "Error generating presentation",
          description: "There was an error preparing your download."
        });
      }
    }
  };
  
  const safeIndex = Math.min(currentIndex, presentation.slides.length - 1);
  const currentSlide: Slide = presentation.slides[safeIndex];
  const references: Reference[] = presentation.references || [];
  
  return (
    <div className="flex flex-col min-h-screen">
      <Header />
      
      <main className="container mx-auto px-4 py-8 flex-1">
        <div className="flex flex-col lg:flex-row gap-8">
          <div className="lg:w-3/4">
            <PresentationHeader 
              title={presentation.title}
              author={presentation.author}
              originalFileName={presentation.originalFileName}
            />
            
            <ProgressDisplay slides={presentation.slides} />
            
            <SlideNavigation
              currentIndex={currentIndex}
              totalSlides={presentation.slides.length}
              onPrevious={handleNavigatePrevious}
              onNext={handleNavigateNext}
            />
            
            {currentSlide && (
              <SlideComparison 
                slide={currentSlide}
                onApprove={handleApprove}
                onReject={handleReject}
                onEdit={handleEdit}
              />
            )}
            
            <GeneratePresentationButton 
              slides={presentation.slides}
              onGenerate={handleGeneratePresentation}
            />
          </div>
          
          <div className="lg:w-1/4">
            <ReferencesList references={references} />
          </div>
        </div>
      </main>
      
      <Footer />
    </div>
  );
};

export default PresentationAnalyzer;
