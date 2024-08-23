using EasyConsole;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System;

namespace ConsoleApp
{
    internal class MenuService : BackgroundService
    {
        public OpenAIRAGOption openAIRAGOption;

        public OpenAIOption openAIOption;
        private readonly DownloadBlogPostsOption downloadBlogPostsOption;
        public MenuService(OpenAIOption openAIOption,OpenAIRAGOption openAIRAGOption,DownloadBlogPostsOption downloadBlogPostsOption)
        {
            this.openAIRAGOption = openAIRAGOption;
            this.openAIOption = openAIOption;
            this.downloadBlogPostsOption = downloadBlogPostsOption;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var menu = new Menu()
                .Add("Open AI - Extract posts from joymonscode", async (token) => await downloadBlogPostsOption.Execute(stoppingToken))
                .Add("Open AI - Talk with joymonscode", async (token) => await openAIOption.Execute(stoppingToken))
                .Add("Open AI - RAG with joymonscode", async (token) => await openAIRAGOption.Execute(stoppingToken))
                .AddSync("Exit", () => Environment.Exit(0));
            await menu.Display(CancellationToken.None);
            await base.StartAsync(stoppingToken);
        }
    }
}