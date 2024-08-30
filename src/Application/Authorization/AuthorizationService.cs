using System;
using System.Linq;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Core;
using Core.Authorization.ParameterValidation;
using Core.Client;
using Core.Authorization;

namespace Application.Authorization;

public class AuthorizationService : IAuthorizationService
{
    private readonly IAuthorizationParameterValidator _authorizationParameterValidator;

    public AuthorizationService(IAuthorizationParameterValidator authorizationParameterValidator)
    {
        _authorizationParameterValidator = authorizationParameterValidator;
    }

    public async Task<AuthorizationResult> ValidateRequest(NameValueCollection requestParameters)
    {
        ArgumentNullException.ThrowIfNull(requestParameters);
        
        // perhaps move this up to the controller layer, parameter here should be AuthorizationRequest
        AuthorizationRequest authorizationRequest = CreateAuthorizationRequest(requestParameters);

        Result<ClientApplication> clientValidationResult = await _authorizationParameterValidator.ValidateClientId(authorizationRequest.ClientId);
        
        if (clientValidationResult.IsFailure)
            return new AuthorizationResult(authorizationRequest, clientValidationResult.ErrorResponse);

        ClientApplication clientApplication = clientValidationResult.Value;

        Result<bool> redirectUrlValidationResult = _authorizationParameterValidator.ValidateRedirectUrl(authorizationRequest.RedirectUri, clientApplication);

        if (redirectUrlValidationResult.IsFailure)
            return new AuthorizationResult(authorizationRequest, redirectUrlValidationResult.ErrorResponse);

        Result<bool> responseTypeValidationResult = _authorizationParameterValidator.ValidateResponseType(authorizationRequest.ResponseType, clientApplication);

        if (responseTypeValidationResult.IsFailure)
            return new AuthorizationResult(authorizationRequest, responseTypeValidationResult.ErrorResponse);

        Result<bool> scopeValidationResult = await _authorizationParameterValidator.ValidateScope(authorizationRequest.Scope);

        if (scopeValidationResult.IsFailure)
            return new AuthorizationResult(authorizationRequest, scopeValidationResult.ErrorResponse);
            
        return new AuthorizationResult(authorizationRequest);
    }

    private AuthorizationRequest CreateAuthorizationRequest(NameValueCollection requestParameters)
    {
        var authorizationRequest = new AuthorizationRequest
        {
            ClientId = requestParameters["client_id"],
            RedirectUri = requestParameters["redirect_uri"],
            ResponseType = requestParameters["response_type"],
            Scope = requestParameters["scope"],
            State = requestParameters["state"],
            CodeChallenge = requestParameters["code_challenge"],
            CodeChallengeMethod = requestParameters["code_challenge_method"]
        };

        return authorizationRequest;
    }
}
