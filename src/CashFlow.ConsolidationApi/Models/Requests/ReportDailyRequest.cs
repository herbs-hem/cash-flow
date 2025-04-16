using System.ComponentModel.DataAnnotations;
using CashFlow.Application.Validators;

namespace CashFlow.ConsolidationApi.Models.Requests;

public record ReportDailyRequest(
    
    [Required(ErrorMessage = "Por favor, informe o identificador da empresa")]
    [ValidateCompanyAccount]
    Guid CompanyAccountId,
    
    [Required(ErrorMessage = "Por favor, informe a data inicial")]
    [ValidateInitialDate]
    DateTime InitialDate,
    
    [Required(ErrorMessage = "Por favor, informe a data final")]
    [ValidateEndDate]
    DateTime EndDate);