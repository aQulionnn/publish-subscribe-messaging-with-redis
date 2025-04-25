using System.Security.Cryptography;
using System.Text;

namespace Publisher.Utilities;

public static class MessageIdGenerator
{
    public static Guid Create(string payload)
    {
        using var sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
        byte[] bytes = new byte[16];
        Array.Copy(hash, 0, bytes, 0, 16);
        return new Guid(bytes);
    }
}