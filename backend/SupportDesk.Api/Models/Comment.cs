namespace SupportDesk.Api.Models;

public class Comment
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public Ticket Ticket { get; set; } = null!;

    public string AuthorName { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
}