namespace Documents.Generator.Contracts
{
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    internal interface ILiquidExecutor
    {
        public ValueTask<string> ExecuteAsync(string template, JsonDocument jsonContext, CancellationToken cancellationToken = default);
    }
}