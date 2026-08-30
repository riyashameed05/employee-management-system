using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Models;

public class Employee
{
    public int Id { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string Department { get; set; } = string.Empty;

    [Range(0, 9999999999.99)]
    public decimal Salary { get; set; }

    [DataType(DataType.Date)]
    public DateTime JoiningDate { get; set; }
}
