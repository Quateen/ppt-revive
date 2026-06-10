using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Constants;
using PPTRevive.Domain.Entities;

namespace PPTRevive.Application.PPT.Command;

public class PPTUploadCommand : IRequest<ResponseBase>
{
    public IFormFile File { get; set; } = null!;
}

public class UploadPptRequestValidator : AbstractValidator<PPTUploadCommand>
{
    private static readonly string[] AllowedExtensions = { ".ppt", ".pptx" };
    private readonly int _maxFileSizeMB;

    public UploadPptRequestValidator(IConfiguration configuration)
    {
        _maxFileSizeMB = configuration.GetValue<int>("FileUploadSettings:MaxFileSizeMB");

        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required.")
            .Must(BePowerPointFile).WithMessage("Only PowerPoint files (.ppt, .pptx) are allowed.")
            .Must(BeUnderMaxSize).WithMessage($"File size must be less than {_maxFileSizeMB} MB.");
    }

    private bool BePowerPointFile(IFormFile file)
    {
        if (file == null || string.IsNullOrEmpty(file.FileName))
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }

    private bool BeUnderMaxSize(IFormFile file)
    {
        if (file == null) return false;
        long maxBytes = _maxFileSizeMB * 1024 * 1024;
        return file.Length <= maxBytes;
    }

}

public class ProcessPptHandler : IRequestHandler<PPTUploadCommand, ResponseBase>
{
    private readonly IMemoryCache _cache;
    private readonly IFileService _fileService;
    private readonly IConfiguration _configuration;

    public ProcessPptHandler(IMemoryCache cache, IFileService fileService, IConfiguration configuration)
    {
        _cache = cache;
        _fileService = fileService;
        _configuration = configuration;
    }

    private ResponseBase ErrorResponse(string error)
    {
        return new ResponseBase
        {
            Status = false,
            Error = error
        };
    }

    public async Task<ResponseBase> Handle(PPTUploadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var validator = new UploadPptRequestValidator(_configuration);
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                // Return only the first error message
                var firstError = result.Errors.First().ErrorMessage;
                return ErrorResponse(firstError);
            }

            var fileExtension = GetFileExtension(request.File);
            var jobId = Guid.NewGuid();
            using var memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream, cancellationToken);
            var pptBytes = memoryStream.ToArray();

            var fileName = string.Format("{0}{1}", jobId.ToString(), fileExtension);
            await _fileService.SaveFile(fileName, pptBytes, PPTDirectories.ORIGNAL_PPT);

            _cache.Set(jobId.ToString(), new ProcessingResult
            {
                Status = ProcessingStatus.Pending,
                FileName = fileName,
            }, TimeSpan.FromHours(2));

            return new ResponseBase
            {
                Status = true,
                Data = jobId,
                Message = "File Uploaded Successfully"
            };
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex.Message);
        }
    }

    private string GetFileExtension(IFormFile file)
    {
        if (file == null || string.IsNullOrEmpty(file.FileName))
            throw new ArgumentException("Invalid file provided.");

        return Path.GetExtension(file.FileName).ToLowerInvariant(); // includes dot, e.g., ".pptx"
    }
}
