using EasyConsole;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using OpenAI;
using Microsoft.Extensions.Configuration;
using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Threading;
namespace ConsoleApp
{
    class OpenAIOption
    {
        ILogger<OpenAIOption> logger;
        IConfiguration configuration;
        public OpenAIOption(ILogger<OpenAIOption> logger, IConfiguration config)
        {
            this.logger = logger;
            this.configuration = config;
        }
        async internal Task Execute(CancellationToken stoppingToken)
        {
            logger.LogTrace($"{nameof(OpenAIRAGOption)} : Start");
            string localBlogPostsFilePath = configuration["LocalBlogPostsFileName"];
            OpenAIClient client = new AzureOpenAIClient(new Uri(configuration["Azure.OpenAI.Url"]), new ApiKeyCredential(configuration["Azure.OpenAI.Key"]));
            string input ="q";
            do 
            {
                input = Input.ReadString("Question: ");
                var chatClient = client.GetChatClient("gpt4");
                ChatCompletionOptions chatCompletionOptions = new ChatCompletionOptions();
                var chatCompletion = await chatClient.CompleteChatAsync([
                    new SystemChatMessage("You are a chatbot answering from the blog named Joymon v/s Code located at joymonscode.blogspot.com. You will be using the latest content available in prompt.Do not answer from any sources other than the mentioned blog"),
                    new UserChatMessage(input)]);
                logger.LogInformation($"ChatGPT: {chatCompletion.Value.Role}: {chatCompletion.Value.Content[0].Text} ");
            } while (!string.Equals(input,"q",StringComparison.OrdinalIgnoreCase) && !string.Equals(input,"quit",StringComparison.OrdinalIgnoreCase));
        }
    }
}