namespace Documents.Generator.Http.Dto
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json;

    public sealed class GenerateRequest
    {
        [Required]
        public string Template { get; set; }

        [Required]
        public JsonDocument Context { get; set; }
    }
}