using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Data;
using SupportDesk.Api.DTOs;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AgentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AgentDto>>> GetAgents(
        [FromQuery] string? search = null)
    {
        var query = _context.Agents.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();

            query = query.Where(a =>
                a.FullName.ToLower().Contains(term) ||
                a.Email.ToLower().Contains(term));
        }

        var agents = await query
            .OrderBy(a => a.FullName)
            .Select(a => new AgentDto
            {
                Id = a.Id,
                FullName = a.FullName,
                Email = a.Email,
                Department = a.Department,
                Active = a.Active
            })
            .ToListAsync();

        return Ok(agents);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AgentDto>> GetAgent(int id)
    {
        var agent = await _context.Agents
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AgentDto
            {
                Id = a.Id,
                FullName = a.FullName,
                Email = a.Email,
                Department = a.Department,
                Active = a.Active
            })
            .FirstOrDefaultAsync();

        if (agent == null)
        {
            return NotFound(new
            {
                code = "AGENT_NOT_FOUND",
                message = $"Agent with id {id} was not found."
            });
        }

        return Ok(agent);
    }
}