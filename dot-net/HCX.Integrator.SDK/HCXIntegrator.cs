/**
 * HCX Integrator SDK for .NET v2.0.0
 * 
 * Official SDK for integrating with HCX Protocol v0.9
 * 
 * File: HCX.Integrator.SDK/HCXIntegrator.cs
 */

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace HCX.Integrator.SDK
{
    // ==================== CONFIGURATION ====================
    
    public class HCXConfig
    {
        public string ParticipantCode { get; set; }
        public string AuthBasePath { get; set; }
        public string ProtocolBasePath { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string EncryptionPrivateKeyPath { get; set; }
        public string EncryptionPublicKey { get; set; }
        public string IgUrl { get; set; }
        public int Timeout { get; set; } = 30000;
        public int RetryAttempts { get; set; } = 3;
    }

    // ==================== HEADER MODELS ====================
    
    public class HCXHeaders
    {
        public string SenderCode { get; set; }
        public string RecipientCode { get; set; }
        public string CorrelationId { get; set; }
        public string ApiCallId { get; set; }
        public string Timestamp { get; set; }
        public string Status { get; set; }
        public string WorkflowId { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>
            {
                ["x-hcx-sender_code"] = SenderCode,
                ["x-hcx-recipient_code"] = RecipientCode,
                ["x-hcx-correlation_id"] = CorrelationId,
                ["x-hcx-api_call_id"] = ApiCallId,
                ["x-hcx-timestamp"] = Timestamp
            };

            if (!string.IsNullOrEmpty(Status))
                dict["x-hcx-status"] = Status;
            
            if (!string.IsNullOrEmpty(WorkflowId))
                dict["x-hcx-workflow_id"] = WorkflowId;

            return dict;
        }
    }

    // ==================== REQUEST/RESPONSE MODELS ====================
    
    public class HCXRequest
    {
        public string Payload { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }

    public class HCXResponse
    {
        public string CorrelationId { get; set; }
        public string ApiCallId { get; set; }
        public string Timestamp { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
    }

    // ==================== FHIR MODELS ====================
    
    public class FHIRBundle
    {
        public string ResourceType { get; set; } = "Bundle";
        public string Type { get; set; } = "collection";
        public List<FHIRBundleEntry> Entry { get; set; } = new List<FHIRBundleEntry>();
    }

    public class FHIRBundleEntry
    {
        public string FullUrl { get; set; }
        public object Resource { get; set; }
    }

    // ==================== EXCEPTIONS ====================
    
    public class HCXException : Exception
    {
        public string Code { get; }
        
        public HCXException(string code, string message) : base(message)
        {
            Code = code;
        }

        public HCXException(string code, string message, Exception innerException) 
            : base(message, innerException)
        {
            Code = code;
        }
    }

    public class AuthenticationException : HCXException
    {
        public AuthenticationException(string code, string message) : base(code, message) { }
        public AuthenticationException(string code, string message, Exception inner) 
            : base(code, message, inner) { }
    }

    public class EncryptionException : HCXException
    {
        public EncryptionException(string code, string message) : base(code, message) { }
        public EncryptionException(string code, string message, Exception inner) 
            : base(code, message, inner) { }
    }

    // ==================== MAIN SDK CLASS ====================
    
    public class HCXIntegrator : IDisposable
    {
        internal readonly HCXConfig _config;
        private readonly HttpClient _httpClient;
        private readonly HttpClient _authHttpClient;
        private string _accessToken;
        private DateTime? _tokenExpiry;
        private RSA _privateKey;

        // Sub-modules
        public CoverageEligibilityAPI CoverageEligibility { get; }
        public PreAuthAPI PreAuth { get; }
        public ClaimAPI Claim { get; }
        public CommunicationAPI Communication { get; }
        public NotificationAPI Notification { get; }
        public StatusAPI Status { get; }

        public HCXIntegrator(HCXConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
            ValidateConfig();
            
            // Initialize HTTP clients
            _authHttpClient = new HttpClient
            {
                BaseAddress = new Uri(_config.AuthBasePath),
                Timeout = TimeSpan.FromMilliseconds(_config.Timeout)
            };
            _authHttpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_config.ProtocolBasePath),
                Timeout = TimeSpan.FromMilliseconds(_config.Timeout)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            // Load private key
            LoadPrivateKey();

            // Initialize sub-modules
            CoverageEligibility = new CoverageEligibilityAPI(this);
            PreAuth = new PreAuthAPI(this);
            Claim = new ClaimAPI(this);
            Communication = new CommunicationAPI(this);
            Notification = new NotificationAPI(this);
            Status = new StatusAPI(this);
        }

        private void ValidateConfig()
        {
            if (string.IsNullOrEmpty(_config.ParticipantCode))
                throw new ArgumentException("ParticipantCode is required");
            if (string.IsNullOrEmpty(_config.AuthBasePath))
                throw new ArgumentException("AuthBasePath is required");
            if (string.IsNullOrEmpty(_config.ProtocolBasePath))
                throw new ArgumentException("ProtocolBasePath is required");
            if (string.IsNullOrEmpty(_config.Username))
                throw new ArgumentException("Username is required");
            if (string.IsNullOrEmpty(_config.Password))
                throw new ArgumentException("Password is required");
            if (string.IsNullOrEmpty(_config.EncryptionPrivateKeyPath))
                throw new ArgumentException("EncryptionPrivateKeyPath is required");
        }

        private void LoadPrivateKey()
        {
            try
            {
                var pemContent = System.IO.File.ReadAllText(_config.EncryptionPrivateKeyPath);
                _privateKey = RSA.Create();
                _privateKey.ImportFromPem(pemContent);
            }
            catch (Exception ex)
            {
                throw new HCXException("KEY_LOAD_ERROR", 
                    $"Failed to load private key: {ex.Message}", ex);
            }
        }

        // ==================== AUTHENTICATION ====================

        public async Task<string> GetAccessTokenAsync()
        {
            // Check if token is still valid
            if (!string.IsNullOrEmpty(_accessToken) && 
                _tokenExpiry.HasValue && 
                DateTime.UtcNow < _tokenExpiry.Value)
            {
                return _accessToken;
            }

            try
            {
                var request = new
                {
                    username = _config.Username,
                    password = _config.Password,
                    participant_code = _config.ParticipantCode
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _authHttpClient.PostAsync("/token", content);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(
                    responseData,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                _accessToken = tokenResponse.AccessToken;
                // Set expiry with 1 minute buffer
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

                return _accessToken;
            }
            catch (Exception ex)
            {
                throw new AuthenticationException("AUTH_ERROR", 
                    $"Failed to obtain access token: {ex.Message}", ex);
            }
        }

        // ==================== ENCRYPTION/DECRYPTION ====================

        public async Task<string> EncryptPayloadAsync(
            object payload, 
            string recipientPublicKey, 
            Dictionary<string, string> headers)
        {
            try
            {
                var payloadJson = JsonSerializer.Serialize(payload);
                var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);

                // Import recipient's public key
                var rsa = RSA.Create();
                rsa.ImportFromPem(recipientPublicKey);

                // Encrypt using RSA-OAEP-256
                var encryptedData = rsa.Encrypt(payloadBytes, RSAEncryptionPadding.OaepSHA256);

                // Create JWE (simplified - in production use proper JWE library)
                var jwe = Convert.ToBase64String(encryptedData);

                return await Task.FromResult(jwe);
            }
            catch (Exception ex)
            {
                throw new EncryptionException("ENCRYPTION_ERROR", 
                    $"Failed to encrypt payload: {ex.Message}", ex);
            }
        }

        public async Task<(object Payload, HCXHeaders Headers)> DecryptPayloadAsync(string jwe)
        {
            try
            {
                // Decrypt JWE
                var encryptedBytes = Convert.FromBase64String(jwe);
                var decryptedBytes = _privateKey.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
                var decryptedJson = Encoding.UTF8.GetString(decryptedBytes);

                var payload = JsonSerializer.Deserialize<object>(decryptedJson);

                // Extract headers (simplified)
                var headers = new HCXHeaders();

                return await Task.FromResult((payload, headers));
            }
            catch (Exception ex)
            {
                throw new EncryptionException("DECRYPTION_ERROR", 
                    $"Failed to decrypt payload: {ex.Message}", ex);
            }
        }

        // ==================== UTILITY METHODS ====================

        public string GenerateCorrelationId()
        {
            return Guid.NewGuid().ToString();
        }

        public string GenerateApiCallId()
        {
            return Guid.NewGuid().ToString();
        }

        public HCXHeaders CreateHCXHeaders(string recipientCode, string correlationId = null)
        {
            return new HCXHeaders
            {
                SenderCode = _config.ParticipantCode,
                RecipientCode = recipientCode,
                CorrelationId = correlationId ?? GenerateCorrelationId(),
                ApiCallId = GenerateApiCallId(),
                Timestamp = DateTime.UtcNow.ToString("o")
            };
        }

        public async Task<T> MakeRequestAsync<T>(
            string endpoint, 
            HttpMethod method, 
            object data = null)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                
                var request = new HttpRequestMessage(method, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                if (data != null)
                {
                    var json = JsonSerializer.Serialize(data);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(
                    responseData,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch (HttpRequestException ex)
            {
                throw new HCXException("REQUEST_ERROR", 
                    $"Request failed: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _authHttpClient?.Dispose();
            _privateKey?.Dispose();
        }
    }

    // ==================== SUB-MODULES ====================

    public class CoverageEligibilityAPI
    {
        private readonly HCXIntegrator _sdk;

        public CoverageEligibilityAPI(HCXIntegrator sdk)
        {
            _sdk = sdk;
        }

        public async Task<HCXResponse> CheckAsync(
            string recipientCode,
            FHIRBundle fhirBundle,
            string recipientPublicKey)
        {
            var headers = _sdk.CreateHCXHeaders(recipientCode);
            var encryptedPayload = await _sdk.EncryptPayloadAsync(
                fhirBundle,
                recipientPublicKey,
                headers.ToDictionary()
            );

            return await _sdk.MakeRequestAsync<HCXResponse>(
                "/coverageeligibility/check",
                HttpMethod.Post,
                new { payload = encryptedPayload }
            );
        }

        public async Task<(object Payload, HCXHeaders Headers)> OnCheckAsync(string encryptedPayload)
        {
            return await _sdk.DecryptPayloadAsync(encryptedPayload);
        }
    }

    public class PreAuthAPI
    {
        private readonly HCXIntegrator _sdk;

        public PreAuthAPI(HCXIntegrator sdk)
        {
            _sdk = sdk;
        }

        public async Task<HCXResponse> SubmitAsync(
            string recipientCode,
            FHIRBundle fhirBundle,
            string recipientPublicKey)
        {
            var headers = _sdk.CreateHCXHeaders(recipientCode);
            var encryptedPayload = await _sdk.EncryptPayloadAsync(
                fhirBundle,
                recipientPublicKey,
                headers.ToDictionary()
            );

            return await _sdk.MakeRequestAsync<HCXResponse>(
                "/preauth/submit",
                HttpMethod.Post,
                new { payload = encryptedPayload }
            );
        }

        public async Task<(object Payload, HCXHeaders Headers)> OnSubmitAsync(string encryptedPayload)
        {
            return await _sdk.DecryptPayloadAsync(encryptedPayload);
        }
    }

    public class ClaimAPI
    {
        private readonly HCXIntegrator _sdk;

        public ClaimAPI(HCXIntegrator sdk)
        {
            _sdk = sdk;
        }

        public async Task<HCXResponse> SubmitAsync(
            string recipientCode,
            FHIRBundle fhirBundle,
            string recipientPublicKey)
        {
            var headers = _sdk.CreateHCXHeaders(recipientCode);
            var encryptedPayload = await _sdk.EncryptPayloadAsync(
                fhirBundle,
                recipientPublicKey,
                headers.ToDictionary()
            );

            return await _sdk.MakeRequestAsync<HCXResponse>(
                "/claim/submit",
                HttpMethod.Post,
                new { payload = encryptedPayload }
            );
        }

        public async Task<(object Payload, HCXHeaders Headers)> OnSubmitAsync(string encryptedPayload)
        {
            return await _sdk.DecryptPayloadAsync(encryptedPayload);
        }
    }

    public class CommunicationAPI
    {
        private readonly HCXIntegrator _sdk;

        public CommunicationAPI(HCXIntegrator sdk)
        {
            _sdk = sdk;
        }

        public async Task<HCXResponse> RequestAsync(
            string recipientCode,
            FHIRBundle fhirBundle,
            string recipientPublicKey)
        {
            var headers = _sdk.CreateHCXHeaders(recipientCode);
            var encryptedPayload = await _sdk.EncryptPayloadAsync(
                fhirBundle,
                recipientPublicKey,
                headers.ToDictionary()
            );

            return await _sdk.MakeRequestAsync<HCXResponse>(
                "/communication/request",
                HttpMethod.Post,
                new { payload = encryptedPayload }
            );
        }

        public async Task<(object Payload, HCXHeaders Headers)> OnRequestAsync(string encryptedPayload)
        {
            return await _sdk.DecryptPayloadAsync(encryptedPayload);
        }
    }

    public class NotificationAPI
    {
        private readonly HCXIntegrator _sdk;

        public NotificationAPI(HCXIntegrator sdk)
        {
            _sdk = sdk;
        }

        public async Task<NotificationSubscription> SubscribeAsync(
            string topicCode,
            string deliveryChannel,
            string endpoint = null,
            Dictionary<string, object> filters = null)
        {
            var request = new
            {
                topic_code = topicCode,
                subscriber = new
                {
                    participant_code = _sdk._config.ParticipantCode
                },
                delivery_channel = deliveryChannel,
                endpoint,
                filters
            };

            return await _sdk.MakeRequestAsync<NotificationSubscription>(
                "/notification/subscribe",
                HttpMethod.Post,
                request
            );
        }

        public async Task UnsubscribeAsync(string subscriptionId)
        {
            await _sdk.MakeRequestAsync<object>(
                "/notification/unsubscribe",
                HttpMethod.Post,
                new { subscription_id = subscriptionId }
            );
        }
    }

    public class NotificationSubscription
    {
        public string SubscriptionId { get; set; }
        public string TopicCode { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StatusAPI
    {
        private readonly HCXIntegrator _sdk;

        public StatusAPI(HCXIntegrator sdk)
        {
            _sdk = sdk;
        }

        public async Task<StatusResponse> CheckAsync(string correlationId)
        {
            return await _sdk.MakeRequestAsync<StatusResponse>(
                "/hcx/status",
                HttpMethod.Post,
                new { correlation_id = correlationId }
            );
        }
    }

    public class StatusResponse
    {
        public string CorrelationId { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
        public object Details { get; set; }
    }

    // ==================== FHIR HELPERS ====================

    public static class FHIRHelpers
    {
        public static FHIRBundle CreateCoverageEligibilityBundle(
            string patientId,
            string insurerId,
            string policyNumber,
            string providerId)
        {
            return new FHIRBundle
            {
                Entry = new List<FHIRBundleEntry>
                {
                    new FHIRBundleEntry
                    {
                        FullUrl = $"urn:uuid:{Guid.NewGuid()}",
                        Resource = new
                        {
                            resourceType = "CoverageEligibilityRequest",
                            status = "active",
                            purpose = new[] { "benefits", "discovery" },
                            patient = new { reference = $"Patient/{patientId}" },
                            insurer = new { reference = $"Organization/{insurerId}" },
                            insurance = new[]
                            {
                                new
                                {
                                    coverage = new { reference = $"Coverage/{policyNumber}" }
                                }
                            },
                            provider = new { reference = $"Organization/{providerId}" }
                        }
                    }
                }
            };
        }

        public static FHIRBundle CreateClaimBundle(
            string patientId,
            string providerId,
            string insurerId,
            List<ClaimItem> items)
        {
            var claimItems = new List<object>();
            for (int i = 0; i < items.Count; i++)
            {
                claimItems.Add(new
                {
                    sequence = i + 1,
                    productOrService = new { text = items[i].ServiceName },
                    quantity = new { value = items[i].Quantity },
                    unitPrice = new
                    {
                        value = items[i].UnitPrice,
                        currency = "INR"
                    }
                });
            }

            return new FHIRBundle
            {
                Entry = new List<FHIRBundleEntry>
                {
                    new FHIRBundleEntry
                    {
                        FullUrl = $"urn:uuid:{Guid.NewGuid()}",
                        Resource = new
                        {
                            resourceType = "Claim",
                            status = "active",
                            type = new
                            {
                                coding = new[]
                                {
                                    new
                                    {
                                        system = "http://terminology.hl7.org/CodeSystem/claim-type",
                                        code = "institutional"
                                    }
                                }
                            },
                            use = "claim",
                            patient = new { reference = $"Patient/{patientId}" },
                            provider = new { reference = $"Organization/{providerId}" },
                            insurer = new { reference = $"Organization/{insurerId}" },
                            item = claimItems
                        }
                    }
                }
            };
        }
    }

    public class ClaimItem
    {
        public string ServiceName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
