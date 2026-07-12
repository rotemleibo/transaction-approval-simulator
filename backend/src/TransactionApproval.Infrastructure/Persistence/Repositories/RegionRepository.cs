using Microsoft.EntityFrameworkCore;
using TransactionApproval.Application.Abstractions;
using TransactionApproval.Domain.Entities;

namespace TransactionApproval.Infrastructure.Persistence.Repositories;

public class RegionRepository : IRegionRepository
{
    private readonly AppDbContext _db;

    public RegionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Region>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _db.Regions
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Region?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await _db.Regions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
    }
}
