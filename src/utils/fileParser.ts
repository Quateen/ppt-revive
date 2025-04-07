
import { Presentation, Slide } from '@/types/presentation';
import * as XLSX from 'xlsx';
import { marked } from 'marked';

// A better PowerPoint parser that combines multiple approaches
export const extractTextFromPowerPoint = async (file: File): Promise<string[]> => {
  try {
    // Check if file is a PowerPoint file
    if (!file.name.match(/\.(ppt|pptx|pot|potx|pps|ppsx|pptm|potm|ppsm)$/i)) {
      throw new Error("File is not a PowerPoint file");
    }
    
    console.log("Starting PowerPoint extraction for:", file.name);
    
    // First attempt: Use Cloudmersive API
    try {
      const formData = new FormData();
      formData.append("inputFile", file);
      
      console.log("Sending file to Cloudmersive API");
      const response = await fetch("https://api.cloudmersive.com/convert/pptx/to/txt", {
        method: "POST",
        headers: {
          "Apikey": "af6862af-3416-4e3a-989c-f3531f34bbb5"
        },
        body: formData,
        // Add timeout to prevent long waits
        signal: AbortSignal.timeout(30000) // 30 sec timeout
      });
      
      if (!response.ok) {
        console.warn(`Cloudmersive API error: ${response.status} ${response.statusText}`);
        throw new Error(`Cloudmersive API error: ${response.statusText}`);
      }
      
      const textContent = await response.text();
      console.log(`Received ${textContent.length} characters from API`);
      
      if (textContent.length < 10) {
        console.warn("Cloudmersive API returned insufficient content");
        throw new Error("Insufficient content from API");
      }
      
      // Process the content to extract slides
      const slides = parseCloudmersiveContent(textContent);
      
      if (slides.length > 0) {
        console.log(`Successfully extracted ${slides.length} slides from Cloudmersive API`);
        return slides;
      } else {
        console.warn("Could not identify slides in Cloudmersive response");
        throw new Error("No slides identified in API response");
      }
    } catch (apiError) {
      console.warn("Cloudmersive API failed:", apiError);
      console.log("Falling back to XLSX parser...");
      return await extractTextWithXLSX(file);
    }
  } catch (error) {
    console.error("Error in PowerPoint extraction:", error);
    return ["Error extracting content from the presentation file. Please try another file."];
  }
};

