using MediatR;
using RVPark.Features.Properties.DTOs;
using RVPark.Shared;

namespace RVPark.Features.Properties.Queries.GetPropertyById;

public record GetPropertyByIdQuery(int PropertyId) : IRequest<Result<PropertyDto>>;
