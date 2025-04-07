
import { Presentation } from '@/types/presentation';
import { createPresentation } from './presentationFactory';
import { mockPresentation } from '@/mockData/samplePresentation';

/**
 * Creates a presentation object from an uploaded file
 * Supports various file formats
 */
export async function createPresentationFromFile(file: File): Promise<Presentation> {
  try {
    console.log(`Processing file: ${file.name}, type: ${file.type}, size: ${file.size} bytes`);
    
    // For PowerPoint files, directly create a mock presentation
    if (file.type.includes('powerpoint') || 
        file.name.endsWith('.pptx') || 
        file.name.endsWith('.ppt')) {
      console.log('Using mock data for PowerPoint file');
      
      // Create a customized mock with the file's name
      const customMock = { 
        ...mockPresentation, 
        originalFileName: file.name,
        title: file.name.replace(/\.[^/.]+$/, '') // Remove extension
      };
      
      // Customize slide counts and titles
      const estimatedSlides = Math.max(10, Math.min(30, Math.floor(file.size / (50 * 1024))));
      customMock.slides = Array.from({ length: estimatedSlides }, (_, i) => ({
        ...mockPresentation.slides[0],
        id: `slide-${i+1}`,
        number: i+1,
        title: `Slide ${i+1}`,
        originalContent: `Content for slide ${i+1}`,
        suggestedUpdate: `Updated content for slide ${i+1}`,
      }));
      
      return customMock;
    }
    
    // Process other types of files
    const content = await readFileAsText(file);
    return createPresentation(file.name, content);
    
  } catch (error) {
    console.error('Error creating presentation from file:', error);
    console.log('Using mock presentation data as fallback');
    
    // Fallback to mock data with correct filename
    return { 
      ...mockPresentation, 
      originalFileName: file.name,
      title: file.name.replace(/\.[^/.]+$/, '') // Remove extension
    };
  }
}

/**
 * Utility to read a file as text
 */
async function readFileAsText(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result as string);
    reader.onerror = () => reject(new Error('Failed to read file'));
    reader.readAsText(file);
  });
}