// Process the content from Cloudmersive API to extract slides
const parseCloudmersiveContent = (content: string): string[] => {
  console.log("Parsing Cloudmersive content");
  let slides: string[] = [];
  
  // Method 1: Exact slide markers
  const slideRegex = /===+\s*Slide\s+(\d+)\s*===+/gi;
  const slideMatches = [...content.matchAll(slideRegex)];
  
  if (slideMatches.length > 0) {
    console.log(`Found ${slideMatches.length} slide markers`);
    
    // Extract content between markers
    for (let i = 0; i < slideMatches.length; i++) {
      const currentMatch = slideMatches[i];
      const currentIndex = currentMatch.index!;
      const nextMatch = slideMatches[i + 1];
      const nextIndex = nextMatch ? nextMatch.index! : content.length;
      
      // Skip the marker itself
      const markerLength = currentMatch[0].length;
      const slideContent = content.substring(currentIndex + markerLength, nextIndex).trim();
      
      if (slideContent.length > 0) {
        slides.push(cleanSlideContent(slideContent));
      }
    }
    
    return slides;
  }
  
  // Method 2: Alternative slide markers
  const altRegex = /Slide\s+(\d+)[\s\n\r]*/gi;
  const altMatches = [...content.matchAll(altRegex)];
  
  if (altMatches.length > 0) {
    console.log(`Found ${altMatches.length} alternative slide markers`);
    
    for (let i = 0; i < altMatches.length; i++) {
      const currentMatch = altMatches[i];
      const currentIndex = currentMatch.index!;
      const nextMatch = altMatches[i + 1];
      const nextIndex = nextMatch ? nextMatch.index! : content.length;
      
      // Skip the marker
      const markerLength = currentMatch[0].length;
      const slideContent = content.substring(currentIndex + markerLength, nextIndex).trim();
      
      if (slideContent.length > 0) {
        slides.push(cleanSlideContent(slideContent));
      }
    }
    
    return slides;
  }
  
  // Method 3: Split by headers and titles (common in presentations)
  const headerRegex = /^(#+)\s+(.+?)$/gm;
  const headerMatches = [...content.matchAll(headerRegex)];
  
  if (headerMatches.length > 0) {
    console.log(`Found ${headerMatches.length} headers that might be slides`);
    
    // Split by headers
    for (let i = 0; i < headerMatches.length; i++) {
      const currentMatch = headerMatches[i];
      const currentIndex = currentMatch.index!;
      const nextMatch = headerMatches[i + 1];
      const nextIndex = nextMatch ? nextMatch.index! : content.length;
      
      // Include the header in the content
      const slideContent = content.substring(currentIndex, nextIndex).trim();
      
      if (slideContent.length > 0) {
        slides.push(cleanSlideContent(slideContent));
      }
    }
    
    return slides;
  }
  
  // Method 4: Split by multiple newlines (typical slide separator)
  const cleanContent = content
    .replace(/\r\n/g, '\n')
    .replace(/\n{4,}/g, '\n\n\n');
  
  slides = cleanContent
    .split(/\n{3,}/)
    .filter(text => text.trim().length > 15) // Only substantial content
    .map(cleanSlideContent);
    
  if (slides.length > 0) {
    console.log(`Found ${slides.length} slides by splitting on newlines`);
    return slides;
  }
  
  // Method 5: Split by paragraphs
  slides = content
    .split(/\n\s*\n/)
    .filter(p => p.trim().length > 20)
    .map(cleanSlideContent);
    
  if (slides.length > 0) {
    console.log(`Found ${slides.length} slides by paragraphs`);
    return slides;
  }
  
  // Last resort: use the whole content as one slide
  if (content.trim().length > 0) {
    console.log("Using entire content as a single slide");
    return [cleanSlideContent(content)];
  }
  
  return [];
};

// Clean up slide content
const cleanSlideContent = (content: string): string => {
  return content
    .replace(/===+\s*Slide\s+\d+\s*===+/gi, '') // Remove slide markers
    .replace(/^Slide\s+\d+[\s\n\r]*/gi, '')     // Remove slide numbers
    .replace(/\s{2,}/g, ' ')                    // Normalize whitespace
    .trim();
};

// XLSX fallback parser
const extractTextWithXLSX = async (file: File): Promise<string[]> => {
  try {
    console.log("Using XLSX fallback parser");
    const buffer = await file.arrayBuffer();
    
    // Use XLSX to parse the PowerPoint
    const workbook = XLSX.read(buffer, { type: 'array', bookVBA: true });
    
    // Each sheet might be a slide
    let slideContents: string[] = [];
    
    workbook.SheetNames.forEach((sheetName, index) => {
      try {
        const worksheet = workbook.Sheets[sheetName];
        
        if (!worksheet) {
          console.warn(`Empty worksheet for sheet ${sheetName}`);
          return;
        }
        
        // Try to extract text content
        let textContent = "";
        
        try {
          textContent = XLSX.utils.sheet_to_txt(worksheet);
        } catch (err) {
          console.warn(`Could not extract text from sheet ${sheetName}:`, err);
          
          // Alternative extraction if sheet_to_txt fails
          const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: 1 });
          textContent = jsonData
            .map((row: any[]) => row.filter(Boolean).join(" "))
            .filter(Boolean)
            .join("\n");
        }
        
        if (textContent.trim()) {
          slideContents.push(cleanSlideContent(textContent));
        }
      } catch (sheetError) {
        console.warn(`Error processing sheet ${sheetName}:`, sheetError);
      }
    });
    
    // Try to parse other worksheets if present
    try {
      if ("_rawSheets" in workbook && workbook._rawSheets) {
        Object.keys(workbook._rawSheets).forEach(key => {
          if (key.includes("SlideText") || key.includes("Notes")) {
            try {
              const rawData = workbook._rawSheets[key];
              const text = rawData.toString('utf8').replace(/[^\x20-\x7E\n\r\t]/g, "");
              if (text.trim()) {
                slideContents.push(cleanSlideContent(text));
              }
            } catch (err) {
              console.warn(`Error extracting text from raw sheet ${key}:`, err);
            }
          }
        });
      }
    } catch (rawError) {
      console.warn("Error accessing raw sheets:", rawError);
    }
    
    // If we couldn't extract slides, return a default message
    if (slideContents.length === 0) {
      console.warn("Could not extract text from PowerPoint file with XLSX");
      return ["No text content could be extracted from this PowerPoint file. Try a different format or file."];
    }
    
    console.log(`XLSX parser extracted ${slideContents.length} potential slides`);
    return slideContents;
  } catch (error) {
    console.error("XLSX parser error:", error);
    return ["Error parsing PowerPoint file. Please try a different file format."];
  }
};

