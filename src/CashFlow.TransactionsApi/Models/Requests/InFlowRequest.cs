using System.ComponentModel.DataAnnotations;
using CashFlow.Application.Validators;

namespace CashFlow.TransactionsApi.Models.Requests
{
    public record InFlowRequest(
        [Required(ErrorMessage = "Por favor, informe o identificador da empresa")]
        [ValidateCompanyAccount]
        Guid CompanyAccountId,

        [Required(ErrorMessage = "Por favor, informe o valor do crédito")]
        [Range(0.01, 1000000.00, ErrorMessage = "O valor deve estar entre 0.01 e 1.000.000,00")]
        Decimal Amount,

        [Required(ErrorMessage = "Por favor, informe a descriçāo")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "A descriçāo deve ter entre entre 5 e 100 caracteres")]
        string Description);
}
