
import React from 'react';
import { Progress } from '@/components/ui/progress';
import { Slide } from '@/types/presentation';

interface ProgressDisplayProps {
  slides: Slide[];
}

const ProgressDisplay: React.FC<ProgressDisplayProps> = ({ slides }) => {
  const completedCount = slides.filter(slide =>
    slide.status === 'approved' || slide.status === 'rejected' || slide.status === 'modified'
  ).length;

  const progressPercentage =
    slides.length > 0
      ? (completedCount / slides.length) * 100
      : 0;      

  return (
    <div className="mt-4 space-y-2">
      <div className="flex justify-between text-sm">
        <span>Review progress</span>
        <span>{completedCount} of {slides.length} slides reviewed</span>
      </div>
      <Progress value={progressPercentage} className="h-2 bg-gray-300" />
    </div>
  );
};

export default ProgressDisplay;
