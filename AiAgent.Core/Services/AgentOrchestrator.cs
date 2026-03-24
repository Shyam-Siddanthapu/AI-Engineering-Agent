using AiAgent.Core.Abstractions;
using AiAgent.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AiAgent.Core.Services;

public sealed class AgentOrchestrator : IAgentOrchestrator
{
    private readonly IRepositoryAnalyzer _repositoryAnalyzer;
    private readonly ILogReader _logReader;
    private readonly IConfigReader _configReader;
    private readonly IPlanner _planner;
    private readonly ICodeFixer _codeFixer;
    private readonly ITestGenerator _testGenerator;
    private readonly IImpactValidator _impactValidator;
    private readonly IChangeApplier _changeApplier;
    private readonly IIntentClassifier _intentClassifier;
    private readonly ILogger<AgentOrchestrator> _logger;

    public AgentOrchestrator(
        IRepositoryAnalyzer repositoryAnalyzer,
        ILogReader logReader,
        IConfigReader configReader,
        IPlanner planner,
        ICodeFixer codeFixer,
        ITestGenerator testGenerator,
        IImpactValidator impactValidator,
        IChangeApplier changeApplier,
        IIntentClassifier intentClassifier,
        ILogger<AgentOrchestrator> logger)
    {
        _repositoryAnalyzer = repositoryAnalyzer;
        _logReader = logReader;
        _configReader = configReader;
        _planner = planner;
        _codeFixer = codeFixer;
        _testGenerator = testGenerator;
        _impactValidator = impactValidator;
        _changeApplier = changeApplier;
        _intentClassifier = intentClassifier;
        _logger = logger;
    }

    public async Task<AgentResponse> ExecuteAsync(ProblemRequest request, CancellationToken cancellationToken)
    {
        var totalTimer = Stopwatch.StartNew();
        _logger.LogInformation("Starting agent workflow for {Title}", request.Title);

        var stepTimer = Stopwatch.StartNew();
        var repository = await _repositoryAnalyzer.AnalyzeAsync(request, cancellationToken);
        _logger.LogInformation("Repository analysis completed in {ElapsedMs} ms", stepTimer.ElapsedMilliseconds);

        stepTimer.Restart();
        var logs = await _logReader.ReadAsync(request, cancellationToken);
        _logger.LogInformation("Log ingestion completed in {ElapsedMs} ms", stepTimer.ElapsedMilliseconds);

        stepTimer.Restart();
        var config = await _configReader.ReadAsync(request, cancellationToken);
        _logger.LogInformation("Config ingestion completed in {ElapsedMs} ms", stepTimer.ElapsedMilliseconds);

        stepTimer.Restart();
        var analysis = await _planner.CreateAnalysisAsync(request, repository, logs, config, cancellationToken);
        _logger.LogInformation("Planning completed in {ElapsedMs} ms", stepTimer.ElapsedMilliseconds);

        stepTimer.Restart();
        var changes = await _codeFixer.GenerateFixesAsync(request, analysis, cancellationToken);
        _logger.LogInformation("Code generation completed in {ElapsedMs} ms", stepTimer.ElapsedMilliseconds);

        stepTimer.Restart();
        var tests = await _testGenerator.GenerateTestsAsync(request, analysis, changes, cancellationToken);
        _logger.LogInformation("Test generation completed in {ElapsedMs} ms", stepTimer.ElapsedMilliseconds);

        stepTimer.Restart();
        var validation = await _impactValidator.ValidateAsync(request, changes, tests, cancellationToken);
        _logger.LogInformation("Validation completed in {ElapsedMs} ms", stepTimer.ElapsedMilliseconds);

        stepTimer.Restart();
        var applied = request.ApplyChanges && changes.Count > 0
            ? await _changeApplier.ApplyAsync(changes, cancellationToken)
            : false;
        _logger.LogInformation("Change application completed in {ElapsedMs} ms", stepTimer.ElapsedMilliseconds);

        totalTimer.Stop();
        _logger.LogInformation("Agent workflow completed for {Title} in {ElapsedMs} ms", request.Title, totalTimer.ElapsedMilliseconds);

        var intent = _intentClassifier.Classify(request.Description).ToString();

        return new AgentResponse
        {
            Intent = intent,
            Summary = analysis.Summary,
            DetailedExplanation = string.Join(Environment.NewLine, analysis.Findings),
            Steps = analysis.Findings.ToList(),
            CodeChanges = changes.ToList(),
            TestCases = tests.Select(test => test.FilePath).ToList(),
            Risks = validation.ImpactNotes.ToList(),
            Suggestions = applied ? ["Changes applied successfully."] : ["Changes not applied."]
        };
    }
}
