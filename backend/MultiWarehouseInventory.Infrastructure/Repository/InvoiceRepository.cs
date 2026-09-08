using Microsoft.EntityFrameworkCore;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Infrastructure.Repository;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _context;

    public InvoiceRepository(AppDbContext context) => _context = context;

    public Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
        => _context.Invoices.AddAsync(invoice, cancellationToken).AsTask();

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(x => x.StockDocument)
                .ThenInclude(x => x.Warehouse)
            .Include(x => x.StockDocument.Lines)
                .ThenInclude(x => x.Product)
            .Include(x => x.StockDocument.Lines)
                .ThenInclude(x => x.WarehouseZone)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Invoice>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .AsNoTracking()
            .Include(x => x.StockDocument)
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.InvoiceDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice?> GetByStockDocumentIdAsync(Guid stockDocumentId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .FirstOrDefaultAsync(x => x.StockDocumentId == stockDocumentId, cancellationToken);
    }

    public Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _context.Invoices.Update(invoice);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
