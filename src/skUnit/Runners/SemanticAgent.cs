using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skUnit.Runners
{
    public class SemanticAgent
    {
        public SemanticAgent(IChatClient chatClient)
        {
            Agent = chatClient.AsAIAgent(
                instructions:
                """

                """);
        }

        private AIAgent Agent { get; set; }

        public async Task<SemanticValidationResult> AreSimilarAsync(string first, string second)
        {
            var prompt = $$"""
            Are the following two texts semantically similar?
            
            [[[First Text]]]
            
            {{first}}
            
            [[[End of First Text]]]
            
            
            [[[Second Text]]]
            
            {{second}}
            
            [[[End of Second Text]]]
            
            
            The result should be a valid json like:
            
            {
                "success": true or false,
                "message": "explain the reason that the texts are or are not semantically similar"
            }
            """;

            var response = await Agent.RunAsync<SemanticValidationResult>(prompt);

            return response.Result;
        }

        public async Task<SemanticValidationResult> HasConditionAsync(string input, string condition)
        {
            var prompt = $$"""
            Does the following text meet the condition semantically?
            
            [[[Input Text]]]
            
            {{input}}
            
            [[[End of Input Text]]]
            
            
            [[[Condition]]]
            
            {{condition}}
            
            [[[End of Condition]]]
            
            
            The result should be a valid json like:
            
            {
                "success": true or false,
                "message": "explain the reason that the input does or does not meet the condition"
            }
            """;
            var response = await Agent.RunAsync<SemanticValidationResult>(prompt);
            return response.Result;
        }
    }
}
