using Google.Apis.Auth.OAuth2;
using Google.Apis.Blogger.v3.Data;
using Google.Apis.Blogger.v3;
using Google.Apis.Services;
using System.Threading;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ConsoleApp
{
    class BlogReader : IBlogReader
    {
        private readonly IConfiguration _config;
        public BlogReader(IConfiguration configuration)
        {
            _config = configuration;
        }
        async IAsyncEnumerable<BlogPost> IBlogReader.GetBlogPost([EnumeratorCancellation]CancellationToken token)
        {
            PostList posts = await GetPostsByLabel("NAS",token);

            // Display the posts
            if (posts.Items != null)
            {
                foreach (var post in posts.Items)
                {
                    Console.WriteLine($"Title: {post.Title}");
                    Console.WriteLine($"Published: {post.Published}");
                    Console.WriteLine($"Content: {post.Content.Substring(0, Math.Min(post.Content.Length, 1000))}...");
                    Console.WriteLine(new string('-', 50));
                    yield return new BlogPost() { Title = post.Title, PublishedOn = DateTime.Parse(post.Published), Content = post.Content };
                }
            }
            else
            {
                Console.WriteLine("No posts found.");
            }
        }

        private async Task<PostList> GetPostsByLabel(string labels,CancellationToken token)
        {
            GoogleCredential credential = await GetCredential(token);

            // Create the Blogger service
            var bloggerService = new BloggerService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "OpenAIRAGonBlogger"
            });

            // Replace with your Blogger blog ID
            string blogId = _config["blogId"];

            // Request the list of posts
            var postsRequest = bloggerService.Posts.List(blogId);
            postsRequest.Labels = labels;// TODO : Make it config
            postsRequest.MaxResults = 5;
            PostList posts = await postsRequest.ExecuteAsync();
            return posts;
        }

        private async Task<GoogleCredential> GetCredential(CancellationToken token)
        {
            var serviceAccountKeyFile = _config["Google.Blogs.ApiKeyPath"];
            GoogleCredential credential;
            using (var stream = new FileStream(serviceAccountKeyFile, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleCredential.FromStreamAsync(stream, token);

                credential = credential.CreateScoped(BloggerService.Scope.BloggerReadonly);
            }
            return credential;
            //return await GoogleWebAuthorizationBroker.AuthorizeAsync(
            //               new ClientSecrets
            //               {
            //                   ClientId = _config["Google.Blogs.ClientId"],
            //                   ClientSecret = _config["Google.Blogs.ClientIdSecret"]
            //               },
            //               new[] { BloggerService.Scope.BloggerReadonly },
            //               "user",
            //               CancellationToken.None);
        }
    }
}