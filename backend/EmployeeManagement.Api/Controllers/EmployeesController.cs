using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees(CancellationToken cancellationToken)
    {
        var employees = await _context.Employees
            .AsNoTracking()
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);

        return Ok(employees);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Employee>> GetEmployee(int id, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpPost]
    public async Task<ActionResult<Employee>> CreateEmployee(Employee employee, CancellationToken cancellationToken)
    {
        employee.Email = employee.Email.Trim();
        employee.FirstName = employee.FirstName.Trim();
        employee.LastName = employee.LastName.Trim();
        employee.Department = employee.Department.Trim();

        if (await _context.Employees.AnyAsync(e => e.Email.ToLower() == employee.Email.ToLower(), cancellationToken))
            return Conflict(new { message = "An employee with this email already exists." });

        _context.Employees.Add(employee);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict(new { message = "An employee with this email already exists." });
        }

        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, Employee employee, CancellationToken cancellationToken)
    {
        if (id != employee.Id) return BadRequest(new { message = "The employee ID does not match the route." });

        employee.Email = employee.Email.Trim();
        employee.FirstName = employee.FirstName.Trim();
        employee.LastName = employee.LastName.Trim();
        employee.Department = employee.Department.Trim();

        if (!await _context.Employees.AnyAsync(e => e.Id == id, cancellationToken))
            return NotFound();

        if (await _context.Employees.AnyAsync(e => e.Id != id && e.Email.ToLower() == employee.Email.ToLower(), cancellationToken))
            return Conflict(new { message = "An employee with this email already exists." });

        _context.Entry(employee).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict(new { message = "An employee with this email already exists." });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEmployee(int id, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FindAsync([id], cancellationToken);
        if (employee is null) return NotFound();

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
