using Azure.Messaging.ServiceBus;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using MiniBilling.Worker.Models;
using System.Text.Json;

namespace MiniBilling.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sbNamespace = _configuration["ServiceBus:FullyQualifiedNamespace"];
        var queueName = _configuration["ServiceBus:QueueName"];
        var sqlConnection = _configuration["ConnectionStrings:SqlDb"];

        var credential = new DefaultAzureCredential();
        await using var client = new ServiceBusClient(sbNamespace, credential);

        var options = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
        };

        await using var processor = client.CreateProcessor(queueName, options);

        processor.ProcessMessageAsync += async args =>
        {
            var body = args.Message.Body.ToString();
            _logger.LogInformation("Mensaje recibido: {Body}", body);

            try
            {
                var charge = JsonSerializer.Deserialize<ChargeMessage>(body);
                if (charge == null) throw new InvalidOperationException("Mensaje inválido");

                await PersistChargeAsync(charge, sqlConnection!, stoppingToken);
                await args.CompleteMessageAsync(args.Message, stoppingToken);
                _logger.LogInformation("Cargo persistido: {ChargeId}", charge.ChargeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando mensaje");
                await args.AbandonMessageAsync(args.Message, cancellationToken: stoppingToken);
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception, "Error en el procesador de Service Bus");
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("Worker iniciado. Esperando mensajes...");

        await Task.Delay(Timeout.Infinite, stoppingToken);

        await processor.StopProcessingAsync();
    }

    private async Task PersistChargeAsync(ChargeMessage charge, string connectionString, CancellationToken ct)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            IF NOT EXISTS (SELECT 1 FROM Charges WHERE ChargeId = @ChargeId)
            BEGIN
                INSERT INTO Charges (ChargeId, CustomerId, Amount, Currency, Description, CreatedAt, ProcessedAt)
                VALUES (@ChargeId, @CustomerId, @Amount, @Currency, @Description, @CreatedAt, @ProcessedAt)
            END";

        cmd.Parameters.AddWithValue("@ChargeId", charge.ChargeId);
        cmd.Parameters.AddWithValue("@CustomerId", charge.CustomerId);
        cmd.Parameters.AddWithValue("@Amount", charge.Amount);
        cmd.Parameters.AddWithValue("@Currency", charge.Currency);
        cmd.Parameters.AddWithValue("@Description", charge.Description);
        cmd.Parameters.AddWithValue("@CreatedAt", charge.CreatedAt);
        cmd.Parameters.AddWithValue("@ProcessedAt", DateTime.UtcNow);

        await cmd.ExecuteNonQueryAsync(ct);
    }
}