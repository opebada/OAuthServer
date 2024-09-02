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

        public enum RequestValidationState
        {
            None,
            ClientIdIsValid,
            RedirectUriIsValid,
            ResponseTypeIsValid,
            ScopeIsValid
        }

        [TestMethod]
        public async Task ValidateRequest_NullRequestParameters_ThrowsArgumentNullException()
        {
            Func<Task> func = async () => await _authorizationService.ValidateRequest(null);

            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [DataTestMethod]
        [DataRow(RequestValidationState.None, Code.InvalidRequest, DisplayName = "InvalidClientId_ReturnsInvalidRequestError")]
        [DataRow(RequestValidationState.ClientIdIsValid, Code.InvalidRequest, DisplayName = "InvalidRedirectUrl_ReturnsInvalidRequestError")]
        [DataRow(RequestValidationState.RedirectUriIsValid, Code.UnsupportedResponseType, DisplayName = "InvalidResponseType_ReturnsUnsupportedResponseTypeError")]
        [DataRow(RequestValidationState.ResponseTypeIsValid, Code.InvalidScope, DisplayName = "InvalidScope_ReturnsInvalidScopeError")]
        [DataRow(RequestValidationState.ScopeIsValid, DisplayName = "AllParametersAreValid_ReturnsEmptyErrorResponse")]
        public async Task ValidateRequest_ShouldReturnCorrectResult(RequestValidationState validationState, string errorCode = "")
        {
            // Arrange
            var authorizationRequest = new AuthorizationRequest
            {
                ClientId = "clientId123",
                RedirectUri = "https://www.example.com/callback",
                ResponseType = ResponseType.Code,
                Scope = "openid",
                State = ""
            };

            SetupSuccessfulClientValidationResult(validationState);
            SetupSuccessfulRedirectUriValidationResult(validationState);
            SetupSuccessfulResponseTypeValidationResult(validationState);
            SetupSuccessfulScopeValidationResult(validationState);

            // Act
            AuthorizationResult result = await _authorizationService.ValidateRequest(authorizationRequest);

            // Assert
            if (result.IsValid)
                result.ErrorResponse.Should().BeNull();
            else
                result.ErrorResponse.Code.Should().Be(errorCode);
        }

        private void SetupSuccessfulClientValidationResult(RequestValidationState validationState)
        {
            Result<ClientApplication> clientResult =  validationState >= RequestValidationState.ClientIdIsValid  
            ? new Result<ClientApplication>(new ClientApplication())
            : new Result<ClientApplication>(new ErrorResponse(Code.InvalidRequest, Descriptions.InvalidRequest));

            _mockAuthorizationParameterValidator
                .Setup(x => x.ValidateClientId(It.IsAny<string>()))
                .ReturnsAsync(clientResult);
        }

        private void SetupSuccessfulRedirectUriValidationResult(RequestValidationState validationState)
        {
            var validRedirectUrlResult = validationState >= RequestValidationState.RedirectUriIsValid 
            ? new Result<bool>(true)
            : new Result<bool>(ErrorResponse.InvalidRequest);

            _mockAuthorizationParameterValidator
                .Setup(x => x.ValidateRedirectUrl(It.IsAny<string>(), It.IsAny<ClientApplication>()))
                .Returns(validRedirectUrlResult);
        }

        private void SetupSuccessfulResponseTypeValidationResult(RequestValidationState validationState)
        {
            var responseTypeResult = validationState >= RequestValidationState.ResponseTypeIsValid
            ? new Result<bool>(true)
            : new Result<bool>(ErrorResponse.UnsupportedResponseType);

            _mockAuthorizationParameterValidator
                .Setup(x => x.ValidateResponseType(It.IsAny<string>(), It.IsAny<ClientApplication>()))
                .Returns(responseTypeResult);
        }

        private void SetupSuccessfulScopeValidationResult(RequestValidationState validationState)
        {
            var scopeResult = validationState >= RequestValidationState.ScopeIsValid
            ? new Result<bool>(true)
            : new Result<bool>(ErrorResponse.InvalidScope);

            _mockAuthorizationParameterValidator
                .Setup(x => x.ValidateScope(It.IsAny<string>()))
                .ReturnsAsync(scopeResult);
        }
}
