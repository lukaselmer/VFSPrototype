using System;
using System.Security.Cryptography;
using System.Text;

namespace VFSBrowser.Helpers
{
    internal static class HashHelper
    {
        public static string GenerateHashCode(string input)
        {
            var shaM = new SHA512Managed();
            var result = shaM.ComputeHash(Encoding.Unicode.GetBytes(input));
            return Convert.ToBase64String(result);
        }
    }
}
