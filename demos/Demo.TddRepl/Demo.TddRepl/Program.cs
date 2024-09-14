// See https://aka.ms/new-console-template for more information
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Demo.TddRepl;
using Microsoft.SemanticKernel.Connectors.OpenAI;
Console.WriteLine("Hello, World!");

var brain = new Brain();


// Create a history store the conversation
var history = new ChatHistory();

// Initiate a back-and-forth chat
string? userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    // Add user input
    history.AddUserMessage(userInput);

    // Get the response from the AI
    var result = await brain.GetChatAnswerAsync(history);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);
