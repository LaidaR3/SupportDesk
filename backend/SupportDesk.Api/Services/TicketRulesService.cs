using SupportDesk.Api.Enums;
using SupportDesk.Api.Models;

namespace SupportDesk.Api.Services;

public class TicketRulesService
{
    public DateTime CalculateDueDate(
        DateTime createdDate,
        TicketPriority priority)
    {
        return priority switch
        {
            TicketPriority.Critical => createdDate.AddHours(4),
            TicketPriority.High => createdDate.AddDays(1),
            TicketPriority.Normal => createdDate.AddDays(3),
            TicketPriority.Low => createdDate.AddDays(7),
            _ => throw new ArgumentOutOfRangeException(
                nameof(priority),
                "Invalid ticket priority.")
        };
    }

    public bool IsValidStatusTransition(
        TicketStatus currentStatus,
        TicketStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (TicketStatus.New, TicketStatus.InProgress) => true,
            (TicketStatus.InProgress, TicketStatus.Resolved) => true,
            (TicketStatus.Resolved, TicketStatus.Closed) => true,
            (TicketStatus.Resolved, TicketStatus.InProgress) => true,
            _ => false
        };
    }

    public void ValidateStatusTransition(
        Ticket ticket,
        TicketStatus newStatus)
    {
        if (ticket.Status == TicketStatus.Closed)
        {
            throw new InvalidOperationException(
                "A closed ticket cannot be reopened or modified.");
        }

        if (!IsValidStatusTransition(ticket.Status, newStatus))
        {
            throw new InvalidOperationException(
                $"Status transition from {ticket.Status} to {newStatus} is not allowed.");
        }

        if (newStatus == TicketStatus.InProgress &&
            (ticket.AssignedAgent == null || !ticket.AssignedAgent.Active))
        {
            throw new InvalidOperationException(
                "An active agent must be assigned before moving the ticket to In Progress.");
        }
    }

    public void ApplyStatusTransition(
        Ticket ticket,
        TicketStatus newStatus)
    {
        ValidateStatusTransition(ticket, newStatus);

        ticket.Status = newStatus;
        ticket.LastModifiedDate = DateTime.UtcNow;

        if (newStatus == TicketStatus.Resolved)
        {
            ticket.ResolvedDate = DateTime.UtcNow;
        }

        if (newStatus == TicketStatus.Closed)
        {
            ticket.ClosedDate = DateTime.UtcNow;
        }

        if (newStatus == TicketStatus.InProgress &&
            ticket.ResolvedDate.HasValue)
        {
            ticket.ResolvedDate = null;
        }
    }

    public bool IsOverdue(Ticket ticket)
    {
        return ticket.DueDate < DateTime.UtcNow &&
               ticket.Status != TicketStatus.Resolved &&
               ticket.Status != TicketStatus.Closed;
    }

    public void EnsureTicketIsEditable(Ticket ticket)
    {
        if (ticket.Status == TicketStatus.Closed)
        {
            throw new InvalidOperationException(
                "A closed ticket is read-only.");
        }
    }

    public void ValidateAgentAssignment(Agent agent)
    {
        if (!agent.Active)
        {
            throw new InvalidOperationException(
                "An inactive agent cannot be assigned to a ticket.");
        }
    }
}