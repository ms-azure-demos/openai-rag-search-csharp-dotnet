using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal interface IBlogReader
    {
        IAsyncEnumerable<BlogPost> GetBlogPost(CancellationToken stoppingToken);
    }
}