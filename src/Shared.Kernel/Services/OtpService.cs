using System.Security.Cryptography;
using Shared.Kernel.Interfaces;

namespace Shared.Kernel.Services;

public class OtpService : IOtpService
{
    public string GenerateOtp(int length = 6)
    {
        if (length <= 0) length = 6;
        
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        
        // Convert to positive int and get last X digits
        uint number = BitConverter.ToUInt32(bytes, 0);
        string otp = (number % (uint)Math.Pow(10, length)).ToString();
        
        // Pad with leading zeros if necessary
        return otp.PadLeft(length, '0');
    }
}
