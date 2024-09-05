# Azure Open AI RAG with Azure Search data_source

This demo shows how can we include the data source in the chat completion API itself.

# How to run
- Replace the values in appsettings.json file with your environment.
	- This require deploying model in your Azure Open AI.
	- Get the Blogger API credential json file from Google cloud dashboard and save as "api-project-gkey.json" in the /src folder
- Compile and run. It will show options
- No RAG option
	- Try the option 2 to talk with Chat GPT without RAG. It can answer based on the training time around Sep 2021
- RAG option
	- First download the blog posts.
	- It will be using hardcoded tag=nas to download posts. Change that as per your blog
	- Downloaded posts will be available at /bin folder and the file name will be as per config[LocalBlogPostsFileName]
	- Next upload the documents to Azure Search Index by using Option 4. (It will be creating 'blog-index' if not present and upload to that)
	- Finally RAG using the option 5.

> In case anything is missing, please create an issue or PR.

# Architecture

![https://raw.githubusercontent.com/ms-azure-demos/openai-rag/main/diagrams/c2-diagram.puml]( https://www.plantuml.com/plantuml/proxy?src=https://raw.githubusercontent.com/ms-azure-demos/openai-rag/main/diagrams/c2-diagram.puml?format=svg)

# Specifications

- .Net version - .Net 8
- Nugets referenced
	- DotNet.Helpers - To reduce some code.
	- easyconsolestd - To show menu and get input
	- Microsoft.Extensions.Hosting
	- Google.Apis.Blogger.v3 - for working with Blogger APIs
	- Azure.Search.Documents - to upload blog posts into Azure search
	- Azure.AI.OpenAI - To interact with Azure OpenAI
# Dependency injection

- Supported. Refer the [Program.cs](/src/Program.cs) file for more details
- The options are injected as dependency to the [MenuService](/src/MenuService.cs then those are invoked based on selection. 

# Disclaimer
This is done for learning purpose and not production ready. This require basic understanding of C# .Net 8, Azure and Open AI integration.

In case anything is missing, wrong, please raise an issue or give a PR.