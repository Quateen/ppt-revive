using Microsoft.Extensions.Caching.Memory;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Constants;

namespace PPTRevive.Application.PPT.Query;

public class DownloadPptResult
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
}

public class DownloadPptQuery : IRequest<ResponseBase>
{
    public string JobId { get; set; } = string.Empty;
}

public class DownloadPptQueryHandler : IRequestHandler<DownloadPptQuery, ResponseBase>
{
    private readonly IMemoryCache _cache;
    private readonly IFileService _fileService;
    private readonly IUser _currentUser;

    public DownloadPptQueryHandler(IMemoryCache cache, IFileService fileService, IUser currentUser)
    {
        _cache = cache;
        _fileService = fileService;
        _currentUser = currentUser;
    }

    public async Task<ResponseBase> Handle(DownloadPptQuery request, CancellationToken cancellationToken)
    {
        // Only the owner of a completed job may download its finalized deck.
        if (!_cache.TryGetValue(request.JobId, out ProcessingResult? result)
            || result == null
            || result.OwnerUserId != _currentUser.Id)
        {
            return new ResponseBase { Status = false, Error = "File not found." };
        }

        var fileName = $"{request.JobId}-revived.pptx";
        try
        {
            var bytes = await _fileService.ReadFileAsBytesAsync(fileName, PPTDirectories.UPDATED_PPT);
            return new ResponseBase
            {
                Status = true,
                Data = new DownloadPptResult { Content = bytes, FileName = fileName }
            };
        }
        catch (FileNotFoundException)
        {
            return new ResponseBase { Status = false, Error = "File not found." };
        }
    }
}
