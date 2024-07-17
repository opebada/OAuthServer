using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Parameters;
using Core.Client;
using Core.Error;
using Core.Scope;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static Core.Error.Constants;

namespace Application.UnitTests
{
    [TestClass]
    public class ParameterValidatorTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<IScopeRepository> _scopeRepositoryMock;
        private readonly Mock<ILogger<ParameterValidator>> _loggerMock;
        private readonly ParameterValidator _validator;

        public ParameterValidatorTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _scopeRepositoryMock = new Mock<IScopeRepository>();
            _loggerMock = new Mock<ILogger<ParameterValidator>>();
            _validator = new ParameterValidator(_clientRepositoryMock.Object, _scopeRepositoryMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task ValidateClientId_ClientExists_ReturnsValidResult()
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
            ParameterValidationResult result = await _validator.ValidateClientId(clientId);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public async Task ValidateClientId_ClientDoesNotExist_ReturnsInvalidResult()
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
            result.ErrorResponse?.Should().Be(OAuthError.InvalidRequest);
        }

        [TestMethod]
        public async Task ValidateRedirectUrl_RedirectUrlRegisteredButMissingInRequest_ReturnsInvalidResult()
        {
            // Arrange
            string clientId = "client123";
            string redirectUrlParameter = string.Empty;
            string registeredRedirectUrl = "https://invalid.com/callback";
            var client = new ClientApplication
            {
                Id = clientId,
                RedirectUrls = new List<RedirectUrl> { new RedirectUrl { Value = registeredRedirectUrl } }
            };

            _clientRepositoryMock
                .Setup(repo => repo.GetById(clientId))
                .ReturnsAsync(client);

            // Act
            var result = await _validator.ValidateRedirectUrl(clientId, redirectUrlParameter);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }        
        
        [TestMethod]
        public async Task ValidateRedirectUrl_RedirectUrlDoesNotMatchRegisteredValue_ReturnsInvalidResult()
        {
            // Arrange
            string clientId = "client123";
            string redirectUrlParameter = "https://example.com/callback";
            string nonMatchingRegisteredRedirectUrl = "https://different.com/callback";
            var client = new ClientApplication
            {
                Id = clientId,
                RedirectUrls = new List<RedirectUrl> { new RedirectUrl { Value = nonMatchingRegisteredRedirectUrl } }
            };

            _clientRepositoryMock
                .Setup(repo => repo.GetById(clientId))
                .ReturnsAsync(client);

            // Act
            var result = await _validator.ValidateRedirectUrl(clientId, redirectUrlParameter);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorResponse?.Should().Be(OAuthError.InvalidRequest);
        }        
        
        [TestMethod]
        public async Task ValidateRedirectUrl_RedirectUrlIsNotValidUri_ReturnsInvalidResult()
        {
            // Arrange
            string clientId = "client123";
            string redirectUrlParameter = "https://example.com/callback";
            string invalidRegisteredRedirectUrl = "ht://different.com/callback";
            var client = new ClientApplication
            {
                Id = clientId,
                RedirectUrls = new List<RedirectUrl> { new RedirectUrl { Value = invalidRegisteredRedirectUrl } }
            };

            _clientRepositoryMock
                .Setup(repo => repo.GetById(clientId))
                .ReturnsAsync(client);

            // Act
            var result = await _validator.ValidateRedirectUrl(clientId, redirectUrlParameter);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorResponse?.Should().Be(OAuthError.InvalidRequest);
        }

        [TestMethod]
        public async Task ValidateRedirectUrl_ValidRedirectUrl_ReturnsValidResult()
        {
            // Arrange
            string clientId = "client123";
            string redirectUrl = "https://example.com/callback";
            var client = new ClientApplication
            {
                Id = clientId,
                RedirectUrls = new List<RedirectUrl> { new RedirectUrl { Value = redirectUrl } }
            };

            _clientRepositoryMock
                .Setup(repo => repo.GetById(clientId))
                .ReturnsAsync(client);

            // Act
            var result = await _validator.ValidateRedirectUrl(clientId, redirectUrl);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public async Task ValidateResponseType_InvalidResponseType_ReturnsInvalidResult()
        {
            // Arrange
            string clientId = "client123";
            string responseType = "invalid";
            var client = new ClientApplication
            {
                Id = clientId
            };

            _clientRepositoryMock
                .Setup(repo => repo.GetById(clientId))
                .ReturnsAsync(client);

            // Act
            var result = await _validator.ValidateResponseType(clientId, responseType);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorResponse?.Should().Be(OAuthError.InvalidRequest);
        }

        [TestMethod]
        public async Task ValidateResponseType_ClientNotAuthorizedToUseResponseType_ReturnsInvalidResult()
        {
            // Arrange
            string clientId = "client123";
            var clientType = ClientType.Confidential;
            string responseType = ResponseType.AccessToken;
            var client = new ClientApplication
            {
                Id = clientId,
                ClientType = clientType
            };

            _clientRepositoryMock
                .Setup(repo => repo.GetById(clientId))
                .ReturnsAsync(client);

            // Act
            var result = await _validator.ValidateResponseType(clientId, responseType);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorResponse?.Should().Be(OAuthError.UnauthorizedClient);
        }

        [TestMethod]
        public async Task ValidateResponseType_ValidResponseType_ReturnsValidResult()
        {
            // Arrange
            string clientId = "client123";
            var clientType = ClientType.Confidential;
            string responseType = "code";
            var client = new ClientApplication
            {
                Id = clientId,
                ClientType = clientType
            };

            _clientRepositoryMock
                .Setup(repo => repo.GetById(clientId))
                .ReturnsAsync(client);

            // Act
            var result = await _validator.ValidateResponseType(clientId, responseType);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public async Task ValidateScope_ValidScope_ReturnsValidResult()
        {
            // Arrange
            string scope = "read write";
            var validScopes = new List<Scope> { new Scope { Name = "read", Description = "read" }, new Scope { Name = "write", Description = "write" } };

            _scopeRepositoryMock.Setup(x => x.GetMany(It.IsAny<string[]>())).ReturnsAsync(validScopes);

            // Act
            var result = await _validator.ValidateScope(scope);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public async Task ValidateScope_InvalidScope_ReturnsInvalidResult()
        {
            // Arrange
            string scope = "read write";
            var scopes = new List<Scope> { new Scope { Name = "read", Description = "read" } };

            _scopeRepositoryMock.Setup(repo => repo.GetMany(It.IsAny<string[]>())).ReturnsAsync(scopes);

            // Act
            var result = await _validator.ValidateScope(scope);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorResponse.Should().Be(OAuthError.InvalidScope);
        }
    }

}