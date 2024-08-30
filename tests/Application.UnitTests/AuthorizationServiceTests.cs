using System;
using System.Collections.Specialized;
using Application.Authorization;
using Core;
using Core.Authorization;
using Core.Authorization.ParameterValidation;
using Core.Client;
using Core.Error;
using FluentAssertions;
using Moq;
using static Core.Error.OAuthErrors;

namespace Application.UnitTests;

[TestClass]
public class AuthorizationServiceTests
{
        private readonly Mock<IAuthorizationParameterValidator> _mockAuthorizationParameterValidator;
        private readonly AuthorizationService _authorizationService;

        public AuthorizationServiceTests()
        {
            _mockAuthorizationParameterValidator = new Mock<IAuthorizationParameterValidator>();
            _authorizationService = new AuthorizationService(_mockAuthorizationParameterValidator.Object);
        }

        [TestMethod]
        public async Task ValidateRequest_NullRequestParameters_ThrowsArgumentNullException()
        {
            Func<Task> func = async () => await _authorizationService.ValidateRequest(null);

            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        // TODO: use that state machine thing
        // Next steps: AI code review
        // Implement changes
        [DataTestMethod]
        [DataRow(false, false, false, false, Code.InvalidRequest, DisplayName = "InvalidClientId_ReturnsInvalidRequestError")]
        [DataRow(true, false, false, false, Code.InvalidRequest, DisplayName = "InvalidRedirectUrl_ReturnsInvalidRequestError")]
        [DataRow(true, true, false, false, Code.UnsupportedResponseType, DisplayName = "InvalidResponseType_ReturnsUnsupportedResponseTypeError")]
        [DataRow(true, true, true, false, Code.InvalidScope, DisplayName = "InvalidScope_ReturnsInvalidScopeError")]
        [DataRow(true, true, true, true, DisplayName = "AllParametersAreValid_ReturnsEmptyErrorResponse")]
        public async Task ValidateRequest_ShouldReturnCorrectResult(bool clientIsValid, bool redirectUriIsValid, 
        bool responseTypeIsValid, bool scopeIsValid, string errorCode = "")
        {
            // Arrange
            string clientId = "clientId123";
            string redirectUrl = "https://www.example.com/callback";
            string responseType = ResponseType.Code;
            string scope = "openid";
            var requestParameters = new NameValueCollection
            {
                { "client_id", clientId },
                { "redirect_uri", redirectUrl },
                { "response_type", responseType },
                { "scope", scope },
            };

            SetupSuccessfulClientValidationResult(clientIsValid);
            SetupSuccessfulRedirectUriValidationResult(redirectUriIsValid);

            SetupSuccessfulResponseTypeValidationResult(responseTypeIsValid);
            SetupSuccessfulScopeValidationResult(scopeIsValid);

            // Act
            AuthorizationResult result = await _authorizationService.ValidateRequest(requestParameters);

            // Assert
            if (result.IsValid)
                result.ErrorResponse.Should().BeNull();
            else
                result.ErrorResponse.Code.Should().Be(errorCode);
        }

        private void SetupSuccessfulClientValidationResult(bool clientIsValid)
        {
            Result<ClientApplication> clientResult =  clientIsValid 
            ? new Result<ClientApplication>(new ClientApplication())
            : new Result<ClientApplication>(new ErrorResponse(Code.InvalidRequest, Descriptions.InvalidRequest));

            _mockAuthorizationParameterValidator
                .Setup(x => x.ValidateClientId(It.IsAny<string>()))
                .ReturnsAsync(clientResult);
        }

        private void SetupSuccessfulRedirectUriValidationResult(bool resultIsValid)
        {
            var validRedirectUrlResult = resultIsValid 
            ? new Result<bool>(resultIsValid)
            : new Result<bool>(ErrorResponse.InvalidRequest);

            _mockAuthorizationParameterValidator
                .Setup(x => x.ValidateRedirectUrl(It.IsAny<string>(), It.IsAny<ClientApplication>()))
                .Returns(validRedirectUrlResult);
        }

        private void SetupSuccessfulResponseTypeValidationResult(bool resultIsValid)
        {
            var responseTypeResult = resultIsValid
            ? new Result<bool>(resultIsValid)
            : new Result<bool>(ErrorResponse.UnsupportedResponseType);

            _mockAuthorizationParameterValidator
                .Setup(x => x.ValidateResponseType(It.IsAny<string>(), It.IsAny<ClientApplication>()))
                .Returns(responseTypeResult);
        }

        private void SetupSuccessfulScopeValidationResult(bool resultIsValid)
        {
            var scopeResult = resultIsValid
            ? new Result<bool>(resultIsValid)
            : new Result<bool>(ErrorResponse.InvalidScope);

            _mockAuthorizationParameterValidator
                .Setup(x => x.ValidateScope(It.IsAny<string>()))
                .ReturnsAsync(scopeResult);
        }
}
