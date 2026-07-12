using FluentValidation;
using TransactionApproval.Application.DTOs;

namespace TransactionApproval.Application.Validators;

public class SimulateTransactionRequestValidator : AbstractValidator<SimulateTransactionRequest>
{
    public SimulateTransactionRequestValidator()
    {
        RuleFor(x => x.RegionCode)
            .NotEmpty().WithMessage("A region must be selected.")
            .MaximumLength(16);

        RuleFor(x => x.SubmittedAt)
            .NotEqual(default(DateTimeOffset)).WithMessage("A valid submission time is required.");
    }
}
