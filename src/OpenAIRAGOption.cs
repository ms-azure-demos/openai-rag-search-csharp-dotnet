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
namespace ConsoleApp
{
    class OpenAIRAGOption
    {
        IBlogReader blogReader;
        ILogger<OpenAIRAGOption> logger;
        IConfiguration configuration;

        public OpenAIRAGOption(IBlogReader dep, ILogger<OpenAIRAGOption> logger, IConfiguration config)
        {
            blogReader = dep;
            this.logger = logger;
            this.configuration = config;
        }
        async internal Task Execute(CancellationToken stoppingToken)
        {
            logger.LogTrace($"{nameof(OpenAIRAGOption)} : Start");
            string localBlogPostsFilePath = configuration["LocalBlogPostsFileName"];
            OpenAIClient client = new AzureOpenAIClient(new Uri(configuration["Azure.OpenAI.Url"]), new ApiKeyCredential(configuration["Azure.OpenAI.Key"]));
            while (true)
            {
                string input = Input.ReadString("Question: ");
                var chatClient = client.GetChatClient("gpt4");
                OpenAI.Embeddings.EmbeddingClient embeddingClient = client.GetEmbeddingClient("text-embedding-ada-002");
                var result = await embeddingClient.GenerateEmbeddingAsync(input);
                if (result != null)
                {
                    logger.LogInformation($"Vector of question: {result.Value}");
                    var fileClient = client.GetFileClient();
                    await ListFiles(fileClient);
                    var oaifiResult = await fileClient.UploadFileAsync(localBlogPostsFilePath, FileUploadPurpose.Batch);
                    logger.LogInformation($"Uploaded file {oaifiResult.Value.Filename}");
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    VectorStoreClient vectorStoreClient = client.GetVectorStoreClient();
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    var vcResult = await vectorStoreClient.GetVectorStoreAsync("blogvc");
                    VectorStore vs = vcResult.Value;
                    await ListFilesInVectorStore(vs);
                    if (vs != null)
                    {
                        var fa = await vectorStoreClient.AddFileToVectorStoreAsync(vs, oaifiResult.Value);
                        
                    }
                }
                ChatCompletionOptions chatCompletionOptions = new ChatCompletionOptions();
                //chatCompletionOptions.Tools.Add(new ChatTool() { });
                var chatCompletion = await chatClient.CompleteChatAsync([
                    new SystemChatMessage("You are a chatbot answering from the blog named Joymon v/s Code located at joymonscode.blogspot.com. You will be using the latest content available in prompt.Do not answer from any sources other than the mentioned blog"),
                    //new UserChatMessage(File.ReadAllText(localBlogPostsFilePath)),
                    new UserChatMessage(input)]);
                logger.LogInformation($"ChatGPT: {chatCompletion.Value.Role}: {chatCompletion.Value.Content[0].Text} ");
            }
        }

        private async Task ListFiles(FileClient fileClient)
        {
            var batchFiles =await fileClient.GetFilesAsync(OpenAIFilePurpose.Batch);
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