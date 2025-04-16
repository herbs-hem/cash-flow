using AutoMapper;
using CashFlow.Application.Dtos;
using CashFlow.Domain.Entities;
using OperationType = CashFlow.Application.Dtos.OperationType;

namespace CashFlow.Application.Mapping;

public class ReportProfile : Profile
{
    public ReportProfile()
    {
        CreateMap<ConsolidateDetails, ConsolidateDetailsDto>()
            .ReverseMap();
    }
}