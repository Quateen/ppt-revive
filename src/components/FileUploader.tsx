
import React, { useCallback, useState } from 'react';
import { useDropzone } from 'react-dropzone';
import { Upload, X, FileType } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { useToast } from '@/components/ui/use-toast';

interface FileUploaderProps {
  onFileUploaded: (file: File) => void;
}

const FileUploader: React.FC<FileUploaderProps> = ({ onFileUploaded }) => {
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const [progress, setProgress] = useState(0);
  const { toast } = useToast();

  const onDrop = useCallback((acceptedFiles: File[]) => {
    const uploadedFile = acceptedFiles[0];
    
    // Check if file is a PowerPoint file or Excel (for pptx files)
    const isPowerPoint = /\.(ppt|pptx|pot|potx|pps|ppsx|pptm|potm|ppsm)$/i.test(uploadedFile.name);
    if (!isPowerPoint) {
      toast({
        variant: "destructive",
        title: "Invalid file type",
        description: "Please upload a PowerPoint presentation file"
      });
      return;
    }
    
    setFile(uploadedFile);
    simulateUpload(uploadedFile);
  }, [toast]);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({ 
    onDrop,
    accept: {
      'application/vnd.ms-powerpoint': ['.ppt', '.pot', '.pps'],
      'application/vnd.openxmlformats-officedocument.presentationml.presentation': ['.pptx'],
      'application/vnd.openxmlformats-officedocument.presentationml.template': ['.potx'],
      'application/vnd.openxmlformats-officedocument.presentationml.slideshow': ['.ppsx'],
      'application/vnd.ms-powerpoint.presentation.macroEnabled.12': ['.pptm'],
      'application/vnd.ms-powerpoint.template.macroEnabled.12': ['.potm'],
      'application/vnd.ms-powerpoint.slideshow.macroEnabled.12': ['.ppsm']
    },
    maxFiles: 1
  });

  const simulateUpload = (uploadedFile: File) => {
    setUploading(true);
    setProgress(0);
    
    const interval = setInterval(() => {
      setProgress(prev => {
        if (prev >= 100) {
          clearInterval(interval);
          setUploading(false);
          // Pass the actual file to the parent component
          onFileUploaded(uploadedFile);
          toast({
            title: "Upload complete",
            description: `${uploadedFile.name} has been uploaded and is being processed.`
          });
          return 100;
        }
        return prev + 10;
      });
    }, 200);
  };

  const removeFile = () => {
    setFile(null);
    setProgress(0);
  };

  return (
    <div className="w-full">
      {!file ? (
        <div 
          {...getRootProps()} 
          className={`file-drop-area h-64 flex flex-col items-center justify-center p-6 border-2 border-dashed rounded-lg ${
            isDragActive ? 'border-medical-600 bg-medical-50' : 'border-gray-300'
          }`}
        >
          <input {...getInputProps()} />
          <div className="bg-medical-100 p-3 rounded-full">
            <Upload className="h-8 w-8 text-medical-600" />
          </div>
          <p className="mt-4 text-lg font-medium text-gray-700">Drag and drop your presentation</p>
          <p className="text-sm text-gray-500 mt-1">or click to browse files</p>
          <p className="text-xs text-gray-400 mt-3">PowerPoint files (.ppt, .pptx, etc.)</p>
        </div>
      ) : (
        <div className="border border-gray-200 rounded-lg p-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-3">
              <div className="bg-medical-100 p-2 rounded-md">
                <FileType className="h-6 w-6 text-medical-600" />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-700">{file.name}</p>
                <p className="text-xs text-gray-500">{(file.size / (1024 * 1024)).toFixed(2)} MB</p>
              </div>
            </div>
            <Button 
              variant="ghost" 
              size="sm" 
              onClick={removeFile}
              disabled={uploading}
              className="text-gray-500 hover:text-gray-700"
            >
              <X className="h-4 w-4" />
            </Button>
          </div>
          {uploading && (
            <div className="mt-4 space-y-2">
              <div className="flex justify-between text-xs text-gray-500">
                <span>Uploading</span>
                <span>{progress}%</span>
              </div>
              <Progress value={progress} className="h-1.5" />
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default FileUploader;
