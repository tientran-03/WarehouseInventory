using Microsoft.EntityFrameworkCore;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Infrastructure.Repository;

public class StockDocumentRepository : IStockDocumentRepository
{
    private readonly AppDbContext _context;

    public StockDocumentRepository(AppDbContext context) => _context = context;

    public Task AddAsync(StockDocument document, CancellationToken cancellationToken = default)
        => _context.StockDocuments.AddAsync(document, cancellationToken).AsTask();

    public async Task<StockDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StockDocuments
            .Include(x => x.Warehouse)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Product)
            .Include(x => x.Lines)
                .ThenInclude(x => x.WarehouseZone)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<StockDocument>> GetByTenantAsync(
        Guid tenantId,
        Guid? warehouseId = null,
        string? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.StockDocuments
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Product)
            .Include(x => x.Lines)
                .ThenInclude(x => x.WarehouseZone)
            .Where(x => x.TenantId == tenantId && x.Status == "Completed");

        if (warehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == warehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(x => x.Type == type);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.DocumentDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.DocumentDate <= toDate.Value);
        }

        return await query
            .OrderByDescending(x => x.DocumentDate)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
