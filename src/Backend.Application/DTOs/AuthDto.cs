
namespace Backend.Application.DTOs;

using System.ComponentModel.DataAnnotations;
public class AuthResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Role { get; set; } = 1;

    public string Email { get; set; } = string.Empty;
    public string? Token { get; set; }
}

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; } = 1;
    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}

public class ForgotPasswordDto
{
    [Required, EmailAddress]
    public string Email {get ; set;} = string.Empty;
}

public class VerifyOtpDto
{
    [Required, EmailAddress]
    public string Email {get ; set;} = string.Empty;
    [Required]
    public string Otp { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required, EmailAddress]
    public string Email {get ; set;} = string.Empty;
    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}


// private async Task SendOtpEmailAsync(string email, string otp)
// {
//     var host = _configuration["EmailSettings:Host"]
//         ?? throw new InvalidOperationException("SMTP host missing.");

//     var port = int.Parse(
//         _configuration["EmailSettings:Port"]
//         ?? throw new InvalidOperationException("SMTP port missing."));

//     var sender = _configuration["EmailSettings:Email"]
//         ?? throw new InvalidOperationException("Sender email missing.");

//     var password = _configuration["EmailSettings:AppPassword"]
//         ?? throw new InvalidOperationException("Email app password missing.");

//     using var message = new MailMessage(sender, email)
//     {
//         Subject = "Password Reset OTP",
//         Body = $"Your password reset OTP is {otp}. " +
//                "It expires in 5 minutes. Do not share it.",
//         IsBodyHtml = false
//     };

//     using var smtp = new SmtpClient(host, port)
//     {
//         EnableSsl = true,
//         Credentials = new NetworkCredential(sender, password)
//     };

//     await smtp.SendMailAsync(message);
// }
