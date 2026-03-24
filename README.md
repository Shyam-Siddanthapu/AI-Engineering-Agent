# AI Engineering Workflow Agent (.NET + Semantic Kernel)

> Transforming developer intent into actionable engineering workflows using AI

---

##  Overview

The **AI Engineering Workflow Agent** is a production-grade .NET 9 solution that helps teams triage incidents, plan fixes, and generate code + tests using local or pluggable LLMs. It provides API, CLI, and Razor Pages web UI interfaces while following clean architecture for maintainability and extensibility.

Unlike simple chatbots or code generators, this system is designed as an **end-to-end engineering assistant** that can:

- Understand repository context
- Interpret developer intent
- Generate structured outputs
- Simulate code changes and diffs
- Support multiple AI providers

---

##  Problem Statement

Modern engineering teams spend too much time correlating logs, tracing code, and creating fixes. Existing AI tools often focus on autocomplete rather than full workflow orchestration. This project demonstrates a scalable, end-to-end workflow agent that can ingest problems, analyze repositories, propose fixes, generate tests, and validate impact—locally and securely.

---

##  Key Features

###  Intelligent Task Understanding

- Supports **free-form input**
- Accepts:
  - Natural language
  - Code snippets
  - SQL queries

---

###  Multi-Provider AI Architecture

Supports dynamic runtime selection of:

-  **Mock Mode** (no external dependency)
-  Azure OpenAI (pluggable)
-  Groq (pluggable)
-  Ollama (local LLM support)

> Designed with a **provider-agnostic architecture**

---

###  Structured AI Responses

Every response is returned in a structured format:

- Summary
- Detailed Explanation
- Execution Steps
- Code Changes
- Test Cases
- Risks
- Suggestions

---

###  Code Change Simulation

- Simulates real-world code modifications
- Supports multiple files
- Generates:
  - Updated code
  - Test cases

---

###  Diff Viewer (Developer Experience)

- Highlights:
  - ? Additions
  - ? Deletions
- Visual comparison of changes

---

###  Mock AI Engine (Unique Feature)

- Fully functional **AI simulation layer**
- No API keys required
- Enables:
  - Offline development
  - Demo scenarios
  - Deterministic outputs

---

### ? Modern UI (Chat-like Experience)

- Inspired by modern AI agents
- Features:
  - Large expandable input box
  - Code-friendly input
  - Loading indicators
  - Responsive layout

---

###  Secure API Key Handling

- API keys are:
  - Passed per request
  - Never stored
  - Never logged

---

##  Architecture

```
AiAgent.Api            -> HTTP API + Razor Pages UI
AiAgent.Cli            -> CLI interface
AiAgent.Core           -> Domain models + abstractions + orchestration
AiAgent.Infrastructure -> LLM, repo, file, logging, and execution services
```

### Flow

1. Receive task via API/CLI/UI
2. Analyze repository + logs
3. Generate plan (LLM-driven)
4. Build context + generate code + tests
5. Validate for breaking changes
6. Optionally apply changes with backups

---

##  Tech Stack

- **.NET 9 (ASP.NET Core)**
- **Semantic Kernel (Microsoft)**
- Razor Pages (UI)
- Clean Architecture
- Dependency Injection

---

##  Getting Started

###  Prerequisites

- .NET 9 SDK
- Visual Studio 2022 or newer
- Ollama running locally (default `http://localhost:11434`)
- Git (optional for apply/commit)

---

###  Run the Application

```bash
dotnet run --project AiAgent.Api
```

Open in browser:

```
https://localhost:<port>
```

---

###  CLI

```bash
dotnet run --project AiAgent.Cli -- execute --repo <url> --task "fix bug"
```

---

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

---

##  Example Tasks

Try these in the UI:

```
Analyze this repository
Explain the technical workflow of this system
Fix null reference issue in OrderService
Generate unit tests for UserService
Add logging to OrderService
```

---

##  Design Decisions
### Why Provider-Agnostic?

- Flexibility across environments
- Enterprise-ready design
- Easy integration with any LLM

---

### Why Structured Output?

- Predictable UI rendering
- Better debugging
- Scalable architecture

---

##  What Makes This Project Stand Out

- Not just a chatbot ? **Full AI workflow system**
- Simulates real engineering tasks
- Supports multiple AI backends
- Designed with **production-level architecture**
- Focus on **developer experience + UX**

---

##  Future Enhancements

- Real repository integration (GitHub / Azure DevOps)
- Automatic PR creation with CI validation
- Vector memory (RAG)
- Streaming responses
- Multi-agent collaboration

---

##  Limitations

- LLM output is probabilistic and may require review
- Code generation quality depends on available context
- Current validation is prompt-based, not deterministic static analysis
- Repository clone and apply are basic (no merge conflict handling)

---

##  Free Resources Implementation

- Local Ollama for LLM inference (no cloud cost)
- Semantic Kernel (open-source)
- ASP.NET Core, xUnit, Moq, and Serilog (open-source)
- GitHub/Azure DevOps APIs for repository access

---

##  Keeping this README Current

This file should be updated whenever new workflows, endpoints, or services are added. Keep the features, architecture, and usage steps aligned with the latest release.

---

## ? Author

**Shyam**
Senior Software Engineer (.NET / Azure)

---

## If you like this project

Give it * on GitHub and feel free to connect!
