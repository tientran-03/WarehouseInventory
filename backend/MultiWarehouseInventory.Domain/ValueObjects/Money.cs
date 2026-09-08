using MultiWarehouseInventory.Domain.Exceptions;

namespace MultiWarehouseInventory.Domain.ValueObjects;
public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    private const decimal MaxAmount = 1_000_000_000; 

    public Money(decimal amount, string currency = "VND")
    {
        if (amount < 0)
            throw new DomainException("Số tiền không được âm.");
        if (amount > MaxAmount)
            throw new DomainException($"Số tiền không được vượt quá {MaxAmount}.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency không được để trống.");
            
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency = "VND") => new Money(0, currency);
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Không thể cộng tiền khác currency.");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Không thể trừ tiền khác currency.");
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal multiplier)
    {
        if (multiplier < 0)
            throw new DomainException("Multiplier không được âm.");
        return new Money(Amount * multiplier, Currency);
    }

    public override string ToString() => $"{Amount:N0} {Currency}";
    
    public static implicit operator decimal(Money money) => money.Amount;
    public static implicit operator Money(decimal amount) => new Money(amount);
}