
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
  
  // Check if the content appears to be binary or has encoding issues
  const isBinaryContent = checkForBinaryContent(content);
  
  // Extract all slides from content
  const rawSlides = isBinaryContent ? ['[Binary PowerPoint Content]'] : extractSlidesFromContent(content);
  console.log(`Extracted ${rawSlides.length} slides from content`);
  
  // Process slides into structured Slide objects
  const slides: Slide[] = rawSlides.map((slideContent, index) => {
    const cleanedContent = cleanSlideContent(slideContent);
    const slideTitle = extractTitleFromSlide(cleanedContent) || `Slide ${index + 1}`;
    
    return {
      id: uuidv4(),
      number: index + 1,
      title: slideTitle,
      originalContent: cleanedContent,
      suggestedUpdate: cleanedContent, // Initially the same as original
      status: 'pending',
    };
  });
  
  // If binary content and no actual slides were detected, create placeholder slides
  if (isBinaryContent && (slides.length === 0 || 
     (slides.length === 1 && slides[0].originalContent === '[Binary PowerPoint Content]'))) {
    // Estimate slide count based on file size (rough approximation)
    const estimatedSlideCount = 10; // Default to 10 slides for binary content
    
    // Create placeholder slides
    for (let i = 0; i < estimatedSlideCount; i++) {
      slides.push({
        id: uuidv4(),
        number: i + 1,
        title: `Slide ${i + 1}`,
        originalContent: `[Binary PowerPoint Content - Slide ${i + 1}]`,
        suggestedUpdate: `[Binary PowerPoint Content - Slide ${i + 1}]`,
        status: 'pending',
      });
    }
  }
  
  // Extract presentation title from first slide or filename
  let title = fileName.replace(/\.[^/.]+$/, ''); // Default to filename without extension
  
  if (slides.length > 0 && !slides[0].title.startsWith('Slide ')) {
    title = slides[0].title;
  }
  
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

/**
 * Checks if content appears to be binary or has encoding issues
 */
function checkForBinaryContent(content: string): boolean {
  if (!content) return false;
  
  // Calculate the ratio of non-printable characters
  let nonTextChars = 0;
  
  // Sample the content (check up to 1000 characters to save performance)
  const sampleSize = Math.min(content.length, 1000);
  
  for (let i = 0; i < sampleSize; i++) {
    const char = content.charCodeAt(i);
    
    // Check for control characters or unusual Unicode ranges
    if ((char < 32 && ![9, 10, 13].includes(char)) || // Not tab, LF, CR
        (char >= 127 && char <= 159) ||              // Control characters
        (char >= 0xD800 && char <= 0xDFFF)) {        // Surrogate pairs
      nonTextChars++;
    }
  }
  
  // If more than 5% of the sampled content contains non-text chars, consider it binary
  return (nonTextChars / sampleSize) > 0.05;
}
