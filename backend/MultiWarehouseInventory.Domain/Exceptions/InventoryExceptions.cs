using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouseInventory.Domain.Exceptions
{
    public class InsufficientStockException : BaseException
    {
        public Guid WarehouseId { get; }
        public Guid ProductId { get; }
        public int RequestedQuantity { get; }
        public int AvailableQuantity { get; }

        public InsufficientStockException(Guid warehouseId, Guid productId, int requestedQuantity, int availableQuantity)
            : base($"Kho '{warehouseId}' không đủ hàng cho sản phẩm '{productId}'. Yêu cầu: {requestedQuantity}, Hiện có khả dụng: {availableQuantity}")
        {
            WarehouseId = warehouseId;
            ProductId = productId;
            RequestedQuantity = requestedQuantity;
            AvailableQuantity = availableQuantity;
        }
    }
    public class WarehouseInventoryNotFoundException : BaseException
    {
        public WarehouseInventoryNotFoundException(Guid warehouseId, Guid productId)
            : base($"Không tìm thấy bản ghi tồn kho cho sản phẩm '{productId}' tại kho '{warehouseId}'.")
        {
        }
    }
}
