using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Application.Abstractions;

/// <summary>
/// Read access to the fixed catalog of supported regions.
/// </summary>
public interface IRegionRepository
{
    Task<IReadOnlyList<Region>> GetAllAsync(CancellationToken cancellationToken);

    Task<Region?> GetByCodeAsync(string code, CancellationToken cancellationToken);
}
