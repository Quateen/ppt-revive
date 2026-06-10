using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Constants;
using PPTRevive.Domain.Entities;

namespace PPTRevive.Application.PPT.Command;

public class PPTStartProcessingCommand : IRequest<ResponseBase>
{
    public string JobId { get; set; } = null!;
}

public class PPTStartProcessingHandler : IRequestHandler<PPTStartProcessingCommand, ResponseBase>
{
    private readonly IMemoryCache _cache;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IFileService _fileService;

    public PPTStartProcessingHandler(IMemoryCache cache, IServiceScopeFactory scopeFactory, IFileService fileService)
    {
        _cache = cache;
        _scopeFactory = scopeFactory;
        _fileService = fileService;
    }

    private ResponseBase ErrorResponse(string error)
    {
        return new ResponseBase
        {
            Status = false,
            Error = error
        };
    }

    public async Task<ResponseBase> Handle(PPTStartProcessingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_cache.TryGetValue(request.JobId.ToString(), out ProcessingResult? result) || result == null)
            {
                return new ResponseBase
                {
                    Status = true,
                    Data = new ProcessingResult
                    {
                        Status = ProcessingStatus.Failed,
                        Error = "Job not found or expired."
                    },
                };
            }

            var processingResult = await Task.FromResult(result);
            var pptBytes = await _fileService.ReadFileAsBytesAsync(processingResult.FileName, PPTDirectories.ORIGNAL_PPT);

            // Start background processing
            _ = Task.Run(async () =>
            {
                try
                {
                    _cache.Set(request.JobId.ToString(), new ProcessingResult
                    {
                        Status = ProcessingStatus.InProgress,
                    }, TimeSpan.FromHours(2));

                    using var scope = _scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<IProcessPptJobService>();
                    var result = await handler.ProcessAsync(pptBytes, processingResult.FileName, cancellationToken);

                    _cache.Set(request.JobId.ToString(), new ProcessingResult
                    {
                        Status = ProcessingStatus.Completed,
                        Result = result.Data
                    }, TimeSpan.FromHours(2));
                }
                catch (Exception ex)
                {
                    _cache.Set(request.JobId.ToString(), new ProcessingResult
                    {
                        Status = ProcessingStatus.Failed,
                        Error = ex.Message
                    }, TimeSpan.FromHours(2));
                }
            });

            if (!_cache.TryGetValue(request.JobId.ToString(), out result) || result == null)
            {
                return new ResponseBase
                {
                    Status = true,
                    Data = new ProcessingResult
                    {
                        Status = ProcessingStatus.Failed,
                        Error = "Job not found or expired."
                    },
                };
            }

            result.Result = request.JobId.ToString();
            return new ResponseBase
            {
                Status = true,
                Data = result,
                Message = "Processing Started"
            };
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex.Message);
        }
    }
}
