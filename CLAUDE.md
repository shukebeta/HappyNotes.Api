# HappyNotes.Api Development Guide

## Build & Test Commands
- Build solution: `dotnet build`
- Run unit tests only: `dotnet test --filter "TestCategory!=Integration"`
- Run all tests (including integration): `dotnet test`
- Run integration tests only: `dotnet test --filter "TestCategory=Integration"`
- Run specific test project: `dotnet test tests/HappyNotes.Services.Tests`
- Run single test class: `dotnet test --filter "FullyQualifiedName~NoteServiceTests"`
- Run single test method: `dotnet test --filter "FullyQualifiedName~NoteServiceTests.Get_WithExistingPublicNote_ReturnsNote"`

### Integration Tests Setup
- Redis integration tests require a Redis instance
- Set `REDIS_CONNECTION_STRING` environment variable (defaults to `localhost:6379`)
- Tests are automatically skipped if Redis is unavailable
- Local Docker: `docker run --rm -p 6379:6379 redis:7-alpine`

### GitHub Actions CI
- **Unit tests**: Run on every push/PR (fast feedback)
- **Integration tests**: Run with Redis service container
- **Parallel execution**: Unit and integration tests run simultaneously
- **PR checks**: Quick unit test feedback for all PRs
- **Label-triggered**: Add `needs-integration-tests` label to run integration tests on PRs

## Code Style Guidelines
- **Naming**: PascalCase for classes/methods, camelCase for variables/parameters
- **Types**: Use explicit types; enable nullable reference types
- **Regex**: Use GeneratedRegex attributes with partial methods for better performance
- **Error Handling**: Use CustomException with EventId for domain errors; ArgumentException for invalid inputs
- **Testing**: Use NUnit with Moq framework; follow AAA pattern (Arrange-Act-Assert)
- **Architecture**: Follow repository pattern with services for business logic
- **Dependencies**: Use constructor injection with interfaces for testability

## Project Structure
- Api.Framework: Base classes and utilities
- HappyNotes.Common: Shared constants and extensions
- HappyNotes.Services: Business logic implementation
- HappyNotes.Entities: Database model classes
- HappyNotes.Repositories: Data access layer
- HappyNotes.Dto: Data transfer objects
- HappyNotes.Models: Request/response models

# Using Gemini CLI for Large Codebase Analysis

When analyzing large codebases or multiple files that might exceed context limits, use the Gemini CLI with its massive
context window. Use `gemini -p` to leverage Google Gemini's large context capacity.

## File and Directory Inclusion Syntax

Use the `@` syntax to include files and directories in your Gemini prompts. The paths should be relative to WHERE you run the
  gemini command:

### Examples:

**Single file analysis:**
gemini -p "@src/main.py Explain this file's purpose and structure"

Multiple files:
gemini -p "@package.json @src/index.js Analyze the dependencies used in the code"

Entire directory:
gemini -p "@src/ Summarize the architecture of this codebase"

Multiple directories:
gemini -p "@src/ @tests/ Analyze test coverage for the source code"

Current directory and subdirectories:
gemini -p "@./ Give me an overview of this entire project"

# Or use --all_files flag:
gemini --all_files -p "Analyze the project structure and dependencies"

Implementation Verification Examples

Check if a feature is implemented:
gemini -p "@src/ @lib/ Has dark mode been implemented in this codebase? Show me the relevant files and functions"

Verify authentication implementation:
gemini -p "@src/ @middleware/ Is JWT authentication implemented? List all auth-related endpoints and middleware"

Check for specific patterns:
gemini -p "@src/ Are there any React hooks that handle WebSocket connections? List them with file paths"

Verify error handling:
gemini -p "@src/ @api/ Is proper error handling implemented for all API endpoints? Show examples of try-catch blocks"

Check for rate limiting:
gemini -p "@backend/ @middleware/ Is rate limiting implemented for the API? Show the implementation details"

Verify caching strategy:
gemini -p "@src/ @lib/ @services/ Is Redis caching implemented? List all cache-related functions and their usage"

Check for specific security measures:
gemini -p "@src/ @api/ Are SQL injection protections implemented? Show how user inputs are sanitized"

Verify test coverage for features:
gemini -p "@src/payment/ @tests/ Is the payment processing module fully tested? List all test cases"

When to Use Gemini CLI

Use gemini -p when:
- Analyzing entire codebases or large directories
- Comparing multiple large files
- Need to understand project-wide patterns or architecture
- Current context window is insufficient for the task
- Working with files totaling more than 100KB
- Verifying if specific features, patterns, or security measures are implemented
- Checking for the presence of certain coding patterns across the entire codebase

Important Notes

- Paths in @ syntax are relative to your current working directory when invoking gemini
- The CLI will include file contents directly in the context
- No need for --yolo flag for read-only analysis
- Gemini's context window can handle entire codebases that would overflow Claude's context
- When checking implementations, be specific about what you're looking for to get accurate results
