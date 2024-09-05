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
using System.Collections.Generic;
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
            const string systemMessage = "You are a chatbot answering from the blog named Joymon v/s Code located at joymonscode.blogspot.com. You will be using the latest blog posts available RAG data source of Azure Search. Strictly use content from the mentioned blog only and data source. Politely refuse answers from anywhere else";
            OpenAIClient client = new AzureOpenAIClient(new Uri(configuration["Azure.OpenAI.Url"]), new ApiKeyCredential(configuration["Azure.OpenAI.Key"]));
            var chatClient = client.GetChatClient("gpt4");
            ChatCompletionOptions chatCompletionOptions = new ChatCompletionOptions()
            {
                Temperature = .7f,
                TopP = .95f,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
                MaxTokens = 800
            };
#pragma warning disable AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            chatCompletionOptions.AddDataSource(new AzureSearchChatDataSource()
            {
                Endpoint = new Uri(configuration["Azure.Search.EndPoint"]),
                IndexName = configuration["Azure.Search.IndexName"],
                SemanticConfiguration = "default",
                QueryType = DataSourceQueryType.Simple,
                InScope = true,
                RoleInformation = systemMessage,
                FieldMappings = new DataSourceFieldMappings(),
                Filter = null,
                Strictness = 3,
                TopNDocuments = 5,
                Authentication = DataSourceAuthentication.FromApiKey(configuration["Azure.Search.ApiKey"]),
            });
            string input = Input.ReadString("Question (q/quit) to quit: ");
            while (!string.Equals(input, "q", StringComparison.OrdinalIgnoreCase) && !string.Equals(input, "quit", StringComparison.OrdinalIgnoreCase))
            {
                var chatCompletion = await chatClient.CompleteChatAsync(
                    [
                        new SystemChatMessage(systemMessage),
                        new UserChatMessage(input)
                    ],
                    chatCompletionOptions);
                AzureChatMessageContext onYourDataContext = chatCompletion.Value.GetAzureMessageContext();
#pragma warning restore AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                logger.LogInformation($"ChatGPT: {chatCompletion.Value.Role}: {chatCompletion.Value.Content[0].Text} ");

                if (onYourDataContext?.Intent is not null)
                {
                    logger.LogInformation($"Intent: {onYourDataContext.Intent}");
                }
                foreach (AzureChatCitation citation in onYourDataContext?.Citations ?? new List<AzureChatCitation>())
                {
                    logger.LogInformation($"Citation:{citation.Url}");
                }

                input = Input.ReadString("Question (q/quit) to quit: ");
            }
        }

        internal async Task AddBlogPostsToSearchIndex(CancellationToken token)
        {
            AzureKeyCredential credential = new AzureKeyCredential(configuration["Azure.Search.ApiKey"]);
            await searchManager.CreateIndexIfNotPresent(credential, SearchIndexName);
            await searchManager.LoadIndex(credential, SearchIndexName);
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