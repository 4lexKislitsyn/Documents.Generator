namespace Documents.Generator.Grpc
{
    using System.Text.Json;
    using Documents.Generator.Contracts;
    using Documents.GRPC;
    using global::Grpc.Core;
    using Google.Protobuf;

    internal sealed class PdfGeneratorService : PdfGenerator.PdfGeneratorBase
    {
        private readonly IDocumentGenerator documentGenerator;

        public PdfGeneratorService(IDocumentGenerator documentGenerator)
        {
            this.documentGenerator = documentGenerator;
        }

        public override async Task HtmlToPdf(
            PdfGenerateRequest request,
            IServerStreamWriter<PdfGenerateResponse> responseStream,
            ServerCallContext context)
        {
            var jsonContext = JsonDocument.Parse(request.Context.ToStringUtf8());
            var stream = await documentGenerator.GenerateAsync(
                request.Template.ToStringUtf8(),
                jsonContext,
                context.CancellationToken);

            int bytesRead = 0;
            byte[] buffer = new byte[64 * 1024];

            while ((bytesRead = await stream.ReadAsync(buffer, context.CancellationToken)) > 0)
            {
                await responseStream.WriteAsync(new PdfGenerateResponse
                {
                    Chunk = ByteString.CopyFrom(buffer[..bytesRead]),
                });
            }
        }
    }
}