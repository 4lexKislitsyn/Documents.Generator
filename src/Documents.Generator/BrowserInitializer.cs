namespace Documents.Generator
{
    using Microsoft.Extensions.Options;
    using PuppeteerSharp;

    internal sealed class BrowserInitializer : IAsyncDisposable
    {
        private readonly IOptions<LaunchOptions> launchOptions;
        private readonly ILoggerFactory loggerFactory;
        private IBrowser? browser;

        public BrowserInitializer(IOptions<LaunchOptions> launchOptions, ILoggerFactory loggerFactory)
        {
            this.launchOptions = launchOptions;
            this.loggerFactory = loggerFactory;
        }

        public async ValueTask InitializeAsync(CancellationToken cancellationToken)
        {
            browser = await Puppeteer.LaunchAsync(launchOptions.Value, loggerFactory).WaitAsync(cancellationToken);
        }

        public IBrowser GetBrowser()
        {
            return browser ?? throw new InvalidOperationException("Browser was not initialized");
        }

        public async ValueTask DisposeAsync()
        {
            if (browser is not null)
            {
                await browser.DisposeAsync();
            }
        }
    }
}