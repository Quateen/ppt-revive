using PPTRevive.Application.Common.Models;

namespace PPTRevive.Application.Common.Behaviours;
public static class ResponseHelper
{
    public static ResponseBase ErrorResponse(string error, int errorCodeId)
    {
        return new ResponseBase
        {
            Status = false,
            Error = new
            {
                error,
                errorCodeId
            }
        };
    }
    public class ErrorDetails
    {
        public List<string> Errors { get; set; } = new();
        public int ErrorCodeId { get; set; }
    }

}
