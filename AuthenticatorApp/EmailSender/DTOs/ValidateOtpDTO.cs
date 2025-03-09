namespace EmailSender.DTOs
{
    public class ValidateOtpDTO
    {
        public string Contact {  get; set; }
        public int StepId { get; set; }
        public string OTP {  get; set; }
        public int Decision { get; set; }
    }
}
