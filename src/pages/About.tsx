
import React from 'react';
import Header from '@/components/Header';
import Footer from '@/components/Footer';
import { Card, CardContent } from '@/components/ui/card';
import { FileText, Search, RotateCcw, FileCheck } from 'lucide-react';

const About = () => {
  return (
    <div className="flex flex-col min-h-screen">
      <Header />
      
      <main className="container mx-auto px-4 py-12 flex-1">
        <div className="max-w-3xl mx-auto">
          <h1 className="text-3xl font-bold text-gray-900 mb-6">About MediPresent Revive</h1>
          
          <div className="prose max-w-none">
            <p className="text-gray-700 mb-6">
              MediPresent Revive is a specialized AI tool designed to help medical professionals update their 
              PowerPoint presentations with the latest research and information. Our platform streamlines the process 
              of incorporating new medical knowledge into existing presentations while preserving the original structure.
            </p>
            
            <h2 className="text-2xl font-semibold text-gray-800 mt-8 mb-4">How It Works</h2>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
              <Card>
                <CardContent className="pt-6">
                  <div className="flex items-center gap-3 mb-3">
                    <div className="bg-medical-100 p-2 rounded-md">
                      <FileText className="h-5 w-5 text-medical-600" />
                    </div>
                    <h3 className="font-medium text-lg">Upload Presentation</h3>
                  </div>
                  <p className="text-gray-600 text-sm">
                    Upload your PowerPoint presentation and any additional reference materials you've already found.
                  </p>
                </CardContent>
              </Card>
              
              <Card>
                <CardContent className="pt-6">
                  <div className="flex items-center gap-3 mb-3">
                    <div className="bg-medical-100 p-2 rounded-md">
                      <Search className="h-5 w-5 text-medical-600" />
                    </div>
                    <h3 className="font-medium text-lg">AI Analysis</h3>
                  </div>
                  <p className="text-gray-600 text-sm">
                    Our AI analyzes your slides and searches medical databases for recent, relevant research.
                  </p>
                </CardContent>
              </Card>
              
              <Card>
                <CardContent className="pt-6">
                  <div className="flex items-center gap-3 mb-3">
                    <div className="bg-medical-100 p-2 rounded-md">
                      <RotateCcw className="h-5 w-5 text-medical-600" />
                    </div>
                    <h3 className="font-medium text-lg">Review Updates</h3>
                  </div>
                  <p className="text-gray-600 text-sm">
                    Review suggested updates slide by slide, with the ability to approve, modify, or reject changes.
                  </p>
                </CardContent>
              </Card>
              
              <Card>
                <CardContent className="pt-6">
                  <div className="flex items-center gap-3 mb-3">
                    <div className="bg-medical-100 p-2 rounded-md">
                      <FileCheck className="h-5 w-5 text-medical-600" />
                    </div>
                    <h3 className="font-medium text-lg">Download Updated Slides</h3>
                  </div>
                  <p className="text-gray-600 text-sm">
                    Generate and download your updated presentation with proper citations and references.
                  </p>
                </CardContent>
              </Card>
            </div>
            
            <h2 className="text-2xl font-semibold text-gray-800 mt-8 mb-4">Our Approach</h2>
            
            <p className="text-gray-700 mb-4">
              MediPresent Revive focuses on providing evidence-based updates from reputable sources. We prioritize:
            </p>
            
            <ul className="list-disc pl-6 mb-6 space-y-2">
              <li className="text-gray-700">
                <span className="font-medium">Recent Research</span>: We search for studies published in the last 2 years.
              </li>
              <li className="text-gray-700">
                <span className="font-medium">Authoritative Sources</span>: We emphasize high-impact journals and guidelines from major medical associations.
              </li>
              <li className="text-gray-700">
                <span className="font-medium">Proper Attribution</span>: All suggested updates include proper citations.
              </li>
              <li className="text-gray-700">
                <span className="font-medium">Style Preservation</span>: We maintain the original presentation's structure and design elements.
              </li>
            </ul>
            
            <p className="text-gray-700">
              MediPresent Revive is designed to save valuable time for medical professionals while ensuring their 
              educational and clinical presentations contain the most up-to-date information available.
            </p>
          </div>
        </div>
      </main>
      
      <Footer />
    </div>
  );
};

export default About;
