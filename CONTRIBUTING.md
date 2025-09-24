# Contributing to qf4net

Thank you for your interest in contributing to qf4net! This document provides guidelines and information for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Making Changes](#making-changes)
- [Testing](#testing)
- [Submitting Changes](#submitting-changes)
- [Code Style](#code-style)
- [Release Process](#release-process)

## Code of Conduct

This project adheres to a code of conduct. By participating, you are expected to uphold this code. Please be respectful and professional in all interactions.

## Getting Started

1. Fork the repository on GitHub
2. Clone your fork locally
3. Set up the development environment
4. Create a branch for your changes
5. Make your changes
6. Test your changes
7. Submit a pull request

## Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- A code editor (Visual Studio, VS Code, JetBrains Rider, etc.)
- Git

### Local Setup

1. Clone your fork:
   ```bash
   git clone https://github.com/YOUR-USERNAME/qf4net.git
   cd qf4net
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

4. Run tests:
   ```bash
   dotnet test
   ```

### CI/CD Notes

- **Windows-specific examples**: Projects targeting `net8.0-windows` or using Windows Forms/WPF are excluded from CI/CD builds to ensure cross-platform compatibility
- **Cross-platform examples**: Console applications and cross-platform examples are built and tested in CI
- **Core library**: The main qf4net library and unit tests are fully cross-platform and run on Ubuntu, Windows, and macOS

## Making Changes

### Branch Naming

Use descriptive branch names:
- `feature/add-new-state-type` - for new features
- `fix/signal-registration-bug` - for bug fixes
- `docs/update-api-documentation` - for documentation updates
- `refactor/improve-dispatch-performance` - for refactoring

### Commit Messages

Follow conventional commit format:
- `feat: add support for composite states`
- `fix: resolve null reference in dispatch method`
- `docs: update state machine examples`
- `test: add unit tests for signal registration`
- `refactor: simplify event handling logic`

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/UnitTests/UnitTests.csproj
```

### Writing Tests

- All new features should include comprehensive unit tests
- Bug fixes should include regression tests
- Test files should be in the `tests/UnitTests` directory
- Use NUnit for testing framework
- Follow the existing test patterns and naming conventions

### Test Coverage

- Aim for high test coverage on new code
- Don't compromise test quality for coverage metrics
- Focus on testing behavior, not implementation details

## Submitting Changes

### Pull Request Process

1. Create a feature branch from `master`
2. Make your changes
3. Add or update tests as needed
4. Ensure all tests pass locally
5. Update documentation if needed
6. Submit a pull request

### Pull Request Guidelines

- Fill out the pull request template completely
- Link any relevant issues
- Include a clear description of the changes
- Ensure CI/CD checks pass
- Respond to review feedback promptly

### Review Process

1. Automated checks must pass (build, tests, code analysis)
2. At least one maintainer review is required
3. All feedback must be addressed or discussed
4. Changes will be squash-merged when approved

## Code Style

### C# Coding Standards

- Use C# 12 language features appropriately
- Follow standard .NET naming conventions
- Use PascalCase for public members
- Use camelCase for private fields with underscore prefix (`_fieldName`)
- Use meaningful names for variables and methods
- Add XML documentation comments for public APIs

### Code Organization

- Keep classes focused and cohesive
- Favor composition over inheritance
- Use interfaces to define contracts
- Minimize public surface area
- Group related functionality in namespaces

### Documentation

- Add XML documentation for all public APIs
- Include code examples in documentation
- Update README.md for new features
- Add inline comments for complex logic

## Performance Considerations

- qf4net is designed for high-performance scenarios
- Consider memory allocation patterns
- Profile performance-critical changes
- Maintain thread safety where appropriate
- Document performance characteristics

## Release Process

### Versioning

We use semantic versioning (SemVer):
- MAJOR: Breaking changes
- MINOR: New features, backwards compatible
- PATCH: Bug fixes, backwards compatible

### Release Timeline

- Bug fixes: Released as needed
- Minor features: Monthly releases
- Major versions: As needed for breaking changes

## Questions?

- Check existing [GitHub Issues](https://github.com/zdomokos/qf4net/issues)
- Start a [GitHub Discussion](https://github.com/zdomokos/qf4net/discussions)
- Review the [documentation](https://github.com/zdomokos/qf4net/wiki)

## Recognition

Contributors will be recognized in:
- Release notes
- GitHub contributors list
- Project documentation (where appropriate)

Thank you for contributing to qf4net!