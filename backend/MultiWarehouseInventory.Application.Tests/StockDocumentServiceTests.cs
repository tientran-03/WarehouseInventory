using Moq;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Services;
using MultiWarehouseInventory.Domain.Common;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Application.Tests;

public class StockDocumentServiceTests
{
    [Fact]
    public async Task CreateAsync_Inbound_AddsInventoryAndMovementInOneDocument()
    {
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var warehouse = new Warehouse { Id = warehouseId, TenantId = tenantId, Name = "Kho Hà Nội", IsActive = true };
        var product = new Product { Id = productId, TenantId = tenantId, Sku = "SKU-01", Name = "Sản phẩm A", Price = 20_000m, IsActive = true };
        var zone = new WarehouseZone { Id = zoneId, TenantId = tenantId, WarehouseId = warehouseId, Code = "A-01", Name = "Kệ A", Capacity = 100 };
        var documents = new Mock<IStockDocumentRepository>();
        var inventory = new Mock<IWarehouseInventoryRepository>();
        var movements = new Mock<IStockMovementRepository>();
        var transaction = new Mock<IUnitOfWorkTransaction>();
        var unitOfWork = new Mock<IUnitOfWork>();
        StockDocument? savedDocument = null;
        StockMovement? savedMovement = null;

        ConfigureRequiredDependencies(tenantId, warehouse, product, zone, documents, inventory, unitOfWork, transaction);
        documents.Setup(x => x.AddAsync(It.IsAny<StockDocument>(), It.IsAny<CancellationToken>()))
            .Callback<StockDocument, CancellationToken>((document, _) => savedDocument = document)
            .Returns(Task.CompletedTask);
        documents.Setup(x => x.GetByTenantAsync(
                tenantId, warehouseId, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                savedDocument!.Warehouse = warehouse;
                foreach (var line in savedDocument.Lines)
                {
                    line.Product = product;
                    line.WarehouseZone = zone;
                }
                return [savedDocument];
            });
        movements.Setup(x => x.AddAsync(It.IsAny<StockMovement>()))
            .Callback<StockMovement>(movement => savedMovement = movement)
            .Returns(Task.CompletedTask);

        var service = CreateService(documents, inventory, movements, unitOfWork, tenantId, warehouse, product, zone);
        var response = await service.CreateAsync(new CreateStockDocumentRequest(
            tenantId,
            warehouseId,
            StockDocumentTypes.Inbound,
            DateTime.UtcNow,
            "Nhà cung cấp A",
            "HD-01",
            null,
            [new CreateStockDocumentLineRequest(productId, zoneId, 5)]), Guid.NewGuid());

        Assert.StartsWith("PN-", response.DocumentCode);
        Assert.Equal(5, response.TotalQuantity);
        Assert.Equal(100_000m, response.TotalValue);
        Assert.Equal(5, savedMovement!.QuantityChanged);
        Assert.Equal(0, savedMovement.StockBefore);
        Assert.Equal(5, savedMovement.StockAfter);
        Assert.Equal(5, zone.UsedCapacity);
        inventory.Verify(x => x.AddAsync(It.Is<WarehouseInventory>(item => item.OnHandStock == 5 && item.WarehouseZoneId == zoneId)), Times.Once);
        transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Outbound_WhenAvailableStockIsInsufficient_RejectsDocument()
    {
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var warehouse = new Warehouse { Id = warehouseId, TenantId = tenantId, IsActive = true };
        var product = new Product { Id = productId, TenantId = tenantId, IsActive = true };
        var zone = new WarehouseZone { Id = zoneId, TenantId = tenantId, WarehouseId = warehouseId, Code = "A-01", Capacity = 100 };
        var documents = new Mock<IStockDocumentRepository>();
        var inventory = new Mock<IWarehouseInventoryRepository>();
        var movements = new Mock<IStockMovementRepository>();
        var transaction = new Mock<IUnitOfWorkTransaction>();
        var unitOfWork = new Mock<IUnitOfWork>();
        ConfigureRequiredDependencies(tenantId, warehouse, product, zone, documents, inventory, unitOfWork, transaction);
        inventory.Setup(x => x.GetByWarehouseProductAndZoneAsync(warehouseId, productId, tenantId, zoneId))
            .ReturnsAsync(new WarehouseInventory { OnHandStock = 4, ReservedStock = 2 });

        var service = CreateService(documents, inventory, movements, unitOfWork, tenantId, warehouse, product, zone);

        await Assert.ThrowsAsync<InsufficientStockException>(() => service.CreateAsync(new CreateStockDocumentRequest(
            tenantId,
            warehouseId,
            StockDocumentTypes.Outbound,
            null,
            null,
            null,
            null,
            [new CreateStockDocumentLineRequest(productId, zoneId, 3)]), Guid.NewGuid()));

        documents.Verify(x => x.AddAsync(It.IsAny<StockDocument>(), It.IsAny<CancellationToken>()), Times.Never);
        movements.Verify(x => x.AddAsync(It.IsAny<StockMovement>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static StockDocumentService CreateService(
        Mock<IStockDocumentRepository> documents,
        Mock<IWarehouseInventoryRepository> inventory,
        Mock<IStockMovementRepository> movements,
        Mock<IUnitOfWork> unitOfWork,
        Guid tenantId,
        Warehouse warehouse,
        Product product,
        WarehouseZone zone)
    {
        var tenants = new Mock<ITenantRepository>();
        tenants.Setup(x => x.GetByIdAsync(tenantId)).ReturnsAsync(new Tenant { Id = tenantId, IsActive = true });
        var warehouses = new Mock<IWarehouseRepository>();
        warehouses.Setup(x => x.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);
        var products = new Mock<IProductRepository>();
        products.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);
        var zones = new Mock<IWarehouseZoneRepository>();
        zones.Setup(x => x.GetByIdAsync(zone.Id)).ReturnsAsync(zone);

        return new StockDocumentService(
            documents.Object,
            tenants.Object,
            warehouses.Object,
            products.Object,
            zones.Object,
            inventory.Object,
            movements.Object,
            unitOfWork.Object);
    }

    private static void ConfigureRequiredDependencies(
        Guid tenantId,
        Warehouse warehouse,
        Product product,
        WarehouseZone zone,
        Mock<IStockDocumentRepository> documents,
        Mock<IWarehouseInventoryRepository> inventory,
        Mock<IUnitOfWork> unitOfWork,
        Mock<IUnitOfWorkTransaction> transaction)
    {
        inventory.Setup(x => x.GetOnHandStockByZoneAsync(zone.Id)).ReturnsAsync(0);
        inventory.Setup(x => x.GetByWarehouseProductAndZoneAsync(warehouse.Id, product.Id, tenantId, zone.Id))
            .ReturnsAsync((WarehouseInventory?)null);
        unitOfWork.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }
}
