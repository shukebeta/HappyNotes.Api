# CI Testing Guide

## Overview

The project uses a layered testing approach with GitHub Actions to ensure fast feedback and comprehensive coverage.

## Test Categories

### Unit Tests
- **Purpose**: Fast feedback, no external dependencies
- **Filter**: `TestCategory!=Integration`
- **Runtime**: ~5-10 seconds
- **Trigger**: Every push and PR

### Integration Tests
- **Purpose**: Test with real external services (Redis)
- **Filter**: `TestCategory=Integration`
- **Runtime**: ~30-60 seconds
- **Dependencies**: Redis service container

## GitHub Actions Workflows

### 1. Main CI Pipeline (`.github/workflows/ci.yml`)
**Triggers**: Push to any branch, PR to master

**Jobs**:
- `unit-tests`: Fast unit test feedback
- `integration-tests`: Full integration testing with Redis
- `test-summary`: Consolidates results

**Parallel Execution**: Unit and integration tests run simultaneously for faster CI times.

### 2. PR Check (`.github/workflows/pr-check.yml`)
**Triggers**: Pull requests to master

**Jobs**:
- `quick-check`: Always runs unit tests for immediate feedback
- `integration-check`: Runs only when PR has `needs-integration-tests` label

## Local Development

### Running Tests Locally

```bash
# Unit tests only (fast)
dotnet test --filter "TestCategory!=Integration"

# Integration tests (requires Redis)
docker run --rm -d -p 6379:6379 --name test-redis redis:7-alpine
export REDIS_CONNECTION_STRING=localhost:6379
dotnet test --filter "TestCategory=Integration"
docker stop test-redis

# All tests
dotnet test
```

### Adding New Tests

1. **Unit Tests**: Default behavior, no special attributes needed
2. **Integration Tests**: Add `[Category("Integration")]` attribute

```csharp
[TestFixture]
[Category("Integration")]
public class MyServiceIntegrationTests
{
    // Tests requiring external services
}
```

## CI Optimization Features

### Fast PR Feedback
- Unit tests complete in ~10 seconds
- Immediate feedback on code quality issues
- Integration tests run in parallel or on-demand

### Resource Efficiency
- Redis container only starts for integration tests
- Automatic cleanup after test completion
- Health checks ensure service readiness

### Flexible Triggering
- Label-based integration test execution for PRs
- Full test suite on main branch pushes
- Artifact collection for test result analysis

## Troubleshooting

### Integration Tests Skipped
```
Skipped! - Failed: 0, Passed: 0, Skipped: 6
```
**Cause**: Redis not available
**Solution**: Check `REDIS_CONNECTION_STRING` or Docker service

### CI Integration Test Failures
1. Check Redis service health in GitHub Actions logs
2. Verify port mapping (6379:6379)
3. Confirm environment variable setting

### Local Redis Issues
```bash
# Check Redis connectivity
redis-cli -h localhost -p 6379 ping
# Should return: PONG

# Manual container management
docker run --rm -d -p 6379:6379 --name test-redis redis:7-alpine
docker logs test-redis
docker stop test-redis
```