using PPTRevive.Domain.Entities;

namespace PPTRevive.Application.Common.Models;

public class ProcessingResult
{
    public ProcessingStatus Status { get; set; }
    public object? Result { get; set; }
    public string? Error { get; set; }
    public string FileName { get; set; } = string.Empty;
}
