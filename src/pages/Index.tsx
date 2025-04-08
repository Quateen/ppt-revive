
import React from 'react';
import { useNavigate } from 'react-router-dom';
import Header from '@/components/Header';
import Footer from '@/components/Footer';
import { Button } from '@/components/ui/button';
import { FileText, Search, RefreshCw, Download } from 'lucide-react';

const Index = () => {
  const navigate = useNavigate();

  return (
    <div className="flex flex-col min-h-screen bg-white">
      <Header />
      
      <main className="flex-1">
        {/* Hero Section */}
        <section className="bg-medical-gradient py-16 md:py-32 text-white text-center">
          <div className="container mx-auto px-4">
            <h1 className="text-3xl md:text-5xl font-bold mb-6">
              Keep Medical Presentations Up-to-Date with the Latest Research
            </h1>
            <p className="text-xl md:text-2xl max-w-3xl mx-auto mb-10">
              MediPresent Revive automatically analyzes your existing slides and suggests updates
              based on recent medical literature.
            </p>
            <Button 
              size="lg"
              variant="outline" 
              className="bg-white text-primary hover:bg-gray-100 text-lg"
              onClick={() => navigate('/analyzer')}
            >
              Get Started
            </Button>
          </div>
        </section>
        
        {/* How It Works Section */}
        <section className="py-16 bg-white">
          <div className="container mx-auto px-4">
            <h2 className="text-3xl font-bold text-center mb-16">How It Works</h2>
            
            <div className="grid md:grid-cols-4 gap-8 text-center">
              <div className="flex flex-col items-center">
                <div className="text-blue-600 mb-6">
                  <FileText size={48} />
                </div>
                <h3 className="text-xl font-semibold mb-2">Upload Presentation</h3>
                <p className="text-gray-600">Upload your PowerPoint presentation for analysis</p>
              </div>
              
              <div className="flex flex-col items-center">
                <div className="text-blue-600 mb-6">
                  <Search size={48} />
                </div>
                <h3 className="text-xl font-semibold mb-2">AI Analysis</h3>
                <p className="text-gray-600">Our AI searches for recent relevant research</p>
              </div>
              
              <div className="flex flex-col items-center">
                <div className="text-blue-600 mb-6">
                  <RefreshCw size={48} />
                </div>
                <h3 className="text-xl font-semibold mb-2">Review Updates</h3>
                <p className="text-gray-600">Approve or reject suggested changes</p>
              </div>
              
              <div className="flex flex-col items-center">
                <div className="text-blue-600 mb-6">
                  <Download size={48} />
                </div>
                <h3 className="text-xl font-semibold mb-2">Download Updated Slides</h3>
                <p className="text-gray-600">Get your presentation with all the latest research</p>
              </div>
            </div>
          </div>
        </section>
      </main>
      
      <Footer />
    </div>
  );
};

export default Index;
