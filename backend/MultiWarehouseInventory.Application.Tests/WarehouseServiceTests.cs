using FluentAssertions;
using AutoMapper;
using Moq;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;
using Xunit;

namespace MultiWarehouseInventory.Application.Tests;

public class WarehouseServiceTests
{
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly Mock<IWarehouseCacheService> _warehouseCacheServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly WarehouseService _warehouseService;

    public WarehouseServiceTests()
    {
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _warehouseCacheServiceMock = new Mock<IWarehouseCacheService>();
        _mapperMock = new Mock<IMapper>();
        _warehouseService = new WarehouseService(_warehouseRepositoryMock.Object, _warehouseCacheServiceMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnWarehouse()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var warehouse = new Warehouse
        {
            Id = warehouseId,
            Name = "Test Warehouse",
            Code = "WH001",
            TenantId = Guid.NewGuid(),
            Latitude = 21.0285,
            Longitude = 105.8542,
            IsActive = true
        };

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId))
            .ReturnsAsync(warehouse);

        // Act
        var result = await _warehouseService.GetByIdAsync(warehouseId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(warehouseId);
        result.Name.Should().Be("Test Warehouse");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId))
            .ReturnsAsync((Warehouse?)null);

        // Act
        var result = await _warehouseService.GetByIdAsync(warehouseId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCreateWarehouse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var request = new UpsertWarehouseRequest(
            tenantId,
            "New Warehouse",
            "WH002",
            21.0285,
            105.8542,
            1000,
            true
        );

        _warehouseRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Warehouse>()))
            .Returns(Task.CompletedTask);
        _warehouseRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _warehouseService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Warehouse");
        result.Code.Should().Be("WH002");
        _warehouseRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Warehouse>()), Times.Once);
        _warehouseRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ShouldUpdateWarehouse()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var existingWarehouse = new Warehouse
        {
            Id = warehouseId,
            Name = "Old Name",
            Code = "WH001",
            TenantId = Guid.NewGuid(),
            Latitude = 21.0285,
            Longitude = 105.8542,
            Capacity = 500,
            IsActive = true
        };

        var request = new UpsertWarehouseRequest(
            existingWarehouse.TenantId,
            "Updated Name",
            "WH001",
            21.0300,
            105.8600,
            1000,
            true
        );

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId))
            .ReturnsAsync(existingWarehouse);
        _warehouseRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Warehouse>()))
            .Returns(Task.CompletedTask);
        _warehouseRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _warehouseService.UpdateAsync(warehouseId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Capacity.Should().Be(1000);
        _warehouseRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Warehouse>()), Times.Once);
        _warehouseRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentWarehouse_ShouldThrowNotFoundException()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var request = new UpsertWarehouseRequest(
            Guid.NewGuid(),
            "Updated Name",
            "WH001",
            21.0300,
            105.8600,
            1000,
            true
        );

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId))
            .ReturnsAsync((Warehouse?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _warehouseService.UpdateAsync(warehouseId, request));
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteWarehouse()
    {
        // Arrange
        var warehouseId = Guid.NewGuid();
        var warehouse = new Warehouse
        {
            Id = warehouseId,
            Name = "Test Warehouse",
            Code = "WH001",
            TenantId = Guid.NewGuid(),
            IsActive = true
        };

        _warehouseRepositoryMock
            .Setup(x => x.GetByIdAsync(warehouseId))
            .ReturnsAsync(warehouse);
        _warehouseRepositoryMock
            .Setup(x => x.DeleteAsync(warehouse))
            .Returns(Task.CompletedTask);
        _warehouseRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _warehouseService.DeleteAsync(warehouseId);

        // Assert
        _warehouseRepositoryMock.Verify(x => x.DeleteAsync(warehouse), Times.Once);
        _warehouseRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
