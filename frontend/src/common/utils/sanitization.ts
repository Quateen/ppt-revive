import DOMPurify from 'dompurify';

/**
 * Sanitizes a string input to prevent XSS attacks
 * @param input The string to sanitize
 * @returns Sanitized string (all HTML tags removed)
 */
export const sanitizeInput = (input: string): string => {
  if (!input) return '';
  // Use USE_PROFILES to disable MathML, SVG, and enforce HTML namespace
  return DOMPurify.sanitize(input, { USE_PROFILES: { html: true }, ALLOWED_TAGS: [] });
};

/**
 * Sanitizes HTML content to allow a safe subset of tags and attributes
 * @param htmlInput The HTML string to sanitize
 * @returns Sanitized HTML string
 */
export const sanitizeHtml = (htmlInput: string): string => {
  if (!htmlInput) return '';
  return DOMPurify.sanitize(htmlInput, {
    USE_PROFILES: { html: true }, // Ensure we are in HTML context
    ALLOWED_TAGS: [
      // Text formatting
      'b', 'i', 'u', 'strong', 'em', 'strike', 'blockquote', 'p', 'br',
      // Lists
      'ul', 'ol', 'li',
      // Headings
      'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
      // Links
      'a',
      // Tables
      'table', 'thead', 'tbody', 'tr', 'th', 'td',
      // Semantic elements
      'div', 'span', 'pre', 'code',
      // Media
      'img',
      // Other formatting
      'hr', 'sup', 'sub',
    ],
    ALLOWED_ATTR: [
      'href', 'target', 'rel', 
      'src', 'alt', 'title', 
      'width', 'height', 
      'style', 'class'
    ],
    // Ensure links open in new tabs and are secure
    ADD_ATTR: ['target', 'rel'],
    // Force target="_blank" and rel="noopener noreferrer" on all <a> tags
    // Note: This might override existing target/rel attributes. Consider if this is desired.
    // Alternatively, use a hook to modify attributes conditionally.
    // FORBID_ATTR: [], // Keep default forbidden attributes
    // FORBID_TAGS: [], // Keep default forbidden tags
  });
};

/**
 * Sanitizes a number input to ensure it's a valid number
 * @param input The string or number to sanitize
 * @returns Sanitized number or undefined if invalid
 */
export const sanitizeNumber = (input: string | number | undefined): number | undefined => {
  if (!input) return undefined;
  const sanitized = DOMPurify.sanitize(input.toString(), { ALLOWED_TAGS: [] });
  const number = Number(sanitized.replace(/,/g, ''));
  return isNaN(number) ? undefined : number;
};

/**
 * Sanitizes an array of strings
 * @param input Array of strings to sanitize
 * @returns Sanitized array of strings
 */
export const sanitizeStringArray = (input: string[]): string[] => {
  if (!Array.isArray(input)) return [];
  return input.map(item => sanitizeInput(item)).filter(Boolean);
};
