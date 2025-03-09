using Authenticator.Utilities;

namespace Authenticator.Models
{
    public class Step
    {
        public int Id { get; set; }
        public StepStatus Status { get; set; }
        public int ResponsibleUserId {  get; set; }
        public User ResponsibleUser { get; set; }
    }
}
