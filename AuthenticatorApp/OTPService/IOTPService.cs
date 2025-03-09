using OTPService.Models;

namespace OTPService
{
    public interface IOTPService
    {

        
        Task<bool> ValidateOtpAsync(string contact, int stepId, string otp);
        Task<(bool Success, string ErrorMessage, string OTP)> GenerateOtpAsync(int stepId, string contact);
        Task<bool> CanGenerateOtp(string contact, int stepId);
        Task<int> GetRemainingOtpGenerations(string contact, int stepId);
        Task<OTPRecord> GetLatestOtpRecord(string contact, int stepId);
        Task<bool> IsUserBlockedAsync(string contact, int stepId);
        Task<DateTime?> GetBlockExpiryTimeAsync(string contact, int stepId);
    }
}
