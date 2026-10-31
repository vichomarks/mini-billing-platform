using FluentValidation;

namespace MiniBilling.Api.Models;

public class CreateChargeRequest
{
    public string CustomerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateChargeRequestValidator : AbstractValidator<CreateChargeRequest>
{
    public CreateChargeRequestValidator(IConfiguration config)
    {
        var maxAmount = config.GetValue<decimal>("Billing:MaxAmount", 1_000_000);

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId es requerido");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a 0")
            .LessThanOrEqualTo(maxAmount).WithMessage($"El monto no puede superar {maxAmount}");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3).WithMessage("Currency debe ser un código de 3 letras, ej: MXN, USD");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es requerida")
            .MaximumLength(200).WithMessage("La descripción no puede superar 200 caracteres");
    }
}