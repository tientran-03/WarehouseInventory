using FluentAssertions;
using AutoMapper;
using Moq;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Common;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Interfaces;
using Xunit;

namespace MultiWarehouseInventory.Application.Tests;

public class StockDocumentServiceTests
{
    private readonly Mock<IStockDocumentRepository> _documentRepositoryMock;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IWarehouseZoneRepository> _zoneRepositoryMock;
    private readonly Mock<IWarehouseInventoryRepository> _inventoryRepositoryMock;
    private readonly Mock<IStockMovementRepository> _movementRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly StockDocumentService _stockDocumentService;

    public StockDocumentServiceTests()
    {
        _documentRepositoryMock = new Mock<IStockDocumentRepository>();
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _zoneRepositoryMock = new Mock<IWarehouseZoneRepository>();
        _inventoryRepositoryMock = new Mock<IWarehouseInventoryRepository>();
        _movementRepositoryMock = new Mock<IStockMovementRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        
        _stockDocumentService = new StockDocumentService(
            _documentRepositoryMock.Object,
            _tenantRepositoryMock.Object,
            _warehouseRepositoryMock.Object,
            _productRepositoryMock.Object,
            _zoneRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _movementRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnStockDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new StockDocument
        {
            Id = documentId,
            Type = "Inbound",
            TenantId = Guid.NewGuid(),
            WarehouseId = Guid.NewGuid(),
            DocumentDate = DateTime.UtcNow
        };

        _documentRepositoryMock
            .Setup(x => x.GetByIdAsync(documentId))
            .ReturnsAsync(document);

        // Act
        var result = await _stockDocumentService.GetByIdAsync(documentId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(documentId);
        result.Type.Should().Be("Inbound");
    }

    [Fact]
    public async Task GetByTenantAsync_WithValidTenantId_ShouldReturnDocuments()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var documents = new List<StockDocument>
        {
            new StockDocument { Id = Guid.NewGuid(), TenantId = tenantId, Type = "Inbound" },
            new StockDocument { Id = Guid.NewGuid(), TenantId = tenantId, Type = "Outbound" }
        };

        _documentRepositoryMock
            .Setup(x => x.GetByTenantAsync(tenantId))
            .ReturnsAsync(documents);

        // Act
        var result = await _stockDocumentService.GetByTenantAsync(tenantId);

        // Assert
        result.Should().HaveCount(2);
        result.All(d => d.TenantId == tenantId).Should().BeTrue();
    }
}
