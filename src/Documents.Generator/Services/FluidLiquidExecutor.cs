namespace Documents.Generator.Services
{
    using System.Text.Json;
    using Documents.Generator.Contracts;
    using Fluid;

    internal sealed class FluidLiquidExecutor : ILiquidExecutor
    {
        private readonly ILogger<FluidLiquidExecutor> logger;
        private readonly FluidParser parser = new();
        private readonly TemplateOptions templateOptions = new();

        public FluidLiquidExecutor(ILogger<FluidLiquidExecutor> logger)
        {
            this.logger = logger;
            templateOptions.MemberAccessStrategy.Register(typeof(JsonElement), new JsonAccessor());
            templateOptions.ValueConverters.Add(ConvertJson);
        }

        public async ValueTask<string> ExecuteAsync(string template, JsonDocument jsonContext, CancellationToken cancellationToken = default)
        {
            if (!parser.TryParse(template, out var liquidTemplate, out var error))
            {
                logger.LogDebug("Template cannot be parsed. {Error}", error);
                throw new ArgumentException($"Template cannot be parsed: {error}");
            }

            var context = new TemplateContext(jsonContext.RootElement, templateOptions);
            context.SetValue("now", DateTime.UtcNow);
            context.SetValue("today", DateTime.UtcNow.Date);
            try
            {
                return await liquidTemplate.RenderAsync(context);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Template render failed");
                throw new ArgumentException($"Template cannot be rendered: {e.Message}");
            }
        }

        private static object? ConvertJson(object value)
        {
            if (value is not JsonElement element)
            {
                return value;
            }

            object? result = element.ValueKind switch
            {
                JsonValueKind.Array => element.EnumerateArray(),
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetDecimal(),
                JsonValueKind.True => element.GetBoolean(),
                JsonValueKind.False => element.GetBoolean(),
                _ => element
            };

            return result;
        }

        private sealed class JsonAccessor : IMemberAccessor
        {
            public object? Get(object obj, string name, TemplateContext ctx)
            {
                return ((JsonElement)obj).TryGetProperty(name, out var element)
                    ? element
                    : default(object?);
            }
        }
    }
}