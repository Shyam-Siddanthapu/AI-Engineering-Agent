using Microsoft.SemanticKernel;

namespace AiAgent.Infrastructure.Abstractions;

public interface IKernelFactory
{
    Kernel CreateKernel(IEnumerable<KernelPlugin>? plugins = null);
}
