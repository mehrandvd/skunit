using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;
using SemanticValidation;
using skUnit.Exceptions;

namespace skUnit
{
    /// <summary>
	/// Contains various methods that are used to semantically verify that conditions are met during the
	/// process of running tests. This class uses SemanticKernel and OpenAI to validate these assertions semantically.
	/// </summary>
    public class SemanticAssert
    {
        private Semantic Semantic { get; set; }
        private HttpClient HttpClient { get; set; }
        private Dictionary<string, McpProcess> McpProcesses { get; set; }
        private string McpEndpoint { get; set; }

        /// <summary>
        /// This class needs a SemanticKernel chatClient to work.
        /// Pass your pre-configured chatClient to this constructor.
        /// </summary>
        /// <param name="chatClient"></param>
        public SemanticAssert(IChatClient chatClient)
        {
            Semantic = new Semantic(chatClient);
            HttpClient = new HttpClient();
        }

        /// <summary>
        /// Sets the MCP endpoint URL for validation.
        /// </summary>
        /// <param name="endpoint">The endpoint URL of the MCP server</param>
        public void UseMcpEndpoint(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
                throw new ArgumentNullException(nameof(endpoint));

            McpEndpoint = endpoint;
            HttpClient = new HttpClient { BaseAddress = new Uri(endpoint) };
        }

        /// <summary>
        /// Configures MCP processes using JSON configuration.
        /// </summary>
        /// <param name="processConfig">JSON configuration for MCP processes</param>
        public void ConfigureMcpProcesses(string processConfig)
        {
            if (string.IsNullOrEmpty(processConfig))
                throw new ArgumentNullException(nameof(processConfig));

            var config = JsonSerializer.Deserialize<Dictionary<string, McpProcess>>(processConfig);
            if (config == null)
                throw new ArgumentException("Invalid process configuration JSON", nameof(processConfig));

            McpProcesses = config;
        }

        /// <summary>
        /// Creates a SemanticAssert instance with both MCP endpoint and process configuration.
        /// </summary>
        /// <param name="mcpEndpoint">The endpoint URL of the MCP server</param>
        /// <param name="processConfig">JSON configuration for MCP processes</param>
        public SemanticAssert(string mcpEndpoint, string processConfig)
        {
            if (string.IsNullOrEmpty(mcpEndpoint))
                throw new ArgumentNullException(nameof(mcpEndpoint));
            if (string.IsNullOrEmpty(processConfig))
                throw new ArgumentNullException(nameof(processConfig));

            McpEndpoint = mcpEndpoint;
            HttpClient = new HttpClient { BaseAddress = new Uri(mcpEndpoint) };

            var config = JsonSerializer.Deserialize<Dictionary<string, McpProcess>>(processConfig);
            if (config == null)
                throw new ArgumentException("Invalid process configuration JSON", nameof(processConfig));

            McpProcesses = config;
        }

        /// <summary>
        /// Validates a response against an MCP server's semantic validation endpoint.
        /// </summary>
        /// <param name="response">The response to validate</param>
        /// <param name="validationRules">The MCP validation rules in JSON format</param>
        public async Task ValidateMcpResponseAsync(string response, string validationRules)
        {
            if (string.IsNullOrEmpty(McpEndpoint))
            {
                throw new InvalidOperationException("MCP endpoint not configured. Call UseMcpEndpoint first.");
            }

            var request = new
            {
                response = response,
                validation = JsonNode.Parse(validationRules)
            };

            var httpResponse = await HttpClient.PostAsJsonAsync("validate", request);
            var result = await httpResponse.Content.ReadFromJsonAsync<ValidationResult>();

            if (result is null)
            {
                throw new SemanticAssertException("Unable to accomplish the MCP validation.");
            }

            if (!result.IsValid)
            {
                throw new SemanticAssertException(result.Reason ?? "MCP validation failed without a reason.");
            }
        }

