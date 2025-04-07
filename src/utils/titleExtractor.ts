
/**
 * Extracts a title from slide content
 */
export function extractTitleFromSlide(content: string): string {
  // Check if content contains binary or encoding issues
  if (containsEncodingIssues(content)) {
    return "Untitled Slide";
  }

  // Look for potential title patterns in decreasing order of confidence
  const titlePatterns = [
    // Headers (Markdown style)
    /^#{1,3} ([^\n]+)/m,
    
    // Bold text at start
    /^\*\*([^*\n]+)\*\*/m,
    
    // Underlined text at start
    /^__([^_\n]+)__/m,
    
    // All caps text at start (potential title)
    /^([A-Z][A-Z\s]{5,})/m,
    
    // First line if it's short (likely a title)
    /^([^\n]{5,60})(?:\n|$)/m,
  ];

  for (const pattern of titlePatterns) {
    const match = content.match(pattern);
    if (match && match[1]) {
      // Clean up the title
      let title = match[1].trim();
      
      // Remove markdown formatting from title
      title = title.replace(/[#*_]/g, '').trim();
      
      // Check if title contains unusual characters indicating encoding issues
      if (containsEncodingIssues(title)) {
        continue; // Skip this title and try next pattern
      }
      
      // Limit title length
      if (title.length > 80) {
        title = title.substring(0, 77) + '...';
      }
      
      return title;
    }
  }

  // Default to first few words if no clear title is found
  const firstLine = content.split('\n')[0] || '';
  
  // Check if first line has encoding issues
  if (containsEncodingIssues(firstLine)) {
    return "Untitled Slide";
  }
  
  const words = firstLine.split(' ').slice(0, 5).join(' ').trim();
  
  if (words && words.length > 3 && !containsEncodingIssues(words)) {
    return words + (words.length < firstLine.length ? '...' : '');
  }
  
  return 'Untitled Slide';
}

/**
 * Checks if the text contains encoding issues or binary data
 */
function containsEncodingIssues(text: string): boolean {
  if (!text) return false;
  
  // Check for high ratio of special characters or control chars
  let specialCharCount = 0;
  let controlCharCount = 0;
  
  for (let i = 0; i < text.length; i++) {
    const charCode = text.charCodeAt(i);
    
    // Check for control characters (except normal whitespace)
    if ((charCode < 32 && charCode !== 9 && charCode !== 10 && charCode !== 13) || 
        (charCode >= 127 && charCode <= 159)) {
      controlCharCount++;
    }
    
    // Check for unusual Unicode characters or potential encoding issues
    // Non-standard punctuation and symbols
    if ((charCode >= 0x2000 && charCode <= 0x206F) || // General punctuation
        (charCode >= 0xFFF0 && charCode <= 0xFFFF) || // Specials
        (charCode >= 0xFE00 && charCode <= 0xFE0F) || // Variation selectors
        (charCode >= 0xD800 && charCode <= 0xDFFF)) { // Surrogate pairs
      specialCharCount++;
    }
    
    // Check for unusual symbol frequency
    if ("�����������".includes(text[i])) {
      specialCharCount += 2; // Give higher weight to known problematic symbols
    }
  }
  
  // If more than 15% are control or special chars, likely corrupt/binary content
  return (controlCharCount + specialCharCount) / text.length > 0.15;
}
