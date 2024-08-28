using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
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
        async public Task CreateIndexIfNotPresent(AzureKeyCredential credential, string SearchIndexName)
        {

            SearchIndexClient client = new SearchIndexClient(new Uri(configuration["Azure.Search.EndPoint"]), credential);
            try
            {
                var response = await client.GetIndexAsync(SearchIndexName);
                logger.LogInformation("Index {Index} exits", SearchIndexName);
            }
            catch (RequestFailedException ex) {
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
                                    ModelName="text-embedding"
                                }
                            }
                        }
                    },
                    Fields = {
                        new SimpleField("Id", SearchFieldDataType.Int32),
                        new SearchableField("content") { AnalyzerName = LexicalAnalyzerName.StandardLucene },
                        new SearchableField("title"){IsFilterable=true,AnalyzerName = LexicalAnalyzerName.StandardLucene},
                        new SearchableField("Url")

                    }
                };
                var searchIndexResponse = await client.CreateIndexAsync(si);
                logger.LogInformation("Index created name:{indexName}", SearchIndexName);
            }
        }
    }
}