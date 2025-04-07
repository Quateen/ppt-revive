
import { v4 as uuidv4 } from 'uuid';
import { Presentation, Slide } from '@/types/presentation';
import { extractTitleFromSlide } from './titleExtractor';
import { cleanSlideContent, extractSlidesFromContent } from './slideExtractor';

/**
 * Creates a Presentation object from raw content
 */
export function createPresentation(
  fileName: string,
  content: string,
  author: string = ''
): Presentation {
  console.log(`Creating presentation from file: ${fileName}`);
  
  // Extract all slides from content
  const rawSlides = extractSlidesFromContent(content);
  console.log(`Extracted ${rawSlides.length} slides from content`);
  
  // Process slides into structured Slide objects
  const slides: Slide[] = rawSlides.map((slideContent, index) => {
    const cleanedContent = cleanSlideContent(slideContent);
    const slideTitle = extractTitleFromSlide(cleanedContent);
    
    return {
      id: uuidv4(),
      number: index + 1,
      title: slideTitle,
      originalContent: cleanedContent,
      suggestedUpdate: cleanedContent, // Initially the same as original
      status: 'pending',
    };
  });
  
  // Extract presentation title from first slide or filename
  const title = slides.length > 0 ? 
    extractTitleFromSlide(slides[0].originalContent) : 
    fileName.replace(/\.[^/.]+$/, ''); // Remove extension
  
  return {
    id: uuidv4(),
    title: title,
    author: author,
    originalFileName: fileName,
    uploadDate: new Date(),
    slides: slides,
    isAnalysisComplete: false,
  };
}
