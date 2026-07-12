using TransactionApproval.Application.DTOs;

namespace TransactionApproval.Application.Services;

/// <summary>Exposes the fixed catalog of supported regions.</summary>
public interface IRegionService
{
    Task<IReadOnlyList<RegionDto>> GetRegionsAsync(CancellationToken cancellationToken);
}
