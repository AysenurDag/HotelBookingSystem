namespace nigar_payment_service.Models;

public class Payment
{
    public int Id { get; set; }
    public string CustomerId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
