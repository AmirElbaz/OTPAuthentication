using Authenticator.DataAccess;
using Authenticator.DTOs;
using Authenticator.Models;
using AuthenticatorService;
using EmailSender.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace Authenticator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StepAuthenticator : ControllerBase
    {
        private readonly ApplicationContext _context;
      
        private readonly HttpClient _httpClient;
        private readonly IAuthenticatorService _emailService;

        public StepAuthenticator(ApplicationContext context, HttpClient httpClient,IAuthenticatorService emailService)
        {
            _context = context;
            _emailService = emailService;
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSteps()
        {
            var steps = await _context.Steps.ToListAsync();
            if (steps.Any())
            {
                var StepDTOS = new List<StepDTO>();
                foreach (var step in steps) {
                    StepDTOS.Add(new StepDTO(step));
                }
                return Ok(StepDTOS);
            }
            return NotFound();
        }
        [HttpPost("authenticate")]
        public async Task<IActionResult> RequestStepAuthentication([FromBody]AuthRequestDTO request)
        {
            var step = await _context.Steps.Include(s=>s.ResponsibleUser).FirstOrDefaultAsync(s => s.Id == request.StepId);
      
            if (request.Method == 1) {
                var (success,URL) = await  _emailService.Authenticate(request.StepId, step.ResponsibleUser.Email);
            if(success)
                return Ok(new {RedirectUrl= URL });
                return BadRequest();

            }
            else
            {
                return Ok();    

            }

        }
        [HttpGet("{stepId}/{decision}")]
        public async Task<IActionResult> AuthenticateStep(int StepId, int decision)
        {
            var step = await _context.Steps.FirstOrDefaultAsync(s => s.Id == StepId);
            step.Status = (decision == 0) ? Utilities.StepStatus.Rejected : Utilities.StepStatus.Authorised;

            _context.Steps.Update(step);
            await _context.SaveChangesAsync();
            return Redirect("http://localhost:4200/");

        }
    }
}