        /// <summary>
        /// Validates a response using a specific MCP process configuration.
        /// </summary>
        /// <param name="response">The response to validate</param>
        /// <param name="validationRules">The MCP validation rules in JSON format</param>
        /// <param name="processName">The name of the MCP process to use</param>
        public async Task ValidateMcpResponseWithProcessAsync(string response, string validationRules, string processName)
        {
            if (McpProcesses == null)
            {
                throw new InvalidOperationException("MCP processes not configured. Call ConfigureMcpProcesses first.");
            }

            if (!McpProcesses.TryGetValue(processName, out var process))
            {
                throw new ArgumentException($"Process '{processName}' not found in configuration");
            }

            var request = new
            {
                response = response,
                validation = JsonNode.Parse(validationRules),
                process = process
            };

            var httpResponse = await HttpClient.PostAsJsonAsync("validate", request);
            var result = await httpResponse.Content.ReadFromJsonAsync<ValidationResult>();

            if (result is null)
            {
                throw new SemanticAssertException("Unable to accomplish the MCP validation.");
            }

            if (!result.IsValid)
            {
                throw new SemanticAssertException(result.Reason ?? "MCP validation failed without a reason.");
            }
        }

        /// <summary>
        /// Synchronous version of ValidateMcpResponseAsync
        /// </summary>
        public void ValidateMcpResponse(string response, string validationRules)
        {
            ValidateMcpResponseAsync(response, validationRules).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Synchronous version of ValidateMcpResponseWithProcessAsync
        /// </summary>
        public void ValidateMcpResponseWithProcess(string response, string validationRules, string processName)
        {
            ValidateMcpResponseWithProcessAsync(response, validationRules, processName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Checks whether <paramref name="first"/> and <paramref name="second"/> string are semantically similar.
        /// It uses the kernel and OpenAI to check this semantically.
        /// <example>
        /// <code>
        /// SemanticAssert.SimilarAsync("This automobile is red", "The car is red") // returns true
        /// SemanticAssert.SimilarAsync("This tree is red", "The car is red") // returns false
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
        public async Task SimilarAsync(string first, string second)
        {
            var result = await Semantic.AreSimilarAsync(first, second);

            if (result is null)
            {
                throw new InvalidOperationException("Unable to accomplish the semantic assert.");
            }

            if (!result.IsValid)
            {
                throw new SemanticAssertException(result.Reason ?? "No reason is provided.");
            }
        }

        /// <summary>
        /// Checks whether <paramref name="first"/> and <paramref name="second"/> string are semantically similar.
        /// It uses the kernel and OpenAI to check this semantically.
        /// <example>
        /// <code>
        /// SemanticAssert.Similar("This automobile is red", "The car is red") // returns true
        /// SemanticAssert.Similar("This tree is red", "The car is red") // returns false
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
        public void Similar(string first, string second)
        {
            SimilarAsync(first, second).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Checks whether <paramref name="first"/> and <paramref name="second"/> string are NOT semantically similar.
        /// It uses the kernel and OpenAI to check this semantically. It also describes the reason that they are not similar.
        /// <example>
        /// <code>
        /// SemanticAssert.NotSimilarAsync("This bicycle is red", "The car is red")
        /// // returns:
        /// {
        ///   IsValid: false,
        ///   Reason: "The first text describes a red bicycle, while the second text describes a red car. They are not semantically equivalent."
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
        public async Task NotSimilarAsync(string first, string second)
        {
            var result = await Semantic.AreSimilarAsync(first, second);

            if (result is null)
            {
                throw new SemanticAssertException("Unable to accomplish the semantic assert.");
            }

            if (result.IsValid)
            {
                throw new SemanticAssertException($"""
                    These are semantically similar:
                    [FIRST]: {first}
                    [SECOND]: {second} 
                    """);
            }
        }

        /// <summary>
        /// Checks whether <paramref name="first"/> and <paramref name="second"/> string are NOT semantically similar.
        /// It uses the kernel and OpenAI to check this semantically. It also describes the reason that they are not similar.
        /// <example>
        /// <code>
        /// SemanticAssert.NotSimilar("This bicycle is red", "The car is red")
        /// // returns:
        /// {
        ///   IsValid: false,
        ///   Reason: "The first text describes a red bicycle, while the second text describes a red car. They are not semantically equivalent."
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
        public void NotSimilar(string first, string second)
        {
            NotSimilarAsync(first, second).GetAwaiter().GetResult();
        }
    }
}
