namespace Documents.Generator.Tests.Services
{
    using System.Globalization;
    using System.Text.Json;
    using Documents.Generator.Contracts;
    using Documents.Generator.Services;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using NUnit.Framework;
    using Shouldly;

    public class FluidLiquidExecutorTests
    {
        private readonly ILiquidExecutor instance = new FluidLiquidExecutor(Substitute.For<ILogger<FluidLiquidExecutor>>());

        [Test]
        public async ValueTask Should_execute_simple_json_template()
        {
            var template = "Hello, {{ name }}";
            var context = JsonDocument.Parse(@"{ ""name"": ""liquid"" }");

            var result = await instance.ExecuteAsync(template, context);

            result.ShouldBe("Hello, liquid");
        }

        [Test]
        public async ValueTask Should_execute_date_time_template()
        {
            var template = "Published: {{ published_at | date: \"%m/%Y %M:%H\"  }}";
            var context = JsonDocument.Parse($@"{{ ""published_at"": ""2022-12-31 13:14:55"" }}");

            var result = await instance.ExecuteAsync(template, context);

            result.ShouldBe("Published: 12/2022 14:13");
        }

        [Test]
        public async ValueTask  Should_execute_today_time_template()
        {
            var template = "Published: {{ today | date: \"%d/%m/%Y %H:%M\"  }}";
            var context = JsonDocument.Parse("{}");

            var result = await instance.ExecuteAsync(template, context);

            result.ShouldBe($"Published: {DateTime.UtcNow.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} 00:00");
        }

        [Test]
        public async ValueTask  Should_execute_now_time_template()
        {
            var template = "Published: {{ today | date: \"%d/%m/%Y\"  }}";
            var context = JsonDocument.Parse("{}");

            var result = await instance.ExecuteAsync(template, context);

            result.ShouldBe($"Published: {DateTime.UtcNow.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}");
        }

        [Test]
        public async ValueTask Should_execute_complex_json_template()
        {
            var template = "Hello, {{ user.name }}";
            var context = JsonDocument.Parse(@"{ ""user"": { ""name"": ""liquid"" } }");

            var result = await instance.ExecuteAsync(template, context);

            result.ShouldBe("Hello, liquid");
        }

        [Test]
        public async ValueTask Should_execute_loop_by_complex_json_template()
        {
            var template = @"{% for product in products %}{{ product.title }}{%- unless forloop.last -%}-{% endunless -%}{% endfor %}";
            var context = JsonDocument.Parse(@"{ ""products"" : [ { ""title"": ""liquid"" }, { ""title"": ""template"" } ] }");

            var result = await instance.ExecuteAsync(template, context);

            result.ShouldBe("liquid-template");
        }

        [Theory]
        public async ValueTask Should_use_bool_and_conditions(bool value)
        {
            var template = @"{%- if condition -%}YES{%- else -%}NO{% endif -%}";
            var context = JsonDocument.Parse($@"{{ ""condition"" : {value.ToString().ToLower()} }}");

            var result = await instance.ExecuteAsync(template, context);

            result.ShouldBe(value ? "YES" : "NO");
        }

        [Test]
         public async ValueTask Should_use_numbers()
         {
             var template = @"result={{ number | minus: 2 }}";
             var context = JsonDocument.Parse($@"{{ ""number"" : 5 }}");

             var result = await instance.ExecuteAsync(template, context);

             result.ShouldBe("result=3");
         }
    }
}