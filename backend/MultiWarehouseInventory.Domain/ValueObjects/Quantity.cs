using MultiWarehouseInventory.Domain.Exceptions;

namespace MultiWarehouseInventory.Domain.ValueObjects;

/// <summary>
/// Value Object đại diện cho số lượng với validation
/// </summary>
public class Quantity : IEquatable<Quantity>
{
    public int Value { get; }
    
    private const int MaxQuantity = 1_000_000; // 1 triệu items

    public Quantity(int value)
    {
        if (value < 0)
            throw new DomainException("Số lượng không được âm.");
        if (value > MaxQuantity)
            throw new DomainException($"Số lượng không được vượt quá {MaxQuantity}.");
            
        Value = value;
    }

    public static Quantity Zero => new Quantity(0);
    public static Quantity One => new Quantity(1);
    
    public Quantity Add(Quantity other)
    {
        return new Quantity(Value + other.Value);
    }

    public Quantity Subtract(Quantity other)
    {
        if (Value < other.Value)
            throw new DomainException("Không thể trừ nhiều hơn số lượng hiện có.");
        return new Quantity(Value - other.Value);
    }

    public Quantity Multiply(int multiplier)
    {
        if (multiplier < 0)
            throw new DomainException("Multiplier không được âm.");
        return new Quantity(Value * multiplier);
    }

    public bool IsZero => Value == 0;
    public bool IsPositive => Value > 0;
    public bool IsNegative => Value < 0;

    public override string ToString() => Value.ToString();
    
    public static implicit operator int(Quantity quantity) => quantity.Value;
    public static implicit operator Quantity(int value) => new Quantity(value);
    
    public static Quantity operator +(Quantity left, Quantity right) => left.Add(right);
    public static Quantity operator -(Quantity left, Quantity right) => left.Subtract(right);
    public static Quantity operator *(Quantity left, int right) => left.Multiply(right);
    
    public bool Equals(Quantity? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }
    
    public override bool Equals(object? obj)
    {
        return Equals(obj as Quantity);
    }
    
    public override int GetHashCode() => Value.GetHashCode();
    
    public static bool operator ==(Quantity? left, Quantity? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }
    
    public static bool operator !=(Quantity? left, Quantity? right)
    {
        return !(left == right);
    }
    
    public static bool operator <(Quantity left, Quantity right) => left.Value < right.Value;
    public static bool operator >(Quantity left, Quantity right) => left.Value > right.Value;
    public static bool operator <=(Quantity left, Quantity right) => left.Value <= right.Value;
    public static bool operator >=(Quantity left, Quantity right) => left.Value >= right.Value;
}