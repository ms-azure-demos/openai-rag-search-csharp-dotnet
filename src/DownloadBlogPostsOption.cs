using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Threading;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.IO;
namespace ConsoleApp
{
    class DownloadBlogPostsOption
    {
        ILogger<DownloadBlogPostsOption> logger;
        IConfiguration configuration;
        IBlogReader blogReader;
        public DownloadBlogPostsOption(IBlogReader dep, ILogger<DownloadBlogPostsOption> logger, IConfiguration config)
        {
            blogReader = dep;
            this.logger = logger;
            this.configuration = config;
        }
        async internal Task Execute(CancellationToken cancellationToken)
        {
            Blog blog = new Blog()
            {
                Author = "Joy George Kunjikkuru",
                Description = "Software engineering and coding blog",
                Name = "JoymonsCode"
            };
            var posts = blogReader.GetBlogPost(cancellationToken);
            await foreach (var post in posts)
            {
                blog.Posts.Add(post);
            }
            string jsonString = JsonSerializer.Serialize(blog);
            string localBlogPostsFilePath = configuration["LocalBlogPostsFileName"];
            File.WriteAllText(localBlogPostsFilePath, jsonString);
             logger.LogInformation($"Downloaded posts and saved to {localBlogPostsFilePath} ");
        }
    }
}