
import { Presentation, Slide } from '@/types/presentation';
import * as XLSX from 'xlsx';

// Legacy parser using XLSX (as fallback)
const extractTextWithXLSX = async (file: File): Promise<string[]> => {
  try {
    // Read the file as an ArrayBuffer
    const buffer = await file.arrayBuffer();
    
    // Use XLSX to parse the PowerPoint file
    const workbook = XLSX.read(buffer, { type: 'array', bookVBA: true });
    
    // Extract content from each sheet (slide)
    const slideContents: string[] = [];
    
    workbook.SheetNames.forEach(sheetName => {
      const worksheet = workbook.Sheets[sheetName];
      
      // Convert worksheet to text
      const textContent = XLSX.utils.sheet_to_txt(worksheet);
      if (textContent.trim()) {
        slideContents.push(textContent);
      }
    });
    
    // If we couldn't extract slides, return a default message
    if (slideContents.length === 0) {
      console.log("Could not extract text from PowerPoint file with XLSX");
      return ["No text content could be extracted from this PowerPoint file"];
    }
    
    return slideContents;
  } catch (error) {
    console.error("Error parsing PowerPoint file with XLSX:", error);
    throw error;
  }
};

// Extract text content from PowerPoint files using Cloudmersive API
export const extractTextFromPowerPoint = async (file: File): Promise<string[]> => {
  try {
    // Check if file is a PowerPoint file
    if (!file.name.match(/\.(ppt|pptx|pot|potx|pps|ppsx|pptm|potm|ppsm)$/i)) {
      throw new Error("File is not a PowerPoint file");
    }
    
    // Try to use Cloudmersive API first
    try {
      const formData = new FormData();
      formData.append("inputFile", file);
      
      const response = await fetch("https://api.cloudmersive.com/convert/pptx/to/txt", {
        method: "POST",
        headers: {
          "Apikey": "af6862af-3416-4e3a-989c-f3531f34bbb5" // Using the provided API key
        },
        body: formData,
      });
      
      if (!response.ok) {
        throw new Error(`Cloudmersive API error: ${response.statusText}`);
      }
      
      const textContent = await response.text();
      
      // Split the content by slides (this is a simplification, the actual response format may vary)
      // Cloudmersive typically returns text with slide markers
      const slideRegex = /===== Slide \d+ =====\n/g;
      const slides = textContent.split(slideRegex).filter(text => text.trim().length > 0);
      
      if (slides.length > 0) {
        return slides;
      } else {
        throw new Error("No slides extracted from Cloudmersive response");
      }
    } catch (cloudmersiveError) {
      console.error("Cloudmersive API error:", cloudmersiveError);
      console.log("Falling back to XLSX parser...");
      
      // Fall back to XLSX parser if Cloudmersive fails
      return await extractTextWithXLSX(file);
    }
  } catch (error) {
    console.error("Error parsing PowerPoint file:", error);
    
    // Last resort fallback
    return ["Error parsing PowerPoint file. Please try a different file format."];
  }
};

// Generate slides from extracted text content
export const generateSlidesFromFile = async (file: File): Promise<Slide[]> => {
  const slides: Slide[] = [];
  
  try {
    // Extract text content from the PowerPoint file
    const slideContents = await extractTextFromPowerPoint(file);
    
    // Create slides based on extracted content
    slideContents.forEach((content, index) => {
      // Generate a slide title from the first line or use default
      const lines = content.split('\n').filter(line => line.trim().length > 0);
      const title = lines.length > 0 ? lines[0].substring(0, 50) : `Slide ${index + 1}`;
      
      slides.push({
        id: `s${index + 1}`,
        number: index + 1,
        title: title,
        originalContent: content,
        suggestedUpdate: `Updated version of: ${content.substring(0, 100)}... \nThis would typically contain AI-generated updates based on recent medical research.`,
        updateReason: "This update would be based on recent medical research findings.",
        sourceCitations: [
          "Example Citation (2023). Journal of Medical Updates.",
          "Recent Medical Study (2024). New Findings in Medical Research."
        ],
        status: 'pending'
      });
    });
    
    // If no slides were created (parsing failed), create a default slide
    if (slides.length === 0) {
      slides.push({
        id: 's1',
        number: 1,
        title: 'Parsing Error',
        originalContent: 'Could not parse the PowerPoint file content. This could be due to file format limitations or encryption.',
        suggestedUpdate: 'Please try with a different PowerPoint file format or contact support.',
        updateReason: "Error in file parsing",
        sourceCitations: [],
        status: 'pending'
      });
    }
    
    return slides;
  } catch (error) {
    console.error("Error generating slides:", error);
    
    // Return a default slide indicating the error
    return [{
      id: 's1',
      number: 1,
      title: 'Error',
      originalContent: 'An error occurred while processing the presentation file.',
      suggestedUpdate: 'Please try uploading the file again or use a different file format.',
      updateReason: "File processing error",
      sourceCitations: [],
      status: 'pending'
    }];
  }
};

export const createPresentationFromFile = async (file: File): Promise<Presentation> => {
  const slides = await generateSlidesFromFile(file);
  
  return {
    id: crypto.randomUUID(),
    title: file.name.split('.')[0] || 'Untitled Presentation',
    author: 'You',
    originalFileName: file.name,
    uploadDate: new Date(),
    isAnalysisComplete: true,
    slides: slides
  };
};
