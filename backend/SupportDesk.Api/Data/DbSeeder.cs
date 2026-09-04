using SupportDesk.Api.Enums;
using SupportDesk.Api.Models;
using SupportDesk.Api.Services;

namespace SupportDesk.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        TicketRulesService ticketRules)
    {
        if (context.Agents.Any() || context.Tickets.Any())
            return;

        var agents = new List<Agent>
        {
            new()
            {
                FullName = "Emma Wilson",
                Email = "emma@supportdesk.com",
                Department = Department.Technical,
                Active = true
            },
            new()
            {
                FullName = "Liam Brown",
                Email = "liam@supportdesk.com",
                Department = Department.Billing,
                Active = true
            },
            new()
            {
                FullName = "Sophia Miller",
                Email = "sophia@supportdesk.com",
                Department = Department.General,
                Active = true
            },
            new()
            {
                FullName = "Noah Davis",
                Email = "noah@supportdesk.com",
                Department = Department.Technical,
                Active = true
            },
            new()
            {
                FullName = "Olivia Taylor",
                Email = "olivia@supportdesk.com",
                Department = Department.General,
                Active = false
            }
        };

        context.Agents.AddRange(agents);
        await context.SaveChangesAsync();

        var now = DateTime.UtcNow;

        var tickets = new List<Ticket>();

        for (var i = 1; i <= 20; i++)
        {
            var createdDate = now.AddDays(-i);

            var priority = (i % 4) switch
            {
                0 => TicketPriority.Critical,
                1 => TicketPriority.High,
                2 => TicketPriority.Normal,
                _ => TicketPriority.Low
            };

            var status = (i % 4) switch
            {
                0 => TicketStatus.New,
                1 => TicketStatus.InProgress,
                2 => TicketStatus.Resolved,
                _ => TicketStatus.Closed
            };

            Agent? assignedAgent = null;

            if (status != TicketStatus.New)
            {
                assignedAgent = agents[(i - 1) % 4];
            }

            var ticket = new Ticket
            {
                Reference = $"TCK-{now.Year}-{i:D4}",
                Title = $"Sample support issue {i}",
                Description = $"Seeded support ticket number {i}.",
                CustomerName = $"Customer {i}",
                CustomerEmail = $"customer{i}@example.com",
                Priority = priority,
                Status = status,
                AssignedAgentId = assignedAgent?.Id,
                CreatedDate = createdDate,
                LastModifiedDate = createdDate,
                DueDate = ticketRules.CalculateDueDate(createdDate, priority)
            };

            if (status == TicketStatus.Resolved)
            {
                ticket.ResolvedDate = createdDate.AddDays(1);
            }

            if (status == TicketStatus.Closed)
            {
                ticket.ResolvedDate = createdDate.AddDays(1);
                ticket.ClosedDate = createdDate.AddDays(2);
            }

            tickets.Add(ticket);
        }

        context.Tickets.AddRange(tickets);
        await context.SaveChangesAsync();
    }
}