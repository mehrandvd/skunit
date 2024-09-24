using Microsoft.SemanticKernel.ChatCompletion;
using Demo.TddRepl;

var brain = new Brain();
var history = new ChatHistory();

string? userInput;
do
{
    Console.Write("User > ");
    userInput = Console.ReadLine();

    history.AddUserMessage(userInput ?? "");
    var result = await brain.GetChatAnswerAsync(history);
    Console.WriteLine("Assistant > " + result);
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);
