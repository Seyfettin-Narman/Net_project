using System.Collections.Generic;
namespace innova_project.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public decimal DailyExpenseLimit { get; set; } = 1000;
        public decimal WeeklyExpenseLimit { get; set; } = 100000;

        public decimal MonthlyExpenseLimit { get; set; } = 1000000;
        public string Role { get; set; }  = "User";
        public ICollection<Transaction> Transactions { get; set; }

        public User()
        {
            Transactions = new List<Transaction>();
        }
    }
}
