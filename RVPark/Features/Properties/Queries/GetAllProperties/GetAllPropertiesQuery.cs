using MediatR;
using RVPark.Features.Properties.DTOs;

namespace RVPark.Features.Properties.Queries.GetAllProperties;

public record GetAllPropertiesQuery(bool EnabledOnly = true) : IRequest<List<PropertyDto>>;
