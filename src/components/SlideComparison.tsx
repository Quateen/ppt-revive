
import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { ThumbsUp, ThumbsDown, Pencil } from 'lucide-react';
import { Separator } from '@/components/ui/separator';
import { Slide } from '@/types/presentation';

interface SlideComparisonProps {
  slide: Slide;
  onApprove: (slideId: string) => void;
  onReject: (slideId: string) => void;
  onEdit: (slideId: string) => void;
}

const SlideComparison: React.FC<SlideComparisonProps> = ({
  slide,
  onApprove,
  onReject,
  onEdit
}) => {
  // Ensure slide content is displayed properly by handling empty content
  const originalContent = slide.originalContent || 'No content available for this slide';
  const suggestedUpdate = slide.suggestedUpdate || 'No suggested update available for this slide';
  
  return (
    <Card className="mb-6 shadow-md">
      <CardHeader className="pb-3">
        <div className="flex justify-between items-center">
          <CardTitle className="text-lg">Slide {slide.number}: {slide.title || `Untitled Slide ${slide.number}`}</CardTitle>
          <Badge variant={slide.status === 'pending' ? 'outline' : 'default'} className={
            slide.status === 'approved' ? 'bg-green-100 text-green-800 hover:bg-green-100' : 
            slide.status === 'rejected' ? 'bg-red-100 text-red-800 hover:bg-red-100' : 
            slide.status === 'modified' ? 'bg-blue-100 text-blue-800 hover:bg-blue-100' : ''
          }>
            {slide.status.charAt(0).toUpperCase() + slide.status.slice(1)}
          </Badge>
        </div>
      </CardHeader>
      
      <CardContent>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <h3 className="text-sm font-medium text-gray-500 mb-2">Original Content</h3>
            <div className="bg-gray-50 p-4 rounded-md min-h-[200px] text-gray-800 whitespace-pre-wrap border border-gray-200">
              {originalContent}
            </div>
          </div>
          
          <div>
            <h3 className="text-sm font-medium text-gray-500 mb-2">Suggested Update</h3>
            <div className="bg-blue-50 p-4 rounded-md min-h-[200px] text-gray-800 whitespace-pre-wrap border border-blue-100">
              {suggestedUpdate}
            </div>
          </div>
        </div>
        
        {slide.updateReason && (
          <>
            <Separator className="my-4" />
            <div>
              <h3 className="text-sm font-medium text-gray-500 mb-2">Reason for Update</h3>
              <p className="text-gray-700 text-sm">{slide.updateReason}</p>
            </div>
          </>
        )}
        
        {slide.sourceCitations && slide.sourceCitations.length > 0 && (
          <>
            <Separator className="my-4" />
            <div>
              <h3 className="text-sm font-medium text-gray-500 mb-2">Sources</h3>
              <div className="text-xs text-gray-600 space-y-1">
                {slide.sourceCitations.map((citation, index) => (
                  <p key={index}>{citation}</p>
                ))}
              </div>
            </div>
          </>
        )}
        
        <div className="flex justify-end gap-3 mt-6">
          <Button 
            variant="outline" 
            onClick={() => onReject(slide.id)}
            className="text-gray-700"
            disabled={slide.status !== 'pending'}
          >
            <ThumbsDown className="h-4 w-4 mr-2" />
            Reject
          </Button>
          <Button 
            variant="outline" 
            onClick={() => onEdit(slide.id)}
            className="text-blue-700"
            disabled={slide.status !== 'pending'}
          >
            <Pencil className="h-4 w-4 mr-2" />
            Edit
          </Button>
          <Button 
            onClick={() => onApprove(slide.id)}
            disabled={slide.status !== 'pending'}
          >
            <ThumbsUp className="h-4 w-4 mr-2" />
            Approve
          </Button>
        </div>
      </CardContent>
    </Card>
  );
};

export default SlideComparison;
