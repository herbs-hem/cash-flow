using System.ComponentModel.DataAnnotations;

namespace CashFlow.Application.Validators
{
    public class ValidateInitialDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("A data não pode ser nula.");
            }
            if (value is DateTime date)
            {
                var today = DateTime.Today;
                var threeMonthsAgo = today.AddMonths(-3);

                if (date > today)
                {
                    return new ValidationResult("A data não pode ser futura.");
                }

                if (date < threeMonthsAgo)
                {
                    return new ValidationResult("A data não pode ser anterior a 3 meses atrás.");
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Data inválida.");
        }
    }
}