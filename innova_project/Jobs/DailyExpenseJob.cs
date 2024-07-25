using Microsoft.EntityFrameworkCore;
using innova_project.Data;
using System.Threading.Tasks;
using innova_project.Services;
using innova_project.Models;
namespace innova_project.Jobs
{
    public enum SummaryType
    {
        Daily,
        Weekly,
        Monthly
    }
    public class DailyExpenseJob
    {
        private readonly MasrafTakipContext _context;
        private readonly IEmailService _emailService;
        public DailyExpenseJob(MasrafTakipContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task Execute()
        {
            var today = DateTime.Today;
            var users = await _context.Users.ToListAsync();

            foreach (var user in users)
            {
                // Günlük harcamaları hesapla
                var dailyExpense = await CalculateDailyExpense(user.Id, today);
                await SaveDailySummary(user.Id, dailyExpense, today);
                var weeklyExpense = await CalculateWeeklyExpense(user.Id, today);
                var monthlyExpense = await CalculateMonthlyExpense(user.Id, today);
               
                if (today.DayOfWeek == DayOfWeek.Sunday)
                {
                    
                    await SaveWeeklySummary(user.Id, weeklyExpense, today);
                }

           
                if (today.Day == DateTime.DaysInMonth(today.Year, today.Month))
                {
                    
                    await SaveMonthlySummary(user.Id, monthlyExpense, today);
                }

               
                await SendNotificationIfNeeded(user.Id, dailyExpense, weeklyExpense, monthlyExpense);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<decimal> CalculateDailyExpense(int userId, DateTime date)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId && t.Date.Date == date)
                .SumAsync(t => t.Amount);
        }

        public async Task<decimal> CalculateWeeklyExpense(int userId, DateTime endDate)
        {
            var startDate = endDate.AddDays(-6);
            return await _context.Transactions
                .Where(t => t.UserId == userId && t.Date.Date >= startDate && t.Date.Date <= endDate)
                .SumAsync(t => t.Amount);
        }

        public async Task<decimal> CalculateMonthlyExpense(int userId, DateTime endDate)
        {
            var startDate = new DateTime(endDate.Year, endDate.Month, 1);
            return await _context.Transactions
                .Where(t => t.UserId == userId && t.Date.Date >= startDate && t.Date.Date <= endDate)
                .SumAsync(t => t.Amount);
        }

        public async Task SaveDailySummary(int userId, decimal amount, DateTime date)
        {
            var summary = new ExpenseSummary
            {
                UserId = userId,
                Amount = amount,
                Date = date,
                SummaryType = SummaryType.Daily
            };
            _context.ExpenseSummaries.Add(summary);
        }

        public async Task SaveWeeklySummary(int userId, decimal amount, DateTime endDate)
        {
            var summary = new ExpenseSummary
            {
                UserId = userId,
                Amount = amount,
                Date = endDate,
                SummaryType = SummaryType.Weekly
            };
            _context.ExpenseSummaries.Add(summary);
        }

        public async Task SaveMonthlySummary(int userId, decimal amount, DateTime endDate)
        {
            var summary = new ExpenseSummary
            {
                UserId = userId,
                Amount = amount,
                Date = endDate,
                SummaryType = SummaryType.Monthly
            };
            _context.ExpenseSummaries.Add(summary);
        }

    public async Task SendNotificationIfNeeded(int userId, decimal dailyExpense,decimal weeklyExpense , decimal monthlyExpense)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return; 
        }

            if(monthlyExpense > user.MonthlyExpenseLimit)
            {
                var subject = "Aylık Harcama Limiti Aşıldı";
                var body = $"Sayın {user.Name},<br><br>" +
                           $"bu ay harcamanız {monthlyExpense:C2} ile aylık limitiniz olan {user.MonthlyExpenseLimit:C2}'yi aşmıştır.<br>" +
                           "Lütfen harcamalarınızı gözden geçiriniz.<br><br>" +
                           "Saygılarımızla,<br>Masraf Takip Uygulaması";

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            else if(weeklyExpense > user.WeeklyExpenseLimit)
            {
                var subject = "Haftalık Harcama Limiti Aşıldı";
                var body = $"Sayın {user.Name},<br><br>" +
                           $"Bu haftaki harcamanız {weeklyExpense:C2} ile haftalık limitiniz olan {user.WeeklyExpenseLimit:C2}'yi aşmıştır.<br>" +
                           "Lütfen harcamalarınızı gözden geçiriniz.<br><br>" +
                           "Saygılarımızla,<br>Masraf Takip Uygulaması";

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            else if (dailyExpense > user.DailyExpenseLimit)
            {
                var subject = "Günlük Harcama Limiti Aşıldı";
                var body = $"Sayın {user.Name},<br><br>" +
                           $"Bugünkü harcamanız {dailyExpense:C2} ile günlük limitiniz olan {user.DailyExpenseLimit:C2}'yi aşmıştır.<br>" +
                           "Lütfen harcamalarınızı gözden geçiriniz.<br><br>" +
                           "Saygılarımızla,<br>Masraf Takip Uygulaması";

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }   

        }
    }
}
