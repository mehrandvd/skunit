using Azure;
using Azure.AI.OpenAI;
using Demo.MoodyChef.Console;
using Google.Protobuf;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.UserSecrets;
using OpenAI.Chat;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Console.WriteLine("Hello, World!");

var builder = new ConfigurationBuilder()
              .AddUserSecrets<Program>()
              .AddEnvironmentVariables();

var configuration = builder.Build();

var apiKey = configuration["AzureOpenAI_ApiKey"] ??
    throw new Exception("No ApiKey is provided.");
var endpoint = configuration["AzureOpenAI_Endpoint"] ??
    throw new Exception("No Endpoint is provided.");
var deploymentName = configuration["AzureOpenAI_Deployment"] ??
    throw new Exception("No Deployment is provided.");


var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey))
    .GetChatClient(deploymentName)
    .AsIChatClient();


var agent = AgentGallery.CreateToolBasedAgent(client);

//var result = await agent.RunAsync("Such a beautiful day, what foods do you have?");
//Console.WriteLine(result);

var session = await agent.CreateSessionAsync();

//var messages = new List<ChatMessage>();

do
{
    Console.Write("> ");
    var input = Console.ReadLine();
    //messages.Add(new ChatMessage(ChatRole.User, input));

    //var response = await agent.RunAsync(messages);
    var response = await agent.RunAsync(input, session);

    var answer = response.Text;
    Console.WriteLine("Moody Chef> " + answer);

    //messages.AddMessages(response.AsChatResponse());
} while (true);


Console.ReadLine();