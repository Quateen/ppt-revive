
import React from 'react';
import { Link } from 'react-router-dom';

const Footer: React.FC = () => {
  return (
    <footer className="bg-white border-t border-gray-200 mt-auto">
      <div className="container mx-auto px-4 py-6">
        <div className="flex flex-col md:flex-row justify-between items-center">
          <div className="mb-4 md:mb-0">
            <div className="flex items-center gap-2">
              <span className="font-semibold text-gray-800">MediPresent</span>
              <span className="text-medical-600">Revive</span>
            </div>
            <p className="text-sm text-gray-600 mt-1">
              Updating medical presentations with the latest research
            </p>
          </div>
          
          <div className="flex gap-6">
            <Link to="/" className="text-sm text-gray-600 hover:text-medical-600">
              Home
            </Link>
            <Link to="/about" className="text-sm text-gray-600 hover:text-medical-600">
              About
            </Link>
            <Link to="/privacy" className="text-sm text-gray-600 hover:text-medical-600">
              Privacy
            </Link>
            <Link to="/terms" className="text-sm text-gray-600 hover:text-medical-600">
              Terms
            </Link>
          </div>
        </div>
        
        <div className="mt-6 border-t border-gray-100 pt-4 text-center text-sm text-gray-500">
          © {new Date().getFullYear()} MediPresent Revive. All rights reserved.
        </div>
      </div>
    </footer>
  );
};

export default Footer;
