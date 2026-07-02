using PPTRevive.Application.Common.Models;

namespace PPTRevive.Application.Common.Interfaces;

public interface IProcessPptJobService
{
    /// <summary>
    /// Processes a PowerPoint (.pptx) file from byte array input and returns an enriched response.
    /// </summary>
    /// <param name="pptBytes">The byte array content of the uploaded PowerPoint file.</param>
    /// <param name="cancellationToken">Token to handle cancellation.</param>
    /// <returns>ResponseBase containing updated file and slide data.</returns>
    Task<ResponseBase> ProcessAsync(byte[] pptBytes, string fileName, CancellationToken cancellationToken);
}
