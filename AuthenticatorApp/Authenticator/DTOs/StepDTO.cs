using Authenticator.Models;
using Authenticator.Utilities;

namespace Authenticator.DTOs
{
    public class StepDTO
    {
        public StepDTO(Step step)
        {
            Id = step.Id;
            Status = step.Status;
            ResponsibleUserId = step.ResponsibleUserId;
        }

        public int Id { get; set; }
        public StepStatus Status { get; set; }
        public int ResponsibleUserId { get; set; }
    }
}
