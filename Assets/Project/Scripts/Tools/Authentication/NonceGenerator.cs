using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Project.Scripts.Tools.Authentication
{
    /// <summary>
    /// SHA-256 のノンスを生成するライブラリです。
    /// </summary>
    public partial class Main
    {
        private static string GenerateRandomString(int length)
        {
            if (length <= 0)
                throw new Exception("Expected nonce to have positive length");

            var uppercase = string.Concat(Enumerable.Range('A', 26).Select(c => (char) c));
            var lowercase = string.Concat(Enumerable.Range('a', 26).Select(c => (char) c));
            var number = string.Concat(Enumerable.Range(0, 10));
            var charset = uppercase + lowercase + number + "-._";

            var cryptographicallySecureRandomNumberGenerator = new RNGCryptoServiceProvider();
            var result = string.Empty;
            var remainingLength = length;

            var randomNumberHolder = new byte[1];
            while (remainingLength > 0)
            {
                var randomNumbers = new List<int>(16);
                for (var randomNumberCount = 0; randomNumberCount < 16; randomNumberCount++)
                {
                    cryptographicallySecureRandomNumberGenerator.GetBytes(randomNumberHolder);
                    randomNumbers.Add(randomNumberHolder[0]);
                }

                foreach (var randomNumber in randomNumbers.TakeWhile(randomNumber => remainingLength != 0)
                             .Where(randomNumber => randomNumber < charset.Length))
                {
                    result += charset[randomNumber];
                    remainingLength--;
                }
            }

            return result;
        }

        private static string GenerateSHA256NonceFromRawNonce(string rawNonce)
        {
            var sha = new SHA256Managed();
            var utf8RawNonce = Encoding.UTF8.GetBytes(rawNonce);
            var hash = sha.ComputeHash(utf8RawNonce);

            return hash.Aggregate(string.Empty, (current, b) => current + b.ToString("x2"));
        }
    }
}