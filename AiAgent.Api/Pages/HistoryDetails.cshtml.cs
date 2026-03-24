using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AiAgent.Api.Pages;

public sealed class HistoryDetailsModel : PageModel
{
    private readonly IAgentExecutionRepository _repository;

    public HistoryDetailsModel(IAgentExecutionRepository repository)
    {
        _repository = repository;
    }

    public AgentExecution? Execution { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Execution = await _repository.GetByIdAsync(id, cancellationToken);
        if (Execution is null)
        {
            return NotFound();
        }


        return Page();
    }
}
