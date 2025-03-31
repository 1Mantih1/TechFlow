using System;
using System.Security.Cryptography;
using System.Text;

namespace TechFlow.Classes
{
    class Verification
    {
        public static string GetSHA512Hash(string input)
        {
            SHA512CryptoServiceProvider SHA512Hasher = new SHA512CryptoServiceProvider();

            byte[] data = SHA512Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public static bool VerifySHA512Hash(string input, string hash)
        {
            string hashOfInput = GetSHA512Hash(input);

            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
