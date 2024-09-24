using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Demo.TddRepl.Plugins;

namespace Demo.TddRepl
{
    public class Brain
    {
        public Kernel Kernel { get; set; }

        public Brain()
        {
            var deploymentName = Environment.GetEnvironmentVariable("openai-deployment-name") ?? throw new InvalidOperationException("No key provided.");
            var endpoint = Environment.GetEnvironmentVariable("openai-endpoint") ?? throw new InvalidOperationException("No key provided.");
            var apiKey = Environment.GetEnvironmentVariable("openai-api-key") ?? throw new InvalidOperationException("No key provided.");

            var builder = Kernel.CreateBuilder();

            builder.Services.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

            //builder.Plugins.AddFromPromptDirectory("Plugins");
            builder.Plugins.AddFromType<PeoplePlugin>("PeoplePlugin");

            Kernel = builder.Build();
        }

        public async Task<ChatMessageContent> GetChatAnswerAsync(ChatHistory history)
        {
            var chatService = Kernel.GetRequiredService<IChatCompletionService>();

            OpenAIPromptExecutionSettings executionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var result = await chatService.GetChatMessageContentAsync(
                history,
                executionSettings: executionSettings,
                kernel: Kernel);

            return result;
        }
    }
}
