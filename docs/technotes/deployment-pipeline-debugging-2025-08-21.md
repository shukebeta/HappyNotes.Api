# Technical Note: Resolving .NET Deployment Pipeline Issues

**Date**: August 21, 2025  
**Duration**: Several hours  
**Impact**: Critical - Production deployment failures  
**Resolution**: Successful - Stable deployment pipeline restored  

## Executive Summary

This technical note documents the investigation and resolution of complex .NET application deployment failures that initially appeared as simple dependency conflicts but revealed deeper issues with build cache pollution, deployment sequencing, and runtime environment mismatches. The resolution involved implementing comprehensive cache cleaning, deployment sequencing improvements, and migration to self-contained deployments.

## Initial Problem Statement

### Primary Issues
1. **Dependency Resolution Failure**: Production deployments failing with `FileNotFoundException: Could not load file or assembly 'Microsoft.Extensions.DependencyInjection.Abstractions, Version=8.0.0.0'`
2. **File Pollution**: 167MB of outdated dependency files (dating 2019-2023) being copied to production environments

### Symptoms
- Successful builds in development and staging environments  
- Consistent runtime crashes in production with missing assembly errors
- Large deployment artifacts containing irrelevant legacy dependencies
- Application startup failures with `ArgumentNullException` during service configuration

## Root Cause Analysis

### Primary Root Causes

**1. Build Cache Pollution**
- Local `src/*/bin` and `src/*/obj` directories contained accumulated dependencies from years of development
- Files dating from 2019-2023 included test frameworks, code coverage tools, and obsolete package versions
- `dotnet publish` copied entire bin directory contents, including irrelevant legacy files

**2. Runtime Environment Mismatch**
- Production server (`racknerd`): Incomplete .NET 8.0.5 runtime installation
- Staging server (`arm64`): Complete system .NET runtime
- Framework-dependent deployment assumed complete runtime libraries on target system
- Missing `Microsoft.Extensions.DependencyInjection.Abstractions 8.0.0.0` in production runtime

**3. Unsafe Deployment Sequencing**
- Services restarted before file deployment completed
- Directory cleaning performed while services were accessing files
- Configuration file replacement happening during service runtime

## Investigation Process

### Discovery Phase
1. **Environment Comparison**: Identified differences between staging (ARM64, complete runtime) and production (x64, incomplete runtime)
2. **Dependency Analysis**: Catalogued 167MB of legacy files in build output
3. **Core Dump Analysis**: Revealed specific missing assemblies in production runtime
4. **Timeline Reconstruction**: Traced deployment pipeline execution order issues

### Hypothesis Testing
1. **Self-contained vs Framework-dependent**: Tested deployment models
2. **Cache Cleaning**: Verified impact of removing build cache directories  
3. **Service Lifecycle**: Analyzed timing of stop/clean/deploy/start sequence
4. **Configuration Binding**: Investigated null reference exceptions during startup

## Solution Implementation

### Phase 1: Build Cache Cleanup
```yaml
- name: Clean project
  run: dotnet clean --configuration Release

- name: Clean build cache directories
  run: rm -rf src/*/bin src/*/obj

- name: Install dependencies
  run: dotnet restore
```

**Rationale**: Ensures clean dependency resolution by removing accumulated legacy artifacts

### Phase 2: Deployment Sequencing
```yaml
- name: Stop service before cleaning directory
  run: |
    export XDG_RUNTIME_DIR=/run/user/$(id -u)
    systemctl --user stop HappyNotes.Api.service || true

- name: Clean deployment directory
  run: rm -rf /var/www/HappyNotes.Api/*
```

**Rationale**: Prevents file access conflicts during deployment

### Phase 3: Self-contained Deployment
```yaml
- name: Publish
  run: dotnet publish -c Release --self-contained true --runtime linux-x64 --property:PublishDir=/var/www/HappyNotes.Api
```

**Rationale**: Eliminates runtime environment dependency mismatches

### Phase 4: Error-resilient Configuration
```csharp
var manticoreOptions = builder.Configuration.GetSection("ManticoreConnectionOptions").Get<ManticoreConnectionOptions>();
if (manticoreOptions != null)
{
    builder.Services.AddSingleton(manticoreOptions);
}
```

**Rationale**: Prevents startup crashes from configuration binding failures

## Results and Validation

### Success Metrics
- **Service Stability**: 22+ minutes continuous operation in production test environment
- **Memory Efficiency**: 110MB memory usage (optimized from previous higher usage)
- **Deployment Size**: Reduced from 167MB polluted artifacts to clean self-contained deployment
- **Startup Time**: Successful initialization without dependency errors

### Verification Process
1. **Service Status**: `systemctl --user status HappyNotes.Api.test.service` - Active (running)
2. **Network Connectivity**: Port 5013 listening and responding to health checks
3. **Log Analysis**: Clean startup logs without assembly resolution errors
4. **Resource Monitoring**: Stable CPU and memory usage patterns

## Lessons Learned

### Technical Insights
1. **Build Cache Hygiene**: Regular cleaning of `bin/obj` directories prevents legacy dependency pollution
2. **Deployment Models**: Self-contained deployments provide better isolation but increase artifact size
3. **Service Lifecycle Management**: Proper stop/clean/deploy/start sequencing prevents file access conflicts
4. **Runtime Environment Validation**: Production and staging environments must have consistent runtime configurations

### Process Improvements
1. **Incremental Problem Solving**: Complex issues often have multiple contributing factors requiring systematic elimination
2. **Environment Parity**: Differences between staging and production can mask underlying issues
3. **Deployment Pipeline Testing**: Comprehensive testing should include both framework-dependent and self-contained deployment models
4. **Error Handling**: Defensive coding practices (null checks) provide resilience against configuration failures

### Monitoring and Prevention
1. **Build Artifact Monitoring**: Regular auditing of deployment package contents
2. **Environment Consistency Checks**: Automated validation of runtime library availability
3. **Deployment Health Checks**: Post-deployment verification steps including service status and endpoint testing
4. **Cache Management Strategy**: Automated cleaning of build caches in CI/CD pipelines

## Future Recommendations

### Immediate Actions
1. Apply identical improvements to all deployment pipelines
2. Implement build artifact size monitoring and alerting
3. Establish environment consistency validation procedures

### Long-term Improvements
1. **Containerization**: Consider Docker deployment for complete environment isolation
2. **Infrastructure as Code**: Standardize runtime environment configurations
3. **Deployment Automation**: Implement blue-green or rolling deployment strategies
4. **Monitoring Enhancement**: Add detailed application performance monitoring

## Conclusion

What initially appeared as simple dependency conflicts revealed a complex interplay of build cache pollution, runtime environment mismatches, and deployment sequencing issues. The systematic investigation process and multi-layered solution approach successfully restored stable deployments while improving overall pipeline reliability.

The resolution demonstrates the importance of:
- Comprehensive root cause analysis beyond surface symptoms
- Environment consistency between staging and production
- Defensive coding practices for configuration handling  
- Proper deployment lifecycle management

This experience reinforces that thorough problem-solving often uncovers multiple contributing factors requiring coordinated solutions rather than simple fixes.

---

*"The most exciting phrase to hear in science, the one that heralds new discoveries, is not 'Eureka!' but 'That's funny...'"* - Isaac Asimov

*In our case, 167MB of 2019-era dependencies in a 2025 deployment was definitely "funny" - and led to discovering much deeper architectural insights.*