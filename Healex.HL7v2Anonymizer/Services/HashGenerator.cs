using System;
using System.Security.Cryptography;
using System.Text;

namespace Healex.HL7v2Anonymizer.Services
{
    public class HashGenerator
    {
        public static string HashString(string value)
        {
            var hasher = SHA512.Create();
            var hashedValue = hasher.ComputeHash(Encoding.UTF8.GetBytes(value));

            var hashAsInt = BitConverter.ToInt32(hashedValue, 0);
            var positiveHashedValue = Math.Abs(hashAsInt);
            return positiveHashedValue.ToString();
        }
    }
}
