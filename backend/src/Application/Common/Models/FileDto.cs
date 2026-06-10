namespace PPTRevive.Application.Common.Models;
public class FileDto
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string AWSBucketName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int FileCategory { get; set; } // Create Enum
    public int? FileSubCategory { get; set; } // Create Enum

    public int FileStoreId { get; set; }
    public string? FileUrl { get; set; }
}
