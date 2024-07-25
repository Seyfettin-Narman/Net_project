using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using innova_project.Data;
using innova_project.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace innova_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly MasrafTakipContext _context;
        private readonly ILogger<UsersController> _logger;
        public UsersController(MasrafTakipContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            try
            {
                return await _context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting users.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the user.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
       // [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromBody] UserCreateDto userDto)
        {
            if (userDto == null)
            {
           
                Console.WriteLine("User data is required.");
                return BadRequest("User data is required.");
            }

            if (string.IsNullOrEmpty(userDto.Name) || string.IsNullOrEmpty(userDto.Email) || string.IsNullOrEmpty(userDto.Password))
            {
                
                Console.WriteLine("Name, Email, and Password are required.");
                return BadRequest("Name, Email, and Password are required.");
            }

            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                {
                    _logger.LogWarning("Email address is already in use.");
                    return BadRequest("Email address is already in use.");
                }

                var user = new User
                {
                    Name = userDto.Name,
                    Email = userDto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                    Role = userDto.Role // Yeni eklenen alan
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the user.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
         //[Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserUpdateDto userDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(userDto.Name))
                {
                    user.Name = userDto.Name;
                }
                if (!string.IsNullOrEmpty(userDto.Email))
                {
                    if (await _context.Users.AnyAsync(u => u.Email == userDto.Email && u.Id != id))
                    {
                        return BadRequest("Email is already in use.");
                    }
                    user.Email = userDto.Email;
                }
                if (!string.IsNullOrEmpty(userDto.Password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the user.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
       // [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the user.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}/total-expenses")]
        public async Task<ActionResult<decimal>> GetTotalExpenses(int id)
        {
            try
            {
                var totalExpenses = await _context.Transactions
                    .Where(t => t.UserId == id)
                    .SumAsync(t => t.Amount);

                return totalExpenses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting total expenses.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [HttpPut("setDailyExpenseLimit")]
        public async Task<IActionResult> SetDailyExpenseLimit(int userId, decimal limit)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                user.DailyExpenseLimit = limit;
                await _context.SaveChangesAsync();

                return Ok($"Daily expense limit set to {limit:C2} for user {user.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting daily expense limit.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        [HttpPut("setWeeklyExpenseLimit")]
        public async Task<IActionResult> SetWeeklyExpenseLimit(int userId, decimal limit)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                user.WeeklyExpenseLimit = limit;
                await _context.SaveChangesAsync();

                return Ok($"Weekly expense limit set to {limit:C2} for user {user.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting weekly expense limit.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        [HttpPut("setMonthlylyExpenseLimit")]
        public async Task<IActionResult> SetMonthlyExpenseLimit(int userId, decimal limit)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                user.MonthlyExpenseLimit = limit;
                await _context.SaveChangesAsync();

                return Ok($"Monthly expense limit set to {limit:C2} for user {user.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting monthly expense limit.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
