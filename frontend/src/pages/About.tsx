import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { FileText, Search, RotateCcw, FileCheck, ShieldCheck } from 'lucide-react';
import { Link } from 'react-router-dom';

const About = () => {
  return (
    <main className="container mx-auto px-4 py-12 flex-1">
      <div className="max-w-3xl mx-auto">
        <p className="text-primary font-medium mb-2">Physician-led. AI-enabled. Human-reviewed.</p>
        <h1 className="text-4xl font-display font-semibold text-foreground mb-6">About PPT-Revive</h1>

        <div className="max-w-none">
          <p className="text-lg text-foreground/80 mb-6">
            Your slides are your credibility. But a lecture built in 2019 can carry guidance that has
            since changed. <span className="font-medium text-foreground">PPT-Revive</span> reads a
            presentation slide by slide, finds what current, peer-reviewed evidence says, and proposes
            a precise, cited update — while you stay in control of every change.
          </p>

          <p className="text-foreground/80 mb-8">
            It's a free tool from <span className="font-medium">Nucleus Digitalis</span>, the
            physician-led program that helps clinicians put AI to work in their actual practice.
            Most clinicians don't need more AI hype. They need a clear, trustworthy path — this is one
            concrete step on it.
          </p>

          <h2 className="text-2xl font-display font-semibold text-foreground mt-8 mb-4">How it works</h2>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-10">
            {[
              { icon: FileText, title: 'Upload your deck', body: 'Drop in a .pptx lecture. The free tier revives up to 5 slides so you can see the quality before you commit.' },
              { icon: Search, title: 'Evidence, not guesswork', body: 'Each slide is checked against current literature retrieved from PubMed — real papers with real citations, not invented references.' },
              { icon: RotateCcw, title: 'You approve every change', body: 'Review old vs. proposed side by side. Approve, edit, or reject each slide. Nothing changes without your sign-off.' },
              { icon: FileCheck, title: 'Download, cited', body: 'Export the revived deck with a references slide — every update traceable to its source, footered NucleusDigitalis.com.' },
            ].map(({ icon: Icon, title, body }) => (
              <Card key={title} className="border-border">
                <CardContent className="pt-6">
                  <div className="flex items-center gap-3 mb-3">
                    <div className="bg-secondary p-2 rounded-lg">
                      <Icon className="h-5 w-5 text-primary" />
                    </div>
                    <h3 className="font-medium text-lg">{title}</h3>
                  </div>
                  <p className="text-muted-foreground text-sm">{body}</p>
                </CardContent>
              </Card>
            ))}
          </div>

          <div className="rounded-2xl border border-border bg-secondary/40 p-6 mb-10">
            <div className="flex items-start gap-3">
              <ShieldCheck className="h-6 w-6 text-primary shrink-0 mt-0.5" />
              <div>
                <h3 className="font-medium text-lg mb-1">Human review is the point</h3>
                <p className="text-muted-foreground text-sm">
                  PPT-Revive suggests; a clinician decides. Every proposed change is evidence-linked and
                  requires your approval. Treat it as a well-read research assistant that shows its work —
                  not an autopilot. Always confirm clinical claims against the cited source and your own
                  judgment before use.
                </p>
              </div>
            </div>
          </div>

          <h2 className="text-2xl font-display font-semibold text-foreground mt-8 mb-4">Built by a physician</h2>
          <p className="text-foreground/80 mb-8">
            Nucleus Digitalis was founded by <span className="font-medium">Ahmed Quateen, MD</span> —
            neurosurgeon and spine surgeon, adjunct professor at UAE University. PPT-Revive exists because
            keeping teaching current shouldn't cost a weekend, and because the fastest way to trust a tool
            is to see it show its evidence.
          </p>

          <div className="rounded-2xl nucleus-gradient p-8 text-center text-white">
            <h3 className="text-2xl font-display font-semibold mb-2">Become an AI-Enhanced Physician</h3>
            <p className="text-white/85 mb-5 max-w-xl mx-auto">
              PPT-Revive is one tool from the Nucleus Digitalis program — a clear path to putting AI to work
              across your practice, with human review at every step.
            </p>
            <Button asChild size="lg" variant="secondary" className="rounded-full font-medium">
              <Link to="/">Try PPT-Revive free</Link>
            </Button>
          </div>
        </div>
      </div>
    </main>
  );
};

export default About;
