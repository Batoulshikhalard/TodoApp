using System.ComponentModel.DataAnnotations;

namespace TodoApp.API.Models;

public class TodoItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [RegularExpression(@"^[a-zA-Z0-9\s\.\-_',!?]*$",
        ErrorMessage = "Title contains invalid characters")]
    public string Title { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    public bool IsCompleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }

    public string? UserId { get; set; }
    public virtual ApplicationUser? User { get; set; }
}
