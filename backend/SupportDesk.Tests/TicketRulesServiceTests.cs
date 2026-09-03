using SupportDesk.Api.Enums;
using SupportDesk.Api.Models;
using SupportDesk.Api.Services;

namespace SupportDesk.Tests;

public class TicketRulesServiceTests
{
    private readonly TicketRulesService _service = new();

    [Fact]
    public void CalculateDueDate_Critical_AddsFourHours()
    {
        var created = new DateTime(2026, 9, 3, 10, 0, 0, DateTimeKind.Utc);

        var result = _service.CalculateDueDate(created, TicketPriority.Critical);

        Assert.Equal(created.AddHours(4), result);
    }

    [Fact]
    public void CalculateDueDate_Normal_AddsThreeDays()
    {
        var created = new DateTime(2026, 9, 3, 10, 0, 0, DateTimeKind.Utc);

        var result = _service.CalculateDueDate(created, TicketPriority.Normal);

        Assert.Equal(created.AddDays(3), result);
    }

    [Fact]
    public void IsValidStatusTransition_NewToInProgress_ReturnsTrue()
    {
        var result = _service.IsValidStatusTransition(
            TicketStatus.New,
            TicketStatus.InProgress);

        Assert.True(result);
    }

    [Fact]
    public void IsValidStatusTransition_NewToResolved_ReturnsFalse()
    {
        var result = _service.IsValidStatusTransition(
            TicketStatus.New,
            TicketStatus.Resolved);

        Assert.False(result);
    }

    [Fact]
    public void ValidateStatusTransition_InProgressWithoutActiveAgent_Throws()
    {
        var ticket = new Ticket
        {
            Status = TicketStatus.New,
            AssignedAgent = null
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            _service.ValidateStatusTransition(
                ticket,
                TicketStatus.InProgress));

        Assert.Contains("active agent", exception.Message.ToLower());
    }

    [Fact]
    public void ValidateAgentAssignment_InactiveAgent_Throws()
    {
        var agent = new Agent
        {
            Active = false
        };

        Assert.Throws<InvalidOperationException>(() =>
            _service.ValidateAgentAssignment(agent));
    }
}