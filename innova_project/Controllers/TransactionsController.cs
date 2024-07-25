using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using innova_project.Data;
using innova_project.Models;
using innova_project.Models.innova_project.Models;
using innova_project.Jobs;
using Microsoft.AspNetCore.Authorization;

namespace innova_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly MasrafTakipContext _context;
        private readonly DailyExpenseJob _dailyExpenseJob;
        private readonly ILogger<UsersController> _logger;
        public TransactionsController(MasrafTakipContext context, DailyExpenseJob dailyExpenseJob, ILogger<UsersController> logger)
        {
            _context = context;
            _dailyExpenseJob = dailyExpenseJob;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactions()
        {
            try
            {
                return await _context.Transactions
                    .Include(t => t.User)
                    .Select(t => new TransactionDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        Amount = t.Amount,
                        Date = t.Date,
                        UserName = t.User.Name
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting transactions.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            try
            {
                var transaction = await _context.Transactions.FindAsync(id);

                if (transaction == null)
                {
                    return NotFound();
                }

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the transaction.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Transaction>> PostTransaction(TransactionCreateDto transactionDto)
        {
            try
            {
                var startOfDay = DateTime.Today;
                var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
                var startOfWeek = startOfDay.AddDays(-(int)startOfDay.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7).AddTicks(-1);
                var startOfMonth = new DateTime(startOfDay.Year, startOfDay.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == transactionDto.UserEmail);
                if (user == null)
                {
                    return BadRequest("Invalid user email.");
                }

                if (!BCrypt.Net.BCrypt.Verify(transactionDto.UserPassword, user.Password))
                {
                    return BadRequest("Invalid password.");
                }

                var transaction = new Transaction
                {
                    UserId = user.Id,
                    Amount = transactionDto.Amount,
                    Date = transactionDto.Date
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                var dailyTotal = await _context.Transactions
                    .Where(t => t.UserId == user.Id && t.Date >= startOfDay && t.Date <= endOfDay)
                    .SumAsync(t => t.Amount);

                var weeklyTotal = await _context.Transactions
                    .Where(t => t.UserId == user.Id && t.Date >= startOfWeek && t.Date <= endOfWeek)
                    .SumAsync(t => t.Amount);

                var monthlyTotal = await _context.Transactions
                    .Where(t => t.UserId == user.Id && t.Date >= startOfMonth && t.Date <= endOfMonth)
                    .SumAsync(t => t.Amount);

                if (monthlyTotal > user.MonthlyExpenseLimit)
                {
                    await _dailyExpenseJob.SendNotificationIfNeeded(user.Id, dailyTotal, weeklyTotal, monthlyTotal);
                }
                else if (weeklyTotal > user.WeeklyExpenseLimit)
                {
                    await _dailyExpenseJob.SendNotificationIfNeeded(user.Id, dailyTotal, weeklyTotal, monthlyTotal);
                }
                else if (dailyTotal > user.DailyExpenseLimit)
                {
                    await _dailyExpenseJob.SendNotificationIfNeeded(user.Id, dailyTotal, weeklyTotal, monthlyTotal);
                }

                return CreatedAtAction("GetTransaction", new { id = transaction.Id }, transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the transaction.");
                return StatusCode(500, "An error occurred while processing your request.");
            }

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaction(int id, TransactionUpdateDto transactionDto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == transactionDto.UserEmail);
                if (user == null)
                {
                    return BadRequest("Invalid user email.");
                }

                if (!BCrypt.Net.BCrypt.Verify(transactionDto.UserPassword, user.Password))
                {
                    return BadRequest("Invalid password.");
                }

                var transaction = await _context.Transactions.FindAsync(id);
                if (transaction == null)
                {
                    return NotFound();
                }

                if (transaction.UserId != user.Id)
                {
                    return Forbid();
                }

                transaction.Amount = transactionDto.Amount;
                transaction.Date = transactionDto.Date;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionExists(id))
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
                _logger.LogError(ex, "An error occurred while updating the transaction.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            try
            {
                var transaction = await _context.Transactions.FindAsync(id);
                if (transaction == null)
                {
                    return NotFound();
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the transaction.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        private bool TransactionExists(int id)
        {
            try
            {
                return _context.Transactions.Any(e => e.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking if the transaction exists.");
                throw;
            }
        }
    }
}
