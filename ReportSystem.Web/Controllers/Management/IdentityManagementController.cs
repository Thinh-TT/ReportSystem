using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Domain.Entities;
using ReportSystem.Infrastructure.Data;

namespace ReportSystem.Web.Controllers.Management;

[ApiController]
[Route("api/management")]
public sealed class IdentityManagementController : ControllerBase
{
    private readonly ReportSystemDbContext _dbContext;

    public IdentityManagementController(ReportSystemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .OrderBy(x => x.EmployeeCode)
            .ToListAsync(cancellationToken);

        return Ok(users.Select(MapUser));
    }

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return user is null ? NotFound() : Ok(MapUser(user));
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] UserUpsertRequest request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        var user = new User
        {
            Id = Guid.NewGuid(),
            EmployeeCode = request.EmployeeCode.Trim(),
            FullName = request.FullName.Trim(),
            Email = request.Email?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.Users.Add(user);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, MapUser(user));
    }

    [HttpPut("users/{id:guid}")]
    public async Task<IActionResult> UpdateUser(
        [FromRoute] Guid id,
        [FromBody] UserUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        user.EmployeeCode = request.EmployeeCode.Trim();
        user.FullName = request.FullName.Trim();
        user.Email = request.Email?.Trim();
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return Ok(MapUser(user));
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        _dbContext.Users.Remove(user);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return NoContent();
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await _dbContext.Roles
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return Ok(roles.Select(MapRole));
    }

    [HttpGet("roles/{id:int}")]
    public async Task<IActionResult> GetRole([FromRoute] int id, CancellationToken cancellationToken)
    {
        var role = await _dbContext.Roles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return role is null ? NotFound() : Ok(MapRole(role));
    }

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] RoleUpsertRequest request, CancellationToken cancellationToken)
    {
        var role = new Role
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim()
        };

        _dbContext.Roles.Add(role);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return CreatedAtAction(nameof(GetRole), new { id = role.Id }, MapRole(role));
    }

    [HttpPut("roles/{id:int}")]
    public async Task<IActionResult> UpdateRole(
        [FromRoute] int id,
        [FromBody] RoleUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var role = await _dbContext.Roles.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null)
        {
            return NotFound();
        }

        role.Code = request.Code.Trim();
        role.Name = request.Name.Trim();

        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return Ok(MapRole(role));
    }

    [HttpDelete("roles/{id:int}")]
    public async Task<IActionResult> DeleteRole([FromRoute] int id, CancellationToken cancellationToken)
    {
        var role = await _dbContext.Roles.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null)
        {
            return NotFound();
        }

        _dbContext.Roles.Remove(role);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return NoContent();
    }

    [HttpGet("user-roles")]
    public async Task<IActionResult> GetUserRoles(CancellationToken cancellationToken)
    {
        var userRoles = await _dbContext.UserRoles
            .AsNoTracking()
            .OrderBy(x => x.UserId)
            .ThenBy(x => x.RoleId)
            .ToListAsync(cancellationToken);

        return Ok(userRoles.Select(MapUserRole));
    }

    [HttpGet("user-roles/{userId:guid}/{roleId:int}")]
    public async Task<IActionResult> GetUserRole(
        [FromRoute] Guid userId,
        [FromRoute] int roleId,
        CancellationToken cancellationToken)
    {
        var userRole = await _dbContext.UserRoles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId, cancellationToken);

        return userRole is null ? NotFound() : Ok(MapUserRole(userRole));
    }

    [HttpPost("user-roles")]
    public async Task<IActionResult> CreateUserRole([FromBody] UserRoleCreateRequest request, CancellationToken cancellationToken)
    {
        var userRole = new UserRole
        {
            UserId = request.UserId,
            RoleId = request.RoleId
        };

        _dbContext.UserRoles.Add(userRole);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return CreatedAtAction(
            nameof(GetUserRole),
            new { userId = userRole.UserId, roleId = userRole.RoleId },
            MapUserRole(userRole));
    }

    [HttpPut("user-roles/{userId:guid}/{roleId:int}")]
    public async Task<IActionResult> UpdateUserRole(
        [FromRoute] Guid userId,
        [FromRoute] int roleId,
        [FromBody] UserRoleUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await _dbContext.UserRoles
            .SingleOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        if (request.UserId == userId && request.RoleId == roleId)
        {
            return Ok(MapUserRole(existing));
        }

        var targetExists = await _dbContext.UserRoles.AnyAsync(
            x => x.UserId == request.UserId && x.RoleId == request.RoleId,
            cancellationToken);
        if (targetExists)
        {
            return Conflict(new { message = "Target user-role mapping already exists." });
        }

        _dbContext.UserRoles.Remove(existing);
        _dbContext.UserRoles.Add(new UserRole
        {
            UserId = request.UserId,
            RoleId = request.RoleId
        });

        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return Ok(new UserRoleResponse(request.UserId, request.RoleId));
    }

    [HttpDelete("user-roles/{userId:guid}/{roleId:int}")]
    public async Task<IActionResult> DeleteUserRole(
        [FromRoute] Guid userId,
        [FromRoute] int roleId,
        CancellationToken cancellationToken)
    {
        var userRole = await _dbContext.UserRoles
            .SingleOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId, cancellationToken);
        if (userRole is null)
        {
            return NotFound();
        }

        _dbContext.UserRoles.Remove(userRole);
        var saveError = await TrySaveChangesAsync(cancellationToken);
        if (saveError is not null)
        {
            return saveError;
        }

        return NoContent();
    }

    private async Task<IActionResult?> TrySaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return null;
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            return Conflict(new { message = ex.InnerException?.Message ?? ex.Message });
        }
    }

    private static UserResponse MapUser(User user)
    {
        return new UserResponse(
            user.Id,
            user.EmployeeCode,
            user.FullName,
            user.Email,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt);
    }

    private static RoleResponse MapRole(Role role)
    {
        return new RoleResponse(role.Id, role.Code, role.Name);
    }

    private static UserRoleResponse MapUserRole(UserRole userRole)
    {
        return new UserRoleResponse(userRole.UserId, userRole.RoleId);
    }

    public sealed record UserResponse(
        Guid Id,
        string EmployeeCode,
        string FullName,
        string? Email,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public sealed class UserUpsertRequest
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public sealed record RoleResponse(int Id, string Code, string Name);

    public sealed class RoleUpsertRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public sealed record UserRoleResponse(Guid UserId, int RoleId);

    public sealed class UserRoleCreateRequest
    {
        public Guid UserId { get; set; }
        public int RoleId { get; set; }
    }

    public sealed class UserRoleUpdateRequest
    {
        public Guid UserId { get; set; }
        public int RoleId { get; set; }
    }
}
