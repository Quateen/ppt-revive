
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
      
      console.log("Sending file to Cloudmersive API:", file.name);
      const response = await fetch("https://api.cloudmersive.com/convert/pptx/to/txt", {
        method: "POST",
        headers: {
          "Apikey": "af6862af-3416-4e3a-989c-f3531f34bbb5"
        },
        body: formData,
      });
      
      if (!response.ok) {
        console.error(`Cloudmersive API error: ${response.status} ${response.statusText}`);
        throw new Error(`Cloudmersive API error: ${response.statusText}`);
      }
      
      const textContent = await response.text();
      console.log("Cloudmersive API raw response length:", textContent.length);
      console.log("Cloudmersive API first 200 chars:", textContent.substring(0, 200));
      
      // Multiple slide extraction methods to ensure we get good results
      
      // Method 1: First try exact slide markers from Cloudmersive
      const slideRegex = /===== Slide (\d+) =====\n/g;
      let slides: string[] = [];
      let match;
      let matches = [...textContent.matchAll(slideRegex)];
      
      console.log(`Found ${matches.length} slide markers in response`);
      
      // If we have slide markers, extract content between them
      if (matches.length > 0) {
        for (let i = 0; i < matches.length; i++) {
          const currentMatch = matches[i];
          const currentIndex = currentMatch.index!;
          const nextMatch = matches[i + 1];
          const nextIndex = nextMatch ? nextMatch.index! : textContent.length;
          
          // Extract the slide content, skipping the marker itself
          const startPos = currentIndex + currentMatch[0].length;
          const slideContent = textContent.substring(startPos, nextIndex).trim();
          
          if (slideContent) {
            console.log(`Extracted slide ${i+1} with length: ${slideContent.length}`);
            slides.push(slideContent);
          }
        }
      }
      
      // Method 2: If that doesn't work, try alternative markers or formats
      if (slides.length === 0) {
        console.log("Trying alternative slide parsing methods");
        
        // Check for different slide separators
        const altSlideRegex = /Slide (\d+)[\s\n\r]+/g;
        matches = [...textContent.matchAll(altSlideRegex)];
        
        if (matches.length > 0) {
          console.log(`Found ${matches.length} alternative slide markers`);
          
          for (let i = 0; i < matches.length; i++) {
            const currentMatch = matches[i];
            const currentIndex = currentMatch.index!;
            const nextMatch = matches[i + 1];
            const nextIndex = nextMatch ? nextMatch.index! : textContent.length;
            
            // Extract the slide content, skipping the marker itself
            const startPos = currentIndex + currentMatch[0].length;
            const slideContent = textContent.substring(startPos, nextIndex).trim();
            
            if (slideContent) {
              console.log(`Extracted alt-slide ${i+1} with length: ${slideContent.length}`);
              slides.push(slideContent);
            }
          }
        }
      }
      
      // Method 3: Split by multiple newlines as a slide separator
      if (slides.length === 0) {
        console.log("Trying split by multiple newlines");
        
        // Clean and normalize the text first
        const cleanText = textContent
          .replace(/\r\n/g, '\n')  // Normalize line endings
          .replace(/\n{4,}/g, '\n\n\n'); // Normalize multiple newlines
        
        slides = cleanText
          .split(/\n{3,}/g)
          .filter(text => text.trim().length > 10); // Only consider substantial content
        
        console.log(`Split by newlines resulted in ${slides.length} potential slides`);
      }
      
      // Method 4: If we still have nothing, try using paragraph breaks
      if (slides.length === 0) {
        console.log("Trying paragraph detection");
        
        // Look for paragraphs that might be slide content
        const paragraphs = textContent
          .split(/\n\s*\n/)
          .filter(p => p.trim().length > 0);
          
        if (paragraphs.length > 0) {
          slides = paragraphs;
          console.log(`Found ${slides.length} paragraphs that could be slides`);
        }
      }
      
      // Method 5: Last resort - treat the whole content as a single slide
      if (slides.length === 0 && textContent.trim()) {
        console.log("Using whole content as a single slide");
        slides = [textContent.trim()];
      }
      
      // Post-process the slides to clean them up
      slides = slides.map(slide => {
        // Clean up any remaining slide markers
        return slide
          .replace(/===== Slide \d+ =====\n?/g, '')
          .replace(/^Slide \d+[\s\n\r]+/g, '')
          .trim();
      });
      
      console.log(`Final slide count after processing: ${slides.length}`);
      
      // Make sure we have at least one slide
      return slides.length > 0 ? slides : [textContent.trim()];
      
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
    console.log(`Extracted ${slideContents.length} slides from PowerPoint file`);
    
    // Log a sample of each slide for debugging
    slideContents.forEach((content, i) => {
      const preview = content.substring(0, 100).replace(/\n/g, '\\n');
      console.log(`Slide ${i+1} preview: ${preview}...`);
    });
    
    // Create slides based on extracted content
    slideContents.forEach((content, index) => {
      // Clean up the content
      const cleanContent = content.replace(/\\r\\n/g, '\n').replace(/\\n/g, '\n');
      
      // Generate a slide title from the first line or use default
      const lines = cleanContent.split('\n').filter(line => line.trim().length > 0);
      const title = lines.length > 0 ? lines[0].substring(0, 50) : `Slide ${index + 1}`;
      
      slides.push({
        id: `s${index + 1}`,
        number: index + 1,
        title: title,
        originalContent: cleanContent,
        suggestedUpdate: `Updated version of: ${cleanContent.substring(0, 100)}... \nThis would typically contain AI-generated updates based on recent medical research.`,
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
