#pragma warning disable CS8618
namespace Documents.Generator.Http.Dto
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json;

    public sealed class PdfGenerateRequest
    {
        [Required]
        public string Template { get; set; }

        [Required]
        public JsonDocument Context { get; set; }
    }
}