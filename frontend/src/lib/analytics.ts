/**
 * Privacy-friendly, cookieless analytics (Plausible), enabled only when
 * VITE_PLAUSIBLE_DOMAIN is set at build time. A lead-gen asset must measure
 * traffic and conversions, but we don't ship a tracker (or a CSP hole) by default.
 *
 * To turn on: set VITE_PLAUSIBLE_DOMAIN=revive.nucleusdigitalis.com (and, if
 * self-hosting, VITE_PLAUSIBLE_SRC). Track a conversion anywhere with:
 *   trackEvent('Join Founding Members')
 */
export function initAnalytics(): void {
  const domain = import.meta.env.VITE_PLAUSIBLE_DOMAIN as string | undefined;
  if (!domain || typeof document === 'undefined') return;

  const src =
    (import.meta.env.VITE_PLAUSIBLE_SRC as string | undefined) ||
    'https://plausible.io/js/script.tagged-events.js';

  const script = document.createElement('script');
  script.defer = true;
  script.setAttribute('data-domain', domain);
  script.src = src;
  document.head.appendChild(script);
}

export function trackEvent(name: string, props?: Record<string, string | number | boolean>): void {
  const w = window as unknown as { plausible?: (n: string, o?: { props: Record<string, unknown> }) => void };
  if (typeof w.plausible === 'function') {
    w.plausible(name, props ? { props } : undefined);
  }
}
