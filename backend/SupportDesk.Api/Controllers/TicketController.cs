using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Data;
using SupportDesk.Api.DTOs;
using SupportDesk.Api.Enums;
using SupportDesk.Api.Models;
using SupportDesk.Api.Services;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TicketRulesService _ticketRules;

    public TicketsController(
        AppDbContext context,
        TicketRulesService ticketRules)
    {
        _context = context;
        _ticketRules = ticketRules;
    }


    [HttpGet]
    public async Task<ActionResult<PagedResultDto<TicketDto>>> GetTickets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] TicketStatus? status = null,
        [FromQuery] TicketPriority? priority = null,
        [FromQuery] int? agentId = null,
        [FromQuery] bool? overdue = null)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 100)
            pageSize = 100;

        var query = _context.Tickets
            .AsNoTracking()
            .Include(t => t.AssignedAgent)
            .AsQueryable();

        // Search by reference, title, customer name or customer email
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();

            query = query.Where(t =>
                t.Reference.ToLower().Contains(term) ||
                t.Title.ToLower().Contains(term) ||
                t.CustomerName.ToLower().Contains(term) ||
                t.CustomerEmail.ToLower().Contains(term));
        }

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (agentId.HasValue)
            query = query.Where(t => t.AssignedAgentId == agentId.Value);

        if (overdue == true)
        {
            var now = DateTime.UtcNow;

            query = query.Where(t =>
                t.DueDate < now &&
                t.Status != TicketStatus.Resolved &&
                t.Status != TicketStatus.Closed);
        }

        var totalCount = await query.CountAsync();

        var tickets = await query
            .OrderByDescending(t => t.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketDto
            {
                Id = t.Id,
                Reference = t.Reference,
                Title = t.Title,
                Description = t.Description,
                CustomerName = t.CustomerName,
                CustomerEmail = t.CustomerEmail,
                Priority = t.Priority,
                Status = t.Status,

                AssignedAgent = t.AssignedAgent == null
                    ? null
                    : new AgentDto
                    {
                        Id = t.AssignedAgent.Id,
                        FullName = t.AssignedAgent.FullName,
                        Email = t.AssignedAgent.Email,
                        Department = t.AssignedAgent.Department,
                        Active = t.AssignedAgent.Active
                    },

                CreatedDate = t.CreatedDate,
                LastModifiedDate = t.LastModifiedDate,
                ResolvedDate = t.ResolvedDate,
                ClosedDate = t.ClosedDate,
                DueDate = t.DueDate,

                IsOverdue =
                    t.DueDate < DateTime.UtcNow &&
                    t.Status != TicketStatus.Resolved &&
                    t.Status != TicketStatus.Closed
            })
            .ToListAsync();

        return Ok(new PagedResultDto<TicketDto>
        {
            Items = tickets,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketDto>> GetTicketById(int id)
    {
        var ticket = await _context.Tickets
            .AsNoTracking()
            .Include(t => t.AssignedAgent)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            return NotFound(new
            {
                code = "TICKET_NOT_FOUND",
                message = $"Ticket with id {id} was not found."
            });
        }

        var result = new TicketDto
        {
            Id = ticket.Id,
            Reference = ticket.Reference,
            Title = ticket.Title,
            Description = ticket.Description,
            CustomerName = ticket.CustomerName,
            CustomerEmail = ticket.CustomerEmail,
            Priority = ticket.Priority,
            Status = ticket.Status,

            AssignedAgent = ticket.AssignedAgent == null
                ? null
                : new AgentDto
                {
                    Id = ticket.AssignedAgent.Id,
                    FullName = ticket.AssignedAgent.FullName,
                    Email = ticket.AssignedAgent.Email,
                    Department = ticket.AssignedAgent.Department,
                    Active = ticket.AssignedAgent.Active
                },

            CreatedDate = ticket.CreatedDate,
            LastModifiedDate = ticket.LastModifiedDate,
            ResolvedDate = ticket.ResolvedDate,
            ClosedDate = ticket.ClosedDate,
            DueDate = ticket.DueDate,
            IsOverdue = _ticketRules.IsOverdue(ticket),

            Comments = ticket.Comments
                .OrderBy(c => c.CreatedDate)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    AuthorName = c.AuthorName,
                    Body = c.Body,
                    CreatedDate = c.CreatedDate
                })
                .ToList()
        };

        return Ok(result);
    }

    // create ticket

    [HttpPost]
    public async Task<ActionResult<TicketDto>> CreateTicket(
        [FromBody] CreateTicketDto dto)
    {
        var now = DateTime.UtcNow;

        var ticket = new Ticket
        {
            // Temporary unique value until we have the database-generated Id.
            Reference = $"TEMP-{Guid.NewGuid()}",
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            CustomerName = dto.CustomerName.Trim(),
            CustomerEmail = dto.CustomerEmail.Trim(),
            Priority = dto.Priority,
            Status = TicketStatus.New,
            AssignedAgentId = null,
            CreatedDate = now,
            LastModifiedDate = now,
            DueDate = _ticketRules.CalculateDueDate(now, dto.Priority)
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        // Id is now generated by PostgreSQL.
        ticket.Reference = $"TCK-{ticket.CreatedDate.Year}-{ticket.Id:D4}";
        await _context.SaveChangesAsync();

        var result = new TicketDto
        {
            Id = ticket.Id,
            Reference = ticket.Reference,
            Title = ticket.Title,
            Description = ticket.Description,
            CustomerName = ticket.CustomerName,
            CustomerEmail = ticket.CustomerEmail,
            Priority = ticket.Priority,
            Status = ticket.Status,
            AssignedAgent = null,
            CreatedDate = ticket.CreatedDate,
            LastModifiedDate = ticket.LastModifiedDate,
            ResolvedDate = ticket.ResolvedDate,
            ClosedDate = ticket.ClosedDate,
            DueDate = ticket.DueDate,
            IsOverdue = _ticketRules.IsOverdue(ticket)
        };

        return CreatedAtAction(
            nameof(GetTicketById),
            new { id = ticket.Id },
            result);
    }


    // Assign Agent

    [HttpPut("{id:int}/assignment")]
    public async Task<ActionResult<TicketDto>> AssignAgent(
        int id,
        [FromBody] AssignAgentDto dto)
    {
        var ticket = await _context.Tickets
            .Include(t => t.AssignedAgent)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            return NotFound(new
            {
                code = "TICKET_NOT_FOUND",
                message = $"Ticket with id {id} was not found."
            });
        }

        // Closed tickets are read-only.
        _ticketRules.EnsureTicketIsEditable(ticket);

        // null means unassign the current agent.
        if (dto.AgentId == null)
        {
            ticket.AssignedAgentId = null;
            ticket.AssignedAgent = null;
            ticket.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetTicketById(id);
        }

        var agent = await _context.Agents
            .FirstOrDefaultAsync(a => a.Id == dto.AgentId.Value);

        if (agent == null)
        {
            return NotFound(new
            {
                code = "AGENT_NOT_FOUND",
                message = $"Agent with id {dto.AgentId.Value} was not found."
            });
        }

        _ticketRules.ValidateAgentAssignment(agent);

        ticket.AssignedAgentId = agent.Id;
        ticket.AssignedAgent = agent;
        ticket.LastModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetTicketById(id);
    }


    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<TicketDto>> ChangeStatus(
    int id,
    [FromBody] ChangeStatusDto dto)
    {
        var ticket = await _context.Tickets
            .Include(t => t.AssignedAgent)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            return NotFound(new
            {
                code = "TICKET_NOT_FOUND",
                message = $"Ticket with id {id} was not found."
            });
        }

        _ticketRules.ApplyStatusTransition(ticket, dto.Status);

        await _context.SaveChangesAsync();

        return await GetTicketById(id);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TicketDto>> UpdateTicket(
    int id,
    [FromBody] UpdateTicketDto dto)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            return NotFound(new
            {
                code = "TICKET_NOT_FOUND",
                message = $"Ticket with id {id} was not found."
            });
        }

        _ticketRules.EnsureTicketIsEditable(ticket);

        var priorityChanged = ticket.Priority != dto.Priority;

        ticket.Title = dto.Title.Trim();
        ticket.Description = dto.Description.Trim();
        ticket.CustomerName = dto.CustomerName.Trim();
        ticket.CustomerEmail = dto.CustomerEmail.Trim();

        if (priorityChanged &&
            ticket.Status != TicketStatus.Resolved &&
            ticket.Status != TicketStatus.Closed)
        {
            ticket.DueDate = _ticketRules.CalculateDueDate(
                ticket.CreatedDate,
                dto.Priority);
        }

        ticket.Priority = dto.Priority;
        ticket.LastModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetTicketById(id);
    }


    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(
    int id,
    [FromBody] AddCommentDto dto)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            return NotFound(new
            {
                code = "TICKET_NOT_FOUND",
                message = $"Ticket with id {id} was not found."
            });
        }

        _ticketRules.EnsureTicketIsEditable(ticket);

        var comment = new Comment
        {
            TicketId = ticket.Id,
            AuthorName = dto.AuthorName.Trim(),
            Body = dto.Body.Trim(),
            CreatedDate = DateTime.UtcNow
        };

        _context.Comments.Add(comment);

        ticket.LastModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var result = new CommentDto
        {
            Id = comment.Id,
            AuthorName = comment.AuthorName,
            Body = comment.Body,
            CreatedDate = comment.CreatedDate
        };

        return Ok(result);
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            return NotFound(new
            {
                code = "TICKET_NOT_FOUND",
                message = $"Ticket with id {id} was not found."
            });
        }

        _ticketRules.EnsureTicketIsEditable(ticket);

        _context.Tickets.Remove(ticket);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}