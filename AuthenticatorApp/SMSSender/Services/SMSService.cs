using AuthenticatorService;
using Microsoft.OpenApi.Services;
using OTPService;

namespace SMSSender.Services
{
    public class SMSService : IAuthenticatorService
    {
        private readonly IOTPService _otpService;
        HttpClient _httpClient;
        private const string apiUrl= "SMSserviceProvider/send-smss";
        public SMSService(IOTPService otpService, HttpClient httpClient)
        {
            _otpService = otpService;
            _httpClient = httpClient;
        }

        public async Task<(bool success, string URL)> Authenticate(int StepId, string Contact)
        {
            var (success, errorMessage, otp) = await _otpService.GenerateOtpAsync( StepId, Contact);

            if (!success)
            {
       
                
                return (false,null);
            }

            string message = $"Please use the one-time password (OTP) below to access the document:\n\n"
                            + $"OTP:{otp}\n\n"
                            + $"Note: This OTP is valid for a limited time of 5 minutes. Do not share it with anyone.";

                string url = $"{apiUrl}/{StepId}/{Contact}";


            return (true,url);
        }

     
    }
}
