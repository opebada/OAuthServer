using Core.Authorization;
using Core.Authorization.ParameterValidation;
using Core.Client;
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
        private const string InvalidRequest = "invalid_request";
        private const string UnauthorizedClient = "unauthorized_client";
        private const string InvalidScope = "invalid_scope";

        public AuthorizationParameterValidatorTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _scopeRepositoryMock = new Mock<IScopeRepository>();
            _loggerMock = new Mock<ILogger<AuthorizationParameterValidator>>();
            _validator = new AuthorizationParameterValidator(_clientRepositoryMock.Object, _scopeRepositoryMock.Object, _loggerMock.Object);
        }

        [DataTestMethod]
        [DataRow("", null, false, InvalidRequest, DisplayName = "EmptyClientId_ReturnsInvalidRequest")]
        [DataRow("nonExistentClientId", null, false, InvalidRequest, DisplayName = "NonExistentClientId_ReturnsInvalidRequest")]
        [DataRow("validClient", "validClient", true, DisplayName = "ValidClientId_ReturnsTrue")]
        public async Task ValidateClientId(string clientId, string client, bool isSuccess, string? errorCode = null)
        {
            // Arrange
            ClientApplication expectedClient = null;

            if (!string.IsNullOrWhiteSpace(client))
            {
                expectedClient = new ClientApplication
                {
                    Id = clientId
                };
            }
            
            _clientRepositoryMock
                .Setup(x => x.GetById(clientId))
                .Returns(Task.FromResult<ClientApplication>(expectedClient));

            // Act
            var result = await _validator.ValidateClientId(clientId);

            // Assert
            result.IsSuccess.Should().Be(isSuccess);

            if (!result.IsSuccess && !string.IsNullOrWhiteSpace(errorCode))
                result.ErrorResponse.Code.Should().Be(errorCode);
        }

        [TestMethod]
        public void ValidateRedirectUrl_ClientIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            string redirectUrlParameter = "https://e.com/callback";
            ClientApplication client = null;

            // Act
            Action action = () => {
                var result = _validator.ValidateRedirectUrl(redirectUrlParameter, client);
            };

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [DataTestMethod]
        [DataRow("", "", false, InvalidRequest, DisplayName = "ClientWithNoRedirectUrl_ReturnsInvalidRequest")]
        [DataRow("", "https://e.com/callback", true, DisplayName = "EmptyRedirectUrl_ButClientHasRedirectUrl_ReturnsTrue")]
        [DataRow("https://n.com/callback", "https://e.com/callback", false, InvalidRequest, DisplayName = "RedirectUrlDoesNotMatchClientRedirectUrl_ReturnsInvalidRequest")]
        [DataRow("https://v.com/callback", "https://v.com/callback", true, DisplayName = "RedirectUrlMatchesClientRedirectUrl_ReturnsTrue")]
        [DataRow("//invalid.com/callback", "https://v.com/callback/", false, InvalidRequest, DisplayName = "RedirectUrlIsInvalidUri_ReturnsInvalidRequest")]
        public void ValidateRedirectUrl(string redirectUrlParameter, string registeredRedirectUrl, bool isSuccess, string? errorCode = null)
        {
            // Arrange
            var client = new ClientApplication { Id = "client123" };;

            if (!string.IsNullOrWhiteSpace(registeredRedirectUrl))
            {
                IEnumerable<RedirectUrl> registeredRedirectUrls = [new RedirectUrl { ClientId = "client123", Value = registeredRedirectUrl }] ;
                client.RedirectUrls = registeredRedirectUrls;
            }

            // Act
            var result = _validator.ValidateRedirectUrl(redirectUrlParameter, client);

            // Assert
            result.IsSuccess.Should().Be(isSuccess);

            if (!result.IsSuccess && !string.IsNullOrWhiteSpace(errorCode))
                result.ErrorResponse.Code.Should().Be(errorCode);
        }

        [TestMethod]
        public void ValidateResponseType_ClientIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            string requestedResponseType = "https://e.com/callback";
            ClientApplication client = null;

            // Act
            Action action = () => {
                var result = _validator.ValidateResponseType(requestedResponseType, client);
            };

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [DataTestMethod]
        [DataRow("", ClientType.Public, false, InvalidRequest, DisplayName = "EmptyResponseType_ReturnsInvalidRequest")]
        [DataRow("invalidResponseType", ClientType.Public, false, InvalidRequest, DisplayName = "InvalidResponseType_ReturnsInvalidRequest")]
        [DataRow(ResponseType.AccessToken, ClientType.Confidential, false, UnauthorizedClient, DisplayName = "ConfidentialClientRequestsTokenResponseType_ReturnsUnauthorizedClient")]
        [DataRow(" code  id_token ", ClientType.Public, true, DisplayName = "ResponseTypeWithWhitespaces_ReturnsTrue")]
        [DataRow("code", ClientType.Confidential, true, DisplayName = "ResponseTypeWithoutWhitespaces_ReturnsTrue")]
        public void ValidateResponseType(string responseType, ClientType clientType, bool isSuccess, string? errorCode = null)
        {
            // Arrange
            var client = new ClientApplication { ClientType = clientType};

            // Act
            var result = _validator.ValidateResponseType(responseType, client);

            // Assert
            result.IsSuccess.Should().Be(isSuccess);
            
            if (!result.IsSuccess && !string.IsNullOrWhiteSpace(errorCode))
                result.ErrorResponse.Code.Should().Be(errorCode);
        }

        [DataTestMethod]
        [DataRow(null, "read write", true, DisplayName = "EmptyRequestScope_ReturnsTrue")]
        [DataRow("read write", "read write", true, DisplayName = "ValidRequestScope_ReturnsTrue")]
        [DataRow("UnrecognizedScope", "read write", false, InvalidScope, DisplayName = "UnrecognizedScope_ReturnsInvalidScope")]
        [DataRow("read UnrecognizedScope", "read write", false, InvalidScope, DisplayName = "UnrecognizedScopeWithValidScope_ReturnsInvalidScope")]
        public async Task ValidateScope(string requestedScope, string configuredScope, bool isSuccess, string? errorCode = null)
        {
            // Arrange
            IReadOnlySet<Scope> scopes = configuredScope.Split(' ').Select(x => new Scope { Name = x, Description = x }).ToHashSet();

            _scopeRepositoryMock
                .Setup(repo => repo.GetScopes(It.IsAny<string[]>()))
                .ReturnsAsync(scopes);

            // Act
            var result = await _validator.ValidateScope(requestedScope);

            // Assert
            result.IsSuccess.Should().Be(isSuccess);

            if (!result.IsSuccess && !string.IsNullOrWhiteSpace(errorCode))
                result.ErrorResponse.Code.Should().Be(errorCode);
        }
    }

}