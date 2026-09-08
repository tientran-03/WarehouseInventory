using AutoMapper;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IStockDocumentRepository _stockDocumentRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IMapper _mapper;

    public InvoiceService(
        IInvoiceRepository invoiceRepository,
        IStockDocumentRepository stockDocumentRepository,
        ITenantRepository tenantRepository,
        IMapper mapper)
    {
        _invoiceRepository = invoiceRepository;
        _stockDocumentRepository = stockDocumentRepository;
        _tenantRepository = tenantRepository;
        _mapper = mapper;
    }

    public async Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request)
    {
        await EnsureTenantExistsAsync(request.TenantId);

        var targetDocument = await _stockDocumentRepository.GetByIdAsync(request.StockDocumentId);
        if (targetDocument == null)
        {
            throw new NotFoundException(nameof(StockDocument), request.StockDocumentId);
        }

        if (targetDocument.TenantId != request.TenantId)
        {
            throw new BadRequestException("Phiếu xuất kho không thuộc về tenant này.");
        }

        if (targetDocument.Type != "Outbound")
        {
            throw new BadRequestException("Chỉ có thể tạo hóa đơn cho phiếu xuất kho.");
        }

        var existingInvoice = await _invoiceRepository.GetByStockDocumentIdAsync(request.StockDocumentId);
        if (existingInvoice != null)
        {
            throw new BadRequestException("Phiếu xuất kho này đã có hóa đơn.");
        }

        var subtotal = targetDocument.Lines.Sum(l => l.Quantity * l.UnitPrice);
        var taxAmount = subtotal * request.TaxRate;
        var totalAmount = subtotal + taxAmount;

        var invoice = new Invoice
        {
            TenantId = request.TenantId,
            InvoiceCode = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}",
            StockDocumentId = request.StockDocumentId,
            CustomerName = request.CustomerName,
            CustomerAddress = request.CustomerAddress,
            CustomerPhone = request.CustomerPhone,
            CustomerTaxCode = request.CustomerTaxCode,
            Subtotal = subtotal,
            TaxRate = request.TaxRate,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            Notes = request.Notes,
            InvoiceDate = DateTime.UtcNow,
            Status = "Issued"
        };

        await _invoiceRepository.AddAsync(invoice);
        await _invoiceRepository.SaveChangesAsync();

        var saved = await _invoiceRepository.GetByIdAsync(invoice.Id);
        return Map(saved!);
    }

    public async Task<InvoiceResponse?> GetByIdAsync(Guid id)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id);
        return invoice == null ? null : Map(invoice);
    }

    public async Task<IEnumerable<InvoiceResponse>> GetByTenantAsync(Guid tenantId)
    {
        await EnsureTenantExistsAsync(tenantId);
        var invoices = await _invoiceRepository.GetByTenantAsync(tenantId);
        return invoices.Select(Map);
    }

    public async Task<InvoiceResponse?> GetByStockDocumentIdAsync(Guid stockDocumentId)
    {
        var invoice = await _invoiceRepository.GetByStockDocumentIdAsync(stockDocumentId);
        return invoice == null ? null : Map(invoice);
    }

    private async Task EnsureTenantExistsAsync(Guid tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant is null || !tenant.IsActive)
        {
            throw new NotFoundException(nameof(Tenant), tenantId);
        }
    }

    private static InvoiceResponse Map(Invoice invoice)
    {
        var lines = invoice.StockDocument.Lines.Select(l => new InvoiceLineResponse(
            l.ProductId,
            l.Product.Sku,
            l.Product.Name,
            l.WarehouseZone.Code,
            l.Quantity,
            l.UnitPrice,
            l.Quantity * l.UnitPrice
        )).ToList();

        return new InvoiceResponse(
            invoice.Id,
            invoice.TenantId,
            invoice.InvoiceCode,
            invoice.StockDocumentId,
            invoice.StockDocument.DocumentCode,
            invoice.CustomerName,
            invoice.CustomerAddress,
            invoice.CustomerPhone,
            invoice.CustomerTaxCode,
            invoice.Subtotal,
            invoice.TaxRate,
            invoice.TaxAmount,
            invoice.TotalAmount,
            invoice.Notes,
            invoice.InvoiceDate,
            invoice.Status,
            lines
        );
    }
}
