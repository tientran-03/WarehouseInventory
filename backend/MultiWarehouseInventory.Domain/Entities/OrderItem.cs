using MultiWarehouseInventory.Domain.Exceptions;

namespace MultiWarehouseInventory.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value <= 0)
                    throw new DomainException("Số lượng phải lớn hơn 0.");
                _quantity = value;
            }
        }
        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (value < 0)
                    throw new DomainException("Đơn giá không được âm.");
                _unitPrice = value;
            }
        }
        public decimal TotalPrice => Quantity * UnitPrice;
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;

        // Domain Methods
        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new DomainException("Số lượng phải lớn hơn 0.");
            Quantity = newQuantity;
        }

        public void UpdateUnitPrice(decimal newUnitPrice)
        {
            if (newUnitPrice < 0)
                throw new DomainException("Đơn giá không được âm.");
            UnitPrice = newUnitPrice;
        }
    }
}