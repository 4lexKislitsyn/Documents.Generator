namespace Documents.Generator.Contracts
{
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDocumentGenerator
    {
        ValueTask<Stream> GenerateAsync(string template, JsonDocument context, CancellationToken cancellationToken = default);
    }
}