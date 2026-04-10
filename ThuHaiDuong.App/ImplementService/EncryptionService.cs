using Microsoft.AspNetCore.DataProtection;
using ThuHaiDuong.Application.InterfaceService;

namespace ThuHaiDuong.Application.ImplementService;

public class EncryptionService : IEncryptionService
{
    private readonly IDataProtector _protector;

    public EncryptionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("SmtpPasswordProtector");
    }

    public string Encrypt(string plainText) => _protector.Protect(plainText);
    public string Decrypt(string cipherText) => _protector.Unprotect(cipherText);
}