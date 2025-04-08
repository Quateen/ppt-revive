
import React from 'react';
import { Link } from 'react-router-dom';
import BrainLogo from './BrainLogo';
import { Button } from '@/components/ui/button';
import { Home, Info } from 'lucide-react';

const Header = () => {
  return (
    <header className="app-header border-b border-gray-800 bg-black">
      <div className="container mx-auto px-4 py-4">
        <div className="flex justify-between items-center">
          <div className="flex items-center space-x-6">
            <Link to="/" className="flex items-center">
              <h1 className="text-2xl font-bold text-white">NeuroSlide<span className="text-neuro-purple">AI</span></h1>
            </Link>
            
            <nav className="hidden md:flex space-x-4">
              <Button variant="ghost" asChild className="text-white hover:text-primary hover:bg-gray-900">
                <Link to="/" className="flex items-center">
                  <Home className="mr-2 h-4 w-4" />
                  Home
                </Link>
              </Button>
              <Button variant="ghost" asChild className="text-white hover:text-primary hover:bg-gray-900">
                <Link to="/about" className="flex items-center">
                  <Info className="mr-2 h-4 w-4" />
                  About
                </Link>
              </Button>
            </nav>
          </div>
          
          <BrainLogo />
        </div>
      </div>
    </header>
  );
};

export default Header;
