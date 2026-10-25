using Microsoft.AspNetCore.Mvc;
using Microsoft.KernelMemory;

namespace AI_Powered_Ticketing_and_Support_System.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IKernelMemory _memory;
        private readonly IWebHostEnvironment _env;

        public DocumentController(IKernelMemory memory, IWebHostEnvironment env)
        {
            _memory = memory;
            _env = env;
        }

        [HttpPost("register")]

        public void add(IFormFile file)    
        {

        }

        [HttpPost("upload-internal")]
        public async Task<IActionResult> UploadInternal( CancellationToken ct )
        {
            // 1. Use the exact filename from your Documents folder
            string filePath = Path.Combine(_env.ContentRootPath, "Documents", "vocabulary.pdf");

            // 2. Safety check to make sure the file exists at runtime
            //if (!File.Exists(filePath))
            //{
            //    return NotFound($"The internal file could not be found at path: {filePath}");
            //}

            // 3. Extract the document ID and file details manually
            var fileName = Path.GetFileName(filePath);
            var documentId = Path.GetFileNameWithoutExtension(filePath);

            // 4. Open an internal read stream instead of getting it from an uploaded file
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // 5. Pass your local stream to your memory engine
            await _memory.ImportDocumentAsync(
                stream,
                fileName: fileName,
                documentId: documentId,
                cancellationToken: ct);

            return Ok(new { documentId, message = "Internal document imported successfully." });
        }



        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AskRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("Question is required.");

            try
            {
                var answer = await _memory.AskAsync(
                    request.Question,
                    minRelevance: 0.5,
                    cancellationToken: ct);


                return Ok(new
                {
                    answer = answer.Result,
                    sources = answer.RelevantSources.Select(s => new
                    {
                        s.SourceName,
                        chunks = s.Partitions.Select(p => new
                        {
                            p.PartitionNumber,
                            p.Relevance,
                            preview = p.Text.Length > 200 ? p.Text[..200] : p.Text
                        })
                    })
                });
            }
            catch (System.ClientModel.ClientResultException ex)
            {
                var raw = ex.GetRawResponse();
                Console.WriteLine(raw?.Content.ToString());
                throw;// OpenAI's JSON error with "message" and "param"
            }
        }

        // ---------- DELETE ----------
        [HttpDelete("{documentId}")]
        public async Task<IActionResult> Delete(string documentId, CancellationToken ct)
        {
            await _memory.DeleteDocumentAsync(documentId, cancellationToken: ct);
            return NoContent();
        }
    }

    public record AskRequest(string Question);

}


   