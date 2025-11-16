using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using UnivForm.Data;
using UnivForm.Models;


[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;

    public UserController(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
    }

    // GET: api/User
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers()
    {
        var users = await _userManager.Users
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.FirstName,
                u.LastName,
                u.CreatedAt,
                u.LastLogin,
                u.IsActive,
                Roles = _userManager.GetRolesAsync(u).Result
            })
            .ToListAsync();

        return Ok(users);
    }

    // GET: api/User/5
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetUser(int id)
    {
        var user = await _userManager.Users
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.FirstName,
                u.LastName,
                u.CreatedAt,
                u.LastLogin,
                u.IsActive,
                u.PhoneNumber,
                Roles = _userManager.GetRolesAsync(u).Result
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    // POST: api/User/Register
    [HttpPost("register")]
    public async Task<ActionResult<object>> Register(RegisterModel model)
    {
        var user = new AppUser
        {
            UserName = model.Username,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        // Rol atama
        if (!string.IsNullOrEmpty(model.Role))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!roleResult.Succeeded)
            {
                // Rol atanamazsa kullanıcıyı sil
                await _userManager.DeleteAsync(user);
                return BadRequest(roleResult.Errors);
            }
        }
        else
        {
            // Default rol atama
            await _userManager.AddToRoleAsync(user, "User");
        }

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.FirstName,
            user.LastName,
            Roles = await _userManager.GetRolesAsync(user)
        });
    }

    // POST: api/User/Login
    [HttpPost("login")]
    public async Task<ActionResult<object>> Login(LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Username) ??
                   await _userManager.FindByEmailAsync(model.Username);

        if (user == null)
        {
            return Unauthorized("Invalid username or password");
        }

        if (!user.IsActive)
        {
            return Unauthorized("Account is deactivated");
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

        if (!result.Succeeded)
        {
            return Unauthorized("Invalid username or password");
        }

        // Son giriş zamanını güncelle
        user.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.FirstName,
            user.LastName,
            user.LastLogin,
            Roles = await _userManager.GetRolesAsync(user)
        });
    }

    // POST: api/User/Logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok("Logged out successfully");
    }

    // PUT: api/User/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserModel model)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.IsActive = model.IsActive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return NoContent();
    }

    // POST: api/User/ChangePassword
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok("Password changed successfully");
    }

    // POST: api/User/AssignRole
    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole(AssignRoleModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId.ToString());
        if (user == null)
        {
            return NotFound("User not found");
        }

        var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
        if (!roleExists)
        {
            return NotFound("Role not found");
        }

        var result = await _userManager.AddToRoleAsync(user, model.RoleName);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok($"Role '{model.RoleName}' assigned successfully");
    }

    // DELETE: api/User/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return NoContent();
    }
}

// Model classes
public class RegisterModel
{
    [Required]
    public string Username { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    [Required]
    public string FirstName { get; set; } = "";

    [Required]
    public string LastName { get; set; } = "";

    public string? Role { get; set; }
}

public class LoginModel
{
    [Required]
    public string Username { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}

public class UpdateUserModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string FirstName { get; set; } = "";

    [Required]
    public string LastName { get; set; } = "";

    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ChangePasswordModel
{
    public int UserId { get; set; }
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}

public class AssignRoleModel
{
    public int UserId { get; set; }
    public string RoleName { get; set; } = "";
}