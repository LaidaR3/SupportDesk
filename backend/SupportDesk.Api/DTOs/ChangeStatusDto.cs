using SupportDesk.Api.Enums;

namespace SupportDesk.Api.DTOs;

public class ChangeStatusDto
{
    public TicketStatus Status { get; set; }
}