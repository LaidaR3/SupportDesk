using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Api.DTOs;

public class AddCommentDto
{
    [Required]
    [MaxLength(150)]
    public string AuthorName { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;
}