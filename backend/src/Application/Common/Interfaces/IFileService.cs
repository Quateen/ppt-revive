using PPTRevive.Application.Common.Models;

namespace PPTRevive.Application.Common.Interfaces;

public interface IFileService
{
    Task<string> SaveFile(string fileName, byte[] pptUpdated, string dirName);
    Task<byte[]> ReadFileAsBytesAsync(string fileName, string dirName);
    void DeleteFilesFromFolder(string dirName);
    void DeletePPTFiles();
}
