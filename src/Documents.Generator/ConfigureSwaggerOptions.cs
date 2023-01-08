namespace Documents.Generator
{
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider apiProvider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider apiProvider)
        {
            this.apiProvider = apiProvider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in apiProvider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
            }
        }

        public void Configure(string? name, SwaggerGenOptions options)
        {
            Configure(options);
        }

        private OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
        {
            return new OpenApiInfo
            {
                Title = "Documents.Generator Web API",
                Version = description.ApiVersion.ToString()
            };
        }
    }
}