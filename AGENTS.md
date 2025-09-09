AGENTS - Agent Guidelines for HappyNotes.Api

Build / Test:
- `dotnet build`
- `dotnet test` (all tests); specific project: `dotnet test tests/HappyNotes.Services.Tests`
- Single test class: `dotnet test --filter "FullyQualifiedName~NoteServiceTests"`
- Single test method: `dotnet test --filter "FullyQualifiedName~NoteServiceTests.Get_WithExistingPublicNote_ReturnsNote"`
- Format / lint: `dotnet format` (respect `.editorconfig`)

Code style:
- Naming: PascalCase for classes/methods/properties; camelCase for locals/params.
- Types: prefer explicit types; enable nullable reference types; annotate nullability (`string?`).
- Imports: group `using System.*` first, then external libs, then project namespaces; remove unused `using`s.
- Structure/Formatting: follow `.editorconfig`; prefer file-scoped namespaces; keep files single-responsibility.
- Regex: prefer `[GeneratedRegex]` partial methods for complex patterns.
- Error handling: use `CustomException` + `EventId` for domain errors; use `ArgumentException`/`ArgumentNullException` for invalid args; do not swallow exceptions.
- Testing: NUnit + Moq; follow Arrange-Act-Assert; write small, deterministic unit tests.

Git & commits:
- Keep commits small and focused; avoid committing secrets; include tests for behavioral changes.

Cursor / Copilot rules:
- No `.cursor/rules/` or `.github/copilot-instructions.md` detected — follow `.editorconfig` and repository conventions.
