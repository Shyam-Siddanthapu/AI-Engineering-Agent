# AI Engineering Workflow Agent

## Project overview
AI Engineering Workflow Agent is a production-grade .NET solution that helps teams triage incidents, plan fixes, and generate code + tests using local LLMs. It provides API, CLI, and web UI interfaces while following clean architecture for maintainability and extensibility.

## Problem statement
Modern engineering teams spend too much time correlating logs, tracing code, and creating fixes. Existing AI tools often focus on autocomplete rather than full workflow orchestration. This project demonstrates a scalable, end-to-end workflow agent that can ingest problems, analyze repositories, propose fixes, generate tests, and validate impact—locally and securely.

## Features
- API, CLI, and Razor Pages web UI
- Local LLM integration (Ollama) with Semantic Kernel
- Repository integration (GitHub + Azure DevOps)
- Code context builder (semantic file selection)
- Planner, executor, code generator, test generator, and validation agent
- Log analysis agent for root cause detection
- Optional apply changes with backups and git commits
- Clean architecture layering and DI throughout

## Architecture
```
AiAgent.Api           -> HTTP API + Razor Pages UI
AiAgent.Cli           -> CLI interface
AiAgent.Core          -> Domain models + abstractions + orchestration
AiAgent.Infrastructure-> LLM, repo, file, logging, and execution services
```

### Flow
1. Receive task via API/CLI/UI
2. Analyze repository + logs
3. Generate plan (LLM-driven)
4. Build context + generate code + tests
5. Validate for breaking changes
6. Optionally apply changes with backups

## How to run
### Prerequisites
- .NET 9 SDK
- Ollama running locally (default `http://localhost:11434`)
- Git (optional for apply/commit)

### API + Web UI
```
dotnet run --project AiAgent.Api
```
Open `https://localhost:<port>`

### CLI
```
dotnet run --project AiAgent.Cli -- execute --repo <url> --task "fix bug"
```

### Configuration (appsettings.json example)
```
Ollama:
  BaseUrl: "http://localhost:11434"
  Model: "llama3"
GitHub:
  Token: "<token>"
AzureDevOps:
  Token: "<token>"
FileApply:
  PreviewOnly: true
  BackupDirectory: ".backups"
  CommitChanges: false
```

## Example queries
- "Fix order total rounding issue"
- "Investigate 500 error in checkout"
- "Add retry logic to payment processor"

## Screenshots
- `docs/screenshots/home.png` (placeholder)
- `docs/screenshots/results.png` (placeholder)

## Why it is different from GitHub Copilot or other AI agents
- Focuses on full workflow orchestration (plan ? context ? code ? tests ? validation ? apply)
- Built for local LLMs and private repositories
- Clean architecture with explicit contracts for every agent stage
- Extensible services for repos, planners, validators, and executors

## Future possibilities
- Multi-repo orchestration with dependency graphs
- Automatic PR creation with CI validation
- Long-running workflow state + human approval gates
- Retrieval from observability platforms (App Insights, Datadog)
- More specialized agents (security, performance, compliance)

## Limitations
- LLM output is probabilistic and may require review
- Code generation quality depends on available context
- Current validation is prompt-based, not deterministic static analysis
- Repository clone and apply are basic (no merge conflict handling)

## Free resources implementation
- Local Ollama for LLM inference (no cloud cost)
- Semantic Kernel (open-source)
- ASP.NET Core, xUnit, Moq, and Serilog (open-source)
- GitHub/Azure DevOps APIs for repository access

## Keeping this README current
This file should be updated whenever new workflows, endpoints, or services are added. Keep the features, architecture, and usage steps aligned with the latest release.
