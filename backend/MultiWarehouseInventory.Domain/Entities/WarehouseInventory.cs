using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Events;

namespace MultiWarehouseInventory.Domain.Entities
{
    public class WarehouseInventory : TenantEntity
    {
        public Guid WarehouseId { get; set; }
        public Guid ProductId { get; set; }
        public int OnHandStock { get; set; }
        public int ReservedStock { get; set; }
        public int AvailableStock => OnHandStock - ReservedStock;

        public int ReorderLevel { get; set; }
        public Guid? WarehouseZoneId { get; set; }
        public string? LocationInWarehouse { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public Warehouse Warehouse { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public WarehouseZone? WarehouseZone { get; set; }

        // Domain Methods (optional - can be used for validation)
        public void ReserveStock(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Số lượng giữ phải lớn hơn 0.");
            if (quantity > AvailableStock)
                throw new InsufficientStockException(WarehouseId, ProductId, quantity, AvailableStock);
            
            ReservedStock += quantity;
        }

        public void ReserveStock(int quantity, Guid orderId)
        {
            ReserveStock(quantity);
            
            // Emit domain event
            if (WarehouseZoneId.HasValue)
            {
                AddDomainEvent(new StockReservedEvent(orderId, ProductId, WarehouseId, quantity, WarehouseZoneId.Value));
            }
        }

        public void ReleaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Số lượng giải phóng phải lớn hơn 0.");
            if (quantity > ReservedStock)
                throw new DomainException("Không thể giải phóng nhiều hơn số lượng đang giữ.");
            
            ReservedStock -= quantity;
        }

        public void AddStock(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Số lượng thêm phải lớn hơn 0.");
            OnHandStock += quantity;
        }

        public void RemoveStock(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Số lượng giảm phải lớn hơn 0.");
            if (quantity > AvailableStock)
                throw new InsufficientStockException(WarehouseId, ProductId, quantity, AvailableStock);
            
            OnHandStock -= quantity;
        }

        public bool IsBelowReorderLevel => OnHandStock <= ReorderLevel;
        public bool IsOutOfStock => OnHandStock == 0;
    }
}
