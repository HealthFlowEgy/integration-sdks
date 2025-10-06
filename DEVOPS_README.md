# HCX Integration SDKs - DevOps Infrastructure

This document describes the DevOps infrastructure, CI/CD pipelines, and automation setup for the HCX Integration SDKs project.

## Table of Contents

1. [Overview](#overview)
2. [CI/CD Pipelines](#cicd-pipelines)
3. [GitHub Actions Workflows](#github-actions-workflows)
4. [Docker Environment](#docker-environment)
5. [Development Workflow](#development-workflow)
6. [Secrets Management](#secrets-management)
7. [Monitoring & Observability](#monitoring--observability)
8. [Release Process](#release-process)

---

## Overview

The HCX Integration SDKs project uses a comprehensive DevOps setup that includes:

- **Multi-language CI/CD**: Automated pipelines for Node.js, Python, Java, and .NET
- **Automated Testing**: Unit, integration, and cross-SDK compatibility tests
- **Security Scanning**: Dependency vulnerability checks and code security analysis
- **Automated Publishing**: Package deployment to npm, PyPI, Maven Central, and NuGet
- **Docker Development Environment**: Containerized development and testing
- **Dependency Management**: Automated dependency updates via Dependabot
- **Code Quality**: Linting, formatting, and static analysis

---

## CI/CD Pipelines

### Pipeline Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Code Push/PR                             │
└────────────────────┬────────────────────────────────────────┘
                     │
        ┌────────────┴────────────┐
        │                         │
        ▼                         ▼
┌──────────────┐          ┌──────────────┐
│  Lint & Format│          │   Security   │
│     Check     │          │     Scan     │
└───────┬───────┘          └──────┬───────┘
        │                         │
        └────────────┬────────────┘
                     │
                     ▼
            ┌────────────────┐
            │   Unit Tests   │
            │  (Multi-version)│
            └────────┬────────┘
                     │
                     ▼
            ┌────────────────┐
            │     Build      │
            │   & Package    │
            └────────┬────────┘
                     │
                     ▼
            ┌────────────────┐
            │  Integration   │
            │     Tests      │
            └────────┬────────┘
                     │
        ┌────────────┴────────────┐
        │                         │
        ▼                         ▼
┌──────────────┐          ┌──────────────┐
│   Publish    │          │   GitHub     │
│  to Registry │          │   Release    │
└──────────────┘          └──────────────┘
```

### Workflow Triggers

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| `nodejs-ci.yml` | Push to main/develop, PRs | Node.js SDK CI/CD |
| `python-ci.yml` | Push to main/develop, PRs | Python SDK CI/CD |
| `java-ci.yml` | Push to main/develop, PRs | Java SDK CI/CD |
| `dotnet-ci.yml` | Push to main/develop, PRs | .NET SDK CI/CD |
| `integration-tests.yml` | Push, PRs, Daily schedule | Cross-SDK integration tests |

---

## GitHub Actions Workflows

### 1. Node.js SDK CI/CD (`nodejs-ci.yml`)

**Jobs:**
- **Lint**: ESLint and Prettier checks
- **Test**: Run tests on Node.js 16, 18, 20
- **Build**: TypeScript compilation and package validation
- **Security**: npm audit and Snyk scanning
- **Publish**: Deploy to npm and GitHub Packages

**Test Matrix:**
```yaml
strategy:
  matrix:
    node-version: [16, 18, 20]
```

### 2. Python SDK CI/CD (`python-ci.yml`)

**Jobs:**
- **Lint**: flake8, black, isort, mypy checks
- **Test**: Run tests on Python 3.8, 3.9, 3.10, 3.11
- **Build**: Build wheel and source distribution
- **Security**: Safety and Bandit scanning
- **Publish**: Deploy to PyPI and Test PyPI

**Test Matrix:**
```yaml
strategy:
  matrix:
    python-version: ['3.8', '3.9', '3.10', '3.11']
```

### 3. Java SDK CI/CD (`java-ci.yml`)

**Jobs:**
- **Lint**: Checkstyle and SpotBugs analysis
- **Test**: Run tests on Java 11, 17, 21
- **Build**: Maven build with SonarCloud analysis
- **Security**: OWASP Dependency Check and Snyk
- **Publish**: Deploy to Maven Central and GitHub Packages

**Test Matrix:**
```yaml
strategy:
  matrix:
    java-version: ['11', '17', '21']
```

### 4. .NET SDK CI/CD (`dotnet-ci.yml`)

**Jobs:**
- **Lint**: dotnet format verification
- **Test**: Run tests on .NET 6.0, 7.0, 8.0
- **Build**: Build and create NuGet package
- **Security**: Vulnerability scanning
- **Publish**: Deploy to NuGet and GitHub Packages

**Test Matrix:**
```yaml
strategy:
  matrix:
    dotnet-version: ['6.0.x', '7.0.x', '8.0.x']
```

### 5. Integration Tests (`integration-tests.yml`)

**Jobs:**
- **Setup Mock Server**: Start MockServer for testing
- **Test Each SDK**: Run integration tests for all SDKs
- **Cross-SDK Compatibility**: Test interoperability
- **Performance Tests**: Load testing with k6
- **Notify Results**: Send notifications to Slack

**Schedule:**
```yaml
schedule:
  - cron: '0 2 * * *'  # Daily at 2 AM UTC
```

---

## Docker Environment

### Services

The `docker-compose.yml` provides a complete development environment:

| Service | Port | Purpose |
|---------|------|---------|
| `mock-hcx-server` | 1080 | Mock HCX Gateway API |
| `postgres` | 5432 | Database for testing |
| `redis` | 6379 | Cache for testing |
| `nodejs-dev` | - | Node.js development container |
| `python-dev` | - | Python development container |
| `java-dev` | - | Java development container |
| `dotnet-dev` | - | .NET development container |
| `docs` | 8080 | Documentation server |
| `swagger-ui` | 8081 | API documentation |
| `prometheus` | 9090 | Metrics collection |
| `grafana` | 3000 | Monitoring dashboards |

### Quick Start

```bash
# Start all services
make docker-up

# View logs
make docker-logs

# Stop all services
make docker-down

# Clean up
make docker-clean
```

### Access Points

- **Mock HCX Server**: http://localhost:1080
- **Documentation**: http://localhost:8080
- **API Docs (Swagger)**: http://localhost:8081
- **Grafana Dashboard**: http://localhost:3000 (admin/admin)
- **Prometheus**: http://localhost:9090

---

## Development Workflow

### Initial Setup

```bash
# Clone repository
git clone https://github.com/HealthFlowEgy/integration-sdks.git
cd integration-sdks

# Check tool versions
make version-check

# Complete setup
make setup-dev
```

### Daily Development

```bash
# Install dependencies
make install

# Run linters
make lint

# Run tests
make test

# Build all SDKs
make build

# Run integration tests
make test-integration

# Generate coverage reports
make test-coverage
```

### Before Committing

```bash
# Run all CI checks locally
make ci-check

# Format code
make format

# Security scan
make security
```

### Working with Individual SDKs

```bash
# Node.js
make install-nodejs
make test-nodejs
make build-nodejs

# Python
make install-python
make test-python
make build-python

# Java
make install-java
make test-java
make build-java

# .NET
make install-dotnet
make test-dotnet
make build-dotnet
```

---

## Secrets Management

### Required GitHub Secrets

Configure these secrets in GitHub repository settings:

#### Package Registry Secrets

| Secret | Purpose | How to Get |
|--------|---------|------------|
| `NPM_TOKEN` | npm publishing | https://www.npmjs.com/settings/tokens |
| `PYPI_TOKEN` | PyPI publishing | https://pypi.org/manage/account/token/ |
| `TEST_PYPI_TOKEN` | Test PyPI publishing | https://test.pypi.org/manage/account/token/ |
| `NUGET_API_KEY` | NuGet publishing | https://www.nuget.org/account/apikeys |
| `OSSRH_USERNAME` | Maven Central username | https://issues.sonatype.org/ |
| `OSSRH_TOKEN` | Maven Central token | https://issues.sonatype.org/ |
| `GPG_PRIVATE_KEY` | Maven signing key | `gpg --export-secret-keys -a KEY_ID` |
| `GPG_PASSPHRASE` | GPG key passphrase | Your GPG passphrase |

#### Code Quality & Security

| Secret | Purpose | How to Get |
|--------|---------|------------|
| `SONAR_TOKEN` | SonarCloud analysis | https://sonarcloud.io/account/security |
| `SNYK_TOKEN` | Snyk security scanning | https://app.snyk.io/account |
| `CODECOV_TOKEN` | Code coverage reporting | https://codecov.io/ |

#### Notifications

| Secret | Purpose | How to Get |
|--------|---------|------------|
| `SLACK_WEBHOOK_URL` | Slack notifications | Slack App Incoming Webhooks |

### Setting Secrets

```bash
# Using GitHub CLI
gh secret set NPM_TOKEN --body "your-token-here"
gh secret set PYPI_TOKEN --body "your-token-here"
gh secret set NUGET_API_KEY --body "your-key-here"

# Or via GitHub UI
# Settings > Secrets and variables > Actions > New repository secret
```

---

## Monitoring & Observability

### Metrics Collection

Prometheus collects metrics from:
- SDK performance tests
- Integration test results
- Build times and success rates
- Dependency vulnerability counts

### Dashboards

Grafana provides pre-configured dashboards for:
- **CI/CD Pipeline Health**: Build success rates, duration trends
- **Test Coverage**: Coverage trends across SDKs
- **Security Posture**: Vulnerability counts and severity
- **Performance Metrics**: API response times, throughput

### Accessing Monitoring

```bash
# Start monitoring stack
make docker-up

# Access Grafana
open http://localhost:3000
# Default credentials: admin/admin

# Access Prometheus
open http://localhost:9090
```

---

## Release Process

### Versioning Strategy

We follow [Semantic Versioning](https://semver.org/):

- **Major (X.0.0)**: Breaking changes
- **Minor (x.Y.0)**: New features, backward compatible
- **Patch (x.y.Z)**: Bug fixes, backward compatible

### Release Workflow

#### 1. Prepare Release

```bash
# Update version in package files
# Node.js: javascript/package.json
# Python: python/hcx-integrator/setup.py
# Java: java/hcx-integrator-sdk/pom.xml
# .NET: dot-net/hcx-integrator-sdk/hcx-integrator-sdk.csproj

# Update CHANGELOG.md
# Commit changes
git add .
git commit -m "chore: bump version to X.Y.Z"
git push origin develop
```

#### 2. Create Release Branch

```bash
git checkout -b release/vX.Y.Z
git push origin release/vX.Y.Z
```

#### 3. Merge to Main

```bash
# Create PR from release branch to main
# After approval and merge, CI/CD automatically:
# 1. Runs all tests
# 2. Builds packages
# 3. Publishes to registries
# 4. Creates GitHub release
```

#### 4. Tag Release

```bash
git checkout main
git pull origin main
git tag -a vX.Y.Z -m "Release version X.Y.Z"
git push origin vX.Y.Z
```

### Manual Publishing

If needed, publish manually:

```bash
# Node.js
make publish-nodejs

# Python
make publish-python

# Java
make publish-java

# .NET
make publish-dotnet
```

---

## Troubleshooting

### Common Issues

#### 1. CI Build Failures

```bash
# Run CI checks locally
make ci-check

# Check specific SDK
make test-nodejs
make test-python
make test-java
make test-dotnet
```

#### 2. Docker Issues

```bash
# Clean and restart
make docker-clean
make docker-up

# Check service health
docker-compose ps
```

#### 3. Dependency Conflicts

```bash
# Update dependencies
make install

# Check for vulnerabilities
make security
```

#### 4. Test Failures

```bash
# Run with verbose output
cd javascript && npm test -- --verbose
cd python/hcx-integrator && pytest -v
cd java/hcx-integrator-sdk && mvn test -X
cd dot-net && dotnet test --logger "console;verbosity=detailed"
```

---

## Contributing

### Pull Request Checklist

- [ ] Code follows style guidelines
- [ ] All tests pass locally
- [ ] New tests added for new features
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] CI checks pass

### Code Review Process

1. Create feature branch from `develop`
2. Make changes and commit
3. Push and create PR to `develop`
4. Wait for CI checks to pass
5. Request review from maintainers
6. Address feedback
7. Merge after approval

---

## Support

For DevOps-related issues:

- **GitHub Issues**: https://github.com/HealthFlowEgy/integration-sdks/issues
- **Documentation**: https://docs.hcx.example.com
- **Slack**: #hcx-devops channel

---

## License

Apache 2.0 License - see [LICENSE](./LICENSE) file for details.
