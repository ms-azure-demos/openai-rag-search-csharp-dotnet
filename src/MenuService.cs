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
        public OpenAIRAGOption openAIRAGOption;
private readonly OpenAIAzureSearchRAGOptions openAIAzureSearchRAGOption;
        public OpenAIOption openAIOption;
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
                .Add("Extract posts from joymonscode", async (token) => await downloadBlogPostsOption.Execute(stoppingToken))
                .Add("Open AI - Talk with joymonscode", async (token) => await openAIOption.Execute(stoppingToken))
                .Add("Open AI - RAG with joymonscode", async (token) => await openAIRAGOption.Execute(stoppingToken))
                .Add("Azure Search - Vectorize blog posts",async (token) => await openAIAzureSearchRAGOption.VectorizeBlogPosts(token))
                .Add("Open AI - Azure Search RAG with joymonscode", async (token) => await openAIAzureSearchRAGOption.ExecuteSearch(stoppingToken))
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