namespace MultiWarehouseInventory.Domain.Interfaces;

public interface ICompanyAccountEmailSender
{
    bool IsConfigured { get; }
    Task SendVerificationEmailAsync(string email, string username, string verificationToken);
}
