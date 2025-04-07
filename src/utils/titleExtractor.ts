
/**
 * Extracts a title from slide content
 */
export function extractTitleFromSlide(content: string): string {
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
      
      // Limit title length
      if (title.length > 80) {
        title = title.substring(0, 77) + '...';
      }
      
      return title;
    }
  }

  // Default to first few words if no clear title is found
  const firstLine = content.split('\n')[0] || '';
  const words = firstLine.split(' ').slice(0, 5).join(' ').trim();
  
  if (words && words.length > 3) {
    return words + (words.length < firstLine.length ? '...' : '');
  }
  
  return 'Untitled Slide';
}
