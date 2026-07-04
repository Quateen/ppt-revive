import React from 'react';
import { NucleusMark } from '@/components/BrandLogo';
import { Button } from '@/components/ui/button';

interface ErrorBoundaryProps {
  children: React.ReactNode;
}

interface ErrorBoundaryState {
  hasError: boolean;
}

/**
 * App-level error boundary. Without it, a single render/runtime error anywhere
 * in the tree white-screens the whole app. This catches it and shows a calm,
 * branded fallback (Nucleus mark + a refresh button) in the ND voice.
 */
class ErrorBoundary extends React.Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(): ErrorBoundaryState {
    return { hasError: true };
  }

  componentDidCatch(error: Error, info: React.ErrorInfo) {
    // Keep a trace for support/debugging without exposing details to the user.
    console.error('Unhandled error caught by ErrorBoundary:', error, info);
  }

  handleRefresh = () => {
    window.location.reload();
  };

  render() {
    if (this.state.hasError) {
      return (
        <main
          role="alert"
          className="min-h-screen flex items-center justify-center bg-background px-4 py-16"
        >
          <div className="max-w-md w-full text-center rounded-2xl border border-border bg-card p-8">
            <div className="nucleus-gradient rounded-2xl p-4 inline-flex mb-6">
              <NucleusMark className="h-10 w-10" />
            </div>
            <h1 className="font-display text-2xl font-semibold text-foreground mb-3">
              Something went wrong on our end
            </h1>
            <p className="text-muted-foreground mb-6">
              A hiccup interrupted the page. Your work isn&apos;t lost anywhere it was saved —
              please refresh to pick back up. If it keeps happening, try again in a moment.
            </p>
            <Button size="lg" onClick={this.handleRefresh} className="font-medium">
              Refresh the page
            </Button>
          </div>
        </main>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
