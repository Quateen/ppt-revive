
import React from 'react';
import { Book, FileText, Home } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Button } from '@/components/ui/button';

const Header: React.FC = () => {
  return (
    <header className="bg-white border-b border-gray-200 sticky top-0 z-10">
      <div className="container mx-auto px-4 py-3 flex items-center justify-between">
        <Link to="/" className="flex items-center gap-2">
          <div className="bg-medical-600 p-1.5 rounded-md">
            <FileText className="w-5 h-5 text-white" />
          </div>
          <span className="font-semibold text-lg text-gray-800">MediPresent</span>
          <span className="text-medical-600 font-medium">Revive</span>
        </Link>
        
        <nav className="flex gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link to="/" className="flex items-center gap-1.5">
              <Home className="w-4 h-4" />
              <span>Home</span>
            </Link>
          </Button>
          
          <Button variant="ghost" size="sm" asChild>
            <Link to="/about" className="flex items-center gap-1.5">
              <Book className="w-4 h-4" />
              <span>About</span>
            </Link>
          </Button>
        </nav>
      </div>
    </header>
  );
};

export default Header;
