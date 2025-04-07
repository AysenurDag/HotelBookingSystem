using Microsoft.AspNetCore.Mvc;

namespace nigar_payment_service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    [HttpPost]
    public IActionResult ProcessPayment([FromBody] PaymentRequest request)
    {
 
        Console.WriteLine($"Ödeme alındı: RezervasyonId = {request.ReservationId}, Tutar = {request.Amount}");
        
        return Ok(new { status = "success", message = "Payment processed." });
    }
}

public class PaymentRequest
{
    public string ReservationId { get; set; }
    public decimal Amount { get; set; }
    public string UserId { get; set; }
}
