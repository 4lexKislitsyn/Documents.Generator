namespace Documents.Generator.Services
{
    using System.Text.Json;
    using Documents.Generator.Contracts;

    internal sealed class DocumentGeneratorService : IDocumentGenerator
    {
        private readonly ILiquidExecutor liquidExecutor;
        private readonly IPdfRenderer pdfRenderer;

        public DocumentGeneratorService(ILiquidExecutor liquidExecutor, IPdfRenderer pdfRenderer)
        {
            this.liquidExecutor = liquidExecutor;
            this.pdfRenderer = pdfRenderer;
        }

        public async ValueTask<Stream> GenerateAsync(
            string template,
            JsonDocument context,
            CancellationToken cancellationToken = default)
        {
            var content = await liquidExecutor.ExecuteAsync(template, context, cancellationToken);
            return await pdfRenderer.RenderAsync(content, cancellationToken);
        }
    }
}