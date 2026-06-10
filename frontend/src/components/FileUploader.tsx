import React, { useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { Upload, X, FileType } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useToast } from '@/components/ui/use-toast';
import JSZip from 'jszip';
import { AppConfig } from '@/config';

interface FileUploaderProps {
  onFileSelected: (file: File) => void;
  selectedFile: File | null;
  onRemoveFile?: () => void;
  disabled?: boolean;
}

const FileUploader: React.FC<FileUploaderProps> = ({ onFileSelected, selectedFile, onRemoveFile, disabled }) => {
  const { toast } = useToast();

  const MAX_FILE_SIZE_MB = AppConfig.MAX_PPT_FILE_SIZE_MB;
  const MAX_SLIDES = AppConfig.MAX_PPT_SLIDES;

  const checkSlideCount = async (file: File): Promise<number> => {
    try {
      const zip = await JSZip.loadAsync(file);
      const slideFiles = Object.keys(zip.files).filter(name => /^ppt\/slides\/slide\d+\.xml$/i.test(name));
      return slideFiles.length;
    } catch (error) {
      console.error("Slide count extraction failed", error);
      return -1;
    }
  };

  const onDrop = useCallback(async (acceptedFiles: File[]) => {
    // debugger

    if (disabled) return;

    const uploadedFile = acceptedFiles[0];

    const isPptx = /\.pptx$/i.test(uploadedFile.name);
    if (!isPptx) {
      toast({
        variant: "destructive",
        title: "Invalid file type",
        description: "Only .pptx files are supported."
      });
      return;
    }

    const sizeMB = uploadedFile.size / (1024 * 1024);
    if (sizeMB > MAX_FILE_SIZE_MB) {
      toast({
        variant: "destructive",
        title: "File too large",
        description: `Maximum allowed size is ${MAX_FILE_SIZE_MB} MB.`
      });
      return;
    }

    const slideCount = await checkSlideCount(uploadedFile);
    if (slideCount === -1) {
      toast({
        variant: "destructive",
        title: "Could not read slides",
        description: "Please try another file or contact support."
      });
      return;
    }

    if (slideCount > MAX_SLIDES) {
      toast({
        variant: "destructive",
        title: "Too many slides",
        description: `The presentation has ${slideCount} slides. Maximum allowed is ${MAX_SLIDES}.`
      });
      return;
    }

    onFileSelected(uploadedFile);
  }, [toast, disabled, onFileSelected, MAX_FILE_SIZE_MB, MAX_SLIDES]);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    maxFiles: 1,
    // accept: {
    //   'application/vnd.openxmlformats-officedocument.presentationml.presentation': ['.pptx']
    // }
  });

  return (
    <div className="w-full">
      {!selectedFile ? (
        <div
          {...getRootProps()}
          className={`file-drop-area h-64 flex flex-col items-center justify-center p-6 border-2 border-dashed rounded-lg ${isDragActive ? 'border-medical-600 bg-medical-50' : 'border-gray-300'}`}
        >
          <input {...getInputProps()} />
          <div className="bg-medical-100 p-3 rounded-full">
            <Upload className="h-8 w-8 text-medical-600" />
          </div>
          <p className="mt-4 text-lg font-medium text-gray-700">Drag and drop your .pptx file</p>
          <p className="text-sm text-gray-500 mt-1">or click to browse files</p>
          <p className="text-xs text-gray-400 mt-3">
            Accepted: .pptx only · Max {MAX_FILE_SIZE_MB} MB · Max {MAX_SLIDES} slides
          </p>
        </div>
      ) : (
        <div className="border border-gray-200 rounded-lg p-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-3">
              <div className="bg-medical-100 p-2 rounded-md">
                <FileType className="h-6 w-6 text-medical-600" />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-700">{selectedFile.name}</p>
                <p className="text-xs text-gray-500">{(selectedFile.size / (1024 * 1024)).toFixed(2)} MB</p>
              </div>
            </div>
            <Button
              variant="ghost"
              size="sm"
              onClick={onRemoveFile}
              disabled={disabled}
              className="text-gray-500 hover:text-gray-700"
            >
              <X className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}
    </div>
  );
};

export default FileUploader;
