using System;
using System.Security.Cryptography;
using System.Text;

namespace Healex.HL7v2Anonymizer.Services {

    public class HashGenerator {

        public static string HashString(string value) {

            var function = SHA512.Create();
            var hash = function.ComputeHash(Encoding.UTF8.GetBytes(value));
            var int32 = BitConverter.ToInt32(hash, 0);
            return $"{Math.Abs(int32)}";
        }
    }
}
