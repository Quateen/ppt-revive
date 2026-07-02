//using PPTRevive.Application.Common.Interfaces;
//using PPTRevive.Application.Common.Models;
//using PPTRevive.Domain.Entities;

//namespace PPTRevive.Infrastructure.Repositories;
//public class FileUploadRepository : IFileUploadRepository
//{
//    private readonly IS3FileRepository _s3FileRepository;
//    private readonly IDataRepository<FileStore> _fileStoreRepository;
//    private readonly IUnitOfWork _unitOfWork;

//    public FileUploadRepository(
//            IS3FileRepository s3FileRepository,
//            IDataRepository<FileStore> fileStoreRepository,
//            IUnitOfWork unitOfWork
//        )
//    {
//        _s3FileRepository = s3FileRepository;
//        _fileStoreRepository = fileStoreRepository;
//        _unitOfWork = unitOfWork;
//    }

//    public async Task<FileDto> UploadAndStoreFileAsync(Stream fileStream, string fileName, string contentType, long fileSize, int userId)
//    {
//        var fileDto = new FileDto
//        {
//            FileName = fileName,
//            ContentType = contentType,
//            FileSize = fileSize
//        };

//        var (uploadedFile, fileUrl) = await _s3FileRepository.UploadFileAsync(fileStream, fileDto);

//        var fileStore = new FileStore
//        {
//            FileName = uploadedFile.FileName,
//            FilePath = uploadedFile.FilePath,
//            ContentType = uploadedFile.ContentType,
//            AWSBucketName = uploadedFile.AWSBucketName,
//            FileSize = uploadedFile.FileSize,
//            CreatedBy = userId,
//            Created = DateTime.UtcNow,
//            LastModifiedBy = userId,
//            LastModified = DateTime.UtcNow
//        };

//        _fileStoreRepository.Add(fileStore, userId);
//        await _unitOfWork.SaveChangesAsync(CancellationToken.None);

//        return new FileDto
//        {
//            FileName = uploadedFile.FileName,
//            ContentType = uploadedFile.ContentType,
//            FileSize = uploadedFile.FileSize,
//            FilePath = uploadedFile.FilePath,
//            AWSBucketName = uploadedFile.AWSBucketName,
//            FileStoreId = fileStore.Id,
//            FileUrl = fileUrl
//        };
//    }
//}
