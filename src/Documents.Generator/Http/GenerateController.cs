namespace Documents.Generator.Http
{
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Documents.Generator.Contracts;
    using Documents.Generator.Http.Dto;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("/generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public sealed class GenerateController : ControllerBase
    {
        private readonly IDocumentGenerator documentGenerator;

        public GenerateController(IDocumentGenerator documentGenerator)
        {
            this.documentGenerator = documentGenerator;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateAsync(GenerateRequest generateRequest, CancellationToken cancellationToken)
        {
            var result = await documentGenerator.GenerateAsync(
                generateRequest.Template,
                generateRequest.Context,
                cancellationToken);

            return File(result, MediaTypeNames.Application.Pdf);
        }
    }
}