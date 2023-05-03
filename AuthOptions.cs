using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace rn_tubeApi
{
    public class AuthOptions
    {
        public const string ISSUER = "rntubeapi"; // издатель токена
        public const string AUDIENCE = "rntubeclient"; // потребитель токена
        const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
        public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
}
