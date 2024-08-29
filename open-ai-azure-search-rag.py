import os
from openai import AzureOpenAI
from azure.identity import DefaultAzureCredential, get_bearer_token_provider
from dotenv import load_dotenv

load_dotenv()
deployment = os.getenv("DEPLOYMENT_NAME", "gpt4") # This is the not the standard model name, instead the name of model deployment.
search_endpoint = os.getenv("SEARCH_ENDPOINT")
search_key = os.getenv("SEARCH_KEY")
search_index = os.getenv("SEARCH_INDEX_NAME", "blog-index")

token_provider = get_bearer_token_provider(
    DefaultAzureCredential(),
    "https://cognitiveservices.azure.com/.default")
client = AzureOpenAI(
    azure_endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
    api_key=os.getenv("AZURE_OPENAI_API_KEY"),
    api_version="2024-05-01-preview",
)
question = input("Question (q/quit) to quit: ")
while question != "q" and question != "quit":
  completion = client.chat.completions.create(
    model=deployment,
    messages= [
    {
      "role": "user",
      "content": f"{question}"
    }],
    max_tokens=800,
    temperature=0.7,
    top_p=0.95,
    frequency_penalty=0,
    presence_penalty=0,
    stop=None,
    stream=False,
    extra_body={
      "data_sources": [{
          "type": "azure_search",
          "parameters": {
            "endpoint": f"{search_endpoint}",
            "index_name": "blog-index",
            "semantic_configuration": "default",
            "query_type": "simple",
            "fields_mapping": {},
            "in_scope": True,
            "role_information": "You are a chatbot answering from the blog named Joymon v/s Code located at joymonscode.blogspot.com. You will be using the latest content available in prompt or RAG data source. Do not answer from any sources other than the mentioned blog",
            "filter": None,
            "strictness": 3,
            "top_n_documents": 5,
            "authentication": {
              "type": "api_key",
              "key": f"{search_key}"
            }
          }
        }]
    }
  )
  print(f"{completion.choices[0].message.role} : {completion.choices[0].message.content}")
  for citation in completion.choices[0].message.context["citations"]:
    print (f"Citation url:{citation['url']}")
  print(completion.to_json())
  question = input("Question (q/quit) to quit: ")