using SupportDesk.Api.Enums;

namespace SupportDesk.Api.DTOs;

public class AgentDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Department Department { get; set; }
    public bool Active { get; set; }
}