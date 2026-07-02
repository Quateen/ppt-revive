using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Domain.Constants;

namespace PPTRevive.Infrastructure.Services;

public class FileService : IFileService
{
    // Uploaded originals and finalized decks live OUTSIDE wwwroot so the static-file
    // middleware never serves them; they are only reachable through the authenticated
    // download endpoint after an ownership check.
    private static string StorageRoot => Path.Combine(Directory.GetCurrentDirectory(), "ppt-storage");

    public async Task<string> SaveFile(string fileName, byte[] pptUpdated, string dirName)
    {
        var saveDirectory = Path.Combine(StorageRoot, dirName);
        Directory.CreateDirectory(saveDirectory);

        var safeName = Path.GetFileName(fileName); // strip any path components
        var filePath = Path.Combine(saveDirectory, safeName);

        await File.WriteAllBytesAsync(filePath, pptUpdated);

        var relativePath = Path.GetRelativePath(StorageRoot, filePath).Replace("\\", "/");
        return $"/{relativePath}";
    }

    public async Task<byte[]> ReadFileAsBytesAsync(string fileName, string dirName)
    {
        var safeName = Path.GetFileName(fileName); // prevent path traversal
        var filePath = Path.Combine(StorageRoot, dirName, safeName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"The file '{safeName}' was not found in directory '{dirName}'.");

        return await File.ReadAllBytesAsync(filePath);
    }

    public void DeleteFilesFromFolder(string dirName)
    {
        try
        {
            var folderPath = Path.Combine(StorageRoot, dirName);
            if (!Directory.Exists(folderPath))
                return;

            // Only remove files older than the job TTL so an in-flight job's files
            // are never deleted out from under it.
            var cutoff = DateTime.UtcNow.AddHours(-3);
            foreach (var file in Directory.GetFiles(folderPath))
            {
                if (File.GetLastWriteTimeUtc(file) < cutoff)
                    File.Delete(file);
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
