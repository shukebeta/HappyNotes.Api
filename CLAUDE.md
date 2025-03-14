# HappyNotes.Api Development Guide

## Build & Test Commands
- Build solution: `dotnet build`
- Run all tests: `dotnet test`
- Run specific test project: `dotnet test tests/HappyNotes.Services.Tests`
- Run single test class: `dotnet test --filter "FullyQualifiedName~NoteServiceTests"`
- Run single test method: `dotnet test --filter "FullyQualifiedName~NoteServiceTests.Get_WithExistingPublicNote_ReturnsNote"`

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