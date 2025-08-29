using System.ComponentModel.DataAnnotations;

namespace TodoApp.Web.Models;
public class TodoItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }

    public string? UserId { get; set; }
}
