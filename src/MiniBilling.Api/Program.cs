using Azure.Identity;
using Azure.Messaging.ServiceBus;
using FluentValidation;
using FluentValidation.AspNetCore;
using MiniBilling.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Controladores
builder.Services.AddControllers();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Service Bus con Managed Identity
var fullyQualifiedNamespace = builder.Configuration["ServiceBus:FullyQualifiedNamespace"]!;
builder.Services.AddSingleton(new ServiceBusClient(fullyQualifiedNamespace, new DefaultAzureCredential()));

var app = builder.Build();

// Middleware de Correlation ID
app.UseMiddleware<CorrelationIdMiddleware>();

// Swagger solo en Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

