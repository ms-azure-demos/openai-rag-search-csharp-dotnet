using EasyConsole;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    internal class MenuService : BackgroundService
    {
        private readonly OpenAIRAGOption openAIRAGOption;
        private readonly OpenAIAzureSearchRAGOptions openAIAzureSearchRAGOption;
        private readonly OpenAIOption openAIOption;
        private readonly DownloadBlogPostsOption downloadBlogPostsOption;
        private readonly ILogger<MenuService> logger;
        public MenuService(OpenAIOption openAIOption,OpenAIRAGOption openAIRAGOption,OpenAIAzureSearchRAGOptions openAIAzureSearchRAGOption, 
            DownloadBlogPostsOption downloadBlogPostsOption,ILogger<MenuService> logger)
        {
            this.openAIRAGOption = openAIRAGOption;
            this.openAIOption = openAIOption;
            this.downloadBlogPostsOption = downloadBlogPostsOption;
            this.openAIAzureSearchRAGOption = openAIAzureSearchRAGOption;
            this.logger = logger;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var menu = new Menu()
                .Add("Extract posts from joymonscode", async (token) => await downloadBlogPostsOption.Execute(token))
                .Add("Open AI - Talk with joymonscode", async (token) => await openAIOption.Execute(token))
                .Add("Open AI - RAG with joymonscode", async (token) => await openAIRAGOption.Execute(token))
                .Add("Azure Search - Add posts to index",async (token) => await openAIAzureSearchRAGOption.AddBlogPostsToSearchIndex(token))
                .Add("Open AI - Azure Search RAG with joymonscode", async (token) => await openAIAzureSearchRAGOption.ExecuteSearch(token))
                .AddSync("Exit", () => Environment.Exit(0));
            try
            {
                await menu.Display(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occured");
            }
            await base.StartAsync(stoppingToken);
        }
    }
}