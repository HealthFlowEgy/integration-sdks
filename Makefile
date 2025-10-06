.PHONY: help install build test clean docker-up docker-down lint format security docs

# Default target
.DEFAULT_GOAL := help

# Colors for output
BLUE := \033[0;34m
GREEN := \033[0;32m
YELLOW := \033[0;33m
RED := \033[0;31m
NC := \033[0m # No Color

help: ## Display this help message
	@echo "$(BLUE)HCX Integration SDKs - Development Commands$(NC)"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "$(GREEN)%-20s$(NC) %s\n", $$1, $$2}'

# ==================== INSTALLATION ====================

install: install-nodejs install-python install-java install-dotnet ## Install all SDK dependencies

install-nodejs: ## Install Node.js SDK dependencies
	@echo "$(BLUE)Installing Node.js SDK dependencies...$(NC)"
	cd javascript && npm install

install-python: ## Install Python SDK dependencies
	@echo "$(BLUE)Installing Python SDK dependencies...$(NC)"
	cd python/hcx-integrator && pip install -e ".[dev]"

install-java: ## Install Java SDK dependencies
	@echo "$(BLUE)Installing Java SDK dependencies...$(NC)"
	cd java/hcx-integrator-sdk && mvn clean install -DskipTests

install-dotnet: ## Install .NET SDK dependencies
	@echo "$(BLUE)Installing .NET SDK dependencies...$(NC)"
	cd dot-net && dotnet restore

# ==================== BUILD ====================

build: build-nodejs build-python build-java build-dotnet ## Build all SDKs

build-nodejs: ## Build Node.js SDK
	@echo "$(BLUE)Building Node.js SDK...$(NC)"
	cd javascript && npm run build

build-python: ## Build Python SDK
	@echo "$(BLUE)Building Python SDK...$(NC)"
	cd python/hcx-integrator && python -m build

build-java: ## Build Java SDK
	@echo "$(BLUE)Building Java SDK...$(NC)"
	cd java/hcx-integrator-sdk && mvn clean package

build-dotnet: ## Build .NET SDK
	@echo "$(BLUE)Building .NET SDK...$(NC)"
	cd dot-net && dotnet build --configuration Release

# ==================== TESTING ====================

test: test-nodejs test-python test-java test-dotnet ## Run all SDK tests

test-nodejs: ## Run Node.js SDK tests
	@echo "$(BLUE)Running Node.js SDK tests...$(NC)"
	cd javascript && npm test

test-python: ## Run Python SDK tests
	@echo "$(BLUE)Running Python SDK tests...$(NC)"
	cd python/hcx-integrator && pytest

test-java: ## Run Java SDK tests
	@echo "$(BLUE)Running Java SDK tests...$(NC)"
	cd java/hcx-integrator-sdk && mvn test

test-dotnet: ## Run .NET SDK tests
	@echo "$(BLUE)Running .NET SDK tests...$(NC)"
	cd dot-net && dotnet test

test-integration: ## Run integration tests
	@echo "$(BLUE)Running integration tests...$(NC)"
	docker-compose up -d mock-hcx-server
	sleep 5
	npm run test:integration --prefix javascript
	pytest tests/integration/ --rootdir=python/hcx-integrator
	mvn verify -P integration-tests --file java/hcx-integrator-sdk/pom.xml
	dotnet test --filter Category=Integration --project dot-net
	docker-compose down

test-coverage: ## Generate test coverage reports
	@echo "$(BLUE)Generating coverage reports...$(NC)"
	cd javascript && npm run test:coverage
	cd python/hcx-integrator && pytest --cov=hcxintegrator --cov-report=html
	cd java/hcx-integrator-sdk && mvn jacoco:report
	cd dot-net && dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# ==================== LINTING & FORMATTING ====================

lint: lint-nodejs lint-python lint-java lint-dotnet ## Run linters on all SDKs

lint-nodejs: ## Lint Node.js SDK
	@echo "$(BLUE)Linting Node.js SDK...$(NC)"
	cd javascript && npm run lint

lint-python: ## Lint Python SDK
	@echo "$(BLUE)Linting Python SDK...$(NC)"
	cd python/hcx-integrator && flake8 hcxintegrator && pylint hcxintegrator

lint-java: ## Lint Java SDK
	@echo "$(BLUE)Linting Java SDK...$(NC)"
	cd java/hcx-integrator-sdk && mvn checkstyle:check

lint-dotnet: ## Lint .NET SDK
	@echo "$(BLUE)Linting .NET SDK...$(NC)"
	cd dot-net && dotnet format --verify-no-changes

format: format-nodejs format-python format-java format-dotnet ## Format all SDK code

format-nodejs: ## Format Node.js SDK code
	@echo "$(BLUE)Formatting Node.js SDK...$(NC)"
	cd javascript && npm run format

format-python: ## Format Python SDK code
	@echo "$(BLUE)Formatting Python SDK...$(NC)"
	cd python/hcx-integrator && black hcxintegrator && isort hcxintegrator

format-java: ## Format Java SDK code
	@echo "$(BLUE)Formatting Java SDK...$(NC)"
	cd java/hcx-integrator-sdk && mvn spotless:apply

format-dotnet: ## Format .NET SDK code
	@echo "$(BLUE)Formatting .NET SDK...$(NC)"
	cd dot-net && dotnet format

# ==================== SECURITY ====================

security: security-nodejs security-python security-java security-dotnet ## Run security scans on all SDKs

security-nodejs: ## Security scan Node.js SDK
	@echo "$(BLUE)Scanning Node.js SDK for vulnerabilities...$(NC)"
	cd javascript && npm audit

security-python: ## Security scan Python SDK
	@echo "$(BLUE)Scanning Python SDK for vulnerabilities...$(NC)"
	cd python/hcx-integrator && safety check && bandit -r hcxintegrator

security-java: ## Security scan Java SDK
	@echo "$(BLUE)Scanning Java SDK for vulnerabilities...$(NC)"
	cd java/hcx-integrator-sdk && mvn org.owasp:dependency-check-maven:check

security-dotnet: ## Security scan .NET SDK
	@echo "$(BLUE)Scanning .NET SDK for vulnerabilities...$(NC)"
	cd dot-net && dotnet list package --vulnerable --include-transitive

# ==================== DOCKER ====================

docker-up: ## Start all Docker services
	@echo "$(BLUE)Starting Docker services...$(NC)"
	docker-compose up -d
	@echo "$(GREEN)Services started successfully!$(NC)"
	@echo "Mock HCX Server: http://localhost:1080"
	@echo "Documentation: http://localhost:8080"
	@echo "Swagger UI: http://localhost:8081"
	@echo "Grafana: http://localhost:3000"

docker-down: ## Stop all Docker services
	@echo "$(BLUE)Stopping Docker services...$(NC)"
	docker-compose down

docker-logs: ## View Docker logs
	docker-compose logs -f

docker-clean: ## Clean Docker volumes and images
	@echo "$(YELLOW)Cleaning Docker resources...$(NC)"
	docker-compose down -v
	docker system prune -f

# ==================== PUBLISHING ====================

publish-nodejs: ## Publish Node.js SDK to npm
	@echo "$(BLUE)Publishing Node.js SDK to npm...$(NC)"
	cd javascript && npm publish --access public

publish-python: ## Publish Python SDK to PyPI
	@echo "$(BLUE)Publishing Python SDK to PyPI...$(NC)"
	cd python/hcx-integrator && python -m build && twine upload dist/*

publish-java: ## Publish Java SDK to Maven Central
	@echo "$(BLUE)Publishing Java SDK to Maven Central...$(NC)"
	cd java/hcx-integrator-sdk && mvn clean deploy -P release

publish-dotnet: ## Publish .NET SDK to NuGet
	@echo "$(BLUE)Publishing .NET SDK to NuGet...$(NC)"
	cd dot-net && dotnet pack --configuration Release && dotnet nuget push ./nupkg/*.nupkg

# ==================== DOCUMENTATION ====================

docs: ## Generate documentation
	@echo "$(BLUE)Generating documentation...$(NC)"
	cd docs && make html

docs-serve: ## Serve documentation locally
	@echo "$(BLUE)Serving documentation at http://localhost:8080$(NC)"
	docker-compose up docs

# ==================== CLEANUP ====================

clean: clean-nodejs clean-python clean-java clean-dotnet ## Clean all build artifacts

clean-nodejs: ## Clean Node.js build artifacts
	@echo "$(BLUE)Cleaning Node.js artifacts...$(NC)"
	cd javascript && rm -rf node_modules dist coverage

clean-python: ## Clean Python build artifacts
	@echo "$(BLUE)Cleaning Python artifacts...$(NC)"
	cd python/hcx-integrator && rm -rf build dist *.egg-info .pytest_cache htmlcov .coverage

clean-java: ## Clean Java build artifacts
	@echo "$(BLUE)Cleaning Java artifacts...$(NC)"
	cd java/hcx-integrator-sdk && mvn clean

clean-dotnet: ## Clean .NET build artifacts
	@echo "$(BLUE)Cleaning .NET artifacts...$(NC)"
	cd dot-net && dotnet clean && rm -rf bin obj

# ==================== UTILITIES ====================

version-check: ## Check versions of all tools
	@echo "$(BLUE)Checking tool versions...$(NC)"
	@echo "Node.js: $$(node --version)"
	@echo "npm: $$(npm --version)"
	@echo "Python: $$(python3 --version)"
	@echo "pip: $$(pip3 --version)"
	@echo "Java: $$(java -version 2>&1 | head -n 1)"
	@echo "Maven: $$(mvn --version | head -n 1)"
	@echo ".NET: $$(dotnet --version)"
	@echo "Docker: $$(docker --version)"

setup-dev: install docker-up ## Complete development environment setup
	@echo "$(GREEN)Development environment setup complete!$(NC)"

ci-check: lint test security ## Run all CI checks locally
	@echo "$(GREEN)All CI checks passed!$(NC)"
