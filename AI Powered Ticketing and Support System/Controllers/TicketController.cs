using AI_Powered_Ticketing_and_Support_System.Feature.TriageAutomation;
using Microsoft.AspNetCore.Mvc;

namespace AI_Powered_Ticketing_and_Support_System.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly TicketAutomationService _aiService;

        public TicketController(TicketAutomationService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("triage")]
        public async Task<ActionResult<TriageReport>> Triage([FromBody] TriageRequest request)
        {

            if (string.IsNullOrWhiteSpace(request.ticketMessage))
            {
                return BadRequest("The ticket message payload cannot be empty.");
            }

            try
            {
                TriageReport triageModel = await _aiService.ExtractTriageDetailsWithFunctionCallingAsync(request.ticketMessage);
                return Ok(triageModel);
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred during automated triage processing.");
            }
        }
    }
    public record TriageRequest(string ticketMessage);
}