// Generate slides from extracted text content
export const generateSlidesFromFile = async (file: File): Promise<Slide[]> => {
  console.log("Generating slides from file:", file.name);
  const slides: Slide[] = [];
  
  try {
    // Extract text content from the PowerPoint file
    const slideContents = await extractTextFromPowerPoint(file);
    console.log(`Extracted ${slideContents.length} slides from file`);
    
    // Log a preview of each slide content
    slideContents.forEach((content, i) => {
      const contentPreview = content.substring(0, 50).replace(/\n/g, '\\n');
      console.log(`Slide ${i+1} preview: ${contentPreview}...`);
    });
    
    // Process each slide content to create structured slides
    slideContents.forEach((content, index) => {
      // Get the first line as title, or first sentence if no line breaks
      let title = "";
      let body = content;
      
      if (content.includes('\n')) {
        const lines = content.split('\n').filter(line => line.trim());
        if (lines.length > 0) {
          title = lines[0].trim();
          body = lines.slice(1).join('\n').trim();
        }
      } else if (content.match(/[.!?]/)) {
        // Use the first sentence as title
        const match = content.match(/^(.+?[.!?])\s/);
        if (match) {
          title = match[1].trim();
          body = content.substring(match[0].length).trim();
        }
      }
      
      // If we still don't have a title, use a generic one
      if (!title) {
        title = `Slide ${index + 1}`;
      }
      
      // Create properly formatted slide
      slides.push({
        id: `s${index + 1}`,
        number: index + 1,
        title: title.substring(0, 100),  // Limit title length
        originalContent: content,
        suggestedUpdate: `Updated version of the slide content would go here, based on latest medical research relevant to: ${title}`,
        updateReason: "This would be updated based on recent medical research findings.",
        sourceCitations: [
          "Medical Journal Reference (2023)",
          "Clinical Guidelines (2024)"
        ],
        status: 'pending'
      });
    });
    
    // If no slides were created, add a default error slide
    if (slides.length === 0) {
      slides.push({
        id: 's1',
        number: 1,
        title: 'Could Not Parse Presentation',
        originalContent: 'The presentation could not be parsed correctly. This could be due to the file format or content structure.',
        suggestedUpdate: 'Please try uploading a different PowerPoint file or format.',
        updateReason: "Error in file parsing",
        sourceCitations: [],
        status: 'pending'
      });
    }
    
    return slides;
  } catch (error) {
    console.error("Error generating slides:", error);
    
    // Return a default error slide
    return [{
      id: 's1',
      number: 1,
      title: 'Error Processing File',
      originalContent: 'An error occurred while processing the presentation file.',
      suggestedUpdate: 'Please try uploading the file again or use a different file format.',
      updateReason: "File processing error",
      sourceCitations: [],
      status: 'pending'
    }];
  }
};

// Create a full presentation object from the file
export const createPresentationFromFile = async (file: File): Promise<Presentation> => {
  console.log("Creating presentation from file:", file.name);
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
