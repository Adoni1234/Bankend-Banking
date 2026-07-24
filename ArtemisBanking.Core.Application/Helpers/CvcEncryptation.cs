using System.Security.Cryptography;
using System.Text;

namespace ArtemisBanking.Core.Application.Helpers
{
    public static class CvcEncryptation
    {
        public static string ComputeSha256Hash(string cvc)
        {
            //Create a SHA256
            using SHA256 sha256Hash = SHA256.Create();
            //ComputeHash
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(cvc));

            //Convert byte array to a string
            StringBuilder sb = new();

            foreach (var item in bytes)
            {
                sb.Append(item.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
