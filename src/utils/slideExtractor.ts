
import { marked } from 'marked';

/**
 * Extracts slides from presentation content using various methods
 */
export function extractSlidesFromContent(content: string): string[] {
  console.log("Attempting to extract slides from content...");
  
  // First, check if content appears to be binary or has encoding issues
  if (hasBinaryContent(content)) {
    console.log("Content appears to be binary or have encoding issues. Creating a single slide.");
    return [sanitizeContent(content)];
  }
  
  const methods = [
    extractSlidesByRegex,
    extractSlidesByHeaders,
    extractSlidesByMultipleNewlines,
    extractSlidesByParagraphs
  ];
  
  for (const method of methods) {
    const slides = method(content);
    if (slides.length > 1) {
      console.log(`Successfully extracted ${slides.length} slides using ${method.name}`);
      return slides.map(sanitizeContent);
    }
  }
  
  // Fallback: Just return the whole content as one slide
  console.log("Falling back to treating entire content as one slide");
  return [sanitizeContent(content)];
}

/**
 * Checks if content appears to be binary or has encoding issues
 */
function hasBinaryContent(content: string): boolean {
  if (!content) return false;
  
  // Sample the first few characters to check for binary content
  const sampleSize = Math.min(content.length, 500);
  let nonPrintableCount = 0;
  
  for (let i = 0; i < sampleSize; i++) {
    const char = content.charCodeAt(i);
    // Count characters outside normal text ranges (excluding common whitespace)
    if ((char < 32 && char !== 9 && char !== 10 && char !== 13) || char > 126) {
      nonPrintableCount++;
    }
  }
  
  // If more than 20% of the sampled content is non-printable, consider it binary
  return (nonPrintableCount / sampleSize) > 0.2;
}

/**
 * Sanitizes content to remove or replace problematic characters
 */
function sanitizeContent(content: string): string {
  if (!content) return '';
  
  // Replace non-printable ASCII characters (except common whitespace)
  let sanitized = content.replace(/[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F]/g, '');
  
  // Replace common problematic Unicode characters
  sanitized = sanitized.replace(/[\uFFFD\uFFFE\uFFFF]/g, '');
  
  return sanitized;
}

/**
 * Extract slides using regex pattern matching
 */
function extractSlidesByRegex(content: string): string[] {
  console.log("Trying to extract slides using regex patterns...");
  
  // Common slide delimiter patterns from PowerPoint exports
  const patterns = [
    /\n\s*---\s*\n/g,                   // Markdown slide separator
    /\n\s*Slide \d+:?\s*\n/g,           // "Slide N:" pattern
    /\n\s*\[Slide \d+\]\s*\n/g,         // "[Slide N]" pattern
    /\n\s*#{1,6} Slide \d+\s*\n/g,      // "# Slide N" header pattern
    /\n\s*\*\*Slide \d+\*\*\s*\n/g,     // "**Slide N**" bold pattern
    /\n\s*==+ Slide \d+ ==+\s*\n/g,     // "=== Slide N ===" pattern
    /\n\s*slide:?\s*\d+\s*\n/i,         // "slide: N" or "slide N" pattern
    /\n\s*\d+\. Slide\s*\n/g,           // "1. Slide" numbered list pattern
    /\n\s*-{3,}\s*\n/g,                 // Three or more hyphens (common separator)
    /\n\s*\*{3,}\s*\n/g,                // Three or more asterisks (common separator)
    /\n\s*_{3,}\s*\n/g,                 // Three or more underscores (common separator)
    /\n\s*\={3,}\s*\n/g,                // Three or more equals signs (common separator)
  ];
  
  for (const pattern of patterns) {
    const parts = content.split(pattern);
    if (parts.length > 1) {
      console.log(`Found slide separator pattern: ${pattern}`);
      return parts.filter(slide => slide.trim().length > 0);
    }
  }
  
  return [];
}

/**
 * Extract slides by finding markdown headers that likely represent slide titles
 */
function extractSlidesByHeaders(content: string): string[] {
  console.log("Trying to extract slides using markdown headers...");
  
  // Look for patterns like "# Title" that might indicate slide titles
  const headerPattern = /\n(#{1,3} .+)\n/g;
  const headers = Array.from(content.matchAll(headerPattern));
  
  if (headers.length > 1) {
    console.log(`Found ${headers.length} potential slide headers`);
    
    const slides: string[] = [];
    let lastIndex = 0;
    
    headers.forEach((match, index) => {
      const startIndex = match.index || 0;
      const endIndex = index < headers.length - 1 ? headers[index + 1].index : content.length;
      
      if (index === 0 && startIndex > 0) {
        // Add content before the first header as a slide
        slides.push(content.substring(0, startIndex).trim());
      }
      
      slides.push(content.substring(startIndex, endIndex).trim());
      lastIndex = endIndex;
    });
    
    return slides;
  }
  
  return [];
}

/**
 * Extract slides by looking for multiple consecutive newlines (common slide break)
 */
function extractSlidesByMultipleNewlines(content: string): string[] {
  console.log("Trying to extract slides using multiple newlines...");
  
  // Try with various newline patterns
  const patterns = [
    /\n{4,}/g,  // 4+ newlines
    /\n{3,}/g,  // 3+ newlines
    /\n\n\n/g,  // Exactly 3 newlines
    /\n\n/g     // 2 newlines (less reliable, use as last resort)
  ];
  
  for (const pattern of patterns) {
    const parts = content.split(pattern);
    const nonEmptyParts = parts.filter(part => part.trim().length > 10); // Require some minimum content
    
    if (nonEmptyParts.length > 1) {
      console.log(`Found ${nonEmptyParts.length} slides using newline pattern: ${pattern}`);
      return nonEmptyParts;
    }
  }
  
  return [];
}

/**
 * Extract slides by analyzing paragraphs and looking for logical breaks
 */
function extractSlidesByParagraphs(content: string): string[] {
  console.log("Trying to extract slides using paragraph analysis...");
  
  // Parse the content with marked for better structure analysis
  const tokens = marked.lexer(content);
  const slides: string[] = [];
  let currentSlide = '';
  
  // Look for patterns that might indicate slide breaks
  tokens.forEach(token => {
    if (token.type === 'heading' && token.depth <= 2) {
      // Likely a new slide title
      if (currentSlide.trim().length > 0) {
        slides.push(currentSlide.trim());
        currentSlide = '';
      }
    }
    
    if (token.type === 'paragraph' || token.type === 'heading' || token.type === 'list') {
      currentSlide += token.raw;
    }
  });
  
  // Add the last slide
  if (currentSlide.trim().length > 0) {
    slides.push(currentSlide.trim());
  }
  
  if (slides.length > 1) {
    console.log(`Found ${slides.length} slides using paragraph analysis`);
    return slides;
  }
  
  return [];
}

/**
 * Clean and normalize the extracted slide content
 */
export function cleanSlideContent(content: string): string {
  if (!content) return '';
  
  let cleaned = content.trim();
  
  // Remove common patterns from exports like slide numbers
  cleaned = cleaned.replace(/^Slide \d+:?\s*/i, '');
  cleaned = cleaned.replace(/^\[Slide \d+\]\s*/i, '');
  cleaned = cleaned.replace(/^#{1,6} Slide \d+\s*/i, '');
  
  // Replace problematic characters
  cleaned = cleaned.replace(/[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F]/g, '');
  cleaned = cleaned.replace(/[\uFFFD\uFFFE\uFFFF]/g, '');
  
  return cleaned;
}
