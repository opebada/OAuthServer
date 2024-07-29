using Application.Authorization.ParameterValidation;
using Core.Client;
using Core.Error;
using Core.Scope;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.UnitTests
{
    [TestClass]
    public class AuthorizationParameterValidatorTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<IScopeRepository> _scopeRepositoryMock;
        private readonly Mock<ILogger<AuthorizationParameterValidator>> _loggerMock;
        private readonly AuthorizationParameterValidator _validator;

        public AuthorizationParameterValidatorTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _scopeRepositoryMock = new Mock<IScopeRepository>();
            _loggerMock = new Mock<ILogger<AuthorizationParameterValidator>>();
            _validator = new AuthorizationParameterValidator(_clientRepositoryMock.Object, _scopeRepositoryMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task ValidateClientId_ClientIdIsNullOrEmpty_ReturnsInvalidRequestOAuthError()
        {
            // Arrange
            string clientId = "";

            // Act
            var result = await _validator.ValidateClientId(clientId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorResponse?.Should().Be(OAuthErrors.InvalidRequest);
        }

        [TestMethod]
        public async Task ValidateClientId_ClientDoesNotExist_ReturnsInvalidRequestOAuthError()
        {
            // Arrange
            string clientId = "client123";
            ClientApplication expectedClient = null;

            _clientRepositoryMock
                .Setup(x => x.GetById(clientId))
                .Returns(Task.FromResult<ClientApplication>(expectedClient));

            // Act
            var result = await _validator.ValidateClientId(clientId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorResponse?.Should().Be(OAuthErrors.InvalidRequest);
        }

        [TestMethod]
        public async Task ValidateClientId_ClientExists_ReturnsSuccess()
        {
            // Arrange
            string clientId = "client123";
            var client = new ClientApplication()
            {
                Id = clientId
            };

            _clientRepositoryMock
                .Setup(x => x.GetById(clientId))
                .ReturnsAsync(client);

            // Act
            AuthorizationParameterValidationResult result = await _validator.ValidateClientId(clientId);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public void ValidateRedirectUrl_RedirectUrlRegisteredButMissingInRequest_ReturnsSuccess()
        {
            // Arrange
            string redirectUrlParameter = string.Empty;
            IEnumerable<RedirectUrl> registeredRedirectUrls = [new RedirectUrl { Value = "https://example.com/callback" }] ;

            // Act
            var result = _validator.ValidateRedirectUrl(redirectUrlParameter, registeredRedirectUrls);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }        
        
        [TestMethod]
        public void ValidateRedirectUrl_RedirectUrlDoesNotMatchRegisteredValue_ReturnsInvalidRequestOAuthError()
        {
            // Arrange
            string redirectUrlParameter = "https://example.com/callback";
            IEnumerable<RedirectUrl> nonMatchingRegisteredRedirectUrls = [new RedirectUrl { Value = "https://different.com/callback" }];

            // Act
            var result = _validator.ValidateRedirectUrl(redirectUrlParameter, nonMatchingRegisteredRedirectUrls);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorResponse?.Should().Be(OAuthErrors.InvalidRequest);
        }        
        
        [TestMethod]
        public void ValidateRedirectUrl_RedirectUrlSchemeIsNotHttps_ReturnsInvalidResult()
        {
            // Arrange
            string redirectUrlParameter = "htt://example.com/callback";
            IEnumerable<RedirectUrl> registeredRedirectUrls = [new RedirectUrl { Value = "https://different.com/callback" }];

            // Act
            var result = _validator.ValidateRedirectUrl(redirectUrlParameter, registeredRedirectUrls);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorResponse?.Should().Be(OAuthErrors.InvalidRequest);
        }

        [TestMethod]
        public void ValidateRedirectUrl_ClientDoesNotHaveRegisteredRedirectUrls_ReturnsInvalidResult()
        {
            // Arrange
            string redirectUrlParameter = "https://example.com/callback";
            IEnumerable<RedirectUrl> registeredRedirectUrls = [];

            // Act
            var result = _validator.ValidateRedirectUrl(redirectUrlParameter, registeredRedirectUrls);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorResponse?.Should().Be(OAuthErrors.InvalidRequest);
        }

        [TestMethod]
        public void ValidateRedirectUrl_ValidRedirectUrl_ReturnsSuccess()
        {
            // Arrange
            string redirectUrl = "https://example.com/callback";
            IEnumerable<RedirectUrl> listWithMatchingRegisteredRedirectUrl = [new RedirectUrl { Value = redirectUrl }];

            // Act
            var result = _validator.ValidateRedirectUrl(redirectUrl, listWithMatchingRegisteredRedirectUrl);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public void ValidateRedirectUrl_ValidRegisteredRedirectUrlDiffersByBackslash_ReturnsValidResult()
        {
            // Arrange
            string redirectUrl = "https://example.com/callback";
            IEnumerable<RedirectUrl> listWithMatchingRegisteredRedirectUrl = [new RedirectUrl { Value = redirectUrl + '/' }];

            // Act
            var result = _validator.ValidateRedirectUrl(redirectUrl, listWithMatchingRegisteredRedirectUrl);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public void ValidateResponseType_ResponseTypeIsEmpty_ReturnsInvalidRequestOAuthError()
        {
            // Arrange
            string responseType = " ";
            var clientType = ClientType.Public;

            // Act
            var result = _validator.ValidateResponseType(responseType, clientType);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorResponse?.Should().Be(OAuthErrors.InvalidRequest);
        }

        [TestMethod]
        public void ValidateResponseType_InvalidResponseType_ReturnsInvalidRequestOAuthError()
        {
            // Arrange
            string responseType = "invalid";
            var clientType = ClientType.Public;

            // Act
            var result = _validator.ValidateResponseType(responseType, clientType);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorResponse?.Should().Be(OAuthErrors.InvalidRequest);
        }

        [TestMethod]
        public void ValidateResponseType_PublicClientNotAuthorizedToRequestTokenResponseType_ReturnsUnauthorizedClientOAuthError()
        {
            // Arrange
            string responseType = ResponseType.AccessToken;
            var clientType = ClientType.Confidential;

            // Act
            var result = _validator.ValidateResponseType(responseType, clientType);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorResponse?.Should().Be(OAuthErrors.UnauthorizedClient);
        }

        [TestMethod]
        public void ValidateResponseType_ValidResponseTypeContainsWhitespaces_ReturnsSuccess()
        {
            // Arrange
            var clientType = ClientType.Confidential;
            string responseType = " code  id_token ";

            // Act
            var result = _validator.ValidateResponseType(responseType, clientType);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public void ValidateResponseType_ValidResponseType_ReturnsSuccess()
        {
            // Arrange
            var clientType = ClientType.Confidential;
            string responseType = "code";

            // Act
            var result = _validator.ValidateResponseType(responseType, clientType);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public async Task ValidateScope_RequestedScopeIsNullOrEmpty_ReturnsSuccess()
        {
            // Arrange
            string requestedScope = null;
            var scopes = new List<Scope> { new Scope { Name = "read", Description = "read" } };

            _scopeRepositoryMock
                .Setup(repo => repo.GetScopes(It.IsAny<string[]>()))
                .ReturnsAsync(scopes);

            // Act
            var result = await _validator.ValidateScope(requestedScope);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public async Task ValidateScope_RequestedScopeIsValid_ReturnsSuccess()
        {
            // Arrange
            string requestedScope = "read write";
            var validScopes = new List<Scope> { new Scope { Name = "read", Description = "read" }, new Scope { Name = "write", Description = "write" } };

            _scopeRepositoryMock
                .Setup(x => x.GetScopes(It.IsAny<string[]>()))
                .ReturnsAsync(validScopes);

            // Act
            var result = await _validator.ValidateScope(requestedScope);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }

}