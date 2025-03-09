using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTPService.Models
{
    public class OTPRecord
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OTP { get; set; }
        public DateTime Expiry {  get; set; }
        public string Contact {  get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime? ValidatedAt { get; set; }
        public int ValidationAttempts { get; set; } = 0;
        public bool IsUsed { get; set; } = false;
        public bool IsRevoked { get; set; } = false;
        public int StepId { get; set; }
        public bool IsExpiredBlock { get; set; }
    }
}
