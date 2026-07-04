import React from 'react';
import { Link } from 'react-router-dom';
import BrandLogo from '@/components/BrandLogo';
import MedicalDisclaimer from '@/components/MedicalDisclaimer';

const Footer: React.FC = () => {
  return (
    <footer className="bg-card border-t border-border mt-auto">
      <div className="container mx-auto px-4 py-8">
        <div className="flex flex-col md:flex-row justify-between items-start gap-6">
          <div className="max-w-sm">
            <BrandLogo />
            <p className="text-sm text-muted-foreground mt-3">
              Bring your medical slides up to current, cited evidence — physician-led,
              AI-enabled, human-reviewed.
            </p>
          </div>

          <div className="flex gap-8">
            <Link to="/" className="text-sm text-muted-foreground hover:text-primary">
              Home
            </Link>
            <Link to="/about" className="text-sm text-muted-foreground hover:text-primary">
              About
            </Link>
            <a
              href="https://nucleusdigitalis.com"
              target="_blank"
              rel="noopener noreferrer"
              className="text-sm text-muted-foreground hover:text-primary"
            >
              NucleusDigitalis.com
            </a>
          </div>
        </div>

        <div className="mt-8 border-t border-border pt-4">
          <MedicalDisclaimer variant="inline" className="max-w-3xl" />
        </div>

        <div className="mt-4 flex flex-col sm:flex-row items-center justify-between gap-2 text-sm text-muted-foreground">
          <span>© {new Date().getFullYear()} Nucleus Digitalis. All rights reserved.</span>
          <span>
            A free tool from the{' '}
            <a
              href="https://nucleusdigitalis.com"
              target="_blank"
              rel="noopener noreferrer"
              className="text-primary hover:underline"
            >
              AI-Enhanced Physician
            </a>{' '}
            program.
          </span>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
