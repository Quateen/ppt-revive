import React from 'react';
import { Button } from '@/components/ui/button';
import { Download } from 'lucide-react';
import { Slide } from '@/types/presentation';

interface GeneratePresentationButtonProps {
  slides: Slide[];
  onGenerate: () => void;
  loading?: boolean; // ✅ optional loading prop
}

const GeneratePresentationButton: React.FC<GeneratePresentationButtonProps> = ({
  slides,
  onGenerate,
  loading = false
}) => {
  const completedCount = slides.filter(slide =>
    slide.status === 'approved' || slide.status === 'rejected' || slide.status === 'modified'
  ).length;

  const isComplete = completedCount >= slides.length;

  return (
    <div className="mt-8 flex flex-col items-center text-center">
      <Button
        size="lg"
        onClick={onGenerate}
        disabled={!isComplete || loading}
        className="px-8 bg-blue-600 hover:bg-blue-700 flex items-center justify-center"
      >
        {loading ? (
          <span className="h-5 w-5 border-2 border-white border-t-transparent rounded-full animate-spin mr-2"></span>
        ) : (
          <Download className="h-5 w-5 mr-2" />
        )}
        {loading ? 'Generating...' : 'Generate Updated Presentation'}
      </Button>

      {!isComplete && !loading && (
        <p className="text-sm text-gray-500 mt-2">
          Please review all slides before generating your updated presentation.
        </p>
      )}
    </div>

  );
};

export default GeneratePresentationButton;
