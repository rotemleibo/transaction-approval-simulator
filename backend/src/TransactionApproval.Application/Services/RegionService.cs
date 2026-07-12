using TransactionApproval.Application.Abstractions;
using TransactionApproval.Application.DTOs;

namespace TransactionApproval.Application.Services;

public class RegionService : IRegionService
{
    private readonly IRegionRepository _regions;

    public RegionService(IRegionRepository regions)
    {
        _regions = regions;
    }

    public async Task<IReadOnlyList<RegionDto>> GetRegionsAsync(CancellationToken cancellationToken)
    {
        var regions = await _regions.GetAllAsync(cancellationToken);
        return regions
            .Select(r => new RegionDto(r.Code, r.Name, r.TimeZoneId))
            .ToList();
    }
}
