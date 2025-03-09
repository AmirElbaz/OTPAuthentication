using AuthenticatorService;
using Azure.Core;
using EmailSender.InterFaces;
using OTPService;
using OTPService.Services;

namespace EmailSender.Services
{
    public class EmailService : IAuthenticatorService
    {
        private readonly IEmailSender _emailSenderService;
        private readonly IOTPService _otpService;

        public EmailService(IEmailSender emailSenderService, IOTPService otpService)
        {
            _emailSenderService = emailSenderService;
            _otpService = otpService;
        }

        public async Task<(bool success, string URL)> Authenticate(int StepId, string Contact)
        {
            var (success, errorMessage, otp) = await _otpService.GenerateOtpAsync( StepId, Contact);

            if (!success)
             return (false,null);
            

            string message = $"Please use the one-time password (OTP) below to access the document:\n\n"
                            + $"OTP:{otp}\n\n"
                            + $"Note: This OTP is valid for a limited time of 5 minutes. Do not share it with anyone.";

            await _emailSenderService.SendEmailAsync(Contact, "Document Authentication", message);
            var URL = $"http://localhost:4500/otp-validation/{StepId}/{Contact}";
            return (true, URL );
        }
    }
}
