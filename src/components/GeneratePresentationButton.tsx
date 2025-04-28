
import React from 'react';
import { Button } from '@/components/ui/button';
import { Download } from 'lucide-react';
import { Slide } from '@/types/presentation';

interface GeneratePresentationButtonProps {
  slides: Slide[];
  onGenerate: () => void;
}

const GeneratePresentationButton: React.FC<GeneratePresentationButtonProps> = ({ 
  slides, 
  onGenerate 
}) => {
  const completedCount = slides.filter(slide => 
    slide.status === 'approved' || slide.status === 'rejected' || slide.status === 'modified'
  ).length;
  
  const isComplete = completedCount >= slides.length;
  
  return (
    <div className="mt-8 text-center">
      <Button 
        size="lg" 
        onClick={onGenerate}
        disabled={!isComplete}
        className="px-8 bg-blue-600 hover:bg-blue-700"
      >
        <Download className="h-5 w-5 mr-2" />
        Generate Updated Presentation
      </Button>
      
      {!isComplete && (
        <p className="text-sm text-gray-500 mt-2">
          Please review all slides before generating your updated presentation.
        </p>
      )}
    </div>
  );
};

export default GeneratePresentationButton;
