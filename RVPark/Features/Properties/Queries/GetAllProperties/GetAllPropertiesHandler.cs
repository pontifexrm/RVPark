using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RVPark.Data;
using RVPark.Features.Properties.DTOs;

namespace RVPark.Features.Properties.Queries.GetAllProperties;

public class GetAllPropertiesHandler : IRequestHandler<GetAllPropertiesQuery, List<PropertyDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllPropertiesHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<PropertyDto>> Handle(GetAllPropertiesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.bkg_Properties.AsQueryable();

        if (request.EnabledOnly)
        {
            query = query.Where(p => p.Enabled);
        }

        var properties = await query.OrderBy(p => p.Name).ToListAsync(cancellationToken);
        return _mapper.Map<List<PropertyDto>>(properties);
    }
}
