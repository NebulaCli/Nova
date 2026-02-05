using System;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;

namespace NovaAPI.Security
{
    public static class HardwareId
    {
        /// <summary>
        /// Generates a unique identifier for the current machine
        /// Used by Nebula Client for licensing and stats tracking
        /// </summary>
        public static string Generate()
        {
            var msi = Environment.MachineName + 
                      Environment.ProcessorCount + 
                      Environment.UserName + 
                      Environment.OSVersion +
                      RuntimeInformation.OSArchitecture;

            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(msi));
                return BitConverter.ToString(bytes).Replace("-", "").Substring(0, 24);
            }
        }
    }
}
