namespace EmailSender.InterFaces
{
    public interface IEmailSender
    {
        public Task SendEmailAsync(string Recepient,string Subject,string body);
    }
}
