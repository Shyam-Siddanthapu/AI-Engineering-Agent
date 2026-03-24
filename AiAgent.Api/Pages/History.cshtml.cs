using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AiAgent.Api.Pages;

public sealed class HistoryModel : PageModel
{
    private readonly IAgentExecutionRepository _repository;

    public HistoryModel(IAgentExecutionRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<AgentExecution> Executions { get; private set; } = Array.Empty<AgentExecution>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Executions = await _repository.GetAllAsync(cancellationToken);
    }
}
