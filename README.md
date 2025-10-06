# HCX Integration SDKs

[![Build Status](https://github.com/HealthFlowEgy/integration-sdks/workflows/CI/badge.svg)](https://github.com/HealthFlowEgy/integration-sdks/actions)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)
[![Node.js SDK](https://img.shields.io/npm/v/hcx-integrator-sdk)](https://www.npmjs.com/package/hcx-integrator-sdk)
[![Python SDK](https://img.shields.io/pypi/v/hcxintegrator)](https://pypi.org/project/hcxintegrator/)
[![Java SDK](https://img.shields.io/maven-central/v/io.hcxprotocol/hcx-integrator-sdk)](https://search.maven.org/artifact/io.hcxprotocol/hcx-integrator-sdk)

Official integration SDKs for the **Health Claims Exchange (HCX) Protocol v0.9**. These SDKs enable healthcare providers, payors, and other participants to integrate with the HCX network seamlessly.

---

## 🚀 Quick Start

### Node.js

```bash
npm install hcx-integrator-sdk
```

```javascript
const { HCXIntegrator } = require('hcx-integrator-sdk');

const hcx = new HCXIntegrator({
  participantCode: 'provider001@hcx-dev',
  authBasePath: 'https://api.hcx.example.com/auth',
  protocolBasePath: 'https://api.hcx.example.com/v0.9',
  username: 'your_username',
  password: 'your_password',
  encryptionPrivateKeyPath: './keys/private_key.pem',
  igUrl: 'https://ig.hcxprotocol.io/v0.9'
});

// Check coverage eligibility
const response = await hcx.coverageEligibility.check({
  recipientCode: 'payor001@hcx-dev',
  fhirBundle: eligibilityBundle,
  recipientPublicKey: payorPublicKey
});
```

### Python

```bash
pip install hcxintegrator
```

```python
from hcxintegrator import HCXIntegrator

hcx = HCXIntegrator({
    'participant_code': 'provider001@hcx-dev',
    'auth_base_path': 'https://api.hcx.example.com/auth',
    'protocol_base_path': 'https://api.hcx.example.com/v0.9',
    'username': 'your_username',
    'password': 'your_password',
    'encryption_private_key_path': './keys/private_key.pem',
    'ig_url': 'https://ig.hcxprotocol.io/v0.9'
})

# Check coverage eligibility
response = hcx.coverage_eligibility.check({
    'recipient_code': 'payor001@hcx-dev',
    'fhir_bundle': eligibility_bundle,
    'recipient_public_key': payor_public_key
})
```

### Java

```xml
<dependency>
    <groupId>io.hcxprotocol</groupId>
    <artifactId>hcx-integrator-sdk</artifactId>
    <version>1.0.8</version>
</dependency>
```

```java
HCXConfig config = new HCXConfig(
    "provider001@hcx-dev",
    "https://api.hcx.example.com/auth",
    "https://api.hcx.example.com/v0.9",
    "your_username",
    "your_password",
    "./keys/private_key.pem",
    "https://ig.hcxprotocol.io/v0.9"
);

HCXIntegrator hcx = new HCXIntegrator(config);

// Check coverage eligibility
HCXResponse response = hcx.checkEligibility(eligibilityRequest).get();
```

### .NET

```bash
dotnet add package HCX.Integrator.SDK
```

```csharp
var config = new HCXConfig
{
    ParticipantCode = "provider001@hcx-dev",
    AuthBasePath = "https://api.hcx.example.com/auth",
    ProtocolBasePath = "https://api.hcx.example.com/v0.9",
    Username = "your_username",
    Password = "your_password",
    EncryptionPrivateKeyPath = "./keys/private_key.pem",
    IgUrl = "https://ig.hcxprotocol.io/v0.9"
};

var hcx = new HCXIntegrator(config);

// Check coverage eligibility
var response = await hcx.CheckEligibilityAsync(eligibilityRequest);
```

---

## 📚 Documentation

- **[Getting Started Guide](docs/getting-started/quickstart.md)** - Quick start guide for new users
- **[API Reference](docs/api-reference/)** - Complete API documentation
- **[Integration Guide](docs/integration-guide/)** - Detailed integration instructions
- **[FHIR Bundle Creation](docs/managing-payload/)** - How to create FHIR bundles
- **[DevOps Documentation](DEVOPS_README.md)** - CI/CD and infrastructure guide
- **[Contributing Guide](CONTRIBUTING.md)** - How to contribute to this project

---

## 🛠️ Available SDKs

| Language | Version | Status | Documentation | Package |
|----------|---------|--------|---------------|---------|
| **Node.js** | 1.0.8 | ✅ Stable | [Docs](javascript/README.md) | [npm](https://www.npmjs.com/package/hcx-integrator-sdk) |
| **Python** | 0.0.0 | 🚧 In Development | [Docs](python/hcx-integrator/README.md) | [PyPI](https://pypi.org/project/hcxintegrator/) |
| **Java** | 1.0.8 | ✅ Stable | [Docs](java/hcx-integrator-sdk/README.md) | [Maven Central](https://search.maven.org/artifact/io.hcxprotocol/hcx-integrator-sdk) |
| **.NET** | - | 🚧 In Development | [Docs](dot-net/README.md) | [NuGet](https://www.nuget.org/packages/HCX.Integrator.SDK/) |

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Your Application                      │
│            (Provider/Payor/Beneficiary System)           │
└────────────────────┬────────────────────────────────────┘
                     │
                     │ HCX SDK
                     ▼
┌─────────────────────────────────────────────────────────┐
│              HCX Integration SDK (Any Language)          │
│  ┌─────────────┬──────────────┬────────────────────┐   │
│  │ Auth Module │ Crypto Module│ FHIR Module        │   │
│  ├─────────────┼──────────────┼────────────────────┤   │
│  │ API Client  │ Validation   │ Error Handling     │   │
│  └─────────────┴──────────────┴────────────────────┘   │
└────────────────────┬────────────────────────────────────┘
                     │
                     │ HTTPS + JWT + JWE
                     ▼
┌─────────────────────────────────────────────────────────┐
│                   HCX Gateway Platform                   │
│         (Health Claims Exchange Network)                 │
└─────────────────────────────────────────────────────────┘
```

---

## ✨ Features

### Core Capabilities

- ✅ **Coverage Eligibility Check** - Verify beneficiary coverage before treatment
- ✅ **Pre-Authorization** - Request approval for planned procedures
- ✅ **Claims Submission** - Submit claims for adjudication
- ✅ **Payment Notices** - Handle payment requests and notifications
- ✅ **Communication** - Intra-cycle communication and queries
- ✅ **Status Tracking** - Track request status in real-time
- ✅ **Notifications** - Subscribe to event notifications

### Security & Compliance

- 🔒 **End-to-End Encryption** - JWE-based payload encryption
- 🔑 **JWT Authentication** - Secure token-based authentication
- 🛡️ **FHIR R4 Compliance** - Standards-compliant data exchange
- ✅ **Request Validation** - Automatic payload validation
- 📝 **Audit Trails** - Complete request/response logging

### Developer Experience

- 📦 **Easy Installation** - Available on npm, PyPI, Maven Central, NuGet
- 📖 **Comprehensive Documentation** - Detailed guides and examples
- 🧪 **Testing Tools** - Mock server and test utilities
- 🐳 **Docker Support** - Containerized development environment
- 🔄 **CI/CD Ready** - Automated testing and deployment

---

## 🚦 Supported Operations

### Coverage Eligibility

```javascript
// Check if patient is covered
await hcx.coverageEligibility.check(request);

// Handle eligibility response
await hcx.coverageEligibility.onCheck(encryptedPayload);
```

### Pre-Authorization

```javascript
// Submit pre-auth request
await hcx.preAuth.submit(request);

// Handle pre-auth response
await hcx.preAuth.onSubmit(encryptedPayload);
```

### Claims

```javascript
// Submit claim
await hcx.claim.submit(request);

// Handle claim response
await hcx.claim.onSubmit(encryptedPayload);
```

### Communication

```javascript
// Request additional information
await hcx.communication.request(request);

// Respond to communication request
await hcx.communication.onRequest(encryptedPayload);
```

### Status & Notifications

```javascript
// Check request status
await hcx.status.check(correlationId);

// Subscribe to notifications
await hcx.notification.subscribe({
  topicCode: 'claim-status',
  deliveryChannel: 'webhook',
  endpoint: 'https://your-app.com/webhook'
});
```

---

## 💻 Development Setup

### Prerequisites

- **Node.js** 16+ and npm 8+
- **Python** 3.8+ and pip
- **Java** 11+ and Maven 3.6+
- **.NET** 6.0+ SDK
- **Docker** and Docker Compose
- **Git** 2.0+

### Quick Setup

```bash
# Clone the repository
git clone https://github.com/HealthFlowEgy/integration-sdks.git
cd integration-sdks

# Run the automated setup script
./scripts/setup-dev.sh

# Or manually install dependencies
make install

# Start Docker services (mock HCX server, databases, monitoring)
make docker-up

# Run tests
make test
```

### Available Commands

```bash
make help              # Display all available commands
make install           # Install all SDK dependencies
make build             # Build all SDKs
make test              # Run all tests
make lint              # Run linters
make format            # Format code
make security          # Run security scans
make docker-up         # Start Docker services
make docker-down       # Stop Docker services
make test-integration  # Run integration tests
make test-coverage     # Generate coverage reports
make ci-check          # Run all CI checks locally
make clean             # Clean all build artifacts
```

### Docker Services

When you run `make docker-up`, the following services are available:

| Service | Port | Purpose |
|---------|------|---------|
| Mock HCX Server | 1080 | Mock HCX Gateway API |
| Documentation | 8080 | Documentation website |
| Swagger UI | 8081 | API documentation |
| Grafana | 3000 | Monitoring dashboards |
| Prometheus | 9090 | Metrics collection |
| PostgreSQL | 5432 | Database for testing |
| Redis | 6379 | Cache for testing |

---

## 🧪 Testing

### Unit Tests

```bash
# Run all unit tests
make test

# Run tests for specific SDK
make test-nodejs
make test-python
make test-java
make test-dotnet
```

### Integration Tests

```bash
# Run integration tests with mock HCX server
make test-integration
```

### Coverage Reports

```bash
# Generate coverage reports for all SDKs
make test-coverage
```

---

## 🔐 Security

### Encryption Keys

Generate RSA key pairs for encryption:

```bash
# Generate private key
openssl genrsa -out private_key.pem 2048

# Generate public key
openssl rsa -in private_key.pem -pubout -out public_key.pem
```

### Environment Variables

Create a `.env` file:

```env
HCX_PARTICIPANT_CODE=provider001@hcx-dev
HCX_AUTH_BASE_URL=https://api.hcx.example.com/auth
HCX_PROTOCOL_BASE_URL=https://api.hcx.example.com/v0.9
HCX_USERNAME=your_username
HCX_PASSWORD=your_password
HCX_ENCRYPTION_PRIVATE_KEY_PATH=./keys/private_key.pem
HCX_IG_URL=https://ig.hcxprotocol.io/v0.9
```

### Security Best Practices

- ✅ Never commit credentials or private keys
- ✅ Use environment variables for configuration
- ✅ Rotate encryption keys every 90 days
- ✅ Use HTTPS for all API calls
- ✅ Implement token refresh logic
- ✅ Validate all incoming payloads

---

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Quick Contribution Steps

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Run tests (`make test`)
5. Commit your changes (`git commit -m 'feat: add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### Coding Standards

- **Node.js/TypeScript**: ESLint + Prettier
- **Python**: PEP 8 + Black + isort
- **Java**: Google Java Style Guide
- **.NET**: Microsoft C# Coding Conventions

---

## 📋 Roadmap

### Current Version (v1.0.x)

- ✅ Node.js SDK
- ✅ Java SDK
- ✅ Basic Python SDK structure
- ✅ Basic .NET SDK structure
- ✅ Coverage eligibility checks
- ✅ Pre-authorization
- ✅ Claims submission

### Upcoming (v2.0.0)

- 🚧 Enhanced Python SDK with full HCX Protocol v0.9 support
- 🚧 Enhanced .NET SDK with full HCX Protocol v0.9 support
- 🚧 Payment notices implementation
- 🚧 Enhanced error handling
- 🚧 Retry mechanisms
- 🚧 Rate limiting support
- 🚧 Webhook support for callbacks

### Future

- 📅 Go SDK
- 📅 Ruby SDK
- 📅 PHP SDK
- 📅 GraphQL API support
- 📅 Real-time status updates via WebSockets
- 📅 Enhanced monitoring and analytics

---

## 📊 CI/CD & DevOps

This project uses comprehensive CI/CD pipelines for:

- ✅ **Automated Testing** - Unit, integration, and cross-SDK tests
- ✅ **Code Quality** - Linting, formatting, and static analysis
- ✅ **Security Scanning** - Dependency vulnerability checks
- ✅ **Automated Publishing** - npm, PyPI, Maven Central, NuGet
- ✅ **Monitoring** - Prometheus & Grafana dashboards
- ✅ **Dependency Updates** - Automated Dependabot PRs

See [DEVOPS_README.md](DEVOPS_README.md) for complete DevOps documentation.

---

## 📄 License

This project is licensed under the Apache 2.0 License - see the [LICENSE](LICENSE) file for details.

---

## 🆘 Support

### Documentation

- **Getting Started**: [docs/getting-started/](docs/getting-started/)
- **API Reference**: [docs/api-reference/](docs/api-reference/)
- **Integration Guide**: [docs/integration-guide/](docs/integration-guide/)
- **DevOps Guide**: [DEVOPS_README.md](DEVOPS_README.md)

### Community

- **GitHub Issues**: [Report bugs or request features](https://github.com/HealthFlowEgy/integration-sdks/issues)
- **GitHub Discussions**: [Ask questions and share ideas](https://github.com/HealthFlowEgy/integration-sdks/discussions)
- **Email**: dev-team@healthflowegy.com

### Resources

- **HCX Protocol Specification**: https://docs.hcxprotocol.io/
- **FHIR R4 Documentation**: https://hl7.org/fhir/R4/
- **Egyptian HCX Platform**: https://hcx.egypt.gov.eg/

---

## 🙏 Acknowledgments

- **HCX Protocol Team** - For the protocol specification
- **FHIR Community** - For the FHIR R4 standard
- **Contributors** - For their valuable contributions

---

## 📈 Project Status

![GitHub stars](https://img.shields.io/github/stars/HealthFlowEgy/integration-sdks?style=social)
![GitHub forks](https://img.shields.io/github/forks/HealthFlowEgy/integration-sdks?style=social)
![GitHub issues](https://img.shields.io/github/issues/HealthFlowEgy/integration-sdks)
![GitHub pull requests](https://img.shields.io/github/issues-pr/HealthFlowEgy/integration-sdks)
![GitHub last commit](https://img.shields.io/github/last-commit/HealthFlowEgy/integration-sdks)

---

<div align="center">

**Made with ❤️ by the HealthFlow Egypt Team**

[Website](https://healthflowegy.com) • [Documentation](docs/) • [Report Bug](https://github.com/HealthFlowEgy/integration-sdks/issues) • [Request Feature](https://github.com/HealthFlowEgy/integration-sdks/issues)

</div>
