using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Azure;
using System.IO;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
namespace ConsoleApp
{
    /// <summary>
    /// Creates, manage and maintain indexes
    /// </summary>
    public class AzureSearchManager
    {
        ILogger<AzureSearchManager> logger;
        IConfiguration configuration;
        public AzureSearchManager(ILogger<AzureSearchManager> logger, IConfiguration config)
        {
            this.logger = logger;
            this.configuration = config;
        }
        async public Task LoadIndex(AzureKeyCredential credential, string SearchIndexName)
        {
            SearchClient client = new SearchClient(new Uri(configuration["Azure.Search.EndPoint"]),SearchIndexName, credential);
            var countResponse = await client.GetDocumentCountAsync();
            if (countResponse.Value == 0)
            {
                string localBlogPostsFilePath = configuration["LocalBlogPostsFileName"];
                var blog = JsonSerializer.Deserialize<Blog>(File.ReadAllText(localBlogPostsFilePath));
                var postUploadActions = blog.Posts.Select(bp => IndexDocumentsAction.Upload<BlogPost>(bp));
                IndexDocumentsBatch<BlogPost> indexDocumentsBatch = IndexDocumentsBatch.Create<BlogPost>(postUploadActions.ToArray());
                var response = await client.IndexDocumentsAsync(indexDocumentsBatch);
                logger.LogInformation("Document uploaded to index:{}", SearchIndexName);
            }
            else
            {
                logger.LogInformation($"Found {countResponse.Value} documents in {SearchIndexName}");
            }
        }
        async public Task CreateIndexIfNotPresent(AzureKeyCredential credential, string SearchIndexName)
        {
            SearchIndexClient client = new SearchIndexClient(new Uri(configuration["Azure.Search.EndPoint"]), credential);
            try
            {
                var response = await client.GetIndexAsync(SearchIndexName);
                logger.LogInformation("Index {Index} exits", SearchIndexName);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                SearchIndex si = new SearchIndex(SearchIndexName)
                {
                    VectorSearch = new()
                    {
                        Algorithms =
                        {
                            new HnswAlgorithmConfiguration($"{SearchIndexName}-config")
                        },
                        Vectorizers =
                        {
                            new AzureOpenAIVectorizer($"{SearchIndexName}-vectorizer")
                            {
                                Parameters = new AzureOpenAIVectorizerParameters()
                                {
                                    ResourceUri = new Uri(configuration["Azure.OpenAI.Url"]),
                                    ApiKey= configuration["Azure.OpenAI.KEY"],
                                    DeploymentName ="gpt4",
                                    ModelName="text-embedding-3-large" // This is not the deployed name, but the standard model name
                                }
                            }
                        }
                    },
                    Fields = {
                        new SimpleField("id", SearchFieldDataType.String){ IsKey=true},
                        new SearchableField("content") { AnalyzerName = LexicalAnalyzerName.StandardLucene },
                        new SearchableField("title"){IsFilterable=true,AnalyzerName = LexicalAnalyzerName.StandardLucene},
                        new SearchableField("url"){IsFilterable = true},
                        new SearchableField("PublishedOn"){IsFilterable=true,IsSortable=true},
                    }
                };
                var searchIndexResponse = await client.CreateIndexAsync(si);
                logger.LogInformation("Index created name:{indexName}", SearchIndexName);
            
            }
            
        }
    }
}