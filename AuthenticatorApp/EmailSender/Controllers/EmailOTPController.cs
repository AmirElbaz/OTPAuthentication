
using AuthenticatorService;
using Microsoft.AspNetCore.Mvc;
using OTPService.Services;
using EmailSender.DTOs;
using OTPService;

namespace EmailSender.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailOTPController : ControllerBase
    {
        private readonly IOTPService _otpService;
        private readonly IAuthenticatorService _emailService;
        private readonly IConfiguration _configuration;

        public EmailOTPController(IOTPService otpService, IAuthenticatorService emailService, IConfiguration configuration)
        {
            _otpService = otpService;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpPost("validate-OTP")]
        public async Task<IActionResult> ValidateOTP([FromBody] ValidateOtpDTO request)
        {
            // Check if user is blocked
            var isBlocked = await _otpService.IsUserBlockedAsync(request.Contact, request.StepId);
            if (isBlocked)
            {
                var blockExpiry = await _otpService.GetBlockExpiryTimeAsync(request.Contact, request.StepId);
                return Unauthorized(new
                {
                    success = false,
                    message = "Your account is temporarily blocked due to too many failed attempts",
                    blocked = true,
                    blockExpiry = blockExpiry
                });
            }

            var result = await _otpService.ValidateOtpAsync(request.Contact, request.StepId, request.OTP);

            if (result)
            {
                string authUrl = _configuration["AuthURL"];
                string url = $"{authUrl}/{request.StepId}/{request.Decision}";
                return Ok(new {success=true, authenticationURL = url });
            }

            // Get the latest OTP record to provide detailed feedback
            var latestOtp = await _otpService.GetLatestOtpRecord(request.Contact, request.StepId);

            if (latestOtp == null)
                return Unauthorized(new { success = false, message = "No valid OTP found for this user and step" });

            if (latestOtp.IsRevoked && latestOtp.ValidationAttempts > 3)
            {
                var blockExpiry = await _otpService.GetBlockExpiryTimeAsync(request.Contact, request.StepId);
                return Unauthorized(new
                {
                    success = false,
                    message = "Maximum validation attempts reached. Your account is temporarily blocked.",
                    maxAttemptsReached = true,
                    blockExpiry = blockExpiry
                });
            }

            if (DateTime.UtcNow > latestOtp.Expiry)
                return Unauthorized(new { success = false, message = "OTP has expired", expired = true });

            return Unauthorized(new
            {
                success = false,
                message = "Invalid OTP",
                attemptsLeft = 3 - latestOtp.ValidationAttempts
            });
        }

        [HttpPost("Generate")]
        public async Task<IActionResult> GenerateOTP([FromBody] GenerateOtpDTO request)
        {
            // Check if user is blocked
            var isBlocked = await _otpService.IsUserBlockedAsync(request.Contact, request.StepId);
            if (isBlocked)
            {
                var blockExpiry = await _otpService.GetBlockExpiryTimeAsync(request.Contact, request.StepId);
                return BadRequest(new
                {
                    success = false,
                    message = "Your account is temporarily blocked due to too many failed attempts",
                    blocked = true,
                    blockExpiry = blockExpiry
                });
            }

            var canGenerate = await _otpService.CanGenerateOtp(request.Contact, request.StepId);

            if (!canGenerate)
            {
                // If user reached max generations, they'll be blocked and we should return the block expiry
                var blockExpiry = await _otpService.GetBlockExpiryTimeAsync(request.Contact, request.StepId);
                return BadRequest(new
                {
                    success = false,
                    message = "Maximum OTP generation limit reached for this step",
                    blocked = true,
                    blockExpiry = blockExpiry
                });
            }

            var (result, errorMessage, otp) = await _otpService.GenerateOtpAsync(request.StepId, request.Contact);
            if (!result)
                return BadRequest(new { success = false, message = errorMessage });

            var (emailResult, url) = await _emailService.Authenticate(request.StepId, request.Contact);
            if (emailResult)
                return Ok(new { success = true, message = "OTP generated successfully" });

            return BadRequest(new { success = false, message = "Failed to send OTP" });
        }

        [HttpGet("RemainingGenerations/{contact}/{stepId}")]
        public async Task<IActionResult> GetRemainingGenerations(string contact, int stepId)
        {
            // Check if user is blocked
            var isBlocked = await _otpService.IsUserBlockedAsync(contact, stepId);
            if (isBlocked)
            {
                var blockExpiry = await _otpService.GetBlockExpiryTimeAsync(contact, stepId);
                return Ok(new
                {
                    remainingGenerations = 0,
                    blocked = true,
                    blockExpiry = blockExpiry
                });
            }

            var remainingGenerations = await _otpService.GetRemainingOtpGenerations(contact, stepId);
            return Ok(new { remainingGenerations, blocked = false });
        }

        [HttpGet("BlockStatus/{contact}/{stepId}")]
        public async Task<IActionResult> GetBlockStatus(string contact, int stepId)
        {
            var isBlocked = await _otpService.IsUserBlockedAsync(contact, stepId);
            if (!isBlocked)
            {
                return Ok(new { blocked = false });
            }

            var blockExpiry = await _otpService.GetBlockExpiryTimeAsync(contact, stepId);
            return Ok(new
            {
                blocked = true,
                blockExpiry = blockExpiry,
                remainingBlockTime = blockExpiry.HasValue ?
                    (blockExpiry.Value - DateTime.UtcNow).TotalMinutes : 0
            });
        }
    }
}
