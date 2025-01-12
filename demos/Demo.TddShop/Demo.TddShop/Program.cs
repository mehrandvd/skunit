// See https://aka.ms/new-console-template for more information
using Demo.TddShop;
using Microsoft.Extensions.AI;

Console.WriteLine("Welcome to my Food Shop!");

var brain = new ShopBrain();

var chatClient = brain.CreateChatClient();
var messages = new List<ChatMessage>();
do
{
    Console.Write("> ");
    var input = Console.ReadLine();
    messages.Add(new ChatMessage(ChatRole.User, input));

    var response = await chatClient.CompleteAsync(messages);
    var answer = response.Choices.First().Text;
    Console.WriteLine("Copilot> "+ answer);
    messages.Add(new ChatMessage(ChatRole.Assistant, answer));
} while (true);
