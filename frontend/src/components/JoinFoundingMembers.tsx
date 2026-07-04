import React, { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { ArrowRight, Check } from 'lucide-react';
import { AppConfig } from '@/config';
import apiClient from '@/api/apiClient';

interface JoinFoundingMembersProps {
  /** Where this CTA appears, so we can attribute the lead source. */
  source?: string;
  /** Compact variant for inline placement (e.g. after download). */
  variant?: 'band' | 'inline';
  heading?: string;
  subheading?: string;
}

/**
 * Primary conversion surface: capture the physician's email (the lead) and send them
 * to the Nucleus Digitalis founding-members page. The email POST is best-effort — if the
 * leads endpoint is unavailable we still advance the prospect to the founding page.
 */
const JoinFoundingMembers: React.FC<JoinFoundingMembersProps> = ({
  source = 'landing',
  variant = 'band',
  heading = 'Become an AI-Enhanced Physician',
  subheading = 'PPT-Revive is one tool from the Nucleus Digitalis program. Join the founding wave of clinicians putting AI to work — with human review at every step. Early-access pricing won’t stay open indefinitely.',
}) => {
  const [email, setEmail] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [done, setDone] = useState(false);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email || submitting) return;
    setSubmitting(true);
    try {
      await apiClient.post('/api/lead', { email, source });
    } catch {
      // Best-effort capture; still convert the prospect to the founding page.
    } finally {
      setSubmitting(false);
      setDone(true);
      window.open(AppConfig.FOUNDING_URL, '_blank', 'noopener,noreferrer');
    }
  };

  const isBand = variant === 'band';

  return (
    <div className={isBand ? 'rounded-3xl nucleus-gradient p-8 md:p-12 text-white text-center' : 'rounded-2xl border border-border bg-secondary/40 p-6'}>
      <h3 className={isBand ? 'text-3xl font-display font-semibold mb-3' : 'text-xl font-display font-semibold mb-2 text-foreground'}>
        {heading}
      </h3>
      <p className={isBand ? 'text-white/85 max-w-xl mx-auto mb-6' : 'text-muted-foreground text-sm mb-4'}>
        {subheading}
      </p>

      {done ? (
        <div className={`flex items-center justify-center gap-2 font-medium ${isBand ? 'text-white' : 'text-primary'}`}>
          <Check className="h-5 w-5" />
          <span>You&rsquo;re on the list — we opened the founding page in a new tab.</span>
        </div>
      ) : (
        <form onSubmit={submit} className="flex flex-col sm:flex-row gap-3 max-w-md mx-auto">
          <Input
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="you@hospital.org"
            aria-label="Email address"
            className={isBand ? 'bg-white/95 text-foreground border-0 rounded-full h-11' : 'rounded-full h-11'}
          />
          <Button
            type="submit"
            size="lg"
            variant={isBand ? 'secondary' : 'default'}
            disabled={submitting}
            className="font-medium shrink-0"
          >
            {submitting ? 'Joining…' : 'Join Founding Members'}
            <ArrowRight className="h-4 w-4" />
          </Button>
        </form>
      )}
    </div>
  );
};

export default JoinFoundingMembers;
