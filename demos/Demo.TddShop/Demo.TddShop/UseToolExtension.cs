using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Demo.TddShop
{
    public static class UseToolExtension
    {
        public static ChatClientBuilder UseTools(this ChatClientBuilder builder, IEnumerable<AIFunction> tools)
        {
            return builder.Use(inner => new UseToolsChatClient(inner, tools));
        }

        private class UseToolsChatClient(IChatClient chatClient, IEnumerable<AIFunction> tools) : FunctionInvokingChatClient(chatClient)
        {
            public override Task<ChatCompletion> CompleteAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            {
                options ??= new ChatOptions();
                options.Tools ??= new List<AITool>();

                foreach(var tool in tools)
                {
                    options.Tools.Add(tool);
                }

                return base.CompleteAsync(chatMessages, options, cancellationToken);
            }
        }
    }
}
