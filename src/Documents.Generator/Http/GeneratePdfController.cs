namespace Documents.Generator.Http
{
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Documents.Generator.Contracts;
    using Documents.Generator.Http.Dto;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Generate PDF files methods
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/generate/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Consumes("application/json")]
    [Produces("application/pdf")]
    public sealed class GeneratePdfController : ControllerBase
    {
        private readonly IDocumentGenerator documentGenerator;

        public GeneratePdfController(IDocumentGenerator documentGenerator)
        {
            this.documentGenerator = documentGenerator;
        }

        /// <summary>
        /// Generate from HTML template
        /// </summary>
        /// <param name="pdfGenerateRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("html")]
        public async Task<IActionResult> HtmlToPdfAsync(PdfGenerateRequest pdfGenerateRequest, CancellationToken cancellationToken)
        {
            var result = await documentGenerator.GenerateAsync(
                pdfGenerateRequest.Template,
                pdfGenerateRequest.Context,
                cancellationToken);

            return File(result, MediaTypeNames.Application.Pdf);
        }
    }
}