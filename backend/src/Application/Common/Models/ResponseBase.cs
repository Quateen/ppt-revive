namespace PPTRevive.Application.Common.Models;

public sealed class ResponseBase
{
    public object? Data { get; set; }
    public bool Status { get; set; }
    public string? Message { get; set; }
    public object? Error { get; set; }
    public Pagination? Pagination { get; set; }

}

public sealed class Pagination
{
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public int PageSize { get; set; }
}
