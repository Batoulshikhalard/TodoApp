using System.ComponentModel.DataAnnotations;

namespace TodoApp.API.Dtos;

public class UserViewModel
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
}
public class UpdateUserModel
{
    [Required]
    public string Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    public bool IsActive { get; set; }

    public List<string> Roles { get; set; } = new List<string>();
}
public class ChangePasswordModel
{
    [Required]
    public string UserId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; }

    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
}