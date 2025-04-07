
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
    
    // Set timeout to handle potential long-running operations
    const timeoutPromise = new Promise<never>((_, reject) => {
      setTimeout(() => reject(new Error('Processing timed out')), 30000); // 30 second timeout
    });
    
    const processingPromise = processFile(file);
    
    // Race between actual processing and timeout
    return Promise.race([processingPromise, timeoutPromise]);
  } catch (error) {
    console.error('Error creating presentation from file:', error);
    
    // Fallback to mock data if processing fails
    console.log('Using mock presentation data as fallback');
    return { ...mockPresentation, originalFileName: file.name };
  }
}

/**
 * Process the uploaded file based on its type
 */
async function processFile(file: File): Promise<Presentation> {
  const fileName = file.name;
  const fileType = file.type;
  
  // For development/demo purposes, always return mock data
  // In production, you would remove this and process actual files
  if (process.env.NODE_ENV === 'development' || process.env.REACT_APP_USE_MOCK_DATA === 'true') {
    console.log('Using mock presentation data (development mode)');
    return { ...mockPresentation, originalFileName: fileName };
  }
  
  // Process based on file type
  if (fileType.includes('powerpoint') || fileName.endsWith('.pptx') || fileName.endsWith('.ppt')) {
    return processPowerPointFile(file);
  } else if (fileType.includes('pdf') || fileName.endsWith('.pdf')) {
    return processPdfFile(file);
  } else if (fileType.includes('text') || fileName.endsWith('.txt') || fileName.endsWith('.md')) {
    return processTextFile(file);
  } else {
    throw new Error(`Unsupported file type: ${fileType}`);
  }
}

/**
 * Process PowerPoint files
 */
async function processPowerPointFile(file: File): Promise<Presentation> {
  console.log('Processing PowerPoint file...');
  
  try {
    // Try multiple methods to read the file content
    let content: string;
    
    // First try reading as text
    try {
      content = await readFileAsText(file);
    } catch (error) {
      // If reading as text fails, try a binary approach
      // For this example, we'll generate a placeholder content with slides based on file size
      console.log('Text parsing failed, using binary content placeholder');
      content = generateBinaryContentPlaceholder(file);
    }
    
    return createPresentation(file.name, content);
  } catch (error) {
    console.error('Error processing PowerPoint file:', error);
    throw error;
  }
}

/**
 * Process PDF files
 */
async function processPdfFile(file: File): Promise<Presentation> {
  console.log('Processing PDF file...');
  
  try {
    // In a real implementation, you would use a PDF parsing library
    // For this example, we'll read it as text
    let content: string;
    
    try {
      content = await readFileAsText(file);
    } catch (error) {
      console.log('Text parsing failed, using binary content placeholder');
      content = generateBinaryContentPlaceholder(file);
    }
    
    return createPresentation(file.name, content);
  } catch (error) {
    console.error('Error processing PDF file:', error);
    throw error;
  }
}

/**
 * Process plain text files
 */
async function processTextFile(file: File): Promise<Presentation> {
  console.log('Processing text file...');
  
  try {
    const content = await readFileAsText(file);
    return createPresentation(file.name, content);
  } catch (error) {
    console.error('Error processing text file:', error);
    throw error;
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

/**
 * Generate placeholder content for binary files based on the file size
 */
function generateBinaryContentPlaceholder(file: File): string {
  // Estimate the number of slides based on file size
  // This is a very rough approximation just for demonstration
  const fileSizeKB = file.size / 1024;
  const estimatedSlides = Math.max(1, Math.min(30, Math.floor(fileSizeKB / 50)));
  
  let content = '';
  
  // Generate placeholder slides
  for (let i = 0; i < estimatedSlides; i++) {
    content += `# Slide ${i + 1}\n\nContent for slide ${i + 1}\n\n---\n\n`;
  }
  
  return content;
}
