namespace EmailSender.DTOs
{
    public class GenerateOtpDTO
    {
        public int UserId { get; set; }
        public int StepId { get; set; }
        public string Contact { get; set; }
    }
}
