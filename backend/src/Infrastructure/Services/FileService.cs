using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Domain.Constants;

namespace PPTRevive.Infrastructure.Services;

public class FileService : IFileService
{
    public async Task<string> SaveFile(string fileName, byte[] pptUpdated, string dirName)
    {
        var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var saveDirectory = Path.Combine(wwwRootPath, dirName);
        // Ensure the directory exists
        Directory.CreateDirectory(saveDirectory);

        var filePath = Path.Combine(saveDirectory, fileName);

        await File.WriteAllBytesAsync(filePath, pptUpdated);

        // Make relative path (e.g., "updated-ppts/xyz_Updated_file.pptx")
        var relativePath = Path.GetRelativePath(wwwRootPath, filePath).Replace("\\", "/");
        var fileUrl = $"/{relativePath}";
        return fileUrl;
    }

    public async Task<byte[]> ReadFileAsBytesAsync(string fileName, string dirName)
    {
        var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var filePath = Path.Combine(wwwRootPath, dirName, fileName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"The file '{fileName}' was not found in directory '{dirName}'.");

        return await File.ReadAllBytesAsync(filePath);
    }

    public void DeleteFilesFromFolder(string dirName)
    {
        try
        {
            var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folderPath = Path.Combine(wwwRootPath, dirName);

            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);

                foreach (string file in files)
                {
                    File.Delete(file);
                }

                Console.WriteLine("All files deleted successfully.");
            }
            else
            {
                Console.WriteLine("Folder does not exist.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while deleting files: {ex.Message}");
        }
    }

    public void DeletePPTFiles()
    {
        DeleteFilesFromFolder(PPTDirectories.ORIGNAL_PPT);
        DeleteFilesFromFolder(PPTDirectories.UPDATED_PPT);
    }

}
