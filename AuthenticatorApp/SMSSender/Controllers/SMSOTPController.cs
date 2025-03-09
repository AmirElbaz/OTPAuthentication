using AuthenticatorService;
using EmailSender.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OTPService;
using SMSSender.Services;
using System.Net.Http;

namespace SMSSender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SMSOTPController : ControllerBase
    {
        private readonly IOTPService _otpService;
        private readonly IAuthenticatorService _smsService;
        private readonly HttpClient _httpClient;

        public SMSOTPController(IOTPService otpService, IAuthenticatorService smsService,HttpClient httpClient)
        {
            _otpService = otpService;
            _smsService = smsService;
            _httpClient = httpClient;
        }

        [HttpPost("validate-OTP")]
        public async Task<IActionResult> ValidateOTP([FromBody] ValidateOtpDTO request)
        {
            
            var result = await _otpService.ValidateOtpAsync(request.Contact, request.StepId, request.OTP);

            if (result)
            {
                string apiUrl = "http://localhost:5214/api/StepAuthenticator";
                HttpResponseMessage response = await _httpClient.PostAsync($"{apiUrl}/{request.StepId}/{request.Decision}", null);

                return Ok(new { success = true, message = response.IsSuccessStatusCode });

            }

            // Get the latest OTP record to provide detailed feedback
            var latestOtp = await _otpService.GetLatestOtpRecord(request.Contact, request.StepId);

            if (latestOtp == null)
                return Unauthorized(new { success = false, message = "No valid OTP found for this user and step" });

            if (latestOtp.IsRevoked && latestOtp.ValidationAttempts > 3)
                return Unauthorized(new { success = false, message = "Maximum validation attempts reached. OTP has been revoked.", maxAttemptsReached = true });

            if (System.DateTime.UtcNow > latestOtp.Expiry)
                return Unauthorized(new { success = false, message = "OTP has expired", expired = true });

            return Unauthorized(new { success = false, message = "Invalid OTP", attemptsLeft = 3 - latestOtp.ValidationAttempts });

        }
        [HttpPost("Generate")]
        public async Task<IActionResult> GenerateOTP([FromBody] GenerateOtpDTO request)
        {
            var canGenerate = await _otpService.CanGenerateOtp(request.Contact, request.StepId);

            if (!canGenerate)
                return BadRequest(new { success = false, message = "Maximum OTP generation limit reached for this step" });

            var success = await _smsService.Authenticate( request.StepId, request.Contact);
            if (success.success)
                return Ok(new { success = true, message = "OTP generated successfully" });

            return BadRequest();
        }
        [HttpGet("RemainingGenerations/{contact}/{stepId}")]
        public async Task<IActionResult> GetRemainingGenerations(string contact, int stepId)
        {
            var remainingGenerations = await _otpService.GetRemainingOtpGenerations(contact, stepId);
            return Ok(new { remainingGenerations });
        }
    }
}
