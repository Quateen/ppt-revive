using Microsoft.Extensions.Caching.Memory;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Entities;

namespace PPTRevive.Application.PPT.Query;

public class GetPptProcessingStatusQuery : IRequest<ResponseBase>
{
    public string JobId { get; set; } = string.Empty;
}

public class GetPptProcessingStatusHandler : IRequestHandler<GetPptProcessingStatusQuery, ResponseBase>
{
    private readonly IMemoryCache _cache;
    private readonly IUser _currentUser;

    public GetPptProcessingStatusHandler(IMemoryCache cache, IUser currentUser)
    {
        _cache = cache;
        _currentUser = currentUser;
    }

    private ResponseBase ErrorResponse(string error)
    {
        return new ResponseBase
        {
            Status = false,
            Error = error
        };
    }

    public async Task<ResponseBase> Handle(GetPptProcessingStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Only the uploading user may read a job's status/result.
            if (_cache.TryGetValue(request.JobId.ToString(), out ProcessingResult? result)
                && result!.OwnerUserId == _currentUser.Id)
            {
                return new ResponseBase
                {
                    Status = true,
                    Data = await Task.FromResult(result!)
                };
            }

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
        catch (Exception ex)
        {
            return ErrorResponse(ex.Message);
        }
    }
}
