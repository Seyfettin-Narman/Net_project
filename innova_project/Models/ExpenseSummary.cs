using innova_project.Jobs;

namespace innova_project.Models
{
    public class ExpenseSummary
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public SummaryType SummaryType { get; set; }
    }
}
