using AutoMapper;
using Microservice.Application.DTOs;
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
        CreateMap<Example, GetAllExamplesDto>();
        CreateMap<Example, GetExamplesFromSqlDto>();
        CreateMap<Example, GetExamplesPaginatedDto>();
        CreateMap<Example, GetExamplesWithProjectionDto>();
        CreateMap<Example, GetExampleWithProjectionDto>();
        CreateMap<Example, ExecuteSqlWithResultDto>();
        CreateMap<Example, GetAllExamplesDapperDto>();
        CreateMap<Example, GetExampleByPublicIdDapperDto>();
        CreateMap<Example, GetExamplesPaginatedDapperDto>();
        CreateMap<Example, SearchExamplesByNameDapperDto>();

    }
}
