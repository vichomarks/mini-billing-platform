using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using MiniBilling.Api.Models;
using System.Text.Json;

namespace MiniBilling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChargesController : ControllerBase
{
    private readonly ILogger<ChargesController> _logger;
    private readonly ServiceBusClient _serviceBusClient;
    private const string QueueName = "charges-queue";

    public ChargesController(ILogger<ChargesController> logger, ServiceBusClient serviceBusClient)
    {
        _logger = logger;
        _serviceBusClient = serviceBusClient;
    }

    [HttpPost]
    public async Task<ActionResult<CreateChargeResponse>> CreateCharge([FromBody] CreateChargeRequest request)
    {
        var correlationId = HttpContext.Items["X-Correlation-ID"]?.ToString() ?? string.Empty;

        _logger.LogInformation("Creando cobro para cliente {CustomerId}, monto {Amount} {Currency}. CorrelationId: {CorrelationId}",
            request.CustomerId, request.Amount, request.Currency, correlationId);

        var response = new CreateChargeResponse
        {
            ChargeId      = Guid.NewGuid().ToString(),
            CustomerId    = request.CustomerId,
            Amount        = request.Amount,
            Currency      = request.Currency,
            Status        = "pending",
            CreatedAt     = DateTime.UtcNow,
            CorrelationId = correlationId
        };

        // Publicar mensaje al Service Bus
        var sender = _serviceBusClient.CreateSender(QueueName);
        var messageBody = JsonSerializer.Serialize(response);
        var message = new ServiceBusMessage(messageBody)
        {
            MessageId       = response.ChargeId,
            CorrelationId   = correlationId
        };

        await sender.SendMessageAsync(message);

        _logger.LogInformation("Mensaje publicado al queue {Queue}. ChargeId: {ChargeId}",
            QueueName, response.ChargeId);

        return CreatedAtAction(nameof(GetCharge), new { id = response.ChargeId }, response);
    }

    [HttpGet("{id}")]
    public ActionResult GetCharge(string id)
    {
        // Placeholder — en el Día 3 conectamos esto a Azure SQL
        return Ok(new { message = $"Charge {id} — persistencia disponible en Día 3" });
    }
}