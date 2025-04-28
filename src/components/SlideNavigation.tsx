
import React from 'react';
import { Button } from '@/components/ui/button';
import { ChevronLeft, ChevronRight } from 'lucide-react';

interface SlideNavigationProps {
  currentIndex: number;
  totalSlides: number;
  onPrevious: () => void;
  onNext: () => void;
}

const SlideNavigation: React.FC<SlideNavigationProps> = ({ 
  currentIndex, 
  totalSlides, 
  onPrevious, 
  onNext 
}) => {
  return (
    <div className="flex justify-between mb-6">
      <Button 
        variant="outline" 
        onClick={onPrevious}
        disabled={currentIndex === 0}
        className="flex items-center"
      >
        <ChevronLeft className="h-4 w-4 mr-1" />
        Previous Slide
      </Button>
      
      <Button 
        variant="outline" 
        onClick={onNext}
        disabled={currentIndex === totalSlides - 1}
        className="flex items-center"
      >
        Next Slide
        <ChevronRight className="h-4 w-4 ml-1" />
      </Button>
    </div>
  );
};

export default SlideNavigation;
