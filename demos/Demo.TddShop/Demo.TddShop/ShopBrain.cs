using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;

namespace Demo.TddShop
{
    public class ShopBrain
    {
        public IChatClient CreateChatClient()
        {
            var deployment = Environment.GetEnvironmentVariable("AzureOpenAI_Gpt4_Deployment")!;
            var azureKey = Environment.GetEnvironmentVariable("AzureOpenAI_Gpt4_ApiKey")!;
            var endpoint = Environment.GetEnvironmentVariable("AzureOpenAI_Gpt4_Endpoint")!;

            var azureChatClient = new AzureOpenAIClient(
                new Uri(endpoint), 
                new System.ClientModel.ApiKeyCredential(azureKey)
                ).AsChatClient(deployment);

            var builder =
                new ChatClientBuilder(azureChatClient)
                .ConfigureOptions(options =>
                {
                    options.Tools ??= [];
                    options.Tools.Add(AIFunctionFactory.Create(GetFoodMenu));
                })
                .UseFunctionInvocation();

            var client = builder.Build();

            return client;
        }

        [Description("Returns the food menu based on the attitude of the user")]
        private string GetFoodMenu(
            [Description("User's mood based on its chat.")]
            UserMood mood
            )
        {
            return mood switch
            {
                UserMood.NormalOrHappy => "Pizza, Pasta, Salad",
                UserMood.Sad => "Ice Cream, Chocolate, Cake",
                UserMood.Angry => "Nothing, you're on a diet",
                _ => "I don't know what you want"
            };
        }

        enum UserMood
        {
            NormalOrHappy,
            Sad,
            Angry
        }
    }
}
