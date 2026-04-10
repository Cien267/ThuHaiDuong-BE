namespace ThuHaiDuong.Application.InterfaceService;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}