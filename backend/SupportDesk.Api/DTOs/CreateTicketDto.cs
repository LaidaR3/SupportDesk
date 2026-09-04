using System.ComponentModel.DataAnnotations;
using SupportDesk.Api.Enums;

namespace SupportDesk.Api.DTOs;

public class CreateTicketDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    public TicketPriority Priority { get; set; } = TicketPriority.Normal;
}