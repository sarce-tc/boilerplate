using AutoMapper;
using Microservice.Application.DTOs;
using Microservice.Application.Features.Examples.Commands.CreateExample;
using Microservice.Domain.Entities;

namespace Microservice.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<CreateExampleCommand, Example>()
                .ConstructUsing(src => new Example(src.Name, src.Description));

            CreateMap<Example, GetExampleByIdDto>();
            CreateMap<Example, GetExampleByPredicateDto>();
            CreateMap<Example, GetAllExamplesDto>();
            CreateMap<Example, GetExamplesFromSqlDto>();
            CreateMap<Example, GetExamplesPaginatedDto>();
            CreateMap<Example, GetExamplesWithProjectionDto>();
            CreateMap<Example, GetExampleWithProjectionDto>();
            CreateMap<Example, ExecuteSqlWithResultDto>();

            CreateMap<Product, GetActiveProductsDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

            CreateMap<Product, GetAllProductsDto>();
            CreateMap<Product, GetProductByIdDto>();
        }
    }
}
