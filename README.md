# Azure Open AI RAG with Azure Search data_source

This demo shows how can we include the data source in the chat completion API itself

# How to run
- Replace the values in appsettings.json file with your environment.
	- This require deploying model in your Azure Open AI.
- Get the Blogger API credential json from Google cloud dashboard and save as "api-project-gkey.json"
- Compile and run. It will show options
- Try the option 2 to talk with Chat GPT without RAG. It can answer based on the training time around Sep 2021
- First download the blog posts.
	- It will be using hardcoded tag=nas. Change that as per your blog
	- Downloaded posts will be available at /bin folder and the file name will be as per config[LocalBlogPostsFileName]
- Next upload the documents to Azure Search Index by using Option 4.
- Finally RAG using the option 5.
# Architecture

![https://raw.githubusercontent.com/ms-azure-demos/openai-rag/main/diagrams/c2-diagram.puml](https://raw.githubusercontent.com/ms-azure-demos/openai-rag/main/diagrams/c2-diagram.puml)

# Specifications

- .Net version - .Net 8
- Nugets referenced
	- DotNet.Helpers
	- easyconsolestd
	- Microsoft.Extensions.Hosting
# Dependency injection

- Supported. Refer the [Program.cs](/src/Program.cs) file for more details
- The options are injected as dependency to the [MenuService](/src/MenuService.cs then those are invoked based on selection. 

# Disclaimer
This is done for learning purpose and not thoroughly tested. It may not be production ready. This require basic understanding of C# .Net 8, Azure and Open AI integration.

In case anything is missing, wrong, please raise an issue or give a PR.