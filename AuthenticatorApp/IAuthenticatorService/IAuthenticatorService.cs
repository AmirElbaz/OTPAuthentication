namespace AuthenticatorService
{
    public interface IAuthenticatorService
    {
        public Task<(bool success,string URL)> Authenticate(int StepId, string Contact);
    }
}
