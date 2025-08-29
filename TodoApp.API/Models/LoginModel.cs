using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TodoApp.API.Models;

public class LoginModel
{
    [DefaultValue("admin@todoapp.com")]
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [DefaultValue("Admin123!")]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
public class RegisterModel
{
    [Required]
    [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "First name can only contain letters and spaces")]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Required]
    [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Last name can only contain letters and spaces")]
    [MaxLength(50)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters and contain uppercase, lowercase, number, and special character")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
}