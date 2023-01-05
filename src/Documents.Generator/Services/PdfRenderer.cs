namespace Documents.Generator.Services
{
    using Documents.Generator.Contracts;
    using Microsoft.Extensions.Options;
    using PuppeteerSharp;
    using PuppeteerSharp.Media;

    internal sealed class PdfRenderer : IPdfRenderer
    {
        private readonly ILogger<PdfRenderer> logger;
        private readonly PdfOptions pdfOptions;
        private readonly IBrowser browser;

        public PdfRenderer(ILogger<PdfRenderer> logger,
            IBrowser browser,
            IOptions<PdfOptions> pdfOptions)
        {
            this.logger = logger;
            this.browser = browser;
            this.pdfOptions = pdfOptions.Value;
        }

        public async ValueTask<Stream> RenderAsync(string content, CancellationToken cancellationToken = default)
        {
            await using var page = await browser.NewPageAsync().WaitAsync(cancellationToken);
            await page.SetRequestInterceptionAsync(true).WaitAsync(cancellationToken);
            page.Request += async (_, args) =>
            {
                logger.LogWarning("Request to {Url} was aborted", args.Request.Url);
                await args.Request.AbortAsync().WaitAsync(cancellationToken);
            };

            await page.SetJavaScriptEnabledAsync(false).WaitAsync(cancellationToken);
            await page.EmulateMediaTypeAsync(MediaType.Screen).WaitAsync(cancellationToken);
            await page.SetContentAsync(content).WaitAsync(cancellationToken);
            return await page.PdfStreamAsync(pdfOptions).WaitAsync(cancellationToken);
        }
    }
}