namespace PPTRevive.Application.Common.Models;
public class TicketDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public string? Status { get; set; }
    public int PriorityId { get; set; }
    public string? Priority { get; set; }
    public int Index { get; set; }
    public int AssignedToId { get; set; }
    public string? AssignedTo { get; set; }
    public int CategoryId { get; set; }
    public string? Category { get; set; }
    public int? User { get; set; }
    public List<TicketCommentDto> TicketComment { get; set; } = new List<TicketCommentDto>();
    public DateTimeOffset Created { get; set; }
    public int? CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public int? LastModifiedBy { get; set; }
    public int TicketId { get; set; }
}

public class TicketCommentDto
{
    public string Comment { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? UserName { get; set; }
}
