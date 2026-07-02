using PPTRevive.Domain.Entities;

namespace PPTRevive.Application.Common.Models;

public class ProcessingResult
{
    public ProcessingStatus Status { get; set; }
    public object? Result { get; set; }
    public string? Error { get; set; }
    public string FileName { get; set; } = string.Empty;

    // Id of the user who uploaded this job; used to enforce per-user access on
    // status/finalize/download. Not serialized to clients.
    [System.Text.Json.Serialization.JsonIgnore]
    public int OwnerUserId { get; set; }
}
