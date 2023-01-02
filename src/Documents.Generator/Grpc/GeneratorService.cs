namespace Documents.Generator.Grpc
{
    using System.Text.Json;
    using Documents.Generator.Contracts;
    using Documents.GRPC;
    using global::Grpc.Core;
    using Google.Protobuf;

    internal sealed class GeneratorService : Generator.GeneratorBase
    {
        private readonly IDocumentGenerator documentGenerator;

        public GeneratorService(IDocumentGenerator documentGenerator)
        {
            this.documentGenerator = documentGenerator;
        }

        public override async Task Generate(
            GenerateRequest request,
            IServerStreamWriter<GenerateResponse> responseStream,
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
                await responseStream.WriteAsync(new GenerateResponse
                {
                    Chunk = ByteString.CopyFrom(buffer[..bytesRead]),
                });
            }
        }
    }
}