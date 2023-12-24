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
    public partial class SemanticKernelAssert
    {
        private static Semantic? _semantic;
        private static Action<string>? OnLog { get; set; }
        private static Semantic Semantic
        {
            get => _semantic ?? throw new InvalidOperationException("KernelAssert has not initialized yet.");
            set => _semantic = value;
        }

        public static void Initialize(string deploymentName, string endpoint, string apiKey, Action<string>? onLog = null)
        {
            Semantic = new Semantic(deploymentName, endpoint, apiKey);
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
