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
        [DataRow("", null, false, InvalidRequest)]
        [DataRow("nonExistentClientId", null, false, InvalidRequest)]
        [DataRow("validClient", "validClient", true, null)]
        public async Task ValidateClientId(string clientId, string client, bool isSuccess, string errorCode)
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

        [DataTestMethod]
        [DataRow("", "https://example.com/callback", true)]
        [DataRow("https://nonmatchingurl.com/callback", "https://example.com/callback", false, InvalidRequest)]
        [DataRow("https://validmatchingurl.com/callback", "https://validmatchingurl.com/callback", true)]
        [DataRow("https://validmatchingurl.com/callback", "https://validmatchingurl.com/callback/", true)]
        public void ValidateRedirectUrl(string redirectUrlParameter, string registeredRedirectUrl, bool isSuccess, string? errorCode = null)
        {
            // Arrange
            IEnumerable<RedirectUrl> registeredRedirectUrls = [new RedirectUrl { Value = registeredRedirectUrl }] ;

            // Act
            var result = _validator.ValidateRedirectUrl(redirectUrlParameter, registeredRedirectUrls);

            // Assert
            result.IsSuccess.Should().Be(isSuccess);

            if (!result.IsSuccess && !string.IsNullOrWhiteSpace(errorCode))
                result.ErrorResponse.Code.Should().Be(errorCode);
        }

        [DataTestMethod]
        [DataRow("", ClientType.Public, false, InvalidRequest)]
        [DataRow("invalidResponseType", ClientType.Public, false, InvalidRequest)]
        [DataRow(ResponseType.AccessToken, ClientType.Confidential, false, UnauthorizedClient)]
        [DataRow(" code  id_token ", ClientType.Public, true)]
        [DataRow("code", ClientType.Confidential, true)]
        public void ValidateResponseType(string responseType, ClientType clientType, bool isSuccess, string? errorCode = null)
        {
            // Act
            var result = _validator.ValidateResponseType(responseType, clientType);

            // Assert
            result.IsSuccess.Should().Be(isSuccess);
            
            if (!result.IsSuccess && !string.IsNullOrWhiteSpace(errorCode))
                result.ErrorResponse.Code.Should().Be(errorCode);
        }

        [DataTestMethod]
        [DataRow(null, "read write", true)]
        [DataRow("read write", "read write", true)]
        [DataRow("invalidscope", "read write", false, InvalidScope)]
        [DataRow("read invalidscope", "read write", false, InvalidScope)]
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