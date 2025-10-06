# Contributing to HCX Integration SDKs

Thank you for your interest in contributing to the HCX Integration SDKs! This document provides guidelines and instructions for contributing.

## Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [Getting Started](#getting-started)
3. [Development Workflow](#development-workflow)
4. [Coding Standards](#coding-standards)
5. [Testing Guidelines](#testing-guidelines)
6. [Documentation](#documentation)
7. [Pull Request Process](#pull-request-process)
8. [Release Process](#release-process)

---

## Code of Conduct

### Our Pledge

We are committed to providing a welcoming and inclusive environment for all contributors.

### Expected Behavior

- Be respectful and considerate
- Welcome newcomers and help them get started
- Focus on constructive feedback
- Accept responsibility for mistakes

### Unacceptable Behavior

- Harassment or discrimination
- Trolling or insulting comments
- Publishing others' private information
- Other unprofessional conduct

---

## Getting Started

### Prerequisites

Ensure you have the following installed:

- **Node.js** 16+ and npm 8+
- **Python** 3.8+ and pip
- **Java** 11+ and Maven 3.6+
- **.NET** 6.0+ SDK
- **Git** 2.0+
- **Docker** and Docker Compose (for local development)

### Fork and Clone

```bash
# Fork the repository on GitHub
# Then clone your fork
git clone https://github.com/YOUR_USERNAME/integration-sdks.git
cd integration-sdks

# Add upstream remote
git remote add upstream https://github.com/HealthFlowEgy/integration-sdks.git
```

### Setup Development Environment

```bash
# Check tool versions
make version-check

# Install all dependencies
make install

# Start Docker services
make docker-up

# Run tests to verify setup
make test
```

---

## Development Workflow

### 1. Create a Feature Branch

```bash
# Update your fork
git checkout develop
git pull upstream develop

# Create feature branch
git checkout -b feature/your-feature-name
# or
git checkout -b fix/your-bug-fix
```

### 2. Make Changes

Follow the coding standards for your SDK:

- **Node.js**: ESLint + Prettier
- **Python**: PEP 8 + Black + isort
- **Java**: Google Java Style Guide
- **.NET**: Microsoft C# Coding Conventions

### 3. Write Tests

```bash
# Run tests for your SDK
make test-nodejs
make test-python
make test-java
make test-dotnet

# Check coverage
make test-coverage
```

### 4. Commit Changes

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```bash
# Format: <type>(<scope>): <subject>

git commit -m "feat(nodejs): add support for payment notices"
git commit -m "fix(python): resolve encryption key loading issue"
git commit -m "docs(java): update authentication examples"
git commit -m "chore(deps): update axios to 1.6.0"
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting)
- `refactor`: Code refactoring
- `test`: Test additions or changes
- `chore`: Maintenance tasks
- `perf`: Performance improvements
- `ci`: CI/CD changes

### 5. Push and Create PR

```bash
# Push to your fork
git push origin feature/your-feature-name

# Create PR on GitHub
# Use the PR template and fill in all sections
```

---

## Coding Standards

### Node.js/TypeScript

```javascript
// Use TypeScript for type safety
export interface HCXConfig {
  participantCode: string;
  authBasePath: string;
  // ...
}

// Use async/await, not callbacks
async function fetchData(): Promise<Data> {
  const response = await axios.get('/api/data');
  return response.data;
}

// Use meaningful variable names
const coverageEligibilityRequest = createRequest();
```

**Style:**
- 2 spaces for indentation
- Single quotes for strings
- Semicolons required
- Max line length: 100 characters

### Python

```python
# Follow PEP 8
from typing import Dict, Optional

class HCXIntegrator:
    """HCX SDK main class."""
    
    def __init__(self, config: Dict[str, str]) -> None:
        """Initialize the integrator.
        
        Args:
            config: Configuration dictionary
        """
        self.config = config
    
    async def check_eligibility(
        self, 
        request: Dict[str, str]
    ) -> Optional[Dict[str, str]]:
        """Check coverage eligibility."""
        # Implementation
        pass
```

**Style:**
- 4 spaces for indentation
- Max line length: 88 characters (Black default)
- Type hints for all functions
- Docstrings for all public methods

### Java

```java
// Follow Google Java Style Guide
public class HCXIntegrator {
    private final HCXConfig config;
    
    /**
     * Creates a new HCX integrator instance.
     *
     * @param config the configuration object
     */
    public HCXIntegrator(HCXConfig config) {
        this.config = Objects.requireNonNull(config);
    }
    
    /**
     * Checks coverage eligibility.
     *
     * @param request the eligibility request
     * @return the eligibility response
     * @throws HCXException if the request fails
     */
    public CompletableFuture<HCXResponse> checkEligibility(
            EligibilityRequest request) throws HCXException {
        // Implementation
    }
}
```

**Style:**
- 2 spaces for indentation
- Opening brace on same line
- Javadoc for all public methods
- Use Optional for nullable returns

### .NET/C#

```csharp
// Follow Microsoft C# conventions
public class HCXIntegrator
{
    private readonly HCXConfig _config;
    
    /// <summary>
    /// Initializes a new instance of the HCXIntegrator class.
    /// </summary>
    /// <param name="config">The configuration object.</param>
    public HCXIntegrator(HCXConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }
    
    /// <summary>
    /// Checks coverage eligibility asynchronously.
    /// </summary>
    /// <param name="request">The eligibility request.</param>
    /// <returns>The eligibility response.</returns>
    public async Task<HCXResponse> CheckEligibilityAsync(
        EligibilityRequest request)
    {
        // Implementation
    }
}
```

**Style:**
- 4 spaces for indentation
- Opening brace on new line
- PascalCase for public members
- camelCase with _ prefix for private fields
- XML documentation for all public APIs

---

## Testing Guidelines

### Test Structure

Each SDK should have:

1. **Unit Tests**: Test individual functions/methods
2. **Integration Tests**: Test SDK with mock HCX server
3. **Contract Tests**: Verify API contract compliance

### Writing Tests

#### Node.js (Jest)

```javascript
describe('HCXIntegrator', () => {
  let hcx: HCXIntegrator;
  
  beforeEach(() => {
    hcx = new HCXIntegrator(mockConfig);
  });
  
  it('should check coverage eligibility', async () => {
    const request = createMockRequest();
    const response = await hcx.coverageEligibility.check(request);
    
    expect(response.status).toBe('success');
    expect(response.correlation_id).toBeDefined();
  });
});
```

#### Python (pytest)

```python
@pytest.fixture
def hcx_client():
    return HCXIntegrator(mock_config)

def test_check_eligibility(hcx_client):
    request = create_mock_request()
    response = hcx_client.coverage_eligibility.check(request)
    
    assert response['status'] == 'success'
    assert 'correlation_id' in response
```

#### Java (JUnit 5)

```java
@Test
void shouldCheckCoverageEligibility() {
    HCXIntegrator hcx = new HCXIntegrator(mockConfig);
    EligibilityRequest request = createMockRequest();
    
    HCXResponse response = hcx.checkEligibility(request).join();
    
    assertEquals("success", response.getStatus());
    assertNotNull(response.getCorrelationId());
}
```

#### .NET (xUnit)

```csharp
[Fact]
public async Task ShouldCheckCoverageEligibility()
{
    var hcx = new HCXIntegrator(mockConfig);
    var request = CreateMockRequest();
    
    var response = await hcx.CheckEligibilityAsync(request);
    
    Assert.Equal("success", response.Status);
    Assert.NotNull(response.CorrelationId);
}
```

### Test Coverage

- Aim for **>80%** code coverage
- All public APIs must have tests
- Critical paths must have 100% coverage

### Running Tests

```bash
# Run all tests
make test

# Run specific SDK tests
make test-nodejs
make test-python
make test-java
make test-dotnet

# Run with coverage
make test-coverage

# Run integration tests
make test-integration
```

---

## Documentation

### Code Documentation

- Document all public APIs
- Include usage examples
- Explain complex logic
- Document error conditions

### README Updates

Update relevant README files:
- `README.md` (root)
- `javascript/README.md`
- `python/hcx-integrator/README.md`
- `java/hcx-integrator-sdk/README.md`
- `dot-net/README.md`

### API Documentation

Update OpenAPI specification if API changes:
- `docs/api-reference/openapi.yaml`

### Changelog

Update `CHANGELOG.md`:

```markdown
## [Unreleased]

### Added
- New feature description

### Changed
- Changed feature description

### Fixed
- Bug fix description

### Deprecated
- Deprecated feature description
```

---

## Pull Request Process

### Before Submitting

1. **Run all checks locally**:
   ```bash
   make ci-check
   ```

2. **Update documentation**

3. **Update CHANGELOG.md**

4. **Ensure tests pass**:
   ```bash
   make test
   ```

5. **Format code**:
   ```bash
   make format
   ```

### PR Checklist

- [ ] Code follows style guidelines
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] All CI checks pass
- [ ] No merge conflicts
- [ ] PR template filled out

### Review Process

1. **Automated checks** run on PR creation
2. **Code owner review** required
3. **Address feedback** and push updates
4. **Approval** from at least one maintainer
5. **Merge** by maintainer

### After Merge

- Delete your feature branch
- Update your fork:
  ```bash
  git checkout develop
  git pull upstream develop
  ```

---

## Release Process

Releases are managed by maintainers. Contributors can:

1. **Propose releases** by creating an issue
2. **Help with release notes** by updating CHANGELOG.md
3. **Test release candidates** when announced

### Versioning

We follow [Semantic Versioning](https://semver.org/):

- **Major (X.0.0)**: Breaking changes
- **Minor (X.Y.0)**: New features, backward compatible
- **Patch (X.Y.Z)**: Bug fixes, backward compatible

---

## Getting Help

### Resources

- **Documentation**: https://docs.hcx.example.com
- **GitHub Issues**: https://github.com/HealthFlowEgy/integration-sdks/issues
- **Discussions**: https://github.com/HealthFlowEgy/integration-sdks/discussions

### Contact

- **Email**: dev-team@healthflowegy.com
- **Slack**: #hcx-sdk-dev

---

## Recognition

Contributors will be recognized in:
- `CONTRIBUTORS.md` file
- Release notes
- Project README

Thank you for contributing to HCX Integration SDKs! 🎉
