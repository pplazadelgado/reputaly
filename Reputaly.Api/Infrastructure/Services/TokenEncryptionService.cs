using Microsoft.AspNetCore.DataProtection;

namespace Reputaly.API.Infrastructure.Services
{
    public interface ITokenEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }

    public class TokenEncryptionService :ITokenEncryptionService
    {
        private readonly IDataProtector _protector;

        public TokenEncryptionService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("GoogleOAuthTokens");
        }

        public string Encrypt(string plainText) => _protector.Protect(plainText);
        public string Decrypt(string cipherText) => _protector.Unprotect(cipherText);
    }
}
