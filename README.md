# 🤖 AI Engineering Workflow Agent

> **Transform Developer Intent into Actionable Engineering Workflows Using AI**

A production-grade **.NET 9** solution that leverages **Semantic Kernel** and pluggable LLMs to help engineering teams automatically triage incidents, plan fixes, generate code, and create comprehensive tests.

**Repository**: [GitHub](https://github.com/Shyam-Siddanthapu/AI-Engineering-Agent)  
**Language**: C# (.NET 9)  
**Created**: March 24, 2026  
**Status**: Active Development  
**Architecture**: Clean Architecture with Dependency Injection

---

## 🎯 What is This?

Unlike simple chatbots or code autocomplete tools, the **AI Engineering Workflow Agent** is an **end-to-end intelligent engineering assistant** that:

✅ Understands repository context and codebase structure  
✅ Interprets natural language developer intent  
✅ Generates structured, validated outputs  
✅ Simulates real code changes with full diffs  
✅ Supports multiple AI backends (Azure OpenAI, Groq, Ollama, Mock)  
✅ Provides a modern chat-like UI and CLI interface  
✅ Handles secure API key management  
✅ Validates changes for breaking modifications  

---

## 🚨 Problem Statement

Modern engineering teams waste significant time on repetitive tasks:

- **Incident Triage** - Correlating logs, tracing code paths, identifying root causes
- **Fix Planning** - Creating detailed action plans for code changes
- **Code Generation** - Writing boilerplate code and edge case handling
- **Test Creation** - Generating unit and integration test cases
- **Context Switching** - Moving between tools and documentation

**Existing AI solutions** focus on autocomplete rather than **full workflow orchestration**, leaving teams to manually integrate outputs and manage contexts.

**This project** provides an integrated AI-powered engineering workflow that handles the complete pipeline.

---

## ✨ Key Features

### 🧠 Intelligent Task Understanding
- Accepts **free-form input** in natural language
- Processes code snippets and SQL queries
- Understands technical intent and domain context
- Supports multi-turn conversations

### 🔌 Multi-Provider AI Architecture
Dynamic runtime selection of AI providers:
- **Mock Mode** - Fully functional simulation (no API keys, offline)
- **Azure OpenAI** - Enterprise-grade GPT models
- **Groq** - Fast inference with open models
- **Ollama** - Local LLM support (privacy-first)

**Provider-agnostic design** - Swap providers without changing code

### 📦 Structured AI Responses
Every response includes:
- **Summary** - High-level overview of findings
- **Detailed Explanation** - Technical deep-dive
- **Execution Steps** - Ordered action items
- **Code Changes** - Complete file modifications
- **Test Cases** - Unit and integration tests
- **Risk Assessment** - Potential issues and edge cases
- **Suggestions** - Recommendations for improvement

### 💻 Code Change Simulation
- Simulates real-world code modifications
- Supports multi-file changes
- Generates both updated code and test cases
- Validates for breaking changes
- Optional: Apply changes with automatic backups

### 📊 Diff Viewer (Developer Experience)
- Highlights additions and deletions
- Visual comparison of changes
- Copy-paste friendly format
- Syntax highlighting for code blocks

### 🎭 Mock AI Engine (Unique Feature)
Fully functional AI simulation layer:
- **No external dependencies** - Works offline
- **No API keys required** - Perfect for demos
- **Deterministic outputs** - Same input = same output
- **Development-friendly** - Test workflows without cost

### 🎨 Modern UI (Chat Experience)
Inspired by modern AI agents:
- Large, expandable input box
- Code-friendly multiline input
- Real-time loading indicators
- Responsive, mobile-friendly layout
- Message history with timestamps

### 🔐 Secure API Key Handling
Enterprise-grade security:
- API keys passed **per request only**
- **Never stored** in configuration files
- **Never logged** or exposed
- Environment variable support
- Secure string handling in memory

### 🚀 CLI Interface
Command-line access for automation:
```bash
dotnet run --project AiAgent.Cli -- execute --repo <url> --task "fix bug"
```

---

## 🏗️ Architecture

### Project Structure

```
AiEngineeringAgent/
├── AiAgent.Api/                 # HTTP API + Razor Pages UI
│   ├── Pages/                   # Razor Pages (Chat UI)
│   ├── Controllers/             # API endpoints
│   ├── appsettings.json        # Configuration
│   └── Program.cs              # Startup & DI
│
├── AiAgent.Cli/                # Command-line interface
│   ├── Commands/               # CLI command handlers
│   └── Program.cs              # CLI entry point
│
├── AiAgent.Core/               # Domain & business logic
│   ├── Models/                 # Domain entities
│   ├── Abstractions/           # Interfaces & contracts
│   ├── Orchestration/          # Workflow coordination
│   └── Extensions/             # Helper methods
│
├── AiAgent.Infrastructure/     # External services
│   ├── LLM/                    # LLM providers (OpenAI, Groq, Ollama)
│   ├── Repository/             # Git/Azure DevOps integration
│   ├── FileServices/           # File I/O and change application
│   ├── Logging/                # Structured logging
│   └── Execution/              # Code execution services
│
└── AiEngineeringAgent.sln      # Visual Studio solution
```

### Data Flow

```
1. User Input (API/CLI/UI)
   ↓
2. Task Parser (understand intent)
   ↓
3. Repository Analyzer (read codebase)
   ↓
4. Context Builder (prepare LLM context)
   ↓
5. LLM Orchestrator (call AI provider)
   ↓
6. Response Formatter (structure output)
   ↓
7. Validation Engine (check for breaking changes)
   ↓
8. Change Preview/Apply (simulate or commit)
   ↓
9. Response Return (API/CLI/UI)
```

### Design Patterns Used

| Pattern | Purpose | Example |
|---------|---------|---------|
| **Factory** | Create LLM providers dynamically | `LLMProviderFactory` |
| **Strategy** | Swap AI backends at runtime | `IAiProvider` interface |
| **Decorator** | Add logging/caching to services | `LoggingDecorator` |
| **Repository** | Abstract data access | `IRepository` |
| **Dependency Injection** | Loose coupling | ASP.NET Core DI Container |

---

## 🛠️ Technology Stack

### Core Framework
- **.NET 9** (Latest LTS)
- **ASP.NET Core** - Web API framework
- **Semantic Kernel** (Microsoft) - LLM orchestration
- **Razor Pages** - Modern web UI

### AI & LLM Integration
- **Azure OpenAI SDK** - Enterprise AI
- **Groq Client** - Fast inference
- **Ollama Integration** - Local LLM support
- **Semantic Kernel** - Provider abstraction

### Supporting Technologies
- **Serilog** - Structured logging
- **xUnit** - Unit testing
- **Moq** - Mocking framework
- **LibGit2Sharp** - Git operations
- **Azure DevOps REST API** - Repository access

### Development Tools
- **Visual Studio 2022** (or newer)
- **Git** (for repository operations)
- **Ollama** (for local LLM support)

---

## 🚀 Getting Started

### Prerequisites

#### Required
- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Visual Studio 2022** or newer
- **Git** - For repository operations

#### Optional (for local LLM)
- **Ollama** - [Download](https://ollama.ai)
- Running locally at `http://localhost:11434`

### Installation

#### Step 1: Clone Repository
```bash
git clone https://github.com/Shyam-Siddanthapu/AI-Engineering-Agent.git
cd AI-Engineering-Agent
```

#### Step 2: Restore Dependencies
```bash
dotnet restore
```

#### Step 3: Configure Settings
Edit `AiAgent.Api/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "LLMProvider": {
    "Type": "Mock"  // Options: Mock, AzureOpenAI, Groq, Ollama
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "Model": "llama2"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4"
  },
  "GitHub": {
    "Token": "your-github-token"
  },
  "FileApply": {
    "PreviewOnly": true,
    "BackupDirectory": ".backups",
    "CommitChanges": false
  }
}
```

#### Step 4: Run the Application

**Web API + UI:**
```bash
dotnet run --project AiAgent.Api
```
Open: `https://localhost:<port>`

**CLI:**
```bash
dotnet run --project AiAgent.Cli -- --help
```

---

## 📡 API Endpoints

### Core Endpoints

#### POST `/api/tasks/analyze`
Analyze a repository or codebase.

**Request:**
```json
{
  "repositoryUrl": "https://github.com/user/repo",
  "task": "Explain the authentication flow",
  "aiProvider": "AzureOpenAI",
  "apiKey": "sk-..."
}
```

**Response:**
```json
{
  "taskId": "task-123",
  "status": "completed",
  "summary": "Authentication uses JWT tokens...",
  "explanation": "Detailed technical explanation...",
  "steps": ["Step 1", "Step 2"],
  "codeChanges": [
    {
      "filePath": "src/Auth/Service.cs",
      "originalCode": "...",
      "modifiedCode": "...",
      "diff": "..."
    }
  ],
  "testCases": ["Test 1", "Test 2"],
  "risks": ["Risk 1"],
  "suggestions": ["Suggestion 1"]
}
```

#### POST `/api/tasks/fix-bug`
Generate a fix for a reported bug.

**Request:**
```json
{
  "repositoryUrl": "https://github.com/user/repo",
  "bugDescription": "NullReferenceException in OrderService.ProcessOrder",
  "stackTrace": "...",
  "aiProvider": "Ollama"
}
```

#### POST `/api/tasks/generate-tests`
Generate unit tests for specified code.

**Request:**
```json
{
  "repositoryUrl": "https://github.com/user/repo",
  "classPath": "src/Services/UserService.cs",
  "className": "UserService"
}
```

#### GET `/api/tasks/{taskId}`
Retrieve a task result.

**Response:** Same structure as analyze endpoint

---

## ⚙️ Configuration Guide

### LLM Provider Selection

**Mock (Default)**
```json
"LLMProvider": {
  "Type": "Mock"
}
```
- ✅ No API keys needed
- ✅ Offline operation
- ✅ Deterministic responses
- ✅ Perfect for development

**Azure OpenAI**
```json
"LLMProvider": {
  "Type": "AzureOpenAI"
},
"AzureOpenAI": {
  "Endpoint": "https://your-resource.openai.azure.com/",
  "ApiKey": "your-api-key",
  "DeploymentName": "gpt-4"
}
```
- ✅ Enterprise-grade models
- ✅ Production ready
- ✅ Azure integration

**Groq**
```json
"LLMProvider": {
  "Type": "Groq"
},
"Groq": {
  "ApiKey": "your-groq-api-key",
  "Model": "mixtral-8x7b"
}
```
- ✅ Fast inference
- ✅ Cost-effective
- ✅ Open models

**Ollama (Local)**
```json
"LLMProvider": {
  "Type": "Ollama"
},
"Ollama": {
  "BaseUrl": "http://localhost:11434",
  "Model": "llama2"
}
```
- ✅ Full privacy
- ✅ No API costs
- ✅ Offline capable

### File Application Settings

```json
"FileApply": {
  "PreviewOnly": true,           // Only preview changes
  "BackupDirectory": ".backups",  // Where to store backups
  "CommitChanges": false,         // Auto-commit to Git
  "ValidateChanges": true         // Check for breaking changes
}
```

---

## 📚 Usage Examples

### Example 1: Analyze Repository
```
Task: "Analyze this repository structure and explain the architecture"

Response:
- Summary: Repository uses Clean Architecture pattern
- Explanation: [Detailed architecture breakdown]
- Steps: [Key architectural components]
- Suggestions: [Improvements]
```

### Example 2: Fix a Bug
```
Task: "Fix null reference issue in OrderService"
Stack Trace: [provided]

Response:
- Root Cause: Missing null check on line 45
- Explanation: [Technical analysis]
- Code Changes: [Fixed code with null checks]
- Test Cases: [Unit tests for the fix]
- Risks: [Potential side effects]
```

### Example 3: Generate Tests
```
Task: "Generate comprehensive unit tests for UserService"

Response:
- Test Cases: [10+ test methods]
- Coverage: [Expected code coverage (%)]
- Code Changes: [Complete test file]
- Mock Setup: [Mock configurations]
```

### Example 4: Add Logging
```
Task: "Add structured logging throughout the OrderService"

Response:
- Changes: [File modifications with logging]
- Logging Levels: [Info, Warning, Error usage]
- Test Cases: [Tests for logging behavior]
```

---

## 🔐 Security Best Practices

### API Key Management
✅ **Pass per-request only** - Never store in appsettings.json for sensitive environments  
✅ **Use Environment Variables** - For production deployments  
✅ **Secure Logging** - API keys never logged or exposed  
✅ **HTTPS Only** - Always use HTTPS for API calls  

### Recommended Setup (Production)
```bash
# Set environment variables
export AZURE_OPENAI_KEY="sk-..."
export GITHUB_TOKEN="ghp_..."

# Pass via configuration
dotnet run --project AiAgent.Api
```

### Code Change Validation
- ✅ Validates for breaking changes
- ✅ Reviews for security issues
- ✅ Checks for dependency conflicts
- ✅ Optional: Preview before applying

---

## 🎨 UI/UX Features

### Chat Interface
- **Large Input Box** - Code-friendly multiline textarea
- **Message History** - View all past interactions
- **Loading States** - Real-time progress indicators
- **Response Formatting** - Syntax highlighting for code
- **Diff Viewer** - Side-by-side change comparison
- **Export** - Copy responses to clipboard

### Responsive Design
- ✅ Mobile-friendly layout
- ✅ Touch-optimized controls
- ✅ Dark mode support
- ✅ Accessible color contrast

---

## 📊 Design Decisions

### Why Provider-Agnostic?
- **Flexibility** - Use different providers for different workloads
- **Enterprise Ready** - Support corporate AI policies
- **Future-Proof** - Easy integration with new LLM providers
- **Cost Optimization** - Pick most cost-effective provider

### Why Structured Output?
- **Predictable UI** - Consistent rendering across all providers
- **Programmatic Access** - Easy integration with other tools
- **Better Debugging** - Clear identification of response sections
- **Scalable** - Supports complex multi-stage workflows

### Why Mock Engine?
- **Development** - Test without API costs
- **Demos** - Show functionality without setup
- **Education** - Learn system without dependencies
- **Offline** - Works anywhere, anytime

### Why Clean Architecture?
- **Testability** - Minimal external dependencies
- **Maintainability** - Clear separation of concerns
- **Extensibility** - Easy to add new features
- **Independent** - Frameworks are implementation details

---

## 🚀 Deployment

### Local Development
```bash
dotnet run --project AiAgent.Api
```

### Docker Deployment
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 as build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "AiAgent.Api.dll"]
```

### Azure App Service
```bash
# Publish to Azure
dotnet publish -c Release

# Deploy
az webapp deployment source config-zip \
  --resource-group myGroup \
  --name myApp \
  --src-path publish.zip
```

---

## 🧪 Testing

### Run Tests
```bash
dotnet test
```

### Coverage
```bash
dotnet test /p:CollectCoverage=true
```

### Test Projects
- `AiAgent.Core.Tests` - Domain logic tests
- `AiAgent.Infrastructure.Tests` - Service tests
- `AiAgent.Api.Tests` - API endpoint tests

---

## 📈 Performance & Scaling

### Benchmarks
- **Mock Provider**: < 100ms response time
- **Local Ollama**: 2-5 seconds (depends on model size)
- **Azure OpenAI**: 1-3 seconds
- **Groq**: 1-2 seconds (fastest)

### Optimization Tips
- ✅ Use Mock for development/testing
- ✅ Cache frequently accessed repository data
- ✅ Implement response streaming for large outputs
- ✅ Use Groq for production workloads with budget constraints

### Scaling Considerations
- **Stateless Design** - Horizontal scaling supported
- **Database** - Add for persistent task storage
- **Caching** - Implement Redis for response caching
- **Message Queue** - Use Azure Service Bus for async processing

---

## 🔮 Future Enhancements

### Planned Features
- 🔄 **Real Repository Integration** - Direct GitHub/Azure DevOps PR creation
- 🤝 **Multi-Agent Collaboration** - Multiple specialized agents working together
- 💾 **Vector Memory (RAG)** - Learn from organization's codebase patterns
- 📡 **Streaming Responses** - Real-time response streaming via WebSockets
- 🧪 **Deterministic Testing** - Validate generated code automatically
- 📊 **Analytics Dashboard** - Track AI effectiveness metrics
- 🔗 **IDE Integration** - VS Code and Visual Studio extensions

---

## ⚠️ Limitations

- **LLM Output** - AI-generated code requires review
- **Context Window** - Large codebases may exceed token limits
- **Validation** - Prompt-based validation, not static analysis
- **Merge Conflicts** - Limited handling of complex merge scenarios
- **Language Support** - Currently optimized for C# (extensible)

---

## 📚 Resources

### Documentation
- [Semantic Kernel Documentation](https://github.com/microsoft/semantic-kernel)
- [.NET 9 Guide](https://docs.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Docs](https://docs.microsoft.com/en-us/aspnet/core)

### AI Providers
- [Azure OpenAI](https://azure.microsoft.com/en-us/products/ai-services/openai-service/)
- [Groq Cloud](https://groq.com)
- [Ollama](https://ollama.ai)

### Tools
- [Visual Studio Download](https://visualstudio.microsoft.com)
- [Git Basics](https://git-scm.com/doc)

---

## 🤝 Contributing

We welcome contributions! Areas for enhancement:

- 🐛 **Bug Fixes** - Report and fix issues
- ✨ **Features** - Add new AI provider support
- 📖 **Documentation** - Improve guides and examples
- 🧪 **Tests** - Increase test coverage
- 🎨 **UI/UX** - Improve chat interface

### Contributing Guidelines
1. Fork the repository
2. Create feature branch: `git checkout -b feature/your-feature`
3. Commit changes: `git commit -am 'Add feature'`
4. Push branch: `git push origin feature/your-feature`
5. Open Pull Request

---

## 📊 Project Stats

- **Lines of Code**: 1,000+ (Core logic)
- **Test Coverage**: Growing
- **Architecture Patterns**: 6+ (Factory, Strategy, Repository, etc.)
- **Supported LLM Providers**: 4 (Mock, OpenAI, Groq, Ollama)
- **Deployment Options**: Local, Docker, Azure App Service

---

## 🎓 What Makes This Stand Out

✨ **Not just a chatbot** - Full AI workflow orchestration system  
✨ **Production-ready architecture** - Clean code, proper patterns  
✨ **Multi-provider support** - Flexibility in AI backend selection  
✨ **Developer experience focus** - Modern UI and API design  
✨ **Mock engine** - Complete functionality without external dependencies  
✨ **Comprehensive security** - Enterprise-grade API key handling  
✨ **Extensible design** - Easy to add new providers and services  

---

## 📞 Support & Contact

- **GitHub Issues**: [Report bugs](https://github.com/Shyam-Siddanthapu/AI-Engineering-Agent/issues)
- **GitHub Discussions**: [Ask questions](https://github.com/Shyam-Siddanthapu/AI-Engineering-Agent/discussions)

---

## 👤 Author

**Shyam Siddanthapu**  
Senior Software Engineer | .NET & Azure Specialist | AI Engineering Enthusiast  

- 🔗 **GitHub**: [@Shyam-Siddanthapu](https://github.com/Shyam-Siddanthapu)
- 💼 **LinkedIn**: [linkedin.com/in/shyam-sundar-ssr](https://linkedin.com/in/shyam-sundar-ssr)
- 📧 **Email**: syammyself@gmail.com

---

## ⭐ If You Like This Project

Give it a star on GitHub! Connect with the author and share your feedback. Your support motivates continued development.

---

## 📜 License

This project is open-source and available for personal and commercial use.

---

## 🎯 Quick Start Command

```bash
# Clone → Restore → Configure → Run
git clone https://github.com/Shyam-Siddanthapu/AI-Engineering-Agent.git && \
cd AI-Engineering-Agent && \
dotnet restore && \
dotnet run --project AiAgent.Api
# Open https://localhost:<port> in your browser
```

---

*Last updated: June 2026*  
*Built with ❤️ using .NET 9 and Semantic Kernel*