using System.ComponentModel.DataAnnotations;

namespace OTPService.Models
{
    public class UserBlock
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Contact { get; set; }

        [Required]
        public int StepId { get; set; }

        [Required]
        public DateTime BlockedAt { get; set; }
    }
}
