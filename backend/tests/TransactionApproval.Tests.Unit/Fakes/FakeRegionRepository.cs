using TransactionApproval.Application.Abstractions;
using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Tests.Unit.Fakes;

public sealed class FakeRegionRepository : IRegionRepository
{
    private readonly Region? _region;

    public FakeRegionRepository(Region? region = null) => _region = region;

    public Task<IReadOnlyList<Region>> GetAllAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<Region>>([]);

    public Task<Region?> GetByCodeAsync(string code, CancellationToken cancellationToken)
        => Task.FromResult(_region);
}
