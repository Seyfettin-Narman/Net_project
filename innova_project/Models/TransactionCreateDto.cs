namespace innova_project.Models
{
    public class TransactionCreateDto
    {
        public string UserEmail { get; set; }
        public string UserPassword { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }

    }
}
