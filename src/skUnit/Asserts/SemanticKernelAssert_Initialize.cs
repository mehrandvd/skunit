using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticValidation;
using skUnit.Exceptions;
using skUnit.Scenarios;

namespace skUnit
{
    /// <summary>
    /// This class is for testing SemanticKernel kernels and functions semantically. It contains various static methods
    /// that you can test kernels and functions with scenarios. Scenarios are some markdown files with a specific format.
    /// </summary>
    public partial class SemanticKernelAssert
    {
        private static Semantic? _semantic;
        private static Action<string>? OnLog { get; set; }
        private static Semantic Semantic
        {
            get => _semantic ?? throw new InvalidOperationException("KernelAssert has not initialized yet.");
            set => _semantic = value;
        }

        /// <summary>
        /// This class needs a SemanticKernel kernel to work.
        /// Using this constructor you can use an AzureOpenAI subscription to configure it.
        /// If you want to connect using an OpenAI client, you can configure your kernel
        /// as you like and pass your pre-configured kernel using the other constructor.
        /// </summary>
        /// <param name="deploymentName"></param>
        /// <param name="endpoint"></param>
        /// <param name="apiKey"></param>
        public static void Initialize(string deploymentName, string endpoint, string apiKey, Action<string>? onLog = null)
        {
            Semantic = new Semantic(deploymentName, endpoint, apiKey);
            OnLog = onLog;
        }

        /// <summary>
        /// This class needs a SemanticKernel kernel to work.
        /// Pass your pre-configured kernel to this constructor.
        /// </summary>
        /// <param name="kernel"></param>
        public static void Initialize(Kernel kernel, Action<string>? onLog = null)
        {
            Semantic = new Semantic(kernel);
            OnLog = onLog;

        }

        private static void Log(string? message = "")
        {
            if (OnLog is not null)
            {
                OnLog(message);
            }
        }
    }
}
