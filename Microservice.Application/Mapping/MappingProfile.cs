using AutoMapper;
using Microservice.Application.DTOs.Dapper;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Features.ExamplesEF.Commands.CreateExample;
using Microservice.Domain.Entities;

namespace Microservice.Application.Mapping;
public class MappingProfile : Profile
{
    public MappingProfile()
    {

        CreateMap<CreateExampleCommand, Example>()
            .ConstructUsing(src => new Example(src.Name, src.Description));

        CreateMap<ExampleItem, GetExampleItemDto>();
        CreateMap<Example, GetExampleWithItemsDto>();
        CreateMap<Example, GetExampleByIdDto>();
        CreateMap<Example, GetExampleByPredicateDto>();
        CreateMap<Example, DTOs.EF.GetAllExamplesDto>();
        CreateMap<Example, GetExamplesFromSqlDto>();
        CreateMap<Example, DTOs.EF.GetExamplesPaginatedDto>();
        CreateMap<Example, GetExamplesWithProjectionDto>();
        CreateMap<Example, GetExampleWithProjectionDto>();
        CreateMap<Example, ExecuteSqlWithResultDto>();
        CreateMap<Example, DTOs.Dapper.GetAllExamplesDto>();
        CreateMap<Example, GetExampleByPublicIdDto>();
        CreateMap<Example, DTOs.Dapper.GetExamplesPaginatedDto>();
        CreateMap<Example, SearchExamplesByNameDto>();

    }
}
