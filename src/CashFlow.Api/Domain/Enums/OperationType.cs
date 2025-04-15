using System.ComponentModel.DataAnnotations;

namespace CashFlow.Api.Domain.Enums;

public enum OperationType
{
    All = 0,

    [Display(Name = "Inflow")]
    Inflow = 1,

    [Display(Name = "Outflow")]
    Outflow = 2
}
