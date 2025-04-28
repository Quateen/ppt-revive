
import React from 'react';

interface PresentationHeaderProps {
  title: string;
  author?: string;
  originalFileName: string;
}

const PresentationHeader: React.FC<PresentationHeaderProps> = ({ 
  title, 
  author,
  originalFileName 
}) => {
  return (
    <div className="mb-6">
      <h1 className="text-2xl font-bold text-gray-900">{title}</h1>
      <div className="text-sm text-gray-500 mt-1">
        {author && <span>By {author} • </span>}
        <span>Original file: {originalFileName}</span>
      </div>
    </div>
  );
};

export default PresentationHeader;
