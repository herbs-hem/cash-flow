using System.ComponentModel.DataAnnotations;

namespace CashFlow.Application.Validators;

public class ValidateCompanyAccountAttribute : ValidationAttribute
{
    private readonly bool _allowEmpty;

    public ValidateCompanyAccountAttribute(bool allowEmpty = false)
    {
        _allowEmpty = allowEmpty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("Você precisa especificar a conta da empresa");
        }

        if ((value is not Guid guid) || (!_allowEmpty && guid == Guid.Empty))
        {
            return new ValidationResult("A conta da empresa deve ser um GUID válido");
        }

        return ValidationResult.Success;
    }
}