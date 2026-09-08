using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Infrastructure.Services;

public sealed class SmtpCompanyAccountEmailSender : ICompanyAccountEmailSender
{
    private readonly IConfiguration _configuration;

    public SmtpCompanyAccountEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_configuration["Email:SmtpHost"]) &&
        !string.IsNullOrWhiteSpace(_configuration["Email:FromAddress"]) &&
        !string.IsNullOrWhiteSpace(_configuration["Frontend:PublicUrl"]);

    public async Task SendVerificationEmailAsync(string email, string username, string verificationToken)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Dịch vụ email chưa được cấu hình. Thiết lập Email và Frontend:PublicUrl trước khi tạo tài khoản công ty.");
        }

        var publicUrl = _configuration["Frontend:PublicUrl"]!.TrimEnd('/');
        var verificationUrl = $"{publicUrl}/verify-email?token={Uri.EscapeDataString(verificationToken)}";
        var fromAddress = _configuration["Email:FromAddress"]!;
        var fromName = _configuration["Email:FromName"] ?? "Hệ thống kho hàng";
        var port = int.TryParse(_configuration["Email:Port"], out var configuredPort) ? configuredPort : 587;
        var useSsl = !bool.TryParse(_configuration["Email:UseSsl"], out var configuredSsl) || configuredSsl;

        using var message = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName),
            Subject = "Xác minh tài khoản công ty",
            Body = $"<p>Xin chào {WebUtility.HtmlEncode(username)},</p><p>Quản trị viên đã tạo tài khoản công ty cho bạn. Hãy xác minh địa chỉ email bằng liên kết dưới đây. Liên kết có hiệu lực trong 24 giờ.</p><p><a href=\"{WebUtility.HtmlEncode(verificationUrl)}\">Xác minh email</a></p><p>Sau khi xác minh và đăng nhập, bạn sẽ được yêu cầu đổi mật khẩu tạm thời.</p>",
            IsBodyHtml = true,
        };
        message.To.Add(email);

        using var client = new SmtpClient(_configuration["Email:SmtpHost"]!, port)
        {
            EnableSsl = useSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
        };

        var smtpUsername = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];
        if (!string.IsNullOrWhiteSpace(smtpUsername) && !string.IsNullOrWhiteSpace(password))
        {
            client.Credentials = new NetworkCredential(smtpUsername, password);
        }

        await client.SendMailAsync(message);
    }
}
