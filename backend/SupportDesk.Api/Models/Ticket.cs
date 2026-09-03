using SupportDesk.Api.Enums;

namespace SupportDesk.Api.Models;

public class Ticket
{
    public int Id { get; set; }

    public string Reference { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;

    public TicketPriority Priority { get; set; }

    public TicketStatus Status { get; set; } = TicketStatus.New;

    public int? AssignedAgentId { get; set; }
    public Agent? AssignedAgent { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public DateTime DueDate { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}