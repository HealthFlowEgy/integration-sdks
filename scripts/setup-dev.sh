#!/bin/bash

# HCX Integration SDKs - Development Environment Setup Script
# This script sets up the complete development environment

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
print_header() {
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

check_command() {
    if command -v $1 &> /dev/null; then
        print_success "$1 is installed"
        return 0
    else
        print_error "$1 is not installed"
        return 1
    fi
}

# Main setup
print_header "HCX Integration SDKs - Development Setup"

echo ""
print_header "Step 1: Checking Prerequisites"

# Check required tools
MISSING_TOOLS=0

if ! check_command node; then
    print_warning "Please install Node.js 16+ from https://nodejs.org/"
    MISSING_TOOLS=1
else
    NODE_VERSION=$(node --version)
    echo "  Node.js version: $NODE_VERSION"
fi

if ! check_command npm; then
    print_warning "Please install npm"
    MISSING_TOOLS=1
else
    NPM_VERSION=$(npm --version)
    echo "  npm version: $NPM_VERSION"
fi

if ! check_command python3; then
    print_warning "Please install Python 3.8+ from https://www.python.org/"
    MISSING_TOOLS=1
else
    PYTHON_VERSION=$(python3 --version)
    echo "  Python version: $PYTHON_VERSION"
fi

if ! check_command pip3; then
    print_warning "Please install pip3"
    MISSING_TOOLS=1
else
    PIP_VERSION=$(pip3 --version)
    echo "  pip version: $PIP_VERSION"
fi

if ! check_command java; then
    print_warning "Please install Java 11+ from https://adoptium.net/"
    MISSING_TOOLS=1
else
    JAVA_VERSION=$(java -version 2>&1 | head -n 1)
    echo "  Java version: $JAVA_VERSION"
fi

if ! check_command mvn; then
    print_warning "Please install Maven from https://maven.apache.org/"
    MISSING_TOOLS=1
else
    MVN_VERSION=$(mvn --version | head -n 1)
    echo "  Maven version: $MVN_VERSION"
fi

if ! check_command dotnet; then
    print_warning "Please install .NET SDK from https://dotnet.microsoft.com/"
    MISSING_TOOLS=1
else
    DOTNET_VERSION=$(dotnet --version)
    echo "  .NET version: $DOTNET_VERSION"
fi

if ! check_command docker; then
    print_warning "Please install Docker from https://www.docker.com/"
    MISSING_TOOLS=1
else
    DOCKER_VERSION=$(docker --version)
    echo "  Docker version: $DOCKER_VERSION"
fi

if ! check_command docker-compose; then
    print_warning "Please install Docker Compose"
    MISSING_TOOLS=1
else
    COMPOSE_VERSION=$(docker-compose --version)
    echo "  Docker Compose version: $COMPOSE_VERSION"
fi

if [ $MISSING_TOOLS -eq 1 ]; then
    echo ""
    print_error "Some required tools are missing. Please install them and run this script again."
    exit 1
fi

echo ""
print_header "Step 2: Installing Node.js SDK Dependencies"
cd javascript
if [ -f package.json ]; then
    npm install
    print_success "Node.js dependencies installed"
else
    print_warning "package.json not found, skipping"
fi
cd ..

echo ""
print_header "Step 3: Installing Python SDK Dependencies"
cd python/hcx-integrator
if [ -f setup.py ]; then
    pip3 install -e ".[dev]" || pip3 install -e .
    print_success "Python dependencies installed"
else
    print_warning "setup.py not found, skipping"
fi
cd ../..

echo ""
print_header "Step 4: Installing Java SDK Dependencies"
cd java/hcx-integrator-sdk
if [ -f pom.xml ]; then
    mvn clean install -DskipTests
    print_success "Java dependencies installed"
else
    print_warning "pom.xml not found, skipping"
fi
cd ../..

echo ""
print_header "Step 5: Installing .NET SDK Dependencies"
cd dot-net
if [ -f *.sln ] || [ -f *.csproj ]; then
    dotnet restore
    print_success ".NET dependencies installed"
else
    print_warning ".NET project files not found, skipping"
fi
cd ..

echo ""
print_header "Step 6: Setting Up Git Hooks"
if [ -d .git ]; then
    # Create pre-commit hook
    cat > .git/hooks/pre-commit << 'EOF'
#!/bin/bash
echo "Running pre-commit checks..."

# Run linting
make lint || {
    echo "Linting failed. Please fix the issues before committing."
    exit 1
}

# Run tests
make test || {
    echo "Tests failed. Please fix the issues before committing."
    exit 1
}

echo "Pre-commit checks passed!"
EOF
    chmod +x .git/hooks/pre-commit
    print_success "Git hooks configured"
else
    print_warning "Not a git repository, skipping git hooks"
fi

echo ""
print_header "Step 7: Creating Environment Files"

# Create .env file if it doesn't exist
if [ ! -f .env ]; then
    cat > .env << 'EOF'
# HCX Configuration
HCX_PARTICIPANT_CODE=provider001@hcx-dev
HCX_AUTH_BASE_URL=http://localhost:1080/auth
HCX_PROTOCOL_BASE_URL=http://localhost:1080/v0.9
HCX_USERNAME=admin@provider001
HCX_PASSWORD=password123
HCX_ENCRYPTION_PRIVATE_KEY_PATH=./keys/private_key.pem
HCX_IG_URL=https://ig.hcxprotocol.io/v0.9

# Development
NODE_ENV=development
PYTHONUNBUFFERED=1
ASPNETCORE_ENVIRONMENT=Development
EOF
    print_success ".env file created"
else
    print_warning ".env file already exists, skipping"
fi

# Create keys directory
mkdir -p keys
if [ ! -f keys/private_key.pem ]; then
    print_warning "Encryption keys not found. Generate them with: make generate-keys"
fi

echo ""
print_header "Step 8: Starting Docker Services"
read -p "Do you want to start Docker services now? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    docker-compose up -d
    print_success "Docker services started"
    echo ""
    echo "Services available at:"
    echo "  - Mock HCX Server: http://localhost:1080"
    echo "  - Documentation: http://localhost:8080"
    echo "  - Swagger UI: http://localhost:8081"
    echo "  - Grafana: http://localhost:3000 (admin/admin)"
    echo "  - Prometheus: http://localhost:9090"
else
    print_warning "Skipping Docker services. Start them later with: make docker-up"
fi

echo ""
print_header "Step 9: Running Initial Tests"
read -p "Do you want to run tests now? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    make test || print_warning "Some tests failed. This is normal for initial setup."
else
    print_warning "Skipping tests. Run them later with: make test"
fi

echo ""
print_header "Setup Complete!"
echo ""
echo "Next steps:"
echo "  1. Review the DEVOPS_README.md for detailed documentation"
echo "  2. Check CONTRIBUTING.md for contribution guidelines"
echo "  3. Run 'make help' to see available commands"
echo "  4. Start developing!"
echo ""
print_success "Happy coding! 🚀"
