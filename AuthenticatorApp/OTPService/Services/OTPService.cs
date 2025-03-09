using Microsoft.EntityFrameworkCore;
using OTPService.DataAccess;
using OTPService.Models;
using OTPService.Utilities;


namespace OTPService.Services
{
    public class OtpService : IOTPService
    {
        private readonly OTPContext _context;
        private readonly int _blockTimeoutMinutes;

        public OtpService(OTPContext context, int blockTimeoutMinutes = 30)
        {
            _context = context;
            _blockTimeoutMinutes = blockTimeoutMinutes;
        }

        private const int MaxOtpResends = 3;
        private const int MaxValidationAttempts = 3;

        public async Task<bool> ValidateOtpAsync(string contact, int stepId, string otp)
        {
            // Check if user is blocked
            var isBlocked = await IsUserBlockedAsync(contact, stepId);
            if (isBlocked)
            {
                return false;
            }

            // Get the latest OTP record for this user and step
            var otpRecord = await _context.records
                .Where(o => o.Contact == contact && o.StepId == stepId && !o.IsUsed && !o.IsRevoked)
                .OrderByDescending(o => o.GeneratedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
            {
                return false;
            }

            // Increment validation attempts
            otpRecord.ValidationAttempts++;

            // Check if max validation attempts reached
            if (otpRecord.ValidationAttempts > MaxValidationAttempts)
            {
                otpRecord.IsRevoked = true;

                // Add user block record
                await BlockUserAsync(contact, stepId);

                await _context.SaveChangesAsync();
                return false;
            }

            // Check if OTP is expired
            if (DateTime.UtcNow > otpRecord.Expiry)
            {
                await _context.SaveChangesAsync();
                return false;
            }

            // Check if OTP matches
            if (otpRecord.OTP == otp)
            {
                otpRecord.IsUsed = true;
                otpRecord.ValidatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }

            // OTP didn't match, save attempts and return false
            await _context.SaveChangesAsync();
            return false;
        }

        public async Task<(bool Success, string ErrorMessage, string OTP)> GenerateOtpAsync(int stepId, string contact)
        {
            // Check if user is blocked
            var isBlocked = await IsUserBlockedAsync(contact, stepId);
            if (isBlocked)
            {
                return (false, "User is temporarily blocked due to too many failed attempts", "");
            }

            // Check if user has reached max OTP generation limit for this step
            var otpCount = await _context.records
                .CountAsync(o => o.Contact == contact && o.StepId == stepId);

            if (otpCount >= MaxOtpResends)
            {
                // Add user block record
                await BlockUserAsync(contact, stepId);

                return (false, "Maximum OTP generation limit reached for this step", "");
            }

            // Invalidate any previous OTPs for this user and step
            var previousOtps = await _context.records
                .Where(o => o.Contact == contact && o.StepId == stepId && !o.IsUsed && !o.IsRevoked)
                .ToListAsync();

            foreach (var prevOtp in previousOtps)
            {
                prevOtp.IsRevoked = true;
            }

            // Generate a new OTP
            string OTP = RandomGenerator.GenerateRandomString(6);

            // Create new OTP record
            var otpRecord = new OTPRecord
            {
                StepId = stepId,
                OTP = OTP,
                Contact = contact,
                GeneratedAt = DateTime.UtcNow,
                Expiry = DateTime.UtcNow.AddMinutes(5), // 5-minute expiry
                ValidationAttempts = 0,
                IsUsed = false,
                IsRevoked = false
            };

            _context.records.Add(otpRecord);
            await _context.SaveChangesAsync();

       
            return (true, null, OTP);
        }

        public async Task<bool> CanGenerateOtp(string contact, int stepId)
        {
            // Check if user is blocked
            var isBlocked = await IsUserBlockedAsync(contact, stepId);
            if (isBlocked)
            {
                return false;
            }

            // Check if user has reached max OTP generation limit for this step
            var otpCount = await _context.records
                .CountAsync(o => o.Contact == contact && o.StepId == stepId);

            return otpCount < MaxOtpResends;
        }

        public async Task<int> GetRemainingOtpGenerations(string contact, int stepId)
        {
            // Check if user is blocked
            var isBlocked = await IsUserBlockedAsync(contact, stepId);
            if (isBlocked)
            {
                return 0;
            }

            var otpCount = await _context.records
                .CountAsync(o => o.Contact == contact && o.StepId == stepId);

            return Math.Max(0, MaxOtpResends - otpCount);
        }

        public async Task<OTPRecord> GetLatestOtpRecord(string contact, int stepId)
        {
            return await _context.records
                .Where(o => o.Contact == contact && o.StepId == stepId)
                .OrderByDescending(o => o.GeneratedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsUserBlockedAsync(string contact, int stepId)
        {
            // Get the latest block record for this user and step
            var blockRecord = await _context.UserBlocks
                .Where(b => b.Contact == contact && b.StepId == stepId)
                .OrderByDescending(b => b.BlockedAt)
                .FirstOrDefaultAsync();

            if (blockRecord == null)
            {
                return false;
            }

            // Check if block has expired
            var blockExpiry = blockRecord.BlockedAt.AddMinutes(_blockTimeoutMinutes);
            if (DateTime.UtcNow > blockExpiry)
            {
                // Block has expired, reset user's OTP records for a fresh start
                await ResetUserOtpRecordsAsync(contact, stepId);
                return false;
            }

            return true;
        }

        public async Task<DateTime?> GetBlockExpiryTimeAsync(string contact, int stepId)
        {
            var blockRecord = await _context.UserBlocks
                .Where(b => b.Contact == contact && b.StepId == stepId)
                .OrderByDescending(b => b.BlockedAt)
                .FirstOrDefaultAsync();

            if (blockRecord == null)
            {
                return null;
            }

            return blockRecord.BlockedAt.AddMinutes(_blockTimeoutMinutes);
        }

        private async Task BlockUserAsync(string contact, int stepId)
        {
            var userBlock = new UserBlock
            {
                Contact = contact,
                StepId = stepId,
                BlockedAt = DateTime.UtcNow
            };

            _context.UserBlocks.Add(userBlock);
            await _context.SaveChangesAsync();
        }

        private async Task ResetUserOtpRecordsAsync(string contact, int stepId)
        {
            // This method resets the user's OTP records count after a block expires
            // to give them a fresh start with 3 new attempts

          

            var oldRecords = await _context.records
                .Where(o => o.Contact == contact && o.StepId == stepId)
                .ToListAsync();

            foreach (var record in oldRecords)
            {
                record.IsExpiredBlock = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
