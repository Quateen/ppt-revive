using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;

namespace PPTRevive.Infrastructure.Repositories;
public class S3FileRepository : IS3FileRepository
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _folderName;

    public S3FileRepository(IConfiguration configuration)
    {
        _bucketName = configuration["AWS:BucketName"]!;
        _folderName = configuration["AWS:FolderName"]!;

        _s3Client = new AmazonS3Client(
            configuration["AWS:AccessKey"],
            configuration["AWS:SecretKey"],
            RegionEndpoint.USEast2
        );
    }
    public async Task<(FileDto file, string url)> UploadFileAsync(Stream fileStream, FileDto fileDto)
    {
        var fileUrl = string.Empty;
        try
        {
            var filePath = $"{_folderName}/{Path.GetFileName(fileDto.FileName)}";

            fileDto.FilePath = filePath;
            fileDto.AWSBucketName = _bucketName;
            var tags = GenerateTagsFromFileDto(fileDto);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = filePath,
                BucketName = _bucketName,
                ContentType = fileDto.ContentType,
                //CannedACL = S3CannedACL. // or Private
                TagSet = tags
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);
            fileUrl = await GetFileUrlAsync(filePath);

            return (fileDto, fileUrl);
        }
        catch (Exception ex)
        {
            // TODO: log
            Console.WriteLine(ex.Message);
        }

        return (fileDto, fileUrl); ;
    }

    public async Task<string> GetFileUrlAsync(string filepath, int expiresInMin = 15)
    {
        try
        {
            return await Task.Run(() =>
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = filepath,
                    Expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(expiresInMin)),
                };

                return _s3Client.GetPreSignedURL(request);
            });
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private List<Tag> GenerateTagsFromFileDto(FileDto fileDto)
    {
        var tags = new List<Tag>();

        var properties = typeof(FileDto).GetProperties();
        foreach (var prop in properties)
        {
            var key = prop.Name;
            var value = prop.GetValue(fileDto)?.ToString() ?? string.Empty;

            tags.Add(new Tag { Key = SanitizeForTag(key), Value = SanitizeForTag(value) });
        }

        return tags;
    }

    private string SanitizeForTag(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        // Replace or remove invalid characters
        var allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ._:/+=@-";
        var result = new string(input.Where(c => allowed.Contains(c)).ToArray());

        return result.Length > 256 ? result.Substring(0, 256) : result;
    }
}
