namespace innova_project.Models
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string UserName { get; set; }
    }
}
