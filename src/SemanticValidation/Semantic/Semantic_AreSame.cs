using Microsoft.SemanticKernel;
using System.Text.Json;
using SemanticValidation.Models;

namespace SemanticValidation;

public partial class Semantic
{
    public async Task<SemanticValidationResult> AreSameAsync(string first, string second)
    {
        var skresult = (
            await AreSameSkFunc.InvokeAsync(TestKernel, new KernelArguments()
            {
                ["first_text"] = first,
                ["second_text"] = second
            })
        ).GetValue<string>() ?? "";

        var result = JsonSerializer.Deserialize<SemanticValidationResult>(skresult);

        if (result is null)
            throw new InvalidOperationException("Can not assert Same");

        return result;
    }
}