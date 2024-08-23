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
using System.Threading;
using System.IO;
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
                var chatCompletion = await chatClient.CompleteChatAsync([
                    new SystemChatMessage("You are a chatbot answering from the blog named Joymon v/s Code located at joymonscode.blogspot.com. You will be using the latest content available in prompt."),
                    new UserChatMessage(File.ReadAllText(localBlogPostsFilePath)),
                    new UserChatMessage(input)]);
                logger.LogInformation($"ChatGPT: {chatCompletion.Value.Role}: {chatCompletion.Value.Content[0].Text} ");
            }
        }
    }
}