import React from 'react';
import { ShieldCheck } from 'lucide-react';

interface MedicalDisclaimerProps {
  /**
   * `inline` renders a single calm line suitable for the footer.
   * `banner` renders a prominent attestation card for the review page.
   */
  variant?: 'inline' | 'banner';
  className?: string;
}

/**
 * Medical disclaimer + human-review attestation in the Nucleus Digitalis voice:
 * calm, physician-led, non-alarmist. PPT-Revive *proposes* evidence-linked
 * updates; the physician remains the final reviewer, responsible for verifying
 * every change against its cited source and their own clinical judgment before use.
 */
const MedicalDisclaimer: React.FC<MedicalDisclaimerProps> = ({
  variant = 'inline',
  className = '',
}) => {
  if (variant === 'banner') {
    return (
      <div
        className={`rounded-xl border border-border bg-secondary/40 p-4 sm:p-5 flex items-start gap-3 ${className}`}
      >
        <div className="nucleus-gradient rounded-lg p-2 shrink-0">
          <ShieldCheck className="h-5 w-5 text-white" />
        </div>
        <div className="text-sm text-foreground/80">
          <p className="font-medium text-foreground">
            You are the physician of record — approve only what you&apos;ve verified.
          </p>
          <p className="mt-1">
            PPT-Revive proposes evidence-linked updates from the current literature. It does
            not replace your clinical judgment. Before using any slide, confirm each proposed
            change against its cited source and your own assessment. Final responsibility for
            the content rests with you.
          </p>
        </div>
      </div>
    );
  }

  return (
    <p className={`text-xs text-muted-foreground ${className}`}>
      Medical disclaimer: PPT-Revive proposes evidence-linked updates for your review. The
      physician remains the final reviewer, responsible for verifying every change against its
      cited source and their own clinical judgment before use. It is not medical advice.
    </p>
  );
};

export default MedicalDisclaimer;
