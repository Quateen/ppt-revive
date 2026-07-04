import React from 'react';

/**
 * The Nucleus Digitalis mark: a rounded square with a teal→blue gradient and two
 * concentric rings (the "nucleus" motif), rendered as inline SVG so it needs no asset.
 */
export const NucleusMark: React.FC<{ className?: string; title?: string }> = ({
  className = 'h-8 w-8',
  title = 'Nucleus Digitalis',
}) => (
  <svg
    className={className}
    viewBox="0 0 64 64"
    role="img"
    aria-label={title}
    xmlns="http://www.w3.org/2000/svg"
  >
    <defs>
      <linearGradient id="nucleus-mark-gradient" x1="0" y1="0" x2="1" y2="1">
        <stop offset="0" stopColor="#01696f" />
        <stop offset="1" stopColor="#006494" />
      </linearGradient>
    </defs>
    <rect width="64" height="64" rx="16" fill="url(#nucleus-mark-gradient)" />
    <circle cx="32" cy="32" r="16" fill="none" stroke="white" strokeWidth="3" opacity="0.9" />
    <circle cx="32" cy="32" r="7" fill="white" />
  </svg>
);

/**
 * Full brand lockup: the mark + "PPT-Revive" wordmark, with a small
 * "by Nucleus Digitalis" endorsement line (the product is the wedge; the
 * company is the brand it funnels into).
 */
const BrandLogo: React.FC<{ withByline?: boolean; className?: string }> = ({
  withByline = true,
  className = '',
}) => (
  <span className={`flex items-center gap-2.5 ${className}`}>
    <NucleusMark className="h-9 w-9 shrink-0" />
    <span className="flex flex-col leading-none">
      <span className="font-display text-lg font-semibold text-foreground">PPT-Revive</span>
      {withByline && (
        <span className="text-[11px] font-medium tracking-wide text-muted-foreground">
          by Nucleus Digitalis
        </span>
      )}
    </span>
  </span>
);

export default BrandLogo;
