namespace Documents.Generator.Contracts
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    internal interface IPdfRenderer
    {
        public ValueTask<Stream> RenderAsync(string content, CancellationToken cancellationToken = default);
    }
}