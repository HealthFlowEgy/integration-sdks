/**
 * HCX Integrator SDK - Unit Tests
 * File: HCX.Integrator.SDK.Tests/HCXIntegratorTests.cs
 */

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;
using System.Threading;
using HCX.Integrator.SDK;

namespace HCX.Integrator.SDK.Tests
{
    public class HCXIntegratorTests
    {
        private readonly HCXConfig _testConfig;

        public HCXIntegratorTests()
        {
            _testConfig = new HCXConfig
            {
                ParticipantCode = "test@hcx-dev",
                AuthBasePath = "https://api-test.hcx.com/auth",
                ProtocolBasePath = "https://api-test.hcx.com/v0.9",
                Username = "test_user",
                Password = "test_password",
                EncryptionPrivateKeyPath = GenerateTestPrivateKey(),
                IgUrl = "https://ig.hcxprotocol.io/v0.9"
            };
        }

        private string GenerateTestPrivateKey()
        {
            // Generate a temporary RSA key for testing
            var rsa = System.Security.Cryptography.RSA.Create(2048);
            var pemKey = rsa.ExportRSAPrivateKeyPem();
            
            var tempFile = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(tempFile, pemKey);
            
            return tempFile;
        }

        [Fact]
        public void Constructor_WithValidConfig_CreatesInstance()
        {
            // Arrange & Act
            using var hcx = new HCXIntegrator(_testConfig);

            // Assert
            Assert.NotNull(hcx);
            Assert.NotNull(hcx.CoverageEligibility);
            Assert.NotNull(hcx.PreAuth);
            Assert.NotNull(hcx.Claim);
            Assert.NotNull(hcx.Communication);
            Assert.NotNull(hcx.Notification);
            Assert.NotNull(hcx.Status);
        }

        [Fact]
        public void Constructor_WithNullConfig_ThrowsException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new HCXIntegrator(null));
        }

        [Fact]
        public void Constructor_WithMissingParticipantCode_ThrowsException()
        {
            // Arrange
            var invalidConfig = new HCXConfig
            {
                ParticipantCode = null,
                AuthBasePath = "https://api.hcx.com/auth",
                ProtocolBasePath = "https://api.hcx.com/v0.9",
                Username = "test",
                Password = "test",
                EncryptionPrivateKeyPath = _testConfig.EncryptionPrivateKeyPath,
                IgUrl = "https://ig.hcxprotocol.io/v0.9"
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new HCXIntegrator(invalidConfig));
        }

        [Fact]
        public void GenerateCorrelationId_ReturnsValidGuid()
        {
            // Arrange
            using var hcx = new HCXIntegrator(_testConfig);

            // Act
            var correlationId = hcx.GenerateCorrelationId();

            // Assert
            Assert.NotNull(correlationId);
            Assert.True(Guid.TryParse(correlationId, out _));
        }

        [Fact]
        public void GenerateApiCallId_ReturnsValidGuid()
        {
            // Arrange
            using var hcx = new HCXIntegrator(_testConfig);

            // Act
            var apiCallId = hcx.GenerateApiCallId();

            // Assert
            Assert.NotNull(apiCallId);
            Assert.True(Guid.TryParse(apiCallId, out _));
        }

        [Fact]
        public void CreateHCXHeaders_ReturnsValidHeaders()
        {
            // Arrange
            using var hcx = new HCXIntegrator(_testConfig);
            var recipientCode = "recipient@hcx-dev";

            // Act
            var headers = hcx.CreateHCXHeaders(recipientCode);

            // Assert
            Assert.NotNull(headers);
            Assert.Equal("test@hcx-dev", headers.SenderCode);
            Assert.Equal(recipientCode, headers.RecipientCode);
            Assert.NotNull(headers.CorrelationId);
            Assert.NotNull(headers.ApiCallId);
            Assert.NotNull(headers.Timestamp);
        }

        [Fact]
        public void CreateHCXHeaders_WithCorrelationId_UsesProvidedId()
        {
            // Arrange
            using var hcx = new HCXIntegrator(_testConfig);
            var recipientCode = "recipient@hcx-dev";
            var customCorrelationId = Guid.NewGuid().ToString();

            // Act
            var headers = hcx.CreateHCXHeaders(recipientCode, customCorrelationId);

            // Assert
            Assert.Equal(customCorrelationId, headers.CorrelationId);
        }
    }

    public class FHIRHelpersTests
    {
        [Fact]
        public void CreateCoverageEligibilityBundle_ReturnsValidBundle()
        {
            // Arrange
            var patientId = "PAT001";
            var insurerId = "INS001";
            var policyNumber = "POL123";
            var providerId = "PROV001";

            // Act
            var bundle = FHIRHelpers.CreateCoverageEligibilityBundle(
                patientId, insurerId, policyNumber, providerId);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal("Bundle", bundle.ResourceType);
            Assert.Equal("collection", bundle.Type);
            Assert.Single(bundle.Entry);
        }

        [Fact]
        public void CreateClaimBundle_ReturnsValidBundle()
        {
            // Arrange
            var patientId = "PAT001";
            var providerId = "PROV001";
            var insurerId = "INS001";
            var items = new System.Collections.Generic.List<ClaimItem>
            {
                new ClaimItem { ServiceName = "Consultation", Quantity = 1, UnitPrice = 500 },
                new ClaimItem { ServiceName = "Lab Test", Quantity = 2, UnitPrice = 300 }
            };

            // Act
            var bundle = FHIRHelpers.CreateClaimBundle(
                patientId, providerId, insurerId, items);

            // Assert
            Assert.NotNull(bundle);
            Assert.Equal("Bundle", bundle.ResourceType);
            Assert.Equal("collection", bundle.Type);
            Assert.Single(bundle.Entry);
        }
    }

    public class HCXExceptionTests
    {
        [Fact]
        public void HCXException_WithCodeAndMessage_CreatesException()
        {
            // Arrange
            var code = "TEST_ERROR";
            var message = "Test error message";

            // Act
            var exception = new HCXException(code, message);

            // Assert
            Assert.Equal(code, exception.Code);
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void AuthenticationException_InheritsFromHCXException()
        {
            // Arrange & Act
            var exception = new AuthenticationException("AUTH_ERROR", "Auth failed");

            // Assert
            Assert.IsAssignableFrom<HCXException>(exception);
            Assert.Equal("AUTH_ERROR", exception.Code);
        }

        [Fact]
        public void EncryptionException_InheritsFromHCXException()
        {
            // Arrange & Act
            var exception = new EncryptionException("ENC_ERROR", "Encryption failed");

            // Assert
            Assert.IsAssignableFrom<HCXException>(exception);
            Assert.Equal("ENC_ERROR", exception.Code);
        }
    }

    // Integration tests would go here
    // Marked with [Fact(Skip = "Integration test")] attribute
}
