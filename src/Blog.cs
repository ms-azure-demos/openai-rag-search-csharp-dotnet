using System.Buffers;
using System.Collections.Generic;

namespace ConsoleApp
{
    class Blog
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public List<BlogPost> Posts { get; set; } = new List<BlogPost>();
    }
}