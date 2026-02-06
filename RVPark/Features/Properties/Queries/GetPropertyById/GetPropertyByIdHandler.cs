using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RVPark.Data;
using RVPark.Features.Properties.DTOs;
using RVPark.Shared;

namespace RVPark.Features.Properties.Queries.GetPropertyById;

public class GetPropertyByIdHandler : IRequestHandler<GetPropertyByIdQuery, Result<PropertyDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPropertyByIdHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PropertyDto>> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var property = await _context.bkg_Properties
            .FirstOrDefaultAsync(p => p.PropertyId == request.PropertyId, cancellationToken);

        if (property == null)
        {
            return Result<PropertyDto>.Failure($"Property with ID {request.PropertyId} not found.");
        }

        var dto = _mapper.Map<PropertyDto>(property);
        return Result<PropertyDto>.Success(dto);
    }
}
