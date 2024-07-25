using System;
using System.ComponentModel.DataAnnotations;
namespace innova_project.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }

        public User? User { get; set; }
    }
}
