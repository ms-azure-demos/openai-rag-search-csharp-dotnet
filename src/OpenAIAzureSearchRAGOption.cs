using EasyConsole;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using OpenAI;
using System.Security.Policy;
using Microsoft.Extensions.Configuration;
using Azure;
using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using System.Threading;
using System.IO;
using OpenAI.VectorStores;
using OpenAI.Files;
using Azure.AI.OpenAI.Chat;
using System.Net;
namespace ConsoleApp
{
    class OpenAIAzureSearchRAGOptions
    {
        private const string SearchIndexName = "blog-index";
        IBlogReader blogReader;
        ILogger<OpenAIAzureSearchRAGOptions> logger;
        IConfiguration configuration;
        AzureSearchManager searchManager;
        public OpenAIAzureSearchRAGOptions(IBlogReader dep, ILogger<OpenAIAzureSearchRAGOptions> logger, IConfiguration config, AzureSearchManager azureSearchManager)
        {
            blogReader = dep;
            this.logger = logger;
            this.configuration = config;
            this.searchManager = azureSearchManager;
        }
        async internal Task ExecuteSearch(CancellationToken stoppingToken)
        {
            logger.LogTrace($"{nameof(OpenAIAzureSearchRAGOptions)} : Start");
            string localBlogPostsFilePath = configuration["LocalBlogPostsFileName"];
            OpenAIClient client = new AzureOpenAIClient(new Uri(configuration["Azure.OpenAI.Url"]), new ApiKeyCredential(configuration["Azure.OpenAI.Key"]));
            string input = "q";
            do
            {
                input = Input.ReadString("Question (q/quit) to quit: ");
                var chatClient = client.GetChatClient("gpt4");
                ChatCompletionOptions chatCompletionOptions = new ChatCompletionOptions();
#pragma warning disable AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                chatCompletionOptions.AddDataSource(new AzureSearchChatDataSource()
                {
                    Endpoint = new Uri(configuration["Azure.Search.EndPoint"]),
                    IndexName = configuration["Azure.Search.IndexName"],
                    Authentication = DataSourceAuthentication.FromApiKey(configuration["AzureSearch.ApiKey"])
                    
                });
#pragma warning restore AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                var chatCompletion = await chatClient.CompleteChatAsync([
                    new SystemChatMessage("You are a chatbot answering from the blog named Joymon v/s Code located at joymonscode.blogspot.com. You will be using the latest content available in prompt.Do not answer from any sources other than the mentioned blog"),
                    //new UserChatMessage(File.ReadAllText(localBlogPostsFilePath)),
                    new UserChatMessage(input)]);
                logger.LogInformation($"ChatGPT: {chatCompletion.Value.Role}: {chatCompletion.Value.Content[0].Text} ");
            } while (!string.Equals(input, "q", StringComparison.OrdinalIgnoreCase) && !string.Equals(input, "quit", StringComparison.OrdinalIgnoreCase));
        }

        internal async Task VectorizeBlogPosts(CancellationToken token)
        {
            AzureKeyCredential credential = new AzureKeyCredential(configuration["Azure.Search.ApiKey"]);
            await searchManager.CreateIndexIfNotPresent(credential,SearchIndexName);
            await searchManager.LoadIndex(credential,SearchIndexName);
        }


        private async Task ListFiles(FileClient fileClient)
        {
            var batchFiles = await fileClient.GetFilesAsync(OpenAIFilePurpose.Batch);
            foreach (var batchFile in batchFiles.Value)
            {
                logger.LogInformation($"{batchFile.Id},{batchFile.Filename},{batchFile.Status}-{batchFile.StatusDetails}");
            }
        }

        private async Task ListFilesInVectorStore(VectorStore vs)
        {
            logger.LogInformation($"Vector Store : Name:{vs.Name},STatus:{vs.Status},Id:{vs.Id}");
            logger.LogInformation($"FileCount:{vs.FileCounts.Total},Completed:{vs.FileCounts.Completed},InProgress:{vs.FileCounts.InProgress},Cancelled:{vs.FileCounts.Cancelled},Failed:{vs.FileCounts.Failed}");
            await Task.FromResult<string>("");
        }
    }
}